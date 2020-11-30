using Grpc.Core;
using Grpc.Net.Client;
using PuppetMaster;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        private Dictionary<string, MethodInfo> methodList;
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
            methodList = this.methodsToList();

            //
            Thread setter = new Thread(new ThreadStart(setup));
            setter.Start();
        }
        public ClientLogic()
        {
            methodList = this.methodsToList();


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
            ClientLogic a = new ClientLogic();
             a.readScript(@"Scripts\client_script3");
            Debug.WriteLine("ola");

        }
        private void read(int partition_id, int object_id,int server_id)
        {
            Debug.WriteLine("Read method : " + partition_id + " " + object_id+ " " + server_id);
        }
        private void write(int partition_id, int object_id, string value)
        {
            Debug.WriteLine("Read method : "+partition_id+" " + object_id+" " + value);
        }
        private void listServer(int server_id)
        {
            Debug.WriteLine("listServer : " + server_id);
        }
        private void listGlobal()
        {
            Debug.WriteLine("listGlobal");
        }
        
        private void wait(int x)
        {
            Debug.WriteLine("wait : " + x);
        }
        






















        /****** SURICATA ******/


        public void readScript(string path)
        {
            //Current runtime directory -> PuppetMaster\bin\Debug\netcoreapp3.1 -> string path1 = Directory.GetCurrentDirectory();

            List<Tuple<MethodInfo, object[]>> queue = new List<Tuple<MethodInfo, object[]>>();
            string pathToFile = @"..\..\..\..\PuppetMaster\" + path; //Go back to \PuppetMaster directory 

            Debug.WriteLine("Reading script in " + pathToFile);

            try
            {
                string[] lines = File.ReadAllLines(pathToFile);
                //int num_line = 0;
                for (int num_line = 0; num_line < lines.Length; num_line++)
                {
                    string[] args = lines[num_line].Split(" ");
                    Debug.WriteLine(lines[num_line]);


                    if (args[0].Equals("begin-repeat"))
                    {

                        num_line++;
                        int num;
                        if (Int32.TryParse(args[1], out num))
                        {
                            int i = 0;
                            int z = 0;
                            int last_line = 0;
                            while (i < num)
                            {
                                Debug.WriteLine("Iteração: i=" + i + "  z=" + z + "  last_line=" + last_line + " num=" + num);
                                Debug.WriteLine("");
                                string curr_line = lines[num_line + z].Replace(@"$i", i.ToString());

                                Debug.WriteLine("Linha corrigida:" + curr_line);
                                string[] helper_args = curr_line.Split(" ");

                                if (helper_args[0].Equals("end-repeat"))
                                {
                                    i++;
                                    last_line = num_line + z;
                                    z = 0;
                                }
                                else
                                {
                                    try
                                    {
                                    queue.Add(scriptReaderHelper(helper_args));
                                    }
                                    catch (ArgumentNullException e)
                                    {
                                        Debug.WriteLine("Error writing to queue: "+e);
                                    }


                                    z++;
                                }
                            }
                            num_line = last_line;
                        }
                        else
                        {
                            Debug.WriteLine("Wrong syntax:" + lines[num_line]);
                        }

                    }
                    else
                    {
                        try
                        {
                            queue.Add(scriptReaderHelper(args));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error writing to queue: "+ e);
                        }
                    }

                }
            }
            catch (IOException e)
            {
                Debug.WriteLine("IO Exception while reading Script, check your path. Current read base path is /PuppetMaster ");
                Debug.WriteLine(e);
            }
            Debug.WriteLine("---------------------------------------------------------------------------------------------------------------");
            foreach (Tuple<MethodInfo, object[]> kvp in queue)
            {
                Debug.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Item1, string.Join(";",kvp.Item2)));
            }
            Debug.WriteLine("---------------------------------------------------------------------------------------------------------------");

            execute(queue);
        }

        private void execute(List<Tuple<MethodInfo, object[]>> tasks)
        {
            foreach(Tuple<MethodInfo,object[]> task in tasks)
            {
                MethodInfo method = task.Item1;
                try
                {
                    object x = method.Invoke(this, task.Item2);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        private  Tuple<MethodInfo,object[]> scriptReaderHelper(string[] line)
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
                        Debug.WriteLine("PARAM:" + z + " : " + z.GetType().ToString());
                    }
                    else Debug.WriteLine("PARAM:" + " :  NULL");

                }

                return new Tuple<MethodInfo, object[]>(m, method_args);
            }
            return null;

            
            //string[] args = new string[line.Length-1];
            //Array.Copy(line, 1, args, 0, line.Length - 1);
            //string res = string.Join(";", args);

            //Debug.WriteLine("[{0}]", string.Join(", ", args));


        }
        private Dictionary<string, MethodInfo> methodsToList()
        {

            Type myType = (typeof(ClientLogic));
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

            Debug.WriteLine("---------------------------------------------------------------------------------------------------------------");
            foreach (KeyValuePair<string, MethodInfo> kvp in res)
            {
                Debug.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value.GetParameters().Length));
            }
            return res;
        }

    }
}


