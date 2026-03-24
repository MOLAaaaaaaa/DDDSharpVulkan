using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using TextReaderWriter;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Globalization;
using GlmNet;
using Graphics3D;
using OpenCLNet;

namespace DataCollection
{
    public struct LineStyle
    {
        public float Width;
        public DashStyle Style;
        public Color Color;
        public LineStyle(Color _color,float _width = 1.0f, DashStyle _style = DashStyle.Solid)
        {
            Width = _width;
            Color = _color;
            Style = _style;
        }
        public bool SaveAs(BinaryWriter br)
        {
            br.Write(Width);
            br.Write((int)Style);
            br.Write(Color.ToArgb());
            return true;
        }
        public bool LoadFrom(BinaryReader br)
        {
            Width = br.ReadSingle();
            Style = (DashStyle)br.ReadInt32();
            Color = Color.FromArgb(br.ReadInt32());
            return true;
        }
    }
    
   

    public struct ColorPaletteStruct
    {
        public string ColorName;
        public string ColorSurname;
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;
        public Color toColor()
        {
            return Color.FromArgb(Alpha, Red, Green, Blue);
        }        
    }

    public class ColorPalette
    {
        public List<ColorPaletteStruct> PaletteColors = new List<ColorPaletteStruct>();
        public ColorPalette()
        {
            string[] colorStrings = new string[]
            {
                "Black,Black,0,0,0",
                "Black90,90% Black,25,25,25",
                "Black80,80% Black,51,51,51",
                "Black70,70% Black,77,77,77",
                "Black60,60% Black,102,102,102",
                "Black50,50% Black,128,128,128",
                "Black40,40% Black,153,153,153",
                "Black30,30% Black,179,179,179",
                "Black20,20% Black,204,204,204",
                "Black10,10% Black,230,230,230",
                "White,White,255,255,255",
                "Blue,Blue,0,0,255",
                "Cyan,Cyan,0,255,255",
                "Green,Green,0,255,0",
                "Yellow,Yellow,255,255,0",
                "Red,Red,255,0,0",
                "Magenta,Magenta,255,0,255",
                "Purple,Purple,153,0,204",
                "Orange,Orange,255,102,0",
                "Pink,Pink,255,153,204",
                "DarkBrown,Dark Brown,102,51,51",
                "PowderBlue,Powder Blue,204,204,255",
                "PastelBlue,Pastel Blue,153,153,255",
                "BabyBlue,Baby Blue,102,153,255",
                "ElectricBlue,Electric Blue,102,102,255",
                "TwilightBlue,Twilight Blue,102,102,204",
                "NavyBlue,Navy Blue,0,51,153",
                "DeepNavyBlue,Deep Navy Blue,0,0,102",
                "DesertBlue,Desert Blue,51,102,153",
                "SkyBlue,Sky Blue,0,204,255",
                "IceBlue,Ice Blue,153,255,255",
                "LightBlueGreen,Light BlueGreen,153,204,204",
                "OceanGreen,Ocean Green,102,153,153",
                "MossGreen,Moss Green,51,102,102",
                "DarkGreen,Dark Green,0,51,51",
                "ForestGreen,Forest Green,0,102,51",
                "DarkBlueGreen,Dark Blue Green,0,128,0",
                "GrassGreen,Grass Green,0,153,51",
                "KentuckyGreen,Kentucky Green,51,153,102",
                "LightGreen,Light Green,51,204,102",
                "SpringGreen,Spring Green,51,204,51",
                "Turquoise,Turquoise,102,255,204",
                "SeaGreen,Sea Green,51,204,153",
                "FadedGreen,Faded Green,153,204,153",
                "GhostGreen,Ghost Green,204,255,204",
                "MintGreen,Mint Green,153,255,153",
                "ArmyGreen,Army Green,102,153,102",
                "AvocadoGreen,Avocado Green,102,153,51",
                "MartianGreen,Martian Green,153,204,51",
                "DullGreen,Dull Green,153,204,102",
                "Chartreuse,Chartreuse,153,255,0",
                "MoonGreen,Moon Green,204,255,102",
                "MurkyGreen,Murky Green,51,51,0",
                "OliveDrab,Olive Drab,102,102,51",
                "Khaki,Khaki,153,153,102",
                "Olive,Olive,153,153,51",
                "BananaYellow,Banana Yellow,204,204,51",
                "LightYellow,Light Yellow,255,255,102",
                "Chalk,Chalk,255,255,153",
                "PaleYellow,Pale Yellow,255,255,204",
                "Brown,Brown,153,102,51",
                "RedBrown,Red Brown,204,102,51",
                "Gold,Gold,204,153,51",
                "AutumnOrange,Autumn Orange,255,102,51",
                "LightOrange,Light Orange,255,153,51",
                "Peach,Peach,255,153,102",
                "DeepYellow,Deep Yellow,255,204,0",
                "Sand,Sand,255,204,153",
                "Walnut,Walnut,102,51,0",
                "RubyRed,Ruby Red,153,0,0",
                "BrickRed,Brick Red,204,51,0",
                "TropicalPink,Tropical Pink,255,102,102",
                "SoftPink,Soft Pink,255,153,153",
                "FadedPink,Faded Pink,255,204,204",
                "DarkRed,Dark Red,128,0,0",
                "Crimson,Crimson,153,51,102",
                "RegalRed,Regal Red,204,51,102",
                "DeepRose,Deep Rose,204,51,153",
                "NeonRed,Neon Red,255,0,102",
                "DeepPink,Deep Pink,255,102,153",
                "HotPink,Hot Pink,255,51,153",
                "DustyRose,Dusty Rose,204,102,153",
                "Plum,Plum,102,0,102",
                "DeepViolet,Deep Violet,153,0,153",
                "LightViolet,Light Violet,255,153,255",
                "Violet,Violet,204,102,204",
                "DustyPlum,Dusty Plum,153,102,153",
                "PalePurple,Pale Purple,204,153,204",
                "MajesticPurple,Majestic Purple,153,51,204",
                "NeonPurple,Neon Purple,204,51,255",
                "LightPurple,Light Purple,204,102,255",
                "TwilightViolet,Twilight Violet,153,102,204",
                "EasterPurple,Easter Purple,204,153,255",
                "DeepPurple,Deep Purple,51,0,102",
                "Grape,Grape,102,51,153",
                "BlueViolet,Blue Violet,153,102,255",
                "BluePurple,Blue Purple,153,0,255",
                "DeepRiver,Deep River,102,0,204",
                "DeepAzure,Deep Azure,102,51,255",
                "StormBlue,Storm Blue,51,0,153",
                "DeepBlue,Deep Blue,51,0,204",
                "DarkBlue,Dark Blue,0,0,128",
            };
            string[] ss;
            char[] cc = new char[] { ',', ';' };
            for( int i = 0; i < colorStrings.Length; i++ )
            {
                ss = colorStrings[i].Split(cc,StringSplitOptions.RemoveEmptyEntries);
                ColorPaletteStruct color = new ColorPaletteStruct();
                color.ColorName = ss[0].Trim();
                color.ColorSurname = ss[1].Trim();
                color.Red = byte.Parse(ss[2]);
                color.Green = byte.Parse(ss[3]);
                color.Blue = byte.Parse(ss[4]);
                color.Alpha = 255;
                PaletteColors.Add(color);
            }
        }

        public Color GetColorFromPalette(string colorName)
        {
            foreach(ColorPaletteStruct cs in PaletteColors)
            {
                if (colorName.Equals(cs.ColorName) || colorName.Equals(cs.ColorSurname))
                    return cs.toColor();
            }
            return Color.Black;
        }
        public Color GetColorFromString(string colorstring)
        {
            string[] ss = colorstring.Split(new char[] {' ','\t'},StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > 3)
            {
                byte red, green, blue, alpha;
                red = byte.Parse(ss[0].Trim('R'));
                green = byte.Parse(ss[1].Trim('G'));
                blue = byte.Parse(ss[2].Trim('B'));
                alpha = byte.Parse(ss[3].Trim('A'));
                return Color.FromArgb(alpha, red, green, blue);
            }
            else return GetColorFromPalette(colorstring);                
        }
    }
    public struct ColorLevel
    {
        public float LevelValue { get; set; } //percentage 0 - 100
        public float R, G, B, A;//0-1,
        public bool Visible { get; set; }
        public TextureStruct Texture;
        public override bool Equals(object obj)
        {
            ColorLevel a = (ColorLevel)obj;
            if (a.R != R) return false;
            if (a.G != G) return false;
            if (a.B != B) return false;
            if (a.A != A) return false;
            if (a.Visible != Visible) return false;
            return true;
        }

        public Color Color
        { 
            get 
            {
                return Color.FromArgb((int)(A*255), (int)(R*255), (int)(G*255), (int)(B*255) );
            }
            set
            {
                R = value.R / 255.0f;
                G = value.G / 255.0f;
                B = value.B / 255.0f;
                A = value.A / 255.0f;
            }
        }
       public ColorLevel(float _val,float _r,float _g,float _b,float _a = 1,bool visible = true)
       {
            LevelValue = _val;   //percentage val %
            R = _r;
            G = _g;
            B = _b;
            A = _a;
            Visible = visible;
            Texture = new TextureStruct();
       }
        public void Read(BinaryReader br)
        {
            LevelValue = br.ReadSingle();
            R = br.ReadSingle();
            G = br.ReadSingle();
            B = br.ReadSingle();            
            Visible = br.ReadBoolean();
            A = br.ReadSingle();
        }
        public void Write(BinaryWriter br)
        {
            br.Write(LevelValue);
            br.Write(R);
            br.Write(G);
            br.Write(B);
            br.Write(Visible);
            br.Write(A);            
        }
        // list.Sort()时会根据该CompareTo()进行自定义比较
        public int CompareTo(ColorLevel other)
        {
            if (this.LevelValue != other.LevelValue)
            {
                return this.LevelValue.CompareTo(other.LevelValue);
            }
            else return 0;
        }
    }
    /// <summary>
    /// 颜色类转换TypeConverter
    /// </summary>
    public class ColorScaleConverter : TypeConverter
    {
        /// <summary>
        /// 是否支持属性显示
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is CColorScale )
            {
                System.Windows.Forms.PropertyGridInternal.IRootGridEntry cc = context as System.Windows.Forms.PropertyGridInternal.IRootGridEntry;
               
                CColorScale scale = value as CColorScale;
                return scale.Name;
            }
            else
            {
                return "";
                //return base.ConvertTo(context, culture, value, destinationType);
            }
        }
        /// <summary>
        /// 返回属性文本的集合定义
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var properties = TypeDescriptor.GetProperties(value);
            return properties;
        }        
    }

    public class ColorEditor: UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
            //return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            var edSvc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (edSvc != null)
            {
                CColorScale colorscale = value as CColorScale;
                var dlg = new ColorScaleForm();
                dlg.colorscale = colorscale;                
                if (dlg.ShowDialog() == DialogResult.OK) return dlg.colorscale;                
                else return value;
            }
            return base.EditValue(context, provider, value);
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override void PaintValue(PaintValueEventArgs e)
        {
            Graphics g = e.Graphics;            
            CColorScale colors = e.Value as CColorScale;
            //base.PaintValue(e);
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 1));
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Y, 1, e.Bounds.Height));
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.Width, e.Bounds.Y, 1, e.Bounds.Height));
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Height, e.Bounds.Width, 1));

            if (colors != null)
            {
                float x = e.Bounds.X;
                float w = 100, h = e.Bounds.Height;
                RectangleF rect;
                float x1 = 0, x2 = 0;
                for (int i = 0; i < colors.Count - 1; i++)
                {                    
                    x2 = (float)(w * colors[i].LevelValue / 100.0);
                    if ((x2 - x1) > 0)
                    {
                        using (var b = new SolidBrush(colors[i].Color))
                        {
                            rect = new RectangleF(x1, 0, (x2 - x1), h);
                            g.FillRectangle(b, rect);
                            x1 = x2;
                        }
                    }//if ((x2 - x1) > 0)
                }//for (int i = 0; i < colors.Count - 1; i++)
            }//if (colors != null)
        }//void PaintValue(PaintValueEventArgs e)
    }
    public class CColorScale
    {
        public List<ColorLevel> Levels { get; set; } = new List<ColorLevel>();
        public int Count { get { return Levels.Count; } }
        public double minv { get; set; } = 0;   //first value
        public double maxv { get; set; } = 100; // end value
        public bool IsSmooth { get; set; } = true;
        public string errMessage = "";
        public string Name { get; set; } = "";

        //隐藏值ranges
        public List<vec2> closedValues = new List<vec2>();
        /// <summary>
        /// 添加到数组，同时合并相连的值范围
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        void AddtoClosedValues(float v1,float v2)
        {
            int n = closedValues.Count;
            if ( n > 0 )
            {
                vec2 p1 = closedValues[n - 1];
                if( p1.y == v1 ) 
                {
                    p1.y = v2;
                    closedValues[n - 1] = p1;
                    return;
                }
            }
            closedValues.Add(new vec2(v1,v2));
        }
        public List<vec2> CreateClosedValues()
        {
            closedValues.Clear();
            double v1, v2;
            for (int i = 0; i < Count; i++)
            {
                if (!Levels[i].Visible)
                {
                    if (i == 0)
                    {
                        v1 = GetScaledValue(0);
                        v2 = 0.5 * (GetScaledValue(i) + GetScaledValue(i + 1));
                    }
                    else if (i == Count - 1)
                    {
                        v1 = 0.5 * (GetScaledValue(i) + GetScaledValue(i - 1));
                        v2 = GetScaledValue(i);
                    }
                    else
                    {
                        v1 = 0.5 * (GetScaledValue(i) + GetScaledValue(i - 1));
                        v2 = 0.5 * (GetScaledValue(i) + GetScaledValue(i + 1));
                    }
                    AddtoClosedValues((float)v1, (float)v2);                    
                }
            }
            return closedValues;
        }

        static public Color ToColor(ColorRGBA color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        static public Color ToColor(ColorLevel color)
        {
            return Color.FromArgb((int)(color.A * 255),
                                    (int)(color.R * 255),
                                    (int)(color.G * 255),
                                    (int)(color.B * 255));
        }
        public ColorLevel this[int index]
        {
            get { return Levels[index]; }
            set { Levels[index] = value; }
        }
        //Color Reverse
        public void Reverse()
        {
            ColorLevel l1,l2;
            int n = Levels.Count;
            Color c1, c2;
            for( int i = 0; i < Levels.Count; i++ )
            {
                if ( i >= n - 1 - i) break;
                l1 = Levels[i];
                l2 = Levels[n - 1 - i];
                c1 = l1.Color;
                c2 = l2.Color;
                l1.Color = c2;
                l2.Color = c1;
                Levels[i] = l1;
                Levels[n - 1 - i] = l2;
            }            
        }
        public bool DifferentFrom(CColorScale obj)
        {
            if (obj.Count != Count) return true;
            if (obj.minv != minv) return true;
            if (obj.maxv != maxv) return true;
            if (obj.IsSmooth != IsSmooth) return true;
            for( int i = 0; i < Count; i++ )
            {
                if ( !Levels[i].Equals(obj.Levels[i]) ) return true;
            }
            return false;
        }

        public double GetScaledValue(int id)
        {
            return minv + Levels[id].LevelValue * (maxv - minv) / 100;
        }
        //level value 0 - 100  (%)
        public double GetScaledValue(double _levelScale)
        {
            return minv + _levelScale * (maxv - minv) / 100;
        }
        public void SetColor(int id,Color color)
        {
            ColorLevel cl = Levels[id];
            cl.R = (float)(color.R / 255.0);
            cl.G = (float)(color.G / 255.0);
            cl.B = (float)(color.B / 255.0);
            cl.A = (float)(color.A / 255.0);
            Levels[id] = cl;
        }
        public void SetVisible(int id, bool visible)
        {
            ColorLevel cl = Levels[id];
            cl.Visible = visible;
            Levels[id] = cl;
        }
        /// <summary>
        /// 根据scale的visible，设置当前色标的visible
        /// </summary>
        /// <param name="scale"></param>
        public void SetVisibleFrom(CColorScale scale)
        {
            double val;
            int n1, n2;
            for (int i = 0; i < scale.Count; i++)
            {
                if (!scale[i].Visible)
                {
                    val = scale.GetScaledValue(i);
                    if (SearchBoder(val, out n1, out n2))
                    {
                        SetVisible(n1, false);
                    }
                }
            }
        }
        public void SetTexture(int id, TextureStruct texture)
        {
            ColorLevel cl = Levels[id];
            cl.Texture = texture;
            Levels[id] = cl;
        }
        public void AddLevel(float value,Color color)
        {
            Levels.Add(new ColorLevel(value,color.R/255f, color.G / 255f, color.B / 255f, color.A / 255f));
        }
        public void AddLevel(ColorLevel level)
        {
            Levels.Add(level);
        }
        public CColorScale Copy()
        {
            CColorScale c1 = new CColorScale();            
            c1.Levels.Clear();
            c1.Name = Name;
            c1.minv = minv;
            c1.maxv = maxv;            
            c1.IsSmooth = IsSmooth;
            c1.Levels.AddRange(Levels);            
            return c1;
        }
        public void DrawColorBar(Graphics e,Rectangle rect)
        {
            float ww = rect.Width / (Count - 1);
            float hh = rect.Bottom - rect.Top;
            float x1, y1;
            y1 = rect.Top;
            Color color;
            ColorLevel cl;
            for(int i=0;i< Count; i++)
            {
                cl = Levels[i];
                color = ToColor(cl);
                SolidBrush br = new SolidBrush(color);
                x1 = rect.Left + ww * i;                
                e.FillRectangle(br, x1, y1, ww, hh);
            }
        }
        
        public void DrawColorBarWithArrow(Graphics e, Rectangle rect)
        {
            if (Levels.Count < 2) return;
            double v1 = Levels[0].LevelValue;
            double v2 = Levels[Levels.Count-1].LevelValue;
            double hs1 = 0.3, hs2 = 0.4, hs3 = 0.3; //比例箭头、颜色、刻度

            double h1 = 0;
            double h2 = h1 + rect.Height * hs1;
            double h3 = h2 + rect.Height * hs2;
            double h4 = rect.Bottom;
            double margin = 6;
            double width = rect.Width - margin*2;
            
            double x1, x2,y1;            
            Color color;
            ColorLevel c1,c2;
            
            for (int i = 0; i < Count; i++)
            {
                c1 = Levels[i];                
                color = ToColor(c1);
                x1 = rect.Left + margin + (c1.LevelValue - v1) / (v2 - v1) * width;
                y1 = h2;
                if ( i == Count-1 )
                {
                    x2 = rect.Right;
                }
                else
                {
                    c2 = Levels[i + 1];                    
                    x2 = rect.Left + margin + (c2.LevelValue - v1) / (v2 - v1) * width;                    
                }
                SolidBrush br = new SolidBrush(color);
                e.FillRectangle(br, (float)x1, (float)y1, (float)(x2 - x1), (float)(h3 - h2));
            }

            //draw arrow
            double arrowWidth = 12;
            double arrowHeight = 16;
            for (int i = 0; i < Count; i++)
            {
                c1 = Levels[i];
                color = ToColor(c1);
                x1 = rect.Left + margin + (c1.LevelValue - v1) / (v2 - v1) * width - arrowWidth / 2f;
                y1 = h2-arrowHeight;                
                RectangleF rect1 = new RectangleF((float)x1, (float)y1, (float)arrowWidth, (float)arrowHeight);
               GeometryDrawing.DrawArrow(e, rect1, Color.Blue, Color.Black, DirectionEnum.down);                
            }

        }

        void Init(double v1,double v2)
        {
            byte[] color_array =
            {
                0,0,160,
                0,0,255,
                65,105,225,
                100,149,237,
                46,139,87,
                32,178,170,
                50,205,50,
                0,250,154,
                127,255,0,
                124,252,0,
                173,255,47,
                225,255,255,
                240,255,255,
                255,255,0,
                255,228,181,
                245,222,179,
                255,182,193,
                255,192,203,
                255,215,0,
                255,165,0,
                255,160,122,
                255,127,80,
                255,0,0,
                238,130,238,
                186,85,211,
                153,50,204,
                128,0,128
            };
            double percent;
            int n = color_array.Length / 3;
            for (int i = 0; i < n; i++)
            {
                percent = 100.0 * (double)i / (n - 1);
                ColorLevel cl = new ColorLevel((float)percent,
                                                (float)(color_array[3 * i] / 255.0),
                                                (float)(color_array[3 * i + 1] / 255.0),
                                                (float)(color_array[3 * i + 2] / 255.0));
                Levels.Add(cl);
            }

            IsSmooth = true;
            SetValueRange(v1, v2);
        }
        public CColorScale()
        {
            Init(0, 100);
        }
        public CColorScale(double v1 = 0,double v2 = 100 )
        {
            Init(v1, v2);
        }
        /// <summary>
        /// 转换成颜色等级数组
        /// </summary>
        /// <returns></returns>
        public List<ColorLevel> toColorLevels()
        {
            List<ColorLevel> levels = new List<ColorLevel>(Levels);
            levels.Add(new ColorLevel((float)minv, 0, 0, 0));
            levels.Add(new ColorLevel((float)maxv, 0, 0, 0));
            return levels;
        }
        /// <summary>
        /// 从颜色等级数组中获取
        /// </summary>
        /// <returns></returns>
        public bool fromColorLevels(List<ColorLevel>levels)
        {
            int n = levels.Count;
            if (n < 2) return false;

            minv = levels[n - 2].LevelValue;
            maxv = levels[n - 1].LevelValue;
            
            Levels.Clear();            
            for (int i = 0; i < n - 2; i++)
                Levels.Add(levels[i]);

            return true;
        }

        public bool WriteBinary(BinaryWriter br)
        {
            C3DData.SaveString(br,Name);
            br.Write(minv);
            br.Write(maxv);
            br.Write(IsSmooth);
            br.Write((Int32)Count);
            foreach(ColorLevel cl in Levels )
            {
                cl.Write(br);
            }
            return true;
        }
        public bool LoadBinary(BinaryReader br)
        {
            Name = C3DData.LoadString(br);
            minv = br.ReadDouble();
            maxv = br.ReadDouble();
            IsSmooth = br.ReadBoolean();
            int n = br.ReadInt32();
            if ( minv >= maxv || n < 0 || n > 10000)
            {
                errMessage = "Warning: invalid minimum and maximum values. \n" + minv + " to " + maxv;
                //return false;
            }            
            Levels.Clear();
            for(int i=0;i<n;i++)
            {
                ColorLevel cl = new ColorLevel(0,0,0,0);
                cl.Read(br);
                Levels.Add(cl);
            }            
            return true;
        }
        public bool WriteStream( StreamWriter wr )
        {
            string str = "[ColorMap]";
            wr.WriteLine(str);
            
            str = "Minimum = " + minv;
            wr.WriteLine(str);

            str = "Maximum = " + maxv;
            wr.WriteLine(str);

            str = "Smooth = " + IsSmooth;
            wr.WriteLine(str);
            
            str = "ColorNum = " + Count;
            wr.WriteLine(str);
            
            foreach(ColorLevel cl in Levels )
            {
                str = cl.LevelValue + ", " + cl.R + "," + cl.G + "," + cl.B + "," + cl.A;
                if (cl.Visible) str += ",TRUE";
                else str += ",FALSE";
                wr.WriteLine(str);
            }
            return true;
        }
        public bool ReadStream(ref StreamReader sr)
        {
            //if ( !AscIIProfile.SeekSection("[ColorMap]",sr) )return false;
            Levels.Clear();
            
            int n = 0;
            double v1, v2;
            bool smooth = true;
            AscIIProfile.ReadDoubleValue(sr, "Minimum", out v1);
            AscIIProfile.ReadDoubleValue(sr, "Maximum", out v2);
            AscIIProfile.ReadBoolValue(sr, "Smooth", out smooth);
            AscIIProfile.ReadIntValue(sr, "ColorNum", out n);
            IsSmooth = smooth;
            if ( n <= 0 || n > 10000 || v1 > v2 ) return false;

            minv = v1;
            maxv = v2;

            string str;
            string[] ss;
            for (int i = 0; i < n; i++)
            {
                str = AscIIProfile.ReadLine(sr);
                if (str == null) return false;

                str = str.Replace((char)9, ' ');
                ss = str.Split(new Char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length < 5) return false;

                ColorLevel cl = new ColorLevel();
                cl.LevelValue = Convert.ToSingle(ss[0]);
                cl.R = Convert.ToSingle(ss[1]);
                cl.G = Convert.ToSingle(ss[2]);
                cl.B = Convert.ToSingle(ss[3]);
                cl.A = Convert.ToSingle(ss[4]);
                cl.Visible = Convert.ToBoolean(ss[5]);
                Levels.Add(cl);
            }
            ss = null;
            return true;
        }
        public bool LoadClr(string path)
        {
            //ColorMap 1 1
            //ColorMap 2 1
            //0.000000 179 179 179
            //11.111111   0   0 255
            //100   0   0 255
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                float r, g, b;
                float percent;
                string[] str;
                char[] cc = new char[] { ' ', '\t', ',' };
                string line;

                Levels.Clear();
                if( AscIIProfile.SeekSection("ColorMap",ref sr) )
                {
                    while( ( line = AscIIProfile.ReadLine(sr) ) != null )
                    {
                        str = line.Split(cc, StringSplitOptions.RemoveEmptyEntries);

                        percent = float.Parse(str[0]);//百分比0 - 100
                        if (percent < 0) percent = 0;
                        if (percent > 100) percent = 100;
                        r = float.Parse(str[1]) / 255;
                        g = float.Parse(str[2]) / 255;
                        b = float.Parse(str[3]) / 255;
                        Levels.Add(new ColorLevel(percent, r, g, b));
                    }                    
                }
                
                sr.Close();
                fs.Close();
                /////////////////////////////
                if ( Levels.Count < 1 )
                {
                    errMessage = "no valid data.";
                    return false;
                }

                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        /// <summary>
        /// 将level值成等级百分比 0-100 %
        /// </summary>
        /// <param name="v1">数据最小值</param>
        /// <param name="v2">数据最大值</param>
        public void ProjectToPercentage(double v1, double v2)
        {
            minv = v1;
            maxv = v2;
            ProjectToPercentage();
        }
        /// <summary>
        /// 重新计算等级百分比 0-100 %
        /// </summary>        
        public void ProjectToPercentage()
        {            
            ColorLevel level;
            float percent;
            //to percentage
            for (int i = 0; i < Levels.Count; i++)
            {
                level = Levels[i];
                percent = (float)((level.LevelValue - minv) / (maxv - minv) * 100);
                level.LevelValue = percent;
                Levels[i] = level;
            }
            //升序排列
            Levels.Sort((p1, p2) =>
            {
                if (p1.LevelValue != p2.LevelValue)
                {
                    return p1.LevelValue.CompareTo(p2.LevelValue);
                }
                else return 0;
            });            
        }

        //从LVL文件载入，version 3
        public bool LoadLvl(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string line;
                bool ret = false;

                line = AscIIProfile.ReadLine(sr).ToLower();
                if ( line.Equals("lvl3") ) ret = LoadLvl3(sr);
                
                sr.Close();
                fs.Close();
                
                if (!ret) return false;
                
                /////////////////////////////
                if (Levels.Count < 1 || minv >= maxv)
                {
                    errMessage = "no valid data.";
                    return false;
                }

                ProjectToPercentage(minv,maxv);
                
                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }            
        }
        //找出匹配的字串对
        int GetMatchItems(string line, List<string> items,char left ='"',char right='"')
        {
            char[] cc = new char[] { ' ', '\t' };
            
            List<char> s1 = new List<char>();
            List<char> s2 = new List<char>();
            
            bool found = false;
            for( int i = 0; i < line.Length; i++ )
            {
                if( line[i] == '"' ) 
                {
                    if( found) //右匹配成功
                    {
                        items.Add(new string(s2.ToArray()));
                        s2.Clear();
                        found = false;
                    }
                    else //左括号
                    {
                        string str = new string(s1.ToArray());
                        string[] ss = str.Split(cc,StringSplitOptions.RemoveEmptyEntries);
                        if( ss != null )
                        {
                            foreach (string s in ss) items.Add(s);
                        }                        
                        s1.Clear();
                        found = true;
                    }
                }
                else
                {
                    if ( found ) s2.Add( line[i] );
                    else s1.Add( line[i] );
                }
            }
            if( s1.Count > 0 )
            {
                string str = new string(s1.ToArray());
                string[] ss = str.Split(cc, StringSplitOptions.RemoveEmptyEntries);
                if (ss != null)
                {
                    foreach (string s in ss) items.Add(s);
                }
                s1.Clear();
            }
            return items.Count;
        }
        public bool LoadLvl3(StreamReader sr)
        {
            /*
             * 
             * LVL3
               'Level Flags LColor LStyle LWidth FVersion FFGColor FBGColor FPattern OffsetX OffsetY ScaleX ScaleY Angle Coverage
               -2000 0 "Black" "Invisible" 0 1 "Red" "R0 G0 B0 A0" "Solid" 0 0 1 1 0 0
                5 0 "Black" "Invisible" 0 1 "R255 G11 B0 A255" "R15 G15 B15 A0" "Solid" 0 0 1 1 0 0
            */

            //100   0   0 255
            
            ColorPalette palette = new ColorPalette();

            try
            {   
                float percent;
                string line;
                Levels.Clear();
                int i = 0;

                List<string> items = new List<string>();

                while ((line = AscIIProfile.ReadLine(sr)) != null)
                {
                    if (line[0] == '\'' || line[0] == '/') continue; //注释行
                    if ( GetMatchItems(line, items) < 15) continue;

                    //str = line.Split(cc, StringSplitOptions.RemoveEmptyEntries);
                    //if (str == null || str.Length < 16) continue; //格式不正确

                    //颜色值，不转换percentage
                    if (!float.TryParse(items[0], out percent)) continue;

                    Color color = palette.GetColorFromString(items[6]);
                    AddLevel(percent, color);

                    if (i == 0) minv = maxv = percent;
                    else
                    {
                        if (percent < minv) minv = percent;
                        if (percent > maxv) maxv = percent;
                    }
                    items.Clear();

                    i++;
                }//while ((line = AscIIProfile.ReadLine(sr)) != null)
                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool SaveClr(string path)
        {
            //ColorMap 1 1
            //0.000000 179 179 179
            //11.111111   0   0 255
            //100   0   0 255
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                string ss = "ColorMap 1 1";
                sr.WriteLine(ss);

                foreach(ColorLevel cl in Levels)
                {
                    ss = "  " + cl.LevelValue;
                    ss += " " + Math.Round(cl.R * 255, 0);
                    ss += " " + Math.Round(cl.G * 255, 0);
                    ss += " " + Math.Round(cl.B * 255, 0);
                    sr.WriteLine(ss);
                }
                sr.Close();
                fs.Close();
                
                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        /// <summary>
        /// 设置值范围，v1最小值，v2最大值，
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="percentage">分配比例</param>
        public void SetValueRange(double v1, double v2,double[]percentage = null)
        {
            minv = v1;
            maxv = v2;
        }
        //search the border of value val
        //n1 lower n2 upper
        public bool SearchBoder(double val, out int n1, out int n2)
        {
            double level = (val - minv) / (maxv - minv) * 100;
            n1 = n2 = -1;
            if (level <= 0)
            {
                n1 = n2 = 0;
                return true;
            }
            else if (level >= 100)
            {
                n1 = n2 = Levels.Count - 1;
                return true;
            }
            //search in levels 0 - 100
            BinarySearch(level,out n1,out n2);
            
            return true;
        }
       /// <summary>
       /// 折半查找
       /// </summary>
       /// <param name="val"></param>
       /// <param name="low"></param>
       /// <param name="high"></param>
       /// <returns></returns>
        private int BinarySearch(double val,out int low,out int high)
        {
            low = 1;
            high = Levels.Count - 1;
            int mid = (low + high) / 2;
            while (low < high)
            {
                mid = (low + high) / 2;
                if ( Levels[mid].LevelValue == val)
                {
                    low = high = mid;
                    return low;
                }
                if (Levels[mid].LevelValue < val) low = mid;
                else high = mid;
                if (high - low <= 1)
                {
                    return low;
                }
            }
            return -1;
        }
        //search the border of value val
        //n1 lower n2 upper
        public bool SearchBoder2(double val,out int n1,out int n2)
        {
            double level = (val - minv) / (maxv - minv) * 100;
            n1 = n2 = -1;
            if (level <= 0) 
            {
                n1 = n2 = 0;
                return true;
            }
            else if ( level >= 100 )
            {
                n1 = n2 = Levels.Count-1;
                return true;
            }

            for (int i = 0; i < Levels.Count; i++ )
            {
                if( level == Levels[i].LevelValue )
                {
                    n1 = n2 = i;
                    return true;
                }
                else if (level > Levels[i].LevelValue)
                {
                    n1 = i;                    
                }
                else if (level < Levels[i].LevelValue)
                {
                    n2 = i;
                    break;
                }
            }
            return true;
        }
        public bool IsBlankValue(double v)
        {
            if (v < minv || v > maxv) return true;
            if (v == C3DData.m_BlankedValue) return true;
            return false;
        }        
        /// <summary>
        /// 根据一个值获取映射颜色Color结构 RGBA，0-255
        /// </summary>
        /// <param name="val">实际值</param>
        /// <returns></returns>
        public Color GetColor( double val )
        {
            if (Levels.Count < 1) return Color.Black;
            if (double.IsNaN(val)) return Color.FromArgb(0,0,0,0);

            if (val <= minv) return Levels[0].Color;
            else if (val >= maxv) return Levels[Count - 1].Color;

            int n1, n2;
            SearchBoder(val, out n1, out n2);            
            if (n1 < 0 || n2 < 0) return Levels[0].Color;
            if (n1 == n2) return Levels[n1].Color;
            
            double v1 = GetScaledValue(Levels[n1].LevelValue);
            double v2 = GetScaledValue(Levels[n2].LevelValue);

            if( IsSmooth )
            {
                double r1 = Levels[n1].R;
                double g1 = Levels[n1].G;
                double b1 = Levels[n1].B;
                double a1 = Levels[n1].A;

                double r2 = Levels[n2].R;
                double g2 = Levels[n2].G;
                double b2 = Levels[n2].B;
                double a2 = Levels[n2].A;

                double r, g, b, a;

                double scale = (val - v1) / (v2 - v1);
                if (scale < 0) scale = 0;
                if (scale > 1) scale = 1;                
                r = (r1 + (r2 - r1) * scale) * 255;
                g = (g1 + (g2 - g1) * scale) * 255;
                b = (b1 + (b2 - b1) * scale) * 255;
                a = (a1 + (a2 - a1) * scale) * 255;                
                return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
            }
            else
            {
                //if (val >= (v1 + v2) * 0.5) return Levels[n2].Color;
                //else 
                    return Levels[n1].Color;
            }            
        }
        
        public Color GetColor(int id)
        {
            if ( Levels.Count < 1 ) return Color.Black;
            if (id <= 0) return Levels[0].Color;
            else if (id >= Count - 1) return Levels[Count - 1].Color;
            else return Levels[id].Color;            
        }

        public int GetColorIndex(double val)
        {
            if ( Levels.Count < 1 ) return 0;
            if (val <= minv) return 0;
            else if (val >= maxv) return Count - 1;
            
            int n1, n2;

            SearchBoder(val, out n1, out n2);
            if (n2 > n1)
            {
                if (val >= 0.5 * (GetScaledValue(n1) + GetScaledValue(n2)))
                    return n2;
                else return n1;
            }
            else return n1;
        }
        
        
        static public void DrawColorBar(Graphics g, Rectangle rectClient, CColorScale colorscale)
        {
            int x = rectClient.Left;
            int y = rectClient.Top;
            int w = rectClient.Width;
            int h = rectClient.Height;

            int step = 1;
            int cn = (int)( w / (colorscale.Count / (double)step ) );
            while (cn < 2)
            {
                step++;
                cn = w / (colorscale.Count / step);
            }

            double nw = w / (colorscale.Count / (double)step);
            Rectangle rect = new Rectangle(x, y, x + cn, y + h);
            
            for (int i = 0; i < colorscale.Count; i += step)
            {
                using (var b = new SolidBrush(colorscale[i].Color))
                    g.FillRectangle(b, rect);
                x = (int)(x + nw);
                rect = new Rectangle(x, y, x + cn, y + h);
            }
        }
            
    }
}
