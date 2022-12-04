using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using DataCollection;

namespace DDDSharp
{
    public class ScaledWindowRect
    {
        public Rectangle DrawRect;
        public DoubleRect DataRect;
        public DoubleRect OrgDataRect;
        public DirectionEnum coordinateDirection = DirectionEnum.up;
        public double xyScale = 1.0; // scale = x / y
        public ScaledWindowRect(DoubleRect _datarect, Rectangle _drawrect,
                                double xyscale = 1.0, DirectionEnum direction = DirectionEnum.up)
        {
            DrawRect = _drawrect;
            DataRect = _datarect;
            OrgDataRect = _datarect;
            xyScale = xyscale;
            AutoSetWindow(xyScale);
        }
        public void AutoSetWindow(double xyscale)
        {            
            double x0 = (OrgDataRect.x1 + OrgDataRect.x2) / 2;
            double y0 = (OrgDataRect.y1 + OrgDataRect.y2) / 2;

            //数据 横向 / 纵向比例
            double dataScale = xyscale * OrgDataRect.Width / OrgDataRect.Height;
            double winScale = (double)DrawRect.Width / DrawRect.Height;//窗口横纵向比例

            //按比例窗口宽度 
            if (dataScale * DrawRect.Height > DrawRect.Width ) //fit to Width
            {
                DataRect.x1 = OrgDataRect.x1;
                DataRect.x2 = OrgDataRect.x2;
                //按比例窗口高度
                double yy = DataRect.Width / winScale;
                DataRect.y1 = y0 - yy;
                DataRect.y2 = y0 + yy;
            }
            else //fit to height
            {
                DataRect.y1 = OrgDataRect.y1;
                DataRect.y2 = OrgDataRect.y2;
                double xx = DataRect.Height * winScale;
                DataRect.x1 = x0 - xx / 2;
                DataRect.x2 = x0 + xx / 2;
            }
        }
        public void UpdateDrawRect(Rectangle _drawRect)
        {
            DrawRect = _drawRect;            
            AutoSetWindow(xyScale);            
        }
        public void LPtoDP(ref double x, ref double y)
        {
            if (coordinateDirection == DirectionEnum.up)
            {
                // y
                // |
                // |_______X
                x = DrawRect.Left + DrawRect.Width * (x - DataRect.x1) / DataRect.Width;
                y = DrawRect.Bottom - DrawRect.Height * (y - DataRect.y1) / DataRect.Height;
            }
            else //y向下
            {
                x = DrawRect.Left + DrawRect.Width * (x - DataRect.x1) / DataRect.Width;
                y = DrawRect.Top + DrawRect.Height * (y - DataRect.y1) / DataRect.Height;
            }
        }
        public void DPtoLP(ref double x, ref double y)
        {
            if (coordinateDirection == DirectionEnum.up)
            {   // y
                // |
                // |_______X
                x = DataRect.x1 + DataRect.Width * (x - DrawRect.Left) / DrawRect.Width;
                y = DataRect.y1 + DataRect.Height * (DrawRect.Bottom - y) / DrawRect.Height;
            }
            else
            {
                x = DataRect.x1 + DataRect.Width * (x - DrawRect.Left) / DrawRect.Width;
                y = DataRect.y1 + DataRect.Height * (y - DrawRect.Top) / DrawRect.Height;
            }
        }
        public void Zoom(double x0,double y0, double scale = 0.8)
        {
            double xx = DataRect.Width * scale;
            double yy = DataRect.Height * scale;
            DataRect.x1 = x0 - xx / 2;
            DataRect.x2 = x0 + xx / 2;
            DataRect.y1 = y0 - yy / 2;
            DataRect.y2 = y0 + yy / 2;            
        }
        //以x,y0为中心，窗口缩放
        public void ZoomRect(double x0, double y0, double width, double height)
        {
            double scalex = width / DataRect.Width;
            double scaley = height / DataRect.Height;
            if (scalex > scaley) Zoom(x0, y0, scalex);
            else Zoom(x0, y0, scaley);
        }
        /*
        //以x,y0为中心，窗口缩放
        public void ZoomRect(double x0,double y0,double width,double height)
        {
            //窗口 横向 / 纵向比例            
            double winScale = (double)DrawRect.Width / DrawRect.Height;//窗口横纵向比例
            double dataScale = DataRect.Width / DataRect.Height;//数据横纵向比例            
            //按比例窗口宽度 
            if ( height * winScale > width ) //fit to height
            {
                DataRect.y1 = y0 - height / 2;
                DataRect.y2 = y0 + height / 2;
                double xx = height * winScale;
                DataRect.x1 = x0 - xx / 2;
                DataRect.x2 = x0 + xx / 2;
            }
            else //fit to height
            {
                DataRect.x1 = x0 - width / 2;
                DataRect.x2 = x0 + width / 2;
                //按比例窗口高度
                double yy = width / winScale;
                DataRect.y1 = y0 - yy;
                DataRect.y2 = y0 + yy;                
            }
        }
        */
    }
}
