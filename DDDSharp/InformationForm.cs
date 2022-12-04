using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Drawing.Imaging;
namespace DDDSharp
{
    public partial class InformationForm : DockingPaneExt
    {
        public InformationForm()
        {
            InitializeComponent();
        }
        public void AddNewLine()
        {
            textBox1.AppendText(System.Environment.NewLine);
        }
        public void AddInfo(string infor)
        {
            if (textBox1 == null) return;
            textBox1.AppendText(infor);            
            AddNewLine();
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                ContextMenuStrip = contextMenuStrip1;
            }//if (e.Button == MouseButtons.Right)
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox1.Text = "";            
        }
        
        private void InformationForm_Load(object sender, EventArgs e)
        {
           
        }
    }
}
