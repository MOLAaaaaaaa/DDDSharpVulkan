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
    public partial class NewSlicerConfirmForm : Form
    {
        public string slicerName;
        public bool IsSmooth = false;
        public bool IsClosed = false;
        public NewSlicerConfirmForm()
        {
            InitializeComponent();
            slicerName = "untitled";
        }

        private void OKbutton1_Click(object sender, EventArgs e)
        {
            slicerName = textBox1.Text;
            DialogResult = DialogResult.OK;
            IsSmooth = checkBox1.Checked;
            IsClosed = checkBox2.Checked;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
