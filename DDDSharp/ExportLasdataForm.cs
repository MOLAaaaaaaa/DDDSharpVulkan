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
    public partial class ExportLasdataForm : Form
    {
        private int pointno = 0;
        private int interval = 1;
        public CBoreholes boreholes;
        double nullvalue;
        public ExportLasdataForm()
        {
            InitializeComponent();
            //boreholes = (CBoreholes)C3DData.GetSelectedObj( ShapeEnum.Borehole );
        }
        public void UpdateDataGridview()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("check", "");
            dataGridView1.Columns.Add("No", "#");
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("X", "X");
            dataGridView1.Columns.Add("Y", "Y");
            dataGridView1.Columns.Add("Z", "Z");
            dataGridView1.Columns.Add("Start", "Start");
            dataGridView1.Columns.Add("End", "End");
            
            double min, max;
            LasFileData data;
            int id;
            for (int i = 0; i < boreholes.Count; i++)
            {
                data = boreholes[i].Curves.lasData;
                id = data.GetDepthIndex();
                pointno = data.LogData[id].Count;
                data.GetDataRange(id, out min, out max);

                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0] = new DataGridViewCheckBoxCell();
                dataGridView1.Rows[i].Cells[1].Value = i + 1;
                dataGridView1.Rows[i].Cells[2].Value = data.Name;
                dataGridView1.Rows[i].Cells[3].Value = Math.Round(data.m_pos.X,6);
                dataGridView1.Rows[i].Cells[4].Value = Math.Round(data.m_pos.Y,6);
                dataGridView1.Rows[i].Cells[5].Value = Math.Round(data.m_pos.Z,6);
                dataGridView1.Rows[i].Cells[6].Value = Math.Round(min,6);
                dataGridView1.Rows[i].Cells[7].Value = Math.Round(max, 6);
                dataGridView1.Rows[i].Cells[0].Value = true;

                nullvalue = data.nullValue;
            }
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            //dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 8, FontStyle.Regular);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            textBoxInterval.Text = interval.ToString();
            totalPointsLabel.Text = (pointno/interval).ToString();            
            textBoxNullValue.Text = nullvalue.ToString();
        }
        private void ExportLasdataForm_Load(object sender, EventArgs e)
        {
            UpdateDataGridview();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (boreholes == null) return;

            string filename;
            try
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter = "las file(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;
                    filename = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            LasFileData data;
            List<float> depths;          
            double x, y, h,z;
            FileStream fs = new FileStream(filename, FileMode.Create);
            StreamWriter wr = new StreamWriter(fs);
            string str = "";
            int id,no = 0;
            nullvalue = double.Parse(textBoxNullValue.Text);
            for (int i = 0; i < boreholes.Count; i++)
            {
                data = boreholes[i].Curves.lasData;
                id = data.GetDepthIndex();

                x = data.m_pos.X;
                y = data.m_pos.Y;
                h = data.m_pos.Z;

                if (no == 0)//write header
                {
                    str = "x,y,z";
                    //depth at 0 column
                    for (int j = 1; j < data.CurveInformation.Count; j++)
                    {
                        str += ",";
                        str += data.CurveInformation[j].Mnemonic;
                    }
                    wr.WriteLine(str);
                }

                depths = data.GetDepthData();
                bool nulldata = false;

                for (int l = 0; l < depths.Count; l += interval )
                {
                    z = h - depths[l];
                    str = x + "," + y + "," + z;
                    nulldata = false;

                    for (int j = 1; j < data.CurveInformation.Count; j++)
                    {
                        str += ",";
                        str += data.LogData[j][l];
                        if (checkBox1.Checked)
                        {
                            if (data.LogData[j][l] == nullvalue)
                            {
                                nulldata = true;
                                break;
                            }
                        }
                    }

                    if (nulldata) continue;

                    wr.WriteLine(str);
                }

                no++;
            }
            wr.Close();
            fs.Close();
        }
        private bool ConvertToInt(string ss, out int ret)
        {
            try
            {
                ret = Convert.ToInt32(ss);
                return true;
            }
            catch
            {
                ret = 0;
                return false;
            }
        }
        private void textBoxInterval_TextChanged(object sender, EventArgs e)
        {
            if (ConvertToInt(textBoxInterval.Text, out interval))
            {
                totalPointsLabel.Text = (pointno / interval).ToString();
            }
            else totalPointsLabel.Text = pointno.ToString();
        }
    }
}
