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

namespace DDDSharp
{
    public partial class Drawing2DForm : Form
    {
#pragma warning disable CS0414 // 字段“Drawing2DForm.drawdata”已被赋值，但从未使用过它的值
        DrawData drawdata = new DrawData();
#pragma warning restore CS0414 // 字段“Drawing2DForm.drawdata”已被赋值，但从未使用过它的值

        Stack<DrawData> undos = new Stack<DrawData>();
        Stack<DrawData> redos = new Stack<DrawData>();

        Rectangle DrawRect;
        DoubleRect DataRect;
#pragma warning disable CS0414 // 字段“Drawing2DForm.leftMargin”已被赋值，但从未使用过它的值
        int leftMargin = 10;
#pragma warning restore CS0414 // 字段“Drawing2DForm.leftMargin”已被赋值，但从未使用过它的值
#pragma warning disable CS0414 // 字段“Drawing2DForm.rightMargin”已被赋值，但从未使用过它的值
        int rightMargin = 10;
#pragma warning restore CS0414 // 字段“Drawing2DForm.rightMargin”已被赋值，但从未使用过它的值
#pragma warning disable CS0414 // 字段“Drawing2DForm.topMargin”已被赋值，但从未使用过它的值
        int topMargin = 10;
#pragma warning restore CS0414 // 字段“Drawing2DForm.topMargin”已被赋值，但从未使用过它的值
#pragma warning disable CS0414 // 字段“Drawing2DForm.bottomMargin”已被赋值，但从未使用过它的值
        int bottomMargin = 10;
#pragma warning restore CS0414 // 字段“Drawing2DForm.bottomMargin”已被赋值，但从未使用过它的值

#pragma warning disable CS0414 // 字段“Drawing2DForm.Modified”已被赋值，但从未使用过它的值
        bool Modified = false;
#pragma warning restore CS0414 // 字段“Drawing2DForm.Modified”已被赋值，但从未使用过它的值

        //mouses
        bool bMouseDown = false;
        Point first, second;

        Point selectedPoint = new Point(-1, -1);
#pragma warning disable CS0414 // 字段“Drawing2DForm.selectedPointIndex”已被赋值，但从未使用过它的值
        int selectedPointIndex = -1;
#pragma warning restore CS0414 // 字段“Drawing2DForm.selectedPointIndex”已被赋值，但从未使用过它的值

        ToolTip toolTip1 = new ToolTip();
        Bitmap Bmp = null;

        MouseState mouseState = MouseState.normal;
        bool bBusy = false;

        public Drawing2DForm()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.          

            KeyPreview = true;
            toolTip1.AutoPopDelay = 2000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;

            KeyPreview = true;

            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);

            WindowState = FormWindowState.Maximized;
        }

        private void Drawing2DForm_Paint(object sender, PaintEventArgs e)
        {
            if ( bBusy ) return;

            bBusy = true;

            Graphics g = e.Graphics;

            if ( Bmp != null ) g.DrawImage(Bmp, 0, 0);

            if (bMouseDown && (mouseState == MouseState.zoom_in ||
                                 mouseState == MouseState.normal))
            {
                if (first.X != -1 && second.X != -1)
                {
                    Rectangle rect = new Rectangle(first, new Size((second.X - first.X), (second.Y - first.Y)));
                    g.DrawRectangle(Pens.BlueViolet, rect);
                }
            }
            if (mouseState == MouseState.draw)
            {
                //DrawDynamicLine(g);//绘制动态线
                if (first.X != -1 && second.X != -1)
                {
                    g.DrawLine(Pens.BlueViolet, first, second);
                }
            }
            bBusy = false;

        }

        public void UpdateDraw(bool update = true)
        {
            /*
            if (slicer == null) return;

            int width = pictureBox1.Width;
            int height = pictureBox1.Height;
            if (width < 5 || height < 5) return;

            DrawRect = new Rectangle(leftMargin, topMargin,
                                     width - leftMargin - rightMargin,
                                     height - topMargin - bottomMargin);

            if (update)
            {
                if (Bmp != null) Bmp.Dispose();
                Bmp = new Bitmap(width, height);

                Graphics g = Graphics.FromImage(Bmp);
                DrawOuterRect(g);//绘制边框
                DrawBackImages(g);
                DrawSlicers(g);//轮廓线底图
                DrawTracedObjects(g);//地质体对象LayerObjects
                DrawSampledGrids(g);//采样点
                DrawTracedPoints(g);//定位点
            }

            pictureBox1.Invalidate();
            */
        }

        void LPtoDP(ref double x, ref double y)
        {
            x = DrawRect.Left + DrawRect.Width * (x - DataRect.X1) / DataRect.Width;
            y = DrawRect.Bottom - DrawRect.Height * (y - DataRect.Y1) / DataRect.Height;
        }
        void DPtoLP(ref double x, ref double y)
        {
            x = DataRect.X1 + DataRect.Width * (x - DrawRect.Left) / DrawRect.Width;
            y = DataRect.Y1 + DataRect.Height * (DrawRect.Bottom - y) / DrawRect.Height;
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

    }
}
