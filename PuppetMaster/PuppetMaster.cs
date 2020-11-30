using Grpc.Core;
using Grpc.Net.Client;
using GStoreClient;
using GStoreServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.IO;
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


        //PM Collections
        private Dictionary<int, string> clients;
        private Dictionary<int, string> servers;
        private Dictionary<int,Partition> partitions;
        private Dictionary<string, MethodInfo> methodList;
        //PM other atributes
        private readonly int repFactor=3;
        private string hostname;
        //private int client_count=0;

            
        public PuppetMaster()
        {
            //Atribute initialization
            hostname = "http://localhost:10001";
            clients = new Dictionary<int, string>();
            servers = new Dictionary<int, string>();
            partitions = new Dictionary<int, Partition>();
            //
            Thread starter = new Thread(new ThreadStart(Start));
            starter.Start();
            
            
       
            

           
        }
        private void Start()
        {

            //methodList = this.methodsToList();
            //
            AppContext.SetSwitch(
"System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //Start setup service
            ServerPort sp = new ServerPort("localhost", 10001, ServerCredentials.Insecure);            
            Server server = new Server
            {
                Services = { PuppetMasterService.BindService(new SetUpService(this)) },
                Ports = { sp }
            };
            server.Start();           
            Debug.WriteLine("Setup service listening...");

            methodList = this.methodsToList();
            //Testing 'Main'
            this.Test();

        }

        //Returns the objects keys and values from a given  partition 
        public Dictionary<int, string> GetObjects(int pID)
        {
            Dictionary<int, string> objects = new Dictionary<int, string>();            
            foreach (var p in partitions[pID].objects)
            {
                objects.Add(p.Key,p.Value);
            }
            return objects;
        }
       
        //Returns a list with the IDs from all the existing partitions
        public List<int> GetPartitions()
        {
            var lista =new List<int>();
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

        ////////Creation Commands////////

        private async void ReplicationFactor(int r)// configures the system to replicate partitions on r servers
        {

        }


        /*This command creates a server process identified by server id, available at URL that delays any incoming message
for a random amount of time(specified in milliseconds) between min delay and max delay.If the value of both min delay and max delay is set to 0, the server
should not add any delay to incoming messages. Note that the delay should affect
all outgoing communications from the server.*/

        private async void Server(int ID, string url, int min_delay, int max_delay)
        {
            //BackgroundWorker bw = new System.ComponentModel.BackgroundWorker();
            ServerShell server = new ServerShell(ID,url,hostname,min_delay,max_delay);
            servers.Add(ID, url);
            Debug.WriteLine("Added server ID:"+ID+"--url--" + url);




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
            this.Partition(repFactor,2, new int[] { 1, 2, 3 });            
            this.Server(1, "http://localhost:8171", 1000, 3000);
            this.Server(2, "http://localhost:8172", 1000, 3000);
            this.Server(3, "http://localhost:8173", 1000, 3000);
            this.Client(1, "http://localhost:8181", "script");


        }








        /* SURICATA**/

        /* Example implementation:  this.readScript(@"Scripts\pm_script1");
         */
        private void readScript(String path)
        {
            //Current runtime directory -> PuppetMaster\bin\Debug\netcoreapp3.1 -> string path1 = Directory.GetCurrentDirectory();


            string pathToFile = @"..\..\..\" + path; //Go back to \PuppetMaster directory 

            Debug.WriteLine("Reading script in " + pathToFile);

            try
            {
                string[] lines = File.ReadAllLines(pathToFile);

                foreach (string line in lines)
                {
                    string[] args = line.Split(" ");
                    Debug.WriteLine(line);

                    scriptReaderHelper(args);


                }
            }
            catch (IOException e)
            {
                Debug.WriteLine("IO Exception while reading Script, check your path. Current read base path is /PuppetMaster ");
                Debug.WriteLine(e);
            }

        }

        private void scriptReaderHelper(string[] line)
        {
            string method = line[0];
            if (methodList.ContainsKey(method))
            {
                int i = 1;

                MethodInfo m = methodList[method];
                ParameterInfo[] pars = m.GetParameters();
                Debug.WriteLine("parameters length_" + pars.Length);
                object[] method_args = new object[pars.Length];

                foreach (ParameterInfo p in pars)
                {
                    if (i < line.Length)
                    {
                        if (p.ParameterType.IsArray)   //is starting server array
                        {

                            List<int> servers = new List<int>();

                            while (i < line.Length && line[i].StartsWith("s"))
                            {
                                servers.Add(Int32.Parse(line[i].Substring(1)));
                                i++;
                            }

                            method_args[p.Position] = servers.ToArray();
                            break;


                        }
                        else if (p.ParameterType == typeof(Int32))
                        {
                            int number;
                            if (Int32.TryParse(line[i], out number))
                            {
                                method_args[p.Position] = number;
                                i++;
                            }
                            else try
                                {
                                    number = Int32.Parse(line[i].Substring(1));
                                    method_args[p.Position] = number;
                                    i++;
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine("Cannot convert this value: " + line[i]);
                                }
                        }
                        else if (p.ParameterType == typeof(string))
                        {
                            Debug.WriteLine("String i: " + i);
                            method_args[p.Position] = line[i];
                            i++;
                        }
                    }
                }

                // m.Invoke(this,method_args);
                Debug.WriteLine("method : " + method + " args: " + method_args);

                foreach (object z in method_args)
                {
                    if (z != null)
                    {
                        Debug.WriteLine("PARAM:" + i + " : " + z.GetType().ToString());
                    }
                    else Debug.WriteLine("PARAM:" + i + " :  NULL");

                }

            }

            //string[] args = new string[line.Length-1];
            //Array.Copy(line, 1, args, 0, line.Length - 1);
            //string res = string.Join(";", args);

            //Debug.WriteLine("[{0}]", string.Join(", ", args));


        }


        private Dictionary<string, MethodInfo> methodsToList()
        {

            Type myType = (typeof(PuppetMaster));
            MethodInfo[] privateMethods = myType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            MethodInfo[] publicMethods = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Dictionary<string, MethodInfo> res = new Dictionary<string, MethodInfo>();

            foreach (MethodInfo m in privateMethods)
            {
                res.Add(m.Name, m);
            }
            foreach (MethodInfo m in publicMethods)
            {
                res.Add(m.Name, m);
            }

            /**Debug.WriteLine("---------------------------------------------------------------------------------------------------------------");
            foreach (KeyValuePair<string, MethodInfo> kvp in res)
            {
                Debug.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value.GetParameters().Length));
            }**/
            return res;
        }
    }
}
