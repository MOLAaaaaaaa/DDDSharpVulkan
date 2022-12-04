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
    public partial class BoreholeListForm : Form
    {
        public string control_file = "";
        public CBoreholes boreholes = new CBoreholes();
        public AscIIColumn pData = new AscIIColumn();
        List<string> columnName = new List<string>();

        public BoreholeListForm()
        {
            InitializeComponent();
        }
        private bool LoadFrom()
        {
           // if( !pData.Load(datafile) )
            {
               // MessageBox.Show("Load data error.");
               // return false;
            }
            return true;
        }
        void UpdateList()
        {
            dataGridView1.Rows.Clear();
            
            //header
            int id = dataGridView1.Columns.Add("index", "index");
            dataGridView1.Columns[id].Width = 60;
            for (int i=0;i<columnName.Count;i++)
            {   
                dataGridView1.Columns.Add(columnName[i], columnName[i]);
            }
            //rows
            for(int i=0;i<pData.pData.Count;i++)
            {
                id = dataGridView1.Rows.Add();
                stringRow row = pData.pData[i];
                for(int j=0;j<columnName.Count;j++)
                {
                    dataGridView1.Rows[id].Cells[0].Value = (i+1);
                    dataGridView1.Rows[id].Cells[j+1].Value = row.GetColumn(j);
                }
            }
            dataGridView1.AutoResizeColumns();
        }
        void CreateColumnTitle()
        {
            columnName.Clear();

            for (int i = 0; i < pData.Titles.Count; i++)
                columnName.Add(pData.Titles[i]);
            
            for (int i = 0; i < columnName.Count; i++)
            {
                comboBox1.Items.Add(columnName[i]);
                comboBox2.Items.Add(columnName[i]);
                comboBox3.Items.Add(columnName[i]);
                comboBox4.Items.Add(columnName[i]);
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
            comboBox3.SelectedIndex = 2;
            comboBox4.SelectedIndex = 3;
        }
        private void BoreholeListForm_Load(object sender, EventArgs e)
        {
            if (!LoadFrom()) return;
            CreateColumnTitle();
            UpdateList();
        }
        bool GetFromList()
        {
            //first colunm is index
            int sel1 = comboBox1.SelectedIndex + 1;
            int sel2 = comboBox2.SelectedIndex + 1;
            int sel3 = comboBox3.SelectedIndex + 1;
            int sel4 = comboBox4.SelectedIndex + 1;            
            double x, y, z;
            string xstring, ystring, zstring, lasfile;
            List<int> bad_rows = new List<int>();            
            string ss;
            string ext;
            string curpath = Path.GetDirectoryName(control_file);
            int n = dataGridView1.RowCount;
            for (int i = 0; i < n; i++)
            {
                xstring = dataGridView1.Rows[i].Cells[sel1].Value.ToString();
                ystring = dataGridView1.Rows[i].Cells[sel2].Value.ToString();
                zstring = dataGridView1.Rows[i].Cells[sel3].Value.ToString();
                lasfile = dataGridView1.Rows[i].Cells[sel4].Value.ToString();

                //check the filename extansion
                ext = Path.GetExtension(lasfile);
                if (ext.Length < 1) lasfile += ".las";
                //the full path of las file
                lasfile = curpath + "\\" + lasfile;

                if( !ConvertData.StringToDouble(xstring, out x) )
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    ss = "row:" +(i+1)+ ", can't convert '" + xstring + "' to double X.";
                    bad_rows.Add(i);
                    listBox1.Items.Add(ss);
                }
                if (!ConvertData.StringToDouble(ystring, out y))
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    ss = "row:" + (i + 1) + ", can't convert '" + ystring + "' to double Y.";
                    bad_rows.Add(i);
                    listBox1.Items.Add(ss);
                    continue;
                }
                if (!ConvertData.StringToDouble(zstring, out z))
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    ss = "row:" + (i + 1) + ", can't convert '" + zstring + "' to double Elevation.";
                    bad_rows.Add(i);
                    listBox1.Items.Add(ss);
                    continue;
                }
                if ( !File.Exists(lasfile) )
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    ss = "row:" + (i + 1) + ", can't find data file '" + lasfile + "'";
                    bad_rows.Add(i);
                    listBox1.Items.Add(ss);
                    continue;
                }
            }//for (int i = 0; i < n; i++)
            
            if( bad_rows.Count > 0 )
            {
                MessageBox.Show("Errors occured while load boreholes infomation!");
                return false;
            }            
            return true;
        }
        bool LoadBoreholes()
        {
            this.Cursor = Cursors.WaitCursor;

            //first colunm is index
            int sel1 = comboBox1.SelectedIndex + 1;
            int sel2 = comboBox2.SelectedIndex + 1;
            int sel3 = comboBox3.SelectedIndex + 1;
            int sel4 = comboBox4.SelectedIndex + 1;
            double x, y, z;
            string xstring, ystring, zstring, lasfile,lasname;
            List<int> bad_rows = new List<int>();
            string ss;
            string ext;
            string curpath = Path.GetDirectoryName(control_file);

            boreholes.Clear();
            int n = dataGridView1.RowCount;                        
            string texture = "";
            for (int i = 0; i < n; i++)
            {
                xstring = dataGridView1.Rows[i].Cells[sel1].Value.ToString();
                ystring = dataGridView1.Rows[i].Cells[sel2].Value.ToString();
                zstring = dataGridView1.Rows[i].Cells[sel3].Value.ToString();
                lasfile = dataGridView1.Rows[i].Cells[sel4].Value.ToString();
                lasname = lasfile;
                //check the filename extansion
                ext = Path.GetExtension(lasfile);
                if (ext.Length < 1) lasfile += ".las";
                //the full path of las file
                lasfile = curpath + "\\" + lasfile;
                x = double.Parse(xstring);
                y = double.Parse(ystring);
                z = double.Parse(zstring);

                CLasFile las = new CLasFile();
                LasFileData data = las.Read(lasfile);
                if (data.LogData == null)
                {
                    ss = "row:" + (i+1) + " load las data error!";
                    listBox1.Items.Add(ss);
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    continue;
                }
                data.m_pos = new Vector64(x, y, z);                
                data.Name = Path.GetFileNameWithoutExtension(lasname);
                CBorehole bh = new CBorehole(data);                
                boreholes.AddBorehole(bh);
            }

            this.Cursor = Cursors.Default;

            if (boreholes.Count < n)
            {
                MessageBox.Show("Errors occured while load boreholes data!");
                return false;
            }

            else return true;

        }
        private void OKbutton_Click(object sender, EventArgs e)
        {
            if ( !GetFromList() ) return;
            if ( !LoadBoreholes() ) return;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
