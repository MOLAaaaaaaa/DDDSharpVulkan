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
using DataCollection.Triangulation;
namespace DDDSharp
{
    public partial class LasInterpolationForm : Form
    {
        static int nx, ny, nz;
        static double xstep, ystep, zstep;
#pragma warning disable CS0649 // 从未对字段“LasInterpolationForm.minv”赋值，字段将一直保持其默认值 0
#pragma warning disable CS0649 // 从未对字段“LasInterpolationForm.maxv”赋值，字段将一直保持其默认值 0
        static double minx, maxx, miny, maxy, minz, maxz, minv, maxv;
#pragma warning restore CS0649 // 从未对字段“LasInterpolationForm.maxv”赋值，字段将一直保持其默认值 0
#pragma warning restore CS0649 // 从未对字段“LasInterpolationForm.minv”赋值，字段将一直保持其默认值 0
        double nullValue = -999.25;
        double belowerValue = 0;
        double greaterValue = 0;

        private bool locked = false;
        private System.Object UpdateLock = new System.Object();
        private CBoreholes boreholes;
        private List<LasFileData> pLasData = new List<LasFileData>();

        public LasInterpolationForm()
        {
            InitializeComponent();
            boreholes = (CBoreholes)C3DData.GetSelectedObj(ShapeEnum.Borehole);
        }
        private LasFileData GetFirstLasData()
        {
            if (pLasData.Count > 0)
                return pLasData[0];
            else return null;
        }
        private void GetLasData()
        {
            if (boreholes == null) return;

            for (int i = 0; i < boreholes.Count; i++)
            {
                pLasData.Add(boreholes[i].Curves.lasData);
            }            
        }
        public delegate void OnUpdateUI();
        private void InitProgressBar(int min = 0, int max = 100)
        {
            progressBar1.Minimum = min;
            progressBar1.Maximum = max;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
        }

        private void UpdateProgress()
        {
            if (progressBar1.InvokeRequired)
            {
                OnUpdateUI outdelegate = new OnUpdateUI(UpdateProgress);
                this.BeginInvoke(outdelegate);
                return;
            }
            progressBar1.PerformStep();
            //progressBar1.Increment(1);

            double value = (double)progressBar1.Value * 100 / (double)(progressBar1.Maximum - progressBar1.Minimum);
            string str = Math.Round(value, 2).ToString() + "%";
            Font font = new Font("Times New Roman", (float)11, FontStyle.Regular);
            PointF pt = new PointF(this.progressBar1.Width / 2 - 10, this.progressBar1.Height / 2 - 10);
            this.progressBar1.CreateGraphics().DrawString(str, font, Brushes.Blue, pt);

            //finished ,hide it
            if (progressBar1.Value >= progressBar1.Maximum)
            {
                progressBar1.Visible = false;
            }
        }
        public void UpdateDataRange()
        {
            if (boreholes == null) return;
            boreholes.UpdateRange();

            minx = boreholes.minx;
            miny = boreholes.miny;
            minz = boreholes.minz;
            maxx = boreholes.maxx;
            maxy = boreholes.maxy;
            maxz = boreholes.maxz;

            nx = ny = nz = 100;
            xstep = (maxx - minx) / (nx - 1);
            ystep = (maxy - miny) / (ny - 1);
            zstep = (maxz - minz) / (nz - 1);
        }
        private void UpdateGeometry()
        {
           // lock (UpdateLock looked)
           if (!locked)
            {
                locked = true;
                textX1.Text = minx.ToString();
                textX2.Text = maxx.ToString();
                textY1.Text = miny.ToString();
                textY2.Text = maxy.ToString();
                textZ1.Text = minz.ToString();
                textZ2.Text = maxz.ToString();

                textStepX.Text = xstep.ToString();
                textStepY.Text = ystep.ToString();
                textStepZ.Text = zstep.ToString();

                textXNum.Text = nx.ToString();
                textYNum.Text = ny.ToString();
                textZNum.Text = nz.ToString();
                locked = false;
            }
        }
        private bool GetFromGeometry()
        {
            if (!locked)
            {
                locked = true;
                bool ret = true;
                ret = ret && ConvertToDouble(textX1.Text,out minx);
                ret = ret && ConvertToDouble(textX2.Text, out maxx);
                ret = ret && ConvertToDouble(textY1.Text, out miny);
                ret = ret && ConvertToDouble(textY2.Text, out maxy);
                ret = ret && ConvertToDouble(textZ1.Text, out minz);
                ret = ret && ConvertToDouble(textZ2.Text, out maxz);

                ret = ret && ConvertToDouble(textStepX.Text, out xstep);
                ret = ret && ConvertToDouble(textStepY.Text, out ystep);
                ret = ret && ConvertToDouble(textStepZ.Text, out zstep);
                ret = ret && ConvertToInt(textXNum.Text, out nx);
                ret = ret && ConvertToInt(textYNum.Text, out ny);
                ret = ret && ConvertToInt(textZNum.Text, out nz);

                ConvertToDouble(NullValueText.Text, out nullValue);
                ConvertToDouble(GreaterValueText.Text, out greaterValue);
                ConvertToDouble( belowValueText.Text, out belowerValue);

                locked = false;
                return ret;
            }
            return false;
        }
        private void DoRangeChangeX(object sender, EventArgs e)
        {
            DoRangeChange(0);
            UpdateGeometry();
        }

        private void DoRangeChangeY(object sender, EventArgs e)
        {
            DoRangeChange(1);
            UpdateGeometry();
        }

        private void DoRangeChangeZ(object sender, EventArgs e)
        {
            DoRangeChange(2);
            UpdateGeometry();
        }

        private void DoSpaceChangeX(object sender, EventArgs e)
        {
            DoSpaceChange(0);
            UpdateGeometry();
        }

        private void DoSpaceChangeY(object sender, EventArgs e)
        {
            DoSpaceChange(1);
            UpdateGeometry();
        }

        private void DoSpaceChangeZ(object sender, EventArgs e)
        {
            DoSpaceChange(2);
            UpdateGeometry();
        }

        private void DoNumChangeX(object sender, EventArgs e)
        {
            DoNumChange(0);
            UpdateGeometry();
        }

        private void DoNumChangeY(object sender, EventArgs e)
        {
            DoNumChange(1);
            UpdateGeometry();
        }

        private void DoNumChangeZ(object sender, EventArgs e)
        {
            DoNumChange(2);
            UpdateGeometry();
        }
        private bool IsFilteredValue(double v)
        {
            if (checkBoxNull.Checked)
            {                
                if (v == nullValue) return true;
            }
            if (checkBoxBelow.Checked)
            {
                if (v < belowerValue) return true;
            }
            if (checkBoxGreater.Checked)
            {
                if (v > greaterValue) return true;
            }
            return false;
        }
        public void UpdateDataGridview()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            LasFileData data = GetFirstLasData();
            if (data == null) return;

            comboBox1.Items.Add("X");
            comboBox2.Items.Add("Y");
            comboBox3.Items.Add("Z");
            for (int i = 0; i < data.CurveInformation.Count; i++)
                comboBox4.Items.Add(data.CurveInformation[i].Mnemonic);

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;            

            dataGridView1.Columns.Add("check", "");
            dataGridView1.Columns.Add("No", "#");
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("X", "X");
            dataGridView1.Columns.Add("Y", "Y");
            dataGridView1.Columns.Add("Z", "Z");

            for (int i = 0; i < data.pLayers.Count; i++)
            {   
                dataGridView1.Columns.Add(data.GetLayer(i).layername, 
                                          data.GetLayer(i).layername);                
            }
            for (int i = 0; i < pLasData.Count; i++)
            {
                data = pLasData[i];
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0] = new DataGridViewCheckBoxCell();
                dataGridView1.Rows[i].Cells[1].Value = i + 1;
                dataGridView1.Rows[i].Cells[2].Value = data.Name;
                dataGridView1.Rows[i].Cells[3].Value = Math.Round(data.m_pos.X, 6);
                dataGridView1.Rows[i].Cells[4].Value = Math.Round(data.m_pos.Y, 6);
                dataGridView1.Rows[i].Cells[5].Value = Math.Round(data.m_pos.Z, 6);
                for (int j = 0; j < data.pLayers.Count; j++)
                {
                    dataGridView1.Rows[i].Cells[6 + j].Value = data.GetLayer(j).start;
                }                
                dataGridView1.Rows[i].Cells[0].Value = true;
            }
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            //dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 8, FontStyle.Regular);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;            
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
        private bool ConvertToFloat(string ss, out float ret)
        {
            try
            {
                ret = Convert.ToSingle(ss);
                return true;
            }
            catch
            {
                ret = 0;
                return false;
            }
        }
        private bool ConvertToDouble(string ss, out double ret)
        {
            try
            {
                ret = Convert.ToDouble(ss);
                return true;
            }
            catch
            {
                ret = 0;
                return false;
            }
        }
        
        private void StartButton_Click(object sender, EventArgs e)
        {
            if( textOutputFile.Text.Length < 5 )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "请指定输出文件。" : "Please specify the output file.");
                return;
            }
            if (!GetFromGeometry())
            {
                MessageBox.Show(AppLocalization.IsChinese ? "插值几何参数不正确。" : "Interpolation geometry is not correct.");
                return;
            }

            C3DGridData data = new C3DGridData(minx, maxx, miny, maxy, minz, maxz, minv, maxv);
            data.xNum = nx;
            data.yNum = ny;
            data.zNum = nz;
            data.pGridData = new float[nx*ny*nz];
            if(data.pGridData == null)
            {
                MessageBox.Show(AppLocalization.IsChinese ? "内存不足。" : "Not enough memory.");
                return;
            }

            TriangleLasPosition();

            InitProgressBar(0,nz);

            double x, y, z, v;
            int no = 0;
            for (int iz = 0; iz < nz; iz++)
            {
                z = minz + iz * zstep;
                for (int iy = 0; iy < ny; iy++)
                {
                    y = miny + iy * ystep;
                    for (int ix = 0; ix < nx; ix++)
                    {
                        x = minx + ix * xstep;
                        v = Interpolate(x, y, z);
                        data[no++] = (float)v;
                    }
                }
                UpdateProgress();
            }
            data.UpdateRange();
            data.SaveAs(textOutputFile.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "3D Gridding data (*.3DGrid)|*.3DGrid;|all files(*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textOutputFile.Text = dlg.FileName;
            }
        }

        private void DoNumChange(int dir)
        {
            //lock (UpdateLock)
            if (!locked)
            {
                locked = true;
                if (dir == 0)
                {
                    if (ConvertToInt(textXNum.Text, out nx))
                        xstep = (maxx - minx) / (nx - 1);
                }
                if (dir == 1)
                {
                    if (ConvertToInt(textYNum.Text, out ny))
                        ystep = (maxy - miny) / (ny - 1);
                }
                if (dir == 2)
                {
                    if (ConvertToInt(textZNum.Text, out nz))
                        zstep = (maxz - minz) / (nz - 1);
                }
                locked = false;
            }
        }
        private void DoSpaceChange(int dir)
        {
            //lock (UpdateLock)
            if (!locked)
            {
                locked = true;
                if (dir == 0)
                {
                    if (ConvertToDouble(textStepX.Text, out xstep))
                        nx = (int)((maxx - minx) / xstep) + 1;
                }
                if (dir == 1)
                {
                    if (ConvertToDouble(textStepY.Text, out ystep))
                        ny = (int)((maxy - miny) / ystep) + 1;
                }
                if (dir == 2)
                {
                    if (ConvertToDouble(textStepZ.Text, out zstep))
                        nz = (int)((maxz - minz) / zstep) + 1;
                }
                locked = false;
            }
               
        }
        private void DoRangeChange(int dir)
        {
            //lock (UpdateLock)
            if (!locked)
            {
                locked = true;
                if (dir == 0)
                {
                    if (ConvertToDouble(textX1.Text, out minx) &&
                         ConvertToDouble(textX2.Text, out maxx))
                        xstep = maxx - minx / (nx - 1);
                }
                if (dir == 1)
                {
                    if (ConvertToDouble(textY1.Text, out miny) &&
                        ConvertToDouble(textY2.Text, out maxy))
                        ystep = maxy - miny / (ny - 1);
                }
                if (dir == 2)
                {
                    if (ConvertToDouble(textZ1.Text, out minz) &&
                         ConvertToDouble(textZ2.Text, out maxz))
                        zstep = maxz - minz / (nz - 1);
                }
                locked = false;
            }
        }
        private void LasInterpolationForm_Load(object sender, EventArgs e)
        {
            GetLasData();
            UpdateDataGridview();
            UpdateDataRange();
            UpdateGeometry();
        }
        private void AddLayer(string site_id, LayerStruct layer)
        {
            LasFileData data;          
            for (int i = 0; i < pLasData.Count; i++)
            {
                data = pLasData[i];
                if(data.Name == site_id)
                {
                    data.AddLayer(layer);
                    pLasData[i] = data;
                    return;
                }
            }
        }
        //SITE_ID	TopData	B_top	B_Lsplit	T_base	B_split	BaseData
        private bool LoadLayerControl(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string ss;
            int i = 0;
            List<string> headers = new List<string>();
            while ((ss = sr.ReadLine()) != null)
            {
                if (ss.Length < 3) continue;
                string[] str = ss.Split(new Char[] { ',', ',' }, 20);
                if (i == 0) //header
                {                    
                    for (int k = 0; k < str.Length; k++)
                    {
                        headers.Add(str[k]);
                    }
                }
                else
                {
                    for (int k = 1; k < headers.Count; k++)
                    {
                        LayerStruct layer = new LayerStruct();
                        layer.layername = headers[k];
                        layer.start = float.Parse(str[k]);
                        AddLayer(str[0], layer);
                    }
                }
                i++;
            }
            sr.Close();
            fs.Close();
            return true;
        }
        private TriangleObj triangles = new TriangleObj();
        private void TriangleLasPosition()
        {
            triangles.Clear();

            Triangulate tr = new Triangulate();
            //tr.switches = "-z";

            Vector64 p;
            for (int i = 0; i < pLasData.Count; i++)
            {
                p = pLasData[i].m_pos;
                tr.AddPoint(p.X, p.Y, p.Z);
            }

            if (tr.Compute() > 0)
            {
                /*
                double x, y, z;
                for (int i = 0; i < tr.outPointNum; i++)
                {
                    tr.GetPoint(i, out x, out y, out z);
                    triangles.AddPoint(new Vector32((float)x, (float)y, (float)z));
                }
                for (int i = 0; i < tr.triangleNum; i++)
                {
                    triangles.AddTriangleIndex(tr.GetTriangle(3 * i),
                                         tr.GetTriangle(3 * i + 1),
                                         tr.GetTriangle(3 * i + 2));
                }
                */
                triangles = tr.toTriangleObj();
                triangles.UpdateRange();       
                
            }

            tr.Clear();
        }
        private double Interpolate(double x,double y,double z)
        {
            CTriangle3f tri = new CTriangle3f();
            Vector64 p0 = new Vector64(x,y,0);
            int i1, i2, i3;
            for(int i=0;i<triangles.triangles.Count;i++)
            {
                i1 = triangles.triangles[i].x;
                i2 = triangles.triangles[i].y;
                i3 = triangles.triangles[i].z;
                tri.p1 = triangles.points[i1].toVector64();
                tri.p2 = triangles.points[i2].toVector64();
                tri.p3 = triangles.points[i3].toVector64();
                if( tri.IsPointInTriangle(p0) )
                {
                   // return InterpolateTriangle(x, y, z, i1, i2, i3);
                    return 1;
                }
            }
            return 0;
        }
        private double InterpolateTriangle(double x0, double y0, double z0,int i1,int i2,int i3)
        {
            int vid = comboBox4.SelectedIndex;

            LasFileData d1 = pLasData[i1];
            LasFileData d2 = pLasData[i2];
            LasFileData d3 = pLasData[i3];
            double x1, y1, z1,v1;
            CInversePower ip = new CInversePower();
            ip.SetSearchDist(-1);
            ip.SetScale(0.1, 0.1, 1);
            if( layerRestrictCheckBox.Checked )
            {   
            }
            else
            {
                List<float> data1 = d1.GetData(vid);
                List<float> depth1 = d1.GetDepthData();
                x1 = d1.m_pos.X;
                y1 = d1.m_pos.Y;                
                for (int i = 0; i < data1.Count; i++)
                {
                    z1 = d1.m_pos.Z - depth1[i];
                    v1 = data1[i];
                    if (IsFilteredValue(v1)) continue;
                    ip.AddPoint(x1, y1, z1, v1);
                }
                data1 = d2.GetData(vid);
                depth1 = d2.GetDepthData();
                x1 = d2.m_pos.X;
                y1 = d2.m_pos.Y;
                for (int i = 0; i < data1.Count; i++)
                {
                    z1 = d1.m_pos.Z - depth1[i];
                    v1 = data1[i];
                    if (IsFilteredValue(v1)) continue;
                    ip.AddPoint(x1, y1, z1, v1);
                }
                //d3
                data1 = d3.GetData(vid);
                depth1 = d3.GetDepthData();
                x1 = d3.m_pos.X;
                y1 = d3.m_pos.Y;
                for (int i = 0; i < data1.Count; i++)
                {
                    z1 = d1.m_pos.Z - depth1[i];
                    v1 = data1[i];
                    if (IsFilteredValue(v1)) continue;
                    ip.AddPoint(x1, y1, z1, v1);
                }
                data1.Clear();
                depth1.Clear();                
                v1 = ip.GetInterValue(x0, y0, z0);
                ip.Clear();
                return v1;
            }
            return 0;
        }
        private void loadRestricButton1_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "las list file(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (LoadLayerControl(dlg.FileName))
                        {
                            UpdateDataGridview();
                        }
                        else MessageBox.Show(AppLocalization.IsChinese ? "加载图层控制数据失败。" : "Load layer control data failed.");
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
