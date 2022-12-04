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
    public partial class SampleGridSetForm : Form
    {
        public int nx = 0, ny = 0;
        public double xstep = 0, ystep = 0;
        public bool EnableSplite = false;
        public bool IsSpliteInverse = false;
        public int dotSize = 4;
        private void OK_Click(object sender, EventArgs e)
        {
            try 
            {
                nx = int.Parse(XNumTextBox.Text);
                ny = int.Parse(YNumTextBox.Text);
                xstep = double.Parse(XSpaceTextBox.Text);
                ystep = double.Parse(YSpaceTextBox.Text);
                EnableSplite = SpliteCheckBox.Checked;
                IsSpliteInverse = SpliteInverseCheckBox.Checked;
                dotSize = int.Parse(DotSizeTextBox.Text);
                DialogResult = DialogResult.OK;
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public SampleGridSetForm()
        {
            InitializeComponent();
        }

        private void SampleGridSetForm_Load(object sender, EventArgs e)
        {
            XNumTextBox.Text = nx.ToString();
            YNumTextBox.Text = ny.ToString();
            XSpaceTextBox.Text = xstep.ToString();
            YSpaceTextBox.Text = ystep.ToString();
            SpliteCheckBox.Checked = EnableSplite;
            SpliteInverseCheckBox.Checked = IsSpliteInverse;
            DotSizeTextBox.Text = dotSize.ToString();
        }

    }
}
