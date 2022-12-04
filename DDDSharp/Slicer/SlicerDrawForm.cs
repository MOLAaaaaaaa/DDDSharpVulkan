using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
namespace DDDSharp
{
    public enum AxisDirection
    {
        LeftUp = 0,
        LeftDown = 1
    };
    public enum DrawMode
    {
        None = 0,
        DrawBaseLine = 1
    }
    public partial class SlicerDrawForm : Form
    {
        RectRuler ruler = new RectRuler();
        public C3DGridData data;
        public List<CSlicer> pSlicers = new List<CSlicer>();
        private CSlicer curSlicer;

        Point cursorPos = new Point(-1,-1);
        private int Position = 0;
        public planEnum plan = planEnum.XOY;
        private double minx, maxx, miny, maxy, minz, maxz;
        private double xstep, ystep, zstep;
        string xMark = "x";
        string yMark = "y";
        private int xNum, yNum, zNum, xyNum;

        // 0 left right is x direction bottom - top is y direction 
        // 1 left right is y direction bottom - top is x direction  
        AxisDirection xyDirection = AxisDirection.LeftUp;
        bool xyReverse = false;

        private int topMargine = 10;
        private int leftMargine = 10;
        private int bottomMargine = 40;
        private int rightMargine = 60;

        private bool bShowBoreholeName = true;
        private DrawMode nDrawMode = DrawMode.None;
        private Point vp1, vp2, lastpoint;

        private Bitmap bmp = null;
        Rectangle DrawRect;
        DoubleRect DataRect;
        int leftMargin = 50;
        int rightMargin = 10;
        int topMargin = 10;
        int bottomMargin = 50;
        double dotSize = 12;
        CColorScale colorscale = new CColorScale();
        //mouses
#pragma warning disable CS0414 // 字段“SlicerDrawForm.bMouseDown”已被赋值，但从未使用过它的值
        bool bMouseDown = false;
#pragma warning restore CS0414 // 字段“SlicerDrawForm.bMouseDown”已被赋值，但从未使用过它的值
        private bool IsControlDown = false;
        Point first, second;
        ToolTip toolTip1 = new ToolTip();

        int nSelected = -1;

        int playerDelay = 200;
        bool playerAsc = true;

        delegate void PlayUI(int value);

        public SlicerDrawForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.          
            KeyPreview = true;
            toolTip1.AutoPopDelay = 2000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);
        }
        public void UpdateDataRect()
        {
            DrawRect = new Rectangle(leftMargin, topMargin,
               pictureBox1.Width - leftMargin - rightMargin,
                pictureBox1.Height - topMargin - bottomMargin);

            double x1, x2, y1, y2;
            if (plan == planEnum.XOY)
            {
                x1 = minx;
                x2 = maxx;
                y1 = miny;
                y2 = maxy;
            }
            else if (plan == planEnum.XOZ)
            {
                x1 = minx;
                x2 = maxx;
                y1 = minz;
                y2 = maxz;
            }
            else //if (plan == planEnum.YOZ)
            {
                x1 = miny;
                x2 = maxy;
                y1 = minz;
                y2 = maxz;
            }

            //居中显示
            double xx = x2 - x1; //Logical Length horiz
            double yy = y2 - y1; //Logical Length vertical
            double scale = (double)DrawRect.Height / (double)DrawRect.Width;
            double len;
            if ( xx * scale >= yy )
            {
                DataRect.x1 = x1;
                DataRect.x2 = x2;
                len = (xx * scale - yy) / 2.0;
                DataRect.y1 = y1 - len;
                DataRect.y2 = y2 + len;
            }
            else
            {
                DataRect.y1 = y1;
                DataRect.y2 = y2;
                len = (yy / scale - xx) / 2.0;
                DataRect.x1 = x1 - len;
                DataRect.x2 = x2 + len;
            }
        }
        public void SetDataGrid(C3DGridData _data)
        {
            data = _data;
            minx = data.minx;
            maxx = data.maxx;
            miny = data.miny;
            maxy = data.maxy;
            minz = data.minz;
            maxz = data.maxz;
            xNum = data.xNum;
            yNum = data.yNum;
            zNum = data.zNum;
            xyNum = xNum * yNum;
            xstep = (maxx - minx) / (xNum - 1);
            ystep = (maxy - miny) / (yNum - 1);
            zstep = (maxz - minz) / (zNum - 1);
        }
        void LPtoDP(ref double x, ref double y)
        {
            double x1 = ruler.bottomRuler.Minimum;
            double x2 = ruler.bottomRuler.Maximum;
            double y1 = ruler.leftRuler.Minimum;
            double y2 = ruler.leftRuler.Maximum;
            if (x1 >= x2 || y1 >= y2) return;

            if ( plan == planEnum.XOZ )
            {   //       z
                //       |
                //x______|o
                x = DrawRect.Right - DrawRect.Width * (x - x1) / (x2 - x1);
                y = DrawRect.Bottom - DrawRect.Height * (y - y1) / (y2 - y1);
            }
            else
            {
                x = DrawRect.Left + DrawRect.Width * (x - x1) / (x2 - x1);
                y = DrawRect.Bottom - DrawRect.Height * (y - y1) / (y2 - y1);
            }
        }
        void DPtoLP(ref double x, ref double y)
        {   
            double x1 = ruler.bottomRuler.Minimum;
            double x2 = ruler.bottomRuler.Maximum;
            double y1 = ruler.leftRuler.Minimum;
            double y2 = ruler.leftRuler.Maximum;
            if (x1 >= x2 || y1 >= y2) return;

            if (plan == planEnum.XOZ)
            {   //       z
                //       |
                //x______|o
                x = x2 -  (x - DrawRect.Left) / DrawRect.Width * (x2 - x1);
                y = y1 + (DrawRect.Bottom - y) / DrawRect.Height *(y2-y1);
            }
            else
            {
                x = x1 + (x - DrawRect.Left) / DrawRect.Width * (x2 - x1);
                y = y1 + (DrawRect.Bottom - y) / DrawRect.Height * (y2 - y1);
            }
        }
        private void SlicerDrawForm_Load(object sender, EventArgs e)
        {
            //	z     y
            //	|   /
            //	| / 
            //  O ------->x            
            comboBox1.Items.Clear();
            comboBox1.Items.Add(planEnum.XOY.ToString());
            comboBox1.Items.Add(planEnum.XOZ.ToString());
            comboBox1.Items.Add(planEnum.YOZ.ToString());
            comboBox1.SelectedIndex = 0;
            Position = data.zNum - 1;
            plan = planEnum.XOY;
            numericUpDown1.Value = Position;
            XPosTextBox.Text = "0";
            YPosTextBox.Text = "0";
            ZPosTextBox.Text = "0";
            XRangeLabel.Text = "0 - " + (data.xNum - 1);
            YRangeLabel.Text = "0 - " + (data.yNum - 1);
            ZRangeLabel.Text = "0 - " + (data.zNum - 1);

            propertyGrid1.SelectedObject = null;
        }

        private void UpdateList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < pSlicers.Count; i++)
            {
                listBox1.Items.Add(pSlicers[i].Name);
            }
            listBox1.SelectedIndex = nSelected;
        }

        private void drawSlicerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ready to draw slicer
            nDrawMode = DrawMode.DrawBaseLine;
            this.Cursor = Cursors.Cross;

            if (plan == planEnum.XOY)
                curSlicer = new CSlicer(zNum, xNum);
            else if (plan == planEnum.YOZ)
                curSlicer = new CSlicer(xNum, yNum);
            else //if (plan == planEnum.XOZ)
                curSlicer = new CSlicer(yNum, zNum);
            curSlicer.Plan = plan;            
            first = second = new Point(-1, -1);
        }
        private void AddToCurBaseLine(Point p)
        {
            double x, y, z;

            if (plan == planEnum.XOY)
            {
                x = p.X;
                y = p.Y;
                z = maxz;
                DPtoLP(ref x, ref y);
            }            
            else if (plan == planEnum.YOZ)
            {
                y = p.X;
                z = p.Y;
                x = maxx;
                DPtoLP(ref y, ref z);
            }
            else //if (plan == planEnum.XOZ)
            {
                x = p.X;
                z = p.Y;
                y = maxy;
                DPtoLP(ref x, ref z);
            }
            curSlicer.AddBaseLine(x, y, z);
        }
        bool IsSlicerExist(string name)
        {
            for (int i = 0; i < pSlicers.Count; i++)
            {
                if (pSlicers[i].Name == name)
                    return true;
            }
            return false;
        }
        //axis = 0 x,1 y,2 z
        private bool AddAxisSlicer(int axis, int pos)
        {
            CSlicer slicer = null;
            if (axis == 0)//x axis y-z plan
            {
                slicer = new CSlicer(data.zNum, data.yNum);
                slicer.CopyTransformFrom(data);

                slicer.Name = "Axis X" + pos;
                slicer.Plan = planEnum.XOY;
                double z = maxz;
                double x = minx + pos * xstep;
                double y1 = miny;
                double y2 = (miny + maxy) / 2;
                double y3 = maxy;
                slicer.AddBaseLine(x, y1, z);
                slicer.AddBaseLine(x, y2, z);
                slicer.AddBaseLine(x, y3, z);
                slicer.minHeight = minz;
                slicer.maxHeight = maxz;
            }
            else if (axis == 1)//y axis
            {
                slicer = new CSlicer(data.zNum, data.xNum);
                slicer.Name = "Axis Y" + pos;
                slicer.Plan = planEnum.XOY;
                double z = maxx;
                double y = miny + pos * ystep;
                double x1 = minx;
                double x2 = (minx + maxx) / 2;
                double x3 = maxx;
                slicer.AddBaseLine(x1, y, z);
                slicer.AddBaseLine(x2, y, z);
                slicer.AddBaseLine(x3, y, z);

                slicer.minHeight = minz;
                slicer.maxHeight = maxz;
            }
            else if (axis == 2)//z axis
            {
                slicer = new CSlicer(data.yNum, data.xNum);
                slicer.Name = "Axis Z" + pos;
                slicer.Plan = planEnum.YOZ;
                double x = maxx;
                double z = minz + pos * zstep;
                double y1 = miny;
                double y2 = (miny + maxy) / 2;
                double y3 = maxy;

                slicer.AddBaseLine(x, y1, z);
                slicer.AddBaseLine(x, y2, z);
                slicer.AddBaseLine(x, y3, z);

                slicer.minHeight = minx;
                slicer.maxHeight = maxx;
            }
            if (IsSlicerExist(slicer.Name))
            {
                MessageBox.Show("the same name slicer existed.");
                return false;
            }

            if (slicer.CreateSlicer(slicer.Plan, data))
            {
                //
                slicer.CopyTransformFrom(data);
                pSlicers.Add(slicer);
                nSelected = pSlicers.Count - 1;
                UpdateList();
                UpdateDraw();
            }
            return true;
        }
        private void AddSlicer(bool updatedraw = true)
        {            
            curSlicer.RemoveDuplicated();
            if (curSlicer.pBaseLine.Count < 2)
            {
                MessageBox.Show("no enough valid points.");
                return;
            }
            if (curSlicer.CreateSlicer(plan, data))
            {
                //修改Slicer边界，以适应高度方向上的变化
                //modified by jian 2020-12-3
                curSlicer.CopyTransformFrom(data);

                pSlicers.Add(curSlicer);
                nSelected = pSlicers.Count - 1;
                if (updatedraw)
                {
                    UpdateList();
                    UpdateDraw();
                }
            }
        }
      
        private bool SeekSection(string section, ref StreamReader sr)
        {
            string str;
            while ((str = sr.ReadLine()) != null)
            {
                str = str.Trim(' ');
                if (str.Length < 1) continue;
                if (str == section) return true;
            }
            return false;
        }

        private string GetLineValue(string str, string name)
        {
            string[] ss = str.Split(new Char[] { '=', '=' }, 2);
            if (ss.Length < 2) return "";

            string s1 = ss[0].Trim(' ');
            if (s1.ToLower() != name.ToLower()) return "";

            return ss[1].Trim(' ');
        }

        private bool ExportSlicer(CSlicer slicer, string file)
        {
            FileStream fs = new FileStream(file, FileMode.Create);
            StreamWriter wr = new StreamWriter(fs);

            string header = "Grid Slicer Line";
            wr.WriteLine(header);

            string str = "[HEADER]";
            wr.WriteLine(str);
            str = "Name = " + slicer.Name;
            wr.WriteLine(str);

            str = "[BASE LINE]";
            wr.WriteLine(str);
            str = "Count = " + slicer.pBaseLine.Count;
            wr.WriteLine(str);

            for (int i = 0; i < slicer.pBaseLine.Count; i++)
            {
                str = slicer.pBaseLine[i].X + "," + slicer.pBaseLine[i].Y + "," + slicer.pBaseLine[i].Z;
                wr.WriteLine(str);
            }

            str = "[GRID]";
            wr.WriteLine(str);
            str = "Row = " + slicer.nRow;
            wr.WriteLine(str);
            str = "Column = " + slicer.nCol;
            wr.WriteLine(str);
            str = "title = x,y,z,v,dist,depth";
            wr.WriteLine(str);
            Vector32 p, p0, oldp0 = new Vector32();
            double dist = 0, depth = 0;
            for (int i = 0; i < slicer.nRow; i++)
            {
                dist = 0;
                p0 = slicer.pData[i * slicer.nCol];
                if (i > 0) depth = p0.Distance(oldp0);
                for (int j = 0; j < slicer.nCol; j++)
                {
                    p = slicer.pData[i * slicer.nCol + j];
                    dist += p.Distance(p0);
                    str = p.X + "," + p.Y + "," + p.Z + "," + p.V + "," + dist + "," + depth;
                    wr.WriteLine(str);
                }
                oldp0 = p0;
            }
            str = "[END]";
            wr.WriteLine(str);

            wr.Close();
            fs.Close();
            return true;
        }
        private bool LoadSlicer(string file)
        {
            curSlicer = new CSlicer(data.xNum, data.yNum);
            curSlicer.Clear();
            curSlicer.minx = data.minx;
            curSlicer.miny = data.miny;
            curSlicer.minz = data.minz;
            curSlicer.minv = data.minv;
            curSlicer.maxx = data.maxx;
            curSlicer.maxy = data.maxy;
            curSlicer.maxz = data.maxz;
            curSlicer.maxv = data.maxv;
            if (!curSlicer.ImportData(file)) 
            {
                return false; 
            }
            if (curSlicer.pBaseLine.Count > 0)
            {
                AddSlicer(false);
                // curSlicer = null;
                return true;
            }
            else return false;
        }
        private void ExportSlicerButton_Click(object sender, EventArgs e)
        {
            if (pSlicers.Count < 1) return;
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Slicer ACSII file (*.dat)|*.dat|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    string pathname = dlg.FileName;
                    //if (ExportSlicer(pSlicers[sel], pathname))
                    if (pSlicers[sel].ExportSlicer(pathname))
                    {
                        MessageBox.Show("data exported to file:" + pathname);
                    }
                    else
                    {
                        MessageBox.Show("failed to export data to file:" + pathname);
                    }
                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void slicerPlanToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            xOYToolStripMenuItem.Checked = false;
            xOZToolStripMenuItem.Checked = false;
            yOZToolStripMenuItem.Checked = false;

            if (plan == planEnum.XOY)
            {
                xOYToolStripMenuItem.Checked = true;
            }
            if (plan == planEnum.XOZ)
            {
                xOZToolStripMenuItem.Checked = true;
            }
            if (plan == planEnum.YOZ)
            {
                yOZToolStripMenuItem.Checked = true;
            }
        }

        private bool CheckWrong()
        {
            bool wrong = false;
            Position = Convert.ToInt32(numericUpDown1.Value);

            if (plan == planEnum.XOY)
            {
                if (Position < 0 || Position >= data.zNum)
                    wrong = true;
            }
            if (plan == planEnum.XOZ)
            {
                if (Position < 0 || Position >= data.yNum)
                    wrong = true;
            }
            if (plan == planEnum.YOZ)
            {
                if (Position < 0 || Position >= data.xNum)
                    wrong = true;
            }
            return wrong;
        }
        /*
        private void UpdatePlanbutton1_Click(object sender, EventArgs e)
        {
            if (CheckWrong())
            {
                MessageBox.Show("wrong number of position.");
                return;
            }            
            UpdateDraw();
        }*/

        void OnPlanSet()
        {
            if (data == null) return;
            if (pictureBox1 == null) return;

            int sel = comboBox1.SelectedIndex;
            if (sel < 0) return;

            plan = (planEnum)sel;
            if (plan == planEnum.XOY)
            {
                if (Position < 0) Position = 0;
                if (Position > zNum - 1) Position = zNum - 1;
                xMark = "x";
                yMark = "y";
                numericUpDown1.Minimum = 0;
                numericUpDown1.Maximum = data.zNum - 1;
                PositionRangeLabel.Text = "0 - " + (data.zNum - 1);
            }
            else if (plan == planEnum.XOZ)
            {
                if (Position < 0) Position = 0;
                if (Position > yNum - 1) Position = yNum - 1;
                xMark = "x";
                yMark = "z";
                numericUpDown1.Minimum = 0;
                numericUpDown1.Maximum = data.yNum - 1;
                PositionRangeLabel.Text = "0 - " + (data.yNum - 1);
            }
            else if (plan == planEnum.YOZ)
            {
                if (Position < 0) Position = 0;
                if (Position > xNum - 1) Position = xNum - 1;
                xMark = "y";
                yMark = "z";
                numericUpDown1.Minimum = 0;
                numericUpDown1.Maximum = data.xNum - 1;
                PositionRangeLabel.Text = "0 - " + (data.xNum - 1);
            }
            numericUpDown1.Value = Position;
            UpdateDataRect();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnPlanSet();
            UpdateDraw();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Slicer ACSII file (*.dat;*.txt)|*.dat;*.txt;|all files(*.*)|*.*";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string filename;
                    string errInfo = "load slicer faild!";
                    int err = 0;
                    this.Cursor = Cursors.WaitCursor;
                    for (int i = 0; i < dlg.FileNames.Length; i++)
                    {
                        filename = dlg.FileNames[i];
                        if ( !LoadSlicer(filename) )
                        {
                            errInfo += filename + "\n";
                            err++;
                        }
                    }
                    if( err > 0 ) MessageBox.Show(errInfo);

                    UpdateList();
                    UpdateDraw();

                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            int width = pictureBox1.Width;
            int height = pictureBox1.Height;
            if (width < 1 || height < 1) return;

            DrawRect = new Rectangle(leftMargin, topMargin,
                width - leftMargin - rightMargin,
                height - topMargin - bottomMargin);
            if (bmp != null)
            {
                bmp.Dispose();
                bmp = null;
            }
            bmp = new Bitmap(width, height);
            UpdateDraw();

            /*
            if (pictureBox1 == null) return;
            if (bmp != null) bmp.Dispose();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            OnPlanSet();
            ResetPara();
            pictureBox1.Invalidate(true);
            */
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (nDrawMode == DrawMode.DrawBaseLine) // in a state to draw
            {
                if (e.Button == MouseButtons.Left)
                {
                    bMouseDown = true;

                    if (IsControlDown && (second.X != first.X || second.Y != first.Y))
                        first = second;
                    else first = new Point(e.X,e.Y);
                    
                    AddToCurBaseLine(first);
                    second = first;                    
                }
                else if (e.Button == MouseButtons.Right)//right button to end draw
                {
                    if (curSlicer.pBaseLine.Count > 0)
                    {
                        NewSlicerConfirmForm cs = new NewSlicerConfirmForm();
                        if (cs.ShowDialog() == DialogResult.OK)
                        {
                            curSlicer.Name = cs.slicerName;
                            curSlicer.Closed = cs.IsClosed;
                            curSlicer.Trim(data.minx, data.miny, data.minz, data.maxx, data.maxy, data.maxz);
                            if (cs.IsSmooth)
                            {
                                curSlicer.CreateSmoothBaseLine();
                            }
                            else
                            {
                                if (curSlicer.Closed)
                                {
                                    // add first point to the end 
                                    curSlicer.AddBaseLine(curSlicer.pBaseLine[0]);
                                }
                            }
                            AddSlicer();

                            // curSlicer.Clear();
                            // curSlicer = null;
                        }//if (cs.ShowDialog() == DialogResult.OK)
                        else pictureBox1.Invalidate();

                        nDrawMode = DrawMode.None;
                        bMouseDown = false;
                        Cursor = Cursors.Default;
                    }//if (curSlicer.pBaseLine.Count > 0)
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            bMouseDown = false;
        }
        private void UpdateInfo(double x,double y)
        {
            double x1 = x;
            double y1 = y;
            DPtoLP(ref x1, ref y1);

            if (plan == planEnum.XOY)
            { 
                InfoLabel.Text = "X = " + x1 + ",  " + "Y = " + y1; 
            }
            if (plan == planEnum.XOZ)
            {
                InfoLabel.Text = "X = " + x1 + ",  " + "Z = " + y1;
            }
            if (plan == planEnum.YOZ)
            {
                InfoLabel.Text = "Y = " + x1 + ",  " + "Z = " + y1;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (nDrawMode == DrawMode.DrawBaseLine)
            {
                second = new Point(e.X, e.Y);
                if (IsControlDown)
                {
                    int xx = Math.Abs(e.X - first.X);
                    int yy = Math.Abs(e.Y - first.Y);
                    if (xx < yy) second = new Point(first.X, e.Y);
                    else second = new Point(e.X, first.Y);
                }
                pictureBox1.Invalidate();
            }
            UpdateInfo(e.X, e.Y);
            
            cursorPos.X = e.X;
            cursorPos.Y = e.Y;
            
            ruler.cursorPosition = e.Location;

            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            ruler.Draw(g);

            if (bmp != null) g.DrawImage(bmp, 0, 0);
            
            if (nDrawMode == DrawMode.DrawBaseLine)
            {
                DrawSlicerBaseLine(curSlicer, g, Pens.Blue);
                if (first.X != second.X && first.X >= 0)
                {
                    g.DrawLine(Pens.Wheat, first, second);
                }
            }

            Point p1 = cursorPos;
            Point p2 = cursorPos;
            p1.X = ruler.drawRect.Left;
            p2.X = ruler.drawRect.Right;
            g.DrawLine(Pens.Gray,p1,p2);
            p1 = p2 = cursorPos;
            p1.Y = ruler.drawRect.Top;
            p2.Y = ruler.drawRect.Bottom;
            g.DrawLine(Pens.Gray, p1, p2);

            double x = cursorPos.X;
            double y = cursorPos.Y;
            DPtoLP(ref x,ref y);

            ruler.DrawCursor(e.Graphics);
        }

        private bool GetGridColor(int ix, int iy, int iz, out ColorRGBA color)
        {
            color = new ColorRGBA();
            if (plan == planEnum.XOY)
            {
                //top to buttom                
                for (int iz1 = iz; iz1 >= 0; iz1--)
                {
                    if (!data.IsBlankedGrid(ix, iy, iz1))
                    {
                        color = data.GetColor(data.GetVerticIndex(ix, iy, iz1));
                        return true;
                    }
                }
            }
            else if (plan == planEnum.XOZ)
            {
                //top to buttom                
                for (int iy1 = iy; iy1 >= 0; iy1--)
                {
                    if (!data.IsBlankedGrid(ix, iy1, iz))
                    {
                        color = data.GetColor(data.GetVerticIndex(ix, iy1, iz));
                        return true;
                    }
                }
            }
            else //if (plan == planEnum.YOZ)
            {
                //top to buttom 
                for (int ix1 = ix; ix1 >= 0; ix1--)
                {
                    if (!data.IsBlankedGrid(ix1, iy, iz))
                    {
                        color = data.GetColor(data.GetVerticIndex(ix1, iy, iz));
                        return true;
                    }
                }
            }
            return false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (CheckWrong())
            {
                MessageBox.Show("wrong number of position.");
                return;
            }
            Position = decimal.ToInt32(numericUpDown1.Value);
            UpdateDraw();
        }

        private void AddX_Click(object sender, EventArgs e)
        {
            int pos = 0;
            if (!int.TryParse(XPosTextBox.Text, out pos))
            {
                MessageBox.Show("index number of X axis not correct.");
                return;
            }
            if (pos < 0 || pos >= xNum)
            {
                MessageBox.Show("index number of X axis not correct.");
                return;
            }
            AddAxisSlicer(0, pos);
        }

        private void AddY_Click(object sender, EventArgs e)
        {
            int pos = 0;
            if (!int.TryParse(YPosTextBox.Text, out pos))
            {
                MessageBox.Show("index number of Y axis not correct.");
                return;
            }
            if (pos < 0 || pos >= yNum)
            {
                MessageBox.Show("index number of Y axis not correct.");
                return;
            }
            AddAxisSlicer(1, pos);
        }

        private void AddZ_Click(object sender, EventArgs e)
        {
            int pos = 0;
            if (!int.TryParse(ZPosTextBox.Text, out pos))
            {
                MessageBox.Show("index number of Z axis not correct.");
                return;
            }
            if (pos < 0 || pos >= zNum)
            {
                MessageBox.Show("index number of Z axis not correct.");
                return;
            }
            AddAxisSlicer(2, pos);
        }

        private void Zoom(double x0, double y0, double scale = 0.8)
        {
            double offx = x0 - (DataRect.x1 + DataRect.x2) / 2.0;
            double offy = y0 - (DataRect.y1 + DataRect.y2) / 2.0;
            DataRect.Offset(offx, offy);
            DataRect.Scale(scale);
        }
        private void Zoom(Point p0, double scale = 0.8)
        {
            double x0 = p0.X;
            double y0 = p0.Y;
            DPtoLP(ref x0, ref y0);
            double offx = x0 - (DataRect.x1 + DataRect.x2) / 2.0;
            double offy = y0 - (DataRect.y1 + DataRect.y2) / 2.0;
            DataRect.Offset(offx, offy);
            DataRect.Scale(scale);
        }
        private void ZoomIn(Point p0, double scale = 0.8)
        {
            Zoom(p0, scale);
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            int n = pSlicers.Count;
            if (n < 1 || sel >= n) return;
            CSlicer slicer = pSlicers[sel];

            if (MessageBox.Show("删除对象：" + slicer.Name, "是否删除该切片对象？", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                pSlicers.RemoveAt(sel);
                nSelected = -1;
                UpdateList();
                UpdateDraw();
            }
        }

        private void SlicerDrawForm_KeyDown(object sender, KeyEventArgs e)
        {
            IsControlDown = e.Control;
        }

        private void SlicerDrawForm_KeyUp(object sender, KeyEventArgs e)
        {
            IsControlDown = e.Control;
        }
        void PlayUpdateUI(int value)
        {
            if( numericUpDown1.InvokeRequired )
            {
                PlayUI play = new PlayUI(PlayUpdateUI);
                this.BeginInvoke(play, value);
            }
            else
            {
                numericUpDown1.Value = value;
            }
        }
        private void PlayThread(object para)
        {
            int n = (int)para;
            
            int delay = playerDelay;

            if (playerAsc) 
            {
                for (int i = 0; i < n; i++)
                {
                    PlayUpdateUI(i);
                    Thread.Sleep(delay);
                }
            }
            else
            {
                for (int i = n-1; i >=0; i--)
                {
                    PlayUpdateUI(i);
                    Thread.Sleep(delay);
                }
            }
            
        }
        private void PlayButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            
            int n = 0;
            if (plan == planEnum.XOY) n = data.zNum;
            if (plan == planEnum.XOZ) n = data.yNum;
            if (plan == planEnum.YOZ) n = data.xNum;

            Thread thread = new Thread(PlayThread);
            thread.Start(n);

            Cursor = Cursors.Default;
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlayerSettingForm pl = new PlayerSettingForm();
            pl.delay = playerDelay;
            pl.asc = playerAsc;
            if( pl.ShowDialog() == DialogResult.OK )
            {
                playerDelay = pl.delay;
                playerAsc = pl.asc;
            }
        }

        private void playToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PlayButton_Click(sender,e);
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            CSlicer slicer = (CSlicer)propertyGrid1.SelectedObject;
            bool update = false;
            
            if( slicer.Smoothed && !curSmoothed ) //做平滑
            {
                curSmoothed = true;
                slicer.CreateSmoothBaseLine();
                update = true;
            }

            if ( slicer.Closed != curClosed ) //封闭
            {
                slicer.AddBaseLine(slicer.pBaseLine[0]); //添加最后一点封闭
                curClosed = slicer.Closed;
                update = true; 
            }
            if ( !slicer.Closed && curClosed ) //解除封闭
            {
                int n = slicer.pBaseLine.Count;
                slicer.pBaseLine.RemoveAt(n-1); //删除最后一点
                curClosed = slicer.Closed;
                update = true;
            }

            if (update)
            {
                pSlicers[nSelected] = slicer;                              
                UpdateDraw();
            }
        }
        //监测控件变量是否改变
        bool curSmoothed = false;
        bool curClosed = false;
        string curName;

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            nSelected = listBox1.SelectedIndex;

            if (nSelected >= 0) 
            {
                CSlicer slicer = pSlicers[nSelected];
                curSmoothed = slicer.Smoothed;
                curClosed = slicer.Closed;
                curName = slicer.Name;

                propertyGrid1.SelectedObject = slicer; 
            }

            UpdateDraw();
        }

        private void ZoomOut(Point p0, double scale = 1.25)
        {
            Zoom(p0, scale);
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
                if (nDrawMode == DrawMode.DrawBaseLine)
                {
                    if (curSlicer != null && curSlicer.pBaseLine.Count > 0)
                    {
                        int n = curSlicer.pBaseLine.Count;
                        first = toDP(curSlicer.pBaseLine[n - 1]);
                    }
                }
                UpdateDraw();
            }
        }
        void DrawGrid(Graphics g)
        {
            if (plan == planEnum.XOY)
            {
                DrawXYGrid(g);
            }
            else if (plan == planEnum.XOZ)
            {
                DrawXZGrid(g);
            }
            else if (plan == planEnum.YOZ)
            {
                DrawYZGrid(g);
            }            
        }
        public void UpdateDraw(bool update = true)
        {
            if (update)
            {
                if (pictureBox1.Width == 0 || pictureBox1.Height == 0) return;
                if (bmp != null)
                {
                    bmp.Dispose();
                    bmp = null;
                }

                ruler.SetDrawRect(DrawRect);
                ruler.drawRect = ruler.windowRect = DrawRect;
                int x = DrawRect.Left - 50;
                int y = DrawRect.Top - 10;
                ruler.windowRect = new Rectangle(x, y, DrawRect.Width + 60, DrawRect.Height + 60);
                ruler.leftRuler.SetValuesRange(DataRect.y1, DataRect.y2);
                ruler.bottomRuler.SetValuesRange(DataRect.x1, DataRect.x2);

                bmp = new Bitmap(ruler.drawRect.Width, ruler.drawRect.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.FillRectangle(Brushes.White, DrawRect);
                g.DrawRectangle(Pens.Black,DrawRect);

                DrawGrid(g);
                DrawSlicerBaseLines(g);
                DrawScatterPoints(g);
            }
            pictureBox1.Invalidate();
        }
        Point toDP(Vector32 p)
        {
            double x, y;
            if (plan == planEnum.XOY)
            {
                x = p.x;
                y = p.y;
            }
            else if (plan == planEnum.YOZ)
            {
                x = p.y;
                y = p.z;
            }
            else //if (slicer.Plan == planEnum.XOZ)
            {
                x = p.x;
                y = p.z;
            }
            LPtoDP(ref x, ref y);
            return new Point((int)x, (int)y);
        }
        private void DrawSlicerBaseLine(CSlicer slicer, Graphics g, Pen pen)
        {
            if (slicer.Plan != plan) return;
            if (slicer.pBaseLine.Count < 2) return;
            
            ArrowRenderer ar = new ArrowRenderer();
            ar.Width = 10;
            ar.Theta = 20;
            Point p1, p2;
            p1 = toDP(slicer.pBaseLine[0]);
            for (int i = 1; i < slicer.pBaseLine.Count; i++)
            {
                p2 = toDP(slicer.pBaseLine[i]);

                g.DrawLine(pen, p1, p2);

                if( i == slicer.pBaseLine.Count - 1 && !slicer.Closed )
                {                    
                    ar.DrawArrow(g, pen, Brushes.Black, p1, p2);
                }
                p1 = p2;
            }
            
        }
        private void DrawSlicerBaseLines(Graphics g)
        {
            if (pSlicers.Count < 1) return;

            Pen pen1 = new Pen(Color.Red, 2 );
            Pen pen2 = new Pen(Color.Blue, 1);
            
            for (int i = 0; i < pSlicers.Count; i++)
            {
                if (nSelected == i)
                    DrawSlicerBaseLine(pSlicers[i], g, pen1);
                else DrawSlicerBaseLine(pSlicers[i], g, pen2);
            }
        }

        Vector64 toWorldVector(C3DObjectBase obj, Vector64 p)
        {
            if (CDataModel.IsEarthMapVision) return obj.toWorldVector(p);
            else return p;
        }
        Vector64 toWorldVector(C3DObjectBase obj, Vector32 p)
        {
            return toWorldVector(obj, p.toVector64());
        }

        //绘制点数据
        private void DrawScatterPoints(Graphics e)
        {
            ScatteredPoints sc;
            List<C3DObjectBase> objects = C3DData.GetObjects();
            for (int i = 0; i < objects.Count; i++ )
            {
                if (objects[i].type == ShapeEnum.Points)
                {
                    sc = (ScatteredPoints)objects[i];
                    if ( sc.Visible )DrawScatterPoint(sc, e);                    
                }
            }            
        }

        void DrawScatterPoint(ScatteredPoints obj,Graphics g)
        {
            Vector32 p,p1;
            double x = 0, y = 0;
            Color color;
            int size = 10;
            Rectangle rect;
            TexturedText text;
            SolidBrush brush = new SolidBrush(Color.Red);
            Font font = new Font("System",8);
            var stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            
            for (int i = 0; i < obj.Count; i += obj.Interval)
            {
                p = obj.points[i];
                if (obj.IsHidePoint(p)) continue;
                p1 = p;
                //p1 = obj.TransformedPoint(p);
                //p1 = toWorldVector(obj, p1);
                //p1 = CDataModel.ToModelVector(p1);
                if (plan == planEnum.XOY)
                {
                    x = p1.X;
                    y = p1.Y;                   
                }
                if (plan == planEnum.XOZ)
                {
                    x = p1.X;
                    y = p1.Z;
                }
                if (plan == planEnum.YOZ)
                {
                    x = p1.Y;
                    y = p1.Z;
                }
                color = obj.GetColor(p1.V);
                LPtoDP(ref x, ref y);
                rect = new Rectangle((int)x, (int)y, size, size);
                g.FillEllipse(new SolidBrush(color), rect);
                
                if ( obj.ShowLabel )
                {
                    text = obj.Labels[i];
                    g.DrawString(text.Name, font, brush, (float)x, (float)y + size, stringFormat);
                }
            }
        }
        private void DrawXYGrid(Graphics e)
        {
            double x1 = DataRect.x1;
            double x2 = DataRect.x2;
            double y1 = DataRect.y1;
            double y2 = DataRect.y2;

            double xx = DrawRect.Width / ((x2 - x1) / xstep);
            double yy = DrawRect.Height / ((y2 - y1) / ystep);

            if (x1 < minx) x1 = minx;
            if (x2 > maxx) x2 = maxx;
            if (y1 < miny) y1 = miny;
            if (y2 > maxy) y2 = maxy;

            int ix1 = (int)((x1 - minx) / xstep);
            int ix2 = (int)((x2 - minx) / xstep);
            int iy1 = (int)((y1 - miny) / ystep);
            int iy2 = (int)((y2 - miny) / ystep);

            Rectangle rect = new Rectangle(0, 0, 0, 0);

            ColorRGBA color;
            Color cc = new Color();
            double x, y;

            for (int iy = iy1; iy < iy2; iy++)
            {
                for (int ix = ix1; ix < ix2; ix++)
                {
                    x = minx + xstep * ix;
                    y = miny + ystep * iy;
                    LPtoDP(ref x, ref y);

                    rect.X = (int)x;
                    rect.Y = (int)(y - yy);
                    rect.Width = (int)xx; //grid width
                    rect.Height = (int)yy; //grid height

                    if (!GetGridColor(ix, iy, Position, out color))
                        continue;
                    cc = Color.FromArgb(255, color.R, color.G, color.B);
                    e.FillRectangle(new SolidBrush(cc), rect);
                    e.DrawRectangle(new Pen(cc), rect);
                }
            }
        }
        private void DrawXZGrid(Graphics e)
        {
            double x1 = DataRect.x1;
            double x2 = DataRect.x2;
            double y1 = DataRect.y1;
            double y2 = DataRect.y2;

            double xx = DrawRect.Width / ((x2 - x1) / xstep);
            double yy = DrawRect.Height / ((y2 - y1) / zstep);

            if (x1 < minx) x1 = minx;
            if (x2 > maxx) x2 = maxx;
            if (y1 < minz) y1 = minz;
            if (y2 > maxz) y2 = maxz;

            int ix1 = (int)((x1 - minx) / xstep);
            int ix2 = (int)((x2 - minx) / xstep);
            int iz1 = (int)((y1 - minz) / zstep);
            int iz2 = (int)((y2 - minz) / zstep);

            Rectangle rect = new Rectangle(0, 0, 0, 0);

            ColorRGBA color;
            Color cc = new Color();
            double x, y;
            for (int iz = iz1; iz < iz2; iz++)
            {
                for (int ix = ix1; ix < ix2; ix++)
                {
                    x = minx + xstep * ix;
                    y = minz + zstep * iz;
                    LPtoDP(ref x, ref y);

                    rect.X = (int)x;
                    rect.Y = (int)(y - yy);
                    rect.Width = (int)xx; //grid width
                    rect.Height = (int)yy; //grid height
                    if (!GetGridColor(ix, Position, iz, out color))
                        continue;
                    cc = Color.FromArgb(255, color.R, color.G, color.B);
                    e.FillRectangle(new SolidBrush(cc), rect);
                    e.DrawRectangle(new Pen(cc), rect);
                }
            }
        }
        private void DrawYZGrid(Graphics e)
        {
            double x1 = DataRect.x1;
            double x2 = DataRect.x2;
            double y1 = DataRect.y1;
            double y2 = DataRect.y2;

            double xx = DrawRect.Width / ((x2 - x1) / ystep);
            double yy = DrawRect.Height / ((y2 - y1) / zstep);

            if (x1 < miny) x1 = miny;
            if (x2 > maxy) x2 = maxy;
            if (y1 < minz) y1 = minz;
            if (y2 > maxz) y2 = maxz;

            int iy1 = (int)((x1 - miny) / ystep);
            int iy2 = (int)((x2 - miny) / ystep);
            int iz1 = (int)((y1 - minz) / zstep);
            int iz2 = (int)((y2 - minz) / zstep);

            Rectangle rect = new Rectangle(0, 0, 0, 0);

            ColorRGBA color;
            Color cc = new Color();
            double x, y;
            for (int iz = iz1; iz < iz2; iz++)
            {
                for (int iy = iy1; iy < iy2; iy++)
                {
                    x = miny + ystep * iy;
                    y = minz + zstep * iz;
                    LPtoDP(ref x, ref y);

                    rect.X = (int)x;
                    rect.Y = (int)(y - yy);
                    rect.Width = (int)xx; //grid width
                    rect.Height = (int)yy; //grid height

                    if (!GetGridColor(Position, iy, iz, out color))
                        continue;

                    cc = Color.FromArgb(255, color.R, color.G, color.B);
                    e.FillRectangle(new SolidBrush(cc), rect);
                    e.DrawRectangle(new Pen(cc), rect);
                }
            }
        }
        private void DrawBaseLine(Graphics e, CSlicer slicer)
        {
            Vector32 p1, p2;
            double x1, y1, x2, y2;
            Pen pen = new Pen(Color.Blue, 2);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
            for (int i = 0; i < slicer.pBaseLine.Count - 1; i++)
            {
                if (plan == planEnum.XOY)
                {
                    p1 = slicer.pBaseLine[i];//logical coordinates
                    x1 = p1.X;
                    y1 = p1.Y;
                    p2 = slicer.pBaseLine[i + 1];
                    x2 = p2.X;
                    y2 = p2.Y;
                    LPtoDP(ref x1, ref y1);
                    LPtoDP(ref x2, ref y2);
                    e.DrawLine(pen, (int)x1, (int)y1, (int)x2, (int)y2);
                }
                else if (plan == planEnum.XOZ)
                {
                    p1 = slicer.pBaseLine[i];//logical coordinates
                    x1 = p1.X;
                    y1 = p1.Z;
                    p2 = slicer.pBaseLine[i + 1];
                    x2 = p2.X;
                    y2 = p2.Z;
                    LPtoDP(ref x1, ref y1);
                    LPtoDP(ref x2, ref y2);
                    e.DrawLine(pen, (int)x1, (int)y1, (int)x2, (int)y2);
                }
            }
        }
       
        private void OKbutton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
