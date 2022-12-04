using System;
using System.IO;
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
    public partial class C3DGridDataPropertyForm : DockingPaneExt
    {
        private C3DGridData p3D = null;
        private ComboBox pTextureCombo = new ComboBox();
        private ComboBox pTextureModeCombo = new ComboBox();

        public C3DGridDataPropertyForm()
        {
            InitializeComponent();
        }
        public Color ConvertTo(ColorRGBA color)
        {
            return Color.FromArgb(color.A,color.R, color.G, color.B );
        }
        public void UpdateTextureList()
        {
            BindTextureList();
        }
        private void BindTextureModeList()
        {
            DataTable dtTexture = new DataTable();
            dtTexture.Columns.Add("Index");
            dtTexture.Columns.Add("Name");
            //pTextureCombo.Items.Clear();
            pTextureModeCombo.ValueMember = "Index";
            pTextureModeCombo.DisplayMember = "Name";
            pTextureModeCombo.DataSource = Enum.GetNames(typeof(DataCollection.TextureMagFilter)); 
            pTextureModeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        private void BindTextureList()
        {
            DataTable dtTexture = new DataTable();
            dtTexture.Columns.Add("Index");
            dtTexture.Columns.Add("Name");

            DataRow drRow = dtTexture.NewRow();
            drRow[0] = -1;
            drRow[1] = "";
            dtTexture.Rows.Add(drRow);

            for (int i = 0; i < C3DData.pTextures.Count; i++)
            {
                drRow = dtTexture.NewRow();
                drRow[0] = i;
                drRow[1] = C3DData.pTextures[i].Name;
                dtTexture.Rows.Add(drRow);                
            }
            //pTextureCombo.Items.Clear();
            pTextureCombo.ValueMember = "Index";
            pTextureCombo.DisplayMember = "Name";
            pTextureCombo.DataSource = dtTexture;
            pTextureCombo.DropDownStyle = ComboBoxStyle.DropDownList;            
        }
        
        public void UpdateDataGridview()
        {
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if (p3D == null) return;

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            
            CColorScale colorScale = p3D.ColorScale;

            //dataGridView1.Columns.Add("check", "");
            DataGridViewCheckBoxColumn dtCheck = new DataGridViewCheckBoxColumn();
            dtCheck.DataPropertyName = "check";
            dtCheck.HeaderText = "check";
            dataGridView1.Columns.Add(dtCheck);

            dataGridView1.Columns.Add("No", "ID");
            dataGridView1.Columns.Add("Value", "Value");
            dataGridView1.Columns.Add("Color", "Color");
            dataGridView1.Columns.Add("Texture", "Texture");
            dataGridView1.Columns.Add("TexMode", "TexMode");
            
            //dataGridView1.Columns.Add("Line", "Line");
            int texIndex = -1;
            for (int i = 0; i < colorScale.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = colorScale[i].Visible;
                dataGridView1.Rows[i].Cells[1].Value = i + 1;
                dataGridView1.Rows[i].Cells[2].Value = colorScale.GetScaledValue(i);
                dataGridView1.Rows[i].Cells[3].Style.ForeColor = colorScale.GetColor(i);
                dataGridView1.Rows[i].Cells[3].Style.BackColor = colorScale.GetColor(i);

                //texture
                texIndex = C3DData.GetTexIndexFromName(colorScale[i].Texture.Name);
                dataGridView1.Rows[i].Cells[4].Value = colorScale[i].Texture.Name;
                dataGridView1.Rows[i].Cells[4].Tag = texIndex;
                //texture mode
                if (texIndex < 0) dataGridView1.Rows[i].Cells[5].Value = "";
                else dataGridView1.Rows[i].Cells[5].Value = colorScale[i].Texture.mode.ToString();
            }

            //first设置下拉列表框不可见
            BindTextureList();
            BindTextureModeList();
            pTextureCombo.Visible = false;
            // 添加下拉列表框事件
            pTextureCombo.SelectedIndexChanged += new EventHandler(TextureCombo_SelectedIndexChanged);
            pTextureModeCombo.SelectedIndexChanged += new EventHandler(TextureModeCombo_SelectedIndexChanged);
            // 将下拉列表框加入到DataGridView控件中
            this.dataGridView1.Controls.Add(pTextureCombo);
            this.dataGridView1.Controls.Add(pTextureModeCombo);

            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            //dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 8, FontStyle.Regular);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }
        private void TextureCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string texName = ((ComboBox)sender).Text;
            if (texName == null || texName.Length < 1 ) return;

            int index = C3DData.GetTexIndexFromName(texName);
            dataGridView1.CurrentCell.Tag = index;
            if (index < 0)
            {
                dataGridView1.CurrentCell.Value = "";                
            }
            else dataGridView1.CurrentCell.Value = texName;
        }
        private void TextureModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string texName = ((ComboBox)sender).Text;
            if (texName == null) return;
            dataGridView1.CurrentCell.Value = texName;

        }
        private void dataGridView1_SizeChanged(object sender, EventArgs e)
        {
            pTextureCombo.Visible = false;
            pTextureModeCombo.Visible = false;
        }

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            pTextureCombo.Visible = false;
            pTextureModeCombo.Visible = false;
        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            pTextureCombo.Visible = false;
            pTextureModeCombo.Visible = false;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            DataGridViewSelectedCellCollection c1 = dataGridView1.SelectedCells;
        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null) return;
            try
            {                
                if ( dataGridView1.CurrentCell.ColumnIndex == 4)
                {
                    Rectangle rect = dataGridView1.GetCellDisplayRectangle(dataGridView1.CurrentCell.ColumnIndex,
                                                                            dataGridView1.CurrentCell.RowIndex, false);
                    string cellValue = dataGridView1.CurrentCell.Value.ToString();
                    pTextureCombo.Text = cellValue;
                    pTextureCombo.Left = rect.Left;
                    pTextureCombo.Top = rect.Top;
                    pTextureCombo.Width = rect.Width;
                    pTextureCombo.Height = rect.Height;
                    pTextureCombo.Visible = true;
                }
                else if (dataGridView1.CurrentCell.ColumnIndex == 5)
                {
                    Rectangle rect = dataGridView1.GetCellDisplayRectangle(dataGridView1.CurrentCell.ColumnIndex,
                                                                            dataGridView1.CurrentCell.RowIndex, false);
                    string cellValue = dataGridView1.CurrentCell.Value.ToString();
                    pTextureModeCombo.Text = cellValue;
                    pTextureModeCombo.Left = rect.Left;
                    pTextureModeCombo.Top = rect.Top;
                    pTextureModeCombo.Width = rect.Width;
                    pTextureModeCombo.Height = rect.Height;
                    pTextureModeCombo.Visible = true;
                }
                else
                {
                    pTextureCombo.Visible = false;
                    pTextureModeCombo.Visible = false;
                }
            }
            catch
            {
            }

        }
        public void UpdateSelect()
        {
            this.Cursor = Cursors.WaitCursor;
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if ( p3D != null )
            {               
                UpdateDataGridview();
            }
            this.Cursor = DefaultCursor;
        }
        private void C3DGridDataPropertyForm_Load(object sender, EventArgs e)
        {
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if (p3D == null) return;
            comboBox1.Items.Add("m3");
            comboBox1.Items.Add("Km3");
            comboBox1.SelectedIndex = 0;
            UpdateDataGridview();                        
        }

        private void SelectAllButton_Click(object sender, EventArgs e)
        {
            int n = dataGridView1.Rows.Count;
            DataGridViewCheckBoxCell cell;
            for (int i = 0; i < n; i++)
            {
                cell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells[0];
                cell.Value = true;
            }
        }

        private void UnselectAll_Click(object sender, EventArgs e)
        {
            int n = dataGridView1.Rows.Count;            
            for (int i = 0; i < n; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = false;                
            }
        }      
        public void UpdateColorScale()
        {
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if (p3D == null) return;

            int n = dataGridView1.Rows.Count;
            if (n < 1) return;

            this.Cursor = Cursors.WaitCursor;
            DataGridViewCheckBoxCell cell;
            int texIndex = -1;
            string texName = "";
            string texModeName = "";
            DataCollection.TextureMagFilter mode = DataCollection.TextureMagFilter.GL_LINEAR;
            for (int i = 0; i < n && i < p3D.ColorScale.Count; i++)
            {
                cell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells[0];
                try
                {
                    p3D.ColorScale.SetVisible(i, (bool)cell.FormattedValue);//bug 指定的转换无效
                }
                catch (Exception e) 
                { 
                }

                //texture
                texIndex = -1;
                texName = dataGridView1.Rows[i].Cells[4].Value.ToString();
                if (texName.Length > 0)
                {
                    texIndex = Convert.ToInt32(dataGridView1.Rows[i].Cells[4].Tag);
                }
                if( texIndex >=0 )
                {
                    texModeName = dataGridView1.Rows[i].Cells[5].Value.ToString();
                    if (texModeName.Length > 0)
                        mode = (TextureMagFilter)Enum.Parse(typeof(TextureMagFilter), texModeName);
                    else mode = TextureMagFilter.GL_LINEAR;
                    p3D.ColorScale.SetTexture(i, new TextureStruct(C3DData.pTextures[texIndex].bmp, mode, true, texName));                    
                }
                else
                {
                    p3D.ColorScale.SetTexture(i, new TextureStruct());
                }
            }

           // C3DData.pObjects[C3DData.curSel] = p3D;

            p3D.UpdateShowTableFromColorScale();

            string info = "";

            Program.m_MainForm.AddtoInfo(info);

            DateTime t0 = DateTime.Now;

            if (p3D.meshMethod == MCMeshMethod.Cube)
            {
                info = "begin to update grid...";
                Program.m_MainForm.AddtoInfo(info);
                p3D.RenderMode = RenderingUpdateMode.Redraw;
            }
            else if (p3D.meshMethod == MCMeshMethod.MC)
            {
                info = "begin to create triangles with MC...";
                Program.m_MainForm.AddtoInfo(info);
                p3D.CreateMarchingCubeTriangle();
                p3D.RenderMode = RenderingUpdateMode.Redraw;
            }
            else if (p3D.meshMethod == MCMeshMethod.ImprovedMC)
            {
                //Register Verify
                RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
                if (!reg.ReadFromRegister())
                {
                    MessageBox.Show("this is a unregistered version.");
                    return;
                }
                Random rand = new Random();
                RegisterAndEncrypt.HardWareInfo.InfoType type = (RegisterAndEncrypt.HardWareInfo.InfoType)rand.Next(3);
                if (!reg.Verify(type))
                {
                    MessageBox.Show("Unreconginized register information.");
                    return;
                }
                //Register Verify

                info = "begin to create triangles with Improved MC...";
                Program.m_MainForm.AddtoInfo(info);
                p3D.CreateMarchingCubeTriangleExt();
                p3D.RenderMode = RenderingUpdateMode.Redraw;
            }

            DateTime t1 = DateTime.Now;
            double ts = t1.Subtract(t0).TotalMilliseconds;

            info = "updated! times elapsed(ms):" + ts;
            Program.m_MainForm.AddtoInfo(info);

            this.Cursor = Cursors.Default;

            Program.m_MainForm.UpdateDraw(C3DData.objSelected);
        }
        private void UpdateShowButton_Click(object sender, EventArgs e)
        {
            //this is a test to compare the time consuming of the two method;
            //for(int i=0;i<5;i++)TimeConsumingComparisionTest();
            //return;
            ////////////////////////////////////
            UpdateColorScale();

            Cursor = Cursors.WaitCursor;

            int sel = comboBox1.SelectedIndex;

            if (p3D == null) VolumeTextBox.Text = "0";
            else VolumeTextBox.Text = p3D.CalculateVolume(sel).ToString();

            Cursor = Cursors.Default;
        }
        //test--export time consuming result
        public struct timeConsuming
        {
            public int type1;       //0-25 bits,indicate the combination of show layers
            public int type2;       //25-50 bits,indicate the combination of show layers
            public int layerNo;    //show == ture 
            public int gridno1;     // type !=0 or 255
            public int gridno2;     // type !=0 or 255
            public double timeMC;
            public double timeIMC;
            public timeConsuming(int _type1, int _type2,int _layerno,int _gridno1, int _gridno2, double t1,double t2)
            {
                type1 = _type1;
                type2 = _type2;
                layerNo = _layerno;
                gridno1 = _gridno1;
                gridno2 = _gridno2;
                timeMC = t1;
                timeIMC = t2;
            }
            public timeConsuming Copy()
            {
               return new timeConsuming(type1, type2,layerNo, gridno1, gridno2,timeMC, timeIMC);                
            }
        }
        private void TimeConsumingComparisionTest()
        {
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if (p3D == null) return;

            List<timeConsuming> layers = new List<timeConsuming>();

            this.Cursor = Cursors.WaitCursor;
            
            //create random conbine
            Random random = new Random();
            int ra, layerno;
            int colorNum = p3D.ColorScale.Count;
            for (int k = 1; k <= 25;)
            {
                //initial showtable ,set to false
                for (int i = 0; i < colorNum; i++)
                    p3D.ColorScale.SetVisible(i, false);

                //generate layerno 1 - 24
                //layerno = ra = random.Next(1, colorNum-1);
                layerno = k;
                //select layers
                for (int i = 0; i < layerno; i++)
                {
                    ra = random.Next(0, colorNum - 1);
                    if ( p3D.ColorScale[ra].Visible ) continue;
                    p3D.ColorScale.SetVisible(ra,true);
                }
                //get the index type
                int type1 = 0;                
                for (int i = 0; i < 25; i++)
                {
                    if(p3D.ColorScale[i].Visible)
                        type1 += (int)Math.Pow(2, i);
                }
                int type2 = 0;
                for (int i = 25; i < colorNum; i++)
                {
                    if (p3D.ColorScale[i].Visible)
                        type2 += (int)Math.Pow(2, i-25);
                }

                //check if the same type exist
                bool exist = false;
                for (int i = 0; i < layers.Count; i++)
                {
                    if (layers[i].type1 == type1 && layers[i].type2 == type2)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist) continue;

                p3D.UpdateShowTableFromColorScale();

                timeConsuming tc = new timeConsuming();
                tc.type1 = type1;
                tc.type2 = type2;

                double ts;
                DateTime t0,t1;

                tc.layerNo = layerno;

                p3D.m_MarchCube.Clear();
                t0 = DateTime.Now;
                p3D.CreateMarchingCubeTriangle();                
                t1 = DateTime.Now;
                ts = t1.Subtract(t0).TotalMilliseconds;                
                tc.timeMC = Math.Round(ts, 0);
                tc.gridno1 = p3D.m_MarchCube.searchedGridNo;

                p3D.m_MarchCubeExt.Clear();
                t0 = DateTime.Now;
                p3D.CreateMarchingCubeTriangleExt();
                t1 = DateTime.Now;
                ts = t1.Subtract(t0).TotalMilliseconds;
                tc.timeIMC = Math.Round(ts,0);
                tc.gridno2 = p3D.m_MarchCubeExt.searchedGridNo;

                layers.Add(tc);

                k++;
            }
            this.Cursor = Cursors.Default;
            
#pragma warning disable CS0168 // 声明了变量“b”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“c”，但从未使用过
            timeConsuming a, b,c;
#pragma warning restore CS0168 // 声明了变量“c”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“b”，但从未使用过
            /*
            //sort on gridno
            for (int i = 0; i < layers.Count; i++)
            {                
                for (int j = i + 1; j < layers.Count; j++)
                {
                    a = layers[i];
                    b = layers[j];
                    if( a.gridno > b.gridno)
                    {
                        c = a.Copy();
                        layers[i] = b.Copy();
                        layers[j] = c;
                    }
                }
            }
           */
            //print it
            FileStream fs = new FileStream("timeConsumingComparision.dat", FileMode.Append);
            StreamWriter wr = new StreamWriter(fs);
            string ss = "type1,type2,layerno,gridnoMC,gridnoIMC,timeMC,timeIMC";
            //wr.WriteLine(ss);
            for (int i = 0; i < layers.Count; i++)
            {
                a = layers[i];
                ss = a.type1 + ",";
                ss += a.type2 + ",";
                ss += a.layerNo + ",";
                ss += a.gridno1 + ",";
                ss += a.gridno2 + ",";
                ss += a.timeMC + ",";
                ss += a.timeIMC;
                wr.WriteLine(ss);
            }
            wr.Close();
            fs.Close();
        }
        private void SmoothButton_Click(object sender, EventArgs e)
        {

        }

        private void Blank_Click(object sender, EventArgs e)
        {            
            if (p3D == null) return;
            
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "blank grid (*.grd)|*.grd|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    
                    CSurferGrid cs = new CSurferGrid();
                    
                    if( cs.Read(dlg.FileName) )
                    {
                        C2DGridData data2d = new C2DGridData();
                        data2d.SetData(cs.pData, cs.xGrid, cs.yGrid, cs.minx, cs.maxx,cs.miny, cs.maxy,cs.minv,cs.maxv);
                        
                        GridBlankDlg gd = new GridBlankDlg();
                        gd.SetGrids(data2d,p3D);
                        if (gd.ShowDialog() == DialogResult.OK)
                        {
                            if (p3D.CutWithZSurface(data2d, gd.keepUpper,gd.exchangeXY,gd.zoffset))
                            {
                                //C3DData.pObjects[sel] = p3D;
                                UpdateShowButton_Click(sender, e);
                            }
                        }
                    }

                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if (p3D == null) return;

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "3DGrid (*.3DGrid)|*.3DGrid|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    
                    if (p3D.SaveAs(dlg.FileName))
                        MessageBox.Show("data saved to file: \n" + dlg.FileName);
                    else
                        MessageBox.Show("failed to save to file: \n" + dlg.FileName);
                    this.Cursor = DefaultCursor;
                }
            }
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            
            if (C3DData.objSelected != null )
            {
                Program.m_MainForm.UpdateDraw(C3DData.objSelected);
            }

            this.Cursor = DefaultCursor;
        }

        //export data to ...
        private void Export(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "3DGrid data (*.3DGrid)|*.3DGrid|ASCII data(*.csv;*.dat;txt)|*.csv;*.dat;txt|all files(*.*)|*.*";

                dlg.FilterIndex = 0;
                dlg.DefaultExt = "3DGrid";
                string filename;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    bool ret = false;
                    filename = dlg.FileName;
                    string ext = Path.GetExtension(filename).ToLower();
                    
                    this.Cursor = Cursors.WaitCursor;

                    if (dlg.FilterIndex == 1 )//3D GRID
                    {
                        if (ext != ".3dgrid") filename += ".3DGrid";

                        Export3DGridForm exp = new Export3DGridForm();
                        exp.p3D = p3D;
                        exp.filename = filename;
                        exp.ShowDialog();
                        //ret = p3D.SaveAs(filename);
                    }
                    else
                    {
                        if( ext != ".csv" && 
                            ext != ".dat" &&
                            ext != ".txt" ) filename += ".dat";

                        ret = p3D.ExportData(filename);
                        if (ret) MessageBox.Show("exported to file successfully! \n" + dlg.FileName);
                        else MessageBox.Show("failed to exported data to file: \n" + dlg.FileName);
                    }

                    this.Cursor = DefaultCursor;
                    
                }
            }
        }

        private void SlicerButton_Click(object sender, EventArgs e)
        {
            p3D =(C3DGridData) C3DData.GetSelectedObj( ShapeEnum.Grid3D);
            if ( p3D == null ) return;
            SlicerDrawForm cs = new SlicerDrawForm();
            cs.SetDataGrid(p3D);
            //Register Verify
            RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
            if( !reg.ReadFromRegister() )
            {
                MessageBox.Show("this is a unregistered version.");
                return;
            }
            
            Random rand = new Random();
            RegisterAndEncrypt.HardWareInfo.InfoType type = (RegisterAndEncrypt.HardWareInfo.InfoType)rand.Next(3);
            if ( !reg.Verify(type) )
            {
                MessageBox.Show("Unreconginized register information.");
                return;
            }
            
            //Register Verify
            if ( cs.ShowDialog() == DialogResult.OK )
            {
                if (cs.pSlicers.Count > 0)
                {
                    C3DData.lastLoadeds.Clear();
                    for (int i = 0; i < cs.pSlicers.Count; i++)
                    {
                        C3DData.AddObject(cs.pSlicers[i],false);                        
                    }
                    Program.m_MainForm.m_ObjectForm.AddToTree(C3DData.lastLoadeds);
                    Program.m_MainForm.UpdateDraw(C3DData.lastLoadeds);
                }                               
            }
        }

        private void OverlayButton_Click(object sender, EventArgs e)
        {
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if (p3D == null) return;
            GridOverlayForm cs = new GridOverlayForm();
            cs.p3D = p3D;
            if (cs.ShowDialog() == DialogResult.OK)
            {
                Program.m_MainForm.m_ObjectForm.UpdateTree(p3D);
                Program.m_MainForm.m_DDDForm.UpdateDraw(p3D);
            }
        }
       

        private void ChangeOverlapColorButon_Click(object sender, EventArgs e)
        {
            p3D = (C3DGridData)C3DData.GetSelectedObj(ShapeEnum.Grid3D);
            if (p3D == null) return;
            if (p3D.overlaps.Count < 1 ) return;

            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "color level (*.clr)|*.clr|all files(*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    
                    this.Cursor = DefaultCursor;

                }
            }
        }

        private void C3DGridDataPropertyForm_Paint(object sender, PaintEventArgs e)
        {
            if( p3D != null )
            {
                Graphics g = ColorBarBox.CreateGraphics();
                Rectangle rect = new Rectangle(0, 0, ColorBarBox.Width, ColorBarBox.Height);
                p3D.ColorScale.DrawColorBar(g, rect);
            }
        }
        private void EditColorScale()
        {
            if (p3D == null) return;            

            ColorScaleForm cm = new ColorScaleForm();
            cm.data = p3D.pGridData;
            cm.colorscale = p3D.ColorScale;

            //color scale updated
            if (cm.ShowDialog() == DialogResult.OK && cm.updated)
            {
                p3D.ResetColorScale(cm.colorscale);
                //C3DData.pObjects[C3DData.curSel] = p3D;
                UpdateDataGridview();
                UpdateColorScale();
            }
        }
        private void ColorBarBox_DoubleClick(object sender, EventArgs e)
        {
            EditColorScale();
        }

        private void EditColorScaleButton_Click(object sender, EventArgs e)
        {
            EditColorScale();
        }

        private void ColorBarBox_Click(object sender, EventArgs e)
        {

        }

        //颜色双击
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (p3D == null) return;
            if (p3D.ColorScale == null) return;
            if (dataGridView1.CurrentCell == null) return;
            int n = p3D.ColorScale.Count;

            //Rows[i].Cells[3]
            if (dataGridView1.CurrentCell.ColumnIndex != 3) return;
            Color color = dataGridView1.CurrentCell.Style.BackColor;
            ColorDialog cd = new ColorDialog();
            cd.Color = color;
            if( cd.ShowDialog() == DialogResult.OK )
            {
                color = cd.Color;
                int id = dataGridView1.CurrentCell.RowIndex;
                p3D.ColorScale.SetColor(id, color);
                dataGridView1.CurrentCell.Style.BackColor = color;
                dataGridView1.CurrentCell.Style.ForeColor = color;
                UpdateColorScale();
            }
        }

        private void VolumeButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            
            int sel = comboBox1.SelectedIndex;

            if (p3D == null) VolumeTextBox.Text = "0";
            else VolumeTextBox.Text = p3D.CalculateVolume(sel).ToString();

            Cursor = Cursors.Default;
        }
    }
}
