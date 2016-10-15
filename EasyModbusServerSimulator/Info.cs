using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace EasyModbusServerSimulator
{
    public partial class Info : Form
    {
        public Info()
        {
            InitializeComponent();
            Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lblVersion.Text = "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
