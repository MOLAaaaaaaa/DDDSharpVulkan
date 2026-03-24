using System;
using System.IO;
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

using PatternFilling;

namespace DataCollection
{
    class PolygonFillPattern
    {
    }
    public enum FillMethodEnum
    {
        Solid = 0,  //实心
        Hatch = 1,  //阴影
        GeoPattern = 2,//地质图案
        Texture = 3, //自定义贴图
        LinearGradient = 4,//渐变色
        None = -1,
    }

    public class FillPatternBase
    {
        public FillMethodEnum FillMethod = FillMethodEnum.None;
        [CategoryAttribute("透明"), DisplayNameAttribute("透明度(0-1)")]
        public virtual float fillTransparent { get; set; } = 0;
        public virtual bool IsValid() { return true; }
        public virtual bool Save(BinaryWriter br)
        {
            br.Write((int)FillMethod);
            br.Write((int)fillTransparent);
            return true;
        }
        public virtual bool Load(BinaryReader br)
        {
            FillMethod = (FillMethodEnum)br.ReadInt32();
            fillTransparent = br.ReadSingle();
            return true;
        }
    }
    public class SolidFillPattern : FillPatternBase
    {
        [CategoryAttribute("实心填充"), DisplayNameAttribute("前颜色")]
        public virtual Color fillForeColor { get; set; } = Color.BlanchedAlmond;
        [CategoryAttribute("实心填充"), DisplayNameAttribute("背景色")]
        public virtual Color fillBackColor { get; set; } = Color.White;
        public SolidFillPattern(Color forcolor)
        {
            FillMethod = FillMethodEnum.Solid;
            fillForeColor = forcolor;            
        }
        public SolidFillPattern()
        {
            FillMethod = FillMethodEnum.Solid;
        }
        public override bool IsValid() { return true; }
        public virtual SolidFillPattern Copy()
        {
            SolidFillPattern A = new SolidFillPattern();
            A.FillMethod = FillMethod;
            A.fillTransparent = fillTransparent;
            A.fillForeColor = fillForeColor;
            A.fillBackColor = fillBackColor;
            return A;
        }
        public override bool Save(BinaryWriter br)
        {
            br.Write((int)FillMethod);
            br.Write(fillTransparent);
            br.Write(fillForeColor.ToArgb());
            br.Write(fillBackColor.ToArgb());
            return true;
        }
        public override bool Load(BinaryReader br)
        {
            FillMethod = (FillMethodEnum)br.ReadInt32();
            fillTransparent = br.ReadSingle();
            fillForeColor = Color.FromArgb(br.ReadInt32());
            fillBackColor = Color.FromArgb(br.ReadInt32());
            return true;
        }
    }
    public class HatchFillPattern : SolidFillPattern
    {
        [CategoryAttribute("阴影填充"), DisplayNameAttribute("前颜色")]
        public override Color fillForeColor { get; set; } = Color.BlanchedAlmond;
        [CategoryAttribute("阴影填充"), DisplayNameAttribute("背景色")]
        public override Color fillBackColor { get; set; } = Color.White;
        [CategoryAttribute("阴影填充"), DisplayNameAttribute("阴影图案")]
        public virtual HatchStyle hatchStyle { get; set; } = HatchStyle.Cross;
        public HatchFillPattern()
        {
            FillMethod = FillMethodEnum.Hatch;
        }
        public override bool IsValid() { return true; }
        public new HatchFillPattern Copy()
        {
            HatchFillPattern A = new HatchFillPattern();
            A.FillMethod = FillMethod;
            A.fillTransparent = fillTransparent;
            A.fillForeColor = fillForeColor;
            A.fillBackColor = fillBackColor;
            A.hatchStyle = hatchStyle;
            return A;
        }
        public override bool Save(BinaryWriter br)
        {
            br.Write((int)FillMethod);
            br.Write(fillTransparent);
            br.Write(fillForeColor.ToArgb());
            br.Write(fillBackColor.ToArgb());
            br.Write((int)hatchStyle);
            return true;
        }
        public override bool Load(BinaryReader br)
        {
            FillMethod = (FillMethodEnum)br.ReadInt32();
            fillTransparent = br.ReadSingle();
            fillForeColor = Color.FromArgb(br.ReadInt32());
            fillBackColor = Color.FromArgb(br.ReadInt32());
            hatchStyle = (HatchStyle)br.ReadInt32();
            return true;
        }
    }
    public class GeoFillPattern : FillPatternBase
    {
        public string ID = "";      //编号-序号
        public string Name = "";    //地质名称
        public GeoCodePattern GeoCode = new GeoCodePattern();//地质编码图片
        public PatPatternFile Pattern = new PatPatternFile();//PAT FILE填充图案
        public GeoFillPattern()
        {
            FillMethod = FillMethodEnum.GeoPattern;
        }
        public override bool IsValid() { return true; }
        public GeoFillPattern Copy()
        {
            GeoFillPattern A = new GeoFillPattern();
            A.FillMethod = FillMethod;
            A.fillTransparent = fillTransparent;
            A.ID = ID;
            A.Name = Name;
            A.GeoCode = GeoCode.Copy();
            A.Pattern = Pattern;
            return A;
        }
        public override bool Save(BinaryWriter br)
        {
            br.Write((int)FillMethod);
            return true;
        }
        public override bool Load(BinaryReader br)
        {
            FillMethod = (FillMethodEnum)br.ReadInt32();
            return true;
        }
    }
    public class TextureFillPattern : FillPatternBase
    {
        [CategoryAttribute("纹理填充"), DisplayNameAttribute("图片")]
        public virtual Bitmap bitmap { get; set; } = null;
        public string FileName = "";
        public override bool IsValid() { return bitmap != null; }
        public TextureFillPattern()
        {
            FillMethod = FillMethodEnum.Texture;
        }
        public TextureFillPattern Copy()
        {
            TextureFillPattern A = new TextureFillPattern();
            A.FillMethod = FillMethod;
            A.fillTransparent = fillTransparent;
            A.bitmap = new Bitmap(bitmap);
            A.FileName = FileName;
            return A;
        }
        public override bool Save(BinaryWriter br)
        {
            br.Write((int)FillMethod);
            return true;
        }
        public override bool Load(BinaryReader br)
        {
            FillMethod = (FillMethodEnum)br.ReadInt32();
            return true;
        }
    }
    public class LinearGradientFillPattern : FillPatternBase
    {
        [CategoryAttribute("渐变色填充"), DisplayNameAttribute("开始颜色")]
        public virtual Color StartColor { get; set; } = Color.Black;
        [CategoryAttribute("渐变色填充"), DisplayNameAttribute("结束颜色")]
        public virtual Color EndColor { get; set; } = Color.White;
        [CategoryAttribute("渐变色填充"), DisplayNameAttribute("角度")]
        public virtual float Angle { get; set; } = 0;
        [CategoryAttribute("渐变色填充"), DisplayNameAttribute("按角度渐变")]
        public virtual bool FillAsAngle { get; set; } = false;
        [CategoryAttribute("渐变色填充"), DisplayNameAttribute("AngleScaleable")]
        public virtual bool IsAngleScaleable { get; set; } = false;


        [CategoryAttribute("渐变色填充"), DisplayNameAttribute("渐变模式")]
        public virtual LinearGradientMode Mode { get; set; } = LinearGradientMode.Horizontal;
        public LinearGradientFillPattern()
        {
            FillMethod = FillMethodEnum.LinearGradient;
        }
        public override bool IsValid() { return true; }
        public LinearGradientFillPattern Copy()
        {
            LinearGradientFillPattern A = new LinearGradientFillPattern();
            A.FillMethod = FillMethod;
            A.fillTransparent = fillTransparent;
            A.StartColor = StartColor;
            A.EndColor = EndColor;
            A.Angle = Angle;
            A.FillAsAngle = FillAsAngle;
            A.IsAngleScaleable = IsAngleScaleable;
            A.Mode = Mode;
            return A;
        }
        public override bool Save(BinaryWriter br)
        {
            br.Write((int)FillMethod);
            br.Write(fillTransparent);
            br.Write(StartColor.ToArgb());
            br.Write(EndColor.ToArgb());
            br.Write(Angle);
            br.Write(FillAsAngle);
            br.Write(IsAngleScaleable);
            br.Write((int)Mode);
            return true;
        }
        public override bool Load(BinaryReader br)
        {
            FillMethod = (FillMethodEnum)br.ReadInt32();
            fillTransparent = br.ReadSingle();
            StartColor = Color.FromArgb(br.ReadInt32());
            EndColor = Color.FromArgb(br.ReadInt32());
            Angle = br.ReadSingle();
            FillAsAngle = br.ReadBoolean();
            IsAngleScaleable = br.ReadBoolean();
            Mode = (LinearGradientMode)br.ReadInt32();
            return true;
        }
    }
    public class FillPatternClass
    {
        public Color fillColor = Color.White;
        public FillMethodEnum FillMethod = FillMethodEnum.None;
        public FillPatternBase FillPattern = null;
        public override string ToString()
        {
            if (FillMethod == FillMethodEnum.Solid)
                return "实心填充";
            else if (FillMethod == FillMethodEnum.GeoPattern)
            {
                return "地质图案：" + ((GeoFillPattern)FillPattern).Name;
            }
            else if (FillMethod == FillMethodEnum.Hatch)
            {
                return "阴影填充：" + ((HatchFillPattern)FillPattern).hatchStyle;
            }
            else if (FillMethod == FillMethodEnum.Texture)
            {
                string filename = Path.GetFileName(((TextureFillPattern)FillPattern).FileName);
                return "纹理填充：" + filename;
            }
            else if (FillMethod == FillMethodEnum.LinearGradient)
            {
                return "渐变色填充";
            }
            return "无";
        }
        public FillPatternClass(FillMethodEnum _fillMethod)
        {
            FillMethod = _fillMethod;
            if (FillMethod == FillMethodEnum.Solid)
                FillPattern = new SolidFillPattern(fillColor);
            else if (FillMethod == FillMethodEnum.Hatch)
                FillPattern = new HatchFillPattern();
            else if (FillMethod == FillMethodEnum.GeoPattern)
                FillPattern = new GeoFillPattern();
            else if (FillMethod == FillMethodEnum.Texture)
                FillPattern = new TextureFillPattern();
            else if (FillMethod == FillMethodEnum.LinearGradient)
                FillPattern = new LinearGradientFillPattern();
        }
       
        public FillPatternClass Copy()
        {
            FillPatternClass A = new FillPatternClass(FillMethod);
            A.fillColor = fillColor;
            A.FillMethod = FillMethod;
            if (FillMethod == FillMethodEnum.Solid)
                A.FillPattern = ((SolidFillPattern)FillPattern).Copy();
            else if (FillMethod == FillMethodEnum.Hatch)
                FillPattern = A.FillPattern = ((HatchFillPattern)FillPattern).Copy();
            else if (FillMethod == FillMethodEnum.GeoPattern)
                FillPattern = A.FillPattern = ((GeoFillPattern)FillPattern).Copy();
            else if (FillMethod == FillMethodEnum.Texture)
                FillPattern = A.FillPattern = ((TextureFillPattern)FillPattern).Copy();
            else if (FillMethod == FillMethodEnum.LinearGradient)
                FillPattern = A.FillPattern = ((LinearGradientFillPattern)FillPattern).Copy();
            return A;
        }
        public virtual bool Save(BinaryWriter br)
        {
            br.Write((int)FillMethod);
            FillPattern.Save(br);
            return true;
        }
        public virtual bool Load(BinaryReader br)
        {
            FillMethod = (FillMethodEnum)br.ReadInt32();
            if( C3DData.DataVersion>=1.27f )
            {
                FillPattern.Load(br);
            }
           
            return true;
        }
    }
    
    public class PatternEditor : UITypeEditor
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
                FillPatternClass pattern = value as FillPatternClass;
                var dlg = new PatternForm();
                dlg.SetValue(pattern);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return dlg.fillPattern;
                }
                else return value;
            }
            return base.EditValue(context, provider, value);
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

    /// <summary>
    /// 图案类转换TypeConverter
    /// </summary>
    public class PatternConverter : TypeConverter
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
            try
            {
                FillPatternClass pattern = value as FillPatternClass;
                if (pattern == null) return "无";
                else return pattern.ToString();
            }
            catch (Exception ex)
            {
                return "无";
            }

            //if (value is FillPatternClass)
            //{
            //    System.Windows.Forms.PropertyGridInternal.IRootGridEntry cc = context as System.Windows.Forms.PropertyGridInternal.IRootGridEntry;
            //    FillPatternClass pattern = value as FillPatternClass;
            //    //return pattern.ToString();
            //}
            //else
            //{
            //    return "";
            //    //return base.ConvertTo(context, culture, value, destinationType);
            //}
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
}
