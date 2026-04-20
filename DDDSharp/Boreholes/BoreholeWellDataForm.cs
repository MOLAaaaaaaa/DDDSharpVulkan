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
    public partial class BoreholeWellDataForm : Form
    {
        public CBorehole borehole = null;
        bool Modified = false;
        double minDepth, maxDepth;
        PointF cursor = new PointF(-1,-1);
        RectRuler ruler = new RectRuler();
        bool IsControlKeyDown = false;
        int layerSelectedIndex = -1;
        int curveSelectedIndex = -1;
        
        List<BoreholeCurve> Curves = new List<BoreholeCurve>();

        public BoreholeWellDataForm(CBorehole _borehole = null)
        {
            InitializeComponent();
            if (_borehole == null) borehole = new CBorehole();
            else borehole = _borehole.Copy();
            CreateCurves();
            KeyPreview = true;
        }
        void CreateCurves()
        {
            Curves.Clear();
            if (borehole == null) return;
            LasFileData las = borehole.Curves.lasData;
            if (las == null) return;

            for(int i=0;i<las.CurveInformation.Count;i++)
            {
                if (i == las.depthIndex) continue;
                BoreholeCurve cv = new BoreholeCurve(las.CurveInformation[i].Mnemonic);
                cv.CreateFromLAS(las, las.depthIndex, i, (float)las.nullValue);
                cv.UpdateRange();
                Curves.Add(cv);
            }            
        }
        void UpdateDepth()
        {
            minDepth = 0;
            maxDepth = 0;
            if (borehole == null) return;
            if( borehole.Curves.lasData != null )
            {
               List<float>depths = borehole.Curves.lasData.GetDepthData();
               for(int i=0;i<depths.Count;i++)
                {
                    if (i == 0) minDepth = maxDepth = depths[i];
                    else
                    {
                        if (depths[i] < minDepth) minDepth = depths[i];
                        if (depths[i] > maxDepth) maxDepth = depths[i];
                    }
                }
            }
            else if( borehole.Stratums.Count > 0 )
            {
                minDepth = 0;
                StratumData layer = borehole.Stratums[0];
                maxDepth = layer.TopDepth;
                for ( int i=0; i < borehole.Stratums.Count; i++ )
                {
                    layer = borehole.Stratums[i];
                    maxDepth += layer.Thickness;
                }
            }
            else if( borehole.Baseline.Count > 0 )
            {
                Vector64 p;
                double h;
                for(int i = 0; i < borehole.Baseline.Count;i++)
                {
                    p = borehole.Baseline[i];
                    h = borehole.Position.Z - p.Z;
                    if (i == 0) minDepth = maxDepth = h;
                    else
                    {
                        if (h < minDepth) minDepth = h;
                        if (h > maxDepth) maxDepth = h;
                    }
                }
            }
        }
        void UpdateCurvesList()
        {
            listBox1.Items.Clear();
            if (borehole == null) return;

            foreach (var item in Curves)
            {
                listBox1.Items.Add(item.Name);
            }

            listBox1.SelectedIndex = -1;
        }
        void ClearLayerSelected()
        {
            layerSelectedIndex = -1;
            listView1.SelectedItems.Clear();
            listView1.SelectedIndices.Clear();            
        }
        void SetLayerSelected(int index)
        {
            ClearLayerSelected();
            if ( index >=0 )listView1.SelectedIndices.Add(index);
        }
        void ClearCurveSelected()
        {
            curveSelectedIndex = -1;
            listBox1.SelectedItems.Clear();
            listBox1.SelectedIndices.Clear();
            listBox1.SelectedIndex = -1;
        }
        void SetCurveSelected(int index)
        {
            ClearCurveSelected();
            curveSelectedIndex = index;
            listBox1.SelectedIndex = index;
        }
        void UpdateView()
        {
            listView1.Items.Clear();
            if (borehole.Stratums.Count < 1) return;

            this.listView1.BeginUpdate();
            StratumData layer;
            for (int i = 0; i < borehole.Stratums.Count; i++)
            {
                layer = borehole.Stratums[i];
                ListViewItem liv = listView1.Items.Add((i + 1).ToString());
                liv.UseItemStyleForSubItems = false;
                liv.SubItems.Add( layer.Name );
                liv.SubItems.Add( layer.Code );                
                liv.SubItems.Add( layer.TopDepth.ToString());
                liv.SubItems.Add(layer.Thickness.ToString());
                liv.SubItems[2].BackColor = layer.Color;
                liv.SubItems[2].ForeColor = Color.FromArgb(255,255-layer.Color.R, 255 - layer.Color.G, 255 - layer.Color.B);
            }
            this.listView1.EndUpdate();
        }

        void UpdateView(int irow, StratumData layer)
        {
            if (layer == null) return;
            if (irow < 0 || irow >= listView1.Items.Count) return;
            ListViewItem liv = listView1.Items[irow];

            this.listView1.BeginUpdate();

            liv.SubItems[1].Text = layer.Name;
            liv.SubItems[2].Text = layer.Code;
            liv.SubItems[2].BackColor = layer.Color;
            liv.SubItems[3].Text = layer.TopDepth.ToString();
            liv.SubItems[4].Text = layer.Thickness.ToString();
            
            this.listView1.EndUpdate();
        }
        void UpdateView(StratumDatas layers)
        {
            if (layers.Count < 1) return;            
            this.listView1.BeginUpdate();
            for (int i = 0; i < layers.Count; i++)
            {
                ListViewItem liv = listView1.Items[i];
                liv.SubItems[1].Text = layers[i].Name;
                liv.SubItems[2].Text = layers[i].Code;
                liv.SubItems[2].BackColor = layers[i].Color;
                liv.SubItems[3].Text = layers[i].TopDepth.ToString();
                liv.SubItems[4].Text = layers[i].Thickness.ToString();
            }
            this.listView1.EndUpdate();
        }
        private void BoreholeWellDataForm_Load(object sender, EventArgs e)
        {
            UpdateDepth();
            Text = borehole.Name + " Strata";
            listView1.Columns.Add("序号",30,HorizontalAlignment.Center);
            listView1.Columns.Add("地层名称", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("地层代号", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("层顶埋深", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("地层厚度", 100, HorizontalAlignment.Center);
            UpdateView();
            UpdateCurvesList();
            propertyGrid1.PropertySort = PropertySort.Categorized;
            propertyGrid2.PropertySort = PropertySort.Categorized;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            layerSelectedIndex = -1;
            propertyGrid1.SelectedObject = null;
            if ( listView1.SelectedIndices.Count > 0 )
            {
                layerSelectedIndex = listView1.SelectedIndices[0];
                if( layerSelectedIndex >= 0 )
                {                    
                    propertyGrid1.SelectedObject = borehole.Stratums[layerSelectedIndex];
                    pictureBox1.Invalidate();
                }
            }
        }

        // 末尾添加
        private void addStratumToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            StratumData layer = new StratumData();            
            Modified = true;            
            borehole.Stratums.AddLayer(layer);
            UpdateView();
            ClearLayerSelected();
            SetLayerSelected(borehole.Stratums.Count - 1);
        }
        // 当前位置插入
        private void insertStratumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = layerSelectedIndex;
            if (sel < 0) return;
            Modified = true;
            StratumData layer = new StratumData();            
            borehole.Stratums.InsertLayer(sel, layer);
            borehole.Stratums.UpdateTopDepth(sel + 1, layer.GetNextTopDepth());
            UpdateView();
            ClearLayerSelected();
            SetLayerSelected(sel);
        }
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = -1;
            if (listView1.SelectedIndices.Count > 0)
            {
                sel = listView1.SelectedIndices[0];
                if (sel >= 0 && sel < borehole.Stratums.Count)
                {
                    DialogResult ret = MessageBox.Show(AppLocalization.IsChinese ? "移除 " + borehole.Stratums[sel].Name : "Remove " + borehole.Stratums[sel].Name, AppLocalization.IsChinese ? "移除地层?" : "Remove Stratum?",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button2);

                    if ( ret == DialogResult.Yes )
                    {
                        borehole.Stratums.RemoveLayer(sel);
                        UpdateView();
                        listView1.SelectedIndices.Clear();
                        if (sel >= 0 && sel < borehole.Stratums.Count)
                            listView1.SelectedIndices.Add(sel);
                        Modified = true;
                    }
                    
                }
            }
        }
        void DoLayerPropertyChanged( int sel )
        {
            if (sel < 0) return;
            borehole.Stratums.UpdateTopDepth(sel + 1);
            UpdateDepth();
            Modified = true;
            //UpdateView(sel, borehole.Stratums[sel]);
            UpdateView(borehole.Stratums);
            propertyGrid1.SelectedObject = borehole.Stratums[sel];
            pictureBox1.Invalidate();
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (propertyGrid1.SelectedObject == null) return;
            if (layerSelectedIndex < 0) return;

            DoLayerPropertyChanged(layerSelectedIndex);
            //StratumData layer = propertyGrid1.SelectedObject as StratumData;
            //if ( layerSelectedIndex >= 0 )
            //{
            //    borehole.Stratums.Stratums[layerSelectedIndex] = layer;
            //    borehole.Stratums.UpdateTopDepth(layerSelectedIndex + 1);
            //    UpdateDepth();                
            //}
            //Modified = true;
            //UpdateView();
            //if (layer.IsValid()) pictureBox1.Invalidate();
        }

        private void BoreholeWellDataForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( Modified )
            {
                DialogResult ret = MessageBox.Show(AppLocalization.IsChinese ? "数据已经修改，仍要放弃吗？" : "Data has been modified, abort it anyway?", AppLocalization.IsChinese ? "忽略更改?" : "changes ignored?",
                                                     MessageBoxButtons.YesNoCancel,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button3);
                if (ret == DialogResult.Cancel || ret == DialogResult.No) { e.Cancel = true; return;  }
                if (ret == DialogResult.Yes) { DialogResult = DialogResult.Cancel;}
            }
            
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            if ( Modified )
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Modified = false;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if ( Modified )
            {
                DialogResult ret = MessageBox.Show(AppLocalization.IsChinese ? "数据已经修改，仍要放弃吗？" : "Data has been modified, abort it anyway?", AppLocalization.IsChinese ? "忽略更改?" : "changes ignored?",
                                                     MessageBoxButtons.YesNoCancel,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button3);
                if (ret == DialogResult.Cancel || ret == DialogResult.No) { return; }
            }
            Modified = false;
            DialogResult = DialogResult.Cancel; 
            this.Close();
        }

        private void addCurveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        public void DrawCurve(BoreholeCurve cv,Graphics g, Rectangle rect)
        {            
            if (cv.Points.Count < 2) return;
            List<PointF> pp = new List<PointF>();            
            double x, y;
            double minh = ruler.leftRuler.Minimum;
            double maxh = ruler.leftRuler.Maximum;
            float last = -1;
            for (int i = 0; i < cv.Points.Count; i++)
            {
                PointF p = cv.Points[i];                
                y = rect.Top + (p.X - minh) / (maxh - minh) * rect.Height;
                if (p.Y == cv.nullValue) x = (rect.Left + rect.Right) / 2;
                else x = rect.Left + (p.Y - cv.minValue) / (cv.maxValue - cv.minValue)*rect.Width;
                if ( (float)x != last || i==0 || i== cv.Points.Count-1)
                { 
                    pp.Add(new PointF((float)x, (float)y)); 
                }
                last = (float)x;
            }

            if (cv.EnableColorScale)
            {
                PointF p1, p2;
                for (int i = 0; i < pp.Count - 1; i++)
                {
                    p1 = pp[i];
                    p2 = pp[i + 1];
                    x = cv.minValue + (p1.X - rect.Left) / rect.Width * (cv.maxValue - cv.minValue);
                    Pen pen = new Pen(cv.ColorScale.GetColor(x), cv.LineWidth);
                    g.DrawLine(pen, p1, p2);
                    pen.Dispose();
                }
            }
            else
            {
                Pen pen = new Pen(cv.Color, cv.LineWidth);
                g.DrawLines(pen, pp.ToArray());
                pen.Dispose();
            }
            pp = null;
        }
        void DrawCurve(Graphics g, Rectangle rect)
        {
            BoreholeCurve cv = GetSelectedCurve(curveSelectedIndex);
            if (cv != null)//cv.Draw(g, rect, 0, cv.maxDepth);
                DrawCurve(cv,g,rect);
        }
        void DrawStrata(Graphics g, Rectangle rect)
        {
            if (borehole == null) return;
            StratumData layer;
            double h1, h2,y1,y2;

            Rectangle selRect = new Rectangle(0,0,0,0);

            for (int i = 0; i < borehole.Stratums.Count; i++ )
            {
                layer = borehole.Stratums[i];
                if (!layer.IsValid()) continue;
                
                double minh = ruler.leftRuler.Minimum;
                double maxh = ruler.leftRuler.Maximum;

                h1 = layer.TopDepth;
                h2 = layer.TopDepth + layer.Thickness;
                y1 = rect.Top + rect.Height * (h1 - minh) / (maxh - minh);
                y2 = rect.Top + rect.Height * (h2 - minh) / (maxh - minh);
                RectangleF rect1 = new RectangleF(rect.Left+1, (float)y1,rect.Width-2,(float)(y2-y1) );
                g.FillRectangle(new SolidBrush(layer.Color), rect1);                
                
                if( i == layerSelectedIndex)//selected
                {
                    selRect = new Rectangle(rect.Left, (int)y1, rect.Width, (int)(y2 - y1));                    
                }
            }
            if(selRect.Width > 0 ) g.DrawRectangle(Pens.Red, selRect);
        }
        double GetCursorDepth()
        {
            if (cursor.X < 0 || cursor.Y < 0) return -1;
            double h1 = ruler.leftRuler.Minimum;
            double h2 = ruler.leftRuler.Maximum;
            double val = h1 + (h2 - h1) * (cursor.Y - ruler.drawRect.Top) / ruler.drawRect.Height;
            return val;
        }
        void DrawCursor(Graphics g)
        {
            if (cursor.X < 0 || cursor.Y < 0) return;
            PointF p1, p2;
            p1 = new PointF(ruler.windowRect.Left,cursor.Y);
            p2 = new PointF(ruler.windowRect.Right, cursor.Y);
            g.DrawLine(Pens.Gray, p1, p2);
            double h1 = ruler.leftRuler.Minimum;
            double h2 = ruler.leftRuler.Maximum;
            double val = h1 + (h2 - h1) * (cursor.Y - ruler.drawRect.Top) / ruler.drawRect.Height;
            val = Math.Round(val, 2);
            
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Far;
            
            SizeF textsize = g.MeasureString(val.ToString(), ruler.leftRuler.scaleFont);

            double x = ruler.drawRect.Left - ruler.leftRuler.longTick;
            double y = cursor.Y - textsize.Height;

            g.DrawString(val.ToString(),ruler.leftRuler.scaleFont,Brushes.Black,(float)x, (float)y,format);
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
            g.FillRectangle(Brushes.White, rect);
            ruler.SetDrawRect(rect);
            ruler.SetMarine(50, 10, 10, 10);            
            ruler.leftRuler.SetValuesRange(0,maxDepth);
            ruler.leftRuler.Direction = AxisDirectionEnum.UpDown;
            ruler.Draw(g);
            DrawStrata(g, ruler.drawRect);
            DrawCurve(g, ruler.drawRect);

            ruler.IsDrawYCursor = true;
            ruler.IsDrawXCursor = false;
            ruler.DrawCursor(g);

            //DrawCursor(g);
        }
        BoreholeCurve GetSelectedCurve(int sel)
        {
            if (sel < 0 || sel >= Curves.Count) return null;
            return Curves[sel];            
        }
        StratumData GetSelectedLayer(int sel)
        {
            if (sel < 0 || sel >= borehole.Stratums.Count) return null;
            return borehole.Stratums[sel]; 
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            curveSelectedIndex = listBox1.SelectedIndex;
            if (curveSelectedIndex < 0) propertyGrid2.SelectedObject = null;
            else
            {
                BoreholeCurve cv = GetSelectedCurve(curveSelectedIndex);
                propertyGrid2.SelectedObject = cv;
                pictureBox1.Invalidate();
            }
        }

        private void propertyGrid2_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            cursor = new PointF(e.X, e.Y);
            ruler.cursorPosition = e.Location;

            pictureBox1.Invalidate(false);
        }
        
        void SetAsLayerTop()
        {
            if (layerSelectedIndex < 0) return;
            StratumData layer = GetSelectedLayer(layerSelectedIndex);
            layer.TopDepth = GetCursorDepth();
            //UpdateView(layerSelectedIndex, layer);            
            DoLayerPropertyChanged(layerSelectedIndex);
            propertyGrid1.Invalidate();
            pictureBox1.Invalidate();
        }
        void SetAsLayerBottom()
        {
            if (layerSelectedIndex < 0) return;
            StratumData layer = GetSelectedLayer(layerSelectedIndex);
            layer.Thickness = GetCursorDepth() - layer.TopDepth;
            DoLayerPropertyChanged(layerSelectedIndex);
            propertyGrid1.Invalidate();
            //UpdateView(layerSelectedIndex, layer);
            //propertyGrid1.Invalidate();
            pictureBox1.Invalidate();
        }
        private void BoreholeWellDataForm_KeyDown(object sender, KeyEventArgs e)
        {
            IsControlKeyDown = e.Control;
            if (e.Control && e.KeyCode == Keys.T && layerSelectedIndex >=0 )
            {
                SetAsLayerTop();
            }
            else if (e.Control && e.KeyCode == Keys.B && layerSelectedIndex >= 0)
            {
                SetAsLayerBottom();
            }
        }
        private void BoreholeWellDataForm_KeyUp(object sender, KeyEventArgs e)
        {
            IsControlKeyDown = e.Control;
        }
        int SelectedAtPoint(int x,int y)
        {
            if (borehole == null) return -1;
            StratumData layer;
            double h1, h2, y1, y2;

            Rectangle rect = ruler.drawRect;
            Rectangle selRect = new Rectangle(0, 0, 0, 0);
            double minh = ruler.leftRuler.Minimum;
            double maxh = ruler.leftRuler.Maximum;
            for (int i = 0; i < borehole.Stratums.Count; i++)
            {
                layer = borehole.Stratums[i];
                if ( !layer.IsValid() ) continue;

                h1 = layer.TopDepth;
                h2 = layer.TopDepth + layer.Thickness;
                y1 = rect.Top + rect.Height * (h1 - minh) / (maxh - minh);
                y2 = rect.Top + rect.Height * (h2 - minh) / (maxh - minh);
                RectangleF rect1 = new RectangleF(rect.Left, (float)y1, rect.Width - 2, (float)(y2 - y1));
                if (rect1.Contains(x, y)) return i;
            }
            return -1;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetLayerSelected(SelectedAtPoint(e.X, e.Y));                
            }
            else if (e.Button == MouseButtons.Right)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                pictureBox1.ContextMenuStrip = contextMenuStripPicture;
            }
        }

        private void setAsTopCtlTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAsLayerTop();
        }

        private void setAsBottomCtlBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAsLayerBottom();
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if(layerSelectedIndex >=0 && e.Button == MouseButtons.Right)
            {
                listView1.ContextMenuStrip = contextMenuStripListView;
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
