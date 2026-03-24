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

    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void About_Load(object sender, EventArgs e)
        {
            RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
            if ( reg.ReadFromRegister() )
                LableAuthorization.Text = "This product had been authorised to " + C3DData.UserID;
            else 
            {
                Text = "About 3D Surfer --Unregistered version";
                LableAuthorization.Text = "Unregistered version "; 
            }

            LabelDevice.Text = C3DData.graphics3D.GetGraphicName();
            LabelEngine.Text = C3DData.graphics3D.engine.ToString();
            VersionLabel.Text = "Version V" + C3DData.Version + ".";
            VersionLabel.Text += "20260101";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
    }
}
