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
using DataCollection.DelaunayVoronoi;

namespace DDDSharp.Modeling
{
    public partial class SymbolModelingForm : Form
    {
        Polygon2D polygon = null;
        TriangleObj triobj = null;        
        PolygonRotatingModeling Rotating = new PolygonRotatingModeling();
        PolygonThickeningModeling Thickening = new PolygonThickeningModeling();
        PolygonExtendingModeling Extending = new PolygonExtendingModeling();
        TriangleObj obj = null;
        PolygonModelingMethodEnum method = PolygonModelingMethodEnum.Rotating;
        public SymbolModelingForm(Polygon2D _polygon)
        {
            InitializeComponent();
            polygon = _polygon;
        }
        void InitPolygon(Polygon2D poly)
        {
            if (poly == null) return;
            
            polygon = poly;
            Rotating.Color = poly.lineColor;
            
            Thickening._Width = polygon.XWidth;
            Thickening._Height = polygon.YWidth;
            Thickening.FrontFaceColor = poly.fillColor;
            Thickening.BackFaceColor = poly.fillColor;
            Thickening.Color = poly.lineColor;
            Thickening.Depth = polygon.XWidth * 0.25;
            
            Extending._Width = polygon.XWidth;
            Extending._Height = polygon.YWidth;
            Extending.ExtendLength = polygon.XWidth * 0.25;
            Extending.Color = poly.lineColor;

            comboBox1.Items.Clear();
            string[] names = Enum.GetNames(typeof(PolygonModelingMethodEnum));
            comboBox1.Items.AddRange(names);
            comboBox1.SelectedIndex = 0;
        }
        private void SymbolModelingForm_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = polygon;
            propertyGrid2.SelectedObject = obj;
            InitPolygon(polygon);            
        }

        void LPtoDP( ref double x, ref double y)
        {
            //边距2pixels
            int margine = 2;
            double x1 = margine;
            double y2 = pictureBox1.Height- margine;
            double width = pictureBox1.Width- margine*2;
            double height = pictureBox1.Height- margine*2;
            x = x1 + width * (x - polygon.Minx) / polygon.XWidth;
            y = y2 - height * (y - polygon.Miny) / polygon.YWidth;
        }

        void DrawTriangles(Graphics g, TriangleObj tri)
        {
            if (tri == null) return;

            //Graphics g = pictureBox1.CreateGraphics();
            Vector32 p1, p2, p3;
            double x1, y1, x2, y2, x3, y3;
            Pen pen = new Pen(Color.LightGray);
            pen.Width = 0.8f;
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            for(int i = 0; i < tri.triangles.Count; i++ )
            {
                p1 = tri.points[tri.triangles[i].x];
                p2 = tri.points[tri.triangles[i].y];
                p3 = tri.points[tri.triangles[i].z];
                x1 = p1.X; y1 = p1.Y;
                x2 = p2.X; y2 = p2.Y;
                x3 = p3.X; y3 = p3.Y;
                LPtoDP(ref x1, ref y1);
                LPtoDP(ref x2, ref y2);
                LPtoDP(ref x3, ref y3);                
                g.DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);
                g.DrawLine(pen, (float)x2, (float)y2, (float)x3, (float)y3);
                g.DrawLine(pen, (float)x3, (float)y3, (float)x1, (float)y1);
            }
        }

        void DrawPolygon( Graphics g,Polygon2D poly )
        {
            if (pictureBox1 == null) return;
            if (poly == null ) return;
          
            g.FillRectangle(Brushes.White, new Rectangle(0,0,pictureBox1.Width,pictureBox1.Height));

            double x, y;            
            
            int n = poly.points.Count;
            if (poly.IsClosed) n++;

            PointF[] pp = new PointF[n];
            for( int i = 0; i < poly.points.Count; i++ )
            {
                x = poly.points[i].X;
                y = poly.points[i].Y;
                LPtoDP(ref x, ref y);
                pp[i] = new PointF((float)x, (float)y);
            }

            if(poly.IsClosed ) pp[n - 1] = pp[0];

            Pen pen = new Pen(poly.lineColor);
            pen.Width = poly.lineWidth;
            pen.DashStyle = poly.dashStyle;
            g.DrawLines(pen, pp);

            if (poly.IsClosed && poly.IsFill) g.FillPolygon(new SolidBrush(poly.fillColor), pp);

            pp = null;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if( polygon == null )
            {
                MessageBox.Show("No source objects.");
                return;
            }
            
            Cursor = Cursors.WaitCursor;
                        
            PolygonTriangulate tri = new PolygonTriangulate(polygon);
            triobj = tri.Triangulate(polygon);
          
            if (method == PolygonModelingMethodEnum.Rotating)
                obj = Rotating.Create(polygon);
            else if (method == PolygonModelingMethodEnum.Thickening)
                obj = Thickening.Create(polygon);
            else obj = Extending.Create(polygon);
           
            Cursor = Cursors.Default;

            propertyGrid2.SelectedObject = obj;

            pictureBox1.Invalidate();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {            
            method = (PolygonModelingMethodEnum)comboBox1.SelectedIndex;
            if(method == PolygonModelingMethodEnum.Rotating)
                propertyGrid1.SelectedObject = Rotating;
            else if (method == PolygonModelingMethodEnum.Thickening)
                propertyGrid1.SelectedObject = Thickening;
            else propertyGrid1.SelectedObject = Extending;
        }

        private void Export_Click(object sender, EventArgs e)
        {
            if (obj == null) return;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PLY model file (*.ply)|*.ply|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    if( !obj.SaveAsPLY(dlg.FileName))
                    {
                        MessageBox.Show("Failed to save 3D Object.\n" + obj.errMessage);
                    }
                    else MessageBox.Show("Object saved successfully to file:\n" + dlg.FileName);
                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (polygon != null)
            {
                DrawPolygon(e.Graphics, polygon);
                DrawTriangles(e.Graphics, triobj);
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void polygon2DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (polygon == null) return;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Polygon files(*.poly;*.dat,*.csv,*.txt)|*.poly;*.dat;*.csv;*.txt|all files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK) 
                {
                    if( !polygon.ExportData(dlg.FileName) )
                    {
                        MessageBox.Show("Errors occurred whihe exporting to file.\n" + polygon.errMessage);                        
                    }
                    else MessageBox.Show((AppLocalization.IsChinese ? "数据已导出到文件。\n" : "Data exported to file.\n") + dlg.FileName);
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Polygon files(*.poly;*.dat,*.csv,*.txt)|*.poly;*.dat;*.csv;*.txt|all files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Polygon2D poly = new Polygon2D();
                    if (!poly.ImportData(dlg.FileName))
                    {
                        MessageBox.Show("Errors occurred whihe loading from file.\n" + poly.errMessage);
                    }
                    else 
                    {                        
                        InitPolygon(poly);
                        pictureBox1.Invalidate();
                    }
                }
            }
        }
    }
    public enum PolygonModelingMethodEnum
    {
        Rotating = 0,//旋转造型
        Thickening = 1,//旋转造型
        Extending = 2,//延展造型
    };
    public class PolygonModelingBase
    {
        public int HorizontalSlices { get; set; } = 100;
        public int VerticalSlices { get; set; } = 100;
        public Color Color { get; set; } = Color.Blue;
        public virtual TriangleObj Create(Polygon2D polygon) { return null; }
    }
    public class PolygonRotatingModeling: PolygonModelingBase
    {
        public enum ModelingRotateAxis 
        {
            Left = 0,
            Center = 1,
            Right = 2,
        };
        public double StartAngle { get; set; } = 0;
        public double EndAngle { get; set; } = 360;
        public int nDivided { get; set; } = 36;        
        public ModelingRotateAxis Axis { get; set; } =  ModelingRotateAxis.Right;
        public string Tips 
        { 
            get 
            {
                if(Axis == ModelingRotateAxis.Right)
                {
                    return "最右侧为旋转轴，沿顺时针旋转0-360度";
                }
                else if (Axis == ModelingRotateAxis.Right)
                {
                    return "最左侧为旋转轴，沿逆时针旋转0-360度";
                }
                else
                {
                    return "以中心为旋转轴，沿顺时针旋转0-180度";
                }
            } 
        }
        bool IsPointsOrderDownToUp(Polygon2D polygon)
        {
            //点顺序，由下至上绘制points order --> down to up
            bool down_to_up = false;
            double y1 = 0, y2 = 0;
            int id1 = 0, id2 = 0;
            for (int i = 0; i < polygon.points.Count; i++)
            {
                if (i == 0)
                {
                    y1 = y2 = polygon.points[i].Y;
                    id1 = id2 = 0;
                }
                else
                {
                    if (polygon.points[i].Y < y1)
                    {
                        y1 = polygon.points[i].Y;
                        id1 = i;
                    }
                    if (polygon.points[i].Y > y2)
                    {
                        y2 = polygon.points[i].Y;
                        id2 = i;
                    }
                }
            }

            if (id2 > id1) down_to_up = true;
            else down_to_up = false;
            return down_to_up;
        }

        public override TriangleObj Create(Polygon2D polygon)
        {
            TriangleObj obj = new TriangleObj();
            if (Axis == ModelingRotateAxis.Right)
                obj = CreateOnRightRotate(polygon);
            else if (Axis == ModelingRotateAxis.Left)
                obj = CreateOnLeftRotate(polygon);
            else obj = CreateOnCenterRotate(polygon);
            obj.uniformColor = Color;
            obj.IsUniformColor = true;
            obj.Normalize();
            
            double xx = obj.XWidth / obj.MaxWidth / 2.0;
            double yy = obj.YWidth / obj.MaxWidth / 2.0;
            double zz = obj.ZWidth / obj.MaxWidth / 2.0;
            obj.ScaledToRange(-xx, -yy, -zz, xx, yy, zz);

            obj.UpdateRange();
            return obj;
        }//Create(Polygon2D polygon)
        /// <summary>
        /// 旋转轴在最右边，顺时针转
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public TriangleObj CreateOnRightRotate(Polygon2D polygon)
        {
            double x1 = polygon.Minx;
            double x2 = polygon.Maxx;
            double a, x, y, z, r;
            float tx, ty;
            TriangleObj obj = new TriangleObj();
            Vector64 p;
            double step = (EndAngle - StartAngle) / nDivided;
            int n = polygon.points.Count;
            for (int i = 0; i < n; i++)
            {
                p = polygon.points[i];
                r = p.x - x2;
                z = p.y;
                ty = (float)i / (n - 1);
                for (int k = 0; k < nDivided; k++)//顺时针
                {
                    a = StartAngle + k * step;
                    x = r * Math.Cos(Vector64.toRad(180 - a));
                    y = r * Math.Sin(Vector64.toRad(180 - a));
                    obj.AddPoint(x, y, z, z);
                    tx = (float)k / (nDivided-1);
                    obj.AddTexture(tx,ty);
                }
            }
            
            bool downtoup = IsPointsOrderDownToUp(polygon);

            // 1---2 内侧面
            // 3---4 内侧面            
            int id1, id2, id3, id4;
            for (int i = 0; i < n - 1; i++)//
            {
                for (int j = 0; j < nDivided; j++)
                {
                    id1 = i * (nDivided) + j;
                    id2 = id1 + 1;
                    id3 = id1 + (nDivided);
                    id4 = id2 + (nDivided);
                    if ( downtoup )
                    {
                        obj.AddTriangleIndex(id1, id3, id2);
                        obj.AddTriangleIndex(id4, id2, id3);
                    }
                    else
                    {
                        obj.AddTriangleIndex(id1, id2, id3);
                        obj.AddTriangleIndex(id4, id3, id2);
                    }
                }
            }
            
            return obj;
        }//Create(Polygon2D polygon)

        /// <summary>
        /// 旋转轴在最左边，逆时针转
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public TriangleObj CreateOnLeftRotate(Polygon2D polygon)
        {
            double x1 = polygon.Minx;
            double x2 = polygon.Maxx;
            double a, x, y, z, r;
            float tx, ty;
            TriangleObj obj = new TriangleObj();
            Vector64 p;
            double step = (EndAngle - StartAngle) / nDivided;
            int n = polygon.points.Count;
            for (int i = 0; i < n; i++)
            {
                p = polygon.points[i];
                r = p.x - x1;
                z = p.y;
                ty = (float)i / (n - 1);
                for (int k = 0; k < nDivided; k++)//逆时针
                {
                    a = StartAngle + k * step;
                    x = r * Math.Cos(Vector64.toRad(a));
                    y = r * Math.Sin(Vector64.toRad(a));
                    obj.AddPoint(x, y, z, z);
                    tx = (float)k / (nDivided-1);
                    obj.AddTexture(tx, ty);
                }
            }
            bool downtoup = IsPointsOrderDownToUp(polygon);
            // 2---1 内侧面
            // 4---3 内侧面
            int id1, id2, id3, id4;
            for (int i = 0; i < n - 1; i++)//
            {
                for (int j = 0; j < nDivided; j++)
                {
                    id1 = i * (nDivided) + j;
                    id2 = id1 + 1;
                    id3 = id1 + (nDivided);
                    id4 = id2 + (nDivided);
                    if (downtoup)
                    {
                        obj.AddTriangleIndex(id1, id2, id3);
                        obj.AddTriangleIndex(id4, id3, id2);
                    }
                    else
                    {
                        obj.AddTriangleIndex(id1, id3, id4);
                        obj.AddTriangleIndex(id4, id2, id1);
                    }                    
                }
            }            
            return obj;
        }//Create(Polygon2D polygon)

        /// <summary>
        /// 旋转轴在最中心边，逆时针转（不完善）
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public TriangleObj CreateOnCenterRotate(Polygon2D polygon)
        {
            double x1 = polygon.Minx;
            double x2 = polygon.Maxx;
            double a, x, y, z, r;
            float tx, ty;
            TriangleObj obj = new TriangleObj();
            Vector64 p;
            double step = (EndAngle - StartAngle) / nDivided;
            int n = polygon.points.Count;
            for (int i = 0; i < n; i++)
            {
                p = polygon.points[i];
                r = p.x - ( x1 + x2 ) / 2.0;
                z = p.y;
                ty = (float)i / (n - 1);
                for (int k = 0; k < nDivided; k++)//逆时针
                {
                    a = StartAngle + k * step;
                    x = r * Math.Cos(Vector64.toRad(a));
                    y = r * Math.Sin(Vector64.toRad(a));
                    obj.AddPoint(x, y, z, z);
                    tx = (float)k / (nDivided - 1);
                    obj.AddTexture(tx, ty);
                }
            }

            // 2---1 内侧面
            // 4---3 内侧面            
            //    000000
            //   0|   0|
            //  000000 |
            //  | |  | |
            //  | 000000
            //  |0   |0
            //  000000
            int id1, id2, id3, id4;
            for (int i = 0; i < n - 1; i++)//
            {
                for (int j = 0; j < nDivided; j++)
                {
                    id1 = i * (nDivided) + j;
                    id2 = id1 + 1;
                    id3 = id1 + (nDivided);
                    id4 = id2 + (nDivided);
                    obj.AddTriangleIndex(id1, id3, id4);
                    obj.AddTriangleIndex(id4, id2, id1);
                }
            }
            return obj;
        }//Create(Polygon2D polygon)
    }
    public class PolygonThickeningModeling: PolygonModelingBase
    {
        public double _Width = 0;
        public double _Height = 0;
        public double Width { get { return _Width; } }
        public double Height { get { return _Height; } }
        public double Depth { get; set; } = 0;
        public bool FrontFace { get; set; } = true;
        public bool BackFace { get; set; } = true;
        public Color FrontFaceColor { get; set; } = Color.White;
        public Color BackFaceColor { get; set; } = Color.White;
        public Color SurfaceColor { get { return Color; } set { Color = value; } }
        /// <summary>
        /// 是否顺时针方向
        /// </summary>
        /// <returns></returns>
        public bool IsCounterClockwise(Vector32 p1, Vector32 p2, Vector32 p3)
        {
            var result = (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);
            return result > 0;
        }
        public override TriangleObj Create(Polygon2D polygon)
        {
            double x1 = polygon.Minx;
            double x2 = polygon.Maxx;
            double y1 = polygon.Miny;
            double y2 = polygon.Maxy;
            double x, y, z;
            double tx, ty;
            TriangleObj obj = new TriangleObj();
            Vector64 p;
            
            int n = polygon.points.Count;
            y = 0;
            for (int i = 0; i < n; i++)
            {
                p = polygon.points[i];
                x = p.X;
                z = p.Y;                
                tx = (x - x1) / (x2 - x1);
                ty = (z - y1) / (y2 - y1);
                obj.AddPoint(x, y, z, z);
                obj.AddPointColor(Color);
                obj.AddTexture((float)tx, (float)ty);
            }
            y = Depth;
            for (int i = 0; i < n; i++)
            {
                p = polygon.points[i];
                x = p.X;
                z = p.Y;                
                tx = (x - x1) / (x2 - x1);
                ty = (z - y1) / (y2 - y1);
                obj.AddPoint(x, y, z, z);
                obj.AddPointColor(SurfaceColor);
                obj.AddTexture((float)tx, (float)ty);
            }

            ClockDirection clockwise = polygon.CalculateClockDirection(false);
            // 3-4-6
            //  \ \ \ 顺时针
            //   1-2-5
            // 6-4-3
            //  \ \ \ 逆时针
            //   5-2-1
            int id1, id2, id3, id4;
            for (int i = 0; i < n; i++)//
            {
                id1 = i;                
                id2 = id1 + 1;
                if (i == n - 1) id2 = 0;
                id3 = id1 + n;
                id4 = id2 + n;
                //顺时针方向
                if( clockwise == ClockDirection.Clockwise)
                {
                    obj.AddTriangleIndex(id1, id2, id3);
                    obj.AddTriangleIndex(id4, id3, id2);
                }
                else//逆时针方向
                {
                    obj.AddTriangleIndex(id1, id3, id2);
                    obj.AddTriangleIndex(id2, id3, id4);
                }
            }
            
            PolygonTriangulate tri = new PolygonTriangulate(polygon);
            //TriangleObj obj1 = tri.TriangulateConvexPolySimple(polygon);
            TriangleObj obj1 = tri.Triangulate(polygon);

            int start = obj.points.Count;
            //Front face，三角形转逆时针方向            
            for (int i = 0; i < obj1.points.Count; i++ )
            {
                x = obj1.points[i].X;
                y = 0;
                z = obj1.points[i].y;
                obj.AddPoint(x,y,z,z);
                obj.AddPointColor(FrontFaceColor);
                tx = (x - x1) / (x2 - x1);
                ty = (z - y1) / (y2 - y1);
                obj.AddTexture((float)tx, (float)ty);
            }            
            for (int i=0;i<obj1.triangles.Count;i++)
            {
                id1 = obj1.triangles[i].x;
                id2 = obj1.triangles[i].y;
                id3 = obj1.triangles[i].z;
                if( IsCounterClockwise(obj1.points[id1], obj1.points[id2], obj1.points[id3]) )
                   obj.AddTriangleIndex(id3 + start, id2 + start, id1 + start);
                else obj.AddTriangleIndex(id1 + start, id2 + start, id3 + start);
            }
            start = obj.points.Count;
            //back face ---- 三角形转顺时针方向           
            for (int i = 0; i < obj1.points.Count; i++)
            {
                x = obj1.points[i].X;
                y = Depth;
                z = obj1.points[i].y;
                obj.AddPoint(x, y, z, z);
                tx = (x - x1) / (x2 - x1);
                ty = (z - y1) / (y2 - y1);
                obj.AddTexture((float)tx, (float)ty);
                obj.AddPointColor(BackFaceColor);
            }
            for (int i = 0; i < obj1.triangles.Count; i++)
            {
                id1 = obj1.triangles[i].x;
                id2 = obj1.triangles[i].y;
                id3 = obj1.triangles[i].z;
                if ( !IsCounterClockwise(obj1.points[id1], obj1.points[id2], obj1.points[id3]) )
                    obj.AddTriangleIndex(id3 + start, id2 + start, id1 + start);
                else obj.AddTriangleIndex(id3 + start, id2 + start, id1 + start);
            }
            
            obj.uniformColor = SurfaceColor;
            obj.IsUniformColor = false;
            obj.Normalize();

            double xx = obj.XWidth / obj.MaxWidth/2.0;
            double yy = obj.YWidth / obj.MaxWidth/2.0;
            double zz = obj.ZWidth / obj.MaxWidth/2.0;
            obj.ScaledToRange(-xx, -yy, -zz, xx, yy, zz);

            obj.UpdateRange();
            return obj;
        }
    }
    public class PolygonExtendingModeling : PolygonModelingBase
    {
        public double _Width = 0;
        public double _Height = 0;
        public double Width { get { return _Width; } }
        public double Height { get { return _Height; } }
        public double ExtendLength { get; set; } = 0;//延展距离,向Y方向延展
        public override TriangleObj Create(Polygon2D polygon)
        {
            double x1 = polygon.Minx;
            double x2 = polygon.Maxx;
            double y1 = polygon.Miny;
            double y2 = polygon.Maxy;
            double x, y, z;
            double tx, ty;
            TriangleObj obj = new TriangleObj();
            Vector64 p;

            int n = polygon.points.Count;
            for (int i = 0; i < n; i++)
            {
                p = polygon.points[i];
                x = p.X;
                z = p.Y;
                y = 0;
                tx = (x - x1) / (x2 - x1);
                ty = (z - y1) / (y2 - y1);
                obj.AddPoint(x, y, z, z);
                obj.AddPoint(x, y + ExtendLength, z, z);
                obj.AddTexture((float)tx, (float)ty);
                obj.AddTexture((float)tx, (float)ty);
            }
            ClockDirection clockwise = polygon.CalculateClockDirection(false);
            // 2-4-6
            //  \ \ \ 顺时针
            //   1-3-5
            // 6-4-2
            //  \ \ \ 逆时针
            //   5-3-1
            int id1, id2, id3, id4;
            int nn = n - 1;
            if (polygon.IsClosed) nn = n;
            for (int i = 0; i < nn; i++)//
            {
                id1 = 2 * i;
                id2 = id1 + 1;
                id3 = 2 * (i + 1);
                id4 = id3 + 1;
                
                if ( i == n - 1) { id3 = 0; id4 = 1; }

                if (clockwise == ClockDirection.Clockwise)
                {
                    obj.AddTriangleIndex(id1, id3, id2);
                    obj.AddTriangleIndex(id4, id2, id3);
                }
                else
                {
                    obj.AddTriangleIndex(id1, id2, id3);
                    obj.AddTriangleIndex(id4, id3, id2);
                }
            }

            obj.uniformColor = Color;
            obj.IsUniformColor = true;
            obj.Normalize();

            double xx = obj.XWidth / obj.MaxWidth / 2.0;
            double yy = obj.YWidth / obj.MaxWidth / 2.0;
            double zz = obj.ZWidth / obj.MaxWidth / 2.0;
            obj.ScaledToRange(-xx, -yy, -zz, xx, yy, zz);

            obj.UpdateRange();
            return obj;
        }
    }
    public class SymbolMethod
    {
        public C2DPolygons polygon = new C2DPolygons();
        PolygonModelingMethodEnum method = PolygonModelingMethodEnum.Rotating;
        public double Width 
        {
            get { return polygon.Maxx - polygon.Minx; }
        }
        public double Height
        {
            get { return polygon.Maxz - polygon.Minz; }
        }
    }
}
