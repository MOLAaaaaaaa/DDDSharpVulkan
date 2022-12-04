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
    public partial class SlicerDataRangeForm : Form
    {
        public PolygonSlicer slicer;

        public SlicerDataRangeForm(PolygonSlicer _slicer)
        {
            slicer = _slicer;
            InitializeComponent();
            textBox1.Text = slicer.minx.ToString();
            textBox2.Text = slicer.maxx.ToString();
            textBox3.Text = slicer.miny.ToString();
            textBox4.Text = slicer.maxy.ToString();            
            
        }

        private void SlicerDataRangeForm_Load(object sender, EventArgs e)
        {
            string[] names = Enum.GetNames(typeof(AxisEnum));
            comboBox1.Items.AddRange(names);

            for (int i = 0; i < names.Length; i++)
            {
                if (slicer.axis.ToString() == names[i])
                {
                    comboBox1.SelectedIndex = i;
                    break;
                }
            }
            names = null;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            double minx, maxx, miny, maxy;

            if( !double.TryParse(textBox1.Text, out minx) )
            {
                MessageBox.Show("wrong data range.");
                return;
            }
            if (!double.TryParse(textBox2.Text, out maxx))
            {
                MessageBox.Show("wrong data range.");
                return;
            }
            if (!double.TryParse(textBox3.Text, out miny))
            {
                MessageBox.Show("wrong data range.");
                return;
            }
            if (!double.TryParse(textBox4.Text, out maxy))
            {
                MessageBox.Show("wrong data range.");
                return;
            }
            if( minx >= maxx || miny >= maxy )
            {
                MessageBox.Show("wrong data range.");
                return;
            }

            slicer.minx = minx;
            slicer.maxx = maxx;
            slicer.miny = slicer.minz = miny;
            slicer.maxy = slicer.maxz = maxy;

            slicer.axis = (AxisEnum) Enum.Parse(typeof(AxisEnum),comboBox1.Text);

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
