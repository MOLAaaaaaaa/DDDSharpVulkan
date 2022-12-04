using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using DataCollection;
using DataCollection.Shapefile;
using Graphics3D;
using AviFile;
using DDDSharp.Modeling;
using DDDSharp.Boreholes;
using DDDSharp.Gridding;

namespace DDDSharp
{
    public partial class MainForm : Form
    {
        //三维窗口
        public DDDForm m_DDDForm;
        //对象浏览窗口
        public ObjectForm m_ObjectForm;
        //信息显示窗口
        private InformationForm m_InforForm;

        public C3DGridDataPropertyForm m_3DGridForm ;
        public PlyPropertyForm m_PlyPropertyForm ;
        public CSlicerPropertyForm m_SlicerPropertyForm;
        public CBoreholesPropertyForm m_BoreholesPropertyForm;
        public C3DLinePropertyForm m_3DLinePropertyForm;
        public ScatteredPointsPropertyForm m_ScatterPointsPropertyForm;

        string CaptionTitle = "3D Surfer -- data visualization";

        static int iBoxCreated = 1;
        static int iConeCreated = 1;
        static int iCylinderCreated = 1;


        public MainForm()
        {  
            InitializeComponent();            
            //三维窗口
            m_DDDForm = new DDDForm();
            drawUpdateEvent += m_DDDForm.OnUpdateDrawEvent; //绘图更新事件
            KeyPreview = true;
            //对象浏览窗口
            m_ObjectForm = new ObjectForm();
            //信息显示窗口
            m_InforForm = new InformationForm();
            m_3DGridForm = new C3DGridDataPropertyForm();
            m_PlyPropertyForm = new PlyPropertyForm();
            m_SlicerPropertyForm = new CSlicerPropertyForm();
            m_BoreholesPropertyForm = new CBoreholesPropertyForm();
            m_3DLinePropertyForm = new C3DLinePropertyForm();
            m_ScatterPointsPropertyForm = new ScatteredPointsPropertyForm();
            Text = CaptionTitle;
        }

        /// <summary>
        /// 删除文件夹及其内容
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
                else  DeleteFolder(d);////递归删除子文件夹
                Directory.Delete(d);
            }
        }
        public void DeleteFolderFiles(string dir)
        {
            if (!Directory.Exists(dir)) return;

            string []dirs = Directory.GetFileSystemEntries(dir);
            if (dirs == null) return;
            foreach (string d in dirs )
            {
                if ( File.Exists(d) )
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            dockPanel1.Visible = !dockPanel1.Visible;
            dockPanel1.Enabled = !dockPanel1.Enabled;
            Invalidate();
        }
        public delegate void AddtoInfoDelegate(string info);
        public void AddtoInfo(string info)
        {
            if (InvokeRequired)
            {
                AddtoInfoDelegate outdelegate = new AddtoInfoDelegate(AddtoInfo);
                this.BeginInvoke(outdelegate,info);
                return;
            }
            else
            {
                if (m_InforForm != null) m_InforForm.AddInfo(info);
            }            
        }
        private bool CheckVialidation()
        {
            int year = C3DData.validYear;
            int month = C3DData.validMonth;
            int day = C3DData.validDay;
            int expiredDays =C3DData.expiredDays;

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
        private void Form1_Load(object sender, EventArgs e)
        {  
            CGraphic3D graphic = C3DData.graphics3D;
            graphic.LoadGraphicConfig();
            if (graphic.engine == gEngine.auto || graphic.engine == gEngine.vulkan)
            {
                if ( !CGraphic3D.IsVulkanSupport() )
                {
                    graphic.engine = gEngine.opengl;
                    AddtoInfo("Vulkan not supported,auto switch to OpenGL.");
                }
            }
            //这句必须要加上  //或者 dockPanel1.Parent = this;
            //dockPanel1.DocumentStyle = DocumentStyle.DockingWindow;
            dockPanel1.Parent = this;
            m_DDDForm.Show(dockPanel1, DockState.Document);
            m_InforForm.Show(dockPanel1, DockState.DockBottom);
            m_ObjectForm.Show(this.dockPanel1, DockState.DockLeft);
            //m_ObjectForm.Show(this.dockPanel1, DockState.DockLeftAutoHide);
            //m_3DGridForm.Show(this.dockPanel1, DockState.DockLeft|DockState.DockBottom);
            //m_PlyPropertyForm.Show(this.dockPanel1, DockState.DockLeftAutoHide);       

            //Register Verify
            bool regist = false;

            //检查注册表
            RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
            if ( !reg.ReadFromRegister() ) regist = true;
            else //验证注册信息
            {                
                Random rand = new Random();
                RegisterAndEncrypt.HardWareInfo.InfoType type = (RegisterAndEncrypt.HardWareInfo.InfoType)rand.Next(3);
                if ( !reg.Verify(type) ) regist = true;
            }            
            if( regist )//注册界面
            {
                //first register
                RegisterForm reg1 = new RegisterForm();
                reg1.Left = Width / 2;
                reg1.Top = Height / 2;
                reg1.Show(this);
                //reg1.BringToFront();                
            }
            
        }

        private void LoadDataFrom3DGrid(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.Grid3DFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;
                        bool ret = C3DData.Load3DGridData(dlg.FileName);
                        Cursor = Cursors.Default;

                        if ( ret )
                        {
                            /////this validation section//////////////
                            ///Validation
                            if (C3DData.DemoVersion)
                            {
                                if (!CheckVialidation()) return;
                            }
                            /////this is end validation section//////////////
                            if ( !CDataModel.IsModelScaleAcceptable(0.01) )
                            {
                                string ss = "This model doesn't have a proper scale." + Environment.NewLine;
                                MessageBoxWarning(ss + CDataModel.m_ModelOrg.toString());
                            }
                            
                            if (C3DData.objectsDiction.Count > 1)
                            {
                                C3DData.UpdateRange();
                                m_ObjectForm.AddToTree(C3DData.lastLoaded);
                                C3DData.objSelected = C3DData.lastLoaded;
                                UpdateDraw(C3DData.lastLoaded);                                
                            }
                            else
                            {
                                m_ObjectForm.AddToTree(C3DData.lastLoaded);
                                C3DData.objSelected = C3DData.lastLoaded;
                                UpdateDraw(C3DData.lastLoaded);
                            }                            
                            //m_DDDForm.UpdateDraw();

                        }
                        else MessageBoxErr(C3DData.errMessage);                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }

        }
        private void load3DObjectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.TriangleFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    dlg.Multiselect = true;
                    bool range_updated = false;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        string errinfo = "";
                        C3DData.lastLoadeds.Clear();
                        TriangleObj obj;
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            obj = C3DData.Load3DObjectFile(dlg.FileNames[i]);
                            if (obj == null)
                            {
                                errinfo += dlg.FileNames[i] + ":" + C3DData.errMessage;
                                errinfo += Environment.NewLine;                                
                            }
                            else if (C3DData.AddObject(obj)) range_updated = true;
                        }
                        if( errinfo.Length > 0 && C3DData.lastLoadeds.Count > 0 )
                        {
                            MessageBoxWarning(errinfo, "Loading data failed !!!");
                        }
                        else if (errinfo.Length > 0 && C3DData.lastLoadeds.Count == 0)
                        {
                            MessageBoxErr(errinfo, "Loading data failed !!!");
                        }
                        if ( C3DData.lastLoadeds.Count > 0 )
                        {
                            C3DData.UpdateRange();
                            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                            C3DData.objSelected = C3DData.lastLoaded;
                            if ( range_updated ) UpdateDraw();
                            else UpdateDraw(C3DData.lastLoadeds);
                        }
                        this.Cursor = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        private void loadLASToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.FormatLASControlFileFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        
                        AscIIColumn asc = new AscIIColumn();
                        if( !asc.Load(dlg.FileName) )
                        {
                            this.Cursor = Cursors.Default;

                            MessageBoxErr(asc.errMessage, "Load data failed.");
                            
                            return;
                        }
                        
                        this.Cursor = Cursors.Default;

                        BoreholeListForm bh = new BoreholeListForm();
                        bh.pData = asc;
                        bh.control_file = dlg.FileName;

                        //载入Las data
                        if ( bh.ShowDialog() == DialogResult.OK )
                        {
                            CBoreholes boreholes = bh.boreholes;
                            bool range_updated = false;
                            //显示曲线
                            LasCurveSetDlg ld = new LasCurveSetDlg();
                            ld.boreholes = boreholes;
                            if (ld.ShowDialog() == DialogResult.OK)
                            {
                                boreholes = ld.boreholes;
                                range_updated = C3DData.AddObject(boreholes);

                                m_ObjectForm.AddToTree(C3DData.lastLoaded);
                                C3DData.objSelected = C3DData.lastLoaded;
                                if (range_updated) UpdateDraw();
                                else UpdateDraw(C3DData.lastLoaded);

                            }
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        private void loadCylinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Cylinder data file(*.dat,*.txt)|*.dat;*.txt|all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (C3DData.LoadCylinderFile(dlg.FileName))
                        {
                            m_ObjectForm.AddToTree(C3DData.lastLoaded);
                            C3DData.objSelected = C3DData.lastLoaded;
                            UpdateDraw(C3DData.lastLoaded);
                        }
                        else MessageBoxErr(C3DData.errMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        private void loadMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.MeshFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    dlg.Multiselect = true;
                    string info = "" ;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        C3DData.lastLoadeds.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            if ( !C3DData.LoadMeshFile(dlg.FileNames[i]) )
                            {
                                info += dlg.FileNames[i] + " : " + C3DData.errMessage;
                                info += Environment.NewLine;
                            }
                        }

                        if ( info.Length > 0 && C3DData.lastLoadeds.Count > 0) 
                        { 
                            MessageBoxWarning(info, "Errors occurred!" );
                        }
                        else if (info.Length > 0 && C3DData.lastLoadeds.Count == 0)
                        {
                            MessageBoxErr(info, "Errors occurred!");
                        }

                        if (C3DData.lastLoadeds.Count > 0)
                        {
                            C3DData.UpdateRange();
                            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                            C3DData.objSelected = C3DData.lastLoaded;
                            UpdateDraw(C3DData.lastLoadeds);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            m_DDDForm.reset();
            m_DDDForm.Invalidate();
        }

        public void DockPlyProperty()
        {
            if (m_PlyPropertyForm == null) m_PlyPropertyForm = new PlyPropertyForm();
            if (m_PlyPropertyForm.IsDisposed) m_PlyPropertyForm = new PlyPropertyForm();
            m_PlyPropertyForm.Show(this.dockPanel1, DockState.DockLeft);
            m_PlyPropertyForm.UpdateList();
            m_PlyPropertyForm.Focus();
        }
        public void DockScattedPointsProperty()
        {            
            if (m_ScatterPointsPropertyForm == null) m_ScatterPointsPropertyForm = new ScatteredPointsPropertyForm();
            if (m_ScatterPointsPropertyForm.IsDisposed) m_ScatterPointsPropertyForm = new ScatteredPointsPropertyForm();
            m_ScatterPointsPropertyForm.Show(this.dockPanel1, DockState.DockRight);
            //m_ScatterPointsPropertyForm.UpdateList();
            m_ScatterPointsPropertyForm.Focus();
        }
        public void Dock3DGridProperty()
        {
            if (m_3DGridForm == null) m_3DGridForm = new C3DGridDataPropertyForm();
            if (m_3DGridForm.IsDisposed) m_3DGridForm = new C3DGridDataPropertyForm();
            m_3DGridForm.Show(dockPanel1, DockState.DockRight);
            m_3DGridForm.Focus();
        }
        public void DockSlicerProperty()
        {
            if (m_SlicerPropertyForm == null) m_SlicerPropertyForm = new CSlicerPropertyForm();
            if (m_SlicerPropertyForm.IsDisposed) m_SlicerPropertyForm = new CSlicerPropertyForm();
            m_SlicerPropertyForm.Show(dockPanel1, DockState.DockRight);
            m_SlicerPropertyForm.Focus();
        }
        public void DockBoreholesProperty()
        {
            if (m_BoreholesPropertyForm == null) m_BoreholesPropertyForm = new CBoreholesPropertyForm();
            if (m_BoreholesPropertyForm.IsDisposed) m_BoreholesPropertyForm = new CBoreholesPropertyForm();
            m_BoreholesPropertyForm.Show(dockPanel1, DockState.DockRight);
            m_BoreholesPropertyForm.Focus();
        }
        public void Dock3DLineProperty()
        {
            if (m_3DLinePropertyForm == null) m_3DLinePropertyForm = new C3DLinePropertyForm();
            if (m_3DLinePropertyForm.IsDisposed) m_3DLinePropertyForm = new C3DLinePropertyForm();
            m_3DLinePropertyForm.GetCurSelectedObject();
            m_3DLinePropertyForm.UpdateLength();
            m_3DLinePropertyForm.Show(dockPanel1, DockState.DockRight);
            m_3DLinePropertyForm.Focus();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Dock3DGridProperty();
        }
        private void SetChecked()
        {
            outerBoxToolStripMenuItem.Checked = C3DData.bShowOuterBox;
            directionArrowToolStripMenuItem.Checked = C3DData.bShowDirectionArrow;
            lightsPositionToolStripMenuItem.Checked = C3DData.bShowLightPositions;
            selectedOutlineToolStripMenuItem.Checked = C3DData.bShowSelectedOuterBox;
        }
        private void viewViewToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            SetChecked();
        }
        private void viewAsToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            SetChecked();
        }        
        private void fillToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (C3DData.nPolygonMode == gDrawMode.Fill)
                fillToolStripMenuItem.Checked = true;
            else fillToolStripMenuItem.Checked = false;
        }

        private void wireFrameToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (C3DData.nPolygonMode == gDrawMode.Wireframe)
                wireFrameToolStripMenuItem.Checked = true;
            else wireFrameToolStripMenuItem.Checked = false;            
        }

        private void fillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.nPolygonMode != gDrawMode.Fill )
            {
                C3DData.nPolygonMode = gDrawMode.Fill;
                m_DDDForm.DrawMode = C3DData.nPolygonMode;
                // m_DDDForm.UpdateDraw();
                UpdateDraw();
            }
        }

        private void wireFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_DDDForm.DrawMode != gDrawMode.Wireframe)
            {
                C3DData.nPolygonMode = gDrawMode.Wireframe;
                m_DDDForm.DrawMode = C3DData.nPolygonMode;
                //m_DDDForm.UpdateDraw();
                UpdateDraw();
            }
        }

        private void dGridPropertyToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (m_3DGridForm.IsDisposed) dGridPropertyToolStripMenuItem.Checked = false;
            else dGridPropertyToolStripMenuItem.Checked = true;
        }
        private void dGridPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dock3DGridProperty();
        }

        private void pLYPropertyToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (m_PlyPropertyForm.IsDisposed) pLYPropertyToolStripMenuItem.Checked = false;
            else pLYPropertyToolStripMenuItem.Checked = true;            
        }
        private void pLYPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockPlyProperty();
        }
        private void scattedPointsPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockScattedPointsProperty();
        }

        private void scattedPointsPropertyToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (m_ScatterPointsPropertyForm.IsDisposed) scattedPointsPropertyToolStripMenuItem.Checked = false;
            else scattedPointsPropertyToolStripMenuItem.Checked = true;
        }
        private void slicerPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_SlicerPropertyForm.IsDisposed) slicerPropertyToolStripMenuItem.Checked = false;
            else slicerPropertyToolStripMenuItem.Checked = true;
        }
        private void slicerPropertyToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            DockSlicerProperty();
        }

        private void boreholesPropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DockBoreholesProperty();
        }
        private void boreholesPropertyToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (m_BoreholesPropertyForm.IsDisposed) boreholesPropertyToolStripMenuItem.Checked = false;
            else boreholesPropertyToolStripMenuItem.Checked = true;
        }

        private void linePropertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dock3DLineProperty();
        }
        private void linePropertyToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (m_3DLinePropertyForm.IsDisposed) linePropertyToolStripMenuItem.Checked = false;
            else linePropertyToolStripMenuItem.Checked = true;
        }

        private void lightsMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_DDDForm.LightAndMaterialSet();
        }

        private void outerBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            C3DData.bShowOuterBox = !C3DData.bShowOuterBox;
            UpdateDraw(false, 1);
        }        
        private void directionArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //对象类型，0 -C3DObjectBase对象，1虚线框，2坐标轴箭头，3Lights位置
            C3DData.bShowDirectionArrow = !C3DData.bShowDirectionArrow;
            UpdateDraw(false,2);            
        }

        private void lightsPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            C3DData.bShowLightPositions = !C3DData.bShowLightPositions;
            UpdateDraw(false, 3);
        }
        private void selectedOutlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            C3DData.bShowSelectedOuterBox = !C3DData.bShowSelectedOuterBox;
            UpdateDraw(false, 4);
        }
        private void vRMLModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objectsDiction.Count < 1)
            {
                MessageBoxInfo("No models presented.");
                return;
            }
            
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.VRMLFileFormatFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    string pathname = dlg.FileName;
                    if (VRML.ExportVRML(pathname, C3DData.GetObjects()))
                    {
                        MessageBoxInfo("Data exported to file:" + pathname);
                    }
                    else
                    {
                        MessageBoxErr("Failed to export data to file: " + pathname);
                    }
                    this.Cursor = DefaultCursor;
                }
            }
        }
        //export as ply model
        private void pLYFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if( C3DData.objSelected == null )
            {
                MessageBoxInfo("Please select an object.");
                return;
            }
            string pathname = "";
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.PLYFileFormatedFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                dlg.FileName = C3DData.objSelected.Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    pathname = dlg.FileName;
                    string ext = Path.GetExtension(pathname);
                    ext = ext.ToLower();
                    if (ext != ".ply") pathname += ".ply";
                }
                else return;
            }
            if (C3DData.objSelected.type == ShapeEnum.Mesh)
            {
                CMesh mesh = (CMesh)C3DData.objSelected;
                TriangleObj tri = mesh.toTriangleObj();
                if (tri.SaveAsPLY(pathname))
                {
                    MessageBoxInfo("Export data successfully.");
                }
                else
                {
                    MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
                }
                tri.Clear();
            }
            if ( C3DData.objSelected.type == ShapeEnum.Triangles)
            {
                TriangleObj tri = (TriangleObj)C3DData.objSelected;
                if( tri.SaveAsPLY(pathname) )
                {
                    MessageBoxInfo("Export data successfully.");
                }
                else
                {
                    MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
                }
            }
            if (C3DData.objSelected.type == ShapeEnum.Grid3D)
            {
                C3DGridData obj = (C3DGridData)C3DData.objSelected;
                //only path name
                if ( obj.SaveAsPly(pathname) ) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + obj.errMessage);
            }
        }
        private void dataRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CModelRangeForm dlg = new CModelRangeForm();
            if( dlg.ShowDialog() == DialogResult.OK )
            {
                //m_DDDForm.UpdateDraw();
                UpdateDraw();
            }
        }
        void ExportAsPLY(string pathname, C3DObjectBase obj)
        {
            if (obj.type == ShapeEnum.Triangles)
            {
                TriangleObj tri = (TriangleObj)obj;
                if (tri.SaveAsPLY(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Mesh)
            {
                TriangleObj tri = ((CMesh)obj).toTriangleObj();
                if (tri.SaveAsPLY(pathname)) MessageBoxInfo("export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Slicer)
            {
                TriangleObj tri = ((CSlicer)obj).toTriangleObj();
                if (tri.SaveAsPLY(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Grid3D)
            {
                C3DGridData data = (C3DGridData)obj;
                //only path name,export multiple models
                if ( data.SaveAsPly(pathname) )MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + data.errMessage);
            }
        }
        void ExportAsSTL(string pathname, C3DObjectBase obj)
        {
            if (obj.type == ShapeEnum.Triangles)
            {
                TriangleObj tri = (TriangleObj)obj;
                //tri.TriangleEdgesVerify(0.01);
                //tri.TriangleVerticsVerify();

                if (tri.SaveAsSTL(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Triangles)
            {
                TriangleObj tri = ((CMesh)obj).toTriangleObj();
                if (tri.SaveAsSTL(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
                
            if (obj.type == ShapeEnum.Grid3D)
            {
                C3DGridData data = (C3DGridData)obj;
                //only path name,export multiple models
                if ( data.SaveAsSTL(pathname) )
                    MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + data.errMessage);
            }
        }
        void ExportAsOBJ(string pathname, C3DObjectBase obj)
        {
            if (obj.type == ShapeEnum.Triangles)
            {
                TriangleObj tri = (TriangleObj)obj;
                if (tri.SaveAsOBJ(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Mesh)
            {
                TriangleObj tri = ((CMesh)obj).toTriangleObj();
                if (tri.SaveAsOBJ(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Grid3D)
            {
                C3DGridData data = (C3DGridData)obj;
                //only path name,export multiple models
                if (data.SaveAsOBJ(pathname))
                    MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + data.errMessage);
            }
        }
        void ExportAsVRML(string pathname, C3DObjectBase obj)
        {
            if (obj.type == ShapeEnum.Triangles)
            {
                TriangleObj tri = (TriangleObj)obj;
                if (tri.SaveAsVRML(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Mesh)
            {
                TriangleObj tri = ((CMesh)obj).toTriangleObj();
                if (tri.SaveAsVRML(pathname)) MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + tri.errMessage);
            }
            if (obj.type == ShapeEnum.Grid3D)
            {
                C3DGridData data = (C3DGridData)obj;
                //only path name,export multiple models
                if (data.SaveAsVRML(pathname))
                    MessageBoxInfo("Export data successfully.");
                else MessageBoxErr("Failed to export file." + Environment.NewLine + data.errMessage);
            }
        }
        public void DoExportTriangle(string path, C3DObjectBase obj)
        {
            string ext = Path.GetExtension(path).ToLower();

            if (ext == ".ply")
            {
                ExportAsPLY(path, obj);
            }
            else if (ext == ".stl")
            {
                if (MessageBoxQestionYesNo("Warning: Color would be lost save as this format.", "Continue?") == DialogResult.Yes)
                    ExportAsSTL(path, obj);
            }
            else if (ext == ".obj")
            {
                if (MessageBoxQestionYesNo("Warning: Color would be lost save as this format.", "Continue?") == DialogResult.Yes)
                    ExportAsOBJ(path, obj);
            }
            else if (ext == ".wrl")
            {
                ExportAsVRML(path, obj);
            }
        }
        //创建二维轮廓PolygonSlicer
        private void dOutlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PolygonSlicer obj = new PolygonSlicer();
            obj.minx = 0;
            obj.maxx = 1000;
            obj.miny = 0;
            obj.maxy = 800;
            obj.minz = obj.miny;
            obj.maxz = obj.maxy;
            
            obj.minz = obj.miny;
            obj.maxz = obj.maxy;

            SlicerModelingForm sf = new SlicerModelingForm();
            sf.SetSlicer(obj);
            if (sf.ShowDialog() == DialogResult.OK)
            {
                obj = sf.slicer;
                if (obj.IsLocated)
                {
                    obj.UpdateTraced();
                    CDataModel.m_ModelOrg = new CubeModel64(-1, -1, -1, 1, 1, 1);
                    CDataModel.UpdateModelSize(obj);
                }

                //obj.UpdateRange();
                bool range_updated = C3DData.AddObject(obj, true);
                C3DData.objSelected = C3DData.lastLoaded;

                m_ObjectForm.AddToTree(obj);

                if (range_updated) UpdateDraw();
                else UpdateDraw(C3DData.lastLoaded);
            }
        }

        private void coneToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            
            double rad = 0.25*Math.Min(CDataModel.m_ModelOrg.GetWidth(0), CDataModel.m_ModelOrg.GetWidth(1));
            double height = 0.5 * CDataModel.m_ModelOrg.GetWidth(2);
            Vector32 p0 = CDataModel.GetCenterPoint32();

            Cone cone = new Cone(p0,rad, height);
            cone.Name = "Cone" + iConeCreated;
            iConeCreated++;
            cone.UpdateRange();

            bool range_updated = C3DData.AddObject(cone,false);
            C3DData.objSelected = C3DData.lastLoaded;
            m_ObjectForm.AddToTree(cone);
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoaded);
        }
        private void cylinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //CreateCylinderForm cy = new CreateCylinderForm();
            //if (cy.ShowDialog() == DialogResult.OK)
            //{
            //    CCylinder cylinder = new CCylinder();
            //    cylinder.Create(cy.radius, cy.height, cy.vertNum, cy.horNum, cy.xlocation, cy.ylocation, cy.zlocation);

            //    C3DData.AddObject(cylinder);

            //    m_ObjectForm.UpdateTree();
            //    UpdateDraw();
            //}
            double rad = 0.25 * Math.Min(CDataModel.m_ModelOrg.GetWidth(0), CDataModel.m_ModelOrg.GetWidth(1));
            double height = 0.5 * CDataModel.m_ModelOrg.GetWidth(2);
            Vector32 p0 = CDataModel.GetCenterPoint32();

            CCylinderExt cylinder = new CCylinderExt(p0, rad, height);
            cylinder.Name = "Cylinder" + iCylinderCreated;
            iCylinderCreated++;
            cylinder.UpdateRange();

            bool range_updated = C3DData.AddObject(cylinder, false);
            C3DData.objSelected = C3DData.lastLoaded;
            m_ObjectForm.AddToTree(cylinder);
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoaded);
        }
        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void boxToolStripMenuItem_Click(object sender, EventArgs e)
        {   
            double xx = 0.5 * CDataModel.m_ModelOrg.GetWidth(0);
            double yy = 0.5 * CDataModel.m_ModelOrg.GetWidth(1);
            double zz = 0.5 * CDataModel.m_ModelOrg.GetWidth(2);
            Vector32 p0 = CDataModel.GetCenterPoint32();
            
            Box3D obj = new Box3D(p0, xx,yy,zz);
            obj.Name = "Box" + iBoxCreated;
            iBoxCreated++;

            obj.UpdateRange();
            bool range_updated = C3DData.AddObject(obj,false);
            C3DData.objSelected = C3DData.lastLoaded;
            m_ObjectForm.AddToTree(obj);
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoaded);
        }
        
        private void CombineUncertainty(string grid1, string grid2,string outgrid)
        {
            C3DGridData data1 = new C3DGridData();
            data1.LoadFrom(grid1);

            C3DGridData data2 = new C3DGridData();
            data2.LoadFrom(grid2);

            int nx = data1.xNum;
            int ny = data1.yNum;
            int nz = data1.zNum;

            //combine 2 uncertainties
            long id;
            double v1, v2, v;
            double minv = 999999, maxv = -999999;
            for (int iz = 0; iz < nz; iz++)
              for (int iy = 0; iy < ny; iy++)
                for (int ix = 0; ix < nx; ix++)
                {
                        id = ix + iy * nx + iz * nx * ny;
                        //normalized v1 0 - 1
                        v1 = data1.pGridData[id];
                        v1 = (v1 - data1.minv) / (data1.maxv - data1.minv);
                        //normalized v2 0 - 1
                        v2 = data2.pGridData[id];
                        v2 = (v2 - data2.minv) / (data2.maxv - data2.minv);

                        v =(float)Math.Sqrt(v1* v1 + v2*v2);
                        if (v < minv) minv = v;
                        if (v > maxv) maxv = v;
                        data1.pGridData[id] = (float)v;
                 }

            //normalized to 0 - 1
            for(long i=0;i<nx*ny*nz;i++)
            {
                v = data1.pGridData[i];
                v = (data1.pGridData[i] - minv) / (maxv - minv);
                data1.pGridData[i] = (float)v;                
            }

            data1.minz = 60;
            data1.maxz = 190;
            data1.minv = 0;
            data1.maxv = 1;
            data1.SaveAs(outgrid);
        }
        //data test- convert P32 fracture to 3DGrid format
        public bool ConvertP32toGrid(string path)
        {
            BinaryReader br;
            try
            {
                br = new BinaryReader(new FileStream(path, FileMode.Open));
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (IOException e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {                
                return false;
            }

            C3DGridData data = new C3DGridData();            
            int xGrid = (int)br.ReadDouble();
            int yGrid = (int)br.ReadDouble();
            int zGrid = (int)br.ReadDouble();
            data.xNum = xGrid;
            data.yNum = yGrid;
            data.zNum = zGrid;
            if (xGrid <= 0 || yGrid <= 0 || zGrid <= 0)
            {
                br.Close();
                return false;
            }
            Vector32 p;
            List<Vector32> points = new List<Vector32>();

            for (int i = 0; i < zGrid * yGrid * xGrid; i++)
            {
                p = new Vector32(0,0,0);
                p.x = (float)br.ReadDouble();
                points.Add(p);
            }
            for (int i = 0; i < zGrid * yGrid * xGrid; i++)
            {
                p = points[i];
                p.y = (float)br.ReadDouble();
                points[i] = p;
            }
            for (int i = 0; i < zGrid * yGrid * xGrid; i++)
            {
                p = points[i];
                p.z = (float)br.ReadDouble();
                points[i] = p;
            }            
            for (int i = 0; i < zGrid * yGrid * xGrid; i++)
            {
                p = points[i];
                p.v = (float)br.ReadDouble();
                points[i] = p;
            }
            br.Close();

            /////////////////////////////////////
            data.pGridData = new float[zGrid * yGrid * xGrid];
            for (int i = 0; i < zGrid * yGrid * xGrid; i++)
            {
                data.pGridData[i] = points[i].v;
            }

            //update data range
            double x, y, z, v;            
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                x = p.x;
                y = p.y;
                z = p.z;
                v = p.v;
                if (i == 0)
                {
                    data.minx = data.maxx = x;
                    data.miny = data.maxy = y;
                    data.minz = data.maxz = z;
                    data.minv = data.maxv = v;
                }
                else
                {
                    if (x < data.minx) data.minx = x;
                    if (y < data.miny) data.miny = y;
                    if (z < data.minz) data.minz = z;
                    if (v < data.minv) data.minv = v;
                    if (x > data.maxx) data.maxx = x;
                    if (y > data.maxy) data.maxy = y;
                    if (z > data.maxz) data.maxz = z;
                    if (v > data.maxv) data.maxv = v;
                }
            }

            data.SaveAs(path + ".3DGrid");

            //Export3DGrid(path+".3DGrid");

            return true;
        }
     
        //data test- get gsr value on p32 position , ix,iy,iz is position in p32 
        private double GetFromGSRValue(int ix,int iy,int iz, C3DGridData data)
        {
            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;
            int iy1 = ix;
            int ix1 = nx -1 - iy;
            int iz1 = iz;
            long id = ix1 + iy1 * nx + iz1 * nx * ny;
            return data.pGridData[id];
        }
        //data test- get p32 value on gsr position , ix,iy,iz is position in gsr 
        private double GetFromP32Value(int ix, int iy, int iz, C3DGridData data)
        {
            int nx = data.xNum;
            int ny = data.yNum;
            int nz = data.zNum;
            int iy1 = ny-1 - ix;
            int ix1 = iy;
            int iz1 = iz;
            long id = ix1 + iy1 * nx + iz1 * nx * ny;
            return data.pGridData[id];
        }
        //data test- combine P32 from GSR,P32 = ( GSR + P32 ) / 2
        private void CombineP32FromGSR(string gsr, string p32)
        {
            C3DGridData d1 = new C3DGridData();
            d1.LoadFrom(p32);            

            C3DGridData d2 = new C3DGridData();
            d2.LoadFrom(gsr);
            int ny = d2.xNum;
            int nx = d2.yNum;
            int nz = d2.zNum;
            d1 = d1.ResizeGrid(nx,ny,nz);

            C3DGridData d = new C3DGridData();
            d.xNum = nx;
            d.yNum = ny;
            d.zNum = nz;
            d.minx = d1.minx;
            d.miny = d1.miny;
            d.minz = d1.minz;
            d.maxx = d1.maxx;
            d.maxy = d1.maxy;
            d.maxz = d1.maxz;
            d.pGridData = new float[nx * ny * nz];
            double v1,v2;
            long id;
            for (int iz = 0; iz < nz; iz++)            
                for (int iy = 0; iy < ny; iy++)
                    for (int ix = 0; ix < nx; ix++)
                    {
                        id = ix + iy * nx + iz * nx * ny;

                        v1 = d1.pGridData[id]; //p32
                        v1 = (v1 - d1.minv) / (d1.maxv - d1.minv);//normalized to 0 - 1

                        v2 = GetFromGSRValue(ix,iy,iz,d2);//gsr
                        v2 = (v2 - d2.minv) / (d2.maxv - d2.minv);//normalized to 0 - 1

                        d.pGridData[id] = (float)((1-v1) * 0.1 + 0.9 * v2 );
                    }
            d.UpdateDataRange();
            d.minv = 0;
            d.maxv = 1;
            d.ColorScale.SetValueRange(0, 1);
            d.SaveAs(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\\Combination_P32meanFromGSR.3DGrid");
        }
        //data test- combine GSR from P32 Fracture GSR = (GSR+P32)/2
        private void CombineGSRFromP32(string gsr,string p32)
        {
            C3DGridData d1 = new C3DGridData();
            d1.LoadFrom(p32);

            C3DGridData d2 = new C3DGridData();
            d2.LoadFrom(gsr);

            int nx = d2.xNum;
            int ny = d2.yNum;
            int nz = d2.zNum;
            d1 = d1.ResizeGrid(ny, nx, nz);

            C3DGridData d = new C3DGridData();
            d.xNum = nx;
            d.yNum = ny;
            d.zNum = nz;
            d.minx = d2.minx;
            d.miny = d2.miny;
            d.minz = d2.minz;
            d.maxx = d2.maxx;
            d.maxy = d2.maxy;
            d.maxz = d2.maxz;
            d.pGridData = new float[nx*ny*nz];
            double v1, v2;
            long id;
            for (int iz = 0; iz < nz; iz++)
                for (int iy = 0; iy < ny; iy++)
                    for (int ix = 0; ix < nx; ix++)
                    {
                        id = ix + iy * nx + iz * nx * ny;

                        v1 = GetFromP32Value(ix, iy, iz, d1);//p32
                        v1 = (v1 - d1.minv) / (d1.maxv - d1.minv);//normalized to 0 - 1

                        v2 = d2.pGridData[id];  //gsr
                        v2 = (v2 - d2.minv) / (d2.maxv - d2.minv);//normalized to 0 - 1

                        d.pGridData[id] = (float)( (1-v1) * 0.1  +  0.9 * v2 );
                    }
            d.minv = 0;
            d.maxv = 1;
            d.ColorScale.SetValueRange(0, 1);
            //d.UpdateDataRange();
            d.SaveAs(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\\Combination_GSRFromP32mean-1.3DGrid");
        }
        private void CombineGSRFromP32_1(string gsr, string p32)
        {
            double gsr1, gsr2;
            C3DGridData d1 = new C3DGridData();
            d1.LoadFrom(gsr);
            gsr1 = d1.minv;
            gsr2 = d1.maxv;
            d1.NormalizeGrid(0,1);
            d1.scale.y = 0.5f;

            C3DGridData d2 = new C3DGridData();            
            d2.LoadFrom(p32);
            d2.NormalizeGrid(0,1);
            d2.rotate.z = 50;
            d2.scale = new GlmNet.vec3(4,4,2);            
            d2.offset = new GlmNet.vec3(150,2400,130);
                        
            int nx1 = d1.xNum;
            int ny1 = d1.yNum;
            int nz1 = d1.zNum;
            int nx2 = d2.xNum;
            int ny2 = d2.yNum;
            int nz2 = d2.zNum;

            //d1-GSR d2 - p32
            double v , v1, v2;
#pragma warning disable CS0168 // 声明了变量“id2”，但从未使用过
            long id1,id2;
#pragma warning restore CS0168 // 声明了变量“id2”，但从未使用过
            Vector64 p1, p2;
#pragma warning disable CS0168 // 声明了变量“z2”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“y2”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“x2”，但从未使用过
            double x1, y1, z1, x2, y2, z2;
#pragma warning restore CS0168 // 声明了变量“x2”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“y2”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“z2”，但从未使用过
            for (int iz = 0; iz < nz1; iz++)
                for (int iy = 0; iy < ny1; iy++)
                    for (int ix = 0; ix < nx1; ix++)
                    {
                        id1 = ix + iy * nx1 + iz * nx1 * ny1;
                        v1 = d1.GetGridValue(id1);
                        x1 = d1.minx + d1.xStep * ix;
                        y1 = d1.miny + d1.yStep * iy;
                        z1 = d1.minz + d1.zStep * iz;

                        //transform to real coord
                        p1 = new Vector64(x1,y1,z1);
                        p1 = d1.TransformedPoint(p1);

                        p2 = d2.UnTransformedPoint(p1);

                        //not in P32 range
                        if (!d2.IsInRange(p2.x, p2.y, p2.z))
                        {
                            v2 = 0;
                            //continue;
                        }
                        else
                        {
                            v2 = d2.GetGridValue(p2.x, p2.y, p2.z, false, false);
                            //id2 = d2.GetVerticIndexByPosition(p2.x, p2.y, p2.z);
                        }

                        v = (float)(v1 * 0.9 + 0.1 * (1 - v2));
                        d1.pGridData[id1] = (float)v;
                    }

            d2.Clear();
            d2 = new C3DGridData();
            d2.LoadFrom(gsr);
            d2.NormalizeGrid(0, 1);
            for (long i = 0; i < nx1 * ny1 * nz1; i++)
            {
                d2.pGridData[i] =(float)( d2.pGridData[i] * 0.9 + 0.1 );
            }
            d2.UpdateRange();

            d1.minv = d2.minv;
            d1.maxv = d2.maxv;

            d1.NormalizeGrid(0,1);
            d2.NormalizeGrid(0,1);

            //for(long i=0;i<nx1*ny1*nz1;i++)
            //{
            //    v = d1.pGridData[i];
            //    d1.pGridData[i] = (float)(gsr1 + v * (gsr2 - gsr1));
            //}
            //d1.minv = gsr1;
            //d1.maxv = gsr2;
            //d1.m_ColorScale.SetValueRange(0, 1);
            //d1.UpdateDataRange();
            d1.SaveAs(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\\Combination_GSRFromP32mean-2.3DGrid");
            d2.SaveAs(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\\GSR-normalized.3DGrid");
        }
       
        private void YaTaMTConvert(string invFileL1, string coordFileL1, string outFileL1,double xstep = 2,double zstep = 1)
        {
            //反演剖面数据 ->data
            //#X	Z	log_Rho	Rho	dist(km)	y(km)
            FileStream fs = new FileStream(invFileL1, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            List<Vector64> data = new List<Vector64>();

            int xNum = 0;
            int zNum = 0;
            //first row
            string line = sr.ReadLine();
            double x, y, z, v;
            double x1 = -999999;
            double z1 = -999999;
            int k = 0;
            double firstx = -99999;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim(' ');
                if (line.Length < 1) continue;
                string[] ss = line.Split(new char[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                x = double.Parse(ss[4])*1000; //km*1000
                y = 0;
                z = double.Parse(ss[1]);//m
                v = double.Parse(ss[3]); //Rho
                data.Add(new Vector64(x,y,z,v));

                if (k == 0) { firstx = x; }

                if (x == firstx)
                {
                    if (z != z1) zNum++;
                    z1 = z;
                }

                if (x != x1) xNum++;

                x1 = x;

                k++;
            }
            sr.Close();
            fs.Close();

            //"测点距", "高程", "测点名","东向坐标(投影)", "北向坐标(投影)", "东向坐标(实际)", "北向坐标(实际)"
            List<Vector64> coords = new List<Vector64>();
            fs = new FileStream(coordFileL1, FileMode.Open, FileAccess.Read);
            sr = new StreamReader(fs);
            //first row
            line = sr.ReadLine();
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim(' ');
                if (line.Length < 1) continue;
                string[] ss = line.Split(new char[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                x = double.Parse(ss[3]);
                y = double.Parse(ss[4]);                
                z = double.Parse(ss[1]);//m
                v = double.Parse(ss[0]);//m dist
                coords.Add(new Vector64(x, y,z,v));
            }
            sr.Close();
            fs.Close();

            bool interpolate = false;

            Vector64 p,p0;
            for(int i=0;i<data.Count;i++)
            {
                p = data[i];
                
                interpolate = false;

                if (i == 0) interpolate = true;
                else 
                {
                    p0 = data[i-1];
                    if( p0.x != p.x) interpolate = true;
                }
                if(interpolate)p0 = GetCoordInterpolated(p.x,coords);
                else p0 = data[i - 1];

                p.x = p0.x;
                p.y = p0.y;
                data[i] = p;
            }

            //write
            fs = new FileStream(outFileL1, FileMode.Create, FileAccess.Write);
            StreamWriter wr = new StreamWriter(fs);

            line = "X(m),   Y(m),   Z(m),   Rho";
            wr.WriteLine(line);
            for ( int i = 0; i < data.Count; i++ )
            {
                p = data[i];
                line = p.X + "," + p.Y + "," + p.Z + "," + p.V;
                wr.WriteLine(line);
            }
            wr.Close();
            fs.Close();
            
        }
        Vector64 GetCoordInterpolated(double dist,List<Vector64>coords)
        {

            int n = coords.Count;

            if (dist <= coords[0].v) 
            {
                return new Vector64(coords[0].x, coords[0].y, coords[0].z);
            }
            if (dist >= coords[n-1].v)
            {
                return new Vector64(coords[n-1].x, coords[n-1].y, coords[n-1].z);
            }

            int id1 = 0, id2 = 0;
            //Match coord
            for (int i = 0; i < coords.Count; i++)
            {
                if (dist >= coords[i].v) { id1 = i; break; }
            }

            id2 = id1;
            if ( id1 < n - 1) id2 = id1 + 1;

            if( id1 == id2 ) return new Vector64(coords[id1].x, coords[id1].y, coords[id1].z);
            else
            {
                Vector64 p1 = coords[id1];
                Vector64 p2 = coords[id2];
                
                double scale = (dist - p1.v) / (p2.v - p1.v);

                return p1 + scale  * (p2 - p1);
            }
        }
        void ConvertXYZtoXYZV(string file,double filter = 0.3)
        {
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            List<Vector64> data = new List<Vector64>();

#pragma warning disable CS0219 // 变量“xNum”已被赋值，但从未使用过它的值
            int xNum = 0;
#pragma warning restore CS0219 // 变量“xNum”已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量“zNum”已被赋值，但从未使用过它的值
            int zNum = 0;
#pragma warning restore CS0219 // 变量“zNum”已被赋值，但从未使用过它的值
            //first row
            string line = sr.ReadLine();
            double x, y, z, v;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim(' ');
                if (line.Length < 1) continue;
                string[] ss = line.Split(new char[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                x = double.Parse(ss[0]);//x
                y = double.Parse(ss[1]);//y
                v = double.Parse(ss[2]); //v
                if (v <= filter) continue;
                data.Add(new Vector64(x, y, 0, v));
            }
            sr.Close();
            fs.Close();

            string dstFile = file + ".dat";
            fs = new FileStream(dstFile, FileMode.Create, FileAccess.Write);
            StreamWriter wr = new StreamWriter(fs);

            line = "x,  y,  z,  value";
            wr.WriteLine(line);
            double z1 = -3520.97705078125;
            double z2 = 1588;
            double step = (z2 - z1) / 100;
            Vector64 p;
            for(int k=0;k<100;k++)
            {
                z = z1 + k * step;
                for(int i=0;i<data.Count;i++)
                {
                    p = data[i];
                    line = p.x + ", " + p.y + ",    " + z + ",    " + p.v;
                    wr.WriteLine(line);
                }
            }
            data.Clear();
            wr.Close();
            fs.Close();

        }
        void Convert2DGrd()
        {
            CSurferGrid cs = new CSurferGrid();
            cs.Read(@"D:\jian\项目\电法软件项目\高密度电法虚拟实验\model\models\模型1\dem.grd");
            cs.minx = 0;
            cs.maxx = 2000;
            cs.miny = 0;
            cs.maxy = 2000;
            double z1 = cs.minv;
            double z2 = cs.maxv;
            float v, v1 = 0, v2 = 0;
            for (int i = 0; i < cs.pData.Length; i++)
            {
                v = cs.pData[i];
                v = (float)(800 + (v - z1) / (z2 - z1) * 200);
                cs.pData[i] = v;
                if (i == 0) v1 = v2 = v;
                else
                {
                    if (v < v1) v1 = v;
                    if (v > v2) v2 = v;
                }
            }
            cs.minv = v1;
            cs.maxv = v2;
            cs.SaveAs(@"D:\jian\项目\电法软件项目\高密度电法虚拟实验\model\models\模型1\dem1.grd");
           
        }
        void Convert3DGrid()
        {
            C3DGridData data = new C3DGridData();
            data.LoadFrom(@"D:\jian\项目\电法软件项目\高密度电法虚拟实验\model\models\模型1\model1-1.3DGRID");
            float v = 100;

            long id1,id2;

            for(long id=0;id<data.pGridData.Length;id++)
            {
                  v = data.pGridData[id];
                  if (v < 100 )
                  {
                    int iz = (int)(id / data.xyNum);
                    int iy = (int)(id - iz * data.xyNum) / data.xNum;
                    int ix = (int)(id - iz * data.xyNum - iy * data.xNum);
                            //if (ix < 80)
                            {
                                int ix1 = ix + 10;
                               // if (ix1 < 100)
                                {
                                    data.pGridData[id] = 100;
                                    id1 = data.GetVerticIndex(ix1, iy, iz);
                                    data.pGridData[id1] = v;                                    
                                }
                            }
                            /*else
                            {
                                id2 = data.GetVerticIndex(data.xNum -ix - 10, iy, iz);
                                data.pGridData[id2] = v;
                            }*/
                              
                        }                        
                    }
                      
            data.UpdateRange();
            data.minx = 0;
            data.maxx = 2000;
            data.miny = 0;
            data.maxy = 2000;
            data.minz = 0;
            data.maxz = 1000;
            data.SaveAs(@"D:\jian\项目\电法软件项目\高密度电法虚拟实验\model\models\模型1\model1-3.3DGRID");
            
        }
        string toDMSFormat(double v)
        {
            Random rd = new Random(DateTime.Now.Millisecond);
            int d, m;
            double s;

            d = (int)v;
            m = (int)( (v - d) * 60 );
            s = v*3600 - d * 3600 - m * 60;

            s += rd.Next(1, 5);
            s = Math.Round(s,1);
            return d + ":" + m + ":" + s;            
        }
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            double rate = 3.55;
            int months = 240;
            double total = 800000;
            double sum = 0;
            double cur = total;
            
            double monthpay = total*rate/ 100/months + total/months;
            List<double> pays = new List<double>();
            
            for(int i = 0; i < months; i++ )
            {
                
                
            }
            string file = @"D:\jian\项目\电磁法勘探\2020地调项目\2022年给瞿程数据\MT实测点位坐标(1).txt";
            string outfile = @"D:\jian\项目\电磁法勘探\2020地调项目\2022年给瞿程数据\MT实测点位坐标(2).txt";
            StreamReader sr = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read));
            string line;
            string []ss;
            double x, y, z;
            string s1, s2;
            List<Vector64> points = new List<Vector64>();
            while( (line = sr.ReadLine())!= null )
            {
                ss = line.Split(new char[] {' ',',','\t' },StringSplitOptions.RemoveEmptyEntries);
                if(ss.Length > 2) 
                {
                    x = double.Parse(ss[1]);
                    y = double.Parse(ss[2]);
                    points.Add(new Vector64(x,y,0));
                }
            }
            sr.Close();

            Vector64 p;
            StreamWriter wr = new StreamWriter(new FileStream(outfile, FileMode.Create, FileAccess.Write));
            
            for( int i=0; i < points.Count; i++ )
            {
                p = points[i];
                line = (i + 1).ToString();
                line += "," + toDMSFormat(p.X);
                line += "," + toDMSFormat(p.Y);
                wr.WriteLine(line);
            }
            wr.Close();
            return;
            //
            this.Cursor = Cursors.WaitCursor;
            string bmpPath = @"D:\jian\项目\何兰方\基性超基性岩\videos\2\";
            Bitmap bmp = new Bitmap(bmpPath + "1.png");
            AviManager aviManager = new AviManager(bmpPath+"record.avi", false);
            VideoStream aviStream = aviManager.AddVideoStream(false,C3DData.framesPersecond, bmp);            
            for(int i=2;i<34;i++)
            {
                bmp = new Bitmap(bmpPath + i+ ".png");
                aviStream.AddFrame(bmp);
                bmp.Dispose();
            }

            aviManager.Close();
            this.Cursor = Cursors.Default;
            return;
            //
            this.Cursor = Cursors.WaitCursor;

            double angle = 0;
            for (int i = 0; i < 10; i++)
            {
                angle = 36 * i * 3.14159 / 180;
                m_DDDForm.graphic.SetTextureWaveStartTime((float)angle);
                m_DDDForm.UpdateView();
                System.Threading.Thread.Sleep(200);
            }

            //Convert3DGrid();
            //ConvertXYZtoXYZV(@"D:\jian\教学\研究生\祁红涛\简兴祥（2020.06.05）\丫他\data\化探\综合异常归一化.dat",0.3);

            /*
            //xnum 211 znum 47
            string invFileL1 = @"d:\L1.dat";
            string coordFileL1 = @"D:\L1-coord.dat";
            string outFileL1 = @"D:\jian\教学\研究生\祁红涛\简兴祥（2020.06.05）\丫他\data\电法\音、宽频大地电磁剖面反演成图\(L1线)丫他幅胡坝-丫他-板其-香甫宽频大地电磁测深剖面\L1反演剖面(XYZV).dat";

            //xnum 225 znum 47
            string invFileL2 = @"d:\L2.dat";
            string coordFileL2 = @"D:\L2-coord.dat"; 
            string outFileL2 = @"D:\jian\教学\研究生\祁红涛\简兴祥（2020.06.05）\丫他\data\电法\音、宽频大地电磁剖面反演成图\(L2线)丫他幅板年-尾远-坝细-尾哈宽频大地电磁测深剖面\L2反演剖面(XYZV).dat";
           // YaTaMTConvert(invFileL1, coordFileL1, outFileL1);
            YaTaMTConvert(invFileL2, coordFileL2, outFileL2);
            */
            /*
            //载入Shape文件，然后转出为高程数据
            Shapefile sp = new Shapefile();
            if( sp.Open(@"D:\jian\项目\地调院\王丽坤\dem\shape\dem.shp") )
            {

            }
            */
            /*
            //CalculateExponentionalUncertainty(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\All\GSR.3DGrid",
            //                                  @"C:\Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\Uncertainty_ExponentialFactor.3DGrid");

            //CombineUncertainty(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\Uncertainty_ExponentialFactor.3DGrid",
            //    @"C:\Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\CrossValidation_Uncertainty.3DGrid",
            //    @"C:\Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\Uncertainty_CrossValidation_And_ExponentialFactor.3DGrid");

            //ConvertP32toGrid(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\P32_100DFN.bin");
            //ConvertP32toGrid(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\P32var_100DFN.bin");            

            CombineGSRFromP32_1(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\All\GSR.3DGrid",
                              @"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\\P32_100DFN.bin.3DGrid");
            //CombineP32FromGSR(@"C:\Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\All\GSR.3DGrid",
            //                  @"C:\Users\XIN011\Documents\Source\Las data\Las 2017\P32\\P32_100DFN.bin.3DGrid");
            */
            this.Cursor = Cursors.Default;
            
            //MessageBox.Show("Done!");
        }

        private void importFromGeoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GeoInterConvertForm gcf = new GeoInterConvertForm();
            gcf.ShowDialog();
        }

        private void coordsRotatingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CoordsRotatingForm cf = new CoordsRotatingForm();
            cf.ShowDialog();
        }
        private void SaveGlobal(BinaryWriter br)
        {
            //data model
            CDataModel.SaveDataModel(br);
            //matrix
            m_DDDForm.graphic.m_modelMatrix.SaveAs(br);
            //global values
            br.Write(C3DData.bShowOuterBox);
            br.Write(C3DData.bShowDirectionArrow);
            br.Write(C3DData.bShowLightPositions);
        }
        private bool LoadGlobal(BinaryReader br)
        {
            try 
            {
                //data model
                CDataModel.LoadDataModel(br);
                //matrix
                m_DDDForm.graphic.m_modelMatrix.LoadFrom(br);
                //global values            
                C3DData.bShowOuterBox = br.ReadBoolean();
                C3DData.bShowDirectionArrow = br.ReadBoolean();
                C3DData.bShowLightPositions = br.ReadBoolean();
                if( C3DData.DataVersion >= 1.21f )//载入
                {

                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }            
        }
        bool SaveProject(string prjFile)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                BinaryWriter br = new BinaryWriter(new FileStream(prjFile, FileMode.Create));
                C3DData.dataTrees = m_ObjectForm.treeToStruct();

                if (!C3DData.SaveG3DFile(br))
                {
                    br.Close();
                    this.Cursor = DefaultCursor;
                    return false; 
                }

                SaveGlobal(br);
                br.Close();
                this.Cursor = DefaultCursor;
                return true;
            }
            catch (IOException ee)
            {
                this.Cursor = DefaultCursor;
                MessageBoxErr("Failed to save to file ." + Environment.NewLine + ee.Message);
                return false;
            }            
        }
       
        //save g3d to file
        private void SaveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.CurrentProjectFile.Length == 0)
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter = Resource1.G3dProjectFileFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    C3DData.CurrentProjectFile = dlg.FileName;
                }
            }

            if (C3DData.CurrentProjectFile.Length > 0)
            {
                if( SaveProject(C3DData.CurrentProjectFile) )
                {
                    AddtoInfo("Project Saved to " + C3DData.CurrentProjectFile);
                    Text = "3D Surfer - " + C3DData.CurrentProjectFile;
                }
                else
                {
                    AddtoInfo("Failed to save Project. ");
                }
            }                
        }

        private void OpenProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objectsDiction.Count > 0 && C3DData.IsDataModified)
            {
                string info = "Changes have not been saved, save it first?";
                DialogResult ret = MessageBoxQestionYesNoCancel(info, "Save Changes？");
                if (ret == DialogResult.Cancel) return;
                else if (ret == DialogResult.Yes)
                {
                    SaveProjectToolStripMenuItem_Click(sender, e);
                    return;
                }
            }

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.G3dProjectFileFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";

                if (dlg.ShowDialog() != DialogResult.OK) return;

                this.Cursor = Cursors.WaitCursor;
                
                bool ret = false;
                try
                {
                    BinaryReader br = new BinaryReader(new FileStream(dlg.FileName, FileMode.Open));
                    ret = C3DData.LoadG3DFile(br);
                    if (ret)
                    {
                        ret = LoadGlobal(br);
                        Text = "3D Surfer - " + dlg.FileName;
                        C3DData.CurrentProjectFile = dlg.FileName;
                    }

                    br.Close();
                }
                catch (IOException ee)
                {
                    MessageBoxErr("Loading data failed." + Environment.NewLine + ee.Message);
                    return;
                }               

                this.Cursor = DefaultCursor;

                if (ret)
                {                    
                    m_DDDForm.InitMatrix();
                    m_ObjectForm.CreateDataTrees();                   
                    m_ObjectForm.UpdateTree();                   
                    UpdateDraw();
                }
                else MessageBoxErr(C3DData.errMessage);
            }            
        }

        private void crossValidationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CrossValidationForm cf = new CrossValidationForm();
            cf.ShowDialog();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //m_DDDForm.UpdateDraw();
            UpdateDraw();
        }

        private void graphicDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GraphicDeviceDlg gld = new GraphicDeviceDlg();

            this.Cursor = Cursors.WaitCursor;

            CGraphic3D graphic = C3DData.graphics3D;
            gld.devices = graphic.GetGraphicsDeviceInfo();
            gld.driversinfo = graphic.GetGraphicsInfoString();
            gld.curName = graphic.GetGraphicName();
            //GraphicDeviceInfo info = graphics.currentGraphicDevice;
            
            this.Cursor = Cursors.Default;
            if ( gld.ShowDialog() == DialogResult.OK )
            {

            }
        }

        private void loadTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.BitmapFileFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        if( C3DData.LoadTexture(dlg.FileName) )
                        {
                            if( m_3DGridForm !=null &&  m_3DGridForm.Visible == true )
                            {
                                m_3DGridForm.UpdateTextureList();
                            }
                        }
                    }
                    catch (IOException ee)
                    {
                        MessageBoxErr("Loading texture failed." +Environment.NewLine + ee.Message);
                        return;
                    }
                    
                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void loadScatteredPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.XYZVFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    //dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        
                        ImportScatterPointsForm im = new ImportScatterPointsForm();
                        im.datafile = dlg.FileName;
                        if( im.ShowDialog() == DialogResult.OK )
                        {
                            if (im.output != null && im.output.Count > 1)
                            {
                                Cursor = Cursors.WaitCursor;
                                bool range_updated = C3DData.AddObject(im.output);                                
                                C3DData.objSelected = C3DData.lastLoaded;
                                m_ObjectForm.AddToTree(C3DData.lastLoaded);
                                Cursor = Cursors.Default;

                                if (range_updated) UpdateDraw();
                                else UpdateDraw(C3DData.lastLoaded);
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }            
        }

        private void fromLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LinesMeshForm lm = new LinesMeshForm();
            lm.ShowDialog();
        }
        //load formatted lines
        private void LoadFormattedLineFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string info = "";
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.LineFileFormatFilter;
                    dlg.Filter += "|" + Resource1.XYZVFormatLineFilter;
                    dlg.Filter += "|" + "All files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        C3DData.lastLoadeds.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            if ( !C3DData.LoadLineFile(dlg.FileNames[i]) )
                            {
                                info += dlg.FileNames[i] + "." + C3DData.errMessage;
                                info += Environment.NewLine;
                            }                            
                        }
                        if (info.Length > 1 ) MessageBoxWarning(info, "Loading data failed !!!");
                        if (C3DData.lastLoadeds.Count > 0)
                        {
                            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                            C3DData.objSelected = C3DData.lastLoaded;
                            UpdateDraw(C3DData.lastLoadeds);
                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        //load lines mesh
        private void outlinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "line file(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if ( C3DData.LoadMeshLines(dlg.FileNames) )
                        {
                            C3DData.objSelected = C3DData.lastLoaded;
                            m_ObjectForm.AddToTree(C3DData.lastLoaded);
                            UpdateDraw(C3DData.lastLoaded);
                        }
                        else MessageBoxErr(C3DData.errMessage);                        
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        private void lineFromSlicerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.SlicerFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    dlg.Multiselect = true;                    
                    string errinfo = "";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        List<C3DLine> lines = new List<C3DLine>();
                        C3DData.lastLoadeds.Clear();
                        bool range_updated = false;
                        foreach (string filename in dlg.FileNames)
                        {
                            CSlicer slicer = new CSlicer(100, 100);
                            if( slicer.LoadSlicer(filename) )
                            {
                                C3DLine line = new C3DLine(slicer.Name);
                                line.AddPoint(slicer.BaseLine);
                                line.UpdateRange();
                                if (C3DData.AddObject(line)) range_updated = true;                                
                            }
                            else
                            {
                                errinfo += filename + "." + C3DData.errMessage;
                                errinfo += Environment.NewLine;
                            }
                        }//foreach (string filename in dlg.FileNames)
                        if (errinfo.Length > 1)
                        {
                            MessageBoxWarning(errinfo);
                        }
                        if (lines.Count > 0)
                        {                           
                            m_ObjectForm.AddToTree(C3DData.lastLoaded);
                            C3DData.objSelected = C3DData.lastLoaded;
                            if (range_updated) UpdateDraw();
                            else UpdateDraw(C3DData.lastLoadeds);
                            lines.Clear();
                        }
                    }//if ( dlg.ShowDialog() )
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        private void SlicerFromFormattedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //CSlicer L4 = new CSlicer(100, 100);
            //L4.LoadSlicer(@"D:\jian\教学\研究生\张陈\应城\1.龙王集\高程调整后原始数据2022-4-7\高程调整后原始数据\L4-slicer.dat");
            //CSlicer L5 = new CSlicer(100, 100);
            //L5.LoadSlicer(@"D:\jian\教学\研究生\张陈\应城\1.龙王集\高程调整后原始数据2022-4-7\高程调整后原始数据\L5-slicer.dat");
            //CSlicer L7 = L4.GetInterpolatedSlicer(L5);
            //L7.Name = "L7";
            //L7.ExportSlicer(@"D:\jian\教学\研究生\张陈\应城\1.龙王集\高程调整后原始数据2022-4-7\高程调整后原始数据\L7-slicer.dat");
            //return;
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.SlicerFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    dlg.Multiselect = true;

                    string errinfo = "";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        C3DData.lastLoadeds.Clear();
                        foreach (string filename in dlg.FileNames)
                        {
                            if ( !C3DData.LoadSlicerFile(filename) )
                            {
                                errinfo += filename + ". " + C3DData.errMessage;
                                errinfo += Environment.NewLine;
                            }                            
                                             
                        }//foreach (string filename in dlg.FileNames)
                        if (errinfo.Length > 1)
                        {
                            MessageBoxWarning(errinfo);
                        }
                        if (C3DData.lastLoadeds.Count > 0)
                        {
                            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                            C3DData.objSelected = C3DData.lastLoaded;
                            UpdateDraw(C3DData.lastLoadeds);
                        }
                    }//if ( dlg.ShowDialog() )
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }
        private void fromGrid2DToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.SlicerFileFormatFilter;
                    dlg.Multiselect = true;

                    string errinfo = "";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        C3DData.lastLoadeds.Clear();
                        foreach (string filename in dlg.FileNames)
                        {
                            if (!C3DData.LoadSurferGridFile(filename))
                            {
                                errinfo += filename + ". " + C3DData.errMessage;
                                errinfo += Environment.NewLine;
                            }
                        }//foreach (string filename in dlg.FileNames)
                        if (errinfo.Length > 1)
                        {
                            MessageBoxWarning(errinfo);
                        }
                        if (C3DData.lastLoadeds.Count > 0)
                        {
                            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                            C3DData.objSelected = C3DData.lastLoaded;
                            UpdateDraw(C3DData.lastLoadeds);
                        }
                    }//if ( dlg.ShowDialog() )
                }
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }            
        }
                
        void AddPolygons( List<Polygon2D>polygons )
        {
            if (polygons.Count == 0) return;
            //add to an polygons existed
            if (C3DData.objSelected != null && C3DData.objSelected.type == ShapeEnum.Polygon2Ds)
            {
                C2DPolygons polys = C3DData.objSelected as C2DPolygons;
                polys.Polygons.AddRange(polygons);
                polys.UpdateRange();
                m_ObjectForm.UpdateTree(polys, 1);
                polys.RenderMode = RenderingUpdateMode.Redraw;
                UpdateDraw(polys);
                return;
            }
            else if (C3DData.objSelected != null &&  //polygon2D
                      C3DData.objSelected.type == ShapeEnum.Polygon2D)
            {
                if (C3DData.objSelected.Parent != null && //add to parent
                    C3DData.objSelected.Parent.type == ShapeEnum.Polygon2Ds)
                {
                    C2DPolygons polys1 = C3DData.objSelected.Parent as C2DPolygons;
                    polys1.Polygons.AddRange(polygons);
                    polys1.UpdateRange();
                    m_ObjectForm.UpdateTree(polys1, 2);
                    polys1.RenderMode = RenderingUpdateMode.Redraw;
                    UpdateDraw(polys1);
                    return;
                }
            }
            //else //其他情况一律添加在根目录
            {
                bool range_updated = false;
                if (polygons.Count == 1)//create single Polygon2D objects
                {
                    range_updated = C3DData.AddObject(polygons[0]);
                }
                else //Create C2DPolygons
                {
                    C2DPolygons polys = new C2DPolygons();
                    polys.Polygons.AddRange(polygons);
                    polys.UpdateRange();
                    range_updated = C3DData.AddObject(polys);
                }

                if (C3DData.objSelected == null)//add to directory                                
                    m_ObjectForm.AddToTree(C3DData.lastLoaded);
                else //add to root
                    m_ObjectForm.AddToTree(C3DData.lastLoaded, 2);
                if ( range_updated ) UpdateDraw();
                else UpdateDraw(C3DData.lastLoaded);
            }

        }
        //load polygons,single or multiple
        private void formattedPolygonFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string info = "";
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.Polygon2DFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        List<Polygon2D> polygons = new List<Polygon2D>();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            Polygon2D poly = new Polygon2D(dlg.FileNames[i]);
                            if (!poly.ImportData(dlg.FileNames[i]))
                            {
                                info += dlg.FileNames[i] + ":" + poly.errMessage;
                                info += Environment.NewLine;                                
                            }
                            else polygons.Add(poly);
                        }
                        
                        if (info.Length > 1 )
                        {
                            MessageBoxWarning(info);
                        }

                        if ( polygons.Count > 0 ) AddPolygons(polygons);
                        polygons.Clear();                                                
                        
                    }//if (dlg.ShowDialog() == DialogResult.OK)

                }//using (var dlg = new OpenFileDialog())
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
            }
        }

        //二进制的PolygonSlicer
        private void PolygonSlicerLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string info = "";
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.PoligonSlicerFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    dlg.Multiselect = true;
                    int no = 0;
                    if ( dlg.ShowDialog() == DialogResult.OK )
                    {
                        C3DData.lastLoadeds.Clear();
                        bool range_updated = false;
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            PolygonSlicer slicer = new PolygonSlicer();
                            slicer.Name = Path.GetFileNameWithoutExtension(dlg.FileNames[i]);                            
                            if ( !slicer.LoadFrom(dlg.FileNames[i]) )
                            {
                                info += dlg.FileNames[i] + ":" + slicer.errMessage;
                                info += Environment.NewLine;                                
                            }
                            else 
                            {
                                if (C3DData.AddObject(slicer)) range_updated = true;                                
                            }
                        }
                        
                        if (info.Length > 1) 
                        {
                            MessageBoxWarning(info);
                        }

                        if ( C3DData.lastLoadeds.Count > 0 )
                        {
                            C3DData.UpdateRange();
                            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                            if (range_updated) UpdateDraw();
                            else UpdateDraw(C3DData.lastLoadeds);

                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)

                }//using (var dlg = new OpenFileDialog())
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);                
            }
        }
        /// <summary>
        /// 从GIS交换文件中获取数据-Polygon2D 多边形或线
        /// </summary>
        /// <param name="index">1 DXF,2 SHP</param>
        /// <returns></returns>
        List<C2DPolygons> ImportFromGISFormatFiles( int index = 1 )
        {
            try
            {
                List<C2DPolygons> polygons = new List<C2DPolygons>();

                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.DXFFileFormatFilter;
                    dlg.Filter += "|" + Resource1.SHPFileFormatFilter;
                    dlg.Filter += "|" + "all files(*.*)|*.*";
                    dlg.FilterIndex = index;
                    dlg.Multiselect = true;
                    if ( dlg.ShowDialog() == DialogResult.OK )
                    {
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            C2DPolygons polys = new C2DPolygons(Path.GetFileNameWithoutExtension(dlg.FileNames[i]));

                            if (dlg.FilterIndex == 2)//SHP
                            {
                                polys.Polygons = C3DData.LoadShpFile(dlg.FileNames[i]);                                                               
                            }
                            else //if (dlg.FilterIndex == 1)DXF 默认
                            {
                                polys.Polygons = C3DData.LoadDxfFile(dlg.FileNames[i]);                                                            
                            }
                            if (polys.Polygons == null)
                            {
                                polys.Clear();
                                polygons.Clear();
                                MessageBoxErr(dlg.FileName[i] + "." + C3DData.errMessage, "Failed to load data");
                                return null;
                            }
                            polys.UpdateRange();
                            polygons.Add(polys);
                        }                       
                        
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())

                return polygons;
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);
                return null;
            }
        }
        private void PolygonFromSHPFiles_Click(object sender, EventArgs e)
        {
            List<C2DPolygons> polygons = ImportFromGISFormatFiles(2);
            if (polygons == null) return;

            C3DData.lastLoadeds.Clear();
            bool range_updated = false;
            foreach (C2DPolygons polys in polygons)
            {
                PolygonSlicer slicer = new PolygonSlicer(polys.Name);
                foreach (Polygon2D p in polys.Polygons)
                    slicer.AddPolygon(p);
                slicer.UpdateRange();
                if (C3DData.AddObject(slicer)) range_updated = true;
            }
            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
            C3DData.objSelected = C3DData.lastLoaded;
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoadeds);
        }

        private void PolygonSlicerFromDXFFiles_Click(object sender, EventArgs e)
        {
            List<C2DPolygons> polygons = ImportFromGISFormatFiles(1);
            if (polygons == null) return;

            C3DData.lastLoadeds.Clear();
            bool range_updated = false;
            foreach (C2DPolygons polys in polygons)
            {
                if (C3DData.AddObject(polys)) range_updated = true;
                //PolygonSlicer slicer = new PolygonSlicer(polys.Name);
                //foreach (Polygon2D p in polys.Polygons)
                //    slicer.AddPolygon(p);

                //slicer.UpdateRange();

                //C3DData.AddObject(slicer);                
            }
            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
            C3DData.objSelected = C3DData.lastLoaded;
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoadeds);
        }

        private void linefromDXFItem_Click(object sender, EventArgs e)
        {
            List<C2DPolygons> polygons = ImportFromGISFormatFiles(1);
            if (polygons == null) return;
            
            C3DData.lastLoadeds.Clear();
            bool range_updated = false;
            foreach (C2DPolygons polys in polygons)
            {                
                foreach ( Polygon2D p in polys.Polygons )
                {
                    if (C3DData.AddObject(p.to3DLine())) range_updated = true;
                }
            }
            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
            C3DData.objSelected = C3DData.lastLoaded;
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoadeds);
        }

        private void linefromSHPItem_Click(object sender, EventArgs e)
        {
            List<C2DPolygons> polygons = ImportFromGISFormatFiles(2);
            if (polygons == null) return;
            C3DData.lastLoadeds.Clear();
            bool range_updated = false;
            foreach (C2DPolygons polys in polygons)
            {
                foreach (Polygon2D p in polys.Polygons)
                {
                    if (C3DData.AddObject(p.to3DLine())) range_updated = true;
                }
            }
            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
            C3DData.objSelected = C3DData.lastLoaded;
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoadeds);
        }
        private void scatteredPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GriddingForm gf = new GriddingForm();
            gf.ShowDialog();
        }        

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About a = new About();
            a.ShowDialog();
        }       

        public event EventHandler drawUpdateEvent; //绘图委托事件       

        //refreshAll - 重绘全部
        //对象类型，0 -C3DObjectBase对象，1虚线框，2坐标轴箭头，3Lights位置
        public void UpdateDraw(bool refreshAll, int type, C3DObjectBase obj = null) //update all object
        {
            drawUpdateEvent(this, new DrawUpdateEventArg(refreshAll, obj, type));
        }
        public void UpdateDraw() //update all object
        {
            drawUpdateEvent( this, new DrawUpdateEventArg(true,null,0) );
        }

        //update a single object, visible = false do not draw this object
        public void UpdateDraw(C3DObjectBase obj)
        {
            if (obj != null) 
            { 
                drawUpdateEvent(this, new DrawUpdateEventArg(false, obj, 0));
                if ( CDataModel.rangeUpdated )
                {
                    if(C3DData.bShowDirectionArrow) UpdateDraw(false, 2, null);
                    if (C3DData.bShowOuterBox) UpdateDraw(false, 1, null);
                    CDataModel.rangeUpdated = false;
                }
            }
        }
        public void UpdateDraw(List<C3DObjectBase> objects)
        {
            foreach (C3DObjectBase obj in objects)
                UpdateDraw(obj);
            if (CDataModel.rangeUpdated)
            {
                if (C3DData.bShowDirectionArrow) UpdateDraw(false, 2, null);
                if (C3DData.bShowOuterBox) UpdateDraw(false, 1, null);
                CDataModel.rangeUpdated = false;
            }
        }       
        
        public void UpdateView()//刷新显示
        {
            drawUpdateEvent(this, new DrawUpdateEventArg(false, null,0));            
        }         

        private void cutingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Register Verify
            RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
            if (!reg.ReadFromRegister())
            {
                MessageBoxWarning("This is a unregistered version.");
                return;
            }
            Random rand = new Random();
            RegisterAndEncrypt.HardWareInfo.InfoType type = (RegisterAndEncrypt.HardWareInfo.InfoType)rand.Next(3);
            if (!reg.Verify(type))
            {
                MessageBoxWarning("Unreconginized register information.");
                return;
            }
            //Register Verify

            CutWithForm form1 = new CutWithForm();
            if (form1.ShowDialog() == DialogResult.OK)
            {
                if (form1.cutIndex >= 0 && form1.selectedTarget != null)
                {
                    C3DData.SetObjectByKey(form1.cutIndex,form1.selectedTarget);
                    C3DData.objSelected = form1.selectedTarget;                    
                    Program.m_MainForm.m_3DGridForm.UpdateColorScale();
                    // Program.m_MainForm.m_DDDForm.UpdateDraw();
                }
            }
        }

        private void withMeshesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IntersetWithMeshesForm dlg = new IntersetWithMeshesForm();
            if( dlg.ShowDialog() == DialogResult.OK )
            {
                C3DData.lastLoadeds.Clear();
                bool range_updated = false;
                for ( int i=0;i< dlg.Lines.Count;i++)
                {
                    if (C3DData.AddObject(dlg.Lines[i], false)) range_updated = true;
                }
                if (C3DData.lastLoadeds.Count > 0)
                {
                    m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                    C3DData.objSelected = C3DData.lastLoaded;
                    if (range_updated) UpdateDraw();
                    else UpdateDraw(C3DData.lastLoadeds);
                }
            }
        }


        //高斯消去法矩阵求逆，C#版本
        public double[] Inverse(double[]A,int row)
        {
            long id;
            double bs;
            //创建扩展矩阵E
            double[]E = new double[row * row];
            for (int i = 0; i < row * row; i++) E[i] = 0;
            for (int i = 0; i < row; i++) E[i * row + i] = 1;

            for (int k = 0; k < row; k++)
            {
                //第1步，k行k列变换为1
                id = k * row + k;
                bs = A[id];
                A[id] = 1;
                for (int j = k + 1; j < row; j++)
                {
                    id = k * row + j;
                    A[id] = (float)(A[id] / bs);
                }
                for (int j = 0; j < row; j++)
                {
                    id = k * row + j;
                    E[id] = (float)(E[id] / bs);
                }
                //第2步，i行（k以外其他行）k列变换为0
                for (int i = 0; i < row; i++)
                {
                    if (i != k)
                    {
                        bs = A[i * row + k];
                        for (int j = k; j < row; j++)
                        {
                            A[i * row + j] -= (float)(bs * A[k * row + j]);
                        }
                        for (int j = 0; j < row; j++)
                        {
                            E[i * row + j] -= (float)(bs * E[k * row + j]);
                        }
                    }
                }
            }
            //A变换为单位矩阵，E变换为逆矩阵
            return E;//返回逆矩阵
        }

        private void earthMappedSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CDataModel.IsEarthMapVision = !CDataModel.IsEarthMapVision;
            UpdateDraw();
        }

        private void coordinateToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            earthMappedSystemToolStripMenuItem.Checked = CDataModel.IsEarthMapVision;
        }

        private void slicersSamplingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SlicerModelingDlg md = new SlicerModelingDlg();
            /*
            foreach (C3DObjectBase obj in C3DData.pObjects)
            {
                if (obj.type == ShapeEnum.PolygonSlicer)
                {
                    PolygonSlicer s = (PolygonSlicer)obj;
                    md.AddSlicer(s);
                }
            }
            */
            if (md.ShowDialog() == DialogResult.OK)
            {
            }

            return;
        }

        private void mergeGridsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridsOverlapForm gm = new GridsOverlapForm();
            gm.ShowDialog();
        }

        private void meshesCreatingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MeshCreateFromSlicersForm mf = new MeshCreateFromSlicersForm();
            if( mf.ShowDialog() == DialogResult.OK)
            {
                m_ObjectForm.AddToTree(C3DData.lastLoaded);
                C3DData.objSelected = C3DData.lastLoaded;
                UpdateDraw(C3DData.lastLoaded);
            }
        }
        
        # region 常用对话框
        //-------常用对话框---------------------------
        public void MessageBoxErr(string errText, string captionText = "Error occurred!!!")
        {
            MessageBox.Show(errText, captionText, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void MessageBoxWarning(string errText, string captionText = "Warning!!!")
        {
            MessageBox.Show(errText, captionText, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public void MessageBoxInfo(string infoText, string captionText = "")
        {
            MessageBox.Show(infoText, captionText, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public DialogResult MessageBoxQestionYesNo(string infoText, string captionText = "", int defaultChoose = 1)
        {
            if( defaultChoose == 1)
            return MessageBox.Show( infoText, captionText, MessageBoxButtons.YesNo, 
                                    MessageBoxIcon.Question, 
                                    MessageBoxDefaultButton.Button1 );
            else return MessageBox.Show(infoText, captionText, MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2);
        }
        public DialogResult MessageBoxQestionYesNoCancel(string infoText, string captionText = "",int defaultChoose = 3)
        {
            if (defaultChoose == 1)
                return MessageBox.Show(infoText, captionText, MessageBoxButtons.YesNoCancel,
                                        MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button1);
            else if (defaultChoose == 2)
                return MessageBox.Show(infoText, captionText, MessageBoxButtons.YesNoCancel,
                                        MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button2);
            else
                return MessageBox.Show(infoText, captionText, MessageBoxButtons.YesNoCancel,
                                        MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button3);

        }
        //-------常用对话框---------------------------
        # endregion 

        private void buffersOverlapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BuffersOverlapForm bf = new BuffersOverlapForm();
            bf.ShowDialog();
        }

        private void fromGrid2DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.SurferGridFileFormatFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    CSurferGrid cs = new CSurferGrid();
                    if( !cs.Read(dlg.FileName) )
                    {
                        MessageBoxErr(cs.errMessage, "Loading data faild.");                        
                        return;
                    }
                    CMesh mesh = new CMesh();
                    mesh.fromGrid2D(cs);

                    bool range_updated = C3DData.AddObject(mesh);
                    C3DData.objSelected = C3DData.lastLoaded;
                    m_ObjectForm.AddToTree(mesh);
                    if (range_updated) UpdateDraw();
                    else UpdateDraw(C3DData.lastLoaded);
                    /*
                    MeshFromGridForm mf = new MeshFromGridForm();
                    mf.SetData(cs);
                    if( mf.ShowDialog() == DialogResult.OK)
                    {
                        m_ObjectForm.UpdateTree();
                        UpdateDrawList();
                    }*/
                }
            }
        }

        private void fromImageFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.BitmapFileFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp = new Bitmap(dlg.FileName);
                    CSurferGrid cs = new CSurferGrid();
                    cs.fromImage(bmp);

                    MeshFromGridForm mf = new MeshFromGridForm();
                    //mf.SetData(bmp,dlg.FileName);
                    mf.SetData(cs);
                    if (mf.ShowDialog() == DialogResult.OK)
                    {
                        m_ObjectForm.AddToTree(mf.mesh);
                        UpdateDraw(C3DData.lastLoaded);
                    }
                }
            }
        }

        // convert to earth texture
        private void toEarthTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConvertImageToEarthForm ce = new ConvertImageToEarthForm();
            ce.ShowDialog();
        }        

        private void loadScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {           
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Script File|*.script;*.txt;*.dat|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    m_DDDForm.DoScript(dlg.FileName);
                }
            }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.BitmapFileFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    
                    Bitmap bmp = m_DDDForm.GetScreenBitmap();
                    bmp.Save(dlg.FileName);                   

                    this.Cursor = DefaultCursor;
                }
            }            
        }

        //start or end recording
        private void startRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }//startRecordingToolStripMenuItem_Click

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            C3DData.ControlKeyDown = e.Control;
            if (C3DData.ControlKeyDown &&  e.KeyCode == Keys.S ) //Save Project
            {
                SaveProjectToolStripMenuItem_Click(null,null);
            }
            else  if (e.KeyCode == Keys.R )
            {
                startEndToolStripMenuItem_Click(sender,e);
            }            
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            C3DData.ControlKeyDown = e.Control;
        }

        private void toolsToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            
        }

        private void objectsToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            cutingToolStripMenuItem.Enabled = false;
            //Register Verify
            RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
            if (!reg.ReadFromRegister()) return;
            Random rand = new Random();
            RegisterAndEncrypt.HardWareInfo.InfoType type = (RegisterAndEncrypt.HardWareInfo.InfoType)rand.Next(3);
            if (!reg.Verify(type)) return;
            cutingToolStripMenuItem.Enabled = true;
            //Register Verify
        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            double xx = CDataModel.m_Model.GetWidth(0);
            Vector64 p0 = CDataModel.GetCenterPoint();
            TexturedText obj = new TexturedText("Text",p0,(float)xx/2);

            //obj.Create();
            //obj.UpdateRange();
            bool range_updated = C3DData.AddObject(obj, false);
            C3DData.objSelected = C3DData.lastLoaded;
            m_ObjectForm.AddToTree(C3DData.lastLoaded);
            if (range_updated) UpdateDraw();
            else UpdateDraw(C3DData.lastLoaded);
        }
               
        //坐标匹配
        private void CoordinatesMatch_Click(object sender, EventArgs e)
        {
            CoordinatesMatchForm ma = new CoordinatesMatchForm();
            if( ma.ShowDialog() == DialogResult.OK )
            {

            }
        }

        private void axisOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AxisOptionForm ax = new AxisOptionForm();
            if( ax.ShowDialog() == DialogResult.OK )
            {
                //m_DDDForm.InitCamera();
                UpdateDraw(false,2,null);
            }
        }

        //Start or end recording
        private void startEndToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.Recording) //end recording
            {
                this.Cursor = Cursors.WaitCursor;

                if ( m_DDDForm.EndScreenRecord() )
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filter = "avi file (*.avi)|*.avi|all files(*.*)|*.*";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        File.Copy(C3DData.aviRecordFile, dlg.FileName, true);
                        AddtoInfo("Recording saved to'" + dlg.FileName + "'");
                    }
                    else AddtoInfo("Recording canceled!");
                }

                //clear all temporary .jpg file
                C3DData.recordingIndex = 0;
                C3DData.recordList.Clear();
                DeleteFolderFiles(C3DData.tempRecordPath);

                this.Cursor = DefaultCursor;
            }
            else //开始录制
            {
                string avifile = Path.GetTempFileName() + ".avi";
                C3DData.tempRecordPath = Path.GetTempPath() + @"3DSurfer\Recording\";
                DeleteFolderFiles(C3DData.tempRecordPath);
                Directory.CreateDirectory(C3DData.tempRecordPath);
                AddtoInfo("start recording...");
                m_DDDForm.StartScreenRecord(avifile, C3DData.framesPersecond);

            }// else
        }

        private void recordingToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            if (C3DData.Recording)
            {
                startEndToolStripMenuItem.Text = "End recording";
            }
            else 
            {
                startEndToolStripMenuItem.Text = "Start recording";
            }
        }        

        private void lBHtoXYZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LBtoXYProjectionForm lb = new LBtoXYProjectionForm();
            if( lb.ShowDialog() == DialogResult.OK )
            {

            }
        }

        private void griddedProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadProfileFromGRDForm grd = new LoadProfileFromGRDForm();
            if( grd.ShowDialog() == DialogResult.OK )
            {
                CSlicer slicer = null;
                bool range_updated = false;
                if (grd.bmp == null)
                { 
                    GeoProfile profile = new GeoProfile(grd.grid2D, grd.Baseline, grd.baselineDirection);
                    slicer = profile;
                    range_updated = C3DData.AddObject(slicer);
                }
                else //Create from image
                {
                    GeoProfile profile = new GeoProfile(grd.bmp, grd.Baseline, grd.baselineDirection);
                    slicer = profile;
                    slicer.CreateTextureCoords();
                    slicer.enbaleTexture = true;
                    slicer.textureStruct.TextureFile = grd.imagefile;
                    range_updated = C3DData.AddObject(slicer);
                }                
                
                m_ObjectForm.AddToTree(slicer);
                C3DData.objSelected = C3DData.lastLoaded;
                if (range_updated) UpdateDraw();
                else UpdateDraw(C3DData.lastLoaded);
            }
        }

        private void formattedGeoFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                bool error = false;
                string info = "Errors occurred while loading data:\n";
                using ( var dlg = new OpenFileDialog() )
                {
                    dlg.Filter = "Geological Profiles(*.profile)|*.profile|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    int no = 0;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        C3DData.lastLoadeds.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            if (!C3DData.LoadPolygonSlicer(dlg.FileNames[i]))
                            {
                                info += dlg.FileNames[i] + ":" + C3DData.errMessage + "\n";
                                error = true;
                                break;
                            }
                            else no++;                           
                        }
                        if (error)
                        {
                            MessageBoxWarning(C3DData.errMessage, "Errors occurred!");                            
                        }
                        if (C3DData.lastLoadeds.Count > 0)
                        {
                            m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                            C3DData.objSelected = C3DData.lastLoaded;
                            UpdateDraw(C3DData.lastLoadeds);
                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)

                }//using (var dlg = new OpenFileDialog())
            }
            catch (Exception ex)
            {
                MessageBoxErr(ex.Message);                
            }
        }
        /// <summary>
        /// 创建多层层状地层（from .grd）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeshLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateMeshLayersFrom ms = new CreateMeshLayersFrom();
            if (ms.ShowDialog() == DialogResult.OK)
            {
                GeoLayerMeshes geolayer = new GeoLayerMeshes();
                geolayer.AddRange(ms.Layers);
                geolayer.UpdateRange();
                bool range_updated = C3DData.AddObject(geolayer);

                m_ObjectForm.AddToTree(C3DData.lastLoaded);
                C3DData.objSelected = C3DData.lastLoaded;
                if (range_updated) UpdateDraw();
                else UpdateDraw(C3DData.lastLoaded);
            }
            /*
            // YTopLeft
            // |
            // |
            // O---------->X
            CreateMeshLayersFrom ms = new CreateMeshLayersFrom();
            if( ms.ShowDialog() == DialogResult.OK )
            {
                CSurferGrid sf;
                float val = 0;
                Vector64 TopLeft, TopRight, BottomLeft, BottomRight;
                double minz = 0, maxz = 0, z1 = 0, z2 = 0, z01 = 0, z02 = 0,oldz1,oldz2,newz1,newz2;
                double zz1, zz2, zz, z0;
                if ( ms.ZScale != 1.0 )
                {                    
                    for (int i = 0; i < ms.Layers.Count; i++)
                    {
                        sf = ms.Layers[i];
                        if (i == 0) { minz = sf.minv; maxz = sf.maxv; }
                        else 
                        {
                            if ( sf.minv < minz ) minz = sf.minv;
                            if ( sf.maxv > maxz ) maxz = sf.maxv;
                        }
                    }

                    z0 = (minz + maxz) / 2.0;
                    zz1 = maxz - minz;
                    oldz1 = minz;
                    oldz2 = maxz;
                    newz1 = z0 - zz1 * ms.ZScale / 2.0;
                    newz2 = z0 + zz1 * ms.ZScale / 2.0;

                    for (int i = 0; i < ms.Layers.Count; i++)
                    {
                        sf = ms.Layers[i];
                        z1 = newz1 + ( newz2 - newz1 ) * (sf.minv - oldz1) / (oldz2 - oldz1);
                        z2 = newz1 + ( newz2 - newz1 ) * (sf.maxv - oldz1) / (oldz2 - oldz1);
                        for (int j = 0; j < sf.pData.Length; j++)
                        { 
                            val = sf.pData[j];
                            if (sf.IsBlankedValue(val)) continue;
                            val = (float)( z1 + (val - sf.minv) * ( z2 - z1 ) / (sf.maxv - sf.minv)  );
                            sf.pData[j] = val;
                        }
                        sf.minv = z1;
                        sf.maxv = z2;
                        ms.Layers[i] = sf;
                    }
                }

                for ( int i = 0; i < ms.Layers.Count; i++ )
                {
                    sf = ms.Layers[i];
                    TopLeft = new Vector64(sf.minx, sf.maxy, sf.GetZValue(0, sf.yGrid - 1), sf.GetZValue(0, sf.yGrid - 1));
                    TopRight = new Vector64(sf.maxx, sf.maxy, sf.GetZValue(sf.xGrid - 1, sf.yGrid - 1), sf.GetZValue(sf.xGrid - 1, sf.yGrid - 1));
                    BottomLeft = new Vector64(sf.minx, sf.miny, sf.GetZValue(0,0), sf.GetZValue(0,0));
                    BottomRight = new Vector64(sf.maxx, sf.miny, sf.GetZValue(sf.xGrid - 1, 0), sf.GetZValue(sf.xGrid - 1, 0));
                    
                    CMesh mesh = new CMesh();
                    mesh.Name = sf.Name;

                    mesh.fromGrid2D(sf, TopLeft, TopRight, BottomLeft, BottomRight, 1, false);
                    mesh.textureImgFile = "";
                    mesh.enbaleTexture = false;
                    C3DData.AddObject(mesh);
                }
                m_ObjectForm.UpdateTree();
                UpdateDrawList();
            }
            */
        }

        private void shape3DModelerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SymbolModelingForm md = new SymbolModelingForm(null);
            if (md.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void fromWELLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BoreholesWellDataForm wd = new BoreholesWellDataForm();            
            if( wd.ShowDialog() == DialogResult.OK )
            {
                if( wd.boreholes.Count > 0 )
                {
                    bool range_updated = C3DData.AddObject(wd.boreholes);
                    m_ObjectForm.AddToTree(C3DData.lastLoaded);
                    C3DData.objSelected = C3DData.lastLoaded;
                    if (range_updated) UpdateDraw();
                    else UpdateDraw(C3DData.lastLoaded);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(C3DData.objectsDiction.Count > 0 && C3DData.IsDataModified )
            {
                string info = "Changes have not been saved, save it?";
                DialogResult ret = MessageBoxQestionYesNoCancel(info, "Save Changes？");
                if (ret == DialogResult.Cancel) e.Cancel = true;
                else if (ret == DialogResult.Yes) 
                {
                    SaveProjectToolStripMenuItem_Click(sender,e);
                    e.Cancel = true;
                }
            }
        }
        private void geophysicProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProfilesGriddingForm gp = new ProfilesGriddingForm();
            if (gp.ShowDialog() == DialogResult.OK)
            {

            }
        }
        private void BoreholesCurvesInterpolation_Click(object sender, EventArgs e)
        {
            BoreholesCurvesInterpolationForm bf = new BoreholesCurvesInterpolationForm();
            if(bf.ShowDialog() == DialogResult.OK)
            {

            }
        }       
    }

    public class DrawUpdateEventArg : EventArgs
    {
        //传递主窗体的数据信息
        public bool refreshAll = false;  //是否绘制全部
        public C3DObjectBase Obj = null; //指定重绘的对象
        public int ObjType = 0;         //对象类型，0 -C3DObjectBase对象，1虚线框，1坐标轴箭头，2Lights位置
        public DrawUpdateEventArg(bool _refresh,C3DObjectBase _obj,int _type)
        {
            refreshAll = _refresh;
            Obj = _obj;
            ObjType = _type;
        }
    }

}
