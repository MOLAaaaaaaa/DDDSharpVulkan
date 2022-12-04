namespace DDDSharp
{
    partial class OptionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lightSpecZ_textBox = new System.Windows.Forms.TextBox();
            this.lightAmtZ_textBox = new System.Windows.Forms.TextBox();
            this.lightSpecY_textBox = new System.Windows.Forms.TextBox();
            this.lightSpecX_textBox = new System.Windows.Forms.TextBox();
            this.lightAmtY_textBox = new System.Windows.Forms.TextBox();
            this.lightAmtX_textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lightDiffuseZ_textBox = new System.Windows.Forms.TextBox();
            this.lightDiffuseY_textBox = new System.Windows.Forms.TextBox();
            this.lightDiffuseX_textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lightPosZ_textBox = new System.Windows.Forms.TextBox();
            this.lightPosY_textBox = new System.Windows.Forms.TextBox();
            this.lightPosX_textBox = new System.Windows.Forms.TextBox();
            this.enableLightCheckBox = new System.Windows.Forms.CheckBox();
            this.lightsComboBox = new System.Windows.Forms.ComboBox();
            this.OKbutton1 = new System.Windows.Forms.Button();
            this.Cancelbutton1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lightsComboBox);
            this.groupBox1.Controls.Add(this.enableLightCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(17, 12);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(445, 60);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Lights";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(323, 22);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 15);
            this.label7.TabIndex = 7;
            this.label7.Text = "z / b";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(233, 22);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 15);
            this.label6.TabIndex = 7;
            this.label6.Text = "y / g";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(137, 22);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "x / r";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 133);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "specular";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 103);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 15);
            this.label3.TabIndex = 13;
            this.label3.Text = "ambient";
            // 
            // lightSpecZ_textBox
            // 
            this.lightSpecZ_textBox.Location = new System.Drawing.Point(303, 130);
            this.lightSpecZ_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightSpecZ_textBox.Name = "lightSpecZ_textBox";
            this.lightSpecZ_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightSpecZ_textBox.TabIndex = 8;
            // 
            // lightAmtZ_textBox
            // 
            this.lightAmtZ_textBox.Location = new System.Drawing.Point(303, 100);
            this.lightAmtZ_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightAmtZ_textBox.Name = "lightAmtZ_textBox";
            this.lightAmtZ_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightAmtZ_textBox.TabIndex = 12;
            // 
            // lightSpecY_textBox
            // 
            this.lightSpecY_textBox.Location = new System.Drawing.Point(210, 130);
            this.lightSpecY_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightSpecY_textBox.Name = "lightSpecY_textBox";
            this.lightSpecY_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightSpecY_textBox.TabIndex = 7;
            // 
            // lightSpecX_textBox
            // 
            this.lightSpecX_textBox.Location = new System.Drawing.Point(117, 130);
            this.lightSpecX_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightSpecX_textBox.Name = "lightSpecX_textBox";
            this.lightSpecX_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightSpecX_textBox.TabIndex = 6;
            // 
            // lightAmtY_textBox
            // 
            this.lightAmtY_textBox.Location = new System.Drawing.Point(210, 100);
            this.lightAmtY_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightAmtY_textBox.Name = "lightAmtY_textBox";
            this.lightAmtY_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightAmtY_textBox.TabIndex = 11;
            // 
            // lightAmtX_textBox
            // 
            this.lightAmtX_textBox.Location = new System.Drawing.Point(117, 100);
            this.lightAmtX_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightAmtX_textBox.Name = "lightAmtX_textBox";
            this.lightAmtX_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightAmtX_textBox.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 73);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "diffuse";
            // 
            // lightDiffuseZ_textBox
            // 
            this.lightDiffuseZ_textBox.Location = new System.Drawing.Point(303, 70);
            this.lightDiffuseZ_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightDiffuseZ_textBox.Name = "lightDiffuseZ_textBox";
            this.lightDiffuseZ_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightDiffuseZ_textBox.TabIndex = 8;
            // 
            // lightDiffuseY_textBox
            // 
            this.lightDiffuseY_textBox.Location = new System.Drawing.Point(210, 70);
            this.lightDiffuseY_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightDiffuseY_textBox.Name = "lightDiffuseY_textBox";
            this.lightDiffuseY_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightDiffuseY_textBox.TabIndex = 7;
            // 
            // lightDiffuseX_textBox
            // 
            this.lightDiffuseX_textBox.Location = new System.Drawing.Point(117, 70);
            this.lightDiffuseX_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightDiffuseX_textBox.Name = "lightDiffuseX_textBox";
            this.lightDiffuseX_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightDiffuseX_textBox.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 43);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "position";
            // 
            // lightPosZ_textBox
            // 
            this.lightPosZ_textBox.Location = new System.Drawing.Point(303, 40);
            this.lightPosZ_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightPosZ_textBox.Name = "lightPosZ_textBox";
            this.lightPosZ_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightPosZ_textBox.TabIndex = 4;
            // 
            // lightPosY_textBox
            // 
            this.lightPosY_textBox.Location = new System.Drawing.Point(210, 40);
            this.lightPosY_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightPosY_textBox.Name = "lightPosY_textBox";
            this.lightPosY_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightPosY_textBox.TabIndex = 3;
            // 
            // lightPosX_textBox
            // 
            this.lightPosX_textBox.Location = new System.Drawing.Point(117, 40);
            this.lightPosX_textBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightPosX_textBox.Name = "lightPosX_textBox";
            this.lightPosX_textBox.Size = new System.Drawing.Size(84, 25);
            this.lightPosX_textBox.TabIndex = 2;
            // 
            // enableLightCheckBox
            // 
            this.enableLightCheckBox.AutoSize = true;
            this.enableLightCheckBox.Location = new System.Drawing.Point(252, 26);
            this.enableLightCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.enableLightCheckBox.Name = "enableLightCheckBox";
            this.enableLightCheckBox.Size = new System.Drawing.Size(85, 19);
            this.enableLightCheckBox.TabIndex = 1;
            this.enableLightCheckBox.Text = "enabled";
            this.enableLightCheckBox.UseVisualStyleBackColor = true;
            // 
            // lightsComboBox
            // 
            this.lightsComboBox.FormattingEnabled = true;
            this.lightsComboBox.Location = new System.Drawing.Point(13, 24);
            this.lightsComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lightsComboBox.Name = "lightsComboBox";
            this.lightsComboBox.Size = new System.Drawing.Size(212, 23);
            this.lightsComboBox.TabIndex = 0;
            this.lightsComboBox.SelectedIndexChanged += new System.EventHandler(this.lightsComboBox_SelectedIndexChanged);
            // 
            // OKbutton1
            // 
            this.OKbutton1.Location = new System.Drawing.Point(17, 270);
            this.OKbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.OKbutton1.Name = "OKbutton1";
            this.OKbutton1.Size = new System.Drawing.Size(126, 37);
            this.OKbutton1.TabIndex = 1;
            this.OKbutton1.Text = "OK";
            this.OKbutton1.UseVisualStyleBackColor = true;
            this.OKbutton1.Click += new System.EventHandler(this.OKbutton1_Click);
            // 
            // Cancelbutton1
            // 
            this.Cancelbutton1.Location = new System.Drawing.Point(336, 270);
            this.Cancelbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Cancelbutton1.Name = "Cancelbutton1";
            this.Cancelbutton1.Size = new System.Drawing.Size(126, 37);
            this.Cancelbutton1.TabIndex = 2;
            this.Cancelbutton1.Text = "Cancel";
            this.Cancelbutton1.UseVisualStyleBackColor = true;
            this.Cancelbutton1.Click += new System.EventHandler(this.Cancelbutton1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.lightDiffuseZ_textBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.lightPosX_textBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.lightPosY_textBox);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.lightPosZ_textBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.lightSpecZ_textBox);
            this.groupBox2.Controls.Add(this.lightDiffuseX_textBox);
            this.groupBox2.Controls.Add(this.lightAmtZ_textBox);
            this.groupBox2.Controls.Add(this.lightDiffuseY_textBox);
            this.groupBox2.Controls.Add(this.lightSpecY_textBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.lightSpecX_textBox);
            this.groupBox2.Controls.Add(this.lightAmtX_textBox);
            this.groupBox2.Controls.Add(this.lightAmtY_textBox);
            this.groupBox2.Location = new System.Drawing.Point(17, 78);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(445, 167);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Parameters";
            // 
            // OptionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(487, 321);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.Cancelbutton1);
            this.Controls.Add(this.OKbutton1);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionForm";
            this.Text = "Lights Option";
            this.Load += new System.EventHandler(this.OptionForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox lightSpecZ_textBox;
        private System.Windows.Forms.TextBox lightAmtZ_textBox;
        private System.Windows.Forms.TextBox lightSpecY_textBox;
        private System.Windows.Forms.TextBox lightSpecX_textBox;
        private System.Windows.Forms.TextBox lightAmtY_textBox;
        private System.Windows.Forms.TextBox lightAmtX_textBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox lightDiffuseZ_textBox;
        private System.Windows.Forms.TextBox lightDiffuseY_textBox;
        private System.Windows.Forms.TextBox lightDiffuseX_textBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox lightPosZ_textBox;
        private System.Windows.Forms.TextBox lightPosY_textBox;
        private System.Windows.Forms.TextBox lightPosX_textBox;
        private System.Windows.Forms.CheckBox enableLightCheckBox;
        private System.Windows.Forms.ComboBox lightsComboBox;
        private System.Windows.Forms.Button OKbutton1;
        private System.Windows.Forms.Button Cancelbutton1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}