using Grpc.Core;
using Grpc.Net.Client;
using GStoreClient;
using GStoreServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace PuppetMaster
{



    //#####Port 10001 reserved for the PuppetMaster and can be used to expose a service that collects// 
    //information from the system’s nodes when the Status command is done########//

    public class PuppetMaster
    {
        //Implementar setup service como cliente nos nodes
        /* private Dictionary<string, NodeService.NodeServiceClient> client_map =
               new Dictionary<string, NodeService.NodeServiceClient>();
         private Dictionary<string, NodeService.NodeServiceClient> server_map =
                new Dictionary<string, NodeService.NodeServiceClient>();*/
        private Dictionary<int, string> clients=new Dictionary<int, string>();
        private Dictionary<int, string> servers = new Dictionary<int, string>();
        private Dictionary<int,Partition> partitions = new Dictionary<int,Partition>();
        private readonly int repFactor=3;
        private string hostname;
        //private int client_count=0;

            
        public PuppetMaster()
        {
            hostname = "http://localhost:10001";
            Thread starter = new Thread(new ThreadStart(Start));
            starter.Start();
            
            
       
            

           
        }
        private void Start()
        {
            //Start setup service
            AppContext.SetSwitch(
"System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            ServerPort sp = new ServerPort("localhost", 10001, ServerCredentials.Insecure);            
            Server server = new Server
            {
                Services = { PuppetMasterService.BindService(new SetUpService(this)) },
                Ports = { sp }
            };
            server.Start();           
            Debug.WriteLine("Setup service listening...");
            this.Test();

        }

        public Dictionary<int, string> GetObjects(int pID)
        {
            Dictionary<int, string> objects = new Dictionary<int, string>();            
            foreach (var p in partitions[pID].objects)
            {
                objects.Add(p.Key,p.Value);
            }
            return objects;
        }
       
        public List<int> GetPartitions()
        {
            var lista =new List<int>();
            lista.AddRange(partitions.Keys);
            return lista;
        }
        public Dictionary<int,string> GetServersUrls(int pID)//node_type can be client or server
        {
            Dictionary<int, string> servers= new Dictionary<int, string>();
            foreach (var serverID in partitions[pID].Servers)
            {               
                servers.Add(serverID,this.servers[serverID]);
            }
            return servers;
        }

        ////////Creation Commands////////

        private async void ReplicationFactor(int r)// configures the system to replicate partitions on r servers
        {

        }


        /*This command creates a server process identified by server id, available at URL that delays any incoming message
for a random amount of time(specified in milliseconds) between min delay and max delay.If the value of both min delay and max delay is set to 0, the server
should not add any delay to incoming messages. Note that the delay should affect
all outgoing communications from the server.*/

        private async void Server(int ID, string url, int min_delay, int max_delay,string script)
        {
            //BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            ServerShell server = new ServerShell(ID,url,hostname,min_delay,max_delay,script);
            servers.Add(ID,url);




        }

        /* configures the system
to store r replicas of partition partition name on the servers identified with the
server ids server id 1 to serverd id r.*/

        private async void Partition(int r,int partition_ID, params int[] servers_ids)
        {
            Partition partition = new Partition(r, partition_ID, servers_ids);
            partitions.Add(partition_ID, partition);
        }

        /* This command creates a client process
identified by the string username, available at client URL and that will execute the
commands in the script file script file. It can be assumed that the script file is
located in the same disk folder as the client executable.
*/

        private async void Client(int ID, string url, string script_file)
        {
            //BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            ClientLogic cl = new ClientLogic(ID,url,hostname,script_file);
            clients.Add(ID, url);
            

        }

        /*This command makes all nodes in the system print their current status.
The status command should present brief information about the state of the system (who is present, which nodes are presumed failed, etc...). Status information
can be printed on each nodes’ console and does not need to be centralised at the
PuppetMaster.*/

        private async void Status()
        {

        }



        ////////Debug Commands////////

        private async void Crash(int server_id)// This command is used to force a process to crash.
        {

        }

        /*This command is used to simulate a delay in the process. After
receiving a freeze, the process continues receiving messages but stops processing
them until the PuppetMaster “unfreezes” it.
*/

        private async void Freeze(int server_id)
        {

        }

        /* This command is used to put a process back to normal operation. Pending messages that were received while the process was frozen, should be
processed when this command is received.*/

        private async void Unfreeze(int server_id)
        {

        }

        /*This command instructs the PuppetMaster to sleep for x milliseconds
before reading and executing the next command in the script file.*/
        private void Wait(int x_ms)
        {

        }


        private void Test()
        {
            
            this.Partition(repFactor,1, new int[] { 1, 2, 3 });
            this.Partition(repFactor,2, new int[] { 5, 5, 5 });
            Wait(3000);
            this.Server(1, "http://localhost:8171", 1000, 3000, "script");
            this.Server(2, "http://localhost:8172", 1000, 3000, "script");
            this.Server(3, "http://localhost:8173", 1000, 3000, "script");
            this.Client(1, "http://localhost:8181", "script");


        }
    }
}
