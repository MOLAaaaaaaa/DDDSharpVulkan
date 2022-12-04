using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharpGLES;
using GlmNet;
using DataCollection;
namespace Graphics3D
{
    
    public class MySharpGLES : CGraphic3D
    {
        private EGLDisplay _display;
        private int program;

        //private MyGLModel curModel = null;
        //public List<MyGLModel> models = new List<MyGLModel>();
        public MySharpGLES()
        {
            engine = gEngine.opengles;
        }
        public override GraphicDeviceInfo GetChoosedGraphicsDevice()
        {
            GraphicDeviceInfo[] devices = GetGraphicsDeviceInfo();
            return devices[0];
        }
        public override void onWindowResized(int width, int height)
        {
            if (width == 0 || height == 0) return;
            windowWidth = width;
            windowHeight = height;
            UpdateDraw();
        }
        //实现窗口创建，三维点数据存储，基本图元绘制(点，线，三角形等）
        //Shader载入和使用，实现旋转矩阵，平移缩放矩阵的计算
        public override bool Initialize(IntPtr window, string title = "OpenGLES", int width = 800, int height = 600)
        {
            windowWidth = width;
            windowHeight = height;            

            _display = new EGLDisplay(window);

            if (LoadProgram("GLES-vert.c", "GLES-frag.c") < 0)
            {
                return false;
            }

            initialized = true;
            LoadIdentity();
            return initialized;
        }
        public override bool DestroyWindow()
        {
            _display.Dispose();
            return true;
        }
        public override string GetGraphicsInfoString()
        {
            return "not supported";
        }
        public override string GetGraphicName()
        {
            return "not supported";
        }
        public int LoadProgram(string vertFile = @"shader\GLES-vert.c", string fragFile = @"shader\GLES-frag.c")
        {
            string vertSource = toShaderString(LoadShaderSource(vertFile));
            string fragSource = toShaderString(LoadShaderSource(fragFile));

            program = GLES20.CreateProgram();

            if (program == 0)
            {
                errMessage = "Could not create new program.";
                return -1;
            }

            int vertexShaderId = LoadShader(GLES20.GL_VERTEX_SHADER, vertSource);
            int fragmentShaderId = LoadShader(GLES20.GL_FRAGMENT_SHADER, fragSource);

            if (vertexShaderId < 0 || fragmentShaderId < 0)
            {
                GLES20.DeleteProgram(program);
                return -1;
            }

            GLES20.AttachShader(program, vertexShaderId);
            GLES20.AttachShader(program, fragmentShaderId);
            GLES20.LinkProgram(program);

            GLES20.DeleteShader(vertexShaderId);
            GLES20.DeleteShader(fragmentShaderId);

            int status;

            GLES20.GetProgramiv(program, GLES20.GL_LINK_STATUS, out status);

            if (status == 0)
            {
                string log = GLES20.GetProgramInfoLog(program);

                GLES20.DeleteProgram(program);
                errMessage = "Error linking program: " + log;
                return -1;
            }

            GLES20.ValidateProgram(program);

            GLES20.GetProgramiv(program, GLES20.GL_VALIDATE_STATUS, out status);

            if (status == 0)
            {
                string log = GLES20.GetProgramInfoLog(program);

                errMessage = "Results of validating program: " + status + ". Log: " + log;

            }
            if (CheckError("Program load") < 0) return -1;
            return program;
        }

        private int LoadShader(int type, string source)
        {
            int id = GLES20.CreateShader(type);

            if (id == 0)
            {
                errMessage = "Could not create shader.";
                return -1;
            }

            GLES20.ShaderSource(id, source);

            GLES20.CompileShader(id);

            int status;

            GLES20.GetShaderiv(id, GLES20.GL_COMPILE_STATUS, out status);

            if (status == 0)
            {
                string log = GLES20.GetShaderInfoLog(id);

                GLES20.DeleteShader(id);

                errMessage = "Error compiling shader: " + log;
                return -1;
            }

            return id;
        }

        private int CheckError(string operation)
        {
            int error;
            int lastError = GLES20.GL_NO_ERROR;

            while ((error = GLES20.GetError()) != GLES20.GL_NO_ERROR)
            {
                lastError = error;
            }
            if (lastError != GLES20.GL_NO_ERROR)
            {
                errMessage = operation + " produced error code " + lastError + ".";
                return -1;
            }
            return 1;
        }
        public override void AddModel(CModel obj)
        {
            MyGLModel model = obj as MyGLModel;
            if (bEnableTexture)
            { 
                if( textureBitmap != null )
                {
                    model.textureMode = textureMode;
                    model.texWidth = textureBitmap.Width;
                    model.texHeight = textureBitmap.Height;
                    model.texBitmap = textureBitmap;
                }
            }
            model.polygonMode = curDrawParameter.polygonMode;
            model.lineStyle = curDrawParameter.lineStyle;
            model.lineWidth = curDrawParameter.lineWidth;

            base.AddModel(model);
        }
        
        public override void Begin(DrawingPrimitive _primitive = DrawingPrimitive.TRIANGLE_LIST)
        {
            lock (modelLocker)
            {
                currentModel = new MyGLModel(_primitive);
            }
            base.ClearDrawBuffer();
        }

        public override bool End(bool _createVerticesBuffer = true, bool _createIndexBuffer = true, bool _createUniformBuffer = true)
        {
            MyGLModel curModel = currentModel as MyGLModel;
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

            base.ClearDrawBuffer();

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
        public override void DrawString(string text, Font font, Color color,
                                         Vector64 start, Vector64 end, Vector64 direct,
                                         Color transparent,
                                         TextHorizontalAlignment horAlignment = TextHorizontalAlignment.Left,
                                        TextVerticalAlignment verAlignment = TextVerticalAlignment.Center)
        {
            if (program < 1) return;

            PushMatrix();

            BitmapString bm = new BitmapString(text, font, color,transparent);
            EnableTexture(true);
            BindTexture(bm.Draw());

            double width = start.Distance(end);
            double height = width * textureBitmap.Height / textureBitmap.Width;
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
            textcolor.w = -1;
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
        private IntPtr toIntPtr(float[]_array)
        {
            unsafe
            {
                fixed (float* pf = _array)
                {
                    return new IntPtr(pf);
                }
            }
        }
        private IntPtr toIntPtr(int[] _array)
        {
            unsafe
            {
                fixed (int* pf = _array)
                {
                    return new IntPtr(pf);
                }
            }
        }
        private void DrawTriangle(MyGLModel model)
        {
            if (model.primitive == DrawingPrimitive.TRIANGLE_LIST ||
                model.primitive == DrawingPrimitive.TRIANGLE_STRIP ||
                model.primitive == DrawingPrimitive.TRIANGLE_FAN)
            {
                if (curDrawParameter.polygonMode == gDrawMode.Fill)
                {
                    if (model.vertices != null)//3
                    {
                        int mPositionHandle = GLES20.GetAttribLocation(program, "inPosition");
                        GLES20.EnableVertexAttribArray(mPositionHandle);                        
                        GLES20.VertexAttribPointer(mPositionHandle, 3, GLES20.GL_FLOAT, false, 0, toIntPtr(model.vertices));
                    }
                    if (model.colors != null)//0
                    {
                        int mColorHandle = GLES20.GetAttribLocation(program, "inColor");
                        GLES20.EnableVertexAttribArray(mColorHandle);
                        GLES20.VertexAttribPointer(mColorHandle, 4, GLES20.GL_FLOAT, false, 0,toIntPtr(model.colors));
                    }

                    //if (bEnableTexture && model.texcoords != null)
                    if (model.texcoords != null)//2
                    {
                        int mcoord = GLES20.GetAttribLocation(program, "inTexCoord");
                        GLES20.EnableVertexAttribArray(mcoord);
                        GLES20.VertexAttribPointer(mcoord, 2, GLES20.GL_FLOAT, false, 0, toIntPtr(model.texcoords) );
                    }
                    if (model.normals != null)//1
                    {
                        int minNormal = GLES20.GetAttribLocation(program, "inNormal");
                        GLES20.EnableVertexAttribArray(minNormal);
                        GLES20.VertexAttribPointer(minNormal, 3, GLES20.GL_FLOAT, false, 0, toIntPtr(model.normals) );
                    }
                    if (model.indices != null)
                    {
                        //GLES not supported FrameWire Mode
                        if (model.primitive == DrawingPrimitive.TRIANGLE_LIST)
                            GLES20.DrawElements(GLES20.GL_TRIANGLES, model.indices.Length, GLES20.GL_UNSIGNED_INT, toIntPtr(model.indices));
                        else if (model.primitive == DrawingPrimitive.TRIANGLE_STRIP)
                            GLES20.DrawElements(GLES20.GL_TRIANGLE_STRIP, model.indices.Length, GLES20.GL_UNSIGNED_INT, toIntPtr(model.indices));
                        else if (model.primitive == DrawingPrimitive.TRIANGLE_FAN)
                            GLES20.DrawElements(GLES20.GL_TRIANGLE_FAN, model.indices.Length, GLES20.GL_UNSIGNED_INT, toIntPtr(model.indices));
                    }
                }               

            }
        }
        private void DrawModel(MyGLModel model)
        {
            int texId = 0;
            if (model.texBitmap != null)
            {
                GLES20.Enable(GLES20.GL_TEXTURE_2D);
                texId = BindTexture(model.texBitmap,model.textureMode);
            }
            else
            {
                GLES20.Disable(GLES20.GL_TEXTURE_2D);
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
                    int mPositionHandle = GLES20.GetAttribLocation(program, "inPosition");
                    GLES20.EnableVertexAttribArray(mPositionHandle);
                    GLES20.VertexAttribPointer(mPositionHandle, 3, GLES20.GL_FLOAT, false, 0, toIntPtr(model.vertices) );
                }
                if (model.colors != null)
                {
                    int mColorHandle = GLES20.GetAttribLocation(program, "inColor");
                    GLES20.EnableVertexAttribArray(mColorHandle);
                    GLES20.VertexAttribPointer(mColorHandle, 4, GLES20.GL_FLOAT, false, 0, toIntPtr(model.colors) );
                }
                if (model.texcoords != null)
                {
                    int mcoord = GLES20.GetAttribLocation(program, "inTexCoord");
                    GLES20.EnableVertexAttribArray(mcoord);
                    GLES20.VertexAttribPointer(mcoord, 2, GLES20.GL_FLOAT, false, 0, toIntPtr(model.texcoords) );
                }
                if (model.indices != null)
                {
                    GLES20.LineWidth(model.lineWidth);
                    if (model.primitive == DrawingPrimitive.LINE_LIST)
                        GLES20.DrawElements(GLES20.GL_LINES, model.indices.Length, GLES20.GL_UNSIGNED_INT, toIntPtr(model.indices));
                    else if (model.primitive == DrawingPrimitive.LINE_STRIP)
                        GLES20.DrawElements(GLES20.GL_LINE_STRIP, model.indices.Length, GLES20.GL_UNSIGNED_INT, toIntPtr(model.indices));
                }
            }
        }
        public override int BindTexture(Bitmap bmp, DataCollection.TextureMagFilter mode = DataCollection.TextureMagFilter.GL_LINEAR)
        {
            if (bmp == null) return 0;
            if (program <= 0) return 0;

            CTexture texture = new CTexture();
            texture.forcePowerOfTwo = false;
            texture.maxTextureSize = GetMax2DTextureImageSize();
            textureBitmap = texture.CreateCompitableBitmap(bmp);            

            int[] textureHandle = new int[1];
            GLES20.GenTextures(1, textureHandle);
            int TextureID = textureHandle[0];

            GLES20.BindTexture(GLES20.GL_TEXTURE_2D, TextureID);
            GLES20.TexParameteri(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
            GLES20.TexParameteri(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
            GLES20.TexParameteri(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
            GLES20.TexParameteri(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
            GLUtils.TexImage2D(GLES20.GL_TEXTURE_2D, 0, textureBitmap, 0);       
            
            return TextureID;
        }
        private void DeleteTexture(int id )
        {
            if (id < 1) return;
            int[] texid = new int[] { id };
            GLES20.DeleteTextures(1, texid);
        }       
        void SetUniformParameters()
        {
            //修改模型矩阵============================================================================================================================================================
            int proj = GLES20.GetUniformLocation(program, "project.proj");
            GLES20.UniformMatrix4fv(proj, 1, false, m_modelMatrix.proj.to_array());            
            int model = GLES20.GetUniformLocation(program, "project.model");
            GLES20.UniformMatrix4fv(model, 1, false, m_modelMatrix.model.to_array());
            int view = GLES20.GetUniformLocation(program, "project.view");
            GLES20.UniformMatrix4fv(view, 1, false, m_modelMatrix.view.to_array());
            int normal = GLES20.GetUniformLocation(program, "project.normal");
            GLES20.UniformMatrix4fv(normal, 1, false, m_modelMatrix.normal.to_array());
            int eyepos = GLES20.GetUniformLocation(program, "project.eyepos");
            GLES20.Uniform4f(eyepos, m_modelMatrix.eyepos[0], m_modelMatrix.eyepos[1], m_modelMatrix.eyepos[2], m_modelMatrix.eyepos[3]);
            //=================================================================================================================================

            //light==========================================================================
            //创建当前开启的灯光数组，已开启的放在数组前
            List<LightStruct> lights = new List<LightStruct>();
            int lightNum = m_modelMatrix.lightsNum;
            for (int i = 0; i < m_modelMatrix.lights.Length; i++)
            {
                if (m_modelMatrix.lights[i].Enable)
                {
                    lights.Add(m_modelMatrix.lights[i]);
                }
            }
            
            int lightnumpos = GLES20.GetUniformLocation(program, "lightNum");
            GLES20.Uniform1i(lightnumpos, lightNum);

            for (int i = 0; i < lightNum; i++)
            {
                int type = GLES20.GetUniformLocation(program, "light" + i + ".type");
                GLES20.Uniform4f(type, lights[i].type.x, lights[i].type.y, lights[i].type.z, lights[i].type.w);
                int pos = GLES20.GetUniformLocation(program, "light" + i + ".pos");
                GLES20.Uniform4f(pos, lights[i].pos.x, lights[i].pos.y, lights[i].pos.z, lights[i].pos.w);
                int ambient = GLES20.GetUniformLocation(program, "light" + i + ".ambient");
                GLES20.Uniform4f(ambient, lights[i].ambient.x, lights[i].ambient.y, lights[i].ambient.z, lights[i].ambient.w);
                int diffuse = GLES20.GetUniformLocation(program, "light" + i + ".diffuse");
                GLES20.Uniform4f(diffuse, lights[i].diffuse.x, lights[i].diffuse.y, lights[i].diffuse.z, lights[i].diffuse.w);
                int specular = GLES20.GetUniformLocation(program, "light" + i + ".specular");
                GLES20.Uniform4f(specular, lights[i].specular.x, lights[i].specular.y, lights[i].specular.z, lights[i].specular.w);
            }
        }

        void DrawModels()
        {
            try
            {
                lock (modelLocker)
                {
                    var obj = objectModels.GetEnumerator();
                    while (obj.MoveNext())
                    {
                        MyGLModel model = obj.Current.Value as MyGLModel;
                        DrawModel(model);
                    } 
                }   
            }
            catch (Exception e)
            {
                errMessage = e.Message;
            }
        }

        public override void UpdateDraw()
        {
            GLES20.Viewport(0, 0, windowWidth, windowHeight);
            GLES20.ClearColor(clearColor.x, clearColor.y, clearColor.z, clearColor.w);

            GLES20.ClearDepthf(1.0f);
            GLES20.Clear(GLES20.GL_COLOR_BUFFER_BIT | GLES20.GL_DEPTH_BUFFER_BIT);
            GLES20.Enable(GLES20.GL_DEPTH_TEST);
            GLES20.Enable(GLES20.GL_BLEND);
            GLES20.Enable(GLES20.GL_DEPTH_BUFFER_BIT);
            GLES20.BlendFunc(GLES20.GL_SRC_ALPHA, GLES20.GL_ONE_MINUS_SRC_ALPHA);
            GLES20.DepthMask(true);

            GLES20.UseProgram(program);
            
            SetUniformParameters();

            DrawModels();

            _display.SwapBuffers();
        }
    }

}
