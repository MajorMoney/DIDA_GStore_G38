using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;


namespace Common
{
    public class ScriptReader
    {
        private Dictionary<string, MethodInfo> methodList;
        private Type caller_type;
        private string pathToDir;
        private string filename;
        public ScriptReader()
        {
            MethodBase method = new StackFrame(1, false).GetMethod();
          //  Debug.WriteLine(method.ToString());
            caller_type = method.DeclaringType;
            methodList = methodsToList();
            parseDirectory();

        }

        private void parseDirectory()
        {
            string _type = caller_type.ToString().ToLower();
            if (_type.Contains("client"))
            {
                pathToDir = @"..\..\..\..\GStoreClient\Scripts\";
            }
            else if (_type.Contains("puppetmaster"))
            {
                pathToDir = @"..\..\..\..\PuppetMaster\Scripts\";
            }
            if (_type.Contains("server"))
            {
                pathToDir = @"..\..\..\..\GStoreServer\Scripts\";
            }
            //Debug.WriteLine(pathToDir);

        }
        public List<Tuple<MethodInfo, object[]>> readScript(String path )
        {
            List<Tuple<MethodInfo, object[]>> queue = new List<Tuple<MethodInfo, object[]>>();

            //Current runtime directory -> PuppetMaster\bin\Debug\netcoreapp3.1 -> string path1 = Directory.GetCurrentDirectory();


            string pathToScript = pathToDir + @path; //Go back to \PuppetMaster directory 

            Debug.WriteLine("Reading script in " + @pathToScript);

            try
            {
                string[] lines = File.ReadAllLines(@pathToScript);

                for (int num_line = 0; num_line < lines.Length; num_line++)
                {
                    string[] args = lines[num_line].Split(" ");
             /**       Debug.WriteLine(lines[num_line]);**/


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
                                /**Debug.WriteLine("Iteração: i=" + i + "  z=" + z + "  last_line=" + last_line + " num=" + num);
                                Debug.WriteLine("");**/
                                string curr_line = lines[num_line + z].Replace(@"$i", i.ToString());

                                //Debug.WriteLine("Linha corrigida:" + curr_line);
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
                                        Debug.WriteLine("Error writing to queue: " + e);
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
                            Debug.WriteLine("Error writing to queue: " + e);
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
                Debug.WriteLine("Method: "+ kvp.Item1.Name+ "-->" + string.Join(" ; ",kvp.Item2));
            }
            Debug.WriteLine("---------------------------------------------------------------------------------------------------------------");
           
            return queue;
        }
        private Tuple<MethodInfo, object[]> scriptReaderHelper(string[] line)
        {
            string method = line[0].ToLower();

            if (methodList.ContainsKey(method))
            {
                int i = 1;

                MethodInfo m = methodList[method];
                ParameterInfo[] pars = m.GetParameters();
               // Debug.WriteLine("parameters length_" + pars.Length);
                object[] method_args = new object[pars.Length];

                foreach (ParameterInfo p in pars)
                {
                    if (i < line.Length)
                    {
                        if (p.ParameterType.IsArray)   
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
                            method_args[p.Position] = line[i];
                            i++;
                        }
                    }


                }

               /** // m.Invoke(this,method_args);
                Debug.WriteLine("method : " + method + " args: " + method_args);

                foreach (object z in method_args)
                {
                    if (z != null)
                    {
                        Debug.WriteLine("PARAM:" + z + " : " + z.GetType().ToString());
                    }
                    else Debug.WriteLine("PARAM:" + " :  NULL");

                }
               **/
                return new Tuple<MethodInfo, object[]>(m, method_args);
            }
            return null;
        }

        private Dictionary<string, MethodInfo> methodsToList()
        {

            Type myType = caller_type;
            MethodInfo[] privateMethods = myType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            MethodInfo[] publicMethods = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Dictionary<string, MethodInfo> res = new Dictionary<string, MethodInfo>();

            foreach (MethodInfo m in privateMethods)
            {
                res.Add(m.Name.ToLower(), m);
            }
            foreach (MethodInfo m in publicMethods)
            {
                res.Add(m.Name.ToLower(), m);
            }

           /** Debug.WriteLine("---------------------------------------------------------------------------------------------------------------");
            foreach (KeyValuePair<string, MethodInfo> kvp in res)
            {
                Debug.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value.GetParameters().Length));
            }**/
            return res;
        }


    }
}

