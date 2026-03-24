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
using GlmNet;
namespace DDDSharp
{
    public partial class ClosedValuesForm : Form
    {
        public CColorScale colorScale = null;
        public List<vec2> ClosedValues = new List<vec2>();
        public ClosedValuesForm()
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
        
        void UpdateListBox()
        {
            listBox1.Items.Clear();
            for(int i=0;i<ClosedValues.Count;i++)
            {
                vec2 p = ClosedValues[i];
                string ss = p.x + "\t" + p.y;
                listBox1.Items.Add(ss);
            }
            listBox1.SelectedIndex = -1;
        }

        private void CreateFromColorButton_Click(object sender, EventArgs e)
        {
            if( colorScale != null )
            {
                ClosedValues = colorScale.CreateClosedValues();
                UpdateListBox();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            vec2 p = ClosedValues[sel];
            textBox1.Text = p.x + "";
            textBox2.Text = p.y + "";
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            bool ret1 = float.TryParse(textBox1.Text, out float x);
            bool ret2 = float.TryParse(textBox2.Text, out float y);
            if( !ret1 || !ret2 )
            {
                MessageBox.Show("Invalid values.");
                return;
            }
            vec2 p = new vec2(x,y);
            ClosedValues.Add(p);
            listBox1.Items.Add(p.x + "\t" + p.y);
            listBox1.SelectedIndex = ClosedValues.Count - 1;
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            bool ret1 = float.TryParse(textBox1.Text, out float x);
            bool ret2 = float.TryParse(textBox2.Text, out float y);
            if (!ret1 || !ret2)
            {
                MessageBox.Show("Invalid values.");
                return;
            }
            vec2 p = new vec2(x, y);
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= ClosedValues.Count) return;
            ClosedValues[sel] = p;
            string ss = p.x + "\t" + p.y;
            listBox1.Items[sel] = ss;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0 || sel >= ClosedValues.Count) return;
            ClosedValues.RemoveAt(sel);
            listBox1.Items.RemoveAt(sel);
            
            if (sel >= ClosedValues.Count) sel = ClosedValues.Count - 1;
            if (sel < 0) sel = 0;
            if (ClosedValues.Count == 0) listBox1.SelectedIndex = -1;
            else listBox1.SelectedIndex = sel;
        }
    }
}
