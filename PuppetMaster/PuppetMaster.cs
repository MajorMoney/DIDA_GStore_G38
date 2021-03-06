﻿using Common;
using Grpc.Core;
using Grpc.Net.Client;
using GStoreClient;
using GStoreServer;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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


        //PM Collections
        private Dictionary<int, string> clients;
        private Dictionary<int, string> servers;
        private ConcurrentDictionary<int,Partition> partitions;
        private Dictionary<string, MethodInfo> methodList;
        //PM other atributes
        private readonly int repFactor=3;
        private string hostname;
        //private int client_count=0;
        private PuppetMasterGUI gui;
        private ScriptReader scriptReader;
        private Log logger;

        public PuppetMaster(PuppetMasterGUI  gi)
        {

            //Atribute initialization
            gui = gi;
            scriptReader = new ScriptReader();
            logger = new Log(string.Format("_log_{0}.txt",DateTime.Now.ToString("d-M-yyyy_H-mm")));
            
            hostname = "http://localhost:10001";
            clients = new Dictionary<int, string>();
            servers = new Dictionary<int, string>();
            partitions = new ConcurrentDictionary<int, Partition>();
            //
            Thread starter = new Thread(new ThreadStart(Start));
            starter.Start();
        }
        private void Start()
        {

            AppContext.SetSwitch(
"System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Start setup service
            ServerPort sp = new ServerPort("localhost", 10001, ServerCredentials.Insecure);            
            Server server = new Server
            {
                Services = { PuppetMasterService.BindService(new PuppetMasterToNodeService(this)) },
                Ports = { sp }
            };
            server.Start();           
            Debug.WriteLine("Setup service listening...");

            //Testing 'Main'
            this.Test();

        }
        public void WriteLine(string s)
        {
            gui.WriteLine(s);

        }
        public void WriteLog(string s)
        {
            logger.WriteLine(s);
        }

        //Returns the objects keys and values from a given  partition 
        public Dictionary<int, string> GetObjects(int pID)
        {
            Dictionary<int, string> reply = new Dictionary<int, string>();            
            foreach (var p in partitions[pID].objects)
            {

                reply.Add(p.Key,p.Value);

            }
            return reply;
        }

        internal List<int> GetObjectsIDs(int i)
        {
            List<int> lista = new List<int>();
            lista.AddRange(partitions[i].objects.Keys);
            return lista;
        }

        //Returns a list with the IDs from all the existing partitions
        public List<int> GetPartitions()
        {
            List<int> lista =new List<int>();
            lista.AddRange(partitions.Keys);
            return lista;
        }

        //Returns the servers IDs and URLs where a given a partition is replicated
        public Dictionary<int,string> GetServersUrls(int pID)//node_type can be client or server
        {
            Dictionary<int, string> temp= new Dictionary<int, string>();
            foreach (var serverID in partitions[pID].Servers)
            {

                temp.Add(serverID,this.servers[serverID]);
            }
            return temp;
        }

        public void SendServerSetup()
        {
            Parallel.ForEach<string>(servers.Values,  (url) =>
            {
                using var channel = GrpcChannel.ForAddress(url);
                var serverNodeService = new NodeServerService.NodeServerServiceClient(channel);
                var ack = serverNodeService.Acknoledge(new CheckUp  { Check = true });
                channel.Dispose();
            });           
        }

        public void SendClientSetup()
        {
            Parallel.ForEach<string>(servers.Values, (url) =>
            {
                using var channel = GrpcChannel.ForAddress(url);
                var clientNodeService = new NodeClientService.NodeClientServiceClient(channel);
                var ack = clientNodeService.Acknoledge(new CCheckUp { Check = true });
                channel.Dispose();
            });
        }


        ////////Creation Commands////////

        public async void ReplicationFactor(int r)// configures the system to replicate partitions on r servers
        {

        }


        /*This command creates a server process identified by server id, available at URL that delays any incoming message
for a random amount of time(specified in milliseconds) between min delay and max delay.If the value of both min delay and max delay is set to 0, the server
should not add any delay to incoming messages. Note that the delay should affect
all outgoing communications from the server.*/

        public async void Server(int ID, string url, int min_delay, int max_delay)
        {
            //BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            ServerShell server = new ServerShell(ID,url,hostname,min_delay,max_delay);
            servers.Add(ID, url);
            Debug.WriteLine("Added server ID:"+ID+"--url--" + url);
            
            


        }

        /* configures the system
to store r replicas of partition partition name on the servers identified with the
server ids server id 1 to serverd id r.*/

        public async void Partition(int r,int partition_ID, params int[] servers_ids)
        {
            //Debug.WriteLine("Partition Error: " + r + " ; " + partition_ID + " " + servers_ids);
            

            Partition partition = new Partition(r, partition_ID, servers_ids);
            bool x =partitions.TryAdd(partition_ID, partition);
            //partitions.Add(partition_ID, partition);
            //gui.WriteLine("Partition:" + x);       
        }

        /* This command creates a client process
identified by the string username, available at client URL and that will execute the
commands in the script file script file. It can be assumed that the script file is
located in the same disk folder as the client executable.
*/

        public async void Client(int ID, string url, string script_file)
        {
            

            //BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            ClientLogic cl = new ClientLogic(ID,url,hostname,script_file);
            clients.Add(ID, url);
            

        }

        /*This command makes all nodes in the system print their current status.
The status command should present brief information about the state of the system (who is present, which nodes are presumed failed, etc...). Status information
can be printed on each nodes’ console and does not need to be centralised at the
PuppetMaster.*/

        public async void Status()
        {

        }



        ////////Debug Commands////////

        public async void Crash(int server_id)// This command is used to force a process to crash.
        {
            var url = servers[server_id];
            using var channel = GrpcChannel.ForAddress(url);
            var serverNodeService = new NodeServerService.NodeServerServiceClient(channel);
            var reply = serverNodeService.Crash(new CrashRequest { Check = true });
            channel.Dispose();
            Debug.WriteLine(server_id + " Crashed");
        }

        /*This command is used to simulate a delay in the process. After
receiving a freeze, the process continues receiving messages but stops processing
them until the PuppetMaster “unfreezes” it.
*/

        public async void Freeze(int server_id)
        {
            var  url = servers[server_id];
            using var channel = GrpcChannel.ForAddress(url);
            var serverNodeService = new NodeServerService.NodeServerServiceClient(channel);
            var reply = serverNodeService.Freeze(new FreezeRequest { Check = true});
            channel.Dispose();
            Debug.WriteLine(server_id+" Frozen" );
        }

        /* This command is used to put a process back to normal operation. Pending messages that were received while the process was frozen, should be
processed when this command is received.*/

        public async void Unfreeze(int server_id)
        {
            var url = servers[server_id];
            using var channel = GrpcChannel.ForAddress(url);
            var serverNodeService = new NodeServerService.NodeServerServiceClient(channel);
            var reply = serverNodeService.Unfreeze(new UnfreezeRequest { Check = true });
            channel.Dispose();
            Debug.WriteLine(server_id + " Unrozen");
        }

        /*This command instructs the PuppetMaster to sleep for x milliseconds
before reading and executing the next command in the script file.*/
        public void Wait(int x_ms)
        {
            gui.WriteLine("Waiting for" + x_ms);
            Thread.Sleep(x_ms);
        }


        public void Test()
        {       }

        public void readScript(string path)
        {
            bool areServerSetup = false;
            List<Tuple<MethodInfo, object[]>> queue = scriptReader.readScript(path);

            List<List<Tuple<MethodInfo, object[]>>> z = new List<List<Tuple<MethodInfo, object[]>>>();
            List<Task> tasks = new List<Task>();
            List<Task> server_tasks = new List<Task>();
            int num_servers=0;
            for (int i = 0; i < queue.Count; i++)
                if (queue[i].Item1.Name.ToLower().Equals("server"))
                {
                    num_servers++;

                }
            for (int i=0; i< queue.Count; i++)
            {
                gui.WriteLine(">>Executing:" + queue[i].Item1.Name + " args: " + string.Join(" ; ", queue[i].Item2));

                if (queue[i].Item1.Name.ToLower().Equals("server"))
                {
                    gui.WriteLine("Server started " + num_servers);
                    Task t = new Task(() => queue[i].Item1.Invoke(this, queue[i].Item2));
                    server_tasks.Add(t);
                    tasks.Add(t);
                    t.Start();
                    num_servers--;
                    
                }
                else
                {
                    waitForServers(server_tasks,areServerSetup);
                    if(num_servers==0 && !areServerSetup)
                    {
                        gui.WriteLine("Server setup " + num_servers);

                        SendServerSetup();
                        areServerSetup = true;
                    }
                if (queue[i].Item1.Name.ToLower().Equals("wait"))
                {
                    gui.WriteLine("----I am wait--------");

                    foreach (Task t in tasks)
                    {
                        t.Wait();
                        gui.WriteLine("Task has finished: " + t.ToString() + " : " + t.Status);
                    }
                    queue[i].Item1.Invoke(this, queue[i].Item2);
                }
                else
                {
                   
                        if (queue[i].Item2 == null || queue[i].Item2.Length == 0)
                        {
                            gui.WriteLine("Status is going");
                            //Task t = new Task(() => queue[i].Item1.Invoke(this, null));
                            Task t = new Task(() => Status());
                            t.Start();
                            tasks.Add(t);
                        }
                        else
                        {

                            Task t = new Task(() => queue[i].Item1.Invoke(this, queue[i].Item2));


                            t.Start();
                            tasks.Add(t);

                        }
                    }

                   
                  

                }
            }
            
        }
        private void waitForServers(List<Task> server_tasks, bool areServerSetup)
        {   if (areServerSetup)
                return;
            foreach(Task t in server_tasks)
            {
                t.Wait();
            }
            gui.WriteLine("All servers finished- resuming execution ");
            
            Debug.WriteLine("All servers finished: resuming script execution");

        }


    }
}
