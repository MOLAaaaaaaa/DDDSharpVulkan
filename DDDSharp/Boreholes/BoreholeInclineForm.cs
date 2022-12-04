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
namespace DDDSharp.Boreholes
{
    public partial class BoreholeInclineForm : Form
    {
        public CBorehole borehole = null;
        bool Modified = false;
        double minDepth, maxDepth;
        PointF cursor = new PointF(-1, -1);
        RectRuler ruler = new RectRuler();
        public BoreholeInclineForm(CBorehole _borehole)
        {
            InitializeComponent();
            borehole = _borehole.Copy();
        }
        void UpdateView()
        {
            listView1.Items.Clear();            
            if (borehole == null) return;
            
            BoreholeAnglesStruct an;
            
            this.listView1.BeginUpdate();

            for( int i = 0; i < borehole.boreholeAngles.Count; i++ )
            {
                an = borehole.boreholeAngles[i];
                ListViewItem liv = new ListViewItem((i + 1).ToString());        
                liv.SubItems.Add(an.Depth.ToString());
                liv.SubItems.Add(an.Azimuth.ToString());
                liv.SubItems.Add(an.Zenith.ToString() );                
                listView1.Items.Add(liv);
            }
            this.listView1.EndUpdate();
        }
        void UpdateView(int irow, BoreholeAnglesStruct an)
        {
            if (an == null) return;
            if (irow < 0 || irow >= listView1.Items.Count) return;
            ListViewItem liv = listView1.Items[irow];

            this.listView1.BeginUpdate();

            liv.SubItems[1].Text = an.Depth.ToString();
            liv.SubItems[2].Text = an.Azimuth.ToString();
            liv.SubItems[3].Text = an.Zenith.ToString();

            this.listView1.EndUpdate();
        }
        private void WellTraceForm_Load(object sender, EventArgs e)
        {
            UpdateDepth();
            Text = borehole.Name +  " Incline Parameters";
            UpdateView();
            pictureBox1.Invalidate();
            propertyGrid1.PropertySort = PropertySort.Categorized;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listView1.Items.Count;
            ListViewItem liv = listView1.Items.Add((n + 1).ToString());
            BoreholeAnglesStruct an = new BoreholeAnglesStruct();
            liv.SubItems.Add(an.Depth.ToString());
            liv.SubItems.Add(an.Azimuth.ToString());
            liv.SubItems.Add(an.Zenith.ToString());
            borehole.boreholeAngles.Add(an);            
            listView1.SelectedItems.Clear();
            listView1.SelectedIndices.Add(n);
            Modified = true;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if( listView1.SelectedIndices.Count > 0 )
            {
                int sel = listView1.SelectedIndices[0];
                propertyGrid1.SelectedObject = borehole.boreholeAngles[sel];
                pictureBox1.Invalidate();
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if( listView1.SelectedIndices.Count > 0 )
            {
                int sel = listView1.SelectedIndices[0];
                BoreholeAnglesStruct an = propertyGrid1.SelectedObject as BoreholeAnglesStruct;
                UpdateView(sel, an);
                if ( an.IsValid() ) 
                {
                    UpdateDepth();
                    pictureBox1.Invalidate(); 
                }
                Modified = true;
            }            
        }

        
        private void OK_Click(object sender, EventArgs e)
        {
            string errinfo = "";
            for(int i=0;i<borehole.boreholeAngles.Count;i++)
            {
                BoreholeAnglesStruct angle = borehole.boreholeAngles[i];
                if( !angle.IsValid() )
                {
                    errinfo += "invalid parameters on line " + (i + 1) + "\n";
                }
            }
            if( errinfo.Length > 1 )
            {
                MessageBox.Show(errinfo,"Invalid Parameters.");
                return;
            }

            this.Close(); DialogResult = DialogResult.OK;
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            if (Modified)
            {
                DialogResult ret = MessageBox.Show("Borhole Traces have been modified, Abort it anyway?", "data not saved",
                                                     MessageBoxButtons.YesNoCancel,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button3);
                if (ret == DialogResult.Cancel || ret == DialogResult.No) return;
                if (ret == DialogResult.Yes) { this.Close(); DialogResult = DialogResult.Cancel; }
            }
            else
            {
                this.Close();
                DialogResult = DialogResult.Cancel;
            }
        }
        bool CheckPlane(List<Vector64>traces, out double x1, out double y1, out double z1,
                                              out double x2, out double y2, out double z2)
        {
            Vector64 p;
            x1 = x2 = 0;
            y1 = y2 = 0;
            z1 = z2 = 0;
            for (int i = 0; i < traces.Count; i++)
            {
                p = traces[i];
                if (i == 0)
                {
                    x1 = x2 = p.X;
                    y1 = y2 = p.Y;
                    z1 = z2 = p.Z;
                }
                else
                {
                    if (p.X < x1) x1 = p.X;
                    if (p.Y < y1) y1 = p.Y;
                    if (p.Z < z1) z1 = p.Z;
                    if (p.X > x2) x2 = p.X;
                    if (p.Y > y2) y2 = p.Y;
                    if (p.Z > z2) z2 = p.Z;
                }
            }
            bool XZPlane = true;
            if (y2 - y1 > x2 - x1) XZPlane = false;
            return XZPlane;
        }
        void DrawTraces(Graphics g, Rectangle rect)
        {
            if (borehole == null || borehole.boreholeAngles.Count < 2) return;           
            List<Vector64> traces = borehole.boreholeAngles.CreateTracesLine(new Vector64(), true);
            
            if (traces.Count < 2) return;
            double x1, y1, z1, x2, y2, z2;
            bool IsXZPlane = CheckPlane(traces, out x1, out y1, out z1, out x2, out y2, out z2);
            Vector64 p;
            double x = 0, y = 0;
            PointF[] points = new PointF[traces.Count];           
            
            int width = rect.Width;
            int height = rect.Height;

            for( int i = 0; i < traces.Count; i++ )
            {
                p = traces[i];
                if (IsXZPlane) x = rect.Left + width * (p.X - x1) / (x2 - x1);
                else x = rect.Left + width * (p.Y - y1) / (y2 - y1);
                y = rect.Bottom - height * (p.Z - z1) / (z2 - z1);
                points[i] = new PointF((float)x, (float)y);
            }

            g.DrawLines(Pens.BlueViolet, points);

            int size = 4;
            foreach (PointF v in points)
                g.DrawEllipse(Pens.Brown, v.X - size, v.Y - size, size * 2, size * 2);

            if (listView1.SelectedIndices.Count > 0)
            { 
                int sel = listView1.SelectedIndices[0];
                if (sel >= 0 && sel < points.Length - 1)
                {
                    PointF p1 = points[sel];
                    PointF p2 = points[sel + 1];
                    g.DrawLine(Pens.Red, p1, p2);
                }
            }
            
            points = null;
        }
        void DrawCursor(Graphics g)
        {
            if ( cursor.X < 0 || cursor.Y < 0 ) return;
            if ( minDepth >= maxDepth ) return;

            PointF p1, p2;
            p1 = new PointF(ruler.windowRect.Left, cursor.Y);
            p2 = new PointF(ruler.windowRect.Right, cursor.Y);
            g.DrawLine(Pens.Gray, p1, p2);
            double val = minDepth + (maxDepth - minDepth) * (cursor.Y - ruler.drawRect.Top) / ruler.drawRect.Height;
            val = Math.Round(val, 2);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;

            SizeF textsize = g.MeasureString(val.ToString(), ruler.leftRuler.scaleFont);

            double x = ruler.drawRect.Left - ruler.leftRuler.longTick;
            double y = cursor.Y - textsize.Height;

            g.DrawString(val.ToString(), ruler.leftRuler.scaleFont, Brushes.Black, (float)x, (float)y, format);
        }
        void UpdateDepth()
        {
            minDepth = 0;
            maxDepth = 0;
            if (borehole == null) return;
            if (borehole.boreholeAngles != null)
            {
                List<Vector64> traces = borehole.boreholeAngles.CreateTracesLine(new Vector64(0,0,0), true);

                for (int i = 0; i < traces.Count; i++)
                {
                    if (i == 0) minDepth = maxDepth = traces[i].Z;
                    else
                    {
                        if (traces[i].Z < minDepth) minDepth = traces[i].Z;
                        if (traces[i].Z > maxDepth) maxDepth = traces[i].Z;
                    }
                }
                
                double len = maxDepth - minDepth;
                minDepth = 0;
                maxDepth = len;

                traces.Clear();
            }
            
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0,0,pictureBox1.Width,pictureBox1.Height);
            ruler.SetDrawRect(rect);
            ruler.SetMarine(50, 10, 10, 10);
            ruler.leftRuler.Direction = AxisDirectionEnum.UpDown;
            ruler.leftRuler.SetValuesRange(0, maxDepth);
            
            ruler.Draw(e.Graphics);
            DrawTraces(e.Graphics, ruler.drawRect);

            ruler.IsDrawYCursor = true;
            ruler.IsDrawXCursor = false;
            ruler.DrawCursor(e.Graphics);
            //DrawCursor(e.Graphics);
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            ruler.cursorPosition = e.Location;
            pictureBox1.Invalidate();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int sel = listView1.SelectedIndices[0];
                if( sel >=0 && sel < borehole.boreholeAngles.Count )
                {
                    borehole.boreholeAngles.Angles.RemoveAt(sel);
                    listView1.Items.RemoveAt(sel);
                    listView1.SelectedIndices.Clear();
                    pictureBox1.Invalidate();
                    Modified = true;
                }                
            }
        }
    }
}
