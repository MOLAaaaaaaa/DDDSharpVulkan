using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDDSharp.Dialogs
{
    public partial class MessageBoxInputDlg : Form
    {
        public string TitleText = "Information";
        public string labelText = "";
        public string textInput = "";
        public string invalidVerifyText = "输入错误！";
        public bool NumericVerify = false;
        public MessageBoxInputDlg()
        {
            InitializeComponent();
        }

        private void MessageBoxInputDlg_Load(object sender, EventArgs e)
        {
            Text = TitleText;
            Label1.Text = labelText;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text.Trim();
            if (text.Length <1 )
            {
                MessageBox.Show(invalidVerifyText);
                return;
            }
            if(NumericVerify)
            {
                if( !double.TryParse(text, out double result) )
                {
                    MessageBox.Show(invalidVerifyText);
                    return;
                }
            }
            textInput = text;
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
