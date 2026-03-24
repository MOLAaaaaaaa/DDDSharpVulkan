using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
namespace DataCollection
{
    public class CSurferGrid
    {
        public float[] pData;
        public int xGrid, yGrid;
        public double minx, maxx;
        public double miny, maxy;
        public double minv, maxv;
        public double xStep, yStep;
        static public double blankValue = 1.70141E+038;
        static public float blankValuefloat = 1.70141E+038f;
        public string Name = "";
        public string errMessage = "";        
        public float this[int index]
        {
            get { return pData[index]; }
            set { pData[index] = value;}
        }
        public void SetBlanked(int id)
        {
            pData[id] = blankValuefloat;
        }
        public void SetBlanked(int ix,int iy)
        {
            pData[ix + iy * xGrid] = blankValuefloat;            
        }
        //地层尖灭相交计算
        //  \blanked
        //   \blanked
        //----\---upper
        //     \ lower
        /// <summary>
        /// 地层尖灭处理
        /// </summary>
        /// <param name="upper"></param>
        /// <param name="lower"></param>
        public void DoLayersBlanking(CSurferGrid upper, CSurferGrid lower)
        {
            double x, y, z1, z2;
            for (int iy = 0; iy < lower.yGrid; iy++)
            {
                for (int ix = 0; ix < lower.xGrid; ix++)
                {
                    x = lower.minx + ix * lower.xStep;
                    y = lower.miny + iy * lower.yStep;
                    if (x < upper.minx || x > upper.maxx ||
                        y < upper.miny || y > upper.maxy) continue;
                    z1 = upper.GetZValue(x, y);
                    z2 = lower.GetZValue(ix, iy);
                    if (z2 < z1) lower[ix + iy * lower.xGrid] = CSurferGrid.blankValuefloat;
                }
            }
        }


        public void Draw(Graphics g,Rectangle rect,CColorScale colorscale = null)
        {
            double x, y, z;
            double dx = rect.Width / (xGrid -1);
            double dy = rect.Width / (xGrid - 1);
            Color color;
            for ( int iy = 0;iy < yGrid; iy++ )
            {
                for (int ix = 0; ix < xGrid; ix++)
                {
                    x = rect.Left + ix * dx;
                    y = rect.Bottom - iy * dy;
                    z = GetZValue(ix, iy);
                    color = colorscale.GetColor(z);
                    g.FillRectangle(new SolidBrush(color), (float)x, (float)y, (float)dx,(float)dy);
                }
            }
        }

        public Bitmap toBitmap(int width,int height, CColorScale colorscale = null)
        {
            Bitmap bmp = new Bitmap(width,height);
            Graphics g = Graphics.FromImage(bmp);
            Draw(g, new Rectangle(0,0,width,height), colorscale);
            return bmp;
        }
        /// <summary>
        /// 从一个图像的中创建网格数据
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public bool fromImage(Bitmap bmp)
        {
            try 
            {
                int width = bmp.Width;
                int height = bmp.Height;
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                minx = miny = minv = 0;
                maxx = width;
                maxy = height;
                maxv = 255;
                xStep = yStep = 1;

                xGrid = width;
                yGrid = height;
                pData = new float[xGrid * yGrid];

                byte[] bytes = new byte[height * width * 4];                
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                double r, g, b, gray;
                long id;
                Color color;
                for (int i = height-1; i >= 0 ; i--)
                {
                    for (int j = 0; j < width; j++)
                    {
                        id = i * width + j;
                        r = bytes[4 * id];
                        g = bytes[4 * id+ 1];
                        b = bytes[4 * id + 2];
                        gray = 0.299 * r + 0.587 * g + 0.114 * b;
                        color = Color.FromArgb((byte)r, (byte)g, (byte)b);
                        pData[id] = color.ToArgb();
                    }
                }

                bmp.UnlockBits(data);                

                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        static public bool IsBlankValue(float v, double zero = 0.0001)
        {
            if (float.IsNaN(v)) return true;
            if (v == blankValuefloat) return true;
            if ( Math.Abs(v - blankValue) / Math.Abs(blankValue) <= zero ) return true;
            else return false;
        }

        static public bool IsBlankValue(double v,double zero = 0.0001)
        {
            if ( double.IsNaN(v) ) return true;
            if ( v == blankValue ) return true;
            if ( Math.Abs(v - blankValue) / Math.Abs(blankValue) <= zero) return true;
            else return false;
        }
        public bool IsInRange(double x,double y)
        {
            if ( x < minx || x > maxx || y < miny || y > maxy )
                return false;
            else return true;
        }
        public bool IsBlankedValue(double val, double err = 1.0E-5)
        {
            if ( IsBlankValue(val) ) return true;
            double len = (maxv - minv);
            if ( val < minv && Math.Abs(val-minv) > err ) return true;
            else if (val > maxv && Math.Abs(val - maxv) > err) return true;
            else return false;
        }
        public bool IsBlanked(int id)
        {
            if (IsBlankedValue(pData[id])) return true;
            else return false;
        }
        public bool IsBlanked(int ix,int iy)
        {
            if (IsBlankedValue(pData[ix+iy*xGrid])) return true;
            else return false;
        }
        public CSurferGrid()
        {
            xGrid = yGrid = 0;
            xStep = yStep = 0;
            minx = maxx = 0;
            miny = maxy = 0;
            minv = maxv = 0;
            errMessage = "";           
            pData = null;
        }
        public void Clear()
        {
            xGrid = yGrid = 0;
            xStep = yStep = 0;
            minx = maxx = 0;
            miny = maxy = 0;
            minv = maxv = 0;
            errMessage = "";
            pData = null;
        }
        public bool Read(string path)
        {
            BinaryReader br;
            try
            {
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                Name = Path.GetFileNameWithoutExtension(path);
                bool ret = false;
                long id = br.ReadInt32();
                if (id == 0x42525344)//Surfer 7.0
                {
                    ret = ReadSurfer7(br);
                }
                else if (id == 0x42425344)//Surfer 6.0
                {
                    ret = ReadSurfer6(br);
                }
                br.Close();
                return ret;
            }
            catch (IOException e)
            {
                errMessage = "Open file failed." + Environment.NewLine + e.Message;                
                return false;
            }
        }
        private bool ReadSurfer7(BinaryReader br)
        {
            int size = br.ReadInt32();
            int version = br.ReadInt32();
            int section = br.ReadInt32();
            if (section != 0x44495247)
            {
                errMessage = "Invalid Section Tag.";
                return false;
            }

            int nByte = br.ReadInt32();
            yGrid = br.ReadInt32(); //row
            xGrid = br.ReadInt32(); //col            
            if (xGrid <= 0 || yGrid <= 0)
            {
                errMessage = "Invalid parameters: xgrid = " + xGrid + ",ygrid = yGrid";
                return false;
            }
            pData = new float[xGrid*yGrid];
            if (pData == null)
            {
                errMessage = "allocate memory error." + Environment.NewLine;
                errMessage += "request memory size:" + xGrid.ToString() + " x ";
                errMessage += yGrid.ToString() + " * 4 ";
                return false;
            }
            minx = br.ReadDouble();
            miny = br.ReadDouble();
            xStep = br.ReadDouble();
            yStep = br.ReadDouble();
            minv = br.ReadDouble();
            maxv = br.ReadDouble();
            double Ration = br.ReadDouble(); 
            blankValue = br.ReadDouble();            
            maxx = minx + xGrid * xStep;
            maxy = miny + yGrid * yStep;
            if (minx >= maxx || miny >= maxy || minv >= maxv)
            {
                errMessage = "Invalid parameters of minimum and maximum : ";
                errMessage += Environment.NewLine;
                errMessage += " X from " + minx + " to " + maxx + Environment.NewLine;
                errMessage += " Y from " + miny + " to " + maxy + Environment.NewLine;
                errMessage += " Z from " + minv + " to " + maxv;
                return false;
            }
            section = br.ReadInt32();
            size = br.ReadInt32();
            if (section != 0x41544144)
            {
                errMessage = "Invalid Section Tag.";
                return false;
            }
            float v;
            for (int i = 0; i < xGrid * yGrid; i++)
            {
                v = (float)br.ReadDouble();
                pData[i] = v;
            }
            return true;
        }
        private bool ReadSurfer6(BinaryReader br)
        {
            xGrid = br.ReadInt16();
            yGrid = br.ReadInt16();
            if( xGrid<=0 || yGrid<=0)
            {
                errMessage = "Invalid parameters: xgrid = " + xGrid + ",ygrid = yGrid";
                return false;
            }

            pData = new float[xGrid * yGrid];
            if (pData == null)
            {
                errMessage = "allocate memory error." + Environment.NewLine;
                errMessage += "request memory size:" + xGrid.ToString() + " x ";
                errMessage += yGrid.ToString() + " * 4 ";
                return false;
            }

            minx = br.ReadDouble();
            maxx = br.ReadDouble();
            miny = br.ReadDouble();
            maxy = br.ReadDouble();
            minv = br.ReadDouble();
            maxv = br.ReadDouble();

            if( minx>=maxx || miny>= maxy || minv>=maxv)
            {
                errMessage = "Invalid parameters of minimum and maximum : ";
                errMessage += Environment.NewLine;
                errMessage += " X from " + minx + " to " + maxx + Environment.NewLine;
                errMessage += " Y from " + miny + " to " + maxy + Environment.NewLine;
                errMessage += " Z from " + minv + " to " + maxv;
                return false;
            }
            xStep = (maxx - minx) / (xGrid - 1);
            yStep = (maxy - miny) / (yGrid - 1);
            float v;
            for(int i=0;i<xGrid*yGrid;i++)
            {
                v = br.ReadSingle();
                pData[i] = v;
            }
            return true;
        }
        public bool SaveAs(string path)
        {
            BinaryWriter br;
            try
            {
                br = new BinaryWriter(new FileStream(path, FileMode.Create));
                int id = 0x42425344;//6.0 , 0x42525344 is 7.0            
                br.Write(id);
                SaveSurfer6(br);
                br.Close();
                return true;
            }
            catch (IOException e)
            {
                errMessage = "Open file failed." + Environment.NewLine + e.Message;
                return false;
            }
        }
        public bool SaveSurfer6(BinaryWriter br)
        {
            br.Write((Int16)xGrid);
            br.Write((Int16)yGrid);
            br.Write(minx);
            br.Write(maxx);
            br.Write(miny);
            br.Write(maxy);
            br.Write(minv);
            br.Write(maxv);
            for (int i = 0; i < xGrid * yGrid; i++)
            {
                br.Write(pData[i]);
            }
            return true;
        }
        
        public double GetZValue(int ix,int iy)
        {
            int id = ix + iy * xGrid;
            return pData[id];
        }

        //通过插值得到网格任意位置的值
        public double GetZValue(double x, double y)
        {
            int ix = (int)( (x - minx ) / xStep);
            int iy = (int)( (y - miny ) / yStep);

            //out of the range of this meshes
            if (ix < 0) ix = 0;
            if (iy < 0) iy = 0;
            if (ix > xGrid - 1) ix = xGrid - 1;
            if (iy > yGrid - 1) iy = yGrid - 1;
            
            int id0 = GetVerticIndex(ix, iy);
            int id1 = GetVerticIndex(ix + 1, iy);
            int id2 = GetVerticIndex(ix, iy + 1);
            int id3 = GetVerticIndex(ix + 1, iy + 1);
            
            if ( id1 >= xGrid * yGrid ) id1 = -1;
            if ( id1 >= xGrid * yGrid ) id2 = -1;
            if ( id3 >= xGrid * yGrid ) id3 = -1;

            if (id0 >= 0) if (IsBlankedValue(pData[id0])) id0 = -1;
            if (id1 >= 0) if (IsBlankedValue(pData[id1])) id1 = -1;
            if (id2 >= 0) if (IsBlankedValue(pData[id2])) id2 = -1;
            if (id3 >= 0) if (IsBlankedValue(pData[id3])) id3 = -1;

            if (id0 < 0 && id1 < 0 && id2 < 0 && id3 < 0) return blankValue;

            double x1 = minx + ix * xStep;
            double y1 = miny + iy * yStep;
            double x2 = x1 + xStep;
            double y2 = y1 + yStep;

            double v0 = 0, v1 = 0, v2 = 0, v3 = 0, v = 0;
            if (id0 >= 0) v0 = pData[id0];
            if (id1 >= 0) v1 = pData[id1];
            if (id2 >= 0) v2 = pData[id2];
            if (id3 >= 0) v3 = pData[id3];
            //    |y
            //    2----3
            //    |    |
            //    0----1---->x
            if ( id0 >= 0 && id1 < 0 && id2 < 0 && id3 < 0) return v0;
            else if (id0 < 0 && id1 >= 0 && id2 < 0 && id3 < 0) return v1;
            else if (id0 < 0 && id1 < 0 && id2 >= 0 && id3 < 0) return v2;
            else if (id0 < 0 && id1 < 0 && id2 < 0 && id3 >= 0) return v3;

            else if (id0 >= 0 && id1 >= 0 && id2 < 0 && id3 < 0)//01
            {
                return v0 + (v1 - v0) * (x - x1) / xStep;
            }
            else if (id0 >= 0 && id2 >= 0 && id1 < 0 && id3 < 0)//02
            {
                return v0 + (v2 - v0) * (y - y1) / yStep;
            }            
            else if (id0 >= 0 && id3 > 0 && id1 < 0 && id2 < 0)//03
            {
                double l = Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
                double ll = Math.Sqrt( xStep * yStep );
                return v0 + (v3 - v0) * l / ll;
            }
            else if (id1 >= 0 && id2 > 0 && id0 < 0 && id3 < 0)//12
            {
                double l = Math.Sqrt((x - x2) * (x - x2) + (y - y1) * (y - y1));
                double ll = Math.Sqrt(xStep * yStep);
                return v1 + (v2 - v1) * l / yStep;
            }
            else if (id1 >= 0 && id3 > 0 && id0 < 0 && id2 < 0)//13
            {
                return v1 + (v3 - v1) * (y - y1) / yStep;
            }
            else if (id0 >= 0 && id1 >= 0 && id2 >= 0 && id3 >= 0)//0123
            {
                double p1 = v0 + (x - x1) * (v1 - v0) / xStep;
                double p2 = v2 + (x - x1) * (v3 - v2) / xStep;
                return p1 + (p2 - p1) * (y - y1) / yStep;
            }
            else return v0;
        }

        public int GetVerticIndex(int ix, int iy)
        {
            if (ix < 0 || ix >= xGrid) return -1;
            if (iy < 0 || iy >= yGrid) return -1;
            return ix + iy * xGrid;
        }
        public double this[int ix, int iy]
        {
            get
            {
                return GetZValue(ix, iy);
            }
            set 
            {
                int id = ix + iy * xGrid;
                pData[id] = (float)value;
            }
        }
    }
}

