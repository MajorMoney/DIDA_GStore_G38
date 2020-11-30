using Grpc.Core;
using Grpc.Net.Client;
using GStoreServer.Services;
using PuppetMaster;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
        private Dictionary<int, Dictionary<int,string>> objects;
        private Dictionary<int,string> serverUrls;
        //Server other atributes
        public int ID { get; }
        private string hostname;
        private string puppet_hostname;//PM could be PCS

        //private ServerPort port;
        private Server server;        

       



        public ServerShell(int id,string server_url, string puppet_hostname,int min, int max)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Atribute initialization
            ID = id;
            hostname = server_url;
            topologyMap = new Dictionary<int, List<int>>();
            objects = new Dictionary<int, Dictionary<int, string>>();
            this.puppet_hostname = puppet_hostname;
            serverUrls = new Dictionary<int, string>();
            //

            Thread setter = new Thread(new ThreadStart(setup));
            setter.Start();
           
            
            // "http://" + puppet_hostname + ":" + puppet_port.ToString();
            // initializeServer(server_hostname, server_port);---->precisa de ser alterado
        }

      
        //setup gets the system topology
        public async void setup()
        {
            Thread.Sleep(1000);//Mudar eventualmente
            //channel setup
            using var channel = GrpcChannel.ForAddress(puppet_hostname);
            var puppetMasterService = new PuppetMasterService.PuppetMasterServiceClient(channel);
            //Send request
            using var call = puppetMasterService.SetUp(new SetUpRequest() { Ok = true });
            //get response
            Debug.WriteLine("Sever:" + this.ID + " has Sent SetUP Request");
            while (await call.ResponseStream.MoveNext())
            {
                var objects = call.ResponseStream.Current;
                var list = new List<int>();
                list.AddRange(objects.ServerInfo.Keys);
                //Populate topologymap
                lock (this.topologyMap) 
                { 
                topologyMap.TryAdd(objects.PartitionID,list);
                }
                //Populate servers urls
                foreach (var o in objects.ServerInfo)
                {
                    lock (this.serverUrls)
                    {
                        serverUrls.TryAdd(o.Key,o.Value);
                    }
                }
            }
            Debug.WriteLine("Sever:"+this.ID+" Got its topologyMap");
            this.GetObjects(puppetMasterService);
        }
        //GetObjects will get the keys and values from the partitions replicated in this server
        private void GetObjects(PuppetMasterService.PuppetMasterServiceClient puppetMasterService)
        {
            Debug.WriteLine("Sever:" + this.ID + " has Sent GetObjects Request");
            foreach (var p in topologyMap)
            {
                if (p.Value.Contains(this.ID)) {
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
            foreach (var o in objects)
            {
                Debug.WriteLine("Server:"+this.ID+"-Partition ID:" + o.Key);
                foreach (var a in o.Value)
                {
                    Debug.WriteLine("Server:" + this.ID + " Object Key:" + a.Key + " Object Value:" + a.Value);
                }
            }
        }
     

        private void initializeServer(string hostname, int port)
        {

            ServerPort sp = new ServerPort(hostname, port, ServerCredentials.Insecure);
            server = new Server
            {
                Services = { AttachServerService.BindService(new AttachService()) },//Adicionar serviços
                Ports = { sp }
            };
            server.Start();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true) ;
        }

        public static void Attach()
        {

        }


        public void PuppetShutdown()
        {
           // puppet_master_server.ShutdownAsync().Wait();
        }

        static void Main(string[] args)
        {
           /* var ss = new ServerShell("localhost", 1001);
            Console.WriteLine("Hello World!");*/
    }
}
}
