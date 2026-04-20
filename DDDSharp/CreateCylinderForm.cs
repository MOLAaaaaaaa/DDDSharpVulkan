using DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDDSharp
{
    public partial class CreateCylinderForm : Form
    {
        public double height = 0;
        public double radius = 0;
        public int vertNum = 20;
        public int horNum = 20;
        public double xlocation = 0;
        public double ylocation = 0;
        public double zlocation = 0;
        public CreateCylinderForm()
        {
            InitializeComponent();
            UpdateToUI();
        }
        private void UpdateToUI()
        {
            HightTextBox1.Text = height.ToString();
            RadiusTextBox.Text = radius.ToString();
            VerticalTextBox.Text = vertNum.ToString();
            HorizontalTextBox.Text = horNum.ToString();
            LocationTextBoxX.Text = xlocation.ToString();
            LocationTextBoxY.Text = ylocation.ToString();
            LocationTextBoxZ.Text = zlocation.ToString();
            
            InfoLabel.Text = "X: " + CDataModel.m_ModelOrg.X1 + " to " + CDataModel.m_ModelOrg.X2 + "\n";
            InfoLabel.Text += "Y: " + CDataModel.m_ModelOrg.Y1 + " to " + CDataModel.m_ModelOrg.Y2 + "\n";
            InfoLabel.Text += "Z: " + CDataModel.m_ModelOrg.Z1 + " to " + CDataModel.m_ModelOrg.Z2;
        }

        private bool GetFromUI()
        {
            try
            {
                height = double.Parse(HightTextBox1.Text);
                radius = double.Parse(RadiusTextBox.Text);
                xlocation = double.Parse(LocationTextBoxX.Text);
                ylocation = double.Parse(LocationTextBoxY.Text);
                zlocation = double.Parse(LocationTextBoxZ.Text);
                vertNum = int.Parse(VerticalTextBox.Text);
                horNum = int.Parse(HorizontalTextBox.Text);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
            
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if( !GetFromUI() )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "参数无效。" : "Invalid parameters.");
                return;
            }            

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CreateCylinderForm_Load(object sender, EventArgs e)
        {
            radius = Math.Min(CDataModel.m_Model.GetWidth(0), CDataModel.m_Model.GetWidth(1))/ 2;
            height = radius;
            
            Vector64 p = CDataModel.m_Model.GetCenterPoint();
            xlocation = p.X;
            ylocation = p.Y;
            zlocation = p.Z;

            UpdateToUI();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
