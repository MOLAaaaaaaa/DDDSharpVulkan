using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using GlmNet;
using MathNet.Numerics.Providers.LinearAlgebra;
// base class of all models
namespace DataCollection
{
    public enum ShapeEnum
    {
        Undefine = 0,
        Box = 1,
        Triangles = 2,
        Shphere = 3,
        Cylinder = 4,
        Line = 5,
        Polygon = 6,
        Points = 7,
        Text = 8,
        Grid3D = 10,
        Mesh = 11,
        Boreholes = 12,        
        Slicer = 13,    //grid slicer
        ISOSurface = 14,//derived from MC
        ISOSurfaceEX = 15,//derived from Improved MC
        LineMesh = 16,
        Polygon2D = 17,
        PolygonSlicer = 18,//slicer as polygon
        GeoProfile = 19,
        Map2D = 21,
        GeoLayerMeshes = 22,
        GeoMesh = 23,
        Polygon2Ds = 24,
        Shape = 50,

        Borehole = 51,
        BoreholeStratum = 52, //单个钻孔地层
        BoreholeStratums = 53,//整个钻孔地层
        BoreholeAngle = 54, //测斜数据
        BoreholeAngles = 55,//整个测斜数据
        BoreholeCurve = 56, //测井曲线
        BoreholeCurves = 57,//多个测井曲线
        LasData = 58,
    };

    public class FileChooseEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            // 编辑属性值时，在右侧显示...更多按钮              
            return UITypeEditorEditStyle.Modal;
            //return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            var edSvc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (edSvc != null)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Texture file (*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png|all files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return dlg.FileName;
                }                
                else return value;
            }
            return base.EditValue(context, provider, value);
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
        public override void PaintValue(PaintValueEventArgs e)
        {
            //Graphics g = e.Graphics;
            base.PaintValue(e);
        }
    }
    public enum RenderingUpdateMode
    {
        None = 0,//无操作
        Visible = 1,//开关显示
        Redraw = 2,//重绘
        Add = 3,//新增
        UpdateVertics = 4,//修改顶点数组
        UpdateColor = 5,//修改Alpha
        UpdateAlpha = 6,//修改Alpha
        UpdateWireTexture = 7,//修改Alpha
        UpdateWireFrameMode = 8,//修改Alpha
    }
    public struct RenderBufferStruct
    {
        public long GID;
        public bool Visible;
        public RenderingUpdateMode RenderMode;
        public RenderBufferStruct(long gid,bool visible, RenderingUpdateMode rendermode)
        {
            GID = gid;
            Visible = visible;
            RenderMode = rendermode;
        }
    }
    public class C3DObjectBase
    {
        public C3DObjectBase Parent = null;
        public float Version = 1.2f;
        public string errMessage = "";
        public ShapeEnum type = ShapeEnum.Undefine;
        public vec3 offset = new vec3(0,0,0);
        public vec3 rotate = new vec3(0, 0, 0);
        public vec3 scale = new vec3(1, 1, 1);
        public int itemKey = -1; //用于建立数据字典
        public virtual void DoOffset(double offx,double offy,double offz)
        {
            offset.x += (float)offx;
            offset.y += (float)offy;
            offset.z += (float)offz;            
        }       
        public virtual void DoRotate(double x, double y, double z)
        {
            rotate.x = (float)x;
            rotate.y = (float)y;
            rotate.z = (float)z;            
        }
        public virtual void DoScale(double x, double y, double z)
        {
            scale.x = (float)x;
            scale.y = (float)y;
            scale.z = (float)z;            
        }
        //added by jian 2021-1-2,对象绘制成绘图缓冲区key列表        
        public bool UpdateNeeded = false; //数据更改，是否需要更新显示？
        public RenderingUpdateMode RenderMode = RenderingUpdateMode.None; //是否需要重绘？
        public virtual void UpdateDrawMode(RenderingUpdateMode render = RenderingUpdateMode.Redraw)
        {
            RenderMode = render;
        }
        //需更新的数据列表
        public List<RenderBufferStruct> UpdateRenderBuffers = new List<RenderBufferStruct>();
        
        public List<long> RenderingBuffers = new List<long>();//绘图缓冲区列表

        /// <summary>
        /// 清除图像对象缓冲区ID，同时清除父对象中的对应数据
        /// </summary>        
        public virtual void ClearRenderingBuffers()
        {
            C3DObjectBase p = Parent;
            while( p!= null )
            {
                ClearRenderingBuffers(p,RenderingBuffers);
                p = p.Parent;
            }
            RenderingBuffers.Clear(); 
        }

        /// <summary>
        /// 从缓冲区对象中筛选并清除对应的数据（部分清除）
        /// </summary>
        /// <param name="p"></param>
        /// <param name="buffers">待清除的数据列表</param>
        public virtual void ClearRenderingBuffers(C3DObjectBase p, List<long> buffers)
        {
            if (p == null) return;

            long id, n;
            n = p.RenderingBuffers.Count;

            if (n < 1 || buffers.Count < 1) return;

            int[] removeindices = new int[n];

            for (int i = 0; i < n; i++)
            {
                id = p.RenderingBuffers[i];
                if ( buffers.Contains(id) ) removeindices[i] = 1;
            }

            List<long> lists = new List<long>();
            for (int i = 0; i < n; i++)
            {
                id = p.RenderingBuffers[i];
                if (removeindices[i] != 1) lists.Add(id);
            }
            p.RenderingBuffers.Clear();
            p.RenderingBuffers.AddRange(lists);
            lists.Clear();
            removeindices = null;
        }

        public virtual void AddRenderingBuffer(long key) 
        {
            C3DObjectBase p = Parent;
            while (p != null)
            {
                AddRenderingBuffer(p, key);
                p = p.Parent;
            }
            RenderingBuffers.Add(key); 
        }
        public virtual void AddRenderingBuffer(List<long> keys)
        {
            C3DObjectBase p = Parent;
            while (p != null)
            {
                AddRenderingBuffer(p, keys);
                p = p.Parent;
            }
            RenderingBuffers.AddRange(keys);
        }
        public virtual void AddRenderingBuffer(C3DObjectBase p,long key)
        {
            if (p != null) p.RenderingBuffers.Add(key);
            RenderingBuffers.Add(key);
        }
        public virtual void AddRenderingBuffer(C3DObjectBase p, List<long> keys)
        {
            if (p != null) p.RenderingBuffers.AddRange(keys);
            RenderingBuffers.AddRange(keys);
        }

        public virtual bool Remove( C3DObjectBase obj )
        {
            return true;
        }

       //properties of Display
       public string _Name = "untitled";
        [CategoryAttribute("Display"), DisplayNameAttribute("Name")]
        public virtual string Name { get { return _Name; } set { _Name = value; } }
                
        [CategoryAttribute("Display"), DisplayNameAttribute("Information"),Browsable(true)]
        public virtual string Information
        {
            get
            {
                string info = "X: " + Minx + " to " + Maxx + ";\n";
                info += "Y: " + Miny + " to " + Maxy + ";\n";
                info += "Z: " + Minz + " to " + Maxz + ";\n";
                info += "V: " + Minv + " to " + Maxv;
                return info;
            }
        }

        public bool _Visible = true;

        [CategoryAttribute("Display"), DisplayNameAttribute("Visible")]
        public virtual bool Visible
        { 
            get { return _Visible; } 
            set 
            {
                _Visible = value;
                UpdateNeeded = true;
                RenderMode = RenderingUpdateMode.Visible; 
            } 
        }

        [CategoryAttribute("Translation"), DisplayNameAttribute("Offset")]
        public virtual string OffsetString
        {
            get { return offset.x + "," + offset.y + "," + offset.z; }
            set 
            {
                Vector32 p;
                if( Vector32.TryParse(value,out p,3) )
                {
                    offset = new vec3(p.X,p.Y,p.Z);
                    UpdateNeeded = true;
                    RenderMode = RenderingUpdateMode.Redraw;
                }                
            }
        }
        [CategoryAttribute("Translation"), DisplayNameAttribute("Rotation")]
        public virtual string RotateString
        {
            get { return rotate.x + "," + rotate.y + "," + rotate.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    rotate = new vec3(p.X, p.Y, p.Z);
                    UpdateNeeded = true;
                    RenderMode = RenderingUpdateMode.Redraw;
                }
            }
        }

        [CategoryAttribute("Translation"), DisplayNameAttribute("Scale")]
        public virtual string ScaleString
        {
            get { return scale.x + "," + scale.y + "," + scale.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    scale = new vec3(p.X, p.Y, p.Z);
                    UpdateNeeded = true;
                    RenderMode = RenderingUpdateMode.Redraw;
                }
            }
        }

        //blending
        public bool _Blend = true;
        [CategoryAttribute("Blend"), DisplayNameAttribute("Blend")]
        public virtual bool Blend
        { 
            get { return _Blend; } 
            set { _Blend = value; RenderMode = RenderingUpdateMode.Redraw; } 
        }

        public float _Alpha = 1;
        [CategoryAttribute("Blend"), DisplayNameAttribute("Alpha (0 ~ 1)")]
        public virtual float Alpha 
        { 
            get { return _Alpha; } 
            set { _Alpha = value; RenderMode = RenderingUpdateMode.Redraw; } 
        }

        public virtual float GetOrderedAlpha()
        {
            return Alpha;
        }

        public bool _IsWireFrameMode = false;
        [CategoryAttribute("Display"), DisplayNameAttribute("WireframeMode")]
        public virtual bool IsWireFrameMode 
        { 
            get { return _IsWireFrameMode; } 
            set { _IsWireFrameMode = value; RenderMode = RenderingUpdateMode.Redraw; } 
        }
       
        [CategoryAttribute("Texture"), DisplayNameAttribute("Enable")]
        public virtual bool enbaleTexture 
        { 
            get { return _textStruct.Enable; } 
            set { _textStruct.Enable = value; RenderMode = RenderingUpdateMode.Redraw; } 
        }

        public TextureStruct _textStruct = new TextureStruct();
        [CategoryAttribute("Texture"), DisplayNameAttribute("Texture Style")]
        [Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(PropertyStyleConverter))]
        public virtual TextureStruct textureStruct
        {
            get { return _textStruct; }
            set { _textStruct = value; if (_textStruct.Enable) RenderMode = RenderingUpdateMode.Redraw; }
        }         

        public double minx = 0.0;
        public double miny = 0.0;
        public double minz = 0.0;
        public double minv = 0.0;
        public double maxx = 0.0;
        public double maxy = 0.0;
        public double maxz = 0.0;
        public double maxv = 0.0;
       
        [CategoryAttribute("Geometries"), DisplayNameAttribute("X Minimum"),Browsable(false)]
        public virtual double Minx { get { return minx; } set { minx = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("X Maximum"), Browsable(false)]
        public virtual double Maxx { get { return maxx; } set { maxx = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("X Length"), Browsable(false)]
        public virtual double XWidth{ get { return Maxx - Minx; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Y Minimum"), Browsable(false)]
        public virtual double Miny { get { return miny; } set { miny = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Y Maximum"), Browsable(false)]
        public virtual double Maxy { get { return maxy; } set { maxy = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Y Length"), Browsable(false)]
        public virtual double YWidth { get { return Maxy - Miny; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Z Minimum"), Browsable(false)]
        public virtual double Minz { get { return minz; } set { minz = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Z Maximum"), Browsable(false)]
        public virtual double Maxz { get { return maxz; } set { maxz = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Z Length"), Browsable(false)]
        public virtual double ZWidth { get { return Maxz - Minz; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("V Minimum"), Browsable(false)]
        public virtual double Minv { get { return minv; } set { minv = value; } }
        [CategoryAttribute("Geometries"), DisplayNameAttribute("V Maximum"), Browsable(false)]
        public virtual double Maxv { get { return maxv; } set { maxv = value; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("V Length"), Browsable(false)]
        public virtual double VWidth { get { return Maxv - Minv; } }

        [CategoryAttribute("Geometries"), DisplayNameAttribute("Minimum Width"), Browsable(false)]
        public virtual double MinWidth
        {
            get 
            {
                double min = XWidth;
                if (YWidth < min) min = YWidth;
                if (ZWidth < min) min = ZWidth;
                return min;
            }
        }
        [CategoryAttribute("Geometries"), DisplayNameAttribute("Maximum Width"), Browsable(false)]
        public virtual double MaxWidth
        {
            get
            {
                double max = XWidth;
                if (YWidth > max) max = YWidth;
                if (ZWidth > max) max = ZWidth;
                return max;
            }
        }
        public double _Longitude1 = 0, _Longitude2 = 0;
        public double _Latitude1 = 0, _Latitude2 = 0;
        public double _Elevation1 = 0, _Elevation2 = 0;
        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Longitude1")]
        public virtual double Longitude1 
        { 
            get { return _Longitude1; } 
            set { _Longitude1 = value;
                  if (IsEarthMapped) RenderMode = RenderingUpdateMode.Redraw; 
                } 
        }

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Longitude2")]
        public virtual double Longitude2
        {
            get { return _Longitude2; }
            set
            {
                _Longitude2 = value;
                if (IsEarthMapped) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Latitude1")]
        public virtual double Latitude1
        {
            get { return _Latitude1; }
            set
            {
                _Latitude1 = value;
                if (IsEarthMapped) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Latitude2")]
        public virtual double Latitude2
        {
            get { return _Latitude2; }
            set
            {
                _Latitude2 = value;
                if (IsEarthMapped) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Elevation1")]        
        public virtual double Elevation1
        {
            get { return _Elevation1; }
            set
            {
                _Elevation1 = value;
                if (IsEarthMapped) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("Elevation2")]
        public virtual double Elevation2
        {
            get { return _Elevation2; }
            set
            {
                _Elevation2 = value;
                if (IsEarthMapped) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        [CategoryAttribute("Earth Coordinates"), DisplayNameAttribute("IsEarthMapped"),Browsable(false)]
        public virtual bool IsEarthMapped
        {
            get
            {
                if ( Latitude1 != Latitude2 && 
                     Longitude1 != Longitude2 && CDataModel.IsEarthMapVision )
                    return true;
                else return false;
            }
        }
       
        public virtual void SetColorRange()
        {            
        }
        public virtual void SetColorRange(double v1, double v2)
        {           
        }
        public virtual EarthVector toEarthCoordinate(Vector64 p)
        {
            if (Longitude1 == Longitude2 || Latitude1 == Latitude2) return new EarthVector(0, 0, p.z, p.v);
            double longitude = Longitude1;
            double latitude = Latitude1;
            double elevation = Elevation1;
            if (Longitude2 != Longitude1 && Maxx != Minx)
                longitude = Longitude1 + (p.x - Minx) / (Maxx - Minx) * (Longitude2 - Longitude1);
            if (Latitude2 != Latitude1 && Maxy != Miny)
                latitude = Latitude1 + (p.y - Miny) / (Maxy - Miny) * (Latitude2 - Latitude1);
            if (Elevation2 != Elevation1 && Maxz != Minz)
                elevation = Elevation1 + (p.z - Minz) / (Maxz - Minz) * (Elevation2 - Elevation1);
            return new EarthVector(longitude, latitude, elevation, p.v);
        }
       
        /// <summary>
        /// 球面系统坐标中根据配准的经纬度来生成纹理坐标
        /// </summary>
        /// <param name="p"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void GetEarthTextureCoord(Vector32 p, out float x, out float y)
        {
            x = y = 0;
            if (maxx != minx) x = (float)((p.x - minx) / (maxx - minx));
            if (maxy != miny) y = (float)((p.y - miny) / (maxy - miny));
        }
        public virtual vec2 GetTextureCoord(Vector32 p)
        {
            vec2 tex = new vec2();
            GetTextureCoord(p,out tex.x,out tex.y);
            return tex;
        }
        public virtual void GetTextureCoord(Vector32 p, out float x, out float y)
        {
            if (IsEarthMapped) { GetEarthTextureCoord(p, out x, out y); return; }
            x = y = -1;
            int ret1 = 0;
            int ret2 = 1;
            double xx = maxx - minx;
            double yy = maxy - miny;
            double zz = maxz - minz;
            if (xx >= yy && xx >= zz)
            {
                ret1 = 0;
                ret2 = 1;
                if (zz >= yy) ret2 = 2;
            }
            else if (yy >= xx && yy >= zz)
            {
                ret1 = 1;
                ret2 = 0;
                if (zz >= xx) ret2 = 2;
            }
            else if (zz >= xx && zz >= yy)
            {
                ret1 = 2;
                ret2 = 0;
                if (yy >= xx) ret2 = 1;
            }

            if (ret1 == 0)
            {
                x = (float)((p.x - minx) / xx);
                if (ret2 == 1) //x,y
                    y = (float)((p.y - miny) / yy);
                else            //x,z
                    y = (float)((p.z - minz) / zz);
            }
            else if (ret1 == 1)
            {
                x = (float)((p.y - miny) / yy);
                if (ret2 == 0) //y,x
                    y = (float)((p.x - minx) / xx);
                else            //x,z
                    y = (float)((p.z - minz) / zz);
            }
            else if (ret1 == 2)
            {
                x = (float)((p.z - minz) / zz);
                if (ret2 == 0) //y,x
                    y = (float)((p.x - minx) / xx);
                else            //x,z
                    y = (float)((p.y - miny) / yy);
            }
        }

        public virtual void Clear() { }
        public virtual void Draw() { }
        public virtual void UpdateRange() { }
        public virtual float[] toValuesArray() { return null; }

        //convert points according to tranform,scale,offset
        public virtual bool IsInRange(double x,double y,double z)
        {
            if (x < minx || x > maxx) return false;
            if (y < miny || y > maxy) return false;
            if (z < minz || z > maxz) return false;
            return true;
        }
        public virtual void Normalize() { }
        public virtual void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2) { }
        //copy transform parameters from other object
        public virtual void CopyTransformFrom( C3DObjectBase obj) 
        {
            offset = obj.offset;
            scale = obj.scale;
            rotate = obj.rotate;
            minx = obj.minx;
            miny = obj.miny;
            minz = obj.minz;
            minv = obj.minv;
            maxx = obj.maxx;
            maxy = obj.maxy;
            maxz = obj.maxz;
            maxv = obj.maxv;
        }
        
        public void CopyHeaderFrom(C3DObjectBase obj1)
        {
            Name = obj1.Name;
            //  obj1.type = type;
            //  obj1.visible = visible;
            //  obj1.IsWireFrameMode = IsWireFrameMode;
            offset = obj1.offset;
            rotate = obj1.rotate;
            scale = obj1.scale;
            Blend = obj1.Blend;
            Alpha = obj1.Alpha;
            textureStruct = obj1.textureStruct.Copy();
            minx = obj1.minx;
            miny = obj1.miny;
            minz = obj1.minz;
            minv = obj1.minv;
            maxx = obj1.maxx;
            maxy = obj1.maxy;
            maxz = obj1.maxz;
            maxv = obj1.maxv;
            Longitude1 = obj1.Longitude1;
            Longitude2 = obj1.Longitude2;
            Latitude1 = obj1.Latitude1;
            Latitude2 = obj1.Latitude2;
            Elevation1 = obj1.Elevation1;
            Elevation2 = obj1.Elevation2;
        }
        public virtual bool SaveAs(string path) { return true; }
        public virtual bool LoadFrom(string path) { return true; }
        public virtual bool SaveAs(BinaryWriter br) { return true; }
        public virtual bool LoadFrom(BinaryReader br) { return true; }

        public virtual bool ImportData(string path) { return true; }
        public virtual bool ExportData(string path) { return true; }

        public virtual string GetLineValue(string str, string name)
        {
            string[] ss = str.Split(new Char[] { '=', '=' }, 2);
            if (ss.Length < 2) return "";

            string s1 = ss[0].Trim(' ');
            if (s1.ToLower() != name.ToLower()) return "";
            return ss[1].Trim(' ');
        }

        public virtual  bool ExportTranslations(StreamWriter wr)
        {
            try
            {
                string str = "[TRANSLATIONS]";
                wr.WriteLine(str);
                str = "Minimum = " + minx + "," + miny + "," + minz + "," + minv;
                wr.WriteLine(str);
                str = "Maximum = " + maxx + "," + maxy + "," + maxz + "," + maxv;
                wr.WriteLine(str);
                str = "Scale = " + scale.x + "," + scale.y + ","+ scale.z;
                wr.WriteLine(str);
                str = "Offset = " + offset.x + "," + offset.y + "," + offset.z;
                wr.WriteLine(str);
                str = "Rotation = " + rotate.x + "," + rotate.y + "," + rotate.z;
                wr.WriteLine(str);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        public virtual bool ImportTranslations(StreamReader sr)
        {
            try
            {
                Vector64 p = Vector64.Parse(GetLineValue(sr.ReadLine(),"Minimum"), 4);
                minx = p.x; miny = p.y; minz = p.z; minv = p.v;
                
                p = Vector64.Parse(GetLineValue(sr.ReadLine(), "Maximum"), 4);
                maxx = p.x; maxy = p.y; maxz = p.z; maxv = p.v;
                
                Vector32 v = Vector32.Parse(GetLineValue(sr.ReadLine(), "Scale"), 3);
                scale = new vec3(v.x, v.y, v.z);

                v = Vector32.Parse(GetLineValue(sr.ReadLine(), "Offset"), 3);
                offset = new vec3(v.x, v.y, v.z);

                v = Vector32.Parse(GetLineValue(sr.ReadLine(), "Rotation"), 3);
                rotate = new vec3(v.x, v.y, v.z);                
                return true;
            }
            catch (Exception e)
            {                
                return false;
            }
        }
        public string LoadString(BinaryReader br)
        {
            try 
            {
                string ss = "";
                Int16 n = br.ReadInt16();
                if (n > 0) ss = new string(br.ReadChars(n));
                return ss;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return null;
            }            
        }
        public bool SaveString(BinaryWriter br, string ss)
        {
            try
            {
                Int16 n = (Int16)ss.Length;
                br.Write(n);
                if (n > 0) br.Write(ss.ToCharArray());
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }            
        }
        public virtual bool LoadObjHeader(BinaryReader br)
        {
            if (C3DData.DataVersion <= 1.2f) return LoadObjHeader120(br);
            else return LoadObjHeader121(br);            
        }

        //version 1.21
        public bool LoadObjHeader121(BinaryReader br)
        {
            type = (ShapeEnum)br.ReadInt32();
            int n = br.ReadInt32();
            char[] header = new char[n];
            header = br.ReadChars(n);
            Name = new string(header);

            Visible = br.ReadBoolean();
            Blend = br.ReadBoolean();
            Alpha = br.ReadSingle();
            IsWireFrameMode = br.ReadBoolean();

            minx = br.ReadDouble();
            maxx = br.ReadDouble();
            miny = br.ReadDouble();
            maxy = br.ReadDouble();
            minz = br.ReadDouble();
            maxz = br.ReadDouble();
            minv = br.ReadDouble();
            maxv = br.ReadDouble();

            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            offset = new vec3(x, y, z);

            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            scale = new vec3(x, y, z);

            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            rotate = new vec3(x, y, z);

            //added new
            Longitude1 = br.ReadDouble();
            Longitude2 = br.ReadDouble();
            Latitude1 = br.ReadDouble();
            Latitude2 = br.ReadDouble();
            Elevation1 = br.ReadDouble();
            Elevation2 = br.ReadDouble();
            Version = br.ReadSingle(); //added 2022-3-9
                                       //added new 2022-8
            if (Version > 1.0f)
            {
                textureStruct = new TextureStruct();
                textureStruct.Load(br);                    
            }
            return true;
        }
        //version 1.20
        public bool LoadObjHeader120(BinaryReader br)
        {
            type = (ShapeEnum)br.ReadInt32();
            int n = br.ReadInt32();
            char[] header = new char[n];
            header = br.ReadChars(n);
            Name = new string(header);

            Visible = br.ReadBoolean();
            Blend = br.ReadBoolean();
            Alpha = br.ReadSingle();
            IsWireFrameMode = br.ReadBoolean();

            minx = br.ReadDouble();
            maxx = br.ReadDouble();
            miny = br.ReadDouble();
            maxy = br.ReadDouble();
            minz = br.ReadDouble();
            maxz = br.ReadDouble();
            minv = br.ReadDouble();
            maxv = br.ReadDouble();

            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            offset = new vec3(x, y, z);

            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            scale = new vec3(x, y, z);

            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            rotate = new vec3(x, y, z);

            //added new
            Longitude1 = br.ReadDouble();
            Longitude2 = br.ReadDouble();
            Latitude1 = br.ReadDouble();
            Latitude2 = br.ReadDouble();
            Elevation1 = br.ReadDouble();
            Elevation2 = br.ReadDouble();
            Version = br.ReadSingle(); //added 2022-3-9
            //added new
            if (Version > 1.0f)
            {
                textureStruct = new TextureStruct();
                textureStruct.Load(br,Version);
            }
            return true;
        }
        /// <summary>
        /// Save Header of this object to Binary File
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public virtual bool SaveObjHeader(BinaryWriter br)
        {
            return SaveObjHeader120(br);
        }
        public bool SaveObjHeader120(BinaryWriter br)
        {
            br.Write((int)type);
            br.Write((Int32)Name.Length);
            br.Write(Name.ToCharArray());

            br.Write(Visible);
            br.Write(Blend);
            br.Write(Alpha);
            br.Write(IsWireFrameMode);

            br.Write(minx);
            br.Write(maxx);
            br.Write(miny);
            br.Write(maxy);
            br.Write(minz);
            br.Write(maxz);
            br.Write(minv);
            br.Write(maxv);
            br.Write(offset.x);
            br.Write(offset.y);
            br.Write(offset.z);
            br.Write(scale.x);
            br.Write(scale.y);
            br.Write(scale.z);
            br.Write(rotate.x);
            br.Write(rotate.y);
            br.Write(rotate.z);

            br.Write(Longitude1);
            br.Write(Longitude2);
            br.Write(Latitude1);
            br.Write(Latitude2);
            br.Write(Elevation1);
            br.Write(Elevation2);
            br.Write(Version); //added 2022-3-9
            if( Version >= 1.2f )
            {
                textureStruct.Save(br);
            }
            return true;
        }       
        public virtual bool ExportVRML(StreamWriter wr) { return true; }       
        public virtual Vector64 GetCenter64()
        {
            double x = (Minx + Maxx) / 2;
            double y = (Miny + Maxy) / 2;
            double z = (Minz + Maxz) / 2;
            return new Vector64(x, y, z);
        }
        public virtual Vector32 GetCenter32()
        {
            float x = (float)(Minx + Maxx) / 2;
            float y = (float)(Miny + Maxy) / 2;
            float z = (float)(Minz + Maxz) / 2;

            return new Vector32(x, y, z);
        }
        public virtual Vector32 GetOffset32()
        {
            //Vector32 p0 = GetCenter32();
            //return new Vector32(p0.X + offset.x, p0.Y + offset.y, p0.Z + offset.z);
            return new Vector32(offset.x, offset.y, offset.z);
        }
        public virtual Vector64 GetOffset64()
        {
            Vector64 p0 = GetCenter64();
            return new Vector64(p0.X + offset.x, p0.Y + offset.y, p0.Z + offset.z);
        }
        public virtual vec3 toPoint(Vector32 p)
        {
            return new vec3(p.x, p.y, p.z);
        }
        public virtual Vector32 toPoint(vec3 p)
        {
            return new Vector32(p.x, p.y, p.z);
        }
        public virtual void CutWith(C3DObjectBase obj,int method,int methodPara)
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
        }
        public virtual Vector32 TransformedPoint(Vector32 p)
        {
            Vector32 p0 = GetCenter32();
            Vector32 p1 = p - p0;

            if (scale.x != 1.0) p1.X = p1.X * scale.x;
            if (scale.y != 1.0) p1.Y = p1.Y * scale.y;
            if (scale.z != 1.0) p1.Z = p1.Z * scale.z;

            if (rotate.x != 0.0) p1.RotateOnAngle(rotate.x, 0);
            if (rotate.y != 0.0) p1.RotateOnAngle(rotate.y, 1);
            if (rotate.z != 0.0) p1.RotateOnAngle(rotate.z, 2);

            p1 = p1 + p0;

            p1.x += offset.x;
            p1.y += offset.y;
            p1.z += offset.z;

            return p1;
        }
        public virtual Vector32 UnTransformedPoint(Vector32 p)
        {
            Vector32 p0 = GetCenter32();

            Vector32 p1 = new Vector32(p.x,p.y,p.z);

            p1.z -= (p0.z + offset.z);
            p1.y -= (p0.y + offset.y);
            p1.x -= (p0.x + offset.x);           

            if (rotate.z != 0.0) p1.RotateOnAngle(-rotate.z, 2);
            if (rotate.y != 0.0) p1.RotateOnAngle(-rotate.y, 1);
            if (rotate.x != 0.0) p1.RotateOnAngle(-rotate.x, 0);

            if (scale.z != 1.0 && scale.z != 0.0) p1.Z = p1.Z / scale.z;
            if (scale.y != 1.0 && scale.y != 0.0) p1.Y = p1.Y / scale.y;
            if (scale.x != 1.0 && scale.x != 0.0) p1.X = p1.X / scale.x;
            
            p1 = p1 + p0;

            return p1;
        }
        public virtual Vector64 TransformedPoint(Vector64 p)
        {
            Vector64 p0 = GetCenter64();
            Vector64 p1 = p - p0;

            if (scale.x != 1.0) p1.X = p1.X * scale.x;
            if (scale.y != 1.0) p1.Y = p1.Y * scale.y;
            if (scale.z != 1.0) p1.Z = p1.Z * scale.z;

            if (rotate.x != 0.0) p1.RotateOnAngle(rotate.x, 0);
            if (rotate.y != 0.0) p1.RotateOnAngle(rotate.y, 1);
            if (rotate.z != 0.0) p1.RotateOnAngle(rotate.z, 2);

            p1 = p1 + p0;

            p1.x += offset.x;
            p1.y += offset.y;
            p1.z += offset.z;

            return p1;
        }
        public virtual Vector64 UnTransformedPoint(Vector64 p)
        {
            Vector64 p0 = GetCenter64();
            Vector64 p1 = new Vector64(p.x, p.y, p.z);

            p1.z -= offset.z;
            p1.y -= offset.y;
            p1.x -= offset.x;
            p1 = p1 - p0;

            if (rotate.z != 0.0) p1.RotateOnAngle(-rotate.z, 2);
            if (rotate.y != 0.0) p1.RotateOnAngle(-rotate.y, 1);
            if (rotate.x != 0.0) p1.RotateOnAngle(-rotate.x, 0);

            if (scale.z != 1.0 && scale.z != 0.0) p1.Z = p1.Z / scale.z;
            if (scale.y != 1.0 && scale.y != 0.0) p1.Y = p1.Y / scale.y;
            if (scale.x != 1.0 && scale.x != 0.0) p1.X = p1.X / scale.x;

            p1 = p1 + p0;

            return p1;
        }

        //模型坐标转换到世界坐标
        public virtual Vector64 toWorldVector(Vector64 p)
        {
            if (IsEarthMapped) return toEarthCoordinate(p).toXYZVector();
            else return p;
        }
        public virtual Vector64 toWorldVector(Vector32 p)
        {
            return toWorldVector(p.toVector64());
        }       
        /// <summary>
        /// Load 3DObject transform parameters
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public virtual bool ReadG3DHeader(BinaryReader br)
        {
            try
            {
                string tag = C3DData.LoadString(br);
                if (tag != "3DOBJ") 
                {
                    return false; 
                }
                
                int version = br.ReadInt32();
                if (version != 0x00FF12)
                { 
                    return false; 
                }
            }
            catch (IOException e)
            {              
                return false;
            }
            return true;
        }

        public bool WriteG3DHeader(BinaryWriter br)
        {
            try
            {
                string header = "3DOBJ"; //3D object mark
                C3DData.SaveString(br,header);
                int version = 0x00FF12;
                br.Write(version);
            }
            catch (IOException e)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 判断值是否是白化值，值是否有效，是否在范围之内
        /// </summary>
        /// <param name="val">输入值</param>
        /// <param name="err">允许容差%</param>
        /// <returns></returns>
        public virtual bool IsBlanked(double val, double err = 1.0E-5)
        {
            if ( IsBlankValue(val) ) return true;
            if ( maxv > minv )
            {
                double len = maxv - minv;
                if (val < minv && Math.Abs(val - minv) / len >= err) return true;
                else if (val > maxv && Math.Abs(val - maxv) / len >= err) return true;
                else return false;
            }
            return false;
        }

        /// <summary>
        /// 判断值val是否是有效值
        /// </summary>
        /// <param name="val"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public virtual bool IsBlankValue(double val, double err = 1.0E-3)
        {
            return C3DData.IsBlankValue(val, err);
        }

        public C3DObjectBase()
        {
           
        }
        public virtual bool CalculateNormals(Vector32[] normals, List<Vector32> points, List<Int32XYZ> indices)
        {
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new Vector32(0, 0, 0);
            }
            Vector32 pn;
            int i1, i2, i3;
            for (int i = 0; i < indices.Count; i++)
            {
                i1 = indices[i].x;
                i2 = indices[i].y;
                i3 = indices[i].z;
                pn = Vector32.GetNormal(points[i1], points[i2], points[i3]);
                normals[i1] += pn;
                normals[i2] += pn;
                normals[i3] += pn;
            }
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].Normalize();
            }
            return true;
        }
        public virtual bool CalculateNormals(Vector64[] normals, List<Vector64> points, List<Int32XYZ> indices)
        {
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new Vector64(0, 0, 0);
            }
            Vector64 pn;
            int i1, i2, i3;
            for (int i = 0; i < indices.Count; i++)
            {
                i1 = indices[i].x;
                i2 = indices[i].y;
                i3 = indices[i].z;
                pn = Vector64.GetNormal(points[i1], points[i2], points[i3]);
                normals[i1] += pn;
                normals[i2] += pn;
                normals[i3] += pn;
            }
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].Normalize();
            }
            return true;
        }
        public virtual bool CalculateNormals(Vector32[]normals,List<Vector32>points,List<int>indices)
        {
            for (int i = 0; i < normals.Length; i++) 
            { 
                normals[i] = new Vector32(0,0,0); 
            }
            Vector32 pn;
            int i1, i2, i3;
            for(int i=0;i<indices.Count/3;i++)
            {
                i1 = indices[3 * i];
                i2 = indices[3 * i+1];
                i3 = indices[3 * i+2];
                pn = Vector32.GetNormal(points[i1], points[i2], points[i3]);
                normals[i1] += pn;
                normals[i2] += pn;
                normals[i3] += pn;
            }
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].Normalize();
            }
            return true;
        }
        public virtual bool CalculateNormals(Vector64[] normals, List<Vector64> points, List<int> indices)
        {
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new Vector64(0, 0, 0);
            }
            Vector64 pn;
            int i1, i2, i3;
            for (int i = 0; i < indices.Count / 3; i++)
            {
                i1 = indices[3 * i];
                i2 = indices[3 * i + 1];
                i3 = indices[3 * i + 2];
                pn = Vector64.GetNormal(points[i1], points[i2], points[i3]);
                normals[i1] += pn;
                normals[i2] += pn;
                normals[i3] += pn;
            }
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i].Normalize();
            }
            return true;
        }
    }//C3DObjectBase

    /// <summary>
    /// 3D Object Base 隐藏属性
    /// </summary>
    public class C3DObjectBaseHide : C3DObjectBase
    {
        [CategoryAttribute("Display"), DisplayNameAttribute("Name"), Browsable(false)]
        public override string Name { get; set; } = "Untitled";
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible"), Browsable(false)]
        public override bool Visible { get; set; } = true;
        [CategoryAttribute("Display"), DisplayNameAttribute("Overview"), Browsable(false)]
        public override string Information { get; } = "";

        [CategoryAttribute("Display"), DisplayNameAttribute("Offset"), Browsable(false)]
        public override string OffsetString { get; set; } = "0,0,0";
        [CategoryAttribute("Display"), DisplayNameAttribute("Rotate"), Browsable(false)]
        public override string RotateString { get; set; } = "0,0,0";
        [CategoryAttribute("Display"), DisplayNameAttribute("Scale"), Browsable(false)]
        public override string ScaleString { get; set; } = "1,1,1";
        [CategoryAttribute("Display"), DisplayNameAttribute("Blend"), Browsable(false)]
        public override bool Blend { get; set; } = true;
        [CategoryAttribute("Display"), DisplayNameAttribute("Alpha"), Browsable(false)]
        public override float Alpha { get; set; } = 1.0f;

        [CategoryAttribute("Display"), DisplayNameAttribute("IsWireFrameMode"), Browsable(false)]
        public override bool IsWireFrameMode { get; set; } = false;
        [CategoryAttribute("Display"), DisplayNameAttribute("enbaleTexture"), Browsable(false)]
        public override bool enbaleTexture { get; set; } = false;
        [CategoryAttribute("Display"), DisplayNameAttribute("textureStruct"), Browsable(false)]
        public override TextureStruct textureStruct
        {
            get { return _textStruct; }
            set { _textStruct = value; }
        }

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
    }
}
