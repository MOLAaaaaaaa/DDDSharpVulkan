using DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDDSharp.Grid3DProperty
{
    public partial class MeshesFrom3DGridsForm : Form
    {
        public bool Created = false;
        List<C3DGridData>gridDatas = new List<C3DGridData>();
        MultiPropertiesMarchingCubes mc = new MultiPropertiesMarchingCubes();
        public MeshesFrom3DGridsForm()
        {
            InitializeComponent();
        }
        void UpdateList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < gridDatas.Count; i++) 
            {
                listBox1.Items.Add(gridDatas[i].Name);
            }
        }
        void Get3DGridDatas()
        {
            gridDatas.Clear();
            List<C3DObjectBase> objs = C3DData.GetObjects();
            for (int i = 0; i < objs.Count; i++)
            {
                if (objs[i].type == ShapeEnum.Grid3D)
                {
                    C3DGridData data = objs[i] as C3DGridData;
                    gridDatas.Add(data);
                }
            }
        }
        private void MeshesFrom3DGridsForm_Load(object sender, EventArgs e)
        {
            Get3DGridDatas();
            UpdateList();
        }

        private void remove_button_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if(sel >= 0) 
            {
                gridDatas.RemoveAt(sel);
                UpdateList();
            }
        }

        private void Create_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            mc.Clear();
            for (int i = 0; i < gridDatas.Count; i++)
            {
                mc.AddProperty(gridDatas[i]);
            }
            if (!mc.DoSearchMultipleEdges()) MessageBox.Show(mc.m_ErrInfo);
            else MessageBox.Show("Created!");

            Cursor = Cursors.Default;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (mc.pISOSurfaceExt.TriangleCount > 0)
            { 
                C3DData.AddObject(mc.toTiangleObj(), false);
                mc.Clear();
                Created = true;
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
