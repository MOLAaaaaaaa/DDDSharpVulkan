using System;
using System.IO;
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
    public partial class ImportScatterPointsForm : Form
    {
        public string datafile = "";
        int sel1 = -1;  //line
        int sel2 = -1;  //point
        int sel3 = -1;  //x
        int sel4 = -1;  //y
        int sel5 = -1;  //z
        public string errMsg = "";

        int Interval = 1;
        public AscIIColumn ascRows = new AscIIColumn();
        double MinX = 0, MaxX = 0;
        double MinY = 0, MaxY = 0;
        double MinZ = 0, MaxZ = 0;
        double MinV = 0, MaxV = 0;
        int PageNum = 500;
        int CurPage = 0;
        int TotalPage = 0;
        long TotalRows = 0;

        int matchingColumn = -1;
        ScatteredPoints Coordinates = null;
        StreamReader streamReader = null;
        BinaryReader binaryReader = null;

        public ScatteredPoints output = null;

        FilePage CurrentPage
        {
            get
            {
                if ( CurPage >=0 && ascRows.PageData.Count > CurPage) 
                    return ascRows.PageData[CurPage];
                else return null;
            }
        }        

        public ImportScatterPointsForm()
        {
            InitializeComponent();
            IgnorFirstRowCheckBox.Checked = true;
        }
        void UpdateInfo()
        {
            SampleIntervalTextBox.Text = Interval.ToString();
            int percent = (int)(100.0 * ascRows.Row / TotalRows);
            InforBox.Text = "Loaded(" +percent +  "%): "+ascRows.Row + "/"+ TotalRows +"\r\n";
            InforBox.Text += "X: " + MinX + " to " + MaxX + "\r\n";
            InforBox.Text += "Y: " + MinY + " to " + MaxY + "\r\n";
            InforBox.Text += "Z: " + MinZ + " to " + MaxZ + "\r\n";
            InforBox.Text += "V: " + MinV + " to " + MaxV;
        }

        //预读文件
        private bool PreLoadFile(string file)
        {  
            try 
            {
                PageNum = 1000;
                ascRows.Clear();
                ascRows.PageNum = PageNum;
                TotalRows =  ascRows.GetFileTotalRows(file);
                
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                // streamReader = new StreamReader(fs, Encoding.Default);
                binaryReader = new BinaryReader(fs, Encoding.Default);


                if ( TotalRows < 1 ) return false;

                //calculate memory size
                long MB = 1024 * 1024;
                double available = PhysicalMemory.GetAvailableMemoryMB() * 0.5;
                //按GridBox计算：8个顶点，12个面
                double required = ( Graphics3D.Vertex3D.GetSize() * 8.0 + 12*sizeof(int) ) * TotalRows / MB;

                Interval = 1;
                
                if (required > available)
                {
                    Interval = (int)(required / available + 1);                    
                    MessageBox.Show("No enough memory to load data,not all data are loaded.Interval set to" + Interval, "Warning!",MessageBoxButtons.OK,MessageBoxIcon.Warning);                    
                }                

                TotalPage = (int)(TotalRows / PageNum);
                if (TotalRows % PageNum != 0) TotalPage++;                
                
                CurPage = 0;

                LoadPageData();

                return true;
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }            
        }

        bool LoadPageData()
        {
            if ( CurrentPage == null ) return false;
            if (CurrentPage.LoadFrom(binaryReader))
            {
                //UpdateListView();
                return true;
            }
            else return false;
        }

        private void SetComboxes()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox4.Items.Clear();
            comboBox5.Items.Clear();

            int coloums = ascRows.Col;
            if (coloums < 1) return;
            foreach (string s in ascRows.Titles)
            {
                comboBox1.Items.Add(s);
                comboBox2.Items.Add(s);
                comboBox3.Items.Add(s);
                comboBox4.Items.Add(s);
                comboBox5.Items.Add(s);
            }
            comboBox5.Items.Add("None");

            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            comboBox4.SelectedIndex = -1;
            comboBox5.SelectedIndex = -1;            
        }

        private bool ImportFromFile(string file,string coordfile,int sel1,int sel2,int xsel,int ysel,int zsel)
        {
            try
            {
                sel1 = comboBox1.SelectedIndex;
                sel2 = comboBox2.SelectedIndex;
                sel3 = comboBox3.SelectedIndex;
                sel4 = comboBox4.SelectedIndex;
                sel5 = comboBox5.SelectedIndex;

                //last one is None
                if (sel5 == comboBox5.Items.Count - 1) sel5 = -1;

                string name = Path.GetFileName(datafile);                
                ScatteredPoints sp = new ScatteredPoints(name);

                if (sp.LoadFrom(file, sel1, sel2, sel3, sel4, sel5, Interval))
                {
                    sp.UpdateLabelTextSize();
                    C3DData.AddObject(sp);
                    return true;
                }
                else return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private ScatteredPoints ImportFromFile(string file)
        {
            string name = Path.GetFileName(file);
            ScatteredPoints sp = new ScatteredPoints(name);
            try
            {
                sel1 = comboBox1.SelectedIndex;
                sel2 = comboBox2.SelectedIndex;
                sel3 = comboBox3.SelectedIndex;
                sel4 = comboBox4.SelectedIndex;
                sel5 = comboBox5.SelectedIndex;
                //last one is None
                if (sel5 == comboBox5.Items.Count - 1) sel5 = -1;
                int xsel = sel1;
                int ysel = sel2;
                int zsel = sel3;
                int vsel = sel4;
                int labelsel = sel5;
                //matching coordinates
                if ( Coordinates != null && Coordinates.Count > 0 && matchingColumn >=0 )
                {
                    xsel = ysel = matchingColumn;
                    if (sp.LoadMatchedFrom(file, Coordinates.points, xsel, ysel, zsel, vsel, labelsel, Interval))
                    {
                        sp.UpdateLabelTextSize();                        
                    }                    
                }
                else
                {
                    if (sp.LoadFrom(file, xsel, ysel, zsel, vsel, labelsel, Interval))
                    {
                        sp.UpdateLabelTextSize();
                    }                    
                }
                return sp;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return null;
            }            
        }
        
        //初始化选择
        void InitColumnSelection()
        {
            int cols = ascRows.Titles.Count;
            if ( cols < 1) return;

            string ss1;
            for (int i = 0; i < ascRows.Titles.Count; i++)
            {
                ss1 = ascRows.Titles[i].Trim().ToLower();

                if (comboBox1.SelectedIndex < 0 &&
                    (ss1.Contains("x") || ss1.Contains("north")))
                { 
                    comboBox1.SelectedIndex = i;
                    continue;
                }

                if (comboBox2.SelectedIndex < 0 &&
                    (ss1.Contains("y") || ss1.Contains("east")))
                { 
                    comboBox2.SelectedIndex = i;
                    continue;
                }

                if (comboBox3.SelectedIndex < 0 &&
                     (ss1.Contains("z") || ss1.Contains("深度") ||
                       ss1.Contains("depth") || ss1.Contains("高程") ||
                       ss1.Contains("elevation")))
                { 
                    comboBox3.SelectedIndex = i;
                    continue;
                }

                if (comboBox4.SelectedIndex < 0 && ss1.Contains("val"))
                { 
                    comboBox4.SelectedIndex = i;
                    continue;
                }
            }

            comboBox5.SelectedIndex = ascRows.Titles.Count;

        }//void InitColumnSelection()

        private void Form_Load(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;   

            if ( !PreLoadFile(datafile) )
            {
                Cursor = Cursors.Default;
                MessageBox.Show("打开文件错误！\n" + errMsg );
                return;
            }
            Cursor = Cursors.Default;
            SetComboxes();
            InitColumnSelection();

            Cursor = Cursors.WaitCursor;
            InitListViewHeader();
            UpdateListView();
            UpdateInfo();
            Cursor = Cursors.Default;
        }
       
        void UpdateSelect()
        {
            sel1 = comboBox1.SelectedIndex;
            sel2 = comboBox2.SelectedIndex;
            sel3 = comboBox3.SelectedIndex;
            sel4 = comboBox4.SelectedIndex;
            sel5 = comboBox5.SelectedIndex;
            if( dataGridView1.Columns.Count > 0 )
            {
                dataGridView1.ClearSelection();
                if (sel1 > -1) dataGridView1.Columns[sel1 + 1].Selected = true;
                if (sel2 > -1) dataGridView1.Columns[sel2 + 1].Selected = true;
                if (sel3 > -1) dataGridView1.Columns[sel3 + 1].Selected = true;
                if (sel4 > -1) dataGridView1.Columns[sel4 + 1].Selected = true;
                if (sel5 > -1&& sel5 < dataGridView1.Columns.Count-1)
                    dataGridView1.Columns[sel5 + 1].Selected = true;                
            }
        }

        void InitListViewHeader()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns[0].Width = 60;
            //dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250,248,232);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Chocolate;
            dataGridView1.AllowUserToResizeColumns = true;            

            for (int i = 0; i < ascRows.Titles.Count; i++)
            {
                dataGridView1.Columns.Add(ascRows.Titles[i], ascRows.Titles[i]);
                dataGridView1.Columns[i + 1].Width = 100;
               // dataGridView1.Columns[i+1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                dataGridView1.Columns[i+1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dataGridView1.Columns[i].HeaderCell.Style.BackColor = Color.Chocolate;
                //dataGridView1.Columns[i].Resizable = DataGridViewTriState.True;
                dataGridView1.Columns[i+1].SortMode = DataGridViewColumnSortMode.NotSortable;               
            }
            dataGridView1.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//行的默认模式居中显示
            //dataGridView1.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;            
        }

        void UpdateListView()
        {
            //if (IgnorFirstRowCheckBox.Checked) start = 1;            
            dataGridView1.Rows.Clear();
            if (CurrentPage == null) return;

            //for(int i = 0; i < dataGridView1.Columns.Count;i++)
            //dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            int start = 0;
            long id = 0;            
            stringRow srow;
            for ( int i = 0 ; i < PageNum; i++ )
            {
                id = CurPage * PageNum + i + start;
                if ( i >= CurrentPage.Row ) break;

                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = id+1;
                dataGridView1.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                srow = CurrentPage.GetRow(i);

                for (int j = 0; j < srow.Count; j++)
                    dataGridView1.Rows[i].Cells[j + 1].Value = srow.GetColumn(j);
            }
            
            UpdateSelect();
            PageLabel.Text = "Page " + (CurPage + 1) + " / " + TotalPage;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(IgnorFirstRowCheckBox.Checked)
            {

            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            output = ImportFromFile(datafile);
            if ( output == null || output.Count < 1 )
            {
                MessageBox.Show("导入数据错误！\n" + errMsg);
                return;
            }
            else
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void PreButton_Click(object sender, EventArgs e)
        {
            if (CurPage > 0)
            {
                CurPage--;
                LoadPageData();
                UpdateListView();
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if ( CurPage < TotalPage-1)
            {
                CurPage++;
                LoadPageData();
                UpdateListView();
            }
        }

        private void EndButton_Click(object sender, EventArgs e)
        {
            if (CurPage < TotalPage-1)
            {
                CurPage = TotalPage-1;
                LoadPageData();
                UpdateListView();
            }
        }

        private void FirstButton_Click(object sender, EventArgs e)
        {
            if (CurPage > 0)
            {
                CurPage = 0;
                LoadPageData();
                UpdateListView();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect();            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect();
        }

        private void ImportScatterPointsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(streamReader != null )
            {
                streamReader.Close();
            }
            if (binaryReader != null) binaryReader.Close();
        }
        
        private void MathingButton_Click(object sender, EventArgs e)
        {
            
        }//MathingButton_Click

        private void Cancel_Click(object sender, EventArgs e)
        {
            output = null;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect();
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect();
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect();
        }
    }
}
