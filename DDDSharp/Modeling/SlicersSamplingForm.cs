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
using DDDSharp.Boreholes;
namespace DDDSharp.Modeling
{
    public partial class SlicersSamplingForm : Form
    {
        List<PolygonSlicer> loadedSlicers = new List<PolygonSlicer>();
        List<PolygonSlicer> Slicers = new List<PolygonSlicer>();
        C3DGridData grid3d = null;
        string errMessage = "";
        double minx, miny, minz, minv;
        double maxx, maxy, maxz, maxv;
        int NX = 101, NY = 101, NZ = 101;
        double stepx, stepy, stepz;
        public SlicersSamplingForm()
        {
            InitializeComponent();
            HideCheckBox.Checked = true;
        }

        void CreateByXOYSlicers(PolygonSlicer slicer1, PolygonSlicer slicer2, C3DGridData grid)
        {
            int z1 = (int)( ( (slicer1.Maxz + slicer1.Minz) * 0.5 - minz ) / stepz + 0.1);
            int z2 = (int)( ( (slicer2.Maxz + slicer2.Minz) * 0.5 - minz ) / stepz + 0.1);
            float[,] grid1 = new float[NX, NY];
            float[,] grid2 = new float[NX, NY];
            C3DData.Stratums.SamplingValuesFromImage(slicer1.BackgroundImage, NX, NY, grid1,1);
            C3DData.Stratums.SamplingValuesFromImage(slicer2.BackgroundImage, NX, NY, grid2,1);
            
            for (int iy = 0; iy < NY; iy++)//设置上下界面
            {
                for (int ix = 0; ix < NX; ix++)
                {
                    grid3d[ix, iy, z1] = grid1[ix, iy];
                    if(z2 > z1) grid3d[ix, iy, z2] = grid2[ix, iy];
                }
            }                        
            for( int iz = z1 + 1; iz < z2; iz++)//插值中间界面
            {
                double scale = (double)(iz - z1) / (z2 - z1);
                for (int iy = 0; iy < NY; iy++)
                { 
                    for (int ix = 0; ix < NX; ix++)
                    {
                        grid3d[ix, iy, iz] = (float)(grid1[ix, iy] + scale * (grid2[ix, iy] - grid1[ix, iy]));
                    } 
                }                        
            }
            grid1 = null;
            grid2 = null;
        }
        void CreateByXOYSlicers()
        {
            grid3d = new C3DGridData(NX,NY,NZ);
            grid3d.ResetDataRange(minx,maxx,miny,maxy,minz,maxz,0,1);

            bool ascOrder = false;
            double dist = Math.Abs(Slicers[0].Minz - minz);
            if(dist <= (maxz-minz)*0.01 ) ascOrder = true; //从低到高
            PolygonSlicer slicer1 = Slicers[0], slicer2, s;
            for(int i = 1; i < Slicers.Count; i++)
            {
                slicer2 = Slicers[i];
                if (ascOrder) CreateByXOYSlicers(slicer1, slicer2, grid3d);
                else CreateByXOYSlicers(slicer2, slicer1, grid3d);
                slicer1 = slicer2;
            }
            grid3d.UpdateRange();
        }
        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (Slicers.Count < 2) 
            {
                MessageBox.Show("No Slicers On List.");
                return; 
            }
            if(C3DData.Stratums.Count < 1)
            {
                MessageBox.Show("No Stratum Scheme Loaded.");
                return;
            }
            if (textOutputFile.Text .Length < 1)
            {
                MessageBox.Show("output file name is missing.");
                return;
            }
            Cursor = Cursors.WaitCursor;
            if ( Slicers[0].axis == AxisEnum.yAxis )
            {
                CreateByXOYSlicers();
                grid3d.SaveAs(textOutputFile.Text);
            }
            else if (Slicers[0].axis == AxisEnum.zAxis)
            {
             //   CreateByXOZSlicers();
            }
            Cursor = Cursors.Default;
            MessageBox.Show("Created!");
        }
        void DoTextStepChanged()
        {

        }

        private void textStepX_TextChanged(object sender, EventArgs e)
        {
            if( double.TryParse(textStepX.Text, out double step) )
            {
                stepx = step;
                NX = (int)((maxx - minx) / stepx + 0.1) + 1;
                textXNum.Text = NX.ToString();
            }
        }

        private void textStepY_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textStepY.Text, out double step))
            {
                stepy = step;
                NY = (int)((maxy - miny) / stepy + 0.1) + 1;
                textYNum.Text = NY.ToString();
            }
        }

        private void textStepZ_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textStepZ.Text, out double step))
            {
                stepz = step;
                NZ = (int)((maxz - minz) / stepz + 0.1) + 1;
                textZNum.Text = NZ.ToString();
            }
        }

        private void textXNum_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textXNum.Text, out int num))
            {
                if (NX > 1)
                {
                    NX = num;
                    stepx = Math.Round( (maxx - minx) / (NX - 1),6);
                    textStepX.Text = stepx.ToString();
                }
            }
        }

        private void textYNum_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textYNum.Text, out int num))
            {
                if (NY > 1)
                {
                    NY = num;
                    stepy = Math.Round((maxy - miny) / (NY - 1), 6);
                    textStepY.Text = stepy.ToString();
                }
            }
        }

        private void textZNum_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textZNum.Text, out int num))
            {
                if (NZ > 1)
                {
                    NZ = num;
                    stepz = Math.Round((maxz - minz) / (NZ - 1), 6);
                    textStepZ.Text = stepz.ToString();
                }
            }
        }

        private void textX1_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textX1.Text, out double val))
            {
                minx = val;
                stepx = Math.Round((maxx - minx) / (NX - 1), 6);
                textStepX.Text = stepx.ToString();
            }
        }

        private void textY1_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textY1.Text, out double val))
            {
                miny = val;
                stepy = Math.Round((maxy - miny) / (NY - 1), 6);
                textStepY.Text = stepy.ToString();
            }
        }

        private void textZ1_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textZ1.Text, out double val))
            {
                minz = val;
                stepz = Math.Round((maxz - minz) / (NZ - 1), 6);
                textStepZ.Text = stepz.ToString();
            }
        }

        private void textX2_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textX2.Text, out double val))
            {
                maxx = val;
                stepx = Math.Round((maxx - minx) / (NX - 1), 6);
                textStepX.Text = stepx.ToString();
            }
        }

        private void textY2_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textY2.Text, out double val))
            {
                maxy = val;
                stepy = Math.Round((maxy - miny) / (NY - 1), 6);
                textStepY.Text = stepy.ToString();
            }
        }

        private void textZ2_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textZ2.Text, out double val))
            {
                maxz = val;
                stepz = Math.Round((maxz - minz) / (NZ - 1), 6);
                textStepZ.Text = stepz.ToString();
            }
        }

        private void LoadColorScale_Click(object sender, EventArgs e)
        {
            GeoLayerEditor dlg = new GeoLayerEditor();
            dlg.stratums = C3DData.Stratums.Copy();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                C3DData.Stratums = dlg.stratums.Copy();
                if (C3DData.Stratums.Count > 0)
                {
                    ColorScaleTextBox.Text = " Stratums Count =  " + C3DData.Stratums.Count;
                }
            }
        }

        private void OutputFileChooseButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.Grid3DFileFormatFilter;
                dlg.Filter += "| All Files(*.*)|*.*";
                if (textOutputFile.Text.Length > 0)
                {
                    dlg.FileName = textOutputFile.Text;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textOutputFile.Text = dlg.FileName;
                }
            }
        }

        void UpdateGemetry()
        {
            textX1.Text = Math.Round(minx,6).ToString();
            textX2.Text = Math.Round(maxx, 6).ToString();
            textY1.Text = Math.Round(miny, 6).ToString();            
            textY2.Text = Math.Round(maxy, 6).ToString();
            textZ1.Text = Math.Round(minz, 6).ToString();
            textZ2.Text = Math.Round(maxz, 6).ToString();
            stepx = (maxx - minx) / (NX - 1);
            stepy = (maxy - miny) / (NY - 1);
            stepz = (maxz - minz) / (NZ - 1);
            textStepX.Text = Math.Round(stepx, 6).ToString();
            textStepY.Text = Math.Round(stepy, 6).ToString();
            textStepZ.Text = Math.Round(stepz, 6).ToString();
            textXNum.Text = NX.ToString();
            textYNum.Text = NY.ToString();
            textZNum.Text = NZ.ToString();
        }
        void UpdateDataRange()
        {
            minx = miny = minz = minv = 0;
            maxx = maxy = maxz = maxv = 0;
            for (int i = 0; i < Slicers.Count; i++)
            {
                PolygonSlicer s = Slicers[i];
                if (i == 0)
                { 
                    minx = s.Minx; 
                    miny = s.Miny;
                    minz = s.Minz;
                    minv = s.Minv;
                    maxx = s.Maxx;
                    maxy = s.Maxy;
                    maxz = s.Maxz;
                    maxv = s.Maxv;
                }
                else
                {
                    if(s.Minx < minx) minx = s.Minx;
                    if(s.Miny < miny) miny = s.Miny;
                    if(s.Minz < minz) minz = s.Minz;
                    if(s.Minv < minv) minv = s.Minv;
                    if(s.Maxx > maxx) maxx = s.Maxx;
                    if(s.Maxy > maxy) maxy = s.Maxy;                    
                    if(s.Maxz > maxz) maxz = s.Maxz;                    
                    if(s.Maxv > maxv) maxv = s.Maxv;
                }
            }
        }
        private void UpdateList1()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < Slicers.Count; i++)
            {
                listBox1.Items.Add(Slicers[i].Name);
            }
            listBox1.SelectedIndex = -1;
        }
        private void SlicersSamplingForm_Load(object sender, EventArgs e)
        {
            List<C3DObjectBase> objects = C3DData.GetObjects();
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].type == ShapeEnum.PolygonSlicer)
                {
                    PolygonSlicer slicer = objects[i] as PolygonSlicer;
                    loadedSlicers.Add(slicer);
                    if (HideCheckBox.Checked) { if (slicer.Visible) Slicers.Add(slicer); }
                    else Slicers.Add(slicer);
                }
            }
            comboBox1.Items.Add("to Grids Values");
            comboBox1.Items.Add("to Stratums Indices");
            comboBox1.SelectedIndex = 0;
            if( C3DData.Stratums.Count > 0 )
            {
                ColorScaleTextBox.Text = " Stratums Count =  " + C3DData.Stratums.Count;
            }
            UpdateDataRange();
            UpdateGemetry();
            UpdateList1();
        }

        bool IsExistedIn(PolygonSlicer slicer, List<PolygonSlicer> _slicers)
        {
            for (int i = 0; i < _slicers.Count; i++)
            {
                if (slicer == _slicers[i]) return true;
            }
            return false;
        }

        private void LoadSlicerButton_Click(object sender, EventArgs e)
        {
            int added = 0;

            List<PolygonSlicer> unselecedslicers = new List<PolygonSlicer>();
            for (int i = 0; i < loadedSlicers.Count; i++)
            {
                PolygonSlicer slicer = loadedSlicers[i];
                if (!IsExistedIn(slicer, Slicers)) unselecedslicers.Add(slicer);
            }

            AddSlicersFromLoadedForm dlg = new AddSlicersFromLoadedForm(unselecedslicers);
            if (dlg.ShowDialog() != DialogResult.OK) return;
            for (int i = 0; i < dlg.selectedSlicers.Count; i++)
            {
                Slicers.Add(dlg.selectedSlicers[i]);
                added++;
            }
            dlg.selectedSlicers.Clear();
            if (added > 0)
            {                
                UpdateList1();
                UpdateDataRange();
                UpdateGemetry();
            }
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            Slicers.RemoveAt(sel);
            UpdateList1();
            UpdateDataRange();
            UpdateGemetry();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            if (Slicers.Count > 1)
            {
                var ret = MessageBox.Show("Are you want to remove all items?", "Remove All Items?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (ret != DialogResult.Yes) return;
            }
            Slicers.Clear();
            listBox1.Items.Clear();            
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel <= 0) return;
            PolygonSlicer cur = Slicers[sel];
            Slicers[sel] = Slicers[sel - 1];
            Slicers[sel - 1] = cur;
            UpdateList1();
            listBox1.SelectedIndex = sel - 1;
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel >= Slicers.Count - 1) return;
            PolygonSlicer cur = Slicers[sel];
            Slicers[sel] = Slicers[sel + 1];
            Slicers[sel + 1] = cur;
            UpdateList1();
            listBox1.SelectedIndex = sel + 1;
        }
    }
}
