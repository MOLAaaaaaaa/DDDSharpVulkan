using DataCollection;
using DataCollection.DSI3DTriangulation;
using OpenCLNet;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DDDSharp.Modeling
{
    public partial class OutlinesModelingForm : Form
    {
        public List<PolygonSlicer> slicers = new List<PolygonSlicer>();
        List<LayerProperty> layers = new List<LayerProperty>();
        List<string> Selectedlayers = new List<string>();
        public TriangleObj modelingResult = null;
        string errMessage = "";
        public OutlinesModelingForm()
        {
            InitializeComponent();
        }

       
        /// <summary>
        /// 从空间轮廓线中构建三维地层模型
        /// </summary>
        /// <param name="polys"> 空间轮廓线对象数组-按顺序排列 </param>
        /// <returns>三维模型</returns>
        TriangleObj CreateModelFromTracedPolygons( List<ContourPolygon3D> contours)
        {            
            try
            {
                // 2. 初始化DSI三角化器
                Dsi3DTriangulator triangulator = new Dsi3DTriangulator();

                // 3. 构建三维三角网模型
                var (vertices, triangles) = triangulator.BuildModel(contours);
                triangulator.ExportToPlyAscii("dsi_3d_model_ascii.ply");
                // 4. 输出结果               
                TriangleObj obj = triangulator.toTriangleObj();
                obj.Name = "Model-Of-" + contours[0].Name;
                return obj;
            }
            catch (Exception ex)
            {
                errMessage = $"模型构建失败：{ex.Message}\n{ex.StackTrace}";                
                return null;
            }
        }
        /// <summary>
        /// 创建投影后的多边形
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        List<Polygon2D> GetProjectedPolygons(string layer)
        {
            List<Polygon2D> polys = new List<Polygon2D>();
            foreach (PolygonSlicer slicer in slicers)
            {
                //pickup polygons named layer from slicers and store to polys
                //此处：一个切片上可能存在多个同名的轮廓多边形，算法先考虑只有一个的情况
                foreach (Polygon2D poly in slicer.tracedGeoObjects.Polygons)
                {
                    //过滤：无效，不可见，非多边形
                    if (!poly.Visible || !poly.IsValid || !poly.IsClosed) continue;
                    if (poly.Name.ToLower() == layer.ToLower())
                    {
                        polys.Add(poly.toProjectedPolygon());
                    }
                }
            }
            return polys;
        }
        ContourPolygon3D toContourPolygon3D(Polygon2D poly, PolygonSlicer slicer)
        {
            ContourPolygon3D contour = new ContourPolygon3D(poly.Name);
            Polygon2D poly1 = poly.Smooth();
            foreach (Vector64 p in poly1.points)
            {
                Vector64 p1 = slicer.toTracedPoint(p);
                contour.AddVertex(p1.X, p1.Z, p1.Y);
            }
            return contour;
        }
        List<Polygon2D>CreateSmoothedPolygons(string layer)
        {
            List<Polygon2D> polys = new List<Polygon2D>();
            foreach (PolygonSlicer slicer in slicers)
            {
                //pickup polygons named layer from slicers and store to polys
                //此处：一个切片上可能存在多个同名的轮廓多边形，算法先考虑只有一个的情况
                foreach (Polygon2D poly in slicer.tracedGeoObjects.Polygons)
                {
                    //过滤：无效，不可见，非多边形
                    if (!poly.Visible || !poly.IsValid || !poly.IsClosed) continue;
                    if (poly.Name.ToLower() == layer.ToLower())
                    {
                        polys.Add(poly.Smooth());
                    }
                }
            }
            return polys;
        }

        /// <summary>
        /// 创建坐标映射后的多边形（3D）
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        List<ContourPolygon3D> GetTracedLayerPolygons( string layer )
        {
            List<Polygon2D> polys = CreateSmoothedPolygons(layer);
            int max = 0;
            for(int i=0;i<polys.Count; i++) 
            {
                if (polys[i].Count > max)max = polys[i].Count;
            }
            for (int i = 0; i < polys.Count; i++)
            {
                Polygon2D poly = polys[i];
                List<Vector64> _points = Vector64.Resample(poly.points, max);
                poly.points.Clear();
                poly.points = _points;
            }

            List<ContourPolygon3D> contours = new List<ContourPolygon3D>();
            for (int i = 0; i < polys.Count; i++)
            {
                Polygon2D poly = polys[i];
                ContourPolygon3D contour = new ContourPolygon3D(layer);
                foreach (Vector64 p in poly.points)
                {
                    Vector64 p1 = slicers[i].toTracedPoint(p);
                    contour.AddVertex(p1.X,p1.Z,p1.Y);
                }
                contours.Add(contour);
            }
            
            polys.Clear();

            return contours;
        }

        TriangleObj CreateModelFromSlicers()
        {
            //是否载入足够轮廓线
            if( slicers.Count < 2 )
            {
                errMessage = "请载入轮廓线";
                return null;
            }

            //是否选择了建模对象
            if ( GetSelectedLayers() < 1 )
            {
                errMessage = "请选择建模地层对象";
                return null;
            }

            List<ContourPolygon3D> contours = GetTracedLayerPolygons(Selectedlayers[0]);
            if (contours.Count < 1) 
            {
                errMessage = "no enough polygons by selected layer.";
                return null; 
            }

            //构建选中的地层模型
            return CreateModelFromTracedPolygons(contours);
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            //----此处产生三维模型 modelingResult ------------
            modelingResult = CreateModelFromSlicers();

            if( modelingResult == null )
            {
                MessageBox.Show(errMessage, "创建模型失败！");                
            }
            else 
            {
                MessageBox.Show("创建模型成功！");
            }
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void LoadSlicerButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Slicers(*.Slicer)|*.Slicer|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        slicers.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            PolygonSlicer slicer = new PolygonSlicer();
                            if (slicer.LoadFrom(dlg.FileNames[i]))
                            {
                                slicers.Add(slicer);
                            }
                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
                SearchLayerValues();
                UpdateList1();
                UpdateList2();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //////////////////////////////////////////////
        void SearchLayerValues()
        {
            layers.Clear();
            PolygonSlicer s;
            Polygon2D p;
            for (int i = 0; i < slicers.Count; i++)
            {
                s = slicers[i];
                for (int j = 0; j < s.tracedGeoObjects.Count; j++)
                {
                    p = s.tracedGeoObjects[j];
                    if ( !p.IsClosed ) continue;//不考虑线对象

                    if (!IsInList(p.Name))
                    {
                        LayerProperty layer = new LayerProperty(p.Name, p.PropertyValue);
                        layer.LayerColor = p.fillColor;
                        layers.Add(layer);
                    }
                }
            }
            //layers.Sort();
        }
        bool IsInList(string name)
        {
            foreach (LayerProperty s in layers)
            {
                if (s.LayerName.ToLower() == name.ToLower()) return true;
            }
            return false;
        }
        int GetSelectedLayers()
        {
            Selectedlayers.Clear();
            int count = listBox2.Items.Count;
            if (count < 1) return 0;

            bool[] marks = new bool[count];
            for (int i = 0; i < count; i++)
                marks[i] = false;

            int id = 0;
            for (int i = 0; i < listBox2.SelectedIndices.Count; i++)
            {
                id = listBox2.SelectedIndices[i];
                if (!marks[id])
                {
                    Selectedlayers.Add(layers[id].LayerName.ToString().ToLower().Trim());
                    marks[id] = true;
                }
            }
            return Selectedlayers.Count;
        }
        private void UpdateList1()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < slicers.Count; i++)
            {
                listBox1.Items.Add(slicers[i].Name);
            }
            listBox1.SelectedItems.Clear();
        }

        private void UpdateList2()
        {
            listBox2.Items.Clear();

            for (int i = 0; i < layers.Count; i++)
            {
                listBox2.Items.Add(layers[i].LayerName);
            }
            listBox2.SelectionMode = SelectionMode.MultiExtended;
            listBox2.SelectedItems.Clear();
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel <= 0) return;
            PolygonSlicer cur = slicers[sel];
            slicers[sel] = slicers[sel - 1];
            slicers[sel - 1] = cur;
            UpdateList1();
            listBox1.SelectedIndex = sel - 1;
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= slicers.Count) return;

            PolygonSlicer cur = slicers[sel];
            slicers[sel] = slicers[sel + 1];
            slicers[sel + 1] = cur;
            UpdateList1();
            listBox1.SelectedIndex = sel + 1;
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= slicers.Count) return;
            slicers.RemoveAt(sel);
            listBox1.Items.RemoveAt(sel);
            if (listBox1.Items.Count > sel) listBox1.SelectedIndex = sel;
            SearchLayerValues();            
            UpdateList2();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            slicers.Clear();
            layers.Clear();
            Selectedlayers.Clear();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
        }

        private void OutlinesModelingForm_Load(object sender, EventArgs e)
        {
            slicers.Clear();
            foreach(var item in C3DData.objectsDiction)
            {
                C3DObjectBase obj = item.Value;
                if( obj.type == ShapeEnum.PolygonSlicer )
                {
                    slicers.Add(obj as PolygonSlicer);
                }
            }

            if (slicers.Count > 0)
            {
                slicers.Sort((a, b) => { return (a.Name).CompareTo(b.Name); });
                SearchLayerValues();
                UpdateList1();
                UpdateList2();
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
