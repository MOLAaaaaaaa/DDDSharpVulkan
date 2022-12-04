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
    public partial class PlayerSettingForm : Form
    {
        public int delay = 500;
        public bool asc = true;
        public PlayerSettingForm()
        {
            InitializeComponent(); 
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void OK_Click(object sender, EventArgs e)
        {
            if( !int.TryParse(textBox1.Text, out delay) )
            {
                MessageBox.Show("Invalid delay time.");
                return;
            }
            asc = radioButton1.Checked;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void PlayerSettingForm_Load(object sender, EventArgs e)
        {
            if (asc)
            {
                radioButton1.Checked = true;
                radioButton2.Checked = false;
            }
            else
            {
                radioButton1.Checked = false;
                radioButton2.Checked = true;
            }
            textBox1.Text = delay.ToString();
        }
    }
}
