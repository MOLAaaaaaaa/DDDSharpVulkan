using DataCollection;
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
    public partial class MouseControlForm : Form
    {
        public MouseControlForm()
        {
            InitializeComponent();
        }

        private void MouseControlForm_Load(object sender, EventArgs e)
        {
            NormalTextBox.Text = C3DData.zoomSpeed.ToString();
            FastTextBox.Text = C3DData.zoomFastSpeed.ToString();
            SlowTextBox.Text = C3DData.zoomSlowSpeed.ToString();
        }

        private void Okbutton1_Click(object sender, EventArgs e)
        {
            if( !float.TryParse(NormalTextBox.Text, out float v1) )
            {
                MessageBox.Show("Invalid values !");
                return;
            }
            if (!float.TryParse(FastTextBox.Text, out float v2))
            {
                MessageBox.Show("Invalid values !");
                return;
            }
            if (!float.TryParse(SlowTextBox.Text, out float v3))
            {
                MessageBox.Show("Invalid values !");
                return;
            }
            
            C3DData.zoomSpeed = v1;
            C3DData.zoomFastSpeed = v2;
            C3DData.zoomSlowSpeed = v3;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancelbutton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
