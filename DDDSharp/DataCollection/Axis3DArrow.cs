using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;

namespace DataCollection
{
    public enum ArrowTextAlignment
    {
        Horizontal = 0,
        Vertical = 1,
    }
    public class Axis3DArrow
    {
        [CategoryAttribute("Axis"), DisplayNameAttribute("Visible")]
        public bool Visible { get; set; } = true;

        [CategoryAttribute("Axis"), DisplayNameAttribute("Name")]
        public string AxisName { get; set; } = "X";

        [CategoryAttribute("Axis"), DisplayNameAttribute("CenterView")]
        public bool Center { get; set; } = false;        

        [CategoryAttribute("Arrow"), DisplayNameAttribute("Radiu")]        
        public float ArrowRadiu{ get; set; } = 0.016f;        
        
        [CategoryAttribute("Arrow"), DisplayNameAttribute("Height")]
        public float ArrowHight { get; set; } = 0.016f * 0.618f * 5;
        [CategoryAttribute("Arrow"), DisplayNameAttribute("Color")]
        public Color HeaderColor { get; set; } = Color.Blue;

        [CategoryAttribute("ArrowLine"), DisplayNameAttribute("Width")]
        public float LineWidth { get; set; } = 2f;
        [CategoryAttribute("ArrowLine"), DisplayNameAttribute("Length")]
        public float LineLength { get; set; } = 0.2f;

        [CategoryAttribute("ArrowLine"), DisplayNameAttribute("Color")]
        public Color LineColor { get; set; } = Color.Blue;

        [CategoryAttribute("Label"), DisplayNameAttribute("Font")]
        public Font labelFont { get; set; } = new Font("宋体", 64);
        [CategoryAttribute("Label"), DisplayNameAttribute("Color")]
        public Color labelColor { get; set; } = Color.Black;
        [CategoryAttribute("Label"), DisplayNameAttribute("Size")]
        public float labelSize { get; set; } = 0.1f;

        [CategoryAttribute("Label"), DisplayNameAttribute("Alignment")]
        public ArrowTextAlignment Alignment { get; set; } = ArrowTextAlignment.Horizontal;
        [CategoryAttribute("Label"), DisplayNameAttribute("Plane Angle")]
        public float PlaneAngle { get; set; } = 0;

        public Vector32 LabelOffset = new Vector32(0, 0, 0);
        [CategoryAttribute("Label"), DisplayNameAttribute("Translation")]
        public virtual string OffsetString
        {
            get { return LabelOffset.x + "," + LabelOffset.y + "," + LabelOffset.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    LabelOffset = new Vector32(p.X, p.Y, p.Z);                    
                }
            }
        }

        public Axis3DArrow(string name)
        {
            AxisName = name;
        }
        public Axis3DArrow Copy()
        {
            Axis3DArrow a = new Axis3DArrow(AxisName);
            a.AxisName = AxisName;
            a.Visible = Visible;
            a.Center = Center;
            a.ArrowRadiu = ArrowRadiu;
            a.ArrowHight = ArrowHight;
            a.LineWidth = LineWidth;
            a.LineLength = LineLength;
            a.HeaderColor = HeaderColor;
            a.LineColor = LineColor;
            a.labelFont = labelFont;
            a.labelColor = labelColor;
            a.labelSize = labelSize;
            a.Alignment = Alignment;
            a.PlaneAngle = PlaneAngle;
            a.LabelOffset = LabelOffset;
            return a;
        }
        public bool Save( BinaryWriter br )
        {
            C3DData.SaveString(br,AxisName);
            br.Write(Visible);
            br.Write(Center);
            br.Write(ArrowRadiu);
            br.Write(ArrowHight);
            br.Write(LineWidth);
            br.Write(LineLength);
            br.Write(HeaderColor.ToArgb());
            br.Write(LineColor.ToArgb());
            C3DData.SaveFont(br,labelFont);
            br.Write(labelColor.ToArgb());
            br.Write(labelSize);
            br.Write((int)Alignment);
            br.Write(PlaneAngle);
            br.Write(LabelOffset.X);
            br.Write(LabelOffset.Y);
            br.Write(LabelOffset.Z);
            return true;
        }
        public bool Load(BinaryReader br)
        {
            AxisName = C3DData.LoadString(br);
            Visible = br.ReadBoolean();
            Center = br.ReadBoolean();
            ArrowRadiu = br.ReadSingle();
            ArrowHight = br.ReadSingle();
            LineWidth = br.ReadSingle();
            LineLength = br.ReadSingle();
            HeaderColor = Color.FromArgb(br.ReadInt32());
            LineColor = Color.FromArgb(br.ReadInt32());
            labelFont = C3DData.LoadFont(br);
            labelColor = Color.FromArgb(br.ReadInt32());
            labelSize = br.ReadSingle();
            Alignment = (ArrowTextAlignment)br.ReadInt32();
            PlaneAngle = br.ReadSingle();
            LabelOffset.X = br.ReadSingle();
            LabelOffset.Y = br.ReadSingle();
            LabelOffset.Z = br.ReadSingle();
            return true;
        }
    }
}
