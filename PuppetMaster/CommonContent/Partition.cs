using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PuppetMaster

{
    public class Partition
    {
        public int Factor { get; set; }
        public int ID { get; set; }
        //public int MasterID { get; set; }
        public int[] Servers { get; set; }
        public Dictionary<int, string> objects { get; set; } = new Dictionary<int, string>();

        public Partition(int factor,int id, params int[] servers_ids)
        {
            Factor = factor;
            ID = id;
            Servers = servers_ids;          
            Populate();
        }

        private void Populate()
        {
            char[] abc = { 'a', 'b', 'c' };
            for (int i =1; i<Factor+1;i++)
            {
                objects.Add(i,"String "+abc[i-1]);
            }
        }

    }


}
