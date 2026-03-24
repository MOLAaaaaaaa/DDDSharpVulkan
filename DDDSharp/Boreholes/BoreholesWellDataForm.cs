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
namespace DDDSharp.Boreholes
{
    public partial class BoreholesWellDataForm : Form
    {
        public CBoreholes boreholes = new CBoreholes();
        bool IsGeoCoord = false;
        int maxLayers = 10;
        bool modified = false;

        public BoreholesWellDataForm(CBoreholes bhs = null)
        {
            InitializeComponent();
            if (bhs == null) boreholes = new CBoreholes();
            else boreholes = bhs.Copy();
        }
       
        public bool fromGridview(CBorehole bh, int i)
        {
            string name = dataGridView1.Rows[i].Cells[0].Value.ToString().Trim();
            if (name.Length < 1) return false;
            
            double depth, thickness;
            bh.Name = name;
            string sx = dataGridView1.Rows[i].Cells[1].Value.ToString().Trim();
            string sy = dataGridView1.Rows[i].Cells[2].Value.ToString().Trim();
            string sz = dataGridView1.Rows[i].Cells[3].Value.ToString().Trim();
            
            Vector64 p0 = new Vector64();
            if (!double.TryParse(sx, out p0.X)) return false;
            if (!double.TryParse(sy, out p0.Y)) return false;
            if (!double.TryParse(sz, out p0.Z)) return false;

            bh.Position = p0;
            for (int j = 0; j < (dataGridView1.Columns.Count - 4) / 3; j += 3)
            {
                name = dataGridView1.Rows[i].Cells[4 + j].Value.ToString().Trim();
                string top = dataGridView1.Rows[i].Cells[4 + j + 1].Value.ToString().Trim();
                string thick = dataGridView1.Rows[i].Cells[4 + j + 2].Value.ToString().Trim();
                if (name.Length < 1) break;

                StratumData layer = new StratumData(name);
                if (!double.TryParse(top, out depth)) layer.TopDepth = 0;
                if (!double.TryParse(thick, out thickness)) layer.Thickness = 0;
                layer.TopDepth = depth;
                layer.Thickness = thickness;
                bh.Stratums.AddLayer(layer, false);
            }
            return true;
        }
        /// <summary>
        /// 从表格中获取数据到boreholes
        /// </summary>
        public bool fromGridview()
        {
            string name,sx,sy,sz,top,thick;
            Vector64 p0 = new Vector64();
            double depth, thickness;
            boreholes.Clear();

            String errinfo = "";
            for ( int i = 0; i < dataGridView1.Rows.Count; i++ )
            {
                name = dataGridView1.Rows[i].Cells[0].Value.ToString().Trim();
                if (name.Length < 1) 
                { 
                    errinfo += "line " + (i+1) + ": invalid borehole name.\n"; 
                    continue; 
                }
                CBorehole bh = new CBorehole(name);
                sx = dataGridView1.Rows[i].Cells[1].Value.ToString().Trim();
                sy = dataGridView1.Rows[i].Cells[2].Value.ToString().Trim();
                sz = dataGridView1.Rows[i].Cells[3].Value.ToString().Trim();

                if ( !double.TryParse(sx, out p0.X) || 
                     !double.TryParse(sy, out p0.Y) ||
                     !double.TryParse(sz, out p0.Z) )
                {
                    errinfo += "line " + (i + 1) + ": invalid borehole coordinate.\n";
                    continue;
                }

                bh.Position = p0;
                
                for (int j = 0; j < (dataGridView1.Columns.Count - 4)/3; j+=3 )
                {
                    name = dataGridView1.Rows[i].Cells[4 + j].Value.ToString().Trim();
                    top = dataGridView1.Rows[i].Cells[4 +  j + 1].Value.ToString().Trim();
                    thick = dataGridView1.Rows[i].Cells[4 + j + 2].Value.ToString().Trim();
                    if ( name.Length < 1 ) continue;

                    StratumData layer = new StratumData(name);
                    
                    if (!double.TryParse(top, out depth)) 
                    {
                        errinfo += "line " + (i + 1) + " " + layer.Name + ": Invalid top depth parameters.\n";
                        continue;
                    }
                    if ( !double.TryParse(thick, out thickness)) 
                    {
                        errinfo += "line " + (i + 1) + " " + layer.Name + ": Invalid thickness parameters.\n";
                        continue;
                    }
                    layer.TopDepth = depth;
                    layer.Thickness = thickness;
                    if( layer.IsValid() ) bh.Stratums.AddLayer(layer,false);
                    else
                    {
                        errinfo += "line " + (i + 1) + " "+layer.Name + ": invalid stratum parameters.\n";
                        continue;
                    }                    
                }                
                bh.CreateBaseLine(IsGeoCoord);
                bh.UpdateRange();
                boreholes.AddBorehole(bh);
            }
            boreholes.ShowCylinder = false;
            boreholes.UpdateRange();
            if( errinfo.Length > 1 )
            {
                MessageBox.Show(errinfo, "errors occurred");
                return false;
            }
            
            modified = false;

            return true;
        }

        /// 从表格中获取数据到boreholes        
        public void UpdateDataGridview()
        {
            Cursor = Cursors.WaitCursor;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();            
            
            dataGridView1.Columns.Add("钻孔编号", "钻孔编号");
            dataGridView1.Columns.Add("孔口坐标X(m)", "孔口坐标X(m)");
            dataGridView1.Columns.Add("孔口坐标Y(m)", "孔口坐标Y(m)");
            dataGridView1.Columns.Add("孔口标高(m)", "孔口标高(m)");
            for ( int i = 1; i <= maxLayers; i++ )
            {
                dataGridView1.Columns.Add("层名" + i, "层名"+i);
                dataGridView1.Columns.Add("顶深(m)" + i, "顶深(m)" + i);
                dataGridView1.Columns.Add("层厚(m)" + i, "层厚(m)" + i);
            }
            for (int i = 0; i < boreholes.Count; i++)                
            {
                dataGridView1.Rows.Add();

                CBorehole bh = boreholes[i];
                Vector64 p0 = bh.Position;

                dataGridView1.Rows[i].Cells[0].Value = bh.Name;
                dataGridView1.Rows[i].Cells[1].Value = p0.X.ToString();
                dataGridView1.Rows[i].Cells[2].Value = p0.Y.ToString();
                dataGridView1.Rows[i].Cells[3].Value = p0.Z.ToString();

                for (int j = 0; j < maxLayers; j++ )
                {
                    if (j < bh.Stratums.Count)
                    {
                        StratumData layer = bh.Stratums[j];
                        dataGridView1.Rows[i].Cells[4 + 3 * j].Value = layer.Name;
                        dataGridView1.Rows[i].Cells[4 + 3 * j + 1].Value = layer.TopDepth.ToString();
                        dataGridView1.Rows[i].Cells[4 + 3 * j + 2].Value = layer.Thickness.ToString();
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[4 + 3 * j].Value = "";
                        dataGridView1.Rows[i].Cells[4 + 3 * j + 1].Value = "";
                        dataGridView1.Rows[i].Cells[4 + 3 * j + 2].Value = "";
                    }
                }
            }           
            
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            //dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 8, FontStyle.Regular);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            Cursor = Cursors.Default;
        }
        public void SetDataGridview(int irow, CBorehole bh)
        {
            if (irow < 0 || irow >= dataGridView1.Rows.Count) return;
            
            Cursor = Cursors.WaitCursor;

            Vector64 p0 = bh.Position;

            dataGridView1.Rows[irow].Cells[0].Value = bh.Name;
            dataGridView1.Rows[irow].Cells[1].Value = p0.X.ToString();
            dataGridView1.Rows[irow].Cells[2].Value = p0.Y.ToString();
            dataGridView1.Rows[irow].Cells[3].Value = p0.Z.ToString();

            for (int j = 0; j < maxLayers; j++)
            {
                if (j < bh.Stratums.Count)
                {
                    StratumData layer = bh.Stratums[j];
                    dataGridView1.Rows[irow].Cells[4 + 3 * j].Value = layer.Name;
                    dataGridView1.Rows[irow].Cells[4 + 3 * j + 1].Value = layer.TopDepth.ToString();
                    dataGridView1.Rows[irow].Cells[4 + 3 * j + 2].Value = layer.Thickness.ToString();
                }
                else
                {
                    dataGridView1.Rows[irow].Cells[4 + 3 * j].Value = "";
                    dataGridView1.Rows[irow].Cells[4 + 3 * j + 1].Value = "";
                    dataGridView1.Rows[irow].Cells[4 + 3 * j + 2].Value = "";
                }
            }
            Cursor = Cursors.Default;
        }
        private void WellDataForm_Load(object sender, EventArgs e)
        {
            UpdateDataGridview();
        }

        private void newLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();
            int index = dataGridView1.Rows.Count - 1;
            CBorehole bh = new CBorehole();
            SetDataGridview(index, bh);
            boreholes.AddBorehole(bh);
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[0];
            modified = true;
        }

        private void traceAnglesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if( dataGridView1.CurrentRow != null )
            {
                int sel = dataGridView1.CurrentRow.Index;
                if( sel >=0 && sel < boreholes.Count )
                {
                    CBorehole bh = boreholes[sel].Copy();
                    BoreholeInclineForm wf = new BoreholeInclineForm( bh);
                    if( wf.ShowDialog()== DialogResult.OK )
                    {
                        CBorehole bh1 = boreholes[sel];
                        bh1.boreholeAngles.Clear();
                        bh1.boreholeAngles.Angles.AddRange(wf.borehole.boreholeAngles.Angles);
                        boreholes[sel] = bh1;
                        modified = true;
                    }
                }
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int sel = dataGridView1.CurrentRow.Index;
                if (sel >= 0 && sel < boreholes.Count)
                {
                    CBorehole bh = boreholes[sel];
                    fromGridview(bh,sel);
                    modified = true;
                }
            }
        }

        private void addColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maxLayers += 10;
            UpdateDataGridview();
        }
        private void OK_Click(object sender, EventArgs e)
        {
            if ( modified )
            { 
                if (!fromGridview()) return; 
            }
            DialogResult = DialogResult.OK;
            this.Close();            
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
       
        private void importFromExelFileMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.WellStratumDataFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if( !boreholes.ImportStratumsData(dlg.FileName) )
                    {
                        MessageBox.Show(boreholes.errMessage);
                    }
                    else
                    {
                        UpdateDataGridview();
                    }
                    this.Cursor = Cursors.Default;
                }
            }
                    
        }

        private void fromMeshesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fromGridview();
            if( boreholes.Count < 1 )
            {
                MessageBox.Show("No valid boreholes.");
                return;
            }

            
            ImportFromMeshesForm dlg = new ImportFromMeshesForm();
            if( dlg.ShowDialog() == DialogResult.OK )
            {
                Cursor = Cursors.WaitCursor;
                for ( int i = 0; i < boreholes.Count; i++ )
                {
                    CBorehole bh = boreholes[i];
                    bh.CreateStrataFromMeshes(dlg.Meshes,dlg.IsTopFace,dlg.IsZDepth);
                    boreholes[i] = bh;
                }
                Cursor = Cursors.Default;
                UpdateDataGridview();
            }
            
        }
    }
}
