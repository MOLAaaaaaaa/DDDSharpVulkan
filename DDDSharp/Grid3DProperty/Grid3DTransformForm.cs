using DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AviFile.Avi;

namespace DDDSharp
{
    public partial class Grid3DTransformForm : Form
    {
        RectRuler ruler = new RectRuler();
        public C3DGridData data = null;     
        public bool Modified = false;
        public bool rangeUpdated = false;
        double minx, maxx, miny, maxy,minz, maxz,minv,maxv;
        public Grid3DTransformForm(C3DGridData grid)
        {
            InitializeComponent();
            data = grid.Copy();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ApplyButton2_Click(object sender, EventArgs e)
        {
            double x1 = minx, x2 = maxx;
            double y1 = miny, y2 = maxy;
            double z1 = minz, z2 = maxz;
            double v1 = minv, v2 = maxv;
            try 
            {
                if (XCheck.Checked)
                {
                    x1 = double.Parse(XtextBox1.Text);
                    x2 = double.Parse(XtextBox2.Text);
                    Modified = true;
                    if (x1 != minx || x2 != maxx) rangeUpdated = true;
                }
                if (YCheck.Checked)
                {
                    y1 = double.Parse(YtextBox1.Text);
                    y2 = double.Parse(YtextBox2.Text);
                    Modified = true;
                    if (y1 != miny || y2 != maxy) rangeUpdated = true;
                }
                if (ZCheck.Checked)
                {
                    z1 = double.Parse(ZtextBox1.Text);
                    z2 = double.Parse(ZtextBox2.Text);
                    Modified = true;
                    if (z1 != minz || z2 != maxz) rangeUpdated = true;
                }
                if (Modified)
                {
                    data.ResetDataRange(x1, x2, y1, y2, z1, z2, v1,v2);
                    if (Modified) { MessageBox.Show("Done!!!"); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }            
        }

        void Draw(Graphics g)
        {
            int width = pictureBox1.Width;
            int height = pictureBox1.Height;
            Rectangle rect = new Rectangle(0,0,width,height);
            ruler.SetDrawRect(rect);
            ruler.SetMarine(50, 10, 10, 10);
            //if()
            //ruler.leftRuler.Direction = AxisDirectionEnum.UpDown;
            //ruler.leftRuler.SetValuesRange(0, maxDepth);
            //ruler.Draw(g);
            //if (cv != null) cv.Draw(g, ruler.drawRect, 0, cv.maxDepth);
            //ruler.DrawCursor(g);
        }
        private void Grid3DTransformForm_Load(object sender, EventArgs e)
        {
            if (data != null)
            {
                minx = data.minx;
                miny = data.miny;
                minz = data.minz;
                minv = data.minv;
                maxx = data.maxx;
                maxy = data.maxy;
                maxz = data.maxz;
                maxv = data.maxv;

                comboBox1.Items.Add("X Direction");
                comboBox1.Items.Add("Y Direction");
                comboBox1.Items.Add("Z Direction");
                comboBox1.Items.Add("XY Exchange");

                labelX.Text = "X: " + minx + " to " + maxx;
                labelY.Text = "Y: " + miny + " to " + maxy;
                labelZ.Text = "Z: " + minz + " to " + maxz;
                labelV.Text = "V: " + minv + " to " + maxv;

                XtextBox1.Text = minx.ToString();
                XtextBox2.Text = maxx.ToString();
                YtextBox1.Text = miny.ToString();
                YtextBox2.Text = maxy.ToString();
                ZtextBox1.Text = minz.ToString();
                ZtextBox2.Text = maxz.ToString();
                comboBox1.SelectedIndex = 0;
            }
        }

        private void ApplyButton1_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (comboBox1.SelectedIndex == 0) 
            {
                data.FlipGridData(AxisEnum.xAxis);
                Modified = true;
            }
            if (comboBox1.SelectedIndex == 1)
            {
                data.FlipGridData(AxisEnum.yAxis);
                Modified = true;
            }
            if (comboBox1.SelectedIndex == 2)
            {
                data.FlipGridData(AxisEnum.zAxis);
                Modified = true;
            }
            if (comboBox1.SelectedIndex == 3)
            {
                data.FlipGridData(AxisEnum.XYExchange);
                Modified = true;
            }            
            Cursor = Cursors.Default;
            MessageBox.Show("Done!!!");
            pictureBox1.Invalidate();
        }
    }
}
