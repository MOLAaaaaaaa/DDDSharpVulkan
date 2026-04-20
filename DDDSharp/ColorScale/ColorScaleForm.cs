using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AviFile.Avi;

using DDDSharp;
namespace DataCollection
{
    public partial class ColorScaleForm : Form
    {
        public float[] data = null;
        public CColorScale colorscale = null;
        public CColorScale nativeColorscale;// = new CColorScale();
        public bool updated = false;
        double minv = 0, maxv = 0;
        
        int curSelected = -1;

        public ColorScaleForm()
        {
            InitializeComponent();
            KeyPreview = true;

        }

        private void ColorScaleForm_Load(object sender, EventArgs e)
        {
            if (colorscale != null)
            {
                nativeColorscale = colorscale.Copy();
                propertyGrid1.SelectedObject = nativeColorscale;
                minv = nativeColorscale.minv;
                maxv = nativeColorscale.maxv;
            }
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 100;            
        }

        private void OKbutton1_Click(object sender, EventArgs e)
        {
            colorscale = nativeColorscale;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancelbutton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoadFrombutton1_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "color scale (*.clr)|*.clr|color level (*.lvl)|*.lvl|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if( dlg.FilterIndex == 1 )
                    {
                        if (nativeColorscale.LoadClr(dlg.FileName))
                        {
                            propertyGrid1.SelectedObject = nativeColorscale;
                            updated = true;
                        }
                    }
                    else
                    {
                        if (nativeColorscale.LoadLvl(dlg.FileName))
                        {
                            propertyGrid1.SelectedObject = nativeColorscale;
                            updated = true;
                        }
                    }
                    
                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "color level (*.clr)|*.clr|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if(nativeColorscale.SaveClr(dlg.FileName) )                    
                        MessageBox.Show(AppLocalization.IsChinese ? "颜色映射已保存到文件：\n" + dlg.FileName : "Color map saved to file:\n" + dlg.FileName);
                    else
                        MessageBox.Show(AppLocalization.IsChinese ? "保存颜色映射到文件失败：\n" + dlg.FileName : "Failed to save color map to file:\n" + dlg.FileName);
                    this.Cursor = DefaultCursor;
                }
            }
        }        
        private void ValueDistributionButton_Click(object sender, EventArgs e)
        {
            if (colorscale == null ) return;
            if(data == null)
            {
                data = C3DData.objSelected.toValuesArray();
            }
            
            if (data == null) return;

            CValueDistribution vb = new CValueDistribution();
            vb.data = data;
            if ( vb.ShowDialog() == DialogResult.OK )
            {
                if (nativeColorscale.minv != vb.outMinv || nativeColorscale.maxv != vb.outMaxv)
                {
                    nativeColorscale.SetValueRange(vb.outMinv, vb.outMaxv);
                    propertyGrid1.SelectedObject = nativeColorscale;
                    propertyGrid1.Update();
                    updated = true;
                }
            }
        }

        public void DrawColorBarWithArrow(Graphics e, Rectangle rect,CColorScale colorscale)
        {
            if (colorscale.Levels.Count < 2) return;
            double v1 = colorscale.Levels[0].LevelValue;
            double v2 = colorscale.Levels[colorscale.Levels.Count - 1].LevelValue;
            double hs1 = 0.3, hs2 = 0.4, hs3 = 0.3; //比例箭头、颜色、刻度

            double h1 = 0;
            double h2 = h1 + rect.Height * hs1;
            double h3 = h2 + rect.Height * hs2;
            double h4 = rect.Bottom;
            double margin = 6;
            double width = rect.Width - margin * 2;

            double x1, x2, y1;
            Color color;
            ColorLevel c1, c2;

            for (int i = 0; i < colorscale.Count; i++)
            {
                c1 = colorscale.Levels[i];
                color = c1.Color;
                x1 = rect.Left + margin + (c1.LevelValue - v1) / (v2 - v1) * width;
                y1 = h2;
                if (i == colorscale.Count - 1)
                {
                    x2 = rect.Right;
                }
                else
                {
                    c2 = colorscale.Levels[i + 1];
                    x2 = rect.Left + margin + (c2.LevelValue - v1) / (v2 - v1) * width;
                }
                
                SolidBrush br = new SolidBrush(color);
                e.FillRectangle(br, (float)x1, (float)y1, (float)(x2 - x1), (float)(h3 - h2));

                if (curSelected == i)
                {
                    e.DrawRectangle(Pens.Black, (float)x1, (float)y1, (float)(x2 - x1), (float)(h3 - h2));                    
                }                
            }

            //draw arrow
            double arrowWidth = 12;
            double arrowHeight = 16;
            for (int i = 0; i < colorscale.Count; i++)
            {
                c1 = colorscale.Levels[i];
                color = c1.Color;
                x1 = rect.Left + margin + (c1.LevelValue - v1) / (v2 - v1) * width - arrowWidth / 2f;
                y1 = h2 - arrowHeight;
                RectangleF rect1 = new RectangleF((float)x1, (float)y1, (float)arrowWidth, (float)arrowHeight);
                if (curSelected == i)
                    GeometryDrawing.DrawArrow(e, rect1, Color.Red, Color.Black, DirectionEnum.down);
                else GeometryDrawing.DrawArrow(e, rect1, Color.Blue, Color.Black, DirectionEnum.down);
            }

        }

        private void ColorScaleForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            if (nativeColorscale != null)
            {
                Cursor = Cursors.WaitCursor;

                Graphics g = colorBar.CreateGraphics();
                Rectangle rect = new Rectangle(0, 0, colorBar.Width, colorBar.Height);
                //nativeColorscale.DrawColorBar(g,rect);
                DrawColorBarWithArrow(g, rect,nativeColorscale);
                Cursor = Cursors.Default;
            }
        }

        private void Inverse_Click(object sender, EventArgs e)
        {
            if (nativeColorscale != null)
            {
                nativeColorscale.Reverse();
                updated = true;
                //colorBar.Invalidate();//??不起作用？？？
                this.Invalidate();
            }            
        }

        private void DefaultButton_Click(object sender, EventArgs e)
        {
            nativeColorscale = new CColorScale(minv,maxv);
            updated = true;
            colorBar.Invalidate();
        }

        private void Edit_Click(object sender, EventArgs e)
        {

        }

        private void colorBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (colorscale.Levels.Count < 2) return;

            Rectangle rect = new Rectangle(0, 0, colorBar.Width, colorBar.Height);

            double v1 = colorscale.Levels[0].LevelValue;
            double v2 = colorscale.Levels[colorscale.Levels.Count - 1].LevelValue;
            double hs1 = 0.3, hs2 = 0.4, hs3 = 0.3; //比例箭头、颜色、刻度

            double h1 = 0;
            double h2 = h1 + rect.Height * hs1;
            double h3 = h2 + rect.Height * hs2;
            double h4 = rect.Bottom;
            double margin = 6;
            double width = rect.Width - margin * 2;

            double x1, x2, y1;
            Color color;
            ColorLevel c1, c2;

            int mousex = e.Location.X;
            int mousey = e.Location.Y;
            
            bool selectchange = false;

            for (int i = 0; i < colorscale.Count; i++)
            {
                c1 = colorscale.Levels[i];
                color = c1.Color;
                x1 = rect.Left + margin + (c1.LevelValue - v1) / (v2 - v1) * width;
                y1 = h2;
                if (i == colorscale.Count - 1)
                {
                    x2 = rect.Right;
                }
                else
                {
                    c2 = colorscale.Levels[i + 1];
                    x2 = rect.Left + margin + (c2.LevelValue - v1) / (v2 - v1) * width;
                }

                if (e.Location.X >= x1 && e.Location.X <= x2 &&
                    e.Location.Y >= h2 && e.Location.Y <= h3)
                {
                    if (curSelected != i)
                    {
                        curSelected = i;
                        selectchange = true;
                    }
                    break;
                }
            }
            if ( !selectchange )
            {
                //draw arrow
                double arrowWidth = 12;
                double arrowHeight = 16;
                for (int i = 0; i < colorscale.Count; i++)
                {
                    c1 = colorscale.Levels[i];
                    color = c1.Color;
                    x1 = rect.Left + margin + (c1.LevelValue - v1) / (v2 - v1) * width - arrowWidth / 2f;
                    y1 = h2 - arrowHeight;
                    
                    if (e.Location.X >= x1 && e.Location.X <= x1+ arrowWidth &&
                    e.Location.Y >= h1 && e.Location.Y <= h2)
                    {
                        if (curSelected != i)
                        {
                            curSelected = i;
                            selectchange = true;
                        }
                        break;
                    }

                }
            }

            if( selectchange )
            {
                ColorLevel level = nativeColorscale.Levels[curSelected];
                AlphaTextBox.Text = level.A.ToString();
                trackBar1.Value = (int)(level.A * 100);
                this.Invalidate();
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if( curSelected >=0 )
            {
                float alpha = trackBar1.Value/100f;
                AlphaTextBox.Text = alpha.ToString();
                ColorLevel level = nativeColorscale.Levels[curSelected];
                level.A = alpha;
                nativeColorscale.Levels[curSelected] = level;
                updated = true;
            }
        }

        private void ColorScaleForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (colorscale.Levels.Count < 2) return;
            if (curSelected < 0 || curSelected >= colorscale.Levels.Count) return;
                switch (e.KeyCode)
                {
                    case Keys.Delete:

                        nativeColorscale.Levels.RemoveAt(curSelected);
                        if (curSelected >= colorscale.Levels.Count) curSelected = colorscale.Levels.Count - 1;
                        updated = true;
                        this.Invalidate();
                        break;
                }
        }

        private void colorBar_DoubleClick(object sender, EventArgs e)
        {
            if (colorscale.Levels.Count < 2) return;
            if (curSelected < 0 || curSelected >= colorscale.Levels.Count) return;
            ColorLevel lel = nativeColorscale.Levels[curSelected];
            ColorDialog dlg = new ColorDialog();
            dlg.Color = lel.Color;
            if (dlg.ShowDialog() == DialogResult.OK) 
            {
                lel.Color = dlg.Color;
                nativeColorscale.Levels[curSelected] = lel;
                updated = true;
                Invalidate();
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (nativeColorscale != null)
            {
                updated = true;
                colorBar.Invalidate();
            }
        }
    }
}
