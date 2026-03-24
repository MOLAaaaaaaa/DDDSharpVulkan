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
    public partial class AxisLableForm : Form
    {
        public Axis3DRuler xRuler, yRuler, zRuler;
        public AxisLableForm()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AxisLableForm_Load(object sender, EventArgs e)
        {
            xRuler = CDataModel.xAxisRuler.Copy();
            yRuler = CDataModel.yAxisRuler.Copy();
            zRuler = CDataModel.zAxisRuler.Copy();
            comboBox1.Items.Add("X Axis");
            comboBox1.Items.Add("Y Axis");
            comboBox1.Items.Add("Z Axis");
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = null;
            if (comboBox1.SelectedIndex == 0)
                propertyGrid1.SelectedObject = xRuler;
            if (comboBox1.SelectedIndex == 1)
                propertyGrid1.SelectedObject = yRuler;
            if (comboBox1.SelectedIndex == 2)
                propertyGrid1.SelectedObject = zRuler;
        }
    }
}
