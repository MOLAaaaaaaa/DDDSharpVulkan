using DataCollection;
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
using static DataCollection.CBorehole;

namespace DDDSharp.Boreholes
{
    public partial class BoreholesSampleForm : Form
    {
        public CBoreholes boreholes = null;
        StratumDatas Stratums = new StratumDatas();
        int layerSelectedIndex = -1;        
        StratumData curLayer = null;
        List<int>targetIndices = new List<int>();
        double sample_step = 0, bksample_step = 0;
        List<BoreholeSamplePoint> Points = new List<BoreholeSamplePoint>();
        public BoreholesSampleForm(CBoreholes bhs)
        {
            InitializeComponent();
            boreholes = bhs;
            Stratums.AddStratums(boreholes);
            sample_step = Math.Round( (boreholes.Maxz - boreholes.Minz) / 50,2);
            bksample_step = sample_step;
            textBox1.Text = sample_step.ToString();
            textBox2.Text = bksample_step.ToString();
            textBox3.Text = "1";
            textBox4.Text = "0";
            BkSampleCheckBox.Checked = true;
            ResetValueCheckBox.Checked = false;
        }
        void UpdateView()
        {
            listView1.Items.Clear();
            if (Stratums.Count < 1) return;            
            this.listView1.BeginUpdate();
            StratumData layer;
            for (int i = 0; i < Stratums.Count; i++)
            {
                layer = Stratums[i];
                ListViewItem liv = listView1.Items.Add((i + 1).ToString());
                liv.UseItemStyleForSubItems = false;
                liv.SubItems.Add(layer.Name);
                liv.SubItems.Add(layer.Code);
                liv.SubItems[2].BackColor = layer.Color;
                liv.SubItems[2].ForeColor = Color.FromArgb(255, 255 - layer.Color.R, 255 - layer.Color.G, 255 - layer.Color.B);
            }
            this.listView1.EndUpdate();
        }
        void UpdateView(int irow, StratumData layer)
        {
            if (layer == null) return;
            if (irow < 0 || irow >= listView1.Items.Count) return;
            ListViewItem liv = listView1.Items[irow];

            this.listView1.BeginUpdate();

            liv.SubItems[1].Text = layer.Name;
            liv.SubItems[2].Text = layer.Code;
            liv.SubItems[2].BackColor = layer.Color;

            this.listView1.EndUpdate();
        }
        void UpdateList()
        {
            listBox1.Items.Clear();
            for(int i=0;i< targetIndices.Count;i++) 
            {
                int id = targetIndices[i];
                string text = Stratums[id].Name + " value = " + Stratums[id].Value;
                listBox1.Items.Add(text);
            }
        }
        private void BoreholesSampleForm_Load(object sender, EventArgs e)
        {
            Text = "钻孔地层采样" + "--" + boreholes.Name;
            listView1.Columns.Add("序号", 30, HorizontalAlignment.Center);
            listView1.Columns.Add("地层名称", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("地层代号", 100, HorizontalAlignment.Center);
            UpdateView();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            layerSelectedIndex = -1;
            propertyGrid1.SelectedObject = null;
            if (listView1.SelectedIndices.Count > 0)
            {
                layerSelectedIndex = listView1.SelectedIndices[0];
                if (layerSelectedIndex >= 0)
                {
                    curLayer = Stratums[layerSelectedIndex];
                    propertyGrid1.SelectedObject = curLayer;
                }
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

        }
        void DoLayerPropertyChanged(int sel)
        {
            if (sel < 0) return;            
            Stratums[sel] = curLayer;
            UpdateView(sel, curLayer);
            UpdateList();
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (propertyGrid1.SelectedObject == null) return;
            if (layerSelectedIndex < 0) return;

            DoLayerPropertyChanged(layerSelectedIndex);
        }

        private void Addto_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count < 1) return;
            bool update = false;
            for(int i=0;i< listView1.SelectedIndices.Count; i++)
            {
                int sel = listView1.SelectedIndices[i];
                if(targetIndices.IndexOf(sel) < 0 ) 
                {
                    targetIndices.Add(sel);
                    update = true;
                }
            }
            if(update) UpdateList();
        }

        private void SampleButton_Click(object sender, EventArgs e)
        {
            try 
            {
                sample_step = double.Parse(textBox1.Text);
                bksample_step = double.Parse(textBox2.Text);
                double target_value = double.Parse(textBox3.Text);
                double bk_value = double.Parse(textBox4.Text);
                bool reset = ResetValueCheckBox.Checked;
                bool bksample = BkSampleCheckBox.Checked;
                List<StratumData> targetStratums = new List<StratumData>();
                foreach (int id in targetIndices)
                {
                    targetStratums.Add(Stratums[id]);
                }
                Points.Clear();
                for(int k = 0;k<boreholes.Count;k++)                 
                {
                    CBorehole bh = boreholes[k];
                    bh.StrataSampling(Points, k, targetStratums, sample_step, bksample_step, reset, bksample, target_value, bk_value);
                }
                MessageBox.Show(AppLocalization.Translate("采样完成，采样点数：") + Points.Count);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }  
            
        }

        private void Exportbutton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.XYZVFormatLineFilter;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;

                    StreamWriter wr = new StreamWriter(dlg.FileName, true);
                    if (wr.BaseStream.Length < 10) wr.WriteLine("X,Y,Z,VALUE,BoreholeID");

                    for (int i = 0; i < Points.Count; i++)
                    {
                        Vector32 p = Points[i].Point;
                        wr.WriteLine(p.toString(3) + "," + Math.Round(p.V, 0) + "," + Points[i].boreholeID);
                    }
                    wr.Close();

                    Cursor = Cursors.Default;

                    MessageBox.Show(AppLocalization.Translate("数据输出成功！") + Environment.NewLine + dlg.FileName);
                }
            }
        }

        private void RemoveFrom_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            targetIndices.RemoveAt(sel);
            UpdateList();
        }
    }
}
