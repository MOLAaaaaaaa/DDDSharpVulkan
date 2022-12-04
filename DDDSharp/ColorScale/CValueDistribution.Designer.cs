namespace DataCollection
{
    partial class CValueDistribution
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.MinValue_textBox = new System.Windows.Forms.TextBox();
            this.MaxValue_textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AutoChooseButton = new System.Windows.Forms.Button();
            this.FilterValue_textBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox1.Location = new System.Drawing.Point(9, 10);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(605, 259);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.ClientSizeChanged += new System.EventHandler(this.pictureBox1_ClientSizeChanged);
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKButton.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.OKButton.Location = new System.Drawing.Point(15, 342);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(164, 42);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = false;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.CancelButton.Location = new System.Drawing.Point(437, 342);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(164, 42);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = false;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "minimum";
            // 
            // MinValue_textBox
            // 
            this.MinValue_textBox.Location = new System.Drawing.Point(72, 8);
            this.MinValue_textBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinValue_textBox.Name = "MinValue_textBox";
            this.MinValue_textBox.Size = new System.Drawing.Size(89, 25);
            this.MinValue_textBox.TabIndex = 4;
            // 
            // MaxValue_textBox
            // 
            this.MaxValue_textBox.Location = new System.Drawing.Point(251, 8);
            this.MaxValue_textBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaxValue_textBox.Name = "MaxValue_textBox";
            this.MaxValue_textBox.Size = new System.Drawing.Size(89, 25);
            this.MaxValue_textBox.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(184, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "maximum";
            // 
            // AutoChooseButton
            // 
            this.AutoChooseButton.Location = new System.Drawing.Point(524, 8);
            this.AutoChooseButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AutoChooseButton.Name = "AutoChooseButton";
            this.AutoChooseButton.Size = new System.Drawing.Size(69, 27);
            this.AutoChooseButton.TabIndex = 7;
            this.AutoChooseButton.Text = "Choose";
            this.AutoChooseButton.UseVisualStyleBackColor = true;
            this.AutoChooseButton.Click += new System.EventHandler(this.AutoChooseButton_Click);
            // 
            // FilterValue_textBox
            // 
            this.FilterValue_textBox.Location = new System.Drawing.Point(420, 8);
            this.FilterValue_textBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.FilterValue_textBox.Name = "FilterValue_textBox";
            this.FilterValue_textBox.Size = new System.Drawing.Size(89, 25);
            this.FilterValue_textBox.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(359, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Filter";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.AutoChooseButton);
            this.panel1.Controls.Add(this.FilterValue_textBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.MinValue_textBox);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.MaxValue_textBox);
            this.panel1.Location = new System.Drawing.Point(9, 274);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(605, 45);
            this.panel1.TabIndex = 10;
            // 
            // CValueDistribution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 385);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.pictureBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "CValueDistribution";
            this.Text = "CValueDistribution";
            this.Load += new System.EventHandler(this.CValueDistribution_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button OKButton;
#pragma warning disable CS0108 // '“CValueDistribution.CancelButton”隐藏继承的成员“Form.CancelButton”。如果是有意隐藏，请使用关键字 new。
        private System.Windows.Forms.Button CancelButton;
#pragma warning restore CS0108 // '“CValueDistribution.CancelButton”隐藏继承的成员“Form.CancelButton”。如果是有意隐藏，请使用关键字 new。
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox MinValue_textBox;
        private System.Windows.Forms.TextBox MaxValue_textBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button AutoChooseButton;
        private System.Windows.Forms.TextBox FilterValue_textBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
    }
}