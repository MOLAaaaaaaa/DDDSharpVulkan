using DataCollection;
using Graphics3D;
using KdTree;
using KdTree.Math;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenCLNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable SuggestBaseTypeForParameter
namespace CLInterpolation
{
    //one grid unit,contain points num and sum value;
    public struct GridPoints
    {
        public double sum;
        public int Count;
        public GridPoints(double v = 0, int n = 0)
        {
            sum = v;
            Count = n;
        }
        public void Add(double val)
        {
            sum += val;
            Count++;
        }
        public double GetValue()
        {
            if (Count > 0) return sum / Count;
            else return 0;
        }
        public void Clear()
        {
            Count = 0;
            sum = 0;
        }
    }

    /// <summary>
    /// 采样点进行网格剖分，分类
    /// </summary>
    public class GridPointsExt
    {        
        public List<int> Indices = new List<int>(); //网格内的点索引数组
        public int Count { get { return Indices.Count; } }
        public void Clear() { Indices.Clear();}
        public GridPointsExt()
        {            
        }
        public void Add(int id)
        {            
            Indices.Add(id);
        }               
        /// <summary>
        /// 得到网格内平均值
        /// </summary>
        /// <param name="Points"></param>
        /// <returns></returns>
        public double GetAverageValue(List<Vector32>Points)
        {            
            if ( Count < 1 ) return double.NaN;            
            double sum = 0;
            foreach (int i in Indices)
                sum += Points[i].V;
            return sum / Count;
        }
        /// <summary>
        /// 得到网格内加权插值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="Points"></param>
        /// <returns></returns>
        public double GetInterpolatedValue(double x, double y, double z, List<Vector64> Points)
        {
            if (Count < 1) return double.NaN;
            InversePower ip = new InversePower();            
            foreach (int i in Indices)
            {
                ip.AddPoint(Points[i]);
            }
            double val = ip.GetInterpolatedValue(x,y,z);
            ip.Clear();
            return val;
        }
        public double GetInterpolatedValue(double x, double y, double z, List<Vector32> Points)
        {
            if (Count < 1) return double.NaN;
            InversePower ip = new InversePower();
            foreach (int i in Indices)
            {
                ip.AddPoint( Points[i] );
            }
            double val = ip.GetInterpolatedValue(x, y, z);
            ip.Clear();
            return val;
        }
    }
    public struct DPoint
    {
        public double Value { get; }

        public double[] Coordinates { get; }

        public DPoint(double value, params double[] coordinates)
        {
            Value = value;
            Coordinates = coordinates;
        }

        public override string ToString()
        {
            return $"{string.Join(";", Coordinates)} -> {Value}";
        }
    }
    public struct InterpolateUnit 
    {
        public double x,y,z;
        public long id;
    }
    public class InterpolationResult
    {
        public enum ResultOptions
        {
            Hit,
            NearestNeighbor,
            Interpolated,
            Extrapolated,
            OutOfBounds
        }
        public ResultOptions Result { get; set; }
        public double Value { get; set; }
        public DPoint Point { get; set; }
    }
    public enum InterpolationMethod
    {
        InverseDistanceWeighted = 0,
        RadicalBasisFunction = 1,   //径向基函数，高斯法
        Linear = 2,
        GriddedInterpolation = 3,
        BoreholesMineralInterpolation = 5,   //钻孔矿体插值
        GeoProfilesGridding = 10,
        BoreholesPropertiesGridding = 20,
        //IDWLocal = 0, //反距离加权        
        //IDWGlobal = 1, //反距离加权        
        //Kriging = 3,//Kriging
    }
    public enum ResampleMethod
    {
        Average = 0,    //取平均值
        IDWInterpolate = 1,//距离加权插值
    }
    public class CLInterpolationUnit:CLUnit
    {  
        public long startTask = 0;        
        public int curRow = 0;
        public int startRow = 0;
        public int Row = 0;        
        public bool MaxAllocated = false; //已分配最大任务
        public double[] A = null;
        public double[] E = null;
        public double[] curLineA = null;//当前行数据
        public double[] curLineE = null;//当前行数据
        public float[] grid = null;
        public int threadID = -1;
        public bool Stop = false;
        public CLInterpolationUnit(Device _device):base(_device)
        {            
        }      
        public bool CreateMatrixAE(int row)
        {
            try
            {
                Row = row;
                A = new double[taskNumbers * row];
                E = new double[taskNumbers * row];
                return true;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return false;
            }
        }
        public void SetUnit(int _currow, int _startrow)
        {
            curRow = _currow;
            startRow = _startrow;
        }
        public void CopyMemoryFrom(double[] SA, double[] SE)
        {
            Array.Copy(SA, startRow * Row, A, 0, taskNumbers * Row);
            Array.Copy(SE, startRow * Row, E, 0, taskNumbers * Row);
        }
        public void CopyMemoryTo(ref double[] DA, ref double[] DE)
        {
            Array.Copy(A, 0, DA, startRow * Row, taskNumbers * Row);
            Array.Copy(E, 0, DE, startRow * Row, taskNumbers * Row);
        } 
        
        public override void ReleaseMemory()
        {
            curLineA = null;
            curLineE = null;
            A = null;
            E = null;
            grid = null;
        }
        public override void ReleaseCL()
        {
            if (oclDevice != null) oclDevice.Dispose();
            if (oclCQ != null) oclCQ.Dispose();
            if (oclContext != null) oclContext.Dispose();
            Kernels.Clear();
            functions.Clear();
        }

    }
    /// <summary>
    /// 钻孔柱状图插值，距离&角度约束
    /// </summary>
    public class CylinderInterpolator
    {
        public Vector64 V1 = new Vector64(), V2= new Vector64();
        CTriangle3f triangle3f = new CTriangle3f();
        public double AnglePower = 2; //阶次
        public double DistancePower = 1; //阶次
        public double MinimumDistance = 0;//最小钻孔间距
        public double MaximumDistance = 100;//最大钻孔间距
        float AnglePercent = 0.5f;
        float DistancePercent = 0.5f;
        public CylinderInterpolator()
        {
            V1 = new Vector64();
            V2 = new Vector64();
            triangle3f = new CTriangle3f(V1, V2, new Vector64());
        }
        public CylinderInterpolator(Vector64 v1,Vector64 v2)
        {
            V1 = v1;
            V2 = v2;
            triangle3f = new CTriangle3f(v1,v2,new Vector64());
        }

        public double GetInterpolatedValue( Vector64 p )
        {
            triangle3f.p1 = V1;
            triangle3f.p2 = V2;
            triangle3f.p3 = p;
            //三角形内角[0,180]
            double angle = triangle3f.GetAngle(2);
            if ( angle <= 1e-6 ) return 0;
            if (Math.Abs(angle - Math.PI) <= 1e-6) return 180;
            return Vector64.toAngle(angle);            
        }
        public double GetInterpolatedOnDist(Vector64 p)
        {
            triangle3f.p1 = V1;
            triangle3f.p2 = V2;
            triangle3f.p3 = p;
            double err = 1e-6;
            //三角形内角[0,180]
            double angle = triangle3f.GetAngle(2);
            if (angle <= 1e-6) return 0;
            if (Math.Abs(angle - Math.PI) <= 1e-6) return 180;

            double dist = p.Distance((V1 + V2) / 2);
            dist = Math.Min(p.Distance(V1), dist);
            dist = Math.Min(p.Distance(V2), dist);

            double scale = (dist - MinimumDistance) / (MaximumDistance - MinimumDistance);

            angle = (1 - scale) * angle;
            return Vector64.toAngle(angle);
            
        }
    }
    public class InterpolatorBase
    {
        public bool NearestValueOnly = false;//是否启用插值还是最近点
        public const double MINE = 5e-4;
        [CategoryAttribute("插值算法"), DisplayNameAttribute("插值算法"),ReadOnly(true)]
        public InterpolationMethod method { get; set; } = InterpolationMethod.InverseDistanceWeighted;
        public string progressTitle;
        public DateTime startTime;
        public double timeSlip = 0; //time have spent (sec)实际耗费时间
        public double timePerStep = 0;//time (sec)每一步运行时间
        public double timeLeft = 0;//time left  (sec)剩余时间
        public double percentage = 0;

        [CategoryAttribute("大数据网格"), DisplayNameAttribute("启用")]
        public bool BigGridData { get; set; } = false; //是否大数据网格
        [CategoryAttribute("大数据网格"), DisplayNameAttribute("内存分块数")]
        public int DividedNum { get; set; } = 1; //内存分块数
        [CategoryAttribute("数据保存"), DisplayNameAttribute("网格文件"), ReadOnly(true)]
        public string gridDataFile { get; set; } = "";

        public double mindist = 0;
        public double maxdist = 0;

        public List<Vector32> points = new List<Vector32>();
        public int xGrid = 50, yGrid = 50, zGrid = 50;

        public double minx_org, maxx_org, miny_org, maxy_org, minz_org, maxz_org, minv_org, maxv_org;
        public double minx, maxx, miny, maxy, minz, maxz, minv, maxv; //geometry
        public double xstep, ystep, zstep;

        protected Vector64 normalizedOrg = new Vector64();//坐标圆点
        protected double normalizedScale = 1;    //变换比例

        public string errMsg = "";
        public const double ZeroValue = 1.0E-18; //极小值        
        public bool Normalized = false;
        public bool threadStoped = false;        
        //GPU devices
        public CLInterpolationUnit[] clDevices = null;

        public string progressFile = "";
        public int progressStep = 0;   //计算进程 0，未开始，1完成距离矩阵，2完成矩阵求逆，3完成伴随矩阵，4完成grid计算
        public int progressRow = 0;    //计算进程的当前行   
       // public double progressPercent = 0;  //当前进度

        //进程共享锁
        protected Object lock1 = new Object();
        protected Object lock2 = new Object();
        protected Object lock3 = new Object();
        protected Object lock4 = new Object();

        //插值结果，三维网格数据
        //public C3DGridData griddedData = null;
        public float[] grid3d = null;
        public float[] px = null; //点数组
        public float[] py = null;
        public float[] pz = null;
        public float[] pv = null;

        //将points数据采样到网格节点
        public GridPointsExt[] gridPoints = null;
        public int gridNX, gridNY, gridNZ;
        public double gridXStep, gridYStep, gridZStep;
        public bool EnableFloatNum = false;
        public int floatNum = 6;

        //任务池，用于指定当前任务
        public Queue<TaskPoolIndices> taskPools = new Queue<TaskPoolIndices>();
        public struct TaskPoolIndices
        {
            public long start;  //当前任务位置
            public int num;     //任务长度            
            public int memindex;//内存起始位置            
            public TaskPoolIndices(long _start, int _num,int _memindex =0)
            {
                start = _start;
                num = _num;
                memindex = _memindex;
            }
        }
        public virtual void Dispose()
        {
            points.Clear();
            grid3d = null;
            gridPoints = null;
        }
        //创建任务队列
        public virtual int CreateGriddingTaskPools(long total, int batch = 100)
        {
            long cur = 0;
            int num;
            taskPools.Clear();
            while (cur < total)
            {
                if (total - cur >= batch) num = batch;
                else num = (int)(total - cur);
                taskPools.Enqueue(new TaskPoolIndices(cur, num));
                cur += num;
            }
            return taskPools.Count;
        }
        //从线程池获取一个任务
        public virtual TaskPoolIndices GetFromTaskPools()
        {
            lock (lock2)
            {
                if (taskPools.Count < 1) return new TaskPoolIndices(0, 0);
                else return taskPools.Dequeue();
            }
        }

        //从线程拷贝网格数据
        public virtual void CopyGridFromThread(int start, int len, float[] grid)
        {
            lock (lock1)
            {
                Array.Copy(grid, 0, grid3d, start, len);
            }
        }
        /// <summary>
        /// //创建点坐标及数组,px,py,pz,pv等
        /// </summary>
        /// <param name="convert">是否将坐标变换到指定区域</param>
        /// <param name="_min">x，y，z坐标最小值</param>
        /// <param name="_max">xyz坐标最大值</param>        
        /// <returns>false is failed </returns>
        public virtual bool CreatePointsMemory(bool convert = false, double _min = 0, double _max = 100)
        {
            if (px != null && py != null && pz != null) return true;
            int row = points.Count;
            if (row < 1)
            {
                errMsg = "no enough points.";
                return false;
            }

            try
            {
                px = new float[row];
                py = new float[row];
                pz = new float[row];
                pv = new float[row];
                if (convert)
                {
                    for (int i = 0; i < row; i++)
                    {
                        px[i] = (float)GetScaledX(points[i].x, _min, _max);
                        py[i] = (float)GetScaledY(points[i].y, _min, _max);
                        pz[i] = (float)GetScaledZ(points[i].z, _min, _max);
                        pv[i] = points[i].v;
                    }
                }
                else
                {
                    for (int i = 0; i < row; i++)
                    {
                        px[i] = points[i].x;
                        py[i] = points[i].y;
                        pz[i] = points[i].z;
                        pv[i] = points[i].v;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                errMsg = "allocating memory failed.no enough memory on CPU device.\n" + e.Message;
                return false;
            }
        }
        public virtual double GetScaledX(double x, double _min = 0, double _max = 100)
        {
            return _min + (x - minx) / MaxLength * (_max - _min);
        }
        public virtual double GetScaledY(double y, double _min = 0, double _max = 100)
        {
            return _min + (y - miny) / MaxLength * (_max - _min);
        }
        public virtual double GetScaledZ(double z, double _min = 0, double _max = 100)
        {
            return _min + (z - minz) / MaxLength * (_max - _min);
        }
        public virtual void ReleasePointsMemory()
        {
            px = py = pz = pv = null;
        }
        public virtual bool SaveProgress(string filename)//保存当前进程到文件
        {
            return true;
        }
        public virtual bool LoadProgress(string filename)//读取当前计算进程
        {
            return true;
        }
        public virtual bool InitDevices(Device[]devices)
        {
            clDevices = null;
            clDevices = new CLInterpolationUnit[devices.Length];
            for(int i=0;i<clDevices.Length;i++)
            {
                clDevices[i] = new CLInterpolationUnit(devices[i]);
            }
            return true;
        }
        /// <summary>
        /// 编译代码并初始化设备
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual bool CompileDevice(string source)
        {
            return false;
        }
        public virtual void ReleaseDevices()
        {            
            clDevices = null;            
        }

        public InterpolatorBase()
        {
            method = InterpolationMethod.InverseDistanceWeighted;
        }
        
        public double MaxLength
        {
            get
            {
                double xx = maxx - minx;
                double yy = maxy - miny;
                double zz = maxz - minz;
                double maxlen = xx;
                if (yy > maxlen) maxlen = yy;
                if (zz > maxlen) maxlen = zz;
                return maxlen;
            }
        }
        public double MinLength
        {
            get
            {
                double xx = maxx - minx;
                double yy = maxy - miny;
                double zz = maxz - minz;
                double minlen = xx;
                if (yy < minlen) minlen = yy;
                if (zz < minlen) minlen = zz;
                return minlen;
            }
        }

        public Vector32 toNormalized(Vector32 p)
        {
            double x = (p.X - normalizedOrg.X) / normalizedScale;
            double y = (p.Y - normalizedOrg.Y) / normalizedScale;
            double z = (p.Z - normalizedOrg.Z) / normalizedScale;
            return new Vector32(x, y, z, p.V);
        }
        public Vector32 UnNormalized(Vector32 p)
        {
            double x = normalizedScale * p.X + normalizedOrg.X;
            double y = normalizedScale * p.Y + normalizedOrg.Y;
            double z = normalizedScale * p.Z + normalizedOrg.Z;            
            return new Vector32(x, y,z, p.V);
        }
        public virtual void CopyFromWithOutPoints(InterpolatorBase ip)
        {
            xGrid = ip.xGrid;
            yGrid = ip.yGrid;
            zGrid = ip.zGrid;
            BigGridData = ip.BigGridData;
            DividedNum = ip.DividedNum;
            gridDataFile = ip.gridDataFile;

            minx = ip.minx;
            maxx = ip.maxx;
            miny = ip.miny;
            maxy = ip.maxy;
            minz = ip.minz;
            maxz = ip.maxz;
            minx = ip.minx;
            maxv = ip.maxv;
            xstep = ip.xstep;
            ystep = ip.ystep;
            zstep = ip.zstep;
        }
        public virtual void CopyFrom(InterpolatorBase ip)
        {
            points = new List<Vector32>(ip.points);
            CopyFromWithOutPoints(ip);
        }
        public int pointCount { get { return points.Count; } }
        public virtual void SetGrid(int xn, int yn, int zn)
        {
            xGrid = xn;
            yGrid = yn;
            zGrid = zn;
            xstep = (maxx - minx) / (xGrid - 1);
            ystep = (maxy - miny) / (yGrid - 1);
            zstep = (maxz - minz) / (zGrid - 1);            
        }
        //点坐标单位化，将所有点坐标均缩放到-1,1范围内，方便后面的距离计算
        public void Normalize()
        {
            if (Normalized) return;
            if (points.Count < 1) return;
            double len = MaxLength;
            if (len <= 0) return;

            double x0 = (minx + maxx) / 2.0;
            double y0 = (miny + maxy) / 2.0;
            double z0 = (minz + maxz) / 2.0;
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                p.x = (float)(2.0 * (p.x - x0) / len);
                p.y = (float)(2.0 * (p.y - y0) / len);
                p.z = (float)(2.0 * (p.z - z0) / len);
                points[i] = p;
            }
            Normalized = true;
        }
        public virtual bool IsZero(double v, double zero = ZeroValue)
        {
            double v1 = v;
            if (v1 < 0) v1 = -v;
            if (v1 > zero) return false;
            else return true;
        }
        public virtual void SetGeometry(double _minx, double _maxx,
                                        double _miny, double _maxy,
                                        double _minz, double _maxz,
                                        double _xstep, double _ystep, double _zstep,
                                        int _nx, int _ny, int _nz)
        {
            minx = _minx;
            miny = _miny;
            minz = _minz;
            maxx = _maxx;
            maxy = _maxy;
            maxz = _maxz;
            xstep = _xstep;
            ystep = _ystep;
            zstep = _zstep;
            xGrid = _nx;
            yGrid = _ny;
            zGrid = _nz;
        }
        public string formatTime(double second)
        {
            int hours = (int)(second / 3600.0);
            double sec1 = second - hours * 3600.0;
            int minutes = (int)(sec1 / 60);
            double sec2 = Math.Round(sec1 - 60 * minutes, 0);
            string tmstr = hours + ":" + minutes + ":" + sec2;
            return tmstr;
        }

        //添加一个点到数组points中
        public virtual void AddPoint(double x, double y, double z, double value)
        {
            if (points.Count == 0)
            {
                minx = maxx = x;
                miny = maxy = y;
                minz = maxz = z;
                minv = maxv = value;
            }
            else
            {
                if (x < minx) minx = x;
                if (y < miny) miny = y;
                if (z < minz) minz = z;
                if (value < minv) minv = value;
                if (x > maxx) maxx = x;
                if (y > maxy) maxy = y;
                if (z > maxz) maxz = z;
                if (value > maxv) maxv = value;
            }
            //x = Math.Round(x, 2);
            //y = Math.Round(y, 2);
            //z = Math.Round(z, 2);
            points.Add(new Vector32((float)x, (float)y, (float)z, (float)value));
        }
        public virtual void AddPoints( List< Vector32 > _points, bool clearOld = true)
        {
            if(clearOld)points.Clear();
            points = new List<Vector32>(_points);
        }
        public virtual void AddPoints(Vector32[]_points, bool clearOld = true)
        {
            if (clearOld) points.Clear();
            points = new List<Vector32>(_points);
        }
        public virtual void AddPoint(XYZPoint p)
        {
            AddPoint(p.x, p.y, p.z, p.v);
        }
        public virtual void AddPoint(Vector32 p)
        {
            AddPoint(p.x, p.y, p.z, p.v);
        }
        public virtual void AddPoint(Vector64 p)
        {
            AddPoint(p.x, p.y, p.z, p.v);
        }
        
        public virtual void UpdatePointsRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;
            int k = 0;
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if ( C3DData.IsBlankValue(p.V) ) continue;
                if (k == 0)
                {
                    minx = maxx = p.x;
                    miny = maxy = p.y;
                    minz = maxz = p.z;
                    minv = maxv = p.v;
                    k++;
                }
                else
                {
                    if (p.x > maxx) maxx = p.x;
                    if (p.x < minx) minx = p.x;
                    if (p.y > maxy) maxy = p.y;
                    if (p.y < miny) miny = p.y;
                    if (p.z > maxz) maxz = p.z;
                    if (p.z < minz) minz = p.z;
                    if (p.v > maxv) maxv = p.v;
                    if (p.v < minv) minv = p.v;
                }
            }
            if (xGrid > 1) xstep = (maxx - minx) / (xGrid - 1);
            if (yGrid > 1) ystep = (maxy - miny) / (yGrid - 1);
            if (zGrid > 1) zstep = (maxz - minz) / (zGrid - 1);
        }
        public virtual void Clear()
        {
            points.Clear();
            gridPoints = null;
            taskPools.Clear();
            ReleasePointsMemory();
        }

        /// <summary>
        /// 创建网格数据结构，将每个点划分到每个网格节点上
        /// 网格大小 (nx-1)*(ny-1)*(nz-1)
        /// </summary>
        /// <param name="nx">X向网格数目，含节点</param>
        /// <param name="ny">Y向网格数目，含节点</param>
        /// <param name="nz">Z向网格数目，含节点</param>
        /// <returns></returns>
        public bool CreateGridExt(int nx, int ny, int nz)
        {
            try
            {
                double _xstep = (maxx - minx) / (nx - 1);
                double _ystep = (maxy - miny) / (ny - 1);
                double _zstep = (maxz - minz) / (nz - 1);

                gridPoints = null;
                gridPoints = new GridPointsExt[(nx-1)*(ny-1)*(nz-1)];
                for ( int i = 0; i < gridPoints.Length; i++ )
                    gridPoints[i] = new GridPointsExt();

                int ix, iy, iz;
                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    ix = (int)( (p.x - minx) / _xstep);
                    iy = (int)( (p.y - miny) / _ystep);
                    iz = (int)( (p.z - minz) / _zstep);

                    if (ix >= nx - 2) ix = nx - 2;
                    if (iy >= ny - 2) iy = ny - 2;
                    if (iz >= nz - 2) iz = nz - 2;
                    gridPoints[ix + iy * (nx-1) + iz * (nx-1) * (ny-1)].Add(i);
                }

                return true;
            }
            catch (Exception e)
            {
                errMsg = "Create grids failed." + e.Message;
                return false;
            }
        }
        public void ClearGridExt()
        {
            if( gridPoints!= null )
            {
                for (int i = 0; i < gridPoints.Length; i++)
                    gridPoints[i].Clear();
                gridPoints = null;
            }
        }
        /// <summary>
        /// 网格重采样
        /// </summary>
        /// <param name="nx">X向网格数目，含节点</param>
        /// <param name="ny">Y向网格数目，含节点</param>
        /// <param name="nz">Z向网格数目，含节点</param>
        /// <param name="method">网格内插值方法</param>
        /// <returns></returns>
        public virtual bool ResamplePoints(int nx, int ny, int nz, InterpolationMethod method = InterpolationMethod.Linear)
        {
            if ( !CreateGridExt(nx, ny, nz) ) 
            {
                errMsg += "Resample failed.";
                return false; 
            }
            try 
            {
                double _xstep = (maxx - minx) / (nx-1);
                double _ystep = (maxy - miny) / (ny-1);
                double _zstep = (maxz - minz) / (nz-1);
                double x, y, z,v;
                
                List<Vector32> points1 = new List<Vector32>();

                for(int iz = 0; iz < nz-1; iz ++)
                {
                    for (int iy = 0; iy < ny - 1; iy++)
                    {
                        for (int ix = 0; ix < nx - 1; ix++)
                        {
                            x = minx + _xstep;
                            y = miny + _ystep;
                            z = minz + _zstep;
                            v = gridPoints[ix + iy * (nx - 1) + iz * (nx - 1) * (ny - 1)].GetInterpolatedValue(x,y,z,points);
                            points1.Add(new Vector32(x,y,z,v));
                        }
                    }
                }
                points.Clear();
                points = points1;
                ClearGridExt();
                UpdatePointsRange();
                return true;
            }
            catch(Exception e)
            {
                errMsg = "Resample failed." + e.Message;
                return false;
            }            
        }        

        public virtual bool Filter(bool isnullx, double nullx,
                                        bool trimx1, double x1,
                                        bool trimx2, double x2,
                                        bool isnully, double nully,
                                        bool trimy1, double y1,
                                        bool trimy2, double y2,
                                        bool isnullz, double nullz,
                                        bool trimz1, double z1,
                                        bool trimz2, double z2,
                                        bool isnullv, double nullv,
                                        bool trimv1, double v1,
                                        bool trimv2, double v2)
        {
            if (points.Count < 1) return false;
            try
            {
                List<Vector32> points1 = new List<Vector32>();
                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];

                    if (isnullx) if (p.x == nullx) continue;
                    if (trimx1) if (p.x < x1) continue;
                    if (trimx2) if (p.x > x2) continue;

                    if (isnully) if (p.y == nully) continue;
                    if (trimy1) if (p.y < y1) continue;
                    if (trimy2) if (p.y > y2) continue;

                    if (isnullz) if (p.z == nullz) continue;
                    if (trimz1) if (p.z < z1) continue;
                    if (trimz2) if (p.z > z2) continue;

                    if (isnullv) if (p.v == nullv) continue;
                    if (trimv1) if (p.v < v1) continue;
                    if (trimv2) if (p.z > v2) continue;

                    points1.Add(p);
                }
                points.Clear();
                points = points1;
                return true;
            }
            catch (Exception e)
            {
                errMsg = "Filter failed.\n" + e.Message;
                return false;
            }
        }

        //数据过滤，过滤重复数据 
        public virtual int RemoveDuplicated(double zerobase = 1E-6)
        {
            if (points.Count < 2) return 0;
            double dist;
            Vector32 p, p1, p2;
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

            //double zero = maxlen * zerobase;
            double zero = zerobase;

            progressTitle = "重复点检查...";

            bool[] del = new bool[points.Count];
            for (int i = 0; i < points.Count; i++) del[i] = false;

            DateTime t1 = DateTime.Now;
            double sec = 0;
            percentage = 0;
            int step = points.Count / 100;
            if (step < 1) step = 1;
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    p1 = points[i];
                    p2 = points[j];
                    dist = Math.Abs(p1.x - p2.x) +
                           Math.Abs(p1.y - p2.y) +
                           Math.Abs(p1.z - p2.z);
                    if (dist <= zero)
                    {
                        del[j] = true;
                    }
                }
                if (i == 0)
                {
                    sec = (DateTime.Now - t1).TotalSeconds;
                    //timeLeft = points.Count * sec;
                }
                else if (i % step == 0)
                {
                    timeLeft = (points.Count - i - 1) * sec;
                    timeSlip += (i + 1) * sec;
                    percentage = (i + 1) * 100.0 / points.Count;
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

        public virtual ulong GetRequiredMemorySizeOnCPU()
        {
            return 0;
        }
        public ulong GetGridMemorySize()
        {
            ulong n = (ulong)pointCount;
            ulong msize = 5 * n * sizeof(float); //points,px,py,pz,pv
            ulong ugrid = (ulong)(xGrid * yGrid * zGrid) * sizeof(float); //grid3d[]
            return msize + ugrid;
        }
        public virtual double GetInterpolatedValue(double x, double y, double z)
        {
            return double.NaN;
        }

        public virtual float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {
            return null;
        }
        public virtual float[] GetInterpolatedValueCPU(int xn, int yn, int zn)
        {
            return null;
        }
       
        public Int32XYZ GetIndices(ulong id, int nx, int ny, int nz)
        {
            ulong xy = (ulong)nx * (ulong)ny;
            Int32XYZ xyz = new Int32XYZ();
            xyz.z = (int)(id / xy);
            ulong left = id % xy;
            xyz.y = (int)(left / (ulong)nx);
            xyz.x = (int)(left % (ulong)nx);
            return xyz;
        }
        //直接网格化，不需插值
        public virtual float[] DirectGridding(int xn, int yn, int zn)
        {
            try
            {
                percentage = 0;
                progressTitle = "正在插值计算...";
                int ix, iy, iz;

                double sec = 0;
                double step = 100 / points.Count;
                int nstep = (int)step;
                if (nstep < 1) nstep = 1;

                grid3d = new float[xn * yn * zn];
                for (int i = 0; i < grid3d.Length; i++) grid3d[i] = (float)minv;

                DateTime t1 = DateTime.Now;
                DateTime t2 = t1;
                for (int i = 0; i < points.Count; i++)
                {
                    ix = ConvertData.Double2Int((points[i].x - minx) / xstep);
                    iy = ConvertData.Double2Int((points[i].y - miny) / ystep);
                    iz = ConvertData.Double2Int((points[i].z - minz) / zstep);
                    grid3d[ ix + iy * xn + iz * xn * yn ] = points[i].v;    

                    if (i % nstep == 0 )
                    {
                        t2 = DateTime.Now;
                        sec = ( t2 - t1).TotalSeconds;
                        timeLeft = (points.Count - i - 1) * sec;
                        percentage += step * (i+1);
                        t1 = t2;
                    } 
                }
                return grid3d;
            }
            catch(Exception ex)
            {
                errMsg = "gridding failed.\n" + ex.Message;
                return null;
            }
         }        

    }//InterpolatorBase
     //————class base——————————————————————————

    /// <summary>
    /// Inverse Distance Weighting (IDW) interpolator. 
    /// </summary>
    /// <remarks>
    /// The interpolator implements the modified Shepard's method, with only nearest neighbours.
    /// https://en.wikipedia.org/wiki/Inverse_distance_weighting
    /// </remarks>
    public class IdwInterpolatorLocalKdTree : InterpolatorBase
    {
        private readonly int _dimensions;
        private const double DefaultPower = 2;
        private readonly KdTree<double, DPoint> _tree;
        private double Power { get; }
        public int NumberOfNeighbours = 5;

        public override void Clear()
        {
            base.Clear();
            _tree.Clear();
        }
        public IdwInterpolatorLocalKdTree(int dimensions, 
                                          double power, 
                                          int numberOfNeighbours)
        {
            if (dimensions < 1)
                throw new ArgumentOutOfRangeException(nameof(dimensions), dimensions,
                    $"Parameter '{nameof(dimensions)}' must be a positive integer.");

            if (power <= 0)
                throw new ArgumentOutOfRangeException(nameof(power), power,
                    $"Parameter '{nameof(power)}' must be a positive real value.");

            if (numberOfNeighbours < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfNeighbours), numberOfNeighbours,
                    $"Parameter '{nameof(numberOfNeighbours)}' must be a positive integer.");

            _dimensions = dimensions;

            Power = power;
            NumberOfNeighbours = numberOfNeighbours;

            _tree = new KdTree<double, DPoint>(dimensions, new DoubleMath());
        }
        public void AddPoints(List<Vector32> _points)
        {
            foreach (Vector32 p in _points)
            {
                AddCoordinates(p.v, new double[] { p.x, p.y, p.z });
            }
        }
        public override void CopyFrom(InterpolatorBase ip)
        {
            base.CopyFrom(ip);
            foreach (Vector32 p in points)
            {
                AddCoordinates(p.v, new double[] { p.x, p.y, p.z });
            }
        }
        public override void AddPoint(double x, double y, double z, double value)
        {
            AddCoordinates(value, new double[] { x, y, z });
        }
        public void AddCoordinates(double value, params double[] coordinates)
        {
            if (coordinates == null)
                throw new ArgumentNullException(nameof(coordinates));

            if (coordinates.Length != _dimensions)
                throw new ArgumentException(nameof(coordinates),
                    $"Size of {nameof(coordinates)} must match the dimension of the interpolator.");

            var point = new DPoint(value, coordinates);

            _tree.Add(coordinates, point);
        }

        public void AddPoint(DPoint point)
        {
            if (point.Coordinates.Length != _dimensions)
                throw new ArgumentException(nameof(point.Coordinates),
                    $"Size of {nameof(point.Coordinates)} must match the dimension of the interpolator.");

            _tree.Add(point.Coordinates, point);
        }

        public void AddPointRange(IEnumerable<DPoint> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            foreach (var point in points)
            {
                if (point.Coordinates.Length != _dimensions)
                {
                    _tree.Clear();

                    throw new ArgumentException(nameof(points),
                        $"Size of coordinates of all items in {nameof(points)} must match the dimension of the interpolator.");
                }

                _tree.Add(point.Coordinates, point);
            }
        }
        public override ulong GetRequiredMemorySizeOnCPU()
        {
            ulong row = (ulong)points.Count;
            ulong umatrix = row * sizeof(double);
            return umatrix + GetGridMemorySize();
        }
        public Vector64 FindNearestPoint(double x, double y, double z)
        {
            double[] coordinates = new double[] { x, y, z };
            if (_tree.Count < NumberOfNeighbours)
            {
                throw new IndexOutOfRangeException(
                    $"The number of found points ({_tree.Count}) is less than " +
                    $"the number of required neighbours ({NumberOfNeighbours}). Consider reducing " +
                    $"the required number of Neighbours with '{nameof(NumberOfNeighbours)}' property.");
            }

            DPoint point;
            if (_tree.TryFindValueAt(coordinates, out point))
            {
                return new Vector64(point.Coordinates[0], 
                                    point.Coordinates[1], 
                                    point.Coordinates[2], 
                                    point.Value);                
            }

            var neighbours = _tree.GetNearestNeighbours(coordinates, NumberOfNeighbours);
            if (neighbours.Length == 1)
            {
                point = neighbours[0].Value;
                return new Vector64(point.Coordinates[0],
                                    point.Coordinates[1],
                                    point.Coordinates[2],
                                    point.Value);
            }
            return FindNearestPoint(neighbours.Select(n => n.Value), coordinates);
        }

        public override double GetInterpolatedValue(double x, double y, double z)
        {
            InterpolationResult ret = Interpolate(new double[] { x, y, z });
            return ret.Value;
        }

        public override float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z, val;
            long xy = xn * yn;
            long id = 0;

            try
            {
                grid3d = new float[xy * zn];

                double sec = 0;
                DateTime t1 = DateTime.Now;

                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + zstep * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + ystep * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + xstep * ix;
                            id = iz * xy + iy * xn + ix;
                            val = GetInterpolatedValue(x, y, z);
                            if (double.IsNaN(val)) grid3d[id] = CSurferGrid.blankValuefloat;
                            else grid3d[id] = (float)val;                            
                        }
                    }
                    if (iz == 0)
                    {
                        sec = (DateTime.Now - t1).TotalSeconds;
                    }
                    //if (k % step == 0)
                    {
                        timeLeft = (zn - iz - 1) * sec;
                        timeSlip += (iz + 1) * sec;
                        percentage = (double)(iz + 1) * 100 / zn;
                    }
                }
                percentage = 100;
                return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                return null;
            }
        }
        public InterpolationResult Interpolate(params double[] coordinates)
        {
            if (coordinates.Length != _dimensions)
                throw new ArgumentException(nameof(coordinates),
                    $"Size of {nameof(coordinates)} must match the dimension of the interpolator.");

            if (_tree.Count < NumberOfNeighbours)
            {
                throw new IndexOutOfRangeException(
                    $"The number of found points ({_tree.Count}) is less than " +
                    $"the number of required neighbours ({NumberOfNeighbours}). Consider reducing " +
                    $"the required number of Neighbours with '{nameof(NumberOfNeighbours)}' property.");
            }

            DPoint point;

            if (_tree.TryFindValueAt(coordinates, out point))
            {
                return new InterpolationResult
                {
                    Point = point,
                    Value = point.Value,
                    Result = InterpolationResult.ResultOptions.Hit
                };
            }

            var neighbours = _tree.GetNearestNeighbours(coordinates, NumberOfNeighbours);

            if (neighbours.Length == 1)
            {
                point = neighbours[0].Value;

                return new InterpolationResult
                {
                    Point = point,
                    Value = point.Value,
                    Result = InterpolationResult.ResultOptions.NearestNeighbor
                };
            }

            var value = CalculateWeightedAverage(neighbours.Select(n => n.Value), coordinates);

            var result = new InterpolationResult
            {
                Value = value,
                Result = InterpolationResult.ResultOptions.Interpolated
            };

            if (false) // TODO: Check convex hull here.
            {
                result.Result = InterpolationResult.ResultOptions.Extrapolated;
            }

            return result;
        }
        private Vector64 FindNearestPoint(IEnumerable<DPoint> points, double[] target)
        {
            double dx, dy, dz;
            double dist = 0, mindist = 1E10;
            Vector64 p = new Vector64();
            foreach (var point in points)
            {
                dx = point.Coordinates[0] - target[0];
                dy = point.Coordinates[1] - target[1];
                dz = point.Coordinates[2] - target[2];
                dist = dx * dx + dy * dy + dz * dz;
                if ( dist < maxdist ) 
                {
                    mindist = dist;
                    p.X = point.Coordinates[0];
                    p.Y = point.Coordinates[1];
                    p.Z = point.Coordinates[2];
                    p.V = point.Value;
                }
            }
            return p;
        }
        private double CalculateWeightedAverage(IEnumerable<DPoint> points, double[] target)
        {
            var nominator = 0.0;
            var denominator = 0.0;

            foreach (var point in points)
            {
                var distance = CalculateDistance(point.Coordinates, target);
                var weight = 1 / Math.Pow(distance, Power);

                nominator += weight * point.Value;
                denominator += weight;
            }

            var result = nominator / denominator;

            return result;
        }

        private static double CalculateDistance(double[] pointA, double[] pointB)
        {
            var sum = 0.0;

            for (var i = 0; i < pointA.Length; i++)
            {
                var p = pointA[i];
                var q = pointB[i];

                sum += Math.Pow(p - q, 2);
            }

            var result = Math.Sqrt(sum);

            return result;
        }
    }//class IdwInterpolatorLocal
    
    //反距离加权插值-global
    public class IdwInterpolatorGlobal : InterpolatorBase
    {
        /*--------反距离插值函数------------------------
        //输入数据:插值点位置x,y,z
        //散点坐标数组points，需要插值前输入到points数组
        //返回值：插值点值value
        -------------------------------------------------*/
        public float Power { get; set; } = 2;
        public double[] distances = null;
        public IdwInterpolatorGlobal()
        {
            method = InterpolationMethod.InverseDistanceWeighted;
        }
        public override ulong GetRequiredMemorySizeOnCPU()
        {
            ulong row = (ulong)points.Count;
            ulong umatrix = row * sizeof(double);
            return umatrix + GetGridMemorySize();
        }
        double GetValueWithInterpolation(double x, double y, double z)
        {
            //距离数组
            if (distances == null) distances = new double[points.Count];
            double dist = 0;
            double fenmu = 0;
            double weight = 0;
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                //距离平方
                dist = (x - p.x) * (x - p.x) +
                        (y - p.y) * (y - p.y) +
                        (z - p.z) * (z - p.z);

                if (dist == 0) return points[i].v;

                if (Power == 1) weight = 1.0 / Math.Sqrt(dist);
                else if (Power == 2) weight = 1.0 / dist;
                else weight = 1.0 / Math.Pow(dist, Power / 2);

                fenmu += weight;
                distances[i] = weight;
            }//for (int i = 0; i < points.Count; i++)

            double value = 0;
            for (int i = 0; i < points.Count; i++)
            {
                //计算权重系数,加权
                value += (points[i].v * distances[i] / fenmu);
            }
            distances = null;
            return value;
        }
        double GetValueWithoutInterpolation(double x, double y, double z)
        {
            double dist, mindist = 1E10;
            int id = -1;
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                //距离平方
                dist =  (x - p.x) * (x - p.x) +
                        (y - p.y) * (y - p.y) +
                        (z - p.z) * (z - p.z);
                if (dist == 0) return points[i].v;
                if (dist < mindist) { mindist = dist; id = i;}
            }//for (int i = 0; i < points.Count; i++)
            if (id >= 0) return points[id].V;
            else return 0;            
        }

        public override double GetInterpolatedValue(double x, double y, double z)
        {
            if( NearestValueOnly )return GetValueWithInterpolation(x, y, z);
            else return GetValueWithoutInterpolation(x, y, z);
        }

        /// <summary>
        /// 反距离加权全局插值
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        public override float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z,val;
            long xy = xn * yn;
            long id = 0;
            int sectionNum = 0;
            Queue<InterpolateUnit> tasks = new Queue<InterpolateUnit>();
            C3DGridDataStream gridStream = null;
            try
            {
                distances = new double[points.Count];

                if (!BigGridData) grid3d = new float[xy * zn];
                else 
                {
                    sectionNum = (int)(xy * zn / (float)DividedNum) + 1;
                    grid3d = new float[sectionNum];
                    gridStream = new C3DGridDataStream();
                    gridStream.Create(gridDataFile, xn, yn, zn,minx,miny,minz,minv,maxx,maxy,maxz,maxv);
                }

                double sec = 0;
                DateTime t1 = DateTime.Now;
                int index = 0, section = 0;
                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + zstep * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + ystep * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + xstep * ix;
                            id = iz * xy + iy * xn + ix;
                            val = GetInterpolatedValue(x, y, z);

                            if (BigGridData && index >= sectionNum)
                            {
                                gridStream.WriteData(grid3d, 0, index);
                                if (double.IsNaN(val)) grid3d[0] = CSurferGrid.blankValuefloat;
                                else grid3d[0] = (float)val;
                                section++;
                            }
                            else
                            {
                                if (double.IsNaN(val)) grid3d[index] = CSurferGrid.blankValuefloat;
                                else grid3d[index] = (float)val;
                            }

                            index++;
                        }
                    }

                    if (iz == 0)
                    {
                        sec = (DateTime.Now - t1).TotalSeconds;
                    }

                    //if (k % step == 0)
                    {
                        timeLeft = (zn - iz - 1) * sec;
                        timeSlip += (iz + 1) * sec;
                        percentage = (double)(iz + 1) * 100 / zn;
                    }
                }

                if (BigGridData && index > 0)//最后一段数据
                {
                    gridStream.WriteData(grid3d, 0, index);                    
                }

                if (BigGridData) 
                {
                    if (!gridStream.Close())
                    {
                        grid3d = null;
                        errMsg = gridStream.errMessage;
                    }
                }
                distances = null;
                percentage = 100;
                return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                return null;
            }
        }
        public override void Clear()
        {
            base.Clear();
            distances = null;
        }
    }//end class global
    public class IdwStratumInterpolatorGlobal : InterpolatorBase
    {
        /*--------反距离加权插值 地层插值------------------------
        //输入数据:插值点位置x,y,z
        //散点坐标数组points，需要插值前输入到points数组
        //返回值：插值点值value
        -------------------------------------------------*/
        public float Power { get; set; } = 2;
        
        public StratumDatas Stratums = new StratumDatas();
        public double zero = 1.0E-6;
        public IdwStratumInterpolatorGlobal(StratumDatas stratums)
        {
            method = InterpolationMethod.InverseDistanceWeighted;
            Stratums = stratums.Copy();           
        }
        bool IsZero(double value) 
        {
            return Math.Abs(value) <= zero;
        }
        public override ulong GetRequiredMemorySizeOnCPU()
        {
            ulong row = (ulong)points.Count;
            ulong umatrix = row * sizeof(double);
            return umatrix + GetGridMemorySize();
        }
        int GetValueWithInterpolation(double x, double y, double z)
        {            
            if ( Stratums.Count < 1 ) return -1;
            int id = -1;
            Vector32 p;
            double dist = 0;
            //double fenmu = 0;
            double weight = 0;
            double []weights = new double[Stratums.Count + 1];            
            for (int i = 0; i < weights.Length; i++) weights[i] = 0;
            
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (p.V < 0) id = -1;//地层编号
                else id = (int)(p.V + 0.1);//地层编号
                //距离平方
                dist =  (x - p.x) * (x - p.x) +
                        (y - p.y) * (y - p.y) +
                        (z - p.z) * (z - p.z);

                if (IsZero(dist)) { weights = null; return id; }
                
                if (Power == 2) weight = 1.0 / Math.Sqrt(dist);
                else if (Power == 1) weight = 1.0 / dist;
                else weight = 1.0 / Math.Pow(dist, Power / 2);
                //fenmu += weight;
                weights[id + 1 ] += weight;
            }//for (int i = 0; i < points.Count; i++)

            id = -1; weight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if( weights[i] > weight) {  weight = weights[i]; id = i - 1; }
            }
            weights = null;
            return id;
        }

        int GetValueWithoutInterpolation(double x, double y, double z)
        {
            double dist, mindist = 1E10;
            int curid = -1,id = -1;
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (p.V < 0) id = -1;
                else id = (int)(p.V+0.1);
                //距离平方
                dist = (x - p.x) * (x - p.x) +
                        (y - p.y) * (y - p.y) +
                        (z - p.z) * (z - p.z);
                if (IsZero(dist)) return id;
                if (dist < mindist) { mindist = dist; curid = id; }
            }//for (int i = 0; i < points.Count; i++)
            return curid;
        }

        public override double GetInterpolatedValue(double x, double y, double z)
        {
            if (NearestValueOnly) return GetValueWithoutInterpolation(x, y, z); 
            else return  GetValueWithInterpolation(x, y, z);
        }

        /// <summary>
        /// 反距离加权全局插值
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        public override float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z, val;
            long xy = xn * yn;
            long id = 0;
            int sectionNum = 0;
            Queue<InterpolateUnit> tasks = new Queue<InterpolateUnit>();
            C3DGridDataStream gridStream = null;
            try
            {   
                if (!BigGridData) grid3d = new float[xy * zn];
                else
                {
                    sectionNum = (int)(xy * zn / (float)DividedNum) + 1;
                    grid3d = new float[sectionNum];
                    gridStream = new C3DGridDataStream();
                    gridStream.Create(gridDataFile, xn, yn, zn, minx, miny, minz, minv, maxx, maxy, maxz, maxv);
                }

                double sec = 0;
                DateTime t1 = DateTime.Now;
                int index = 0, section = 0;
                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + zstep * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + ystep * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + xstep * ix;
                            id = iz * xy + iy * xn + ix;
                            val = GetInterpolatedValue(x, y, z);

                            if (BigGridData && index >= sectionNum)
                            {
                                gridStream.WriteData(grid3d, 0, index);
                                if (double.IsNaN(val)) grid3d[0] = CSurferGrid.blankValuefloat;
                                else grid3d[0] = (float)val;
                                section++;
                            }
                            else
                            {
                                if (double.IsNaN(val)) grid3d[index] = CSurferGrid.blankValuefloat;
                                else grid3d[index] = (float)val;
                            }

                            index++;
                        }
                    }

                    if (iz == 0)
                    {
                        sec = (DateTime.Now - t1).TotalSeconds;
                    }

                    //if (k % step == 0)
                    {
                        timeLeft = (zn - iz - 1) * sec;
                        timeSlip += (iz + 1) * sec;
                        percentage = (double)(iz + 1) * 100 / zn;
                    }
                }

                if (BigGridData && index > 0)//最后一段数据
                {
                    gridStream.WriteData(grid3d, 0, index);
                }

                if (BigGridData)
                {
                    if (!gridStream.Close())
                    {
                        grid3d = null;
                        errMsg = gridStream.errMessage;
                    }
                }
                percentage = 100;
                return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                return null;
            }
        }
        public override void Clear()
        {
            base.Clear();
            points.Clear();
        }
    }//end class global
    //规则网格插值，直接网格化
    public class GriddedInterpolator : InterpolatorBase
    {
        //original data range
        public C3DGridData grid3d0 = null;
        [CategoryAttribute("搜索"), DisplayNameAttribute("网格半径")]
        public int SearchingGridLength { get; set; } = 0;

        public GriddedInterpolator()
        {
            method = InterpolationMethod.GriddedInterpolation;
        }       
        public override ulong GetRequiredMemorySizeOnCPU()
        {
            return GetGridMemorySize();
        }
       
        /// <summary>
        /// 从点中创建X方向的网格
        /// </summary>
        /// <param name="col"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public float[] CreateGridsFromPoints(int col, double err = 1e-6 )
        {
            float v;            
            List<float> xx = new List<float>();
            foreach(Vector32 p in points )
            {
                if (col == 0) v = p.x; // - minx;
                else if (col == 1) v = p.y; // - miny;
                else v = p.z; // - minz;
                xx.Add(v);
            }
            xx.Sort();
            if ( xx.Count < 1 ) return null;

            List<float> xgrids = new List<float>();
            float x = xx[0];
            xgrids.Add(x);
            for(int i = 1; i < xx.Count; i++)
            {
                if ( xx[i] > x ) 
                {
                    x = xx[i];
                    xgrids.Add(x);
                }
            }
            xx.Clear();
            return xgrids.ToArray();
        }
        Dictionary<double ,int>CreateDictionary(double []xx)
        {
            Dictionary<double, int> dicts = new Dictionary<double, int>();
            for (int i = 0; i < xx.Length; i++)
            {
                dicts.Add(xx[i], i);
            }            
            return dicts;
        }
       
        public void GetStepFromPoints(ref double stepx, ref double stepy, ref double stepz,int maxNum = 100000)
        {
            //不可能的步长 - 用于过滤计算误差           
            double minstepx = (maxx - minx) / maxNum;
            double minstepy = (maxy - miny) / maxNum;
            double minstepz = (maxz - minz) / maxNum;
            double xx = 0, yy = 0, zz = 0;
            stepx = maxx - minx;
            stepy = maxy - miny;
            stepz = maxz - minz;
            foreach (Vector32 p in points)
            {
                xx = p.x - minx;
                yy = p.y - miny;
                zz = p.z - minz;
                if( xx > minstepx && xx < stepx )stepx = xx;
                if( yy > minstepy && yy < stepy) stepy = yy;
                if( zz > minstepz && zz < stepz) stepz = zz;
            }
        }

        /// <summary>
        /// 从X数组中获取步长
        /// </summary>
        /// <param name="xx"></param>
        /// <returns></returns>
        double GetStepFromArray(double []xx)
        {
            double v1 = 0;
            int k = 0;
            for (int i = 0; i < xx.Length; i++)
            {
                if ( !double.IsNaN(xx[i]) )
                {
                    if (k > 0) return xx[i] - v1;
                    else { v1 = xx[i]; k++; }
                }
            }
            return 0;
        }
        int GetGridFromArray(double[] xx)
        {
            int k = 0;
            for (int i = 0; i < xx.Length; i++)
            {
                if ( !double.IsNaN(xx[i]) ) 
                    k++;                
            }
            return k;
        }       
        void ValueLimited(ref int ix, int x1, int x2)
        {
            if (ix < x1 ) ix = x1;
            if ( ix > x2) ix = x2;
        }
        /// <summary>
        /// 从网格点数据中构造网格数据
        /// </summary>
        /// <param name="n">网格剖分数</param>
        /// <returns></returns>
        C3DGridData CreateGrid3DFromPoints(int n = 20001)
        {
            //获取网格框架--可能是非均匀网格
            float[] xgrids = CreateGridsFromPoints(0);
            float[] ygrids = CreateGridsFromPoints(1);
            float[] zgrids = CreateGridsFromPoints(2);            
            //根据步长计算网格数目            
            int nx0 = xgrids.Length;
            int ny0 = ygrids.Length;
            int nz0 = zgrids.Length;
            C3DGridData data = new C3DGridData(nx0, ny0, nz0);          
            data.ResetDataRange(minx,maxx,miny,maxy,minz,maxz,minv,maxv);
            data.pXGrids = xgrids;
            data.pYGrids = ygrids;
            data.pZGrids = zgrids;
            Int32XYZ xyz;
            foreach (Vector32 p in points)
            {
                xyz = data.GetIndices(p.toVector64());
                data.SetGridValue(xyz.x, xyz.y, xyz.z, p.v);
            }                     
            return data;
        }     

        public override double GetInterpolatedValue(double x, double y, double z)
        {
            if (grid3d0 == null) return double.NaN;
            else return grid3d0.GetGridValueWithInterpolation(x, y, z, SearchingGridLength);            
        }
        public override float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {
            return GetInterpolatedValueCPUParallel(xn,yn,zn);
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z,val;
            long xy = xn * yn;
            long id = 0;

            try
            {
                grid3d0 = CreateGrid3DFromPoints();
                grid3d = new float[xn * yn * zn];

                double sec = 0;
                DateTime t1 = DateTime.Now;

                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + zstep * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + ystep * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + xstep * ix;
                            id = iz * xy + iy * xn + ix;
                            val = GetInterpolatedValue(x, y, z);
                            if ( double.IsNaN(val) ) grid3d[id] = CSurferGrid.blankValuefloat;
                            else grid3d[id] = (float)val;
                        }
                    }
                    if (iz == 0)
                    {
                        sec = (DateTime.Now - t1).TotalSeconds;
                    }
                    //if (k % step == 0)
                    {
                        timeLeft = (zn - iz - 1) * sec;
                        timeSlip += (iz + 1) * sec;
                        percentage = (double)(iz + 1) * 100 / zn;
                    }
                }
                percentage = 100;
                return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                return null;
            }
        }
        /// <summary>
        /// 插值线程
        /// </summary>
        /// <param name="para"></param>
        void InterpolationThread(Object para)
        {
            TaskPoolIndices task = (TaskPoolIndices)para;
            ulong start = (ulong)task.start;//任务开始编号
            ulong num = (ulong)task.num;     //任务数
            ulong memindex = (ulong)task.memindex;//内存块起始位置

            double x, y, z, val;
            Int32XYZ xyz;           
            ulong all = (ulong)xGrid * (ulong)yGrid * (ulong)zGrid;//全部任务数

            DateTime t1 = DateTime.Now;

            for (ulong id = start; id < start + num; id++)
            {
                if ( id >= all ) break;
                xyz = GetIndices(id, xGrid, yGrid, zGrid);
                x = minx + xstep * xyz.x;
                y = miny + ystep * xyz.y;
                z = minz + zstep * xyz.z;

                val = GetInterpolatedValue(x, y, z);
                grid3d[id - start + memindex] = (float)val;

                Interlocked.Decrement(ref taskNum);
                ulong nfinished = all - (ulong)taskNum;//已完成任务数

                if (id - start > 0 && (id - start) % 100 == 0)
                {
                    timeLeft = taskNum * timePerStep;//剩余时间
                    percentage = 100 * (double)nfinished / all;
                }
            }

            Interlocked.Decrement(ref threadNum);
            if (threadNum == 0)
            {
                percentage = 100;
                taskDone.Set();
            }

        }
        /// <summary>
        /// 距离加权插值，CPU并行版
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        int taskNum = 0; //任务数
        int threadNum = 0; //线程数
        AutoResetEvent taskDone = new AutoResetEvent(false);
        public float[] GetInterpolatedValueCPUParallel(int xn, int yn, int zn)
        {
            timeLeft = 0;
            timeSlip = 0;
            percentage = 0;
            progressTitle = "正在插值计算...";
            ulong all = (ulong)xn * (ulong)yn * (ulong)zn;
            C3DGridDataStream gridStream = null;
            try
            {
                if( grid3d0 == null ) grid3d0 = CreateGrid3DFromPoints();
                SetGrid(xn, yn, zn);

                //试运行1个任务，得出预估时间
                progressTitle = "正在估算计算时间...";
                taskNum = 1;
                TaskPoolIndices task = new TaskPoolIndices((long)all/2, 100);
                grid3d = new float[100];
                DateTime t1 = DateTime.Now;
                InterpolationThread(task);
                timePerStep = (DateTime.Now - t1).TotalMilliseconds/1000/100;
                timeLeft = all * timePerStep;
                //试运行1个任务，得出预估时间

                percentage = 0;

                grid3d = null;
                progressTitle = "正在插值计算...";

                taskDone = new AutoResetEvent(false);
                taskNum = (int)all; 
                int batch = Environment.ProcessorCount * 2;//CPU核数-线程数
                batch = 1;
                if (!BigGridData) 
                { 
                    grid3d = new float[all]; 
                    DividedNum = 1; 
                }
                else //分块插值
                {
                    gridStream = new C3DGridDataStream();
                    gridStream.Create(gridDataFile, xn, yn, zn, minx, miny, minz, minv, maxx, maxy, maxz, maxv);
                }

                //估计时间加速比 
                timePerStep = timePerStep / batch;
                ulong start = 0;
                ulong sectionNum = all / (ulong)DividedNum + 1;//分块网格数据大小
                ulong num = sectionNum / (ulong)batch;//单次任务数
                //if (sectionNum % batch != 0) num++;

                ulong written = 0;
                for (int k = 0; k < DividedNum; k++)//grid分块数
                {
                    taskDone.Reset();
                    threadNum = batch;
                    if (sectionNum % (ulong)batch != 0) threadNum++;
                    grid3d = new float[sectionNum + 1];
                    ThreadPool.SetMaxThreads(batch, batch);
                    for (int i = 0; i < batch; i++)
                    {
                        task = new TaskPoolIndices((long)start, (int)num, i * (int)num);
                        start += num;
                        ThreadPool.QueueUserWorkItem(InterpolationThread, task);
                    }

                    if (sectionNum % (ulong)batch != 0)//最后一次任务 
                    {
                        task = new TaskPoolIndices((long)start, (int)sectionNum - batch * (int)num, batch * (int)num);
                        start += (sectionNum - (ulong)batch * num);
                        ThreadPool.QueueUserWorkItem(InterpolationThread, task);
                    }

                    taskDone.WaitOne();//等待所有线程结束

                    if (BigGridData)
                    {
                        if (written + sectionNum < all)
                        {
                            gridStream.WriteData(grid3d, 0, (int)sectionNum);
                            written += sectionNum;
                        }
                        else
                        {
                            gridStream.WriteData(grid3d, 0, (int)(all - written));
                            written = all;
                        }
                        grid3d = null;
                    }
                }//for (int k = 0; k < DividedNum; k++)
                Clear();
                //实际计算时间
                timeSlip = (DateTime.Now - t1).TotalSeconds;
                if (BigGridData) return new float[10];
                else return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                Clear();
                return null;
            }
        }
        
        public override float[] DirectGridding(int xn, int yn, int zn)
        {
            return GetInterpolatedValue(xn, yn, zn);
        }
        public override void Clear()
        {
            if (grid3d0 != null) grid3d0.Clear();
            base.Clear();
        }

    }//end class
    //线性插值Linear，直接网格化
    public class LinearInterpolator : InterpolatorBase
    {        
        public LinearInterpolator()
        {
            method = InterpolationMethod.Linear;
        }
        public override ulong GetRequiredMemorySizeOnCPU()
        {
            return GetGridMemorySize();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ix0"></param>
        /// <param name="iy0"></param>
        /// <param name="iz0"></param>
        /// <param name="gridRad"></param>
        /// <returns>网格节点列表</returns>
        private List<Int32XYZ> SearchGrids(int ix0, int iy0, int iz0, int rad = 1)
        {
            long id = 0;
            List<Int32XYZ> grids = new List<Int32XYZ>();
            for( int iz = iz0 - rad; iz <= iz0 + rad ; iz++ )
            {
                if (iz < 0 || iz >= gridNZ ) continue;
                for (int iy = iy0 - rad; iy <= iy0 + rad; iy++)
                {
                    if (iy < 0 || iy >= gridNY ) continue;
                    for (int ix = ix0 - rad; ix <= ix0 + rad; ix++)
                    {
                        if (ix < 0 || ix >= gridNX ) continue;

                        //except self grid
                        if (ix == ix0 && iy == iy0 && iz == iz0) continue;

                        id = ix + iy * gridNX + iz * gridNX * gridNY;

                        if ( gridPoints[id].Count > 0 )
                            grids.Add(new Int32XYZ(ix, iy, iz));
                    }
                }
            }
            return grids;
        }
        /// <summary>
        /// 从gridPoints网格中插值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="ix0"></param>
        /// <param name="iy0"></param>
        /// <param name="iz0"></param>      
        /// <param name="minRad">网格最小搜索半径，默认1</param>
        /// <returns>x,y,z位置的计算值</returns>
        public double GetInterpolatedValueFromGrids(double x, double y, double z,
                                                    int ix0,int iy0,int iz0,
                                                    InterpolationMethod method = InterpolationMethod.InverseDistanceWeighted,
                                                    int minRad = 4)
        {
            InterpolatorBase ip = null;
            if (method == InterpolationMethod.InverseDistanceWeighted)
                ip = new IdwInterpolatorGlobal();
            else if (method == InterpolationMethod.RadicalBasisFunction)
                ip = new RBFInterpolation();
            else return 0;

            int rad = 1;
            long id = 0;
            List<Int32XYZ> grids = new List<Int32XYZ>();
            while ( ip.pointCount < 1 || rad <= minRad)
            {
                grids = SearchGrids(ix0,iy0,iz0,rad);

                if ( grids.Count > 0 )
                {
                    foreach (Int32XYZ xyz in grids)
                    {
                        id = xyz.x + xyz.y * (xGrid-1) + xyz.z * (xGrid - 1) * (yGrid - 1);
                        foreach(int i in gridPoints[id].Indices)
                        ip.AddPoint(points[i]); 
                    }
                }
                rad++;
            }
            double ret = ip.GetInterpolatedValue(x, y, z);
            ip.Clear();
            return ret;
        }
        public override double GetInterpolatedValue(double x, double y, double z)
        {
            double _xstep = (maxx - minx) / (xGrid - 2);
            double _ystep = (maxy - miny) / (yGrid - 2);
            double _zstep = (maxz - minz) / (zGrid - 2);
            int ix0 = (int)( (x - minx) / _xstep );
            int iy0 = (int)( (y - miny) / _ystep );
            int iz0 = (int)( (z - minz) / _zstep );
            long id0 = ix0 + iy0 * xGrid + iz0 * xGrid * yGrid;
            return GetInterpolatedValueFromGrids(x, y, z, ix0, iy0, iz0, InterpolationMethod.RadicalBasisFunction,6);
            if ( gridPoints[id0].Count > 0 )
            {
                return gridPoints[id0].GetInterpolatedValue(x,y,z,points);
            }
            else
            {
                return GetInterpolatedValueFromGrids(x,y,z,ix0,iy0,iz0, InterpolationMethod.RadicalBasisFunction);
            }
        }
        public override float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z,val;
            long xy = xn * yn;
            long id = 0;

            try
            {
                if ( !CreateGridExt(xn, yn, zn) ) return null;

                grid3d = new float[xn * yn * zn];

                double sec = 0;
                DateTime t1 = DateTime.Now;

                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + zstep * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + ystep * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + xstep * ix;
                            id = iz * xy + iy * xn + ix;
                            val = GetInterpolatedValue(x, y, z);
                            if (double.IsNaN(val)) grid3d[id] = CSurferGrid.blankValuefloat;
                            else grid3d[id] = (float)val;
                        }
                    }
                    if (iz == 0)
                    {
                        sec = (DateTime.Now - t1).TotalSeconds;
                    }
                    //if (k % step == 0)
                    {
                        timeLeft = (zn - iz - 1) * sec;
                        timeSlip += (iz + 1) * sec;
                        percentage = (double)(iz + 1) * 100 / zn;
                    }
                }
                ClearGridExt();
                percentage = 100;
                return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                return null;
            }
        }
        public override void Clear()
        {
            base.Clear();            
        }

    }//end class



    /// <summary>
    /// 距离加权插值
    /// </summary>
    public class IDWInterpolator : IdwInterpolatorGlobal
    {
        public int dimension = 3;   //维度 2，3
        //按矩形区域最长边计算比例0-100
        [CategoryAttribute("搜索"), DisplayNameAttribute("搜索半径%")]
        public double searchRadiu { get; set; } = 10;  //搜索半径百分比，100全搜索（全局插值）
        [CategoryAttribute("搜索"), DisplayNameAttribute("X方向比例")]
        public double searchXScale { get; set; } = 1; //三个方向搜索比例因子
        [CategoryAttribute("搜索"), DisplayNameAttribute("Y方向比例")]
        public double searchYScale { get; set; } = 1;
        [CategoryAttribute("搜索"), DisplayNameAttribute("Z方向比例")]
        public double searchZScale { get; set; } = 1;
        double searchRadiuX = 0;    //3个方向的搜索步长
        double searchRadiuY = 0;    //3个方向的搜索步长
        double searchRadiuZ = 0;    //3个方向的搜索步长
        public bool IsMultiThread = false;
        [CategoryAttribute("搜索"), DisplayNameAttribute("全局搜索")]
        public bool IsGlobalSearching { get; set; } = false; // 是否全局搜索
        [CategoryAttribute("搜索"), DisplayNameAttribute("网格搜索")]
        public bool IsGridSearching { get; set; } = false; // 是否按网格搜索
        GridPoints estimated = new GridPoints();
        double estimatedFragment = 0;
        
        List<Vector32> searchedPoints = new List<Vector32>();//已搜索点索引数组
        
        public IDWInterpolator()
        {
            method = InterpolationMethod.InverseDistanceWeighted;
        }
        public override void CopyFromWithOutPoints( InterpolatorBase ip)
        {
            base.CopyFromWithOutPoints(ip);
            IDWInterpolator ip1 = ip as IDWInterpolator;
            searchRadiu = ip1.searchRadiu;
            dimension = ip1.dimension;
            searchRadiuX = ip1.searchRadiuX;
            searchRadiuY = ip1.searchRadiuY;
            searchRadiuZ = ip1.searchRadiuZ;
        }
        
        bool IsGlobalSearch()
        {
           // IsGridSearching = false;
            IsGlobalSearching = false;

            if ( searchRadiu >= 100 )
            {
                IsGlobalSearching = true;
          //      IsGridSearching = false;
                return true;
            }

            double rad = MaxLength * searchRadiu *0.01;
            searchRadiuX = rad * searchXScale;
            searchRadiuY = rad * searchYScale;
            searchRadiuZ = rad * searchZScale;
            if ( searchRadiuX >= (maxx - minx) &&
                 searchRadiuX >= (maxy - miny) &&
                 searchRadiuX >= (maxz - minz) )
            {
                IsGlobalSearching = true;
            //    IsGridSearching = false;
                return true;
            }
            
            int xx = (int)(searchRadiuX / xstep) + 1;
            int yy = (int)(searchRadiuY / ystep) + 1;
            int zz = (int)(searchRadiuZ / zstep) + 1;

         //   if ( xx * yy * zz * 8 < points.Count) IsGridSearching = true;

            return IsGlobalSearching;
        }
        public override bool CompileDevice(string source)
        {
            for (int i = 0; i < clDevices.Length; i++)
            {
                clDevices[i].AddFunction("gridInterpolate");
                if (!clDevices[i].InitDevice())
                {
                    errMsg = clDevices[i].errMsg;
                    return false;
                }
                if (!clDevices[i].Compile(source))
                {
                    errMsg = clDevices[i].errMsg;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 根据搜索半径创建搜索网格
        /// </summary>
        /// <returns></returns>
        public bool CreateSearchGrids()
        {
            try
            {
                //计算搜索半径
                double rad = MaxLength * searchRadiu / 100;
                double xx = rad * searchXScale;//各向异性介质搜索半径
                double yy = rad * searchYScale;//各向异性介质搜索半径
                double zz = rad * searchZScale;//各向异性介质搜索半径

                gridNX = (int)((maxx - minx) / xx + 0.1) + 1;
                gridNY = (int)((maxy - miny) / yy + 0.1) + 1;
                gridNZ = (int)((maxz - minz) / zz + 0.1) + 1;
                
                gridXStep = (maxx - minx) / (gridNX - 1);
                gridYStep = (maxy - miny) / (gridNY - 1);
                gridZStep = (maxz - minz) / (gridNZ - 1);

                gridPoints = null;
                gridPoints = new GridPointsExt[gridNX * gridNY * gridNZ];
                for (int i = 0; i < gridPoints.Length; i++)
                    gridPoints[i] = new GridPointsExt();

                int ix, iy, iz;
                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    ix = (int)( (p.x - minx) / gridXStep + 0.1);
                    iy = (int)( (p.y - miny) / gridYStep + 0.1);
                    iz = (int)( (p.z - minz) / gridZStep + 0.1);
                    gridPoints[ix + iy * gridNX + iz * gridNX * gridNY].Add(i);
                }

                return true;
            }
            catch (Exception e)
            {
                errMsg = "Create grids failed." + e.Message;
                return false;
            }
        }

        //按搜索半径全局点搜索
        private List<int> SearchFromGlobal( double x, double y, double z )
        {
            Vector32 p;
            List<int> indices = new List<int>();
            double rr;
            //各向同性搜索
            if (searchXScale == searchYScale && searchXScale == searchZScale)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    rr = (x - p.x) * (x - p.x) + (y - p.y) * (y - p.y) + (z - p.z) * (z - p.z);
                    if ( rr <= searchRadiuX * searchRadiuX) indices.Add(i);
                }
            }
            else//各向异性搜索
            {
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    if ( Math.Abs(x - p.x) <= searchRadiuX &&
                         Math.Abs(y - p.y) <= searchRadiuY &&
                         Math.Abs(z - p.z) <= searchRadiuZ ) indices.Add(i);
                }
            }
            return indices;
        }

        /// <summary>
        /// //按搜索半径做网格局部搜索
        /// </summary>
        /// <param name="ix0">点所在网格ix</param>
        /// <param name="iy0">点所在网格iy</param>
        /// <param name="iz0">点所在网格iz</param>
        /// <param name="ixRad">x方向网格搜索半径（网格数）</param>
        /// <param name="iyRad">y方向网格搜索半径（网格数）</param>
        /// <param name="izRad">z方向网格搜索半径（网格数）</param>
        /// <returns>是否到达边界</returns>
        //private bool SearchPointsFromGrids(int ix0, int iy0, int iz0,
        //                                   int ixRad, int iyRad, int izRad,ref List<int>indices)
        //{
        //    long id;
        //    long xy = (xGrid-1) * (yGrid - 1); //待搜索网格            
        //    int ix1 = ix0 - ixRad;
        //    int iy1 = iy0 - iyRad;
        //    int iz1 = iz0 - izRad;
        //    int ix2 = ix0 + ixRad;
        //    int iy2 = iy0 + iyRad;
        //    int iz2 = iz0 + izRad;

        //    //搜索网格前后范围限定
        //    //to check if reach the boundary
        //    bool boundary = false;
        //    if (ix1 < 0) { ix1 = 0; boundary = true; }
        //    if (iy1 < 0) { iy1 = 0; boundary = true; }
        //    if (iz1 < 0) { iz1 = 0; boundary = true; }
        //    if (ix2 > xGrid - 2) { ix2 = xGrid - 2; boundary = true; }
        //    if (iy2 > yGrid - 2) { iy2 = yGrid - 2; boundary = true; }
        //    if (iz2 > zGrid - 2) { iz2 = zGrid - 2; boundary = true; }            
        //    for (int k = iz1; k <= iz2; k++)
        //    {
        //        for (int j = iy1; j <= iy2; j++)
        //        {
        //            for (int i = ix1; i <= ix2; i++)
        //            {
        //                id = k * xy + j * (xGrid-1) + i;
        //                if ( gridPoints[id].Count > 0 )
        //                    indices.AddRange(gridPoints[id].Indices);
        //            }//for (int i = ix1; i < ix2; i++)
        //        }// for (int j = iy1; j < iy2; j++)
        //    }//for (int k = iz1; k < iz2; k++)
        //    return boundary;
        //}

        public List<int> SearchFromGrids( double x, double y, double z )
        {
            List<int> indices = new List<int>();
            if (gridPoints == null || points.Count < 1) return indices;

            //网格所在位置ix,iy,iz
            int ix = (int)((x - minx) / gridXStep + 0.1);
            int iy = (int)((y - miny) / gridYStep + 0.1);
            int iz = (int)((z - minz) / gridZStep + 0.1);

            long id;
            long xy = gridNX * gridNY; //待搜索网格            
            int ix1 = ix - 2;
            int ix2 = ix + 2;
            int iy1 = iy - 2;
            int iy2 = iy + 2;
            int iz1 = iz - 2;
            int iz2 = iz + 2;
            if (ix1 < 0) ix1 = 0;
            if (iy1 < 0) iy1 = 0;
            if (iz1 < 0) iz1 = 0;
            if (ix2 >= gridNX) ix2 = gridNX - 1;
            if (iy2 >= gridNY) iy2 = gridNY - 1;
            if (iz2 >= gridNZ) iz2 = gridNZ - 1;

            Vector32 p;
            double rr = 0; //搜索半径平方
            double rr2 = searchRadiuX * searchRadiuX;//各向同性搜索半径平方
            for (iz = iz1; iz <= iz2; iz++)
            {
                for (iy = iy1; iy <= iy2; iy++)
                {
                    for (ix = ix1; ix <= ix2; ix++)
                    {
                        id = iz * xy + iy * gridNX + ix;
                        if (gridPoints[id].Count < 1) continue;
                        foreach (int k in gridPoints[id].Indices)
                        {
                            p = points[k];
                            //各向同性
                            if ( searchXScale == searchYScale && searchXScale == searchZScale)
                            {
                                rr = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y) + (p.Z - z) * (p.Z - z);
                                if ( rr <= rr2 ) indices.Add(k);
                            }
                            else //各向异性
                            {
                                if ( Math.Abs(x - p.x) <= searchRadiuX &&
                                     Math.Abs(y - p.y) <= searchRadiuY &&
                                     Math.Abs(z - p.z) <= searchRadiuZ) indices.Add(k);
                            }                            
                        }
                    }//for (int i = ix1; i < ix2; i++)
                }// for (int j = iy1; j < iy2; j++)
            }//for (int k = iz1; k < iz2; k++)
            return indices;
        }

        /// <summary>
        /// 搜索周围的点
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <returns>已搜索点数</returns>
        //public bool SearchPoints(double x,double y,double z,ref List<int>indices)
        //{
        //    if ( gridPoints == null || points.Count < 1 ) return false;

        //    indices.Clear();

        //    //网格所在位置ix,iy,iz
        //    int ix = (int)( (x - minx ) / xstep  );
        //    int iy = (int)( (y - miny ) / ystep);
        //    int iz = (int)( (z - minz ) / zstep);
        //    if (ix > xGrid-2) ix = xGrid - 2;
        //    if (iy > yGrid-2) iy = yGrid - 2;
        //    if (iz > zGrid-2) iz = zGrid - 2;

        //    //计算搜索半径
        //    double rad = MaxLength * searchRadiu / 100;
        //    double xx = rad * searchXScale;//各向异性介质搜索半径
        //    double yy = rad * searchYScale;//各向异性介质搜索半径
        //    double zz = rad * searchZScale;//各向异性介质搜索半径

        //    //搜索网格数（取整），再扩展一个网格
        //    int ixRad = (int)(xx / xstep) + 1;
        //    int iyRad = (int)(yy / ystep) + 1;
        //    int izRad = (int)(zz / zstep) + 1;
            
        //    return SearchPointsFromGrids(ix, iy, iz, ixRad, iyRad, izRad, ref indices);
            
        //}
        /// <summary>
        /// 从搜索点中插值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override double GetInterpolatedValue(double x, double y, double z)
        {
            double val = double.NaN;

            InversePower ip = new InversePower();
            ip.NearestValueOnly = NearestValueOnly;

            if (IsMultiThread) ip.Distances = null;//多线程版本不能用固定变量
            else ip.Distances = distances;//多线程版本不能用固定变量            
            if (IsGlobalSearching) //全局搜索
            { 
                ip.AddPoints(points);
                val = ip.GetInterpolatedValue(x, y, z);
            }
            else
            {
                ip.AddPoints(searchedPoints);//局部搜索
                val = ip.GetInterpolatedValue(x, y, z);
                ip.Clear();
            }            
            return val;
        }
        public double GetInterpolatedValueByIndices(double x, double y, double z,List<int>indices,List<Vector32>_points)
        {
            double val = double.NaN;            
            if (indices.Count < 1) return val;
            if (indices.Count == 1) return _points[indices[0]].V;
            InversePower ip = new InversePower();                        
            val = ip.GetInterpolatedValueByIndices(x, y, z,indices,_points);
            ip.Clear();
            return val;
        }

        
        /// <summary>
        /// 将数据变换到0-100区间，更新数据范围
        /// </summary>
        public override void UpdatePointsRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;
            int k = 0;
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (C3DData.IsBlankValue(p.V)) continue;
                if (k == 0)
                {
                    minx = maxx = p.x;
                    miny = maxy = p.y;
                    minz = maxz = p.z;
                    minv = maxv = p.v;
                    k++;
                }
                else
                {
                    if (p.x > maxx) maxx = p.x;
                    if (p.x < minx) minx = p.x;
                    if (p.y > maxy) maxy = p.y;
                    if (p.y < miny) miny = p.y;
                    if (p.z > maxz) maxz = p.z;
                    if (p.z < minz) minz = p.z;
                    if (p.v > maxv) maxv = p.v;
                    if (p.v < minv) minv = p.v;
                }
            }

            normalizedScale = MaxLength;
            normalizedOrg = new Vector64(minx,miny,minz);
            maxx = (maxx - minx) / normalizedScale;
            maxy = (maxy - miny) / normalizedScale;
            maxz = (maxz - minz) / normalizedScale;
            minx = miny = minz = 0;
            Normalized = true;

            if (xGrid > 1) xstep = (maxx - minx) / (xGrid - 1);
            if (yGrid > 1) ystep = (maxy - miny) / (yGrid - 1);
            if (zGrid > 1) zstep = (maxz - minz) / (zGrid - 1);

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = toNormalized(points[i]);
            }
        }

        /*
        public override double GetInterpolatedValue(double x, double y, double z)
        {
            if (searchedPoints.Count < 1) return CDataModel.BlankValue;
            //距离数组
            distances = new double[searchedPoints.Count];
            double dist = 0;
            double fenmu = 0;
            double weight = 0;
            GridPointsExt gp = new GridPointsExt(true);
            int i = 0;
            foreach (Vector32 p in searchedPoints)
            {
                //距离平方
                dist = (x - p.x) * (x - p.x) +
                       (y - p.y) * (y - p.y) +
                       (z - p.z) * (z - p.z);

                if ( IsZero( dist ) )
                {
                    gp.Add(p);
                    distances[i] = -1;
                }
                else
                {
                    if (power == 1) weight = 1.0 / Math.Sqrt(dist);
                    else if (power == 2) weight = 1.0 / Math.Sqrt(dist);
                    else if (power == 4) weight = 1.0 / dist;
                    else weight = 1.0 / Math.Pow(dist, 0.5 * power);
                    distances[i] = weight;
                    fenmu += weight;
                }
                i++;
            }

            i = 0;
            double value1 = 0;
            if (fenmu > 0)
            {
                foreach (Vector32 p in searchedPoints)
                {
                    //计算权重系数,加权
                    if ( distances[i] > 0 )
                    {
                        value1 += p.v * distances[i] / fenmu;
                        i++;
                    }
                }
            }

            distances = null;

            double value2 = 0;
            if (gp.Count > 0) value2 = gp.GetGridAverageValue();

            if ( fenmu > 0 )
            {
                if (gp.Count > 0)
                    return value1 * 0.1 + value2 * 0.9;
                else return value1;
            }
            else
            {
                if (gp.Count > 0)
                    return value2;
                else return CDataModel.BlankValue;
            }
        }
        */
      
        /// <summary>
        /// 插值计算线程
        /// </summary>
        /// <param name="para"></param>
        void IDWInterpolationThread( Object para )
        {
            TaskPoolIndices task = (TaskPoolIndices)para;

            long start = task.start;//任务开始编号
            int num = task.num;     //任务数
            int memindex = task.memindex;//内存块起始位置

            double x, y, z,val;
            Int32XYZ xyz;
            List<int> indices = null;
            int all = xGrid * yGrid * zGrid;//全部任务数

            DateTime t1 = DateTime.Now;

            for ( long id = start; id< start + num; id++ )
            {
                if (id >= all) break;
                xyz = GetIndices((ulong)id,xGrid,yGrid,zGrid);
                x = minx + xstep * xyz.x;
                y = miny + ystep * xyz.y;
                z = minz + zstep * xyz.z;
                if (IsGlobalSearching)//全局搜索
                {
                    val = GetInterpolatedValue(x, y, z);
                    if (double.IsNaN(val)) grid3d[id-start + memindex] = CSurferGrid.blankValuefloat;
                    else grid3d[id - start + memindex] = (float)val;
                }
                else//按半径搜索
                {                   
                    if (IsGridSearching) indices = SearchFromGrids(x, y, z);                    
                    else indices = SearchFromGlobal(x, y, z);
                    val = GetInterpolatedValueByIndices(x, y, z, indices, points);
                    if (double.IsNaN(val)) grid3d[id - start + memindex] = CSurferGrid.blankValuefloat;
                    else grid3d[id - start + memindex] = (float)val;
                    indices.Clear();
                }
                
                Interlocked.Decrement(ref taskNum);
                int nfinished = all - taskNum;//已完成任务数

                if ( id - start > 0 && (id-start)%100 == 0 )
                {
                   // timePerStep = (DateTime.Now - t1).TotalSeconds / (id-start);
                    timeLeft = taskNum * timePerStep;//剩余时间
                    percentage = 100 * (double)nfinished / all;
                }
            }

            Interlocked.Decrement(ref threadNum);
            if (threadNum == 0) 
            {
                percentage = 100;
                taskDone.Set();
            }

        }
        /// <summary>
        /// 距离加权插值，CPU并行版
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        int taskNum = 0;
        int threadNum = 0;
        AutoResetEvent taskDone = new AutoResetEvent(false);
        public float[] GetInterpolatedValueCPUParallel(int xn, int yn, int zn)
        {
            timeLeft = 0;
            timeSlip = 0;
            percentage = 0;
            progressTitle = "正在插值计算...";
            int all = xn * yn * zn;
            C3DGridDataStream gridStream = null;
            try
            {
                UpdatePointsRange();
                SetGrid(xn, yn, zn);
                if ( IsGlobalSearch() ) //全局搜索
                {
                    //distances = new double[points.Count];
                }
                else //非全局搜索
                {
                    if (IsGridSearching)//按网格方式搜索
                    {
                        if ( !CreateSearchGrids() ) return null;
                    }
                }               

                //试运行1个任务，得出预估时间
                progressTitle = "正在估算计算时间...";
                taskNum = 1;
                TaskPoolIndices task = new TaskPoolIndices(all/2,10);
                grid3d = new float[10];

                //试运行1个任务，得出预估时间
                DateTime t1 = DateTime.Now;
                IDWInterpolationThread(task);
                
                timePerStep = (DateTime.Now - t1).TotalMilliseconds/10000.0;                
                
                timeLeft = all * timePerStep;
                percentage = 0;

                grid3d = null;
                progressTitle = "正在插值计算...";

                taskDone = new AutoResetEvent(false);
                taskNum = all;
                int sectionNum = taskNum;

                int batch = Environment.ProcessorCount;//CPU核数-线程数

                if (!BigGridData) grid3d = new float[all];
                else //分块插值
                {   
                    gridStream = new C3DGridDataStream();
                    gridStream.Create(gridDataFile, xn, yn, zn, minx, miny, minz, minv, maxx, maxy, maxz, maxv);
                }
                
                //估计时间加速比 
                timePerStep = timePerStep / batch;
                
                int start = 0;
                
                sectionNum = all / DividedNum + 1;//分块网格数据大小
                int num = sectionNum / batch;//单次任务数
                //if (sectionNum % batch != 0) num++;

                int written = 0;
                for (int k = 0; k < DividedNum; k++)//grid分块数
                {
                    taskDone.Reset();
                    
                    threadNum = batch;
                    if (sectionNum % batch != 0) threadNum++;

                    grid3d = new float[sectionNum+1];
                    ThreadPool.SetMaxThreads(batch, batch);
                    
                    for (int i = 0; i < batch; i++)
                    {
                        task = new TaskPoolIndices(start, num, i*num);
                        start += num;
                        ThreadPool.QueueUserWorkItem(IDWInterpolationThread, task);
                    }

                    if ( sectionNum % batch != 0 )//最后一次任务 
                    {
                        task = new TaskPoolIndices(start, sectionNum-batch*num, batch * num);
                        start += (sectionNum - batch * num);
                        ThreadPool.QueueUserWorkItem(IDWInterpolationThread, task);
                    }

                    taskDone.WaitOne();//等待所有线程结束

                    if ( BigGridData )
                    {
                        if (written + sectionNum < all)
                        {
                            gridStream.WriteData(grid3d, 0, sectionNum);
                            written += sectionNum;
                        }
                        else 
                        { 
                            gridStream.WriteData(grid3d, 0, all - written);
                            written = all;
                        }
                        grid3d = null;
                    }
                }//for (int k = 0; k < DividedNum; k++)

                //实际计算时间
                timeSlip = (DateTime.Now - t1).TotalSeconds;
                if (BigGridData) return new float[10];
                else return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                Clear();
                return null;
            }
        }
        /// <summary>
        /// 距离加权插值，CPU串行版
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        public float[] GetInterpolatedValueCPU(int xn, int yn, int zn)
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z,val;
            long xy = xn * yn;
            long id;

            try
            {                
                UpdatePointsRange();
                SetGrid(xn, yn, zn);

                if ( IsGlobalSearch() ) //全局搜索
                {  
                    distances = new double[points.Count];
                }
                else //非全局搜索
                {
                    if ( IsGridSearching )//按网格方式搜索
                    {
                        if ( !CreateSearchGrids() ) return null;
                    }
                }

                grid3d = new float[xy * zn];

                double sec = 0;
                DateTime t1 = DateTime.Now;
                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + zstep * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + ystep * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + xstep * ix;
                            id = iz * xy + iy * xn + ix;
                            if (IsGlobalSearching)
                            {
                                val = GetInterpolatedValue(x, y, z);                                
                                if (double.IsNaN(val)) grid3d[id] = CSurferGrid.blankValuefloat;
                                else grid3d[id] = (float)val;
                            }
                            else 
                            {
                                List<int> indices = new List<int>();
                                if (IsGridSearching)
                                {
                                    indices = SearchFromGrids(x, y, z);
                                }
                                else 
                                {
                                    indices = SearchFromGlobal(x, y, z); 
                                }
                                
                                val = GetInterpolatedValueByIndices(x, y, z, indices, points);
                                if (double.IsNaN(val)) grid3d[id] = CSurferGrid.blankValuefloat;
                                else grid3d[id] = (float)val;
                                
                                indices.Clear();
                            }
                        }
                    }
                    if (iz == 0)
                    {
                        sec = (DateTime.Now - t1).TotalSeconds;
                    }
                    //if (k % step == 0)
                    {
                        timeLeft = (zn - iz - 1) * sec;
                        timeSlip += (iz + 1) * sec;
                        percentage = (double)(iz + 1) * 100 / zn;
                    }
                }
                distances = null;
                percentage = 100;
                return grid3d;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                Clear();
                return null;
            }
        }
        
        /// <summary>
        /// 距离加权插值，GPU并行版
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        public override float[] GetInterpolatedValue(int nx, int ny, int nz, Device[] devices = null)
        {
            if (points.Count < 1)
            {
                errMsg = "no enough points.";
                Clear();
                return null;
            }

            //CPU串行版本
            //if (devices == null) return GetInterpolatedValueCPU(nx, ny, nz);
            //CPU并行版
            if (devices == null) return GetInterpolatedValueCPUParallel(nx, ny, nz);           

            SetGrid(nx, ny, nz);

            percentage = 0;

            startTime = DateTime.Now;
            progressTitle = "正在计算所需内存...";

            ulong requireMemorySize = GetRequiredMemorySizeOnCPU();
            
            ulong cpuAvailableMemorySize = MMPhysicalMemory.GetAvailableMemorySize();
            if (requireMemorySize >= cpuAvailableMemorySize)
            {
                errMsg = "no enough memory available on CPU device.\n";
                errMsg += "Required:" + Math.Round((double)requireMemorySize / 1024 / 1024, 2) + "MB\n";
                errMsg += "Available:" + Math.Round((double)cpuAvailableMemorySize / 1024 / 1024, 2) + "MB\n";
                return null;
            }

            ulong sum = 0;
            foreach (Device d in devices)
            {
                sum += OpenCLObj.GetMaxAllocMemorySize(d);
            }

            if (sum < requireMemorySize)
            {
                errMsg += "no enough memory available on GPU devices.\n";
                errMsg += "Required:" + Math.Round((double)requireMemorySize / 1024 / 1024, 2) + "MB\n";
                errMsg += "Available:" + Math.Round((double)sum / 1024 / 1024, 2) + "MB\n";
                return null;
            }

            progressTitle = "初始化GPU设备...";
            if (!InitDevices(devices))
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "正在编译...";
            if (!CompileDevice(InterpolateSource))
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "创建内存变量...";
            if ( !CreatePointsMemory() )
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }

            progressTitle = "计算网格单元...";
            if ( !StartMultiGridInterpolation() )            
            {
                //progressTitle = "正在保存当前进度....";
                //SaveProgress(progressFile);
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                grid3d = null;
                return null;
            }

            progressStep = 1;            
            percentage = 100;

            Clear();
            ReleaseDevices();
            DateTime endTime = DateTime.Now;
            timeSlip = (endTime - startTime).TotalSeconds;
            return grid3d;
        }

        public bool StartMultiGridInterpolation()
        {
            progressTitle = "正在进行网格插值计算...";
            percentage = 0;

            int batch = 1000;
            long total = xGrid * yGrid * zGrid;

            DateTime t0 = DateTime.Now;
            DateTime t1 = t0;
            DateTime t2 = t0;

            try
            {
                percentage = 0;
                
                estimated.Clear();//估算时间
                estimatedFragment = 0;

                if (grid3d == null) grid3d = new float[total];

                CreateGriddingTaskPools(total, batch);

                ThreadPool.SetMaxThreads(clDevices.Length, clDevices.Length);
                //启动计算线程
                //Thread[] threads = new Thread[clDevices.Length];
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].Stop = false;
                    clDevices[i].grid = new float[batch];
                    clDevices[i].SetWorkItemSize(batch);
                    ThreadPool.QueueUserWorkItem(GridInterpolationThread, clDevices[i]);
                    //threads[i] = new Thread(GridInterpolationThread);
                    //threads[i].Start(clDevices[i]);
                }

                //等待线程结束
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].WaitSiginal();
                }
                //release memory               
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].ReleaseMemory();
                    //threads[i] = null;
                }
                //threads = null;
                return true;
            }
            catch (Exception e)
            {
                errMsg = "gridding failed.\n" + e.Message;
                return false;
            }
        }

        public void GridInterpolationThread(Object para)
        {
            int np = points.Count;
            int taskCount = taskPools.Count;            
            double step = taskCount / 100.0;
            int nstep = (int)step;
            if (nstep == 0) nstep = 1;

            double sec = 0; //估算时间

            #region 线程配置------------------
            CLInterpolationUnit clUnit = (CLInterpolationUnit)para;
            int[] workItemSizes = new int[] { (int)clUnit.workItemSize };
            int[] globalWorkItemSizes = new int[] { (int)clUnit.totalWorkSize };
            #endregion 线程配置

            TaskPoolIndices task = GetFromTaskPools();

            double scale = 100 / MaxLength;

            //在显存创建缓冲区并把HOST的数据拷贝过去                                
            var px1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, px.Length * sizeof(float), px.ToFloatPtr());
            var py1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, py.Length * sizeof(float), py.ToFloatPtr());
            var pz1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pz.Length * sizeof(float), pz.ToFloatPtr());
            var pv1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pv.Length * sizeof(float), pv.ToFloatPtr());
            double rad = MaxLength * searchRadiu / 100;
            
            int m = 0;//任务次数
            while (!clUnit.Stop && !threadStoped && task.num > 0)
            {
                DateTime t1 = DateTime.Now;

                int startid = (int)task.start;
                int num = task.num;

                workItemSizes = new int[] { num };
                globalWorkItemSizes = new int[] { num };

                //网格置0，数组重置
                for (int i = 0; i < num; i++) clUnit.grid[i] = 0;

                var grid1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, clUnit.grid.Length * sizeof(float), clUnit.grid.ToFloatPtr());
                
                int k = 0;
                int nearest_value = 0;
                if (NearestValueOnly) nearest_value = 1;
                clUnit.Kernels["gridInterpolate"].SetArg(k++, px1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, py1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, pz1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, pv1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, grid1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, startid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, num);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, nearest_value);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, rad);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, Power);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, minx);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, miny);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, minz);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, xstep);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, ystep);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, zstep);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, xGrid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, yGrid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, zGrid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, np);

                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["gridInterpolate"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                clUnit.oclCQ.EnqueueBarrier();
                clUnit.oclCQ.EnqueueReadBuffer(grid1, true, 0, clUnit.grid.Length * sizeof(float), clUnit.grid.ToFloatPtr());
                clUnit.oclCQ.Finish();

                //拷贝到grid3d数组对应位置
                CopyGridFromThread(startid, num, clUnit.grid);

                
                grid1.Dispose();
                
                if (m == 0)
                {
                    sec = (DateTime.Now - t1).TotalSeconds;
                    lock (lock3)
                    {
                        estimated.Add(sec);
                        estimatedFragment = estimated.GetValue();
                    }
                }
                
                //下一任务
                task = GetFromTaskPools();                
                lock (lock2)
                {
                    percentage = (1.0 - (double)taskPools.Count / (double)taskCount) * 100;
                    timeLeft = estimatedFragment * taskPools.Count;
                }
                m++;
            }//while (!clUnit.Stop && !threadStoped)
           
            px1.Dispose();
            py1.Dispose();
            pz1.Dispose();

            clUnit.ReleaseSiginal();//线程结束信号
        }
        public override void Clear()
        {
            base.Clear();           
        }

        #region Inverse Distance Weighted Interpolation Kernel Source
        private string InterpolateSource =
            @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable   
            #pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
                    
         double CalculateDistance(double x, double y, double z,float x1, float y1, float z1,double power )
         {
            double dist = 0;
             if( power == 1.0 ) 
                dist = fabs(x - x1) + fabs(y - y1)  + fabs(z - z1);
             else if( power == 2.0 ) 
                dist = sqrt( (x - x1) * (x - x1) + (y - y1) * (y - y1) + (z - z1) * (z - z1) );
             else 
             {
                dist = sqrt( (x - x1) * (x - x1) +  (y - y1) * (y - y1) + (z - z1) * (z - z1) );
                if( dist > 0 ) dist = pow( dist, power );
             }
             return dist;
         }
        
        __kernel void gridInterpolate(  __global float * px, 
                                        __global float * py, 
                                        __global float * pz,
                                        __global float * pv,
                                        __global float * grid,
                                        int startid,
                                        int tasknum,
                                        int nearest_value,
                                        double rad,double power,
                                        double minx,double miny,double minz,
                                        double xstep,double ystep,double zstep,
                                        int nx,int ny,int nz,int np )
                                        
        {
            int id = get_global_id(0);
            if( id >= tasknum ) return;
            id = id + startid;
            if( id >= nx*ny*nz ) return;

            int ix,iy,iz,xy,md;
            double x,y,z;

            xy = nx * ny; 
            iz = (int)(id/xy);
            md = id - iz * xy;
            iy = md / nx;
            ix = md % nx;

            x =  minx + ix * xstep;
            y =  miny + iy * ystep;
            z =  minz + iz * zstep;

            int i=0;            
            double dist = 0;
            double fenmu = 0;
            double sum = 0;
            double mindist = 1E10;
            while( i < np )
            {
                dist = CalculateDistance(x,y,z,px[i],py[i],pz[i],power);
                if( dist < rad )
                {   
                    if( dist == 0 )
                    {
                       grid[ id - startid] = pv[i];
                       return;         
                    }                    
                    else fenmu += (1.0 / dist);
                }
                if( nearest_value > 0 )
                {
                    if(dist < mindist)
                    {
                         mindist = dist;
                         sum = pv[i];
                     }
                 }
                i++;
            }

            if( nearest_value > 0 )
            {
                grid[ id - startid] = sum;
                return;
            }

            i = 0;
            sum = 0;
            while( i < np )
            {
                dist = CalculateDistance(x,y,z,px[i],py[i],pz[i],power);
                if( dist < rad )
                {
                    sum += ( 1.0 / dist * pv[i] / fenmu );
                }
                i++;
            }
            grid[ id - startid] = sum;             
        }      
        "; //end of InterpolationSource
        #endregion interpolation gpu source

    }//end class of IDWInterpolator
     /////////////////////////////////////////////////

    //分块矩阵，按行分
    public class Martrix2D
    {
        public int Start = 0;
        public int rowNum = 0; 
        public int Row = 0;
        public double[] A = null;//距离矩阵
        public Martrix2D(int row, int start, int num)
        {
            Row = row;
            Start = start;
            rowNum = num;
            A = new double[rowNum * Row];
        }
        public void CopyFrom(double []s)
        {
            Array.Copy(s, Start * Row, A, 0, rowNum * Row);
        }
        public void CopyTo(ref double[] d)
        {
            Array.Copy(A, 0, d, Start * Row, rowNum * Row);
        }
        public long Length
        {
            get 
            {
                return rowNum * Row;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="row">此处为全局矩阵的行号</param>
        /// <param name="col"></param>
        /// <returns></returns>
        public double this[int row,int col] 
        {            
            get 
            {
                return A[(row - Start) * Row + col];                
            }
            set
            {
                A[(row - Start) * Row + col] = value;
            }
        }

        public void Release()
        {
            A = null;
        }
    }
    //分块矩阵管理
    public class MartrixManage
    {
        Martrix2D[] martrixs = null;
        public MartrixManage( int num )
        {
            martrixs = new Martrix2D[num];
        }
        public bool InitMatrix(int index, int row, int start, int num)
        {
            try 
            {
                martrixs[index] = new Martrix2D(row, start, num);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        public double this[int row,int col]
        {
            get 
            {
                for(int i=0;i<martrixs.Length;i++)
                {
                    if ( row <= martrixs[i].Start ) 
                        return martrixs[i][row, col];
                }
                return 0;
            }
            set 
            {
                for (int i = 0; i < martrixs.Length; i++)
                {
                    if ( row <= martrixs[i].Start )
                      martrixs[i][row, col] = value;
                }                
            }
        }
    }
    public class RBFInterpolationGlobal : InterpolatorBase
    {
        #region 变量定义
        public int pointNum { get { return points.Count; } }
        public double[] A = null;//距离矩阵
        public double[] E = null;//扩展矩阵
        public double[] a = null;//系数矩阵
        #endregion 变量定义

        public RBFInterpolationGlobal()
        {
            method = InterpolationMethod.RadicalBasisFunction;
        }
        public override double GetInterpolatedValue(double x, double y, double z)
        {
            int n = points.Count;
            int i, j;
            double r, sum = 0, sum1 = 0;            
            DenseMatrix QMatix = new DenseMatrix(n, n);
            //计算|Pi-Pj|矩阵Q，这里可能需要检查重点
            Vector32 p1, p2;
            for (i = 0; i < n; i++)
            {
                p1 = points[i];
                for (j = 0; j < n; j++)
                {
                    p2 = points[j];
                    r = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                   (p1.y - p2.y) * (p1.y - p2.y) +
                                   (p1.z - p2.z) * (p1.z - p2.z) +
                                    MINE * MINE);
                    QMatix[i, j] = r;
                }
            }

            Matrix<double> qm = QMatix.Inverse();
            a = new double[n];

            //求伴随矩阵a[j]
            for (i = 0; i < n; i++)
            {
                sum = 0.0;
                for (j = 0; j < n; j++)
                {
                    sum += qm[i, j] * points[j].v;
                }
                a[i] = sum;
            }
            qm.Clear();
            QMatix.Clear();

            sum1 = 0.0;
            //************************************************************************	
            for (j = 0; j < n; j++)
            {
                r = Math.Sqrt((x - points[j].x) * (x - points[j].x) +
                               (y - points[j].y) * (y - points[j].y) +
                               (z - points[j].z) * (z - points[j].z) +
                               MINE * MINE);
                sum1 = sum1 + r * a[j];
            }
            a = null;
            return sum1;
        }

    }
    //径向基函数
    public class RBFInterpolation : InterpolatorBase
    {
        //--------并行算法----------------------------------------------------------------
        //-------------------------------------------------------------------------
        #region 变量定义
        
        public int pointNum { get { return points.Count; } }

        public double[] A = null;//距离矩阵
        public double[] E = null;//扩展矩阵
        public double[] aFactor = null;//系数矩阵
        public double[] lineA = null; //第K行A矩阵数据
        public double[] lineE = null;//第K行E矩阵数据
        
        //距离变换
        double minDist = 0;
        double maxDist = 0;

        #endregion 变量定义

        public RBFInterpolation()
        {
            method = InterpolationMethod.RadicalBasisFunction;
        }        
        public override ulong GetRequiredMemorySizeOnCPU()
        {
            ulong row = (ulong)points.Count;
            ulong umatrix = 2*row * row * sizeof(double); //A,E
            //lineA, lineE, BS
            ulong uext = 3*row*sizeof(double);
            return umatrix + uext + GetGridMemorySize();
        }

        public override bool SaveProgress(string filename)//保存当前进程到文件
        {
            progressTitle = "正在保存进度...";
            BinaryWriter br;
            try
            {
                br = new BinaryWriter(new FileStream(filename, FileMode.Create));
                int mark = 0x1a2b3d4f;
                br.Write(mark);
                br.Write(progressStep);
                br.Write(progressRow);
                
                br.Write((int)method);
                br.Write(timeSlip);
                br.Write(timeLeft);
                br.Write(percentage);
                
                br.Write(xGrid);
                br.Write(yGrid);
                br.Write(zGrid);

                br.Write(minx);
                br.Write(maxx);
                br.Write(miny);
                br.Write(maxy);
                br.Write(minz);
                br.Write(maxz);
                
                br.Write(xstep);
                br.Write(ystep);
                br.Write(zstep);
                br.Write(minDist);
                br.Write(maxDist);
                //计算进程 0，未开始，1完成距离矩阵，2完成矩阵求逆，3完成伴随矩阵，4完成grid计算
                int row = points.Count;
                br.Write(row);
                for(int i=0;i<row;i++)
                {
                    br.Write(points[i].x);
                    br.Write(points[i].y);
                    br.Write(points[i].z);
                    br.Write(points[i].v);
                }
                if ( progressStep == 1)//1完成距离矩阵,未完成矩阵求逆2
                {
                    int alen = 0;
                    if( A != null ) alen = A.Length;
                    br.Write(alen);
                    if( alen > 0 )
                    {   
                        for (long i = 0; i < A.Length; i++) br.Write(A[i]);
                    }
                    int elen = 0;
                    if (E != null && progressRow > 0) elen = E.Length;
                    br.Write(elen);
                    if ( elen > 0)
                    { 
                        for (long i = 0; i < E.Length; i++) br.Write(E[i]); 
                    }
                }
                else if (progressStep == 2)//完成矩阵求逆2，未完成伴随矩阵3
                {
                    int elen = 0;
                    if (E != null && progressRow > 0) elen = E.Length;
                    br.Write(elen);
                    if (elen > 0) 
                    {
                        for (long i = 0; i < E.Length; i++) br.Write(E[i]);
                    }                    
                    int alen = 0;
                    if (aFactor != null) alen = aFactor.Length;
                    br.Write(alen);
                    if( alen > 0 )
                    {
                        for (long i = 0; i < row; i++) br.Write(aFactor[i]);
                    }                    
                }
                else if (progressStep == 3) //完成伴随矩阵3，未完成grid计算4
                {
                    int alen = 0;
                    if (aFactor != null) alen = aFactor.Length;
                    br.Write(alen);
                    if (alen > 0)
                    {
                        for (long i = 0; i < row; i++) br.Write(aFactor[i]);
                    }
                }
                br.Close();
                return true;
            }
            catch (IOException e)
            {
                return false;
            }        
        }
        public override bool LoadProgress(string filename)//读取当前计算进程
        {
            progressTitle = "正在载入进度...";
            BinaryReader br;
            try
            {
                br = new BinaryReader(new FileStream(filename, FileMode.Open));
                //int mark = 0x1a2b3d4f;
                int mark = br.ReadInt32();
                if( mark != 0x1a2b3d4f )
                {
                    errMsg = "not a valid file format.";
                    br.Close();
                    return false;
                }
                progressStep = br.ReadInt32();
                progressRow = br.ReadInt32();

                method = (InterpolationMethod)br.ReadInt32();
                timeSlip = br.ReadDouble();
                timeLeft = br.ReadDouble();
                percentage = br.ReadDouble();

                xGrid = br.ReadInt32();
                yGrid = br.ReadInt32();
                zGrid = br.ReadInt32();

                minx = br.ReadDouble();
                maxx = br.ReadDouble();
                miny = br.ReadDouble();
                maxy = br.ReadDouble();
                minz = br.ReadDouble();
                maxz = br.ReadDouble();
               
                xstep = br.ReadDouble();
                ystep = br.ReadDouble();
                zstep = br.ReadDouble();
                minDist = br.ReadDouble();
                maxDist = br.ReadDouble();
                int row = br.ReadInt32();
                points.Clear();
                float x, y, z, v;
                for(int i=0;i<row;i++)
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    v = br.ReadSingle();
                    points.Add(new Vector32(x, y, z, v));
                }
                //计算进程 0，未开始，1完成距离矩阵，2完成矩阵求逆，3完成伴随矩阵，4完成grid计算
                if (progressStep == 1)//1完成距离矩阵,未完成矩阵求逆2
                {
                    int alen = br.ReadInt32();
                    if ( alen > 0 )
                    {
                        A = new double[row * row];
                        for (long i = 0; i < A.Length; i++) A[i] = br.ReadDouble();
                    }
                    int elen = br.ReadInt32();
                    if ( elen > 0 )
                    {
                        E = new double[row * row];
                        for (long i = 0; i < E.Length; i++) E[i] = br.ReadDouble();
                    }
                }
                else if (progressStep == 2)//完成矩阵求逆2，未完成伴随矩阵3
                {
                    int elen = br.ReadInt32();
                    if (elen > 0)
                    { 
                        E = new double[row * row];
                        for (long i = 0; i < E.Length; i++) E[i] = br.ReadDouble();
                    }
                    int alen = br.ReadInt32();
                    if (elen > 0)
                    {
                        aFactor = new double[row];
                        for (long i = 0; i < row; i++) aFactor[i] = br.ReadDouble();
                    }
                }
                else if (progressStep == 3) //完成伴随矩阵3，未完成grid计算4
                {
                    int alen = br.ReadInt32();
                    if (alen > 0)
                    {
                        aFactor = new double[row];
                        for (long i = 0; i < row; i++) aFactor[i] = br.ReadDouble();
                    }
                }
                br.Close();
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        public override void Clear()
        {
            A = null;
            E = null;
            aFactor = null;
            lineA = null;
            lineE = null;
            points.Clear();
            progressStep = 0;
            progressRow = 0;
            ReleasePointsMemory();
            //grid3d = null;
        }
       
       
       // /*
        //为避免过大或过小浮点运算，将距离矩阵单位简化
        bool CalculateDistanceMatrix()
        {
            int row = points.Count;

            double d;
            long id = 0;

            DateTime t0 = DateTime.Now;
            DateTime t1 = t0;
            DateTime t2 = t0;

            double sec = 0;
            double step = 100.0 / row;
            int nstep = (int)step;
            if ( nstep < 1 ) nstep = 1;
            
            percentage = 0;

            try 
            {
                A = new double[row * row];
            }
            catch (Exception e)
            {
                errMsg = "Allocating memory failed,no enough memory on CPU.\n" + e.Message;
                return false;
            }
            
            double x1, y1, z1, x2, y2, z2;
            for (int i = 0; i < row; i++)
            {
                x1 = px[i];
                y1 = py[i];
                z1 = pz[i];
                for (int j = i + 1; j < row; j++ )
                {
                    x2 = px[j];
                    y2 = py[j];
                    z2 = pz[j];
                    
                    d = Math.Sqrt(  (x1-x2) * (x1-x2) +
                                    (y1-y2) * (y1-y2) +
                                    (z1-z2) * (z1-z2) );
                    //距离
                    A[i * row + j] = d;
                    
                    if (id == 0) { minDist = maxDist = d; }
                    else
                    {
                        if (d < minDist) minDist = d;
                        if (d > maxDist) maxDist = d;
                    }
                    id++;
                }

                if ( i % nstep == 0 )
                {
                    t2 = DateTime.Now;
                    sec = (t2 - t1).TotalSeconds;
                    timeLeft = (row - i - 1) * sec;
                    timeSlip = (t2-t0).TotalSeconds;
                    t1 = t2;
                }
                percentage += step;
            }
            /* 经过测试，此变换无意义
            //距离矩阵变换到1 - 101，避免过大的浮点运算
            for (int i = 0; i < row; i++)
            {
                for (int j = i + 1; j < row; j++)
                {
                    id = i * row + j;
                    A[id] = ( A[id] - minDist) / (maxDist - minDist) * 100 + 1.0;
                }
            }
            */
            //对称变换
            for (int i = 0; i < row; i++)
            {
                for (int j = i + 1; j < row; j++)
                {
                    A[j * row + i] = A[i * row + j];
                }
            }

            //对角线本来是0值，赋予一个极小值
            for (int i = 0; i < row; i++) A[i * row + i] = 0.0001f;

            errMsg = "distance matrix created.";
            
            return true;
        }//end CalculateDistanceMatrix()

        bool CalculateWeightMatrix()
        {
            try 
            {               
                long id;
                int row = points.Count;
                aFactor = new double[row];
                for (int i = 0; i < row; i++)
                {
                    aFactor[i] = 0.0;
                    for (int j = 0; j < row; j++)
                    {
                        id = i * row + j;
                        aFactor[i] += E[id] * pv[j];
                    }
                }
                return true;
            }
            catch(Exception e)
            {
                errMsg = "计算权重矩阵失败！\n" + e.Message;
                return false;
            }                  
        }
        bool GridInterpolation()
        {
            try
            {
                double x, y, z, sum;
                grid3d = new float[ xGrid * yGrid * zGrid ];
                progressTitle = "网格计算...";
                DateTime t0 = DateTime.Now;
                DateTime t1 = t0;
                DateTime t2 = t0;

                double sec = 0;
                double step = 100.0 / zGrid;
                int nstep = (int)step;
                if (nstep < 1) nstep = 1;
                
                percentage = 0;
                for (int iz = 0; iz < zGrid; iz++)
                {                    
                    for (int iy = 0; iy < yGrid; iy++)
                    {                        
                        for (int ix = 0; ix < xGrid; ix++)
                        {
                            x = GetScaledX(minx + ix * xstep); 
                            y = GetScaledY(miny + iy * ystep); 
                            z = GetScaledZ(minz + iz * zstep);
                            sum = 0;
                            for( int j = 0; j < points.Count; j++ )
                            {
                                sum += aFactor[j]* Math.Sqrt( (x - px[j]) * (x - px[j]) +
                                                        (y - py[j]) * (y - py[j]) +
                                                        (z - pz[j]) * (z - pz[j]));
                            }
                            grid3d[ix + iy * xGrid + iz * xGrid * yGrid] = (float)sum;
                        }
                    }
                    if (iz % nstep == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = (zGrid - iz - 1) * sec;
                        timeSlip = (t2 - t0).TotalSeconds;
                        t1 = t2;
                    }
                    percentage += step;
                }
                
                return true;
            }
            catch (Exception e)
            {
                errMsg = "计算权重矩阵失败！\n" + e.Message;
                return false;
            }

        }

        double GetInterpolatedValueByThread(double x,double y,double z)
        {
            double r, sum1 = 0;
            for (int j = 0; j < pointCount; j++)
            {
                r = Math.Sqrt((x - points[j].x) * (x - points[j].x) +
                               (y - points[j].y) * (y - points[j].y) +
                               (z - points[j].z) * (z - points[j].z) +
                               MINE * MINE);
                sum1 = sum1 + r * aFactor[j];
            }
            return sum1;
        }
        /// <summary>
        /// 串行求单点值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override double GetInterpolatedValue(double x, double y, double z)
        {
            int n = points.Count;
            int i, j;
            double r, sum = 0, sum1 = 0;            
            DenseMatrix QMatix = new DenseMatrix(n, n);
            //计算|Pi-Pj|矩阵Q，这里可能需要检查重点
            Vector32 p1, p2;
            for (i = 0; i < n; i++)
            {
                p1 = points[i];
                for (j = 0; j < n; j++)
                {
                    p2 = points[j];
                    r = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                   (p1.y - p2.y) * (p1.y - p2.y) +
                                   (p1.z - p2.z) * (p1.z - p2.z) +
                                    MINE * MINE);
                    QMatix[i, j] = r;
                }
            }

            Matrix<double> qm = QMatix.Inverse();
            aFactor = new double[n];

            //求伴随矩阵a[j]
            for (i = 0; i < n; i++)
            {
                sum = 0.0;
                for (j = 0; j < n; j++)
                {
                    sum += qm[i, j] * points[j].v;
                }
                aFactor[i] = sum;
            }
            qm.Clear();
            QMatix.Clear();

            sum1 = 0.0;
            //************************************************************************	
            for (j = 0; j < n; j++)
            {
                r = Math.Sqrt((x - points[j].x) * (x - points[j].x) +
                               (y - points[j].y) * (y - points[j].y) +
                               (z - points[j].z) * (z - points[j].z) +
                               MINE * MINE);
                sum1 = sum1 + r * aFactor[j];
            }
            aFactor = null;
            return sum1;
        }

        public override float[] GetInterpolatedValue(int nx, int ny, int nz, Device[] devices)
        {
            SetGrid(nx, ny, nz);

            if (devices == null)
            {
                return GetInterpolatedValueCPU(nx, ny, nz);
                //return GetInterpolatedValueCPUAnisotropy(nx, ny, nz,1);
            }

            if ( progressStep < 1 ) percentage = 0;

            startTime = DateTime.Now;
            progressTitle = "正在计算所需内存...";
            
            ulong requireMemorySize = GetRequiredMemorySizeOnCPU();
            //double total = obj.GetCpuTotalMemory();

            ulong cpuAvailableMemorySize = MMPhysicalMemory.GetAvailableMemorySize();
            if ( requireMemorySize >= cpuAvailableMemorySize )
            {
                errMsg = "no enough memory available on CPU device.\n";
                errMsg += "Required:" + Math.Round((double)requireMemorySize / 1024 / 1024,2) + "MB\n";
                errMsg += "Available:" + Math.Round((double)cpuAvailableMemorySize / 1024 / 1024,2) + "MB\n";
                return null;
            }

            ulong sum = 0;
            foreach (Device d in devices)
            {
                sum += OpenCLObj.GetMaxAllocMemorySize(d);
            }

            if( sum < requireMemorySize )
            {
                errMsg += "no enough memory available on GPU devices.\n";
                errMsg += "Required:" + Math.Round((double)requireMemorySize / 1024 / 1024,2) + "MB\n";
                errMsg += "Available:" + Math.Round((double)sum / 1024 / 1024,2) + "MB\n";
                return null;
            }
           
            progressTitle = "初始化GPU设备...";
            if ( !InitDevices(devices) )
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "正在编译...";
            if ( !CompileDevice( InterpolateSource ) )
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "计算距离矩阵...";
            if ( !CreatePointsMemory(true) )
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "计算距离矩阵...";
            if (progressStep < 1) 
            {
                if( !CalculateDistanceMatrix() )
                {
                    Clear();
                    ReleaseDevices();
                    progressTitle = "失败，进程终止！";
                    return null;
                }
                progressStep = 1;
            }

            /*
            if ( !StartDistMatrixCalculate(clDevices[0]) )
            {
                Clear();
                ReleaseDevices();
                return null;
            } 
            */
            if ( progressStep < 2 )
            {
                long row = points.Count;
                progressTitle = "正在分配设备内存...";
                //分配任务到设备
               // long size = (row + 2) * sizeof(float) * 2;
               // AllocateInverseTasks(row, size);
                
                progressTitle = "正在计算逆矩阵...";
                if ( !StartMultiThreadMatrixInverse2(progressRow, percentage) )
                {
                    progressTitle = "正在保存当前进度....";
                    SaveProgress(progressFile);
                    Clear();
                    ReleaseDevices();
                    progressTitle = "失败，进程终止！";
                    return null;
                }
                progressStep = 2;
                ReleaseMatrixInverseMemory();
            }

            progressTitle = "计算伴随矩阵...";
            if ( progressStep < (int)RBFInterpolationStep.weightedCompleted )
            {                
                if ( !CalculateWeightMatrix() )
                {
                    progressTitle = "正在保存当前进度....";
                    SaveProgress(progressFile);
                    Clear();
                    ReleaseDevices();
                    progressTitle = "失败，进程终止！";
                    return null;
                }
                progressStep = 3;
                E = null;
            }

            /*
            if (!StartWeightMatrixCalculate(cl))
            {
                Clear();
                ReleaseDevices();
                return null;
            }
           */
           if( progressStep < (int)RBFInterpolationStep.allCompleted )
            {
                progressTitle = "计算网格单元...";

                //分配任务到设备
                //AllocateGridTasks( (ulong) points.Count );
                if ( ! StartMultiGridInterpolation() )
                //if (!GridInterpolation())
                {
                    progressTitle = "正在保存当前进度....";
                    SaveProgress(progressFile);
                    Clear();
                    ReleaseDevices();
                    progressTitle = "失败，进程终止！";
                    return null;
                }
            }
            
            progressStep = 4;
            aFactor = null;
            percentage = 100;
            
            Clear();
            ReleaseDevices();
            DateTime endTime = DateTime.Now;
            timeSlip = (endTime - startTime).TotalSeconds;
            return grid3d;
        }

        //矩阵求逆第一步，化第A[K,K]为1
        public bool MatrixInverse1(CLInterpolationUnit clUnit)
        {
            int row = pointNum;
            int currow = clUnit.curRow;

            //配置任务
            int tasksize = row - clUnit.curRow;
            clUnit.SetWorkItemSize( tasksize + row );

            int[] workItemSizes = new int[] { clUnit.workItemSize };
            int[] globalWorkItemSizes = new int[] { clUnit.groupNumber* clUnit.workItemSize };
            
            //拷贝K行数据到unit中
            double bs = A[currow * row + currow];//行首数据
            A[currow * row + currow] = 1;        //直接置为1
            
            lock (lock2)
            {
                Array.Copy(A, currow * row, clUnit.A, 0, row);
                Array.Copy(E, currow * row, clUnit.E, 0, row);
            }

            //创建内存对象
            var A1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, clUnit.A.Length * sizeof(double));
            var E1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, clUnit.E.Length * sizeof(double));
            clUnit.oclCQ.EnqueueWriteBuffer(A1, true, 0, clUnit.A.Length * sizeof(double), clUnit.A.ToDoublePtr());
            clUnit.oclCQ.EnqueueWriteBuffer(E1, true, 0, clUnit.E.Length * sizeof(double), clUnit.E.ToDoublePtr());
            //启动 row - currow + row个线程，按列循环,列号=线程号
            // 0 -> (row - currow)   线程计算A,
            // 线程号 > (row - currow)计算E
            clUnit.Kernels["MatrixInverse1"].SetArg(0, A1);
            clUnit.Kernels["MatrixInverse1"].SetArg(1, E1);
            clUnit.Kernels["MatrixInverse1"].SetArg(2, bs);
            clUnit.Kernels["MatrixInverse1"].SetArg(3, currow);
            clUnit.Kernels["MatrixInverse1"].SetArg(4, row);
            clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["MatrixInverse1"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
            clUnit.oclCQ.EnqueueBarrier();
            clUnit.oclCQ.EnqueueReadBuffer(E1, true, 0, clUnit.E.Length * sizeof(double), clUnit.E.ToDoublePtr());
            clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, clUnit.A.Length * sizeof(double), clUnit.A.ToDoublePtr());
            clUnit.oclCQ.Finish();

            //拷贝内存回A，E
            lock(lock2)
            {
                Array.Copy(clUnit.A, 0, A, currow * row, row);
                Array.Copy(clUnit.E, 0, E, currow * row, row);
            }

            return true;
        }
        /*
        public void Inverse()
        {
            long id, id0;
            double bs;

            int row = pointNum;

            E = new double[pointNum * pointNum];
            for (int i = 0; i < row * row; i++) E[i] = 0;
            for (int i = 0; i < row; i++) E[i * row + i] = 1;

            for (int k = 0; k < row; k++)
            {
                id = k * row + k;
                bs = A[id];
                A[id] = 1;
                for (int j = k + 1; j < row; j++)
                {
                    id = k * row + j;
                    A[id] = (float)(A[id] / bs);
                }
                for (int j = 0; j < row; j++)
                {
                    id = k * row + j;
                    E[id] = (float)(E[id] / bs);
                }

                for (int i = 0; i < row; i++)
                {
                    if (i != k)
                    {
                        bs = A[i * row + k];
                        for (int j = k; j < row; j++)
                        {
                            A[i * row + j] -= (float)(bs * A[k * row + j]);
                        }
                        for (int j = 0; j < row; j++)
                        {
                            E[i * row + j] -= (float)(bs * E[k * row + j]);
                        }
                    }
                }
            }
            A = null;
        }
        */

        /*
        public bool StartDistMatrixCalculate(OpenCLComputing clUnit)
        {
            #region 创建变量赋值

            bool ret = false;
            int row = points.Count;
            int np = 1;
            int[] workItemSizes = new int[2];
            int[] globalWorkItemSizes = new int[2];

            //计算workItemSize
            int itemSize = (int)Math.Sqrt(clUnit.oclDevice.MaxWorkGroupSize);
            
            if ( row <= itemSize ) itemSize = row;
            else
            { 
                np = row / itemSize;
                if (row % itemSize > 0) np++;
            }
            workItemSizes[0] = workItemSizes[1] = itemSize;
            globalWorkItemSizes[0] = np * workItemSizes[0];
            globalWorkItemSizes[1] = np * workItemSizes[1];

            for (int i = 0; i < row; i++)
            {
                px[i] = (float)points[i].x;
                py[i] = (float)points[i].y;
                pz[i] = (float)points[i].z;
            }

            #endregion  

            #region 调用gpuThread核

            DateTime t1 = DateTime.Now;
            //在显存创建缓冲区并把HOST的数据拷贝过去                
            try
            {
                var A1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, A.Length * sizeof(float), A.ToFloatPtr());
                var px1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, px.Length * sizeof(float), px.ToFloatPtr());
                var py1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, py.Length * sizeof(float), py.ToFloatPtr());
                var pz1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pz.Length * sizeof(float), pz.ToFloatPtr());

                clUnit.Kernels["distCalculate"].SetArg(0, A1);
                clUnit.Kernels["distCalculate"].SetArg(1, px1);
                clUnit.Kernels["distCalculate"].SetArg(2, py1);
                clUnit.Kernels["distCalculate"].SetArg(3, pz1);
                clUnit.Kernels["distCalculate"].SetArg(4, row);

                //把调用请求添加到队列里,参数分别是:Kernel,数据的维度1,每个维度的全局工作项ID偏移0,每个维度工作项数量4,每个维度的工作组长度(这里设为每4个一组)
                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["distCalculate"], 2, new[] { 0, 0 }, globalWorkItemSizes, workItemSizes);
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                clUnit.oclCQ.EnqueueBarrier();
                //添加一个读取数据命令到队列里,用来读取运算结果
                clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, A.Length * sizeof(double), A.ToFloatPtr());

                clUnit.oclCQ.Finish();

                A1.Dispose();
                px1.Dispose();
                py1.Dispose();
                pz1.Dispose();

                ret = true;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                ret = false;
            }

            #endregion
            
            return ret;
        }
        //计算系数矩阵
        public bool StartWeightMatrixCalculate(OpenCLComputing clUnit)
        {
            #region 创建变量赋值            

            bool ret = false;

            int row = points.Count;
            int np = 1;
            int[] workItemSizes = new int[1];
            int[] globalWorkItemSizes = new int[1];

            int itemSize = (int)clUnit.oclDevice.MaxWorkGroupSize;
            workItemSizes[0] = itemSize;

            if ( row <= itemSize )workItemSizes[0] = row;
            else
            {
                np = row / itemSize;
                if (row % itemSize > 0) np++;
                workItemSizes[0] = itemSize;                
            }

            globalWorkItemSizes[0] = np * workItemSizes[0];

            float[] pv;
            try
            {
                a = new double[points.Count];
                pv = new float[points.Count];
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return ret;
            }

            for (int i = 0; i < row; i++)
            {
                pv[i] = (float)points[i].v;
            }

            #endregion            

            long workgroup = clUnit.oclDevice.MaxWorkGroupSize;

            #region 调用gpuThread核

            DateTime t1 = DateTime.Now;
            //在显存创建缓冲区并把HOST的数据拷贝过去                
            try
            {
                var E1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, E.Length * sizeof(float), E.ToFloatPtr());
                var pv1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pv.Length * sizeof(float), pv.ToFloatPtr());
                var a1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, a.Length * sizeof(double), a.ToDoublePtr());

                clUnit.Kernels["weightCalculate"].SetArg(0, E1);
                clUnit.Kernels["weightCalculate"].SetArg(1, pv1);
                clUnit.Kernels["weightCalculate"].SetArg(2, a1);
                clUnit.Kernels["weightCalculate"].SetArg(3, pointNum);

                //把调用请求添加到队列里,参数分别是:Kernel,数据的维度1,每个维度的全局工作项ID偏移0,每个维度工作项数量4,每个维度的工作组长度(这里设为每4个一组)
                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["weightCalculate"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                clUnit.oclCQ.EnqueueBarrier();
                //添加一个读取数据命令到队列里,用来读取运算结果
                clUnit.oclCQ.EnqueueReadBuffer(a1, true, 0, a.Length * sizeof(double), a.ToDoublePtr());

                clUnit.oclCQ.Finish();

                E1.Dispose();
                pv1.Dispose();
                a1.Dispose();

                ret = true;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                ret = false;
            }

            pv = null;
            A = null;
            E = null;

            #endregion

            return ret;
        }
        public bool StartMatrixInverse(OpenCLComputing clUnit)
        {
            double minerr = 1.0E-30;

            int row = pointNum;
            E = new float[pointNum * pointNum];
            for (int i = 0; i < row * row; i++) E[i] = 0;
            for (int i = 0; i < row; i++) E[i * row + i] = 1;

            int[] workItemSizes = new int[1];
            int[] groupItemSizes = new int[1];
            int[] globalWorkItemSizes = new int[1];

            int itemSize = (int)clUnit.oclDevice.MaxWorkGroupSize;
            workItemSizes[0] = itemSize;
            if (pointNum <= itemSize)
            {
                workItemSizes[0] = row;
                groupItemSizes[0] = 1;
            }
            else
            {
                int np = row / itemSize;
                if (row % itemSize > 0) np++;
                workItemSizes[0] = itemSize;
                groupItemSizes[0] = np;
            }

            globalWorkItemSizes[0] = groupItemSizes[0] * workItemSizes[0];          

            #region 调用gpuThread核

            double ts = 100.0 / row;
            double sec = 0;

            int step = row / 100;

            long id;
            double bs = 0;
            int[] currow = new int[1];

            var A1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, A.Length * sizeof(float));
            var E1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, E.Length * sizeof(float));

            DateTime t1 = DateTime.Now;

            bool beginStep1 = false;

            for (int k = 0; k < row; k++)
            {
                //------------Step 1 --------------
                currow[0] = k;
                id = k * row + k;
                if (A[id] >= 1 - minerr && A[id] <= 1 + minerr)
                {
                    A[id] = 1;
                }
                else
                {
                    bs = A[id];
                    A[id] = 1;                    
                    for (int j = k + 1; j < row; j++)
                    {
                        id = k * row + j;
                        A[id] = (float)(A[id] / bs);
                    }
                    for (int j = 0; j < row; j++)
                    {
                        id = k * row + j;
                        E[id] =(float)(E[id]/ bs);
                    }                    
                }
                //end ------------Step 1 --------------

                // ------------Step 2 --------------
                //创建内存对象
                clUnit.oclCQ.EnqueueWriteBuffer(A1, true, 0, A.Length * sizeof(float), A.ToFloatPtr());
                clUnit.oclCQ.EnqueueWriteBuffer(E1, true, 0, E.Length * sizeof(float), E.ToFloatPtr());

                #region Inverse Step1
                if (beginStep1)
                {
                    clUnit.Kernels["MatrixInverse1"].SetArg(0, A1);
                    clUnit.Kernels["MatrixInverse1"].SetArg(1, E1);
                    clUnit.Kernels["MatrixInverse1"].SetArg(2, bs);
                    clUnit.Kernels["MatrixInverse1"].SetArg(3, k);
                    clUnit.Kernels["MatrixInverse1"].SetArg(4, row);
                    clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["MatrixInverse1"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                    clUnit.oclCQ.EnqueueBarrier();
                    clUnit.oclCQ.EnqueueReadBuffer(E1, true, 0, E.Length * sizeof(float), E.ToFloatPtr());
                    clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, A.Length * sizeof(float), A.ToFloatPtr());
                    clUnit.oclCQ.Finish();
                }
                #endregion Inverse Step1

                #region Inverse Step2
                clUnit.Kernels["MatrixInverse2"].SetArg(0, A1);
                clUnit.Kernels["MatrixInverse2"].SetArg(1, E1);
                clUnit.Kernels["MatrixInverse2"].SetArg(2, k);
                clUnit.Kernels["MatrixInverse2"].SetArg(3, row);
                clUnit.Kernels["MatrixInverse2"].SetArg(4, 0);
                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["MatrixInverse2"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                clUnit.oclCQ.EnqueueBarrier();
                clUnit.oclCQ.EnqueueReadBuffer(E1, true, 0, E.Length * sizeof(float), E.ToFloatPtr());
                clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, A.Length * sizeof(float), A.ToFloatPtr());
                clUnit.oclCQ.Finish();
                #endregion Inverse Step2

                if (k == 0)
                {
                    DateTime t2 = DateTime.Now;
                    sec = (t2 - t1).TotalSeconds;
                }
                if (k % step == 0)
                {
                    timeLeft = (row - k - 1) * sec;
                    timeSlip += (k + 1) * sec;
                    percentage += ts;
                }
            }//for (int k = 0; k < row; k++)

            A1.Dispose();
            E1.Dispose();

            #endregion

            //按顺序释放之前构造的对象
            //oclCQ.Dispose();
            //oclContext.Dispose();
            //oclDevice.Dispose();
            A = null;
            return true;
        }
        */
        public override void ReleaseDevices()
        {
            if (clDevices != null)
            {
                for (int i = 0; i < clDevices.Length; i++)
                {
                    if( clDevices[i] != null ) 
                        clDevices[i].ReleaseCL();
                }
            }
            clDevices = null;            
        }
        
        bool IsTaskFull()
        {
            bool full = true;
            foreach (CLInterpolationUnit cl in clDevices )
            {
                if ( !cl.MaxAllocated )full = false;
            }
            return full;
        }

        long GetAllocatedTasks()
        {
            long sum = 0;
            foreach (CLInterpolationUnit cl in clDevices)
            {
                sum += cl.taskNumbers;
            }
            return sum;
        }       

        //根据每个设备能力，计算分配任务的比例
        double GetUnitAvailableScale(CLInterpolationUnit device)
        {
            if ( device.MaxAllocated ) return 0;
            ulong sum = 0;
            foreach (CLInterpolationUnit cl in clDevices)
            {
                //根据内存大小来分配
                if (!cl.MaxAllocated) sum += cl.MaxAllocMemorySize;
            }
            return device.MaxAllocMemorySize / (double)sum;            
        }

        //
        /// <summary>
        /// 矩阵求逆任务分配，到各个设备单元中
        /// </summary>
        /// <param name="alltask">总任务数</param>
        /// <param name="size">每个任务需内存字节数</param>
        /// <returns>是否分配任务成功</returns>
        public bool AllocateInverseTasks( long alltask, long size)
        {
            long num = 0;
            long maxnum = 0;
            long left = alltask;

            for (int i = 0; i < clDevices.Length; i++)
            {
                clDevices[i].taskNumbers = 0;
                clDevices[i].MaxAllocated = false;
            }
            long allocated = 0;
            while ( left > 0 )//
            {   
                if ( IsTaskFull() ) break;
                
                allocated = 0;
                for (int i = 0; i < clDevices.Length; i++ )
                {
                    if (clDevices[i].MaxAllocated) continue;

                    if ( left - allocated < 1 ) break;

                    num = (long)(left * GetUnitAvailableScale(clDevices[i]));
                    if (num < 1) num = 1;

                    maxnum = (long)clDevices[i].GetMaxTaskNumberOnMemory( size );
                    
                    if( i== clDevices.Length - 1) //最后一个
                        num = left - allocated;
                    
                    if ( clDevices[i].taskNumbers + num >= maxnum )
                    {
                        num = maxnum - clDevices[i].taskNumbers;
                        clDevices[i].MaxAllocated = true;
                    }

                    clDevices[i].taskNumbers += num;
                    allocated += num;
                }
                left = alltask - GetAllocatedTasks();
            }//while ( leftrow > 0  )

            if (left > 0) //任务未分配完
            {
                errMsg = "no enough memory on Device.";
                return false;
            }
            else
            {
                for (int i = 0; i < clDevices.Length; i++)
                { 
                    clDevices[i].SetWorkItemSize((int)clDevices[i].taskNumbers); 
                }
                return true;
            }
        }
        /// <summary>
        /// 网格插值线程任务分配，考虑设备的workItem单元计算能力
        /// </summary>
        /// <param name="alltask"></param>        
        /// <returns></returns>
        public bool AllocateGridTasks(long alltask)
        {
            long num = 0;            
            for (int i = 0; i < clDevices.Length; i++)
            {
                clDevices[i].taskNumbers = 0;
                clDevices[i].MaxAllocated = false;
            }
            long alocated = 0;
            for (int i = 0; i < clDevices.Length; i++)
            {
                num = (long)( alltask * GetUnitAvailableScale(clDevices[i]) );
                if( i== clDevices.Length - 1)
                {
                    num = alltask - alocated;
                }                
                clDevices[i].SetWorkItemSize((int)num);
                alocated += num;
            }
            return true;
        }

        public override bool CompileDevice(string source)
        {
            for (int i = 0; i < clDevices.Length; i++)
            {                
                clDevices[i].AddFunction("distCalculate");
                clDevices[i].AddFunction("MatrixInverse1");
                clDevices[i].AddFunction("MatrixInverse2");
                clDevices[i].AddFunction("weightCalculate");                
                
                clDevices[i].AddFunction("gridInterpolate");
                if (!clDevices[i].InitDevice())
                {
                    errMsg = clDevices[i].errMsg;
                    return false;
                }
                if ( !clDevices[i].Compile(source) )
                {
                    errMsg = clDevices[i].errMsg;
                    return false;
                }                
            }
            return true;
        }

        enum RBFInterpolationStep
        {
            start = 0,
            distMatrixCompleted = 1,
            matrixInverseCompleted = 2,
            weightedCompleted = 3,
            allCompleted = 4,
        }
        
        void ReleaseMatrixInverseMemory()
        {
            A = null;
            lineA = null;
            lineE = null;
            for (int i = 0; i < clDevices.Length; i++)
            {
                clDevices[i].ReleaseMemory();
            }
        }
        
        public bool StartMultiThreadMatrixInverse2(int startrow = 0,double init_percent = 0)
        {
            double minerr = 1.0E-20;
            
            progressTitle = "正在计算逆矩阵...";
            percentage = init_percent;

            int row = pointNum;
            
            long id;
            double bs = 0;
            double sec = 0;

            double step = 100.0 / row ;            
            int nstep = (int)step;
            if (nstep == 0) nstep = 1;
            
            DateTime t0 = DateTime.Now;
            DateTime t1 = t0;
            DateTime t2 = t0;

            int batch = 500;

            try
            {
                if ( E == null )
                {
                    E = new double[row * row];
                    for (int i = 0; i < row; i++) E[i * row + i] = 1;
                }

                lineA = new double[row];
                lineE = new double[row];

                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].A = new double[row * batch];
                    clDevices[i].E = new double[row * batch];
                }
                
                ThreadPool.SetMaxThreads(clDevices.Length, clDevices.Length);

                //startrow，已保存的开始行位置
                for ( int k = startrow; k < row; k++ )
                {
                    if (threadStoped) break;//线程被终止

                    progressRow = k;
                    //------------Step 1 -------------- 
                    id = k * row + k;
                    if (A[id] >= 1 - minerr && A[id] <= 1 + minerr)
                    {
                        A[id] = 1;
                    }
                    else
                    {
                        bs = A[id];
                        A[id] = 1;
                        for (int j = k + 1; j < row; j++)
                        {
                            id = k * row + j;
                            A[id] = (A[id] / bs);
                        }
                        for (int j = 0; j < row; j++)
                        {
                            id = k * row + j;
                            E[id] = (E[id] / bs);
                        }
                    }

                    //创建任务队列                
                    CreateInverseTaskPools(row, batch);

                    //复制第k行
                    Array.Copy(A, k * row, lineA, 0, row);
                    Array.Copy(E, k * row, lineE, 0, row);
                    
                    //Thread[] threads = new Thread[clDevices.Length];
                    //更新线程参数
                    for (int i = 0; i < clDevices.Length; i++)
                    {   
                        clDevices[i].curRow = k;
                        clDevices[i].curLineA = lineA;
                        clDevices[i].curLineE = lineE;
                        ThreadPool.QueueUserWorkItem(MatrixInverseThread3, clDevices[i]);
                        //threads[i] = new Thread(MatrixInverseThread2);
                        //threads[i].Start(clDevices[i]);
                    }

                    //------------Step 2 ------------------- 
                    //等待线程结束
                    for (int i = 0; i < clDevices.Length; i++)
                    {
                        clDevices[i].WaitSiginal();
                    }

                    //threads = null;
                    if (threadStoped) break;//线程被终止

                    //内存线程内已拷贝
                    if (k % nstep == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = (row - k - 1) * sec;
                        timeSlip = (t2 - t0).TotalSeconds;
                        t1 = t2;
                    }

                    percentage += step;
                }//for (int k = 0; k < row; k++)

                //release memory
                A = null;
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].ReleaseMemory();
                }
                if (threadStoped)
                {
                    progressTitle = "user breaked.";
                    errMsg = "user breaked.";
                    return false;//线程被终止
                }
                else return true;
            }
            catch (Exception e)
            {
                errMsg = "matrix inverse failed.\n" + e.Message;
                return false;
            }
        }
        /*
         public void MatrixInverseThread1(object para)
        {
            int row = points.Count;
            OpenCLComputing clUnit = (OpenCLComputing)para;
            
            int end = clUnit.startRow + clUnit.taskNumbers;

            int[] workItemSizes = new int[] { (int)clUnit.workItemSize };
            int[] globalWorkItemSizes = new int[] { (int)clUnit.totalWorkSize };

            //创建内存 #region 调用gpuThread核     
            var A1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, clUnit.A.Length * sizeof(float));
            var E1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, clUnit.E.Length * sizeof(float));
            var A2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY, clUnit.curLineA.Length * sizeof(float));
            var E2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY, clUnit.curLineE.Length * sizeof(float));
            //创建内存对象
            clUnit.oclCQ.EnqueueWriteBuffer(A1, true, 0, clUnit.A.Length * sizeof(float), clUnit.A.A.ToFloatPtr());
            clUnit.oclCQ.EnqueueWriteBuffer(E1, true, 0, clUnit.E.Length * sizeof(float), clUnit.E.A.ToFloatPtr());
            clUnit.oclCQ.EnqueueWriteBuffer(A2, true, 0, clUnit.curLineA.Length * sizeof(float), clUnit.curLineA.ToFloatPtr());
            clUnit.oclCQ.EnqueueWriteBuffer(E2, true, 0, clUnit.curLineE.Length * sizeof(float), clUnit.curLineE.ToFloatPtr());

            #region Inverse Step2
            clUnit.Kernels["MatrixInverse2"].SetArg(0, A1);
            clUnit.Kernels["MatrixInverse2"].SetArg(1, E1);
            clUnit.Kernels["MatrixInverse2"].SetArg(2, A2);
            clUnit.Kernels["MatrixInverse2"].SetArg(3, E2);
            clUnit.Kernels["MatrixInverse2"].SetArg(4, clUnit.curRow);
            clUnit.Kernels["MatrixInverse2"].SetArg(5, row);
            clUnit.Kernels["MatrixInverse2"].SetArg(6, clUnit.startRow);
            clUnit.Kernels["MatrixInverse2"].SetArg(7, end);
            clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["MatrixInverse2"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
            clUnit.oclCQ.EnqueueBarrier();
            clUnit.oclCQ.EnqueueReadBuffer(E1, true, 0, clUnit.E.Length * sizeof(float), clUnit.E.A.ToFloatPtr());
            clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, clUnit.A.Length * sizeof(float), clUnit.A.A.ToFloatPtr());
            clUnit.oclCQ.Finish();
            A1.Dispose();
            E1.Dispose();
            A2.Dispose();
            E2.Dispose();
            #endregion Inverse Step2

            clUnit.ReleaseSiginal();
        }
        public bool StartMultiThreadMatrixInverse1(int startrow = 0, double init_percent = 0)
        {
            double minerr = 1.0E-30;

            progressTitle = "正在计算逆矩阵...";
            percentage = init_percent;

            int row = pointNum;

            long id;
            double bs = 0;
            double sec = 0;

            double step = 100.0 / row;
            int nstep = (int)step;
            if (nstep == 0) nstep = 1;

            DateTime t0 = DateTime.Now;
            DateTime t1 = t0;
            DateTime t2 = t0;
            //启动计算线程
            int start = 0;

            E = new double[row * row];
            for (int i = 0; i < row; i++) E[i * row + i] = 1;
            lineA = new double[row];
            lineE = new double[row];

            for (int i = 0; i < clDevices.Length; i++)
            {
                if (!clDevices[i].CreateMatrixAE(row))
                {
                    errMsg = "allocating memory failed,no enough memory on GPU device.";
                    return false;
                }
            }

            Thread[] threads = new Thread[clDevices.Length];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(MatrixInverseThread1);
                threads[i].Start(clDevices[i]);
            }

            for (int k = startrow; k < row; k++)
            {
                progressRow = k;
                //------------Step 1 --------------                
                id = k * row + k;
                if (A[id] >= 1 - minerr && A[id] <= 1 + minerr)
                {
                    A[id] = 1;
                }
                else
                {
                    bs = A[id];
                    A[id] = 1;
                    for (int j = k + 1; j < row; j++)
                    {
                        id = k * row + j;
                        A[id] = (A[id] / bs);
                    }
                    for (int j = 0; j < row; j++)
                    {
                        id = k * row + j;
                        E[id] = (E[id] / bs);
                    }
                }

                Array.Copy(A, k * row, lineA, 0, row);
                Array.Copy(E, k * row, lineE, 0, row);
                //end ------------Step 1 --------------   

                //更新线程参数
                start = 0;
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].SetUnit(k, start);
                    clDevices[i].CopyMemoryFrom(A, E);
                    clDevices[i].curLineA = lineA;
                    clDevices[i].curLineE = lineE;
                    start += (int)clDevices[i].taskNumbers;
                    clDevices[i].externSema.Release();
                }

                //------------Step 2 -------------------  
                
                //等待线程结束
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].WaitSiginal();
                }
               
                for (int i = 0; i < clDevices.Length; i++)
                    clDevices[i].CopyMemoryTo(ref A, ref E);

                //恢复第k行数据 ,destructed by Inverse
                if (clDevices.Length > 0)
                {
                    Array.Copy(lineA, 0, A, k * row, row);
                    Array.Copy(lineE, 0, E, k * row, row);
                }

                if (k % nstep == 0)
                {
                    t2 = DateTime.Now;
                    sec = (t2 - t1).TotalSeconds;
                    timeLeft = (row - k - 1) * sec;
                    timeSlip = (t2 - t0).TotalSeconds;
                    t1 = t2;
                }

                percentage += step;
            }//for (int k = 0; k < row; k++)

            //release memory
            A = null;
            for (int i = 0; i < clDevices.Length; i++)
            {
                clDevices[i].ReleaseMemory();
                threads[i] = null;
            }
            threads = null;
            return true;
        }
        */
        /*
        //currow = 当前行首为1的行 A[currow,currow] = 1
        //矩阵求逆，多线程计算第2步
        //从start行开始，共num计算步
        public void MatrixInverseThread(object para)
        {
            int row = points.Count;
            OpenCLComputing clUnit = (OpenCLComputing)para;
            
            int end = clUnit.startRow + clUnit.taskNumbers;

            int[] workItemSizes = new int[] { (int)clUnit.workItemSize };
            int[] globalWorkItemSizes = new int[] { (int)clUnit.totalWorkSize };

            //创建内存 #region 调用gpuThread核     
            var A1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, clUnit.A.Length * sizeof(float));
            var E1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE, clUnit.E.Length * sizeof(float));
            var A2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY, clUnit.curLineA.Length * sizeof(float));
            var E2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY, clUnit.curLineE.Length * sizeof(float));
            //创建内存对象
            clUnit.oclCQ.EnqueueWriteBuffer(A1, true, 0, clUnit.A.Length * sizeof(float), clUnit.A.A.ToFloatPtr());
            clUnit.oclCQ.EnqueueWriteBuffer(E1, true, 0, clUnit.E.Length * sizeof(float), clUnit.E.A.ToFloatPtr());
            clUnit.oclCQ.EnqueueWriteBuffer(A2, true, 0, clUnit.curLineA.Length * sizeof(float), clUnit.curLineA.ToFloatPtr());
            clUnit.oclCQ.EnqueueWriteBuffer(E2, true, 0, clUnit.curLineE.Length * sizeof(float), clUnit.curLineE.ToFloatPtr());

            #region Inverse Step2
            clUnit.Kernels["MatrixInverse2"].SetArg(0, A1);
            clUnit.Kernels["MatrixInverse2"].SetArg(1, E1);
            clUnit.Kernels["MatrixInverse2"].SetArg(2, A2);
            clUnit.Kernels["MatrixInverse2"].SetArg(3, E2);
            clUnit.Kernels["MatrixInverse2"].SetArg(4, clUnit.curRow);
            clUnit.Kernels["MatrixInverse2"].SetArg(5, row);
            clUnit.Kernels["MatrixInverse2"].SetArg(6, clUnit.startRow);
            clUnit.Kernels["MatrixInverse2"].SetArg(7, end);
            clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["MatrixInverse2"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
            clUnit.oclCQ.EnqueueBarrier();
            clUnit.oclCQ.EnqueueReadBuffer(E1, true, 0, clUnit.E.Length * sizeof(float), clUnit.E.A.ToFloatPtr());
            clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, clUnit.A.Length * sizeof(float), clUnit.A.A.ToFloatPtr());
            clUnit.oclCQ.Finish();
            A1.Dispose();
            E1.Dispose();
            A2.Dispose();
            E2.Dispose();
            #endregion Inverse Step2

            clUnit.ReleaseSiginal();
        }
        */

        ///*
        public void MatrixInverseThread2(object para)
        {
            int row = points.Count;

            CLInterpolationUnit clUnit = (CLInterpolationUnit)para; 
            TaskPoolIndices task = GetFromTaskPools();

            var A2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.ALLOC_HOST_PTR, clUnit.curLineA.Length * sizeof(double));
            var E2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.ALLOC_HOST_PTR, clUnit.curLineE.Length * sizeof(double));
            clUnit.oclCQ.EnqueueWriteBuffer(A2, true, 0, clUnit.curLineA.Length * sizeof(double), clUnit.curLineA.ToDoublePtr());
            clUnit.oclCQ.EnqueueWriteBuffer(E2, true, 0, clUnit.curLineE.Length * sizeof(double), clUnit.curLineE.ToDoublePtr());

            double[] bss = new double[ task.num ];
            while (!clUnit.Stop && !threadStoped && task.num > 0 )
            {                
                int curcol = clUnit.curRow;
                int num = task.num;
                int start = (int)task.start;   //当前处理行 

                clUnit.SetWorkItemSize(row + row - curcol);
                int[] workItemSizes = new int[] { (int)clUnit.workItemSize };
                int[] globalWorkItemSizes = new int[] { (int)clUnit.totalWorkSize };

                lock (lock1) 
                {
                    for (int i = 0; i < num; i++) 
                    {
                        bss[i] = A[(start+i) * row + curcol]; 
                    }
                }
               
                lock (lock1)
                {
                    Array.Copy(A, start * row , clUnit.A, 0, num * row);
                    Array.Copy(E, start * row , clUnit.E, 0, num * row);
                }
                //创建内存 #region 调用gpuThread核     
                var A1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE|MemFlags.ALLOC_HOST_PTR, clUnit.A.Length * sizeof(double));
                var E1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE|MemFlags.ALLOC_HOST_PTR, clUnit.E.Length * sizeof(double));
                var bs1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY|MemFlags.ALLOC_HOST_PTR, bss.Length * sizeof(double));
                
                //创建内存对象
                clUnit.oclCQ.EnqueueWriteBuffer(A1, true, 0, clUnit.A.Length * sizeof(double), clUnit.A.ToDoublePtr());
                clUnit.oclCQ.EnqueueWriteBuffer(E1, true, 0, clUnit.E.Length * sizeof(double), clUnit.E.ToDoublePtr());
                clUnit.oclCQ.EnqueueWriteBuffer(bs1, true, 0, bss.Length * sizeof(double), bss.ToDoublePtr());
               
                #region Inverse Step2
                clUnit.Kernels["MatrixInverse2"].SetArg(0, A1);
                clUnit.Kernels["MatrixInverse2"].SetArg(1, E1);
                clUnit.Kernels["MatrixInverse2"].SetArg(2, A2);
                clUnit.Kernels["MatrixInverse2"].SetArg(3, E2);
                clUnit.Kernels["MatrixInverse2"].SetArg(4, bs1);
                clUnit.Kernels["MatrixInverse2"].SetArg(5, curcol);
                clUnit.Kernels["MatrixInverse2"].SetArg(6, start);
                clUnit.Kernels["MatrixInverse2"].SetArg(7, num);
                clUnit.Kernels["MatrixInverse2"].SetArg(8, row);
                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["MatrixInverse2"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                clUnit.oclCQ.EnqueueBarrier();

                clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, clUnit.A.Length * sizeof(double), clUnit.A.ToDoublePtr());
                clUnit.oclCQ.EnqueueReadBuffer(E1, true, 0, clUnit.E.Length * sizeof(double), clUnit.E.ToDoublePtr());
                
                clUnit.oclCQ.Finish();
                A1.Dispose();
                E1.Dispose();                
                
                #endregion Inverse Step2
                
                lock (lock1)
                {
                    Array.Copy(clUnit.A, 0, A, start * row,  num * row);
                    Array.Copy(clUnit.E, 0, E, start * row,  num * row);
                }
                // 下一任务
                task = GetFromTaskPools();
            }
            A2.Dispose();
            E2.Dispose();
            bss = null;
            clUnit.ReleaseSiginal();
        }

        //test，使用内存映射到GPU设备
        public void MatrixInverseThread3(object para)
        {
            int row = points.Count;

            CLInterpolationUnit clUnit = (CLInterpolationUnit)para;
            TaskPoolIndices task = GetFromTaskPools();

            var A2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.USE_HOST_PTR, clUnit.curLineA.Length * sizeof(double), clUnit.curLineA.ToDoublePtr());
            var E2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.USE_HOST_PTR, clUnit.curLineE.Length * sizeof(double), clUnit.curLineE.ToDoublePtr());
            IntPtr ptA2 = clUnit.oclCQ.EnqueueMapBuffer(A2, true, MapFlags.READ, 0, clUnit.curLineA.Length * sizeof(double),0,null);
            IntPtr ptE2 = clUnit.oclCQ.EnqueueMapBuffer(E2, true, MapFlags.READ, 0, clUnit.curLineE.Length * sizeof(double), 0, null);
            
            double[] bss = new double[task.num];
            while (!clUnit.Stop && !threadStoped && task.num > 0)
            {
                int curcol = clUnit.curRow;
                int num = task.num;
                int start = (int)task.start;   //当前处理行 

                clUnit.SetWorkItemSize(row + row - curcol);
                int[] workItemSizes = new int[] { (int)clUnit.workItemSize };
                int[] globalWorkItemSizes = new int[] { (int)clUnit.totalWorkSize };

                lock (lock1)
                {
                    for (int i = 0; i < num; i++)
                    {
                        bss[i] = A[(start + i) * row + curcol];
                    }
                }

                lock (lock1)
                {
                    Array.Copy(A, start * row, clUnit.A, 0, num * row);
                    Array.Copy(E, start * row, clUnit.E, 0, num * row);
                }
                //创建内存 #region 调用gpuThread核   

                //创建内存对象
                var A1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.USE_HOST_PTR, clUnit.A.Length * sizeof(double), clUnit.A.ToDoublePtr());
                var E1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.USE_HOST_PTR, clUnit.E.Length * sizeof(double), clUnit.E.ToDoublePtr());
                var bs1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.USE_HOST_PTR, bss.Length * sizeof(double), bss.ToDoublePtr() );                
                
                //写入数据
                IntPtr ptA1 = clUnit.oclCQ.EnqueueMapBuffer(A1, true,MapFlags.READ_WRITE, 0, clUnit.A.Length * sizeof(double), 0,null);
                IntPtr ptE1 = clUnit.oclCQ.EnqueueMapBuffer(E1, true, MapFlags.READ_WRITE, 0, clUnit.E.Length * sizeof(double),0,null);
                IntPtr ptBss = clUnit.oclCQ.EnqueueMapBuffer(bs1, true, MapFlags.READ_WRITE, 0, bss.Length * sizeof(double), 0, null);
                
                #region Inverse Step2
                clUnit.Kernels["MatrixInverse2"].SetArg(0, A1);
                clUnit.Kernels["MatrixInverse2"].SetArg(1, E1);
                clUnit.Kernels["MatrixInverse2"].SetArg(2, A2);
                clUnit.Kernels["MatrixInverse2"].SetArg(3, E2);
                clUnit.Kernels["MatrixInverse2"].SetArg(4, bs1);
                clUnit.Kernels["MatrixInverse2"].SetArg(5, curcol);
                clUnit.Kernels["MatrixInverse2"].SetArg(6, start);
                clUnit.Kernels["MatrixInverse2"].SetArg(7, num);
                clUnit.Kernels["MatrixInverse2"].SetArg(8, row);
                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["MatrixInverse2"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                clUnit.oclCQ.EnqueueBarrier();

                
                clUnit.oclCQ.EnqueueReadBuffer(A1, true, 0, clUnit.A.Length * sizeof(double), clUnit.A.ToDoublePtr());
                clUnit.oclCQ.EnqueueReadBuffer(E1, true, 0, clUnit.E.Length * sizeof(double), clUnit.E.ToDoublePtr());

                if( !clUnit.oclCQ.EnqueueUnmapMemObject(E1, clUnit.E.ToDoublePtr()) )
                {
                    //throw new OpenCLException("EnqueueUnmapMemObject failed with error code ");
                }
                if( !clUnit.oclCQ.EnqueueUnmapMemObject(A1, clUnit.A.ToDoublePtr()) )
                {
                    //throw new OpenCLException("EnqueueUnmapMemObject failed with error code ");
                }
                if( !clUnit.oclCQ.EnqueueUnmapMemObject(bs1, bss.ToDoublePtr()) )
                {
                    //throw new OpenCLException("EnqueueUnmapMemObject failed with error code ");
                }

                clUnit.oclCQ.Finish();
                A1.Dispose();
                E1.Dispose();

                #endregion Inverse Step2

                lock (lock1)
                {
                    Array.Copy(clUnit.A, 0, A, start * row, num * row);
                    Array.Copy(clUnit.E, 0, E, start * row, num * row);
                }
                // 下一任务
                task = GetFromTaskPools();
            }

            if( !clUnit.oclCQ.EnqueueUnmapMemObject(A2, clUnit.curLineA.ToDoublePtr()) )
            {
                errMsg = "EnqueueUnmapMemObject failed with error code ";
                //throw new OpenCLException("EnqueueUnmapMemObject failed with error code ");
            }
            if( !clUnit.oclCQ.EnqueueUnmapMemObject(E2, clUnit.curLineE.ToDoublePtr()) )
            {
                errMsg = "EnqueueUnmapMemObject failed with error code ";
                //throw new OpenCLException("EnqueueUnmapMemObject failed with error code ");
            }
            
            A2.Dispose();
            E2.Dispose();
            bss = null;
            clUnit.ReleaseSiginal();
        }
        //创建任务队列
        int CreateInverseTaskPools(int row, int batch = 100)
        {            
            int num;
            taskPools.Clear();
            int total = row;
            long cur = 0;
            while (cur < total)
            {
                if ( total - cur >= batch ) num = batch;
                else num = (int)(total - cur);
                taskPools.Enqueue(new TaskPoolIndices(cur, num));
                cur += num;
            }
            return taskPools.Count;
        }
        
        public bool StartMultiGridInterpolation()
        {
            progressTitle = "正在进行网格插值计算...";
            percentage = 0;

            int batch = 1000;
            long total = xGrid * yGrid * zGrid;

            DateTime t0 = DateTime.Now;
            DateTime t1 = t0;
            DateTime t2 = t0;

            try 
            {
                percentage = 0;
                
                if (grid3d == null) grid3d = new float[total];
                
                CreateGriddingTaskPools(total, batch);
                
                ThreadPool.SetMaxThreads(clDevices.Length, clDevices.Length);

                //启动计算线程
                //Thread[] threads = new Thread[clDevices.Length];
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].Stop = false;
                    clDevices[i].grid = new float[batch];
                    clDevices[i].SetWorkItemSize(batch);
                    ThreadPool.QueueUserWorkItem(GridInterpolationThread, clDevices[i]);
                    //threads[i] = new Thread(GridInterpolationThread);
                    //threads[i].Start(clDevices[i]);
                }                

                //等待线程结束
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].WaitSiginal();
                }

                //release memory
                A = null;
                for (int i = 0; i < clDevices.Length; i++)
                {
                    clDevices[i].ReleaseMemory();
                    //threads[i] = null;
                }
                //threads = null;
                return true;
            }
            catch(Exception e)
            {
                errMsg = "gridding failed.\n" + e.Message;
                return false;
            }
            
        }
        public void GridInterpolationThread(Object para)
        {
            int np = points.Count;
            int taskCount = taskPools.Count;            
            double step = taskCount / 100.0;
            int nstep = (int)step;
            if (nstep == 0) nstep = 1;

            #region 线程配置------------------
            CLInterpolationUnit clUnit = (CLInterpolationUnit)para;
            int[] workItemSizes = new int[] { (int)clUnit.workItemSize };
            int[] globalWorkItemSizes = new int[] { (int)clUnit.totalWorkSize };
            #endregion 线程配置
            
            TaskPoolIndices task = GetFromTaskPools();

            double scale = 100 / MaxLength; // px, py, pz 已缩放到0-100之间

            //在显存创建缓冲区并把HOST的数据映射过去                                
            var px1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.USE_HOST_PTR, px.Length * sizeof(float), px.ToFloatPtr());
            var py1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.USE_HOST_PTR, py.Length * sizeof(float), py.ToFloatPtr());
            var pz1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.USE_HOST_PTR, pz.Length * sizeof(float), pz.ToFloatPtr());
            var a1 = clUnit.oclContext.CreateBuffer( MemFlags.READ_ONLY | MemFlags.USE_HOST_PTR, aFactor.Length * sizeof(double), aFactor.ToDoublePtr());
            IntPtr ptPx1 = clUnit.oclCQ.EnqueueMapBuffer(px1, true, MapFlags.READ, 0, px.Length * sizeof(float), 0, null);
            IntPtr ptPy1 = clUnit.oclCQ.EnqueueMapBuffer(py1, true, MapFlags.READ, 0, py.Length * sizeof(float), 0, null);
            IntPtr ptPz1 = clUnit.oclCQ.EnqueueMapBuffer(pz1, true, MapFlags.READ, 0, pz.Length * sizeof(float), 0, null);
            IntPtr ptA1 = clUnit.oclCQ.EnqueueMapBuffer(a1, true, MapFlags.READ, 0, aFactor.Length * sizeof(double), 0, null);

            while (!clUnit.Stop && !threadStoped && task.num > 0 )
            {
                int startid = (int)task.start;
                int num = task.num;

                workItemSizes = new int[] { num };
                globalWorkItemSizes = new int[] { num };

                //网格置0，数组重置
                for ( int i = 0;i < num; i++ ) clUnit.grid[i] = 0;
                
                var grid1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.USE_HOST_PTR, clUnit.grid.Length * sizeof(float), clUnit.grid.ToFloatPtr());
                IntPtr ptGrid1 = clUnit.oclCQ.EnqueueMapBuffer(grid1, true, MapFlags.READ_WRITE, 0, clUnit.grid.Length * sizeof(float), 0, null);

                int[]mutex = new int[] {0 };
                var mutex1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, mutex.Length * sizeof(int), mutex.ToIntPtr());
             
                int k = 0;
                //RBF gridInterpolate
                clUnit.Kernels["gridInterpolate"].SetArg(k++, a1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, px1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, py1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, pz1);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, startid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, num);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, grid1);
                //clUnit.Kernels["gridInterpolate"].SetArg(k++, mutex1);
                //clUnit.Kernels["gridInterpolate"].SetArg(k++, minx);
                //clUnit.Kernels["gridInterpolate"].SetArg(k++, miny);
                //clUnit.Kernels["gridInterpolate"].SetArg(k++, minz);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, scale);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, xstep);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, ystep);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, zstep);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, xGrid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, yGrid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, zGrid);
                clUnit.Kernels["gridInterpolate"].SetArg(k++, np);

                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["gridInterpolate"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                //clUnit.oclCQ.EnqueueBarrier();
                
                clUnit.oclCQ.Finish();

                clUnit.oclCQ.EnqueueReadBuffer(grid1, true, 0, clUnit.grid.Length * sizeof(float), clUnit.grid.ToFloatPtr());

                //拷贝到grid3d数组对应位置
                CopyGridFromThread(startid,num,clUnit.grid);

                clUnit.oclCQ.EnqueueUnmapMemObject(grid1, clUnit.grid.ToFloatPtr());
                grid1.Dispose();

                //下一任务
                task = GetFromTaskPools();
                percentage = (1.0-(double)taskPools.Count / (double)taskCount) * 100;                
                
            }//while (!clUnit.Stop && !threadStoped)

            //解除内存映射
            clUnit.oclCQ.EnqueueUnmapMemObject(px1, px.ToFloatPtr());
            clUnit.oclCQ.EnqueueUnmapMemObject(py1, py.ToFloatPtr());
            clUnit.oclCQ.EnqueueUnmapMemObject(pz1, pz.ToFloatPtr());
            clUnit.oclCQ.EnqueueUnmapMemObject(a1, aFactor.ToDoublePtr());

            a1.Dispose();
            px1.Dispose();
            py1.Dispose();
            pz1.Dispose();

            aFactor = null;

            clUnit.ReleaseSiginal();//线程结束信号
        }
        
        string InversionSource =
            @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable   
            #pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
            #define LOCK(a) atom_cmpxchg(a, 0, 1)
            #define UNLOCK(a) atom_xchg(a, 0)
        
         __kernel void Iterate(int iterate,         //迭代次数
                                __global double* G,  //输入G
                                __global double* BT, //输入BT
                                __global double* T,  //输入T
                                __global double* Aw,  //临时变量
                                __global double* Wd,  //临时变量
                                __global double* We,  //临时变量
                                __global double* Q,  //临时变量
                                __global double* q,  //临时变量
                                __global double* mw,  //临时变量
                                __global double* Wm,  //临时变量
                                __global double* f0,  //临时变量
                                __global double* d0,  //临时变量
                                __global double* mid,  //临时变量
                                
                                int lines,           //测线数
                                int points,          //测点数
                                int layers,          //分层数   
                                int maxIteration,    //最大迭代次数
                                int BIG,
                                int SMALL )
        {            
            int m_iIterations = get_global_id(0); //线程号 = 迭代次数
            if( m_iIterations >= maxIteration ) return;

            barrier(CLK_Local_MEM_FENCE);//设置线程阻塞，等待当前迭代完成
			
            for (int i = 0; i < SMALL; i++)
				for (int j = 0; j < BIG; j++)
					Aw[i * BIG + j] = G[i * BIG + j] * Wd[i] * (1.0 / Wm[j]) * (1.0 / We[j]);
			
			for (int i = 0; i < BIG; i++)
				for (int j = 0; j < SMALL; j++)
					Aw_T[i * SMALL + j] = Aw[j * BIG + i];

			mat* Aw_Tmp = new mat(Aw_T, SMALL, BIG);
			mat* Q_Tmp = new mat(BIG, BIG, fill::zeros);
			*Q_Tmp = Aw_Tmp->t() * *Aw_Tmp;

			for (int i = 0; i < BIG; i++)
				for (int j = 0; j < BIG; j++) 
				{
					Q[i * BIG + j] = (*Q_Tmp)(i, j) * (1.0 / We[i]) * (1.0 / We[i]);
					if (i == j)Q[i * BIG + j] += miu * e * e;
				}

			delete Aw_Tmp;
			delete Q_Tmp;

			for (int i = 0; i < BIG; i++) 
			{
				for (int j = 0; j < SMALL; j++)mid[i] += Aw_T[i * SMALL + j] * dw[j];					
				q[i] = (1.0 / We[i]) * (1.0 / We[i]) * mid[i];
			}
			memset(mid, 0, sizeof(double) * BIG);

			for (int i = 0; i < BIG; i++)
				f0[i] = f1[i];

			Q_Tmp = new mat(Q, BIG, BIG);
			mat* mwTmp = new mat(mw, BIG, 1);
			mat* f1Tmp = new mat(BIG, 1, fill::zeros);
			*f1Tmp = Q_Tmp->t() * *mwTmp;

			for (int i = 0; i < BIG; i++)f1[i] = (*f1Tmp)(i, 0) - q[i];
				
			delete Q_Tmp, mwTmp, f1Tmp;

			double molecule = 0, denominator = 0;

			for (int i = 0; i < BIG; i++)
			{
				molecule += f1[i] * f1[i];
				denominator += f0[i] * f0[i];
			}
			m_Beta = molecule / denominator;

			for (int i = 0; i < BIG; i++)d0[i] = -f1[i] + m_Beta * d0[i];
				
			molecule = 0, denominator = 0;
			
			for (int i = 0; i < BIG; i++) 
			{
				molecule += d0[i] * f1[i];
				for (int j = 0; j < BIG; j++)
					mid[i] += d0[j] * Q[j * BIG + j];
				denominator += mid[i] * d0[i];
			}
			t0 = molecule / denominator;
			memset(mid, 0, sizeof(double) * BIG);
			molecule = 0, denominator = 0;

			for (int i = 0; i < BIG; i++) {
				mw[i] -= t0 * d0[i];
				m0[i] = mw[i] * (1.0 / Wm[i]) * (1.0 / We[i]);
			}

			for (int i = 0; i < BIG; i++)
			{
				if (m0[i] > m_Max)
					m0[i] = m_Max;
				else if (m0[i] < m_Min)
					m0[i] = m_Min;
			}

			for (int i = 0; i < BIG; i++)
			{
				We[i] = 1.0 / sqrt(m0[i] * m0[i] + e * e);
				mw[i] = We[i] * Wm[i] * m0[i];
			}
        }
        __kernel void Matrix_Mult(  const int Ndim,
                                    const int Mdim,
                                    const int Pdim,
                                    __global const double * A, 
                                    __global const double * B, 
                                    __global double * C 
                                  )
        {
            int i = get_global_id(0);
            int j = get_global_id(1);

            int k;
            double tmp;

            if ((i < Ndim) && (j < Mdim)) 
            {
                tmp = 0.0;
                for (k = 0; k < Pdim; k++) tmp += A[i*Pdim + k] * B[k*Mdim + j];
                C[i*Mdim + j] = tmp;
            }
        }
       ";

        #region RBF Interpolation Kernel Source
        private string InterpolateSource =
            @"
#pragma OPENCL EXTENSION cl_khr_fp64 : enable   
#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
#define LOCK(a) atom_cmpxchg(a, 0, 1)
#define UNLOCK(a) atom_xchg(a, 0)
        __kernel void distCalculate(__global double* A, __global float *px,__global float *py, __global float *pz, int np )
        {
            int irow = get_global_id(0);
            int icol = get_global_id(1);
            
            if( irow >= np || icol >= np ) return;
            
            int id1 = irow * np + icol;
            int id2 = icol * np + irow;
            
            const double MIN = 1;

            if( A[id2] > 0 ) A[id1] = A[id2];
            else 
               A[id1] = sqrt( ( px[irow] - px[icol] )*( px[irow] - px[icol] ) + 
                              ( py[irow] - py[icol] )*( py[irow] - py[icol] ) + 
                              ( pz[irow] - pz[icol] )*( pz[irow] - pz[icol] ) + MIN*MIN );
        }
        
        __kernel void MatrixInverse1(__global double* A, __global double* E, double bs,int currow, int row )
        {            
            int j = get_global_id( 0 ) + currow;
            if( j == currow )
            {
                A[0] = 1;
            }
            else if( j > currow && j < row  )
            {
                A[j-currow] /= bs;
            }
            else if( j >= row && j < row + row )
            {
                E[j-row] /= bs;
            }
        }
        
        __kernel void MatrixInverse2(__global double* A, 
                                     __global double* E, 
                                     __global double* lineA, 
                                     __global double* lineE, 
                                     __global double* bss,
                                     int curcol,int start,int num,int row )
        {    
            int j = get_global_id( 0 ) + curcol;
            
            int i;
            double bs;
            if( j >= curcol && j < row ) 
            {
                for( i = start; i < start + num; i++ )
                {
                    if(i != curcol )
                    {   
                        bs = bss[i-start];
                        A[ (i-start)*row + j] += -bs * lineA[j];
                    }
                }
            }
            else if( j >= row && j < row + row ) 
            {
                for( i = start; i < start + num; i++ )
                {
                    if(i != curcol )
                    {   
                        bs = bss[i-start];
                        E[ (i-start)*row + j-row] += -bs * lineE[j-row];     
                    }
                }
            }           
        }       

        __kernel void weightCalculate( __global float* A, __global float *pv, __global double *a, int np )
        {
            int i,j,id;
            i = get_global_id(0);
            if( i >= np ) return;            
            a[i] = 0;
            for ( j = 0; j < np; j++ )
            {
                id = i * np + j;
                a[i] += A[id] * pv[j];
            }                           
        }
        double GetScaledValue(double x,double minx,double maxlen,double scale)
        {
            return (x - minx) / maxlen * scale;
        }
        
        __kernel void gridInterpolate(  __global double* a,
                                        __global float * px, 
                                        __global float * py, 
                                        __global float * pz,
                                        int startid,
                                        int idnum,
                                        __global float * grid,
                                        double scale,
                                        double xstep,
                                        double ystep,
                                        double zstep,
                                        int nx,int ny,int nz,int np )
                                        
        {
            int id = get_global_id(0) + startid;
            if( id >= nx*ny*nz ) return;
            
            int j,ix,iy,iz,xy,md;
            double x,y,z,val,sum;

            xy = nx * ny; 
            iz = (int)(id/xy);
            md = id - iz * xy;
            iy = md / nx;
            ix = md % nx;

            sum = 0.0;
            j = 0.0;
            while( j < np )
            {   
                x =  ix * xstep * scale;
                y =  iy * ystep * scale;
                z =  iz * zstep * scale; 
                val = a[j] * sqrt( (x - px[j]) * (x - px[j]) +
                                   (y - py[j]) * (y - py[j]) +
                                   (z - pz[j]) * (z - pz[j]) );
                sum += val;    
                j++;
            }
            grid[ id - startid] = sum;
        } 
       
        "; //end of InterpolationSource        

        #endregion InterpolationSource


        #region 反距离加权插值Soruce
        //反距离加权插值
        private static string IDWSoruce = @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable
            __kernel void gpuThread(__global float * px,__global float * py,__global float * pz,__global float * pv, int np,
                                         __global float * dists, 
                                          int nx, int ny, int nz,
                                          double minx, double miny, double minz,
                                          double maxx, double maxy, double maxz,
                                         __global float* grid3d )
                            {
                                    int i,iy,ix,id;
                                    double x,y,z,value,dist,distinv;    
                                    const int iz = get_global_id(0); //cur iz position
    
                                    double sum = 0;
    
                                    double err = 1.0E-20;
                                    double xstep = ( maxx - minx ) / (nx - 1);
                                    double ystep = ( maxy - miny ) / (ny - 1);
                                    double zstep = ( maxz - minz ) / (nz - 1); 
                                    z = minz + (iz * zstep);

                                    for(iy=0;iy<ny;iy++)
                                    {  
                                        for(ix=0;ix<nx;ix++)
                                        {                                            
                                            x = minx + (ix * xstep);
                                            y = miny + (iy * ystep);
                                            value = 0;
                                            sum = 0;
                                            for( i = 0;i < np; i++ )
                                            {
                                                dist  = (float)sqrt(  (x - px[i])*(x - px[i]) + 
                                                                      (y - py[i])*(y - py[i]) + 
                                                                      (z - pz[i])*(z - pz[i]) );
                                               
                                                if( dist >= -err && dist <= err )
                                                {                                                    
                                                    sum = 0;
                                                    value = pv[i];
                                                    break;
                                                }
                                                else 
                                                {   
                                                    distinv = 1.0 / dist;
                                                    sum = sum + distinv;
                                                    dists[ iz * np + i ] = distinv;
                                                }
                                            }//for( i = 0;i < np; i++ )
                        
                                            ////////////////////////////////////////////////////
                                            if( sum > 0.0 )
                                            {    
                                                value = 0;
                                                for( i = 0;i < np; i++ )                                                
                                                {   
                                                    value = value + pv[i] * dists[iz * np + i] / sum ;
                                                }
                                            }

                                            id = ix + iy*nx + iz * nx * ny;
                                            grid3d[id] = (float)value;

                                        }//for(ix=0;ix<nx;ix++)
                                    }//for(iy=0;iy<ny;iy++)
                            } ";
        #endregion
        #region 反距离加权插值Soruce
        //反距离加权插值
        private static string IDWSoruce1 = @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable
            __kernel void gpuThread(__global float * px,__global float * py,__global float * pz,__global float * pv, int np,
                                         __global float * dists, 
                                          int nx, int ny, int nz,
                                          double minx, double miny, double minz,
                                          double maxx, double maxy, double maxz,
                                         __global float* grid3d )
                            {
                                    int i,iy,ix,id;
                                    double x,y,z,value,dist,distinv;    
                                    const int iz = get_global_id(0); //cur iz position
    
                                    double sum = 0;
    
                                    double err = 1.0E-20;
                                    double xstep = ( maxx - minx ) / (nx - 1);
                                    double ystep = ( maxy - miny ) / (ny - 1);
                                    double zstep = ( maxz - minz ) / (nz - 1); 
                                    z = minz + (iz * zstep);

                                    for(iy=0;iy<ny;iy++)
                                    {  
                                        for(ix=0;ix<nx;ix++)
                                        {                                            
                                            x = minx + (ix * xstep);
                                            y = miny + (iy * ystep);
                                            value = 0;
                                            sum = 0;
                                            for( i = 0;i < np; i++ )
                                            {
                                                dist  = (float)sqrt(  (x - px[i])*(x - px[i]) + 
                                                                      (y - py[i])*(y - py[i]) + 
                                                                      (z - pz[i])*(z - pz[i]) );
                                               
                                                if( dist >= -err && dist <= err )
                                                {                                                    
                                                    sum = 0;
                                                    value = pv[i];
                                                    break;
                                                }
                                                else 
                                                {   
                                                    distinv = 1.0 / dist;
                                                    sum = sum + distinv;
                                                    dists[ iz * np + i ] = distinv;
                                                }
                                            }//for( i = 0;i < np; i++ )
                        
                                            ////////////////////////////////////////////////////
                                            if( sum > 0.0 )
                                            {    
                                                value = 0;
                                                for( i = 0;i < np; i++ )                                                
                                                {   
                                                    value = value + pv[i] * dists[iz * np + i] / sum ;
                                                }
                                            }

                                            id = ix + iy*nx + iz * nx * ny;
                                            grid3d[id] = (float)value;

                                        }//for(ix=0;ix<nx;ix++)
                                    }//for(iy=0;iy<ny;iy++)
                            } ";
        #endregion
        //-------------------------------------------------------------------------
        //--------------串行算法-----------------------------------------------------------
        //--------------------------------------------------------------------------
        //--------矩阵求逆法插值--------------------
        int taskNum = 0;
        int threadNum = 0;
        AutoResetEvent taskDone = new AutoResetEvent(false);
        protected void InterpolationThread(Object para)
        {
            TaskPoolIndices task = (TaskPoolIndices)para;
            long start = task.start;//任务开始编号
            int num = task.num;     //任务数
            int memindex = task.memindex;//内存块起始位置
            double x, y, z, val;
            Int32XYZ xyz;            
            int all = xGrid * yGrid * zGrid;//全部任务数

            for (long id = start; id < start + num; id++)
            {
                if (id >= all) break;
                xyz = GetIndices((ulong)id, xGrid, yGrid, zGrid);
                x = minx + xstep * xyz.x;
                y = miny + ystep * xyz.y;
                z = minz + zstep * xyz.z;
                val = GetInterpolatedValueByThread(x, y, z);
                if (double.IsNaN(val)) grid3d[id - start + memindex] = CSurferGrid.blankValuefloat;
                else grid3d[id - start + memindex] = (float)val;
                Interlocked.Decrement(ref taskNum);
                int nfinished = all - taskNum;//已完成任务数
                if (id - start > 0 && (id - start) % 100 == 0)
                {   
                    timeLeft = taskNum * timePerStep;//剩余时间
                    percentage = 100 * (double)nfinished / all;
                }
            }

            Interlocked.Decrement(ref threadNum);
            if (threadNum == 0)
            {
                percentage = 100;
                taskDone.Set();
            }

        }
        public override float[] GetInterpolatedValueCPU(int xn, int yn, int zn)
        {
            try
            {
                grid3d = null;
                int all = xn * yn * zn;
                C3DGridDataStream gridStream = null;
                SetGrid(xn, yn, zn);
                int n = points.Count;
                int ix, iy, iz;
                double r, sum = 0, sum1 = 0;
                double x, y, z, xstep, ystep, zstep;
                
                aFactor = new double[n];
                double[,] QQ = new double[n, n];
                progressTitle = "计算距离函数...";
                startTime = DateTime.Now;

                double step = 0;
                double timeDistribute1 = 5; //计算 | Pi - Pj | 矩阵Q
                double timeDistribute2 = 20; //matrix
                double timeDistribute3 = 5; //"计算伴随矩阵...";
                double timeDistribute4 = 70; //网格插值
                percentage = 0;

                double sec = 0;
                DateTime t1, t2;
                t1 = t2 = startTime;

                //计算|Pi-Pj|矩阵Q，这里可能需要检查重点
                // 5%
                Vector32 p1, p2;
                step = timeDistribute1 / n;
                for (int i = 0; i < n; i++)
                {
                    p1 = points[i];
                    for (int j = 0; j < n; j++)
                    {
                        p2 = points[j];
                        r = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                       (p1.y - p2.y) * (p1.y - p2.y) +
                                       (p1.z - p2.z) * (p1.z - p2.z) +
                                        MINE * MINE);                        
                        QQ[i, j] = r;
                    }
                    if (i == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = n * sec;
                    }
                    timeSlip += (i + 1) * sec;
                    percentage = (i + 1) * step;
                }

                progressTitle = "计算逆矩阵...";//20%                              
                QQ = Inverse(QQ, timeDistribute2); 

                progressTitle = "计算伴随矩阵..."; //5%
                t1 = DateTime.Now;
                double curpos = percentage;
                //求伴随矩阵a[j]
                for (int i = 0; i < n; i++)
                {
                    sum = 0.0;
                    for (int j = 0; j < n; j++)
                    {
                        //----------------------------------
                        // sum += qm[i, j] * points[j].v;
                        sum += QQ[i, j] * points[j].v;
                    }
                    aFactor[i] = sum;
                    if (i == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = n * sec;
                    }
                    timeSlip += (i + 1) * sec;
                    percentage = curpos + (i + 1) * timeDistribute3 / n;
                }
                QQ = null;

                progressTitle = "网格插值..."; //70%
                xstep = (maxx - minx) / (xn - 1);
                ystep = (maxy - miny) / (yn - 1);
                zstep = (maxz - minz) / (zn - 1);
                //网格插值
                taskDone = new AutoResetEvent(false);
                taskNum = (int)all;
                int batch = Environment.ProcessorCount * 2;//CPU核数-线程数
                batch = 1;
                if ( !BigGridData )
                {
                    grid3d = new float[all];
                    DividedNum = 1;
                }
                else //分块插值
                {
                    gridStream = new C3DGridDataStream();
                    gridStream.Create(gridDataFile, xn, yn, zn, minx, miny, minz, minv, maxx, maxy, maxz, maxv);
                }
                
                //估计时间加速比                 
                //试运行1个任务，得出预估时间
                progressTitle = "正在估算计算时间...";
                taskNum = 1;
                TaskPoolIndices task = new TaskPoolIndices(0, 1000);
                grid3d = new float[1000];

                //试运行1个任务，得出预估时间
                t1 = DateTime.Now;
                InterpolationThread(task);
                t2 = DateTime.Now;
                timePerStep = (t2 - t1).TotalMilliseconds / 10000.0;
                timeLeft = all * timePerStep;
                timePerStep = timePerStep / batch;                
                
                int start = 0;                
                int sectionNum = all / DividedNum + 1;//分块网格数据大小
                int num = sectionNum / batch;//单次任务数
                if (sectionNum % batch != 0) num++;
                taskNum = all;

                progressTitle = "正在进行网格插值...";
                int written = 0;
                for (int k = 0; k < DividedNum; k++)//grid分块数
                {
                    taskDone.Reset();

                    threadNum = batch;
                    if (sectionNum % batch != 0) threadNum++;

                    grid3d = new float[sectionNum + 1];
                    ThreadPool.SetMaxThreads(batch, batch);

                    for (int i = 0; i < batch; i++)
                    {
                        task = new TaskPoolIndices(start, num, i * num);
                        start += num;
                        ThreadPool.QueueUserWorkItem(InterpolationThread, task);
                    }

                    if (sectionNum % batch != 0)//最后一次任务 
                    {
                        task = new TaskPoolIndices(start, sectionNum - batch * num, batch * num);
                        start += (sectionNum - batch * num);
                        ThreadPool.QueueUserWorkItem(InterpolationThread, task);
                    }

                    taskDone.WaitOne();//等待所有线程结束

                    if (BigGridData)
                    {
                        if (written + sectionNum < all)
                        {
                            gridStream.WriteData(grid3d, 0, sectionNum);
                            written += sectionNum;
                        }
                        else
                        {
                            gridStream.WriteData(grid3d, 0, all - written);
                            written = all;
                        }
                        grid3d = null;
                    }
                }//for (int k = 0; k < DividedNum; k++)

                //实际计算时间
                timeSlip = (DateTime.Now - t1).TotalSeconds;
                if (BigGridData) return new float[10];
                else return grid3d;
            }
            catch (Exception e)
            {
                errMsg = e.Message + "请尝试减少插值点数目.";
                return null;
            }
        }
        /// <summary>
        /// 各向异性矿体查找
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <param name="layerID">矿体编号</param>
        /// <returns></returns>
        public float[] GetInterpolatedValueCPUAnisotropy(int xn, int yn, int zn,int layerID)
        {
            try
            {
                grid3d = null;
                int all = xn * yn * zn;
                C3DGridDataStream gridStream = null;
                SetGrid(xn, yn, zn);
                int n = points.Count;
                int ix, iy, iz;
                double r, sum = 0, sum1 = 0;
                double x, y, z, xstep, ystep, zstep;

                aFactor = new double[n];
                double[,] QQ = new double[n, n];
                progressTitle = "计算距离函数...";
                startTime = DateTime.Now;

                double step = 0;
                double timeDistribute1 = 5; //计算 | Pi - Pj | 矩阵Q
                double timeDistribute2 = 20; //matrix
                double timeDistribute3 = 5; //"计算伴随矩阵...";
                double timeDistribute4 = 70; //网格插值
                percentage = 0;

                double sec = 0;
                DateTime t1, t2;
                t1 = t2 = startTime;

                //各向异性距离计算

                //计算|Pi-Pj|矩阵Q，这里可能需要检查重点
                // 5%
                Vector32 p1, p2;
                step = timeDistribute1 / n;
                for (int i = 0; i < n; i++)
                {
                    p1 = points[i];
                    for (int j = 0; j < n; j++)
                    {
                        p2 = points[j];
                        r = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                       (p1.y - p2.y) * (p1.y - p2.y) +
                                       (p1.z - p2.z) * (p1.z - p2.z) +
                                        MINE * MINE);
                        QQ[i, j] = r;
                    }
                    if (i == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = n * sec;
                    }
                    timeSlip += (i + 1) * sec;
                    percentage = (i + 1) * step;
                }

                progressTitle = "计算逆矩阵...";//20%                              
                QQ = Inverse(QQ, timeDistribute2);

                progressTitle = "计算伴随矩阵..."; //5%
                t1 = DateTime.Now;
                double curpos = percentage;
                //求伴随矩阵a[j]
                for (int i = 0; i < n; i++)
                {
                    sum = 0.0;
                    for (int j = 0; j < n; j++)
                    {
                        //----------------------------------
                        // sum += qm[i, j] * points[j].v;
                        sum += QQ[i, j] * points[j].v;
                    }
                    aFactor[i] = sum;
                    if (i == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = n * sec;
                    }
                    timeSlip += (i + 1) * sec;
                    percentage = curpos + (i + 1) * timeDistribute3 / n;
                }
                QQ = null;

                progressTitle = "网格插值..."; //70%
                xstep = (maxx - minx) / (xn - 1);
                ystep = (maxy - miny) / (yn - 1);
                zstep = (maxz - minz) / (zn - 1);
                //网格插值
                taskDone = new AutoResetEvent(false);
                taskNum = (int)all;
                int batch = Environment.ProcessorCount * 2;//CPU核数-线程数
                batch = 1;
                if (!BigGridData)
                {
                    grid3d = new float[all];
                    DividedNum = 1;
                }
                else //分块插值
                {
                    gridStream = new C3DGridDataStream();
                    gridStream.Create(gridDataFile, xn, yn, zn, minx, miny, minz, minv, maxx, maxy, maxz, maxv);
                }

                //估计时间加速比                 
                //试运行1个任务，得出预估时间
                progressTitle = "正在估算计算时间...";
                taskNum = 1;
                TaskPoolIndices task = new TaskPoolIndices(0, 1000);
                grid3d = new float[1000];

                //试运行1个任务，得出预估时间
                t1 = DateTime.Now;
                InterpolationThread(task);
                t2 = DateTime.Now;
                timePerStep = (t2 - t1).TotalMilliseconds / 10000.0;
                timeLeft = all * timePerStep;
                timePerStep = timePerStep / batch;

                int start = 0;
                int sectionNum = all / DividedNum + 1;//分块网格数据大小
                int num = sectionNum / batch;//单次任务数
                if (sectionNum % batch != 0) num++;
                taskNum = all;

                progressTitle = "正在进行网格插值...";
                int written = 0;
                for (int k = 0; k < DividedNum; k++)//grid分块数
                {
                    taskDone.Reset();

                    threadNum = batch;
                    if (sectionNum % batch != 0) threadNum++;

                    grid3d = new float[sectionNum + 1];
                    ThreadPool.SetMaxThreads(batch, batch);

                    for (int i = 0; i < batch; i++)
                    {
                        task = new TaskPoolIndices(start, num, i * num);
                        start += num;
                        ThreadPool.QueueUserWorkItem(InterpolationThread, task);
                    }

                    if (sectionNum % batch != 0)//最后一次任务 
                    {
                        task = new TaskPoolIndices(start, sectionNum - batch * num, batch * num);
                        start += (sectionNum - batch * num);
                        ThreadPool.QueueUserWorkItem(InterpolationThread, task);
                    }

                    taskDone.WaitOne();//等待所有线程结束

                    if (BigGridData)
                    {
                        if (written + sectionNum < all)
                        {
                            gridStream.WriteData(grid3d, 0, sectionNum);
                            written += sectionNum;
                        }
                        else
                        {
                            gridStream.WriteData(grid3d, 0, all - written);
                            written = all;
                        }
                        grid3d = null;
                    }
                }//for (int k = 0; k < DividedNum; k++)

                //实际计算时间
                timeSlip = (DateTime.Now - t1).TotalSeconds;
                if (BigGridData) return new float[10];
                else return grid3d;
            }
            catch (Exception e)
            {
                errMsg = e.Message + "请尝试减少插值点数目.";
                return null;
            }
        }
        /// <summary>
        /// CPU 版插值，采样数学库函数求逆矩阵
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="yn"></param>
        /// <param name="zn"></param>
        /// <returns></returns>
        public float[] GetInterpolatedValueCPU_MATH(int xn, int yn, int zn)
        {
            try
            {
                SetGrid(xn, yn, zn);
                float[] result = new float[xn * yn * zn]; //输出结果：插值结果数组，三维网格
                if (result == null)
                {
                    errMsg = "no enough memory to create 3D grids";
                    return null;
                }

                int n = points.Count;

                int i, j, ix, iy, iz;
                double m, r, sum = 0, sum1 = 0;
                double x, y, z, xstep, ystep, zstep;

                double[] a = new double[n];
                if (a == null) //内存不够
                {
                    errMsg = "no enough memory to allocate";
                    return null;
                }

                ///*------------------------------------------------
                DenseMatrix QMatix = new DenseMatrix(n, n);
                if (QMatix == null)
                {
                    errMsg = "no enough memory to create matrix";
                    return null;
                }
                //----------------------------------------------*/
                progressTitle = "计算距离函数...";
                startTime = DateTime.Now;

                double step = 0;
                double timeDistribute1 = 5; //计算 | Pi - Pj | 矩阵Q
                double timeDistribute2 = 20; //matrix
                double timeDistribute3 = 5; //"计算伴随矩阵...";
                double timeDistribute4 = 70; //网格插值
                percentage = 0;

                double sec = 0;
                DateTime t1, t2;
                t1 = t2 = startTime;

                //计算|Pi-Pj|矩阵Q，这里可能需要检查重点
                // 5%
                Vector32 p1, p2;
                step = timeDistribute1 / n;
                for (i = 0; i < n; i++)
                {
                    p1 = points[i];
                    for (j = 0; j < n; j++)
                    {
                        p2 = points[j];
                        r = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                       (p1.y - p2.y) * (p1.y - p2.y) +
                                       (p1.z - p2.z) * (p1.z - p2.z) +
                                        MINE * MINE);
                        //-------------------------------
                        QMatix[i, j] = r;
                        //-------------------------------                        
                    }
                    if (i == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = n * sec;
                    }
                    timeSlip += (i + 1) * sec;
                    percentage = (i + 1) * step;
                }

                progressTitle = "计算逆矩阵...";//20%

                //------------------------------------
                Matrix<double> qm = QMatix.Inverse();

                progressTitle = "计算伴随矩阵..."; //5%
                t1 = DateTime.Now;
                double curpos = percentage;
                //求伴随矩阵a[j]
                for (i = 0; i < n; i++)
                {
                    sum = 0.0;
                    for (j = 0; j < n; j++)
                    {
                        //----------------------------------
                        sum += qm[i, j] * points[j].v;
                    }
                    a[i] = sum;
                    if (i == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = n * sec;
                    }
                    timeSlip += (i + 1) * sec;
                    percentage = curpos + (i + 1) * timeDistribute3 / n;
                }
                ///*-----------------------
                qm.Clear();
                QMatix.Clear();
                //--------------------------*/

                progressTitle = "网格插值..."; //70%
                t1 = DateTime.Now;

                xstep = (maxx - minx) / (xn - 1);
                ystep = (maxy - miny) / (yn - 1);
                zstep = (maxz - minz) / (zn - 1);
                int id = 0;
                curpos = percentage;
                for (iz = 0; iz < zn; iz++)
                {
                    z = minz + zstep * iz;
                    for (iy = 0; iy < yn; iy++)
                    {
                        y = miny + ystep * iy;
                        for (ix = 0; ix < xn; ix++)
                        {
                            x = minx + xstep * ix;
                            sum1 = 0.0;
                            //************************************************************************	
                            for (j = 0; j < n; j++)
                            {
                                r = Math.Sqrt((x - points[j].x) * (x - points[j].x) +
                                               (y - points[j].y) * (y - points[j].y) +
                                               (z - points[j].z) * (z - points[j].z) +
                                               MINE * MINE);
                                sum1 = sum1 + r * a[j];
                            }
                            //**********************************************************************	

                            result[id++] = (float)sum1;

                        }//for (ix = 0; ix < xn; ix++)
                    }//for (iy = 0; iy < yn; iy++)

                    if (iz == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                    }
                    timeLeft = (zn - iz) * sec;
                    timeSlip += (iz + 1) * sec;
                    percentage = curpos + (iz + 1) * timeDistribute4 / zn;

                }//for (iz = 0; iz < zn; iz++)

                percentage = 100;

                a = null;
                return result;
            }
            catch (Exception e)
            {
                errMsg = e.Message + "请尝试减少插值点数目.";
                return null;
            }
        }
        
        bool brinv(double[] Q)
        {
            int n = points.Count;
            int i, j, k, l, u, v;
            double d, p;

            int[] iss = new int[n];
            int[] jss = new int[n];

            if (iss == null || jss == null) //分配内存失败;
                return false;

            for (k = 0; k <= n - 1; k++)
            {
                d = 0.0;
                for (i = k; i <= n - 1; i++)
                {
                    for (j = k; j <= n - 1; j++)
                    {
                        p = Math.Abs(Q[i * n + j]);
                        if (p > d)
                        {
                            d = p;
                            iss[k] = i;
                            jss[k] = j;
                        }
                    }
                }
                ///////////
                if (d + 1.0 == 1.0)
                {
                    //     iss = jss = null;
                    //     return false;
                    int dd = 0;
                }

                ///////////////
                if (iss[k] != k)
                {
                    for (j = 0; j <= n - 1; j++)
                    {
                        u = k * n + j;
                        v = iss[k] * n + j;
                        p = Q[u];
                        Q[u] = Q[v];
                        Q[v] = p;
                    }
                }

                if (jss[k] != k)
                {
                    for (i = 0; i <= n - 1; i++)
                    {
                        u = i * n + k;
                        v = i * n + jss[k];
                        p = Q[u];
                        Q[u] = Q[v];
                        Q[v] = p;
                    }
                }

                l = k * n + k;
                Q[l] = 1.0 / Q[l];

                for (j = 0; j <= n - 1; j++)
                    if (j != k)
                    {
                        u = k * n + j;
                        Q[u] = Q[u] * Q[l];
                    }

                /////////////////
                for (i = 0; i <= n - 1; i++)
                {
                    if (i != k)
                    {
                        for (j = 0; j <= n - 1; j++)
                        {
                            if (j != k)
                            {
                                u = i * n + j;
                                Q[u] = Q[u] - Q[i * n + k] * Q[k * n + j];
                            }
                        }
                    }
                }
                ////////////////
                for (i = 0; i <= n - 1; i++)
                {
                    if (i != k)
                    {
                        u = i * n + k;
                        Q[u] = -Q[u] * Q[l];
                    }
                }
            }// for (k = 0; k <= n - 1; k++)

            for (k = n - 1; k >= 0; k--)
            {
                if (jss[k] != k)
                {
                    for (j = 0; j <= n - 1; j++)
                    {
                        u = k * n + j;
                        v = jss[k] * n + j;
                        p = Q[u];
                        Q[u] = Q[v];
                        Q[v] = p;
                    }
                }
                if (iss[k] != k)
                {
                    for (i = 0; i <= n - 1; i++)
                    {
                        u = i * n + k;
                        v = i * n + iss[k];
                        p = Q[u];
                        Q[u] = Q[v];
                        Q[v] = p;
                    }
                }
            }// for (k = n - 1; k >= 0; k--)

            iss = jss = null;
            return true;
        }
        // 矩阵求逆，percent 为计算进度 %
        public double[,] Inverse(double[,] matrix, double percent = 10)
        {
            double minerr = 1.0E-30;//定义一个最小的数0
            int row = matrix.GetLength(0); //获取矩阵的行数
            int col = row;
            double[,] exMatrix = new double[row, col];
            if (exMatrix == null)
            {
                errMsg = "no enough memory.";
                return null;
            }

            /**************************************************
            行变换求逆矩阵首先要做的就是把A和I并列放在一起[A|I]
            也就是这个形式：
            a00,a01,a02,......,ann,1,0,0,0,....,0
            a10,a11,a12,......,a1n,0,1,0,0,....,0
            a20,a21,a22,......,a2n,0,0,1,0,....,0
            ...
            ...
            an0,an1,an2,......,ann,0,0,0,0,....,1
            下面三个for做的就是这件事
            ***************************************************/

            int i, j;
            double bs;

            progressTitle = "计算逆矩阵...";

            //矩阵求逆
            DateTime t1, t2;
            t1 = t2 = DateTime.Now;
            double sec = 0;

            double curpos = percentage;

            //构建扩展矩阵exMatrix
            for (i = 0; i < row; i++)
            {
                for (j = 0; j < row; j++)
                {
                    if (j == i) exMatrix[i, j] = 1;
                    else exMatrix[i, j] = 0;
                }
            }// for (i = 0; i < row; i++)

            t2 = DateTime.Now;
            sec = (t2 - t1).TotalSeconds;

            timeSlip += sec;
            timeLeft = row * sec;

            double avpercent = percent / row;
            t1 = DateTime.Now;
            curpos = percentage;
            //得到逆矩阵
            //这样把A经过行变化变成I，那么右面的I就会变为A逆            
            for (int k = 0; k < row; k++)
            {
                //1把对角元素变为1
                if (matrix[k, k] >= 1 - minerr &&
                     matrix[k, k] <= 1 + minerr) matrix[k, k] = 1;
                else
                {
                    bs = matrix[k, k];
                    matrix[k, k] = 1;

                    for (j = k + 1; j < row; j++)
                        matrix[k, j] = matrix[k, j] / bs;
                    for (j = 0; j < row; j++)
                        exMatrix[k, j] = (exMatrix[k, j] / bs);
                }

                //2全部其他行减第k行，使第k列只有[k,k]元素是1，其余是0。   
                //消去k列为0
                for (i = 0; i < row; i++) //row
                {
                    if (i != k)
                    {
                        bs = matrix[i, k];
                        //A矩阵
                        for (j = k; j < row; j++)
                            matrix[i, j] -= bs * matrix[k, j];
                        //I矩阵
                        for (j = 0; j < row; j++)
                            exMatrix[i, j] -= bs * exMatrix[k, j];
                    }
                }
                ///////////////////////////////////////////////////////
                if (k == 0)
                {
                    t2 = DateTime.Now;
                    sec = (t2 - t1).TotalSeconds;
                }
                timeLeft = (row - k) * sec;
                timeSlip += (k + 1) * sec;             
                percentage = curpos + avpercent * (k + 1);
            }
            //通过上面的变化
            /*******************************
            [A|I]变为[I|A逆]
            所以右面的那一半就是A逆
            下面就是把它拿粗来
            ******************************/
            curpos = percentage;
            for (i = 0; i < row; i++)
            {
                for (j = 0; j < row; j++)
                {
                    matrix[i, j] = exMatrix[i, j];
                }
            }

            exMatrix = null;

            return matrix;
        }

        public float[,] InverseOrg(float[,] Array)
        {
            int row = Array.GetLength(0);//获取矩阵的行数
            int col = Array.GetLength(1);//获取矩阵的列数
            if (row != col)
            {
                errMsg = "不是正定矩阵！";
                return null;
            }

            float[,] array = new float[row, 2 * row];
            for (int k = 0; k < row; k++)
            {
                for (int t = 0; t < 2 * row; t++)
                {     //疑为t<2n
                    array[k, t] = 0;
                }
            }
            //初始化左面的A
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    array[i, j] = Array[i, j];
                }
            }
            //初始化右面的I
            for (int k = 0; k < row; k++)
            {
                for (int t = row; t < 2 * row; t++)
                {
                    if ((t - k) == row)
                    {
                        array[k, t] = 1;
                    }
                    else
                    {
                        array[k, t] = 0;
                    }
                }
            }
            float bs;
            //得到逆矩阵
            //这样把A经过行变化变成I，那么右面的I就会变为A逆
            for (int k = 0; k < row; k++)
            {
                //把对角元素变为1
                if (array[k, k] != 1)
                {
                    bs = array[k, k];
                    array[k, k] = 1;
                    for (int p = k + 1; p < 2 * row; p++)
                    {
                        array[k, p] /= bs;
                    }
                }
                //全部其他行减第k行，使第k列只有[k,k]元素是1，其余是0。   
                for (int q = 0; q < row; q++)
                {
                    if (q != k)
                    {
                        bs = array[k, k];
                        bs = array[q, k];
                        for (int p = 0; p < 2 * row; p++)
                        {
                            array[q, p] -= bs * array[k, p];
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            //通过上面的变化
            /*******************************
            [A|I]变为[I|A逆]
            所以右面的那一半就是A逆
            下面就是把它拿粗来
            ******************************/
            float[,] NI = new float[row, row];
            for (int x = 0; x < row; x++)
            {
                for (int y = row; y < 2 * row; y++)
                {
                    NI[x, y - row] = array[x, y];
                }
            }
            return NI;
        }

    }
    public class RBFBoreholesInterpolation : RBFInterpolation
    {
        int taskNum = 0;
        int threadNum = 0;
        AutoResetEvent taskDone = new AutoResetEvent(false);

        [CategoryAttribute("矿层插值"), DisplayNameAttribute("钻孔列编号")]
        public int boreholeColumn { get; set; } = -1; //设置钻孔列编号
        [CategoryAttribute("矿层插值"), DisplayNameAttribute("矿体属性值")]
        public string mineralproperties //矿体属性值
        {
            get 
            {
                string text = "";
                for(int i=0;i<MineralValues.Count;i++)
                {
                    text += MineralValues[i].ToString();
                    if( i < MineralValues.Count - 1 )text += ",";                    
                }
                return text;
            }
            set 
            {
                string text = value;
                if( text.Length > 0 )
                {
                    MineralValues.Clear();
                    string[]ss = text.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for(int i=0;i<ss.Length;i++)
                    {
                        MineralValues.Add(int.Parse(ss[i]));
                    }
                }
            }
        }
        [CategoryAttribute("矿层插值"), DisplayNameAttribute("变程倍数")]
        public double DistantScale { get; set; } = 10; 
        public List<short>boreholeIndices = new List<short>(); //每个点对应的钻孔编号
        public List<int> MineralValues = new List<int>();//可能的矿层的值
        int[] nearestMineralPoints = null;  //矿点最近点对
        public override void Clear()
        {
            base.Clear();
            boreholeIndices.Clear();
            nearestMineralPoints = null;
        }
        public RBFBoreholesInterpolation()
        {
            method = InterpolationMethod.BoreholesMineralInterpolation;
        }
        public override void CopyFromWithOutPoints(InterpolatorBase ip)
        {
            base.CopyFromWithOutPoints(ip);
            if (ip.method == InterpolationMethod.BoreholesMineralInterpolation) 
            {
                RBFBoreholesInterpolation ip1 = (RBFBoreholesInterpolation)ip;
                boreholeColumn = ip1.boreholeColumn;
                MineralValues.AddRange(ip1.MineralValues);
                DistantScale = ip1.DistantScale;
            }
        }

        bool IsMineralPoint(Vector32 p) //是否含矿点
        {
            int val = (int)(p.V + 0.1);
            if (MineralValues.IndexOf(val) >= 0) return true;
            return false;
        }
        /// <summary>
        /// 计算矿点最近邻钻孔点对，-1无配对点
        /// </summary>
        void CalculateNearestMineralPoints()
        {
            nearestMineralPoints = null;
            nearestMineralPoints = new int[points.Count];
            
            for (int i = 0; i < nearestMineralPoints.Length; i++)
                nearestMineralPoints[i] = -1;

            Vector32 p1, p2;
            double dist, mindist;
            int id = -1;
            for(int i=0;i<points.Count;i++)
            {
                if ( nearestMineralPoints[i] >= 0 ) continue;
                p1 = points[i];
                if ( !IsMineralPoint(p1) ) continue;

                mindist = 1E30;id = -1;                
                for (int j = i+1; j < points.Count; j++)
                {
                    p2 = points[j];
                    if (!IsMineralPoint(p2)) continue; //非矿点
                    if (boreholeIndices[i] == boreholeIndices[j]) continue;//同钻孔
                    dist = p1.Distance(p2);
                    if (dist < mindist) { mindist = dist; id = j; }
                }
                if( id >=0 ) 
                { 
                    nearestMineralPoints[i] = id;
                    nearestMineralPoints[id] = i;
                }
            }
        }
        /// <summary>
        /// 串行求单点值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override double GetInterpolatedValue(double x, double y, double z)
        {
            int n = points.Count;
            int i, j;
            double r, sum = 0, sum1 = 0,factor = 10;           
            DenseMatrix QMatix = new DenseMatrix(n, n);
            //计算|Pi-Pj|矩阵Q，这里可能需要检查重点
            Vector32 p1, p2;
            for (i = 0; i < n; i++)
            {
                p1 = points[i];
                for (j = 0; j < n; j++)
                {
                    p2 = points[j];
                    r = Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) +
                                   (p1.y - p2.y) * (p1.y - p2.y) +
                                   (p1.z - p2.z) * (p1.z - p2.z) +
                                    MINE * MINE);
                    
                    //近邻矿对
                    if (nearestMineralPoints[i]>=0 &&
                         (nearestMineralPoints[i] == j || 
                          nearestMineralPoints[j] == i) )
                    {
                        r = r / DistantScale; //距离缩小
                    }
                    QMatix[i, j] = r;
                }
            }

            Matrix<double> qm = QMatix.Inverse();
            aFactor = new double[n];

            //求伴随矩阵a[j]
            for (i = 0; i < n; i++)
            {
                sum = 0.0;
                for (j = 0; j < n; j++)
                {
                    sum += qm[i, j] * points[j].v;
                }
                aFactor[i] = sum;
            }
            qm.Clear();
            QMatix.Clear();

            sum1 = 0.0;
            //************************************************************************	
            for (j = 0; j < n; j++)
            {
                r = Math.Sqrt((x - points[j].x) * (x - points[j].x) +
                               (y - points[j].y) * (y - points[j].y) +
                               (z - points[j].z) * (z - points[j].z) +
                               MINE * MINE);
                sum1 = sum1 + r * aFactor[j];
            }
            aFactor = null;
            return sum1;
        }
        public override float[] GetInterpolatedValue(int nx, int ny, int nz, Device[] devices)
        {
            SetGrid(nx, ny, nz);

            if (devices == null)
            {
                return GetInterpolatedValueCPU(nx, ny, nz);
                //return GetInterpolatedValueCPUAnisotropy(nx, ny, nz,1);
            }
            return null;
        }

        double [,] CreateDistanceMatrix()
        {
            progressTitle = "计算距离函数...";            
            DateTime t1 = DateTime.Now, t2;
            double r = 0,sec = 0;
            percentage = 0;
            Vector32 p1, p2;
            int n = points.Count;
            double step = 100.0 / n;
            double[,] QQ = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                p1 = points[i];
                QQ[i, i] = MINE;
                for (int j = i + 1; j < n; j++)
                {
                    p2 = points[j];
                    r = Math.Sqrt( (p1.x - p2.x) * (p1.x - p2.x) +
                                   (p1.y - p2.y) * (p1.y - p2.y) +
                                   (p1.z - p2.z) * (p1.z - p2.z) +
                                    MINE * MINE);
                    //近邻矿对
                    if (nearestMineralPoints[i] >= 0 &&
                         (nearestMineralPoints[i] == j ||
                          nearestMineralPoints[j] == i))
                    {
                        r = r / DistantScale; //距离缩小
                    }
                    QQ[i, j] = QQ[j,i] = r;
                }
                if (i == 0)
                {
                    t2 = DateTime.Now;
                    sec = (t2 - t1).TotalSeconds;
                    timeLeft = n * sec;
                }
                timeSlip += (i + 1) * sec;
                percentage = (i + 1) * step;
            }
            return QQ;
        }
        public double[,] Inverse1(double[,] matrix)
        {
            double minerr = 1.0E-30;//定义一个最小的数0
            int row = matrix.GetLength(0); //获取矩阵的行数
            int col = row;
            double[,] exMatrix = new double[row, col];
            if (exMatrix == null)
            {
                errMsg = "no enough memory.";
                return null;
            }
            
            int i, j;
            double bs;
            
            percentage = 0;
            progressTitle = "构建扩展矩阵...";
            //构建扩展矩阵exMatrix
            for (i = 0; i < row; i++)
            {
                for (j = 0; j < row; j++)
                {
                    if (j == i) exMatrix[i, j] = 1;
                    else exMatrix[i, j] = 0;
                }
            }// for (i = 0; i < row; i++)

            progressTitle = "计算逆矩阵...";
            DateTime t1, t2;
            t1 = t2 = DateTime.Now;
            double sec = 0;                    
            //得到逆矩阵
            //这样把A经过行变化变成I，那么右面的I就会变为A逆            
            for (int k = 0; k < row; k++)
            {
                //1把对角元素变为1
                if (matrix[k, k] >= 1 - minerr &&
                     matrix[k, k] <= 1 + minerr) matrix[k, k] = 1;
                else
                {
                    bs = matrix[k, k];
                    matrix[k, k] = 1;

                    for (j = k + 1; j < row; j++)
                        matrix[k, j] = matrix[k, j] / bs;
                    for (j = 0; j < row; j++)
                        exMatrix[k, j] = (exMatrix[k, j] / bs);
                }

                //2全部其他行减第k行，使第k列只有[k,k]元素是1，其余是0。   
                //消去k列为0
                for (i = 0; i < row; i++) //row
                {
                    if (i != k)
                    {
                        bs = matrix[i, k];
                        //A矩阵
                        for (j = k; j < row; j++)
                            matrix[i, j] -= bs * matrix[k, j];
                        //I矩阵
                        for (j = 0; j < row; j++)
                            exMatrix[i, j] -= bs * exMatrix[k, j];
                    }
                }
                ///////////////////////////////////////////////////////
                if (k == 0)
                {
                    t2 = DateTime.Now;
                    sec = (t2 - t1).TotalSeconds;
                }
                timeLeft = (row - k-1) * sec;
                timeSlip += (k + 1) * sec;
                percentage = 100 * (k + 1) / (double)row;
            }            
            for (i = 0; i < row; i++)
            {
                for (j = 0; j < row; j++)
                {
                    matrix[i, j] = exMatrix[i, j];
                }
            }
            exMatrix = null;
            return matrix;
        }
        public override float[] GetInterpolatedValueCPU(int xn, int yn, int zn)
        {
            try
            {
                grid3d = null;
                int all = xn * yn * zn;
                C3DGridDataStream gridStream = null;
                SetGrid(xn, yn, zn);
                int n = points.Count;
                double r, sum = 0, sum1 = 0;
                double x, y, z, xstep, ystep, zstep;

                percentage = 0;
                progressTitle = "计算矿点空间函数...";
                startTime = DateTime.Now;                
                CalculateNearestMineralPoints();//计算最近孔矿对
                aFactor = new double[n];

                progressTitle = "计算距离函数...";
                double[,] QQ = CreateDistanceMatrix();
                
                double sec = 0;
                DateTime t1, t2;
                t1 = t2 = startTime;

                percentage = 0;
                progressTitle = "计算逆矩阵...";//20%                              
                QQ = Inverse1(QQ);
                percentage = 0;
                progressTitle = "计算伴随矩阵..."; //5%
                t1 = DateTime.Now;                
                //求伴随矩阵a[j]
                for (int i = 0; i < n; i++)
                {
                    sum = 0.0;
                    for (int j = 0; j < n; j++)
                    {
                        //----------------------------------
                        // sum += qm[i, j] * points[j].v;
                        sum += QQ[i, j] * points[j].v;
                    }
                    aFactor[i] = sum;
                    if (i == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        timeLeft = n * sec;
                    }
                    timeSlip += (i + 1) * sec;
                    percentage = 100 * (double)(i + 1) / n;
                }
                QQ = null;

                percentage = 0;
                grid3d = new float[xn*yn*zn];
                progressTitle = "网格插值..."; //70%
                xstep = (maxx - minx) / (xn - 1);
                ystep = (maxy - miny) / (yn - 1);
                zstep = (maxz - minz) / (zn - 1);
                int count = 0;
                progressTitle = "正在进行网格插值...";
                //Parallel.For(0, zn, iz =>
                for (int iz = 0; iz < zn; iz++)
                {
                    for (int iy = 0; iy < yn; iy++)
                    {
                        for (int ix = 0; ix < xn; ix++)
                        {
                            sum1 = 0;
                            x = minx + xstep * ix;
                            y = miny + ystep * iy;
                            z = minz + zstep * iz;
                            for (int j = 0; j < pointCount; j++)
                            {
                                r = Math.Sqrt((x - points[j].x) * (x - points[j].x) +
                                               (y - points[j].y) * (y - points[j].y) +
                                               (z - points[j].z) * (z - points[j].z) +
                                               MINE * MINE);
                                sum1 = sum1 + r * aFactor[j];
                            }
                            grid3d[ix + iy * xn + iz * xn * yn] = (float)sum1;
                        }
                    }
                    Interlocked.Increment(ref count);
                    if (count == 1)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;                        
                    }
                    timeLeft = (zn - count) * sec;
                    percentage = 100 * (double)count / zn;
                }//);
                //实际计算时间
                timeSlip = (DateTime.Now - startTime).TotalSeconds;
                return grid3d;
            }
            catch (Exception e)
            {
                errMsg = e.Message + "请尝试减少插值点数目.";
                return null;
            }
        }
        
    }
}
