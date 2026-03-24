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
    public partial class MessageBoxOnListForm : Form
    {
        public string CaptionText = "";
        public string TitleText = "";
        public string MessageText = "";
        public string[] Messages = null;
        public MessageBoxOnListForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MessageBoxOnListForm_Load(object sender, EventArgs e)
        {
            Text = CaptionText;
            TitileLabel.Text = TitleText;
            if (Messages != null) textBox1.Lines = Messages;
            else textBox1.Text = MessageText.Replace("\n", "\r\n");
        }
    }
    public static class MessageBoxs
    {
        public static void ListFormBoxShow(string caption,string title,string message)
        {
            MessageBoxOnListForm form = new MessageBoxOnListForm();
            form.CaptionText = caption;
            form.TitleText = title;
            form.MessageText = message;
            form.ShowDialog();
        }
        public static void ListFormBoxShow(string caption, string title, List<string> messages)
        {
            MessageBoxOnListForm form = new MessageBoxOnListForm();
            form.CaptionText = caption;
            form.TitleText = title;
            form.Messages = messages.ToArray();
            form.ShowDialog();
        }
        public static string InputDlgShow(string caption, string title)
        {
            MessageBoxInputDlg form = new MessageBoxInputDlg();
            form.TitleText = caption;
            form.labelText = title;
            if (form.ShowDialog() == DialogResult.OK)
            {
                return form.textInput;
            }
            else return "";
        }
    }
}
