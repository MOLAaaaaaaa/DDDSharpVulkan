using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using GlmNet;
using DataCollection;
using Graphics3D;
using OpenGL;
using ArcTrackball;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using AviFile;
using System.Threading;
using ScriptInterpreter;
using WeifenLuo.WinFormsUI.Docking;

namespace DDDSharp
{
    public partial class DDDForm : DockingPaneExt
    {
        public CGraphic3D graphic = C3DData.graphics3D;
        public gDrawMode DrawMode = gDrawMode.Fill;
        public Thread recordingThread = null;
        private IntPtr OldHandle;

        #region ArcBall Control        
        // Rotation/Zoom/Pan
        private System.Object matrixLock = new System.Object();
        private Trackball arcBall = new Trackball(1024.0f, 768.0f);
        private float[] matrix = new float[16];
        private Matrix4f LastTransformation = new Matrix4f();
        private Matrix4f ThisTransformation = new Matrix4f();
        // mouse         
        private bool bMouseDown = false;
        private Point mouseStartDrag;
        private static bool isLeftDrag = false;
        private static bool isRightDrag = false;
        private static bool isMiddleDrag = false;        
        private bool IsControlKeyDown = false;
        private bool IsAltKeyDown = false;
        float av = 45;
        List<C3DObjectBase> pObjects = new List<C3DObjectBase>();

        C3DObjectBase arrowObject = new C3DObjectBase();
        C3DObjectBase outLinesObject = new C3DObjectBase();
        C3DObjectBase outLinesOfSelectedObject = new C3DObjectBase();
        C3DObjectBase axisXObject = new C3DObjectBase();
        C3DObjectBase axisYObject = new C3DObjectBase();
        C3DObjectBase axisZObject = new C3DObjectBase();

        C3DObjectBase lightposObject = new C3DObjectBase();

        #endregion ArcBall Control        

        public DDDForm()
        {
            InitializeComponent();
            KeyPreview = true;

            this.HandleCreated += DDDForm_HandleCreated;
        }
        private void DDDForm_HandleCreated(object sender, EventArgs e)
        {
            this.OldHandle = this.Handle;

            //Reinitialize();
        }
        public bool ResetEngine(CGraphic3D g3d)
        {
            graphic = g3d;
            if (graphic != null)
            {
                graphic.DestroyWindow();
            }
            if (InitGraphicsEngine())
            {
                C3DData.graphics3D = graphic;
                DrawObjects();
                UpdateView();
                return true;
            }
            return false;
        }

        private bool Reinitialize()
        {
            if (graphic.Initialize(this.Handle, "vulkan", Width, Height))
            {

            }
            foreach (var item in C3DData.GetObjects())
            {
                item.RenderMode = RenderingUpdateMode.Redraw;
            }
            UpdateDraw();
         
            return true;
        }
        public bool InitVulkan()
        {
            if (!CGraphic3D.IsVulkanSupport())
            {
                return false;
            }

            graphic = new CVulkan();
            if (graphic.Initialize(this.Handle, "vulkan", Width, Height))
            {
                CreateEvents();
                InitMatrixVulkan();
                InitArcBall();
                return true;
            }
            return false;
        }
        private bool InitOpenGLES()
        {
            graphic = new MySharpGLES();

            if (graphic.Initialize(this.Handle, "GLES", Width, Height))
            {
                CreateEvents();
                InitMatrixVulkan();
                InitArcBall();
                return true;
            }
            return false;
        }

        private bool InitOpenGL()
        {
            graphic = new SharpGLNet();

            return true;
        }

        public bool InitGraphicsEngine()
        {
            if (graphic.engine == gEngine.auto || graphic.engine == gEngine.vulkan)
            {
                if (InitVulkan()) return true;
                else
                {
                    graphic.engine = gEngine.opengl;
                    AddToMessage("Initiating Vulkan Device Failed.\n" + graphic.GetLastErrMessage());
                }
            }

            if (graphic.engine == gEngine.opengl)
            {
                if (InitOpenGL())
                {
                    CreateEvents();
                    return true;
                }
                else
                {
                    graphic.engine = gEngine.opengles;
                    AddToMessage("Initiating OpenGL Device Failed.\n" + graphic.GetLastErrMessage());
                    return false;
                }
            }

            if (graphic.engine == gEngine.opengles)
            {
                if (InitOpenGLES()) return true;
                else
                {
                    graphic.engine = gEngine.auto;
                    AddToMessage("Initiating OpenGLES Device Failed.\n" + graphic.GetLastErrMessage());
                    return false;
                }
            }

            return false;
        }
        void CreateEvents()
        {
            if (graphic.engine == gEngine.opengles || graphic.engine == gEngine.vulkan)
            {
                this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DoMouseDown);
                this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DoMouseMove);
                this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DoMouseUp);
                this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DoMouseWheel);
                this.SizeChanged += new System.EventHandler(this.DoSizeChanged);
                this.Paint += new System.Windows.Forms.PaintEventHandler(this.DoPaint);
     
            }
            else if (graphic.engine == gEngine.opengl)
            {
                this.SizeChanged += new System.EventHandler(this.DoSizeChanged);
            }
        }
        private void glRender(object sender, GlControlEventArgs e)
        {
            if (graphic == null || !graphic.initialized) return;
            graphic.UpdateDraw();
        }
        private void glUpdate(object sender, GlControlEventArgs e)
        {
            if (graphic == null || !graphic.initialized) return;
            graphic.UpdateDraw();
        }
        private void glContextCreated(object sender, GlControlEventArgs e)
        {
            if (graphic != null && graphic.engine == gEngine.opengl)
            {
                graphic = new SharpGLNet();
                if (!graphic.Initialize(this.Handle, "OPENGL", Width, Height))
                {
                    AddToMessage("Initiating OpenGL Failed.\n" + graphic.GetLastErrMessage());
                    return;
                }
                CreateEvents();
               
                //this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.DoPaint);
                C3DData.graphics3D = graphic;
               
                //modified by li 2022/11/22
                //InitMatrixVulkan();
                InitMatrixOpenGL();
                InitArcBall();
            }
            else
            {
                
            }
        }

        public void InitArcBall()
        {
            //Init ArcBall
            LastTransformation.SetIdentity();
            ThisTransformation.SetIdentity();
            ThisTransformation.get_Renamed(matrix);
            arcBall.setBounds(Width, Height); // Update mouse bounds for arcball
            arcBall.horDragInverse = false;
            arcBall.verDragInverse = false;
            if (graphic.engine == gEngine.opengl ||
                graphic.engine == gEngine.opengles) arcBall.verDragInverse = true;
        }

        private bool CheckVialidation()
        {
            int year = C3DData.validYear;
            int month = C3DData.validMonth;
            int day = C3DData.validDay;
            int expiredDays = C3DData.expiredDays;

            if (DateTime.Now.Year != year) return false;

            DateTime d1 = new DateTime(year, month, day);
            DateTime d2 = DateTime.Now;

            TimeSpan sp = d2 - d1;
            if (sp.TotalDays < 0 || sp.TotalDays > expiredDays) return false;

            FileInfo ff = new FileInfo("3DSurferV3.exe");
            if (!ff.Exists) return false;
            d1 = ff.LastWriteTime;
            sp = d2 - d1;
            if (sp.TotalDays < 0 || sp.TotalDays > expiredDays) return false;

            return true;
        }
        private void DDDForm_Load(object sender, EventArgs e)
        {
            if (!graphic.initialized)
            {
                if (InitGraphicsEngine()) C3DData.graphics3D = graphic;
            }

            //if (graphic == null || !graphic.initialized) return;
            CDataModel.CalculateModelSize();

            InitAxis();
            
            DrawObjects();

            SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            UpdateView();
        }
        public void InitCamera()
        {
            // set view matrixVDB
            if (CDataModel.IsGeoCoordinateSystem) //此处有问题
            {
                graphic.LoadIdentity();
                vec3 m_eye = new vec3(0f, 0f, 5f);
                vec3 m_center = new vec3(0, 0, 0);
                vec3 m_up = new vec3(0, 1, 0);
                graphic.LookAt(m_eye, m_center, m_up);
                graphic.Perspective(45, (float)Width / (float)Height, 0.001f, 1000f);
                graphic.m_modelMatrix.model = glm.rotate(graphic.m_modelMatrix.model, glm.radians(250), new vec3(1, 0, 0));
                graphic.m_modelMatrix.model = glm.rotate(graphic.m_modelMatrix.model, glm.radians(90), new vec3(0, 0, 1));
            }
            else
            {
                graphic.LoadIdentity();
                vec3 m_eye = new vec3(0.0f, 0.0f, 5.0f);
                vec3 m_center = new vec3(0, 0, 0);
                vec3 m_up = new vec3(0, 1, 0);
                graphic.LookAt(m_eye, m_center, m_up);
                graphic.Perspective(45, (float)Width / (float)Height, 0.001f, 1000f);
            }
            //需要将跟踪球设置成当前模型矩阵状态m_modelMatrix.model
            //for (int i = 0; i < 4; i++)
            //    for (int j = 0; j < 4; j++)
            //    {
            //        matrix[i * 4 + j] = graphic.m_modelMatrix.model[i, j];
            //    }

            //ThisTransformation.get_Renamed(matrix);            

        }
        public void InitMatrix()
        {
            InitArcBall();
            if (graphic.engine == gEngine.vulkan) InitMatrixVulkan();
            else InitMatrixOpenGL();
        }
        public void InitMatrixVulkan()
        {
            // initialize projection ,model and view matrixs
            graphic.LoadIdentity();
            InitCamera();
            for (int i = 0; i < 4; i++)
            {
                graphic.EnableLight(i, true);
                graphic.SetLightDiffuse(i, new vec4(0.8f, 0.8f, 0.8f, 1.0f));
                graphic.SetLightAmbient(i, new vec4(0.02f, 0.02f, 0.02f, 1.0f));
                graphic.SetLightSpecular(i, new vec4(0.8f, 0.8f, 0.8f, 1.0f));
            }
            int id = 0;
            graphic.SetLightPos(id++, new vec3(0, 0, 5));
            graphic.SetLightPos(id++, new vec3(0, 0, -5));
            graphic.SetLightPos(id++, new vec3(0, 5, 0));
            graphic.SetLightPos(id++, new vec3(0, -5, 0));

            //graphic.SetLightPos(id++, new vec3(0, -5, 0));
            //graphic.SetLightPos(id++, new vec3(5, 0, 0));
            //graphic.SetLightPos(id++, new vec3(-5, 0, 0));

            graphic.SetMaterialShininess(60);
            graphic.SetMaterialSpecular(new vec4(1.0f, 1.0f, 1.0f, 1.0f));
            graphic.SetClearColor(new vec4(0.125f, 0.25f, 0.5f, 1));
        }
        private void InitMatrixOpenGL()
        {
            // initialize projection ,model and view matrixs
            graphic.LoadIdentity();
            // set view matrix
            vec3 m_eye = new vec3(0.0f, 0.0f, 5.0f);
            vec3 m_center = new vec3(0, 0, 0);
            vec3 m_up = new vec3(0, 1, 0);
            graphic.LookAt(m_eye, m_center, m_up);
            graphic.Perspective(30, (float)Width / Height, 0.001f, 1000f);

            graphic.SetLightDiffuse(0, new vec4(0.5f, 0.5f, 0.5f, 1.0f));
            graphic.SetLightAmbient(0, new vec4(0.2f, 0.2f, 0.2f, 1.0f));
            graphic.SetLightSpecular(0, new vec4(1.0f, 1.0f, 1.0f, 1.0f));
            graphic.SetLightPos(0, new vec3(0, 0, 5));
            graphic.EnableLight(0, true);

            graphic.SetLightDiffuse(1, new vec4(0.5f, 0.5f, 0.5f, 1.0f));
            graphic.SetLightAmbient(1, new vec4(0.2f, 0.2f, 0.2f, 1.0f));
            graphic.SetLightSpecular(1, new vec4(1.0f, 1.0f, 1.0f, 1.0f));
            graphic.SetLightPos(1, new vec3(0, 0, -5));
            graphic.EnableLight(1, true);

            graphic.SetMaterialShininess(3);
            graphic.SetMaterialSpecular(new vec4(0.5f, 0.5f, 0.5f, 0.5f));

            graphic.SetClearColor(new vec4(0.125f, 0.25f, 0.5f, 1));
        }

        public void LightAndMaterialSet()
        {
            OptionForm op = new OptionForm();

            for (int i = 0; i < graphic.m_modelMatrix.lights.Length; i++)
            {
                op.pLights.Add(graphic.m_modelMatrix.lights[i]);
            }

            if (op.ShowDialog() != DialogResult.OK) return;

            for (int i = 0; i < op.pLights.Count; i++)
            {
                graphic.m_modelMatrix.lights[i] = op.pLights[i].Copy();
            }

            UpdateDraw();
        }
        
        private vec4 ConvertColor(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;
            return new vec4(r, g, b, a);
        }
        private vec4 ConvertColor(ColorRGBA color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;
            return new vec4(r, g, b, a);
        }

        //对象绘制
        public void DrawObject(C3DObjectBase obj)
        {
            if ( !CDataModel.IsInModelRange(obj.Minx, obj.Miny, 
                                            obj.Minz, obj.Maxx, 
                                            obj.Maxy, obj.Maxz)) return;
          //  try
            {
                if (obj.type == ShapeEnum.Grid3D)
                {
                    Draw3DGrid((C3DGridData)obj);
                }
                else if (obj.type == ShapeEnum.Mesh)
                {
                    DrawMeshObj((CMesh)obj);
                }
                else if (obj.type == ShapeEnum.Triangles)
                {
                    DrawTrianglesObj((TriangleObj)obj);
                }
                else if (obj.type == ShapeEnum.Shape)
                {
                    DrawShapeObj( (Symbol3D)obj );
                }
                else if (obj.type == ShapeEnum.Text)
                {
                    DrawTextObj((TexturedText)obj);
                }
                else if (obj.type == ShapeEnum.Boreholes)
                {
                    DrawBoreHolesObj((CBoreholes)obj);                    
                }
                else if (obj.type == ShapeEnum.Borehole)
                {
                    DrawBoreHoleObj((CBorehole)obj);
                }
                else if (obj.type == ShapeEnum.LineMesh)
                {
                    DrawMeshLinesObj((LineMesh)obj);
                }
                else if (obj.type == ShapeEnum.GeoLayerMeshes)
                {
                    DrawGeoMeshesObj((GeoLayerMeshes)obj);
                }
                else if (obj.type == ShapeEnum.Line)
                {
                    DrawLineObj((C3DLine)obj);
                }
                else if (obj.type == ShapeEnum.Polygon2D)
                {
                    DrawPolygon2DObj((Polygon2D)obj, null);
                }
                else if (obj.type == ShapeEnum.Polygon2Ds)
                {
                    Draw2DPolygonsObj((C2DPolygons)obj);
                }
                else if (obj.type == ShapeEnum.PolygonSlicer)
                {
                    DrawPolygonSlicer((PolygonSlicer)obj);
                }
                else if (obj.type == ShapeEnum.Points)
                {
                    DrawScatteredPoints((ScatteredPoints)obj);
                }
                else if (obj.type == ShapeEnum.Slicer || obj.type == ShapeEnum.GeoProfile)
                {
                    DrawSlicer((CSlicer)obj);
                }
            }
          //  catch (Exception ex)
            {
            //    AddToMessage(ex.Message);
            }
        }
        bool CompareShapesType(ShapeEnum type1, ShapeEnum type2)
        {
            if (type1 == ShapeEnum.Grid3D ||
                type1 == ShapeEnum.Triangles ||
                type1 == ShapeEnum.Cylinder ||                
                type1 == ShapeEnum.Shphere ||
                type1 == ShapeEnum.Polygon ||
                type1 == ShapeEnum.Box ||
                type1 == ShapeEnum.ISOSurface ||
                type1 == ShapeEnum.ISOSurfaceEX ||
                type1 == ShapeEnum.Slicer ||
                type1 == ShapeEnum.PolygonSlicer)
                return true;
            else if ((type1 == ShapeEnum.LineMesh ||
                      type1 == ShapeEnum.Polygon2D ||
                      type1 == ShapeEnum.Line) &&
                      (type2 == ShapeEnum.Points ||
                       type2 == ShapeEnum.Text))
                return true;

            return false;
        }
        /// <summary>
        /// 对显示数据进行排序，排序原则
        /// ->面->线->点->透明色
        /// </summary>
        /// <typeparam name=""></typeparam>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        List<int> SortObjects()
        {
            List<int> list1 = new List<int>();
            List<int> list2 = new List<int>();
            
            List<C3DObjectBase> objects = C3DData.GetObjects();

            int n = objects.Count;
            if (n < 1) return list1;

            C3DObjectBase obj, obj1, obj2;
            for (int i = 0; i < n; i++)
            {
                obj = objects[i];
                if (obj.GetOrderedAlpha() < 1) list2.Add(i);
                else list1.Add(i);
            }

            int id1, id2;
            n = list1.Count;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    obj1 = objects[list1[i]];
                    obj2 = objects[list1[j]];
                    if (!CompareShapesType(obj1.type, obj2.type))
                    {
                        id1 = list1[i];
                        id2 = list1[j];
                        list1[i] = id2;
                        list1[j] = id1;
                    }
                }
            }
            n = list2.Count;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    obj1 = objects[list2[i]];
                    obj2 = objects[list2[j]];
                    if (!CompareShapesType(obj1.type, obj2.type))
                    {
                        id1 = list2[i];
                        id2 = list2[j];
                        list2[i] = id2;
                        list2[j] = id1;
                    }
                }
            }

            list1.AddRange(list2);
            list2.Clear();

            return list1;
        }
        /// <summary>
        /// 绘制全部三维数据
        /// </summary>
        void DrawObjects()
        {
            ClearObjectDrawBuffer();

            //set polygon mode, default is filled
            graphic.SetClearColor(1f, 1f, 1f);

            //set Polygon Mode,default is fill
            graphic.SetPolygonMode(gDrawMode.Fill);

            this.Cursor = Cursors.WaitCursor;

            List<C3DObjectBase> objects = C3DData.GetObjects();
            
            graphic.ClearModelKeyBuffers();//不包括含有子对象的对象

            foreach (C3DObjectBase obj in objects)//先绘制不透明物体
            {   
                if ( obj.Alpha == 1f)
                {
                    DrawObject(obj); // 对象可能含有子对象
                    obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//不包括含有子对象的对象
                }
            }         
            foreach (C3DObjectBase obj in objects)//最后绘制透明物体
            {
                if ( obj.Alpha < 1f )
                {
                    DrawObject(obj); // 对象可能含有子对象
                    obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//不包括含有子对象的对象
                }
            }
            lightposObject.Visible = C3DData.bShowLightPositions;
            outLinesObject.Visible = C3DData.bShowOuterBox;
            arrowObject.Visible = C3DData.bShowDirectionArrow;
            DrawLightsPosition(lightposObject);

            DrawOutLines(outLinesObject);
            DrawDirectionArrow(arrowObject);
            
            UpdateAxisofRangeUpdated();
            
            DrawXAxis(CDataModel.xAxisRuler);
            DrawYAxis(CDataModel.yAxisRuler);
            DrawZAxis(CDataModel.zAxisRuler);

            DrawSelectedOutLines();           

            this.Cursor = DefaultCursor;

        }//DrawObjects


        void DrawOutLines(C3DObjectBase obj)
        {
            //开关视图
            if (obj.Visible != C3DData.bShowOuterBox &&
                obj.RenderingBuffers.Count > 0)
            {
                obj.Visible = C3DData.bShowOuterBox;
                graphic.SetModelsVisibleByKeys(obj.RenderingBuffers, obj.Visible);
                return;
            }

            obj.Visible = C3DData.bShowOuterBox;
            if (!obj.Visible) return;

            //重绘
            //清除缓存
            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();

            //set current color,default is white
            graphic.SetColor(0.8, 0.8, 0.8, 1.0);
            if (CDataModel.IsEarthMapVision)
            {
                DrawEarthWireframe(10, 10);
                return;
            }

            double xs = CDataModel.m_Model.XWidth / CDataModel.m_Model.MaxLength;
            double ys = CDataModel.m_Model.YWidth / CDataModel.m_Model.MaxLength;
            double zs = CDataModel.m_Model.ZWidth / CDataModel.m_Model.MaxLength;

            double outlineExtScale = 1.1;
            xs = xs * outlineExtScale;
            ys = ys * outlineExtScale;
            zs = zs * outlineExtScale;

            graphic.DrawBoxOutline(-xs/2, -ys / 2, -zs / 2, xs, ys, zs );

            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);
        }

        void InitAxis()
        {
            //Axis Arrow

            CDataModel.xArrow3D.labelColor = Color.Red;
            CDataModel.xArrow3D.HeaderColor = Color.Blue;
            CDataModel.xArrow3D.LineColor = Color.Red;
            
            CDataModel.yArrow3D.labelColor = Color.Green;
            CDataModel.yArrow3D.HeaderColor = Color.Blue;
            CDataModel.yArrow3D.LineColor = Color.Green;

            CDataModel.zArrow3D.labelColor = Color.Blue;
            CDataModel.zArrow3D.HeaderColor = Color.Blue;
            CDataModel.zArrow3D.LineColor = Color.Blue;

            CDataModel.xGeoArrow3D.labelColor = Color.Red;
            CDataModel.xGeoArrow3D.HeaderColor = Color.Blue;
            CDataModel.xGeoArrow3D.LineColor = Color.Red;

            CDataModel.yGeoArrow3D.labelColor = Color.Green;
            CDataModel.yGeoArrow3D.HeaderColor = Color.Blue;
            CDataModel.yGeoArrow3D.LineColor = Color.Green;

            CDataModel.zGeoArrow3D.labelColor = Color.Blue;
            CDataModel.zGeoArrow3D.HeaderColor = Color.Blue;
            CDataModel.zGeoArrow3D.LineColor = Color.Blue;

            //ticks and label
            CDataModel.xAxisRuler.SetMinumMaximum(CDataModel.m_Model.X1, CDataModel.m_Model.X2);
            CDataModel.xAxisRuler.dockingEdge = AxisDockingEdge.Bottom;
            CDataModel.xAxisRuler.labelColor = Color.Red;
            CDataModel.xAxisRuler.labelSize = 0.2f;

            CDataModel.yAxisRuler.SetMinumMaximum(CDataModel.m_Model.Y1, CDataModel.m_Model.Y2);
            CDataModel.yAxisRuler.dockingEdge = AxisDockingEdge.Bottom;
            CDataModel.yAxisRuler.labelColor = Color.Green;
            CDataModel.yAxisRuler.labelSize = 0.22f;

            CDataModel.zAxisRuler.SetMinumMaximum(CDataModel.m_Model.Z1, CDataModel.m_Model.Z2);
            CDataModel.zAxisRuler.dockingEdge = AxisDockingEdge.FrontLeft;
            CDataModel.zAxisRuler.labelColor = Color.Blue;
            CDataModel.zAxisRuler.labelSize = 0.2f;
        }
        void DoRangeUpdated()
        {
            if ( CDataModel.rangeUpdated )
            {
                DrawOutLines(outLinesObject);
                DrawDirectionArrow(arrowObject);
                UpdateAxisofRangeUpdated();
                DrawXAxis(CDataModel.xAxisRuler);
                DrawYAxis(CDataModel.yAxisRuler);
                DrawZAxis(CDataModel.zAxisRuler);
                CDataModel.rangeUpdated = false;
            }
        }
        void UpdateAxisofRangeUpdated()
        {          
            if (CDataModel.m_Model.X1 != CDataModel.xAxisRuler.Minimum ||
                CDataModel.m_Model.X2 != CDataModel.xAxisRuler.Maximum )
            {
                CDataModel.xAxisRuler.SetMinumMaximum(CDataModel.m_Model.X1, CDataModel.m_Model.X2); 
            }
            if (CDataModel.m_Model.Y1 != CDataModel.yAxisRuler.Minimum ||
                CDataModel.m_Model.Y2 != CDataModel.yAxisRuler.Maximum)
            {
                CDataModel.yAxisRuler.SetMinumMaximum(CDataModel.m_Model.Y1, CDataModel.m_Model.Y2);
            }

            if (CDataModel.m_Model.Z1 != CDataModel.zAxisRuler.Minimum ||
                CDataModel.m_Model.Z2 != CDataModel.zAxisRuler.Maximum)
            {
                CDataModel.zAxisRuler.SetMinumMaximum(CDataModel.m_Model.Z1, CDataModel.m_Model.Z2);
            }               
            
        }
        
        void DrawXAxis(Axis3DRuler axis)
        {
            ClearObjectDrawBuffer(axisXObject);
            graphic.ClearModelKeyBuffers();

            if (CDataModel.IsEarthMapVision) return;
            if (!axis.Visible) return;
            if (!axis.IsValid()) return;

            double v1 = axis.StartValue;
            double v2 = axis.EndValue;
            float step = (float)axis.Step;

            string text;
            Vector64 p1 = new Vector64();

            Vector64 direct = new Vector64(0, -1, 0);
            Vector64 up = new Vector64(1, 0, 0);
            if ( CDataModel.IsGeoCoordinateSystem )
            {
                direct = new Vector64(0, 1, 0);
                up = new Vector64(1, 0, 0);
            }

            Font font = axis.labelFont;
            Color color = axis.labelColor;
            float size = axis.labelSize;

            double xs = CDataModel.m_Model.XWidth / CDataModel.m_Model.MaxLength;
            double ys = CDataModel.m_Model.YWidth / CDataModel.m_Model.MaxLength;
            double zs = CDataModel.m_Model.ZWidth / CDataModel.m_Model.MaxLength;
            double outlineExtScale = 1.1;
            int k = 0;

            Vertex3D vp1 = new Vertex3D();
            Vertex3D vp2 = new Vertex3D();
            vp1.color = ConvertColor(axis.tickColor);
            vp2.color = vp1.color;

            float shorttick = axis.shortTick;
            float longtick = axis.longTick;

            double ypos = ys * outlineExtScale / 2;
            double zpos = zs * outlineExtScale / 2;
            if( (axis.dockingEdge == AxisDockingEdge.Bottom && CDataModel.IsGeoCoordinateSystem ) ||
                (axis.dockingEdge == AxisDockingEdge.Top && !CDataModel.IsGeoCoordinateSystem) )
                zpos = zs * outlineExtScale / 2;


            //平面内旋转角
            Vector64 p2 = p1 + up;
            p2 = p2.RotateOnAngle(axis.LabelRotateAngle, 2);
            up = (p2 - p1).Normalize();
            p2 = p1 + direct;
            p2 = p2.RotateOnAngle(axis.LabelRotateAngle, 2);
            direct = (p2 - p1).Normalize();
            //倾角-UP不动
            p2 = p1 + direct;
            p2 = p2.RotateOnAngle(axis.PlaneAngle, 0);
            direct = (p2 - p1).Normalize();

            graphic.PushMatrix();
            //draw ticks first
            for ( double v = v1; v <= v2; v += step,k++ )
            {
                p1.X = v; p1.Y = p1.Z = 0;
                p1 = CDataModel.ToModelVector(p1);
                p1.Y = -ypos;
                p1.Z = -zpos;
                vp1.pos = new vec3( (float)p1.X, (float)p1.Y, (float)p1.Z);

                double tick = axis.shortTick;
                if (k % axis.minScale == 0) tick = axis.longTick;

                p2 = p1 + direct * tick;
                if ( (CDataModel.IsGeoCoordinateSystem && 
                    axis.dockingPosition == AxisDockingPosition.Outer) ||
                    ( !CDataModel.IsGeoCoordinateSystem &&
                    axis.dockingPosition == AxisDockingPosition.Inner) )
                    p2 = p1 - direct * tick;
                
                vp2.pos = new vec3((float)p2.X, (float)p2.Y, (float)p2.Z);
                //显示大刻度&&是否显示小刻度
                if (k % axis.minScale == 0 || (k % axis.minScale != 0 && axis.ShowTicks ) )
                graphic.DrawLine(vp1, vp2,false);
            }
            graphic.PopMatrix();            

            //draw string
            TextHorizontalAlignment hAlign = TextHorizontalAlignment.Left;
            TextVerticalAlignment vAlign = TextVerticalAlignment.Center;
            if (  (CDataModel.IsGeoCoordinateSystem && 
                  axis.dockingPosition == AxisDockingPosition.Outer) ||
                  (!CDataModel.IsGeoCoordinateSystem && 
                  axis.dockingPosition == AxisDockingPosition.Inner))
                  hAlign = TextHorizontalAlignment.Right;
            
            graphic.PushMatrix();
            step = (float)axis.Step * axis.minScale;
            for (double v = v1; v <= v2; v += step)
            {
                p1.X = v; p1.Y = p1.Z = 0;
                p1 = CDataModel.ToModelVector(p1);
                p1.Y = -ypos;
                p1.Z = -zpos;

                p2 = p1 + direct * (longtick + 0.002);
                if ((CDataModel.IsGeoCoordinateSystem &&
                    axis.dockingPosition == AxisDockingPosition.Outer) ||
                    (!CDataModel.IsGeoCoordinateSystem &&
                    axis.dockingPosition == AxisDockingPosition.Inner))
                    p2 = p1 - direct * (longtick + 0.002);                        

                text = axis.FormatValue(v);
                if (axis.IsUnitAttached && axis.secondTitle.Length > 0) 
                    text = text + axis.secondTitle;

                graphic.DrawString(text, font, color, size, p2, direct, up, hAlign, vAlign);

                //
                if( axis.secondTitle.Length > 0 && !axis.IsUnitAttached &&
                    C3DData.IsZero(v-v2,step*0.001 ) )
                {
                    p1.X = 0.55; p1.Y = -ypos; p1.Z = -zpos;
                    p2 = p1 + direct * (longtick + 0.002);
                    if ((CDataModel.IsGeoCoordinateSystem &&
                        axis.dockingPosition == AxisDockingPosition.Outer) ||
                        (!CDataModel.IsGeoCoordinateSystem &&
                        axis.dockingPosition == AxisDockingPosition.Inner))
                        p2 = p1 - direct * (longtick + 0.002);

                    text = axis.secondTitle;
                    graphic.DrawString(text, axis.secondTitleFont, axis.secondTitleColor, axis.secondTitleSize, p2, direct, up, hAlign, vAlign);
                }
            }
            graphic.PopMatrix();

            axisXObject.AddRenderingBuffer(graphic.objectModelKeyBuffers);
        }
        void DrawYAxis(Axis3DRuler axis)
        {
            ClearObjectDrawBuffer(axisYObject);
            graphic.ClearModelKeyBuffers();

            if (!axis.Visible) return;
            if (!axis.IsValid()) return;
            if (CDataModel.IsEarthMapVision) return;

            double v1 = axis.StartValue;
            double v2 = axis.EndValue;
            float step = (float)axis.Step;

            string text;
            Vector64 p1 = new Vector64();            
            Vector64 direct = new Vector64(1, 0, 0);
            Vector64 up = new Vector64(0, 1, 0);
            if (CDataModel.IsGeoCoordinateSystem)
            {
                direct = new Vector64(-1, 0, 0);
                up = new Vector64(0, 1, 0);
            }
            Font font = axis.labelFont;
            Color color = axis.labelColor;
            float size = axis.labelSize;

            double xs = CDataModel.m_Model.XWidth / CDataModel.m_Model.MaxLength;
            double ys = CDataModel.m_Model.YWidth / CDataModel.m_Model.MaxLength;
            double zs = CDataModel.m_Model.ZWidth / CDataModel.m_Model.MaxLength;
            double outlineExtScale = 1.1;
            int k = 0;

            Vertex3D vp1 = new Vertex3D();
            Vertex3D vp2 = new Vertex3D();
            vp1.color = ConvertColor(axis.tickColor);
            vp2.color = vp1.color;

            float shorttick = axis.shortTick;
            float longtick = axis.longTick;
            double xpos = xs * outlineExtScale / 2;
            double ypos = ys * outlineExtScale / 2;
            double zpos = zs * outlineExtScale / 2;

            if ((axis.dockingEdge == AxisDockingEdge.Bottom && CDataModel.IsGeoCoordinateSystem) ||
                (axis.dockingEdge == AxisDockingEdge.Top && !CDataModel.IsGeoCoordinateSystem))
                zpos = zs * outlineExtScale / 2;

            //平面内旋转角
            Vector64 p2 = p1 + up;
            p2 = p2.RotateOnAngle(axis.LabelRotateAngle, 2);
            up = (p2 - p1).Normalize();
            p2 = p1 + direct;
            p2 = p2.RotateOnAngle(axis.LabelRotateAngle, 2);
            direct = (p2 - p1).Normalize();
            //倾角-UP不动
            p2 = p1 + direct;
            p2 = p2.RotateOnAngle(axis.PlaneAngle, 1);
            direct = (p2 - p1).Normalize();

            graphic.PushMatrix();
            //draw ticks first
            for (double v = v1; v <= v2; v += step, k++)
            {
                p1.Y = v; p1.X = p1.Z = 0;
                p1 = CDataModel.ToModelVector(p1);
                p1.X = -xpos;
                p1.Z = -zpos;
                vp1.pos = new vec3((float)p1.X, (float)p1.Y, (float)p1.Z);
                
                double tick = axis.shortTick;
                if (k % axis.minScale == 0) tick = axis.longTick;
                p2 = p1 + direct * tick;
                if ( (CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Inner)||
                    (!CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Outer) )
                    p2 = p1 - direct * tick;
                vp2.pos = new vec3((float)p2.X, (float)p2.Y, (float)p2.Z);

                //显示大刻度&&是否显示小刻度
                if (k % axis.minScale == 0 || (k % axis.minScale != 0 && axis.ShowTicks))
                    graphic.DrawLine(vp1, vp2,false);
            }
            graphic.PopMatrix();

            //draw string
            TextHorizontalAlignment hAlign = TextHorizontalAlignment.Right;
            TextVerticalAlignment vAlign = TextVerticalAlignment.Center;
            if ( (CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Outer)
                || (!CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Inner))
                   hAlign = TextHorizontalAlignment.Left;

            step = (float)axis.Step * axis.minScale;
            graphic.PushMatrix();
            for (double v = v1; v <= v2; v += step)
            {
                p1.Y = v; p1.X = p1.Z = 0;
                p1 = CDataModel.ToModelVector(p1);
                p1.X = -xpos;
                p1.Z = -zpos;
                
                p2 = p1 + (longtick + 0.002) * direct;
                if ((CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Inner) ||
                    (!CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Outer))
                    p2 = p1 - (longtick + 0.002) * direct;

                text = axis.FormatValue(v);
                if (axis.IsUnitAttached && axis.secondTitle.Length > 0)
                    text = text + axis.secondTitle;

                graphic.DrawString(text, font, color, size, p2, direct, up, hAlign, vAlign);

                if (axis.secondTitle.Length > 0 && !axis.IsUnitAttached &&
                    C3DData.IsZero(v - v2, step * 0.001))
                {
                    p1.X = -xpos; p1.Y = ypos; p1.Z = -zpos;
                    p2 = p1 + direct * (longtick + 0.002);
                    if ((CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Inner) ||
                    (!CDataModel.IsGeoCoordinateSystem && axis.dockingPosition == AxisDockingPosition.Outer))
                        p2 = p1 - (longtick + 0.002) * direct;

                    text = axis.secondTitle;
                    graphic.DrawString(text, axis.secondTitleFont, axis.secondTitleColor, axis.secondTitleSize, p2, direct, up, hAlign, vAlign);
                }

            }
            graphic.PopMatrix();

            axisYObject.AddRenderingBuffer(graphic.objectModelKeyBuffers);
        }
        void DrawZAxis(Axis3DRuler axis)
        {
            ClearObjectDrawBuffer(axisZObject);
            graphic.ClearModelKeyBuffers();

            if (!axis.Visible) return;
            if (!axis.IsValid()) return;
            if (CDataModel.IsEarthMapVision) return;            

            double v1 = axis.StartValue;
            double v2 = axis.EndValue;
            float step = (float)axis.Step;

            string text;

            //docking outer 
            Vector64 p1 = new Vector64();
            Vector64 direct = new Vector64(1, 0, 0);
            Vector64 up = new Vector64(0, 0, 1);            
            if (CDataModel.IsGeoCoordinateSystem)
            {
                direct = new Vector64(0,1, 0);
                up = new Vector64(0, 0, -1);
            }
            Font font = axis.labelFont;
            Color color = axis.labelColor;
            float size = axis.labelSize;

            double xs = CDataModel.m_Model.XWidth / CDataModel.m_Model.MaxLength;
            double ys = CDataModel.m_Model.YWidth / CDataModel.m_Model.MaxLength;
            double zs = CDataModel.m_Model.ZWidth / CDataModel.m_Model.MaxLength;
            double outlineExtScale = 1.1;
            double xpos = xs * outlineExtScale / 2;
            double ypos = ys * outlineExtScale / 2;
            double zpos = zs * outlineExtScale / 2;
            int k = 0;

            Vertex3D vp1 = new Vertex3D();
            Vertex3D vp2 = new Vertex3D();
            vp1.color = ConvertColor(axis.tickColor);
            vp2.color = vp1.color;

            float shorttick = axis.shortTick;
            float longtick = axis.longTick;

            //平面内旋转角
            int rot_axis = 1;
            if (CDataModel.IsGeoCoordinateSystem) rot_axis = 0;
            Vector64 p2 = p1 + up;
            p2 = p2.RotateOnAngle(axis.LabelRotateAngle, rot_axis);
            up = (p2 - p1).Normalize();
            p2 = p1 + direct;
            p2 = p2.RotateOnAngle(axis.LabelRotateAngle, rot_axis);
            direct = (p2 - p1).Normalize();
            //倾角-UP不动
            p2 = p1 + direct;
            p2 = p2.RotateOnAngle(axis.PlaneAngle, 2);
            direct = (p2 - p1).Normalize();

            graphic.PushMatrix();

            //draw ticks first
            for (double v = v1; v <= v2; v += step, k++)
            {
                p1.Z = v; p1.Y = p1.X = 0;
                p1 = CDataModel.ToModelVector(p1);
                p1.Y = -ypos;
                p1.X = -xpos;
                vp1.pos = new vec3((float)p1.X, (float)p1.Y, (float)p1.Z);
                vp2 = vp1;
                
                float tick = shorttick;
                if (k % axis.minScale == 0)tick =longtick;
                
                p2 = p1 + direct * tick;                
                if ( axis.dockingPosition == AxisDockingPosition.Outer )
                    p2 = p1 - direct * tick;
                vp2.pos = new vec3((float)p2.X, (float)p2.Y, (float)p2.Z);

                //显示大刻度&&是否显示小刻度
                if (k % axis.minScale == 0 || (k % axis.minScale != 0 && axis.ShowTicks))
                    graphic.DrawLine(vp1, vp2,false);
            }
            graphic.PopMatrix();

            //draw string
            TextHorizontalAlignment hAlign = TextHorizontalAlignment.Right;
            TextVerticalAlignment vAlign = TextVerticalAlignment.Center;
            if (axis.dockingPosition == AxisDockingPosition.Inner)
                hAlign = TextHorizontalAlignment.Left;            

            k = 0;
            step = (float)axis.Step * axis.minScale;
            graphic.PushMatrix();
            for (double v = v1; v <= v2; v += step)
            {
                p1.Z = v; p1.Y = p1.X = 0;
                p1 = CDataModel.ToModelVector(p1);
                p1.X = -xpos;
                p1.Y = -ypos;

                p2 = p1 + (longtick + 0.002) * direct;
                if (axis.dockingPosition == AxisDockingPosition.Outer)
                    p2 = p1 - (longtick + 0.002) * direct;
                          
                text = axis.FormatValue(v);
                if (axis.IsUnitAttached && axis.secondTitle.Length > 0)
                    text = text + axis.secondTitle;

                graphic.DrawString(text, font, color, size, p2, direct, up,hAlign, vAlign);

                if (axis.secondTitle.Length > 0 && !axis.IsUnitAttached &&
                    C3DData.IsZero(v - v2, step * 0.001))
                {
                    p1.X = -xpos; p1.Y = -ypos; p1.Z = zpos;
                    p2 = p1 + (longtick + 0.002) * direct;
                    if (axis.dockingPosition == AxisDockingPosition.Outer)
                        p2 = p1 - (longtick + 0.002) * direct;                    
                    graphic.DrawString(axis.secondTitle, axis.secondTitleFont, axis.secondTitleColor, 
                        axis.secondTitleSize, p2, direct, up, hAlign, vAlign);
                }
            }
            graphic.PopMatrix();
            axisZObject.AddRenderingBuffer(graphic.objectModelKeyBuffers);
        }

        void DrawSelectedOutLines()
        {  
            ClearObjectDrawBuffer(outLinesOfSelectedObject);
            graphic.ClearModelKeyBuffers();

            if (!C3DData.bShowSelectedOuterBox) return;

            C3DObjectBase obj = C3DData.objSelected;
            if (obj == null) return;

            graphic.PushMatrix();
            
            //set current color,default is white
            graphic.SetColor(1.0, 0, 0);
            
            double x0 = (obj.Minx + obj.Maxx) / 2.0;
            double y0 = (obj.Miny + obj.Maxy) / 2.0;
            double z0 = (obj.Minz + obj.Maxz) / 2.0;
            
            double dx = (obj.Maxx - obj.Minx) * 0.01;
            double dy = (obj.Maxy - obj.Miny) * 0.01;
            double dz = (obj.Maxz - obj.Minz) * 0.01;

            Vector64 p1 = new Vector64(obj.Minx - dx, obj.Miny - dy, obj.Minz - dz);
            Vector64 p2 = new Vector64(obj.Maxx + dx, obj.Maxy + dy, obj.Maxz + dz);

            Vector64 p0 = new Vector64(x0, y0, z0);
            p0 = obj.TransformedPoint(p0);
            p0 = toWorldVector(obj, p0);
            p0 = CDataModel.ToModelVector(p0);
            p1 = obj.TransformedPoint(p1);
            p1 = toWorldVector(obj, p1);
            p1 = CDataModel.ToModelVector(p1);
            p2 = obj.TransformedPoint(p2);
            p2 = toWorldVector(obj, p2);
            p2 = CDataModel.ToModelVector(p2);           
            dx = Math.Abs(p2.X - p1.X);
            dy = Math.Abs(p2.Y - p1.Y);
            dz = Math.Abs(p2.Z - p1.Z);
            if (dx == 0) dx = 0.01;
            if (dy == 0) dy = 0.01;
            if (dz == 0) dz = 0.01;

            graphic.DrawBoxOutline(p1.X, p1.Y, p1.Z,dx,dy,dz);

            graphic.PopMatrix();

            outLinesOfSelectedObject.AddRenderingBuffer(graphic.objectModelKeyBuffers);
        }
        void DrawLightsPosition(C3DObjectBase obj)
        {
            return;
            //开关视图
            if ( obj.Visible != C3DData.bShowLightPositions &&
                obj.RenderingBuffers.Count > 0 )
            {
                obj.Visible = C3DData.bShowLightPositions;
                graphic.SetModelsVisibleByKeys(obj.RenderingBuffers, obj.Visible);
                return;
            }

            obj.Visible = C3DData.bShowLightPositions;
            if ( !obj.Visible ) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();

            int n = graphic.m_modelMatrix.lights.Length;
            LightStruct light;

            graphic.enableTranslate = true;
            graphic.enableScale = true;
            graphic.enableRotate = false;

            graphic.PushMatrix();
            for (int i = 0; i < n; i++)
            {
                light = graphic.m_modelMatrix.lights[i];
                if ( !light.Enable ) continue;
                graphic.PushMatrix();
                graphic.LoadIdentity();
                CSphere sp = new CSphere(light.pos.x, light.pos.y, light.pos.z);
                graphic.SetColor(new vec3(1, 1, 0));
                sp.Create(0.04, 0.001);                
                graphic.DrawShpere(sp);
                sp.Destroy();
                graphic.PopMatrix();
            }
            graphic.PopMatrix();

            graphic.enableTranslate = true;
            graphic.enableScale = true;
            graphic.enableRotate = true;
            //obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);
        }
        void DrawXDirectionArrow(Axis3DArrow arrow)
        {
            if (!arrow.Visible) return; 

            float size = arrow.labelSize;           
            string text = arrow.AxisName;           
            Font font = arrow.labelFont;

            double xs = CDataModel.m_Model.XWidth / CDataModel.m_Model.MaxLength;
            double ys = CDataModel.m_Model.YWidth / CDataModel.m_Model.MaxLength;
            double zs = CDataModel.m_Model.ZWidth / CDataModel.m_Model.MaxLength;

            double outlineScale = 1.1;
            double xp = xs * outlineScale * 0.5 + arrow.LineLength;            

            Vector32 centerStart = new Vector32(0, 0, 0);
            Vector32 cornerStart = new Vector32(-xs * outlineScale / 2, -ys * outlineScale / 2, -zs * outlineScale / 2);
            Vector32 xp1, xp2;
            //箭头中心绘制
            xp1 = xp2 = centerStart;
            //箭头靠边绘制
            if (!arrow.Center) xp1 = xp2 = cornerStart;            
            xp2.X = (float)xp; //延长线
           
            graphic.PushMatrix();
            graphic.SetColor(arrow.LineColor);
            graphic.SetLineWidth(arrow.LineWidth);
            graphic.SetLineStyle(gLineStyle.DashDot);
            graphic.DrawLine(CreateVertex(xp1), CreateVertex(xp2));
            graphic.PopMatrix();           

            //Draw Arrow3D            
            Cone ax = new Cone();
            ax.Rad = arrow.ArrowRadiu;
            ax.Height = arrow.ArrowHight;
            ax.Start = new Vector32(0, 0, 0);
            ax.Create();            

            TriangleObj tri = ax.toTriangleObject();
            tri.rotate = new vec3(0, -90, 0);
            tri.offset = new vec3(xp2.X, xp2.Y, xp2.Z);
            tri.color = ConvertColor(arrow.HeaderColor);
            DrawTriangles(tri, true);
           
            double offx = 0, offy = 0, offz = 0;
            if (text.Length > 0)
            {
                offx = arrow.ArrowHight / 2.0 + arrow.LabelOffset.X;
                offy = arrow.LabelOffset.Y;
                offz = arrow.LabelOffset.Z;

                TextHorizontalAlignment hAlign = TextHorizontalAlignment.Left;
                TextVerticalAlignment vAlign = TextVerticalAlignment.Center;
                if (arrow.Alignment == ArrowTextAlignment.Vertical)
                {
                    hAlign = TextHorizontalAlignment.Center;
                    vAlign = TextVerticalAlignment.Bottom;
                }
                Vector64 direct = new Vector64(1, 0, 0);//direction
                Vector64 up = new Vector64(0, 1, 0);//upwards

                if (CDataModel.IsGeoCoordinateSystem)
                {
                    if (arrow.Alignment == ArrowTextAlignment.Horizontal)
                    {
                        direct = new Vector64(1, 0, 0);
                        up = new Vector64(0, -1, 0);
                    }
                    else if (arrow.Alignment == ArrowTextAlignment.Vertical)
                    {
                        direct = new Vector64(0, 1, 0);
                        up = new Vector64(1, 0, 0);
                    }
                }
                else if (arrow.Alignment == ArrowTextAlignment.Vertical)
                {
                    direct = new Vector64(0, -1, 0);
                    up = new Vector64(1, 0, 0);
                }
                // upwards
                // ^  top
                // |  Hello -->direction
                // |  bottom
                graphic.DrawString(text, font, arrow.labelColor, size,
                                     new Vector64(xp2.X + offx, xp2.Y + offy, xp2.Z + offz), //start
                                     direct, //direction
                                     up, //upwards
                                     hAlign,
                                     vAlign);
            }            

        }
        void DrawYDirectionArrow(Axis3DArrow arrow)
        {
            if (!arrow.Visible) return; 

            float size = arrow.labelSize;   
            string text = arrow.AxisName;
            Font font = arrow.labelFont;

            double xs = CDataModel.m_Model.XWidth / CDataModel.m_Model.MaxLength;
            double ys = CDataModel.m_Model.YWidth / CDataModel.m_Model.MaxLength;
            double zs = CDataModel.m_Model.ZWidth / CDataModel.m_Model.MaxLength;

            double outlineScale = 1.1;
            double yp = ys * outlineScale * 0.5 + arrow.LineLength;

            Vector32 centerStart = new Vector32(0, 0, 0);
            Vector32 cornerStart = new Vector32(-xs * outlineScale / 2, -ys * outlineScale / 2, -zs * outlineScale / 2);
            Vector32 yp1, yp2;
            //箭头中心绘制            
            yp1 = yp2 = centerStart;           
            //箭头靠边绘制            
            if (!arrow.Center) yp1 = yp2 = cornerStart; 
            yp2.Y = (float)yp; //延长线           

            graphic.PushMatrix();
            graphic.SetColor(arrow.LineColor);
            graphic.SetLineWidth(arrow.LineWidth);
            graphic.SetLineStyle(gLineStyle.DashDot);
            graphic.DrawLine(CreateVertex(yp1), CreateVertex(yp2));
            graphic.PopMatrix();

            //Draw Arrow3D            
            Cone ay = new Cone();
            ay.Rad = arrow.ArrowRadiu;
            ay.Height = arrow.ArrowHight;
            ay.Start = new Vector32(0, 0, 0);
            ay.Create();

            TriangleObj tri = ay.toTriangleObject();
            tri.color = ConvertColor(arrow.HeaderColor);
            tri.rotate = new vec3(90, 0, 0);
            tri.offset = new vec3(yp2.X, yp2.Y, yp2.Z);
            DrawTriangles(tri, true);

            double offx = 0, offy = 0, offz = 0;
            if (text.Length > 0)
            {
                offx = arrow.LabelOffset.X;
                offy = arrow.ArrowHight / 2.0 + arrow.LabelOffset.Y;
                offz = arrow.LabelOffset.Z;
                if (CDataModel.IsGeoCoordinateSystem) //地质坐标系（Z向下）
                    graphic.DrawString(text, font, arrow.labelColor, size,
                                   new Vector64(yp2.X + offx, yp2.Y + offy, yp2.Z + offz),
                                   new Vector64(0, 1, 0),
                                   new Vector64(1, 0, 0),
                                   TextHorizontalAlignment.Left,
                                   TextVerticalAlignment.Center);
                else graphic.DrawString(text, font, arrow.labelColor, size,
                                   new Vector64(yp2.X + offx, yp2.Y + offy, yp2.Z + offz),
                                   new Vector64(0, 1, 0),
                                   new Vector64(-1, 0, 0),
                                   TextHorizontalAlignment.Left,
                                   TextVerticalAlignment.Center);
            }

        }
        void DrawZDirectionArrow(Axis3DArrow arrow)
        {
            if (!arrow.Visible) return;

            float size = arrow.labelSize;
            string text = arrow.AxisName;
            Font font = arrow.labelFont;

            double xs = CDataModel.m_Model.XWidth / CDataModel.m_Model.MaxLength;
            double ys = CDataModel.m_Model.YWidth / CDataModel.m_Model.MaxLength;
            double zs = CDataModel.m_Model.ZWidth / CDataModel.m_Model.MaxLength;

            double outlineScale = 1.1;
            double zp = zs * outlineScale * 0.5 + arrow.LineLength;

            Vector32 centerStart = new Vector32(0, 0, 0);
            Vector32 cornerStart = new Vector32(-xs * outlineScale / 2, -ys * outlineScale / 2, -zs * outlineScale / 2);
            Vector32 zp1, zp2;
            //箭头中心绘制            
            zp1 = zp2 = centerStart;
            //箭头靠边绘制            
            if (!arrow.Center) zp1 = zp2 = cornerStart;
            zp2.Z = (float)zp; //延长线           

            graphic.PushMatrix();
            graphic.SetColor(arrow.LineColor);
            graphic.SetLineWidth(arrow.LineWidth);
            graphic.SetLineStyle(gLineStyle.DashDot);
            graphic.DrawLine(CreateVertex(zp1), CreateVertex(zp2));
            graphic.PopMatrix();

            //Draw Arrow3D            
            Cone az = new Cone();
            az.Rad = arrow.ArrowRadiu;
            az.Height = arrow.ArrowHight;
            az.Start = new Vector32(0, 0, 0);
            az.Create();

            TriangleObj tri = az.toTriangleObject();
            tri.color = ConvertColor(arrow.HeaderColor);
            tri.rotate = new vec3(0, 180, 0);
            tri.offset = new vec3(zp2.X, zp2.Y, zp2.Z);
            DrawTriangles(tri, true);

            double offx = 0, offy = 0, offz = 0;
            if (text.Length > 0)
            {
                offx = arrow.LabelOffset.X;
                offy = arrow.LabelOffset.Y;
                offz = arrow.ArrowHight / 2.0 + arrow.LabelOffset.Z;
                if (CDataModel.IsGeoCoordinateSystem) //地质坐标系（Z向下）
                    graphic.DrawString(text, font, arrow.labelColor, size,
                                   new Vector64(zp2.X + offx, zp2.Y + offy, zp2.Z + offz),
                                   new Vector64(0, 0, 1),
                                   new Vector64(0, 1, 0),
                                   TextHorizontalAlignment.Left,
                                   TextVerticalAlignment.Center);
                else graphic.DrawString(text, font, arrow.labelColor, size,
                                   new Vector64(zp2.X + offx, zp2.Y + offy, zp2.Z + offz),
                                   new Vector64(1, 0, 0),
                                   new Vector64(0, 0, 1),
                                   TextHorizontalAlignment.Center,
                                   TextVerticalAlignment.Bottom);
            }
        }
        void DrawDirectionArrow(C3DObjectBase obj)
        {
            //开关视图
            if (obj.Visible != C3DData.bShowDirectionArrow &&
                obj.RenderingBuffers.Count > 0)
            {
                obj.Visible = C3DData.bShowDirectionArrow;
                graphic.SetModelsVisibleByKeys(obj.RenderingBuffers, obj.Visible);
                return;
            }
            
            //重绘
            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();

            obj.Visible = C3DData.bShowDirectionArrow;
            if (!obj.Visible) return;            
            
            Axis3DArrow xArrow = CDataModel.xArrow3D;
            Axis3DArrow yArrow = CDataModel.yArrow3D;
            Axis3DArrow zArrow = CDataModel.zArrow3D;
            if (CDataModel.IsEarthMapVision)
            {
                xArrow = CDataModel.xEarthArrow3D;
                yArrow = CDataModel.yEarthArrow3D;
                zArrow = CDataModel.zEarthArrow3D;
            }
            else
            {
                if (CDataModel.IsGeoCoordinateSystem)
                {
                    xArrow = CDataModel.xGeoArrow3D;
                    yArrow = CDataModel.yGeoArrow3D;
                    zArrow = CDataModel.zGeoArrow3D;
                }
            }

            DrawXDirectionArrow(xArrow);
            DrawYDirectionArrow(yArrow);
            DrawZDirectionArrow(zArrow);

            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);
        }

        public void AddToMessage(string info)
        {
            Program.m_MainForm.AddtoInfo(info);
        }
        Vertex3D CreateVertex(Vector32 p)
        {
            Vertex3D p1 = new Vertex3D(p.x, p.y, p.z);
            p1.SetColor(graphic.curColor);
            return p1;
        }
        Vertex3D CreateVertex(Vector64 p)
        {
            Vertex3D p1 = new Vertex3D((float)p.x, (float)p.y, (float)p.z);
            p1.SetColor(graphic.curColor);
            return p1;
        }
        Vector64 toWorldVector(C3DObjectBase obj, Vector64 p)
        {
            if (CDataModel.IsEarthMapVision) return obj.toWorldVector(p);
            else return p;
        }
        Vector64 toWorldVector(C3DObjectBase obj, Vector32 p)
        {
            return toWorldVector(obj, p.toVector64());
        }
        void ResetBitmapAlpha(Bitmap bmp,float alpha)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), 
                                         ImageLockMode.ReadWrite, 
                                         System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int stride = bd.Stride;
            //创建数组
            byte[] bytes = new byte[stride * bmp.Height];
            //拷贝图像数据到数组
            Marshal.Copy(bd.Scan0, bytes, 0, bytes.Length);
            for (int i = 0; i < bytes.Length; i += 4)
            {
                //对数组字节数据进行处理
                bytes[i + 3] = (byte)(alpha*255);
            }
            //拷贝数组到图像
            Marshal.Copy(bytes, 0, bd.Scan0, bytes.Length);
            //解释图像数据
            bmp.UnlockBits(bd);
            bytes = null;
        }

        Bitmap LoadTexture(TextureStruct tex)
        {
            if ( !tex.Enable || tex.TextureFile.Length < 1) return null;
            Bitmap bmp = graphic.LoadTexture(tex.TextureFile);
            if (bmp == null) return null;
            if (tex.FlipVertical) bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if (tex.FlipHorizontal) bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            //if( tex.Blend && tex.Alpha != 1.0f )
            //{
            //    ResetBitmapAlpha(bmp, tex.Alpha );
            //}
            if( tex.TransparentColors.Count > 0)
            {
                tex.MakeTransparent(bmp);
            }
            return bmp;
        }

       
        void DrawEarthWireframe(double stepx = 1, double stepy = 1)
        {
            int nx = (int)(360 / stepx);
            int ny = (int)(180 / stepy);
            Vertex3D[] points = new Vertex3D[nx * ny];
            EarthVector p = new EarthVector();

            graphic.PushMatrix();
            graphic.SetLineWidth(1);
            graphic.SetLineStyle(gLineStyle.Solid);
            graphic.SetColor(0.85, 0.8, 0.8, 1);
            
            //维线
            for (int i = 1; i < ny; i++)
            {
                if ( i * stepy == 90 )
                {
                    graphic.SetLineWidth(1);
                    graphic.SetColor(0, 0, 0, 1);
                }
                else 
                { 
                    graphic.SetLineWidth(1);
                    graphic.SetColor(0.85, 0.8, 0.8, 1);
                }
                graphic.Begin(DrawingPrimitive.LINE_STRIP);
                for (int j = 0; j < nx; j++)
                {
                    p.Longitude = j * stepx;
                    p.Latitude =  i * stepy;
                    graphic.AddPoint(CreateVertex(CDataModel.ToModelVector(p.toXYZVector())));
                    graphic.AddPointIndex(j);
                }
                graphic.AddPointIndex(0);
                graphic.End();
            }

            //经线
            graphic.SetColor(0.8, 0.85, 0.8, 1);
            graphic.SetLineStyle(gLineStyle.Dash);
            for (int j = 0; j < nx; j++)
            {
                if (j == 0)
                {
                    graphic.SetLineWidth(1);
                    graphic.SetColor(0, 0, 0, 1);
                }
                else 
                { 
                    graphic.SetLineWidth(1);
                    graphic.SetColor(0.8, 0.85, 0.8, 1);
                }
                graphic.Begin(DrawingPrimitive.LINE_STRIP);
                for (int i = 1; i < ny; i++)
                {
                    p.Longitude = j * stepx;
                    p.Latitude = i * stepy;
                    graphic.AddPoint(CreateVertex(CDataModel.ToModelVector(p.toXYZVector())));
                    graphic.AddPointIndex(i-1);
                }
                graphic.End();
            }
             
            graphic.PopMatrix();
        }
        private void DrawText(TexturedText obj)
        {
            Vector32 p1;
            vec4 _color = obj.color;
            _color.w = obj.Alpha;

            //create points
            Vertex3D[] points = new Vertex3D[obj.points.Count];
            for (int i = 0; i < points.Length; i++)
            {
                p1 = obj.TransformedPoint(obj.points[i]);
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);
                points[i] = graphic.CreatePoint(p1);
                points[i].color = obj.colors[i];
                points[i].texCoord = obj.texCoords[i];
            }
            //create indices
            int[] indices = new int[obj.triangles.Count * 3];
            for (int i = 0; i < obj.triangles.Count; i++)
            {
                indices[3 * i] = obj.triangles[i].x;
                indices[3 * i + 1] = obj.triangles[i].y;
                indices[3 * i + 2] = obj.triangles[i].z;
            }
            //begin draw
            graphic.PushMatrix();

            graphic.EnableTexture(true);

            graphic.BindTexture(obj.textureImage);

            graphic.SetPolygonMode(gDrawMode.Fill);

            graphic.DrawTriangles(points, indices);

            graphic.PopMatrix();

            graphic.DisableTexture();

            points = null;
            indices = null;
        }

        public void DrawTextObj(TexturedText obj)
        {
            if ( !obj.Visible ) return;
            obj.Create();
            if (obj.points.Count < 4) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲
            
            DrawText(obj);

            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }
        
        /// <summary>
        /// 绘制三角形对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="worldCoordinate">是否采样世界坐标，逻辑坐标</param>
        private void DrawTriangles(TriangleObj obj, bool worldCoordinate = false)
        {
            Vector32 p1;
            vec4 _color = obj.color;
            _color.w = obj.Alpha;

            //create points
            Vertex3D[] points = new Vertex3D[obj.points.Count];
            if (points == null) return;
            for (int i = 0; i < points.Length; i++)
            {
                p1 = obj.TransformedPoint(obj.points[i]);
                if ( !worldCoordinate )//转换成世界坐标
                {                    
                    p1 = toWorldVector(obj, p1);
                    p1 = CDataModel.ToModelVector(p1);
                }
                points[i] = graphic.CreatePoint(p1);
                points[i].color = _color; //object颜色
            }

            if (!obj.IsUniformColor && obj.colors.Count > 0)//顶点颜色不统一
            {
                for (int i = 0; i < obj.colors.Count; i++)
                {
                    _color = obj.colors[i];
                    _color.w = obj.Alpha;
                    points[i].color = _color;
                }
            }

            //try to load texture
            Bitmap bmp = null;
            if (obj.textureStruct.IsValidate())
            {
                bmp = LoadTexture(obj.textureStruct);
                if (bmp == null) AddToMessage("load texture failed.\n" + obj.textureStruct.TextureFile);
                else
                {
                    if (obj.texCoords.Count == 0)//计算纹理坐标
                    {
                        float x, y;
                        for (int i = 0; i < obj.points.Count; i++)
                        {
                            obj.GetTextureCoord(obj.points[i], out x, out y);
                            points[i].SetTexcoord(x, y);
                        }
                    }
                    else//使用已有纹理坐标
                    {
                        for (int i = 0; i < obj.points.Count; i++)
                            points[i].SetTexcoord(obj.texCoords[i]);
                    }
                }

            }//if (obj.textureImgFile.Length > 0 && obj.enbaleTexture)           

            int[] indices = new int[obj.triangles.Count * 3];
            for (int i = 0; i < obj.triangles.Count; i++)
            {
                indices[3 * i] = obj.triangles[i].x;
                indices[3 * i + 1] = obj.triangles[i].y;
                indices[3 * i + 2] = obj.triangles[i].z;
            }

            //begin draw
            graphic.PushMatrix(); 

            if (bmp != null)
            {
                graphic.EnableTexture(true);
                graphic.BindTexture(bmp);
                bmp.Dispose();
            }

            if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
            else graphic.SetPolygonMode(gDrawMode.Fill);

            graphic.DrawTriangles(points, indices);
           
            if (obj.WireFrameVisible && !obj.IsWireFrameMode)
            {
                graphic.SetPolygonMode(gDrawMode.Wireframe);
                _color = ConvertColor(obj.WireFrameColor);
                _color.w = obj.WireFrameAlpha;
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].color = _color;
                }
                graphic.DrawTriangles(points, indices);
            }

            graphic.PopMatrix();
            graphic.DisableTexture();

            points = null;
            indices = null;
        }

        public void DrawTrianglesObj(TriangleObj obj)
        {
            if (!obj.Visible) return;
            if (obj.points.Count < 2) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲            

            graphic.PushMatrix();
            if (obj.Alpha < 1) { graphic.SetDepthWriteEnable(false); graphic.SetDepthTestEnable(false); }

            DrawTriangles(obj);
            graphic.SetDepthWriteEnable(true);
            graphic.PopMatrix();

            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }
        private void DrawShape(Symbol3D obj)
        {
            Vector32 p1;
            vec4 _color = new vec4();

            foreach (TriangleObj tri in obj.Faces)
            {
                if (!tri.Visible) continue;

                _color = tri.color;
                _color.w = obj.Alpha;
                
                Vertex3D[] points = new Vertex3D[tri.points.Count];
                for (int i = 0; i < points.Length; i++)
                {
                    p1 = obj.TransformedPoint(tri.points[i]);
                    p1 = toWorldVector(obj, p1);
                    p1 = CDataModel.ToModelVector(p1);
                    points[i] = graphic.CreatePoint(p1);
                    points[i].color = _color; //object颜色
                }

                if (!tri.IsUniformColor && tri.colors.Count > 0)//顶点颜色不统一
                {
                    for (int i = 0; i < tri.colors.Count; i++)
                    {
                        _color = tri.colors[i];
                        _color.w = tri.Alpha;
                        points[i].color = _color;
                    }
                }

                //try to load texture
                Bitmap bmp = null;
                if (tri.textureStruct.IsValidate())
                {
                    bmp = LoadTexture(tri.textureStruct);
                    if (bmp == null) AddToMessage("load texture failed.\n" + tri.textureStruct.TextureFile);
                    else
                    {
                        if (tri.texCoords.Count == 0)//计算纹理坐标
                        {
                            float x, y;
                            for (int i = 0; i < tri.points.Count; i++)
                            {
                                tri.GetTextureCoord(tri.points[i], out x, out y);
                                points[i].SetTexcoord(x, y);
                            }
                        }
                        else//使用已有纹理坐标
                        {
                            for (int i = 0; i < tri.points.Count; i++)
                                points[i].SetTexcoord(tri.texCoords[i]);
                        }
                    }

                }//if (obj.textureImgFile.Length > 0 && obj.enbaleTexture)           

                int[] indices = new int[tri.triangles.Count * 3];
                for (int i = 0; i < tri.triangles.Count; i++)
                {
                    indices[3 * i] = tri.triangles[i].x;
                    indices[3 * i + 1] = tri.triangles[i].y;
                    indices[3 * i + 2] = tri.triangles[i].z;
                }

                //begin draw
                graphic.PushMatrix();

                if (bmp != null)
                {
                    graphic.EnableTexture(true);
                    graphic.BindTexture(bmp);
                    bmp.Dispose();
                }

                if (tri.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
                else graphic.SetPolygonMode(gDrawMode.Fill);


                graphic.DrawTriangle(points, indices);

                graphic.PopMatrix();

                graphic.DisableTexture();

                points = null;
                indices = null;
            }
        }
        public void DrawShapeObj(Symbol3D obj)
        {             
            if ( !obj.Visible ) return;
            if ( !obj.Create() ) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲            
            DrawShape(obj);
            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }
        public void DrawMeshObj(CMesh obj)
        {           
            if (!obj.Visible) return;
            
            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲

            if (obj.ShowMesh) 
            {
               // if (obj.Boundaries.Count > 0)
                    DrawMeshGridWithBlanked(obj);
               // else DrawMeshGrid(obj);
            }
            if (obj.ShowContour) DrawMeshContourLines(obj);
            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }
        public void DrawMeshGrid(CMesh obj)
        {
            if (obj.nRow < 2 || obj.nCol < 2) return;
            if (obj.pData == null) return;            
            Vertex3D[] points = null;
            int[] indices = null;

            try
            {
                points = new Vertex3D[obj.nRow * obj.nCol];
                indices = new int[(obj.nRow - 1) * (obj.nCol - 1) * 6];
            }
            catch (Exception e)
            {
                AddToMessage("out of memory while rendering meshes: " + obj.Name + ".");
                return;
            }
            //begin draw
            graphic.PushMatrix();

            if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
            else graphic.SetPolygonMode(gDrawMode.Fill);

            Vector64 p1;
            vec4 _color = ConvertColor(obj.ObjColor);
            _color.w = obj.Alpha;
            Vertex3D p;

            bool enableTexture = false;
            if (obj.textureStruct.IsValidate())
            {
                Bitmap bmp = LoadTexture(obj.textureStruct);
                if (bmp == null) AddToMessage("load texture failed.\n" + obj.textureStruct.TextureFile);
                else
                {                    
                    enableTexture = true;
                    graphic.EnableTexture(true);
                    graphic.BindTexture(bmp);
                    bmp.Dispose();
                }
            }
            vec2 tex = new vec2();
            for (int i = 0; i < obj.nRow; i++)
            {
                for (int j = 0; j < obj.nCol; j++)
                {
                    p1 = obj.pData[i * obj.nCol + j];

                    if (double.IsNaN(p1.V) || obj.IsBlanked(p1)) _color.w = 0;
                    else if (obj.EnableColorLevel) _color = ConvertColor(obj.GetColor(p1.v));
                    else 
                    {
                        ConvertColor(obj.ObjColor);
                        _color.w = obj.Alpha;
                    }                   


                    if (obj.IsFlat) p1.Z = obj.ZOffset;

                    if (enableTexture) tex = obj.GetTextureCoord(p1);

                    p1 = obj.TransformedPoint(p1);
                    p1 = toWorldVector(obj, p1);
                    p1 = CDataModel.ToModelVector(p1);

                    p = graphic.CreatePoint(p1);
                    p.color = _color; //object颜色

                    if (enableTexture)
                    {
                        p.SetTexcoord(tex);
                        //p.SetTexcoord((float)j / (float)obj.nCol, (float)i / (float)obj.nRow);
                    }
                    points[i * obj.nCol + j] = p;
                }
            }

            //  -1---2--- 
            //  -3---4---
            long k = 0;
            int id1, id2, id3, id4;
            for (int i = 0; i < obj.nRow - 1; i++)
            {
                for (int j = 0; j < obj.nCol - 1; j++)
                {
                    id1 = i * obj.nCol + j;
                    id2 = id1 + 1;
                    id3 = id1 + obj.nCol;
                    id4 = id2 + obj.nCol;

                    if (points[id1].color.w > 0f && points[id2].color.w>0f && points[id3].color.w>0f)
                    {
                        indices[k++] = id1;
                        indices[k++] = id2;
                        indices[k++] = id3;
                    }
                    if (points[id2].color.w > 0f && points[id3].color.w > 0f && points[id4].color.w > 0f)
                    {
                        indices[k++] = id4;
                        indices[k++] = id3;
                        indices[k++] = id2;
                    }
                }
            }

            graphic.DrawTriangle(points, indices);         
            
            graphic.EnableTexture(false);

            graphic.PopMatrix();

            points = null;
            indices = null;

        }
        public void DrawMeshGridWithBlanked(CMesh obj)
        {
            TriangleObj tri = obj.toBlankedTriangleObj();            
            DrawTriangles(tri);
            tri.Clear();
        }
        #region CubeTest
        public int m_TestValue = 1;
        C3DGridData dataTest = new C3DGridData();
        MarchingCubesExt marchingTest = new MarchingCubesExt();
        public void CreateTest()
        {  /*
            dataTest.minx = 0;
            dataTest.maxx = 1;
            dataTest.miny = 0;
            dataTest.maxy = 1;
            dataTest.minz = 0;
            dataTest.maxz = 1;
            dataTest.minv = 0;
            dataTest.maxv = 1;
            dataTest.xNum = 2;
            dataTest.yNum = 2;
            dataTest.zNum = 2;
            dataTest.m_ColorScale = new CColorScale();
            dataTest.pGridData = new float[dataTest.xNum * dataTest.yNum * dataTest.zNum];
            dataTest.m_ColorScale.SetValueRange(dataTest.minv, dataTest.maxv);
            dataTest.pgridShowTable = new byte[]
            {
                0,0,0,0,
                0,0,0,0,
                0,0,0,0,  //z0
                0,0,0,0,
                0,0,0,0,
                0,0,1,0,  //z1
                1,0,0,0,
                0,0,0,0,
                0,0,0,0,  //z2                
                0,1,0,0,
                0,0,0,0,
                0,0,0,0,
                0,0,0,0,
                0,0,0,0,
                0,0,0,0,
                0,0,0,0,
            };

            int ix = 0;
            int iy = 0;
            int iz = 0;
            
            marchingTest.SetData(dataTest);

            bool[] showstate = new bool[8];
            for (int i = 0; i < 8; i++) showstate[i] = false;
            if ((m_TestValue & 1) > 0) showstate[0] = true;
            if ((m_TestValue & 2) > 0) showstate[1] = true;
            if ((m_TestValue & 4) > 0) showstate[2] = true;
            if ((m_TestValue & 8) > 0) showstate[3] = true;
            if ((m_TestValue & 16) > 0) showstate[4] = true;
            if ((m_TestValue & 32) > 0) showstate[5] = true;
            if ((m_TestValue & 64) > 0) showstate[6] = true;
            if ((m_TestValue & 128) > 0) showstate[7] = true;

            int id;
            for (int i = 0; i < 8; i++)
            {
                id = marchingTest.GetVerticIndex(ix, iy, iz, i);
                marchingTest.p3DData.pGridData[id] = 0;
                if (showstate[i])
                {                    
                    marchingTest.p3DData.pgridShowTable[id] = 1;
                    marchingTest.p3DData.pGridData[id] = 1;
                }
            }
            marchingTest.pClosedValues1.Add(0.5);
            marchingTest.pClosedValues2.Add(0.6);
            marchingTest.ExtractTriangleFromCubeTest(0, 0, 0, m_TestValue);
            */
        }
        private void DrawTest()
        {            
            Vertex3D[] p = new Vertex3D[8];
            p[0] = new Vertex3D(0, 0, 0);
            p[1] = new Vertex3D(1, 0, 0);
            p[2] = new Vertex3D(1, 0, 1);
            p[3] = new Vertex3D(0, 0, 1);
            p[4] = new Vertex3D(0, 1, 0);
            p[5] = new Vertex3D(1, 1, 0);
            p[6] = new Vertex3D(1, 1, 1);
            p[7] = new Vertex3D(0, 1, 1);

            graphic.PushMatrix();
            
           // graphic.SetPolygonMode( VkPolygonMode.VK_POLYGON_MODE_LINE);
            graphic.SetColor(ConvertColor(Color.Green));
           
            graphic.DrawLine(p[0], p[1]);
            graphic.DrawLine(p[1], p[2]);
            graphic.DrawLine(p[2], p[3]);
            graphic.DrawLine(p[0], p[3]);
            graphic.DrawLine(p[4], p[5]);
            graphic.DrawLine(p[5], p[6]);
            graphic.DrawLine(p[6], p[7]);
            graphic.DrawLine(p[4], p[7]);
            graphic.DrawLine(p[0], p[4]);
            graphic.DrawLine(p[1], p[5]);
            graphic.DrawLine(p[2], p[6]);
            graphic.DrawLine(p[3], p[7]);            
           
            graphic.SetColor(ConvertColor(Color.Red));

            CSphere sp = new CSphere(0,0,0);
            sp.Create(0.02, 0.001);            
            int type = m_TestValue;
            if ((type & 1) > 0) graphic.DrawShpere(p[0],sp);
            if ((type & 2) > 0) graphic.DrawShpere(p[1], sp);
            if ((type & 4) > 0) graphic.DrawShpere(p[2], sp);
            if ((type & 8) > 0) graphic.DrawShpere(p[3], sp);
            if ((type & 16) > 0) graphic.DrawShpere(p[4], sp);
            if ((type & 32) > 0) graphic.DrawShpere(p[5], sp);
            if ((type & 64) > 0) graphic.DrawShpere(p[6], sp);
            if ((type & 128) > 0) graphic.DrawShpere(p[7], sp);
            
            graphic.PopMatrix();
           
            CISOSurfaceExt sf = marchingTest.pISOSurfaceExt;
            if (sf.pTriangleIndex.Count < 1) return;

            TriangleObj obj = new TriangleObj();
            for (int j = 0; j < sf.pCoordArray.Count; j++)
            {
                obj.points.Add(new Vector32(sf.pCoordArray[j].x, sf.pCoordArray[j].y, sf.pCoordArray[j].z));
            }
            for (int j = 0; j < sf.pTriangleIndex.Count/3; j++)
            {
                obj.triangles.Add( new Int32XYZ(sf.pTriangleIndex[3*j], sf.pTriangleIndex[3 * j+1], sf.pTriangleIndex[3 * j+2]) );
            }

            obj.SmoothTriangle(0.001);
            graphic.SetColor(0.1f,0.8f,0.8f);
            //graphic.DrawTriangles(obj);
        }
        #endregion CubeTest

        public void DrawMeshLinesObj(LineMesh obj)
        {           
            if ( !obj.Visible ) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲
            
            foreach( C3DLine line in obj.lines )            
            {
                line.Alpha = obj.Alpha;
                if (line.Visible) 
                {
                    DrawLineObj(line);
                    obj.AddRenderingBuffer(line.RenderingBuffers);//添加到对象缓冲
                }
            }
        }

        public void DrawLineObj(C3DLine obj)
        {
            if (!obj.Visible) return;
            if (obj.points.Count < 2) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲
            DrawLine(obj);
            if ( (obj.Arrow.arrowStyle ==  ArrowStyle.Right|| 
                  obj.Arrow.arrowStyle == ArrowStyle.Both) && 
                  obj.points.Count > 2 )
            {
                Vector64 start = obj.points[obj.points.Count - 2];
                Vector64 end = obj.points[obj.points.Count - 1];                
                if (Math.Abs(obj.Arrow.drawExtentScale - 0) > 1e-8)
                {  //前后各增加一个点，作为方向
                    var calculator = new CurveExtensionCalculator();
                    calculator.CalculateExtensionPoints(obj.points, out var linearFront, out var linearBack, obj.Arrow.drawExtentScale);
                    start = obj.points[obj.points.Count - 1];
                    end = linearBack;                    
                }
                DrawLineArrow(start,end,obj); 
            }
            if ( (obj.Arrow.arrowStyle == ArrowStyle.Left ||
                  obj.Arrow.arrowStyle == ArrowStyle.Both) &&                
                  obj.points.Count > 2)
            {
                Vector64 start = obj.points[1];
                Vector64 end = obj.points[0];
                if (Math.Abs(obj.Arrow.drawExtentScale - 0) > 1e-8)
                {  //前后各增加一个点，作为方向
                    var calculator = new CurveExtensionCalculator();
                    calculator.CalculateExtensionPoints(obj.points, out var linearFront, out var linearBack, obj.Arrow.drawExtentScale);
                    start = obj.points[0];
                    end = linearFront;
                }
                DrawLineArrow(start, end, obj);
            }
            
            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }

        void DrawLineArrow(Vector64 start, Vector64 end, C3DLine obj)
        {
            obj.Arrow.Start = start;
            obj.Arrow.End = end;
            TriangleObj tri = obj.Arrow.GenerateSmoothArrow();
            tri.color = ConvertColor(obj.Arrow.Color);
            tri.color.w = obj.Alpha;
            DrawTriangles(tri);
            tri.Clear();  
        }
        private void DrawLine(C3DLine obj)
        {
            Vertex3D[] points = new Vertex3D[obj.points.Count];
            if (points == null) return;
            Vector32 p1;
            vec4 _color = ConvertColor(obj.Color);

            for (int i = 0; i < points.Length; i++)
            {
                p1 = obj.points[i];
                p1 = obj.TransformedPoint(p1);
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);
                points[i] = graphic.CreatePoint(p1);
                if (obj.EnableColorLevel)
                {
                    _color = ConvertColor(obj.GetColor(p1.V));
                }
                _color.w = obj.Alpha;
                points[i].SetColor(_color);
            }//for (int i = 0; i < points.Length; i++)

            //begin draw
            graphic.PushMatrix();

            graphic.SetLineWidth(obj.lineWidth);

            graphic.DrawLines(points, obj.Closed);
            
            graphic.PopMatrix();

            points = null;

        }        

        //绘制地层侧面
        void DrawGeoMeshesFace(GeoLayerMeshes obj, GeoMesh top, GeoMesh bottom)
        {
            int count = top.Boundaries.Count;
            if (count < 1) return;
            graphic.PushMatrix();

            vec4 _color = new vec4();
            List<Vertex3D> points = new List<Vertex3D>();
            Vector64 p1, p2;
            Vertex3D p;
            for (int i=0;i< count;i++)
            {
                 p1 = top.Boundaries[i];
                _color = ConvertColor(top.ObjColor);
                if (top.EnableColorLevel)
                {
                    _color = ConvertColor(top.GetColor(p1.V));
                }
                p1 = obj.TransformedPoint(p1);
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);
                p = graphic.CreatePoint(p1);
                
                _color.w = obj.Alpha;
                p.SetColor(_color);
                points.Add(p);

                p2 = bottom.Boundaries[i];
                _color = ConvertColor(bottom.ObjColor);
                if (bottom.EnableColorLevel)
                {
                    _color = ConvertColor(bottom.GetColor(p2.V));
                }

                p2 = obj.TransformedPoint(p2);
                p2 = toWorldVector(obj, p2);
                p2 = CDataModel.ToModelVector(p2);
                p = graphic.CreatePoint(p2);                
                _color.w = obj.Alpha;
                p.SetColor(_color);
                points.Add(p);
            }

            bool enableTexture = false;
            TextureStruct tex = top.textureStruct;
            if (tex.IsValidate())
            {
                Bitmap bmp = LoadTexture(tex);
                if (bmp == null) AddToMessage("load texture failed.\n" + tex.TextureFile);
                else
                {
                    enableTexture = true;
                    graphic.EnableTexture(true);
                    graphic.BindTexture(bmp);
                    bmp.Dispose();
                }
            }

            if ( enableTexture )
            {
                for(int i = 0; i < count; i++ )
                {
                    Vertex3D v1 = points[2 * i];
                    Vertex3D v2 = points[2 * i+1];
                    v1.SetTexcoord((float)i / (count - 1), 0);
                    v2.SetTexcoord((float)i / (count - 1), 1);
                    points[2 * i] = v1;
                    points[2 * i+1] = v2;
                }
            }
            
            List<int> indices = new List<int>();
            int k = 0;
            int id1, id2, id3, id4;
            for (int i = 0; i < count; i++)
            {
                id1 = 2 * i;
                id2 = 2*i + 1;
                if (i == count - 1)
                {
                    id3 = 0;
                    id4 = 1;
                }
                else
                {
                    id3 = id1 + 2;
                    id4 = id1 + 3;
                }
                if (top.Clockwise == ClockDirection.Clockwise)
                {
                    indices.Add(id1);
                    indices.Add(id3);
                    indices.Add(id2);
                    indices.Add(id2);
                    indices.Add(id3);
                    indices.Add(id4);
                }
                else
                {
                    indices.Add(id1);
                    indices.Add(id2);
                    indices.Add(id3);
                    indices.Add(id4);
                    indices.Add(id3);
                    indices.Add(id2);
                }
            }
            
            if (top.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
            else graphic.SetPolygonMode(gDrawMode.Fill);

            graphic.DrawTriangle(points.ToArray(),indices.ToArray());
            graphic.DisableTexture();

            points.Clear();
            indices.Clear();

            graphic.PopMatrix();

        }

        //绘制地层
        public void DrawGeoMeshesObj(GeoLayerMeshes obj)
        {
            if (!obj.Visible) return;
            if (obj.Count < 1) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲
            obj.ClearRenderingBuffers();

            DrawGeoMeshes(obj);
            
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }
        private void DrawGeoMeshes(GeoLayerMeshes obj)
        {
            //默认地层按从上到下排列
            int row = obj.nRow; //行和列一致
            int col = obj.nCol;
            //绘制正面
            for (int i = 0; i < obj.Count; i++)
            {
                CMesh mesh = obj[i];
                if (mesh.ShowMesh)
                {
                    if (mesh.CoordIntersections.Count < 1) DrawMeshGrid(mesh);
                    else DrawMeshGridWithBlanked(mesh);
                }
                if (mesh.ShowContour) DrawMeshContourLines(mesh);
            }
            //绘制侧面
            for (int i = 0; i < obj.Count - 1; i++)
            {
                GeoMesh top = obj[i];
                GeoMesh bottom = obj[i + 1];
                // DrawGeoMeshesFace(obj, top, bottom);
                TriangleObj tri = top.toBlankedTriangleObj(bottom);
                DrawTriangles(tri);
                tri.Clear();

            }//for( int m = 0; m < obj.Count; m++ )             
        }
        private void Draw2DPolygonsObj(C2DPolygons obj, PolygonSlicer slicer = null)
        {
            if ( !obj.Visible ) return;

            ClearObjectDrawBuffer(obj);
            
            PolygonSlicer slicer1 = slicer;
            if (slicer == null) slicer1 = (PolygonSlicer)obj.Parent;
            foreach (Polygon2D poly in obj.Polygons)
            {
                if ( !poly.Visible ) continue;
                if ( poly.points.Count < 2 ) continue;
                DrawPolygon2DObj(poly, slicer);
                poly.Parent = obj;
                obj.AddRenderingBuffer(poly.RenderingBuffers);

            }//foreach (Polygon2D poly in obj.polygons.Polygons)           
        }
        public void DrawPolygonSlicer(PolygonSlicer obj)
        {
            if (!obj.Visible) return;

            ClearObjectDrawBuffer(obj);
            
            DrawPolygonsBackImage(obj);

            Draw2DPolygonsObj(obj.polygons,obj);
            Draw2DPolygonsObj(obj.tracedGeoObjects,obj);

            obj.AddRenderingBuffer(obj.polygons.RenderingBuffers);
            obj.AddRenderingBuffer(obj.tracedGeoObjects.RenderingBuffers);
        }

        void DrawPolygonsBackImage(PolygonSlicer obj)
        {
            if (obj == null) return;
            if ( obj.backImages.Count < 1) return;
            if ( !obj.polygons.Visible ) return;
            if ( !obj.ShowBackgroundImage ) return;

            double x1, y1, x2, y2;
            Vector64 p1;

            Vector64[] corners = new Vector64[4];
            Vertex3D[] points = new Vertex3D[4];
            int[] indices = new int[6];

            graphic.ClearModelKeyBuffers();

            foreach (ImageStruct im  in obj.backImages)
            {
                //0--x1,y1    1--x2,y1
                //2--x1,y2    3--x2,y2
                x1 = im.rect.X1;
                y1 = im.rect.Y1;
                x2 = im.rect.X2;
                y2 = im.rect.Y2;                
                corners[0] = new Vector64(x1, y1, 0);
                corners[1] = new Vector64(x2, y1, 0);
                corners[2] = new Vector64(x2, y2, 0);
                corners[3] = new Vector64(x1, y2, 0);
                for (int i = 0;i < corners.Length; i++)
                {
                    p1 = corners[i];
                    p1 = obj.toTracedPoint(p1);
                    p1 = obj.TransformedPoint(p1);
                    p1 = toWorldVector(obj, p1);
                    p1 = CDataModel.ToModelVector(p1);
                    points[i] = CreateVertex(p1);
                    points[i].color = new vec4(1,1,1,1);
                }
                // P3(0,1)   P2(1,1)
                // P0(0,0)   P1(1,0)                
                points[0].SetTexcoord(0, 1);
                points[1].SetTexcoord(1, 1);
                points[2].SetTexcoord(1, 0);                                
                points[3].SetTexcoord(0, 0);
                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
                indices[3] = 2;
                indices[4] = 3;
                indices[5] = 0;

                graphic.PushMatrix();
                graphic.EnableTexture(true);
                
                Bitmap bmp = new Bitmap(im.bmp);

                if (obj.textureStruct.Enable)
                    obj.textureStruct.MakeTransparent(bmp);

                graphic.BindTexture( bmp );
                bmp.Dispose();

                graphic.DrawTriangle(points,indices);               
                graphic.PopMatrix();
                graphic.EnableTexture(false);
            }
            
            corners = null;
            points = null;

            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲

        }
        /// <summary>
        /// //绘制2D Polygon
        /// </summary>
        /// <param name="obj">Polygon2d</param>
        /// <param name="slicer">父节点--3D切片</param>
        public void DrawPolygon2DObj(Polygon2D obj, PolygonSlicer slicer)
        {
            if (!obj.Visible) return;
            if (obj.points.Count < 2) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲
            
            DrawPolygon2DFilled(obj,slicer);
            DrawPolygon2DOutLine(obj,slicer);

            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }

        private void DrawPolygon2DOutLine(Polygon2D obj, PolygonSlicer slicer)
        {
            if (!obj.Visible || obj.points.Count < 2) return;

            Vector32 p1;
            vec4 _color;
            Vertex3D[] points;
            float lineWidth = 1.0f;

            _color = ConvertColor(obj.lineColor);
            _color.w = obj.Alpha;
            lineWidth = obj.lineWidth;

            if (obj.IsClosed) points = new Vertex3D[obj.points.Count + 1];
            else points = new Vertex3D[obj.points.Count];

            int i = 0;
            foreach (Vector32 p in obj.points)
            {
                p1 = p;                
                p1 = obj.TransformedPoint(p1);
                if (slicer != null) p1 = slicer.toTracedPoint(p.toVector64());
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);
                points[i] = graphic.CreatePoint(p1);
                points[i].color = _color;
                i++;
            }
            if (obj.IsClosed) points[i] = points[0];

            //begin draw
            graphic.PushMatrix();

            if (lineWidth < 1) lineWidth = 1;
            graphic.SetLineWidth(lineWidth);

            graphic.DrawLines(points, obj.IsClosed);

            graphic.PopMatrix();

            points = null;
        }
        //三角剖分的问题-3D多边形需要先投影成2D多边形
        private void DrawPolygon2DFilled(Polygon2D obj, PolygonSlicer slicer = null)
        {
            if (!obj.IsClosed || !obj.IsFill) return;
            bool IsPoly2D = obj.IsPoly2D;

            Vector32 p1;
            //if ( obj.RenderMode == RenderingUpdateMode.Redraw ||
            //     (slicer != null && slicer.RenderMode == RenderingUpdateMode.Redraw) )

            if (obj.NetTopologySuiteTriangulate(true) == null) 
            {
                AddToMessage("Triangulating failed of "+ obj.Name + "--" + obj.errMessage);
                return; 
            }

            for (int k = 0; k < obj.triangledObject.points.Count; k++)
            {
                p1 = obj.triangledObject.points[k];
                if (slicer != null && IsPoly2D)
                {
                    p1 = slicer.toTracedPoint(p1.toVector64());
                }
                obj.triangledObject.points[k] = p1;
            }
            obj.triangledObject.UpdateRange();

            obj.triangledObject.color = ConvertColor(obj.fillColor);
            obj.triangledObject.uniformColor = obj.fillColor;
            obj.triangledObject.IsUniformColor = true;
            obj.triangledObject.IsWireFrameMode = obj.IsWireFrameMode;
            obj.triangledObject.Alpha = obj.Alpha;
            obj.triangledObject.textureStruct = obj.textureStruct;
            obj.triangledObject.offset = obj.offset;
            obj.triangledObject.scale = obj.scale;
            obj.triangledObject.rotate = obj.rotate;

            DrawTriangles(obj.triangledObject);
        }

        void DrawArrowSymbles(ScatteredPoints obj)
        {
            if (obj.Symbol != SymbolEnum.Arrow) return;

            int Interval = obj.SymbolInterval;

            int size = Arrow2D.Size;
            double MB = 1024 * 1024;
            
            double required = obj.points.Count / MB * (size + Vertex3D.GetSize()) * 4 / Interval;
            double available = PhysicalMemory.GetAvailableMemoryMB() * 0.6;

            if (required > available)
            {
                AddToMessage("Warning : no enough memory to render all the objects.");
                return;
            }

            List<Vertex3D> points = new List<Vertex3D>();

            Vector32 p1;

            graphic.PushMatrix();

            if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
            else graphic.SetPolygonMode(gDrawMode.Fill);
                       
            graphic.BeginTriangles();

            Vector32 p;
            for (int i = 0; i < obj.Count; i += Interval)
            {
                p = obj.points[i];
                if (obj.IsHidePoint(p)) continue;

                p1 = obj.TransformedPoint(p);
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);
                
                Arrow2D arrow = new Arrow2D();
                arrow.Width = obj.xWidth;
                arrow.Create(p1.toVector64(), obj.xWidth*2, p.V);
                points.Add(CreateVertex(arrow.Start));
                points.Add(CreateVertex(arrow.End));

                TriangleObj tri = arrow.toTriangleObject();
                tri.IsUniformColor = true;
                if (!obj.IsUniformColor)tri.color = ConvertColor(obj.GetColor(p.v));
                else tri.color = ConvertColor(obj.ObjColor);

                tri.IsUniformColor = true;
                tri.Alpha = obj.Alpha;
                tri.Blend = obj.Blend;
                //tri.enbaleTexture = obj.enbaleTexture;

                graphic.TriangleMemory(tri);
            }

            graphic.EndTriangles();

            int[] indices = new int[points.Count];
            for(int i = 0 ; i < indices.Length; i++ )
            {
                indices[i] = i;
            }
            graphic.DrawLinesList(points.ToArray(), indices);
            
            graphic.PopMatrix();
            
            indices = null;
            points = null;

            if (Interval > 1) AddToMessage(obj.Name + ": not all the points are rendered,interval is " + Interval);
        }

        void DrawScatteredPointsSymbles(ScatteredPoints obj)
        {
            if (obj.Symbol == SymbolEnum.None) return;
            if (obj.Symbol == SymbolEnum.Arrow) { DrawArrowSymbles(obj);return;}

            int Interval = obj.SymbolInterval;

            int size = 0;
            double MB = 1024 * 1024;
            if (obj.Symbol == SymbolEnum.Cone) size = GridBox.Size;
            else size = obj.horizontalRounds * obj.verticalRounds * sizeof(float);

            double required = obj.points.Count / MB * (size + Vertex3D.GetSize()) * 4 / Interval ;
            double available = PhysicalMemory.GetAvailableMemoryMB() * 0.6;

            if (required > available)
            {
                AddToMessage("Warning : no enough memory to render all the objects.");
                return;
            }

            Vector32 p1;

            graphic.PushMatrix();

            if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
            else graphic.SetPolygonMode(gDrawMode.Fill);

            //try to load texture
            if (obj.textureStruct.IsValidate())
            {
                Bitmap bmp = LoadTexture(obj.textureStruct);
                if (bmp == null) AddToMessage("load texture failed.\n" + obj.textureStruct.TextureFile);
                else
                {                   
                    graphic.EnableTexture(true);
                    graphic.BindTexture(bmp);
                    bmp.Dispose();
                }                
            }            

            graphic.BeginTriangles();

            Vector32 p;
            for (int i = 0; i < obj.Count; i += Interval)
            {
                p = obj.points[i];
                if (obj.IsHidePoint(p)) continue;

                p1 = obj.TransformedPoint(p);
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);

                TriangleObj tri = null;

                double x1 = p1.x - obj.xWidth / 2.0;
                double x2 = p1.x + obj.xWidth / 2.0;
                double y1 = p1.y - obj.yWidth / 2.0;
                double y2 = p1.y + obj.yWidth / 2.0;
                double z1 = p1.z - obj.zWidth / 2.0;
                double z2 = p1.z + obj.zWidth / 2.0;

                if (obj.Symbol == SymbolEnum.Cone)
                {                    
                    Cone cone = new Cone(p1, (obj.xWidth+ obj.yWidth)/2, obj.zWidth, obj.verticalRounds, obj.horizontalRounds);
                    cone.Create();
                    tri = cone.toTriangleObject();
                    //tri.ScaledToRange(x1, y1, z1, x2, y2, z2);
                    //box.offset = obj.offset;
                    //box.rotate = obj.rotate;
                    //box.scale = obj.scale;
                    //box.Normalize();
                    
                }
                else if (obj.Symbol == SymbolEnum.Cylinder)
                {
                    CCylinderExt cy = new CCylinderExt(p1,1,3);                   
                    cy.Create();
                    tri = cy.toTriangleObject();
                    //tri.ScaledToRange(x1, y1, z1, x2, y2, z2);
                    //box.offset = obj.offset;
                    //box.rotate = obj.rotate;
                    //box.scale = obj.scale;
                    //box.Normalize();
                   
                }
                else
                {
                    Box3D box = new Box3D(p1, obj.xWidth, obj.yWidth, obj.zWidth);
                    box.Create();
                    //GridBox box = new GridBox(p1.x, p1.y, p1.z, obj.xWidth, obj.yWidth, obj.zWidth);
                    //box.offset = obj.symbolOffset;
                    //box.rotate = obj.symbolRotate;
                    //box.Normalize();
                    tri = box.toTriangleObject();
                }
                
                tri.ScaledToRange(x1, y1, z1, x2, y2, z2);
                //tri.scale.z = -1;
                tri.DoTransform();

                tri.IsUniformColor = true;
                if (!obj.IsUniformColor) tri.color = ConvertColor(obj.GetColor(p.v));
                else tri.color = ConvertColor(obj.ObjColor);

                tri.IsUniformColor = true;
                tri.Alpha = obj.Alpha;
                tri.Blend = obj.Blend;
                //tri.enbaleTexture = obj.enbaleTexture;

                graphic.TriangleMemory(tri);
            }           

            graphic.EndTriangles();            

            graphic.PopMatrix();
            if (Interval > 1) AddToMessage(obj.Name + ": not all the points are rendered,interval is " + Interval);
        }
        void DrawScatteredPointsLines(ScatteredPoints obj)
        {
            Vector32 p1,p;            
            Vertex3D[] points = new Vertex3D[obj.points.Count];
            float alpha = 1f;
            if (obj.Blend) alpha = obj.Alpha;
            vec4 color = ConvertColor(obj.ObjColor);
            color.w = alpha;

            for (int i = 0; i < obj.Count - 1; i ++)
            {
                p = obj.points[i];
                //if (obj.IsHidePoint(p)) continue;
                p1 = obj.TransformedPoint(p);
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);
                points[i] = graphic.CreatePoint(p1);

                if (obj.IsUniformColor) points[i].SetColor(color);
                else 
                {
                    color = ConvertColor(obj.GetColor(p.v));
                    color.w = alpha;
                    points[i].SetColor(color); 
                }
            }

            //begin draw
            graphic.PushMatrix();

            graphic.SetLineWidth(obj.lineWidth);

            graphic.DrawLines(points, obj.lineClosed);

            graphic.PopMatrix();

            points = null;

        }
        void DrawScatteredPointsLabels(ScatteredPoints obj)
        {
            if ( obj.Labels.Count < 1 ) return;
            Vector32 p;
            float alpha = 1f;
            if (obj.Blend) alpha = obj.Alpha;
            vec4 color = ConvertColor(obj.ObjColor);
            color.w = alpha;

            TexturedText text;
            for (int i = 0; i < obj.Labels.Count; i += obj.LabelInterval)
            {
                p = obj.points[i];
                if (obj.IsHidePoint(p)) continue;
                if ( !obj.Labels[i].Visible) continue;
                if ( obj.Labels[i].TextLength < 1) continue;
                //  p1 = obj.TransformedPoint(p);
                //  p1 = toWorldVector(obj, p1);
                //  p1 = CDataModel.ToModelVector(p1);
                text = obj.Labels[i];
                if( obj.RenderMode == RenderingUpdateMode.Redraw )text.Create();
                if (obj.IsUniformStyle) 
                { 
                    text.textStyle = obj.textStyle.Copy();
                }
                DrawText(text);
            }            
        }

        public void DrawScatteredPoints(ScatteredPoints obj)
        {
            if (!obj.Visible) return;
            if (obj.points.Count < 1) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲

            if ( obj.ShowSymbol) DrawScatteredPointsSymbles(obj);
            if( obj.ShowLines ) DrawScatteredPointsLines(obj);
            if( obj.ShowLabel )DrawScatteredPointsLabels(obj);

            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }
        void DrawBoreHoleBaseLine(CBorehole obj)
        {
            if ( !obj.ShowBaseLine || obj.Baseline.Count < 1 ) return;
            
            Vector64 p;
            Vertex3D[] points = new Vertex3D[obj.Baseline.Count];

            graphic.PushMatrix();
            graphic.SetLineWidth(obj.BaseLineWidth);
            vec4 color = ConvertColor(obj.BaseLineColor);
            for (int i = 0; i < obj.Baseline.Count; i++)
            {
                p = obj.Baseline[i];
                p = obj.TransformedPoint(p);
                p = toWorldVector(obj, p);
                p = CDataModel.ToModelVector(p);
                points[i] = CreateVertex(p);
                points[i].color = color;
            }
            graphic.DrawLines(points, false);
            graphic.PopMatrix();
            points = null;
        }

        void DrawBoreHoleCylinder(CBorehole obj)
        {
            if ( !obj.ShowCylinder ) return;
            TriangleObj tri = obj.cylinderTriangleObj;
            if ( tri.points.Count < 1 || obj.UpdateCylinder)
            {
                tri = obj.CreateCylinder(CDataModel.IsGeoCoordinateSystem);
            }            
            tri.uniformColor = obj.CylinderColor;
            tri.IsUniformColor = true;
            tri.Alpha = obj.Alpha;
            tri.textureStruct = obj.textureStruct;
            tri.IsWireFrameMode = obj.IsWireFrameMode;
            tri.CopyHeaderFrom(obj);

            DrawTriangles(tri);            
        }
        /// <summary>
        /// 绘制钻孔地层数据
        /// </summary>
        /// <param name="obj"></param>
        void DrawBoreHoleStratum(StratumDatas strata)
        {
            if ( !strata.Visible ) return;

            int i = 0;
            CBorehole obj = strata.Parent as CBorehole;
            foreach (StratumData layer in obj.Stratums.Stratums)
            {
                if( layer.Visible )
                {
                    TriangleObj tri = obj.CreateStratumTriangles(layer, CDataModel.IsGeoCoordinateSystem);
                    DrawTriangles(tri);                    
                }
                i++;
            }
        }
        
        void DrawBoreHoleCurves(BoreholeCurves curves)
        {
            if (!curves.Visible) return;

            BoreholeCurve cv;            
            double z, v, v0, rad;
            Vector64 p;
            
            CBorehole obj = curves.Parent as CBorehole;
            double angle = Math.PI / obj.Curves.Count;
            
            List<Vector64> traces = new List<Vector64>(obj.Baseline);
            traces.Sort((a, b) => { return a.Z.CompareTo(b.Z); }); //升序排列

            for (int i = 0; i < obj.Curves.Count; i++)
            {
                cv = obj.Curves[i];
                v0 = (cv.minValue + cv.maxValue) / 2.0;

                Vertex3D[] points = new Vertex3D[cv.Points.Count];

                graphic.PushMatrix();
                graphic.SetLineWidth(cv.LineWidth);

                for (int j = 0; j < cv.Points.Count; j++)
                {
                    z = obj.Position.Z - cv.Points[j].X; //深度值
                    v = cv.Points[j].Y; //井曲线值

                    //基准线上的位置p
                    p = obj.GetPositionFromBaseline(z, traces);
                    p.z = z;

                    if (v == cv.nullValue)
                    {                        
                    }
                    else
                    {
                        rad = cv.Radius * 2 * (v - v0) / (cv.maxValue - cv.minValue);
                        p.X = p.X + rad * Math.Sin(angle * i);
                        p.Y = p.Y + rad * Math.Cos(angle * i);
                    }
                    p = obj.TransformedPoint(p);
                    p = toWorldVector(obj, p);
                    p = CDataModel.ToModelVector(p);
                    points[j] = CreateVertex(p);

                    if (cv.EnableColorScale)
                        points[j].color = ConvertColor(cv.ColorScale.GetColor(v));
                    else points[j].color = ConvertColor(cv.Color);
                }//for (int j = 0; j < cv.Points.Count; j++ )

                graphic.DrawLines(points, false);
                graphic.PopMatrix();
                cv.RenderMode =  RenderingUpdateMode.None;
                obj.Curves[i] = cv;
            }//for (int i = 0; i < obj.Curves.Count; i++)
            
        }
        public bool DrawBoreHoleObj(CBorehole obj)
        {
            if ( !obj.Visible ) return true;
            
            Cursor = Cursors.WaitCursor;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲
            
            obj.CreateBaseLine(CDataModel.IsGeoCoordinateSystem);

            obj.Curves.Parent = obj;
            obj.Stratums.Parent = obj;
            DrawBoreHoleBaseLine(obj);
            DrawBoreHoleCurves( obj.Curves );            
            DrawBoreHoleStratum( obj.Stratums );

            if (obj.ShowCylinder) // 显示柱体
            {
               DrawBoreHoleCylinder(obj);
            }
            
            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
            obj.RenderMode = RenderingUpdateMode.None;
            Cursor = Cursors.Default;

            return true;
        }
        public bool DrawBoreHolesObj(CBoreholes obj)
        {
            if ( !obj.Visible ) return true;

            ClearObjectDrawBuffer(obj);
            
            foreach (CBorehole bh in obj.pData)
            {
                DrawBoreHoleObj(bh);                
                bh.UpdateNeeded = false;       
                //添加子对象的绘制ID
                obj.AddRenderingBuffer(bh.RenderingBuffers);                
            }            

            return true;
        }

        public bool DrawPly(PlyFile ply)
        {
            if (!ply.Visible) return false;
            if (ply.vertno < 3) return false;
            
            int vertno = ply.vertno;
            int normal_no = ply.normal_no;
            int color_no = ply.color_no;
            int faceno = ply.faceno;
            int uv_no = ply.uv_no;
            double minx = ply.minx;
            double miny = ply.miny;
            double minz = ply.minz;
            double maxx = ply.maxx;
            double maxy = ply.maxy;
            double maxz = ply.maxz;
            
#pragma warning disable CS0168 // 声明了变量“z”，但从未使用过
            float x, y, z;
#pragma warning restore CS0168 // 声明了变量“z”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“b”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“a”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“g”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“r”，但从未使用过
            float r,g,b,a;
#pragma warning restore CS0168 // 声明了变量“r”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“g”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“a”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“b”，但从未使用过

            //maximum triangles once 200000
            int maxtri = 300000;
            int seg = faceno / maxtri;
            int no = seg;
            int i1,id = 0;
            Vector32 p;

            graphic.PushMatrix();

            bool texture = false;
            graphic.EnableTexture(false);
            if (ply.textureStruct.IsValidate())
            {
                Bitmap bmp = LoadTexture(ply.textureStruct);
                if (bmp == null) AddToMessage("load texture failed.\n" + ply.textureStruct.TextureFile);
                else
                { 
                    graphic.EnableTexture(true);
                    graphic.BindTexture(bmp);
                    bmp.Dispose();
                    texture = true;
                }                
            }

            for ( int k = 0; k < seg + 1; k++ )
            {
                if (k == seg) no = faceno - k * maxtri;
                else no = maxtri;
                if (no < 3) break;

                Vertex3D[] points = new Vertex3D[no];
                if (points == null) break;
                int []indices = new int[no];
                if (indices == null) break;

                for( int i = 0; i < no; i++ )
                {
                    id = k * maxtri + i;
                    i1 = ply.faces[id];
                    p = new Vector32(ply.vertices[3 * i1], ply.vertices[3 * i1 + 1], ply.vertices[3 * i1 + 2]);
                    p = ply.TransformedPoint(p);
                    p = toWorldVector(ply, p);
                    points[i] = new Vertex3D(p.x,p.y,p.z);
                    indices[i] = i;
                    if (texture && ply.uvs != null)
                    {                        
                        x = ply.uvs[2 * id];
                        y = ply.uvs[2 * id + 1];
                        points[i].SetTexcoord(x, y);
                    }
                    else
                    {
                        points[i].SetColor(graphic.curColor);
                    }
                }
                
                graphic.DrawTriangle(points, indices);

                points = null;
                indices = null;
            }           
            
            graphic.PopMatrix();

            return true;
        }
       
        private void Draw3DGrid(C3DGridData obj)
        {
            if ( !obj.Visible ) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲
            
            if (obj.meshMethod == MCMeshMethod.Cube) 
            { 
                Draw3DGridCubes(obj);
                Draw3DGridCubesOverlaps(obj);
            }
            if (obj.meshMethod == MCMeshMethod.MC) Draw3DGridMarchingCubesWithTexture(obj);
            if (obj.meshMethod == MCMeshMethod.ImprovedMC) 
            { 
                Draw3DGridMarchingCubesExt(obj);
                Draw3DGridMarchingCubesExtOverlaps(obj);
            }
            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }

        private void DrawSlicer(CSlicer obj)
        {
            if (!obj.Visible) return;
            if (obj.pData.Length < 1) return;

            ClearObjectDrawBuffer(obj);
            graphic.ClearModelKeyBuffers();//清空绘制区对象ID临时缓冲

            if (obj.ShowMesh) DrawSlicerMesh(obj, true);
            if (obj.ShowContour) DrawMeshContourLines(obj);

            obj.ClearRenderingBuffers();
            obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);//添加到对象缓冲
        }

        // i1----i2---i1(j=0)
        // |     |
        // i3----i4---i3(j=0)
        private List<int> DrawMesh4(bool b1, bool b2, bool b3, bool b4, int i1, int i2, int i3, int i4)
        {
            byte id = 0;
            if (b1) id += 1;
            if (b2) id += 2;
            if (b3) id += 4;
            if (b4) id += 8;

            List<int> lists = new List<int>();
            if (id == 7) { lists.Add(i1); lists.Add(i2); lists.Add(i3); }
            else if (id == 11) { lists.Add(i1); lists.Add(i2); lists.Add(i4); }
            else if (id == 13) { lists.Add(i1); lists.Add(i4); lists.Add(i3); }
            else if (id == 14) { lists.Add(i2); lists.Add(i4); lists.Add(i3); }
            else if (id == 15)
            {
                lists.Add(i1); lists.Add(i2); lists.Add(i3);
                lists.Add(i2); lists.Add(i4); lists.Add(i3);
            }
            return lists;
        }

        private void DrawSlicerMesh(CSlicer obj, bool upside = true)
        {            
            graphic.PushMatrix();

            if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
            else graphic.SetPolygonMode(gDrawMode.Fill);

            ////Begin draw meshes///////////////////////////////////
            Vector32 p;
            Color color;
            int Columns = obj.nCol;

            //try to load texture
            bool texture = false;
            if (obj.textureStruct.IsValidate())
            {
                Bitmap bmp = LoadTexture(obj.textureStruct);
                if (bmp == null) AddToMessage("load texture failed.\n" + obj.textureStruct.TextureFile);
                else
                {                    
                    graphic.EnableTexture(true);
                    graphic.BindTexture(bmp);
                    bmp.Dispose();
                    texture = true;
                }                         
            }

            float texX = 0, texY = 0;
            graphic.BeginTriangles();

            for (int i = 0; i < obj.nRow; i++)
            {
                for (int j = 0; j < obj.nCol; j++)
                {
                    p = obj.GetPoint(i, j);
                    
                    if (obj.EnableColorLevel) color = obj.GetColor(p.V);
                    else color = obj.ObjColor;
                    
                    //added 2022-9-27
                    if (obj.IsFlat) p.Z = obj.ZOffset;

                    p = obj.TransformedPoint(p);
                    p = toWorldVector(obj, p);
                    p = CDataModel.ToModelVector(p);
                    Vertex3D v = new Vertex3D(p.X, p.Y, p.Z);
                    v.SetColor(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, obj.Alpha);
                    if (texture)
                    {
                        // texX = (float)j / (obj.nCol - 1);
                        // texY = 1 - (float)i / (obj.nRow - 1);                        
                        //v.SetTexcoord(texX, texY);
                        v.SetTexcoord(obj.GetTextureCoord(j, i));
                    }                    
                    graphic.AddPoint(v);
                }
            }
            int i1, i2, i3, i4;
            bool b1, b2, b3, b4;

            int ncol = obj.nCol;
            if (obj.Closed) ncol++;

            for (int i = 0; i < obj.nRow - 1; i++)
            {
                for (int j = 0; j < ncol - 1; j++)
                {
                    // i1----i2  i1(j=0)
                    // i3----i4  i3(j=0)
                    i1 = i * obj.nCol + j;
                    i3 = i1 + obj.nCol;
                    if (j == obj.nCol - 1)
                    {
                        i2 = i * obj.nCol;
                        i4 = i2 + obj.nCol;
                    }
                    else
                    {
                        i2 = i1 + 1;
                        i4 = i3 + 1;
                    }

                    //all 3 triangle points are valid
                    b1 = !obj.IsBlanked(obj.pData[i1].v) && obj.IsValidHeight(obj.pData[i1]);
                    b2 = !obj.IsBlanked(obj.pData[i2].v) && obj.IsValidHeight(obj.pData[i2]);
                    b3 = !obj.IsBlanked(obj.pData[i3].v) && obj.IsValidHeight(obj.pData[i3]);
                    b4 = !obj.IsBlanked(obj.pData[i4].v) && obj.IsValidHeight(obj.pData[i4]);

                    //Draw Grid Mesh to Fit the Blanked Edge
                    List<int> lists = DrawMesh4(b1, b2, b3, b4, i1, i2, i3, i4);
                    for (int k = 0; k < lists.Count; k += 3)
                    {
                        graphic.AddPointIndex(lists[k]);
                        graphic.AddPointIndex(lists[k + 1]);
                        graphic.AddPointIndex(lists[k + 2]);
                    }
                    lists.Clear();
                }
            }
            graphic.EnableTexture(obj.enbaleTexture);          

            graphic.EndTriangles();
            graphic.DisableTexture();

            graphic.PopMatrix();
        }
        private void DrawMeshContourLines(CMesh obj)
        {
            C2DISOSurface sf;
            FLOAT_POINT ep;
            Vector32 p1;

            graphic.PushMatrix();

            for (int i = 0; i < obj.marchingCube.p2DIsoSurfaces.Count; i++)
            {
                graphic.SetLineWidth(obj.LineWidth);
                graphic.BeginLines();

                sf = obj.marchingCube.p2DIsoSurfaces[i];
                for (int j = 0; j < sf.pCoordArray.Count; j++)
                {
                    ep = sf.pCoordArray[j];
                    
                    //added 2022-9-27
                    if (obj.IsFlat) ep.z = obj.ZOffset;

                    p1 = obj.TransformedPoint(new Vector32(ep.x, ep.y, ep.z));
                    p1 = toWorldVector(obj, p1);
                    p1 = CDataModel.ToModelVector(p1);

                    Vertex3D v = new Vertex3D(p1.x, p1.y, p1.z);

                    if (obj.EnableColorLevel)
                        v.color = ConvertColor(obj.GetColor(sf.isoVale));
                    else v.color = ConvertColor(obj.LineColor);

                    v.color.w = obj.Alpha;

                    graphic.AddPoint(v);
                }
                for (int j = 0; j < sf.pLineIndex.Count; j++)
                {
                    graphic.AddPointIndex(sf.pLineIndex[j]);
                }              

                graphic.EndLines();
                graphic.SetLineWidth(1);
            }//for (int i=0;i<obj.marchingCube.pIsoSurfaces.Count;i++)

            graphic.PopMatrix();
        }

        // draw meshes created by MarchingCubes Method
        private void Draw3DGridMarchingCubesWithTexture(C3DGridData obj)
        {
            bool btexture = false;
            graphic.PushMatrix();
            {
                graphic.DisableTexture();

                if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
                else graphic.SetPolygonMode(gDrawMode.Fill);
                TextureStruct texture;
                int ic;
                for (int k = 0; k < obj.m_MarchCube.pIsoSurface.Count; k++)
                {
                    graphic.PushMatrix();

                    CISOSurface sf = obj.m_MarchCube.pIsoSurface[k];
                    texture = sf.texture;
                    btexture = false;
                    if (texture.bmp != null && texture.Name.Length > 0)
                        btexture = true;
                    // the value show state is false
                    ic = obj.ColorScale.GetColorIndex(sf.isoVale);
                    if (!obj.ColorScale[ic].Visible) continue;

                    graphic.EnableTexture(btexture);
                    if (btexture)
                    {
                        graphic.BindTexture(texture.bmp, texture.mode);
                    }

                    graphic.BeginTriangles();

                    float tex_x, tex_y;
                    
                    vec4 color = new vec4();
                    if(obj.EnableColorLevel)
                    {
                        color = ConvertColor(obj.GetColor(sf.isoVale));
                        color.w = obj.Alpha;
                    }
                    else
                    {
                        color = ConvertColor(obj.ObjColor);
                        color.w = obj.Alpha;
                    }

                    //transform vertices
                    for (int j = 0; j < sf.pCoordArray.Count; j++)
                    {
                        FLOAT_POINT p1 = sf.pCoordArray[j];
                        Vector32 p2 = obj.TransformedPoint(new Vector32(p1.x, p1.y, p1.z));
                        p2 = toWorldVector(obj, p2);
                        p2 = CDataModel.ToModelVector(p2);
                        Vertex3D p = new Vertex3D(p2.X, p2.Y, p2.Z);
                        p.SetColor(color);
                        if (btexture)
                        {
                            sf.GetTextureCoord(new Vector32(p1.x, p1.y, p1.z), out tex_x, out tex_y);
                            p.texCoord = new vec2(tex_x, tex_y);
                        }

                        graphic.AddPoint(p);
                    }

                    for (int j = 0; j < sf.pTriangleIndex.Count; j++)
                        graphic.AddPointIndex(sf.pTriangleIndex[j]);

                    graphic.EndTriangles();

                    graphic.PopMatrix();

                }//for (int k = 0; k < obj.m_MarchCube.pIsoSurface.Count; k++)

            }
            graphic.PopMatrix();

        }

        // draw meshes created by MarchingCubesExt Method
        private void Draw3DGridMarchingCubesExt(C3DGridData obj)
        {
            vec2 texCoord = new vec2(-1, -1);
            graphic.PushMatrix();
            {
                graphic.EnableTexture(false);

                if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
                else graphic.SetPolygonMode(gDrawMode.Fill);

                CISOSurfaceExt sf = obj.m_MarchCubeExt.pISOSurfaceExt;
                
                graphic.BeginTriangles();
                vec4 color;
                //transform vertices
                for (int j = 0; j < sf.pCoordArray.Count; j++)
                {
                    FLOAT_POINT_EXT p1 = sf.pCoordArray[j];
                    //Vector32 p2 = obj.toMatchedCoord(p1.x,p1.y,p1.z);//model matched added 2024-7
                    Vector32 p2 = obj.TransformedPoint(new Vector32(p1.x, p1.y, p1.z));
                    //p2 = obj.toTracedPoint(p2.toVector64());
                    p2 = toWorldVector(obj, p2);
                    p2 = CDataModel.ToModelVector(p2);

                    Vertex3D p = new Vertex3D(p2.X, p2.Y, p2.Z);
                    if (graphic.bEnableTexture)
                    {
                        sf.GetTextureCoord(new Vector32(p1.x, p1.y, p1.z), out texCoord.x, out texCoord.y);
                        p.texCoord = new vec2(texCoord.x, texCoord.y);
                    }
                    
                    if (obj.EnableColorLevel)
                    {
                        color = ConvertColor(sf.GetColor(p1.icolor));
                        color.w = obj.Alpha;
                        p.color = color;
                    }
                    else
                    {
                        color = ConvertColor(obj.ObjColor);
                        color.w = obj.Alpha;
                        p.color = color;
                    }

                    graphic.AddPoint(p);
                }

                for (int j = 0; j < sf.pTriangleIndex.Count; j++)
                    graphic.AddPointIndex(sf.pTriangleIndex[j]);              

                graphic.EndTriangles();
            }
            graphic.PopMatrix();
        }
        void Draw3DGridMarchingCubesExtOverlaps(C3DGridData obj)
        {
            if (obj.overlaps.Count < 1 || !obj.enableOverlap) return;

            int nx, ny,nz, id;
            int ix, iy, iz,ox,oy,oz;
            double x, y, z;

            //need change nx,ny,nz order                
            nx = obj.xNum;
            ny = obj.yNum;
            nz = obj.zNum;
            int id1, id2, id3;
            Color color;
            Vector32 p1;           
            Arrow2DOverlayObject over;
            List<Vertex3D> points = new List<Vertex3D>();
                        
            graphic.PushMatrix();

            for (int k = 0; k < obj.overlaps.Count; k++)
            {
                if (obj.overlaps[k].type != ShapeEnum.Points) continue;
                over = (Arrow2DOverlayObject)obj.overlaps[k];
                if (!over.Enable) continue;
                if (over.channel != OverlapChannel.Vector2D) continue;
                CISOSurfaceExt sf = obj.m_MarchCubeExt.pISOSurfaceExt;
                
                if (sf.pTriangleIndex.Count < 3) continue;

                bool[] rendered = new bool[over.data.Length];

                graphic.PushMatrix();

                if (over.Filled) graphic.SetPolygonMode(gDrawMode.Fill);
                else graphic.SetPolygonMode(gDrawMode.Wireframe);

                graphic.BeginTriangles();

                for (int i = 0; i < sf.pTriangleIndex.Count/3; i++)
                {                    
                    id1 = sf.pTriangleIndex[3*i];
                    id2 = sf.pTriangleIndex[3 * i+1];
                    id3 = sf.pTriangleIndex[3 * i+2];
                    
                    x = (sf.pCoordArray[id1].x + sf.pCoordArray[id2].x + sf.pCoordArray[id3].x) / 3;
                    y = (sf.pCoordArray[id1].y + sf.pCoordArray[id2].y + sf.pCoordArray[id3].y) / 3;
                    z = (sf.pCoordArray[id1].z + sf.pCoordArray[id2].z + sf.pCoordArray[id3].z) / 3;
                    
                    ix = (int)( ( x - obj.minx ) / obj.xStep);
                    iy = (int)( ( y - obj.miny ) / obj.yStep);
                    iz = (int)( ( z - obj.minz ) / obj.zStep);
                    if (ix < 0 || ix >= nx ||
                        iy < 0 || iy >= ny ||
                        iz < 0 || iz >= nz) continue;

                    id = ix + iy * nx + iz * nx*ny;
                    
                    if ( float.IsNaN(over.data[id]) ) continue;
                    if ( rendered[id] ) continue;
                    ox = ix;oy = iy;oz = iz;

                    p1 = obj.TransformedPoint(new Vector32(x, y, z));
                    
                    p1 = toWorldVector(obj, p1);
                    p1 = CDataModel.ToModelVector(p1);

                    Arrow2D arrow = new Arrow2D();
                    arrow.offset = over.offset;
                    arrow.Width = over.arrowSize;
                    arrow.Theta = over.Theta;
                    arrow.Create(p1.toVector64(), over.LineLength, over.data[id]);

                    //箭头所在位置
                    Vector32 p2 = arrow.End;
                    p2.x -= arrow.offset.x;
                    p2.y -= arrow.offset.y;
                    p2.z -= arrow.offset.z;
                    p2 = CDataModel.FromModelVector(p2);
                    p2 = obj.UnTransformedPoint(p2);
                    obj.GetIndices(p2, ref ix, ref iy, ref iz);

                    if ( obj.IsBlankedGrid(ox,oy,oz) )
                    {
                        if (!obj.IsBlankedGrid(ox, oy, oz - 1)) iz = oz - 1;
                        else iz = oz + 1;                        
                        if (obj.IsBlankedGrid(ix, iy, iz)) continue;
                        if (obj.IsBlankedGrid(ix + 1, iy, iz)) continue;
                        if (obj.IsBlankedGrid(ix, iy + 1, iz)) continue;
                    }
                    else
                    {
                        if (obj.IsBlankedGrid(ix, iy, iz)) continue;
                        if (obj.IsBlankedGrid(ix + 1, iy, iz)) continue;
                        if (obj.IsBlankedGrid(ix, iy + 1, iz)) continue;
                    }
                    
                    if (!over.EnableColorLevel) color = over.ObjColor;
                    else color = over.GetColor(over.data[id]);
                    if (over.Blend) color = Color.FromArgb((byte)(255 * over.Alpha), color);

                    TriangleObj tri = arrow.toTriangleObject();
                    tri.Alpha = over.Alpha;
                    tri.Blend = over.Blend;
                    tri.uniformColor = color;
                    tri.IsUniformColor = true;
                    graphic.TriangleMemory(tri);

                    if (over.EnableLineColor) color = over.LineColor;

                    Vertex3D vp1 = CreateVertex(arrow.Start);
                    Vertex3D vp2 = CreateVertex(arrow.End);
                    if (over.Blend) color = Color.FromArgb((byte)(255 * over.Alpha), color);
                    vp1.SetColor(color);
                    vp2.SetColor(color);

                    points.Add(vp1);
                    points.Add(vp2);

                    rendered[id] = true;

                }//for (int i = 0; i < obj.pShowIndexArray.Count; i++)

                graphic.EndTriangles();

                int[] indices = new int[points.Count];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = i;
                }

                graphic.SetLineWidth(over.LineWidth);
                graphic.DrawLinesList(points.ToArray(), indices);
                indices = null;
                points.Clear();
                rendered = null;

                graphic.PopMatrix();
            }//for (int k = 0; k < obj.overlaps.Count; k++)

            graphic.PopMatrix();            
        }
        private void Draw3DGridCubes(C3DGridData obj)
        {
            //maximum numbers one time,
            //memory may not support all points at one time
            int maxobjnum = 100000;
            int n = obj.pShowIndexArray.Count / maxobjnum;
            //------------empty voxel may cause vulkan crush------
            //------------bug fixed by jian 2019.8.26 ------------
            //  int lastnum = obj.pShowIndexArray.Count % maxobjnum;
            // if ( lastnum > 0 ) n++;
            //------------bug fixed by jian 2019.8.26 ------------
            
            int nx, ny, nz, id;
            UInt32 ix, iy, iz;
            double x, y, z;

            ColorRGBA cc = new ColorRGBA(0, 0, 0);
            //need change nx,ny,nz order                
            nx = obj.xNum;
            ny = obj.yNum;
            nz = obj.zNum;
            int nb = 0;
            if( obj.pBlankTable !=null )
            for (int i = 0; i < nx * ny * nz; i++)
            {
                if (obj.pBlankTable[i]) nb++;
            }            

            vec4 color;
            graphic.PushMatrix();
            {
                graphic.DisableTexture();

                int ni = 0;
                for (int k = 0; k < n + 1; k++)
                {
                    graphic.PushMatrix();
                    graphic.BeginBoxes();

                    if (obj.IsWireFrameMode) graphic.SetPolygonMode(gDrawMode.Wireframe);
                    else graphic.SetPolygonMode(gDrawMode.Fill);

                    //------------bug fixed by jian 2019.8.26 ------------
                    //  if ( lastnum > 0 && k < n - 1) num = lastnum;
                    //  else num = maxobjnum;
                    //------------bug fixed by jian 2019.8.26 ------------

                    for (int j = 0; j < maxobjnum; j++)
                    {                        
                        ni = k * maxobjnum + j;
                        if (ni >= obj.pShowIndexArray.Count) break;

                        ix = obj.pShowIndexArray[ni].x;
                        iy = obj.pShowIndexArray[ni].y;
                        iz = obj.pShowIndexArray[ni].z;
                        id = (int)(ix + iy * nx + iz * nx * ny);
                        x = obj.minx + obj.xStep * ix;
                        y = obj.miny + obj.yStep * iy;
                        z = obj.minz + obj.zStep * iz;
                        
                        //if ( !IsInDrawingBox(x, y, z) ) continue;

                        // if (obj.pBlankTable[id]) continue;
                        //2021-8-9修改，颜色采用smooth方式
                        if (obj.EnableColorLevel)
                        {
                            cc = obj.GetColor(obj[id]);
                            color = ConvertColor(cc);
                            color.w = obj.Alpha * color.w;
                        }
                        else
                        {
                            color = ConvertColor(obj.ObjColor);
                            color.w = obj.Alpha * color.w;
                        }
                        
                        if (obj.overlaps.Count > 0 && obj.enableOverlap)
                        {
                            foreach(COverlayObject over in obj.overlaps)
                            {
                                if (!over.Enable) continue;
                                if( over.channel == OverlapChannel.Alpha)
                                    color.w = over.data[id];
                            }
                        }
                        
                        graphic.SetColor(color);
                          
                        
                        GridBox box = new GridBox();
                        box.Create(x, y, z, obj.xStep, obj.yStep, obj.zStep);
                        
                        for (int l = 0; l < 8; l++)
                        {                            
                            var p = box.points[l];
                            var p1 = new Vector64(p.x,p.y,p.z); 
                            p1 = obj.TransformedPoint(p1);
                            //p1 = obj.toMatchedCoord(p1);//model matched added 2024-7
                            //p1 = obj.toTracedPoint(p1);
                            p1 = toWorldVector(obj, p1);
                            p1 = CDataModel.ToModelVector(p1);
                            box.points[l] = box.toPoint(p1);
                        }
                        //      |(y)
                        //      p3--------p2 
                        //      |         |
                        //   p7 |     p6  |
                        //   |  /p0---|---p1--->(x)
                        //   | /      | / 
                        // p4|/-------p5--->east 
                        //   / (z)
                        bool border1 = false;
                        bool border2 = false;
                        bool border5 = false;
                        if (ix >= obj.xNum - 1)
                        {
                            box.FacesEnabled(false);
                            box.FacesEnabled(2, true);
                            border2 = true;
                        }
                        if (iy >= obj.yNum - 1)
                        {
                            box.FacesEnabled(false);
                            box.FacesEnabled(1, true);
                            border1 = true;
                        }
                        if (iz >= obj.zNum - 1)
                        {
                            box.FacesEnabled(false);
                            box.FacesEnabled(5, true);
                            border5 = true;
                        }

                        if (box.faces[0]) box.faces[0] = !obj.IsNeedShow(ix, iy + 1, iz); //up
                        if (box.faces[1] && !border1) box.faces[1] = !obj.IsNeedShow(ix, iy - 1, iz); //down
                        if (box.faces[2] && !border2) box.faces[2] = !obj.IsNeedShow(ix - 1, iy, iz); //left
                        if (box.faces[3]) box.faces[3] = !obj.IsNeedShow(ix + 1, iy, iz); //right
                        if (box.faces[4]) box.faces[4] = !obj.IsNeedShow(ix, iy, iz + 1); //front
                        if (box.faces[5] && !border5) box.faces[5] = !obj.IsNeedShow(ix, iy, iz - 1); //back

                        graphic.BoxMemory(box);

                        box.Destroy();
                    }//for (int j = 0; j < obj.pShowIndexArray.Count; j++)

                    graphic.EndBoxes();

                    graphic.PopMatrix();
                }
            }
            graphic.PopMatrix();
        }

        void Draw3DGridCubesOverlapsPoints(C3DGridData obj, COverlayObject over)
        {
            if(over.channel == OverlapChannel.Vector2D )
            {
                Draw3DGridCubesOverlapsArrow2D(obj,(Arrow2DOverlayObject)over);
            }
        }
        private void Draw3DGridCubesOverlapsArrow2D(C3DGridData obj, Arrow2DOverlayObject over)
        {
            int nx, ny, id;
            int ix, iy, iz;
            double x, y, z;

            //need change nx,ny,nz order                
            nx = obj.xNum;
            ny = obj.yNum;

            Color color;
            Vector32 p1;           
            List<Vertex3D> points = new List<Vertex3D>();
            graphic.PushMatrix();
                        
            if (over.Filled) graphic.SetPolygonMode(gDrawMode.Fill);
            else graphic.SetPolygonMode(gDrawMode.Wireframe);

            graphic.BeginTriangles();
            double ox, oy, oz;
            for (int i = 0; i < obj.pShowIndexArray.Count; i++)
            {
                ix = (int)obj.pShowIndexArray[i].x;
                iy = (int)obj.pShowIndexArray[i].y;
                iz = (int)obj.pShowIndexArray[i].z;
                id = (int)(ix + iy * nx + iz * nx * ny);

                if (float.IsNaN(over.data[id])) continue;
                if (obj.IsBlankedGrid(ix, iy, iz)) continue;

                x = obj.minx + obj.xStep * (ix + 0.5);
                y = obj.miny + obj.yStep * (iy + 0.5);
                z = obj.minz + obj.zStep * (iz + 0.5);
                
                ox = x;oy = y;oz = z;//网格中的位置

                p1 = obj.TransformedPoint(new Vector32(x, y, z)); 
                p1 = toWorldVector(obj, p1);
                p1 = CDataModel.ToModelVector(p1);

                /////////////////////////////////
                Arrow2D arrow = new Arrow2D();
                arrow.offset = over.offset;
                arrow.Width = over.arrowSize;
                arrow.Theta = over.Theta;               
                arrow.Create(p1.toVector64(), over.LineLength, over.data[id]);
                
                Vector32 p2 = arrow.End;
                p2.x -= arrow.offset.x;
                p2.y -= arrow.offset.y;
                p2.z -= arrow.offset.z;
                p2 = CDataModel.FromModelVector(p2);
                p2 = obj.UnTransformedPoint(p2);
                if ( obj.IsBlankedGrid( obj.GetIndices(p2)) )continue;

                if (!over.EnableColorLevel) color = over.ObjColor;
                else color = over.GetColor(over.data[id]);

                if ( over.Blend ) color = Color.FromArgb((byte)(255*over.Alpha),color);

                TriangleObj tri = arrow.toTriangleObject();
                tri.Alpha = over.Alpha;
                tri.Blend = over.Blend;
                tri.uniformColor = color;
                tri.IsUniformColor = true;
                graphic.TriangleMemory(tri);

                if (over.EnableLineColor) color = over.LineColor;

                Vertex3D vp1 = CreateVertex(arrow.Start);
                Vertex3D vp2 = CreateVertex(arrow.End);
                if (over.Blend) color = Color.FromArgb((byte)(255 * over.Alpha), color);
                vp1.SetColor(color);
                vp2.SetColor(color);

                points.Add(vp1);
                points.Add(vp2);

            }//for (int i = 0; i < obj.pShowIndexArray.Count; i++)

            graphic.EndTriangles();

            int[] indices = new int[points.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }
            graphic.SetLineWidth(over.LineWidth);
            graphic.DrawLinesList(points.ToArray(), indices);

            indices = null;
            points.Clear();
            
            graphic.PopMatrix();
        }
        private void Draw3DGridCubesOverlaps(C3DGridData obj)
        {
            if (obj.overlaps.Count < 1 || !obj.enableOverlap) return; 
           
            for (int k = 0; k < obj.overlaps.Count; k++)
            {
                if ( obj.overlaps[k].Enable )
                {
                    if(obj.overlaps[k].type == ShapeEnum.Points)
                    {
                        Draw3DGridCubesOverlapsPoints(obj,obj.overlaps[k]);
                    }
                }                
            }//for (int k = 0; k < obj.overlaps.Count; k++)
        }

        //对象坐标转换成模型坐标
        private Vector32 toModelVector(Vector32 p)
        {
            return CDataModel.ToModelVector(p);
        }
        private Vector64 toModelVector64(Vector64 p)
        {
            return CDataModel.ToModelVector(p);
        }

        
        /// <summary>
        /// 开始屏幕录制，将图片记录到图片列表中
        /// </summary>
        /// <param name="aviPath">avi录制路径</param>
        /// <param name="frames">每秒帧数</param>
        /// <returns></returns>
        public bool StartScreenRecord(string aviPath,int frames = 2 )
        {
            C3DData.tempRecordPath = Path.GetTempPath() + @"3DSurfer\Recording\";
            if ( !Directory.Exists(C3DData.tempRecordPath) )
            {
                try
                {
                    Directory.CreateDirectory(C3DData.tempRecordPath);
                }
                catch (Exception ex)
                {
                    AddToMessage("Writting to temporary files failed." + ex.Message);
                    return false;
                }
            }

            if ( !Directory.Exists(C3DData.tempRecordPath) )
            {
                AddToMessage("Writting to temporary files failed.");
                return false;
            }            

            C3DData.aviRecordFile = aviPath;
            C3DData.framesPersecond = frames;

            C3DData.recordingIndex = 0;
            C3DData.recordList.Clear();
            C3DData.lastRecording = DateTime.Now;
                       
            C3DData.Recording = true;

            Thread thread = new Thread(RecordingThread);
            thread.Start();

            return true;
        }
        
        public void RecordingThread()
        {
            int tsInterval = 1000 / C3DData.framesPersecond;
            if ( tsInterval < 50) tsInterval = 50;
            DateTime starttime = DateTime.Now;
            C3DData.recordingIndex = 0;
            while (C3DData.Recording)
            {
                try
                {
                    TimeSpan dd = DateTime.Now - starttime;// 记录时间间隔 
                    string jpg = C3DData.tempRecordPath + C3DData.recordingIndex + ".jpg";
                    ScreenBitmapCapture(jpg);
                    C3DData.recordList.Add(dd.TotalMilliseconds);
                    C3DData.lastRecording = DateTime.Now;
                    C3DData.recordingIndex++;
                }
                catch (Exception ex)
                {
                    AddToMessage("Recording breaked for errors." + ex.Message);
                    C3DData.Recording = false;
                    break;
                }

                Thread.Sleep(tsInterval);
            }
        }

        unsafe public bool EndScreenRecord()
        {
            if ( !C3DData.Recording ) 
            {
                AddToMessage("no actived screen recording.");
                return false; 
            }
            
            bool ret = false;

            C3DData.Recording = false;
            Thread.Sleep(1000); //等待线程自然结束

            try
            {
                Avi.AVICOMPRESSOPTIONS opts = new Avi.AVICOMPRESSOPTIONS();
                opts.fccType = 0;
                opts.fccHandler = (int)0x6376736d; //microsoft video 1 compression
                //opts.fccHandler = (int)0x64697678; //xvid 1684633208，出错
                //opts.fccHandler = (int)0x64697663; // for cinepak codec
                opts.dwKeyFrameEvery = 0;                
                opts.dwQuality = 10000;
                opts.dwFlags = 8;
                opts.dwBytesPerSecond = 0;
                opts.lpFormat = new IntPtr(0);
                opts.cbFormat = 0;
                opts.lpParms = new IntPtr(0);
                opts.cbParms = 4;
                opts.dwInterleaveEvery = 0;
                //Avi.AVICOMPRESSOPTIONS* p = &opts;
                //Avi.AVICOMPRESSOPTIONS** pp = &p;
                //IntPtr x = p;
                //IntPtr* ptr_ps = &x;
                opts.lpParms = Marshal.AllocHGlobal(sizeof(int));
                //AVISaveOptions(0, 0, 1, ptr_ps, pp);
                //int hr = Avi.AVIMakeCompressedStream(out _psCompressed, _ps, ref opts, 0);

                Bitmap bmp = new Bitmap(C3DData.tempRecordPath + "0.jpg");
                AviManager aviManager = new AviManager(C3DData.aviRecordFile, false);
                //VideoStream aviStream = aviManager.AddVideoStream(false, C3DData.framesPersecond, bmp);
                VideoStream aviStream = aviManager.AddVideoStream(opts, C3DData.framesPersecond, bmp);

                //C3DData.tempRecordPath = Path.GetTempPath() + @"3DSurfer\Recording\";
                if ( C3DData.recordList.Count > 0 )
                {
                    int count = 0;
                    string jpg;
                    for (int i = 1; i < C3DData.recordList.Count; i++)
                    {
                        jpg = C3DData.tempRecordPath + i + ".jpg";
                        if (File.Exists(jpg))
                        {
                            bmp = new Bitmap(jpg);
                            if (bmp != null)
                            {
                                aviStream.AddFrame(bmp);
                                count++;
                                bmp.Dispose();
                            }
                        }
                    }                    
                }

                aviManager.Close();
                                
                ret = true;
            }
            catch (Exception ex)
            {
                AddToMessage("recording failed ! " + ex.Message);
                ret = false;
            }           

            return ret;
        }

        public delegate void OnDoScript(ScriptFunction sf);
        private void DoScriptThread(Object obj) //线程版本
        {
            if ( this.InvokeRequired )
            {
                OnDoScript outdelegate = new OnDoScript(DoScriptThread);
                this.BeginInvoke(outdelegate, obj);
            }
            else
            {
                ScriptFunction sf = (ScriptFunction)obj;
                //sf.DoScripts();   
                string line;
                char[] cc = new char[] { ' ', '\t' };
                for (int k = 0; k < sf.Lines.Count; k++)
                {
                    line = sf.Lines[k].Trim(cc);
                    if (line.Length > 0)
                    {
                        if (line[0] == '#' || line[0] == '/' || line[0] == '!')
                            continue;

                        FunctionStruct fs = sf.Parse(line, k);
                        if (fs.IsValid)
                        {
                            AddToMessage("Doing function " + fs.Key.ToString());
                            sf.DoFunction(fs);
                        }
                    }
                }
            }
        }
        public void DoScript(string path) //线程版本
        {
            ScriptFunction sf = new ScriptFunction();
            if( sf.LoadScript(path) > 1 )
            {
                Thread scriptThread = new Thread(DoScriptThread);
                scriptThread.Start(sf);
                AddToMessage("scripts interpreter started...");
            }
        }

        //clear and redraw all objects
        public void UpdateDraw()
        {
            this.Cursor = Cursors.WaitCursor;

            if ( graphic.initialized)
            {
                try
                {                    
                    DrawObjects();
                    UpdateView();
                }
                catch(Exception e)
                {
                    //MessageBox.Show(e.Message);                    
                    AddToMessage(e.Message);
                }                
            }
            this.Cursor = DefaultCursor;
        }

        internal void OnUpdateDrawEvent(object sender, EventArgs e)
        {
            //取到主窗体的传来的文本
            DrawUpdateEventArg arg = e as DrawUpdateEventArg;
            
            bool refreshAll = arg.refreshAll;

            //对象类型，0 -C3DObjectBase对象，1虚线框，2坐标轴箭头，3Lights位置, 4选择物体虚线框
            UpdateDrawTypeEnum type = arg.updateType;
            C3DObjectBase obj = arg.Obj;

            if( refreshAll )
            {
                UpdateDraw();                
            }
            else
            {
                if (type == UpdateDrawTypeEnum.UpdateObject && obj != null)
                {
                    UpdateDraw(obj);
                }
                else if (type == 0 && obj == null)
                {
                   // UpdateView();
                }
                else if (type == UpdateDrawTypeEnum.UpdateOutline)
                {
                    DrawOutLines(outLinesObject);                    
                }
                else if (type == UpdateDrawTypeEnum.Update3DArrow)
                {
                    DrawDirectionArrow(arrowObject);
                }
                else if (type == UpdateDrawTypeEnum.Update3DLights)
                {
                    DrawLightsPosition(lightposObject);
                }
                else if (type == UpdateDrawTypeEnum.UpdateSelectedBox)
                {
                    DrawSelectedOutLines();
                }
                else if (type == UpdateDrawTypeEnum.UpdateAxisLabel)
                {
                    DrawXAxis(CDataModel.xAxisRuler);
                    DrawYAxis(CDataModel.yAxisRuler);
                    DrawZAxis(CDataModel.zAxisRuler);
                }
            }
            UpdateView();
        }

        //clear all
        public void ClearObjectDrawBuffer()
        {
            graphic.ClearDrawBuffer();
            graphic.ClearModelKeyBuffers();

            foreach(C3DObjectBase obj in C3DData.GetObjects() )
            {
                obj.ClearRenderingBuffers();
            }

            axisXObject.ClearRenderingBuffers();
            axisYObject.ClearRenderingBuffers();
            axisZObject.ClearRenderingBuffers();
            lightposObject.ClearRenderingBuffers();
            arrowObject.ClearRenderingBuffers();
            outLinesObject.ClearRenderingBuffers();
        }
        public void ClearObjectDrawBuffer(C3DObjectBase obj)
        {
            if (obj.type == ShapeEnum.PolygonSlicer)
            {
                PolygonSlicer slicer = obj as PolygonSlicer;
                graphic.ClearModelsByKeys(slicer.tracedGeoObjects.RenderingBuffers);
                graphic.ClearModelsByKeys(slicer.polygons.RenderingBuffers);
                graphic.ClearModelsByKeys(slicer.RenderingBuffers);                
                slicer.ClearRenderingBuffers();
            }
            else
            {
                if (obj.RenderingBuffers.Count > 0)
                {
                    graphic.ClearModelsByKeys(obj.RenderingBuffers);
                    //graphic.ClearModelKeyBuffers();
                    obj.ClearRenderingBuffers();
                }
            }            
        }
        public void ClearObjectDrawBuffers(List<C3DObjectBase>objects)
        {
            foreach (C3DObjectBase obj in objects) 
                ClearObjectDrawBuffer(obj);            
        }
        //Update single object
        public void UpdateDraw(C3DObjectBase obj, bool refresh = false)
        {
            RenderingUpdateMode mode = obj.RenderMode;
            if (mode == RenderingUpdateMode.Visible)
            {
                if( obj.RenderingBuffers.Count > 0 )
                   graphic.SetModelsVisibleByKeys(obj.RenderingBuffers, obj.Visible);
                else
                {
                    ClearObjectDrawBuffer(obj);
                    graphic.ClearModelKeyBuffers();
                    DrawObject(obj);                    
                    obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);
                }                
            }
            else if (mode == RenderingUpdateMode.Redraw)
            {
                ClearObjectDrawBuffer(obj);
                graphic.ClearModelKeyBuffers();
                DrawObject(obj);                
                obj.AddRenderingBuffer(graphic.objectModelKeyBuffers);
                DoRangeUpdated();                
            }

            obj.RenderMode = RenderingUpdateMode.None;
            
            UpdateView();
        }
        //update 3D window, No object updated
        Object viewObject = new object();
        public void UpdateView()
        {
            this.Cursor = Cursors.WaitCursor;
            lock(viewObject)
            {
                if (graphic.engine == gEngine.opengl)
                {
                  
                }
                else graphic.UpdateDraw();                
            }            
            
            this.Cursor = DefaultCursor;
        }

        private CInversePower SearchNearest(C3DGridData data,
                                    double x0,double y0,double z0,
                                    int min_points_no = 8 )
        {
            CInversePower ip = new CInversePower();
            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;
            double minx = data.minx;
            double miny = data.miny;
            double minz = data.minz;
            double maxx = data.maxx;
            double maxy = data.maxy;
            double maxz = data.maxz;
            double stepx = (maxx - minx) / (nx - 1);
            double stepy = (maxy - miny) / (ny - 1);
            double stepz = (maxz - minz) / (nz - 1);
            int ix0 = (int)((x0 - minx) / stepx);
            int iy0 = (int)((y0 - miny) / stepy);
            int iz0 = (int)((z0 - minz) / stepz);

            double x, y, z,v;
            int rx, ry, rz,id;

            double rstep = Math.Sqrt(stepx * stepx + stepy * stepy + stepz * stepz);

            for (int len = 4; len < 5; len++)
            {
                rx = (int)(len * rstep / stepx);
                ry = (int)(len * rstep / stepy);
                rz = (int)(len * rstep / stepz);

                for (int k = iz0 -rz; k <= iz0 + rz; k ++ )
                {
                    if (k < 0 || k >= nz) continue;
                    z = minz + k * stepz;

                    for (int j = iy0 - ry; j <= iy0 + ry; j ++ )
                    {
                        if (j < 0 || j >= ny) continue;
                        y = miny + j * stepy;

                        for (int i = ix0-rx; i <= ix0 + rx; i ++ )
                        {
                            if (i < 0 || i >= nx) continue;

                            x = minx + i * stepx;

                            id = i + j * nx + k * nx * ny;
                            v = data[id];

                            if (!data.IsBlankValue(v))
                                ip.AddPoint(x, y, z, v);

                            if (len >= 4)
                            {
                                if (ip.pScatterPoint.Count >= min_points_no)                                
                                    return ip;
                            }
                        }
                    }
                }
            }
            return ip;
        }
        
        
        //linear
        public C3DGridData ResampleGridX(C3DGridData data,int nx1)
        {
            //data.m_blankvalue = data.pGridData[0];
            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;
            double minx = data.minx;
            double miny = data.miny;
            double minz = data.minz;
            double minv = data.minv;
            double maxx = data.maxx;
            double maxy = data.maxy;
            double maxz = data.maxz;
            double maxv = data.maxv;
            double stepx = (maxx - minx) / (nx - 1);            
            double stepx1 = (maxx - minx) / (nx1 - 1);
            C3DGridData data1 = new C3DGridData();
            data1.pGridData = new float[nx1 * ny * nz];
            if(data1.pGridData == null) //no enough memory
                return null;
            int i, j, k;
            int id,id1,id2;            
            double v, v1, v2;
            int ix, ix1, ix2;
            double x, x1, x2;

            ix1 = ix2 = 0;
            x1 = x2 = 0;
            //x direction resample
            for (i = 0; i < nx1; i ++)
            {
                //sample position x1
                x = minx + i * stepx1;
                ix = (int)( (x-minx) / stepx );
                for (j = 0; j < ny; j++)
                {
                    for (k = 0; k < nz; k++)
                    {
                        id = i + j * nx1 + k * nx1 * ny;

                        if (i == 0)
                        {
                            id1 = j * nx + k * nx * ny;
                            data1[id] = data[id1];
                        }
                        else if (i == nx1 - 1)
                        {
                            id1 = nx-1 + j * nx + k * nx * ny;
                            data1[id] = data[id1];
                        }
                        else
                        {
                            ix1 = ix;
                            ix2 = ix1 + 1;

                            x1 = minx + ix1 * stepx;
                            x2 = minx + ix2 * stepx;

                            id1 = ix1 + j * nx + k * nx * ny;
                            v1 = data[id1];

                            id2 = ix2 + j * nx + k * nx * ny;
                            v2 = data[id2];

                            v = v1 + (v2 - v1) * (x - x1) / (x2 - x1);
                            data1[id] = (float)v;
                        }
                    }
                }
            }
            data1.xNum = nx1;
            data1.yNum = ny;
            data1.zNum = nz;
            data1.minx = minx;
            data1.miny = miny;
            data1.minz = minz;
            data1.minv = minv;
            data1.maxx = maxx;
            data1.maxy = maxy;
            data1.maxz = maxz;
            data1.maxv = maxv;
            return data1;
        }
        public C3DGridData ResampleGridY(C3DGridData data, int ny1)
        {
            //data.m_blankvalue = data.pGridData[0];
            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;
            double minx = data.minx;
            double miny = data.miny;
            double minz = data.minz;
            double minv = data.minv;
            double maxx = data.maxx;
            double maxy = data.maxy;
            double maxz = data.maxz;
            double maxv = data.maxv;
            double stepy = (maxy - miny) / (ny - 1);
            double stepy1 = (maxy - miny) / (ny1 - 1);
            C3DGridData data1 = new C3DGridData();
            data1.pGridData = new float[nx * ny1 * nz];
            if (data1.pGridData == null) //no enough memory
                return null;
            int i, j, k;
            int id, id1, id2;
            double v, v1, v2;
            int iy, iy1, iy2;
            double y, y1, y2;

            iy1 = iy2 = 0;
            y1 = y2 = 0;
            //y direction resample
            for (j = 0; j < ny1; j++)
            {
                //sample position x1
                y = miny + j * stepy1;
                iy = (int)((y-miny) / stepy);
                for (i = 0; i < nx; i++)
                {
                    for (k = 0; k < nz; k++)
                    {
                        id = i + j * nx + k * nx * ny1;

                        if (j == 0)
                        {
                            id1 = i + j * nx + k * nx * ny;
                            data1.pGridData[id] = data.pGridData[id1];
                        }
                        else if (j == ny1 - 1)
                        {
                            id1 = i + (ny-1) * nx + k * nx * ny;
                            data1.pGridData[id] = data.pGridData[id1];
                        }
                        else
                        {
                            iy1 = iy;
                            iy2 = iy1 + 1;

                            y1 = miny + iy1 * stepy;
                            y2 = miny + iy2 * stepy;

                            id1 = i + iy1 * nx + k * nx * ny;
                            v1 = data.pGridData[id1];

                            id2 = i + iy2 * nx + k * nx * ny;
                            v2 = data.pGridData[id2];

                            v = v1 + (v2 - v1) * (y - y1) / (y2 - y1);
                            data1.pGridData[id] = (float)v;
                        }
                    }
                }
            }
            data1.xNum = nx;
            data1.yNum = ny1;
            data1.zNum = nz;
            data1.minx = minx;
            data1.miny = miny;
            data1.minz = minz;
            data1.minv = minv;
            data1.maxx = maxx;
            data1.maxy = maxy;
            data1.maxz = maxz;
            data1.maxv = maxv;
            return data1;
        }
        public C3DGridData ResampleGridZ(C3DGridData data, int nz1)
        {
            //data.m_blankvalue = data.pGridData[0];
            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;
            double minx = data.minx;
            double miny = data.miny;
            double minz = data.minz;
            double minv = data.minv;
            double maxx = data.maxx;
            double maxy = data.maxy;
            double maxz = data.maxz;
            double maxv = data.maxv;
            double stepz = (maxz - minz) / (nz - 1);
            double stepz1 = (maxz - minz) / (nz1 - 1);
            C3DGridData data1 = new C3DGridData();
            data1.pGridData = new float[nx * ny * nz1];
            if (data1.pGridData == null) //no enough memory
                return null;
            int i, j, k;
            int id, id1, id2;
            double v, v1, v2;
            int iz, iz1, iz2;
            double z, z1, z2;

            iz1 = iz2 = 0;
            z1 = z2 = 0;
            //z direction resample
            for (k = 0; k < nz1; k++)
            {
                //sample position z1
                z = minz + k * stepz1;
                iz = (int)((z-minz) / stepz);
                for (i = 0; i < nx; i++)
                {
                    for (j = 0; j < ny; j++)
                    {
                        id = i + j * nx + k * nx * ny;

                        if (k == 0)
                        {
                            id1 = i + j * nx + k * nx * ny;
                            data1.pGridData[id] = data.pGridData[id1];
                        }
                        else if (k == nz1 - 1)
                        {
                            id1 = i + j * nx + (nz-1) * nx * ny;
                            data1.pGridData[id] = data.pGridData[id1];
                        }
                        else
                        {
                            iz1 = iz;
                            iz2 = iz1 + 1;

                            z1 = minz + iz1 * stepz;
                            z2 = minz + iz2 * stepz;

                            id1 = i + j * nx + iz1 * nx * ny;
                            v1 = data.pGridData[id1];

                            id2 = i + j * nx + iz2 * nx * ny;
                            v2 = data.pGridData[id2];

                            v = v1 + (v2 - v1) * (z - z1) / (z2 - z1);
                            data1.pGridData[id] = (float)v;
                        }
                    }
                }
            }
            data1.xNum = nx;
            data1.yNum = ny;
            data1.zNum = nz1;
            data1.minx = minx;
            data1.miny = miny;
            data1.minz = minz;
            data1.minv = minv;
            data1.maxx = maxx;
            data1.maxy = maxy;
            data1.maxz = maxz;
            data1.maxv = maxv;
            return data1;
        }
        public C3DGridData ResampleGrid(C3DGridData data, int nx1, int ny1, int nz1)
        {            
            data.m_blankvalue = data[0];

            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;

            double minx = data.minx;
            double miny = data.miny;
            double minz = data.minz;
            double minv = data.minv;
            double maxx = data.maxx;
            double maxy = data.maxy;
            double maxz = data.maxz;
            double maxv = data.maxv;

            double stepx = (maxx - minx) / (nx - 1);
            double stepy = (maxy - miny) / (ny - 1);
            double stepz = (maxz - minz) / (nz - 1);
            double stepx1 = (maxx - minx) / (nx1 - 1);
            double stepy1 = (maxy - miny) / (ny1 - 1);
            double stepz1 = (maxz - minz) / (nz1 - 1);

            C3DGridData data1 = null;
            if ( nx1 != nx )
            {
                data1 = ResampleGridX(data, nx1);
            }
            if (ny1 != ny)
            {
                if( data1 != null )
                   data1 = ResampleGridY(data1, nx1);
            }
            if (nz1 != nz)
            {
                if (data1 != null)
                    data1 = ResampleGridZ(data1, nx1);
            }

            if (data1 == null) return data;

            data1.xNum = nx1;
            data1.yNum = ny1;
            data1.zNum = nz1;
            data1.minx = minx;
            data1.miny = miny;
            data1.minz = minz;
            data1.minv = minv;
            data1.maxx = maxx;
            data1.maxy = maxy;
            data1.maxz = maxz;
            data1.maxv = maxv;
            
            return data1;
        }     

        public void DoMouseDrag(Point start,Point end)
        {
            isLeftDrag = true;
            this.startDrag(start);
            this.drag(end);
            isLeftDrag = false;
            ThisTransformation.get_Renamed(matrix);
            graphic.SetModelMatrix(matrix);
            UpdateView();
        }
        public void MoveForward(float step)
        {
            graphic.MoveForward(step);
        }
        public void MoveBackward(float step)
        {
            graphic.MoveBackward(step);
        }

        #region Deal With Window Messages
        private void DoPaint(object sender, PaintEventArgs e)
        {
            UpdateView();
            //graphic.UpdateDraw();
        }

        public Bitmap GetScreenBitmap()
        {
            //获得当前屏幕的大小
            Rectangle rect = new Rectangle(this.Location, new Size(this.Width, this.Height));
            //rect = Screen.GetWorkingArea(this);
            //创建一个以当前屏幕为模板的图象
            Graphics g1 = this.CreateGraphics();
            ////创建以屏幕大小为标准的位图
            Bitmap MyImage = new Bitmap(rect.Width, rect.Height, g1);
            Graphics g2 = Graphics.FromImage(MyImage);
            Point p0 = this.PointToScreen(new Point(0, 0));
            g2.CopyFromScreen(p0, new Point(0, 0), new Size(this.Width, this.Height));
            return MyImage;
        }

        private delegate void OnScreenBitmapCapture(string jpg);
        public void ScreenBitmapCapture(string jpg)
        {
            if (this.InvokeRequired)
            {
                OnScreenBitmapCapture bit = new OnScreenBitmapCapture(ScreenBitmapCapture);
                this.BeginInvoke(bit,jpg);
            }
            else
            {
                //获得当前屏幕的大小
                Rectangle rect = new Rectangle(this.Location, new Size(this.Width, this.Height));
                //rect = Screen.GetWorkingArea(this);
                //创建一个以当前屏幕为模板的图象
                Graphics g1 = this.CreateGraphics();
                ////创建以屏幕大小为标准的位图
                Bitmap MyImage = new Bitmap(rect.Width, rect.Height, g1);
                Graphics g2 = Graphics.FromImage(MyImage);
                Point p0 = this.PointToScreen(new Point(0, 0));
                g2.CopyFromScreen(p0, new Point(0, 0), new Size(this.Width, this.Height));
                MyImage.Save(jpg);
                MyImage.Dispose();
            }
        }
        
        
        private void DoSizeChanged(object sender, EventArgs e)
        {           

            if( Width <5 || Height <5 ) return; //window is too small,stop rendering

            if (!this.IsHandleCreated) return;
                       
            if (this.OldHandle != this.Handle)
            {
                Reinitialize();
            }
           
            if (graphic == null) return;            
            //if (!graphic.initialized) return;
            if (graphic.engine == gEngine.opengl)
            {                       
                graphic.onWindowResized(Width, Height);
                arcBall.setBounds(Width, Height);
                UpdateView();
            }
            else
            {                
                graphic.onWindowResized(Width, Height);
                arcBall.setBounds(Width, Height);                
                UpdateView();
            }
        }
        private void DoMouseDown(object sender, MouseEventArgs e)
        {
            bMouseDown = true;
            isLeftDrag = isRightDrag = isMiddleDrag = false;
            if (e.Button == MouseButtons.Left) isLeftDrag = true;
            else if (e.Button == MouseButtons.Right) isRightDrag = true;
            else if (e.Button == MouseButtons.Middle) isMiddleDrag = true;

            this.startDrag(new Point(e.X, e.Y));
        }
        private void DoMouseUp(object sender, MouseEventArgs e)
        {
            bMouseDown = false;
            //arcball Draw
            isLeftDrag = false;
        }
        private void DoMouseMove(object sender, MouseEventArgs e)
        {
            if (!bMouseDown) return;
            if (!graphic.initialized) return;

            if (mouseStartDrag == null) return;
            if (mouseStartDrag == e.Location) return;
            drag(e.Location);
            mouseStartDrag = e.Location;
            startDrag(mouseStartDrag);

            /////this validation section//////////////
            ///Validation
            if (C3DData.DemoVersion)
            {
                if (!CheckVialidation()) return;
            }
            /////this is end validation section//////////////
            ThisTransformation.get_Renamed(matrix);
            graphic.SetModelMatrix(matrix);
            UpdateView();
        }

        private void DoMouseWheel(object sender, MouseEventArgs e)
        {
            float zoomSpeed = C3DData.zoomSpeed;
            if (IsAltKeyDown) zoomSpeed = C3DData.zoomFastSpeed;
            else if (IsControlKeyDown) zoomSpeed = C3DData.zoomSlowSpeed;

            float step = Math.Abs( e.Delta * 0.001f * zoomSpeed);
            //step = 2;
            if (e.Delta < 0)    //move down-up
            {
                av += step;
                //graphic.Perspective(av,0.01f,100f);
                graphic.MoveBackward(step);
                UpdateView();
            }
            else  // move up-down
            {
               // if (av > step)
                {
                    av -= step;
                    //graphic.Perspective(av,0.01f, 100f);
                    graphic.MoveForward(step);
                    UpdateView();
                }
            }
        }
        #endregion Deal With Window Messsages

        
        #region Mouse Control
        // reset the matrix
        public void reset()
        {
            lock (matrixLock)
            {
                LastTransformation.SetIdentity();                                // Reset Rotation
                ThisTransformation.SetIdentity();                                // Reset Rotation
            }
        }
        private void startDrag(Point MousePt)
        {           
            // Update Start Vector And Prepare For Dragging
            arcBall.click(MousePt);
            mouseStartDrag = MousePt;
        }       

        private void drag(Point MousePt)
        {
           /* float offx = mouseStartDrag.X - MousePt.X;
            float offy = mouseStartDrag.Y - MousePt.Y;
            if (offx < 0) offx = -offx;
            if (offy < 0) offy = -offy;

            // < minimum offset
            if (offx + offy < 4) return;

            //距离越近，转动幅度越小
            double r = graphic.EyeDistance / 5.0;
            if (r < 0.001) r = 0.001;
            if ( r > 2 ) r = 5;
            offx = (float)(offx * r * r);
            offy = (float)(offy * r * r);*/

            Quat4f ThisQuat = new Quat4f();

            // Update End Vector And Get Rotation As Quaternion
            arcBall.drag(MousePt, ThisQuat); 

            lock (matrixLock)
            {
                if (isMiddleDrag) //zoom
                {
                    double len = Math.Sqrt(mouseStartDrag.X * mouseStartDrag.X + mouseStartDrag.Y * mouseStartDrag.Y)
                        / Math.Sqrt(MousePt.X * MousePt.X + MousePt.Y * MousePt.Y);

                    ThisTransformation.Scale = (float)len;
                    ThisTransformation.Pan = new Vector3f(0, 0, 0);
                    ThisTransformation.Rotation = new Quat4f();
                    ThisTransformation.MatrixMultiply(ThisTransformation, LastTransformation);// Accumulate Last Rotation Into This One
                }
                else if (isRightDrag) //pan
                {
                    float x = (MousePt.X - mouseStartDrag.X) / (float)Width;
                    float y = (MousePt.Y - mouseStartDrag.Y) / (float)Height;
                    float z = 0.0f;

                    ThisTransformation.Pan = new Vector3f(x, y, z);
                    ThisTransformation.Scale = 1.0f;
                    ThisTransformation.Rotation = new Quat4f();
                    ThisTransformation.MatrixMultiply(ThisTransformation, LastTransformation);
                }
                else if (isLeftDrag) //rotate
                {
                    ThisTransformation.Pan = new Vector3f(0, 0, 0);
                    ThisTransformation.Scale = 1.0f;
                    ThisTransformation.Rotation = ThisQuat;
                    ThisTransformation.MatrixMultiply(ThisTransformation, LastTransformation);                                     
                }
                lock (matrixLock)
                {
                    // Set Last Static Rotation To Last Dynamic One
                    LastTransformation.set_Renamed(ThisTransformation);
                    //LastTransformation = ThisTransformation.Copy();
                }
            }
        }
        #endregion Mouse Control

        private void glControl1_Click(object sender, EventArgs e)
        {
            this.Focus();
            this.BringToFront();
        }

        private void DDDForm_Click(object sender, EventArgs e)
        {
            this.Focus();
            this.BringToFront();
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void glControl1_SizeChanged(object sender, EventArgs e)
        {

        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:                    
                    break;
                case Keys.Right:                    
                    break;
                case Keys.Up:
                    MoveForward(0.01f);
                    UpdateView();
                    break;
                case Keys.Down:
                    MoveBackward(0.01f);
                    UpdateView();
                    break;                
            }
            return true;
        }

        private void DDDForm_KeyDown(object sender, KeyEventArgs e)
        {
            //屏幕录制
            if( e.KeyCode == Keys.A && C3DData.Recording)
            {
                //C3DData.aviBitmaps.Add(GetBitmap());
            }
            
            else if ( e.KeyCode == Keys.Up )
            {
          //      MoveForward(0.001f);
            }
            else if (e.KeyCode == Keys.Down)
            {
             //   MoveBackward(0.001f);
            }
            IsControlKeyDown = e.Control;
            IsAltKeyDown = e.Alt;            
        }

        private void DDDForm_KeyUp(object sender, KeyEventArgs e)
        {
            IsControlKeyDown = e.Control;
            IsAltKeyDown = e.Alt;
        }

        private void DDDForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            IsControlKeyDown = e.Control;
            IsAltKeyDown = e.Alt;
        }
    }
}

