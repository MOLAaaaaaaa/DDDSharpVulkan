
namespace DDDSharp
{
    partial class SampleGridSetForm
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
            this.XNumTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.XSpaceTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.YSpaceTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.YNumTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.OK = new System.Windows.Forms.Button();
            this.CANCEL = new System.Windows.Forms.Button();
            this.SpliteCheckBox = new System.Windows.Forms.CheckBox();
            this.SpliteInverseCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.DotSizeTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "XNum";
            // 
            // XNumTextBox
            // 
            this.XNumTextBox.Location = new System.Drawing.Point(51, 35);
            this.XNumTextBox.Name = "XNumTextBox";
            this.XNumTextBox.Size = new System.Drawing.Size(79, 25);
            this.XNumTextBox.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.XSpaceTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.YSpaceTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.YNumTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.XNumTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(31, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(345, 107);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sample Grid";
            // 
            // XSpaceTextBox
            // 
            this.XSpaceTextBox.Location = new System.Drawing.Point(229, 35);
            this.XSpaceTextBox.Name = "XSpaceTextBox";
            this.XSpaceTextBox.Size = new System.Drawing.Size(100, 25);
            this.XSpaceTextBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(169, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "YSpace";
            // 
            // YSpaceTextBox
            // 
            this.YSpaceTextBox.Location = new System.Drawing.Point(229, 66);
            this.YSpaceTextBox.Name = "YSpaceTextBox";
            this.YSpaceTextBox.Size = new System.Drawing.Size(100, 25);
            this.YSpaceTextBox.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(169, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "XSpace";
            // 
            // YNumTextBox
            // 
            this.YNumTextBox.Location = new System.Drawing.Point(51, 66);
            this.YNumTextBox.Name = "YNumTextBox";
            this.YNumTextBox.Size = new System.Drawing.Size(79, 25);
            this.YNumTextBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "YNum";
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(31, 280);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(107, 35);
            this.OK.TabIndex = 3;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // CANCEL
            // 
            this.CANCEL.Location = new System.Drawing.Point(269, 280);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(107, 35);
            this.CANCEL.TabIndex = 4;
            this.CANCEL.Text = "CANCEL";
            this.CANCEL.UseVisualStyleBackColor = true;
            this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
            // 
            // SpliteCheckBox
            // 
            this.SpliteCheckBox.AutoSize = true;
            this.SpliteCheckBox.Location = new System.Drawing.Point(31, 232);
            this.SpliteCheckBox.Name = "SpliteCheckBox";
            this.SpliteCheckBox.Size = new System.Drawing.Size(59, 19);
            this.SpliteCheckBox.TabIndex = 5;
            this.SpliteCheckBox.Text = "切割";
            this.SpliteCheckBox.UseVisualStyleBackColor = true;
            // 
            // SpliteInverseCheckBox
            // 
            this.SpliteInverseCheckBox.AutoSize = true;
            this.SpliteInverseCheckBox.Location = new System.Drawing.Point(119, 232);
            this.SpliteInverseCheckBox.Name = "SpliteInverseCheckBox";
            this.SpliteInverseCheckBox.Size = new System.Drawing.Size(59, 19);
            this.SpliteInverseCheckBox.TabIndex = 6;
            this.SpliteInverseCheckBox.Text = "反向";
            this.SpliteInverseCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.DotSizeTextBox);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(31, 142);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(345, 71);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rendering";
            // 
            // DotSizeTextBox
            // 
            this.DotSizeTextBox.Location = new System.Drawing.Point(77, 33);
            this.DotSizeTextBox.Name = "DotSizeTextBox";
            this.DotSizeTextBox.Size = new System.Drawing.Size(79, 25);
            this.DotSizeTextBox.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 15);
            this.label8.TabIndex = 0;
            this.label8.Text = "DotSize";
            // 
            // SampleGridSetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 336);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.SpliteInverseCheckBox);
            this.Controls.Add(this.SpliteCheckBox);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.groupBox1);
            this.Name = "SampleGridSetForm";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.SampleGridSetForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox XNumTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox XSpaceTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox YSpaceTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox YNumTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.CheckBox SpliteCheckBox;
        private System.Windows.Forms.CheckBox SpliteInverseCheckBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox DotSizeTextBox;
        private System.Windows.Forms.Label label8;
    }
}