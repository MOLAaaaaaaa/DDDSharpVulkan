using System;
using System.IO;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using GlmNet;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DataCollection
{
    /// <summary>
    /// TexturedTextStyleConverter
    /// </summary>
    public class TexturedTextStyleConverter : ExpandableObjectConverter
    {
        //该方法判断此类型可以转换为哪些类型
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(TexturedTextStyle))
            {
                return true;
            }

            //调用基类方法处理其他情况            
            return base.CanConvertTo(context, destinationType);
        }
        //该方法判断哪些类型可以转换为此类型
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(TexturedTextStyle))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

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
            if (value is TexturedTextStyle)
            {
                //System.Windows.Forms.PropertyGridInternal.IRootGridEntry cc = context as System.Windows.Forms.PropertyGridInternal.IRootGridEntry;
                //CColorScale scale = value as CColorScale;
                //return scale.Name;
                return "TextStyle";
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
    public class TexturedTextStyleEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            //return UITypeEditorEditStyle.None;
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            return base.EditValue(context, provider, value);
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

    public class PropertyStyleEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            //return UITypeEditorEditStyle.None;
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            return base.EditValue(context, provider, value);
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
    /// <summary>
    /// TexturedTextStyleConverter
    /// </summary>
    public class PropertyStyleConverter : TypeConverter
    {
        //该方法判断此类型可以转换为哪些类型
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            //if (destinationType == typeof(TexturedTextStyle))
            {
                return true;
            }

            //调用基类方法处理其他情况            
            return base.CanConvertTo(context, destinationType);
        }
        //该方法判断哪些类型可以转换为此类型
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            //if (sourceType == typeof(TexturedTextStyle))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

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
            return value;
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
    public enum TextureMagFilter
    {
        //取最邻近像素
        GL_NEAREST = 0,
        //线性内部插值
        GL_LINEAR = 1,
        //最近多贴图等级的最邻近像素
        GL_NEAREST_MIPMAP_NEAREST = 2,
        // 在最近多贴图等级的内部线性插值
        GL_NEAREST_MIPMAP_LINEAR = 3,
        //在最近多贴图等级的外部线性插值
        GL_LINEAR_MIPMAP_NEAREST = 4,
        //在最近多贴图等级的外部和内部线性插值
        GL_LINEAR_MIPMAP_LINEAR = 5
    };
    public enum TextureWrapMode
    {
        REPEAT = 0,
        CLAMP = 1, 
        CLAMP_TO_EDGE = 2, 
        WRAP_TO_BORDER = 3,
    };

    public class TextureStruct
    {
        public string Name = "";
        public Bitmap bmp = null;        
        public bool IsValidate()
        {
            return Enable && TextureFile.Length > 0;
        }
        [CategoryAttribute("Texture"), DisplayNameAttribute("Enable")]
        public bool Enable { get; set; } = false;

        [CategoryAttribute("Texture"), DisplayNameAttribute("TexureFile")]
        [Editor(typeof(FileChooseEditor), typeof(UITypeEditor))]
        public virtual string TextureFile { get; set; } = "";

        [CategoryAttribute("Texture"), DisplayNameAttribute("Horizontal Flip")]
        public virtual bool FlipHorizontal { get; set; } = false;

        [CategoryAttribute("Texture"), DisplayNameAttribute("Vertical Flip")]
        public virtual bool FlipVertical { get; set; } = false;       

        [CategoryAttribute("Texture"), DisplayNameAttribute("Wrap Mode")]
        public TextureWrapMode wrapMode { get; set; } = TextureWrapMode.CLAMP;//重复 or 拉伸
        [CategoryAttribute("Texture"), DisplayNameAttribute("Mag Filter")]
        public TextureMagFilter mode { get; set; } = TextureMagFilter.GL_LINEAR;

        public DoubleRect textureRect = new DoubleRect(0,0,0,0);

        [CategoryAttribute("Texture"), DisplayNameAttribute("Range")]
        public string textureRectString
        { 
            get { return textureRect.toString(); }
            set { textureRect.fromString(value); }
        }
        
        [CategoryAttribute("Texture"), DisplayNameAttribute("Transparent Colors")]
        public List<TransparentColorStruct> TransparentColors { get; set; } = new List<TransparentColorStruct>();
        //颜色反转
        [CategoryAttribute("Texture"), DisplayNameAttribute("Transparent Inverse")]
        public bool TransparentInverse { get; set; } = false;

        //[CategoryAttribute("Texture"), DisplayNameAttribute("Blend")]
        //public bool Blend { get; set; } = false;
        //[CategoryAttribute("Texture"), DisplayNameAttribute("Blend Alpha")]
        //public float Alpha { get; set; } = 1.0f;

        public TextureStruct()
        {
            
        }
        public TextureStruct(Bitmap _bmp)
        {
            bmp = _bmp;
        }
        public TextureStruct(Bitmap _bmp, TextureMagFilter _mode,bool _enable,string _name)
        {
            bmp = _bmp;
            mode = _mode;
            Enable = _enable;
            Name = _name;
        }
        
        public TextureStruct Copy()
        {
            this.MemberwiseClone();
            TextureStruct t = new TextureStruct();            
            t.Name = Name;
            t.Enable = Enable;
            t.bmp = bmp;
            t.mode = mode;
            t.wrapMode = wrapMode;            

            //t.Blend = Blend;
            //t.Alpha = Alpha;
            t.TextureFile = TextureFile;
            t.FlipHorizontal = FlipHorizontal;
            t.FlipVertical = FlipVertical;
            t.textureRect = textureRect;
            t.TransparentInverse = TransparentInverse;
            t.TransparentColors = new List<TransparentColorStruct>(TransparentColors);
            return t;
        }
        bool IsInTransparentList(Color c)
        {    
            for(int i=0;i< TransparentColors.Count;i++)
            {
                TransparentColorStruct tc = TransparentColors[i];
                if( C3DData.IsSimilarColor(tc.Color,c, tc.Deviation) ) return true;
            }
            return false;
        }     

        public void MakeTransparent(Bitmap bmp)
        {
            if (TransparentColors.Count < 1) return;

            //1拷贝图像到数据bytes
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bd.Stride;
            byte[] bytes = new byte[bmp.Height* stride];
            Marshal.Copy(bd.Scan0, bytes, 0, bytes.Length);
            
            byte r, g, b, a;
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < stride; j += 4)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        b = bytes[i * stride + j];
                        g = bytes[i * stride + j + 1];
                        r = bytes[i * stride + j + 2];
                        a = bytes[i * stride + j + 3];
                    }
                    else 
                    {
                        a = bytes[i * stride + j];
                        r = bytes[i * stride + j + 1];
                        g = bytes[i * stride + j + 2];
                        b = bytes[i * stride + j + 3];
                    }

                    if (IsInTransparentList(Color.FromArgb(r, g, b)))
                    {
                        if (!TransparentInverse) a = 0;
                    }
                    else if (TransparentInverse) a = 0;

                    if (BitConverter.IsLittleEndian)
                        bytes[i * stride + j + 3] = a;
                    else bytes[i * stride + j] = a;                                     
                }
            }            
            //拷贝数据bytes到图像
            Marshal.Copy(bytes, 0, bd.Scan0, bytes.Length);
            bmp.UnlockBits(bd);
            bytes = null;
        }
        /// <summary>
        /// 将图片颜色置为透明,效率太低
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="err">与指定颜色差值误差</param>
        public void MakeTransparent_Old(Bitmap bmp)
        {
            if (TransparentColors.Count < 1) return;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(x,y);
                    if (IsInTransparentList(c))
                    {
                        c = Color.FromArgb(0, c);
                        bmp.SetPixel(x,y,c);
                    }                    
                }
            }
        }

        public bool Save(BinaryWriter br)
        {
            C3DData.SaveString(br, Name);
            br.Write(Enable);
            br.Write((Int16)mode);
            br.Write((Int16)wrapMode);
            C3DData.SaveString(br, TextureFile);
            br.Write(FlipHorizontal);
            br.Write(FlipVertical);
            //br.Write(Blend);
            //br.Write(Alpha);
            br.Write(textureRect.X1);
            br.Write(textureRect.Y1);
            br.Write(textureRect.X2);
            br.Write(textureRect.Y2);            
            br.Write(TransparentColors.Count);

            br.Write(TransparentInverse); //2025-8-28
            for (int i=0;i< TransparentColors.Count;i++)
            {
                br.Write(TransparentColors[i].Color.ToArgb());
                br.Write(TransparentColors[i].Deviation);
            }
            return true;
        }
        public bool Load(BinaryReader br,float version = 1.2f)
        {
            Name = C3DData.LoadString(br);
            Enable = br.ReadBoolean();
            mode = (TextureMagFilter)br.ReadInt16();
            wrapMode = (TextureWrapMode)br.ReadInt16();
            TextureFile = C3DData.LoadString(br);
            FlipHorizontal = br.ReadBoolean();
            FlipVertical = br.ReadBoolean();
            //if (version >= 1.3)//1.3
            //{
            //    Blend = br.ReadBoolean();
            //    Alpha = br.ReadSingle();
            //}
            if( C3DData.DataVersion>=1.27f )
            {
                double x1,y1, x2, y2;
                x1 = br.ReadDouble();
                y1 = br.ReadDouble();
                x2 = br.ReadDouble();
                y2 = br.ReadDouble();
                textureRect = new DoubleRect(x1,y1,x2,y2);
            }
            if (C3DData.DataVersion >= 1.28f)
            {
                TransparentColors.Clear();
                int n = br.ReadInt32();
                if (C3DData.DataVersion >= 1.31f)
                {
                    TransparentInverse = br.ReadBoolean();
                    for (int i = 0; i < n; i++)
                    {
                        Color c = C3DData.LoadColor(br);
                        float deviation = br.ReadSingle();
                        TransparentColors.Add(new TransparentColorStruct(c, deviation));
                    }
                }
                else 
                {
                    for (int i = 0; i < n; i++)
                    {
                        TransparentColors.Add(new TransparentColorStruct(C3DData.LoadColor(br)));
                    }
                }                
            }
            return true;
        }
        public int GetWidth()
        {
            if (bmp == null) return 0;
            return bmp.Width;
        }
        public int GetHeight()
        {
            if (bmp == null) return 0;
            return bmp.Height;
        }
    }

    public class TexturedTextStyle
    {
        public vec3 Offset = new vec3(0, 0, 0);
        public vec3 Scale = new vec3(1, 1, 1);
        public vec3 Rotate = new vec3(0, 0, 0);

        public bool DifferentFrom(TexturedTextStyle obj)
        {
            if (obj.Offset.x != Offset.x) return true;
            if (obj.Offset.y != Offset.y) return true;
            if (obj.Offset.z != Offset.z) return true;
            if (obj.Scale.x != Scale.x) return true;
            if (obj.Scale.y != Scale.y) return true;
            if (obj.Scale.z != Scale.z) return true;
            if (obj.Rotate.x != Rotate.x) return true;
            if (obj.Rotate.y != Rotate.y) return true;
            if (obj.Rotate.z != Rotate.z) return true;

            if (obj.Font.Size != Font.Size) return true;
            if (obj.Font.Name != Font.Name) return true;
            if (obj.Font.Strikeout != Font.Strikeout) return true;
            if (obj.Font.Style != Font.Style) return true;
            if (obj.Font.Bold != Font.Bold) return true;
            if (obj.Font.Underline != Font.Underline) return true;
            if (obj.Font.Height != Font.Height) return true;

            if (obj.LabelLength != LabelLength) return true;
            if (obj.Color != Color) return true;
            if (obj.BackgroundColor != BackgroundColor) return true;
            if (obj.BackgroundTransparent != BackgroundTransparent) return true;
            if (obj.VerticalAlignment != VerticalAlignment) return true;
            if (obj.HorizontalAlignment != HorizontalAlignment) return true;
            return false;
        }

        public TexturedTextStyle Copy()
        {
            TexturedTextStyle obj = new TexturedTextStyle();
            obj.Offset = Offset;
            obj.Scale = Scale;
            obj.Rotate = Rotate;
            obj.Font = Font;
            obj.LabelLength = LabelLength;
            obj.Color = Color;
            obj.FlipHorizontal = FlipHorizontal;
            obj.FlipVertical = FlipVertical;
            obj.BackgroundColor = BackgroundColor;
            obj.BackgroundTransparent = BackgroundTransparent;
            obj.VerticalAlignment = VerticalAlignment;
            obj.HorizontalAlignment = HorizontalAlignment;
            return obj;
        }
        bool SaveBinary120(BinaryWriter br)
        {
            br.Write(Offset.x);
            br.Write(Offset.y);
            br.Write(Offset.z);
            br.Write(Scale.x);
            br.Write(Scale.y);
            br.Write(Scale.z);
            br.Write(Rotate.x);
            br.Write(Rotate.y);
            br.Write(Rotate.z);
            //Font(string familyName, float emSize, FontStyle style);
            string familyName = Font.FontFamily.Name;
            C3DData.SaveString(br, familyName);
            br.Write(Font.Size);
            br.Write((Int32)Font.Style);
            br.Write(LabelLength);
            br.Write(Color.ToArgb());
            br.Write(BackgroundColor.ToArgb());
            br.Write(BackgroundTransparent);
            br.Write((Int32)VerticalAlignment);
            br.Write((Int32)HorizontalAlignment);
            return true;
        }
        bool SaveBinary121(BinaryWriter br)
        {
            br.Write(Offset.x);
            br.Write(Offset.y);
            br.Write(Offset.z);
            br.Write(Scale.x);
            br.Write(Scale.y);
            br.Write(Scale.z);
            br.Write(Rotate.x);
            br.Write(Rotate.y);
            br.Write(Rotate.z);
            //Font(string familyName, float emSize, FontStyle style);
            string familyName = Font.FontFamily.Name;
            C3DData.SaveString(br, familyName);
            br.Write(Font.Size);
            br.Write((Int32)Font.Style);
            br.Write(LabelLength);
            br.Write(Color.ToArgb());
            br.Write(FlipHorizontal);
            br.Write(FlipVertical);
            br.Write(BackgroundColor.ToArgb());
            br.Write(BackgroundTransparent);
            br.Write((Int32)VerticalAlignment);
            br.Write((Int32)HorizontalAlignment);
            return true;
        }
        public bool SaveBinary(BinaryWriter br)
        {
            return SaveBinary121(br);
        }
        public bool LoadFrom(BinaryReader br)
        {
            if (C3DData.DataVersion <= 1.2f) return LoadFrom120(br);
            else return LoadFrom121(br);
        }
        bool LoadFrom120(BinaryReader br)
        {
            Offset.x = br.ReadSingle();
            Offset.y = br.ReadSingle();
            Offset.z = br.ReadSingle();
            Scale.x = br.ReadSingle();
            Scale.y = br.ReadSingle();
            Scale.z = br.ReadSingle();
            Rotate.x = br.ReadSingle();
            Rotate.y = br.ReadSingle();
            Rotate.z = br.ReadSingle();

            string familyName = C3DData.LoadString(br);
            FontFamily ff = new FontFamily(familyName);
            float size = br.ReadSingle();
            int style = br.ReadInt32();
            Font = new Font(ff, size, (FontStyle)style);

            LabelLength = br.ReadSingle();
            Color = Color.FromArgb(br.ReadInt32());
            //FlipHorizontal = br.ReadBoolean();
            //FlipVertical = br.ReadBoolean();
            BackgroundColor = Color.FromArgb(br.ReadInt32());
            BackgroundTransparent = br.ReadBoolean();
            VerticalAlignment = (TextVerticalAlignment)br.ReadInt32();
            HorizontalAlignment = (TextHorizontalAlignment)br.ReadInt32();

            return true;
        }
        bool LoadFrom121(BinaryReader br)
        {
            Offset.x = br.ReadSingle();
            Offset.y = br.ReadSingle();
            Offset.z = br.ReadSingle();
            Scale.x = br.ReadSingle();
            Scale.y = br.ReadSingle();
            Scale.z = br.ReadSingle();
            Rotate.x = br.ReadSingle();
            Rotate.y = br.ReadSingle();
            Rotate.z = br.ReadSingle();

            string familyName = C3DData.LoadString(br);
            FontFamily ff = new FontFamily(familyName);
            float size = br.ReadSingle();
            int style = br.ReadInt32();
            Font = new Font(ff, size, (FontStyle)style);

            LabelLength = br.ReadSingle();
            Color = Color.FromArgb(br.ReadInt32());
            FlipHorizontal = br.ReadBoolean();
            FlipVertical = br.ReadBoolean();
            BackgroundColor = Color.FromArgb(br.ReadInt32());
            BackgroundTransparent = br.ReadBoolean();
            VerticalAlignment = (TextVerticalAlignment)br.ReadInt32();
            HorizontalAlignment = (TextHorizontalAlignment)br.ReadInt32();

            return true;
        }
        [CategoryAttribute("Text"), DisplayNameAttribute("Font")]
        public Font Font { get; set; } = new Font(SystemFonts.DefaultFont.Name, 32, FontStyle.Regular);

        [CategoryAttribute("Text"), DisplayNameAttribute("Size")]
        public float LabelLength { get; set; } = 1.0f;

        [CategoryAttribute("Text"), DisplayNameAttribute("Flip Horizontal")]
        public bool FlipHorizontal { get; set; } = false;
        [CategoryAttribute("Text"), DisplayNameAttribute("Flip Vertical")]
        public bool FlipVertical { get; set; } = false;

        [CategoryAttribute("Color"), DisplayNameAttribute("Text Color")]
        public Color Color { get; set; } = Color.Black;

        [CategoryAttribute("Color"), DisplayNameAttribute("Background Color")]
        public Color BackgroundColor { get; set; } = Color.White;

        [CategoryAttribute("Color"), DisplayNameAttribute("Backgound Transparency")]
        public bool BackgroundTransparent { get; set; } = true;

        [CategoryAttribute("Alignment"), DisplayNameAttribute("Vertical")]
        public TextVerticalAlignment VerticalAlignment { get; set; } = TextVerticalAlignment.Center;

        [CategoryAttribute("Alignment"), DisplayNameAttribute("Horizontal")]
        public TextHorizontalAlignment HorizontalAlignment { get; set; } = TextHorizontalAlignment.Center;

        [CategoryAttribute("Translation"), DisplayNameAttribute("Offset")]
        public string TranslateString
        {
            get { return Offset.x + "," + Offset.y + "," + Offset.z; }
            set
            {
                char[] splites = new char[] { ',', ' ', '\t' };
                string s = value;
                string[] ss = s.Split(splites, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length >= 3)
                {
                    float offx = 0, offy = 0, offz = 0;
                    if (float.TryParse(ss[0], out offx)) Offset.x = offx;
                    if (float.TryParse(ss[1], out offy)) Offset.y = offy;
                    if (float.TryParse(ss[2], out offz)) Offset.z = offz;
                }
                ss = null;
            }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Rotation")]
        public string RotateString
        {
            get { return Rotate.x + "," + Rotate.y + "," + Rotate.z; }
            set
            {
                char[] splites = new char[] { ',', ' ', '\t' };
                string s = value;
                string[] ss = s.Split(splites, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length >= 3)
                {
                    float x = 0, y = 0, z = 0;
                    if (float.TryParse(ss[0], out x)) Rotate.x = x;
                    if (float.TryParse(ss[1], out y)) Rotate.y = y;
                    if (float.TryParse(ss[2], out z)) Rotate.z = z;
                }
                ss = null;
            }
        }

        [CategoryAttribute("Display"), DisplayNameAttribute("Scale")]
        public string ScaleString
        {
            get { return Scale.x + "," + Scale.y + "," + Scale.z; }
            set
            {
                char[] splites = new char[] { ',', ' ', '\t' };
                string s = value;
                string[] ss = s.Split(splites, StringSplitOptions.RemoveEmptyEntries);

                if (ss.Length > 2)
                {
                    float x = 0, y = 0, z = 0;
                    if (float.TryParse(ss[0], out x)) Scale.x = x;
                    if (float.TryParse(ss[1], out y)) Scale.y = y;
                    if (float.TryParse(ss[2], out z)) Scale.z = z;
                }
                ss = null;
            }
        }
    }
}
