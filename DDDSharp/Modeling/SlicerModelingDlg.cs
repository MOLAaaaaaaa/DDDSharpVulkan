using DataCollection;
using DDDSharp.Boreholes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DDDSharp
{
    public partial class SlicerModelingDlg : Form
    {
        public double xSampleStep = 20; //采样网格x
        public double ySampleStep = 20; //采样网格y

        public double xBkSampleStep = 20; //采样网格x
        public double yBkSampleStep = 20; //采样网格y
        public int extent = 1;
        public bool sampleBoundary = true;
        public bool sampleBoundaryInter = true;
        public bool sampleBoundaryOuter = true;
        float BkValue = 0;    //背景值
        float resetValue = 0; //重设值        
        float redundantFilterRadiu = 1.0f; //(0 - 100)重复点滤波半径
        double minx, maxx, miny, maxy;
        
        double minSlicerWidth = 1E30,  maxSlicerWidth=-1E30;  //剖面最小最大长度
        double minSlicerHeight = 1E30, maxSlicerHeight = -1E30; //剖面最小最大高度

        public List<PolygonSlicer> slicers = new List<PolygonSlicer>();
        bool created = false;
        
        StratumDatas stratums = new StratumDatas();       
        public SlicerModelingDlg()
        {
            InitializeComponent();            
        }
        public void AddSlicer(PolygonSlicer s)
        {
            slicers.Add(s);

            if (s.slicerWidth > maxSlicerWidth) maxSlicerWidth = s.slicerWidth;
            if (s.slicerWidth < minSlicerWidth) minSlicerWidth = s.slicerWidth;
            if (s.slicerHeight > maxSlicerHeight) maxSlicerHeight = s.slicerHeight;
            if (s.slicerHeight < minSlicerHeight) minSlicerHeight = s.slicerHeight;

            xSampleStep = minSlicerWidth / 100;
            ySampleStep = minSlicerHeight / 100;
            xBkSampleStep = xSampleStep / 4;
            yBkSampleStep = ySampleStep / 4;
            UpdateDataRange();

            double step = Math.Sqrt((maxx - minx) * (maxx - minx) + (maxy - miny) * (maxy - miny))/100;
            BoundaryStepTextBox.Text = step.ToString();
        }
        void UpdateDataRange() 
        {
            minx = maxx = 0;
            miny = maxy = 0;
            for(int i=0;i<slicers.Count;i++) 
            {
                PolygonSlicer s = slicers[i];                
                if (i == 0) 
                {
                    minx = s.minx;
                    miny = s.miny;
                    maxx = s.maxx;
                    maxy = s.maxy;
                }
                else 
                {
                    if (s.minx < minx) minx = s.minx;
                    if (s.miny < miny) miny = s.miny;
                    if (s.maxx > maxx) maxx = s.maxx;
                    if (s.maxy > maxy) maxy = s.maxy;
                }
            }
        }        
       
        void CreateStratumsFromSlicers()
        {
            stratums.Clear();
            foreach(PolygonSlicer s in slicers)
            {
                foreach(Polygon2D p in s.tracedGeoObjects.Polygons)                
                {
                    if( !stratums.IsExist( p.Name ) )
                    {
                        StratumData layer = new StratumData(p.Name);
                        layer.Color = p.fillColor;
                        layer.Value = p.PropertyValue;
                        stratums.AddLayer(layer);
                    }
                }
            }            
        }
        
        private void UpdateDataGridview()
        {
           
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            /*
            DataGridViewCheckBoxColumn dtCheck = new DataGridViewCheckBoxColumn();
            dtCheck.DataPropertyName = "check";
            dtCheck.HeaderText = "check";
            dataGridView1.Columns.Add(dtCheck);
            */
            dataGridView1.Columns.Add("No", "ID");
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("Value", "Value");
            dataGridView1.Columns.Add("Color", "Color");
            
            for (int i = 0; i < stratums.Count; i++)
            {
                dataGridView1.Rows.Add();                
                dataGridView1.Rows[i].Cells[0].Value = i + 1;
                dataGridView1.Rows[i].Cells[1].Value = stratums[i].Name;
                dataGridView1.Rows[i].Cells[2].Value = stratums[i].Value;
                dataGridView1.Rows[i].Cells[3].Style.ForeColor = stratums[i].Color;
                dataGridView1.Rows[i].Cells[3].Style.BackColor = stratums[i].Color;                
            }

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }
      
        void UpdateList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < slicers.Count; i++)
            {
                listBox1.Items.Add(slicers[i].Name);
            }
        }
        private void SlicerModelingDlg_Load(object sender, EventArgs e)
        {
            CreateStratumsFromSlicers();
            UpdateDataGridview();
            
            comboBox1.Items.Add("Selected Stratum");
            comboBox1.Items.Add("All Stratums");

            BkStepXTextBox.Text = xSampleStep.ToString();
            BkStepYTextBox.Text = xSampleStep.ToString();  
            RedundantFilterRadiuTextBox.Text = redundantFilterRadiu.ToString();

            BkValueTextBox.Text = "0";
            LayerValueTextBox.Text = "1";
            UpdateList();
        } 
        List<string>GetSelectedLayers()
        {
            List<string>selectedLayers = new List<string>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++) 
            {
                if( dataGridView1.Rows[i].Selected )
                {
                    selectedLayers.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());
                }
            }
            return selectedLayers;
        }
        /// <summary>
        /// 地层采样
        /// </summary>
        /// <param name="sample_all">是否全部地层采样</param>
        /// <param name="sampleborder">是否采样地层边界点</param>
        /// <param name="border_sample_step">地层边界点采样步长</param>
        /// <returns>成功与否</returns>
        bool CreateSamplingGrids(bool sample_all,bool sampleborder,float border_sample_step)
        {  
            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = slicers.Count;
            progressBar1.Step = 1;

            List<string> selectedlayers = new List<string>();
            //获取已选择的地层
            if (!sample_all) selectedlayers = GetSelectedLayers();
            bool sampleBkgound = BackgroundSampleCheck.Checked;
            int count = 0;
            for (int i = 0; i < slicers.Count; i++)
            {
                PolygonSlicer slicer = slicers[i];
                slicer.sampledGrids.Clear();

                DoubleRect rect = slicer.GetTracedGeoObjectsRange(selectedlayers);
                int xgid = (int)(rect.Width / xSampleStep + 0.1 );
                int ygid = (int)(rect.Height / ySampleStep + 0.1);
                int xbksample = (int)(xBkSampleStep / xSampleStep + 0.1);
                int ybksample = (int)(yBkSampleStep / ySampleStep + 0.1);

                slicer.SetLayersPropertyByName(stratums);//重设地层属性值和颜色
                slicer.SampleLayerCoords(selectedlayers, xgid + 1, ygid + 1,
                                         xbksample,
                                         ybksample,
                                              ResetLayerCheckBox.Checked, 
                                              sampleBkgound,
                                              BkValue, resetValue );
                if (sampleborder)//边界采样
                {
                    slicers[i].SampleBoudary(selectedlayers, border_sample_step, 
                                             ResetLayerCheckBox.Checked, 
                                             resetValue );
                }
                //if(FilterCheckBox1.Checked)//重采样过滤
                //slicers[i].ResampleFilter(xSampleGrid, ySampleGrid, xResampleGrid, yResampleGrid);

                if (FilterCheckBox1.Checked)//冗余点过滤
                slicers[i].SampledDuplicatedFilter(redundantFilterRadiu);

                progressBar1.Value = i;
                count += slicer.sampledGrids.Count;
            }// for (int i = 0; i < slicers.Count; i++)

            progressBar1.Visible = false;
            MessageBox.Show("data sampled points : " + count);

            created = true;
            return true;
        }

        bool ExportSlicer(string filename)
        {           
            int err = 0;
            int exported = 0;
            for (int i=0;i<slicers.Count;i++)
            {
                if (slicers[i].sampledGrids.Count > 0)
                { 
                    slicers[i].ExportLayerPropertyToXYZ(filename);
                    exported++;
                }
                else err++;                
            }

            if (err == 0 && exported > 0) MessageBox.Show("data exported.");
            else 
            { 
                if(err > 0 && exported == 0) MessageBox.Show("sampling grids not found .");
                else if (err > 0 && exported > 0) 
                { 
                    MessageBox.Show("exported:" +exported + ", failed:"+ err); 
                }
            }
            return true;
        }
        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "ASCII XYZ (*.dat,*.txt)|*.dat;*.txt|all files(*.*)|*.*";
                dlg.OverwritePrompt = false;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ExportSlicer(dlg.FileName);
                }
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(StepXTextBox.Text, out xSampleStep) ||
                !double.TryParse(StepYTextBox.Text, out xSampleStep) ||
                !double.TryParse(BkStepXTextBox.Text, out xBkSampleStep) ||
                !double.TryParse(BkStepYTextBox.Text, out yBkSampleStep) )
            {
                MessageBox.Show("parameter not correct.");
                return;
            }
            //背景值
            if (!float.TryParse(BkValueTextBox.Text, out BkValue))
            {
                MessageBox.Show("Background value not correct.");
                return;
            }
            //重设地层值
            if (ResetLayerCheckBox.Checked)
            {
                if (!float.TryParse(LayerValueTextBox.Text, out resetValue))
                {
                    MessageBox.Show("Reset layer value not correct.");
                    return;
                }
            }
            float border_samp_step = 0;
            if (SampleBoudaryCheckBox.Checked) 
            {
                if (!float.TryParse(BoundaryStepTextBox.Text, out border_samp_step))
                {
                    MessageBox.Show("Boundary sampling step value not correct.");
                    return;
                }
            }
            if (FilterCheckBox1.Checked)
            {
                if (!float.TryParse(RedundantFilterRadiuTextBox.Text, out redundantFilterRadiu))
                {
                    MessageBox.Show("Redundant filter radiu value not correct.");
                    return;
                }
            }
            

            //获取地层参数，名称，属性值，颜色 --> layes
            if (stratums.Count<1 ) return;         
            if ( comboBox1.SelectedIndex == 0 )//single layer
            {
                //参数获取
                if (dataGridView1.SelectedCells.Count < 1)
                {
                    MessageBox.Show("please select a layer value.");
                    return;
                }
                CreateSamplingGrids(false,SampleBoudaryCheckBox.Checked, border_samp_step);
            }
            else //all layer
            {
                CreateSamplingGrids(true, SampleBoudaryCheckBox.Checked, border_samp_step);
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.StratumColorSchemeFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                if (StratumDatas.ExportStratumScheme(stratums, dlg.FileName))
                {                  
                    MessageBox.Show("Stratum Color Scheme Save to \n" + dlg.FileName);
                }
            }
        }
      
        private void ExportColorScaleButton_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "color level (*.clr)|*.clr|all files(*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                CColorScale scale = stratums.CreateColorScale();
                if (scale.SaveClr(dlg.FileName))
                {
                    MessageBox.Show("Color scale saved to " + dlg.FileName);
                }
                else MessageBox.Show("Faild to save color scale." + scale.errMessage);
            }
        }
        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.StratumColorSchemeFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                stratums.LoadFromStratumScheme(dlg.FileName);
                if (stratums.Count < 1)
                {
                    MessageBox.Show("Stratum Color Scheme Loaded Failed \n" + stratums.errMessage);
                }
                UpdateDataGridview();
            }           
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentCell.ColumnIndex == 3)
            {
                int irow = dataGridView1.CurrentCell.RowIndex;
                Color color = dataGridView1.CurrentCell.Style.ForeColor;
                ColorDialog cd = new ColorDialog();
                cd.Color = color;
                if( cd.ShowDialog() == DialogResult.OK)
                {
                    dataGridView1.Rows[irow].Cells[3].Style.ForeColor = cd.Color;
                    dataGridView1.Rows[irow].Cells[3].Style.BackColor = cd.Color;
                    //dataGridView1.Update();
                }                
            }
        }
        /// <summary>
        /// 设置地层默认值
        /// </summary>
        void SetDefaultResetLayerValue()
        {
            if (!ResetLayerCheckBox.Checked) return;

            if (dataGridView1.CurrentCell == null) return;

            int row = dataGridView1.CurrentCell.RowIndex;
            if (row < 0) return;
            double value = (row + 1) * 10;
            LayerValueTextBox.Text = value.ToString();
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            //SetDefaultResetLayerValue();
        }

        private void LoadSlicersButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Slicers(*.Slicer)|*.Slicer|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        slicers.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            PolygonSlicer slicer = new PolygonSlicer();
                            if (slicer.LoadFrom(dlg.FileNames[i]))
                            {
                                slicers.Add(slicer);
                            }                            
                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
                UpdateList();
                UpdateDataGridview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadSlicersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Slicers(*.Slicer)|*.Slicer|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        slicers.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            PolygonSlicer slicer = new PolygonSlicer();
                            if (slicer.LoadFrom(dlg.FileNames[i]))
                            {
                                slicers.Add(slicer);
                            }
                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
                UpdateDataRange();
                UpdateList();
                UpdateDataGridview();
                double step = Math.Sqrt((maxx - minx) * (maxx - minx) + (maxy - miny) * (maxy - miny)) / 100;
                BoundaryStepTextBox.Text = step.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
