using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DDDSharp
{
    public class ScreenImageBase
    {
        [DllImport("user32.dll")]//取设备场景
        private static extern IntPtr GetDC(IntPtr hwnd);//返回设备场景句柄
        [DllImport("gdi32.dll")]//取指定点颜色
        private static extern int GetPixel(IntPtr hdc, Point p);
        public Color ColorPicker(Point point)
        {
            IntPtr hdc = GetDC(new IntPtr(0));//取到设备场景(0就是全屏的设备场景)
            int c = GetPixel(hdc, point);//取指定点颜色
            int r = (c & 0xFF);//转换R
            int g = (c & 0xFF00) / 256;//转换G
            int b = (c & 0xFF0000) / 65536;//转换B
            return Color.FromArgb(r, g, b);
        }

    }
}
