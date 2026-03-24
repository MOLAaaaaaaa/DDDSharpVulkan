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

namespace DDDSharp.Dialogs
{
    public partial class RectangleInputDlg : Form
    {
        PolygonSlicer slicer = null;
        public DoubleRect rect = new DoubleRect();
        public RectangleInputDlg(PolygonSlicer _slicer)
        {
            InitializeComponent();
            slicer = _slicer;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            try 
            {
                double x1 = double.Parse(textBoxX1.Text);
                double x2 = double.Parse(textBoxX2.Text);
                double y1 = double.Parse(textBoxY1.Text);
                double y2 = double.Parse(textBoxY2.Text);
                rect = new DoubleRect(x1, y1, x2, y2);
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
        void UpdateRaneInfo()
        {

        }
        private void RectangleInputDlg_Load(object sender, EventArgs e)
        {
            Bitmap bmp = slicer.backImages[0].bmp;
            if (checkBox1.Checked) 
            {
                textBoxX1.Text = "0";
                textBoxX2.Text = "0";
                textBoxY1.Text = bmp.Width.ToString();
                textBoxY2.Text = bmp.Height.ToString();
            }
            else 
            {
                textBoxX1.Text = slicer.Minx.ToString();
                textBoxX2.Text = slicer.Maxx.ToString();
                textBoxY1.Text = slicer.Miny.ToString();
                textBoxY2.Text = slicer.Maxy.ToString();
            }
            
        }
    }
}
