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

namespace DDDSharp.Meshes
{
    public partial class CreateMeshBufferForm : Form
    {
        public CMesh mesh = null;
        public CreateMeshBufferForm(CMesh _mesh)
        {
            InitializeComponent();

            mesh = _mesh;

            textBox2.Text = "0.0";
            textBox3.Text = "1.0";
            textBox4.Text = "10";
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (mesh == null) return;

            double size = 0, v1 = 0, v2 = 0;
            int num = 100;
            if( !double.TryParse(textBox1.Text, out size) )
            {
                MessageBox.Show("Invalide buffer size.");
                return;
            }
            if (!double.TryParse(textBox2.Text, out v1))
            {
                MessageBox.Show("Invalide values.");
                return;
            }
            if (!double.TryParse(textBox3.Text, out v2))
            {
                MessageBox.Show("Invalide values.");
                return;
            }
            if (!int.TryParse(textBox4.Text, out num))
            {
                MessageBox.Show("Invalide sample numbers.");
                return;
            }

            ScatteredPoints sc = new ScatteredPoints(mesh.CreateBuffer(size, num, v1, v2));
            sc.Name = mesh.Name + "_buffers";
            sc.SetColorRange(v1,v2);
            sc.xWidth = 0.001f;
            sc.yWidth = 0.001f;
            sc.zWidth = 0.001f;

            C3DData.AddObject(sc, false);

            DialogResult = DialogResult.OK;

            this.Close();
        }

        private void CreateMeshBufferForm_Load(object sender, EventArgs e)
        {
            MeshInfotextBox.Text = "X Width: " + (mesh.Maxx - mesh.Minx) + "\r\n";
            MeshInfotextBox.Text += "Y Width: " + (mesh.Maxy - mesh.Miny) + "\r\n";
            MeshInfotextBox.Text += "Z Width: " + (mesh.Maxz - mesh.Minz);
            double size = mesh.MaxWidth;
            if (size > 0)
            {
                size = size / 4;
                textBox1.Text = size.ToString();
            }
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
