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
        private List<int> serversID;
        private int masterID;
        private List<Objecto> objectos;
    }

    public class AppMap
    {
        private List<int> partitionsID;
        private List<int> serversID;

    }
}
