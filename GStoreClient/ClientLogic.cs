using Grpc.Core;
using Grpc.Net.Client;
using PuppetMaster;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

        private int ID;
        private string hostname;
        private Dictionary<int, List<int>> topologyMap;
        private Dictionary<int, string> serverUrls;

        private readonly AttachServerService.AttachServerServiceClient client;
        private Server server;

        private string puppet_hostname;



        public ClientLogic(int id,string client_hostname, string puppet_hostname,string script)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            this.ID = id;
            hostname = client_hostname;
            this.puppet_hostname= puppet_hostname;
            topologyMap = new Dictionary<int, List<int>>();

            this.setup();
        }


        //setup gets the system topology
        public async void setup()
        {
            //channel setup
            using var channel = GrpcChannel.ForAddress(puppet_hostname);  
            var puppetMasterService = new PuppetMasterService.PuppetMasterServiceClient(channel);
            //Send request
            using var call = puppetMasterService.SetUp(new SetUpRequest() { Ok=true});
            //get response
            while (await call.ResponseStream.MoveNext())
            {
                var map = call.ResponseStream.Current;
                int[] servers = new int[map.ServerID.Count];
                map.ServerID.CopyTo(servers, 0);
                var list = new List<int>();
                list.AddRange(servers);
                lock (this.topologyMap)
                {
                    topologyMap.Add(map.PartitionID, list);
                }
            }         
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


