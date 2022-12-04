namespace DDDSharp
{
    partial class SlicerModelingDlg
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LoadSlicersButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ExportColorScaleButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.LoadButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.CreateButton = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.LayerValueTextBox = new System.Windows.Forms.TextBox();
            this.BkValueTextBox = new System.Windows.Forms.TextBox();
            this.ResetLayerCheckBox = new System.Windows.Forms.CheckBox();
            this.SampleBoudaryCheckBox = new System.Windows.Forms.CheckBox();
            this.BoundaryStepTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(6, 19);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(229, 274);
            this.listBox1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LoadSlicersButton);
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.ForeColor = System.Drawing.Color.MediumOrchid;
            this.groupBox1.Location = new System.Drawing.Point(30, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(241, 336);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Slicers list";
            // 
            // LoadSlicersButton
            // 
            this.LoadSlicersButton.ForeColor = System.Drawing.Color.MediumBlue;
            this.LoadSlicersButton.Location = new System.Drawing.Point(6, 305);
            this.LoadSlicersButton.Name = "LoadSlicersButton";
            this.LoadSlicersButton.Size = new System.Drawing.Size(125, 31);
            this.LoadSlicersButton.TabIndex = 17;
            this.LoadSlicersButton.Text = "Load From";
            this.LoadSlicersButton.UseVisualStyleBackColor = true;
            this.LoadSlicersButton.Click += new System.EventHandler(this.LoadSlicersButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ExportColorScaleButton);
            this.groupBox2.Controls.Add(this.SaveButton);
            this.groupBox2.Controls.Add(this.dataGridView1);
            this.groupBox2.Controls.Add(this.LoadButton);
            this.groupBox2.ForeColor = System.Drawing.Color.MediumOrchid;
            this.groupBox2.Location = new System.Drawing.Point(345, 18);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(359, 336);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Layers list";
            // 
            // ExportColorScaleButton
            // 
            this.ExportColorScaleButton.ForeColor = System.Drawing.Color.MediumBlue;
            this.ExportColorScaleButton.Location = new System.Drawing.Point(240, 305);
            this.ExportColorScaleButton.Name = "ExportColorScaleButton";
            this.ExportColorScaleButton.Size = new System.Drawing.Size(113, 31);
            this.ExportColorScaleButton.TabIndex = 18;
            this.ExportColorScaleButton.Text = "Export Color";
            this.ExportColorScaleButton.UseVisualStyleBackColor = true;
            this.ExportColorScaleButton.Click += new System.EventHandler(this.ExportColorScaleButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.ForeColor = System.Drawing.Color.MediumBlue;
            this.SaveButton.Location = new System.Drawing.Point(87, 305);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 31);
            this.SaveButton.TabIndex = 17;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(6, 24);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(347, 271);
            this.dataGridView1.TabIndex = 15;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // LoadButton
            // 
            this.LoadButton.ForeColor = System.Drawing.Color.MediumBlue;
            this.LoadButton.Location = new System.Drawing.Point(6, 305);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(75, 31);
            this.LoadButton.TabIndex = 16;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.BoundaryStepTextBox);
            this.groupBox3.Controls.Add(this.SampleBoudaryCheckBox);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.textBox3);
            this.groupBox3.Controls.Add(this.textBox4);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.ForeColor = System.Drawing.Color.MediumOrchid;
            this.groupBox3.Location = new System.Drawing.Point(30, 360);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(293, 130);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Layer Sampling";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label6.Location = new System.Drawing.Point(11, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(127, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "Resample Extent";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label5.Location = new System.Drawing.Point(28, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 15);
            this.label5.TabIndex = 15;
            this.label5.Text = "Sampling Grid";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label2.Location = new System.Drawing.Point(219, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "Y Number";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(142, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "X Number";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(145, 67);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(68, 25);
            this.textBox3.TabIndex = 8;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(219, 68);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(68, 25);
            this.textBox4.TabIndex = 7;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(219, 37);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(68, 25);
            this.textBox2.TabIndex = 5;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(145, 37);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(68, 25);
            this.textBox1.TabIndex = 1;
            // 
            // ExportButton
            // 
            this.ExportButton.ForeColor = System.Drawing.Color.MediumBlue;
            this.ExportButton.Location = new System.Drawing.Point(280, 82);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(83, 30);
            this.ExportButton.TabIndex = 4;
            this.ExportButton.Text = "Export ";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(580, 525);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(124, 30);
            this.Cancel.TabIndex = 5;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(30, 496);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(674, 23);
            this.progressBar1.TabIndex = 6;
            // 
            // CreateButton
            // 
            this.CreateButton.ForeColor = System.Drawing.Color.MediumBlue;
            this.CreateButton.Location = new System.Drawing.Point(280, 48);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(83, 30);
            this.CreateButton.TabIndex = 7;
            this.CreateButton.Text = "Create";
            this.CreateButton.UseVisualStyleBackColor = true;
            this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(30, 525);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(124, 30);
            this.OK.TabIndex = 7;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.comboBox1);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.LayerValueTextBox);
            this.groupBox4.Controls.Add(this.BkValueTextBox);
            this.groupBox4.Controls.Add(this.ExportButton);
            this.groupBox4.Controls.Add(this.ResetLayerCheckBox);
            this.groupBox4.Controls.Add(this.CreateButton);
            this.groupBox4.ForeColor = System.Drawing.Color.MediumOrchid;
            this.groupBox4.Location = new System.Drawing.Point(329, 360);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(375, 130);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Layers Select";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(20, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 15);
            this.label7.TabIndex = 21;
            this.label7.Text = "Layers";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(81, 20);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(178, 23);
            this.comboBox1.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(19, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 15);
            this.label3.TabIndex = 18;
            this.label3.Text = "Background Value";
            // 
            // LayerValueTextBox
            // 
            this.LayerValueTextBox.Location = new System.Drawing.Point(191, 86);
            this.LayerValueTextBox.Name = "LayerValueTextBox";
            this.LayerValueTextBox.Size = new System.Drawing.Size(68, 25);
            this.LayerValueTextBox.TabIndex = 17;
            // 
            // BkValueTextBox
            // 
            this.BkValueTextBox.Location = new System.Drawing.Point(191, 49);
            this.BkValueTextBox.Name = "BkValueTextBox";
            this.BkValueTextBox.Size = new System.Drawing.Size(68, 25);
            this.BkValueTextBox.TabIndex = 16;
            // 
            // ResetLayerCheckBox
            // 
            this.ResetLayerCheckBox.AutoSize = true;
            this.ResetLayerCheckBox.ForeColor = System.Drawing.Color.Black;
            this.ResetLayerCheckBox.Location = new System.Drawing.Point(22, 89);
            this.ResetLayerCheckBox.Name = "ResetLayerCheckBox";
            this.ResetLayerCheckBox.Size = new System.Drawing.Size(165, 19);
            this.ResetLayerCheckBox.TabIndex = 15;
            this.ResetLayerCheckBox.Text = "Reset Layer Value";
            this.ResetLayerCheckBox.UseVisualStyleBackColor = true;
            // 
            // SampleBoudaryCheckBox
            // 
            this.SampleBoudaryCheckBox.AutoSize = true;
            this.SampleBoudaryCheckBox.Location = new System.Drawing.Point(14, 102);
            this.SampleBoudaryCheckBox.Name = "SampleBoudaryCheckBox";
            this.SampleBoudaryCheckBox.Size = new System.Drawing.Size(149, 19);
            this.SampleBoudaryCheckBox.TabIndex = 17;
            this.SampleBoudaryCheckBox.Text = "Sample Boundary";
            this.SampleBoudaryCheckBox.UseVisualStyleBackColor = true;
            // 
            // BoundaryStepTextBox
            // 
            this.BoundaryStepTextBox.Location = new System.Drawing.Point(159, 98);
            this.BoundaryStepTextBox.Name = "BoundaryStepTextBox";
            this.BoundaryStepTextBox.Size = new System.Drawing.Size(83, 25);
            this.BoundaryStepTextBox.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label4.Location = new System.Drawing.Point(248, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 17;
            this.label4.Text = "step";
            // 
            // SlicerModelingDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 596);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "SlicerModelingDlg";
            this.Text = "Create Models From Outlines";
            this.Load += new System.EventHandler(this.SlicerModelingDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button CreateButton;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.Button ExportColorScaleButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox LayerValueTextBox;
        private System.Windows.Forms.TextBox BkValueTextBox;
        private System.Windows.Forms.CheckBox ResetLayerCheckBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button LoadSlicersButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox BoundaryStepTextBox;
        private System.Windows.Forms.CheckBox SampleBoudaryCheckBox;
    }
}