using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Multicord
{
    public partial class Add : Form
    {
        private Form1 mainform;
        public Add(Form1 form)
        {
            InitializeComponent();

            mainform = form;
        }

        private void Add_Load(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Discord dc = new Discord();
                Config config = new Config();

                while(!dc.isActive() || config.Exists())
                {
                    Thread.Sleep(5000);
                }

                this.Invoke((MethodInvoker)delegate
                {
                    pictureBox1.Hide();
                    panel2.BackColor = Color.FromArgb(92, 184, 92);
                    bunifuCustomLabel3.Text = "Successfully authorised your account.";
                });

                config.Add();

                mainform.UpdateList();
            }).Start();
        }
    }
}
