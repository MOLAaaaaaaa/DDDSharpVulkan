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
    
    public partial class GridsOverlapForm : Form
    {
        struct LayerModelStruct
        {
            public string file;
            public bool reset;
            public double value;
            public double minv;
            public double maxv;
            public LayerModelStruct(string _file,bool _reset,double _value,double _minv = 0, double _maxv = 0)
            {
                file = _file;
                reset = _reset;
                value = _value;
                minv = _minv;
                maxv = _maxv;
            }
        }

        List<LayerModelStruct> files = new List<LayerModelStruct>();
        C3DGridData data = null;
        public GridsOverlapForm()
        {
            InitializeComponent();            
            FillBackgroundCheckBox.Checked = true;            
            FillBackGridValueTextBox.Text = "0";  
            ResetOutputRangeCheckBox.Checked = true;
            OutputValueTextBox1.Text = "0";
            OutputValueTextBox2.Text = "100";
        }
        private void GridsOverlapForm_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("replace");
            comboBox1.Items.Add("average");
            comboBox1.SelectedIndex = 0;
        }
        void UpdateList()
        {
            listBox1.Items.Clear();
            string name;
            for(int i=0;i<files.Count;i++)
            {
                name = Path.GetFileName(files[i].file);
                if( files[i].reset )
                {
                    name += "----" + files[i].value;
                }
                listBox1.Items.Add(name);
            }
            listBox1.SelectedIndex = -1;
        }
        
        void UpdateList(int i)
        {
            if (i < 0 || i > listBox1.Items.Count) return;
            string name = Path.GetFileName(files[i].file);
            name += "----" + files[i].value;
            listBox1.Items[i] = name;
        }

        private void LoadGridButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "3DGrid(*.3DGrid)|*.3DGrid|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            C3DGridData data = new C3DGridData();
                            if (data.LoadFrom(dlg.FileNames[i]))
                            { 
                                files.Add(new LayerModelStruct(dlg.FileNames[i], false, data.maxv,data.minv,data.maxv)); 
                            }
                            data.Clear();
                        }
                        
                        this.Cursor = Cursors.Default;

                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MergeButton_Click(object sender, EventArgs e)
        {
            if( files.Count < 2) 
            {
                MessageBox.Show(" no enough grid data loaded.");
                return;
            }
            
            float bkvalue = 0;  
            if (!float.TryParse(FillBackGridValueTextBox.Text, out bkvalue))
            {
                MessageBox.Show("Invalid Filled Background Value.");
                return;
            }

            if (data != null) 
            { 
                data.Clear();
                data = null;
            }

            data = new C3DGridData();            
            //第1层
            if ( !data.LoadFrom(files[0].file) )
            {
                MessageBox.Show("Load grid data failed.\n",files[0].file);
                return;
            }
            
            this.Cursor = Cursors.WaitCursor;

            //背景填充
            for (long id = 0; id < data.pGridData.Length; id++)
            {
                if (data.IsBlankValue(data.pGridData[id]))
                {
                    data.pGridData[id] = bkvalue;
                }
            }            
            
            //叠加
            long count = 0;
            int gridmethod = comboBox1.SelectedIndex; //0-- replace,1--average
            for(int i = 1; i < files.Count; i++ )
            {
                C3DGridData data1 = new C3DGridData();
                if ( data1.LoadFrom(files[i].file) )
                {
                    count += C3DGridData.LayerOverlapedOnLocation(data, data1, gridmethod, files[i].reset,files[i].value);
                }
                data1.Clear();
            }//叠加完成

            //没有填充的网格，是否用周围的值填充            
            if (FillBackgroundCheckBox.Checked)
            {
                double v1;
                //无效网格替换
                for (int iz = 0; iz < data.zNum; iz++)
                    for (int iy = 0; iy < data.yNum; iy++)
                        for (int ix = 0; ix < data.xNum; ix++)
                        {
                            v1 = data.GetGridValue(ix, iy, iz);
                            if (v1 == bkvalue)
                            {
                                if (data.SearchNearestValue(ix, iy, iz, out v1, bkvalue))
                                {
                                    data.SetGridValue(ix, iy, iz, v1);
                                }
                            }
                        }
            }            
            
            //update value range
            if ( count > 0 )
            {
                data.UpdateDataRange();
                OutputValueTextBox1.Text = data.minv.ToString();
                OutputValueTextBox2.Text = data.maxv.ToString();
                MessageBox.Show("Overlaped " + count + " grids.");
            }

            this.Cursor = Cursors.Default;

        }

        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            if( data == null )
            {
                MessageBox.Show(" no merged grid.");
                return;
            }
            double minv = 0, maxv = 100;
            if (ResetOutputRangeCheckBox.Checked)//重置值范围
            {
                if( !double.TryParse(OutputValueTextBox1.Text,out minv) )
                {
                    MessageBox.Show("invalid minimum value.");
                    return;
                }
                if (!double.TryParse(OutputValueTextBox2.Text, out maxv))
                {
                    MessageBox.Show("invalid maximum value.");
                    return;
                }
                if ( minv >= maxv )
                {
                    MessageBox.Show("invalid minimum and maximum value.");
                    return;
                }
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "3DGrid (*.3DGrid)|*.3DGrid|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    if(ResetOutputRangeCheckBox.Checked)
                    {
                        data.minv = minv;
                        data.maxv = maxv;
                    }
                    if (data.SaveAs(dlg.FileName))
                        MessageBox.Show("data saved to file: \n" + dlg.FileName);
                    else
                        MessageBox.Show("failed to save to file: \n" + dlg.FileName);

                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel <= 0) return;
            LayerModelStruct cur = files[sel];
            files[sel] = files[sel-1];
            files[sel - 1] = cur;
            UpdateList();
            listBox1.SelectedIndex = sel - 1;
        }
        void UpdateSelectedLayerState()
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= files.Count) return;
            LayerModelStruct layer = files[sel];
            LayerMintextBox1.Text = layer.minv.ToString();
            LayerMaxtextBox1.Text = layer.maxv.ToString();
            EnableLayerValueCheckBox1.Checked = layer.reset;
            LayerValuetextBox1.Text = layer.value.ToString();
            if (layer.reset) LayerValuetextBox1.Enabled = true;
            else LayerValuetextBox1.Enabled = false;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedLayerState();
        }
        
        private void DownButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= files.Count ) return;

            LayerModelStruct cur = files[sel];
            files[sel] = files[sel + 1];
            files[sel + 1] = cur;
            UpdateList();
            listBox1.SelectedIndex = sel + 1;
        }      
      
        private void UpdateLayerButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= files.Count) return;
            double value;
            if ( !double.TryParse(LayerValuetextBox1.Text, out value) )
            {
                MessageBox.Show("Invalid Layer Value.");
                return;
            }
            double minv, maxv;
            if (!double.TryParse(LayerMintextBox1.Text, out minv))
            {
                MessageBox.Show("Invalid minimum Value.");
                return;
            }
            if (!double.TryParse(LayerMaxtextBox1.Text, out maxv))
            {
                MessageBox.Show("Invalid maximum Value.");
                return;
            }
            LayerModelStruct layer = files[sel];
            layer.value = value;
            layer.minv = minv;
            layer.maxv = maxv;
            layer.reset = EnableLayerValueCheckBox1.Checked;
            files[sel] = layer;
            UpdateList(sel);
        }

        private void EnableLayerValueCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= files.Count) return;
            LayerModelStruct layer = files[sel];            
            layer.reset = EnableLayerValueCheckBox1.Checked;
            files[sel] = layer;
            if (layer.reset) LayerValuetextBox1.Enabled = true;
            else LayerValuetextBox1.Enabled = false;
        }
        //save selected layer to grid3d
        private void SaveLayerbutton1_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= files.Count) 
            {
                MessageBox.Show("no grid data selected.");
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "3DGrid (*.3DGrid)|*.3DGrid|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    LayerModelStruct layer = files[sel];
                    C3DGridData data1 = new C3DGridData();
                    if( !data1.LoadFrom(layer.file) )
                    {
                        MessageBox.Show("load data error." + data1.errMessage);
                    }
                    else
                    {
                        data1.minv = layer.minv;
                        data1.maxv = layer.maxv;
                        if (data1.SaveAs(dlg.FileName))
                            MessageBox.Show("data saved to file: \n" + dlg.FileName);
                        else
                            MessageBox.Show("failed to save to file: \n" + dlg.FileName);
                    }                   

                    this.Cursor = DefaultCursor;
                }
            }
        }

    }
}
