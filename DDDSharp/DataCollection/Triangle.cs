using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DataCollection
{
    /*
    class Trianglate
    {        
        [DllImport("TriangleLib.dll", CallingConvention = CallingConvention.Cdecl,CharSet =CharSet.Ansi)]
        static extern int trianglate(char[] triswitches, float[] px, float[] py, float[] pa, int np,
            out int np1, out int attrib, out int corner);

        [DllImport("TriangleLib.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern int GetOutout(float[] ox, float[] oy, float[] oa, int[] angle);

        List<float> px = new List<float>();
        List<float> py = new List<float>();
        List<float> pz = new List<float>();
        List<int> triangles = new List<int>();
        public string switches { get; set; } = "-z";
        public int outPointNum = 0;
        public int outAttribNum = 0;
        public int outCornerNum = 0;
        public int triangleNum = 0;
        public int PointsCount { get { return px.Count; } }

        public int GetTriangle(int index)
        {
            if (index < triangles.Count)
                return triangles[index];
            else return -1;
        }
        public void GetPoint(int index,out double x,out double y,out double z)
        {
            x = y = z = 0;
            if( index < px.Count )
            {
                x = px[index];
                y = py[index];
                z = pz[index];
            }
        }

        public Trianglate()
        {      

        }
        public void Clear()
        {
            px.Clear();
            py.Clear();
            pz.Clear();
            triangles.Clear();            
        }
        public void AddPoint(double x,double y,double z)
        {
            px.Add((float)x);
            py.Add((float)y);
            pz.Add((float)z);
        }      
        public int Triangle()
        {
            try
            {
                triangleNum = trianglate(switches.ToCharArray(),
                            px.ToArray(),
                            py.ToArray(),
                            pz.ToArray(),
                            px.Count,
                            out outPointNum, out outAttribNum, out outCornerNum);
            }
            catch(Exception e)
            {
                return 0;
            }

            if (triangleNum < 1 || outPointNum < 1 ||
                 outAttribNum < 1 || outCornerNum < 1)
            {
                Clear();
                return 0;
            }

            float[] ox = new float[outPointNum + 1];
            float[] oy = new float[outPointNum + 1];
            float[] oa = new float[outAttribNum + 1];
            //outCornerNum = 3
            int[] angles = new int[(triangleNum + 1) * outCornerNum];

            GetOutout(ox, oy, oa, angles);

            px.Clear();
            py.Clear();
            pz.Clear();
            triangles.Clear();
            //scale 0-1 to  minx-maxx,miny-maxy
            for (int i = 0; i < outPointNum; i++)
            {
                px.Add(ox[i]);
                py.Add(oy[i]);
                pz.Add(oa[i]);
            }
            for (int i = 0; i < outAttribNum; i++)
            {                
            }
            for (int i = 0; i < triangleNum* outCornerNum; i++)
            {
                triangles.Add(angles[i]);                
            }
            ox = null;
            oy = null;
            oa = null;
            return triangles.Count;
        }
    }
    */
}
