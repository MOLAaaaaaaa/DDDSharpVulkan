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
    public partial class ImageTrimForm : Form
    {
        List<Polygon2D>polys = new List<Polygon2D>();
        public PolygonSlicer slicer = null;
        public Polygon2D selectedPoly = null;
        public bool keepInner = false;

        public ImageTrimForm(PolygonSlicer _slicer)
        {
            InitializeComponent();
            slicer = _slicer;
        }

        private void ImageTrimForm_Load(object sender, EventArgs e)
        {
            for(int i = 0;i<slicer.tracedGeoObjects.Count;i++) 
            {
                Polygon2D poly = slicer.tracedGeoObjects[i];
                if(poly.IsClosed)
                {
                    listBox1.Items.Add(poly.Name);
                    polys.Add(poly);
                }
            }
            for (int i = 0; i < slicer.polygons.Count; i++)
            {
                Polygon2D poly = slicer.polygons[i];
                if (poly.IsClosed)
                {
                    listBox1.Items.Add(poly.Name);
                    polys.Add(poly);
                }
            }
            radioButton1.Checked = true; 
            radioButton2.Checked = false;
        }
        
        private void OK_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) 
            {
                MessageBox.Show("未选择裁剪多边形！");
                return; 
            }
            selectedPoly = polys[listBox1.SelectedIndex];
            if ( radioButton1.Checked )
                keepInner = false;
            else keepInner = true;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            selectedPoly = null;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
