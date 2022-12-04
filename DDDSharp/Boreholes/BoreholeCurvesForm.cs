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
    public partial class BoreholeCurvesForm : Form
    {
        public CBorehole borehole = null;
        public bool lasDataReplaced = false;
        float nullValue = -999.25f;
        int idepth = -1;
        double minDepth = 0, maxDepth = 100;
        Dictionary<int, string> Columns = new Dictionary<int, string>();
        List<BoreholeCurve> Curves = new List<BoreholeCurve>();
        List<BoreholeCurve> CurvesSelected = new List<BoreholeCurve>();
        PointF cursor = new PointF(-1, -1);
        RectRuler ruler = new RectRuler();
        public BoreholeCurvesForm( CBorehole bh )
        {
            InitializeComponent();
            borehole = bh.Copy();
        }
        int GetKeyByName(string name, Dictionary<int, string> dicts)
        {
            foreach(var item in dicts)
            {
                if (item.Value == name) return item.Key;
            }
            return -1;
        }
        string GetNameByKey(int key, Dictionary<int, string> dicts)
        {
            string name;
            dicts.TryGetValue(key, out name);
            return name;
        }

        BoreholeCurve GetCurveByName(string name,List<BoreholeCurve>curves)
        {
            foreach (BoreholeCurve cv in curves)
            {
                if ( cv.Name == name )
                {
                    return cv;
                }
            }
            return null;
        }
      
        //创建所有的初始列，id,Name
        void InitColumns(LasFileData lasData)
        {
            Columns.Clear();
            for (int i = 0; i < lasData.CurveInformation.Count;i++)
            {
                Columns.Add(i, lasData.CurveInformation[i].Mnemonic);
            }
        }

        void InitCurves( LasFileData lasData )
        {
            if (lasData == null) return;

            //已创建曲线列表
            CurvesSelected.Clear();
            foreach (BoreholeCurve cv in borehole.Curves.Curves)
                CurvesSelected.Add(cv);

            //未创建曲线列表
            Curves.Clear();
            for ( int i = 0; i < lasData.CurveInformation.Count; i++ )
            {
                if ( i == idepth ) continue;
                string name = lasData.CurveInformation[i].Mnemonic;
                BoreholeCurve cv = GetCurveByName(name, CurvesSelected);
                if( cv == null )                
                {
                    cv = new BoreholeCurve(name);                    
                    float.TryParse(InvalidValueTextBox.Text, out nullValue);
                    cv.CreateFromLAS(lasData, idepth, i, nullValue);
                    
                    cv.Radius = borehole.Radius;
                    cv.UpdateRange();
                    Curves.Add(cv);
                }
            }
        }

        void UpdateColumnSelections()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            LasFileData lasData = borehole.Curves.lasData;
            if (lasData == null) return;            

            int id1 = 0, id2 = 0;
            foreach( var item in Columns )
            {
                if ( GetCurveByName( item.Value, CurvesSelected ) == null )
                {
                    comboBox1.Items.Add(item.Value); id1++;
                    if (idepth == item.Key) comboBox1.SelectedIndex = id1 - 1;
                    else
                    {       
                        comboBox2.Items.Add(item.Value);
                        id2++;
                    }
                }
            }
            comboBox2.SelectedIndex = -1;            
        }
        void UpdateSelectedList()
        {
            listBox1.Items.Clear();
            
            foreach (var item in CurvesSelected)
            {
                listBox1.Items.Add(item.Name);
            }
            listBox1.SelectedIndex = -1;
        }
        private void BoreholeCurvesForm_Load(object sender, EventArgs e)
        {
            LasFileData lasData = borehole.Curves.lasData;
            if (lasData == null) return;
            
            propertyGrid1.PropertySort = PropertySort.Categorized;

            UpdateDepth();
            InitColumns(lasData);
            
            nullValue = (float)borehole.Curves.lasData.nullValue;
            InvalidValueTextBox.Text = nullValue.ToString();

            idepth = lasData.depthIndex;
            InitCurves(lasData);

            UpdateColumnSelections();

            UpdateSelectedList();
        }
        
        int getIndicesByName(string name)
        {
            LasFileData lasData = borehole.Curves.lasData;
            if (lasData == null) return -1;
            for (int i = 0; i < lasData.CurveInformation.Count; i++)
            {                
                if ( name.Trim().ToLower() == 
                     lasData.CurveInformation[i].Mnemonic.Trim().ToLower() )
                {
                    return i;
                }
            }
            return -1;
        }
        void UpdateDepth()
        {
            minDepth = 0;
            maxDepth = 0;
            if (borehole == null) return;
            if (borehole.Curves.lasData != null)
            {
                List<float> depths = borehole.Curves.lasData.GetDepthData();
                for (int i = 0; i < depths.Count; i++)
                {
                    if (i == 0) minDepth = maxDepth = depths[i];
                    else
                    {
                        if (depths[i] < minDepth) minDepth = depths[i];
                        if (depths[i] > maxDepth) maxDepth = depths[i];
                    }
                }
            }
            minDepth = 0;
        }
        void DrawCurve(Graphics g, BoreholeCurve cv)
        {
            Rectangle rect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);            
            g.FillRectangle(Brushes.White, rect);            
            ruler.SetDrawRect(rect);
            ruler.SetMarine(50, 10, 10, 10);
            ruler.leftRuler.Direction = AxisDirectionEnum.UpDown;
            ruler.leftRuler.SetValuesRange(0,maxDepth);
            ruler.Draw(g);
            if( cv != null )cv.Draw(g, ruler.drawRect, 0, cv.maxDepth );
            ruler.DrawCursor(g);
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) propertyGrid1.SelectedObject = null;
            else 
            {
                string name = listBox1.SelectedItem.ToString();
                BoreholeCurve cv = GetCurveByName(name, CurvesSelected);
                propertyGrid1.SelectedObject = cv;
                pictureBox1.Invalidate();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (borehole == null) return;
            LasFileData lasData = borehole.Curves.lasData;
            if (lasData != null) 
            {
                //确定深度列
                idepth = lasData.depthIndex;
                if ( comboBox1.SelectedIndex >= 0 )
                {
                    string name = comboBox1.SelectedItem.ToString();
                    idepth = getIndicesByName(name);
                }                
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex < 0) return;
            string name = comboBox2.SelectedItem.ToString();
            BoreholeCurve cv = GetCurveByName(name,Curves);
            if (cv == null) return;

            minimumTextBox.Text = cv.minValue.ToString();
            maximumTextBox.Text = cv.maxValue.ToString();
            InvalidValueTextBox.Text = cv.nullValue.ToString();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex < 0) return;
            string name = comboBox2.SelectedItem.ToString();
            BoreholeCurve cv = GetCurveByName(name, Curves);
            if (cv == null) return;
            CurvesSelected.Add(cv);
            Curves.Remove(cv);            
            
            UpdateColumnSelections();
            UpdateSelectedList();

            comboBox2.SelectedIndex = -1;
            comboBox2.SelectedItem = null;
            comboBox2.Text = "";

            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if ( listBox1.SelectedIndex < 0 ) return;
            string name = listBox1.SelectedItem.ToString();
            BoreholeCurve cv = GetCurveByName(name, CurvesSelected);
            if (cv == null) return;
            Curves.Add(cv);
            CurvesSelected.Remove(cv);

            UpdateColumnSelections();
            UpdateSelectedList();

            listBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = comboBox2.Items.Count - 1;            
        }

        private void OK_Click(object sender, EventArgs e)
        {
            borehole.Curves.Clear();
            borehole.Curves.Curves.AddRange(CurvesSelected);

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CANCEL_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
        void DrawCursor(Graphics g)
        {
            if (cursor.X < 0 || cursor.Y < 0) return;
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
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel < 0) return;
            string name = listBox1.SelectedItem.ToString();
            BoreholeCurve cv = GetCurveByName(name, CurvesSelected);
            DrawCurve(e.Graphics, cv);
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pictureBox1.Invalidate();
        }
        bool LoadLasData(string filename)
        {
            CLasFile las = new CLasFile();
            LasFileData data = las.Read(filename);
            if (data.LogData == null) return false;
            data.m_pos = borehole.Position;
            
            borehole.Curves.lasData = data;
            lasDataReplaced = true;
            return true;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            cursor = new PointF(e.X, e.Y);
            ruler.cursorPosition = e.Location;
            pictureBox1.Invalidate(false);
        }

        private void LoadFrom_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.WellingCurveDataFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if( LoadLasData(dlg.FileName) )
                    {
                        LasFileData lasData = borehole.Curves.lasData;
                        if (lasData != null)
                        {
                            InitColumns(lasData);
                            UpdateDepth();
                            nullValue = (float)borehole.Curves.lasData.nullValue;
                            InvalidValueTextBox.Text = nullValue.ToString();
                            idepth = lasData.depthIndex;
                            InitCurves(lasData);
                            UpdateColumnSelections();
                            UpdateSelectedList();
                        }
                    }
                }
            }
        }
    }
}
