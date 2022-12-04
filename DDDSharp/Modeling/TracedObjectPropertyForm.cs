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
    public partial class TracedObjectPropertyForm : Form
    {
        public Polygon2D obj = null;       
        public TracedObjectPropertyForm()
        {
            InitializeComponent();
        }
        void UpdateButtonState()
        {
            if (comboBox1.SelectedIndex == 0)
                FillColorButton.Enabled = true;
            else
                FillColorButton.Enabled = false;
        }
        private void TracedObjectPropertyForm_Load(object sender, EventArgs e)
        {
            if(obj != null)
            {
                textBox1.Text = obj.Name;
                textBox2.Text = obj.PropertyValue.ToString();
                FillColorButton.BackColor = obj.fillColor;
                BorderColorButton.BackColor = obj.lineColor;
                comboBox1.Items.Add("Enclosed Layer");
                comboBox1.Items.Add("Section Line");
                if (obj.IsClosed) comboBox1.SelectedIndex = 0;
                else comboBox1.SelectedIndex = 1;

                comboBox2.Items.Add("Squaleat");
                comboBox2.Items.Add("Spline");
                comboBox2.SelectedIndex = 0;
                UpdateButtonState();
            }            
        }

        private void OK_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            if( name.Length < 1 )
            {
                MessageBox.Show("Please input the object name.");
                return;
            }
            double val = 0;
            if( !double.TryParse(textBox2.Text, out val) )
            {
                MessageBox.Show("Please input valid property value.");
                return;
            }
            obj.Name = name;
            obj.PropertyValue = val;
            
            if (comboBox1.SelectedIndex == 0) obj.IsClosed = true;
            else obj.IsClosed = false;

            obj.fillColor = FillColorButton.BackColor;
            obj.lineColor = BorderColorButton.BackColor;

            if (comboBox2.SelectedIndex == 0)//最小二乘拟合
            {

            }                
            else if (comboBox2.SelectedIndex == 1)//曲线平滑5点3次
            {
                Cursor = Cursors.WaitCursor;
                List<Vector64> lists = new List<Vector64>();
                int n = obj.points.Count;
                for (int i = 0; i < n / 5; i++)
                {
                    CurveFit cf = new CurveFit();

                    for(int j=0;j<5;j++) cf.AddPoint(obj.points[i*5+j]);

                    List<Vector64>pp = cf.Fit();
                    foreach (Vector64 p in pp)  lists.Add(p);
                }
                obj.points.Clear();
                foreach (Vector64 p in lists)
                {
                    obj.Add(p.x, p.y, p.z);
                }

                Cursor = Cursors.Default;                
            }


            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FillColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = FillColorButton.BackColor;
            if( cd.ShowDialog()== DialogResult.OK )
            {
                FillColorButton.BackColor = cd.Color;
            }
        }

        private void BorderColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = BorderColorButton.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                BorderColorButton.BackColor = cd.Color;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }
    }
}
