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

using DDDSharp;
namespace DDDSharp.Boreholes
{
    public partial class BoreholesSlicerForm : Form
    {
        public CBoreholes Boreholes = null;
        double minx, miny, maxx, maxy;
        RectRuler ruler = new RectRuler();
        int dotSize = 12;
        int smallDotSize = 8;        
        List<int> selectedIndices = new List<int>();
        public string selectedProperty = "";
        public List<CBorehole> selectedBoreholes = new List<CBorehole>();
        void DrawBoreholes(Graphics g)
        {
            if (Boreholes == null) return;
            double x, y;

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            for(int i = 0;i<Boreholes.Count;i++)
            {
                CBorehole bh = Boreholes[i];
                x = bh.Position.X;
                y = bh.Position.Y;
                ruler.LPtoDP(ref x, ref y);
                if (selectedIndices.IndexOf(i) >= 0)
                {
                    g.DrawEllipse(Pens.Red, (float)(x- smallDotSize/2), (float)(y- smallDotSize/2), smallDotSize, smallDotSize);
                    g.DrawEllipse(Pens.Red, (float)(x - dotSize/2), (float)(y - dotSize/2), dotSize, dotSize);
                    g.DrawString(bh.Name, DefaultFont, Brushes.Red, (float)x, (float)(y + dotSize/2 + 2), format);
                }
                else
                {
                    g.DrawEllipse(Pens.Black, (float)(x - smallDotSize / 2), (float)(y - smallDotSize / 2), smallDotSize, smallDotSize);
                    g.DrawEllipse(Pens.Black, (float)(x - dotSize / 2), (float)(y - dotSize / 2), dotSize, dotSize);
                    g.DrawString(bh.Name, DefaultFont, Brushes.Blue, (float)x, (float)(y + dotSize/2 + 2), format);
                }
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            ruler.cursorPosition = e.Location;
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            float ptx = e.X;
            float pty = e.Y;
            
            for (int i = 0; i < Boreholes.Count; i++)
            {
                CBorehole bh = Boreholes[i];
                double x = bh.Position.X;
                double y = bh.Position.Y;
                ruler.LPtoDP(ref x, ref y);
                x = x - dotSize / 2;
                y = y - dotSize / 2;
                RectangleF rect = new RectangleF((float)x, (float)y, dotSize, dotSize);
                if( rect.Contains(ptx, pty) )
                {
                    if (selectedIndices.IndexOf(i) >= 0)
                        selectedIndices.Remove(i);
                    else selectedIndices.Add(i);
                    pictureBox1.Invalidate();
                    break;
                }
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void Create_Click(object sender, EventArgs e)
        {
            if( selectedIndices.Count < 2 )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "选择的钻孔数量不足。" : "Not enough boreholes selected.");
                return;
            }
            
            for (int i = 0; i < selectedIndices.Count; i++)
                selectedBoreholes.Add(Boreholes.pData[selectedIndices[i]]);

            selectedProperty = comboBox1.SelectedItem.ToString();

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void DrawSelectedBoreholes(Graphics g)
        {
            if (selectedIndices.Count == 0) return;
            int i = 0;
            PointF p1 = new PointF(), p;
            
            ArrowRenderer ar = new ArrowRenderer();
            ar.Width = 10;
            ar.Theta = 20;            
            foreach (int k in selectedIndices)
            {
                CBorehole bh = Boreholes[k];
                double x = bh.Position.X;
                double y = bh.Position.Y;
                ruler.LPtoDP(ref x, ref y);
                p = new PointF((float)x, (float)y);
                if (i > 0)
                {
                    double r = Math.Sqrt((p1.X - p.X) * (p1.X - p.X) + (p1.Y - p.Y) * (p1.Y - p.Y));
                    if (r > 0)
                    {
                        double dy = (ar.Width) * (p.Y - p1.Y) / r;
                        double dx = (ar.Width) * (p.X - p1.X) / r;                        
                        ar.DrawArrow(g, Pens.Blue, Brushes.BlueViolet, p1.X,p1.Y, (float)(p.X - dx),(float)(p.Y - dy) );
                    }
                }
                i++;
                p1 = p;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
            ruler.SetDrawRect(rect);
            ruler.SetMarine(50, 10, 10, 50);
            ruler.leftRuler.SetValuesRange(miny,maxy);
            ruler.bottomRuler.SetValuesRange(minx, maxx);
            ruler.Draw(e.Graphics);
            DrawSelectedBoreholes(e.Graphics);
            DrawBoreholes(e.Graphics);
            
            ruler.DrawCursor(e.Graphics);
        }
        
        public BoreholesSlicerForm(CBoreholes boreholes)
        {
            InitializeComponent();
            Boreholes = boreholes;
            minx = Boreholes.minx;
            miny = Boreholes.miny;
            maxx = Boreholes.maxx;
            maxy = Boreholes.maxy;
            ApplyLanguage();
        }  

        private void ApplyLanguage()
        {
            bool zh = AppLocalization.IsChinese;
            Create.Text = zh ? "创建切片" : "Create Slicer";
            Cancel.Text = zh ? "关闭" : "Close";
            groupBox1.Text = zh ? "属性" : "Properties";
            Text = zh ? "钻孔切片" : "Boreholes Slicer";
        }
        
        private void BoreholesSlicerForm_Load(object sender, EventArgs e)
        {
            List<string>properties = Boreholes.GetAllProperties();

            for (int i = 0; i < properties.Count; i++)
            { 
                comboBox1.Items.Add(properties[i]); 
            }

            comboBox1.SelectedIndex = 1;
        }
    }
}
