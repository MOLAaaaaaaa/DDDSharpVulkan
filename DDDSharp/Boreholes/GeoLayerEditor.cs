using DataCollection;
using DDDSharp.Dialogs;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DDDSharp.Boreholes
{
    public partial class GeoLayerEditor : Form
    {
        Bitmap LastBitmap = null;
        string LoadedFileName = "";        
        bool Modified = false;
        double minDepth, maxDepth;
        PointF cursor = new PointF(-1, -1);
        RectRuler ruler = new RectRuler();
        bool IsControlKeyDown = false;
        int layerSelectedIndex = -1;
        int curveSelectedIndex = -1;
        StratumData curLayer = null;
        public StratumDatas stratums = new StratumDatas();        
        public GeoLayerEditor()
        {
            InitializeComponent();
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
                    curLayer = stratums[layerSelectedIndex];
                    propertyGrid1.SelectedObject = curLayer;                    
                }
            }
        }

       

        private void GeoLayerEditor_Load(object sender, EventArgs e)
        {
            Text = "地层配色方案" + "--" + LoadedFileName;
            listView1.Columns.Add("序号", 30, HorizontalAlignment.Center);
            listView1.Columns.Add("地层名称", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("地层代号", 100, HorizontalAlignment.Center); 
            UpdateView(); 
            propertyGrid1.PropertySort = PropertySort.Categorized;
            
        }
        
        void UpdateView()
        {
            listView1.Items.Clear();
            if (stratums.Count < 1) return;

            this.listView1.BeginUpdate();
            StratumData layer;
            for (int i = 0; i < stratums.Count; i++)
            {
                layer = stratums[i];
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
        void UpdateView(StratumDatas layers)
        {
            if (layers.Count < 1) return;
            this.listView1.BeginUpdate();
            for (int i = 0; i < layers.Count; i++)
            {
                ListViewItem liv = listView1.Items[i];
                liv.SubItems[1].Text = layers[i].Name;
                liv.SubItems[2].Text = layers[i].Code;
                liv.SubItems[2].BackColor = layers[i].Color;
            }
            this.listView1.EndUpdate();
        }
        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void newStrataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StratumData layer = new StratumData();
            Modified = true;
            stratums.AddLayer(layer);
            UpdateView();
            ClearLayerSelected();
            SetLayerSelected(stratums.Count - 1);
        }
        void ClearLayerSelected()
        {
            layerSelectedIndex = -1;
            listView1.SelectedItems.Clear();
            listView1.SelectedIndices.Clear();
        }
        void SetLayerSelected(int index)
        {
            ClearLayerSelected();
            if (index >= 0) 
            { 
                listView1.SelectedIndices.Add(index); 
                curLayer = stratums[index];
                propertyGrid1.SelectedObject = curLayer;
            }
        }
        void DoLayerPropertyChanged(int sel)
        {
            if (sel < 0) return;
//            borehole.Stratums.UpdateTopDepth(sel + 1);            
            Modified = true;
            stratums[sel] = curLayer;
            UpdateView(sel, curLayer);
            //UpdateView(stratums);            
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (propertyGrid1.SelectedObject == null) return;
            if (layerSelectedIndex < 0) return;

            DoLayerPropertyChanged(layerSelectedIndex);
        }

        private void insertBeforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = layerSelectedIndex;
            if (sel < 0) return;
            Modified = true;
            StratumData layer = new StratumData();
            stratums.InsertLayer(sel, layer);            
            UpdateView();
            ClearLayerSelected();
            SetLayerSelected(sel);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = -1;
            if (listView1.SelectedIndices.Count > 0)
            {
                sel = listView1.SelectedIndices[0];
                if (sel >= 0 && sel < stratums.Count)
                {
                    DialogResult ret = MessageBox.Show("Remove " + stratums[sel].Name, "Remove Stratum?",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button2);

                    if (ret == DialogResult.Yes)
                    {
                        stratums.RemoveLayer(sel);
                        UpdateView();
                        listView1.SelectedIndices.Clear();
                        if (sel >= 0 && sel < stratums.Count)
                            listView1.SelectedIndices.Add(sel);
                        Modified = true;
                    }

                }
            }
        }
        
        
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.StratumColorSchemeFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                if( StratumDatas.ExportStratumScheme(stratums,dlg.FileName) )
                {
                    Modified = false;
                    MessageBox.Show("Stratum Color Scheme Save to \n" + dlg.FileName);
                }
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if( Modified ) 
            {
                if (MessageBox.Show("color scheme is modified, work have done will be lost!!!",
                    "Continue any way?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign) == DialogResult.No) return;
            }

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.StratumColorSchemeFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                
                stratums = new StratumDatas();
                stratums.LoadFromStratumScheme(dlg.FileName);
                if (stratums.Count < 1 )
                {
                    MessageBox.Show("Stratum Color Scheme Loaded Failed \n" + stratums.errMessage);
                }
                
                Modified = false;

                UpdateView();
            }
        }
        void DoColorPick()
        {
            int sel = -1;
            if (listView1.SelectedIndices.Count > 0)
            {
                sel = listView1.SelectedIndices[0];
                if (sel < 0 || sel >= stratums.Count) return;
            }

            ColorAndTextPickerForm dlg = new ColorAndTextPickerForm(LastBitmap);
            dlg.Layer = curLayer.Copy();

            if ( dlg.ShowDialog() == DialogResult.OK && dlg.Modified)
            {
                StratumData layer = dlg.Layer;
                curLayer = layer.Copy();                
                stratums[sel] = layer;
                propertyGrid1.SelectedObject = stratums[sel];
                DoLayerPropertyChanged(layerSelectedIndex);

                Modified = true;
                if (dlg.m_Bmp != null)
                {
                    if (LastBitmap != null) LastBitmap.Dispose();
                    LastBitmap = new Bitmap(dlg.m_Bmp);
                    dlg.m_Bmp.Dispose();
                }
            }
        }
        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

        }

        private void ColorPickerButton_Click(object sender, EventArgs e)
        {
            DoColorPick();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DoColorPick();
        }

        private void exportColorLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "color level (*.clr)|*.clr|all files(*.*)|*.*";
            if( dlg.ShowDialog() == DialogResult.OK)
            {
                CColorScale scale = stratums.CreateColorScale();
                if (scale.SaveClr(dlg.FileName))
                {
                    MessageBox.Show("Color scale saved to " + dlg.FileName);
                }
                else MessageBox.Show("Faild to save color scale." + scale.errMessage);
            }
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            if (Modified)
            {
                DialogResult ret = MessageBox.Show("data has been modified, abort it anyway?", "changes ignored?",
                                                     MessageBoxButtons.YesNoCancel,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button3);
                if (ret == DialogResult.Cancel || ret == DialogResult.No) { return; }
            }
            Modified = false;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
