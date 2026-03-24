using DataCollection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DDDSharp.Slicer.FloodFill
{
    internal class FloodFillEx
    {
        public Bitmap bitmap;
        public List<Color>FillColors = new List<Color>();
        List<Point>Boundaries = new List<Point>();

        bool IsSimilarColor(Color c1, Color c2, float difference = 5)
        {
            if (Math.Abs(c1.R - c2.R) > difference) return false;
            if (Math.Abs(c1.G - c2.G) > difference) return false;
            if (Math.Abs(c1.B - c2.B) > difference) return false;
            return true;
        }
        bool IsColorInList(Color c, List<Color> colors, float difference)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                if (C3DData.IsSimilarColor(c, colors[i], difference))
                    return true;
            }
            return false;
        }


        public void DoFill(Point p)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            //拷贝图像数据
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);            
            int stride = data.Stride;//data.Stride;
            byte[] bytes = new byte[height * stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);//图像数据解锁
            int ix = p.X, iy = p.Y;
            
            bytes = null;
        }
    }
}
