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
using DataCollection.Projection;
namespace DDDSharp
{
    public partial class LBtoXYProjectionForm : Form
    {
        public AscIIColumn ascRows = new AscIIColumn();
        int PageNum = 500;
        int curPage = 0;        
        int TotalPage = 0;
        bool Projected = false;
        public LBtoXYProjectionForm()
        {
            InitializeComponent();
            FloatNumTextBox.Text = "10";
        }

        private void LBtoXYProjectionForm_Load(object sender, EventArgs e)
        {
            string []names = Enum.GetNames( typeof(EnumProjectionStrip) );
            StripComboBox.Items.AddRange(names);
            StripComboBox.SelectedIndex = 0;

            names = Enum.GetNames(typeof(EnumProjectionCoordinate));
            PlaneSystemcomboBox.Items.AddRange(names);
            PlaneSystemcomboBox.SelectedIndex = 0;

            names = Enum.GetNames(typeof(EnumUnit));
            UnitComboBox.Items.AddRange(names);
            UnitComboBox.SelectedIndex = 0;
        }
       
        void InitListViewHeader()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (ascRows.Titles.Count < 1) return;
            
            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].Width = 60;
            //dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;            
            dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250, 248, 232);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Chocolate;
            dataGridView1.AllowUserToResizeColumns = true;

            for (int i = 0; i < ascRows.Titles.Count; i++)
            {
                dataGridView1.Columns.Add(ascRows.Titles[i], ascRows.Titles[i]);
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

        void UpdateListView()
        {
            //if (IgnorFirstRowCheckBox.Checked) start = 1;            
            dataGridView1.Rows.Clear();
            if (ascRows.TotalRows < 1) return;
            int start = curPage * PageNum;

            for (int i = 0; i < PageNum ; i++)
            {
                if ( start + i >= ascRows.TotalRows) break;
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = start + i + 1;
                dataGridView1.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                stringRow rows = ascRows.GetRow(start + i);
                for (int j = 0; j < rows.Count; j++)
                    dataGridView1.Rows[i].Cells[j + 1].Value = rows.GetColumn(j);
            }

            UpdatePageLabes();
        }
        void UpdateColumnComboxes()
        {
            LongitudeComboBox.Items.Clear();
            LongitudeComboBox.Items.AddRange(ascRows.Titles.ToArray());
            LatitudeComboBox.Items.Clear();
            LatitudeComboBox.Items.AddRange(ascRows.Titles.ToArray());
        }
        void UpdatePageLabes()
        {
            PageLabel.Text = "第" + (curPage + 1) + "页 / 共" + TotalPage+ "页";
        }
        bool LoadFrom(string filename)
        {
            ascRows.Clear();
            return ascRows.Load(filename);
        }
        private void ImportFrom_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (LoadFrom(dlg.FileName))
                    {
                        PageNum = 500;
                        TotalPage = (int)(ascRows.TotalRows / PageNum) + 1;                        
                        InitListViewHeader();
                        UpdateColumnComboxes();
                        UpdateListView();
                    }
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void FirstButton_Click(object sender, EventArgs e)
        {
            if(curPage > 0 )
            {
                curPage = 0;
                UpdateListView();
            }
        }

        private void PreButton_Click(object sender, EventArgs e)
        {
            if (curPage > 0)
            {
                curPage--;
                UpdateListView();
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if ( curPage < TotalPage - 1 )
            {
                curPage ++;
                UpdateListView();
            }
        }

        private void EndButton_Click(object sender, EventArgs e)
        {
            if ( curPage < TotalPage - 1)
            {
                curPage = TotalPage-1;
                UpdateListView();
            }
        }

        bool GetDefaultCentralLongitude()
        {
            if (ascRows.TotalRows < 0) return false;
            int sel = LongitudeComboBox.SelectedIndex;
            if (sel < 0) return false;            
            double l = 0, L1 = 0, L2 = 0;
            string s1;
            for(int i=0;i<ascRows.TotalRows;i++)
            {
                s1 = ascRows.GetRow(i).GetColumn(sel);
                if ( !double.TryParse(s1, out l) ) return false;
                if ( i == 0 ) L1 = L2 = l;                
                else
                {
                    if (l < L1) L1 = l;
                    if (l > L2) L2 = l;
                }
            }

            if(StripComboBox.SelectedIndex == 0)//strip3
            {
                double L0 = (L1 + L2) / 2.0;
                double dd,del = 360;
                int iStrip = 0;
                for(int i = 0; i < 120; i++ )
                {
                    dd = Math.Abs(3 + i * 3 - L0);
                    if (dd < del) 
                    {
                        iStrip = i;
                        del = dd;
                    }
                }
                CentralLongitudeTextBox.Text = (iStrip * 3 + 3).ToString();
            }
            else if (StripComboBox.SelectedIndex == 1)//strip6
            {
                double L0 = (L1 + L2) / 2.0;
                double dd, del = 360;
                int iStrip = 0;
                for (int i = 0; i < 60; i++)
                {
                    dd = Math.Abs(3 + i * 6 - L0);
                    if (dd < del)
                    {
                        iStrip = i;
                        del = dd;
                    }
                }
                CentralLongitudeTextBox.Text = (iStrip * 6 + 3).ToString();
            }
            return true;
        }
        private void LongitudeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetDefaultCentralLongitude();
        }

        private void StripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetDefaultCentralLongitude();
        }

        private void Convert_Click(object sender, EventArgs e)
        {
            if (ascRows.TotalRows < 1) return;
            int LSel = LongitudeComboBox.SelectedIndex;
            int BSel = LatitudeComboBox.SelectedIndex;
            int cgcsSel = PlaneSystemcomboBox.SelectedIndex;
            int stripSel = StripComboBox.SelectedIndex;
            int unitSel = UnitComboBox.SelectedIndex;
            if (LSel < 0 || BSel < 0 || stripSel < 0 || unitSel < 0) return;
            stringRow  rows;
            decimal B = 0, L = 0, x = 0, y = 0;
            
            GSCoordConvertionClass_2000 cc = new GSCoordConvertionClass_2000();
            cc.L0 = decimal.Parse(CentralLongitudeTextBox.Text);
            if(stripSel==0 )cc.Strip = EnumProjectionStrip.Strip3;
            else cc.Strip = EnumProjectionStrip.Strip6;

            GSCoordConvertionClass_Xian80 cc80 = new GSCoordConvertionClass_Xian80();
            cc80.L0 = decimal.Parse(CentralLongitudeTextBox.Text);
            if (stripSel == 0) cc80.Strip = EnumProjectionStrip.Strip3;
            else cc80.Strip = EnumProjectionStrip.Strip6;

            int floatNum = int.Parse(FloatNumTextBox.Text);
            List<string> BStrings = new List<string>();
            List<string> LStrings = new List<string>();

            Cursor = Cursors.WaitCursor;

            for ( int i = 0; i < ascRows.TotalRows; i++ )
            {
                rows = ascRows.GetRow(i);
                if (!Decimal.TryParse(rows.GetColumn(BSel), out B)) return;
                if (!Decimal.TryParse(rows.GetColumn(LSel), out L)) return;

                //正算
                //B = 36.155619734M;
                //L = 105.254854607M;
                //cc80.GetXYFromBL(B, L, ref x, ref y);
                if ( cgcsSel == 1) cc80.GetXYFromBL(B, L, ref x, ref y);
                else cc.GetXYFromBL(B, L, ref x, ref y);
                if (unitSel == 1) { x = x / 1000; y = y / 1000; }
                x = Math.Round(x,floatNum);
                y = Math.Round(y, floatNum);
                BStrings.Add(x.ToString());
                LStrings.Add(y.ToString());
            }

            if (!Projected)
            {
                ascRows.Titles.Add("Projected_X");
                ascRows.Titles.Add("Projected_Y");
            }

            for(int i = 0; i < ascRows.TotalRows; i++ )
            {
                rows = ascRows.GetRow(i);
                if (!Projected) 
                { 
                    rows.Add(BStrings[i]); 
                    rows.Add(LStrings[i]); 
                }
                else
                {
                    int n = rows.Count;
                    rows.pData[n - 2] = BStrings[i];
                    rows.pData[n - 1] = LStrings[i];
                }
                ascRows.pData[i] = rows;
            }

            Projected = true;
            BStrings.Clear();
            LStrings.Clear();
            
            Cursor = Cursors.Default;

            InitListViewHeader();
            UpdateListView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitListViewHeader();
            UpdateListView();
        }

        private void Export_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if ( ascRows.Export(dlg.FileName))
                    {
                        MessageBox.Show("data exported to file:" + dlg.FileName);
                    }
                    else
                    {
                        MessageBox.Show("failed to export data to file." + ascRows.errMessage);
                    }
                    this.Cursor = DefaultCursor;
                }
            }
        }
    }
}
