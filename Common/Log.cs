using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Common
{
    public class Log
    {

        private BlockingCollection<string> logMessages;

        private string pathToFile;
        public Log(string filename)
        {
            logMessages = new BlockingCollection<string>();
            parseFile(@filename);
            Thread writerThread = new Thread(new ThreadStart(write));
           writerThread.Start();
        }

        //Use to stop logger
        public void close()
        {
            logMessages.CompleteAdding();
            
        }
        private void write() {
           
             
            while (true)
            {
                if (logMessages.IsCompleted)
                {
                    Debug.WriteLine("Log closed");
                    
                    break;
                }
                    if (logMessages.Count > 0)
                {

                    string line;
                        if (logMessages.TryTake(out line))
                        {

                        using (StreamWriter sw = new StreamWriter(@pathToFile,append: true))
                        {
                            
                            sw.WriteLine(line);
                            sw.Close();

                        }
                    }
                   

                }
                else
                {
                    Thread.Sleep(100);
                }



            }
            return;
        }

        //use to write line to previously defined log file
        public void WriteLine(string line)
        {
            logMessages.Add(line);
        }

        private void parseFile(string filename)
        {
            MethodBase method = new StackFrame(2, false).GetMethod();
            Type declaringType = method.DeclaringType;
            string _type = declaringType.ToString().ToLower();
            if (_type.Contains("client"))
            {
                pathToFile = @"..\..\..\..\GStoreClient\Logs\" + @filename;
                Debug.WriteLine(pathToFile);
            }
            else if (_type.Contains("puppetmaster"))
            {
                pathToFile = @"..\..\..\..\PuppetMaster\Logs\" + @filename;
                Debug.WriteLine(pathToFile);
            }
            if (_type.Contains("server"))
            {
                pathToFile = @"..\..\..\..\GStoreServer\Logs\" + @filename;
                Debug.WriteLine(pathToFile);
            }
        }







    }



}
