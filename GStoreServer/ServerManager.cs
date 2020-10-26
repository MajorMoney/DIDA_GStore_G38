using System;
using System.Collections.Generic;
using System.Text;

namespace GStoreServer
{

    class ServerManager
    {
        private ServerShell[][] servers;


        public  ServerManager()
        {

        }

        public static int GetPort() //hardcoded por agora
        {
            return 1001;
        }
    }
}
