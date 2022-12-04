namespace DDDSharp
{
    partial class CBoreholesPropertyForm
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
            this.UnselectAllbutton = new System.Windows.Forms.Button();
            this.SelectAllbutton = new System.Windows.Forms.Button();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // UnselectAllbutton
            // 
            this.UnselectAllbutton.Location = new System.Drawing.Point(94, 697);
            this.UnselectAllbutton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.UnselectAllbutton.Name = "UnselectAllbutton";
            this.UnselectAllbutton.Size = new System.Drawing.Size(108, 30);
            this.UnselectAllbutton.TabIndex = 17;
            this.UnselectAllbutton.Text = "Unselect All";
            this.UnselectAllbutton.UseVisualStyleBackColor = true;
            this.UnselectAllbutton.Click += new System.EventHandler(this.UnselectAllbutton_Click);
            // 
            // SelectAllbutton
            // 
            this.SelectAllbutton.Location = new System.Drawing.Point(2, 697);
            this.SelectAllbutton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectAllbutton.Name = "SelectAllbutton";
            this.SelectAllbutton.Size = new System.Drawing.Size(85, 30);
            this.SelectAllbutton.TabIndex = 16;
            this.SelectAllbutton.Text = "Select All";
            this.SelectAllbutton.UseVisualStyleBackColor = true;
            this.SelectAllbutton.Click += new System.EventHandler(this.SelectAllbutton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.Location = new System.Drawing.Point(211, 697);
            this.UpdateButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(73, 30);
            this.UpdateButton.TabIndex = 15;
            this.UpdateButton.Text = "Update";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(2, 4);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(281, 691);
            this.dataGridView1.TabIndex = 14;
            // 
            // CBoreholesPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 739);
            this.Controls.Add(this.UnselectAllbutton);
            this.Controls.Add(this.SelectAllbutton);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.dataGridView1);
            this.Font = new System.Drawing.Font("Times New Roman", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "CBoreholesPropertyForm";
            this.Text = "Borehole Properties";
            this.Load += new System.EventHandler(this.CBoreholesPropertyForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button UnselectAllbutton;
        private System.Windows.Forms.Button SelectAllbutton;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}