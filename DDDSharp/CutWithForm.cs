using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
using CLPolygonTrim;
using OpenCLNet;
namespace DDDSharp
{
    public partial class CutWithForm : Form
    {
        List<int> models = new List<int>();
        List<int> objects = new List<int>();
       
        public int cutIndex = -1;
        public C3DObjectBase selectedModel = null;
        public C3DObjectBase selectedTarget = null;
        int selectedModelIndex = -1;
        int selectedTargetIndex = -1;

        int Method;
        int Para;

        //GPU Interpolation 
        List<Device> devices = new List<Device>();
        List<Device> selectedDevices = new List<Device>();

        PolygonTrim clTrim = new PolygonTrim();
        double Percentage;
        string progressTitle = "";
        string timeLeft = "";
        bool checkProgress = false;

        Thread cutThread = null;        
        Thread checkThread = null;
        int progressState = 0;
        public CutWithForm()
        {
            InitializeComponent();
        }

        public void UpdateList()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            models.Clear();
            objects.Clear();
            
            C3DObjectBase obj;
            foreach (var item in C3DData.objectsDiction)             
            {
                obj = item.Value;
                //models
                if (obj.type == ShapeEnum.Mesh ||
                    obj.type == ShapeEnum.Polygon ||
                    obj.type == ShapeEnum.Triangles ||
                    obj.type == ShapeEnum.Slicer )
                {
                    comboBox1.Items.Add(obj.Name);
                    models.Add(item.Key);
                }

                //target objects
                comboBox2.Items.Add(obj.Name);
                objects.Add(item.Key);
            }

            devices = OpenCLObj.GetDevices();
            string text;
            //double gb = 1024 * 1024*1024;
            for (int i = 0; i < devices.Count; i++)
            {
                text = devices[i].Name;
                text += "|";
                text += Math.Round(OpenCLObj.GetMaxAllocMemoryMB(devices[i]), 0);
                text += "G|Items:";
                text += devices[i].MaxWorkGroupSize;
                checkedListBox1.Items.Add(text);
            }
        }

        private void CutWithForm_Load(object sender, EventArgs e)
        {
            UpdateButtonState();
            UpdateList();
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
            if (Percentage < 0) Percentage = 0;
            if (Percentage > 100) Percentage = 100;
            progressBar1.Value = (int)(Percentage);

            string str = Math.Round(Percentage, 2).ToString() + "%";
            //Font font = new Font("Times New Roman", (float)11, FontStyle.Regular);
            //PointF pt = new PointF(this.progressBar1.Width / 2 - 10, this.progressBar1.Height / 2 - 10);
            //this.progressBar1.CreateGraphics().DrawString(str, font, Brushes.Blue, pt);

            LableProgressTitle.Text = clTrim.progressTitle + str;
            LabelTimeLeft.Text = clTrim.strTimeLeft;

            
            UpdateButtonState();

        }
        private void PolygonTrimThread()
        {
            progressState = 1;// 0没开始，1已开始，2已暂停

            TriangleObj obj = ((TriangleObj)selectedModel).Copy();
            obj.Normalize();
            
            bool keepoutside = true;
            if (Para == 1) keepoutside = false;
            bool ret = false;
            if ( selectedDevices.Count > 0 )
            {
                ret = clTrim.CutWithPolygon((C3DGridData)selectedTarget, new Polygon3D(obj), keepoutside,selectedDevices.ToArray());
            }
            else 
            {
                ret = clTrim.CutWithPolygon((C3DGridData)selectedTarget, new Polygon3D(obj), keepoutside,null);                
            }
            if (! ret )
            {
                MessageBox.Show("failed to trim with polygon object.\n" + clTrim.errMsg);
                cutIndex = -1;
            }
            else
            {
                cutIndex = selectedTargetIndex;
                MessageBox.Show("Successed.");
            }
            
            progressState = 0;

           // checkProgress = false;
        }
        private void SlicerTrimThread()
        {
            progressState = 1;// 0没开始，1已开始，2已暂停

            bool ret = false;
            if (selectedDevices.Count > 0)
            {
                ret = clTrim.TrimWithSlicer((C3DGridData)selectedTarget, (CSlicer)selectedModel,Para);
            }
            else
            {
                ret  = clTrim.TrimWithSlicer((C3DGridData)selectedTarget, (CSlicer)selectedModel, Para);

            }
            if (!ret )
            {
                MessageBox.Show("failed.\n" + clTrim.errMsg);
                cutIndex = -1;
            }
            else
            {                
                cutIndex = selectedTargetIndex;
                MessageBox.Show("Successed.");
            }
            
            progressState = 0;

           // checkProgress = false;
        }
        bool GetSelected()
        {
            if (comboBox1.SelectedIndex < 0) return false;
            if (comboBox2.SelectedIndex < 0) return false;

            selectedModelIndex = models[comboBox1.SelectedIndex];
            selectedTargetIndex = objects[comboBox2.SelectedIndex];

            selectedModel = C3DData.GetObjectByKey(selectedModelIndex);//model
            selectedTarget = C3DData.GetObjectByKey(selectedTargetIndex); //target

            Para = comboBox3.SelectedIndex;
            Method = -1;

            if (selectedModel.type == ShapeEnum.Grid3D ||
                selectedModel.type == ShapeEnum.Triangles ||
                selectedModel.type == ShapeEnum.Shphere ||
                selectedModel.type == ShapeEnum.Polygon ||
                selectedModel.type == ShapeEnum.Box ||
                selectedModel.type == ShapeEnum.Cylinder) Method = 0;                
            else if (selectedModel.type == ShapeEnum.Mesh) Method = 1;                
            else if (selectedModel.type == ShapeEnum.Slicer) Method = 2;
                

            return true;
        }
        private void StartTrimThread()
        {           
            if (selectedTarget.type == ShapeEnum.Grid3D)
            {
                if( Method == 0)//Polygon
                {                   
                    cutThread = new Thread(PolygonTrimThread);
                    cutThread.Start();
                    //bool[] blanked = clTrim.Trim((C3DGridData)pObj, (TriangleObj)obj1, selectedDevices.ToArray());
                    //pObj.CutWith(obj1, method1, method2);
                    //cutIndex = id2;
                }
                else if (Method == 1)//Mesh
                {
                   // pObj = (C3DGridData)selectedTarget;
                   // pObj.CutWith(selectedModel, Method, Para);
                   // cutIndex = selectedTargetIndex;
                }
                else if (Method == 2)//Slicer
                {
                    cutThread = new Thread(SlicerTrimThread);
                    cutThread.Start();
                }
            }
            
            return ;
        }
        void UpdateButtonState()
        {
            if (progressState == 0)//before start
            {
                StartButton.Enabled = true;
                PauseButton.Enabled = false;
                StopButton.Enabled = false;
                OKButton.Enabled = true;                
                progressBar1.Visible = false;
                LableProgressTitle.Visible = false;
                LabelTimeLeft.Visible = false;
            }
            else //in progress
            {
                StartButton.Enabled = false;
                PauseButton.Enabled = true;
                StopButton.Enabled = true;
                OKButton.Enabled = false;

                progressBar1.Visible = true;
                LableProgressTitle.Visible = true;
                LabelTimeLeft.Visible = true;
            }
            if( progressState <= 1)
            {
                PauseButton.Text = "Pause";
            }
            else
            {
                PauseButton.Text = "Resume";
            }
        }
        private void ProgressStatusCheckThread()
        {
            while (checkProgress)
            {
                Percentage = clTrim.Percentage;
                progressTitle = clTrim.progressTitle;
                timeLeft = CLUnit.formatTime(clTrim.TimeLeft);
                UpdateProgress();
                Thread.Sleep(1000);
            }
        }
       
        private void UpdateMethodCombo()
        {
            comboBox3.Items.Clear();

            if (comboBox1.SelectedIndex < 0) return;
            if (comboBox2.SelectedIndex < 0) return;

            int id1 = models[comboBox1.SelectedIndex];
            int id2 = objects[comboBox1.SelectedIndex];

            C3DObjectBase obj1 = C3DData.GetObjectByKey(id1); //model
            C3DObjectBase obj2 = C3DData.GetObjectByKey(id2); //target
            //
            //obj.type == ShapeEnum.Mesh ||
            //obj.type == ShapeEnum.Polygon ||
            //obj.type == ShapeEnum.Triangles ||
            //obj.type == ShapeEnum.Slicer )

            if (obj1.type == ShapeEnum.Triangles ||
                 obj1.type == ShapeEnum.Polygon)//polygon 0
            {
                comboBox3.Items.Add("keep outside"); //0 
                comboBox3.Items.Add("keep inside");  //1
               // comboBox3.Items.Add("keep intersection");//2
            }
            else if (obj1.type == ShapeEnum.Mesh)
            {
                comboBox3.Items.Add("keep left"); //0 
                comboBox3.Items.Add("keep right");//1
               // comboBox3.Items.Add("keep on line");//2
            }
            else if (obj1.type == ShapeEnum.Slicer)
            {
                if( ((CSlicer) obj1).Closed )
                {
                    comboBox3.Items.Add("keep inside"); //0 
                    comboBox3.Items.Add("keep outside");//1
                     //comboBox3.Items.Add("keep on line");//2
                }
                else
                {
                    comboBox3.Items.Add("keep left"); //0 
                    comboBox3.Items.Add("keep right");//1
                    comboBox3.Items.Add("keep on line");//2
                }
            }           
            comboBox3.SelectedIndex = 0;
        }  

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMethodCombo();
        }       

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMethodCombo();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            checkProgress = false;
            Thread.Sleep(1000);
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Pausebutton_Click(object sender, EventArgs e)
        {
            if (progressState == 1)
            {
                if (cutThread != null && cutThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    cutThread.Suspend();
#pragma warning restore CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                progressState = 2;                
            }
            else if (progressState == 2)
            {
                if (cutThread != null && cutThread.IsAlive)
#pragma warning disable CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    cutThread.Resume();               
#pragma warning restore CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                progressState = 1;                
            }
            UpdateButtonState();
        }
        private void Stop_Click(object sender, EventArgs e)
        {
            if (progressState < 1) return;
            
            
            bool resume = true;
            if (progressState == 2) resume = false;

            //先暂停，等待确认
            if (cutThread != null && cutThread.IsAlive)
            {
                if (progressState == 1)
                { 
#pragma warning disable CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    cutThread.Suspend();
#pragma warning restore CS0618 // '“Thread.Suspend()”已过时:“Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    progressState = 2;
                    UpdateButtonState();
                }
            }

            if (MessageBox.Show("Are you sure to abort current progress?\n Works have done will not be saved.", "Abort the progress?",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                try 
                {
                    cutThread.Abort();
                    
                }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
                catch(Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
                {
                    progressTitle = "Progress Canceled!";
                }
                
                progressState = 0;
                Thread.Sleep(1200);
                UpdateButtonState();
                checkProgress = false;                
            }
            else //恢复线程
            {
                if (progressState == 2 && resume )
                {
#pragma warning disable CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    cutThread.Resume();
#pragma warning restore CS0618 // '“Thread.Resume()”已过时:“Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202”
                    progressState = 1;
                    UpdateButtonState();
                }
            }
            
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("no model object selected.");
                return;
            }
            if (comboBox2.SelectedIndex < 0)
            {
                MessageBox.Show("please select an object to cut.");
                return;
            }

            int id1 = models[comboBox1.SelectedIndex];
            int id2 = objects[comboBox2.SelectedIndex];

            if (id1 == id2)
            {
                MessageBox.Show("please select two diffrent object.");
                return;
            }

            int methodPara = comboBox3.SelectedIndex;
            if (methodPara < 0)
            {
                MessageBox.Show("please select method.");
                return;
            }
            C3DObjectBase obj1 = C3DData.GetObjectByKey(id1);
            C3DObjectBase obj2 = C3DData.GetObjectByKey(id2);

            if (!GetSelected()) 
            {
                MessageBox.Show("Invalid parameters.");
                return;
            }
            if (selectedTarget.type != ShapeEnum.Grid3D)
            {
                MessageBox.Show("selected target not a supported type.");
                return;
            }

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
            
            //启动进度线程
            if (checkThread == null || !checkThread.IsAlive)
            {
                checkProgress = true;
                checkThread = new Thread(ProgressStatusCheckThread);
                checkThread.Start();
            }

            InitProgressBar();
            StartTrimThread();
        }

       
    }
}
