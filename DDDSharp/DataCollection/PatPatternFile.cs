using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using DataCollection;
using System.ComponentModel;
using System.Globalization;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace DataCollection
{
    //地质符号--
    public class GeoCodePattern
    {
        public Bitmap bitmap = null;
        public string ID = "";      //编码
        public string Name = "";    //名称
        public Color Background = Color.White;
        public GeoCodePattern Copy()
        {
            GeoCodePattern A = new GeoCodePattern();
            A.ID = ID;
            A.Name = Name;
            A.Background = Background;
            A.bitmap = new Bitmap(bitmap);
            return A;
        }
    }
    public struct PaternLineStruct
    {
        public float Angle;
        public float X;
        public float Y;
        public List<PointF> Offsets;
        public PaternLineStruct(float angle, float x, float y)
        {
            Angle = angle;
            X = x;
            Y = y;
            Offsets = new List<PointF>();
        }
        public void AddOffset(float dx, float dy)
        {
            Offsets.Add(new PointF(dx, dy));
        }
    }
    public class PatPatternFile
    {
        public string ID = "";//编号
        public string Name = "";
        public List<PaternLineStruct> Patterns = new List<PaternLineStruct>();
        public void Draw(Graphics g, Rectangle rect)
        {
            if (Patterns.Count < 1) return;
            PaternLineStruct line;
            float x1, y1, x2, y2;
            for (int i = 0; i < Patterns.Count; i++)
            {
                line = Patterns[i];

            }
        }
        void UpdateRange()
        {

        }
    }
   
}
