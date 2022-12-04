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
    public partial class ReplaceWithInputForm : Form
    {
        public double value = 0;
        public ReplaceWithInputForm()
        {
            InitializeComponent();
            textBox1.Text = value.ToString();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if( !double.TryParse(textBox1.Text,out value))
            {
                MessageBox.Show("please input a numeric value.");
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
