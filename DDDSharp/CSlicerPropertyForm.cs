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
    public partial class CSlicerPropertyForm : DockingPaneExt
    {
        public CSlicer slicer = null;
        public CSlicerPropertyForm()
        {
            InitializeComponent();
        }
        public Color ConvertTo(ColorRGBA color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public bool GetCurSelectedObject()
        {
            slicer = (CSlicer)C3DData.GetSelectedObj(ShapeEnum.Slicer);
            if (slicer == null) return false;
            else return true;
        }
        public void ReplaceCurSelectedObject()
        {
            int sel = C3DData.curSel;
            if (sel < 0) return;
            C3DData.pObjects[sel] = slicer;            
        }
        private void UpdateDataGridview()
        {   
            if ( slicer == null ) return;

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            CColorScale colorScale = slicer.colorScale;

            dataGridView1.Columns.Add("check", "");
            dataGridView1.Columns.Add("No", "ID");
            dataGridView1.Columns.Add("Value", "Value");
            dataGridView1.Columns.Add("Color", "Color");

            for (int i = 0; i < colorScale.nColorNum; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0] = new DataGridViewCheckBoxCell();
                dataGridView1.Rows[i].Cells[1].Value = i + 1;
                dataGridView1.Rows[i].Cells[2].Value = colorScale.pValue[i];
                dataGridView1.Rows[i].Cells[3].Style.ForeColor = ConvertTo(colorScale.GetColor(i));
                dataGridView1.Rows[i].Cells[3].Style.BackColor = ConvertTo(colorScale.GetColor(i));
                dataGridView1.Rows[i].Cells[0].Value = colorScale.pShowTable[i];
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
        private void CSlicerPropertyForm_Load(object sender, EventArgs e)
        {
            UpdateSelect();
        }

        private void ISOLineCreateButton_Click(object sender, EventArgs e)
        {
            if (!GetCurSelectedObject()) return;

            int n = dataGridView1.Rows.Count;
            if (n < 1) return;

            this.Cursor = Cursors.WaitCursor;
            DataGridViewCheckBoxCell cell;
            for (int i = 0; i < n; i++)
            {
                cell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells[0];
                slicer.colorScale.pShowTable[i] = (bool)cell.FormattedValue;
            }            

            string info = "create contour...";
            Program.m_MainForm.AddtoInfo(info);

            this.Cursor = Cursors.WaitCursor;

            slicer.DoMarchingCube();
            ReplaceCurSelectedObject();
            Program.m_MainForm.m_DDDForm.UpdateDraw();

            this.Cursor = DefaultCursor;

            info = "contour created!";
            Program.m_MainForm.AddtoInfo(info);
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
    }
}
