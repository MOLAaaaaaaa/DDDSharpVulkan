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
    public partial class PlyPropertyForm : DockingPaneExt
    {
        PlyFile ply = null;
        List<int> Keys = new List<int>();
        public PlyPropertyForm()
        {
            InitializeComponent();
        }
        public void UpdateList()
        {
            listBox1.Items.Clear();
            Keys.Clear();
            foreach (var item in C3DData.objectsDiction)               
            {                
                listBox1.Items.Add(item.Value.Name);
                Keys.Add(item.Key);
            }
        }
        private void PlyPropertyForm_Load(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if( listBox1.Items.Count>0 )
            {
                int sel = listBox1.SelectedIndex;
                if (sel >= 0)
                {
                    ply = (PlyFile)C3DData.GetObjectByKey(Keys[sel]);
                    propertyGrid1.SelectedObject = ply;
                }
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (listBox1.Items.Count <= 0) return;            
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;

            C3DData.SetObjectByKey( Keys[sel], ply );

            if ( e.ChangedItem.Label == "name")
            {
                UpdateList();
            }
            else
            {
                Program.m_MainForm.m_DDDForm.UpdateDraw();
            }
        }
    }
}
