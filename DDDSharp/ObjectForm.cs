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
using GlmNet;
using DDDSharp.Gridding;
using DDDSharp.Meshes;
using DDDSharp.Boreholes;
using DataCollection.DelaunayVoronoi;
namespace DDDSharp
{
    public partial class ObjectForm : DockingPaneExt
    {
        double moveStepPercent = 5;        
        ImageList ImageList1 = new ImageList();
        TreeNode rootNode = new TreeNode("Objects");
        TreeNode lastTreeNode = null;
        List<C3DObjectBase> objectsSelected = new List<C3DObjectBase>();

        public ObjectForm()
        {
            InitializeComponent();
            StepTextBox1.Text = moveStepPercent.ToString();
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
        
        
        /*
        public void UpdateTree1()
        {
            /////this validation section//////////////
            ///Validation
            if (C3DData.DemoVersion)
            {
                if (!CheckVialidation()) return;
            }
            /////this is end validation section//////////////

            this.Cursor = Cursors.WaitCursor;

            treeView1.Nodes.Clear();

            foreach (C3DObjectBase obj in C3DData.pObjects)
            {
                TreeNode node = CreateTreeNode(obj);
                node.Checked = obj.Visible;
                lastTreeNode = node;
                treeView1.Nodes.Add(node);
            }

            C3DData.curSel = -1;
            C3DData.curSubSel = -1;
            C3DData.curSubSubSel = -1;
            C3DData.objSelected = null;
            propertyGrid1.SelectedObject = null;

            this.Cursor = Cursors.Default;
        }
        */
        public void CreateDataTrees()
        {
            if (C3DData.dataTrees.Count > 0) return;
            C3DData.dataTrees.Clear();
            C3DData.dataTrees = new TreeStructData("Objects",TreeNodeType.Folder);
            foreach (var item in C3DData.objectsDiction)
            {
                TreeStructData data = new TreeStructData(item.Value.Name,TreeNodeType.Data,item.Value);
                C3DData.dataTrees.AddItem(data);
            }
        }

        /// <summary>
        /// 从TreeStructData对象中生成树形节点
        /// 递归调用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(TreeStructData data)
        {
            if(data.Type == TreeNodeType.Folder)//目录，含子节点
            {                
                TreeNode node1 = new TreeNode(data.Name);
                node1.Checked = true;
                node1.Tag = null;
                
                foreach(TreeStructData t in data.Items)
                    node1.Nodes.Add( CreateTreeNode(t) );
                
                return node1;
            }
            else //叶子节点
            {
                TreeNode node1 = new TreeNode(data.Name);
                node1.Tag = data.Item;
                node1.Checked = data.Item.Visible;
                node1 = CreateTreeNode(data.Item);

                return node1;
            } 
        }
        /// <summary>
        /// 生成树形节点--fromC3DObjectBase对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //C3DObjectBase对象创建为TreeNode
        TreeNode CreateTreeNode(C3DObjectBase obj)
        {
            string type = obj.type.ToString();
            string name = type + ":" + obj.Name;

            TreeNode node = new TreeNode(name);
            node.Tag = obj;
            node.Checked = obj.Visible;

            TreeNode node1, node2, node3;
            switch (obj.type)//对象还具有子对象
            {
                case ShapeEnum.Boreholes:
                    CBoreholes boreholes = (CBoreholes)obj;
                    node.Checked = obj.Visible;
                    foreach (CBorehole bh in boreholes.pData)
                    {
                        bh.Parent = boreholes;
                        node1 = new TreeNode(bh.Name);
                        node1.Tag = bh;
                        node1.Checked = bh.Visible;

                        //测斜数据--生成井轨迹线
                        node2 = new TreeNode(bh.boreholeAngles.Name);
                        node2.Tag = bh.boreholeAngles;
                        bh.boreholeAngles.Parent = bh;
                        node2.Checked = bh.boreholeAngles.Visible;
                        if (bh.boreholeAngles.Count > 0)
                        {
                            foreach (BoreholeAnglesStruct angle in bh.boreholeAngles.Angles)
                            {
                                angle.Parent = bh.boreholeAngles;
                                node3 = new TreeNode(angle.ToString());
                                node3.Tag = angle;
                                node3.Checked = angle.Visible;
                                node2.Nodes.Add(node3);
                            }
                        }
                        node1.Nodes.Add(node2);
                        //地层数据
                        node2 = new TreeNode(bh.Stratums.Name);
                        node2.Tag = bh.Stratums;
                        bh.Stratums.Parent = bh;
                        node2.Checked = bh.Stratums.Visible;
                        if ( bh.Stratums.Count > 0)
                        {
                            foreach(StratumData layer in bh.Stratums.Stratums)
                            {
                                node3 = new TreeNode(layer.Name);
                                node3.Tag = layer;
                                node3.Checked = layer.Visible;
                                node2.Nodes.Add(node3);
                            }
                        }
                        node1.Nodes.Add(node2);
                        //----测井曲线数据-----------------------------
                        node2 = new TreeNode(bh.Curves.Name);
                        node2.Tag = bh.Curves;
                        bh.Curves.Parent = bh;
                        node2.Checked = bh.Curves.Visible;
                        if (bh.Curves.Count > 0)
                        {
                            foreach (BoreholeCurve cv in bh.Curves.Curves)
                            {
                                cv.Parent = bh.Curves;
                                node3 = new TreeNode(cv.Name);
                                node3.Tag = cv;
                                node3.Checked = cv.Visible;
                                node2.Nodes.Add(node3);
                            }
                        }
                        node1.Nodes.Add(node2);

                        node.Nodes.Add(node1);
                    }
                    break;
                case ShapeEnum.Borehole:
                    node.Checked = obj.Visible;                    
                    CBorehole bh1 = (CBorehole)obj;
                    node.Tag = bh1;
                    //-----Incline angles---------------------------------
                    node1 = new TreeNode(bh1.boreholeAngles.Name);
                    node1.Tag = bh1.boreholeAngles;
                    bh1.boreholeAngles.Parent = bh1;
                    node1.Checked = bh1.boreholeAngles.Visible;
                    if (bh1.boreholeAngles.Count > 0)
                    {
                        foreach (BoreholeAnglesStruct angle in bh1.boreholeAngles.Angles)
                        {
                            angle.Parent = bh1.boreholeAngles;
                            node2 = new TreeNode(angle.ToString());
                            node2.Tag = angle;
                            node2.Checked = angle.Visible;
                            node1.Nodes.Add(node2);
                        }
                    }
                    node.Nodes.Add(node1);

                    //--------Strata------------------------------
                    node1 = new TreeNode(bh1.Stratums.Name);
                    node1.Tag = bh1.Stratums;
                    bh1.Stratums.Parent = bh1;
                    node1.Checked = bh1.Stratums.Visible;
                    if (bh1.Stratums.Count > 0)
                    {
                        foreach (StratumData layer in bh1.Stratums.Stratums)
                        {
                            layer.Parent = bh1.Stratums;
                            node2 = new TreeNode(layer.Name);
                            node2.Tag = layer;
                            node2.Checked = layer.Visible;
                            node1.Nodes.Add(node2);
                        }
                    }
                    node.Nodes.Add(node1);

                    //-----Well curves---------------------------------
                    node1 = new TreeNode(bh1.Curves.Name);
                    node1.Tag = bh1.Curves;
                    bh1.Curves.Parent = bh1;
                    node1.Checked = bh1.Curves.Visible;
                    if (bh1.Curves.Count > 0)
                    {
                        foreach (BoreholeCurve cv in bh1.Curves.Curves)
                        {
                            cv.Parent = bh1.Curves;
                            node2 = new TreeNode(cv.Name);
                            node2.Tag = cv;
                            node2.Checked = cv.Visible;
                            node1.Nodes.Add(node2);
                        }
                    }
                    node.Nodes.Add(node1);

                    break;
                case ShapeEnum.LineMesh:
                    LineMesh mesh = (LineMesh)obj;
                    node.Checked = mesh.Visible;
                    foreach (C3DLine line in mesh.lines)
                    {
                        line.Parent = mesh;
                        node1 = new TreeNode(line.Name);
                        node1.Tag = line;
                        node1.Checked = line.Visible;
                        node.Nodes.Add(node1);
                    }
                    break;
                case ShapeEnum.Line:
                    node.Checked = obj.Visible;
                    node.Tag = (C3DLine)obj;
                    break;
                case ShapeEnum.GeoLayerMeshes:
                    GeoLayerMeshes meshes = (GeoLayerMeshes)obj;
                    node.Checked = obj.Visible;
                    for (int i = 0; i < meshes.Count; i++)
                    {
                        GeoMesh m = meshes[i];
                        m.Parent = meshes;
                        node1 = new TreeNode(m.Name);
                        node1.Tag = m;
                        node1.Checked = m.Visible;
                        node.Nodes.Add(node1);
                    }
                    break;
                case ShapeEnum.GeoMesh:
                    node.Checked = obj.Visible;
                    node.Tag = (GeoMesh)obj;
                    break;
                case ShapeEnum.PolygonSlicer:
                    PolygonSlicer slicer = (PolygonSlicer)obj;
                    //background
                    node1 = new TreeNode( slicer.polygons.Name );
                    node1.Tag = slicer.polygons;
                    slicer.polygons.Parent = slicer;
                    node1.Checked = slicer.polygons.Visible;
                    node1.ImageIndex = 0;
                    foreach (Polygon2D obj1 in slicer.polygons.Polygons)
                    {
                        obj1.Parent = slicer.polygons;
                        node3 = new TreeNode(obj1.Name);
                        node3.Tag = obj1;
                        node3.Checked = obj1.Visible;
                        node3.ImageIndex = 1;
                        node1.Nodes.Add(node3);
                    }
                    //geological
                    node2 = new TreeNode( slicer.tracedGeoObjects.Name );
                    node2.Tag = slicer.tracedGeoObjects;
                    slicer.tracedGeoObjects.Parent = slicer;
                    node2.ImageIndex = 0;
                    node2.Checked = slicer.tracedGeoObjects.Visible;
                    foreach (Polygon2D obj2 in slicer.tracedGeoObjects.Polygons)
                    {
                        obj2.Parent = slicer.tracedGeoObjects;
                        node3 = new TreeNode(obj2.Name);
                        node3.Tag = obj2;
                        node3.Checked = obj2.Visible;
                        node3.ImageIndex = 1;
                        node2.Nodes.Add(node3);
                    }
                    node.Nodes.Add(node1);
                    node.Nodes.Add(node2);
                    break;
                case ShapeEnum.Polygon2D:
                    node.Checked = obj.Visible;
                    node.Tag = (Polygon2D)obj;
                    break;
                case ShapeEnum.Polygon2Ds:
                    node.Checked = obj.Visible;
                    node.Tag = (C2DPolygons)obj;
                    C2DPolygons polys = obj as C2DPolygons;
                    foreach (Polygon2D poly in polys.Polygons)
                    {
                        poly.Parent = obj;
                        node3 = new TreeNode(poly.Name);
                        node3.Tag = poly;
                        node3.Checked = poly.Visible;
                        node3.ImageIndex = 1;
                        node.Nodes.Add(node3);
                    }
                    break;
                case ShapeEnum.Triangles:
                    node.Checked = obj.Visible;
                    node.Tag = (TriangleObj)obj;
                    break;
                case ShapeEnum.Grid3D:
                    node.Checked = obj.Visible;
                    node.Tag = (C3DGridData)obj;
                    break;
                case ShapeEnum.Mesh:
                    node.Checked = obj.Visible;
                    node.Tag = (CMesh)obj;
                    break;
                case ShapeEnum.Shape:
                    node.Checked = obj.Visible;
                    node.Tag = (Symbol3D)obj;
                    break;                
                case ShapeEnum.Points:
                    node.Checked = obj.Visible;
                    node.Tag = (ScatteredPoints)obj;
                    break;
                case ShapeEnum.Slicer:
                    node.Checked = obj.Visible;
                    node.Tag = (CSlicer)obj;
                    break;
                case ShapeEnum.GeoProfile:
                    node.Checked = obj.Visible;
                    node.Tag = (GeoProfile)obj;
                    break;
                case ShapeEnum.Text:
                    node.Checked = obj.Visible;
                    node.Tag = (TexturedText)obj;
                    break;
            }
            return node;
        }
        /// <summary>
        /// Add objects to tree
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nodeType">1 add to selected node, 2 add to root node</param>
        public void AddToTree(C3DObjectBase obj,int nodeType = 1)
        {
            this.Cursor = Cursors.WaitCursor;
                        
            C3DData.objSelected = obj;

            TreeNode node = CreateTreeNode(obj);
            node.Checked = obj.Visible;
            node.Tag = obj;

            if (nodeType == 1)
            {
                if ( treeView1.SelectedNode != null && 
                     treeView1.SelectedNode.Tag == null )
                    treeView1.SelectedNode.Nodes.Add(node);
                else rootNode.Nodes.Add(node);
            }
            else if (nodeType == 2)
            {
                rootNode.Nodes.Add(node);
            }
            treeView1.SelectedNode = node;

            propertyGrid1.SelectedObject = C3DData.objSelected;

            this.Cursor = Cursors.Default;
        }
        public void AddToTree(List<C3DObjectBase> objects)
        {
            this.Cursor = Cursors.WaitCursor;
            
            //C3DData.curSel = -1;
            //C3DData.curSubSel = -1;
            //C3DData.curSubSubSel = -1;
            
            TreeNode curnode = treeView1.SelectedNode;
            if (curnode == null || curnode.Tag != null) curnode = rootNode;          

            foreach (C3DObjectBase obj in objects)
            {
                TreeNode node = CreateTreeNode(obj);
                node.Checked = obj.Visible;
                node.Tag = obj;
                curnode.Nodes.Add(node);
                lastTreeNode = node;
            }
            curnode.Expand();

            treeView1.SelectedNode = lastTreeNode;

            //C3DData.objSelected = C3DData.lastLoaded;
            //treeView1.SelectedNode = lastTreeNode;
            //propertyGrid1.SelectedObject = C3DData.objSelected;

            this.Cursor = Cursors.Default;
        }
        void RemoveFromTree(TreeNode node)
        {
            treeView1.Nodes.Remove(node);
            node.Nodes.Clear();
            treeView1.SelectedNode = null;
        }
        
        public TreeNode structToTree()
        {
            return structToTree(C3DData.dataTrees);
        }
        public TreeNode structToTree(TreeStructData data)
        {
            return CreateTreeNode(data);
        }
        public TreeStructData treeToStruct()
        {
            return treeToStruct(rootNode);
        }
        public TreeStructData treeToStruct(TreeNode node)
        {
            TreeStructData data = new TreeStructData(node.Text);
            if (node.Tag == null) //目录树
            { 
                data.Type = TreeNodeType.Folder;
                data.Item = null;
                data.Checked = node.Checked;
                foreach(TreeNode nd in node.Nodes )
                    data.AddItem(treeToStruct(nd));
                
                return data;
            }
            else //data节点
            {
                data.Type = TreeNodeType.Data;
                data.Item = (C3DObjectBase) node.Tag;
                data.Checked = data.Item.Visible;
                data.ItemKey = C3DData.GetObjectKeyFromDiction(data.Item);
                return data;
            }           
        }

        List<TreeNodeStruct> nodesList = new List<TreeNodeStruct>();
        void toNodesList(TreeNode node,TreeNode parent,int index)
        {
            nodesList.Add(new TreeNodeStruct( node,parent,index) );
            for(int i = 0; i < node.Nodes.Count; i++ )
            {
                TreeNode p = node.Nodes[i];
                toNodesList(p,node,i);
            }
        }
        
        public void UpdateTree(C3DObjectBase obj, bool changeName = false)
        {
            this.Cursor = Cursors.WaitCursor;
            
            nodesList.Clear();
            for( int i = 0; i < rootNode.Nodes.Count; i++ )
            {
                TreeNode p = rootNode.Nodes[i];
                toNodesList(p,rootNode,i); 
            }

            for(int i = 0; i < nodesList.Count; i++)
            {
                TreeNodeStruct p = nodesList[i];
                if (p.Node.Tag != null && p.Node.Tag == obj)
                {
                    TreeNode node = CreateTreeNode(obj);
                    if( !changeName ) node.Text = p.Node.Text;
                    //p.Parent.Nodes[p.Index].Nodes.Clear();                    
                    p.Parent.Nodes.Insert(p.Index, node);
                    p.Parent.Nodes.RemoveAt(p.Index+1);
                    treeView1.SelectedNode = p.Parent.Nodes[p.Index];
                    treeView1.SelectedNode.Expand();
                    //p.Parent.Nodes[p.Index] = node; 
                    break;
                }
            }
            nodesList.Clear();
            this.Cursor = Cursors.Default;
        }
        /// <summary>
        /// Update object tree 
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="nodeType">1 selected node, 2 parent of selected node </param>
        public void UpdateTree( C3DObjectBase obj, int nodeType )
        {
            this.Cursor = Cursors.WaitCursor;
            
            TreeNode thisNode = treeView1.SelectedNode;
            if (nodeType == 2) thisNode = treeView1.SelectedNode.Parent;

            if( thisNode != null )
            {
                TreeNode pNode = thisNode.Parent;
                if( pNode != null )
                {
                    int pos = pNode.Nodes.IndexOf(thisNode);
                    if (pos >= 0)
                    {
                        TreeNode curNode = CreateTreeNode(obj);
                        curNode.Text = thisNode.Text;
                        curNode.Checked = thisNode.Checked;
                        pNode.Nodes[pos] = curNode;
                        thisNode.Nodes.Clear();
                        treeView1.SelectedNode = pNode.Nodes[pos];
                        treeView1.SelectedNode.Expand();
                    }
                }                
            }

            propertyGrid1.SelectedObject = obj;

            this.Cursor = Cursors.Default;
        }
        public void UpdateTree()
        {
            this.Cursor = Cursors.WaitCursor;

            treeView1.Nodes.Clear();

            rootNode = CreateTreeNode(C3DData.dataTrees);

            treeView1.Nodes.Add(rootNode);
            rootNode.Expand();
           
            C3DData.objSelected = null;
            propertyGrid1.SelectedObject = null;

            this.Cursor = Cursors.Default;
        }
        
        public void RemoveObjectFromDictionary(C3DObjectBase obj)
        {
            foreach (var item in C3DData.objectsDiction)
            {
                if (obj == item.Value)
                    C3DData.objectsDiction.Remove(item.Key);
            }
        }
        public void RemoveObjectFromDictionary(TreeStructData data)
        {
            if( data.Type == TreeNodeType.Data )
            {
                RemoveObjectFromDictionary(data.Item);
                return;
            }
            else
            {
                foreach (var item in data.Items) 
                    RemoveObjectFromDictionary(item);
            }            
        }
        private string GetNodeName(C3DObjectBase obj,int level, int sub0 = -1, int sub1 = -1, int sub2 = -1)
        {
            string type = obj.type.ToString();
            string name = type + ":" + obj.Name;
            
            switch (obj.type)//对象还具有子对象
            {
                case ShapeEnum.Borehole:
                    if( sub1 >= 0 )
                    {
                        CBoreholes boreholes = (CBoreholes)obj;
                        CBorehole bh = boreholes[sub1];
                        name = bh.Name;
                    }                    
                    break;
                case ShapeEnum.LineMesh:
                    if (sub1 >= 0)
                    { 
                        LineMesh mesh = (LineMesh)obj;
                        C3DLine line = mesh.lines[sub1];
                        name = line.Name;
                    }
                    break;
                case ShapeEnum.GeoLayerMeshes:
                    if (sub1 >= 0)
                    {
                        GeoLayerMeshes meshes = (GeoLayerMeshes)obj;
                        GeoMesh m = meshes[sub1];
                        name = m.Name;
                    }
                    break;
                case ShapeEnum.PolygonSlicer:
                    if( level == 1 )
                    {
                        if (sub0 == 0) name = "background objects";
                        if (sub0 == 1) name = "geological objects";
                    }
                    if (level == 2)
                    {
                        PolygonSlicer slicer = (PolygonSlicer)obj;
                        if (sub0 == 0)name = slicer.polygons[sub1].Name;                        
                        if (sub0 == 1) name = slicer.tracedGeoObjects[sub1].Name;
                    }                    
                    break;
            }
            
            return name;
        }
        //refreshAll - 重绘全部
        //对象类型，0 -C3DObjectBase对象，1虚线框，2坐标轴箭头，3Lights位置
        void UpdateDraw(bool refreshAll, int type, C3DObjectBase obj = null) //update all object
        {
            Program.m_MainForm.UpdateDraw(refreshAll,type,obj);
        }
        void UpdateDraw()//update all
        {
            Program.m_MainForm.UpdateDraw();
        }
        void UpdateDraw(C3DObjectBase obj)
        {
            Program.m_MainForm.UpdateDraw(obj);            
        }
        void UpdateDraw(List<C3DObjectBase> objects)
        {
            Program.m_MainForm.UpdateDraw(objects);
        }
        private void ObjectForm_Load(object sender, EventArgs e)
        {
            treeView1.HideSelection = false;
            treeView1.AllowDrop = true;

            rootNode.Tag = null;
            rootNode.ImageIndex = 0;

            ImageList1.Images.Add(Resource1.Folder01);
            ImageList1.Images.Add(Resource1.Grid3D);
            ImageList1.Images.Add(Resource1.PolygonSlicer);
            
            treeView1.ImageList = ImageList1;
            UpdateTree();
        }        

        private void DoTreeSelected()
        {
            C3DData.objSelected = null;
            C3DData.objSubSelected = null;
            C3DData.objSubSubSelected = null;
            if (treeView1.SelectedNode == null) return;
            TreeNode node = treeView1.SelectedNode;

            if (treeView1.SelectedNode.Tag == null) C3DData.objSelected = null;
            else C3DData.objSelected = (C3DObjectBase)treeView1.SelectedNode.Tag;
           
            propertyGrid1.SelectedObject = C3DData.objSelected;
            //propertyGrid1.PropertySort = PropertySort.CategorizedAlphabetical;
            //设置属性不按A-Z排序
            propertyGrid1.PropertySort = PropertySort.Categorized;
            //设置属性按A-Z排序
            //propertyGrid1.PropertySort = PropertySort.Alphabetical;
            //propertyGrid1.ExpandAllGridItems();
            UpdateSelectedPropertyForm();
        }        
       
        void UpdateSelectedPropertyForm()
        {
            if (C3DData.objSelected == null) return;
            ShapeEnum type = C3DData.objSelected.type;
            if (type == ShapeEnum.Grid3D)
            {
                Program.m_MainForm.Dock3DGridProperty();
                Program.m_MainForm.m_3DGridForm.UpdateSelect();
            }
            else if (type == ShapeEnum.Slicer || type == ShapeEnum.Mesh || type == ShapeEnum.GeoProfile)
            {
                Program.m_MainForm.DockSlicerProperty();
                Program.m_MainForm.m_SlicerPropertyForm.UpdateSelect();
            }
            else if (type == ShapeEnum.Line)
            {
                Program.m_MainForm.Dock3DLineProperty();
            }            
            else if (type == ShapeEnum.Borehole)
            {                
                Program.m_MainForm.DockBoreholesProperty();
            }
            else if (type == ShapeEnum.Points)
            {
                Program.m_MainForm.DockScattedPointsProperty();               
            }
            else if (type == ShapeEnum.Triangles)
            {
               // C3DData.currentColorScale = ((TriangleObj)C3DData.objSelected).co;
            }
            else if (type == ShapeEnum.Mesh)
            {
                
            }
            else if (type == ShapeEnum.PolygonSlicer)
            {
                
            }
        }
       
        private void treeView1_Click(object sender, EventArgs e)
        {            
            
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DoTreeSelected();
            UpdateDraw(false,4,null);//绘制选择物体线框            
        }
       
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {   
            C3DObjectBase obj = CheckPropertyUpdated(C3DData.objSelected);
            if(obj != null )UpdateDraw(obj);
        }
        C3DObjectBase CheckPropertyUpdated(C3DObjectBase obj)
        {
            if (obj.type == ShapeEnum.Points)
            {
                ScatteredPoints sc = obj as ScatteredPoints;
                if (sc.IsTextStyleChanged())
                {
                    sc.RenderMode = RenderingUpdateMode.Redraw;
                    return sc;
                }
            }
            else if (obj.type == ShapeEnum.Text)
            {
                TexturedText sc = obj as TexturedText;
                if (sc.IsTextStyleChanged())
                {
                    sc.RenderMode = RenderingUpdateMode.Redraw;
                    return sc;
                }
            }
            else if (obj.type == ShapeEnum.Grid3D)
            {
                C3DGridData grid = obj as C3DGridData;
                if (grid.IsColorScaleUpdated())
                {
                    grid.ColorScale.SetVisibleFrom(grid._OlderColorScale);
                    grid.UpdateColorIndexTable();
                    Program.m_MainForm.m_3DGridForm.UpdateDataGridview();
                    Program.m_MainForm.m_3DGridForm.UpdateColorScale();
                    grid.RenderMode = RenderingUpdateMode.Redraw;
                    return grid;
                }
            }
            else if (obj.type == ShapeEnum.Slicer)
            {
                CSlicer slicer = obj as CSlicer;
                if (slicer.IsColorScaleUpdated())
                {
                    slicer.ColorScale.SetVisibleFrom(slicer._OlderColorScale);
                    Program.m_MainForm.m_SlicerPropertyForm.UpdateDataGridview();
                    slicer.RenderMode = RenderingUpdateMode.Redraw;
                    return slicer;
                }
            }

            else if ( obj.type == ShapeEnum.Boreholes )
            {
                CBoreholes boreholes = obj as CBoreholes;
                if (boreholes.IsUniformStyle && boreholes.UpdateNeeded)
                {
                    //boreholes.ApplyToAll();
                    boreholes.RenderMode = RenderingUpdateMode.Redraw;
                    return boreholes;
                }
            }
            else if (obj.type == ShapeEnum.Borehole)
            {
                CBorehole borehole = obj as CBorehole;
                if (borehole.Curves.UniformCurveStyle && borehole.UpdateNeeded)
                {
                    //borehole.ApplyToAll();
                    Program.m_MainForm.m_SlicerPropertyForm.UpdateDataGridview();
                    borehole.RenderMode = RenderingUpdateMode.Redraw;
                    return borehole;
                }
            }
            else if (obj.type == ShapeEnum.BoreholeAngle)
            {
                BoreholeAnglesStruct an = obj as BoreholeAnglesStruct;
                treeView1.SelectedNode.Text = an.ToString();
            }
            else if (obj.type == ShapeEnum.BoreholeStratum)
            {
                StratumData layer = obj as StratumData;
                treeView1.SelectedNode.Text = layer.Name;
            }
            else if (obj.type == ShapeEnum.GeoLayerMeshes)
            {
                GeoLayerMeshes layermeshes = (GeoLayerMeshes)obj;
                layermeshes.RenderMode = RenderingUpdateMode.Redraw;
                return layermeshes;
            }
            else if (obj.type == ShapeEnum.GeoMesh)
            {
                GeoMesh mesh = obj as GeoMesh;
                if (mesh.Parent != null)
                {
                    GeoLayerMeshes layermeshes = (GeoLayerMeshes)mesh.Parent;
                    layermeshes.RenderMode = RenderingUpdateMode.Redraw;
                    return layermeshes;
                }
            }
            else if (obj.type == ShapeEnum.GeoProfile)
            {
                GeoProfile slicer = obj as GeoProfile;
                if (slicer.IsColorScaleUpdated())
                {
                    slicer.ColorScale.SetVisibleFrom(slicer._OlderColorScale);
                    Program.m_MainForm.m_SlicerPropertyForm.UpdateDataGridview();
                    slicer.RenderMode = RenderingUpdateMode.Redraw;
                    return slicer;
                }
            }
            else
            {
                obj.RenderMode = RenderingUpdateMode.Redraw;
                return obj;
            }
            return null;
        }
        //删除对象列表
        List<C3DObjectBase> removedObjects = new List<C3DObjectBase>();
        void DeleteObjectByNode(TreeNode node)
        {
            if (node == null) return;
            if( node.Tag == null )//目录
            {
                foreach(TreeNode nd in node.Nodes)
                {
                    DeleteObjectByNode(nd);
                }                
            }
            else //对象
            {
                C3DObjectBase obj = node.Tag as C3DObjectBase;

                // 父对象是对象节点
                if ( node.Parent != null && node.Parent.Tag != null)
                { 
                    C3DObjectBase parent = treeView1.SelectedNode.Parent.Tag as C3DObjectBase;
                    parent.Remove(obj);//主对象中删除其中的子对象
                    removedObjects.Add(obj);
                }
                else
                {
                    C3DData.RemoveObjectFromDictionary(obj);//从字典中清除
                    removedObjects.Add(obj);
                }
                return;
            }
        }


        //remove object from list
        private void removeObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( treeView1.SelectedNode == null ) return;

            string info = "this node and all the objects included will be removed?";
            if (MessageBox.Show(info, "Remove items？", MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            removedObjects.Clear();
            DeleteObjectByNode(treeView1.SelectedNode);

            Program.m_MainForm.m_DDDForm.ClearObjectDrawBuffers(removedObjects);//清除显示
            Program.m_MainForm.m_DDDForm.UpdateView();

            C3DData.objSelected = null;
            RemoveFromTree(treeView1.SelectedNode);
            treeView1.SelectedNode = null;
            propertyGrid1.SelectedObject = C3DData.objSelected;

            C3DData.IsDataModified = true;
        }

        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;

            string info = "All objects will be removed?";
            if (MessageBox.Show(info, "Remove All？", MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            C3DData.ClearObjects();
            rootNode.Nodes.Clear();
            treeView1.SelectedNode = rootNode;
            UpdateDraw();
        }

        private void DebugTest()
        {
            /*
           C3DGridData d1 = new C3DGridData();
           d1.Load3DGridData(@"C: \Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\All\GSR.3DGrid");
           C3DGridData d2 = new C3DGridData();
           d2.Load3DGridData(@"C: \Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\42369\3D-1.3DGrid");

           FileStream fs = new FileStream(@"C: \Users\XIN011\Documents\Source\Las data\Las 2017\Cross_Validation\42369Diff.dat", FileMode.Create);
           StreamWriter wr = new StreamWriter(fs);
           string str = "x,y,z,v1,v2";
           double x, y, z,v1,v2;
           long id;
           wr.WriteLine(str);
           for(int iz=0;iz<d1.zNum;iz++)
               for (int iy = 0; iy < d1.yNum; iy++)
                   for (int ix = 0; ix < d1.xNum; ix++)
               {
                       id = iz * d1.xyNum + iy * d1.xNum + ix;
                       v1 = d1.pGridData[id];
                       v2 = d2.pGridData[id];
                       if (v1 != v2)
                       {
                           x = d1.minx + ix * d1.xStep;
                           y = d1.miny + iy * d1.yStep;
                           z = d1.minz + iz * d1.zStep;
                           str = x + "," + y + "," + z + "," + v1 + "," + v2;
                           wr.WriteLine(str);
                       }
               }
           wr.Close();
           fs.Close();
           */
            /* data test 
            CSlicer cs1 = new CSlicer(0, 0);
            cs1.LoadSlicer(@"C: \Users\XIN011\Documents\Source\Las data\Las 2017\Slicers\Grid-42372-42376all.dat");
            CSlicer cs2 = new CSlicer(0, 0);
            cs2.LoadSlicer(@"C: \Users\XIN011\Documents\Source\Las data\Las 2017\Slicers\Grid-42372-42376-takeout42369.dat");
            Vector32 p1, p2;
            no = 0;
            for (int i = 0; i < cs1.nRow; i++)
                for (int j = 0; j < cs1.nCol; j++)
                {
                    p1 = cs1.pData[i * cs1.nCol + j];
                    p2 = cs2.pData[i * cs1.nCol + j];
                    if(p1.v != p2.v )
                    {
                        no++;
                    }
                }
                
            return;
            */
        }
        private void centerButton_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;

            double x1 = CDataModel.m_ModelOrg.X1;
            double y1 = CDataModel.m_ModelOrg.Y1;
            double z1 = CDataModel.m_ModelOrg.Z1;
            double x2 = CDataModel.m_ModelOrg.X2;
            double y2 = CDataModel.m_ModelOrg.Y2;
            double z2 = CDataModel.m_ModelOrg.Z2;
            C3DData.objSelected.offset = new vec3(0,0,0);
            C3DData.objSelected.scale = new vec3(1, 1, 1);
            C3DData.objSelected.rotate = new vec3(0, 0, 0);
            double mx1 = C3DData.objSelected.Minx;
            double mx2 = C3DData.objSelected.Maxx;
            double my1 = C3DData.objSelected.Miny;
            double my2 = C3DData.objSelected.Maxy;
            double mz1 = C3DData.objSelected.Minz;
            double mz2 = C3DData.objSelected.Maxz;
            
            double maxlen = mx2 - mx1;
            double x0 = (x1 + x2) / 2;
            double y0 = (y1 + y2) / 2;
            double z0 = (z1 + z2) / 2;
            double dx = x2-x1;
            double dy = (y2 - y1) * (my2 - my1) / maxlen;
            double dz = (z2 - z1) * (mz2 - mz1) / maxlen;
            if ( maxlen < my2 - my1 )
            {
                maxlen = my2 - my1;
                dx = (x2 - x1) * (mx2 - mx1) / maxlen;
                dz = (z2 - z1) * (mz2 - mz1) / maxlen;
                dy = y2 - y1;
            }
            if (maxlen < mz2 - mz1)
            {
                maxlen = mz2 - mz1;
                dx = (x2 - x1) * (mx2 - mx1) / maxlen;
                dy = (y2 - y1) * (my2 - my1) / maxlen;
                dz = z2 - z1;
            }

            C3DData.objSelected.ScaledToRange(x0-dx/2,y0-dy/2,z0-dz/2, x0 + dx / 2, y0 + dy / 2, z0 + dz / 2);
            
            C3DData.objSelected.RenderMode = RenderingUpdateMode.Redraw;
            UpdateDraw(C3DData.objSelected);
        }

        private void NormalizeButton_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;

            C3DData.objSelected.Normalize();
            C3DData.objSelected.RenderMode = RenderingUpdateMode.Redraw;          

            UpdateDraw(C3DData.objSelected);
        }        
       
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            /////this validation section//////////////
            ///Validation
            if (C3DData.DemoVersion)
            {
                if (!CheckVialidation()) return;
            }
            /////this is end validation section//////////////
            ///
            if (e.Button == MouseButtons.Right)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeView1.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    treeView1.SelectedNode = CurrentNode;//选中这个节点

                    if ( treeView1.SelectedNode.Tag == null ) //节点可能是个目录
                    {
                        if(treeView1.SelectedNode.Level == 0)//根节点
                           CurrentNode.ContextMenuStrip = contextMenuStripRoot;
                        else CurrentNode.ContextMenuStrip = contextMenuStripFolder;
                    }
                    else
                    {
                        //CurrentNode.Name
                        //根据不同节点显示不同的右键菜单，当然你可以让它显示一样的菜单
                        switch (C3DData.objSelected.type)
                        {
                            case ShapeEnum.Grid3D:
                                CurrentNode.ContextMenuStrip = contextMenuStrip3DGrid;
                                break;
                            case ShapeEnum.Triangles:
                                CurrentNode.ContextMenuStrip = contextMenuStripTriangleOBJ;
                                break;
                            case ShapeEnum.Slicer:
                                CurrentNode.ContextMenuStrip = contextMenuStripSlicer;
                                break;
                            case ShapeEnum.PolygonSlicer:
                                CurrentNode.ContextMenuStrip = contextMenuPolygonSlicer;
                                break;
                            case ShapeEnum.Polygon2D:
                                CurrentNode.ContextMenuStrip = contextMenuPolygon2D;
                                break;
                            case ShapeEnum.Polygon2Ds:
                                CurrentNode.ContextMenuStrip = contextMenuPolygon2Ds;
                                break;
                            case ShapeEnum.Line:
                                CurrentNode.ContextMenuStrip = contextMenuStripLine;
                                break;
                            case ShapeEnum.Mesh:
                                CurrentNode.ContextMenuStrip = contextMenuStripMesh;
                                break;
                            case ShapeEnum.Points:
                                CurrentNode.ContextMenuStrip = contextMenuStripScatterPoints;
                                break;
                            case ShapeEnum.Borehole:
                            case ShapeEnum.Boreholes:
                                CurrentNode.ContextMenuStrip = contextMenuStripBoreholes;
                                break;
                            case ShapeEnum.BoreholeStratums:
                            case ShapeEnum.BoreholeAngles:
                            case ShapeEnum.BoreholeCurves:
                                CurrentNode.ContextMenuStrip = contextMenuStripBoreholeSub;
                                break;
                            default:
                                CurrentNode.ContextMenuStrip = contextMenuStripOthers;
                                break;
                        }
                    }
                    
                }
            }//if (e.Button == MouseButtons.Right)

        }


        private void DoLineExport(C3DObjectBase obj,string filter)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.FileName = obj.Name;
                dlg.FilterIndex = 1;

                if (dlg.ShowDialog() == DialogResult.OK)
                {                   
                    this.Cursor = Cursors.WaitCursor;

                    if (dlg.FilterIndex == 1 || dlg.FilterIndex == 2)
                    {
                        if (obj.ExportData(dlg.FileName))                    
                            MessageBox.Show("data saved to file:" + dlg.FileName);
                        else
                            MessageBox.Show("failed to save data to file.\n" + obj.errMessage);
                    }                    

                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void DoPointsExport(C3DObjectBase obj, string filter)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.FileName = obj.Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string pathname = dlg.FileName;
                    
                    this.Cursor = Cursors.WaitCursor;

                    if (obj.ExportData(pathname))
                        MessageBox.Show("data exported to file:" + pathname);
                    else
                        MessageBox.Show("failed to export data to file:" + pathname);

                    this.Cursor = DefaultCursor;
                }
            }
        }
        
        private void DoMeshExport(CMesh obj, string filter)
        {            
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.FileName = Path.GetFileNameWithoutExtension(obj.Name);
                dlg.FilterIndex = 1;

                if (dlg.ShowDialog() == DialogResult.OK)
                {                 
                    this.Cursor = Cursors.WaitCursor;

                    if (dlg.FilterIndex == 1 || dlg.FilterIndex == 6 )//formatted mesh
                    {
                        if ( obj.ExportData(dlg.FileName) )
                            MessageBox.Show("Data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("Failed to export data.\n" + obj.errMessage);
                    }
                    else if (dlg.FilterIndex == 2 || dlg.FilterIndex == 3)
                    {
                        //2 --3d triangles object,  3 --vrml                          
                        Program.m_MainForm.DoExportTriangle(dlg.FileName, obj);
                    }
                    else if (dlg.FilterIndex == 4)//.grd
                    {
                        if (obj.ExportToGrid2D(dlg.FileName))
                            MessageBox.Show("data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("failed to export data to file.\n" + obj.errMessage);
                    }
                    else if (dlg.FilterIndex == 5)//AscII XYZV(*.dat)
                    {
                        if ( obj.ExportXYZVData(dlg.FileName) )
                            MessageBox.Show("Data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("Failed to export data.\n" + obj.errMessage);
                    }
                   
                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void DoSlicerExport(CSlicer obj, string filter)
        {            
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.FileName = obj.Name;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                     this.Cursor = Cursors.WaitCursor;

                    if (dlg.FilterIndex == 1 || dlg.FilterIndex == 7) // SlicerFileFormatFilter .slicer
                    {
                        if (obj.ExportData(dlg.FileName))
                            MessageBox.Show("Data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("Failed to export data.\n" + obj.errMessage);
                    }
                    else if (dlg.FilterIndex == 2 || dlg.FilterIndex == 3)//Triangles file or VRML
                    {                        
                        Program.m_MainForm.DoExportTriangle(dlg.FileName, obj);
                    }
                    else if (dlg.FilterIndex == 4)//.grd
                    {
                        if (obj.ExportToGrid2D(dlg.FileName))
                            MessageBox.Show("data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("failed to export data to file.\n" + obj.errMessage);
                    }
                    else if (dlg.FilterIndex == 5)//AscII XYZV(*.dat)
                    {
                        if (obj.ExportXYZVData(dlg.FileName))
                            MessageBox.Show("Data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("Failed to export data.\n" + obj.errMessage);
                    }
                    else if (dlg.FilterIndex == 6)//DXF(*.DXF)
                    {
                        if (obj.ExportContourLinesToDXF2D(dlg.FileName))
                            MessageBox.Show("Data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("Failed to export data.\n" + obj.errMessage);
                    }                    
                    
                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void DoPolygon2DExport(Polygon2D obj, string filter)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.FileName = obj.Name;
                dlg.FilterIndex = 1;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    if (dlg.FilterIndex == 1 || dlg.FilterIndex == 2)//polygon2D file .poly2D
                    {
                        if (obj.ExportData(dlg.FileName))
                            MessageBox.Show("data saved to file:" + dlg.FileName);
                        else
                            MessageBox.Show("failed to save data to file.\n" + obj.errMessage);
                    }                    

                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void DoPolygonSlicerExport(PolygonSlicer obj, string filter)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.FileName = obj.Name;
                dlg.FilterIndex = 1;

                if (dlg.ShowDialog() == DialogResult.OK)
                {                    
                    this.Cursor = Cursors.WaitCursor;

                    if (dlg.FilterIndex == 1 || dlg.FilterIndex == 3)//polygon slicer file .poly
                    {
                        if ( obj.SaveAs(dlg.FileName) )
                            MessageBox.Show("data saved to file:" + dlg.FileName);
                        else
                            MessageBox.Show("failed to save data to file.\n" + obj.errMessage);
                    }                    
                    else if (dlg.FilterIndex == 2)//.dat scatterred XYZ 
                    {
                        if (obj.ExportLayerPropertyToXYZ(dlg.FileName))
                            MessageBox.Show("data exported to file:" + dlg.FileName);
                        else
                            MessageBox.Show("failed to export data to file.\n" + obj.errMessage);
                    }
                    
                    this.Cursor = DefaultCursor;
                }
            }
        }
        void DoExportTriangle(C3DObjectBase obj,string filter)
        {
            string pathname = "";
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = filter;
                dlg.FilterIndex = 0;
                dlg.FileName = obj.Name;

                if (dlg.ShowDialog() != DialogResult.OK) return;

                pathname = dlg.FileName;
                //文件名默认*.ply
                if ( Path.GetExtension(pathname).Length < 4 )
                {
                    pathname += ".ply";
                }

                Program.m_MainForm.DoExportTriangle(pathname, obj);
            }
        }
        
        private void Do3DGridExport(C3DObjectBase obj, int index = 1)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.Grid3DFileFormatFilter;
                dlg.Filter += "|" + Resource1.XYZVFileFormatFilter;
                dlg.Filter += "|" + Resource1.TriangleFileFormatFilter;
                dlg.Filter += "|" + Resource1.VRMLFileFormatFilter;
                dlg.Filter += "|all files(*.*) | *.*";
                dlg.FilterIndex = index;

                string filename = Path.GetFileNameWithoutExtension(obj.Name);
                if(index == 1) dlg.FileName = filename + ".3DGrid";//默认后缀
                else if (index == 2) dlg.FileName = filename + ".csv";//默认后缀
                else if (index == 3) dlg.FileName = filename + ".ply";//默认后缀
                else if (index == 4) dlg.FileName = filename + ".3DGrid";//默认后缀
                else  dlg.FileName = filename + ".csv";//默认后缀

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string pathname = dlg.FileName;

                    this.Cursor = Cursors.WaitCursor;

                    if (dlg.FilterIndex == 1) //3D grid
                    {
                        if (obj.SaveAs(pathname))
                            MessageBox.Show("data saved to file:" + pathname);
                        else
                            MessageBox.Show("failed to save data to file.\n" + obj.errMessage);
                    }
                    else if (dlg.FilterIndex == 2)//ascii ascii data
                    {
                        if (obj.ExportData(pathname))
                            MessageBox.Show("data exported to file:" + pathname);
                        else
                            MessageBox.Show("failed to export data to file.\n" + obj.errMessage);
                    }
                    else if (dlg.FilterIndex == 3 || dlg.FilterIndex == 4)//3d or vrml
                    {
                        Program.m_MainForm.DoExportTriangle(dlg.FileName, obj);
                    }                    

                    this.Cursor = DefaultCursor;
                }
            }
        }
        
        private void exportDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            C3DObjectBase obj = C3DData.objSelected;
            if (obj == null) return;

            string filter = "";
            switch (obj.type)
            {
                case ShapeEnum.Grid3D:
                    Do3DGridExport(obj);
                    break;
                case ShapeEnum.Triangles:
                case ShapeEnum.Polygon:
                    filter = Resource1.TriangleFileFormatFilter;
                    filter += "|" + Resource1.VRMLFileFormatFilter;
                    filter += "|all files(*.*)|*.*";
                    DoExportTriangle(obj,filter);
                    break;
                case ShapeEnum.Mesh:
                    filter = Resource1.MeshFileFormatFilter;
                    filter += "|" + Resource1.TriangleFileFormatFilter;
                    filter += "|" + Resource1.VRMLFileFormatFilter;
                    filter += "|" + Resource1.SurferGridFileFormatFilter;
                    filter += "|" + Resource1.XYZVFileFormatFilter;
                    filter += "|all files(*.*)|*.*";
                    DoMeshExport((CMesh)obj, filter);
                    break;
                case ShapeEnum.GeoMesh:
                    filter = Resource1.GeoMeshFileFormatFilter;
                    filter += "|" + Resource1.TriangleFileFormatFilter;
                    filter += "|" + Resource1.VRMLFileFormatFilter;
                    filter += "|" + Resource1.SurferGridFileFormatFilter;
                    filter += "|" + Resource1.XYZVFileFormatFilter;
                    filter += "|all files(*.*)|*.*";
                    DoMeshExport((GeoMesh)obj, filter);                
                    break;
                case ShapeEnum.Slicer:
                    filter = Resource1.SlicerFileFormatFilter;
                    filter += "|" + Resource1.TriangleFileFormatFilter;
                    filter += "|" + Resource1.VRMLFileFormatFilter;
                    filter += "|" + Resource1.SurferGridFileFormatFilter;
                    filter += "|" + Resource1.XYZVFileFormatFilter;
                    filter += "|" + Resource1.DXFFileFormatFilter;
                    filter += "|all files(*.*)|*.*";
                    DoSlicerExport((CSlicer)obj, filter);
                    break;
                case ShapeEnum.PolygonSlicer:
                    filter = Resource1.PoligonSlicerFileFormatFilter;
                    filter += "|" + Resource1.XYZVFileFormatFilter;
                    filter += "|all files(*.*)|*.*";
                    DoPolygonSlicerExport((PolygonSlicer)obj, filter);
                    break;
                case ShapeEnum.Polygon2D:
                    filter = Resource1.Polygon2DFileFormatFilter;                    
                    filter += "|all files(*.*)|*.*";
                    DoPolygon2DExport((Polygon2D)obj, filter);
                    break;
                case ShapeEnum.Line:
                    filter = Resource1.LineFileFormatFilter;
                    filter += "|all files(*.*)|*.*";
                    DoLineExport(obj, filter);
                    break;
                case ShapeEnum.Points:
                    filter = Resource1.XYZVFileFormatFilter;
                    filter += "|all files(*.*)|*.*";
                    DoPointsExport(obj, filter);
                    break;
                case ShapeEnum.Borehole:
                    ExportLasdataForm ep = new ExportLasdataForm();
                    ep.boreholes = (CBoreholes)obj;
                    ep.ShowDialog();
                    return;
                default: return;
            }
        }

        private void griddingDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LasInterpolationForm ip = new LasInterpolationForm();
            ip.ShowDialog();
        }

        //从散乱点数据中，创建三角网
        private void createTrianglesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Vector32 p;
            DelaunayTriangulator tr = new DelaunayTriangulator();
            
            Cursor = Cursors.WaitCursor;

            if (C3DData.objSelected.type == ShapeEnum.Boreholes)                
            {
                CBoreholes boreholes = (CBoreholes)C3DData.GetSelectedObj(ShapeEnum.Boreholes);
                if (boreholes != null) 
                {
                    for (int i = 0; i < boreholes.Count; i++)
                    {
                        p = boreholes[i].Position;
                        tr.AddPoint(p.X, p.Y, p.Z);
                    }
                }
            }
            else if (C3DData.objSelected.type == ShapeEnum.Points)
            {
               ScatteredPoints obj = (ScatteredPoints)C3DData.GetSelectedObj(ShapeEnum.Points);
                if (obj != null)
                {
                    foreach(Vector32 p1 in obj.points)
                    {
                        tr.AddPoint(p1.x, p1.y, p1.z);
                    }
                }
            }
            
            if (tr.Count > 3)
            {
                tr.BowyerWatson();
                /*
                var voronoiTimer = Stopwatch.StartNew();
                var vornoiEdges = voronoi.GenerateEdgesFromDelaunay(triangulation);
                voronoiTimer.Stop();
                DrawVoronoi(vornoiEdges);
                */
                
                TriangleObj obj = tr.toTriangleObj();
                obj.Name = C3DData.objSelected.Name + "_angles";

                /*
                double x, y, z;
                for (int i = 0; i < tr.outPointNum; i++)
                {
                    tr.GetPoint(i, out x, out y, out z);
                    obj.AddPoint(new Vector32((float)x, (float)y, (float)z));
                }
                for (int i = 0; i < tr.triangleNum; i++)
                {
                    obj.AddTriangleIndex(tr.GetTriangle(3 * i),
                                         tr.GetTriangle(3 * i + 1),
                                         tr.GetTriangle(3 * i + 2));
                }
                */

                tr.Clear();

                Cursor = Cursors.Default;

                obj.UpdateRange();
                C3DData.AddObject(obj,false);
                
                AddToTree(C3DData.lastLoaded);
                C3DData.objSelected = C3DData.lastLoaded;
                UpdateDraw(C3DData.lastLoaded);
            }            
        }

        //曲线平滑
        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;
            if ( C3DData.objSelected.type != ShapeEnum.Line ) return;

            C3DLine obj = (C3DLine)C3DData.objSelected;
            C3DData.AddObject(obj.Smooth(),false);
            //BezierSmooth bs = new BezierSmooth();
            //bs.points = obj.points;
            AddToTree(C3DData.lastLoaded);
            C3DData.objSelected = C3DData.lastLoaded;
            UpdateDraw(C3DData.lastLoaded);
        }

        private void contextMenuStripLine_Opening(object sender, CancelEventArgs e)
        {

        }

       
        // polygonSlicer trace menu
        private void traceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            if (C3DData.curSel < 0) return;
            PolygonSlicer obj = (PolygonSlicer)C3DData.objSelected;

            TracePathDlg td = new TracePathDlg();
            td.poly = obj.Copy();

            if ( td.ShowDialog() == DialogResult.OK )
            {
                obj = td.poly.Copy();

                obj.TraceGrid();

                obj.TracePolygon();
                C3DData.pObjects[C3DData.curSel] = obj;
                C3DData.objSelected = obj;

                Program.m_MainForm.m_DDDForm.UpdateDraw();
            }*/
        } 

        private void editorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( C3DData.objSelected == null ) return;
            if (C3DData.objSelected.type != ShapeEnum.PolygonSlicer) return;

            PolygonSlicer obj = (PolygonSlicer)C3DData.objSelected;
            SlicerModelingForm sf = new SlicerModelingForm();
            sf.SetSlicer(obj);
            if( sf.ShowDialog() == DialogResult.OK )
            {
                obj = sf.slicer;
                if ( obj.IsLocated ) 
                {
                    obj.UpdateTraced();
                    CDataModel.m_ModelOrg = new CubeModel64(-1, -1, -1, 1, 1, 1);
                    CDataModel.UpdateModelSize(obj);                    
                }               
                C3DData.objSelected = obj;
                UpdateDraw(C3DData.objSelected);
                C3DData.IsDataModified = true;
            }
        }
        //CSlicer Convert to PolygonSlicer then edit
        private void toolSlicerEdit_Click(object sender, EventArgs e)
        {
            if ( C3DData.objSelected == null ) return;
            if ( C3DData.objSelected.type != ShapeEnum.Slicer ) return;

            CSlicer obj = (CSlicer)C3DData.objSelected;

            PolygonSlicer slicer = obj.toPolygonSlicer();
            ImageStruct im = obj.toImage();
            slicer.backImages.Add(im);

            SlicerModelingForm sf = new SlicerModelingForm();
            sf.SetSlicer(slicer);

            if (sf.ShowDialog() == DialogResult.OK)
            {
                slicer = sf.slicer.Copy();

                //从原始切片复制transform信息
                double x1 = slicer.minx;
                double x2 = slicer.maxx;
                double y1 = slicer.miny;
                double y2 = slicer.maxy;
                double z1 = slicer.minz;
                double z2 = slicer.maxz;
                slicer.CopyTransformFrom(obj);
                slicer.minx = x1;
                slicer.maxx = x2;
                slicer.miny = y1;
                slicer.maxy = y2;
                slicer.minz = z1;
                slicer.maxz = z2;
                
                C3DData.AddObject(slicer, false);

                AddToTree(C3DData.lastLoaded);
                C3DData.objSelected = C3DData.lastLoaded;
                UpdateDraw(C3DData.lastLoaded);
                C3DData.IsDataModified = true;
            }
        }
        
        bool MoveObject(int direct,double scale = 1 )
        {
            if (C3DData.objSelected == null) return false;

            if ( !double.TryParse(StepTextBox1.Text,out moveStepPercent) )
            {
                MessageBox.Show("Please input a valid step ( 0 - 100 )%");
                return false;
            }
            if( moveStepPercent < 0 || moveStepPercent > 100 )
            {
                MessageBox.Show("Please input a valid step ( 0 - 100 )%");
                return false;
            }
            
            double step = CDataModel.m_ModelOrg.GetWidth(0) * moveStepPercent / 100.0;

            C3DObjectBase obj = C3DData.objSelected;
            if (C3DData.objSubSelected != null) obj = C3DData.objSubSelected;

            if ( direct == 0)
            {
                obj.DoOffset(step * scale,0,0);
            }
            
            if (direct == 1)
            {
                obj.DoOffset(0,step * scale, 0);
            }
            
            if (direct == 2)
            {
                obj.DoOffset(0, 0, step * scale);                
            }
            obj.RenderMode = RenderingUpdateMode.Redraw;
            UpdateDraw(obj);

            return true;
        }

        private void xIncButton_Click(object sender, EventArgs e)
        {
            MoveObject(0, 1);
        }

        private void xDecButton_Click(object sender, EventArgs e)
        {
            MoveObject(0, -1);
        }

        private void yIncButton_Click(object sender, EventArgs e)
        {
            MoveObject(1, 1);
        }

        private void yDecButton_Click(object sender, EventArgs e)
        {
            MoveObject(1, -1);
        }

        private void zIncButton_Click(object sender, EventArgs e)
        {
            MoveObject(2, 1);
        }

        private void zDecButton_Click(object sender, EventArgs e)
        {
            MoveObject(2, -1);
        }

        private void toSolidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;

            if (C3DData.objSelected.type != ShapeEnum.Mesh) return;

            CMesh obj = (CMesh)C3DData.objSelected;

            MeshToSolidForm me = new MeshToSolidForm(obj);

            if (me.ShowDialog() == DialogResult.OK)
            {
                AddToTree(C3DData.lastLoaded);
                UpdateDraw(C3DData.objSelected);
            }
        }
        private void createBufferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;
            if (C3DData.objSelected.type != ShapeEnum.Mesh) return;

            CMesh obj = (CMesh)C3DData.objSelected;

            CreateMeshBufferForm me = new CreateMeshBufferForm(obj);

            if (me.ShowDialog() == DialogResult.OK)
            {
                UpdateTree();
                UpdateDraw(C3DData.objSelected);
            }
        }
        private void contextMenuStripMesh_Opening(object sender, CancelEventArgs e)
        {

        }

        //图像边缘检测
        private void sobelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;
            if (C3DData.objSelected.type != ShapeEnum.Slicer) return;

            CSlicer obj = (CSlicer)C3DData.objSelected;
            
            CSlicer newobj = obj.Copy();
            newobj.Name = obj.Name + "_Sobel";

            float[] grid = new float[obj.nRow * obj.nCol];

#pragma warning disable CS0168 // 声明了变量“g”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“r”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“b”，但从未使用过
            int r, g, b;
#pragma warning restore CS0168 // 声明了变量“b”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“r”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“g”，但从未使用过
            Color color;
            for (int i = 0; i < grid.Length; i++) 
            {
                color = obj.GetColor(obj.pData[i].v);
                grid[i] =(float)( (color.R * 30.0 + color.G * 59.0 + color.B * 11.0 + 50) / 100.0 );
            }
            
            PictureAlgorithm2D pc = new PictureAlgorithm2D();
            float[] outgrid = pc.Sobel(grid, obj.nCol, obj.nRow, 0, 255);

            Vector64 p;
#pragma warning disable CS0219 // 变量“minv”已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量“maxv”已被赋值，但从未使用过它的值
            double minv = 0, maxv = 0;
#pragma warning restore CS0219 // 变量“maxv”已被赋值，但从未使用过它的值
#pragma warning restore CS0219 // 变量“minv”已被赋值，但从未使用过它的值
#pragma warning disable CS0219 // 变量“k”已被赋值，但从未使用过它的值
            int k = 0;
#pragma warning restore CS0219 // 变量“k”已被赋值，但从未使用过它的值
            for(int i=0;i<outgrid.Length;i++)
            {
                p = newobj.pData[i];
                p.v = outgrid[i];
                newobj.pData[i] = p;

            }
            newobj.minv = 0;
            newobj.maxv = 255;
            newobj.ColorScale.SetValueRange(0, 255);

            outgrid = null;

            C3DData.AddObject(newobj, false);
            UpdateTree();
            UpdateDraw(newobj);
        }

        //从grid文件载入数据，替换切片原数据
        private void fromGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;
            if (C3DData.objSelected.type != ShapeEnum.Slicer) return;

            CSlicer obj = (CSlicer)C3DData.objSelected;

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "grid data(*.grd)|*.grd|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    CSurferGrid cs = new CSurferGrid();
                    if( !cs.Read(dlg.FileName) )
                    {
                        this.Cursor = DefaultCursor;
                        MessageBox.Show("Import grid data failed.\n" + cs.errMessage);
                        return;
                    }

                    obj.SetGridData(new C2DGridData(cs));
                    
                    cs.Clear();
                    
                    UpdateDraw(C3DData.objSelected);

                    this.Cursor = DefaultCursor;
                }
            }//using (var dlg = new OpenFileDialog())

        }//private void fromGridToolStripMenuItem_Click(object sender, EventArgs e)   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            if ( e.Node != treeView1.SelectedNode) return; //引发重复check事件

            //if (C3DData.objSelected == null) return;

                //C3DObjectBase obj = C3DData.objSelected;
                //obj.Visible = treeView1.SelectedNode.Checked;            
                //propertyGrid1.SelectedObject = obj;

                Cursor = Cursors.WaitCursor;
            DoCheckUpdated(treeView1.SelectedNode, treeView1.SelectedNode.Checked);
            Cursor = Cursors.Default;
            //UpdateDraw(obj);
        }        

        void DoCheckUpdated(TreeNode node,bool check)
        {
            if(node.Tag== null )//目录
            {
                foreach (TreeNode p in node.Nodes)
                {
                    p.Checked = check;
                    DoCheckUpdated(p,check);
                }                
            }
            else // object
            {
                C3DObjectBase obj = node.Tag as C3DObjectBase;
                obj.Visible = check;
                obj.RenderMode = RenderingUpdateMode.Visible;
                UpdateDraw(obj);
                return;
            }
        }

        void GetObjectFromNode(TreeNode node)
        {
            if(node.Tag == null)
            {
                foreach (TreeNode p in node.Nodes)
                    GetObjectFromNode(p);
            }
            else
            {
                C3DObjectBase obj = node.Tag as C3DObjectBase;
                if (obj.Visible)
                {
                    obj.RenderMode = RenderingUpdateMode.Redraw;
                    objectsSelected.Add(obj);
                }
                return;
            }            
        }
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            if (C3DData.objSelected == null)//目录
            {
                objectsSelected.Clear();
                GetObjectFromNode(treeView1.SelectedNode);
                UpdateDraw(objectsSelected);
            }
            else
            {
                C3DData.objSelected.RenderMode = RenderingUpdateMode.Redraw;
                UpdateDraw(C3DData.objSelected);
            }
        }      

        private void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView1.GetNodeAt(new Point(e.X,e.Y));
            if( node != null )
            {
                treeView1.SelectedNode = node;
                DoTreeSelected();
                //ChangeChild(node, node.Checked);                
            }
        }
        private void ChangeChild(TreeNode node, bool state)
        {
            node.Checked = state;
            foreach (TreeNode tn in node.Nodes) ChangeChild(tn, state);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                treeView1.LabelEdit = true;
                treeView1.SelectedNode.BeginEdit();
                C3DData.IsDataModified = true;
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if(e.Label != null )
            {
                if( e.Label.Trim().Length > 0)
                {
                    if (C3DData.objSelected != null)
                    { 
                        C3DData.objSelected.Name = e.Label.Trim();
                        propertyGrid1.SelectedObject = C3DData.objSelected;
                    }                    
                }
                else
                {
                    e.CancelEdit = true;
                    MessageBox.Show("Label cannot be empy.","Waring",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            }
            treeView1.LabelEdit = false;
        }

        private void toolStripMenuItemNewFolder_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            if( treeView1.SelectedNode.Tag == null )
            {
                TreeNode node = new TreeNode("New Folder");
                node.Tag = null;
                treeView1.SelectedNode.Nodes.Add(node);
                treeView1.SelectedNode = node;

                C3DData.objSelected = null;
                propertyGrid1.SelectedObject = null;

                treeView1.LabelEdit = true;
                treeView1.SelectedNode.BeginEdit();
                C3DData.IsDataModified = true;
            }
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode myNode = null;
            if ( e.Data.GetDataPresent( typeof(TreeNode) ) )
            {
                myNode = (TreeNode)(e.Data.GetData(typeof(TreeNode)));
            }
            else
            {
                //MessageBox.Show("error");
                return;
            }
            
            Point Position = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode DropNode = this.treeView1.GetNodeAt(Position); //目标节点
            
            // 1.目标节点不是空。2.目标节点不是被拖拽接点的字节点。3.目标节点不是被拖拽节点本身
            if (DropNode != null && DropNode.Parent != myNode && DropNode != myNode)
            {
                TreeNode DragNode = myNode;//当前节点-被拖曳节点
                // 将被拖拽节点从原来位置删除。
                myNode.Remove();

                // 在目标节点下增加被拖拽节点
                DropNode.Nodes.Add(DragNode);

                treeView1.SelectedNode = DragNode;
                C3DData.objSelected = (C3DObjectBase)DragNode.Tag;
                DropNode.Expand();
            }
            // 如果目标节点不存在，即拖拽的位置不存在节点，那么就将被拖拽节点放在根节点之下
            //if (DropNode == null)
            //{
            //    TreeNode DragNode = myNode;
            //    myNode.Remove();
            //    treeView1.Nodes.Add(DragNode);
            //}
        }

        private void dGridDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Do3DGridExport(C3DData.objSelected,1);
        }

        private void ASCIIXYZVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Do3DGridExport(C3DData.objSelected, 2);
        }

        private void formattedtriangles_Click(object sender, EventArgs e)
        {
            Do3DGridExport(C3DData.objSelected, 3);
        }

        private void vRMLFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Do3DGridExport(C3DData.objSelected, 4);
        }

        private void toPolygon2DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            C3DObjectBase obj = C3DData.objSelected;
            if (obj == null) return;

            string filter = Resource1.Polygon2DFileFormatFilter;
            filter += "|all files(*.*)|*.*";            
            Polygon2D poly = (Polygon2D)obj;
            poly.polygonSlicer = null;
            DoPolygon2DExport(poly, filter);
        }

        private void toTracedPolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            C3DObjectBase obj = C3DData.objSelected;
            if ( obj == null ) return;

            string filter = Resource1.Polygon2DFileFormatFilter;
            filter += "|all files(*.*)|*.*";

            PolygonSlicer slicer = null;
            if (slicer == null)
            {
                if (obj.Parent != null && obj.Parent.type == ShapeEnum.PolygonSlicer)
                    slicer = (PolygonSlicer)obj.Parent;
            }
            if (slicer == null)
            {
                if (obj.Parent != null && obj.Parent.Parent != null && obj.Parent.Parent.type == ShapeEnum.PolygonSlicer)
                    slicer = (PolygonSlicer)obj.Parent.Parent;
            }
            Polygon2D poly = (Polygon2D)obj;
            poly.polygonSlicer = slicer;
            DoPolygon2DExport(poly, filter);            
        }

        private void toolStripMenuItemEditor_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;
            if (C3DData.objSelected.type != ShapeEnum.PolygonSlicer) return;          

            PolygonSlicer slicer = C3DData.objSelected as PolygonSlicer;
            SlicerModelingForm sf = new SlicerModelingForm();
            sf.SetSlicer(slicer);

            if (sf.ShowDialog() == DialogResult.OK)
            {
                slicer = sf.slicer;
                //C3DData.AddObject(slicer, false);
                slicer.UpdateDrawMode();
                UpdateTree(slicer);                
                UpdateDraw(slicer);
                C3DData.IsDataModified = true;
            }
        }

        private void inclineAnglesToolStripEditor_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;
            
            if (C3DData.objSelected.type == ShapeEnum.Borehole)
            {
                CBorehole bh = C3DData.objSelected as CBorehole;
                if ( BoreholeInclineEditor(bh) )
                {
                    UpdateTree(bh,1);
                    bh.RenderMode = RenderingUpdateMode.Redraw;
                    UpdateDraw(bh);
                }
            }
            else if (C3DData.objSelected.type == ShapeEnum.BoreholeAngles)
            {
                BoreholeAngles angles = C3DData.objSelected as BoreholeAngles;
                CBorehole bh = angles.Parent as CBorehole;
                if (BoreholeInclineEditor(bh) )
                {
                    UpdateTree(bh, 2);
                    bh.RenderMode = RenderingUpdateMode.Redraw;
                    UpdateDraw(bh);
                }
            }
        }
        bool BoreholeInclineEditor(CBorehole borehole)
        {
            BoreholeInclineForm dlg = new BoreholeInclineForm(borehole);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                CBorehole bh1 = dlg.borehole;
                borehole.boreholeAngles.Clear();
                borehole.boreholeAngles = bh1.boreholeAngles.Copy();
                borehole.CreateBaseLine(CDataModel.IsGeoCoordinateSystem);
                borehole.UpdateRange();
                return true;
            }
            return false;
        }
        bool BoreholeWellStrataEditor(CBorehole bh)
        {
            BoreholeWellDataForm dlg = new BoreholeWellDataForm(bh);
            DialogResult ret = dlg.ShowDialog();
            if ( ret == DialogResult.OK )
            {
                CBorehole bh1 = dlg.borehole;
                bh.Stratums.Clear();
                bh.Stratums.Stratums.AddRange(bh1.Stratums.Stratums);
                bh.CreateBaseLine(CDataModel.IsGeoCoordinateSystem);
                bh.UpdateRange();
                return true;
            }
            else return false;
        }
        bool BoreholesWellStrataEditor(CBoreholes boreholes )
        {
            BoreholesWellDataForm wd = new BoreholesWellDataForm(boreholes);
            if (wd.ShowDialog() == DialogResult.OK)
            {
                if ( wd.boreholes.Count < 1) return false;
                int count = 0;
                foreach (CBorehole bh1 in wd.boreholes.pData)
                {
                    int id = boreholes.pData.FindIndex(a => a.Name == bh1.Name && 
                                                       a.Position == bh1.Position );
                    if (id < 0) continue;

                    CBorehole bh = boreholes[id];
                    bh.Stratums.Clear();
                    bh.Stratums = bh1.Stratums.Copy();
                    bh.CreateBaseLine(CDataModel.IsEarthMapVision);
                    bh.UpdateRange();
                    boreholes[id] = bh;

                    bh1.Clear();
                    count++;
                }
                return count > 0 ;
            }
            return false;
        }
        /// <summary>
        /// 单井/多井地层编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void strataToolStripEditor_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null ) return;
           
            if ( C3DData.objSelected.type == ShapeEnum.Boreholes )
            {
                CBoreholes boreholes = C3DData.objSelected as CBoreholes;
                if( BoreholesWellStrataEditor(boreholes) )
                {
                    UpdateTree(boreholes);
                    boreholes.RenderMode = RenderingUpdateMode.Redraw;
                    UpdateDraw(boreholes);
                }
            }
            else if (C3DData.objSelected.type == ShapeEnum.Borehole)
            {
                CBorehole bh = C3DData.objSelected as CBorehole;
                if( BoreholeWellStrataEditor(bh) )
                {
                    UpdateTree(bh);
                    bh.RenderMode = RenderingUpdateMode.Redraw;
                    UpdateDraw(bh);
                }
            }
            else if (C3DData.objSelected.type == ShapeEnum.BoreholeStratums )
            {
                StratumDatas strata = C3DData.objSelected as StratumDatas;
                CBorehole bh = strata.Parent as CBorehole;
                if( BoreholeWellStrataEditor(bh) )
                {
                    UpdateTree(bh);
                    bh.RenderMode = RenderingUpdateMode.Redraw;
                    UpdateDraw(bh);
                }
            }           
        }        
        
        private void toolEditorBoreholeSub_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected == null) return;
            if (C3DData.objSelected.type == ShapeEnum.BoreholeAngles)
                inclineAnglesToolStripEditor_Click(sender,e);
            else if (C3DData.objSelected.type == ShapeEnum.BoreholeStratums)
                strataToolStripEditor_Click(sender, e);
            else if (C3DData.objSelected.type == ShapeEnum.BoreholeCurves)
                BoreholeCurvesEditor();
        }
        void BoreholeCurvesEditor()
        {
            if (C3DData.objSelected == null) return;
            CBorehole bh = null;
            if (C3DData.objSelected.type == ShapeEnum.Borehole)
            {
                bh = C3DData.objSelected as CBorehole;                
            }
            else if (C3DData.objSelected.type == ShapeEnum.BoreholeCurves)
            {
                BoreholeCurves curves = C3DData.objSelected as BoreholeCurves;
                bh = curves.Parent as CBorehole;
            }

            BoreholeCurvesForm bf = new BoreholeCurvesForm(bh);
            if (bf.ShowDialog() == DialogResult.OK)
            {
                bh.Curves.Clear();
                
                if (bf.lasDataReplaced) bh.Curves.lasData = bf.borehole.Curves.lasData;
                bh.Curves.Curves.AddRange(bf.borehole.Curves.Curves);

                UpdateTree(bh);
                bh.RenderMode = RenderingUpdateMode.Redraw;
                UpdateDraw(bh);
            }
        }

        private void wellCurvesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BoreholeCurvesEditor();
        }

        private void interpolationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected.type == ShapeEnum.Boreholes)
            {
                CBoreholes obj = C3DData.objSelected as CBoreholes;
                BoreholesCurvesInterpolationForm bf = new BoreholesCurvesInterpolationForm(obj);
                if (bf.ShowDialog() == DialogResult.OK)
                {

                }
            }
            
        }

        private void slicerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (C3DData.objSelected.type == ShapeEnum.Boreholes)
            {
                CBoreholes obj = C3DData.objSelected as CBoreholes;
                BoreholesSlicerForm bf = new BoreholesSlicerForm(obj);
                if (bf.ShowDialog() == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    BoreholesInterpolation2D ip = new BoreholesInterpolation2D(bf.selectedBoreholes);
                    ip.InterpolatePropertyName = bf.selectedProperty;
                    CSlicer slicer = ip.CreateLasSlicer();
                    Cursor = Cursors.Default;
                    if ( slicer != null )
                    {
                        C3DData.AddObject(slicer, false);
                        AddToTree(C3DData.lastLoaded);
                        C3DData.objSelected = C3DData.lastLoaded;
                        UpdateDraw(C3DData.lastLoaded);
                    }
                }
            }
            
        }

        /////////////////////////////////////
    }//end of class
    public struct TreeNodeStruct
    {
        public TreeNode Node; //当前节点
        public TreeNode Parent;//当前节点父节点
        public int Index;//当前节点在父节点的顺序
        public TreeNodeStruct(TreeNode _node,TreeNode _parent,int _index)
        {
            Node = _node;
            Parent = _parent;
            Index = _index;
        }
    }
}
