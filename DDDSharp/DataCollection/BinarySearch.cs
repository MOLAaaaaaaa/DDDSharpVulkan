using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    public class MyBinarySearch
    {
        public float[] Values = null;
        public MyBinarySearch(float[] _values)
        {
            Values = _values;
        }
        bool ValuesEqual(double v1, double v2, double err)
        {
            if (Math.Abs(v2 - v1) <= err) return true;
            else return false;
        }
        public int Search(double val, out int low, out int high,double err = 1.0e-10)
        {
            low = high = -1;
            if (Values.Length < 1) return -1;
            int n = Values.Length;
            if ( ValuesEqual(val, Values[0],err ))
            {
                low = high = 0;
                return 0;
            }
            if (ValuesEqual(val, Values[n-1], err))
            {
                low = high = n-1;
                return n-1;
            }
            if ( val < Values[0] ||val > Values[n-1] ) return -1;
            
            low = 0;
            high = n - 1;
            int mid = (low + high) / 2;
            while (low < high)
            {
                mid = (low + high) / 2;
                if ( ValuesEqual(Values[mid], val, err) )
                {
                    low = high = mid;
                    return low;
                }
                if ( Values[mid] < val ) low = mid;
                else high = mid;                
                if (high - low <= 1) return low;                
            }
            return -1;
        }
    }
}
