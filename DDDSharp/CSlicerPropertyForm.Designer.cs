namespace DDDSharp
{
    partial class CSlicerPropertyForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ISOLineCreateButton = new System.Windows.Forms.Button();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.SelectAllbutton = new System.Windows.Forms.Button();
            this.UnselectAllbutton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(243, 246);
            this.dataGridView1.TabIndex = 7;
            // 
            // ISOLineCreateButton
            // 
            this.ISOLineCreateButton.Location = new System.Drawing.Point(169, 264);
            this.ISOLineCreateButton.Name = "ISOLineCreateButton";
            this.ISOLineCreateButton.Size = new System.Drawing.Size(87, 23);
            this.ISOLineCreateButton.TabIndex = 8;
            this.ISOLineCreateButton.Text = "Create contour";
            this.ISOLineCreateButton.UseVisualStyleBackColor = true;
            this.ISOLineCreateButton.Click += new System.EventHandler(this.ISOLineCreateButton_Click);
            // 
            // dataGridView2
            // 
            this.dataGridView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(12, 325);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(243, 224);
            this.dataGridView2.TabIndex = 11;
            // 
            // SelectAllbutton
            // 
            this.SelectAllbutton.Location = new System.Drawing.Point(12, 264);
            this.SelectAllbutton.Name = "SelectAllbutton";
            this.SelectAllbutton.Size = new System.Drawing.Size(64, 23);
            this.SelectAllbutton.TabIndex = 12;
            this.SelectAllbutton.Text = "Select All";
            this.SelectAllbutton.UseVisualStyleBackColor = true;
            this.SelectAllbutton.Click += new System.EventHandler(this.SelectAllbutton_Click);
            // 
            // UnselectAllbutton
            // 
            this.UnselectAllbutton.Location = new System.Drawing.Point(82, 264);
            this.UnselectAllbutton.Name = "UnselectAllbutton";
            this.UnselectAllbutton.Size = new System.Drawing.Size(81, 23);
            this.UnselectAllbutton.TabIndex = 13;
            this.UnselectAllbutton.Text = "Unselect All";
            this.UnselectAllbutton.UseVisualStyleBackColor = true;
            this.UnselectAllbutton.Click += new System.EventHandler(this.UnselectAllbutton_Click);
            // 
            // CSlicerPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(267, 588);
            this.Controls.Add(this.UnselectAllbutton);
            this.Controls.Add(this.SelectAllbutton);
            this.Controls.Add(this.dataGridView2);
            this.Controls.Add(this.ISOLineCreateButton);
            this.Controls.Add(this.dataGridView1);
            this.Name = "CSlicerPropertyForm";
            this.Text = "CSlicerPropertyForm";
            this.Load += new System.EventHandler(this.CSlicerPropertyForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button ISOLineCreateButton;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.Button SelectAllbutton;
        private System.Windows.Forms.Button UnselectAllbutton;
    }
}