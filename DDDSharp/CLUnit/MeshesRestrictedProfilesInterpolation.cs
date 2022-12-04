using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;
using OpenCLNet;

namespace CLInterpolation
{
    /// <summary>
    ///基于地层约束的剖面插值 
    /// </summary>
    class MeshesRestrictedProfilesInterpolation : InterpolatorBase
    {
        public List<CMesh> Layers = new List<CMesh>();//平面--地层界面
        public List<GeoProfile> Profiles = new List<GeoProfile>();//剖面(纵向）

        //平面与剖面的交线，Lines.Count 剖面数,C3Dline[].Length平面数
        List<C3DLine[]> Lines = new List<C3DLine[]>(); 
        public MeshesRestrictedProfilesInterpolation()
        {
            method = InterpolationMethod.GeoProfilesGridding;
        }

        public void AddProfile(GeoProfile sp) { Profiles.Add(sp); }
        public void AddLayer(CMesh layer) { Layers.Add(layer); }

        /// <summary>
        /// 将地层相交部分白化切除
        /// 上下层界面相交，切除上层（尖灭），切除下层（断层）
        /// 地层按Z值排序 
        /// z
        /// ^------------->  L3 upper
        /// |------------->  L2
        /// O------------->x L1 lower
        /// </summary>
        /// <param name="depthDown">深度是否向下，true坐标系Z表示深度,false坐标系Z表示高程</param>
        /// <returns>true已切割/false未切割</returns>
        public bool BlankLayers( bool depthDown = false ,bool upper = true)
        {
            CMesh s1, s2;
            double x, y,z1,z2;
            bool blanked = false;
            for( int i = 0; i < Layers.Count - 1; i++ )
            {
                s1 = Layers[i];
                s2 = Layers[i+1];                
                for (int iy = 0; iy < s1.nRow; iy++)
                {
                    for (int ix = 0; ix < s1.nCol; ix++)
                    {
                        x = s1.minx + ix * s1.xStep;
                        y = s1.miny + iy * s1.yStep;                        
                        if ( s1.IsBlankValue(ix,iy) ) continue;
                        
                        if ( !s2.IsInRange(x, y) ) 
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
                            
                            if ( z1 > z2 )
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
                if( blanked ) Layers[i] = s1;
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
        void GetLayerBorders( double x0,double y0,double z0,out double z1,out double z2,out int id1,out int id2 )
        {
            /// z   z0
            /// ^------------->  L3 upper
            /// |---z0-------->  L2
            /// O------------->x L1 lower
            ///     z0
            z1 = z2 = 0;
            id1 = id2 = -1;
            double z = 0;
            for (int i = 0; i < Layers.Count; i++ )//逐层比较,Layers按Z排序
            {
                z = Layers[i].GetValue(x0, y0);
                if ( CSurferGrid.IsBlankValue(z) ) continue;
                if ( z0 < z ) //顶界面
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
        /// 计算地层界面与剖面的交线
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public void GetIntersectionLines()
        {
            Lines.Clear();
            GeoProfile profile;
            CMesh mesh;
            for( int k = 0; k < Profiles.Count; k++ )
            {
                progressTitle = "计算地层界面"+(k+1) + "/" + Profiles.Count + "...";
                profile = Profiles[k];
                C3DLine[] lines = new C3DLine[Layers.Count];
                for(int i = 0; i < Layers.Count; i++)
                {
                    mesh = Layers[i];
                    C3DLine[]ll = profile.CreateIntersectionLines(mesh);
                    lines[i] = ll[0];
                }
                Lines.Add(lines);
            }            
        }

        /// <summary>
        ///获取输入数据的范围 
        /// </summary>
        public override void UpdatePointsRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;

            if ( Profiles.Count < 1 ) return;

            GeoProfile line;
            for( int i = 0; i < Profiles.Count; i++ )
            {
                line = Profiles[i];
                line.UpdateRange();
                if (i == 0) 
                {
                    minx = line.minx;
                    miny = line.miny;
                    minz = line.minz;
                    minv = line.minv;
                    maxx = line.maxx;
                    maxy = line.maxy;
                    maxz = line.maxz;
                    maxv = line.maxv;
                }
                else
                {
                    if (line.minx < minx) minx = line.minx;
                    if (line.miny < miny) miny = line.miny;
                    if (line.minz < minz) minz = line.minz;
                    if (line.minv < minv) minv = line.minv;
                    if (line.maxx > maxx) maxx = line.maxx;
                    if (line.maxy > maxy) maxy = line.maxy;
                    if (line.maxz > maxz) maxz = line.maxz;
                    if (line.maxv > maxv) maxv = line.maxv;
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

                //UpdatePointsRange();//这句不需要，插值前已经赋值
                SetGrid(xn, yn, zn);

                progressTitle = "计算地层界线...";

                //获取剖面与平面的相交线,存入Lines[i]
                GetIntersectionLines();
                
                grid3d = new float[xn *yn * zn];
                double dx = (maxx - minx) / (xn - 1);
                double dy = (maxy - miny) / (yn - 1);
                double dz = (maxz - minz) / (zn - 1);
                double sec = 0;
                DateTime t1 = DateTime.Now;

                progressTitle = "正在进行网格插值...";               

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
        /// <summary>
        /// 玫瑰图过滤器，目标点周围进行360度分带，过滤带中远端点
        /// </summary>
        /// <param name="dividNum">分带数</param>
        /// <param name="filterFar">远端过滤（0-100）%，从0（最近点）-100（最远点）</param>
        /// <returns>过滤后点集合</returns>
        public List<Vector32> RoseDirectionFilter(Vector32 p0, List<Vector32>_points, int dividNum = 10, double farFilterPercentage = 10)
        {
            List<Vector32> points1 = new List<Vector32>();

            double angle_step = 360 / dividNum;
            List<Vector32>[] bands = new List<Vector32>[dividNum];
            double[] mindists = new double[dividNum];
            double[] maxdists = new double[dividNum];

            for (int i=0;i< dividNum;i++)bands[i] = new List<Vector32>();
            Vector32 p;
            double angle,rad,dist;
            int ib = 0;
            for (int i=0;i<_points.Count;i++)
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

                rad = Math.Asin( (p.Y - p0.Y) / dist );
                angle = 180 * rad / Math.PI;
                if (angle < 0) angle = 360 + angle;
                ib = (int)(angle / angle_step); //带号
                if( bands[ib].Count == 0 )
                {
                    mindists[ib] = maxdists[ib] = dist;
                }
                else
                {
                    if (dist < mindists[ib]) mindists[ib] = dist;
                    if (dist > maxdists[ib]) maxdists[ib] = dist;
                }
                //放入带内，距离dist，序号i
                bands[ib].Add(new Vector64( 0,0,i,dist) );
            }

            bool[] filtered = new bool[_points.Count];
            double filter = farFilterPercentage * 0.01;
            double percent;
            for( int i = 0; i < dividNum; i++ )
            {
                if ( bands[i].Count < 1 ) continue; //带内无数据
                if ( mindists[i] == maxdists[i] ) continue;//1个点或距离相等点，不过滤
                for( int k = 0; k < bands[i].Count; k++ )
                {
                    dist = bands[i][k].V;
                    percent = ( dist - mindists[i] ) / ( maxdists[i] - mindists[i] );
                    if (percent >= filter) filtered[(int)(bands[i][k].Z)] = true;
                }
            }
            
            for(int i=0;i<filtered.Length;i++)
            {
                if ( !filtered[i] ) points1.Add( _points[i] );
            }
            
            filtered = null;
            bands = null;
            mindists = maxdists = null;

            return points1;
        }

        public override double GetInterpolatedValue(double x, double y, double z)
        {
            double z1, z2,val;
            int id1, id2;
            //
            //  ------------- id2
            //
            //  --------------id1
            //
            //获取点(x,y,z)所在地层上下界面，从下到上
            GetLayerBorders(x, y, z, out z1,out z2,out id1,out id2);
            InversePower ip = new InversePower();
            if ( id1 >=0 && id2 >= 0 )//位于两层之间
            {
                //  ------------- id2
                //  --------------id1
                GeoProfile profile;
                C3DLine line1,line2;
                for( int i = 0; i < Profiles.Count; i++ )
                {
                    profile = Profiles[i];
                    line1 = Lines[i][id1];
                    line2 = Lines[i][id2];
                    profile.SearchPointsOnProfile(z, z1, z2, line1, line2, ref ip.Points);
                }
            }
            else if (id1 < 0 && id2 >= 0)//有顶界面id2，无底界面
            {
                //  ------------- id2
                //  --------------id1 < 0                
                GeoProfile profile;
                C3DLine line2;                
                //将顶界面下移作为底界面
                double offz = Layers[id2].maxz - minz;//界面最高位置与数据最低处                
                for (int i = 0; i < Profiles.Count; i++)
                {
                    profile = Profiles[i];
                    line2 = Lines[i][id2];
                    C3DLine line1 = line2.Copy();
                    offz = line1.maxz - minz;
                    line1.Offset(0, 0, -offz);
                    z1 = z2 - offz;
                    profile.SearchPointsOnProfile(z, z1, z2, line1, line2, ref ip.Points);
                    //profile.SearchPointsOnProfileLower(z,z2,line2, ref ip.Points);
                }                
            }
            else if (id1 >= 0 && id2 < 0)//有底界面id1,无顶界面
            {
                //  ------------- id2 < 0
                //  --------------id1
                GeoProfile profile;
                C3DLine line1;
                //将底界面上移作为顶底界面
                double offz;// = maxz - Layers[id1].minz;//界面最高位置与数据最低处
                for (int i = 0; i < Profiles.Count; i++)
                {
                    profile = Profiles[i];
                    line1 = Lines[i][id1];                    
                    offz = maxz - line1.minz;
                    C3DLine line2 = line1.Copy();
                    line2.Offset(0, 0, offz);//底界面上移
                    z2 = z1 + offz;
                    profile.SearchPointsOnProfile(z, z1, z2, line1, line2, ref ip.Points);
                    //profile.SearchPointsOnProfileUpper(z, z1, line1, ref ip.Points);
                }                
            }
            if (ip.Count > 0)
            {
                ip.Points = RoseDirectionFilter(new Vector64(x,y,z), ip.Points);
                val = ip.GetInterpolatedValue(x, y, z);
                ip.Clear();
                return val;
            }
            else return C3DData.m_BlankedValue;
        }
        public override float[] GetInterpolatedValue(int xn, int yn, int zn, Device[] devices = null)
        {   
            if (devices == null)
            {
                return GetInterpolatedValueCPU(xn, yn, zn);
            }
            
            return grid3d;
        }
    }
}
