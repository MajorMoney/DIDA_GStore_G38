using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GStoreClient
{
    public partial class ClientGui : Form
    {
        private ClientLogic cl;
        public ClientGui()
        {
            this.cl = new ClientLogic("Teste","localhost","localhost",1001);//hardcoded
            InitializeComponent();
        }

        

        private void register_on_click(object sender, EventArgs e)
        {
            try
            {
                this.cl.Register(8080);
                registerBt.Enabled = false;
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Failed to bind port 'localhost:8080',port alredy binded ");
            }
        }
    }
}
