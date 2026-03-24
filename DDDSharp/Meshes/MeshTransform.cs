using DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DDDSharp.Meshes
{
    public partial class MeshTransform : Form
    {
        public CMesh mesh = null;
        double minx,maxx,miny,maxy,minz,maxz;

        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SaveAs_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.SurferGridFileFormatFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                if(mesh.ExportToGrid2D(dlg.FileName))
                {
                    MessageBox.Show("Saved!!!");
                }
                else 
                {
                    MessageBox.Show(mesh.errMessage);
                }
            }
            
        }

        public bool Modified = false;
        public bool rangeUpdated = false;
        private void ApplyButton2_Click(object sender, EventArgs e)
        {
            double x1 = minx, x2 = maxx;
            double y1 = miny, y2 = maxy;
            double z1 = minz, z2 = maxz;
            
            try
            {
                if (XCheck.Checked)
                {
                    x1 = double.Parse(XtextBox1.Text);
                    x2 = double.Parse(XtextBox2.Text);
                    Modified = true;
                    if (x1 != minx || x2 != maxx) rangeUpdated = true;
                }
                if (YCheck.Checked)
                {
                    y1 = double.Parse(YtextBox1.Text);
                    y2 = double.Parse(YtextBox2.Text);
                    Modified = true;
                    if (y1 != miny || y2 != maxy) rangeUpdated = true;
                }
                if (ZCheck.Checked)
                {
                    z1 = double.Parse(ZtextBox1.Text);
                    z2 = double.Parse(ZtextBox2.Text);
                    Modified = true;
                    if (z1 != minz || z2 != maxz) rangeUpdated = true;
                }
                if (Modified) 
                {
                    mesh.ResetDataRange(x1, x2, y1, y2, z1, z2);
                    MessageBox.Show("Done!!!"); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        public MeshTransform(CMesh _mesh)
        {
            mesh = _mesh;
            InitializeComponent();
        }

        private void MeshTransform_Load(object sender, EventArgs e)
        {
            if (mesh != null)
            {
                minx = mesh.minx;
                miny = mesh.miny;
                minz = mesh.minz;
                maxx = mesh.maxx;
                maxy = mesh.maxy;
                maxz = mesh.maxz;                
                XtextBox1.Text = minx.ToString();
                XtextBox2.Text = maxx.ToString();
                YtextBox1.Text = miny.ToString();
                YtextBox2.Text = maxy.ToString();
                ZtextBox1.Text = minz.ToString();
                ZtextBox2.Text = maxz.ToString();                
            }
        }
    }
}
