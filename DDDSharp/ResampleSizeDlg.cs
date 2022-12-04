using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDDSharp
{
    public partial class ResampleSizeDlg : Form
    {
        public int new_sizex = 0;
        public int new_sizey = 0;
        public int new_sizez = 0;
        public int sizex = 0;
        public int sizey = 0;
        public int sizez = 0;
        public ResampleSizeDlg()
        {
            InitializeComponent();
        }

        private void ResampleSizeDlg_Load(object sender, EventArgs e)
        {
            new_sizex = sizex;
            new_sizey = sizey;
            new_sizez = sizez;
            originx.Text = "original x size:" + sizex;
            originy.Text = "original y size:" + sizey;
            originz.Text = "original z size:" + sizez;
            newXSize.Text = new_sizex.ToString();
            newYSize.Text = new_sizey.ToString();
            newZSize.Text = new_sizez.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            new_sizex = int.Parse(newXSize.Text);
            new_sizey = int.Parse(newYSize.Text);
            new_sizez = int.Parse(newZSize.Text);
            if( new_sizex <= 0 || new_sizey <= 0 || new_sizez <= 0)
            {
                MessageBox.Show("invalid size inputed.");
                return;
            }
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
