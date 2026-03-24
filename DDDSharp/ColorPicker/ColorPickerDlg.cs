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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DDDSharp.ColorPicker
{
    public partial class ColorPickerDlg : Form
    {
        public List<Color> colors = null;
        Color curColor = Color.White;
        SlicerModelingForm pParent = null;
        public ColorPickerDlg(SlicerModelingForm parent)
        {
            InitializeComponent();            
            pParent = parent;
        }
        public ColorPickerDlg(List<Color> _colors)
        {
            InitializeComponent();
            colors = _colors;
        }
        public void UpdateView()
        {
            listView1.Items.Clear();
            if (colors.Count < 1) return;

            this.listView1.BeginUpdate();
            
            for (int i = 0; i < colors.Count; i++)
            {
                Color c = colors[i];
                ListViewItem liv = listView1.Items.Add((i + 1).ToString());
                liv.UseItemStyleForSubItems = false;
                liv.SubItems.Add("    ");                
                liv.SubItems[1].BackColor = c;
                liv.SubItems[1].ForeColor = Color.FromArgb(255, 255 - c.R, 255 - c.G, 255 - c.B);
            }
            this.listView1.EndUpdate();
        }

        private void ColorPickerDlg_Load(object sender, EventArgs e)
        {
            Text = "颜色拾取器";
            listView1.Columns.Add("序号", 30, HorizontalAlignment.Center);
            listView1.Columns.Add("颜色", 100, HorizontalAlignment.Center);
            UpdateView();
            propertyGrid1.PropertySort = PropertySort.Categorized;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listView1.SelectedIndices.Count;
            if (n < 1) return;
            int sel = listView1.SelectedIndices[0];
            curColor = colors[sel];
            propertyGrid1.SelectedObject = curColor;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            int n = listView1.SelectedIndices.Count;
            if (n < 0) return;
            int sel = listView1.SelectedIndices[0];            
            colors[sel] = curColor;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            pParent.ConfirmColorPicked();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            pParent.AbortColorPicked();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            pParent.SetMouseState(MouseState.ColorPickerOnPoint);
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            int n = listView1.SelectedIndices.Count;
            if (n < 0) return;
            int sel = listView1.SelectedIndices[0];
            colors.RemoveAt(sel);
            UpdateView();
        }
    }
}
