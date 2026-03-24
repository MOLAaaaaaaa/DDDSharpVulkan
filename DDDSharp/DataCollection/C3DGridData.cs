using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using GlmNet;
using System.Drawing.Design;
using Graphics3D;
using CLInterpolation;
using System.Xml.Schema;
using System.Runtime.CompilerServices;
using glfw3;
using WinFormAnimation;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DataCollection
{
    //point position to a cube
    public enum CubePosition
    {
        OutSideCube = -1,
        InSideCube = 0,
        OnXFace = 1,
        OnYFace = 2,
        OnZFace = 3,
        OnEdge = 4,
        OnNode = 5
    };
    public class C2DGridData : C3DObjectBase
    {
        public int xNum;
        public int yNum;
        float[] pGridData;
        private long xyNum;
        private double xStep, yStep;
        
        public C2DGridData()
        {
            xNum = yNum = 0;
            pGridData = null;
        }
        public C2DGridData(CSurferGrid cs)
        {
            xNum = cs.xGrid;
            yNum = cs.yGrid;

            minx = cs.minx;
            maxx = cs.maxx;
            miny = cs.miny;
            maxy = cs.maxy;
            minv = minz = cs.minv;
            maxv = maxz = cs.maxv;

            xStep = (maxx - minx) / (xNum - 1);
            yStep = (maxy - miny) / (yNum - 1);

            pGridData = new float[cs.pData.Length];

            for (int i = 0; i < pGridData.Length; i++)
                pGridData[i] = cs.pData[i];
        }
        public C2DGridData(C3DGridData data, int ix, int iy, int iz)
        {
            int id;
            if (ix >= 0)
            {
                xNum = data.yNum;
                yNum = data.zNum;
                minx = data.miny;
                maxx = data.maxy;
                miny = data.minz;
                maxy = data.maxz;
                xStep = (maxx - minx) / (xNum - 1);
                yStep = (maxy - miny) / (yNum - 1);
                pGridData = new float[xNum * yNum];
                for (int i = 0; i < yNum; i++)
                {
                    for (int j = 0; j < xNum; j++)
                    {
                        id = i * data.xNum * data.yNum + j * data.xNum + ix;
                        pGridData[i * xNum + j] = data[id];
                    }
                }
            }
            if (iy >= 0)
            {
                xNum = data.xNum;
                yNum = data.zNum;
                minx = data.minx;
                maxx = data.maxx;
                miny = data.minz;
                maxy = data.maxz;
                xStep = (maxx - minx) / (xNum - 1);
                yStep = (maxy - miny) / (yNum - 1);
                pGridData = new float[xNum * yNum];
                for (int i = 0; i < yNum; i++)
                {
                    for (int j = 0; j < xNum; j++)
                    {
                        id = i * data.xNum * data.yNum + iy * data.xNum + j;
                        pGridData[i * xNum + j] = data[id];
                    }
                }
            }
            if (iz >= 0)
            {
                xNum = data.xNum;
                yNum = data.yNum;
                minx = data.minx;
                maxx = data.maxx;
                miny = data.miny;
                maxy = data.maxy;
                xStep = (maxx - minx) / (xNum - 1);
                yStep = (maxy - miny) / (yNum - 1);
                pGridData = new float[xNum * yNum];
                for (int i = 0; i < yNum; i++)
                {
                    for (int j = 0; j < xNum; j++)
                    {
                        id = iz * data.xNum * data.yNum + i * data.xNum + j;
                        pGridData[i * xNum + j] = data[id];
                    }
                }
            }
            UpdateRange();
        }
        public void SetData(float[] grid, int nx, int ny, double _minx, double _maxx, double _miny, double _maxy, double _minv, double _maxv)
        {
            xNum = nx;
            yNum = ny;
            minx = _minx;
            miny = _miny;
            maxx = _maxx;
            maxy = _maxy;
            minz = minv = _minv;
            maxz = maxv = _maxv;
            xStep = (maxx - minx) / (nx - 1);
            yStep = (maxy - miny) / (ny - 1);
            xyNum = xNum * yNum;
            pGridData = grid;
        }
        public override void UpdateRange()
        {
            if (pGridData == null) return;
            long k = 0;
            for (int i = 0; i < pGridData.Length; i++)
            {
                if ( IsBlankValue( pGridData[i] ) ) continue;

                if (k == 0) minv = maxv = pGridData[i];
                else
                {
                    if (pGridData[i] < minv) minv = pGridData[i];
                    if (pGridData[i] > maxv) maxv = pGridData[i];
                }
                k++;
            }
            minz = minv;
            maxz = maxv;
        }
        public double GetGridValue(int ix, int iy)
        {
            int id = ix + iy * xNum;
            return pGridData[id];
        }
        public int GetVerticIndex(int ix, int iy)
        {
            if (ix < 0 || ix >= xNum) return -1;
            if (iy < 0 || iy >= yNum) return -1;
            return ix + iy * xNum;
        }

        //通过插值得到网格任意位置的值
        public double GetGridValue(double x, double y)
        {
            int ix = (int)((x - minx + 0.01 * xStep) / xStep);
            int iy = (int)((y - miny + 0.01 * yStep) / yStep);

            //out of the range of this meshes
            if (ix < 0) ix = 0;
            if (iy < 0) iy = 0;
            if (ix > xNum - 1) ix = xNum - 1;
            if (iy > yNum - 1) iy = yNum - 1;
                        
            double x1 = minx + ix * xStep;
            double y1 = miny + iy * yStep;
            double x2 = x1 + xStep;
            double y2 = y1 + yStep;

            int id0 = GetVerticIndex(ix, iy);
            int id1 = GetVerticIndex(ix + 1, iy);
            int id2 = GetVerticIndex(ix, iy + 1);
            int id3 = GetVerticIndex(ix + 1, iy + 1);
            
            if (id0 >= 0 && CSurferGrid.IsBlankValue(pGridData[id0])) id0 = -1;
            if (id1 >= 0 && CSurferGrid.IsBlankValue(pGridData[id1])) id1 = -1;
            if (id2 >= 0 && CSurferGrid.IsBlankValue(pGridData[id2])) id2 = -1;
            if (id3 >= 0 && CSurferGrid.IsBlankValue(pGridData[id3])) id3 = -1;
            if (id0 < 0 && id1 < 0 && id2 < 0 && id3 < 0) return CSurferGrid.blankValue;

            double v0 = 0, v1 = 0, v2 = 0, v3 = 0;
            if (id0 >= 0) v0 = pGridData[id0];
            if (id1 >= 0) v1 = pGridData[id1];
            if (id2 >= 0) v2 = pGridData[id2];
            if (id3 >= 0) v3 = pGridData[id3];
            //    |y
            //    2----3
            //    |    |
            //    0----1---->x
            if (id0 >= 0 && id1 < 0 && id2 < 0 && id3 < 0) return v0;
            else if (id0 < 0 && id1 >= 0 && id2 < 0 && id3 < 0) return v1;
            else if (id0 < 0 && id1 < 0 && id2 >= 0 && id3 < 0) return v2;
            else if (id0 < 0 && id1 < 0 && id2 < 0 && id3 >= 0) return v3;

            else if (id0 >= 0 && id1 >= 0 && id2 < 0 && id3 < 0)//01
            {
                return v0 + (v1 - v0) * (x - x1) / xStep;
            }
            else if (id0 >= 0 && id2 >= 0 && id1 < 0 && id3 < 0)//02
            {
                return v0 + (v2 - v0) * (y - y1) / yStep;
            }
            else if (id0 >= 0 && id3 > 0 && id1 < 0 && id2 < 0)//03
            {
                double l = Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
                double ll = Math.Sqrt(xStep * yStep);
                return v0 + (v3 - v0) * l / ll;
            }
            else if (id1 >= 0 && id2 > 0 && id0 < 0 && id3 < 0)//12
            {
                double l = Math.Sqrt((x - x2) * (x - x2) + (y - y1) * (y - y1));
                double ll = Math.Sqrt(xStep * yStep);
                return v1 + (v2 - v1) * l / yStep;
            }
            else if (id1 >= 0 && id3 > 0 && id0 < 0 && id2 < 0)//13
            {
                return v1 + (v3 - v1) * (y - y1) / yStep;
            }
            else if (id0 >= 0 && id1 >= 0 && id2 >= 0 && id3 >= 0)//0123
            {
                double p1 = v0 + (x - x1) * (v1 - v0) / xStep;
                double p2 = v2 + (x - x1) * (v3 - v2) / xStep;
                return p1 + (p2 - p1) * (y - y1) / yStep;
            }
            else return v0;
        }
    }
    public enum OverlapMethod
    {
        Add = 0,
        Subtract =1,
        Average =2 ,
        Replace =3,
        None = 10,
    }
    public enum OverlapChannel
    {
        Value = 0,
        Alpha = 1,
        Vector2D = 2,
        Vector3D = 3,
        None = 10,
    }

    //叠加到grid对象上的数据
    public class COverlayObject
    {
        public string errMessage = "";
        public C3DGridData p3DGrid = null;          //3DGrid对象
        public int Demension = 1;                   //维度，1维只有1个值
        public float[] data = null;                 //grid 数据
        public C3DObjectBase sourceObject = null;   //叠加对象原始数据
        public ShapeEnum type = ShapeEnum.Points; //叠加数据对象类型        

        double[] minvArray = null, maxvArray = null;
        double minv = 0, maxv = 0;
        public bool Updated = true;
        public bool Enable { get; set; } = true;
        public bool Blend { get; set; } = true;
        public float Alpha { get; set; } = 1.0f;
        
        public OverlapMethod method { get; set; } = OverlapMethod.Add;
        public OverlapChannel channel { get; set; } = OverlapChannel.Alpha;

        public vec3 offset = new vec3(0, 0, 0);
        public vec3 rotate = new vec3(0, 0, 0);
        public vec3 scale = new vec3(1, 1, 1);
        [CategoryAttribute("Display"), DisplayNameAttribute("Translation")]
        public string OffsetString
        {
            get { return offset.x + "," + offset.y + "," + offset.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    offset = new vec3(p.X, p.Y, p.Z);
                    Updated = true;
                }
            }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Rotation")]
        public string RotateString
        {
            get { return rotate.x + "," + rotate.y + "," + rotate.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    rotate = new vec3(p.X, p.Y, p.Z);
                    Updated = true;
                }
            }
        }

        [CategoryAttribute("Display"), DisplayNameAttribute("Scale")]
        public string ScaleString
        {
            get { return scale.x + "," + scale.y + "," + scale.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    scale = new vec3(p.X, p.Y, p.Z);
                    Updated = true;
                }
            }
        }

        #region Color section

        public Color _ObjColor = Color.FromArgb(128, 128, 128);
        [CategoryAttribute("Color"), DisplayNameAttribute("Object Color")]
        public Color ObjColor
        {
            get { return _ObjColor; }
            set
            {
                _ObjColor = value;
                if (!_EnableColorLevel) Updated = true;
            }
        }

        public bool _EnableColorLevel = true;
        [CategoryAttribute("Color"), DisplayNameAttribute("Enable Color Map")]
        public bool EnableColorLevel
        {
            get { return _EnableColorLevel; }
            set { _EnableColorLevel = value; Updated = true; }
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
        [CategoryAttribute("Color"), DisplayNameAttribute("Color Map")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorScaleConverter))]
        public CColorScale ColorScale
        {
            get { return _ColorScale; }
            set
            {
                _OlderColorScale = _ColorScale;
                _ColorScale = value; Updated = true;
            }
        }

        #endregion Color section

        public COverlayObject(long length,int demension = 1)
        {
            Demension = demension;
            data = new float[demension * length];
        }
        public virtual bool Save(BinaryWriter br)
        {
            br.Write(Demension);
            br.Write((int)type);
            br.Write(minv);
            br.Write(maxv);
            br.Write(Enable);
            br.Write(Blend);
            br.Write(Alpha);
            br.Write((int)method);
            br.Write((int)channel);
            br.Write(offset.x);
            br.Write(offset.y);
            br.Write(offset.z);
            br.Write(rotate.x);
            br.Write(rotate.y);
            br.Write(rotate.z);
            br.Write(scale.x);
            br.Write(scale.y);
            br.Write(scale.z);
            if (data == null) br.Write((int)0);
            else 
            { 
                br.Write(data.Length);
                for (int i = 0; i < data.Length; i++)
                    br.Write(data[i]);
            }

            return true;
        }
        public virtual bool Load(BinaryReader br)
        {
            try 
            {
                Demension = br.ReadInt32();
                type = (ShapeEnum)br.ReadInt32();
                minv = br.ReadDouble();
                maxv = br.ReadDouble();
                Enable = br.ReadBoolean();
                Blend = br.ReadBoolean();
                Alpha = br.ReadSingle();
                method = (OverlapMethod)br.ReadInt32();
                channel = (OverlapChannel)br.ReadInt32();
                offset.x = br.ReadSingle();
                offset.y = br.ReadSingle();
                offset.z = br.ReadSingle();
                rotate.x = br.ReadSingle();
                rotate.y = br.ReadSingle();
                rotate.z = br.ReadSingle();
                scale.x = br.ReadSingle();
                scale.y = br.ReadSingle();
                scale.z = br.ReadSingle();

                data = null;
                int n = br.ReadInt32();
                if (n > 0)
                {
                    data = new float[n];
                    for (int i = 0; i < n; i++)
                        data[i] = br.ReadSingle();
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }            
        }
        public virtual Color GetColor(double v)
        {
            return ColorScale.GetColor(v);
        }       
        public virtual void UpdateRange()
        {
            minv = maxv = 0;
            if (data == null) return;
            int k = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (C3DData.IsBlankValue(data[i])) continue;
                if (k == 0) { minv = maxv = data[i]; k++; }
                else
                {
                    if (data[i] < minv) minv = data[i];
                    if (data[i] > maxv) maxv = data[i];
                }
            }
        }
        public virtual void UpdateRange(int col)
        {
            if (data == null) return;
            if (minvArray == null || maxvArray == null)
            {
                minvArray = new double[Demension];
                maxvArray = new double[Demension];
            }
            long id;
            for (int i = 0; i < data.Length / Demension; i++)
            {
                id = Demension * i + col;
                if (i == 0) minvArray[col] = maxvArray[col] = data[id];
                else
                {
                    if (data[id] < minvArray[col]) minvArray[col] = data[id];
                    if (data[id] > maxvArray[col]) maxvArray[col] = data[id];
                }
            }
        }
    }
    public class Arrow2DOverlayObject: COverlayObject
    {
        public bool Filled { get; set; } = true;
        public float arrowSize { get; set; } = 0.005f;
        public float Theta { get; set; } = 0.5f;
        public float LineWidth { get; set; } = 0.5f;
        public float LineLength { get; set; } = 0.02f;
        public Color LineColor { get; set; } = Color.Blue;
        public bool EnableLineColor { get; set; } = true;

        public int XInterval { get; set; } = 1;
        public int YInterval { get; set; } = 1;
        public int ZInterval { get; set; } = 1;
        
        public Arrow2DOverlayObject(long length, int demension = 1) 
            :base(length, demension = 1)
        {
            Demension = demension;
            data = new float[demension * length];
        }      
        public void from3Dgrid(C3DGridData grid3d)
        {
            sourceObject = grid3d;
            type = grid3d.type;
            Array.Copy(grid3d.pGridData,data, grid3d.Length);
            ColorScale.SetValueRange(sourceObject.minv, sourceObject.maxv);
        }
        public void fromScatterPoints(ScatteredPoints sc)
        {
            sourceObject = sc;
            type = sc.type;
            for (int i = 0; i < data.Length; i++ )
            {
                data[i] = float.NaN;
            }

            int ix, iy, iz;
            foreach(Vector32 p in sc.points)
            {
                ix = ( int ) ( (p.x - p3DGrid.minx) / p3DGrid.xStep );
                iy = ( int ) ( (p.y - p3DGrid.miny) / p3DGrid.yStep );
                iz = ( int ) ( (p.z - p3DGrid.minz) / p3DGrid.zStep );
                data[ix + iy * p3DGrid.xNum + iz * p3DGrid.xyNum] = p.V;
            }
            ColorScale.SetValueRange(sourceObject.minv,sourceObject.maxv);
        }
        public override bool Save(BinaryWriter br)
        {
            if (!base.Save(br)) return false;
            br.Write(Filled);
            br.Write(arrowSize);
            br.Write(Theta);
            br.Write(LineWidth);
            br.Write(LineLength);
            br.Write(EnableLineColor);
            br.Write(LineColor.ToArgb());            
            br.Write(XInterval);
            br.Write(YInterval);
            br.Write(ZInterval);
            return true;
        }
        public override bool Load(BinaryReader br)
        {
            if (!base.Load(br)) return false;
            Filled = br.ReadBoolean();
            arrowSize = br.ReadSingle();
            Theta = br.ReadSingle();
            LineWidth = br.ReadSingle();
            LineLength = br.ReadSingle();
            EnableLineColor = br.ReadBoolean();
            LineColor = Color.FromArgb(br.ReadInt32());
            XInterval = br.ReadInt32();
            YInterval = br.ReadInt32();
            ZInterval = br.ReadInt32();
            return true;
        }
    }
    public class Arrow3DOverlayObject : COverlayObject
    {
        public bool Filled { get; set; } = true;
        public float arrowSize { get; set; } = 0.01f;
        public float Theta { get; set; } = 0.5f;
        public float LineWidth { get; set; } = 0.5f;
        public float LineLength { get; set; } = 0.02f;
        public Color LineColor { get; set; } = Color.Blue;
        public bool EnableLineColor { get; set; } = true;

        public int XInterval { get; set; } = 1;
        public int YInterval { get; set; } = 1;
        public int ZInterval { get; set; } = 1;

        public Arrow3DOverlayObject(long length, int demension = 1)
            : base(length, demension = 1)
        {
            Demension = demension;
            data = new float[demension * length];
        }
        public void from3Dgrid(C3DGridData grid3d)
        {
            Array.Copy(grid3d.pGridData, data, grid3d.Length);
        }
        public void fromScatterPoints(ScatteredPoints sc)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = float.NaN;
            }

            offset.z = (float)(-p3DGrid.zStep * 0.5);

            int ix, iy, iz;
            foreach (Vector32 p in sc.points)
            {
                ix = (int)((p.x - p3DGrid.minx) / p3DGrid.xStep);
                iy = (int)((p.y - p3DGrid.miny) / p3DGrid.yStep);
                iz = (int)((p.z - p3DGrid.minz) / p3DGrid.zStep);
                data[ix + iy * p3DGrid.xNum + iz * p3DGrid.xyNum] = p.V;
            }
        }
    }
    public enum MCMeshMethod
    {
        Cube = 0,   //cube grid,no meshes
        MC = 1,     //MC Meshes
        ImprovedMC = 2,//Improved MC Meshes
    };
    /// <summary>
    /// 大数据3DGrid输入/输出
    /// </summary>
    public class C3DGridDataStream
    {
        public string dataFile = "";
        public string errMessage = "";
        public C3DGridData gridData = new C3DGridData();
        int num = 0;
        public bool Create( string filename,int nx, int ny, int nz, 
                            double minx, double miny, double minz,double minv,
                            double maxx, double maxy, double maxz,double maxv )
        {
            try
            {
                dataFile = filename;
                BinaryWriter br;
                br = new BinaryWriter(new FileStream(dataFile, FileMode.Create));
                gridData = new C3DGridData(minx,maxx,miny,maxy,minz,maxz,minv,maxv);
                gridData.xNum = nx;
                gridData.yNum = ny;
                gridData.zNum = nz;
                gridData.Write3DGridHeader(br,10);
                num = 0;
                br.Close();
                return true;
            }
            catch (IOException e)
            {
                errMessage = "write file failed.\n" + e.Message;
                return false;
            }
        }
        
        public bool WriteData(float[] grid,int start,int length) 
        {
            try
            {
                BinaryWriter br = new BinaryWriter(new FileStream(dataFile, FileMode.Append));
                for (int i = start; i < start + length; i++)
                { 
                    br.Write(grid[i + start]); 
                    num++;
                }
                br.Close();
                return true;
            }
            catch (IOException e)
            {
                errMessage = "write file failed.\n" + e.Message;
                return false;
            }
        }
        public bool Close()
        {
            if (num == gridData.xNum * gridData.yNum * gridData.zNum)
                return true;
            else 
            {
                errMessage = "data not correct!";
                return false; 
            }
        }
    }
    public class C3DGridData : C3DObjectBase
    {
        public int xNum;
        public int yNum;
        public int zNum;
        public long xyzNum
        {
            get { return xNum * yNum * zNum; }
        }

        public double xStep, yStep, zStep;
        public long xyNum = 0;
        
        //grid data
        public float[] pGridData = null;
        //public Vector32[] pGridCoords = null; //网格节点坐标 Added 2024.12.6
        public double m_blankvalue = C3DData.m_BlankedValue;
        public AxisEnum zAxis = AxisEnum.zAxis; //Z轴
        //show state of each unit, 0-don't show,1-show,2-blend
        public byte[] pgridShowTable;
        public Int16[] pColorIndexTable;        
        
        public List<UInt32XYZ> pShowIndexArray = new List<UInt32XYZ>();
        public List<CTriangle3f> pTriangleArray = new List<CTriangle3f>();
        public float[] pXGrids = null; //非均匀网格情况下X坐标
        public float[] pYGrids = null; //非均匀网格情况下Y坐标
        public float[] pZGrids = null; //非均匀网格情况下Z坐标
        MyBinarySearch binarySearch = new MyBinarySearch(null);

        public MarchingCubes m_MarchCube = new MarchingCubes();
        public MarchingCubesExt m_MarchCubeExt = new MarchingCubesExt();
        public List<vec2> visibleValues = new List<vec2>();

        //public List<COverlayObject> overlaps = new List<COverlayObject>();
        public C3DGridData objP32 = null;

        public int[] pBlankedPointIndexes = null;
        public List<vec3> pBlankedPoints = new List<vec3>();
        public bool[] pBlankTable = null;        
        public virtual int Length 
        { 
            get 
            {
                if ( pGridData == null ) return 0;
                else return pGridData.Length; 
            } 
        }


        #region Overlaps section
        [CategoryAttribute("Overlaps"), DisplayNameAttribute("Count")]
        public int OverlayCount { get { return overlaps.Count; } }
        [CategoryAttribute("Overlaps"), DisplayNameAttribute("Objects")]
        public List<COverlayObject> overlaps { get; set; } = new List<COverlayObject>();
        [CategoryAttribute("Overlaps"), DisplayNameAttribute("Enabled")]
        public bool enableOverlap { get; set; }
        #endregion Overlaps section

        #region Display section
        public MCMeshMethod _meshMethod = MCMeshMethod.Cube;
        [CategoryAttribute("Display"), DisplayNameAttribute("Render Method")]
        public MCMeshMethod meshMethod 
        { 
            get { return _meshMethod; } 
            set { _meshMethod = value;RenderMode = RenderingUpdateMode.Redraw; } 
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Is Stratums")]
        public bool ShowAsStratum { get; set; } = false;

        #endregion Display section

        #region Color section

        public Color _ObjColor = Color.FromArgb(128, 128, 128);
        [CategoryAttribute("Color"), DisplayNameAttribute("Object Color")]
        public Color ObjColor 
        {   
            get { return _ObjColor; }
            set { _ObjColor = value;
                  if (!_EnableColorLevel)RenderMode = RenderingUpdateMode.Redraw;
                } 
        }

        public bool _EnableColorLevel = true;
        [CategoryAttribute("Color"), DisplayNameAttribute("Enable Color Map")]
        public bool EnableColorLevel 
        {   get { return _EnableColorLevel; } 
            set { _EnableColorLevel = value; RenderMode = RenderingUpdateMode.Redraw; } 
        }

        public bool IsColorScaleUpdated()
        {
            if(EnableColorLevel)
            {
                if (_ColorScale.DifferentFrom(_OlderColorScale)) 
                    return true;
            }
            return false;
        }
        public CColorScale _OlderColorScale = new CColorScale();
        public CColorScale _ColorScale = new CColorScale();
        [CategoryAttribute("Color"), DisplayNameAttribute("Color Map")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorScaleConverter))]
        public CColorScale ColorScale
        { 
            get { return _ColorScale; }           
            set { _OlderColorScale = _ColorScale;
                  _ColorScale = value;                  
                  RenderMode = RenderingUpdateMode.Redraw; 
                }
        }

        #endregion Color section

        //double tracedMinx, tracedMiny, tracedMinz, tracedMaxx, tracedMaxy, tracedMaxz;
        public override string Information
        {
            get
            {
                string info = "Size = " + xNum + " * " + yNum + " * " + zNum + ";\n";
                info += "X: " + Math.Round(Minx,4) + " to " + Math.Round(Maxx,4) + ";\n";
                info += "Y: " + Math.Round(Miny,4) + " to " + Math.Round(Maxy,4) + ";\n";
                info += "Z: " + Math.Round(Minz,4) + " to " + Math.Round(Maxz,4) + ";\n";
                info += "V: " + Math.Round(Minv,4) + " to " + Math.Round(Maxv,4);
                return info;
            }
            //set { name = value; }
        }
       
        /// <summary>
        /// 网格重插值
        /// </summary>
        /// <param name="invalid_value">需插值网格</param>
        /// <param name="blank_interpolate">是否对白化网格插值</param>
        public float[] GridsSampledInterpolating( float invalid_value = 0, 
                                                  bool blank_interpolate = false )
        {
            float[] grid = new float[pGridData.Length];
            float val;
            
            for(int i = 0; i < pGridData.Length; i++ )
            {
                val = pGridData[i];
                if( (IsBlankValue(val) && blank_interpolate)
                    || val <= 0 )
                    {
                        val = ReInterpolateFromGrid(i);
                    }
                grid[i] = val;
            }
            return grid;
        }
        public float ReInterpolateFromGrid(int id)
        {
            Int32XYZ xyz = GetIndices(id);
            List<Vector32>points = SearchNearestPoints(xyz.x, xyz.y, xyz.z, -1);
            if (points.Count < 1) return float.NaN;

            float val = 0;
            double maxdist = 1E10, dist;
            Vector32 p0 = GetGridCoord(xyz.x, xyz.y, xyz.z);
            for (int i = 0; i < points.Count; i++)
            {
                Vector32 p = points[i];
                dist = p.DistancePower2(p0);
                if (dist < maxdist)
                {
                    maxdist = dist;
                    val = p.V;
                }
            }
            points.Clear();
            return val;
        }
        public C3DGridData Copy()
        {
            C3DGridData data = new C3DGridData(xNum, yNum, zNum);
            data.CopyHeaderFrom(this);           
            data.xStep = xStep;
            data.yStep = yStep;
            data.zStep = zStep;
            data.xyNum = xyNum;
            //grid data
            if(pGridData != null)
            {
                data.pGridData = new float[Length];
                Array.Copy(pGridData, data.pGridData, Length);
            }
            //show state of each unit, 0-don't show,1-show,2-blend
            if (pgridShowTable != null)
            {
                data.pgridShowTable = new byte[pgridShowTable.Length];
                Array.Copy(pgridShowTable, data.pgridShowTable, pgridShowTable.Length);
            }
            if (pColorIndexTable != null)
            {
                data.pColorIndexTable = new Int16[pgridShowTable.Length];
                Array.Copy(pColorIndexTable, data.pColorIndexTable, pColorIndexTable.Length);
            }      
            if(pShowIndexArray.Count>0)
            {
                data.pShowIndexArray = new List<UInt32XYZ>(pShowIndexArray);
            }
            if (pXGrids != null)
            {
                data.pXGrids = new float[pXGrids.Length];
                Array.Copy(pXGrids, data.pXGrids, pXGrids.Length);
            }
            if (pYGrids != null)
            {
                data.pYGrids = new float[pYGrids.Length];
                Array.Copy(pYGrids, data.pYGrids, pYGrids.Length);
            }
            if (pZGrids != null)
            {
                data.pZGrids = new float[pZGrids.Length];
                Array.Copy(pZGrids, data.pZGrids, pZGrids.Length);
            }
            
            if (pTriangleArray.Count > 0)
            {
                data.pTriangleArray = new List<CTriangle3f>(pTriangleArray);
            }

            data.Version = Version;
            data._meshMethod = _meshMethod;
            data._ObjColor = _ObjColor;
            data._EnableColorLevel = _EnableColorLevel;
            data._ColorScale = _ColorScale.Copy();

            if ( pBlankedPointIndexes != null)
            {
                data.pBlankedPointIndexes = new int[pBlankedPointIndexes.Length];
                Array.Copy(pBlankedPointIndexes, data.pBlankedPointIndexes, pBlankedPointIndexes.Length);
            }
            if (pBlankTable != null)
            {
                data.pBlankTable = new bool[pBlankTable.Length];
                Array.Copy(pBlankTable, data.pBlankTable, pBlankTable.Length);
            }
            if (pBlankedPoints.Count > 0)
            {
                data.pBlankedPoints = new List<vec3>(pBlankedPoints);
            }
            
            data.m_MarchCube = m_MarchCube;
            data.m_MarchCubeExt = m_MarchCubeExt;
            data.visibleValues = new List<vec2>(visibleValues);

            return data;
    }

    //this table include all blanked intersections on edge
    // int index, vec3 p, p.x,p.y,p.z are intersect value on x,y,z axis,
        public void AddBlankIntersection(long id, vec3 pos)
        {
            pBlankedPointIndexes[id] = pBlankedPoints.Count;
            pBlankedPoints.Add(pos);
        }
        //计数单位 0 m, 1 km
        public double CalculateVolume(int unit = 0)
        {
            if (pGridData == null) return 0;

            double cube = xStep * yStep * zStep;
            if (unit == 1) cube = xStep / 1000 * yStep / 1000 * zStep / 1000;
            long count = 0;
            int icolor;
            for( long i=0; i < xNum *yNum * zNum; i++ )
            {
                if( !IsBlankedGrid( i ) )
                {
                    icolor = pColorIndexTable[i];
                    if( ColorScale[icolor].Visible )
                        count++;
                }
            }
            return cube * count;
        }


        public float this[int id]
        {
            get
            {
                return pGridData[id];
            }
            set
            {
                pGridData[id] = value;
            }
        }
        public float this[long id]
        {
            get
            {
                return pGridData[id];
            }
            set
            {
                pGridData[id] = value;
            }
        }
        public float this[int ix, int iy, int iz]
        {
            get
            {
                long id = GetVerticIndex(ix, iy, iz);
                return pGridData[id];
            }
            set
            {
                long id = GetVerticIndex(ix, iy, iz);
                pGridData[id] = value;
            }
        }

       
        public void GetIndices(Vector32 p, ref int ix, ref int iy, ref int iz)
        {
            ix = (int)( (p.X - minx) / xStep);
            iy = (int)( (p.Y - miny) / yStep);
            iz = (int)( (p.Z - minz) / zStep);
        }
        public Int32XYZ GetIndices(Vector32 p)
        {
            Int32XYZ xyz = new Int32XYZ();
            xyz.x = (int)((p.X - minx) / xStep);
            xyz.y = (int)((p.Y - miny) / yStep);
            xyz.z = (int)((p.Z - minz) / zStep);
            return xyz;
        }
       
        public Int32XYZ GetIndices(Vector64 p,double percent)
        {
            int ix = (int)( (p.x - minx) / xStep );
            double dx = p.X - ix * xStep;
            if ( Math.Abs(xStep - dx) < xStep * percent ) ix++;
            
            int iy = (int)((p.y - miny) / yStep);
            double dy = p.Y - iy * yStep;
            if ( Math.Abs(yStep - dy) < yStep * percent) iy++;

            int iz = (int)((p.z - minz) / zStep);
            double dz = p.Z - iz * zStep;
            if ( Math.Abs(zStep - dz) < zStep * percent) iz++;

            return (new Int32XYZ(ix, iy, iz)); 
        }
        public Int32XYZ GetIndices(long id)
        {
            xyNum = xNum * yNum;
            Int32XYZ xyz = new Int32XYZ();
            xyz.z = (int)(id /xyNum);
            long left = id % xyNum;
            xyz.y = (int)(left / xNum);
            xyz.x = (int)(left % xNum);
            return xyz;
        }

        double GetXStep(int ix,int iy,int iz)
        {
            return xStep;           
        }
        double GetYStep(int ix, int iy, int iz)
        {
            return yStep;            
        }
        double GetZStep(int ix, int iy, int iz)
        {
            return zStep;            
        }

        public bool IsEdgeGrid(long id)
        {
            Int32XYZ xyz = GetIndices(id);
            return IsEdgeGrid(xyz.x,xyz.y,xyz.z);
        }
        public bool IsEdgeGrid(int ix, int iy, int iz)
        {
            if (IsBlankedGrid(ix, iy, iz)) return true;
            if (ix == 0 || iy == 0 || iz == 0 ||
                ix == xNum - 1 || iy == yNum - 1 || iz == zNum - 1) return true;
            if ( IsBlankedGrid(ix + 1, iy, iz) ) return true;
            if ( IsBlankedGrid(ix, iy+1, iz)) return true;
            if ( IsBlankedGrid(ix, iy, iz+1)) return true;
            if ( ix > 0 && IsBlankedGrid(ix - 1, iy, iz) ) return true;
            if (iy > 0 && IsBlankedGrid(ix, iy-1, iz)) return true;
            if (iz > 0 && IsBlankedGrid(ix, iy, iz-1)) return true;
            return false;
        }
        public bool IsBlankedGrid(Int32XYZ p)
        {
            return IsBlankedGrid(p.x,p.y,p.z);
        }
        public bool IsBlankedGrid(long id)
        {
            if (id < 0 || id >= xNum * yNum * zNum) return true;
            if ( pBlankTable == null ) return IsBlankValue( pGridData[id] );
            return pBlankTable[id];
        }
        public bool IsBlankedGrid(int ix, int iy, int iz, int ver = 0)
        {
            if ( ix < 0 || iy < 0 || iz < 0 || 
                 ix >= xNum || iy >= yNum || iz >= zNum ) return true;
            return IsBlankedGrid( GetVerticIndex( ix, iy, iz, ver ) );
        }

        public void SetBlankGrid(int ix, int iy, int iz, bool blanked = true)
        {
            pBlankTable[GetVerticIndex(ix, iy, iz)] = blanked;
        }
        public bool GetBlankedValue(long id, out vec3 p)
        {
            if (pBlankedPointIndexes[id] < 0)
            {
                p = new vec3(0, 0, 0);
                return false;
            }
            else
            {
                p = pBlankedPoints[pBlankedPointIndexes[id]];
                return true;
            }
        }
        public bool GetBlankedValue(long id, AxisEnum axis, out vec3 p)
        {
            if ( GetBlankedValue(id, out p) )
            { 
                if (axis == AxisEnum.xAxis && !float.IsNaN(p.x))
                    return true;
                if (axis == AxisEnum.yAxis && !float.IsNaN(p.y))
                    return true;
                if (axis == AxisEnum.zAxis && !float.IsNaN(p.z))
                    return true;
            }
            return false;
        }
        public bool GetBlankedValue(int ix, int iy, int iz, out vec3 p)
        {
            return GetBlankedValue(GetVerticIndex(ix, iy, iz), out p);
        }
        public bool GetBlankedPoint(long id, out vec3 p)
        {
            p = new vec3(0, 0, 0);
            int index = pBlankedPointIndexes[id];
            if (index < 0) return false;
            p = pBlankedPoints[index];
            return true;
        }

        //sort = 0, set anyway, 
        //sort = 1, set the lower one, axis direction
        //sort = 2, set the greate one, axis direction 
        public void SetBlankValue(long id, double v, int axis, int sort = 0)
        {
            SetBlankValue(id,v,(AxisEnum)axis,(AxisOrderEnum)sort);
        }
        public void SetBlankValue(long id, double v, AxisEnum axis, AxisOrderEnum sort = AxisOrderEnum.Anyway)
        {
           // if ( id < 0 || id >= pBlankedPointIndexes.Length ||
           //      id >= pBlankedPoints.Count) return;

            //      |y
            //      |1
            //      o---0---->x
            //   z /2    
            int index = pBlankedPointIndexes[id];
            if (index < 0)//是否存数据，没有则创建一个并添加
            {
                pBlankedPointIndexes[id] = pBlankedPoints.Count;
                vec3 p = new vec3(float.NaN, float.NaN, float.NaN);
                if (axis == AxisEnum.xAxis) p.x = (float)v;
                if (axis == AxisEnum.yAxis) p.y = (float)v;
                if (axis == AxisEnum.zAxis) p.z = (float)v;
                pBlankedPoints.Add(p);
                return;
            }

            //else blanked value existed
            vec3 p0 = pBlankedPoints[index];
            
            if (axis == AxisEnum.xAxis) // x轴
            {
                if ( !IsValid(p0.x) ) p0.x = (float)v;                
                else
                {
                    if (sort == AxisOrderEnum.Lower)//保留低值
                    {
                        if (v < p0.x) p0.x = (float)v;
                    }
                    else if (sort == AxisOrderEnum.Upper)//保留高值
                    {
                        if (v > p0.x) p0.x = (float)v;
                    }
                }
            }
            else if (axis == AxisEnum.yAxis)// Y轴
            {
                if (!IsValid(p0.y)) p0.y = (float)v;
                else
                {
                    if (sort == AxisOrderEnum.Lower)//保留低值
                    {
                        if (v < p0.y) p0.y = (float)v;
                    }
                    else if (sort == AxisOrderEnum.Upper)//保留高值
                    {
                        if (v > p0.y) p0.y = (float)v;
                    }
                }
            }
            else if (axis == AxisEnum.zAxis)// Z轴
            {
                if (!IsValid(p0.z)) p0.z = (float)v;
                else
                {
                    if (sort == AxisOrderEnum.Lower)//保留低值
                    {
                        if (v < p0.z) p0.z = (float)v;
                    }
                    else if (sort == AxisOrderEnum.Upper)//保留高值
                    {
                        if (v > p0.z) p0.z = (float)v;
                    }
                }
            }

            pBlankedPoints[index] = p0;
        }
        public void SetBlankValue(int ix, int iy, int iz, double v, AxisEnum axis, AxisOrderEnum sort = AxisOrderEnum.Anyway)
        {
            SetBlankValue(GetVerticIndex(ix, iy, iz), v, axis, sort);
        }
        public bool GetBlankedValue(long id, out vec3 p, int axis)
        {
            if (pBlankedPointIndexes[id] < 0)
            {
                p = new vec3(0, 0, 0);
                return false;
            }
            else
            {
                p = new vec3(0, 0, 0);
                int pos = pBlankedPointIndexes[id];
                if (pos < 0) return false;

                p = pBlankedPoints[pBlankedPointIndexes[id]];
                if (axis == 0) return IsValidX(p.x);
                else if (axis == 1) return IsValidY(p.y);
                else if (axis == 2) return IsValidZ(p.z);
                else return false;
            }
        }
        /// <summary>
        /// Flip grid data
        /// </summary>
        /// <param name="xyz"></param>
        public void FlipGridData(AxisEnum xyz)
        {
            float[] grid = new float[pGridData.Length];
            Array.Copy(pGridData, grid, grid.Length);
            long id ,id1 = 0;
            for (int iz = 0; iz < zNum; iz++)
            {
                for (int iy = 0; iy < yNum; iy++)
                {
                    for (int ix = 0; ix < xNum; ix++)
                    {
                        id = GetVerticIndex(ix, iy, iz);
                        if (xyz == AxisEnum.xAxis) id1 = GetVerticIndex(xNum - 1 - ix, iy, iz);
                        else if (xyz == AxisEnum.yAxis) id1 = GetVerticIndex(ix, yNum - 1 - iy, iz);
                        else if (xyz == AxisEnum.zAxis) id1 = GetVerticIndex(ix, iy, zNum - 1 - iz);
                        else if (xyz == AxisEnum.XYExchange) id1 = GetVerticIndex(iy, ix, iz);
                        pGridData[id] = grid[id1];
                    }
                }
            }
            grid = null;
        }
        public void SampleTo(string path, int xstep,int ystep,int zstep)
        {
            double x, y, z;
            int ix, iy, iz;
            float val = 0;
            StreamWriter wr = new StreamWriter(path);
            wr.WriteLine("x,y,z,value");
            for (iz = 0; iz <zNum; iz+=zstep)
                for (iy = 0; iy < yNum; iy+=ystep)
                    for (ix = 0; ix < xNum; ix+=xstep)
                    {
                        x = minx + ix * xStep;
                        y = miny + iy * yStep;
                        z = minz + iz * zStep;
                        val = this[ix, iy, iz];
                        Vector64 p = new Vector64(x,y,z,val);
                        wr.WriteLine(p.toString(4));
                    }
            wr.Close();
        }
        public void DoFilter(int step = 1)
       {
            int ix, iy, iz,ix1,iy1,iz1;
            float val = 0,val1 = 0;
            for(int i=0;i<pGridData.Length;i++)
            {
                GetXYZIndexFromIndex(i, out ix, out iy, out iz);
                val = pGridData[i];
                int count = 0, countall = 0;
                for (iz1 = iz - step; iz1 <= iz + step; iz1++)
                  for (iy1 = iy - step; iy1 <= iy + step; iy1++)
                    for (ix1 = ix - step; ix1 <= ix + step; ix1++ )
                     {
                         if (!IsGridIndexValid(ix1, iy1, iz1)) continue;
                         if (ix1 == ix && iy1 == iy && iz1 == iz) continue;
                         val1 = this[ix1, iy1, iz1];
                         if (Math.Abs(val - val1) > (maxv - minv) * 0.1) count++;
                         countall++;
                     }
                if( (float)count / (float)countall >= 0.8 ) pGridData[i] = val1;
            }
       }
        public override float[] toValuesArray()
        {
            return pGridData;
        }
       
        public override void UpdateRange()
        {
            minv = maxv = 0;
            if (xNum < 1 || yNum < 1 || zNum < 1) return;
            if (pGridData == null) return;            
            int k = 0;
            for (int i = 0; i < xNum * yNum * zNum; i++)
            {
                if (float.IsNaN(pGridData[i]) ||
                    float.IsInfinity(pGridData[i]) ||
                    pGridData[i]== float.MaxValue ||
                    IsBlankValue(pGridData[i]) ) continue;
                if ( k == 0) { minv = maxv = pGridData[i]; k++; }
                else
                {
                    if (pGridData[i] < minv) minv = pGridData[i];
                    if (pGridData[i] > maxv) maxv = pGridData[i];
                }                
            }
        }       

        //normalize grid value to v1 v2
        public void NormalizeGrid(double v1 = 0, double v2 = 1)
        {
            if (pGridData == null) return;

            double scale = (v2 - v1) / (maxv - minv);

            for (long i = 0; i < xNum * yNum * zNum; i++)
            {
                pGridData[i] = (float)(v1 + (pGridData[i] - minv) * scale);
            }
            minv = v1;
            maxv = v2;
        }

        public override void Normalize()
        {
            if (xNum < 2 || yNum < 2 || zNum < 2) return;

            Vector32 p0 = GetCenter32();
            Vector64 p = new Vector64(0, 0, 0);

            double x0 = (minx + maxx) / 2.0;
            double y0 = (miny + maxy) / 2.0;
            double z0 = (minz + maxz) / 2.0;

            double xl = scale.x * (maxx - minx);
            double yl = scale.y * (maxy - miny);
            double zl = scale.z * (maxz - minz);

            minx = x0 - xl / 2 + offset.x;
            maxx = x0 + xl / 2 + offset.x;

            miny = y0 - yl / 2 + offset.y;
            maxy = y0 + yl / 2 + offset.y;

            minz = z0 - zl / 2 + offset.z;
            maxz = z0 + zl / 2 + offset.z;

            scale = new vec3(1, 1, 1);
            offset = new vec3(0, 0, 0);
            //rotate = new vec3(0, 0, 0);
        }
        public override void ScaledToRange(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            minx = x1;
            miny = y1;
            minz = z1;
            maxx = x2;
            maxy = y2;
            maxz = z2;
        }
        public bool IsValid(float v)
        {
            return !float.IsNaN(v);
        }
        public bool IsValid(double v)
        {
            return !double.IsNaN(v);
        }
        public bool IsValidX(double v)
        {
            if (!IsValid(v)) return false;
            if (v >= minx && v <= maxx)
                return true;
            else return false;
        }
        public bool IsValidY(double v)
        {
            if (!IsValid(v)) return false;
            if (v >= miny && v <= maxy)
                return true;
            else return false;
        }
        public bool IsValidZ(double v)
        {
            if (!IsValid(v)) return false;
            if (v >= minz && v <= maxz)
                return true;
            else return false;
        }
        public bool IsValidV(double v)
        {
            if (!IsValid(v)) return false;
            if (v >= minv && v <= maxv)
                return true;
            else return false;
        }
        public bool GetXYZIndexFromIndex(long id, out int ix, out int iy, out int iz)
        {
            xyNum = xNum * yNum;
            iz = (int)(id / xyNum);
            iy = (int)((id - iz * xyNum) / xNum);
            ix = (int)(id - iy * xNum - iz * xyNum);
            if (ix < 0 || iy < 0 || iz < 0) return false;
            else return true;
        }
        public Int32XYZ GetVerticIndexByPosition(double x, double y, double z, double err = 0.1)
        {
            int ix = (int)((x - minx) / xStep + err);            
            int iy = (int)((y - miny) / yStep + err);            
            int iz = (int)((z - minz) / zStep + err);            
            return new Int32XYZ(ix, iy, iz);
        }
        
        public long GetVerticIndex(int ix, int iy, int iz)
        {
            return ix + iy * xNum + iz * xNum * yNum;
        }
        public long GetVerticIndex(Int32XYZ p)
        {
            return GetVerticIndex(p.x,p.y,p.z);
        }
        public long GetVerticIndex(int ix, int iy, int iz, int vert)
        {
            xyNum = xNum * yNum;
            long icur = ix + iy * xNum + iz * xyNum;
            switch (vert)
            {
                case 0:
                    break;
                case 1:
                    icur += 1;
                    break;
                case 2:
                    icur += 1;
                    icur += xyNum;
                    break;
                case 3:
                    icur += xyNum;
                    break;
                case 4:
                    icur += xNum;
                    break;
                case 5:
                    icur += 1;
                    icur += xNum;
                    break;
                case 6:
                    icur += 1;
                    icur += xyNum;
                    icur += xNum;
                    break;
                case 7:
                    icur += xyNum;
                    icur += xNum;
                    break;
            }
            return icur;
        }
        /// <summary>
        /// center coordinate of the grid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// y
        /// |-----|
        /// |  p  |
        /// --------->x
        public Vector32 GetGridCoord(int ix, int iy, int iz)
        {
            double x = minx + ix * xStep;
            double y = miny + iy * yStep;
            double z = minz + iz * zStep;
            double v = this[ix,iy,iz];
            return new Vector32(x, y, z,v);
        }
        public Vector32 GetGridCoord(long id)
        {
            GetXYZIndexFromIndex(id, out int ix, out int iy, out int iz);
            return GetGridCoord(ix, iy, iz);
        }

        public Vector32 GetVerticCoord(int ix0, int iy0, int iz0, int ver)
        {
            int ix = ix0;
            int iy = iy0;
            int iz = iz0;
            if (ver == 1) ix++;
            if (ver == 2) { ix++; iz++; }
            if (ver == 3) iz++;
            if (ver == 4) iy++;
            if (ver == 5) { ix++; iy++; }
            if (ver == 6) { ix++; iy++;iz++; }
            if (ver == 7) { iy++; iz++; }
            if (ix >= xNum) ix = xNum - 1;
            if (iy >= yNum) iy = yNum - 1;
            if (iz >= zNum) iz = zNum - 1;
            return GetGridCoord(ix, iy, iz);
        }

        public void InitBlankTable()
        {
            pBlankedPoints.Clear();
            if (pBlankedPointIndexes == null)
            {
                pBlankedPointIndexes = new int[xNum * yNum * zNum];
            }
            if (pBlankTable == null)
            {
                pBlankTable = new bool[xNum * yNum * zNum];
            }
            for (int i = 0; i < xNum * yNum * zNum; i++)
            {
                pBlankTable[i] = false;
                pBlankedPointIndexes[i] = -1;
            }
        }
        public void ClearBlankTable()
        {
            pBlankedPoints.Clear();
            pBlankedPointIndexes = null;
            pBlankTable = null;
        }
        //para: 0 keep left 1 keep right
        private bool CutWithClosedXOYSlicer(CSlicer slicer, int para = 0)
        {
            int ix, iy, iz, id, id1, id2, id3;
            Vector64 p1, p2;
            Vector64 p = new Vector64();

            bool keepinside = true;
            Polygon2D polygon;
            //create polygon2d
            if (slicer.Closed)
            {
                polygon = new Polygon2D(slicer.pBaseLine);
                if (para == 0) keepinside = true;
                else if (para == 1) keepinside = false;
            }
            else
            {
                keepinside = true;
                bool left = true;
                if (para == 1) left = false;
                polygon = slicer.RectIntersect(minx, miny, minz, maxx, maxy, maxz);
            }

            bool[] point_in = new bool[xNum * yNum];
            point_in.Initialize();
            double x1, x2, y1, y2, z1, z2;
            int height1 = (int)((slicer.minHeight - minz + 0.01 * zStep) / zStep);
            int height2 = (int)((slicer.maxHeight - minz + 0.01 * zStep) / zStep + 1);
            if (height1 < 0) height1 = 0;
            if (height2 > zNum) height2 = zNum;
            //1 check in or out
            for (iy = 0; iy < yNum; iy++)
            {
                for (ix = 0; ix < xNum; ix++)
                {
                    p.X = minx + ix * xStep;
                    p.Y = miny + iy * yStep;
                    //p.Z = (float)maxz;
                    id = iy * xNum + ix;
                    point_in[id] = polygon.IsPointInsidePoly(p);
                }
            }
            //2 blank grid
            for (iy = 0; iy < yNum; iy++)
            {
                for (ix = 0; ix < xNum; ix++)
                {
                    id = iy * xNum + ix;
                    if ((!point_in[id] && keepinside) ||
                         (point_in[id] && !keepinside))
                    {
                        for (iz = height1; iz < height2; iz++)
                            SetBlankGrid(ix, iy, iz);
                    }
                }//for (ix = 0; ix < xNum; ix++)
            }//for (iy = 0; iy < yNum; iy++)

            //3 calculate and set intersections
            z1 = z2 = maxz;
            for (iy = 0; iy < yNum - 1; iy++)
            {
                y1 = miny + iy * yStep;
                for (ix = 0; ix < xNum - 1; ix++)
                {
                    x1 = minx + ix * xStep;
                    id = iy * xNum + ix;//x1y1
                    id1 = id + 1;   //x2
                    id2 = id + xNum;//y2

                    if (point_in[id] != point_in[id1]) //x1-x2
                    {
                        x2 = x1 + xStep;
                        y2 = y1;
                        p1 = new Vector64(x1, y1, z1);
                        p2 = new Vector64(x2, y1, z1);
                        CLine line = new CLine(p1, p2);
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if (inters.Length > 0)
                        {
                            for (iz = height1; iz < height2; iz++)
                            {
                                id3 = iz * (int)xyNum + iy * xNum + ix;//x1y1
                                if (pBlankTable[id3]) //0---inertp---->1
                                    SetBlankValue(id3, inters[0].X, AxisEnum.xAxis, AxisOrderEnum.Upper);
                                else //1---inertp---->0 (x)
                                    SetBlankValue(id3, inters[0].X, AxisEnum.xAxis, AxisOrderEnum.Lower);
                            }
                        }
                    }//if (point_in[id] != point_in[id1]) //x1-x2
                    if (point_in[id] != point_in[id2]) //y1-y2
                    {
                        y2 = y1 + yStep;
                        p1 = new Vector64(x1, y1, z1);
                        p2 = new Vector64(x1, y2, z1);
                        CLine line = new CLine(p1, p2);
                        //get intersect points
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if (inters.Length > 0)
                        {   //cut and set intersetions
                            for (iz = height1; iz < height2; iz++)
                            {
                                id3 = iz * (int)xyNum + iy * xNum + ix;//x1y1
                                if ( pBlankTable[id3] ) //0---inertp---->1
                                    SetBlankValue(id3, inters[0].Y, AxisEnum.yAxis, AxisOrderEnum.Upper);
                                else //1---inertp---->0
                                    SetBlankValue(id3, inters[0].Y, AxisEnum.yAxis, AxisOrderEnum.Lower);
                            }
                        }
                    }//if (point_in[id] != point_in[id2]) //y1-y2
                }//for (ix = 0; ix < xNum - 1; ix++)
            }//for (iy = 0; iy < yNum - 1; iy++)
            return true;
        }//bool CutWithXOYSlicer
        /*
        //--------------------------------------------------
        //----------Cut with none-enclosed slicer-----------
        //  para: 0 keep left,
        //        1 keep right 
        //        2keep intersection
        //---------------------------------------------
        private bool CutWithXOYSlicer(CSlicer slicer, int para = 0 )
        {
            int ix, iy, iz, id, id1, id2, id3;
            Vector32 p1, p2;
            Vector32 p = new Vector32();

            bool[] blanked = new bool[xNum * yNum];
            blanked.Initialize();

            double x1, x2, y1, y2, z1, z2;
            int height1 = (int)((slicer.minHeight - minz) / zStep);
            int height2 = (int)((slicer.maxHeight - minz) / zStep + 1);
            if (height1 < 0) height1 = 0;
            if (height2 > zNum) height2 = zNum;
            //1 blanked or not
            PointLineRelation relation = PointLineRelation.Left;
            for (iy = 0; iy < yNum; iy++)
            {
                for (ix = 0; ix < xNum; ix++)
                {
                    p.X = (float)(minx + ix * xStep);
                    p.Y = (float)(miny + iy * yStep);
                    //p.Z = (float)maxz;
                    id = iy * xNum + ix;
                    relation = slicer.CheckPointPosition(p);
                    if ( (relation != PointLineRelation.Left  &&  para == 0 ) ||
                         (relation != PointLineRelation.Right &&  para == 1 ) ||
                         (relation != PointLineRelation.OnLine && para == 2 ) )
                    {
                        blanked[id] = true;
                        for (iz = height1; iz < height2; iz++)
                            SetBlankGrid(ix, iy, iz);
                    }
                }//for (ix = 0; ix < xNum; ix++)
            }//for (iy = 0; iy < yNum; iy++)           
            //3 calculate and set intersections
            Vector32 inters;
            z1 = z2 = maxz;
            for (iy = 0; iy < yNum - 1; iy++)
            {
                y1 = miny + iy * yStep;
                for (ix = 0; ix < xNum - 1; ix++)
                {
                    x1 = minx + ix * xStep;
                    id = iy * xNum + ix;//x1y1
                    id1 = id + 1;   //x2
                    id2 = id + xNum;//y2

                    if (blanked[id] != blanked[id1]) //x1-x2
                    {
                        x2 = x1 + xStep;
                        y2 = y1;
                        p1 = new Vector32((float)x1, (float)y1, (float)z1);
                        p2 = new Vector32((float)x2, (float)y1, (float)z1);
                        CLine line = new CLine(p1, p2);
                        if( slicer.GetLineIntersection(line,out inters) )                        
                        {
                            for (iz = height1; iz < height2; iz++)
                            {
                                id3 = iz * (int)xyNum + iy * xNum + ix;//x1y1
                                if (pBlankTable[id3]) //0<---inertp-----1
                                    SetBlankValue(id3, inters.X, 0, 2);
                                else //0---inertp---->1
                                    SetBlankValue(id3, inters.X, 0, 1);
                            }
                        }
                    }//if (point_in[id] != point_in[id1]) //x1-x2
                    if (blanked[id] != blanked[id2]) //y1-y2
                    {
                        y2 = y1 + yStep;
                        p1 = new Vector32((float)x1, (float)y1, (float)z1);
                        p2 = new Vector32((float)x1, (float)y2, (float)z1);
                        CLine line = new CLine(p1, p2);
                        //get intersect points
                        if ( slicer.GetLineIntersection(line, out inters) )
                        {   //cut and set intersetions
                            for (iz = height1; iz < height2; iz++)
                            {
                                id3 = iz * (int)xyNum + iy * xNum + ix;//x1y1
                                if (pBlankTable[id3]) //0<---inertp-----1
                                    SetBlankValue(id3, inters.Y, 1, 2);
                                else //0---inertp---->1
                                    SetBlankValue(id3, inters.Y, 1, 1);
                            }
                        }
                    }//if (point_in[id] != point_in[id2]) //y1-y2
                }//for (ix = 0; ix < xNum - 1; ix++)
            }//for (iy = 0; iy < yNum - 1; iy++)
            return true;
        }//bool CutWithXOYSlicer(CSlicer slicer, bool keepleft = true)
        */
        //keepleft = true,
        //if close slicer,keepinside 
        public bool CutWithSurface(CSlicer slicer, int para = 0)
        {
            int n = slicer.pBaseLine.Count;
            if (n < 2) return false;

            if (slicer.Plan == planEnum.XOY)
            {
                if (slicer.Closed)
                    return CutWithClosedXOYSlicer(slicer, para);
                else
                    return CutWithClosedXOYSlicer(slicer, para);
            }
            else if (slicer.Plan == planEnum.YOZ)
            {

            }
            else if (slicer.Plan == planEnum.XOZ)
            {

            }
            return false;
        }
        /*
        public bool CutWithSurface(CSlicer slicer,bool keepleft = true)
        {
            int n = slicer.pBaseLine.Count;
            if (n < 2) return false;
            Vector32 p1 = slicer.pBaseLine[0];
            Vector32 p2 = slicer.pBaseLine[1];
            double x1, y1, z1;
            PointLineRelation relation;
            long id;
            double depth1 = slicer.minHeight;
            double depth2 = slicer.maxHeight;
            if ( slicer.Plan == planEnum.XOY )
            {
                int grid1 = (int)((depth1 - minz) / zStep);
                int grid2 = (int)((depth2 - minz) / zStep);
                if (depth1 == minz) grid1 = 0;
                if (depth2 == maxz) grid2 = zNum - 1;
                z1 = maxz;
                for (int iy = 0; iy < yNum; iy++)
                {
                    y1 = miny + yStep * iy;
                    for (int ix = 0; ix < xNum; ix++)
                    {
                        x1 = minx + xStep * ix;
                        relation = slicer.CheckPointPosition(x1, y1, z1);
                        if ( relation == PointLineRelation.Right && keepleft ||
                             relation == PointLineRelation.Left && !keepleft )
                        {   //blank grid
                            for( int iz = grid1;iz <= grid2; iz++ )
                            {
                                id = GetVerticIndex(ix, iy, iz);
                                pBlankTable[id] = true;
                            }
                        }
                    }//end for(int ix
                }//end for(int iy       
            }//if ( slicer.Plan == planEnum.XOY )

            return false;
        }
        */
        public void OverlapDem(C2DGridData data, bool on_top = true)
        {
            double x, y, z;
            double val;
            double z1 = data.minz;
            double z2 = data.maxz; //最高点
            int znum = 0;
            maxz = z2;
            
            zStep = (maxz - minz) / (zNum-1);

            for( int iy = 0; iy < yNum; iy++ )
            {
                for (int ix = 0; ix < xNum; ix++)
                {
                    x = minx + xStep * ix;
                    y = miny + yStep * iy;
                    z = data.GetGridValue(x, y);
                    znum = (int)((z2 - z) / zStep);
                    if ( znum <= 0 || znum >= zNum ) continue;
                    for(int iz = 0;iz < zNum-znum; iz++ )
                    {
                        val = GetGridValue(ix, iy,iz+znum);
                        SetGridValue(ix, iy, iz,val);
                    }
                }
            }

        }

        public bool CutWithZSurfaceOnUniform(C2DGridData data, bool keepup = false, bool exchangeXY = false, double offset = 0)
        {
            double x0, y0, z0;
            double vx, vy, vz;
            double vz1;
            int ix, iy, iz;
            long id;

            bool[] blanked = new bool[xNum * yNum * zNum];
            for (id = 0; id < xNum * yNum * zNum; id++)
                blanked[id] = false;

            //set z grid first
            for (iy = 0; iy < yNum; iy++)
            {
                y0 = miny + yStep * iy;
                for (ix = 0; ix < xNum; ix++)
                {
                    x0 = minx + xStep * ix;
                    if (exchangeXY) vz = data.GetGridValue(y0, x0) + offset;
                    else vz = data.GetGridValue(x0, y0) + offset;                    
                    //0--1--2--3
                    //iz = (int)((vz - minz + 0.01 * zStep) / zStep);
                    iz = (int)((vz - minz) / zStep);

                    if (keepup) // blank below( 0 - iz )
                    {
                        if (vz < minz || iz < 0) continue;
                        if (iz > zNum - 1) iz = zNum - 1;

                        //2--z轴，sort=2坐标轴方向，保留较高位置的值
                        SetBlankValue(ix, iy, iz, vz, AxisEnum.zAxis, AxisOrderEnum.Upper);
                        for (int i = 0; i <= iz; i++)
                        {
                            id = ix + iy * xNum + i * xyNum;
                            blanked[id] = true;
                            SetBlankGrid(ix, iy, i);
                        }
                    }
                    else // keep lower，blank upper(iz - zNum）
                    {
                        if (iz >= zNum - 1) continue;
                        if (iz < -1) iz = -1;

                        //2--z轴，sort=1坐标轴方向，保留较低位置的值
                        if (iz >= 0) SetBlankValue(ix, iy, iz, vz, AxisEnum.zAxis, AxisOrderEnum.Lower);

                        for (int i = iz + 1; i < zNum; i++)
                        {
                            id = ix + iy * xNum + i * xyNum;
                            blanked[id] = true;
                            SetBlankGrid(ix, iy, i);
                        }
                    }//else keep lower
                }//for (ix = 0; ix < xNum; ix++)
            }//for (iy = 0; iy < yNum; iy++)

            ////calculate  x,y intersections
            ///Y
            ///|b4-----
            ///|      |
            ///O------b1>X
            ///b0

            bool b0, b1, b4;
            long id0, id1, id4;
            double l1, l2;
            for (iy = 0; iy < yNum - 1; iy++)
            {
                y0 = miny + yStep * iy;
                for (ix = 0; ix < xNum - 1; ix++)
                {
                    x0 = minx + xStep * ix;
                    for (iz = 0; iz < zNum - 1; iz++)
                    {
                        id0 = GetVerticIndex(ix, iy, iz);
                        b0 = blanked[id0];

                        // intersection on y axis
                        //y axis edge 04
                        id4 = GetVerticIndex(ix, iy + 1, iz);
                        b4 = blanked[id4];
                        if (b0 != b4) //surface cross it
                        {
                            z0 = minz + iz * zStep;
                            vz = data.GetGridValue(x0, y0);
                            vz1 = data.GetGridValue(x0, y0 + yStep);

                            //vz = data.GetGridValue(ix, iy);
                            //vz1 = data.GetGridValue(ix, iy + 1);

                            l1 = vz - z0;
                            l2 = vz1 - z0;
                            if (l1 < 0) l1 = -l1;
                            if (l2 < 0) l2 = -l2;
                            vy = y0 + yStep * l1 / (l1 + l2);
                            //0----vy---->1
                            if (b0) SetBlankValue(id0, vy, AxisEnum.yAxis, AxisOrderEnum.Upper);
                            else SetBlankValue(id0, vy, AxisEnum.yAxis, AxisOrderEnum.Lower);
                        }

                        // intersection on x axis
                        //x axis edge 01
                        id1 = GetVerticIndex(ix + 1, iy, iz);
                        b1 = blanked[id1];
                        if (b0 != b1) //surface cross it
                        {
                            vz = data.GetGridValue(x0, y0);
                            vz1 = data.GetGridValue(x0 + xStep, y0);
                            //vz = data.GetGridValue(ix, iy);
                            //vz1 = data.GetGridValue(ix + 1, iy);

                            z0 = minz + iz * zStep;
                            l1 = vz - z0;
                            l2 = vz1 - z0;
                            if (l1 < 0) l1 = -l1;
                            if (l2 < 0) l2 = -l2;
                            vx = x0 + xStep * l1 / (l1 + l2);
                            if (b0) SetBlankValue(id0, vx, AxisEnum.xAxis, AxisOrderEnum.Upper);
                            else SetBlankValue(id0, vx, AxisEnum.xAxis, AxisOrderEnum.Lower);
                        }

                    }//for (iz = 0; iz < zNum - 1; iz++)
                }//for (ix = 0; ix < xNum-1; ix++)
            }//for (iy = 0; iy < yNum-1; iy++)

            blanked = null;

            return true;
        }

        public bool CutWithZSurface(C2DGridData data, bool keepup = false,bool exchange=false, double offset = 0)
        {
            return CutWithZSurfaceOnUniform(data, keepup);
        }

        /// <summary>
        /// Cut 3D grid with meshes on Z axis
        /// </summary>
        /// <param name="data">2D grid data</param>
        /// <param name="keepup">keep upper part</param>
        /// <param name="exchangeXY">change x and y</param>        
        /// <param name="zscale"></param>
        /// <returns></returns>
        public bool CutWithZSurfaceOnYAxis(C2DGridData data, bool keepup = false,
                                    bool exchangeXY = false, double offset = 0)
        {
            double x0, y0, z0;
            double vx, vy, vz;
            double vz1;
            int ix, iy, iz,iy0;
            long id;

            bool[] blanked = new bool[xNum * yNum * zNum];
            for (id = 0; id < xNum * yNum * zNum; id++)
                blanked[id] = false;

            Vector32 p,p1,p2;
            
            for (ix = 0; ix < xNum; ix++)
            {
                for (iz = 0; iz < zNum; iz++)
                {
                    p = GetGridCoord(ix, yNum-1, iz);
                    //是否在范围内
                    if (p.X < data.minx||p.X > data.maxx||
                        p.Y < data.miny||p.Y > data.maxy ) continue;
                    
                    //计算高程位置
                    if (exchangeXY) vz = data.GetGridValue(p.Y, p.X) + offset;
                    else vz = data.GetGridValue(p.X, p.Y) + offset;
                    iy0 = -1;
                    for(iy = 0; iy <yNum; iy++ )
                    {
                        p = GetGridCoord(ix, iy, iz);
                        if (p.Z > vz) { iy0 = iy; break; }
                    }

                    if ( iy0 <= 0 ) continue;

                    if ( keepup) // blank below( 0 - iz )
                    {
                        //2--z轴，sort=2坐标轴方向，保留较高位置的值
                        SetBlankValue(ix, iy0, iz, vz, AxisEnum.yAxis, AxisOrderEnum.Upper);
                        for (iy = 0; iy <= iy0; iy++)
                        {                            
                            blanked[GetVerticIndex(ix, iy, iz)] = true;
                            SetBlankGrid(ix, iy, iz);
                        }
                    }
                    else // keep lower，blank upper(iz - zNum）
                    {
                        //2--z轴，sort=1坐标轴方向，保留较低位置的值
                        SetBlankValue(ix, iy0, iz, vz, AxisEnum.yAxis, AxisOrderEnum.Lower);
                        for (iy = iy0; iy < yNum; iy++)
                        {
                            blanked[GetVerticIndex(ix, iy, iz)] = true;
                            SetBlankGrid(ix, iy, iz);
                        }
                    }//else keep lower
                }
            }              

            ////calculate  x,y intersections
            ///Y
            ///|b4-----
            ///|      |
            ///O------b1>X
            ///b0

            bool b0, b1;
            long id0;
            double l1, l2;
            for (iz = 0; iz < zNum - 1; iz++)
            {
                for (ix = 0; ix < xNum - 1; ix++)
                {                    
                    for (iy = 0; iy < yNum - 1; iy++)
                    {
                        id0 = GetVerticIndex(ix, iy, iz);
                        b0 = blanked[id0];
                        
                        // intersection on x axis
                        b1 = blanked[GetVerticIndex(ix + 1, iy, iz)];
                        if ( b0 != b1 ) //surface cross it
                        {
                            p = GetGridCoord(ix, iy, iz);
                            p1 = GetGridCoord(ix+1, iy, iz);
                            vz =  data.GetGridValue(p.X, p.Y);
                            vz1 = data.GetGridValue(p1.X,p1.Y);

                            l1 = vz - p.Z;
                            l2 = vz1 - p.Z;
                            if (l1 < 0) l1 = -l1;
                            if (l2 < 0) l2 = -l2;
                            double xstep = GetXStep(ix, iy, iz);
                            vx = p.X + xstep * l1 / (l1 + l2);
                            //0----vy---->1
                            if (b0) SetBlankValue(id0, vx, AxisEnum.xAxis, AxisOrderEnum.Upper);
                            else SetBlankValue(id0, vx, AxisEnum.xAxis, AxisOrderEnum.Lower);
                        }

                        // intersection on z axis
                        b1 = blanked[GetVerticIndex(ix, iy, iz+1)];
                        if (b0 != b1) //surface cross it
                        {
                            p = GetGridCoord(ix, iy, iz);
                            p1 = GetGridCoord(ix, iy, iz+1);
                            vz = data.GetGridValue(p.X, p.Y);
                            vz1 = data.GetGridValue(p1.X, p1.Y);

                            l1 = vz - p.Z;
                            l2 = vz1 - p.Z;
                            if (l1 < 0) l1 = -l1;
                            if (l2 < 0) l2 = -l2;
                            double zstep = GetZStep(ix, iy, iz);
                            vz = p.Y + zstep * l1 / (l1 + l2);
                            //0----vy---->1
                            if (b0) SetBlankValue(id0, vz, AxisEnum.zAxis, AxisOrderEnum.Upper);
                            else SetBlankValue(id0, vz, AxisEnum.zAxis, AxisOrderEnum.Lower);
                        }

                    }//for (iz = 0; iz < zNum - 1; iz++)
                }//for (ix = 0; ix < xNum-1; ix++)
            }//for (iy = 0; iy < yNum-1; iy++)

            blanked = null;

            return true;
        }

        private void BlankGrid(long id, int side, int methodPara)
        {
            if (side > 0 && methodPara != 0)
                pBlankTable[id] = true;
            else if (side < 0 && methodPara != 1)
                pBlankTable[id] = true;
        }
        //methodPara
        //keep front //0 
        //keep back //1
        //create intersection//2
        public void CutWithMesh(TriangleObj obj, int methodPara)
        {
            TriangleObj obj1 = obj.TrimObject(minx, maxx, miny, maxy, minz, maxz);

            double x, y, z;
            Vector32 p;
            Vector32 p1 = new Vector32();
            Vector32 p2 = new Vector32();
            long id1, id2 = 0, id3, id4;
            ByteXYZ b;
            int ix, iy, iz;
            int side1, side2;
            CTriangle3f tri0, tri1;

            //optimise filtered grid not in mesh range
            List<long> grids = new List<long>();
            List<ByteXYZ> sects = new List<ByteXYZ>();
            double x1, x2, y1, y2, z1, z2;

            //blank grid first
            p = obj1.GetCenter32();
            tri0 = obj1.GetNearestTriangle(p);
            for (iz = 0; iz < zNum; iz++)
            {
                z = minz + zStep * iz;
                for (iy = 0; iy < yNum; iy++)
                {
                    y = miny + yStep * iy;
                    for (ix = 0; ix < xNum; ix++)
                    {
                        x = minx + xStep * ix;
                        p.x = (float)x;
                        p.y = (float)y;
                        p.z = (float)z;
                        id1 = GetVerticIndex(ix, iy, iz);

                        // tri1 = obj1.GetNearestTriangle(p);
                        //side1 = tri0.GetRelationOfPoint(p);
                        // BlankGrid(id1, side1, methodPara);

                        if (!obj1.IsInRange(x, y, z))
                        {
                            side1 = tri0.GetRelationOfPoint(p.toVector64());
                            BlankGrid(id1, side1, methodPara);
                        }
                        else
                        {
                            tri1 = tri0;
                            //tri1 = obj1.GetNearestTriangle(p);
                            side1 = tri1.GetRelationOfPoint(p.toVector64());
                            BlankGrid(id1, side1, methodPara);
                        }

                    }
                }
            }
            return;
        }//end of CutMesh

        //get intersections of each edge, ix,iy,iz,3 edge
        public int curProgressPos = 0;
        public int totalProgressStep = 100;
        public string curProgressTip = "";
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
            //method = 2 Slicer
            //keep left //0 
            //keep right//1
            //keep intersection//2
            if (method == 0)
            {
                if (methodPara == 0)
                    CutWithPolygon(new Polygon3D((TriangleObj)obj), true, null);
                else if (methodPara == 1)
                    CutWithPolygon(new Polygon3D((TriangleObj)obj), false, null);
            }
            else if (method == 1)
            {
                CutWithMesh((TriangleObj)obj, methodPara);
            }
            else if (method == 2)
            {
                CutWithSurface((CSlicer)obj, methodPara);
            }

            UpdateOptimisedShowArray();
        }
        /*
         public bool CutWithPolygon(Polygon3D polygon,bool keepOuter)
         {
             long id=0,id1,id3,id4;
             int ix, iy, iz;

             bool[] point_in = new bool[xNum * yNum * zNum];

             //get points state - outside or inside Polygon
             totalProgressStep = zNum;
             curProgressTip = "calculating points in polygon...";
             Vector32 p = new Vector32();
             for (iz = 0; iz < zNum; iz++)
             {
                 curProgressPos = iz;
                 for (iy = 0; iy < yNum; iy++)
                     for (ix = 0; ix < xNum; ix++)
                     {
                         id = ix + iy * xNum + iz * xyNum;

                         //already blanked
                         //if (pBlankTable[id]) continue;
                         //invisible grid
                         //if (pgridShowTable[id] == 0) continue;

                         p.X = (float)(minx + ix * xStep);
                         p.Y = (float)(miny + iy * yStep);
                         p.Z = (float)(minz + iz * zStep);

                         point_in[id] = polygon.IsPointInPolygon(p);

                         //blank data
                         if ((keepOuter && point_in[id]) ||
                              (!keepOuter && !point_in[id]))
                         {
                             //pGridData[id] = (float)(maxv + 100);
                             pBlankTable[id] = true;
                             pgridShowTable[id] = 0;
                         }
                     }
             }
             //get edge intersections            
             vec3 sect = new vec3();
             bool b0, b1, b3, b4;            
             Vector32 p0,p1, p3, p4;
             totalProgressStep = zNum;
             curProgressTip = "calculating intersections ...";
             for (iz = 0; iz < zNum; iz++)
             {
                 curProgressPos = iz;
                 for (iy = 0; iy < yNum; iy++)
                     for (ix = 0; ix < xNum; ix++)
                     {
                         sect.x = (float)minx - 2;
                         sect.y = (float)miny - 2;
                         sect.z = (float)minz - 2;

                         if (ix < xNum - 1)
                         {
                             id = GetVerticIndex(ix, iy, iz, 0);
                             id1 = GetVerticIndex(ix, iy, iz, 1);
                             b0 = point_in[id];
                             b1 = point_in[id1];
                             if (b0 != b1)    //0-1  x axis
                             {
                                 p0 = GetVerticCoord(ix, iy, iz, 0);
                                 p1 = GetVerticCoord(ix, iy, iz, 1);
                                 if (polygon.GetIntersectionOnAxis(p0, p1, b0, b1, out p, 0))
                                 {
                                     if (pBlankTable[id]) //0<---inertp-----1
                                         SetBlankValue(id, p.X, 0, 2);
                                     else //0---inertp---->1
                                         SetBlankValue(id, p.X, 0, 1);
                                 }//if (polygon.GetIntersectionOnAxis(p0, p1, b0, b1, out p, 0))
                             }// if (b0 != b1)    //0-1  x axis
                         }//if (ix < xNum - 1)
                         if (iy < yNum - 1)
                         {
                             id = GetVerticIndex(ix, iy, iz, 0);
                             id4 = GetVerticIndex(ix, iy, iz, 4);
                             b0 = point_in[id];
                             b4 = point_in[id4];
                             if (b0 != b4)  //0-4  y axis
                             {
                                 p0 = GetVerticCoord(ix, iy, iz, 0);
                                 p4 = GetVerticCoord(ix, iy, iz, 4);
                                 if (polygon.GetIntersectionOnAxis(p0, p4, b0, b4, out p, 1))
                                 {
                                     if (pBlankTable[id])//0<---inertp-----4
                                         SetBlankValue(id, p.Y, 1, 2);
                                     else //0---inertp---->4
                                         SetBlankValue(id, p.Y, 1, 1);
                                 }
                             }
                         }
                         if (iz < zNum - 1)
                         {
                             id = GetVerticIndex(ix, iy, iz, 0);
                             id3 = GetVerticIndex(ix, iy, iz, 3);
                             b0 = point_in[id];
                             b3 = point_in[id3];
                             if (b0 != b3)  //0-3  z axis
                             {
                                 p0 = GetVerticCoord(ix, iy, iz, 0);
                                 p3 = GetVerticCoord(ix, iy, iz, 3);
                                 if (polygon.GetIntersectionOnAxis(p0, p3, b0, b3, out p, 2))
                                 {
                                     if (pBlankTable[id])//0<---inertp-----3
                                         SetBlankValue(id, p.Z, 2, 2);
                                     else //0---inertp---->3
                                         SetBlankValue(id, p.Z, 2, 1);
                                 }
                             }
                         }
                     }
             }
             point_in = null;

             return true;
         }
         */
        public bool CutWithPolygon(Polygon3D polygon, bool keepOuter, bool[] point_in)
        {
            long id = 0, id1, id3, id4;
            int ix, iy, iz;

            if (point_in == null) point_in = new bool[xNum * yNum * zNum];

            for (int i = 0; i < point_in.Length; i++)
            {
                if ((keepOuter && point_in[i]) ||
                     (!keepOuter && !point_in[i]))
                {
                    pBlankTable[i] = true;
                    pgridShowTable[i] = 0;
                }
            }

            //get edge intersections            
            vec3 sect = new vec3();
            bool b0, b1, b3, b4;
            Vector32 p0, p, p1, p3, p4;
            totalProgressStep = zNum;
            curProgressTip = "calculating intersections ...";
            for (iz = 0; iz < zNum; iz++)
            {
                curProgressPos = iz;
                for (iy = 0; iy < yNum; iy++)
                    for (ix = 0; ix < xNum; ix++)
                    {
                        sect.x = (float)minx - 2;
                        sect.y = (float)miny - 2;
                        sect.z = (float)minz - 2;

                        if (ix < xNum - 1)
                        {
                            id = GetVerticIndex(ix, iy, iz, 0);
                            id1 = GetVerticIndex(ix, iy, iz, 1);
                            b0 = point_in[id];
                            b1 = point_in[id1];
                            if (b0 != b1)    //0-1  x axis
                            {
                                p0 = GetVerticCoord(ix, iy, iz, 0);
                                p1 = GetVerticCoord(ix, iy, iz, 1);
                                if (polygon.GetIntersectionOnAxis(p0, p1, b0, b1, out p, 0))
                                {
                                    if (pBlankTable[id]) //0---inertp----->1
                                        SetBlankValue(id, p.X, AxisEnum.xAxis,AxisOrderEnum.Upper);
                                    else //1---inertp----->0
                                        SetBlankValue(id, p.X, AxisEnum.xAxis, AxisOrderEnum.Lower);
                                }//if (polygon.GetIntersectionOnAxis(p0, p1, b0, b1, out p, 0))
                            }// if (b0 != b1)    //0-1  x axis
                        }//if (ix < xNum - 1)
                        if (iy < yNum - 1)
                        {
                            id = GetVerticIndex(ix, iy, iz, 0);
                            id4 = GetVerticIndex(ix, iy, iz, 4);
                            b0 = point_in[id];
                            b4 = point_in[id4];
                            if (b0 != b4)  //0-4  y axis
                            {
                                p0 = GetVerticCoord(ix, iy, iz, 0);
                                p4 = GetVerticCoord(ix, iy, iz, 4);
                                if (polygon.GetIntersectionOnAxis(p0, p4, b0, b4, out p, 1))
                                {
                                    if (pBlankTable[id])//0---inertp---->1(b4)
                                        SetBlankValue(id, p.Y, AxisEnum.yAxis, AxisOrderEnum.Upper);
                                    else //1---inertp---->0(b4)
                                        SetBlankValue(id, p.Y, AxisEnum.yAxis, AxisOrderEnum.Lower);
                                }
                            }
                        }
                        if (iz < zNum - 1)
                        {
                            id = GetVerticIndex(ix, iy, iz, 0);
                            id3 = GetVerticIndex(ix, iy, iz, 3);
                            b0 = point_in[id];
                            b3 = point_in[id3];
                            if (b0 != b3)  //0-3  z axis
                            {
                                p0 = GetVerticCoord(ix, iy, iz, 0);
                                p3 = GetVerticCoord(ix, iy, iz, 3);
                                if (polygon.GetIntersectionOnAxis(p0, p3, b0, b3, out p, 2))
                                {
                                    if (pBlankTable[id])//0---inertp----->1(p3)
                                        SetBlankValue(id, p.Z, AxisEnum.zAxis, AxisOrderEnum.Upper);
                                    else //1---inertp----->0(p3)
                                        SetBlankValue(id, p.Z, AxisEnum.zAxis, AxisOrderEnum.Lower);
                                }
                            }
                        }
                    }
            }
            point_in = null;

            return true;
        }
        public double Interpolate(int ix, int iy, int iz, int min_point_no)
        {
            CInversePower ip = new CInversePower();
            int i, j, k, id;
            int rx = 2, ry = 2, rz = 2;
            double x, y, z, v;
            for (int r = 1; r < zNum; r++)
            {
                rx = ry = rz = r;
                for (k = iz - rz; k < iz + rz; k += 2 * rz)
                {
                    if (k < 0 || k >= zNum) continue;
                    z = minz + k * zStep;
                    for (j = iy - ry; j < iy + ry; j += 2 * ry)
                    {
                        y = miny + j * yStep;
                        if (j < 0 || j >= yNum) continue;
                        for (i = ix - rx; i < ix + rx; i += 2 * rx)
                        {
                            if (i < 0 || i >= xNum) continue;
                            x = minx + i * xStep;
                            id = i + j * xNum + k * xNum * yNum;
                            v = pGridData[id];

                            if (IsBlankValue(v)) continue;
                            ip.AddPoint(x, y, z, v);
                            if (r >= 2)
                            {
                                if (ip.pScatterPoint.Count >= min_point_no)
                                {
                                    v = ip.GetInterValue(x, y, z);
                                    ip.pScatterPoint.Clear();
                                    return v;
                                }
                            }
                        }
                    }
                }
            }//for (int r = 1; r < zNum; r++)
            return 0;
        }       
    
        public override void Clear()
        {
            base.Clear();
            pShowIndexArray.Clear();
            pTriangleArray.Clear();
            pGridData = null;            
            pgridShowTable = null;
            pColorIndexTable = null;
            ColorScale = new CColorScale();           
        }
        public void Init()
        {
            xNum = yNum = zNum = 0;
            minx = miny = minz = minv = 0.0;
            maxx = maxy = maxz = maxv = 0.0;
            pGridData = null;
            pgridShowTable = null;
            pColorIndexTable = null;
            enableOverlap = false;            
            type = ShapeEnum.Grid3D;
            pShowIndexArray = new List<UInt32XYZ>();
        }
        public C3DGridData(double _minx, double _maxx,
                           double _miny, double _maxy,
                           double _minz, double _maxz,
                           double _minv, double _maxv)
        {
            Init();
            minx = _minx;
            miny = _miny;
            minz = _minz;
            minv = _minv;
            maxx = _maxx;
            maxy = _maxy;
            maxz = _maxz;
            maxv = _maxv;
        }
        public C3DGridData()
        {
            Init();
        }
        public C3DGridData(int nx, int ny, int nz, float initval = float.NaN)
        {
            Init();
            xNum = nx;
            yNum = ny;
            zNum = nz;
            xyNum = xNum * yNum;
            pGridData = new float[nx * ny * nz];
            for( int i = 0;i<pGridData.Length;i++)
            {
                pGridData[i] = initval;
            }
        }
        public void ClearShowBuffer()
        {
            pShowIndexArray.Clear();
            for (int i = 0; i < xNum * yNum * zNum; i++)
                pgridShowTable[i] = 0;
        }
        // 判断指定网格单元是否需要显示
        public bool GetShowState(int index)
        {
            if (pgridShowTable[index] == 0) return false;
            else return true;
        }
        public bool GetShowState(long index)
        {
            if (pgridShowTable[index] == 0) return false;
            else return true;
        }
        public bool GetShowState(int ix, int iy, int iz)
        {
            long index = ix + iy * xNum + iz * xNum * yNum;
            if (pgridShowTable[index] == 0) return false;
            else return true;
        }
        public bool GetShowState(UInt32 ix, UInt32 iy, UInt32 iz)
        {
            long index = ix + iy * xNum + iz * xNum * yNum;
            if (pgridShowTable[index] == 0) return false;
            else return true;
        }
        public bool IsNeedShow(UInt32XYZ p)
        {
            return IsNeedShow(p.x, p.y, p.z);
        }
        // 判断指定网格单元是否需要显示
        public bool IsNeedShow(UInt32 ix, UInt32 iy, UInt32 iz)
        {
            if (pgridShowTable == null) return false;

            if (ix >= xNum || iy >= yNum || iz >= zNum) return false;
            if (ix < 0 || iy < 0 || iz < 0) return false;

            long index = ix + iy * xNum + iz * xNum * yNum;
            if (pgridShowTable[index] == 0) return false;

            if (pBlankTable[index]) return false;   //blanked
            if (IsBlankValue(pGridData[index])) return false; //blanked value

            // if grid state == 1, and if enclosed by sorroundings 
            // and it needn't to be show
            if (ix == 0 || ix == xNum - 1 ||
                iy == 0 || iy == yNum - 1 ||
                iz == 0 || iz == zNum - 1) return true;

            if (pgridShowTable[index - 1] == 1 && pgridShowTable[index + 1] == 1 &&
                pgridShowTable[index - xNum] == 1 && pgridShowTable[index + xNum] == 1 &&
                pgridShowTable[index - xNum * yNum] == 1 && pgridShowTable[index + xNum * yNum] == 1)
                return false;

            return true;
        }
        public ColorRGBA GetColor(Int32XYZ xyz)
        {
            int ix = xyz.x;
            int iy = xyz.y;
            int iz = xyz.z;
            if ((ix < 0 || ix >= xNum) || (iy < 0 || iy >= yNum) || (iz < 0 || iz >= zNum))
                return new ColorRGBA(0, 0, 0);
            long id = xyz.x + xyz.y * xNum + xyz.z * xNum * yNum;
            return GetColor(id);
        }
        public ColorRGBA GetColor(double v)
        {
            if (ShowAsStratum) ColorScale.IsSmooth = false;
            return new ColorRGBA(ColorScale.GetColor(v));
        }
        public int GetColorIndex(double v)
        {
            if (ShowAsStratum) ColorScale.IsSmooth = false;
            return ColorScale.GetColorIndex(v);
        }
        public int GetColorIndex(Int32XYZ xyz)
        {
            int ix = xyz.x;
            int iy = xyz.y;
            int iz = xyz.z;
            if ((ix < 0 || ix >= xNum) || (iy < 0 || iy >= yNum) || (iz < 0 || iz >= zNum))
                return 0;
            long id = xyz.x + xyz.y * xNum + xyz.z * xNum * yNum;
            return pColorIndexTable[id];
        }
        public ColorRGBA GetColor(UInt32XYZ xyz,bool smooth = true)
        {
            uint ix = xyz.x;
            uint iy = xyz.y;
            uint iz = xyz.z;
            if ((ix < 0 || ix >= xNum) || (iy < 0 || iy >= yNum) || (iz < 0 || iz >= zNum))
                return new ColorRGBA(0, 0, 0);
            long id = xyz.x + xyz.y * xNum + xyz.z * xNum * yNum;
            return GetColor(id, smooth);
        }
        public ColorRGBA GetColor(long id,bool smooth = true)
        {
            if ( smooth )
            {
                return new ColorRGBA(ColorScale.GetColor(pGridData[id]));
            }
            else
            {
                Int16 nc = pColorIndexTable[id];
                return new ColorRGBA(ColorScale.GetColor(nc));
            }            
        }
        public void CreateColorIndexTable()
        {
            if (xNum > 0 && yNum > 0 && zNum > 0)
            {
                pColorIndexTable = new Int16[xNum * yNum * zNum];
                for (int i = 0; i < xNum * yNum * zNum; i++)
                {
                    if (IsBlankValue(pGridData[i])) pColorIndexTable[i] = 0;
                    else pColorIndexTable[i] = (Int16)GetColorIndex(pGridData[i]);
                }
            }
        }
        public double NormalizeValue(double v)
        {
            v = (v - minv) / (maxv - v);
            if (v < 0) v = 0;
            if (v > 1) v = 1;
            return v;
        }
      
        public bool IsZero(double v)
        {
            double v1 = v;
            if (v < 0) v1 = -v;
            double len = maxx - minx + maxy - miny + maxz - minz;
            if (v < len * 0.000001) return true;
            else return false;
        }
        public CubePosition GetPosition(double x, double y, double z)
        {
            //     
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c

            if (!IsInRange(x, y, z)) return CubePosition.OutSideCube;
            int ix1 = (int)((x - minx + 0.01 * xStep) / xStep);
            int iy1 = (int)((y - miny + 0.01 * yStep) / yStep);
            int iz1 = (int)((z - minz + 0.01 * zStep) / zStep);
            double xs = x - (minx + ix1 * xStep);
            double ys = y - (miny + iy1 * yStep);
            double zs = z - (minz + iz1 * zStep);
            int ix2 = ix1 + 1;
            int iy2 = iy1 + 1;
            int iz2 = iz1 + 1;
            int same = 0;
            if (IsZero(xs)) { ix2 = ix1; same++; }
            if (IsZero(ys)) { iy2 = iy1; same++; }
            if (IsZero(zs)) { iz2 = iz1; same++; }

            if (same == 0) return CubePosition.InSideCube;
            else if (same == 1) //on face
            {
                if (ix1 == ix2) return CubePosition.OnXFace;
                else if (iy1 == iy2) return CubePosition.OnYFace;
                else return CubePosition.OnZFace;
            }
            else if (same == 2) //on Edge            
                return CubePosition.OnEdge;
            else return CubePosition.OnNode;//on Node
        }
        public bool IsInBlankArea(double x, double y, double z)
        {
            //out of ranges
            if (!IsInRange(x, y, z)) return true;
            //     
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c
            int ix1 = (int)((x - minx + 0.01 * xStep) / xStep);
            int iy1 = (int)((y - miny + 0.01 * yStep) / yStep);
            int iz1 = (int)((z - minz + 0.01 * zStep) / zStep);
            int ix2 = ix1 + 1;
            int iy2 = iy1 + 1;
            int iz2 = iz1 + 1;
            List<long> indexs = new List<long>();
            long id = GetVerticIndex(ix1, iy1, iz1);
            indexs.Add(id);
            CubePosition pos = GetPosition(x, y, z);
            switch (pos)
            {
                case CubePosition.InSideCube:
                    id = GetVerticIndex(ix1, iy2, iz1);
                    indexs.Add(id);
                    id = GetVerticIndex(ix2, iy2, iz1);
                    indexs.Add(id);
                    id = GetVerticIndex(ix2, iy1, iz1);
                    indexs.Add(id);
                    id = GetVerticIndex(ix1, iy1, iz2);
                    indexs.Add(id);
                    id = GetVerticIndex(ix1, iy2, iz2);
                    indexs.Add(id);
                    id = GetVerticIndex(ix2, iy2, iz2);
                    indexs.Add(id);
                    id = GetVerticIndex(ix2, iy1, iz2);
                    indexs.Add(id);
                    break;
                case CubePosition.OnXFace:
                    id = GetVerticIndex(ix1, iy2, iz1);
                    indexs.Add(id);
                    id = GetVerticIndex(ix1, iy2, iz2);
                    indexs.Add(id);
                    id = GetVerticIndex(ix1, iy1, iz2);
                    indexs.Add(id);
                    break;
                case CubePosition.OnYFace:
                    id = GetVerticIndex(ix2, iy1, iz1);
                    indexs.Add(id);
                    id = GetVerticIndex(ix2, iy1, iz2);
                    indexs.Add(id);
                    id = GetVerticIndex(ix1, iy1, iz2);
                    indexs.Add(id);
                    break;
                case CubePosition.OnZFace:
                    id = GetVerticIndex(ix1, iy2, iz1);
                    indexs.Add(id);
                    id = GetVerticIndex(ix2, iy2, iz1);
                    indexs.Add(id);
                    id = GetVerticIndex(ix2, iy1, iz1);
                    indexs.Add(id);
                    break;
            }
            bool ret = false;
            for (int i = 0; i < indexs.Count; i++)
            {
                if (IsBlankedGrid(indexs[i]))
                {
                    ret = true;
                    break;
                }
            }
            indexs.Clear();
            return ret;
        }        
        public double GetGridValue(long id)
        {
            return pGridData[id];
        }
        public double GetGridValue(int ix, int iy, int iz)
        {
            return GetGridValue(GetVerticIndex(ix, iy, iz));
        }
        public void SetGridValue(int ix, int iy, int iz,double val)
        {
            pGridData[GetVerticIndex(ix, iy, iz)] = (float)val;
        }

        public double GetGridValue( double x, double y, double z, 
                                    bool interpolation = true, 
                                    bool normalized = false)
        {
            //check is in blank area
            //if ( IsInBlankArea(x, y, z) ) return m_blankvalue;

            double v = 0;
            if ( !interpolation ) v = GetGridValueWithOutInterpolation(x, y, z);
            else v = GetGridValueWithInterpolation(x, y, z, 0);
            
            if( v < minv ) v = minv;
            if( v > maxv ) v = maxv;

            if (normalized)
            {
                v = NormalizeValue(v);
            }
            return v;
        }
        public bool IsGridIndexValid(int ix,int iy,int iz)
        {
            if(ix <0 || iy <0 || iz <0)return false;
            if(ix >= xNum ||  iy >= yNum || iz >= zNum) return false;
            return true;
        }
        public void GeometryLimited(ref int ix, ref int iy, ref int iz)
        {
            if (ix < 0) ix = 0;
            if (iy < 0) iy = 0;
            if (iz < 0) iz = 0;
            if (ix >= xNum ) ix = xNum - 1;
            if (iy >= yNum ) iy = yNum - 1;
            if (iz >= zNum ) iz = zNum - 1;
        }
        public int GeometryLimited(int id,AxisEnum axis)
        {
            if ( id < 0 ) return 0;

            if (axis == AxisEnum.xAxis)
            { 
                if( id >= xNum ) return xNum-1;
            }
            else if (axis == AxisEnum.yAxis)
            {
                if (id >= yNum) return yNum - 1;
            }
            else //if (axis == AxisEnum.zAxis)
            {
                if (id >= zNum) return zNum - 1;
            }
            return id;
        }
        public bool IsGridValid(int ix,int iy,int iz)
        {
            if (ix < 0 || iy < 0 || iz < 0) return false;
            if (ix >= xNum || iy >= yNum || iz >= zNum) return false;
            return true;
        }
        public bool IsGridValid(long id)
        {
            if (id < 0 || id >= xNum*yNum*zNum) return false;            
            return true;
        }
        /// <summary>
        /// 返回节点最近的有效值
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <returns></returns>
        public double GetNearestValue(int ix,int iy,int iz)
        {
            int nn = xNum;
            if (yNum > nn) nn = yNum;
            if (zNum > nn) nn = zNum;
            double v1;
            int ix1, ix2, iy1, iy2, iz1, iz2;
            nn = 10;
            for (int w = 1; w <= nn; w++)
            {   
                ix1 = ix - w;
                ix2 = ix + w;
                iy1 = iy - w;
                iy2 = iy + w;
                iz1 = iz - w;
                iz2 = iz + w;
                
                if ( IsGridValid(ix1,iy,iz) )
                {
                    v1 = GetGridValue(ix1, iy, iz);
                    if ( !IsBlankValue(v1) ) return v1;
                }
                if (IsGridValid(ix2, iy, iz))
                {
                    v1 = GetGridValue(ix2, iy, iz);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix, iy1, iz))
                {
                    v1 = GetGridValue(ix, iy1, iz);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix, iy2, iz))
                {
                    v1 = GetGridValue(ix, iy2, iz);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix, iy, iz1))
                {
                    v1 = GetGridValue(ix, iy, iz1);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix, iy, iz2))
                {
                    v1 = GetGridValue(ix, iy, iz2);
                    if (!IsBlankValue(v1)) return v1;
                }
                ////////////////////////
                if (IsGridValid(ix1, iy1, iz1))
                {
                    v1 = GetGridValue(ix1, iy1, iz1);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix2, iy1, iz1))
                {
                    v1 = GetGridValue(ix2, iy1, iz1);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix1, iy2, iz1))
                {
                    v1 = GetGridValue(ix1, iy2, iz1);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix2, iy2, iz1))
                {
                    v1 = GetGridValue(ix2, iy2, iz1);
                    if (!IsBlankValue(v1)) return v1;
                }
                //////////////////////////////////
                if (IsGridValid(ix1, iy1, iz2))
                {
                    v1 = GetGridValue(ix1, iy1, iz2);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix2, iy1, iz2))
                {
                    v1 = GetGridValue(ix2, iy1, iz2);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix1, iy2, iz2))
                {
                    v1 = GetGridValue(ix1, iy2, iz2);
                    if (!IsBlankValue(v1)) return v1;
                }
                if (IsGridValid(ix2, iy2, iz2))
                {
                    v1 = GetGridValue(ix2, iy2, iz2);
                    if (!IsBlankValue(v1)) return v1;
                }
            }
            return m_blankvalue;
        }

        /// <summary>
        /// 检查网格是否联通到边界
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <param name="bkvalue">背景值</param>
        /// <returns></returns>
        public bool IsGridConnectEdge(int ix, int iy, int iz, double bkvalue = 0)
        {
            bool edge = true;
            for (int iz2 = iz + 1; iz2 < zNum; iz2++)
            {
                if( GetGridValue(ix,iy,iz2) != bkvalue )
                {
                    edge = false;
                    break;
                }
            }
            if (edge) return true;
            
            for (int iz1 = iz - 1; iz1 >= 0; iz1--)
            {
                if (GetGridValue(ix, iy, iz1) != bkvalue)
                {
                    edge = false;
                    break;
                }
            }
            if (edge) return true;

            //
            for (int iy1 = iy - 1; iy1 >= 0; iy1--)
            {
                if (GetGridValue(ix, iy1, iz) != bkvalue)
                {
                    edge = false;
                    break;
                }
            }
            if (edge) return true;
            for (int iy2 = iy + 1; iy2 < yNum; iy2++)
            {
                if (GetGridValue(ix, iy2, iz) != bkvalue)
                {
                    edge = false;
                    break;
                }
            }
            if (edge) return true;

            //x
            for (int ix1 = ix - 1; ix1 >= 0; ix1--)
            {
                if (GetGridValue(ix1, iy, iz) != bkvalue)
                {
                    edge = false;
                    break;
                }
            }
            if (edge) return true;
            for (int ix2 = ix + 1; ix2 < xNum; ix2++)
            {
                if (GetGridValue(ix2, iy, iz) != bkvalue)
                {
                    edge = false;
                    break;
                }
            }
            return edge;
        }

        struct SearchBoundBox 
        {
            public int X1;
            public int Y1;
            public int Z1;
            public int X2;
            public int Y2;
            public int Z2;
            public SearchBoundBox(int x1,int y1,int z1,int x2,int y2,int z2)
            {
                X1 = x1;Y1 = y1; Z1 = z1;
                X2 = x2;Y2 = y2; Z2 = z2;
            }
            public bool IsValid()
            {
                if (X1 >= X2 || Y1 >= Y2 || Z1 >= Z2) return false;
                else return true;
            }
        }
        void DoSearchOnBoundBox( SearchBoundBox box, ref List<Vector32>points,float invalid_value)
        {
            float val;
            double x, y, z;
            for (int iz = box.Z1; iz < box.Z2; iz++)
            {
                z = minz + iz * zStep;
                for (int iy = box.Y1; iy < box.Y2; iy++)
                {
                    y = miny + iy * yStep;
                    for (int ix = box.X1; ix < box.X2; ix++)
                    {
                        x = minx + ix * xStep;
                        val = this[ix, iy, iz];
                        if (val <= 0) continue;
                        if (val == invalid_value) continue;
                        if (IsBlankValue(val)) continue;
                        points.Add(new Vector32(x, y, z, val));
                    }//for (int ix = x1; ix < x2; ix++)
                }//for (int iy = y1; iy < y2; iy++
            }//for (int iz = z1; iz < z2; iz++)
        }
        public List<Vector32> SearchNearestPoints(int ix0,int iy0,int iz0,float invalid_value)
        {
            int rad, xrad,yrad,zrad;
            int x1, x2, y1, y2, z1, z2;
            int ox1=-1, ox2=-1, oy1=-1, oy2=-1, oz1=-1, oz2=-1;
            int max_rad = xNum;
            if (yNum > max_rad) max_rad = yNum;
            if (zNum > max_rad) max_rad = zNum;
            
            List<Vector32>points = new List<Vector32>();
            List<SearchBoundBox>boxes = new List<SearchBoundBox>();
            SearchBoundBox box;
            //最大步长
            double xx = xStep;
            if (yStep > xx) xx = yStep;
            if (zStep > xx) xx = zStep;            
           // for (rad = 1; rad < max_rad; rad++)
            {
                rad = max_rad / 5;
                xrad = (int)(xx / xStep * rad + 0.5);
                yrad = (int)(xx / yStep * rad + 0.5);
                zrad = (int)(xx / zStep * rad + 0.5);
                x1 = ix0 - xrad; x2 = ix0 + xrad;
                y1 = iy0 - yrad; y2 = iy0 + yrad;
                z1 = iz0 - zrad; z2 = iz0 + zrad;
                GeometryLimited(ref x1, ref y1, ref z1);
                GeometryLimited(ref x2, ref y2, ref z2);
                DoSearchOnBoundBox(new SearchBoundBox(x1, y1, z1, x2, y2, z2), ref points, invalid_value);
                //if (rad == 1)
                //{
                //    boxes.Add(new SearchBoundBox(x1, y1, z1, x2, y2, z2));
                //}
                //else
                //{
                //    box = new SearchBoundBox(x1, y1, z1, x2, y2, oz1);
                //    if (box.IsValid()) boxes.Add(box);
                //    box = new SearchBoundBox(x1, y1, oz2, x2, y2, z2);
                //    if (box.IsValid()) boxes.Add(box);
                //    box = new SearchBoundBox(x1, y1, oz1, x2, oy1, oz2);
                //    if (box.IsValid()) boxes.Add(box);
                //    box = new SearchBoundBox(x1, oy2, oz1, x2, y2, oz2);
                //    if (box.IsValid()) boxes.Add(box);
                //    box = new SearchBoundBox(x1, oy1, oz1, ox1, oy2, oz2);
                //    if (box.IsValid()) boxes.Add(box);
                //    box = new SearchBoundBox(ox2, oy1, oz1, x2, oy2, oz2);
                //    if (box.IsValid()) boxes.Add(box);
                //}
                //foreach (SearchBoundBox b in boxes)
                //{
                //    DoSearchOnBoundBox(b, ref points, invalid_value);
                //}
                //boxes.Clear();
                if (points.Count > 1) return points;
                ox1 = x1; ox2 = x2;
                oy1 = y1; oy2 = y2;
                oz1 = z1; oz2 = z2;
            }
            return points;
        }
        /// <summary>
        /// 查找周围的值，有效且非背景值
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SearchNearestValue(int ix, int iy, int iz,out double val, double bkvalue = 0)
        {
            val = 0;
            int nn = xNum;
            if (yNum > nn) nn = yNum;
            if (zNum > nn) nn = zNum;
            int ix1, ix2, iy1, iy2, iz1, iz2;
            for (int w = 1; w <= nn; w++)
            {
                //网格已经联通到边界
                if (IsGridConnectEdge(ix, iy, iz, bkvalue)) return false;

                ix1 = ix - w;
                ix2 = ix + w;
                iy1 = iy - w;
                iy2 = iy + w;
                iz1 = iz - w;
                iz2 = iz + w;

                if (!IsGridValid(ix1, iy, iz)) return false;
                if (!IsGridValid(ix, iy1, iz)) return false;
                if (!IsGridValid(ix, iy, iz1)) return false;
                if (!IsGridValid(ix2, iy, iz)) return false;
                if (!IsGridValid(ix, iy2, iz)) return false;
                if (!IsGridValid(ix, iy, iz2)) return false;
                if (!IsGridValid(ix1, iy1, iz1)) return false;
                if (!IsGridValid(ix2, iy1, iz1)) return false;
                if (!IsGridValid(ix1, iy2, iz1)) return false;
                if (!IsGridValid(ix2, iy2, iz1)) return false;
                if (!IsGridValid(ix1, iy1, iz2)) return false;
                if (!IsGridValid(ix2, iy1, iz2)) return false;
                if (!IsGridValid(ix1, iy2, iz2)) return false;
                if (!IsGridValid(ix2, iy2, iz2)) return false;

                val = GetGridValue(ix1, iy, iz);
                if (val != bkvalue && !IsBlankValue(val) ) return true;
                val = GetGridValue(ix, iy1, iz);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix, iy, iz1);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix2, iy, iz);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix, iy2, iz);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix, iy, iz2);
                if (val != bkvalue && !IsBlankValue(val)) return true;

                val = GetGridValue(ix1, iy1, iz1);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix2, iy1, iz1);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix1, iy2, iz1);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix2, iy2, iz1);
                if (val != bkvalue && !IsBlankValue(val)) return true;

                val = GetGridValue(ix1, iy1, iz2);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix2, iy1, iz2);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix1, iy2, iz2);
                if (val != bkvalue && !IsBlankValue(val)) return true;
                val = GetGridValue(ix2, iy2, iz2);
                if (val != bkvalue && !IsBlankValue(val)) return true;
            }
            return false;
        }
        //scale 0 - 1
        public double GetGridValueWithOutInterpolation(double x, double y, double z)
        {
            int ix = (int)((x - minx) / xStep);
            int iy = (int)((y - miny) / yStep);
            int iz = (int)((z - minz) / zStep);
            GeometryLimited(ref ix, ref iy, ref iz);
            return GetGridValue(ix, iy, iz);
        }       

        /// <summary>
        /// 从网格中获取任意点的值-有问题需要优化？？2024-3-29
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="err">是否节点位置误差判断%步长</param>
        /// <returns></returns>
        public double GetGridValueWithInterpolation(double x, double y, double z, int searchingGridLength = 0)
        {
            if ( x < minx || y < miny || z < minz || 
                 x > maxx || y > maxy || z > maxz ) 
                return float.NaN;
            
            Vector32 p = new Vector32(x,y,z);
            int ix0 = (int)((x - minx) / xStep);
            int iy0 = (int)((y - miny) / yStep);
            int iz0 = (int)((z - minz) / zStep);
            int sn = searchingGridLength;
            InterpolatorBase ip = null;
            if (sn >= 1)
            {
                ip = new RBFInterpolationGlobal();
                for (int iz = iz0 - sn; iz <= iz0 + sn && iz >= 0 && iz < zNum; iz++)
                {
                    for (int iy = iy0 - sn; iy <= iy0 + sn && iy >= 0 && iy < yNum; iy++)
                    {
                        for (int ix = ix0 - sn; ix <= ix0 + sn && ix >= 0 && ix < xNum; ix++)
                        {
                            p = GetGridCoord(ix, iy, iz);
                            if (!IsBlanked(p.V)) ip.AddPoint(p);
                        }
                    }
                }
            }
            else 
            {
                ip = new IdwInterpolatorGlobal();
                for (int i = 0; i < 8; i++)
                {
                    p = GetVerticCoord(ix0, iy0, iz0, i);
                    if (!IsBlanked(p.V)) ip.AddPoint(p);
                }
            }

            if (ip.pointCount < 1)return float.NaN;
            double val = ip.GetInterpolatedValue(x, y, z);
            ip.Clear();
            return val;           
        }
        
        public bool InitTables()
        {
            long all = xNum * yNum * zNum;            
            pgridShowTable = new byte[all];
            for (long i = 0; i < all; i++)
            {
                if (IsBlankValue(pGridData[i]))
                    pgridShowTable[i] = 0;
                else pgridShowTable[i] = 1;
            }

            //create defaut color scale
            if( ColorScale.Levels.Count < 1 )
            ColorScale = new CColorScale(minv, maxv);
            
            InitBlankTable();

            //create Color index table
            CreateColorIndexTable();
            UpdateColorIndexTable();
            UpdateShowTableFromColorScale();
            UpdateTriangleFromShowArray();

            return true;
        }
        public void ResetColorScale(CColorScale scale)
        {
            scale.SetVisibleFrom(ColorScale);
            ColorScale = scale;            
            UpdateColorIndexTable();
        }        
        public void Create(int nx, int ny, int nz, double x1, double y1, double z1, double x2, double y2, double z2, double v1, double v2)
        {
            xNum = nx;
            yNum = ny;
            zNum = nz;
            minx = x1;
            maxx = x2;
            miny = y1;
            maxy = y2;
            minz = z1;
            maxz = z2;
            minv = v1;
            maxv = v2;
            xStep = (maxx - minx) / (xNum - 1);
            yStep = (maxy - miny) / (yNum - 1);
            zStep = (maxz - minz) / (zNum - 1);
            xyNum = xNum * yNum;
        }

        bool SaveVersion10(BinaryWriter br)
        {
            float v;
            for (long i = 0; i < zNum * xyNum; i++)
            {
                v = pGridData[i];
                if (IsBlankedGrid(i)) v = float.NaN;
                br.Write(v);
            }
            return true;
        }
                
        bool LoadVersion10BySampling(BinaryReader br, double scale, int nx, int ny, int nz)
        {
            //read data from stream
            if (pGridData == null) pGridData = new float[nx * ny * nz];            
            for (long i = 0; i < pGridData.Length; i++) pGridData[i] = float.NaN;
            long id;
            int ix, iy, iz;            
            for (long i = 0;i<xyzNum;i++)
            {
                float v = br.ReadSingle();
                Int32XYZ xyz = GetIndices(i);
                ix = (int)(xyz.x / scale);
                iy = (int)(xyz.y / scale);
                iz = (int)(xyz.z / scale);
                if (ix >= nx) ix = nx - 1;
                if (iy >= ny) iy = ny - 1;
                if (iz >= nz) iz = nz - 1;
                id = ix + iy * nx + iz * nx * ny;
                if ( float.IsNaN(pGridData[id]) ) pGridData[id] = v;
            }
            return true;
        }
        public static byte[] FloatArrayToByteArray(float[] floatArr)
        {
            int intSize = sizeof(float) * floatArr.Length;
            byte[] bytArr = new byte[intSize];
            //申请一块非托管内存
            IntPtr ptr = Marshal.AllocHGlobal(intSize);
            //复制int数组到该内存块
            Marshal.Copy(floatArr, 0, ptr, floatArr.Length);
            //复制回byte数组
            Marshal.Copy(ptr, bytArr, 0, bytArr.Length);
            //释放申请的非托管内存
            Marshal.FreeHGlobal(ptr);
            return bytArr;
        }
        public float[] ByteArrayToFloatArray(byte[] source)
        {
            int len = source.Length / sizeof(float);
            float[] dest = new float[len];
            IntPtr srcArrayPtr = Marshal.UnsafeAddrOfPinnedArrayElement(source, 0);
            Marshal.Copy(srcArrayPtr, dest, 0, dest.Length);
            return dest;
            //int len = source.Length / sizeof(float);
            //float []dest = new float[len];
            //for(int i=0;i<len;i++) 
            //{
            //    dest[i] = BitConverter.ToSingle(source,4*i);
            //}
            //return dest;
        }
        bool LoadVersion10(BinaryReader br)
        {
            //read data from stream
            //if (pGridData == null) pGridData = new float[xNum*yNum*zNum];
            //for (long i = 0; i < zNum * xyNum; i++)
            //{ 
            //    pGridData[i] = br.ReadSingle(); 
            //    if( pGridData[i]==float.MaxValue ||
            //        pGridData[i] == m_blankvalue ||
            //        float.IsInfinity(pGridData[i]) )
            //    {
            //        pGridData[i] = float.NaN;
            //    }
            //}
            pGridData = null;
            byte[] bytes = br.ReadBytes(xNum * yNum * zNum * sizeof(float));
            pGridData = ByteArrayToFloatArray(bytes);
            bytes = null;
            //UpdateRange();
            return true;
        }

        bool SaveVersion11(BinaryWriter br)
        {
            //read ColorScale
            ColorScale.WriteBinary(br);

            //version 1.1 ,end is blanked table and edge intersections
            int n1 = 0;
            if (pBlankTable != null) n1 = pBlankTable.Length;
            br.Write(n1);
            for (int i = 0; i < n1; i++) br.Write(pBlankTable[i]);

            //load pBlankedPointIndexes
            int n2 = 0;
            if ( pBlankedPointIndexes != null ) n2 = pBlankedPointIndexes.Length;
            br.Write(n2);
            for ( int i = 0; i < n2; i++ ) br.Write(pBlankedPointIndexes[i]);

            //load pBlankedPoints
            br.Write(pBlankedPoints.Count);
            foreach(vec3 p in pBlankedPoints)
            {
                br.Write(p.x);
                br.Write(p.y);
                br.Write(p.z);
            }
            
            return true;
        }
        //flag == "DSGE"
        bool LoadVersion11(BinaryReader br)
        {
            //read ColorScale
            ColorScale.LoadBinary(br);

            //version 1.1 ,end is blanked table and edge intersections
            int n1 = br.ReadInt32();
            if (n1 > 0 ) //load pBlankTable[]
            {
                byte[] bytes = br.ReadBytes((int)xyNum * zNum);
                for (long i = 0; i < zNum * xyNum; i++)
                {
                    if (bytes[i] > 0) pBlankTable[i] = true;
                    else pBlankTable[i] = false;
                }
                bytes = null;
            }

            //load pBlankedPointIndexes
            int n2 = br.ReadInt32();
            if (n2 > 0 )
            {
                pBlankedPointIndexes = new int[zNum * xyNum];
                for (long i = 0; i < zNum * xyNum; i++)
                    pBlankedPointIndexes[i] = br.ReadInt32();
            }

            //load pBlankedPoints
            pBlankedPoints.Clear();
            int n3 = br.ReadInt32();
            if ( n3 > 0 )
            {
                vec3 p = new vec3();
                for (int i = 0; i < n3; i++)
                {
                    p.x = br.ReadSingle();
                    p.y = br.ReadSingle();
                    p.z = br.ReadSingle();
                    pBlankedPoints.Add(p);
                }               
            }
            
            return true;
        }
        bool SaveVersion12(BinaryWriter br)//flag == "DSGF"
        {
            //pColorIndexTable
            int n = 0;
            if (pColorIndexTable != null) n = pColorIndexTable.Length;
            br.Write(n);
            for (int i = 0; i < n; i++) br.Write(pColorIndexTable[i]);

            //pgridShowTable
            n = 0;
            if ( pgridShowTable != null ) n = pgridShowTable.Length;
            br.Write(n);
            for (int i = 0; i < n; i++) br.Write(pgridShowTable[i]);

            //pShowIndexArray           
            br.Write(pShowIndexArray.Count);
            foreach( UInt32XYZ p in pShowIndexArray )
            {
                br.Write(p.x);
                br.Write(p.y);
                br.Write(p.z);
            }
            //render method
            br.Write((int)meshMethod);

            bool ret = true;
            //load Matching Cubes
            if (meshMethod == MCMeshMethod.MC)
            {
                ret = m_MarchCube.SaveBinary(br);
            }
            if (meshMethod == MCMeshMethod.ImprovedMC)
            {
                ret = m_MarchCubeExt.SaveBinary(br);
            }
            //save overlay info 12.5
            br.Write(enableOverlap);
            br.Write(overlaps.Count);
            for (int i = 0; i < overlaps.Count; i++)
            {
                br.Write((int)overlaps[i].channel);
                if (!overlaps[i].Save(br)) return false;
            }
            return ret;
        }
        bool LoadVersion12(BinaryReader br)//flag == "DSGF"
        {            
            // if (!InitTables()) return false;
            //pColorIndexTable
            pColorIndexTable = null;
            int blank = br.ReadInt32();
            if (blank > 0)
            {
                pColorIndexTable = new short[blank];
                for (int i = 0; i < blank; i++)pColorIndexTable[i] = br.ReadInt16();
            }           

            //pgridShowTable
            pgridShowTable = null;
            blank = br.ReadInt32();
            if (blank > 0)
            {
                pgridShowTable = new byte[blank];
                for (int i = 0; i < blank; i++) pgridShowTable[i] = br.ReadByte();
            }          

            //pShowIndexArray            
            pShowIndexArray.Clear();
            uint ix, iy, iz;
            blank = br.ReadInt32();
            for (int i = 0; i < blank; i++)
            {
                ix = (uint)br.ReadInt32();
                iy = (uint)br.ReadInt32();
                iz = (uint)br.ReadInt32();
                pShowIndexArray.Add(new UInt32XYZ(ix, iy, iz));
            }

            //render method
            meshMethod = (MCMeshMethod)br.ReadInt32();

            //load Matching Cubes
            if (meshMethod == MCMeshMethod.MC)
            {
                m_MarchCube.SetInputData(pGridData, xNum, yNum, zNum, minx, maxx, miny, maxy, minz, maxz, minv, maxv);
                if( !m_MarchCube.LoadBinary(br) ) return false;
            }
            if (meshMethod == MCMeshMethod.ImprovedMC)
            {
                m_MarchCubeExt.m_ColorScale = ColorScale;
                m_MarchCubeExt.SetData(this);
                if( !m_MarchCubeExt.LoadBinary(br) ) return false;
            }

            //load overlay info 12.5
            enableOverlap = br.ReadBoolean();
            int n = br.ReadInt32();
            overlaps.Clear();
            for (int i = 0; i < n; i++)
            {
                COverlayObject overlap;
                OverlapChannel channel = (OverlapChannel)br.ReadInt32();
                if (channel == OverlapChannel.Vector2D)
                    overlap = new Arrow2DOverlayObject(0, 1);
                else overlap = new COverlayObject(0);
                if (!overlap.Load(br))
                {
                    errMessage = "Failed load overlaps. " + overlap.errMessage;
                    return false;
                }
                overlaps.Add(overlap);
            }

            return true;
        }

        //bool SaveVersion13(BinaryWriter br)//flag == "DSGG"
        //{
        //    int length = 0;
        //    if (pGridCoords != null) length = pGridCoords.Length;
        //    br.Write(length);            
        //    for(int i = 0; i < length; i++) 
        //    {
        //        Vector32 p = pGridCoords[i];
        //        br.Write(p.X);
        //        br.Write(p.Y);
        //        br.Write(p.Z);
        //    }
        //    return true;
        //}
        //bool LoadVersion13(BinaryReader br)
        //{
        //    //return true;
        //    int length = br.ReadInt32();
        //    if ( length <= 0 ) return true;
        //    if (pGridCoords != null)pGridCoords = null;
        //    if (pGridCoords == null)pGridCoords = new Vector32[length];
        //    float x, y, z,v;
        //    for (int i = 0; i < length; i++)
        //    {
        //        x = br.ReadSingle();
        //        y = br.ReadSingle();
        //        z = br.ReadSingle();
        //        v = pGridData[i];
        //        pGridCoords[i] = new Vector32(x,y,z,v);
        //    }
        //    //UpdateGeometry();            
        //    return true;
        //}
               
        //save to Geo3D Stream file
        public override bool SaveAs(BinaryWriter br)
        {
            try
            {               
                Write3DGridHeader(br, 13);
                SaveObjHeader(br);
                SaveVersion10(br);
                SaveVersion11(br);
                SaveVersion12(br);                
                //SaveVersion13(br);
                return true;
            }
            catch (IOException e)
            {
                errMessage = "write file failed.\n" + e.Message;
                return false;
            }
        }//Save3DGridData
        //save to 3dgrid file
        public override bool SaveAs(string path,int version = 10)
        {
            try
            {
                BinaryWriter br;
                br = new BinaryWriter(new FileStream(path, FileMode.Create));
                Write3DGridHeader(br, version);
                if (version >= 10) SaveVersion10(br);
                if (version >= 11) SaveVersion11(br);
                if (version >= 12) SaveVersion12(br);
                //if (version >= 13) SaveVersion13(br);
                br.Close();
                return true;
            }
            catch (IOException e)
            {
                errMessage = "write file failed.\n" + e.Message;
                return false;
            }
        }//SaveAs(BinaryWriter br)          

        public override bool LoadObjHeader(BinaryReader br)
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
            float version = br.ReadSingle(); //added 2022-3-9
                                             //added new 2022-8
            if (version > 1.0f)
            {
                textureStruct = new TextureStruct();
                textureStruct.Load(br);
            }
            return true;
        }
        bool LoadFrom10(BinaryReader br)
        {
            try
            {  
                if (!LoadObjHeader(br))
                {
                    Clear();
                    return false;
                }

                if( !LoadVersion10(br) )
                {
                    Clear();
                    return false;
                }
                InitTables();
                return true;
            }
            catch (IOException e)
            {
                Clear();
                errMessage = "Open file failed.\n" + e.Message;
                return false;
            }
        }
        

        public override bool LoadFrom(BinaryReader br)
        {
            Clear();
            if ( !Read3DGridHeader(br) )
            {
                Clear();
                return false;
            }
            bool ret = false;
            if (Version >= 10) ret = LoadFrom10(br);            
            if (Version >= 11) ret = LoadVersion11(br);
            if (Version >= 12) ret = LoadVersion12(br);            
            //if (Version >= 13)ret = LoadVersion13(br);            
            return ret;
        }//bool LoadFrom(BinaryReader br)       
        /// <summary>
        /// 将大网格数据采样保存到文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="nx">新采样网格NX</param>
        /// <param name="ny">新采样网格NY</param>
        /// <param name="nz">新采样网格NZ</param>
        /// <returns></returns>
        public bool SaveBigGridTo(string source, string dest,
                                  int nx, int ny, int nz)
        {
            BinaryReader br = new BinaryReader(new FileStream(source, FileMode.Open));
            if (!Read3DGridHeader(br))
            {
                br.Close();
                return false;
            }
            br.Close();
            return SaveBigGridTo(source,dest,nx,ny,nz,minx,miny,minz,maxx,maxy,maxz);
        }
        /// <summary>
        /// 将大网格数据抽样保存到
        /// </summary>
        /// <param name="source">源数据文件</param>
        /// <param name="dest">目标数据文件</param>
        /// <param name="nx">x抽样网格数</param>
        /// <param name="ny">y抽样网格数</param>
        /// <param name="nz">z抽样网格数</param>
        /// <param name="x1">抽样数据起始位置x1</param>
        /// <param name="y1">抽样数据起始位置y1</param>
        /// <param name="z1">抽样数据起始位置z1</param>
        /// <param name="x2">抽样数据结束位置x2</param>
        /// <param name="y2">抽样数据结束位置y2</param>
        /// <param name="z2">抽样数据结束位置z2</param>
        /// <returns></returns>
        public bool SaveBigGridTo(string source, string dest,
                                  int nx, int ny, int nz,
                                  double x1, double y1, double z1,
                                  double x2, double y2, double z2)
        {
            BinaryReader br = null;
            try
            {
                br = new BinaryReader(new FileStream(source, FileMode.Open));                
                if (!Read3DGridHeader(br))
                {
                    br.Close();
                    return false;
                }

                xStep = (maxx - minx) / (xNum - 1);
                yStep = (maxy - miny) / (yNum - 1);
                zStep = (maxz - minz) / (zNum - 1);
                int ix1 = (int)((x1 - minx) / xStep + 0.1);
                int ix2 = (int)((x2 - minx) / xStep + 0.1);
                int iy1 = (int)((y1 - miny) / yStep + 0.1);
                int iy2 = (int)((y2 - miny) / yStep + 0.1);
                int iz1 = (int)((z1 - minz) / zStep + 0.1);
                int iz2 = (int)((z2 - minz) / zStep + 0.1);
                GeometryLimited(ref ix1, ref iy1, ref iz1);
                GeometryLimited(ref ix2, ref iy2, ref iz2);

                C3DGridData data = new C3DGridData(nx, ny, nz);                
                float[] grid = new float[xNum * yNum];//一个Z平面数据
                
                //定位到Z平面位置
                if (iz1 > 0) br.BaseStream.Seek(iz1 * xNum * yNum * sizeof(float),
                                                SeekOrigin.Current );
                
                int ix0, iy0, iz0, ix, iy, iz;
                for (iz = iz1; iz <= iz2; iz++ )
                {
                    byte[] buf = br.ReadBytes(xNum * yNum * sizeof(float));
                    Buffer.BlockCopy(buf, 0, grid, 0, buf.Length);
                    buf = null;                    
                    iz0 = (int)( (double)(iz - iz1) / (iz2 - iz1) * (nz-1) + 0.1);
                    if (iz0 >= nz) iz0 = nz - 1;
                    for (iy0 = 0; iy0 < ny; iy0++)
                    {
                        iy = (int)(iy1 + (double)iy0 / (ny - 1) * (iy2 - iy1) + 0.1);
                        if (iy >= iy2) iy = iy2;
                        for (ix0 = 0; ix0 < nx; ix0++)
                        {
                            ix = (int)( ix1 + (double)ix0 / (nx - 1) * (ix2 - ix1) + 0.1 );
                            if (ix >= ix2) ix = ix2;                            
                            data[ix0, iy0, iz0] = grid[ix + iy * xNum];
                        }
                    }                   
                }
                grid = null;
                br.Close();
                data.ResetDataRange(x1, x2, y1, y2, z1, z2, minv, maxv);
                data.SaveAs(dest);                
                data.Clear();
                return true;
            }
            catch (IOException e)
            {
                if (br != null) br.Close();
                errMessage = "Open file failed.\n" + e.Message;
                return false;
            }

        }
        /// <summary>
        /// 读取大数据网格
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sampleScale"> 抽样比例 > 1 </param>
        /// <returns></returns>
        public bool LoadFromBigGrid(string path, double sampleScale = 1.0)
        {
            BinaryReader br = null;
            try
            {               
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                Name = path;

                if (!Read3DGridHeader(br))
                {
                    br.Close();
                    Clear();
                    return false;
                }
                
                int nx = (int)(xNum / sampleScale);
                int ny = (int)(yNum / sampleScale);
                int nz = (int)(zNum / sampleScale);
                if (nx < 2) nx = 2;
                if (ny < 2) ny = 2;
                if (nz < 2) nz = 2;
                pGridData = new float[nx * ny * nz];

                bool ret = false;
                if (Version >= 10) ret = LoadVersion10BySampling(br, sampleScale,nx, ny,nz);
                if (!ret)
                {
                    Clear();
                    br.Close();
                    return false;
                }
                
                xNum = nx;
                yNum = ny;
                zNum = nz;
                xyNum = xNum * yNum;
                UpdateRange();

                br.Close();
                InitTables();
                return true;
            }
            catch (IOException e)
            {
                if (br != null) br.Close();
                errMessage = "Open file failed.\n" + e.Message;
                return false;
            }

        }
        //bool Load3DGridData(string path)
        
        public override bool LoadFrom(string path)
        {
            BinaryReader br = null;
            try
            {
               // Stopwatch stopwatch = new Stopwatch();
               // stopwatch.Start();
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                Name = path;

                if (!Read3DGridHeader(br))
                {
                    br.Close();
                    Clear();
                    return false;
                }

                bool ret = false;
                if (Version >= 10) ret = LoadVersion10(br);
                if (Version >= 11) ret = LoadVersion11(br);
                if (Version >= 12) { ret = LoadVersion12(br); }
               // if (Version >= 13) ret = LoadVersion13(br);
                if (!ret)
                {
                    Clear();
                    br.Close();
                    return false;
                }                
                br.Close();
               // stopwatch.Stop();
              //  double els = stopwatch.Elapsed.TotalMilliseconds;
                InitTables();
                return true;
            }
            catch (IOException e)
            {
                if (br != null) br.Close();
                errMessage = "Open file failed.\n" + e.Message;
                return false;
            }

        }//bool Load3DGridData(string path)

        public bool Read3DGridHeader(string path )
        {
            BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open));

            string tag = new string(br.ReadChars(4));
            string header1 = "DSGD";//Version tag = "DSGD" 1.0
            string header2 = "DSGE";//Version tag = "DSGE" 1.1
            string header3 = "DSGF";//Version tag = "DSGF" 1.2
            string header4 = "DSGG";//Version tag = "DSGF" 1.3
            Version = 0;
            if (tag == header1) Version = 10;
            else if (tag == header2) Version = 11;
            else if (tag == header3) Version = 12;
            else if (tag == header4) Version = 13;

            if (Version < 10)
            {
                errMessage = "not a correct file format.";
                Clear();
                return false;
            }

            xNum = br.ReadInt32();
            yNum = br.ReadInt32();
            zNum = br.ReadInt32();
            minx = br.ReadDouble();
            maxx = br.ReadDouble();
            miny = br.ReadDouble();
            maxy = br.ReadDouble();
            minz = br.ReadDouble();
            maxz = br.ReadDouble();
            minv = br.ReadDouble();
            maxv = br.ReadDouble();

            if ((xNum <= 0) || (yNum <= 0) || (zNum <= 0) ||
                double.IsNaN(minv) || double.IsNaN(maxv) ||
                double.IsNaN(minx) || double.IsNaN(maxx) ||
                double.IsNaN(miny) || double.IsNaN(maxy) ||
                double.IsNaN(minz) || double.IsNaN(maxz) ||
                 (minx >= maxx) || (miny >= maxy) ||
                 (minz >= maxz) || (minv >= maxv))
            {
                Clear();
                errMessage = "Invalid data header.";
                //  return false;
            }            
            
            br.Close();
            return true;
        }
       
        /// <summary>
        /// Read Object Header
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public bool Read3DGridHeader(BinaryReader br)
        {
            string tag = new string(br.ReadChars(4));
            string header1 = "DSGD";//Version tag = "DSGD" 1.0
            string header2 = "DSGE";//Version tag = "DSGE" 1.1
            string header3 = "DSGF";//Version tag = "DSGF" 1.2
            string header4 = "DSGG";//Version tag = "DSGF" 1.3
            Version = 0;
            if (tag == header1) Version = 10;
            else if (tag == header2) Version = 11;
            else if (tag == header3) Version = 12;
            else if (tag == header4) Version = 13;

            if (Version < 10)
            {
                errMessage = "not a correct file format.";
                Clear();
                return false;
            }

            xNum = br.ReadInt32();
            yNum = br.ReadInt32();
            zNum = br.ReadInt32();
            minx = br.ReadDouble();
            maxx = br.ReadDouble();
            miny = br.ReadDouble();
            maxy = br.ReadDouble();
            minz = br.ReadDouble();
            maxz = br.ReadDouble();
            minv = br.ReadDouble();
            maxv = br.ReadDouble();

            if ((xNum <= 0) || (yNum <= 0) || (zNum <= 0) ||
                double.IsNaN(minv) || double.IsNaN(maxv) ||
                double.IsNaN(minx) || double.IsNaN(maxx) ||
                double.IsNaN(miny) || double.IsNaN(maxy) ||
                double.IsNaN(minz) || double.IsNaN(maxz) ||
                 (minx >= maxx) || (miny >= maxy) ||
                 (minz >= maxz) || (minv >= maxv))
            {
                Clear();
                errMessage = "Invalid data header.";
              //  return false;
            }

            //if (C3DData.DataVersion >= 1.26f&& version>=12)
            //{
            //    EnableGeoreference = br.ReadBoolean();
            //    Corner1.X = br.ReadDouble();
            //    Corner1.Y = br.ReadDouble();
            //    Corner1.Z = br.ReadDouble();
            //    Corner2.X = br.ReadDouble();
            //    Corner2.Y = br.ReadDouble();
            //    Corner2.Z = br.ReadDouble();
            //}


            xyNum = xNum * yNum;
            xStep = (maxx - minx) / (xNum - 1);
            yStep = (maxy - miny) / (yNum - 1);
            zStep = (maxz - minz) / (zNum - 1);            

            ColorScale.SetValueRange(minv, maxv);           
            
            return true;
        }
        public bool Write3DGridHeader(BinaryWriter br, int _version)
        {
            try 
            {                
                string header1 = "DSGD";//Version tag = "DSGD" 1.0
                string header2 = "DSGE";//Version tag = "DSGE" 1.1
                string header3 = "DSGF";//Version tag = "DSGF" 1.2
                string header4 = "DSGG";//Version tag = "DSGG" 1.30f                                        
                if (_version == 11) br.Write(header2.ToCharArray());
                else if (_version == 12) br.Write(header3.ToCharArray());
                else if (_version == 13) br.Write(header4.ToCharArray());
                else  br.Write(header1.ToCharArray());
                
                xyNum = xNum * yNum;

                br.Write(xNum);
                br.Write(yNum);
                br.Write(zNum);
                br.Write(minx);
                br.Write(maxx);
                br.Write(miny);
                br.Write(maxy);
                br.Write(minz);
                br.Write(maxz);
                br.Write(minv);
                br.Write(maxv);

                //if( C3DData.Version >=1.26f )
                //{
                //    br.Write(EnableGeoreference);
                //    br.Write(Corner1.X);
                //    br.Write(Corner1.Y);
                //    br.Write(Corner1.Z);
                //    br.Write(Corner2.X);
                //    br.Write(Corner2.Y);
                //    br.Write(Corner2.Z);
                //}
                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }            
        }

        /// <summary>
        /// 叠加data2到data1
        /// </summary>
        /// <param name="data1">叠加目标数据</param>
        /// <param name="data2">叠加源数据</param>        
        /// <param name="method">单元处理方法0替换，1平均</param>
        /// <param name="reset">是否用layervalue代替data2网格单元值</param>
        /// <param name="layervalue">地层值layervalue</param>
        /// <returns>叠加网格数</returns>
        static public int LayerOverlaped(C3DGridData data1, C3DGridData data2, int method =0, bool reset = false, double layervalue = 0)
        {
            long length = data1.Length;
            if (data2.Length != length) return 0; //网格不一致
            int count = 0;
            float v1, v2, v;
            for (long id = 0; id < length; id++)
            {
                v1 = data1[id];
                v2 = data2[id];
                if ( data2.IsBlankValue(v2) ) continue;

                if (reset) v2 = (float)layervalue; //地层值重置
                
                if ( method == 0 )v = v2;//直接替换
                else //平均
                {
                    if ( data1.IsBlankValue(v1) ) v = v2;
                    else v = (v1 + v2) / 2;
                }

                data1[id] = v;
                count++;

                //update data range
                if (v < data1.minv) data1.minv = v;
                if (v > data1.maxv) data1.maxv = v;
            }
            return count;
        }
        /// <summary>
        /// 按照空间位置叠加
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <param name="method"></param>
        /// <param name="reset"></param>
        /// <param name="layervalue"></param>
        /// <returns></returns>
        static public int LayerOverlapedOnLocation(C3DGridData data1, C3DGridData data2, int method = 0, bool reset = false, double layervalue = 0)
        {
            int count = 0;
            float v1, v2, v;
            Vector64 p2;            
            long id1; // index on data1 grid, id2 index on data2
            double x, y, z;

            int xw = (int)(data2.xStep / data1.xStep);
            int yw = (int)(data2.yStep / data1.yStep);
            int zw = (int)(data2.zStep / data1.zStep);
            double xl = data2.xStep - xw * data2.xStep;
            double yl = data2.yStep - yw * data2.yStep;
            double zl = data2.zStep - zw * data2.zStep;
            if (xl > 0) xw++;
            if (yl > 0) yw++;
            if (zl > 0) zw++;
            for (long id2 = 0; id2 < data2.pGridData.Length; id2++)
            {
                v2 = data2.pGridData[id2];
                if ( data2.IsBlankValue(v2) ) continue; //invalid value

                //grid position at data2
                p2 = data2.GetGridCoord(id2).toVector64();
                
                if (reset) v2 = (float)layervalue; //地层值重置

                //粗网格可能叠加到细网格
                for (int iz = 0; iz <= zw; iz++)
                    for (int iy = 0; iy <= yw; iy++)
                        for (int ix = 0; ix <= xw; ix++)
                        {
                            if ( ix == xw && xw > 1) x = p2.x + (ix-1) * data1.xStep + xl;
                            else x = p2.x + ix * data1.xStep;

                            if (iy == yw && yw > 1) y = p2.y + (iy-1) * data1.yStep + yl;
                            else y = p2.y + iy * data1.yStep;

                            if (iz == zw && zw > 1) z = p2.z + (iz-1) * data1.zStep + zl;
                            else z = p2.z + iz * data1.zStep;

                            //grid index at data1
                            id1 = data1.GetVerticIndex(data1.GetVerticIndexByPosition(x, y, z));

                            //data2 excceed data1 range
                            if ( !data1.IsGridValid( id1 ) ) continue;

                            v1 = data1.pGridData[id1];

                            if (method == 0) v = v2;//直接替换
                            else //平均
                            {
                                if (data1.IsBlankValue(v1)) v = v2;
                                else v = (v1 + v2) / 2;
                            }

                            data1.pGridData[id1] = v;

                            //update data range
                            if (v < data1.minv) data1.minv = v;
                            if (v > data1.maxv) data1.maxv = v;
                            
                            count++;
                        }
                
            }
            return count;
        }
        public bool SaveAsPly(string path, string ply = "")
        {
            string filename = path;
            if ( meshMethod == MCMeshMethod.MC )
            {
                int n1 = m_MarchCube.pIsoSurface.Count;

                List<TriangleObj> objects = new List<TriangleObj>();
                for (int i = 0; i < m_MarchCube.pIsoSurface.Count; i++)
                {
                    TriangleObj obj = m_MarchCube.pIsoSurface[i].toTriangleObj();
                    ColorRGBA c = GetColor(m_MarchCube.pIsoSurface[i].isoVale);
                    obj.color = new vec4((float)(c.R / 255.0), (float)(c.G / 255.0), (float)(c.B / 255.0), 1);
                    obj.IsUniformColor = true;
                    objects.Add(obj);                    
                }
                if (objects.Count > 0)
                {
                    TriangleObj tri = TriangleObj.toTriangleObj(objects); 
                    tri.SaveAsPLY(filename);
                    tri.Clear();
                    objects.Clear();
                    return true;
                }
                errMessage = "no surfaces exported.";
                return false;
            }
            else if (meshMethod == MCMeshMethod.ImprovedMC)
            {
                TriangleObj obj = m_MarchCubeExt.pISOSurfaceExt.toTriangleObj();

                if (ply.Length > 0) filename = ply;
              
                obj.SaveAsPLY(filename);
                obj.Clear();

                return true;
            }
            else 
            {
                errMessage = "no surfaces exported.";
                return false; 
            }
        }
        public bool SaveAsSTL(string path, string stl = "")
        {
            string filename = path;
            if (meshMethod == MCMeshMethod.MC)
            {
                int n1 = m_MarchCube.pIsoSurface.Count;
                List<TriangleObj> objects = new List<TriangleObj>();
                for (int i = 0; i < m_MarchCube.pIsoSurface.Count; i++)
                {
                    TriangleObj obj = m_MarchCube.pIsoSurface[i].toTriangleObj();

                    ColorRGBA c = GetColor(m_MarchCube.pIsoSurface[i].isoVale);
                    obj.color = new vec4((float)(c.R / 255.0), (float)(c.G / 255.0), (float)(c.B / 255.0), 1);
                    obj.IsUniformColor = true;
                    objects.Add(obj);
                }
                if (objects.Count > 0)
                {
                    TriangleObj tri = TriangleObj.toTriangleObj(objects);
                    tri.SaveAsPLY(filename);
                    tri.Clear();
                    objects.Clear();
                    return true;
                }
                errMessage = "no surfaces exported.";
                return false;
            }
            if (meshMethod == MCMeshMethod.ImprovedMC)
            {
                TriangleObj obj = m_MarchCubeExt.pISOSurfaceExt.toTriangleObj();

                if (stl.Length > 0) filename = stl;
                else filename = path + "\\" + obj.Name + ".stl";

                obj.SaveAsSTL(filename);
                obj.Clear();

                return true;
            }
            return false;
        }

        public bool SaveAsOBJ(string path, string objfile = "")
        {
            string filename = path;
            if (meshMethod == MCMeshMethod.MC)
            {
                int n1 = m_MarchCube.pIsoSurface.Count;
                List<TriangleObj> objects = new List<TriangleObj>();
                for (int i = 0; i < m_MarchCube.pIsoSurface.Count; i++)
                {
                    TriangleObj obj = m_MarchCube.pIsoSurface[i].toTriangleObj();
                    ColorRGBA c = GetColor(m_MarchCube.pIsoSurface[i].isoVale);
                    obj.color = new vec4((float)(c.R / 255.0), (float)(c.G / 255.0), (float)(c.B / 255.0), 1);
                    obj.IsUniformColor = true;
                    objects.Add(obj);
                }
                if (objects.Count > 0)
                {
                    TriangleObj tri = TriangleObj.toTriangleObj(objects);
                    tri.SaveAsPLY(filename);
                    tri.Clear();
                    objects.Clear();
                    return true;
                }
                errMessage = "no surfaces exported.";
                return false;
            }
            if (meshMethod == MCMeshMethod.ImprovedMC)
            {
                TriangleObj obj = m_MarchCubeExt.pISOSurfaceExt.toTriangleObj();

                if (objfile.Length > 0) filename = objfile;
                else filename = path + "\\" + obj.Name + ".obj";

                obj.SaveAsOBJ(filename);

                return true;
            }
            return false;
        }

        public override bool ExportData(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);

                long id;
                string ss;
                ss = "x,y,z,value\n";
                wr.WriteLine(ss);
                double x, y, z, v;

                for (int iz = 0; iz < zNum; iz++)
                    for (int iy = 0; iy < yNum; iy++)
                        for (int ix = 0; ix < xNum; ix++)
                        {
                            id = GetVerticIndex(ix, iy, iz);
                            if (pgridShowTable[id] == 0) continue;
                            if (pBlankTable[id]) continue;
                            v = pGridData[id];
                            if (IsBlankValue(v)) continue;

                            x = minx + ix * xStep;
                            y = miny + iy * yStep;
                            z = minz + iz * zStep;
                            x = Math.Round(x, 6);
                            y = Math.Round(y, 6);
                            z = Math.Round(z, 6);
                            v = Math.Round(v, 6);
                            ss = x.ToString(); ss += ",";
                            ss += y.ToString(); ss += ",";
                            ss += z.ToString(); ss += ",";
                            ss += v.ToString(); ss += "\n";
                            wr.WriteLine(ss);
                        }

                wr.Close();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        //InversePower Method
        public void SmoothData(int winLen1 = 2, int winLen2 = 2, int winLen3 = 2)
        {
            if (xNum < 1 || yNum < 1 || zNum < 1) return;
            if (winLen1 < 2 || winLen2 < 2 || winLen3 < 2) return;
            float[] data = new float[xNum * yNum * zNum];
            if (data == null)
            {
                return;
            }
            int ix1, iy1, iz1;
            long cur0, cur = 0;
            double x, y, z, v = 0;
            for (int iz = 0; iz < zNum; iz++)
            {
                for (int iy = 0; iy < yNum; iy++)
                    for (int ix = 0; ix < xNum; ix++)
                    {
                        CInversePower ip = new CInversePower();
                        //ip.Clear();
                        cur0 = ix + iy * xNum + iz * xNum * yNum;
                        for (int k = -winLen3; k <= winLen3; k++)
                            for (int j = -winLen2; j <= winLen2; j++)
                                for (int i = -winLen1; i <= winLen1; i++)
                                {
                                    ix1 = ix + i;
                                    iy1 = iy + j;
                                    iz1 = iz + k;
                                    if (i == 0 && j == 0 && k == 0) continue;
                                    if (ix1 < 0 || ix1 >= xNum) continue;
                                    if (iy1 < 0 || iy1 >= yNum) continue;
                                    if (iz1 < 0 || iz1 >= zNum) continue;

                                    cur = ix1 + iy1 * xNum + iz1 * xNum * yNum;
                                    x = minx + ix1 * xStep;
                                    y = miny + iy * yStep;
                                    z = minz + iz * zStep;
                                    v = pGridData[cur];
                                    if (IsBlankValue(v)) continue;
                                    ip.AddPoint(x, y, z, v);
                                }
                        x = minx + ix * xStep;
                        y = miny + iy * yStep;
                        z = minz + iz * zStep;

                        v = ip.GetInterValue(x, y, z);
                        data[cur0] = (float)v;
                    }
            }
            for (int i = 0; i < xNum * yNum * zNum; i++)
            {
                pGridData[i] = data[i];
                //update value range
                if (i == 0)
                {
                    minv = maxv = data[i];
                }
                else
                {
                    if (minv > data[i]) minv = data[i];
                    if (maxv < data[i]) maxv = data[i];
                }
            }
            ColorScale.SetValueRange(minv, maxv);
            data = null;
        }
        public void ResetGridData(float value)
        {
            if (pGridData == null) return;
            for (int i = 0; i < pGridData.Length; i++)
                pGridData[i] = value;
        }
        public void ResetDataRange(double x1, double x2, double y1, double y2, double z1, double z2, double v1, double v2)
        {
            minx = x1;
            maxx = x2;
            miny = y1;
            maxy = y2;
            minz = z1;
            maxz = z2;
            minv = v1;
            maxv = v2;
            xStep = (maxx - minx) / (xNum - 1);
            yStep = (maxy - miny) / (yNum - 1);
            zStep = (maxz - minz) / (zNum - 1);
        }
        public C3DGridData ResizeGrid(int nx1, int ny1, int nz1)
        {
            //new space step
            double stepx1 = (maxx - minx) / (nx1 - 1);
            double stepy1 = (maxy - miny) / (ny1 - 1);
            double stepz1 = (maxz - minz) / (nz1 - 1);

            List<C2DGridData> p2DSlicers = new List<C2DGridData>();

            long id;
            double x, y, z;
            for (int iz = 0; iz < zNum; iz++)
            {
                C2DGridData s0 = new C2DGridData(this, -1, -1, iz);
                float[] grid = new float[nx1 * ny1];
                for (int iy = 0; iy < ny1; iy++)
                    for (int ix = 0; ix < nx1; ix++)
                    {
                        id = ix + iy * nx1;
                        x = minx + ix * stepx1;
                        y = miny + iy * stepy1;
                        grid[id] = (float)s0.GetGridValue(x, y);
                    }
                C2DGridData s1 = new C2DGridData();
                s1.SetData(grid, nx1, ny1, minx, maxx, miny, maxy, minv, maxv);
                p2DSlicers.Add(s1);
            }

            C3DGridData d0 = new C3DGridData();
            d0.pGridData = new float[nx1 * ny1 * nz1];
            d0.xNum = nx1;
            d0.yNum = ny1;
            d0.zNum = nz1;
            d0.minx = minx;
            d0.miny = miny;
            d0.minz = minz;
            d0.maxx = maxx;
            d0.maxy = maxy;
            d0.maxz = maxz;

            int iz1, iz2;
            double v, v1, v2, dz;
            for (int iz = 0; iz < nz1; iz++)
            {
                z = minz + iz * stepz1;
                iz1 = (int)((z - minz + 0.01 * zStep) / (zStep));
                dz = z - (iz1 * zStep + minz);

                iz2 = iz1 + 1;
                if (dz == 0) iz2 = iz1;
                if (iz2 >= zNum) iz2 = iz1;

                for (int iy = 0; iy < ny1; iy++)
                    for (int ix = 0; ix < nx1; ix++)
                    {
                        v1 = p2DSlicers[iz1].GetGridValue(ix, iy);
                        v2 = p2DSlicers[iz2].GetGridValue(ix, iy);
                        v = v1 + (v2 - v1) * dz / zStep;
                        id = ix + iy * nx1 + iz * nx1 * ny1;
                        d0[id] = (float)v;
                    }
            }
            p2DSlicers.Clear();
            d0.UpdateRange();
            return d0;
        }
        // if the color table changed,the ColorIndexTable should be updated
        public void UpdateColorIndexTable()
        {
            if (pColorIndexTable == null) return;
            if (xNum <= 0 || yNum <= 0 || zNum <= 0) return;
            for (long i = 0; i < zNum * xNum * yNum; i++)
                pColorIndexTable[i] = (Int16)ColorScale.GetColorIndex(pGridData[i]);
        }
        public void UpdateOptimisedShowArray()
        {
            // optimize the show,update show array
            pShowIndexArray.Clear();
            UInt32 ix, iy, iz;
            for (iz = 0; iz < zNum; iz++)
                for (iy = 0; iy < yNum; iy++)
                    for (ix = 0; ix < xNum; ix++)
                    {
                        if (IsNeedShow(ix, iy, iz))
                        {
                            UInt32XYZ xyz = new UInt32XYZ(ix, iy, iz);
                            pShowIndexArray.Add(xyz);
                        }
                    }           
        }
        //if the color table change,reset the showtable 
        public void UpdateShowTableFromColorScale()
        {
            if (pColorIndexTable == null) return;
            if (pgridShowTable == null) return;
            bool is_show = false;
            int icolor = 0;
            UInt32 ix, iy, iz;
            long id;
            float alpha = 0;
            for (iz = 0; iz < zNum; iz++)
                for (iy = 0; iy < yNum; iy++)
                    for (ix = 0; ix < xNum; ix++)
                    {
                        id = ix + iy * xNum + iz * xNum * yNum;
                        if (IsBlankedGrid(id))
                        {
                            pgridShowTable[id] = 0;
                            continue;
                        }
                        if (IsBlankValue(pGridData[id]))
                        {
                            pgridShowTable[id] = 0;
                            continue;
                        }

                        icolor = pColorIndexTable[id];
                        is_show = ColorScale[icolor].Visible;
                        alpha = ColorScale[icolor].A;

                        if (is_show)
                        {
                            if (alpha == 1f) pgridShowTable[id] = 1;
                            else pgridShowTable[id] = 2;//透明色
                        }                        
                        else pgridShowTable[id] = 0;
                    }
            UpdateOptimisedShowArray();

            RenderMode = RenderingUpdateMode.Redraw;
        }//public void UpdateShowTable()

        //根据颜色表的指示，过滤显示列表
        //refine the  pgridShowTable,set those 1 to be 0
        public void RefineShowTableFromColorScale(CColorScale _colorscale)
        {
            if (pColorIndexTable == null) return;
            if (pgridShowTable == null) return;
            bool is_show = false;
            int icolor = 0;
            UInt32 ix, iy, iz;
            long id;
            for (iz = 0; iz < zNum; iz++)
                for (iy = 0; iy < yNum; iy++)
                    for (ix = 0; ix < xNum; ix++)
                    {
                        id = ix + iy * xNum + iz * xNum * yNum;
                        icolor = pColorIndexTable[id];
                        is_show = _colorscale[icolor].Visible;
                        if (is_show)
                        {
                            //pgridShowTable[id] = 1;
                        }
                        else pgridShowTable[id] = 0;
                    }
            UpdateOptimisedShowArray();
        }//public void UpdateShowTable()
        private void CreateTriangleFromPyramid(CPyramid p)
        {
            if (p.pShowState[0] && p.pShowState[1] == false && p.pShowState[2] == false && p.pShowState[3] == false)
            {

            }
            ///*
            //for (int i = 0; i < 4; i++)
            //    if (!p.pShowState[i]) return;
            if (p.pShowState[0] && p.pShowState[2] && p.pShowState[1])
            {
                CTriangle3f tri1 = new CTriangle3f(p.p, p.p2, p.p1);
                tri1.pColor[0] = p.pColor[0];
                tri1.pColor[1] = p.pColor[2];
                tri1.pColor[2] = p.pColor[1];
                pTriangleArray.Add(tri1);
            }
            if (p.pShowState[0] && p.pShowState[3] && p.pShowState[2])
            {
                CTriangle3f tri2 = new CTriangle3f(p.p, p.p3, p.p2);
                tri2.pColor[0] = p.pColor[0];
                tri2.pColor[1] = p.pColor[3];
                tri2.pColor[2] = p.pColor[2];
                pTriangleArray.Add(tri2);
            }
            if (p.pShowState[0] && p.pShowState[1] && p.pShowState[3])
            {
                CTriangle3f tri3 = new CTriangle3f(p.p, p.p1, p.p3);
                tri3.pColor[0] = p.pColor[0];
                tri3.pColor[1] = p.pColor[1];
                tri3.pColor[2] = p.pColor[3];
                pTriangleArray.Add(tri3);
            }
            if (p.pShowState[1] && p.pShowState[2] && p.pShowState[3])
            {
                CTriangle3f tri4 = new CTriangle3f(p.p1, p.p2, p.p3);
                tri4.pColor[0] = p.pColor[1];
                tri4.pColor[1] = p.pColor[2];
                tri4.pColor[2] = p.pColor[3];
                pTriangleArray.Add(tri4);
            }
            // */
        }
        //p 顶点，123下三角形
        private void CreateTriangleFromPyramid(UInt32XYZ p, UInt32XYZ p1, UInt32XYZ p2, UInt32XYZ p3)
        {
            double x, y, z;
            x = minx + p.x * xStep;
            y = miny + p.y * yStep;
            z = minz + p.z * zStep;
            Vector32 pp = new Vector32((float)x, (float)y, (float)z);
            x = minx + p1.x * xStep;
            y = miny + p1.y * yStep;
            z = minz + p1.z * zStep;
            Vector32 pa = new Vector32((float)x, (float)y, (float)z);
            x = minx + p2.x * xStep;
            y = miny + p2.y * yStep;
            z = minz + p2.z * zStep;
            Vector32 pb = new Vector32((float)x, (float)y, (float)z);
            x = minx + p3.x * xStep;
            y = miny + p3.y * yStep;
            z = minz + p3.z * zStep;
            Vector32 pc = new Vector32((float)x, (float)y, (float)z);

            CPyramid pm = new CPyramid(pp, pa, pb, pc);
            pm.pColor[0] = GetColor(p);
            pm.pColor[1] = GetColor(p1);
            pm.pColor[2] = GetColor(p2);
            pm.pColor[3] = GetColor(p3);

            pm.pShowState[0] = IsNeedShow(p);
            pm.pShowState[1] = IsNeedShow(p1);
            pm.pShowState[2] = IsNeedShow(p2);
            pm.pShowState[3] = IsNeedShow(p3);
            CreateTriangleFromPyramid(pm);
        }
        private void Create4FaceObj(UInt32XYZ[] p)
        {
            CreateTriangleFromPyramid(p[4], p[1], p[3], p[5]);
            //4,3,2,1
            CreateTriangleFromPyramid(p[1], p[0], p[3], p[5]);
            //5,3,2,4
            CreateTriangleFromPyramid(p[1], p[0], p[5], p[2]);
            /*
            //0,1,2,3            
            CreateTriangleFromPyramid(p[0], p[1], p[2], p[3]);
            //4,3,2,1
            CreateTriangleFromPyramid(p[4], p[3], p[2], p[1]);
            //5,3,2,4
            CreateTriangleFromPyramid(p[5], p[3], p[2], p[4]);
            */
        }
        private void CreateTriangle(UInt32XYZ xyz)
        {
            //if ((xyz.x >= xNum - 1) || (xyz.y >= yNum - 1) || (xyz.z >= zNum - 1))
            //    return;
            UInt32XYZ[] p = new UInt32XYZ[8];
            p[0] = xyz;
            p[1] = xyz; p[1].x++;
            p[2] = p[1]; p[2].y++;
            p[3] = p[2]; p[3].x--;
            p[4] = p[0]; p[4].z++;
            p[5] = p[1]; p[5].z++;
            p[6] = p[2]; p[6].z++;
            p[7] = p[3]; p[7].z++;

            UInt32XYZ[] pa = new UInt32XYZ[] { p[0], p[1], p[3], p[4], p[5], p[7] };
            Create4FaceObj(pa);
            UInt32XYZ[] pb = new UInt32XYZ[] { p[1], p[2], p[3], p[5], p[6], p[7] };
            Create4FaceObj(pb);
        }
        public void UpdateTriangleFromShowArray()
        {
            pTriangleArray.Clear();

            xStep = (maxx - minx) / (xNum - 1);
            yStep = (maxy - miny) / (yNum - 1);
            zStep = (maxz - minz) / (zNum - 1);

            int n = pShowIndexArray.Count;
            UInt32XYZ xyz;
            for (int i = 0; i < n; i++)
            {
                xyz = pShowIndexArray[i];
                CreateTriangle(xyz);
            }
        }//public void UpdateTriangleFromShowArray()

        public void CreateMarchingCubeTriangleExt(List<PointF>closedValues)
        {
            InitTables();
            m_MarchCubeExt.Clear();
            m_MarchCubeExt.SetData(this);
            m_MarchCubeExt.ClearClosedValues();
            for(int i=0;i<closedValues.Count;i++) 
            {
                m_MarchCubeExt.AddClosedValue(closedValues[i].X, closedValues[i].Y);
            }
            
            m_MarchCubeExt.DoSearchEdges();
        }

        public void CreateMarchingCubeTriangleExt()
        {
            m_MarchCubeExt.Clear();
            m_MarchCubeExt.SetData(this);
            m_MarchCubeExt.DoSearchEdges();
        }//public void UpdateTriangleFromShowArray()
        private bool IsEqual(double v1, double v2)
        {
            if (v1 > v2) return false;
            if (v2 > v1) return false;
            return true;
        }
        public void CreateMarchingCubeTriangle()
        {
            if (m_MarchCube.pGridData == null)
            {
                m_MarchCube.Clear();
                m_MarchCube.SetInputData(pGridData, xNum, yNum, zNum,
                                                    minx, maxx, miny,
                                                    maxy, minz, maxz, minv, maxv);
            }

            m_MarchCube.SetDividedNum(2);
            m_MarchCube.searchedGridNo = 0;
            bool exist = false;
            TextureStruct texture;
            for (int i = 0; i < ColorScale.Count; i++)
            {
                if (ColorScale[i].Visible)
                {
                    // to check if the value is already exist
                    exist = false;
                    texture = ColorScale[i].Texture;
                    for (int j = 0; j < m_MarchCube.pIsoSurface.Count; j++)
                    {
                        if (IsEqual(ColorScale.GetScaledValue(i), m_MarchCube.pIsoSurface[j].isoVale))
                        {
                            CISOSurface sf = m_MarchCube.pIsoSurface[j];
                            sf.texture = texture;
                            m_MarchCube.pIsoSurface[j] = sf;
                            exist = true;
                            break;
                        }
                    }
                    // if the value not exist,then do DoSearchSurface
                    if (!exist)
                    {
                        m_MarchCube.DoSearchSurface(ColorScale.GetScaledValue(i), texture);
                    }
                }
            }
        }//public void UpdateTriangleFromShowArray()     
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
        public override bool ExportVRML(StreamWriter wr)
        {
            if (!Visible) return false;
            CISOSurfaceExt iso = m_MarchCubeExt.pISOSurfaceExt;
            int np = iso.pCoordArray.Count;
            int ip = iso.pTriangleIndex.Count;
            if (np < 3 || ip < 1) return false;

            string line;
            line = "# --Isosurface extracted from " + Name;
            wr.WriteLine(line);
            return iso.toTriangleObj().ExportVRML(wr);
        }

        //updated Mar 25 2023--add multiple properties support
        public bool IsIntersected(vec2 p1, vec2 p2)
        {
            if (p1.y < p2.x || p2.y < p1.x) return false;
            else return true;
        }
        
        public vec2 MergeVisibleValues(vec2 p1,vec2 p2)
        {
            float v1 = Math.Min(p1.x, p2.x);
            float v2 = Math.Max(p1.y, p2.y);
            return new vec2(v1,v2);
        }

        public void MergeVisibleValues()
        {
            if (visibleValues.Count < 2) return;

            bool[] merged = new bool[visibleValues.Count];
            for (int i = 0; i < merged.Length; i++) merged[i] = false;
            vec2 p0,p;            
            for (int i = 0; i < visibleValues.Count; i++)
            {
                p0 = visibleValues[i];                
                for (int j = i+1; j < visibleValues.Count; j++)
                {
                    if ( merged[j] ) continue;
                    p = visibleValues[j];
                    if (IsIntersected(p0, p))
                    { 
                        p0 = MergeVisibleValues(p0, p);
                        merged[j] = true;
                    }
                }
                visibleValues[i] = p0;
            }
            List<vec2> values = new List<vec2>();
            for (int i = 0; i < visibleValues.Count; i++)
            {
                if (!merged[i]) values.Add(visibleValues[i]);
            }
            visibleValues.Clear();
            visibleValues = values;
        }

        public void UpdateByVisibleValues()
        {
            if ( visibleValues.Count < 1 ) return;
            float v;
            for (long i = 0; i < pGridData.Length; i++)
            {
                v = pGridData[i];
                pBlankTable[i] = true;
                pgridShowTable[i]= 0;
                if (IsBlankValue(v)) continue;
                if (!IsValidV(v)) continue;                
                if( IsVisibleValues(v)) pBlankTable[i] = false;
                pgridShowTable[i]= 1;
            }
        }
        bool IsVisibleValues(double v)
        {
            foreach(vec2 p in visibleValues)
            {
                if (v >= p.x && v <= p.y) 
                    return true;
            }
            return false;
        }
    }
}
