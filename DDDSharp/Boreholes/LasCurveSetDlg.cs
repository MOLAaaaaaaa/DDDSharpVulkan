using System;
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
    public partial class LasCurveSetDlg : Form
    {
        public CBoreholes boreholes = null;
        public CColorScale colorscale = new CColorScale();
        public float[] curValues = null;
        float nullValue = -999.25f;
        public List<BoreholeCurve> curveList = new List<BoreholeCurve>();

        public LasCurveSetDlg()
        {
            InitializeComponent();
            InvalidValueTextBox.Text = nullValue.ToString();
        }
        
        private void LasCurveSetDlg_Load(object sender, EventArgs e)
        {
            if( boreholes != null && boreholes.Count > 0 )
            {
                CBorehole bh = boreholes[0];
                for(int i=0;i< bh.Curves.lasData.CurveInformation.Count;i++)
                {
                    comboBox1.Items.Add(bh.Curves.lasData.CurveInformation[i].Mnemonic);
                    comboBox2.Items.Add(bh.Curves.lasData.CurveInformation[i].Mnemonic);
                }
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = -1;
                
                InvalidValueTextBox.Text = bh.Curves.lasData.nullValue.ToString();

                string text = "total boreholes " + boreholes.Count + ".";
                AddInfo(text);
            }            
        }
        void GetValueRange(int index,out double minv,out double maxv)
        {
            double v1, v2;
            minv = maxv = 0;
            if (boreholes == null) return;
            LasFileData data;            
            int id = 0;
            for( int i = 0; i < boreholes.Count; i++ )
            {
                data = boreholes[i].Curves.lasData;

                if (!data.GetDataRange(index, out v1, out v2))
                    continue;

                if (id == 0) { minv = v1;maxv = v2; }
                else
                {
                    if (v1 < minv) minv = v1;
                    if (v2 > maxv) maxv = v2;
                }
                
                id++;
            }            
        }
        void UpdateValueRange()
        {
            MinValue_textBox.Text = "0";
            MaxValue_textBox.Text = "0";
            int index = comboBox2.SelectedIndex;
            if (index < 0) return;
            double v1, v2;
            GetValueRange(index, out v1, out v2);
            MinValue_textBox.Text = v1.ToString();
            MaxValue_textBox.Text = v2.ToString();
        }       

        void UpdateList()
        {
            listBox1.Items.Clear();
            if (boreholes == null) return;

            for (int i = 0; i < curveList.Count; i++)
            {
                BoreholeCurve ct = curveList[i];
                listBox1.Items.Add(ct.Name);
            }
        }
        void AddInfo(string text)
        {
            listBox2.Items.Add(text);            
        }

        //检查指定的列序号和名称是否存在于所有钻孔
        void CheckColuwn( int index, string name )
        {
            string text = "";
            for(int i = 0; i< boreholes.Count; i++ )
            {
                CBorehole bh = boreholes[i];
                if( !bh.Curves.IsColuwnExist(index,name) )
                {
                    text = "can not find the coluwn '" + name + "'in " +bh.Name; 
                    AddInfo(text);
                }
            }
        }
        void UpdateParaSelect()
        {
            int n = curveList.Count;
            int sel = listBox1.SelectedIndex;
            if ( sel < 0 || sel >= n ) return;
            BoreholeCurve ct = curveList[sel];
            comboBox1.SelectedIndex = ct.xIndex;
            comboBox2.SelectedIndex = ct.yIndex;
            MinValue_textBox.Text = ct.ColorScale.minv.ToString();
            MaxValue_textBox.Text = ct.ColorScale.maxv.ToString();
        }
        private void AddButton_Click(object sender, EventArgs e)
        {
            int xIndex = comboBox1.SelectedIndex;
            int yIndex = comboBox2.SelectedIndex;
            if( xIndex < 0 || yIndex < 0 )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择列。" : "No column selected.");
                return;
            }

            for(int i = 0; i< curveList.Count;i++)
            {
                if( curveList[i].yIndex == yIndex )
                {
                    MessageBox.Show(curveList[i].Name + (AppLocalization.IsChinese ? " 已存在。" : " already exists."));
                    return;
                }
            }

            BoreholeCurve ct = new BoreholeCurve();
            ct.xIndex = xIndex;
            ct.yIndex = yIndex;
            ct.xName = comboBox1.Text;
            ct.yName = comboBox2.Text;
            ct.Name = comboBox2.Text;

            //double v1 = ConvertData.StringToDouble(MinValue_textBox.Text);
            //double v2 = ConvertData.StringToDouble(MaxValue_textBox.Text);
            //colorscale.SetValueRange(v1, v2);
            ct.UpdateRange();
            ct.ColorScale.SetValueRange(ct.minValue,ct.maxValue);
            curveList.Add(ct);

            CheckColuwn(xIndex, comboBox1.Text);
            CheckColuwn(yIndex, comboBox2.Text);
            
            UpdateList();
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            if( curveList.Count < 1 )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择曲线。" : "No curve selected.");
                return;
            }

            //深度列选择
            int ndepth = comboBox1.SelectedIndex;
            if (ndepth < 0)
            {
                MessageBox.Show(AppLocalization.IsChinese ? "未选择深度列。" : "No depth selected.");
                return;
            }            

            if ( !float.TryParse(InvalidValueTextBox.Text, out nullValue) )
            {
                nullValue = float.NaN;
            }
            
            double[] minvs = new double[curveList.Count];
            double[] maxvs = new double[curveList.Count];

            for (int i = 0; i < boreholes.Count; i++ )
            {
                CBorehole bh = boreholes[i];
                bh.Curves.Clear();
                bh.Curves.depthIndex = ndepth;
                
                for( int j = 0; j < curveList.Count; j++ )                
                {
                    if (!bh.Curves.IsColuwnExist(curveList[j].xIndex, curveList[j].xName)) continue;
                    if (!bh.Curves.IsColuwnExist(curveList[j].yIndex, curveList[j].yName)) continue;
                    BoreholeCurve cv = curveList[j].Copy();
                    cv.CreateFromLAS(bh.Curves.lasData, nullValue);
                    cv.UpdateRange();
                    if (i == 0) 
                    { 
                        minvs[j] = cv.minValue;
                        maxvs[j] = cv.maxValue;
                    }
                    else
                    {
                        if (cv.minValue < minvs[j]) minvs[j] = cv.minValue;
                        if (cv.maxValue > maxvs[j]) maxvs[j] = cv.maxValue;
                    }
                    bh.Curves.Add(cv);
                }
                bh.CreateBaseLine(CDataModel.IsGeoCoordinateSystem);
                //bh.CreateBaselineFromLasDepth(ndepth,bh.Curves.lasData,CDataModel.IsGeoCoordinateSystem);
                bh.Alpha = 0.5f; //外部柱体透明显示
                bh.UpdateRange();
                boreholes[i] = bh;
            }
            
            boreholes.UpdateRange();

            boreholes.Radius = (float)(boreholes.MinWidth * 0.1);
            boreholes.Alpha = 0.5f;//外部柱体透明显示
            boreholes.ApplyToAll(true, minvs, maxvs );            

            DialogResult = DialogResult.OK;
            this.Close();
        }
       

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateValueRange();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }       

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            int n = curveList.Count;
            int sel = listBox1.SelectedIndex;
        
            if ( sel >= 0 && sel < n )
            {
                curveList.RemoveAt(sel);
                UpdateList();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateParaSelect();            
        }
    }
}
