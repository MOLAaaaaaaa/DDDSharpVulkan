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
        public C3DObjectBase curObj = null;
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
            curObj = C3DData.objSelected;
            if (curObj == null) return false;
            else return true;
        }
        public void ReplaceCurSelectedObject()
        {
            C3DData.objSelected = curObj;            
        }
        public void UpdateDataGridview()
        {   
            if (curObj == null ) return;

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            CMesh mesh = (CMesh)curObj;
            CColorScale colorScale = mesh.ColorScale;

            DataGridViewCheckBoxColumn dtCheck = new DataGridViewCheckBoxColumn();
            dtCheck.DataPropertyName = "check";
            dtCheck.HeaderText = "";
            dataGridView1.Columns.Add(dtCheck);            
           
            dataGridView1.Columns.Add("No", "No");
            dataGridView1.Columns.Add("Value", "Value");
            dataGridView1.Columns.Add("Color", "Color");

            for (int i = 0; i < colorScale.Count; i++)
            {
                dataGridView1.Rows.Add();
                //dataGridView1.Rows[i].Cells[0] = new DataGridViewCheckBoxCell();
                dataGridView1.Rows[i].Cells[0].Value = colorScale[i].Visible;
                dataGridView1.Rows[i].Cells[1].Value = i + 1;
                dataGridView1.Rows[i].Cells[2].Value = colorScale.GetScaledValue(i) ;
                dataGridView1.Rows[i].Cells[3].Value = "";
                dataGridView1.Rows[i].Cells[3].Style.ForeColor = colorScale.GetColor(i);
                dataGridView1.Rows[i].Cells[3].Style.BackColor = colorScale.GetColor(i);
                
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
                Invalidate();
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
            
            CMesh mesh = (CMesh)curObj;

            int n = dataGridView1.Rows.Count;
            if (n < 1) return;

            this.Cursor = Cursors.WaitCursor;
            DataGridViewCheckBoxCell cell;
            for (int i = 0; i < n; i++)
            {
                cell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells[0];
                mesh.ColorScale.SetVisible(i,(bool)cell.FormattedValue );
            }            

            string info = "create contour...";
            Program.m_MainForm.AddtoInfo(info);

            this.Cursor = Cursors.WaitCursor;

            mesh.DoMarchingCube();
            ReplaceCurSelectedObject();

            //create new object
            if( checkBox1.Checked )
            {
               C3DLine[]lines = mesh.marchingCube.to3DLine();
                for (int i = 0; i < lines.Length; i++)
                    C3DData.AddObject(lines[i],false);
                
                Program.m_MainForm.m_ObjectForm.UpdateTree();
            }

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
                if (cell == null) continue;
                cell.Value = true;
            }
        }

        private void UnselectAllbutton_Click(object sender, EventArgs e)
        {
            int n = dataGridView1.Rows.Count;
            DataGridViewCheckBoxCell cell;
            for (int i = 0; i < n; i++)
            {
                cell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells[0];
                if (cell == null) continue;
                cell.Value = false;
            }
        }
        /// <summary>
        /// 曲面与面相交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntersectionLineButton_Click(object sender, EventArgs e)
        {
            int n = 0;

            this.Cursor = Cursors.WaitCursor;

            List<C3DObjectBase> objects = C3DData.GetObjects();

            for (int i=0;i< objects.Count;i++)
            {
                if ( objects[i].type == ShapeEnum.Mesh ||
                     objects[i].type == ShapeEnum.Slicer )
                {
                    if (C3DData.objSelected == objects[i]) continue;//slicer self

                    C3DLine[] lines = null;
                    
                    if ( curObj.type == ShapeEnum.Slicer )
                    {
                        CSlicer slicer = (CSlicer)curObj;
                        if (objects[i].type == ShapeEnum.Mesh)
                        {
                            lines = slicer.CreateIntersectionLines((CMesh)objects[i]);
                        }
                        else if (objects[i].type == ShapeEnum.Slicer)
                        {
                            lines = slicer.CreateIntersectionLines((CSlicer)objects[i]);
                        }
                    }                    
                    else if (objects[i].type == ShapeEnum.Mesh)
                    {
                        CMesh mesh = (CMesh)curObj;
                        if (objects[i].type == ShapeEnum.Slicer)
                        {
                            CSlicer slicer = (CSlicer)curObj;
                            lines = slicer.CreateIntersectionLines(mesh);
                        }                        
                    }

                    if (lines != null)
                    {
                        for (int k = 0; k < lines.Length; k++)
                        {
                            C3DData.AddObject(lines[k]);
                            n++;
                        }
                    }
                }
            }

            this.Cursor = DefaultCursor;

            if (n > 0)
            {
                Program.m_MainForm.m_ObjectForm.UpdateTree();
                Program.m_MainForm.m_DDDForm.UpdateDraw();
            }            
        }
        private void EditColorScale()
        {
            CMesh mesh = (CMesh)curObj;
            if (mesh == null) return;

            ColorScaleForm cm = new ColorScaleForm();
            cm.colorscale = mesh.ColorScale.Copy();

            if (cm.ShowDialog() == DialogResult.OK && cm.updated )
            {
                mesh.ColorScale = cm.colorscale.Copy();
                ReplaceCurSelectedObject();
                UpdateDataGridview();
                Program.m_MainForm.m_DDDForm.UpdateDraw();
            }
        }
        private void ColorBarBox_DoubleClick(object sender, EventArgs e)
        {
            EditColorScale();
        }

        private void CSlicerPropertyForm_Paint(object sender, PaintEventArgs e)
        {
            CMesh mesh = (CMesh)curObj;
            if (mesh == null) return;
            Graphics g = ColorBarBox.CreateGraphics();
            Rectangle rect = new Rectangle(0, 0, ColorBarBox.Width, ColorBarBox.Height);
            mesh.ColorScale.DrawColorBar(g, rect);
        }
       
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CMesh mesh = (CMesh)curObj;
            if (mesh == null) return;
            if (dataGridView1.CurrentCell.ColumnIndex != 3) return;
            CColorScale colorScale = mesh.ColorScale;

            Color color = dataGridView1.CurrentCell.Style.BackColor;
            ColorDialog cd = new ColorDialog();
            cd.Color = color;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                color = cd.Color;
                int id = dataGridView1.CurrentCell.RowIndex;
                colorScale.SetColor(id, color);
                dataGridView1.CurrentCell.Style.BackColor = color;
                dataGridView1.CurrentCell.Style.ForeColor = color;
                mesh.RenderMode = RenderingUpdateMode.Redraw;
                Program.m_MainForm.UpdateDraw(mesh);
            }
        }
    }
}
