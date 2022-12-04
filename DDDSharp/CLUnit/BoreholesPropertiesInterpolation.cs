using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using DataCollection;
using OpenCLNet;
namespace CLInterpolation
{
    struct layerGridsStruct
    {
        public double Percent;
        public List<Point> Points;
        public List<float[]> grids;
        public layerGridsStruct(double percent)
        {
            Percent = percent;
            Points = new List<Point>();
            grids = null;
        }
        public void Add(int iborehole,int idepth)
        {
            Points.Add(new Point(iborehole,idepth));
        }
        float[] ToFloatArray(double[] values)
        {
            if (values == null || values.Length == 0) return null;
            float[] values1 = new float[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (double.IsNaN(values[i])) values1[i] = float.NaN;
                else values1[i] = (float)values[i];
            }
            return values1;
        }
        public void CreateInterpolatedGrids(int nx, int ny,
                                            CBoreholes boreholes,
                                            List<string> properties,
                                            double minx,double miny,
                                            double xstep,double ystep)
        {
            InversePower ip = new InversePower();
            int ibh, idepth;
            LasFileData las;
            double x, y;
            double[] values;
            foreach (Point p in Points)
            {
                ibh = p.X;
                idepth = p.Y;
                CBorehole bh = boreholes[ibh];
                las = bh.Curves.lasData;
                x = bh.Position.X;
                y = bh.Position.Y;
                //z = bh.Position.Z - las.GetDepth(idepth);
                values = las.GetPropertyValues(idepth, properties);
                if (values == null) continue;
                ip.AddPoint(x, y, 0, values);
            }

            grids = new List<float[]>();

            for (int k = 0; k < properties.Count; k++)
            {
                for (int i = 0; i < ny; i++)
                {
                    y = miny + i * ystep;
                    for (int j = 0; j < nx; j++)
                    {
                        x = minx + j * xstep;
                        grids.Add(ToFloatArray(ip.GetInterpolatedValues(x, y, 0)));
                    }
                }
            }
        }
    }
    /// <summary>
    /// 基于钻孔的多属性插值
    /// </summary>
    class BoreholesPropertiesInterpolation: InterpolatorBase
    {
        //输入数据
        public List<CMesh> Layers = new List<CMesh>();//平面-地层界面-按Z升序排
        public CBoreholes Boreholes = new CBoreholes();//钻孔数据
        public List<string> Properties = new List<string>();//待插值属性

        //输出数据
        List<float[]> grid3ds = new List<float[]>();


        List<layerGridsStruct>[]meshesGrids = null;

        //待插值属性范围-由UpdatePointsRange获取
        private List<Vector64> PropertiesRanges = new List<Vector64>(); 
        public BoreholesPropertiesInterpolation()
        {
            method = InterpolationMethod.BoreholesPropertiesGridding;
        }
       
        public void AddLayer(CMesh layer) { Layers.Add(layer); }

        public override void Clear()
        {
            base.Clear();
            Layers.Clear();
            grid3ds.Clear();
            Properties.Clear();
            if( meshesGrids != null )
            {
                for(int i=0;i<meshesGrids.Length;i++)                
                {
                    List<layerGridsStruct> grids = meshesGrids[i];
                    foreach(layerGridsStruct g in grids)
                    {
                        g.grids.Clear();
                        g.Points.Clear();
                    }
                    grids.Clear();
                }
                meshesGrids = null;
            }
            
        }
        /// <summary>
        /// 计算两地层最大高差
        /// </summary>
        /// <param name="mesh1">底，网格一样</param>
        /// <param name="mesh2">顶，网格一样</param>
        /// <returns>最大高差</returns>
        double GetMaximumDeltaHight(CMesh mesh1, CMesh mesh2)
        {
            double z1, z2, dz = 0;
            for (int i = 0; i < mesh1.nRow; i++)
            {
                for (int j = 0; j < mesh1.nRow; j++)
                {
                    z1 = mesh1.GetPoint(i, j).Z;
                    z2 = mesh2.GetPoint(i, j).Z;
                    if ( Math.Abs(z2 - z1) > dz) 
                    {
                        dz = Math.Abs(z2 - z1);
                    }
                }
            }
            return dz;
        }
        
        

        //void CreateInterpolatedGrids(layerGridsStruct grid, int nx, int ny)
        //{
        //    InversePower ip = new InversePower();
        //    int ibh, idepth;
        //    LasFileData las;
        //    double x, y;
        //    double[] values;
        //    foreach (Point p in grid.Points)
        //    {
        //        ibh = p.X;
        //        idepth = p.Y;
        //        CBorehole bh = Boreholes[ibh];
        //        las = bh.Curves.lasData;
        //        x = bh.Position.X;
        //        y = bh.Position.Y;
        //        //z = bh.Position.Z - las.GetDepth(idepth);
        //        values = las.GetPropertyValues(idepth, Properties);
        //        if (values == null) continue;
        //        ip.AddPoint(x, y, 0, values);
        //    }           

        //    grid.grids = new List<float[]>();
            
        //    for (int k = 0;k<Properties.Count;k++)
        //    {
        //        for (int i = 0; i < ny; i++)
        //        {
        //            y = miny + i * ystep;
        //            for (int j = 0; j < nx; j++)
        //            {
        //                x = minx + j * xstep;
        //                grid.grids.Add(ToFloatArray(ip.GetInterpolatedValues(x, y, 0)) );                        
        //            }
        //        }
        //    }            
        //}
        List<layerGridsStruct> CreateMeshesZGrids(CMesh mesh1, CMesh mesh2)
        {
            int id;
            double x, y, z, z1, z2, percent;
            List<layerGridsStruct> grids = new List<layerGridsStruct>();
            
            //double dz = GetMaximumDeltaHight(mesh1, mesh2);
            //int nz = (int)( dz / zstep ) + 1;

            for(int iz = 0; iz < 100; iz++)
            {
                percent = iz;
                layerGridsStruct grid = new layerGridsStruct(percent);
                for (int i=0;i<Boreholes.Count;i++)                
                {
                    CBorehole bh = Boreholes[i];
                    LasFileData las = bh.Curves.lasData;
                    if (las == null) continue;
                    x = bh.Position.X;
                    y = bh.Position.Y;
                    z1 = mesh1.GetValue(x, y);//有底界面
                    z2 = mesh2.GetValue(x, y);//有顶界面
                    z = z1 + percent * (z2 - z1) / 100;                    
                    id = las.GetIndexFromDepth( bh.Position.Z - z);//深度
                    if ( id >= 0 )grid.Add(i, id);                    
                }
                grid.CreateInterpolatedGrids(xGrid, yGrid, Boreholes, Properties, minx, miny, xstep, ystep);                
                grids.Add(grid);
            }
            return grids;
        }

        void CreateMeshesZGrids()
        {
            meshesGrids = new List<layerGridsStruct>[Layers.Count - 1];
            CMesh mesh1, mesh2;
            for( int i = 0; i < Layers.Count - 1; i++ )
            {
                mesh1 = Layers[i];
                mesh2 = Layers[i+1];
                meshesGrids[i] = CreateMeshesZGrids(mesh1, mesh2);
            }
        }
        /// <summary>
        /// 将地层相交部分白化切除
        /// 上下层界面相交，切除上层（尖灭），切除下层（断层）
        /// 地层按Z值排序 从小到大
        /// z
        /// ^------------->  L3 upper
        /// |------------->  L2
        /// O------------->x L1 lower
        /// </summary>
        /// <param name="depthDown">深度是否向下，true坐标系Z表示深度,false坐标系Z表示高程</param>
        /// <returns>true已切割/false未切割</returns>
        public bool BlankLayers(bool depthDown = false, bool upper = true)
        {
            CMesh s1, s2;
            double x, y, z1, z2;
            bool blanked = false;
            for (int i = 0; i < Layers.Count - 1; i++)
            {
                s1 = Layers[i];
                s2 = Layers[i + 1];
                for (int iy = 0; iy < s1.nRow; iy++)
                {
                    for (int ix = 0; ix < s1.nCol; ix++)
                    {
                        x = s1.minx + ix * s1.xStep;
                        y = s1.miny + iy * s1.yStep;
                        if (s1.IsBlankValue(ix, iy)) continue;

                        if (!s2.IsInRange(x, y))
                        {
                            s1.SetBlanked(ix, iy);
                            blanked = true;
                        }
                        else
                        {
                            z1 = s1.GetValue(x, y);
                            z2 = s2.GetValue(x, y);

                            if (CSurferGrid.IsBlankValue(z1)) continue;
                            if (CSurferGrid.IsBlankValue(z2)) continue;

                            if (z1 > z2)
                            {
                                if (upper)
                                {
                                    //深度(Z)向下坐标系
                                    if (depthDown) s1.SetBlanked(ix, iy);
                                    else s2.SetBlanked(ix, iy);//Z向上坐标系
                                }
                                else
                                {
                                    //深度(Z)向下坐标系
                                    if (depthDown) s2.SetBlanked(ix, iy);
                                    else s1.SetBlanked(ix, iy);//Z向上坐标系
                                }
                                blanked = true;
                            }
                        }//else
                    }//for (int ix = 0; ix < s1.xGrid; ix++)
                }//for (int ix = 0; ix < s1.xGrid; ix++)
                if (blanked) Layers[i] = s1;
            }
            return blanked;
        }

        /// <summary>
        /// 获得点所在的上下地层Z1,Z2
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="z0"></param>
        /// <param name="z2">顶界面位置</param>
        /// <param name="z1">底界面位置</param>
        /// <param name="id2">顶界面地层序号，<0 无顶界面</param>
        /// <param name="id1">底界面地层序号，<0 无底界面</param>
        /// <returns></returns>
        void GetLayerBorders(double x0, double y0, double z0, out double z1, out double z2, out int id1, out int id2)
        {
            /// z   z0
            /// ^------------->  L3 upper
            /// |---z0-------->  L2
            /// O------------->x L1 lower
            ///     z0
            z1 = z2 = 0;
            id1 = id2 = -1;
            double z = 0;
            for (int i = 0; i < Layers.Count; i++)//逐层比较,Layers按Z排序
            {
                z = Layers[i].GetValue(x0, y0);
                if (CSurferGrid.IsBlankValue(z)) continue;
                if (z0 < z) //顶界面
                {
                    id2 = i;
                    z2 = z;
                    break;
                }
                else // z0 >= z，底界面
                {
                    id1 = i;
                    z1 = z;
                }
            }
        }        

        /// <summary>
        ///获取输入数据的范围 x,y,z
        ///属性值范围：v1,v2,v3...
        /// </summary>
        public override void UpdatePointsRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;
            
            if ( Boreholes.Count < 1 ) return;

            int id;
            CBorehole bh;
            CubeModel64 cube;
            LasFileData las;
            Vector64 p,p1;
            PropertiesRanges.Clear();
            for (int i = 0; i < Boreholes.Count; i++)
            {
                bh = Boreholes[i];
                cube = bh.GetRangesByLasData();
                las = bh.Curves.lasData;                
                if ( i == 0 )
                {
                    minx = cube.X1;
                    miny = cube.Y1;
                    minz = cube.Z1;
                    maxx = cube.X2;
                    maxy = cube.Y2;
                    maxz = cube.Z2;
                    for(int k = 0; k< Properties.Count;k++)                    
                    {
                        id = las.GetColumnIndex(Properties[k]);
                        PropertiesRanges.Add(las.DataRanges[id]);
                    }                    
                }
                else
                {
                    if (cube.X1 < minx) minx = cube.X1;
                    if (cube.Y1 < miny) miny = cube.Y1;
                    if (cube.Z1 < minz) minz = cube.Z1;                    
                    if (cube.X2 > maxx) maxx = cube.X2;
                    if (cube.Y2 > maxy) maxy = cube.Y2;
                    if (cube.Z2 > maxz) maxz = cube.Z2;
                    //update range
                    for (int k = 0; k < Properties.Count; k++)                        
                    {
                        id = las.GetColumnIndex(Properties[k]);
                        p = las.DataRanges[id];
                        p1 = PropertiesRanges[k];
                        if ( p.X < p1.X ) p1.X = p.X;
                        if ( p.Y > p1.Y) p1.Y = p.Y;
                        PropertiesRanges[k] = p1;
                    }
                }
            }
        }

        float[] GetInterpolatedValueCPU(int xn, int yn, int zn)
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z;
            long xy = xn * yn;
            long id;

            try
            {
                progressTitle = "数据整理...";
                UpdatePointsRange();//这句不需要，插值前已经赋值
                SetGrid(xn, yn, zn);

                progressTitle = "计算地层界线...";
                
                grid3ds.Clear();

                for (int i = 0; i < Properties.Count; i++)
                { 
                    float []grid = new float[xn * yn * zn];
                    grid3ds.Add( grid );
                }

                double dx = (maxx - minx) / (xn - 1);
                double dy = (maxy - miny) / (yn - 1);
                double dz = (maxz - minz) / (zn - 1);
                double sec = 0;
                DateTime t1 = DateTime.Now;

                progressTitle = "正在进行网格插值...";
                List<double> values;
                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + dz * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + dy * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + dx * ix;
                            id = iz * xy + iy * xn + ix;
                            grid3d[id] = (float)GetInterpolatedValue(x, y, z);
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
                Clear();
                return null;
            }
        }
        List<float[]> GetInterpolatedValuesCPU(int xn, int yn, int zn)
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z;
            long xy = xn * yn;
            long id;

            try
            {
                progressTitle = "数据整理...";
                UpdatePointsRange();//这句不需要，插值前已经赋值
                SetGrid(xn, yn, zn);

                progressTitle = "创建内存...";

                grid3ds.Clear();
                for (int i = 0; i < Properties.Count; i++)
                {
                    float[] grid = new float[xn * yn * zn];
                    for (int j = 0; j < grid.Length; j++) //置为空
                        grid[j] = CSurferGrid.blankValuefloat;
                    grid3ds.Add(grid);
                }

                double dx = (maxx - minx) / (xn - 1);
                double dy = (maxy - miny) / (yn - 1);
                double dz = (maxz - minz) / (zn - 1);
                double sec = 0;
                DateTime t1 = DateTime.Now;

                progressTitle = "正在创建网格...";

                CreateMeshesZGrids();
                
                progressTitle = "正在进行网格插值...";
                
                float[]values;
                for (int iz = 0; iz < zn; iz++)
                {
                    z = minz + dz * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        y = miny + dy * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            x = minx + dx * ix;
                            id = iz * xy + iy * xn + ix;
                            values = GetInterpolatedValuesFromGrids(x, y, z);
                            if (values != null)
                            {
                                for (int k = 0; k < values.Length; k++)
                                {
                                    if ( !float.IsNaN(values[k]) )
                                        grid3ds[k][id] = values[k];
                                }
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
                percentage = 100;
               
                return grid3ds;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                Clear();
                return null;
            }
        }
        /// <summary>
        /// 玫瑰图过滤器，目标点周围进行360度分带，过滤带中远端点
        /// </summary>
        /// <param name="dividNum">分带数</param>
        /// <param name="filterFar">远端过滤（0-100）%，从0（最近点）-100（最远点）</param>
        /// <returns>过滤后点集合</returns>
        public List<Vector64> RoseDirectionFilter(Vector64 p0, List<Vector64> _points, int dividNum = 10, double farFilterPercentage = 10)
        {
            List<Vector64> points1 = new List<Vector64>();

            double angle_step = 360 / dividNum;
            List<Vector64>[] bands = new List<Vector64>[dividNum];
            double[] mindists = new double[dividNum];
            double[] maxdists = new double[dividNum];

            for (int i = 0; i < dividNum; i++) bands[i] = new List<Vector64>();
            Vector64 p;
            double angle, rad, dist;
            int ib = 0;
            for (int i = 0; i < _points.Count; i++)
            {
                p = _points[i];
                dist = p0.Distance(p);
                if (dist == 0)
                {
                    bands = null;
                    mindists = maxdists = null;
                    points1.Add(p);
                    return points1;
                }

                rad = Math.Asin((p.Y - p0.Y) / dist);
                angle = 180 * rad / Math.PI;
                if (angle < 0) angle = 360 + angle;
                ib = (int)(angle / angle_step); //带号
                if (bands[ib].Count == 0)
                {
                    mindists[ib] = maxdists[ib] = dist;
                }
                else
                {
                    if (dist < mindists[ib]) mindists[ib] = dist;
                    if (dist > maxdists[ib]) maxdists[ib] = dist;
                }
                //放入带内，距离dist，序号i
                bands[ib].Add(new Vector64(0, 0, i, dist));
            }

            bool[] filtered = new bool[_points.Count];
            double filter = farFilterPercentage * 0.01;
            double percent;
            for (int i = 0; i < dividNum; i++)
            {
                if (bands[i].Count < 1) continue; //带内无数据
                if (mindists[i] == maxdists[i]) continue;//1个点或距离相等点，不过滤
                for (int k = 0; k < bands[i].Count; k++)
                {
                    dist = bands[i][k].V;
                    percent = (dist - mindists[i]) / (maxdists[i] - mindists[i]);
                    if (percent >= filter) filtered[(int)(bands[i][k].Z)] = true;
                }
            }

            for (int i = 0; i < filtered.Length; i++)
            {
                if (!filtered[i]) points1.Add(_points[i]);
            }

            filtered = null;
            bands = null;
            mindists = maxdists = null;

            return points1;
        }
        public List<MultiValuesPoint> RoseDirectionFilter(Vector64 p0, List<MultiValuesPoint> _points, int dividNum = 10, double farFilterPercentage = 10)
        {
            List<MultiValuesPoint> points1 = new List<MultiValuesPoint>();

            double angle_step = 360 / dividNum;
            List<Vector64>[] bands = new List<Vector64>[dividNum];
            double[] mindists = new double[dividNum];
            double[] maxdists = new double[dividNum];

            for (int i = 0; i < dividNum; i++) bands[i] = new List<Vector64>();
            MultiValuesPoint p;
            double angle, rad, dist;
            int ib = 0;
            for (int i = 0; i < _points.Count; i++)
            {
                p = _points[i];
                dist = Math.Sqrt( (p.X - p0.X) * (p.X - p0.X) +
                                  (p.Y - p0.Y) * (p.Y - p0.Y) +
                                  (p.Z - p0.Z) * (p.Z - p0.Z) );
                if (dist == 0)
                {
                    bands = null;
                    mindists = maxdists = null;
                    points1.Add(p);
                    return points1;
                }

                rad = Math.Asin((p.Y - p0.Y) / dist);
                angle = 180 * rad / Math.PI;
                if (angle < 0) angle = 360 + angle;
                ib = (int)(angle / angle_step); //带号
                if (bands[ib].Count == 0)
                {
                    mindists[ib] = maxdists[ib] = dist;
                }
                else
                {
                    if (dist < mindists[ib]) mindists[ib] = dist;
                    if (dist > maxdists[ib]) maxdists[ib] = dist;
                }
                //放入带内，距离dist，序号i
                bands[ib].Add(new Vector64(0, 0, i, dist));
            }

            bool[] filtered = new bool[_points.Count];
            double filter = farFilterPercentage * 0.01;
            double percent;
            for (int i = 0; i < dividNum; i++)
            {
                if (bands[i].Count < 1) continue; //带内无数据
                if (mindists[i] == maxdists[i]) continue;//1个点或距离相等点，不过滤
                for (int k = 0; k < bands[i].Count; k++)
                {
                    dist = bands[i][k].V;
                    percent = (dist - mindists[i]) / (maxdists[i] - mindists[i]);
                    if (percent >= filter) filtered[(int)(bands[i][k].Z)] = true;
                }
            }

            for (int i = 0; i < filtered.Length; i++)
            {
                if (!filtered[i]) points1.Add(_points[i]);
            }

            filtered = null;
            bands = null;
            mindists = maxdists = null;

            return points1;
        }
        public float[] GetInterpolatedValuesFromGrids(double x, double y, double z)
        {
            double z1, z2;
            int id1, id2;
            
            //获取点(x,y,z)所在地层上下界面，从下到上
            GetLayerBorders(x, y, z, out z1, out z2, out id1, out id2);
            if (id1 >= 0 && id2 >= 0)//位于两层之间
            {
                //  ------------- id2
                //  --------------id1
                double s = (z - z1) / (z2 - z1) * 100;                
                int ix = (int)((x - minx) / xstep);
                int iy = (int)((y - miny) / ystep);
                List<layerGridsStruct> grids = meshesGrids[id1];
                return grids[(int)s].grids[ ix + iy * xGrid];                
            }            
            else return null;
        }
        public List<double> GetInterpolatedValues(double x, double y, double z)
        {
            double z1, z2;
            int id1, id2;
            //获取点(x,y,z)所在地层上下界面，从下到上
            GetLayerBorders(x, y, z, out z1, out z2, out id1, out id2);
            InversePowerMultiProperties ip = new InversePowerMultiProperties();
            if (id1 >= 0 && id2 >= 0)//位于两层之间
            {
                //  ------------- id2
                //  --------------id1
                double s = (z - z1) / (z2 - z1) * 100;
                ip = SearchBetween(s, id1, id2);                
            }
            else if (id1 < 0 && id2 >= 0)//有顶界面id2，无底界面
            {
                //  ------------- id2
                //  --------------id1 < 0       
                double off = z2 - z;
                ip = SearchLower(off, id2);                
            }
            else if (id1 >= 0 && id2 < 0)//有底界面id1,无顶界面
            {
                //  ------------- id2 < 0
                //  --------------id1
                double off = z - z1;
                ip = SearchUpper(off, id1);
            }
            if (ip.Count > 0)
            {
                //ip.Points = RoseDirectionFilter(new Vector64(x, y, z), ip.Points);
                List<double> values = ip.GetInterpolatedValues(x, y, z);
                ip.Clear();
                return values;
            }
            else return null;
        }

        InversePowerMultiProperties SearchBetween(double percent, int id1, int id2)
        {
            int id;
            double x, y, z, z1, z2, depth;
            InversePowerMultiProperties ip = new InversePowerMultiProperties();
            foreach (CBorehole bh in Boreholes.pData)
            {
                LasFileData las = bh.Curves.lasData;
                if (las == null) continue;
                x = bh.Position.X;
                y = bh.Position.Y;                
                z1 = Layers[id1].GetValue(x, y);//有底界面
                z2 = Layers[id2].GetValue(x, y);//有顶界面
                z = z1 + percent * (z2 - z1) / 100;
                depth = bh.Position.Z - z;//转换为深度
                if (depth < las.minDepth || depth > las.maxDepth) continue;//无效                
                id = las.GetIndexFromDepth(depth);
                if (id < 0) continue;
                double[] values = las.GetPropertyValues(id, Properties);
                if(values != null )ip.AddPoint(x, y, z, values );
            }
            return ip;
        }
        InversePowerMultiProperties SearchUpper(double offset, int id1)
        {
            int id;
            double x, y, depth, z1, z2;
            InversePowerMultiProperties ip = new InversePowerMultiProperties();
            foreach (CBorehole bh in Boreholes.pData)
            {
                LasFileData las = bh.Curves.lasData;
                if (las == null) continue;
                x = bh.Position.X;
                y = bh.Position.Y;
                //----------------id2 <0 //有底界面,无顶界面
                //----------------id1 >=0 
                z1 = Layers[id1].GetValue(x, y);
                z2 = z1 + offset;
                depth = bh.Position.Z - z2; //转换成深度
                if (depth < las.minDepth || depth > las.maxDepth) continue;//无效
                id = las.GetIndexFromDepth(depth);
                if (id < 0) continue;
                double[]values = las.GetPropertyValues(id, Properties);
                if (values != null) ip.AddPoint(x, y, z2, values);                
            }
            return ip;
        }
        InversePowerMultiProperties SearchLower( double offset, int id2 )
        {
            int id;
            double x, y, z1 = 0, z2 = 0,depth;
            InversePowerMultiProperties ip = new InversePowerMultiProperties();
            foreach (CBorehole bh in Boreholes.pData)
            {
                LasFileData las = bh.Curves.lasData;
                if (las == null) continue;
                x = bh.Position.X;
                y = bh.Position.Y;
                //--------------id2 有顶界面,无底界面
                //--------------id1 < 0
                z2 = Layers[id2].GetValue(x, y);
                z1 = z2 - offset;//偏移顶面位置
                depth = bh.Position.Z - z1;//转换乘深度
                if (depth < las.minDepth || depth > las.maxDepth) continue;//无效
                id = las.GetIndexFromDepth(depth);
                if (id < 0) continue;
                double[]values = las.GetPropertyValues(id, Properties);
                if (values != null) ip.AddPoint(x, y, z1, values);                
            }
            return ip;
        }

        public override float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {
            if (devices == null)
            {
                return GetInterpolatedValueCPU(xn, yn, zn);
            }

            return grid3d;
        }
        public List<float[]> GetInterpolatedValues(int xn, int yn, int zn, Device[] devices = null)
        {
            if (devices == null)
            {
                return GetInterpolatedValuesCPU(xn, yn, zn);
            }
            return null;
        }
    }

}
