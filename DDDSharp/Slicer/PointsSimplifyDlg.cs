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
    public partial class PointsSimplifyDlg : Form
    {
        public bool remove1 = true;
        public bool remove2 = true;
        public double scale = 1.0;
        public PointsSimplifyDlg()
        {
            InitializeComponent();
        }

        private void PointsSimplifyDlg_Load(object sender, EventArgs e)
        {
            textBox1.Text = scale.ToString();
            checkBox1.Checked = remove1;
            checkBox2.Checked = remove2;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            remove1 = checkBox1.Checked;
            remove2 = checkBox2.Checked;
            if( !double.TryParse(textBox1.Text,out scale) )
            {
                MessageBox.Show("distance filter not correct.");
                return;
            }            
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
