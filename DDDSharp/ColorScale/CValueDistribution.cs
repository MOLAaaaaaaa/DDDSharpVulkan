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
namespace DataCollection
{
    public partial class CValueDistribution : Form
    {
        public float[] data = null;
        float[] pValues = null;        
        double minx, maxx, miny, maxy;
        public double inMinv = 0, inMaxv = 0;
        public double outMinv = 0, outMaxv = 0;
        int nValue = 0;
        int barWidth = 4;
        float filterValue = 0.5f;//percent
        int marginBottom = 40;
        int marginLeft = 40;
        int marginRight = 20;
        public CValueDistribution()
        {
            InitializeComponent();
        }      
        void GetValueRange()
        {
            inMinv = inMaxv = 0;
            if (data == null) return;
            double v;
            for (int i = 0; i < data.Length; i++)
            {
                v = data[i];
                if (i == 0) inMinv = inMaxv = v;
                else
                {
                    if (v > inMaxv) inMaxv = v;
                    if (v < inMinv) inMinv = v;
                }
            }
        }
        void CalculateDistrubtion()
        {
            pValues = null;
            if (data == null) return;

            GetValueRange();
            if (inMinv >= inMaxv) return;

            nValue = 101;//pictureBox1.Width / barWidth;
            double vstep = (inMaxv - inMinv) / (nValue-1);

            //0-s,s-2s,2s-3s,...,(n-1)s-ns
            pValues = new float[nValue];
            if (pValues == null) return;
            for (int i = 0; i < nValue; i++) pValues[i] = 0;

            //map v1 - v2 to Len
            double v;
            int index;
            for (int i=0;i< data.Length; i++)
            {
                v = data[i];
                if (v >= inMinv && v <= inMaxv)
                {
                    index = (int)((v - inMinv) / vstep);
                    pValues[index]++;
                }
            }
            float maxYValue = 0;
            for (int i = 0; i < pValues.Length; i++)
            {
                if (pValues[i] > maxYValue) maxYValue = pValues[i];
            }
            //convert to percent
            for (int i = 0; i < nValue; i++)
            {
                pValues[i] = 100 * pValues[i] / maxYValue;
            }
        }

        private void CValueDistribution_Load(object sender, EventArgs e)
        {            
            outMinv = inMinv;
            outMaxv = inMaxv;
            FilterValue_textBox.Text = filterValue.ToString();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawDistrubtionBar(e.Graphics);
            DrawRuler(e.Graphics);
        }

        void LPtoDP(double x,double y,out double ox,out double oy)
        {
            ox = oy = 0;
            int width = pictureBox1.Width - (marginLeft + marginRight);//left and right margin
            int height = pictureBox1.Height - marginBottom; //bottom margin
            double nx = width / nValue;
            ox = marginLeft + x * nx;
            oy = y * height / 100;
        }

        private void pictureBox1_ClientSizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate(true);
        }

        private void AutoChooseButton_Click(object sender, EventArgs e)
        {
            if (nValue < 2) return;

            double filter;
            if( !double.TryParse(FilterValue_textBox.Text, out filter) )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "过滤值不正确。" : "Filter value not correct.");
                return;
            }
            
            outMinv = inMinv;
            outMaxv = inMaxv;

            //choose value1 -- from left to right
            double vstep = (inMaxv - inMinv) / (nValue - 1);
            for ( int i = 0; i < nValue; i++ )
            {
                if( pValues[i] >= filter) // 0 - 100 percentage
                {
                    outMinv = inMinv + i * vstep;
                    break;
                }
            }

            //choose value2 -- from right to left
            for (int i = 0; i < nValue; i++)
            {
                if (pValues[ nValue - 1 - i ] >= filter)
                {
                    outMaxv = inMaxv - i * vstep;
                    break;
                }
            }

            MinValue_textBox.Text = outMinv.ToString();
            MaxValue_textBox.Text = outMaxv.ToString();
        }

        void DrawRuler(Graphics e)
        {
            if (nValue < 2) return;

            int width = pictureBox1.Width ;//left and right margin
            int height = pictureBox1.Height; //bottom margin
            Pen pen1 = new Pen(Color.FromKnownColor(KnownColor.Black));            
            //e.DrawRectangle(pen1, 0, 0, width, height);
            double vstep = (inMaxv - inMinv) / (nValue - 1);
            double x,y,v;
            double xstep = ( width - (marginLeft + marginRight) ) / (nValue - 1);
            StringFormat _format = new StringFormat();
            _format.Alignment = StringAlignment.Center; //居中
            //_format.Alignment = StringAlignment.Far; //右对齐
            string str = "";            
            Font font = new Font("Times New Roman", 6F);
            Brush brush = Brushes.Blue;
            SizeF size;
            double lastTextPos = -1000;
            int shortLine = 6;
            for (int i=0;i<nValue;i++)
            {
                v = inMinv + i * vstep;
                x = marginLeft + i * xstep;                
                str = v.ToString("F6");
                str = str.TrimEnd('0');
                size = e.MeasureString(str, font);
                float y1 = height - marginBottom;
                float y2 = height - marginBottom + shortLine;
                float y3 = y2; //height - marginBottom + shortLine + size.Height;
                if (x - size.Width / 2 > lastTextPos)
                {
                    e.DrawLine(pen1, (float)x, y1, (float)x, y2);
                    e.DrawString(str, font, brush, (float)x, y3, _format);
                    lastTextPos = x + size.Width / 2;
                }
            }            
        }
        void DrawDistrubtionBar(Graphics e)
        {
            float width = pictureBox1.Width;
            float height = pictureBox1.Height;

            Pen pen1 = new Pen(Color.FromKnownColor(KnownColor.Red));
            Pen pen2 = new Pen(Color.FromKnownColor(KnownColor.Blue));
            e.DrawRectangle(pen1, 0, 0, width, height);

            if (data == null) return;
            if (pValues == null) CalculateDistrubtion();
            double x, y,ox,oy;
            for( int i = 0; i < nValue; i++ )
            {
                x = i;
                y = pValues[i];
                LPtoDP(x, y, out ox, out oy);
                y = height - marginBottom - oy ;
                
                e.DrawRectangle(pen2,(float)ox, (float)y, barWidth,(float)oy);
            }
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void OKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
