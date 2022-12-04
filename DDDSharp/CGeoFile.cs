using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;

namespace DDDSharp
{
    public class CGeoFile
    {
        public int CrossLineNum = 0;    //xNum
        public int InLineNum = 0;       //yNum
        public double TimeAxisScale = 1; //*elevation
        public string TimeAxisLabel = "Elevation (m)";
        public string errMsg = "";
        public List<Vector32> pCoords = new List<Vector32>();//z = 0;
        public double minx, maxx, miny, maxy;
        public Vector32 cp1, cp2, cp3, cp4;

        public void Clear()
        {
            pCoords.Clear();
        }
        private string LocateSection(ref StreamReader sr,string section)
        {
            string ss;
            while ( (ss = sr.ReadLine()) != null )
            {
                if (ss.Length < 1) continue;
                if (ss.IndexOf(section) == 0) return ss;
            }
            return "";
        }

        private string SpliteString(string str)
        {
            string []sp = str.Split(new Char[] { ' ', ' ' }, 4);
            if (sp.Length > 1) return sp[1];            
            else return "";
        }

        public void Get4Corners()
        {
            int n = pCoords.Count;
            if (n < 1) return;
            cp1 = pCoords[0];
            cp2 = pCoords[CrossLineNum - 1];
            cp4 = pCoords[n - 1];
            cp3 = pCoords[n - 1 - CrossLineNum + 1];            
        }
        void UpdateDataRangeByCorners()
        {
            minx = maxx = cp1.x;
            miny = maxy = cp1.y;

            if (cp2.x < minx) minx = cp2.x;
            if (cp2.y < miny) miny = cp2.y;
            if (cp3.x < minx) minx = cp3.x;
            if (cp3.y < miny) miny = cp3.y;
            if (cp4.x < minx) minx = cp4.x;
            if (cp4.y < miny) miny = cp4.y;

            if (cp2.x > maxx) maxx = cp2.x;
            if (cp2.y > maxy) maxy = cp2.y;
            if (cp3.x > maxx) maxx = cp3.x;
            if (cp3.y > maxy) maxy = cp3.y;
            if (cp4.x > maxx) maxx = cp4.x;
            if (cp4.y > maxy) maxy = cp4.y;            
        }
        public void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;

            if (pCoords.Count < 1) return;
            minx = maxx = pCoords[0].x;
            miny = maxy = pCoords[0].y;
            Vector32 p;
            for(int i = 1; i < pCoords.Count; i++ )
            {
                p = pCoords[i];
                if (p.x < minx) minx = p.x;
                if (p.y < miny) miny = p.y;
                if (p.x > maxx) maxx = p.x;
                if (p.y > maxy) maxy = p.y;
            }
        }
        string [] TrimNull(string []ss)
        {
            List<string> ret = new List<string>();
            string str;
            for(int i=0;i<ss.Length;i++)
            {
                str = ss[i].Trim();
                if (str.Length > 0) ret.Add(str);
            }
            return ret.ToArray();
        }
        private bool ReadCoords(ref StreamReader sr)
        {
            string section1 = "LineID";
            string section2 = "!Trc_NUM";
            int nx = CrossLineNum;
            int ny = InLineNum;
            string ss;
            string[] sp;
            float x, y, z;
            pCoords.Clear();
            for ( int i = 0; i < ny; i++ )
            {
                LocateSection(ref sr, section1);
                LocateSection(ref sr, section2);
                for( int j=0;j < nx; j++ )
                {
                    ss = sr.ReadLine();
                    if (ss == null)
                    {
                        errMsg = "error occurred while load coordinates." + nx + "*" +  ny;
                        return false;
                    }
                    sp = ss.Split(new Char[] { ' ', '\t' }, 10);
                    sp = TrimNull(sp);
                    if (sp.Length<5)
                    {
                        errMsg = "error occurred while load coordinates." + nx + "*" + ny;
                        return false;
                    }
                    
                    x = float.Parse(sp[1]);
                    y = float.Parse(sp[2]);
                    z = float.Parse(sp[3]);
                    pCoords.Add(new Vector32(x, y, z));
                }
            }

            Get4Corners();
            UpdateDataRangeByCorners();

            if (pCoords.Count == nx * ny)
            {
                return true;
            }
            else return false;
        }
        public bool LoadFrom(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string ss = LocateSection(ref sr, "#InLineNumber#");
                InLineNum = int.Parse( SpliteString( ss ) );

                ss = LocateSection(ref sr, "#CrossLineNumber#");
                CrossLineNum = int.Parse(SpliteString(ss));

                ss = LocateSection(ref sr, "#TimeAxisScale#");
                TimeAxisScale = double.Parse(SpliteString(ss));

                ReadCoords(ref sr);

                sr.Close();
                fs.Close();
                return true;
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return false;
            }
        }
    }
}
