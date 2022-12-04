using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

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
            Step = Math.Round(Step, floatNum);
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
            for( int i = 0; i < 10; i++ )
            {
                v1 = Math.Round(5 * Step, i);
                v2 = Math.Round(6 * Step, i);
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
}
