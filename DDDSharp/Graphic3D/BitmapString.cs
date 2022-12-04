using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
namespace DataCollection
{
    public enum TextVerticalAlignment
    {
        Top = 1,
        Bottom = 2,
        Center = 4,        
    }
    public enum TextHorizontalAlignment
    {
        Left = 8,
        Center = 16,
        Right = 24
    }
    internal class BitmapString
    {
        public Font Font { get; set; } = new Font(SystemFonts.DefaultFont.Name, 32, FontStyle.Regular);
        public Color Color { get; set; } = Color.Black;
        public string Text { get; set; } = "";
        public Color BackgroundColor { get; set; } = Color.White;
        
        // each glyph maps to a bitmap
        //private static Dictionary<BitmapString, Bitmap> _cache = new Dictionary<BitmapString, Bitmap>();

        public BitmapString(string text, Font font, Color color, Color backColor)
        {
            Text = text;
            Font = font;
            Color = color;
            BackgroundColor = backColor;
        }

        // return a Bitmap rendering of the glyph
        public Bitmap Draw(bool backgroundTransparent = true)
        {
            // has this string been already drawn ? 
            //if (_cache.ContainsKey(this))
            //{
            //    return _cache[this];
            //}

            Bitmap _bmp = new Bitmap(1, 1);            
            
            // Create a graphics object to measure the text's width and height.
            Graphics g = Graphics.FromImage(_bmp);
            
            // This is where the bitmap size is determined.
            int intWidth = (int)g.MeasureString(Text, Font).Width;
            int intHeight = (int)g.MeasureString(Text, Font).Height;

            // Create the bmpImage again with the correct size for the text and font.
            _bmp = new Bitmap(intWidth, intHeight, PixelFormat.Format32bppArgb);

            // Add the colors to the new bitmap.
            g = Graphics.FromImage(_bmp);

            // Set Background color          
            Color bk = BackgroundColor;
            if (backgroundTransparent) bk = Color.FromArgb(0, BackgroundColor);
            g.Clear(bk);
            g.FillRectangle(new SolidBrush(bk),new Rectangle(0,0,intWidth,intHeight));
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;            
            g.DrawString(Text, Font, new SolidBrush(Color), 0, 0);
            g.Flush();            

            return _bmp;
        }      

        public override int GetHashCode()
        {
            return Font.GetHashCode() ^ Color.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if ( !(obj is BitmapString) )
            {
                return false;
            }

            BitmapString bmpstr = (BitmapString)obj;
            return Color.Equals(bmpstr.Color) && 
                   Font.Equals(bmpstr.Font) && 
                   bmpstr.Text == Text;
        }
    }
}
