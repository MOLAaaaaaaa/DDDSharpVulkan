namespace DDDSharp
{
    partial class BuffersOverlapForm
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.PercentageTextBox1 = new System.Windows.Forms.TextBox();
            this.PercentageGroupBox = new System.Windows.Forms.GroupBox();
            this.ModifyButton = new System.Windows.Forms.Button();
            this.DataFileLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TextBoxZNum = new System.Windows.Forms.TextBox();
            this.TextBoxStepZ = new System.Windows.Forms.TextBox();
            this.TextBoxYNum = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.TextBoxStepY = new System.Windows.Forms.TextBox();
            this.TextBoxZ2 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.TextBoxY2 = new System.Windows.Forms.TextBox();
            this.TextBoxZ1 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TextBoxY1 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TextBoxXNum = new System.Windows.Forms.TextBox();
            this.TextBoxStepX = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TextBoxX2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TextBoxX1 = new System.Windows.Forms.TextBox();
            this.DataInfoTextBox = new System.Windows.Forms.TextBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.OverlapButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.PercentageGroupBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Controls.Add(this.RemoveButton);
            this.groupBox1.Controls.Add(this.AddButton);
            this.groupBox1.Location = new System.Drawing.Point(21, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(275, 368);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Buffer Files";
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
            this.listBox1.Size = new System.Drawing.Size(263, 304);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // RemoveButton
            // 
            this.RemoveButton.Location = new System.Drawing.Point(189, 336);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(80, 26);
            this.RemoveButton.TabIndex = 14;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseVisualStyleBackColor = true;
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(6, 336);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(55, 26);
            this.AddButton.TabIndex = 12;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(18, 408);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(124, 30);
            this.OK.TabIndex = 28;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(622, 408);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(124, 30);
            this.Cancel.TabIndex = 27;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // PercentageTextBox1
            // 
            this.PercentageTextBox1.Location = new System.Drawing.Point(112, 27);
            this.PercentageTextBox1.Name = "PercentageTextBox1";
            this.PercentageTextBox1.Size = new System.Drawing.Size(82, 25);
            this.PercentageTextBox1.TabIndex = 31;
            // 
            // PercentageGroupBox
            // 
            this.PercentageGroupBox.Controls.Add(this.ModifyButton);
            this.PercentageGroupBox.Controls.Add(this.DataFileLabel);
            this.PercentageGroupBox.Controls.Add(this.label2);
            this.PercentageGroupBox.Controls.Add(this.checkBox1);
            this.PercentageGroupBox.Controls.Add(this.PercentageTextBox1);
            this.PercentageGroupBox.Location = new System.Drawing.Point(307, 12);
            this.PercentageGroupBox.Name = "PercentageGroupBox";
            this.PercentageGroupBox.Size = new System.Drawing.Size(439, 71);
            this.PercentageGroupBox.TabIndex = 33;
            this.PercentageGroupBox.TabStop = false;
            this.PercentageGroupBox.Text = "Percentages";
            // 
            // ModifyButton
            // 
            this.ModifyButton.Location = new System.Drawing.Point(221, 24);
            this.ModifyButton.Name = "ModifyButton";
            this.ModifyButton.Size = new System.Drawing.Size(68, 26);
            this.ModifyButton.TabIndex = 15;
            this.ModifyButton.Text = "Modify";
            this.ModifyButton.UseVisualStyleBackColor = true;
            this.ModifyButton.Click += new System.EventHandler(this.ModifyButton_Click);
            // 
            // DataFileLabel
            // 
            this.DataFileLabel.AutoSize = true;
            this.DataFileLabel.Location = new System.Drawing.Point(21, 30);
            this.DataFileLabel.Name = "DataFileLabel";
            this.DataFileLabel.Size = new System.Drawing.Size(87, 15);
            this.DataFileLabel.TabIndex = 35;
            this.DataFileLabel.Text = "Percentage";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(200, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 15);
            this.label2.TabIndex = 34;
            this.label2.Text = "%";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(348, 27);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(85, 19);
            this.checkBox1.TabIndex = 33;
            this.checkBox1.Text = "As Same";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new System.EventHandler(this.checkBox1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TextBoxZNum);
            this.groupBox2.Controls.Add(this.TextBoxStepZ);
            this.groupBox2.Controls.Add(this.TextBoxYNum);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.TextBoxStepY);
            this.groupBox2.Controls.Add(this.TextBoxZ2);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.TextBoxY2);
            this.groupBox2.Controls.Add(this.TextBoxZ1);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.TextBoxY1);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.TextBoxXNum);
            this.groupBox2.Controls.Add(this.TextBoxStepX);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.TextBoxX2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.TextBoxX1);
            this.groupBox2.Location = new System.Drawing.Point(307, 89);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(439, 142);
            this.groupBox2.TabIndex = 38;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Grid";
            // 
            // TextBoxZNum
            // 
            this.TextBoxZNum.Location = new System.Drawing.Point(356, 105);
            this.TextBoxZNum.Name = "TextBoxZNum";
            this.TextBoxZNum.Size = new System.Drawing.Size(67, 25);
            this.TextBoxZNum.TabIndex = 45;
            this.TextBoxZNum.TextChanged += new System.EventHandler(this.UpdateStep);
            // 
            // TextBoxStepZ
            // 
            this.TextBoxStepZ.Location = new System.Drawing.Point(253, 105);
            this.TextBoxStepZ.Name = "TextBoxStepZ";
            this.TextBoxStepZ.Size = new System.Drawing.Size(82, 25);
            this.TextBoxStepZ.TabIndex = 44;
            // 
            // TextBoxYNum
            // 
            this.TextBoxYNum.Location = new System.Drawing.Point(356, 74);
            this.TextBoxYNum.Name = "TextBoxYNum";
            this.TextBoxYNum.Size = new System.Drawing.Size(67, 25);
            this.TextBoxYNum.TabIndex = 48;
            this.TextBoxYNum.TextChanged += new System.EventHandler(this.UpdateStep);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(117, 111);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(23, 15);
            this.label11.TabIndex = 43;
            this.label11.Text = "to";
            // 
            // TextBoxStepY
            // 
            this.TextBoxStepY.Location = new System.Drawing.Point(253, 74);
            this.TextBoxStepY.Name = "TextBoxStepY";
            this.TextBoxStepY.Size = new System.Drawing.Size(82, 25);
            this.TextBoxStepY.TabIndex = 47;
            // 
            // TextBoxZ2
            // 
            this.TextBoxZ2.Location = new System.Drawing.Point(150, 105);
            this.TextBoxZ2.Name = "TextBoxZ2";
            this.TextBoxZ2.Size = new System.Drawing.Size(82, 25);
            this.TextBoxZ2.TabIndex = 42;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(117, 80);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(23, 15);
            this.label9.TabIndex = 46;
            this.label9.Text = "to";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 111);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(15, 15);
            this.label12.TabIndex = 41;
            this.label12.Text = "Z";
            // 
            // TextBoxY2
            // 
            this.TextBoxY2.Location = new System.Drawing.Point(150, 74);
            this.TextBoxY2.Name = "TextBoxY2";
            this.TextBoxY2.Size = new System.Drawing.Size(82, 25);
            this.TextBoxY2.TabIndex = 45;
            // 
            // TextBoxZ1
            // 
            this.TextBoxZ1.Location = new System.Drawing.Point(28, 105);
            this.TextBoxZ1.Name = "TextBoxZ1";
            this.TextBoxZ1.Size = new System.Drawing.Size(82, 25);
            this.TextBoxZ1.TabIndex = 40;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 80);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(15, 15);
            this.label10.TabIndex = 44;
            this.label10.Text = "Y";
            // 
            // TextBoxY1
            // 
            this.TextBoxY1.Location = new System.Drawing.Point(28, 74);
            this.TextBoxY1.Name = "TextBoxY1";
            this.TextBoxY1.Size = new System.Drawing.Size(82, 25);
            this.TextBoxY1.TabIndex = 43;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(353, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 15);
            this.label8.TabIndex = 42;
            this.label8.Text = "No#";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(257, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 15);
            this.label7.TabIndex = 41;
            this.label7.Text = "space";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(152, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 15);
            this.label6.TabIndex = 40;
            this.label6.Text = "maximum";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 39;
            this.label1.Text = "minimum";
            // 
            // TextBoxXNum
            // 
            this.TextBoxXNum.Location = new System.Drawing.Point(356, 43);
            this.TextBoxXNum.Name = "TextBoxXNum";
            this.TextBoxXNum.Size = new System.Drawing.Size(67, 25);
            this.TextBoxXNum.TabIndex = 39;
            this.TextBoxXNum.TextChanged += new System.EventHandler(this.UpdateStep);
            // 
            // TextBoxStepX
            // 
            this.TextBoxStepX.Location = new System.Drawing.Point(253, 43);
            this.TextBoxStepX.Name = "TextBoxStepX";
            this.TextBoxStepX.Size = new System.Drawing.Size(82, 25);
            this.TextBoxStepX.TabIndex = 38;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(117, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 15);
            this.label5.TabIndex = 37;
            this.label5.Text = "to";
            // 
            // TextBoxX2
            // 
            this.TextBoxX2.Location = new System.Drawing.Point(150, 43);
            this.TextBoxX2.Name = "TextBoxX2";
            this.TextBoxX2.Size = new System.Drawing.Size(82, 25);
            this.TextBoxX2.TabIndex = 36;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 15);
            this.label3.TabIndex = 35;
            this.label3.Text = "X";
            // 
            // TextBoxX1
            // 
            this.TextBoxX1.Location = new System.Drawing.Point(28, 43);
            this.TextBoxX1.Name = "TextBoxX1";
            this.TextBoxX1.Size = new System.Drawing.Size(82, 25);
            this.TextBoxX1.TabIndex = 31;
            // 
            // DataInfoTextBox
            // 
            this.DataInfoTextBox.BackColor = System.Drawing.Color.WhiteSmoke;
            this.DataInfoTextBox.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.DataInfoTextBox.Location = new System.Drawing.Point(307, 237);
            this.DataInfoTextBox.Multiline = true;
            this.DataInfoTextBox.Name = "DataInfoTextBox";
            this.DataInfoTextBox.Size = new System.Drawing.Size(439, 104);
            this.DataInfoTextBox.TabIndex = 46;
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(622, 344);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(124, 30);
            this.ExportButton.TabIndex = 47;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // OverlapButton
            // 
            this.OverlapButton.Location = new System.Drawing.Point(307, 347);
            this.OverlapButton.Name = "OverlapButton";
            this.OverlapButton.Size = new System.Drawing.Size(124, 30);
            this.OverlapButton.TabIndex = 48;
            this.OverlapButton.Text = "Do Overlap";
            this.OverlapButton.UseVisualStyleBackColor = true;
            this.OverlapButton.Click += new System.EventHandler(this.OverlapButton_Click);
            // 
            // BuffersOverlapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 450);
            this.Controls.Add(this.OverlapButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.DataInfoTextBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.PercentageGroupBox);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.groupBox1);
            this.Name = "BuffersOverlapForm";
            this.Text = "BuffersOverlapForm";
            this.Load += new System.EventHandler(this.BuffersOverlapForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.PercentageGroupBox.ResumeLayout(false);
            this.PercentageGroupBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.TextBox PercentageTextBox1;
        private System.Windows.Forms.GroupBox PercentageGroupBox;
        private System.Windows.Forms.Button ModifyButton;
        private System.Windows.Forms.Label DataFileLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox TextBoxZNum;
        private System.Windows.Forms.TextBox TextBoxStepZ;
        private System.Windows.Forms.TextBox TextBoxYNum;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox TextBoxStepY;
        private System.Windows.Forms.TextBox TextBoxZ2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox TextBoxY2;
        private System.Windows.Forms.TextBox TextBoxZ1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TextBoxY1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TextBoxXNum;
        private System.Windows.Forms.TextBox TextBoxStepX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TextBoxX2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextBoxX1;
        private System.Windows.Forms.TextBox DataInfoTextBox;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button OverlapButton;
    }
}