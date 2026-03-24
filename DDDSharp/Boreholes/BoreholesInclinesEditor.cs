using DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormAnimation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DDDSharp.Boreholes
{
    public partial class BoreholesInclinesEditor : Form
    {
        public CBoreholes boreholes = null;
        bool Modified = false;
        double minDepth, maxDepth;
        PointF cursor = new PointF(-1, -1);
        
        int sel1=-1, sel2 = -1;
        public BoreholesInclinesEditor(CBoreholes _boreholes)
        {
            InitializeComponent();
            boreholes = _boreholes.Copy();            
        }

        private void BoreholesInclinesEditor_Load(object sender, EventArgs e)
        {
            Text = boreholes.Name + " Incline Parameters";
            UpdateView();            
            propertyGrid1.PropertySort = PropertySort.Categorized;
        }
        /// <summary>
        /// 根据表格内容确定选择当前数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        bool DoSelect(string name, int index)
        {
            sel1 = sel2 = -1;
            for (int k = 0; k < boreholes.Count; k++)
            {
                CBorehole bh = boreholes[k];
                if(bh.Name.ToLower() == name.ToLower())
                {
                    sel1 = k;
                    sel2 = index - 1;
                    return true;
                }
            }
            return false;
        }
        void UpdateView()
        {
            listView1.Items.Clear();
            if (boreholes == null) return;

            BoreholeAnglesStruct an;

            this.listView1.BeginUpdate();
            for (int k = 0; k < boreholes.Count; k++)
            {
                CBorehole bh = boreholes[k];
                
                for (int i = 0; i < bh.boreholeAngles.Count; i++)
                {
                    an = bh.boreholeAngles[i];
                    ListViewItem liv = new ListViewItem(bh.Name);
                    liv.SubItems.Add((i + 1).ToString()); 
                    liv.SubItems.Add(an.Depth.ToString());
                    liv.SubItems.Add(an.Azimuth.ToString());
                    liv.SubItems.Add(an.Zenith.ToString());
                    listView1.Items.Add(liv);
                }
            }
            this.listView1.EndUpdate();
        }
        void UpdateView(int irow, BoreholeAnglesStruct an)
        {
            if (an == null) return;
            if (irow < 0 || irow >= listView1.Items.Count) return;
            ListViewItem liv = listView1.Items[irow];

            this.listView1.BeginUpdate();
            //0 - Borehole name
            //1 - index
            liv.SubItems[2].Text = an.Depth.ToString();
            liv.SubItems[3].Text = an.Azimuth.ToString();
            liv.SubItems[4].Text = an.Zenith.ToString();

            this.listView1.EndUpdate();
        }        

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                for(int i=0;i< listView1.SelectedIndices.Count;i++)
                {
                    int sel = listView1.SelectedIndices[i];
                    string name = listView1.Items[sel].SubItems[0].Text;
                    string ids = listView1.Items[sel].SubItems[1].Text;
                    if (DoSelect(name.Trim(), int.Parse(ids))) 
                    {
                        CBorehole bh = boreholes[sel1];
                        BoreholeAnglesStruct an = bh.boreholeAngles[sel2];
                        an.Depth = an.Azimuth = 0; //设置为无效数据                         
                        bh.boreholeAngles[sel2] = an;
                        boreholes[sel1] = bh;
                    }                       
                }   

                for( int i=boreholes.Count-1; i>=0; i--)
                {
                    CBorehole bh = boreholes[sel1];
                    
                    for(int j=bh.boreholeAngles.Count-1;j>=0; j--)
                    {
                        if (!bh.boreholeAngles[j].IsValid())
                            bh.boreholeAngles.Angles.RemoveAt(j);
                    }
                    if (bh.boreholeAngles.Count < 1) 
                        boreholes.pData.RemoveAt(i);

                    listView1.SelectedIndices.Clear();                    
                    Modified = true;                    
                }
                UpdateView();
            }
        }

        private void aNewBoreholeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void addToCurrentBoreholeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = listView1.SelectedIndices[0];
            if ( sel < 0 || sel1<0 ) return;
            
            CBorehole bh = boreholes[sel1];

            int n = listView1.Items.Count;
            int index = bh.boreholeAngles.Count;
            
            BoreholeAnglesStruct an = new BoreholeAnglesStruct(bh.Name);
            ListViewItem liv = listView1.Items.Add(bh.Name);
            liv.SubItems.Add(index.ToString());
            liv.SubItems.Add(an.Depth.ToString());
            liv.SubItems.Add(an.Azimuth.ToString());
            liv.SubItems.Add(an.Zenith.ToString());
            listView1.Items.Insert(sel,liv);

            bh.boreholeAngles.Add(an);
            boreholes[sel1] = bh;

            listView1.SelectedItems.Clear();
            listView1.SelectedIndices.Add(sel);
            Modified = true;
        }

        private void OK_Click(object sender, EventArgs e)
        {            
            this.Close(); 
            DialogResult = DialogResult.OK;
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

        private void beforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = listView1.SelectedIndices[0];
            if (sel < 0 || sel1 < 0) return;

            CBorehole bh = boreholes[sel1];

            int n = listView1.Items.Count;
            int index = bh.boreholeAngles.Count;

            BoreholeAnglesStruct an = new BoreholeAnglesStruct(bh.Name);
            ListViewItem liv = new ListViewItem(bh.Name);
            liv.SubItems.Add(index.ToString());
            liv.SubItems.Add(an.Depth.ToString());
            liv.SubItems.Add(an.Azimuth.ToString());
            liv.SubItems.Add(an.Zenith.ToString());
            listView1.Items.Insert(sel, liv);

            bh.boreholeAngles.Add(an);
            boreholes[sel1] = bh;

            listView1.SelectedItems.Clear();
            listView1.SelectedIndices.Add(sel);
            Modified = true;
        }

        private void afterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = listView1.SelectedIndices[0];
            if (sel < 0 || sel1 < 0) return;

            CBorehole bh = boreholes[sel1];

            int n = listView1.Items.Count;
            int index = bh.boreholeAngles.Count;

            BoreholeAnglesStruct an = new BoreholeAnglesStruct(bh.Name);
            ListViewItem liv = new ListViewItem(bh.Name);
            liv.SubItems.Add(index.ToString());
            liv.SubItems.Add(an.Depth.ToString());
            liv.SubItems.Add(an.Azimuth.ToString());
            liv.SubItems.Add(an.Zenith.ToString());
            if (sel < n-1) listView1.Items.Insert(sel + 1, liv);
            else listView1.Items.Add(liv);

            bh.boreholeAngles.Add(an);
            boreholes[sel1] = bh;

            listView1.SelectedItems.Clear();
            listView1.SelectedIndices.Add(sel+1);
            Modified = true;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listView1.Items.Count;
            string name = Dialogs.MessageBoxs.InputDlgShow("信息输入", "输入钻孔名");
            
            CBorehole bh = new CBorehole(name);
            bool existed = false;
            int id = -1;
            //search from list
            for (int i=0; i < boreholes.Count; i++)
            {                
                if(name == boreholes[i].Name)
                {
                    bh = boreholes[i];
                    existed = true;
                    id = i;
                    break;
                }
            }            

            BoreholeAnglesStruct an = new BoreholeAnglesStruct(name);
            ListViewItem liv = listView1.Items.Add(bh.Name);
            liv.SubItems.Add("1");
            liv.SubItems.Add(an.Depth.ToString());
            liv.SubItems.Add(an.Azimuth.ToString());
            liv.SubItems.Add(an.Zenith.ToString());
            bh.boreholeAngles.Add(an);
                        
            if( existed ) boreholes[id] = bh;
            else boreholes.AddBorehole(bh);

            listView1.SelectedItems.Clear();
            listView1.SelectedIndices.Add(n);
            Modified = true;
        }
        bool LoadFromExcel(string filename)
        {
            string[]columns = new string[4];
            char[] splitchar = new char[] { ',', '\t', ' ' };
            try 
            {
                StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
                string line = "";
                int i = 0;
                string bhName = "";
                
                boreholes.Clear();
                CBorehole bh = new CBorehole();
                BoreholeAnglesStruct an = new BoreholeAnglesStruct();
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length < 1) continue;
                    if (line.Length > 1 && line[0] == '/' && line[1] == '/') continue; //双斜杠备注                    
                    if (line[0] == '!' || line[0] == '#') continue;
                    string[] ss = line.Split(splitchar, StringSplitOptions.RemoveEmptyEntries);
                    if (ss.Length < 4) continue;
                    if (i == 0)  //标题行
                    {
                        columns = ss;
                        i++;continue; 
                    }
                    an = new BoreholeAnglesStruct();
                    an.Name = ss[0];    //钻孔编号
                    an.Depth = double.Parse(ss[1]);//钻孔深度

                    if (columns[2].Contains("倾角") ||
                        columns[2].Contains("顶角") ||
                        columns[2].ToLower().Contains("zenith"))
                    {
                        an.Zenith = double.Parse(ss[2]); //天顶角（倾角）
                        an.Azimuth = double.Parse(ss[3]);//方位角（倾向）
                    }
                    else if (columns[2].Contains("倾向") ||
                        columns[2].Contains("方位") ||
                        columns[2].ToLower().Contains("azimuth"))
                    {                       
                        an.Azimuth = double.Parse(ss[2]);//方位角（倾向）
                        an.Zenith = double.Parse(ss[3]); //天顶角（倾角）
                    }
                    else
                    {
                        an.Azimuth = double.Parse(ss[2]);//方位角（倾向）
                        an.Zenith = double.Parse(ss[3]); //天顶角（倾角）
                    }                   
                    //-----------------
                    if (bh.Name != ss[0]) 
                    {
                        if(bh.boreholeAngles.Count == 0) 
                        {
                            bh = new CBorehole(ss[0]);
                            bh.boreholeAngles.Add(an);
                        }
                        else
                        {
                            boreholes.AddBorehole(bh);
                            bh = new CBorehole(ss[0]);
                            bh.boreholeAngles.Add(an);
                        }
                    }
                    else bh.boreholeAngles.Add(an);
                    ss = null;
                }
                columns = null;
                splitchar = null;
                boreholes.AddBorehole(bh);
                sr.Close();
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            
        }
        private void loadFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = Resource1.BoreholesInclinesFileFormatFilter;
                dlg.Filter += "|" + "All Files(*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                
                if( LoadFromExcel(dlg.FileName))                
                {
                    Modified = true;
                    UpdateView();
                }                
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int sel = listView1.SelectedIndices[0];
                string name = listView1.Items[sel].SubItems[0].Text;
                string ids = listView1.Items[sel].SubItems[1].Text;
                if( DoSelect(name.Trim(),int.Parse(ids)) )
                {
                    propertyGrid1.SelectedObject = boreholes[sel1].boreholeAngles[sel2];                    
                }                
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int sel = listView1.SelectedIndices[0];
                string name = listView1.Items[sel].SubItems[0].Text;
                string ids = listView1.Items[sel].SubItems[1].Text;
                if (DoSelect(name.Trim(), int.Parse(ids)))
                {
                    BoreholeAnglesStruct an = propertyGrid1.SelectedObject as BoreholeAnglesStruct;
                    if (an.IsValid())
                    {
                        UpdateView(sel, an);
                        CBorehole bh = boreholes[sel1];                        
                        bh.boreholeAngles[sel2] = an;
                        boreholes[sel1] = bh;
                        Modified = true;
                    }                    
                }                
            }
        }

        



    }
}
