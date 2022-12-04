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
    public partial class LoadProfileFromGRDForm : Form
    {
        public Bitmap bmp = null;
        public string imagefile = "";
        public CSurferGrid grid2D = new CSurferGrid();
        public List<Vector64> Baseline = new List<Vector64>();
        public BaselineDirectionEnum baselineDirection = BaselineDirectionEnum.Top;

        AscIIColumn Coordinates = new AscIIColumn();
        //matching by selection
        int distSel = -1, xsel = -1, ysel = -1, zsel = -1;

        public LoadProfileFromGRDForm()
        {
            InitializeComponent();
        }
        void InitListViewHeader()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            if (Coordinates.TotalRows < 1) return;

            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns[0].Width = 60;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(250, 248, 232);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Chocolate;
            dataGridView1.AllowUserToResizeColumns = true;

            for (int i = 0; i < Coordinates.Titles.Count; i++)
            {
                dataGridView1.Columns.Add(Coordinates.Titles[i], Coordinates.Titles[i]);
                dataGridView1.Columns[i + 1].Width = 100;
                dataGridView1.Columns[i + 1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns[i + 1].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            //dataGridView2.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        void UpdateListView()
        {
            //if (IgnorFirstRowCheckBox.Checked) start = 1;
            dataGridView1.Rows.Clear();
            if (Coordinates.TotalRows < 1) return;
            stringRow srow;           

            for (int i = 0; i < Coordinates.TotalRows; i++)
            {               
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = i + 1;
                dataGridView1.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                srow = Coordinates.GetRow(i);
                for (int j = 0; j < srow.Count; j++)
                    dataGridView1.Rows[i].Cells[j + 1].Value = srow.GetColumn(j);
            }
        }
        private void LoadCoordinatesClick(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "data file(*.dat,*.txt,*.csv)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    Coordinates.Clear();
                    if (Coordinates.Load(dlg.FileName))
                    {
                        InitListViewHeader();
                        UpdateListView();
                        InitSelection();
                    }
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void LoadGrdButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Gridded Data(*.grd) | *.grd | Image file(*.jpg;*.png;*.bmp;*.gif) |*.jpg;*.jpeg;*.png;*.bmp;*.gif |all files(*.*)|*.*";
                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if( dlg.FilterIndex == 2 )//load from image
                    {
                        imagefile = dlg.FileName;
                        bmp = new Bitmap(dlg.FileName);
                        grid2D.fromImage(bmp);
                    }
                    else //load from grd file
                    {
                        bmp = null;
                        if (!grid2D.Read(dlg.FileName))
                        {
                            MessageBox.Show("Load gridding data faild.\n" + grid2D.errMessage);
                            return;
                        }
                    }
                    UpdateGridingInfo();
                }
            }
        }

        void UpdateGridingInfo()
        {
            if (grid2D == null || grid2D.xGrid < 1 || grid2D.yGrid < 1)
                return;
            GrdInforTextBox.Text = "";
            GrdInforTextBox.Text += "XGrid = " + grid2D.xGrid + "\r\n";
            GrdInforTextBox.Text += "YGrid = " + grid2D.yGrid + "\r\n";
            GrdInforTextBox.Text += "Minimum X = " + grid2D.minx +  "\r\n";
            GrdInforTextBox.Text += "Maximum X = " + grid2D.maxx + "\r\n";
            GrdInforTextBox.Text += "Minimum Y = " + grid2D.miny + "\r\n";
            GrdInforTextBox.Text += "Maximum Y = " + grid2D.maxy + "\r\n";
            GrdInforTextBox.Text += "Minimum V = " + grid2D.minv + "\r\n";
            GrdInforTextBox.Text += "Maximum V = " + grid2D.maxv ;
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void InitSelection()
        {
            XComboBox.Items.Clear();
            YComboBox.Items.Clear();
            ZComboBox.Items.Clear();
            MatchByComboBox.Items.Clear();
            for (int i = 0; i < Coordinates.Titles.Count; i++)
            {
                XComboBox.Items.Add(Coordinates.Titles[i]);
                YComboBox.Items.Add(Coordinates.Titles[i]);
                ZComboBox.Items.Add(Coordinates.Titles[i]);
                MatchByComboBox.Items.Add(Coordinates.Titles[i]);
            }
            XComboBox.SelectedIndex = -1;
            YComboBox.SelectedIndex = -1;
            ZComboBox.SelectedIndex = -1;
            MatchByComboBox.SelectedIndex = -1;
            string ss1;
            for (int i = 0; i < Coordinates.Titles.Count; i++)
            {
                ss1 = Coordinates.Titles[i].Trim().ToLower();

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

            BaselinePosComboBox.Items.Clear();
            string[] names = Enum.GetNames(typeof( BaselineDirectionEnum));
            BaselinePosComboBox.Items.AddRange(names);
            BaselinePosComboBox.SelectedIndex = 0;
        }
        bool GetBaseline()
        {
            if( Coordinates.TotalRows < 1 )
            {
                MessageBox.Show("No coordinates data loaded.");
                return false;
            }
            xsel = XComboBox.SelectedIndex;
            ysel = YComboBox.SelectedIndex;
            zsel = ZComboBox.SelectedIndex;
            distSel = MatchByComboBox.SelectedIndex;
            if(xsel < 0 || ysel < 0 || zsel <0 || distSel < 0 )
            {
                MessageBox.Show("No Colunms selected.");
                return false;
            }

            try 
            {
                Baseline.Clear();
                stringRow row;
                double x, y, z, v;
                for(int i=0;i< Coordinates.TotalRows;i++)
                {
                    row = Coordinates.GetRow(i);
                    x = double.Parse(row.GetColumn(xsel));
                    y = double.Parse(row.GetColumn(ysel));
                    z = double.Parse(row.GetColumn(zsel));
                    v = double.Parse(row.GetColumn(distSel));
                    Baseline.Add(new Vector64(x,y,z,v));
                }

                //按V值（dist）升序排列
                Baseline.Sort((a, b) => { return a.V.CompareTo(b.V); });
                
                return true;
            }
            catch(Exception ex)
            {
                Baseline.Clear();
                MessageBox.Show("Error occurred." + ex.Message);
                return false;
            }
        }
        private void OK_Click(object sender, EventArgs e)
        {
            if (grid2D == null || grid2D.xGrid < 1 || grid2D.yGrid < 1)
            {
                MessageBox.Show("No gridding data loaded.");
                return;
            }
            if ( !GetBaseline() ) return;
            baselineDirection = (BaselineDirectionEnum) BaselinePosComboBox.SelectedIndex;
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
