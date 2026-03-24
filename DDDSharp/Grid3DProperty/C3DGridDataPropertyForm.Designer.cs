namespace DDDSharp
{
    partial class C3DGridDataPropertyForm
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
            this.SaveButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.SlicerButton = new System.Windows.Forms.Button();
            this.Blank = new System.Windows.Forms.Button();
            this.OverlayButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ChangeOverlapColorButon = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ColorBarBox = new System.Windows.Forms.PictureBox();
            this.SelectAllButton = new System.Windows.Forms.Button();
            this.UnselectAll = new System.Windows.Forms.Button();
            this.UpdateShowButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.VolumeTextBox = new System.Windows.Forms.TextBox();
            this.VolumeButton = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ISOValueExtactButton = new System.Windows.Forms.Button();
            this.PropertiesValuesButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorBarBox)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // SaveButton
            // 
            this.SaveButton.ForeColor = System.Drawing.Color.Black;
            this.SaveButton.Location = new System.Drawing.Point(130, 19);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(60, 26);
            this.SaveButton.TabIndex = 5;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // exportButton
            // 
            this.exportButton.ForeColor = System.Drawing.Color.Black;
            this.exportButton.Location = new System.Drawing.Point(196, 19);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(61, 28);
            this.exportButton.TabIndex = 3;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.Export);
            // 
            // SlicerButton
            // 
            this.SlicerButton.ForeColor = System.Drawing.Color.Black;
            this.SlicerButton.Location = new System.Drawing.Point(68, 19);
            this.SlicerButton.Name = "SlicerButton";
            this.SlicerButton.Size = new System.Drawing.Size(56, 28);
            this.SlicerButton.TabIndex = 6;
            this.SlicerButton.Text = "Slicer";
            this.SlicerButton.UseVisualStyleBackColor = true;
            this.SlicerButton.Click += new System.EventHandler(this.SlicerButton_Click);
            // 
            // Blank
            // 
            this.Blank.ForeColor = System.Drawing.Color.Black;
            this.Blank.Location = new System.Drawing.Point(0, 19);
            this.Blank.Name = "Blank";
            this.Blank.Size = new System.Drawing.Size(62, 28);
            this.Blank.TabIndex = 4;
            this.Blank.Text = "Blank";
            this.Blank.UseVisualStyleBackColor = true;
            this.Blank.Click += new System.EventHandler(this.Blank_Click);
            // 
            // OverlayButton
            // 
            this.OverlayButton.Location = new System.Drawing.Point(3, 25);
            this.OverlayButton.Name = "OverlayButton";
            this.OverlayButton.Size = new System.Drawing.Size(83, 28);
            this.OverlayButton.TabIndex = 7;
            this.OverlayButton.Text = "Overlap";
            this.OverlayButton.UseVisualStyleBackColor = true;
            this.OverlayButton.Click += new System.EventHandler(this.OverlayButton_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 32);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(258, 332);
            this.dataGridView1.TabIndex = 8;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridView1_ColumnWidthChanged);
            this.dataGridView1.CurrentCellChanged += new System.EventHandler(this.dataGridView1_CurrentCellChanged);
            this.dataGridView1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dataGridView1_Scroll);
            this.dataGridView1.SizeChanged += new System.EventHandler(this.dataGridView1_SizeChanged);
            this.dataGridView1.DoubleClick += new System.EventHandler(this.dataGridView1_DoubleClick);
            // 
            // ChangeOverlapColorButon
            // 
            this.ChangeOverlapColorButon.Location = new System.Drawing.Point(92, 25);
            this.ChangeOverlapColorButon.Name = "ChangeOverlapColorButon";
            this.ChangeOverlapColorButon.Size = new System.Drawing.Size(150, 28);
            this.ChangeOverlapColorButon.TabIndex = 10;
            this.ChangeOverlapColorButon.Text = "ChangeOverlapColor";
            this.ChangeOverlapColorButon.UseVisualStyleBackColor = true;
            this.ChangeOverlapColorButon.Click += new System.EventHandler(this.ChangeOverlapColorButon_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.Blank);
            this.groupBox1.Controls.Add(this.SaveButton);
            this.groupBox1.Controls.Add(this.exportButton);
            this.groupBox1.Controls.Add(this.SlicerButton);
            this.groupBox1.ForeColor = System.Drawing.Color.Black;
            this.groupBox1.Location = new System.Drawing.Point(3, 404);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(258, 57);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Functions";
            // 
            // ColorBarBox
            // 
            this.ColorBarBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ColorBarBox.Location = new System.Drawing.Point(3, 1);
            this.ColorBarBox.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.ColorBarBox.Name = "ColorBarBox";
            this.ColorBarBox.Size = new System.Drawing.Size(258, 28);
            this.ColorBarBox.TabIndex = 9;
            this.ColorBarBox.TabStop = false;
            this.ColorBarBox.DoubleClick += new System.EventHandler(this.ColorBarBox_DoubleClick);
            // 
            // SelectAllButton
            // 
            this.SelectAllButton.Location = new System.Drawing.Point(5, 370);
            this.SelectAllButton.Name = "SelectAllButton";
            this.SelectAllButton.Size = new System.Drawing.Size(82, 28);
            this.SelectAllButton.TabIndex = 0;
            this.SelectAllButton.Text = "SelectAll";
            this.SelectAllButton.UseVisualStyleBackColor = true;
            this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // UnselectAll
            // 
            this.UnselectAll.Location = new System.Drawing.Point(93, 370);
            this.UnselectAll.Name = "UnselectAll";
            this.UnselectAll.Size = new System.Drawing.Size(92, 28);
            this.UnselectAll.TabIndex = 1;
            this.UnselectAll.Text = "UnselectAll";
            this.UnselectAll.UseVisualStyleBackColor = true;
            this.UnselectAll.Click += new System.EventHandler(this.UnselectAll_Click);
            // 
            // UpdateShowButton
            // 
            this.UpdateShowButton.Location = new System.Drawing.Point(191, 370);
            this.UpdateShowButton.Name = "UpdateShowButton";
            this.UpdateShowButton.Size = new System.Drawing.Size(69, 28);
            this.UpdateShowButton.TabIndex = 2;
            this.UpdateShowButton.Text = "Update";
            this.UpdateShowButton.UseVisualStyleBackColor = true;
            this.UpdateShowButton.Click += new System.EventHandler(this.UpdateShowButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.OverlayButton);
            this.groupBox2.Controls.Add(this.ChangeOverlapColorButon);
            this.groupBox2.Location = new System.Drawing.Point(3, 527);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(258, 66);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Overlap";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.VolumeTextBox);
            this.groupBox3.Controls.Add(this.VolumeButton);
            this.groupBox3.Location = new System.Drawing.Point(3, 467);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(258, 54);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Volume Statics";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(132, 22);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(51, 25);
            this.comboBox1.TabIndex = 10;
            // 
            // VolumeTextBox
            // 
            this.VolumeTextBox.Location = new System.Drawing.Point(2, 22);
            this.VolumeTextBox.Name = "VolumeTextBox";
            this.VolumeTextBox.Size = new System.Drawing.Size(124, 25);
            this.VolumeTextBox.TabIndex = 9;
            // 
            // VolumeButton
            // 
            this.VolumeButton.Location = new System.Drawing.Point(188, 20);
            this.VolumeButton.Name = "VolumeButton";
            this.VolumeButton.Size = new System.Drawing.Size(61, 28);
            this.VolumeButton.TabIndex = 7;
            this.VolumeButton.Text = "Update";
            this.VolumeButton.UseVisualStyleBackColor = true;
            this.VolumeButton.Click += new System.EventHandler(this.VolumeButton_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.ISOValueExtactButton);
            this.groupBox4.Controls.Add(this.PropertiesValuesButton);
            this.groupBox4.Location = new System.Drawing.Point(5, 599);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(258, 66);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Multiple Properties ISO Surfaces";
            // 
            // ISOValueExtactButton
            // 
            this.ISOValueExtactButton.Location = new System.Drawing.Point(128, 25);
            this.ISOValueExtactButton.Name = "ISOValueExtactButton";
            this.ISOValueExtactButton.Size = new System.Drawing.Size(108, 28);
            this.ISOValueExtactButton.TabIndex = 8;
            this.ISOValueExtactButton.Text = "Extract";
            this.ISOValueExtactButton.UseVisualStyleBackColor = true;
            // 
            // PropertiesValuesButton
            // 
            this.PropertiesValuesButton.Location = new System.Drawing.Point(3, 25);
            this.PropertiesValuesButton.Name = "PropertiesValuesButton";
            this.PropertiesValuesButton.Size = new System.Drawing.Size(108, 28);
            this.PropertiesValuesButton.TabIndex = 7;
            this.PropertiesValuesButton.Text = "Properties";
            this.PropertiesValuesButton.UseVisualStyleBackColor = true;
            this.PropertiesValuesButton.Click += new System.EventHandler(this.ClosedValuesButton_Click);
            // 
            // C3DGridDataPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 661);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.ColorBarBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.UpdateShowButton);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.UnselectAll);
            this.Controls.Add(this.SelectAllButton);
            this.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.Name = "C3DGridDataPropertyForm";
            this.Text = "3DGrid Properties";
            this.Load += new System.EventHandler(this.C3DGridDataPropertyForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.C3DGridDataPropertyForm_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ColorBarBox)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button SlicerButton;
        private System.Windows.Forms.Button Blank;
        private System.Windows.Forms.Button OverlayButton;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button ChangeOverlapColorButon;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox ColorBarBox;
        private System.Windows.Forms.Button SelectAllButton;
        private System.Windows.Forms.Button UnselectAll;
        private System.Windows.Forms.Button UpdateShowButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox VolumeTextBox;
        private System.Windows.Forms.Button VolumeButton;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button ISOValueExtactButton;
        private System.Windows.Forms.Button PropertiesValuesButton;
    }
}