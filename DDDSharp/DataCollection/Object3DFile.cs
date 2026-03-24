using DataCollection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDSharp.DataCollection
{
    internal class Object3DFile: TriangleObj
    {
        public int Count { get; set; } = 0;
        const int MaxNum = 99999999;
        char[] chars = new char[] { ' ', ',', '\t' };
        /// <summary>
        /// Read a line frome a text file without empty line
        /// </summary>
        /// <param name="br"></param>
        /// <returns>null - error,empty - end of a file </returns>
        public string ReadLine(StreamReader br)
        {
            string str = "";
            char[] cc = new char[] { ' ', '\t' };
            while ( (str = br.ReadLine()) != null )
            {
                str = str.Trim(cc);
                if ( str.Length > 0 ) break;
            }
            return str;
        }
        void AddVerticis(string line)
        {
            string[] ss = line.Split(chars, StringSplitOptions.RemoveEmptyEntries);
            if(ss.Length >3 )
            {
                float x = float.Parse(ss[1]);
                float y = float.Parse(ss[2]);
                float z = float.Parse(ss[3]);
                AddPoint(x, y, z);
            }
        }
        void AddFaces(string line)
        {
            string[] ss = line.Split(chars, StringSplitOptions.RemoveEmptyEntries);
            char []cc = new char[] {'\\','/'};
            List<int> indices = new List<int>();
            for(int i = 1; i < ss.Length; i++)            
            {
                string []tt = ss[i].Split( cc, StringSplitOptions.RemoveEmptyEntries);
                if(tt.Length == 1) indices.Add( int.Parse(ss[i]) - 1 );
                else 
                {
                    indices.Add(int.Parse(tt[0]) -1 );  // 72/13/24
                }                
            }
            if (indices.Count == 3) AddTriangleIndex(indices[0], indices[1], indices[2]);
            else if (indices.Count == 4) 
            {
                AddTriangleIndex(indices[0], indices[1], indices[2]);
                AddTriangleIndex(indices[0], indices[2], indices[3]);
            }
        }
        public bool Read(string path)
        {
            Clear();
            StreamReader br;
            try
            {
                string line = "";
                br = new StreamReader(new FileStream(path, FileMode.Open));
                while (true)
                {
                    line = ReadLine(br);
                    if (line == null || line.Length < 1) break;
                    if (line[0] == '#' || line[0] == '!' || line[0] == '/') continue;
                    if (line[0] == 'v' || line[0] == 'V') AddVerticis(line);
                    else if (line[0] == 'f' || line[0] == 'F') AddFaces(line);
                    else
                    {
                        continue; //ignored
                        //errMessage = "unrecongnized line format!" + Environment.NewLine + line;
                        //br.Close();
                        //Clear();
                        //return false;
                    }
                }
                UpdateRange();
                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }
        }
       
        public bool Save(string path)
        {
            BinaryWriter br;
            try
            {
                br = new BinaryWriter(new FileStream(path, FileMode.Create));

                string headerInfo = "STL Object Created By 3D Surfer." + DateTime.Now.ToString();

                int len = headerInfo.Length;
                for (int i = 0; i < 80 - len; i++)
                {
                    headerInfo += "c";
                }
                byte[] header = Encoding.ASCII.GetBytes(headerInfo);

                br.Write(header, 0, 80);
                br.Write(Count);

                Int16 property = 0;
                byte r = (byte)(255 * color.x);
                byte g = (byte)(255 * color.y);
                byte b = (byte)(255 * color.z);
                System.Drawing.Color cc = System.Drawing.Color.FromArgb(1, r, g, b);
                property = (Int16)cc.ToArgb();

                int id1, id2, id3;
                Vector32 p1, p2, p3, pn;
                for (int i = 0; i < Count; i++)
                {
                    id1 = triangles[i].x;
                    id2 = triangles[i].y;
                    id3 = triangles[i].z;
                    p1 = points[id1];
                    p2 = points[id2];
                    p3 = points[id3];
                    pn = Vector32.GetNormal(p1, p2, p3);

                    br.Write(pn.x);
                    br.Write(pn.y);
                    br.Write(pn.z);

                    br.Write(p1.x);
                    br.Write(p1.y);
                    br.Write(p1.z);

                    br.Write(p2.x);
                    br.Write(p2.y);
                    br.Write(p2.z);

                    br.Write(p3.x);
                    br.Write(p3.y);
                    br.Write(p3.z);

                    br.Write(property);
                }

                br.Close();

                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }
        }
    }
}
