namespace DDDSharp.Dialogs
{
    partial class RectangleInputDlg
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
            this.CANCEL = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.textBoxX1 = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.textBoxX2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxY2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxY1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelXRangeInfo = new System.Windows.Forms.Label();
            this.labelOfYRange = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CANCEL
            // 
            this.CANCEL.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CANCEL.Location = new System.Drawing.Point(261, 196);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(114, 33);
            this.CANCEL.TabIndex = 10;
            this.CANCEL.Text = "CANCEL";
            this.CANCEL.UseVisualStyleBackColor = true;
            this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OK.Location = new System.Drawing.Point(28, 196);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(114, 33);
            this.OK.TabIndex = 9;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // textBoxX1
            // 
            this.textBoxX1.Location = new System.Drawing.Point(41, 27);
            this.textBoxX1.Name = "textBoxX1";
            this.textBoxX1.Size = new System.Drawing.Size(112, 25);
            this.textBoxX1.TabIndex = 8;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(23, 32);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(15, 15);
            this.Label1.TabIndex = 7;
            this.Label1.Text = "X";
            // 
            // textBoxX2
            // 
            this.textBoxX2.Location = new System.Drawing.Point(204, 27);
            this.textBoxX2.Name = "textBoxX2";
            this.textBoxX2.Size = new System.Drawing.Size(112, 25);
            this.textBoxX2.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(167, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "TO";
            // 
            // textBoxY2
            // 
            this.textBoxY2.Location = new System.Drawing.Point(204, 80);
            this.textBoxY2.Name = "textBoxY2";
            this.textBoxY2.Size = new System.Drawing.Size(112, 25);
            this.textBoxY2.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(167, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 15);
            this.label3.TabIndex = 15;
            this.label3.Text = "TO";
            // 
            // textBoxY1
            // 
            this.textBoxY1.Location = new System.Drawing.Point(41, 80);
            this.textBoxY1.Name = "textBoxY1";
            this.textBoxY1.Size = new System.Drawing.Size(112, 25);
            this.textBoxY1.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 15);
            this.label4.TabIndex = 13;
            this.label4.Text = "Y";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelOfYRange);
            this.groupBox1.Controls.Add(this.labelXRangeInfo);
            this.groupBox1.Controls.Add(this.textBoxY2);
            this.groupBox1.Controls.Add(this.Label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxX1);
            this.groupBox1.Controls.Add(this.textBoxY1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxX2);
            this.groupBox1.Location = new System.Drawing.Point(28, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(347, 136);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Geometry";
            // 
            // labelXRangeInfo
            // 
            this.labelXRangeInfo.AutoSize = true;
            this.labelXRangeInfo.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.labelXRangeInfo.Location = new System.Drawing.Point(40, 59);
            this.labelXRangeInfo.Name = "labelXRangeInfo";
            this.labelXRangeInfo.Size = new System.Drawing.Size(15, 15);
            this.labelXRangeInfo.TabIndex = 18;
            this.labelXRangeInfo.Text = "X";
            // 
            // labelOfYRange
            // 
            this.labelOfYRange.AutoSize = true;
            this.labelOfYRange.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.labelOfYRange.Location = new System.Drawing.Point(40, 108);
            this.labelOfYRange.Name = "labelOfYRange";
            this.labelOfYRange.Size = new System.Drawing.Size(15, 15);
            this.labelOfYRange.TabIndex = 19;
            this.labelOfYRange.Text = "X";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(28, 148);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(101, 19);
            this.checkBox1.TabIndex = 18;
            this.checkBox1.Text = "As pixels";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // RectangleInputDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 241);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.OK);
            this.Name = "RectangleInputDlg";
            this.Text = "RectangleInputDlg";
            this.Load += new System.EventHandler(this.RectangleInputDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.TextBox textBoxX1;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.TextBox textBoxX2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxY2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxY1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelOfYRange;
        private System.Windows.Forms.Label labelXRangeInfo;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}