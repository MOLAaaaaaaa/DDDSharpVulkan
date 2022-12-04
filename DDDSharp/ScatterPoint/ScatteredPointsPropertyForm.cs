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
    public partial class ScatteredPointsPropertyForm : DockingPaneExt
    {
        public ScatteredPointsPropertyForm()
        {
            InitializeComponent();
        }
        
        private void ScatteredPointsPropertyForm_Load(object sender, EventArgs e)
        {

        }

        private void Reload_Click(object sender, EventArgs e)
        {
            ScatteredPoints sc = (ScatteredPoints)C3DData.GetSelectedObj( ShapeEnum.Points);
            if (sc == null) return;
            ScatteredPointsLoadForm load = new ScatteredPointsLoadForm(sc);
            if( load.ShowDialog() == DialogResult.OK)
            {
                C3DData.objSelected = sc;
            }
        }

        private void Resample_Click(object sender, EventArgs e)
        {
            ScatteredPoints sc = (ScatteredPoints)C3DData.GetSelectedObj(ShapeEnum.Points);
            if (sc == null) return;
            ScatterPointsResampleForm sample = new ScatterPointsResampleForm(sc);
            if( sample.ShowDialog() == DialogResult.OK )
            {

            }
        }
    }
}
