using CLInterpolation;
using DataCollection;
using OpenCLNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Khronos.Platform;

namespace DDDSharp.BoreholesModeling
{
    /// <summary>
    /// 地层及概率统计
    /// </summary>
    public struct StratumStatisticsStruct
    {
        public string StratumName;
        public double Percent;
        public int Count;
        double Sum;
        public StratumStatisticsStruct(string stratum, double percent = 0)
        {
            StratumName = stratum;
            Percent = percent;
            Count = 0;
            Sum = 0;
        }
        public void AddTo(double percent)
        {
            if(Percent<percent)Percent = percent;
            Count++;
            return;

            if (Sum == double.MaxValue || Percent == double.MaxValue)
            {
                Sum=Percent = double.MaxValue;
                Count++;
            }
            else 
            {
                if (percent == double.MaxValue)
                {
                    Sum = Percent = double.MaxValue;
                    Count++;
                }
                else
                {
                    Sum += percent;
                    Count++;
                    Percent = Sum / Count;
                }
            }               
            
        }
    }
    public class StratumStatistics
    {
        public StratumStatisticsStruct[] Layers;
        public StratumStatistics(int n)
        {
            Layers = new StratumStatisticsStruct[n];
            for(int i = 0;i<n;i++) Layers[i] = new StratumStatisticsStruct();            
        }
        public void SetLayer(int index, string name,double percent = 0)
        {
            Layers[index].StratumName = name;
            Layers[index].Percent = percent;
        }
        /// <summary>
        /// 取最大加入
        /// </summary>
        /// <param name="index"></param>
        /// <param name="percent"></param>
        public void SetToLayer(int index, double percent = 0)
        {
            if( Layers[index].Percent < percent ) 
                Layers[index].Percent = percent;
        }
        /// <summary>
        /// 累计
        /// </summary>
        /// <param name="index"></param>
        /// <param name="percent"></param>
        public void AddToLayer(int index, double percent = 0)
        {
            Layers[index].Percent += percent;
        }
        public double GetPercent(int index)
        {
            return Layers[index].Percent;
        }
        public StratumStatisticsStruct GetMainLayer()
        {
            StratumStatisticsStruct layer = new StratumStatisticsStruct();
            double maxpercent = 0;
            for(int i=0;i< Layers.Length;i++) 
            {
                if (Layers[i].Percent > maxpercent) 
                {
                    layer = Layers[i];
                    maxpercent = Layers[i].Percent; 
                }
            }
            return layer;
        }
    }
    /// <summary>
    /// 钻孔地层三维插值
    /// </summary>
    internal class BoreholesStratumInterpolation: InterpolatorBase
    {
        public CBoreholes Boreholes = new CBoreholes();
        StratumDatas Stratums = new StratumDatas();
        Dictionary<string, int> StratumDictionary = new Dictionary<string, int>();
        StratumStatistics[] Statistics3d = null;
        CylinderInterpolator cylinder = new CylinderInterpolator();
        
        public bool Start = false;      

        public BoreholesStratumInterpolation(CBoreholes boreholes) 
        {
            Boreholes = boreholes;
            Stratums.AddStratums(boreholes); 
            
            for (int i=0;i<Stratums.Count; i++)
            {                
                StratumDictionary.Add(Stratums[i].Name, i);
            }
        }
       
        public List<TriangleObj> CreateLayers(int xn,int yn,int zn)
        {
            //创建地层网格统计数据
            percentage = 0;
            Start = true;
            if ( !CreateInterpolatedGrid(xn, yn, zn) ) return null;
            
            List<TriangleObj>objs = new List<TriangleObj>();
            StratumStatisticsStruct ls;

            C3DGridData data = new C3DGridData(xn,yn,zn);
            data.minx = Boreholes.Minx;
            data.miny = Boreholes.Miny;
            data.minz = Boreholes.Minz;
            data.maxx = Boreholes.Maxx;
            data.maxy = Boreholes.Maxy;
            data.maxz = Boreholes.Maxz;
            
            data.minv = 0;
            data.maxv = Stratums.Count-1;

            double minv = 0, maxv = 0;
            for (int i = 0; i < Statistics3d.Length; i++)
            {   
                ls = Statistics3d[i].GetMainLayer();
                int index = StratumDictionary[ls.StratumName];
                data[i] = index;
                //if (ls.StratumName == "工业矿石TFe≥17%")
                //    data.pGridData[i] = (float)ls.Percent;
                //else data.pGridData[i] = 0;                
            }
            data.UpdateRange();
            data.SaveAs(@"C:\jian\2024\简楚\攀枝花\白马\all.3DGrid");
            percentage = 100;
            Start = false;
            //for (int k = 0; k < Stratums.Count; k++)
            //{ 
            //    StratumData layer = Stratums[k];

            //    if (layer.Name != "工业矿石TFe≥17%") continue;

            //    for (int i = 0; i < Statistics3d.Length; i++)
            //    {
            //        data.pGridData[i] = 0;
            //        ls = Statistics3d[i].GetMainLayer();
            //        //if(ls.StratumName == layer.Name) 
            //        //{
            //        //    data.pGridData[i] = (float)ls.Percent;
            //        //}

            //    }

            //data.UpdateRange();
            //if (layer.Name[0] == '/') data.SaveAs(@"C:\jian\2024\简楚\攀枝花\白马\" + "--" + ".3DGrid");
            //else data.SaveAs(@"C:\jian\2024\简楚\攀枝花\白马\" + layer.Name + ".3DGrid");


            //data.m_MarchCubeExt.DoSearchSurface(0.8f, new TextureStruct());
            //TriangleObj obj = data.m_MarchCubeExt.pISOSurfaceExt.toTriangleObj();
            //obj.Name = layer.Name;               
            //obj.uniformColor = layer.Color;
            //obj.IsUniformColor = true;
            //objs.Add(obj);
            //}

            data.Clear();
            
            return objs;
        }

        public bool CreateInterpolatedGrid(int xn, int yn, int zn )
        {
            percentage = 0;
            progressTitle = "正在插值计算...";
            double x, y, z, val;
            long xy = xn * yn;
            long id = 0;
            try
            {   
                //创建地层网格统计
                Statistics3d = new StratumStatistics[xy * zn];   
                for(int i = 0;i<Statistics3d.Length;i++)
                {
                    Statistics3d[i] = new StratumStatistics(Stratums.Count);    
                    for(int j=0;j<Stratums.Count;j++)
                    {
                        Statistics3d[i].SetLayer(j, Stratums[j].Name,0);
                    }
                }

                double sec = 0;                
                minx = Boreholes.Minx;
                miny = Boreholes.Miny;
                minz = Boreholes.Minz;
                maxx = Boreholes.Maxx;
                maxy = Boreholes.Maxy;
                maxz = Boreholes.Maxz;
                
                xstep = (maxx - minx) / (xn - 1);
                ystep = (maxy - miny) / (yn - 1);
                zstep = (maxz - minz) / (zn - 1);

                double mm = xstep;
                
                mm = Math.Min(mm,ystep);                
                cylinder.MinimumDistance = mm;
                cylinder.MaximumDistance = 5*mm;

                DateTime t1 = DateTime.Now;
                Vector64 p = new Vector64();

                for (int iz = 0; iz < zn; iz++)
                {
                    p.z = minz + zstep * iz;
                    for (int iy = 0; iy < yn; iy++)
                    {
                        p.y = miny + ystep * iy;
                        for (int ix = 0; ix < xn; ix++)
                        {
                            p.x = minx + xstep * ix;
                            id = iz * xy + iy * xn + ix;
                            val = Interpolate(id, p);                            
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
                return true;
            }
            catch (Exception e)
            {
                errMsg = "计算失败！" + e.Message;
                return false;
            }
        }

        public double Interpolate(long id, Vector64 p)
        {
            int index = 0;
            double maxval = 0, val,z1,z2;
            CBorehole bh;
            Vector64 pxy = new Vector64(p.X,p.Y,0);
            Vector64 v1= new Vector64(), v2 =new Vector64();

            double mm = Math.Min(Boreholes.maxx - Boreholes.minx, Boreholes.maxy - Boreholes.miny)/4;

            for (int i = 0; i < Boreholes.Count; i++)
            {
                bh = Boreholes[i];
                v1 = v2 = bh.Position;

                pxy.Z = v1.Z;//水平投影
               // if ( pxy.Distance(v1) > mm ) continue;

                for (int j = 0; j < bh.Stratums.Count; j++)
                {
                    StratumData layer = bh.Stratums[j];

                    //if (layer.Name != "工业矿石TFe≥17%") continue;
                    
                    index = StratumDictionary[layer.Name];

                    z2 = bh.Position.Z - layer.TopDepth;
                    z1 = z2 - layer.Thickness;                    
                    cylinder.V1 = bh.GetPositionFromBaseline(z1);
                    cylinder.V2 = bh.GetPositionFromBaseline(z2);

                    val = cylinder.GetInterpolatedValue(p);//插值角度【0-180】                    
                    Statistics3d[id].SetToLayer(index, val); //权值累计

                    //val = cylinder.GetInterpolatedOnDist(p);//角度约束的距离插值                    
                    //Statistics3d[id].AddToLayer(index, val); //权值最大
                }
            }
            return maxval;
        }

    }
}
