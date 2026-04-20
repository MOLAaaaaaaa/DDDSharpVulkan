using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;

namespace DDDSharp
{
    public partial class ScatterPointsResampleForm : Form
    {
        public ScatteredPoints sc = null;
        ScaledWindowRect ScaledRect = null;
        Bitmap bmp;
        int Interval = 1;
        int SampledCount = 0;
        int VisibalCount = 0;
        int VisibalAllCount = 0;
        //mouses
        bool bMouseDown = false;
        Point first, second;
        
        int dotSize = 4;

        MouseState mouseState = MouseState.normal;        
        Cursor cursorArrow;
        Cursor cursorFill;
        Cursor cursorZoomIn;
        Cursor cursorZoomOut;
        Cursor cursorPan;

        bool bBusy = false;
        double XSampleSpace, YSampleSpace;
        bool EnableSplite = false;
        bool IsSpliteInverse = false;
        List<int> SplitedIndices = new List<int>();//切割后数据
        DoubleRect SplitedRect = new DoubleRect();
        bool[] marked = null; //deleted points marked
        bool[] drawingGrids = null;//已绘制网格
        int xgrid = 0;
        int ygrid = 0;
        public ScatterPointsResampleForm(ScatteredPoints _sc)
        {
            sc = _sc;            
            XSampleSpace = YSampleSpace = (sc.maxx - sc.minx)/2000;            
            marked = new bool[sc.points.Count];
            InitializeComponent();            
            
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲

            cursorArrow = Cursors.Arrow;
            cursorFill = Cursors.Cross; //new Cursor(GetType(), "DDDSharp.icos.Fill.cur");
            cursorZoomIn = Cursors.SizeAll;
            cursorZoomOut = Cursors.Hand;
            cursorPan = Cursors.Hand;

            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);
            WindowState = FormWindowState.Maximized;
        }
        void LPtoDP(ref double x, ref double y)
        {
            ScaledRect.LPtoDP(ref x, ref y);
        }
        void DPtoLP(ref double x, ref double y)
        {
            ScaledRect.DPtoLP(ref x, ref y);
        }
        void ResetMouseState()
        {
            first = second = new Point(-1, -1);
            mouseState = MouseState.normal;
            UpdateMouseState();
        }
        public void UpdateMouseState()
        {
            switch (mouseState)
            {
                case MouseState.zoom_in:
                    pictureBox1.Cursor = Cursors.Cross;
                    break;
                case MouseState.zoom_out:
                    pictureBox1.Cursor = cursorZoomOut;
                    break;
                case MouseState.pan:
                    pictureBox1.Cursor = cursorPan;
                    break;
                case MouseState.moving:
                    pictureBox1.Cursor = Cursors.SizeAll;
                    break;
                case MouseState.draw:
                    pictureBox1.Cursor = Cursors.Cross;
                    break;
                case MouseState.normal:
                case MouseState.reset:
                    pictureBox1.Cursor = Cursors.Default;
                    break;
                case MouseState.Fill:
                    pictureBox1.Cursor = cursorFill;
                    break;
            }
        }

        public void UpdateDrawRect(Rectangle drawrect)
        {
            if (sc == null) return;
            if (ScaledRect == null) 
            {
                DoubleRect datarect = new DoubleRect(sc.minx, sc.miny, sc.maxx, sc.maxy);
                ScaledRect = new ScaledWindowRect(datarect, drawrect); 
            }
            else ScaledRect.UpdateDrawRect(drawrect);
        }
        //更新绘制网格
        bool UpdateDrawingGrids(int newdotsize)
        {            
            try 
            {
                drawingGrids = null;
                dotSize = newdotsize;
                xgrid = (int)((double)ScaledRect.DrawRect.Width / dotSize);
                ygrid = (int)((double)ScaledRect.DrawRect.Height / dotSize);
                drawingGrids = new bool[xgrid * ygrid];
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }            
        }
        void SetDrawingGrid(int x,int y,bool filled = true)
        {
            if (drawingGrids == null) return;
            int ix = (x - ScaledRect.DrawRect.Left) / dotSize;
            int iy = (ScaledRect.DrawRect.Bottom - y) / dotSize;
            drawingGrids[ix + iy * xgrid] = filled;
        }
        bool IsHaveDraw(int x,int y)
        {
            if (drawingGrids == null) return true;
            else
            {
                int ix = (x - ScaledRect.DrawRect.Left) / dotSize;
                int iy = (ScaledRect.DrawRect.Bottom - y) / dotSize;
                if (ix >= xgrid || iy >= ygrid )
                {
                    return true;
                }
                else
                {
                    return drawingGrids[ix + iy * xgrid];
                }
            }
        }
        int Splite(Point p1, Point p2)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double x2 = p2.X;
            double y2 = p2.Y;
            DPtoLP(ref x1, ref y1);
            DPtoLP(ref x2, ref y2);
            DoubleRect rect = new DoubleRect();
            rect.X1 = Math.Min(x1, x2);
            rect.X2 = Math.Max(x1, x2);
            rect.Y1 = Math.Min(y1, y2);
            rect.Y2 = Math.Max(y1, y2);
            return Splite(rect);
        }
        int Splite(DoubleRect rect)
        {
            SplitedRect = rect;
            SplitedIndices.Clear();
            return 1;
            Vector32 p;
            for (int i = 0; i < sc.points.Count; i++)
            {
                if (!marked[i])
                {
                    p = sc.points[i];
                    if (rect.Contains(p.x, p.y))
                    {
                        SplitedIndices.Add(i);
                    }
                }
            }
            return SplitedIndices.Count;
        }
        int Sample(Point p1,Point p2, double xx, double yy)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double x2 = p2.X;
            double y2 = p2.Y;
            DPtoLP(ref x1, ref y1);
            DPtoLP(ref x2, ref y2);
            DoubleRect rect = new DoubleRect();
            rect.X1 = Math.Min(x1, x2);
            rect.X2 = Math.Max(x1, x2);
            rect.Y1 = Math.Min(y1, y2);
            rect.Y2 = Math.Max(y1, y2);
            return Sample(rect, xx, yy);
        }
        /// <summary>
        /// 按照网格间距进行重采样
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <returns></returns>
        int Sample(DoubleRect rect,double xx, double yy)
        {
            List<int> indices = new List<int>();
            Vector32 p;            
            for (int i = 0; i < sc.points.Count; i++)
            {
                if ( !marked[i] )
                {
                    p = sc.points[i];
                    if ( rect.Contains(p.x, p.y) ) 
                    {
                        indices.Add(i);
                    }
                }
            } 

            if (indices.Count < 4) return 0;

            return Sample(rect,indices, xx, yy);
        }
        struct gridPoints
        {
            public List<int> indices;
            public void Add(int id)
            {
                indices.Add(id);
            }
            public gridPoints(bool init)
            {
                indices = new List<int>();
            }
            public void Clear()
            {
                indices.Clear();
            }
            //统计Z众数
            public void GetZRange(List<Vector32> points, out double minz, out double maxz)
            {
                minz = maxz = 0;
                Vector32 p;
                for(int i=0;i<indices.Count;i++)
                {
                    p = points[indices[i]];
                    if (i == 0) minz = maxz = p.z;
                    else
                    {
                        if (p.z < minz) minz = p.z;
                        if (p.z > maxz) maxz = p.z;
                    }
                }
            }
            /// <summary>
            /// 对网格内数据进行采样,保留最高的Z值点
            /// </summary>
            /// <param name="x0"></param>
            /// <param name="y0"></param>
            /// <param name="marked"></param>
            /// <param name="points"></param>
            /// <returns></returns>
            public int Sample(double x0, double y0, bool[] marked, List<Vector32> points)
            {
                double minz = 0, maxz = 0;
                //GetZRange(points, out minz, out maxz);
                Vector32 p;                
                int keepid = -1;
                for (int i = 0; i < indices.Count; i++)
                {
                    p = points[indices[i]];
                    marked[indices[i]] = true;//删除标记
                    if( i== 0) 
                    {
                        maxz = p.z;                        
                        keepid = indices[i];
                    }
                    else
                    {
                        if (p.z > maxz)
                        {
                            maxz = p.z;
                            keepid = indices[i];
                        }
                    }                    
                }
                //网格内最高值点
                if (keepid > -1) 
                { 
                    marked[keepid] = false; 
                }
                return 1;
            }
        }
        /// <summary>
        /// 对网格内数据进行采样
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="indices"></param>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <returns></returns>
        int Sample(DoubleRect rect,List<int>indices,double xx, double yy)
        {
            //创建网格
            int nx = (int)(rect.Width / xx ) + 1;
            int ny = (int)(rect.Height / yy) + 1;
            
            gridPoints[] grids = new gridPoints[nx*ny];
            for(int i=0;i<grids.Length;i++)
            {
                grids[i] = new gridPoints(true);
            }

            //数据点分配到网格
            Vector32 p;
            int ix, iy;
            for(int i = 0; i < indices.Count; i++ )
            {
                p = sc.points[indices[i]];
                ix = (int)( (p.x - rect.X1) / xx );
                iy = (int)( (p.y - rect.Y1) / yy );
                grids[ix + iy * nx].Add(indices[i]);
            }

            double x0, y0; //网格中心
            for(iy = 0;iy<ny;iy++)
            {
                for(ix = 0;ix<nx;ix++)
                {
                    x0 = rect.X1 + ix * xx + xx / 2;
                    y0 = rect.Y1 + iy * yy + yy / 2;
                    grids[ix + iy * nx].Sample(x0,y0,marked,sc.points);
                }
            }
            
            grids = null;

            return 1;
        }
        int CountVisiblePoints(DoubleRect rect)
        {
            int count = 0;
            SampledCount = 0;
            Vector32 p;
            for( int i=0;i<sc.points.Count;i++)
            {
                if (!marked[i])
                {
                    p = sc.points[i];
                    if (rect.Contains(p.x, p.y)) count++;
                    SampledCount++;
                }
            }
            return count;
        }

        void UpdateDraw(bool update = true)
        {            
            if (update)
            {
                if (sc == null) return;
                if (ScaledRect.DrawRect.Width < 10 || 
                    ScaledRect.DrawRect.Height < 10)return;

                if (bmp != null) bmp.Dispose();

                Cursor = Cursors.WaitCursor;

                bmp = new Bitmap(ScaledRect.DrawRect.Width, ScaledRect.DrawRect.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.FillRectangle(Brushes.White, ScaledRect.DrawRect);                
                
                double x = 0, y = 0;
                Color c;
                Vector32 p;
                
                int n = CountVisiblePoints(ScaledRect.DataRect);
                /*
                int nx = pictureBox1.Width / dotSize + 1;
                int ny = pictureBox1.Height / dotSize + 1;
                Interval = n / nx / ny + 1;
                */
                try
                {
                    Array.Clear(drawingGrids, 0, drawingGrids.Length);
                    VisibalCount = 0;
                    VisibalAllCount = 0;
                    for (int i = 0; i < n; i++ )
                    {
                        p = sc.points[i];
                        x = p.x;
                        y = p.y;
                        
                        //不在绘制区域
                        if ( !ScaledRect.DataRect.Contains(x, y) ) continue;
                        VisibalAllCount++;

                        LPtoDP(ref x, ref y);

                        //该位置已绘制
                        if ( IsHaveDraw( (int)x, (int)y ) ) continue;

                        c = sc.GetColor(p.z);
                        if (marked[i]) c = Color.Gray;
                        g.FillRectangle(new SolidBrush(c), (float)x, (float)y, dotSize, dotSize);

                        //g.DrawRectangle(Pens.Gray, (float)x, (float)y, size, size);

                        SetDrawingGrid((int)x, (int)y);
                        VisibalCount++;
                    }
                    
                    Cursor = Cursors.Default;

                    pictureBox1.Invalidate();
                }
                catch(Exception e)
                {
                    MessageBox.Show("erro occurred while draw the points." + e.Message);
                }                
            }
            else
            {
                pictureBox1.Invalidate();
            }            
        }
        private void ScatterPointsResampleForm_Load(object sender, EventArgs e)
        {
            UpdateDraw(true);
        }        

        void ZoomRect(Point p1, Point p2)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double x2 = p2.X;
            double y2 = p2.Y;

            //中心点放大
            if (Math.Abs(x2 - x1) < 5 || Math.Abs(y2 - y1) < 5)
            {
                ZoomIn(new Point((int)((x1 + x1) / 2.0), (int)((y1 + y1) / 2.0)));
                return;
            }

            DPtoLP(ref x1, ref y1);
            DPtoLP(ref x2, ref y2);
            
            //选择数据中心点
            double x0 = (x1 + x2) / 2.0;
            double y0 = (y1 + y2) / 2.0;
            
            ScaledRect.ZoomRect(x0, y0, Math.Abs(x2 - x1), Math.Abs(y2 - y1));
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

            ScaledRect.Zoom(x0, y0, scale);

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

        private void toolStripButtonZoomIn_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.zoom_in;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }

        private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.zoom_out;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }

        private void toolStripButtonArrow_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.normal;
            UpdateMouseState();
        }

        private void toolStripButtonPan_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.pan;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }
        //对数据进行采样
        private void toolStripButtonSample_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.Fill;
            first = second = new Point(-1, -1);
            UpdateMouseState();            
        }
        private void toolStripButtonReset_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.normal;
            first = second = new Point(-1, -1);
            bMouseDown = false;
            Rectangle drawrect = new Rectangle(10, 10, pictureBox1.Width - 20, pictureBox1.Height - 20);
            UpdateDrawRect(drawrect);
            UpdateDrawingGrids(dotSize);
            UpdateMouseState();
            UpdateDraw(true);
        }      
     
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            bBusy = true;

            Graphics g = e.Graphics;
            Rectangle drawrect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
            if (bmp != null) g.DrawImage(bmp, ScaledRect.DrawRect);

            if (bMouseDown && (mouseState == MouseState.zoom_in ||
                                mouseState == MouseState.Fill ||
                                mouseState == MouseState.normal))
            {
                if (first.X != -1 && second.X != -1)
                {
                    Rectangle rect = new Rectangle(first, new Size((second.X - first.X), (second.Y - first.Y)));
                    g.DrawRectangle(Pens.BlueViolet, rect);
                }
            }
            bBusy = false;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            bMouseDown = true;
            first = new Point(e.X, e.Y);
            second = new Point(-1, -1);

            if (e.Button == MouseButtons.Left)
            {
                if (mouseState == MouseState.normal)
                {

                }
                else if (mouseState == MouseState.zoom_out)
                {
                    ZoomOut(first);
                    UpdateDraw();
                }
                else if (mouseState == MouseState.draw)
                {
                    first = new Point(e.X, e.Y);
                }
            }//if (e.Button == MouseButtons.Left)

            else if (e.Button == MouseButtons.Right)
            {
                bMouseDown = false;
                first = second = new Point(-1, -1);

                if (mouseState == MouseState.normal)
                {
                }
                else if (mouseState == MouseState.draw) //end of draw
                {
                    first = second = new Point(-1, -1);
                    mouseState = MouseState.normal;
                    UpdateDraw();
                }
                else
                {
                    ResetMouseState();
                }
            }//else if (e.Button == MouseButtons.Right)
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (ScaledRect != null)
            {
                double x = e.X;
                double y = e.Y;
                DPtoLP(ref x, ref y);
                CoordinateLable.Text = "(" + Math.Round(x, 4) + " , " + Math.Round(y, 4) + ")";                
                CoordinateLable.Text += "(Visible = " + VisibalCount + "/" + VisibalAllCount + ")";
                CoordinateLable.Text += "(Sampled = " + SampledCount + "/" + sc.points.Count +  ")";                
            }
            if ( bMouseDown && (mouseState == MouseState.normal || 
                  mouseState == MouseState.zoom_in ||
                  mouseState == MouseState.Fill ) )
            {
                second = new Point(e.X, e.Y);
                pictureBox1.Invalidate();
            }            
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseState == MouseState.normal && bMouseDown)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (mouseState == MouseState.normal && bMouseDown)
                    {
                    }
                }
            }//if ( mouseState == MouseState.normal && bMouseDown)

            if (mouseState == MouseState.zoom_in && bMouseDown)
            {
                second = new Point(e.X, e.Y);
                ZoomRect(first, second);
                UpdateDraw();
                pictureBox1.Invalidate();
            }

            if (mouseState == MouseState.Fill && bMouseDown)
            {
                second = new Point(e.X, e.Y);
                if (EnableSplite) //切割数据
                {
                    Splite(first, second);
                }
                else 
                { 
                    Sample(first, second, XSampleSpace, YSampleSpace); 
                }
                UpdateDraw();
                pictureBox1.Invalidate();
            }

            bMouseDown = false;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Rectangle drawrect = new Rectangle(10, 10, pictureBox1.Width - 20, pictureBox1.Height - 20);
            UpdateDrawRect(drawrect);
            UpdateDrawingGrids(dotSize);
            UpdateDraw(true);
        }
        bool Export(string path)
        {
            try
            {
                string line = "";
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);
                string name = Path.GetFileName(path);
                if (sc.Demension == 1) line = "X";
                else if (sc.Demension == 2) line = "X,Y";
                else if (sc.Demension == 3) line = "X,Y,Z";
                else if (sc.Demension == 4) line = "X,Y,Z,Value";
                wr.WriteLine(line);

                Vector32 p;
                for (int i = 0; i < sc.points.Count; i++)
                {
                    if (marked[i]) continue;
                    p = sc.points[i];
                    if ( EnableSplite )
                    {
                        if ( IsSpliteInverse )
                        { 
                            if ( SplitedRect.Contains(p.x, p.y) )
                                continue; 
                        }
                        else
                        {
                            if ( !SplitedRect.Contains(p.x, p.y) )
                                continue;
                        }
                    }                    
                    if (sc.Demension == 1) line = p.x.ToString();
                    else if (sc.Demension == 2) line = p.x + "," + p.y;
                    else if (sc.Demension == 3) line = p.x + "," + p.y + "," + p.z;
                    else if (sc.Demension == 4) line = p.x + "," + p.y + "," + p.z + "," + p.v;
                    wr.WriteLine(line);
                }

                wr.Close();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sc == null) return;
            string filter = " Points(*.csv)|*.csv|ACSII file (*.dat)|*.dat|all files(*.*)|*.*";
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string pathname = dlg.FileName;

                    this.Cursor = Cursors.WaitCursor;

                    if (Export(pathname))
                        MessageBox.Show((AppLocalization.IsChinese ? "数据已导出到文件：" : "Data exported to file: ") + pathname);
                    else
                        MessageBox.Show((AppLocalization.IsChinese ? "导出数据到文件失败：" : "Failed to export data to file: ") + pathname);

                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void gridSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sc == null) return;
            SampleGridSetForm sg = new SampleGridSetForm();
            sg.xstep = XSampleSpace;
            sg.ystep = YSampleSpace;
            sg.EnableSplite = EnableSplite;
            sg.IsSpliteInverse = IsSpliteInverse;
            sg.dotSize = dotSize;
            if( sg.ShowDialog() == DialogResult.OK)
            {
                XSampleSpace = sg.xstep;
                YSampleSpace = sg.ystep;
                EnableSplite = sg.EnableSplite;
                IsSpliteInverse = sg.IsSpliteInverse;
                if( sg.dotSize != dotSize )
                {
                    UpdateDrawingGrids(sg.dotSize);
                }                
            }
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
                UpdateDraw(true);
            }
        }
    }
}
