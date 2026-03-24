namespace DDDSharp.Modeling
{
    partial class InterpolatedSlicerModelingForm
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
            this.Cancel = new System.Windows.Forms.Button();
            this.CreateButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Clear = new System.Windows.Forms.Button();
            this.Remove = new System.Windows.Forms.Button();
            this.DownButton = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.UpButton = new System.Windows.Forms.Button();
            this.LoadSlicerButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressLabl = new System.Windows.Forms.Label();
            this.OK = new System.Windows.Forms.Button();
            this.TotalTimesLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.Imerging = new System.Windows.Forms.Button();
            this.SamplingInterpolation = new System.Windows.Forms.Button();
            this.ColorDiffTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.GPUInterpolation = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SampleByPixelCheckBox = new System.Windows.Forms.CheckBox();
            this.ColorSampleStepY = new System.Windows.Forms.TextBox();
            this.ColorSampleStepX = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.ExportButton = new System.Windows.Forms.Button();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.Location = new System.Drawing.Point(635, 486);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(124, 32);
            this.Cancel.TabIndex = 38;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // CreateButton
            // 
            this.CreateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CreateButton.Location = new System.Drawing.Point(325, 23);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(75, 29);
            this.CreateButton.TabIndex = 37;
            this.CreateButton.Text = "Start";
            this.CreateButton.UseVisualStyleBackColor = true;
            this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.listBox2);
            this.groupBox2.Location = new System.Drawing.Point(451, 20);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(308, 298);
            this.groupBox2.TabIndex = 36;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Stratum Objects";
            // 
            // listBox2
            // 
            this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalExtent = 500;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.ItemHeight = 15;
            this.listBox2.Location = new System.Drawing.Point(6, 24);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(296, 259);
            this.listBox2.TabIndex = 31;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.Clear);
            this.groupBox1.Controls.Add(this.Remove);
            this.groupBox1.Controls.Add(this.DownButton);
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Controls.Add(this.UpButton);
            this.groupBox1.Controls.Add(this.LoadSlicerButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(402, 345);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Slicers";
            // 
            // Clear
            // 
            this.Clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Clear.Location = new System.Drawing.Point(332, 125);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(65, 26);
            this.Clear.TabIndex = 17;
            this.Clear.Text = "Clear";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Remove.Location = new System.Drawing.Point(332, 83);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(64, 26);
            this.Remove.TabIndex = 16;
            this.Remove.Text = "Remove";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // DownButton
            // 
            this.DownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DownButton.Location = new System.Drawing.Point(332, 313);
            this.DownButton.Name = "DownButton";
            this.DownButton.Size = new System.Drawing.Size(65, 26);
            this.DownButton.TabIndex = 15;
            this.DownButton.Text = "Down";
            this.DownButton.UseVisualStyleBackColor = true;
            this.DownButton.Click += new System.EventHandler(this.DownButton_Click);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalExtent = 500;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(6, 24);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(304, 304);
            this.listBox1.TabIndex = 0;
            // 
            // UpButton
            // 
            this.UpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.UpButton.Location = new System.Drawing.Point(332, 272);
            this.UpButton.Name = "UpButton";
            this.UpButton.Size = new System.Drawing.Size(65, 26);
            this.UpButton.TabIndex = 14;
            this.UpButton.Text = "Up";
            this.UpButton.UseVisualStyleBackColor = true;
            this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
            // 
            // LoadSlicerButton
            // 
            this.LoadSlicerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadSlicerButton.Location = new System.Drawing.Point(332, 24);
            this.LoadSlicerButton.Name = "LoadSlicerButton";
            this.LoadSlicerButton.Size = new System.Drawing.Size(64, 26);
            this.LoadSlicerButton.TabIndex = 12;
            this.LoadSlicerButton.Text = "Load";
            this.LoadSlicerButton.UseVisualStyleBackColor = true;
            this.LoadSlicerButton.Click += new System.EventHandler(this.LoadSlicerButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar1.Location = new System.Drawing.Point(12, 495);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(402, 23);
            this.progressBar1.TabIndex = 39;
            // 
            // progressLabl
            // 
            this.progressLabl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressLabl.AutoSize = true;
            this.progressLabl.Location = new System.Drawing.Point(12, 521);
            this.progressLabl.Name = "progressLabl";
            this.progressLabl.Size = new System.Drawing.Size(39, 15);
            this.progressLabl.TabIndex = 40;
            this.progressLabl.Text = "XNum";
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OK.Location = new System.Drawing.Point(457, 486);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(124, 32);
            this.OK.TabIndex = 41;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // TotalTimesLabel
            // 
            this.TotalTimesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TotalTimesLabel.AutoSize = true;
            this.TotalTimesLabel.Location = new System.Drawing.Point(12, 543);
            this.TotalTimesLabel.Name = "TotalTimesLabel";
            this.TotalTimesLabel.Size = new System.Drawing.Size(39, 15);
            this.TotalTimesLabel.TabIndex = 42;
            this.TotalTimesLabel.Text = "XNum";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.textBox3);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(12, 371);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(402, 61);
            this.groupBox3.TabIndex = 43;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Geometry of Interpolation";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(213, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 15);
            this.label3.TabIndex = 49;
            this.label3.Text = "ZNum";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(151, 24);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(58, 25);
            this.textBox2.TabIndex = 46;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(253, 24);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(58, 25);
            this.textBox3.TabIndex = 48;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(48, 24);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(58, 25);
            this.textBox1.TabIndex = 44;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(111, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 15);
            this.label2.TabIndex = 47;
            this.label2.Text = "YNum";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 45;
            this.label1.Text = "XNum";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox4.Controls.Add(this.checkBox2);
            this.groupBox4.Controls.Add(this.checkBox1);
            this.groupBox4.Controls.Add(this.CreateButton);
            this.groupBox4.Location = new System.Drawing.Point(12, 437);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(402, 61);
            this.groupBox4.TabIndex = 44;
            this.groupBox4.TabStop = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 15);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(189, 19);
            this.checkBox1.TabIndex = 47;
            this.checkBox1.Text = "NearestInterpolation";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // Imerging
            // 
            this.Imerging.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Imerging.Location = new System.Drawing.Point(457, 446);
            this.Imerging.Name = "Imerging";
            this.Imerging.Size = new System.Drawing.Size(124, 29);
            this.Imerging.TabIndex = 45;
            this.Imerging.Text = "Imerging";
            this.Imerging.UseVisualStyleBackColor = true;
            this.Imerging.Click += new System.EventHandler(this.Imerging_Click);
            // 
            // SamplingInterpolation
            // 
            this.SamplingInterpolation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SamplingInterpolation.Location = new System.Drawing.Point(339, 536);
            this.SamplingInterpolation.Name = "SamplingInterpolation";
            this.SamplingInterpolation.Size = new System.Drawing.Size(75, 29);
            this.SamplingInterpolation.TabIndex = 46;
            this.SamplingInterpolation.Text = "Create";
            this.SamplingInterpolation.UseVisualStyleBackColor = true;
            this.SamplingInterpolation.Click += new System.EventHandler(this.SamplingInterpolation_Click);
            // 
            // ColorDiffTextBox
            // 
            this.ColorDiffTextBox.Location = new System.Drawing.Point(98, 55);
            this.ColorDiffTextBox.Name = "ColorDiffTextBox";
            this.ColorDiffTextBox.Size = new System.Drawing.Size(58, 25);
            this.ColorDiffTextBox.TabIndex = 47;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 62);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 15);
            this.label5.TabIndex = 48;
            this.label5.Text = "Deviation";
            // 
            // GPUInterpolation
            // 
            this.GPUInterpolation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GPUInterpolation.Location = new System.Drawing.Point(420, 536);
            this.GPUInterpolation.Name = "GPUInterpolation";
            this.GPUInterpolation.Size = new System.Drawing.Size(124, 29);
            this.GPUInterpolation.TabIndex = 49;
            this.GPUInterpolation.Text = "GPUInterpo";
            this.GPUInterpolation.UseVisualStyleBackColor = true;
            this.GPUInterpolation.Click += new System.EventHandler(this.GPUInterpolation_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.SampleByPixelCheckBox);
            this.groupBox5.Controls.Add(this.ColorSampleStepY);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.ColorDiffTextBox);
            this.groupBox5.Controls.Add(this.ColorSampleStepX);
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Location = new System.Drawing.Point(451, 324);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(308, 96);
            this.groupBox5.TabIndex = 50;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Slicers Sampling Step";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 15);
            this.label6.TabIndex = 51;
            this.label6.Text = "Horizontal";
            // 
            // SampleByPixelCheckBox
            // 
            this.SampleByPixelCheckBox.AutoSize = true;
            this.SampleByPixelCheckBox.Location = new System.Drawing.Point(173, 59);
            this.SampleByPixelCheckBox.Name = "SampleByPixelCheckBox";
            this.SampleByPixelCheckBox.Size = new System.Drawing.Size(101, 19);
            this.SampleByPixelCheckBox.TabIndex = 51;
            this.SampleByPixelCheckBox.Text = "By pixels";
            this.SampleByPixelCheckBox.UseVisualStyleBackColor = true;
            this.SampleByPixelCheckBox.CheckedChanged += new System.EventHandler(this.SampleByPixelCheckBox_CheckedChanged);
            // 
            // ColorSampleStepY
            // 
            this.ColorSampleStepY.Location = new System.Drawing.Point(244, 24);
            this.ColorSampleStepY.Name = "ColorSampleStepY";
            this.ColorSampleStepY.Size = new System.Drawing.Size(58, 25);
            this.ColorSampleStepY.TabIndex = 46;
            // 
            // ColorSampleStepX
            // 
            this.ColorSampleStepX.Location = new System.Drawing.Point(98, 24);
            this.ColorSampleStepX.Name = "ColorSampleStepX";
            this.ColorSampleStepX.Size = new System.Drawing.Size(58, 25);
            this.ColorSampleStepX.TabIndex = 44;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(170, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 15);
            this.label7.TabIndex = 47;
            this.label7.Text = "Vertical";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 15);
            this.label8.TabIndex = 45;
            // 
            // ExportButton
            // 
            this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExportButton.Location = new System.Drawing.Point(635, 446);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(124, 29);
            this.ExportButton.TabIndex = 51;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 40);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(141, 19);
            this.checkBox2.TabIndex = 52;
            this.checkBox2.Text = "Invalid Filter";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // InterpolatedSlicerModelingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 585);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.GPUInterpolation);
            this.Controls.Add(this.SamplingInterpolation);
            this.Controls.Add(this.Imerging);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.TotalTimesLabel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.progressLabl);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "InterpolatedSlicerModelingForm";
            this.Text = "InterpolatedSlicerModelingForm";
            this.Load += new System.EventHandler(this.InterpolatedSlicerModelingForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button CreateButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button Clear;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button DownButton;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button UpButton;
        private System.Windows.Forms.Button LoadSlicerButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label progressLabl;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Label TotalTimesLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button Imerging;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button SamplingInterpolation;
        private System.Windows.Forms.TextBox ColorDiffTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button GPUInterpolation;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox SampleByPixelCheckBox;
        private System.Windows.Forms.TextBox ColorSampleStepY;
        private System.Windows.Forms.TextBox ColorSampleStepX;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}