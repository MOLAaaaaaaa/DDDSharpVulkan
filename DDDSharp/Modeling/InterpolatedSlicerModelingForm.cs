using DataCollection;
using OpenCLNet;
using DDDSharp.DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCLNet;
using System.Threading;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Xml.Linq;
using CLInterpolation;
using System.IO;

namespace DDDSharp.Modeling
{
    public partial class InterpolatedSlicerModelingForm : Form
    {
        public C3DGridData grid3d = null;
        public List<PolygonSlicer> Slicers = new List<PolygonSlicer>();
        StratumDatas layers = C3DData.Stratums;
        public List<TriangleObj>createdModels = new List<TriangleObj>();        
        string errMessage = "";
        int xNum = 201, yNum=201, zNum = 101;
        int ColorPickDiff = 5;
        bool NearestOnly = false;
        bool InvalidFilter = false; //过滤地层无效点
        double sampleStepX=10, sampleStepY=10;
        
        double SearchingStep = 10;
        
        SlicerInterpolation ip;
        IDWInterpolator idw;
        delegate void UpdateProgressUI(string text);

        StratumInterpolationFromImageRecognize ip1;

        string Infomation = "";
        bool threadStoped = false;
        
        double minx, miny, minz;
        double maxx, maxy, maxz;
        int maxwidth, maxheight;//图片的尺寸
        public InterpolatedSlicerModelingForm()
        {
            InitializeComponent();
            textBox1.Text = xNum.ToString();
            textBox2.Text = yNum.ToString();
            textBox3.Text = zNum.ToString();            
            ColorDiffTextBox.Text = ColorPickDiff.ToString();
            checkBox1.Checked = NearestOnly;
            checkBox2.Checked = InvalidFilter;
        }

        public InterpolatedSlicerModelingForm(List<PolygonSlicer>_slicers)
        {
            InitializeComponent();
            textBox1.Text = xNum.ToString();
            textBox2.Text = yNum.ToString();
            textBox3.Text = zNum.ToString();            
            ColorDiffTextBox.Text = ColorPickDiff.ToString();
            for(int i=0;i<_slicers.Count;i++) 
            {
                if( _slicers[i].Visible) Slicers.Add(_slicers[i]);
            }
            
        }
        public void AddSlicer(PolygonSlicer slicer)
        {
            Slicers.Add(slicer);
        }
        void UpdateDataRange()
        {
            for (int i = 0; i < Slicers.Count; i++)
            {
                PolygonSlicer s = Slicers[i];
                if (i == 0)
                {
                    minx = s.Minx;
                    maxx = s.Maxx;
                    miny = s.Miny;
                    maxy = s.Maxy;
                    minz = s.Minz;
                    maxz = s.Maxz;
                    if( s.BackgroundImage!=null )
                    {
                        maxwidth = s.BackgroundImage.Width;
                        maxheight = s.BackgroundImage.Height;
                    }
                }
                else
                {
                    if (s.Minx < minx) minx = s.Minx;
                    if (s.Miny < miny) miny = s.Miny;
                    if (s.Minz < minz) minz = s.Minz;
                    if (s.Maxx > maxx) maxx = s.Maxx;
                    if (s.Maxy > maxy) maxy = s.Maxy;
                    if (s.Maxz > maxz) maxz = s.Maxz;
                    if (s.BackgroundImage != null)
                    {
                       if(s.BackgroundImage.Width> maxwidth) 
                            maxwidth = s.BackgroundImage.Width;
                       if(s.BackgroundImage.Height> maxheight)
                            maxheight= s.BackgroundImage.Height;                        
                    }
                }
            }            
        }
        private void LoadSlicerButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Slicers(*.Slicer)|*.Slicer|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Slicers.Clear();
                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            PolygonSlicer slicer = new PolygonSlicer();
                            if (slicer.LoadFrom(dlg.FileNames[i]))
                            {
                                Slicers.Add(slicer);
                            }
                        }
                        SortSlicers();
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
                
                UpdateList1();
                UpdateList2();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= Slicers.Count) return;
            Slicers.RemoveAt(sel);
            listBox1.Items.RemoveAt(sel);
            if (listBox1.Items.Count > sel) listBox1.SelectedIndex = sel;
            
            UpdateList2();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            Slicers.Clear();
            layers.Clear();
            
            listBox1.Items.Clear();
            listBox2.Items.Clear();
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel <= 0) return;
            PolygonSlicer cur = Slicers[sel];
            Slicers[sel] = Slicers[sel - 1];
            Slicers[sel - 1] = cur;
            UpdateList1();
            listBox1.SelectedIndex = sel - 1;
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= Slicers.Count) return;

            PolygonSlicer cur = Slicers[sel];
            Slicers[sel] = Slicers[sel + 1];
            Slicers[sel + 1] = cur;
            UpdateList1();
            listBox1.SelectedIndex = sel + 1;
        }
        
        private void UpdateList1()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < Slicers.Count; i++)
            {
                listBox1.Items.Add(Slicers[i].Name);
            }
            listBox1.SelectedItems.Clear();
        }

        private void UpdateList2()
        {
            listBox2.Items.Clear();

            for (int i = 0; i < layers.Count; i++)
            {
                listBox2.Items.Add(layers[i].Name);
            }
            listBox2.SelectionMode = SelectionMode.MultiExtended;
            listBox2.SelectedItems.Clear();
        }
        void SortSlicers()
        {
           // Slicers.Sort((x, y) => x.Name.CompareTo(y.Name));
        }
        private void InterpolatedSlicerModelingForm_Load(object sender, EventArgs e)
        {
            SortSlicers();
            UpdateDataRange();
            UpdateList1();
            UpdateList2();
            UpdateSampleStepInfo();
        }

        void UpdateProgress(string text)
        {
            if (progressBar1.InvokeRequired)
            {
                UpdateProgressUI outdelegate = new UpdateProgressUI(UpdateProgress);
                this.BeginInvoke(outdelegate,text);
                return;
            }
            
            double percent1 = ip1.percentage;
            if (percent1 >= 100) percent1 = 100;
            progressBar1.Value = (int)percent1;           

            string str = Math.Round(percent1, 2).ToString() + "%";
            progressLabl.Text = text + ": " + str;
            TotalTimesLabel.Text = Infomation;
        }

        private void InterpolatingThread22(object obj)
        {
            ip1.Interpolating();
            grid3d = ip1.grid3d;
            Thread.Sleep(300);
            ip1.Clear();
            threadStoped = true;
        }
        private void InterpolatingThread(object obj)
        {
            string dataFileName = "";
            int[,] sample1=null, sample2 = null;            
            
            for (int i = 0; i < Slicers.Count - 1; i++)
            {
                DateTime t1 = DateTime.Now;
                
                Infomation = "第" + (i+1)+"次/共 " + (Slicers.Count-1)+"次";
                PolygonSlicer slicer1 = Slicers[i];
                PolygonSlicer slicer2 = Slicers[i+1];
                dataFileName = (i+1).ToString()+ "-" + slicer1.Name + "_" + slicer2.Name + ".3DGrid";
                ip.Slicer1 = slicer1;
                ip.Slicer2 = slicer2;
                
                if (i == 0)
                {
                    ip.progressTitle = "正在创建网格...";
                    ip.CreateGrid(xNum, yNum, zNum, C3DData.Stratums);
                    sample1 = ip.SamplingFromImage(slicer1.BackgroundImage, xNum, yNum);
                    sample2 = ip.SamplingFromImage(slicer2.BackgroundImage, xNum, yNum);
                    ip.slicerSampled1 = sample1;
                    ip.slicerSampled2 = sample2;
                }
                else 
                {
                    ip.ResetStratumGrid();
                    sample1 = sample2;
                    sample2 = ip.SamplingFromImage(slicer2.BackgroundImage, xNum, yNum);
                    ip.slicerSampled1 = sample1;
                    ip.slicerSampled2 = sample2;
                }

                ip.CreateModelThread();
                ip.dataGrid3D.SaveAs(@"C:\jian\2024\简楚\攀枝花\白马\勘探剖面线\Slicers\" + dataFileName,13);
                ip.dataGrid3D.ResetGridData(-1);
                DateTime t2 = DateTime.Now;
                
                double ms = (Slicers.Count-1 - i )*(t2 - t1).TotalSeconds;
                Infomation += " 剩余时间：" + new TimeSpan((long)(ms*1E7)).ToString(@"hh\:mm\:ss");
            }
            
            Thread.Sleep(300);
            ip.Clear();
            threadStoped = true;
        }
        private void CheckThread(object obj)
        {
            StratumInterpolationFromImageRecognize _ip = obj as StratumInterpolationFromImageRecognize;
            while ( !threadStoped )
            {
                UpdateProgress(_ip.progressTitle);                
                Thread.Sleep( 200 );
            }           
        }

        private void SamplingInterpolation_Click(object sender, EventArgs e)
        {
            Start();
        }
        private void InterpolatingThread1(object obj)
        {
            ip.CreateModelThread1();            
            Thread.Sleep(300);
            ip.Clear();
            threadStoped = true;
        }

        private void GPUInterpolation_Click(object sender, EventArgs e)
        {
            idw = new IDWInterpolator();
            for(int i=0;i<ip.points.Count;i++)
            {
                idw.AddPoint(ip.points[i]);
            }
            
        }
        void UpdateSampleStepInfo()
        {
            double dx, dy;
            if (SampleByPixelCheckBox.Checked)
            {
                dx = (int)(maxwidth / 100.0);
                dy = (int)(maxheight / 100.0);
            }
            else
            {
                dx = (maxx - minx) / 100;
                dy = (maxy - miny) / 100;
            }
            ColorSampleStepX.Text = dx.ToString();
            ColorSampleStepY.Text = dy.ToString();
        }
        private void SampleByPixelCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSampleStepInfo();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (Slicers.Count < 1) return;
            if (C3DData.Stratums.Count < 1) 
            {
                MessageBox.Show("No Stratum Color Scheme.");
                return; 
            }
            var dlg = new SaveFileDialog();
            dlg.Filter = Resource1.XYZVFileFormatFilter;
            dlg.Filter += "|" + "All Files(*.*)|*.*";
            if (dlg.ShowDialog() != DialogResult.OK) return;
            string filename = dlg.FileName;
            dlg.Dispose();

            int nx, ny;
            sampleStepX = double.Parse(ColorSampleStepX.Text);
            sampleStepY = double.Parse(ColorSampleStepY.Text);
            int colordiff = int.Parse(ColorDiffTextBox.Text);
            double dist = 0;
            Vector64 p1, p2;
            
            Cursor = Cursors.WaitCursor;

            for (int i=0;i<Slicers.Count;i++)
            {
                PolygonSlicer slicer = Slicers[i];                
                if (SampleByPixelCheckBox.Checked)
                {
                    nx = (int)(slicer.BackgroundImage.Width / (sampleStepX));
                    ny = (int)(slicer.BackgroundImage.Height / (sampleStepY));
                }
                else
                {
                    if (slicer.axis == AxisEnum.zAxis)//Y向是z
                    {
                        p1 = new Vector64(slicer.Minx, slicer.Miny, slicer.Minz);
                        p2 = new Vector64(slicer.Maxx, slicer.Maxy, slicer.Minz);                        
                    }
                    else //if (slicer.axis == AxisEnum.yAxis)//Y向是y
                    {
                        p1 = new Vector64(slicer.Minx, slicer.Miny, slicer.Minz);
                        p2 = new Vector64(slicer.Maxx, slicer.Miny, slicer.Minz);                        
                    }
                    dist = p2.Distance(p1);
                    nx = (int)(dist / (sampleStepX));
                    ny = (int)(dist / (sampleStepY));
                }
                SlicerSampling(slicer,nx, ny,filename, colordiff,i);
            }
            Cursor = Cursors.Default;

            MessageBox.Show("points of slicers sampled exported.");
        }
        void SlicerSampling(PolygonSlicer slicer, int nx,int ny,string filename,int colordiff,int index)
        {           
            if ( C3DData.Stratums.Count < 1 ) return;            
            int[,] grid = C3DData.Stratums.SamplingFromImage(slicer.BackgroundImage, nx, ny, colordiff);
            double x, y, dx, dy;
            dx = (slicer.maxx - slicer.minx) / (nx - 1);
            dy = (slicer.maxy - slicer.miny) / (ny - 1);
            Vector64 p;
            StreamWriter br;
            if (index == 0) 
            { 
                br = new StreamWriter(new FileStream(filename, FileMode.Create));
                br.WriteLine("X,Y,Z,Value");
            }
            else br = new StreamWriter(new FileStream(filename, FileMode.Append));
            for (int iy = 0; iy < ny; iy++)
            {
                y = slicer.miny + iy * dy;
                for (int ix = 0; ix < nx; ix++)
                {
                    x = slicer.minx + ix * dx;
                    p = slicer.toTracedPoint(new Vector64(x, y, 0, 0));
                    p.V = grid[ix, iy] + 1;
                    br.WriteLine(p.toString(4));
                }
            }
            br.Close();            
        }

        void StartSamplingInterpolation()
        {
            if (Slicers.Count < 2) return;
            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }           
        }
        void Start()
        {
            if (Slicers.Count < 2) return;
            try
            {
                //progressBar1.Visible = true;                
                xNum = int.Parse(textBox1.Text);
                yNum = int.Parse(textBox2.Text);
                zNum = int.Parse(textBox3.Text);
                ColorPickDiff = int.Parse(ColorDiffTextBox.Text);
                NearestOnly = checkBox1.Checked;
                InvalidFilter = checkBox2.Checked;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            UpdateDataRange();

            ip1 = new StratumInterpolationFromImageRecognize();
            ip1.NearestOnly = NearestOnly;
            ip1.InvalidFilter = InvalidFilter;
            ip1.xGrid = xNum;
            ip1.yGrid = yNum;
            ip1.zGrid = zNum;
            ip1.minx = minx; ip1.maxx = maxx;
            ip1.miny = miny; ip1.maxy = maxy;
            ip1.minz = minz; ip1.maxz = maxz;
            ip1.Slicers = Slicers;
            //ip1.ColorPickDiff = ColorPickDiff;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            progressBar1.Value = 0;

            threadStoped = false;

            Thread thread1 = new Thread(InterpolatingThread22);
            thread1.Start(ip1);
            Thread thread2 = new Thread(CheckThread);
            thread2.Start(ip1);
        }
        private void CreateButton_Click(object sender, EventArgs e)
        {
            Start();            
        }

        int SearchingIn(double x,double y,double z,List<C3DGridData> objects)
        {
            for(int i=0;i< objects.Count;i++)
            {
                C3DGridData data = objects[i];
                if (x < data.Minx || x > data.Maxx ||
                    y < data.Miny || y > data.Maxy ||
                    z < data.Minz || z > data.Maxz) continue;
                return i;
            }
            return -1;
        }


        private void Imerging_Click(object sender, EventArgs e)
        {
            C3DGridData data;
            List<string> files = new List<string>();
            List<C3DGridData> objects = new List<C3DGridData>();

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "3DGrid(*.3DGrid)|*.3DGrid|all files(*.*)|*.*";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {                   
                    for (int i = 0; i < dlg.FileNames.Length; i++)
                    {
                        data = new C3DGridData();
                        if (data.LoadFrom(dlg.FileNames[i]))
                        {
                            objects.Add(data);
                        }
                    }
                }//if (dlg.ShowDialog() == DialogResult.OK)
            }//using (var dlg = new OpenFileDialog())
             //
            for( int i = 0; i < objects.Count; i++ )
            {
                data = objects[i];                
                if (i == 0) 
                { 
                    minx = data.Minx;
                    maxx = data.Maxx;
                    miny = data.Miny;
                    maxy = data.Maxy;
                    minz = data.Minz;
                    maxz = data.Maxz;
                }
                else
                {
                    if (data.Minx < minx) minx = data.Minx;
                    if (data.Miny < miny) miny = data.Miny;
                    if (data.Minz < minz) minz = data.Minz;
                    if (data.Maxx > maxx) maxx = data.Maxx;
                    if (data.Maxy > maxy) maxy = data.Maxy;
                    if (data.Maxz > maxz) maxz = data.Maxz;
                }                
            }

            double x, y, z;
            int nx = 201, ny=201, nz = 201;
            C3DGridData data1 = new C3DGridData(nx,ny,nz,float.NaN);
            data1.ResetDataRange(minx, maxx, miny,maxy,minz,maxz,0,9);

            //for (int i = 0; i < objects.Count; i++)
            //{
            //    data = objects[i];
            //    for(int k = 0;k<data.Length;k++)
            //    {
            //        Vector32 p = data.GetGridCoord(k);
            //        Int32XYZ xyz = data1.GetIndices(p.toVector64(),0.001);
            //        data1[xyz.x, xyz.y, xyz.z] = p.V;
            //    }
            //}            
            //data1.GridsSampledInterpolated();

            for (int iz = 0; iz < nz; iz++)
            {
                z = data1.minz + iz * data1.zStep;
                for (int iy = 0; iy < ny; iy++)
                {
                    y = data1.miny + iy * data1.yStep;
                    for (int ix = 0; ix < nx; ix++)
                    {
                        x = data1.minx + ix*data1.xStep;
                        int k = SearchingIn(x, y, z, objects);
                        if (k < 0) data1[ix, iy, iz] = 0;
                        else data1[ix, iy, iz] = (float)objects[k].GetGridValue(x,y,z,false,false);
                    }
                }
            }

            data1.minv = objects[0].minv;
            data1.maxv = objects[0].maxv;
            data1.ColorScale = objects[0].ColorScale;
            data1.SaveAs(@"C:\jian\2024\简楚\攀枝花\白马\勘探剖面线\Slicers\" + "all.3DGrid", 11);
            data1.Clear();
            for (int i = 0; i < objects.Count; i++) 
            {
                objects[i].Clear();
            }
            objects.Clear();
            MessageBox.Show("Done!!!");
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if(grid3d != null)
            {
                Cursor = Cursors.WaitCursor;
                grid3d.InitTables();        
                Cursor = Cursors.Default;
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
