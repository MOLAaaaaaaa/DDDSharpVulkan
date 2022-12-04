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
namespace DDDSharp
{
    public partial class MeshFromGridForm : Form
    {
        Vector64 TopLeft = new Vector64();
        Vector64 TopRight = new Vector64();
        Vector64 BottomLeft = new Vector64();
        Vector64 BottomRight = new Vector64();
        double zscale = 1.0;

        public CSurferGrid grid2d = null;
        public CMesh mesh = null;
        planEnum plan = planEnum.XOY;
        
        public Bitmap Bmp = null;
        public string textureFile = "";
        public bool IsGridData = false;        
        public MeshFromGridForm()
        {
            InitializeComponent();
            checkBox1.Checked = false;
        }
        /// <summary>
        /// 从网格数据中创建Mesh
        /// </summary>
        /// <param name="_grid2d"></param>
        public void SetData(CSurferGrid _grid2d)
        {
            grid2d = _grid2d;
            SetCorners(_grid2d);
            IsGridData = true;
        }
        /// <summary>
        /// 从位图中创建Mesh
        /// </summary>
        /// <param name="_bmp"></param>
        /// <param name="path"></param>
        public void SetData(Bitmap _bmp,string path)
        {
            Bmp = _bmp;
            textureFile = path;
            IsGridData = false;
            SetCorners(_bmp);
        }
        void SetCorners(CSurferGrid _grd)
        {
            TopLeftTextBox1.Text = _grd.minx.ToString();
            TopRightTextBox1.Text = _grd.maxx.ToString();
            BottomLeftTextBox1.Text = _grd.miny.ToString();
            BottomLeftTextBox1.Text = _grd.maxy.ToString();
        }
        void SetCorners(Bitmap _bmp)
        {
            TopLeftTextBox1.Text = "0";
            TopRightTextBox1.Text = _bmp.Width.ToString();
            BottomLeftTextBox1.Text = "0";
            BottomLeftTextBox1.Text = _bmp.Height.ToString();
        }
        void UpdateGridInfo()
        {
            if (IsGridData && grid2d != null)
            {
                GridInfoBox.Text = "Row = " + grid2d.yGrid + ", Column = " + grid2d.xGrid + ";" + System.Environment.NewLine;
                GridInfoBox.Text += "X from " + grid2d.minx + " to " + grid2d.maxx + ";" + System.Environment.NewLine;
                GridInfoBox.Text += "Y from " + grid2d.miny + " to " + grid2d.maxy + ";" + System.Environment.NewLine;
                GridInfoBox.Text += "V from " + grid2d.minv + " to " + grid2d.maxv + ";" + System.Environment.NewLine;
                GridInfoBox.Text += "X Length = " + (grid2d.maxx - grid2d.minx) + ";" + System.Environment.NewLine;
                GridInfoBox.Text += "Y Length = " + (grid2d.maxy - grid2d.miny) + ";" + System.Environment.NewLine; ;
                GridInfoBox.Text += "V Length = " + (grid2d.maxv - grid2d.minv) + ";";

                // YTopLeft
                // |
                // |
                // O---------->X
                TopLeftTextBox1.Text = grid2d.minx.ToString();
                TopLeftTextBox2.Text = grid2d.maxy.ToString();
                TopLeftTextBox3.Text = grid2d.GetZValue(0, grid2d.yGrid-1).ToString();

                TopRightTextBox1.Text = grid2d.maxx.ToString();
                TopRightTextBox2.Text = grid2d.maxy.ToString();
                TopRightTextBox3.Text = grid2d.GetZValue(grid2d.xGrid-1, grid2d.yGrid - 1).ToString();

                BottomLeftTextBox1.Text = grid2d.minx.ToString();
                BottomLeftTextBox2.Text = grid2d.miny.ToString();
                BottomLeftTextBox3.Text = grid2d.GetZValue(0, 0).ToString();

                BottomRightTextBox1.Text = grid2d.maxx.ToString();
                BottomRightTextBox2.Text = grid2d.miny.ToString();
                BottomRightTextBox3.Text = grid2d.GetZValue(grid2d.xGrid - 1, 0).ToString();

            }
            if ( !IsGridData && Bmp != null)
            {
                GridInfoBox.Text = "Width = " + Bmp.Width + ", Height = " + Bmp.Height + ";" + System.Environment.NewLine;
                // YTopLeft
                // |
                // |
                // O---------->X
                //(0,1)
                TopLeftTextBox1.Text = "0";
                TopLeftTextBox2.Text = Bmp.Height.ToString();
                TopLeftTextBox3.Text = "0";
                //(1,1)
                TopRightTextBox1.Text = Bmp.Width.ToString();
                TopRightTextBox2.Text = Bmp.Height.ToString();
                TopRightTextBox3.Text = "0";
                //(0,0)
                BottomLeftTextBox1.Text = "0";
                BottomLeftTextBox2.Text = "0";
                BottomLeftTextBox3.Text = "0";
                //(1,0)
                BottomRightTextBox1.Text = Bmp.Width.ToString();
                BottomRightTextBox2.Text = "0";
                BottomRightTextBox3.Text = "0";
            }
        }
        bool CheckCorneres()
        {
            if (!double.TryParse(TopLeftTextBox1.Text, out TopLeft.X)) return false;
            if (!double.TryParse(TopLeftTextBox2.Text, out TopLeft.Y)) return false;
            if (!double.TryParse(TopLeftTextBox3.Text, out TopLeft.Z)) return false;

            if (!double.TryParse(TopRightTextBox1.Text, out TopRight.X)) return false;
            if (!double.TryParse(TopRightTextBox2.Text, out TopRight.Y)) return false;
            if (!double.TryParse(TopRightTextBox3.Text, out TopRight.Z)) return false;

            if (!double.TryParse(BottomLeftTextBox1.Text, out BottomLeft.X)) return false;
            if (!double.TryParse(BottomLeftTextBox2.Text, out BottomLeft.Y)) return false;
            if (!double.TryParse(BottomLeftTextBox3.Text, out BottomLeft.Z)) return false;

            if (!double.TryParse(BottomRightTextBox1.Text, out BottomRight.X)) return false;
            if (!double.TryParse(BottomRightTextBox2.Text, out BottomRight.Y)) return false;
            if (!double.TryParse(BottomRightTextBox3.Text, out BottomRight.Z)) return false;

            if (!double.TryParse(ZScaleTextBox1.Text, out zscale)) return false;
            
            return true;
        }        

        private void OKBUTTON_Click(object sender, EventArgs e)
        {
            if( !CheckCorneres() )
            {
                MessageBox.Show("Invalid input values.");
                return;
            }

            Cursor = Cursors.WaitCursor;
            
            mesh = new CMesh();
            
            if (IsGridData)
            {
                mesh.Name = grid2d.Name;
                mesh.fromGrid2D(grid2d, TopLeft, TopRight, BottomLeft, BottomRight, zscale, checkBox1.Checked);
                mesh.textureStruct.TextureFile = "";
                mesh.enbaleTexture = false;
            }
            else
            {
                mesh.Name = System.IO.Path.GetFileName(textureFile);
                mesh.fromImage(Bmp, TopLeft, TopRight, BottomLeft, BottomRight);
                mesh.textureStruct.TextureFile = textureFile;
                mesh.enbaleTexture = true;
            }
            C3DData.AddObject(mesh, true);

            DialogResult = DialogResult.OK;
            this.Close();

            Cursor = Cursors.Default;
        }

        private void CANCELBUTTON_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void MeshFromGridForm_Load(object sender, EventArgs e)
        {
            UpdateGridInfo();
            if ( IsGridData && grid2d != null )
            {
                double xx = (grid2d.maxx - grid2d.minx);
                double yy = (grid2d.maxy - grid2d.miny);
                double vv = (grid2d.maxv - grid2d.minv);
                if (vv > Math.Max(xx, yy)) zscale = Math.Min(xx, yy) / vv;
            }
            else
            {
                zscale = 1;
            }
            ZScaleTextBox1.Text = zscale.ToString();
        }
    }
}
