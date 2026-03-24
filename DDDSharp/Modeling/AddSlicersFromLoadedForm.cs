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

namespace DDDSharp.Modeling
{
    public partial class AddSlicersFromLoadedForm : Form
    {
        public List<PolygonSlicer> selectedSlicers = new List<PolygonSlicer>();
        List<PolygonSlicer> unselectedSlicers = new List<PolygonSlicer>();

        public AddSlicersFromLoadedForm(List<PolygonSlicer>_unselectedSlicers)
        {
            InitializeComponent();
            unselectedSlicers = _unselectedSlicers;
        }        
        void UpdateList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < unselectedSlicers.Count; i++)
            {
                listBox1.Items.Add(unselectedSlicers[i].Name);
            }
            listBox1.SelectedIndices.Clear();            
        }
        private void AddSlicersFromLoadedForm_Load(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndices.Count>0)
            {
                for(int i=0;i<listBox1.SelectedIndices.Count;i++)
                {
                    int id = listBox1.SelectedIndices[i];
                    selectedSlicers.Add(unselectedSlicers[id]);
                }
            }
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
