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
using Graphics3D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
namespace DDDSharp
{
    public partial class TestForm : DockingPaneExt
    {
        public CVulkan graphics = null;
        public Bitmap bmp;
        public TestForm()
        {
            InitializeComponent();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Graphics gg = pictureBox1.CreateGraphics();
            int width = bmp.Width;
            int height = bmp.Height;
            Bitmap bit = new Bitmap(width, height,PixelFormat.Format32bppPArgb);
            /*
            //g.DrawImage(bmp, 10, 10);
            byte[] bytes = graphics.BitmapToByte(bmp, SharpVulkan.VkFormat.VK_FORMAT_A2B10G10R10_UINT_PACK32, 4);           
            
            */

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            byte[] bytes = new byte[bmp.Width * bmp.Height * 4];
            Marshal.Copy(bd.Scan0, bytes, 0, bytes.Length);
            bmp.UnlockBits(bd);

            int id;
            byte r, g, b, a;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    id = i * width + j;
                    r = bytes[4 * id];
                    g = bytes[4 * id + 1];
                    b = bytes[4 * id + 2];
                    a = 255;
                    /*
                    b = bytes[4 * id];
                    g = bytes[4 * id+1];
                    r = bytes[4 * id+2];
                    a = bytes[4 * id+3];
                    */
                    bit.SetPixel(j, i, Color.FromArgb(a,r,g,b));
                }
            gg.DrawImage(bit,10,10);
        }
    }
}
