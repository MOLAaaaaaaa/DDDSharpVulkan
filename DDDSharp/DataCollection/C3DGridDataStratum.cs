using DataCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDSharp.DataCollection
{
    
    //3D GRID data of strataum    
    internal class C3DGridDataStratum : C3DGridData
    {
        public struct StratumIndexStruct 
        {
            public double []Values;
            public int Count 
            {
                get 
                {
                    if (Values == null) return 0;
                    else return Values.Length; 
                } 
            }
            public double this[int index] 
            {
                get { return Values[index]; }
                set { Values[index] = value; }
            }
            public StratumIndexStruct(int count)
            {
                Values = new double[count];
                for(int i = 0; i < Values.Length; i++) Values[i] = 0;
            }
        }

        public StratumDatas Stratums = null;
        public StratumIndexStruct[] pStratumGrid3D = null;
        public C3DGridDataStratum(int nx, int ny, int nz)
        {
            Init();
            xNum = nx;
            yNum = ny;
            zNum = nz;
            xyNum = xNum * yNum;            
        }
        public override int Length
        {
            get
            {
                if (pStratumGrid3D == null) return 0;
                else return pStratumGrid3D.Length;
            }
        }
        public StratumIndexStruct this[int id]
        {
            get
            {
                return pStratumGrid3D[id];
            }
            set 
            {
                pStratumGrid3D[id] = value;
            }
        }
        
        public StratumIndexStruct this[int ix,int iy,int iz]
        {
            get
            {
                long id = GetVerticIndex(ix, iy, iz);
                return pStratumGrid3D[id];
            }
            set
            {
                long id = GetVerticIndex(ix, iy, iz);
                pStratumGrid3D[id] = value;
            }
        }

        public bool CreateGrids(StratumDatas stratums)
        {
            try 
            {
                Stratums = stratums;
                pStratumGrid3D = new StratumIndexStruct[xNum * yNum * zNum];
                //pGridCoords = new Vector32[xNum * yNum * zNum];
                int n = Stratums.Count; //地层数量
                for(int i=0;i< pStratumGrid3D.Length;i++) 
                {
                    //0非地层，1-n地层序号
                    pStratumGrid3D[i] = new StratumIndexStruct(n+1);
                }
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        public override void UpdateRange()
        {
            double val;
            long k = 0;
            for (int i = 0; i < pStratumGrid3D.Length; i++)
            {
                StratumIndexStruct layers = pStratumGrid3D[i];
                if (layers.Count == 0 ) continue;
                for (int j = 0; j < layers.Count; j++)
                {
                    val = layers[j];
                    if (val == double.MaxValue ||
                         double.IsNaN(val) || 
                         double.IsInfinity(val)) continue;
                    if (k == 0) minv = maxv = val;
                    else 
                    {
                        if(val < minv) minv = val;
                        if(val > maxv) maxv = val;
                    }
                    k++;
                }
            }
        }
        
        public override void Normalize()
        {            
            for (int i = 0; i < pStratumGrid3D.Length; i++)
            {
                StratumIndexStruct layers = pStratumGrid3D[i];
                for(int j=0;j<layers.Count;j++)
                {
                    double val = layers[j];
                    if (double.IsNaN(val) ||double.IsInfinity(val)) continue;
                    if (val == double.MaxValue) layers[j] = 1;
                    else layers[j] = (val - minv) / (maxv - minv);
                }
                pStratumGrid3D[i] = layers;
            }
        }

    }
}
