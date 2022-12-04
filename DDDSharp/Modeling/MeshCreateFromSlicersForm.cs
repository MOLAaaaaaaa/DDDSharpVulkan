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
namespace DDDSharp
{
    public partial class MeshCreateFromSlicersForm : Form
    {
        public List<PolygonSlicer> slicers = new List<PolygonSlicer>();
        List<LayerProperty> layers = new List<LayerProperty>();
        List<string> Selectedlayers = new List<string>();

        public MeshCreateFromSlicersForm()
        {
            InitializeComponent();
            radioButton1.Checked = true;
            radioButton2.Checked = false;
            textBox1.Text = "50";
            textBox2.Text = "50";
            ThichnessTextBox.Text = "0";
            checkBox1.Checked = false;
        }
        private void MeshCreateFromSlicersForm_Load(object sender, EventArgs e)
        {
            
        }
        private void LoadSlicersButton_Click(object sender, EventArgs e)
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
                            if ( slicer.LoadFrom(dlg.FileNames[i]))
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
        
        LineMesh CreateLineMeshFromSlicers(string section)
        {
            LineMesh ms = new LineMesh(section);
            foreach(PolygonSlicer slicer in slicers)
            {
                //check section
                foreach ( Polygon2D poly in slicer.tracedGeoObjects.Polygons )
                {
                    if ( !poly.IsClosed && poly.Name.ToLower() == section.ToLower() && poly.IsValid )
                    {
                        C3DLine  line = slicer.toTraced3DLine(poly.Smooth());
                        line.Name = slicer.Name;
                        ms.Add(line);
                    }
                }
            }
            ms.UpdateRange();
            return ms;
        }

        private void CreateMeshesButton_Click(object sender, EventArgs e)
        {
            GetSelectedLayers();
            if ( Selectedlayers.Count < 1 && radioButton1.Checked )
            {
                MessageBox.Show("No line objects selected.");
                return;
            }
            if (layers.Count < 1 && radioButton2.Checked)
            {
                MessageBox.Show("No line object found in slicers.");
                return;
            }
            int xgrid = 100;
            int ygrid = 100;
            if ( !int.TryParse(textBox1.Text,out xgrid) || !int.TryParse(textBox2.Text, out ygrid) )
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
            for(int i=0;i<Selectedlayers.Count;i++)
            {
                linemeshes.Add(CreateLineMeshFromSlicers(Selectedlayers[i]));
            }
            
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
                if( mesh != null ) C3DData.AddObject(mesh,true);               
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
    }
}
