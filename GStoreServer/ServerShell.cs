using Grpc.Core;
using Grpc.Net.Client;
using GStoreServer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GStoreServer
{

    //interface para implementar os master servers


    /* public class NodeServicer : NodeService.NodeServiceBase//Por criar
     {
         public NodeServicer()
         {
         }
     }*/


    //Já existe a classe GRPC.Core.Server, a ServerShell serve de 'shell' para 
    //o server implement partition id,server id*(se necessário), entre outras coisas
    public class ServerShell
    {
        //private int port;

        //Server Collections
        private Dictionary<int, List<int>> topologyMap;
        private Dictionary<int, Dictionary<int, string>> objects;
        private Dictionary<int, string> serverUrls;
        private ConcurrentQueue<Task> writeRequests;
        //private ConcurrentQueue<Task> readRequests;
        //Server other atributes
        public int ID { get; }
        private string hostname;
        private string puppet_hostname;//PM could be PCS
        private bool isFrozen;
        //for synchro
        private AutoResetEvent gatekeeper;
        private ManualResetEventSlim frozen;
        private string writeKey = "dummy";

        //private ServerPort port;
        //private Server server;        




        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="server_url"></param>
        /// <param name="puppet_hostname"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public ServerShell(int id, string server_url, string puppet_hostname, int min, int max)
        {
            AppContext.SetSwitch(
              "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Atribute initialization
            ID = id;
            hostname = server_url;
            topologyMap = new Dictionary<int, List<int>>();
            objects = new Dictionary<int, Dictionary<int, string>>();
            serverUrls = new Dictionary<int, string>();
            this.puppet_hostname = puppet_hostname;
            gatekeeper = new AutoResetEvent(false);
            frozen = new ManualResetEventSlim(false);
            writeRequests = new ConcurrentQueue<Task>();
            //readRequests = new ConcurrentQueue<Task>();
            isFrozen = false;

            //StartServerService(); 
            //
            //Thread.CurrentThread.Name = "MAIN";
            Thread starter = new Thread(new ThreadStart(StartServerService));
            starter.Name = "Starter";
            starter.IsBackground = true;
            Thread setter = new Thread(new ThreadStart(Setup));
            setter.Name = "Setter";
            setter.IsBackground = true;
            Thread tester = new Thread(new ThreadStart(Test));
            tester.Name = "Tester";
            tester.IsBackground = true;
            starter.Start();
            //setter.Start();
            tester.Start();
            //Debug.WriteLine("Tester Started");




        }


        //#############-----Begin SetUp/Start Block Functions/Methods-----#############//


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Setuper()
        {
            Setup();
            Debug.WriteLine("SetUper Done");
            return true;
        }

        //setup gets the system topology
        /// <summary>
        /// 
        /// </summary>
        private async void Setup()
        {
            //Debug.WriteLine(Thread.CurrentThread.Name +" is going to sleep");
            //Debug.WriteLine(Thread.CurrentThread.Name + " just woke up");
            //channel setup
            using var channel = GrpcChannel.ForAddress(puppet_hostname);
            var puppetMasterService = new PuppetMasterService.PuppetMasterServiceClient(channel);
            //Send request
            using var call = puppetMasterService.SetUp(new SetUpRequest() { Ok = true });
            //get response
            //Debug.WriteLine("Sever:" + this.ID + " has Sent SetUP Request");
            while (await call.ResponseStream.MoveNext())
            {
                var objects = call.ResponseStream.Current;
                var list = new List<int>();
                list.AddRange(objects.ServerInfo.Keys);
                //Populate topologymap
                lock (this.topologyMap)
                {
                    topologyMap.TryAdd(objects.PartitionID, list);
                }
                //Populate servers urls
                foreach (var o in objects.ServerInfo)
                {
                    lock (this.serverUrls)
                    {
                        serverUrls.TryAdd(o.Key, o.Value);
                    }
                }
            }
            Debug.WriteLine("SetUp Done");
            GetObjects(puppetMasterService);
        }



        //GetObjects will get the keys and values from the partitions replicated in this server
        /// <summary>
        /// 
        /// </summary>
        /// <param name="puppetMasterService"></param>
        private void GetObjects(PuppetMasterService.PuppetMasterServiceClient puppetMasterService)
        {
            //Debug.WriteLine("Sever:" + this.ID + " has Sent GetObjects Request");
            foreach (var p in topologyMap)
            {
                if (p.Value.Contains(this.ID))
                {
                    var call = puppetMasterService.GetObjectsAsync(new PopulateRequest() { PartitionID = p.Key });
                    var reply = call.ResponseAsync.Result;
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    lock (this.objects)
                    {
                        foreach (var o in reply.Objectos)
                        {
                            dic.Add(o.Key, o.Value);
                        }
                        objects.Add(reply.PartitionID, dic);
                    }
                }
            }
            Debug.WriteLine("GetObjects Done");
        }

        /// <summary>
        /// 
        /// </summary>
        private void StartServerService()
        {
            //Debug.WriteLine(Thread.CurrentThread.Name + " started");
            string[] url = hostname.Split("//");
            string[] urlv2 = url[1].Split(':');
            ServerPort sp = new ServerPort(urlv2[0], Int32.Parse(urlv2[1]), ServerCredentials.Insecure);
            Server server = new Server
            {
                Services =
                { AttachServerService.BindService(new ServerToClientService(this)),
                 PartitionMasterService.BindService(new ServerToServerService(this)),
                 NodeServerService.BindService(new SNodeService(this))
                },
                Ports = { sp }
            };
            server.Start();
            Debug.WriteLine("Server" + this.ID + "-->serving on adress:");
            Debug.WriteLine("host-   " + sp.Host + "  Port-  " + sp.Port);
            //while (true) ;
        }


        //#############-----End SetUp/Start Block Functions/Methods-----#############//
        //#############-----Begin Read Requests handler Block Functions/Methods-----#############//


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="objID"></param>
        /// <returns></returns>
        public string GetObjectValue(int pID, int objID)
        {
            if (isFrozen)
            {
                frozen.Wait();
               
            }
            string reply;
            lock (objects[pID][objID])
            {
                reply = objects[pID][objID];
            }
            return reply;

        }

        /*private void ReadEnqueue(int partitionID, int objectID)
        {
            readRequests.Enqueue(new Task(() =>
            {
                GetObjectValue(partitionID, objectID);
            }));
        }*/



        //#############-----End Read Requests handler Block Functions/Methods-----#############//
        //#############-----Begin Write Requests handler Block Functions/Methods-----#############//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionID"></param>
        /// <param name="objectID"></param>
        /// <param name="value"></param>
        /// 
        internal void Write(int partitionID, int objectID, string value)
        {
            if (!isFrozen)
            {

                if (Monitor.TryEnter(writeKey))
                {
                    try
                    {
                        SendLock(partitionID, objectID, value);
                        SendUnlock(partitionID, objectID, value);
                    }
                    finally
                    {
                        Monitor.Exit(writeKey);
                        WriteDequeue();
                       
                    }
                }
                else
                {
                    WriteEnqueue(partitionID, objectID, value);
                }
            }
            else
            {
                WriteEnqueue(partitionID, objectID, value);
            }

        }

        private void WriteEnqueue(int partitionID, int objectID, string value)
        {
            writeRequests.Enqueue(new Task(() =>
            {
                Write(partitionID, objectID, value);
            }));
        }

        private void WriteDequeue()
        {
            Task task;
            writeRequests.TryDequeue(out task);
            if (writeRequests.Count != 0)
                task.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionID"></param>
        /// <param name="objectID"></param>
        /// <param name="value"></param>
        public void SendLock(int partitionID, int objectID, string value)
        {
            Thread locker = new Thread(() =>
           {
               Monitor.Enter(objects[partitionID]);
               gatekeeper.WaitOne();
               objects[partitionID][objectID] = (string)value;
               Monitor.Exit(objects[partitionID]);
           });
            locker.Start();
            Parallel.ForEach<string>(serverUrls.Values, (url) =>
          {
              if (!url.Equals(serverUrls[this.ID]))
              {
                  using var channel = GrpcChannel.ForAddress(url);
                  var serverMasterService = new PartitionMasterService.PartitionMasterServiceClient(channel);
                  var ack = serverMasterService.Lock(new LockRequest { PartitionID = partitionID, ObjectID = objectID, Newvalue = value }
                  //,                  deadline: DateTime.UtcNow.AddSeconds(10)
                  );
                  channel.Dispose();
              }
          });

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionID"></param>
        /// <param name="objectID"></param>
        /// <param name="newValue"></param>
        public void SendUnlock(int partitionID, int objectID, string newValue)
        {
            Parallel.ForEach<string>(serverUrls.Values, (url) =>
            {
                if (!url.Equals(serverUrls[this.ID]))
                {
                    using var channel = GrpcChannel.ForAddress(url);
                    var serverMasterService = new PartitionMasterService.PartitionMasterServiceClient(channel);
                    var ack = serverMasterService.Unlock(new UnlockRequest { Ok = true }               
                    //,                    deadline: DateTime.UtcNow.AddSeconds(10)
                    ) ;
                    channel.Dispose();
                }
            });
            Unlock();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pID"></param>
        /// <param name="objID"></param>
        /// <param name="newValue"></param>
        public void Lock(int pID, int objID, string newValue)
        {
            Thread locker = new Thread(() =>
           {
               Monitor.Enter(objects[pID]);
               gatekeeper.WaitOne();
               objects[pID][objID] = newValue;
               Monitor.Exit(objects[pID]);
           });
            locker.Start();
        }



        /// <summary>
        /// 
        /// </summary>
        public void Unlock()
        {
            gatekeeper.Set();
        }


        //#############-----End Write Block Functions/Methods-----#############//

        //#############-----Begin PuppetMaster Requests Handlers Block Functions/Methods-----#############//

        public void Freeze() 
        {
            isFrozen = true;
        }

        public void Unfreeze()
        {
            isFrozen = false;
            frozen.Set();
            WriteDequeue();
        }

        //#############-----End PuppetMaster Requests Handlers Functions/Methods-----#############//




        public void PuppetShutdown()
        {
            // puppet_master_server.ShutdownAsync().Wait();
        }

        static void Main(string[] args)
        {
            /* var ss = new ServerShell("localhost", 1001);
             Console.WriteLine("Hello World!");*/
        }

        private void Test()
        {

            if (this.ID == 1)
            {
                /*Debug.WriteLine(Thread.CurrentThread.Name + " is going to sleep");
                Thread.Sleep(4000);
                Debug.WriteLine(Thread.CurrentThread.Name + "woke up");
                SendLock(1, 1);
                Thread.Sleep(2000);
                SendUnlock(1, 1, "TESTEA"); */
            }
        }
    }
}
