namespace DDDSharp
{
    partial class C3DLinePropertyForm
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
            this.buttonCreatePolygon = new System.Windows.Forms.Button();
            this.Radiu = new System.Windows.Forms.Label();
            this.textRadiuBox = new System.Windows.Forms.TextBox();
            this.textBoxLength = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCreatePolygon
            // 
            this.buttonCreatePolygon.Location = new System.Drawing.Point(20, 78);
            this.buttonCreatePolygon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCreatePolygon.Name = "buttonCreatePolygon";
            this.buttonCreatePolygon.Size = new System.Drawing.Size(183, 27);
            this.buttonCreatePolygon.TabIndex = 0;
            this.buttonCreatePolygon.Text = "Create Polygon";
            this.buttonCreatePolygon.UseVisualStyleBackColor = true;
            this.buttonCreatePolygon.Click += new System.EventHandler(this.buttonCreatePolygon_Click);
            // 
            // Radiu
            // 
            this.Radiu.AutoSize = true;
            this.Radiu.Location = new System.Drawing.Point(17, 52);
            this.Radiu.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Radiu.Name = "Radiu";
            this.Radiu.Size = new System.Drawing.Size(47, 15);
            this.Radiu.TabIndex = 1;
            this.Radiu.Text = "Radiu";
            // 
            // textRadiuBox
            // 
            this.textRadiuBox.Location = new System.Drawing.Point(71, 47);
            this.textRadiuBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textRadiuBox.Name = "textRadiuBox";
            this.textRadiuBox.Size = new System.Drawing.Size(132, 25);
            this.textRadiuBox.TabIndex = 2;
            // 
            // textBoxLength
            // 
            this.textBoxLength.Location = new System.Drawing.Point(71, 17);
            this.textBoxLength.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxLength.Name = "textBoxLength";
            this.textBoxLength.ReadOnly = true;
            this.textBoxLength.Size = new System.Drawing.Size(132, 25);
            this.textBoxLength.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Length";
            // 
            // C3DLinePropertyForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(285, 445);
            this.Controls.Add(this.textBoxLength);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textRadiuBox);
            this.Controls.Add(this.Radiu);
            this.Controls.Add(this.buttonCreatePolygon);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "C3DLinePropertyForm";
            this.Text = "C3DLinePropertyForm";
            this.Load += new System.EventHandler(this.C3DLinePropertyForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCreatePolygon;
        private System.Windows.Forms.Label Radiu;
        private System.Windows.Forms.TextBox textRadiuBox;
        private System.Windows.Forms.TextBox textBoxLength;
        private System.Windows.Forms.Label label1;
    }
}