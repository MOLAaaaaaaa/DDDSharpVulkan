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
    public partial class Export3DGridForm : Form
    {
        public C3DGridData p3D = null;
        public string filename = "";
        public Export3DGridForm()
        {
            InitializeComponent();
            radioButton1.Checked = true;
            radioButton2.Checked = false;
            checkBox1.Checked = false;
            textBox1.Text = "0";
            textBox2.Text = "10";
        }

        private void Export3DGridForm_Load(object sender, EventArgs e)
        {
                       
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (p3D == null) return;

            int nx = p3D.xNum;
            int ny = p3D.yNum;
            int nz = p3D.zNum;
            float value;
            C3DGridData data = new C3DGridData(nx,ny,nz);
            
            //背景值，默认0
            double bkvalue = 0;
            if (checkBox1.Checked) 
            { 
                if( !double.TryParse(textBox1.Text, out bkvalue) )
                    bkvalue = p3D.minv - 100;
            }
            for (long i = 0; i < nx * ny * nz; i++)
            {
                data.pGridData[i] = (float)bkvalue;
            }
            
            int icolor = 0;
            bool reset = checkBox2.Checked;
            float LayerValue = 0;
            float.TryParse(textBox2.Text,out LayerValue);

            long count = 0;
            double minv = 0, maxv = 0;
            for ( long i = 0; i < nx * ny * nz; i++ )
            {
                value = p3D.pGridData[i];
                if (p3D.IsBlankValue(value) || p3D.IsBlankedGrid(i)) continue;
                
                icolor = p3D.GetColorIndex(value);
                if (icolor < 0 || icolor >= p3D.ColorScale.Count) continue;
                if (!p3D.ColorScale[icolor].Visible) continue;

                if(reset) value = LayerValue;

                data[i] = value;

                if (count == 0) minv = maxv = value;
                else
                {
                    if (minv > value ) minv = value;
                    if (maxv < value ) maxv = value;
                }
                count++;
            }

            if (maxv <= minv) minv = maxv - maxv * 0.001;
            data.minv = minv;
            data.maxv = maxv;

            data.minx = p3D.minx;
            data.miny = p3D.miny;
            data.minz = p3D.minz;
            data.maxx = p3D.maxx;
            data.maxy = p3D.maxy;
            data.maxz = p3D.maxz;
            
            if( data.SaveAs(filename) )
            {
                MessageBox.Show("Exported!");
            }
            else
            {
                MessageBox.Show("Failed to export to file.\n" + filename);
            }
            data.Clear();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
