using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace GStoreClient
{

    public class ClientServicer : ClientService.ClientServiceBase
    {
         public  ClientServicer()
    {
        
    }
    }
    class ClientLogic
    {

        
      

        private readonly GrpcChannel channel;
        private readonly AttachServerService.AttachServerServiceClient client;

        private Server server;
        private string nick;
        private string hostname;


        public ClientLogic(string nick, string client_hostname, string server_hostname, int server_port)
        {
            this.hostname = client_hostname;
            this.nick = nick;
            // setup the client side

            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress("http://" + server_hostname + ":" + server_port.ToString());

            client = new AttachServerService.AttachServerServiceClient(channel);

        }

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
                Nick = nick,
                Url = "http://localhost:" + client_port
            });
            Console.WriteLine(reply.Ok);
        }

        public void ClientShutdown()
        {
            server.ShutdownAsync().Wait();
        }




    }
}


