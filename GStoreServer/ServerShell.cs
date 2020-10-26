using Grpc.Core;
using System;

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

        public ServerShell(int partition,string[] services,int port)
        {
            initializeServer(partition,services,port);
        }

        private void initializeServer(int partition, string[] services, int port)
        {
            ServerPort sp = new ServerPort(hostname, port, ServerCredentials.Insecure);
            server = new Server
            {
                Services = { ChatServerService.BindService(new ServerService()) },//por finalizar
                Ports = { sp }
            };
        }

        public static void Attach()
        {

        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
