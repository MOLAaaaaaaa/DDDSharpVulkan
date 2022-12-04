using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using PictureBoxScroll;
using DataCollection;

namespace FloodFill2
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public delegate void UpdateScreenDelegate(ref int x, ref int y);

    /// <summary>
    /// The base class that the flood fill algorithms inherit from. Implements the
    /// basic flood filler functionality that is the same across all algorithms.
    /// </summary>
    public abstract class AbstractFloodFiller
    {

        protected EditableBitmap bitmap;
        protected byte[] tolerance = new byte[] { 25, 25, 25 };
        protected Color fillColor = Color.Magenta;
        protected bool fillDiagonally = false;
        protected bool slow = false;

        //cached bitmap properties
        protected int bitmapWidth = 0;
        protected int bitmapHeight = 0;
        protected int bitmapStride = 0;
        protected int bitmapPixelFormatSize = 0;
        protected byte[] bitmapBits = null;

        //internal int timeBenchmark = 0;
        internal Stopwatch watch = new Stopwatch();
#pragma warning disable CS0649 // 从未对字段“AbstractFloodFiller.UpdateScreen”赋值，字段将一直保持其默认值 null
        internal UpdateScreenDelegate UpdateScreen;
#pragma warning restore CS0649 // 从未对字段“AbstractFloodFiller.UpdateScreen”赋值，字段将一直保持其默认值 null

        //internal, initialized per fill
        //protected BitArray pixelsChecked;
        protected bool[] pixelsChecked;
        protected byte[] byteFillColor;
        protected byte[] startColor;

        //filled area indecs
        protected List<int> filledIndices = new List<int>();

        //protected int stride;

        public AbstractFloodFiller()
        {

        }

        public AbstractFloodFiller(AbstractFloodFiller configSource)
        {
            if (configSource != null)
            {
                this.Bitmap = configSource.Bitmap;
                this.FillColor = configSource.FillColor;
                this.FillDiagonally = configSource.FillDiagonally;
                this.Slow = configSource.Slow;
                this.Tolerance = configSource.Tolerance;
            }
        }

        public bool Slow
        {
            get { return slow; }
            set { slow = value; }
        }

        public Color FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }

        public bool FillDiagonally
        {
            get { return fillDiagonally; }
            set { fillDiagonally = value; }
        }

        public byte[] Tolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }
        public string errMeesage = "";
        public virtual bool SetBitmap(Bitmap bmp)
        {
            if (bmp.PixelFormat == PixelFormat.Format32bppArgb || bmp.PixelFormat == PixelFormat.Format24bppRgb || bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                //TODO: Right now only 32bpp is supported. We may also want to allow for other pixel formats.
                Bitmap = new EditableBitmap(bmp, PixelFormat.Format32bppArgb);
                return true;
            }
            else
            {
                errMeesage = "The bitmap you selected is the wrong pixel format!  This utility only works with 8-bit indexed/grayscale, 24bpp RGB, and 32bpp ARGB pixel formats.";
                return false;
            }            
        }
        public EditableBitmap Bitmap
        {
            get { return bitmap; }
            set 
            { 
                bitmap = value;
            }
        }

        public abstract void FloodFill(Point pt);
        public abstract List<Vector32> TraceFilledEdges(Rectangle drawrect, double minx,double miny,double maxx,double maxy);
        protected void PrepareForFloodFill(Point pt)
        {   
            //cache data in member variables to decrease overhead of property calls
            //this is especially important with Width and Height, as they call
            //GdipGetImageWidth() and GdipGetImageHeight() respectively in gdiplus.dll - 
            //which means major overhead.
            byteFillColor = new byte[] { fillColor.B, fillColor.G, fillColor.R };
            bitmapStride=bitmap.Stride;
            bitmapPixelFormatSize=bitmap.PixelFormatSize;
            bitmapBits = bitmap.Bits;
            bitmapWidth = bitmap.Bitmap.Width;
            bitmapHeight = bitmap.Bitmap.Height;

            pixelsChecked = new bool[bitmapBits.Length / bitmapPixelFormatSize];
        }
    }
}
