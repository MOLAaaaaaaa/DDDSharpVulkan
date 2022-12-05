using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using System.Drawing;
using System.Drawing.Design;
using GlmNet;
namespace DataCollection
{
    public enum SymbolEnum
    {
        Box = 0,
        Ball = 1,    //球体
        Arrow = 2,  //箭头
        Cone = 3,   //圆锥
        Cross = 4,  //十字架
        Cylinder = 5,  //圆柱体
        None = 1000,
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Symbol3D: C3DObjectBase
    {
        //BOX
        [CategoryAttribute("Cube"), DisplayNameAttribute("Size X"),Browsable(false)]
        public virtual double XSize { get; set; } = 1.0;
        [CategoryAttribute("Cube"), DisplayNameAttribute("Size Y"), Browsable(false)]
        public virtual double ySize { get; set; } = 1.0;
        [CategoryAttribute("Cube"), DisplayNameAttribute("Size Z"), Browsable(false)]
        public virtual double zSize { get; set; } = 1.0;

        //Cylinder
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Radius"), Browsable(false)]
        public virtual double Rad { get; set; } = 1.0; //半径
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Height"), Browsable(false)]
        public virtual double Height { get; set; } = 5.0;//高度
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Vertical Slices"), Browsable(false)]
        public virtual int vertSlices { get; set; } = 20;//横向剖分网格数
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Horizontal Slices"), Browsable(false)]
        public virtual int horSlices { get; set; } = 20;//垂向剖分网格数
       
        //CONE
        //[CategoryAttribute("Cone"), DisplayNameAttribute("Radius"), Browsable(false)]
        //public virtual double Rad { get; set; } = 1.0; //半径
        //[CategoryAttribute("Cone"), DisplayNameAttribute("Height"), Browsable(false)]
        //public virtual double Height { get; set; } = 5.0;//高度
        //[CategoryAttribute("Cone"), DisplayNameAttribute("Vertical Slices"), Browsable(false)]
        //public virtual int vertSlices { get; set; } = 20;//横向剖分网格数
        //[CategoryAttribute("Cone"), DisplayNameAttribute("Horizontal Slices"), Browsable(false)]
        //public virtual int horSlices { get; set; } = 20;//垂向剖分网格数

        //[CategoryAttribute("Cone"), DisplayNameAttribute("Show Top"), Browsable(false)]
        //public virtual bool TopFace { get; set; } = true;//是否显示顶面
     
        public virtual List<TriangleObj> Faces { get; set; } = new List<TriangleObj>();
        public SymbolEnum symbolType = SymbolEnum.Box; 
        public virtual bool Create() { return false; }       
      
        public Symbol3D()
        {
            type = ShapeEnum.Shape;
        }
        public override void Clear() 
        {
            foreach (TriangleObj obj in Faces) obj.Clear();
        }
        public virtual TriangleObj toTriangleObject()
        {
            TriangleObj obj = new TriangleObj(Name);
            foreach( TriangleObj tri in Faces )
            {
                int id0 = obj.points.Count;
                for( int i=0; i < tri.points.Count; i++ )
                {
                    obj.points.Add( tri.points[i] );
                }
                for (int i = 0; i < tri.triangles.Count; i++)
                {
                    obj.AddTriangleIndex(tri.triangles[i].x + id0, tri.triangles[i].y + id0, tri.triangles[i].z + id0);
                }
                for (int i = 0; i < tri.texCoords.Count; i++)
                {
                    obj.AddTexture(tri.texCoords[i]);
                }
            }

            obj.Visible = Visible;
            obj.IsUniformColor = true;            
            obj.IsWireFrameMode = IsWireFrameMode;
            obj.textureStruct = textureStruct;
            obj.Blend = Blend;
            obj.Alpha = Alpha;

            obj.offset = offset;
            obj.rotate = rotate;
            obj.scale = scale;

            return obj;
        }

        public virtual List<TriangleObj> toTriangleObjects()
        {
            return Faces;
        }
        public override void UpdateRange()
        {
            TriangleObj obj;
            for( int i = 0; i < Faces.Count; i++ )
            {
                obj = Faces[i];
                if (i == 0) 
                {
                    minx = obj.minx;
                    miny = obj.miny;
                    minz = obj.minz;
                    minv = obj.minv;
                    maxx = obj.maxx;
                    maxy = obj.maxy;
                    maxz = obj.maxz;
                    maxv = obj.maxv;
                }
                else
                {
                    if (minx > obj.minx) minx = obj.minx;
                    if (miny > obj.miny) miny = obj.miny;
                    if (minz > obj.minz) minz = obj.minz;
                    if (minv > obj.minv) minv = obj.minv;
                    if (maxx < obj.maxx) maxx = obj.maxx;
                    if (maxy < obj.maxy) maxy = obj.maxy;
                    if (maxz < obj.maxz) maxz = obj.maxz;
                    if (maxv < obj.maxv) maxv = obj.maxv;
                }
            }
        }

    }


    /// <summary>
    /// TexturedTextStyleConverter
    /// </summary>
    public class PropertySymbolStyleConverter : TypeConverter
    {
        //该方法判断此类型可以转换为哪些类型
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Symbol3D))
            {
                return true;
            }

            //调用基类方法处理其他情况            
            return base.CanConvertTo(context, destinationType);
        }
        //该方法判断哪些类型可以转换为此类型
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(Symbol3D))
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
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Symbol3D sm = value as Symbol3D;
            if (sm.symbolType == SymbolEnum.Box)
            {
                Box3D box = value as Box3D;
                return box;
            }
            else if (sm.symbolType == SymbolEnum.Cone)
            {
                Cone cone = value as Cone;
                return cone;
            }
            else if (sm.symbolType == SymbolEnum.Cylinder)
            {
                CCylinderExt cylinder = value as CCylinderExt;
                return cylinder;
            }
            else if (sm.symbolType == SymbolEnum.Arrow)
            {
                Arrow2D arrow = value as Arrow2D;
                return arrow;
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ( destinationType == typeof(string) )
            {
                Symbol3D sm = value as Symbol3D;
                return sm.symbolType.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
            //if (destinationType == typeof(Symbol3D) && value is Symbol3D)
            //{
            //    Symbol3D s1 = new Symbol3D();
            //    s1 = (Symbol3D)value;

            //    Box3D b1 = (Box3D)value;
            //}
            //Symbol3D symbol = value as Symbol3D;

            //if (symbol.symbolType == SymbolEnum.Box) 
            //{
            //    Box3D box = new Box3D(new Vector32(),0,0,0);
            //    return box;
            //}
            //if (symbol.symbolType == SymbolEnum.Cylinder) return (CCylinderExt)symbol;
            //if (symbol.symbolType == SymbolEnum.Cone) return (CCylinderExt)symbol;
            //if (symbol.symbolType == SymbolEnum.Arrow) return (Arrow2D)symbol;
            //if (symbol.symbolType == SymbolEnum.Cone) return (CCylinderExt)symbol;            

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
