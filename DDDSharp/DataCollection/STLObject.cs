using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    public class STLObject:TriangleObj
    {
        public int Count { get; set; } = 0;
        const int MaxNum = 99999999;

        public bool Read(string path)
        {
            Clear();
            BinaryReader br;
            try
            {
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                byte[] header = br.ReadBytes(80);
                Count = br.ReadInt32();
                br.Close();

                if (Count > 0 && Count < MaxNum) return ReadBinary(path);
                else return ReadAscII(path);
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool ReadBinary(string path)
        {
            BinaryReader br;
            try
            {
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                byte[] header = br.ReadBytes(80);
                Count = br.ReadInt32();
                float x, y, z;
                for(int i=0;i<Count;i++)
                {                    
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();

                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    AddPoint(x, y, z);

                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();                    
                    AddPoint(x, y, z);
                    
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();                    
                    AddPoint(x, y, z);

                    br.ReadInt16();

                    int id = points.Count;
                    AddTriangleIndex(id-3,id-2,id-1);
                }

                br.Close();

                UpdateRange();

                return true;
            }
            catch (IOException e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool ReadAscII(string path)
        {
            return false;
        }
        public bool Save(string path)
        {
            BinaryWriter br;
            try
            {
                br = new BinaryWriter(new FileStream(path, FileMode.Create));

                string headerInfo = "STL Object Created By 3D Surfer." + DateTime.Now.ToString();
                
                int len = headerInfo.Length;
                for( int i = 0; i < 80-len; i++ )
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
                Vector32 p1, p2, p3,pn;
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
