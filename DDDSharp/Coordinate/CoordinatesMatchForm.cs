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
        
    public partial class CoordinatesMatchForm : Form
    {
        public AscIIColumn asc1 = new AscIIColumn();
        public AscIIColumn asc2 = new AscIIColumn();        
        
        public string coordsFile = "";

        int select1 = -1, select2 = -1;
        int zsel1 = -1;
        int xsel = -1, ysel = -1, zsel = -1;

        int PageNum = 500;
        int curPage = 0;
        int TotalPage = 0;
        bool Matched = false;

        public CoordinatesMatchForm()
        {
            InitializeComponent();
        }
        void InitSelect1()
        {
            int n = asc1.Titles.Count;
            if (Matched) n = asc1.Titles.Count - 3;

            MatchColumnComboBox1.Items.Clear();            
            for (int i = 0; i < n; i++ )
            {
                MatchColumnComboBox1.Items.Add(asc1.Titles[i]);
            }
            MatchColumnComboBox1.SelectedIndex = -1;

            ComboBoxZ1.Items.Clear();
            for (int i = 0; i < n; i++)
            {
                ComboBoxZ1.Items.Add(asc1.Titles[i]);
            }
            ComboBoxZ1.SelectedIndex = -1;

            DataTypeComboBox.Items.Clear();
            DataTypeComboBox.Items.Add("Profile");
            DataTypeComboBox.Items.Add("Points");            
            DataTypeComboBox.SelectedIndex = 0;

            DoElevationComboBox.Items.Clear();
            DoElevationComboBox.Items.Add("Top");
            DoElevationComboBox.Items.Add("Bottom");
            DoElevationComboBox.Items.Add("Replace");
            DoElevationComboBox.Items.Add("NoChange");
            DoElevationComboBox.SelectedIndex = 3;

            MatchMethodCombox1.Items.Clear();
            MatchMethodCombox1.Items.Add("By Distance");
            MatchMethodCombox1.Items.Add("By Order");
            MatchMethodCombox1.Items.Add("By Name");
            MatchMethodCombox1.SelectedIndex = 0;
        }
        void InitSelect2()
        {
            XComboBox.Items.Clear();
            YComboBox.Items.Clear();
            ZComboBox.Items.Clear();

            MatchColumnComboBox2.Items.Clear();
            for (int i = 0; i < asc2.Titles.Count; i++)
            {
                XComboBox.Items.Add(asc2.Titles[i]);
                YComboBox.Items.Add(asc2.Titles[i]);
                ZComboBox.Items.Add(asc2.Titles[i]);
                MatchColumnComboBox2.Items.Add(asc2.Titles[i]);
            }

            XComboBox.SelectedIndex = -1;
            YComboBox.SelectedIndex = -1;
            ZComboBox.SelectedIndex = -1;            
            MatchColumnComboBox2.SelectedIndex = -1;

            string ss1;
            for (int i = 0; i < asc2.Titles.Count; i++)
            {
                ss1 = asc2.Titles[i].Trim().ToLower();

                if (XComboBox.SelectedIndex < 0 &&
                    (ss1.Contains("x") || ss1.Contains("north")))
                { 
                    XComboBox.SelectedIndex = i;
                    continue;
                }

                if (YComboBox.SelectedIndex < 0 &&
                    (ss1.Contains("y") || ss1.Contains("east")))
                { 
                    YComboBox.SelectedIndex = i;
                    continue;
                }

                if (ZComboBox.SelectedIndex < 0 &&
                     (ss1.Contains("z") || ss1.Contains("深度") ||
                       ss1.Contains("depth") || ss1.Contains("高程") ||
                        ss1.Contains("海拔") || ss1.Contains("eleva")))
                { 
                    ZComboBox.SelectedIndex = i;
                    continue;
                }
            }         

        }

        private void CoordinatesMatchForm_Load(object sender, EventArgs e)
        {            
            InitListViewHeader1();
            InitListViewHeader2();

            InitSelect1();
            InitSelect2();

            UpdateListView1();
            UpdateListView2();
        }
        void InitListViewHeader1()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            if ( asc1.TotalRows < 1) return;
            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].Width = 60;
            //dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;            
            dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250, 248, 232);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Chocolate;
            dataGridView1.AllowUserToResizeColumns = true;

            for (int i = 0; i < asc1.Titles.Count; i++)
            {
                dataGridView1.Columns.Add(asc1.Titles[i], asc1.Titles[i]);
                dataGridView1.Columns[i + 1].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView1.Columns[i + 1].Width = 100;
                // dataGridView1.Columns[i+1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                dataGridView1.Columns[i + 1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dataGridView1.Columns[i].HeaderCell.Style.BackColor = Color.Chocolate;
                //dataGridView1.Columns[i].Resizable = DataGridViewTriState.True;                
            }
            //dataGridView1.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//行的默认模式居中显示
            //dataGridView1.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;            
        }
        void InitListViewHeader2()
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();

            if (asc2.TotalRows < 1) return;

            dataGridView2.Columns.Add("ID", "ID");
            dataGridView2.Columns[0].Width = 60;
            dataGridView2.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250, 248, 232);
            dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = Color.Chocolate;
            dataGridView2.AllowUserToResizeColumns = true;

            for (int i = 0; i < asc2.Titles.Count; i++)
            {
                dataGridView2.Columns.Add(asc2.Titles[i], asc2.Titles[i]);
                dataGridView2.Columns[i + 1].Width = 100;
                dataGridView2.Columns[i + 1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView2.Columns[i + 1].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            //dataGridView2.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;                    
        }

        private void MatchColumnComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //clear last selection
            if (select1 >= 0) dataGridView2.Columns[select1 + 1].Selected = false;

            //new selection
            select1 = MatchColumnComboBox2.SelectedIndex;
            if (select1 >= 0 )
            {
                dataGridView2.Columns[select1 + 1].Selected = true;
            }            
        }
        private void MatchColumnComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //clear last selection
            if (select2 >= 0) dataGridView1.Columns[select2 + 1].Selected = false;

            //new selection
            select2 = MatchColumnComboBox1.SelectedIndex;
            if (select2 >= 0)
            {
                dataGridView1.Columns[select2 + 1].Selected = true;
            }
        }
        private void XComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //clear last selection
            if (xsel >= 0) dataGridView2.Columns[xsel + 1].Selected = false;

            //new selection
            xsel = XComboBox.SelectedIndex;
            if (xsel >= 0)
            {
                dataGridView2.Columns[xsel + 1].Selected = true;
            }
        }
        private void YComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //clear last selection
            if (ysel >= 0) dataGridView2.Columns[ysel + 1].Selected = false;

            //new selection
            ysel = YComboBox.SelectedIndex;
            if (ysel >= 0)
            {
                dataGridView2.Columns[ysel + 1].Selected = true;
            }
        }
        private void ZComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //clear last selection
            if (zsel >= 0) dataGridView2.Columns[zsel + 1].Selected = false;

            //new selection
            zsel = ZComboBox.SelectedIndex;
            if (zsel >= 0)
            {
                dataGridView2.Columns[zsel + 1].Selected = true;
            }
        }

        List<Vector32> GetCoordinatesFromView()
        {
            List<Vector32> Coordinates = new List<Vector32>();
            try 
            {                
                stringRow row;
                float x, y, z, dist;
                select2 = MatchColumnComboBox2.SelectedIndex;
                for (int i = 0; i < asc2.TotalRows; i++)
                {                    
                    row = asc2.GetRow(i);
                    if (row.Count < 4) continue;

                    dist = float.Parse(row[select2]);
                    x = float.Parse(row[xsel]);
                    y = float.Parse(row[ysel]);
                    z = float.Parse(row[zsel]);
                    Coordinates.Add(new Vector32(x, y, z, dist));
                }                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Coordinates;
        }
        // 0 -- top
        // 1 -- down
        // 2 -- replace
        // 3 -- nochange
        public virtual bool MatchedFrom(List<Vector32> coordinates, int zSet = 0 )
        {
            int n = coordinates.Count;
            if (n < 1) return false;

            float z0 = 0;
            double dist1 = coordinates[0].V;
            double dist2 = coordinates[n - 1].V;

            Vector32 p1, p2, pp, p = new Vector32();
            int n1, n2;

            stringRow row;
            select1 = MatchColumnComboBox1.SelectedIndex;
            for (int i = 0; i < asc1.TotalRows; i++)
            {
                row = asc1.GetRow(i);
                p.X = float.Parse( row[select1] );
                p.Z = float.Parse( row[zsel1] );
                
                //不在范围之内
                if ( !Vector32.SearchBoder(p.X, coordinates, out n1, out n2, dist1, dist2)) 
                    continue;

                p1 = coordinates[n1];
                p2 = coordinates[n2];

                if (n1 == n2)
                {
                    p.X = p1.X;
                    p.Y = p1.Y;
                    z0 = p1.Z;
                }
                else
                {
                    pp = p1 + (p2 - p1) * (p.X - p1.V) / (p2.V - p1.V);
                    p.X = pp.X;
                    p.Y = pp.Y;
                    z0 = pp.Z;
                }

                if (zSet == 0) p.Z = z0 - p.Z;//top
                if (zSet == 1) p.Z = z0 + p.Z;//down
                if (zSet == 2) p.Z = z0;//replace
                //if (zSet == 3) p.Z = z0;//nochange
                if (!Matched)
                {
                    row.Add(p.X.ToString());
                    row.Add(p.Y.ToString());
                    row.Add(p.Z.ToString());
                }
                else
                {
                    row[row.Count - 3] = p.X.ToString();
                    row[row.Count - 2] = p.Y.ToString();
                    row[row.Count - 1] = p.Z.ToString();
                }
                
                asc1.pData[i] = row;

            }//for (int i = 0; i < asc1.TotalRows; i++)

            if( !Matched )
            {
                asc1.Titles.Add("MatchedX");
                asc1.Titles.Add("MatchedY");
                asc1.Titles.Add("MatchedZ");
            }

            Matched = true;

            return true;
        }
        bool MatchByProfile()
        {
            List<Vector32> coordinates = GetCoordinatesFromView();

            //V值（点距）升序排列
            coordinates.Sort((a, b) => { return a.V.CompareTo(b.V);});

            if ( coordinates.Count < 1 ) return false;
            //z 高程处理方式
            int doz = DoElevationComboBox.SelectedIndex;
            return MatchedFrom(coordinates,doz);
        }
        void MatchByPoints()
        {

        }
        private void DoMatchingButton_Click(object sender, EventArgs e)
        {
            if (asc1.TotalRows < 1 || asc2.TotalRows < 1) return;

            Cursor = Cursors.WaitCursor;

            if( DataTypeComboBox.SelectedIndex == 0 )//profile
            {
                if( MatchByProfile() )
                {
                    InitListViewHeader1();
                    UpdateListView1();
                }
            }
            else
            {
                MatchByPoints();
            }

            Cursor = Cursors.Default;
        }
        
        void UpdateListView1()
        {
            //if (IgnorFirstRowCheckBox.Checked) start = 1;            
            dataGridView1.Rows.Clear();
            if (asc1.TotalRows < 1 ) return;            
            stringRow srow;            
            int start = curPage * PageNum;

            for (int i = 0; i < PageNum; i++)
            {
                if (start + i >= asc1.TotalRows) break;                
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = start + i + 1;
                dataGridView1.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                srow = asc1.GetRow(start + i);
                for (int j = 0; j < srow.Count; j++)
                    dataGridView1.Rows[i].Cells[j + 1].Value = srow.GetColumn(j);
            }
            UpdatePageLabes();
        }

        private void LoadButton1_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    asc1.Clear();

                    Matched = false;

                    if (asc1.Load(dlg.FileName))
                    {
                        PageNum = 500;
                        TotalPage = (int)(asc1.TotalRows / PageNum) + 1;
                        select1 = -1;
                        dataGridView1.Columns.Clear();
                        dataGridView1.Rows.Clear();
                        InitListViewHeader1();
                        UpdateListView1();
                        InitSelect1();
                    }
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void FirstButton_Click(object sender, EventArgs e)
        {
            if (curPage > 0)
            {
                curPage = 0;
                UpdateListView1();
            }
        }

        private void PreButton_Click(object sender, EventArgs e)
        {
            if (curPage > 0)
            {
                curPage--;
                UpdateListView1();
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (curPage < TotalPage - 1)
            {
                curPage++;
                UpdateListView1();
            }
        }

        private void EndButton_Click(object sender, EventArgs e)
        {
            if (curPage < TotalPage - 1)
            {
                curPage = TotalPage - 1;
                UpdateListView1();
            }
        }
        void UpdatePageLabes()
        {
            PageLabel.Text = "第" + (curPage + 1) + "页 / 共" + TotalPage + "页";
        }

        private void LoadButton2_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    asc2.Clear();

                    if (asc2.Load(dlg.FileName))
                    {
                        select2 = -1;
                        xsel = ysel = zsel = -1;

                        dataGridView2.Columns.Clear();
                        dataGridView2.Rows.Clear();
                        InitListViewHeader2();
                        UpdateListView2();
                        InitSelect2();
                    }
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void ComboBoxZ1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //clear last selection
            if (zsel1 >= 0) dataGridView1.Columns[zsel1 + 1].Selected = false;

            //new selection
            zsel1 = ComboBoxZ1.SelectedIndex;
            if (zsel1 >= 0)
            {
                dataGridView1.Columns[zsel1 + 1].Selected = true;
            }
        }

        private void DataTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int type = DataTypeComboBox.SelectedIndex;
            if(type == 0 )//profile
            {
                ComboBoxZ1.Enabled = true;
            }
            else
            {
                ComboBoxZ1.Enabled = false;
            }            
        }

        private void Export_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (asc1.Export(dlg.FileName))
                    {
                        MessageBox.Show((AppLocalization.IsChinese ? "数据已导出到文件：" : "Data exported to file: ") + dlg.FileName);
                    }
                    else
                    {
                        MessageBox.Show((AppLocalization.IsChinese ? "导出数据到文件失败。" : "Failed to export data to file.") + asc1.errMessage);
                    }
                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void ExportButton2_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (asc2.Export(dlg.FileName))
                    {
                        MessageBox.Show((AppLocalization.IsChinese ? "数据已导出到文件：" : "Data exported to file: ") + dlg.FileName);
                    }
                    else
                    {
                        MessageBox.Show((AppLocalization.IsChinese ? "导出数据到文件失败。" : "Failed to export data to file.") + asc1.errMessage);
                    }
                    this.Cursor = DefaultCursor;
                }
            }
        }

        void UpdateListView2()
        {
            //if (IgnorFirstRowCheckBox.Checked) start = 1;
            dataGridView2.Rows.Clear();
            if (asc2.TotalRows < 1 ) return;
            stringRow srow;
            int start = curPage * PageNum;

            for (int i = 0; i < PageNum; i++)
            {
                if (start + i >= asc2.TotalRows) break;
                dataGridView2.Rows.Add();
                dataGridView2.Rows[i].Cells[0].Value = start + i + 1;
                dataGridView2.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                srow = asc2.GetRow(start + i);
                for (int j = 0; j < srow.Count; j++)
                    dataGridView2.Rows[i].Cells[j + 1].Value = srow.GetColumn(j);
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
