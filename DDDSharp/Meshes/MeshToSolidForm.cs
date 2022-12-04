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
    public partial class MeshToSolidForm : Form
    {

        CMesh mesh = null;
        public MeshToSolidForm(CMesh _mesh)
        {
            InitializeComponent();
            mesh = _mesh;
        }

        private void MeshToSolidForm_Load(object sender, EventArgs e)
        {
            MeshInfotextBox.Text =  "X Width: " + (mesh.Maxx - mesh.Minx) + "\r\n";
            MeshInfotextBox.Text += "Y Width: " + (mesh.Maxy - mesh.Miny) + "\r\n";
            MeshInfotextBox.Text += "Z Width: " + (mesh.Maxz - mesh.Minz);
            double depth = mesh.MinWidth;
            if ( depth > 0 )
            {
                depth = depth / 20;
                textBox1.Text = depth.ToString();
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            double depth = 0;
            if( !double.TryParse(textBox1.Text, out depth) )
            {
                MessageBox.Show("Depth is invalid.");
                return;
            }
            if ( depth <= 0 )
            {
                MessageBox.Show("Depth is invalid.");
                return;
            }
            
            TriangleObj obj = mesh.toTriangleObj(depth);
            obj.Name = mesh.Name + "_solid";
            obj.uniformColor = mesh.ObjColor;
            obj.IsUniformColor = true;
            C3DData.AddObject(obj, false);

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
