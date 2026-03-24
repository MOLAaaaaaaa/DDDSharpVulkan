namespace DDDSharp
{
    partial class MouseControlForm
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
            this.Cancelbutton2 = new System.Windows.Forms.Button();
            this.Okbutton1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.NormalTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SlowTextBox = new System.Windows.Forms.TextBox();
            this.FastTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancelbutton2
            // 
            this.Cancelbutton2.Location = new System.Drawing.Point(263, 204);
            this.Cancelbutton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Cancelbutton2.Name = "Cancelbutton2";
            this.Cancelbutton2.Size = new System.Drawing.Size(110, 41);
            this.Cancelbutton2.TabIndex = 18;
            this.Cancelbutton2.Text = "Cancel";
            this.Cancelbutton2.UseVisualStyleBackColor = true;
            this.Cancelbutton2.Click += new System.EventHandler(this.Cancelbutton2_Click);
            // 
            // Okbutton1
            // 
            this.Okbutton1.Location = new System.Drawing.Point(39, 204);
            this.Okbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Okbutton1.Name = "Okbutton1";
            this.Okbutton1.Size = new System.Drawing.Size(110, 41);
            this.Okbutton1.TabIndex = 17;
            this.Okbutton1.Text = "OK";
            this.Okbutton1.UseVisualStyleBackColor = true;
            this.Okbutton1.Click += new System.EventHandler(this.Okbutton1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.NormalTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.SlowTextBox);
            this.groupBox1.Controls.Add(this.FastTextBox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Location = new System.Drawing.Point(41, 12);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(332, 158);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Zoom Speed";
            // 
            // NormalTextBox
            // 
            this.NormalTextBox.Location = new System.Drawing.Point(114, 29);
            this.NormalTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.NormalTextBox.Name = "NormalTextBox";
            this.NormalTextBox.Size = new System.Drawing.Size(132, 25);
            this.NormalTextBox.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(53, 35);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Normal";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(69, 117);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 15);
            this.label6.TabIndex = 9;
            this.label6.Text = "Slow";
            // 
            // SlowTextBox
            // 
            this.SlowTextBox.Location = new System.Drawing.Point(114, 111);
            this.SlowTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SlowTextBox.Name = "SlowTextBox";
            this.SlowTextBox.Size = new System.Drawing.Size(132, 25);
            this.SlowTextBox.TabIndex = 8;
            // 
            // FastTextBox
            // 
            this.FastTextBox.Location = new System.Drawing.Point(114, 71);
            this.FastTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.FastTextBox.Name = "FastTextBox";
            this.FastTextBox.Size = new System.Drawing.Size(132, 25);
            this.FastTextBox.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(69, 77);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 15);
            this.label8.TabIndex = 5;
            this.label8.Text = "Fast";
            // 
            // MouseControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 265);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Cancelbutton2);
            this.Controls.Add(this.Okbutton1);
            this.Name = "MouseControlForm";
            this.Text = "MouseControlForm";
            this.Load += new System.EventHandler(this.MouseControlForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Cancelbutton2;
        private System.Windows.Forms.Button Okbutton1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox NormalTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox SlowTextBox;
        private System.Windows.Forms.TextBox FastTextBox;
        private System.Windows.Forms.Label label8;
    }
}