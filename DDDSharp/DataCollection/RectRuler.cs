using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace DataCollection
{
    public enum AxisDirectionEnum
    {
        LeftRight = 0,
        RightLeft = 1,
        DownUp = 2,
        UpDown = 3,        
    }
    public class AxisRuler
    {
        public bool Visible = true;
        public AxisDirectionEnum Direction = AxisDirectionEnum.LeftRight;
        public double Minimum = 0;  //最小值
        public double Maximum = 0;  //最大值
        public int maxScale = 5; //大刻度数
        public int minScale = 10;//小刻度数
        public double Step = 0;
        public int longTick = 8;    //长刻度（像素）
        public int shortTick = 4;   //短刻度（像素）        
        public string Title = "";   //主轴名称
        public string legendTitle = "";//副轴名称
        public int floatNum = 2;        //小数点位数
        public Font titleFont = new Font("宋体" , 8.0f);
        public Font scaleFont = new Font("宋体", 8.0f);
        public bool Inverse = false;
        public Color scaleColor = Color.Black;
        public Color titleColor = Color.Black;
        public AxisRuler(AxisDirectionEnum direction)
        {
            Direction = direction;
        }
        public bool IsValid
        {
            get { return Maximum > Minimum; }
        }
        public void SetValuesRange(double minv, double maxv)
        {
            AutoSetScale(minv, maxv);
            AutoSetMinumMaximum(minv, maxv);
            Step = (Maximum - Minimum) / (maxScale * minScale);
            //Step = Math.Round(Step, floatNum);
        }
        /// <summary>
        /// 逻辑坐标映射设备坐标
        /// </summary>
        /// <param name="lx">逻辑坐标</param>
        /// <param name="dx1">设备坐标最小</param>
        /// <param name="dx2">设备坐标最大</param>
        /// <returns></returns>
        public float LptoDp(double lx, double dx1, double dx2)
        {
            if ( dx1 == dx2 ) return 0;
            if (Direction == AxisDirectionEnum.LeftRight ||
                Direction == AxisDirectionEnum.UpDown )
            {
                return (float)(dx1 + (lx - Minimum) /
                      (Maximum - Minimum) * (dx2 - dx1));
            }
            else
            {
                return (float)(dx2 - (lx - Minimum) /
                      (Maximum - Minimum) * (dx2 - dx1));
            }
        }
        /// <summary>
        /// 设备坐标映射逻辑坐标
        /// </summary>
        /// <param name="dx">设备坐标</param>
        /// <param name="dx1">设备坐标最小</param>
        /// <param name="dx2">设备坐标最大</param>
        /// <returns></returns>
        public double DptoLp(double dx, double dx1, double dx2)
        {
            if (dx1 == dx2) return 0;
            if (Direction == AxisDirectionEnum.LeftRight ||
                Direction == AxisDirectionEnum.UpDown)
            { 
                return Minimum + (Maximum - Minimum) * (dx - dx1) / (dx2 - dx1); 
            }
            else
            {
                return Maximum - (Maximum - Minimum) * (dx - dx1) / (dx2 - dx1);
            }
        }
        void AutoSetMinumMaximum(double minv, double maxv)
        {
            double width = maxv - minv;
            if (width <= 0) return;
            double step = (maxv - minv) / maxScale;
            double minstep = step / minScale;
            Minimum = Math.Round(minv, floatNum);
            Maximum = Math.Round(maxv, floatNum);
            if( maxv > Maximum )
            {
                Maximum = Math.Round(Maximum+minstep,floatNum);
            }
        }

        void AutoSetScale(double minv, double maxv)
        {
            double width = maxv - minv;
            if ( width <= 0 ) return;
            Step = (maxv - minv) /(maxScale * minScale);
            double v1, v2;
            for( int i = 1; i < minScale; i++ )
            {
                v1 = Math.Round( (i+1) * Step, i);
                v2 = Math.Round((i+2) * Step, i);
                if( v2 > v1 )
                {
                    floatNum = i;
                    return;
                }
            }
            floatNum = 10;
        }

        /// <summary>
        /// 获取小数位数
        /// </summary>
        /// <param name="decimalV">小数</param>
        /// <returns></returns>
        public int GetNumberOfDecimal(double decimalV)
        {
            string[] temp = decimalV.ToString().Split('.');
            if (temp.Length == 2 && temp[1].Length > 0)
            {
                int index = temp[1].Length - 1;
                while (temp[1][index] == '0' && index-- > 0) ;
                return index + 1;
            }
            return 0;
        }
    }
    public class RectRuler
    {
        public Rectangle windowRect = new Rectangle(); //窗口区域
        public Rectangle drawRect = new Rectangle(); //绘图区域
        public int leftMargine = 10;
        public int rightMargine = 10;
        public int topMargine = 10;
        public int bottomMargine = 10;
        public bool drawAreaRectangle = true;
        public bool fillDrawArea = true;
        public Color fillDrawAreaColor = Color.White;
        public Color drawAreaRectangleColor = Color.Black;

        public AxisRuler leftRuler = new AxisRuler(AxisDirectionEnum.DownUp);
        public AxisRuler rightRuler = new AxisRuler(AxisDirectionEnum.DownUp);
        public AxisRuler topRuler = new AxisRuler(AxisDirectionEnum.LeftRight);
        public AxisRuler bottomRuler = new AxisRuler(AxisDirectionEnum.LeftRight);

        public bool IsDrawXCursor = true; //是否绘制X横向游标
        public bool IsDrawYCursor = true; //是否绘制Y纵向游标
        public Color cursorColor = Color.Gray;//游标线颜色
        public PointF cursorPosition = new PointF(-1,-1);//当前游标位置
        public RectRuler()
        {

        }
        public RectRuler(Rectangle rect)
        {
            windowRect = rect;
            int x = rect.Left + leftMargine;
            int y = rect.Top + topMargine;
            int w = rect.Width - leftMargine - rightMargine;
            int h = rect.Height - topMargine - bottomMargine;
            drawRect = new Rectangle(x, y, w, h);
        }
        public void SetDrawRect(Rectangle rect)
        {
            windowRect = rect;
            int x = rect.Left + leftMargine;
            int y = rect.Top + topMargine;
            int w = rect.Width - leftMargine - rightMargine;
            int h = rect.Height - topMargine - bottomMargine;
            drawRect = new Rectangle(x, y, w, h);
        }
        public void SetMarine(int left,int top,int right,int bottom)
        {
            leftMargine = left;
            rightMargine = right;
            topMargine = top;
            bottomMargine = bottom;
            int x = windowRect.Left + leftMargine;
            int y = windowRect.Top + topMargine;
            int w = windowRect.Width - leftMargine - rightMargine;
            int h = windowRect.Height - topMargine - bottomMargine;
            drawRect = new Rectangle(x, y, w, h);
        }
        
        void DrawLeftRuler(Graphics g)
        {
            if ( !leftRuler.Visible ) return;
            if ( leftRuler.Minimum >= leftRuler.Maximum ) return;            

            string ss;
            Brush brush = new SolidBrush(leftRuler.scaleColor);
            PointF p1,p2;
            SizeF textsize;
            
            RectangleF rect,lastRect = new Rectangle(0,0,0,0);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;

            double x = 0, y;
            int i = 0, len = 0;
            Pen pen = new Pen(leftRuler.scaleColor);
            for (double v = leftRuler.Minimum; v <= leftRuler.Maximum;v += leftRuler.Step)
            {
                if( leftRuler.Direction == AxisDirectionEnum.UpDown )
                  y =  drawRect.Top + (v - leftRuler.Minimum) / (leftRuler.Maximum - leftRuler.Minimum)* drawRect.Height;
                else 
                  y = drawRect.Bottom - (v - leftRuler.Minimum) / (leftRuler.Maximum - leftRuler.Minimum) * drawRect.Height;

                if (i % leftRuler.minScale == 0) 
                { 
                    len = leftRuler.longTick;
                    ss = Math.Round(v, leftRuler.floatNum).ToString();
                    textsize = g.MeasureString(ss, leftRuler.scaleFont);
                    x = drawRect.Left - len - textsize.Width;
                    rect = new RectangleF((float)x,(float)y- textsize.Height/2, textsize.Width,textsize.Height);
                    g.DrawString(ss, leftRuler.scaleFont, brush,rect, format);
                }
                else len = leftRuler.shortTick;

                p1 = new PointF(drawRect.Left - len, (float)y);
                p2 = new PointF(drawRect.Left, (float)y);
                g.DrawLine(pen, p1, p2);
                i++;
            }            
        }
        void DrawBottomRuler(Graphics g)
        {
            if (!bottomRuler.Visible) return;
            if (bottomRuler.Minimum >= bottomRuler.Maximum) return;
            
            string ss;
            Brush brush = new SolidBrush(bottomRuler.scaleColor);
            PointF p1, p2;
            SizeF textsize;
            RectangleF rect, lastRect = new Rectangle(0, 0, 0, 0);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            
            int i = 0, len = 0;
            double x = 0, y = drawRect.Bottom;
            Pen pen = new Pen(bottomRuler.scaleColor);
            for (double v = bottomRuler.Minimum; v <= bottomRuler.Maximum; v += bottomRuler.Step)
            {
                if(bottomRuler.Direction == AxisDirectionEnum.LeftRight)
                    x = drawRect.Left + (v - bottomRuler.Minimum) / (bottomRuler.Maximum - bottomRuler.Minimum) * drawRect.Width;
                else
                    x = drawRect.Right - (v - bottomRuler.Minimum) / (bottomRuler.Maximum - bottomRuler.Minimum) * drawRect.Width;
                if (i % bottomRuler.minScale == 0) //长刻度
                {
                    len = bottomRuler.longTick;
                    ss = Math.Round(v, bottomRuler.floatNum).ToString();
                    textsize = g.MeasureString(ss, bottomRuler.scaleFont);
                    rect = new RectangleF((float)(x- textsize.Width / 2), (float)(y+len+1), textsize.Width, textsize.Height);
                    g.DrawString(ss, bottomRuler.scaleFont, brush, rect, format);
                }
                else len = bottomRuler.shortTick;//短刻度

                p1 = new PointF((float)x, (float)y);
                p2 = new PointF((float)x, (float)(y+len));
                g.DrawLine(pen, p1, p2);
               
                i++;
            }
        }
        /// <summary>
        /// 逻辑坐标映射设备坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void LPtoDP(ref double x, ref double y)
        {
            if (topRuler.Maximum > topRuler.Minimum)
            {
                x = topRuler.LptoDp(x, drawRect.Left, drawRect.Right);
            }
            else if (bottomRuler.Maximum > bottomRuler.Minimum)
            {
                x = bottomRuler.LptoDp(x, drawRect.Left, drawRect.Right);
            }

            if ( leftRuler.Maximum > leftRuler.Minimum )//有效
            {
                y = leftRuler.LptoDp(y, drawRect.Top, drawRect.Bottom);
            }
            else if (rightRuler.Maximum > rightRuler.Minimum)
            {
                y = rightRuler.LptoDp(y, drawRect.Top, drawRect.Bottom);
            }                        
        }
        public void DPtoLP(ref double x, ref double y)
        {
            if (topRuler.Maximum > topRuler.Minimum)
            {
                x = topRuler.DptoLp(x, drawRect.Left, drawRect.Right);
            }
            else if (bottomRuler.Maximum > bottomRuler.Minimum)
            {
                x = bottomRuler.DptoLp(x, drawRect.Left, drawRect.Right);
            }

            if (leftRuler.Maximum > leftRuler.Minimum)
            {
                y = leftRuler.DptoLp(y, drawRect.Top, drawRect.Bottom);
            }
            else if (rightRuler.Maximum > rightRuler.Minimum)
            {
                y = rightRuler.DptoLp(y, drawRect.Top, drawRect.Bottom);
            }            
        }

        public void DrawCursor( Graphics g )
        {
            if ( IsDrawXCursor && cursorPosition.X > 0 )
            {
                PointF p1 = new PointF(cursorPosition.X, drawRect.Top);
                PointF p2 = new PointF(cursorPosition.X, drawRect.Bottom);
                g.DrawLine(new Pen(cursorColor, 1), p1, p2);
                double val = 0;
                float x = cursorPosition.X, y = 0;                
                
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center; //水平                
                
                if ( bottomRuler.IsValid )
                {
                    format.LineAlignment = StringAlignment.Far;//垂直对齐
                    y = drawRect.Bottom - 2;
                    val = bottomRuler.DptoLp(cursorPosition.X, drawRect.Left, drawRect.Right);
                    val = Math.Round(val, bottomRuler.floatNum + 2 );
                    g.DrawString(val.ToString(), bottomRuler.scaleFont, Brushes.Black, x, y, format);
                }
                if (topRuler.IsValid)
                {
                    format.LineAlignment = StringAlignment.Near;//垂直对齐
                    y = drawRect.Top + 2;
                    val = topRuler.DptoLp(cursorPosition.X, drawRect.Left, drawRect.Right);
                    val = Math.Round(val, topRuler.floatNum + 2);
                    g.DrawString(val.ToString(), topRuler.scaleFont, Brushes.Black, x, y, format);
                }
            }
            if (IsDrawYCursor && cursorPosition.Y > 0)
            {
                PointF p1 = new PointF(drawRect.Left, cursorPosition.Y);
                PointF p2 = new PointF(drawRect.Right, cursorPosition.Y);
                g.DrawLine(new Pen(cursorColor, 1), p1, p2);

                double val = 0;
                float x = 0, y = cursorPosition.Y;

                StringFormat format = new StringFormat();                
                format.LineAlignment = StringAlignment.Center;

                if (leftRuler.IsValid)
                {
                    format.Alignment = StringAlignment.Near;
                    x = drawRect.Left + 2;
                    val = leftRuler.DptoLp(cursorPosition.Y, drawRect.Top, drawRect.Bottom);
                    val = Math.Round(val, leftRuler.floatNum + 2);
                    g.DrawString(val.ToString(), leftRuler.scaleFont, Brushes.Black, x, y, format);
                }
                if (rightRuler.IsValid)
                {
                    format.Alignment = StringAlignment.Far;
                    x = drawRect.Right - 2;
                    val = rightRuler.DptoLp(cursorPosition.Y, drawRect.Top, drawRect.Bottom);
                    val = Math.Round(val, rightRuler.floatNum + 2);
                    g.DrawString(val.ToString(), rightRuler.scaleFont, Brushes.Black, x, y, format);
                }
            }
        }
        
        public void Draw(Graphics g)
        {
            if(fillDrawArea)
            {
                Brush brush = new SolidBrush(fillDrawAreaColor);
                g.FillRectangle(brush,drawRect);
            }
            if(drawAreaRectangle)
            {
                Pen pen = new Pen(drawAreaRectangleColor);
                g.DrawRectangle(pen, drawRect);
            }

            DrawLeftRuler(g);
            DrawBottomRuler(g);
        }
    }   
    public enum AxisDockingEdge
    {
        Top = 0, //上停靠 - X Y Axis
        Bottom = 1,//下停靠 - X Y Axis
        FrontLeft = 2, //左停靠 -Z Axis
        FrontRight = 3,//右停靠 -Z Axis
        BackLeft = 4, //左停靠 -Z Axis
        BackRight = 5,//右停靠 -Z Axis        
    }
    public enum AxisDockingPosition
    {
        Outer = 0, //框外
        Inner = 1, //框内
    }
    public class Axis3DRuler
    {
        AxisEnum _Axis = AxisEnum.xAxis;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visble")]
        public bool Visible { get; set; } = true;

        [CategoryAttribute("Display"), DisplayNameAttribute("Axis")]
        public AxisEnum Axis { get { return _Axis; } }

        //平面旋转角度
        [CategoryAttribute("Display"), DisplayNameAttribute("Plane Angle")]
        public float PlaneAngle { get; set; } = 0;
        //停靠边位置
        [CategoryAttribute("Display"), DisplayNameAttribute("Docking Edge")]
        public AxisDockingEdge dockingEdge { get; set; } = AxisDockingEdge.Top;
        //停靠边位置
        [CategoryAttribute("Display"), DisplayNameAttribute("Docking Position")]
        public AxisDockingPosition dockingPosition { get; set; } = AxisDockingPosition.Outer;

        [CategoryAttribute("Title"), DisplayNameAttribute("Visble")]
        public bool ShowTitle { get; set; } = true;

        [CategoryAttribute("Title"), DisplayNameAttribute("Text")]
        public string Title { get; set; } = "";   //主轴名称
        [CategoryAttribute("Title"), DisplayNameAttribute("Font")]
        public Font titleFont { get; set; } = new Font(SystemFonts.DefaultFont.Name, SystemFonts.DefaultFont.Size);//new Font("宋体", 8.0f);
        [CategoryAttribute("Title"), DisplayNameAttribute("Size")]
        public float titleSize { get; set; } = 0.5f;
        [CategoryAttribute("Title"), DisplayNameAttribute("Color")]
        public Color titleColor { get; set; } = Color.Black;
        [CategoryAttribute("Title"), DisplayNameAttribute("Rotate Angle")]
        public float titleAngle { get; set; } = 0;

        [CategoryAttribute("Label Unit"), DisplayNameAttribute("Visble")]
        public bool ShowSecondTitle { get; set; } = true;

        [CategoryAttribute("Label Unit"), DisplayNameAttribute("Text")]
        public string secondTitle { get; set; } = "";//副轴名称
        [CategoryAttribute("Label Unit"), DisplayNameAttribute("Font")]
        public Font secondTitleFont { get; set; } = new Font(SystemFonts.DefaultFont.Name, SystemFonts.DefaultFont.Size);//new Font("宋体", 8.0f);
        [CategoryAttribute("Label Unit"), DisplayNameAttribute("Size")]
        public float secondTitleSize { get; set; } = 0.3f;
        [CategoryAttribute("Label Unit"), DisplayNameAttribute("Color")]
        public Color secondTitleColor { get; set; } = Color.Black;
        [CategoryAttribute("Label Unit"), DisplayNameAttribute("Rotate Angle")]
        public float secondTitleAngle { get; set; } = 0;
        [CategoryAttribute("Label Unit"), DisplayNameAttribute("Attached")]
        public bool IsUnitAttached { get; set; } = true;

        [CategoryAttribute("Ticks"), DisplayNameAttribute("Visible")]
        public bool ShowTicks { get; set; } = true;  //是否绘制刻度
        private double _minimum = 0;
        [CategoryAttribute("Axis"), DisplayNameAttribute("Maximum")]
        public double Minimum 
        {
            get { return _minimum; }
            set 
            { 
                _minimum = value;
                SetMinumMaximum(_minimum, _maximum);
            }
        }        
        private double _maximum = 0;
        [CategoryAttribute("Axis"), DisplayNameAttribute("Maximum")]
        public double Maximum 
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                SetMinumMaximum(_minimum, _maximum);
            }
        }

        public string FormatValue(double val)
        {
            string _format = "F" + floatNum;            
            return val.ToString(_format);
        }

        public int _maxScale = 5;//大刻度数
        public int _minScale = 10;//小刻度数
       
        [CategoryAttribute("Ticks"), DisplayNameAttribute("Long Scale Num#")]
        public int maxScale
        {
            get { return _maxScale; }
            set
            {
                _maxScale = value;
                SetMinumMaximum(_minimum, _maximum);
            }
        }
        [CategoryAttribute("Ticks"), DisplayNameAttribute("Short TicksNum")]
        public int minScale
        {
            get { return _minScale; }
            set
            {
                _minScale = value;
                SetMinumMaximum(_minimum, _maximum);
            }
        }
        [CategoryAttribute("Ticks"), DisplayNameAttribute("Long Tick")]
        public float longTick { get; set; } = 0.015f;//长刻度
        [CategoryAttribute("Ticks"), DisplayNameAttribute("Short Scale")]
        public float shortTick { get; set; } = 0.01f;//短刻度
        [CategoryAttribute("Ticks"), DisplayNameAttribute("Color")]
        public Color tickColor { get; set; } = Color.Gray; //刻度颜色


        [CategoryAttribute("Label"), DisplayNameAttribute("Visible")]
        public bool ShowLable { get; set; } = true;   

        [CategoryAttribute("Label"), DisplayNameAttribute("Start")]
        public double StartValue { get; set; } = 0;  //开始刻度值
        [CategoryAttribute("Label"), DisplayNameAttribute("End")]
        public double EndValue { get; set; } = 0;  //结束刻度值

        [CategoryAttribute("Label"), DisplayNameAttribute("Step")]
        public double Step { get; set; } = 0;

        [CategoryAttribute("Label"), DisplayNameAttribute("Font")]
        public Font labelFont { get; set; } = new Font(SystemFonts.DefaultFont.Name, SystemFonts.DefaultFont.Size); //new Font("宋体", 8.0f);
        [CategoryAttribute("Label"), DisplayNameAttribute("Color")]
        public Color labelColor { get; set; } = Color.Black;
        [CategoryAttribute("Label"), DisplayNameAttribute("Size")]
        public float labelSize { get; set; } = 0.2f;
        [CategoryAttribute("Label"), DisplayNameAttribute("Float Numer")]
        public int floatNum { get; set; } = 2;        //小数点位数        

        public Vector32 LabelOffset = new Vector32(0, 0, 0);
        [CategoryAttribute("Label"), DisplayNameAttribute("Translation")]
        public virtual string OffsetString
        {
            get { return LabelOffset.x + "," + LabelOffset.y + "," + LabelOffset.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    LabelOffset = new Vector32(p.X, p.Y, p.Z);
                }
            }
        }
        
        
        [CategoryAttribute("Label"), DisplayNameAttribute("Rotate Angle")]
        public float LabelRotateAngle { get; set; } = 0;//平面内旋转角度

        [CategoryAttribute("Label"), DisplayNameAttribute("Inverse")]
        public bool LabelInverse { get; set; } = false;        
       
        public Axis3DRuler(AxisEnum ax)
        {
            _Axis = ax;
        }
        public Axis3DRuler Copy()
        {
            Axis3DRuler ruler = new Axis3DRuler(_Axis);
            ruler.Visible = Visible;
            ruler._Axis = _Axis;
            ruler.dockingEdge = dockingEdge;
            ruler.dockingPosition = dockingPosition;
            ruler.PlaneAngle = PlaneAngle;

            //Title
            ruler.Title = Title;
            ruler.ShowTitle = ShowTitle;
            ruler.titleFont = titleFont;
            ruler.titleSize = titleSize;
            ruler.titleColor = titleColor;
            ruler.titleAngle = titleAngle;
            //Second Title
            ruler.ShowSecondTitle = ShowSecondTitle;
            ruler.secondTitle = secondTitle;
            ruler.secondTitleFont = secondTitleFont;
            ruler.secondTitleSize = secondTitleSize;
            ruler.secondTitleColor = secondTitleColor;
            ruler.secondTitleAngle = secondTitleAngle;
            ruler.IsUnitAttached = IsUnitAttached;
            //Ticks
            ruler.ShowTicks = ShowTicks;
            ruler.Minimum = Minimum;
            ruler.Maximum = Maximum;
            ruler.maxScale = maxScale;
            ruler.minScale = minScale;
            ruler.longTick = longTick;
            ruler.shortTick = shortTick;
            ruler.tickColor = tickColor;
            //Label
            ruler.ShowLable = ShowLable;
            ruler.StartValue = StartValue;
            ruler.EndValue = EndValue;
            ruler.Step = Step;
            ruler.labelFont = labelFont;
            ruler.labelSize = labelSize;
            ruler.labelColor = labelColor;
            ruler.floatNum = floatNum;
            ruler.LabelOffset = LabelOffset;
            ruler.LabelInverse = LabelInverse;
            ruler.LabelRotateAngle = LabelRotateAngle;
            
            return ruler;
        }
        public bool Save(BinaryWriter br)
        {
            br.Write((int)_Axis);
            br.Write(Visible);
            br.Write((int)dockingEdge);
            br.Write((int)dockingPosition);            
            br.Write(PlaneAngle);

            //Title
            br.Write(ShowTitle);
            C3DData.SaveString(br, Title);            
            C3DData.SaveFont(br, titleFont);
            br.Write(titleSize);
            br.Write(titleColor.ToArgb());
            br.Write(titleAngle);
            //Second Title
            br.Write(ShowSecondTitle);
            C3DData.SaveString(br, secondTitle);
            C3DData.SaveFont(br, secondTitleFont);
            br.Write(secondTitleSize);
            br.Write(secondTitleColor.ToArgb());
            br.Write(secondTitleAngle);
            br.Write(IsUnitAttached);

            //Ticks
            br.Write(ShowTicks);
            br.Write(Minimum);
            br.Write(Maximum);
            br.Write(maxScale);
            br.Write(minScale);
            br.Write(longTick);
            br.Write(shortTick);
            br.Write(tickColor.ToArgb());
            //Label
            br.Write(ShowLable);
            br.Write(StartValue);
            br.Write(EndValue);
            br.Write(Step);
            C3DData.SaveFont(br, labelFont);
            br.Write(labelSize);
            br.Write(labelColor.ToArgb());
            br.Write(floatNum);
            br.Write(LabelOffset.X);
            br.Write(LabelOffset.Y);
            br.Write(LabelOffset.Z);
            br.Write(LabelInverse);
            br.Write(LabelRotateAngle);
            return true;
        }
        public bool Load(BinaryReader br)
        {
            _Axis = (AxisEnum)br.ReadInt32();
            Visible = br.ReadBoolean();
            dockingEdge = (AxisDockingEdge) br.ReadInt32();
            dockingPosition = (AxisDockingPosition)br.ReadInt32();
            PlaneAngle = br.ReadSingle();
            //Title
            ShowTitle = br.ReadBoolean();
            Title = C3DData.LoadString(br);            
            titleFont = C3DData.LoadFont(br);
            titleSize = br.ReadSingle();
            titleColor = Color.FromArgb(br.ReadInt32());
            titleAngle = br.ReadSingle();
            //Second Title
            ShowSecondTitle = br.ReadBoolean();
            secondTitle = C3DData.LoadString(br);
            secondTitleFont = C3DData.LoadFont(br);
            secondTitleSize = br.ReadSingle();
            secondTitleColor = Color.FromArgb(br.ReadInt32());
            secondTitleAngle = br.ReadSingle();
            IsUnitAttached = br.ReadBoolean();
            //Ticks
            ShowTicks = br.ReadBoolean();
            Minimum = br.ReadDouble();
            Maximum = br.ReadDouble();
            maxScale = br.ReadInt32();
            minScale = br.ReadInt32();
            longTick = br.ReadSingle();
            shortTick = br.ReadSingle();
            tickColor = Color.FromArgb(br.ReadInt32());
            //Label
            ShowLable = br.ReadBoolean();
            StartValue = br.ReadDouble();
            EndValue = br.ReadDouble();
            Step = br.ReadDouble();
            labelFont = C3DData.LoadFont(br);
            labelSize = br.ReadSingle();
            labelColor = Color.FromArgb(br.ReadInt32());
            floatNum = br.ReadInt32();
            LabelOffset.X = br.ReadSingle();
            LabelOffset.Y = br.ReadSingle();
            LabelOffset.Z = br.ReadSingle();
            LabelInverse = br.ReadBoolean();
            LabelRotateAngle = br.ReadSingle();
            return true;
        }
        public bool IsValid()
        {
            if (Maximum == Minimum) return false;
            if (maxScale < 1 || minScale < 1) return false;
            if (Step == 0) return false;
            return true;
        }

        public void SetMinumMaximum(double minv, double maxv)
        {
            _minimum = minv;
            _maximum = maxv;
            
            double width = maxv - minv;
            if (width == 0) return;
            if (minScale == 0 || maxScale == 0) return;

            Step = width / minScale/maxScale;
            StartValue = Math.Round(minv, floatNum);
            EndValue = Math.Round(maxv, floatNum);            
        }
        /// <summary>
        /// 步长计算刻度数
        /// </summary>
        void SetScaleByStep()
        {
            double width = Maximum - Minimum;
            if (width == 0) return;
            maxScale = (int)( width / minScale * Step);
        }
        /// <summary>
        /// 通过刻度数计算步长
        /// </summary>
        void SetSetpByScale()
        {
            Step = 0;
            double width = Maximum - Minimum;
            if (width == 0) return;
            Step = width / (maxScale * minScale);            
        }

        /// <summary>
        /// 获取小数位数
        /// </summary>
        /// <param name="decimalV">小数</param>
        /// <returns></returns>
        public int GetNumberOfDecimal(double decimalV)
        {
            string[] temp = decimalV.ToString().Split('.');
            if (temp.Length == 2 && temp[1].Length > 0)
            {
                int index = temp[1].Length - 1;
                while (temp[1][index] == '0' && index-- > 0) ;
                return index + 1;
            }
            return 0;
        }
    }

    }
