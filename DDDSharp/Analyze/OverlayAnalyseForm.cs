using DataCollection;
using DDDSharp.Boreholes;
using Graphics3D;
using KdTree;
using KdTree.Math;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using NetTopologySuite.Triangulate.Tri;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Khronos.Platform;

namespace DDDSharp.Analyze
{
    public enum MineralContralMetaTypeEnum
    {
        None = 0,
        断层控矿 = 1,
        地质岩性控矿2D = 2,
        地质岩性控矿3D = 3,
        属性控矿3D = 4,
        物化遥属性控矿2D = 5,
        断裂控矿2D = 6,
        断裂带控矿2D = 7,
        断裂控矿3D = 8,
        其他 = 9,
        距离控矿 = 10,
        矿体趋势分析 = 11,
    }
    public enum Fault2DPropertyEnum
    {
        缓冲区 = 0,
        东向 = 1,
        西向 = 2,
        南向 = 3,
        北向 = 4,        
        断层带 = 5,
    }
    public enum LogicalBoolTypeEnum 
    {
        And = 0,
        Or = 1,
        Not = 2,
    }
    
    //[TypeConverter(typeof(CategoriesSortedByClassDefinitionConverter))]
    //public struct OverlayPropertyStruct
    //{
    //    public int TypeSel;//类型
    //    public string TypeName;
    //    public int ObjectSel;//对象
    //    public string ObjectName;
    //    public int PropertySel;//属性
    //    public string PropertyName;

    //    [CategoryAttribute("Object"), DisplayNameAttribute("Object Name")]
    //    public string _ObjectName { get { return ObjectName; } }
    //    [CategoryAttribute("Object"), DisplayNameAttribute("Property Type")]
    //    public string _TypeName { get { return TypeName; } }

    //    [CategoryAttribute("Object"), DisplayNameAttribute("Property Name")]
    //    public string _PropertyName { get { return PropertyName; } }        

    //    [CategoryAttribute("Properties"), DisplayNameAttribute("Minimum Value")]
    //    public double Val1 { get; set; }
    //    [CategoryAttribute("Properties"), DisplayNameAttribute("Maximum Value")]
    //    public double Val2 { get; set; }
    //    [CategoryAttribute("Properties"), DisplayNameAttribute("Weight Value")]
    //    public double Weight { get; set; }

    //    [CategoryAttribute("Properties"), DisplayNameAttribute("Maximum Weight")]
    //    public double MaxWeight { get; set; }

    //    [CategoryAttribute("Properties"), DisplayNameAttribute("Inverse")]
    //    public bool Inverse { get; set; }        

    //    public int Count;
    //    public OverlayPropertyStruct(int count )
    //    {
    //        TypeSel = -1;
    //        ObjectSel = -1;            
    //        ObjectName = "";
    //        TypeName = "";
    //        PropertySel = -1;
    //        PropertyName = "";
    //        Val1 = 0;
    //        Val2 = 0;
    //        Weight = 0;
    //        MaxWeight = 100;
    //        Count = 0;
    //        Inverse = false;
    //    }
    //    public string toString()
    //    {
    //        string ss = TypeName + "|";
    //        ss += ObjectName + "|";
    //        ss += PropertyName + "|";            
    //        ss += Val1.ToString() + " to ";
    //        ss += Val2.ToString() + "|";
    //        ss += "Weight = " + Weight;
    //        if(Inverse) ss += "| Inversed";
    //        return ss;
    //    }
    //    public bool IsValid()
    //    {
    //        if (TypeSel < 0 || ObjectSel < 0 || PropertySel < 0 ) return false;
    //        if (Val1 > Val2) return false;
    //        if (TypeName.Length < 1 || 
    //            ObjectName.Length < 1 || 
    //            PropertyName.Length < 1) return false;
    //        return true;
    //    }
    //    public MineralContralMetaTypeEnum GetControlType()
    //    {
    //        return (MineralContralMetaTypeEnum)Enum.Parse(typeof(MineralContralMetaTypeEnum),
    //            TypeName);
    //    }
    //    public bool Write(BinaryWriter bw)
    //    {
    //        bw.Write(TypeSel);
    //        C3DData.SaveString(bw, TypeName);
    //        bw.Write(ObjectSel);
    //        C3DData.SaveString(bw, ObjectName);
    //        bw.Write(PropertySel);
    //        C3DData.SaveString(bw, PropertyName);
    //        bw.Write(Val1);
    //        bw.Write(Val2);
    //        bw.Write(Weight);
    //        bw.Write(MaxWeight);
    //        bw.Write(Count);
    //        bw.Write(Inverse);
    //        return true;
    //    }
    //    public bool Read(BinaryReader br)
    //    {
    //        TypeSel = br.ReadInt32();
    //        TypeName = C3DData.LoadString(br);
    //        ObjectSel = br.ReadInt32();
    //        ObjectName = C3DData.LoadString(br);
    //        PropertySel = br.ReadInt32();
    //        PropertyName = C3DData.LoadString(br);
    //        Val1 = br.ReadDouble();
    //        Val2 = br.ReadDouble();
    //        Weight = br.ReadDouble();
    //        MaxWeight = br.ReadDouble();
    //        Count = br.ReadInt32();
    //        Inverse = br.ReadBoolean();
    //        return true;
    //    }
    //}
    
    //public struct GridPropertyStruct
    //{
    //    public List<OverlayPropertyStruct> Properties;
    //    public GridPropertyStruct(List<OverlayPropertyStruct> properties)
    //    {
    //        Properties = new List<OverlayPropertyStruct>(properties);
    //    }
    //    public int GetCount(OverlayPropertyStruct p)
    //    {
    //        for (int i = 0; i < Properties.Count; i++)
    //        {
    //            OverlayPropertyStruct pe = Properties[i];
    //            if (pe.ObjectSel == p.ObjectSel &&
    //                pe.PropertyName == p.PropertyName)
    //                return Properties[i].Count;
    //        }
    //        return 0;
    //    }
    //    public int GetCount(int id)
    //    {
    //        if (id >= 0) return Properties[id].Count;
    //        else return -1;
    //    }
    //    public void SetCount(int id)
    //    {
    //        OverlayPropertyStruct pe = Properties[id];
    //        pe.Count++;
    //        Properties[id] = pe;
    //    }
    //    public void SetCount(OverlayPropertyStruct p)
    //    {
    //        for (int i = 0; i < Properties.Count; i++)
    //        {
    //            OverlayPropertyStruct pe = Properties[i];
    //            if (pe.ObjectSel == p.ObjectSel &&
    //                pe.PropertyName == p.PropertyName)
    //            {
    //                pe.Count++;
    //                Properties[i] = pe;
    //            }
    //        }
    //    }
    //}
    public partial class OverlayAnalyseForm : Form
    {
        int[,] AIndex = null;
        double minx, maxx, miny, maxy, minz, maxz;
        public bool newAnalysing = true;
        public C3DGridData data = null;
        public List<C3DObjectBase>Objects = new List<C3DObjectBase>();
        public List<OverlayPropertyPara> Properties = new List<OverlayPropertyPara>();
        
        bool []doneGrids = null;         //已叠加，不重复叠加
        float[] curGridBuffer = null; //当前缓冲区，和3DGridData一致
        OverlayPropertyPara lastProperty = new OverlayPropertyPara();//上次叠加数据
        OverlayPropertyPara overlayPropertyPara = new OverlayPropertyPara();
        public OverlayAnalyseForm(List<C3DObjectBase>objs, C3DGridData _data = null)
        {
            InitializeComponent();
            
            textBoxNX.Text = "101";
            textBoxNY.Text = "101";
            textBoxNZ.Text = "101";

            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i].type == ShapeEnum.Polygon ||
                    objs[i].type == ShapeEnum.Grid3D  ||
                    objs[i].type == ShapeEnum.Mesh  ||
                    objs[i].type == ShapeEnum.GeoLayerMeshes ||
                    objs[i].type == ShapeEnum.PolygonSlicer ||
                    objs[i].type == ShapeEnum.Triangles ||
                    objs[i].type == ShapeEnum.Slicer ) 
                {
                    Objects.Add(objs[i]);    
                }
            }
            
            if(_data != null) 
            {
                data = _data;
                newAnalysing = false;
            }

            UpdateDataRange();
        }
        void UpdateDataRange()
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                C3DObjectBase s = Objects[i];
                if (i == 0)
                {
                    minx = s.Minx;
                    maxx = s.Maxx;
                    miny = s.Miny;
                    maxy = s.Maxy;
                    minz = s.Minz;
                    maxz = s.Maxz;                    
                }
                else
                {
                    if (s.Minx < minx) minx = s.Minx;
                    if (s.Miny < miny) miny = s.Miny;
                    if (s.Minz < minz) minz = s.Minz;
                    if (s.Maxx > maxx) maxx = s.Maxx;
                    if (s.Maxy > maxy) maxy = s.Maxy;
                    if (s.Maxz > maxz) maxz = s.Maxz;                    
                }
            }
        }
        MineralContralMetaTypeEnum GetControlType(string typename)
        {
            return (MineralContralMetaTypeEnum)Enum.Parse(typeof(MineralContralMetaTypeEnum),
               typename);
        }
        private void OverlayAnalyseForm_Load(object sender, EventArgs e)
        {            
            C3DObjectBase obj = null;
            for(int i=0;i< Objects.Count;i++) 
            {
                obj = Objects[i];
                string ss = obj.type.ToString()+"_";
                ss += obj.Name;
                listBox1.Items.Add(ss);
            }            
            TypeComboBox.Items.AddRange(Enum.GetNames(typeof(MineralContralMetaTypeEnum)));
            UpdateRangeGeometry();            
        }

        void UpdatePropertySelected()
        {
            int sel1 = listBox1.SelectedIndex;
            if (sel1 < 0) return;
            int sel2 = PropertyComboBox.SelectedIndex;
            if(sel2 < 0) return;

            C3DObjectBase obj = Objects[sel1];
            double v1 = 0, v2 = 0;
            if ( obj.type == ShapeEnum.Grid3D )
            {                
                C3DGridData grid = (C3DGridData)obj;
                for(int i=0;i< grid.ColorScale.Levels.Count;i++)
                {
                    if (grid.ColorScale.Levels[i].Visible) 
                    { 
                        v1 = grid.ColorScale.Levels[i].LevelValue;
                        v1 = grid.ColorScale.minv + v1 * (grid.ColorScale.maxv - grid.ColorScale.minv) / 100;
                        break;
                    }
                }
                for (int i = grid.ColorScale.Levels.Count-1; i>=0; i--)
                {
                    if (grid.ColorScale.Levels[i].Visible)
                    {
                        v2 = grid.ColorScale.Levels[i].LevelValue;
                        v2 = grid.ColorScale.minv + v2 * (grid.ColorScale.maxv - grid.ColorScale.minv) / 100;
                        break;
                    }
                }
                overlayPropertyPara = new OverlayPropertyPara3DGrid();
                overlayPropertyPara.Minimum = Math.Round(v1, 4);
                overlayPropertyPara.Maximum = Math.Round(v2, 4);
                overlayPropertyPara.Info = "Minimum = " + Math.Round(v1,4) + "; Maximum = " + Math.Round(v2, 4);
                PropertyChoosePropertyGrid.SelectedObject = overlayPropertyPara;
            }            
            else if (obj.type == ShapeEnum.PolygonSlicer)
            {
                PolygonSlicer poly = (PolygonSlicer)obj;
                                
                MineralContralMetaTypeEnum m = (MineralContralMetaTypeEnum)Enum.Parse(typeof(MineralContralMetaTypeEnum),
                TypeComboBox.SelectedItem.ToString());

                //MineralContralMetaTypeEnum m = overlayPropertyPara.GetControlType();
                if ( m == MineralContralMetaTypeEnum.地质岩性控矿2D )
                {
                    overlayPropertyPara = new OverlayPropertyPara岩性控矿2D();                    
                    v1 = minz; 
                    v2 = maxz;
                }
                if(m==MineralContralMetaTypeEnum.距离控矿)
                {
                    overlayPropertyPara = new OverlayPropertyParaDistanceGaosi();
                    overlayPropertyPara.Minimum = 0;
                    overlayPropertyPara.Maximum = 10000;
                    overlayPropertyPara.Info = "Minimum = " + Math.Round(v1, 4) + "; Maximum = " + Math.Round(v2, 4);
                }
                if ( m == MineralContralMetaTypeEnum.断裂控矿2D )
                {
                    overlayPropertyPara = new OverlayPropertyPara断裂控矿2D();                    
                    if (overlayPropertyPara.PropertyName.Contains("东向") ||
                        overlayPropertyPara.PropertyName.Contains("西向"))
                    {
                        v1 = 0; 
                        v2 = maxx - minx;
                    }
                    else if (overlayPropertyPara.PropertyName.Contains("南向") ||
                             overlayPropertyPara.PropertyName.Contains("北向"))
                    {
                        v1 = 0; v2 = maxy - miny;
                    }
                }              

                overlayPropertyPara.Minimum = Math.Round(v1, 4);
                overlayPropertyPara.Maximum = Math.Round(v2, 4);
                overlayPropertyPara.Info = "Minimum = " + Math.Round(v1, 4) + "; Maximum = " + Math.Round(v2, 4);
                
            }
            else if (obj.type == ShapeEnum.Triangles)   //距离控矿-高斯模型
            {
                TriangleObj tri = (TriangleObj)obj;                
                overlayPropertyPara = new OverlayPropertyParaDistanceGaosi();                
                overlayPropertyPara.Minimum = 0;
                overlayPropertyPara.Maximum = 10000;
                overlayPropertyPara.Info = "Minimum = " + Math.Round(v1, 4) + "; Maximum = " + Math.Round(v2, 4);
            }
            else if (obj.type == ShapeEnum.GeoLayerMeshes) //地层趋势面分析
            {
                GeoLayerMeshes mesh = (GeoLayerMeshes)obj;
                overlayPropertyPara = new OverlayPropertyParaTrendAnalysis();                
                overlayPropertyPara.Minimum = 0;
                overlayPropertyPara.Maximum = 0;
                overlayPropertyPara.Info = "Minimum = " + Math.Round(v1, 4) + "; Maximum = " + Math.Round(v2, 4);
            }
            PropertyChoosePropertyGrid.SelectedObject = overlayPropertyPara;
        }
        
        
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TypeComboBox.Items.Clear();
            TypeComboBox.Text = string.Empty;
            TypeComboBox.SelectedIndex = -1;

            PropertyComboBox.Items.Clear();
            PropertyComboBox.Text = string.Empty;
            PropertyComboBox.SelectedIndex = -1;

            int sel = listBox1.SelectedIndex;            
            if (sel < 0) return;
            C3DObjectBase obj = Objects[sel];
            if (obj.type == ShapeEnum.Grid3D)
            {                
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.属性控矿3D);
            }
            else if (obj.type == ShapeEnum.PolygonSlicer)
            {
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.地质岩性控矿2D);
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.距离控矿);
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.断裂控矿2D);
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.断裂带控矿2D);
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.断裂控矿3D);
            }
            else if (obj.type == ShapeEnum.Mesh)
            {
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.地质岩性控矿2D);
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.断裂控矿3D);
            }
            else if (obj.type == ShapeEnum.Triangles)
            {
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.距离控矿);                
            }
            else if (obj.type == ShapeEnum.GeoLayerMeshes)
            {
                TypeComboBox.Items.Add(MineralContralMetaTypeEnum.矿体趋势分析);
            }

            TypeComboBox.SelectedIndex = -1;
        }

        void UpdateListBox2()
        {
            listBox2.Items.Clear();
            for ( int i=0;i<Properties.Count;i++) 
            {
                OverlayPropertyPara pe = Properties[i];
                listBox2.Items.Add(pe.toString());
            }
            listBox2.SelectedIndex = -1;
        }
        bool IsExisted(OverlayPropertyPara pe)
        {
            for(int i=0;i<Properties.Count;i++)
            {
                OverlayPropertyPara p = Properties[i];
                if (
                    p.ObjectSel == pe.ObjectSel &&
                    p.TypeSel == pe.TypeSel &&
                    p.PropertySel == pe.PropertySel)
                    return true;
            }
            return false;
        }

        bool UpdateCurPropertyParameter()
        {            
            int sel1 = listBox1.SelectedIndex;
            int sel2 = TypeComboBox.SelectedIndex;
            int sel3 = PropertyComboBox.SelectedIndex;
            if (sel1 < 0 || sel2 < 0 || sel3 < 0) return false;
            try 
            {   
                overlayPropertyPara.ObjectSel = sel1;
                overlayPropertyPara.TypeSel = sel2;
                overlayPropertyPara.PropertySel = sel3;
                overlayPropertyPara.ObjectName = Objects[sel1].Name;
                overlayPropertyPara.TypeName = TypeComboBox.Items[sel2].ToString();
                overlayPropertyPara.PropertyName = PropertyComboBox.Items[sel3].ToString();
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private void AddToButton_Click(object sender, EventArgs e)
        {
            UpdateCurPropertyParameter();
            if ( IsExisted(overlayPropertyPara) )
            {
                MessageBox.Show("Same property is already existed.");
                return;
            }
            Properties.Add(overlayPropertyPara);
            UpdateListBox2();            
            UpdateRangeGeometry();
        }
        void UpdateRangeGeometry()
        {
            textBoxX1.Text = Math.Round(minx, 4).ToString();
            textBoxX2.Text = Math.Round(maxx, 4).ToString();
            textBoxY1.Text = Math.Round(miny, 4).ToString();
            textBoxY2.Text = Math.Round(maxy, 4).ToString();
            textBoxZ1.Text = Math.Round(minz, 4).ToString();
            textBoxZ2.Text = Math.Round(maxz, 4).ToString();
        }
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            int sel3 = listBox2.SelectedIndex;
            if (sel3 < 0) return;
            Properties.RemoveAt(sel3);
            UpdateListBox2();
        }
        
        void GetPolygonslicerRange(OverlayPropertyPara p, out double x1, out double x2, 
                                                            out double y1, out double y2,
                                                            out double z1, out double z2)
        {
            PolygonSlicer slicer = Objects[p.ObjectSel] as PolygonSlicer;
            int k = 0;
            x1 = x2 = 0;
            y1 = y2 = 0;
            z1 = z2 = 0;
            for(int i=0;i<slicer.tracedGeoObjects.Count;i++)
            {
                Polygon2D poly = slicer.tracedGeoObjects[i];
                if (poly.Name.ToLower() != p.PropertyName.ToLower()) continue;
                Vector64 p1 = new Vector64(poly.minx, poly.minx, poly.minz);
                Vector64 p2 = new Vector64(poly.maxx, poly.maxx, poly.maxz);
                p1 = slicer.toTracedPoint(p1);
                p2 = slicer.toTracedPoint(p2);
                if (k == 0) 
                {                    
                    x1 = Math.Min(p1.X, p2.X);
                    x2 = Math.Max(p1.X, p2.X);
                    y1 = Math.Min(p1.Y, p2.Y);
                    y2 = Math.Max(p1.Y, p2.Y);
                    z1 = Math.Min(p1.Z, p2.Z);
                    z2 = Math.Max(p1.Z, p2.Z);
                }
                else 
                {
                    if (x1 > Math.Min(p1.X, p2.X)) x1 = Math.Min(p1.X, p2.X);
                    if (y1 > Math.Min(p1.Y, p2.Y)) y1 = Math.Min(p1.Y, p2.Y);
                    if (z1 > Math.Min(p1.Z, p2.Z)) z1 = Math.Min(p1.Z, p2.Z);
                    if (x2 < Math.Min(p1.X, p2.X)) x2 = Math.Min(p1.X, p2.X);
                    if (y2 < Math.Min(p1.Y, p2.Y)) y2 = Math.Min(p1.Y, p2.Y);
                    if (z2 < Math.Min(p1.Z, p2.Z)) z2 = Math.Min(p1.Z, p2.Z);
                }
                k++;
            } 
        }
        
        
        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }           

        private void LoadStratum_Click(object sender, EventArgs e)
        {
            GeoLayerEditor dlg = new GeoLayerEditor();
            dlg.stratums = C3DData.Stratums.Copy();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                C3DData.Stratums = dlg.stratums.Copy();
            }
        }
        void DoGrid3DBuffer(C3DObjectBase obj,
                            OverlayPropertyPara ops,
                            C3DGridData data)
        {
            C3DGridData grid = obj as C3DGridData;
            if (ops.PropertyName == "Grid Value") DoGrid3DBufferOnGridValue(grid,ops,data);
            else DoGrid3DBufferOnStratumValue(grid, ops, data);
        }
        
        void ResetDoneGrids(int length)
        {
            if(doneGrids == null) doneGrids = new bool[length];
            for (int i = 0; i < length; i++) doneGrids[i] = false;
        }
        void ResetCurGridBuffer(int length)
        {
            if( curGridBuffer == null) curGridBuffer = new float[data.Length];
            for (int i = 0; i < curGridBuffer.Length; i++) curGridBuffer[i] = 0;
        }

        
        bool IsPropertyDone(OverlayPropertyPara ops)
        {
            if( ops.ObjectSel == lastProperty.ObjectSel)
            {
                if (ops.TypeName == "断裂带控矿2D" && lastProperty.TypeName == "断裂带控矿2D")
                    return true;
                if (ops.TypeName == "断裂控矿2D" && lastProperty.TypeName == "断裂控矿2D")
                    return true;
                if (ops.TypeName == "断裂控矿2D" && lastProperty.TypeName == "断裂带控矿2D")
                    return true;
                if (ops.TypeName == "断裂带控矿2D" && lastProperty.TypeName == "断裂控矿2D")
                    return true;

                if (ops.TypeSel == lastProperty.TypeSel) return true;
                else return false;
            }
            return false;            
        }

        void DoGrid3DBufferOnGridValue(C3DGridData grid,
                             OverlayPropertyPara ops,
                             C3DGridData data)
        {

            long id;
            double val;
            Vector64 p;
            for (int i = 0; i < grid.Length; i++ )
            {
                val = grid[i];
                if ( grid.IsBlanked(val)|| !grid.IsValidV(val)) continue;
                if ( val < ops.Minimum || val > ops.Maximum ) continue;
                p = grid.GetGridCoord(i).toVector64();
                if ( !data.IsInRange(p.X, p.Y, p.Z) ) continue;

                //计算权重,不再加权
                if (val < ops.Minimum || val > ops.Maximum) continue;
                //val = (val - ops.Val1) / (ops.Val2 - ops.Val1);//0-1
                //val = val * ops.Weight; // 0 - 100

                double x1 = p.X - grid.xStep / 2;
                double x2 = p.X + grid.xStep / 2;
                double y1 = p.Y - grid.yStep / 2;
                double y2 = p.Y + grid.yStep / 2;
                double z1 = p.Z - grid.zStep / 2;
                double z2 = p.Z + grid.zStep / 2;

                for (double z = z1; z <= z2; z += data.zStep)
                {
                    for (double y = y1; y <= y2; y += data.yStep)
                    {
                        for (double x = x1; x <= x2; x += data.xStep)
                        {
                            Int32XYZ xyz = data.GetIndices(new Vector64(x,y,z));
                            if (!data.IsGridValid(xyz.x, xyz.y, xyz.z)) continue;
                            id = data.GetVerticIndex(xyz);
                            if ( doneGrids[id] ) continue;//本次已叠加
                            if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次已叠加
                            curGridBuffer[id] += (float)ops.Weight;
                            doneGrids[id] = true;
                        }
                    }
                }
            }//for (int i = 0; i < grid.Length; i++ )
        }

        /// <summary>
        /// 按地层属性赋予权重
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="ops"></param>
        /// <param name="data"></param>
        void DoGrid3DBufferOnStratumValue(C3DGridData grid,
                             OverlayPropertyPara ops,
                             C3DGridData data)
        {
            long id;
            double val;            
            Vector64 p;
            for (int i = 0; i < grid.Length; i++)
            {
                val = grid[i];
                if (grid.IsBlanked(val) || !grid.IsValidV(val)) continue;

                //地层序号从1开始，0无地层
                int ilayer = (int)grid[i] - 1;
                if (ilayer < 0 || ilayer >= C3DData.Stratums.Count) continue;

                StratumData s = C3DData.Stratums[ilayer];
                if (s.Name != ops.PropertyName) continue;
                
                p = grid.GetGridCoord(i).toVector64();
                double x1 = p.X - grid.xStep / 2;
                double x2 = p.X + grid.xStep / 2;
                double y1 = p.Y - grid.yStep / 2;
                double y2 = p.Y + grid.yStep / 2;
                double z1 = p.Z - grid.zStep / 2;
                double z2 = p.Z + grid.zStep / 2;
                
                for (double z = z1; z <= z2; z += data.zStep)
                {
                    for (double y = y1; y <= y2; y += data.yStep)
                    {
                        for (double x = x1; x <= x2; x += data.xStep)
                        {
                            Int32XYZ xyz = data.GetIndices(new Vector64(x, y, z));
                            if (!data.IsGridValid(xyz.x, xyz.y, xyz.z)) continue;
                            id = data.GetVerticIndex(xyz);
                            if (doneGrids[id]) continue; //本次已叠加
                            if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次已叠加
                            curGridBuffer[(int)id] += (float)ops.Weight;
                            doneGrids[id] = true;
                        }
                    }
                }
            }            
        }

        private void TypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PropertyComboBox.Items.Clear();
            PropertyComboBox.Text = string.Empty;
            PropertyComboBox.SelectedIndex = -1;
            
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            int sel1 = TypeComboBox.SelectedIndex;
            if (sel1 < 0) return;

            C3DObjectBase obj = Objects[sel];
            MineralContralMetaTypeEnum m = (MineralContralMetaTypeEnum)Enum.Parse(typeof(MineralContralMetaTypeEnum), 
                TypeComboBox.SelectedItem.ToString());
            if (obj.type == ShapeEnum.Grid3D )
            {
                if (m == MineralContralMetaTypeEnum.属性控矿3D)
                {
                    PropertyComboBox.Items.Add("Grid Value");                    
                }
                if (m == MineralContralMetaTypeEnum.地质岩性控矿3D)
                {
                    for (int i = 0; i < C3DData.Stratums.Count; i++)
                        PropertyComboBox.Items.Add(C3DData.Stratums[i].Name);
                }                 
            }
            else if (obj.type == ShapeEnum.Mesh)
            {
                if (m == MineralContralMetaTypeEnum.地质岩性控矿2D)
                {
                    List<string> names = C3DData.Stratums.toStratumNames();
                    for (int i = 0; i < names.Count; i++)
                         PropertyComboBox.Items.Add(names[i]);
                }
            }
            else if (obj.type == ShapeEnum.PolygonSlicer)
            {
                PolygonSlicer poly = (PolygonSlicer)obj;
                if (m == MineralContralMetaTypeEnum.地质岩性控矿2D || m == MineralContralMetaTypeEnum.距离控矿)
                {
                    List<string> names = C3DData.Stratums.toStratumNames();
                    if (names.Count < 1) names = poly.tracedGeoObjects.toObjectNames(true, false);
                    for (int i = 0; i < names.Count; i++)
                        PropertyComboBox.Items.Add(names[i]);
                }
                
                if (m == MineralContralMetaTypeEnum.断裂控矿2D)
                {
                    for (int i = 0; i < poly.tracedGeoObjects.Count; i++)
                    {
                        Polygon2D p = poly.tracedGeoObjects[i];
                        if ( !p.IsClosed ) 
                        {   
                            PropertyComboBox.Items.Add(p.Name + ":西向");
                            PropertyComboBox.Items.Add(p.Name + ":东向");
                            PropertyComboBox.Items.Add(p.Name + ":北向");
                            PropertyComboBox.Items.Add(p.Name + ":南向");
                        }
                    }
                }
                if (m == MineralContralMetaTypeEnum.断裂带控矿2D)
                {
                    List<int> lists = new List<int>();
                    for (int i = 0; i < poly.tracedGeoObjects.Count; i++) 
                    {
                        Polygon2D p = poly.tracedGeoObjects[i];
                        if ( !p.IsClosed )lists.Add(i);
                    }

                    for (int i = 0; i < lists.Count;i++)
                    { 
                        for (int j = i + 1; j < lists.Count; j++)
                        {
                            Polygon2D p1 = poly.tracedGeoObjects[i];
                            Polygon2D p2 = poly.tracedGeoObjects[j];
                            PropertyComboBox.Items.Add(p1.Name + " Between "+ p2.Name);
                        }
                    }
                    lists.Clear();
                }
            }
            else if (obj.type == ShapeEnum.Triangles)
            {
                TriangleObj poly = (TriangleObj)obj;
                if (m == MineralContralMetaTypeEnum.距离控矿)
                {
                    PropertyComboBox.Items.Add("高斯模型");
                }
            }
            else if (obj.type == ShapeEnum.GeoLayerMeshes)
            {
                GeoLayerMeshes mesh = (GeoLayerMeshes)obj;
                if (m == MineralContralMetaTypeEnum.矿体趋势分析)
                {
                    PropertyComboBox.Items.Add("趋势分析");
                }
            }
        }

        private void XYZStepChanged(object sender, EventArgs e)
        {
            try 
            {
                double x1 = double.Parse(textBoxX1.Text);
                double x2 = double.Parse(textBoxX2.Text);
                double y1 = double.Parse(textBoxY1.Text);
                double y2 = double.Parse(textBoxY2.Text);
                double z1 = double.Parse(textBoxZ1.Text);
                double z2 = double.Parse(textBoxZ2.Text);
                double dx = double.Parse(XStepTextBox.Text);
                double dy = double.Parse(YStepTextBox.Text);
                double dz = double.Parse(ZStepTextBox.Text);
                if (dx > 0)
                {
                    int nx = (int)((x2 - x1) / dx + 0.1) + 1;
                    textBoxNX.Text = nx.ToString();
                }
                if (dy > 0)
                {
                    int ny = (int)((y2 - y1) / dy + 0.1) + 1;
                    textBoxNY.Text = ny.ToString();
                }
                if (dz > 0)
                {
                    int nz = (int)((z2 - z1) / dz + 0.1) + 1;
                    textBoxNZ.Text = nz.ToString();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void XYZNumChanged(object sender, EventArgs e)
        {
            try 
            {
                double x1 = double.Parse(textBoxX1.Text);
                double x2 = double.Parse(textBoxX2.Text);
                double y1 = double.Parse(textBoxY1.Text);
                double y2 = double.Parse(textBoxY2.Text);
                double z1 = double.Parse(textBoxZ1.Text);
                double z2 = double.Parse(textBoxZ2.Text);
                int nx = int.Parse(textBoxNX.Text);
                int ny = int.Parse(textBoxNY.Text);
                int nz = int.Parse(textBoxNZ.Text);
                if (nx > 1)
                {
                    double dx = Math.Round((x2 - x1) / (nx - 1), 4);
                    XStepTextBox.Text = dx.ToString();
                }
                if (ny > 1)
                {
                    double dy = Math.Round((y2 - y1) / (ny - 1), 4);
                    YStepTextBox.Text = dy.ToString();
                }
                if (nz>1)
                {
                    double dz = Math.Round((z2 - z1) / (nz - 1), 4);
                    ZStepTextBox.Text = dz.ToString();
                }
            }
            catch(Exception ex) 
            {

            }            
        }

        private void XYZRangeChanged(object sender, EventArgs e)
        {
            try 
            {
                double x1 = double.Parse(textBoxX1.Text);
                double x2 = double.Parse(textBoxX2.Text);
                double y1 = double.Parse(textBoxY1.Text);
                double y2 = double.Parse(textBoxY2.Text);
                double z1 = double.Parse(textBoxZ1.Text);
                double z2 = double.Parse(textBoxZ2.Text);
                int nx = int.Parse(textBoxNX.Text);
                int ny = int.Parse(textBoxNY.Text);
                int nz = int.Parse(textBoxNZ.Text);
                if (nx > 1) 
                {
                    double dx = Math.Round((x2 - x1) / (nx - 1), 4);
                    XStepTextBox.Text = dx.ToString();
                }
                if (ny > 1)
                {
                    double dy = Math.Round((y2 - y1) / (ny - 1), 4);
                    YStepTextBox.Text = dy.ToString();
                }
                if(nz > 1) 
                {
                    double dz = Math.Round((z2 - z1) / (nz - 1), 4);
                    ZStepTextBox.Text = dz.ToString();
                }
            }
            catch(Exception ex ) 
            {

            }
        }

        private void PropertiesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePropertySelected();
        }

        void DoPolygonSlicerBuffer(C3DObjectBase obj, OverlayPropertyPara ops, C3DGridData data)
        {
            PolygonSlicer slicer = obj as PolygonSlicer;
            MineralContralMetaTypeEnum m = ops.GetControlType();
            if ( m == MineralContralMetaTypeEnum.地质岩性控矿2D )
            {
                if ( slicer.BackgroundImage != null && 
                     C3DData.Stratums.Count > 0 &&
                     slicer.axis == AxisEnum.yAxis ) //XOY平面
                     DoPolygonSlicerBufferByImageScan(slicer, ops, data);
                else DoPolygonSlicerBufferByPolygons(slicer, ops, data);
            }
            if (m == MineralContralMetaTypeEnum.距离控矿)
            {   
                DoPolygonSlicerBufferByDistance(slicer, ops, data);
            }
            else if( m == MineralContralMetaTypeEnum.断裂控矿2D)
            {
                DoPolygonSlicerBufferOnFault2D(slicer,ops,data);//断裂控矿
            }
            else if (m == MineralContralMetaTypeEnum.断裂带控矿2D)
            {
                DoPolygonSlicerBufferOnFaults2D(slicer, ops, data);
            }
        }

        void DoPolygonSlicerBufferByPolygons(PolygonSlicer slicer, OverlayPropertyPara ops, C3DGridData data)
        {            
            float val;
            Vector64 p,p1,p2;
            int iz,id,nx,ny;
            double x, y, xx, yy;
            
            for (int i = 0; i < slicer.tracedGeoObjects.Count; i++)
            {
                Polygon2D poly = slicer.tracedGeoObjects[i].Copy();
                if ( !poly.IsClosed ) continue;
                if( poly.Name == ops.PropertyName)
                {
                    val = (float)ops.Weight; // 0 - 100
                    poly.UpdateRange();
                    
                    p1 = slicer.toTracedPoint(new Vector64(minx, miny, 0));
                    p2 = slicer.toTracedPoint(new Vector64(maxx, maxy, 0));
                    nx = (int)(Math.Abs(p2.X - p1.X) / data.xStep);
                    ny = (int)(Math.Abs(p2.Y - p1.Y) / data.yStep);
                    xx = (poly.maxx - poly.minx) / nx;
                    yy = (poly.maxy - poly.miny) / ny;

                    for (y = poly.miny; y <= poly.maxy; y += yy)
                    {
                        for (x = poly.minx; x <= poly.maxx; x += xx)
                        {
                            if ( !poly.IsPointInsidePoly(x, y) ) continue;
                            p = slicer.toTracedPoint(new Vector64(x, y, 0, 0));                            
                            Int32XYZ xyz = data.GetIndices(p);
                            if ( !data.IsGridValid(xyz.x, xyz.y, 0)) continue;

                            for(iz = 0; iz < data.zNum; iz++)
                            {
                                id = (int)data.GetVerticIndex(xyz.x, xyz.y, iz);
                                if (doneGrids[id]) continue; //本次已叠加
                                if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次已叠加
                                curGridBuffer[id] += val;
                                doneGrids[id] = true;
                            }
                        }
                    }
                }//if( poly.Name == ops.PropertyName)               
            }            
        }
        void DoPolygonSlicerBufferByDistance(PolygonSlicer slicer, OverlayPropertyPara ops, C3DGridData data)
        {
            for (int i = 0; i < slicer.tracedGeoObjects.Count; i++)
            {
                Polygon2D poly = slicer.tracedGeoObjects[i];
                if (!poly.IsClosed) continue;
                if (poly.Name == ops.PropertyName)
                {
                    DoPolygonSlicerBufferOn高斯模型(slicer,poly,ops, data);
                }             
            }
        }
        void DoPolygonSlicerBufferOn高斯模型(PolygonSlicer slicer, Polygon2D poly, OverlayPropertyPara ops, C3DGridData data)
        {            
            double dist, weight, thres = 1e-6;
            double thresholdDistance = ((OverlayPropertyParaDistanceGaosi)ops).GetThresholdDistance(thres);
            Polygon2D poly1 = new Polygon2D();//转换成世界坐标
            for (int i = 0; i < poly.Count; i++)
            {
                poly1.Add(slicer.toTracedPoint(poly[i]));
            }
            poly1.UpdateRange();
            double x1 = poly1.minx - ops.Maximum;
            double x2 = poly1.maxx + ops.Maximum;
            double y1 = poly1.miny - ops.Maximum;
            double y2 = poly1.maxy + ops.Maximum;
            x1 = Math.Max(x1, data.minx);
            x2 = Math.Min(x2, data.maxx);
            y1 = Math.Max(y1, data.miny);
            y2 = Math.Min(y2, data.maxy);

            List<Vector32>tasts = new List<Vector32>();
            Int32XYZ xyz1 = data.GetVerticIndexByPosition(x1, y1, 0);
            Int32XYZ xyz2 = data.GetVerticIndexByPosition(x2, y2, 0);

            for (int iy = xyz1.y; iy < xyz2.y; iy++)
            {
                for (int ix = xyz1.x; ix < xyz2.x; ix++)
                {
                    var id = data.GetVerticIndex(ix, iy, 0);
                    if (doneGrids[id]) continue;//本次已叠加
                    if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次已叠加
                    var p = data.GetGridCoord(ix, iy, 0);
                    //放入任务列表
                    p.V = id;   //缓存到多线程任务列表中
                    tasts.Add(p);                    
                }
            }
            Parallel.For(0, tasts.Count, i =>
            {
                Vector32 p = tasts[i];
                Int32XYZ xyz = data.GetVerticIndexByPosition(p.X,p.Y,0);                
                dist = poly1.GetNearestDistance(p.toVector64());                
                if( ops.Maximum > ops.Minimum && dist < ops.Maximum ||
                    ops.Maximum <= ops.Minimum ) //有效距离
                {
                    weight = ops.GetWeight(dist) * ops.Weight;                    
                    for (int iz = 0; iz < data.zNum; iz++)
                    {
                        int id = (int)data.GetVerticIndex(xyz.x, xyz.y, iz);                        
                        if(doneGrids[id]) continue;
                        curGridBuffer[id] += (float)weight;
                        if(curGridBuffer[id] > ops.MaximumWeight) curGridBuffer[id] = (float)ops.MaximumWeight;
                        doneGrids[id] = true;
                    }
                }
            });
            poly1.Clear();
            tasts.Clear();
        }

            /// <summary>
            /// 地层扫描
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="ops"></param>
            /// <param name="data"></param>
        void DoPolygonSlicerBufferByImageScan(PolygonSlicer slicer,
                                    OverlayPropertyPara ops, 
                                    C3DGridData data )
        {
            int nx = data.xNum - 1;//网格节点数-1
            int ny = data.yNum - 1;//网格节点数-1            
            int[,] layers = null;
            if (slicer.axis == AxisEnum.yAxis) //XOY平面
            {
                layers = C3DData.Stratums.SamplingFromImageByColor(slicer.BackgroundImage, nx, ny,
                SamplingMethodEnum.Squared);
            }
            if (slicer.axis == AxisEnum.zAxis)//垂直与XOY平面
            {
                if( slicer.Maxx - slicer.Minx > slicer.Maxy - slicer.Miny )
                    nx = data.xNum - 1;
                else nx = data.yNum - 1;
                ny = data.zNum - 1;
                layers = C3DData.Stratums.SamplingFromImageByColor(slicer.BackgroundImage, nx, ny,
                SamplingMethodEnum.Squared);
            }
            if (layers == null) return;

            int iz1 = (int)((ops.Minimum - data.minz) / data.zStep + 0.1);
            int iz2 = (int)((ops.Maximum - data.minz) / data.zStep + 0.1);
            iz1 = data.GeometryLimited(iz1, AxisEnum.zAxis);
            iz2 = data.GeometryLimited(iz2, AxisEnum.zAxis);

            int id;
            double x, y;
            //nx,ny网格单元数目,dx,dy单元间距
            double dx = (slicer.maxx - slicer.minx) / nx;
            double dy = (slicer.maxy - slicer.miny) / ny;            
            
            for (int iy = 0; iy < ny; iy++)
            {
                for (int ix = 0; ix < nx; ix++)
                {
                    int ilayer = layers[ix, iy];
                    if( ilayer < 0 ) continue;
                    StratumData layer = C3DData.Stratums[ilayer];
                    if ( ops.PropertyName == layer.Name )
                    {
                        x = slicer.minx + ix * dx + dx / 2;
                        y = slicer.miny + iy * dy + dy / 2;
                        Vector64 p = slicer.toTracedPoint(new Vector64(x, y, 0));
                        Int32XYZ xyz = data.GetVerticIndexByPosition(p.X, p.Y, p.Z);
                        if (!data.IsGridValid(xyz.x, xyz.y, 0)) continue;
                        for (int iz = iz1; iz < iz2; iz++)
                        {
                            id = (int)data.GetVerticIndex(xyz.x, xyz.y, iz);
                            if (doneGrids[id]) continue; //本次已叠加
                            if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次已叠加
                            curGridBuffer[id] += (float)ops.Weight;
                            doneGrids[id] = true;
                        }
                    }
                } 
            }
            
           // br.Close();
        }
       
        void DoPolygonSlicerBufferOnFault2D(PolygonSlicer slicer, OverlayPropertyPara ops, C3DGridData data)
        {                  
            int ix1, ix2, iy1, iy2, dd;           

            for (int i = 0; i < slicer.tracedGeoObjects.Count; i++)
            {
                Polygon2D line = slicer.tracedGeoObjects[i].Copy();
                if (line.IsClosed ) continue;
                if (ops.PropertyName == line.Name + ":东向")
                {
                    List<Int32XYZ> grids = CreateGridsFromSlicerLine(slicer, line, data);
                    foreach (Int32XYZ xyz in grids)
                    {
                        dd = (int)((ops.Maximum - ops.Minimum) / data.xStep);
                        ix2 = data.GeometryLimited(xyz.x + dd, AxisEnum.xAxis);                        
                        Do2DGridOverlayOnX(xyz.x, ix2, xyz.y, ops, data);
                    }                    
                    grids.Clear();                   
                }
                if (ops.PropertyName == line.Name + ":西向")
                {
                    List<Int32XYZ> grids = CreateGridsFromSlicerLine(slicer, line, data);
                    foreach (Int32XYZ xyz in grids)
                    {
                        dd = (int)((ops.Maximum - ops.Minimum) / data.xStep);
                        ix1 = data.GeometryLimited(xyz.x - dd, AxisEnum.xAxis);
                        Do2DGridOverlayOnX(ix1, xyz.x, xyz.y, ops, data);
                    }                    
                    grids.Clear();                    
                }
                if (ops.PropertyName == line.Name + ":南向")
                {
                    List<Int32XYZ> grids = CreateGridsFromSlicerLine(slicer, line, data);
                    foreach (Int32XYZ xyz in grids)
                    {
                        dd = (int)((ops.Maximum - ops.Minimum) / data.yStep);
                        iy1 = data.GeometryLimited(xyz.y - dd, AxisEnum.yAxis);
                        Do2DGridOverlayOnY(iy1, xyz.y, xyz.x, ops, data);
                    }                    
                    grids.Clear();                  
                }
                if (ops.PropertyName == line.Name + ":北向")
                {
                    List<Int32XYZ> grids = CreateGridsFromSlicerLine(slicer, line, data);
                    foreach (Int32XYZ xyz in grids)
                    {
                        dd = (int)((ops.Maximum - ops.Minimum) / data.yStep);
                        iy2 = data.GeometryLimited(xyz.y + dd, AxisEnum.yAxis);
                        Do2DGridOverlayOnY(xyz.y, iy2, xyz.x, ops, data);
                    }                    
                    grids.Clear();                   
                }
            }//for (int i = 0; i < slicer.tracedGeoObjects.Count; i++)
        }
        
        void DoTriangleObj3DBuffer(C3DObjectBase obj, OverlayPropertyPara ops, C3DGridData data)
        {
            TriangleObj tri = obj as TriangleObj;
            MineralContralMetaTypeEnum m = ops.GetControlType();
            if (m == MineralContralMetaTypeEnum.距离控矿 && ops.PropertyName == "高斯模型")
            {
                DoTriangleObjBufferOn高斯模型(tri, ops, data);
            }            
        }
        double GetNearestDistanceFrom(Vector32 p0, float[]point)
        {
            double x = p0.X - point[0];
            double y = p0.Y - point[1];
            double z = p0.Z - point[2];
            return Math.Sqrt(x*x+y*y+z*z);
        }
        /// <summary>
        /// 高斯模型距离场控矿
        /// </summary>
        /// <param name="tri">矿体三角网模型</param>
        /// <param name="ops"></param>
        /// <param name="data"></param>
        void DoTriangleObjBufferOn高斯模型(TriangleObj tri, OverlayPropertyPara ops, C3DGridData data)
        {
            //var kdtree = new KdTree<float, int>(3, new FloatMath());
            //for(int i=0;i<tri.points.Count;i++)
            //{
            //    kdtree.Add(tri.points[i].toArray(3),i);
            //}            
            List<Vector32>tasts = new List<Vector32>();
            double dist,weight,thres = 1e-6;
            double thresholdDistance = ((OverlayPropertyParaDistanceGaosi)ops).GetThresholdDistance(thres);
            for (int iz = 0;iz<data.zNum; iz++)
            {
                for (int iy = 0; iy < data.yNum; iy++)
                {
                    for (int ix = 0; ix < data.xNum; ix++)
                    {
                        var id = data.GetVerticIndex(ix, iy, iz);
                        if (doneGrids[id]) continue;//本次已叠加
                        if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次已叠加

                        var p = data.GetGridCoord(ix, iy, iz);
                        dist = tri.GetNearestDistanceWrapped(p.toVector64());
                        if (ops.Maximum > ops.Minimum && dist > ops.Maximum) continue;

                        if (dist < thresholdDistance)
                        {
                            p.V = id;   //缓存到多线程任务列表中
                            tasts.Add(p);
                        }
                        else
                        {
                            weight = thres * ops.Weight;
                            curGridBuffer[id] += (float)weight;
                            doneGrids[id] = true;
                        }
                    }
                }
            }           
          
            //kdtree.Clear();
        }
        /// <summary>
        /// 地层模型控矿-模型之间
        /// </summary>
        /// <param name="meshes">地层模型（2层）</param>
        /// <param name="ops"></param>
        /// <param name="data"></param>
        void DoGeoLayerMesh3DBuffer(C3DObjectBase obj, OverlayPropertyPara ops, C3DGridData data)
        {
            GeoLayerMeshes meshes = obj as GeoLayerMeshes; 
            for (int iz = 0; iz < data.zNum; iz++)
            {
                for (int iy = 0; iy < data.yNum; iy++)
                {
                    for (int ix = 0; ix < data.xNum; ix++)
                    {
                        var id = data.GetVerticIndex(ix, iy, iz);                        
                        if (doneGrids[id]) continue;//本次已叠加                        
                        if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次已叠加
                        var p = data.GetGridCoord(ix, iy, iz);
                        if( meshes.IsPointOnMeshes(p.toVector64()) )curGridBuffer[id] += (float)ops.Weight;
                        doneGrids[id] = true;
                    }
                }
            }
        }

        /// <summary>
        /// 是否横向线段，西-东
        /// </summary>
        /// <param name="slicer"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        bool IsHorizontalLine(PolygonSlicer slicer, Polygon2D line)
        {
            double x1 = line.minx;
            double y1 = line.miny;
            double x2 = line.maxx;
            double y2 = line.maxy;
            Vector64 p1 = slicer.toTracedPoint(new Vector64(x1, y1, 0));
            Vector64 p2 = slicer.toTracedPoint(new Vector64(x2, y2, 0));
            if (Math.Abs(p1.X - p2.X) > Math.Abs(p1.Y - p2.Y))
                return true;
            else return false;
        }

        void GetMinMaxXYZRange(List<Int32XYZ>list1, List<Int32XYZ> list2,
            ref int ix1, ref int ix2,
            ref int iy1, ref int iy2 )
        {
            ix1 = iy1 = 10000000;
            ix2 = iy2 = -100;
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].x < ix1) ix1 = list1[i].x;
                if (list1[i].x > ix2) ix2 = list1[i].x;
                if (list1[i].y < iy1) iy1 = list1[i].y;
                if (list1[i].y > iy2) iy2 = list1[i].y;
            }
            for (int i = 0; i < list2.Count; i++)
            {
                if (list2[i].x < ix1) ix1 = list2[i].x;
                if (list2[i].x > ix2) ix2 = list2[i].x;
                if (list2[i].y < iy1) iy1 = list2[i].y;
                if (list2[i].y > iy2) iy2 = list2[i].y;
            }
        }
        List<Int32XYZ> TrimToSameXSize(List<Int32XYZ> lists, int ix1, int ix2)
        {
            int n = lists.Count;
            if (n < 1) return lists;
            List<Int32XYZ> list1 = new List<Int32XYZ>();
            Int32XYZ p0 = lists[0];

            if (p0.x > ix1)
            {
                for (int ix = ix1; ix < p0.x; ix++)
                    list1.Add(new Int32XYZ(ix, p0.y, p0.z));
            }

            for (int i = 0; i < lists.Count; i++) list1.Add(lists[i]);
            p0 = lists[n - 1];
            if (p0.x < ix2)
            {
                for (int ix = p0.x + 1; ix <= ix2; ix++)
                    list1.Add(new Int32XYZ(ix, p0.y, p0.z));
            }
            lists.Clear();
            return list1;
        }

        List<Int32XYZ> TrimToSameYSize(List<Int32XYZ>lists,int iy1,int iy2)
        {            
            int n = lists.Count;
            if ( n < 1 ) return lists;            
            List<Int32XYZ> list1 = new List<Int32XYZ>();
            Int32XYZ p0 = lists[0];
            
            if( p0.y > iy1 )
            {
                for (int iy = iy1; iy < p0.y; iy++) 
                    list1.Add(new Int32XYZ(p0.x, iy, p0.z));
            }
            for (int i = 0; i < lists.Count; i++) list1.Add(lists[i]);
            p0 = lists[n - 1];
            if(p0.y < iy2)
            {
                for (int iy = p0.y+1; iy <= iy2; iy++)
                    list1.Add(new Int32XYZ(p0.x, iy, p0.z));
            }
            lists.Clear();
            return list1;
        }

        //断裂带控矿
        void DoPolygonSlicerBufferOnFaults2D(PolygonSlicer slicer, OverlayPropertyPara ops, C3DGridData data)
        {   
            string property = ops.PropertyName;
            if (!property.Contains("Between")) return;
            property = property.Replace("Between", ",");
            string[] ss = property.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length < 2) return;
            string name1 = ss[0];
            string name2 = ss[1];
            ss = null;
            Polygon2D line1 = null, line2 = null;
            for (int i = 0; i < slicer.tracedGeoObjects.Count; i++)
            {
                Polygon2D line = slicer.tracedGeoObjects[i];
                if (line.IsClosed) continue;
                if (line.Name == name1) line1 = line;
                else if (line.Name == name2) line2 = line;
                if (line1 != null && line2 != null) break;
            }

            if (line1 == null || line2 == null) return;

            bool horizontal1 = IsHorizontalLine(slicer, line1);
            bool horizontal2 = IsHorizontalLine(slicer, line2);

            List<Int32XYZ> grids1 = CreateGridsFromSlicerLine(slicer, line1, data);
            List<Int32XYZ> grids2 = CreateGridsFromSlicerLine(slicer, line2, data);
            
            if( horizontal1 ) grids1.Sort((a, b) => { return a.x.CompareTo(b.x); });
            else grids1.Sort((a, b) => { return a.y.CompareTo(b.y); });
            if (horizontal2) grids2.Sort((a, b) => { return a.x.CompareTo(b.x); });
            else grids2.Sort((a, b) => { return a.y.CompareTo(b.y); });
          
            int ix1 = 0, ix2 = 0, id,iy1 = 0, iy2 = 0;
            GetMinMaxXYZRange(grids1, grids2, ref ix1, ref ix2, ref iy1, ref iy2);
            List<Point> points = new List<Point>();
            for (int i = 0; i < grids1.Count; i++) 
                points.Add(new Point(grids1[i].x, grids1[i].y));
            for (int i = grids2.Count-1; i >= 0; i--)
                points.Add(new Point(grids2[i].x, grids2[i].y));

            MyRegion rgn = new MyRegion(points);            

            if ( horizontal1 ) //横向
            {
                for (int ix = ix1; ix <= ix2; ix  ++ )
                {                  
                    for (int iy = iy1 + 1; iy < iy2; iy++)
                    {
                        if (!rgn.IsPointInRgn(ix, iy)) continue;
                        for (int iz = 0; iz < data.zNum; iz++)
                        {
                            id = (int)data.GetVerticIndex(ix, iy, iz);
                            if (doneGrids[id]) continue;//本次已叠加
                            if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次次已叠加
                            curGridBuffer[id] += (float)ops.Weight;
                            doneGrids[id] = true;
                        }
                    }
                }
            }
            else
            {
                for (int iy = iy1; iy <= iy2; iy++)
                {  
                    for (int ix = ix1 + 1; ix < ix2; ix++)
                    {
                        if (!rgn.IsPointInRgn(ix, iy)) continue;
                        for (int iz = 0; iz < data.zNum; iz++)
                        {
                            id = (int)data.GetVerticIndex(ix, iy, iz);                            
                            if (doneGrids[id]) continue;//本次已叠加
                            if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0) continue;//上次次已叠加
                            curGridBuffer[id] += (float)ops.Weight;
                            doneGrids[id] = true;
                        }
                    }
                }// for (int iy = iy1; iy <= iy2; iy++)
            }//else
            rgn.Clear();
        }
        void Do2DGridOverlayOnX(int ix1, int ix2, int iy, OverlayPropertyPara ops , C3DGridData data)
        {
            int id;
            for (int ix = ix1; ix < ix2; ix++)
            {
                for (int iz = 0; iz < data.zNum; iz++)
                {
                    id = (int)data.GetVerticIndex(ix, iy, iz);
                    if (doneGrids[id]) continue;//本次已叠加
                    if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight > 0 ) continue;//上次次已叠加
                    curGridBuffer[id] += (float)ops.Weight;
                    doneGrids[id] = true;
                }                
            }
        }

        private void Loadbutton_Click(object sender, EventArgs e)
        {            
            var dlg = new OpenFileDialog();
            dlg.Filter = Resource1.AnalyzeParametersStructerFilter;
            dlg.Filter += "|" + "All Files(*.*)|*.*";            
            if (dlg.ShowDialog() != DialogResult.OK) return;
            Properties.Clear();
            BinaryReader br = new BinaryReader(new FileStream(dlg.FileName, FileMode.Open));
            int header = 20241212;
            if( br.ReadInt32() != header )
            {
                MessageBox.Show("not a correct parameter file format.");
                br.Close();
                return;
            }
            Properties.Clear();
            int n = br.ReadInt32();
            string err = "";
            int count = 1;
            for (int i = 0; i < n; i++)
            {
                OverlayPropertyPara s = new OverlayPropertyPara();
                OverlayTypeEnum type = (OverlayTypeEnum)br.ReadInt32();
                if (type == OverlayTypeEnum.DistanceGaosi) s = new OverlayPropertyParaDistanceGaosi();
                else if (type == OverlayTypeEnum.LayerProperty2D) s = new OverlayPropertyPara岩性控矿2D();
                else if (type == OverlayTypeEnum.TrendAnalysis) s = new OverlayPropertyParaTrendAnalysis();
                else if (type == OverlayTypeEnum.Property3DGrid) s = new OverlayPropertyPara3DGrid();
                else if (type == OverlayTypeEnum.Fault2D) s = new OverlayPropertyPara断裂控矿2D();
                if ( s.Read(br) ) 
                {
                    if(s.IsValid(Objects)) Properties.Add(s);
                    else 
                    {
                        err += count + "==>" + s.toString() + Environment.NewLine + s.errMessage + Environment.NewLine;                        
                        count++;
                    }
                }
                else
                {
                    MessageBox.Show("读取数据错误！" + Environment.NewLine + s.errMessage);
                    br.Close();
                    return;
                }
            }
            br.Close();
            if( err.Length > 0 ) MessageBox.Show("以下参数错误：" + Environment.NewLine + err);

            UpdateListBox2();
        }

        private void listBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = null;
            int sel3 = listBox2.SelectedIndex;
            if (sel3 < 0 || sel3 >= Properties.Count) return;
            propertyGrid1.SelectedObject = Properties[sel3];
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            int sel3 = listBox2.SelectedIndex;
            if (sel3 < 0 || sel3 >= Properties.Count) return;
            OverlayPropertyPara obj = (OverlayPropertyPara)propertyGrid1.SelectedObject;
            Properties[sel3] = obj;
            UpdateListBox2();
        }

        void Do2DGridOverlayOnY(int iy1, int iy2, int ix, OverlayPropertyPara ops, C3DGridData data)
        {
            int id;
            for (int iy = iy1; iy < iy2; iy++)
            {
                for (int iz = 0; iz < data.zNum; iz++)
                {
                    id = (int)data.GetVerticIndex(ix, iy, iz);
                    if (doneGrids[id]) continue;//本次已叠加
                    if (curGridBuffer[id] >= ops.MaximumWeight && ops.Weight>0) continue;//上次次已叠加
                    curGridBuffer[id] += (float)ops.Weight;
                    doneGrids[id] = true;
                }               
            }
        }

        private void PropertTMore_Click(object sender, EventArgs e)
        {

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (Properties.Count < 1) return;
            var dlg = new SaveFileDialog();
            dlg.Filter = Resource1.AnalyzeParametersStructerFilter;
            dlg.Filter += "|" + "All Files(*.*)|*.*";
            dlg.OverwritePrompt = true;
            if (dlg.ShowDialog() != DialogResult.OK) return;
            BinaryWriter bw = new BinaryWriter(new FileStream(dlg.FileName, FileMode.Create));
            int header = 20241212;
            bw.Write(header);
            bw.Write(Properties.Count);
            for(int i=0;i< Properties.Count;i++)
            {
                OverlayPropertyPara s = Properties[i];
                bw.Write((int)s.Type);
                if( !s.Write(bw)) 
                {
                    MessageBox.Show("保存数据错误！" + s.errMessage);
                    bw.Close();
                    return;
                }
            }
            bw.Close();
        }

        List<Int32XYZ> RemoveDuplicatedGrid(List<Int32XYZ>grids)
        {
            if (grids.Count < 1) return grids;
            List<Int32XYZ> grids1 = new List<Int32XYZ>();
            Int32XYZ p1 = grids[0], p;
            grids1.Add(p1);
            for(int i=1; i < grids.Count;i++)
            {
                p = grids[i];
                if (p.x == p1.x && p.y == p1.y) continue;
                grids1.Add(p);
                p1 = p;
            }
            grids.Clear();
            return grids1;
        }

        List<Int32XYZ>CreateGridsFromSlicerLine(PolygonSlicer slicer,Polygon2D line,C3DGridData data)
        {
            List<Int32XYZ> grids = new List<Int32XYZ>();
            if (line.points.Count < 2) return grids;
            Vector64 p1,p2,p = new Vector64();
            double x, y, z;
            for (int i = 1;i < line.points.Count-1; i++ ) 
            {
                p1 = slicer.toTracedPoint(line.points[i]);
                p2 = slicer.toTracedPoint(line.points[i+1]);

                if (p1.X < data.minx && p1.X > data.maxx &&
                    p2.X < data.minx && p2.X > data.maxx  ) continue;

                if( Math.Abs(p2.X-p1.X) > Math.Abs(p2.Y - p1.Y) )
                {
                    for( x = p1.X; x < p2.X; x+= data.xStep)
                    {
                        p.Y = p1.Y + (p2.Y - p1.Y) * (x - p1.X) / (p2.X - p1.X);                                                
                        grids.Add(data.GetVerticIndexByPosition(x, p.Y, 0));
                    }                    
                }
                else if (Math.Abs(p2.Y - p1.Y) > Math.Abs(p2.X - p1.X))
                {
                    for (y = p1.Y; y < p2.Y; y += data.yStep)
                    {
                        p.X = p1.X + (p2.X - p1.X) * (y - p1.Y) / (p2.Y - p1.Y);
                        grids.Add(data.GetVerticIndexByPosition(p.X, y, 0));
                    }                    
                }
                grids.Add(data.GetVerticIndexByPosition(p2.X, p2.Y, 0));
            }            
            return RemoveDuplicatedGrid(grids);
        }

        void OverlayCurrentGridBuff(C3DGridData data)
        {
            for(int i = 0; i < data.Length; i++) 
            {
                data[i] += curGridBuffer[i];
            }
        }
        bool DoBufferCreate()
        {
            if (Properties.Count < 1) return false;

            try
            {
                minx = double.Parse(textBoxX1.Text);
                miny = double.Parse(textBoxY1.Text);
                minz = double.Parse(textBoxZ1.Text);
                maxx = double.Parse(textBoxX2.Text);
                maxy = double.Parse(textBoxY2.Text);
                maxz = double.Parse(textBoxZ2.Text);
                int nx = int.Parse(textBoxNX.Text);
                int ny = int.Parse(textBoxNY.Text);
                int nz = int.Parse(textBoxNZ.Text);
                if (newAnalysing && data != null) data.Clear();
                if (data == null) data = new C3DGridData(nx, ny, nz, 0f);
                data.Name = "叠加缓冲区分析";
                data.ResetDataRange(minx, maxx, miny, maxy, minz, maxz, 0, 100);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
                return false;
            }           
            
            
            List<OverlayPropertyPara> Properties1 = new List<OverlayPropertyPara>(Properties);
            Properties1.Sort((a, b) => { return a.ObjectSel.CompareTo(b.ObjectSel); });
            
            lastProperty = new OverlayPropertyPara();
            for ( int i = 0; i < Properties1.Count; i++ )
            {
                ResetDoneGrids(data.Length);

                OverlayPropertyPara ops = Properties1[i];
                C3DObjectBase obj = Objects[ops.ObjectSel];
                if( !ops.IsValid(Objects) )
                {
                    MessageBox.Show(ops.toString() + Environment.NewLine + ops.errMessage, "参数无效！");
                    return false;
                }
                if ( !IsPropertyDone(ops) )
                {
                    if (i > 0) OverlayCurrentGridBuff(data);//叠加上一次的数据
                    ResetCurGridBuffer(data.Length);
                }
                if (obj.type == ShapeEnum.Grid3D) DoGrid3DBuffer(obj, ops, data);
                if (obj.type == ShapeEnum.PolygonSlicer) DoPolygonSlicerBuffer(obj, ops, data);
                if (obj.type == ShapeEnum.Triangles) DoTriangleObj3DBuffer(obj, ops, data);
                if (obj.type == ShapeEnum.GeoLayerMeshes) DoGeoLayerMesh3DBuffer(obj, ops, data);
                
                //if (obj.type == ShapeEnum.Mesh) DoMeshBuffer(obj, ops, data);
                lastProperty = ops;
            }
            
            OverlayCurrentGridBuff(data);//叠加最后一次的数据

            doneGrids = null;
            curGridBuffer = null;
            Properties1.Clear();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < 0) data[i] = 0;
                if (data[i] > 100) data[i] = 100;
            }
            return true;
        }        

        private void DoAnalyze_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            bool ret = DoBufferCreate();
            Cursor = Cursors.Default;
            if (ret && data != null && data.maxv > data.minv ) 
            {
                MessageBox.Show("analyzed buffer created.");
            }            
        }
    }

    #region 相关属性支持的类
    public enum OverlayTypeEnum 
    {
        Base = 0,
        Property3DGrid = 1,
        TrendAnalysis = 2,
        LayerProperty2D = 3,
        Fault2D = 4,
        DistanceGaosi = 5,
    }
    public class OverlayPropertyPara
    {
        public OverlayTypeEnum Type = OverlayTypeEnum.Base;
        public int TypeSel = -1;//类型
        public string TypeName = "";
        public int ObjectSel = -1;//对象
        public string ObjectName = "";
        public int PropertySel = -1;//属性
        public string PropertyName = "";
        public int Count = 0;   //暂时不用
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最小属性值")]
        public virtual double Minimum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大属性值")]
        public virtual double Maximum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("权重系数(%)")]
        public virtual double Weight { get; set; } = 5;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大权重(%)")]
        public virtual double MaximumWeight { get; set; } = 100;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("是否反选？")]
        public virtual bool Inverse { get; set; } = false;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("属性信息"), ReadOnly(true)]
        public virtual string Info { get; set; } = "";
        public string errMessage = "";
        public virtual bool Save(BinaryWriter wr)
        {
            try 
            {
                wr.Write((int)Type);
                wr.Write(TypeSel);//类型
                C3DData.SaveString(wr, TypeName);
                wr.Write(ObjectSel);//对象
                C3DData.SaveString(wr, ObjectName);
                wr.Write(PropertySel);//属性
                C3DData.SaveString(wr, PropertyName);
                wr.Write(Minimum);
                wr.Write(Maximum);
                wr.Write(Weight);
                wr.Write(MaximumWeight);
                wr.Write(Inverse);
                C3DData.SaveString(wr, Info);
                return true;
            }
            catch(Exception ex) 
            {                
                errMessage = ex.Message;
                return false;
            }            
        }
        public virtual bool Load(BinaryReader br)
        {
            try
            {
                Type = (OverlayTypeEnum)br.ReadInt32();
                TypeSel = br.ReadInt32();
                TypeName = C3DData.LoadString(br);
                ObjectSel = br.ReadInt32();//对象
                ObjectName = C3DData.LoadString(br);
                PropertySel = br.ReadInt32(); //属性
                PropertyName = C3DData.LoadString(br);
                Minimum = br.ReadDouble();
                Maximum = br.ReadDouble();
                Weight = br.ReadDouble();
                MaximumWeight = br.ReadDouble();
                Inverse = br.ReadBoolean();
                Info = C3DData.LoadString(br);
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        public virtual string toString()
        {
            string ss = TypeName + "|";
            ss += ObjectName + "|";
            ss += PropertyName + "|";
            ss += Minimum.ToString() + " to ";
            ss += Maximum.ToString() + "|";
            ss += "Weight = " + Weight;
            if (Inverse) ss += "| Inversed";
            return ss;
        }
        public virtual bool IsValid(List<C3DObjectBase>objects)
        {
            if (TypeSel < 0 || ObjectSel < 0 || PropertySel < 0) 
            {
                errMessage += " | 目标对象不存在！";
                return false; 
            }
            if (Minimum > Maximum) 
            {
                errMessage += " | 最小值最大值无效！";
                return false; 
            }
            if (TypeName.Length < 1 ||
                ObjectName.Length < 1 ||
                PropertyName.Length < 1) 
            {
                errMessage += " | 名称无效：" + "目标对象=" + ObjectName + ";类型=" + TypeName + ";属性=" + PropertyName;
                return false; 
            }

            if (ObjectSel >= objects.Count) 
            {
                errMessage += " | 对象不存在;";
                return false; 
            }
            C3DObjectBase obj = objects[ObjectSel];
            if (Type == OverlayTypeEnum.DistanceGaosi &&
                 (obj.type != ShapeEnum.Triangles && obj.type != ShapeEnum.PolygonSlicer))
            {
                errMessage += " | 对象类型和参数类型不匹配;";
                return false;
            }
            else if (Type == OverlayTypeEnum.Property3DGrid && obj.type != ShapeEnum.Grid3D)
            {
                errMessage += " | 对象类型和参数类型不匹配;";
                return false;
            }
            else if (Type == OverlayTypeEnum.TrendAnalysis && obj.type != ShapeEnum.GeoLayerMeshes)
            {
                errMessage += " | 对象类型和参数类型不匹配;";
                return false;
            }
            else if (Type == OverlayTypeEnum.LayerProperty2D &&
                    (obj.type != ShapeEnum.PolygonSlicer &&
                     obj.type != ShapeEnum.Mesh &&
                     obj.type != ShapeEnum.GeoMesh &&
                     obj.type != ShapeEnum.Slicer))
            {
                errMessage += " | 对象类型和参数类型不匹配;";
                return false;
            }
            else if (Type == OverlayTypeEnum.Fault2D &&
                obj.type != ShapeEnum.PolygonSlicer)
            {
                errMessage += " | 对象类型和参数类型不匹配;";
                return false; 
            }

            return true;
        }
        public virtual MineralContralMetaTypeEnum GetControlType()
        {
            return (MineralContralMetaTypeEnum)Enum.Parse(typeof(MineralContralMetaTypeEnum),
                TypeName);
        }
        public virtual double GetWeight(double val)
        {
            return 0;
        }
        public virtual bool Write(BinaryWriter bw)
        {
            bw.Write(TypeSel);
            C3DData.SaveString(bw, TypeName);
            bw.Write(ObjectSel);
            C3DData.SaveString(bw, ObjectName);
            bw.Write(PropertySel);
            C3DData.SaveString(bw, PropertyName);
            bw.Write(Minimum);
            bw.Write(Maximum);
            bw.Write(Weight);
            bw.Write(MaximumWeight);
            bw.Write(Count);
            bw.Write(Inverse);
            return true;
        }
        public virtual bool Read(BinaryReader br)
        {
            TypeSel = br.ReadInt32();
            TypeName = C3DData.LoadString(br);
            ObjectSel = br.ReadInt32();
            ObjectName = C3DData.LoadString(br);
            PropertySel = br.ReadInt32();
            PropertyName = C3DData.LoadString(br);
            Minimum = br.ReadDouble();
            Maximum = br.ReadDouble();
            Weight = br.ReadDouble();
            MaximumWeight = br.ReadDouble();
            Count = br.ReadInt32();
            Inverse = br.ReadBoolean();
            return true;
        }
    }
    public class OverlayPropertyParaTrendAnalysis: OverlayPropertyPara  //矿层趋势分析
    {
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最小属性值"),Browsable(false)]
        public override double Minimum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大属性值"), Browsable(false)]
        public override double Maximum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("权重系数(%)")]
        public override double Weight { get; set; } = 5;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大权重(%)")]
        public override double MaximumWeight { get; set; } = 100;

        [CategoryAttribute("参数设置"), DisplayNameAttribute("是否反选？")]
        public override bool Inverse { get; set; } = false;

        [CategoryAttribute("参数设置"), DisplayNameAttribute("趋势面信息")]
        public override string Info { get; set; } = "";
        public OverlayPropertyParaTrendAnalysis()
        {
            Type = OverlayTypeEnum.TrendAnalysis;
        }
        public override string toString()
        {
            string ss = TypeName + " | ";
            ss += ObjectName + " | ";
            ss += PropertyName + " | ";            
            ss += "Weight = " + Weight + "|";
            ss += "MaxWeight = " + MaximumWeight;
            if (Inverse) ss += "| Inversed";
            return ss;
        }
               
    }
    public class OverlayPropertyPara3DGrid : OverlayPropertyPara
    {
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最小属性值")]
        public override double Minimum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大属性值")]
        public override double Maximum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("权重系数(%)")]
        public override double Weight { get; set; } = 5;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大权重(%)")]
        public override double MaximumWeight { get; set; } = 100;        
        [CategoryAttribute("参数设置"), DisplayNameAttribute("是否反选？"), Browsable(false)]
        public override bool Inverse { get; set; } = false;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("网格信息")]
        public override string Info {  get; set; } = "";
        public OverlayPropertyPara3DGrid()
        {
            Type = OverlayTypeEnum.Property3DGrid;
        }
        
    }
    public class OverlayPropertyPara岩性控矿2D : OverlayPropertyPara
    {
        [CategoryAttribute("参数设置"), DisplayNameAttribute("起始高度(m)") ]
        public override double Minimum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("结束高度(m)")]
        public override double Maximum { get; set; } = 100;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("权重系数(%)")]
        public override double Weight { get; set; } = 5;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大权重(%)")]
        public override double MaximumWeight { get; set; } = 100;

        [CategoryAttribute("参数设置"), DisplayNameAttribute("是否反选？")]
        public override bool Inverse { get; set; } = false;

        [CategoryAttribute("参数设置"), DisplayNameAttribute("断面信息")]
        public override string Info { get; set; } = "";
        public OverlayPropertyPara岩性控矿2D()
        {
            Type = OverlayTypeEnum.LayerProperty2D;
        }
        public override string toString()
        {
            string ss = TypeName;
            ss += " | " + ObjectName;
            ss += " | " + PropertyName;
            ss += " | " + "起始高度 = " + Minimum;
            ss += " | " + "结束高度 = " + Maximum;
            ss += " | " + "Weight = " + Weight;
            ss += " | " +  "MaxWeight = " + MaximumWeight;
            if (Inverse) ss += " | Inversed";
            return ss;
        }
    }
    public class OverlayPropertyPara断裂控矿2D : OverlayPropertyPara
    {
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最小偏移距离(m)")]
        public override double Minimum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大偏移距离(m)")]
        public override double Maximum { get; set; } = 100000;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("权重系数(%)")]
        public override double Weight { get; set; } = 5;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大权重(%)")]
        public override double MaximumWeight { get; set; } = 100;

        [CategoryAttribute("参数设置"), DisplayNameAttribute("是否反选？")]
        public override bool Inverse { get; set; } = false;

        [CategoryAttribute("参数设置"), DisplayNameAttribute("断面信息")]
        public override string Info { get; set; } = "";
        public OverlayPropertyPara断裂控矿2D()
        {
            Type = OverlayTypeEnum.Fault2D;
        }
        public override string toString()
        {
            string ss = TypeName;
            ss += " | " + ObjectName;
            ss += " | " + PropertyName;
            ss += " | " + "最小偏移 = " + Minimum;
            ss += " | " + "最大偏移 = " + Maximum;
            ss += " | " + "Weight = " + Weight;
            ss += " | " + "MaxWeight = " + MaximumWeight;
            if (Inverse) ss += " | Inversed";
            return ss;
        }
    }
    public class OverlayPropertyParaDistanceGaosi : OverlayPropertyPara
    {
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最近距离(m)")]
        public override double Minimum { get; set; } = 0;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最远距离(m)")]
        public override double Maximum { get; set; } = 10000;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("权重系数")]
        public override double Weight { get; set; } = 5;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("最大权重(%)")]
        public override double MaximumWeight { get; set; } = 100;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("勘查网度(m)")]
        public double MeshDistance { get; set; } = 100;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("Alpha(0-1)")]
        public double Alpha { get; set; } = 1;
        [CategoryAttribute("参数设置"), DisplayNameAttribute("是否反选？")]
        public override bool Inverse { get; set; } = false;

        [CategoryAttribute("参数设置"), DisplayNameAttribute("矿体信息")]
        public override string Info { get; set; } = "";
        public OverlayPropertyParaDistanceGaosi()
        {
            Type = OverlayTypeEnum.DistanceGaosi;
        }
        public override bool Load(BinaryReader br)
        {
            if (!base.Load(br)) return false;
            try
            {
                Alpha = br.ReadDouble();
                MeshDistance = br.ReadDouble();
                return true;
            }
            catch(Exception ex) 
            {
                errMessage = ex.Message;
                return false;
            }
        }
        public override bool Save(BinaryWriter wr)
        {
            if (!base.Save(wr)) return false;
            try
            {
                wr.Write(Alpha);
                wr.Write(MeshDistance);
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        public override double GetWeight(double dist)
        {
            if (Math.Abs(dist) < 1e-8) return 1;
            double d = dist * dist * 0.5;
            d = d / (MeshDistance * MeshDistance);
            return Alpha * Math.Exp(-d);
        }
        public double GetThresholdDistance(double threshold)
        {
            double dist = -2*MeshDistance*MeshDistance * Math.Log(threshold/Alpha);
            if (dist < 0) dist = 0;
            return Math.Sqrt(dist);
        }
        public override string toString()
        {
            string ss = TypeName;
            ss += " | " + ObjectName;
            ss += " | " + PropertyName;
            ss += " | " + "最近距离 = " + Minimum;
            ss += " | " + "最远距离 = " + Maximum;
            ss += " | " + "勘查网度 = " + MeshDistance;
            ss += " | " + "Alpha = " + Alpha;
            ss += " | " + "Weight = " + Weight;
            ss += " | " + "MaxWeight = " + MaximumWeight;
            if (Inverse) ss += " | Inversed";
            return ss;
        }
        public override bool Write(BinaryWriter bw)
        {
            base.Write(bw);
            bw.Write(MeshDistance);
            bw.Write(Alpha);
            return true;
        }
        public override bool Read(BinaryReader br)
        {
            base.Read(br);
            MeshDistance = br.ReadDouble();
            Alpha = br.ReadDouble();
            return true;
        }

    }
    #endregion 相关属性支持的类
}
