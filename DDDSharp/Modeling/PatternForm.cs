using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
namespace PatternFilling
{
    public partial class PatternForm : Form
    {        
        public FillPatternClass fillPattern = null;        
        SolidFillPattern solidFill = new SolidFillPattern();
        HatchFillPattern hatchFill = new HatchFillPattern();
        GeoFillPattern geoPatternFill = new GeoFillPattern();
        TextureFillPattern textureFill = new TextureFillPattern();
        LinearGradientFillPattern linearFill = new LinearGradientFillPattern();

        public PatternForm()
        {
            InitializeComponent();
        }
        public void SetValue(FillPatternClass _fillPattern)
        {
            fillPattern = _fillPattern;
            if (fillPattern.FillMethod == FillMethodEnum.Solid)
            {
                solidFill = (SolidFillPattern)fillPattern.FillPattern;
                solidFill.fillForeColor = _fillPattern.fillColor;
            }
            else if (fillPattern.FillMethod == FillMethodEnum.Hatch)
            {
                hatchFill = (HatchFillPattern)fillPattern.FillPattern;
                hatchFill.fillForeColor = _fillPattern.fillColor;
            }
            else if (fillPattern.FillMethod == FillMethodEnum.GeoPattern)
                geoPatternFill = (GeoFillPattern)fillPattern.FillPattern;
            else if (fillPattern.FillMethod == FillMethodEnum.Texture)
                textureFill = (TextureFillPattern)fillPattern.FillPattern;
            else if (fillPattern.FillMethod == FillMethodEnum.LinearGradient)
                linearFill = (LinearGradientFillPattern)fillPattern.FillPattern;
        }
        public void GetValue()
        {
            FillMethodEnum method = (FillMethodEnum)comboBox1.SelectedIndex;
            fillPattern.FillMethod = method;
            if (method == FillMethodEnum.Solid)
            {
                fillPattern.FillPattern = solidFill;
                fillPattern.fillColor = solidFill.fillForeColor;
            }
            else if (method == FillMethodEnum.Hatch)
            { 
                fillPattern.FillPattern = hatchFill;
                fillPattern.fillColor = solidFill.fillForeColor;
            }
            else if (method == FillMethodEnum.GeoPattern)
                fillPattern.FillPattern = geoPatternFill;
            else if (method == FillMethodEnum.Texture)
                fillPattern.FillPattern = textureFill;
            else if (method == FillMethodEnum.LinearGradient)
                fillPattern.FillPattern = linearFill;
            else if (method == FillMethodEnum.None)
                fillPattern.FillPattern = null;
        }
        private void PatternForm_Load(object sender, EventArgs e)
        {
            string[] names = Enum.GetNames(typeof(FillMethodEnum));
            comboBox1.Items.AddRange(names);
            int sel = (int)fillPattern.FillMethod;
            comboBox1.SelectedIndex = sel;            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = comboBox1.SelectedIndex;
            if (sel >= 0 && sel < tabControl1.TabPages.Count)
            {
                tabControl1.SelectedTab = tabControl1.TabPages[sel];
                if( sel == 0) propertyGrid1.SelectedObject = solidFill;
                if (sel == 1) propertyGrid2.SelectedObject = hatchFill;
                if (sel == 2) propertyGrid3.SelectedObject = geoPatternFill;
                if (sel == 3) propertyGrid4.SelectedObject = textureFill;
                if (sel == 4) propertyGrid5.SelectedObject = linearFill;
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            GetValue();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = tabControl1.SelectedIndex;
            comboBox1.SelectedIndex = sel;
        }
        /// <summary>
        /// 透明度调整
        /// </summary>
        /// <param name="image"></param>
        /// <param name="opacity">  0.1  -- 1 </param>
        /// <returns></returns>
        public Image ToTransparent(Image image, float opacity)
        {
            if (opacity >= 1 || opacity < 0) return image;//透明度应在0.1 - 1之间
            Bitmap bitmap = new Bitmap(image.Width, image.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                var matrix = new ColorMatrix { Matrix33 = opacity };
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                g.DrawImage(image, rectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return bitmap;
        }
        void PolygonSolidFill(Graphics g,Rectangle rect)
        {
            Color color = Color.FromArgb( 
                (int)(255 - solidFill.fillTransparent * 255),
                solidFill.fillForeColor.R, 
                solidFill.fillForeColor.G, 
                solidFill.fillForeColor.B );
            Brush brush = new SolidBrush(color);
            g.FillRectangle(brush, rect);
        }
        void PolygonHatchFill(Graphics g, Rectangle rect)
        {
            Color color1 = Color.FromArgb((int)(255 - hatchFill.fillTransparent * 255),
                hatchFill.fillForeColor.R, hatchFill.fillForeColor.G, hatchFill.fillForeColor.B);
            Color color2 = Color.FromArgb((int)(255 - hatchFill.fillTransparent * 255),
                hatchFill.fillBackColor.R, hatchFill.fillBackColor.G, hatchFill.fillBackColor.B);
            HatchBrush brush = new HatchBrush(hatchFill.hatchStyle, color1, color2);
            g.FillRectangle(brush, rect);
        }

        void PolygonTextureFill(Graphics g, Rectangle rect)
        {
            if (textureFill.IsValid())
            {
                TextureBrush brush = new TextureBrush(ToTransparent(textureFill.bitmap, 1 - textureFill.fillTransparent));
                g.FillRectangle(brush, rect);
            }
        }
        void PolygonLinearFill(Graphics g, Rectangle rect)
        {
            LinearGradientFillPattern fill = linearFill;
            if (fill.IsValid() )
            {
                Color color1 = Color.FromArgb((int)(255 - fill.fillTransparent * 255),
                fill.StartColor.R, fill.StartColor.G, fill.StartColor.B);
                Color color2 = Color.FromArgb((int)(255 - fill.fillTransparent * 255),
                    fill.EndColor.R, fill.EndColor.G, fill.EndColor.B);

                LinearGradientBrush brush;
                if (linearFill.FillAsAngle )
                {
                    brush = new LinearGradientBrush(rect, color1,
                        color2, fill.Angle, fill.IsAngleScaleable);
                }
                else
                {
                    brush = new LinearGradientBrush(rect, color1, color2,fill.Mode);
                }
                g.FillRectangle(brush, rect);
            }
        }
        
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0,0,pictureBox1.Width,pictureBox1.Height);
            PolygonSolidFill(e.Graphics, rect);
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, pictureBox2.Width, pictureBox2.Height);
            PolygonHatchFill(e.Graphics, rect);
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void pictureBox4_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, pictureBox4.Width, pictureBox4.Height);
            PolygonTextureFill(e.Graphics, rect);
        }

        private void pictureBox5_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, pictureBox5.Width, pictureBox5.Height);
            PolygonLinearFill(e.Graphics, rect);
        }       

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void propertyGrid2_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pictureBox2.Invalidate();
        }

        private void propertyGrid3_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pictureBox3.Invalidate();
        }

        private void propertyGrid4_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pictureBox4.Invalidate();
        }

        private void propertyGrid5_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            pictureBox5.Invalidate();
        }
    }
}
