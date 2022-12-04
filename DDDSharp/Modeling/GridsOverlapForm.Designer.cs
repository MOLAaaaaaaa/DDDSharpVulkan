namespace DDDSharp
{
    partial class GridsOverlapForm
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
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DownButton = new System.Windows.Forms.Button();
            this.UpButton = new System.Windows.Forms.Button();
            this.LoadGridButton = new System.Windows.Forms.Button();
            this.MergeButton = new System.Windows.Forms.Button();
            this.SaveAsButton = new System.Windows.Forms.Button();
            this.FillBackGridValueTextBox = new System.Windows.Forms.TextBox();
            this.FillBackgroundCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ResetOutputRangeCheckBox = new System.Windows.Forms.CheckBox();
            this.OutputValueTextBox1 = new System.Windows.Forms.TextBox();
            this.OutputValueTextBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.LayerMintextBox1 = new System.Windows.Forms.TextBox();
            this.LayerMaxtextBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.LayerValuetextBox1 = new System.Windows.Forms.TextBox();
            this.UpdateLayerButton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.EnableLayerValueCheckBox1 = new System.Windows.Forms.CheckBox();
            this.SaveLayerbutton1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
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
            this.listBox1.Size = new System.Drawing.Size(314, 199);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(274, 518);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(124, 30);
            this.OK.TabIndex = 11;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(470, 518);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(124, 30);
            this.Cancel.TabIndex = 10;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DownButton);
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Controls.Add(this.UpButton);
            this.groupBox1.Controls.Add(this.LoadGridButton);
            this.groupBox1.ForeColor = System.Drawing.Color.Blue;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(326, 267);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Grid List";
            // 
            // DownButton
            // 
            this.DownButton.Location = new System.Drawing.Point(266, 229);
            this.DownButton.Name = "DownButton";
            this.DownButton.Size = new System.Drawing.Size(54, 26);
            this.DownButton.TabIndex = 15;
            this.DownButton.Text = "Down";
            this.DownButton.UseVisualStyleBackColor = true;
            this.DownButton.Click += new System.EventHandler(this.DownButton_Click);
            // 
            // UpButton
            // 
            this.UpButton.Location = new System.Drawing.Point(206, 229);
            this.UpButton.Name = "UpButton";
            this.UpButton.Size = new System.Drawing.Size(54, 26);
            this.UpButton.TabIndex = 14;
            this.UpButton.Text = "Up";
            this.UpButton.UseVisualStyleBackColor = true;
            this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
            // 
            // LoadGridButton
            // 
            this.LoadGridButton.Location = new System.Drawing.Point(6, 229);
            this.LoadGridButton.Name = "LoadGridButton";
            this.LoadGridButton.Size = new System.Drawing.Size(102, 26);
            this.LoadGridButton.TabIndex = 12;
            this.LoadGridButton.Text = "Load From";
            this.LoadGridButton.UseVisualStyleBackColor = true;
            this.LoadGridButton.Click += new System.EventHandler(this.LoadGridButton_Click);
            // 
            // MergeButton
            // 
            this.MergeButton.Location = new System.Drawing.Point(447, 47);
            this.MergeButton.Name = "MergeButton";
            this.MergeButton.Size = new System.Drawing.Size(118, 38);
            this.MergeButton.TabIndex = 13;
            this.MergeButton.Text = "Do Overlap";
            this.MergeButton.UseVisualStyleBackColor = true;
            this.MergeButton.Click += new System.EventHandler(this.MergeButton_Click);
            // 
            // SaveAsButton
            // 
            this.SaveAsButton.Location = new System.Drawing.Point(478, 23);
            this.SaveAsButton.Name = "SaveAsButton";
            this.SaveAsButton.Size = new System.Drawing.Size(98, 29);
            this.SaveAsButton.TabIndex = 14;
            this.SaveAsButton.Text = "Save As";
            this.SaveAsButton.UseVisualStyleBackColor = true;
            this.SaveAsButton.Click += new System.EventHandler(this.SaveAsButton_Click);
            // 
            // FillBackGridValueTextBox
            // 
            this.FillBackGridValueTextBox.Location = new System.Drawing.Point(112, 27);
            this.FillBackGridValueTextBox.Name = "FillBackGridValueTextBox";
            this.FillBackGridValueTextBox.Size = new System.Drawing.Size(129, 25);
            this.FillBackGridValueTextBox.TabIndex = 17;
            // 
            // FillBackgroundCheckBox
            // 
            this.FillBackgroundCheckBox.AutoSize = true;
            this.FillBackgroundCheckBox.ForeColor = System.Drawing.Color.Black;
            this.FillBackgroundCheckBox.Location = new System.Drawing.Point(247, 29);
            this.FillBackgroundCheckBox.Name = "FillBackgroundCheckBox";
            this.FillBackgroundCheckBox.Size = new System.Drawing.Size(101, 19);
            this.FillBackgroundCheckBox.TabIndex = 22;
            this.FillBackgroundCheckBox.Text = "full fill";
            this.FillBackgroundCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Controls.Add(this.FillBackGridValueTextBox);
            this.groupBox2.Controls.Add(this.FillBackgroundCheckBox);
            this.groupBox2.Controls.Add(this.MergeButton);
            this.groupBox2.ForeColor = System.Drawing.Color.Blue;
            this.groupBox2.Location = new System.Drawing.Point(12, 292);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(582, 97);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Overlap Option";
            // 
            // ResetOutputRangeCheckBox
            // 
            this.ResetOutputRangeCheckBox.AutoSize = true;
            this.ResetOutputRangeCheckBox.Location = new System.Drawing.Point(17, 27);
            this.ResetOutputRangeCheckBox.Name = "ResetOutputRangeCheckBox";
            this.ResetOutputRangeCheckBox.Size = new System.Drawing.Size(69, 19);
            this.ResetOutputRangeCheckBox.TabIndex = 24;
            this.ResetOutputRangeCheckBox.Text = "Reset";
            this.ResetOutputRangeCheckBox.UseVisualStyleBackColor = true;
            // 
            // OutputValueTextBox1
            // 
            this.OutputValueTextBox1.Location = new System.Drawing.Point(159, 24);
            this.OutputValueTextBox1.Name = "OutputValueTextBox1";
            this.OutputValueTextBox1.Size = new System.Drawing.Size(84, 25);
            this.OutputValueTextBox1.TabIndex = 25;
            // 
            // OutputValueTextBox2
            // 
            this.OutputValueTextBox2.Location = new System.Drawing.Point(318, 24);
            this.OutputValueTextBox2.Name = "OutputValueTextBox2";
            this.OutputValueTextBox2.Size = new System.Drawing.Size(96, 25);
            this.OutputValueTextBox2.TabIndex = 26;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(249, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 15);
            this.label3.TabIndex = 27;
            this.label3.Text = "maximum";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(90, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 15);
            this.label4.TabIndex = 28;
            this.label4.Text = "minimum";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.ResetOutputRangeCheckBox);
            this.groupBox4.Controls.Add(this.OutputValueTextBox2);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.OutputValueTextBox1);
            this.groupBox4.Controls.Add(this.SaveAsButton);
            this.groupBox4.ForeColor = System.Drawing.Color.Blue;
            this.groupBox4.Location = new System.Drawing.Point(12, 412);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(582, 70);
            this.groupBox4.TabIndex = 29;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(24, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 15);
            this.label5.TabIndex = 31;
            this.label5.Text = "minimum";
            // 
            // LayerMintextBox1
            // 
            this.LayerMintextBox1.Location = new System.Drawing.Point(93, 27);
            this.LayerMintextBox1.Name = "LayerMintextBox1";
            this.LayerMintextBox1.Size = new System.Drawing.Size(129, 25);
            this.LayerMintextBox1.TabIndex = 30;
            // 
            // LayerMaxtextBox1
            // 
            this.LayerMaxtextBox1.Location = new System.Drawing.Point(93, 61);
            this.LayerMaxtextBox1.Name = "LayerMaxtextBox1";
            this.LayerMaxtextBox1.Size = new System.Drawing.Size(129, 25);
            this.LayerMaxtextBox1.TabIndex = 32;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(24, 64);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 15);
            this.label6.TabIndex = 33;
            this.label6.Text = "maximum";
            // 
            // LayerValuetextBox1
            // 
            this.LayerValuetextBox1.Location = new System.Drawing.Point(27, 126);
            this.LayerValuetextBox1.Name = "LayerValuetextBox1";
            this.LayerValuetextBox1.Size = new System.Drawing.Size(173, 25);
            this.LayerValuetextBox1.TabIndex = 34;
            // 
            // UpdateLayerButton
            // 
            this.UpdateLayerButton.Location = new System.Drawing.Point(27, 192);
            this.UpdateLayerButton.Name = "UpdateLayerButton";
            this.UpdateLayerButton.Size = new System.Drawing.Size(100, 31);
            this.UpdateLayerButton.TabIndex = 36;
            this.UpdateLayerButton.Text = "Update";
            this.UpdateLayerButton.UseVisualStyleBackColor = true;
            this.UpdateLayerButton.Click += new System.EventHandler(this.UpdateLayerButton_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.SaveLayerbutton1);
            this.groupBox5.Controls.Add(this.EnableLayerValueCheckBox1);
            this.groupBox5.Controls.Add(this.LayerMaxtextBox1);
            this.groupBox5.Controls.Add(this.UpdateLayerButton);
            this.groupBox5.Controls.Add(this.LayerMintextBox1);
            this.groupBox5.Controls.Add(this.LayerValuetextBox1);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.ForeColor = System.Drawing.Color.Blue;
            this.groupBox5.Location = new System.Drawing.Point(355, 12);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(239, 267);
            this.groupBox5.TabIndex = 37;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Grid Property";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(112, 62);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(236, 23);
            this.comboBox1.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(6, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 15);
            this.label2.TabIndex = 29;
            this.label2.Text = "Overlap with";
            // 
            // EnableLayerValueCheckBox1
            // 
            this.EnableLayerValueCheckBox1.AutoSize = true;
            this.EnableLayerValueCheckBox1.Location = new System.Drawing.Point(27, 101);
            this.EnableLayerValueCheckBox1.Name = "EnableLayerValueCheckBox1";
            this.EnableLayerValueCheckBox1.Size = new System.Drawing.Size(173, 19);
            this.EnableLayerValueCheckBox1.TabIndex = 37;
            this.EnableLayerValueCheckBox1.Text = "Enable Layer Value";
            this.EnableLayerValueCheckBox1.UseVisualStyleBackColor = true;
            this.EnableLayerValueCheckBox1.CheckedChanged += new System.EventHandler(this.EnableLayerValueCheckBox1_CheckedChanged);
            // 
            // SaveLayerbutton1
            // 
            this.SaveLayerbutton1.Location = new System.Drawing.Point(135, 192);
            this.SaveLayerbutton1.Name = "SaveLayerbutton1";
            this.SaveLayerbutton1.Size = new System.Drawing.Size(98, 31);
            this.SaveLayerbutton1.TabIndex = 38;
            this.SaveLayerbutton1.Text = "Save As";
            this.SaveLayerbutton1.UseVisualStyleBackColor = true;
            this.SaveLayerbutton1.Click += new System.EventHandler(this.SaveLayerbutton1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(6, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 15);
            this.label1.TabIndex = 30;
            this.label1.Text = "background";
            // 
            // GridsOverlapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 560);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.groupBox1);
            this.Name = "GridsOverlapForm";
            this.Text = "GridsOverlapForm";
            this.Load += new System.EventHandler(this.GridsOverlapForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button LoadGridButton;
        private System.Windows.Forms.Button MergeButton;
        private System.Windows.Forms.Button SaveAsButton;
        private System.Windows.Forms.Button DownButton;
        private System.Windows.Forms.Button UpButton;
        private System.Windows.Forms.TextBox FillBackGridValueTextBox;
        private System.Windows.Forms.CheckBox FillBackgroundCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox ResetOutputRangeCheckBox;
        private System.Windows.Forms.TextBox OutputValueTextBox1;
        private System.Windows.Forms.TextBox OutputValueTextBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox LayerMintextBox1;
        private System.Windows.Forms.TextBox LayerMaxtextBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox LayerValuetextBox1;
        private System.Windows.Forms.Button UpdateLayerButton;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox EnableLayerValueCheckBox1;
        private System.Windows.Forms.Button SaveLayerbutton1;
        private System.Windows.Forms.Label label1;
    }
}