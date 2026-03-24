using System;
using System.Threading;
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
using OpenCLNet;
using CLInterpolation;
using DDDSharp.Dialogs;
using System.Security.Policy;

namespace DDDSharp
{    
    public partial class GriddingForm : Form
    {
        EncodingInfo[] encodingInfos = null;
        public Encoding encoding = Encoding.UTF8;
        public C3DGridData loaded3DGrid = null;
        public int CodePage
        {
            get { return encoding.CodePage; }
            set
            {
                encoding = Encoding.GetEncoding(value);
            }
        }

        List<Vector32> pOrg = new List<Vector32>();
        
        ColumnDataList pDataList = new ColumnDataList();

        bool pointsTransformed = false; //是否需要重新读取数据
        int select1, select2, select3, select4;
        static int nx, ny, nz;
        static double xstep, ystep, zstep;
        static double minx, maxx, miny, maxy, minz, maxz, minv, maxv;
        double nullValue = CSurferGrid.blankValue;
        private string errMessage = "";

        static private bool workDisposed = false;
        private System.Object UpdateLock = new System.Object();
        static private System.Object InterpolateLock = new System.Object();
        
        bool doGeometryChange = true;

        protected char[] remarkChars = new char[] { '/', '#', '!' };
        protected char[] splitChars = new char[] { ' ', ',', '\t' };
        protected char[] trimChars = new char[] { ' ', '\t', '"' };

        private bool Initializing = false;

        double memrequired = 0;
        double memavailable = 0;

        int progressState = 0;// 0没开始，1已开始，2已暂停        

        Thread globalThread = null;
        Thread checkThread = null;

        //GPU Interpolation 
        List<Device> devices = new List<Device>();
        List<Device> selectedDevices = new List<Device>();
        //InterpolatorBase ip = new InterpolatorBase();
        InterpolatorBase ipmethod = null;// new InterpolatorBase();

        static C3DGridData data = new C3DGridData();
        private double percentage = 0;
        private string progressTitle = "";
        private string timeLeftTitle = "";
        private string timeSlipedTitle = "";
        private bool checkProgress = false;
        private const int XNUM = 101;
        private const int YNUM = 101;
        private const int ZNUM = 101;
        
        int floatNum = 6;

        public delegate void OnUpdateUI();

        public GriddingForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            select1 = 0;
            select2 = 1;
            select3 = 2;
            select4 = 3;
        }       
        private void textX1_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoRangeChange(0);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textX2_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoRangeChange(0);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textY1_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoRangeChange(1);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }

        private void textY2_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoRangeChange(1);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textZ1_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoRangeChange(2);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textZ2_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoRangeChange(2);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }

        private void textStepX_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoSpaceChange(0);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textStepY_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoSpaceChange(1);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textStepZ_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoSpaceChange(2);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textXNum_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoNumChange(0);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textYNum_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoNumChange(1);
            UpdateGeometry();
            UpdateMemoryStatusInfo();
        }
        private void textZNum_TextChanged(object sender, EventArgs e)
        {
            if (!doGeometryChange) return;
            DoNumChange(2);
            UpdateGeometry();
        }

        private void DoNumChange(int dir)
        {
            lock (UpdateLock)
            {
                if (!Initializing)
                {

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
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(0);
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(1);
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(2);
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelect(3);
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
        }
        private bool GetSelectIndex()
        {
            select1 = comboBox1.SelectedIndex;
            select2 = comboBox2.SelectedIndex;
            select3 = comboBox3.SelectedIndex;
            select4 = comboBox4.SelectedIndex;
            if (select1 < 0 || select2 < 0 || select3 < 0 || select4 < 0)
                return false;
            return true;
        }
        private bool GetPointValue(int index,ref Vector32 p)
        {
            select1 = comboBox1.SelectedIndex;
            select2 = comboBox2.SelectedIndex;
            select3 = comboBox3.SelectedIndex;
            select4 = comboBox4.SelectedIndex;
            if(select1<0 || select2 <0 || select3 <0 || select4<0)
            {
                return false;
            }
            p.X = pDataList[index, select1];
            p.Y = pDataList[index, select2];
            p.Z = pDataList[index, select3];
            p.V = pDataList[index, select4];

            return true;
        }
       
        private void InitProgressBar(int min = 0,int max = 100)
        {
            progressBar1.Minimum = min;
            progressBar1.Maximum = max;
            progressBar1.Step = 1;
            progressBar1.Value = 0;            
            percentage = 0;
            progressTitle = "";
            timeLeftTitle = "";
            progressBar1.Visible = true;
            labelTitle.Visible = true;
            labelTimeLeft.Visible = true;
        }
        //*********************************************************************************
        //*************This is for global interpolation************************************
        private void UpdateProgress()
        {
            if (progressBar1.InvokeRequired)
            {
                OnUpdateUI outdelegate = new OnUpdateUI(UpdateProgress);
                this.BeginInvoke(outdelegate);
                return;
            }
            double percent1 = percentage;
            if (percent1 >= 100) percent1 = 100;
            progressBar1.Value = (int)percent1;
            
            string str = Math.Round(percent1, 2).ToString() + "%";
            /*Font font = new Font("Times New Roman", (float)11, FontStyle.Regular);
            PointF pt = new PointF(this.progressBar1.Width / 2 - 10, this.progressBar1.Height / 2 - 10);
            this.progressBar1.CreateGraphics().DrawString(str, font, Brushes.Blue, pt);
            */
            labelTitle.Text = progressTitle + str;
            labelTimeLeft.Text = timeLeftTitle ;
            //labelTimeLeft.Text += timeSlipedTitle;

            UpdateButtonState();

            //finished ,hide it
            if (progressBar1.Value >= progressBar1.Maximum || workDisposed == true)
            {
                progressBar1.Visible = false;
                labelTitle.Text = "";
                labelTimeLeft.Text = "";
            }
            else
            {
                if(progressBar1.Visible == false)
                    progressBar1.Visible = true;
            }
        }
        private void ProgressStatusCheckThread()
        {
            while (checkProgress)
            {
                percentage = ipmethod.percentage;
                progressTitle = ipmethod.progressTitle;
                timeLeftTitle = ipmethod.formatTime(ipmethod.timeLeft);
                timeSlipedTitle = ipmethod.formatTime(ipmethod.timeSlip);
                UpdateProgress();
                Thread.Sleep(1000);
            }
        }
        private void InterpolationThread()
        {
            lock (InterpolateLock)
            {
                workDisposed = false;
                checkProgress = true;

                if ( ipmethod.method == InterpolationMethod.GriddedInterpolation)
                {
                    data.pGridData  = ipmethod.DirectGridding(nx, ny, nz);
                }
                else
                {
                    //  ipmethod.Normalize();
                    //重点检查
                    if ( ipmethod.progressStep < 1 )
                    {
                        if (ipmethod.method == InterpolationMethod.RadicalBasisFunction)
                        {
                            if (ipmethod.RemoveDuplicated() > 0) 
                                UpdateDataInfo();
                        }
                    }

                    if (selectedDevices.Count > 0) //GPU 并行计算
                        data.pGridData = ipmethod.GetInterpolatedValue(nx, ny, nz, selectedDevices.ToArray());//GPU version
                    else data.pGridData = ipmethod.GetInterpolatedValue(nx, ny, nz, null); //CPU version                    
                    if( !ipmethod.BigGridData ) data.UpdateRange();
                }

                percentage = 0;
                progressState = 0;
                checkProgress = false;
                workDisposed = true;
                UpdateProgress();
                Thread.Sleep(2000);
            }//lock (InterpolateLock)

            if (data.pGridData == null)
            {
                string info = "gridding failed!\n\n" + ipmethod.errMsg + "\n\n";
                info += "Time elapsed -- " + ipmethod.formatTime(ipmethod.timeSlip);
                MessageBox.Show(info);
                data = null;
                return;
            }
            bool ret = true;
            if (!ipmethod.BigGridData) ret = data.SaveAs(textOutputFile.Text);
            if ( ret )
            {
                string info = "Gridding successfully!\n\n";
                info += "Time elapsed -- " + ipmethod.formatTime(ipmethod.timeSlip);
                MessageBox.Show(info);
            }
            else
            {
                MessageBox.Show("save gridding data failed!" + data.errMessage);
            }
        }
        
       void TransformPointsFrom3DGrid(C3DGridData _data)
       {
            ipmethod.Clear();
            nx = _data.xNum;
            ny = _data.yNum;
            nz = _data.zNum;
            minx = _data.minx;
            miny = _data.miny;
            minz = _data.minz;
            minv = _data.minv;
            maxx = _data.maxx;
            maxy = _data.maxy;
            maxz = _data.maxz;
            maxv = _data.maxv;
            xstep = _data.xStep;
            ystep = _data.yStep;
            zstep = _data.zStep;            
            Vector32 p = new Vector32();
            for (int i = 0; i < _data.pGridData.Length; i++)
            {
                p = _data.GetGridCoord(i);
                ipmethod.AddPoint(p);
            }            
            pointsTransformed = true;
            ipmethod.SetGeometry(minx, maxx, miny, maxy, minz, maxz, xstep, ystep, zstep, nx, ny, nz);
            ipmethod.minv = minv;
            ipmethod.maxv = maxv;
        }
        //Get points data from List buffer pDataList
        /// <summary>
        /// 数据传输到ipmethod
        /// </summary>
        /// <param name="forced">是否强制传输</param>
        private void TransformPoints(bool forced = false)
        {
            select1 = comboBox1.SelectedIndex;
            select2 = comboBox2.SelectedIndex;
            select3 = comboBox3.SelectedIndex;
            select4 = comboBox4.SelectedIndex;
            if (select1 < 0 || select2 < 0 || 
                select3 < 0 || select4 < 0)
                return;

            //数据未更改，不需要重新读取            
            if ( !forced && pointsTransformed && ipmethod.pointCount > 0) return;

            ipmethod.Clear();
            
            int select5 = -1;
            RBFBoreholesInterpolation ip = null;
            if (ipmethod.method == InterpolationMethod.BoreholesMineralInterpolation)
            {
                ip = ipmethod as RBFBoreholesInterpolation;
                select5 = ip.boreholeColumn;
                if (select5 < 0 || select5 >= pDataList.Col) return;
            }

            Vector32 p = new Vector32();
            for (int i = 0; i < pDataList.Row; i++)
            {
                p.X = pDataList[i, select1];
                p.Y = pDataList[i, select2];
                p.Z = pDataList[i, select3];
                p.V = pDataList[i, select4];
                if (roundUpCheckBox.Checked)
                {
                    p.X = (float)Math.Round(p.X, floatNum);
                    p.Y = (float)Math.Round(p.Y, floatNum);
                    p.Z = (float)Math.Round(p.Z, floatNum);
                }
                ipmethod.AddPoint(p);
                if(ip != null) 
                {
                    short id = (short)(pDataList[i, select5] + 0.1);
                    ip.boreholeIndices.Add(id);
                }
            }
            pointsTransformed = true;
            ipmethod.SetGeometry(minx, maxx, miny, maxy, minz, maxz, xstep, ystep, zstep, nx, ny, nz);                       
        }
        
        private void StartButton_Click(object sender, EventArgs e)
        {   
            if(ipmethod.method == InterpolationMethod.GriddedInterpolation && loaded3DGrid!=null)
            {

            }
            else
            {
                if (pDataList.Row < 1 || pDataList.Col < 1)
                {
                    MessageBox.Show("No enough data.");
                    return;
                }
                select1 = comboBox1.SelectedIndex;
                select2 = comboBox2.SelectedIndex;
                select3 = comboBox3.SelectedIndex;
                select4 = comboBox4.SelectedIndex;
                if (select1 < 0 || select2 < 0 || select3 < 0 || select4 < 0)
                {
                    MessageBox.Show("no valid columns selected.");
                    return;
                }
                if (ipmethod.method == InterpolationMethod.BoreholesMineralInterpolation)
                {
                    RBFBoreholesInterpolation ip = ipmethod as RBFBoreholesInterpolation;
                    if (ip.boreholeColumn < 0)
                    {
                        MessageBox.Show("Please Set Borehole Column on Option.");
                        return;
                    }
                }
                if (!GetGeometryRange())
                {
                    MessageBox.Show("interpolation geometry is invalid.");
                    return;
                }

                TransformPoints();
            }                        

            try 
            {
                data = new C3DGridData(minx, maxx, miny, maxy, minz, maxz, minv, maxv);
                data.xNum = nx;
                data.yNum = ny;
                data.zNum = nz;
                
                ulong msize = ipmethod.GetRequiredMemorySizeOnCPU();
                memrequired = msize / 1024 / 1024;
                memavailable = PhysicalMemory.GetAvailableMemoryMB();

                if ( memrequired < memavailable )
                { 
                    data.pGridData = new float[nx * ny * nz];
                    //ipmethod.BigGridData = false;
                }
                else if( !ipmethod.BigGridData )
                {
                    MessageBox.Show("no enough memory available.");
                    ipmethod.Clear();
                    return;
                }
                ipmethod.DividedNum = 1;
                if (ipmethod.BigGridData)
                {
                    int divided = (int)(memrequired / memrequired) + 1;
                    ipmethod.DividedNum = divided > 4 ? divided: 4;                    
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("no enough memory,try to decrease grid numbers.\n" + ex.Message);
                ipmethod.Clear();
                return;
            }

            ipmethod.SetGeometry(minx,maxx,miny,maxy,minz,maxz,xstep,ystep,zstep,nx,ny,nz);

            #region 获取选定GPU设备 
            selectedDevices.Clear();
            for(int i=0;i<checkedListBox1.Items.Count;i++)
            {
                if( checkedListBox1.GetItemChecked(i) )
                {
                    selectedDevices.Add(devices[i]);
                }
            }
            #endregion 获取选定GPU设备

            
            ipmethod.threadStoped = false;

            //启动插值线程
            InitProgressBar(0, 100);

            ipmethod.gridDataFile = textOutputFile.Text;
            ipmethod.progressFile = textOutputFile.Text + ".prog";
            FileInfo fi = new FileInfo(ipmethod.progressFile);
            if( fi.Exists && ipmethod.method == InterpolationMethod.RadicalBasisFunction)
            {
                //Register Verify
                RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
                if (!reg.ReadFromRegister())
                {
                    MessageBox.Show("this is a unregistered version.");
                    return;
                }
                Random rand = new Random();
                RegisterAndEncrypt.HardWareInfo.InfoType type = (RegisterAndEncrypt.HardWareInfo.InfoType)rand.Next(3);
                if (!reg.Verify(type))
                {
                    MessageBox.Show("Unreconginized register information.");
                    return;
                }
                //Register Verify

                if ( MessageBox.Show("Previous work not completed,load the previous task ?","Load the previous task?",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes )
                {
                    Cursor = Cursors.WaitCursor;
                    ipmethod.LoadProgress(ipmethod.progressFile);
                    Cursor = Cursors.Default;
                }
            }
            if (globalThread != null && globalThread.IsAlive)
            {
                globalThread.Join();
                globalThread = null;
            }
            ipmethod.threadStoped = false;
            globalThread = new Thread(InterpolationThread);
            globalThread.Start();
            //启动进度线程
            if (checkThread == null || !checkThread.IsAlive)
            {
                checkProgress = true;
                checkThread = new Thread(ProgressStatusCheckThread);
                checkThread.Start();
            }

            progressState = 1;// 0没开始，1已开始，2已暂停
        }
        private void UpdateButtonState()
        {
            if(progressState == 0)
            {
                StartButton.Text = "Start";
                StartButton.Enabled = true;

                PauseButton.Text = "Pause";
                PauseButton.Enabled = false;
                SaveAsButton.Enabled = false;

                StopButton.Text = "Stop";
                StopButton.Enabled = false;
                progressBar1.Value = 0;
                progressBar1.Visible = false;
                labelTimeLeft.Visible = false;
                labelTitle.Visible = false;
            }
            else if (progressState == 1) //started
            {
                StartButton.Text = "Start";
                StartButton.Enabled = false;

                PauseButton.Text = "Pause";
                PauseButton.Enabled = true;
                SaveAsButton.Enabled = false;

                StopButton.Text = "Stop";
                StopButton.Enabled = true;
                progressBar1.Visible = true;
                labelTimeLeft.Visible = true;
                labelTitle.Visible = true;
            }
            else if (progressState == 2) //paused
            {
                StartButton.Text = "Start";
                StartButton.Enabled = false;

                PauseButton.Text = "Resume";
                PauseButton.Enabled = true;
                SaveAsButton.Enabled = true;

                StopButton.Text = "Stop";
                StopButton.Enabled = true;
                progressBar1.Visible = true;
                labelTimeLeft.Visible = true;
                labelTitle.Visible = true;
            }
        }
        private void PauseContinue_Click(object sender, EventArgs e)
        {
            if (progressState == 1)
            {
                if (globalThread != null && globalThread.IsAlive)               
#pragma warning disable CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    globalThread.Suspend();
#pragma warning restore CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                  progressState = 2;
            }
            else if (progressState == 2)
            {
                if (globalThread != null && globalThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    globalThread.Resume();
#pragma warning restore CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                
                progressState = 1;
            }
            
            UpdateButtonState();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if ( progressState < 1) return;

            if (globalThread != null && globalThread.IsAlive)
                globalThread.Suspend();            

            if (MessageBox.Show("Are you sure to abort current progress?\n Works have done will not be saved.", "Abort the progress?",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                globalThread.Resume();                
                return; 
            }
            
            if ( ipmethod.progressStep > 0 )
            {
                if (MessageBox.Show("work not completed,save the task?", "Save uncompleted task?",
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Cursor = Cursors.WaitCursor;
                    ipmethod.SaveProgress(ipmethod.progressFile);
                    Cursor = Cursors.Default;
                }
            }

            globalThread.Resume();            
            ipmethod.threadStoped = true;
            checkProgress = false;

            workDisposed = true;
            Thread.Sleep(1200);

            ipmethod.Clear();
            progressState = 0;
            UpdateButtonState();
        }

        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            if ( progressState == 2 && ipmethod.progressStep > 0 )//暂停状态
            {
                Cursor = Cursors.WaitCursor;
                SaveProgress();
                Cursor = Cursors.Default;
                MessageBox.Show(errMessage);
            }

            UpdateButtonState();
        }       

       
       
        private void GriddingForm_Load(object sender, EventArgs e)
        {
            encodingInfos = Encoding.GetEncodings();
            for (int i = 0; i < encodingInfos.Length; i++)
            {
                encodeComboBox.Items.Add(encodingInfos[i].DisplayName);
                if (CodePage == encodingInfos[i].CodePage)
                    encodeComboBox.SelectedIndex = i;
            }            

            XFilterCombox.Items.Add("==");
            XFilterCombox.Items.Add("<");            
            XFilterCombox.Items.Add(">");
            XFilterCombox.Items.Add("between");
            XFilterCombox.SelectedIndex = -1;

            YFilterCombox.Items.Add("==");
            YFilterCombox.Items.Add("<");
            YFilterCombox.Items.Add(">");
            YFilterCombox.Items.Add("between");
            YFilterCombox.SelectedIndex = -1;

            ZFilterCombox.Items.Add("==");
            ZFilterCombox.Items.Add("<");
            ZFilterCombox.Items.Add(">");
            ZFilterCombox.Items.Add("between");
            ZFilterCombox.SelectedIndex = -1;
            
            VFilterCombox.Items.Add("==");
            VFilterCombox.Items.Add("<");
            VFilterCombox.Items.Add(">");
            VFilterCombox.Items.Add("between");
            VFilterCombox.SelectedIndex = -1;

            roundUpCheckBox.Checked = true;
            decimalsNumTextBox.Text = floatNum.ToString();

            string[] names = Enum.GetNames(typeof(InterpolationMethod));
            foreach(string s in names )
            {
                methodComboBox.Items.Add(s);
            }

            
            methodComboBox.SelectedIndex = 0;

            devices = OpenCLObj.GetDevices( DeviceType.ALL);
            string text;
            //double gb = 1024 * 1024*1024;
            for (int i = 0; i < devices.Count; i++)
            {
                text = devices[i].Name;
                text += ", ";
                text += Math.Round(OpenCLObj.GetMaxAllocMemoryMB(devices[i]),0);
                text += "MB, Items:";
                text += devices[i].MaxWorkGroupSize;
                checkedListBox1.Items.Add(text);
            }
            UpdateButtonState();
        }

        private void encodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if( encodeComboBox.SelectedIndex > 0 )
            {
                int code = encodingInfos[encodeComboBox.SelectedIndex].CodePage;
                if (code != CodePage)
                {
                    CodePage = code;
                    if (textInputFile.Text.Length > 0)
                    { 
                        if( ReadFromFile(textInputFile.Text))
                        {
                            InitSelect();
                            UpdateGeometry();
                            TransformPoints();
                            UpdateDataInfo();
                        }
                    }
                }
            }
            
        }

        private void outBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.Grid3DFileFormatFilter;
                dlg.Filter += "| All Files(*.*)|*.*";
                if (textOutputFile.Text.Length > 0)
                {
                    dlg.FileName = textOutputFile.Text;
                }
                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textOutputFile.Text = dlg.FileName;
                }
            }
        }

        private void DoSpaceChange(int dir)
        {
            lock (UpdateLock)
            {
                if (!Initializing)
                {
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
                        if ( ConvertToDouble(textStepZ.Text, out zstep))
                            nz = (int)((maxz - minz) / zstep) + 1;
                    }
                }
            }       
        }
        /// <summary>
        /// 过滤数据
        /// </summary>
        /// <param name="col">0 X,1 Y,2 Z,3 V</param>
        /// <returns></returns>
        bool Filter(int col,bool inverse = false)
        {
            int sel = -1;
            int selcol = -1;
            string v1str = "";
            string v2str = "";
            float v1 = 0, v2 = 0;
            if (col == 0) {v1str = X1Text.Text; v2str = X2Text.Text; sel = XFilterCombox.SelectedIndex; selcol = select1; }
            if (col == 1) {v1str = Y1Text.Text; v2str = Y2Text.Text; sel = YFilterCombox.SelectedIndex; selcol = select2; }
            if (col == 2) {v1str = Z1Text.Text; v2str = Z2Text.Text; sel = ZFilterCombox.SelectedIndex; selcol = select3; }
            if (col == 3) {v1str = V1Text.Text; v2str = V2Text.Text; sel = VFilterCombox.SelectedIndex; selcol = select4; }
            if (sel < 0) return false;

            if (!float.TryParse(v1str, out v1))
            {
                MessageBox.Show("Invalid Filter Value.");
                return false;
            }
            if( sel ==3)
            {
                if (!float.TryParse(v2str, out v2))
                {
                    MessageBox.Show("Invalid Filter Value.");
                    return false;
                }
            }

            //filter begin
            if (sel == 0) //" == "
            {
                pDataList.EqualFilter(selcol,v1,inverse);
                return true;
            }
            if (sel == 1) //" < "
            {
                pDataList.LowerFilter(selcol, v1, inverse);
                return true;
            }
            if (sel == 2) //" < "
            {
                pDataList.GreaterFilter(selcol, v1, inverse);
                return true;
            }
            if (sel == 3) //" between "
            {
                pDataList.BetweenFilter(selcol, v1,v2, inverse);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 替换数据
        /// </summary>
        /// <param name="col">0 X,1 Y,2 Z,3 V</param>
        /// <returns></returns>
        bool Replace(int col,double value)
        {
            int sel = -1;
            int selcol = -1;
            string v1str = "";
            string v2str = "";
            float v1 = 0, v2 = 0;
            if (col == 0) { v1str = X1Text.Text; v2str = X2Text.Text; sel = XFilterCombox.SelectedIndex; selcol = select1; }
            if (col == 1) { v1str = Y1Text.Text; v2str = Y2Text.Text; sel = YFilterCombox.SelectedIndex; selcol = select2; }
            if (col == 2) { v1str = Z1Text.Text; v2str = Z2Text.Text; sel = ZFilterCombox.SelectedIndex; selcol = select3; }
            if (col == 3) { v1str = V1Text.Text; v2str = V2Text.Text; sel = VFilterCombox.SelectedIndex; selcol = select4; }
            if (sel < 0) return false;

            if (!float.TryParse(v1str, out v1))
            {
                MessageBox.Show("Invalid Value: " + v1str);
                return false;
            }
            if (sel == 3)
            {
                if (!float.TryParse(v2str, out v2))
                {
                    MessageBox.Show("Invalid Value: "+ v1str);
                    return false;
                }
            }

            //filter begin
            if (sel == 0) //" == "
            {
                pDataList.EqualReplace(selcol, v1,value);
                return true;
            }
            if (sel == 1) //" < "
            {
                pDataList.LowerReplace(selcol, v1, value);
                return true;
            }
            if (sel == 2) //" < "
            {
                pDataList.GreaterReplace(selcol, v1, value);
                return true;
            }
            if (sel == 3) //" between "
            {
                pDataList.BetweenReplace(selcol, v1, v2, value);
                return true;
            }
            return false;
        }
        private bool GetGeometryRange()
        {
            if ( !ConvertToDouble(textX1.Text, out minx) ||
                 !ConvertToDouble(textX2.Text, out maxx) ||
                 !ConvertToDouble(textY1.Text, out miny) ||
                 !ConvertToDouble(textY2.Text, out maxy) ||
                 !ConvertToDouble(textZ1.Text, out minz) ||
                 !ConvertToDouble(textZ2.Text, out maxz) )
                return false;

            return true;

        }

        //创建插值类
        void CreateInterpolationMethod(bool update = true)
        {
            if (ipmethod == null || update == true) //重新生成
            {
                InterpolationMethod method = (InterpolationMethod)Enum.Parse(typeof(InterpolationMethod), methodComboBox.SelectedItem.ToString());
                if (method == InterpolationMethod.InverseDistanceWeighted)
                {
                    IDWInterpolator ip = new IDWInterpolator();                    
                    ip.IsMultiThread = true;
                    if ( ipmethod!= null && ipmethod.pointCount > 0)
                    {
                        ip.CopyFrom(ipmethod);
                        ipmethod.Clear();                        
                    }
                    ip.gridDataFile = textOutputFile.Text;
                    ipmethod = ip;
                }                
                else if (method == InterpolationMethod.RadicalBasisFunction)
                {
                    RBFInterpolation ip = new RBFInterpolation();                   
                    if (ipmethod != null && ipmethod.pointCount > 0)
                    {
                        ip.CopyFrom(ipmethod);
                        ipmethod.Clear();                       
                    }
                    ip.gridDataFile = textOutputFile.Text;
                    ipmethod = ip;
                }
                else if (method == InterpolationMethod.BoreholesMineralInterpolation)
                {
                    RBFBoreholesInterpolation ip = new RBFBoreholesInterpolation();
                    if (ipmethod != null && ipmethod.pointCount > 0)
                    {
                        ip.CopyFrom(ipmethod);
                        ipmethod.Clear();
                    }
                    ip.gridDataFile = textOutputFile.Text;
                    ipmethod = ip;
                    pointsTransformed = false;
                }
                else if (method == InterpolationMethod.Linear)
                {
                    LinearInterpolator ip = new LinearInterpolator();
                    if (ipmethod != null && ipmethod.pointCount > 0)
                    {
                        ip.CopyFrom(ipmethod);
                        ipmethod.Clear();                        
                    }
                    ip.gridDataFile = textOutputFile.Text;
                    ipmethod = ip;
                }
                else if (method == InterpolationMethod.GriddedInterpolation)
                {
                    GriddedInterpolator ip = new GriddedInterpolator();
                    if (ipmethod != null && ipmethod.pointCount > 0)
                    {
                        ip.CopyFrom(ipmethod);                      
                        ip.floatNum = floatNum;
                        if (loaded3DGrid == null)
                        {
                            //更新geometry
                            float[] xgrids = ip.CreateGridsFromPoints(0);
                            float[] ygrids = ip.CreateGridsFromPoints(1);
                            float[] zgrids = ip.CreateGridsFromPoints(2);
                            nx = xgrids.Length;
                            ny = ygrids.Length;
                            nz = zgrids.Length;
                            xstep = Math.Round((maxx - minx) / (nx - 1), floatNum);
                            ystep = Math.Round((maxy - miny) / (ny - 1), floatNum);
                            zstep = Math.Round((maxz - minz) / (nz - 1), floatNum);
                        }
                        if ( nx*ny*nz != ip.pointCount )
                        {
                            MessageBox.Show("Not a Gridded data","Warning",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        ipmethod.Clear();
                        UpdateGeometry();
                    }
                    ip.gridDataFile = textOutputFile.Text;
                    if (loaded3DGrid != null) ip.grid3d0 = loaded3DGrid;
                    ipmethod = ip;                    
                }
            }
            
        }
        private void methodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateInterpolationMethod(true);
            UpdateMemoryStatusInfo();
        }

        private void Methodbutton1_Click(object sender, EventArgs e)
        {
            InterpolationMethod method = (InterpolationMethod)Enum.Parse(typeof(InterpolationMethod), methodComboBox.SelectedItem.ToString());
            if ( method ==  InterpolationMethod.InverseDistanceWeighted )
            {
                IDWSet id = new IDWSet();             
                id.ip = new IDWInterpolator();
                id.ip.CopyFromWithOutPoints(ipmethod);
                if (id.ShowDialog() == DialogResult.OK)
                {                    
                    ipmethod.CopyFromWithOutPoints(id.ip);                    
                }
            }
            else if (method == InterpolationMethod.GriddedInterpolation)
            {
                IDWSet id = new IDWSet();
                id.ip = new GriddedInterpolator();
                id.ip.CopyFromWithOutPoints(ipmethod);
                if (id.ShowDialog() == DialogResult.OK)
                {
                    ipmethod.CopyFromWithOutPoints(id.ip);
                    ((GriddedInterpolator)ipmethod).SearchingGridLength = ((GriddedInterpolator)id.ip).SearchingGridLength;
                }
            }
            else if (method == InterpolationMethod.RadicalBasisFunction)
            {
                IDWSet id = new IDWSet();
                id.ip = new RBFInterpolation();
                id.ip.CopyFromWithOutPoints(ipmethod);
                if (id.ShowDialog() == DialogResult.OK)
                {
                    ipmethod.CopyFromWithOutPoints(id.ip);                    
                }
            }
            else if (method == InterpolationMethod.BoreholesMineralInterpolation)
            {
                IDWSet id = new IDWSet();
                id.ip = new RBFBoreholesInterpolation();
                id.ip.CopyFromWithOutPoints(ipmethod);
                if (id.ShowDialog() == DialogResult.OK)
                {
                    ipmethod.CopyFromWithOutPoints(id.ip);
                }
            }
        }

        private void ResampleButton_Click(object sender, EventArgs e)
        {
            int resampleNX, resampleNY, resampleNZ;
            try 
            {
                resampleNX = Convert.ToInt32(ResampleNumX.Text);
                resampleNY = Convert.ToInt32(ResampleNumY.Text);
                resampleNZ = Convert.ToInt32(ResampleNumZ.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Invalid Resample Grids." + ex.Message);
                return;
            }                     

            if( !ipmethod.ResamplePoints(resampleNX, resampleNY, resampleNZ ) )
            {               
                MessageBox.Show("Failed to resample points.");
                return;
            }            

            UpdateDataInfo();
        }
        private void XFilterButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            Filter(0,InverseCheckBoxX.Checked);
            pDataList.UpdateRanges();
            ipmethod.Clear();
            
            UpdateSelect();
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }

        private void ReplaceXButton_Click(object sender, EventArgs e)
        {
            ReplaceWithInputForm dlg = new ReplaceWithInputForm();
            if( dlg.ShowDialog() != DialogResult.OK )
            {
                return;
            }
            
            double value = dlg.value;            
            Cursor = Cursors.WaitCursor;
            Replace(0, value);
            pDataList.UpdateRanges();
            ipmethod.Clear();
            TransformPoints(true);
            UpdateSelect();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }

        private void YFilterButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Filter(1,InverseCheckBoxY.Checked);
            pDataList.UpdateRanges();
            ipmethod.Clear();
            UpdateSelect();
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }
        private void ReplaceYButton_Click(object sender, EventArgs e)
        {
            ReplaceWithInputForm dlg = new ReplaceWithInputForm();
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            double value = dlg.value;
            Cursor = Cursors.WaitCursor;
            Replace(1, value);
            pDataList.UpdateRanges();
            ipmethod.Clear();
            TransformPoints();
            UpdateSelect();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }
        private void ZFilterButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Filter(2, InverseCheckBoxZ.Checked);
            pDataList.UpdateRanges();
            ipmethod.Clear();            
            UpdateSelect();
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }
        private void ReplaceZButton_Click(object sender, EventArgs e)
        {
            ReplaceWithInputForm dlg = new ReplaceWithInputForm();
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            double value = dlg.value;
            Cursor = Cursors.WaitCursor;
            Replace(2, value);
            pDataList.UpdateRanges();
            ipmethod.Clear();
            TransformPoints();
            UpdateSelect();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }
        private void VFilterButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Filter(3, InverseCheckBoxV.Checked);
            pDataList.UpdateRanges();
            ipmethod.Clear();
            UpdateSelect();
            TransformPoints();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }
        private void ReplaceVButton_Click(object sender, EventArgs e)
        {
            ReplaceWithInputForm dlg = new ReplaceWithInputForm();
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            double value = dlg.value;
            Cursor = Cursors.WaitCursor;
            Replace(3, value);
            pDataList.UpdateRanges();
            ipmethod.Clear();
            TransformPoints();
            UpdateSelect();
            UpdateGeometry();
            UpdateDataInfo();
            Cursor = Cursors.Default;
        }
        private void ExportButton_Click(object sender, EventArgs e)
        {
            if(ipmethod.pointCount < 1 )
            {
                MessageBox.Show("no valid points loaded.");
                return;
            }
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "data file (*.csv)|*.csv|data file (*.dat)|*.dat|data file (*.txt)|*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    if (SaveToFile(dlg.FileName))
                        MessageBox.Show("data exported to file: \n" + dlg.FileName);
                    else
                        MessageBox.Show("failed to export to file: \n" + dlg.FileName);
                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void GriddingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( data != null ) 
            {   
                data.Clear(); 
                data = null; 
            }
            pDataList.Clear();
            ipmethod.Clear();
            devices.Clear();
        }        

        private void DoRangeChange(int dir)
        {
            lock (UpdateLock)
            {
                if (!Initializing)
                {

                    if (dir == 0)
                    {
                        if ( ConvertToDouble(textX1.Text, out minx) &&
                             ConvertToDouble(textX2.Text, out maxx))
                                xstep = (maxx - minx) / (nx - 1);
                    }
                    if (dir == 1)
                    {
                        if ( ConvertToDouble(textY1.Text, out miny) &&
                            ConvertToDouble(textY2.Text, out maxy)) 
                            ystep = (maxy - miny) / (ny - 1);
                    }
                    if (dir == 2)
                    {
                        if ( ConvertToDouble(textZ1.Text, out minz) &&
                             ConvertToDouble(textZ2.Text, out maxz))
                            zstep = (maxz - minz) / (nz - 1);
                    }
                }
            }
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

        private void RemoveAllButton_Click(object sender, EventArgs e)
        {
            DialogResult ret = MessageBox.Show("Remove All data loaded?",
                                                "Clear data?",
                                                MessageBoxButtons.OKCancel,
                                                MessageBoxIcon.Question, 
                                                MessageBoxDefaultButton.Button2);
            if( ret == DialogResult.OK )
            {
                pOrg.Clear();
                pDataList.Clear();
                ipmethod.Clear();
                
                minx = maxx = 0;
                miny = maxy = 0;
                minz = maxz = 0;
                minv = maxv = 0;

                nx = ny = nz = 0;
                InitSelect();
                UpdateGeometry();
                UpdateDataInfo();
                UpdateBounds();                
            }
        }

        private void decimalsNumTextBox_TextChanged(object sender, EventArgs e)
        {
            if( !int.TryParse(decimalsNumTextBox.Text,out floatNum) )
            {
                MessageBox.Show("Invalid Integer Number.");
            }
        }

        private void decimalsNumTextBox_Validated(object sender, EventArgs e)
        {

        }

        private void EnableBigGrid_Click(object sender, EventArgs e)
        {
            ipmethod.BigGridData = true;
        }

        private void SaveGeomtryButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Geometry File(*.txt)|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK) 
            {
                StreamWriter wr = new StreamWriter(dlg.FileName);
                wr.WriteLine(textX1.Text);
                wr.WriteLine(textX2.Text);
                wr.WriteLine(textY1.Text);
                wr.WriteLine(textY2.Text);
                wr.WriteLine(textZ1.Text);
                wr.WriteLine(textZ2.Text);
                wr.WriteLine(textStepX.Text);
                wr.WriteLine(textStepY.Text);
                wr.WriteLine(textStepZ.Text);
                wr.WriteLine(textXNum.Text);
                wr.WriteLine(textYNum.Text);
                wr.WriteLine(textZNum.Text);
                wr.Close();
                MessageBox.Show("Saved to " + dlg.FileName);
            }
        }

        private void LoadGeomtryButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Geometry File(*.txt)|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                doGeometryChange = false;
                try 
                {
                    StreamReader sr = new StreamReader(dlg.FileName);
                    textX1.Text = sr.ReadLine();
                    textX2.Text = sr.ReadLine();
                    textY1.Text = sr.ReadLine();
                    textY2.Text = sr.ReadLine();
                    textZ1.Text = sr.ReadLine();
                    textZ2.Text = sr.ReadLine();
                    textStepX.Text = sr.ReadLine();
                    textStepY.Text = sr.ReadLine();
                    textStepZ.Text = sr.ReadLine();
                    textXNum.Text = sr.ReadLine();
                    textYNum.Text = sr.ReadLine();
                    textZNum.Text = sr.ReadLine();
                    sr.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Failed to Load geometry parameters." + Environment.NewLine +  ex.Message);
                }
                
                doGeometryChange = true;
            }
        }

        public bool IsNullValue(double value, double nullvalue, double zero = 0.0001)
        {
            if (value == nullvalue) return true;
            if (nullvalue == 0 && value <= zero) return true;
            if (Math.Abs(value - nullvalue) / Math.Abs(nullvalue) <= zero) return true;
            else return false;
        }

        private bool ProcessLine( string[] ss )
        {
            bool title = false;
            float[] values = new float[ss.Length];
            for (int j = 0; j < ss.Length; j++)
            { 
                if( !float.TryParse(ss[j], out values[j]) )
                {
                    title = true;
                    break;
                }

                if( C3DData.IsBlankValue(values[j]) )
                {
                    values = null;
                    return false;
                }

                if( roundUpCheckBox.Checked && floatNum >=0 && floatNum <10)
                {
                    values[j] = (float)Math.Round(values[j],floatNum);
                }
            }

            if (title && !pDataList.IsHaveTitle)
            {
                values = null;
                pDataList.AddHeader(ss);
            }
            else pDataList.Add(values);

            return true;
        }

        public bool SaveProgress()
        {
            if (ipmethod.progressStep < 1) return false;
            string filename = ipmethod.progressFile;
            if( !ipmethod.SaveProgress(filename) )
            {
                errMessage = "save progress failed.\n" + ipmethod.errMsg;
                return false;
            }
            errMessage = "progress data saved.";
            return true;
        }
        public bool SaveToFile(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);

                double x, y, z, v;
                string ss = pDataList.FormatedHeaderLine;                  
                wr.WriteLine(ss);
                for (int i = 0; i < ipmethod.pointCount; i++)
                {
                    x = ipmethod.points[i].x;
                    y = ipmethod.points[i].y;
                    z = ipmethod.points[i].z;
                    v = ipmethod.points[i].v;
                    x = Math.Round(x, 6);
                    y = Math.Round(y, 6);
                    z = Math.Round(z, 6);
                    v = Math.Round(v, 6);
                    ss = x.ToString(); ss += ",";
                    ss += y.ToString(); ss += ",";
                    ss += z.ToString(); ss += ",";
                    ss += v.ToString(); 
                    wr.WriteLine(ss);
                }
                wr.Close();
                fs.Close();

                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        protected bool IsRemarkedLine(string line)
        {
            if (line.Length < 1) return true;
            char c = line[0];
            for (int i = 0; i < remarkChars.Length; i++)
            {
                if (remarkChars[i] == c) return true;
            }
            return false;
        }
        public bool ReadFromFile(string filename,bool clear = true)
        {
            if (clear) 
            { 
                pDataList.Clear(); 
            }

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, encoding );            
            
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim(trimChars);
                if (line.Length < 3) continue;//空行
                if (IsRemarkedLine(line)) continue; //注释行
                string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                if ( ss.Length < 3 )
                {
                    MessageBox.Show("No enough Collums, required atleast 3 columns for (X,Y,Z).", "Reading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //pDataList.Clear();
                    break;
                }
                ProcessLine(ss);
                ss = null;
            }

            pDataList.UpdateRanges();
           
            sr.Close();
            fs.Close();

            return true;
        }
        private void UpdateGeometry()
        {
            lock (UpdateLock)
            {
                Initializing = true;

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

                ResampleNumX.Text = textXNum.Text;
                ResampleNumY.Text = textYNum.Text;
                ResampleNumZ.Text = textZNum.Text;

                Initializing = false;
            }
        }
        private void InitSelect()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox4.Items.Clear();
            if (pDataList.Row < 1) return;
            
            string head = "";
            for(int i=0;i<pDataList.Col;i++)
            {                
                head = pDataList.GetHeader(i);                
                comboBox1.Items.Add(head);
                comboBox2.Items.Add(head);
                comboBox3.Items.Add(head);
                comboBox4.Items.Add(head);
                if( comboBox1.SelectedIndex < 0 &&
                    CDataModel.ChooseXCoordinate(head) ) 
                {
                    comboBox1.SelectedIndex = i;
                }
                if (comboBox2.SelectedIndex < 0 &&
                    CDataModel.ChooseYCoordinate(head))
                {
                    comboBox2.SelectedIndex = i;
                }
                if (comboBox3.SelectedIndex < 0 &&
                    CDataModel.ChooseZCoordinate(head))
                {
                    comboBox3.SelectedIndex = i;
                }
                if (comboBox4.SelectedIndex < 0 &&
                    CDataModel.ChooseVCoordinate(head))
                {
                    comboBox4.SelectedIndex = i;
                }
            }

            //if( comboBox1.Items.Count > 0 )comboBox1.SelectedIndex = 0;
            //if (comboBox2.Items.Count > 1) comboBox2.SelectedIndex = 1;
            //if (comboBox3.Items.Count > 2) comboBox3.SelectedIndex = 2;
            //if (comboBox4.Items.Count > 3) comboBox4.SelectedIndex = 3;
            //else comboBox4.SelectedIndex = comboBox1.Items.Count - 1;

            UpdateSelect();
        }
        void UpdateMemoryStatusInfo()
        {
            if ( ipmethod == null ) return;
            ipmethod.xGrid = nx;
            ipmethod.yGrid = ny;
            ipmethod.zGrid = nz;
            ulong msize = ipmethod.GetRequiredMemorySizeOnCPU();
            memrequired = msize / 1024 / 1024;
            memavailable = PhysicalMemory.GetAvailableMemoryMB();
            
            if (memrequired > memavailable)
            {
                labelMemoryInfo.Text = "No enough memory! ";
                labelMemoryInfo.Text += "Required:" + Math.Round(memrequired, 2) + " MB";
                labelMemoryInfo.Text += "| Available:" + Math.Round(memavailable, 2) + " MB";
                labelMemoryInfo.Text += " | " + MMPhysicalMemory.GetDeveiceNum();
                labelMemoryInfo.Text += " CPUs";
                labelMemoryInfo.ForeColor = Color.Red;
            }
            else 
            {
                labelMemoryInfo.Text = "Memory required " + Math.Round(memrequired, 2) + " MB";
                labelMemoryInfo.Text += "| Memory available  " + Math.Round(memavailable, 2) + " MB";
                labelMemoryInfo.Text += " | " + MMPhysicalMemory.GetDeveiceNum();
                labelMemoryInfo.Text += " CPUs";
                labelMemoryInfo.ForeColor = Color.Green; 
            }
        }
        void UpdateDataInfo()
        {
            if (textBoxGridInfo.InvokeRequired)
            {
                OnUpdateUI outdelegate = new OnUpdateUI(UpdateDataInfo);
                this.BeginInvoke(outdelegate);
                return;
            }

            double x1 = Math.Round(minx, 6);
            double y1 = Math.Round(miny, 6);
            double z1 = Math.Round(minz, 6);
            double v1 = Math.Round(minv, 6);
            double x2 = Math.Round(maxx, 6);
            double y2 = Math.Round(maxy, 6);
            double z2 = Math.Round(maxz, 6);
            double v2 = Math.Round(maxv, 6);

            textBoxGridInfo.Text = "valid row:" + pDataList.Row + "\r\n";
            textBoxGridInfo.Text += "valid points number:" + ipmethod.pointCount + "\r\n";
            textBoxGridInfo.Text += "x range: " + x1.ToString() + " to " + x2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "y range: " + y1.ToString() + " to " + y2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "z range: " + z1.ToString() + " to " + z2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "v range: " + v1.ToString() + " to " + v2.ToString();

            UpdateMemoryStatusInfo();
        }

        //col = -1 ,update all column
        private void UpdateSelect(int col = -1)
        {
            select1 = comboBox1.SelectedIndex;
            select2 = comboBox2.SelectedIndex;
            select3 = comboBox3.SelectedIndex;
            select4 = comboBox4.SelectedIndex;

            //update x selection
            if (select1 >= 0 )
            {
                if (col == 0 || col < 0)
                {
                    minx = pDataList.GetMiniumValue(select1);
                    maxx = pDataList.GetMaxiumValue(select1);
                    if (nx < 1) nx = XNUM;
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
                    miny = pDataList.GetMiniumValue(select2);
                    maxy = pDataList.GetMaxiumValue(select2);
                    if (ny < 1) ny = YNUM;
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
                    minz = pDataList.GetMiniumValue(select3);
                    maxz = pDataList.GetMaxiumValue(select3);

                    if (nz < 1) nz = ZNUM;
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
                    minv = pDataList.GetMiniumValue(select4);
                    maxv = pDataList.GetMaxiumValue(select4);
                }
            }
            else
            {
                minv = maxv = 0;                
            }

            pointsTransformed = false;

            UpdateDataInfo();
        }        
        private bool LoadSourceData(string filename,bool clear = true)
        {
            this.Cursor = Cursors.WaitCursor;
            bool ret = false;
            string ext = Path.GetExtension(filename).ToLower();
            if(ext == ".3dgrid") 
            {
                if (loaded3DGrid != null) loaded3DGrid.Clear();
                loaded3DGrid = new C3DGridData();
                loaded3DGrid.LoadFrom(filename);
                TransformPointsFrom3DGrid(loaded3DGrid);
                textInputFile.Text = filename;
                textOutputFile.Text = filename.PadLeft(filename.Length - 4) + ".3DGrid";                
                UpdateGeometry();
                UpdateDataInfo();
                ret = true;
            }
            else if (ReadFromFile(filename, clear))
            {
                if (loaded3DGrid != null) loaded3DGrid.Clear();
                loaded3DGrid = null;

                textInputFile.Text = filename;
                textOutputFile.Text = filename.PadLeft(filename.Length - 4) + ".3DGrid";
                nx = XNUM;
                ny = YNUM;
                nz = ZNUM;
                InitSelect();
                UpdateGeometry();
                TransformPoints();
                UpdateDataInfo();
                ret = true;
            }
            this.Cursor = DefaultCursor;
            return ret;
        }
        
        //open an text file including 3D scattered points
        private void inputButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (roundUpCheckBox.Checked)
                {   
                    if( !int.TryParse(decimalsNumTextBox.Text,out floatNum) )
                    {
                        MessageBox.Show("Decimals number not correct!");
                        return;
                    }
                }
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "ASCII data(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt";
                    dlg.Filter += "|Gridded data(*.3DGrid)|*.3DGrid";
                    dlg.Filter += "| all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if( !LoadSourceData(dlg.FileName, true) )
                        {
                            MessageBox.Show("load data error.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //open an text file including 3D scattered points and 
        //add to current points collection
        private void AddToButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "ASCII Data(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        
                        bool error = false;

                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            if ( !ReadFromFile( dlg.FileNames[i],false ) )
                            {
                                error = true;
                                MessageBox.Show("Load data error.\n" + errMessage);
                                break;
                            }
                        }
                        
                        if ( !error && dlg.FileNames.Length > 0 )
                        {
                            textInputFile.Text = dlg.FileNames[0];
                            textOutputFile.Text = dlg.FileNames[0].PadLeft(dlg.FileNames[0].Length - 4) + ".3DGrid";
                            nx = XNUM;
                            ny = YNUM;
                            nz = ZNUM;
                            InitSelect();
                            UpdateGeometry();
                            TransformPoints();
                            UpdateDataInfo();
                        }

                        this.Cursor = Cursors.Default;
                    }//if (dlg.ShowDialog() == DialogResult.OK)

                }// using (var dlg = new OpenFileDialog())
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        ////////////////////////////////////
    }//end class
}//end namespace
