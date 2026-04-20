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
using Graphics3D;
namespace DDDSharp
{
    public partial class OptionForm : Form
    {
        private int curLightSelectIndex = -1;
        public List<LightStruct> pLights = new List<LightStruct>();
        public OptionForm()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        public void ApplyLanguage()
        {
            bool zh = AppLocalization.IsChinese;
            Text = zh ? "灯光选项" : "Lights Option";
            groupBox1.Text = zh ? "灯光" : "Lights";
            groupBox2.Text = zh ? "参数" : "Parameters";
            enableLightCheckBox.Text = zh ? "启用" : "enabled";
            OKbutton1.Text = zh ? "确定" : "OK";
            Cancelbutton1.Text = zh ? "取消" : "Cancel";
            label1.Text = zh ? "位置" : "position";
            label2.Text = zh ? "漫反射" : "diffuse";
            label3.Text = zh ? "环境光" : "ambient";
            label4.Text = zh ? "镜面反射" : "specular";
            label5.Text = "x / r";
            label6.Text = "y / g";
            label7.Text = "z / b";
        }

        private bool ConvertToInt(string ss, out int ret)
        {
            try
            {
                ret = Convert.ToInt32(ss);
                return true;
            }
            catch
            {
                ret = 0;
                return false;
            }
        }
        private bool ConvertToFloat(string ss, out float ret)
        {
            try
            {
                ret = Convert.ToSingle(ss);
                return true;
            }
            catch
            {
                ret = 0;
                return false;
            }
        }
        private bool ConvertToDouble(string ss, out double ret)
        {
            try
            {
                ret = Convert.ToDouble(ss);
                return true;
            }
            catch
            {
                ret = 0;
                return false;
            }
        }
        private void OptionForm_Load(object sender, EventArgs e)
        {
            ApplyLanguage();
            for(int i=0;i<pLights.Count && i<8;i++)
            {
                lightsComboBox.Items.Add(AppLocalization.IsChinese ? "灯光" + i.ToString() : "Light" + i.ToString());
            }
        }
        private void UpdateLightSelectedUI()
        {
            enableLightCheckBox.Checked = false;
            lightPosX_textBox.Text = "";
            lightPosY_textBox.Text = "";
            lightPosZ_textBox.Text = "";
            lightDiffuseX_textBox.Text = "";
            lightDiffuseY_textBox.Text = "";
            lightDiffuseZ_textBox.Text = "";
            lightAmtX_textBox.Text = "";
            lightAmtY_textBox.Text = "";
            lightAmtZ_textBox.Text = "";
            lightSpecX_textBox.Text = "";
            lightSpecY_textBox.Text = "";
            lightSpecZ_textBox.Text = "";
            int index = lightsComboBox.SelectedIndex;
            curLightSelectIndex = index;
            if (index>=0)
            {
                enableLightCheckBox.Checked = pLights[index].Enable;
                lightPosX_textBox.Text = pLights[index].pos.x.ToString();
                lightPosY_textBox.Text = pLights[index].pos.y.ToString();
                lightPosZ_textBox.Text = pLights[index].pos.z.ToString();
                lightDiffuseX_textBox.Text = pLights[index].diffuse.x.ToString();
                lightDiffuseY_textBox.Text = pLights[index].diffuse.y.ToString();
                lightDiffuseZ_textBox.Text = pLights[index].diffuse.z.ToString();
                lightAmtX_textBox.Text = pLights[index].ambient.x.ToString();
                lightAmtY_textBox.Text = pLights[index].ambient.y.ToString();
                lightAmtZ_textBox.Text = pLights[index].ambient.z.ToString();
                lightSpecX_textBox.Text = pLights[index].specular.x.ToString();
                lightSpecY_textBox.Text = pLights[index].specular.y.ToString();
                lightSpecZ_textBox.Text = pLights[index].specular.z.ToString();
            }
        }

        private bool UpdateLightSelectedArray()
        {
            if (curLightSelectIndex < 0) return false;

            LightStruct light = new LightStruct(true);
            light.Enable = enableLightCheckBox.Checked;            

            bool ret = true;
            ret = ret && ConvertToFloat(lightPosX_textBox.Text, out light.pos.x);
            ret = ret && ConvertToFloat(lightPosY_textBox.Text, out light.pos.y);
            ret = ret && ConvertToFloat(lightPosZ_textBox.Text, out light.pos.z);

            ret = ret && ConvertToFloat(lightDiffuseX_textBox.Text, out light.diffuse.x);
            ret = ret && ConvertToFloat(lightDiffuseY_textBox.Text, out light.diffuse.y);
            ret = ret && ConvertToFloat(lightDiffuseZ_textBox.Text, out light.diffuse.z);

            ret = ret && ConvertToFloat(lightAmtX_textBox.Text, out light.ambient.x);
            ret = ret && ConvertToFloat(lightAmtY_textBox.Text, out light.ambient.y);
            ret = ret && ConvertToFloat(lightAmtZ_textBox.Text, out light.ambient.z);

            ret = ret && ConvertToFloat(lightSpecX_textBox.Text, out light.specular.x);
            ret = ret && ConvertToFloat(lightSpecY_textBox.Text, out light.specular.y);
            ret = ret && ConvertToFloat(lightSpecZ_textBox.Text, out light.specular.z);
            if( ret )
            {
                pLights[curLightSelectIndex] = light;
            }
            return ret;
        }

        private void lightsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLightSelectedArray();
            UpdateLightSelectedUI();
        }

        private void OKbutton1_Click(object sender, EventArgs e)
        {
            if( !UpdateLightSelectedArray() )
            {
                MessageBox.Show(AppLocalization.IsChinese ? "灯光参数不正确。" : "light set not correct.");
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancelbutton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
