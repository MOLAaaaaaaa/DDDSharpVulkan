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
using System.Threading;

namespace DDDSharp
{
    public partial class CrossValidationForm : Form
    {
        List<string> pBoreholes = new List<string>();
        string errMessage = "";

        CGeoFile geodata0 = new CGeoFile();
        SgyData3D data0 = new SgyData3D();
        
        C3DGridData maxUncertainty = new C3DGridData();
        int nx, ny, nz;
#pragma warning disable CS0414 // 字段“CrossValidationForm.maxv”已被赋值，但从未使用过它的值
        double minx, maxx, miny, maxy, minz, maxz, minv, maxv;
#pragma warning restore CS0414 // 字段“CrossValidationForm.maxv”已被赋值，但从未使用过它的值

        public Thread fThread;

        public CrossValidationForm()
        {
            InitializeComponent();
        }

        private bool LoadGeoData0()
        {
            geodata0.Clear();
            string geofile = DataPath_textBox.Text + "\\all\\" + GeoFile_textBox.Text;
            if( !geodata0.LoadFrom(geofile) )
            {
                errMessage = geodata0.errMsg;
                return false;
            }
            CrossLineNum_textBox.Text = geodata0.CrossLineNum.ToString();
            InLineNum_textBox.Text = geodata0.InLineNum.ToString();
            MinX_textBox.Text = geodata0.minx.ToString();
            MinY_textBox.Text = geodata0.miny.ToString();
            MaxX_textBox.Text = geodata0.maxx.ToString();
            MaxY_textBox.Text = geodata0.maxy.ToString();

            return true;
        }

        private bool LoadSgyData0()
        {
            data0.Clear();
            string sgyfile = Path.GetFileNameWithoutExtension(GeoFile_textBox.Text) + ".sgy"; ;
            sgyfile = DataPath_textBox.Text + "\\all\\" + sgyfile;             
            if (!data0.LoadFrom(sgyfile))
            {
                errMessage = data0.ErrMsg;
                return false;
            }

            Sampled_textBox.Text = data0.nSampleNum.ToString();
            MinZ_textBox.Text = data0.minz.ToString();
            MaxZ_textBox.Text = data0.maxz.ToString();

            return true;
        }
        private bool LoadBoreholes(string file)
        {
            pBoreholes.Clear();
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string ss;
                while ((ss = sr.ReadLine()) != null)
                {
                    ss.Trim();
                    if (ss.Length < 1) continue;
                    pBoreholes.Add(ss);
                }

                sr.Close();
                fs.Close();

                Boreholes_textBox.Text = file;
                DataPath_textBox.Text = Path.GetDirectoryName(file);

                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        private void BrowseBoreholesButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Boreholes Name List file(*.dat;*.txt)|*.dat;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    if (!LoadBoreholes(dlg.FileName))
                        MessageBox.Show(errMessage);

                    this.Cursor = DefaultCursor;
                }
            }
        }
        
        private void CrossValidationForm_Load(object sender, EventArgs e)
        {
            GeoFile_textBox.Text = "GeoInterpOutput-VP.geo";
            //progressBar1.Visible = false;
        }
        
        private void GeoUpdatebutton_Click(object sender, EventArgs e)
        {
            if (pBoreholes.Count < 2)
            {
                errMessage = "please load boreholes data first.";
                MessageBox.Show(errMessage);
                return;
            }
            if (GeoFile_textBox.Text.Length < 1)
            {
                errMessage = "please specify the .Geo file.";
                MessageBox.Show(errMessage);
                return;
            }
            this.Cursor = Cursors.WaitCursor;

            if (!LoadGeoData0())
            {
                MessageBox.Show(errMessage);
                this.Cursor = Cursors.Default;
                return;
            }
            if (!LoadSgyData0())
            {
                MessageBox.Show(errMessage);
                this.Cursor = Cursors.Default;
                return;
            }

            this.Cursor = Cursors.Default;
        }        
        
        void MatchMaxUncertainty(SgyData3D data)
        {
            SgyDao dao0, dao;
            int ix, iy, iz,index;
            float v0, v,vmax,val;
            int n1, n2;
            for(int i=0;i<data0.Length;i++)
            {
                dao0 = data0.pDao[i];
                dao = data.pDao[i];

                n1 = dao0.pData.Length;
                n2 = dao.pData.Length;

                iy = i / nx;
                ix = i - iy * nx;
                for(int j=0;j<dao0.pData.Length;j++)
                {
                    iz = nz -1 - j;
                    index = iz * nx * ny + iy * nx + ix;

                    v0 = dao0.pData[j];

                    if(n2 == n1 )v = dao.pData[j];                         
                    else v = dao.pData[(int)(j*(double)n2 /n1)];

                    vmax = maxUncertainty.pGridData[index];

                    if (v0 == 0) val = 100;
                    else val = 100 * Math.Abs(v - v0) / Math.Abs(v0);                    

                    if(val > vmax) maxUncertainty.pGridData[index] = val;
                }
            }
        }

        private delegate void SetPos(int ipos);
        private void SetProgressMessage(int ipos)
        {
            if (this.InvokeRequired)
            {
                SetPos setpos = new SetPos(SetProgressMessage);
                this.Invoke(setpos, new object[] { ipos });
            }
            else
            {
                this.ProgressLabel.Text = ipos.ToString() + "%";
                this.progressBar1.Value = Convert.ToInt32(ipos);
            }
        }
        private delegate void SetInfoText(string info);
        private void SetInfoTextMessage(string info)
        {
            if (this.InvokeRequired)
            {
                SetInfoText setinfo = new SetInfoText(SetInfoTextMessage);
                this.Invoke(setinfo, new object[] { info });
            }
            else
            {
                this.InfoTextBox.AppendText(info);
                this.InfoTextBox.AppendText("\r\n");
            }
        }
        private void Start()
        {
            string sgyfile = Path.GetFileNameWithoutExtension(GeoFile_textBox.Text) + ".sgy"; ;
            string ss,sgyfile1;

            maxUncertainty.pGridData = new float[nx*ny*nz];
            for (int i = 0; i < nx * ny * nz; i++) maxUncertainty.pGridData[i] = 0;
            int err = 0;
            SetInfoTextMessage("Start to calculating ...");
            for (int k=0;k<pBoreholes.Count;k++)
            {
                ss = pBoreholes[k];
                sgyfile1 = DataPath_textBox.Text + "\\" + ss + "\\" + sgyfile;
                SgyData3D data = new SgyData3D();
                if ( !data.LoadFrom(sgyfile1) )
                {
                    // MessageBox.Show(data.ErrMsg);
                    errMessage = "Load SGEY file failed. " + data.ErrMsg;                    
                    SetInfoTextMessage(errMessage);
                    err++;
                    continue;
                }
                if( data.Length != data0.Length )
                {
                    //MessageBox.Show("data size can't match.--" + pBoreholes[k]);
                    errMessage = "data size can't match.--" + pBoreholes[k];
                    SetInfoTextMessage(errMessage);
                    err++;                    
                    continue;
                }
                MatchMaxUncertainty(data);

                SetProgressMessage( 100 * k / pBoreholes.Count + 1 );
            }

            maxUncertainty.xNum = nx;
            maxUncertainty.yNum = ny;
            maxUncertainty.zNum = nz;
            maxUncertainty.minx = minx;
            maxUncertainty.miny = miny;
            maxUncertainty.minz = minz;
            maxUncertainty.maxx = maxx;
            maxUncertainty.maxy = maxy;
            maxUncertainty.maxz = maxz;

            maxUncertainty.UpdateRange();

            if( err >0 )
            {
                MessageBox.Show(errMessage);
            }

            string outpath = DataPath_textBox.Text + "\\" + "maxUncertainty.3DGrid";
            maxUncertainty.SaveAs(outpath);

            MessageBox.Show("MaxUncertainty Calculation Completed.\n Data saved to maxUncertainty.3DGrid");
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            nx = ConvertData.StringToInt(CrossLineNum_textBox.Text);
            ny = ConvertData.StringToInt(InLineNum_textBox.Text);
            nz = ConvertData.StringToInt(Sampled_textBox.Text);
            minx = ConvertData.StringToDouble(MinX_textBox.Text);
            miny = ConvertData.StringToDouble(MinY_textBox.Text);
            minz = ConvertData.StringToDouble(MinZ_textBox.Text);
            maxx = ConvertData.StringToDouble(MaxX_textBox.Text);
            maxy = ConvertData.StringToDouble(MaxY_textBox.Text);
            maxz = ConvertData.StringToDouble(MaxZ_textBox.Text);
            minv = maxv = 0;
            if(nx<1||ny<1||nz<1||minx>=maxx||miny>=maxy||minz>=maxz)
            {
                errMessage = "prameters not correct.";
                MessageBox.Show(errMessage);
                return;
            }

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;

            InfoTextBox.Text = "";
            fThread = new Thread(new ThreadStart(Start));//开辟一个新的线程
            fThread.Start();

        }
    }
}
