namespace DDDSharp
{
    partial class CrossValidationForm
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
            this.BrowseBoreholesButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Boreholes_textBox = new System.Windows.Forms.TextBox();
            this.DataPath_textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.GeoUpdatebutton = new System.Windows.Forms.Button();
            this.GeoFile_textBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.MaxZ_textBox = new System.Windows.Forms.TextBox();
            this.MinZ_textBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.MaxY_textBox = new System.Windows.Forms.TextBox();
            this.MinY_textBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.MaxX_textBox = new System.Windows.Forms.TextBox();
            this.MinX_textBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.Sampled_textBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.InLineNum_textBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.CrossLineNum_textBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.OKbutton = new System.Windows.Forms.Button();
            this.Cancelbutton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.InfoTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowseBoreholesButton
            // 
            this.BrowseBoreholesButton.Location = new System.Drawing.Point(652, 29);
            this.BrowseBoreholesButton.Name = "BrowseBoreholesButton";
            this.BrowseBoreholesButton.Size = new System.Drawing.Size(75, 26);
            this.BrowseBoreholesButton.TabIndex = 0;
            this.BrowseBoreholesButton.Text = "Browse";
            this.BrowseBoreholesButton.UseVisualStyleBackColor = true;
            this.BrowseBoreholesButton.Click += new System.EventHandler(this.BrowseBoreholesButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Boreholes";
            // 
            // Boreholes_textBox
            // 
            this.Boreholes_textBox.Location = new System.Drawing.Point(107, 29);
            this.Boreholes_textBox.Name = "Boreholes_textBox";
            this.Boreholes_textBox.Size = new System.Drawing.Size(539, 28);
            this.Boreholes_textBox.TabIndex = 2;
            // 
            // DataPath_textBox
            // 
            this.DataPath_textBox.Location = new System.Drawing.Point(107, 65);
            this.DataPath_textBox.Name = "DataPath_textBox";
            this.DataPath_textBox.Size = new System.Drawing.Size(539, 28);
            this.DataPath_textBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "Data Path";
            // 
            // GeoUpdatebutton
            // 
            this.GeoUpdatebutton.Location = new System.Drawing.Point(575, 97);
            this.GeoUpdatebutton.Name = "GeoUpdatebutton";
            this.GeoUpdatebutton.Size = new System.Drawing.Size(75, 30);
            this.GeoUpdatebutton.TabIndex = 3;
            this.GeoUpdatebutton.Text = "Update";
            this.GeoUpdatebutton.UseVisualStyleBackColor = true;
            this.GeoUpdatebutton.Click += new System.EventHandler(this.GeoUpdatebutton_Click);
            // 
            // GeoFile_textBox
            // 
            this.GeoFile_textBox.Location = new System.Drawing.Point(107, 99);
            this.GeoFile_textBox.Name = "GeoFile_textBox";
            this.GeoFile_textBox.Size = new System.Drawing.Size(462, 28);
            this.GeoFile_textBox.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 102);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 18);
            this.label7.TabIndex = 9;
            this.label7.Text = "GEO File";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.MaxZ_textBox);
            this.groupBox1.Controls.Add(this.MinZ_textBox);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.MaxY_textBox);
            this.groupBox1.Controls.Add(this.MinY_textBox);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.MaxX_textBox);
            this.groupBox1.Controls.Add(this.MinX_textBox);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.Sampled_textBox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.InLineNum_textBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.CrossLineNum_textBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(15, 144);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(650, 151);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Export Uncertainties 3DGrid";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(445, 111);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(26, 18);
            this.label12.TabIndex = 19;
            this.label12.Text = "to";
            // 
            // MaxZ_textBox
            // 
            this.MaxZ_textBox.Location = new System.Drawing.Point(494, 108);
            this.MaxZ_textBox.Name = "MaxZ_textBox";
            this.MaxZ_textBox.Size = new System.Drawing.Size(137, 28);
            this.MaxZ_textBox.TabIndex = 21;
            // 
            // MinZ_textBox
            // 
            this.MinZ_textBox.Location = new System.Drawing.Point(306, 108);
            this.MinZ_textBox.Name = "MinZ_textBox";
            this.MinZ_textBox.Size = new System.Drawing.Size(124, 28);
            this.MinZ_textBox.TabIndex = 20;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(445, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(26, 18);
            this.label11.TabIndex = 16;
            this.label11.Text = "to";
            // 
            // MaxY_textBox
            // 
            this.MaxY_textBox.Location = new System.Drawing.Point(494, 74);
            this.MaxY_textBox.Name = "MaxY_textBox";
            this.MaxY_textBox.Size = new System.Drawing.Size(137, 28);
            this.MaxY_textBox.TabIndex = 18;
            // 
            // MinY_textBox
            // 
            this.MinY_textBox.Location = new System.Drawing.Point(306, 74);
            this.MinY_textBox.Name = "MinY_textBox";
            this.MinY_textBox.Size = new System.Drawing.Size(124, 28);
            this.MinY_textBox.TabIndex = 17;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(445, 43);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 18);
            this.label10.TabIndex = 13;
            this.label10.Text = "to";
            // 
            // MaxX_textBox
            // 
            this.MaxX_textBox.Location = new System.Drawing.Point(494, 40);
            this.MaxX_textBox.Name = "MaxX_textBox";
            this.MaxX_textBox.Size = new System.Drawing.Size(137, 28);
            this.MaxX_textBox.TabIndex = 15;
            // 
            // MinX_textBox
            // 
            this.MinX_textBox.Location = new System.Drawing.Point(306, 40);
            this.MinX_textBox.Name = "MinX_textBox";
            this.MinX_textBox.Size = new System.Drawing.Size(124, 28);
            this.MinX_textBox.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(283, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 18);
            this.label6.TabIndex = 13;
            this.label6.Text = "z";
            // 
            // Sampled_textBox
            // 
            this.Sampled_textBox.Location = new System.Drawing.Point(183, 108);
            this.Sampled_textBox.Name = "Sampled_textBox";
            this.Sampled_textBox.Size = new System.Drawing.Size(66, 28);
            this.Sampled_textBox.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(283, 77);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 18);
            this.label8.TabIndex = 12;
            this.label8.Text = "y";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(283, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(17, 18);
            this.label9.TabIndex = 11;
            this.label9.Text = "x";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 111);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 18);
            this.label5.TabIndex = 10;
            this.label5.Text = "Sampled Num(z)";
            // 
            // InLineNum_textBox
            // 
            this.InLineNum_textBox.Location = new System.Drawing.Point(183, 74);
            this.InLineNum_textBox.Name = "InLineNum_textBox";
            this.InLineNum_textBox.Size = new System.Drawing.Size(66, 28);
            this.InLineNum_textBox.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 18);
            this.label4.TabIndex = 8;
            this.label4.Text = "InLine Num(y)";
            // 
            // CrossLineNum_textBox
            // 
            this.CrossLineNum_textBox.Location = new System.Drawing.Point(183, 40);
            this.CrossLineNum_textBox.Name = "CrossLineNum_textBox";
            this.CrossLineNum_textBox.Size = new System.Drawing.Size(66, 28);
            this.CrossLineNum_textBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 18);
            this.label3.TabIndex = 6;
            this.label3.Text = "CrossLine Num(x)";
            // 
            // OKbutton
            // 
            this.OKbutton.Location = new System.Drawing.Point(624, 301);
            this.OKbutton.Name = "OKbutton";
            this.OKbutton.Size = new System.Drawing.Size(70, 30);
            this.OKbutton.TabIndex = 11;
            this.OKbutton.Text = "Start";
            this.OKbutton.UseVisualStyleBackColor = true;
            this.OKbutton.Click += new System.EventHandler(this.OKbutton_Click);
            // 
            // Cancelbutton
            // 
            this.Cancelbutton.Location = new System.Drawing.Point(684, 522);
            this.Cancelbutton.Name = "Cancelbutton";
            this.Cancelbutton.Size = new System.Drawing.Size(75, 30);
            this.Cancelbutton.TabIndex = 12;
            this.Cancelbutton.Text = "Close";
            this.Cancelbutton.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 301);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(606, 30);
            this.progressBar1.TabIndex = 13;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.AutoSize = true;
            this.ProgressLabel.Location = new System.Drawing.Point(277, 334);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(89, 18);
            this.ProgressLabel.TabIndex = 14;
            this.ProgressLabel.Text = "         ";
            // 
            // InfoTextBox
            // 
            this.InfoTextBox.Location = new System.Drawing.Point(15, 360);
            this.InfoTextBox.Multiline = true;
            this.InfoTextBox.Name = "InfoTextBox";
            this.InfoTextBox.Size = new System.Drawing.Size(650, 160);
            this.InfoTextBox.TabIndex = 15;
            // 
            // CrossValidationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 555);
            this.Controls.Add(this.InfoTextBox);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.Cancelbutton);
            this.Controls.Add(this.OKbutton);
            this.Controls.Add(this.GeoFile_textBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.DataPath_textBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.GeoUpdatebutton);
            this.Controls.Add(this.Boreholes_textBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BrowseBoreholesButton);
            this.Name = "CrossValidationForm";
            this.Text = "CrossValidationForm";
            this.Load += new System.EventHandler(this.CrossValidationForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BrowseBoreholesButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Boreholes_textBox;
        private System.Windows.Forms.TextBox DataPath_textBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button GeoUpdatebutton;
        private System.Windows.Forms.TextBox GeoFile_textBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox MaxZ_textBox;
        private System.Windows.Forms.TextBox MinZ_textBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox MaxY_textBox;
        private System.Windows.Forms.TextBox MinY_textBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox MaxX_textBox;
        private System.Windows.Forms.TextBox MinX_textBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox Sampled_textBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox InLineNum_textBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox CrossLineNum_textBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button OKbutton;
        private System.Windows.Forms.Button Cancelbutton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label ProgressLabel;
        private System.Windows.Forms.TextBox InfoTextBox;
    }
}