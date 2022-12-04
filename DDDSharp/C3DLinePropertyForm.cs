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
    public partial class C3DLinePropertyForm : DockingPaneExt
    {
        public C3DLine line = null;
        public double length = 0;
        public double rad = 0;        

        public C3DLinePropertyForm()
        {
            InitializeComponent();           
        }
        public void UpdateLength()
        {
            if (GetCurSelectedObject())
            {
                length = line.GetLength();
                textBoxLength.Text = length.ToString();
                textRadiuBox.Text = (length / 10).ToString();
            }
        }
        private void C3DLinePropertyForm_Load(object sender, EventArgs e)
        {
            UpdateLength();
        }
        public bool GetCurSelectedObject()
        {
            line = (C3DLine)C3DData.GetSelectedObj(ShapeEnum.Line);
            if (line == null) return false;
            else return true;
        }
       
        private void buttonCreatePolygon_Click(object sender, EventArgs e)
        {
            if ( line == null ) return;
            if( !double.TryParse(textRadiuBox.Text,out rad) )            
            {
                MessageBox.Show("Invalid radiu.");
                return;
            }

            if ( rad <= 0 )
            {
                MessageBox.Show("Invalid radiu.");
                return;
            }
            C3DLine line1 = line.Copy();
            line1.Normalize();
            Polygon3D poly = new Polygon3D(line1, rad);
            
            C3DData.AddObject(poly);
            Program.m_MainForm.m_ObjectForm.UpdateTree();
            Program.m_MainForm.m_DDDForm.UpdateDraw();
        }

        
    }
}
