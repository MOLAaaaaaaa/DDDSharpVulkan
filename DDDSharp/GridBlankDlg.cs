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
    public partial class GridBlankDlg : Form
    {
        public C3DGridData grid3d = null;
        public C2DGridData grid2d = null;
        public double zoffset = 0.0;
        public bool exchangeXY = false;
        public bool keepUpper = false;
        public bool geoCoordinateSystem = false;
        public GridBlankDlg()
        {
            InitializeComponent();            
        }
        public void SetGrids(C2DGridData _grid2d, C3DGridData _grid3d)
        {
            grid2d = _grid2d;
            grid3d = _grid3d;
        }
        bool CheckIsMatched()
        {
            if (grid2d == null || grid3d == null) return false;

            DoubleRect rect1 = new DoubleRect(grid3d.Minx, grid3d.Miny, grid3d.Maxx, grid3d.Maxy);
            DoubleRect rect2 = new DoubleRect(grid2d.Minx, grid2d.Miny, grid2d.Maxx, grid2d.Maxy);
            if ( rect1.IsIntersectWith(rect2) )
            {
                exchangeXY = false;
                return true; 
            }
            
            DoubleRect rect3 = new DoubleRect(grid2d.Miny,grid2d.Minx, grid2d.Maxy, grid2d.Maxx);
            if ( rect1.IsIntersectWith(rect3) )
            {
                exchangeXY = true;
                return true;
            }

            return false;
        }

        void toControl()
        {
            if (grid2d != null)
            {
                NX2DtextBox.Text = grid2d.xNum.ToString();
                NY2DtextBox.Text = grid2d.yNum.ToString();
                X12DtextBox.Text = grid2d.Minx.ToString();
                X22DtextBox.Text = grid2d.Maxx.ToString();
                Y12DtextBox.Text = grid2d.Miny.ToString();
                Y22DtextBox.Text = grid2d.Maxy.ToString();
                Z12DtextBox.Text = grid2d.Minv.ToString();
                Z22DtextBox.Text = grid2d.Maxv.ToString();
            }
            if (grid3d != null)
            {
                Nx3DtextBox.Text = grid3d.xNum.ToString();
                Ny3DtextBox.Text = grid3d.yNum.ToString();
                X13DtextBox.Text = grid3d.Minx.ToString();
                X23DtextBox.Text = grid3d.Maxx.ToString();
                Y13DtextBox.Text = grid3d.Miny.ToString();
                Y23DtextBox.Text = grid3d.Maxy.ToString();
                Z13DtextBox.Text = grid3d.Minz.ToString();
                Z23DtextBox.Text = grid3d.Maxz.ToString();
            }
            ZOffsetTextBox.Text = zoffset.ToString();
            checkBox1.Checked = exchangeXY;            

            if (keepUpper) comboBox1.SelectedIndex = 1;
            else comboBox1.SelectedIndex = 0;
        }
        private void GridBlankDlg_Load(object sender, EventArgs e)
        {   
            if( !CheckIsMatched() )
            {
                MessageBox.Show("meshes loaded not match this 3Dgrid data.","data not matched.", MessageBoxButtons.OK,MessageBoxIcon.Warning );
            }
            
            comboBox1.Items.Add("Keep Z Lower");
            comboBox1.Items.Add("Keep Z Upper");
            comboBox1.SelectedIndex = 0;

            toControl();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if ( !CheckIsMatched() )
            {
                MessageBox.Show("meshes does not match the 3D grid data.", "data not matched.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if( !double.TryParse(ZOffsetTextBox.Text, out zoffset) )
            {
                MessageBox.Show("z scale is not valid.");
                return;
            }

            exchangeXY = checkBox1.Checked;
            
            if (comboBox1.SelectedIndex == 0) keepUpper = false;
            else keepUpper = true;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
