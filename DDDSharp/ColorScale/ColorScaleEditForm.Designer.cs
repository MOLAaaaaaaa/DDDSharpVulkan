namespace DDDSharp.ColorScale
{
    partial class ColorScaleEditForm
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
            this.colorBar = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).BeginInit();
            this.SuspendLayout();
            // 
            // colorBar
            // 
            this.colorBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorBar.Location = new System.Drawing.Point(12, 11);
            this.colorBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.colorBar.Name = "colorBar";
            this.colorBar.Size = new System.Drawing.Size(699, 27);
            this.colorBar.TabIndex = 22;
            this.colorBar.TabStop = false;
            // 
            // ColorScaleEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 461);
            this.Controls.Add(this.colorBar);
            this.Name = "ColorScaleEditForm";
            this.Text = "ColorScaleEditForm";
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox colorBar;
    }
}