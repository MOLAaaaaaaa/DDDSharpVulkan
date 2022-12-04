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
using OpenCLNet;
using CLInterpolation;
using DataCollection;
using DDDSharp.Dialogs;
namespace DDDSharp.Boreholes
{
    public partial class ImportFromMeshesForm : Form
    {
        public List<CMesh> Meshes = new List<CMesh>();
        public bool IsZDepth = false;
        public bool IsTopFace = true;

        public ImportFromMeshesForm()
        {
            InitializeComponent();            
        }
        void UpdateList2()
        {
            listBox2.Items.Clear();
            if (Meshes.Count < 1) return;

            CMesh sp;
            string ss;
            for (int i = 0; i < Meshes.Count; i++)
            {
                sp = Meshes[i];
                ss = (i + 1) + "=>";
                ss += sp.errMessage;
                ss += ", ";
                ss += "minz = " + sp.minv;
                ss += " to ";
                ss += sp.maxv;
                listBox2.Items.Add(ss);
            }
            listBox2.SelectedIndex = Meshes.Count - 1;
        }
        private void AddButton2_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Formatted mesh files(*.mesh)|*.mesh | Surfer grid(*.grd) | *.grd|all files(*.*)|*.*";
                dlg.Multiselect = true;
                string ext;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in dlg.FileNames)
                    {
                        ext = Path.GetExtension(file).ToLower();
                        if (ext == ".grd")
                        {
                            CSurferGrid cs = new CSurferGrid();
                            if (!cs.Read(file))
                            {
                                MessageBox.Show("Load gridding data faild.\n" + cs.errMessage);
                                break;
                            }
                            CMesh mesh = new CMesh();
                            mesh.fromGrid2D(cs);
                            mesh.errMessage = Path.GetFileName(file);
                            Meshes.Add(mesh);
                        }
                        else //if (ext == ".mesh")
                        {
                            CMesh mesh = new CMesh();
                            if (!mesh.LoadFrom(file))
                            {
                                MessageBox.Show("Load gridding data faild.\n" + mesh.errMessage);
                                break;
                            }
                            else
                            {
                                mesh.errMessage = Path.GetFileName(file);
                                Meshes.Add(mesh);
                            }
                        }
                    }
                    SortMeshes(IsZDepth);
                    UpdateList2();
                }
            }
        }
        void SortMeshes(bool zdepth)
        {
            if (zdepth)//按Z值排序
            {
                Meshes.Sort((a, b) => { return ((a.minv + a.maxv)).CompareTo((b.minv + b.maxv)); });
            }
            else
                Meshes.Sort((a, b) => { return ((b.minv + b.maxv)).CompareTo((a.minv + a.maxv)); });

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
                Meshes.RemoveAt(indices[i]);
                listBox2.Items.RemoveAt(indices[i]);
            }
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel >= 1 && sel < Meshes.Count)
            {
                CMesh cs1 = Meshes[sel - 1];
                CMesh cs2 = Meshes[sel];
                Meshes[sel - 1] = cs2;
                Meshes[sel] = cs1;
                UpdateList2();
                listBox2.SelectedIndex = sel - 1;
            }
        }

        private void MoveDownBotton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel >= 0 && sel < Meshes.Count - 1)
            {
                CMesh cs1 = Meshes[sel + 1];
                CMesh cs2 = Meshes[sel];
                Meshes[sel + 1] = cs2;
                Meshes[sel] = cs1;
                UpdateList2();
                listBox2.SelectedIndex = sel + 1;
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) IsZDepth = true;
            else IsZDepth = false;
            
            IsTopFace = checkBox1.Checked;
            
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ImportFromMeshesForm_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Depth");
            comboBox1.Items.Add("Elevation");
            if(IsZDepth) comboBox1.SelectedIndex = 0;
            else comboBox1.SelectedIndex = 1;
            if (IsTopFace) checkBox1.Checked = true;
            else checkBox1.Checked = false;
        }

        private void ReverseButton_Click(object sender, EventArgs e)
        {
            Meshes.Reverse();
            UpdateList2();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) IsZDepth = true;
            else IsZDepth = false;
            SortMeshes(IsZDepth);
            UpdateList2();
        }
    }
}
