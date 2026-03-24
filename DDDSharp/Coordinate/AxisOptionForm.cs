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
        Axis3DArrow xArrow3D1, yArrow3D1, zArrow3D1;
        Axis3DArrow xArrow3D2, yArrow3D2, zArrow3D2;
        Axis3DArrow xArrow3D3, yArrow3D3, zArrow3D3;
        public AxisOptionForm()
        {
            InitializeComponent();
        }
       
        void UpdateCoordinates()
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            if (CDataModel.IsEarthMapVision)
                radioButton3.Checked = true;
            else
            {
                if (CDataModel.IsGeoCoordinateSystem) 
                    radioButton2.Checked = true;
                else radioButton1.Checked = true;
            }            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = comboBox1.SelectedIndex;
            if (sel == 0) propertyGrid1.SelectedObject = xArrow3D1;
            else if (sel == 1) propertyGrid1.SelectedObject = yArrow3D1;
            else if (sel == 2) propertyGrid1.SelectedObject = zArrow3D1;
            else propertyGrid1.SelectedObject = null;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = comboBox2.SelectedIndex;
            if (sel == 0) propertyGrid2.SelectedObject = xArrow3D2;
            else if (sel == 1) propertyGrid2.SelectedObject = yArrow3D2;
            else if (sel == 2) propertyGrid2.SelectedObject = zArrow3D2;
            else propertyGrid2.SelectedObject = null;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = comboBox3.SelectedIndex;
            if (sel == 0) propertyGrid3.SelectedObject = xArrow3D3;
            else if (sel == 1) propertyGrid3.SelectedObject = yArrow3D3;
            else if (sel == 2) propertyGrid3.SelectedObject = zArrow3D3;
            else propertyGrid3.SelectedObject = null;
        }

        private void AxisOptionForm_Load(object sender, EventArgs e)
        {
            xArrow3D1 = CDataModel.xArrow3D.Copy();
            yArrow3D1 = CDataModel.yArrow3D.Copy();
            zArrow3D1 = CDataModel.zArrow3D.Copy();
            xArrow3D2 = CDataModel.xGeoArrow3D.Copy();
            yArrow3D2 = CDataModel.yGeoArrow3D.Copy();
            zArrow3D2 = CDataModel.zGeoArrow3D.Copy();
            xArrow3D3 = CDataModel.xEarthArrow3D.Copy();
            yArrow3D3 = CDataModel.yEarthArrow3D.Copy();
            zArrow3D3 = CDataModel.zEarthArrow3D.Copy();

            comboBox1.Items.Add("  X Axis  ");
            comboBox1.Items.Add("  Y Axis  ");
            comboBox1.Items.Add("  Z Axis  ");
            comboBox1.SelectedIndex = 0;

            comboBox2.Items.Add("  X Axis  ");
            comboBox2.Items.Add("  Y Axis  ");
            comboBox2.Items.Add("  Z Axis  ");
            comboBox2.SelectedIndex = 0;

            comboBox3.Items.Add("  X Axis  ");
            comboBox3.Items.Add("  Y Axis  ");
            comboBox3.Items.Add("  Z Axis  ");
            comboBox3.SelectedIndex = 0;

            propertyGrid1.SelectedObject = xArrow3D1;
            propertyGrid2.SelectedObject = xArrow3D2;
            propertyGrid3.SelectedObject = xArrow3D3;

            UpdateCoordinates();
        }

        private void Okbutton1_Click(object sender, EventArgs e)
        {
            CDataModel.xArrow3D = xArrow3D1.Copy();
            CDataModel.yArrow3D = yArrow3D1.Copy();
            CDataModel.zArrow3D = zArrow3D1.Copy();
            CDataModel.xGeoArrow3D = xArrow3D2.Copy();
            CDataModel.yGeoArrow3D = yArrow3D2.Copy();
            CDataModel.zGeoArrow3D = zArrow3D2.Copy();
            CDataModel.xEarthArrow3D = xArrow3D3.Copy();
            CDataModel.yEarthArrow3D = yArrow3D3.Copy();
            CDataModel.zEarthArrow3D = zArrow3D3.Copy();
            if ( radioButton1.Checked )
            {
                CDataModel.IsEarthMapVision = false;
                CDataModel.IsGeoCoordinateSystem = false;
            }
            if (radioButton2.Checked)
            {                
                CDataModel.IsEarthMapVision = false;
                CDataModel.IsGeoCoordinateSystem = true;
            }
            if (radioButton3.Checked)
            {  
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
