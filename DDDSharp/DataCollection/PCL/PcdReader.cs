using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DataCollection
{
    class PcdReader
    {
        //FIELDS x y z                                # XYZ data
        //FIELDS x y z rgb                            # XYZ + colors
        //FIELDS x y z normal_x normal_y normal_z     # XYZ + surface normals
        //FIELDS j1 j2 j3
        //rgb 颜色（打包为 float）	F 或 U(uint32)
        //intensity 激光反射强度 F 或 U
        //timestamp 时间戳（纳秒）D (double)
        //label 语义标签（用于 AI 训练）u
        //VIEWPOINT 字段存储传感器位姿，便于坐标转换。
        //width height 有序 or 无序
        //width = count height = 1 无序
        PCDHeader header = new PCDHeader();
        public List<Vector32> Points = new List<Vector32>();
        public int pointCount = 0;
        public bool LoadPcdHeader(string path)
        {
            try 
            {
                FileInfo info = new FileInfo(path);
                long size = info.Length > 256 ? 256 : info.Length;
                BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open));
                byte[] bytes = br.ReadBytes((int)size);  //读取文件到字节数组
                br.Close();
                string text = Encoding.UTF8.GetString(bytes);  //转为文本，方便分离文件头部信息
                
                header = GetHeader(text);

                string dataType = header.DATA.Split(' ')[1];  //储存的数据类型，ascii binary binary_compressed
                header.Size = header.SIZE.Split(' ').Length - 1;  //SIZE的长度，可以判断是否包含rgb信息
                pointCount = Convert.ToInt32(header.POINTS.Split(' ')[1]);  //点的数量
                header.Count = pointCount;
                header.dataIndex = text.IndexOf(header.DATA) + header.DATA.Length + 1;  //数据开始的索引                 
                header.fileSize = info.Length;
                return true;
            }
            catch(Exception ex) 
            {
                return false;
            }            
        }

        byte[] ReadFrom(BinaryReader br,int size)
        {
            return br.ReadBytes(size);
        }

        /// <summary>
        /// 获取头部信息
        /// </summary>
        /// <param name="s"></param>
        private PCDHeader GetHeader(string s)
        {
            header = new PCDHeader();
            Regex reg_VERSION = new Regex("VERSION .*");  //版本
            Regex reg_FIELDS = new Regex("FIELDS .*");  //字段
            Regex reg_SIZE = new Regex("SIZE .*");  //数据大小
            Regex reg_TYPE = new Regex("TYPE .*");  //存储数据的格式 F-Float U-uint
            Regex reg_COUNT = new Regex("COUNT .*");
            Regex reg_WIDTH = new Regex("WIDTH .*");
            Regex reg_HEIGHT = new Regex("HEIGHT .*");
            Regex reg_VIEWPOINT = new Regex("VIEWPOINT .*");
            Regex reg_POINTS = new Regex("POINTS .*");  //点的数量
            Regex reg_DATA = new Regex("DATA .*");  //数据类型

            Match m_VERSION = reg_VERSION.Match(s);
            header.VERSION = m_VERSION.Value;

            header.FistLine = "# .PCD v" + header.VERSION.Split(' ')[1] + " - Point Cloud Data file format";

            Match m_FIELDS = reg_FIELDS.Match(s);
            header.FIELDS = m_FIELDS.Value;

            Match m_SIZE = reg_SIZE.Match(s);
            header.SIZE = m_SIZE.Value;

            Match m_TYPE = reg_TYPE.Match(s);
            header.TYPE = m_TYPE.Value;

            Match m_COUNT = reg_COUNT.Match(s);
            header.COUNT = m_COUNT.Value;

            Match m_WIDTH = reg_WIDTH.Match(s);
            header.WIDTH = m_WIDTH.Value;

            Match m_HEIGHT = reg_HEIGHT.Match(s);
            header.HEIGHT = m_HEIGHT.Value;

            Match m_VIEWPOINT = reg_VIEWPOINT.Match(s);
            header.VIEWPOINT = m_VIEWPOINT.Value;

            Match m_POINTS = reg_POINTS.Match(s);
            header.POINTS = m_POINTS.Value;

            Match m_DATA = reg_DATA.Match(s);
            header.DATA = m_DATA.Value;
            return header;
        }

        /// <summary>
        /// 读取 binary 和 binary_compressed 格式的 pcd 文件
        /// </summary>
        public void ReadPcdFile(string path)
        {   
            //数据放到Points
            Points = new List<Vector32>();
            BinaryReader br = new BinaryReader(new FileStream(path,FileMode.Open));
            br.BaseStream.Seek(header.dataIndex, SeekOrigin.Begin);

            Vector32 point;
            long curpos = header.dataIndex;            
            if (header.TYPE == "binary")
            {
                long maxSize = 1024*1024;//1MB
                while(curpos < header.fileSize)
                {
                    //0 1 2 3 4 5 6 7 8 9 10 11
                    long leftsize = header.fileSize - curpos; //剩余字节
                    long size = leftsize <= maxSize ? leftsize : maxSize;
                    byte[] data = new byte[size];
                    //二进制文件，直接按字节数组读取即可
                    for (long i = curpos; i < size;)
                    {
                        point = new Vector32();
                        point.X = BitConverter.ToSingle(data, (int)i);
                        point.Y = BitConverter.ToSingle(data, (int)i + 4);
                        point.Z = BitConverter.ToSingle(data, (int)i + 8);
                        if (header.Size == 4)
                        {
                            //point.Color = BitConverter.ToUInt32(data, (int)i + 12);
                        }
                        Points.Add(point);
                        if (Points.Count == pointCount) break;
                        i += 4 * header.Size;
                    }
                    curpos += size;
                    data = null;
                }// while(curpos < header.fileSize)                
            }
            else if (header.TYPE == "binary_compressed")
            {
                /*
                //二进制压缩，先解析压缩前的数据量和压缩后数据量
                int[] bys = new int[2];
                int dataIndex = 0;
                for (int i = 0; i < bys.Length; i++)
                {
                    bys[i] = BitConverter.ToInt32(bytes, index + i * 4);
                    dataIndex = index + i * 4;
                }

                dataIndex += 4;  //数据开始的索引
                int compressedSize = bys[0];  //压缩之后的长度
                int decompressedSize = bys[1];  //解压之后的长度

                //将压缩后的数据单独拿出来
                byte[] compress = new byte[compressedSize];
                //将bs，从索引为a开始，复制到compress的[0至compressedSize]区间内
                Array.Copy(bytes, dataIndex, compress, 0, compressedSize);

                //LZF解压算法
                //byte[] data = Decompress(compress, decompressedSize);
                byte[] data = new byte[decompressedSize];
                Lzf.LZF lzf = new Lzf.LZF();
                lzf.Decompress(compress, compressedSize, data, decompressedSize);
                int type = 0;
                for (int i = 0; i < data.Length; i += 4)
                {
                    //先读取x坐标
                    if (type == 0)
                    {
                        point = new Point3D();
                        point.X = BitConverter.ToSingle(data, i);
                        Points.Add(point);
                        if (Points.Count == pointCount) type++;
                    }
                    else if (type == 1)  //y 坐标
                    {
                        Points[i / 4 - pointCount].Y = BitConverter.ToSingle(data, i);
                        if (i / 4 == pointCount * 2 - 1) type++;
                    }
                    else if (type == 2)   //z 坐标
                    {
                        Points[i / 4 - pointCount * 2].Z = BitConverter.ToSingle(data, i);
                        if (i / 4 == pointCount * 3 - 1) type++;
                    }
                    else if (size == 4)  //颜色信息
                    {
                        Points[i / 4 - pointCount * 3].Color = BitConverter.ToUInt32(data, i);
                        if (i / 4 == pointCount * 4 - 1) break;
                    }
                }
                */
            }
        }
        
        /// <summary>
        /// 使用LZF算法解压缩数据
        /// </summary>
        /// <param name="input">要解压的数据</param>
        /// <param name="outputLength">解压之后的长度</param>
        /// <returns>返回解压缩之后的内容</returns>
        public static byte[] Decompress(byte[] input, int outputLength)
        {
            uint iidx = 0;
            uint oidx = 0;
            int inputLength = input.Length;
            byte[] output = new byte[outputLength];
            do
            {
                uint ctrl = input[iidx++];

                if (ctrl < (1 << 5))
                {
                    ctrl++;

                    if (oidx + ctrl > outputLength)
                    {
                        return null;
                    }

                    do
                        output[oidx++] = input[iidx++];
                    while ((--ctrl) != 0);
                }
                else
                {
                    var len = ctrl >> 5;
                    var reference = (int)(oidx - ((ctrl & 0x1f) << 8) - 1);
                    if (len == 7)
                        len += input[iidx++];
                    reference -= input[iidx++];
                    if (oidx + len + 2 > outputLength)
                    {
                        return null;
                    }
                    if (reference < 0)
                    {
                        return null;
                    }
                    output[oidx++] = output[reference++];
                    output[oidx++] = output[reference++];
                    do
                        output[oidx++] = output[reference++];
                    while ((--len) != 0);
                }
            }
            while (iidx < inputLength);

            return output;
        }
    } 
    /// <summary>
    /// Pcd 文件的头部信息
    /// </summary>
    struct PCDHeader
    {
        public string FistLine;  //pcd 文件的第一行
        public string VERSION;
        public string FIELDS;
        public string SIZE;
        public string TYPE;
        public string COUNT;
        public string WIDTH;
        public string HEIGHT;
        public string VIEWPOINT;
        public string POINTS;
        public string DATA;
        public long fileSize;   //文件字节大小
        public long dataIndex; //数据开始位置
        public long Count;
        public int Size;
        public int Width;
        public int Height;
    }
}

