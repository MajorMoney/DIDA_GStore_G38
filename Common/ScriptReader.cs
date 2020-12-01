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
            Debug.WriteLine(method.ToString());
            caller_type = method.DeclaringType;
            methodList = methodsToList();
            parseDirectory();

        }

        private void parseDirectory()
        {
            string _type = caller_type.ToString().ToLower();
            if (_type.Contains("client"))
            {
                pathToDir = @"..\..\..\..\GStoreClient\";
                Debug.WriteLine(pathToDir);
            }
            else if (_type.Contains("puppetmaster"))
            {
                pathToDir = @"..\..\..\..\PuppetMaster\";
                Debug.WriteLine(pathToDir);
            }
            if (_type.Contains("server"))
            {
                pathToDir = @"..\..\..\..\GStoreServer\";
                Debug.WriteLine(pathToDir);
            }
        }
        public void readScript(String path )
        {
            //Current runtime directory -> PuppetMaster\bin\Debug\netcoreapp3.1 -> string path1 = Directory.GetCurrentDirectory();


            string pathToScript = pathToDir + path; //Go back to \PuppetMaster directory 

            Debug.WriteLine("Reading script in " + pathToScript);

            try
            {
                string[] lines = File.ReadAllLines(pathToScript);

                foreach (string line in lines)
                {
                    string[] args = line.Split(" ");
                    Debug.WriteLine(line);

                    scriptReaderHelper(args);


                }
            }
            catch (IOException e)
            {
                Debug.WriteLine("IO Exception while reading Script, check your reading path: "+pathToScript);
                Debug.WriteLine(e);
            }

        }

        public void scriptReaderHelper(string[] line)
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

            Type myType = caller_type;
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

