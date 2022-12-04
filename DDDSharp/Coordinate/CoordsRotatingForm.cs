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
    public partial class CoordsRotatingForm : Form
    {
        List<Vector32> pCoordsOrg = new List<Vector32>();
        List<Vector32> pCoordsRotated = new List<Vector32>();

        double minx, miny, maxx, maxy;
        string coordFile = "";
        public CoordsRotatingForm()
        {
            InitializeComponent();
        }
        string[] TrimNull(string[] ss)
        {
            List<string> ret = new List<string>();
            string str;
            for (int i = 0; i < ss.Length; i++)
            {
                str = ss[i].Trim();
                if (str.Length > 0) ret.Add(str);
            }
            return ret.ToArray();
        }

        void UpdateRangeOrg()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            double x, y;
            for (int i = 0; i < pCoordsOrg.Count; i++)
            {
                x = pCoordsOrg[i].x;
                y = pCoordsOrg[i].y;
                if (i == 0)
                {
                    minx = maxx = x;
                    miny = maxy = y;
                }
                else
                {
                    if (x < minx) minx = x;
                    if (y < miny) miny = y;
                    if (x > maxx) maxx = x;
                    if (y > maxy) maxy = y;
                }
            }
        }
        void UpdateRangeRotated()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            double x, y;
            for (int i = 0; i < pCoordsRotated.Count; i++)
            {
                x = pCoordsRotated[i].x;
                y = pCoordsRotated[i].y;
                if (i == 0)
                {
                    minx = maxx = x;
                    miny = maxy = y;
                }
                else
                {
                    if (x < minx) minx = x;
                    if (y < miny) miny = y;
                    if (x > maxx) maxx = x;
                    if (y > maxy) maxy = y;
                }
            }
        }
        void UpdateList1()
        {
            listBox1.Items.Clear();
            double x, y;
            string ss;
            for(int i=0;i<pCoordsOrg.Count;i++)
            {
                x = pCoordsOrg[i].x;
                y = pCoordsOrg[i].y;
                ss = (i + 1).ToString();
                ss += "  ";
                ss += x.ToString("G");
                ss +=  "\t";
                ss += y.ToString("G");
                listBox1.Items.Add(ss);
            }
        }
        void UpdateList2()
        {
            listBox2.Items.Clear();
            string ss;
            double x, y;
            for (int i = 0; i < pCoordsRotated.Count; i++)
            {
                x = pCoordsRotated[i].x;
                y = pCoordsRotated[i].y;
                ss = (i + 1).ToString();
                ss += "  ";
                ss += x.ToString("G");
                ss += "\t";
                ss += y.ToString("G");
                
                listBox2.Items.Add(ss);
            }
        }
        bool LoadFrom(string file)
        {
            pCoordsOrg.Clear();
            pCoordsRotated.Clear();
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string ss;
                string[] sp;
                bool ret1, ret2;
                float x, y;
                while ((ss = sr.ReadLine()) != null)
                {
                    ss = ss.Trim();
                    if (ss.Length < 2) continue;
                    sp = ss.Split(new Char[] { ' ', '\t', ',' }, 50);
                    sp = TrimNull(sp);
                    if (sp.Length < 2) continue;
                    ret1 = ConvertData.StringToFloat(sp[0], out x);
                    ret2 = ConvertData.StringToFloat(sp[1], out y);
                    if (ret1 == false || ret2 == false) continue;
                    pCoordsOrg.Add(new Vector32(x, y, 0));
                }
                sr.Close();
                fs.Close();
                return true;
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                return false;
            }

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "text file(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (LoadFrom(dlg.FileName))
                        {
                            coordFile = dlg.FileName;

                            UpdateList1();
                            UpdateList2();
                            UpdateRangeOrg();
                            Angle_textBox.Text = "0";
                            Center_x_textBox.Text = ((minx + maxx) / 2.0).ToString();
                            Center_y_textBox.Text = ((miny + maxy) / 2.0).ToString();
                        }
                        else MessageBox.Show("Load coordinate data failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ExportRotated(string file)
        {
            if( pCoordsRotated.Count < 1 )
            {
                MessageBox.Show("no coordinates rotated.");
                return;
            }
            try
            {
                FileStream fs = new FileStream(file, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);
                string str;
                double x, y;
                for(int i=0;i<pCoordsRotated.Count;i++)
                {
                    x = pCoordsRotated[i].x;
                    y = pCoordsRotated[i].y;
                    str = x.ToString("G") + "," + y.ToString("G");
                    wr.WriteLine(str);
                }
                wr.Close();
                fs.Close();
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch (Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                MessageBox.Show("save to file failed.");
            }
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "text file(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";
                dlg.FileName = coordFile;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    ExportRotated(dlg.FileName);

                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void RotateButton_Click(object sender, EventArgs e)
        {
            double x0, y0, angle;
            if (!ConvertData.StringToDouble(Angle_textBox.Text, out angle))
            {
                MessageBox.Show("Invalidate angle.");
                return;
            }            
            if (!ConvertData.StringToDouble(Center_x_textBox.Text, out x0))
            {
                MessageBox.Show("Invalidate angle.");
                return;
            }
            if (!ConvertData.StringToDouble(Center_y_textBox.Text, out y0))
            {
                MessageBox.Show("Invalidate angle.");
                return;
            }
            pCoordsRotated.Clear();
            Vector32 p, p1,p0 = new Vector32((float)x0,(float)y0,0);
            for (int i = 0; i < pCoordsOrg.Count; i++)
            {
                p = pCoordsOrg[i];
                p1 = (p-p0).RotateOnAngle(angle, 2);
                pCoordsRotated.Add(p1);
            }
            UpdateRangeRotated();
            UpdateList2();
        }
    }
}
