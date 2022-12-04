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
    public partial class CylinderIntersectForm : Form
    {
        public List<int> pSelected = new List<int>();
        public List<int> pUnSelected = new List<int>();
        public List<CCylinderExt> pObjects = new List<CCylinderExt>();

        public CylinderIntersectForm()
        {
            InitializeComponent();
        }
        private void GetObjects()
        {
            C3DObjectBase cy;
            pObjects.Clear();
            List<C3DObjectBase> objects = C3DData.GetObjects();
            for (int i = 0; i < objects.Count; i++)
            {
                cy = objects[i];
                if (cy.type == ShapeEnum.Shape )
                {
                    CCylinderExt symbol = (CCylinderExt)cy;
                    pObjects.Add(symbol);
                }
            }
        }
        private void CylinderIntersectForm_Load(object sender, EventArgs e)
        {
            GetObjects();

            foreach(var item in C3DData.objectsDiction)            
            {                
                if ( item.Value.type == ShapeEnum.Cylinder )
                {
                    CCylinderExt symbol = (CCylinderExt)item.Value;
                    pObjects.Add(symbol);
                }
                pSelected.Add(item.Key);
            }

            UpdateList();
        }
        private bool IsSelected(int index)
        {
            for (int i = 0; i < pSelected.Count; i++)
            {
                if (pSelected[i] == index) return true;
            }
            return false;
        }
        private void UpdateList()
        {
            C3DObjectBase cy;
            int id;
            listBox1.Items.Clear();
            for ( int i = 0; i < pSelected.Count; i++ )
            {
                id = pSelected[i];
                if (id >= 0)
                {
                    cy = C3DData.GetObjectByKey(id);
                    listBox1.Items.Add(cy.Name);
                }
            }
            listBox2.Items.Clear();
            for (int i = 0; i < pUnSelected.Count; i++)
            {
                id = pUnSelected[i];
                cy = pObjects[id];
                listBox2.Items.Add(cy.Name);
            }
        }
        
        private void AddTobutton1_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel < 0) return;
            pSelected.Add(pUnSelected[sel]);
            pUnSelected.RemoveAt(sel);
            UpdateList();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            
            pUnSelected.Add(pSelected[sel]);
            pSelected.RemoveAt(sel);

            UpdateList();
        }

        private void OKbutton1_Click(object sender, EventArgs e)
        {
            //if (pSelected.Count < 2) return;

            //int id = pSelected[0];
            //CCylinderExt cy = pObjects[id];
            //CCylinderExt cy1;
            //for (int i=1;i<pSelected.Count;i++)
            //{
            //    id = pSelected[i];
            //    cy1 = pObjects[id];
            //    cy = CCylinderExt.Intersect(cy, cy1);
            //    cy.Name = cy.Name + "_" + cy1.Name;
            //}

            //C3DData.AddObject(cy);

            DialogResult = DialogResult.OK;
            Close();
        }
        private void Cancelbutton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
