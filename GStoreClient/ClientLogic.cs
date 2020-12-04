using Common;
using Grpc.Core;
using Grpc.Net.Client;
using GStoreClient.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GStoreClient
{

   

    public class ClientLogic
    {
        //Client Collections
        private Dictionary<int, List<int>> topologyMap;
        private Dictionary<int, List<int>> objectsMap;
        private Dictionary<int, string> serverUrls;
        private Dictionary<string, MethodInfo> methodList;
        //Client other atributes
        private int ID;
        private string hostname;
        private string puppet_hostname;//PM could be PCS
        private ScriptReader scriptReader;
        private Log logger;
        private string script;
        private string attachedServerUrl;
        private GrpcChannel attachedServerChannel;





        public ClientLogic(int id, string client_hostname, string puppet_hostname, string script)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Atribute initialization
            this.ID = id;
            hostname = client_hostname;
            this.puppet_hostname = puppet_hostname;
            topologyMap = new Dictionary<int, List<int>>();
            objectsMap = new Dictionary<int, List<int>>();
            serverUrls = new Dictionary<int, string>();
            attachedServerUrl = null;
            this.script = script;
            scriptReader = new ScriptReader();
            logger = new Log(string.Format("_log_{0}.txt", DateTime.Now.ToString("d-M-yyyy_H-mm")));
            Thread setter = new Thread(new ThreadStart(setup));
            Thread starter = new Thread(new ThreadStart(StartClientNodeService));
            setter.IsBackground = true;
            starter.IsBackground = true;
            setter.Start();
            starter.Start();
        }

        private void StartClientNodeService()
        {
            string[] url = hostname.Split("//");
            string[] urlv2 = url[1].Split(':');
            ServerPort sp = new ServerPort(urlv2[0], Int32.Parse(urlv2[1]), ServerCredentials.Insecure);
            Server server = new Server
            {
                Services = { NodeClientService.BindService(new CNodeService(this)) },
                Ports = { sp }
            };
            Debug.WriteLine("host-   " + sp.Host + "  Port-  " + sp.Port);
            server.Start();
            Debug.WriteLine("Client" + this.ID + "-->serving on adress:");
        }


        //setup gets the system topology
        public async void setup()
        {
           
            //channel setup
            var channel = GrpcChannel.ForAddress(puppet_hostname);
            var puppetMasterService = new PuppetMasterService.PuppetMasterServiceClient(channel);
            Debug.WriteLine("Client:" + this.ID + " has Sent SetUP Request");
            //Send request
            using var call = puppetMasterService.SetUp(new SetUpRequest() { Ok = true });
            //get response
            while (await call.ResponseStream.MoveNext())
            {
                var objects = call.ResponseStream.Current;
                Debug.WriteLine(objects);

                var serversID = new List<int>();
                var objectsID = new List<int>();
                objectsID.AddRange(objects.ObjectsID);
                serversID.AddRange(objects.ServerInfo.Keys);
                lock (this.topologyMap)
                {
                    topologyMap.TryAdd(objects.PartitionID, serversID);
                }
                lock (this.objectsMap)
                {
                    objectsMap.TryAdd(objects.PartitionID, objectsID);
                }
                lock (this.serverUrls)
                {
                    foreach (var o in objects.ServerInfo)
                    {
                        serverUrls.TryAdd(o.Key, o.Value);
                    }
                }
            }


            readScript(script); 

        }



        public void TryAttachMaster(int partitionID)
        {
            var temp = new int[topologyMap[partitionID].Count];
            topologyMap[partitionID].CopyTo(temp);
            Debug.WriteLine(string.Join(";",topologyMap[partitionID]));
            TryAttach(serverUrls[temp[0]]);
        }



        public void TryAttach(string url)
        {
            if (String.IsNullOrEmpty(attachedServerUrl))
            {
                AttachApply(url);
            }
            else
            {
                if (!url.Contains(attachedServerUrl))
                {
                    Task.Run(() => AttachedServerShutdown(attachedServerChannel));
                    AttachApply(url);
                }
                else
                {
                    Debug.WriteLine("alredy conected to " + url);
                }
            }
        }

        private void AttachApply(string url)
        {
            var channel = GrpcChannel.ForAddress(url);
            var attachService = new AttachServerService.AttachServerServiceClient(channel);
            var reply = attachService.Attach(new AttachRequest()
            {
                Ok = true
            });
            attachedServerUrl = channel.Target;
            attachedServerChannel = channel;
            Debug.WriteLine("Client:" + this.ID + "now conected to " + url);
        }


        public void AttachedServerShutdown(GrpcChannel channel)
        {
            channel.ShutdownAsync().Wait();
            Debug.WriteLine("Attach conection with:" + channel.Target + "  was shutdowned sucefully");
        }
      

        static void Main(string[] args)
        {

           
        }
        private string ReadLogic(int partition_id, int object_id, int server_id)
        {
            string res = null;

            if (!topologyMap.ContainsKey(partition_id) || !objectsMap[partition_id].Contains(object_id))
            {
                //Partition does not exist  || Item does not exist in the specified Partition
                Debug.WriteLine("N/A");
            }
            else if (!String.IsNullOrEmpty(attachedServerUrl))
            {
                //Client alredy attached to a server
                //Get Key by value, since the values(urls) are unique
                int attachedServerID = serverUrls.FirstOrDefault(x => x.Value.Equals("http://" + attachedServerUrl)).Key;
                //attached server has the object
                if (topologyMap[partition_id].Contains(attachedServerID))
                {
                    res = Read(partition_id, object_id);
                }
                else if (server_id != -1)
                {
                    TryAttach(serverUrls[server_id]);
                    res = ReadLogic(partition_id, object_id, server_id);
                }
                else
                {
                    //attached server doesnt have the refered object and no other server reference
                    Debug.WriteLine("it was impossible to send the read Request");
                    Debug.WriteLine("attached server doesnt have the refered object and no other server reference");
                }
            }
            else if (server_id != -1)
            {
                //Client will try to attach to the given ID
                TryAttach(serverUrls[server_id]);
                res = ReadLogic(partition_id, object_id, server_id);
                Debug.WriteLine("1");
            }
            else
            {

                //Client not atached to a server and has no server ID as an attach reference
                Debug.WriteLine("it was impossible to send the read Request ");
                Debug.WriteLine("Client not atached to a server and has no server ID as an attach reference ");
            }
            return res;
        }

        private string Read(int partition_id, int object_id)
        {
            var attachService = new AttachServerService.AttachServerServiceClient(attachedServerChannel);
            var reply = attachService.Read(new ReadRequest()
            {
                PartitionID = partition_id,
                ObjectID = object_id,
                ClientUrl = this.hostname

            });
            if (reply.HasValue)
            {
                Debug.WriteLine(reply.Value);
                return reply.Value;
            }
            Debug.WriteLine("Will wait for the value");
            return "Will wait for the value";
        }

        public string ReceiveValue(string value)
        {
            Debug.WriteLine(value);
            return value;
        }

        public string ReceiveWrite(string value)
        {
            Debug.WriteLine(value);
            return value;
        }

        private void Write(int partition_id, int object_id, string value)
        {
            if (topologyMap.ContainsKey(partition_id) && objectsMap[partition_id].Contains(object_id))
            {
                TryAttachMaster(partition_id);
                var attachService = new AttachServerService.AttachServerServiceClient(attachedServerChannel);
                var reply = attachService.Write(new WriteRequest()
                {
                    PartitionID = partition_id,
                    ObjectID = object_id,
                    Value = value
                });
                Debug.WriteLine("-3");

            }
            else
            {
                //Partition does not exist  || Item does not exist in the specified Partition
                Debug.WriteLine("N/A");
                Debug.WriteLine("Failed  to write,Partition does not exist  || Item does not exist in the specified Partition");
            }
        }
        private void listServer(int server_id)
        {
            List<int> partitions = new List<int>();
            Dictionary<int, int[]> toRead = new Dictionary<int, int[]>();

            foreach (KeyValuePair<int, List<int>> kvp in topologyMap)
            {
                if (kvp.Value.Contains(server_id))
                {
                    partitions.Add(kvp.Key);
                    Debug.WriteLine("Partition to check: " + kvp.Key);
                }
            }
            List<Tuple<int, Tuple<int, string>>> list = new List<Tuple<int, Tuple<int, string>>>();
            foreach (int i in partitions)
            {
                foreach (int z in objectsMap[i])
                {
                    string res = ReadLogic(i, z, server_id);
                    Tuple<int, string> object_response = new Tuple<int, string>(z, res); //object_id + object content
                    Tuple<int, Tuple<int, string>> partition_object = new Tuple<int, Tuple<int, string>>(i, object_response); //partition + object
                    list.Add(partition_object);
                }
            }
            Debug.WriteLine("Contents in server " + server_id + " :");

            int init = list[0].Item1;
            Debug.WriteLine("Partition " + init);

            foreach (Tuple<int, Tuple<int, string>> x in list)
            {

                if (x.Item1 == init)
                {
                    Debug.WriteLine("Object id: " + x.Item2.Item1 + " --> Contents: " + x.Item2.Item2.ToString());

                }
                else
                {
                    init = x.Item1;
                    Debug.WriteLine("Partition " + init);

                    Debug.WriteLine("Object id: " + x.Item2.Item1 + " --> Contents: " + x.Item2.Item2);

                }

            }


        }
        private void listGlobal()
        {
            foreach (int i in serverUrls.Keys)
            {
                Debug.WriteLine("Server " + i + " at " + serverUrls[i]);
                foreach (KeyValuePair<int, List<int>> part_server in topologyMap)
                {
                    if (part_server.Value.Contains(i))
                    {
                        Debug.WriteLine("Partition " + part_server.Key + " :");
                        foreach (int obj in objectsMap[part_server.Key])
                        {
                            Debug.WriteLine("Object id: " + obj);
                        }
                    }
                }

            }
            Debug.WriteLine("listGlobal");
        }

        private void wait(int x)
        {


            Thread.Sleep(x);
            Debug.WriteLine("wait : " + x);

            Thread.Sleep(x);
            Debug.WriteLine("Stopped sleep at: " + DateTime.Now.ToString("G"));

        }

        public void readScript(string path)
        {
            List<Tuple<MethodInfo, object[]>> queue = scriptReader.readScript(path);

            for (int i = 0; i < queue.Count; i++)
            {
                if (queue[i].Item2 == null || queue[i].Item2.Length == 0)
                {
                    queue[i].Item1.Invoke(this, null);
                }
                else
                {
                    queue[i].Item1.Invoke(this, queue[i].Item2);
                }
            }
        }

    } }






