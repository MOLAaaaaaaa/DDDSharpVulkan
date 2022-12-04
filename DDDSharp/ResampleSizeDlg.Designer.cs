namespace DDDSharp
{
    partial class ResampleSizeDlg
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
            this.label1 = new System.Windows.Forms.Label();
            this.newXSize = new System.Windows.Forms.TextBox();
            this.newYSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.newZSize = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.originx = new System.Windows.Forms.Label();
            this.originy = new System.Windows.Forms.Label();
            this.originz = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "X Size";
            // 
            // newXSize
            // 
            this.newXSize.Location = new System.Drawing.Point(56, 31);
            this.newXSize.Name = "newXSize";
            this.newXSize.Size = new System.Drawing.Size(100, 20);
            this.newXSize.TabIndex = 1;
            // 
            // newYSize
            // 
            this.newYSize.Location = new System.Drawing.Point(56, 57);
            this.newYSize.Name = "newYSize";
            this.newYSize.Size = new System.Drawing.Size(100, 20);
            this.newYSize.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y Size";
            // 
            // newZSize
            // 
            this.newZSize.Location = new System.Drawing.Point(56, 83);
            this.newZSize.Name = "newZSize";
            this.newZSize.Size = new System.Drawing.Size(100, 20);
            this.newZSize.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Z Size";
            // 
            // originx
            // 
            this.originx.AutoSize = true;
            this.originx.Location = new System.Drawing.Point(162, 34);
            this.originx.Name = "originx";
            this.originx.Size = new System.Drawing.Size(69, 13);
            this.originx.TabIndex = 6;
            this.originx.Text = "original x size";
            // 
            // originy
            // 
            this.originy.AutoSize = true;
            this.originy.Location = new System.Drawing.Point(162, 60);
            this.originy.Name = "originy";
            this.originy.Size = new System.Drawing.Size(69, 13);
            this.originy.TabIndex = 7;
            this.originy.Text = "original x size";
            // 
            // originz
            // 
            this.originz.AutoSize = true;
            this.originz.Location = new System.Drawing.Point(162, 86);
            this.originz.Name = "originz";
            this.originz.Size = new System.Drawing.Size(69, 13);
            this.originz.TabIndex = 8;
            this.originz.Text = "original x size";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(32, 133);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(156, 133);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // ResampleSizeDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 176);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.originz);
            this.Controls.Add(this.originy);
            this.Controls.Add(this.originx);
            this.Controls.Add(this.newZSize);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.newYSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.newXSize);
            this.Controls.Add(this.label1);
            this.Name = "ResampleSizeDlg";
            this.Text = "Resample Size";
            this.Load += new System.EventHandler(this.ResampleSizeDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox newXSize;
        private System.Windows.Forms.TextBox newYSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox newZSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label originx;
        private System.Windows.Forms.Label originy;
        private System.Windows.Forms.Label originz;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}