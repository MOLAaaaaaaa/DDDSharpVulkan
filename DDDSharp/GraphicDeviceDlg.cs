using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graphics3D;
using DataCollection;
namespace DDDSharp
{
    public partial class GraphicDeviceDlg : Form
    {
        public GraphicDeviceInfo[] devices = null;
        public string driversinfo = "";
        public string curName = "";
        int graphicsSel = -1;
        int engineSel = -1;

        public GraphicDeviceDlg()
        {
            InitializeComponent();
        }
        
        void UpdateList()
        {
            graphicsSel = -1;
            engineSel = -1;
            if (devices != null)
            {                
                for (int i = 0; i < devices.Length; i++)
                {
                    comboBox1.Items.Add(devices[i].Name);
                    if (CGraphic3D.DeviceNameCompare(curName, devices[i].Name))
                    { 
                        comboBox1.SelectedIndex = i;
                        graphicsSel = i;
                    }
                }
            }
            string[] names = Enum.GetNames(typeof(gEngine));
            for (int i = 0; i < names.Length; i++)
            {
                comboBox2.Items.Add(names[i]);
                if ( C3DData.graphics3D.engine.ToString() == names[i] )
                {
                    comboBox2.SelectedIndex = i;
                    engineSel = i;
                }
            }
            GLInfoLabel.Text = driversinfo;
        }
        private void GraphicDeviceDlg_Load(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBox1.SelectedIndex;
            if (index < 0) return;
            GraphicDeviceInfo device = devices[index];
            textBox1.Text = device.Name;
            textBox2.Text = device.MemorySize.ToString()+" MB";
            textBox3.Text = device.InstalledDisplayDrivers;
            textBox4.Text = device.DriverVersion;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            int sel1 = comboBox1.SelectedIndex;
            if (sel1 > -1) C3DData.graphics3D.graphicDevice = devices[sel1].Name;

            int sel2 = comboBox2.SelectedIndex;
            if (sel2 < 0)
            {
                MessageBox.Show("Invalid selection.");
                return;
            }
            gEngine engine = (gEngine)sel2;
            if (engine != C3DData.graphics3D.engine)
            {
                C3DData.graphics3D.engine = engine;
            }

            if(sel1 != graphicsSel || sel2 != engineSel )
            {
                C3DData.graphics3D.SaveGraphicConfig();

                MessageBox.Show("graphics configeration have been changed,please restart the application.");
            }
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
