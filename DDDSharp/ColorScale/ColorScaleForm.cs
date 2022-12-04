using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataCollection
{
    public partial class ColorScaleForm : Form
    {
        public float[] data = null;
        public CColorScale colorscale = null;
        public CColorScale nativeColorscale;// = new CColorScale();
        public bool updated = false;
        double minv = 0, maxv = 0;
        public ColorScaleForm()
        {
            InitializeComponent();
        }

        private void ColorScaleForm_Load(object sender, EventArgs e)
        {
            if (colorscale != null)
            {
                nativeColorscale = colorscale.Copy();
                propertyGrid1.SelectedObject = nativeColorscale;
                minv = nativeColorscale.minv;
                maxv = nativeColorscale.maxv;
            }            
        }

        private void OKbutton1_Click(object sender, EventArgs e)
        {
            colorscale = nativeColorscale;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancelbutton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoadFrombutton1_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "color scale (*.clr)|*.clr|color level (*.lvl)|*.lvl|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if( dlg.FilterIndex == 1 )
                    {
                        if (nativeColorscale.LoadClr(dlg.FileName))
                        {
                            propertyGrid1.SelectedObject = nativeColorscale;
                            updated = true;
                        }
                    }
                    else
                    {
                        if (nativeColorscale.LoadLvl(dlg.FileName))
                        {
                            propertyGrid1.SelectedObject = nativeColorscale;
                            updated = true;
                        }
                    }
                    
                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "color level (*.clr)|*.clr|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if(nativeColorscale.SaveClr(dlg.FileName) )                    
                        MessageBox.Show("color map saved to file: \n" + dlg.FileName);
                    else
                        MessageBox.Show("failed to save color map to file: \n" + dlg.FileName);
                    this.Cursor = DefaultCursor;
                }
            }
        }        
        private void ValueDistributionButton_Click(object sender, EventArgs e)
        {
            if (colorscale == null ) return;
            if(data == null)
            {
                data = C3DData.objSelected.toValuesArray();
            }
            
            if (data == null) return;

            CValueDistribution vb = new CValueDistribution();
            vb.data = data;
            if ( vb.ShowDialog() == DialogResult.OK )
            {
                if (nativeColorscale.minv != vb.outMinv || nativeColorscale.maxv != vb.outMaxv)
                {
                    nativeColorscale.SetValueRange(vb.outMinv, vb.outMaxv);
                    propertyGrid1.SelectedObject = nativeColorscale;
                    propertyGrid1.Update();
                    updated = true;
                }
            }
        }

        private void ColorScaleForm_Paint(object sender, PaintEventArgs e)
        {
            if (nativeColorscale != null)
            {
                Cursor = Cursors.WaitCursor;

                Graphics g = colorBar.CreateGraphics();
                Rectangle rect = new Rectangle(0, 0, colorBar.Width, colorBar.Height);
                nativeColorscale.DrawColorBar(g,rect);

                Cursor = Cursors.Default;
            }
        }

        private void Inverse_Click(object sender, EventArgs e)
        {
            if (nativeColorscale != null)
            {
                nativeColorscale.Reverse();
                updated = true;
                colorBar.Invalidate();
            }            
        }

        private void DefaultButton_Click(object sender, EventArgs e)
        {
            nativeColorscale = new CColorScale(minv,maxv);
            updated = true;
            colorBar.Invalidate();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (nativeColorscale != null)
            {
                updated = true;
                colorBar.Invalidate();
            }
        }
    }
}
