namespace DDDSharp.Analyze
{
    partial class OverlayAnalyseForm
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.AddToButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PropertTMore = new System.Windows.Forms.Button();
            this.TypeComboBox = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.PropertyComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LoadStratum = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.CANCEL = new System.Windows.Forms.Button();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.DoAnalyze = new System.Windows.Forms.Button();
            this.textBoxX1 = new System.Windows.Forms.TextBox();
            this.textBoxX2 = new System.Windows.Forms.TextBox();
            this.textBoxNX = new System.Windows.Forms.TextBox();
            this.textBoxNY = new System.Windows.Forms.TextBox();
            this.textBoxY2 = new System.Windows.Forms.TextBox();
            this.textBoxY1 = new System.Windows.Forms.TextBox();
            this.textBoxNZ = new System.Windows.Forms.TextBox();
            this.textBoxZ2 = new System.Windows.Forms.TextBox();
            this.textBoxZ1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ZStepTextBox = new System.Windows.Forms.TextBox();
            this.XStepTextBox = new System.Windows.Forms.TextBox();
            this.YStepTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SaveButton = new System.Windows.Forms.Button();
            this.Loadbutton = new System.Windows.Forms.Button();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.PropertyChoosePropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(12, 16);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(469, 304);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // listBox2
            // 
            this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.ItemHeight = 15;
            this.listBox2.Location = new System.Drawing.Point(503, 16);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(592, 304);
            this.listBox2.TabIndex = 1;
            this.listBox2.SelectedValueChanged += new System.EventHandler(this.listBox2_SelectedValueChanged);
            // 
            // AddToButton
            // 
            this.AddToButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddToButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.AddToButton.Location = new System.Drawing.Point(425, 426);
            this.AddToButton.Name = "AddToButton";
            this.AddToButton.Size = new System.Drawing.Size(56, 30);
            this.AddToButton.TabIndex = 2;
            this.AddToButton.Text = ">>";
            this.AddToButton.UseVisualStyleBackColor = false;
            this.AddToButton.Click += new System.EventHandler(this.AddToButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.PropertTMore);
            this.groupBox1.Controls.Add(this.TypeComboBox);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.PropertyComboBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 329);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(469, 91);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Properties";
            // 
            // PropertTMore
            // 
            this.PropertTMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PropertTMore.Location = new System.Drawing.Point(418, 52);
            this.PropertTMore.Name = "PropertTMore";
            this.PropertTMore.Size = new System.Drawing.Size(43, 25);
            this.PropertTMore.TabIndex = 26;
            this.PropertTMore.Text = "...";
            this.PropertTMore.UseVisualStyleBackColor = true;
            this.PropertTMore.Click += new System.EventHandler(this.PropertTMore_Click);
            // 
            // TypeComboBox
            // 
            this.TypeComboBox.FormattingEnabled = true;
            this.TypeComboBox.Location = new System.Drawing.Point(80, 22);
            this.TypeComboBox.Name = "TypeComboBox";
            this.TypeComboBox.Size = new System.Drawing.Size(332, 23);
            this.TypeComboBox.TabIndex = 23;
            this.TypeComboBox.SelectedIndexChanged += new System.EventHandler(this.TypeComboBox_SelectedIndexChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(37, 25);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(39, 15);
            this.label13.TabIndex = 22;
            this.label13.Text = "Type";
            // 
            // PropertyComboBox
            // 
            this.PropertyComboBox.FormattingEnabled = true;
            this.PropertyComboBox.Location = new System.Drawing.Point(80, 53);
            this.PropertyComboBox.Name = "PropertyComboBox";
            this.PropertyComboBox.Size = new System.Drawing.Size(332, 23);
            this.PropertyComboBox.TabIndex = 2;
            this.PropertyComboBox.SelectedIndexChanged += new System.EventHandler(this.PropertiesComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Property";
            // 
            // LoadStratum
            // 
            this.LoadStratum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadStratum.Location = new System.Drawing.Point(946, 646);
            this.LoadStratum.Name = "LoadStratum";
            this.LoadStratum.Size = new System.Drawing.Size(43, 30);
            this.LoadStratum.TabIndex = 22;
            this.LoadStratum.Text = "...";
            this.LoadStratum.UseVisualStyleBackColor = true;
            this.LoadStratum.Click += new System.EventHandler(this.LoadStratum_Click);
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OK.Location = new System.Drawing.Point(502, 700);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(128, 33);
            this.OK.TabIndex = 4;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // CANCEL
            // 
            this.CANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CANCEL.Location = new System.Drawing.Point(967, 700);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(128, 33);
            this.CANCEL.TabIndex = 5;
            this.CANCEL.Text = "CANCEL";
            this.CANCEL.UseVisualStyleBackColor = true;
            this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
            // 
            // RemoveButton
            // 
            this.RemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RemoveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.RemoveButton.Location = new System.Drawing.Point(502, 646);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(56, 30);
            this.RemoveButton.TabIndex = 6;
            this.RemoveButton.Text = "<<";
            this.RemoveButton.UseVisualStyleBackColor = false;
            this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // DoAnalyze
            // 
            this.DoAnalyze.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DoAnalyze.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.DoAnalyze.Location = new System.Drawing.Point(1003, 643);
            this.DoAnalyze.Name = "DoAnalyze";
            this.DoAnalyze.Size = new System.Drawing.Size(92, 33);
            this.DoAnalyze.TabIndex = 7;
            this.DoAnalyze.Text = "Create";
            this.DoAnalyze.UseVisualStyleBackColor = false;
            this.DoAnalyze.Click += new System.EventHandler(this.DoAnalyze_Click);
            // 
            // textBoxX1
            // 
            this.textBoxX1.Location = new System.Drawing.Point(19, 36);
            this.textBoxX1.Name = "textBoxX1";
            this.textBoxX1.Size = new System.Drawing.Size(104, 25);
            this.textBoxX1.TabIndex = 8;
            this.textBoxX1.TextChanged += new System.EventHandler(this.XYZRangeChanged);
            // 
            // textBoxX2
            // 
            this.textBoxX2.Location = new System.Drawing.Point(143, 36);
            this.textBoxX2.Name = "textBoxX2";
            this.textBoxX2.Size = new System.Drawing.Size(104, 25);
            this.textBoxX2.TabIndex = 9;
            this.textBoxX2.TextChanged += new System.EventHandler(this.XYZRangeChanged);
            // 
            // textBoxNX
            // 
            this.textBoxNX.Location = new System.Drawing.Point(399, 35);
            this.textBoxNX.Name = "textBoxNX";
            this.textBoxNX.Size = new System.Drawing.Size(44, 25);
            this.textBoxNX.TabIndex = 10;
            this.textBoxNX.TextChanged += new System.EventHandler(this.XYZNumChanged);
            // 
            // textBoxNY
            // 
            this.textBoxNY.Location = new System.Drawing.Point(399, 64);
            this.textBoxNY.Name = "textBoxNY";
            this.textBoxNY.Size = new System.Drawing.Size(44, 25);
            this.textBoxNY.TabIndex = 16;
            this.textBoxNY.TextChanged += new System.EventHandler(this.XYZNumChanged);
            // 
            // textBoxY2
            // 
            this.textBoxY2.Location = new System.Drawing.Point(143, 65);
            this.textBoxY2.Name = "textBoxY2";
            this.textBoxY2.Size = new System.Drawing.Size(104, 25);
            this.textBoxY2.TabIndex = 15;
            this.textBoxY2.TextChanged += new System.EventHandler(this.XYZRangeChanged);
            // 
            // textBoxY1
            // 
            this.textBoxY1.Location = new System.Drawing.Point(19, 65);
            this.textBoxY1.Name = "textBoxY1";
            this.textBoxY1.Size = new System.Drawing.Size(104, 25);
            this.textBoxY1.TabIndex = 14;
            this.textBoxY1.TextChanged += new System.EventHandler(this.XYZRangeChanged);
            // 
            // textBoxNZ
            // 
            this.textBoxNZ.Location = new System.Drawing.Point(399, 93);
            this.textBoxNZ.Name = "textBoxNZ";
            this.textBoxNZ.Size = new System.Drawing.Size(44, 25);
            this.textBoxNZ.TabIndex = 19;
            this.textBoxNZ.TextChanged += new System.EventHandler(this.XYZNumChanged);
            // 
            // textBoxZ2
            // 
            this.textBoxZ2.Location = new System.Drawing.Point(143, 94);
            this.textBoxZ2.Name = "textBoxZ2";
            this.textBoxZ2.Size = new System.Drawing.Size(104, 25);
            this.textBoxZ2.TabIndex = 18;
            this.textBoxZ2.TextChanged += new System.EventHandler(this.XYZRangeChanged);
            // 
            // textBoxZ1
            // 
            this.textBoxZ1.Location = new System.Drawing.Point(19, 94);
            this.textBoxZ1.Name = "textBoxZ1";
            this.textBoxZ1.Size = new System.Drawing.Size(104, 25);
            this.textBoxZ1.TabIndex = 17;
            this.textBoxZ1.TextChanged += new System.EventHandler(this.XYZRangeChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label6.Location = new System.Drawing.Point(18, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 15);
            this.label6.TabIndex = 20;
            this.label6.Text = "Minimum";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.ZStepTextBox);
            this.groupBox2.Controls.Add(this.XStepTextBox);
            this.groupBox2.Controls.Add(this.YStepTextBox);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.textBoxX1);
            this.groupBox2.Controls.Add(this.textBoxNZ);
            this.groupBox2.Controls.Add(this.textBoxX2);
            this.groupBox2.Controls.Add(this.textBoxZ2);
            this.groupBox2.Controls.Add(this.textBoxNX);
            this.groupBox2.Controls.Add(this.textBoxZ1);
            this.groupBox2.Controls.Add(this.textBoxNY);
            this.groupBox2.Controls.Add(this.textBoxY2);
            this.groupBox2.Controls.Add(this.textBoxY1);
            this.groupBox2.Location = new System.Drawing.Point(12, 603);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(469, 130);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "buffer geometry";
            // 
            // ZStepTextBox
            // 
            this.ZStepTextBox.Location = new System.Drawing.Point(271, 93);
            this.ZStepTextBox.Name = "ZStepTextBox";
            this.ZStepTextBox.Size = new System.Drawing.Size(97, 25);
            this.ZStepTextBox.TabIndex = 27;
            this.ZStepTextBox.VisibleChanged += new System.EventHandler(this.XYZStepChanged);
            // 
            // XStepTextBox
            // 
            this.XStepTextBox.Location = new System.Drawing.Point(271, 35);
            this.XStepTextBox.Name = "XStepTextBox";
            this.XStepTextBox.Size = new System.Drawing.Size(97, 25);
            this.XStepTextBox.TabIndex = 25;
            this.XStepTextBox.TextChanged += new System.EventHandler(this.XYZStepChanged);
            // 
            // YStepTextBox
            // 
            this.YStepTextBox.Location = new System.Drawing.Point(271, 64);
            this.YStepTextBox.Name = "YStepTextBox";
            this.YStepTextBox.Size = new System.Drawing.Size(97, 25);
            this.YStepTextBox.TabIndex = 26;
            this.YStepTextBox.TextChanged += new System.EventHandler(this.XYZStepChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(2, 99);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(15, 15);
            this.label12.TabIndex = 23;
            this.label12.Text = "Z";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(2, 71);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(15, 15);
            this.label11.TabIndex = 23;
            this.label11.Text = "Y";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(2, 41);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(15, 15);
            this.label10.TabIndex = 22;
            this.label10.Text = "X";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label9.Location = new System.Drawing.Point(269, 17);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 15);
            this.label9.TabIndex = 24;
            this.label9.Text = "Space";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label8.Location = new System.Drawing.Point(397, 17);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 15);
            this.label8.TabIndex = 23;
            this.label8.Text = "#No.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.label7.Location = new System.Drawing.Point(145, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 15);
            this.label7.TabIndex = 22;
            this.label7.Text = "Maximum";
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveButton.Location = new System.Drawing.Point(680, 646);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(61, 30);
            this.SaveButton.TabIndex = 23;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // Loadbutton
            // 
            this.Loadbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Loadbutton.Location = new System.Drawing.Point(589, 646);
            this.Loadbutton.Name = "Loadbutton";
            this.Loadbutton.Size = new System.Drawing.Size(61, 30);
            this.Loadbutton.TabIndex = 24;
            this.Loadbutton.Text = "Load";
            this.Loadbutton.UseVisualStyleBackColor = true;
            this.Loadbutton.Click += new System.EventHandler(this.Loadbutton_Click);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(503, 340);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid1.Size = new System.Drawing.Size(592, 301);
            this.propertyGrid1.TabIndex = 25;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // PropertyChoosePropertyGrid
            // 
            this.PropertyChoosePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PropertyChoosePropertyGrid.HelpVisible = false;
            this.PropertyChoosePropertyGrid.Location = new System.Drawing.Point(12, 426);
            this.PropertyChoosePropertyGrid.Name = "PropertyChoosePropertyGrid";
            this.PropertyChoosePropertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.PropertyChoosePropertyGrid.Size = new System.Drawing.Size(469, 171);
            this.PropertyChoosePropertyGrid.TabIndex = 26;
            // 
            // OverlayAnalyseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1107, 749);
            this.Controls.Add(this.AddToButton);
            this.Controls.Add(this.PropertyChoosePropertyGrid);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.Loadbutton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.DoAnalyze);
            this.Controls.Add(this.LoadStratum);
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.listBox1);
            this.MaximizeBox = false;
            this.Name = "OverlayAnalyseForm";
            this.Text = "成矿空间分析";
            this.Load += new System.EventHandler(this.OverlayAnalyseForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button AddToButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox PropertyComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Button DoAnalyze;
        private System.Windows.Forms.TextBox textBoxX1;
        private System.Windows.Forms.TextBox textBoxX2;
        private System.Windows.Forms.TextBox textBoxNX;
        private System.Windows.Forms.TextBox textBoxNY;
        private System.Windows.Forms.TextBox textBoxY2;
        private System.Windows.Forms.TextBox textBoxY1;
        private System.Windows.Forms.TextBox textBoxNZ;
        private System.Windows.Forms.TextBox textBoxZ2;
        private System.Windows.Forms.TextBox textBoxZ1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button LoadStratum;
        private System.Windows.Forms.ComboBox TypeComboBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox ZStepTextBox;
        private System.Windows.Forms.TextBox XStepTextBox;
        private System.Windows.Forms.TextBox YStepTextBox;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button Loadbutton;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button PropertTMore;
        private System.Windows.Forms.PropertyGrid PropertyChoosePropertyGrid;
    }
}