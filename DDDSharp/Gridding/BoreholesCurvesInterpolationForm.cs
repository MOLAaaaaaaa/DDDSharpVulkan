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
using DDDSharp.Dialogs;

namespace DDDSharp.Gridding
{
    public partial class BoreholesCurvesInterpolationForm : Form
    {
        List<string> BoreholesColumns = new List<string>();
        List<string> SelectedBoreholesProperties = new List<string>();
        List<Vector64> PropertiesRanges = new List<Vector64>();

        CBoreholes Boreholes = new CBoreholes();
        
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

        BoreholesPropertiesInterpolation ipmethod = new BoreholesPropertiesInterpolation();

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

        public BoreholesCurvesInterpolationForm()
        {
            InitializeComponent();
        }
        public BoreholesCurvesInterpolationForm(CBoreholes boreholes)
        {
            InitializeComponent();
            Boreholes = boreholes;
            BoreholesColumns = Boreholes.GetLasDataColumns();
        }
        void UpdateBoreholesList()
        {
            listBox1.Items.Clear();
            for(int i = 0; i < Boreholes.Count; i++ )
            {
                CBorehole bh = Boreholes[i];
                listBox1.Items.Add(bh.Name);
            }
            listBox1.SelectionMode = SelectionMode.One;
            listBox1.SelectedIndex = -1;
        }
        void UpdateBoreholesList(int index, bool valid = true)
        {
            string text = Boreholes[index].Name;
            if( index >=0 && index < listBox1.Items.Count)
            {
                if(valid)listBox1.Items[index] = text;
                else     listBox1.Items[index] = text + " invalid";
            }
        }
        void UpdateColumnsCheckedList()
        {
            checkedListBox2.Items.Clear();
            if (BoreholesColumns.Count < 1) BoreholesColumns = Boreholes.GetLasDataColumns();
            for (int i = 0; i < BoreholesColumns.Count; i++)
            {
                checkedListBox2.Items.Add(BoreholesColumns[i]);
            }
            checkedListBox2.SelectedIndex = -1;
        }
        void UpdateRange()
        {
            Boreholes.UpdateRange();
            minx = Boreholes.minx;
            miny = Boreholes.miny;
            minz = Boreholes.minz;
            minv = Boreholes.minv;
            maxx = Boreholes.maxx;
            maxy = Boreholes.maxy;
            maxz = Boreholes.maxz;
            maxv = Boreholes.maxv;

            if (nx > 1) xstep = (maxx - minx) / (nx - 1);
            if (ny > 1) ystep = (maxy - miny) / (ny - 1);
            if (nz > 1) zstep = (maxz - minz) / (nz - 1);

            UpdatePropertiesValueRange();
            UpdateGeometry();
            UpdateDataInfo();
        }
        private void BoreholesCurvesInterpolationForm_Load(object sender, EventArgs e)
        {
            UpdateBoreholesList();
            UpdateColumnsCheckedList();

            nx = XNUM;
            ny = YNUM;
            nz = ZNUM;
            
            UpdateRange();            
        }

       
        void UpdateList2()
        {
            listBox2.Items.Clear();
            if (Meshes.Count < 1) return;

            CMesh sp;
            string ss;
            for (int i = 0; i < Meshes.Count; i++)
            {
                sp = Meshes[i];
                ss = (i + 1) + "=>";
                ss += sp.errMessage;
                ss += ", ";
                ss += "minz = " + sp.minv;
                ss += " to ";
                ss += sp.maxv;
                listBox2.Items.Add(ss);
            }
            listBox2.SelectedIndex = Meshes.Count - 1;
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
            List<float[]> grids;

            lock (InterpolateLock)
            {
                workDisposed = false;
                checkProgress = true;

                if (selectedDevices.Count > 0) //GPU 并行计算
                    grids = ipmethod.GetInterpolatedValues(nx, ny, nz, selectedDevices.ToArray());//GPU version
                else grids = ipmethod.GetInterpolatedValues(nx, ny, nz, null); //CPU version

                percentage = 0;
                progressState = 0;
                checkProgress = false;
                workDisposed = true;
                UpdateProgress();
                Thread.Sleep(2000);
            }//lock (InterpolateLock)

            if ( grids == null )
            {
                string info = "gridding failed!\n\n" + ipmethod.errMsg + "\n\n";
                info += "Time elapsed -- " + ipmethod.formatTime(ipmethod.timeSlip);
                MessageBox.Show(info);
                data = null;
                return;
            }

            int succ = 0;
            for( int i = 0; i < grids.Count; i++ )
            {
                string file = textOutputFile.Lines[i];
                data.pGridData = grids[i];
                data.UpdateRange();
                if ( data.SaveAs(file) )
                {
                    succ++;
                }
            }
            if (succ == SelectedBoreholesProperties.Count)
            {
                string info = "Gridding successfully!\n\n";
                info += "Time elapsed -- " + ipmethod.formatTime(ipmethod.timeSlip);
                MessageBox.Show(info);
            }
            else
            {
                string info = "Gridding Completed," + succ + " Successed.";
                info += "Time elapsed -- " + ipmethod.formatTime(ipmethod.timeSlip);
                MessageBox.Show(info);
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

            if (MessageBox.Show("Are you sure to abort current progress?\n Works have done will not be saved.", "Abort the progress?",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            if (ipmethod.progressStep > 0)
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
            if (sel >= 1 && sel < Meshes.Count)
            {
                CMesh cs1 = Meshes[sel - 1];
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
            if (sel >= 0 && sel < Meshes.Count - 1)
            {
                CMesh cs1 = Meshes[sel + 1];
                CMesh cs2 = Meshes[sel];
                Meshes[sel + 1] = cs2;
                Meshes[sel] = cs1;
                UpdateList2();
                listBox2.SelectedIndex = sel + 1;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel >= 0 && sel < Boreholes.Count)
            {
                CBorehole bh = Boreholes[sel];
                propertyGrid1.SelectedObject = bh.Curves.lasData;
            }
            else propertyGrid1.SelectedObject = null;
        }

        void UpdatePropertiesValueRange()
        {
            PropertiesRanges.Clear();
            Vector64 range = new Vector64();
            double v1 = 0, v2 = 0;

            foreach(string name in SelectedBoreholesProperties)            
            {
                int i = 0;
                foreach( int k in GetValidBoreholesByProperty(name) )
                {
                    LasFileData las = Boreholes[k].Curves.lasData;
                    if ( las == null ) continue;
                    int id = las.GetColumnIndex(name);
                    if (id < 0) continue;

                    range = las.GetDataRange(id);
                    if (i == 0)
                    {
                        v1 = range.X;
                        v2 = range.Y;
                    }
                    else
                    {
                        if (range.X < v1) v1 = range.X;
                        if (range.Y > v2) v2 = range.Y;
                    }
                    i++;
                }//foreach( int k in GetValidBoreholesByProperty(name) )
                PropertiesRanges.Add(new Vector64(v1, v2, 0));
            }
        }
        /// <summary>
        /// 筛选出有效的钻孔数据
        /// </summary>
        /// <param name="property">列名称</param>
        /// <returns>钻孔序号数组</returns>
        List<int> GetValidBoreholesByProperty(string property)
        {
            List<int> indices = new List<int>();
            for (int j = 0; j < Boreholes.Count; j++)
            {
                LasFileData las = Boreholes[j].Curves.lasData;
                if (las != null && las.IsColuwnExist(property))
                    indices.Add(j);
            }
            return indices;
        }     
        
        void CheckPropertyIsExistInBoreholes(string name)
        {
            //检查钻孔中是否存在          
            List<string> errs = new List<string>();            
            for (int k = 0; k < Boreholes.Count; k++)
            {                
                LasFileData las = Boreholes[k].Curves.lasData;
                if (las == null)
                {
                    errs.Add(Boreholes[k].Name + " missed property" + name);
                    UpdateBoreholesList(k, false);
                }
                else if ( !las.IsColuwnExist(name) )
                {
                    errs.Add(Boreholes[k].Name + " missed property" + name);
                    UpdateBoreholesList(k, false);
                }
            }// for (int k = 0; k < Boreholes.Count; k++)
            if( errs.Count > 0 )
            {
                MessageBoxOnListForm msg = new MessageBoxOnListForm();
                msg.TitleText = "Property missed on Bohoreles:";
                msg.CaptionText = "Waining!!!";
                msg.Messages = errs.ToArray();
                msg.ShowDialog();
            }
        }
        private void checkedListBox2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int index = e.Index;
            string name = checkedListBox2.Items[index].ToString();

            if ( e.NewValue == CheckState.Unchecked ) //取消选择
            {
                if ( SelectedBoreholesProperties.Count > 0)
                {                    
                    SelectedBoreholesProperties.Remove(name);
                    UpdateDataInfo();
                }
                return;
            }

            //勾选  
            SelectedBoreholesProperties.Add(name);
            CheckPropertyIsExistInBoreholes(name);            
            UpdatePropertiesValueRange();
            UpdateOutputFileNames();
            UpdateDataInfo();
        }

        void UpdateOutputFileNames(string curpath = "")
        {
            string s0 = Boreholes.Name + "_Interpolated_";

            string path = System.IO.Directory.GetCurrentDirectory();//.CurrentDirectory;            
            if (curpath.Length > 1) path = Path.GetDirectoryName(curpath);
            
            path += "\\";
            List<string> files = new List<string>();
            foreach ( string name in SelectedBoreholesProperties )
            {
                files.Add(path + s0 + name + ".3DGrid");
            }
            textOutputFile.Lines = files.ToArray();
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            LasFileData data = propertyGrid1.SelectedObject as LasFileData;
            if (data.Parent != null)
            { 
                CBorehole bh = data.Parent as CBorehole;
                bh.UpdateRange();
            }
            UpdateRange();
        }

        private void StartButton_Click_1(object sender, EventArgs e)
        {
            if ( Boreholes.Count < 2 || Meshes.Count < 2)
            {
                MessageBox.Show("no enough boreholes or meshes loaded.");
                return;
            }
            
            //minx-maxx,miny-maxy,minz-maxz
            GetRangeFromGeometry();

            ipmethod.Clear();
            ipmethod.Layers.AddRange(Meshes);
            ipmethod.Boreholes = Boreholes;
            ipmethod.Properties.AddRange(SelectedBoreholesProperties);

            ipmethod.UpdatePointsRange();
            ipmethod.minx = minx;
            ipmethod.miny = miny;
            ipmethod.minz = minz;
            ipmethod.maxx = maxx;
            ipmethod.maxy = maxy;
            ipmethod.maxz = maxz;        
            
            try
            {
                data = new C3DGridData(minx, maxx, miny, maxy, minz, maxz, minv, maxv);
                data.xNum = nx;
                data.yNum = ny;
                data.zNum = nz;                
            }
            catch (Exception ex)
            {
                MessageBox.Show("no enough memory,try to decrease grid numbers.\n" + ex.Message);
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

        private void AddButton1_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.FormatLASFileFilter + "|all files(*.*)|*.*";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    
                    foreach (string file in dlg.FileNames)
                    {
                        CLasFile las = new CLasFile();
                        LasFileData data = las.Read(file);
                        if ( data == null )
                        {
                            Cursor = Cursors.Default;
                            MessageBox.Show("Load data failed.\n" + file);
                            return;
                        }
                        
                        data.Name = Path.GetFileNameWithoutExtension(file);
                                                
                        CBorehole bh = new CBorehole(data);
                        Boreholes.AddBorehole( bh );

                        textOutputFile.Text = file;

                    }//foreach (string file in dlg.FileNames)                    
                    
                    UpdateRange();

                    BoreholesColumns = Boreholes.GetLasDataColumns();
                    UpdateBoreholesList();
                    UpdateColumnsCheckedList();

                    textOutputFile.Text = Path.GetDirectoryName(textOutputFile.Text) + "\\grid3d.3DGrid";

                    Cursor = Cursors.Default;
                }
            }
        }

        private void RemoveButton1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndices.Count < 1)
            {
                MessageBox.Show("no selections.");
                return;
            }

            List<int> indices = new List<int>();
            foreach (int sel in listBox1.SelectedIndices)
            {
                indices.Add(sel);
            }

            indices.Sort((a, b) => { return b.CompareTo(a); });

            for (int i = 0; i < indices.Count; i++)
            {
                Boreholes.pData.RemoveAt(indices[i]);
                listBox1.Items.RemoveAt(indices[i]);
            }
           
            UpdateRange();
            BoreholesColumns = Boreholes.GetLasDataColumns();            
            UpdateColumnsCheckedList();
        }

        private void outputButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.Grid3DFileFormatFilter;
                dlg.Filter += "| All Files(*.*)|*.*";
                dlg.FileName = Boreholes.Name + "_Interpolated";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    UpdateOutputFileNames(dlg.FileName);                    
                }
            }
        }

        private void AddButton2_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Formatted mesh files(*.mesh)|*.mesh | Surfer grid(*.grd) | *.grd|all files(*.*)|*.*";
                dlg.Multiselect = true;
                string ext;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in dlg.FileNames)
                    {
                        ext = Path.GetExtension(file).ToLower();
                        if (ext == ".grd")
                        {
                            CSurferGrid cs = new CSurferGrid();
                            if (!cs.Read(file))
                            {
                                MessageBox.Show("Load gridding data faild.\n" + cs.errMessage);
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
                                MessageBox.Show("Load gridding data faild.\n" + mesh.errMessage);
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
                MessageBox.Show("no selections.");
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
       
        private bool GetRangeFromGeometry()
        {
            if (!double.TryParse(textX1.Text, out minx) ||
                 !double.TryParse(textX2.Text, out maxx) ||
                 !double.TryParse(textY1.Text, out miny) ||
                 !double.TryParse(textY2.Text, out maxy) ||
                 !double.TryParse(textZ1.Text, out minz) ||
                 !double.TryParse(textZ2.Text, out maxz)) return false;
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
            for (int i = 0; i < SelectedBoreholesProperties.Count; i++)
            {
                string name = SelectedBoreholesProperties[i];
                v1 = Math.Round(PropertiesRanges[i].X, 6);
                v2 = Math.Round(PropertiesRanges[i].Y, 6);
                textBoxGridInfo.Text += name + " range: " + v1.ToString() + " to " + v2.ToString();
                textBoxGridInfo.Text += "\r\n";
            }
            
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
                        if (int.TryParse(textXNum.Text, out nx))
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
