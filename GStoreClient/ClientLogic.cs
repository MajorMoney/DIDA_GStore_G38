using Common;
using Grpc.Core;
using Grpc.Net.Client;
using GStoreClient_server;
using PuppetMaster;
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

    /* public class ClientServicer : ClientService.ClientServiceBase
     {
         public ClientServicer()
         {

         }
     }
     public class NodeServicer : NodeService.NodeServiceBase
     {
         public NodeServicer()
         {

         }
     }*/

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

        //private readonly AttachServerService.AttachServerServiceClient client;
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
            //methodList = this.methodsToList();

            //
            Thread setter = new Thread(new ThreadStart(setup));
            setter.Start();
        }


        //setup gets the system topology
        public async void setup()
        {
            Thread.Sleep(500);//mudar eventualmente
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
                lock (this)
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
            /*Debug.WriteLine("Client:" + this.ID + "TEST1");
            foreach (var p in objectsMap) //print for testing serves
            {
                Debug.WriteLine("Client:" + this.ID + "TEST2");
                Debug.WriteLine("Client:" + this.ID + "-Partition ID:" + p.Key);
                foreach (var a in p.Value)
                {
                    Debug.WriteLine("Client:" + this.ID + " Object ID:" + a );
                }
            }*/
            
            Debug.WriteLine("Client:" + this.ID + " Got its topologyMap");
            Debug.WriteLine("1)");
            ReadLogic(1, 1, -1);//1)harcoded test
            Debug.WriteLine("2)");
            ReadLogic(1, 1, 2);//2)harcoded test
            Debug.WriteLine("3)");
            ReadLogic(1, 1, -1);//3)harcoded test
            Debug.WriteLine("4)");
            ReadLogic(1, 1, 3);//4)harcoded test
            Debug.WriteLine("5)");
            TryAttach("http://localhost:8171");//5)
            Debug.WriteLine("6)");
            ReadLogic(1, 1, 3);//6)harcoded test
            Debug.WriteLine("7)");
            ReadLogic(4, 1, 3);//7)harcoded test
            Debug.WriteLine("8)");
            ReadLogic(1, 5, -1);//8)harcoded test
            //OI


            listServer(3);
            listGlobal();
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
        public void PuppetShutdown()
        {
            //puppet_master_server.ShutdownAsync().Wait();
        }


        static void Main(string[] args)
        {
            ScriptReader x = new ScriptReader();
            List<Tuple<MethodInfo, object[]>> queue = x.readScript(@"Scripts\client_script1");
            /**Debug.WriteLine("---------------------------------------------------------------------------------------------------------------");
            foreach (Tuple<MethodInfo, object[]> kvp in queue)
            {
                Debug.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Item1, string.Join(";", kvp.Item2)));
            }**/
            //var ss = new ServerShell("localhost", 1001);
            //ClientLogic a = new ClientLogic();
            //a.readScript(@"Scripts\custom_test");
            //Debug.WriteLine("ola");

        }
        //problema de recursividade,infinite while loop, mudar eventualmente
        private string ReadLogic(int partition_id, int object_id, int server_id)
        {
            string res=null;

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
                 res=   Read(partition_id, object_id);
                }
                else if (server_id != -1)
                {
                    TryAttach(serverUrls[server_id]);
                  res=  ReadLogic(partition_id, object_id, server_id);
                }
                else
                {
                    //attached server doesnt have the refered object and no other server reference
                    Debug.WriteLine("it was impossible to send the read Request ");
                    Debug.WriteLine("attached server doesnt have the refered object and no other server reference");
                }
            }
            else if (server_id != -1)
            {
                //Client will try to attach to the given ID
                TryAttach(serverUrls[server_id]);
                res = ReadLogic(partition_id, object_id, server_id);

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
                ObjectID = object_id
            });
            Debug.WriteLine("Client read value:" + reply.Value + " from: " + attachedServerUrl + "in partition "+partition_id);
            return reply.Value;
        }

        private void write(int partition_id, int object_id, string value)
        {
            Debug.WriteLine("Read method : " + partition_id + " " + object_id + " " + value);
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
            List<Tuple<int,Tuple<int,string>>> list = new List<Tuple<int, Tuple<int, string>>>(); 
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
            Debug.WriteLine("Contents in server " + server_id +" :");
            
            int init = list[0].Item1;
            Debug.WriteLine("Partition " + init);

            foreach (Tuple<int, Tuple<int, string>> x in list) {

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
            foreach(int i in serverUrls.Keys)
            {
                Debug.WriteLine("Server " + i + " at " + serverUrls[i]);
                foreach(KeyValuePair < int, List<int>> part_server in topologyMap)
                {
                    if (part_server.Value.Contains(i))
                    {//OI
                        Debug.WriteLine("Partition " + part_server.Key + " :");
                        foreach(int obj in objectsMap[part_server.Key]) {
                            Debug.WriteLine("Object id: " + obj);
                        }
                    }
                }

            }
            Debug.WriteLine("listGlobal");
        }

        private void wait(int x)
        {
            Debug.WriteLine("wait : " + x);
        }





    }
}


