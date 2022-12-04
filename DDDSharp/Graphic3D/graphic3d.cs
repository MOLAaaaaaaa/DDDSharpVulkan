////////////////////////////////////////////////////////////////////////////////
//  Graphics 3D Base Class
//  Created By jiansir 2017
//  Modified History
//  2021-11-10 , modified the shader , add TexturedText class, support the Text by texture修改Shader，增加了透明貼圖的文字功能
//  2021-11-10,  modified the bitmap to bytes function,optimized the  speed.修改圖像到字節的轉換，支持圖像BGRA逆序，無需順序轉換，提高效率
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DataCollection;
using GlmNet;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace Graphics3D
{
    public enum gEngine
    {
        auto = 0,
        vulkan = 1,
        opengl = 2,
        opengles = 3,
    };
    public enum gDrawMode
    {
        Points =0, Wireframe = 1, Fill = 2
    }
    public enum gLineStyle
    {        
        Solid = 0, Dash = 1, Dot = 2,DashDot = 3, DashDotDot = 4,
    }    
    public struct DrawParameter
    {
        public gDrawMode polygonMode;
        public float lineWidth;
        public float pointSize;
        public gLineStyle lineStyle;
        vec4 color;
        public DrawParameter(bool init)
        {            
            polygonMode = gDrawMode.Fill;
            lineWidth = 1.0f;
            pointSize = 1.0f;
            color = new vec4(0,0,0,1);
            lineStyle = gLineStyle.Solid; 
        }
    };
    public enum VertexType
    {
        Vertex2D = 1,
        Vertex3D = 2
    };
    
    public enum DrawingPrimitive
    {
        POINT_LIST = 0,
        LINE_LIST = 1,
        LINE_STRIP = 2,
        TRIANGLE_LIST = 3,
        TRIANGLE_STRIP = 4,
        TRIANGLE_FAN = 5,
        LINE_LIST_WITH_ADJACENCY = 6,
        LINE_STRIP_WITH_ADJACENCY = 7,
        TRIANGLE_LIST_WITH_ADJACENCY = 8,
        TRIANGLE_STRIP_WITH_ADJACENCY = 9,
        PATCH_LIST = 10
    }
    public enum CullModeEnum
    {
        NONE = 0,
        FRONT = 1,
        BACK = 2,
        FRONT_AND_BACK = 3
    }
    public enum FrontFaceOder
    {
        COUNTER_CLOCKWISE = 0,
        CLOCKWISE = 1
    }
    public class GraphicDeviceInfo
    {
        public string Name = "";
        public double MemorySize = 0;   //MB
        public string InstalledDisplayDrivers = "";
        public string DriverVersion = "";
        public GraphicDeviceInfo Copy()
        {
            GraphicDeviceInfo d = new GraphicDeviceInfo();
            d.Name = Name;
            d.MemorySize = MemorySize;
            d.InstalledDisplayDrivers = InstalledDisplayDrivers;
            d.DriverVersion = DriverVersion;
            return d;
        }
        public string toString()
        {
            string info = "";
            info += "Name:" + Name + "\n";
            info += "MemorySize:" + MemorySize + "MB" + "\n";
            info += "Driver:" + DriverVersion + "\n";
            return info;
        }
    }
    public struct UniformBufferObject
    {
        public mat4 model;
        public mat4 view;
        public mat4 proj;
        public vec4 eyepos;
        public MaterialStruct material;
        public LightStruct[] lights;
        public int lightsNum 
        {
            get 
            {
                if (lights == null || lights.Length < 1) return 0;
                int count = 0;
                for( int i = 0; i < lights.Length;i++ )
                {
                    if (lights[i].Enable) count++;
                }
                return count;
            }
        }
        public mat4 normal
        {
            get
            {
                return Transpose(glm.inverse(model));
            }
        }
        public UniformBufferObject(bool autoinit = true)
        {
            model = new mat4(new vec4(), new vec4(), new vec4(), new vec4());
            //normal = new mat4(new vec4(), new vec4(), new vec4(), new vec4());
            view = new mat4(new vec4(), new vec4(), new vec4(), new vec4());
            proj = new mat4(new vec4(), new vec4(), new vec4(), new vec4());
            eyepos = new vec4(0, 0, 0, 1);
            lights = new LightStruct[8];
            material = new MaterialStruct(true);

            for (int i = 0; i < 8; i++)
                lights[i] = new LightStruct(true);

            //enable default light0
            lights[0].Enable = true;
        }
        public void Clear()
        {
            lights = null;
        }
        private int CopyToBytes(float[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(float source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(int[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(int source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(bool[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(bool source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(byte[] source, byte[] dest, int start)
        {
            int index = start;
            Array.Copy(source, 0, dest, index, source.Length);
            index += source.Length;
            return index;
        }
        public byte[] toByteArray()
        {
            int size = GetSize();
            byte[] _array = new byte[size];
            int index = 0;
            index = CopyToBytes(model.to_array(), _array, index);    //mat4 model
            index = CopyToBytes(normal.to_array(), _array, index);  //mat4 normal
            index = CopyToBytes(view.to_array(), _array, index);    //mat4 view
            index = CopyToBytes(proj.to_array(), _array, index);    //mat4 proj
            index = CopyToBytes(eyepos.to_array(), _array, index);  //vec4 eyepos
            index = CopyToBytes(material.toByteArray(), _array, index); //material
            for (int i = 0; i < lights.Length; i++)                 //vec3 lightpos[8]
            {
                index = CopyToBytes(lights[i].toByteArray(), _array, index);
            }
            return _array;
        }
        public float[] toFloatArray()
        {
            int size = GetSize() / sizeof(float);
            float[] _array = new float[size];
            int index = 0;

            //normal matrix is the inverse and transpose of model
            //normal = Transpose(glm.inverse(model));

            float[] a0 = model.to_array();
            float[] a1 = normal.to_array();
            float[] a2 = view.to_array();
            float[] a3 = proj.to_array();
            float[] a4 = eyepos.to_array();
            float[] a5 = material.toFloatArray();

            Array.Copy(a0, 0, _array, index, a0.Length);
            index += a0.Length;
            Array.Copy(a1, 0, _array, index, a1.Length);
            index += a1.Length;
            Array.Copy(a2, 0, _array, index, a2.Length);
            index += a2.Length;
            Array.Copy(a3, 0, _array, index, a3.Length);
            index += a3.Length;
            Array.Copy(a4, 0, _array, index, a4.Length);
            index += a4.Length;
            Array.Copy(a5, 0, _array, index, a5.Length);
            index += a5.Length;
            for (int i = 0; i < 8; i++)
            {
                float[] a6 = lights[i].toFloatArray();
                Array.Copy(a6, 0, _array, index, a6.Length);
                index += a6.Length;
            }

            return _array;
        }
        public mat4 Transpose(mat4 m)
        {
            mat4 a = new mat4(new vec4(), new vec4(), new vec4(), new vec4());
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    a[i, j] = m[j, i];
            return a;
        }
        public bool LoadFrom(BinaryReader br)
        {
            try
            {
                model = new mat4(new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0));
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        model[i, j] = br.ReadSingle();

                view = new mat4(new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0));
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        view[i, j] = br.ReadSingle();

                proj = new mat4(new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0));
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                         proj[i, j] = br.ReadSingle();

                eyepos = new vec4(0, 0, 0, 0);
                eyepos.x = br.ReadSingle();
                eyepos.y = br.ReadSingle();
                eyepos.z = br.ReadSingle();
                eyepos.w = br.ReadSingle();

                material = new MaterialStruct(true);
                material.LoadFrom(br);
                lights = new LightStruct[8];
                for (int i = 0; i < 8; i++)
                {
                    lights[i] = new LightStruct(true);
                    lights[i].LoadFrom(br);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool SaveAs(BinaryWriter br)
        {
            try 
            {
                UniformBufferObject p = new UniformBufferObject(true);
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        br.Write(model[i, j]);

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        br.Write(view[i, j]);

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        br.Write(proj[i, j]);

                br.Write(eyepos.x);
                br.Write(eyepos.y);
                br.Write(eyepos.z);
                br.Write(eyepos.w);
                material.SaveAs(br);
                for (int i = 0; i < 8; i++)
                {
                    lights[i].SaveAs(br);
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
            
        }
        public UniformBufferObject Copy()
        {
            UniformBufferObject p = new UniformBufferObject(true);
            int i, j;

            p.model = new mat4(new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0));
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    p.model[i, j] = model[i, j];

            p.view = new mat4(new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0)); ;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    p.view[i, j] = view[i, j];

            //normal matrix is the inverse and Transpose of view            
            //p.normal = Transpose(glm.inverse(model));

            p.proj = new mat4(new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0), new vec4(0, 0, 0, 0));
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++)
                    p.proj[i, j] = proj[i, j];

            p.eyepos = new vec4(eyepos.x, eyepos.y, eyepos.z, eyepos.w);
            p.material = material.Copy();
            for (i = 0; i < 8; i++)
            {
                p.lights[i] = lights[i].Copy();
            }

            /*
            vec4 v1 = new vec4(model[0].x, model[0].y, model[0].z, model[0].w);
            vec4 v2 = new vec4(model[1].x, model[1].y, model[1].z, model[1].w);
            vec4 v3 = new vec4(model[2].x, model[2].y, model[2].z, model[2].w);
            vec4 v4 = new vec4(model[3].x, model[3].y, model[3].z, model[3].w);
            p.model = new mat4(v1, v2, v3, v4);

            v1 = new vec4(view[0].x, view[0].y, view[0].z, view[0].w);
            v2 = new vec4(view[1].x, view[1].y, view[1].z, view[1].w);
            v3 = new vec4(view[2].x, view[2].y, view[2].z, view[2].w);
            v4 = new vec4(view[3].x, view[3].y, view[3].z, view[3].w);
            p.view = new mat4(v1, v2, v3, v4);

            v1 = new vec4(normal[0].x, normal[0].y, normal[0].z, normal[0].w);
            v2 = new vec4(normal[1].x, normal[1].y, normal[1].z, normal[1].w);
            v3 = new vec4(normal[2].x, normal[2].y, normal[2].z, normal[2].w);
            v4 = new vec4(normal[3].x, normal[3].y, normal[3].z, normal[3].w);            
            p.normal = new mat4(v1, v2, v3, v4);

            v1 = new vec4(proj[0].x, proj[0].y, proj[0].z, proj[0].w);
            v2 = new vec4(proj[1].x, proj[1].y, proj[1].z, proj[1].w);
            v3 = new vec4(proj[2].x, proj[2].y, proj[2].z, proj[2].w);
            v4 = new vec4(proj[3].x, proj[3].y, proj[3].z, proj[3].w);
            p.proj = new mat4(v1, v2, v3, v4);

            p.eyepos = eyepos;
            p.material = material.Copy();
            for (int i=0;i<8;i++)
            {
                p.lights[i] = lights[i].Copy();                
            }
            */

            return p;
        }
        public static int GetSize()
        {
            return Marshal.SizeOf<vec4>() * 4 * 4 + //mat4 model,normal,view,proj
                   Marshal.SizeOf<vec4>() +         //vec4 eyepos;  
                   LightStruct.GetSize() * 8 +     //LightStruct []lights;
                   MaterialStruct.GetSize();       //MaterialStruct material;
        }
    };
    public class MyGLModel : CModel
    {
        public float[] vertices = null;
        public int[] indices = null;
        public float[]  normals = null;
        public float[]  colors = null;
        public float[]  texcoords = null;

        public gDrawMode polygonMode = gDrawMode.Fill;

        public float lineWidth = 1.0f;
        public gLineStyle lineStyle = gLineStyle.Solid;

        public Bitmap texBitmap = null;
        public byte[] textureBytes = null;
        public TextureMagFilter textureMode = TextureMagFilter.GL_LINEAR;
        public int texWidth = 0;
        public int texHeight = 0;       
        
        public MyGLModel(DrawingPrimitive _primitive)
        {
            primitive = _primitive;
            CreateHashKey();
        }
        public override long CreateHashKey()
        {
            HashKey = this.GetHashCode();
            return HashKey;
        }
        public bool CreateVerticesBuffer(Vertex3D[] points, bool _enableTexture = false)
        {
            bool isTriangle = IsTriangleMode(primitive);

            try
            {
                vertices = new float[points.Length * 3]; //顶点                                
                colors = new float[points.Length * 4]; //颜色
                for (int i = 0; i < points.Length; i++)
                {
                    vertices[3 * i] = points[i].pos.x;
                    vertices[3 * i + 1] = points[i].pos.y;
                    vertices[3 * i + 2] = points[i].pos.z;
                    colors[4 * i] = points[i].color.x;
                    colors[4 * i + 1] = points[i].color.y;
                    colors[4 * i + 2] = points[i].color.z;
                    colors[4 * i + 3] = points[i].color.w;
                }
                /////////////////////////////////////
                texcoords = new float[points.Length * 2];//纹理                
                if (_enableTexture)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        texcoords[2 * i] = points[i].texCoord.x;
                        texcoords[2 * i + 1] = points[i].texCoord.y;
                    }
                }
                else
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        texcoords[2 * i] = -1;
                        texcoords[2 * i + 1] = -1;
                    }
                }
                /////////////////////////////////////
                if (isTriangle)
                {
                    normals = new float[points.Length * 3]; //法线
                    for (int i = 0; i < points.Length; i++)
                    {
                        normals[3 * i] = points[i].normal.x;
                        normals[3 * i + 1] = points[i].normal.y;
                        normals[3 * i + 2] = points[i].normal.z;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                errMessage = "Create Vertics Buffer Failed." + e.Message;
                Clear();
                return false;
            }
        }
        public bool CreateVerticesBuffer(List<Vertex3D> points, bool _enableTexture = false)
        {
            bool isTriangle = IsTriangleMode(primitive);

            try
            {
                vertices = new float[points.Count * 3]; //顶点
                colors = new float[points.Count * 4]; //颜色
                for (int i = 0; i < points.Count; i++)
                {
                    vertices[3 * i] = points[i].pos.x;
                    vertices[3 * i + 1] = points[i].pos.y;
                    vertices[3 * i + 2] = points[i].pos.z;
                    colors[4 * i] = points[i].color.x;
                    colors[4 * i + 1] = points[i].color.y;
                    colors[4 * i + 2] = points[i].color.z;
                    colors[4 * i + 3] = points[i].color.w;
                }
                /////////////////////////////////////
                texcoords = new float[points.Count * 2];//纹理                
                if (_enableTexture)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        texcoords[2 * i] = points[i].texCoord.x;
                        texcoords[2 * i + 1] = points[i].texCoord.y;
                    }
                }
                else
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        texcoords[2 * i] = -1;
                        texcoords[2 * i + 1] = -1;
                    }
                }
                /////////////////////////////////////
                if (isTriangle)
                {
                    normals = new float[points.Count * 3]; //法线
                    for (int i = 0; i < points.Count; i++)
                    {
                        normals[3 * i] = points[i].normal.x;
                        normals[3 * i + 1] = points[i].normal.y;
                        normals[3 * i + 2] = points[i].normal.z;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                errMessage = "Create Vertics Buffer Failed." + e.Message;
                Clear();
                return false;
            }
        }

        public bool CreateIndicesBuffer(int[] _indices)
        {
            try
            {
                indices = new int[_indices.Length];
                for (int i = 0; i < _indices.Length; i++)
                    indices[i] = _indices[i];
                return true;
            }
            catch (Exception e)
            {
                errMessage = "Create Indices Buffer Failed." + e.Message;
                Clear();
                return false;
            }
        }
        public bool CreateIndicesBuffer(List<int> _indices)
        {
            try
            {
                indices = new int[_indices.Count];
                for (int i = 0; i < _indices.Count; i++)
                    indices[i] = _indices[i];
                return true;
            }
            catch (Exception e)
            {
                errMessage = "Create Indices Buffer Failed." + e.Message;
                Clear();
                return false;
            }
        }

        vec3 Normalize(vec3 p)
        {
            vec3 p0 = new vec3(p.x, p.y, p.z);
            double d = Math.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
            if (d > 0)
            {
                p0.x = (float)(p.x / d);
                p0.y = (float)(p.y / d);
                p0.z = (float)(p.z / d);
            }
            return p0;
        }

        vec3 AddNormal(vec3 p1, vec3 p2)
        {
            vec3 v1 = Normalize(p1);
            vec3 v2 = Normalize(p2);
            vec3 v0 = new vec3((p1.x + p2.x) / 2, (p1.y + p2.y) / 2, (p1.z + p2.z) / 2);
            return v0;
        }
        //计算法向量
        public bool CreateNormalBuffer()
        {
            if (vertices == null) return false;
            if (indices == null) return false;
            if (!IsTriangleMode(primitive)) return false;

            try
            {
                int np = vertices.Length / 3;
                int nd = indices.Length / 3;
                if (np < 3 || nd < 1) return false;
                normals = new float[3 * np];
                for (int i = 0; i < 3 * np; i++) normals[i] = 0;

                int id1, id2, id3;
                vec3 p1 = new vec3();
                vec3 p2 = new vec3();
                vec3 p3 = new vec3();
                vec3 nor;
                vec3 p0 = new vec3();
                for (int i = 0; i < nd; i++)
                {
                    id1 = indices[3 * i];
                    id2 = indices[3 * i + 1];
                    id3 = indices[3 * i + 2];
                    p1.x = vertices[3 * id1];
                    p1.y = vertices[3 * id1 + 1];
                    p1.z = vertices[3 * id1 + 2];
                    p2.x = vertices[3 * id2];
                    p2.y = vertices[3 * id2 + 1];
                    p2.z = vertices[3 * id2 + 2];
                    p3.x = vertices[3 * id3];
                    p3.y = vertices[3 * id3 + 1];
                    p3.z = vertices[3 * id3 + 2];

                    nor = CGraphic3D.GetNormal(p1, p2, p3);
                    p0.x = normals[3 * id1];
                    p0.y = normals[3 * id1 + 1];
                    p0.z = normals[3 * id1 + 2];
                    p0 = AddNormal(p0, nor);
                    normals[3 * id1] = p0.x;
                    normals[3 * id1 + 1] = p0.y;
                    normals[3 * id1 + 2] = p0.z;

                    p0.x = normals[3 * id2];
                    p0.y = normals[3 * id2 + 1];
                    p0.z = normals[3 * id2 + 2];
                    p0 = AddNormal(p0, nor);
                    normals[3 * id2] = p0.x;
                    normals[3 * id2 + 1] = p0.y;
                    normals[3 * id2 + 2] = p0.z;

                    p0.x = normals[3 * id3];
                    p0.y = normals[3 * id3 + 1];
                    p0.z = normals[3 * id3 + 2];
                    p0 = AddNormal(p0, nor);
                    normals[3 * id3] = p0.x;
                    normals[3 * id3 + 1] = p0.y;
                    normals[3 * id3 + 2] = p0.z;
                }
                return true;
            }
            catch (Exception e)
            {
                errMessage = "Create Normal Buffer Failed." + e.Message;
                return false;
            }

        }

        public void Clear()
        {
            vertices = null;
            indices = null;
            normals = null;
            colors = null;
            texcoords = null;
            textureBytes = null;
            texBitmap = null;
        }
    }
    //创建三维图形渲染类，从CGraphic3D继承统一接口
    public class CGraphic3D
    {
        public string errMessage = "";
        
        public gEngine engine = gEngine.auto; //驱动
        public string graphicDevice = "Auto";  //设备

        public virtual bool Blend { get; set; }
        public bool _blending = false;
       
        public bool enableTranslate = true;
        public bool enableScale = true;
        public bool enableRotate = true;
        public int maxTrianglesNum = 2000000;
        public bool InverseBitmap = false;//图像字节反序RGBA to ABGR
        public CTexture MyTexture = new CTexture();

        public GraphicDeviceInfo currentGraphicDevice = null;
        public string graphicLIBPath 
        {   get { return System.IO.Directory.GetCurrentDirectory(); }
            set { }
        }      
        
        public CGraphic3D()
        {
           
        }
        public CModel currentModel = null;       
        //release only do not remove it
        public virtual bool ReleaseModelsByKeys(long key)
        {
            CModel model;
            lock (modelLocker)
            {
                if (objectModels.TryGetValue(key, out model))
                {
                    model.Release();                    
                    return true;
                }
                else return false;
            }
        }
        //release only do not remove it
        public virtual bool ReleaseModelsByKeys(List<long> keys)
        {
            CModel model;
            int n = 0;
            long key;
            lock (modelLocker)
            {  
                for(int i=0;i<keys.Count;i++)
                {
                    key = keys[i];
                    if (objectModels.TryGetValue(key, out model))
                    {
                        model.Release();
                        n++;
                    }                    
                }
            }
            return n == keys.Count;
        }

        //release and remove it
        public virtual bool ClearModelsByKeys(long key)
        {
            CModel model;
            bool removed = false;
            lock (modelLocker)
            {
                if (objectModels.TryGetValue(key, out model))
                {
                    model.Release();
                    removed = true;
                }
                objectModels.Remove(key);
            }
            return removed;
        }
        //release and remove it
        public virtual bool ClearModelsByKeys(List<long> keys)
        {
            CModel model;
            int n = 0;
            long key;
            lock (modelLocker)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    key = keys[i];
                    if (objectModels.TryGetValue(key, out model))
                    {
                        model.Release();
                        n++;
                    }
                    objectModels.Remove(key);
                }
            }
            return n == keys.Count;
        }

        //find it, release and replace it
        public virtual bool UpdateModels(List<long> keys, List<CModel> objs)
        {   
            CModel model;
            long key;
            int n = 0;
            lock (modelLocker)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    key = keys[i];
                    if (objectModels.TryGetValue(key, out model))
                    {
                        model.Release();
                        objectModels[key] = objs[i];
                        n++;
                    }
                }
            }
            return n == keys.Count;
        }

        //find it, release and replace it
        public virtual bool UpdateModels(long key, CModel obj)
        {
            CModel model;
            lock (modelLocker)
            {
                if (objectModels.TryGetValue(key, out model))
                {
                    model.Release();
                    objectModels[key] = obj;
                    return true;
                }
                else return false;
            }
        }

        public virtual bool UpdateModelVisible(CModel model,bool visible)
        {
            model.Visible = visible;
            return true;
        }
        public virtual bool UpdateModelVertexBuffer(CModel model, Object obj)
        {
            return true;
        }
        public virtual bool UpdateModelIndexBuffer(CModel model,Object obj)
        {
            return true;
        }
        public virtual bool UpdateModelColors(CModel model, Object obj)
        {
            return true;
        }
        public virtual bool UpdateModelTextureImage(CModel model, Object obj)
        {
            return true;
        }
        public virtual bool UpdateModelTextureCoords(CModel model, Object obj)
        {
            return true;
        }
        public virtual bool UpdateModelAlpha(CModel model, float alpha)
        {
            return true;
        }
        public virtual bool UpdateModelFillMode(CModel model, gDrawMode mode)
        {
            return true;
        }
        public virtual bool UpdateModelLineStyle(CModel model, float width, gLineStyle style)
        {
            return true;
        }

        //release only do not remove it
        public virtual bool SetModelsVisibleByKeys(long key, bool visible)
        {
            CModel model;
            lock (modelLocker)
            {
                if (objectModels.TryGetValue(key, out model))
                {
                    model.Visible = visible;
                    return true;
                }
                else return false;
            }
        }
        //release only do not remove it
        public virtual bool SetModelsVisibleByKeys(List<long> keys,bool visible)
        {
            CModel model;
            int n = 0;
            long key;
            lock (modelLocker)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    key = keys[i];
                    if (objectModels.TryGetValue(key, out model))
                    {
                        model.Visible = visible;
                        n++;
                    }
                }
            }
            return n == keys.Count;
        }
       
        public static bool IsVulkanSupport()
        {
            string vulkandll = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\vulkan-1.dll";
            if (!File.Exists(vulkandll)) return false;
            else return true;
        }
        //vulkan priority        
        public virtual bool Initialize(IntPtr window, string title = "vulkan", int width = 800, int height = 600)
        {
            return true;
        }      
       
        public virtual bool DestroyWindow()
        {
            return true;
        }
        public virtual string GetLastErrMessage()
        {
            return errMessage;
        }
        /// <summary>
        /// 绘制文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color">颜色</param>
        /// <param name="transparentColor">背景透明色</param> 
        /// <param name="p1">起始位置</param>
        /// <param name="p2">结束位置</param>
        /// <param name="direct">字体顶端单位方向向量</param>        
        public virtual void DrawString( string text, Font font, Color color, 
                                        Vector64 p1, Vector64 p2, Vector64 direct, 
                                        Color transparentColor,
                                        TextHorizontalAlignment horAlignment = TextHorizontalAlignment.Left,
                                        TextVerticalAlignment verAlignment = TextVerticalAlignment.Center )
        {
            
        }
        /// <summary>
        /// 绘制文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color">颜色</param>
        /// <param name="size">大小,参考1.0</param> 
        /// <param name="p1">起始位置</param>        
        /// <param name="direct">字体顶端单位方向向量</param>
        /// <param name="up">字体正方向向量</param>
        public virtual void DrawString(string text, Font font, Color color,float size,
                                        Vector64 p1, Vector64 direct, Vector64 up,                                      
                                        TextHorizontalAlignment horAlignment = TextHorizontalAlignment.Left,
                                        TextVerticalAlignment verAlignment = TextVerticalAlignment.Center)
        {

        }
        public virtual bool IsTriangleMode( DrawingPrimitive pritive)
        {
            switch (pritive)
            {
                case DrawingPrimitive.POINT_LIST:return false;
                case DrawingPrimitive.LINE_LIST: return false;
                case DrawingPrimitive.LINE_STRIP: return false;
                case DrawingPrimitive.TRIANGLE_LIST: return true;
                case DrawingPrimitive.TRIANGLE_STRIP: return true;
                case DrawingPrimitive.TRIANGLE_FAN: return true;
                case DrawingPrimitive.LINE_LIST_WITH_ADJACENCY: return false;
                case DrawingPrimitive.LINE_STRIP_WITH_ADJACENCY: return false;
                case DrawingPrimitive.TRIANGLE_LIST_WITH_ADJACENCY: return true;
                case DrawingPrimitive.TRIANGLE_STRIP_WITH_ADJACENCY: return true;
                case DrawingPrimitive.PATCH_LIST: return false;
                default:return false;
            }
        }
        public bool LoadGraphicConfig(string _path = "")
        {
            string path = _path;
            if( path.Length < 1 ) path = graphicLIBPath + "\\Graphics.ini";
            AsciiReader ac = new AsciiReader();
            if ( !ac.Load(path) ) return false;

            string line = "";
            string deviceString, engineString;
            if ( ac.SeekSection("[Config]") )
            {
                while( (line = ac.GetLine()) != null )
                {
                    deviceString = ac.GetItemValue(line, "device").Trim();
                    engineString = ac.GetItemValue(line, "engine").Trim();
                    if (deviceString.Length > 0) graphicDevice = deviceString;
                    if (engineString.Length > 0) 
                    {
                        if (engineString.ToLower() == "auto")
                            engine = gEngine.auto;
                        else if (engineString.ToLower() == "opengl")
                            engine = gEngine.opengl;
                        else if (engineString.ToLower() == "vulkan")
                            engine = gEngine.vulkan;
                        else if (engineString.ToLower() == "opengles")
                            engine = gEngine.opengles;
                        else engine = gEngine.auto;
                    }
                }                
            }
            
            ac.Close();


            return true;
        }
        public bool SaveGraphicConfig(string _path = "")
        {            
            try
            {
                string path = _path;
                if (path.Length < 1) path = graphicLIBPath + "\\Graphics.ini";

                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);                
                sw.WriteLine("[Config]");
                sw.WriteLine("device = " + graphicDevice);
                sw.WriteLine("engine = " + engine.ToString());
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        #region WMIPath 
        public enum WMIPath
        {
            // 硬件 
            Win32_Processor, // CPU 处理器 
            Win32_PhysicalMemory, // 物理内存条 
            Win32_Keyboard, // 键盘 
            Win32_PointingDevice, // 点输入设备，包括鼠标。 
            Win32_FloppyDrive, // 软盘驱动器 
            Win32_DiskDrive, // 硬盘驱动器 
            Win32_CDROMDrive, // 光盘驱动器 
            Win32_BaseBoard, // 主板 
            Win32_BIOS, // BIOS 芯片 
            Win32_ParallelPort, // 并口 
            Win32_SerialPort, // 串口 
            Win32_SerialPortConfiguration, // 串口配置 
            Win32_SoundDevice, // 多媒体设置，一般指声卡。 
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP) 
            Win32_USBController, // USB 控制器 
            Win32_NetworkAdapter, // 网络适配器 
            Win32_NetworkAdapterConfiguration, // 网络适配器设置 
            Win32_Printer, // 打印机 
            Win32_PrinterConfiguration, // 打印机设置 
            Win32_PrintJob, // 打印机任务 
            Win32_TCPIPPrinterPort, // 打印机端口 
            Win32_POTSModem, // MODEM 
            Win32_POTSModemToSerialPort, // MODEM 端口 
            Win32_DesktopMonitor, // 显示器 
            Win32_DisplayConfiguration, // 显卡 
            Win32_DisplayControllerConfiguration, // 显卡设置 
            Win32_VideoController, // 显卡细节。 
            Win32_VideoSettings, // 显卡支持的显示模式。 

            // 操作系统 
            Win32_TimeZone, // 时区 
            Win32_SystemDriver, // 驱动程序 
            Win32_DiskPartition, // 磁盘分区 
            Win32_LogicalDisk, // 逻辑磁盘 
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。 
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置 
            Win32_PageFile, // 系统页文件信息 
            Win32_PageFileSetting, // 页文件设置 
            Win32_BootConfiguration, // 系统启动配置 
            Win32_ComputerSystem, // 计算机信息简要 
            Win32_OperatingSystem, // 操作系统信息 
            Win32_StartupCommand, // 系统自动启动程序 
            Win32_Service, // 系统安装的服务 
            Win32_Group, // 系统管理组 
            Win32_GroupUser, // 系统组帐号 
            Win32_UserAccount, // 用户帐号 
            Win32_Process, // 系统进程 
            Win32_Thread, // 系统线程 
            Win32_Share, // 共享 
            Win32_NetworkClient, // 已安装的网络客户端 
            Win32_NetworkProtocol, // 已安装的网络协议 
        }
        #endregion
        public virtual GraphicDeviceInfo GetChoosedGraphicsDevice()
        {
            return null;
        }
        public virtual string GetGraphicsInfoString()
        {
            return "not supported.";
        }
        public virtual string GetGraphicName()
        {
            return "not supported.";
        }
        string GetDeviceNameFromString(string info)
        {
            //"APIVersion: 401046\r\nDriverVersion: 63890000\r\nvendorID: 10DE\r\ndeviceID: 1C20\r\ndeviceType: VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU\r\ndeviceName: GeForce GTX 1060 with Max-Q Design\r\npipelineCacheUUID: 4F-75-AC-35-E8-31-77-57-F5-A8-32-70-D1-51-53-92\r\n"
            string[] ss = info.Split(new char[] {'\r','\n'},StringSplitOptions.RemoveEmptyEntries);
            string name = "";
            for( int i = 0; i < ss.Length; i++ )
            {
                name = ss[i].Trim();
                string[] rr = name.Split(new char[] { ':'}, StringSplitOptions.RemoveEmptyEntries);
                if ( rr.Length > 1 )
                {
                    name = rr[0].Trim().ToLower();
                    if ( name == "devicename" )
                    {
                        return rr[1];
                    }
                }
            }
            return "";
        }
        //比较name1和name2相似度
        static public bool DeviceNameCompare(string name1,string name2)
        {
            string[] ss1 = name1.Split(new char[] { ' ','\t' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ss2 = name2.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int n1 = ss1.Length;
            int n2 = ss2.Length;
            int n = 0;
            foreach (string s1 in ss1)
            {
                foreach(string s2 in ss2)
                {
                    if (s1.ToLower().Trim() == s2.ToLower().Trim())
                        n++;  
                }
            }
            ss1 = ss2 = null;
            if ((double)n / (double)n1 >= 0.8) return true;
            else return false;
        }
        
        public virtual GraphicDeviceInfo GetGraphicsDeviceInfo(string name)
        {
            GraphicDeviceInfo[] infos = GetGraphicsDeviceInfo();
            if ( infos.Length < 1 ) return null;
            
            string devicename = GetDeviceNameFromString(name);
            if (devicename.Length < 1) return null;

            foreach ( GraphicDeviceInfo info in infos )
            {
                if (DeviceNameCompare(devicename,info.Name))
                {                    
                    return info;
                }
            }
            return null;
        }
        public virtual GraphicDeviceInfo[] GetGraphicsDeviceInfo()
        {            
            try
            {
                ManagementClass mc = new ManagementClass(WMIPath.Win32_VideoController.ToString());
                ManagementObjectCollection moc = mc.GetInstances();
                GraphicDeviceInfo[] info = new GraphicDeviceInfo[moc.Count];
                int i = 0;
                ulong mb = 1024 * 1024;
                foreach (ManagementObject mo in moc)
                {
                    info[i] = new GraphicDeviceInfo();
                    info[i].Name = mo.Properties["NAME"].Value.ToString();
                    info[i].MemorySize = ulong.Parse(mo.Properties["AdapterRAM"].Value.ToString()) / mb;
                    info[i].InstalledDisplayDrivers = mo.Properties["InstalledDisplayDrivers"].Value.ToString();
                    info[i].DriverVersion = mo.Properties["DriverVersion"].Value.ToString();
                    i++;
                }
                return info; 
            }
            catch(Exception e)
            {
                errMessage = "Acquiring video device information failed!" + e.Message;
                return null;
            }
        }
        
        public virtual void onWindowResized(int width,int height) { }
        #region Window
        public int windowWidth = 800;
        public int windowHeight = 600;
        public string windowTitle = "OpenGL";
        #endregion Window

        public float m_LineWidth = 1.0f;
        public vec4 curColor = new vec4(1, 1, 1, 1);

        #region Matrix Declaration--------------
        public bool initialized = false;
        //projection,model view
        public UniformBufferObject m_modelMatrix = new UniformBufferObject(true);
        Stack<UniformBufferObject> matrixArrayStack = new Stack<UniformBufferObject>();                
        //model matrix
        public vec3 m_translate = new vec3(0, 0, 0);
        public vec3 m_scale = new vec3(1, 1, 1);
        public vec3 m_rotate = new vec3(0, 0, 0);
        public Stack<mat4> modelMatrixStack = new Stack<mat4>();

        public DrawParameter curDrawParameter = new DrawParameter(true);
        public Stack<DrawParameter> drawParametersStack = new Stack<DrawParameter>();        

        public vec3 m_eye = new vec3(0,0,1);      // lookAt view matrix
        public vec3 m_center = new vec3(0,0,0);   // lookAt view matrix
        public vec3 m_up = new vec3(0,1,0);       // lookAt view matrix
        public float m_fovy = 30;
        public float m_aspect = 8.0f / 6;
        public float m_near = 0.001f;
        public float m_far = 1000f;
        public vec3 m_lasteye;      // the last position of view matrix
        public vec3 m_lastcenter;   // the last position of view matrix
        public vec3 m_lastup;       // the last position of view matrix
        #endregion Matrix Declaration--------------

        #region Matrix Functions-------------------
        public virtual void PushMatrix()
        {
            //push current matrix:projection,view,model...
            UniformBufferObject p = m_modelMatrix.Copy();
            matrixArrayStack.Push(p);

            //push current model transfor matrix translation,rotate,scale
            vec4 t1 = new vec4(m_translate.x, m_translate.y, m_translate.z, 0);
            vec4 t2 = new vec4(m_rotate.x, m_rotate.y, m_rotate.z, 0);
            vec4 t3 = new vec4(m_scale.x, m_scale.y, m_scale.z, 0);
            vec4 t4 = new vec4(0, 0, 0, 0); //for reserved
            mat4 m = new mat4(t1, t2, t3, t4);
            modelMatrixStack.Push(m);

            drawParametersStack.Push(curDrawParameter);            
        }
        public virtual void PopMatrix()
        {
            if (matrixArrayStack.Count > 0)
            {
                m_modelMatrix = matrixArrayStack.Pop();                
            }            
            if (modelMatrixStack.Count > 0)
            {
                mat4 m = modelMatrixStack.Pop();
                m_translate = new vec3(m[0].x, m[0].y, m[0].z);
                m_rotate = new vec3(m[1].x, m[1].y, m[1].z);
                m_scale = new vec3(m[2].x, m[2].y, m[2].z);
            }            
            if (drawParametersStack.Count > 0)
            {
                curDrawParameter = drawParametersStack.Pop();                
            }
        }
        public virtual void SetTextureWaveStartTime(float starttime)
        {
            m_modelMatrix.material.shininess.z = starttime;
        }
        public virtual void DisableTextureWave()
        {
            m_modelMatrix.material.shininess.z = 0;
        }

        public virtual void LoadIdentity()        
        {
            m_translate = new vec3(0, 0, 0);
            m_scale = new vec3(1, 1, 1);
            m_rotate = new vec3(0, 0, 0);//x,y,z axe angle
            m_eye = new vec3(0, 0, 1);
            m_center = new vec3(0, 0, 0);
            m_up = new vec3(0, 1, 0);

            m_modelMatrix.model = new mat4(1.0f); 
            m_modelMatrix.view = glm.lookAt(m_eye, m_center, m_up);
            m_modelMatrix.proj = glm.perspective(glm.radians(45.0f),
                                         (float)windowWidth/ (float)windowHeight,
                                         1.0f, 1000.0f);

            m_modelMatrix.eyepos = new vec4(m_eye, 1.0f);

            m_modelMatrix.lights[0].Enable = true;

            matrixArrayStack.Clear();
            modelMatrixStack.Clear();
            drawParametersStack.Clear();
            curDrawParameter = new DrawParameter(true);
        }
        //眼睛与目标点（原点）距离
        public virtual double EyeDistance 
        {
            get 
            {
                double xl = m_center.x - m_eye.x;
                double yl = m_center.y - m_eye.y;
                double zl = m_center.z - m_eye.z;
                return Math.Sqrt(xl * xl + yl * yl + zl * zl);
            }
        }
        //眼睛与目标点（原点）方向向量
        public virtual vec3 EyeDirectionVector
        {
            get
            {
                double dist = EyeDistance;
                if (dist <= 0) return new vec3(0, 0, 0);
                double xl = m_center.x - m_eye.x;
                double yl = m_center.y - m_eye.y;
                double zl = m_center.z - m_eye.z;
                return new vec3((float)(xl / dist), (float)(yl / dist), (float)(zl / dist));
            }
        }
        public virtual Vector64 EyeDirectionVector64
        {
            get
            {
                double dist = EyeDistance;
                if (dist <= 0) return new Vector64(0, 0, 0);
                double xl = m_center.x - m_eye.x;
                double yl = m_center.y - m_eye.y;
                double zl = m_center.z - m_eye.z;
                return new Vector64(xl / dist,yl/dist, zl / dist);
            }
        }
        // move along the direction m_eye to m_center
        // modified by jian 2020.12.29
        public virtual void MoveForward(float step = 1.0f, bool movecenter = false)
        {
            double dist = EyeDistance; // eye to viewpoint distance 
            
            double movestep = step;

            if ( CDataModel.IsEarthMapVision )//球坐标系统
            {
                double rad = 0.5;//球半径
                if ( dist <= rad ) return; //视点在球面内
                else
                {
                    if ( dist - step < rad )//穿越球面
                    {
                        movestep = (dist - rad) / 2.0; //视点无限接近球面
                    }
                }
            }

            if( dist <= movestep ) movestep = dist / 2.0;
           
            Vector64 v1 = new Vector64(m_eye.x, m_eye.y, m_eye.z);
            Vector64 v2 = new Vector64(m_center.x, m_center.y, m_center.z);
            Vector64 eyeVector = (v2 - v1).Normalize();

            Vector64 p = movestep * eyeVector;
            m_eye.x += (float)p.x;
            m_eye.y += (float)p.y;
            m_eye.z += (float)p.z;
            m_center.x += (float)p.x;
            m_center.y += (float)p.y;
            m_center.z += (float)p.z;
            LookAt();

            // m_fovy = m_fovy - 0.5f;
            // if (m_fovy < 0.1) m_fovy = 0.1f;
            // m_modelMatrix.proj = glm.perspective(glm.radians(m_fovy), m_aspect, m_near, m_far);
        }

        // move back along the direction m_eye to m_center
        // modified by jian 2020.12.29
        public virtual void MoveBackward(float step = 1.0f, bool movecenter = false)
        {
            Vector64 v1 = new Vector64(m_eye.x, m_eye.y, m_eye.z);
            Vector64 v2 = new Vector64(m_center.x, m_center.y, m_center.z);
            Vector64 eyeVector = (v1-v2).Normalize();
            Vector64 p = step * eyeVector;
            m_eye.x += (float)p.x;
            m_eye.y += (float)p.y;
            m_eye.z += (float)p.z;
            m_center.x += (float)p.x;
            m_center.y += (float)p.y;
            m_center.z += (float)p.z;
            LookAt();

            //adjust project matrix
            // m_fovy = m_fovy + 0.5f;
            //  if (m_fovy > 90) m_fovy = 90;
            //  m_modelMatrix.proj = glm.perspective(glm.radians(m_fovy), m_aspect, m_near, m_far);
        }
        public virtual void MoveLeft(float step = 1.0f)
        {
            m_center.x -= step;
            LookAt();
        }
        public virtual void MoveRight(float step = 1.0f)
        {
            m_center.x += step;
            LookAt();
        }
        public virtual void MoveUp(float step = 1.0f)
        {
            m_center.y -= step;
            LookAt();
        }
        public virtual void MoveDown(float step = 1.0f)
        {
            m_center.y += step;
            LookAt();
        }

        //rotate just current model
        public virtual void Rotate(float angle, float x, float y, float z)
        {
            Rotate(angle, new vec3(x, y, z));
        }
        //rotate just current model
        public virtual void Rotate(float angle, vec3 axis)
        {
            //m_modelMatrix.model = glm.rotate(m_modelMatrix.model, glm.radians(angle), axis );
            if (axis.x != 0) m_rotate.x += angle;
            if (axis.y != 0) m_rotate.y += angle;
            if (axis.z != 0) m_rotate.z += angle;
        }
        //scale current model
        public virtual void Scale(float xscale, float yscale, float zscale)
        {
            Scale(new vec3(xscale, yscale, zscale));
        }
        //scale current model
        public virtual void Scale(vec3 _scale)
        {
            //m_modelMatrix.model = glm.scale(m_modelMatrix.model,_scale);
            m_scale.x *= _scale.x;
            m_scale.y *= _scale.y;
            m_scale.z *= _scale.z;
        }
        //translate current model
        public virtual void Translate(float xoff, float yoff, float zoff)
        {
            Translate(new vec3(xoff, yoff, zoff));
        }
        //translate current model
        public virtual void Translate(vec3 _translate)
        {
            //m_modelMatrix.model = glm.translate(m_modelMatrix.model,_translate);
            m_translate.x += _translate.x;

            //convert vulkan y axe to downwards
            m_translate.y += _translate.y;

            m_translate.z += _translate.z;
        }
        public virtual void LookAt(vec3 eye, vec3 center, vec3 up)
        {
            m_modelMatrix.view = glm.lookAt(eye, center, up);
            m_eye = eye;
            m_center = center;
            m_up = up;
        }
        public virtual void LookAt(float eye_x, float eye_y, float eye_z,
                           float center_x, float center_y, float center_z,
                           float up_x, float up_y, float up_z)
        {
            m_eye = new vec3(eye_x, eye_y, eye_z);
            m_center = new vec3(center_x, center_y, center_z);
            m_up = new vec3(up_x, up_y, up_z);
            m_modelMatrix.view = glm.lookAt(m_eye, m_center, m_up);
        }
        public virtual void LookAt()
        {
            m_modelMatrix.view = glm.lookAt(m_eye, m_center, m_up);
            //save the last position of view matrix
            m_lasteye = m_eye;
            m_lastcenter = m_center;
            m_lastup = m_up;
        }
        public virtual void Perspective(float fovy, float aspect, float near, float far)
        {
            m_modelMatrix.proj = glm.perspective(glm.radians(fovy), aspect, near, far);
            m_fovy = fovy;
            m_aspect = aspect;
            m_near = near;
            m_far = far;
        }
        public virtual void Perspective(float fovy = 45, float near = 0.1f, float far = 100)
        {
            float aspect = (float)windowWidth / (float)windowHeight;
            m_modelMatrix.proj = glm.perspective(glm.radians(fovy), aspect, near, far);
            m_fovy = fovy;
            m_aspect = aspect;
            m_near = near;
            m_far = far;
        }
        public double GetLength(vec3 v)
        {
            return Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }
        public vec3 Normalize(vec3 v)
        {
            double r = GetLength(v);

            if (r == 0) return new vec3(0, 0, 0);
            else return new vec3((float)(v.x / r), (float)(v.y / r), (float)(v.z / r));
        }
        public void SetMatrixFromFloat(ref mat4 matrix, float[]_array)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = _array[i * 4 + j];
                }
        }
        public virtual void SetProjectMatrix(float[] matrix)
        {
            SetMatrixFromFloat(ref m_modelMatrix.proj, matrix);
        }
        public virtual void SetViewMatrix(float[] matrix)
        {
            double r = GetLength(m_eye);
            SetMatrixFromFloat(ref m_modelMatrix.view, matrix);            
        }
        public virtual void SetModelMatrix(float[] matrix)
        {
            SetMatrixFromFloat(ref m_modelMatrix.model, matrix);
        }
        public virtual void SetDepthWriteEnable(bool enable = true)
        {
            
        }
        public virtual void SetDepthTestEnable(bool enable = true)
        {

        }        
        #endregion Matrix Functions-------------------

        #region shader function-------------------------------------------------------- 
        public string toShaderString(List<string> lines)
        {
            string str = "";
            foreach (string s in lines)
            {
                str += s;
                str += "\n";
            }
            return str;
        }
        public virtual List<string> LoadShaderSource(string shaderFile)
        {
            List<string> lines = new List<string>();
            string ss;
            try
            {
                FileStream fs = new FileStream(shaderFile, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                while ((ss = sr.ReadLine()) != null)
                {
                    if (ss.Length > 0) lines.Add(ss);
                }
                sr.Close();
                fs.Close();
            }
            catch (IOException e)
            {
                errMessage = e.Message;
            }
            return lines;
        }
        #endregion shader function-------------------------------------------------------- 

        //------light function------------------------------
        #region light function---------------------------------------
        public virtual void SetLight(int index, LightStruct _light)
        {
            m_modelMatrix.lights[index] = _light.Copy();
        }
        public virtual void SetLightAmbient(int index, vec4 amb)
        {
            if (index < 0 || index > 7) return;
            m_modelMatrix.lights[index].ambient = amb;
        }
        public virtual void SetLightDiffuse(int index, vec4 diff)
        {
            if (index < 0 || index > 7) return;
            m_modelMatrix.lights[index].diffuse = diff;
        }
        public virtual void SetLightSpecular(int index, vec4 specular)
        {
            if (index < 0 || index > 7) return;
            m_modelMatrix.lights[index].specular = specular;
        }
        public virtual void SetLightPos(int index, vec3 pos)
        {
            if (index < 0 || index > 7) return;
            m_modelMatrix.lights[index].pos = new vec4(pos, 1);
        }
        public virtual void SetLightPos(int index, float x, float y, float z)
        {
            SetLightPos(index, new vec3(x, y, z));
        }
        public virtual void EnableLight(int index, bool enable = true)
        {
            if (index < 0 || index > 7) return;
            m_modelMatrix.lights[index].Enable = enable;
        }
        public virtual void DisableLight(int index)
        {
            if (index < 0 || index > 7) return;
            m_modelMatrix.lights[index].Enable = false;
        }
        public virtual vec4 ConvertColor(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;
            return new vec4(r, g, b, a);
        }
        public virtual vec4 ConvertColor(ColorRGBA color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;
            return new vec4(r, g, b, a);
        }
        public virtual Color ConvertColor(vec4 color)
        {
            byte r = (byte)(color.x * 255f);
            byte g = (byte)(color.y * 255f);
            byte b = (byte)(color.z * 255f);
            byte a = (byte)(color.w * 255f);
            return Color.FromArgb(a, r, g, b);
        }
        #endregion light function---------------------------------------
        //------material function------------------------------
        #region materials
        public virtual void SetMaterial(MaterialStruct _material)
        {
            m_modelMatrix.material = _material.Copy();
        }
        public virtual void SetMaterialShininess(float _shininess)
        {
            m_modelMatrix.material.SetShininess(_shininess);
        }
        public virtual void SetMaterialNS(float _ns)
        {
            m_modelMatrix.material.SetNS(_ns);
        }
        public virtual void SetMaterialAmbient(vec4 amb)
        {
            m_modelMatrix.material.ambient = amb;
        }
        public virtual void SetMaterialDiffuse(vec4 diff)
        {
            m_modelMatrix.material.diffuse = diff;
        }
        public virtual void SetMaterialSpecular(vec4 specular)
        {
            m_modelMatrix.material.specular = specular;
        }
        #endregion materials       

        #region Texture
        public Bitmap textureBitmap = null;
        public DataCollection.TextureMagFilter textureMode = DataCollection.TextureMagFilter.GL_LINEAR;
        public bool bEnableTexture = false;       
        public virtual int GetMax2DTextureImageSize() { return 1024; }
        public virtual void EnableTexture(bool _enable = true) 
        {
            bEnableTexture = _enable;
        }
        public virtual void DisableTexture(bool _enable = false)
        {
            bEnableTexture = _enable;
        }

        #region 图像字节顺序转换-------------------------------------------------      
        /// <summary>
        /// RGBA to BGRA
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bitNum"></param>
        /// <returns></returns>
        public virtual byte[] BitmapBytesConvert(byte[] source, int bitNum = 4 )
        {
            byte r, g, b, a;
            byte[] newbytes = new byte[source.Length];
            if( bitNum == 3 )
            {
                int len = source.Length / 3;
                for(int i = 0; i < len; i++ )
                {
                    r = source[3 * i];
                    g = source[3 * i+1];
                    b = source[3 * i+2];
                    newbytes[3 * i] = b;
                    newbytes[3 * i + 1] = g;
                    newbytes[3 * i + 2] = r;
                }
            }
            else if (bitNum == 4)
            {
                int len = source.Length / 4;
                for (int i = 0; i < len; i++)
                {
                    a = source[4 * i];
                    r = source[4 * i + 1];
                    g = source[4 * i + 2];
                    b = source[4 * i + 3];
                    newbytes[4 * i] = b;
                    newbytes[4 * i + 1] = g;
                    newbytes[4 * i + 2] = r;
                    newbytes[4 * i + 3] = a;
                }
            }
            return newbytes;
        }

        public virtual int ImageColorConvert(int value, int byteNum = 4)
        {
            byte r, g, b, a;
            r = (byte)(value);
            g = (byte)(value >> 8);
            b = (byte)(value >> 16);
            a = (byte)(value >> 24);
            return (b & 0xFF) | ((g & 0xFF) << 8) | ((r & 0xFF) << 16) | ((a & 0xFF) << 24);
        }
        public virtual byte[] BitmapToBytes(Bitmap bmp, int bitNum = 4,bool inverse = false)
        {
            return IntArrayToByteArray(BitmapToIntBytes(bmp, bitNum, inverse));
        }
        /// <summary>
        /// 图像数据转换成贴图所所需的int[]字节数据序列--静态版本
        /// </summary>
        /// <param name="bitmap">Bitmap图像</param>
        /// <param name="byteNum">图像位数，默认4位
        /// BGRA贴图格式（-4），对应于图片RGBA序列,需和具体设备格式做比较是否需要转换
        /// RGRA贴图格式4，对应于图片RGBA逆序列，
        /// </param>
        /// <returns></returns>
        public virtual int[] BitmapToIntBytesStatic(Bitmap bmp, int bitnum,bool inverse = false)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] intBytes = new int[bmp.Width * bmp.Height];
            Marshal.Copy(bd.Scan0, intBytes, 0, intBytes.Length);
            bmp.UnlockBits(bd);

            int value;
            byte r, g, b, a;
            if (inverse)//rgba to bgra
            {
                for (int i = 0; i < bmp.Height * bmp.Width; i++)
                {
                    value = intBytes[i];
                    r = (byte)(value);
                    g = (byte)(value >> 8);
                    b = (byte)(value >> 16);
                    a = (byte)(value >> 24);
                    intBytes[i] = (int)((b & 0xFF) | ((g & 0xFF) << 8) | ((r & 0xFF) << 16) | ((a & 0xFF) << 24));
                }
            }
            return intBytes;
        }
        /// <summary>
        /// 图像数据转换成贴图所所需的int[]字节数据序列--线程版本
        /// </summary>
        /// <param name="bitmap">Bitmap图像</param>
        /// <param name="byteNum">图像位数，默认4位
        /// BGRA贴图格式（-4），对应于图片RGBA序列,需和具体设备格式做比较是否需要转换
        /// RGRA贴图格式4，对应于图片RGBA逆序列，
        /// </param>
        /// <returns></returns>
        public virtual int[] BitmapToIntBytes(Bitmap bitmap, int byteNum = 4,bool inverse = false)
        {
            BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] intBytes = new int[bitmap.Width * bitmap.Height];
            Marshal.Copy(bd.Scan0, intBytes, 0, intBytes.Length);
            bitmap.UnlockBits(bd);

            int maxthread = 1024;//最大线程数
            ImageBitsStruct []datas = null;
            int nthread = bitmap.Height;
            int num = 1;
            int others = 0;
            if (inverse) //BGRA字节顺序，不需转换
            {
                if( bitmap.Height > maxthread )
                {
                    nthread = maxthread;
                    num = bitmap.Height / maxthread;
                    others = bitmap.Height % maxthread;
                }
                
                datas = new ImageBitsStruct[nthread];
                if( others == 0 )
                {
                    for (int i = 0; i < nthread; i++)
                    {
                        datas[i] = new ImageBitsStruct(i, intBytes, bitmap.Width, num);
                    }
                }
                else
                {
                    int rowid = 0;
                    for (int i = 0; i < others; i++)
                    {
                        datas[i] = new ImageBitsStruct(rowid, intBytes, bitmap.Width, num + 1);
                        rowid += (num + 1);
                    }
                    for (int i = 0; i < nthread- others; i++)
                    {
                        datas[i] = new ImageBitsStruct(rowid, intBytes, bitmap.Width, num);
                        rowid += num;
                    }
                }            

                ThreadPool.SetMaxThreads(nthread, nthread);
                lockedValue = 0;
                for (int i = 0; i < nthread; i++)
                {
                    Thread thread = new Thread(byteConvertThread);
                    ThreadPool.QueueUserWorkItem(byteConvertThread, datas[i]);
                }
                //等待线程运行完成，lockedValue = 已完成线程数
                while (lockedValue < bitmap.Height) Thread.Sleep(10);
            }
            
            return intBytes;
        }
       
        public static byte[] IntArrayToByteArray(int[] intArr)
        {
            int intSize = sizeof(int) * intArr.Length;
            byte[] bytArr = new byte[intSize];
            //申请一块非托管内存
            IntPtr ptr = Marshal.AllocHGlobal(intSize);
            //复制int数组到该内存块
            Marshal.Copy(intArr, 0, ptr, intArr.Length);
            //复制回byte数组
            Marshal.Copy(ptr, bytArr, 0, bytArr.Length);
            //释放申请的非托管内存
            Marshal.FreeHGlobal(ptr);
            return bytArr;
        }
        struct ImageBitsStruct
        {
            public int[] intBytes; //图像数组
            public int rowid;      //当前行
            public int width;      //图像宽度
            public int rowNum;     //处理行数 
            public ImageBitsStruct(int _rowid, int[] bytes, int _width,int _rowNum = 1)
            {
                intBytes = bytes;
                rowid = _rowid;
                width = _width;
                rowNum = _rowNum;
            }
        }
        /// <summary>
        /// Texture贴图多线程版本
        /// 图形设备不支持BGRA字节顺序，需要将图片字节序列RGBA转换成BGRA
        /// 此过程耗费较多时间，多线程版本提高了速度
        /// 修改时间：2021-11-3
        /// 修改人：jiansir
        /// </summary>
        /// 
        int lockedValue = 0;//线程完成计数器，用于同步

        /// <summary>
        /// 字节顺序转换线程，每线程转换一到n行数据
        /// </summary>
        /// <param name="obj">ImageBitsStruct 圖像結構數據</param>
        void byteConvertThread(Object obj)
        {
            ImageBitsStruct data = (ImageBitsStruct)obj;
            int rows = data.rowNum;
            int value;
            byte r, g, b, a = 255;
            for (int k = 0; k < rows; k++)
            {
                for (int i = 0; i < data.width; i++)
                {
                    value = data.intBytes[i + (data.rowid + k) * data.width];
                    r = (byte)(value);
                    g = (byte)(value >> 8);
                    b = (byte)(value >> 16);
                    data.intBytes[i + data.rowid * data.width] = (int)((b & 0xFF)
                            | ((g & 0xFF) << 8)
                            | ((r & 0xFF) << 16)
                            | ((a & 0xFF) << 24));
                }
                Interlocked.Increment(ref lockedValue);
            }
        }        
        #endregion 图像字节顺序转换-------------------------------------------------

        public virtual Bitmap LoadTexture(String fileName, bool flip = true, bool foreoftwo = true)
        {
            Bitmap bmp = null;
            MyTexture.forcePowerOfTwo = foreoftwo;
            MyTexture.maxTextureSize = GetMax2DTextureImageSize();

            FileInfo file = new FileInfo(fileName);
            if (file.Exists == false)
            {
                errMessage = "file ' " + fileName + " ' not exist.";
                return null;
            }
            try
            {
                if (file.Extension.ToUpper() == ".TGA")
                {
                    ///http://blog.csdn.net/zgke/article/details/4667499
                    //ImageTGA tga = new ImageTGA(fileName);
                    //image = tga.Image;
                }
                else
                {
                    bmp = new Bitmap(fileName);
                    if (bmp == null) return null;
                    //check the format and force of two requirement
                    if (bmp.PixelFormat != PixelFormat.Format32bppArgb ||
                        (MyTexture.forcePowerOfTwo && !MyTexture.IsForceOfTwoImage(bmp)))
                    {
                        bmp = MyTexture.CreateCompitableBitmap(bmp, MyTexture.forcePowerOfTwo);
                    }

                    if (flip) bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }
            }
            catch (Exception ex)
            {
                errMessage = "loading file ' " + fileName + " ' failed.";
                errMessage += "\nexception:" + ex.Message;
                return null;
            }
            return bmp;
        }

        public virtual int BindTexture(Bitmap bmp, TextureMagFilter mode = TextureMagFilter.GL_LINEAR)
        {
            textureBitmap = bmp;
            textureMode = mode;
            return 1;
        }
        public virtual int BindTexture(byte[] texBytes, int width, int height, DataCollection.TextureMagFilter mode = DataCollection.TextureMagFilter.GL_LINEAR)
        {
            return 1;
        }
        public virtual void SetTextureMode(DataCollection.TextureMagFilter mode = DataCollection.TextureMagFilter.GL_LINEAR)
        {
            textureMode = mode;
        }
        #endregion Texture


        #region functions        
        protected object modelLocker = new object();
        public Dictionary<long, CModel>objectModels = new Dictionary<long, CModel>();        

        public List<long> objectModelKeyBuffers = new List<long>();       
        public virtual void AddModel(CModel model)
        {
            lock (modelLocker)
            {
                objectModels.Add(model.HashKey, model);
                objectModelKeyBuffers.Add(model.HashKey);
            }
        }
        public void ClearModelKeyBuffers()
        {
            lock (modelLocker)
            {
                objectModelKeyBuffers.Clear();
            }
        }
        //检测是否透明绘制
        public virtual bool IsTransparentDraw() { return false; }
        public virtual List<CModel> UpdateDrawOrder()
        {
            List<CModel> models1 = new List<CModel>();
            List<CModel> models2 = new List<CModel>();
            
            lock (modelLocker)
            {
                foreach (var obj in objectModels)
                {
                    if (obj.Value.Transparent ) 
                        models2.Add(obj.Value);
                    else models1.Add(obj.Value);
                }
                foreach (CModel m in models2)
                {
                    models1.Add(m);
                }                
            }
            models2.Clear();
            return models1;
        } //根据透明，重新排列绘制顺序
        /// <summary>
        /// 重设所有模型得统一参数，矩阵，光照，视点，材质等
        /// </summary>
        public virtual void ResetModelUniformMatrixs(List<CModel> models)
        {            
        }
        public Vertex3D[] verticesArray = null;
        public int[] indicesArray = null;
        public List<Vertex3D> pVertics = new List<Vertex3D>();
        public List<int> pIndices = new List<int>();
        public virtual void Vertex(Vertex3D p)
        {
            pVertics.Add(p);
        }
        public virtual void Vertex3f(float x,float y,float z)
        {
            pVertics.Add(new Vertex3D(x,y,z));
        }
        public virtual void VertexArray(Vertex3D []points)
        {
            verticesArray = points;
            pVertics.Clear();
        }
        public virtual void VertexIndexArray(int []indices)
        {
            indicesArray = indices;
            pIndices.Clear();
        }
        public virtual void VertexIndex(int index)
        {
            pIndices.Add(index);
        }
        public virtual Vertex3D CreateVertex(float x, float y, float z)
        {
            return CreateVertex(x, y, z, curColor);
        }
        public virtual float[] toArray(Vertex3D[]points)
        {
            int i, j;
            int size = Vertex3D.GetSize() / sizeof(float);
            float[] _floatArray = new float[size*points.Length];
            if(_floatArray == null)
            {
                errMessage = "Out of Memory.";
                return null;
            }
            float []_convert = null;
            for(i=0;i< points.Length;i++)
            {
                _convert = points[i].toArray();
                for (j = 0; j < size; j++)
                    _floatArray[i * size + j] = _convert[j];
            }
            _convert = null;
            return _floatArray;
        }
        public virtual Vertex3D CreateVertex(float x, float y, float z, vec4 color)
        {
            Vertex3D p = new Vertex3D(x, y, z);
            p.color = color;
            p.texCoord = new vec2(-1, -1);
            return p;
        }
        public virtual Vertex3D CreateVertex(float x, float y, float z, vec4 color, vec2 _texcoord)
        {
            Vertex3D p = new Vertex3D(x, y, z);
            p.color = color;
            p.texCoord = _texcoord;
            return p;
        }
        public virtual Vertex3D CreateVertex(float x, float y, float z, vec2 _texcoord)
        {
            Vertex3D p = new Vertex3D(x, y, z);
            p.color = curColor;
            p.texCoord = _texcoord;
            return p;
        }
        public virtual Vertex3D CreateVertex(float x, float y, float z, float texx, float texy)
        {
            Vertex3D p = new Vertex3D(x, y, z);
            p.color = curColor;
            p.texCoord = new vec2(texx, texy);
            return p;
        }
        //create normals from triangle array and save to Vertex3D
        //added by jian 2019-9-16
        public virtual bool CreateNormals(Vertex3D[] points, int[] indices)
        {
            if (points.Length < 3 || indices.Length < 3) return false;

            for (int i = 0; i < points.Length; i++)
                points[i].normal = new vec3(0, 0, 0);

            int id1, id2, id3;
            vec3 p1, p2, p3, pn;
            vec3 normal = new vec3(0, 0, 0);
            for (int i = 0; i < indices.Length / 3; i++)
            {
                id1 = indices[3 * i];
                id2 = indices[3 * i + 1];
                id3 = indices[3 * i + 2];

                p1 = points[id1].pos;
                p2 = points[id2].pos;
                p3 = points[id3].pos;

                //get normalized normal
                pn = GetNormal(p1, p2, p3);

                normal = points[id1].normal;
                normal += pn;
                points[id1].normal = normal;

                normal = points[id2].normal;
                normal += pn;
                points[id2].normal = normal;

                normal = points[id3].normal;
                normal += pn;
                points[id3].normal = normal;
            }
            return true;
        }
        public virtual float[] GetTriangleListNormals(float[] points, int[] indices)
        {
            if (points.Length < 3 * 3 || indices.Length < 3) return null;
            float[] normals = new float[points.Length];
            if (normals == null) return null;

            for (int i = 0; i < normals.Length; i++)
                normals[i] = 0;

            int id;
            vec3[] p = new vec3[3];
            vec3 pn;
            for (int i = 0; i < indices.Length / 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    id = indices[3 * i + k];
                    p[k].x = points[3 * id];
                    p[k].y = points[3 * id + 1];
                    p[k].z = points[3 * id + 2];
                }

                pn = GetNormal(p[0], p[1], p[2]);

                for (int k = 0; k < 3; k++)
                {
                    id = indices[3 * i + k];
                    normals[3 * id] += pn.x;
                    normals[3 * id + 1] += pn.y;
                    normals[3 * id + 2] += pn.z;
                }
            }

            return normals;
        }
        public virtual float[] GetTriangleStripNormals(float[] points, int[] indices)
        {
            if (points.Length < 3 * 3 || indices.Length < 3) return null;
            float[] normals = new float[points.Length];
            if (normals == null) return null;

            for (int i = 0; i < normals.Length; i++)
                normals[i] = 0;

            int[] id = new int[4];
            vec3[] p = new vec3[4];
            vec3 pn;
            //  0--- 2 
            //  1--- 3  012,213
            for (int i = 0; i < indices.Length / 2 - 1; i++)
            {
                id[0] = indices[2 * i];
                id[1] = indices[2 * (i+1)];
                id[2] = indices[2 * i+1];
                id[3] = indices[2 * (i+1)+1];
                for (int k = 0; k < 4; k++)
                {
                    p[k].x = points[3*id[k]];
                    p[k].y = points[3*id[k]+1];
                    p[k].z = points[3*id[k]+2];
                }
                pn = GetNormal(p[0], p[1], p[2]);
                normals[3 * id[0]] += pn.x;
                normals[3 * id[0]+1] += pn.y;
                normals[3 * id[0]+2] += pn.z;
                normals[3 * id[1]] += pn.x;
                normals[3 * id[1] + 1] += pn.y;
                normals[3 * id[1] + 2] += pn.z;
                normals[3 * id[2]] += pn.x;
                normals[3 * id[2] + 1] += pn.y;
                normals[3 * id[2] + 2] += pn.z;
                pn = GetNormal(p[2], p[1], p[3]);
                normals[3 * id[2]] += pn.x;
                normals[3 * id[2] + 1] += pn.y;
                normals[3 * id[2] + 2] += pn.z;
                normals[3 * id[1]] += pn.x;
                normals[3 * id[1] + 1] += pn.y;
                normals[3 * id[1] + 2] += pn.z;
                normals[3 * id[3]] += pn.x;
                normals[3 * id[3] + 1] += pn.y;
                normals[3 * id[3] + 2] += pn.z;
            }

            return normals;
        }
        //points - 3 float x,y,z
        public virtual float[] CalculateNormals(float[] points, int[] indices,
                                                DrawingPrimitive primitive = DrawingPrimitive.TRIANGLE_LIST)
        {
            if (primitive == DrawingPrimitive.TRIANGLE_LIST)
                return GetTriangleListNormals(points, indices);
            else if (primitive == DrawingPrimitive.TRIANGLE_STRIP)
                return GetTriangleStripNormals(points, indices);
            else return null;
        }
        /// <summary>
        /// 计算法线，默认计算
        /// </summary>
        /// <returns></returns>
        
        public bool CalculateNormals()
        {
            if (verticesArray != null && indicesArray != null)
                return CalculateNormals(verticesArray, indicesArray);
            else if (pVertics.Count > 2 && pIndices.Count > 2)
                return CalculateNormals(pVertics, pIndices);
            else return false;
        }

        static public bool CalculateNormals(List<Vertex3D>points, List<int>indices)
        {
            if (points == null || indices == null || points.Count < 3 || indices.Count < 3 )
            {
                //errMessage = "no enough points or indices.";
                return false;
            }

            Vertex3D p,p1,p2,p3;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                p.SetNormal(0,0,0);
                points[i] = p;
            }

            int i1, i2, i3;
            for (int i = 0; i < indices.Count / 3; i++)
            {
                i1 = indices[3 * i];
                i2 = indices[3 * i + 1];
                i3 = indices[3 * i + 2];

                if (i1 < 0 || i2 < 0 || i3 < 0 ||
                    i1 >= points.Count || i2 >= points.Count || i3 >= points.Count)
                    continue;

                vec3 normal = GetNormal(points[i1].pos, points[i2].pos, points[i3].pos);

                p1 = points[i1];
                p2 = points[i2];
                p3 = points[i3];
                
                p1.AddNormal(normal);
                p2.AddNormal(normal);
                p3.AddNormal(normal);

                points[i1] = p1;
                points[i2] = p2;
                points[i3] = p3;
            }
            return true;
        }
        //根据顶点及三角形indices，计算法线量，结果累加到顶点数组Normal中
        static public bool SetNormals(Vertex3D[] points, vec3[] normals)
        {
            if (points == null || normals == null)
            {
                //errMessage = "no enough points or indices.";
                return false;
            }

            for (int i = 0; i < normals.Length; i++)
            { 
                if( points.Length > i )
                    points[i].SetNormal(normals[i]); 
            }
            
            return true;
        }
        //根据顶点及三角形indices，计算法线量，结果累加到顶点数组Normal中
        static public bool CalculateNormals(Vertex3D[] points, int[] indices)
        {
            if (points == null || indices == null)
            {
                //errMessage = "no enough points or indices.";
                return false;
            }

            for (int i = 0; i < points.Length; i++)
                points[i].SetNormal(0, 0, 0);

            int i1, i2, i3;
            for (int i = 0; i < indices.Length / 3; i++)
            {
                i1 = indices[3 * i];
                i2 = indices[3 * i + 1];
                i3 = indices[3 * i + 2];

                if ( i1 < 0 || i2 < 0 || i3 < 0 || 
                     i1 >= points.Length || i2 >= points.Length || i3 >= points.Length)
                    continue;

                vec3 normal = GetNormal(points[i1].pos, points[i2].pos, points[i3].pos);
                points[i1].AddNormal(normal);
                points[i2].AddNormal(normal);
                points[i3].AddNormal(normal);
            }
            return true;
        }
        public virtual Vertex3D CreatePoint( vec3 p)
        {
            return new Vertex3D(p.x,p.y,p.z);
        }
        public virtual Vertex3D CreatePoint(vec4 p)
        {
            return new Vertex3D(p.x, p.y, p.z);
        }
        public virtual Vertex3D CreatePoint(double x,double y,double z)
        {
            return new Vertex3D((float)x, (float)y, (float)z);
        }
        public virtual Vertex3D CreatePoint(Vector32 p)
        {
            return new Vertex3D(p.X, p.Y, p.Z);
        }
        public virtual Vertex3D CreatePoint(Vector64 p)
        {
            return new Vertex3D((float)p.X, (float)p.Y, (float)p.Z);
        }
        public virtual void Begin(DrawingPrimitive _primitive = DrawingPrimitive.TRIANGLE_LIST)
        {
            return;
        }
        public virtual bool End( bool _createVerticesBuffer = true, 
                                 bool _createIndexBuffer = true, 
                                 bool _createUniformBuffer = true) 
        {
            return true;
        }

        //public vec4 clearColor = new vec4(0,0,0,1);
        public vec4 clearColor = new vec4(0.125f, 0.25f, 0.5f, 1);
        public virtual void SetClearColor(vec4 _color)
        {
            clearColor = _color;
        }
        public virtual void SetClearColor(float r, float g, float b, float a = 1.0f)
        {
            clearColor.x = r;
            clearColor.y = g;
            clearColor.z = b;
            clearColor.w = a;
        }
        public virtual void ClearColor() { }
        public virtual void ClearColor(vec4 color) { }

        public virtual void SetColor(vec3 color,float alpha = 1)
        {
            curColor = new vec4(color.x, color.y, color.z, alpha);
        }
        public virtual void SetColor(vec4 _color)
        {
            curColor = _color;
        }
        public virtual void SetColor(vec4 _color,float alpha)
        {
            curColor = _color;
            curColor.w = alpha;
        }
        public virtual void SetColor( Color color,float alpha = 1)
        {
            SetColor((float)(color.R / 255.0), (float)(color.G / 255.0), (float)(color.B / 255.0), alpha);
        }
        public virtual void SetColor(float r, float g, float b, float a = 1.0f)
        {
            curColor = new vec4(r, g, b, a);
        }
        public virtual void SetRGBAColor(byte r, byte g, byte b, byte a = 255)
        {
            curColor = new vec4(r / 255, g / 255, b / 255, a / 255);
        }
        public virtual void SetColor(double r, double g, double b, double a = 1.0f)
        {
            curColor = new vec4((float)r, (float)g, (float)b, (float)a);
        }
        //set current draw parameters:polygon fill,linewidth,point size
        public virtual void SetPolygonMode(gDrawMode _mode = gDrawMode.Fill)
        {
            curDrawParameter.polygonMode = _mode;
        }
        public virtual void SetPointSize(double _size = 1.0f)
        {
            curDrawParameter.pointSize = (float)_size;
        }
        public virtual void SetLineWidth(double _width = 1.0f)
        {
            curDrawParameter.lineWidth = (float)_width;
        }
        public virtual void SetLineStyle(gLineStyle style)
        {
            curDrawParameter.lineStyle = style;
        }
        public virtual void AddPoint(Vertex3D point)
        {
            pVertics.Add(point);
        }
        public virtual void AddPointIndex(int indice)
        {
            pIndices.Add(indice);
        }
        public virtual void AddPoints(Vertex3D[] points)
        {
            verticesArray = points;
            pVertics.Clear();
        }        
        public virtual void AddPointIndices(int[] indices)
        {
            indicesArray = indices;
            pIndices.Clear();
        }        

        /////////////////点/////////////////////////////
        public virtual void DrawPoint(Vertex3D p)
        {
            Begin(DrawingPrimitive.POINT_LIST);
            AddPoint(p);
            AddPointIndex(0);
            End();
        }
        public virtual void DrawPoints(Vertex3D[] points)
        {
            if (points == null) return;
            int n = points.Length;
            if (n <= 0) return;
            Begin(DrawingPrimitive.POINT_LIST);
            for (int i = 0; i < n; i++)
            {
                AddPoint(points[i]);
                AddPointIndex(i);
            }
            End();
        }
        /////////////////线/////////////////////////////
        public virtual void DrawLine(Vertex3D p1, Vertex3D p2,bool resetColor = true)
        {
            if(resetColor)
            {
                p1.SetColor(curColor);
                p2.SetColor(curColor);
            }            
            BeginLines();
            AddPoint(p1);
            AddPoint(p2);
            AddPointIndex(0);
            AddPointIndex(1);
            EndLines();
        }
        //绘制线
        public virtual void DrawLine( Vertex3D p1, Vertex3D p2, vec4 color) 
        {
            p1.SetColor(color);
            p2.SetColor(color);
            BeginLines();
            AddPoint(p1);
            AddPoint(p2);
            AddPointIndex(0);
            AddPointIndex(1);
            EndLines();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="close"> Draw Closed Line </param>
        /// <param name="resetColor"> use current color to replace points color</param>
        public virtual void DrawLines(Vertex3D[] points, bool close = false, bool resetColor = false)
        {
            if (points.Length < 2) return;
            int n = points.Length;
            if (close) n = points.Length + 1;

            Begin(DrawingPrimitive.LINE_STRIP);

            verticesArray = points;
            indicesArray = new int[n];

            if (resetColor)//颜色重置
            {
                for (int i = 0; i < verticesArray.Length; i++)
                    verticesArray[i].color = curColor;
            }

            for (int i = 0; i < points.Length; i++)
            {
                indicesArray[i] = i;
            }

            if (close) indicesArray[n - 1] = 0;

            End();           
        }
        public virtual void DrawLines(Vertex3D[] points, int[] indices, bool resetColor = false)
        {
            if (points.Length < 2) return;
            int n = points.Length;

            Begin(DrawingPrimitive.LINE_STRIP);

            verticesArray = points;
            indicesArray = indices;

            if (resetColor)//颜色重置
            {
                for (int i = 0; i < verticesArray.Length; i++)
                    verticesArray[i].color = curColor;
            }

            End();
        }
        public virtual void DrawLinesList(Vertex3D[] points, int[] indices, bool resetColor = false)
        {
            if (points.Length < 2) return;
            int n = points.Length;

            Begin(DrawingPrimitive.LINE_LIST);

            verticesArray = points;
            indicesArray = indices;

            if (resetColor)//颜色重置
            {
                for (int i = 0; i < verticesArray.Length; i++)
                    verticesArray[i].color = curColor;
            }

            End();
        }
        public virtual void BeginLines()
        {
            Begin(DrawingPrimitive.LINE_STRIP);
        }
        public virtual bool EndLines() 
        {
            return End();
        }
        public virtual void BeginTriangles() 
        {
            Begin(DrawingPrimitive.TRIANGLE_LIST);
        }
        public virtual bool EndTriangles() 
        { 
            return End(); 
        }

        //绘制三角形对象，一次性绘制
        public virtual void DrawTriangle(Vertex3D[] points, int[] indices, vec3[]normals = null)
        {
            int np = points.Length;
            int na = indices.Length;
            if (np < 3 || na < 1) return;

            PushMatrix();
            
            BeginTriangles();

            verticesArray = points;
            indicesArray = indices;

            if (normals == null) CalculateNormals(verticesArray, indicesArray);
            else SetNormals(verticesArray,normals);

            EndTriangles();

            PopMatrix();            
        }

        //绘制三角形对象，分批次性绘制，如果三角形数据量大，需多次绘制
        //int maxTrianglesNum = 200000 一次性绘制最大三角形数，可根据显存计算一个合适量
        public virtual void DrawTriangles(Vertex3D[] points, int[] indices, vec3[]normals = null)
        {
            if (points.Length < 3 || indices.Length < 3) return;
            
            //计算批次n
            int n = indices.Length / (maxTrianglesNum * 3);
            if (n < 1)//只需绘制一次
            {
                DrawTriangle(points, indices, normals);
                return;
            }

            //多次绘制
            //顶点坐标所在位置
            long[] pointIndices = new long[points.Length];
            long id = 0, len = maxTrianglesNum * 3;//三角形indices数;
            long k = 0;
            for (long i = 0; i < n + 1; i++)
            {
                if (i == n) //the last segment
                {
                    len = indices.Length - i * len;
                    if (len < 3) break;
                }

                BeginTriangles();   

                Array.Clear(pointIndices, 0, pointIndices.Length);
                for (int j = 0; j < len; j++)
                {
                    k = indices[id + j];
                    if (pointIndices[k] == 0)//数组中未添加点
                    {
                        AddPoint(points[k]);
                        pointIndices[k] = pVertics.Count;
                    }
                    AddPointIndex((int)(pointIndices[k] - 1));
                }
                id += maxTrianglesNum * 3;

                EndTriangles();
            }//for (long i = 0; i < n + 1; i++)          

        }

        public virtual vec3 SetTriangleNormal(ref Vertex3D p1, ref Vertex3D p2, ref Vertex3D p3)
        {
            vec3 normal = GetNormal(p1.pos, p2.pos, p3.pos);
            p1.AddNormal(normal);
            p2.AddNormal(normal);
            p3.AddNormal(normal);
            return normal;
        }    

        public virtual void DrawShpere(vec3 pos, CSphere obj, vec4[] colors = null) { }
        public virtual void DrawShpere(Vertex3D p, CSphere obj, vec4[] colors = null) { }
        public virtual void DrawShpere(CSphere sp, vec4[] colors = null) { }
        public virtual void DrawCylinder(vec3 pos, CCylinder obj, vec4[] colors = null) { }
        public virtual void DrawCylinder(Vertex3D p, CCylinder obj, vec4[] colors = null) { }
        public virtual bool DrawCylinder(CCylinder sp, vec4[] colors = null) { return false; }

        public virtual void DrawMesh(Vertex3D[] points, int row, int col,
                                bool autoNormal = true,
                                bool resetColor = true,
                                bool autoTexCoord = false)
        { }

               
        public virtual void BeginBoxes()
        {
            Begin(DrawingPrimitive.TRIANGLE_LIST);
        }
        public virtual void EndBoxes()
        {
            End();
        }

        public virtual void TriangleMemory(TriangleObj obj)
        {
            if ( obj.points.Count < 3 ) return;
            float alpha = 1.0f;
            if (obj.Blend) alpha = obj.Alpha;

            Vertex3D[] points = new Vertex3D[obj.points.Count];
            for (int i = 0; i < obj.points.Count; i++)
            {
                points[i] = new Vertex3D(obj.points[i].x, obj.points[i].y, obj.points[i].z);
                if (obj.IsUniformColor) points[i].SetColor( ConvertColor(obj.uniformColor));
            }
            if (obj.colors.Count == obj.points.Count && !obj.IsUniformColor )
            {
                for (int i = 0; i < obj.points.Count; i++)
                    points[i].SetColor(obj.colors[i]);
            }
            
            for (int i = 0; i < obj.points.Count; i++) 
                points[i].color.w = alpha;

            if ( obj.texCoords.Count == obj.points.Count )
            {
                for (int i = 0; i < obj.points.Count; i++)
                    points[i].SetTexcoord(obj.texCoords[i]);
            }
            int start = pVertics.Count;
            for (int i = 0; i < obj.points.Count; i++)
            {
                AddPoint(points[i]);
            }
            int id1, id2, id3;
            for (int i = 0; i < obj.triangles.Count; i++)
            {
                id1 = obj.triangles[i].x + start;
                id2 = obj.triangles[i].y + start;
                id3 = obj.triangles[i].z + start;
                AddPointIndex(id1);
                AddPointIndex(id2);
                AddPointIndex(id3);
            }
            points = null;
        }
        public virtual void BoxMemory(GridBox box)
        {
            //      |(y)
            //      p3--------p2 
            //      |         |
            //   p7 |     p6  |
            //   |  /p0---|---p1--->(x)
            //   | /      | / 
            // p4|/-------p5--->east 
            //   / (z)
            if (box.points == null || box.faces == null) return;
            int[] up = new int[] { 2, 3, 7, 7, 6, 2 };
            int[] down = new int[] { 0, 1, 5, 5, 4, 0 };
            int[] left = new int[] { 3, 0, 4, 4, 7, 3 };
            int[] right = new int[] { 1, 2, 6, 6, 5, 1 };
            int[] front = new int[] { 4, 5, 6, 6, 7, 4 };
            int[] back = new int[] { 3, 2, 1, 1, 0, 3 };

            int i;

            Vertex3D[] points = new Vertex3D[box.points.Length];
            for (i = 0; i < points.Length; i++)
            {
                points[i] = new Vertex3D(box.points[i].x, box.points[i].y, box.points[i].z);
            }
            int start = pVertics.Count;

            if (box.colors == null)
            {
                for (i = 0; i < 8; i++)
                    points[i].SetColor(curColor);
            }
            else
            {
                for (i = 0; i < box.colors.Length && i < 8; i++)
                    points[i].SetColor(box.colors[i]);
            }

            points[0].SetNormal(0, 0, 0); points[1].SetNormal(0, 0, 0);
            points[2].SetNormal(0, 0, 0); points[3].SetNormal(0, 0, 0);
            points[4].SetNormal(0, 0, 0); points[5].SetNormal(0, 0, 0);
            points[6].SetNormal(0, 0, 0); points[7].SetNormal(0, 0, 0);

            if (box.faces[0])    //up
            {
                for (i = 0; i < up.Length; i++) pIndices.Add(up[i] + start);
                points[2].AddNormal(0, 1, 0); points[3].AddNormal(0, 1, 0);
                points[6].AddNormal(0, 1, 0); points[7].AddNormal(0, 1, 0);
            }
            if (box.faces[1])    //down
            {
                for (i = 0; i < down.Length; i++) pIndices.Add(down[i] + start);
                points[0].AddNormal(0, -1, 0); points[1].AddNormal(0, -1, 0);
                points[4].AddNormal(0, -1, 0); points[5].AddNormal(0, -1, 0);
            }
            if (box.faces[2])    //left
            {
                for (i = 0; i < left.Length; i++) pIndices.Add(left[i] + start);
                points[0].AddNormal(-1, 0, 0); points[3].AddNormal(-1, 0, 0);
                points[4].AddNormal(-1, 0, 0); points[7].AddNormal(-1, 0, 0);
            }
            if (box.faces[3])    //right
            {
                for (i = 0; i < right.Length; i++) pIndices.Add(right[i] + start);
                points[1].AddNormal(1, 0, 0); points[2].AddNormal(1, 0, 0);
                points[5].AddNormal(1, 0, 0); points[6].AddNormal(1, 0, 0);
            }
            if (box.faces[4])    //front
            {
                for (i = 0; i < front.Length; i++) pIndices.Add(front[i] + start);
                points[4].AddNormal(0, 0, 1); points[6].AddNormal(0, 0, 1);
                points[5].AddNormal(0, 0, 1); points[7].AddNormal(0, 0, 1);
            }
            if (box.faces[5])    //back
            {
                for (i = 0; i < back.Length; i++) pIndices.Add(back[i] + start);
                points[0].AddNormal(0, 0, -1); points[1].AddNormal(0, 0, -1);
                points[2].AddNormal(0, 0, -1); points[3].AddNormal(0, 0, -1);
            }
            for (i = 0; i < 8; i++) Vertex(points[i]);

            //release memory
            points = null;
            up = down = null;
            left = right = null;
            front = back = null;
        }
        public virtual void DrawBox(GridBox box) { }
        public virtual void DrawBoxOutline(GridBox box) { }
        public virtual void DrawBox(double x, double y, double z, double xlen, double ylen, double zlen, vec4[] _colors=null) { }
        public virtual void DrawBoxOutline(double x, double y, double z, double xlen, double ylen, double zlen, vec4[] _colors=null) { }

        //reset all parameters and redraw all objects,
        public virtual void UpdateDraw() { }
        //clear all objects
        public virtual void ClearDrawBuffer() 
        {
            lock (modelLocker)
            {
                var obj = objectModels.GetEnumerator();
                while(obj.MoveNext())
                {
                    CModel model = obj.Current.Value;
                    if( model != null )model.Release();
                }
                objectModels.Clear();
                pVertics.Clear();
                pIndices.Clear();
                verticesArray = null;
                indicesArray = null;                
            }            
        }
       
        #endregion functions
        #region common functions
        public Vertex3D toVertex3D(vec3 p)
        {
            return new Vertex3D(p.x, p.y, p.z);
        }
        public Vertex3D toVertex3D(Vector32 p)
        {
            return new Vertex3D(p.x, p.y, p.z);
        }
        public Vertex3D toVertex3D(Vector64 p)
        {
            return new Vertex3D((float)p.X, (float)p.Y, (float)p.Z);
        }
        public Vertex3D toVertex3D(float x,float y,float z)
        {
            return new Vertex3D(x,y,z);
        }
        static public vec3 GetNormal(vec3 p1, vec3 p2, vec3 p3)
        {
            vec3 pn = new vec3(0, 0, 0);
            double ax = p1.x - p2.x;
            double ay = p1.y - p2.y;
            double az = p1.z - p2.z;

            double bx = p2.x - p3.x;
            double by = p2.y - p3.y;
            double bz = p2.z - p3.z;

            double dx = ay * bz - az * by;
            double dy = az * bx - ax * bz;
            double dz = ax * by - ay * bx;

            ax = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (ax > 0)
            {
                pn.x = (float)(dx / ax);
                pn.y = (float)(dy / ax);
                pn.z = (float)(dz / ax);
            }
            return pn;
        }
        #endregion common functions
    }//class CGraphic3D

    public struct LightStruct
    {        
        public vec4 type;       //type.w >0 enabled, else disabled
        public vec4 pos;        //pos.xyz use as position, pos.w >0 enable
        public vec4 ambient;
        public vec4 diffuse;
        public vec4 specular;
        public LightStruct(bool autoInitial = true)
        {
            // type.w >0 enabled,type.x == 0 point light
            // defined in shader.vert
            type = new vec4(0, 0, 0, 0);
            pos = new vec4(0, 0, 0, 1);            
            ambient = new vec4(0.02f, 0.02f, 0.02f, 1.0f);
            diffuse = new vec4(0.8f, 0.8f, 0.8f, 1.0f);
            specular = new vec4(0.8f, 0.8f, 0.8f, 1.0f);
        }
        public bool SaveAs(BinaryWriter br)
        {
            br.Write(type.x);
            br.Write(type.y);
            br.Write(type.z);
            br.Write(type.w);

            br.Write(pos.x);
            br.Write(pos.y);
            br.Write(pos.z);
            br.Write(pos.w);

            br.Write(ambient.x);
            br.Write(ambient.y);
            br.Write(ambient.z);
            br.Write(ambient.w);

            br.Write(diffuse.x);
            br.Write(diffuse.y);
            br.Write(diffuse.z);
            br.Write(diffuse.w);

            br.Write(specular.x);
            br.Write(specular.y);
            br.Write(specular.z);
            br.Write(specular.w);
            return true;
        }
        public bool LoadFrom(BinaryReader br)
        {
            float x, y, z, w;
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            type = new vec4(x,y,z,w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            pos = new vec4(x, y, z, w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            ambient = new vec4(x, y, z, w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            diffuse = new vec4(x, y, z, w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            specular = new vec4(x, y, z, w);
            return true;
        }

        public bool Enable
        {
            get 
            {
                if ( type.w > 0 ) return true;
                else return false;
            }
            set 
            {
                if (value == true) type.w = 1;
                else type.w = 0;
            }            
        }
        public static int GetSize()
        {
            return sizeof(float) * 4 * 5;   //ambient diffuse specular
        }
        public LightStruct Copy()
        {
            LightStruct ret = new LightStruct(true);            
            ret.type = type;
            ret.pos = pos;
            ret.ambient = ambient;
            ret.diffuse = diffuse;
            ret.specular = specular;
            return ret;
        }
        public byte[] toByteArray()
        {
            int size = GetSize();
            byte[] _array = new byte[size];

            int index = 0;
            index = CopyToBytes(type.to_array(), _array, index);       //type
            index = CopyToBytes(pos.to_array(), _array, index);        //pos
            index = CopyToBytes(ambient.to_array(), _array, index);    //ambient
            index = CopyToBytes(diffuse.to_array(), _array, index);    //diffuse
            index = CopyToBytes(specular.to_array(), _array, index);   //specular

            return _array;
        }
        public float[] toFloatArray()
        {
            int size = GetSize() / sizeof(float);
            float[] _array = new float[size];

            float[] a0 = type.to_array();
            float[] a1 = pos.to_array();
            float[] a2 = ambient.to_array();
            float[] a3 = diffuse.to_array();
            float[] a4 = specular.to_array();

            int index = 0;
            Array.Copy(a0, 0, _array, index, a0.Length);
            index += a0.Length;
            Array.Copy(a1, 0, _array, index, a1.Length);
            index += a1.Length;
            Array.Copy(a2, 0, _array, index, a2.Length);
            index += a2.Length;
            Array.Copy(a3, 0, _array, index, a3.Length);
            index += a3.Length;
            Array.Copy(a4, 0, _array, index, a4.Length);
            index += a4.Length;

            return _array;
        }
        public bool WriteBinary(ref BinaryWriter br)
        {            
            br.Write(type.x);
            br.Write(type.y);
            br.Write(type.z);
            br.Write(type.w);

            br.Write(pos.x);
            br.Write(pos.y);
            br.Write(pos.z);
            br.Write(pos.w);

            br.Write(ambient.x);
            br.Write(ambient.y);
            br.Write(ambient.z);
            br.Write(ambient.w);

            br.Write(diffuse.x);
            br.Write(diffuse.y);
            br.Write(diffuse.z);
            br.Write(diffuse.w);

            br.Write(specular.x);
            br.Write(specular.y);
            br.Write(specular.z);
            br.Write(specular.w);

            return true;
        }
        public bool LoadFromBinary(ref BinaryReader br)
        {
            LightStruct lt = new LightStruct(true);            
            type.x = br.ReadSingle();
            type.y = br.ReadSingle();
            type.z = br.ReadSingle();
            type.w = br.ReadSingle();

            pos.x = br.ReadSingle();
            pos.y = br.ReadSingle();
            pos.z = br.ReadSingle();
            pos.w = br.ReadSingle();

            ambient.x = br.ReadSingle();
            ambient.y = br.ReadSingle();
            ambient.z = br.ReadSingle();
            ambient.w = br.ReadSingle();

            diffuse.x = br.ReadSingle();
            diffuse.y = br.ReadSingle();
            diffuse.z = br.ReadSingle();
            diffuse.w = br.ReadSingle();

            specular.x = br.ReadSingle();
            specular.y = br.ReadSingle();
            specular.z = br.ReadSingle();
            specular.w = br.ReadSingle();

            return true;
        }
        private int CopyToBytes(float[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(float source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(int[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(int source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(bool[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(bool source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(byte[] source, byte[] dest, int start)
        {
            int index = start;
            Array.Copy(source, 0, dest, index, source.Length);
            index += source.Length;
            return index;
        }
    };
    public struct MaterialStruct
    {
        public vec4 shininess;   // Srm  
        public vec4 emission;    // Ecm   
        public vec4 ambient;     // Acm   
        public vec4 diffuse;     // Dcm   
        public vec4 specular;    // Scm 
        public MaterialStruct(bool autoInitial = true)
        {
            shininess = new vec4(0.1f, 0, 0, 60);
            emission = new vec4(0, 0, 0, 1);
            ambient = new vec4(0.03f, 0.03f, 0.03f, 1.0f);
            diffuse = new vec4(0.8f, 0.8f, 0.8f, 1.0f);
            specular = new vec4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        public bool SaveAs(BinaryWriter br)
        {
            br.Write(shininess.x);
            br.Write(shininess.y);
            br.Write(shininess.z);
            br.Write(shininess.w);

            br.Write(emission.x);
            br.Write(emission.y);
            br.Write(emission.z);
            br.Write(emission.w);

            br.Write(ambient.x);
            br.Write(ambient.y);
            br.Write(ambient.z);
            br.Write(ambient.w);

            br.Write(diffuse.x);
            br.Write(diffuse.y);
            br.Write(diffuse.z);
            br.Write(diffuse.w);

            br.Write(specular.x);
            br.Write(specular.y);
            br.Write(specular.z);
            br.Write(specular.w);
            return true;
        }
        public bool LoadFrom(BinaryReader br)
        {
            float x, y, z, w;
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            shininess = new vec4(x, y, z, w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            emission = new vec4(x, y, z, w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            ambient = new vec4(x, y, z, w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            diffuse = new vec4(x, y, z, w);
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();
            w = br.ReadSingle();
            specular = new vec4(x, y, z, w);
            return true;
        }
        public void SetShininess(float _shininess)
        {
            shininess.w = _shininess;
        }
        public void SetNS(float _ns)
        {
            shininess.x = _ns;
        }        
        public static int GetSize()
        {
            return sizeof(float) * 4 * 5;
        }
        public MaterialStruct Copy()
        {
            MaterialStruct ret = new MaterialStruct(true);
            ret.shininess = shininess;
            ret.emission = emission;
            ret.ambient = ambient;
            ret.diffuse = diffuse;
            ret.specular = specular;
            return ret;
        }

        public byte[] toByteArray()
        {
            int size = GetSize();
            byte[] _array = new byte[size];

            int index = 0;
            index = CopyToBytes(shininess.to_array(), _array, index);  //shininess
            index = CopyToBytes(emission.to_array(), _array, index);   //emission
            index = CopyToBytes(ambient.to_array(), _array, index);    //ambient
            index = CopyToBytes(diffuse.to_array(), _array, index);    //diffuse
            index = CopyToBytes(specular.to_array(), _array, index);   //specular

            return _array;
        }
        public float[] toFloatArray()
        {
            int size = GetSize() / sizeof(float);
            float[] _array = new float[size];

            float[] a0 = shininess.to_array();
            float[] a1 = emission.to_array();
            float[] a2 = ambient.to_array();
            float[] a3 = diffuse.to_array();
            float[] a4 = specular.to_array();

            int index = 0;
            Array.Copy(a0, 0, _array, index, a0.Length);
            index += a0.Length;
            Array.Copy(a1, 0, _array, index, a1.Length);
            index += a1.Length;
            Array.Copy(a2, 0, _array, index, a2.Length);
            index += a2.Length;
            Array.Copy(a3, 0, _array, index, a3.Length);
            index += a3.Length;
            Array.Copy(a4, 0, _array, index, a4.Length);
            index += a4.Length;

            return _array;
        }
        private int CopyToBytes(float[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(float source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(int[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(int source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(bool[] source, byte[] dest, int start)
        {
            int index = start;
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(source[i]);
                Array.Copy(bytes, 0, dest, index, bytes.Length);
                index += bytes.Length;
            }
            return index;
        }
        private int CopyToBytes(bool source, byte[] dest, int start)
        {
            int index = start;
            byte[] bytes = BitConverter.GetBytes(source);
            Array.Copy(bytes, 0, dest, index, bytes.Length);
            index += bytes.Length;
            return index;
        }
        private int CopyToBytes(byte[] source, byte[] dest, int start)
        {
            int index = start;
            Array.Copy(source, 0, dest, index, source.Length);
            index += source.Length;
            return index;
        }
    };
    public struct Vertex2D
    {
        public vec2 pos;       //x,y,z
        public vec3 color;     //r,g,b  
        public static int GetSize()
        {
            return Marshal.SizeOf<vec2>() + Marshal.SizeOf<vec3>();
        }
    };
    public struct Vertex3D
    {
        public vec3 pos;       //x,y,z
        public vec4 color;     //r,g,b,a
        public vec2 texCoord;  //texCoord
        public vec3 normal;    //normal        
        public Vertex3D(vec3 _pos, vec4 _color, vec2 _texCoord)
        {
            pos = _pos;
            color = _color;
            texCoord = _texCoord;
            normal = new vec3(0, 0, 0);
        }
        public Vertex3D(vec3 _pos, vec4 _color)
        {
            pos = _pos;
            color = _color;
            texCoord = new vec2(-1, -1);
            normal = new vec3(0, 0, 0);
        }
        public Vertex3D(vec3 _pos)
        {
            pos = _pos;
            color = new vec4(0, 0, 0, 1);
            texCoord = new vec2(-1, -1);
            normal = new vec3(0, 0, 0);
        }
        public Vertex3D(float x, float y, float z)
        {
            pos = new vec3(x, y, z);
            color = new vec4(0, 0, 0, 1);
            texCoord = new vec2(-1, -1);
            normal = new vec3(0, 0, 0);
        }
        public void SetNormal(vec3 _normal)
        {
            this.normal = _normal;
        }
        public void SetNormal(float x, float y, float z)
        {
            this.normal = new vec3(x, y, z);
        }
        public void AddNormal(vec3 _normal)
        {
            this.normal = new vec3(normal.x + _normal.x, normal.y + _normal.y, normal.z + _normal.z);
        }
        public void AddNormal(float x, float y, float z)
        {
            this.normal = new vec3(normal.x + x, normal.y + y, normal.z + z);
        }
        public void SetColor(vec4 _color)
        {
            color = _color;
        }
        public void SetColor(Color _color)
        {
            float x = _color.R / 255f;
            float y = _color.G / 255f;
            float z = _color.B / 255f;
            float w = _color.A / 255f;
            color.x = x;
            color.y = y;
            color.z = z;
            color.w = w;
        }
        public void SetColor(double r, double g, double b, double a =1.0)
        {
            color = new vec4((float)r, (float)g, (float)b, (float)a);
        }        
        public void SetTexcoord(vec2 tex)
        {
            texCoord = tex;
        }
        public void SetTexcoord(float u, float v)
        {
            texCoord = new vec2(u, v);
            //texCoord.x = u;
            //texCoord.y = v;
        }
        public float[] toArray()
        {
            float[] a1 = pos.to_array();
            float[] a2 = color.to_array();
            float[] a3 = texCoord.to_array();
            float[] a4 = normal.to_array();
            float[] _array = new float[a1.Length + a2.Length + a3.Length + a4.Length];
            int index = 0;
            Array.Copy(a1, 0, _array, index, a1.Length);
            index += a1.Length;
            Array.Copy(a2, 0, _array, index, a2.Length);
            index += a2.Length;
            Array.Copy(a3, 0, _array, index, a3.Length);
            index += a3.Length;
            Array.Copy(a4, 0, _array, index, a4.Length);
            index += a4.Length;
            return _array;
        }
        public static int GetSize()
        {
            return 12 * sizeof(float);
            /*return sizeof(float) * 3 +     //pos
                   sizeof(float) * 4 +     //color
                   sizeof(float) * 2 +     //texCoord
                   sizeof(float) * 3;      //normal
                   */
        }        
    };
    public class CModel
    {
        //Hash Code
        public long HashKey = 0;
        public bool Visible = true;
        public DrawingPrimitive primitive = DrawingPrimitive.TRIANGLE_LIST;
        public bool bInitialized = false;
        public UniformBufferObject m_modelMatrix;        
        
        //model transform matrix
        public vec3 m_translate = new vec3(0, 0, 0);
        public vec3 m_scale = new vec3(1, 1, 1);
        public vec3 m_rotate = new vec3(0, 0, 0);

        public bool m_enableTranslate = true;
        public bool m_enableScale = true;
        public bool m_enableRotate = true;
        public string errMessage = "";
        public virtual bool Transparent { get; set; } = false; //是否透明绘制标记
        public virtual bool UpdateUniformBuffer() { return false; }
        public virtual void Release() { } 
        public CModel()
        {
            CreateHashKey();
        }
        public virtual long CreateHashKey()
        {
            HashKey = this.GetHashCode();
            return HashKey;
        }
        public virtual bool IsTriangleMode(DrawingPrimitive pritive)
        {
            switch (pritive)
            {
                case DrawingPrimitive.POINT_LIST: return false;
                case DrawingPrimitive.LINE_LIST: return false;
                case DrawingPrimitive.LINE_STRIP: return false;
                case DrawingPrimitive.TRIANGLE_LIST: return true;
                case DrawingPrimitive.TRIANGLE_STRIP: return true;
                case DrawingPrimitive.TRIANGLE_FAN: return true;
                case DrawingPrimitive.LINE_LIST_WITH_ADJACENCY: return false;
                case DrawingPrimitive.LINE_STRIP_WITH_ADJACENCY: return false;
                case DrawingPrimitive.TRIANGLE_LIST_WITH_ADJACENCY: return true;
                case DrawingPrimitive.TRIANGLE_STRIP_WITH_ADJACENCY: return true;
                case DrawingPrimitive.PATCH_LIST: return false;
                default: return false;
            }
        }

    }//end of class CModel
}//end of the file/////////////////////////////////////
