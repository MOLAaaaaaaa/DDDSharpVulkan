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
    public partial class CModelRangeForm : Form
    {
        public CModelRangeForm()
        {
            InitializeComponent();
        }

        private void CModelRangeForm_Load(object sender, EventArgs e)
        {
            textBoxX1.Text = CDataModel.m_ModelOrg.X1.ToString();
            textBoxX2.Text = CDataModel.m_ModelOrg.X2.ToString();
            textBoxY1.Text = CDataModel.m_ModelOrg.Y1.ToString();
            textBoxY2.Text = CDataModel.m_ModelOrg.Y2.ToString();
            textBoxZ1.Text = CDataModel.m_ModelOrg.Z1.ToString();
            textBoxZ2.Text = CDataModel.m_ModelOrg.Z2.ToString();
        }

        private void Okbutton1_Click(object sender, EventArgs e)
        {
            CDataModel.m_ModelOrg.X1 = Convert.ToDouble(textBoxX1.Text);
            CDataModel.m_ModelOrg.X2 = Convert.ToDouble(textBoxX2.Text);
            CDataModel.m_ModelOrg.Y1 = Convert.ToDouble(textBoxY1.Text);
            CDataModel.m_ModelOrg.Y2 = Convert.ToDouble(textBoxY2.Text);
            CDataModel.m_ModelOrg.Z1 = Convert.ToDouble(textBoxZ1.Text);
            CDataModel.m_ModelOrg.Z2 = Convert.ToDouble(textBoxZ2.Text);
            
            CDataModel.CalculateModelSize();
            this.DialogResult = DialogResult.OK;
        }

        private void Cancelbutton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if( checkBox1.Checked )
            {
                CubeModel64 range = C3DData.GetObjectsRange();
                textBoxX1.Text = range.X1.ToString();
                textBoxX2.Text = range.X2.ToString();
                textBoxY1.Text = range.Y1.ToString();
                textBoxY2.Text = range.Y2.ToString();
                textBoxZ1.Text = range.Z1.ToString();
                textBoxZ2.Text = range.Z2.ToString();
            }
        }
    }
}
