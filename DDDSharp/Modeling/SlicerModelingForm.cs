using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
using FloodFill2;
using DDDSharp.Modeling;

namespace DDDSharp
{
    public enum MouseState
    {
        normal = 0,//arrow
        zoom_in = 1,
        zoom_out = 2,
        pan = 3, //hand
        reset = 4,
        locate = 5,
        draw = 15,
        moving = 20,
        Fill = 50,
        Test = 100,
    }

    public struct DrawData //数据结构，用于保存当前绘图状态数据
    {
        public PolygonSlicer slicer;
        public Point first;
        public Point second;        
        public Point selectedPoint;
        public int selectedPointIndex;
        public List<Point> SelectedArray;
        public bool bShowGridPoint;
        public bool bSmoothLine;
        public bool bShowLocation;
        public bool bShowOutlines;
        public bool bShowLayers;
        public bool bShowSampledGrids;
        public DrawData Copy()
        {
            DrawData p = new DrawData();
            p.slicer = slicer.Copy();
            p.first = first;
            p.second = second;
            p.selectedPoint = selectedPoint;
            p.selectedPointIndex = selectedPointIndex;
            p.SelectedArray = new List<Point>();
            foreach (Point p1 in SelectedArray) p.SelectedArray.Add(p1);
            
            p.bShowGridPoint = bShowGridPoint;
            p.bSmoothLine = bSmoothLine;
            p.bShowLocation = bShowLocation;
            p.bShowOutlines = bShowOutlines;
            p.bShowLayers = bShowLayers;
            p.bShowSampledGrids = bShowSampledGrids;
            return p;
        }
    }
    

    public partial class SlicerModelingForm : Form
    {
        public PolygonSlicer slicer = null; //多边形切片
        
        DrawData drawdata = new DrawData();

        Stack<DrawData> undos = new Stack<DrawData>();
        Stack<DrawData> redos = new Stack<DrawData>();

        Rectangle DrawRect;
        DoubleRect DataRect;
        int leftMargin = 10;
        int rightMargin = 10;
        int topMargin = 10;
        int bottomMargin = 10;

        //mouses
        bool bMouseDown = false;
        Point first = new Point(-1, -1);
        Point second = new Point(-1, -1);

        Point selectedPoint = new Point(-1, -1);
        int selectedPointIndex = -1;

        ToolTip toolTip1 = new ToolTip();
        Bitmap Bmp = null;

        MouseState mouseState = MouseState.normal;

        Cursor cursorArrow;
        Cursor cursorFill;
        Cursor cursorZoomIn;
        Cursor cursorZoomOut;
        Cursor cursorPan;

        List<Point> SelectedArray = new List<Point>();
        bool bBusy = false;
        bool IsControlDown = false;

        bool bShowGridPoint = false;
        bool bSmoothLine = false;
        bool bShowLocation = true;
        bool bShowOutlines = true;
        bool bShowBackgroundImages = true;
        bool bShowLayers = true;
        bool bShowLayerFault = true;//线
        bool bShowLayerPolygon = true;//多边形
        bool bShowSampledGrids = false;    //显示剖分后的网格分布

        bool bEnableSnap = false; //网格捕捉

        float sampledGridSize = 12;

        float dotSize = 10;
        int drawstate = 1;//1 slicer 2 geo 3 locate

        Polygon2D drawslicer = null;
        Polygon2D drawlayer = null;

        //Flood Fill
        protected AbstractFloodFiller floodFiller;

        public bool Modified = false;
        TreeNode treeNode1;
        TreeNode treeNode2;
        
        string OutlinesNode = "OutLines";
        string GeoObjectsNode = "GeoObjects";
                
        protected enum ObectType
        {
            OutLine = 0, //图形对象（辅助)
            GeoObject = 1,//地质对象(建模)
        }

        public SlicerModelingForm()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.          
            KeyPreview = true;
            toolTip1.AutoPopDelay = 2000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;

            cursorArrow = Cursors.Arrow;
            cursorFill = Cursors.Cross; //new Cursor(GetType(), "DDDSharp.icos.Fill.cur");
            cursorZoomIn = Cursors.SizeAll;
            cursorZoomOut = Cursors.Hand;
            cursorPan = Cursors.Hand;

            KeyPreview = true;

            floodFiller = new QueueLinearFloodFiller(floodFiller);

            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);

            WindowState = FormWindowState.Maximized;

        }

        void ResetRect()
        {
            DrawRect = new Rectangle(leftMargin, topMargin,
                                     pictureBox1.Width - leftMargin - rightMargin,
                                     pictureBox1.Height - topMargin - bottomMargin);
            if (slicer == null) return;

            double x1 = slicer.minx;
            double x2 = slicer.maxx;
            double y1 = slicer.miny;
            double y2 = slicer.maxy;
            double x0 = (x1 + x2) / 2.0;
            double y0 = (y1 + y2) / 2.0;

            double width = (y2 - y1) * DrawRect.Width / DrawRect.Height;
            double height = (x2 - x1) * DrawRect.Height / DrawRect.Width;
            double scale = (y2 - y1) / (x2 - x1);
            if (scale * DrawRect.Width < DrawRect.Height)
            {
                DataRect.x1 = x1;
                DataRect.x2 = x2;
                DataRect.y1 = y0 - height / 2.0;
                DataRect.y2 = y0 + height / 2.0;
            }
            else
            {
                DataRect.y1 = y1;
                DataRect.y2 = y2;
                DataRect.x1 = x0 - width / 2.0;
                DataRect.x2 = x0 + width / 2.0;
            }
        }

        public void SetSlicer(PolygonSlicer poly)
        {
            slicer = poly.Copy();
            ResetRect();
            UpdateTree();
        }
        /// <summary>
        /// 拷贝当前绘图数据
        /// </summary>
        /// <returns></returns>
        private DrawData CopyDrawData()
        {
            DrawData p = new DrawData();

            p.slicer = slicer.Copy();
            p.first = first;
            p.second = second;
            p.selectedPoint = selectedPoint;
            p.selectedPointIndex = selectedPointIndex;

            p.SelectedArray = new List<Point>();
            foreach (Point v in SelectedArray) p.SelectedArray.Add(v);

            p.bShowGridPoint = bShowGridPoint;
            p.bSmoothLine = bSmoothLine;
            p.bShowLocation = bShowLocation;
            p.bShowOutlines = bShowOutlines;
            p.bShowLayers = bShowLayers;
            p.bShowSampledGrids = bShowSampledGrids;
            return p;
        }
        private DrawData CopyDrawData(DrawData data)
        {
            DrawData p = new DrawData();

            p.slicer = data.slicer.Copy();
            p.first = data.first;
            p.second = data.second;
            p.selectedPoint = data.selectedPoint;
            p.selectedPointIndex = data.selectedPointIndex;

            p.SelectedArray = new List<Point>();
            foreach (Point v in data.SelectedArray) p.SelectedArray.Add(v);

            p.bShowGridPoint = data.bShowGridPoint;
            p.bSmoothLine = data.bSmoothLine;
            p.bShowLocation = data.bShowLocation;
            p.bShowOutlines = data.bShowOutlines;
            p.bShowLayers = data.bShowLayers;
            p.bShowSampledGrids = data.bShowSampledGrids;
            return p;
        }
        /// <summary>
        /// 移除栈底元素
        /// </summary>
        /// <param name="count">移除个数</param>
        void RemoveStackBottomItems(Stack<DrawData> stack1, int count)
        {
            //undos : 0 1 2 3 4 5 ...20
            //tempstack:20,19,18,...,10
            Stack<DrawData> tempstack = new Stack<DrawData>();
            while (stack1.Count >= count + 1) //丢弃前10个
            {
                tempstack.Push(stack1.Pop());
            }
            stack1.Clear();
            while (tempstack.Count > 0)
            {
                stack1.Push(tempstack.Pop());
            }
        }
        //保存当前绘图数据
        public void PushData()
        {
            if (undos.Count >= 20)//已到最大栈，丢弃栈底元素
            {
                RemoveStackBottomItems(undos, 10);
            }
            undos.Push(CopyDrawData());
        }
        public void PushData(DrawData p)
        {
            undos.Push(p);
        }
        public bool PeekDrawData()
        {
            if (undos.Count < 1) return false;

            DrawData p = undos.Peek();

            slicer = p.slicer.Copy();
            first = p.first;
            second = p.second;
            selectedPoint = p.selectedPoint;
            selectedPointIndex = p.selectedPointIndex;
            SelectedArray = p.SelectedArray;
            bShowGridPoint = p.bShowGridPoint;
            bSmoothLine = p.bSmoothLine;
            bShowLocation = p.bShowLocation;
            bShowOutlines = p.bShowOutlines;
            bShowLayers = p.bShowLayers;
            bShowSampledGrids = p.bShowSampledGrids;

            return true;
        }

        public bool UndoData()
        {
            //栈底需保留一份数据，作为最初数据绘图使用
            if (undos.Count <= 1) return false;

            redos.Push(undos.Pop().Copy());

            PeekDrawData();
            return true;
        }

        //弹出当前绘图数据
        public bool RedoData()
        {
            if (redos.Count < 1) return false;
            if (redos.Count >= 20)//已到最大栈，丢弃栈底元素
            {
                RemoveStackBottomItems(redos, 10);
            }

            undos.Push(redos.Pop().Copy());

            PeekDrawData();

            return true;
        }

        void ClearSelected()
        {
            SelectedArray.Clear();
            selectedPoint = new Point(-1, -1);
            selectedPointIndex = -1;
        }
        void DeleteObject(int type, int index)
        {
            if (type == 1)
            {
                slicer.polygons.RemoveAt(index);
                for (int j = index + 1; j < treeNode1.Nodes.Count; j++)
                {
                    treeNode1.Nodes[j].Tag = (int)(treeNode1.Nodes[j].Tag) - 1;
                }
                treeNode1.Nodes.Remove(treeNode1.Nodes[index]);// (index);
            }
            else if (type == 2)
            {
                slicer.tracedGeoObjects.RemoveAt(index);
                for (int j = index + 1; j < treeNode2.Nodes.Count; j++)
                {
                    treeNode2.Nodes[j].Tag = (int)(treeNode2.Nodes[j].Tag) - 1;
                }
                treeNode2.Nodes.Remove(treeNode2.Nodes[index]);
            }
            else if (type == 3)
            {
                slicer.Locations2D.Clear();
                slicer.Locations3D.Clear();
            }
            else if (type == 4)
            {
                slicer.sampledGrids.RemoveAt(index);
            }

            Modified = true;
        }
        void DeleteSelected()
        {
            List<int> select1 = new List<int>();
            List<int> select2 = new List<int>();
            List<int> select4 = new List<int>();

            foreach (Point p in SelectedArray)
            {
                if (p.X == 1) select1.Add(p.Y);
                if (p.X == 2) select2.Add(p.Y);
                if (p.X == 3) slicer.Locations2D.Clear();
                if (p.X == 4) select4.Add(p.Y);
            }

            ////////////////////////////////////
            select1.Sort();
            select2.Sort();
            select4.Sort();
            ////////////////////////////////////////                
            for (int i = select1.Count - 1; i >= 0; i--)
            {
                DeleteObject(1, select1[i]);
            }
            for (int i = select2.Count - 1; i >= 0; i--)
            {
                DeleteObject(2, select2[i]);
            }
            for (int i = select4.Count - 1; i >= 0; i--)
            {
                DeleteObject(4, select4[i]);
            }

            select1.Clear();
            select2.Clear();
            select4.Clear();
            Modified = true;
            ClearSelected();

        }
        void AddSelected(int type, int index)
        {
            SelectedArray.Add(new Point(type, index));
        }
        bool IsInSelected(int type, int index)
        {
            foreach (Point p in SelectedArray)
            {
                if (p.X == type && p.Y == index)
                    return true;
            }
            return false;
        }
        bool IsInSelected(int type, int index, int gridindex)
        {
            if (selectedPoint.X == type &&
                selectedPoint.Y == index &&
                selectedPointIndex == gridindex)
                return true;
            return false;
        }

        void AddTreeItem1(string name, int id)
        {
            TreeNode node = new TreeNode();
            node.Text = name;
            node.Name = name;
            node.Tag = id;
            treeNode1.Nodes.Add(node);
        }
        void AddTreeItem2(string name, int id)
        {
            TreeNode node = new TreeNode();
            node.Text = name;
            node.Name = name;
            node.Tag = id;
            treeNode2.Nodes.Add(node);
        }
        public void UpdateTree()
        {
            if (slicer == null) return;

            this.Cursor = Cursors.WaitCursor;

            treeView1.Nodes.Clear();

            string name = "";
            string type = "";
            Polygon2D poly;

            treeNode1 = new TreeNode();
            treeNode1.Name = OutlinesNode;
            treeNode1.Text = OutlinesNode;
            treeNode1.Tag = 1;
            for (int i = 0; i < slicer.polygons.Count; i++)
            {
                poly = slicer.polygons[i];
                name = poly.Name;
                type = ObectType.OutLine.ToString();

                TreeNode node = new TreeNode(name);
                node.Text = name;
                node.Tag = i;
                treeNode1.Nodes.Add(node);
            }
            treeView1.Nodes.Add(treeNode1);

            treeNode2 = new TreeNode();
            treeNode2.Name = GeoObjectsNode;
            treeNode2.Text = GeoObjectsNode;
            treeNode2.Tag = 2;
            Polygon2D obj;
            for (int i = 0; i < slicer.tracedGeoObjects.Count; i++)
            {
                obj = slicer.tracedGeoObjects[i];
                name = obj.Name;
                TreeNode node = new TreeNode(name);
                node.Text = name;
                node.Tag = i;
                treeNode2.Nodes.Add(node);
            }
            treeView1.Nodes.Add(treeNode2);

            propertyGrid1.SelectedObject = null;

            this.Cursor = Cursors.Default;
        }
        PointF[] toDPPoints(List<Vector32> points)
        {
            int n = points.Count;
            if (n < 1) return null;
            PointF[] dps = new PointF[n];
            double x, y;
            Vector32 p;
            for (int i = 0; i < n; i++)
            {
                p = points[i];
                x = p.x;
                y = p.y;
                LPtoDP(ref x, ref y);
                dps[i] = new PointF((float)x, (float)y);
            }
            return dps;
        }
        PointF[] toDPPoints(List<Vector64> points)
        {
            int n = points.Count;
            if (n < 1) return null;
            PointF[] dps = new PointF[n];
            double x, y;
            Vector64 p;
            for (int i = 0; i < n; i++)
            {
                p = points[i];
                x = p.x;
                y = p.y;
                LPtoDP(ref x, ref y);
                dps[i] = new PointF((float)x, (float)y);
            }
            return dps;
        }

        void DrawPolygons(Graphics g, List<Polygon2D> polys, bool isclosed)
        {
            Polygon2D obj;

            float tension = 0.5f;
            if (!bSmoothLine) tension = 0;

            for (int i = 0; i < polys.Count; i++)
            {
                obj = polys[i];
                if (!obj.IsValid) continue;
                if (obj.IsClosed != isclosed) continue;

                //不在显示范围内
                if (!DataRect.IsIntersectWith(obj.minx, obj.miny, obj.maxx, obj.maxy))
                    continue;

                Pen pen;
                if (IsInSelected(2, i))
                {
                    pen = new Pen(Color.Red);
                    pen.Width = obj.lineWidth;
                    if (pen.Width < 2) pen.Width = 2;
                    pen.DashStyle = DashStyle.Solid;
                }
                else
                {
                    pen = new Pen(obj.lineColor);
                    pen.Width = obj.lineWidth;
                    pen.DashStyle = obj.dashStyle;
                }

                PointF[] points = toDPPoints(obj.points);
                if (obj.IsClosed && obj.IsFill)
                {
                    Brush brush = new SolidBrush(obj.fillColor);
                    g.FillClosedCurve(brush, points, FillMode.Alternate, tension);
                    g.DrawClosedCurve(pen, points, tension, FillMode.Alternate);
                    brush.Dispose();
                }
                else 
                {
                    //SplineCurve line = new SplineCurve();
                    //for (int k = 0; k < points.Length; k++)
                    //{
                       PointF[]pp = SplineCurve.FetchPoints(points).ToArray();
                    //}
                    //line.Draw(g, Pens.Blue);
                    //g.DrawCurve(pen, points, tension); 
                    g.DrawLines(pen, pp);
                }

                //绘制对象节点
                if (IsInSelected(2, i) && bShowGridPoint)
                {
                    Rectangle dotRect = new Rectangle();
                    pen.Width = 1;
                    int id = 0;
                    foreach (PointF p in points)
                    {
                        dotRect.Location = new Point((int)(p.X - dotSize / 2), (int)(p.Y - dotSize / 2));
                        dotRect.Width = (int)dotSize;
                        dotRect.Height = (int)dotSize;
                        if (IsInSelected(2, i, id))
                        {
                            Pen selpointPen = new Pen(Color.BlueViolet);
                            selpointPen.Width = 2;
                            g.DrawRectangle(selpointPen, dotRect);
                            selpointPen.Dispose();
                        }
                        else g.DrawRectangle(pen, dotRect);
                        id++;
                    }
                }

                pen.Dispose();
                points = null;
            }
        }

        void DrawTracedObjects(Graphics g)
        {
            if (!bShowLayers) return;
            if (slicer == null) return;
            if (slicer.tracedGeoObjects.Count < 1) return;

            //显示多边形内部
            if (bShowLayerPolygon) DrawPolygons(g, slicer.tracedGeoObjects.Polygons, true);
            //显示多边形边界
            if (bShowLayerFault) DrawPolygons(g, slicer.tracedGeoObjects.Polygons, false);
        }
        void DrawSampledGrids(Graphics g)
        {
            if (!bShowSampledGrids) return;
            if (slicer == null) return;
            if (slicer.sampledGrids.Count < 1) return;

            double x, y, z, v;
            Pen pen;
            Brush selbrush = new SolidBrush(Color.Red);

            Color color;
            Vector64 p;
            Rectangle rect = new Rectangle();
            for (int i = 0; i < slicer.sampledGrids.Count; i++)
            {
                p = slicer.sampledGrids[i];
                x = p.x;
                y = p.y;
                z = p.z;
                v = p.v;
                //if (z < 0) continue;

                LPtoDP(ref x, ref y);

                rect.Location = new Point((int)(x - sampledGridSize / 2), (int)(y - sampledGridSize / 2));
                rect.Width = (int)sampledGridSize;
                rect.Height = (int)sampledGridSize;

                if (IsInSelected(4, i))
                {
                    g.FillRectangle(selbrush, rect);                    
                }
                else
                {
                    if (z < 0) color = Color.Gray;
                    else color = slicer.tracedGeoObjects[(int)z].fillColor;
                    pen = new Pen(color);
                    //g.FillRectangle(new SolidBrush(color),rect);
                    g.DrawRectangle(pen, rect);
                    pen.Dispose();
                }

            }//foreach (Vector64 p in slicer.sampledGrids)
        }

        void DrawTracedPoints(Graphics g)
        {
            if (!bShowLocation) return;

            if (slicer == null) return;
            if (!slicer.IsLocated) return;

            PointF[] points = toDPPoints(slicer.Locations2D);

            Pen pen1, pen2;
            pen1 = new Pen(Color.BurlyWood);
            pen1.Width = 2;
            pen2 = new Pen(Color.Blue);
            pen2.Width = 1;

            int size1 = 4;
            int size2 = 10;
            int k = 0;
            PointF p1, p2;
            foreach (PointF p in points)
            {
                p1 = p2 = p;
                p1.X -= size1;
                p2.X += size1;
                g.DrawLine(pen1, p1, p2);

                p1 = p2 = p;
                p1.Y -= size1;
                p2.Y += size1;
                g.DrawLine(pen1, p1, p2);

                if (IsInSelected(3, 0, k))
                {
                    pen2.Color = Color.Red;
                    pen2.Width = 2;
                }
                else
                {
                    pen2.Color = Color.Blue;
                    pen2.Width = 1;
                }

                g.DrawEllipse(pen2, p.X - size2, p1.Y - size2, size2 * 2, size2 * 2);

            }
            points = null;

            pen1.Dispose();
            pen2.Dispose();
        }
        void DrawOuterRect(Graphics g)
        {
            g.Clear(Color.White);
            g.DrawRectangle(Pens.Gray, DrawRect);

            if (slicer != null)//绘制目标对象的范围，
            {
                double x1, y1, x2, y2;
                x1 = slicer.minx;
                x2 = slicer.maxx;
                y1 = slicer.miny;
                y2 = slicer.maxy;
                LPtoDP(ref x1, ref y1);
                LPtoDP(ref x2, ref y2);
                double mx1 = x1;
                if (mx1 > x2) mx1 = x2;
                double my1 = y1;
                if (my1 > y2) my1 = y2;
                Rectangle rect = new Rectangle((int)mx1, (int)my1, (int)Math.Abs(x1 - x2), (int)Math.Abs(y1 - y2));
                Color color = Color.FromArgb(253, 255, 238);
                //Color color = Color.White;
                g.FillRectangle(new SolidBrush(color), rect);
            }
        }
       
        void DrawBackImages(Graphics g)
        {
            if ( !bShowBackgroundImages ) return;
            double x1, y1,x2,y2;
            double width, height;
            double scale0 = 1;
            Rectangle sourceRect, destRect;
            foreach(ImageStruct im in slicer.backImages )
            {
                //图像RECT范围
                x1 = im.rect.x1;
                x2 = im.rect.x2;
                y1 = im.rect.y1;
                y2 = im.rect.y2;
                
                scale0 = Math.Abs(x2 - x1)/ Math.Abs(y2 - y1); 

                LPtoDP(ref x1, ref y1);
                LPtoDP(ref x2, ref y2);

                height = Math.Abs(y2 - y1);
                width = Math.Abs(x2 - x1);//scale0 * height;
                
                if (x1 > x2) x1 = x2;
                if (y1 > y2) y1 = y2;

                /*
                imRect = new Rectangle((int)x1, (int)y1, width, height);
                destRect = imRect;
                destRect.Intersect(DrawRect);//裁剪后的区域

                //计算图像中的位置
                double xs1 = 0,ys1=0,xs2 = 1,ys2 = 1;
                if (destRect.Left > imRect.Left) xs1 = (double)(destRect.Left - imRect.Left) / (double)imRect.Width;
                if (destRect.Top > imRect.Top) ys1 = (double)(destRect.Top - imRect.Top) / (double)imRect.Height;
                if (destRect.Right < imRect.Right) xs2 = (double)(destRect.Right - imRect.Left) / (double)imRect.Width;
                if (destRect.Bottom > imRect.Bottom) ys2 = (double)(destRect.Bottom - imRect.Top) / (double)imRect.Height;

                x1 =  xs1 * im.img.Width;
                y1 =  ys1 * im.img.Height;
                x2 =  xs2 * im.img.Width;
                y2 =  ys2 * im.img.Height;

                width = (int)Math.Abs(x2 - x1);
                height = (int)Math.Abs(y2 - y1);
                if (x1 > x2) x1 = x2;
                if (y1 > y2) y1 = y2;
                sourceRect = new Rectangle((int)x1, (int)y1,width,height);
                //g.FillRectangle(new SolidBrush(Color.Blue), destRect);
                g.DrawImage(im.img, destRect, sourceRect, GraphicsUnit.Pixel);
                */
                destRect = new Rectangle((int)x1, (int)y1, (int)width, (int)height);
                sourceRect = new Rectangle(0,0,im.img.Width,im.img.Height);

                g.DrawImage(im.img, destRect, sourceRect, GraphicsUnit.Pixel);
            }

        }
        void DrawSlicers(Graphics g)
        {
            if ( !bShowOutlines ) return;
            if (slicer == null) return;             
            
            float tension = 0.5f;
            if ( !bSmoothLine ) tension = 0;
            
            Polygon2D poly;
            for (int i=0;i<slicer.polygons.Count;i++)            
            {
                poly = slicer.polygons[i];
                if ( !DataRect.IsIntersectWith(poly.minx, poly.miny, poly.maxx, poly.maxy) )
                     continue;

                PointF[] points = toDPPoints(poly.points);
                if (points == null || points.Length < 2)
                {
                    points = null;
                    continue; 
                }

                Pen pen;                
                if ( IsInSelected(1, i) )
                {
                    pen = new Pen(Color.Red);
                    pen.Width = poly.lineWidth;
                    if (pen.Width < 2) pen.Width = 2;
                    pen.DashStyle = DashStyle.Solid;
                }
                else
                {
                    pen = new Pen(poly.lineColor);
                    pen.Width = poly.lineWidth;
                    pen.DashStyle = DashStyle.Solid;
                }

                if (poly.IsClosed)
                {
                    Brush brush = new SolidBrush(poly.fillColor);
                    g.FillClosedCurve(brush, points, FillMode.Alternate, tension);
                    g.DrawClosedCurve(pen, points, tension, FillMode.Alternate);
                    brush.Dispose();
                }
                else 
                {                   
                    g.DrawCurve(pen, points, tension); 
                }

                //绘制线上的节点
                if ( IsInSelected(1, i) && bShowGridPoint )
                {
                    Rectangle dotRect = new Rectangle();
                    pen.Width = 1;
                    pen.DashStyle = DashStyle.Solid;
                    
                    int id = 0;
                    foreach (PointF p in points)
                    {
                        dotRect.Location = new Point((int)(p.X - dotSize / 2), (int)(p.Y - dotSize / 2));
                        dotRect.Width = (int)dotSize;
                        dotRect.Height = (int)dotSize;
                        if( IsInSelected( 1, i, id ) )
                        {
                            Pen selpointPen = new Pen(Color.BlueViolet);
                            selpointPen.Width = 2;
                            g.DrawRectangle(selpointPen, dotRect);
                            selpointPen.Dispose();
                        }
                        else g.DrawRectangle(pen, dotRect);
                        id++;
                    }
                }
                
                points = null;
                pen.Dispose();
                                
            }//for (int i=0;i<slicer.polygons.Count;i++)
        }
        void DrawDynamicLine(Graphics g)
        {
            PointF[] points = null;
            if (drawstate == 1)
            {
                if (drawslicer != null && drawslicer.points.Count > 0)
                {
                    points = toDPPoints(drawslicer.points);
                }
            }
            else if (drawstate == 2)
            {
                if (drawlayer != null && drawlayer.points.Count > 0)
                {
                    points = toDPPoints(drawlayer.points);
                }
            }
            else if (drawstate == 3)
            {
                if (slicer.Locations2D.Count > 0)
                {
                    points = toDPPoints(slicer.Locations2D);
                }
            }     

            if (points != null && points.Length > 1)
            {
                if (bSmoothLine) g.DrawCurve(Pens.BurlyWood, points);
                else g.DrawCurve(Pens.BurlyWood, points,0);
            }

            if (drawstate == 3 && points != null && points.Length > 0)
            {
                int size1 = 4;
                int size2 = 10;

                Pen pen1 = new Pen(Color.BurlyWood);
                pen1.Width = 2;
                Pen pen2 = new Pen(Color.Blue);
                pen2.Width = 1;

                PointF p1, p2;
                foreach (PointF p in points)
                {
                    ////////////////////////
                    p1 = p2 = p;
                    p1.X -= size1;
                    p2.X += size1;
                    g.DrawLine(pen1, p1, p2);
                    p1 = p2 = p;
                    p1.Y -= size1;
                    p2.Y += size1;
                    g.DrawLine(pen1, p1, p2);
                    ////////////////////////
                    p1 = p2 = p;
                    p1.X -= size2;
                    p1.Y -= size2;
                    g.DrawEllipse(pen2, p1.X, p1.Y, size2 * 2, size2 * 2);
                }
            }

        }
        public void UpdateDraw(bool update = true)
        {           
            if (slicer == null) return;

            int width = pictureBox1.Width;
            int height = pictureBox1.Height;
            if (width < 5 || height < 5) return;

            DrawRect = new Rectangle(leftMargin, topMargin,
                                     width - leftMargin - rightMargin,
                                     height - topMargin - bottomMargin);
            
            if ( update ) 
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
        }
        //返回点位置索引
        bool SelectPointsFromArray(Point p1, Point p2,List<Vector64> _points, int type,float size)
        {
            if (_points.Count < 1) return false;
#pragma warning disable CS0168 // 声明了变量“x”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“y”，但从未使用过
            double x, y;
#pragma warning restore CS0168 // 声明了变量“y”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“x”，但从未使用过
           
            int x0 = p1.X;
            int y0 = p1.Y;
            if (p2.X < x0) x0 = p2.X;
            if (p2.Y < y0) y0 = p2.Y;
            
            PointF[] pp = toDPPoints(_points);

            bool singleSelect = false;
            Rectangle rect = new Rectangle();
            if (p1 == p2) 
            {
                singleSelect = true;
                rect = new Rectangle((int)(p1.X - size / 2), (int)(p1.Y - size / 2), (int)size, (int)size); 
            }
            else rect = new Rectangle(x0, y0, Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
            
            int selected = 0;

            for (int i = 0; i < pp.Length; i++ )
            {
                if ( pp[i].X >= rect.Left && pp[i].X <= rect.Right &&
                     pp[i].Y >= rect.Top && pp[i].Y <= rect.Bottom )
                {                    
                    AddSelected(type, i);
                    selected++;
                    if ( singleSelect ) 
                    {
                        pp = null;
                        return true; 
                    }
                }
            }
            pp = null;
            if ( selected > 0 ) return true;
            else return false;
        }
        //点是否在此直线上
        bool SelectLineOnPoint(Point p1, Point p2, List<Vector32> points, int len = 10)
        {
            if (points.Count < 1) return false;
            PointF[] pp = toDPPoints(points);
            return SelectLineOnPoint(p1, p2, pp, len);
        }
        bool SelectLineOnPoint(Point p1, Point p2, List<Vector64> points, int len = 10)
        {
            if (points.Count < 1) return false;
            PointF[] pp = toDPPoints(points);
            return SelectLineOnPoint(p1,p2, pp, len);
        }
        bool SelectLineOnPoint(Point first, Point second,PointF[]pp,int len = 10)
        {
            int x0 = first.X;
            int y0 = first.Y;
            if (second.X < x0) x0 = second.X;
            if (second.Y < y0) y0 = second.Y;

            Rectangle rect1;
            if ( first == second ) rect1 = new Rectangle(first.X-10,first.Y-10,20,20);
            else rect1 = new Rectangle(x0, y0, Math.Abs(second.X - first.X), Math.Abs(second.Y - first.Y));

            PointF p1, p2;
            double dist = 0;

            Rectangle rect2 = new Rectangle();
            
            for (int i=1;i<pp.Length;i++)
            {
                p1 = pp[i - 1];
                p2 = pp[i];
                
                //判断是否在p1,p2矩形区域内
                x0 = (int)p1.X;
                y0 = (int)p1.Y;
                if (p2.X < x0) x0 = (int)p2.X;
                if (p2.Y < y0) y0 = (int)p2.Y;
                rect2 = new Rectangle(x0, y0, (int)Math.Abs(p2.X - p1.X), (int)Math.Abs(p2.Y - p1.Y));

                if( first == second ) //点选
                {
                    if ( !rect1.IntersectsWith( rect2 ) ) continue;

                    //判断p与p1,p2距离
                    dist = Vector32.PointToLineDistance(first.X, first.Y, p1.X, p1.Y, p2.X, p2.Y);
                    if (dist <= len)
                    {
                        pp = null;
                        return true;
                    }
                }
                else //框选
                {
                    if ( rect1.IntersectsWith(rect2) ) 
                        return true;
                }
                
            }            
            return false;
        }
        bool SelectPolygonPoint(Point p1, Point p2, double x1,double y1,double x2,double y2)
        {
            int x0 = p1.X;
            int y0 = p1.Y;
            if (p2.X < x0) x0 = p2.X;
            if (p2.Y < y0) y0 = p2.Y;
            Rectangle rect1 = new Rectangle(x0,y0, Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
            
            LPtoDP(ref x1, ref y1);
            LPtoDP(ref x2, ref y2);
            double mx1 = x1;
            double mx2 = x2;
            double my1 = y1;
            double my2 = y2;
            if (x2 < x1) { mx1 = x2; mx2 = x1; }
            if (y2 < y1) { my1 = y2; my2 = y1; }
            Rectangle rect2 = new Rectangle((int)mx1, (int)my1, (int)(mx2 - mx1), (int)(my2 - my1));
            
            if (rect1.Width < 1 && rect1.Height < 1) //select on point
            {
                if ( rect2.Contains(rect1.Location) )
                    return true;
            }
            else //select on rect
            {
                if (rect1.IntersectsWith(rect2))
                    return true;
            }
            return false;
        }

        bool SelectPolygonPoint(Point p1,Point p2, List<Vector32> points)
        {
            if (points.Count < 1) return false;
            PointF[] pp = toDPPoints(points);

            //////////////////////////////////////
            int w = Math.Abs(p1.X - p2.X);
            int h = Math.Abs(p1.Y - p2.Y);
            int x0 = p1.X;
            int y0 = p1.Y;
            if (p2.X < x0) x0 = p2.X;
            if (p2.Y < y0) y0 = p2.Y;
            Rectangle rect1 = new Rectangle(x0, y0, w, h);

            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            for(int i=0;i<pp.Length;i++)
            {
                if (i == 0) 
                {
                    x1 = x2 = (int)pp[i].X;
                    y1 = y2 = (int)pp[i].Y;
                }
                else
                {
                    if (x1 > pp[i].X) x1 = (int)pp[i].X;
                    if (y1 > pp[i].Y) y1 = (int)pp[i].Y;
                    if (x2 < pp[i].X) x2 = (int)pp[i].X;
                    if (y2 < pp[i].Y) y2 = (int)pp[i].Y;
                }
            }
            pp = null;
            Rectangle rect2 = new Rectangle(x1, y1, x2 - x1, y2 - y1);

            if (w < 1 && h < 1)            
                return rect2.Contains(p1);            
            else 
                return rect1.IntersectsWith(rect2);            
        }
        int SelectPointsGrid(List<Vector32>points,Point p,float size = 10)
        {
            if (points.Count < 1) return -1;
            PointF[] pp = toDPPoints(points);
            Rectangle rect = new Rectangle();
            for( int i = 0; i < pp.Length; i++ )
            {
                rect.Location = new Point( (int)( pp[i].X - size / 2), (int)(pp[i].Y - size / 2));
                rect.Width = (int)size;
                rect.Height = (int)size;
                if (rect.Contains(p)) 
                {
                    pp = null;
                    return i; 
                }
            }
            pp = null;
            return -1;
        }
        int SelectPointsGrid(List<Vector64> points,Point p, float size = 10)
        {
            PointF[] pp = toDPPoints(points);
            Rectangle rect = new Rectangle();
            for (int i = 0; i < pp.Length; i++)
            {
                rect.Location = new Point((int)(pp[i].X - size / 2), (int)(pp[i].Y - size / 2));
                rect.Width = (int)size;
                rect.Height = (int)size;
                if (rect.Contains(p))
                {
                    pp = null;
                    return i;
                }
            }
            pp = null;
            return -1;
        }
        //是否选中了已选择对象的节点,gridindex节点序号
        //返回值：p.X==1 polygons, p.X ==2 tracedGeoObjects
        //p.X ==3 tracedLine location point
        Point SelectPointsGrid(Point p,out int gridindex)
        {            
            gridindex = -1;
            Point sp = new Point(-1,-1);
            foreach(Point a in SelectedArray)
            {
                sp = a;
                if (sp.X == 1 && bShowGridPoint)
                {
                    gridindex = SelectPointsGrid(slicer.polygons[sp.Y].points,p);
                    if (gridindex >= 0) return sp;
                }
                else if (sp.X == 2 && bShowGridPoint) 
                {
                    gridindex = SelectPointsGrid(slicer.tracedGeoObjects[sp.Y].points,p);
                    if (gridindex >= 0) return sp;
                }                
                else if (sp.X == 3)
                {
                    gridindex = SelectPointsGrid(slicer.Locations2D, p, 20 );     
                    if (gridindex >= 0) return sp;
                }
            }
            return new Point(-1,-1);
        }
        void UpdateTreeSelected()
        {
            treeView1.SelectedNode = null;
            if( SelectedArray.Count > 0 )
            {
                Point p = SelectedArray[0];
                if( p.X ==1 )
                {
                    treeView1.SelectedNode = treeNode1.Nodes[p.Y];
                }
                else if (p.X == 2)
                {
                    treeView1.SelectedNode = treeNode2.Nodes[p.Y];
                }
            }
        }
        int DoSelectOnPoint(Point p1, Point p2)
        {
            if( !IsControlDown ) ClearSelected();

            int w = Math.Abs(p1.X - p2.X);
            int h = Math.Abs(p1.Y - p2.Y);
            if( w < 2 || h < 2) { w = h = 0; }

            //3---if location line selected,return
            //if ( SelectTracedLineOnPoint(p1, p2) > 0 ) return 1;
            if( slicer.Locations2D.Count > 0 && bShowLocation )
            {
                if (SelectLineOnPoint(p1,p2,slicer.Locations2D))
                {
                    AddSelected(3, 0);
                    if(p1 == p2) return 1; //点选
                }
            }
            //4---            
            if ( bShowSampledGrids && slicer.sampledGrids.Count > 0 )
            {
                if (SelectPointsFromArray(p1, p2, slicer.sampledGrids,4,sampledGridSize))
                {
                    //AddSelected(4, 0);
                    if (p1 == p2) return 1; //点选
                }
            }
            //2 - select traced objects
            if (slicer.tracedGeoObjects.Count > 0 && bShowLayers )
            {
                Polygon2D obj;                
                for (int i = slicer.tracedGeoObjects.Count - 1; i >= 0 ; i--)
                {
                    obj = slicer.tracedGeoObjects[i];
                    if (obj.IsClosed)
                    {
                        if (SelectPolygonPoint(p1, p2, obj.minx, obj.miny, obj.maxx, obj.maxy))
                        {
                            AddSelected(2, i);
                            if (w < 1 && !IsControlDown ) return SelectedArray.Count;
                        }
                    }
                    else
                    {
                        if (SelectLineOnPoint(p1,p2,obj.points))
                        {
                            AddSelected(2, i);
                            if (w < 1 && IsControlDown ) return SelectedArray.Count;
                        }
                    }
                }
            }
            //1 - select graphics objects
            if (slicer.polygons.Count > 0 && bShowOutlines )
            {
                Polygon2D obj;
                for (int i = slicer.polygons.Count - 1; i >= 0; i--)
                {
                    obj = slicer.polygons[i];
                    if (obj.IsClosed) // 封闭多边形
                    {
                        if (SelectPolygonPoint(p1, p2, obj.minx, obj.miny, obj.maxx, obj.maxy))
                        {
                            AddSelected(1, i);
                            if (w < 1 && !IsControlDown) //点选
                                return SelectedArray.Count;
                        }
                    }
                    else // 线
                    {
                        if (SelectLineOnPoint(p1, p2, obj.points))
                        {
                            AddSelected(1, i);
                            if (w < 1 && !IsControlDown) //点选
                                return SelectedArray.Count;
                        }
                    }
                }
            }
            return SelectedArray.Count;
        }
       
        /// <summary>
        /// 将点p捕捉到附近的轮廓线上
        /// </summary>
        /// <param name="p">输入点</param>
        /// <param name="p0">捕捉点</param>
        /// <returns> 是否捕捉成功</returns>
        bool SnapToOutlines(Vector64 p, out Vector64 p0 )
        {
            double mindist = 1.0E20, dist;
            p0 = new Vector64(0, 0, 0);
            bool found = false;

            Vector64 snaped = new Vector64();

            foreach (Polygon2D poly in slicer.tracedGeoObjects.Polygons)
            {
                dist = Vector64.SnapOnPointsGrid(p, poly.points, out snaped);
                if( dist < mindist )
                {
                    mindist = dist;
                    p0 = snaped;
                    found = true;
                }   
                
                if( Vector64.SnapOnEdgeLines(p, poly.points,poly.IsClosed, out snaped,out dist) )
                {
                    if (dist < mindist)
                    {
                        mindist = dist;
                        p0 = snaped;
                        found = true;
                    }
                }
            }//foreach (Polygon2D poly in slicer.tracedGeoObjects)
            foreach (Polygon2D poly in slicer.polygons.Polygons)
            {
                dist = Vector64.SnapOnPointsGrid(p, poly.points, out snaped);
                if (dist < mindist)
                {
                    mindist = dist;
                    p0 = snaped;
                    found = true;
                }                
                
                if (Vector64.SnapOnEdgeLines(p, poly.points, poly.IsClosed, out snaped, out dist))
                {
                    if (dist < mindist)
                    {
                        mindist = dist;
                        p0 = snaped;
                        found = true;
                    }
                }
            }
            
            return found;            
        }

        /// <summary>
        /// 将边界点捕捉到周围多边形节点
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        List<Vector32> DoTracedSnap( List<Vector32> points )
        {
            List<Vector32> lists = new List<Vector32>();
            Vector64 p0;            
            foreach(Vector32 p in points)
            {
                if ( SnapToOutlines(p.toVector64(), out p0) ) lists.Add(p0);
                else 
                {
                    lists.Add(p);
                    throw new Exception("no snaped found.");                    
                }
            }//foreach(Vector32 p in points)
            
            points.Clear();

            return lists;
        }

        bool DoFill(Point p)
        {
            floodFiller.FillColor = Color.Red;
            floodFiller.SetBitmap(Bmp);
            floodFiller.FloodFill(p);            
            
            //检测边界点
            List<Vector32>points = floodFiller.TraceFilledEdges(DrawRect,DataRect.x1, DataRect.y1, DataRect.x2, DataRect.y2);

            if(points == null )
            {
                MessageBox.Show("Tracing boundary failed.");
                return false;
            }

            Bmp = floodFiller.Bitmap.Bitmap;
            pictureBox1.Invalidate();

            //节点捕捉
            if( bEnableSnap )
            {
                points = DoTracedSnap(points);
            }
            
           // Vector32.RemoveLineRedundant(ref points);

            Polygon2D obj = new Polygon2D(points);
            obj.IsClosed = true;
            obj.fillColor = floodFiller.FillColor;            
            //obj.Simplify(true,true,0.001); //过滤点距小于千分之一平均点

            TracedObjectPropertyForm dlg = new TracedObjectPropertyForm();
            dlg.obj = obj;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                obj = dlg.obj;
                slicer.AddTracedGeoObject(obj);
                Modified = true;
                UpdateTree();
                PushData();
            }
            
            UpdateDraw();

            return true;
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
        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }
        void LPtoDP(ref double x, ref double y)
        {
            x = DrawRect.Left + DrawRect.Width * (x - DataRect.x1) / DataRect.Width;
            y = DrawRect.Bottom - DrawRect.Height * (y - DataRect.y1) / DataRect.Height;
        }
        void DPtoLP(ref double x, ref double y)
        {
            x = DataRect.x1 + DataRect.Width * (x - DrawRect.Left) / DrawRect.Width;
            y = DataRect.y1 + DataRect.Height * (DrawRect.Bottom - y) / DrawRect.Height;
        }
        private void Zoom(double x0, double y0, double scale = 0.8)
        {
            double offx = x0 - (DataRect.x1 + DataRect.x2) / 2.0;
            double offy = y0 - (DataRect.y1 + DataRect.y2) / 2.0;
            DataRect.Offset(offx, offy);
            DataRect.Scale(scale);
        }

        private void toolStripButton2ZoomIn_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.zoom_in;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }

        private void toolStripButton3ZoomOut_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.zoom_out;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }

        private void toolStripButton4Pan_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.pan;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }

        private void toolStripButton5Reset_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.normal;
            first = second = new Point(-1, -1);
            bMouseDown = false;
            ResetRect();
            UpdateMouseState();
            UpdateDraw();
        }
        
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            bBusy = true;

            Graphics g = e.Graphics;
            
            if ( Bmp != null ) g.DrawImage(Bmp, 0, 0);
            
            if ( bMouseDown && ( mouseState == MouseState.zoom_in ||
                                 mouseState == MouseState.normal ) )
            {
                if (first.X != -1 && second.X != -1)
                {
                    Rectangle rect = new Rectangle(first, new Size((second.X - first.X), (second.Y - first.Y)));
                    g.DrawRectangle(Pens.BlueViolet, rect);
                }
            }
            if ( mouseState == MouseState.draw )
            {
                DrawDynamicLine(g);//绘制动态线
                if (first.X != -1 && second.X != -1)
                {
                    g.DrawLine(Pens.BlueViolet, first, second);
                }
            }
            bBusy = false;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            ResetRect();
            UpdateDraw();  
        }

        private void SlicerModelingForm_Load(object sender, EventArgs e)
        {
            splitContainer1.SplitterDistance = 200;
            PushData();
            UpdateDraw();
        }

        void AddSlicerPoint(Point p)
        {
            double x = p.X;
            double y = p.Y;
            DPtoLP(ref x, ref y);
            drawslicer.Add(new Vector64(x, y, 0));
        }
        void AddGeoPoint(Point p)
        {
            double x = p.X;
            double y = p.Y;
            DPtoLP(ref x, ref y);
            drawlayer.Add(new Vector64(x, y, 0));
        }
        void AddLocationPoint(Point p)
        {
            double x = p.X;
            double y = p.Y;
            DPtoLP(ref x, ref y);
            slicer.AddLocationPoint(new Vector64(x, y, 0), new Vector64(0, 0, 0));
            PushData();
        }
        void EndDrawObject()
        {  
            if (drawstate == 1)
            {
               if( drawslicer.points.Count > 1 )
               {
                    drawslicer.UpdateRange();
                    ObjectPropertyForm dlg = new ObjectPropertyForm();
                    dlg.SetObject(drawslicer);
                    if ( dlg.ShowDialog() == DialogResult.OK)
                    {
                        slicer.AddPolygon(drawslicer);
                        Modified = true;
                        AddTreeItem1(drawslicer.Name, slicer.polygons.Count - 1);
                        drawslicer = new Polygon2D();                        
                    }
               }
            }
            else if (drawstate == 2)
            {
                if (drawlayer.points.Count > 1)
                {
                    drawlayer.UpdateRange();
                    ObjectPropertyForm dlg = new ObjectPropertyForm();
                    dlg.SetObject(drawlayer);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        slicer.AddTracedGeoObject(drawlayer);
                        Modified = true;
                        AddTreeItem2(drawlayer.Name, slicer.tracedGeoObjects.Count - 1);
                        drawlayer = new Polygon2D();                        
                    }
                }
            }
            else if (drawstate == 3)
            {
                Modified = true;
                if ( slicer.IsLocated ) slicer.UpdateTraced();
            }

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
                    selectedPoint = SelectPointsGrid(first, out selectedPointIndex);
                    if (selectedPointIndex >= 0) 
                    {
                        drawdata = CopyDrawData();
                        UpdateDraw(); 
                    }
                }
                else if (mouseState == MouseState.zoom_out)
                {
                    ZoomOut(first);
                    UpdateDraw();
                }                
                else if (mouseState == MouseState.Fill)
                {                   
                    DoFill(first);
                    ResetMouseState();
                }
                else if (mouseState == MouseState.draw)
                {
                    first = new Point(e.X, e.Y);
                    if (drawstate == 1) AddSlicerPoint(first);
                    else if (drawstate == 2) AddGeoPoint(first);
                    else if (drawstate == 3) AddLocationPoint(first);
                }
                else if (mouseState == MouseState.Test)
                {
                    first = new Point(e.X, e.Y);

                    double x = e.X;
                    double y = e.Y;
                    DPtoLP(ref x, ref y);
                    Vector64 p = new Vector64(x, y, 0);
                    Vector64 p0;
                    if (SnapToOutlines(p, out p0))
                    {
                        Graphics g = pictureBox1.CreateGraphics();

                        x = p0.x;
                        y = p0.y;
                        LPtoDP(ref x, ref y);

                        g.DrawLine(Pens.Red, e.X, e.Y, (float)x, (float)y);
                    }
                    else MessageBox.Show("no snap");
                }
            }//if (e.Button == MouseButtons.Left)

            else if (e.Button == MouseButtons.Right)
            {
                bMouseDown = false;
                first = second = new Point(-1, -1);

                if (mouseState == MouseState.normal)
                {
                    selectedPoint = SelectPointsGrid(new Point(e.X, e.Y), out selectedPointIndex);
                    if ( selectedPointIndex >= 0 ) //选中节点
                    {
                        UpdateDraw();
                        pictureBox1.ContextMenuStrip = contextMenuStrip3;
                    }
                    else pictureBox1.ContextMenuStrip = contextMenuStrip2;
                }
                else if (mouseState == MouseState.draw) //end of draw
                {
                    first = second = new Point(-1, -1);
                    mouseState = MouseState.normal;
                    pictureBox1.Cursor = Cursors.Default;

                    EndDrawObject();
                    Modified = true;
                    PushData();
                    UpdateDraw();
                }
                else
                {
                    ResetMouseState();
                }
            }//else if (e.Button == MouseButtons.Right)
        }

        //搜索可捕捉的最近的网格节点
        Point SearchSnapedGridPoint(Point p,int curType = -1, int curIndex = -1)
        {
            Point outp = new Point(-1,-1);
            float snapsize = dotSize * 1.5f;
            
            List<Point> searched = new List<Point>();
            Polygon2D poly = null;
            double x = 0, y = 0;
            if ( bShowGridPoint && SelectedArray.Count > 0 )
            {
                foreach(Point sp in SelectedArray )
                {
                    if (searched.Count > 0) break;
                    
                    if (curIndex >= 0 && curType >= 0) //是否排除自身
                    { 
                        if (sp.X == curType && sp.Y == curIndex) continue; 
                    }

                    if (sp.X == 1) poly = slicer.polygons[sp.Y];
                    else if (sp.X == 2) poly = slicer.tracedGeoObjects[sp.Y];
                    else continue;

                    for (int j = 0; j < poly.points.Count; j++)
                    {
                        x = poly.points[j].x;
                        y = poly.points[j].y;
                        LPtoDP(ref x, ref y);
                        if (Math.Abs(x - p.X) <= snapsize && Math.Abs(y - p.Y) <= snapsize)
                        {
                            searched.Add(new Point((int)x, (int)y));
                            break;
                        }
                    }
                }
            }
            
            if (searched.Count > 0) outp = searched[0];
            
            searched.Clear();

            return outp;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if ( bBusy ) return;
            
            double x = e.X;
            double y = e.Y;
            DPtoLP(ref x,ref y);

            if (mouseState == MouseState.draw) curPositionLabel.Text = "Drawing --> ";
            if (mouseState == MouseState.Fill) curPositionLabel.Text = "Filling --> ";
            if (mouseState == MouseState.zoom_in) curPositionLabel.Text = "Zoom In --> ";
            if (mouseState == MouseState.zoom_out) curPositionLabel.Text = "Zoom Out --> ";
            if (mouseState == MouseState.moving) curPositionLabel.Text = "Moving Object --> ";
            if (mouseState == MouseState.normal) curPositionLabel.Text = "Selecting --> ";
            
            curPositionLabel.Text += "x = " + Math.Round(x, 6);
            curPositionLabel.Text += ", y =" + Math.Round(y, 6);

            if( mouseState == MouseState.normal && bMouseDown )
            {
                if (selectedPoint.X >= 1 && selectedPoint.Y >= 0 && selectedPointIndex >= 0)
                {
                    second = new Point(e.X, e.Y);
                    if ( Math.Abs(second.X - first.X) > 2 || Math.Abs(second.Y - first.Y) > 2)
                    {                        
                        if( bEnableSnap )//网格移动捕捉
                        {
                            Point snaped = SearchSnapedGridPoint(e.Location, selectedPoint.X, selectedPoint.Y);
                            if( snaped.X > -1 )
                            {
                                second = snaped;
                            }
                        }

                        MovePointGrid(selectedPoint, selectedPointIndex, second);
                        //PushData();移动结束的时候再入栈
                        second = new Point(-1,-1);
                        UpdateDraw();
                    }
                }                
                else
                {
                    second = new Point(e.X, e.Y);
                    pictureBox1.Invalidate();
                }
            }
            if (SelectedArray.Count > 0 && mouseState == MouseState.moving && bMouseDown )
            {
                second = new Point(e.X, e.Y);
                MoveSelectedObjects(first, second);
                first = second;
                PushData();
                UpdateDraw();
            }
            if ( mouseState == MouseState.draw)
            {
                second = new Point(e.X, e.Y);
                if ( bEnableSnap )//绘制网格捕捉
                {
                    Point snaped = SearchSnapedGridPoint(second, -1,-1);
                    if (snaped.X > -1)
                    {
                        second = snaped;
                    }
                }

                pictureBox1.Invalidate();
            }
            if (mouseState == MouseState.zoom_in && bMouseDown)
            {
                second = new Point(e.X, e.Y);
                pictureBox1.Invalidate();
            }

        }
        void MoveSelectedObjects(Point p1,Point p2)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double x2 = p2.X;
            double y2 = p2.Y;
            DPtoLP(ref x1,ref y1);
            DPtoLP(ref x2, ref y2);

            foreach (Point p in SelectedArray)
            {
                if( p.X == 1 )
                {
                    slicer.polygons[p.Y].Offset(x2-x1,y2-y1,0);
                }
                else if (p.X == 2)
                {
                    slicer.tracedGeoObjects[p.Y].Offset(x2 - x1, y2 - y1,0);
                }
            }
           
            Modified = true;
        }
        void MovePointGrid(Point select,int index,Point dp)
        {
            double x = dp.X;
            double y = dp.Y;
            DPtoLP(ref x, ref y);
            if(select.X == 1)
            {
                Vector64 p = slicer.polygons[select.Y].points[index];
                p.X = x;
                p.Y = y;
                slicer.polygons[select.Y].points[index] = p;
            }
            else if (select.X == 2)
            {
                Vector64 p = slicer.tracedGeoObjects[select.Y].points[index];
                p.X = x;
                p.Y = y;
                slicer.tracedGeoObjects[select.Y].points[index] = p;
            }
            else if (select.X == 3)
            {
                Vector64 p = slicer.Locations2D[index];
                p.X = x;
                p.Y = y;
                slicer.Locations2D[index] = p;
            }

            Modified = true;
        }
        void RemovePointGrid(Point select, int index)
        {            
            if (select.X == 1)
            {
                slicer.polygons[select.Y].points.RemoveAt(index);                
            }
            else if (select.X == 2)
            {
                slicer.tracedGeoObjects[select.Y].points.RemoveAt(index);
            }
            else if (select.X == 3)
            {
                slicer.Locations2D.RemoveAt(index);
            }
            else if (select.X == 4)
            {
                slicer.sampledGrids.RemoveAt(index);
            }            
            Modified = true;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if ( mouseState == MouseState.normal && bMouseDown)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (mouseState == MouseState.normal && bMouseDown)
                    {
                        //移动节点结束？？？？
                        if (selectedPoint.X >= 1 && selectedPoint.Y >= 0 && selectedPointIndex >= 0)
                        {
                            if (Math.Abs(second.X - first.X) > 2 || Math.Abs(second.X - first.X) > 2)
                            {
                                Modified = true;
                                PushData(); 
                            }
                        }
                        else if (selectedPointIndex < 0)//not a end of moving
                        {
                            //clear selected grid info
                            selectedPoint = new Point(-1, -1);
                            selectedPointIndex = -1;

                            second = new Point(e.X, e.Y);

                            int selected = SelectedArray.Count;

                            if (DoSelectOnPoint(first, second) > 0)
                            {
                                first = second = new Point(-1, -1);
                                UpdateDraw();
                            }
                            else
                            {
                                if (selected > 0) UpdateDraw();
                            }
                            //UpdateTreeSelected();不能更新选择树，不然嵌套循环

                            first = second = new Point(-1, -1);

                            pictureBox1.Invalidate();
                        }
                    }
                }
            }//if ( mouseState == MouseState.normal && bMouseDown)

            if (mouseState == MouseState.zoom_in && bMouseDown)
            {
                second = new Point(e.X, e.Y);
                ZoomRect(first,second);
                UpdateDraw();
            }

            bMouseDown = false;
        }

        void ZoomRect(Point p1,Point p2)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double x2 = p2.X;
            double y2 = p2.Y;
            
            if( Math.Abs(x2 - x1) < 5 || Math.Abs(y2 - y1) < 5)
            {
                ZoomIn(new Point( (int)((x1 + x1 ) / 2.0), (int)((y1 + y1) / 2.0 )) );
                return;
            }

            DPtoLP(ref x1, ref y1);
            DPtoLP(ref x2, ref y2);
            double x0 = (x1 + x2) / 2.0;
            double y0 = (y1 + y2) / 2.0;

            double xx = Math.Abs(x2 - x1);
            double yy = Math.Abs(y2 - y1);
            
            double scale = DataRect.Height / DataRect.Width;            
            if ( scale * DrawRect.Width < DrawRect.Height )
            {
                yy = xx * scale;
            }
            else
            {
                xx = yy / scale;
            }
            
            DataRect.x1 = x0 - xx / 2.0;
            DataRect.y1 = y0 - yy / 2.0;
            DataRect.x2 = x0 + xx / 2.0;
            DataRect.y2 = y0 + yy / 2.0; ;
        }

        //以p0点为中心缩放
        private void Zoom(Point p0, double scale = 0.8)
        {
            double x1=0, y1=0;
            double x2=0, y2=0;
            
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

            DataRect.x1 = x0 - xx / 2;
            DataRect.x2 = x0 + xx / 2;
            DataRect.y1 = y0 - yy / 2;
            DataRect.y2 = y0 + yy / 2;

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

        private void toolStripButtonFill_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.Fill;
            UpdateMouseState();
        }
        void DoSelectTree()
        {
            int index = -1;
            int type = -1;
            ClearSelected();
            
            if (treeView1.SelectedNode == null) return;            

            if( treeView1.SelectedNode.Level == 1 )
            {
                try 
                {
                    type = Convert.ToInt32(treeView1.SelectedNode.Parent.Tag);
                    index = Convert.ToInt32(treeView1.SelectedNode.Tag);
                }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                catch(Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                {
                    index = -1;
                    type = -1;
                }
            }            

            propertyGrid1.SelectedObject = null;
            if ( type == 1 && index >= 0)
            {
                propertyGrid1.SelectedObject = slicer.polygons[index];
            }
            else if ( type == 2 && index >= 0)
            {
                propertyGrid1.SelectedObject = slicer.tracedGeoObjects[index];
            }
            AddSelected(type, index);
            UpdateDraw();
        }        
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DoSelectTree();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Modified = true;
            PushData();
            UpdateDraw();
        }

        private void SlicerModelingForm_SizeChanged(object sender, EventArgs e)
        {            
                      
        }

        private void splitContainer2_Panel1_SizeChanged(object sender, EventArgs e)
        {
            int w = splitContainer2.Panel1.Width;
            int h = splitContainer2.Panel1.Height;
            treeView1.Width = w - 10;
            treeView1.Height = h - 10;
            //w = splitContainer2.Panel2.Width;
            //h = splitContainer2.Panel2.Height;
            propertyGrid1.Width = treeView1.Width;
            propertyGrid1.Height = splitContainer2.Height - h - 10;
        }
        //delete object base on the selection from tree
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( treeView1.SelectedNode == null ) return; //no selection
            int level = treeView1.SelectedNode.Level;
            if (level == 0) //parent selected
            {
                if (MessageBox.Show("Delect all Objects in this Content?" + treeView1.SelectedNode.Text, "Delete All Objects？", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    if (treeView1.SelectedNode.Name == OutlinesNode && slicer.polygons.Count > 0)
                    {
                        slicer.polygons.Clear();
                        PushData();
                        UpdateDraw();
                    }
                    else if (treeView1.SelectedNode.Name == GeoObjectsNode && slicer.tracedGeoObjects.Count > 0)
                    {
                        slicer.tracedGeoObjects.Clear();
                        PushData();
                        UpdateDraw();
                    }
                }
            }
            else if (level == 1) //children item selected
            {
                TreeNode node1 = treeView1.SelectedNode.Parent;
                if ( node1 == null || node1.Level < 0 ) return;
                if (MessageBox.Show("Delete this object ?" + treeView1.SelectedNode.Text, "Delete object ？", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    if ( treeView1.SelectedNode.Parent.Name == OutlinesNode )
                    {
                        int index = (int)treeView1.SelectedNode.Tag;
                        DeleteObject(1, index);
                        treeView1.SelectedNode = null;
                        PushData();
                        UpdateDraw();
                    }
                    else if (treeView1.SelectedNode.Parent.Name == GeoObjectsNode )
                    {
                        int index = (int)treeView1.SelectedNode.Tag;
                        DeleteObject(2, index);
                        treeView1.SelectedNode = null;
                        PushData();
                        UpdateDraw();
                    }
                }
            }
            
            
        }
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                
                TreeNode CurrentNode = treeView1.GetNodeAt(ClickPoint);
                if (CurrentNode == null) return;
                DoSelectTree();

                CurrentNode.ContextMenuStrip = contextMenuStrip1;
                //treeView1.SelectedNode = CurrentNode;

            }//if (e.Button == MouseButtons.Right)
        }

        private void toolStripButtonArrow_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.normal;
            UpdateMouseState();
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if( SelectedArray.Count > 0 )
            {
                ObjectPropertyEdit(SelectedArray[0].X, SelectedArray[0].Y);
            }
        }
        void ObjectPropertyEdit(int type,int index)
        {
            if (type == 1)
            {
                ObjectPropertyForm dlg = new ObjectPropertyForm();
                Polygon2D obj = slicer.polygons[index].Copy();
                dlg.SetObject(obj);                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Modified = true;
                    slicer.polygons[index] = obj;
                    PushData();
                    UpdateDraw();
                }
            }
            else if (type == 2)
            {
                ObjectPropertyForm dlg = new ObjectPropertyForm();
                Polygon2D obj = slicer.tracedGeoObjects[index].Copy();                
                dlg.SetObject(obj);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Modified = true;
                    slicer.tracedGeoObjects[index] = obj.Copy();
                    PushData();
                    UpdateDraw();
                }
                /*
                TracedObjectPropertyForm dlg = new TracedObjectPropertyForm();
                dlg.line = slicer.tracedGeoObjects[index];
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    UpdateDraw();
                }*/
            }
            else if (type == 3)
            {
                if (selectedPointIndex >= 0)
                {
                    Vector64 screenPoint = slicer.Locations2D[selectedPointIndex];
                    Vector64 spacePoint = slicer.Locations3D[selectedPointIndex];
                    LocationDlg dlg = new LocationDlg();
                    dlg.screenPoint = screenPoint;
                    dlg.spacePoint = spacePoint;
                    dlg.axis = slicer.axis;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {   
                        Modified = true;
                        spacePoint = dlg.spacePoint;
                        slicer.axis = dlg.axis;
                        slicer.Locations3D[selectedPointIndex] = spacePoint;
                        PushData();
                        UpdateDraw();
                    }
                }
            }
        }
        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedArray.Count < 1) return;
            
            string names = "\n";
            foreach(Point p in SelectedArray)
            {
                if (p.X == 1) names += slicer.polygons[p.Y].Name;
                if (p.X == 2) names += slicer.tracedGeoObjects[p.Y].Name;
                if (p.X == 3) names += "Location Line";
                if (p.X != 4) names += ";\n ";
            }

            if ( MessageBox.Show("删除" +SelectedArray.Count+ "个对象："+ names, 
                "是否删除已选中对象？", MessageBoxButtons.YesNoCancel, 
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteSelected();
                PushData();
                UpdateDraw();
            }            
        }

        private void unselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearSelected();
            
            UpdateDraw();
        }

        private void showPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowGridPoint = !bShowGridPoint;
            showPointToolStripMenuItem.Checked = bShowGridPoint;
            if (SelectedArray.Count > 0) UpdateDraw();
        }
        private void smoothLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bSmoothLine = !bSmoothLine;
            smoothLineToolStripMenuItem.Checked = bSmoothLine;
            UpdateDraw();
        }       

        private void removeGridPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedPoint.X >= 1 && selectedPoint.Y >= 0 && selectedPointIndex >= 0)
            {
                RemovePointGrid(selectedPoint, selectedPointIndex);
                selectedPoint = new Point(-1, -1);
                selectedPointIndex = -1;
                UpdateDraw();
            }
        }

        private void toolStripButtonLocate_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.draw;
            drawstate = 3;
            first = second = new Point(-1,-1);
            UpdateMouseState();
        }
        private void toolStripButtonMove_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.moving;
            drawstate = 0;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }
        private void geoObjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.draw;
            drawstate = 2;
            drawlayer = new Polygon2D();
            drawlayer.IsClosed = false;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }

        private void graphicsObjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.draw;
            drawstate = 1;
            drawslicer = new Polygon2D();
            drawslicer.IsClosed = false;
            first = second = new Point(-1, -1);
            UpdateMouseState();
        }
        private void propertyEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedArray.Count > 0)
            {
                ObjectPropertyEdit(SelectedArray[0].X, SelectedArray[0].Y);
            }
        }
        private void SlicerModelingForm_KeyDown(object sender, KeyEventArgs e)
        {            
            IsControlDown = e.Control;

            //撤销
            if( IsControlDown && e.KeyCode == Keys.Z )
            {
                UndoToolStripButton_Click(sender,e);
            }
            //重做
            else if (IsControlDown && e.KeyCode == Keys.Y)
            {
                RedoToolStripButton_Click(sender, e);
            }
            if ( e.KeyCode == Keys.Delete )
            {
                if (selectedPoint.X >= 1 && selectedPoint.Y >= 0 && selectedPointIndex < 0)
                {
                    if (MessageBox.Show("是否删除已选中对象？","是否删除已选中对象？", MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        DeleteObject(selectedPoint.X, selectedPoint.Y);
                        ClearSelected();
                        Modified = true;
                        PushData();
                        UpdateDraw();
                    }
                    
                }
                if (selectedPoint.X >= 1 && selectedPoint.Y >= 0 && selectedPointIndex >= 0)
                {   
                    RemovePointGrid(selectedPoint, selectedPointIndex);
                    selectedPointIndex = -1;
                    Modified = true;
                    PushData();
                    UpdateDraw();
                }
             }

            if( IsControlDown && e.KeyCode == Keys.S )
            {
                SaveData();
            }
        }

        private void SlicerModelingForm_KeyUp(object sender, KeyEventArgs e)
        {
            IsControlDown = e.Control;
        }

        private void viewToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            showPointToolStripMenuItem.Checked = bShowGridPoint;
            smoothLineToolStripMenuItem.Checked = bSmoothLine;
            showBackgroundToolStripMenuItem.Checked = bShowOutlines || bShowBackgroundImages ;
            showLayersToolStripMenuItem.Checked = bShowLayers;
            showLocationToolStripMenuItem.Checked = bShowLocation;
            showPropertyGridToolStripMenuItem.Checked = bShowSampledGrids;
            snapToolStripMenuItem.Checked = bEnableSnap;
        }

        private void SlicerModelingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult ret = DialogResult.No;
            e.Cancel = false;
            if ( Modified ) ret = MessageBox.Show("数据已经修改，是否保存已修改数据？", "是否保存数据？", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if ( ret == DialogResult.Yes )
            {
                DialogResult = DialogResult.OK;
                e.Cancel = false;                
            }
            else if (ret == DialogResult.No)
            {
                DialogResult = DialogResult.No;
                e.Cancel = false;
            }
            else if (ret == DialogResult.Cancel)
            {
                DialogResult = DialogResult.Cancel;
                e.Cancel = true;
            }
        }

        private void showLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowLocation = !bShowLocation;
            UpdateDraw();
        }
       
        private void showPropertyGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowSampledGrids = !bShowSampledGrids;
            UpdateDraw();
        }
     

        private void simplifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedArray.Count > 0)
            {
                PointsSimplifyDlg pd = new PointsSimplifyDlg();
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    double scale = pd.scale;
                    bool remove1 = pd.remove1;
                    bool remove2 = pd.remove2;
                    Polygon2D obj = null;
                    if(SelectedArray[0].X ==1 )
                    {
                        obj = slicer.polygons[SelectedArray[0].Y];
                        obj.Simplify(remove1, remove2, scale);
                        slicer.polygons[SelectedArray[0].Y] = obj;
                    }
                    else if (SelectedArray[0].X == 2)
                    {
                        obj = slicer.tracedGeoObjects[SelectedArray[0].Y];
                        obj.Simplify(remove1, remove2, scale);
                        slicer.tracedGeoObjects[SelectedArray[0].Y] = obj;
                    }
                    Modified = true;
                    PushData();
                    UpdateDraw();
                }
            }
            
        }
        //对剖面进行网格采样，获取散乱点
        private void gridSampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (slicer == null) return;
            if (slicer.tracedGeoObjects.Count < 1) return;

            SlicerModelingDlg md = new SlicerModelingDlg();
            md.AddSlicer(slicer);
            if (md.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;

               // slicer.SampleLayerCoords(md.xGridOuter, md.yGridOuter,md.XResampleExt, md.YResampleExt);
                Modified = true;
                slicer = md.slicers[0];

                Cursor = Cursors.Default;
                
                UpdateDraw();
            }
        }
        
        private void UndoToolStripButton_Click(object sender, EventArgs e)
        {
            if (UndoData())
            {
                Cursor = Cursors.WaitCursor;
                UpdateDraw();
                UpdateTree();
                Modified = true;
                Cursor = Cursors.Default;
            }
        }

        private void RedoToolStripButton_Click(object sender, EventArgs e)
        {
            if (RedoData())
            {
                Cursor = Cursors.WaitCursor;
                UpdateDraw();
                UpdateTree();
                Modified = true;
                Cursor = Cursors.Default;
            }
        }
        /// <summary>

        /// </summary>
        /// <param name="select">
        /// select.x， 1,2,3,4代表不同的对象类别
        /// select.y,代表序号
        /// </param>
        /// <param name="index">代表选择polygon对象节点序号，-1表示为选中节点</param>
        void ToUnclosedOrClosed(Point select, int index)
        {
            if (select.X == 1)
            {
                slicer.polygons[select.Y].ToUnClosed(index);
            }
            else if (select.X == 2)
            {
                slicer.tracedGeoObjects[select.Y].ToUnClosed(index);
            }
            
            PushData();
            Modified = true;
        }

        //选中对象节点，节点重组为非封闭多边形
        private void toUnclosedMenuItem1_Click(object sender, EventArgs e)
        {
            if (selectedPoint.X >= 1 && selectedPoint.Y >= 0 && selectedPointIndex >= 0)
            {
                ToUnclosedOrClosed(selectedPoint, selectedPointIndex);
                selectedPoint = new Point(-1, -1);
                selectedPointIndex = -1;
                UpdateDraw();
            }
        }
        //选中对象，对象设置为非封闭多边形
        private void toUnclosedMenuItem0_Click(object sender, EventArgs e)
        {
            if( selectedPointIndex < 0 && SelectedArray.Count > 0 )
            {   
                ToUnclosedOrClosed(SelectedArray[0], -1);
                UpdateDraw();
            }
        }
        //置后,放到队列前
        private void backgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( SelectedArray.Count > 0 )
            {
                Point selp = SelectedArray[0];

                if (selp.X == 1)
                {
                    if (selp.Y > 0)
                    { 
                        Polygon2D poly = slicer.polygons[selp.Y];
                        slicer.polygons.RemoveAt(selp.Y);
                        slicer.polygons.Insert(0, poly);                        
                    }
                }
                else if (selp.X == 2)
                {
                    if (selp.Y > 0)
                    {
                        Polygon2D poly = slicer.tracedGeoObjects[selp.Y];
                        slicer.tracedGeoObjects.RemoveAt(selp.Y);
                        slicer.tracedGeoObjects.Insert(0, poly);
                    }                    
                }
                //修改选择项
                SelectedArray[0] = new Point(selp.X,0);
                PushData();
                Modified = true;
                UpdateDraw();
            }
        }
        //置前,放到队列最后
        private void frontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedArray.Count > 0)
            {
                Point selp = SelectedArray[0];
                if (selp.X == 1)
                {
                    if (selp.Y < slicer.polygons.Count -1 )
                    {
                        Polygon2D poly = slicer.polygons[selp.Y];
                        slicer.polygons.RemoveAt(selp.Y);
                        slicer.polygons.Add(poly);
                        SelectedArray[0] = new Point(selp.X, slicer.polygons.Count - 1);
                    }
                }
                else if (selp.X == 2)
                {
                    if (selp.Y < slicer.tracedGeoObjects.Count - 1 )
                    {
                        Polygon2D poly = slicer.tracedGeoObjects[selp.Y];
                        slicer.tracedGeoObjects.RemoveAt(selp.Y);
                        slicer.tracedGeoObjects.Add(poly);
                        //修改选择项
                        SelectedArray[0] = new Point(selp.X, slicer.tracedGeoObjects.Count - 1);
                    }
                }                

                PushData();
                Modified = true;
                UpdateDraw();
            }
        }

        private void dataRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SlicerDataRangeForm sf = new SlicerDataRangeForm(slicer);
            if( sf.ShowDialog() == DialogResult.OK )
            {
                Modified = true;
                UpdateBackImageRange();
                UpdateDraw();
            }
        }
        public void SaveData()
        {
            if (slicer == null) return;
            if( slicer.originalPath.Length > 1 )
            {
                slicer.SaveAs(slicer.originalPath);
                undos.Clear();
                redos.Clear();
            }
            else
            {
                saveAsToolStripMenuItem_Click(null,null);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if  (slicer == null ) return;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Polygon Slicer (*.slicer)|*.slicer|all files(*.*)|*.*";
                dlg.OverwritePrompt = true;
                if (slicer.originalPath.Length > 1) dlg.FileName = slicer.originalPath;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (slicer.SaveAs(dlg.FileName))
                    {
                        MessageBox.Show("data saved.");
                    }
                    else
                    {
                        MessageBox.Show("data save failed.");
                    }
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (slicer == null) return;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Polygon Slicer (*.slicer)|*.slicer|all files(*.*)|*.*";
                dlg.OverwritePrompt = true;
                if (slicer.originalPath.Length > 1) dlg.FileName = slicer.originalPath;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (slicer.SaveAs(dlg.FileName))
                    {
                        MessageBox.Show("data saved.");
                    }
                    else
                    {
                        MessageBox.Show("data save failed.");
                    }
                }
            }
        }

        private void snapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bEnableSnap = !bEnableSnap;
        }
        private void showLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowLayers = !bShowLayers;
            bShowLayerFault = bShowLayerPolygon = bShowLayers;
            UpdateDraw();
        }
        private void faultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowLayerFault = !bShowLayerFault;
            UpdateDraw();            
        }

        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowLayerPolygon = !bShowLayerPolygon;
            UpdateDraw();
        }

        private void showLayersToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            polygonToolStripMenuItem.Checked = bShowLayerPolygon;
            faultToolStripMenuItem.Checked = bShowLayerFault;
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
        void UpdateBackImageRange()
        {
            if (slicer == null) return;
            if( slicer.backImages.Count >0 )
            {
                ImageStruct im = slicer.backImages[0];
                im.rect = new DoubleRect(slicer.minx, slicer.miny, slicer.maxx, slicer.maxy);
                slicer.backImages[0] = im;
            }
        }
        private void importImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (slicer == null) return;
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Images File(*.jpg,*.png,*.bmp,*.gif,*.tif)|*.jpg;*.png;*.bmp;*.gif;*.tif|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ImageStruct im = new ImageStruct();
                    im.img = new Bitmap(dlg.FileName);
                    im.rect = new DoubleRect(slicer.minx, slicer.miny, slicer.maxx, slicer.maxy);
                    slicer.backImages.Add(im);
                    UpdateDraw();
                }
            }
        }

        private void outLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowOutlines = !bShowOutlines;
            UpdateDraw();
        }

        private void imagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bShowBackgroundImages = !bShowBackgroundImages;
            UpdateDraw();
        }       

        private void showBackgroundToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            outLinesToolStripMenuItem.Checked = bShowOutlines;
            backgroundToolStripMenuItem.Checked = bShowBackgroundImages;

        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mouseState = MouseState.Test;
            UpdateMouseState();
        }        

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void shapeModelingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Polygon2D polygon = null;
            if ( SelectedArray.Count == 1 )
            {
                int type = SelectedArray[0].X;
                int id = SelectedArray[0].Y;
                
                if ( type == 1 )
                {
                    polygon = slicer.polygons[id];
                }
                else if (type == 2)
                {
                    polygon = slicer.tracedGeoObjects[id];
                }
            }

            if (polygon == null) return;
            
            SymbolModelingForm md = new SymbolModelingForm(polygon);
            if( md.ShowDialog() == DialogResult.OK )
            {

            }
        }

        private void polygon2DToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void selectedGridsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "ASCII XYZ (*.dat,*.txt)|*.dat;*.txt|all files(*.*)|*.*";
                dlg.OverwritePrompt = false;
                if ( dlg.ShowDialog() == DialogResult.OK )
                {
                    List<int> indices = new List<int>();
                    
                    foreach (Point p in SelectedArray)
                    {
                        if (p.X == 4) indices.Add(p.Y);
                    }
                    
                    int n = indices.Count;

                    if (indices.Count > 0) slicer.ExportLayerPropertyToXYZ(dlg.FileName, indices);
                    else { slicer.ExportLayerPropertyToXYZ(dlg.FileName); n = slicer.sampledGrids.Count; }
                    indices.Clear();
                    MessageBox.Show(n +" rows exported to file \n'"+dlg.FileName+"'","Successfully");
                }
            }
            
        }
    }
}
