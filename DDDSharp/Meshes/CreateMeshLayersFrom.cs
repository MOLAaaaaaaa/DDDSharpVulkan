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
using DataCollection;
using CLInterpolation;

namespace DDDSharp
{
    public partial class CreateMeshLayersFrom : Form
    {
        public List<GeoMesh> Layers = new List<GeoMesh>();
        public double ZScale = 1.0;
        public CreateMeshLayersFrom()
        {
            InitializeComponent();            
        }
        
        private void CreateMeshLayersFrom_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Blank Upper Layer");
            comboBox1.Items.Add("Blank Lower Layer");
            comboBox1.SelectedIndex = 0;
            checkBox1.Checked = true;            
        }
        string FormatString(GeoMesh mesh)
        {
            string ss = "";
            ss += "Name =" + mesh.Name;
            ss += ", ";
            //ss += "Path =" + mesh.errMessage;//文件路径
            //ss += ", ";
            ss += "Z = " + mesh.minv;
            ss += " to ";
            ss += mesh.maxv;            
            ss += ", ";
            ss += "Depth = " + mesh.Depth;
            ss += ", ";
            if (mesh.IsTop) ss += "TopFace";
            else ss += "BottomFace";
            return ss;
        }
        void UpdateList2()
        {
            listBox2.Items.Clear();
            if (Layers.Count < 1) return;
            GeoMesh sp;
            string ss;
            for (int i = 0; i < Layers.Count; i++)
            {
                sp = Layers[i];
                ss = (i + 1) + "=>";
                ss += FormatString(sp);
                listBox2.Items.Add(ss);
            }            
        }

        private void AddButton2_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "surfer grid(*.grd) | *.grd|all files(*.*)|*.*";
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in dlg.FileNames)
                    {
                        CSurferGrid cs = new CSurferGrid();                        
                        if ( !cs.Read(file) )
                        {
                            MessageBox.Show("Load gridding data faild.\n" + cs.errMessage);
                            break;
                        }
                        cs.errMessage = Path.GetFileName(file);
                        GeoMesh mesh = new GeoMesh();
                        mesh.fromGrid2D(cs);
                        Layers.Add(mesh);                        
                    }
                    UpdateList2();
                }
            }
        }

        private void RemoveButton2_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndices.Count < 1)
            {
                MessageBox.Show("no selections.");
                return;
            }

            List<int> indices = new List<int>();
            foreach (int sel in listBox2.SelectedIndices)
            {
                indices.Add(sel);
            }

            indices.Sort((a, b) => { return b.CompareTo(a); });

            for (int i = 0; i < indices.Count; i++)
            {
                Layers.RemoveAt(indices[i]);
                listBox2.Items.RemoveAt(indices[i]);
            }
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel >= 1 && sel < Layers.Count)
            {
                GeoMesh cs1 = Layers[sel - 1];
                GeoMesh cs2 = Layers[sel];
                Layers[sel - 1] = cs2;
                Layers[sel] = cs1;
                UpdateList2();
                listBox2.SelectedIndex = sel - 1;
            }
        }

        private void MoveDownBotton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel >= 0 && sel < Layers.Count - 1)
            {
                GeoMesh cs1 = Layers[sel + 1];
                GeoMesh cs2 = Layers[sel];
                Layers[sel + 1] = cs2;
                Layers[sel] = cs1;
                UpdateList2();
                listBox2.SelectedIndex = sel + 1;
            }
        }

        private void OKBUTTON_Click(object sender, EventArgs e)
        {
            if( Layers.Count < 1 )
            {
                MessageBox.Show("No Layers Loaded.");
                return;
            }           
            

            Cursor = Cursors.WaitCursor;

            //地层相交切割--尖灭处理
            
            MeshesRestrictedProfilesInterpolation ms = new MeshesRestrictedProfilesInterpolation();
            foreach(GeoMesh cs in Layers)
            {
                ms.Layers.Add(cs);
            }            
            if (checkBox1.Checked)
            { 
                if (comboBox1.SelectedIndex == 0) ms.BlankLayers(true, true);
                else if (comboBox1.SelectedIndex == 1) ms.BlankLayers(true, false);
            }
            
            Cursor = Cursors.Default;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCELBUTTON_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel < 0) return;
            propertyGrid1.SelectedObject = Layers[sel];
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel < 0) return;
            if (propertyGrid1.SelectedObject == null) return;
            GeoMesh mesh = Layers[sel];
            string ss = (sel + 1) + "=>";
            ss += FormatString(mesh);
            listBox2.Items[sel] = ss;
        }
    }
}
