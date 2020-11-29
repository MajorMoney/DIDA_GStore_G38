using GStoreClient;
using GStoreServer;//temporário para teste
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterGUI : Form
    {
        public PuppetMasterGUI()
        {
            InitializeComponent();
        }

        private void InitBtn_Click(object sender, EventArgs e)
        {
            PuppetMaster pm = new PuppetMaster();
            
           
        }
    }
}
