using System;
using System.IO;
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
    public partial class ScatteredPointsLoadForm : Form
    {
        struct FilterStruct
        {
            bool except; //except or keep
            string XYZV; //
        }
        public ScatteredPoints sc = null;        
        public ScatteredPointsLoadForm(ScatteredPoints p)
        {
            InitializeComponent();
            sc = p;
        }
        void UpdateInfo()
        {
            if (sc == null) return;
            listBox1.Items.Clear();
            string info;
            FileInfo file = new FileInfo(sc.FilePath);
            double mb = 1024 * 1024;

            double required = Math.Round(sc.TotalRows * 16 / sc.Interval / mb,2);
            double available = PhysicalMemory.GetAvailableMemoryMB();

            info = "filesize:" + Math.Round(file.Length / mb,2) + "mb";
            listBox1.Items.Add(info);
            
            info = "total  row: " + sc.TotalRows;
            listBox1.Items.Add(info);

            info = "Memory Required: " + required;
            listBox1.Items.Add(info);

            info = "Memory Available: " + available;
            listBox1.Items.Add(info);

            info = "Loaded row: " + sc.points.Count;
            listBox1.Items.Add(info);

            info = "x range: " + sc.minx + " to " + sc.maxx;
            listBox1.Items.Add(info);

            info = "y range: " + sc.miny + " to " + sc.maxy;
            listBox1.Items.Add(info);

            info = "z range: " + sc.minz + " to " + sc.maxz;
            listBox1.Items.Add(info);

            info = "values: " + sc.minv + " to " + sc.maxv;
            listBox1.Items.Add(info);
        }
        private void ScatteredPointsLoadForm_Load(object sender, EventArgs e)
        {
            FilePathTextBox.Text = sc.FilePath;
            IntervalTextBox.Text = sc.Interval.ToString();
            RotateXTextBox.Text = "0";
            RotateYTextBox.Text = "0";
            RotateZTextBox.Text = "0";
            comboBox1.Items.Add("Except");
            comboBox1.Items.Add("Keep");
            UpdateInfo();
        }

        private void Interval_TextChanged(object sender, EventArgs e)
        {

        }

        private void Addto_Click(object sender, EventArgs e)
        {
            string line = comboBox1.SelectedItem.ToString();
            line += " | ";
            line += FilterTextBox.Text;
            listBox2.Items.Add(line);
        }

        private void OK_Click(object sender, EventArgs e)
        {

        }
    }
}
