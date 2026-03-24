using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    struct ColumnData
    {
        public List<float> pData;
        public int size
        {
            get { return pData.Count; }
        }
        public ColumnData(bool initial)
        {
            pData = new List<float>();
        }
        public void Clear()
        {
            pData.Clear();
        }
        public void AddData(float v)
        {
            pData.Add(v);
        }
        public float GetData(int index)
        {
            if (index < 0 || index >= pData.Count)
            {
                throw new Exception("index out of range");
#pragma warning disable CS0162 // 检测到无法访问的代码
                return 0;
#pragma warning restore CS0162 // 检测到无法访问的代码
            }
            return pData[index];
        }
        public void RemovePoint(int id)
        {
            pData.RemoveAt(id);           
        }   

    }

    class ColumnDataList
    {
        public List<string> pHeader = new List<string>();
        public List<Vector64> pDataRanges = new List<Vector64>();
        public List<double> MaxValues = new List<double>();
        public List<float[]> pData = new List<float[]>();
        double nullData = -999.25;
        public bool IsHaveTitle 
        {
            get { return pHeader.Count > 0; }
        }
        public string GetHeader(int col)
        {
            if (IsHaveTitle && col <pHeader.Count)
            {
                return pHeader[col];
            }
            else
            {
                char c0 = 'A', c1;
                c1 = (char)((byte)c0 + col);
                return "Column " + c1.ToString();
            }
        }

        public string FormatedHeaderLine 
        {
            get 
            {
                string ss = "";
                int n = Col;
                for(int i=0;i<n;i++)
                {
                    ss += GetHeader(i);
                    if (i < n - 1) ss += ",    ";
                }
                return ss;
            }
        }
        public float[] this[int index]
        {
          get 
            { 
                return pData[index]; 
            }
        }
        public float this[int irow,int icol]
        {
            get
            {
                return pData[irow][icol];
            }
        }
        public int Row
        {
            get { return pData.Count; }
        }
        public int Col
        {
            get 
            { 
                if( pData.Count > 0 )
                {
                    if (pData[0] != null)
                        return pData[0].Length;
                }
                return 0;
            }
        }
        public void Add(float[]values)
        {
            pData.Add(values);
        }
        public void Clear()
        {
            pHeader.Clear();
            pDataRanges.Clear();
            MaxValues.Clear();
            pData.Clear();
        }
        public double GetMiniumValue(int col)
        {
            if (pDataRanges.Count <= col) return 0;
            return pDataRanges[col].X;
        }
        public double GetMaxiumValue(int col)
        {
            if (pDataRanges.Count <= col) return 0;
            return pDataRanges[col].Y;
        }
        public int EqualFilter(int col, float value,bool inverse = false)
        {
            List<float[]> data = new List<float[]>();
            int count = 0;
            for(int i = 0; i < Row; i++)
            {
                if (inverse) //是否反选
                {
                    if (pData[i][col] != value) count++;
                    else data.Add(pData[i]);
                }
                else
                {
                    if (pData[i][col] == value) count++;
                    else data.Add(pData[i]);
                }                              
            }
            pData.Clear();
            pData = data;
            return count;
        }        
        public int LowerFilter(int col, float value,bool inverse = false)
        {
            List<float[]> data = new List<float[]>();
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                if (inverse) //是否反选
                {
                    if (pData[i][col] >= value) count++;
                    else data.Add(pData[i]);
                }
                else
                {
                    if (pData[i][col] < value) count++;
                    else data.Add(pData[i]);
                }                
            }
            pData.Clear();
            pData = data;
            return count;
        }
        public int GreaterFilter(int col, float value,bool inverse = false)
        {
            List<float[]> data = new List<float[]>();
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                if (inverse) 
                {
                    if (pData[i][col] <= value) count++;
                    else data.Add(pData[i]);
                }
                else
                { 
                    if (pData[i][col] > value) count++;
                    else data.Add(pData[i]);
                }
            }
            pData.Clear();
            pData = data;
            return count;
        }
        public int BetweenFilter(int col, float value1,float value2,bool inverse = false)
        {
            List<float[]> data = new List<float[]>();
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                if( inverse )
                {
                    if (pData[i][col] < value1 || pData[i][col] > value2) count++;
                    else data.Add(pData[i]);
                }
                else
                {
                    if (pData[i][col] >= value1 && pData[i][col] <= value2) count++;
                    else data.Add(pData[i]);
                }                
            }
            pData.Clear();
            pData = data;
            return count;
        }
        public int EqualReplace(int col, float value, double newvalue)
        {
            float[] data;
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                data = pData[i];
                if (data[col] == value)
                {
                    data[col] = (float)newvalue;
                    pData[i] = data;
                    count++;
                }
            }
            return count;
        }
        public int LowerReplace(int col, float value, double newvalue)
        {
            float[] data;
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                data = pData[i];
                if (data[col] < value)
                {
                    data[col] = (float)newvalue;
                    pData[i] = data;
                    count++;
                }
            }
            return count;
        }
        public int GreaterReplace(int col, float value, double newvalue)
        {
            float[] data;
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                data = pData[i];
                if (data[col] > value)
                {
                    data[col] = (float)newvalue;
                    pData[i] = data;
                    count++;
                }
            }
            return count;
        }
        public int BetweenReplace(int col, float value1, float value2, double newvalue)
        {
            float[] data;
            int count = 0;
            for (int i = 0; i < Row; i++)
            {
                data = pData[i];
                if (data[col] > value1 && data[col] < value2 )
                {
                    data[col] = (float)newvalue;
                    pData[i] = data;
                    count++;
                }
            }
            return count;
        }
        public void UpdateRanges()
        {
            pDataRanges.Clear();
            float v1;
            float[] values;
            for(int i=0;i<Row;i++)
            {
                values = pData[i];                
                for( int j = 0; j < values.Length; j++ )
                {
                    v1 = values[j];
                    if (i == 0)
                    {
                        Vector64 p = new Vector64(v1,v1,0);
                        pDataRanges.Add(p);
                    }
                    else
                    {
                        Vector64 p = pDataRanges[j];
                        if (v1 < p.x) p.x = v1;
                        if (v1 > p.y) p.y = v1;
                        pDataRanges[j] = p;
                    }
                }//for(int j=0;j<Col;j++)
            }//for(int i=0;i<Row;i++)
        }//public void UpdateRanges()
        public void AddHeader(string[]ss)
        {
            pHeader.Clear();
            foreach(string s in ss)
            {
                pHeader.Add(s);
            }
        }
    }

    class CDataList
    {
        public List<string> pHeader;
        public List<Vector64> pDataRange;
        public List<ColumnData> pData;
#pragma warning disable CS0414 // 字段“CDataList.nullData”已被赋值，但从未使用过它的值
        double nullData = -999.25;
#pragma warning restore CS0414 // 字段“CDataList.nullData”已被赋值，但从未使用过它的值
        public int Row
        {
            get
            {
                if (pData.Count < 1)
                    return 0;
                else return pData[0].size;
            }
        }
        public int Col
        {
            get
            {
                return pData.Count;
            }
        }
        public CDataList()
        {
           pHeader = new List<string>();
           pDataRange = new List<Vector64>();
           pData = new List<ColumnData>();
        }
        public void Clear()
        {
            pHeader.Clear();
            pDataRange.Clear();           
            pData.Clear();
        }
        private void RemovePoint(int index)
        {
            ColumnData data;
            for (int i=0;i<pData.Count;i++)
            {
                data = pData[i];
                data.RemovePoint(index);
                pData[i] = data;
            }            
        }
       
        /// <summary>
        /// Remove data from pDataList
        /// </summary>
        /// <param name="index">Column Index</param>
        /// <param name="isNull">Filter Null</param>
        /// <param name="nullValue">Null Value</param>
        /// <param name="below"> remove below</param>
        /// <param name="belowValue"></param>
        /// <param name="greater"></param>
        /// <param name="greaterValue"></param>
        public void Filter(int index,bool isNull,double nullValue,bool below,double belowValue,bool greater,double greaterValue)
        {
            if (pData.Count <= index) return;
            ColumnData data = pData[index];
            float v;
            bool remove;
            int n = data.size;

            List<int> pRemoveList = new List<int>();

            for (int i= n-1; i>=0;i--)
            {
                v = data.GetData(i);
                remove = false;
                if (isNull)
                {
                    if (v == nullValue)
                        remove = true;
                }
                if( below && !remove )
                {
                    if (v < belowValue)
                        remove = true;
                }
                if (greater && !remove)
                {
                    if (v > greaterValue)
                        remove = true;
                }
                if (!remove) continue;
                pRemoveList.Add(i);                
            }

            if(pRemoveList.Count>0)            
            {
                for (int i = 0; i < pRemoveList.Count; i++)
                {
                    RemovePoint(pRemoveList[i]);
                }

                UpdateRange();
            }

            pRemoveList.Clear();
        }
        public bool GetRange(int col,out double minv,out double  maxv)
        {
            minv = maxv = 0;
            if (pDataRange.Count <= col) return false;
            minv = pDataRange[col].X;
            maxv = pDataRange[col].Y;
            if (minv < maxv) return true;
            else return false;
        }
        public string GetHeadName(int col)
        {
            if (col < 0 || col >= pHeader.Count)
                return "";
            else return pHeader[col];
        }
        public float GetData(int col, int index)
        {
            if (pData.Count <= col) return 0;            
            if (pData[col].size <= index ) return 0;
            return pData[col].GetData(index);
        }
        public void UpdateRange(int id)
        {
            ColumnData data;
            Vector64 p;
            float v;            
            data = pData[id];
            p = new Vector64(0, 0, 0);
            for (int j = 0; j < data.size; j++)
            {
                    v = data.GetData(j);
                    if (j == 0) { p.X = p.Y = v; }
                    else
                    {
                        if (v < p.X) p.X = v;
                        if (v > p.Y) p.Y = v;
                    }
            }
            pDataRange[id] = p;
        }
        public void UpdateRange()
        {
            pDataRange.Clear();            
            Vector64 p = new Vector64();            
            for (int i=0;i<pData.Count;i++)
            {
                pDataRange.Add(p);
                UpdateRange(i);
            }            
        }
        public void AddData(int col,float v)
        {
            if( pData.Count > col )
            {
                ColumnData data =  pData[col];
                data.AddData(v);
                pData[col] = data;
            }
            else
            {
                ColumnData data = new ColumnData(true);
                data.AddData(v);
                pData.Add(data);
            }
        }
        public void AddData(ColumnData data)
        {
            pData.Add(data);
        }
        public void AddHeader(string name)
        {
            pHeader.Add(name);
        }
    }
}
