using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multicord
{
    public partial class Alert : Form
    {
        private int[] success = { 92, 184, 92 };
        private int[] failure = { 217, 83, 79 };
        public Alert(string title, string message, int type)
        {
            InitializeComponent();

            int[] argb = { };

            switch (type)
            {
                case 0:
                    argb = success;
                    break;
                case -1:
                    argb = failure;
                    break;
            }

            this.BackColor = Color.FromArgb(argb[0], argb[1], argb[2]);

            bunifuFlatButton1.Normalcolor = Color.FromArgb(argb[0] - 20, argb[1] - 20, argb[2] - 20);
            bunifuFlatButton1.OnHovercolor = Color.FromArgb(argb[0] - 30, argb[1] - 30, argb[2] - 30);

            bunifuCustomLabel1.Text = title;
            bunifuCustomLabel2.Text = message;

            this.Show();
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
