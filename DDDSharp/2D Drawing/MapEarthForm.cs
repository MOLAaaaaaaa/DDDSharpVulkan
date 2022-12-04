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
    public partial class MapEarthForm : Form
    {
        public int dpx = 0;
        public int dpy = 0;
        public double longitude = 0;
        public double latitude = 0;

        public MapEarthForm()
        {
            InitializeComponent();            
        }

        private void OK_Click(object sender, EventArgs e)
        {
            double.TryParse(textBoxLatitude.Text, out latitude);
            double.TryParse(textBoxLongitude.Text, out longitude);
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void MapEarthForm_Load(object sender, EventArgs e)
        {
            textBoxPixelX.Text = dpx.ToString();
            textBoxPixelY.Text = dpy.ToString();
        }
    }
}
