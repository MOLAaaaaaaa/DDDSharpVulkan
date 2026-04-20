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
            ApplyLanguage();
        }

        public void ApplyLanguage()
        {
            bool zh = AppLocalization.IsChinese;
            Text = zh ? "关于 3D Surfer" : "About 3D Surfer";
            groupBox1.Text = "3D Surfer Plus";
            label5.Text = zh ? "版权所有(C) 2012 - 2026" : "Copyright(C) 2012 - 2026";
            label4.Text = zh ? "成都理工大学。" : "Chengdu University of Technology.";
            label3.Text = zh ? "保留所有权利。" : "All Rights Reserved.";
            button1.Text = zh ? "确定" : "OK";
            label2.Text = zh ? "三维引擎" : "3D Engine";
            label1.Text = zh ? "图形设备" : "Graphics";
            VersionLabel.Text = zh ? "版本 V" + C3DData.Version + ".20260101" : "Version V" + C3DData.Version + ".20260101";
        }

        private void About_Load(object sender, EventArgs e)
        {
            ApplyLanguage();
            RegisterAndEncrypt.RegisterVerify reg = new RegisterAndEncrypt.RegisterVerify(C3DData.UserID);
            if ( reg.ReadFromRegister() )
                LableAuthorization.Text = AppLocalization.IsChinese
                    ? "本产品已授权给 " + C3DData.UserID
                    : "This product had been authorised to " + C3DData.UserID;
            else 
            {
                Text = AppLocalization.IsChinese ? "关于 3D Surfer --未注册版本" : "About 3D Surfer --Unregistered version";
                LableAuthorization.Text = AppLocalization.IsChinese ? "未注册版本" : "Unregistered version"; 
            }

            LabelDevice.Text = C3DData.graphics3D.GetGraphicName();
            LabelEngine.Text = C3DData.graphics3D.engine.ToString();
            VersionLabel.Text = AppLocalization.IsChinese
                ? "版本 V" + C3DData.Version + ".20260101"
                : "Version V" + C3DData.Version + ".20260101";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
    }
}
