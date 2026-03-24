namespace DDDSharp
{
    partial class Grid3DTransformForm
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
            this.OK = new System.Windows.Forms.Button();
            this.CANCEL = new System.Windows.Forms.Button();
            this.ApplyButton1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.VTextbox2 = new System.Windows.Forms.TextBox();
            this.ZtextBox2 = new System.Windows.Forms.TextBox();
            this.VtextBox1 = new System.Windows.Forms.TextBox();
            this.VCheck = new System.Windows.Forms.CheckBox();
            this.ZtextBox1 = new System.Windows.Forms.TextBox();
            this.ZCheck = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.YtextBox2 = new System.Windows.Forms.TextBox();
            this.YtextBox1 = new System.Windows.Forms.TextBox();
            this.YCheck = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.XtextBox2 = new System.Windows.Forms.TextBox();
            this.XtextBox1 = new System.Windows.Forms.TextBox();
            this.XCheck = new System.Windows.Forms.CheckBox();
            this.ApplyButton2 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelV = new System.Windows.Forms.Label();
            this.labelZ = new System.Windows.Forms.Label();
            this.labelY = new System.Windows.Forms.Label();
            this.labelX = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OK.Location = new System.Drawing.Point(12, 666);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(122, 42);
            this.OK.TabIndex = 1;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // CANCEL
            // 
            this.CANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CANCEL.Location = new System.Drawing.Point(331, 666);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(122, 42);
            this.CANCEL.TabIndex = 2;
            this.CANCEL.Text = "Cancel";
            this.CANCEL.UseVisualStyleBackColor = true;
            this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
            // 
            // ApplyButton1
            // 
            this.ApplyButton1.Location = new System.Drawing.Point(50, 60);
            this.ApplyButton1.Name = "ApplyButton1";
            this.ApplyButton1.Size = new System.Drawing.Size(96, 26);
            this.ApplyButton1.TabIndex = 4;
            this.ApplyButton1.Text = "aplly";
            this.ApplyButton1.UseVisualStyleBackColor = true;
            this.ApplyButton1.Click += new System.EventHandler(this.ApplyButton1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.ApplyButton1);
            this.groupBox1.Location = new System.Drawing.Point(689, 154);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(159, 102);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Flip";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(9, 27);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(137, 25);
            this.comboBox1.TabIndex = 5;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.VTextbox2);
            this.groupBox2.Controls.Add(this.ZtextBox2);
            this.groupBox2.Controls.Add(this.VtextBox1);
            this.groupBox2.Controls.Add(this.VCheck);
            this.groupBox2.Controls.Add(this.ZtextBox1);
            this.groupBox2.Controls.Add(this.ZCheck);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.YtextBox2);
            this.groupBox2.Controls.Add(this.YtextBox1);
            this.groupBox2.Controls.Add(this.YCheck);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.XtextBox2);
            this.groupBox2.Controls.Add(this.XtextBox1);
            this.groupBox2.Controls.Add(this.XCheck);
            this.groupBox2.Controls.Add(this.ApplyButton2);
            this.groupBox2.Location = new System.Drawing.Point(689, 263);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(159, 358);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Transform To";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 264);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 17);
            this.label4.TabIndex = 16;
            this.label4.Text = "to";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 197);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 17);
            this.label3.TabIndex = 20;
            this.label3.Text = "to";
            // 
            // VTextbox2
            // 
            this.VTextbox2.Location = new System.Drawing.Point(50, 257);
            this.VTextbox2.Name = "VTextbox2";
            this.VTextbox2.Size = new System.Drawing.Size(100, 25);
            this.VTextbox2.TabIndex = 15;
            // 
            // ZtextBox2
            // 
            this.ZtextBox2.Location = new System.Drawing.Point(50, 190);
            this.ZtextBox2.Name = "ZtextBox2";
            this.ZtextBox2.Size = new System.Drawing.Size(100, 25);
            this.ZtextBox2.TabIndex = 19;
            // 
            // VtextBox1
            // 
            this.VtextBox1.Location = new System.Drawing.Point(50, 226);
            this.VtextBox1.Name = "VtextBox1";
            this.VtextBox1.Size = new System.Drawing.Size(100, 25);
            this.VtextBox1.TabIndex = 14;
            // 
            // VCheck
            // 
            this.VCheck.AutoSize = true;
            this.VCheck.Location = new System.Drawing.Point(6, 228);
            this.VCheck.Name = "VCheck";
            this.VCheck.Size = new System.Drawing.Size(41, 21);
            this.VCheck.TabIndex = 13;
            this.VCheck.Text = "V";
            this.VCheck.UseVisualStyleBackColor = true;
            // 
            // ZtextBox1
            // 
            this.ZtextBox1.Location = new System.Drawing.Point(50, 159);
            this.ZtextBox1.Name = "ZtextBox1";
            this.ZtextBox1.Size = new System.Drawing.Size(100, 25);
            this.ZtextBox1.TabIndex = 18;
            // 
            // ZCheck
            // 
            this.ZCheck.AutoSize = true;
            this.ZCheck.Location = new System.Drawing.Point(6, 161);
            this.ZCheck.Name = "ZCheck";
            this.ZCheck.Size = new System.Drawing.Size(39, 21);
            this.ZCheck.TabIndex = 17;
            this.ZCheck.Text = "Z";
            this.ZCheck.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(19, 17);
            this.label2.TabIndex = 16;
            this.label2.Text = "to";
            // 
            // YtextBox2
            // 
            this.YtextBox2.Location = new System.Drawing.Point(50, 124);
            this.YtextBox2.Name = "YtextBox2";
            this.YtextBox2.Size = new System.Drawing.Size(100, 25);
            this.YtextBox2.TabIndex = 15;
            // 
            // YtextBox1
            // 
            this.YtextBox1.Location = new System.Drawing.Point(50, 92);
            this.YtextBox1.Name = "YtextBox1";
            this.YtextBox1.Size = new System.Drawing.Size(100, 25);
            this.YtextBox1.TabIndex = 14;
            // 
            // YCheck
            // 
            this.YCheck.AutoSize = true;
            this.YCheck.Location = new System.Drawing.Point(6, 94);
            this.YCheck.Name = "YCheck";
            this.YCheck.Size = new System.Drawing.Size(41, 21);
            this.YCheck.TabIndex = 13;
            this.YCheck.Text = "Y";
            this.YCheck.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(19, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "to";
            // 
            // XtextBox2
            // 
            this.XtextBox2.Location = new System.Drawing.Point(50, 57);
            this.XtextBox2.Name = "XtextBox2";
            this.XtextBox2.Size = new System.Drawing.Size(100, 25);
            this.XtextBox2.TabIndex = 11;
            // 
            // XtextBox1
            // 
            this.XtextBox1.Location = new System.Drawing.Point(50, 25);
            this.XtextBox1.Name = "XtextBox1";
            this.XtextBox1.Size = new System.Drawing.Size(100, 25);
            this.XtextBox1.TabIndex = 10;
            // 
            // XCheck
            // 
            this.XCheck.AutoSize = true;
            this.XCheck.Location = new System.Drawing.Point(6, 27);
            this.XCheck.Name = "XCheck";
            this.XCheck.Size = new System.Drawing.Size(37, 21);
            this.XCheck.TabIndex = 6;
            this.XCheck.Text = "x";
            this.XCheck.UseVisualStyleBackColor = true;
            // 
            // ApplyButton2
            // 
            this.ApplyButton2.Location = new System.Drawing.Point(50, 308);
            this.ApplyButton2.Name = "ApplyButton2";
            this.ApplyButton2.Size = new System.Drawing.Size(96, 26);
            this.ApplyButton2.TabIndex = 4;
            this.ApplyButton2.Text = "aplly";
            this.ApplyButton2.UseVisualStyleBackColor = true;
            this.ApplyButton2.Click += new System.EventHandler(this.ApplyButton2_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.labelV);
            this.groupBox3.Controls.Add(this.labelZ);
            this.groupBox3.Controls.Add(this.labelY);
            this.groupBox3.Controls.Add(this.labelX);
            this.groupBox3.Location = new System.Drawing.Point(689, 14);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(159, 134);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Grid Informarion";
            // 
            // labelV
            // 
            this.labelV.AutoSize = true;
            this.labelV.Location = new System.Drawing.Point(6, 113);
            this.labelV.Name = "labelV";
            this.labelV.Size = new System.Drawing.Size(19, 17);
            this.labelV.TabIndex = 18;
            this.labelV.Text = "V";
            // 
            // labelZ
            // 
            this.labelZ.AutoSize = true;
            this.labelZ.Location = new System.Drawing.Point(6, 83);
            this.labelZ.Name = "labelZ";
            this.labelZ.Size = new System.Drawing.Size(17, 17);
            this.labelZ.TabIndex = 19;
            this.labelZ.Text = "Z";
            // 
            // labelY
            // 
            this.labelY.AutoSize = true;
            this.labelY.Location = new System.Drawing.Point(6, 54);
            this.labelY.Name = "labelY";
            this.labelY.Size = new System.Drawing.Size(19, 17);
            this.labelY.TabIndex = 18;
            this.labelY.Text = "Y";
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(6, 24);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(18, 17);
            this.labelX.TabIndex = 17;
            this.labelX.Text = "X";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(12, 14);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(654, 607);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // Grid3DTransformForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 722);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.pictureBox1);
            this.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Grid3DTransformForm";
            this.Text = "Grid3DTransformForm";
            this.Load += new System.EventHandler(this.Grid3DTransformForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.Button ApplyButton1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ZtextBox2;
        private System.Windows.Forms.TextBox ZtextBox1;
        private System.Windows.Forms.CheckBox ZCheck;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox YtextBox2;
        private System.Windows.Forms.TextBox YtextBox1;
        private System.Windows.Forms.CheckBox YCheck;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox XtextBox2;
        private System.Windows.Forms.TextBox XtextBox1;
        private System.Windows.Forms.CheckBox XCheck;
        private System.Windows.Forms.Button ApplyButton2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox VTextbox2;
        private System.Windows.Forms.TextBox VtextBox1;
        private System.Windows.Forms.CheckBox VCheck;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelV;
        private System.Windows.Forms.Label labelZ;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}