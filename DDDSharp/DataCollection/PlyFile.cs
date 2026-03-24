using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GlmNet;
namespace DataCollection
{
    public enum DataTypeEnum
    {
        charType = 0,
        ucharType = 1,
        shortType = 2,
        ushortType = 3,
        intType = 4,
        uintType = 5,
        floatType = 6,
        doubleType = 7,
        stringType = 8,
    };
    public struct PlyProperty
    {
        public string name;
        public DataTypeEnum type;
        public bool IsList;
        public List<byte> pByteArray;
        public List<int> pIntArray;
        public List<float> pFloatArray;
        public List<double> pDoubleArray;
        public int ListPropertyNum;
        public PlyProperty(string _name, DataTypeEnum _type, bool _IsList = false)
        {
            name = _name;
            type = _type;
            IsList = _IsList;
            ListPropertyNum = 0;
            pByteArray = null;
            pIntArray = null;
            pFloatArray = null;
            pDoubleArray = null;
            switch(type)
            {
                case DataTypeEnum.charType:
                case DataTypeEnum.ucharType:
                    pByteArray = new List<byte>();
                    break;
                case DataTypeEnum.shortType:
                case DataTypeEnum.ushortType:
                case DataTypeEnum.intType:
                case DataTypeEnum.uintType:
                    pIntArray = new List<int>();
                    break;
                case DataTypeEnum.floatType:
                    pFloatArray = new List<float>();
                    break;
                case DataTypeEnum.doubleType:
                    pDoubleArray = new List<double>();
                    break;
            }            
        }
        public void Clear()
        {
            if (pByteArray != null) pByteArray.Clear();
            if (pIntArray != null) pIntArray.Clear();
            if (pFloatArray != null) pFloatArray.Clear();
            if (pDoubleArray != null) pDoubleArray.Clear();
        }
        public void AddValue(double value)
        {
            switch (type)
            {
                case DataTypeEnum.charType:
                case DataTypeEnum.ucharType:
                    if (pByteArray != null) pByteArray.Add((byte)value);
                    break;
                case DataTypeEnum.shortType:
                case DataTypeEnum.ushortType:
                case DataTypeEnum.intType:
                case DataTypeEnum.uintType:
                    if (pIntArray != null) pIntArray.Add((int)value);
                    break;
                case DataTypeEnum.floatType:
                    if (pFloatArray != null) pFloatArray.Add((float)value);
                    break;
                case DataTypeEnum.doubleType:
                    if (pDoubleArray != null) pDoubleArray.Add(value);
                    break;
            }
        }
        public double GetValue(int id)
        {
            switch (type)
            {
                case DataTypeEnum.charType:
                case DataTypeEnum.ucharType:
                    if (pByteArray != null) return pByteArray[id];
                    break;
                case DataTypeEnum.shortType:
                case DataTypeEnum.ushortType:
                case DataTypeEnum.intType:
                case DataTypeEnum.uintType:
                    if (pIntArray != null) return pIntArray[id];
                    break;
                case DataTypeEnum.floatType:
                    if (pFloatArray != null) return pFloatArray[id];
                    break;
                case DataTypeEnum.doubleType:
                    if (pDoubleArray != null) return pDoubleArray[id];
                    break;                
            }
            return 0;
        }
        public float GetFloatValue(int id)
        {
            switch (type)
            {
                case DataTypeEnum.charType:
                case DataTypeEnum.ucharType:
                    return pByteArray[id]/255f;                    
                case DataTypeEnum.shortType:
                case DataTypeEnum.ushortType:
                case DataTypeEnum.intType:
                case DataTypeEnum.uintType:
                    return pIntArray[id]/255f;                    
                case DataTypeEnum.floatType:
                    return pFloatArray[id];                    
                case DataTypeEnum.doubleType:
                    return (float)pDoubleArray[id];                    
            }
            return 0;
        }
    }
    public struct PlyElement
    {
        public string name;
        public int row;
        public List<PlyProperty> properties;
       
        public void AddProperty(string _name,DataTypeEnum type,bool _IsList = false)
        {
            properties.Add(new PlyProperty(_name,type, _IsList) );
        }
        public PlyElement(string _name)
        {
            name = _name;
            row = 0;
            properties = new List<PlyProperty>();
        }
        public PlyElement(string _name,int _row)
        {
            name = _name;
            row = _row;
            properties = new List<PlyProperty>();
        }
        public void Clear()
        {
            name = "untitled";
            row = 0;
            properties.Clear();
        }
    }
    

    public class PlyFile : C3DObjectBase
    {
        [DllImport("PlyReader.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern int ReadPlyFile(char[] path,
                out int vertno,
                out int normal_no,
                out int color_no,
                out int faceno,
                out int uv_no);
        [DllImport("PlyReader.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern int GetPlyData(float[] _verts, float[] _norms, char[] _colors, int[] _faces, float[] _uvs);

        public int vertno, normal_no, color_no, faceno, uv_no;
        public float[] vertices = null;
        public float[] normals = null;
        public float[] colors = null;
        public int[] faces = null;
        public float[] uvs = null;        
        
        public string errMessage = "";
        
        //used while colors == null or IsUniforColor == true
        public bool IsUniforColor = false;
        public vec4 color = new vec4(1, 1, 1, 1);

        public bool IsUVTexcFormat = true; //U V 格式 or S T格式
        public PlyFile()
        {
            minx = miny = minz = 0;
            maxx = maxy = maxz = 0;
            vertno = normal_no = 0;
            color_no = faceno = uv_no = 0;
            type = ShapeEnum.Mesh;
        }        
        public TriangleObj toTriangleObj()
        {
            int np = vertices.Length;
            int nface = faces.Length;
            if (np < 1 || nface < 3) return null;

            TriangleObj obj = new TriangleObj();
            obj.minx = minx;
            obj.miny = miny;
            obj.minz = minz;
            obj.minv = minv;
            obj.maxx = maxx;
            obj.maxy = maxy;
            obj.maxz = maxz;
            obj.maxv = maxv;
            obj.scale = scale;
            obj.rotate = rotate;
            obj.offset = offset;
            obj.Name = Name;
            obj.textureStruct = textureStruct.Copy();

            for (int i=0;i<np/3;i++)
            {
                obj.points.Add(new Vector32(vertices[3*i], vertices[3 * i+1], vertices[3 * i+2]));
            }
            for (int i = 0; i < nface / 3; i++)
            {
                obj.triangles.Add(new Int32XYZ(faces[3 * i], faces[3 * i + 1], faces[3 * i + 2]));
            }

            obj.IsUniformColor = true;
            if( color_no > 0 )
            {
                obj.IsUniformColor = false;
                float r,g,b;
                for (int i = 0; i < color_no / 3; i++)
                {
                    r = colors[3 * i];
                    g = colors[3 * i+1];
                    b = colors[3 * i+2];                    
                    obj.AddPointColor(r,g,b);
                }
            }

            //U,V,S,T纹理坐标
            if ( uv_no > 0 )
            {
                for(int i=0;i<uv_no/2;i++)
                {
                    obj.texCoords.Add(new vec2(uvs[2*i], uvs[2 * i+1]));
                }
            }

            //uv coord -> each faces 
                /*
                if (uv_no > 0)
                {
                    for (int i = 0; i < obj.points.Count; i++)
                    {
                        obj.texCoords.Add(new vec2(-1, -1));
                    }
                    int id;
                    for (int i = 0; i < nface ; i++)
                    {
                        id = faces[i];
                        obj.texCoords[id] = new vec2(uvs[2 * i], uvs[2 * i + 1]);
                    }
                }           
                */
                obj.UpdateRange();

            if (IsUniforColor || colors == null)
            { 
                obj.IsUniformColor = true;
                obj.uniformColor = System.Drawing.Color.FromArgb((int)(255*color.w), (int)(255*color.x), (int)(255*color.y),(int)(255*color.z) );
            }

            obj.toFloatColors();

            return obj;
        }
        
        public override bool SaveAs(string path,int version = 0)
        {
            if (vertno < 3 || faceno < 3)
            {
                errMessage = "vertics and faces are not enough";
                return false;
            }
            try
            {
                StringBuilder builder = new StringBuilder();
                File.WriteAllText(path, builder.ToString());
                builder.AppendLine("ply");
                builder.AppendLine("format ascii 1.0");
                builder.AppendLine("comment name " + Name);
                
                //颜色
                if (colors == null || colors.Length < 1 )
                {
                    builder.AppendLine("comment color " + color.x + ","+ color.y + "," + color.z);
                }

                builder.AppendLine("element vertex " + vertno / 3 );

                builder.AppendLine("property float32 x");
                builder.AppendLine("property float32 y");
                builder.AppendLine("property float32 z");
                if (colors != null && colors.Length > 0)
                {
                    builder.AppendLine("property float32 red");
                    builder.AppendLine("property float32 green");
                    builder.AppendLine("property float32 blue");
                }

                builder.AppendLine("element face " + faceno / 3);
                builder.AppendLine("property list uint8 int32 vertex_indices");
                builder.AppendLine("end_header");

                string line;
                for (int i = 0; i < vertno / 3; i++)
                {
                    line = vertices[3 * i] + " " + vertices[3 * i + 1] + " " + vertices[3 * i + 2];
                    if (color_no > 0)line += " " + colors[3 * i] + " " + colors[3 * i + 1] + " " + colors[3 * i + 2];
                    builder.AppendLine(line);
                }
                for (int i = 0; i < faceno / 3; i++)
                {
                    builder.AppendLine("3 " + faces[3 * i] + " "+ faces[3 * i + 1] + " " + faces[3 * i + 2]);
                }
                File.WriteAllText(path, builder.ToString());
                builder.Clear();
                builder = null;
            }
            catch(Exception e)
            {
                errMessage = "writing to file faild!\n" + e.Message;
                return false;
            }
            
            return true;
        }
        private string SeekSection(string section, ref StreamReader sr)
        {
            string str;
            while ((str = sr.ReadLine()) != null)
            {
                str = str.Trim(' ');
                if (str.Length < 1) continue;
                if (str == section) return str;
            }
            return null;
        }
        private string SeekSectionContain(string section, ref StreamReader sr)
        {
            string str;            
            while ((str = sr.ReadLine()) != null)
            {
                str = str.Trim(' ');

                if (str.Length < 1) continue;

                if (str.Contains(section))
                    return str;
            }
            return null;
        }
        private string GetLineValue(string str, string name)
        {
            string[] ss = str.Split(new Char[] { '=', '=' }, 2);
            if (ss.Length < 2) return "";

            string s1 = ss[0].Trim(' ');
            if (s1.ToLower() != name.ToLower()) return "";

            return ss[1].Trim(' ');
        }

        private DataTypeEnum toDataType(string type)
        {
            string ss = type;
            ss.ToLower();
            if (ss.Contains("char")) return DataTypeEnum.charType;
            //if (ss == "uchar") return DataTypeEnum.ucharType;
            if (ss.Contains("short")) return DataTypeEnum.shortType;
            //if (ss == "ushort") return DataTypeEnum.ushortType;
            if (ss.Contains("int")) return DataTypeEnum.intType;
            //if (ss == "uint") return DataTypeEnum.uintType;
            if (ss.Contains("float")) return DataTypeEnum.floatType;
            if (ss.Contains("double")) return DataTypeEnum.doubleType;
            return DataTypeEnum.stringType;
        }
        public List<PlyElement> elements = new List<PlyElement>();
        private vec4 GetColorFromComment(string str)
        {
            color = new vec4(0.8f, 0.8f, 0.8f, 1.0f);
            string[] ss = str.Split(new char[] { ' ', '\t', ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length == 3) //comment Color: -16666880
            {
                System.Drawing.Color c = System.Drawing.Color.FromArgb(int.Parse(ss[2]));
                color.x = c.R / 255f;
                color.y = c.G / 255f;
                color.z = c.B / 255f;
                color.w = c.A / 255f;
            }
            else if (ss.Length == 5) //comment Color: 255,128,20
            {
                float r, g, b;
                if (!float.TryParse(ss[2], out r)) return color;
                if (!float.TryParse(ss[3], out g)) return color;
                if (!float.TryParse(ss[4], out b)) return color;
                if( r > 1 || g > 1 || b >1 )
                {
                    color.x = r/255f;
                    color.y = g / 255f;
                    color.z = b / 255f;
                    color.w = 1;
                }
                else
                {
                    color.x = r;
                    color.y = g;
                    color.z = b;
                    color.w = 1;
                }
            }

            IsUniforColor = true;

            return color;
        }
        private string GetNameFromComment(string str)
        {
            Name = "";
            string[] ss = str.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > 2) Name = ss[2];
            return Name;
        }
        private string GetTextureFileFromComment(string str)
        {
            string textureImgFile = "";
            string str1 = str.ToLower();
            string ss1;
            string[] ss = str1.Split(new char[] {' ','\t',',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] imgs = new string[] { "jpg", "jpeg","png", "bmp","gif","tif","pcx" };
            for(int i=0;i<ss.Length;i++)
            {
                ss1 = ss[i].Trim(new char[] {' ','\'','"' });
                for (int j = 0; j < imgs.Length; j++)
                {
                    if ( ss[i].Contains(imgs[j]) )
                    {
                        textureImgFile = ss[i];
                        return textureImgFile;
                    }
                }
            }            
            return textureImgFile;
        }
        private bool ReadHeader(ref StreamReader sr)
        {
            string str,str1;
            string[] ss;
            int no;
            PlyElement el = new PlyElement("untitled");
            elements.Clear();
            while ((str = sr.ReadLine()) != null)
            {
                str = str.Trim();
                if (str.Length < 1) continue;

                //comment texture ....
                //comment name ....
                //comment color....
                if (str.Contains("comment"))
                {
                    str1 = str.ToLower();
                    if (str1.Contains("texture")) textureStruct.TextureFile = GetTextureFileFromComment(str);
                    else if(str1.Contains("name")) Name = GetNameFromComment(str);
                    else if (str1.Contains("color")) color = GetColorFromComment(str);
                }
                else if (str.Contains("element"))
                {
                    ss = str.Split(new Char[] { ' ', ' ' }, 3);
                    if( ss.Length < 2 )
                    {
                        errMessage = "wrong element:" + str;                        
                        return false;
                    }
                    //deal with the last element
                    if (el.properties.Count > 0)
                    {
                        elements.Add(el);
                       // el.Clear();
                    }

                    //new element
                    no = Convert.ToInt32(ss[2]);                    
                    el = new PlyElement(ss[1],no);
                }
                if (str.Contains("property"))
                {
                    ss = str.Split(new Char[] { ' ', ' ' }, 5);
                    if (ss.Length < 2) continue; //wrong property

                    if( ss[1].Contains("list") && ss.Length == 5)
                    {
                        //property list uchar float texcoord
                        el.AddProperty(ss[4], toDataType(ss[3]),true);
                    }
                    else el.AddProperty(ss[2], toDataType(ss[1]),false);
                }
                if (str.Contains("end_header"))
                {
                    if( el.properties.Count > 0 )
                    {
                        elements.Add(el);
                        //el.Clear();
                    }
                    break;
                }
            }//while ((str = sr.ReadLine()) != null)
            if (elements.Count > 0) return true;
            else return false;
        }

        private bool ReadElementData(ref PlyElement el,ref StreamReader sr)
        {
            string str;
            string[] ss;            
            int i,j,k;
            PlyProperty property;            
            int ni,cur;
            for ( k = 0; k < el.row; )
            {
                str = sr.ReadLine();
                if (str == null)return false;                
                str = str.Trim();
                if (str.Length < 1) continue;

                ss = str.Split(new Char[] { ' ', ' ' },  Math.Max(20, el.properties.Count) );

                cur = 0;
                for (i = 0; i < el.properties.Count; i++)
                {
                    property = el.properties[i];

                    if (property.IsList)
                    {
                        ni = Convert.ToInt32(ss[cur++]);
                        property.ListPropertyNum = ni;
                        for (j = 0; j < ni; j++)
                            property.AddValue(Convert.ToDouble(ss[cur++]));
                    }
                    else property.AddValue(Convert.ToDouble(ss[cur++]));
                    
                    el.properties[i] = property;
                }

                k++;
            }
            return true;
        }
        private bool GetVertexProperties(PlyElement el)
        {
            vertices = null;
            vertno = 0;
            normals = null;
            normal_no = 0;
            colors = null;
            color_no = 0;
            uv_no = 0;
            uvs = null;
            int nproperties = el.properties.Count;
            if (nproperties < 3)
            {
                errMessage = "no enough coordinates' information.";
                return false;
            }
            vertno = 3 * el.row;
            vertices = new float[vertno];
            for (int i = 0; i < el.row; i++)
            {
                vertices[3 * i] = (float)el.properties[0].GetValue(i);
                vertices[3 * i + 1] = (float)el.properties[1].GetValue(i);
                vertices[3 * i + 2] = (float)el.properties[2].GetValue(i);
            }

            int u1 = -1;
            int v1 = -1;
            int r1 = -1;
            int g1 = -1;
            int b1 = -1;

            for ( int i = 3; i < nproperties; i++ )
            {
                string pname = el.properties[i].name.ToLower();

                if (pname == "u") { u1 = i; IsUVTexcFormat = true; }
                else if (pname == "v") { u1 = i; IsUVTexcFormat = true; }
                else if (pname == "s") { u1 = i; IsUVTexcFormat = false; }
                else if (pname == "t") { u1 = i; IsUVTexcFormat = false; }
                else if ( pname == "red"   || pname == "r" ) r1 = i;
                else if ( pname == "green" || pname == "g" ) g1 = i;
                else if ( pname == "blue"  || pname == "b" ) b1 = i;                
            }
           
            //   0---->U  T
            //   |        |
            //   V        0----->S

            if( u1 > -1 && v1 > -1 )
            {
                uv_no = 2 * el.row;
                uvs = new float[uv_no];
                for (int i = 0; i < el.row; i++)
                {
                    uvs[2 * i] = (float)el.properties[u1].GetValue(i);
                    uvs[2 * i+1] = (float)el.properties[v1].GetValue(i);
                    if (!IsUVTexcFormat) uvs[2 * i + 1] = 1 - uvs[2 * i + 1];
                }
            }
           
            //颜色转换成float 格式
            if ( r1 > -1 && g1 > -1 && b1 > -1 )
            {
                color_no = 3 * el.row;
                colors = new float[color_no];
                for (int i = 0; i < el.row; i++)
                {
                    colors[3 * i] = el.properties[r1].GetFloatValue(i);
                    colors[3 * i+1] = el.properties[g1].GetFloatValue(i);
                    colors[3 * i+2] = el.properties[b1].GetFloatValue(i);
                }
            }
            
            return true;
        }
        
        private bool GetFaceProperties(PlyElement el)
        {
            int n1 = 0, n2 = 0;
            if (el.properties.Count > 0) n1 = el.properties[0].ListPropertyNum;
            if (el.properties.Count > 1) n2 = el.properties[1].ListPropertyNum;
            if (n1 > 0)
            {
                faceno = 3 * elements[1].row;
                faces = new int[faceno];
            }
            if (n2 > 0 && uv_no ==0)
            {
                uv_no = 2 * faceno;
                uvs = new float[uv_no];
            }
            for (int i = 0; i < el.row; i++)
            {
                if (n1 > 0)
                {
                    faces[n1 * i] = (int)el.properties[0].GetValue(n1 * i);
                    faces[n1 * i + 1] = (int)el.properties[0].GetValue(n1 * i + 1);
                    faces[n1 * i + 2] = (int)el.properties[0].GetValue(n1 * i + 2);
                }
                if (n2 > 0)
                {
                    uvs[n2 * i] = (float)el.properties[1].GetValue(n2 * i);
                    uvs[n2 * i + 1] = (float)el.properties[1].GetValue(n2 * i + 1);
                    uvs[n2 * i + 2] = (float)el.properties[1].GetValue(n2 * i + 2);
                    uvs[n2 * i + 3] = (float)el.properties[1].GetValue(n2 * i + 3);
                    uvs[n2 * i + 4] = (float)el.properties[1].GetValue(n2 * i + 4);
                    uvs[n2 * i + 5] = (float)el.properties[1].GetValue(n2 * i + 5);
                }
            }
            return true;
        }
        public bool LoadAscII(string path)
        {
            elements.Clear();

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs,Encoding.UTF8);            
            
            if (!ReadHeader(ref sr)) return false;
            PlyElement el;
            for(int i=0;i<elements.Count;i++)
            {
                el = elements[i];
                if(!ReadElementData(ref el, ref sr))
                {
                    errMessage = "data not correct.";
                    sr.Close();
                    fs.Close();
                    return false;
                }
                elements[i] = el;
            }
            sr.Close();
            fs.Close();

            if( !GetVertexProperties(elements[0]))
            {
                return false;
            }
            if( !GetFaceProperties(elements[1]))
            {
                return false;
            }
            
            UpdateRange();          

            return true;
        }
        
        public bool LoadBinary(string path)
        {
            try 
            {
                int ret = ReadPlyFile(path.ToCharArray(), out vertno, out normal_no, out color_no, out faceno, out uv_no);
                if (ret < 1)
                {
                    errMessage = "Load PLY file failed.";
                    vertno = normal_no = 0;
                    color_no = faceno = uv_no = 0;
                    return false;
                }

                vertices = new float[vertno];
                normals = new float[1];                
                faces = new int[faceno];
                uvs = new float[uv_no];
                
                char[] _colors = new char[1];                
                ret = GetPlyData(vertices, normals, _colors, faces, uvs);
                if (ret < 1)
                {
                    errMessage = "Load PLY file failed.";
                    vertno = normal_no = 0;
                    color_no = faceno = uv_no = 0;
                    return false;
                }
                if (_colors != null && _colors.Length > 1)
                {
                    colors = new float[ _colors.Length ];
                    for (int i = 0; i < _colors.Length; i++)
                    {
                        colors[i] = _colors[i] / 255f;
                    }
                    _colors = null;
                }
                UpdateRange();                
             
                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                vertno = normal_no = 0;
                color_no = faceno = uv_no = 0;
                return false;
            }            
        }
        public override bool LoadFrom(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            if (SeekSection("ply", ref sr) == null)
            {
                sr.Close();
                fs.Close();
                errMessage = "not a valid ply file.";
                return false;
            }
            string str = SeekSectionContain("format", ref sr);
            sr.Close();
            fs.Close();

            if (str == null)
            {                
                errMessage = "not a valid ply file.";
                return false;
            }

            if (str.Contains("ascii"))
            {
                return LoadAscII(path);
            }
            else return LoadBinary(path);

        }
        public override void UpdateRange()
        {
            float x, y, z;
            for (int i = 0; i < vertno / 3; i++)
            {
                x = vertices[3 * i];
                y = vertices[3 * i + 1];
                z = vertices[3 * i + 2];
                if (i == 0)
                {
                    minx = maxx = x;
                    miny = maxy = y;
                    minz = maxz = z;
                }
                else
                {
                    if (x < minx) minx = x;
                    if (y < miny) miny = y;
                    if (z < minz) minz = z;
                    if (x > maxx) maxx = x;
                    if (y > maxy) maxy = y;
                    if (z > maxz) maxz = z;
                }
            }
        }
        public override void Normalize()
        {
            if (vertno < 3 || faceno < 3) return;
            TriangleObj obj =  toTriangleObj();
            obj.Normalize();
            fromTriangle(obj);
        }
        public override void Clear()
        {
            vertices = null;
            normals = null;
            colors = null;
            faces = null;
            uvs = null;
            textureStruct.TextureFile = "";
            vertno = normal_no = 0;
            color_no = faceno = uv_no = 0;
        }
        void fromTriangle( TriangleObj obj )
        {
            vertno = 3*obj.points.Count;
            faceno = 3*obj.triangles.Count;
            vertices = new float[vertno];
            faces = new int[faceno];
            for(int i=0;i< obj.points.Count;i++)
            {
                vertices[3 * i] = obj.points[i].X;
                vertices[3 * i+1] = obj.points[i].Y;
                vertices[3 * i+2] = obj.points[i].Z;
            }
            for (int i = 0; i < obj.triangles.Count; i++)
            {
                faces[3 * i] = obj.triangles[i].x;
                faces[3 * i + 1] = obj.triangles[i].y;
                faces[3 * i + 2] = obj.triangles[i].z;
            }
        }        
    }
}
