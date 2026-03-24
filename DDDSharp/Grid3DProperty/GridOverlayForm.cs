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
    public partial class GridOverlayForm : Form
    {
        public C3DGridData p3D = null;        
        private C3DObjectBase data1,data2;
        List<int> Keys = new List<int>();
        public GridOverlayForm()
        {
            InitializeComponent();
        }

        private void UpdateList()
        {  
            gridsListcomboBox.Items.Clear();
            Keys.Clear();

            foreach (var item in C3DData.objectsDiction)
            {
                gridsListcomboBox.Items.Add(item.Value.Name);
                Keys.Add(item.Key);
            }

            combineMethodComboBox.Items.Clear();
            combineMethodComboBox.Items.Add("Add to");   
            combineMethodComboBox.Items.Add("substract");
            combineMethodComboBox.Items.Add("Average");
            combineMethodComboBox.Items.Add("replace");

            string[] names = Enum.GetNames( typeof(OverlapChannel) );
            channelComboBox.Items.Clear();
            channelComboBox.Items.AddRange(names);         
        }        
        private C3DObjectBase GetFromObj()
        {
            if (radioButton1.Checked)
            {
                int id1 = gridsListcomboBox.SelectedIndex;
                if (id1 < 0)
                {
                    MessageBox.Show("please select an object.");
                    return null;
                }
                return C3DData.GetObjectByKey(Keys[id1]);
            }
            else
            {
                if(textGrdFile.Text.Length <1 )
                {
                    MessageBox.Show("please choose data from file.");
                    return null;
                }
                C3DGridData data = new C3DGridData();
                if (data.LoadFrom(textGrdFile.Text)) return data;
                else
                {
                    MessageBox.Show("load data failed." + data.errMessage);
                    return null;
                }
            }
        }
        private void OK_Click(object sender, EventArgs e)
        {
            data1 = p3D;
            data2 = GetFromObj();

            if (data1 == null || data2 == null) return;

            this.Cursor = Cursors.WaitCursor;

            Arrow2DOverlayObject obj = new Arrow2DOverlayObject(p3D.pGridData.Length,1);
            obj.channel = (OverlapChannel)Enum.Parse(typeof(OverlapChannel), channelComboBox.SelectedItem.ToString(), false);            

            obj.p3DGrid = p3D;
            if(data2.type == ShapeEnum.Grid3D) obj.from3Dgrid( (C3DGridData)data2 );
            else if(data2.type == ShapeEnum.Points) obj.fromScatterPoints((ScatteredPoints)data2);
                        
            p3D.overlaps.Add(obj);

            this.Cursor = Cursors.Default;

            DialogResult = DialogResult.OK;
            this.Close();
            /*
            data1 = p3D;
            data2 = GetFromObj();

            if (data1 == null || data2 == null) return;

            this.Cursor = Cursors.WaitCursor;

            int method = combineMethodComboBox.SelectedIndex;            
            C3DGridData data = MergeData(data1,data2,method);
            data.UpdateRange();
            data.NormalizeGrid(0,1);
            //normalize to 0 - 1
            
            int id2 = gridsListcomboBox.SelectedIndex;
            int channel = channelComboBox.SelectedIndex;
            
            if (channel == 0)//value
            {                
                p3D = data;
            }
            if (channel == 1)//alpha
            {
                data1.overlap = new COverlayObject(1);
                data1.overlap.data = data.pGridData;
                data1.overlap.UpdateRange();
                data1.objP32 = p32;
                p3D = data1;
            }

            this.Cursor = Cursors.Default;

            DialogResult = DialogResult.OK;
            this.Close();
            */
        }
        private double MergeValue(double v1,double v2, int method)
        {
            //comboBox2.Items.Add("Add to");
            //comboBox2.Items.Add("substract");
            //comboBox2.Items.Add("Average");
            //comboBox2.Items.Add("replace");
            if (method == 0) return v1 + v2;
            else if (method == 1) return v1 - v2;
            else if (method == 2) return (v1 + v2) / 2;
            else if (method == 3) return 1-v2;
            else return 0;
        }
        private C3DGridData MergeData(C3DGridData data1, C3DGridData data2,int method)
        {
            int nx1 = data1.xNum;
            int ny1 = data1.yNum;
            int nz1 = data1.zNum;
            int nx2 = data2.xNum;
            int ny2 = data2.yNum;
            int nz2 = data2.zNum;
            long id1, id2;
            int ix2, iy2, iz2;
            double v1, v2,v;

            C3DGridData data = new C3DGridData();
            data.pGridData = new float[nx1*ny1*nz1];
            if (data.pGridData == null) return null;
            data.xNum = nx1;
            data.yNum = ny1;
            data.zNum = nz1;
            data.minx = data1.minx;
            data.miny = data1.miny;
            data.minz = data1.minz;
            data.maxx = data1.maxx;
            data.maxy = data1.maxy;
            data.maxz = data1.maxz;

            for (int iz = 0; iz < nz1; iz++)
            {
                iz2 = iz * (nz2-1) / (nz1-1);
                for (int iy = 0; iy < ny1; iy++)
                {
                    iy2 = iy * (ny2 - 1) / (ny1 - 1);
                    for (int ix = 0; ix < nx1; ix++)
                    {
                        ix2 = ix * (nx2 - 1) / (nx1 - 1);
                        id1 = ix + iy * nx1 + iz * nx1 * ny1;
                        id2 = ix2 + iy2 * nx2 + iz2 * nx2 * ny2;
                        v1 = data1.pGridData[id1];
                        v2 = data2.pGridData[id2];
                        v = MergeValue(v1, v2, method);
                        data.pGridData[id1] = (float)v;
                    }
                }
            }

            data.UpdateRange();
            return data;
        }
        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "3D Grid Data(*.3Dgrid)|*.3Dgrid|all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        textGrdFile.Text = dlg.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        C3DGridData p32 = null;
        private void LoadP32button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "3D Grid Data(*.3Dgrid)|*.3Dgrid|all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        p32 = new C3DGridData();
                        p32.LoadFrom(dlg.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }


        private void GridOverlayForm_Load(object sender, EventArgs e)
        {
            UpdateList();
        }

    }
}
