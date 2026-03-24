using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;

namespace DDDSharp
{
    struct ImageEarthMapStruct
    {
        public int dx;//像素位置
        public int dy;
        public double longitude;
        public double latitude;
        public ImageEarthMapStruct(int _dx,int _dy,double _longitude, double _latitude)
        {
            dx = _dx;
            dy = _dy;
            longitude = _longitude;
            latitude = _latitude;
        }        
    }

    public partial class ConvertImageToEarthForm : Form
    {
        public Bitmap bmp = null;
        Color []bmpColors = null;

        Rectangle DrawRect;
        DoubleRect DataRect;

        int leftMargin = 10;
        int rightMargin = 10;
        int topMargin = 10;
        int bottomMargin = 10;

#pragma warning disable CS0414 // 字段“ConvertImageToEarthForm.bMouseDown”已被赋值，但从未使用过它的值
        bool bMouseDown = false;
#pragma warning restore CS0414 // 字段“ConvertImageToEarthForm.bMouseDown”已被赋值，但从未使用过它的值
        Point first = new Point(-1, -1);
        Point second = new Point(-1, -1);

        MouseState mouseState = MouseState.normal;

        List<ImageEarthMapStruct> mappedPoints = new List<ImageEarthMapStruct>();
        List<PLine> Lines = new List<PLine>(); //curves 
        PLine tempLine;

        List<PLine> smoothedLines = new List<PLine>();

        public ConvertImageToEarthForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.          

            KeyPreview = true;
            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);
            //WindowState = FormWindowState.Maximized;            
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Images File(*.jpg;*.png;*.gif;*.jpeg;*.bmp)|*.jpg;*.png;*.gif;*.jpeg;*.bmp|all files(*.*)|*.*";
                    dlg.Multiselect = false;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        bmp = new Bitmap(dlg.FileName);
                        CreateBmpColorArray(bmp);

                        ResetRect();
                        this.Cursor = Cursors.Default;
                        UpdateDraw();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            y = DataRect.Y1 + DataRect.Height * (y - DrawRect.Top) / DrawRect.Height;
        }
        void UpdateDraw()
        {
            pictureBox1.Invalidate();
        }

        void DrawImage(Graphics g)
        {
            if (bmp == null) return;

            double x1 = DataRect.X1;
            double x2 = DataRect.X2;
            double y1 = DataRect.Y1;
            double y2 = DataRect.Y2;

            double dx1 = x1;
            double dy1 = y1;
            double dx2 = x2;
            double dy2 = y2;

            LPtoDP(ref dx1, ref dy1);
            LPtoDP(ref dx2, ref dy2);

            Rectangle dstRect = new Rectangle((int)dx1, (int)dy1, (int)(dx2 - dx1), (int)(dy2 - dy1));
            Rectangle srcRect = new Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));

            g.DrawImage(bmp, dstRect, srcRect, GraphicsUnit.Pixel);
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawImage(e.Graphics);
            DrawLines(e.Graphics);
            DrawDynamicLine(e.Graphics);
            DrawMappedPoints(e.Graphics);
        }       

        void ResetRect()
        {

            DrawRect = new Rectangle(leftMargin, topMargin,
                                     pictureBox1.Width - leftMargin - rightMargin,
                                     pictureBox1.Height - topMargin - bottomMargin);
            if (bmp == null) return;

            double x1 = 0;
            double x2 = bmp.Width;
            double y1 = 0;
            double y2 = bmp.Height;

            double x0 = (x1 + x2) / 2.0;
            double y0 = (y1 + y2) / 2.0;

            //size of draw rect
            double width = (y2 - y1) * DrawRect.Width / DrawRect.Height;
            double height = (x2 - x1) * DrawRect.Height / DrawRect.Width;

            //ratio of data  
            double scale = (y2 - y1) / (x2 - x1);

            if (scale * DrawRect.Width < DrawRect.Height)
            {
                DataRect.X1 = x1;
                DataRect.X2 = x2;
                DataRect.Y1 = y0 - height / 2.0;
                DataRect.Y2 = y0 + height / 2.0;
            }
            else
            {
                DataRect.Y1 = y1;
                DataRect.Y2 = y2;
                DataRect.X1 = x0 - width / 2.0;
                DataRect.X2 = x0 + width / 2.0;
            }
        }

        bool IsInSelected(int i)
        {
            return true;
        }

        void DrawMappedPoints(Graphics g)
        {
            if (mappedPoints.Count < 1) return;

            Pen pen1, pen2;
            pen1 = new Pen(Color.BurlyWood);
            pen1.Width = 2;
            pen2 = new Pen(Color.Blue);
            pen2.Width = 1;

            int size1 = 4;
            int size2 = 10;
            int k = 0;
            Point p1, p2;
            double x1, y1;

            Point p0 = new Point();

            foreach (ImageEarthMapStruct p in mappedPoints)
            {
                x1 = p.dx;
                y1 = p.dy;
                LPtoDP(ref x1, ref y1);

                p0 = new Point((int)x1, (int)y1);
                p1 = p2 = p0;

                p1.X -= size1;
                p2.X += size1;
                g.DrawLine(pen1, p1, p2);

                p1 = p2 = p0;
                p1.Y -= size1;
                p2.Y += size1;
                g.DrawLine(pen1, p1, p2);

                if (IsInSelected(k))
                {
                    pen2.Color = Color.Red;
                    pen2.Width = 2;
                }
                else
                {
                    pen2.Color = Color.Blue;
                    pen2.Width = 1;
                }

                g.DrawEllipse(pen2, (int)(x1 - size2), (int)(y1 - size2), size2 * 2, size2 * 2);

            }


            pen1.Dispose();
            pen2.Dispose();
        }
        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            ResetRect();
            UpdateDraw();
        }
        void UpdatePosition(int x,int y)
        {
            if (bmp == null) return;

            double x1 = x;
            double y1 = y;
            DPtoLP(ref x1, ref y1);

            string text = "Image Size " + bmp.Width + " * " + bmp.Height + " | ";
            text += "(" + (int)x1 + " , " + (int)y1 + ")";
            curPositionLabel.Text = text;
        }

        void DrawLine(Graphics g, PLine line, Color color, Color gridcolor, bool smooth = true,bool showgrid = true, int size = 6,bool filled = true)
        {
            if ( line.Count < 1 ) return;

            Point[] points = new Point[line.Count];
            double x1, y1;
            for (int i = 0; i < line.Count; i++)
            {
                x1 = line[i].x;
                y1 = line[i].y;
                LPtoDP(ref x1, ref y1);
                points[i] = new Point((int)x1, (int)y1);
            }
            
            Pen pen = new Pen(color, 1);
            if ( line.Count > 1)
            {
                if (smooth) g.DrawCurve(pen, points);
                else g.DrawCurve(pen, points, 0);
            }            

            if (showgrid)
            {
                Brush brush = new SolidBrush(gridcolor);

                foreach (Point p in points)
                {
                   if(filled) g.FillRectangle(brush, p.X-size, p.Y-size, size * 2, size * 2);
                   else g.DrawRectangle(pen, p.X - size, p.Y - size, size * 2, size * 2);
                }

                brush.Dispose();
            }

            pen.Dispose();
            points = null;            
        }

        void DrawLines(Graphics g, bool smooth = true)
        {
            if (bmp == null) return;

            foreach (PLine line in Lines)
            {
                DrawLine(g, line, Color.Blue, Color.BlueViolet,smooth);
            }
        }

        void DrawDynamicLine(Graphics g,bool smooth = true)
        {
            if (bmp == null) return;
            if (mouseState != MouseState.draw) return;
            if (tempLine.Count < 2) return;
            DrawLine(g, tempLine, Color.Blue, Color.BlueViolet, smooth,true,6,false);            
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if( mouseState == MouseState.draw )
            {
                pictureBox1.Invalidate();
            }

            UpdatePosition(e.X,e.Y);
        }
        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                Point p0 = new Point(e.X, e.Y);
                if (e.Delta > 0)
                {
                    ZoomIn(p0);
                }
                else
                {
                    ZoomOut(p0);
                }
                UpdateDraw();
            }
        }
        void ZoomRect(Point p1, Point p2)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double x2 = p2.X;
            double y2 = p2.Y;

            if (Math.Abs(x2 - x1) < 5 || Math.Abs(y2 - y1) < 5)
            {
                ZoomIn(new Point((int)((x1 + x1) / 2.0), (int)((y1 + y1) / 2.0)));
                return;
            }

            DPtoLP(ref x1, ref y1);
            DPtoLP(ref x2, ref y2);
            double x0 = (x1 + x2) / 2.0;
            double y0 = (y1 + y2) / 2.0;

            double xx = Math.Abs(x2 - x1);
            double yy = Math.Abs(y2 - y1);

            double scale = DataRect.Height / DataRect.Width;
            if (scale * DrawRect.Width < DrawRect.Height)
            {
                yy = xx * scale;
            }
            else
            {
                xx = yy / scale;
            }

            DataRect.X1 = x0 - xx / 2.0;
            DataRect.Y1 = y0 - yy / 2.0;
            DataRect.X2 = x0 + xx / 2.0;
            DataRect.Y2 = y0 + yy / 2.0; ;
        }

        //以p0点为中心缩放
        private void Zoom(Point p0, double scale = 0.8)
        {
            double x1 = 0, y1 = 0;
            double x2 = 0, y2 = 0;
            //滚轮缩放时，重新计算点
            if (first.X != -1 && second.X != -1)
            {
                x1 = first.X;
                y1 = first.Y;
                x2 = second.X;
                y2 = second.Y;
                DPtoLP(ref x1, ref y1);
                DPtoLP(ref x2, ref y2);
            }

            double x0 = p0.X;
            double y0 = p0.Y;
            DPtoLP(ref x0, ref y0);
            double xx = DataRect.Width * scale;
            double yy = DataRect.Height * scale;
            DataRect.X1 = x0 - xx / 2;
            DataRect.X2 = x0 + xx / 2;
            DataRect.Y1 = y0 - yy / 2;
            DataRect.Y2 = y0 + yy / 2;

            //滚轮缩放时，重新计算点
            if (first.X != -1 && second.X != -1)
            {
                LPtoDP(ref x1, ref y1);
                LPtoDP(ref x2, ref y2);
                first.X = (int)x1;
                first.Y = (int)y1;
                second.X = (int)x2;
                second.Y = (int)y2;
            }
        }
        private void ZoomIn(Point p0, double scale = 0.8)
        {
            Zoom(p0, scale);
        }

        private void ZoomOut(Point p0, double scale = 1.25)
        {
            Zoom(p0, scale);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            bMouseDown = true;
            first = new Point(e.X, e.Y);
            second = new Point(-1, -1);
            if ( e.Button == MouseButtons.Left )
            {
                if(mouseState == MouseState.locate)
                {
                    double x1 = e.X;
                    double y1 = e.Y;
                    DPtoLP(ref x1, ref y1);
                    /*
                    MapEarthForm mp = new MapEarthForm();
                    mp.dpx = (int)x1;
                    mp.dpy = (int)y1;
                    if ( mp.ShowDialog() == DialogResult.OK )
                    {                        
                        mappedPoints.Add(new ImageEarthMapStruct((int)x1, (int)y1, mp.longitude, mp.latitude) );
                        UpdateDraw();
                    }
                    */
                    mappedPoints.Add(new ImageEarthMapStruct((int)x1, (int)y1, 0,0));
                    UpdateDraw();
                }

                if (mouseState == MouseState.draw && tempLine != null )
                {
                    double x1 = e.X;
                    double y1 = e.Y;
                    DPtoLP(ref x1, ref y1);
                    tempLine.Add(x1, y1, 0);                   
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                if (mouseState == MouseState.draw)
                {
                    if(tempLine != null && tempLine.Count > 1 )
                    {
                        AddCurve(tempLine);
                        tempLine = null;
                        UpdateDraw();
                    }
                }

                mouseState = MouseState.normal;
                UpdateMouseState();
            }
        }
        public void UpdateMouseState()
        {
            switch (mouseState)
            {
                case MouseState.zoom_in:
                    pictureBox1.Cursor = Cursors.Cross;
                    break;
                case MouseState.zoom_out:
                    //pictureBox1.Cursor = cursorZoomOut;
                    break;
                case MouseState.pan:
                    //pictureBox1.Cursor = cursorPan;
                    break;
                case MouseState.moving:
                    pictureBox1.Cursor = Cursors.SizeAll;
                    break;
                case MouseState.locate:
                case MouseState.draw:
                    pictureBox1.Cursor = Cursors.Cross;
                    break;
                case MouseState.normal:
                case MouseState.reset:
                    pictureBox1.Cursor = Cursors.Default;
                    break;               
            }
        }
        private void toolStripButtonLocate_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.locate;            
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }

        int Clamp(int v,int v1,int v2)
        {
            if (v < v1) v = v1;
            if (v > v2) v = v2;
            return v;
        }

        //   ? 3 ? 
        //   1 0 2
        //   ? 4 ?
        Color GetInterpolatedColor(double x,double y)
        {
            int ix1, ix2, iy1, iy2;
            int ix = (int)x;
            int iy = (int)y;
            ix1 = ix - 1;
            ix2 = ix + 1;
            iy1 = iy - 1;
            iy2 = iy + 1;

            int width = bmp.Width;
            int height = bmp.Height;

            ix = Clamp(ix, 0, width - 1);
            iy = Clamp(iy, 0, height - 1);
            ix1 = Clamp(ix1, 0, width - 1);
            iy1 = Clamp(iy1, 0, height - 1);
            ix2 = Clamp(ix2, 0, width - 1);
            iy2 = Clamp(iy2, 0, height - 1);

            Color c0 = bmpColors[ix + iy * width];
            double r0 = (x - ix) * (x - ix) + (y - iy) * (y - iy);
            if (r0 == 0) return c0;

            Color c1 = bmpColors[ix1 + iy * width];
            double r1 = (x - ix1) * (x - ix1) + (y - iy) * (y - iy);
            if (r1 == 0) return c1;

            Color c2 = bmpColors[ix2 + iy * width];
            double r2 = (x - ix2) * (x - ix2) + (y - iy) * (y - iy);
            if (r2 == 0) return c2;

            Color c3 = bmpColors[ix + iy1 * width];
            double r3 = (x - ix) * (x - ix) + (y - iy1) * (y - iy1);
            if (r3 == 0) return c3;

            Color c4 = bmpColors[ix + iy2 * width];
            double r4 = (x - ix) * (x - ix) + (y - iy2) * (y - iy2);
            if (r4 == 0) return c4;

            r0 = 1.0 / Math.Sqrt(r0);
            r1 = 1.0 / Math.Sqrt(r1);
            r2 = 1.0 / Math.Sqrt(r2);
            r3 = 1.0 / Math.Sqrt(r3);
            r4 = 1.0 / Math.Sqrt(r4);

            double sum = r0 + r1 + r2 + r3 + r4;
            double r, g, b;
            r = r0 / sum * c0.R + r1 / sum * c1.R + r2 / sum * c2.R + r3 / sum * c3.R + r4 / sum * c4.R;
            g = r0 / sum * c0.G + r1 / sum * c1.G + r2 / sum * c2.G + r3 / sum * c3.G + r4 / sum * c4.G;
            b = r0 / sum * c0.B + r1 / sum * c1.B + r2 / sum * c2.B + r3 / sum * c3.B + r4 / sum * c4.B;

            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            return Color.FromArgb((int)r, (int)g, (int)b);

        }

        //第一步图像梯形校正
        void trapezoidRevise()
        {
            if (mappedPoints.Count < 4)
            {
                MessageBox.Show("no enough mapped points.");
                return;
            }
            double longitude1;
            double longitude2;
            double latitude1;
            double latitude2;
            int px1, py1;
            int px2, py2;

            ImageEarthMapStruct p = mappedPoints[0];
            longitude1 = longitude2 = p.longitude;
            latitude1 = latitude2 = p.latitude;
            px1 = px2 = p.dx;
            py1 = py2 = p.dy;
            for (int i = 1; i < mappedPoints.Count; i++)
            {
                p = mappedPoints[i];
           /*   
            *   if (p.longitude < longitude1) longitude1 = p.longitude;
                if (p.longitude > longitude2) longitude2 = p.longitude;
                if (p.latitude < latitude1) latitude1 = p.latitude;
                if (p.latitude > latitude2) latitude2 = p.latitude;
             */
                if (p.dx < px1) px1 = p.dx;
                if (p.dx > px2) px2 = p.dx;
                if (p.dy < py1) py1 = p.dy;
                if (p.dy > py2) py2 = p.dy;
            }
            //   p1       p2
            // p3          p4
            // p5          p6
            ImageEarthMapStruct p1 = mappedPoints[0];
            ImageEarthMapStruct p2 = mappedPoints[1];
            ImageEarthMapStruct p3 = mappedPoints[2];
            ImageEarthMapStruct p4 = mappedPoints[3];
            double len13 = Math.Sqrt((p1.dx - p3.dx) * (p1.dx - p3.dx) + (p1.dy - p3.dy) * (p1.dy - p3.dy));
            double len24 = Math.Sqrt((p2.dx - p4.dx) * (p2.dx - p4.dx) + (p2.dy - p4.dy) * (p2.dy - p4.dy));
            double len = len13;
            if (len24 > len) len = len24;

            //window len from bottom
            int hh1 = bmp.Height - p3.dy;
            int hh2 = bmp.Height - p4.dy;
            int hh = hh1;
            if (hh2 > hh) hh = hh2;           

            int width = Math.Abs(px2 - px1);
            int height = (int)len + hh;

            Color[] colors = new Color[width * height];            

            double x, y, x1, y1, x2, y2;            
            double s1, s2;
            for (int iy = 0; iy < (int)len; iy++)
            {
                s1 = (double)iy / ((int)len-1); //0 - 1
                x1 = p1.dx + (p3.dx - p1.dx) * s1; //p1 - p3
                y1 = p1.dy + (p3.dy - p1.dy) * s1; //p1 - p3
                x2 = p2.dx + (p4.dx - p2.dx) * s1; //p2 - p4
                y2 = p2.dy + (p4.dy - p2.dy) * s1; //p2 - p4
               
                for (int ix = 0; ix < width; ix++)
                {
                    s2 = (double)ix / (width - 1); //0 - 1
                    x = x1 + (x2 - x1) * s2;
                    y = y1 + (y2 - y1) * s2;
                    //get interpolated color at x,y
                    colors[iy * width + ix] = GetInterpolatedColor( x, y);
                }
            }
            
            for (int iy = 0; iy < hh; iy++)
            {
                s1 = (double)iy / (double)(hh - 1); //0 - 1
                x1 = p3.dx;
                x2 = p4.dx;

                y1 = p3.dy + (bmp.Height - 1 - p3.dy) * s1;                
                y2 = p4.dy + (bmp.Height - 1 - p4.dy) * s1;

                for (int ix = 0; ix < width; ix++)
                {
                    s2 = (double)ix / (width - 1); //0 - 1
                    x = x1 + (x2 - x1) * s2;
                    y = y1 + (y2 - y1) * s2;
                    colors[(iy + (int)len) * width + ix] = GetInterpolatedColor( x, y);
                }
            }

            Bitmap img = new Bitmap(width, height);            
            LockBitmap lockbmp = new LockBitmap(img);
            //锁定Bitmap，通过Pixel访问颜色
            lockbmp.LockBits();
            lockbmp.fromColorArray(colors);            
            //从内存解锁Bitmap
            lockbmp.UnlockBits();
            bmp.Dispose();
            bmp = img;
            colors = null;
            mappedPoints.Clear();            

            CreateBmpColorArray( bmp );
            UpdateDraw();

        }

        void CreateBmpColorArray(Bitmap _bmp)
        {
            bmpColors = null;
            int width = _bmp.Width;
            int height = _bmp.Height;
            bmpColors = new Color[width * height];            
            LockBitmap lockbmp = new LockBitmap(_bmp);
            lockbmp.LockBits();
            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    bmpColors[ix + iy * width] = lockbmp.GetPixel(ix, iy);
                }
            }
            lockbmp.UnlockBits();            
        }

        void AddCurve(PLine line)
        {
            if (line.Count < 2) return;

            Vector64 p1 = line[0];
            Vector64 p2 = line[line.Count -1];

            if (p1.x != 0  ) p1.x = 0;
            if (p2.x != bmp.Width - 1 ) p2.x = bmp.Width - 1;
            line[0] = p1;
            line[line.Count - 1] = p2;

            Lines.Add(line);
            CreateSmoothedLine(line);
        }
        bool CreateSmoothedLine(PLine line)
        {
            CubicSpline spline = new CubicSpline();
            PLine sline = new PLine();
            sline.points = spline.CreateSpline( line.points );
            smoothedLines.Add(sline);
            return true;
        }

        double GetInterpolatedYValue( PLine line, double x)
        {
            if (line.Count < 2) return 0;
            Vector64 p,p1,p2;
            double y;
            p = p1 = p2 = new Vector64();
            for( int i = 0; i < line.Count; i++ )
            {
                p = line[i];
                if (x == p.x) return p.y;
                else if (x > p.x) p1 = p;
                else 
                { 
                    p2 = p;
                    y = p1.y + (p2.y - p1.y) *(x-p1.x)/(p2.x-p1.x);
                    return y;
                }
            }
            return p.y;
        }

        void earthRevise()
        {
            if( Lines.Count < 2 )
            {
                MessageBox.Show("no enough latitude curves.");
                return;
            }
            
            int width = bmp.Width;
            int height = bmp.Height;

            Color color;
            double x, y, y1, y2,s1;

            Bitmap img = new Bitmap(width, height);
            LockBitmap lockbmp = new LockBitmap(img);
            //锁定Bitmap，通过Pixel访问颜色
            lockbmp.LockBits();

            for (int ix = 0; ix < width; ix++ )
            {
                x = ix;
                y1 = GetInterpolatedYValue(smoothedLines[0], x);                
                y2 = GetInterpolatedYValue(smoothedLines[1], x);
                if( y1 > y2 ) { y = y1;y1 = y2; y2 = y; }
                for(int iy = 0; iy < height; iy++ )
                {
                    s1 = (double)iy / (double)(height - 1);
                    y = y1 + (y2 - y1) * s1;
                    color = GetInterpolatedColor( x, y);
                    lockbmp.SetPixel(ix, iy, color);
                }
            }
            //从内存解锁Bitmap
            lockbmp.UnlockBits();
            bmp.Dispose();
            bmp = img;

            CreateBmpColorArray(bmp);

            UpdateDraw();

        }
        //Convert to Earth Texture
        private void EarthReviseMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            earthRevise();

            Cursor = Cursors.Default;

            Invalidate();
        }

        private void trapezoidReviseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            trapezoidRevise();

            Cursor = Cursors.Default;

            Invalidate();
        }

        private void DrawEarthCurveMenuItem_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.draw;
            tempLine = new PLine();
            first = second = new Point(-1,-1);
            UpdateMouseState();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bmp == null) return;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Images File(*.jpg;*.png;*.gif;*.jpeg;*.bmp)|*.jpg;*.png;*.gif;*.jpeg;*.bmp|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    string pathname = dlg.FileName;
                    bmp.Save(pathname);
                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void toolStripButton2ZoomIn_Click(object sender, EventArgs e)
        {

        }
    }
}
