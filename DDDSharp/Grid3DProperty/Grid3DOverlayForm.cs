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

namespace DDDSharp.Grid3DProperty
{
    public partial class Grid3DOverlayForm : Form
    {
        public C3DGridData grid3d = null;
        List<C3DGridData>grids = new List<C3DGridData>();
        List<C3DGridOverlayProperty>Properties = new List<C3DGridOverlayProperty>();
        public Grid3DOverlayForm(C3DGridData _data)
        {
            InitializeComponent();
            grid3d = _data;
        }
        void UpdateList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < grids.Count; i++) 
            {
                listBox1.Items.Add(grids[i].Name);
            }
        }
        private void Grid3DOverlayForm_Load(object sender, EventArgs e)
        {
            grids.Clear();
            propertyGrid1.SelectedObject = grid3d;
        }

        private void LoadFromButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = Resource1.Grid3DFileFormatFilter;
                    dlg.Filter += "|" + "All Files(*.*)|*.*";
                    dlg.Multiselect = true;                    
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;                        
                        
                        Properties.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            C3DGridData data = new C3DGridData();
                            if( data.LoadFrom(dlg.FileNames[i]))
                            {
                                C3DGridOverlayProperty p = new C3DGridOverlayProperty();
                                p._gridInformation = data.Information;
                                p.MinimumValue = data.minv;
                                p.MaximumValue = data.maxv;
                                Properties.Add(p);
                                grids.Add(data);
                            }
                        }
                        UpdateList();
                        this.Cursor = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex >= 0) 
            {
                propertyGrid2.SelectedObject = Properties[listBox1.SelectedIndex];
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            for (int i = 0; i < grids.Count; i++) 
            {
                C3DGridData data = grids[i];
                C3DGridOverlayProperty property = Properties[i];
                double v = 0;
                for(int j = 0; j < data.pGridData.Length;j++)
                {
                    v = data.pGridData[j];
                    if ( data.IsBlankValue(v) ) continue;
                    if (property.EnableFilter &&
                       (v < property.MinimumValue || v > property.MaximumValue)) continue;
                    if (property.IsRestvalue) v = property.Restvalue;
                    if (property.SetAsInteger) 
                    {
                        if (v < 0) v = (int)(v - 0.1);
                        else v = (int)(v + 0.1);
                    }                    
                    data.GetXYZIndexFromIndex(j, out int ix, out int iy, out int iz);
                    Vector32 p1 = data.GetVerticCoord(ix, iy, iz,0);
                    Int32XYZ xyz = grid3d.GetVerticIndexByPosition(p1.X,p1.Y,p1.Z);
                    if (property.SetAsInteger) grid3d[xyz.x, xyz.y, xyz.z] = (int)v;
                    else grid3d[xyz.x, xyz.y, xyz.z] = (float)v;
                }
            }
            grid3d.UpdateRange();
            grid3d.ColorScale.SetValueRange(grid3d.minv, grid3d.maxv);
            Cursor = Cursors.Default;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < grids.Count; i++) 
            {
                grids[i].Clear();
            }
            grids.Clear();
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public class C3DGridOverlayProperty
    {
        public string _gridInformation = "";
        [CategoryAttribute("Filter"), DisplayNameAttribute("Grid Information")]
        public string gridInformation { get { return _gridInformation; } }
        [CategoryAttribute("Filter"), DisplayNameAttribute("Enable")]
        public bool EnableFilter { get; set; } = true;
        [CategoryAttribute("Filter"), DisplayNameAttribute("Minimum Value")]
        public double MinimumValue { get; set; } = 0;
        [CategoryAttribute("Filter"), DisplayNameAttribute("Maximum Value")]
        public double MaximumValue { get; set; } = 0;
        [CategoryAttribute("Output"), DisplayNameAttribute("Reset Value")]
        public bool IsRestvalue { get; set; } = true;
        [CategoryAttribute("Output"), DisplayNameAttribute("Reset As")]
        public double Restvalue { get; set; } = 14;
        [CategoryAttribute("Output"), DisplayNameAttribute("Set As Integer")]
        public bool SetAsInteger { get; set; } = true;

    }
}
