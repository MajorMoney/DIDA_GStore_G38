using Grpc.Core;
using Grpc.Net.Client;
using PuppetMaster;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace GStoreClient
{

    public class ClientServicer : ClientService.ClientServiceBase
    {
        public ClientServicer()
        {

        }
    }
  /*  public class NodeServicer : NodeService.NodeServiceBase
    {
        public NodeServicer()
        {

        }
    }*/

    public class ClientLogic
    {
        //Client Collections
        private Dictionary<int, List<int>> topologyMap;
        private Dictionary<int, string> serverUrls;
        //Client other atributes
        private int ID;
        private string hostname;
        private string puppet_hostname;//PM could be PCS

        private readonly AttachServerService.AttachServerServiceClient client;
        private Server server;

       



        public ClientLogic(int id,string client_hostname, string puppet_hostname,string script)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Atribute initialization
            this.ID = id;
            hostname = client_hostname;
            this.puppet_hostname= puppet_hostname;
            topologyMap = new Dictionary<int, List<int>>();
            serverUrls = new Dictionary<int, string>();
            //
            Thread setter = new Thread(new ThreadStart(setup));
            setter.Start();
        }


        //setup gets the system topology
        public async void setup()
        {
            Thread.Sleep(500);//mudar eventualmente
            //channel setup
            using var channel = GrpcChannel.ForAddress(puppet_hostname);
            var puppetMasterService = new PuppetMasterService.PuppetMasterServiceClient(channel);
            Debug.WriteLine("Client:" + this.ID + " has Sent SetUP Request");
            //Send request
            using var call = puppetMasterService.SetUp(new SetUpRequest() { Ok = true });
            //get response
            while (await call.ResponseStream.MoveNext())
            {
                var objects = call.ResponseStream.Current;
                var list = new List<int>();
                list.AddRange(objects.ServerInfo.Keys);
                lock (this.topologyMap)
                {
                    topologyMap.TryAdd(objects.PartitionID, list);
                }
                lock (this.serverUrls)
                {
                    foreach (var o in objects.ServerInfo)
                    {
                    
                        serverUrls.TryAdd(o.Key, o.Value);
                    }
                }
            }
            Debug.WriteLine("Client:" + this.ID + " Got its topologyMap");
        }



        //atach, por mudar
        public void Register(int client_port)
        {

            // setup the client service
            server = new Server
            {
                Services = { ClientService.BindService(new ClientServicer()) },
                Ports = { new ServerPort(hostname, Convert.ToInt32(client_port), ServerCredentials.Insecure) }
            };
            server.Start();
            AttachReply reply = client.Attach(new AttachRequest
            {
                Nick = "teste",
                Url = "http://localhost:" + client_port //Hardcoded
            });
            Console.WriteLine(reply.Ok);
        }

        
        public void ClientShutdown()
        {
            server.ShutdownAsync().Wait();
        }
        public void PuppetShutdown()
        {
            //puppet_master_server.ShutdownAsync().Wait();
        }


        static void Main(string[] args)
        {
            /* var ss = new ServerShell("localhost", 1001);
             Console.WriteLine("Hello World!");*/
        }

    }
}


