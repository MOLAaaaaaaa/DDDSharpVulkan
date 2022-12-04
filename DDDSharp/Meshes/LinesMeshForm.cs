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
    public partial class LinesMeshForm : Form
    {
        private List<int> list1 = new List<int>();
        private List<int> list2 = new List<int>();

        public LinesMeshForm()
        {
            InitializeComponent();
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

        private void GetDefaultNumbers()
        {
            int n1 = 0, n2 = 0;
            C3DLine line;
            for(int i=0;i<list2.Count;i++)
            {
                line = (C3DLine)C3DData.GetObjectByKey(i);
                if (i == 0) n1 = line.Count;
                else
                {
                    if (line.Count > n1) n1 = line.Count;
                }
            }
            n2 = n1;
            hortNumextBox.Text = n1.ToString();
            vertNumTextBox.Text = n2.ToString();
        }

        private void LinesMeshForm_Load(object sender, EventArgs e)
        {
            foreach (var item in C3DData.objectsDiction)
            {
                if(item.Value.type == ShapeEnum.Line )
                {
                    list1.Add(item.Key);
                }
            }
            UpdateList1();
        }
        private void UpdateList1()
        {
            comboBox1.Items.Clear();            
            foreach(int id in list1)
            {                
                comboBox1.Items.Add(C3DData.GetObjectByKey(id).Name);
            }
        }
        private void UpdateList2()
        {
            listBox1.Items.Clear();            
            foreach (int id in list2)
            {              
                listBox1.Items.Add(C3DData.GetObjectByKey(id).Name);
            }
            GetDefaultNumbers();
        }

        private void AddTo_Click(object sender, EventArgs e)
        {
            int sel = comboBox1.SelectedIndex;
            if (sel < 0) return;
            int id = list1[sel];
            list2.Add(id);
            list1.RemoveAt(sel);
            UpdateList1();
            UpdateList2();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            int id = list2[sel];

            list1.Add(id);
            list2.RemoveAt(sel);
            UpdateList1();
            UpdateList2();
        }
    }
}
