using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CLInterpolation;
namespace DDDSharp
{
    public partial class IDWSet : Form
    {
        public IDWInterpolator ip = null;
        public IDWSet()
        {
            InitializeComponent();
        }

        private void OKbutton1_Click(object sender, EventArgs e)
        {
            double power,rad, xs, ys, zs;
            
            if (!double.TryParse(RadiusTextBox.Text, out rad))
            {
                MessageBox.Show("searching radius percentage not correct.");
                return;
            }
            if (!double.TryParse(XScaleTextBox.Text, out xs))
            {
                MessageBox.Show("Scale of X axis not correct.");
                return;
            }
            if (!double.TryParse(YScaleTextBox.Text, out ys))
            {
                MessageBox.Show("Scale of Y axis not correct.");
                return;
            }
            if (!double.TryParse(ZScaleTextBox.Text, out zs))
            {
                MessageBox.Show("Scale of Z axis not correct.");
                return;
            }
            if (!double.TryParse(PowerTextBox.Text, out power))
            {
                MessageBox.Show("power not correct.");
                return;
            }
            if ( rad <=0 || rad > 100 )
            {
                MessageBox.Show("radius percentage must between 0 to 100 (%).");
                return;
            }
            if ( power == 0 )
            {
                MessageBox.Show("power is not correct.");
                return;
            }
            if ( xs <= 0 )
            {
                MessageBox.Show("scale of x axis must be greater 0");
                return;
            }
            if ( ys <= 0 )
            {
                MessageBox.Show("scale of y axis must be greater 0");
                return;
            }
            if ( zs <= 0)
            {
                MessageBox.Show("scale of z axis must be greater 0");
                return;
            }
            ip.power = power;
            ip.searchRadiu = rad;
            ip.searchXScale = xs;
            ip.searchYScale = ys;
            ip.searchZScale = zs;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void IDWSet_Load(object sender, EventArgs e)
        {
            PowerTextBox.Text = ip.power.ToString();
            RadiusTextBox.Text = ip.searchRadiu.ToString();
            XScaleTextBox.Text = ip.searchXScale.ToString();
            YScaleTextBox.Text = ip.searchYScale.ToString();
            ZScaleTextBox.Text = ip.searchZScale.ToString();
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
