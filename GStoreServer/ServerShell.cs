using Grpc.Core;
using GStoreServer.Services;
using System;
using System.Collections.Generic;

namespace GStoreServer
{

    //interface para implementar os master servers
    public interface IMasterServer { 
        
    }
    //Já existe a classe GRPC.Core.Server, a ServerShell serve de 'shell' para 
    //o server implement partition id,server id*(se necessário), entre outras coisas
    class ServerShell
    {
        //private int port;
        public int partitionID { get; set; }
        public int serverID { get; set; }
        private string hostname = "localhost";
        //private ServerPort port;
        private Server server;
        

        public ServerShell(string hostname, int port, int partition) //string[] services
        {
            initializeServer( hostname,  port,  partition);
        }

        private void initializeServer(string hostname, int port, int partition)
        {
            partitionID = partition;
            ServerPort sp = new ServerPort(hostname, port, ServerCredentials.Insecure);
            server = new Server
            {
                Services = { AttachServerService.BindService(new AttachService()) },//Adicionar serviços
                Ports = { sp }
            };
            server.Start();
            AppContext.SetSwitch(
 "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true) ;
        }

        public static void Attach()
        {

        }


        static void Main(string[] args)
        {
            var ss = new ServerShell("localhost",1001,1);
            Console.WriteLine("Hello World!");
        }
    }
}
