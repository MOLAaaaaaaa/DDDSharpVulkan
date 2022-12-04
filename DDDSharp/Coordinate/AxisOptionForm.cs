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
    public partial class AxisOptionForm : Form
    {
        public AxisOptionForm()
        {
            InitializeComponent();
        }
       
        void UpdateCoordinates()
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;

            if (CDataModel.IsEarthMapVision) radioButton3.Checked = true;
            else
            { 
                if (CDataModel.IsGeoCoordinateSystem) radioButton2.Checked = true; 
                else radioButton1.Checked = true;
            }

            textBoxX1.Text = CDataModel.xAxisText1;
            textBoxY1.Text = CDataModel.yAxisText1;
            textBoxZ1.Text = CDataModel.zAxisText1;
            TextSizeBox1.Text = CDataModel.axisFontScale1.ToString();

            textBoxX2.Text = CDataModel.xAxisText2;
            textBoxY2.Text = CDataModel.yAxisText2;
            textBoxZ2.Text = CDataModel.zAxisText2;
            TextSizeBox2.Text = CDataModel.axisFontScale2.ToString();

            textBoxX3.Text = CDataModel.xEarthAxisText;
            textBoxY3.Text = CDataModel.yEarthAxisText;
            textBoxZ3.Text = CDataModel.zEarthAxisText;
            TextSizeBox3.Text = CDataModel.axisFontScale3.ToString();
        }
        private void AxisOptionForm_Load(object sender, EventArgs e)
        {   
            UpdateCoordinates();
        }

        private void Okbutton1_Click(object sender, EventArgs e)
        {
            if ( radioButton1.Checked )
            {
                if (!float.TryParse(TextSizeBox1.Text, out CDataModel.axisFontScale1))
                {
                    MessageBox.Show("Text size is not correct.");
                    return;
                }
                CDataModel.xAxisText1 = textBoxX1.Text;
                CDataModel.yAxisText1 = textBoxY1.Text;
                CDataModel.zAxisText1 = textBoxZ1.Text;
                CDataModel.IsEarthMapVision = false;
                CDataModel.IsGeoCoordinateSystem = false;
            }
            if (radioButton2.Checked)
            {
                if (!float.TryParse(TextSizeBox2.Text, out CDataModel.axisFontScale2))
                {
                    MessageBox.Show("Text size is not correct.");
                    return;
                }
                CDataModel.xAxisText1 = textBoxX2.Text;
                CDataModel.yAxisText1 = textBoxY2.Text;
                CDataModel.zAxisText1 = textBoxZ2.Text;
                CDataModel.IsEarthMapVision = false;
                CDataModel.IsGeoCoordinateSystem = true;
            }
            if (radioButton3.Checked)
            {                
                if ( !float.TryParse(TextSizeBox3.Text, out CDataModel.axisFontScale3) )
                {
                    MessageBox.Show("Text size is not correct.");
                    return;
                }
                CDataModel.xEarthAxisText = textBoxX3.Text;
                CDataModel.yEarthAxisText = textBoxY3.Text;
                CDataModel.zEarthAxisText = textBoxZ3.Text;
                CDataModel.IsEarthMapVision = true;
                //CDataModel.IsGeoCoordinateSystem = true;
            }            
            
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancelbutton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {            
            if( radioButton1.Checked )
            {
                radioButton2.Checked = false;
                radioButton3.Checked = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                radioButton3.Checked = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                radioButton1.Checked = false;
                radioButton2.Checked = false;
            }
        }
    }
}
