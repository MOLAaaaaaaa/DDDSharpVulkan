using DataCollection.DelaunayVoronoi;
using DDDSharp.DataCollection.Trianglate;
using GlmNet;
using Graphics3D;
using IxMilia.Dxf.Entities;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using Poly2Tri;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using TextReaderWriter;
using static AviFile.Avi;
using static Khronos.Platform;

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
        public void FacesEnabled(bool enable)
        {
            for(int i=0;i<faces.Length;i++)
            {
                faces[i] = enable;
            }
        }
        public void FacesEnabled(int id,bool enable)
        {
            faces[id] = enable;
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
            points[0] = new vec3((float)x, (float)y, (float)z);
            points[1] = new vec3((float)(x + xlen), (float)y, (float)z);
            points[2] = new vec3((float)(x + xlen), (float)(y + ylen), (float)z);
            points[3] = new vec3((float)x, (float)(y + ylen), (float)z);
            points[4] = new vec3((float)x, (float)(y), (float)(z + zlen));
            points[5] = new vec3((float)(x + xlen), (float)(y), (float)(z + zlen));
            points[6] = new vec3((float)(x + xlen), (float)(y + ylen), (float)(z + zlen));
            points[7] = new vec3((float)(x), (float)(y + ylen), (float)(z + zlen));

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
        public Color GetColor(int index)
        {
            if (IsUniformColor || colors.Count <= index ) return uniformColor;
            else 
            {
                vec4 c = colors[index];
                byte r = (byte)(c.x * 255.0);
                byte g = (byte)(c.y * 255.0);
                byte b = (byte)(c.z * 255.0);
                byte w = (byte)(c.w * 255.0);
                return Color.FromArgb(w,r, g, b);
            }
        }
        public Vector64 CalculateDensityCenter(List<Vector64> points, int nx, int ny, double x1, double x2, double y1, double y2)
        {

            double dx = (x2 - x1) / (nx - 1);
            double dy = (y2 - y1) / (ny - 1);

            int[,] grids = new int[nx, ny];
            for (int ix = 0; ix < nx; ix++)
                for (int iy = 0; iy < ny; iy++)
                {
                    grids[ix, iy] = 0;
                }

            foreach (var p in points)
            {
                int ix = (int)((p.X - x1) / dx + 0.1);
                int iy = (int)((p.Y - y1) / dy + 0.1);
                grids[ix, iy]++;
            }
            int count = points.Count;
            double x = 0, y = 0, cx = 0, cy = 0;
            for (int ix = 0; ix < nx; ix++)
                for (int iy = 0; iy < ny; iy++)
                {
                    if (grids[ix, iy] > 0)
                    {
                        x = x1 + ix * dx;
                        y = y1 + iy * dy;
                        cx += x * grids[ix, iy] / (double)count;
                        cy += y * grids[ix, iy] / (double)count;
                    }
                }
            grids = null;
            return new Vector64(cx, cy, 0);
        }
        public C3DLine to3DLine1(int n)
        {
            int nx = 100,ny = 100, nz = n;
            double x, y, z, dx, dy, dz;

            AxisEnum axis = AxisEnum.xAxis;
            double len = XWidth;
            if (YWidth > len) { len = YWidth; axis = AxisEnum.yAxis; }
            if (ZWidth > len) { len = ZWidth; axis = AxisEnum.zAxis; }            
            
            List<Vector64>[]lists = new List<Vector64>[n];
            for (int i = 0; i < n; i++) lists[i] = new List<Vector64>();

            C3DLine line = new C3DLine(Name);            
            if (axis == AxisEnum.xAxis)
            {
                nx = n;
                ny = nz = 100;
                dx = (maxx - minx) / (nx - 1);
                dy = (maxy - miny) / (ny - 1);
                dz = (maxz - minz) / (nz - 1);
                foreach (Vector32 p in points)
                {
                    int ix = (int)((p.X - minx) / dx + 0.1);
                    lists[ix].Add(new Vector64(p.Y, p.Z, 0));
                }

                for (int ix = 0; ix < nx; ix++)
                {
                    if (lists[ix].Count == 0) continue;
                    Vector64 densityCenter = CalculateDensityCenter(lists[ix], 100, 100, miny, maxy, minz, maxz);
                    x = minx + ix * dx;
                    line.AddPoint(new Vector64(x, densityCenter.X, densityCenter.Y));
                }
            }
            else if (axis == AxisEnum.yAxis)
            {
                ny = n;
                nx = nz = 100;
                dx = (maxx - minx) / (nx - 1);                
                dy = (maxy - miny) / (ny - 1);
                dz = (maxz - minz) / (nz - 1);                
                foreach(Vector32 p in points)                
                {
                    int iy = (int)((p.Y - miny) / dy + 0.1);
                    lists[iy].Add(new Vector64(p.X, p.Z, 0));
                }

                for (int iy = 0; iy < ny; iy++)
                {
                    if ( lists[iy].Count == 0 ) continue;
                    Vector64 densityCenter = CalculateDensityCenter(lists[iy],100,100,minx,maxx,minz,maxz);
                    y = miny + iy * dy;
                    line.AddPoint(new Vector64(densityCenter.X, y, densityCenter.Y));
                }
            }
            else if (axis == AxisEnum.zAxis)
            {
                nz = n;
                nx = ny = 100;
                dx = (maxx - minx) / (nx - 1);
                dy = (maxy - miny) / (ny - 1);
                dz = (maxz - minz) / (nz - 1);
                foreach (Vector32 p in points)
                {
                    int iz = (int)((p.Z - minz) / dz + 0.1);
                    lists[iz].Add(new Vector64(p.X, p.Y, 0));
                }

                for (int iz = 0; iz < nz; iz++)
                {
                    if (lists[iz].Count == 0) continue;
                    Vector64 densityCenter = CalculateDensityCenter(lists[iz], 100, 100, minx, maxx, miny, maxy);
                    z = minz + iz * dz;
                    line.AddPoint(new Vector64(densityCenter.X, densityCenter.Y,z));
                }
            } 
            lists = null;            
            return line;
        }

        public C3DLine to3DLine(int n)
        {           
            AxisEnum axis = AxisEnum.xAxis;
            double len = XWidth;
            if (YWidth > len) { len = YWidth; axis = AxisEnum.yAxis; }
            if (ZWidth > len) { len = ZWidth; axis = AxisEnum.zAxis; }
            
            Vector32[] pp = new Vector32[n];
            int[] counts = new int[n];
            for (int i = 0; i < n; i++) { pp[i] = new Vector32(0, 0, 0, 0); counts[i] = 0; }
            if (axis == AxisEnum.xAxis)
            {
                double dx = len / (n-1);
                double x, y, z;
                for (int i = 0; i < points.Count; i++)
                {
                    Vector32 p = points[i];
                    int id = (int)((p.X - minx) / dx + 0.1);
                    pp[id].Y += p.Y;
                    pp[id].Z += p.Z;
                    counts[id]++;
                }
                for (int i = 0; i < n; i++)
                {
                    x = minx + i * dx;
                    if (counts[i] == 0) continue;
                    y = pp[i].Y / counts[i];
                    z = pp[i].Z / counts[i];
                    pp[i] = new Vector32(x, y, z);
                }
            }
            else if (axis == AxisEnum.yAxis)
            {
                double dy = len / (n-1);
                double x, y, z;
                for (int i = 0; i < points.Count; i++)
                {
                    Vector32 p = points[i];
                    int id = (int)((p.Y - miny) / dy + 0.1);
                    pp[id].X += p.X;
                    pp[id].Z += p.Z;
                    counts[id]++;
                }
                for (int i = 0; i < n; i++)
                {
                    if (counts[i] == 0) continue;
                    y = miny + i * dy;
                    x = pp[i].X / counts[i];
                    z = pp[i].Z / counts[i];
                    pp[i] = new Vector32(x, y, z);
                }
            }
            else if (axis == AxisEnum.zAxis)
            {
                double dz = len / (n-1);
                double x, y, z;
                for (int i = 0; i < points.Count; i++)
                {
                    Vector32 p = points[i];
                    int id = (int)((p.Z - minz) / dz + 0.1);
                    pp[id].Y += p.Y;
                    pp[id].X += p.X;
                    counts[id]++;
                }
                for (int i = 0; i < n; i++)
                {
                    if (counts[i] == 0) continue;
                    z = minz + i * dz;
                    y = pp[i].Y / counts[i];
                    x = pp[i].X / counts[i];
                    pp[i] = new Vector32(x, y, z);
                }
            }
            C3DLine line =   new C3DLine(Name);
            for(int i=0;i<pp.Length;i++)
            {
                if (counts[i] > 0)
                    line.AddPoint(pp[i]);
            }
            counts = null;
            pp = null;
            return line;

        }
        //public C3DLine to3DLine(int n)
        //{
        //    List<Vector64> _points = new List<Vector64>();
        //    foreach(Vector32 p  in points) 
        //    {
        //        _points.Add(new Vector64(p.x, p.y, p.z));
        //    }
        //    var generator = new TrajectoryGenerator();
        //    var linearTrajectory = generator.GenerateTrajectory(_points, trajectoryPointCount: 20);

        //    C3DLine line = new C3DLine(Name);
        //    line.AddPoint(linearTrajectory);
        //    line.UpdateRange();
        //    _points.Clear();
        //    linearTrajectory.Clear();
        //    return line;

        //}
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
                    line = p.X + " " + p.Y + " " + p.Z;
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
                //also uniform color
                br.Write(color.x);
                br.Write(color.y);
                br.Write(color.z);
                br.Write(color.w);

                //added 2025-11-14 1.32version
                br.Write(IsUniformColor);
                br.Write(WireFrameVisible);
                br.Write(WireFrameColor.ToArgb());
                br.Write(WireFrameAlpha);

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

                //added 2025-11-14 1.32version
                if (C3DData.DataVersion >= 1.32f)
                {
                    IsUniformColor = br.ReadBoolean();
                    WireFrameVisible = br.ReadBoolean();
                    WireFrameColor = Color.FromArgb(br.ReadInt32());
                    WireFrameAlpha = br.ReadSingle();
                }

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
        public void AddPoints(List<Vector32> _points)
        {
            points.AddRange(_points);
        }
        public void AddPoints(List<Vector64> _points)
        {
            foreach(Vector64 p in _points) points.Add(p);
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
        public override double GetNearestDistance(Vector64 p0)
        {
            double mindist = 1e30,dist;
            for(int i=0;i<points.Count;i++)
            {
                dist = points[i].Distance(p0);
                if (dist < mindist) mindist = dist;
            }
            return mindist;
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
            minx = miny = minz = double.MaxValue;
            maxx = maxy = maxz = double.MinValue;
            foreach( Vector32 p in  points )             
            {
                if (!float.IsNaN(p.x) && minx > p.x) minx = p.x;
                if (!float.IsNaN(p.y) && miny > p.y) miny = p.y;
                if (!float.IsNaN(p.z) && minz > p.z) minz = p.z;
                if (!float.IsNaN(p.x) && maxx < p.x) maxx = p.x;
                if (!float.IsNaN(p.y) && maxy < p.y) maxy = p.y;
                if (!float.IsNaN(p.z) && maxz < p.z) maxz = p.z;
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
        public override bool SaveAs(string path,int version = 0)
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
        public Bitmap bmp;
        public string errMsg;
        public ImageStruct Copy()
        {
            ImageStruct im = new ImageStruct();
            im.rect = rect;
            im.errMsg = errMsg;
            im.bmp = new Bitmap(bmp);
            return im;
        }
        public void Clear()
        {
            if (bmp != null) bmp.Dispose();
        }
        public void LPtoDP(ref double x,ref double y)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            x = 0 + (width-1) * (x - rect.X1) / rect.Width;
            y = (height-1) - (height - 1) * (y - rect.Y1) / rect.Height;
        }
        public void DPtoLP(ref double x, ref double y)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            x = rect.X1 + rect.Width * x / (width - 1);
            y = rect.Y2 - rect.Height * y / (height-1);
        }
        public bool SaveAs(ref BinaryWriter br)
        {
            try
            {
                br.Write(rect.X1);
                br.Write(rect.Y1);
                br.Write(rect.X2);
                br.Write(rect.Y2);
                C3DData.toStream(br, bmp);
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
                bmp = C3DData.fromStream(br, 4096);
                if (bmp == null) { errMsg = C3DData.errMessage;return false; }
                else return true;
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

        double globalHight1 = 0, globalHight2 = 0;
        double localHight1 = 0, localHight2 = 0;

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
        [CategoryAttribute("Background Image"), DisplayNameAttribute("Visibal")]
        public bool ShowBackgroundImage { get; set; } = false;
        [CategoryAttribute("Background Image"), DisplayNameAttribute("Image")]
        public Bitmap BackgroundImage 
        { 
            get 
            {
                if (backImages.Count > 0) return backImages[0].bmp;
                else return null;
            }
            set 
            {
                if (backImages.Count > 0)
                {
                    ImageStruct im = backImages[0];
                    im.bmp = value;
                    backImages[0] = im;
                }
            }
        }
        [CategoryAttribute("Background Image"), DisplayNameAttribute("Value")]
        public double backgroundPropertyValue { get; set; } = 0;
        public double slicerWidth //剖面长度（全局）
        {
            get 
            {
                if(axis == AxisEnum.zAxis )
                {
                    return Math.Sqrt(XWidth*XWidth +YWidth*YWidth);
                }
                else if (axis == AxisEnum.yAxis)
                {
                    return Math.Sqrt(XWidth * XWidth + ZWidth * ZWidth);
                }
                else return Math.Sqrt(YWidth * YWidth + ZWidth * ZWidth);
            }
        }
        public double slicerHeight //剖面高度（全局）
        {
            get
            {
                if (axis == AxisEnum.zAxis)
                {
                    return ZWidth;
                }
                else if (axis == AxisEnum.yAxis)
                {
                    return YWidth;
                }
                else return XWidth;
            }
        }
        public DoubleRect imageRect 
        {
            get 
            {
                DoubleRect rect = new DoubleRect();
                if(backImages.Count > 0)
                {
                    rect = backImages[0].rect;
                }
                return rect;
            }
        }
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
        public bool TopographyBlank(C2DPolygons polys, CMesh mesh)
        {
            for (int i = polys.Count - 1; i >= 0; i--)
            {
                List<int> vertices = new List<int>();
                Polygon2D poly = polys[i];
                for (int j = 0; j < poly.Count; j++)
                {
                    Vector64 p = poly[j];
                    p = toTracedPoint(p);
                    if (p.Z > mesh.GetValue(p.X, p.Y))
                        vertices.Add(j);
                }
                if (vertices.Count > 0)
                {
                    poly.RemovePoints(vertices);
                    polys[i] = poly;
                    if (poly.Count < 3) polys.RemoveAt(i);
                }
            }
            return true;
        }
        public bool TopographyBlank(List<ImageStruct> images, CMesh mesh)
        {
            if (images.Count < 1) return true;
            ImageStruct im = images[0];
            Bitmap bmp = im.bmp;

            //1拷贝图像到数据bytes
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bd.Stride;
            byte[] bytes = new byte[bmp.Height * stride];
            Marshal.Copy(bd.Scan0, bytes, 0, bytes.Length);
            Vector64 p;
            double x, y, z;
            byte r, g, b, a;
            for (int j = 0; j < stride; j += 4) 
            {
                for (int i = 0; i < bmp.Height; i++)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        b = bytes[i * stride + j];
                        g = bytes[i * stride + j + 1];
                        r = bytes[i * stride + j + 2];
                        //a = bytes[i * stride + j + 3];
                    }
                    else
                    {
                        //a = bytes[i * stride + j];
                        r = bytes[i * stride + j + 1];
                        g = bytes[i * stride + j + 2];
                        b = bytes[i * stride + j + 3];
                    }

                    x = j/4; y = i;   //图像坐标
                    im.DPtoLP(ref x, ref y);//切片局部坐标
                    p = toTracedPoint(new Vector64(x, y, 0));//3D全局坐标
                    z = mesh.GetValue(p.X, p.Y);

                    if (p.Z > z) //地形之上
                    {
                        if (BitConverter.IsLittleEndian)
                            bytes[i * stride + j + 3] = 0;
                        else bytes[i * stride + j] = 0;
                    }
                    else break; //地形以下不扫描
                }                
            }
            
            //拷贝数据bytes到图像
            Marshal.Copy(bytes, 0, bd.Scan0, bytes.Length);
            bmp.UnlockBits(bd);
            bytes = null;

            im.bmp = bmp;
            images[0] = im;

            return true;
        }
        public override bool TopographyBlank(CMesh mesh)
        {
            TopographyBlank(polygons,mesh);
            TopographyBlank(tracedGeoObjects, mesh);
            TopographyBlank(backImages, mesh);
            return true;
        }
        public PolygonSlicer Copy()
        {
            PolygonSlicer poly = new PolygonSlicer();            
            poly.CopyHeaderFrom(this);

            poly.ShowBackgroundImage = ShowBackgroundImage;

            foreach (Polygon2D p in polygons.Polygons)
                poly.AddPolygon(p);
            foreach (Polygon2D obj in tracedGeoObjects.Polygons)
                poly.tracedGeoObjects.Add(obj);

            poly.axis = axis;
            poly.minxLocated = minxLocated;
            poly.minyLocated = minyLocated;
            poly.minzLocated = minzLocated;
            poly.maxxLocated = maxxLocated;
            poly.maxyLocated = maxyLocated;
            poly.maxzLocated = maxzLocated;
            poly.backgroundPropertyValue = backgroundPropertyValue;
            poly.originalPath = originalPath;            

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
        public List<string>GetPolygonNames()
        {
            List<string> polys = new List<string>();
            for(int i=0;i<tracedGeoObjects.Count;i++)
            {
                string s = tracedGeoObjects[i].Name;
                if ( s.Length < 1 ) continue;
                if (polys.IndexOf(s) < 0) polys.Add(s);
            }
            return polys;
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
        public override bool SaveAs(string path,int version = 0)
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

            br.Write(ShowBackgroundImage); //added 2025-8-28
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
                if (C3DData.DataVersion >= 1.31f) ShowBackgroundImage = br.ReadBoolean();

                int imgCount = br.ReadInt32();
                for (int i = 0; i < imgCount; i++)
                {
                    ImageStruct img = new ImageStruct();
                    if (img.ReadFrom(br))
                        backImages.Add(img);
                    else 
                    {
                        errMessage = img.errMsg;
                        return false; 
                    }
                }
            }
            UpdateRange();
            //*/
            return true;
        }
        public override bool ExportData(string path)
        {
            StreamWriter wr = new StreamWriter(new FileStream(path, FileMode.Create));
            
            string headerLine = "POLYGON Slicer 1200";
            wr.WriteLine(headerLine);
            string line = "Polygon Count = " + tracedGeoObjects.Count;
            wr.WriteLine(line);

            Vector64 v;
            int i = 1;
            foreach (Polygon2D obj in tracedGeoObjects.Polygons)
            {
                line = "Polygon_" + i + " " + obj.Name + " Count " + obj.Count;
                wr.WriteLine(line);
                foreach(Vector32 p in obj.points)
                {
                    v =  toTracedPoint(p.toVector64());
                    line = v.X + "," + v.Y + "," + v.Z;
                    wr.WriteLine(line);
                }
                i++;
            }
            wr.Close();
            return true;
        }
        public int SetLayersPropertyByName(StratumDatas stratums)
        {
            int count = 0;
            string name1, name2;
            Polygon2D poly;
            for (int i = 0; i < tracedGeoObjects.Count; i++)
            {
                poly = tracedGeoObjects[i];
                name1 = poly.Name.ToLower();
                for (int j = 0; j < stratums.Count; j++)
                {
                    name2 = stratums[j].Name.ToLower();
                    if (name1 == name2)
                    {
                        poly.fillColor = stratums[j].Color;
                        poly.PropertyValue = stratums[j].Value;
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
            //if (Locations3D.Contains(p3d)) return;//默认为(0,0,0)
            Locations2D.Add(p2d);
            Locations3D.Add(p3d);
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
        public void ClearLocations()
        {
            Locations2D.Clear();
            Locations3D.Clear();            
        }
        /// <summary>
        /// 按照local X顺序排列定位点
        /// </summary>
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
        /// <summary>
        /// Convert Point between id1 - id2
        /// 映射点在两个定位点之间
        /// </summary>
        /// <param name="p"></param>
        /// <param name="id1">index in Locations2D</param>
        /// <param name="id2">index in Locations2D</param>
        /// <returns></returns>
        private Vector64 toTracedPoint(Vector64 p, int id1, int id2)
        {
            Vector64 p1 = Locations2D[id1]; //local p1
            Vector64 p2 = Locations2D[id2]; //local p2

            Vector64 v1 = Locations3D[id1]; //global p1
            Vector64 v2 = Locations3D[id2]; //global p2

            double scale = (p.X - p1.X) / (p2.X - p1.X);

            Vector64 v = v1 + scale * (v2 - v1);

            if (axis == AxisEnum.xAxis) 
            {
                v.X = globalHight1 + (globalHight2 - globalHight1) *(p.Y - localHight1) / (localHight2 - localHight1);
            }
            if (axis == AxisEnum.yAxis)
            {
                v.Y = globalHight1 + (globalHight2 - globalHight1) * (p.Y - localHight1) / (localHight2 - localHight1);
            }
            if (axis == AxisEnum.zAxis)
            {
                v.Z = globalHight1 + (globalHight2 - globalHight1) * (p.Y - localHight1) / (localHight2 - localHight1);
            }
            v.V = p.V;
            return v;
        }
        public Vector64 toTracedPoint(double x, double y, double z, double v)
        {
            return toTracedPoint(new Vector64(x, y, z, v));
        }
        /// <summary>
        /// 空间点到剖面的投影
        /// 默认XOZ平面
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>投影坐标</returns>
        public Vector64 ProjectTo(double x, double y, double z)
        {
            Vector64 p1 = Locations3D[0];
            Vector64 p2 = Locations3D[Locations3D.Count-1];
            double x1 = Math.Min(p1.X, p2.X);
            double x2 = Math.Max(p1.X, p2.X);
            double y1 = Math.Min(p1.Y, p2.Y);
            double y2 = Math.Max(p1.Y, p2.Y);
            double z1 = Math.Min(p1.Z, p2.Z);
            double z2 = Math.Max(p1.Z, p2.Z);
            CLine line = new CLine(p1, p2);
            //点到直线投影
            Vector64 p0 = line.GetPointProjection(new Vector64(x, y, z));
            p0.Z = z;
            return p0;
        }
        /// <summary>
        /// 空间点投影到剖面上，返回剖面局部点坐标x,y
        /// 默认zAxis切片
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Vector64 ProjectToLocalPoint(double x, double y, double z)
        {
            Vector64 p1 = Locations3D[0];
            Vector64 p2 = Locations3D[Locations3D.Count - 1];
            double x1 = Math.Min(p1.X, p2.X);
            double x2 = Math.Max(p1.X, p2.X);
            double y1 = Math.Min(p1.Y, p2.Y);
            double y2 = Math.Max(p1.Y, p2.Y);
            double z1 = Math.Min(p1.Z, p2.Z);
            double z2 = Math.Max(p1.Z, p2.Z);
            CLine line = new CLine(p1, p2);

            //点到直线投影
            Vector64 p0 = line.GetPointProjection(new Vector64(x, y, z));
            p0.Z = z;
            double mx1 = imageRect.X1;
            double mx2 = imageRect.X2;
            double my1 = imageRect.Y1;
            double my2 = imageRect.Y2;

            double width = Math.Sqrt((x2-x1)*(x2-x1) + (y2-y1)*(y2-y1));
            double height = z2 - z1;
            double len = Math.Sqrt((p0.X - x1) * (p0.X - x1) + (p0.Y - y1) * (p0.Y - y1));
            double px = mx1 + (mx2 - mx1) * (p0.X - x1) / (x2 - x1);
            double pz = my1 + (my2 - my1) * (z - z1) / (z2 - z1);
            return new Vector64(px,pz,0);
        }
        /// <summary>
        /// X1,Y2  ---------  X2,Y2
        /// 
        /// X1,Y1  --------- X2,Y1
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector64 toTracedPoint(Vector64 p)
        {
            if (Locations2D.Count < 2) return p;

            if (localHight1 >= localHight2 || globalHight1 >= globalHight2) 
                UpdateLocationHights();

            //left side            
            if (p.X <= Locations2D[0].X)//0
            {
                return toTracedPoint(p, 0, 1);
            }
            //right side            
            if (p.X >= Locations2D[Locations2D.Count - 1].X)//n-1
            {
                return toTracedPoint(p, Locations2D.Count - 2, Locations2D.Count - 1);
            }

            for (int i = 1; i < Locations2D.Count; i++)//1 - (n-1)
            {
                if (p.X < Locations2D[i].X) return toTracedPoint(p, i - 1, i);
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
        /// <summary>
        /// 将切片2D对象转换成空间多边形
        /// </summary>
        /// <param name="poly">2D多边形对象</param>
        /// <returns>3D多边形对象</returns>
        public Polygon2D toTraced3DPolygon(Polygon2D poly)
        {
            Polygon2D poly3d = poly.Copy();
            poly3d.points.Clear();
            foreach (Vector64 p in poly.points)
            {
                poly3d.points.Add(toTracedPoint(p));
            }
            poly3d.UpdateRange();
            return poly3d;
        }
        public C2DPolygons toTraced3DPolygons(C2DPolygons polys)
        {
            C2DPolygons polygons = new C2DPolygons(Name);
            foreach(Polygon2D poly in polys.Polygons)
            {
                polygons.Add(toTraced3DPolygon(poly));
            }
            polygons.UpdateRange();
            return polygons;
        }
        /// <summary>
        /// 更新空间定位后的坐标范围
        /// localHeight,globalHeight
        /// </summary>
        void UpdateLocationHights()
        {
            localHight1 = localHight2 = 0;
            globalHight1 = globalHight2 = 0;
            if (!IsLocated) return;            
            Vector64 v;
            for (int j = 0; j < Locations2D.Count; j++)
            {
                v = Locations2D[j];
                if (j == 0)
                {
                    localHight1 = localHight2 = v.Y;
                }
                else
                {
                    if (v.Y < localHight1) localHight1 = v.Y;
                    if (v.Y > localHight2) localHight2 = v.Y;
                }
            }

            //更新定位坐标范围
            for (int j = 0; j < Locations3D.Count; j++)
            {
                v = Locations3D[j];
                if (j == 0)
                {
                    if (axis == AxisEnum.xAxis) globalHight1 = globalHight2 = v.X;
                    if (axis == AxisEnum.yAxis) globalHight1 = globalHight2 = v.Y;
                    if (axis == AxisEnum.zAxis) globalHight1 = globalHight2 = v.Z;
                }
                else
                {
                    if (axis == AxisEnum.xAxis)
                    {
                        if (v.X < globalHight1) globalHight1 = v.X;
                        if (v.X > globalHight2) globalHight2 = v.X;
                    }
                    if (axis == AxisEnum.yAxis)
                    {
                        if (v.Y < globalHight1) globalHight1 = v.Y;
                        if (v.Y > globalHight2) globalHight2 = v.Y;
                    }
                    if (axis == AxisEnum.zAxis)
                    {
                        if (v.Z < globalHight1) globalHight1 = v.Z;
                        if (v.Z > globalHight2) globalHight2 = v.Z;
                    }
                }
            } 
        }
        /// <summary>
        /// 更新空间定位后的坐标范围
        /// localHeight1,localHeight2
        /// object映射后的空间坐标范围，Range of the slicer
        /// </summary>
        private void UpdateLocationRange()
        {
            if (!IsLocated) return;
            UpdateRange();
            UpdateLocationHights();

            minxLocated = maxxLocated = 0;
            minyLocated = maxyLocated = 0;
            minzLocated = maxzLocated = 0;

            Vector64 v; 
            //更新定位坐标范围
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
                    if (v.z < minzLocated) minzLocated = v.z;
                    if (v.x > maxxLocated) maxxLocated = v.x;
                    if (v.y > maxyLocated) maxyLocated = v.y;
                    if (v.z > maxzLocated) maxzLocated = v.z;
                }
            }

            if (axis == AxisEnum.zAxis)//Z is height
            {                
                minzLocated = globalHight1 + (globalHight2 - globalHight1) * (miny - localHight1) / (localHight2 - localHight1);
                maxzLocated = globalHight1 + (globalHight2 - globalHight1) * (maxy - localHight1) / (localHight2 - localHight1);
            }
            else if (axis == AxisEnum.yAxis)//Y is height
            {
                minyLocated = globalHight1 + (globalHight2 - globalHight1) * (miny - localHight1) / (localHight2 - localHight1);
                maxyLocated = globalHight1 + (globalHight2 - globalHight1) * (maxy - localHight1) / (localHight2 - localHight1);
            }
            else if (axis == AxisEnum.xAxis)
            {                
                minxLocated = globalHight1 + (globalHight2 - globalHight1) * (miny - localHight1) / (localHight2 - localHight1);
                maxxLocated = globalHight1 + (globalHight2 - globalHight1) * (maxy - localHight1) / (localHight2 - localHight1);
            }

            //update local range except hight, slicer of first and last
            Vector64 p1 = toTracedPoint(new Vector64(minx, miny, 0));
            Vector64 p2 = toTracedPoint(new Vector64(maxx, maxy, 0));
            if (axis == AxisEnum.zAxis)//Z is height
            {
                minxLocated = Math.Min(p1.X, p2.X);
                maxxLocated = Math.Max(p1.X, p2.X);
                minyLocated = Math.Min(p1.Y, p2.Y);
                maxyLocated = Math.Max(p1.Y, p2.Y);                
            }
            if (axis == AxisEnum.yAxis)//Y is height
            {
                minxLocated = Math.Min(p1.X, p2.X);
                maxxLocated = Math.Max(p1.X, p2.X);
                minzLocated = Math.Min(p1.Z, p2.Z);
                maxzLocated = Math.Max(p1.Z, p2.Z);
            }
            if (axis == AxisEnum.xAxis)//X is height
            {
                minyLocated = Math.Min(p1.Y, p2.Y);
                maxyLocated = Math.Max(p1.Y, p2.Y);
                minzLocated = Math.Min(p1.Z, p2.Z);
                maxzLocated = Math.Max(p1.Z, p2.Z);
            }
        }
        public DoubleRect GetTracedGeoObjectsRange(List<string>names = null)
        {
            double x1 = 1E30, y1 = 1E30, x2 = -1E30, y2=-1E30;            
            for (int i = 0; i < tracedGeoObjects.Count; i++)
            {
                Polygon2D obj = tracedGeoObjects[i];
                obj.UpdateRange();

                bool ignore = false;
                if( names != null && names.Count > 0 )
                {
                    foreach (string name in names)
                    {
                        if (obj.Name.ToLower() == name.ToLowerInvariant())
                        {
                            ignore = true;
                            break;
                        }
                    }
                }
                
                if (ignore) continue;

                if (obj.minx < x1) x1 = obj.minx;
                if (obj.maxx > x2) x2 = obj.maxx;
                if (obj.miny < y1) y1 = obj.miny;
                if (obj.maxy > y2) y2 = obj.maxy;
            }

            if (names == null || names.Count < 1)
            {
                x1 = minx;x2 = maxx;
                y1 = miny;y2 = maxy;
            }
            
            Vector64 p1 = new Vector64(x1,y1,0);
            p1 = toTracedPoint(p1);
            Vector64 p2 = new Vector64(x2, y2, 0);
            p2 = toTracedPoint(p2);

            if (axis == AxisEnum.zAxis)
            {
                x1 = 0;
                x2 = Math.Sqrt( (p1.X-p2.X)* (p1.X - p2.X) + 
                                (p1.Y - p2.Y)* (p1.Y - p2.Y) );                
                y1 = Math.Min(p1.Z, p2.Z);
                y2 = Math.Max(p1.Z, p2.Z);
            }
            else if (axis == AxisEnum.yAxis)
            {
                x1 = 0;
                x2 = Math.Sqrt( (p1.X - p2.X) * (p1.X - p2.X) +
                                (p1.Z - p2.Z) * (p1.Z - p2.Z));
                y1 = Math.Min(p1.Y, p2.Y);
                y2 = Math.Max(p1.Y, p2.Y);
            }
            else if (axis == AxisEnum.xAxis)
            {
                x1 = 0;
                x2 = Math.Sqrt( (p1.Y - p2.Y) * (p1.Y - p2.Y) +
                                (p1.Z - p2.Z) * (p1.Z - p2.Z));
                y1 = Math.Min(p1.X, p2.X);
                y2 = Math.Max(p1.X, p2.X);
            }
            return new DoubleRect(x1, y1, x2, y2);
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
            Vector64.DuplicatedFilter(ref points, 0.1);            
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

        bool IsInLists(string name, List<string>Names)
        {
            foreach (string s in Names) 
            {
                if (s.ToLower() == name.ToLower())
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 对地层进行采样(指定地层或全部,均匀采样,均匀采样）
        /// </summary>
        /// <param name="names">地层名称数组，null表示全部地层</param>
        /// <param name="xgrid"></param>
        /// <param name="ygrid"></param>
        /// <param name="resetLayer"></param>
        /// <param name="bkvalue"></param>
        /// <param name="resetValue"></param>
        /// <returns>采样数组</returns>      
        public List<Vector64> SampleLayerCoords(List<string> names,//地层名称
                                                int xgrid,//网格采样数 +1
                                                int ygrid,//网格采样数 +1                                               
                                                int xbksample, int ybksample, //背景采样网格
                                                bool resetLayer = false, //重置地层值
                                                bool samplebackground = true,//是否采样背景地层                                                
                                                float bkvalue = 0,  //背景值（地层外节点值）
                                                float resetValue = 1)//地层值重置为
        {
            double x, y, val;
            double xx = (maxx - minx) / (xgrid - 1);
            double yy = (maxy - miny) / (ygrid - 1);
            
            int all = xgrid * ygrid;
            double[] grids = new double[all];

            for (int i = 0; i < grids.Length; i++) grids[i] = bkvalue;            
            
            //将剖面数据采样到网格grids 中，后面的覆盖前面的
            for (int iy = 0; iy < ygrid; iy++)
            {
                y = miny + iy * yy + yy / 2;
                for (int ix = 0; ix < xgrid; ix++)
                {
                    x = minx + ix * xx + xx / 2;
                    Vector64 p = new Vector64(x, y, -1, bkvalue);
                    bool islayer = false; //地层样本点
                    for (int k = 0; k < tracedGeoObjects.Count; k++) //按绘制顺序
                    {
                        Polygon2D poly = tracedGeoObjects[k];
                        if (names != null && names.Count > 0)
                        {
                            if ( !IsInLists( poly.Name,names) ) 
                                continue;
                        }
                        if( poly.IsPointInsidePoly(x, y) )
                        {
                            val = poly.PropertyValue;
                            if (resetLayer) val = resetValue;
                            grids[ix + iy * xgrid] = val;
                            p.z = k;
                            p.v = val;
                            islayer = true; //地层样本点
                            break;
                        }
                    }
                    if(islayer) sampledGrids.Add(p);
                    else 
                    {
                        if(samplebackground && iy % ybksample == 0 && ix % xbksample == 0)
                        {
                            sampledGrids.Add(p);
                        }
                    }

                }//for (int ix = 0; ix < xgrid; ix++)
            }//for (int iy = 0; iy < ygrid; iy++)           

            grids = null;
            return sampledGrids;
        }


        public List<Vector64> SampleLayerCoords(List<Color> colors,//颜色列表
                                                int xgrid,//网格采样数 +1
                                                int ygrid,//网格采样数 +1                                               
                                                float resetValue = 1,  //地层值
                                                float bkvalue = -1)//背景值（地层外节点值）
        {
            sampledGrids.Clear();
            if (colors.Count < 1) return sampledGrids;

            double x, y;
            double xx = (maxx - minx) / (xgrid - 1);
            double yy = (maxy - miny) / (ygrid - 1);
            int all = xgrid * ygrid;
            double[] grids = new double[all];
            for (int i = 0; i < grids.Length; i++) grids[i] = bkvalue;

            ImageStruct im = backImages[0];
            int width = im.bmp.Width;
            int height = im.bmp.Height;
            //拷贝图像数据
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData data = im.bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;//data.Stride;
            byte[] bytes = new byte[height * stride];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            im.bmp.UnlockBits(data);//图像数据解锁
 
            Color c;
            int id = 0;           
            byte r, g, b;
            int dx = width / xgrid + 1;
            int dy = height / ygrid + 1;
            
            for (int iy = 0; iy < height; iy += dy)
            {
                for (int ix = 0; ix < width; ix += dx)
                {                   
                    id = iy * stride + 4*ix;
                    x = ix; y = iy;
                    im.DPtoLP(ref x,ref y);
                    Vector64 p = new Vector64(x, y, -1, bkvalue);
                   // p = toTracedPoint(p);
                    if (BitConverter.IsLittleEndian)
                    {
                        b = bytes[id];
                        g = bytes[id+1];
                        r = bytes[id+2];
                        //a = bytes[id+3];
                    }
                    else
                    {
                        //a = bytes[id];
                        r = bytes[id + 1];
                        g = bytes[id + 2];
                        b = bytes[id+3];
                    }
                    c = Color.FromArgb(r, g, b);                    
                    if ( C3DData.IsColorInList(c, colors, 10) )
                        p.Z = p.V = resetValue;
                    sampledGrids.Add(p);
                }
            }            
            bytes = null;           
            return sampledGrids;
        }

        /// <summary>
        /// 地层边界采样 
        /// </summary>
        /// <param name="names">指定地层名称，空则为全部地层</param>
        /// <param name="step"></param>
        /// <param name="resetLayer"></param>
        /// <param name="resetValue"></param>
        /// <returns></returns>
        public List<Vector64> SampleBoudary(List<string> names, double step,
                                            bool resetLayer = false, //重置地层值                                            
                                            float resetValue = 10)
        {
            double val;
            Vector64 p;
            for (int k = tracedGeoObjects.Count - 1; k >= 0; k--)
            {
                Polygon2D poly = tracedGeoObjects[k];

                if( names == null || names.Count < 1 || 
                    names.Contains(poly.Name.ToLower()) )
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




        /// <summary>
        /// 对采样网格进行过滤
        /// </summary>
        /// <param name="xgrid"></param>
        /// <param name="ygrid"></param>
        /// <param name="stepx"></param>
        /// <param name="stepy"></param>
        public void ResampleFilter(int xgrid, int ygrid, int stepx, int stepy)
        {
            int id;
            int all = xgrid * ygrid;
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

                bool bodergrid = false;
                foreach (int idnear in Nears)
                {
                    if (idnear < 0 || idnear >= all)//范围边界点
                        bodergrid = true;
                    else if (sampledGrids[idnear].V != sampledGrids[id].V) //属性边界点
                        bodergrid = true;
                    if ( bodergrid )
                    {
                        boders[id] = true;
                        keep[id] = true;
                        break;
                    }
                }//foreach(int idnear in Nears)
            }//for (int id = 0; id < all; id++)

            //重采样网格过滤
            for (int iy = 0; iy < ygrid; iy += stepy)
             for (int ix = 0; ix < xgrid; ix += stepx)
                {
                    id = ix + iy * xgrid;
                    keep[id] = true;
                }

            List<Vector64> sampled = new List<Vector64>();

            //输出网格            
            for (int iy = 0; iy < ygrid; iy++)
            {
                for (int ix = 0; ix < xgrid; ix++)
                {
                    if (keep[ix + iy * xgrid])
                    {
                        sampled.Add(sampledGrids[id]);
                    }
                }
            }

            sampledGrids.Clear();
            sampledGrids = sampled;
            
            Nears = null;
            boders = null;
            keep = null;
        }

        /// <summary>
        /// 采样网格，冗余点过滤
        /// </summary>
        /// <param name="percent"></param>
        public void SampledDuplicatedFilter(double percent)
        {
            Vector64.DuplicatedFilter(ref sampledGrids, percent);
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

        public void TrimBackgroundImage(Rectangle rect)
        {
            if (rect.Width < 1 || rect.Height < 1) return;

            double x1 = rect.X;
            double y1 = rect.Y;
            double x2 = x1 + rect.Width-1;
            double y2 = y1 + rect.Height-1;
            //to logical coordinates
            x1 = minx + (maxx - minx) * x1 / (BackgroundImage.Width-1);
            x2 = minx + (maxx - minx) * x2 / (BackgroundImage.Width-1);
            y1 = miny + (maxy - miny) * y1 / (BackgroundImage.Height-1);
            y2 = miny + (maxy - miny) * y2 / (BackgroundImage.Height-1);
            //new logical coordinates
            Vector64 p1 = new Vector64(x1, y1, 0);
            Vector64 p2 = new Vector64(x2, y2, 0);
            //new logical traced coordinates
            Vector64 p11 = toTracedPoint(p1);
            Vector64 p12 = toTracedPoint(p2);            
            minx = 0;
            maxx = rect.Width-1;
            miny = 0;
            maxy = rect.Height-1;
            Locations2D.Clear();
            Locations3D.Clear();
            AddLocationPoint(new Vector64(minx, miny, 0), p11);
            AddLocationPoint(new Vector64(maxx, maxy, 0), p12);
            UpdateTraced();

            ImageStruct im = backImages[0];                        
            im.rect = new DoubleRect(0,0,rect.Width-1,rect.Height-1);
            im.bmp = CGraphic3D.TrimImage(im.bmp, rect);
            //im.bmp.Dispose();
            //im.bmp = newBmp;
            backImages[0] = im;
        }

        public bool ExportImage(string filename)
        {
            if (backImages.Count < 1) { errMessage = "no image files"; return false; }
            try
            {                
                ImageStruct im = backImages[0];
                im.bmp.Save(filename);
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
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
        Counterclockwise = 2,
        Collinear = 3,//点共线

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

        [CategoryAttribute("Display"), DisplayNameAttribute("Name")]
        public override string Name 
        {   get; 
            set; 
        }

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
        /// <summary>
        /// 获取唯一性的地层名称列表
        /// </summary>
        /// <param name="closed"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public List<string>toObjectNames(bool closed,bool all = false)
        {
            List<string>names = new List<string>();
            foreach(var s  in Polygons) 
            {
                if (!all && s.IsClosed != closed) continue;
                if( !names.Contains(s.Name) ) names.Add(s.Name);                
            }
            return names;
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
    public class MyRegion
    {
        GraphicsPath gp = new GraphicsPath();
        Bitmap bmp = null;
        int minx = 0, maxx=0, miny=0, maxy=0;
        void UpdateDataRange(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (i == 0)
                {
                    minx = maxx = points[i].X;
                    miny = maxy = points[i].Y;
                }
                else
                {
                    if (points[i].X < minx) minx = points[i].X;
                    if (points[i].X > maxx) maxx = points[i].X;
                    if (points[i].Y < miny) miny = points[i].Y;
                    if (points[i].Y > maxy) maxy = points[i].Y;
                }
            }
        }
        public MyRegion( List<Point>points )
        {
            UpdateDataRange(points);
            int width  = maxx - minx - 1;
            int height = maxy - miny - 1;            
            bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);            
            List<Point> points1 = new List<Point>();
            for(int i = 0; i < points.Count; i++)
            points1.Add(new Point(points[i].X - minx, points[i].Y - miny));
            gp.AddPolygon(points1.ToArray());
            Region rgn = new Region(gp);
            g.FillRegion(Brushes.Red, rgn);
            points1.Clear();            
            g.Dispose();
            rgn.Dispose();
        }
        public bool IsPointInRgn(int ix,int iy)
        {
            int ix0 = ix - minx, iy0 = iy - miny;// maxy - iy - 1;
            if (ix0 < 0 || ix0 >= bmp.Width || 
                iy0 < 0 || iy0 >= bmp.Height) return false;
            Color color = bmp.GetPixel(ix0, iy0);
            if (color.R == 255 && color.G == 0 && color.B == 0) return true;
            else return false;
        }
        public void Clear()
        {
            bmp.Dispose();
        }
    }
    public class Polygon2D : C3DObjectBase
    {
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Is Closed")]
        public bool IsClosed { get; set; } = true;
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Property Value")]
        public double PropertyValue { get; set; } = 0; //property value
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Alpha(0-1)")]
        public override float Alpha { get; set; } = 1;
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Smooth")]
        public bool SmoothDraw { get; set; } = false;
        
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Locked")]
        public bool Locked { get; set; } = false;

        [CategoryAttribute("Filling"), DisplayNameAttribute("Fill")]
        public bool IsFill { get; set; } = true;        
        FillPatternClass _fillMethod = new FillPatternClass(FillMethodEnum.Solid);        
        [CategoryAttribute("Filling"), DisplayNameAttribute("Pattern")]
        [Editor(typeof(PatternEditor), typeof(UITypeEditor)), TypeConverter(typeof(PatternConverter))]
        public FillPatternClass fillMethod
        {
            get 
            {
                _fillMethod.fillColor = _fillColor;
                return _fillMethod; 
            }
            set 
            { 
                _fillMethod = value;
                _fillColor = _fillMethod.fillColor;
            }
        }
        
        Color _fillColor = Color.White;
        [CategoryAttribute("Object Properties"), DisplayNameAttribute("Fill Color")]
        public Color fillColor 
        {
            get 
            {
                return _fillColor;
            }
            set 
            {
                _fillColor = value;
                _fillMethod.fillColor = _fillColor;
            }
        }

        [CategoryAttribute("Line"), DisplayNameAttribute("Style")]
        public DashStyle dashStyle { get; set; } = DashStyle.Solid;
        
        [CategoryAttribute("Line"), DisplayNameAttribute("Color")]
        public Color lineColor { get; set; } = Color.Black;
        [CategoryAttribute("Line"), DisplayNameAttribute("Width")]
        public float lineWidth { get; set; } = 1.0f;
        [CategoryAttribute("Line"), DisplayNameAttribute("Visible")]
        public bool ShowLine { get; set; } = true;

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
            poly.SmoothDraw = SmoothDraw;
            poly.fillColor = fillColor;
            poly.fillMethod = fillMethod;
            poly.lineColor = lineColor;
            poly.fillMethod = fillMethod.Copy();
            poly.PropertyValue = PropertyValue;
            poly.axis = axis;
            poly.lineWidth = lineWidth;
            poly.dashStyle = dashStyle;
            poly.ShowLine = ShowLine;
            poly.Alpha = Alpha;
            
            if(triangledObject != null) poly.triangledObject = triangledObject.Copy();
            return poly;
        }
        public bool RemovePoints(List<int>indices)
        {
            indices.Sort();
            for (int j = indices.Count-1; j >= 0; j--)
            {
                int id = indices[j];
                points.RemoveAt(id);                
            }
            return true;
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

        public ClockDirection GetClockDirection()
        {
            return MyMath.DetectDirection(points);
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
                //BezierSmooth bz = new BezierSmooth();
                //bz.AddPoint(points);
                //poly.points = bz.Smooth().points;
                //if (selfsmooth) points = poly.points;                
              //  List<Vector64>_points = PolygonProcessor.SmoothPolygon(poly.points, 20);
                //_points.RemoveAt(_points.Count-1);
                //poly.points.Clear();
                //poly.points = _points;
            }
            else
            {   //只有两个点的线段平滑存在问题，CreateSpline已修正
                CubicSpline spline = new CubicSpline();
                poly.points = spline.CreateSpline(points);                
                if (selfsmooth) points = poly.points;
            }
            return poly;
        }
        
        public override vec2 GetTextureCoord(Vector32 p,planEnum plan = planEnum.XOY)
        {
            vec2 tex = new vec2(-1,-1);
            if( p.X >= Minx && p.X <= Maxx && 
                p.Y >= Miny && p.Y <= Maxy )
            {
                tex.x = (float)((p.x - minx) / (maxx - minx));
                tex.y = (float)((p.y - miny) / (maxy - miny));
            }            
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
            if (triangledObject != null && update == false)
                return triangledObject;

            if (triangledObject != null) triangledObject.Clear();

            TriangleObj triangles = new TriangleObj();

            //这里的三角剖分，多边形必须是2D的
            Polygon2D poly2d = this;
            if (!IsPoly2D) poly2d = toProjectedPolygon();

            Poly2Tri.Polygon poly = new Polygon(poly2d);

            if (!P2T.Triangulate(poly)) 
            { 
                return null; 
            }

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

                if (poly2d.polygonSlicer != null)
                {
                    p1 = poly2d.polygonSlicer.toTracedPoint(new Vector64(p1.X, p1.Y, 0));
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

            for (int i = 0; i < triangles.points.Count; i++)
            {
                triangles.AddTexture(GetTextureCoord(triangles.points[i]));
            }

            triangledObject = triangles;
            return triangledObject;
        }
        /// <summary>
        /// 基于几何库NetTopologySuite的多边形约束三角剖分
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public TriangleObj NetTopologySuiteTriangulate(bool update = true)
        {
            if (triangledObject != null && update == false)
                return triangledObject;

            if (triangledObject != null) triangledObject.Clear();

            TriangleObj triangles = new TriangleObj();

            //这里的三角剖分，多边形必须是2D的
            Polygon2D poly2d = this;
           // if (!IsPoly2D) poly2d = toProjectedPolygon();

            PolygonTriangulator triangulator = new PolygonTriangulator();
            if (!triangulator.CreateTriangles(poly2d)) 
            {
                errMessage = triangulator.errMessage;
                return null; 
            }           
            

            Vector32 p1 = new Vector32();
            Vector32 p2 = new Vector32();
            Vector32 p3 = new Vector32();
            float z = (float)points[0].Z;
            float v = (float)points[0].V;
            int n = 0;

            List<Vector32> tri_points = triangulator.toTriangleCoords();
            for(int i = 0; i < tri_points.Count/3; i++ )
            {
                p1 = tri_points[3*i];
                p2 = tri_points[3 * i+1];
                p3 = tri_points[3 * i+2];
                if (poly2d.polygonSlicer != null)
                {
                    p1 = poly2d.polygonSlicer.toTracedPoint(new Vector64(p1.X, p1.Y, 0));
                    p2 = poly2d.polygonSlicer.toTracedPoint(new Vector64(p2.X, p2.Y, 0));
                    p3 = poly2d.polygonSlicer.toTracedPoint(new Vector64(p3.X, p3.Y, 0));
                }
                triangles.AddPoint(p1);
                triangles.AddPoint(p2);
                triangles.AddPoint(p3);
                n = triangles.points.Count;
                triangles.AddTriangleIndex(n - 3, n - 2, n - 1);
            }
            
            triangles.UpdateRange();

            for (int i = 0; i < triangles.points.Count; i++)
            {
                triangles.AddTexture(GetTextureCoord(triangles.points[i]));
            }

            triangledObject = triangles;
            return triangledObject;
        }
        /*
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
       */
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
        
        public bool LoadFrom123(BinaryReader br)
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

            //added 2023-1-16
            br.Write(SmoothDraw);
            br.Write(ShowLine);
            br.Write((int)dashStyle);
            fillMethod.Save(br);

            return true;
        }
        
        public override bool LoadFrom(BinaryReader br)
        {
            if (C3DData.DataVersion <= 1.23f) return LoadFrom123(br);

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
            //added 2023 - 1 - 16
            SmoothDraw = br.ReadBoolean();
            ShowLine = br.ReadBoolean();
            dashStyle = (DashStyle)br.ReadInt32();
            fillMethod.Load(br);
            return true;
        }
        public override bool SaveAs(string path,int version = 0)
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
        public List<Vector64> SampleBoudary(double _step, double errbase = 1000,int sampNum = 100)
        {
            List<Vector64> lists = new List<Vector64>();
            if (points.Count < 2) return lists;

            double len;
            double total_len = Vector64.GetLength(points,-1,IsClosed);
            double step = total_len / sampNum;
            if (_step < step) step = _step;

            //误差容差，小于这个容差的点默认为0
            double err = step / errbase;
            Vector64 p, p1, p2; 
            
            int num = points.Count;
            if ( IsClosed ) num++;
            p1 = points[0];            
            for (int i = 1; i < num; i++) //中间的采样点数
            {
                if (i == points.Count) p2 = points[0];
                else p2 = points[i];
                len = p1.Distance(p2);//线段长度
                
                if(len <= step) lists.Add(p1);
                
                //len > step: 线段p1 - p2 分段采样
                while ( len > step )
                {
                    p = p1 + (p2 - p1) * step / len;
                    lists.Add(p);
                    p1 = p;
                    len -= step;
                }

                //添加最后一段
                if ( Math.Abs(len) > err && i == points.Count) 
                    lists.Add(p2);
                                
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
        /// <summary>
        /// 计算点到多边形的最短距离
        /// </summary>
        /// <param name="point">待计算的点</param>
        /// <param name="polygon">多边形顶点列表（按顺时针/逆时针顺序）</param>
        /// <returns>点到多边形的最短距离</returns>        
        public override double GetNearestDistance(Vector64 point)
        {
            // 边界检查：空多边形直接返回无穷大
            if (Count < 3) return double.NaN;                

            // 步骤1：判断点是否在多边形内部，内部则距离为0
            if (IsPointInsidePoly(point.X,point.Y))return 0.0;

            // 步骤2：点在外部，计算到每条边的最短距离
            double minDistance = double.MaxValue;
            int vertexCount = Count;

            // 遍历所有边（最后一条边是 polygon[vertexCount-1] -> polygon[0]）
            for (int i = 0; i < vertexCount; i++)
            {
                Vector64 a = points[i];
                Vector64 b = points[(i + 1) % vertexCount];
                double distance = DistanceToLineSegment(point, a, b);

                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }
        /// <summary>
        /// 计算点到线段的最短距离（核心几何计算）
        /// </summary>
        /// <param name="p">待计算的点</param>
        /// <param name="a">线段起点</param>
        /// <param name="b">线段终点</param>
        /// <returns>点到线段的最短距离</returns>
        private double DistanceToLineSegment(Vector64 p, Vector64 a, Vector64 b)
        {
            // 向量AB
            Vector64 ab = b - a;
            // 向量AP
            Vector64 ap = p - a;

            // 计算投影参数t：t = (AP · AB) / |AB|²
            double dotProduct = ap.X * ab.X + ap.Y * ab.Y;
            if (dotProduct <= 0) // 投影在A点左侧，最短距离是PA
                return p.Distance(a);

            double abLengthSquared = ab.X * ab.X + ab.Y * ab.Y;
            if (abLengthSquared <= 0) // 线段退化为点
                return p.Distance(a);

            double t = dotProduct / abLengthSquared;
            if (t >= 1) // 投影在B点右侧，最短距离是PB
                return p.Distance(b);

            // 投影在线段内部，计算投影点Q并返回PQ距离
            Vector64 q = new Vector64(a.X + t * ab.X, a.Y + t * ab.Y,0);
            return p.Distance(q);
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
        public string Name = "Untitled";//线段名称
        public int Id = 0; //线段编号
        public Vector64 p1;
        public Vector64 p2;
        public CLine()
        {

        }
        public CLine(Vector64 _p1, Vector64 _p2,int id = -1, string name = "Untitled")
        {
            p1 = _p1;
            p2 = _p2;
            Id = id;
            Name = name;
        }
        public CLine(Vector32 _p1, Vector32 _p2, int id=-1,string name = "Untitled")
        {
            p1 = new Vector64(_p1.x, _p1.y, _p1.z);
            p2 = new Vector64(_p2.x, _p2.y, _p2.z);
            Id = id;
            Name = name;
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
        public double XDirection
        {
            get { return p2.x - p1.x; }
        }
        public double YDirection
        {
            get { return p2.y - p1.y; }
        }
        public double ZDirection
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
        public double Length 
        {
            get 
            {
                return p1.Distance(p2,2);
            }
        }
        static public bool IsZero(double val, double zero = 1.0E-20)
        {
            double v = val;
            if (v < 0) v = -v;
            if (v <= zero) return true;
            else return false;
        }
        public bool IsPointOnLine(Vector64 p, double tolerance = 1e-8)
        {
            // 1. 检查点是否在包围盒内
            double minX = Math.Min(p1.X, p2.X) - tolerance;
            double maxX = Math.Max(p1.X, p2.X) + tolerance;
            double minY = Math.Min(p1.Y, p2.Y) - tolerance;
            double maxY = Math.Max(p1.Y, p2.Y) + tolerance;

            if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
                return false;

            // 2. 向量叉乘判断共线性（面积法）
            double cross = (p2.X - p1.X) * (p.Y - p1.Y) - (p2.Y - p1.Y) * (p.X - p1.X);
            return Math.Abs(cross) < tolerance;
        }
        /// <summary>
        /// 计算点到线段的垂直距离（带符号，区分左右/上下）
        /// </summary>
        public double GetSignedDistanceToPoint(Vector64 p)
        {
            // 向量AB
            double abX = p2.X - p1.X;
            double abY = p2.Y - p1.Y;
            // 向量AP
            double apX = p.X - p1.X;
            double apY = p.Y - p1.Y;

            // 叉乘结果的符号表示点在直线的哪一侧
            double cross = abX * apY - abY * apX;
            // 线段长度
            double len = Math.Sqrt(abX * abX + abY * abY);

            if (len < 1e-8) // 线段退化为点
                return p.Distance(p1);

            // 带符号的垂直距离
            return cross / len;
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

            if ( Math.Abs(sq2) <= 1e-8) return p1; //直线坍缩为点

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
            p = new Vector64(double.NaN, double.NaN, 0);
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


        /// <summary>
        /// 计算两条线段的交点
        /// </summary>
        /// <param name="p1">L1起点</param>
        /// <param name="p2">L1终点</param>
        /// <param name="p3">L2起点</param>
        /// <param name="p4">L2终点</param>
        /// <param name="intersection">输出交点坐标（无交点时为默认值）</param>
        /// <param name="epsilon">浮点精度阈值（默认1e-8）</param>
        /// <returns>是否存在有效交点</returns>
        public static bool CalculateIntersection(CLine line1,CLine line2,out Vector64 intersection,double epsilon = 1e-8)
        {
            Vector64 p1 = line1.p1;
            Vector64 p2 = line1.p2;
            Vector64 p3 = line2.p1;
            Vector64 p4 = line2.p2;
            intersection = new Vector64(double.NaN, double.NaN, 0);

            // 1. 计算向量与分母（判断是否平行）
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dx2 = p4.X - p3.X;
            double dy2 = p4.Y - p3.Y;
            double dx3 = p1.X - p3.X;
            double dy3 = p1.Y - p3.Y;

            // 分母：两线段方向向量的叉乘（denom=0 → 平行/重合）
            double denom = dx1 * dy2 - dy1 * dx2;
            // 分子：用于计算参数t和s
            double tNum = dx2 * dy3 - dy2 * dx3;
            double sNum = dx1 * dy3 - dy1 * dx3;

            // 2. 处理平行/重合场景
            if (Math.Abs(denom) < epsilon)
            {
                // 2.1 平行但不重合（无交点）
                if (Math.Abs(tNum) > epsilon || Math.Abs(sNum) > epsilon)
                    return false;

                // 2.2 重合，判断是否有重叠区间
                return GetOverlapPoint(p1, p2, p3, p4, out intersection, epsilon);
            }

            // 3. 非平行场景，计算参数t和s
            double t = tNum / denom;
            double s = sNum / denom;

            // 4. 校验t和s是否在[0,1]范围内（交点在线段上）
            if (t >= -epsilon && t <= 1 + epsilon && s >= -epsilon && s <= 1 + epsilon)
            {
                // 修正t/s到[0,1]区间（处理精度误差导致的微小越界）
                t = MyMath.Clamp(t, 0, 1);
                s = MyMath.Clamp(s, 0, 1);

                // 计算交点坐标
                intersection = new Vector64(p1.X + t * dx1,p1.Y + t * dy1,0,0);
                return true;
            }

            // 5. 交点在直线上但不在线段上
            return false;
        }

        #region 辅助方法
        /// <summary>
        /// 判断点是否在线段上（兼容精度）
        /// </summary>
        private static bool IsPointOnSegment(Vector64 p, Vector64 a, Vector64 b, double epsilon)
        {
            // 1. 点在线段的包围盒内
            bool inBox = (Math.Min(a.X, b.X) - epsilon <= p.X && p.X <= Math.Max(a.X, b.X) + epsilon) &&
                          (Math.Min(a.Y, b.Y) - epsilon <= p.Y && p.Y <= Math.Max(a.Y, b.Y) + epsilon);
            if (!inBox) return false;

            // 2. 点与线段共线（叉乘为0）
            double cross = (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X);
            return Math.Abs(cross) < epsilon;
        }

        /// <summary>
        /// 重合线段的重叠区间交点计算（返回任意一个重叠点）
        /// </summary>
        private static bool GetOverlapPoint(Vector64 p1, Vector64 p2, Vector64 p3, Vector64 p4, out Vector64 overlapPoint, double epsilon)
        {
            overlapPoint = new Vector64();

            // 检查各端点是否在对方线段上
            if (IsPointOnSegment(p1, p3, p4, epsilon)) { overlapPoint = p1; return true; }
            if (IsPointOnSegment(p2, p3, p4, epsilon)) { overlapPoint = p2; return true; }
            if (IsPointOnSegment(p3, p1, p2, epsilon)) { overlapPoint = p3; return true; }
            if (IsPointOnSegment(p4, p1, p2, epsilon)) { overlapPoint = p4; return true; }

            // 无重叠区间
            return false;
        }
        #endregion

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
    public class CurveSimplifierFitter
    {
        // 数值稳定性参数
        private const double EPS = 1e-12;

        /// <summary>
        /// 精简原始点集（道格拉斯-普克算法，增加异常处理）
        /// </summary>
        public List<Vector64> SimplifyPoints(List<Vector64> originalPoints, double threshold)
        {
            // 深度拷贝，避免修改原始数据
            var points = new List<Vector64>(originalPoints);

            // 参数校验
            if (points == null || points.Count <= 2)
                return new List<Vector64>(points.Where(p => p.IsValid()));

            threshold = Math.Max(threshold, EPS); // 阈值≥极小值

            // 过滤无效点
            points = points.Where(p => p.IsValid()).ToList();
            if (points.Count <= 2)
                return new List<Vector64>(points);

            return DouglasPeucker(points, 0, points.Count - 1, threshold);
        }

        /// <summary>
        /// 道格拉斯-普克算法递归实现（增强稳定性）
        /// </summary>
        private List<Vector64> DouglasPeucker(List<Vector64> points, int startIdx, int endIdx, double threshold)
        {
            var simplified = new List<Vector64>();
            double maxDist = 0;
            int maxIdx = startIdx + 1;

            Vector64 lineStart = points[startIdx];
            Vector64 lineEnd = points[endIdx];

            // 跳过无效点
            if (!lineStart.IsValid() || !lineEnd.IsValid())
            {
                simplified.Add(lineStart);
                simplified.Add(lineEnd);
                return simplified;
            }

            // 找到距离线段最远的点
            for (int i = startIdx + 1; i < endIdx; i++)
            {
                if (!points[i].IsValid()) continue;

                double dist = Vector64.DistanceToLineSegment(points[i], lineStart, lineEnd);
                if (dist > maxDist && !double.IsNaN(dist))
                {
                    maxDist = dist;
                    maxIdx = i;
                }
            }

            // 递归条件：最远点距离超过阈值，保留该点并递归处理左右段
            if (maxDist > threshold && !double.IsNaN(maxDist))
            {
                var left = DouglasPeucker(points, startIdx, maxIdx, threshold);
                var right = DouglasPeucker(points, maxIdx, endIdx, threshold);

                // 合并时去重中间点，过滤无效点
                simplified.AddRange(left.Take(left.Count - 1).Where(p => p.IsValid()));
                simplified.AddRange(right.Where(p => p.IsValid()));
            }
            else
            {
                // 保留首尾点（确保有效）
                if (lineStart.IsValid()) simplified.Add(lineStart);
                if (lineEnd.IsValid() && lineEnd.Distance(lineStart) > EPS)
                    simplified.Add(lineEnd);
            }

            return simplified;
        }

        /// <summary>
        /// 基于精简后的点集拟合光滑曲线（修复NaN核心错误）
        /// </summary>
        public List<Vector64> FitSmoothCurve(List<Vector64> simplifiedPoints, double smoothFactor, int sampleCount = 100)
        {
            // 严格参数校验
            if (simplifiedPoints == null || simplifiedPoints.Count < 2)
                throw new ArgumentException("精简后的点集至少包含2个有效点");

            // 过滤无效点
            var validPoints = simplifiedPoints.Where(p => p.IsValid()).ToList();
            if (validPoints.Count < 2)
                throw new ArgumentException("有效点数量不足，无法拟合曲线");

            smoothFactor = smoothFactor; // 限制在0~1
            sampleCount = Math.Max(sampleCount, 10); // 最少10个采样点

            // 步骤1：参数化（弦长参数化，修复0/0错误）
            var tList = ParameterizePoints(validPoints);

            // 步骤2：生成B样条节点向量（修复节点与t不匹配问题）
            int degree = Math.Min(3, validPoints.Count - 1); // 动态调整阶数，避免阶数超过点数量
            var knots = GenerateKnotVector(tList, degree, smoothFactor);

            // 步骤3：求解控制点（修复矩阵奇异/NaN问题）
            var controlPoints = SolveControlPoints(validPoints, tList, knots, degree, smoothFactor);

            // 步骤4：采样生成光滑曲线（增加有效性检查）
            return SampleBSplineCurve(controlPoints, knots, degree, sampleCount);
        }

        /// <summary>
        /// 一键完成：精简原始点 + 拟合光滑曲线（全流程异常处理）
        /// </summary>
        public List<Vector64> SimplifyAndFit(List<Vector64> originalPoints, double simplifyThreshold,
                                           double smoothFactor, int sampleCount, out List<Vector64> simplifiedPoints)
        {
            // 全流程try-catch，保证稳定性
            try
            {
                // 第一步：精简点集
                simplifiedPoints = SimplifyPoints(originalPoints, simplifyThreshold);

                // 第二步：拟合光滑曲线
                return FitSmoothCurve(simplifiedPoints, smoothFactor, sampleCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"拟合过程出错：{ex.Message}");
                // 降级处理：返回原始点的线性插值
                simplifiedPoints = new List<Vector64>(originalPoints.Where(p => p.IsValid()));
                return LinearInterpolate(simplifiedPoints, sampleCount);
            }
        }

        #region 核心拟合方法（修复NaN错误）
        /// <summary>
        /// 弦长参数化（修复totalLength=0导致的0/0错误）
        /// </summary>
        private List<double> ParameterizePoints(List<Vector64> points)
        {
            var tList = new List<double> { 0.0 };
            double totalLength = 0.0;

            for (int i = 1; i < points.Count; i++)
            {
                if (!points[i].IsValid() || !points[i - 1].IsValid())
                {
                    totalLength += EPS; // 避免长度为0
                }
                else
                {
                    totalLength += points[i].Distance(points[i - 1]);
                }
                tList.Add(totalLength);
            }

            // 修复：当总长度接近0时，使用均匀参数化
            if (totalLength < EPS)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    tList[i] = (double)i / (points.Count - 1);
                }
            }
            else
            {
                // 归一化到0~1，增加数值保护
                for (int i = 0; i < tList.Count; i++)
                {
                    tList[i] = MyMath.Clamp(tList[i] / totalLength, 0.0, 1.0);
                }
            }

            return tList;
        }

        /// <summary>
        /// 生成B样条节点向量（修复节点与t不匹配问题）
        /// </summary>
        private List<double> GenerateKnotVector(List<double> tList, int degree, double smoothFactor)
        {
            int n = tList.Count;
            int m = n + degree + 1;
            var knots = new List<double>(new double[m]);

            // 首尾节点重复度=degree+1（确保边界条件）
            for (int i = 0; i <= degree; i++)
            {
                knots[i] = 0.0;
                knots[m - 1 - i] = 1.0;
            }

            // 修复：当n <= degree时，直接返回均匀节点
            if (n <= degree)
            {
                for (int i = degree + 1; i < m - degree - 1; i++)
                {
                    knots[i] = (double)(i - degree) / (n - degree);
                }
                return knots;
            }

            // 中间节点：基于光滑因子调整分布，修复权重映射
            double smoothWeight = 0.2 + smoothFactor * 0.6; // 映射到0.2~0.8，避免极端值
            for (int i = 1; i <= n - degree - 1; i++)
            {
                double sum = 0.0;
                int count = 0;
                for (int j = i; j < i + degree; j++)
                {
                    if (j < tList.Count)
                    {
                        sum += tList[j] * smoothWeight;
                        count++;
                    }
                }
                // 修复：避免除以0
                knots[degree + i] = count > 0 ? sum / count : (double)i / (n - degree);
                knots[degree + i] = MyMath.Clamp(knots[degree + i], 0.0, 1.0);
            }

            return knots;
        }

        /// <summary>
        /// 计算B样条基函数（修复NaN/Inf）
        /// </summary>
        private double[] ComputeBSplineBasis(double t, List<double> knots, int degree, int controlCount)
        {
            var basis = new double[controlCount];
            Array.Clear(basis, 0, controlCount);

            // 修复：当t超出节点范围时的处理
            t = MyMath.Clamp(t, knots[0], knots[knots.Count - 1] - EPS);

            var temp = new double[controlCount];

            // 0阶基函数（增加数值保护）
            for (int i = 0; i < controlCount; i++)
            {
                if (i + 1 >= knots.Count) continue;
                basis[i] = (knots[i] <= t && t < knots[i + 1]) ? 1.0 : 0.0;
            }

            // 递推计算高阶基函数（修复分母为0）
            for (int d = 1; d <= degree; d++)
            {
                Array.Copy(basis, temp, controlCount);
                Array.Clear(basis, 0, controlCount);

                for (int i = 0; i < controlCount - d; i++)
                {
                    if (i + d >= knots.Count || i + d + 1 >= knots.Count) continue;

                    double denom1 = knots[i + d] - knots[i];
                    double term1 = (denom1 > EPS) ? (t - knots[i]) / denom1 * temp[i] : 0.0;

                    double denom2 = knots[i + d + 1] - knots[i + 1];
                    double term2 = (denom2 > EPS) ? (knots[i + d + 1] - t) / denom2 * temp[i + 1] : 0.0;

                    basis[i] = term1 + term2;
                    // 数值保护：避免NaN/Inf
                    if (double.IsNaN(basis[i]) || double.IsInfinity(basis[i]))
                        basis[i] = 0.0;
                }
            }

            return basis;
        }

        /// <summary>
        /// 最小二乘求解控制点（修复NaN核心错误）
        /// </summary>
        private List<Vector64> SolveControlPoints(List<Vector64> points, List<double> tList,
                                                List<double> knots, int degree, double smoothFactor)
        {
            int n = points.Count;
            int c = Math.Min(n, 20); // 限制控制点数量，避免矩阵过大
            double smoothWeight = smoothFactor * 0.1; // 修复：光滑权重映射（0~0.1），避免矩阵奇异

            // 初始化控制点为有效点
            var controlPoints = new List<Vector64>();
            foreach (var p in points.Take(c))
            {
                controlPoints.Add(p.IsValid() ? p : new Vector64(0, 0, 0));
            }

            // 构建矩阵（增加对角占优项，避免奇异）
            var matrix = new double[c, c];
            var bx = new double[c];
            var by = new double[c];
            var bz = new double[c];

            // 初始化矩阵为对角占优（增加小的对角值，避免奇异）
            for (int i = 0; i < c; i++)
            {
                matrix[i, i] = EPS; // 对角加极小值，避免全0
            }

            // 填充矩阵（修复权重逻辑）
            for (int i = 0; i < n; i++)
            {
                if (!points[i].IsValid()) continue;

                var basis = ComputeBSplineBasis(tList[i], knots, degree, c);
                for (int j = 0; j < c; j++)
                {
                    if (double.IsNaN(basis[j]) || double.IsInfinity(basis[j]))
                        basis[j] = 0.0;

                    // 修复：矩阵元素计算（拟合项 + 光滑项）
                    matrix[i % c, j] += basis[j] * basis[j] + smoothWeight;
                    // 数值保护
                    if (double.IsNaN(matrix[i % c, j]))
                        matrix[i % c, j] = EPS;

                    bx[i % c] += points[i].X * basis[j];
                    by[i % c] += points[i].Y * basis[j];
                    bz[i % c] += points[i].Z * basis[j];
                }
            }

            // 高斯消元求解（增加异常处理）
            try
            {
                for (int dim = 0; dim < 3; dim++)
                {
                    double[] b = dim == 0 ? bx : (dim == 1 ? by : bz);
                    double[] solution = GaussianElimination(matrix, b);

                    for (int i = 0; i < Math.Min(c, solution.Length); i++)
                    {
                        // 数值保护：限制解的范围
                        double val = MyMath.Clamp(solution[i], -1e6, 1e6);
                        if (double.IsNaN(val) || double.IsInfinity(val))
                            val = controlPoints[i].X; // 降级为原始值

                        if (dim == 0) controlPoints[i] = new Vector64(val, controlPoints[i].Y, controlPoints[i].Z);
                        else if (dim == 1) controlPoints[i] = new Vector64(controlPoints[i].X, val, controlPoints[i].Z);
                        else controlPoints[i] = new Vector64(controlPoints[i].X, controlPoints[i].Y, val);
                    }
                }
            }
            catch (Exception)
            {
                // 降级：直接使用原始点作为控制点
                controlPoints = new List<Vector64>(points.Take(c));
            }

            // 过滤无效控制点
            return controlPoints.Where(p => p.IsValid()).ToList();
        }

        /// <summary>
        /// 高斯消元法求解线性方程组（增强数值稳定性）
        /// </summary>
        private double[] GaussianElimination(double[,] matrix, double[] b)
        {
            int n = b.Length;
            var aug = new double[n, n + 1];

            // 构建增广矩阵（增加数值保护）
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    aug[i, j] = double.IsNaN(matrix[i, j]) ? EPS : matrix[i, j];
                }
                aug[i, n] = double.IsNaN(b[i]) ? 0.0 : b[i];
            }

            // 前向消元（增加主元选择的稳定性）
            for (int i = 0; i < n; i++)
            {
                // 选主元（增加绝对值保护）
                int pivot = i;
                double maxVal = Math.Abs(aug[i, i]);
                for (int j = i; j < n; j++)
                {
                    double val = Math.Abs(aug[j, i]);
                    if (val > maxVal + EPS)
                    {
                        maxVal = val;
                        pivot = j;
                    }
                }

                // 交换行
                if (pivot != i)
                {
                    for (int j = i; j <= n; j++)
                    {
                        (aug[i, j], aug[pivot, j]) = (aug[pivot, j], aug[i, j]);
                    }
                }

                // 修复：主元为0时，增加极小值
                if (Math.Abs(aug[i, i]) < EPS)
                {
                    aug[i, i] = EPS;
                }

                // 消元
                for (int j = i + 1; j < n; j++)
                {
                    double factor = aug[j, i] / aug[i, i];
                    // 数值保护：限制因子范围
                    factor = MyMath.Clamp(factor, -1e6, 1e6);

                    for (int k = i; k <= n; k++)
                    {
                        aug[j, k] -= factor * aug[i, k];
                        // 数值保护
                        if (double.IsNaN(aug[j, k]))
                            aug[j, k] = 0.0;
                    }
                }
            }

            // 回代求解（增加数值保护）
            var solution = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0.0;
                for (int j = i + 1; j < n; j++)
                {
                    sum += aug[i, j] * solution[j];
                }

                // 修复：分母为0
                double denom = aug[i, i];
                if (Math.Abs(denom) < EPS)
                    denom = EPS;

                solution[i] = (aug[i, n] - sum) / denom;
                // 数值保护：限制解的范围
                solution[i] = MyMath.Clamp(solution[i], -1e6, 1e6);
                if (double.IsNaN(solution[i]) || double.IsInfinity(solution[i]))
                    solution[i] = 0.0;
            }

            return solution;
        }

        /// <summary>
        /// 采样生成B样条曲线（增加有效性检查）
        /// </summary>
        private List<Vector64> SampleBSplineCurve(List<Vector64> controlPoints,
                                                List<double> knots, int degree, int sampleCount)
        {
            var curvePoints = new List<Vector64>();
            int c = controlPoints.Count;

            // 修复：控制点不足时，返回线性插值
            if (c < 2)
            {
                return LinearInterpolate(controlPoints, sampleCount);
            }

            // 均匀采样t∈[0,1]
            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)i / (sampleCount - 1);
                var basis = ComputeBSplineBasis(t, knots, degree, c);

                // 加权计算采样点（增加数值保护）
                Vector64 p = new Vector64();
                double totalWeight = 0.0;
                for (int j = 0; j < c; j++)
                {
                    if (!controlPoints[j].IsValid() || double.IsNaN(basis[j]))
                        continue;

                    p = p + controlPoints[j] * basis[j];
                    totalWeight += basis[j];
                }

                // 归一化权重，避免偏移
                if (totalWeight > EPS)
                {
                    p = p * (1.0 / totalWeight);
                }

                // 确保点有效
                if (!p.IsValid())
                {
                    p = new Vector64(0, 0, 0);
                }

                curvePoints.Add(p);
            }

            return curvePoints;
        }

        /// <summary>
        /// 降级方案：线性插值（当B样条拟合失败时）
        /// </summary>
        private List<Vector64> LinearInterpolate(List<Vector64> points, int sampleCount)
        {
            var result = new List<Vector64>();
            if (points.Count < 2)
            {
                // 单点时，返回重复点
                Vector64 p = points.Count > 0 ? points[0] : new Vector64(0, 0, 0);
                for (int i = 0; i < sampleCount; i++)
                    result.Add(p);
                return result;
            }

            // 线性插值
            double step = (double)(points.Count - 1) / (sampleCount - 1);
            for (int i = 0; i < sampleCount; i++)
            {
                double t = i * step;
                int idx = (int)Math.Floor(t);
                if (idx >= points.Count - 1)
                {
                    result.Add(points.Last());
                    continue;
                }

                double frac = t - idx;
                Vector64 p1 = points[idx];
                Vector64 p2 = points[idx + 1];
                Vector64 p = new Vector64(
                    p1.X + (p2.X - p1.X) * frac,
                    p1.Y + (p2.Y - p1.Y) * frac,
                    p1.Z + (p2.Z - p1.Z) * frac
                );
                result.Add(p);
            }

            return result;
        }
        #endregion
    }
    public class TrajectoryGenerator
    {
        // 数值稳定性阈值
        private const double EPS = 1e-12;

        /// <summary>
        /// 生成点集的主趋势轨迹线
        /// </summary>
        /// <param name="originalPoints">原始空间点集</param>
        /// <param name="trajectoryPointCount">轨迹线输出点数（默认50）</param>
        /// <param name="linearityThreshold">线性判断阈值（0~1，越小越严格，默认0.1）</param>
        /// <returns>代表点集走向的轨迹线点序列</returns>
        /// <exception cref="ArgumentException">输入点集无效</exception>
        public List<Vector64> GenerateTrajectory(List<Vector64> originalPoints,
                                               int trajectoryPointCount = 50,
                                               double linearityThreshold = 0.1)
        {
            // 步骤1：预处理点集（过滤无效点、去重、排序）
            var processedPoints = PreprocessPoints(originalPoints);
            if (processedPoints.Count < 2)
                throw new ArgumentException("有效点数量不足，无法生成轨迹线（至少需要2个有效点）");

            // 步骤2：判断点集的主趋势（线性/曲线）
            bool isLinear = IsLinearTrend(processedPoints, linearityThreshold);

            // 步骤3：根据趋势生成轨迹线
            List<Vector64> trajectory;
            if (isLinear)
            {
                // 线性走向：拟合空间直线，采样生成轨迹
                trajectory = GenerateLinearTrajectory(processedPoints, trajectoryPointCount);
            }
            else
            {
                // 曲线走向：保留特征点+移动平均，生成平滑且走向一致的轨迹
                trajectory = GenerateCurveTrajectory(processedPoints, trajectoryPointCount);
            }

            return trajectory;
        }

        #region 核心处理逻辑
        /// <summary>
        /// 点集预处理（过滤无效点、去重、按路径排序）
        /// </summary>
        private List<Vector64> PreprocessPoints(List<Vector64> originalPoints)
        {
            if (originalPoints == null) return new List<Vector64>();

            // 1. 过滤无效点
            var validPoints = originalPoints.Where(p => p.IsValid()).ToList();
            if (validPoints.Count == 0) return new List<Vector64>();

            // 2. 去重（距离小于EPS的点视为同一点）
            var uniquePoints = new List<Vector64>();
            foreach (var p in validPoints)
            {
                if (!uniquePoints.Any(up => up.Distance(p) < EPS))
                {
                    uniquePoints.Add(p);
                }
            }

            // 3. 按路径排序（从第一个点开始，依次找最近点，模拟路径走向）
            var sortedPoints = new List<Vector64> { uniquePoints[0] };
            var remainingPoints = new List<Vector64>(uniquePoints.Skip(1));

            while (remainingPoints.Count > 0)
            {
                var lastPoint = sortedPoints.Last();
                // 找最近的点
                var nearestPoint = remainingPoints.OrderBy(p => p.Distance(lastPoint)).First();
                sortedPoints.Add(nearestPoint);
                remainingPoints.Remove(nearestPoint);
            }

            return sortedPoints;
        }

        /// <summary>
        /// 判断点集是否为线性走向（核心：拟合直线后计算点到直线的平均距离）
        /// </summary>
        /// <param name="points">预处理后的点集</param>
        /// <param name="threshold">线性阈值（0~1，平均距离/总长度 < 阈值则为线性）</param>
        /// <returns>是否为线性走向</returns>
        private bool IsLinearTrend(List<Vector64> points, double threshold)
        {
            // 拟合空间直线（最小二乘法）
            Vector64 lineStart, lineEnd;
            FitLineLeastSquares(points, out lineStart, out lineEnd);

            // 计算所有点到直线的平均距离
            double totalDistance = 0;
            foreach (var p in points)
            {
                totalDistance += DistanceToLine(p, lineStart, lineEnd);
            }
            double avgDistance = totalDistance / points.Count;

            // 计算点集的总长度（首尾点距离）
            double totalLength = lineStart.Distance(lineEnd);
            if (totalLength < EPS) totalLength = 1; // 避免除以0

            // 平均距离占总长度的比例 < 阈值 → 线性走向
            return (avgDistance / totalLength) < MyMath.Clamp(threshold, 0.01, 1.0);
        }

        /// <summary>
        /// 最小二乘法拟合空间直线（输出直线的首尾点）
        /// </summary>
        private void FitLineLeastSquares(List<Vector64> points, out Vector64 lineStart, out Vector64 lineEnd)
        {
            // 计算点集的中心点
            double avgX = points.Average(p => p.X);
            double avgY = points.Average(p => p.Y);
            double avgZ = points.Average(p => p.Z);
            Vector64 centroid = new Vector64(avgX, avgY, avgZ);

            // 计算协方差矩阵
            double xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;
            foreach (var p in points)
            {
                double dx = p.X - avgX;
                double dy = p.Y - avgY;
                double dz = p.Z - avgZ;
                xx += dx * dx;
                xy += dx * dy;
                xz += dx * dz;
                yy += dy * dy;
                yz += dy * dz;
                zz += dz * dz;
            }

            // 构造特征值问题（简化版：找主方向）
            double[] eigenVector = new double[3];
            // 计算主方向（简化版，适用于大多数场景）
            double maxVariance = 0;
            // X方向方差
            if (xx > maxVariance) { maxVariance = xx; eigenVector = new[] { 1.0, 0.0, 0.0 }; }
            // Y方向方差
            if (yy > maxVariance) { maxVariance = yy; eigenVector = new[] { 0.0, 1.0, 0.0 }; }
            // Z方向方差
            if (zz > maxVariance) { maxVariance = zz; eigenVector = new[] { 0.0, 0.0, 1.0 }; }

            // 构造直线的方向向量
            Vector64 dir = new Vector64(eigenVector[0], eigenVector[1], eigenVector[2]).Normalize();
            // 计算直线的首尾点（沿主方向延伸，覆盖所有点）
            double maxDist = points.Max(p => Math.Abs((p - centroid).X * dir.X + (p - centroid).Y * dir.Y + (p - centroid).Z * dir.Z));

            lineStart = centroid - dir * maxDist;
            lineEnd = centroid + dir * maxDist;
        }

        /// <summary>
        /// 计算点到空间直线的垂直距离
        /// </summary>
        private double DistanceToLine(Vector64 p, Vector64 lineStart, Vector64 lineEnd)
        {
            Vector64 v = lineEnd - lineStart;
            Vector64 w = p - lineStart;

            double c1 = w.X * v.X + w.Y * v.Y + w.Z * v.Z;
            double c2 = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            if (c2 < EPS) return w.Distance(new Vector64(0, 0, 0));

            double b = c1 / c2;
            Vector64 pb = lineStart + v * b;
            return p.Distance(pb);
        }

        /// <summary>
        /// 生成线性走向的轨迹线（沿拟合直线等间距采样）
        /// </summary>
        private List<Vector64> GenerateLinearTrajectory(List<Vector64> points, int pointCount)
        {
            // 重新拟合直线（确保覆盖所有点）
            Vector64 lineStart, lineEnd;
            FitLineLeastSquares(points, out lineStart, out lineEnd);

            // 等间距采样
            List<Vector64> trajectory = new List<Vector64>();
            Vector64 dir = lineEnd - lineStart;
            double step = 1.0 / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                double t = i * step;
                Vector64 p = lineStart + dir * t;
                trajectory.Add(p);
            }

            return trajectory;
        }

        /// <summary>
        /// 生成曲线走向的轨迹线（保留特征点+移动平均，保证走向）
        /// </summary>
        private List<Vector64> GenerateCurveTrajectory(List<Vector64> points, int pointCount)
        {
            // 步骤1：提取关键特征点（道格拉斯-普克算法，保留曲线走向）
            var keyPoints = DouglasPeucker(points, 0, points.Count - 1, GetAdaptiveThreshold(points));
            if (keyPoints.Count < 2) keyPoints = points;

            // 步骤2：移动平均平滑（避免毛刺，保留走向）
            var smoothedKeyPoints = MovingAverageSmooth(keyPoints, windowSize: 3);

            // 步骤3：等间距插值生成指定数量的轨迹点
            return InterpolateCurve(smoothedKeyPoints, pointCount);
        }

        /// <summary>
        /// 道格拉斯-普克算法提取关键特征点（保留曲线走向，删除冗余点）
        /// </summary>
        private List<Vector64> DouglasPeucker(List<Vector64> points, int startIdx, int endIdx, double threshold)
        {
            double maxDist = 0;
            int maxIdx = startIdx + 1;

            Vector64 start = points[startIdx];
            Vector64 end = points[endIdx];

            // 找距离线段最远的点
            for (int i = startIdx + 1; i < endIdx; i++)
            {
                double dist = DistanceToLine(points[i], start, end);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxIdx = i;
                }
            }

            // 递归保留特征点
            if (maxDist > threshold)
            {
                var left = DouglasPeucker(points, startIdx, maxIdx, threshold);
                var right = DouglasPeucker(points, maxIdx, endIdx, threshold);
                return left.Take(left.Count - 1).Concat(right).ToList();
            }
            else
            {
                return new List<Vector64> { start, end };
            }
        }

        /// <summary>
        /// 自适应计算道格拉斯-普克阈值（基于点集密度）
        /// </summary>
        private double GetAdaptiveThreshold(List<Vector64> points)
        {
            // 计算点集的平均间距
            double totalDist = 0;
            for (int i = 1; i < points.Count; i++)
            {
                totalDist += points[i].Distance(points[i - 1]);
            }
            double avgDist = totalDist / (points.Count - 1);
            // 阈值=平均间距的1/2，保证保留核心特征
            return avgDist * 0.5;
        }

        /// <summary>
        /// 移动平均平滑（保留走向，消除小毛刺）
        /// </summary>
        private List<Vector64> MovingAverageSmooth(List<Vector64> points, int windowSize)
        {
            if (points.Count <= windowSize) return points;

            var smoothed = new List<Vector64>();
            // 首尾点保留
            smoothed.Add(points[0]);

            // 中间点移动平均
            for (int i = windowSize / 2; i < points.Count - windowSize / 2; i++)
            {
                double sumX = 0, sumY = 0, sumZ = 0;
                int count = 0;
                for (int j = i - windowSize / 2; j <= i + windowSize / 2; j++)
                {
                    sumX += points[j].X;
                    sumY += points[j].Y;
                    sumZ += points[j].Z;
                    count++;
                }
                smoothed.Add(new Vector64(sumX / count, sumY / count, sumZ / count));
            }

            // 尾点保留
            smoothed.Add(points.Last());
            return smoothed;
        }

        /// <summary>
        /// 曲线插值（等间距采样，保证轨迹点数量）
        /// </summary>
        private List<Vector64> InterpolateCurve(List<Vector64> keyPoints, int pointCount)
        {
            // 计算关键特征点的累计长度
            var cumLength = new List<double> { 0 };
            double totalLength = 0;
            for (int i = 1; i < keyPoints.Count; i++)
            {
                totalLength += keyPoints[i].Distance(keyPoints[i - 1]);
                cumLength.Add(totalLength);
            }

            // 等间距采样
            List<Vector64> trajectory = new List<Vector64>();
            double step = totalLength / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                double targetLength = i * step;
                // 找到目标长度所在的线段
                int segIdx = 0;
                for (int j = 1; j < cumLength.Count; j++)
                {
                    if (cumLength[j] >= targetLength)
                    {
                        segIdx = j - 1;
                        break;
                    }
                }

                // 插值计算当前点
                if (segIdx >= keyPoints.Count - 1)
                {
                    trajectory.Add(keyPoints.Last());
                }
                else
                {
                    double segLen = cumLength[segIdx + 1] - cumLength[segIdx];
                    double t = segLen < EPS ? 0 : (targetLength - cumLength[segIdx]) / segLen;
                    Vector64 p1 = keyPoints[segIdx];
                    Vector64 p2 = keyPoints[segIdx + 1];
                    Vector64 interpolated = new Vector64(
                        p1.X + (p2.X - p1.X) * t,
                        p1.Y + (p2.Y - p1.Y) * t,
                        p1.Z + (p2.Z - p1.Z) * t
                    );
                    trajectory.Add(interpolated);
                }
            }

            return trajectory;
        }
        #endregion
    }
    public class CurveExtensionCalculator
    {
        private const double EPS = 1e-12; // 数值稳定性阈值

        /// <summary>
        /// 计算空间曲线的前后延长点（各1个）
        /// </summary>
        /// <param name="curvePoints">原始曲线点集（已按曲线顺序排序，≥5个有效点）</param>
        /// <param name="extensionRatio">延长距离比例（基于末端平均间距，默认1.0）</param>
        /// <param name="frontExtensionPoint">输出：前端延长点（起点向前）</param>
        /// <param name="backExtensionPoint">输出：后端延长点（终点向后）</param>
        /// <exception cref="ArgumentException">输入点集无效</exception>
        public void CalculateExtensionPoints(List<Vector64> curvePoints,
                                            out Vector64 frontExtensionPoint,
                                            out Vector64 backExtensionPoint,
                                            double extensionRatio = 1.0)
        {
            // 步骤1：预处理点集（过滤无效点、校验数量）
            var processedPoints = PreprocessCurvePoints(curvePoints);
            if (processedPoints.Count < 5)
                throw new ArgumentException("曲线点集需包含至少5个有效点");

            // 步骤2：计算自适应延长距离（基于末端点间距）
            double extensionDistance = CalculateAdaptiveExtensionDistance(processedPoints, extensionRatio);

            // 步骤3：计算前端延长点（起点向前）
            frontExtensionPoint = CalculateFrontExtensionPoint(processedPoints, extensionDistance);

            // 步骤4：计算后端延长点（终点向后）
            backExtensionPoint = CalculateBackExtensionPoint(processedPoints,   extensionDistance);
        }

        #region 核心计算逻辑
        /// <summary>
        /// 曲线点集预处理（过滤无效点、去重、校验顺序）
        /// </summary>
        private List<Vector64> PreprocessCurvePoints(List<Vector64> curvePoints)
        {
            if (curvePoints == null) return new List<Vector64>();

            // 1. 过滤无效点
            var validPoints = curvePoints.Where(p => p.IsValid()).ToList();
            if (validPoints.Count == 0) return new List<Vector64>();

            // 2. 去重（距离小于EPS的点视为同一点）
            var uniquePoints = new List<Vector64>();
            foreach (var p in validPoints)
            {
                if (!uniquePoints.Any(up => up.Distance(p) < EPS))
                {
                    uniquePoints.Add(p);
                }
            }

            return uniquePoints;
        }

        /// <summary>
        /// 计算自适应延长距离（基于末端3个点的平均间距）
        /// </summary>
        private double CalculateAdaptiveExtensionDistance(List<Vector64> processedPoints, double extensionRatio)
        {
            // 前端平均间距（前3个点）
            double frontAvgDist = 0;
            for (int i = 1; i < 3; i++)
            {
                frontAvgDist += processedPoints[i].Distance(processedPoints[i - 1]);
            }
            frontAvgDist /= 2;

            // 后端平均间距（后3个点）
            double backAvgDist = 0;
            for (int i = processedPoints.Count - 2; i < processedPoints.Count; i++)
            {
                backAvgDist += processedPoints[i].Distance(processedPoints[i - 1]);
            }
            backAvgDist /= 2;

            // 延长距离 = 平均间距 * 比例（保证≥EPS）
            double avgDist = (frontAvgDist + backAvgDist) / 2;
            return Math.Max(avgDist * MyMath.Clamp(extensionRatio, 0.1, 10.0), EPS);
        }

        /// <summary>
        /// 计算前端延长点（起点向前，基于前3个点的切线趋势）
        /// </summary>
        private Vector64 CalculateFrontExtensionPoint(List<Vector64> processedPoints, double extensionDistance)
        {
            // 取前3个点，拟合局部切线方向
            Vector64 p0 = processedPoints[0];
            Vector64 p1 = processedPoints[1];
            Vector64 p2 = processedPoints[2];

            // 计算前两个向量
            Vector64 v1 = p1 - p0;
            Vector64 v2 = p2 - p1;

            // 拟合前端切线方向（加权平均，更贴近起点）
            Vector64 frontDir = (v1 * 2 + v2).Normalize(); // v1权重更高，保证方向贴合起点

            // 前端延长点 = 起点 - 切线方向 * 延长距离（向前延长）
            Vector64 frontExtension = p0 - frontDir * extensionDistance;

            return frontExtension;
        }

        /// <summary>
        /// 计算后端延长点（终点向后，基于后3个点的切线趋势）
        /// </summary>
        private Vector64 CalculateBackExtensionPoint(List<Vector64> processedPoints, double extensionDistance)
        {
            // 取后3个点，拟合局部切线方向
            Vector64 pn_2 = processedPoints[processedPoints.Count - 3];
            Vector64 pn_1 = processedPoints[processedPoints.Count - 2];
            Vector64 pn = processedPoints[processedPoints.Count - 1];

            // 计算后两个向量
            Vector64 vn_2 = pn_1 - pn_2;
            Vector64 vn_1 = pn - pn_1;

            // 拟合后端切线方向（加权平均，更贴近终点）
            Vector64 backDir = (vn_2 + vn_1 * 2).Normalize(); // vn_1权重更高，保证方向贴合终点

            // 后端延长点 = 终点 + 切线方向 * 延长距离（向后延长）
            Vector64 backExtension = pn + backDir * extensionDistance;

            return backExtension;
        }
        #endregion
        
    }
    
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
        
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [CategoryAttribute("Arrow"), DisplayNameAttribute("Arrow3D Style")]        
        public Arrow3D Arrow { get; set; }=new Arrow3D(new Vector64(),new Vector64());

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


        public C3DLine toSmoothLine(double smoothFactor = 0.8,int sampleCount = 100 )
        {
            C3DLine line = new C3DLine(Name);

            var curveProcessor = new CurveSimplifierFitter();

            // 3. 配置参数（极端参数测试）
            double simplifyThreshold = 0.15; // 精简阈值
            // 4. 一键完成：精简+拟合
            List<Vector64> simplifiedPoints;
            var smoothCurve = curveProcessor.SimplifyAndFit(
                points,
                simplifyThreshold,
                smoothFactor,
                sampleCount,
                out simplifiedPoints
            );
            line.AddPoint(smoothCurve);
            line.UpdateRange();
            smoothCurve.Clear();
            return line;
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
                            string text = AscIIProfile.ReadLine(sr);
                            if(text != null && text.Length > 1)
                                AddPoint(Vector64.Parse(text, 4));
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

                //added 2025-12-26
                Arrow.Save(br);

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
                
                Arrow.Load(br);//added 2025-12-26

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
            double step = GetLength(3) / (ptNum - 1);

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

        public double CreateLenthes(int index = -1)
        {
            if (points.Count < 1) return 0;

            int id = index;
            if (id < 0) id = Count - 1;

            CreateDistances();

            return distances[id];
        }

        public double Length
        {
            get { return GetLength(3); }
        }
        public double Length2D
        {
            get { return GetLength(2); }
        }

        public double GetLength(int dimension)
        {
            if (Count < 2) return 0;
            Vector32 p1, p2;
            double sum = 0, len = 0;
            for (int i = 0; i <Count-1; i++)
            {
                p1 = points[i];
                p2 = points[i + 1];
                if (dimension == 3)
                {
                    len = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                            (p1.y - p2.y) * (p1.y - p2.y) +
                            (p1.z - p2.z) * (p1.z - p2.z));
                }
                else if (dimension == 2)
                {
                    len = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                            (p1.y - p2.y) * (p1.y - p2.y) );
                }
                else if (dimension == 1)
                {
                    len = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x));
                }
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
    /// <summary>
    /// 三维箭头生成器（含PLY导出）
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Arrow3D
    {
        public Vector64 Start = new Vector64();
        public Vector64 End = new Vector64();
        
        ArrowStyle _ArrowStyle = ArrowStyle.Right;
        [CategoryAttribute("Arrow"), DisplayNameAttribute("Style")]
        [Editor(typeof(ArrowStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(ArrowStyleConverter))]
        public ArrowStyle arrowStyle
        {
            get { return _ArrowStyle; }
            set { _ArrowStyle = value;}
        }
        
        [CategoryAttribute("Arrow"), DisplayNameAttribute("Color")]
        public Color Color { get; set; } = Color.Black;

        // 箭头核心参数（可自定义）
        [Category("Arrow"), DisplayName("HeadLengthRatio")]
        public double _arrowHeadLengthRatio { get; set; } = 0.3;    // 箭头头部长度占总长度比例
        [Category("Arrow"), DisplayName("HeadWidthRatio")]
        public double _arrowHeadWidthRatio { get; set; } = 0.12;    // 箭头头部宽度占总长度比例
        [Category("Arrow"), DisplayName("ShaftRadiusRatio")]
        public double _arrowShaftRadiusRatio { get; set; } = 0.01;  // 箭头杆半径占总长度比例
        [Category("Arrow"), DisplayName("SideCount")]
        public int _sideCount { get; set; } = 8;                    // 侧面数量（≥3）

        [Category("Arrow"), DisplayName("延长点距倍数")]
        public double drawExtentScale { get; set; } = 1.0;

        public bool Save(BinaryWriter wr)
        {
            wr.Write(Start.X); wr.Write(Start.Y); wr.Write(Start.Z);
            wr.Write(End.X); wr.Write(End.Y); wr.Write(End.Z);
            wr.Write((int)_ArrowStyle);
            wr.Write(Color.ToArgb());
            wr.Write(_arrowHeadLengthRatio);
            wr.Write(_arrowHeadWidthRatio);
            wr.Write(_arrowShaftRadiusRatio);            
            wr.Write(drawExtentScale);
            wr.Write(_sideCount);
            return true;
        }
        public bool Load(BinaryReader br)
        {
            double x = br.ReadDouble();
            double y = br.ReadDouble();
            double z = br.ReadDouble();
            Start = new Vector64(x,y,z);
            x = br.ReadDouble();
            y = br.ReadDouble();
            z = br.ReadDouble();
            End = new Vector64(x, y, z);
            _ArrowStyle = (ArrowStyle)br.ReadInt32();
            Color = Color.FromArgb(br.ReadInt32());
            _arrowHeadLengthRatio = br.ReadDouble();
            _arrowHeadWidthRatio = br.ReadDouble();
            _arrowShaftRadiusRatio = br.ReadDouble();
            drawExtentScale = br.ReadDouble();
            _sideCount = br.ReadInt32();

            return true;
        }
        /// <summary>
        /// 构造函数（新增侧面数量参数）
        /// </summary>
        /// <param name="sideCount">侧面数量（≥3，默认8）</param>
        /// <param name="headLengthRatio">头部长度比例（0.1-0.3）</param>
        /// <param name="headWidthRatio">头部宽度比例（0.1-0.2）</param>
        /// <param name="shaftRadiusRatio">杆半径比例（0.02-0.05）</param>
        public Arrow3D(Vector64 start, Vector64 end,
                                int sideCount = 8,
                                double headLengthRatio = 0.3,
                                double headWidthRatio = 0.12,
                                double shaftRadiusRatio = 0.01)
        {
            Start = start; End = end;
            // 强制保证侧面数量≥3
            _sideCount = sideCount < 3 ? 3 : sideCount;
            _arrowHeadLengthRatio = headLengthRatio;
            _arrowHeadWidthRatio = headWidthRatio;
            _arrowShaftRadiusRatio = shaftRadiusRatio;
        }

        /// <summary>
        /// 生成可配置侧面数量的圆润三维箭头
        /// </summary>
        public TriangleObj GenerateSmoothArrow()
        {
            Vector64 startPoint = Start, endPoint = End;
            TriangleObj tri = new TriangleObj();

            // 基础校验// throw new ArgumentException("起点和终点不能重合！");
            if (Math.Abs((startPoint - endPoint).Length) < 1e-6)
            {
                return tri;
            } 

            // ===================== 步骤1：计算基础向量和参数 =====================
            Vector64 dirVector = endPoint - startPoint; // 从起点到终点的方向向量
            double totalLength = dirVector.Length;   // 箭头总长度
            Vector64 dirUnit = Vector64.Normalize(dirVector); // 单位方向向量

            // 箭头关键参数（基于总长度计算）
            double arrowHeadLength = totalLength * _arrowHeadLengthRatio; // 箭头头部长度
            double arrowHeadWidth = totalLength * _arrowHeadWidthRatio;   // 箭头头部宽度
            double shaftRadius = totalLength * _arrowShaftRadiusRatio;    // 箭头杆半径

            // 箭头头部基准点（从终点向起点回退头部长度）
            Vector64 headBasePoint = endPoint - (dirUnit * arrowHeadLength);

            // ===================== 步骤2：生成正交基向量（确保圆周均分） =====================
            Vector64 orthoVec1, orthoVec2;
            CreateOrthonormalBasis(dirUnit, out orthoVec1, out orthoVec2);

            // ===================== 步骤3：动态生成圆周顶点（核心：按侧面数量均分） =====================
            List<Vector64> shaftStartVertices = new List<Vector64>(); // 箭头杆起点圆周顶点
            List<Vector64> shaftEndVertices = new List<Vector64>();   // 箭头杆终点圆周顶点
            List<Vector64> headSideVertices = new List<Vector64>();   // 箭头头部侧点

            // 按侧面数量均分圆周（角度步长 = 2π / 侧面数）
            double angleStep = 2 * Math.PI / _sideCount;
            for (int i = 0; i < _sideCount; i++)
            {
                double angle = i * angleStep;
                // 计算当前角度的单位圆周坐标（cosθ, sinθ）
                double cosA = Math.Cos(angle);
                double sinA = Math.Sin(angle);

                // --- 生成箭头杆顶点 ---
                // 起点圆周顶点（半径=shaftRadius）
                Vector64 shaftStart = startPoint + (orthoVec1 * cosA * shaftRadius) + (orthoVec2 * sinA * shaftRadius);
                shaftStartVertices.Add(shaftStart);
                // 终点圆周顶点（半径=shaftRadius，位置在头部基准点）
                Vector64 shaftEnd = headBasePoint + (orthoVec1 * cosA * shaftRadius) + (orthoVec2 * sinA * shaftRadius);
                shaftEndVertices.Add(shaftEnd);

                // --- 生成箭头头部侧点（半径更大，更圆润） ---
                Vector64 headSide = headBasePoint + (orthoVec1 * cosA * arrowHeadWidth) + (orthoVec2 * sinA * arrowHeadWidth);
                headSideVertices.Add(headSide);
            }
            
            // ===================== 汇总所有顶点 =====================
            // 1. 箭头杆起点顶点（sideCount个）
            tri.AddPoints(shaftStartVertices);
            // 2. 箭头杆终点顶点（sideCount个）
            tri.AddPoints(shaftEndVertices);
            // 3. 箭头头部尖点（1个）
            tri.AddPoint(endPoint);
            // 4. 箭头头部侧点（sideCount个）
            tri.AddPoints(headSideVertices);

            // ===================== 动态生成面（按侧面数量） =====================
            int shaftStartCount = _sideCount;          // 杆起点顶点数
            int shaftEndCount = _sideCount;            // 杆终点顶点数
            int headTipIndex = shaftStartCount + shaftEndCount; // 尖点索引

            // --- 1. 箭头杆的侧面（每个侧面拆分为2个三角面） ---
            for (int i = 0; i < _sideCount; i++)
            {
                int nextI = (i + 1) % _sideCount; // 下一个顶点索引（循环）

                // 当前杆起点顶点索引
                int sStartI = i;
                // 当前杆终点顶点索引
                int sEndI = shaftStartCount + i;
                // 下一个杆起点顶点索引
                int sStartNext = nextI;
                // 下一个杆终点顶点索引
                int sEndNext = shaftStartCount + nextI;

                // 生成两个三角面（逆时针顺序）
                tri.AddTriangleIndex(sStartI, sEndI, sEndNext);
                tri.AddTriangleIndex(sStartI, sEndNext, sStartNext);
            }

            // --- 2. 箭头头部的三角面（尖点指向侧点） ---
            for (int i = 0; i < _sideCount; i++)
            {
                int nextI = (i + 1) % _sideCount;
                // 头部侧点索引
                int headSideI = headTipIndex + 1 + i;
                int headSideNext = headTipIndex + 1 + nextI;

                // 尖点 → 当前侧点 → 下一个侧点
                tri.AddTriangleIndex( headTipIndex, headSideI, headSideNext );
            }

            // --- 3. 箭头头部与杆的过渡面 ---
            for (int i = 0; i < _sideCount; i++)
            {
                int nextI = (i + 1) % _sideCount;
                // 杆终点顶点索引
                int sEndI = shaftStartCount + i;
                int sEndNext = shaftStartCount + nextI;
                // 头部侧点索引
                int headSideI = headTipIndex + 1 + i;
                int headSideNext = headTipIndex + 1 + nextI;

                // 过渡面（三角面）
                tri.AddTriangleIndex( sEndI, headSideI, headSideNext );
                tri.AddTriangleIndex(sEndI, headSideNext, sEndNext );
            }
            tri.color = ConvertColor.Convert(Color);
            shaftStartVertices.Clear();
            shaftEndVertices.Clear();
            headSideVertices.Clear();

            return tri;
        }



        /// <summary>
        /// 生成正交基向量（确保箭头在三维空间对称）
        /// </summary>
        private void CreateOrthonormalBasis(Vector64 dir, out Vector64 ortho1, out Vector64 ortho2)
        {
            // 找到第一个垂直向量
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                ortho1 = new Vector64(-dir.Z, 0, dir.X);
            else
                ortho1 = new Vector64(0, dir.Z, -dir.Y);

            ortho1 = Vector64.Normalize(ortho1);
            // 第二个垂直向量 = 方向向量 × 第一个垂直向量
            ortho2 = Vector64.Normalize(Vector64.Cross(dir, ortho1));
        }

        /// <summary>
        /// 生成三维箭头的所有顶点和面
        /// </summary>        
        /// <param name="vertices">输出：箭头所有顶点</param>
        /// <param name="faces">输出：箭头所有面（顶点索引列表）</param>
        //public TriangleObj GenerateArrow()
        //{
        //    Vector64 p1 = Start;
        //    Vector64 p2 = End;
        //    // 异常处理：P1和P2重合
        //    if (Math.Abs(p1.X - p2.X) < 1e-6 && Math.Abs(p1.Y - p2.Y) < 1e-6 && Math.Abs(p1.Z - p2.Z) < 1e-6)
        //    {
        //        throw new ArgumentException("起点P1和终点P2不能重合！");
        //    }

        //    // 步骤1：计算基础向量
        //    Vector64 vecP1P2 = p2 - p1;
        //    double totalLength = Math.Sqrt(vecP1P2.X * vecP1P2.X + vecP1P2.Y * vecP1P2.Y + vecP1P2.Z * vecP1P2.Z);
        //    Vector64 u = Vector64.Normalize(vecP1P2); // P1-P2单位方向向量

        //    // 步骤2：生成垂直向量（用于箭头杆和头部的宽度）
        //    Vector64 v = GetPerpendicularVector(u);  // 垂直向量1
        //    Vector64 w = Vector64.Normalize(Vector64.Cross(u, v)); // 垂直向量2（与v正交）

        //    // 步骤3：计算关键参数
        //    double arrowHeadLength = totalLength * _arrowLengthRatio; // 箭头头部长度
        //    double arrowHeadWidth = arrowHeadLength * _arrowWidthRatio; // 箭头头部宽度
        //    double shaftRadius = totalLength * _shaftRadiusRatio; // 箭头杆半径
        //    Vector64 arrowBasePoint = new Vector64( // 箭头头部基准点（P2往P1回退）
        //        p2.X - u.X * arrowHeadLength,
        //        p2.Y - u.Y * arrowHeadLength,
        //        p2.Z - u.Z * arrowHeadLength
        //    );

        //    // ===================== 生成顶点 =====================
        //    // 1. 箭头杆顶点（4个：P1的上下左右）
        //    Vector64 shaftP1_1 = new Vector64(p1.X + v.X * shaftRadius, p1.Y + v.Y * shaftRadius, p1.Z + v.Z * shaftRadius);
        //    Vector64 shaftP1_2 = new Vector64(p1.X + w.X * shaftRadius, p1.Y + w.Y * shaftRadius, p1.Z + w.Z * shaftRadius);
        //    Vector64 shaftP1_3 = new Vector64(p1.X - v.X * shaftRadius, p1.Y - v.Y * shaftRadius, p1.Z - v.Z * shaftRadius);
        //    Vector64 shaftP1_4 = new Vector64(p1.X - w.X * shaftRadius, p1.Y - w.Y * shaftRadius, p1.Z - w.Z * shaftRadius);

        //    // 2. 箭头杆末端顶点（箭头基准点的上下左右）
        //    Vector64 shaftP2_1 = new Vector64(arrowBasePoint.X + v.X * shaftRadius, arrowBasePoint.Y + v.Y * shaftRadius, arrowBasePoint.Z + v.Z * shaftRadius);
        //    Vector64 shaftP2_2 = new Vector64(arrowBasePoint.X + w.X * shaftRadius, arrowBasePoint.Y + w.Y * shaftRadius, arrowBasePoint.Z + w.Z * shaftRadius);
        //    Vector64 shaftP2_3 = new Vector64(arrowBasePoint.X - v.X * shaftRadius, arrowBasePoint.Y - v.Y * shaftRadius, arrowBasePoint.Z - v.Z * shaftRadius);
        //    Vector64 shaftP2_4 = new Vector64(arrowBasePoint.X - w.X * shaftRadius, arrowBasePoint.Y - w.Y * shaftRadius, arrowBasePoint.Z - w.Z * shaftRadius);

        //    // 3. 箭头头部顶点（4个：尖点+3个基准侧点）
        //    Vector64 arrowTip = p2; // 箭头尖点
        //    Vector64 arrowHead_1 = new Vector64(arrowBasePoint.X + v.X * arrowHeadWidth, arrowBasePoint.Y + v.Y * arrowHeadWidth, arrowBasePoint.Z + v.Z * arrowHeadWidth);
        //    Vector64 arrowHead_2 = new Vector64(arrowBasePoint.X + w.X * arrowHeadWidth, arrowBasePoint.Y + w.Y * arrowHeadWidth, arrowBasePoint.Z + w.Z * arrowHeadWidth);
        //    Vector64 arrowHead_3 = new Vector64(arrowBasePoint.X - v.X * arrowHeadWidth, arrowBasePoint.Y - v.Y * arrowHeadWidth, arrowBasePoint.Z - v.Z * arrowHeadWidth);
        //    Vector64 arrowHead_4 = new Vector64(arrowBasePoint.X - w.X * arrowHeadWidth, arrowBasePoint.Y - w.Y * arrowHeadWidth, arrowBasePoint.Z - w.Z * arrowHeadWidth);

        //    TriangleObj tri = new TriangleObj();
        //    // 0-3：箭头杆起点顶点
        //    tri.AddPoint(shaftP1_1); tri.AddPoint(shaftP1_2); tri.AddPoint(shaftP1_3); tri.AddPoint(shaftP1_4);
        //    // 4-7：箭头杆末端顶点
        //    tri.AddPoint(shaftP2_1); tri.AddPoint(shaftP2_2); tri.AddPoint(shaftP2_3); tri.AddPoint(shaftP2_4);
        //    // 添加所有顶点到列表（按顺序，索引从0开始）
        //    tri.AddPoint(arrowTip); // 8：箭头尖点    
        //    // 9-12：箭头头部侧点
        //    tri.AddPoint(arrowHead_1); tri.AddPoint(arrowHead_2); tri.AddPoint(shaftP2_3); tri.AddPoint(arrowHead_4);
        //    // ===================== 生成面（三角面/四边形面） =====================
        //    // 1. 箭头杆的4个矩形面（拆分为三角面）
        //    // 面1：shaftP1_1(0) → shaftP2_1(4) → shaftP2_2(5) → shaftP1_2(1)
        //    tri.AddTriangleIndex( 0,4,5);
        //    tri.AddTriangleIndex( 0, 5, 1 );
        //    // 面2：shaftP1_2(1) → shaftP2_2(5) → shaftP2_3(6) → shaftP1_3(2)
        //    tri.AddTriangleIndex(1, 5, 6 );
        //    tri.AddTriangleIndex(1, 6, 2 );
        //    // 面3：shaftP1_3(2) → shaftP2_3(6) → shaftP2_4(7) → shaftP1_4(3)
        //    tri.AddTriangleIndex(2, 6, 7 );
        //    tri.AddTriangleIndex( 2, 7, 3 );
        //    // 面4：shaftP1_4(3) → shaftP2_4(7) → shaftP2_1(4) → shaftP1_1(0)
        //    tri.AddTriangleIndex( 3, 7, 4 );
        //    tri.AddTriangleIndex(3, 4, 0 );
        //    // 2. 箭头头部的4个三角面（尖点指向各侧点）
        //    tri.AddTriangleIndex(8, 9, 10 );  // 尖点(8) → arrowHead_1(9) → arrowHead_2(10)
        //    tri.AddTriangleIndex(8, 10, 11 ); // 尖点(8) → arrowHead_2(10) → arrowHead_3(11)
        //    tri.AddTriangleIndex(8, 11, 12 ); // 尖点(8) → arrowHead_3(11) → arrowHead_4(12)
        //    tri.AddTriangleIndex(8, 12, 9 );  // 尖点(8) → arrowHead_4(12) → arrowHead_1(9)

        //    // 3. 箭头头部与杆连接的过渡面（4个三角面）
        //    tri.AddTriangleIndex(4, 9, 5 );   // shaftP2_1(4) → arrowHead_1(9) → shaftP2_2(5)
        //    tri.AddTriangleIndex(5, 10, 6 );  // shaftP2_2(5) → arrowHead_2(10) → shaftP2_3(6)
        //    tri.AddTriangleIndex(6, 11, 7 );  // shaftP2_3(6) → arrowHead_3(11) → shaftP2_4(7)
        //    tri.AddTriangleIndex(7, 12, 4 );  // shaftP2_4(7) → arrowHead_4(12) → shaftP2_1(4)
        //    tri.UpdateRange();
        //    return tri;
        //}

        /// <summary>
        /// 生成垂直于指定单位向量的固定向量
        /// </summary>
        private Vector64 GetPerpendicularVector(Vector64 u)
        {
            if (Math.Abs(u.X) < 0.9)
                return Vector64.Normalize(Vector64.Cross(u, new Vector64(1, 0, 0)));
            else if (Math.Abs(u.Y) < 0.9)
                return Vector64.Normalize(Vector64.Cross(u, new Vector64(0, 1, 0)));
            else
                return Vector64.Normalize(Vector64.Cross(u, new Vector64(0, 0, 1)));
        }
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

        /// <summary>
        /// 计算从P1指向P2的箭头三个顶点坐标（所有点共面）
        /// </summary>
        /// <param name="p1">起点</param>
        /// <param name="p2">终点（箭头尖点）</param>
        /// <param name="arrowLengthRatio">箭头长度占总长度的比例（默认1/8）</param>
        /// <param name="arrowWidthRatio">箭头宽度占箭头长度的比例（默认1/2）</param>
        /// <returns>箭头三个顶点：[0]尖点(P2)、[1]左侧点、[2]右侧点</returns>
        public Vector64[] CalculateArrowPoints(double arrowLengthRatio = 0.125,
                                               double arrowWidthRatio = 0.5)
        {
            Vector64 p1 = Start, p2 = End;
            // 异常处理：P1和P2重合时返回空
            if (p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z)
            {
                throw new ArgumentException("起点P1和终点P2不能重合！");
            }

            // 步骤1：计算P1到P2的方向向量和总长度
            Vector64 vecP1P2 = p2 - p1;
            double totalLength = Math.Sqrt(vecP1P2.X * vecP1P2.X + vecP1P2.Y * vecP1P2.Y + vecP1P2.Z * vecP1P2.Z);

            // 步骤2：计算箭头头部的长度（总长度 * 比例）
            double arrowLength = totalLength * arrowLengthRatio;
            if (arrowLength < 0.01) // 避免箭头过短
            {
                arrowLength = 0.01;
            }

            // 步骤3：归一化P1-P2方向向量（单位向量）
            Vector64 u = new Vector64(
                vecP1P2.X / totalLength,
                vecP1P2.Y / totalLength,
                vecP1P2.Z / totalLength
            );

            // 步骤4：生成唯一的垂直向量（确保所有点共面的核心）
            // 选择固定参考点（优先X轴，否则Y轴，最后Z轴），生成垂直于P1-P2的向量
            Vector64 refVec = GetPerpendicularVector(u);
            // 归一化垂直向量
            double refVecLen = Math.Sqrt(refVec.X * refVec.X + refVec.Y * refVec.Y + refVec.Z * refVec.Z);
            Vector64 v = new Vector64(
                refVec.X / refVecLen,
                refVec.Y / refVecLen,
                refVec.Z / refVecLen
            );

            // 步骤5：计算箭头头部的基准点（P2 往 P1 方向回退 arrowLength 距离）
            Vector64 basePoint = new Vector64(
                p2.X - u.X * arrowLength,
                p2.Y - u.Y * arrowLength,
                p2.Z - u.Z * arrowLength
            );

            // 步骤6：计算箭头的两个侧点（仅在v向量的正负方向偏移，确保共面）
            double arrowWidth = arrowLength * arrowWidthRatio;
            // 左侧点：基准点 + v向量 * 宽度
            Vector64 leftPoint = new Vector64(
                basePoint.X + v.X * arrowWidth,
                basePoint.Y + v.Y * arrowWidth,
                basePoint.Z + v.Z * arrowWidth
            );
            // 右侧点：基准点 - v向量 * 宽度（与左侧点对称，共面）
            Vector64 rightPoint = new Vector64(
                basePoint.X - v.X * arrowWidth,
                basePoint.Y - v.Y * arrowWidth,
                basePoint.Z - v.Z * arrowWidth
            );

            // 验证所有点是否共面（可选，用于调试）
            bool isCoplanar = CheckCoplanar(p1, p2, leftPoint, rightPoint);
            if (!isCoplanar)
            {
                throw new Exception("计算异常：箭头顶点未共面！");
            }

            // 返回箭头三个顶点：左侧点、尖点(P2),右侧点
            return new[] { leftPoint, p2, rightPoint };
        }

        /// <summary>
        /// 生成垂直于指定单位向量的固定向量（确保共面）
        /// </summary>
        private Vector64 GetPerpendicularVector(Vector64 u)
        {
            // 优先选择与X轴垂直的向量，若u接近X轴则选Y轴，否则选Z轴
            if (Math.Abs(u.X) < 0.9)
            {
                // 与X轴单位向量(1,0,0)叉乘，得到垂直于u的向量
                return Vector64.Cross(u, new Vector64(1, 0, 0));
            }
            else if (Math.Abs(u.Y) < 0.9)
            {
                // 与Y轴单位向量(0,1,0)叉乘
                return Vector64.Cross(u, new Vector64(0, 1, 0));
            }
            else
            {
                // 与Z轴单位向量(0,0,1)叉乘
                return Vector64.Cross(u, new Vector64(0, 0, 1));
            }
        }

        /// <summary>
        /// 验证四个点是否共面（核心：混合积为0）
        /// </summary>
        private bool CheckCoplanar(Vector64 p1, Vector64 p2, Vector64 p3, Vector64 p4)
        {
            // 构造三个向量：p1p2, p1p3, p1p4
            Vector64 vec1 = p2 - p1;
            Vector64 vec2 = p3 - p1;
            Vector64 vec3 = p4 - p1;

            // 混合积 = vec1 · (vec2 × vec3)，若混合积为0则共面
            Vector64 cross = Vector64.Cross(vec2, vec3);
            double dot = Vector64.Dot(vec1, cross);

            // 考虑浮点误差，允许极小的偏差
            return Math.Abs(dot) < 1e-6;
        }
        ///// <summary>
        ///// 计算从P1指向P2的箭头三个顶点坐标
        ///// </summary>
        ///// <param name="p1">起点</param>
        ///// <param name="p2">终点（箭头尖点）</param>
        ///// <param name="arrowLengthRatio">箭头长度占总长度的比例（默认1/8）</param>
        ///// <param name="arrowWidthRatio">箭头宽度占箭头长度的比例（默认1/2）</param>
        ///// <returns>箭头三个顶点：[0]尖点(P2)、[1]左侧点、[2]右侧点</returns>
        //public Vector64[] CalculateFlatArrowPoints(double arrowLengthRatio = 0.125,
        //                                                  double arrowWidthRatio = 0.5)
        //{
        //    Vector64 p1 = Start;
        //    Vector64 p2 = End;
        //    // 异常处理：P1和P2重合时返回空
        //    if (p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z)
        //    {
        //        return null;
        //    }

        //    // 步骤1：计算P1到P2的方向向量和总长度
        //    double dx = p2.X - p1.X;
        //    double dy = p2.Y - p1.Y;
        //    double dz = p2.Z - p1.Z;
        //    double totalLength = Math.Sqrt(dx * dx + dy * dy + dz * dz);

        //    // 步骤2：计算箭头头部的长度（总长度 * 比例）
        //    double arrowLength = totalLength * arrowLengthRatio;
        //    if (arrowLength < 0.01) // 避免箭头过短
        //    {
        //        arrowLength = 0.01;
        //    }

        //    // 步骤3：归一化方向向量（单位向量）
        //    double ux = dx / totalLength;
        //    double uy = dy / totalLength;
        //    double uz = dz / totalLength;

        //    // 步骤4：构造垂直于方向向量的两个正交向量（用于确定箭头两侧）
        //    // 先找一个不平行于方向向量的参考向量（优先X轴，否则Y轴）
        //    double rx = Math.Abs(ux) > 0.1 ? 0 : 1;
        //    double ry = Math.Abs(uy) > 0.1 ? 0 : 1;
        //    double rz = Math.Abs(uz) > 0.1 ? 0 : 1;

        //    // 第一个垂直向量：参考向量 × 方向向量
        //    double vx = ry * uz - rz * uy;
        //    double vy = rz * ux - rx * uz;
        //    double vz = rx * uy - ry * ux;
        //    // 归一化垂直向量
        //    double vLen = Math.Sqrt(vx * vx + vy * vy + vz * vz);
        //    vx /= vLen;
        //    vy /= vLen;
        //    vz /= vLen;

        //    // 第二个垂直向量：方向向量 × 第一个垂直向量（正交）
        //    double wx = uy * vz - uz * vy;
        //    double wy = uz * vx - ux * vz;
        //    double wz = ux * vy - uy * vx;

        //    // 步骤5：计算箭头头部的基准点（P2 往 P1 方向回退 arrowLength 距离）
        //    double baseX = p2.X - ux * arrowLength;
        //    double baseY = p2.Y - uy * arrowLength;
        //    double baseZ = p2.Z - uz * arrowLength;

        //    // 步骤6：计算箭头的两个侧点（基准点向两侧偏移）
        //    double arrowWidth = arrowLength * arrowWidthRatio;
        //    // 左侧点
        //    Vector64 leftPoint = new Vector64(
        //        baseX + vx * arrowWidth,
        //        baseY + vy * arrowWidth,
        //        baseZ + vz * arrowWidth  );
        //    // 右侧点
        //    Vector64 rightPoint = new Vector64(
        //        baseX + wx * arrowWidth,
        //        baseY + wy * arrowWidth,
        //        baseZ + wz * arrowWidth  );

        //    // 返回箭头三个顶点：尖点(P2)、左侧点、右侧点
        //    return new[] { leftPoint, p2, rightPoint };
        //}

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

