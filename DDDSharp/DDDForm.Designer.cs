namespace DDDSharp
{
    partial class DDDForm
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
            this.SuspendLayout();
            // 
            // DDDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 348);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DDDForm";
            this.Text = "Rendering";
            this.Load += new System.EventHandler(this.DDDForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DDDForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.DDDForm_KeyUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DDDForm_PreviewKeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        // private OpenGL.GlControl glControl1;
    }
}