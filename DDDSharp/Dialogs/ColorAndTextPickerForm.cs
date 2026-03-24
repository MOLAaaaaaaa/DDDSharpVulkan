using MathNet.Numerics.RootFinding;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
using static AviFile.Avi;

namespace DDDSharp.Dialogs
{
    public partial class ColorAndTextPickerForm : Form
    {
        int pickMode = 0;//0 nothing 1 color 2 text
        public Bitmap m_Bmp = null;
        public StratumData Layer = null;
        Rectangle DrawRect;
        DoubleRect DataRect;
        public bool Modified = false;
        string errMessage = "";
        public ColorAndTextPickerForm()
        {
            InitializeComponent();
            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);
           // WindowState = FormWindowState.Maximized;
        }
        void CreateDataRect()
        {
            if (m_Bmp == null) return;

            DrawRect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);

            double width = pictureBox1.Width;
            double height = pictureBox1.Height;            

            double x1 = 0, y1 = 0;
            double x2, y2;
            if (m_Bmp.Width > m_Bmp.Height)
            {
                x1 = 0;
                x2 = m_Bmp.Width;
                y1 = 0;
                y2 = (x2 - x1) * height / width;
            }
            else
            {
                y1 = 0;
                y2 = m_Bmp.Height;
                x1 = 0;
                x2 = (y2 - y1) * width / height;
            }
            DataRect = new DoubleRect(x1, y1, x2, y2);
        }
        bool CreateBitmap(string path)
        {
            try 
            {
                if (m_Bmp != null) m_Bmp.Dispose();
                Bitmap bmp = new Bitmap(path);
                m_Bmp = new Bitmap(bmp);
                bmp.Dispose();
                CreateDataRect();
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }            
        }
        void CreateBitmap(Bitmap bmp)
        {
            if (bmp == null) return;
            if (m_Bmp != null) m_Bmp.Dispose();
            m_Bmp = new Bitmap(bmp);

            CreateDataRect();
        }
        public ColorAndTextPickerForm(Bitmap bmp)
        {
            InitializeComponent();
            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);
            //WindowState = FormWindowState.Maximized;
            CreateBitmap(bmp);
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

        private void ColorButton_Click(object sender, EventArgs e)
        {
            pickMode = 1;
            Cursor = Cursors.Cross;
        }
        Color pickColor(Point p)
        {
            Point p0 = pictureBox1.PointToScreen(new Point(0,0));
            int x0 = p0.X;
            int y0 = p0.Y;

            int width = pictureBox1.Width;
            int height = pictureBox1.Height;
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.CopyFromScreen(x0, y0, 0, 0, new Size(width, height));
            Color color = bmp.GetPixel(p.X,p.Y);            
            bmp.Dispose();
            return color;
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left) 
            {
                if ( pickMode == 1 )
                {
                    Color color = pickColor(e.Location);
                    Layer.Color = color;
                    ColorButton.BackColor = color;
                    Modified = true;
                    propertyGrid1.SelectedObject = Layer;
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                pickMode = 0;
                Cursor = Cursors.Default;
            }
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Resource1.ImageFilesFormatFilter;
            if( dlg.ShowDialog() == DialogResult.OK ) 
            {
                if(CreateBitmap(dlg.FileName )) pictureBox1.Invalidate();
                else MessageBox.Show(errMessage);
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if(m_Bmp == null) return;
            double x1 = DataRect.X1;
            double x2 = DataRect.X2;
            double y1 = DataRect.Y1;
            double y2 = DataRect.Y2;
            if ( x1 < 0 ) x1 = 0;
            if ( y1 < 0 ) y1 = 0;
            if ( x2 > m_Bmp.Width ) x2  = m_Bmp.Width;
            if ( y2 > m_Bmp.Height ) y2 = m_Bmp.Height;
            double dx1 = x1;
            double dy1 = y1;
            double dx2 = x2;
            double dy2 = y2;
            LPtoDP(ref dx1, ref dy1);
            LPtoDP(ref dx2, ref dy2);
            if(dx1< 0 ) dx1 = 0;
            if(dy1< 0 ) dy1 = 0;            
            if (dx2 > pictureBox1.Width) dx2 = pictureBox1.Width;
            if (dy2 > pictureBox1.Height) dy2 = pictureBox1.Height;
            Rectangle srRect = new Rectangle((int)x1, (int)y1, (int)(x2-x1), (int)(y2-y1));
            Rectangle dstRect = new Rectangle((int)dx1, (int)dy1, (int)(dx2 - dx1), (int)(dy2 - dy1));
            e.Graphics.DrawImage(m_Bmp,dstRect,srRect,GraphicsUnit.Pixel);
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                Point p0 = new Point(e.X, e.Y);
                if (e.Delta > 0)
                {
                    Zoom(p0,0.8);
                }
                else
                {
                    Zoom(p0, 1/0.8);
                }
                Invalidate();
            }
        }
        void LPtoDP(ref double x, ref double y)
        {
            x = DrawRect.Left + DrawRect.Width * (x - DataRect.X1) / DataRect.Width;
            y = DrawRect.Top + DrawRect.Height * (y - DataRect.Y1) / DataRect.Height;
        }
        void DPtoLP(ref double x, ref double y)
        {
            x = DataRect.X1 + DataRect.Width * (x - DrawRect.Left) / DrawRect.Width;
            y = DataRect.Y1 + DataRect.Height * (y- DrawRect.Top) / DrawRect.Height;
        }
        /// <summary>
        /// ZoomIn or ZoomOut
        /// </summary>
        /// <param name="p"></param>
        /// <param name="scale"></param>
        private void Zoom(Point p, double scale )
        {
            if (m_Bmp == null) return;
            double x0 = p.X;
            double y0 = p.Y;
            DPtoLP(ref x0, ref y0);

            double dx = DataRect.Width * scale;
            double dy = DataRect.Height * scale;
            double x1 = x0 - dx / 2;
            double x2 = x0 + dx / 2;
            double y1 = y0 - dy / 2;
            double y2 = y0 + dy / 2;
            if (x1 >= x2 || y1 >= y2) return; //放大极限
            DataRect = new DoubleRect(x1, y1, x2, y2);
            pictureBox1.Invalidate();
        }       

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            CreateDataRect();
            Invalidate();
        }

        private void retate90ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if( m_Bmp == null ) return;
            m_Bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            DataRect = new DoubleRect(0, 0, m_Bmp.Width, m_Bmp.Height);
            pictureBox1.Invalidate();
        }

        private void flipHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_Bmp == null) return;
            m_Bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            DataRect = new DoubleRect(0, 0, m_Bmp.Width, m_Bmp.Height);
            pictureBox1.Invalidate();
        }

        private void flipVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_Bmp == null) return;
            m_Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            DataRect = new DoubleRect(0, 0, m_Bmp.Width, m_Bmp.Height);
            pictureBox1.Invalidate();
        }

        private void ColorAndTextPickerForm_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = Layer;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Modified = true;
        }
    }
}
