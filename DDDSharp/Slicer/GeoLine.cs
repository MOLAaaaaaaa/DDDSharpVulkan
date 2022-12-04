using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace DataCollection
{
    public class DoubleCompare : IComparer
    {
        int IComparer.Compare(object obj1, object obj2)
        {
            double v1 = Convert.ToDouble(obj1);
            double v2 = Convert.ToDouble(obj2);
            if (v1 > v2) return 1;
            else if (v1 < v2) return -1;
            else return 0;
        }
    }
    
    public enum BaselineDirectionEnum
    {
        Top = 0,    //baseline on the top
        Bottom = 1, //baseline on the bottom
        ReplaceWith = 2,//grid z replace the baseline        
    }

    public class GeoProfile : CSlicer
    {
        //base line on ground        
        public float lineWidth = 1.0f;
        public Color lineColor = Color.Black;
        public BaselineDirectionEnum baseLineDirection = BaselineDirectionEnum.Top;
        public double[] Minzs = null; //z方向的最小Z值
        public double[] Maxzs = null; //z方向的最大Z值
        public double[] zSteps = null; //z方向的步长值
        public GeoProfile()
        {
            type = ShapeEnum.GeoProfile;
        }
        /// <summary>
        /// 从图片中创建切片
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="baseLine"></param>
        /// <param name="direct"></param>
        public GeoProfile(Bitmap bmp, List<Vector64> baseLine, BaselineDirectionEnum direct = BaselineDirectionEnum.Top)
        {
            type = ShapeEnum.GeoProfile;
            baseLineDirection = direct;

            double xx = Vector64.GetLength(baseLine);//切片长度
            double zz = xx * bmp.Height/ bmp.Width; //切片深度

            nRow = 3;
            nCol = baseLine.Count;
            Minzs = new double[nCol];
            Maxzs = new double[nCol];
            zSteps = new double[nCol];
            double zstep = zz / ( nRow - 1 );

            pBaseLine.Clear();
            pBaseLine.AddRange(baseLine);

            pInterpolatedBaseLine.Clear();
            pInterpolatedBaseLine.AddRange(baseLine);

            pData = new Vector64[nRow * nCol];
            double x, y, z, v;
            Vector64 p0;
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    p0 = pInterpolatedBaseLine[j];
                    x = p0.X;
                    y = p0.Y;
                    z = i * zstep;

                    if (direct == BaselineDirectionEnum.Top) z = p0.Z - z;
                    else if (direct == BaselineDirectionEnum.Bottom) z = p0.Z + z;
                    v = 0;
                    pData[i * nCol + j] = new Vector64(x, y, z, v);
                }
            }            
            UpdateRange();
            ColorScale.SetValueRange(0, 100);
        }
        public GeoProfile(CSurferGrid sf, List<Vector64> baseLine, BaselineDirectionEnum direct = BaselineDirectionEnum.Top)
        {
            type = ShapeEnum.GeoProfile;
            baseLineDirection = direct;

            nRow = sf.yGrid;
            nCol = sf.xGrid;
            Minzs = new double[nCol];
            Maxzs = new double[nCol];
            zSteps = new double[nCol];
            double zstep = (sf.maxy - sf.miny) / (nRow - 1);

            pBaseLine.AddRange(baseLine);
            CreateGriddedBaseline(sf, pBaseLine);

            pData = new Vector64[nRow * nCol];
            double x, y, z, v;
            Vector64 p0;
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    p0 = pInterpolatedBaseLine[j];
                    x = p0.X;
                    y = p0.Y;
                    z = sf.miny + zstep * i;

                    if (direct == BaselineDirectionEnum.Top) z = p0.Z - z;
                    else if (direct == BaselineDirectionEnum.Bottom) z = p0.Z + z;
                    v = sf.GetZValue(j, i);
                    pData[i * nCol + j] = new Vector64(x, y, z, v);
                }
            }
            UpdateRange();
            ColorScale.SetValueRange(minv,maxv);
        }
        public CSlicer toSlicer()
        {
            CSlicer slicer = Copy();            
            return slicer;
        }
        //从有序坐标数组中（baseLine）创建规则数组
        public void CreateGriddedBaseline(CSurferGrid sf, List<Vector64> baseLine)
        {
            pInterpolatedBaseLine.Clear();

            //V值（点距）升序排列
            baseLine.Sort((a, b) => { return a.V.CompareTo(b.V); });

            nRow = sf.yGrid;
            nCol = sf.xGrid;

            double xstep = (sf.maxx - sf.minx) / (nCol - 1);
            //double zstep = (sf.maxy - sf.miny) / (nRow - 1);
            double dist, x, y, z, v;
            int n1, n2;
            Vector32 p1, p2, pp;
            for (int i = 0; i < sf.xGrid; i++)
            {
                dist = sf.minx + i * xstep;

                //超界，不在范围之内
                if ( !Vector64.SearchBoder(dist, baseLine, out n1, out n2, sf.minx, sf.maxx) )
                {
                    if (n1 < 0 && n2 >= 0)//下界
                    {
                        p1 = baseLine[n2 + 1];
                        p2 = baseLine[n2];
                    }
                    else //if (n1 >= 0 && n2 < 0)//上界
                    {
                        p1 = baseLine[n1 - 1];
                        p2 = baseLine[n1];
                    }
                }
                else
                {
                    p1 = baseLine[n1];
                    p2 = baseLine[n2];
                }

                if (n1 == n2)
                {
                    x = p1.X;
                    y = p1.Y;
                    z = p1.Z;
                }
                else
                {
                    pp = p1 + (p2 - p1) * (dist - p1.V) / (p2.V - p1.V);
                    x = pp.X;
                    y = pp.Y;
                    z = pp.Z;
                }

                v = sf.GetZValue(i, 0);
                pInterpolatedBaseLine.Add(new Vector64(x, y, z,v));
            }
        }
        /// <summary>
        /// 切面与平面相交，得到与切片相同数目的交点坐标
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public override C3DLine[] CreateIntersectionLines(CMesh mesh)
        {
            C3DLine[] lines = new C3DLine[1];
            lines[0] = new C3DLine();
            lines[0].Name = Name + "_" + mesh.Name;
            Vector64 p, p1, p2, p0;            
            for (int i = 0; i < nCol; i++)
            {
                p = pData[i];
                p1 = p2 = p;

                p1.z = Minzs[i] - 1;
                p2.z = Maxzs[i] + 1;

                if ( !mesh.GetInterSection(p1, p2, out p0))//无交点坐标
                {
                    p0 = (p1 + p2) / 2.0;
                    p0.V = m_BlankValue;
                }

                lines[0].AddPoint(p0);
            }
            //保留当前垂向切面的值
            lines[0].UpdateRange();
            lines[0].minv = minv;
            lines[0].maxv = maxv;
            lines[0].ColorScale.SetValueRange(minv, maxv);
            return lines;
        }
        public override void UpdateRange()
        {
            minx = miny = minz = 0;
            maxx = maxy = maxz = 0;

            if (pData == null) return;

            Vector32 p;
            for (int i = 0; i < pData.Length; i++)
            {
                p = pData[i];
                if (i == 0)
                {
                    minx = maxx = p.x;
                    miny = maxy = p.y;
                    minz = maxz = p.z;
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
            int k = 0;
            for (int i = 0; i < pData.Length; i++)
            {
                p = pData[i];
                if (IsBlankValue(p.V)) continue;
                if (k == 0)
                {
                    minv = maxv = p.v;
                    k++;
                }
                else
                {
                    if (p.v < minv) minv = p.v;
                    if (p.v > maxv) maxv = p.v;
                }
            }

            xStep = (maxx - minx) / (nCol - 1);
            
            if (Minzs == null) Minzs = new double[nCol];
            if (Maxzs == null) Maxzs = new double[nCol];
            if (zSteps == null) zSteps = new double[nCol];

            Vector64 p1,p2;
            int iz1 = 0, iz2 = nRow-1;
            for(int i = 0; i < nCol; i++ )
            {
                p1 = pData[iz1 * nCol + i];
                p2 = pData[iz2 * nCol + i];
                Minzs[i] = Math.Min(p1.Z,p2.Z);
                Maxzs[i] = Math.Max(p1.Z, p2.Z);
                zSteps[i] = (Maxzs[i] - Minzs[i]) / (nRow - 1);
            }
        }
        /// <summary>
        /// 在剖面顶部搜索对应Z0（高程）的点
        /// </summary>
        /// <param name="z0">Z0高程值</param>        
        /// <param name="z01">底界面Z值</param>
        /// <param name="line">界面线</param>
        /// <param name="points"></param>
        /// <returns></returns>
        public int SearchPointsOnProfileUpper(double z0, double z01, C3DLine line, ref List<Vector64> points)
        {
            //0(z01) - 1(z02)
            double scale = (z0 - z01) / (maxz - z01);
            double z, z1, z2;
            int iz;
            Vector64 p;
            for (int i = 0; i < nCol; i++)
            {
                z1 = line[i].Z;
                z2 = maxz;

                //z0, 对应的z位置
                z = z1 + (z2 - z1) * scale;

                //对应的z网格序号
                if (GetPoint(0, 0).Z < GetPoint(nRow - 1,0).Z) // 0 -> nRow方向，深度增加                
                    iz = (int)((z - Minzs[i]) / zSteps[i]);
                else iz = nRow - (int)((z - Minzs[i]) / zSteps[i]);
                if (iz >= 0 && iz < nRow)
                {
                    p = pData[i + iz * nCol];
                    //是有效值
                    if (!IsBlankValue(p.V)) points.Add(p);
                }
            }
            return points.Count;
        }
        /// <summary>
        /// 在剖面底部搜索对应Z0（高程）的点
        /// </summary>
        /// <param name="z0">Z0高程值</param>        
        /// <param name="z02">底界面Z值</param>
        /// <param name="line">顶界面线</param>
        /// <param name="points"></param>
        /// <returns></returns>
        public int SearchPointsOnProfileLower(double z0, double z02, C3DLine line, 
                                              ref List<Vector64> points, 
                                              int rowRad = 1 )
        {
            double dd = line.maxz - minz;//顶界面到底部距离
            double scale = (dd-(z02 - z0)) / dd;
            double z, z1, z2;
            int iz;
            Vector64 p;
            for ( int i = 0; i < nCol; i++)
            {
                z2 = line[i].Z;
                z1 = line[i].Z - dd;

                //z0, 对应的z位置
                z = z1 + (z2 - z1) * scale;

                //对应的z网格序号
                if (GetPoint(0, 0).Z < GetPoint(nRow - 1,0).Z) // 0 -> nRow方向，深度增加                
                    iz = (int)((z - Minzs[i]) / zSteps[i]);
                else iz = nRow - (int)((z - Minzs[i]) / zSteps[i]);

                for (int kz = iz - 1; kz <= iz + 1; kz++)//同时搜索前后俩行
                {
                    if (kz >= 0 && kz < nRow)
                    {
                        p = pData[i + kz * nCol];
                        //是有效值
                        if (!IsBlankValue(p.V)) points.Add(p);
                    }
                }
            }
            return points.Count;
        }
        /// <summary>
        /// 在两层界面之间搜索Z方向对应Z0的点
        /// </summary>
        /// <param name="z0">纵向深度值</param>
        /// <param name="z01">地层顶界面值</param>
        /// <param name="z02">地层底界面值</param>
        /// <param name="line1">底界面剖面分割线</param>
        /// <param name="line2">顶界面剖面分割线</param>
        /// <param name="points">搜索点结果数组</param>            
        /// <param name="rad">搜索网格半径（前后行数）</param>
        /// <returns>返回已搜索点数目</returns>
        public int SearchPointsOnProfile(double z0, double z01, double z02, 
                                         C3DLine line1, C3DLine line2, 
                                         ref List<Vector32>points, int rad = 0 )
        {
            int iz;
            Vector64 p;
            double z, z1, z2;
            double scale = (z0 - z01) / (z02 - z01);//底界面0-->顶界面1
            for ( int i = 0; i < nCol; i++ )
            {
                z1 = line1[i].Z;
                z2 = line2[i].Z;                
                //z0, 对应的z位置
                z = z1 + (z2 - z1) * scale;
                //对应的z网格序号
                if( GetPoint(0, 0).Z < GetPoint(nRow - 1,0).Z)//z序->递增
                    iz = (int)((z - Minzs[i]) / zSteps[i]);                
                else iz = nRow - (int)((z - Minzs[i]) / zSteps[i]);//z序->递减
                for (int kz = iz - rad; kz <= iz + rad; kz++)
                {
                    if (kz >= 0 && kz < nRow)
                    {
                        p = pData[i + kz * nCol];                       
                        if (!IsBlankValue(p.V)) points.Add(p); //是否有效值
                    }
                }
            }
            return points.Count;
        }

        public override bool LoadFrom(BinaryReader br)
        {
            if ( C3DData.DataVersion <= 1.2f ) return LoadFrom12(br);
            else if(C3DData.DataVersion > C3DData.Version)
            { errMessage = "not supported version";  return false; }
            return false;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            return SaveAs12(br);
        }
        public bool SaveAs12(BinaryWriter br)
        {
            if (!SaveObjHeader(br)) return false;
            try
            {
                br.Write(lineWidth);
                br.Write(lineColor.ToArgb());
                br.Write((int)pBaseLine.Count);
                for (int i = 0; i < pBaseLine.Count; i++)
                {
                    br.Write(pBaseLine[i].X);
                    br.Write(pBaseLine[i].Y);
                    br.Write(pBaseLine[i].Z);
                    br.Write(pBaseLine[i].V);
                }
                
                if ( pData == null )
                {
                    br.Write((int)0);
                }
                else
                {
                    br.Write((int)pData.Length);
                    br.Write(ObjColor.ToArgb());
                    ColorScale.WriteBinary(br);
                    br.Write(nRow);
                    br.Write(nCol);                    
                    for (int i = 0; i < pData.Length; i++)
                    {
                        br.Write(pData[i].X);
                        br.Write(pData[i].Y);
                        br.Write(pData[i].Z);
                        br.Write(pData[i].V);
                    }                    
                }
                
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool LoadFrom12(BinaryReader br)
        {
            if ( !LoadObjHeader(br) ) return false;
            try
            {
                lineWidth = br.ReadSingle();
                lineColor = Color.FromArgb( br.ReadInt32() );
                pBaseLine.Clear();
                int n = br.ReadInt32();
                double x, y, z, v;
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadDouble();
                    y = br.ReadDouble();
                    z = br.ReadDouble();
                    v = br.ReadDouble();
                    pBaseLine.Add(new Vector64(x,y,z,v));
                }

                pData = null;
                
                n = br.ReadInt32();
                if ( n > 0 )
                {
                    ObjColor = Color.FromArgb(br.ReadInt32());
                    ColorScale.LoadBinary(br);
                    nRow = br.ReadInt32();
                    nCol = br.ReadInt32();
                    pData = new Vector64[n];
                    for (int i = 0; i < n; i++)
                    {
                        x = br.ReadDouble();
                        y = br.ReadDouble();
                        z = br.ReadDouble();
                        v = br.ReadDouble();
                        pData[i] = new Vector64(x, y, z, v);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }

        public override bool ExportData(string path)
        {  
            if ( pData == null ) 
            {
                errMessage = "No gridded data.";
                return false; 
            }
            try 
            {                
                StreamWriter wr = new StreamWriter(new FileStream(path, FileMode.Create));
                string line = "x,   y,  z,  value";
                wr.WriteLine(line);
                Vector64 p;
                for (int i = 0; i < nRow; i++)
                {
                    for (int j = 0; j < nCol; j++)
                    {
                        p = pData[i * nCol + j];
                        line = p.X + ", " + p.Y + ", " + p.Z + ", " + p.V;
                        wr.WriteLine(line);
                    }
                }
                wr.Close();
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }            
        }

        public double GetErr( double basedValue = 1.0E-6)
        {
            //求取平面范围,最宽的一个方向
            double xx = Math.Max(maxx - minx, maxy - miny);
            //最小的距离误差
            return xx * basedValue;            
        }
        private int GetFirstPoint(List<Vector64> Points)
        {
            UpdateRange();

            //先计算点的平面范围
            double xx = maxx - minx;
            double yy = maxy - miny;

            Vector64 p;
            //先确定首点
            if (xx >= yy)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    p = Points[i];
                    if (p.x == minx) return i;                   
                }
            }
            else
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    p = Points[i];
                    if (p.y == miny)return i;                   
                }
            }

            return 0;
        }        

        //将测点坐标points投影到地面作为地表基准线line
        public void GetBaseLineFromPoints(List<Vector64> Points) 
        {
            pBaseLine.Clear();
            int n = Points.Count;
            if (n <= 0) return;

            Vector64 p, p0 = new Vector64();
            Hashtable htobj = new Hashtable();
            int id = 0;
            double dist = 0;

            id = GetFirstPoint(Points);
            htobj.Add(dist, id);
            p0 = Points[id];
            p0.z = 0;

            for (int i=0;i < Points.Count;i++)
            {
                p = Points[i];
                p.z = 0;                
                dist = Math.Round(p.Distance(p0),6);
                if ( !htobj.Contains(dist) )htobj.Add(dist, i);
            }

            ArrayList list = new ArrayList(htobj.Keys);
            list.Sort( new DoubleCompare() );
           
            for (int i=0;i<list.Count;i++)
            {
                id = Convert.ToInt32(htobj[list[i]]);
                pBaseLine.Add( Points[id] );
            }

            list.Clear();
            htobj.Clear();            
        }
    }
}
