using System;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;
using GlmNet;
using OpenGL;
namespace Graphics3D
{
    public class SharpGLNet: CGraphic3D
    {
        private object modelLocker = new object();
        private MyGLModel curModel = null;             
        public uint program = 0;
        public uint vertexShaderId = 0;
        public uint fragmentShaderId = 0;        

        public SharpGLNet()
        {
            engine = gEngine.opengl;
        }        
        //实现窗口创建，三维点数据存储，基本图元绘制(点，线，三角形等）
        //Shader载入和使用，实现旋转矩阵，平移缩放矩阵的计算
        public override bool Initialize(IntPtr window, string title = "OpenGLES", int width = 800, int height = 600)
        {
            windowWidth = width;
            windowHeight = height;
           
            if ( LoadProgram("GLES-vert.c", "GLES-frag.c") < 1 )
            {
                return false;
            }

            initialized = true;
            LoadIdentity();
            return initialized;
        }
        public override void onWindowResized(int width, int height)
        {
            if (width == 0 || height == 0) return;
            windowWidth = width;
            windowHeight = height;
            UpdateDraw();
        }
        public override bool DestroyWindow()
        {            
            ClearDrawBuffer();
            return true;
        }
        public override string GetGraphicsInfoString()
        {
            string info = "";
            string name = Gl.GetString( StringName.Vendor);//返回负责当前OpenGL实现厂商的名字
            string biaozhi = Gl.GetString(StringName.Renderer);//返回一个渲染器标识符，通常是个硬件平台
            string version = Gl.GetString( StringName.Version);//返回当前OpenGL实现的版本号
            string extension = Gl.GetString( StringName.Extensions);
            string glslversion = Gl.GetString( StringName.ShadingLanguageVersion);
            info += "Name:" + name + "\n";
            info += "Render:" + biaozhi + "\n";
            info += "Version:" + version + "\n";            
            info += "GLSL Version:" + glslversion + "\n";
          //  info += "Extension:" + extension + "\n";
            return info;
        }
        public override string GetGraphicName()
        {
            return Gl.GetString(StringName.Renderer);//返回一个渲染器标识符，通常是个硬件平台            
        }
        
        public override int BindTexture(byte[] texBytes, int width,int height, DataCollection.TextureMagFilter mode = DataCollection.TextureMagFilter.GL_LINEAR)
        {
            if (texBytes == null) return 0;
            if (program <= 0) return 0;
            uint id = Gl.GenTexture();          

            Gl.BindTexture(TextureTarget.Texture2d, id);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, OpenGL.TextureMagFilter.Linear);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, OpenGL.TextureMagFilter.Linear);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, OpenGL.TextureWrapMode.ClampToEdge);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, OpenGL.TextureWrapMode.ClampToEdge);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, texBytes);
            return (int)id;
        }
        public override int BindTexture(Bitmap bmp, DataCollection.TextureMagFilter mode = DataCollection.TextureMagFilter.GL_LINEAR)
        {
            if (bmp == null) return 0;
            if (program <= 0) return 0;
            uint id = Gl.GenTexture();

            CTexture texture = new CTexture();
            texture.forcePowerOfTwo = false;
            texture.maxTextureSize = GetMax2DTextureImageSize();
            textureBitmap = texture.CreateCompitableBitmap(bmp);
            int width = textureBitmap.Width;
            int height = textureBitmap.Height;

            BitmapData bd = textureBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] newByte = new byte[width * height * 4];
            System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, newByte, 0, newByte.Length);
            textureBitmap.UnlockBits(bd);            

            return BindTexture(newByte, width, height, mode);            
        }
        private void DeleteTexture(int id)
        {
            if (id < 1) return;
            uint[] texid = new uint[] { (uint)id };
            Gl.DeleteTextures(texid);
        }
        private string GetShaderInfoLog(uint id)
        {
            int len;
            StringBuilder infolog = new StringBuilder(1024);
            Gl.GetShaderInfoLog(id, 1024, out len, infolog);
            errMessage = infolog.ToString();
            return errMessage;
        }
        private string GetProgramInfoLog(uint id)
        {
            int len;
            StringBuilder infolog = new StringBuilder(1024);
            Gl.GetProgramInfoLog(id, 1024, out len, infolog);
            errMessage = infolog.ToString();
            return errMessage;
        }
        uint CompileShader(string []source, ShaderType type)
        {
            uint shaderid = Gl.CreateShader(type);
            int compiled = 0;
            Gl.ShaderSource(shaderid, source);
            Gl.CompileShader(shaderid);
            Gl.GetShader(shaderid, ShaderParameterName.CompileStatus, out compiled);
            if (compiled == 0) 
            {
                GetShaderInfoLog(shaderid);
                return 0; 
            }
            return shaderid;
        }
        public void DeleteProgram()
        {            
            if( vertexShaderId > 0 )Gl.DeleteShader(vertexShaderId);
            if ( fragmentShaderId > 0) Gl.DeleteShader(fragmentShaderId);
            if ( program > 0) Gl.DeleteProgram(program);       
        }
        public uint LoadProgram(string vertFile = @"shader\GLES-vert.c", string fragFile = @"shader\GLES-frag.c")
        {
            string []vertSource = LoadShaderSource(vertFile).ToArray();
            string []fragSource = LoadShaderSource(fragFile).ToArray();
            if( vertSource.Length < 1 || fragSource.Length < 1)
            {
                 errMessage = "Shader not found.";
                return 0;
            }
            vertexShaderId = CompileShader(vertSource, ShaderType.VertexShader);
            if ( vertexShaderId <= 0  )
            {
                DeleteProgram();
                return 0;
            }
            
            fragmentShaderId = CompileShader(fragSource, ShaderType.FragmentShader);
            if (fragmentShaderId <= 0)
            {                
                DeleteProgram();
                return 0;
            }

            program = Gl.CreateProgram();
            if( program <= 0  )
            {
                GetProgramInfoLog(program);
                DeleteProgram();
                return 0;
            }

            Gl.AttachShader(program, vertexShaderId);
            Gl.AttachShader(program, fragmentShaderId);
            Gl.LinkProgram(program);

            int linked;
            Gl.GetProgram(program, ProgramProperty.LinkStatus, out linked);
            if (linked == 0)
            {
                GetProgramInfoLog(program);
                DeleteProgram();
                return 0;
            }
            return program;
        }
        Matrix4x4f toMatrix4x4f(mat4 A)
        {
            Matrix4x4f B = new Matrix4x4f();
            for (uint row = 0; row < 4; row++)
                for (uint col = 0; col < 4; col++)
                    B[col, row] = A[(int)col, (int)row];
            return B;
        }        
        ///////////////////////////////////////////////////
        void SetUniformParameters()
        {
            //修改模型矩阵============================================================================================================================================================
            int proj = Gl.GetUniformLocation(program, "project.proj");
            Gl.UniformMatrix4f(proj, 1, false, toMatrix4x4f(m_modelMatrix.proj));
            int model = Gl.GetUniformLocation(program, "project.model");            
            Gl.UniformMatrix4f(model, 1, false, toMatrix4x4f(m_modelMatrix.model));
            int view = Gl.GetUniformLocation(program, "project.view");
            Gl.UniformMatrix4f(view, 1, false, toMatrix4x4f(m_modelMatrix.view));
            int normal = Gl.GetUniformLocation(program, "project.normal");
            Gl.UniformMatrix4f(normal, 1, false, toMatrix4x4f(m_modelMatrix.normal));
            int eyepos = Gl.GetUniformLocation(program, "project.eyepos");
            Gl.Uniform4(eyepos, m_modelMatrix.eyepos[0], m_modelMatrix.eyepos[1], m_modelMatrix.eyepos[2], m_modelMatrix.eyepos[3]);
            //=================================================================================================================================
            
            //light==========================================================================
            //创建当前开启的灯光数组，已开启的放在数组前
            List<LightStruct> lights = new List<LightStruct>();
            int lightNum = m_modelMatrix.lightsNum;
            for(int i=0;i< m_modelMatrix.lights.Length;i++)
            {
                if( m_modelMatrix.lights[i].Enable )
                {
                    lights.Add(m_modelMatrix.lights[i]);
                }
            }

            int lightnumpos = Gl.GetUniformLocation(program, "lightNum");
            Gl.Uniform1(lightnumpos, lightNum);

            for ( int i=0; i <lightNum;i++ )
            {
                int type = Gl.GetUniformLocation(program, "light"+i + ".type");
                Gl.Uniform4(type, lights[i].type.x, lights[i].type.y, lights[i].type.z, lights[i].type.w);
                int pos = Gl.GetUniformLocation(program, "light" + i + ".pos");
                Gl.Uniform4(pos, lights[i].pos.x, lights[i].pos.y, lights[i].pos.z, lights[i].pos.w);
                int ambient = Gl.GetUniformLocation(program, "light" + i + ".ambient");
                Gl.Uniform4(ambient, lights[i].ambient.x, lights[i].ambient.y, lights[i].ambient.z, lights[i].ambient.w);
                int diffuse = Gl.GetUniformLocation(program, "light" + i + ".diffuse");
                Gl.Uniform4(diffuse, lights[i].diffuse.x, lights[i].diffuse.y, lights[i].diffuse.z, lights[i].diffuse.w);
                int specular = Gl.GetUniformLocation(program, "light" + i + ".specular");
                Gl.Uniform4(specular, lights[i].specular.x, lights[i].specular.y, lights[i].specular.z, lights[i].specular.w);
            }

        }
        /// <summary>
        /// convert Bitmap data to bytes array
        /// </summary>
        /// <param name="bmp">Bitmap</param>
        /// <param name="bitNum">number of Bits</param>
        /// <param name="inverse">convert Bits order RGB to BGR</param>
        /// <returns></returns>
        public override byte[] BitmapToBytes(Bitmap bmp, int bitNum = 4, bool inverse = false)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bd = null;
            if (bitNum == 3)
            {
                bd = textureBitmap.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                byte[] newByte = new byte[width * height * bitNum];
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, newByte, 0, newByte.Length);
                textureBitmap.UnlockBits(bd);   
                if(inverse) return BitmapBytesConvert(newByte,3);
                else return newByte;
            }
            else if(bitNum == 4)
            {
                bd = textureBitmap.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                byte[] newByte = new byte[width * height * bitNum];
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, newByte, 0, newByte.Length);
                textureBitmap.UnlockBits(bd);
                if(inverse) return BitmapBytesConvert(newByte,4);
                else return newByte;
            }
            errMessage = "not a supported Bit Number.";
            return null;
        }
        public override void AddModel(CModel cmodel)
        {
            MyGLModel model = cmodel as MyGLModel;
            if (bEnableTexture)
            {
                if (textureBitmap != null)
                {
                    model.textureBytes = BitmapToBytes(textureBitmap, 4, false);
                    model.textureMode = textureMode;
                    model.texWidth = textureBitmap.Width;
                    model.texHeight = textureBitmap.Height;
                    //model.texBitmap = textureBitmap;
                    //model.texBitmap = null;
                }
            }

            model.polygonMode = curDrawParameter.polygonMode;
            model.lineStyle = curDrawParameter.lineStyle;
            model.lineWidth = curDrawParameter.lineWidth;

            //lock (modelLocker)
            {
                objectModels.Add(model.HashKey, model);
                objectModelKeyBuffers.Add(model.HashKey);
            }
        }
        public void AddModel(MyGLModel model)
        {           
            if (bEnableTexture)
            {
                if (textureBitmap != null)
                {
                    model.textureBytes = BitmapToBytes(textureBitmap, 4, false);
                    model.textureMode = textureMode;
                    model.texWidth = textureBitmap.Width;
                    model.texHeight = textureBitmap.Height;
                    //model.texBitmap = textureBitmap;
                    //model.texBitmap = null;
                }
            }

            model.polygonMode = curDrawParameter.polygonMode;
            model.lineStyle = curDrawParameter.lineStyle;
            model.lineWidth = curDrawParameter.lineWidth;

            //lock (modelLocker)
            {          
                objectModels.Add(model.HashKey, model);
                objectModelKeyBuffers.Add(model.HashKey);
            }
        }
        //public List<MyGLModel> models = new List<MyGLModel>();   
        //public void AddModel(MyGLModel model)
        //{
        //    if (bEnableTexture)
        //    {
        //        if (textureBitmap != null)
        //        {
        //            model.textureBytes = BitmapToBytes(textureBitmap, 4, false);
        //            model.textureMode = textureMode;
        //            model.texWidth = textureBitmap.Width;
        //            model.texHeight = textureBitmap.Height;
        //            //model.texBitmap = textureBitmap;
        //            //model.texBitmap = null;
        //        }
        //    }

        //    model.polygonMode = curDrawParameter.polygonMode;
        //    model.lineStyle = curDrawParameter.lineStyle;
        //    model.lineWidth = curDrawParameter.lineWidth;

        //    models.Add(model);
        //}


        public override void Begin(DrawingPrimitive _primitive = DrawingPrimitive.TRIANGLE_LIST)
        {
            lock (modelLocker)
            {                
                curModel = new MyGLModel(_primitive);
                pVertics.Clear();
                pIndices.Clear();
                verticesArray = null;
                indicesArray = null;
            }
            
        }

        public override bool End(bool _createVerticesBuffer = true, bool _createIndexBuffer = true, bool _createUniformBuffer = true)
        {
            if (_createVerticesBuffer)
            {
                if (verticesArray != null && verticesArray.Length > 0)
                {
                    if (!curModel.CreateVerticesBuffer(verticesArray, bEnableTexture))
                    {
                        curModel.Clear();
                        return false;
                    }
                }
                else if (pVertics.Count > 0)
                {
                    if (!curModel.CreateVerticesBuffer(pVertics, bEnableTexture))
                    {
                        curModel.Clear();
                        return false;
                    }
                }
            }

            if (_createIndexBuffer)//create index buffers
            {
                if (indicesArray != null && indicesArray.Length > 0)
                {
                    if (!curModel.CreateIndicesBuffer(indicesArray))
                    {
                        curModel.Clear();
                        return false;
                    }
                }
                else if (pIndices.Count > 0)
                {
                    if (!curModel.CreateIndicesBuffer(pIndices))
                    {
                        curModel.Clear();
                        return false;
                    }
                }
            }

            curModel.CreateNormalBuffer();

            //create successed,add to models list        
            lock (modelLocker)
            {
                AddModel(curModel);
            }

            //base.ClearDrawBuffer();

            return true;
        }
        public override void DrawBoxOutline(double x, double y, double z, 
                                            double xlen, double ylen, double zlen, 
                                            vec4[] _colors = null)
        {
            GridBox box = new GridBox();
            box.Create(x, y, z, xlen, ylen, zlen, _colors);
            DrawBoxOutline(box);
            box.Destroy();
        }
        public override void DrawBoxOutline(GridBox box)
        {
            Vertex3D[] p = new Vertex3D[8];

            for (int i = 0; i < 8; i++)
                p[i] = toVertex3D(box.points[i]);

            if (box.colors == null)
                for (int i = 0; i < 8; i++)
                    p[i].SetColor(curColor);
            else for (int i = 0; i < 8; i++)
                    p[i].SetColor(box.colors[i]);

            Begin(DrawingPrimitive.LINE_STRIP);
            Vertex(p[0]); Vertex(p[1]); Vertex(p[2]); Vertex(p[3]);
            VertexIndex(0); VertexIndex(1);
            VertexIndex(2); VertexIndex(3);
            VertexIndex(0);
            End();
            Begin(DrawingPrimitive.LINE_STRIP);
            Vertex(p[4]); Vertex(p[5]); Vertex(p[6]); Vertex(p[7]);
            VertexIndex(0); VertexIndex(1);
            VertexIndex(2); VertexIndex(3);
            VertexIndex(0);
            End();

            DrawLine(p[0], p[4]);
            DrawLine(p[1], p[5]);
            DrawLine(p[2], p[6]);
            DrawLine(p[3], p[7]);
        }
                

        public override void DrawString(string text, Font font, Color color, float size,
                                        Vector64 start, Vector64 direct, Vector64 up,
                                        TextHorizontalAlignment horAlignment = TextHorizontalAlignment.Left,
                                        TextVerticalAlignment verAlignment = TextVerticalAlignment.Center)
        {
            if (program < 1) return;

            PushMatrix();

            byte r = (byte)(color.R);
            byte g = (byte)(color.G);
            byte b = (byte)(color.B);
            Color backcolor = Color.FromArgb(0, r, g, b);
            BitmapString bm = new BitmapString(text, font, color, backcolor);

            EnableTexture(true);
            BindTexture(bm.Draw());

            double width = text.Length * size * 0.1;
            double height = width * textureBitmap.Height / (double)textureBitmap.Width;
            //      |
            //      p1-----p2
            //      |      |
            //      p0-----p3-->
            Vector64 p0, p1, p2, p3;
            if (verAlignment == TextVerticalAlignment.Top)
            {
                //      |
                //      p1-----p2-->
                //      |      |
                //      p0-----p3
                p1 = start;
                p2 = p1 + width * direct;
                p0 = p1 - up * height;
                p3 = p2 - up * height;
            }
            else if (verAlignment == TextVerticalAlignment.Bottom)
            {
                //      |
                //      p1-----p2
                //      |      |
                //      p0-----p3-->
                p0 = start;
                p3 = p0 + width * direct;
                p1 = p0 + up * height;
                p2 = p3 + up * height;
            }
            else //if (verAlignment == TextVerticalAlignment.Center)
            {
                //      |
                //      p1-----p2
                //      |      |-->
                //      p0-----p3
                p0 = start - 0.5 * height * up;
                p3 = p0 + width * direct;
                p1 = p0 + up * height;
                p2 = p3 + up * height;
            }

            if (horAlignment == TextHorizontalAlignment.Center)
            {
                p0 = p0 - width * 0.5 * direct;
                p1 = p1 - width * 0.5 * direct;
                p2 = p2 - width * 0.5 * direct;
                p3 = p3 - width * 0.5 * direct;
            }
            else if (horAlignment == TextHorizontalAlignment.Right)
            {
                p0 = p0 - width * 1.0 * direct;
                p1 = p1 - width * 1.0 * direct;
                p2 = p2 - width * 1.0 * direct;
                p3 = p3 - width * 1.0 * direct;
            }
            else //if (horAlignment == TextHorizontalAlignment.Left)
            {

            }
            Vertex3D v0 = new Vertex3D((float)p0.x, (float)p0.y, (float)p0.z);
            Vertex3D v1 = new Vertex3D((float)p1.x, (float)p1.y, (float)p1.z);
            Vertex3D v2 = new Vertex3D((float)p2.x, (float)p2.y, (float)p2.z);
            Vertex3D v3 = new Vertex3D((float)p3.x, (float)p3.y, (float)p3.z);

            v0.SetTexcoord(0, 1);
            v1.SetTexcoord(0, 0);
            v2.SetTexcoord(1, 0);
            v3.SetTexcoord(1, 1);

            vec4 textcolor = ConvertColor(backcolor);
            textcolor.w = -1;   //字体所在矩形
            v0.SetColor(textcolor);
            v1.SetColor(textcolor);
            v2.SetColor(textcolor);
            v3.SetColor(textcolor);

            SetPolygonMode(gDrawMode.Fill);

            BeginTriangles();
            AddPoint(v0);
            AddPoint(v1);
            AddPoint(v2);
            AddPoint(v3);
            AddPointIndex(0);
            AddPointIndex(3);
            AddPointIndex(2);
            AddPointIndex(1);
            AddPointIndex(0);
            AddPointIndex(2);
            EndTriangles();
            EnableTexture(false);
            PopMatrix();
        }
        //      |
        //      p1-----p2
        //      |      |
        //      p0-----p3-->
        /// <summary>
        /// 绘制文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color">颜色</param>
        /// <param name="p1">起始位置</param>
        /// <param name="p2">结束位置</param>
        /// <param name="direct">字体顶端单位方向向量</param>        
        public override void DrawString(string text, Font font, Color color,
                                        Vector64 start, Vector64 end, Vector64 direct,
                                        Color transparent,
                                        TextHorizontalAlignment horAlignment = TextHorizontalAlignment.Left,
                                        TextVerticalAlignment verAlignment = TextVerticalAlignment.Center)
        {
            if (program < 1) return;
            PushMatrix();

            BitmapString bm = new BitmapString(text, font, color, transparent);
            EnableTexture(true);
            BindTexture(bm.Draw());

            double width = start.Distance(end);
            double height = width * textureBitmap.Height / (double)textureBitmap.Width;
            Vector64 p0, p1, p2, p3;
            if (verAlignment == TextVerticalAlignment.Top)
            {
                p0 = start - height * direct;
                p3 = end - height * direct;
                p1 = start;
                p2 = end;
            }
            else if (verAlignment == TextVerticalAlignment.Bottom)
            {
                p0 = start;
                p3 = end;
                p1 = p0 + height * direct;
                p2 = p3 + height * direct;
            }
            else //if (verAlignment == TextAlignment.Center)
            {
                p0 = start - 0.5 * height * direct;
                p3 = end - 0.5 * height * direct;
                p1 = start + 0.5 * height * direct;
                p2 = end + 0.5 * height * direct;
            }
            Vertex3D v0 = new Vertex3D((float)p0.x, (float)p0.y, (float)p0.z);
            Vertex3D v1 = new Vertex3D((float)p1.x, (float)p1.y, (float)p1.z);
            Vertex3D v2 = new Vertex3D((float)p2.x, (float)p2.y, (float)p2.z);
            Vertex3D v3 = new Vertex3D((float)p3.x, (float)p3.y, (float)p3.z);

            v0.SetTexcoord(0, 1);
            v1.SetTexcoord(0, 0);
            v2.SetTexcoord(1, 0);
            v3.SetTexcoord(1, 1);

            vec4 textcolor = ConvertColor(transparent);
            textcolor.w = -1;//字体所在矩形

            v0.SetColor(textcolor);
            v1.SetColor(textcolor);
            v2.SetColor(textcolor);
            v3.SetColor(textcolor);

            SetPolygonMode( gDrawMode.Fill );

            BeginTriangles();
            AddPoint(v0);
            AddPoint(v1);
            AddPoint(v2);
            AddPoint(v3);
            AddPointIndex(0);
            AddPointIndex(3);
            AddPointIndex(2);
            AddPointIndex(1);
            AddPointIndex(0);
            AddPointIndex(2);
            EndTriangles();
            EnableTexture(false);

            PopMatrix();
        }

        private void DrawTriangle(MyGLModel model)
        {
            if (model.vertices != null)
            {
                MemoryLock verticBuffer = new MemoryLock(model.vertices);
                uint mPositionHandle = (uint)Gl.GetAttribLocation(program, "inPosition");
                Gl.EnableVertexAttribArray(mPositionHandle);
                Gl.VertexAttribPointer(mPositionHandle, 3, VertexAttribType.Float, false, 0, verticBuffer.Address);
            }
            if (model.colors != null)
            {                
                MemoryLock colorBuffer = new MemoryLock(model.colors);
                uint mColorHandle = (uint)Gl.GetAttribLocation(program, "inColor");
                Gl.EnableVertexAttribArray(mColorHandle);
                Gl.VertexAttribPointer(mColorHandle, 4, VertexAttribType.Float, false, 0, colorBuffer.Address);
            }
            if (model.texcoords != null)
            {
                MemoryLock texBuffer = new MemoryLock(model.texcoords);
                uint mcoord = (uint)Gl.GetAttribLocation(program, "inTexCoord");
                Gl.EnableVertexAttribArray(mcoord);
                Gl.VertexAttribPointer(mcoord, 2, VertexAttribType.Float, false, 0, texBuffer.Address);
            }
            if (model.normals != null)
            {
                MemoryLock normalBuffer = new MemoryLock(model.normals);
                uint iNormal = (uint)Gl.GetAttribLocation(program, "inNormal");
                Gl.EnableVertexAttribArray(iNormal);
                Gl.VertexAttribPointer(iNormal, 3, VertexAttribType.Float, false, 0, normalBuffer.Address);
            }
            if (model.indices != null)
            {
                if (model.polygonMode == gDrawMode.Fill) 
                    Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                else if (model.polygonMode == gDrawMode.Wireframe)
                    Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                else if (model.polygonMode == gDrawMode.Points)
                    Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);

                MemoryLock indexBuffer = new MemoryLock(model.indices);
                if (model.primitive == DrawingPrimitive.TRIANGLE_LIST)
                    Gl.DrawElements(PrimitiveType.Triangles, model.indices.Length, DrawElementsType.UnsignedInt, indexBuffer.Address);
                else if (model.primitive == DrawingPrimitive.TRIANGLE_STRIP)
                    Gl.DrawElements(PrimitiveType.TriangleStrip, model.indices.Length, DrawElementsType.UnsignedInt, indexBuffer.Address);
                else if (model.primitive == DrawingPrimitive.TRIANGLE_FAN)
                    Gl.DrawElements(PrimitiveType.TriangleFan, model.indices.Length, DrawElementsType.UnsignedInt, indexBuffer.Address);
                else if (model.primitive == DrawingPrimitive.POINT_LIST)
                    Gl.DrawElements(PrimitiveType.Points, model.indices.Length, DrawElementsType.UnsignedInt, indexBuffer.Address);
            }            
        }
        private void DrawModel(MyGLModel model)
        {
            if ( !model.Visible ) return;

            int texId = 0;
            if( model.textureBytes != null )
            {
                Gl.Enable(EnableCap.Texture2d);
                texId = BindTexture(model.textureBytes,model.texWidth,model.texHeight,model.textureMode);
            }
            else
            {
                Gl.Disable(EnableCap.Texture2d);
            }

            if (model.primitive == DrawingPrimitive.LINE_LIST ||
                model.primitive == DrawingPrimitive.LINE_STRIP)
            {
                DrawLine(model);
            }
            else if (model.primitive == DrawingPrimitive.TRIANGLE_LIST ||
                model.primitive == DrawingPrimitive.TRIANGLE_STRIP ||
                model.primitive == DrawingPrimitive.TRIANGLE_FAN)
            {
                DrawTriangle(model);
            }
            DeleteTexture(texId);
        }
        private void DrawLine(MyGLModel model)
        {
            if (model.primitive == DrawingPrimitive.LINE_LIST ||
                model.primitive == DrawingPrimitive.LINE_STRIP)
            {
                if (model.vertices != null)
                {
                    MemoryLock verticBuffer = new MemoryLock(model.vertices);
                    uint mPositionHandle = (uint)Gl.GetAttribLocation(program, "inPosition");
                    Gl.EnableVertexAttribArray(mPositionHandle);
                    Gl.VertexAttribPointer(mPositionHandle, 3, VertexAttribType.Float, false, 0, verticBuffer.Address);
                }
                if (model.colors != null)
                {
                    MemoryLock colorBuffer = new MemoryLock(model.colors);
                    uint mColorHandle = (uint)Gl.GetAttribLocation(program, "inColor");
                    Gl.EnableVertexAttribArray(mColorHandle);
                    Gl.VertexAttribPointer(mColorHandle, 4, VertexAttribType.Float, false, 0, colorBuffer.Address);
                }
                if (model.texcoords != null)
                {
                    MemoryLock texBuffer = new MemoryLock(model.texcoords);
                    uint mcoord = (uint)Gl.GetAttribLocation(program, "inTexCoord");
                    Gl.EnableVertexAttribArray(mcoord);
                    Gl.VertexAttribPointer(mcoord, 2, VertexAttribType.Float, false, 0, texBuffer.Address);
                }
                if (model.indices != null)
                {/*
                    //启用反走样
                    if (model.lineStyle != gLineStyle.Solid)
                    {
                        Gl.LineStipple(1, 0x3f07);
                        Gl.Enable(EnableCap.LineStipple);
                        Gl.Enable(EnableCap.Blend);
                        Gl.Enable(EnableCap.LineSmooth);
                        Gl.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);  // Antialias the lines
                        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);                        
                    }
                    else Gl.Disable(EnableCap.LineStipple);
                    */
                    Gl.LineWidth(model.lineWidth);
                    MemoryLock indexBuffer = new MemoryLock(model.indices);
                    if (model.primitive == DrawingPrimitive.LINE_LIST)
                        Gl.DrawElements(PrimitiveType.Lines, model.indices.Length, DrawElementsType.UnsignedInt, indexBuffer.Address);
                    else if (model.primitive == DrawingPrimitive.LINE_STRIP)
                        Gl.DrawElements(PrimitiveType.LineStrip, model.indices.Length, DrawElementsType.UnsignedInt, indexBuffer.Address);
                }
            }
        }
        /// <summary>
        /// 重设所有模型得统一参数，矩阵，光照，视点，材质等
        /// </summary>
        public override void ResetModelUniformMatrixs(List<CModel> models)
        {
            lock (modelLocker)
            {
                foreach (var obj in objectModels)
                {
                    MyGLModel model = obj.Value as MyGLModel;
                    
                    //update model view eyepos,modelview
                    model.m_modelMatrix.proj = m_modelMatrix.proj;
                    model.m_modelMatrix.view = m_modelMatrix.view;

                    // this model include global rotate
                    if (model.m_enableRotate) model.m_modelMatrix.model = m_modelMatrix.model;

                    model.m_modelMatrix.eyepos = new vec4(m_eye, 1.0f);
                    model.m_modelMatrix.lights = m_modelMatrix.lights;
                    model.m_modelMatrix.material = m_modelMatrix.material;
                }
            }
        }
        void DrawModels()
        {
            try
            {
                List<CModel> models = UpdateDrawOrder();//按透明排序        
                
                //ResetModelUniformMatrixs(models);
                for (int i = 0; i < models.Count; i++)
                {                   
                    DrawModel(models[i] as MyGLModel);
                }
            }
            catch (Exception e)
            {
                errMessage = e.Message;
            }
        }
        //双面光照和透明失败？？？
        public override void UpdateDraw()
        {            
            Gl.Initialize(); 

            Gl.UseProgram(program);

            Gl.Viewport(0, 0, windowWidth, windowHeight);
            Gl.ClearColor(clearColor.x, clearColor.y, clearColor.z, clearColor.w);

            //Gl.ClearDepth(1.0f);
            //Gl.Clear( ClearBufferMask.ColorBufferBit |  ClearBufferMask.DepthBufferBit);
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);
            Gl.ShadeModel( ShadingModel.Smooth );
     
            //开启gamma校正
            Gl.Enable(EnableCap.FramebufferSrgb);
            //Gl.DepthFunc( DepthFunction.Lequal);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Hint( HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            //Gl.Enable(EnableCap.CullFace);
            //Gl.CullFace(CullFaceMode.Back); 
            
            //Gl.LightModel(LightModelParameter.LightModelTwoSide, 1.0f);
            //Gl.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse |MaterialParameter.Specular, 1.0f);
          
            // Gl.Enable(EnableCap.Dither);
            //Gl.Enable(EnableCap.AlphaTest);
            Gl.Enable(EnableCap.Blend);
            //设置混合函数
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            // Gl.DepthMask(true);            

            //Gl.DepthFunc( DepthFunction.Lequal);
            //Gl.Enable( EnableCap.DepthTest);
            // Gl.EnableClientState(EnableCap.DepthTest);
            // Gl.EnableClientState(EnableCap.Dither);
            //Gl.EnableClientState(EnableCap.AlphaTest);
            //Gl.EnableClientState(EnableCap.Blend);

            Gl.PushMatrix();           

            SetUniformParameters();
            
            DrawModels();
            
            Gl.PopMatrix();

            Gl.Flush();
        }
    }
}
