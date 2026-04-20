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
    public partial class IntersetWithMeshesForm : Form
    {        
        public List<C3DLine> Lines = new List<C3DLine>();
        List<int> models = new List<int>(); //Slicer and Meshes列表
        List<int> targets = new List<int>();
        public int sourceSel = -1;
        public int KeepWithSelection = 0;
        public IntersetWithMeshesForm()
        {
            InitializeComponent();
        }
        void UpdateSourceSelectList()
        {
            comboBox1.Items.Clear();
            int id;
            C3DObjectBase obj;
            for ( int i = 0; i < models.Count; i++ )
            {
                id = models[i];
                obj = C3DData.GetObjectByKey(id);
                comboBox1.Items.Add( obj.type + ":" + obj.Name);
            }
            if ( sourceSel >= 0 ) comboBox1.SelectedIndex = models.IndexOf(sourceSel);            
        }

        void UpdateTargetSelectList()
        {
            comboBox2.Items.Clear();
            int id;
            C3DObjectBase obj;
            for ( int i = 0; i < models.Count; i++ )
            {
                id = models[i];
                obj = C3DData.GetObjectByKey(id);
                comboBox2.Items.Add( obj.type + ":" + obj.Name);
            }
            
            //Init selection
            for( int i = 0; i < models.Count; i++ )
            {
                id = models[i];
                if(id != sourceSel )
                {
                    comboBox2.SelectedIndex = i;
                    break;
                }
            }            
        }
        void UpdateTargetsListBox()
        {
            listBox1.Items.Clear();            
            int id;
            C3DObjectBase obj;
            for (int i = 0; i < targets.Count; i++)
            {
                id = targets[i];
                obj = C3DData.GetObjectByKey(id);
                listBox1.Items.Add(obj.type + ":" + obj.Name);
            }
            listBox1.SelectedIndex = listBox1.Items.Count-1;
        }
        private void IntersetWithMeshesForm_Load(object sender, EventArgs e)
        {
            C3DObjectBase obj;
            foreach(var item in C3DData.objectsDiction)
            {
                obj = item.Value;
                if ( obj.type == ShapeEnum.Mesh || obj.type == ShapeEnum.Slicer )
                    models.Add(item.Key);
            }
            
            comboBox3.Items.Add("Source Properties");
            comboBox3.Items.Add("Targets Properties");
            comboBox3.SelectedIndex = 0;

            UpdateSourceSelectList();
            if (models.Count > 0) 
            { 
                comboBox1.SelectedIndex = 0;
                sourceSel = models[comboBox1.SelectedIndex];
            }
            
            UpdateTargetSelectList();
        }//IntersetWithMeshesForm_Load

        private void AddToButton_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex >= 0 )sourceSel = models[comboBox1.SelectedIndex];

            if (models.Count < 1 || comboBox2.SelectedIndex < 0 )
                return;
            
            int id = models[comboBox2.SelectedIndex];
            C3DObjectBase obj = C3DData.GetObjectByKey(id);

            targets.Add(id);
            models.RemoveAt(comboBox2.SelectedIndex);

            UpdateSourceSelectList();
            UpdateTargetSelectList();
            UpdateTargetsListBox();            
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0) sourceSel = models[comboBox1.SelectedIndex];

            if ( listBox1.Items.Count < 1 || listBox1.SelectedIndex < 0)
                return;

            int id = targets[listBox1.SelectedIndex];
            
            targets.RemoveAt(listBox1.SelectedIndex);
            models.Add(id);

            UpdateSourceSelectList();
            UpdateTargetSelectList();
            UpdateTargetsListBox();
        }
        C3DLine[] IntersectWith(CSlicer slicer, CSlicer slicer1)
        {
            return slicer.CreateIntersectionLines(slicer1);
        }
        C3DLine []IntersectWith(CSlicer slicer,CMesh mesh )
        {
            return slicer.CreateIntersectionLines(mesh);
        }
        C3DLine[] IntersectWith(CMesh mesh, CSlicer slicer)
        {
            return slicer.CreateIntersectionLines(mesh);
        }
        C3DLine[] IntersectWith(CMesh mesh1, CMesh mesh2)
        {
            return null;
        }
        void DoIntersectWith()
        {
            this.Cursor = Cursors.WaitCursor;

            C3DObjectBase source = C3DData.GetObjectByKey(sourceSel); 
            
            for (int i = 0; i < targets.Count; i++)
            {
                C3DObjectBase obj = C3DData.GetObjectByKey(targets[i]);
                C3DLine[] lines = null;
                if (source.type == ShapeEnum.Slicer)
                {
                    if (obj.type == ShapeEnum.Slicer) 
                        lines = IntersectWith((CSlicer)source, (CSlicer)obj);
                    else if (obj.type == ShapeEnum.Mesh)
                        lines = IntersectWith((CSlicer)source, (CMesh)obj);
                }
                else if (source.type == ShapeEnum.Mesh)
                {
                    if (obj.type == ShapeEnum.Slicer)
                        lines = IntersectWith((CMesh)source, (CSlicer)obj);
                    else if (obj.type == ShapeEnum.Mesh)
                        lines = IntersectWith((CMesh)source, (CMesh)obj);
                }
                if (lines != null && lines.Length > 0)
                {
                    foreach (C3DLine line in lines)
                    { 
                        if(line.Count > 0 ) Lines.Add(line); 
                    }
                }
            }

            this.Cursor = Cursors.Default;
        }
        private void OK_Click(object sender, EventArgs e)
        {
            if( comboBox1.SelectedIndex < 0 )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择源对象。" : "No source object selected.");
                return;
            }
            if (targets.Count < 1)
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择目标对象。" : "No target objects selected.");
                return;
            }
            
            KeepWithSelection = comboBox3.SelectedIndex;
            
            DoIntersectWith();

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
