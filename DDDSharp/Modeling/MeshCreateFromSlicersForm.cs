using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
using DDDSharp.Modeling;

namespace DDDSharp
{
    public partial class MeshCreateFromSlicersForm : Form
    {
        public List<CMesh> createdMeshes = new List<CMesh>();
        public List<SlicerFaultStruct> slicerFalutsList = new List<SlicerFaultStruct>();
        public List<SlicerFaultStruct> selctedSlicerFalutsList = new List<SlicerFaultStruct>();
        public List<PolygonSlicer> slicers = new List<PolygonSlicer>();        
        List<LayerProperty> layers = new List<LayerProperty>();
        List<string> Selectedlayers = new List<string>();
        List<PolygonSlicer> loadedSlicers = new List<PolygonSlicer>();
        string errMessage = "";
        int added = 0;       

        public MeshCreateFromSlicersForm()
        {
            InitializeComponent();
            textBox1.Text = "50";
            textBox2.Text = "50";
            ThichnessTextBox.Text = "0";
            checkBox1.Checked = false;
            HideCheckBox.Checked = true;
        }

        private void MeshCreateFromSlicersForm_Load(object sender, EventArgs e)
        {
            List<C3DObjectBase>objects = C3DData.GetObjects();
            for(int i=0;i < objects.Count;i++)
            {
                if( objects[i].type == ShapeEnum.PolygonSlicer )
                {
                    PolygonSlicer slicer = objects[i] as PolygonSlicer;
                    loadedSlicers.Add(slicer);
                    if(HideCheckBox.Checked) { if(slicer.Visible) slicers.Add(slicer); }
                    else slicers.Add(slicer);
                }
            }
            UpdateList1();
        }
        bool LoadFromFiles()
        {
            errMessage = "";
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Slicers(*.Slicer)|*.Slicer|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            PolygonSlicer slicer = new PolygonSlicer();
                            if (slicer.LoadFrom(dlg.FileNames[i]))
                            {
                                slicers.Add(slicer);
                                added++;
                            }
                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
                
                return true;
            }
            catch (Exception ex)
            {
                errMessage += ex.Message;
                errMessage += Environment.NewLine;                
                return false;
            }
        }


        bool IsExistedIn(PolygonSlicer slicer, List<PolygonSlicer> _slicers)
        {
            for (int i = 0; i < _slicers.Count; i++)
            {
                if (slicer == _slicers[i]) return true;
            }
            return false;
        }
        bool IsFaultInList(SlicerFaultStruct s)
        {
            for (int i = 0; i < slicerFalutsList.Count; i++)
            {               
                if (s == slicerFalutsList[i]) 
                    return true;
            }
            return false;
        }
        private void LoadSlicersButton_Click(object sender, EventArgs e)
        {
            added = 0;
            
            List<PolygonSlicer>unselecedslicers = new List<PolygonSlicer>();
            for (int i = 0; i < loadedSlicers.Count; i++)
            {
                PolygonSlicer slicer = loadedSlicers[i];
                if (!IsExistedIn(slicer,slicers)) unselecedslicers.Add(slicer);
            }

            AddSlicersFromLoadedForm dlg = new AddSlicersFromLoadedForm(unselecedslicers);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            for(int i=0;i< dlg.selectedSlicers.Count;i++)
            {
                slicers.Add(dlg.selectedSlicers[i]);
                added++;
            }
            dlg.selectedSlicers.Clear();
            if (added > 0)
            {
                SearchLayerValues();
                UpdateList1();
                UpdateList2();
            }
        }

        private void FromFileButton_Click(object sender, EventArgs e)
        {
            added = 0;
            if (!LoadFromFiles()) MessageBox.Show(errMessage);               
            if (added > 0)
            {               
                UpdateList1();
                UpdateList2();
            }
        }
        public void AddSlicer(PolygonSlicer s)
        {
            slicers.Add(s);
        }
        bool IsInList(string name)
        {
            foreach (LayerProperty s in layers)
            {
                if (s.LayerName.ToLower() == name.ToLower()) return true;
            }
            return false;
        }

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
                    if (p.IsClosed) continue;//closed is layer,otherwise is line

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

        int GetSelectedLayers()
        {
            Selectedlayers.Clear();
            int count = listBox2.Items.Count;
            if ( count < 1 ) return 0;

            bool[] marks = new bool[count];
            for (int i = 0; i < count; i++)
                marks[i] = false;

            int id = 0;
            for (int i = 0; i < listBox2.SelectedIndices.Count; i++)
            {
                id = listBox2.SelectedIndices[i];
                if ( !marks[id] )
                {
                    Selectedlayers.Add( layers[id].LayerName.ToString().ToLower().Trim() );
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
            listBox1.SelectedIndex = -1;
        }
        void UpdateList3() 
        {
            listBox3.Items.Clear();
            for (int i = 0; i < selctedSlicerFalutsList.Count; i++)
            {
                listBox3.Items.Add(selctedSlicerFalutsList[i].ToString());
            }
            listBox3.SelectedIndex = -1;
        }
        private void UpdateList2()
        {
            listBox2.Items.Clear();
            for (int i = 0; i < slicerFalutsList.Count; i++)
            {
                listBox2.Items.Add(slicerFalutsList[i].ToString());
            }            
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel <= 0) return;
            SlicerFaultStruct cur = slicerFalutsList[sel];
            slicerFalutsList[sel] = slicerFalutsList[sel - 1];
            slicerFalutsList[sel - 1] = cur;
            UpdateList2();
            listBox2.SelectedIndex = sel - 1;
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel < 0 || sel >= slicerFalutsList.Count) return;

            SlicerFaultStruct cur = slicerFalutsList[sel];
            slicerFalutsList[sel] = slicerFalutsList[sel + 1];
            slicerFalutsList[sel + 1] = cur;
            UpdateList2();
            listBox2.SelectedIndex = sel + 1;
        }
        
        LineMesh CreateLineMeshFromSlicers(string section)
        {
            LineMesh ms = new LineMesh(section);
            for(int i=0;i<slicerFalutsList.Count;i++)                     
            {
                string falutName = slicerFalutsList[i].FalutName;
                PolygonSlicer slicer = slicerFalutsList[i].Slicer;
                foreach ( Polygon2D poly in slicer.tracedGeoObjects.Polygons )
                {
                    if ( !poly.IsClosed && poly.IsValid &&
                         poly.Name == falutName )
                    {
                        C3DLine  line = slicer.toTraced3DLine(poly.Smooth());
                        line.Name = slicer.Name;
                        ms.Add(line);                        
                        break;
                    }
                }
            }
            ms.UpdateRange();
            return ms;
        }

        private void CreateMeshesButton_Click(object sender, EventArgs e)
        {
           
            //if ( Selectedlayers.Count < 1 && radioButton1.Checked )
            //{
            //    MessageBox.Show("No line objects selected.");
            //    return;
            //}
            //if (layers.Count < 1 && radioButton2.Checked)
            //{
            //    MessageBox.Show("No line object found in slicers.");
            //    return;
            //}
            int xgrid = 100;
            int ygrid = 100;
            if ( !int.TryParse(textBox1.Text,out xgrid) || 
                 !int.TryParse(textBox2.Text, out ygrid) )
            {
                MessageBox.Show("meshes grid not correct.");
                return;
            }

            if (xgrid < 2 || xgrid > 1000 || ygrid < 2 || ygrid > 1000)
            {
                MessageBox.Show("meshes grid not correct.");
                return;
            }

            List<LineMesh> linemeshes = new List<LineMesh>();
            linemeshes.Add(CreateLineMeshFromSlicers(slicerFalutsList[0].FalutName));

            float thickness = 0;
            if (!float.TryParse(ThichnessTextBox.Text, out thickness)) thickness = 0;
            thickness = Math.Abs(thickness);

            for( int i=0; i < linemeshes.Count; i++ )
            {
                //返回null，线 < 2 
                CMesh mesh = linemeshes[i].CreateGridMesh(xgrid + 1, ygrid + 1,checkBox1.Checked );

                /*
                TriangleObj obj = mesh.toTriangleObj(thickness);
                obj.name = linemeshes[i].name + "_mesh";
                mesh.Clear();*/
                
                if ( mesh != null ) createdMeshes.Add(mesh);
            }
            
            linemeshes.Clear();

            MessageBox.Show("Created.");
        }

        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            slicers.RemoveAt(sel);
            UpdateList1();          
            listBox3.Items.Clear();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            if(slicers.Count > 1)
            {
                var ret = MessageBox.Show("Are you want to remove all items?","Remove All Items?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2);
                if (ret != DialogResult.Yes) return;
            }
            slicers.Clear();            
            listBox1.Items.Clear();
            listBox3.Items.Clear();
        }

        void UpdateSelectedSlicers()
        {
            selctedSlicerFalutsList.Clear();
            int id = listBox1.SelectedIndex;
            if (id < 0) return;            
            PolygonSlicer slicer = slicers[id];
            for (int j = 0; j < slicer.tracedGeoObjects.Count; j++)
            {
                Polygon2D poly = slicer.tracedGeoObjects[j];
                SlicerFaultStruct s = new SlicerFaultStruct(slicer, poly.Name);
                if (!IsFaultInList(s)) selctedSlicerFalutsList.Add(s);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedSlicers();
            UpdateList3();
        }

        private void AddToButton_Click(object sender, EventArgs e)
        {
            int sel = listBox3.SelectedIndex;
            if (sel < 0) return;
            SlicerFaultStruct s = selctedSlicerFalutsList[sel];
            slicerFalutsList.Add((SlicerFaultStruct)s);
            selctedSlicerFalutsList.RemoveAt(sel);
            UpdateList3();
            UpdateList2();
        }

        private void RemoveFromButton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel < 0) return;
            slicerFalutsList.RemoveAt(sel);
            UpdateSelectedSlicers();
            UpdateList3();
            UpdateList2();
        }

        private void ListBox2Clr_Click(object sender, EventArgs e)
        {
            if (slicerFalutsList.Count > 1)
            {
                var ret = MessageBox.Show("Are you want to remove all items?", "Remove All Items?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (ret != DialogResult.Yes) return;
            }
            slicerFalutsList.Clear();
            listBox2.Items.Clear();
        }

        private void HideCheckBox_Click(object sender, EventArgs e)
        {
            HideCheckBox.Checked = !HideCheckBox.Checked;
            UpdateList1();
        }
    }

    public struct SlicerFaultStruct
    {
        public PolygonSlicer Slicer;
        public string FalutName;
        
        public SlicerFaultStruct(PolygonSlicer slicer, string falutname)
        {
            Slicer = slicer;
            FalutName = falutname;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is SlicerFaultStruct))
            {
                return false;
            }
            SlicerFaultStruct p = (SlicerFaultStruct)obj;
            return Slicer.Name.Equals(p.Slicer.Name) &&
                   FalutName.Equals(p.FalutName);
        }
        public override string ToString()
        {
            return Slicer.Name + "->" + FalutName;
        }
        public override int GetHashCode()
        {
            return Slicer.GetHashCode() ^ FalutName.GetHashCode();
        }
        public static bool operator ==(SlicerFaultStruct s1, SlicerFaultStruct s2)
        {

            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(s1, s2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)s1 == null) || ((object)s2 == null))
            {
                return false;
            }
            return (s1.Slicer.Name == s2.Slicer.Name) &&
                   (s1.FalutName == s2.FalutName);
        }
        public static bool operator !=(SlicerFaultStruct s1, SlicerFaultStruct s2)
        {
            // Equals handles case of null on right side.
            return (!(s1 == s2));
        }
    }
}
