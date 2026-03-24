using CLInterpolation;
using DataCollection;
using DDDSharp.DataCollection;
using StratumInterpolation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DDDSharp.DataCollection.C3DGridDataStratum;
using static Khronos.Platform;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using MathNet.Numerics;
namespace OpenCLNet
{
    public class ProfileSampleData
    {
        public double minx;
        public double miny;
        public double maxx;
        public double maxy;
        public int xGrid;
        public int yGrid;
        public int profileID = -1;
        public int[,] stratumGrids = null;
        public ProfileSampleData(int profileid, double x1, double x2, double y1, double y2, int nx, int ny)
        {
            profileID = profileid;
            xGrid = nx;
            yGrid = ny;
            minx = x1;
            miny = y1;
            maxx = x2;
            maxy = y2;
        }
    }

    // 适配.NET Framework 4.8  
    /// <summary>
    /// 线段点位置判断核心类
    /// </summary>
    public class LinePointLocator
    {
        private List<CLine> _lines;

        public LinePointLocator(List<CLine> lines)
        {
            if (lines == null || lines.Count == 0)
                throw new ArgumentException("线段集合不能为空");
            _lines = lines;
        }

        /// <summary>
        /// 核心方法：判断点的位置
        /// </summary>
        /// <param name="p">目标点</param>
        /// <returns>结果：包含点所在线段（若有），或相邻的两条线段</returns>
        public PointLineLocationResult LocatePoint(Vector64 p)
        {
            var result = new PointLineLocationResult();

            // 第一步：检查点是否在某条线段上
            var onLine = _lines.FirstOrDefault(line => line.IsPointOnLine(p));
            if (onLine != null)
            {
                result.IsOnLine = true;
                result.LineOnPoint = onLine.Id;
                return result;
            }

            // 第二步：计算点到所有线段的带符号距离
            var distanceDict = new Dictionary<CLine, double>();
            foreach (var line in _lines)
            {
                distanceDict[line] = line.GetSignedDistanceToPoint(p);
            }

            // 第三步：按距离符号分组，找相邻线段
            // 假设线段是近似平行的，按距离绝对值排序
            var sortedLines = distanceDict.OrderBy(kv => Math.Abs(kv.Value)).ToList();

            if (sortedLines.Count >= 2)
            {
                // 取距离最近的两条线段作为“相邻线段”
                result.AdjacentLines = new List<int>
                {
                    sortedLines[0].Key.Id,
                    sortedLines[1].Key.Id
                };
            }
            else if (sortedLines.Count == 1)
            {
                // 只有一条线段，返回该线段（点在其一侧）
                result.AdjacentLines = new List<int> { sortedLines[0].Key.Id };
            }

            return result;
        }
    }

    /// <summary>
    /// 位置结果类
    /// </summary>
    public struct PointLineLocationResult
    {
        /// 是否在某条线段上
        public bool IsOnLine { get; set; }
        //点所在的线段ID（IsOnLine=true时有效）
        public int LineOnPoint { get; set; }

        //点相邻的两条线段id（IsOnLine=false时有效）
        public List<int> AdjacentLines;

        public override string ToString()
        {
            if (IsOnLine)
            {
                return $"点位于线段上：{LineOnPoint}";
            }
            else
            {
                if (AdjacentLines.Count == 0)
                    return "未找到相邻线段";
                else if (AdjacentLines.Count == 1)
                    return $"点位于线段 {AdjacentLines[0]} 附近";
                else
                    return $"点位于线段 {AdjacentLines[0]} 和 {AdjacentLines[1]} 之间";
            }
        }
    } 

    /// <summary>
    /// 地层建模-图像识别版本
    /// </summary>
    internal class StratumInterpolationFromImageRecognize : InterpolatorBase
    {
        public bool NearestOnly = false;
        public bool InvalidFilter = false;
        public List<PolygonSlicer> Slicers = new List<PolygonSlicer>();
        public string errMessage = "";       
        public C3DGridData grid3d = null;
        List<CLine>Lines = new List<CLine>();
        PointLineLocationResult[,] gridsLocation = null;
        List<ProfilePoint[,]> profilePoints = new List<ProfilePoint[,]>();
        List<ProfileSampleData> profileSamples = new List<ProfileSampleData>();
        /// <summary>
        /// 计算网格节点在剖面间的位置关系
        /// </summary>
        bool CalculateGridsLocations()
        {
            if(xGrid < 1 || yGrid < 1) return false;
            if (Slicers.Count < 2) return false;

            //平面网格点与剖面位置关系
            gridsLocation = new PointLineLocationResult[xGrid,yGrid];
            
            //剖面构造线段（互不相交的平行线段）
            Lines = Create2DLines();
            // 初始化定位器
            var locator = new LinePointLocator(Lines);

            // 测试点1：在线段1上
            double x, y;
            for(int iy = 0;iy<yGrid;iy++)
            {
                y = miny + iy * ystep;
                for (int ix = 0; ix < xGrid; ix++ )
                {
                    x = minx + ix * xstep;
                    gridsLocation[ix,iy] = locator.LocatePoint(new Vector64(x,y,0));
                }
            }
            return true;
        }
        List<CLine> Create2DLines()
        {
            Lines.Clear();            
            for(int i = 0; i < Slicers.Count; i++)
            {
                PolygonSlicer slicer = Slicers[i];
                if ( slicer.Locations3D.Count < 2 ) continue;
                Vector64 p1 = slicer.Locations3D[0];
                Vector64 p2 = slicer.Locations3D[slicer.Locations3D.Count-1];
                Lines.Add(new CLine(p1, p2, i, slicer.Name));
            }
            return Lines;
        }
        List<Vector32> SamplingFromImage(PolygonSlicer slicer,int nx, int ny,bool filterInvalid)
        {
            points.Clear();
            int[,] grids = C3DData.Stratums.SamplingFromImageByColor(slicer.BackgroundImage, nx, ny, SamplingMethodEnum.Squared);
            ImageStruct im = slicer.backImages[0];
            double imx1 = im.rect.X1;
            double imy1 = im.rect.Y1;
            double imx2 = im.rect.X2;
            double imy2 = im.rect.Y2;
            int col = grids.GetLength(0);
            int row = grids.GetLength(1);
            double dx = (imx2 - imx1) / col;
            double dy = (imy2 - imy1) / row;
            double x, y;            
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (filterInvalid && grids[j, i] < 0) continue;
                    x = imx1 + j * dx + dx / 2;
                    y = imy1 + i * dy + dy / 2;
                    Vector64 p = slicer.toTracedPoint(x, y, 0, 0);
                    points.Add(new Vector32( p.X, p.Y, p.Z, grids[j,i]) );
                }
            }
            grids = null;
            return points;
        }

        /// <summary>
        /// 剖面采样
        /// </summary>
        /// <returns></returns>
        bool ProfileSampleFromSlicers(double samplestep,bool invalidfilter)
        {
            int n = Slicers.Count;
            int nx, nz;
            double slicerlength = 0;
            profilePoints.Clear();
            StreamWriter wr = new StreamWriter("D:\\jian\\sampledpoints.csv");
            wr.WriteLine("x,y,z,value");
            for (int i = 0; i < n; i++) 
            {
                percentage = 100 * i / (double)n;
                progressTitle = "第" +(i+1)+"个剖面采样/" + n;
                PolygonSlicer slicer = Slicers[i];
                slicerlength = Math.Sqrt((slicer.Maxx - slicer.Minx) * (slicer.Maxx - slicer.Minx) +
                                (slicer.Maxy - slicer.Miny) * (slicer.Maxy - slicer.Miny));
                nx = (int)(slicerlength / samplestep + 0.1);
                nz = (int)((slicer.Maxz - slicer.Minz) / samplestep + 0.1);
                points = SamplingFromImage(slicer, nx, nz,invalidfilter);
                ProfilePoint[,] profile = new ProfilePoint[1, points.Count];
                for (int j = 0; j < points.Count; j++) 
                { 
                    Vector32 p = points[j];
                    wr.WriteLine(p.toString(4));
                    profile[0, j] = new ProfilePoint(p.X, p.Y, p.Z, (int)(p.V + 0.1), i);
                }
                points.Clear();
                profilePoints.Add(profile);
            }
            wr.Close();
            return true;
        }
        bool ProfileSampleFromSlicers1(double samplestep)
        {
            int nx, ny;
            double slicerlength = 0;
            int n = Slicers.Count;
            profileSamples.Clear();            
            for (int i = 0; i < n; i++)
            {
                percentage = 100 * i / (double)n;
                progressTitle = "第" + (i + 1) + "个剖面采样/" + n;
                PolygonSlicer slicer = Slicers[i];
                //sample step calculate
                slicerlength = Math.Sqrt((slicer.Maxx - slicer.Minx) * (slicer.Maxx - slicer.Minx) +
                                (slicer.Maxy - slicer.Miny) * (slicer.Maxy - slicer.Miny));
                nx = (int)(slicerlength / samplestep + 0.1);
                ny = (int)((slicer.Maxz - slicer.Minz) / samplestep + 0.1);
                DoubleRect rect = slicer.imageRect;
                ProfileSampleData profile = new ProfileSampleData(i,rect.X1,rect.X2,rect.Y1,rect.Y2,nx,ny);
                profile.stratumGrids = C3DData.Stratums.SamplingFromImageByColor(slicer.BackgroundImage, nx, ny, SamplingMethodEnum.Squared);
                profileSamples.Add(profile);
            }            
            return true;
        }


        List<ProfilePoint> GetPointsBetweenSlicers(PolygonSlicer slicer1,PolygonSlicer slicer2, int id1,int id2,C3DGridData data)
        {
            List<ProfilePoint> points = new List<ProfilePoint>();
            for (int iy = 0; iy < yGrid; iy++)
                for (int ix = 0; ix < xGrid; ix++)
                {
                    for (int iz = 0; iz < zGrid; iz++)
                    {
                        Vector64 p = data.GetGridCoord(ix, iy, iz).toVector64();
                        points.Add(new ProfilePoint(p.X, p.Y, p.Z, -1, -1));
                    }

                    //未考虑在剖面上
                    //if (gridsLocation[ix, iy].AdjacentLines.Count > 1)
                    //{
                    //    int lineid1 = gridsLocation[ix, iy].AdjacentLines[0];
                    //    int lineid2 = gridsLocation[ix, iy].AdjacentLines[1];
                    //    if ((lineid1 == id1 && lineid2 == id2) || (lineid1 == id2 && lineid2 == id1) )
                    //    {
                    //        for(int iz = 0; iz < zGrid; iz++) 
                    //        {
                    //            Vector64 p = data.GetGridCoord(ix, iy, iz).toVector64();
                    //            points.Add(new ProfilePoint(p.X,p.Y,p.Z,-1,-1));
                    //        }
                    //    }
                    //}                    
                }
            return points;
        }
        void SetGrids(List<ProfilePoint> points,int[]id,C3DGridData data)
        {
            for(int i=0; i < points.Count; i++)
            {
                ProfilePoint p = points[i];
                Int32XYZ xyz = data.GetIndices(new Vector32(p.X,p.Y,p.Z));
                data.SetGridValue(xyz.x, xyz.y, xyz.z, id[i]);
            }
        }
        /// <summary>
        /// 从剖面插值获取地层编号
        /// </summary>
        /// <param name="p">空间点坐标</param>
        /// <param name="id">剖面编号</param>
        /// <param name="rad">搜索半径（空间）</param>
        /// <returns>地层编号</returns>
        void GetInterpolatedStratumFromSlicer(Vector64 p, int id, double rad, double[]Weights )
        {
            PolygonSlicer slicer = Slicers[id];            
            //投影到剖面局部坐标
            Vector64 p0 = slicer.ProjectToLocalPoint(p.X, p.Y, p.Z);
            DoubleRect rect = slicer.imageRect;
            double rad1 = rad / slicer.slicerWidth * rect.Width; //半径转局部

            ProfileSampleData grid = profileSamples[id];
            double dx = rect.Width / grid.xGrid;
            double dy = rect.Height / grid.yGrid;            
            int nx = (int)(rad1 / dx + 0.1);
            int ny = (int)(rad1 / dy + 0.1);            
            int nn = Math.Max(nx,ny) / 2; //搜索网格大小
            Vector64 p1 = new Vector64(p0.X,p0.Y,0);
            Vector64 p2 = new Vector64(p0.X, p0.Y, 0);
            int ix0 = (int)((p0.X - rect.X1) / dx + 0.1);
            int iy0 = (int)((p0.Y - rect.Y1) / dy + 0.1); 
            int layerid = -1;
            double dist = 0;
            for (int iy = iy0 - nn; iy < iy0 + nn; iy++)
            {
                for (int ix = ix0 - nn; ix < ix0 + nn; ix++)
                {
                    if (ix < 0 || ix >= grid.xGrid) continue;
                    if (iy < 0 || iy >= grid.yGrid) continue;
                    p2.X = rect.X1 + ix * dx;
                    p2.Y = rect.Y1 + iy * dy;
                    dist = p1.Distance(p2);
                    if ( dist > rad1 ) continue;
                    layerid = grid.stratumGrids[ix, iy];
                    p2 = slicer.toTracedPoint(p2);
                    dist = p.Distance(p2);
                    if (dist <= 1e-8) { Weights[layerid + 1] = 1E30;return; }
                    else Weights[layerid + 1] += 1.0 / dist;
                }
            }
            
        }
        
        int ChooseLayer(double [] weights)
        {
            int layerid = -1;
            double maxwight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] > maxwight)
                {
                    maxwight = weights[i];
                    layerid = i - 1;
                }
            }
            return layerid;
        }
        /// <summary>
        /// 剖面间插值
        /// </summary>
        /// <param name="id1">Slicer剖面编号1</param>
        /// <param name="id2">Slicer剖面编号1</param>
        /// <param name="data">网格数据</param>
        void Interpolating(int id1, int id2, C3DGridData data)
        {
            //构建剖面线多边形（四边形）
            CLine line1 = Lines[id1];
            CLine line2 = Lines[id2];
            Vector64 p1, p2, p3, p4;
            if (Math.Abs(line1.XDirection) >= Math.Abs(line1.YDirection))
            {
                if (line1.p1.X <= line1.p2.X) { p1 = line1.p1; p2 = line1.p2; }
                else { p1 = line1.p2; p2 = line1.p1; }
                if (line2.p1.X <= line2.p2.X) { p3 = line2.p2; p4 = line2.p1; }
                else { p3 = line2.p1; p4 = line2.p2; }
            }
            else
            {
                if (line1.p1.Y <= line1.p2.Y) { p1 = line1.p1; p2 = line1.p2; }
                else { p1 = line1.p2; p2 = line1.p1; }
                if (line2.p1.Y <= line2.p2.Y) { p3 = line2.p2; p4 = line2.p1; }
                else { p3 = line2.p1; p4 = line2.p2; }
            }
            Polygon2D poly = new Polygon2D();
            poly.Add(p1); poly.Add(p2); poly.Add(p3); poly.Add(p4);

            //检索剖面间的网格点
            points.Clear();
            for (int iy = 0; iy < data.yNum; iy++)
            {
                for (int ix = 0; ix < data.xNum; ix++)
                {
                    Vector32 p = data.GetGridCoord(ix, iy, 0);
                    if (poly.IsPointInsidePoly(p)) points.Add(p);
                }
            }

            //剖面间的网格点插值
            IdwStratumInterpolatorGlobal ip = new IdwStratumInterpolatorGlobal(C3DData.Stratums);
            ip.Power = 1;            
            ip.NearestValueOnly = NearestOnly;
            ProfilePoint[,] profileA = profilePoints[id1];
            ProfilePoint[,] profileB = profilePoints[id2];
            for (int i = 0; i < profileA.GetLength(1); i++)
            {
                ProfilePoint p = profileA[0, i];
                ip.AddPoint(new Vector32(p.X, p.Y, p.Z, p.Value));
            }
            for (int i = 0; i < profileB.GetLength(1); i++)
            {
                ProfilePoint p = profileB[0, i];
                ip.AddPoint(new Vector32(p.X, p.Y, p.Z, p.Value));
            }

            int count = 0;
            int np = points.Count;
            Parallel.For(0, np, k=>
            //foreach (Vector32 p in points)
            {
                Vector32 p = points[k];
                for (int iz = 0; iz < data.zNum; iz++)
                {
                    double z = data.minz + data.zStep * iz;
                    Int32XYZ xyz = data.GetVerticIndexByPosition(p.X, p.Y, z);
                    data.SetGridValue(xyz.x, xyz.y, xyz.z, ip.GetInterpolatedValue(p.X, p.Y, z));
                }
                Interlocked.Increment(ref count);
                percentage = 100 * (double)count / (np - 1);
            });

            points.Clear();
            ip.Clear();
        }
        /// <summary>
        /// 剖面间插值
        /// </summary>
        /// <param name="id1">Slicer剖面编号1</param>
        /// <param name="id2">Slicer剖面编号1</param>
        /// <param name="data">网格数据</param>
        void Interpolating1(int id1,int id2, C3DGridData data)
        {
            //构建剖面线多边形（四边形）
            CLine line1 = Lines[id1];
            CLine line2 = Lines[id2];
            Vector64 p1, p2, p3, p4;
            if( Math.Abs(line1.XDirection) >= Math.Abs(line1.YDirection) )
            {
                if (line1.p1.X <= line1.p2.X) { p1 = line1.p1; p2 = line1.p2; }
                else { p1 = line1.p2; p2 = line1.p1; }
                if (line2.p1.X <= line2.p2.X) { p3 = line2.p2; p4 = line2.p1; }
                else { p3 = line2.p1; p4 = line2.p2; }
            }
            else 
            {
                if (line1.p1.Y <= line1.p2.Y) { p1 = line1.p1; p2 = line1.p2; }
                else { p1 = line1.p2; p2 = line1.p1; }
                if (line2.p1.Y <= line2.p2.Y) { p3 = line2.p2; p4 = line2.p1; }
                else { p3 = line2.p1; p4 = line2.p2; }
            }
            Polygon2D poly = new Polygon2D();
            poly.Add(p1); poly.Add(p2); poly.Add(p3); poly.Add(p4);

            //检索剖面间的网格点
            points.Clear();
            for(int iy = 0; iy < data.yNum; iy++)
            {
                for (int ix = 0; ix < data.xNum; ix++)
                {
                   Vector32 p = data.GetGridCoord(ix, iy, 0);
                    if( poly.IsPointInsidePoly(p))points.Add(p);
                }
            }

            //剖面间的网格点插值
            int layerid = -1;
            Vector64 vp = new Vector64();
            double rad = Math.Sqrt( data.xStep * data.xStep +
                                    data.yStep * data.yStep +
                                    data.zStep * data.zStep)*10;
            double []Weights = new double [C3DData.Stratums.Count + 1];
            percentage = 100 * 1.0 / points.Count;
            int count = 0;
            foreach (Vector32 p in points)
            {
                for (int iz = 0; iz < data.zNum; iz++)
                {
                    double z = data.minz + data.zStep * iz;
                    vp.X = p.X;vp.Y = p.Y;vp.Z = z;
                    
                    for (int i = 0; i < Weights.Length; i++) Weights[i] = 0;
                    GetInterpolatedStratumFromSlicer(vp, id1, rad, Weights);
                    GetInterpolatedStratumFromSlicer(vp, id2, rad, Weights);
                    layerid = ChooseLayer(Weights);
                    //if (line1.Distance(vp) < line2.Distance(vp)) layerid = layer1;
                    //else layerid = layer2;
                    Int32XYZ xyz = data.GetVerticIndexByPosition(p.X, p.Y, z);
                    data.SetGridValue(xyz.x, xyz.y, xyz.z, layerid);
                }
                count++;
                percentage = 100 * (double)count / points.Count;
            }
            Weights = null;
            points.Clear();
            //IdwStratumInterpolatorGlobal ip = new IdwStratumInterpolatorGlobal(C3DData.Stratums);
            //ip.Power = 1;
            ////ip.NearestValueOnly = true;
            //ProfilePoint[,] profileA = profilePoints[id1];
            //ProfilePoint[,] profileB = profilePoints[id2];
            //for (int i = 0; i < profileA.GetLength(1);i++)
            //{
            //    ProfilePoint p = profileA[0, i];                
            //    ip.AddPoint( new Vector32 (p.X,p.Y,p.Z,p.Value) );
            //}
            //for (int i = 0; i < profileB.GetLength(1); i++)
            //{
            //    ProfilePoint p = profileB[0, i];
            //    ip.AddPoint(new Vector32(p.X, p.Y, p.Z, p.Value));
            //}            
            //foreach(Vector32 p in points)
            //{
            //    for(int iz = 0;iz <data.zNum;iz++)
            //    {
            //       double z = data.minz + data.zStep * iz;
            //       Int32XYZ xyz = data.GetVerticIndexByPosition(p.X, p.Y, z);
            //       data.SetGridValue(xyz.x, xyz.y, xyz.z, ip.GetInterpolatedValue(p.X, p.Y, z));
            //    }
            //}
            //points.Clear();
            //ip.Clear();
        }
        public void Interpolating()
        {
            percentage = 1;
            progressTitle = "网格节点分类计算";
            if (grid3d == null) grid3d = new C3DGridData(xGrid, yGrid, zGrid, -1);
            else grid3d.Clear();
            
            grid3d.ResetDataRange(minx, maxx, miny, maxy, minz, maxz, -1, C3DData.Stratums.Count);
            grid3d.ColorScale = C3DData.Stratums.CreateColorScale();
            grid3d.ColorScale.SetValueRange(-1, C3DData.Stratums.Count);

            Create2DLines();
            //CalculateGridsLocations();
            percentage = 1;
            progressTitle = "剖面采样";            
            double step = Math.Min(grid3d.xStep, grid3d.yStep);
            step = Math.Min(grid3d.zStep, step) / 2;
            ProfileSampleFromSlicers(step, InvalidFilter);
            //double _clusterEpsilon = 2*Math.Sqrt(data.xStep* data.xStep + data.yStep* data.yStep + data.zStep* data.zStep);
            progressTitle = "正在插值计算";
            
            for (int i=0;i<Slicers.Count-1;i++)
            {                
                progressTitle = "正在进行第"+(i+1)+"个剖面计算/" + Slicers.Count;
                PolygonSlicer slicer1 = Slicers[i];
                PolygonSlicer slicer2 = Slicers[i+1];                
                Interpolating(i, i + 1, grid3d);
            }
            grid3d.SaveAs("d:\\jian\\slicers.3DGrid");
            progressTitle = "计算完成";
            percentage = 100;           
        }       
    }
    /// <summary>
    /// 剖面插值
    /// </summary>
    internal class SlicerInterpolation: InterpolatorBase
    {
        public int xNum, yNum, zNum;
        public List<PolygonSlicer>Slicers = new List<PolygonSlicer>();
        public List<int[,]> SampledGrids = new List<int[,]>();
        public PolygonSlicer Slicer1 = null;
        public PolygonSlicer Slicer2 = null;
        public C3DGridDataStratum stratumGrid = null;
        public string errMessage = "";
        public TriangleObj triObject = null;
        StratumDatas Stratums = null;
        public int[,] slicerSampled1 = null;
        public int[,] slicerSampled2 = null;        
        Rectangle validateRect1, validateRect2;
        public C3DGridData dataGrid3D = null;
        public List<TriangleObj> Objects = new List<TriangleObj>();
        public double SearchingStep = 10;
        public int ColorPickDiff = 20;
        public SlicerInterpolation(PolygonSlicer s1, PolygonSlicer s2) 
        {
            Slicer1 = s1;
            Slicer2 = s2;
        }
        public SlicerInterpolation()
        {            
        }
        public override void Clear()
        {
            base.Clear();
            if(dataGrid3D != null)dataGrid3D.Clear();
            Objects.Clear();
            slicerSampled1 = null;
            slicerSampled2 = null;
            Slicers.Clear();
            SampledGrids.Clear();
            stratumGrid = null;
        }
        /// <summary>
        /// 图像地层采样
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <returns></returns>
        public int[,]SamplingFromImage(Bitmap bmp,int nx,int ny)
        {
            return Stratums.SamplingFromImageByColor(bmp, nx, ny, SamplingMethodEnum.Squared, ColorPickDiff);
        }

        Bitmap ImageTransform(Bitmap bmp,Rectangle rect)
        {           
            Bitmap newbmp = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(newbmp);
            Rectangle rect0 = new Rectangle(0,0,bmp.Width,bmp.Height);
            g.DrawImage(bmp, rect, rect0, GraphicsUnit.Pixel);
            g.Dispose();
            return newbmp;
        }
        public void ResetStratumGrid()
        {
            for (int i = 0; i < stratumGrid.Length; i++)
            {
                //0非地层，1-n地层序号
                StratumIndexStruct Values = stratumGrid[i];
                for (int j = 0; j < Values.Count; j++) Values[j] = 0;
                stratumGrid[i] = Values;
            }
        }
        public bool CreateGrid(int nx, int ny, int nz,StratumDatas stratums) 
        {
            xNum = nx;
            yNum = ny;
            zNum = nz;
            dataGrid3D = new C3DGridData(xNum, yNum, zNum, -1);
            dataGrid3D.zAxis = AxisEnum.yAxis;
            stratumGrid = new C3DGridDataStratum(nx, ny, nz);
            stratumGrid.CreateGrids(stratums);
            stratumGrid.ResetDataRange(0, 100, 0, 100, 0, 100, 0, 1);
            Stratums = stratums;            
            return true;
        }
        public bool CreateGrid1(int nx, int ny, int nz, StratumDatas stratums)
        {
            xNum = nx;
            yNum = ny;
            zNum = nz;
            dataGrid3D = new C3DGridData(xNum, yNum, zNum, float.NaN);
            dataGrid3D.ResetDataRange(minx, maxx, miny, maxy, minz, maxz, 0, C3DData.Stratums.Count - 1);
            stratumGrid = new C3DGridDataStratum(nx, ny, nz);
            stratumGrid.CreateGrids(stratums);
            stratumGrid.ResetDataRange(minx, maxx, miny, maxy, minz, maxz, 0, C3DData.Stratums.Count - 1);
            Stratums = stratums;
            return true;
        }
        
        /// <summary>
        /// 用统计的方法对地层进行选择
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ix0"></param>
        /// <param name="iy0"></param>
        /// <param name="iz0"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        int StratumChooseByStatics(C3DGridData data,int ix0,int iy0,int iz0,List<int>layers)
        {
            int rad = 2;//搜索半径
            int ix1 = ix0 - rad;
            int ix2 = ix0 + rad;
            int iy1 = iy0 - rad;
            int iy2 = iy0 + rad;
            int iz1 = iz0 - rad;
            int iz2 = iz0 + rad;
            data.GeometryLimited(ref ix1, ref iy1, ref iz1);
            data.GeometryLimited(ref ix2, ref iy2, ref iz2);
            
            int value = 0;            
            int[] counties = new int[layers.Count];
            for(int i = 0; i < counties.Length; i++) counties[i] = 0;
            for (int i = 0; i < layers.Count; i++)
            {
                for (int iz = iz1; iz < iz2; iz++)
                {
                    for (int iy = iy1; iy < iy2; iy++)
                    {
                        for (int ix = ix1; ix < ix2; ix++)
                        {
                            value = (int)data[ix, iy, iz];//地层数据，初始值-1，无地层
                            if (value >= 0 && layers[i] == value)
                            counties[i]++;
                        }
                    }
                }
            }

            int ilayer = -1, count = 0;
            for (int i = 0; i < counties.Length; i++)
            {
                if (counties[i] > count) 
                {
                    count = counties[i]; 
                    ilayer = i; 
                }
            }
            counties = null;
            return ilayer;
        }
        void StratumChooseByZ(int z0,C3DGridData data,C3DGridDataStratum dataStratum)
        {
            List<int> layers = null;            
            for (int iy = 0; iy < yNum; iy++)
            {
                for (int ix = 0; ix < xNum; ix++)
                {
                    layers = ChooseStratums(dataStratum[ix, iy, z0]);
                    if (layers.Count == 0) data[ix, iy, z0] = -1;
                    else if (layers.Count == 1)data[ix, iy, z0] = layers[0];
                    else if (layers.Count > 1)
                        data[ix, iy, z0] = StratumChooseByStatics(data, ix, iy, z0, layers);
                }
            }
            layers.Clear();
        }
        void StratumGridChoose(C3DGridData data, C3DGridDataStratum dataStratum)
        {
            for(int i=0;i < dataStratum.Length;i++)
            {
                StratumIndexStruct s = dataStratum[i];
                //抉择出一个地层 0 - (n-1) n,-1无数据
                int id = ChooseOneFromStratums(s);
                if ( id < 0 ) data[i] = float.NaN;// 0 - n, -1需插值
                else data[i] = id;//0 - n ，0无地层
                percentage = (i + 1) * 100.0 / dataStratum.Length;
            }
        }
        void StratumChoose(C3DGridData data,C3DGridDataStratum dataStratum)
        {
            //(1)创建第一个截面 z0 = 0
            StratumChooseByZ(0, data, dataStratum);
            //(2)创建最后一个截面 z0 = zNum - 1            
            StratumChooseByZ(zNum - 1, data, dataStratum);
            int iz,z0 = zNum / 2;
            for(iz = 1; iz < z0; iz++)StratumChooseByZ(iz, data, dataStratum);
            for (iz=zNum-2;iz>=z0;iz--)StratumChooseByZ(iz, data, dataStratum);
        }

        /// <summary>
        /// choose the value highest
        /// </summary>
        /// <param name="stratums"></param>
        /// <returns></returns>
        List<int> ChooseStratums(StratumIndexStruct stratums)
        {
            //先找出最大值
            double maxvalue = 0;
            for(int i = 0; i <stratums.Count; i++ )
            {
                if ( stratums[i] > maxvalue) 
                    maxvalue = stratums[i];
            }
            //统计最大值的个数
            List<int> layers = new List<int>();
            if ( maxvalue > 0)
            {
                for (int i = 0; i < stratums.Count; i++)
                {
                    if (stratums[i] == maxvalue)
                        layers.Add(i);
                }
            }
            return layers;
        }

        //抉择出一个地层 0 - (n-1) n,-1无数据
        int ChooseOneFromStratums(StratumIndexStruct stratums)
        {
            double count = 0;
            int ilayer = -1;
            for( int i = 0; i < stratums.Count; i++ )
            {
                if( stratums[i] > count ) 
                {
                    count = stratums[i];
                    ilayer = i;
                }
            }
            return ilayer;
        }

        IdwInterpolatorLocalKdTree kdTreeInterpolation = new IdwInterpolatorLocalKdTree(3,2,1);
        public void GridSampleFromSlicers1()
        {
            int id;
            Vector64 p;
            StratumIndexStruct layers;
            int[,] sample1 = null;
            int nx = (int)(xNum * 2);
            int ny = (int)(yNum * 2);
            float val = -1;
            double x, y, z, dx, dy;
            int count = 0;
            double total = nx * ny * Slicers.Count;
            percentage = 0;
            kdTreeInterpolation.Clear();
            for (int i = 0; i < Slicers.Count; i++)
            {
                PolygonSlicer slicer = Slicers[i];
                sample1 = SamplingFromImage(slicer.BackgroundImage, nx, ny);
                dx = (slicer.maxx - slicer.minx) / (nx - 1);
                dy = (slicer.maxy - slicer.miny) / (ny - 1);
                for (int ix = 0; ix < nx; ix++)
                {
                    x = slicer.minx + ix * dx;
                    for (int iy = 0; iy < ny; iy++)
                    {
                        y = slicer.miny + iy * dy;
                        val = sample1[ix, iy]; //地层序号-1，0,(n-1)
                        p = slicer.toTracedPoint(x, y, 0, 0);
                        if (val >= 0) 
                        {
                            p.V = val;
                            kdTreeInterpolation.AddPoint(p.X,p.Y,p.Z,p.V);
                        }
                        count++;
                        percentage = 100.0 * count / total;
                    }
                }
                sample1 = null;
            }
        }
        public void GridSampleFromSlicers()
        {
            int id;
            Vector64 p;
            StratumIndexStruct layers;            
            int[,] sample1 = null;
            int nx = xNum;
            int ny = yNum;
            float val = -1;
            double x, y, z, dx, dy;
            int count = 0;
            double total = nx * ny* Slicers.Count;
            percentage = 0;
            //StreamWriter br = new StreamWriter(new FileStream(@"C:\jian\2024\简楚\攀枝花\红格\勘探剖面\勘探线\all.dat", FileMode.Create));
            for (int i = 0; i < Slicers.Count; i++)
            {
                PolygonSlicer slicer = Slicers[i];
                sample1 = SamplingFromImage(slicer.BackgroundImage, nx, ny);
                SampledGrids.Add(sample1);

                dx = (slicer.maxx - slicer.minx) / (nx-1);
                dy = (slicer.maxy - slicer.miny) / (ny-1);
                for (int ix = 0; ix < nx; ix++)
                {
                    x = slicer.minx + ix * dx;
                    for (int iy = 0; iy < ny; iy++)
                    {
                        y = slicer.miny + iy * dy;
                        val = sample1[ix, iy]; //地层序号-1，0,(n-1)
                        p = slicer.toTracedPoint(x, y, 0, 0);
                        Int32XYZ xyz = stratumGrid.GetIndices(p);
                        id = (int)stratumGrid.GetVerticIndex(xyz.x, xyz.y, xyz.z);
                        layers = stratumGrid[id];
                        if ( val < 0) layers[0]++;
                        else 
                        { 
                            layers[(int)(val) + 1]++;
                            p.V = val;
                        }
                        stratumGrid[id] = layers;
                        count++;
                        percentage = 100.0 * count / total;
                    }
                }
                //sample1 = null;
            }
          //  br.Close();
        }
        public void InterpolatedThread()
        {
            int id;
            Vector64 p;
            StratumIndexStruct layers;
            int count = 0;
            double total = xNum * yNum * zNum;
            progressTitle = "正在插值";
            
            for (int iy = 0; iy < yNum; iy++)
            {
                for (int ix = 0; ix < xNum; ix++)
                {
                    for (int iz = 0; iz < zNum; iz++)
                    {
                        id = (int)stratumGrid.GetVerticIndex(ix, iy, iz);
                        p = GetPositionFrom(ix, iy, iz);
                       // stratumGrid.pGridCoords[id] = p;
                        layers = stratumGrid[id];
                        WeightInterpolate(p, ix, iy, Slicer1, slicerSampled1, ref layers);
                        WeightInterpolate(p, ix, iy, Slicer2, slicerSampled2, ref layers);
                        stratumGrid[id] = layers;

                        count++;
                        percentage = 100.0 * count / total;
                    }
                }
            }            
        }

        //public void InterpolatedThread()
        //{
        //    int id;
        //    Vector64 p;
        //    StratumIndexStruct layers;
        //    int count = 0;
        //    double total = xNum * yNum * zNum;
        //    progressTitle = "正在插值";
        //    for (int iz = 0; iz < zNum; iz++)
        //    {
        //        for (int iy = 0; iy < yNum; iy++)
        //        {
        //            for (int ix = 0; ix < xNum; ix++)
        //            {
        //                id = (int)stratumGrid.GetVerticIndex(ix, iy, iz);
        //                p = GetPositionFrom(ix, iy, iz);
        //                stratumGrid.pGridCoords[id] = p;
        //                layers = stratumGrid[id];
        //                WeightInterpolate(p, Slicer1, slicerSampled1, validateRect1, ref layers);
        //                WeightInterpolate(p, Slicer2, slicerSampled2, validateRect2, ref layers);
        //                stratumGrid[id] = layers;
        //                count++;
        //                percentage = progressPercent = 100.0 * count / total;                        
        //            }
        //        }
        //    }            
        //}

        public CColorScale CreateColorScale()
        {
            CColorScale colorScale = new CColorScale();
            colorScale.Levels.Clear(); 
            
            ColorLevel lvl = new ColorLevel(0,1,1,1); //背景，无地层
            colorScale.Levels.Add(lvl);
            
            float step = 100f / Stratums.Count;

            for (int i = 0; i < Stratums.Count; i++)
            {
                float percent = (i+1) * step;
                float r = Stratums[i].Color.R / 255f;
                float g = Stratums[i].Color.G / 255f;
                float b = Stratums[i].Color.B / 255f;
                lvl = new ColorLevel(percent, r, g, b);
                colorScale.Levels.Add(lvl);
            }
            colorScale.SetValueRange(0, Stratums.Count);
            return colorScale;
        }
        public void StratumSampling(bool s1,bool s2)
        {
            if(s1) slicerSampled1 = SamplingFromImage(Slicer1.BackgroundImage, xNum, yNum);
            if(s2) slicerSampled2 = SamplingFromImage(Slicer2.BackgroundImage, xNum, yNum);
        }
        public void GridsSampledInterpolating1(C3DGridData data)
        {
            int count = 0;
            int total = data.Length;
            Vector32 p;
            for (int iz = 0; iz < data.zNum; iz++)
            {
                for (int iy = 0; iy < data.yNum; iy++)
                {
                    for (int ix = 0; ix < data.xNum; ix++)
                    {
                        p = data.GetGridCoord(ix, iy, iz);
                        p = kdTreeInterpolation.FindNearestPoint(p.X, p.Y, p.Z);
                        data[ix, iy, iz] = p.V;
                        count++;
                        percentage = count * 100.0 / total;
                    }
                }
            }
            kdTreeInterpolation.Clear();
        }

        //切片平面搜索插值
        public void GridsSampledInterpolating2(C3DGridData data)
        {
            int count = 0;
            int total = data.Length;
            Vector32 p;            
            for (int iz = 0; iz < data.zNum; iz++)
            {
                for (int iy = 0; iy < data.yNum; iy++)
                {
                    for (int ix = 0; ix < data.xNum; ix++)
                    {
                        p = data.GetGridCoord(ix, iy, iz);
                        p.V = SearchFromSlicers(p);

                        //if(iy == data.yNum / 2 && iz == data.zNum / 2)
                        //{
                        //    StreamWriter br = new StreamWriter(new FileStream(@"C:\jian\2024\简楚\攀枝花\红格\勘探剖面\勘探线\1.dat", FileMode.Append));
                        //    br.WriteLine(p.toString(4));
                        //    br.Close();
                        //    p.V = SearchFromSlicers(p,true);
                        //}
                        //else p.V = SearchFromSlicers(p);
                        data[ix, iy, iz] = p.V;
                        count++;
                        percentage = count * 100.0 / total;
                    }
                }
            }            
        }

        int getLayerValueFromSlicer(int islicer,Vector64 p0)
        {
            int ix = 0, iy = 0;
            PolygonSlicer slicer = Slicers[islicer];
            if (slicer.axis == AxisEnum.zAxis)
            {
                double xx = (slicer.Maxx - slicer.Minx) / (xNum - 1);
                double yy = (slicer.Maxz - slicer.Minz) / (zNum - 1);
                ix = (int)((p0.X - slicer.Minx) / xx);
                iy = (int)((p0.Z - slicer.Minz) / yy);                
            }
            if (slicer.axis == AxisEnum.yAxis)
            {
                double xx = (slicer.Maxx - slicer.Minx) / (xNum - 1);
                double yy = (slicer.Maxy - slicer.Miny) / (yNum - 1);
                ix = (int)((p0.X - slicer.Minx) / xx + 0.1);
                iy = (int)((p0.Y - slicer.Miny) / yy + 0.1);
            }
            int x1 = ix - 2;
            int x2 = ix + 2;
            int y1 = iy - 2;
            int y2 = iy + 2;
            int[,] sample = SampledGrids[islicer];
            for (int y = y1; y <= y2; y++)
            {
                for (int x = x1; x <= x2; x++)
                {
                    if (x < 0 || x >= xNum) continue;
                    if (y < 0 || y >= yNum) continue;
                    if( sample[x,y] >= 0 ) return sample[x, y] + 1;
                }
            }
            return 0;
        }
        

        float SearchFromSlicers(Vector32 p,bool write = false)
        {
            PolygonSlicer slicer;
            Vector64 p0 = new Vector64(), p1 = new Vector64();
            Vector64 p2 = new Vector64(), p3=new Vector64(), p4 = new Vector64();
            CPlane3F plane = new CPlane3F();
            double dist = 0, mindist = 1E10;
            float val = -1;
            for (int i=0;i<Slicers.Count;i++)
            {
                slicer = Slicers[i];
                if( slicer.axis == AxisEnum.zAxis) 
                {
                    p1 = new Vector64(slicer.Minx, slicer.Miny, slicer.Minz);
                    p2 = new Vector64(slicer.Minx, slicer.Miny, slicer.Maxz);
                    p3 = new Vector64(slicer.Maxx, slicer.Maxy, slicer.Maxz);
                    //p4 = new Vector64(slicer.Maxx, slicer.Maxy, slicer.Minz);                    
                }
                else if (slicer.axis == AxisEnum.yAxis)
                {
                    p1 = new Vector64(slicer.Minx, slicer.Miny, slicer.Minz);
                    p2 = new Vector64(slicer.Minx, slicer.Maxy, slicer.Minz);
                    p3 = new Vector64(slicer.Maxx, slicer.Maxy, slicer.Minz);
                    //p4 = new Vector64(slicer.Maxx, slicer.Miny, slicer.Minz);                    
                }
                plane = new CPlane3F(p1, p2, p3);
                p0 = plane.Projected(p.toVector64());
                dist = plane.Distance(p.toVector64());
                if (p0.X >= slicer.Minx && p0.X <= slicer.Maxx &&
                    p0.Y >= slicer.Miny && p0.Y <= slicer.Maxy &&
                    p0.Z >= slicer.Minz && p0.Z <= slicer.Maxz )
                {
                    if ( dist < mindist ) 
                    { 
                        mindist = dist;                        
                        val = getLayerValueFromSlicer(i, p0);
                        p4 = p0;
                        p4.V = val;
                    } 
                }
            }
            if (write)
            {
                StreamWriter br = new StreamWriter(new FileStream(@"C:\jian\2024\简楚\攀枝花\红格\勘探剖面\勘探线\2.dat", FileMode.Append));
                br.WriteLine(p4.toString(4));
                br.Close();
            }
            return val;            
        }
        /// <summary>
        /// 网格重插值
        /// </summary>
        /// <param name="invalid_value">需插值网格</param>
        /// <param name="blank_interpolate">是否对白化网格插值</param>
        public void GridsSampledInterpolating(C3DGridData data, float invalid_value, bool blank_interpolate)
        {
            long id;
            float val;
            float[] grid = new float[data.Length];
            int count = 0;
            int total = grid.Length;
            for(int iz=0;iz < data.zNum;iz++)
            {
                for (int iy = 0; iy < data.yNum; iy++)
                {
                    for (int ix = 0; ix < data.xNum; ix++)
                    {
                        id = data.GetVerticIndex(ix, iy, iz);
                        val = data[id];                        
                        if ((data.IsBlankValue(val) && blank_interpolate)
                            || val <= 0 )
                        {
                            val = ReInterpolateFromGrid(ix,iy,iz, data);
                        }
                        grid[id] = val;
                        count++;
                        percentage = count * 100.0 / total;
                    }
                }
            }
            //br.Close();
            dataGrid3D.pGridData = null;
            dataGrid3D.pGridData = grid;
        }
        public float ReInterpolateFromGrid(int ix,int iy,int iz,C3DGridData data)
        {
            points = data.SearchNearestPoints(ix,iy,iz,-1);
            if ( points.Count < 1)return float.NaN;
            float val = 0;
            double maxdist = 1E10, dist;
            Vector32 p0 = data.GetGridCoord(ix, iy, iz);            
            for (int i=0;i<points.Count; i++)
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
        public void CreateModelThread1()
        {           
           // try 
            {
                threadStoped = false;
                //1-创建网格-----------------------------
                progressTitle = "1正在创建网格";
                CreateGrid1(xNum, yNum, zNum, C3DData.Stratums);
                progressTitle = "2正在进行地层采样";
                //2-网格插值-----------------------------
                GridSampleFromSlicers();
                //GridSampleFromSlicers1();//kdtree
                //3-地层赋值-----------------------------
                progressTitle = "3正在进行地层统计";
                StratumGridChoose(dataGrid3D, stratumGrid);
                progressTitle = "4正在进行地层插值";
                GridsSampledInterpolating(dataGrid3D, -1, true);
                //GridsSampledInterpolating1(dataGrid3D);//kdtree
                //GridsSampledInterpolating2(dataGrid3D);
                //4-地层纠正-----------------------------
                //for (int i = 0; i < dataGrid3D.Length; i++) dataGrid3D[i]++;
                dataGrid3D.ColorScale = CreateColorScale();
                dataGrid3D.minv = 0;
                dataGrid3D.maxv = Stratums.Count;

                dataGrid3D.SaveAs(@"C:\jian\2024\简楚\攀枝花\红格\勘探剖面\勘探线\all.3DGrid", 13);
                                
                progressTitle = "计算完成";
                Thread.Sleep(300);
                threadStoped = true;                
            }
            //catch (Exception e) 
            //{
            //    errMessage = e.Message;                
            //}
        }
        public void CreateModelThread()
        {
            if (stratumGrid == null) 
            {
                errMessage = "no grids created.";
                return; 
            }
          //  try 
            {
                threadStoped = false;
                
                progressTitle = "正在插值";
                //1-网格插值-----------------------------
                InterpolatedThread();
                 //2-归一化-----------------------------
                progressTitle = "正在归一化";
                stratumGrid.UpdateRange();
                stratumGrid.Normalize();
                //3-地层赋值-----------------------------
                progressTitle = "正在统计地层";
                
                dataGrid3D.ResetDataRange(0, 100, 0, 100, 0, 100, 0, Stratums.Count+1);
               // dataGrid3D.pGridCoords = stratumGrid.pGridCoords;

                StratumChoose(dataGrid3D, stratumGrid);

                //4-地层纠正-----------------------------
                progressTitle = "正在进行地层校正：";
                int count0 = 0;
                int k = 0;
                while (true)
                {
                    int count = DoStratumCorrecting(dataGrid3D, 2);
                    progressTitle = "正在进行地层校正：" + count + "(个).";
                    k++;
                    if (count < 1) break;
                    if (count == count0) break;
                    if (count != count0) count0 = count;
                    if (k >= 100) break;
                }
                //dataGrid3D.UpdateRange();

                //convert -1 to (Count-1) to 1 to Count
                for (int i = 0; i < dataGrid3D.Length; i++) dataGrid3D[i]++;
                dataGrid3D.ColorScale = CreateColorScale();
                //dataGrid3D.UpdateRange();

                //dataGrid3D.SaveAs(@"C:\jian\2024\简楚\攀枝花\白马\slice12modeling.3DGrid",13);

                //4-地层等值面提取-----------------------------
                //progressTitle = "正在创建地层等值面";
                //Objects.Clear();

                //List<PointF> values = new List<PointF>();
                //for (int i = 0; i < Stratums.Count-2; i++)
                //{
                //    values.Add(new PointF(i, i + 1));
                //}
                //dataGrid3D.CreateMarchingCubeTriangleExt(values);

                //TriangleObj obj = dataGrid3D.m_MarchCubeExt.pISOSurfaceExt.toTriangleObj();
                //obj.uniformColor = Stratums[Stratums.Count-1].Color;
                //obj.IsUniformColor = true;

                //progressTitle = "正在坐标转换";
                //Vector64 p,p1, p2;
                //double zs;
                //for(int i=0;i<obj.points.Count;i++)
                //{
                //    Vector32 v = obj.points[i];                    
                //    p1 = GetPositionFrom(v, dataGrid3D, Slicer1);
                //    p2 = GetPositionFrom(v, dataGrid3D, Slicer2);
                //    zs = (v.Z - dataGrid3D.minz) / (dataGrid3D.maxz - dataGrid3D.minz);
                //    p = p1 + zs * (p2 - p1);
                //    v.X = (float)p.X;
                //    v.Y = (float)p.Y;
                //    v.Z = (float)p.Z;
                //    obj.points[i] = v;
                //}
                //obj.UpdateRange();
                //Objects.Add(obj);

                //dataGrid3D.Clear();
                //dataGrid3D = null;
                //stratumGrid.Clear();

                progressTitle = "计算完成";
                Thread.Sleep(300);
                threadStoped = true;
                
//                return true;
            }
          //  catch (Exception e) 
            {
             //   errMessage = e.Message;
            //    return false;                
            }
        }

        //void WeightInterpolate( Vector64 p, PolygonSlicer slicer, int[,]grid, 
        //                        Rectangle rect, ref StratumIndexStruct stratumsUnit, double err = 1E-6 )
        //{
        //    int id;
        //    double dist = 0,weight = 0;

        //    for (int iy = 0; iy < yNum; iy++)
        //    {
        //        for (int ix = 0; ix < xNum; ix++)
        //        {
        //            id = grid[ix, iy];

        //            dist = p.DistancePower2(GetPositionFrom(slicer, ix, iy));
        //            if (dist <= err) 
        //            {
        //                //id < 0 此处无地层，地层序号1-n
        //                stratumsUnit[id+1] = double.MaxValue;
        //            }
        //            else 
        //            {
        //                //id < 0 此处无地层，地层序号1-n
        //                weight = 1.0 / dist;
        //                if(stratumsUnit[id + 1]< weight) 
        //                    stratumsUnit[id + 1]=weight;
        //                //stratumsUnit[id+1] += weight;
        //            }                    
        //        }
        //    }
        //}

        void GetSearchingGridRad(ref int xRad,ref int yRad)
        {
            xRad = (int)(xGrid * SearchingStep / 100);
            yRad = (int)(yGrid * SearchingStep / 100);
            if (xRad <= 0) xRad = 1;
            if (xRad > xNum) xRad = xNum;
            if (yRad <= 0) yRad = 1;
            if (yRad > yNum) yRad = yNum;
        }

        void WeightInterpolate( Vector64 p, int ix0,int iy0,
                                PolygonSlicer slicer, int[,] grid,
                                ref StratumIndexStruct stratumsUnit, 
                                double err = 1E-6)
        {
            int iLayer = 0;
            double dist = 0, weight = 0;
            int rx = 0, ry = 0;
            GetSearchingGridRad(ref rx, ref ry);
            int ix1 = ix0 - rx;
            int ix2 = ix0 + rx;
            if (ix1 < 0) ix1 = 0;
            if (ix2 > xNum) ix2 = xNum;
            int iy1 = iy0 - ry;
            int iy2 = iy0 + ry;
            if (iy1 < 0) iy1 = 0;
            if (iy2 > yNum) iy2 = yNum;

            for (int iy = iy1; iy < iy2; iy++)
            {
                for (int ix = ix1; ix < ix2; ix++)
                {
                    iLayer = grid[ix, iy]; // grid[ix, iy]:-1无地层，0地层0，1地层1
                    if ( iLayer < 0 ) continue;

                    dist = p.DistancePower2(GetPositionFrom(slicer, ix, iy));
                    if ( dist <= err ) stratumsUnit[iLayer] = double.MaxValue;
                    else
                    {
                        weight = 1.0 / dist;
                        //if (stratumsUnit[iLayer] < weight)
                        //    stratumsUnit[iLayer] = weight;
                        stratumsUnit[iLayer] += weight;
                    }
                }
            }
        }

        //地层孤点检查
        bool StratumCorrect(C3DGridData data, int ix0,int iy0,int iz0,int rad = 2) 
        {
            int ix1 = ix0 - rad;
            int ix2 = ix0 + rad;
            int iy1 = iy0 - rad;
            int iy2 = iy0 + rad;
            int iz1 = iz0 - rad;
            int iz2 = iz0 + rad;
            data.GeometryLimited(ref ix1, ref iy1, ref iz1);
            data.GeometryLimited(ref ix2, ref iy2, ref iz2);
            
            int ilayer = -1;
            int[] layerCounts = new int[Stratums.Count];
            for (int i = 0; i < layerCounts.Length; i++) layerCounts[i] = 0;
            int layer0 = (int)data[ix0, iy0, iz0];
            for (int iz = iz1; iz < iz2; iz++)
            {
                for (int iy = iy1; iy < iy2; iy++)
                {
                    for (int ix = ix1; ix < ix2; ix++)
                    {
                        //中心点（自己）
                        if (ix == ix0 && iy == iy0 && iz == iz0) continue;
                        ilayer = (int)data[ix, iy, iz];
                        if( ilayer >= 0 )layerCounts[ilayer]++;
                    }
                }
            }

            ilayer = -1;
            int count = 0;
            int total = 0;
            for (int i = 0; i < layerCounts.Length; i++)
            {
                total += layerCounts[i];
                if ( layerCounts[i] > count ) 
                {
                    count = layerCounts[i];
                    ilayer = i;
                }
            }
            layerCounts = null;
            double percent = 100.0 * (double)count / total;
            if (percent >= 80) 
            { 
                data[ix0, iy0, iz0] = ilayer;
                return true;
            }
            return false;
        }

        int DoStratumCorrecting(C3DGridData data,int rad = 2)
        {
            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;
            int count = 0;
            for (int i = 0; i < data.Length;i++)
            {
                Int32XYZ xyz = data.GetIndices(i);
                if (xyz.x < rad || xyz.x > nx - rad) continue;
                if (xyz.y < rad || xyz.y > ny - rad) continue;
                if (xyz.z < rad || xyz.z > nz - rad) continue;
                if( StratumCorrect(data,xyz.x,xyz.y,xyz.z))count++;
            }
            return count;
        }

        Vector64 GetPositionFrom(PolygonSlicer slicer,int ix, int iy)
        {
            double x, y;
            double dx = (slicer.maxx - slicer.minx) / (xNum - 1);
            double dy = (slicer.maxy - slicer.miny) / (yNum - 1);
            x = slicer.minx + ix * dx;
            y = slicer.miny + iy * dy;
            return slicer.toTracedPoint(new Vector64(x, y, 0));
        }

        /// <summary>
        /// X-Y is slicer direction
        /// Z is 剖面插值方向
        /// </summary>
        /// <param name="nx"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <returns></returns>
        Vector64 GetPositionFrom(int ix, int iy, int iz)
        {
            Vector64 p1 = GetPositionFrom(Slicer1, ix, iy);
            Vector64 p2 = GetPositionFrom(Slicer2, ix, iy);
            Vector64 p = p1 + iz / (zNum -1)* (p2 - p1);
            return p;
        }
        Vector64 GetPositionFrom(Vector32 p, C3DGridData data, PolygonSlicer slicer)
        {
            Int32XYZ xyz = data.GetIndices(p);
            double sx = (p.X - xyz.x * data.xStep) / data.xStep;
            double sy = (p.Y - xyz.y * data.yStep) / data.yStep;            

            Vector64 p0 = GetPositionFrom(slicer, xyz.x, xyz.y);
            Vector64 p1 = GetPositionFrom(slicer, xyz.x + 1, xyz.y);
            Vector64 p2 = GetPositionFrom(slicer, xyz.x, xyz.y + 1);
            
            double xx = p1.X - p0.X;
            double yy = p2.Y - p0.Y;

            p0.X += xx * sx;
            p0.Y += yy * sy;

            return p0;
        }
        //-----------------------------------
    }
}
