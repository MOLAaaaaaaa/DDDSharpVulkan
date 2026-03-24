using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataCollection;

namespace DataCollection
{
    internal class VTKFile
    {
        struct VTKHeader 
        {
            public string FistLine;  //pcd 文件的第一行
            public string VERSION;
            public string VTKOUT;
            public string dataType;
            public string ASCII;
            public string DATASET;
            public string POINTS;
            public long fileSize;   //文件字节大小
            public long dataIndex; //数据开始位置
            public long PointsCount;
            public int CellsCount;
            public int CellGridsNum;
        }
        VTKHeader header;
        public List<Vector32>Points = new List<Vector32>();
        public int Count { get { return Points.Count; } }
        public string errMessage = "";
        public double minx, miny, minz, minv;
        public double maxx, maxy, maxz, maxv;
        private VTKHeader GetHeader(string s)
        {
            header = new VTKHeader();
            Regex reg_VERSION = new Regex("version .*");
            Regex reg_VTKOUT = new Regex("vtk output .*");
            Regex reg_BINAERY = new Regex("binary.*");
            Regex reg_ASCII = new Regex("ascii.*"); 
            Regex reg_DATASET = new Regex("dataset .*"); 
            Regex reg_POINTS = new Regex("points .*");

            Match m_VERSION = reg_VERSION.Match(s);
            header.VERSION = m_VERSION.Value;
            header.FistLine = "# .PCD v" + header.VERSION.Split(' ')[1] + " - Point Cloud Data file format";

            Match m_BINAERY = reg_BINAERY.Match(s);
            if (m_BINAERY.Success) header.dataType = "binary";
            Match m_ASCII = reg_ASCII.Match(s);
            if (m_ASCII.Success) header.dataType = "ascii";

            Match m_DATASET = reg_DATASET.Match(s);
            if (m_DATASET.Success) header.DATASET = m_DATASET.Value;

            Match m_POINTS = reg_POINTS.Match(s);
            if (m_POINTS.Success) 
            {
                char[] chars = new char[] { ' ', '\t' };
                header.POINTS = m_POINTS.Value;
                header.PointsCount = long.Parse(header.POINTS.Split(chars,StringSplitOptions.RemoveEmptyEntries)[1]);
            }
            
            return header;
        }
        public string ReadLine(BinaryReader br)
        {
            string line = "";
            while (true) 
            {
               char c = br.ReadChar();
               if (c == '\n' || c == '\r') return line;
               else line += c;
            }
            return line;
        }
        public bool Load(string path)
        {
            try
            {
                Points.Clear();   //数据放到Points     
                minx = miny = minz = minv = 1E30;
                maxx = maxy = maxz = maxv = -1E30;

                FileInfo info = new FileInfo(path);
                long size = info.Length > 256 ? 256 : info.Length;
                BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open));
                byte[] bytes = br.ReadBytes((int)size);  //读取文件到字节数组
                br.Close();

                string text = Encoding.UTF8.GetString(bytes).ToLower();  //转为文本，方便分离文件头部信息
                header = GetHeader(text);
                header.dataIndex = text.IndexOf(header.POINTS) + header.POINTS.Length + 1;  //数据开始的索引
                header.fileSize = info.Length;
                if (header.PointsCount < 1) return false;

                if (header.dataType == "binary") return LoadBinary(path);
                if (header.dataType == "ascii") return LoadASCII(path);

                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        bool SeekMark(StreamReader br,string mark)
        {
            int len = mark.Length;
            string line, subtxt, markstring = mark.ToLower();
            while (br.BaseStream.Position< br.BaseStream.Length)
            {
                line = br.ReadLine();
                if (line.Length < len) continue;

                subtxt = line.Substring(0, mark.Length).ToLower();
                if( subtxt == markstring )return true;
            }
            return false;
        }

        string ReadLine(StreamReader br)
        {
            string line = "";
            while( (line = br.ReadLine()) != null )
            {
                line = line.Trim();
                if( line.Length > 0 )return line;
            }
            return null;
        }


        public bool LoadBinary(string path)
        {
            try 
            {
                byte[] buf;
                float x, y, z, v;
                BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open));
                br.BaseStream.Seek(header.dataIndex, SeekOrigin.Begin);
                for (int i = 0; i < header.PointsCount; i++)
                {
                    buf = br.ReadBytes(12);
                    if(BitConverter.IsLittleEndian)Array.Reverse(buf);
                    z = BitConverter.ToSingle(buf, 0);                    
                    y = BitConverter.ToSingle(buf, 4);                     
                    x = BitConverter.ToSingle(buf, 8);        
                    if (x < minx ) minx = x;
                    if (y < miny ) miny = y;
                    if (z < minz ) minz = z;
                    if (x > maxx ) maxx = x;
                    if (y > maxy ) maxy = y;
                    if (z > maxz ) maxz = z;
                    Points.Add(new Vector32(x,y,z,z));
                }                

                br.ReadByte();//换行符
                string line = "";
                char[] splitechars = new char[] { ' ' };
                
                while (br.BaseStream.Position < br.BaseStream.Length) 
                {
                    line = ReadLine(br);
                    string[] ss = line.Split(splitechars, StringSplitOptions.RemoveEmptyEntries);
                    if(ss.Length >2 && ss[0].ToLower() == "cells")
                    {
                        header.CellsCount = int.Parse(ss[1])-1;
                        header.CellGridsNum = int.Parse(ss[2])/ header.CellsCount;
                        line = ReadLine(br); //OFFSETS vtktypeint64
                        break;
                    }
                }
                long[] Connectivity = new long[header.CellsCount+1];
                for (int i=0;i<header.CellsCount+1;i++)
                {
                    buf = br.ReadBytes(8);
                    if (BitConverter.IsLittleEndian) Array.Reverse(buf);
                    Connectivity[i] = BitConverter.ToInt64(buf, 0);
                }
                br.ReadByte();//换行符
                line = ReadLine(br); //CONNECTIVITY vtktypeint64
                long [,]cellGrids = new long[header.CellsCount, header.CellGridsNum];

                for (int i = 0; i < header.CellsCount; i++)
                {
                    for (int j = 0; j < header.CellGridsNum; j++)
                    {
                        buf = br.ReadBytes(8);
                        if (BitConverter.IsLittleEndian) Array.Reverse(buf);
                        cellGrids[i,j] = BitConverter.ToInt64(buf, 0);
                    }                        
                }

                br.ReadByte();//换行符
                line = ReadLine(br); //CELLS_TYPE 6
                int[]cellsType = new int[header.CellsCount];
                for (int i = 0; i < header.CellsCount; i++) 
                {
                    buf = br.ReadBytes(4);
                    if (BitConverter.IsLittleEndian) Array.Reverse(buf);
                    cellsType[i] = BitConverter.ToInt32(buf, 0);
                }

                br.ReadByte();//换行符
                line = ReadLine(br); //CELL_DATA 464100 482632 
                line = ReadLine(br); //SCALARS F float
                line = ReadLine(br); //LOOKUP_TABLE default
                
                float []cellValues = new float[header.CellsCount];
                for (int i = 0; i < header.CellsCount; i++)
                {
                    buf = br.ReadBytes(4);
                    if (BitConverter.IsLittleEndian) Array.Reverse(buf);
                    cellValues[i] = BitConverter.ToSingle(buf, 0);
                }
                br.Close();

                double xstep = 1000;
                double ystep = 1000;
                double zstep = 1000;
                minz = -20000;
                int nx = (int)((maxx - minx) / xstep) + 1;
                int ny = (int)((maxy - miny) / ystep) + 1;
                int nz = (int)((maxz - minz) / zstep) + 1;

                C3DGridData data = new C3DGridData(nx-1,ny-1,nz-1);
                data.ResetDataRange(minx, maxx, miny, maxy, minz, maxz, 0, 1);

                //StreamWriter wr = new StreamWriter(new FileStream(path + ".dat", FileMode.Create));
                int ix, iy, iz;
                for (int i = 0; i < header.CellsCount; i++)
                {
                    x = y = z = 0;
                    for (int j = 0; j < header.CellGridsNum; j++)
                    { 
                        long id = cellGrids[i, j];
                        x += Points[(int)id].X;
                        y += Points[(int)id].Y;
                        z += Points[(int)id].Z;
                    }
                    x = x / header.CellGridsNum;
                    y = y / header.CellGridsNum;
                    z = z / header.CellGridsNum;
                    
                    if (z > maxz) continue;

                    v = cellValues[i];
                    if (v < minv) minv = v;
                    if (v > maxv) maxv = v;
                    //  Vector32 p = new Vector32(x,y,z,v);
                    ix = (int)( (x - minx) / xstep);
                    iy = (int)( (y - miny) / ystep);
                    iz = (int)( (z - minz) / zstep);
                    data.GeometryLimited(ref ix, ref iy, ref iz);
                    data[ix, iy, iz] = v;
                    
                    //  wr.WriteLine(p.toString(4), 2);
                }
                data.minv = minv;
                data.maxv = maxv;
                data.SaveAs(path + ".3DGrid");

                //wr.Close();

                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }

        }
        public bool LoadASCII(string path)
        {
            try
            {
                string[] ss;
                string line = "";
                char[] chars = new char[] { ' ', '\t' };
                float x, y, z, v;
                StreamReader br = new StreamReader(new FileStream(path, FileMode.Open));
                br.BaseStream.Seek(header.dataIndex, SeekOrigin.Begin);
                minx = miny = minz = minv = 1E30;
                maxx = maxy = maxz = maxv = -1E30;
                for (int i = 0; i < header.PointsCount;i++)
                {
                    line = ReadLine(br);                    
                    ss = line.Split(chars,StringSplitOptions.RemoveEmptyEntries);
                    if (ss.Length < 3) { errMessage = "数据错误！" + line; br.Close();return false; }
                    x = float.Parse(ss[0]); y = float.Parse(ss[1]); z = float.Parse(ss[2]);
                    if (x < minx) minx = x;
                    if (y < miny) miny = y;
                    if (z < minz) minz = z;
                    if (x > maxx) maxx = x;
                    if (y > maxy) maxy = y;
                    if (z > maxz) maxz = z;
                    Points.Add(new Vector32(x, y, z, z));                    
                }
               
                line = ReadLine(br);
                ss = line.Split(chars, StringSplitOptions.RemoveEmptyEntries);
                if (ss[0].ToLower().Trim() == "cells") 
                { 
                    header.CellsCount = int.Parse(ss[1]); 
                    header.CellGridsNum = int.Parse(ss[2])/ ( header.CellsCount + 1 );
                }

                int[,] cellGrids = new int[header.CellsCount, header.CellGridsNum];

                for (int i = 0; i < header.CellsCount; i++)
                {
                    line = ReadLine(br);
                    ss = line.Split(chars, StringSplitOptions.RemoveEmptyEntries);
                    int num = int.Parse((string)ss[0]);
                    for (int j = 0; j < num; j++)
                    {
                        cellGrids[i, j] = int.Parse((string)ss[j+1]);
                    }
                }
               
               // line = ReadLine(br);//cell_types
               // line = ReadLine(br);
                float []cellValues = new float[header.CellsCount];
                if( SeekMark(br, "CELL_DATA") )
                {
                    line = ReadLine(br); //SCALARS F FLOAT
                    line = ReadLine(br); //LOOKUP_TABLE default
                    for (int i = 0; i < header.CellsCount; i++)
                    {
                        line = ReadLine(br);
                        cellValues[i] = float.Parse(line);
                    }
                }

                double xstep = 1000;
                double ystep = 1000;
                double zstep = 1000;
                minz = -20000;
                int nx = (int)((maxx - minx) / xstep) + 1;
                int ny = (int)((maxy - miny) / ystep) + 1;
                int nz = (int)((maxz - minz) / zstep) + 1;

                C3DGridData data = new C3DGridData(nx - 1, ny - 1, nz - 1);
                data.ResetDataRange(minx, maxx, miny, maxy, minz, maxz, 0, 1);

                 int ix, iy, iz;
                for (int i = 0; i < header.CellsCount; i++)
                {
                    x = y = z = 0;
                    for (int j = 0; j < header.CellGridsNum; j++)
                    {
                        long id = cellGrids[i, j];
                        x += Points[(int)id].X;
                        y += Points[(int)id].Y;
                        z += Points[(int)id].Z;
                    }
                    x = x / header.CellGridsNum;
                    y = y / header.CellGridsNum;
                    z = z / header.CellGridsNum;

                    if (z > maxz) continue;

                    v = cellValues[i];
                    if (v < minv) minv = v;
                    if (v > maxv) maxv = v;
                    
                    ix = (int)((x - minx) / xstep);
                    iy = (int)((y - miny) / ystep);
                    iz = (int)((z - minz) / zstep);
                    data.GeometryLimited(ref ix, ref iy, ref iz);
                    data[ix, iy, iz] = v;
                }
                data.minv = minv;
                data.maxv = maxv;
                data.SaveAs(path + ".3DGrid");

                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }

        }
    }
}
