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
            this.IntersectionLineButton = new System.Windows.Forms.Button();
            this.ColorBarBox = new System.Windows.Forms.PictureBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColorBarBox)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 26);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(294, 314);
            this.dataGridView1.TabIndex = 7;
            // 
            // ISOLineCreateButton
            // 
            this.ISOLineCreateButton.Location = new System.Drawing.Point(147, 346);
            this.ISOLineCreateButton.Name = "ISOLineCreateButton";
            this.ISOLineCreateButton.Size = new System.Drawing.Size(76, 31);
            this.ISOLineCreateButton.TabIndex = 8;
            this.ISOLineCreateButton.Text = "Create Line";
            this.ISOLineCreateButton.UseVisualStyleBackColor = true;
            this.ISOLineCreateButton.Click += new System.EventHandler(this.ISOLineCreateButton_Click);
            // 
            // dataGridView2
            // 
            this.dataGridView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(3, 383);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.RowHeadersWidth = 51;
            this.dataGridView2.Size = new System.Drawing.Size(294, 293);
            this.dataGridView2.TabIndex = 11;
            // 
            // SelectAllbutton
            // 
            this.SelectAllbutton.Location = new System.Drawing.Point(2, 346);
            this.SelectAllbutton.Name = "SelectAllbutton";
            this.SelectAllbutton.Size = new System.Drawing.Size(66, 31);
            this.SelectAllbutton.TabIndex = 12;
            this.SelectAllbutton.Text = "Select All";
            this.SelectAllbutton.UseVisualStyleBackColor = true;
            this.SelectAllbutton.Click += new System.EventHandler(this.SelectAllbutton_Click);
            // 
            // UnselectAllbutton
            // 
            this.UnselectAllbutton.Location = new System.Drawing.Point(74, 346);
            this.UnselectAllbutton.Name = "UnselectAllbutton";
            this.UnselectAllbutton.Size = new System.Drawing.Size(67, 31);
            this.UnselectAllbutton.TabIndex = 13;
            this.UnselectAllbutton.Text = "Unselect";
            this.UnselectAllbutton.UseVisualStyleBackColor = true;
            this.UnselectAllbutton.Click += new System.EventHandler(this.UnselectAllbutton_Click);
            // 
            // IntersectionLineButton
            // 
            this.IntersectionLineButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IntersectionLineButton.Location = new System.Drawing.Point(119, 682);
            this.IntersectionLineButton.Name = "IntersectionLineButton";
            this.IntersectionLineButton.Size = new System.Drawing.Size(158, 31);
            this.IntersectionLineButton.TabIndex = 14;
            this.IntersectionLineButton.Text = "Create Intersection Line";
            this.IntersectionLineButton.UseVisualStyleBackColor = true;
            this.IntersectionLineButton.Click += new System.EventHandler(this.IntersectionLineButton_Click);
            // 
            // ColorBarBox
            // 
            this.ColorBarBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ColorBarBox.Location = new System.Drawing.Point(3, 3);
            this.ColorBarBox.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.ColorBarBox.Name = "ColorBarBox";
            this.ColorBarBox.Size = new System.Drawing.Size(294, 17);
            this.ColorBarBox.TabIndex = 16;
            this.ColorBarBox.TabStop = false;
            this.ColorBarBox.DoubleClick += new System.EventHandler(this.ColorBarBox_DoubleClick);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(229, 353);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 19);
            this.checkBox1.TabIndex = 17;
            this.checkBox1.Text = "As New";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // CSlicerPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 768);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.ColorBarBox);
            this.Controls.Add(this.IntersectionLineButton);
            this.Controls.Add(this.UnselectAllbutton);
            this.Controls.Add(this.SelectAllbutton);
            this.Controls.Add(this.dataGridView2);
            this.Controls.Add(this.ISOLineCreateButton);
            this.Controls.Add(this.dataGridView1);
            this.Font = new System.Drawing.Font("Times New Roman", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CSlicerPropertyForm";
            this.Text = "Mesh Property";
            this.Load += new System.EventHandler(this.CSlicerPropertyForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CSlicerPropertyForm_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColorBarBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button ISOLineCreateButton;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.Button SelectAllbutton;
        private System.Windows.Forms.Button UnselectAllbutton;
        private System.Windows.Forms.Button IntersectionLineButton;
        private System.Windows.Forms.PictureBox ColorBarBox;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}