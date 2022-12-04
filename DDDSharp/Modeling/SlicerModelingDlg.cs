using System;
using System.IO;
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
    public partial class SlicerModelingDlg : Form
    {
        public int xGridOuter = 100;
        public int yGridOuter = 100;
        public int XResampleExt = 2;
        public int YResampleExt = 2;
        public int extent = 1;
        public bool sampleBoundary = true;
        public bool sampleBoundaryInter = true;
        public bool sampleBoundaryOuter = true;

        public List<PolygonSlicer> slicers = new List<PolygonSlicer>();
        bool created = false;

        List<LayerProperty> layers = new List<LayerProperty>();
        List<string> Selectedlayers = new List<string>();
        
        public SlicerModelingDlg()
        {
            InitializeComponent();            
        }
        public void AddSlicer(PolygonSlicer s)
        {
            slicers.Add(s);
        }
        bool IsInList(string name)
        {
            foreach(LayerProperty s in layers)
            {   
                if (s.LayerName.ToLower() == name.ToLower() ) return true;
            }
            return false;
        }
        void SearchLayerValues()
        {
            layers.Clear();
            PolygonSlicer s;
            Polygon2D p;
            for(int i=0;i<slicers.Count;i++)
            {
                s = slicers[i];
                for(int j=0; j <s.tracedGeoObjects.Count;j++)
                {
                    p = s.tracedGeoObjects[j];
                    if( !IsInList( p.Name ) )
                    {
                        LayerProperty layer = new LayerProperty(p.Name, p.PropertyValue);
                        layer.LayerColor = p.fillColor;
                        layers.Add(layer);
                    }
                }
            }
            //layers.Sort();
        }
        int GetSelectedLayers()
        {
            Selectedlayers.Clear();

            if (dataGridView1.Rows.Count < 1) return 0;
            bool[] marks = new bool[dataGridView1.Rows.Count];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                marks[i] = false;

            int id = 0;
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                id = dataGridView1.SelectedCells[i].RowIndex;
                if ( !marks[id] )
                { 
                    Selectedlayers.Add( dataGridView1.Rows[id].Cells[1].Value.ToString().ToLower().Trim() );
                    marks[id] = true;
                }
            }
            return Selectedlayers.Count;
        }
        private void UpdateDataGridview()
        {
            SearchLayerValues();
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
            
            for (int i = 0; i < layers.Count; i++)
            {
                dataGridView1.Rows.Add();                
                dataGridView1.Rows[i].Cells[0].Value = i + 1;
                dataGridView1.Rows[i].Cells[1].Value = layers[i].LayerName;
                dataGridView1.Rows[i].Cells[2].Value = layers[i].LayerValue;
                dataGridView1.Rows[i].Cells[3].Style.ForeColor = layers[i].LayerColor;
                dataGridView1.Rows[i].Cells[3].Style.BackColor = layers[i].LayerColor;                
            }

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }
        bool GetLayersFromGridView()
        {
            string name;
            double value;
            Color color;
            layers.Clear();
            try 
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    name = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    double.TryParse(dataGridView1.Rows[i].Cells[2].Value.ToString(), out value);
                    color = dataGridView1.Rows[i].Cells[3].Style.ForeColor;
                    LayerProperty layer = new LayerProperty(name, value);
                    layer.LayerColor = color;
                    layers.Add(layer);
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
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
            UpdateDataGridview();
            comboBox1.Items.Add("Selected Layers");
            comboBox1.Items.Add("All Layers");
            comboBox1.SelectedIndex = 0;

            textBox1.Text = xGridOuter.ToString();
            textBox2.Text = yGridOuter.ToString();
            
            textBox3.Text = XResampleExt.ToString();
            textBox4.Text = YResampleExt.ToString();
            
            BkValueTextBox.Text = "0";
            LayerValueTextBox.Text = "10";
        }        

        //sampling all layers
        bool CreateSampling1()
        {
            if (!int.TryParse(textBox1.Text, out xGridOuter))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }
            if (!int.TryParse(textBox2.Text, out yGridOuter))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }
            
            if (!int.TryParse(textBox3.Text, out XResampleExt))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }
            if (!int.TryParse(textBox4.Text, out YResampleExt))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }

            //获取地层参数，名称，属性值，颜色
            if (!GetLayersFromGridView()) return false;

            xGridOuter++;
            yGridOuter++;
            
            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = slicers.Count;
            progressBar1.Step = 1;

            for (int i = 0; i < slicers.Count; i++)
            {
                slicers[i].SetLayersPropertyByName(layers);//重设地层属性值和颜色
                slicers[i].SampleLayerCoords(xGridOuter, yGridOuter, XResampleExt, YResampleExt);
                progressBar1.Value = i;
            }

            progressBar1.Visible = false;
            MessageBox.Show("data sampled.");

            created = true;
            return true;
        }

        //单个地层采样
        bool CreateSampling2()
        {
            if( dataGridView1.SelectedCells.Count < 1 )
            {
                MessageBox.Show("please select a layer value.");
                return false;
            }
            if (!int.TryParse(textBox1.Text, out xGridOuter))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }
            if (!int.TryParse(textBox2.Text, out yGridOuter))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }

            if (!int.TryParse(textBox3.Text, out XResampleExt))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }
            if (!int.TryParse(textBox4.Text, out YResampleExt))
            {
                MessageBox.Show("parameter not correct.");
                return false;
            }
            float BkValue = 0;
            if (!float.TryParse(BkValueTextBox.Text, out BkValue))
            {
                MessageBox.Show("Background value not correct.");
                return false;
            }

            float resetValue = 0;
            if( ResetLayerCheckBox.Checked )
            {                
                if (!float.TryParse(LayerValueTextBox.Text, out resetValue))
                {
                    MessageBox.Show("Reset layer value not correct.");
                    return false;
                }
            }

            //获取地层参数，名称，属性值，颜色
            if (!GetLayersFromGridView()) return false;
           
            xGridOuter++;
            yGridOuter++;          

            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = slicers.Count;
            progressBar1.Step = 1;

            //获取已选择的地层
            GetSelectedLayers();
            
            for (int i = 0; i < slicers.Count; i++)
            {
                slicers[i].SetLayersPropertyByName(layers);//重设地层属性值和颜色
                slicers[i].SampleLayerCoords( Selectedlayers,xGridOuter, yGridOuter,
                                              ResetLayerCheckBox.Checked, BkValue, resetValue );                
                if (SampleBoudaryCheckBox.Checked)
                {
                    double xx = slicers[i].XWidth / (xGridOuter - 1);
                    double yy = slicers[i].YWidth / (yGridOuter - 1);
                    double zz = slicers[i].ZWidth / (yGridOuter - 1);

                    double step = Math.Sqrt(xx*xx+yy*yy) /40 ;

                    slicers[i].SampleBoudary(Selectedlayers, step, ResetLayerCheckBox.Checked, resetValue); 
                }
                slicers[i].ResampleGrids(xGridOuter, yGridOuter,XResampleExt,YResampleExt);

                progressBar1.Value = i;
            }

            progressBar1.Visible = false;
            MessageBox.Show("data sampled.");

            created = true;
            return true;
        }

        bool ExportSlicer(string filename)
        {
            if (!created) return false;
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
            if (comboBox1.SelectedIndex < 0) 
            {
                MessageBox.Show("Please Choose Layers.");
                return; 
            }
            if ( comboBox1.SelectedIndex == 0 ) CreateSampling2();//single layer
            if ( comboBox1.SelectedIndex == 1 ) CreateSampling1();//all layer           
        }

        private void OK_Click(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!GetLayersFromGridView()) return;
            
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Layer Property File (*.LPF)|*.LPF|all files(*.*)|*.*";
                dlg.OverwritePrompt = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if( !LayerProperty.Export(dlg.FileName,layers) )
                    {
                        MessageBox.Show("Failed to save Layer Property Values.");
                    }
                    else
                    {
                        ExportColorScale(dlg.FileName+".clr");
                        MessageBox.Show("Saved to File Successfully\n." + dlg.FileName);
                    }
                }
            }
        }
        bool ExportColorScale(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);

                string header = "ColorMap 1 1";
                wr.WriteLine(header);
                string line = "";

                double minvalue = 0, maxvalue = 0;
                // 找出值范围		
                for (int i = 0; i < layers.Count; i++)
                {
                    if (i == 0) minvalue = maxvalue = layers[i].LayerValue;
                    else
                    {
                        if (minvalue > layers[i].LayerValue) minvalue = layers[i].LayerValue;
                        if (maxvalue < layers[i].LayerValue) maxvalue = layers[i].LayerValue;
                    }
                }
                //写入clr色标文件
                double percent = 0;
                foreach (LayerProperty layer in layers)
                {
                    if (maxvalue > minvalue)
                        percent = 100 * (layer.LayerValue - minvalue) / (maxvalue - minvalue);
                    else percent = 0;

                    line = "	" + percent + " ";
	                line += layer.LayerColor.R + " ";
                    line += layer.LayerColor.G + " ";
                    line += layer.LayerColor.B;
                    wr.WriteLine(line);
                }

                wr.Close();
                fs.Close();
                return true;
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                return false;
            }
        }

        private void ExportColorScaleButton_Click(object sender, EventArgs e)
        {
            if (!GetLayersFromGridView()) return;
            using ( var dlg = new SaveFileDialog() )
            {
                dlg.Filter = "Color Scale (*.clr)|*.clr|all files(*.*)|*.*";
                dlg.OverwritePrompt = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if ( !ExportColorScale( dlg.FileName ) )
                    {
                        MessageBox.Show("Failed to export color scales.");
                    }
                    else
                    {
                        MessageBox.Show("Exported color scale successfully\n." + dlg.FileName);
                    }
                }
            }
        }
        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Layer Property File (*.LPF)|*.LPF|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    List<LayerProperty>lists = LayerProperty.Import(dlg.FileName);                    
                    if(lists.Count < 1 )
                    {
                        MessageBox.Show("Load failed,nothing changed.");
                        return;
                    }
                    layers = lists;
                    UpdateDataGridview();
                    this.Cursor = Cursors.Default;
                }
            }
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
    }
}
