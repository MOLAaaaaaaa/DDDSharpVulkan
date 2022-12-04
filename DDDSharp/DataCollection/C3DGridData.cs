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
                        pGridData[i * xNum + j] = data.pGridData[id];
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
                        pGridData[i * xNum + j] = data.pGridData[id];
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
                        pGridData[i * xNum + j] = data.pGridData[id];
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
        public C3DGridData p3DGrid = null;          //3DGrid对象
        public int Demension = 1;                   //维度，1维只有1个值
        public float[] data = null;                 //grid 数据
        public C3DObjectBase sourceObject = null;   //叠加对象原始数据
        public ShapeEnum type //叠加数据对象类型
        { 
            get 
            {
                if (sourceObject != null) return sourceObject.type;
                else return ShapeEnum.Undefine;
            } 
        }  

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
            Array.Copy(grid3d.pGridData,data, grid3d.pGridData.Length);
            ColorScale.SetValueRange(sourceObject.minv, sourceObject.maxv);
        }
        public void fromScatterPoints(ScatteredPoints sc)
        {
            sourceObject = sc;

            for(int i=0;i<data.Length;i++)
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
            Array.Copy(grid3d.pGridData, data, grid3d.pGridData.Length);
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
    public class C3DGridData : C3DObjectBase
    {
        public int xNum;
        public int yNum;
        public int zNum;
        public double xStep, yStep, zStep;
        public long xyNum = 0;
        public MarchingCubes m_MarchCube = new MarchingCubes();
        public MarchingCubesExt m_MarchCubeExt = new MarchingCubesExt();
        //public List<COverlayObject> overlaps = new List<COverlayObject>();
        public C3DGridData objP32 = null;

        public int[] pBlankedPointIndexes = null;
        public List<vec3> pBlankedPoints = new List<vec3>();
        public bool[] pBlankTable = null;
        public int version = -1;    //10,11,12

        #region Overlaps section
        [CategoryAttribute("Overlaps"), DisplayNameAttribute("Count")]
        public int Count { get { return overlaps.Count; } }
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

        
        public override string Information
        {
            get
            {
                string info = "Size = " + xNum + " * " + yNum + " * " + zNum + ";\n";
                info += "X: " + Minx + " to " + Maxx + ";\n";
                info += "Y: " + Miny + " to " + Maxy + ";\n";
                info += "Z: " + Minz + " to " + Maxz;
                return info;
            }
            //set { name = value; }
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
        public void GetIndices(long id,ref int ix,ref int iy,ref int iz)
        {
            iz =(int)( id / xyNum );
            long left = id % xyNum;
            iy = (int)(left / xNum);
            ix = (int)(left % xNum);
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
        public void GetIndices(Vector64 p, ref int ix, ref int iy, ref int iz)
        {
            ix = (int)((p.X - minx) / xStep);
            iy = (int)((p.Y - miny) / yStep);
            iz = (int)((p.Z - minz) / zStep);
        }
        public Int32XYZ GetIndices(Vector64 p)
        {
            Int32XYZ xyz = new Int32XYZ();
            xyz.x = (int)((p.X - minx) / xStep);
            xyz.y = (int)((p.Y - miny) / yStep);
            xyz.z = (int)((p.Z - minz) / zStep);
            return xyz;
        }
        public Int32XYZ GetIndices(long id)
        {
            Int32XYZ xyz = new Int32XYZ();
            xyz.z = (int)(id / xyNum);
            long left = id % xyNum;
            xyz.y = (int)(left / xNum);
            xyz.x = (int)(left % xNum);
            return xyz;
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
                 ix >= xNum || ix >= yNum || ix >= zNum ) return true;
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
                vec3 p = new vec3((float)minx - 1, (float)miny - 1, (float)minz - 1);
                if (axis == AxisEnum.xAxis) p.x = (float)v;
                if (axis == AxisEnum.yAxis) p.y = (float)v;
                if (axis == AxisEnum.zAxis) p.z = (float)v;
                pBlankedPoints.Add(p);
                return;
            }

            //else blanked value existed
            vec3 p0 = pBlankedPoints[index];
            double newv = v;
            if (axis == AxisEnum.xAxis) // x轴
            {
                if (IsValidX(p0.x))
                {
                    if (sort == AxisOrderEnum.Lower)//保留低值
                    {
                        if (newv > p0.x) newv = p0.x;
                    }
                    else if (sort == AxisOrderEnum.Upper)//保留高值
                    {
                        if (newv < p0.x) newv = p0.x;
                    }
                }
                p0.x = (float)newv;
            }
            else if (axis == AxisEnum.yAxis)// Y轴
            {
                if (IsValidY(p0.y))
                {
                    if (sort == AxisOrderEnum.Lower)//保留低值
                    {
                        if (newv > p0.y) newv = p0.y;
                    }
                    else if (sort == AxisOrderEnum.Upper)//保留高值
                    {
                        if (newv < p0.y) newv = p0.y;
                    }
                }
                p0.y = (float)newv;
            }
            else if (axis == AxisEnum.zAxis)// Z轴
            {
                if (IsValidZ(p0.z))
                {
                    if (sort == AxisOrderEnum.Lower)//保留低值
                    {
                        if (newv > p0.z) newv = p0.z;
                    }
                    else if (sort == AxisOrderEnum.Upper)//保留高值
                    {
                        if (newv < p0.z) newv = p0.z;
                    }
                }
                p0.z = (float)newv;
            }

            pBlankedPoints[index] = p0;
        }
        public void SetBlankValue(int ix, int iy, int iz, float v, AxisEnum axis, AxisOrderEnum sort = AxisOrderEnum.Anyway)
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
                if ( IsBlankValue(pGridData[i]) ) continue;
                if ( k == 0) { minv = maxv = pGridData[i]; k++; }
                else
                {
                    if (pGridData[i] < minv) minv = pGridData[i];
                    if (pGridData[i] > maxv) maxv = pGridData[i];
                }                
            }
        }

        public void UpdateDataRange()
        {
            UpdateRange();
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
        public bool IsValidX(double v)
        {
            if (v >= minx && v <= maxx)
                return true;
            else return false;
        }
        public bool IsValidY(double v)
        {
            if (v >= miny && v <= maxy)
                return true;
            else return false;
        }
        public bool IsValidZ(double v)
        {
            if (v >= minz && v <= maxz)
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
        public long GetVerticIndexByPosition(double x, double y, double z)
        {
            int ix = (int)((x - minx + 0.01 * xStep) / xStep);
            int iy = (int)((y - miny + 0.01 * yStep) / yStep);
            int iz = (int)((z - minz + 0.01 * zStep) / zStep);
            return GetVerticIndex(ix, iy, iz);
        }
        public long GetVerticIndex(int ix, int iy, int iz)
        {
            return ix + iy * xNum + iz * xNum * yNum;
        }
        public long GetVerticIndex(int ix, int iy, int iz, int vert)
        {
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
        public Vector64 GetGridCoord(int ix, int iy, int iz)
        { 
            double x = minx + ix * xStep;
            double y = miny + iy * yStep;
            double z = minz + iz * zStep;
            return new Vector64(x, y, z);
        }
        public Vector64 GetGridCoord(long id)
        {
            int ix, iy, iz;
            GetXYZIndexFromIndex(id, out ix, out iy, out iz);
            return GetGridCoord(ix,iy,iz);
        }
        public Vector32 GetVerticCoord(int ix, int iy, int iz, int ver)
        {
            Vector32 p = new Vector32();
            p.X = (float)(minx + ix * xStep);
            p.Y = (float)(miny + iy * yStep);
            p.Z = (float)(minz + iz * zStep);
            long icur = ix + iy * xNum + iz * xyNum;
            switch (ver)
            {
                case 0:
                    break;
                case 1:
                    icur += 1;
                    p.X += (float)xStep;
                    break;
                case 2:
                    icur += 1;
                    icur += xyNum;
                    p.X += (float)xStep;
                    p.Z += (float)zStep;
                    break;
                case 3:
                    icur += xyNum;
                    p.Z += (float)zStep;
                    break;
                case 4:
                    icur += xNum;
                    p.Y += (float)yStep;
                    break;
                case 5:
                    icur += 1;
                    icur += xNum;
                    p.X += (float)xStep;
                    p.Y += (float)yStep;
                    break;
                case 6:
                    icur += 1;
                    icur += xNum;
                    icur += xyNum;
                    p.X += (float)xStep;
                    p.Y += (float)yStep;
                    p.Z += (float)zStep;
                    break;
                case 7:
                    icur += xyNum;
                    icur += xNum;
                    p.Y += (float)yStep;
                    p.Z += (float)zStep;
                    break;
            }
            return p;
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
        public bool CutWithSurface(C2DGridData data, int onaxis, bool keepup = false)
        {
            if (onaxis == 2) return CutWithZSurface(data, keepup);
            else return false;
        }
        /// <summary>
        /// Cut 3D grid with meshes on Z axis
        /// </summary>
        /// <param name="data">2D grid data</param>
        /// <param name="keepup">keep upper part</param>
        /// <param name="exchangeXY">change x and y</param>        
        /// <param name="zscale"></param>
        /// <returns></returns>
        public bool CutWithZSurface(C2DGridData data, bool keepup = false, bool exchangeXY = false, double offset = 0)
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
                    iz = (int)( (vz - minz) / zStep );

                    if (keepup) // blank below( 0 - iz )
                    {
                        if ( vz < minz || iz < 0) continue;
                        if (iz > zNum - 1) iz = zNum - 1;

                        //2--z轴，sort=2坐标轴方向，保留较高位置的值
                        SetBlankValue(ix, iy, iz, (float)vz, AxisEnum.zAxis, AxisOrderEnum.Upper);
                        for (int i = 0; i <= iz; i++)
                        {
                            id = ix + iy * xNum + i * xyNum;
                            blanked[id] = true;
                            SetBlankGrid(ix, iy, i);
                        }
                    }
                    else // keep lower，blank upper(iz - zNum）
                    {
                        if ( iz >= zNum - 1 ) continue;
                        if ( iz < -1 ) iz = -1;

                        //2--z轴，sort=1坐标轴方向，保留较低位置的值
                        if(iz >= 0)SetBlankValue(ix, iy, iz, (float)vz, AxisEnum.zAxis, AxisOrderEnum.Lower);

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
                            if (b0) SetBlankValue(id0, (float)vy, AxisEnum.yAxis, AxisOrderEnum.Upper);
                            else SetBlankValue(id0, (float)vy, AxisEnum.yAxis, AxisOrderEnum.Lower);
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
                            //vz1 = data.GetGridValue(ix+1,iy);

                            z0 = minz + iz * zStep;
                            l1 = vz - z0;
                            l2 = vz1 - z0;
                            if (l1 < 0) l1 = -l1;
                            if (l2 < 0) l2 = -l2;
                            vx = x0 + xStep * l1 / (l1 + l2);
                            if (b0) SetBlankValue(id0, (float)vx, AxisEnum.xAxis, AxisOrderEnum.Upper);
                            else SetBlankValue(id0, (float)vx, AxisEnum.xAxis, AxisOrderEnum.Lower);
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
        //grid data
        public float[] pGridData = null;
        public double m_blankvalue = C3DData.m_BlankedValue;

        //show state of each unit, 0-don't show,1-show,2-blend
        public byte[] pgridShowTable;
        public Int16[] pColorIndexTable;

        public List<UInt32XYZ> pShowIndexArray = new List<UInt32XYZ>();
        public List<CTriangle3f> pTriangleArray = new List<CTriangle3f>();
       
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
        private void Init()
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
        public C3DGridData(int nx, int ny, int nz)
        {
            Init();
            xNum = nx;
            yNum = ny;
            zNum = nz;
            xyNum = xNum * yNum;
            pGridData = new float[nx * ny * nz];
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
            return new ColorRGBA(ColorScale.GetColor(v));
        }
        public int GetColorIndex(double v)
        {
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
            return pGridData[GetVerticIndex(ix, iy, iz)];
        }
        public void SetGridValue(int ix, int iy, int iz,double val)
        {
            pGridData[GetVerticIndex(ix, iy, iz)] = (float)val;
        }
        public double GetGridValue(double x, double y, double z, bool interpolation = true, bool normalized = false)
        {
            //check is in blank area
            //if ( IsInBlankArea(x, y, z) ) return m_blankvalue;

            double v = 0;
            if (!interpolation) v = GetGridValueWithOutInterpolation(x, y, z);
            else v = GetGridValueWithInterpolation(x, y, z);
            if (normalized)
            {
                v = NormalizeValue(v);
            }
            return v;
        }
        /// <summary>
        /// 在网格边上进行插值 x 
        /// </summary>
        /// <param name="ix">网格单元ix</param>
        /// <param name="iy">网格单元iy</param>
        /// <param name="iz">网格单元iz</param>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetXEdgeGridValue(int ix, int iy, int iz, double x)
        {
            if (ix >= xNum - 1) return GetGridValue(xNum - 1, iy, iz);
            double x1 = minx + ix * xStep;
            double v1 = GetGridValue(ix, iy, iz);
            double v2 = GetGridValue(ix + 1, iy, iz);
            return v1 + (v2 - v1) * (x - x1) / xStep;
        }
        public double GetYEdgeGridValue(int ix, int iy, int iz, double y)
        {
            if (iy >= yNum - 1) return GetGridValue(ix, yNum - 1, iz);
            double y1 = miny + iy * yStep;
            double v1 = GetGridValue(ix, iy, iz);
            double v2 = GetGridValue(ix, iy + 1, iz);
            return v1 + (v2 - v1) * (y - y1) / yStep;
        }
        public double GetZEdgeGridValue(int ix, int iy, int iz, double z)
        {
            //最右侧单元节点
            if (iz >= zNum - 1) return GetGridValue(ix, iy, zNum - 1);
            double z1 = minz + iz * zStep;
            double v1 = GetGridValue(ix, iy, iz);
            double v2 = GetGridValue(ix, iy, iz + 1);
            return v1 + (v2 - v1) * (z - z1) / zStep;
        }

        /// <summary>
        /// 在六面体网格面中插值计算
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double GetXOYFaceGridValue(int ix, int iy, int iz, double x, double y)
        {
            if (ix >= xNum - 1 || iy >= yNum - 1)
            {
                throw new Exception("out of range.");
            }
            double y1 = miny + iy * yStep;
            double v1 = GetXEdgeGridValue(ix, iy, iz, x);
            double v2 = GetXEdgeGridValue(ix, iy + 1, iz, x);
            return v1 + (v2 - v1) * (y - y1) / yStep;
        }
        public double GetXOZFaceGridValue(int ix, int iy, int iz, double x, double z)
        {
            if (ix >= xNum - 1 || iz >= zNum - 1)
            {
                throw new Exception("out of range.");
            }
            double z1 = minz + iz * zStep;
            double v1 = GetXEdgeGridValue(ix, iy, iz, x);
            double v2 = GetXEdgeGridValue(ix, iy, iz + 1, x);
            return v1 + (v2 - v1) * (z - z1) / zStep;
        }
        public double GetYOZFaceGridValue(int ix, int iy, int iz, double y, double z)
        {
            if (iz >= zNum - 1 || iy >= yNum - 1)
            {
                throw new Exception("out of range.");
            }
            double z1 = minz + iz * zStep;
            double v1 = GetYEdgeGridValue(ix, iy, iz, y);
            double v2 = GetYEdgeGridValue(ix, iy, iz + 1, y);
            return v1 + (v2 - v1) * (z - z1) / zStep;
        }
        /// <summary>
        /// 获取网格内任意位置的值，插值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public double GetValueFromCubeGrid(double x, double y, double z)
        {
            int ix0 = (int)((x - minx) / xStep);
            int iy0 = (int)((y - miny) / yStep);
            int iz0 = (int)((z - minz) / zStep);
            int ix1, ix2, iy1, iy2, iz1, iz2;

            ix1 = ix2 = ix0;
            if (minx + ix0 * xStep - x > 0 //不在网格节点上
                 && ix0 < xNum - 1) //非最右侧网格
                ix2 = ix1 + 1;

            iy1 = iy2 = iy0;
            if (miny + iy0 * yStep - y > 0 && iy0 < yNum - 1)
                iy2 = iy1 + 1;

            iz1 = iz2 = iz0;
            if (minz + iz0 * zStep - z > 0 && iz0 < zNum - 1)
                iz2 = iz1 + 1;

            if (ix1 == ix2 && iy1 == iy2 && iz1 == iz2) return GetGridValue(ix0, iy0, iz0);

            return 0;
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
            int ix = (int)((x - minx + 0.01 * xStep) / xStep);
            int iy = (int)((y - miny + 0.01 * yStep) / yStep);
            int iz = (int)((z - minz + 0.01 * zStep) / zStep);
            if (ix < 0) ix = 0;
            if (iy < 0) iy = 0;
            if (iz < 0) iz = 0;
            if (ix >= xNum) ix = xNum - 1;
            if (iy >= yNum) iy = yNum - 1;
            if (iz >= zNum) iz = zNum - 1;
            return GetGridValue(ix, iy, iz);
        }

        public double GetGridValueWithInterpolation(double x, double y, double z)
        {
            if ( x < minx || y < miny || z < minz || 
                 x > maxx || y > maxy || z > maxz ) 
                return m_blankvalue;

            int ix = (int)((x - minx) / xStep);
            int iy = (int)((y - miny) / yStep);
            int iz = (int)((z - minz) / zStep);

            //网格节点位置
            double x1 = minx + ix * xStep;
            double y1 = miny + iy * yStep;
            double z1 = minz + iz * zStep;

            bool bx = false, by = false, bz = false;

            //在节点位置
            if (x - x1 < xStep * 0.0001) bx = true;
            if (y - y1 < yStep * 0.0001) by = true;
            if (z - z1 < zStep * 0.0001) bz = true;

            // if on the corner of the cube 在节点上
            if (bx && by && bz) return GetGridValue(ix, iy, iz);

            // if on the edge of the cube 在X边上
            if (!bx && by && bz)
            {
                return GetXEdgeGridValue(ix, iy, iz, x);
            }
            if (!by && bx && bz)//在Y边上
            {
                return GetYEdgeGridValue(ix, iy, iz, y);
            }
            if (!bz && bx && by)//在Z边上
            {
                return GetZEdgeGridValue(ix, iy, iz, z);
            }

            // if on the faces of the cube 在面上
            if (bx && !by && !bz)
            {
                return GetYOZFaceGridValue(ix, iy, iz, y, z);
            }
            if (by && !bx && !bz)
            {
                return GetXOZFaceGridValue(ix, iy, iz, x, z);
            }
            if (bz && !bx && !by)
            {
                return GetXOYFaceGridValue(ix, iy, iz, x, y);
            }
            //否则在网格内
            double v1 = GetXOYFaceGridValue(ix, iy, iz, x, y);
            double v2 = GetXOYFaceGridValue(ix, iy, iz + 1, x, y);
            return v1 + (v2 - v1) * (z - z1) / zStep;
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

        public bool SaveVersion10(BinaryWriter br)
        {
            float v;
            for (long i = 0; i < zNum * xyNum; i++)
            {
                v = pGridData[i];
                if (IsBlankedGrid(i)) v =(float)( minv - (maxv - minv) * 0.5 - 1);
                br.Write(v); 
            }
            return true;
        }
        public bool LoadVersion10(BinaryReader br)
        {
            //read data from stream
            for (long i = 0; i < zNum * xyNum; i++)
                pGridData[i] = br.ReadSingle();

            return true;
        }

        public bool SaveVersion11(BinaryWriter br)
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
        public bool LoadVersion11(BinaryReader br)
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
        public bool SaveVersion12(BinaryWriter br)//flag == "DSGF"
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

            return ret;
        }
        public bool LoadVersion12(BinaryReader br)//flag == "DSGF"
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

            return true;
        }

        //save to Geo3D Stream file
        public override bool SaveAs(BinaryWriter br)
        {
            try
            {               
                Write3DGridHeader(br, 12);
                SaveObjHeader(br);
                SaveVersion10(br);
                SaveVersion11(br);
                SaveVersion12(br);                
                return true;
            }
            catch (IOException e)
            {
                errMessage = "write file failed.\n" + e.Message;
                return false;
            }
        }//Save3DGridData
        //save to 3dgrid file
        public override bool SaveAs(string path)
        {
            try
            {
                BinaryWriter br;
                br = new BinaryWriter(new FileStream(path, FileMode.Create));
                Write3DGridHeader(br, 10);
                SaveVersion10(br);
                br.Close();
                return true;
            }
            catch (IOException e)
            {
                errMessage = "write file failed.\n" + e.Message;
                return false;
            }
        }//SaveAs(BinaryWriter br)          

        public bool LoadFrom10(BinaryReader br)
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
                errMessage = "Open file failed.\n" + e.Message;
                return false;
            }
        }
        public bool LoadFrom11(BinaryReader br)
        {
            try
            {  
                if (!LoadObjHeader(br))
                {
                    Clear();
                    return false;
                }

                if (!LoadVersion10(br))
                {
                    Clear();
                    return false;
                }
                InitTables();
                if (!LoadVersion11(br))
                {
                    Clear();
                    return false;
                }

                return true;
            }
            catch (IOException e)
            {
                errMessage = "Open file failed.\n" + e.Message;
                return false;
            }
        }
        public bool LoadFrom12(BinaryReader br)
        {
            try
            { 
                if (!LoadObjHeader(br))
                {
                    Clear();
                    return false;
                }
                if (!LoadVersion10(br))
                {
                    Clear();
                    return false;
                }
                InitTables();
                if (!LoadVersion11(br))
                {
                    Clear();
                    return false;
                }
                if (!LoadVersion12(br))
                {
                    Clear();
                    return false;
                }
                return true;
            }
            catch (IOException e)
            {
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
            if (Version == 1.0f) return LoadFrom10(br);
            else if (Version == 1.1f) return LoadFrom11(br);
            else //if (version == 1.2f) 
                return LoadFrom12(br);

        }//bool LoadFrom(BinaryReader br)

        public override bool LoadFrom(string path)
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

                bool ret = false;
                if (version == 10) ret = LoadVersion10(br);
                else if (version == 11) ret = LoadVersion10(br);
                else if (version == 12) ret = LoadVersion12(br);
                else
                {
                    Clear();
                    errMessage = "not a valid 3D grid file.";
                    br.Close();
                    return false;
                }

                if (!ret)
                {
                    Clear();
                    br.Close();
                    return false;
                }
                
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

        }//bool Load3DGridData(string path)

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

            version = 0;
            if (tag == header1) version = 10;
            if (tag == header2) version = 11;
            if (tag == header3) version = 12;
            if (version < 1)
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
                return false;
            }
            xyNum = xNum * yNum;
            xStep = (maxx - minx) / (xNum - 1);
            yStep = (maxy - miny) / (yNum - 1);
            zStep = (maxz - minz) / (zNum - 1);            

            ColorScale.SetValueRange(minv, maxv);
            pGridData = new float[xyNum * zNum];  

            return true;
        }
        public bool Write3DGridHeader(BinaryWriter br, int _version = 12)
        {
            try 
            {                
                string header1 = "DSGD";//Version tag = "DSGD" 1.0
                string header2 = "DSGE";//Version tag = "DSGE" 1.1
                string header3 = "DSGF";//Version tag = "DSGF" 1.2     
                if(_version == 11) br.Write(header2.ToCharArray());
                else if (_version == 12) br.Write(header3.ToCharArray());
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
            long length = data1.pGridData.Length;
            if (data2.pGridData.Length != length) return 0; //网格不一致
            int count = 0;
            float v1, v2, v;
            for (long id = 0; id < length; id++)
            {
                v1 = data1.pGridData[id];
                v2 = data2.pGridData[id];
                if ( data2.IsBlankValue(v2) ) continue;

                if (reset) v2 = (float)layervalue; //地层值重置
                
                if ( method == 0 )v = v2;//直接替换
                else //平均
                {
                    if ( data1.IsBlankValue(v1) ) v = v2;
                    else v = (v1 + v2) / 2;
                }

                data1.pGridData[id] = v;
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
                p2 = data2.GetGridCoord(id2);
                
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
                            id1 = data1.GetVerticIndexByPosition(x,y,z);

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
                else filename = path + "\\" + obj.Name + ".ply";

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
                        d0.pGridData[id] = (float)v;
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
                        if (is_show) pgridShowTable[id] = 1;
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
    }
}
