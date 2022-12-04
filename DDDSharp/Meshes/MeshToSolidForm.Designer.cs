namespace DDDSharp
{
    partial class MeshToSolidForm
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
            this.MeshInfotextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.OK = new System.Windows.Forms.Button();
            this.CANCEL = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MeshInfotextBox
            // 
            this.MeshInfotextBox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.MeshInfotextBox.Location = new System.Drawing.Point(47, 85);
            this.MeshInfotextBox.Multiline = true;
            this.MeshInfotextBox.Name = "MeshInfotextBox";
            this.MeshInfotextBox.ReadOnly = true;
            this.MeshInfotextBox.Size = new System.Drawing.Size(364, 71);
            this.MeshInfotextBox.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Thickness";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(129, 47);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(282, 25);
            this.textBox1.TabIndex = 3;
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(47, 195);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(129, 37);
            this.OK.TabIndex = 4;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // CANCEL
            // 
            this.CANCEL.Location = new System.Drawing.Point(282, 195);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(129, 37);
            this.CANCEL.TabIndex = 5;
            this.CANCEL.Text = "CANCEL";
            this.CANCEL.UseVisualStyleBackColor = true;
            this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(44, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(327, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "To create a solid object from mesh grids";
            // 
            // MeshToSolidForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 293);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.MeshInfotextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MeshToSolidForm";
            this.Text = "MeshToSolidForm";
            this.Load += new System.EventHandler(this.MeshToSolidForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MeshInfotextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.Label label1;
    }
}