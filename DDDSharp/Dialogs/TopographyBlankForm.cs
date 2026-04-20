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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

using DDDSharp;
namespace DDDSharp.Dialogs
{
    public partial class TopographyBlankForm : Form
    {
        List<C3DObjectBase> objects = C3DData.GetObjects();
        List<int> objects1 = new List<int>(); //待选择对象
        List<int> objects2 = new List<int>(); //已选择对象
        List<int> topoobjects = new List<int>(); //地形对象列表
        public TopographyBlankForm()
        {
            InitializeComponent();
        }
        void UpdateList()
        {            
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            for (int i=0;i<objects1.Count; i++) 
            {
                listBox1.Items.Add(objects[objects1[i]].Name);
            }
            for (int i = 0; i < objects2.Count; i++)
            {
                listBox2.Items.Add(objects[objects2[i]].Name);
            }
        }

        private void TopographyBlankForm_Load(object sender, EventArgs e)
        {
            int n = C3DData.objectsDiction.Count;
            for (int i = 0; i < n; i++) 
            { 
                objects1.Add(i);
                C3DObjectBase obj = objects[i];
                if(obj.type == ShapeEnum.Mesh)
                {
                    comboBox1.Items.Add(objects[i].Name);
                    topoobjects.Add(i);
                }
            }
            
            UpdateList();
        }

        private void AddToButton_Click(object sender, EventArgs e)
        {
            int sel1 = listBox1.SelectedIndex;
            if (sel1 < 0) return;
            
            int id = objects1[sel1];
            objects2.Add(id);
            objects1.RemoveAt(sel1);

            UpdateList();
        }

        private void RemoveFromButton_Click(object sender, EventArgs e)
        {
            int sel2 = listBox2.SelectedIndex;
            if (sel2 < 0) return;

            int id = objects2[sel2];
            objects1.Add(id);
            objects2.RemoveAt(sel2);

            UpdateList();
        }
        bool DoTopographyBlank()
        {
            CMesh mesh = (CMesh)objects[topoobjects[comboBox1.SelectedIndex]];
            for(int i=0;i<objects2.Count;i++) 
            {
                C3DObjectBase obj = objects[objects2[i]];
                obj.TopographyBlank(mesh);
            }

            return true;
        }
        private void OK_Click(object sender, EventArgs e)
        {
            if(objects2.Count < 1) 
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择对象。" : "No objects selected.");
                return;
            }
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未加载地形对象。" : "No topography objects loaded.");
                return;
            }

            Cursor = Cursors.WaitCursor;
            
            DoTopographyBlank();
            
            Cursor = Cursors.Default;

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
