using System;
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
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;//矩阵类
using MathNet.Numerics.LinearAlgebra.Double;//矩阵类

namespace DataCollection
{
    /// <summary>
    /// 图像处理类
    /// </summary>
    static public class MyImageConvert
    {
        /// <summary>
        /// 背景透明化，改变图像
        /// </summary>
        /// <param name="img">原图</param>
        /// <param name="backcolor">背景色</param>
        /// <param name="diff">背景色误差范围</param>
        /// <returns>新图</returns>
        public static Bitmap BackgroundTransparent(Bitmap img, Color backcolor, int diff = 40)
        {
            int width = img.Width;
            int height = img.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData data = img.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            byte[] bytes = new byte[height * stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            byte r0 = backcolor.R;
            byte g0 = backcolor.G;
            byte b0 = backcolor.B;
            byte r,g,b,alpha;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < stride; j += 4)
                {
                    r = bytes[i * stride + j];
                    g = bytes[i * stride + j + 1];
                    b = bytes[i * stride + j + 2];
                    alpha = 255;
                    if (Math.Abs(r - r0) < diff && 
                        Math.Abs(g - g0) < diff && 
                        Math.Abs(b - b0) < diff)
                    {
                        alpha = 0;//RGB在色差范围内，透明度为0  
                    }
                    bytes[i * stride + j + 3] = alpha;
                }
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            //图像数据解锁
            img.UnlockBits(data);            
            return img;
        }
        /// <summary>
        /// 背景透明化
        /// </summary>
        /// <param name="img">原图</param>
        /// <param name="x0">背景色拾取颜色位置</param>
        /// <param name="y0">背景色拾取颜色位置</param>
        /// <param name="diff">背景色误差范围</param>        
        /// <returns>新图像</returns>
        public static Bitmap BackgroundTransparent(Bitmap _img, int x0 = 1, int y0 = 1, int diff = 40)
        {
            Color backcolor = _img.GetPixel(1, 1);
            return BackgroundTransparent(_img,backcolor,diff);
        }
        /// <summary>
        /// 图像灰度变换：彩色到灰度，改变图像
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns>新图像</returns>
        public static Bitmap toGrayImage(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            byte r, g, b, gray = 0;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            byte[] bytes = new byte[height * stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < stride; j += 4)
                {
                    r = bytes[i * stride + j];
                    g = bytes[i * stride + j + 1];
                    b = bytes[i * stride + j + 2];
                    gray = (byte)((r * 28 + g * 151 + b * 77) >> 8);
                    bytes[i * stride + j] = gray;
                    bytes[i * stride + j + 1] = gray;
                    bytes[i * stride + j + 2] = gray;
                }
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            //图像数据解锁
            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
