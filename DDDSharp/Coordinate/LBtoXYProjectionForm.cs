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
        string datafile = "";        
        EncodingInfo[] encodingInfos = null;
        public Encoding encoding = Encoding.UTF8;
        ProjectionConversion2 GSC = null;
        bool IgnoreDeform = false;//忽略变形

        List<Vector32>Points = new List<Vector32>();//经纬度原始点        
        List<Vector32> ProjectedPoints = new List<Vector32>();//投影后的点XY
        Dictionary<string, Vector32> LBDictionary = new Dictionary<string, Vector32>();
        Dictionary<float, float> LDictionary = new Dictionary<float, float>();
        Dictionary<float, float> BDictionary = new Dictionary<float, float>();

        int LBfloatNum = 6;
        int XYfloatNum = 6;
        public int CodePage
        {
            get { return encoding.CodePage; }
            set
            {
                encoding = Encoding.GetEncoding(value);
            }
        }

        public LBtoXYProjectionForm()
        {
            InitializeComponent();
            LBFloatNumTextBox.Text = LBfloatNum.ToString();
            XYFloatNumTextBox.Text = XYfloatNum.ToString();
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

            BigNumerProject.Items.Add("False");
            BigNumerProject.Items.Add("True");
            BigNumerProject.SelectedIndex = 0;

            DeformIgnoreComboBox.Items.Add("False");
            DeformIgnoreComboBox.Items.Add("True");            
            DeformIgnoreComboBox.SelectedIndex = 0;
            if (IgnoreDeform == true) DeformIgnoreComboBox.SelectedIndex = 1;

            InitEncodingCombox();
        }
       
        void InitEncodingCombox()
        {
            EncodingComboBox.Items.Clear();
            EncodingComboBox.SelectedIndex = -1;
            encodingInfos = Encoding.GetEncodings();
            for (int i = 0; i < encodingInfos.Length; i++)
            {
                EncodingComboBox.Items.Add(encodingInfos[i].DisplayName);
                if (CodePage == encodingInfos[i].CodePage)
                    EncodingComboBox.SelectedIndex = i;
            }            
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
            bool ret = ascRows.Load(filename, encoding);            
            if(ascRows.errMessage.Length > 0 )MessageBox.Show(ascRows.errMessage);
            return ret;
        }
        private void ImportFrom_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    datafile = dlg.FileName;
                    ImportDataFrom(datafile);
                }
            }
        }

        void ImportDataFrom(string filename)
        {
            this.Cursor = Cursors.WaitCursor;
            
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            ascRows.Clear();            

            if (LoadFrom(filename))
            {
                PageNum = 500;
                TotalPage = (int)(ascRows.TotalRows / PageNum) + 1;
                InitListViewHeader();
                UpdateColumnComboxes();
                UpdateListView();
            }            
            this.Cursor = Cursors.Default;
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

        bool GetCenterLB(ref float l0, ref float b0)
        {
            if (ascRows.TotalRows < 1) return false;
            int LSel = LongitudeComboBox.SelectedIndex;
            int BSel = LatitudeComboBox.SelectedIndex;
            if (LSel < 0 || BSel < 0) return false;

            int k = 0;
            l0 = b0 = 0;
            stringRow rows;
            float L1=0, L2=0, L=0,B1=0,B2=0, B=0;
            for (int i = 0; i < ascRows.TotalRows; i++)
            {
                rows = ascRows.GetRow(i);
                if ( !float.TryParse(rows.GetColumn(LSel), out L)) continue;
                if ( !float.TryParse(rows.GetColumn(BSel), out B)) continue;
                if (k == 0) 
                { 
                    L1 = L2 = L;
                    B1 = B2 = B;
                }
                else
                {
                    if (L1 > L) L1 = L;
                    if (L2 < L) L2 = L;
                    if (B1 > B) B1 = B;
                    if (B2 < B) B2 = B;
                }
                k++;
            }
            l0 = (L1 + L2) / 2;
            b0 = (B1 + B2) / 2;
            return true;
        }


        /// <summary>
        /// 投影为直角网格
        /// </summary>
        /// <returns></returns>
        int ProjecteWithoutDeform()
        {
            if (ascRows.TotalRows < 1) return 0;
            int LSel = LongitudeComboBox.SelectedIndex;
            int BSel = LatitudeComboBox.SelectedIndex;
            int unitSel = UnitComboBox.SelectedIndex;
            if (LSel < 0 || BSel < 0 || unitSel < 0) return 0;

            float l0 = 0, b0 = 0;
            if (!GetCenterLB(ref l0, ref b0)) return 0;
            if (!CreateGSC()) return 0;

            if (!int.TryParse(LBFloatNumTextBox.Text, out LBfloatNum))
            {
                MessageBox.Show(AppLocalization.IsChinese ? "无效的浮点数。" : "Invalid float number.");
            }
            if (!int.TryParse(XYFloatNumTextBox.Text, out XYfloatNum))
            {
                MessageBox.Show(AppLocalization.IsChinese ? "无效的浮点数。" : "Invalid float number.");
            }
            stringRow rows;
            float B1 = 0, L1 = 0;
            LDictionary.Clear();
            BDictionary.Clear();
            for (int i = 0; i < ascRows.TotalRows; i++)
            {
                rows = ascRows.GetRow(i);
                if (!float.TryParse(rows.GetColumn(LSel), out L1)) continue;
                if (!float.TryParse(rows.GetColumn(BSel), out B1)) continue;
                L1 = (float)Math.Round(L1, LBfloatNum);                
                if ( !LDictionary.ContainsKey(L1))
                {
                    Vector32 p = GSC.GetXYFromBL(b0, L1);
                    if (unitSel == 1) p.Y = p.Y / 1000;
                    p.Y = (float)Math.Round(p.Y, XYfloatNum);
                    LDictionary.Add(L1, p.Y);
                }
                B1 = (float)Math.Round(B1, LBfloatNum);
                if ( !BDictionary.ContainsKey(B1) )
                {
                    Vector32 p = GSC.GetXYFromBL(B1, l0);
                    if (unitSel == 1) p.X = p.X / 1000;
                    p.X = (float)Math.Round(p.X, XYfloatNum);
                    BDictionary.Add(B1, p.X);
                }
            }
            return LDictionary.Count;
        }

        /// <summary>
        /// 投影为直角网格
        /// </summary>
        /// <returns></returns>
        int ProjecteToDictionary( bool ignoreDeform = false )
        {
            if (ascRows.TotalRows < 1) return 0;
            int LSel = LongitudeComboBox.SelectedIndex;
            int BSel = LatitudeComboBox.SelectedIndex;          
            int unitSel = UnitComboBox.SelectedIndex;
            if (LSel < 0 || BSel < 0 || unitSel < 0) return 0;
            
            float l0=0, b0=0;
            if (!GetCenterLB(ref l0,ref b0)) return 0;
            if (!CreateGSC()) return 0;

            if ( !int.TryParse(LBFloatNumTextBox.Text, out LBfloatNum))
            {
                MessageBox.Show(AppLocalization.IsChinese ? "无效的浮点数。" : "Invalid float number.");
            }
            if ( !int.TryParse(XYFloatNumTextBox.Text, out XYfloatNum))
            {
                MessageBox.Show(AppLocalization.IsChinese ? "无效的浮点数。" : "Invalid float number.");
            }   
            stringRow rows;
            float B1 = 0, L1 = 0;
            LBDictionary.Clear();
            for (int i = 0; i < ascRows.TotalRows; i++)
            {
                rows = ascRows.GetRow(i);
                if (!float.TryParse(rows.GetColumn(LSel), out L1)) continue;
                if (!float.TryParse(rows.GetColumn(BSel), out B1)) continue;
                L1 = (float)Math.Round(L1, LBfloatNum);
                B1 = (float)Math.Round(B1, LBfloatNum);
                string ss = L1.ToString() + "," + B1.ToString();
                if (!LBDictionary.ContainsKey(ss))
                {
                    Vector32 p = GSC.GetXYFromBL(B1, L1);
                    if (unitSel == 1)
                    {
                        p.X = p.X / 1000;
                        p.Y = p.Y / 1000;
                    }
                    p.X = (float)Math.Round(p.X, XYfloatNum);
                    p.Y = (float)Math.Round(p.Y, XYfloatNum);
                    LBDictionary.Add(ss, p);
                }
            }
            return LBDictionary.Count;
        }

        void ProjectedToListView()
        {
            int LSel = LongitudeComboBox.SelectedIndex;
            int BSel = LatitudeComboBox.SelectedIndex;
            float B1 = 0, L1 = 0;

            if ( !Projected )
            {
                ascRows.Titles.Add("Projected_North");
                ascRows.Titles.Add("Projected_East");
            }
            Vector32 p = new Vector32();
            string format = "f" + XYfloatNum;
            for (int i = 0; i < ascRows.TotalRows; i++)
            {
                stringRow rows = ascRows.GetRow(i);
                if (!float.TryParse(rows.GetColumn(LSel), out L1)) continue;
                if (!float.TryParse(rows.GetColumn(BSel), out B1)) continue;
                L1 = (float)Math.Round(L1, LBfloatNum);
                B1 = (float)Math.Round(B1, LBfloatNum);
                
                if( IgnoreDeform )
                {
                    p.Y = LDictionary[L1];
                    p.X = BDictionary[B1];
                }
                else
                {
                    string ss = L1.ToString() + "," + B1.ToString();
                    p = LBDictionary[ss];
                }

                if ( !Projected )
                {
                    rows.Add(p.X.ToString());
                    rows.Add(p.Y.ToString());
                }
                else
                {
                    int n = rows.Count;                    
                    rows.pData[n - 2] = p.X.ToString(format);
                    rows.pData[n - 1] = p.Y.ToString(format);
                }
                ascRows.pData[i] = rows;
            }

        }
        private void Convert_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            int count = 0;
            if ( IgnoreDeform )count = ProjecteWithoutDeform();
            else count =  ProjecteToDictionary();
            if ( count > 0)
            {
                ProjectedToListView();
                Projected = true;
                InitListViewHeader();
                UpdateListView();
            }

            Cursor = Cursors.Default;            
        }

        bool CreateGSC()
        {
            int stripSel = StripComboBox.SelectedIndex;
            int unitSel = UnitComboBox.SelectedIndex;
            int cgcsSel = PlaneSystemcomboBox.SelectedIndex;
            double L0;
            if (cgcsSel < 0 || stripSel < 0 || unitSel < 0) return false;
            if (!double.TryParse(CentralLongitudeTextBox.Text, out L0))
            {
                MessageBox.Show(AppLocalization.IsChinese ? "中央经线未填写！" : "Central longitude is missing!");
                return false;
            }
            EnumProjectionCoordinate coord = (EnumProjectionCoordinate)Enum.Parse(typeof(EnumProjectionCoordinate), PlaneSystemcomboBox.SelectedItem.ToString());
            EnumProjectionStrip strip = (EnumProjectionStrip)Enum.Parse(typeof(EnumProjectionStrip), StripComboBox.SelectedItem.ToString());
            EnumUnit unit = (EnumUnit)Enum.Parse(typeof(EnumUnit), UnitComboBox.SelectedItem.ToString());

            EnumProjectionCoordinate gsceum = (EnumProjectionCoordinate)cgcsSel;
            if (gsceum == EnumProjectionCoordinate.CGCS2000)
            {
                GSC = new GSCoordConvertionClass_2000();
                GSC.L0 = System.Convert.ToDecimal(L0);
                if (stripSel == 0) GSC.Strip = EnumProjectionStrip.Strip3;
                else GSC.Strip = EnumProjectionStrip.Strip6;
            }
            if (gsceum == EnumProjectionCoordinate.Xian80)
            {
                GSC = new GSCoordConvertionClass_Xian80();
                GSC.L0 = System.Convert.ToDecimal(L0);
                if (stripSel == 0) GSC.Strip = EnumProjectionStrip.Strip3;
                else GSC.Strip = EnumProjectionStrip.Strip6;
            }
            if (gsceum == EnumProjectionCoordinate.Beijing54)
            {
                //GSC = new GSCoordConvertionClass_2000();
                //GSC.L0 = System.Convert.ToDecimal(L0);
                //if (stripSel == 0) GSC.Strip = EnumProjectionStrip.Strip3;
                //else GSC.Strip = EnumProjectionStrip.Strip6;
            }

            GSC.IsBigNumber = false;
            if (BigNumerProject.SelectedIndex == 1) GSC.IsBigNumber = true;
            return true;
        }
        private void SingleConvert_Click(object sender, EventArgs e)
        {
            if (!CreateGSC()) return;

            double L0, B =0, L = 0;            
            if (!double.TryParse(inBtextBox.Text, out B))
            {
                MessageBox.Show(AppLocalization.IsChinese ? "缺少纬度值！" : "Latitude value is missing!");
                return;
            }
            if (!double.TryParse(inLtextBox.Text, out L))
            {
                MessageBox.Show(AppLocalization.IsChinese ? "缺少经度值！" : "Longitude value is missing!");
                return;
            }           

            Vector64  p = GSC.GetXYFromBL(B,L);
            outXtextBox.Text = Math.Round(p.X, XYfloatNum ).ToString();
            outYtextBox.Text = Math.Round(p.Y, XYfloatNum).ToString();            
            
            //InitListViewHeader();
            //UpdateListView();
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
                        MessageBox.Show((AppLocalization.IsChinese ? "数据已导出到文件：" : "Data exported to file: ") + dlg.FileName);
                    }
                    else
                    {
                        MessageBox.Show((AppLocalization.IsChinese ? "导出数据到文件失败。" : "Failed to export data to file.") + ascRows.errMessage);
                    }
                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void EncodingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EncodingComboBox.SelectedIndex > 0)
            {
                int code = encodingInfos[EncodingComboBox.SelectedIndex].CodePage;
                if (code != CodePage)
                {
                    CodePage = code;                   
                }
            }            
        }

        private void EncodingUpdateButton_Click(object sender, EventArgs e)
        {
            if (datafile.Length > 0) 
            { 
                ImportDataFrom(datafile); 
            }
        }  

        private void DeformIgnoreComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            IgnoreDeform = false;
            if (DeformIgnoreComboBox.SelectedIndex == 1) 
                IgnoreDeform = true;
        }
    }
}
