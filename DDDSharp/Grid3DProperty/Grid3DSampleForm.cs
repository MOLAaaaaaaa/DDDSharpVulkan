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

using DDDSharp;
namespace DDDSharp.Grid3DProperty
{
    public partial class Grid3DSampleForm : Form
    {
        public C3DGridData data = null;
        public Grid3DSampleForm(C3DGridData _data)
        {
            data = _data;
            InitializeComponent();
            textBox1.Text = "1";
            textBox2.Text = "1";
            textBox3.Text = "1";
        }

        private void OK_Click(object sender, EventArgs e)
        {
            try 
            {
                int stepx = int.Parse(textBox1.Text);
                int stepy = int.Parse(textBox2.Text);
                int stepz = int.Parse(textBox3.Text);
                string path = textBox4.Text;    
                if(path.Length < 1 || stepx <=0 || stepy <=0 || stepz<=0 )
                {
                    MessageBox.Show(AppLocalization.IsChinese ? "参数不正确。" : "Parameters are not correct.");
                    return;
                }
                Cursor = Cursors.WaitCursor; 
                data.SampleTo(path, stepx, stepy, stepz);
                Cursor = Cursors.Default;
                MessageBox.Show(AppLocalization.IsChinese ? "网格数据已完成采样！" : "Gridded data sampled!");
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
                return;
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.XYZVFormatLineFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                textBox4.Text = dlg.FileName;
            }
        }
    }
}
