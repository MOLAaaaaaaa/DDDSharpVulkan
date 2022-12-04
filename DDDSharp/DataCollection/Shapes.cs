using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using GlmNet;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using MathNet.Numerics.Distributions;
using IxMilia.Dxf.Entities;
using System.Globalization;
using System.Runtime.InteropServices;
using TextReaderWriter;
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using Poly2Tri;

namespace DataCollection
{
    //intersection type of point and triangle 

    //box in a 3DGrid
    //box in a 3DGrid
    public class GridBox : C3DObjectBase
    {
        public bool[] faces = null;    //6 faces,6
        public vec3[] points = null;   //x,y,z,8*3*4
        public vec4[] colors = null;   //8*4*4
        public GridBox()
        {
            points = null;
            faces = null;
            colors = null;
            Name = "untitled";
            type = ShapeEnum.Box;
        }
        static public int Size
        {
            get { return 7 * 32 + 72 + 144 + 6; }
        }
        public virtual void Destroy()
        {
            points = null;
            faces = null;
            colors = null;
        }
        public void SetColor(vec4 _colors)
        {
            if (colors == null) colors = new vec4[8];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = _colors;
        }
        public void SetColor(vec4[] _colors)
        {
            colors = _colors;
        }
        public GridBox(double x, double y, double z, double xlen, double ylen, double zlen, vec4[] _colors = null)
        {
            points = null;
            faces = null;
            colors = null;
            Name = "untitled";
            type = ShapeEnum.Box;
            Create(x,y,z,xlen,ylen,zlen,_colors);
        }
        public void Create(double x, double y, double z, double xlen, double ylen, double zlen, vec4[] _colors = null)
        {
            //      p3--------p2 
            //      |         |
            //   p7 |     p6  |
            //   |  /p0---|---p1
            //   | /      | / 
            // p4|/-------p5--->east 
            points = new vec3[8];
            points[0] = new vec3((float)(x - xlen / 2), (float)(y - ylen / 2), (float)(z - zlen / 2));
            points[1] = new vec3((float)(x + xlen / 2), (float)(y - ylen / 2), (float)(z - zlen / 2));
            points[2] = new vec3((float)(x + xlen / 2), (float)(y + ylen / 2), (float)(z - zlen / 2));
            points[3] = new vec3((float)(x - xlen / 2), (float)(y + ylen / 2), (float)(z - zlen / 2));
            points[4] = new vec3((float)(x - xlen / 2), (float)(y - ylen / 2), (float)(z + zlen / 2));
            points[5] = new vec3((float)(x + xlen / 2), (float)(y - ylen / 2), (float)(z + zlen / 2));
            points[6] = new vec3((float)(x + xlen / 2), (float)(y + ylen / 2), (float)(z + zlen / 2));
            points[7] = new vec3((float)(x - xlen / 2), (float)(y + ylen / 2), (float)(z + zlen / 2));

            faces = new bool[6];
            for (int i = 0; i < 6; i++) faces[i] = true;

            colors = _colors;

            UpdateRange();
        }
        public TriangleObj toTriangleObj()
        {
            //      |(y)
            //      p3--------p2 
            //      |         |
            //   p7 |     p6  |
            //   |  /p0---|---p1--->(x)
            //   | /      | / 
            // p4|/-------p5--->east 
            //   / (z)
            TriangleObj obj = new TriangleObj();
            obj.Name = Name;
            obj.IsUniformColor = true;

            if (points == null || faces == null) return obj;
            Int16[] up = new Int16[] { 2, 3, 7, 7, 6, 2 };
            Int16[] down = new Int16[] { 0, 1, 5, 5, 4, 0 };
            Int16[] left = new Int16[] { 3, 0, 4, 3, 4, 7 };
            Int16[] right = new Int16[] { 1, 2, 6, 1, 6, 5 };
            Int16[] front = new Int16[] { 4, 5, 6, 4, 6, 7 };
            Int16[] back = new Int16[] { 3, 2, 1, 3, 1, 0 };

            int i;
            for (i = 0; i < points.Length; i++)
            {
                obj.AddPoint(points[i].x, points[i].y, points[i].z);
            }

            obj.color = new vec4(0, 0, 0, 1);

            if (colors != null)
            {
                for (i = 0; i < colors.Length && i < 8; i++)
                    obj.AddPointColor(colors[i]);
            }
            int start = 0;
            List<int> indices = new List<int>();
            if (faces[0])    //up
            {
                for (i = 0; i < up.Length; i++) indices.Add(up[i] + start);
            }
            if (faces[1])    //down
            {
                for (i = 0; i < down.Length; i++) indices.Add(down[i] + start);
            }
            if (faces[2])    //left
            {
                for (i = 0; i < left.Length; i++) indices.Add(left[i] + start);
            }
            if (faces[3])    //right
            {
                for (i = 0; i < right.Length; i++) indices.Add(right[i] + start);
            }
            if (faces[4])    //front
            {
                for (i = 0; i < front.Length; i++) indices.Add(front[i] + start);
            }
            if (faces[5])    //back
            {
                for (i = 0; i < back.Length; i++) indices.Add(back[i] + start);
            }
            for (i = 0; i < indices.Count / 3; i++)
            {
                obj.AddTriangleIndex(indices[3 * i], indices[3 * i + 1], indices[3 * i + 2]);
            }
            obj.AddTexture(1,1);
            obj.AddTexture(0, 1);
            obj.AddTexture(0, 0);
            obj.AddTexture(1, 0);
            obj.AddTexture(0, 1);
            obj.AddTexture(1, 1);
            obj.AddTexture(0, 0);
            obj.AddTexture(1, 0);
            indices.Clear();
            return obj;
        }
        public override void Normalize()
        {
            UpdateRange();
            Vector32 p;
            for (int i = 0; i < points.Length; i++)
            {
                p = TransformedPoint(toPoint(points[i]));
                points[i] = toPoint(p);
            }
            scale = new vec3(1, 1, 1);
            offset = new vec3(0, 0, 0);
            rotate = new vec3(0, 0, 0);
            UpdateRange();
        }
        public override void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0)
                {
                    minx = maxx = points[i].x;
                    miny = maxy = points[i].y;
                    minz = maxz = points[i].z;
                }
                else
                {
                    if (points[i].x < minx) minx = points[i].x;
                    if (points[i].y < miny) miny = points[i].y;
                    if (points[i].z < minz) minz = points[i].z;
                    if (points[i].x > maxx) maxx = points[i].x;
                    if (points[i].y > maxy) maxy = points[i].y;
                    if (points[i].z > maxz) maxz = points[i].z;
                }
            }
        }
        public void EnableFace(DirectionEnum face, bool show)
        {
            int c = 0;
            for (int i = 0; i < 6; i++)
            {
                c = (int)face & (1 << i);
                if (c > 0) faces[i] = show;
            }
        }
    }

    public class ColorListEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            // 编辑属性值时，在右侧显示...更多按钮
            return UITypeEditorEditStyle.Modal;
            //return UITypeEditorEditStyle.DropDown;
        }

        //edit Color Array
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            //List<Color>colors            
            return base.EditValue(context, provider, value);
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override void PaintValue(PaintValueEventArgs e)
        {
            Graphics g = e.Graphics;

            List<Color>colors = e.Value as List<Color>;
            if (colors != null)
            {
                int x = e.Bounds.X, y = e.Bounds.Y;
                int w = e.Bounds.Width, h = e.Bounds.Height;

                int step = 1;
                int cn = (int)( w / (colors.Count / (double)step));
                while (cn < 2)
                {
                    step++;
                    cn = w / (colors.Count / step);
                }

                double nw = (double)w / ((double)colors.Count / (double)step);
                Rectangle rect = new Rectangle(x, y, x + cn, y + h);
                for (int i = 0; i < colors.Count - 1; i += step)
                {
                    using (var b = new SolidBrush(colors[i]))
                        g.FillRectangle(b, rect);
                    x = (int)(x + nw);
                    rect = new Rectangle(x, y, x + cn, y + h);
                }
            }
            base.PaintValue(e);
        }
    }
    
    /// <summary>
    /// 贴图文字，矩形区域
    /// </summary>
    public class TexturedText : TriangleObj
    {       
        public Vector64 Start = new Vector64(0, 0, 0);
        public Bitmap textureImage = null;
        public bool IsTextStyleChanged()
        {
            return _textStyle.DifferentFrom(_oldertextStyle);
        }
        public TexturedTextStyle _oldertextStyle = new TexturedTextStyle();
        public TexturedTextStyle _textStyle = new TexturedTextStyle();
        [CategoryAttribute("Text"), DisplayNameAttribute("Style")]
        [Editor(typeof(TexturedTextStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(TexturedTextStyleConverter))]
        public TexturedTextStyle textStyle
        {
            get { return _textStyle; }
            set
            {
                _oldertextStyle = _textStyle;
                _textStyle = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        [CategoryAttribute("Text"), DisplayNameAttribute("Length"), Browsable(false)]
        public int TextLength { get { return Name.Length; } }

        [CategoryAttribute("Text"), DisplayNameAttribute("Text")]
        public string Text 
        {  
            get { return Name; }
            set { Name = value; RenderMode = RenderingUpdateMode.Redraw; }
        }

        [CategoryAttribute("Text"), DisplayNameAttribute("Location")]
        public string LocationString
        {
            get { return Start.toString(3); }
            set { Start = Vector64.Parse(value,3); RenderMode = RenderingUpdateMode.Redraw; }
        }

        [CategoryAttribute("Texture"), DisplayNameAttribute("Enable"), Browsable(false)]
        public override bool enbaleTexture { get; set; } = false;

        [CategoryAttribute("Texture"), DisplayNameAttribute("TexureFile"), Browsable(false)]
        public override TextureStruct textureStruct { get; set; } = new TextureStruct();

        //override the properties
        [CategoryAttribute("Geometries"), DisplayNameAttribute("X Minimum"), Browsable(false)]
        public override double Minx { get { return minx; } set { minx = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("X Maximum"), Browsable(false)]
        public override double Maxx { get { return maxx; } set { maxx = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("X Length"), Browsable(false)]
        public override double XWidth { get { return Maxx - Minx; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Y Minimum"), Browsable(false)]
        public override double Miny { get { return miny; } set { miny = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Y Maximum"), Browsable(false)]
        public override double Maxy { get { return maxy; } set { maxy = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Y Length"), Browsable(false)]
        public override double YWidth { get { return Maxy - Miny; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Z Minimum"), Browsable(false)]
        public override double Minz { get { return minz; } set { minz = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Z Maximum"), Browsable(false)]
        public override double Maxz { get { return maxz; } set { maxz = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Z Length"), Browsable(false)]
        public override double ZWidth { get { return Maxz - Minz; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("V Minimum"), Browsable(false)]
        public override double Minv { get { return minv; } set { minv = value; } }
        [CategoryAttribute("Geometries"), DisplayNameAttribute("V Maximum"), Browsable(false)]
        public override double Maxv { get { return maxv; } set { maxv = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("V Length"), Browsable(false)]
        public override double VWidth { get { return Maxv - Minv; } }

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Longitude1"), Browsable(false)]
        public override double Longitude1 { get; set; } = 0;

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Longitude2"), Browsable(false)]
        public override double Longitude2 { get; set; } = 0;

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Latitude1"), Browsable(false)]
        public override double Latitude1 { get; set; } = 0;

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Latitude2"), Browsable(false)]
        public override double Latitude2 { get; set; } = 0;

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Elevation1"), Browsable(false)]
        public override double Elevation1 { get; set; } = 0;

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Elevation2"), Browsable(false)]
        public override double Elevation2 { get; set; } = 0;

        public TexturedText(string text)
        {
            type = ShapeEnum.Text;
            Name = text;
        }
        public TexturedText(string text,Vector64 p,float length)
        {
            type = ShapeEnum.Text;
            Name = text;
            Start = p;
            textStyle.LabelLength = length;            
        }
        public TexturedText(string text, Vector64 p, float length, Color textColor,Color backcolor)
        {
            type = ShapeEnum.Text;
            Name = text;
            Start = p;
            textStyle.LabelLength = length;
            textStyle.Color = textColor;
            textStyle.BackgroundColor = backcolor;
        }
        public override float GetOrderedAlpha()
        {
            return 0.8f;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            SaveObjHeader(br);            
            br.Write(Start.X);
            br.Write(Start.Y);
            br.Write(Start.Z);
            br.Write(Start.V);
            textStyle.SaveBinary(br);
            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {
            LoadObjHeader(br);
            Start.X = br.ReadDouble();
            Start.Y = br.ReadDouble();
            Start.Z = br.ReadDouble();
            Start.V = br.ReadDouble();
            textStyle.LoadFrom(br);
            return true;
        }
        public override void Clear()
        {
            base.Clear();
            if (textureImage != null) textureImage.Dispose();
             textureImage = null;
        }
        
        /// <summary>
        /// 获取偏移点
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p0">旋转中心</param>
        /// <returns></returns>
        public Vector32 GetTransformedPoint(Vector32 p,Vector32 p0)
        {
            Vector32 p1 = p - p0;

            if (textStyle.Scale.x != 1.0) p1.X = p1.X * textStyle.Scale.x;
            if (textStyle.Scale.y != 1.0) p1.Y = p1.Y * textStyle.Scale.y;
            if (textStyle.Scale.z != 1.0) p1.Z = p1.Z * textStyle.Scale.z;

            if (textStyle.Rotate.x != 0.0) p1.RotateOnAngle(textStyle.Rotate.x, 0);
            if (textStyle.Rotate.y != 0.0) p1.RotateOnAngle(textStyle.Rotate.y, 1);
            if (textStyle.Rotate.z != 0.0) p1.RotateOnAngle(textStyle.Rotate.z, 2);

            p1 = p1 + p0;

            p1.x += textStyle.Offset.x;
            p1.y += textStyle.Offset.y;
            p1.z += textStyle.Offset.z;

            return p1;
        }
        public bool Create()
        {   // y
            // |
            // |          
            // 0----------- 1--->x top
            // |            |
            // |2-----------3 
            try 
            {
                Clear();
                BitmapString bm = new BitmapString(Name, textStyle.Font, textStyle.Color, textStyle.BackgroundColor);
                textureImage = bm.Draw(textStyle.BackgroundTransparent);
                if ( textStyle.FlipHorizontal )
                    textureImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                if (textStyle.FlipVertical)
                    textureImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                
                double width = textStyle.LabelLength;
                double height = width * (double)textureImage.Height / (double)textureImage.Width;
                Vector64 p0 = new Vector64(0, 0, 0);
                Vector64 p1 = new Vector64(width, 0, 0);
                Vector64 p2 = new Vector64(width, height, 0);
                Vector64 p3 = new Vector64(0, height, 0);

                Vector64 xdirect = new Vector64(1, 0, 0);
                Vector64 ydirect = new Vector64(0, 1, 0);
                if (textStyle.VerticalAlignment == TextVerticalAlignment.Top)
                {
                    p0 = new Vector64(0, 0, 0);
                    p1 = new Vector64(width, 0, 0);                    
                }
                else if (textStyle.VerticalAlignment == TextVerticalAlignment.Bottom)
                {
                    p0 = new Vector64(0, height, 0);
                    p1 = new Vector64(width, height, 0);                    
                }
                else //if (VerticalAlignment == TextVerticalAlignment.Center)
                {
                    p0 = new Vector64(0, height/2, 0);
                    p1 = new Vector64(width, height/2, 0);
                }
                p2 = p0 - height * ydirect;
                p3 = p1 - height * ydirect;
                
                p0 = p0 + Start;
                p1 = p1 + Start;
                p2 = p2 + Start;
                p3 = p3 + Start;
                if(textStyle.HorizontalAlignment == TextHorizontalAlignment.Center )
                {
                    p0 = p0 - width / 2.0 * xdirect;
                    p1 = p1 - width / 2.0 * xdirect;
                    p2 = p2 - width / 2.0 * xdirect;
                    p3 = p3 - width / 2.0 * xdirect;
                }
                else if(textStyle.HorizontalAlignment == TextHorizontalAlignment.Right)
                {
                    p0 = p0 - width * xdirect;
                    p1 = p1 - width * xdirect;
                    p2 = p2 - width * xdirect;
                    p3 = p3 - width * xdirect;
                }
                
                Vector32 v0 = new Vector32((float)p0.x, (float)p0.y, (float)p0.z);
                Vector32 v1 = new Vector32((float)p1.x, (float)p1.y, (float)p1.z);
                Vector32 v2 = new Vector32((float)p2.x, (float)p2.y, (float)p2.z);
                Vector32 v3 = new Vector32((float)p3.x, (float)p3.y, (float)p3.z);

                Vector32 center = (v1 + v2 + v3 + v0) / 4;

                v0 = GetTransformedPoint(v0, center);
                v1 = GetTransformedPoint(v1, center);
                v2 = GetTransformedPoint(v2, center);
                v3 = GetTransformedPoint(v3, center);

                AddPoint(v0);
                AddPoint(v1);
                AddPoint(v2);
                AddPoint(v3);
                
                vec4 color = ConvertColor.Convert(textStyle.BackgroundColor);
                
                if (textStyle.BackgroundTransparent) color.w = 1;
                else color.w = Alpha;

                AddPointColor(color);
                AddPointColor(color);
                AddPointColor(color);
                AddPointColor(color);

                AddTexture(0, 0);
                AddTexture(1, 0);
                AddTexture(0, 1);
                AddTexture(1, 1);
                AddTriangleIndex(0, 2, 1);
                AddTriangleIndex(1, 2, 3);

                //scale = textStyle.Scale;
                //rotate = textStyle.Rotate;
                //offset = textStyle.Offset;

                UpdateRange();

                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
    }

    public class TriangleObj : C3DObjectBase
    {
        public List<Vector32> points = new List<Vector32>();
        public vec3[] normals = null;
        public List<vec4> colors = new List<vec4>();
        public List<Int32XYZ> triangles = new List<Int32XYZ>();
        public List<vec2> texCoords = new List<vec2>();
        public vec4 color = new vec4(0.8f, 0.8f, 0.8f, 1.0f);

        private bool _IsUniformColor = false;        
        [CategoryAttribute("Uniform Color"), DisplayNameAttribute("Uniform Color")]
        public virtual bool IsUniformColor 
        { 
            get { return _IsUniformColor; }
            set { _IsUniformColor = value; RenderMode = RenderingUpdateMode.Redraw; }
        }

        [CategoryAttribute("Uniform Color"), DisplayNameAttribute("Color")]
        public virtual Color uniformColor
        {
            get
            {
                return Color.FromArgb((int)(color.w * 255.0), (int)(color.x * 255.0), (int)(color.y * 255.0), (int)(color.z * 255.0));
            }
            set
            {
                Color c = value;
                color = new vec4((float)(c.R / 255.0), (float)(c.G / 255.0), (float)(c.B / 255.0), (float)(c.A / 255.0));
                if(IsUniformColor)RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        //check if color is 0-255
        [CategoryAttribute("Color"), DisplayNameAttribute("Is Byte Color"),Browsable(false)]
        public bool IsByteColor
        {
            get
            {
                for (int i = 0; i < colors.Count; i++)
                {
                    if (colors[i].x > 1 || colors[i].y > 1 || colors[i].z > 1)
                        return true;
                }
                return false;
            }
        }

        [CategoryAttribute("WireFrame"), DisplayNameAttribute("Visible")]
        public bool WireFrameVisible { get; set; } = false;
        [CategoryAttribute("WireFrame"), DisplayNameAttribute("Color")]
        public Color WireFrameColor { get; set; } = Color.Gray;
        [CategoryAttribute("WireFrame"), DisplayNameAttribute("Alpha")]
        public float WireFrameAlpha { get; set; } = 1.0f;

        public void toFloatColors()
        {
            if (IsByteColor)
            {
                float x, y, z, w;
                for (int i = 0; i < colors.Count; i++)
                {
                    x = colors[i].x / 255;
                    y = colors[i].y / 255;
                    z = colors[i].z / 255;
                    w = colors[i].w / 255;
                    colors[i] = new vec4(x, y, z, w);
                }
            }
        }

        [CategoryAttribute("ColorListEditor"), DisplayNameAttribute("ColorList"),Browsable(false)]
        public List<Color> Colors
        {
            //mapped to List<Color>
            get 
            {
                List<Color> colorlist = new List<Color>();
                foreach(vec4 c in colors)
                {
                   Color c1 = Color.FromArgb((int)(c.w*255), (int)(c.x*255), (int)(c.y * 255), (int)(c.z * 255));
                    colorlist.Add(c1);
                }
                return colorlist;
            }
            set 
            {
                colors.Clear();
                foreach(Color c in value )
                {
                    vec4 c1 = new vec4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
                    colors.Add(c1);
                }
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        static public vec3 GetNormal(Vector32 p1, Vector32 p2, Vector32 p3)
        {
            vec3 pn = new vec3(0, 0, 0);
            double ax = p1.x - p2.x;
            double ay = p1.y - p2.y;
            double az = p1.z - p2.z;

            double bx = p2.x - p3.x;
            double by = p2.y - p3.y;
            double bz = p2.z - p3.z;

            double dx = ay * bz - az * by;
            double dy = az * bx - ax * bz;
            double dz = ax * by - ay * bx;

            ax = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (ax > 0)
            {
                pn.x = (float)(dx / ax);
                pn.y = (float)(dy / ax);
                pn.z = (float)(dz / ax);
            }
            return pn;
        }
        static public vec3 GetNormal(vec3 p1, vec3 p2, vec3 p3)
        {
            vec3 pn = new vec3(0, 0, 0);
            double ax = p1.x - p2.x;
            double ay = p1.y - p2.y;
            double az = p1.z - p2.z;

            double bx = p2.x - p3.x;
            double by = p2.y - p3.y;
            double bz = p2.z - p3.z;

            double dx = ay * bz - az * by;
            double dy = az * bx - ax * bz;
            double dz = ax * by - ay * bx;

            ax = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (ax > 0)
            {
                pn.x = (float)(dx / ax);
                pn.y = (float)(dy / ax);
                pn.z = (float)(dz / ax);
            }
            return pn;
        }
        /// <summary>
        /// 将多个三角形对象合并成一个，并优化顶点序列
        /// </summary>
        /// <param name="objs"></param>
        static public TriangleObj toTriangleObj(List<TriangleObj> objs)
        {
            if (objs.Count == 1) return objs[0];

            TriangleObj obj = new TriangleObj();
            foreach (TriangleObj tri in objs)
            {
                foreach (Vector32 v in tri.points)
                {
                    if (!obj.points.Contains(v))
                        obj.AddPoint(v);
                }
            }
            Vector32 p1, p2, p3;
            int id1, id2, id3;
            foreach (TriangleObj tri in objs)
            {
                for (int i = 0; i < tri.triangles.Count; i++)
                {
                    p1 = tri.points[tri.triangles[i].x];
                    p2 = tri.points[tri.triangles[i].y];
                    p3 = tri.points[tri.triangles[i].z];
                    id1 = obj.points.IndexOf(p1);
                    id2 = obj.points.IndexOf(p2);
                    id3 = obj.points.IndexOf(p3);
                    if (id1 < 0 || id2 < 0 || id3 < 0)
                    {
                        throw (new Exception("point not found."));
                    }
                    obj.AddTriangleIndex(id1, id2, id3);
                }
            }

            return obj;
        }
        public void SetNormal(int id,vec3 normal)
        {
            if ( normals == null || normals.Length <= id ) return;
            normals[id] = normal;
        }
        public void AddToNormal(int id, vec3 normal)
        {
            AddToNormal(id, normal.x, normal.y, normal.z);
        }
        public void AddToNormal(int id, float x, float y, float z)
        {
            if (normals == null || normals.Length <= id) return;
            normals[id].x += x;
            normals[id].y += y;
            normals[id].z += z;
        }
        public double minSquare;   //minimum triangle 
        public double maxSquare;    //minimum angle
        public double minAngle;    //minimum angle        
        public double minEdge;     //minimum edge length
        public TriangleObj(string name = "Untitled")
        {
            Name = name;
            type = ShapeEnum.Triangles;
            enbaleTexture = false;
        }
        /// <summary>
        /// 复制对象
        /// </summary>
        /// <returns></returns>
        public TriangleObj Copy()
        {
            TriangleObj obj = new TriangleObj();
            obj.CopyHeaderFrom(this);
            foreach (Vector32 p in points)
            {
                obj.points.Add(p);
            }            
            foreach (vec4 c in colors)
            {
                obj.colors.Add(c);
            }
            foreach (Int32XYZ id in triangles)
            {
                obj.triangles.Add(id);
            }
            foreach (vec2 tex in texCoords)
            {
                obj.texCoords.Add(tex);
            }
            
            obj.normals = normals;
            
            obj.color = color;
            obj._IsUniformColor = _IsUniformColor;
            
            return obj;
        }
        public override void Clear()
        {
            points.Clear();
            triangles.Clear();
            texCoords.Clear();
            colors.Clear();
            normals = null;
        }

        public virtual void DoTransform()
        {
            Vector32 p;
            for(int i=0;i<points.Count;i++)
            {
                p = points[i];
                p = TransformedPoint(p);
                points[i] = p;
            }
        }
       
        public virtual void Destroy()
        {
            Clear();
        }

        /// <summary>
        /// 三角形顶点冗余检查，去掉冗余顶点
        /// </summary>
        /// <returns>返回冗余顶点数目</returns>
        public virtual int TriangleVerticsVerify()
        {
            int np = points.Count;
            int nt = triangles.Count;
            if (np < 2) return 0;
            int[] indices = new int[np];

            for (int i = 0; i < np; i++)
                indices[i] = -1;

            int ichecked = 0;

            //检查冗余顶点
            int id1, id2, id3;
            for (int i = nt - 1; i >= 0; i--)
            {
                id1 = triangles[i].x;
                id2 = triangles[i].y;
                id3 = triangles[i].z;
                indices[id1] = id1;
                indices[id2] = id2;
                indices[id3] = id3;
            }

            for (int i = np - 1; i >= 0; i--)
            {
                if (indices[i] < 0)
                {
                    points.RemoveAt(i);
                    indices[i] = -1;

                    for (int j = i + 1; j < np; j++)
                        indices[j]--;

                    ichecked++;
                }
            }

            //重建三角形
            Int32XYZ p;
            for (int i = 0; i < nt; i++)
            {
                p = triangles[i];
                p.x = indices[p.x];
                p.y = indices[p.y];
                p.z = indices[p.z];
                triangles[i] = p;
            }

            return ichecked;
        }

        /// <summary>
        /// 三角形边冗余检查，去掉退化三角形
        /// 三条边比例小于0.001认为异常三角形
        /// </summary>
        /// <returns>返回退化三角形数目</returns>
        public virtual int TriangleEdgesVerify(double minedge = 0.001)
        {
            int nt = triangles.Count;
            int ichecked = 0;
            //检查冗余三角形
            int id1, id2, id3;
            Vector32 p1, p2, p3;
            double maxwidth;
            double l12, l13, l23;
            for (int i = nt - 1; i >= 0; i--)
            {
                id1 = triangles[i].x;
                id2 = triangles[i].y;
                id3 = triangles[i].z;
                if (id1 == id2 || id1 == id3 || id2 == id3)
                {
                    triangles.RemoveAt(i);
                    ichecked++;
                }

                p1 = points[id1];
                p2 = points[id2];
                p3 = points[id3];

                l12 = p1.Distance(p2);
                l13 = p1.Distance(p3);
                l23 = p2.Distance(p3);

                //异常三角形
                maxwidth = Math.Max(l12, l13);
                maxwidth = Math.Max(maxwidth, l23);
                if (l12 / maxwidth <= minedge ||
                    l13 / maxwidth <= minedge ||
                    l23 / maxwidth <= minedge)
                {
                    triangles.RemoveAt(i);
                    ichecked++;
                }
            }

            return ichecked;
        }
        public bool SaveAsOBJ(string path)
        {
            try
            {
                StreamWriter wr = new StreamWriter(new FileStream(path, FileMode.Create));
                string line = "# Alias|Wavefront 3D Object format ";
                wr.WriteLine(line);
                line = "# Created by 3D Surfer  " + DateTime.Now.ToString();
                wr.WriteLine(line);

                string objname = Name.Replace(' ', '_');
                objname = objname.Replace('\t', '_');

                line = "g " + objname; wr.WriteLine(line);

                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    line = "v " + p.x + " " + p.y + " " + p.z;
                    wr.WriteLine(line);
                }

                int id1, id2, id3;
                Vector32 p1, p2, p3, pn;
                for (int i = 0; i < triangles.Count; i++)
                {
                    id1 = triangles[i].x;
                    id2 = triangles[i].y;
                    id3 = triangles[i].z;
                    p1 = points[id1];
                    p2 = points[id2];
                    p3 = points[id3];
                    pn = Vector32.GetNormal(p1, p2, p3);
                    line = "vn " + pn.x + " " + pn.y + " " + pn.z;
                    wr.WriteLine(line);
                }

                for (int i = 0; i < triangles.Count; i++)
                {
                    id1 = triangles[i].x + 1;
                    id2 = triangles[i].y + 1;
                    id3 = triangles[i].z + 1;
                    //line = "f " + id1 + "/" + id1 + " ";
                    //line += id2 + "/" + id2 + " ";
                    //line += id3 + "/" + id3;
                    line = "f " + id1 + " " + id2 + " " + id3;
                    wr.WriteLine(line);
                }
                wr.Close();

                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool SaveAsSTL(string path)
        {
            BinaryWriter br;
            try
            {
                br = new BinaryWriter(new FileStream(path, FileMode.Create));

                string headerInfo = "STL Object Created By 3D Surfer." + DateTime.Now.ToString();

                int len = headerInfo.Length;
                for (int i = 0; i < 80 - len; i++)
                {
                    headerInfo += "c";
                }
                byte[] header = Encoding.ASCII.GetBytes(headerInfo);

                br.Write(header, 0, 80);
                br.Write(triangles.Count);

                Int16 property = 0;
                byte r = (byte)(255 * color.x);
                byte g = (byte)(255 * color.y);
                byte b = (byte)(255 * color.z);
                System.Drawing.Color cc = System.Drawing.Color.FromArgb(1, r, g, b);
                property = (Int16)cc.ToArgb();

                int id1, id2, id3;
                Vector32 p1, p2, p3, pn;
                for (int i = 0; i < triangles.Count; i++)
                {
                    id1 = triangles[i].x;
                    id2 = triangles[i].y;
                    id3 = triangles[i].z;
                    p1 = points[id1];
                    p2 = points[id2];
                    p3 = points[id3];
                    pn = Vector32.GetNormal(p1, p2, p3);

                    br.Write(pn.x);
                    br.Write(pn.y);
                    br.Write(pn.z);

                    br.Write(p1.x);
                    br.Write(p1.y);
                    br.Write(p1.z);

                    br.Write(p2.x);
                    br.Write(p2.y);
                    br.Write(p2.z);

                    br.Write(p3.x);
                    br.Write(p3.y);
                    br.Write(p3.z);

                    br.Write(property);
                }

                br.Close();

                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }

        }
        //Save as PLY 
        public bool SaveAsPLY(string path)
        {
            if (points.Count < 3 || triangles.Count < 1)
            {
                errMessage = "no valid vertics and faces.";
                return false;
            }
            try
            {
                StringBuilder builder = new StringBuilder();
                File.WriteAllText(path, builder.ToString());
                builder.AppendLine("ply");
                builder.AppendLine("format ascii 1.0");

                builder.AppendLine("comment created by 3D Surfer v3.0.");
                builder.AppendLine("comment name " + Name);

                if ( colors.Count < 1 )builder.AppendLine("comment color " + color.x + " "+ color.y + " " + color.z);

                builder.AppendLine("comment  " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());


                builder.AppendLine("element vertex " + points.Count);

                builder.AppendLine("property float32 x");
                builder.AppendLine("property float32 y");
                builder.AppendLine("property float32 z");

                //if colors.Count == points.Count
                //if colors.Count < points.Count
                if ( colors.Count > 0 )
                {
                    builder.AppendLine("property float32 Red");
                    builder.AppendLine("property float32 Green");
                    builder.AppendLine("property float32 Blue");
                }
                if (texCoords.Count > 0)
                {
                    builder.AppendLine("property float32 u");
                    builder.AppendLine("property float32 v");
                }
                builder.AppendLine("element face " + triangles.Count);
                builder.AppendLine("property list uint8 int32 vertex_indices");
                builder.AppendLine("end_header");
                string line;
                float r = 0, g = 0, b = 0;
                int ic = 0;
                Vector32 p;
                bool byteColor = IsByteColor;
                for (int i = 0; i < points.Count; i++)
                {
                    //xyz
                    p = TransformedPoint(points[i]);
                    //line = points[i].x + " " + points[i].y + " " + points[i].z;
                    line = p.x + " " + p.y + " " + p.z;
                    //r,g,b
                    if (colors.Count > 0)
                    {
                        if (colors.Count >= points.Count)
                        {
                            r = colors[i].x;
                            g = colors[i].y;
                            b = colors[i].z;
                        }
                        else
                        {
                            r = colors[ic].x;
                            g = colors[ic].y;
                            b = colors[ic].z;
                            ic++;
                            if (ic >= colors.Count) ic = 0;
                        }

                        if( byteColor )line += " " + r/255f + " " + g/255f + " " + b/255f;
                        else line += " " + r + " " + g + " " + b;
                    }

                    //u,v
                    if (texCoords.Count > 0)
                        line += " " + texCoords[i].x + " " + texCoords[i].y;

                    builder.AppendLine(line);
                }
                for (int i = 0; i < triangles.Count; i++)
                {
                    builder.AppendLine("3 " + triangles[i].x + " " + triangles[i].y + " " + triangles[i].z);
                }
                File.WriteAllText(path, builder.ToString());
                builder.Clear();
                builder = null;

            }
            catch (Exception e)
            {
                errMessage = "writing to file faild!\n" + e.Message;
                return false;
            }

            return true;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if ( !SaveObjHeader(br) ) return false;
            try 
            {
                br.Write(points.Count);
                for (int i = 0; i < points.Count; i++)
                {
                    br.Write(points[i].x);
                    br.Write(points[i].y);
                    br.Write(points[i].z);
                    br.Write(points[i].v);
                }
                br.Write(colors.Count);
                for (int i = 0; i < colors.Count; i++)
                {
                    br.Write(colors[i].x);
                    br.Write(colors[i].y);
                    br.Write(colors[i].z);
                    br.Write(colors[i].w);
                }
                br.Write(triangles.Count);
                for (int i = 0; i < triangles.Count; i++)
                {
                    br.Write(triangles[i].x);
                    br.Write(triangles[i].y);
                    br.Write(triangles[i].z);
                }
                br.Write(texCoords.Count);
                for (int i = 0; i < texCoords.Count; i++)
                {
                    br.Write(texCoords[i].x);
                    br.Write(texCoords[i].y);
                }
                br.Write(color.x);
                br.Write(color.y);
                br.Write(color.z);
                br.Write(color.w);

                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }            
        }
        public override bool LoadFrom(BinaryReader br)
        {
            if ( !LoadObjHeader(br) ) return false;
            
            points.Clear();
            colors.Clear();
            triangles.Clear();
            texCoords.Clear();

            float x, y, z, v, w;
            int ix, iy, iz;

            try 
            {
                int n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    v = br.ReadSingle();
                    points.Add(new Vector32(x, y, z, v));
                }
                n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    w = br.ReadSingle();
                    colors.Add(new vec4(x, y, z, w));
                }
                n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    ix = br.ReadInt32();
                    iy = br.ReadInt32();
                    iz = br.ReadInt32();
                    triangles.Add(new Int32XYZ(ix, iy, iz));
                }
                n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    texCoords.Add(new vec2(x, y));
                }

                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                w = br.ReadSingle();
                color = new vec4(x, y, z, w);
                UpdateRange();
                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }            
        }
        public vec4 GetPointColor(int index)
        {
            if (index >= 0 && index < colors.Count)
            {
                return colors[index];
            }
            return color;
        }
        public void AddPoint(Vector32 p)
        {
            points.Add(p);
        }
        public void AddTexture(vec2 p)
        {
            texCoords.Add(p);
        }
        public void AddTexture(float x, float y)
        {
            texCoords.Add(new vec2(x,y));
        }
        public void AddPoint(double x, double y, double z, double v = 0)
        {
            points.Add(new Vector32((float)x, (float)y, (float)z, (float)v));
        }
        public void AddPointColor(vec4 cc)
        {
            colors.Add(cc);
        }
        public void AddPointColor(Color c)
        {
            AddPointColor(c.R/255f,c.G/255f,c.B/255f,c.A/255f);
        }
        public void AddPointColor(double r, double g, double b, double a = 1)
        {
            colors.Add(new vec4((float)r, (float)g, (float)b, (float)a));
        }
        public void AddTriangleIndex(int i1, int i2, int i3)
        {
            triangles.Add(new Int32XYZ(i1, i2, i3));
        }
        public CTriangle3f GetTriangle(int index)
        {
            CTriangle3f tri = new CTriangle3f();
            if (index >= 0 && index < triangles.Count)
            {
                tri.p1 = points[triangles[index].x].toVector64();
                tri.p2 = points[triangles[index].y].toVector64();
                tri.p3 = points[triangles[index].z].toVector64();
            }
            return tri;
        }
        public CTriangle3f GetNearestTriangle(Vector32 p)
        {
            double len, lx, ly, lz, dist = 1.0E10;
            int i1, i2, i3;
            Vector32 p1, p2, p3;
            int id = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                p1 = points[i1];
                p2 = points[i2];
                p3 = points[i3];

                lx = p1.x - p.x;
                ly = p1.y - p.y;
                lz = p1.z - p.z;
                if (lx < 0) lx = -lx;
                if (ly < 0) ly = -ly;
                if (lz < 0) lz = -lz;
                len = lx + ly + lz;

                lx = p2.x - p.x;
                ly = p2.y - p.y;
                lz = p2.z - p.z;
                if (lx < 0) lx = -lx;
                if (ly < 0) ly = -ly;
                if (lz < 0) lz = -lz;
                len += (lx + ly + lz);

                lx = p3.x - p.x;
                ly = p3.y - p.y;
                lz = p3.z - p.z;
                if (lx < 0) lx = -lx;
                if (ly < 0) ly = -ly;
                if (lz < 0) lz = -lz;
                len += (lx + ly + lz);

                if (len < dist)
                {
                    id = i;
                    dist = len;
                }
            }
            return GetTriangle(id);
        }
        public virtual Vector32 GetMiddlePoint(Vector32 p1, Vector32 p2)
        {
            Vector32 p = (p1 + p2) / 2;
            return p;
        }
        private double GetSquare(Vector32 p1, Vector32 p2, Vector32 p3)
        {
            Vector32 p12 = new Vector32(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector32 p23 = new Vector32(p3.X - p2.X, p3.Y - p2.Y, p3.Z - p2.Z);
            Vector32 p = p12.Cross(p23);
            return p.Length;
        }
        private void DoSmooth()
        {
            Int32XYZ[] indexes = new Int32XYZ[triangles.Count];

            for (int i = 0; i < triangles.Count; i++)
                indexes[i] = triangles[i];

            for (int i = 0; i < indexes.Length; i++)
                DividTriangle(i);

            indexes = null;
        }
        public void SmoothTriangle(double _minSquare = 0.01)
        {
            if (_minSquare >= 1) return;
            /*
            minSquare = maxSquare = 0;
            double square = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                square = GetSquare(points[triangles[i].x], points[triangles[i].y], points[triangles[i].z]);
                if (i == 0) minSquare = maxSquare = square;
                else
                {
                    if (square < minSquare) minSquare = square;
                    if (square > maxSquare) maxSquare = square;
                }
            }

            minSquare = _minSquare * maxSquare;
            */
            minSquare = _minSquare;
            DoSmooth();
        }
        public bool IsPointInRange(double x, double y, double z, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            if (x < x1 || x > x2) return false;
            if (y < y1 || y > y2) return false;
            if (z < z1 || z > z2) return false;
            return true;
        }
        /// <summary>
        /// 对三角形进行裁剪，保留指定范围内的三角形
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="z1"></param>
        /// <param name="z2"></param>
        /// <returns></returns>
        public TriangleObj TrimObject(double x1, double x2, double y1, double y2, double z1, double z2)
        {
            TriangleObj obj = new TriangleObj();
            bool[] trim = new bool[points.Count];
            int[] indices = new int[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                trim[i] = false;
                indices[i] = i;
            }
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (!IsPointInRange(p.x, p.y, p.z, x1, x2, y1, y2, z1, z2))
                    trim[i] = true;
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (trim[i]) continue;
                indices[i] = obj.points.Count;
                obj.AddPoint(points[i]);
            }
            int i1, i2, i3;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                if (trim[i1] || trim[i2] || trim[i3]) continue;
                obj.AddTriangleIndex(indices[i1], indices[i2], indices[i3]);
            }
            obj.UpdateRange();

            trim = null;
            indices = null;

            obj.colors = new List<vec4>();
            obj.texCoords = texCoords;
            obj.color = color;
            obj.textureStruct = textureStruct.Copy();

            return obj;
        }
        public void SmoothTriangle(int dividNum = 2)
        {
            minSquare = maxSquare = 0;
            double square = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                square = GetSquare(points[triangles[i].x], points[triangles[i].y], points[triangles[i].z]);
                if (i == 0) minSquare = maxSquare = square;
                else
                {
                    if (square < minSquare) minSquare = square;
                    if (square > maxSquare) maxSquare = square;
                }
            }

            square = maxSquare / dividNum;
            minSquare = maxSquare / dividNum;

            DoSmooth();
        }
        virtual public void DividTriangle(int index)
        {
            //       p1
            //     /   \
            // i12/     \i13
            //   /       \
            // p2 --i23-- p3
            int i1 = triangles[index].x;
            int i2 = triangles[index].y;
            int i3 = triangles[index].z;
            Vector32 p1 = points[i1];
            Vector32 p2 = points[i2];
            Vector32 p3 = points[i3];

            //triangle meet minimum square requirment
            if (GetSquare(p1, p2, p3) <= minSquare) return;

            Vector32 p12 = GetMiddlePoint(p1, p2);
            Vector32 p23 = GetMiddlePoint(p2, p3);
            Vector32 p13 = GetMiddlePoint(p1, p3);

            int i12 = points.Count;
            int i23 = i12 + 1;
            int i13 = i23 + 1;

            //add new points
            points.Add(p12);
            points.Add(p23);
            points.Add(p13);

            Int32XYZ d0 = new Int32XYZ(i12, i23, i13);
            Int32XYZ d1 = new Int32XYZ(i1, i12, i13);
            Int32XYZ d2 = new Int32XYZ(i12, i2, i23);
            Int32XYZ d3 = new Int32XYZ(i13, i23, i3);

            int t0 = triangles.Count;
            //new triangles
            //replace original one
            triangles[index] = d0;
            //add to end triangles
            triangles.Add(d1);
            triangles.Add(d2);
            triangles.Add(d3);

            DividTriangle(index);
            DividTriangle(t0);
            DividTriangle(t0 + 1);
            DividTriangle(t0 + 2);
        }

        public override void Normalize()
        {
            UpdateRange();

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = TransformedPoint(points[i]);
            }
            scale = new vec3(1, 1, 1);
            offset = new vec3(0, 0, 0);
            rotate = new vec3(0, 0, 0);

            UpdateRange();
        }
        /// <summary>
        /// 将物体缩放到指定范围
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="z2"></param>
        public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            UpdateRange();
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (maxx == minx) p.x = (float)((x1 + x2) / 2.0);
                else p.x = (float)(x1 + (x2 - x1) * (p.x - minx) / (maxx - minx));
                if (maxy == miny) p.y = (float)((y1 + y2) / 2.0);
                else p.y = (float)(y1 + (y2 - y1) * (p.y - miny) / (maxy - miny));
                if (maxz == minz) p.z = (float)((z1 + z2) / 2.0);
                else p.z = (float)(z1 + (z2 - z1) * (p.z - minz) / (maxz - minz));
                points[i] = p;
            }
            minx = x1;
            miny = y1;
            minz = z1;
            maxx = x2;
            maxy = y2;
            maxz = z2;
        }
        public override void CutWith(C3DObjectBase obj, int method, int methodPara)
        {
            //method = 0 Polygon
            //"keep outside"
            //"keep inside"
            //"create intersection"
            //method = 1 mesh
            //keep up(y+) //0 
            //keep down(y-)//1
            //keep left(-x)//2
            //keep right(x+)//3
            //keep front(z+)//4
            //keep back(z-)//5
            //create intersection//6
            if (method == 0)
            {
                if (methodPara == 0)
                    CutWithPolygon(new Polygon3D((TriangleObj)obj), true);
                else if (methodPara == 1)
                    CutWithPolygon(new Polygon3D((TriangleObj)obj), false);
            }
        }
        //p1 - p2 is on axis = 0 x 1 y 2 z
        //return 
        public bool GetAxisIntersection(Vector32 v1, Vector32 v2, int axis, out List<Vector32> sects, out List<int> triIndices)
        {
            sects = new List<Vector32>();
            triIndices = new List<int>();

            //not in the range
            if (!IsInRange(v1.x, v1.y, v1.z) &&
                 !IsInRange(v2.x, v2.y, v2.z)) return false;

            int i1, i2, i3;

            CTriangle3f tri = new CTriangle3f();
            Vector32 p1, p2, p3;
            Vector64 p = new Vector64();
            double r1, r2;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                p1 = points[i1];
                p2 = points[i2];
                p3 = points[i3];
                tri.p1 = p1.toVector64();
                tri.p2 = p2.toVector64();
                tri.p3 = p3.toVector64();
                if (axis == 0) // x axis
                {
                    r1 = r2 = p1.x;
                    if (r1 > p2.x) r1 = p2.x;
                    if (r1 > p3.x) r1 = p3.x;
                    if (r2 < p2.x) r2 = p2.x;
                    if (r2 < p3.x) r2 = p3.x;

                    //outof range
                    if ((v1.x < r1 && v2.x < r1) ||
                         (v1.x > r2 && v2.x > r2)) continue;

                    //axis triangle relation
                    p = v1.toVector64(); p.x = 0;
                    p1.x = 0; p2.x = 0; p3.x = 0;
                    if (!CTriangle2f.IsPointInTriangle(p, p1, p2, p3)) continue;
                    //calculation intersection
                    if (tri.GetIntersectionOnTriangle(v1.toVector64(), v2.toVector64(), out p))
                    {
                        sects.Add(p);
                        triIndices.Add(i);
                    }
                }
                else if (axis == 1) // y axis
                {
                    r1 = r2 = p1.y;
                    if (r1 > p2.y) r1 = p2.y;
                    if (r1 > p3.y) r1 = p3.y;
                    if (r2 < p2.y) r2 = p2.y;
                    if (r2 < p3.y) r2 = p3.y;
                    //outof range
                    if ((v1.y < r1 && v2.y < r1) ||
                         (v1.y > r2 && v2.y > r2)) continue;
                    //axis triangle relation
                    p = v1.toVector64(); p.y = 0;
                    p1.y = 0; p2.y = 0; p3.y = 0;
                    if (!CTriangle2f.IsPointInTriangle(p, p1, p2, p3)) continue;
                    //calculation intersection
                    if (tri.GetIntersectionOnTriangle(v1.toVector64(), v2.toVector64(), out p))
                    {
                        sects.Add(p);
                        triIndices.Add(i);
                    }
                }
                else if (axis == 2) // z axis
                {
                    r1 = r2 = p1.z;
                    if (r1 > p2.z) r1 = p2.z;
                    if (r1 > p3.z) r1 = p3.z;
                    if (r2 < p2.z) r2 = p2.z;
                    if (r2 < p3.z) r2 = p3.z;
                    //outof range
                    if ((v1.z < r1 && v2.z < r1) ||
                         (v1.z > r2 && v2.z > r2)) continue;
                    //axis triangle relation
                    p = v1.toVector64(); p.z = 0;
                    p1.z = 0; p2.z = 0; p3.z = 0;
                    if (!CTriangle2f.IsPointInTriangle(p, p1, p2, p3)) continue;
                    //calculation intersection
                    if (tri.GetIntersectionOnTriangle(v1.toVector64(), v2.toVector64(), out p))
                    {
                        sects.Add(p);
                        triIndices.Add(i);
                    }
                }

            }//for (int i = 0; i < triangles.Count; i++)

            if (sects.Count > 0)
                return true;
            else return false;
        } //end of function

        public bool GetLineIntersection(Vector64 p1, Vector64 p2, out Vector64 p, out CTriangle3f ret)
        {
            int i1, i2, i3;
            p = new Vector64(0, 0, 0);
            ret = new CTriangle3f();
            if (!IsInRange(p1.x, p1.y, p1.z) && !IsInRange(p2.x, p2.y, p2.z))
                return false;

            CTriangle3f tri = new CTriangle3f();
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                tri.p1 = points[i1].toVector64();
                tri.p2 = points[i2].toVector64();
                tri.p3 = points[i3].toVector64();
                if (tri.GetIntersectionOnTriangle(p1, p2, out p))
                {
                    ret = tri;
                    return true;
                }
            }
            return false;
        }      

        public virtual void CutWithPolygon(Polygon3D poly, bool keep_outer = true)
        {
            if (points.Count < 1) return;

            bool[] cuted = new bool[points.Count];
            int[] indices = new int[points.Count];
            for (int i = 0; i < cuted.Length; i++)
            {
                cuted[i] = false;
                indices[i] = 0;
            }

            Vector32 p1;
            for (int i = 0; i < points.Count; i++)
            {
                p1 = TransformedPoint(points[i]);
                if (poly.IsPointInPolygon(p1))
                {
                    if (keep_outer) cuted[i] = true;
                }
                else
                {
                    if (!keep_outer) cuted[i] = true;
                }
            }
            int index = 0;
            for (int i = 0; i < cuted.Length; i++)
            {
                if (cuted[i]) index++;
                indices[i] = index;
            }

            List<Int32XYZ> new_tries = new List<Int32XYZ>();
            int i1, i2, i3;
            for (int i = 0; i < triangles.Count; i++)
            {
                i1 = triangles[i].x;
                i2 = triangles[i].y;
                i3 = triangles[i].z;
                if (cuted[i1]) continue;
                if (cuted[i2]) continue;
                if (cuted[i3]) continue;
                i1 = i1 - indices[i1];
                i2 = i2 - indices[i2];
                i3 = i3 - indices[i3];
                new_tries.Add(new Int32XYZ(i1, i2, i3));
            }

            triangles.Clear();
            for (int i = 0; i < new_tries.Count; i++)
            {
                triangles.Add(new_tries[i]);
            }
            new_tries.Clear();

            for (int i = cuted.Length - 1; i >= 0; i--)
            {
                if (cuted[i])
                {
                    points.RemoveAt(i);
                }
            }
            cuted = null;
            indices = null;
        }

        public override void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    minx = maxx = points[i].X;
                    miny = maxy = points[i].Y;
                    minz = maxz = points[i].Z;
                }
                else
                {
                    if (points[i].X < minx) minx = points[i].X;
                    if (points[i].Y < miny) miny = points[i].Y;
                    if (points[i].Z < minz) minz = points[i].Z;
                    if (points[i].X > maxx) maxx = points[i].X;
                    if (points[i].Y > maxy) maxy = points[i].Y;
                    if (points[i].Z > maxz) maxz = points[i].Z;
                }
            }
        }
        public override bool LoadFrom(string path)
        {
            Clear();
            
            PlyFile ply = new PlyFile();
            if (!ply.LoadFrom(path)) return false;

            TriangleObj obj = ply.toTriangleObj();
            points = obj.points;
            colors = obj.colors;
            triangles = obj.triangles;
            texCoords = obj.texCoords;
            color = obj.color;
            textureStruct = obj.textureStruct.Copy();

            return true;
        }
        public override bool SaveAs(string path)
        {
            return ExportData(path);

            Vector32 p;
            Int32XYZ tri;
            if (points.Count < 3 || triangles.Count < 1)
            {
                errMessage = "vertics and faces are not enough";
                return false;
            }
            try
            {
                StringBuilder builder = new StringBuilder();
                File.WriteAllText(path, builder.ToString());
                builder.AppendLine("ply");
                builder.AppendLine("format ascii 1.0");
                builder.AppendLine("element vertex " + points.Count);

                builder.AppendLine("property float32 x");
                builder.AppendLine("property float32 y");
                builder.AppendLine("property float32 z");
                builder.AppendLine("element face " + triangles.Count);
                builder.AppendLine("property list uint8 int32 vertex_indices");
                builder.AppendLine("end_header");

                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    builder.AppendLine(p.x + " " + p.y + " " + p.z);
                }
                for (int i = 0; i < triangles.Count; i++)
                {
                    tri = triangles[i];
                    builder.AppendLine("3 " + tri.x + " " + tri.y + " " + tri.z);
                }
                File.WriteAllText(path, builder.ToString());
                builder.Clear();
                builder = null;
            }
            catch (Exception e)
            {
                errMessage = "writing to file faild!\n" + e.Message;
                return false;
            }
            return true;
        }
        public override bool ExportData(string path)
        {
            return SaveAsPLY(path);            
        }
        public bool SaveAsVRML(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);

                VRML.ExportVRMLHeader(wr);
                VRML.ExportVRMLSky(wr);

                bool ret = ExportVRML(wr);

                wr.Close();
                fs.Close();
                return ret;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }

        void ExportVRMLApperance(StreamWriter wr, string txtfile = "")
        {
            string line;

            line = "    appearance Appearance"; wr.WriteLine(line);
            line = "    {"; wr.WriteLine(line);

            if (txtfile.Length > 1)
            {
                line = "      texture ImageTexture "; wr.WriteLine(line);
                line = "      {"; wr.WriteLine(line);

                line = "            url " + "\"" + txtfile + "\"";
                wr.WriteLine(line);

                line = "            repeatS TRUE"; wr.WriteLine(line);
                line = "            repeatT TRUE"; wr.WriteLine(line);

                line = "      }"; wr.WriteLine(line);
            }

            //material Material
            line = "      material Material"; wr.WriteLine(line);
            line = "      {"; wr.WriteLine(line);
            if (IsUniformColor)
            {
                line = "      diffuseColor " + color.x + " " + color.y + " " + color.z;
                wr.WriteLine(line);
                line = "      ambientIntensity 0.4"; wr.WriteLine(line);
                line = "      specularColor 0.8 0.8 0.8"; wr.WriteLine(line);
            }

            if (Alpha < 1 && Blend)
            {
                line = "      transparency " + Alpha;
                wr.WriteLine(line);
            }

            line = "      }"; wr.WriteLine(line);

            line = "    }#end Apperance"; wr.WriteLine(line);
        }
        
        void ExportVRMLCoordinates(StreamWriter wr)
        {
            string line;
            Vector32 p;

            line = "    #begin coord Coordinate"; wr.WriteLine(line);
            line = "    coord Coordinate"; wr.WriteLine(line);
            line = "    {"; wr.WriteLine(line);
            line = "        point["; wr.WriteLine(line);
            int n = points.Count;
            line = " ";
            for (int i = 0; i < n; i++)
            {              
                p = TransformedPoint(points[i]);
                if ( CDataModel.IsEarthMapVision ) p = toWorldVector(p);
                p = CDataModel.ToModelVector32(p);
                line += (p.x + " " + p.y + " " + p.z + ", ");
                if ((i == n - 1) || (i > 0 && i % 50 == 0))
                {
                    wr.WriteLine(line);
                    line = "          ";
                }
            }
            line = "   ]"; wr.WriteLine(line);
            line = "  } #end Coordinate"; wr.WriteLine(line);

            line = "coordIndex["; wr.WriteLine(line);
            n = triangles.Count;
            line = "";
            for (int i = 0; i < n; i++)
            {
                line += (triangles[i].x + " " + triangles[i].y + " " + triangles[i].z + " -1,");
                if (i == n - 1 || (i > 0 && i % 100 == 0))
                {
                    wr.WriteLine(line);
                    line = "          ";
                }
            }
            line = "]#end coordIndex"; wr.WriteLine(line);
            line = "#enf of coord Coordinate \n"; wr.WriteLine(line);
        }
        void ExportVRMLNormals(StreamWriter wr)
        {
            string line;
            Vector32 pn;

            line = " #Begin Normal------------"; wr.WriteLine(line);
            line = "normal Normal"; wr.WriteLine(line);
            line = "{"; wr.WriteLine(line);
            line = "vector["; wr.WriteLine(line);

            int n = points.Count;
            Vector32[] normals = new Vector32[points.Count];
            CalculateNormals(normals, points, triangles);
            line = "";
            for (int i = 0; i < n; i++)
            {
                pn = normals[i];
                line += (pn.x + " " + pn.y + " " + pn.z + ",");
                if (i == n - 1 || (i > 0 && i % 50 == 0))
                {
                    wr.WriteLine(line);
                    line = "";
                }
            }
            normals = null;
            line = "] #end vector"; wr.WriteLine(line);
            line = "}"; wr.WriteLine(line);
            line = "normalIndex["; wr.WriteLine(line);
            line = "";
            for (int i = 0; i < n; i++)
            {
                line += (i + ",");
                if (i == n - 1 || (i > 0 && i % 100 == 0))
                {
                    wr.WriteLine(line);
                    line = "";
                }
            }
            line = "] #end normalIndex"; wr.WriteLine(line);
            line = "normalPerVertex TRUE"; wr.WriteLine(line);
            //line = "ccw	FALSE"; wr.WriteLine(line);
            line = " #End of Normal------------\n"; wr.WriteLine(line);
        }
        void ExportVRMLColors(StreamWriter wr)
        {
            string line;

            line = " #Begin Colors ------------"; wr.WriteLine(line);
            line = "color Color"; wr.WriteLine(line);
            line = "{"; wr.WriteLine(line);
            line = "color["; wr.WriteLine(line);
            int n = colors.Count;
            line = "";
            vec4 cc;
            
            bool byteColor = IsByteColor;

            for (int i = 0; i < n; i++)
            {
                cc = colors[i];
                if( byteColor ) line += (cc.x/255 + " " + cc.y/255 + " " + cc.z/255 + ",");
                else line += (cc.x + " " + cc.y + " " + cc.z + ",");

                if (i == n - 1 || (i > 0 && i % 50 == 0))
                {
                    wr.WriteLine(line);
                    line = "";
                }
            }
            line = "]"; wr.WriteLine(line);
            line = "}#end color Color"; wr.WriteLine(line);
            /*不需要这个？？？
            line = "colorIndex["; wr.WriteLine(line);
            line = "";
            for (int i = 0; i < n; i++)
            {
                line += ( i + ",");
                if (i == n - 1 || (i > 0 && i % 100 == 0))
                {
                    wr.WriteLine(line);
                    line = "";
                }
            }
            line = "]#end colorIndex"; wr.WriteLine(line);
            */
            line = "colorPerVertex TRUE"; wr.WriteLine(line);
            line = " #End Colors ------------\n"; wr.WriteLine(line);
        }
        void ExportVRMLTexture(StreamWriter wr)
        {
            string line;
            //有纹理贴图，优先使用纹理
            line = " #-------------------------"; wr.WriteLine(line);
            line = "         texCoord TextureCoordinate "; wr.WriteLine(line);
            line = "         { point["; wr.WriteLine(line);
            line = "                 ";
            for (int i = 0; i < texCoords.Count; i++)
            {
                line += (texCoords[i].x + " " + texCoords[i].y + ",");
                if (i == texCoords.Count - 1 ||
                    (i > 0 && i % 100 == 0))
                {
                    wr.WriteLine(line);
                    line = "                 ";
                }
            }
            line = "           ]#end of point"; wr.WriteLine(line);
            line = "         }#end of TextureCoordinate"; wr.WriteLine(line);

        }
        public override bool ExportVRML(StreamWriter wr)
        {
            if (!Visible) return false;
            int n = points.Count;
            if (n < 1) return false;

            Vector32 p;
            string line;
            line = "#---Triangles " + Name; wr.WriteLine(line);
            string defname = Name;
            defname = defname.Replace(' ', '_');
            defname = defname.Replace('\t', '_');
            //export transform
            line = "DEF " + "MESH_" + defname + " Transform"; wr.WriteLine(line);
            line = "{"; wr.WriteLine(line);

            bool texture = false;
            if (textureStruct.TextureFile.Length > 1 && texCoords.Count > 1)
            {
                FileInfo ff = new FileInfo(textureStruct.TextureFile);
                if (ff.Exists) texture = true;
            }

            // translation            
            line = "  translation 0 0 0";
            wr.WriteLine(line);

            //scale
            line = "  scale 1 1 1";
            wr.WriteLine(line);
            line = "  children"; wr.WriteLine(line);
            line = "  ["; wr.WriteLine(line);
            line = "   Shape"; wr.WriteLine(line);
            line = "   {"; wr.WriteLine(line);

            if (texture) ExportVRMLApperance(wr, textureStruct.TextureFile);
            else ExportVRMLApperance(wr);

            line = "      geometry IndexedFaceSet"; wr.WriteLine(line);
            line = "      { "; wr.WriteLine(line);
            ExportVRMLCoordinates(wr);
            if (!IsUniformColor) ExportVRMLNormals(wr);

            //有纹理贴图，优先使用纹理
            if (texture) ExportVRMLTexture(wr);
            else//没有纹理,使用颜色
            {
                if (!IsUniformColor && colors.Count > 0)
                    ExportVRMLColors(wr);
            }

            line = "     }#end of geometry"; wr.WriteLine(line);
            line = "    }#end Shape "; wr.WriteLine(line);
            line = "   ]#end children "; wr.WriteLine(line);
            line = "}#end of transform"; wr.WriteLine(line);
            return true;
        }
    }


    public class GeometryHelper
    {
        const double EquityTolerance = 0.000000001d;

        public static bool IsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= EquityTolerance;
        }
        //math logic from http://www.wyrmtale.com/blog/2013/115/2d-line-intersection-in-c
        public static bool GetIntersectionPoint(Vector32 l1p1, Vector32 l1p2, Vector32 l2p1, Vector32 l2p2, out Vector32 p)
        {
            p = new Vector32(0, 0, 0);

            double A1 = l1p2.Y - l1p1.Y;
            double B1 = l1p1.X - l1p2.X;
            double C1 = A1 * l1p1.X + B1 * l1p1.Y;

            double A2 = l2p2.Y - l2p1.Y;
            double B2 = l2p1.X - l2p2.X;
            double C2 = A2 * l2p1.X + B2 * l2p1.Y;

            //lines are parallel
            double det = A1 * B2 - A2 * B1;
            if (IsEqual(det, 0d))
            {
                return false; //parallel lines
            }
            else
            {
                double x = (B2 * C1 - B1 * C2) / det;
                double y = (A1 * C2 - A2 * C1) / det;
                bool online1 = ((Math.Min(l1p1.X, l1p2.X) < x || IsEqual(Math.Min(l1p1.X, l1p2.X), x))
                    && (Math.Max(l1p1.X, l1p2.X) > x || IsEqual(Math.Max(l1p1.X, l1p2.X), x))
                    && (Math.Min(l1p1.Y, l1p2.Y) < y || IsEqual(Math.Min(l1p1.Y, l1p2.Y), y))
                    && (Math.Max(l1p1.Y, l1p2.Y) > y || IsEqual(Math.Max(l1p1.Y, l1p2.Y), y))
                    );
                bool online2 = ((Math.Min(l2p1.X, l2p2.X) < x || IsEqual(Math.Min(l2p1.X, l2p2.X), x))
                    && (Math.Max(l2p1.X, l2p2.X) > x || IsEqual(Math.Max(l2p1.X, l2p2.X), x))
                    && (Math.Min(l2p1.Y, l2p2.Y) < y || IsEqual(Math.Min(l2p1.Y, l2p2.Y), y))
                    && (Math.Max(l2p1.Y, l2p2.Y) > y || IsEqual(Math.Max(l2p1.Y, l2p2.Y), y))
                    );

                if (online1 && online2)
                {
                    p = new Vector32((float)x, (float)y, 0);
                    return true;
                }
            }
            return false; //intersection is at out of at least one segment.
        }
    }

    public struct ImageStruct
    {
        public DoubleRect rect;
        public Image img;
        public string errMsg;
        public bool SaveAs(ref BinaryWriter br)
        {
            try
            {
                br.Write(rect.x1);
                br.Write(rect.y1);
                br.Write(rect.x2);
                br.Write(rect.y2);

                Bitmap bmp = new Bitmap(img);//a copy from locked img

                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);

                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                br.Write(Convert.ToInt32(stream.Length));
                br.Write(data, 0, Convert.ToInt32(stream.Length));
                stream.Close();
                stream.Dispose();
                bmp.Dispose();

                data = null;
                return true;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return false;
            }
        }
        public bool ReadFrom(BinaryReader br)
        {
            try
            {
                double x1, y1, x2, y2;
                x1 = br.ReadDouble();
                y1 = br.ReadDouble();
                x2 = br.ReadDouble();
                y2 = br.ReadDouble();
                rect = new DoubleRect(x1, y1, x2, y2);

                int length = br.ReadInt32();
                byte[] data = new byte[length];
                data = br.ReadBytes(length);

                MemoryStream stream = new MemoryStream();
                stream.Write(data, 0, length);

                img = Image.FromStream(stream);


                stream.Close();
                stream.Dispose();
                data = null;

                return true;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return false;
            }
        }
    }

    //include polygons and lines
    public class PolygonSlicer : C3DObjectBase
    {
        //空间定位
        public AxisEnum axis = AxisEnum.zAxis;
        public List<Vector64> Locations2D = new List<Vector64>(); //Slicer平面定位点
        public List<Vector64> Locations3D = new List<Vector64>(); //空间定位点

        public Vector64 LocationCorner1 = new Vector64();   //空间坐标最低点, XY对应着2D点位置
        public Vector64 LocationCorner2 = new Vector64();   //空间坐标最高点，XY对应着2D点位置

        public double minxLocated = 0;  //空间定位后的坐标范围
        public double minyLocated = 0;
        public double minzLocated = 0;
        public double maxxLocated = 0;
        public double maxyLocated = 0;
        public double maxzLocated = 0;
        public List<Vector64> sampledGrids = new List<Vector64>();//网格采样后的空间散乱点
        //----------------------------------------------------------
        //辅助图形对象,多边形，线，etc
        public C2DPolygons polygons = new C2DPolygons("Background Objects");
        //public List<Polygon2D> polygons = new List<Polygon2D>();

        //地层对象，通过边界追踪提取的geo对象，或者绘制的geo对象，
        //包括layer和section line
        public C2DPolygons tracedGeoObjects = new C2DPolygons("Geological Objects");
        //public List<Polygon2D> tracedGeoObjects = new List<Polygon2D>();

        //modified by jian 2020-12-9
        public List<ImageStruct> backImages = new List<ImageStruct>();

        public double backgroundPropertyValue { get; set; } = 0;
        public string originalPath = "";    //打开文件全路径
        public override float Alpha 
        {
            get => base.Alpha;
            set 
            {
                _Alpha = value;
                foreach(Polygon2D poly in tracedGeoObjects.Polygons)
                {
                    poly.Alpha = value;
                }
                foreach (Polygon2D poly in polygons.Polygons)
                {
                    poly.Alpha = value;
                }
            }
        }
        public override bool Blend 
        {
            get => base.Blend;
            set
            {
                _Blend = value;
                foreach (Polygon2D poly in tracedGeoObjects.Polygons)
                {
                    poly._Blend = value;
                }
                foreach (Polygon2D poly in polygons.Polygons)
                {
                    poly._Blend = value;
                }
            }
        }
        public override bool IsWireFrameMode
        {
            get => base._IsWireFrameMode;
            set
            {
                _IsWireFrameMode = value;
                foreach (Polygon2D poly in tracedGeoObjects.Polygons)
                {
                    poly._IsWireFrameMode = value;
                }
                foreach (Polygon2D poly in polygons.Polygons)
                {
                    poly._IsWireFrameMode = value;
                }
            }
        }
        public override bool enbaleTexture 
        {
            get { return _textStruct.Enable; }
            set
            {
                _textStruct.Enable = value;
                foreach (Polygon2D poly in tracedGeoObjects.Polygons)
                {
                    poly._textStruct.Enable = value;
                }
                foreach (Polygon2D poly in polygons.Polygons)
                {
                    poly._textStruct.Enable = value;
                }
            }
        }
        public override double Minx
        {
            get
            {
                if (IsLocated) return minxLocated;
                else return minx;
            }
        }
        public override double Miny
        {
            get
            {
                if (IsLocated) return minyLocated;
                else return miny;
            }
        }
        public override double Minz
        {
            get
            {
                if (IsLocated) return minzLocated;
                else return minz;
            }
        }
        public override double Maxx
        {
            get
            {
                if (IsLocated) return maxxLocated;
                else return maxx;
            }
        }
        public override double Maxy
        {
            get
            {
                if (IsLocated) return maxyLocated;
                else return maxy;
            }
        }
        public override double Maxz
        {
            get
            {
                if (IsLocated) return maxzLocated;
                else return maxz;
            }
        }
        public PolygonSlicer(string name = "untitled")
        {
            type = ShapeEnum.PolygonSlicer;
            axis = AxisEnum.zAxis;
            polygons.Parent = this;
            tracedGeoObjects.Parent = this;
            Name = name;
        }
        /// <summary>
        /// 父对象中清除子对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Remove(C3DObjectBase obj)
        {
            obj.Clear();
            return true;
        }
        public override void ClearRenderingBuffers()
        {
            polygons.ClearRenderingBuffers();
            tracedGeoObjects.ClearRenderingBuffers();
            RenderingBuffers.Clear();
        }
        public override void UpdateDrawMode(RenderingUpdateMode render = RenderingUpdateMode.Redraw)
        {
            base.UpdateDrawMode(render);
            polygons.RenderMode = render;
            tracedGeoObjects.RenderMode = render;
        }
        public PolygonSlicer Copy()
        {
            PolygonSlicer poly = new PolygonSlicer();

            foreach (Polygon2D p in polygons.Polygons)
                poly.AddPolygon(p.Copy());
            
            poly.CopyHeaderFrom(this);

            poly.axis = axis;
            poly.minxLocated = minxLocated;
            poly.minyLocated = minyLocated;
            poly.minzLocated = minzLocated;
            poly.maxxLocated = maxxLocated;
            poly.maxyLocated = maxyLocated;
            poly.maxzLocated = maxzLocated;
            poly.backgroundPropertyValue = backgroundPropertyValue;
            poly.originalPath = originalPath;

            foreach (Polygon2D obj in tracedGeoObjects.Polygons)
                poly.tracedGeoObjects.Add(obj.Copy());

            //背景图片
            foreach (ImageStruct img in backImages)
                poly.backImages.Add(img);

            Vector64 p1, p2;
            poly.Locations2D.Clear();
            poly.Locations3D.Clear();
            for (int i = 0; i < Locations2D.Count; i++)
            {
                p1 = Locations2D[i];
                p2 = Locations3D[i];
                poly.AddLocationPoint(p1, p2);
            }

            poly.LocationCorner1 = LocationCorner1;
            poly.LocationCorner2 = LocationCorner2;

            for (int i = 0; i < sampledGrids.Count; i++)
                poly.sampledGrids.Add(sampledGrids[i]);

            return poly;
        }
        public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            if (IsLocated)
            {
                Vector64 p;

                //double zmax = maxz - minz; //MaxZ
                //double zlen = Math.Abs(LocationCorner2.z - LocationCorner1.z);//tracedZ
                //double zscale = zlen / zmax;

                for (int i = 0; i < Locations3D.Count; i++)
                {
                    p = Locations3D[i];
                    if (Maxx > Minx)
                        p.x = x1 + (x2 - x1) * (p.x - Minx) / (Maxx - Minx);
                    else p.x = (x1 + x2) / 2;
                    if (Maxy > Miny)
                        p.y = y1 + (y2 - y1) * (p.y - Miny) / (Maxy - Miny);
                    else p.y = (y1 + y2) / 2;
                    if (Maxz > Minz)
                        p.z = z1 + (z2 - z1) * (p.z - Minz) / (Maxz - Minz);
                    else p.z = (z1 + z2) / 2;
                    Locations3D[i] = p;
                }
                UpdateLocationRange();
            }
            else
            {
                UpdateRange();
                for (int i = 0; i < polygons.Count; i++)
                {
                    Polygon2D poly = polygons[i];
                    for (int j = 0; j < poly.points.Count; j++)
                    {
                        Vector64 p = Locations3D[j];
                        if (Maxx > Minx)
                            p.x = (x1 + (x2 - x1) * (p.x - Minx) / (Maxx - Minx));
                        else p.x = ((x1 + x2) / 2);
                        if (Maxy > Miny)
                            p.y = (y1 + (y2 - y1) * (p.y - Miny) / (Maxy - Miny));
                        else p.y = (y1 + y2) / 2;
                        if (Maxz > Minz)
                            p.z = (z1 + (z2 - z1) * (p.z - Minz) / (Maxz - Minz));
                        else p.z = (z1 + z2) / 2;
                        poly.points[j] = p;
                    }
                    polygons[i] = poly;
                }
                for (int i = 0; i < tracedGeoObjects.Count; i++)
                {
                    Polygon2D poly = tracedGeoObjects[i];
                    for (int j = 0; j < poly.points.Count; j++)
                    {
                        Vector64 p = Locations3D[j];
                        if (Maxx > Minx)
                            p.x = (x1 + (x2 - x1) * (p.x - Minx) / (Maxx - Minx));
                        else p.x = ((x1 + x2) / 2);
                        if (Maxy > Miny)
                            p.y = (y1 + (y2 - y1) * (p.y - Miny) / (Maxy - Miny));
                        else p.y = (y1 + y2) / 2;
                        if (Maxz > Minz)
                            p.z = (z1 + (z2 - z1) * (p.z - Minz) / (Maxz - Minz));
                        else p.z = (z1 + z2) / 2;
                        poly.points[j] = p;
                    }
                    tracedGeoObjects[i] = poly;
                }
                minx = x1;
                maxx = x2;
                miny = y1;
                maxy = y2;
                minz = z1;
                maxz = z2;
            }
        }       
        public override bool SaveAs(string path)
        {
            BinaryWriter br;
            try
            {
                originalPath = path;
                br = new BinaryWriter(new FileStream(path, FileMode.Create));
                bool ret = SaveAs(br);
                br.Close();
                return ret;
            }
            catch (IOException e)
            {
                return false;
            }
        }
        public override bool LoadFrom(string path)
        {
            BinaryReader br = null;
            try
            {
                originalPath = path;
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                bool ret = LoadFrom(br);
                br.Close();
                return ret;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                if( br != null) br.Close();
                return false;
            }
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if (!SaveObjHeader(br)) return false;
            br.Write((int)axis);
            br.Write(minxLocated);
            br.Write(minyLocated);
            br.Write(minzLocated);
            br.Write(maxxLocated);
            br.Write(maxyLocated);
            br.Write(maxzLocated);
            br.Write(backgroundPropertyValue);

            br.Write(polygons.Count);
            foreach (Polygon2D obj in polygons.Polygons)
            {
                if (!obj.SaveAs(br)) return false;
            }
            br.Write(tracedGeoObjects.Count);
            foreach (Polygon2D obj in tracedGeoObjects.Polygons)
            {
                if (!obj.SaveAs(br)) return false;
            }
            Vector64 p1, p2;
            br.Write(Locations2D.Count);
            for (int i = 0; i < Locations2D.Count; i++)
            {
                p1 = Locations2D[i];
                p2 = Locations3D[i];
                br.Write(p1.X);
                br.Write(p1.Y);
                br.Write(p1.Z);
                br.Write(p2.X);
                br.Write(p2.Y);
                br.Write(p2.Z);
            }
            p1 = LocationCorner1;
            p2 = LocationCorner2;
            br.Write(p1.X);
            br.Write(p1.Y);
            br.Write(p1.Z);
            br.Write(p2.X);
            br.Write(p2.Y);
            br.Write(p2.Z);

            br.Write(backImages.Count);
            foreach (ImageStruct img in backImages)
            {
                if (!img.SaveAs(ref br))
                {
                    return false;
                }
            }

            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {
            LoadObjHeader(br);
            axis = (AxisEnum)br.ReadInt32();
            minxLocated = br.ReadDouble();
            minyLocated = br.ReadDouble();
            minzLocated = br.ReadDouble();
            maxxLocated = br.ReadDouble();
            maxyLocated = br.ReadDouble();
            maxzLocated = br.ReadDouble();
            backgroundPropertyValue = br.ReadDouble();

            int n = br.ReadInt32();
            polygons.Clear();
            for (int i = 0; i < n; i++)
            {
                Polygon2D obj = new Polygon2D();
                if (!obj.LoadFrom(br)) return false;
                obj.Parent = polygons;
                polygons.Add(obj);
            }

            n = br.ReadInt32();
            tracedGeoObjects.Clear();
            for (int i = 0; i < n; i++)
            {
                Polygon2D obj = new Polygon2D();
                if (!obj.LoadFrom(br)) return false;
                obj.Parent = tracedGeoObjects;
                tracedGeoObjects.Add(obj);
            }

            n = br.ReadInt32();
            Locations2D.Clear();
            Locations3D.Clear();

            Vector64 p1, p2;
            double x, y, z;
            for (int i = 0; i < n; i++)
            {
                x = br.ReadDouble();
                y = br.ReadDouble();
                z = br.ReadDouble();
                p1 = new Vector64(x, y, z);
                x = br.ReadDouble();
                y = br.ReadDouble();
                z = br.ReadDouble();
                p2 = new Vector64(x, y, z);
                AddLocationPoint(p1, p2);
            }

            x = br.ReadDouble();
            y = br.ReadDouble();
            z = br.ReadDouble();
            LocationCorner1 = new Vector64(x, y, z);
            x = br.ReadDouble();
            y = br.ReadDouble();
            z = br.ReadDouble();
            LocationCorner2 = new Vector64(x, y, z);

            backImages.Clear();
            ///*
            if (br.PeekChar() >= 0) //是否有图像数据
            {
                int imgCount = br.ReadInt32();
                for (int i = 0; i < imgCount; i++)
                {
                    ImageStruct img = new ImageStruct();
                    if (img.ReadFrom(br))
                        backImages.Add(img);
                    else return false;
                }
            }
            //*/
            return true;
        }
        public int SetLayersPropertyByName(List<LayerProperty> layers)
        {
            int count = 0;
            string name1, name2;
            Polygon2D poly;
            for (int i = 0; i < tracedGeoObjects.Count; i++)
            {
                poly = tracedGeoObjects[i];
                name1 = poly.Name.ToLower();
                for (int j = 0; j < layers.Count; j++)
                {
                    name2 = layers[j].LayerName.ToLower();
                    if (name1 == name2)
                    {
                        poly.fillColor = layers[j].LayerColor;
                        poly.PropertyValue = layers[j].LayerValue;
                        tracedGeoObjects[i] = poly;
                        count++;
                        break;
                    }
                }
            }
            return count;
        }
        public void AddTracedGeoObject(Polygon2D obj)
        {
            obj.Parent = tracedGeoObjects;
            tracedGeoObjects.Add(obj);
        }
        public void AddPolygon(Polygon2D poly)
        {
            poly.Parent = polygons;
            polygons.Add(poly);
        }
        public bool IsLocated
        {
            get
            {
                if (Locations2D.Count > 1) return true;
                else return false;
            }
        }

        public void AddLocationPoint(Vector64 p2d, Vector64 p3d)
        {
            if (Locations2D.Contains(p2d)) return;
            if (Locations3D.Contains(p3d)) return;
            Locations2D.Add(p2d);
            Locations3D.Add(p3d);
        }

        //得到空间定位点的最高值和最低值
        private void UpdateLocationCorner()
        {
            int n = Locations3D.Count;
            if (n < 2) return;
            Vector64 p1, p2;
            for (int i = 0; i < Locations3D.Count; i++)
            {
                p1 = Locations2D[i];
                p2 = Locations3D[i];
                if (i == 0)
                {
                    if (axis == AxisEnum.zAxis) //Hight is Z
                    {
                        LocationCorner1.X = p1.X; //2D Point
                        LocationCorner1.Y = p1.Y; //2D Point
                        LocationCorner1.Z = p2.Z; //3D Point
                        LocationCorner2 = LocationCorner1;
                    }
                    if (axis == AxisEnum.yAxis)//Hight is Y
                    {
                        LocationCorner1.X = p1.X;
                        LocationCorner1.Y = p1.Y;
                        LocationCorner1.Z = p2.Y;
                        LocationCorner2 = LocationCorner1;
                    }
                    if (axis == AxisEnum.xAxis)//Hight is X
                    {
                        LocationCorner1.X = p1.X;
                        LocationCorner1.Y = p1.Y;
                        LocationCorner1.Z = p2.X;
                        LocationCorner2 = LocationCorner1;
                    }
                }
                else
                {
                    if (axis == AxisEnum.zAxis)//Hight is Z
                    {
                        if (p2.Z < LocationCorner1.Z)
                        {
                            LocationCorner1.X = p1.X;
                            LocationCorner1.Y = p1.Y;
                            LocationCorner1.Z = p2.Z;
                        }
                        if (p2.Z > LocationCorner2.Z)
                        {
                            LocationCorner2.X = p1.X;
                            LocationCorner2.Y = p1.Y;
                            LocationCorner2.Z = p2.Z;
                        }
                    }
                    else if (axis == AxisEnum.yAxis)//Hight is Y
                    {
                        if (p2.Y < LocationCorner1.Z)
                        {
                            LocationCorner1.X = p1.X;
                            LocationCorner1.Y = p1.Y;
                            LocationCorner1.Z = p2.Y;
                        }
                        if (p2.Y > LocationCorner2.Z)
                        {
                            LocationCorner2.X = p1.X;
                            LocationCorner2.Y = p1.Y;
                            LocationCorner2.Z = p2.Y;
                        }
                    }
                    else if (axis == AxisEnum.xAxis)//Hight is X
                    {
                        if (p2.X < LocationCorner1.Z)
                        {
                            LocationCorner1.X = p1.X;
                            LocationCorner1.Y = p1.Y;
                            LocationCorner1.Z = p2.X;
                        }
                        if (p2.X > LocationCorner2.Z)
                        {
                            LocationCorner2.X = p1.X;
                            LocationCorner2.Y = p1.Y;
                            LocationCorner2.Z = p2.X;
                        }
                    }
                }
            }
        }

        //更新空间定位数据
        public void UpdateTraced()
        {
            if (!IsLocated) return;
            SortLocation();
            DuplicationMask();
            UpdateLocationRange();
        }
        
        /// <summary>
        /// 相同X位置点如何处理？
        /// 保留高度差异最大的点
        /// </summary>
        private void DuplicationMask()
        {
            int n = Locations2D.Count;
            if (n < 1) return;

            Vector64 p1, p2, v;
            
            bool[] marked = new bool[n];
            double y1 = 0, y2 = 0;            
            p1 = Locations2D[0];
            y1 = y2 = p1.Y;
            v = p1;
            for (int i = 1; i < n; i++)
            {
                p2 = Locations2D[i];
                if (p1.X == p2.X) //相同点
                {
                    if ((p2.Y < y1 && p2.Y < p1.Y) &&
                         (p2.Y > y2 && p2.y > p1.Y))
                    {
                        marked[i - 1] = true; //保留p2
                        if (p2.Y < y1) y1 = p2.Y;
                        if (p2.Y > y2) y2 = p2.Y;
                        p1 = p2;
                    }
                    else if ((p1.Y < y1 && p1.y < p2.Y) &&
                              (p1.Y > y2 && p1.y > p2.Y))
                    {
                        marked[i] = true;   //保留p1
                        if (p1.Y < y1) y1 = p1.Y;
                        if (p1.Y > y2) y2 = p1.Y;
                    }
                    else
                    {
                        marked[i] = true;   //保留p1
                    }
                }
                else //不同X的点
                {
                    if (p2.Y < y1) y1 = p2.Y;
                    if (p2.Y > y2) y2 = p2.Y;
                    p1 = p2;
                }
            }
            for(int i=n-1;i>=0;i--)
            {
                if( marked[i] )
                {
                    Locations2D.RemoveAt(i);
                    Locations3D.RemoveAt(i);
                }
            }
            marked = null;
        }

        private void SortLocation()
        {
            int n = Locations2D.Count;
            Vector64 p1, p2;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                {
                    p1 = Locations2D[i];
                    p2 = Locations2D[j];
                    if (p1.X > p2.X)
                    {
                        Locations2D[i] = p2;
                        Locations2D[j] = p1;
                        p1 = Locations3D[i];
                        p2 = Locations3D[j];
                        Locations3D[i] = p2;
                        Locations3D[j] = p1;
                    }
                }
        }

        //根据屏幕坐标y值，计算空间的高程 Z -zAxis, Y -yAxis, X - xAxis
        private double GetTracedHight(double y)
        {
            double yy = LocationCorner2.y - LocationCorner1.y;
            if (yy == 0) return 0;
            double hh = LocationCorner2.Z - LocationCorner1.Z;
            return LocationCorner1.Z + hh * (y - LocationCorner1.y) / yy;
        }

        private Vector64 toTracedPoint(Vector64 p, int id1, int id2)
        {
            Vector64 p1 = Locations2D[id1];
            Vector64 p2 = Locations2D[id2];

            Vector64 v1 = Locations3D[id1];
            Vector64 v2 = Locations3D[id2];

            double scale = (p.X - p1.X) / (p2.X - p1.X);

            Vector64 v = v1 + scale * (v2 - v1);
            if (axis == AxisEnum.xAxis) v.X = GetTracedHight(p.Y);
            if (axis == AxisEnum.yAxis) v.Y = GetTracedHight(p.Y);
            if (axis == AxisEnum.zAxis) v.Z = GetTracedHight(p.Y);
            v.V = p.V;

            return v;
        }
        public Vector64 toTracedPoint(double x, double y, double z, double v)
        {
            return toTracedPoint(new Vector64(x, y, z, v));
        }
        public Vector64 toTracedPoint(Vector64 p)
        {
            if (Locations2D.Count < 2) return p;
            //left side            
            if (p.X <= Locations2D[0].X)
            {
                return toTracedPoint(p, 0, 1);
            }
            //right side            
            if (p.X >= Locations2D[Locations2D.Count - 1].X)
            {
                return toTracedPoint(p, Locations2D.Count - 2, Locations2D.Count - 1);
            }

            for (int i = 1; i < Locations2D.Count; i++)
            {
                if (p.X <= Locations2D[i].X) return toTracedPoint(p, i - 1, i);
            }
            return p;
        }

        /// <summary>
        /// 将对象转换成空间曲线
        /// </summary>
        /// <param name="poly">2D多边形对象</param>
        /// <returns></returns>
        public C3DLine toTraced3DLine(Polygon2D poly)
        {
            C3DLine line = new C3DLine(poly.Name);
            line.Closed = poly.IsClosed;
            line.lineStyle.Width = poly.lineWidth;
            line.lineStyle.Color = poly.lineColor;
            line.Value = poly.PropertyValue;

            foreach (Vector64 p in poly.points)
            {
                line.AddPoint(toTracedPoint(p));
            }

            return line;
        }

        //更新空间定位后的坐标范围
        private void UpdateLocationRange()
        {
            minxLocated = maxxLocated = 0;
            minyLocated = maxyLocated = 0;
            minzLocated = maxzLocated = 0;

            if (!IsLocated) return;

            if (LocationCorner1.Z <= LocationCorner2.Z) UpdateLocationCorner();

            //更新定位坐标范围
            Vector64 v;
            for (int j = 0; j < Locations3D.Count; j++)
            {
                v = Locations3D[j];
                if (j == 0)
                {
                    minxLocated = maxxLocated = v.x;
                    minyLocated = maxyLocated = v.y;
                    minzLocated = maxzLocated = v.z;
                }
                else
                {
                    if (v.x < minxLocated) minxLocated = v.x;
                    if (v.y < minyLocated) minyLocated = v.y;
                    if (v.x < minzLocated) minzLocated = v.z;
                    if (v.x > maxxLocated) maxxLocated = v.x;
                    if (v.y > maxyLocated) maxyLocated = v.y;
                    if (v.z > maxzLocated) maxzLocated = v.z;
                }
            }
            //按需更新
            if (minx >= maxx || miny >= maxy)
                UpdateRange();

            if (axis == AxisEnum.zAxis)
            {
                minzLocated = LocationCorner1.Z + (LocationCorner2.Z - LocationCorner1.Z) * (miny - LocationCorner1.Y) / (LocationCorner2.Y - LocationCorner1.Y);
                maxzLocated = LocationCorner1.Z + (LocationCorner2.Z - LocationCorner1.Z) * (miny - LocationCorner1.Y) / (LocationCorner2.Y - LocationCorner1.Y);
            }
            if (axis == AxisEnum.yAxis)
            {
                minyLocated = LocationCorner1.Z + (LocationCorner2.Z - LocationCorner1.Z) * (miny - LocationCorner1.Y) / (LocationCorner2.Y - LocationCorner1.Y);
                maxyLocated = LocationCorner1.Z + (LocationCorner2.Z - LocationCorner1.Z) * (miny - LocationCorner1.Y) / (LocationCorner2.Y - LocationCorner1.Y);
            }
            if (axis == AxisEnum.xAxis)
            {
                minxLocated = LocationCorner1.Z + (LocationCorner2.Z - LocationCorner1.Z) * (miny - LocationCorner1.Y) / (LocationCorner2.Y - LocationCorner1.Y);
                maxxLocated = LocationCorner1.Z + (LocationCorner2.Z - LocationCorner1.Z) * (miny - LocationCorner1.Y) / (LocationCorner2.Y - LocationCorner1.Y);
            }
        }

        public override void UpdateRange()
        {
            Polygon2D poly;
            bool init = false;
            for (int i = 0; i < polygons.Count; i++)
            {
                poly = polygons[i];
                poly.UpdateRange();
                if (i == 0)
                {
                    minx = poly.minx;
                    maxx = poly.maxx;
                    miny = poly.miny;
                    maxy = poly.maxy;
                    minz = poly.minz;
                    maxz = poly.maxz;
                }
                else
                {
                    if (poly.minx < minx) minx = poly.minx;
                    if (poly.maxx > maxx) maxx = poly.maxx;
                    if (poly.miny < miny) miny = poly.miny;
                    if (poly.maxy > maxy) maxy = poly.maxy;
                    if (poly.minz < minz) minz = poly.minz;
                    if (poly.maxz > maxz) maxz = poly.maxz;
                }
                init = true;
            }

            for (int i = 0; i < tracedGeoObjects.Count; i++)
            {
                Polygon2D obj = tracedGeoObjects[i];
                obj.UpdateRange();
                if (!init && i == 0)
                {
                    minx = obj.minx;
                    maxx = obj.maxx;
                    miny = obj.miny;
                    maxy = obj.maxy;
                    minz = obj.minz;
                    maxz = obj.maxz;

                    init = true;
                }
                else
                {
                    if (obj.minx < minx) minx = obj.minx;
                    if (obj.maxx > maxx) maxx = obj.maxx;
                    if (obj.miny < miny) miny = obj.miny;
                    if (obj.maxy > maxy) maxy = obj.maxy;
                    if (obj.minz < minz) minz = obj.minz;
                    if (obj.maxz > maxz) maxz = obj.maxz;
                }
            }
        }
        //获取指定点地层属性值
        public double GetPropertyValue(double x, double y)
        {
            //intColor = 0;
            Vector32 p = new Vector32(x, y, 0);
            foreach (Polygon2D obj in tracedGeoObjects.Polygons)
            {
                if (obj.IsPointInsidePoly(p))
                {
                    //intColor = ColorRGBA.ParseRGB(obj.fillColor);
                    return obj.PropertyValue;
                }
            }
            return backgroundPropertyValue;
        }
        //--按照网格剖分采样，对地层属性值进行采样,xgrid横向,ygrid纵向
        public float[] SampleToGrid(int xgrid = 100, int ygrid = 100)
        {
            double x, y;
            double xstep = (maxx - minx) / (xgrid - 1);
            double ystep = (maxy - miny) / (ygrid - 1);

            float[] grids = new float[xgrid * ygrid];
            //int intColor = 0;
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + ystep * iy;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + xstep * ix;
                    grids[ix + iy * xgrid] = (float)GetPropertyValue(x, y);
                }
            }
            return grids;
        }
        //--按照网格剖分采样，网格节点转换到空间坐标对
        //地层属性值进行采样,xgrid横向,ygrid纵向
        // IsUniforGrid = true，全局网格，否则每个Polygon采用一个网格
        List<int> sampledPointsColor = new List<int>();
        public List<Vector32> SampleToPoints(int xgrid = 100, int ygrid = 100, bool IsUniformGrid = false)
        {
            sampledPointsColor.Clear();
            if (IsUniformGrid) return SampleToPointsUniform(xgrid, ygrid);
            else return SampleToPointsNotUniform(xgrid, ygrid);
        }
        private List<Vector32> SampleToPointsNotUniform(int xgrid = 40, int ygrid = 40)
        {
            Vector32 p;
            double x, y, xstep, ystep, val;
            List<Vector32> points = new List<Vector32>();

            foreach (Polygon2D obj in tracedGeoObjects.Polygons)
            {
                float[] grids = obj.SampleToGrid(xgrid, ygrid);

                xstep = (obj.maxx - obj.minx) / (xgrid - 1);
                ystep = (obj.maxy - obj.miny) / (ygrid - 1);

                for (int iy = 0; iy < ygrid; iy++)
                {
                    y = obj.miny + ystep * iy;
                    for (int ix = 0; ix < xgrid; ix++)
                    {
                        val = grids[ix + xgrid * iy];
                        if (val != 0)
                        {
                            x = obj.minx + xstep * ix;
                            p = toTracedPoint(x, y, 0, 0);
                            p.v = (float)val;
                            points.Add(p);
                            //sampledPointsColor.Add(ColorRGBA.ParseRGB(obj.fillColor));
                        }
                    }//for (int ix = 0; ix < xgrid; ix++)
                }//for (int iy = 0; iy < ygrid; iy++)
                grids = null;
            }//foreach (Polygon2D obj in tracedGeoObjects)
            return points;
        }

        private List<Vector32> SampleToPointsUniform(int xgrid = 100, int ygrid = 100)
        {
            double x, y;
            double xstep = (maxx - minx) / (xgrid - 1);
            double ystep = (maxy - miny) / (ygrid - 1);
            Vector32 p;
            //int intColor =0;
            List<Vector32> points = new List<Vector32>();
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + ystep * iy;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + xstep * ix;
                    p = toTracedPoint(x, y, 0, 0);
                    p.v = (float)GetPropertyValue(x, y);
                    points.Add(p);
                    // sampledPointsColor.Add(intColor);
                }
            }
            return points;
        }
        /// <summary>
        /// 当前点是否是其他多边形内点
        /// </summary>
        /// <param name="propertyval"></param>        
        /// <param name="ignorelayer">自身多边形不检查</param>
        /// <returns></returns>
        bool IsInsideLayer(double x, double y, double propertyval, int ignorelayer)
        {
            Polygon2D obj;
            for (int i = 0; i != ignorelayer && i < tracedGeoObjects.Count; i++)
            {
                obj = tracedGeoObjects[i];
                if (obj.PropertyValue == propertyval)
                {
                    if (obj.IsPointInsidePoly(x, y))
                        return true;
                }
            }
            return false;
        }
        //对指定的地层属性进行散乱点坐标采样
        public int SampleLayerCoords(ref List<Vector64> points, double propertyValue,
            bool sampleBoudary = true, bool sampleBoudaryInter = true, bool sampleBoudaryOuter = true,
            int xGridOuter = 101, int yGridOuter = 101,
            int xGridInter = 41, int yGridInter = 41,
            int outerExt = 1)
        {

            int sampled = 0;
            Polygon2D obj;
            List<Vector64> lists = new List<Vector64>();
            bool ignore;
            double xlen = maxx - minx;
            double ylen = maxy - miny;
            int xgrid, ygrid;
            double xx = xlen / (xGridOuter - 1);
            double yy = ylen / (yGridOuter - 1);
            double boundaryStep = Math.Sqrt(xx * xx + yy * yy);

            for (int i = 0; i < tracedGeoObjects.Count; i++)
            {
                obj = tracedGeoObjects[i];
                if (obj.PropertyValue != propertyValue) continue;

                xgrid = (int)(xGridOuter * (obj.maxx - obj.minx) / xlen) + 2;
                ygrid = (int)(yGridOuter * (obj.maxy - obj.miny) / ylen) + 2;
                //z = 0 边界点，1边界外点，2边界内点，
                lists.Clear();
                if (sampleBoudary)
                {
                    List<Vector64> samplepoints = obj.SampleBoudary(boundaryStep);
                    Vector64 p;
                    for (int j = 0; j < samplepoints.Count; j++)
                    {
                        p = samplepoints[j];
                        p.v = propertyValue;
                        p.z = i;
                        lists.Add(p);
                    }
                    sampled += samplepoints.Count;
                }
                if (sampleBoudaryOuter) sampled += obj.SampleOuterBoudary(ref lists, xgrid, ygrid, outerExt);

                xgrid = (int)(xGridInter * (obj.maxx - obj.minx) / xlen) + 2;
                ygrid = (int)(yGridInter * (obj.maxy - obj.miny) / ylen) + 2;

                if (sampleBoudaryInter) sampled += obj.SampleInterBoudary(ref lists, xgrid, ygrid);

                //剔除边界外点有可能和其他多边形内点重复点
                foreach (Vector64 p in lists)
                {
                    ignore = false;
                    //如果是边界或边界外点，还需要判别是否是其他多边形内点
                    if (p.z < 2) //边界点及外点
                    {
                        if (IsInsideLayer(p.x, p.y, p.v, i))
                            ignore = true;
                    }
                    if (!ignore)
                    {
                        //边界外点
                        if (p.z == 1) points.Add(new Vector64(p.x, p.y, 0, 0));
                        //边界点或者边界内点
                        else points.Add(new Vector64(p.x, p.y, 0, obj.PropertyValue));
                    }
                }//foreach (Vector64 p1 in lists) 
                lists.Clear();
            }//for ( int i = 0; i < tracedGeoObjects.Count; i++ )

            Vector64 p1, p2;
            for (int i = 0; i < points.Count; i++)
            {
                p1 = points[i];
                p2 = toTracedPoint(p1);
                p2.v = p1.v;
                points[i] = p2;
            }
            RemoveDuplicatedSampled(ref points);
            return points.Count;
        }

        //对所有地层进行采样sample all 
        public List<Vector64> SampleLayerCoords(
                                     int xgrid = 101,//边界采样网格 
                                     int ygrid = 101,
                                     int XResampleExtGrid = 2,//重采样网格扩展 > 1
                                     int YResampleExtGrid = 2,//重采样网格扩展 > 1                                                
                                     float bkvalue = 0)  //背景值（地层外节点值）
        {
            double x, y, z, v;
            double xx = (maxx - minx) / (xgrid - 1);
            double yy = (maxy - miny) / (ygrid - 1);

            int all = xgrid * ygrid;
            double[] grids = new double[all]; //记录地层序号+1            
            for (int i = 0; i < grids.Length; i++) grids[i] = 0; //默认序号，无值点

            sampledGrids.Clear();

            //采样到grids 中，grids[i] 保存tracedGeoObjects索引
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + iy * yy + yy / 2;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + ix * xx + xx / 2;
                    //根据图层顺序进行采样，队尾在最上层
                    for (int i = tracedGeoObjects.Count - 1; i >= 0; i--)
                    {
                        if (tracedGeoObjects[i].IsPointInsidePoly(x, y))
                        {
                            z = (i + 1);
                            grids[ix + iy * xgrid] = z;
                            break;
                        }
                    }
                }//for (int ix = 0; ix < xgrid; ix++)
            }//for (int iy = 0; iy < ygrid; iy++)

            //搜索边界点，保存到boders中
            int id;
            int[] Nears = new int[8];
            bool[] boders = new bool[all];//是否边界点
            for (int i = 0; i < all; i++) boders[i] = false;

            bool[] keep = new bool[all];//是否保留点，默认全部保留
            for (int i = 0; i < all; i++) keep[i] = true;
            string name = "", name1;

            for (id = 0; id < all; id++)
            {
                if (grids[id] > 0) name = tracedGeoObjects[(int)grids[id] - 1].Name.Trim().ToLower();

                Nears[0] = id - xgrid;
                Nears[1] = id + xgrid;
                Nears[2] = id - 1;
                Nears[3] = id + 1;
                Nears[4] = id - xgrid - 1;
                Nears[5] = id + xgrid - 1;
                Nears[6] = id - xgrid + 1;
                Nears[7] = id + xgrid + 1;
                foreach (int idnear in Nears)
                {
                    if (idnear < 0 || idnear >= all)//范围边界点
                    {
                        boders[id] = true;
                        break;
                    }

                    if (grids[idnear] <= 0 && grids[id] > 0)//有值，边界无值
                    {
                        boders[id] = true;
                        break;
                    }
                    else if (grids[idnear] > 0 && grids[id] <= 0)//无值，边界有值
                    {
                        boders[id] = true;
                        break;
                    }
                    //均有值，通过名称来判别是否不同地层
                    else if (grids[idnear] > 0 && grids[id] > 0)
                    {
                        name1 = tracedGeoObjects[(int)grids[idnear] - 1].Name.Trim().ToLower();
                        if (name != name1)
                        {
                            boders[id] = true;
                            break;
                        }
                    }
                }//foreach(int idnear in Nears)
            }//for (int id = 0; id < all; id++)


            //重采样过滤
            int ix0, iy0, id1;
            for (id = 0; id < all; id++)
            {
                if (!keep[id]) continue; //已经是不保留点
                if (boders[id]) continue;//边界点

                iy0 = id / xgrid;
                ix0 = id % xgrid;

                //搜索周围点
                for (int iy = iy0 - YResampleExtGrid; iy <= iy0 + YResampleExtGrid; iy++)
                {
                    if (!keep[id]) break;
                    for (int ix = ix0 - XResampleExtGrid; ix <= ix0 + XResampleExtGrid; ix++)
                    {
                        if (!keep[id]) break;
                        id1 = ix + iy * xgrid;
                        if (id1 < 0 || id1 >= all) continue;
                        if (id == id1) continue;
                        if (id1 < 0 || id1 >= all) continue;
                        if (!keep[id1]) continue; //不保留点
                        if (id == id1) continue;//自身点

                        //不是同一属性点
                        if (grids[id] <= 0 && grids[id1] > 0) continue;
                        if (grids[id] > 0 && grids[id1] <= 0) continue;
                        if (grids[id] > 0 && grids[id1] > 0)
                        {
                            name1 = tracedGeoObjects[(int)grids[id1] - 1].Name.Trim().ToLower();
                            if (name != name1) continue;
                        }
                        keep[id] = false;
                        break;
                    }//
                }//

            }//for (id = 0; id < all; id++)

            //输出网格
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + iy * yy + yy / 2;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + ix * xx + xx / 2;
                    id = ix + iy * xgrid;
                    if (keep[id])
                    {
                        z = grids[id] - 1;
                        if (z < 0) v = bkvalue;
                        else v = tracedGeoObjects[(int)z].PropertyValue;
                        sampledGrids.Add(new Vector64(x, y, z, v));
                    }
                }
            }

            grids = null;
            boders = null;
            keep = null;

            return sampledGrids;
        }

        //对指定的某些地层（names）进行采样,均匀采样
        public List<Vector64> SampleLayerCoords(List<string> names,
                                                int xgrid = 101,//网格采样数 
                                                int ygrid = 101,//网格采样数                                                
                                                bool resetLayer = false, //重置地层值
                                                float bkvalue = 0,  //背景值（地层外节点值）
                                                float resetValue = 1)//地层值重置为
        {
            double x, y, z, v;
            double xx = (maxx - minx) / (xgrid - 1);
            double yy = (maxy - miny) / (ygrid - 1);

            int all = xgrid * ygrid;
            double[] grids = new double[all];

            for (int i = 0; i < grids.Length; i++) grids[i] = bkvalue;

            sampledGrids.Clear();

            double val;
            //将剖面数据采样到网格grids 中，
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + iy * yy + yy / 2;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + ix * xx + xx / 2;
                    Vector64 p = new Vector64(x, y, -1, bkvalue);
                    for (int k = tracedGeoObjects.Count - 1; k >= 0; k--)
                    {
                        Polygon2D poly = tracedGeoObjects[k];
                        if (names.Contains(poly.Name.ToLower()) &&
                             poly.IsPointInsidePoly(x, y))
                        {
                            val = poly.PropertyValue;
                            if (resetLayer) val = resetValue;
                            grids[ix + iy * xgrid] = val;
                            p.z = k;
                            p.v = val;
                            break;
                        }
                    }

                    sampledGrids.Add(p);

                }//for (int ix = 0; ix < xgrid; ix++)
            }//for (int iy = 0; iy < ygrid; iy++)           


            return sampledGrids;
        }

        //对指定的某些地层（names）进行采样 
        public List<Vector64> SampleLayerCoords(List<string> names,
                                                int xgrid = 101,//网格采样数 
                                                int ygrid = 101,//网格采样数
                                                int XResampleExtGrid = 2,//重采样网格扩展 > 1
                                                int YResampleExtGrid = 2,//重采样网格扩展 > 1
                                                bool resetLayer = false, //重置地层值
                                                float bkvalue = 0,  //背景值（地层外节点值）
                                                float resetValue = 10)//地层值重置为
        {
            double x, y, z, v;
            double xx = (maxx - minx) / (xgrid - 1);
            double yy = (maxy - miny) / (ygrid - 1);

            int all = xgrid * ygrid;
            double[] grids = new double[all];

            for (int i = 0; i < grids.Length; i++) grids[i] = bkvalue;

            sampledGrids.Clear();

            //将剖面数据采样到网格grids 中，
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + iy * yy + yy / 2;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + ix * xx + xx / 2;
                    for (int k = tracedGeoObjects.Count - 1; k >= 0; k--)
                    {
                        Polygon2D poly = tracedGeoObjects[k];
                        if (names.Contains(poly.Name.ToLower()) &&
                             poly.IsPointInsidePoly(x, y))
                        {
                            if (resetLayer)
                                grids[ix + iy * xgrid] = resetValue;
                            else grids[ix + iy * xgrid] = poly.PropertyValue;
                            break;
                        }
                    }
                }//for (int ix = 0; ix < xgrid; ix++)
            }//for (int iy = 0; iy < ygrid; iy++)

            /////////////////////////////////
            //搜索边界点，保存到boders中
            int id;
            int[] Nears = new int[8];
            bool[] boders = new bool[all];//是否边界点
            for (int i = 0; i < all; i++) boders[i] = false;
            for (id = 0; id < all; id++)
            {
                //8个方向搜索
                Nears[0] = id - xgrid;
                Nears[1] = id + xgrid;
                Nears[2] = id - 1;
                Nears[3] = id + 1;
                Nears[4] = id - xgrid - 1;
                Nears[5] = id + xgrid - 1;
                Nears[6] = id - xgrid + 1;
                Nears[7] = id + xgrid + 1;
                foreach (int idnear in Nears)
                {
                    if (idnear < 0 || idnear >= all)//数据边界点
                    {
                        boders[id] = true;
                        break;
                    }
                    else if (grids[idnear] != grids[id])//周围存在值不一致的点
                    {
                        boders[id] = true;
                        break;
                    }
                }
            }//for (int id = 0; id < all; id++)


            bool[] keep = new bool[all];//是否要保留点
            for (int i = 0; i < all; i++) keep[i] = false;
            //网格重采样
            for (int iy = 0; iy < ygrid; iy += YResampleExtGrid)
            {
                for (int ix = 0; ix < xgrid; ix += XResampleExtGrid)
                {
                    id = ix + iy * xgrid;
                    keep[id] = true;
                }
            }

            //输出
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + iy * yy + yy / 2;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + ix * xx + xx / 2;
                    id = ix + iy * xgrid;
                    if (keep[id] || boders[id])
                    {
                        z = 0;
                        v = grids[id];
                        sampledGrids.Add(new Vector64(x, y, z, v));
                    }
                }
            }

            grids = null;
            boders = null;
            keep = null;

            return sampledGrids;
        }

        //对指定的某些地层（names）进行采样 
        public List<Vector64> SampleBoudary(List<string> names, double step,
                                            bool resetLayer = false, //重置地层值                                            
                                            float resetValue = 10)
        {
            double val;
            Vector64 p;
            for (int k = tracedGeoObjects.Count - 1; k >= 0; k--)
            {
                Polygon2D poly = tracedGeoObjects[k];
                if (names.Contains(poly.Name.ToLower()))
                {
                    if (resetLayer) val = resetValue;
                    else val = poly.PropertyValue;

                    List<Vector64> lists = poly.SampleBoudary(step);
                    for (int i = 0; i < lists.Count; i++)
                    {
                        p = lists[i];
                        p.z = k;    //polygon index
                        p.v = val;  //val
                        sampledGrids.Add(p);
                    }
                    lists.Clear();

                }
            }
            return sampledGrids;
        }

        //对网格进行重采样
        public void ResampleGrids(int xgrid, int ygrid, int stepx, int stepy)
        {
            int all = xgrid * ygrid;

            int id;

            int[] Nears = new int[8];

            bool[] boders = new bool[all];//是否边界点
            for (int i = 0; i < all; i++) boders[i] = false;

            bool[] keep = new bool[all];//是否保留点，默认不保留
            for (int i = 0; i < all; i++) keep[i] = false;

            for (id = 0; id < all; id++)
            {
                Nears[0] = id - xgrid;
                Nears[1] = id + xgrid;
                Nears[2] = id - 1;
                Nears[3] = id + 1;
                Nears[4] = id - xgrid - 1;
                Nears[5] = id + xgrid - 1;
                Nears[6] = id - xgrid + 1;
                Nears[7] = id + xgrid + 1;

                foreach (int idnear in Nears)
                {
                    if (idnear < 0 || idnear >= all)//范围边界点
                    {
                        boders[id] = true;
                        keep[id] = true;
                        break;
                    }
                    if (sampledGrids[idnear].V != sampledGrids[id].V) //边界点
                    {
                        boders[id] = true;
                        keep[id] = true;
                        break;
                    }
                }//foreach(int idnear in Nears)
            }//for (int id = 0; id < all; id++)


            //重采样过滤
            for (int iy = 0; iy < ygrid; iy += stepy)
                for (int ix = 0; ix < xgrid; ix += stepx)
                {
                    id = ix + iy * xgrid;
                    keep[id] = true;
                }//

            List<Vector64> sampled = new List<Vector64>();

            //输出网格
            double x, y, z, v;
            for (int iy = 0; iy < ygrid; iy++)
            {
                for (int ix = 0; ix < xgrid; ix++)
                {
                    id = ix + iy * xgrid;
                    if (keep[id])
                    {
                        sampled.Add(sampledGrids[id]);
                    }
                }
            }

            sampledGrids.Clear();
            sampledGrids = sampled;

            boders = null;
            keep = null;
        }
        /// <summary>
        /// 将可能重复采样的点过滤，优先删除边界外点
        /// </summary>
        /// <param name="points"></param>
        /// <param name="zerobase"></param>
        /// <returns></returns>
        int RemoveDuplicatedSampled(ref List<Vector64> points, double zerobase = 0.0001)
        {
            if (points.Count < 2) return 0;

            double dist;
            Vector64 p, p1, p2;
            double maxlen = 0;
            double x1 = 0, y1 = 0, z1 = 0;
            double x2 = 0, y2 = 0, z2 = 0;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (i == 0)
                {
                    x1 = x2 = p.x;
                    y1 = y2 = p.y;
                    z1 = z2 = p.z;
                }
                else
                {
                    if (p.x < x1) x1 = p.x;
                    if (p.y < y1) y1 = p.y;
                    if (p.z < z1) z1 = p.z;
                    if (p.x > x2) x2 = p.x;
                    if (p.y > y2) y2 = p.y;
                    if (p.z > z2) z2 = p.z;
                }
            }
            maxlen = x2 - x1;
            if (y2 - y1 > maxlen) maxlen = y2 - y1;
            if (z2 - z1 > maxlen) maxlen = z2 - z1;

            double err = maxlen * zerobase;

            bool[] del = new bool[points.Count];
            for (int i = 0; i < points.Count; i++) del[i] = false;

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    p1 = points[i];
                    p2 = points[j];
                    dist = Math.Abs(p1.x - p2.x) +
                           Math.Abs(p1.y - p2.y) +
                           Math.Abs(p1.z - p2.z);
                    if (dist <= err)
                    {
                        if (p2.v == 0) del[j] = true;
                        else del[i] = true;
                    }
                }
            }
            int num = 0;
            for (int i = points.Count - 1; i >= 0; i--)
            {
                if (del[i]) { points.RemoveAt(i); num++; }
            }
            del = null;
            return num;
        }           

        /// <summary>
        /// 将指定的地层属性数据输出为散乱点XYZ格式
        /// </summary>
        /// <param name="filename">文件名，如已存在则追加到后</param>
        /// <param name="selectedIndices">索引数组，null 输出全部</param>
        /// <param name="filterValue1">过滤值v1 <v1 过滤 v1=v2忽略</param>
        /// <param name="filterValue2">过滤值v2 >v2 过滤 v1=v2忽略</param>
        /// <returns></returns>
        public bool ExportLayerPropertyToXYZ(string filename, List<int>selectedIndices = null,double filterValue1 = 0, double filterValue2 = 0)
        {
            try
            {
                bool appended = false;
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists && fi.Length > 0) appended = true;

                FileStream fs;
                if (!fi.Exists) fs = new FileStream(filename, FileMode.CreateNew);
                else fs = new FileStream(filename, FileMode.Append);

                StreamWriter wr = new StreamWriter(fs);

                string line = "x,   y,  z,  value";
                if ( !appended ) wr.WriteLine(line);

                Vector64 p1;
                if (selectedIndices == null)
                {
                    foreach (Vector64 p in sampledGrids)
                    {
                        p1 = toTracedPoint(p);
                        if (filterValue2 > filterValue1)
                        {
                            if (p1.V < filterValue1 || p1.V > filterValue2)
                                continue;
                        }

                        line = p1.x + "," + p1.y + "," + p1.z + "," + p1.v;
                        wr.WriteLine(line);
                    }
                }
                else
                {
                    foreach (int i in selectedIndices)
                    {
                        p1 = toTracedPoint(sampledGrids[i]);
                        if (filterValue2 > filterValue1)
                        {
                            if (p1.V < filterValue1 || p1.V > filterValue2)
                                continue;
                        }

                        line = p1.x + "," + p1.y + "," + p1.z + "," + p1.v;
                        wr.WriteLine(line);
                    }
                }

                wr.Close();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }//public bool ExportLayerPropertyToXYZ   
    }

    public enum ClockDirection
    {
        /// <summary>
        /// 无.可能是不可计算的图形，比如多点共线
        /// </summary>
        None = 0,
        /// <summary>
        /// 顺时针方向
        /// </summary>
        Clockwise = 1,
        /// <summary>
        /// 逆时针方向
        /// </summary>
        Counterclockwise = 2
    }
    public enum PolygonType
    {
        /// <summary>
        /// 无.不可计算的多边形(比如多点共线)
        /// </summary>
        None = 0,
        /// <summary>
        /// 凸多边形
        /// </summary>
        Convex = 1,
        /// <summary>
        /// 凹多边形
        /// </summary>
        Concave = 2
    }
    public class C2DPolygons : C3DObjectBase
    {
        [CategoryAttribute("Display"), DisplayNameAttribute("Uniform Style")]
        public bool IsUniformStyle { get; set; } = false;

        [CategoryAttribute("Display"), DisplayNameAttribute("Filled")]
        public bool IsFill { get; set; } = false;

        [CategoryAttribute("Display"), DisplayNameAttribute("Fill Color")]
        public Color fillColor { get; set; } = Color.White;

        [CategoryAttribute("Display"), DisplayNameAttribute("Draw Line")]
        public bool IsDrawLine { get; set; } = true;

        [CategoryAttribute("Display"), DisplayNameAttribute("Line Color")]
        public Color lineColor { get; set; } = Color.Black;

        [CategoryAttribute("Display"), DisplayNameAttribute("Line Width")]
        public float lineWidth { get; set; } = 1.0f;

        [CategoryAttribute("Properties"), DisplayNameAttribute("Property Value")]
        public double PropertyValue { get; set; } = 0; //property value
        
        public List<Polygon2D> Polygons = new List<Polygon2D>();
        public void Add(Polygon2D poly) { Polygons.Add(poly); }
        public override void Clear() { Polygons.Clear(); }
        public int Count { get { return Polygons.Count; } }
        public override bool Visible
        {
            get { return _Visible; }
            set
            {
                _Visible = value;
                foreach(Polygon2D poly in Polygons)
                {
                    poly.Visible = value;
                    poly.RenderMode = RenderingUpdateMode.Visible;
                }
                UpdateNeeded = true;
                RenderMode = RenderingUpdateMode.Visible;
            }
        }

        public Polygon2D this[int index] 
        { 
            get 
            { 
                return Polygons[index]; 
            }
            set
            {
                if( Count > index )
                {
                    Polygons[index] = value;
                }
            }
        }
        public void RemoveAt(int index)
        {
            Polygons.RemoveAt(index);
        }
        public override bool Remove(C3DObjectBase obj)
        {
            return Polygons.Remove((Polygon2D)obj);
        }
        public void Insert(int index,Polygon2D poly)
        {
            Polygons.Insert(index, poly);
        }
        public C2DPolygons(string name = "untitled")
        {
            Name = name;
            type = ShapeEnum.Polygon2Ds;
        }

        public override void UpdateRange()
        {
            for(int i=0;i< Polygons.Count; i++)
            {
                Polygon2D poly = Polygons[i];
                if (i == 0) 
                {
                    minx = poly.minx;
                    miny = poly.miny;
                    minz = poly.minz;
                    minv = poly.minv;
                    maxx = poly.maxx;
                    maxy = poly.maxy;
                    maxz = poly.maxz;
                    maxv = poly.maxv;
                }
                else
                {
                    if (poly.minx < minx) minx = poly.minx;
                    if (poly.miny < miny) miny = poly.miny;
                    if (poly.minz < minz) minz = poly.minz;
                    if (poly.minv < minv) minv = poly.minv;
                    if (poly.maxx > maxx) maxx = poly.maxx;
                    if (poly.maxy > maxy) maxy = poly.maxy;
                    if (poly.maxz > maxz) maxz = poly.maxz;
                    if (poly.maxv > maxv) maxv = poly.maxv;
                }
            }
        }

        public override bool SaveAs(BinaryWriter br)
        {
            try 
            {
                SaveObjHeader(br);
                br.Write(IsUniformStyle);
                br.Write(IsFill);
                br.Write(fillColor.ToArgb());
                br.Write(IsDrawLine);
                br.Write(lineColor.ToArgb());
                br.Write(lineWidth);
                br.Write(PropertyValue);
                br.Write(Polygons.Count);
                foreach (Polygon2D poly in Polygons)
                {
                    poly.SaveAs(br);
                }
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }            
        }
        public override bool LoadFrom(BinaryReader br)
        {
            try 
            {
                Polygons.Clear();
                LoadObjHeader(br);
                IsUniformStyle = br.ReadBoolean();
                IsFill = br.ReadBoolean();
                fillColor = Color.FromArgb(br.ReadInt32());
                IsDrawLine = br.ReadBoolean();
                lineColor = Color.FromArgb(br.ReadInt32());
                lineWidth = br.ReadSingle();
                PropertyValue = br.ReadDouble();
                int n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    Polygon2D poly = new Polygon2D();
                    if (!poly.LoadFrom(br))
                    {
                        errMessage = poly.errMessage;
                        return false; 
                    }
                    Polygons.Add(poly);
                }
                UpdateRange();
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }            
        }
    }

    public class Polygon2D : C3DObjectBase
    {
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Is Closed")]
        public bool IsClosed { get; set; } = true;
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Fill Color")]
        public Color fillColor { get; set; } = Color.White;
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Line Color")]
        public Color lineColor { get; set; } = Color.Black;
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Property Value")]
        public double PropertyValue { get; set; } = 0; //property value
        public string headerLine = "POLYGON 2D 1200";

        public PolygonSlicer polygonSlicer = null;

        public bool IsValid
        {
            get
            {
                if (IsClosed) return points.Count > 2;
                else return points.Count > 1;
            }
        }
        //z == 0        
        public List<Vector64> points = new List<Vector64>();
        //切面轴，默认垂向切面z
        public AxisEnum axis = AxisEnum.zAxis;

        [CategoryAttribute("Display"), DisplayNameAttribute("Line Width")]
        public float lineWidth { get; set; } = 1.0f;        

        [CategoryAttribute("Display"), DisplayNameAttribute("Dash Style")]
        public DashStyle dashStyle { get; set; } = DashStyle.Solid;

        [CategoryAttribute("Display"), DisplayNameAttribute("Is Filled")]
        public bool IsFill { get; set; } = true;

        public Vector64 this[int index]
        {
            get
            {
                return points[index];
            }
            set
            {
                points[index] = value;
            }
        }
        public Polygon2D(string name = "untitled")
        {
            Name = name;
            type = ShapeEnum.Polygon2D;
        }
        public Polygon2D(Vector64[] array)
        {
            type = ShapeEnum.Polygon2D;
            for (int i = 0; i < array.Length; i++)
                points.Add(array[i]);
            UpdateRange();
        }
        public Polygon2D(Vector32[] array)
        {
            type = ShapeEnum.Polygon2D;
            for (int i = 0; i < array.Length; i++)
                points.Add(array[i].toVector64());
            UpdateRange();
        }
        public Polygon2D(List<Vector64> lists)
        {
            type = ShapeEnum.Polygon2D;
            for (int i = 0; i < lists.Count; i++)
                points.Add(lists[i]);
            UpdateRange();
        }
        public Polygon2D(List<Vector32> lists)
        {
            type = ShapeEnum.Polygon2D;
            for (int i = 0; i < lists.Count; i++)
                points.Add(lists[i].toVector64());
            UpdateRange();
        }
        public int Count { get { return points.Count; } }
        public Polygon2D Copy()
        {
            Polygon2D poly = new Polygon2D(points);
            poly.CopyHeaderFrom(this);

            poly.IsClosed = IsClosed;
            poly.IsFill = IsFill;
            poly.fillColor = fillColor;
            poly.lineColor = lineColor;
            poly.PropertyValue = PropertyValue;
            poly.axis = axis;
            poly.lineWidth = lineWidth;
            if(triangledObject != null) poly.triangledObject = triangledObject.Copy();
            return poly;
        }
        /// <summary>
        /// Project 3D Polygon to 2D Plane XOY,XOZ,YOZ
        /// </summary>
        /// <returns></returns>
        public Polygon2D toProjectedPolygon()
        {
            Polygon2D poly = new Polygon2D(Name+"_Projected");

            Vector64 p0 = GetCenter64(); //中心点
            double xx = maxx - minx;
            double yy = maxy - miny;
            double zz = maxz - minz;

            Vector64 p1 = new Vector64();

            //  |Z  /Y
            //  |  /
            //  | /---------->X
            poly.polygonSlicer = new PolygonSlicer();
            if (zz <= xx && zz <= yy) //XOY
            {
                for (int i = 0; i < points.Count; i++)
                {
                    p1 = new Vector64(points[i].X, points[i].Y, 0, 0);
                    poly.points.Add(p1);
                }
                poly.polygonSlicer.axis = AxisEnum.yAxis;
            }
            else if (yy <= xx && yy <= zz) //XOZ
            {
                for (int i = 0; i < points.Count; i++)
                {
                    p1 = new Vector64(points[i].X, points[i].Z, 0, 0);
                    poly.points.Add(p1);
                }
                poly.polygonSlicer.axis = AxisEnum.zAxis;
            }
            else //YOZ
            {
                for (int i = 0; i < points.Count; i++)
                {
                    p1 = new Vector64(points[i].Y, points[i].Z, 0, 0);
                    poly.points.Add(p1);
                }
                poly.polygonSlicer.axis = AxisEnum.zAxis;
            }

            poly.UpdateRange();
            double x1 = 0,x2=0, y1=0, y2=0;
            int n1=0, n2=0, n3=0, n4=0;
            for( int i=0;i < poly.points.Count; i++ )
            {
                if (i == 0) 
                { 
                    x1 = x2 = poly.points[i].X; 
                    y1 = y2 = poly.points[i].Y;
                    n1 = n2 = i;
                }
                else
                {
                    if(poly.points[i].X < x1)
                    {
                        x1 = poly.points[i].X;
                        n1 = i;
                    }
                    if (poly.points[i].X > x2)
                    {
                        x2 = poly.points[i].X;
                        n2 = i;
                    }
                }
            }
            double dy = 0;
            for (int i = 0; i < poly.points.Count; i++)
            {
                if (poly.points[i].X == x1 || poly.points[i].X == x2) continue;
                if( Math.Abs(poly.points[i].Y - poly.points[n1].Y) > dy )
                {
                    dy = Math.Abs(y1 - poly.points[n1].Y);
                    n3 = i;
                }
                if ( Math.Abs(poly.points[i].Y - poly.points[n2].Y) > dy )
                {
                    dy = Math.Abs(y1 - poly.points[n2].Y);
                    n3 = i;
                }                
            }

            if (n1 >= 0) poly.polygonSlicer.AddLocationPoint(poly.points[n1], points[n1]);
            if (n3 >= 0) poly.polygonSlicer.AddLocationPoint(poly.points[n3], points[n3]);
            if (n2 >= 0) poly.polygonSlicer.AddLocationPoint(poly.points[n2], points[n2]);
            
            poly.UpdateRange();
            poly.polygonSlicer.UpdateTraced();
            return poly;
        }

        public bool IsPoly2D 
        {
            get 
            {
                foreach(Vector64 p in points)
                {
                    if (p.Z != 0) return false;
                }
                return true;
            }
        }
        public Vector64 toTracedPoint(Vector64 p)
        {
            if( polygonSlicer != null )
            {
                return polygonSlicer.toTracedPoint(p);
            }
            else  return p;
        }
        /// <summary>
        /// Convert to a traced line
        /// </summary>
        /// <returns></returns>
        public C3DLine to3DLine()
        {
            C3DLine line = new C3DLine(Name);
            line.Closed = IsClosed;
            line.lineStyle.Width = lineWidth;
            line.lineStyle.Color = lineColor;
            foreach (Vector64 p in points)
            {
                line.AddPoint(toTracedPoint(p));
            }
            line.UpdateRange();
            return line;
        }
        /// <summary>
        /// Project to a plane
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public C3DLine to3DLine( planEnum plan )
        {
            C3DLine line = new C3DLine(Name);
            line.Closed = IsClosed;
            line.lineStyle.Width = lineWidth;
            line.lineStyle.Color = lineColor;
            foreach( Vector64 p in points )
            {
                if( plan == planEnum.XOY )line.AddPoint(p);
                else if (plan == planEnum.XOZ)line.AddPoint(p.X,0,p.Y,p.V);
                else if (plan == planEnum.YOZ)line.AddPoint(0, p.X, p.Y, p.V);
            }
            line.UpdateRange();
            return line;
        }
        /// <summary>
        /// 多边形或曲线平滑
        /// selfsmooth = true,平滑自身
        /// </summary>
        public Polygon2D Smooth(bool selfsmooth = false)
        {
            if (points.Count < 2) return null;

            Polygon2D poly = this.Copy();
            if (IsClosed)
            {
                BezierSmooth bz = new BezierSmooth();
                bz.AddPoint(points);
                poly.points = bz.Smooth().points;
                if (selfsmooth) points = poly.points;
            }
            else
            {
                CubicSpline spline = new CubicSpline();
                poly.points = spline.CreateSpline(points);
                if (selfsmooth) points = poly.points;
            }
            return poly;
        }
        
        public override vec2 GetTextureCoord(Vector32 p)
        {
            vec2 tex = new vec2(-1,-1);
            tex.x = (float)( (p.x - minx) / (maxx - minx) );
            tex.y = (float)( (p.y - miny) / (maxy - miny) );
            return tex;
        }
        /// <summary>
        /// transform polygon to an unclosed poly
        /// </summary>
        /// <param name="index">-1 is default ,otherwise reset the point at index as the start point</param>
        /// <returns></returns>
        public bool ToUnClosed(int index = -1)
        {
            IsClosed = false;

            if (index > -1)
            {
                List<Vector64> lists = new List<Vector64>();
                for (int i = index; i < Count; i++) lists.Add(points[i]);
                for (int i = 0; i < index; i++) lists.Add(points[i]);
                points.Clear();
                for (int i = 0; i < lists.Count; i++) points.Add(lists[i]);
                lists.Clear();
            }

            return true;
        }

        public void InverseOrder()
        {
            List<Vector64> _points = new List<Vector64>();
            for(int i = Count-1; i >=0; i-- )
            {
                _points.Add( points[i] );
            }
            points.Clear();
            points = _points;
        }
        /// <summary>
        /// 在XOY平面内剖分
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public TriangleObj triangledObject = null;
        public TriangleObj Triangulate(bool update = true)
        {
            if ( triangledObject != null && update == false) 
                return triangledObject;

            TriangleObj triangles = new TriangleObj();

            //这里的三角剖分，多边形必须是2D的
            Polygon2D poly2d = this;
            if ( !IsPoly2D ) poly2d = toProjectedPolygon();

            Poly2Tri.Polygon poly = new Polygon(poly2d);
            P2T.Triangulate(poly);

            Vector32 p1 = new Vector32();
            Vector32 p2 = new Vector32();
            Vector32 p3 = new Vector32();
            float z = (float)points[0].Z;
            float v = (float)points[0].V;
            int n = 0;
            foreach (DelaunayTriangle tri in poly.Triangles)
            {
                p1 = new Vector32(tri.Points[0].Xf, tri.Points[0].Yf, z, v);
                p2 = new Vector32(tri.Points[1].Xf, tri.Points[1].Yf, z, v);
                p3 = new Vector32(tri.Points[2].Xf, tri.Points[2].Yf, z, v);

                if ( poly2d.polygonSlicer != null ) 
                {
                    p1 = poly2d.polygonSlicer.toTracedPoint(new Vector64(p1.X, p1.Y,0));
                    p2 = poly2d.polygonSlicer.toTracedPoint(new Vector64(p2.X, p2.Y, 0));
                    p3 = poly2d.polygonSlicer.toTracedPoint(new Vector64(p3.X, p3.Y, 0));
                }
                triangles.AddPoint(p1);
                triangles.AddPoint(p2);
                triangles.AddPoint(p3);
                n = triangles.points.Count;
                triangles.AddTriangleIndex(n - 3, n - 2, n - 1);
            }
            poly.Clear();
            //poly2d.Clear();

            triangles.UpdateRange();

            for(int i=0; i < triangles.points.Count; i++)
            {
                triangles.AddTexture( GetTextureCoord(triangles.points[i]) );
            }
            
            triangledObject = triangles;
            return triangledObject;
        }
       
        /// <summary>
        /// 简单凸多边形三角剖分，在XOY平面内剖分
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        static public TriangleObj ConvexPolyTriangulateSimple(Polygon2D poly)
        {
            TriangleObj obj = new TriangleObj();
            int n = poly.Count;
            Vector64 p0 = poly.GetCenter64();
            Vector64 p;
            obj.AddPoint(p0);
            for (int i = 0; i < n; i++)
            {
                p = poly[i];
                obj.AddPoint(p);
            }
            for (int i = 0; i < n; i++)
            {
                if (i < n - 1) obj.AddTriangleIndex(0, i + 1, i + 2);
                else obj.AddTriangleIndex(0, i + 1, 1);
            }
            return obj;
        }
        /// <summary>
        /// 递归法-凹多边形拆分为多个凸多边形,--该方法不好，有bug
        /// 要求多边形为顺时针方向（Y坐标向上）
        /// </summary>
        List<Polygon2D> polyList = new List<Polygon2D>();//拆分结果
        public List<Polygon2D>PolyDividing()
        {
            polyList.Clear();
            ClockDirection di = CalculateClockDirection();//Y轴向上
            //转换成顺时针方向（Y轴向上），逆时针方向（Y轴向下）
            if (di == ClockDirection.Counterclockwise) InverseOrder();
            Divide(points);
            return polyList;
        }        
        private void Divide( List<Vector64> tmep )
        {
            //如果是凸多边形，不需要往下走了,存入polyList数组
            if ( IsConvex(tmep) )
            {
                polyList.Add(new Polygon2D(tmep));
                return;
            }

            List<Vector64> poly = new List<Vector64>(tmep);
            int count = poly.Count;
            for (int i = 0; i < count; i++)
            {
                poly.Add(poly[i]);
            }

            bool flag = false;
            int NumP1 = 0, NumP2 = 0;
            List<Vector64> poly2 = new List<Vector64>();
            for (int i = 0; i < poly.Count / 2; i++)
            {
                Vector64 now = new Vector64(poly[i + 1].x - poly[i].x, poly[i + 1].y - poly[i].y, 0);
                Vector64 next = new Vector64(poly[i + 2].x - poly[i].x, poly[i + 2].y - poly[i].y, 0);
                double corss = now.x * next.y - now.y * next.x;

                if (corss > 0)
                {
                    //Console.WriteLine("i is {0}", i);
                    for (int j = i + 3; j < poly.Count; j++)
                    {
                        // 找到余下的第一个不在x轴下方的顶点
                        Vector64 v = new Vector64(poly[j].x - poly[i].x, poly[j].y - poly[i].y, 0);
                        if (now.x * v.y - now.y * v.x < 0)
                        {
                            // 该点即为分割点。找到分割点即跳出循环
                            flag = true;
                            NumP1 = i + 1;
                            NumP2 = j;
                            break;
                        }
                    }//for (int j = i + 3; j < poly.Count; j++)
                    if (flag) break;
                }//if (corss > 0)
            }//for (int i = 0; i < poly.Count / 2; i++)
            // 此时分割多边形的两个点分别为poly[i + 1]和poly[j]
            Vector64 p1 = new Vector64(poly[NumP1].x, poly[NumP1].y, 0);
            Vector64 p2 = new Vector64(poly[NumP2].x, poly[NumP2].y, 0);

            for (int i = NumP1; i < NumP2 + 1; i++) poly2.Add(poly[i]);

            List<Vector64> poly1 = new List<Vector64>(tmep);
            for (int i = 1; i < poly2.Count - 1; i++)
            {
                if (poly1.Contains(poly2[i]))
                    poly1.Remove(poly2[i]);
            }
            Divide(poly1);
            Divide(poly2);
        }

        /// <summary>
        /// 判断多边形是顺时针还是逆时针.
        /// </summary>
        /// <param name="points">所有的点</param>
        /// <param name="isYAxixToDown">true:Y轴向下为正(屏幕坐标系),false:Y轴向上为正(一般的坐标系)</param>
        /// <returns></returns>
        public ClockDirection CalculateClockDirection(bool isYAxixToDown = false)
        {
            int i, j, k;
            int count = 0;
            double z;
            int yTrans = isYAxixToDown ? (-1) : (1);
            if (points.Count < 3)
            {
                return ClockDirection.None;
            }

            int n = points.Count;
            for (i = 0; i < n; i++)
            {
                j = (i + 1) % n;
                k = (i + 2) % n;
                z = (points[j].X - points[i].X) * (points[k].Y * yTrans - points[j].Y * yTrans);
                z -= (points[j].Y * yTrans - points[i].Y * yTrans) * (points[k].X - points[j].X);
                if (z < 0)
                {
                    count--;
                }
                else if (z > 0)
                {
                    count++;
                }
            }
            if (count > 0)
            {
                return (ClockDirection.Counterclockwise);
            }
            else if (count < 0)
            {
                return (ClockDirection.Clockwise);
            }
            else
            {
                return (ClockDirection.None);
            }
        }
        
        //是否凸多边形
        public bool IsConvexPoly
        {
            get { return IsConvex(points); }
        }
        //是否凸多边形
        public bool IsConvex(List<Vector64> pp)
        {
            if (pp.Count <= 3) return true;
            for (int i = 0; i < pp.Count; i++)
            {
                double x1, x2, y1, y2;
                //向量1
                if (i == 0)
                {
                    x1 = pp[pp.Count - 1].x - pp[i].x;
                    y1 = pp[pp.Count - 1].y - pp[i].y;
                }
                else
                {
                    x1 = pp[i - 1].x - pp[i].x;
                    y1 = pp[i - 1].y - pp[i].y;
                }
                //向量2
                if (i == pp.Count - 1)
                {
                    x2 = pp[0].x - pp[i].x;
                    y2 = pp[0].y - pp[i].y;
                }
                else
                {
                    x2 = pp[i + 1].x - pp[i].x;
                    y2 = pp[i + 1].y - pp[i].y;
                }
                double corss = x1 * y2 - y1 * x2;
                if (corss < 0) return false;
            }
            return true;
        }

        public override bool SaveAs(BinaryWriter br)
        {
            SaveObjHeader(br);
            br.Write(points.Count);
            foreach (Vector32 p in points)
            {
                br.Write(p.x);
                br.Write(p.y);
                br.Write(p.z);
            }
            br.Write(IsClosed);
            br.Write(IsFill);
            br.Write(ColorRGBA.ParseRGB(fillColor));
            br.Write(ColorRGBA.ParseRGB(lineColor));
            br.Write(PropertyValue);
            br.Write((int)axis);
            br.Write(lineWidth);
            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {
            points.Clear();
            LoadObjHeader(br);
            int n = br.ReadInt32();
            float x, y, z;
            for (int i = 0; i < n; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                Add(x, y, z);
            }

            IsClosed = br.ReadBoolean();
            IsFill = br.ReadBoolean();

            fillColor = ColorRGBA.RGB(br.ReadInt32());
            lineColor = ColorRGBA.RGB(br.ReadInt32());

            PropertyValue = br.ReadDouble();
            axis = (AxisEnum)br.ReadInt32();
            lineWidth = br.ReadSingle();

            return true;
        }

        public override bool SaveAs(string path)
        {
            BinaryWriter br;
            try
            {
                br = new BinaryWriter(new FileStream(path, FileMode.Create));
                bool ret = SaveAs(br);
                br.Close();
                return ret;
            }
            catch (IOException e)
            {
                return false;
            }
        }
        public override bool LoadFrom(string path)
        {
            BinaryReader br;
            try
            {
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                bool ret = LoadFrom(br);
                br.Close();
                return ret;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        public override bool ExportData(string filename)
        {
            StreamWriter wr = new StreamWriter(new FileStream(filename, FileMode.Create));
            string str;
            headerLine = "POLYGON 2D 1200";
            wr.WriteLine(headerLine);

            str = "[HEADER]";
            wr.WriteLine(str);
            str = "Name = " + Name;
            wr.WriteLine(str);
            str = "PropertyValue = " + PropertyValue;
            wr.WriteLine(str);
            str = "Closed = " + IsClosed;
            wr.WriteLine(str);
            str = "IsFilled = " + IsFill;
            wr.WriteLine(str);            
            str = "Axis = " + axis;
            wr.WriteLine(str);

            str = "[Style]";
            wr.WriteLine(str);
            str = "FillColor = " + fillColor.ToArgb();
            wr.WriteLine(str);
            str = "LineColor = " + lineColor.ToArgb();
            wr.WriteLine(str);
            str = "LineWidth = " + lineWidth;
            wr.WriteLine(str);
            str = "DashStyle = " + dashStyle;
            wr.WriteLine(str);
            
            str = "[POINTS]";
            wr.WriteLine(str);
            str = "Count = " + points.Count;
            wr.WriteLine(str);            

            foreach( Vector64 p in points )
            {
                if(polygonSlicer == null ) wr.WriteLine(p.toString(4));
                else wr.WriteLine( polygonSlicer.toTracedPoint(p).toString(4) );
            }
            
            wr.Close();
            
            return true;
        }
        public override bool ImportData(string filename)
        {
            try
            {
                StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
                Name = Path.GetFileName(filename);

                Clear();
                
                if (AscIIProfile.ReadLine(sr) != headerLine)
                {
                    errMessage = "not a valid polygon file.";
                    sr.Close();
                    return false;
                }

                if (!AscIIProfile.SeekSection("[HEADER]", ref sr))
                {
                    errMessage = "not a valid polygon file.";
                    sr.Close();
                    return false;
                }

               
                Name = AscIIProfile.ReadStringValue(sr, "Name");
                PropertyValue = AscIIProfile.ReadDoubleValue(sr, "PropertyValue");
                IsClosed = AscIIProfile.ReadBoolValue(sr, "Closed");
                IsFill = AscIIProfile.ReadBoolValue(sr, "IsFilled");   
                axis = (AxisEnum)Enum.Parse(typeof(AxisEnum), AscIIProfile.ReadStringValue(sr, "Axis"));

                if (AscIIProfile.SeekSection("[Style]", ref sr))
                {
                    fillColor = Color.FromArgb(AscIIProfile.ReadIntValue(sr, "FillColor"));
                    lineColor = Color.FromArgb(AscIIProfile.ReadIntValue(sr, "LineColor"));
                    lineWidth = AscIIProfile.ReadFloatValue(sr, "LineWidth");
                    dashStyle = (DashStyle)Enum.Parse(typeof(DashStyle), AscIIProfile.ReadStringValue(sr, "DashStyle"));                    
                }

                if (AscIIProfile.SeekSection("[POINTS]", ref sr))
                {
                    points.Clear();
                    int count = AscIIProfile.ReadIntValue(sr, "Count");
                    if ( count > 0 && count < 1.0E10 )
                    {
                        for (int i = 0; i < count; i++)
                        {
                            points.Add(Vector64.Parse(AscIIProfile.ReadLine(sr), 4));
                        }
                    }
                }

                UpdateRange();

                sr.Close();
                
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        public void Offset(double offx, double offy, double offz = 0)
        {
            Vector64 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                p.x += (float)offx;
                p.y += (float)offy;
                p.z += (float)offz;
                points[i] = p;
            }
            minx += offx;
            miny += offy;
            minz += offz;
            maxx += offx;
            maxy += offy;
            maxz += offz;
        }
        public void Add(Vector64 p)
        {
            points.Add(p);
        }
        public void Add(double x, double y, double z = 0)
        {
            points.Add(new Vector64(x, y, z));
        }        

        /// <summary>
        /// 简化点，将直线上冗余点去掉
        /// 过滤掉距离小于平均点距的1/filter的点
        /// 删除距离很近的点
        /// 保证点数不少于3个点，否在多边形绘制错误！！！
        /// </summary>
        public void Simplify(bool distFilter = true, bool lineFilter = true, double zero = 0.001)
        {
            //去重点,过滤两点点距小于总长1/1000的点 zero = 0.001
            if (distFilter) Vector64.RemoveLineDuplicated(ref points, zero);
            if (lineFilter) Vector64.RemoveLineRedundant(ref points);
            UpdateRange();
        }
        public override void UpdateRange()
        {
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (i == 0)
                {
                    minx = maxx = p.X;
                    miny = maxy = p.Y;
                    minz = maxz = p.Z;
                }
                else
                {
                    if (p.x < minx) minx = p.x;
                    if (p.x > maxx) maxx = p.x;

                    if (p.y < miny) miny = p.y;
                    if (p.y > maxy) maxy = p.y;

                    if (p.z < minz) minz = p.z;
                    if (p.z > maxz) maxz = p.z;
                }
            }
        }
        //多边形剖分成网格，返回网格的值
        public float[] SampleToGrid(int xgrid = 100, int ygrid = 100)
        {
            double x, y;
            double xstep = (maxx - minx) / (xgrid - 1);
            double ystep = (maxy - miny) / (ygrid - 1);

            float[] grids = new float[xgrid * ygrid];

            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + ystep * iy;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + xstep * ix;
                    if (IsPointInsidePoly(x, y))
                        grids[ix + iy * xgrid] = (float)PropertyValue;
                    else grids[ix + iy * xgrid] = 0;
                }
            }
            return grids;
        }

        /// <summary>
        /// sample boudary points on average step
        /// </summary>
        /// <param name="lists">采样数组</param>
        /// <param name="step">采样步长sample step</param>
        /// <param name="val">采样点值</param>
        /// <param name="errbase">采样容差分母</param>
        /// <returns>本次采样点数</returns>
        public List<Vector64> SampleBoudary(double _step, double errbase = 1000)
        {
            List<Vector64> lists = new List<Vector64>();
            if (points.Count < 2) return lists;

            double total_len = Vector64.GetLength(points);
            double step = total_len / 200;
            if (_step < step) step = _step;

            //误差容差，小于这个容差的点默认为0
            double err = step / errbase;

            Vector64 p, p1, p2;

            p1 = points[0];

            //第1点
            lists.Add(p1);

            double len;
            p1 = points[0];

            int num = points.Count;
            if (IsClosed) num++;

            for (int i = 1; i < num; i++) //中间的采样点数
            {
                if (i < points.Count) p2 = points[i];
                else p2 = points[0];

                len = p1.Distance(p2);//线段长度

                //len > step: 线段p1 - p2 分段采样
                while (len >= step)
                {
                    p = p1 + (p2 - p1) * step / len;
                    lists.Add(p);
                    p1 = p;
                    len -= step;
                }
                if (Math.Abs(len) <= err) continue;//忽略该点
                else lists.Add(p2);
                p1 = p2;
            }

            //本次采样点数
            return lists;
        }

        //对边界及边界内坐标进行采样
        public int SampleInterBoudary(ref List<Vector64> lists, int xgrid = 11, int ygrid = 11)
        {
            double xstep = (maxx - minx) / (xgrid - 1);
            double ystep = (maxy - miny) / (ygrid - 1);
            double x, y;
            int sampled = 0;
            for (int iy = 0; iy < ygrid; iy++)
            {
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + ix * xstep + xstep / 2;
                    y = miny + iy * ystep + ystep / 2;
                    if (IsPointInsidePoly(x, y))
                    {
                        lists.Add(new Vector64(x, y, 2, PropertyValue));//z = 2 边界内点
                        sampled++;
                    }
                }
            }
            return sampled;
        }
        //对外边界坐标进行采样,采用边界坐标和边界外的坐标
        //保存坐标点：lists, x,y,0,v = 0
        //采样网格，xgrid,ygrid
        //网格扩展数：ext
        //返回：采样点数
        public int SampleOuterBoudary(ref List<Vector64> lists, int xgrid = 21, int ygrid = 21, int ext = 1)
        {
            double xstep = (maxx - minx) / (xgrid - 1);
            double ystep = (maxy - miny) / (ygrid - 1);
            double x, y;

            //grid width and height
            int width = xgrid + 2 * ext;
            int height = ygrid + 2 * ext;

            bool[] boders = new bool[width * height];

            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    x = minx + (ix - ext) * xstep + xstep / 2;
                    y = miny + (iy - ext) * ystep + ystep / 2;
                    if (IsPointInsidePoly(x, y))
                        boders[iy * width + ix] = true;
                    else
                        boders[iy * width + ix] = false;
                }
            }

            int id, ix1, iy1;
            int sampled = 0;
            bool keep = true;
            int[] nears = new int[8];
            for (int iy = 0; iy < height; iy++)
            {
                for (int ix = 0; ix < width; ix++)
                {
                    id = iy * width + ix;
                    if (boders[id]) continue;//只处理边界外点

                    nears[0] = id - 1;
                    nears[1] = id + 1;
                    nears[2] = id - width;
                    nears[3] = id + width;
                    nears[4] = id - width - 1;
                    nears[5] = id - width + 1;
                    nears[6] = id + width - 1;
                    nears[7] = id + width + 1;

                    keep = false;
                    for (int j = 0; j < 8; j++)
                    {
                        if (nears[j] >= 0 && nears[j] < boders.Length &&
                             boders[nears[j]]) //相邻点内点，则为边界点
                        {
                            iy1 = nears[j] / width;
                            ix1 = nears[j] - iy1 * width;
                            x = minx + (ix1 - ext) * xstep + xstep / 2;
                            y = miny + (iy1 - ext) * ystep + ystep / 2;
                            lists.Add(new Vector64(x, y, 2, PropertyValue));//z =2 边界内点
                            sampled++;
                            keep = true;
                            break;
                        }
                    }
                    if (keep)//边界外点
                    {
                        x = minx + (ix - ext) * xstep + xstep / 2;
                        y = miny + (iy - ext) * ystep + ystep / 2;
                        lists.Add(new Vector64(x, y, 1, PropertyValue));//z =1 边界外点
                        sampled++;
                    }
                }
            }
            boders = null;
            nears = null;
            return sampled;
        }

        public override void Clear()
        {
            points.Clear();
        }
        //点在多边形内判断，2D版本
        // taken from https://wrf.ecse.rpi.edu//Research/Short_Notes/pnpoly.html
        public bool IsPointInsidePoly(Vector32 test)
        {
            return IsPointInsidePoly(test.x, test.y);
        }
        public bool IsPointInsidePoly(double x, double y)
        {
            int i;
            int j;
            bool result = false;
            double x1, y1, x2, y2;
            for (i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                x1 = points[i].x;
                y1 = points[i].y;
                x2 = points[j].x;
                y2 = points[j].y;

                if ((y1 > y) != (y2 > y) && (x < (x2 - x1) * (y - y1) / (y2 - y1) + x1))
                {
                    result = !result;
                }
            }
            return result;
        }

        //get intersections with line
        public virtual Vector64[] GetIntersectPoints(CLine line)
        {
            List<Vector64> intersets = new List<Vector64>();
            Vector64 p1, p2, p;
            for (int i = 0; i < points.Count - 1; i++)
            {
                p1 = points[i];
                p2 = points[i + 1];
                CLine s1 = new CLine(p1, p2);
                if (line.GetIntersection(s1, out p))
                {
                    intersets.Add(p);
                    break;
                }
            }
            return intersets.ToArray();
        }


        //Finding Intersection Points of a line segment and given convex polygon
        public virtual Vector32[] GetIntersectionPoints(Vector32 l1p1, Vector32 l1p2)
        {
            List<Vector32> intersectionPoints = new List<Vector32>();
            Vector32 ip;
            for (int i = 0; i < points.Count; i++)
            {
                int next = (i + 1 == points.Count) ? 0 : i + 1;
                if (GeometryHelper.GetIntersectionPoint(l1p1, l1p2, points[i], points[next], out ip))
                    intersectionPoints.Add(ip);
            }
            return intersectionPoints.ToArray();
        }

        //One Important Tip
        //Some edge cases, such as two overlapping corners or intersection on a corner can cause 
        //some duplicates corner added to the polygon.
        //We can easily get rid of these with such small utility function:
        public static void Add(List<Vector32> pool, Vector32[] newpoints)
        {
            foreach (Vector32 np in newpoints)
            {
                bool found = false;
                foreach (Vector32 p in pool)
                {
                    if (GeometryHelper.IsEqual(p.X, np.X) && GeometryHelper.IsEqual(p.Y, np.Y))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) pool.Add(np);
            }
        }
        //Ordering the corners of a polygon clockwise
        public static Vector32[] OrderClockwise(Vector32[] points)
        {
            double mX = 0;
            double my = 0;
            foreach (Vector32 p in points)
            {
                mX += p.X;
                my += p.Y;
            }
            mX /= points.Length;
            my /= points.Length;

            return points.OrderBy(v => Math.Atan2(v.Y - my, v.X - mX)).ToArray();
        }
        //main algorithm
        public static Polygon2D GetIntersectionOfPolygons(Polygon2D poly1, Polygon2D poly2)
        {
            List<Vector32> clippedCorners = new List<Vector32>();

            //Add  the corners of poly1 which are inside poly2       
            for (int i = 0; i < poly1.points.Count; i++)
            {
                if (poly2.IsPointInsidePoly(poly1.points[i]))
                    Add(clippedCorners, new Vector32[] { poly1.points[i] });
            }

            //Add the corners of poly2 which are inside poly1
            for (int i = 0; i < poly2.points.Count; i++)
            {
                if (poly1.IsPointInsidePoly(poly2.points[i]))
                    Add(clippedCorners, new Vector32[] { poly2.points[i] });
            }

            //Add  the intersection points
            for (int i = 0, next = 1; i < poly1.points.Count;
                 i++, next = (i + 1 == poly1.points.Count) ? 0 : i + 1)
            {
                Add(clippedCorners, poly2.GetIntersectionPoints(poly1.points[i], poly1.points[next]));
            }

            return new Polygon2D(OrderClockwise(clippedCorners.ToArray()));
        }
    }

    public class Box3D : Symbol3D
    {
        [CategoryAttribute("Shape"), DisplayNameAttribute("Size X"),Browsable(true)]
        public override double XSize { get; set; } = 1.0;
        [CategoryAttribute("Shape"), DisplayNameAttribute("Size Y"), Browsable(true)]
        public override double ySize { get; set; } = 1.0;
        [CategoryAttribute("Shape"), DisplayNameAttribute("Size Z"), Browsable(true)]
        public override double zSize { get; set; } = 1.0;
        [CategoryAttribute("Shape"), DisplayNameAttribute("Color"), Browsable(true)]
        public Color Color { get; set; } = Color.Gray;

        public Vector32 Start = new Vector32();

        [CategoryAttribute("Shape"), DisplayNameAttribute("Box"), Browsable(true)]
        public TriangleObj BoxFace
        {
            get
            {
                if (Faces.Count == 1) return Faces[0];
                else return null;
            }
            set
            {
                if (Faces.Count == 1)
                {
                    Faces[0] = value;
                }
            }
        }

        public Box3D(Vector32 p0, double xl, double yl, double zl)
        {
            type = ShapeEnum.Shape;
            symbolType = SymbolEnum.Box;
            Start = p0;
            XSize = xl;
            ySize = yl;
            zSize = zl;
            TriangleObj obj = new TriangleObj();
            obj.Name = "Box";
            Faces.Add(obj);
        }

        public override bool Create()
        {
            Clear();            

            double x = Start.x;
            double y = Start.y;
            double z = Start.z;
            double xs = XSize / 2;
            double ys = ySize / 2;
            double zs = zSize / 2;
            BoxFace.AddPoint(x - xs / 2, y - ys / 2, z - zs / 2);
            BoxFace.AddPoint(x + xs / 2, y - ys / 2, z - zs / 2);
            BoxFace.AddPoint(x - xs / 2, y + ys / 2, z - zs / 2);
            BoxFace.AddPoint(x + xs / 2, y + ys / 2, z - zs / 2);
            BoxFace.AddPoint(x - xs / 2, y - ys / 2, z + zs / 2);
            BoxFace.AddPoint(x + xs / 2, y - ys / 2, z + zs / 2);
            BoxFace.AddPoint(x - xs / 2, y + ys / 2, z + zs / 2);
            BoxFace.AddPoint(x + xs / 2, y + ys / 2, z + zs / 2);
            int[] indices = new int[] 
            {   0, 2, 3, 3, 1, 0,
                4, 5, 6, 5, 7, 6,
                4, 2, 0, 6, 2, 4, 
                1, 3, 5, 3, 7, 5, 
                0, 1, 4, 1, 5, 4, 
                6, 7, 2, 7, 3, 2 
            };
            for (int i = 0; i < indices.Length / 3; i++)
            {
                BoxFace.AddTriangleIndex(indices[3 * i], indices[3 * i + 1], indices[3 * i + 2]);
            }
            indices = null;
            UpdateRange();
            return true;
        }
        /*
        public override bool IsPointInPolygon(Vector32 point)
        {
            //移动到中心，            
            Vector32 p = UnTransformedPoint(point);
            if( p.x > minx && p.x < maxx &&
                p.y > miny && p.y < maxy &&
                p.z > minz && p.z < maxz )
                return true;
            return false;
        }*/
    }
    public enum PointLineRelation
    {
        None = -1,
        Left = 0,
        Right = 1,
        OnLine = 2,
        OnLineExtend = 10
    }

    public class PLine
    {
        public List<Vector64> points = new List<Vector64>();
        public PLine()
        {
        }
        public PLine(List<Vector64> _points)
        {
            points = _points;
        }

        public Vector64 GetInterpolatedPositionByDistance( double x )
        {
            if ( points.Count < 2 ) return new Vector64();

            Vector64 p1 = points[0],p;
            if (x <= 0) return p1;

            double dist, len = 0;
            for (int i = 1; i < points.Count; i++ )
            {
                p = points[i];
                dist = p.Distance(p1);
                len += dist;
                if (x == len) return p;
                else if( x < len )
                {
                    return p1 + (x - len - dist ) / dist * (p - p1);
                }
                p1 = p;
            }
            return points[points.Count-1];
        }

        public int Count { get { return points.Count; } }

        public Vector64 this[int id]
        {
            get { return points[id]; }
            set { points[id] = value; }
        }
        public void Clear() { points.Clear(); }
        public void Add(double x, double y, double z, double v = 0)
        {
            points.Add(new Vector64(x, y, z, v));
        }
    }
    public class CLine
    {
        //x = x0 + at
        //y = y0 + bt
        //z = z0 + ct
        public Vector64 p1;
        public Vector64 p2;
        public CLine()
        {

        }
        public CLine(Vector64 _p1, Vector64 _p2)
        {
            p1 = _p1;
            p2 = _p2;
        }
        public CLine(Vector32 _p1, Vector32 _p2)
        {
            p1 = new Vector64(_p1.x, _p1.y, _p1.z);
            p2 = new Vector64(_p2.x, _p2.y, _p2.z);
        }
        /// <summary>
        /// 线方向，单位矢量
        /// </summary>
        public Vector64 Direction
        {
            get
            {
                Vector64 v = (p2 - p1);
                return v.Normalize();
            }
        }
        public double a
        {
            get { return p2.x - p1.x; }
        }
        public double b
        {
            get { return p2.y - p1.y; }
        }
        public double c
        {
            get { return p2.z - p1.z; }
        }
        public double x0
        {
            get { return p1.x; }
        }
        public double y0
        {
            get { return p1.y; }
        }
        public double z0
        {
            get { return p1.z; }
        }
        static public bool IsZero(double val, double zero = 1.0E-20)
        {
            double v = val;
            if (v < 0) v = -v;
            if (v <= zero) return true;
            else return false;
        }
        /// <summary>
        /// p点是否在线段p1p2上，通过距离来判断
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        //——p---p1 -- p --- p2——p
        public bool IsOnLine(Vector64 p,double err = 1.0E-10 )
        {
            //是否共线
            if ( !IsEquals( (p.x - p1.x) * (p1.y - p2.y), 
                            (p1.x - p2.x) * (p.y - p1.y), err ) )
                return false;

            //是否在线段值范围内
            if ( p.x < Math.Min(p1.x, p2.x) ) return false;
            if ( p.x > Math.Max(p1.x, p2.x) ) return false;
            if ( p.y < Math.Min(p1.y, p2.y) ) return false;
            if ( p.y > Math.Max(p1.y, p2.y) ) return false;

            return true;

            /*
            double dist1 = p.Distance(p1);
            double dist2 = p.Distance(p2);
            double dist = p1.Distance(p2);
            if (IsEquals(dist1 + dist2, dist)) return true;
            else return false;
            */
        }
        /// <summary>
        /// 点是否在直线及延长线上
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// //——p---p1 -- p --- p2——p
        public bool IsOnLineExt(Vector64 p,double err = 1.0E-10)
        {
            //是否共线            
            return IsEquals( (p.x - p1.x) * (p1.y - p2.y),
                             (p1.x - p2.x) * (p.y - p1.y), err );            
            /*
            double dist1 = p.Distance(p1,4);
            double dist2 = p.Distance(p2,4);
            double dist12 = p1.Distance(p2,4);
            if (IsEquals(dist1 + dist2, dist12)) return true;
            if (IsEquals(dist1 + dist12, dist2)) return true;
            if (IsEquals(dist2 + dist12, dist1)) return true;
            return false;
            */
        }
        public static bool IsEquals(double v1, double v2, double err = 1.0E-20)
        {
            double dd = Math.Abs(v1 - v2);
            if ( dd <= err ) return true;
            else return false;
        }
        //p点到直线距离p1,p2
        public double Distance(Vector64 p,int pow = 2)
        {
            double a = p2.y - p1.y;
            double b = p1.x - p2.x;
            double c = p2.x * p1.y - p1.x * p2.y;
            if (a == 0 && b == 0) return Vector64.Distance(p1, p, pow);//直线坍缩为一点
            else 
            {
                if (pow == 4)
                {
                    double zz = a * p.x + b * p.y + c;
                    return zz * zz / (a * a + b * b); 
                }
                else return Math.Abs(a * p.x + b * p.y + c) / Math.Sqrt(a * a + b * b);
            }
        }
        //点与直线的关系2D version
        public PointLineRelation GetPointReletion(Vector32 p)
        {
            double x0 = p.x;
            double y0 = p.y;
            double z0 = p.z;
            double val = (p2.x - p1.x) * (y0 - p1.y) - (x0 - p1.x) * (p2.y - p1.y);
            if (IsZero(val))
            {
                if (IsOnLine(p.toVector64())) //在直线上
                    return PointLineRelation.OnLine;
                else return PointLineRelation.OnLineExtend;
            }
            else
            {
                if (val > 0) return PointLineRelation.Left;
                else return PointLineRelation.Right;
            }
        }

        //点到直线投影2D
        public Vector64 GetPointProjection(Vector64 p)
        {
            Vector64 p0 = new Vector64();

            double dx = p1.x - p2.x;
            double dy = p1.y - p2.y;

            double sq2 = (dx * dx) + (dy * dy);

            if (sq2 == 0) return p1; //直线坍缩为点

            double u = (p.x - p1.x) * (p1.x - p2.x) +
                       (p.y - p1.y) * (p1.y - p2.y);

            u = u / sq2;

            p0.x = p1.x + u * dx;
            p0.y = p1.y + u * dy;

            return p0;
        }
        //直线line是否在当前直线矩形范围内
        public bool IsRectIntersect(CLine line)
        {
            if (Math.Min(p1.x, p2.x) <= Math.Max(line.p1.x, line.p2.x) &&
                 Math.Min(p1.y, p2.y) <= Math.Max(line.p1.y, line.p2.y) &&
                 Math.Min(p1.z, p2.z) <= Math.Max(line.p1.z, line.p2.z) &&
                 Math.Min(line.p1.x, line.p2.x) <= Math.Max(p1.x, p2.x) &&
                 Math.Min(line.p1.y, line.p2.y) <= Math.Max(p1.y, p1.y) &&
                 Math.Min(line.p1.z, line.p2.z) <= Math.Max(p1.z, p2.z))
                return true;
            else return false;
        }

        public bool IsInRectRange(Vector64 p)
        {
            double x1 = p1.x;
            double y1 = p1.y;
            double z1 = p1.z;
            double x2 = x1;
            double y2 = y1;
            double z2 = z1;
            if (p2.x > x2) x2 = p2.x;
            if (p2.y > y2) y2 = p2.y;
            if (p2.z > z2) z2 = p2.z;
            if (p.x < x1 || p.x > x2 ||
                p.y < y1 || p.y > y2 ||
                p.z < z1 || p.z > z2) return false;
            else return true;
        }

        /// <summary>
        /// 判断线与线之间的相交
        /// </summary>
        /// <param name="intersection">交点</param>
        /// <param name="p1">直线1上一点</param>
        /// <param name="v1">直线1方向,单位矢量</param>
        /// <param name="p2">直线2上一点</param>
        /// <param name="v2">直线2方向,单位矢量</param>
        /// <returns>是否相交</returns>
        public static bool GetIntersection(CLine line1, CLine line2, out Vector64 intersection)
        {
            intersection = new Vector64();

            Vector64 P1 = line1.p1;
            Vector64 P2 = line2.p1;
            Vector64 V1 = line1.Direction;
            Vector64 V2 = line2.Direction;

            // 两线是否平行
            if (IsZero(Vector64.Dot(V1, V2))) return false;
            Vector64 startPointSeg = P2 - P1;
            Vector64 vecS1 = Vector64.Cross(V1, V2);            // 有向面积1
            Vector64 vecS2 = Vector64.Cross(startPointSeg, V2); // 有向面积2
            double num = Vector64.Dot(startPointSeg, vecS1);
            // 判断两这直线是否共面
            if (Math.Abs(num) >= 1E-05)
            {
                return false;
            }

            // 有向面积比值，利用点乘是因为结果可能是正数或者负数
            double num2 = Vector64.Dot(vecS2, vecS1) / vecS1.sqrMagnitude;
            intersection = P1 + V1 * num2;
            return true;
        }
        //---线段相交并求交点：2D版本-------------
        // this is only 2D version, improvement needed
        //作者：Away - Far
        //来源：CSDN
        //原文：https://blog.csdn.net/wcl0617/article/details/78654944 
        //版权声明：本文为博主原创文章，转载请附上博文链接！
        //int get_line_intersection(float p0_x, float p0_y, float p1_x, float p1_y,
        //float p2_x, float p2_y, float p3_x, float p3_y, float* i_x, float* i_y)
        public bool GetIntersection(CLine line, out Vector64 p)
        {
            double p0_x = p1.x;
            double p0_y = p1.y;
            double p1_x = p2.x;
            double p1_y = p2.y;
            double p2_x = line.p1.x;
            double p2_y = line.p1.y;
            double p3_x = line.p2.x;
            double p3_y = line.p2.y;

            double s02_x, s02_y, s10_x, s10_y, s32_x, s32_y, s_numer, t_numer, denom, t;
            s10_x = p1_x - p0_x;
            s10_y = p1_y - p0_y;
            s32_x = p3_x - p2_x;
            s32_y = p3_y - p2_y;
            p = new Vector64(0, 0, 0);
            denom = s10_x * s32_y - s32_x * s10_y;
            if (denom == 0)//平行或共线
                return false;
            bool denomPositive = denom > 0;

            s02_x = p0_x - p2_x;
            s02_y = p0_y - p2_y;
            s_numer = s10_x * s02_y - s10_y * s02_x;

            //参数是大于等于0且小于等于1的，分子分母必须同号且分子小于等于分母
            if ((s_numer < 0) == denomPositive)
                return false; // No collision

            t_numer = s32_x * s02_y - s32_y * s02_x;
            if ((t_numer < 0) == denomPositive)
                return false; // No collision

            if (Math.Abs(s_numer) > Math.Abs(denom) ||
                 Math.Abs(t_numer) > Math.Abs(denom))
                return false; // No collision,Collision detected

            t = t_numer / denom;

            p.x = p0_x + (t * s10_x);
            p.y = p0_y + (t * s10_y);

            return true;
        }
        //直线与直线相交于延长线
        //2D版本
        public bool GetIntersectionExt(CLine line, out Vector64 p)
        {
            double p0_x = p1.x;
            double p0_y = p1.y;
            double p1_x = p2.x;
            double p1_y = p2.y;
            double p2_x = line.p1.x;
            double p2_y = line.p1.y;
            double p3_x = line.p2.x;
            double p3_y = line.p2.y;

            double s02_x, s02_y, s10_x, s10_y, s32_x, s32_y, s_numer, t_numer, denom, t;
            s10_x = p1_x - p0_x;
            s10_y = p1_y - p0_y;
            s32_x = p3_x - p2_x;
            s32_y = p3_y - p2_y;
            p = new Vector64(0, 0, 0);
            denom = s10_x * s32_y - s32_x * s10_y;
            if (denom == 0)//平行或共线
                return false;
            bool denomPositive = denom > 0;

            s02_x = p0_x - p2_x;
            s02_y = p0_y - p2_y;
            s_numer = s10_x * s02_y - s10_y * s02_x;
            t_numer = s32_x * s02_y - s32_y * s02_x;
            t = t_numer / denom;
            p.x = p0_x + (t * s10_x);
            p.y = p0_y + (t * s10_y);

            return true;
        }
        // two extended lines intersection
        /*
        public bool GetIntersectionExt(CLine line,out Vector32 p)
        {
            p = new Vector32(0, 0, 0);
            //if ( !IsRectIntersect(line) ) return false;
            double a1 = line.a;
            double b1 = line.b;
            double c1 = line.c;
            double b0 = a1 * b - a * b1;
            if (b0 == 0) return false;
            double t = (b1 * (x0 - line.x0) + (y0 - line.y0)) / b0;
            p.x = (float)(x0 + a * t);
            p.y = (float)(y0 + b * t);
            p.z = (float)(z0 + c * t);
            return true;
        }
        */
    }
    public class Polygon3D : TriangleObj
    {        
        public Polygon3D()
        {
            type = ShapeEnum.Triangles;
        }

        public Polygon3D(TriangleObj obj)
        {
            points = obj.points;
            triangles = obj.triangles;
            minx = obj.minx;
            miny = obj.miny;
            minz = obj.minz;
            maxx = obj.maxx;
            maxy = obj.maxy;
            maxz = obj.maxz;
            type = ShapeEnum.Triangles;
        }

        //创建球缓冲区？
        private void CreateCircle(double x0, double y0, double z0, double B, double rad, int slice)
        {
            double angleStep = 2.0 * Math.PI / slice;
            double a, b = B;
            double x, y, z;
            Vector32 p = new Vector32();
            for (int i = 0; i < slice; i++)
            {
                a = i * angleStep;

                x = Math.Round(rad * Math.Cos(a), 5);
                z = Math.Round(rad * Math.Sin(a), 5);
                y = 0;
                p = new Vector32((float)x, (float)y, (float)z);
                p.Rotate((float)b, 2);

                p.X += (float)x0;
                p.Y += (float)y0;
                p.Z += (float)z0;
                points.Add(p);
            }
        }
        public Polygon3D(C3DLine obj, double rad, int slice = 20)
        {
            type = ShapeEnum.Triangles;
            int np = obj.points.Count;
            if (np < 2) return;
            Name = obj.Name + "_poly";

            Vector32 p1, p2;
            Vector32 v1, v2, vp1, vp2, v0 = new Vector32(0, 1, 0);
            double a;
            for (int i = 0; i < np; i++)
            {
                if (i == 0)
                {
                    p1 = obj.points[i];
                    p2 = obj.points[i + 1];
                    v1 = new Vector32(p1.x, p1.y, p1.z);
                    v2 = new Vector32(p2.x, p2.y, p2.z);
                    a = Vector32.VectorAngle(v0, v2 - v1);
                }
                else if (i == np - 1)
                {
                    p1 = obj.points[i - 1];
                    p2 = obj.points[i];
                    v1 = new Vector32(p1.x, p1.y, p1.z);
                    v2 = new Vector32(p2.x, p2.y, p2.z);
                    a = Vector32.VectorAngle(v0, v2 - v1);
                }
                else
                {
                    p1 = obj.points[i - 1];
                    p2 = obj.points[i];
                    v1 = new Vector32(p1.x, p1.y, p1.z);
                    v2 = new Vector32(p2.x, p2.y, p2.z);
                    vp1 = v2 - v1;

                    p1 = obj.points[i];
                    p2 = obj.points[i + 1];
                    v1 = new Vector32(p1.x, p1.y, p1.z);
                    v2 = new Vector32(p2.x, p2.y, p2.z);
                    vp2 = v2 - v1;

                    //mid vector,angle 0-90
                    a = Vector32.VectorAngle(v0, (vp1 + vp2) / 2);
                }

                p1 = obj.points[i];

                a = Vector32.toRad(a);

                CreateCircle(p1.x, p1.y, p1.z, a, rad, slice);
            }
            //create triangle indices
            int i1, i2, i3, i4;
            for (int i = 0; i < np - 1; i++)
            {
                for (int j = 0; j < slice; j++)
                {
                    i1 = i * slice + j;
                    i2 = (i + 1) * slice + j;

                    i3 = i1 + 1;
                    i4 = i2 + 1;
                    if (j == slice - 1)
                    {
                        i3 = i * slice;
                        i4 = (i + 1) * slice;
                    }
                    triangles.Add(new Int32XYZ(i1, i2, i3));
                    triangles.Add(new Int32XYZ(i2, i4, i3));
                }
            }

            //create head and tail triangle indices            
            p1 = obj.points[0];
            v1 = new Vector32(p1.x, p1.y, p1.z);
            int id = points.Count;
            points.Add(v1);
            for (int j = 0; j < slice; j++)
            {
                i1 = id;
                i2 = j;
                i3 = j + 1;
                if (j == slice - 1) i3 = 0;
                triangles.Add(new Int32XYZ(i1, i2, i3));
            }
            p1 = obj.points[np - 1];
            v1 = new Vector32(p1.x, p1.y, p1.z);
            id = points.Count;
            points.Add(v1);
            int start = (np - 1) * slice;
            for (int j = 0; j < slice; j++)
            {
                i1 = id;
                i2 = start + j;
                i3 = start + j + 1;
                if (j == slice - 1) i3 = start;
                triangles.Add(new Int32XYZ(i1, i3, i2));
            }
            UpdateRange();
        }

#if DEBUG
        private static int errno = 0;
#endif
        //p1,p2 edge points, b1 b2 is point status if point is blanked
        public bool GetIntersectionOnAxis(Vector32 p1, Vector32 p2, bool b1, bool b2, out Vector32 sect, int axis)
        {
            // p1--->p2
            Vector64 p;

            if (b1) sect = new Vector32(p2.X, p2.Y, p2.Z);
            else sect = new Vector32(p1.X, p1.Y, p1.Z);

            int nsec = 0;
            CTriangle3f tri = new CTriangle3f();
            for (int i = 0; i < triangles.Count; i++)
            {
                tri.p1 = points[triangles[i].x].toVector64();
                tri.p2 = points[triangles[i].y].toVector64();
                tri.p3 = points[triangles[i].z].toVector64();
                if (tri.GetIntersectionOnTriangle(p1.toVector64(), p2.toVector64(), out p))
                {
                    if (b1)
                    {
                        if (axis == 0)
                        {
                            if (p.X < sect.X) sect = p;
                        }
                        else if (axis == 1)
                        {
                            if (p.Y < sect.Y) sect = p;
                        }
                        else if (axis == 2)
                        {
                            if (p.Z < sect.Z) sect = p;
                        }
                    }
                    else if (b2)
                    {
                        if (axis == 0)
                        {
                            if (p.X > sect.X) sect = p;
                        }
                        else if (axis == 1)
                        {
                            if (p.Y > sect.Y) sect = p;
                        }
                        else if (axis == 2)
                        {
                            if (p.Z > sect.Z) sect = p;
                        }
                    }
                    nsec++;
                }
            }

            if (nsec < 1)
            {
#if DEBUG
                errno++;
#endif
                return false;
            }
            else return true;
        }

        //direct =0 x,1 y 2 z
        private IntersectionType CheckCrossTriangle(double x0, double y0, double z0,
                                                    double h1, double h2,
                                                    int tri, int direct)
        {
            Vector32 p1 = points[triangles[tri].x];
            Vector32 p2 = points[triangles[tri].y];
            Vector32 p3 = points[triangles[tri].z];

            //get triangle range
            double _minx, _maxx, _miny, _maxy, _minz, _maxz;
            _minx = _maxx = p1.X;
            _miny = _maxy = p1.Y;
            _minz = _maxz = p1.Z;
            if (_minx > p2.X) _minx = p2.X;
            if (_miny > p2.Y) _miny = p2.Y;
            if (_minz > p2.Z) _minz = p2.Z;
            if (_maxx < p2.X) _maxx = p2.X;
            if (_maxy < p2.Y) _maxy = p2.Y;
            if (_maxz < p2.Z) _maxz = p2.Z;
            if (_minx > p3.X) _minx = p3.X;
            if (_miny > p3.Y) _miny = p3.Y;
            if (_minz > p3.Z) _minz = p3.Z;
            if (_maxx < p3.X) _maxx = p3.X;
            if (_maxy < p3.Y) _maxy = p3.Y;
            if (_maxz < p3.Z) _maxz = p3.Z;

            //triangle out of the range of line h1---h2 
            switch (direct)
            {
                case 0://x direction
                    if (h2 < _minx || h1 > _maxx) return IntersectionType.none;
                    if (y0 < _miny || y0 > _maxy) return IntersectionType.none;
                    if (z0 < _minz || z0 > _maxz) return IntersectionType.none;
                    //triangle project to yz plane
                    p1.X = p2.X = p3.X = 0;
                    if (p1.Y == p2.Y || p1.Y == p3.Y || p2.Y == p3.Y) return IntersectionType.line;
                    if (p1.Z == p2.Z || p1.Z == p3.Z || p2.Z == p3.Z) return IntersectionType.line;
                    return Vector32.IsPointInTriangle(p1, p2, p3, new Vector32(0, (float)y0, (float)z0));
                case 1://y direction
                    if (h2 < _miny || h1 > _maxy) return IntersectionType.none;
                    if (x0 < _minx || x0 > _maxx) return IntersectionType.none;
                    if (z0 < _minz || z0 > _maxz) return IntersectionType.none;
                    //triangle project to xz plane
                    p1.Y = p2.Y = p3.Y = 0;
                    if (p1.X == p2.X || p1.X == p3.X || p2.X == p3.X) return IntersectionType.line;
                    if (p1.Z == p2.Z || p1.Z == p3.Z || p2.Z == p3.Z) return IntersectionType.line;
                    //if (!CTriangle3f.ValidTriangle(p1, p2, p3)) return IntersectionType.triangle;
                    return Vector32.IsPointInTriangle(p1, p2, p3, new Vector32((float)x0, 0, (float)z0));
                case 2://z direction
                    if (h2 < _minz || h1 > _maxz) return IntersectionType.none;
                    if (x0 < _minx || x0 > _maxx) return IntersectionType.none;
                    if (y0 < _miny || y0 > _maxy) return IntersectionType.none;
                    //triangle project to xy plane
                    p1.Z = p2.Z = p3.Z = 0;
                    if (p1.X == p2.X || p1.X == p3.X || p2.X == p3.X) return IntersectionType.line;
                    if (p1.Y == p2.Y || p1.Y == p3.Y || p2.Y == p3.Y) return IntersectionType.line;
                    //if (!CTriangle3f.ValidTriangle(p1, p2, p3)) return IntersectionType.triangle;
                    return Vector32.IsPointInTriangle(p1, p2, p3, new Vector32((float)x0, (float)y0, 0));
            }
            return IntersectionType.none;
        }

        // Copyright 2001 softSurfer, 2012 Dan Sunday
        // This code may be freely used and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        // Assume that classes are already given for the objects:
        //    Point and Vector with
        //        coordinates {float x, y, z;}
        //        operators for:
        //            == to test  equality
        //            != to test  inequality
        //            (Vector)0 =  (0,0,0)         (null vector)
        //            Point   = Point ± Vector
        //            Vector =  Point - Point
        //            Vector =  Scalar * Vector    (scalar product)
        //            Vector =  Vector * Vector    (cross product)
        //    Line and Ray and Segment with defining  points {Point P0, P1;}
        //        (a Line is infinite, Rays and  Segments start at P0)
        //        (a Ray extends beyond P1, but a  Segment ends at P1)
        //    Plane with a point and a normal {Point V0; Vector  n;}
        //    Triangle with defining vertices {Point V0, V1, V2;}
        //    Polyline and Polygon with n vertices {int n;  Point *V;}
        //        (a Polygon has V[n]=V[0])
        //===================================================================

        //#define SMALL_NUM   0.00000001 // anything that avoids division overflow
        // dot product (3D) which allows vector operations in arguments
        //#define dot(u,v)   ((u).x * (v).x + (u).y * (v).y + (u).z * (v).z)

        // intersect3D_RayTriangle(): find the 3D intersection of a ray with a triangle
        //    Input:  a ray R, and a triangle T
        //    Output: *I = intersection point (when it exists)
        //    Return: -1 = triangle is degenerate (a segment or point)
        //             0 =  disjoint (no intersect)
        //             1 =  intersect in unique point I1
        //             2 =  are in the same plane
        //Intersection I of Rayline p1p2 and triangle v1v2v3
        public int intersect3D_RayTriangle(Vector32 p1, Vector32 p2, Vector32 v0, Vector32 v1, Vector32 v2, out Vector32 I)
        {
            Vector32 u, v, n;              // triangle vectors
            Vector32 dir, w0, w;           // ray vectors
            double r, a, b;              // params to calc ray-plane intersect
            double SMALL_NUM = 0.00000001f;

            I = new Vector32(0, 0, 0);
            // get triangle edge vectors and plane normal
            u = v1 - v0;
            v = v2 - v0;
            n = Vector32.Cross(u, v);              // cross product
            if (n.Length == 0) return -1;
            //if (n == (Vector32)0)             // triangle is degenerate
            //    return -1;                  // do not deal with this case

            dir = p2 - p1;              // ray direction vector
            w0 = p1 - v0;
            a = -Vector32.Dot(n, w0);
            b = Vector32.Dot(n, dir);
            if (Math.Abs(b) < SMALL_NUM)
            {     // ray is  parallel to triangle plane
                if (a == 0)                 // ray lies in triangle plane
                    return 2;
                else return 0;              // ray disjoint from plane
            }

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0.0)                    // ray goes away from triangle
                return 0;                   // => no intersect
                                            // for a segment, also test if (r > 1.0) => no intersect

            I = p1 + r * dir;            // intersect point of ray and plane

            // is I inside T?
            double uu, uv, vv, wu, wv, D;
            uu = Vector32.Dot(u, u);
            uv = Vector32.Dot(u, v);
            vv = Vector32.Dot(v, v);
            w = I - v0;
            wu = Vector32.Dot(w, u);
            wv = Vector32.Dot(w, v);
            D = uv * uv - uu * vv;

            // get and test parametric coords
            double s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)         // I is outside T
                return 0;
            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                return 0;

            return 1;                       // I is in T
        }

        public bool IsPointInPolygon3(Vector32 point)
        {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;
            //point may on the edge of the polygon
            if (x < minx || x > maxx) return false;
            if (y < miny || y > maxy) return false;
            if (z < minz || z > maxz) return false;
            //collect all the triangles on the direction of p1p2;
            Vector32 v0, v1, v2;
            Vector32 p1 = point;
            Vector32 p2 = new Vector32(p1.X, p1.Y, p1.Z);
            Vector32 sect;
            int ret;
            p2.Y = (float)maxx + 100;
            int nsec = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                // return 0-no intersection,1-intersected,
                // 2 -intersected on line or on cornerpoint
                v0 = points[triangles[i].x];
                v1 = points[triangles[i].y];
                v2 = points[triangles[i].z];
                ret = intersect3D_RayTriangle(p1, p2, v0, v1, v2, out sect);
                if (ret == 1) nsec++;
            }
            if (nsec > 0)
            {
                int d = nsec;
                d = d >> 1;
                d = d << 1;
                //odd
                if (d != nsec) return true;
            }
            return false;
        }
        /// <summary>
        /// 判断点是否在多面体内
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual bool IsPointInPolygon(Vector32 point)
        {
            float x = point.X;
            float y = point.Y;
            float z = point.Z;

            //point may on the edge of the polygon
            if (x < minx || x > maxx) return false;
            if (y < miny || y > maxy) return false;
            if (z < minz || z > maxz) return false;

            //check point in which part of the polygon
            double x0 = (minx + maxx) / 2;
            double y0 = (miny + maxy) / 2;
            double z0 = (minz + maxz) / 2;

            //ret ==0, outside, ret == 1 inside, ret < 0 faild
            int ret = 0;

            if (x < x0)
                ret = CheckPointInPolygon(new Vector32((float)minx - 1, y, z), new Vector32(x, y, z), 0);
            else ret = CheckPointInPolygon(new Vector32(x, y, z), new Vector32((float)maxx + 1, y, z), 0);

            //try y direction
            if (ret < 0)
            {
                if (y < y0) ret = CheckPointInPolygon(new Vector32(x, (float)miny - 1, z), new Vector32(x, y, z), 1);
                else ret = CheckPointInPolygon(new Vector32(x, y, z), new Vector32(x, (float)maxy + 1, z), 1);
            }
            //try z direction
            if (ret < 0)
            {
                if (z < z0) ret = CheckPointInPolygon(new Vector32(x, y, (float)minz - 1), new Vector32(x, y, z), 2);
                else ret = CheckPointInPolygon(point, new Vector32(x, y, (float)maxz + 1), 2);
            }

            /////try another direction------------------------- 
            if (ret == 0) return false;
            else if (ret == 1) return true;
            else
            {
                throw new Exception("Ambigous of point status.");
                return true;
            }

        }
        /// <summary>
        /// check point x,y,z is inside polygon
        /// point on left part of polygon
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>
        /// ret == 0, outside
        /// ret == 1 inside
        /// ret < 0 faild
        /// </returns>        
        int CheckPointInPolygon(Vector32 p1, Vector32 p2, int direct)
        {
            //draw a line cross through the polygon            
            int nsec = 0;
            IntersectionType ret = 0;
            CTriangle3f tri = new CTriangle3f();
            //collect all the triangles on the direction of p1p2;
            for (int i = 0; i < triangles.Count; i++)
            {
                // return 0-no intersection,1-intersected,
                // 2 -intersected on line or on cornerpoint

                tri.p1 = points[triangles[i].x].toVector64();
                tri.p2 = points[triangles[i].y].toVector64();
                tri.p3 = points[triangles[i].z].toVector64();

                ret = tri.CheckLineCrossTriangle(p1.toVector64(), p2.toVector64(), direct);
                if (ret == IntersectionType.none) continue;
                else if (ret == IntersectionType.triangle) nsec++;
                else //intersection on lines or on coner,can't determine the result
                {
                    return -1;
                }
            }
            // intersection points num is odd, 
            // even is outside polygon
            if (nsec > 0 && (nsec % 2) != 0) return 1;
            else return 0;
        }
    }
    public enum SphereSurface
    {
        UpLeftBack = 1,
        UpLeftFront = 2,
        UpRightBack = 4,
        UpRightFront = 8,
        DownLeftBack = 16,
        DownLeftFront = 32,
        DownRightBack = 64,
        DownRightFront = 128
    };
    public class C3DLine : C3DObjectBase
    {       
        public List<Vector64> points = new List<Vector64>();        
        public LineStyle lineStyle = new LineStyle(Color.Gray,1);
        public double[] distances = null;
        public double Value { get; set; } = 0;    // property value;
        public string headerLine = "3D Line 2100";

        [CategoryAttribute("Display"), DisplayNameAttribute("Color")]
        public Color Color
        {
            get { return lineStyle.Color;}
            set
            {
                lineStyle.Color = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        public bool _Closed = false; // is closed obj-polygon
        [CategoryAttribute("Display"), DisplayNameAttribute("Closed")]
        public bool Closed 
        {
            get { return _Closed; }
            set
            {
                _Closed = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }        

        public bool _EnableColorLevel = false;
        [CategoryAttribute("ColorMap"), DisplayNameAttribute("Enable")]
        public bool EnableColorLevel
        { 
            get { return _EnableColorLevel; }
            set { _EnableColorLevel = value;RenderMode = RenderingUpdateMode.Redraw; }
        }
        public bool IsColorScaleUpdated()
        {
            if (EnableColorLevel)
            {
                if (_ColorScale.DifferentFrom(_OlderColorScale))
                    return true;
            }
            return false;
        }
        public CColorScale _OlderColorScale = new CColorScale();
        public CColorScale _ColorScale = new CColorScale();
        [CategoryAttribute("ColorMap"), DisplayNameAttribute("ColorMap")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorScaleConverter))]
        public CColorScale ColorScale
        {
            get { return _ColorScale; }
            set
            {
                _OlderColorScale = _ColorScale;
                _ColorScale = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        
        public Color GetColor(double v) 
        {
            if ( EnableColorLevel ) return ColorScale.GetColor(v);
            else return Color;
        }

        [CategoryAttribute("Lines"), DisplayNameAttribute("Width")]
        public float lineWidth 
        {
            get { return lineStyle.Width; }
            set { lineStyle.Width = value;RenderMode = RenderingUpdateMode.Redraw; }
        }        
        /*
        [CategoryAttribute("DOT"), DisplayNameAttribute("Show Dot")]
        public bool IsShowDot { get; set; } = false;

        [CategoryAttribute("DOT"), DisplayNameAttribute("Dot Size")]
        public float dotSize { get; set; } = 5;
        [CategoryAttribute("DOT"), DisplayNameAttribute("Dot Color")]
        public Color dotColor { get; set; } = Color.Red;
        */

        public C3DLine(string _name = "untitled")
        {
            type = ShapeEnum.Line;
        }
        public C3DLine(List<Vector32> _points)
        {
            type = ShapeEnum.Line;
            for (int i = 0; i < _points.Count; i++)
            {
                points.Add(_points[i].toVector64());
            }
        }
        public C3DLine(List<Vector64> _points)
        {
            type = ShapeEnum.Line;
            for (int i = 0; i < _points.Count; i++)
            {
                points.Add(_points[i]);
            }
        }
        public int Count { get { return points.Count; } }
        public Vector64 this[int id]
        {
            get
            {
                return points[id];
            }
        }
        public void AddPoint(List<Vector64> points_array)
        {
            for (int i = 0; i < points_array.Count; i++)
                AddPoint(points_array[i]);
        }
        public void AddPoint(List<Vector32> points_array)
        {
            for (int i = 0; i < points_array.Count; i++)
                AddPoint(points_array[i]);
        }
        public void AddPoint(Vector64 p)
        {
            AddPoint(p.x, p.y, p.z, p.v);
        }
        public void Offset(double offx, double offy, double offz)
        {
            Vector64 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                p.x += offx;
                p.y += offy;
                p.z += offz;
                points[i] = p;
            }
            minx += offx;
            miny += offy;
            minz += offz;
            maxx += offx;
            maxy += offy;
            maxz += offz;
        }
        /// <summary>
        /// 简化点，将直线上冗余点去掉
        /// </summary>
        public void Simplify()
        {
            //去重点
            Vector64.RemoveLineDuplicated(ref points);
            Stack<Vector64> lists = new Stack<Vector64>();

            for (int i = Count - 1; i >= 0; i--)
                lists.Push(points[i]);

            points.Clear();

            //过滤直线冗余点
            Vector64 p1 = lists.Pop();
            Vector64 p2 = lists.Pop();
            Vector64 p;

            points.Add(p1);
            while (lists.Count > 0)
            {
                p = lists.Pop();
                if (Vector64.IsPointOnLine(p, p1, p2)) p2 = p;
                else
                {
                    points.Add(p2);
                    p1 = p2;
                    p2 = p;
                }
            }
            //last one
            points.Add(p2);
            UpdateRange();
        }
        public void AddPoint(double x, double y, double z, double v = 0)
        {
            if (Count == 0)
            {
                minx = maxx = x;
                miny = maxy = y;
                minz = maxz = z;
                minv = maxv = v;
            }
            else
            {
                if (x < minx) minx = x;
                if (y < miny) miny = y;
                if (z < minz) minz = z;
                if (v < minv) minv = v;
                if (x > maxx) maxx = x;
                if (y > maxy) maxy = y;
                if (z > maxz) maxz = z;
                if (v > maxv) maxv = v;
            }
            points.Add(new Vector64(x, y, z,v));
        }
        public void AddPoint(Vector32 p)
        {
            AddPoint(p.x, p.y, p.z, p.v);
        }

        public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            UpdateRange();
            Vector64 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (maxx > minx)
                    p.x = x1 + (x2 - x1) * (p.x - minx) / (maxx - minx);
                else p.x = (x1 + x2) / 2;
                if (maxy > miny)
                    p.y = y1 + (y2 - y1) * (p.y - miny) / (maxy - miny);
                else p.y = (y1 + y2) / 2;
                if (maxz > minz)
                    p.z = z1 + (z2 - z1) * (p.z - minz) / (maxz - minz);
                else p.z = (z1 + z2) / 2;
                points[i] = p;
            }
            minx = x1;
            miny = y1;
            minz = z1;
            maxx = x2;
            maxy = y2;
            maxz = z2;
        }
        public override void Normalize()
        {
            if (points.Count < 2) return;

            UpdateRange();

            Vector64 p0 = GetCenter64();
            Vector64 p = new Vector64(0, 0, 0);

            double x, y, z;
            for (int i = 0; i < points.Count; i++)
            {
                p.x = scale.x * (points[i].x - p0.x);
                p.y = scale.y * (points[i].y - p0.y);
                p.z = scale.z * (points[i].z - p0.z);

                //GL is clockwise,otherwise Rotate is counter clockwise
                //if (rotate.x != 0) p.RotateOnAngle(-rotate.x, 0,0);
                //if (rotate.y != 0) p.RotateOnAngle(0,rotate.y, 0);
                //if (rotate.z != 0) p.RotateOnAngle(0,0,-rotate.z);

                if (rotate.x != 0 || rotate.y != 0 || rotate.z != 0)
                    p.RotateOnAngle(-rotate.x, rotate.y, -rotate.z);

                x = (p.x + p0.x + offset.x);
                y = (p.y + p0.y + offset.y);
                z = (p.z + p0.z + offset.z);

                points[i] = new Vector64(x, y, z);
            }

            scale = new vec3(1, 1, 1);
            offset = new vec3(0, 0, 0);
            rotate = new vec3(0, 0, 0);

            UpdateRange();
        }
        public override void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;
            if (points.Count < 1) return;

            minx = maxx = points[0].x;
            miny = maxy = points[0].y;
            minz = maxz = points[0].z;            
            Vector64 p;
            for (int i = 1; i < points.Count; i++)
            {
                p = points[i];
                if (p.x < minx) minx = p.x;
                if (p.x > maxx) maxx = p.x;
                if (p.y < miny) miny = p.y;
                if (p.y > maxy) maxy = p.y;
                if (p.z < minz) minz = p.z;
                if (p.z > maxz) maxz = p.z;
            }
            int k = 0;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if ( IsBlankValue(p.V) ) continue;
                if(k == 0) { minv = maxv = p.V;k++; }
                else 
                {
                    if (p.v < minv) minv = p.V;
                    if (p.z > maxv) maxv = p.V;
                }
            }
        }
        /// <summary>
        /// 曲线顺序反转
        /// </summary>
        /// <returns></returns>
        public void OrderInverse()
        {
            List<Vector64> points1 = new List<Vector64>();

            for (int i = Count - 1; i >= 0; i--)
                points1.Add(points[i]);

            points.Clear();
            points = points1;
        }

        public override bool ExportData(string filename)
        {
            StreamWriter wr = new StreamWriter(new FileStream(filename, FileMode.Create));
            string str;
            
            wr.WriteLine(headerLine);
            
            str = "[HEADER]";
            wr.WriteLine(str);

            str = "Name = " + Name;
            wr.WriteLine(str);
            str = "Color = " + Color.ToArgb();
            wr.WriteLine(str);
            str = "Closed = " + Closed;
            wr.WriteLine(str);
            str = "PropertyValue = " + Value;
            wr.WriteLine(str);

            str = "[POINTS]";
            wr.WriteLine(str);
            str = "Count = " + points.Count;
            wr.WriteLine(str);

            for (int i = 0; i < points.Count; i++)
            {
                wr.WriteLine(points[i].toString(4));
            }
            
            str = "[Style]";
            wr.WriteLine(str);
            str = "Width = " + lineWidth;
            wr.WriteLine(str);

            ExportTranslations(wr);

            if ( EnableColorLevel )
            {
                str = "[ColorMap]";
                wr.WriteLine(str);
                ColorScale.WriteStream(wr);
            }
            wr.Close();
            return true;
        }
        public override bool ImportData(string filename)
        {
            try
            {                
                StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
                Name = Path.GetFileName(filename);

                Clear(); 
                
                if (AscIIProfile.ReadLine(sr) != headerLine)
                {
                    errMessage = "not a valid 3D line file.";
                    sr.Close();
                    return false;
                }

                if (!AscIIProfile.SeekSection("[HEADER]", ref sr))
                {
                    errMessage = "not a valid 3D line file.";
                    sr.Close();                    
                    return false;
                }

                Name = AscIIProfile.ReadStringValue(sr, "Name");
                Color = Color.FromArgb(AscIIProfile.ReadIntValue(sr, "Color"));
                Closed = AscIIProfile.ReadBoolValue(sr, "Closed");
                Value = AscIIProfile.ReadDoubleValue(sr, "PropertyValue");               

                if ( AscIIProfile.SeekSection("[POINTS]", ref sr) )
                {
                    points.Clear();
                    int count = 0;                    
                    AscIIProfile.ReadIntValue(sr, "Count", out count);
                    if( count > 0 && count < 1.0E10)
                    {
                        for(int i=0;i<count;i++)
                        {
                            AddPoint(Vector64.Parse(AscIIProfile.ReadLine(sr), 4));
                        }
                    }
                }
                if (AscIIProfile.SeekSection("[Style]", ref sr))
                {
                    AscIIProfile.ReadFloatValue(sr, "Width", out lineStyle.Width);
                }
                if (AscIIProfile.SeekSection("[TRANSLATIONS]", ref sr))
                {
                    ImportTranslations(sr);
                }
                else UpdateRange();

                if (AscIIProfile.SeekSection("[ColorMap]", ref sr))
                {
                    ColorScale = new CColorScale(minv, maxv);
                    ColorScale.ReadStream(ref sr);
                    ColorScale.SetValueRange(minv, maxv);
                }
                sr.Close();               
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if ( !SaveObjHeader(br) ) return false;
            try
            {   
                br.Write(Value);
                br.Write(_Closed);
                br.Write(EnableColorLevel);
                lineStyle.SaveAs(br);
                br.Write(Count);
                foreach(Vector64 p in points)
                {
                    br.Write(p.X);
                    br.Write(p.Y);
                    br.Write(p.Z);
                    br.Write(p.V);
                }
                _ColorScale.WriteBinary(br);
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public override bool LoadFrom(BinaryReader br)
        {
            if ( !LoadObjHeader(br) ) return false;
            double x, y, z, v;
            try
            {
                Value = br.ReadDouble();
                Closed = br.ReadBoolean();
                EnableColorLevel = br.ReadBoolean();
                lineStyle.LoadFrom(br);
                int n = br.ReadInt32();
                points.Clear();
                for(int i = 0;i<n;i++)
                {
                    x = br.ReadDouble();
                    y = br.ReadDouble();
                    z = br.ReadDouble();
                    v = br.ReadDouble();
                    points.Add(new Vector64(x,y,z,v));
                }
                ColorScale.LoadBinary(br);
                UpdateRange();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public override void Clear()
        {
            base.Clear();
            points.Clear();
        }
        
        public C3DLine Copy()
        {
            C3DLine line = new C3DLine(points);
            line.CopyHeaderFrom(this);

            line.EnableColorLevel = EnableColorLevel;
            line.lineStyle = lineStyle;
            line.Closed = Closed;
            line.Value = Value;
            return line;
        }

        //sect < = p1p2
        private Vector64 GetInterpolate(double sect, Vector64 p1, Vector64 p2)
        {
            if (sect == 0) return p1;

            double len = p1.Distance(p2);

            if (sect >= len) return p2;
            else return p1 + (p2 - p1) * sect / len;
        }
        /// <summary>
        /// 曲线重分段，重采样
        /// </summary>
        /// <param name="ptNum">采样点数</param>        
        /// <returns>采样后的曲线</returns>
        //
        public C3DLine Resample(int ptNum = 101)
        {
            C3DLine line = Copy();
            line.Name = Name + "_Resampled";
            line.points.Clear();

            //采样间隔
            double step = GetLength() / (ptNum - 1);

            //第1点
            line.AddPoint(points[0]);

            Vector64 p, p1, p2;
            double leftstep = step, len;
            p1 = points[0];
            int k = 1;
            for (int i = 0; i < ptNum - 2; i++) //中间的采样点数
            {
                while (k < Count)
                {
                    p2 = points[k];
                    len = p2.Distance(p1);//线段长度
                    if (len >= leftstep) //插值求点
                    {
                        p = p1 + (p2 - p1) * leftstep / len;
                        line.AddPoint(p);
                        p1 = p;
                        leftstep = step;
                        break;
                    }
                    else
                    {
                        k++; //下一点
                        p1 = p2;
                        leftstep -= len;
                    }
                }
            }

            //最后一点
            line.AddPoint(points[Count - 1]);

            return line;
        }//Resample

        /// <summary>
        /// 三维空间曲线平滑
        /// </summary>
        /// <returns></returns>
        // 分段光滑似乎不可行，只能将曲线投影到一个平面上
        public C3DLine Smooth()
        {
            int n = points.Count;
            if (n < 3) return this;

            C3DLine line = new C3DLine();
            line.Name = Name + "_smoothed";

            int n1, n2;
            Vector64 p, p1, p2;
            Vector64[] pt = new Vector64[points.Count];

            double xlen = maxx - minx;
            double ylen = maxy - miny;
            double zlen = maxz - minz;
            double len, len1;

            //将曲线投影到平面，然后再进行平滑
            if (xlen <= ylen && xlen <= zlen) //project to YOZ
            {
                for (int i = 0; i < points.Count; i++)
                {
                    pt[i] = new Vector64(points[i].y, points[i].z, 0);
                }

                Spline sp = new Spline(pt);
                List<Vector64> pps = sp.CreateSpline();
                for (int i = 0; i < pps.Count; i++)
                {
                    p = new Vector64(0, pps[i].x, pps[i].y);
                    p.x = points[0].x;
                    n1 = n2 = 0;
                    for (int j = 0; j < sp.gridIndics.Length; j++)
                    {
                        if (i == sp.gridIndics[j])
                        {
                            n1 = n2 = j;
                            break;
                        }
                        else if (i < sp.gridIndics[j])
                        {
                            n2 = j;
                            break;
                        }
                        else n1 = j;//if (i > sp.gridIndics[j])                        
                    }

                    if (n1 == n2) p.x = points[n1].x;
                    else
                    {
                        p1 = points[n1];
                        p2 = points[n2];
                        len = Math.Sqrt((p.y - p1.y) * (p.y - p1.y) + (p.z - p1.z) * (p.z - p1.z));
                        len1 = Math.Sqrt((p2.y - p1.y) * (p2.y - p1.y) + (p2.z - p1.z) * (p2.z - p1.z));
                        if (len1 > 0) p.x = p1.x + (p2.x - p1.x) * len / len1;
                        else p.x = p1.x;
                    }
                    line.points.Add(p);
                }// for (int i = 0; i < pps.Count; i++)

                pps.Clear();
                sp.Clear();

            }
            else if (ylen <= xlen && ylen <= zlen) //project to XOZ
            {
                for (int i = 0; i < points.Count; i++)
                {
                    pt[i] = new Vector64(points[i].x, points[i].z, 0);
                }
                Spline sp = new Spline(pt);
                List<Vector64> pps = sp.CreateSpline();
                for (int i = 0; i < pps.Count; i++)
                {
                    p = new Vector64(pps[i].x, 0, pps[i].y);
                    p.y = points[0].y;
                    n1 = n2 = 0;
                    for (int j = 0; j < sp.gridIndics.Length; j++)
                    {
                        if (i == sp.gridIndics[j])
                        {
                            n1 = n2 = j;
                            break;
                        }
                        else if (i < sp.gridIndics[j])
                        {
                            n2 = j;
                            break;
                        }
                        else n1 = j;//if (i > sp.gridIndics[j])                        
                    }

                    if (n1 == n2) p.y = points[n1].y;
                    else
                    {
                        p1 = points[n1];
                        p2 = points[n2];
                        len = Math.Sqrt((p.x - p1.x) * (p.x - p1.x) + (p.z - p1.z) * (p.z - p1.z));
                        len1 = Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.z - p1.z) * (p2.z - p1.z));
                        if (len1 > 0) p.y = p1.y + (p2.y - p1.y) * len / len1;
                        else p.y = p1.y;
                    }
                    line.points.Add(p);
                }// for (int i = 0; i < pps.Count; i++)

                pps.Clear();
                sp.Clear();
            }
            else    //project to XOY
            {
                for (int i = 0; i < points.Count; i++)
                {
                    pt[i] = new Vector64(points[i].x, points[i].y, 0);
                }
                Spline sp = new Spline(pt);
                List<Vector64> pps = sp.CreateSpline();

                for (int i = 0; i < pps.Count; i++)
                {
                    p = pps[i];
                    p.z = points[0].z;
                    n1 = n2 = 0;
                    for (int j = 0; j < sp.gridIndics.Length; j++)
                    {
                        if (i == sp.gridIndics[j])
                        {
                            n1 = n2 = j;
                            break;
                        }
                        else if (i < sp.gridIndics[j])
                        {
                            n2 = j;
                            break;
                        }
                        else n1 = j;//if (i > sp.gridIndics[j])                        
                    }

                    if (n1 == n2) p.z = points[n1].z;
                    else
                    {
                        p1 = points[n1];
                        p2 = points[n2];
                        len = Math.Sqrt((p.x - p1.x) * (p.x - p1.x) + (p.y - p1.y) * (p.y - p1.y));
                        len1 = Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
                        if (len1 > 0) p.z = p1.z + (p2.z - p1.z) * len / len1;
                        else p.z = p1.z;
                    }
                    line.points.Add(p);
                }// for (int i = 0; i < pps.Count; i++)

                pps.Clear();
                sp.Clear();

            }//else  //project to XOY

            pt = null;
            line.UpdateRange();
            return line;
        }//end of smooth()

        public C3DLine Smooth1()
        {
            int n = points.Count;
            if (n < 3) return this;

            C3DLine line = new C3DLine();
            line.Name = Name + "_smoothed";

            int n1, n2;
            Vector64 p, p1, p2;
            Vector64[] pt = new Vector64[points.Count];

            double xlen = maxx - minx;
            double ylen = maxy - miny;
            double zlen = maxz - minz;
            double len, len1;

            //将曲线投影到平面，然后再进行平滑
            if (xlen <= ylen && xlen <= zlen) //project to YOZ
            {
                for (int i = 0; i < points.Count; i++)
                {
                    pt[i] = new Vector64(points[i].y, points[i].z, 0);
                }

                Spline sp = new Spline(pt);
                List<Vector64> pps = sp.CreateSpline();
                for (int i = 0; i < pps.Count; i++)
                {
                    p = new Vector64(0, pps[i].x, pps[i].y);
                    p.x = points[0].x;
                    n1 = n2 = 0;
                    for (int j = 0; j < sp.gridIndics.Length; j++)
                    {
                        if (i == sp.gridIndics[j])
                        {
                            n1 = n2 = j;
                            break;
                        }
                        else if (i < sp.gridIndics[j])
                        {
                            n2 = j;
                            break;
                        }
                        else n1 = j;//if (i > sp.gridIndics[j])                        
                    }

                    if (n1 == n2) p.x = points[n1].x;
                    else
                    {
                        p1 = points[n1];
                        p2 = points[n2];
                        len = Math.Sqrt((p.y - p1.y) * (p.y - p1.y) + (p.z - p1.z) * (p.z - p1.z));
                        len1 = Math.Sqrt((p2.y - p1.y) * (p2.y - p1.y) + (p2.z - p1.z) * (p2.z - p1.z));
                        if (len1 > 0) p.x = p1.x + (p2.x - p1.x) * len / len1;
                        else p.x = p1.x;
                    }
                    line.points.Add(p);
                }// for (int i = 0; i < pps.Count; i++)

                pps.Clear();
                sp.Clear();

            }
            else if (ylen <= xlen && ylen <= zlen) //project to XOZ
            {
                for (int i = 0; i < points.Count; i++)
                {
                    pt[i] = new Vector64(points[i].x, points[i].z, 0);
                }
                Spline sp = new Spline(pt);
                List<Vector64> pps = sp.CreateSpline();
                for (int i = 0; i < pps.Count; i++)
                {
                    p = new Vector64(pps[i].x, 0, pps[i].y);
                    p.y = points[0].y;
                    n1 = n2 = 0;
                    for (int j = 0; j < sp.gridIndics.Length; j++)
                    {
                        if (i == sp.gridIndics[j])
                        {
                            n1 = n2 = j;
                            break;
                        }
                        else if (i < sp.gridIndics[j])
                        {
                            n2 = j;
                            break;
                        }
                        else n1 = j;//if (i > sp.gridIndics[j])                        
                    }

                    if (n1 == n2) p.y = points[n1].y;
                    else
                    {
                        p1 = points[n1];
                        p2 = points[n2];
                        len = Math.Sqrt((p.x - p1.x) * (p.x - p1.x) + (p.z - p1.z) * (p.z - p1.z));
                        len1 = Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.z - p1.z) * (p2.z - p1.z));
                        if (len1 > 0) p.y = p1.y + (p2.y - p1.y) * len / len1;
                        else p.y = p1.y;
                    }
                    line.points.Add(p);
                }// for (int i = 0; i < pps.Count; i++)

                pps.Clear();
                sp.Clear();
            }
            else    //project to XOY
            {
                for (int i = 0; i < points.Count; i++)
                {
                    pt[i] = new Vector64(points[i].x, points[i].y, 0);
                }
                Spline sp = new Spline(pt);
                List<Vector64> pps = sp.CreateSpline();

                for (int i = 0; i < pps.Count; i++)
                {
                    p = pps[i];
                    p.z = points[0].z;
                    n1 = n2 = 0;
                    for (int j = 0; j < sp.gridIndics.Length; j++)
                    {
                        if (i == sp.gridIndics[j])
                        {
                            n1 = n2 = j;
                            break;
                        }
                        else if (i < sp.gridIndics[j])
                        {
                            n2 = j;
                            break;
                        }
                        else n1 = j;//if (i > sp.gridIndics[j])                        
                    }

                    if (n1 == n2) p.z = points[n1].z;
                    else
                    {
                        p1 = points[n1];
                        p2 = points[n2];
                        len = Math.Sqrt((p.x - p1.x) * (p.x - p1.x) + (p.y - p1.y) * (p.y - p1.y));
                        len1 = Math.Sqrt((p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y));
                        if (len1 > 0) p.z = p1.z + (p2.z - p1.z) * len / len1;
                        else p.z = p1.z;
                    }
                    line.points.Add(p);
                }// for (int i = 0; i < pps.Count; i++)

                pps.Clear();
                sp.Clear();

            }//else  //project to XOY

            pt = null;
            line.UpdateRange();
            return line;
        }//end of smooth()
        /// <summary>
        /// GetLength Functions
        /// </summary>
        public void CreateDistances()
        {
            if (distances != null) return;
            if (points.Count < 2) return;

            distances = new double[points.Count];

            double len = 0;
            Vector64 p, p0 = points[0];

            distances[0] = 0;
            for (int i = 1; i < points.Count; i++)
            {
                p = points[i];
                len += p.Distance(p0);
                p0 = p;
                distances[i] = len;
            }
        }

        public double GetLength(int index = -1)
        {
            if (points.Count < 1) return 0;

            int id = index;
            if (id < 0) id = Count - 1;

            CreateDistances();

            return distances[id];
        }

        public double Length
        {
            get { return GetLength(); }
        }

        public double GetLength(List<Vector64> pp, int index = -1)
        {
            if (index == 0) return 0;
            int end = pp.Count - 1;
            if (index >= 0) end = index;

            Vector32 p1, p2;
            double sum = 0, len;
            for (int i = 1; i <= end; i++)
            {
                p1 = pp[i - 1];
                p2 = pp[i];
                len = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                            (p1.y - p2.y) * (p1.y - p2.y) +
                            (p1.z - p2.z) * (p1.z - p2.z));
                sum += len;
            }
            return sum;
        }

        /*
         public C3DLine Smooth()
         {
             int n = points.Count;
             if (n < 3) return this;

             C3DLine line = Copy();
             line.name = name + "_smoothed";
             line.Clear();

             double x1, x2, y1, y2, z1, z2,xl,yl,zl;
             double px1, px2, py1, py2;
             Vector32 p1, p2, p3;            
             Vector32[] pt = new Vector32[3];

             for (int k=0;k<n-2;k++)
             {
                 p1 = points[k];
                 p2 = points[k+1];
                 p3 = points[k+2];

                 x1 = x2 = p1.x;
                 y1 = y2 = p1.y;
                 z1 = z2 = p1.z;
                 if (p2.x < x1) x1 = p2.x;
                 if (p2.x > x2) x2 = p2.x;
                 if (p2.y < y1) y1 = p2.y;
                 if (p2.y > y2) y2 = p2.y;
                 if (p2.z < z1) z1 = p2.z;
                 if (p2.z > z2) z2 = p2.z;
                 if (p3.x < x1) x1 = p3.x;
                 if (p3.x > x2) x2 = p3.x;
                 if (p3.y < y1) y1 = p3.y;
                 if (p3.y > y2) y2 = p3.y;
                 if (p3.z < z1) z1 = p3.z;
                 if (p3.z > z2) z2 = p3.z;
                 xl = x2 - x1;
                 yl = y2 - y1;
                 zl = z2 - z1;

                 if (x1 <= yl && xl <= z1) //project to YOZ
                 {
                     pt[0] = new Vector32(p1.y, p1.z, 0);
                     pt[1] = new Vector32(p2.y, p2.z, 0);
                     pt[2] = new Vector32(p3.y, p3.z, 0);
                     Spline sp = new Spline(pt);
                     List<Vector32> pps = sp.CreateSpline();

                     double yy1 = Math.Abs(p2.y - p1.y);
                     double yy2 = Math.Abs(p3.y - p2.y);
                     double zz1 = Math.Abs(p2.z - p1.z);
                     double zz2 = Math.Abs(p3.z - p2.z);

                     px1 = p1.y; px2 = p2.y;
                     if (px2 < px1) { px1 = p2.y; px2 = p1.y; }
                     py1 = p1.z; py2 = p2.z;
                     if (py2 < py1) { py1 = p2.z; py2 = p1.z; }

                     double dx = 0;
                     for (int i = 0; i < pps.Count; i++)
                     {
                         //p1--p2
                         if ((pps[i].x >= px1 && pps[i].x < px2) &&
                             (pps[i].y >= py1 && pps[i].y < py2))
                         {
                             if (k == 0)//first fragment
                             {
                                 if (yy1 >= zz1)//y                             
                                     dx = p1.x + (p2.x - p1.x) * (pps[i].x - p1.y) / (p2.y - p1.y);
                                 else //z                            
                                     dx = p1.x + (p2.x - p1.x) * (pps[i].y - p1.z) / (p2.z - p1.z);
                                 line.points.Add(new Vector32((float)dx, pps[i].x, pps[i].y));
                             }
                         }
                         else//p2--p3
                         {
                             if ((i == pps.Count - 1 && k == n - 3) || i < pps.Count - 1)
                             {
                                 if (yy2 >= zz2)//y                             
                                     dx = p2.x + (p3.x - p2.x) * (pps[i].x - p2.y) / (p3.y - p2.y);
                                 else //z                            
                                     dx = p2.x + (p3.x - p2.x) * (pps[i].y - p2.z) / (p3.z - p2.z);
                                 line.points.Add(new Vector32((float)dx, pps[i].x, pps[i].y));
                             }
                         }


                     }
                     pps.Clear();
                 }
                 else if (y1 <= xl && yl <= z1) //project to XOZ
                 {
                     pt[0] = new Vector32(p1.x, p1.z, 0);
                     pt[1] = new Vector32(p2.x, p2.z, 0);
                     pt[2] = new Vector32(p3.x, p3.z, 0);
                     Spline sp = new Spline(pt);
                     List<Vector32> pps = sp.CreateSpline();

                     double xx1 = Math.Abs(p2.x - p1.x);
                     double xx2 = Math.Abs(p3.x - p2.x);
                     double zz1 = Math.Abs(p2.z - p1.z);
                     double zz2 = Math.Abs(p3.z - p2.z);

                     px1 = p1.x; px2 = p2.x;
                     if (px2 < px1) { px1 = p2.x; px2 = p1.x; }
                     py1 = p1.z; py2 = p2.z;
                     if (py2 < py1) { py1 = p2.z; py2 = p1.z; }

                     double dy = 0;
                     for (int i = 0; i < pps.Count; i++)
                     {
                         //p1--p2
                         if ((pps[i].x >= px1 && pps[i].x < px2) &&
                             (pps[i].y >= py1 && pps[i].y < py2))
                         {
                             if (k == 0)
                             {
                                 if (xx1 >= zz1)//x                             
                                     dy = p1.y + (p2.y - p1.y) * (pps[i].x - p1.x) / (p2.x - p1.x);
                                 else //z                            
                                     dy = p1.y + (p2.y - p1.y) * (pps[i].z - p1.z) / (p2.z - p1.z);
                                 line.points.Add(new Vector32(pps[i].x, (float)dy, pps[i].y));
                             }
                         }
                         else//p2--p3
                         {
                             if ((i == pps.Count - 1 && k == n - 3) || i < pps.Count - 1)
                             {
                                 if (xx2 >= zz2)//x                             
                                     dy = p2.y + (p3.y - p2.y) * (pps[i].x - p2.x) / (p3.x - p2.x);
                                 else //z                            
                                     dy = p2.y + (p3.y - p2.y) * (pps[i].z - p2.z) / (p3.z - p2.z);
                                 line.points.Add(new Vector32(pps[i].x, (float)dy, pps[i].y));
                             }
                         }

                     }
                     pps.Clear();
                 }
                 else //if (z1 <= xl && zl <= y1) //project to XOY
                 {
                     pt[0] = new Vector32(p1.x, p1.y, 0);
                     pt[1] = new Vector32(p2.x, p2.y, 0);
                     pt[2] = new Vector32(p3.x, p3.y, 0);
                     Spline sp = new Spline(pt);
                     List<Vector32> pps = sp.CreateSpline();

                     double xx1 = Math.Abs(p2.x - p1.x);
                     double xx2 = Math.Abs(p3.x - p2.x);
                     double yy1 = Math.Abs(p2.y - p1.y);
                     double yy2 = Math.Abs(p3.y - p2.y);

                     px1 = p1.x; px2 = p2.x;
                     if (px2 < px1) { px1 = p2.x; px2 = p1.x; }
                     py1 = p1.y; py2 = p2.y;
                     if (py2 < py1) { py1 = p2.y; py2 = p1.y; }

                     double dz = 0;
                     for (int i = 0; i < pps.Count; i++)
                     {
                         //p1--p2
                         if ((pps[i].x >= px1 && pps[i].x < px2) &&
                             (pps[i].y >= py1 && pps[i].y < py2))
                         {
                          //   if (k == 0)
                             {
                                 if (xx1 >= yy1)//x                             
                                     dz = p1.z + (p2.z - p1.z) * (pps[i].x - p1.x) / (p2.x - p1.x);
                                 else //y                            
                                     dz = p1.z + (p2.z - p1.z) * (pps[i].y - p1.y) / (p2.y - p1.y);
                                 line.points.Add(new Vector32(pps[i].x, pps[i].y, (float)dz));
                             }
                         }
                         else //p2--p3
                         {
                            // if ((i == pps.Count - 1 && k == n - 3) || i < pps.Count - 1)
                             {
                                 if (xx2 >= yy2)//x                             
                                     dz = p2.z + (p3.z - p2.z) * (pps[i].x - p2.x) / (p3.x - p2.x);
                                 else //y                            
                                     dz = p2.z + (p3.z - p2.z) * (pps[i].y - p2.y) / (p3.y - p2.y);
                            //     line.points.Add(new Vector32(pps[i].x, pps[i].y, (float)dz));
                             }
                         }   

                     }
                     pps.Clear();
                 }
             }

             return line;
         }//end of smooth()
         */
    }//end of class C3DLine

    public class CSphere : TriangleObj
    {
        public double rad = 1.0;
        
        public Vector32 top = new Vector32(0, 1, 0);
        public Vector32 bottom = new Vector32(0, -1, 0);
        public Vector32 left = new Vector32(-1, 0, 0);
        public Vector32 right = new Vector32(1, 0, 0);
        public Vector32 front = new Vector32(0, 0, 1);
        public Vector32 back = new Vector32(0, 0, -1);
        public CSphere(float x,float y,float z)
        {
            points.Add(top);
            points.Add(bottom);
            points.Add(left);
            points.Add(right);
            points.Add(front);
            points.Add(back);
            type = ShapeEnum.Shphere;
            minSquare = 0.001;
            offset = new vec3(x,y,z);
        }
        //generate Texcoords
        public void GenTexcoords()
        {
            int n = points.Count;
            if (n < 1) return;
            float x, y, z, u, v;

            for (int i = 0; i < n; i++)
            {
                x = points[i].X;
                y = points[i].Y;
                z = points[i].Z;
                v = (float)(Math.Asin(z / rad) / Math.PI + 0.5);
                u = (float)(Math.Atan(y / x) / 2 / Math.PI);
            }
        }
        public void CreateHalf(double _rad, double _minSquare, bool top = true)
        {
            if (_rad <= 0) return;
            if (minSquare >= 0.1) return;
            rad = _rad;
            minSquare = _minSquare;
            if (top)
            {
                CreateHalf8(1);
                CreateHalf8(2);
                CreateHalf8(4);
                CreateHalf8(8);
            }
            else
            {
                CreateHalf8(16);
                CreateHalf8(32);
                CreateHalf8(64);
                CreateHalf8(128);
            }
        }
        public void Create(double _rad, double _minSquare)
        {
            CreateHalf(_rad, _minSquare, true);
            CreateHalf(_rad, _minSquare, false);
            Vector32 p;            
            for (int i = 0; i < points.Count; i++)
            {
                p = (float)_rad * points[i];
                p = TransformedPoint(p);
                points[i] = p;                
            }
        }
        public void CreateHalf8(int surface = 1)
        {
            int start = points.Count;
            if ((surface & (int)SphereSurface.UpLeftBack) > 0)
            {
                start = points.Count;
                //points.Add(top);  0
                //points.Add(back); 5
                //points.Add(left); 2
                triangles.Add(new Int32XYZ(0, 5, 2));
                DividTriangle(triangles.Count - 1);
            }
            if ((surface & (int)SphereSurface.UpLeftFront) > 0)
            {
                start = points.Count;
                //points.Add(top);  0 2 4
                //points.Add(left);
                //points.Add(front);                
                triangles.Add(new Int32XYZ(0, 2, 4));
                DividTriangle(triangles.Count - 1);
            }
            if ((surface & (int)SphereSurface.UpRightBack) > 0)
            {
                start = points.Count;
                //points.Add(top);
                //points.Add(right);
                //points.Add(back);
                triangles.Add(new Int32XYZ(0, 3, 5));
                DividTriangle(triangles.Count - 1);
            }
            if ((surface & (int)SphereSurface.UpRightFront) > 0)
            {
                start = points.Count;
                //points.Add(top);
                //points.Add(front);
                //points.Add(right);
                triangles.Add(new Int32XYZ(0, 4, 3));
                DividTriangle(triangles.Count - 1);
            }
            /////////////////////////down////////////////////////
            if ((surface & (int)SphereSurface.DownLeftBack) > 0)
            {
                start = points.Count;
                //points.Add(bottom);
                //points.Add(left);
                //points.Add(back);                
                triangles.Add(new Int32XYZ(1, 2, 5));
                DividTriangle(triangles.Count - 1);
            }
            if ((surface & (int)SphereSurface.DownLeftFront) > 0)
            {
                start = points.Count;
                //points.Add(bottom);                
                //points.Add(front);
                //points.Add(left);
                triangles.Add(new Int32XYZ(1, 4, 2));
                DividTriangle(triangles.Count - 1);
            }
            if ((surface & (int)SphereSurface.DownRightBack) > 0)
            {
                start = points.Count;
                //points.Add(bottom);
                //points.Add(back);
                //points.Add(right);                
                triangles.Add(new Int32XYZ(1, 5, 3));
                DividTriangle(triangles.Count - 1);
            }
            if ((surface & (int)SphereSurface.DownRightFront) > 0)
            {
                start = points.Count;
                //points.Add(bottom);
                //points.Add(right);
                //points.Add(front);                
                triangles.Add(new Int32XYZ(1, 3, 4));
                DividTriangle(triangles.Count - 1);
            }
        }
        public override void DividTriangle(int index)
        {
            //      p1
            //    / |  \
            // p2 - p0- p3
            int i1 = triangles[index].x;
            int i2 = triangles[index].y;
            int i3 = triangles[index].z;
            Vector32 p1 = points[i1];
            Vector32 p2 = points[i2];
            Vector32 p3 = points[i3];
            Vector32 p0 = (p2 + p3) / 2;
            double r01 = Vector32.Distance(p0, p1);
            double r23 = Vector32.Distance(p2, p3);

            //triangle meet minimum square requirment
            if (0.5 * r01 * r23 <= minSquare) return;

            Vector32 p12 = GetMiddlePoint(p1, p2);
            Vector32 p23 = GetMiddlePoint(p2, p3);
            Vector32 p13 = GetMiddlePoint(p1, p3);

            int i12 = points.Count;
            int i23 = i12 + 1;
            int i13 = i23 + 1;
            //new points
            points.Add(p12);
            points.Add(p23);
            points.Add(p13);

            Int32XYZ d0 = new Int32XYZ(i12, i23, i13);
            Int32XYZ d1 = new Int32XYZ(i1, i12, i13);
            Int32XYZ d2 = new Int32XYZ(i12, i2, i23);
            Int32XYZ d3 = new Int32XYZ(i13, i23, i3);
            triangles[index] = d0;
            int i0 = triangles.Count;

            triangles.Add(d1);
            triangles.Add(d2);
            triangles.Add(d3);

            DividTriangle(index);
            DividTriangle(i0);
            DividTriangle(i0 + 1);
            DividTriangle(i0 + 2);
        }
        public override Vector32 GetMiddlePoint(Vector32 p1, Vector32 p2)
        {
            Vector32 p = (p1 + p2) / 2;
            p = p.Normalize();
            return p;
        }
    }
    
    public class Cone : Symbol3D
    {
        [CategoryAttribute("Cone"), DisplayNameAttribute("Radius"), Browsable(true)]
        public override double Rad { get; set; } = 1.0; //半径
        [CategoryAttribute("Cone"), DisplayNameAttribute("Height"), Browsable(true)]
        public override double Height { get; set; } = 5.0;//高度
        [CategoryAttribute("Cone"), DisplayNameAttribute("Vertical Slices"), Browsable(true)]
        public override int vertSlices { get; set; } = 30;//横向剖分网格数
        [CategoryAttribute("Cone"), DisplayNameAttribute("Horizontal Slices"), Browsable(true)]
        public override int horSlices { get; set; } = 20;//垂向剖分网格数        
           
        public Vector32 Start = new Vector32();

        [CategoryAttribute("Cone"), DisplayNameAttribute("Top"), Browsable(true)]
        [Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(PropertyStyleConverter))]
        public TriangleObj TopFace
        {
            get
            {
                if (Faces.Count == 2) return Faces[0];
                else return null;
            }
            set
            {
                if (Faces.Count == 2)
                {
                    Faces[0] = value;
                }
            }
        }
        [CategoryAttribute("Cone"), DisplayNameAttribute("Body"), Browsable(true)]
        [Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(PropertyStyleConverter))]
        public TriangleObj BodyFace 
        {
            get 
            {
                if (Faces.Count == 2) return Faces[1];
                else return null;
            }
            set 
            {
                if (Faces.Count == 2)
                {
                    Faces[1] = value;
                }
            }
        }
        void InitFaces()
        {
            TriangleObj obj1 = new TriangleObj("Top");
            obj1.IsUniformColor = true;        
            Faces.Add(obj1);

            TriangleObj obj2 = new TriangleObj("Body");
            obj2.IsUniformColor = true;           
            Faces.Add(obj2);
        }
        public Cone()
        {           
            symbolType = SymbolEnum.Cone;
            type = ShapeEnum.Shape;
            InitFaces();
        }        

        public Cone(Vector32 p0, double _rad, double _height, int _verSlices = 20, int _horSlices = 20 )
        {
            Start = p0;
            Rad = _rad;
            Height = _height;
            vertSlices = _verSlices;
            horSlices = _horSlices;
            symbolType = SymbolEnum.Cone;
            type = ShapeEnum.Shape;
            InitFaces();
        }

        void CreateBody()
        {
            BodyFace.Clear();
            //if ( !BodyFace.Visible ) return;

            if (vertSlices < 2 || horSlices < 2) return;
            double zstep = Height / vertSlices;
            double x, y, z, a;

            int rounds = horSlices + 1;
            double angleStep = 2.0 * Math.PI / horSlices;
            double radstep = Rad / vertSlices;
            vec2 tex;

            //p0 - bottom--first point锥体顶点
            BodyFace.points.Add(new Vector32(Start.X, Start.Y, Start.Z - Height / 2.0));
            BodyFace.texCoords.Add(new vec2(0.5f, 0.5f));

            //Circle from bottom to top: 0 --> maximum hight
            for (int i = 1; i <= vertSlices; i++)
            {
                z = -Height / 2.0 + i * zstep;
                for (int j = 0; j <= horSlices; j++)
                {
                    a = j * angleStep;
                    if (j == horSlices) a = 0;
                    
                    x = radstep * i * Math.Cos(a);
                    y = radstep * i * Math.Sin(a);
                    
                    BodyFace.points.Add( new Vector32(x + Start.X, y + Start.Y, z + Start.Z));
                    tex = new vec2((float)j / (horSlices), (float)i / vertSlices);
                    BodyFace.texCoords.Add(tex);
                }
            }

            //indices
            int id1, id2, id3, id4;
            //bottom- 锥底id=0 到第一圈
            id1 = 0;
            for (int j = 0; j <= horSlices; j++)
            {
                id2 = 1 + j;
                id3 = id2 + 1;
                if (j == horSlices) id3 = 1;
                BodyFace.AddTriangleIndex(id1, id3, id2);
            }

            //body 第一圈 到 顶圈
            for (int i = 0; i < vertSlices-1; i++)
            {
                for (int j = 0; j < rounds; j++)
                {
                    id1 = i * rounds + 1 + j;
                    id2 = id1 + 1;
                    if (j == horSlices) id2 = i * rounds + 1;
                    id3 = id1 + rounds;
                    id4 = id2 + rounds;
                    BodyFace.AddTriangleIndex(id1, id2, id3);
                    BodyFace.AddTriangleIndex(id4, id3, id2);
                }
            }
        }
        void CreateTop()
        {
            TopFace.Clear();
            //if ( !TopFace.Visible ) return;

            if (vertSlices < 2 || horSlices < 2) return;
            double x, y, a;
            double angleStep = 2.0 * Math.PI / horSlices;
            vec2 tex;

            //top face
            for (int j = 0; j <= horSlices; j++)
            {
                a = j * angleStep;
                if (j == horSlices) a = 0;
                x = Rad * Math.Cos(a);
                y = Rad * Math.Sin(a);
                TopFace.points.Add(new Vector32(x + Start.X, y + Start.Y, Height / 2.0 + Start.Z));
                tex = new vec2((float)(1 + x / Rad) / 2, (float)(1 + y / Rad) / 2);
                TopFace.texCoords.Add(tex);
            }

            //p0 of Top face
            TopFace.points.Add(new Vector32(Start.x, Start.y, Start.z + Height / 2.0));
            TopFace.texCoords.Add(new vec2(0.5f, 0.5f));

            int id1, id2, id3;
            id1 = TopFace.points.Count - 1;
            for (int j = 0; j <= horSlices; j++)
            {
                id2 = j;
                id3 = id2 + 1;
                if (j == horSlices) id3 = 0;
                TopFace.AddTriangleIndex(id1, id3, id2);
            }
        }
        public override bool Create()
        {
            CreateBody();
            CreateTop();
            UpdateRange();
            return true;
        }
        
    }//Cone
    public class Arrow3D : Cone
    {

    }
    public class Arrow2D : Symbol3D
    {
        public Vector64[] aptArrowHead = new Vector64[3];
        public Vector64 Start, End; //baseline        
        public double Width { get; set; } = 15;
        public double Theta { get; set; } = 0.5;

        [CategoryAttribute("Arrow"), DisplayNameAttribute("Head"), Browsable(true)]
        public TriangleObj HeadFace
        {
            get
            {
                if (Faces.Count == 2) return Faces[0];
                else return null;
            }
            set
            {
                if (Faces.Count == 2)
                {
                    Faces[0] = value;
                }
            }
        }
        [CategoryAttribute("Arrow"), DisplayNameAttribute("Body"), Browsable(true)]
        public TriangleObj BodyFace
        {
            get
            {
                if (Faces.Count == 2) return Faces[1];
                else return null;
            }
            set
            {
                if (Faces.Count == 2)
                {
                    Faces[1] = value;
                }
            }
        }

        public Arrow2D()
        {
            type = ShapeEnum.Shape;
            symbolType = SymbolEnum.Arrow;
            TriangleObj obj = new TriangleObj();
            obj.Name = "Head";
            Faces.Add(obj);
            obj = new TriangleObj();
            obj.Name = "Body";
            Faces.Add(obj);
        }

        static public int Size{ get{ return sizeof(double) * 4 * 5; } }

        public void Create(Vector64 start, double length, double angle)
        {
            HeadFace.Clear();
            BodyFace.Clear();

            // O----->X(north)
            // |\
            // | \
            // Y(east)
            Vector64 p1 = start;
            p1.x += offset.x;
            p1.y += offset.y;
            p1.z += offset.z;

            double dx = length * Math.Cos(Math.PI * angle / 180);
            double dy = length * Math.Sin(Math.PI * angle / 180);
            Vector64 p2 = new Vector64(start.X + dx, start.Y + dy, start.Z);
            Create(p1, p2);
        }
       
        public void Create(Vector64 p1,Vector64 p2)
        {
            // set first node to terminal point
            aptArrowHead[0] = p2;
            double z1 = p1.z;
            double z2 = p2.z;
            
            Vector64 vecLine = new Vector64(p2.X - p1.X, p2.Y - p1.Y, 0);// build the line vector
            Vector64 vecLeft = new Vector64(-vecLine.Y, vecLine.X, 0);// build the arrow base vector - normal to the line

            // setup remaining arrow head points
            double lineLength = vecLine.Length;
            double th = Width / (2.0f * lineLength);
            double ta = Width / (2.0f * Math.Tan(Theta / 2.0f) * lineLength);

            // find the base of the arrow
            Vector64 pBase = new Vector64(aptArrowHead[0].X + -ta * vecLine.X, 
                                          aptArrowHead[0].Y + -ta * vecLine.Y, z1 ); //base of the arrow

            // build the points on the sides of the arrow
            aptArrowHead[1] = new Vector64(pBase.X + th * vecLeft.X, pBase.Y + th * vecLeft.Y,z1);
            aptArrowHead[2] = new Vector64(pBase.X + -th * vecLeft.X, pBase.Y + -th * vecLeft.Y,z1);            
            Start = p1;
            End = pBase;

            HeadFace.AddPoint(aptArrowHead[0]);
            HeadFace.AddPoint(aptArrowHead[1]);
            HeadFace.AddPoint(aptArrowHead[2]);
            HeadFace.AddTriangleIndex(0, 1, 2);
        }//void Create()
    }//Cone

    public class CCylinder : TriangleObj
    {
        public double rad = 1.0;
        public double height = 5.0;
        public int vertSlices = 20;
        public int horSlices = 20;
        public CCylinder()
        {
            type = ShapeEnum.Triangles;            
        }
       
        public void Create(double _rad, double _height, int _verSlices = 20, int _horSlices = 20, double offx = 0, double offy = 0, double offz = 0)
        {
            if (_verSlices < 2 || _horSlices < 2) return;

            rad = _rad;
            height = _height;

            vertSlices = _verSlices;
            horSlices = _horSlices;

            double zstep = height / vertSlices;
            double x, y, z, a;

            double angleStep = 2.0 * Math.PI / horSlices;

            Vector64[] pp = new Vector64[horSlices];

            for (int j = 0; j < horSlices; j++)
            {
                a = j * angleStep;
                x = rad * Math.Cos(a);
                y = rad * Math.Sin(a);
                pp[j] = new Vector64(x, y, 0);
            }

            for (int i = 0; i < vertSlices; i++)
            {
                z = -height / 2.0 + i * zstep;
                for (int j = 0; j < horSlices; j++)
                {
                    x = pp[j].x;
                    y = pp[j].y;
                    points.Add(new Vector32(x + offx, y + offy, z + offz));
                    vec2 tex = new vec2((float)j / (horSlices - 1), (float)i / (vertSlices - 1));
                    texCoords.Add(tex);
                }
            }
            int id1, id2, id3, id4;
            for (int i = 0; i < vertSlices - 1; i++)
            {
                for (int j = 0; j < horSlices; j++)
                {
                    id1 = i * horSlices + j;
                    id2 = id1 + 1;
                    if (j == horSlices - 1) id2 = i * horSlices;
                    id3 = id1 + horSlices;
                    id4 = id2 + horSlices;
                    AddTriangleIndex(id1, id2, id3);
                    AddTriangleIndex(id4, id3, id2);
                }
            }
            pp = null;
            UpdateRange();
        }//void Create()
    }//CCylinder

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CCylinderExt : Symbol3D
    {
        public Vector32 Start = new Vector32();
        //Cylinder
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Radius"), Browsable(true)]
        public override double Rad { get; set; } = 1.0; //半径
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Height"), Browsable(true)]
        public override double Height { get; set; } = 5.0;//高度
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Vertical Slices"), Browsable(true)]
        public override int vertSlices { get; set; } = 30;//横向剖分网格数
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Horizontal Slices"), Browsable(true)]
        public override int horSlices { get; set; } = 20;//垂向剖分网格数       
        
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Top"), Browsable(true)]
        [Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(ExpandableObjectConverter))]
        public TriangleObj TopFace
        {
            get
            {
                if (Faces.Count == 3) return Faces[0];
                else return null;
            }
            set
            {
                if (Faces.Count == 3)
                {
                    Faces[0] = value;
                }
            }
        }
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Body"), Browsable(true), TypeConverter(typeof(ExpandableObjectConverter))]
        //[Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(PropertyStyleConverter))]
        public TriangleObj BodyFace
        {
            get
            {
                if (Faces.Count == 3) return Faces[1];
                else return null;
            }
            set
            {
                if (Faces.Count == 3)
                {
                    Faces[1] = value;
                }
            }
        }
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Bottom"), Browsable(true), TypeConverter(typeof(ExpandableObjectConverter))]
        //[Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(PropertyStyleConverter))]
        public TriangleObj BottomFace
        {
            get
            {
                if (Faces.Count == 3) return Faces[2];
                else return null;
            }
            set
            {
                if (Faces.Count == 3)
                {
                    Faces[2] = value;
                }
            }
        }
        void InitFaces()
        {
            TriangleObj obj = new TriangleObj();
            obj.Name = "Top";
            Faces.Add(obj);
            obj = new TriangleObj();
            obj.Name = "Body";
            Faces.Add(obj);
            obj = new TriangleObj();
            obj.Name = "Bottom";
            Faces.Add(obj);
        }
        public CCylinderExt()
        {
            type = ShapeEnum.Cylinder;
            symbolType = SymbolEnum.Cylinder;
            InitFaces();
        }
        public CCylinderExt(Vector32 p0, double _rad, double _height, int _verSlices = 20, int _horSlices = 30)
        {
            Start = p0;
            Rad = _rad;
            Height = _height;
            vertSlices = _verSlices;
            horSlices = _horSlices;
            symbolType = SymbolEnum.Cylinder;
            type = ShapeEnum.Shape;
            InitFaces();
        }
        void CreateBody()
        {
            double zstep = Height / vertSlices;
            double x, y, z, a;
            double angleStep = 2.0 * Math.PI / horSlices;
            int rounds = horSlices + 1;

            Vector32[] pp = new Vector32[rounds];
            for (int j = 0; j < rounds; j++)
            {
                a = j * angleStep;
                if (j == horSlices) a = 0;
                x = Rad * Math.Cos(a);
                y = Rad * Math.Sin(a);
                pp[j] = new Vector32(x, y, 0);
            }
            
            //body
            for (int i = 0; i <= vertSlices; i++)
            {
                z = -Height / 2.0 + i * zstep;
                for (int j = 0; j < rounds; j++)
                {
                    x = pp[j].X;
                    y = pp[j].Y;
                    BodyFace.points.Add(new Vector32(x + Start.X, y + Start.Y, z + Start.Z));
                    vec2 tex = new vec2((float)j / horSlices, (float)i / vertSlices);
                    BodyFace.texCoords.Add(tex);
                }
            }

            int id1, id2, id3, id4;
            //3  4-3  4
            //1  2-1  2
            for (int i = 0; i < vertSlices; i++)
            {
                for (int j = 0; j < horSlices; j++)
                {
                    id1 = i * rounds + j;
                    id2 = id1 + 1;
                    id3 = id1 + rounds;
                    id4 = id2 + rounds;
                    BodyFace.AddTriangleIndex(id1, id2, id3);
                    BodyFace.AddTriangleIndex(id4, id3, id2);
                }
            }
        }
        void CreateTopFace()
        {           
            double x, y, a;
            double angleStep = 2.0 * Math.PI / horSlices;
            
            //顶面中心
            TopFace.points.Add(new Vector32(Start.x, Start.y, Start.z + Height / 2.0));
            TopFace.texCoords.Add(new vec2(0.5f, 0.5f));
            
            for (int j = 0; j <= horSlices; j++)
            {
                a = j * angleStep;
                if (j == horSlices) a = 0;
                x = Rad * Math.Cos(a);
                y = Rad * Math.Sin(a);
                TopFace.points.Add(new Vector32(x + Start.X, y + Start.Y, Start.Z + Height / 2.0));
                vec2 tex = new vec2((float)(1 + x/Rad) / 2, (float)(1 + y/Rad) / 2);
                TopFace.texCoords.Add(tex);                
            }
            
            int id1, id2, id3;
            id1 = 1;
            for (int j = 0; j < horSlices; j++)
            {
                id2 = 1 + j;
                id3 = id2 + 1;
                TopFace.AddTriangleIndex(id1, id2, id3);
            }
        }
        void CreateBottomFace()
        {
            double x, y, a;
            double angleStep = 2.0 * Math.PI / horSlices;

            //底面中心
            BottomFace.points.Add(new Vector32(Start.x, Start.y, Start.z - Height / 2.0));
            BottomFace.texCoords.Add(new vec2(0.5f, 0.5f));

            //底面            
            for (int j = 0; j <= horSlices; j++)
            {
                a = j * angleStep;
                if (j == horSlices) a = 0;
                x = Rad * Math.Cos(a);
                y = Rad * Math.Sin(a);
                BottomFace.points.Add(new Vector32(x + Start.X, y + Start.Y, Start.Z - Height / 2.0));
                vec2 tex = new vec2((float)(1-(1 + x / Rad) / 2), (float)(1 + y / Rad) / 2);
                BottomFace.texCoords.Add(tex);
            }            

            int id1, id2, id3;
            id1 = 1;
            for (int j = 0; j < horSlices; j++)
            {
                id2 = 1 + j;
                id3 = id2 + 1;
                BottomFace.AddTriangleIndex(id3, id2, id1);
            }
        }
        public override bool Create()
        {
            Clear();
            if ( vertSlices < 2 || horSlices < 2) return false;
            
            if( TopFace.Visible )CreateTopFace();
            if( BodyFace.Visible ) CreateBody();
            if( BottomFace.Visible ) CreateBottomFace();

            UpdateRange();
            return true;
        }

    }    
}//namespace DataCollection

