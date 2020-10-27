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
        private readonly ServerService.ServerServiceClient client;

        private Server server;
        private string nick;
        private string hostname;


        private void Register(string nick, int port)
        {
            this.nick = nick;
            // setup the client service
            server = new Server
            {
                Services = { ClientService.BindService(new ClientServicer()) },
                Ports = { new ServerPort(hostname, Convert.ToInt32(port), ServerCredentials.Insecure) }
            };
            server.Start();
            AttachReply reply = client.Attach(new AttachRequest
           {
                Nick = nick,
                Url = "http://localhost:" + port
            });

        }




    }
}


