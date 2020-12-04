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
        private Dictionary<int, Dictionary<int, string>> lockers;
        private Dictionary<int, string> serverUrls;
        private ConcurrentQueue<Task> writeRequests;
        private ConcurrentQueue<Task> readRequests;
        //Server other atributes
        public int ID { get; }
        private string hostname;
        private string puppet_hostname;//PM could be PCS
        private bool isFrozen;
        //for synchro
        private AutoResetEvent gatekeeper;
        //private ManualResetEventSlim frozen;
        private string writeKey = "dummy";

        //private ServerPort port;
        private Server server;        




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
            lockers = new Dictionary<int, Dictionary<int, string>>();
            serverUrls = new Dictionary<int, string>();
            this.puppet_hostname = puppet_hostname;
            gatekeeper = new AutoResetEvent(false);
            //frozen = new ManualResetEventSlim(false);
            writeRequests = new ConcurrentQueue<Task>();
            readRequests = new ConcurrentQueue<Task>();
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
                    Dictionary<int, string> dic2 = new Dictionary<int, string>();
                    lock (this.objects)
                    {
                        foreach (var o in reply.Objectos)
                        {
                            dic.Add(o.Key, o.Value);
                            dic2.Add(o.Key, " ");
                        }
                        objects.Add(reply.PartitionID, dic);
                        lockers.Add(reply.PartitionID, dic2);
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
            this.server = server;
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
        public ReadReply GetObjectValue(int pID, int objID, string clientUrl)
        {
            ReadReply reply = new ReadReply()
            {
                HasValue = false,
                Value = "TTTTTT"
            };
            if (isFrozen)
            {
                ReadEnqueue(pID, objID, clientUrl);
            }
            else
            {
                if (Monitor.TryEnter(lockers[pID][objID]))
                {

                    try
                    {
                        reply.HasValue = true;
                        reply.Value = objects[pID][objID];

                    }
                    finally
                    {
                        Monitor.Exit(lockers[pID][objID]);

                    }
                }
                else
                {
                    try
                    {
                        SendRead(pID, objID, clientUrl);
                    }
                    catch(Exception e)
                    {

                    }
                }
            }
            return reply;
        }

        private void SendRead(int pID, int objID, string clientUrl)
        {
            Task t = new Task(() =>
            {
                using var channel = GrpcChannel.ForAddress(clientUrl);
                var service = new ClientService.ClientServiceClient(channel);
                lock (lockers[pID][objID])
                {
                    var reply = service.ReadValue(new ValueNotification()
                    {
                        Value = objects[pID][objID]
                    });
                }
                channel.Dispose();
            });
        }

        private void ReadEnqueue(int partitionID, int objectID, string clientUrl)
        {
            readRequests.Enqueue(new Task(() =>
            {
                SendRead(partitionID, objectID, clientUrl);//Mudar o metodo
            }));
        }

        private void ReadDequeue()
        {
            Parallel.ForEach<Task>(readRequests, (task) =>
            {
                if (readRequests.Count != 0)
                {
                    readRequests.TryDequeue(out task);
                    task.Start();
                }
            });
          
        }




        //#############-----End Read Requests handler Block Functions/Methods-----#############//
        //#############-----Begin Write Requests handler Block Functions/Methods-----#############//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionID"></param>
        /// <param name="objectID"></param>
        /// <param name="value"></param>
        /// 
        public void Write(int partitionID, int objectID, string value)
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
                        //SendWrite(partitionID,objectID/*,clientUrl*/);//é preciso alterar o write também
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


        private void SendWrite(int pID, int objID, string clientUrl)

        {
            using var channel = GrpcChannel.ForAddress(clientUrl);
            var service = new ClientService.ClientServiceClient(channel);
            lock (lockers[pID][objID])
            {
                var reply = service.WriteValue(new ValueNotification()
                {
                    Value = objects[pID][objID]
                });
            }
            channel.Dispose();
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
            if (writeRequests.Count != 0)
            {
                Task task;
                writeRequests.TryDequeue(out task);
                task.Start();
            }
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


               try
               {
                   Monitor.Enter(lockers[partitionID][objectID]);
                   gatekeeper.WaitOne();
                   objects[partitionID][objectID] = value;

               }
               finally
               {

                   Monitor.Exit(lockers[partitionID][objectID]);
               }
               /*catch (Exception e)
               {

               } */
           });
            //locker.IsBackground = true;
            locker.Name = "locker";
            locker.Start();
            Parallel.ForEach<string>(serverUrls.Values, (url) =>
          {
              if (!url.Equals(serverUrls[this.ID]))
              {
                  using var channel = GrpcChannel.ForAddress(url);
                  var serverMasterService = new PartitionMasterService.PartitionMasterServiceClient(channel);
                  var ack = serverMasterService.Lock(new LockRequest { PartitionID = partitionID, ObjectID = objectID, Newvalue = value }
                  , deadline: DateTime.UtcNow.AddSeconds(10)
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
                    try
                    {

                        var ack = serverMasterService.Unlock(new UnlockRequest { Ok = true }
                        , deadline: DateTime.UtcNow.AddSeconds(10)
                        );
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        channel.Dispose();
                    }

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
        public bool Lock(int pID, int objID, string newValue)
        {
            Thread locker1 = new Thread(() =>
           {
               var a = true;
               while (a)
               {
                   try
                   {
                       Monitor.Enter(lockers[pID][objID]);
                       a = false;
                       gatekeeper.WaitOne();
                       objects[pID][objID] = newValue;
                       Monitor.Exit(lockers[pID][objID]);
                   }
                   catch (Exception e)
                   {

                   }
               }
           });
            //locker.IsBackground = true;
            locker1.Start();
            return true;
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
            ReadDequeue();
            WriteDequeue();
        }

        //#############-----End PuppetMaster Requests Handlers Functions/Methods-----#############//




        public void PuppetShutdown()
        {
            // puppet_master_server.ShutdownAsync().Wait();
        }

        public void Crash()
        {
            server.KillAsync();
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
