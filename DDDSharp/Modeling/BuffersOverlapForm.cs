using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
using MathNet.Numerics;

namespace DDDSharp
{
    public partial class BuffersOverlapForm : Form
    {
        struct LayerModelStruct
        {
            public string file;
            public bool reset;
            public double value;
            public LayerModelStruct(string _file, bool _reset, double _value)
            {
                file = _file;
                reset = _reset;
                value = _value;
            }
        }

        ScatteredPoints result = new ScatteredPoints();
        C3DGridData grid3d = new C3DGridData();
        List<LayerModelStruct> files = new List<LayerModelStruct>();
        List<ScatteredPoints> buffers = new List<ScatteredPoints>();
        double minx, maxx, miny, maxy, minz, maxz,minv,maxv;
        int xNum, yNum, zNum;
        double xStep, yStep, zStep;

        List<Vector64> points = new List<Vector64>();
        
#pragma warning disable CS0414 // 字段“BuffersOverlapForm.changing”已被赋值，但从未使用过它的值
        bool changing = false;
#pragma warning restore CS0414 // 字段“BuffersOverlapForm.changing”已被赋值，但从未使用过它的值

        private void UpdateStep(object sender, EventArgs e)
        {
            UpdateStepFun();
        }

        private void UpdateNum(object sender, EventArgs e)
        {

        }

        private void OverlapButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            grid3d.Clear();
            result.Clear();

            GetValues();

            grid3d = new C3DGridData(xNum, yNum, zNum);
            for (int i = 0; i < grid3d.pGridData.Length; i++) grid3d.pGridData[i] = 0;


            int[] counts = new int[xNum*yNum*zNum];
            float[] values = new float[xNum * yNum * zNum];

            grid3d.minx = minx;
            grid3d.miny = miny;
            grid3d.minz = minz;
            grid3d.maxx = maxx;
            grid3d.maxy = maxy;
            grid3d.maxz = maxz;
            grid3d.minv = 0;
            grid3d.maxv = 1;
            int ix, iy, iz;
            double x, y, z, val,scale = 1;
            for (int i = 0;i<buffers.Count;i++)
            {
                ScatteredPoints obj = buffers[i];
                scale = files[i].value / 100;

                for (int j = 0; j < counts.Length; j++) 
                { 
                    counts[j] = 0; 
                    values[j] = 0; 
                }

                foreach (Vector32 p in obj.points)
                {
                    if (p.x < minx || p.x > maxx ||
                        p.y < miny || p.y > maxy ||
                        p.z < minz || p.z > maxz) continue;
                    if (p.v < 0.2) continue; //低值不叠加

                    ix = (int)((p.x - minx) / xStep);
                    iy = (int)((p.y - miny) / yStep);
                    iz = (int)((p.z - minz) / zStep);

                    counts[ix + iy * xNum + iz * xNum * yNum]++;
                    values[ix + iy * xNum + iz * xNum * yNum] += p.v;
                }

                //对应网格叠加
                for (int j = 0; j < counts.Length; j++)
                {
                    if( counts[j] > 0 )grid3d.pGridData[j] += values[j] / counts[j];
                }
            }
            counts = null;
            values = null;

            grid3d.UpdateDataRange();
            double v;
            double v1 = grid3d.minv;
            double v2 = grid3d.maxv;

            //normalize
            for (int j = 0; j < grid3d.pGridData.Length; j++)
            {
                v = grid3d.pGridData[j];
                grid3d.pGridData[j] =(float)( (v - v1) / (v2 - v1) );
            }
            
            grid3d.minv = 0;
            grid3d.maxv = 1;

            //add to scattered points
            for (iz = 0; iz < zNum; iz++)
            {
                for (iy = 0; iy < yNum; iy++)
                {
                    for (ix = 0; ix < xNum; ix++)
                    {
                        val = grid3d.pGridData[ix + iy * xNum + iz * xNum * yNum];
                        if ( val <= 0) continue;

                        x = minx + ix * xStep;
                        y = miny + iy * yStep;
                        z = minz + iz * zStep;
                        result.AddPoint(x, y, z, val);
                    }
                }
            }
            
            this.Cursor = Cursors.Default;

            MessageBox.Show("Done.");
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "3DGrid File(*.3DGrid;)|*.3DGrid|XYZV Points(*.dat;*.csv;*.txt)|*.dat;*.csv;*.txt|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;

                    if( dlg.FilterIndex == 1 )
                    {
                        if (grid3d.SaveAs(dlg.FileName)) MessageBox.Show("Exported.");
                        else MessageBox.Show("export faild.\n" + grid3d.errMessage);
                    }
                    else
                    {
                        if (result.ExportData(dlg.FileName)) MessageBox.Show("Exported.");
                        else MessageBox.Show("export faild.\n" + result.errMessage);
                    }
                    

                    this.Cursor = DefaultCursor;
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            if (sel >= files.Count) return;

            PercentageGroupBox.Text = Path.GetFileName(files[sel].file);
            PercentageTextBox1.Text = files[sel].value.ToString(); 
        }

        private void ModifyButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            if (sel >= files.Count) return;
            double val = 0;
            if( !double.TryParse(PercentageTextBox1.Text,out val) )
            {
                MessageBox.Show("Invalid percentage.");
                return;
            }
            LayerModelStruct ls = files[sel];
            ls.value = val;
            files[sel] = ls;
            UpdateList();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (files.Count < 1) return;
            if( checkBox1.Checked)
            {
                double val = 100 / files.Count;
                for(int i=0;i<files.Count;i++)
                {
                    LayerModelStruct ls = files[i];
                    ls.value = val;
                    files[i] = ls;
                }
                UpdateList();
            }
        }

        
        private void BuffersOverlapForm_Load(object sender, EventArgs e)
        {

        }

        public BuffersOverlapForm()
        {
            InitializeComponent();

            TextBoxXNum.Text = "101";
            TextBoxYNum.Text = "101";
            TextBoxZNum.Text = "101";
        }
        
        void UpdateList()
        {
            listBox1.Items.Clear();
            string name;
            for (int i = 0; i < files.Count; i++)
            {
                name = Path.GetFileName(files[i].file);
                if (files[i].reset)
                {
                    name += ",  " + files[i].value;
                }
                listBox1.Items.Add(name);
            }
        }

        void UpdateDataRange()
        {
            minx = maxx = 0;
            miny = maxy = 0;
            minz = maxz = 0;
            minv = maxv = 0;
            ScatteredPoints obj;
            for(int i = 0; i < buffers.Count;i++)
            {
                obj = buffers[i];
                if(i == 0) 
                {
                    minx = obj.Minx;
                    miny = obj.Miny;
                    minz = obj.Minz;
                    minv = obj.Minv;

                    maxx = obj.Maxx;
                    maxy = obj.Maxy;
                    maxz = obj.Maxz;
                    maxv = obj.Maxv;
                }
                else
                {
                    if (obj.Minx < minx) minx = obj.Minx;
                    if (obj.Miny < miny) miny = obj.Miny;
                    if (obj.Minz < minz) minz = obj.Minz;
                    if (obj.Minv < minv) minv = obj.Minv;

                    if (obj.Maxx > maxx) maxx = obj.Maxx;
                    if (obj.Maxy > maxy) maxy = obj.Maxy;
                    if (obj.Maxz > maxz) maxz = obj.Maxz;
                    if (obj.Maxv > maxv) maxv = obj.Maxv;
                }                
            }            
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "XYZV Points(*.dat;*.csv;*.txt)|*.dat;*.csv;*.txt|all files(*.*)|*.*";
                    dlg.Multiselect = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        double val = 100 / dlg.FileNames.Length;

                        for (int i = 0; i < dlg.FileNames.Length; i++)
                        {
                            ScatteredPoints st = new ScatteredPoints();
                            if (st.ImportData(dlg.FileNames[i]))
                            {
                                st.UpdateRange();
                                buffers.Add(st);
                                files.Add(new LayerModelStruct(dlg.FileNames[i], false, val));                                
                            }                            
                        }
                    }//if (dlg.ShowDialog() == DialogResult.OK)
                }//using (var dlg = new OpenFileDialog())
                UpdateList();
                UpdateDataRange();
                UpdateRangeTextBoxes();
                UpdateStepFun();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void GetValues()
        {
            double.TryParse(TextBoxX1.Text, out minx);
            double.TryParse(TextBoxY1.Text, out miny);
            double.TryParse(TextBoxZ1.Text, out minz);
            double.TryParse(TextBoxX2.Text, out maxx);
            double.TryParse(TextBoxY2.Text, out maxy);
            double.TryParse(TextBoxZ2.Text, out maxz);

            double.TryParse(TextBoxStepX.Text, out xStep);
            double.TryParse(TextBoxStepY.Text, out yStep);
            double.TryParse(TextBoxStepZ.Text, out zStep);
            int.TryParse(TextBoxXNum.Text, out xNum);
            int.TryParse(TextBoxYNum.Text, out yNum);
            int.TryParse(TextBoxZNum.Text, out zNum);
        }
        void UpdateRangeTextBoxes()
        {
            TextBoxX1.Text = minx.ToString();
            TextBoxX2.Text = maxx.ToString();
            TextBoxY1.Text = miny.ToString();
            TextBoxY2.Text = maxy.ToString();
            TextBoxZ1.Text = minz.ToString();
            TextBoxZ2.Text = maxz.ToString();
        }
        void UpdateStepFun()
        {
            GetValues();
            if( xNum > 1 ) xStep = (maxx - minx) / (xNum-1);
            if (yNum > 1) yStep = (maxy - miny) / (yNum-1);
            if (zNum > 1) zStep = (maxz - minz) / (zNum-1);
            TextBoxStepX.Text = xStep.ToString();
            TextBoxStepY.Text = yStep.ToString();
            TextBoxStepZ.Text = zStep.ToString();
        }
        void UpdateNumFun()
        {
            GetValues();
            if (xStep > 0) xNum = (int)( (maxx - minx) / xStep ) + 1;
            if (yStep > 0) yNum = (int)((maxy - miny) / yStep) + 1;
            if (zStep > 0) zNum = (int)((maxz - minz) / zStep) + 1;
            TextBoxXNum.Text = xNum.ToString();
            TextBoxYNum.Text = yNum.ToString();
            TextBoxZNum.Text = zNum.ToString();
        }
    }
}
