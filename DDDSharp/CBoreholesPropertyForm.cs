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
namespace DDDSharp
{
    public partial class CBoreholesPropertyForm : DockingPaneExt
    {
        public CBoreholes boreholes = null;
        public CBoreholesPropertyForm()
        {
            InitializeComponent();
        }
        public Color ConvertTo(ColorRGBA color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public bool GetCurSelectedObject()
        {
            boreholes = (CBoreholes)C3DData.GetSelectedObj(ShapeEnum.Boreholes);
            if (boreholes == null) return false;
            else return true;
        }       
        private void UpdateDataGridview()
        {
            if (boreholes == null) return;

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
                        
            dataGridView1.Columns.Add("check", "");
            dataGridView1.Columns.Add("No", "ID");
            dataGridView1.Columns.Add("Name", "Value");
            CBorehole bh;
            for (int i = 0; i < boreholes.pData.Count; i++)
            {
                bh = boreholes.pData[i];
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0] = new DataGridViewCheckBoxCell();
                dataGridView1.Rows[i].Cells[1].Value = i + 1;
                dataGridView1.Rows[i].Cells[2].Value = bh.Name;
                //dataGridView1.Rows[i].Cells[3].Style.ForeColor = ConvertTo(colorScale.GetColor(i));
                dataGridView1.Rows[i].Cells[0].Value = bh.Visible;
            }
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            //dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 8, FontStyle.Regular);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }
        public void UpdateSelect()
        {
            if (GetCurSelectedObject())
            {
                this.Cursor = Cursors.WaitCursor;
                UpdateDataGridview();
                this.Cursor = DefaultCursor;
            }
        }
        private void SelectAllbutton_Click(object sender, EventArgs e)
        {
            int n = dataGridView1.Rows.Count;
            DataGridViewCheckBoxCell cell;
            for (int i = 0; i < n; i++)
            {
                cell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells[0];
                cell.Value = true;
            }
        }
        private void UnselectAllbutton_Click(object sender, EventArgs e)
        {
            int n = dataGridView1.Rows.Count;
            //DataGridViewCheckBoxCell cell;
            for (int i = 0; i < n; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = false;
                //cell.Value = 0;
            }
        }

        private void CBoreholesPropertyForm_Load(object sender, EventArgs e)
        {
            UpdateSelect();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (!GetCurSelectedObject()) return;

            int n = dataGridView1.Rows.Count;
            if (n < 1) return;

            this.Cursor = Cursors.WaitCursor;
            DataGridViewCheckBoxCell cell;
            CBorehole bh;
            bool show;
            bool updated = false;
            for (int i = 0; i < n; i++)
            {
                bh = boreholes.pData[i];

                cell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells[0];
                show = (bool)cell.FormattedValue;

                if (bh.Visible != show)
                {
                    bh.Visible = show;
                    boreholes.pData[i] = bh;
                    updated = true;
                }                
            }
            if (updated)
            {                
                Program.m_MainForm.UpdateDraw(C3DData.objSelected);
            }
            this.Cursor = DefaultCursor;
        }
    }
}
