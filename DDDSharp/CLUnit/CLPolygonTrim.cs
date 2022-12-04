///////////////////////////////////////////////////////////////////////////////////////
////多边形切割计算类，集成自OpenCL并行计算，--Create by jian 2019-10-12
/// 作用：1.完成3D多面体与网格数据的切割计算，并计算交点
///       2.完成切片基面线与网格数据的切割计算，并计算交点（已完成）
/// Last modified: 2020-3-12, finished the XOY,YOZ,XOZ slicer triming.
///
///
//////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DataCollection;
using OpenCLNet;
using GlmNet;
namespace CLPolygonTrim
{
    public class PolygonTrimUnit: CLUnit
    {
        public bool Stop = false;
        public bool[] blankedGrids = null; 
        public PolygonTrimUnit(Device _device):base(_device)
        {

        }
        public override void ReleaseMemory()
        {
            blankedGrids = null;
        }
    }
    public class PolygonTrim
    {
        Object lock1 = new object();
        Object lock2 = new object();

        public double Percentage = 0;//(0-100)%
        public double TimeLeft = 0; //by second
        public double TimeElapsed = 0; //by second
        public string progressTitle = "";
        public string errMsg = "";
        public bool threadStop = false;

        int progressStep = 0;
        DateTime startTime;
        
        PolygonTrimUnit[] clDevices = null;
        int xGrid, yGrid, zGrid,pointsNum,triNum;
        private double[] px;
        private double[] py;
        private double[] pz;
        private int[] triangle1;
        private int[] triangle2;
        private int[] triangle3;
        private bool[] blankedGrids;
        double xstep, ystep, zstep;
        double minx, miny, minz, maxx, maxy, maxz;
        public string formatTime(double second)
        {
            int hours = (int)(second / 3600);
            double sec1 = second - hours * 3600;
            int minutes = (int)(sec1 / 60);
            double sec2 = Math.Round(sec1 - 60 * minutes, 0);
            string tmstr = hours + ":" + minutes + ":" + sec2;
            return tmstr;
        }
        public string strTimeLeft 
        {
            get { return formatTime(TimeLeft); }
        }
        bool CreateMemory(TriangleObj obj)
        {
            try 
            {
                px = new double[obj.points.Count];
                py = new double[obj.points.Count];
                pz = new double[obj.points.Count];
                for(int i=0;i< obj.points.Count;i++)
                {
                    px[i] = obj.points[i].x;
                    py[i] = obj.points[i].y;
                    pz[i] = obj.points[i].z;
                }
                triangle1 = new int[obj.triangles.Count];
                triangle2 = new int[obj.triangles.Count];
                triangle3 = new int[obj.triangles.Count];
                for(int i=0;i< obj.triangles.Count;i++)
                {
                    triangle1[i] = obj.triangles[i].x;
                    triangle2[i] = obj.triangles[i].y;
                    triangle3[i] = obj.triangles[i].z;
                }
                blankedGrids = new bool[xGrid* yGrid * zGrid];
                return true;
            }
            catch(Exception e)
            {
                errMsg = "allocating memory failed,no enough memory on CPU.\n" + e.Message;
                return false;
            }
        }

        public bool InitDevices(Device[] devices)
        {
            clDevices = null;
            clDevices = new PolygonTrimUnit[devices.Length];
            for (int i = 0; i < clDevices.Length; i++)
            {
                clDevices[i] = new PolygonTrimUnit(devices[i]);
            }
            return true;
        }
        public bool CompileDevice(string source)
        {
            for (int i = 0; i < clDevices.Length; i++)
            {
                clDevices[i].AddFunction("gridsBlankTest");
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
        public void ReleaseDevices()
        {
            if(clDevices != null )
            {
                for(int i=0;i<clDevices.Length;i++)
                {
                    clDevices[i].ReleaseMemory();
                    clDevices[i].ReleaseCL();
                }
            }
            clDevices = null;
        }
        public void Clear() 
        {
            px = null;
            py = null;
            pz = null;
            triangle1 = null;
            triangle2 = null;
            triangle3 = null;
        }
        ulong GetRequiredMemorySize(TriangleObj obj)
        {
            ulong sum = (ulong)(obj.triangles.Count * 3 *sizeof(int));
            sum += (ulong)(obj.points.Count * 3 * sizeof(double));
            return sum;
        }
        //任务池，用于指定当前任务
        Queue<TaskPoolIndices> taskPools = new Queue<TaskPoolIndices>();
        struct TaskPoolIndices
        {
            public long start; //当前任务位置
            public int num;    //任务长度
            public TaskPoolIndices(long _start, int _num)
            {
                start = _start;
                num = _num;
            }
        }
        //创建任务池，每次以最小单元
        int CreateTaskPools(long total, int batch = 100)
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
        TaskPoolIndices GetFromTaskPools()
        {
            lock (lock1)
            {
                if (taskPools.Count < 1) return new TaskPoolIndices(0, 0);
                else return taskPools.Dequeue();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="slicer"></param>
        /// <param name="para">
        /// slicer:  0-keep left,1 keep right
        /// Closed slicer: 0 - keep left, 1 keep right
        /// </param>
        /// <returns></returns>
        private bool TrimWithXOYSlicer(C3DGridData grid, CSlicer slicer, int para = 0)
        {
            int ix, iy, iz, id, id3;
            Vector64 p1, p2;
            Vector64 p = new Vector64();

            bool keepleft = true;
            if (para == 1) keepleft = false;            
            bool keepinside = !keepleft;

            Polygon2D polygon;
            if (slicer.Closed)
            {
                polygon = new Polygon2D(slicer.toXYPoints(slicer.pBaseLine));
                if (para == 0) keepinside = true;
                else if (para == 1) keepinside = false;
            }
            else
            {
                //线与边界求交，返回相交多边形（顺时针方向）
                polygon = slicer.RectIntersect(minx, miny, minz, maxx, maxy, maxz);
                if (polygon == null)
                {
                    errMsg = "failed.";
                    return false;
                }
            }

            bool[] point_in = new bool[xGrid * yGrid];
            
            double x1, x2, y1, y2, z1, z2;
            int height1 = (int)((slicer.minHeight - minz) / zstep);
            int height2 = (int)((slicer.maxHeight - minz) / zstep);
            if ( slicer.maxHeight - height2 * zstep > minz ) height2++;
            if (height1 < 0) height1 = 0;
            if (height2 > zGrid - 1) height2 = zGrid - 1;

            Percentage = 0;
            double step = 100 / (2*yGrid);
            double sec = 0;
            progressTitle = "calculating blanked grids...";
            
            DateTime t1 = DateTime.Now;

            //1 check in or out
            for (iy = 0; iy < yGrid; iy++)
            {
                for (ix = 0; ix < xGrid; ix++)
                {
                    p.X = minx + ix * xstep;
                    p.Y = miny + iy * ystep;
                    p.Z = maxz;
                    id = iy * xGrid + ix;
                    point_in[id] = polygon.IsPointInsidePoly(p);
                }
                if (iy == 0)
                {
                    sec = (DateTime.Now - t1).TotalSeconds;
                }
                //if (iy % step == 0)
                {
                    TimeLeft = (2*yGrid - iy - 1) * sec;
                    Percentage += step;
                }
                Percentage += step;
            }

            //2 blank grid
            for (iy = 0; iy < yGrid; iy++)
            {
                for (ix = 0; ix < xGrid; ix++)
                {
                    id = iy * xGrid + ix;
                    if ((!point_in[id] && keepinside) ||
                         (point_in[id] && !keepinside))
                    {
                        for (iz = height1; iz <= height2; iz++)
                        {
                            grid.SetBlankGrid(ix, iy, iz);
                        }
                    }
                }
                Percentage += step;
            }

            progressTitle = "calculating intersections...";
            //3 calculate and set intersections
            z1 = z2 = maxz;
            for (iy = 0; iy < yGrid; iy++)
            {
                y1 = miny + iy * ystep;
                for (ix = 0; ix < xGrid; ix++)
                {
                    x1 = minx + ix * xstep;
                    id = iy * xGrid + ix;//x1y1

                    if ( ix < xGrid - 1 && point_in[id] != point_in[id + 1]) //x1 - x2
                    {
                        x2 = x1 + xstep;                        
                        p1 = new Vector64(x1, y1, z1);
                        p2 = new Vector64(x2, y1, z1);
                        CLine line = new CLine(p1, p2);
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if (inters.Length > 0)
                        {
                            for (iz = height1; iz <= height2 ; iz++)
                            {
                                id3 = iz * xGrid*yGrid + iy * xGrid + ix;//x1y1
                                if (grid.pBlankTable[id3]) //0<---inertp-----1
                                    grid.SetBlankValue(id3, inters[0].X, 0, 1);
                                else //0---inertp---->1
                                    grid.SetBlankValue(id3, inters[0].X, 0, 2);
                            }
                        }
                    }//if (point_in[id] != point_in[id1]) //x1-x2

                    if ( iy < yGrid - 1 && point_in[id] != point_in[id + xGrid] ) //y1-y2
                    {
                        y2 = y1 + ystep;
                        p1 = new Vector64(x1, y1, z1);
                        p2 = new Vector64(x1, y2, z1);
                        CLine line = new CLine(p1, p2);
                        //get intersect points
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if (inters.Length > 0)
                        {   //cut and set intersetions
                            for (iz = height1; iz <= height2 ; iz++)
                            {
                                id3 = iz * xGrid * yGrid + iy * xGrid + ix;//x1y1
                                if (grid.pBlankTable[id3]) //0<---inertp-----1
                                    grid.SetBlankValue(id3, inters[0].Y, 1, 1);
                                else //0---inertp---->1
                                    grid.SetBlankValue(id3, inters[0].Y, 1, 2);
                            }
                        }
                    }//if (point_in[id] != point_in[id2]) //y1-y2
                }//for (ix = 0; ix < xNum - 1; ix++)

                Percentage += step;
                //if (iy % step == 0)
                {
                    TimeLeft = (yGrid - iy - 1) * sec;
                    Percentage += step;
                }
                Percentage += step;
            }//for (iy = 0; iy < yNum - 1; iy++)

            point_in = null;

            return true;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="slicer"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        ///     z
        ///     |
        ///     |________y
        ///    /
        ///  x
        private bool TrimWithYOZSlicer(C3DGridData grid, CSlicer slicer, int para = 0)
        {
            int ix, iy, iz, id, id3;
            Vector64 p1, p2;
            Vector64 p = new Vector64();

            bool keepleft = true;
            if (para == 1) keepleft = false;
            bool keepinside = !keepleft;

            Polygon2D polygon;
            if (slicer.Closed)
            {
                polygon = new Polygon2D(slicer.toXYPoints(slicer.pBaseLine));
                if (para == 0) keepinside = true;
                else if (para == 1) keepinside = false;
            }
            else
            {
                //线与边界求交，返回相交多边形（顺时针方向）
                polygon = slicer.RectIntersect(minx, miny, minz, maxx, maxy, maxz);
                if (polygon == null) 
                {
                    errMsg = "failed.";
                    return false; 
                }
            }

            bool[] point_in = new bool[yGrid * zGrid];

            double x1, x2, y1, y2, z1, z2;
            int height1 = (int)((slicer.minHeight - minx) / xstep);
            int height2 = (int)((slicer.maxHeight - minx) / xstep);
            if (slicer.maxHeight - height2 * xstep > minx) height2++;
            if (height1 < 0) height1 = 0;
            if (height2 > xGrid - 1) height2 = xGrid - 1;

            Percentage = 0;
            double step = 100 / (3 * zGrid);

            progressTitle = "calculating...";
            //1 check in or out
            for (iz = 0; iz < zGrid; iz++)
            {
                for (iy = 0; iy < yGrid; iy++)
                {
                    p.X = miny + iy * ystep;
                    p.Y = minz + iz * zstep;
                    p.Z = maxx;
                    id = iz * yGrid + iy;
                    point_in[id] = polygon.IsPointInsidePoly(p);
                }
                Percentage += step;
            }

            //2 blank grid
            for (iz = 0; iz < zGrid; iz++)
            {
                for (iy = 0; iy < yGrid; iy++)
                {
                    id = iz * yGrid + iy;
                    if (( !point_in[id] && keepinside) ||
                          (point_in[id] && !keepinside))
                    {
                        for (ix = height1; ix <= height2; ix++)
                        {
                            grid.SetBlankGrid(ix, iy, iz);
                        }
                    }
                }
                Percentage += step;
            }
            
            //3 calculate and set intersections
            x1 = x2 = maxx;
            for (iz = 0; iz < zGrid; iz++)
            {
                z1 = minz + iz * zstep;
                for (iy = 0; iy < yGrid; iy++)
                {
                    y1 = miny + iy * ystep;
                    id = iz * yGrid + iy; //y1z1
                    
                    if ( iy < yGrid - 1 && point_in[id] != point_in[id+1]) //y1---y2
                    {
                        y2 = y1 + ystep;
                        p1 = new Vector64(y1, z1, x1);
                        p2 = new Vector64(y2, z1, x1);
                        CLine line = new CLine(p1, p2);
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if ( inters.Length > 0 )
                        {
                            for (ix = height1; ix <= height2; ix++)
                            {
                                id3 = iz * xGrid * yGrid + iy * xGrid + ix;
                                if ( grid.pBlankTable[id3] ) //0<---inertp-----1(blanked)
                                    grid.SetBlankValue(id3, inters[0].X, 1, 1);
                                else //0(blanked)---inertp---->1
                                    grid.SetBlankValue(id3, inters[0].X, 1, 2);
                            }
                        }
                    }//if (point_in[id] != point_in[id1]) //y1 -- y2

                    if ( iz < zGrid - 1 && point_in[id] != point_in[ id + yGrid ] ) //z1-----z2
                    {
                        z2 = z1 + zstep;
                        p1 = new Vector64(y1, z1, x1);
                        p2 = new Vector64(y1, z2, x1);
                        CLine line = new CLine(p1, p2);
                        //get intersect points
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if (inters.Length > 0)
                        {   //cut and set intersetions
                            for (ix = height1; ix <= height2; ix++)
                            {
                                id3 = iz * xGrid * yGrid + iy * xGrid + ix;
                                if (grid.pBlankTable[id3]) //0<---inertp-----1
                                    grid.SetBlankValue(id3, inters[0].Y, 2, 1);
                                else //0---inertp---->1
                                    grid.SetBlankValue(id3, inters[0].Y, 2, 2);
                            }
                        }
                    }//if (point_in[id] != point_in[id2]) //y1-y2
                }//for (iy = 0; iy < yGrid-1; iy++)
                Percentage += step;
            }//for (iz = 0; iz < zGrid-1; iz++)

            point_in = null;

            return true;            
        }
        ///      z    XOZ slicer
        //       1-----/1----2-  
        //       |    /      |
        //     0 |   /       |2
        //   x<--0--/--3-----3
        private bool TrimWithXOZSlicer(C3DGridData grid, CSlicer slicer, int para = 0)
        {
            int ix, iy, iz, id, id3;
            Vector64 p1, p2;
            Vector64 p = new Vector64();

            bool keepleft = true;
            if (para == 1) keepleft = false;
            bool keepinside = !keepleft;

            Polygon2D polygon;
            if (slicer.Closed)
            {
                polygon = new Polygon2D(slicer.toXYPoints(slicer.pBaseLine));
                if (para == 0) keepinside = true;
                else if (para == 1) keepinside = false;
            }
            else
            {
                //线与边界求交，返回相交多边形（顺时针方向）
                polygon = slicer.RectIntersect(minx, miny, minz, maxx, maxy, maxz);
                if (polygon == null)
                {
                    errMsg = "failed.";
                    return false;
                }
            }

            bool[] point_in = new bool[xGrid * zGrid];

            double x1, x2, y1, y2, z1, z2;
            int height1 = (int)((slicer.minHeight - miny) / ystep);
            int height2 = (int)((slicer.maxHeight - miny) / ystep);
            if (slicer.maxHeight - height2 * ystep > miny) height2++;
            if (height1 < 0) height1 = 0;
            if (height2 > yGrid - 1) height2 = yGrid - 1;

            Percentage = 0;
            double step = 100 / (3 * zGrid);

            progressTitle = "calculating...";
            //1 check in or out
            for (iz = 0; iz < zGrid; iz++)
            {
                for (ix = 0; ix < xGrid; ix++)
                {
                    p.X = minx + ix * xstep;
                    p.Y = minz + iz * zstep;
                    p.Z = maxy;
                    id = iz * xGrid + ix;
                    point_in[id] = polygon.IsPointInsidePoly(p);
                }
                Percentage += step;
            }

            //2 blank grid
            for (iz = 0; iz < zGrid; iz++)
            {
                for (ix = 0; ix < xGrid; ix++)
                {
                    id = iz * xGrid + ix;
                    if ( (!point_in[id] && keepinside) ||
                          (point_in[id] && !keepinside))
                    {
                        for (iy = height1; iy <= height2; iy++)
                        {
                            grid.SetBlankGrid(ix, iy, iz);
                        }
                    }
                }
                Percentage += step;
            }

            //3 calculate and set intersections
            y1 = y2 = maxy;
            for (iz = 0; iz < zGrid; iz++)
            {
                z1 = minz + iz * zstep;
                for (ix = 0; ix < xGrid; ix++)
                {
                    x1 = minx + ix * xstep;
                    id = iz * xGrid + ix;

                    if (ix < xGrid - 1 && point_in[id] != point_in[id + 1]) //x1---x2
                    {
                        x2 = x1 + xstep;
                        p1 = new Vector64(x1, z1, y1);
                        p2 = new Vector64(x2, z1, y1);
                        CLine line = new CLine(p1, p2);
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if (inters.Length > 0)
                        {
                            for (iy = height1; iy <= height2; iy++)
                            {
                                id3 = iz * xGrid * yGrid + iy * xGrid + ix;
                                if (grid.pBlankTable[id3]) //0<---inertp-----1
                                    grid.SetBlankValue(id3, inters[0].X, 0, 1);
                                else //0---inertp---->1
                                    grid.SetBlankValue(id3, inters[0].X, 0, 2);
                            }
                        }
                    }//if (point_in[id] != point_in[id1]) //x1 -- x2

                    if (iz < zGrid - 1 && point_in[id] != point_in[id + xGrid]) //z1-----z2
                    {
                        z2 = z1 + zstep;
                        p1 = new Vector64(x1, z1, y1);
                        p2 = new Vector64(x1, z2, y1);
                        CLine line = new CLine(p1, p2);
                        //get intersect points
                        Vector64[] inters = polygon.GetIntersectPoints(line);
                        if (inters.Length > 0)
                        {   //cut and set intersetions
                            for (iy = height1; iy <= height2; iy++)
                            {
                                id3 = iz * xGrid * yGrid + iy * xGrid + ix;
                                if (grid.pBlankTable[id3]) //0<---inertp-----1
                                    grid.SetBlankValue(id3, inters[0].Y, 2, 1);
                                else //0---inertp---->1
                                    grid.SetBlankValue(id3, inters[0].Y, 2, 2);
                            }
                        }
                    }//if (point_in[id] != point_in[id2]) //z1---z2
                }//for (ix = 0; ix < xGrid; ix++)
                Percentage += step;
            }//for (iz = 0; iz < zGrid-1; iz++)

            point_in = null;

            return true;
        }
        /////////////////////////
        /// grid face direct
        /// 0------>1
        /// 0------>1
        /// 0------>1
        /// 2------>3
        /// <summary>
        /// 用mesh切割三维网格
        /// </summary>
        /// <param name="grid3d"></param>
        /// <param name="mesh"></param>
        /// <param name="para">
        /// 0---keep left, mesh face
        /// 1---keep right, mesh face
        /// 2---keep both, blank intersetion
        /// </param>
        /// <returns>true successed</returns>
        public bool TrimWithGridMesh(C3DGridData grid3d, CMesh mesh, int para = 0)
        {
            /*
            startTime = DateTime.Now;
            progressTitle = "Calculating blanked grids...";
            try
            {
                int ix, iy, iz, id, id3;
                Vector64 p1, p2;
                Vector64 p = new Vector64();

                bool keepleft = true;
                if (para == 1) keepleft = false;
                bool keepinside = !keepleft;

                Polygon2D polygon;
                if (slicer.Closed)
                {
                    polygon = new Polygon2D(slicer.toXYPoints(slicer.pBaseLine));
                    if (para == 0) keepinside = true;
                    else if (para == 1) keepinside = false;
                }
                else
                {
                    //线与边界求交，返回相交多边形（顺时针方向）
                    polygon = slicer.RectIntersect(minx, miny, minz, maxx, maxy, maxz);
                    if (polygon == null)
                    {
                        errMsg = "failed.";
                        return false;
                    }
                }

                bool[] point_in = new bool[xGrid * yGrid];

                double x1, x2, y1, y2, z1, z2;
                int height1 = (int)((slicer.minHeight - minz) / zstep);
                int height2 = (int)((slicer.maxHeight - minz) / zstep);
                if (slicer.maxHeight - height2 * zstep > minz) height2++;
                if (height1 < 0) height1 = 0;
                if (height2 > zGrid - 1) height2 = zGrid - 1;

                Percentage = 0;
                double step = 100 / (2 * yGrid);
                double sec = 0;
                progressTitle = "calculating blanked grids...";

                DateTime t1 = DateTime.Now;

                //1 check in or out
                for (iy = 0; iy < yGrid; iy++)
                {
                    for (ix = 0; ix < xGrid; ix++)
                    {
                        p.X = minx + ix * xstep;
                        p.Y = miny + iy * ystep;
                        p.Z = maxz;
                        id = iy * xGrid + ix;
                        point_in[id] = polygon.IsPointInsidePoly(p);
                    }
                    if (iy == 0)
                    {
                        sec = (DateTime.Now - t1).TotalSeconds;
                    }
                    //if (iy % step == 0)
                    {
                        TimeLeft = (2 * yGrid - iy - 1) * sec;
                        Percentage += step;
                    }
                    Percentage += step;
                }

                //2 blank grid
                for (iy = 0; iy < yGrid; iy++)
                {
                    for (ix = 0; ix < xGrid; ix++)
                    {
                        id = iy * xGrid + ix;
                        if ((!point_in[id] && keepinside) ||
                             (point_in[id] && !keepinside))
                        {
                            for (iz = height1; iz <= height2; iz++)
                            {
                                grid.SetBlankGrid(ix, iy, iz);
                            }
                        }
                    }
                    Percentage += step;
                }

                progressTitle = "calculating intersections...";
                //3 calculate and set intersections
                z1 = z2 = maxz;
                for (iy = 0; iy < yGrid; iy++)
                {
                    y1 = miny + iy * ystep;
                    for (ix = 0; ix < xGrid; ix++)
                    {
                        x1 = minx + ix * xstep;
                        id = iy * xGrid + ix;//x1y1

                        if (ix < xGrid - 1 && point_in[id] != point_in[id + 1]) //x1 - x2
                        {
                            x2 = x1 + xstep;
                            p1 = new Vector64(x1, y1, z1);
                            p2 = new Vector64(x2, y1, z1);
                            CLine line = new CLine(p1, p2);
                            Vector64[] inters = polygon.GetIntersectPoints(line);
                            if (inters.Length > 0)
                            {
                                for (iz = height1; iz <= height2; iz++)
                                {
                                    id3 = iz * xGrid * yGrid + iy * xGrid + ix;//x1y1
                                    if (grid.pBlankTable[id3]) //0<---inertp-----1
                                        grid.SetBlankValue(id3, inters[0].X, 0, 2);
                                    else //0---inertp---->1
                                        grid.SetBlankValue(id3, inters[0].X, 0, 1);
                                }
                            }
                        }//if (point_in[id] != point_in[id1]) //x1-x2

                        if (iy < yGrid - 1 && point_in[id] != point_in[id + xGrid]) //y1-y2
                        {
                            y2 = y1 + ystep;
                            p1 = new Vector64(x1, y1, z1);
                            p2 = new Vector64(x1, y2, z1);
                            CLine line = new CLine(p1, p2);
                            //get intersect points
                            Vector64[] inters = polygon.GetIntersectPoints(line);
                            if (inters.Length > 0)
                            {   //cut and set intersetions
                                for (iz = height1; iz <= height2; iz++)
                                {
                                    id3 = iz * xGrid * yGrid + iy * xGrid + ix;//x1y1
                                    if (grid.pBlankTable[id3]) //0<---inertp-----1
                                        grid.SetBlankValue(id3, inters[0].Y, 1, 2);
                                    else //0---inertp---->1
                                        grid.SetBlankValue(id3, inters[0].Y, 1, 1);
                                }
                            }
                        }//if (point_in[id] != point_in[id2]) //y1-y2
                    }//for (ix = 0; ix < xNum - 1; ix++)

                    Percentage += step;
                    //if (iy % step == 0)
                    {
                        TimeLeft = (yGrid - iy - 1) * sec;
                        Percentage += step;
                    }
                    Percentage += step;
                }//for (iy = 0; iy < yNum - 1; iy++)

                point_in = null;

                return true;
            }
            catch (Exception e)
            {
                errMsg = "Failed.\n" + e.Message;
                return false;
            }
            */
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid3d"></param>
        /// <param name="slicer"></param>
        /// <param name="para">
        /// keep inside 0 closed slicer
        /// keep outside 1 closed slicer
        /// keep left 0     non-closed slicer
        /// keep right 1    non-closed slicer
        /// keep on line 2  non-closed slicer      
        /// </param>
        /// <returns></returns>
        public bool TrimWithSlicer(C3DGridData grid3d, CSlicer slicer, int para = 0)
        {
            int n = slicer.pBaseLine.Count;
            if (n < 2) return false;

            startTime = DateTime.Now;
            progressTitle = "Calculating blanked grids...";
            try
            {
                xGrid = grid3d.xNum;
                yGrid = grid3d.yNum;
                zGrid = grid3d.zNum;
                minx = grid3d.Minx;
                miny = grid3d.Miny;
                minz = grid3d.Minz;
                maxx = grid3d.Maxx;
                maxy = grid3d.Maxy;
                maxz = grid3d.Maxz;
                
                xstep = (grid3d.Maxx - grid3d.Minx) / (xGrid - 1);
                ystep = (grid3d.Maxy - grid3d.Miny) / (yGrid - 1);
                zstep = (grid3d.Maxz - grid3d.Minz) / (zGrid - 1);                 
                
                if (slicer.Plan == planEnum.XOY)
                {
                    return TrimWithXOYSlicer(grid3d, slicer, para);
                }
                else if (slicer.Plan == planEnum.YOZ)
                {
                    return TrimWithYOZSlicer(grid3d, slicer, para);
                }
                else //if (slicer.Plan == planEnum.XOZ)
                {
                    return TrimWithXOZSlicer(grid3d, slicer, para);
                }
            }
            catch (Exception e)
            {
                errMsg = "Failed.\n" + e.Message;
                return false;
            }
        }

        public bool CutWithPolygon(C3DGridData grid3d, Polygon3D polygon, bool keepOuter,Device[] devices)
        {
            bool[] point_in;
            
            if( devices == null ) point_in = CalculateBlanks(grid3d, polygon);
            else point_in = CalculateBlanks(grid3d, polygon,devices);

            if (point_in == null) return false;

            for (int i = 0; i < point_in.Length; i++)
            {
                if ((keepOuter && point_in[i]) ||
                     (!keepOuter && !point_in[i]))
                {
                    grid3d.pBlankTable[i] = true;
                    grid3d.pgridShowTable[i] = 0;
                }
            }

            CalculateIntersetion(grid3d, polygon, point_in);

            point_in = null;

            return true;
        }
        private bool[] CalculateBlanks(C3DGridData grid3d, Polygon3D obj)
        {
            progressTitle = "Calculating blanked grids...";
            try
            {
                xGrid = grid3d.xNum;
                yGrid = grid3d.yNum;
                zGrid = grid3d.zNum;
                minx = grid3d.Minx;
                miny = grid3d.Miny;
                minz = grid3d.Minz;
                maxx = grid3d.Maxx;
                maxy = grid3d.Maxy;
                maxz = grid3d.Maxz;
                pointsNum = obj.points.Count;
                triNum = obj.triangles.Count;
                xstep = (grid3d.Maxx - grid3d.Minx) / (xGrid - 1);
                ystep = (grid3d.Maxy - grid3d.Miny) / (yGrid - 1);
                zstep = (grid3d.Maxz - grid3d.Minz) / (zGrid - 1);
                blankedGrids = new bool[xGrid * yGrid * zGrid];
                double x, y, z;

                double step = 100 / zGrid;
                Percentage = 0;
                double sec = 0;
                
                startTime = DateTime.Now;
                DateTime t1 = startTime;
                DateTime t2;
                for (int iz = 0; iz < zGrid; iz++)
                {
                    z = minz + zstep * iz;
                    for (int iy = 0; iy < yGrid; iy++)
                    {
                        y = miny + ystep * iy;
                        for (int ix = 0; ix < xGrid; ix++)
                        {
                            x = minx + xstep * ix;
                            if (obj.IsPointInPolygon(new Vector32(x, y, z)))
                                blankedGrids[ix + iy * xGrid + iz * xGrid * yGrid] = true;
                            else blankedGrids[ix + iy * xGrid + iz * xGrid * yGrid] = false;
                        }
                    }
                    //if (iz == 0)
                    {
                        t2 = DateTime.Now;
                        sec = (t2 - t1).TotalSeconds;
                        t1 = t2;
                    }
                    //if (iz % step == 0)
                    {
                        TimeLeft = (zGrid - iz - 1) * sec;
                        Percentage += step;
                    }
                }
                return blankedGrids;
            }
            catch (Exception e)
            {
                errMsg = "Failed.\n" + e.Message;
                progressTitle = errMsg;
                return null;
            }

        }
        //get edge intersections
        private bool CalculateIntersetion(C3DGridData grid3d, Polygon3D polygon,bool[]point_in)
        {
            long id = 0, id1, id3, id4;
            int ix, iy, iz;

            vec3 sect = new vec3();
            bool b0, b1, b3, b4;
            Vector32 p0, p, p1, p3, p4;

            double sec = 0;
            Percentage = 0;
            double step = 100 / zGrid;
            DateTime t1 = DateTime.Now;
            progressTitle = "calculating intersections ...";
            for (iz = 0; iz < zGrid; iz++)
            {
                for (iy = 0; iy < yGrid; iy++)
                {
                    for (ix = 0; ix < xGrid; ix++)
                    {
                        sect.x = (float)(minx - 2);
                        sect.y = (float)(miny - 2);
                        sect.z = (float)(minz - 2);

                        if (ix < xGrid - 1)
                        {
                            id = grid3d.GetVerticIndex(ix, iy, iz, 0);
                            id1 = grid3d.GetVerticIndex(ix, iy, iz, 1);
                            b0 = point_in[id];
                            b1 = point_in[id1];
                            if (b0 != b1)    //0-1  x axis
                            {
                                p0 = grid3d.GetVerticCoord(ix, iy, iz, 0);
                                p1 = grid3d.GetVerticCoord(ix, iy, iz, 1);
                                if (polygon.GetIntersectionOnAxis(p0, p1, b0, b1, out p, 0))
                                {
                                    if (grid3d.pBlankTable[id]) //0<---inertp-----1
                                        grid3d.SetBlankValue(id, p.X, 0, 1);
                                    else //0---inertp---->1
                                        grid3d.SetBlankValue(id, p.X, 0, 2);
                                }//if (polygon.GetIntersectionOnAxis(p0, p1, b0, b1, out p, 0))
                            }// if (b0 != b1)    //0-1  x axis
                        }//if (ix < xNum - 1)
                        if (iy < yGrid - 1)
                        {
                            id = grid3d.GetVerticIndex(ix, iy, iz, 0);
                            id4 = grid3d.GetVerticIndex(ix, iy, iz, 4);
                            b0 = point_in[id];
                            b4 = point_in[id4];
                            if (b0 != b4)  //0-4  y axis
                            {
                                p0 = grid3d.GetVerticCoord(ix, iy, iz, 0);
                                p4 = grid3d.GetVerticCoord(ix, iy, iz, 4);
                                if (polygon.GetIntersectionOnAxis(p0, p4, b0, b4, out p, 1))
                                {
                                    if (grid3d.pBlankTable[id])//0<---inertp-----4
                                        grid3d.SetBlankValue(id, p.Y, 1, 1);
                                    else //0---inertp---->4
                                        grid3d.SetBlankValue(id, p.Y, 1, 2);
                                }
                            }
                        }
                        if (iz < zGrid - 1)
                        {
                            id = grid3d.GetVerticIndex(ix, iy, iz, 0);
                            id3 = grid3d.GetVerticIndex(ix, iy, iz, 3);
                            b0 = point_in[id];
                            b3 = point_in[id3];
                            if (b0 != b3)  //0-3  z axis
                            {
                                p0 = grid3d.GetVerticCoord(ix, iy, iz, 0);
                                p3 = grid3d.GetVerticCoord(ix, iy, iz, 3);
                                if (polygon.GetIntersectionOnAxis(p0, p3, b0, b3, out p, 2))
                                {
                                    if (grid3d.pBlankTable[id])//0<---inertp-----3
                                        grid3d.SetBlankValue(id, p.Z, 2, 1);
                                    else //0---inertp---->3
                                        grid3d.SetBlankValue(id, p.Z, 2, 2);
                                }
                            }
                        }
                    }//for(ix = 0;ix<xGrid;ix++)
                }//for(iy = 0;iy<yGrid;iy++)

                if (iz == 0)
                {
                    sec = (DateTime.Now - t1).TotalSeconds;
                }
                //if (iz % step == 0)
                {
                    TimeLeft = (zGrid - iz - 1) * sec;
                    Percentage += step;
                }
            }//for(iz = 0;iz<zGrid;iz++)
            
            return true;
        }

       
        private bool[] CalculateBlanks(C3DGridData grid3d, TriangleObj obj, Device[]devices )
        {       
            if (devices == null)
            {
                errMsg = "请添加计算设备！";
                return null;
            }

            if (progressStep < 1) Percentage = 0;

            startTime = DateTime.Now;
            progressTitle = "正在计算所需内存...";

            xGrid = grid3d.xNum;
            yGrid = grid3d.yNum;
            zGrid = grid3d.zNum;
            minx = grid3d.Minx;
            miny = grid3d.Miny;
            minz = grid3d.Minz;
            maxx = grid3d.Maxx;
            maxy = grid3d.Maxy;
            maxz = grid3d.Maxz;
            pointsNum = obj.points.Count;
            triNum = obj.triangles.Count;
            xstep = (grid3d.Maxx - grid3d.Minx) / (xGrid - 1);
            ystep = (grid3d.Maxy - grid3d.Miny) / (yGrid - 1);
            zstep = (grid3d.Maxz - grid3d.Minz) / (zGrid - 1);
            ulong requireMemorySize = GetRequiredMemorySize(obj);
            //double total = obj.GetCpuTotalMemory();

            ulong cpuAvailableMemorySize = PhysicalMemory.GetAvailableMemorySize();
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
            if (!CompileDevice( clSource ))
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "正在分配内存...";
            if (!CreateMemory( obj ))
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "正在计算...";
            if (!StartGridTrim(grid3d,obj))
            {
                Clear();
                ReleaseDevices();
                progressTitle = "失败，进程终止！";
                return null;
            }
            progressTitle = "finished!";
            return blankedGrids;
        }
        
        private bool StartGridTrim(C3DGridData grid3d, TriangleObj obj)
        {
            progressTitle = "Trim grids by the polygon...";
            Percentage = 0;

            int batch = 1000;
            long total = grid3d.xNum * grid3d.yNum * grid3d.zNum;
            startTime = DateTime.Now;
            DateTime t0 = startTime;
            DateTime t1 = t0;
            DateTime t2 = t0;

            try
            {
                Percentage = 0;

                CreateTaskPools(total, batch);

                //启动计算线程
                Thread[] threads = new Thread[clDevices.Length];
                for (int i = 0; i < threads.Length; i++)
                {
                    clDevices[i].Stop = false;
                    clDevices[i].blankedGrids = new bool[batch];
                    clDevices[i].SetWorkItemSize(batch);

                    threads[i] = new Thread(PolygonTrimThread);
                    threads[i].Start(clDevices[i]);
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
                    threads[i] = null;
                }
                threads = null;
                ReleaseDevices();

                return true;
            }
            catch (Exception e)
            {
                errMsg = "grids triming failed.\n" + e.Message;
                return false;
            }

        }
        private void PolygonTrimThread(Object para)
        {
            int taskCount = taskPools.Count;
            double step = taskCount / 100.0;
            int nstep = (int)step;
            if (nstep == 0) nstep = 1;

            #region 线程配置------------------
            PolygonTrimUnit clUnit = (PolygonTrimUnit)para;
            int[] workItemSizes = new int[] { (int)clUnit.workItemSize };
            int[] globalWorkItemSizes = new int[] { (int)clUnit.totalWorkSize };
            #endregion 线程配置

            //get first tast
            TaskPoolIndices task = GetFromTaskPools();

            //在显存创建缓冲区并把HOST的数据拷贝过去                                
            var px1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, px.Length * sizeof(double), px.ToDoublePtr());
            var py1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, py.Length * sizeof(double), py.ToDoublePtr());
            var pz1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, pz.Length * sizeof(double), pz.ToDoublePtr());
            var tr1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, triangle1.Length * sizeof(int), triangle1.ToIntPtr());
            var tr2 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, triangle2.Length * sizeof(int), triangle2.ToIntPtr());
            var tr3 = clUnit.oclContext.CreateBuffer(MemFlags.READ_ONLY | MemFlags.COPY_HOST_PTR, triangle3.Length * sizeof(int), triangle3.ToIntPtr());

            while (!clUnit.Stop && !threadStop && task.num > 0)
            {
                int startid = (int)task.start;
                int num = task.num;

                workItemSizes = new int[] { num };
                globalWorkItemSizes = new int[] { num };

                //网格数组重置
                for (int i = 0; i < num; i++) clUnit.blankedGrids[i] = false;
                var grid1 = clUnit.oclContext.CreateBuffer(MemFlags.READ_WRITE | MemFlags.COPY_HOST_PTR, clUnit.blankedGrids.Length * sizeof(bool), clUnit.blankedGrids.ToBoolPtr());
                
                int k = 0;
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, startid);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, num);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, pointsNum);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, triNum);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, xGrid);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, yGrid);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, zGrid);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, xstep);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, ystep);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, zstep);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, minx);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, miny);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, minz);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, maxx);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, maxy);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, maxz);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, px1);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, py1);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, pz1);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, tr1);                
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, tr2);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, tr3);
                clUnit.Kernels["gridsBlankTest"].SetArg(k++, grid1);

                clUnit.oclCQ.EnqueueNDRangeKernel(clUnit.Kernels["gridsBlankTest"], 1, new[] { 0 }, globalWorkItemSizes, workItemSizes);
                //设置栅栏强制要求上面的命令执行完才继续下面的命令.
                clUnit.oclCQ.EnqueueBarrier();
                clUnit.oclCQ.EnqueueReadBuffer(grid1, true, 0, clUnit.blankedGrids.Length * sizeof(bool), clUnit.blankedGrids.ToBoolPtr());
                clUnit.oclCQ.Finish();

                //拷贝到grid3d数组对应位置
                lock(lock2)
                {
                    Array.Copy(clUnit.blankedGrids, 0, blankedGrids, startid, num);
                }                

                //下一任务
                task = GetFromTaskPools();
                Percentage = (1.0 - (double)taskPools.Count / (double)taskCount) * 100;

                grid1.Dispose();

            }//while (!clUnit.Stop && !threadStoped)
            tr1.Dispose();
            tr2.Dispose();
            tr3.Dispose();
            px1.Dispose();
            py1.Dispose();
            pz1.Dispose();

            
            clUnit.ReleaseSiginal();//线程结束信号
        }
        ////原子操作LOCK/UNLOCK很慢
        /****
         * startid      三维网格节点开始位置
           idnum,       三维网格节点数开始位置
           np,          Polygon三角形顶点数
           ntri,        Polygon三角形数
           px,py,pz,    Polygon三角形顶点坐标
           tr1,tr2,tr3, Polygon三角形索引数组
           blanks,      返回结果数组是否blanked,数组长度idnum
           nx,          网格xGrid
           xy,          网格xGrid * yGrid
           线程数 >= idnum, 线程号 = id序号
         ****/
        private string clSource =
            @"#pragma OPENCL EXTENSION cl_khr_fp64 : enable   
            #pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
            #define LOCK(a) atom_cmpxchg(a, 0, 1)
            #define UNLOCK(a) atom_xchg(a, 0)
            int CheckLineCrossTriangle( double3 v1,double3 v2,double3 p1,double3 p2,double3 p3,int direct );
            double3 Cross(double3 v1,double3 v2);
            double Dot( double3 v1,double3 v2);
            bool IsPointOnLine( double3 p, double3 v1,double3 v2 );
            bool IsPointOnSectionLine( double3 p,double3 v1,double3 v2);
            bool GetIntersectionOnTriangle(double3 v1, double3 v2, double3 p1,double3 p2,double3 p3,double3 intersect);

            __kernel void gridsBlankTest(   int startid,
                                            int idnum,
                                            int np,
                                            int ntri, 
                                            int nx, 
                                            int ny, 
                                            int nz, 
                                            double xstep, 
                                            double ystep, 
                                            double zstep, 
                                            double minx, 
                                            double miny, 
                                            double minz, 
                                            double maxx, 
                                            double maxy, 
                                            double maxz,
                                            __global double * px, 
                                            __global double * py, 
                                            __global double * pz,
                                            __global int * tr1,
                                            __global int * tr2,
                                            __global int * tr3,
                                            __global bool * blanks )
                                        
        {
            int id = get_global_id(0)+ startid;
            if( id >= nx * ny * nz ) return;
           
            int ix,iy,iz,md;
            double x,y,z;
            int xy = nx * ny;

            iz = (int)( id / xy );
            md = id - iz * xy;
            iy = md / nx;
            ix = md % nx;
            
            x =  ix * xstep;
            y =  iy * ystep;
            z =  iz * zstep; 
            
            if (x < minx || x > maxx) { blanks[ id - startid] = false; return; }
            if (y < miny || y > maxy) { blanks[ id - startid] = false; return; }
            if (z < minz || z > maxz) { blanks[ id - startid] = false; return; }
           
            int j,count,ret;
            double3 v1,v2;
            double3 p1,p2,p3;

            v1.x = x; 
            v1.y = y; 
            v1.z = z;
            
            v2.x = minx - 1; 
            v2.y = y; 
            v2.z = z;
            count = 0;
            for( j = 0; j < ntri; j++ )
            {   
                p1.x = px[tr1[j]];
                p1.y = py[tr1[j]];
                p1.z = pz[tr1[j]];
                p2.x = px[tr2[j]];
                p2.y = py[tr2[j]];
                p2.z = pz[tr2[j]];
                p3.x = px[tr3[j]];
                p3.y = py[tr3[j]];
                p3.z = pz[tr3[j]];
                ret = CheckLineCrossTriangle(v1,v2,p1,p2,p3,0);
                if (ret == 0) continue;
                else if (ret == 1) count++;
                else { count = 0; break; }
            }
            if( count > 0 )
            {
                if( count % 2 == 0 )blanks[ id - startid] = false;
                else blanks[ id - startid] = true;
                return;
            }

            v2.x = maxx + 1; 
            v2.y = y; 
            v2.z = z;
            count = 0;
            for( j = 0; j < ntri; j++ )
            {   
                p1.x = px[tr1[j]];
                p1.y = py[tr1[j]];
                p1.z = pz[tr1[j]];
                p2.x = px[tr2[j]];
                p2.y = py[tr2[j]];
                p2.z = pz[tr2[j]];
                p3.x = px[tr3[j]];
                p3.y = py[tr3[j]];
                p3.z = pz[tr3[j]];
                ret = CheckLineCrossTriangle(v1,v2,p1,p2,p3,0);
                if (ret == 0) continue;
                else if (ret == 1) count++;
                else { count = 0; break; }
            }
            if( count > 0 )
            {
                if( count % 2 == 0 )blanks[ id - startid] = false;
                else blanks[ id - startid] = true;
                return;
            }

            v2.x = x; 
            v2.y = maxy + 1; 
            v2.z = z;
            count = 0;
            for( j = 0; j < ntri; j++ )
            {   
                p1.x = px[tr1[j]];
                p1.y = py[tr1[j]];
                p1.z = pz[tr1[j]];
                p2.x = px[tr2[j]];
                p2.y = py[tr2[j]];
                p2.z = pz[tr2[j]];
                p3.x = px[tr3[j]];
                p3.y = py[tr3[j]];
                p3.z = pz[tr3[j]];
                ret = CheckLineCrossTriangle(v1,v2,p1,p2,p3,1);
                if (ret == 0) continue;
                else if (ret == 1) count++;
                else { count = 0; break; }
            }
            if( count > 0 )
            {
                if( count % 2 == 0 )blanks[ id - startid] = false;
                else blanks[ id - startid] = true;
                return;
            }

            v2.x = x; 
            v2.y = y; 
            v2.z = minz - 1;
            count = 0;
            for( j = 0; j < ntri; j++ )
            {   
                p1.x = px[tr1[j]];
                p1.y = py[tr1[j]];
                p1.z = pz[tr1[j]];
                p2.x = px[tr2[j]];
                p2.y = py[tr2[j]];
                p2.z = pz[tr2[j]];
                p3.x = px[tr3[j]];
                p3.y = py[tr3[j]];
                p3.z = pz[tr3[j]];
                ret = CheckLineCrossTriangle(v1,v2,p1,p2,p3,2);
                if (ret == 0) continue;
                else if (ret == 1) count++;
                else { count = 0; break; }
            }
            if( count > 0 )
            {
                if( count % 2 == 0 )blanks[ id - startid] = false;
                else blanks[ id - startid] = true;
                return;
            }

            v2.x = x; 
            v2.y = y; 
            v2.z = maxz + 1;
            count = 0;
            for( j = 0; j < ntri; j++ )
            {   
                p1.x = px[tr1[j]];
                p1.y = py[tr1[j]];
                p1.z = pz[tr1[j]];
                p2.x = px[tr2[j]];
                p2.y = py[tr2[j]];
                p2.z = pz[tr2[j]];
                p3.x = px[tr3[j]];
                p3.y = py[tr3[j]];
                p3.z = pz[tr3[j]];
                ret = CheckLineCrossTriangle(v1,v2,p1,p2,p3,2);
                if (ret == 0) continue;
                else if (ret == 1) count++;
                else { count = 0; break; }
            }
            if( count > 0 )
            {
                if( count % 2 == 0 )blanks[ id - startid] = false;
                else blanks[ id - startid] = true;
                return;
            }
        }
        
        double3 Cross(double3 v1,double3 v2) 
        {
            double3 v;  
            v.x = v1.y * v2.z - v1.z * v2.y;
            v.y = v1.z * v2.z - v1.x * v2.z;
            v.z = v1.x * v2.y - v1.y * v2.x;
            return v;
        }

        double Dot( double3 v1,double3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        
        bool IsPointOnLine( double3 p, double3 v1,double3 v2 )
        {
            double3 v = Cross(v1-p,v2-p);            
            if ( fabs( v.x + v.y + v.z ) <= 1.0E-6 ) return true;   
            else return false;
        }

        bool IsPointOnSectionLine( double3 p,double3 v1,double3 v2)
        {
            if( IsPointOnLine(p,v1,v2) )
            {
                double minx = v1.x;
                double miny = v1.y;
                double minz = v1.z;
                double maxx = v1.x;
                double maxy = v1.y;
                double maxz = v1.z;

                if( v2.x < minx ) minx = v2.x;
                if( v2.y < miny ) miny = v2.y;
                if( v2.z < minz ) minz = v2.z;
                if( v2.x > maxx ) maxx = v2.x;
                if( v2.y > maxy ) maxy = v2.y;
                if( v2.z > maxz ) maxz = v2.z;
                if( p.x >= minx && p.x <= maxx &&
                    p.y >= miny && p.y <= maxy &&
                    p.z >= minz && p.z <= maxz )
                    return true;
                
            }
            return true;
        }
        bool GetIntersectionOnTriangle(double3 v1, double3 v2, double3 p1,double3 p2,double3 p3,double3 intersect)
        {
            double t, u, v;
            double3 orig = v1;
            double3 dir = v2 - v1;            
            double3 E1 = p2 - p1;            
            double3 E2 = p3 - p1;            
            double3 P = Cross(E2,dir);
            double det = Dot(E1,P);
            double3 T;
            if (det > 0)
            {
                T = orig - p1;
            }
            else
            {
                T = p1 - orig;
                det = -det;
            }

            intersect = v1;
            if (det <= 1.0e-6) return false;            
            u = Dot(P,T);
            if (u < 0.0f || u > det) return false;
            double3 Q = Cross(E1,T);
            v = Dot(Q,dir);
            if (v < 0.0f || u + v > det) return false;
            t = Dot(Q,E2);
            double fInvDet = 1.0f / det;
            t *= fInvDet;            
            intersect = orig + (float)t * dir;
            if ( t < 0 || t > 1.0000001 ) return false;
            else  return true;
        }

        int CheckLineCrossTriangle( double3 v1,double3 v2,double3 p1,double3 p2,double3 p3,int direct )
        {
            double minx = p1.x;
            double miny = p1.y;
            double minz = p1.z;
            double maxx = p1.x;
            double maxy = p1.y;
            double maxz = p1.z;

            if( p2.x < minx ) minx = p2.x;
            if( p2.y < miny ) miny = p2.y;
            if( p2.z < minz ) minz = p2.z;
            if( p2.x > maxx ) maxx = p2.x;
            if( p2.y > maxy ) maxy = p2.y;
            if( p2.z > maxz ) maxz = p2.z;
            if( p3.x < minx ) minx = p3.x;
            if( p3.y < miny ) miny = p3.y;
            if( p3.z < minz ) minz = p3.z;
            if( p3.x > maxx ) maxx = p3.x;
            if( p3.y > maxy ) maxy = p3.y;
            if( p3.z > maxz ) maxz = p3.z;

            double3 p0;

            if( direct == 0 )
            {
                if ( v1.x > maxx || v2.x < minx ) return 0;
                if ( v1.y > maxy || v1.y < miny ) return 0;
                if ( v1.z > maxz || v1.z < minz ) return 0;

                if (p1.y == p2.y && p1.z == p2.z)
                {
                   if ( IsPointOnLine( v1,p1,p3 ) )return 2;
                   else return 0;
                }
                if (p2.y == p3.y && p2.z == p3.z)
                {
                    if ( IsPointOnLine( v1,p1,p2 ) )return 2;
                    else return 0;
                }
                if (p1.y == p3.y && p1.z == p3.z)
                {
                    if ( IsPointOnLine( v1,p1,p2 ) )return 2;
                    else return 0;                    
                }
                if ( GetIntersectionOnTriangle( v1,v2,p1,p2,p3,p0 ) )
                        return 1;
            }
            else if( direct == 1 )
            {
                if ( v1.x > maxx || v1.x < minx ) return 0;
                if ( v1.y > maxy || v2.y < miny ) return 0;
                if ( v1.z > maxz || v1.z < minz ) return 0;

                if (p1.x == p2.z && p1.z == p2.z)
                {
                   if ( IsPointOnLine( v1,p1,p3 ) )return 2;
                   else return 0;
                }
                if (p2.x == p3.x && p2.z == p3.z)
                {
                    if ( IsPointOnLine( v1,p1,p2 ) )return 2;
                    else return 0;
                }
                if (p1.x == p3.x && p1.z == p3.z)
                {
                    if ( IsPointOnLine( v1,p1,p2 ) )return 2;
                    else return 0;                    
                }
                if ( GetIntersectionOnTriangle( v1,v2,p1,p2,p3,p0 ) )
                        return 1;
            }   
            else if( direct == 2 )
            {
                if ( v1.x > maxx || v1.x < minx ) return 0;
                if ( v1.y > maxy || v1.y < miny ) return 0;
                if ( v1.z > maxz || v2.z < minz ) return 0;

                if (p1.x == p2.x && p1.y == p2.y)
                {
                   if ( IsPointOnLine( v1,p1,p3 ) )return 2;
                   else return 0;
                }
                if (p2.x == p3.x && p2.y == p3.y)
                {
                    if ( IsPointOnLine( v1,p1,p2 ) )return 2;
                    else return 0;
                }
                if (p1.x == p3.x && p1.y == p3.y)
                {
                    if ( IsPointOnLine( v1,p1,p2 ) )return 2;
                    else return 0;                    
                }
                if ( GetIntersectionOnTriangle( v1,v2,p1,p2,p3,p0 ) )
                        return 1;
            }
               
            return 0;
        }
        "; //end of Source
    }
}
