using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCLNet;
using CLInterpolation;
using DataCollection;

namespace DDDSharp
{
    public partial class ProfilesGriddingForm : Form
    {
        List<GeoProfile> Profiles = new List<GeoProfile>();
        //List<CSurferGrid> Layers = new List<CSurferGrid>();
        List<CMesh> Meshes = new List<CMesh>();

        static int nx = 101, ny = 101, nz = 101;
        static double xstep, ystep, zstep;
        static double minx, maxx, miny, maxy, minz, maxz, minv, maxv;
        double nullValue = CSurferGrid.blankValue;

        private string errMessage = "";

        static private bool workDisposed = false;
        private System.Object UpdateLock = new System.Object();
        static private System.Object InterpolateLock = new System.Object();

        private bool Initializing = false;

        int progressState = 0;// 0没开始，1已开始，2已暂停        

        Thread globalThread = null;
        Thread localThread = null;
        Thread checkThread = null;

        //GPU Interpolation 
        List<Device> devices = new List<Device>();
        List<Device> selectedDevices = new List<Device>();
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
        public delegate void OnUpdateUI();

        private void textX1_TextChanged(object sender, EventArgs e)
        {
            DoRangeChange(0);
            UpdateGeometry();
        }

        private void textX2_TextChanged(object sender, EventArgs e)
        {
            DoRangeChange(0);
            UpdateGeometry();
        }

        private void textStepX_TextChanged(object sender, EventArgs e)
        {
            DoSpaceChange(0);
            UpdateGeometry();
        }

        private void textXNum_TextChanged(object sender, EventArgs e)
        {
            DoNumChange(0);
            UpdateGeometry();

        }

        private void textY1_TextChanged(object sender, EventArgs e)
        {
            DoRangeChange(1);
            UpdateGeometry();
        }

        private void textY2_TextChanged(object sender, EventArgs e)
        {
            DoRangeChange(1);
            UpdateGeometry();
        }

        private void textStepY_TextChanged(object sender, EventArgs e)
        {
            DoSpaceChange(1);
            UpdateGeometry();
        }

        private void textYNum_TextChanged(object sender, EventArgs e)
        {
            DoNumChange(1);
            UpdateGeometry();
        }

        private void textZ1_TextChanged(object sender, EventArgs e)
        {
            DoRangeChange(2);
            UpdateGeometry();
        }

        private void textZ2_TextChanged(object sender, EventArgs e)
        {
            DoRangeChange(2);
            UpdateGeometry();
        }

        private void textStepZ_TextChanged(object sender, EventArgs e)
        {
            DoSpaceChange(2);
            UpdateGeometry();
        }

        private void textZNum_TextChanged(object sender, EventArgs e)
        {
            DoNumChange(2);
            UpdateGeometry();
        }

        public ProfilesGriddingForm()
        {
            InitializeComponent();
        }

        private void ProfilesGriddingForm_Load(object sender, EventArgs e)
        {

        }
        
        void UpdateList1(GeoProfile sp)
        {            
            string ss = sp.errMessage;
            ss += ", ";
            ss += sp.Name + ", "; ;
            ss += "Row = " + sp.nRow;
            ss += ", ";
            ss += "Col = " + sp.nCol;
            ss += ", ";
            ss += "x = " + sp.minx;
            ss += " to " + sp.maxx;
            ss += ", ";
            ss += "y = " + sp.miny;
            ss += " to " + sp.maxy;
            ss += ", ";
            ss += "z = " + sp.minz;
            ss += " to " + sp.maxz;
            ss += ", V = " + sp.minv;
            ss += " to " + sp.maxv;
            listBox1.Items.Add(ss);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }
        void UpdateList2()
        {
            listBox2.Items.Clear();
            if ( Meshes.Count < 1 ) return;

            CMesh sp;
            string ss;
            for (int i=0;i< Meshes.Count;i++)
            {
                sp = Meshes[i];
                ss = (i + 1 ) + "=>";
                ss += sp.errMessage;
                ss += ", ";
                ss += "minz = " + sp.minv;
                ss += " to ";
                ss += sp.maxv;
                listBox2.Items.Add(ss);
            }
            listBox2.SelectedIndex = Meshes.Count - 1;
        }
        private void AddButton1_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Geological profiles(*.csv,*.dat,*.txt)|*.csv;*.dat;*.txt|all files(*.*)|*.*";
                dlg.Multiselect = true;
                if ( dlg.ShowDialog() == DialogResult.OK )
                {
                    Cursor = Cursors.WaitCursor;

                    foreach(string file in dlg.FileNames )
                    {
                        GeoProfile profile = new GeoProfile();
                        if( !profile.LoadFrom(file) )
                        {
                            MessageBox.Show(AppLocalization.IsChinese ? "加载数据失败。\n" + file + "\n" + profile.errMessage : "Load data failed.\n" + file + "\n" + profile.errMessage);
                            return;
                        }
                        
                        textOutputFile.Text = file;

                        Profiles.Add(profile);
                        GetRangeFromProfiles();

                        profile.errMessage = Path.GetFileName(file);                        
                        UpdateList1(profile);
                        
                        if (nx > 1) xstep = (maxx - minx) / (nx - 1);
                        if (ny > 1) ystep = (maxy - miny) / (ny - 1);
                        if (nz > 1) zstep = (maxz - minz) / (nz - 1);

                        UpdateGeometry();
                    }

                    textOutputFile.Text = Path.GetDirectoryName(textOutputFile.Text) + "\\grid3d.3DGrid";

                    Cursor = Cursors.Default;
                }
            }
        }

        private void RemoveButton1_Click(object sender, EventArgs e)
        {
            if( listBox1.SelectedIndices.Count < 1 )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择对象。" : "No selections.");
                return;
            }

            List<int> indices = new List<int>();
            foreach(int sel in listBox1.SelectedIndices)
            {
                indices.Add(sel);
            }            
            
            indices.Sort((a, b) => { return b.CompareTo(a); });

            for(int i=0;i<indices.Count;i++)
            {
                Profiles.RemoveAt( indices[i] );
                listBox1.Items.RemoveAt(indices[i]);
            }            
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if( Profiles.Count < 1 )
            {
                MessageBox.Show("no enough profiles loaded.");
                return;
            }            
            //minx-maxx,miny-maxy,minz-maxz
            GetRangeFromGeometry();
            
            MeshesRestrictedProfilesInterpolation ip = new MeshesRestrictedProfilesInterpolation();
            ip.Layers.AddRange(Meshes);           
            ip.Profiles = Profiles;
            
            ip.UpdatePointsRange();
            ip.minx = minx;
            ip.miny = miny;
            ip.minz = minz;
            ip.maxx = maxx;
            ip.maxy = maxy;
            ip.maxz = maxz;

            ipmethod = ip;
            try
            {
                data = new C3DGridData(minx, maxx, miny, maxy, minz, maxz, minv, maxv);
                data.xNum = nx;
                data.yNum = ny;
                data.zNum = nz;
                data.pGridData = new float[nx * ny * nz];
            }
            catch (Exception ex)
            {
                MessageBox.Show(AppLocalization.IsChinese ? "内存不足，请尝试减少网格数量。\n" + ex.Message : "Not enough memory, try to decrease grid numbers.\n" + ex.Message);
                ipmethod.Clear();
                return;
            }

            ipmethod.SetGeometry(minx, maxx, miny, maxy, minz, maxz, xstep, ystep, zstep, nx, ny, nz);
            
            #region 获取选定GPU设备 
            selectedDevices.Clear();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    selectedDevices.Add(devices[i]);
                }
            }
            #endregion 获取选定GPU设备

            ipmethod.threadStoped = false;

            //启动插值线程
            InitProgressBar(0, 100);
            
            if (globalThread == null || !globalThread.IsAlive)
            {
                ipmethod.threadStoped = false;
                globalThread = new Thread(InterpolationThread);
                globalThread.Start();
            }

            //启动进度线程
            if (checkThread == null || !checkThread.IsAlive)
            {
                checkProgress = true;
                checkThread = new Thread(ProgressStatusCheckThread);
                checkThread.Start();
            }

            progressState = 1;// 0没开始，1已开始，2已暂停

        }
        private void InitProgressBar(int min = 0, int max = 100)
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
            labelTimeLeft.Text = timeLeftTitle;
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
                if (progressBar1.Visible == false)
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

                if (ipmethod.method == InterpolationMethod.GriddedInterpolation)
                {
                    data.pGridData = ipmethod.DirectGridding(nx, ny, nz);
                }
                else
                {
                    //  ipmethod.Normalize();
                    //重点检查
                    if (ipmethod.progressStep < 1)
                    {
                        if (ipmethod.method == InterpolationMethod.RadicalBasisFunction)
                        {
                            //int np = ipmethod.RemoveDuplicated();
                            //if (np > 0)
                            //{
                            //    UpdateDataInfo();
                            //}
                        }
                    }

                    if (selectedDevices.Count > 0) //GPU 并行计算
                        data.pGridData = ipmethod.GetInterpolatedValue(nx, ny, nz, selectedDevices.ToArray());//GPU version
                    else data.pGridData = ipmethod.GetInterpolatedValue(nx, ny, nz, null); //CPU version

                    data.UpdateRange();
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

            if (data.SaveAs(textOutputFile.Text))
            {
                string info = "Gridding successfully!\n\n";
                info += "Time elapsed -- " + ipmethod.formatTime(ipmethod.timeSlip);
                MessageBox.Show(info);
            }
            else
            {
                // MessageBox.Show("save gridding data failed!");
            }
        }

        private void UpdateButtonState()
        {
            if (progressState == 0)
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
                if (localThread != null && localThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    localThread.Suspend();
#pragma warning restore CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                progressState = 2;
            }
            else if (progressState == 2)
            {
                if (globalThread != null && globalThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    globalThread.Resume();
#pragma warning restore CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                if (localThread != null && localThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    localThread.Resume();
#pragma warning restore CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                progressState = 1;
            }

            UpdateButtonState();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (progressState < 1) return;

            if (globalThread != null && globalThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                globalThread.Suspend();
#pragma warning restore CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
            if (localThread != null && localThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                localThread.Suspend();
#pragma warning restore CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”

            if (MessageBox.Show(AppLocalization.IsChinese ? "确定要中止当前操作吗？\n已完成的工作不会保存。" : "Are you sure to abort current progress?\nWorks done will not be saved.", AppLocalization.IsChinese ? "中止操作？" : "Abort the progress?",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            if (ipmethod.progressStep > 0)
            {
                if (MessageBox.Show(AppLocalization.IsChinese ? "工作尚未完成，是否保存任务？" : "Work not completed, save the task?", AppLocalization.IsChinese ? "保存未完成任务？" : "Save uncompleted task?",
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Cursor = Cursors.WaitCursor;
                    ipmethod.SaveProgress(ipmethod.progressFile);
                    Cursor = Cursors.Default;
                }
            }

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
            if (progressState == 2 && ipmethod.progressStep > 0)//暂停状态
            {                
                MessageBox.Show(errMessage);
            }
            UpdateButtonState();
        }


        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if( sel >=1 && sel < Meshes.Count )
            {
                CMesh cs1 = Meshes[sel-1];
                CMesh cs2 = Meshes[sel];
                Meshes[sel - 1] = cs2;
                Meshes[sel] = cs1;
                UpdateList2();
                listBox2.SelectedIndex = sel - 1;
            }
        }

        private void MoveDownBotton_Click(object sender, EventArgs e)
        {
            int sel = listBox2.SelectedIndex;
            if (sel >= 0 && sel < Meshes.Count -1 )
            {
                CMesh cs1 = Meshes[sel + 1];
                CMesh cs2 = Meshes[sel];
                Meshes[sel + 1] = cs2;
                Meshes[sel] = cs1;
                UpdateList2();
                listBox2.SelectedIndex = sel + 1;
            }
        }

        private void AddButton2_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Formatted mesh files(*.mesh)|*.mesh | Surfer grid(*.grd) | *.grd|all files(*.*)|*.*";
                dlg.Multiselect = true;
                string ext;
                if ( dlg.ShowDialog() == DialogResult.OK )
                {
                    foreach (string file in dlg.FileNames)
                    {
                        ext = Path.GetExtension(file).ToLower();
                        if (ext == ".grd")
                        {
                            CSurferGrid cs = new CSurferGrid();
                            if (!cs.Read(file))
                            {
                                MessageBox.Show(AppLocalization.IsChinese ? "加载网格数据失败。\n" + cs.errMessage : "Load gridding data failed.\n" + cs.errMessage);
                                break;
                            }                            
                            CMesh mesh = new CMesh();
                            mesh.fromGrid2D(cs);
                            mesh.errMessage = Path.GetFileName(file);
                            Meshes.Add(mesh);
                        }
                        else //if (ext == ".mesh")
                        {
                            CMesh mesh = new CMesh();
                            if (!mesh.LoadFrom(file))
                            {
                                MessageBox.Show(AppLocalization.IsChinese ? "加载网格数据失败。\n" + mesh.errMessage : "Load gridding data failed.\n" + mesh.errMessage);
                                break;
                            }
                            else
                            {
                                mesh.errMessage = Path.GetFileName(file);
                                Meshes.Add(mesh);
                            }
                        }
                    }
                    //按Z值排序
                    Meshes.Sort((a, b) => { return ((a.minv + a.maxv)).CompareTo((b.minv + b.maxv)); });
                    UpdateList2();
                }
            }
        }

        private void RemoveButton2_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndices.Count < 1)
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择对象。" : "No selections.");
                return;
            }

            List<int> indices = new List<int>();
            foreach (int sel in listBox2.SelectedIndices)
            {
                indices.Add(sel);
            }

            indices.Sort((a, b) => { return b.CompareTo(a); });

            for (int i = 0; i < indices.Count; i++)
            {
                Meshes.RemoveAt(indices[i]);
                listBox2.Items.RemoveAt(indices[i]);
            }
        }
        void UpdateMemoryStatusInfo()
        {
            if (ipmethod == null) return;

            ulong msize = ipmethod.GetRequiredMemorySizeOnCPU();
            double memrequired = msize / 1024 / 1024;
            double memavailable = MMPhysicalMemory.GetAvailableMemoryMB();

            if (memrequired > memavailable)
            {
                labelMemoryInfo.Text = "No enough memory on CPU ! ";
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

                Initializing = false;
            }
        }
        private void GetRangeFromProfiles()
        {
            int i = 0;
            foreach(GeoProfile sp in Profiles )
            {
                if (i == 0)
                { 
                    minx = sp.Minx;
                    miny = sp.Miny;
                    minz = sp.Minz;
                    minv = sp.Minv;
                    maxx = sp.Maxx;
                    maxy = sp.Maxy;
                    maxz = sp.Maxz;
                    maxv = sp.Maxv;
                }
                else GetRangeFromProfiles(sp);
                i++;
            }
        }
        private void GetRangeFromProfiles(GeoProfile sp)
        {
            if (sp.Minx < minx) minx = sp.Minx;
            if (sp.Miny < miny) miny = sp.Miny;
            if (sp.Minz < minz) minz = sp.Minz;
            if (sp.Minv < minv) minv = sp.Minv;
            if (sp.Maxx > maxx) maxx = sp.Maxx;
            if (sp.Maxy > maxy) maxy = sp.Maxy;
            if (sp.Maxz > maxz) maxz = sp.Maxz;
            if (sp.Maxv > maxv) maxv = sp.Maxv;
        }
        private bool GetRangeFromGeometry()
        {
            if ( !double.TryParse(textX1.Text, out minx) ||
                 !double.TryParse(textX2.Text, out maxx) ||
                 !double.TryParse(textY1.Text, out miny) ||
                 !double.TryParse(textY2.Text, out maxy) ||
                 !double.TryParse(textZ1.Text, out minz) ||
                 !double.TryParse(textZ2.Text, out maxz) ) return false;
            return true;
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

            textBoxGridInfo.Text = "x range: " + x1.ToString() + " to " + x2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "y range: " + y1.ToString() + " to " + y2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "z range: " + z1.ToString() + " to " + z2.ToString();
            textBoxGridInfo.Text += "\r\n";
            textBoxGridInfo.Text += "v range: " + v1.ToString() + " to " + v2.ToString();

            UpdateMemoryStatusInfo();
        }

        private void DoRangeChange(int dir)
        {
            lock (UpdateLock)
            {
                if (!Initializing)
                {

                    if (dir == 0)
                    {
                        if (double.TryParse(textX1.Text, out minx) &&
                             double.TryParse(textX2.Text, out maxx))
                            xstep = (maxx - minx) / (nx - 1);
                    }
                    if (dir == 1)
                    {
                        if (double.TryParse(textY1.Text, out miny) &&
                            double.TryParse(textY2.Text, out maxy))
                            ystep = (maxy - miny) / (ny - 1);
                    }
                    if (dir == 2)
                    {
                        if (double.TryParse(textZ1.Text, out minz) &&
                             double.TryParse(textZ2.Text, out maxz))
                            zstep = (maxz - minz) / (nz - 1);
                    }
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
                        if (double.TryParse(textStepX.Text, out xstep))
                            nx = (int)((maxx - minx) / xstep) + 1;
                    }
                    if (dir == 1)
                    {
                        if (double.TryParse(textStepY.Text, out ystep))
                            ny = (int)((maxy - miny) / ystep) + 1;
                    }
                    if (dir == 2)
                    {
                        if (double.TryParse(textStepZ.Text, out zstep))
                            nz = (int)((maxz - minz) / zstep) + 1;
                    }
                }
            }
        }

        private void DoNumChange(int dir)
        {
            lock (UpdateLock)
            {
                if (!Initializing)
                {

                    if (dir == 0)
                    {
                        if ( int.TryParse(textXNum.Text, out nx) )
                            xstep = (maxx - minx) / (nx - 1);
                    }
                    if (dir == 1)
                    {
                        if (int.TryParse(textYNum.Text, out ny))
                            ystep = (maxy - miny) / (ny - 1);
                    }
                    if (dir == 2)
                    {
                        if (int.TryParse(textZNum.Text, out nz))
                            zstep = (maxz - minz) / (nz - 1);
                    }
                }
            }
        }
       
        //-----------------------------------
    }
}
