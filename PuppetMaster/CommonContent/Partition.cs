using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PuppetMaster.CommonContent
{
    public class Objecto
    {
        private UniqueKey key;
        private string value { get; set; }
    }
    public class UniqueKey
    {
        private Tuple<int,int> keys;
    }
    public class Partition
    {
        private int ID;
        private int[] serversID;
        private int masterID;
        private List<Objecto> objectos;
    }


}
