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
    public partial class ColumnDataImportForm : Form
    {
        private Encoding[] encodes = new Encoding[7];

        public List<Vector32> points = new List<Vector32>();

        private List<Vector32> pOrg = new List<Vector32>();
        private CDataList pDataList = new CDataList();
        private int select1, select2, select3, select4;
        private int nx, ny, nz;
        private double xstep, ystep, zstep;        

        private double minx, maxx, miny, maxy, minz, maxz, minv, maxv;

        private string errMessage = "";

        char[] splitChars = new char[] { ' ', ',', '\t' };

#pragma warning disable CS0414 // 字段“ColumnDataImportForm.Initializing”已被赋值，但从未使用过它的值
        private bool Initializing = false;  
#pragma warning restore CS0414 // 字段“ColumnDataImportForm.Initializing”已被赋值，但从未使用过它的值

        public ColumnDataImportForm()
        {
            InitializeComponent();
        }

        private void ColumnDataImportForm_Load(object sender, EventArgs e)
        {
            encodes[0] = Encoding.Default;
            encodes[1] = Encoding.Unicode;
            encodes[2] = Encoding.BigEndianUnicode;
            encodes[3] = Encoding.ASCII;
            encodes[4] = Encoding.UTF8;
            encodes[5] = Encoding.UTF7;
            encodes[6] = Encoding.UTF32;
            for (int i = 0; i < encodes.Length; i++)
            {
                encodeComboBox.Items.Add(encodes[i].EncodingName);
            }
            encodeComboBox.SelectedIndex = 0;
        }

        private Encoding GetSelectedCoding()
        {
            int id = encodeComboBox.SelectedIndex;
            return encodes[id];
        }
        public bool ExportToFile(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);

                double x, y, z, v;
                string ss = "x,y,z,value\n";

                string head1 = pDataList.GetHeadName(select1);
                string head2 = pDataList.GetHeadName(select2);
                string head3 = pDataList.GetHeadName(select3);
                string head4 = pDataList.GetHeadName(select4);
                if (head1.Length == 0) head1 = "x";
                if (head2.Length == 0) head2 = "y";
                if (head3.Length == 0) head3 = "z";
                if (head4.Length == 0) head4 = "v";
                ss = head1 + ",";
                ss += head2;
                ss += ",";
                ss += head3;
                ss += ",";
                ss += head4;
                ss += "\n";
                wr.WriteLine(ss);
                for (int i = 0; i < points.Count; i++)
                {
                    x = points[i].x;
                    y = points[i].y;
                    z = points[i].z;
                    v = points[i].v;
                    x = Math.Round(x, 10);
                    y = Math.Round(y, 10);
                    z = Math.Round(z, 10);
                    v = Math.Round(v, 10);
                    ss = x.ToString(); ss += ",";
                    ss += y.ToString(); ss += ",";
                    ss += z.ToString(); ss += ",";
                    ss += v.ToString(); ss += "\n";
                    wr.WriteLine(ss);
                }
                wr.Close();
                fs.Close();

                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        private void exportToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (points.Count < 1)
            {
                MessageBox.Show("void points selected.");
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "ASCII data (*.dat;txt)|*.dat;txt|all files(*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (ExportToFile(dlg.FileName))
                {
                    MessageBox.Show("save to file successfully.\n" + dlg.FileName);
                }
                else
                {
                    MessageBox.Show("save to file failed." + "\n" + errMessage);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(0);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(2);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(2);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(3);
        }

        public bool LoadFromFile(string filename)
        {
            pDataList.Clear();
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, GetSelectedCoding());
            bool nullcheck = checkBoxNullValue.Checked;
            double nullValue = 0;
            if (nullcheck)
            {
                nullValue = ConvertData.toDouble(NullValueTextBox.Text);                
            }

            string ss;
            while ((ss = sr.ReadLine()) != null)
            {
                if (ss.Length < 3) continue;
                string[] entities = ss.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                ProcessLine(entities, nullcheck, nullValue);
            }
            pDataList.UpdateRange();

            sr.Close();
            fs.Close();

            return true;
        }

        private void ProcessLine(string[] str, bool nullcheck, double nullValue)
        {
            int n = str.Length;
            List<float> data = new List<float>();
            float v;
            bool title = false;
            for (int i = 0; i < n; i++)
            {
                if ( !ConvertData.StringToFloat(str[i], out v) )
                {
                    title = true;
                    break;
                }

                if (nullcheck)
                {
                    if (v == nullValue)
                        return;
                }

                data.Add(v);
            }

            if (title)
            {
                if (pDataList.pHeader.Count == 0)
                {
                    for (int i = 0; i < n; i++)
                    {
                        pDataList.AddHeader(str[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                    pDataList.AddData(i, data[i]);
            }
        }

        private bool IsFiltered(Vector32 p)
        {
            float x1, x2, y1, y2, z1, z2, v1, v2;
           
            if ( checkBoxX1.Checked )
            {
                if ( ConvertData.StringToFloat(textBoxX1.Text,out x1 ) )
                {
                    if (p.V < x1 ) return true;
                }                
            }
            if (checkBoxX2.Checked)
            {
                if (ConvertData.StringToFloat(textBoxX2.Text, out x2))
                {
                    if (p.V > x2) return true;
                }
            }

            if (checkBoxY1.Checked)
            {
                if (ConvertData.StringToFloat(textBoxY1.Text, out y1))
                {
                    if (p.Y < y1) return true;
                }
            }
            if (checkBoxY2.Checked)
            {
                if (ConvertData.StringToFloat(textBoxY2.Text, out y2))
                {
                    if (p.Y > y2) return true;
                }
            }

            if (checkBoxZ1.Checked)
            {
                if (ConvertData.StringToFloat(textBoxZ1.Text, out z1))
                {
                    if (p.Z < z1) return true;
                }
            }
            if (checkBoxZ2.Checked)
            {
                if (ConvertData.StringToFloat(textBoxZ2.Text, out z2))
                {
                    if (p.Z > z2) return true;
                }
            }

            if (checkBoxV1.Checked)
            {
                if (ConvertData.StringToFloat(textBoxV1.Text, out v1))
                {
                    if (p.V < v1) return true;
                }
            }
            if (checkBoxV2.Checked)
            {
                if (ConvertData.StringToFloat(textBoxV2.Text, out v2))
                {
                    if (p.V > v2) return true;
                }
            }
            //null value
            if (checkBoxNullValue.Checked)
            {
                if (ConvertData.StringToFloat(NullValueTextBox.Text, out v1))
                {
                    if (p.V == v1) return true;
                }
            }

            return false;
        }
        private void XFilterButton_Click(object sender, EventArgs e)
        {
            TransformPoints();
        }

        private void YFilterButton_Click(object sender, EventArgs e)
        {
            TransformPoints();
        }

        private void ZFilterButton_Click(object sender, EventArgs e)
        {
            TransformPoints();
        }

        private void VFilterButton_Click(object sender, EventArgs e)
        {
            TransformPoints();
        }

        private void NullValueFilterButton1_Click(object sender, EventArgs e)
        {
            TransformPoints();
        }
        private void TransformPoints()
        {
            points.Clear();

            select1 = comboBox1.SelectedIndex;
            select2 = comboBox2.SelectedIndex;
            select3 = comboBox3.SelectedIndex;
            select4 = comboBox4.SelectedIndex;

            if (select1 < 0 || select2 < 0 || select3 < 0 || select4 < 0)
                return;

            Cursor = Cursors.WaitCursor;

            Vector32 p = new Vector32();
            for (int i = 0; i < pDataList.Row; i++)
            {
                p.X = pDataList.GetData(select1, i);
                p.Y = pDataList.GetData(select2, i);
                p.Z = pDataList.GetData(select3, i);
                p.V = pDataList.GetData(select4, i);
                if (!IsFiltered(p))
                {
                    points.Add( new Vector32( p.X, p.Y, p.Z, p.V) );
                }
            }
            UpdateRange();
            UpdateDataInfo();

            Cursor = Cursors.Default;
        }

        public void UpdateRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;
            Vector32 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (i == 0)
                {
                    minx = maxx = p.X;
                    miny = maxy = p.Y;
                    minz = maxz = p.Z;
                    minv = maxv = p.Z;
                }
                else
                {
                    if (p.X < minx) minx = p.X;
                    if (p.Y < miny) miny = p.Y;
                    if (p.Z < minz) minz = p.Z;
                    if (p.X > maxx) maxx = p.X;
                    if (p.Y > maxy) maxy = p.Y;
                    if (p.Z > maxz) maxz = p.Z;
                    if (p.V < minv) minv = p.V;
                    if (p.V > maxv) maxv = p.V;
                }               
            }
        }

        void UpdateDataInfo()
        {
            int n = points.Count;
            double x1 = Math.Round(minx, 6);
            double y1 = Math.Round(miny, 6);
            double z1 = Math.Round(minz, 6);
            double v1 = Math.Round(minv, 6);
            double x2 = Math.Round(maxx, 6);
            double y2 = Math.Round(maxy, 6);
            double z2 = Math.Round(maxz, 6);
            double v2 = Math.Round(maxv, 6);

            if (n == 0) n = pDataList.Row;
            textBoxGridInfo.Text = "valid row:" + n + "\r\n";
            textBoxGridInfo.Text += "x range: " + x1.ToString() + " to " + x2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "y range: " + y1.ToString() + " to " + y2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "z range: " + z1.ToString() + " to " + z2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "v range: " + v1.ToString() + " to " + v2.ToString();
        }
        //col = -1 ,update all column
        private void UpdateSelect(int col = -1)
        {
            select1 = comboBox1.SelectedIndex;
            select2 = comboBox2.SelectedIndex;
            select3 = comboBox3.SelectedIndex;
            select4 = comboBox4.SelectedIndex;

            //update x selection
            if (select1 >= 0)
            {
                if (col == 0 || col < 0)
                {
                    pDataList.GetRange(select1, out minx, out maxx);
                    if (nx < 1) nx = 100;
                    xstep = (maxx - minx) / (nx - 1);
                }
            }
            else
            {
                minx = maxx = 0;
                xstep = 0;
            }

            //update y selection
            if (select2 >= 0)
            {
                if (col == 1 || col < 0)
                {
                    pDataList.GetRange(select2, out miny, out maxy);
                    if (ny < 1) ny = 100;
                    ystep = (maxy - miny) / (ny - 1);
                }
            }
            else
            {
                miny = maxy = 0;
                ystep = 0;
            }

            //update z selection
            if (select3 >= 0)
            {
                if (col == 2 || col < 0)
                {
                    pDataList.GetRange(select3, out minz, out maxz);
                    if (nz < 1) nz = 100;
                    zstep = (maxz - minz) / (nz - 1);
                }
            }
            else
            {
                minz = maxz = 0;
                zstep = 0;
            }

            //update value selection
            if (select4 >= 0)
            {
                if (col == 3 || col < 0)
                {
                    pDataList.GetRange(select4, out minv, out maxv);
                }
            }
            else
            {
                minv = maxv = 0;
            }

            TransformPoints();

        }

        private void InitSelect()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox4.Items.Clear();
            if (pDataList.Row < 1) return;

            string head = "";
            for (int i = 0; i < pDataList.Col; i++)
            {
                head = pDataList.GetHeadName(i);
                if (head.Length < 1)
                {
                    head = "Column " + (i + 1).ToString();
                }
                comboBox1.Items.Add(head);
                comboBox2.Items.Add(head);
                comboBox3.Items.Add(head);
                comboBox4.Items.Add(head);
            }

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
            comboBox3.SelectedIndex = 2;
            comboBox4.SelectedIndex = 3;

            UpdateSelect();
        }
        private void importFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "ASCII Data(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if( LoadFromFile(dlg.FileName) )
                        {
                            InitSelect();
                            //UpdateDataInfo();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
