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
    public partial class LocationDlg : Form
    {
        public Vector64 screenPoint,spacePoint;
        public AxisEnum axis;
        public LocationDlg()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            double x = 0, y = 0, z = 0;
            if( !double.TryParse(textBox5.Text,out x) )
            {
                MessageBox.Show("not a valid coordinate.");
                return;
            }
            if (!double.TryParse(textBox6.Text, out y))
            {
                MessageBox.Show("not a valid coordinate.");
                return;
            }
            if (!double.TryParse(textBox7.Text, out z))
            {
                MessageBox.Show("not a valid coordinate.");
                return;
            }
            spacePoint = new Vector64(x,y,z);
            axis = (AxisEnum)comboBox1.SelectedIndex;

            DialogResult = DialogResult.OK;
            
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LocationDlg_Load(object sender, EventArgs e)
        {
            string[] names = Enum.GetNames(typeof(AxisEnum));
            for (int i = 0; i < names.Length; i++)
            {
                comboBox1.Items.Add(names[i]);
            }
            comboBox1.SelectedIndex = (int)axis;
            textBox1.Text = "XOY";
            textBox2.Text = ((double)screenPoint.x).ToString();
            textBox3.Text = ((double)screenPoint.y).ToString();
            textBox4.Text = ((double)screenPoint.z).ToString();
            textBox5.Text = ((double)spacePoint.x).ToString();
            textBox6.Text = ((double)spacePoint.y).ToString();
            textBox7.Text = ((double)spacePoint.z).ToString();
        }
    }
}
