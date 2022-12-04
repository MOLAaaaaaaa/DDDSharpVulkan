using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using DataCollection;
using glfw3;
namespace Graphics3D
{
    /// <summary>
	/// Represents an array of vertex
	/// 
	/// When possible this array is represented as a VRAM buffer
	/// Otherwise it's a pointer to host memory
	/// 
	/// In the case of OpenGL that's called a "Vertex Array"
	/// </summary>
	abstract public class GPUArray<T>
    {
        private int _length = 0;

        public GPUArray(int length)
        {
            _length = length;
        }

        public int Length
        {
            get { return _length; }
        }

        public abstract T[] Data
        {
            get;
        }
    }
    /// <summary>
	/// A VRAM array stored on host 
	/// Typically used when VBO are not supported
	/// </summary>
	public class GPUArrayHost<T> : GPUArray<T>
    {
        private T[] _data;

        public GPUArrayHost(T[] data)
            : base(data.Length)
        {
            _data = data;
        }

        public override T[] Data
        {
            get { return _data; }
        }
    }
   
    class COpenGL: CGraphic3D
    {
        public string errorString = "";        
#pragma warning disable CS0414 // 字段“COpenGL.m_DrawingMode”已被赋值，但从未使用过它的值
        private gDrawMode m_DrawingMode = gDrawMode.Fill;
#pragma warning restore CS0414 // 字段“COpenGL.m_DrawingMode”已被赋值，但从未使用过它的值
        private Font m_Font = new Font(FontFamily.GenericSerif, 14);
#pragma warning disable CS0414 // 字段“COpenGL.m_FillPolygonBackFace”已被赋值，但从未使用过它的值
        private bool m_FillPolygonBackFace = false;
#pragma warning restore CS0414 // 字段“COpenGL.m_FillPolygonBackFace”已被赋值，但从未使用过它的值
        public bool m_ConvertCoordinates = true;   //是否转换坐标系统
        public Vector64 m_EyePos = new Vector64(0, 100,0);
        public Vector64 m_ViewPos = new Vector64(0, 0, 0);
        public Vector32 m_Rotate = new Vector32(0, 0, 0);
        public Vector32 m_Offset = new Vector32(0, 0, 0);
        public Vector32 m_Scale = new Vector32(1,1,1);
        public int m_MaxMoveStep = 20;
        private const int FontDisplayListBase = 1001;
#pragma warning disable CS0169 // 从不使用字段“COpenGL._clipPlanesEnabled”
        private bool _clipPlanesEnabled;
#pragma warning restore CS0169 // 从不使用字段“COpenGL._clipPlanesEnabled”
        //光照
        public List<LightStruct> m_Light = new List<LightStruct>();
        public MaterialStruct m_Material = new MaterialStruct();
        // 模型体的大小范围
        public CubeModel64 m_Model = new CubeModel64(-1, -1, -1,1,1,1);
    
        //initial window
#pragma warning disable CS0649 // 从未对字段“COpenGL.glfwWindow”赋值，字段将一直保持其默认值 null
        public GLFWwindow glfwWindow;       // = glfwCreateWindow()
#pragma warning restore CS0649 // 从未对字段“COpenGL.glfwWindow”赋值，字段将一直保持其默认值 null
        private IntPtr windowHandle = IntPtr.Zero;  // = WinForm handle
        //private EGLDisplay _display = null;

        public COpenGL()
        {            
        }
        public override bool Initialize(IntPtr window, string title = "vulkan", int width = 800, int height = 600)
        {
            windowHandle = window;
            windowWidth = width;
            windowHeight = height;
            windowTitle = title;
            try
            {
                //_display = new EGLDisplay(windowHandle);
                return true;
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch(Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                CheckError("Create OpenGL Window");
                return false;
            }
        }
        public override bool DestroyWindow()
        {
           // if (_display == null) return false;
           // _display.Dispose();
            return true;
        }
        public void SetWindowSize(int width,int height)
        {
            windowWidth = width;
            windowHeight = height;
        }
        private static void CheckError(string operation)
        {
           
        }
        
        private void activateDrawingMode( gDrawMode drawingMode)
        {
           
        }
        
        public float[] ToGraphicsCoordinates(float x, float y, float z)
        {
            if( m_ConvertCoordinates )return new[] { x, z, -y };
            else return new[] { x, y, z };
        }public double[] ToGraphicsCoordinates(double x, double y, double z)
        {
            if (m_ConvertCoordinates)
                return new[] { x, z, -y };
            else return new[] { x, y, z };
        }
        public Vector32 ToGraphicsCoordinates(Vector32 p)
        {
            if (m_ConvertCoordinates)
                return new Vector32(p.X, p.Z, -p.Y);
            else return p;
        }
        public Vector64 ToGraphicsCoordinates(Vector64 p)
        {
            if (m_ConvertCoordinates)
                return new Vector64(p.X, p.Z, -p.Y);
            else return p;
        }

        public Vector32 FromGraphicsCoordinates(float x, float y, float z)
        {
            if (m_ConvertCoordinates)
                return new Vector32(x, -z, y);
            else return new Vector32(x, y, z);
        }
        public Vector64 FromGraphicsCoordinates(double x, double y, double z)
        {
            if (m_ConvertCoordinates)
                return new Vector64(x, -z, y);
            else return new Vector64(x,y,z);
        }
        public override void PushMatrix()
        {
            PushMatrix();
            CheckError("glPushMatrix");
        }

        public override void PopMatrix()
        {
            PopMatrix();
            CheckError("glPopMatrix");
        }
        //OPENGL绝对坐标
        public void SetRotate(float xs, float ys, float zs)
        {   
            m_Rotate.X += xs;
            m_Rotate.Y += ys;

            if(m_ConvertCoordinates)
                m_Rotate.Z -= zs;
            else m_Rotate.Z += zs;
        }
        /*
        public override void Rotate(float angle, float x, float y, float z)
        {
            GLES20.(angle, x, y, z);
            CheckError("glRotatef");
        }
        public void Rotate()
        {
            //Vector32 p = ToGraphicsCoordinates(m_Rotate);
            Rotate(m_Rotate.X, 0, 0, 1);
            Rotate(m_Rotate.Y, 0, 1, 0);
            Rotate(m_Rotate.Z, 1, 0, 0);
        }        
        public void Scale(Single scaleFactor)
        {
            GLES20.Scalef(scaleFactor, scaleFactor, scaleFactor);
            CheckError("glScalef");
        }
        public void Scale3f(Vector32 p)
        {
            var p1 = ToGraphicsCoordinates(p);
            GLES20.Scalef(p1.X, p1.Y, p1.Z);
            CheckError("glScalef");
        }
        
        public void Scale3f(Vector64 p)
        {
            var p1 = ToGraphicsCoordinates(p);
            GLES20.Scaled(p1.X, p1.Y, p1.Z);
            CheckError("glScalef");
        }
        public void Scale3d(double xscale, double yscale, double zscale)
        {
            var p = new Vector64(xscale, yscale, zscale);
            Scale3f(p);
        }
        public void Scale3f(float xscale, float yscale, float zscale)
        {
            var p = new Vector32(xscale, yscale, zscale);
            Scale3f(p);
        }
        public void Translate3f(double x, double y, double z)
        {
            Vector64 p = new Vector64(x, y, z);
            Translate(p);
        }
        public void Translate3f(float x, float y, float z)
        {
            Vector32 p = new Vector32(x, y, z);
            Translate(p);
        }
        public void Translate(Vector32 point)
        {
            var pointGL = ToGraphicsCoordinates(point);
            GLES20.Translatef(pointGLES20.X, pointGLES20.Y, pointGLES20.Z);
            CheckError("glTranslatef");
        }
        public void Translate(Vector64 point)
        {
            var pointGL = ToGraphicsCoordinates(point);
            GLES20.Translated(pointGLES20.X, pointGLES20.Y, pointGLES20.Z);
            CheckError("glTranslated");
        }
        */
        /***************************************************************          
         *  Multipl Texture Section
         *  added by jian Jun 2017
         *  usage:         
            //load texture to chanel 0 - 7
            //this load operation only excute at the first time 
            //except call the ClearTexture(void) or ReleaseTexture(int)
              LoadTexture(imagefile,0);
                      
            //bind the texture to channel 0 - 7,this only excute at the first time except call 
            //ClearTexture(void) or ReleaseTexture(int)
              BindTexture(0);

            //also can bind a bitmap already exist and don't need call LoadTexture() to load it
            //original image can be released after binded
              BindTexture(0,bitmap);

            //there are serval ways to set the texture filters parameter:
              1. specified the filters parameter when call BindTexture(...)
              BindTexture(0,bitmap,texPara);              
              2.call the SetTextureFilter(TextureParameter param )
              SetTextureFilter(texPara);
              3.use the global class variable multiTextures
              multiTextures.ClearTexParameters(); 
              multiTextures.glTexParameteri(GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_REPEAT); 
              multiTextures.glTexParameterf(GLES20.GL_TEXTURE_MAX_ANISOTROPY_EXT, floatPara); //floatPara is float value
              multiTextures.glTexEnvi(GLES20.GL_TEXTURE_ENV_MODE, GLES20.GL_COMBINE_ARB);
              multiTextures.glTexEnvf(GLES20.GL_COMBINE_RGB_EXT, floatPara);    //floatPara is float value

            //bind the texture coords,texCoordsArray is a points array,such as float[]texCoordsArray,            
              BindTextureCoords(texCoordsArray,0 ); 
            // 0 is the specified chanel,default,it can be called as follow
              BindTextureCoords(texCoordsArray); 
            //default texture Coords is 2 floats,it can be changed as
              BindTextureCoords(texCoordsArray,0,3);
              
            //repeat to load and bind more textures( maximum 8 )
              LoadTexture(imagefile,1); 
              BindTexture(1); 
              BindTextureCoords(texCoordsArray,1 ); 
              
            //then draw objects, recommend to use VBODraw(), 
            //multiple textures should call VBODrawOnMultiTexture 
            //refer to the usage of VBODrawOnMultiTexture
              VBODrawOnMultiTexture(..); 

            // the default image size was scaled to fit the size of PowerOfTwo, call SetPowerOfTwo(false) to change it 
            // image size also be scaled if orginal image size exceed GL_MAX_TEXTURE_SIZE,            
            // all texure information are stored in multiTextures, call ClearTexture() to clear all or
            // call ReleaseTexture(int id) to clear the specified channel when need to update the texture
         * ************************************************************/
        private GPUArray<float> m_GPUVertics;
        private GPUArray<byte> m_GPUColors;
        private GPUArray<float> m_GPUNormals;
        private GPUArray<float> m_GPUTexCoords;
#pragma warning disable CS0414 // 字段“COpenGL._supportNonPowerOfTwo”已被赋值，但从未使用过它的值
        private bool _supportNonPowerOfTwo = true;
#pragma warning restore CS0414 // 字段“COpenGL._supportNonPowerOfTwo”已被赋值，但从未使用过它的值
        public enum BufferDataType { Vertex, Color, Normal, TexCoord };
        public void EnableTexture(int chanel)
        {
         
        }
        public void DisableTexture(int chanel)
        {
         
        }
        public bool LoadTexture(string imagefile, int id = 0)
        {           
            return true;
        }
        public void SetPowerOfTwo(bool val)
        {            
        }
        public void ClearTexture()
        {           
        }
        public void ReleaseTexture(int textureid)
        {           
            DisableTexture(textureid);
        }
        public bool BindTexture(int textureid = 0, Bitmap img = null)
        {            
            return true;
        }
        public void BindTextureCoords(float[] texCoords, int id = 0, int texCoordFormat = 2)
        {            
        }
        public bool IsSupportNonPowerOfTwo()
        {
            return true;
        }
        public void ClearGPUBuffer()
        {
            m_GPUVertics = null;
            m_GPUVertics = null;
            m_GPUColors = null;
            m_GPUNormals = null;
            m_GPUTexCoords = null;
        }
        // send color data (byte) to buffer
        public bool SendDataToGPUBuffer(byte[] data, BufferDataType type)
        {
            if (type != BufferDataType.Color)
                return false;
            m_GPUColors = new GPUArrayHost<byte>(data);
            return true;
        }
        public bool SendDataToGPUBuffer(float[] data, BufferDataType type)
        {
            switch (type)
            {
                case BufferDataType.Vertex:
                    m_GPUVertics = new GPUArrayHost<float>(data);
                    break;
                case BufferDataType.Normal:
                    m_GPUNormals = new GPUArrayHost<float>(data);
                    break;
                case BufferDataType.TexCoord:
                    m_GPUTexCoords = new GPUArrayHost<float>(data);
                    break;
                case BufferDataType.Color:
                    // data[i] value must be 0-1
                    byte[] colorData = new byte[data.Length];
                    for(int i=0;i<data.Length;i++)
                    {
                        colorData[i] = (byte)(data[i] * 255);
                    }
                    m_GPUColors = new GPUArrayHost<byte>(colorData);
                    break;
                default: return false;
            }
            return true;
        }
        /******************************************************************
         * Draw Object useing VBO - similar to DrawElement()
         * Created by Jian in Jun 2017
         * usage:
         * // clear buffers VBO
         *    ClearVBOBuffer();
         * 
         * // then send data to buffers,          * 
         * // send vertex data,vertexs declared as float[]vertics;
         *    SendDataToGPUBuffer(vertexs,BufferDataType.Vertex);         * 
         * // send color data,colors declared as byte[]colors;
         *    SendDataToGPUBuffer(colors,BufferDataType.Color);
         * // send normal data,normals declared as float[]normals;
         *    SendDataToGPUBuffer(normals,BufferDataType.Normal);
         * // send texture coords data,texCoords declared as float[]texCoords;
         *    SendDataToGPUBuffer(texCoords,BufferDataType.TexCoord);
         *    
         * // then call VBODraw() to draw objects
         *    VBODraw();
         * // the default is draw Triangles,can be specified as
         *    VBODraw(DrawingPrimitive.Quads);
         * // buffer data format default as vertexs take 3 float,color take 3 bytes
         * // texture coord take 2 float, can be specified as
         *    VBODraw(DrawingPrimitive.Quads,4,4,3);
         * // if use multiple textures, should call VBODrawOnMultiTexture()
         * // you don't need specified the texture parameter when call VBODrawOnMultiTexture()
         * // you should call LoadTexture() and BindTexture() first to bind multiple texture
         * // the VBODrawOnMultiTexture() parameter is the same as VBODraw() except the Texcoords
         *    VBODrawOnMultiTexture ( DrawingPrimitive.Quads );
         * ****************************************************************/
        public enum DrawingPrimitive
        {
            Points, Triangles, TrangleStrip, Quads, QuadStrip
        }
        public bool VBODraw(DrawingPrimitive mode = DrawingPrimitive.Triangles,
                            int nVertexFormat = 3,  // 3,4
                            int nColorFormat = 3,   // 3 RGB or 4 RGBA
                            int nTexCoordFormat = 2)
        {
            return VBODraw(m_GPUVertics, m_GPUColors, m_GPUNormals,
                            m_GPUTexCoords, mode, nVertexFormat,
                            nColorFormat, nTexCoordFormat);
        }
        public bool VBODraw(GPUArray<float> Vertexs,
                            GPUArray<byte> Colors,
                            GPUArray<float> Normals,
                            GPUArray<float> Texcoords,
                            DrawingPrimitive mode = DrawingPrimitive.Triangles,
                            int nVertexFormat = 3,  // 3,4
                            int nColorFormat = 3,   // 3 RGB or 4 RGBA
                            int nTexCoordFormat = 2 // 2, 3
                            )
        {
            
            /*
            uint bufferid;
            // The following will be enabled/disabled with glEnableClientState/glDisableClientState			
            var clientStates = new List<int>();
            // The following will be enabled/disabled with glEnable/glDisable
            var states = new List<int>();

            // Set appripriate polygon mode and remember old one
            var oldModes = new int[2];
            GLES20.GetIntegerv(GLES20.GL_POLYGON_MODE, oldModes);
            CheckError("glGetIntegerv");

            //activateDrawingMode(DrawingMode);

            // Generate And Bind The Vertex Buffer
            if (Vertexs != null)
            {
                GLES20.EnableClientState(GLES20.GL_VERTEX_ARRAY);
                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.BufferDataARB(GLES20.GL_ARRAY_BUFFER_ARB, (IntPtr)(Vertexs.Length * sizeof(float)), Vertexs.Data, GLES20.GL_DYNAMIC_DRAW_ARB);
                GLES20.VertexPointer(nVertexFormat, GLES20.GL_FLOAT, 0, IntPtr.Zero);
                clientStates.Add(GLES20.GL_VERTEX_ARRAY);
            }
            if (Colors != null)
            {
                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.ColorPointer(nColorFormat, GLES20.GL_UNSIGNED_BYTE, 0, Colors.Data);
                clientStates.Add(GLES20.GL_COLOR_ARRAY);
            }
            if (Normals != null)
            {
                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.EnableClientState(GLES20.GL_NORMAL_ARRAY);
                GLES20.Enable(GLES20.GL_NORMALIZE);
                GLES20.NormalPointer(GLES20.GL_FLOAT, 0, Normals.Data);
                clientStates.Add(GLES20.GL_NORMAL_ARRAY);
                states.Add(GLES20.GL_NORMALIZE);
            }
            // Generate And Bind The Texture Coordinate Buffer
            if (Texcoords != null)
            {
                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.BufferDataARB(GLES20.GL_ARRAY_BUFFER_ARB, (IntPtr)(Texcoords.Length * sizeof(float)), Texcoords.Data, GLES20.GL_DYNAMIC_DRAW_ARB);
                GLES20.ClientActiveTexture(GLES20.GL_TEXTURE0_ARB);
                GLES20.EnableClientState(GLES20.GL_TEXTURE_COORD_ARRAY);
                GLES20.TexCoordPointer(nTexCoordFormat, GLES20.GL_FLOAT, 0, IntPtr.Zero);
                clientStates.Add(GLES20.GL_TEXTURE_COORD_ARRAY);
                states.Add(GLES20.GL_TEXTURE_2D);
            }

            // Convert GraphicAPI.DrawingPrimitive to the correspongind OpenGL value
            var glDrawMode = drawingPrimitiveToOGL(mode);

            //draw objects
            GLES20.DrawArrays(glDrawMode, 0, Vertexs.Length / nVertexFormat);
            CheckError("glDrawArrays");

            // Disable states			
            foreach (var clientState in clientStates)
            {
                GLES20.DisableClientState(clientState);
                CheckError("glDisableClientState");
            }
            foreach (var clientState in states)
            {
                GLES20.Disable(clientState);
                CheckError("glDisable");
            }
            // Restore old drawing modes
            GLES20.PolygonMode(GLES20.GL_FRONT, oldModes[0]);
            CheckError("glPolygonMode");

            GLES20.PolygonMode(GLES20.GL_BACK, oldModes[1]);
            CheckError("glPolygonMode");
            */
            return true;
        }
        public bool VBODrawOnMultiTexture(DrawingPrimitive mode = DrawingPrimitive.Triangles,
                            int nVertexFormat = 3,  // 3,4
                            int nColorFormat = 3,   // 3 RGB or 4 RGBA
                            int nTexCoordFormat = 2)
        {
            return VBODrawOnMultiTexture(m_GPUVertics, m_GPUColors, m_GPUNormals, mode, nVertexFormat, nColorFormat, nTexCoordFormat);
        }
        public bool VBODrawOnMultiTexture(GPUArray<float> Vertexs,
                            GPUArray<byte> Colors,
                            GPUArray<float> Normals,
                            DrawingPrimitive mode = DrawingPrimitive.Triangles,
                            int nVertexFormat = 3,  // 3,4
                            int nColorFormat = 3,   // 3 RGB or 4 RGBA
                            int nTexCoordFormat = 2 // 2, 3
                            )
        {
            /*
            uint bufferid;
            int nTexture = multiTextures.GetActivedTextureNum();
            // The following will be enabled/disabled with glEnableClientState/glDisableClientState			
            var clientStates = new List<int>();
            // The following will be enabled/disabled with glEnable/glDisable
            var states = new List<int>();

            // Set appripriate polygon mode and remember old one
            var oldModes = new int[2];
            GLES20.GetIntegerv(GLES20.GL_POLYGON_MODE, oldModes);
            CheckError("glGetIntegerv");

            //activateDrawingMode(DrawingMode);

            // Generate And Bind The Vertex Buffer
            if (Vertexs != null)
            {
                GLES20.EnableClientState(GLES20.GL_VERTEX_ARRAY);
                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.BufferDataARB(GLES20.GL_ARRAY_BUFFER_ARB, (IntPtr)(Vertexs.Length * sizeof(float)), Vertexs.Data, GLES20.GL_DYNAMIC_DRAW_ARB);
                GLES20.VertexPointer(nVertexFormat, GLES20.GL_FLOAT, 0, IntPtr.Zero);
                clientStates.Add(GLES20.GL_VERTEX_ARRAY);
            }
            if (Colors != null)
            {
                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.ColorPointer(nColorFormat, GLES20.GL_UNSIGNED_BYTE, 0, Colors.Data);
                clientStates.Add(GLES20.GL_COLOR_ARRAY);
            }
            if (Normals != null)
            {
                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.EnableClientState(GLES20.GL_NORMAL_ARRAY);
                GLES20.Enable(GLES20.GL_NORMALIZE);
                GLES20.NormalPointer(GLES20.GL_FLOAT, 0, Normals.Data);
                clientStates.Add(GLES20.GL_NORMAL_ARRAY);
                states.Add(GLES20.GL_NORMALIZE);
            }
            // Generate And Bind The multiTexture Coordinate Buffer
            for (int i = 0; i < nTexture; i++)
            {
                CTextureCoord coord = multiTextures.GetTexCoordData(i);
                if (coord == null) continue;

                GLES20.GenBuffersARB(1, out bufferid);
                GLES20.BindBufferARB(GLES20.GL_ARRAY_BUFFER_ARB, bufferid);
                GLES20.BufferDataARB(GLES20.GL_ARRAY_BUFFER_ARB,
                                    (IntPtr)(coord.nCoordNum * nTexCoordFormat * sizeof(float)),
                                    coord.pTextCoords, GLES20.GL_STREAM_DRAW);
                GLES20.ClientActiveTexture(GLES20.GL_TEXTURE0_ARB + i);
                GLES20.EnableClientState(GLES20.GL_TEXTURE_COORD_ARRAY);
                GLES20.TexCoordPointer(nTexCoordFormat, GLES20.GL_FLOAT, 0, IntPtr.Zero);
            }

            // Convert GraphicAPI.DrawingPrimitive to the correspongind OpenGL value
            var glDrawMode = drawingPrimitiveToOGL(mode);

            //draw objects
            GLES20.DrawArrays(glDrawMode, 0, Vertexs.Length / nVertexFormat);
            CheckError("glDrawArrays");

            // Disable states			
            foreach (var clientState in clientStates)
            {
                GLES20.DisableClientState(clientState);
                CheckError("glDisableClientState");
            }
            for (int i = 0; i < nTexture; i++)
            {
                GLES20.ClientActiveTexture(GLES20.GL_TEXTURE0_ARB + i);
                GLES20.DisableClientState(GLES20.GL_TEXTURE_COORD_ARRAY);
            }
            foreach (var clientState in states)
            {
                GLES20.Disable(clientState);
                CheckError("glDisable");
            }

            // Restore old drawing modes
            GLES20.PolygonMode(GLES20.GL_FRONT, oldModes[0]);
            CheckError("glPolygonMode");

            GLES20.PolygonMode(GLES20.GL_BACK, oldModes[1]);
            CheckError("glPolygonMode");
            */
            return true;
        }

        /************************************************************
         *  Shader Section - to use the shader
         *  created by jian Jun 2017         
         *  usage:
         *  // begin.. load from shader file
         *  if( !BeginUseShader(vertexShaderFile,vertexFragFile) )
         *      MessageBox.Show(errorString);         *  
         *  // bind parameters
         *  BindAttribLocation(0,"vertex");  //bind attibes
         *  BindAttribLocation(1,"color");   //bind attibes
         *  SetShaderUniform1i(2,"texture"); //bind uniform parameters         *  
         *  // draw objects - recommend to use VBO to draw objecs         
         *  VBODraw(GLES20.GL_QUADS);
         *  // when support multiple textures,should call VBODrawOnMultiTexture(..)
         *  VBODrawOnMultiTexture(GLES20.GL_QUADS);
         *  refer to the usage of VBODraw() and VBODrawOnMultiTexture()
        ***********************************************************/
#pragma warning disable CS0169 // 从不使用字段“COpenGL.vShader”
        private int vShader;
#pragma warning restore CS0169 // 从不使用字段“COpenGL.vShader”
#pragma warning disable CS0169 // 从不使用字段“COpenGL.fShader”
        private int fShader;
#pragma warning restore CS0169 // 从不使用字段“COpenGL.fShader”
#pragma warning disable CS0169 // 从不使用字段“COpenGL.program”
        private int program;
#pragma warning restore CS0169 // 从不使用字段“COpenGL.program”
#pragma warning disable CS0169 // 从不使用字段“COpenGL.vertexString”
        private string vertexString;
#pragma warning restore CS0169 // 从不使用字段“COpenGL.vertexString”
#pragma warning disable CS0169 // 从不使用字段“COpenGL.fragmentString”
        private string fragmentString;        
#pragma warning restore CS0169 // 从不使用字段“COpenGL.fragmentString”
       
        /// <summary>
        /// Shader function
        /// </summary>       
        public string ReadShaderSource(string shaderFile)
        {
            string ss, shaderString = "";
            try
            {
                FileStream fs = new FileStream(shaderFile, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                while ((ss = sr.ReadLine()) != null)
                {
                    if (ss.Length > 0)
                        shaderString += ss;
                }
                sr.Close();
                fs.Close();
            }
            catch (IOException e)
            {
                errorString = e.Message;
            }
            return shaderString;
        }
       
        
        
    }

}
