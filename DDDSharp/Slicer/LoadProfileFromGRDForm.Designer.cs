
namespace DDDSharp
{
    partial class LoadProfileFromGRDForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.GrdInforTextBox = new System.Windows.Forms.TextBox();
            this.LoadGridButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.CANCEL = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.BaselinePosComboBox = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.LoadCoordinateButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.MatchByComboBox = new System.Windows.Forms.ComboBox();
            this.ZComboBox = new System.Windows.Forms.ComboBox();
            this.XComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.YComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.groupBox1);
            this.groupBox3.Controls.Add(this.pictureBox1);
            this.groupBox3.ForeColor = System.Drawing.Color.Blue;
            this.groupBox3.Location = new System.Drawing.Point(7, 14);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Size = new System.Drawing.Size(974, 392);
            this.groupBox3.TabIndex = 20;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Gridded Data";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.GrdInforTextBox);
            this.groupBox1.Controls.Add(this.LoadGridButton);
            this.groupBox1.Location = new System.Drawing.Point(663, 19);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(307, 366);
            this.groupBox1.TabIndex = 45;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "grid data infomation";
            // 
            // GrdInforTextBox
            // 
            this.GrdInforTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GrdInforTextBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.GrdInforTextBox.Location = new System.Drawing.Point(7, 29);
            this.GrdInforTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.GrdInforTextBox.Multiline = true;
            this.GrdInforTextBox.Name = "GrdInforTextBox";
            this.GrdInforTextBox.ReadOnly = true;
            this.GrdInforTextBox.Size = new System.Drawing.Size(293, 294);
            this.GrdInforTextBox.TabIndex = 33;
            // 
            // LoadGridButton
            // 
            this.LoadGridButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadGridButton.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.LoadGridButton.Location = new System.Drawing.Point(7, 320);
            this.LoadGridButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.LoadGridButton.Name = "LoadGridButton";
            this.LoadGridButton.Size = new System.Drawing.Size(111, 29);
            this.LoadGridButton.TabIndex = 31;
            this.LoadGridButton.Text = "Load";
            this.LoadGridButton.UseVisualStyleBackColor = true;
            this.LoadGridButton.Click += new System.EventHandler(this.LoadGrdButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pictureBox1.Location = new System.Drawing.Point(8, 29);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(646, 356);
            this.pictureBox1.TabIndex = 34;
            this.pictureBox1.TabStop = false;
            // 
            // CANCEL
            // 
            this.CANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CANCEL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.CANCEL.Location = new System.Drawing.Point(810, 704);
            this.CANCEL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(171, 49);
            this.CANCEL.TabIndex = 31;
            this.CANCEL.Text = "CANCEL";
            this.CANCEL.UseVisualStyleBackColor = true;
            this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.OK.Location = new System.Drawing.Point(14, 704);
            this.OK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(171, 49);
            this.OK.TabIndex = 30;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.dataGridView1);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.ForeColor = System.Drawing.Color.Blue;
            this.groupBox2.Location = new System.Drawing.Point(14, 414);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(974, 265);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Baseline from Coordinates";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.ColumnHeadersHeight = 29;
            this.dataGridView1.Location = new System.Drawing.Point(7, 29);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 80;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(640, 229);
            this.dataGridView1.TabIndex = 16;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.BaselinePosComboBox);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.LoadCoordinateButton);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.MatchByComboBox);
            this.groupBox4.Controls.Add(this.ZComboBox);
            this.groupBox4.Controls.Add(this.XComboBox);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.YComboBox);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBox4.Location = new System.Drawing.Point(663, 19);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox4.Size = new System.Drawing.Size(305, 239);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Matching";
            // 
            // BaselinePosComboBox
            // 
            this.BaselinePosComboBox.ForeColor = System.Drawing.Color.Black;
            this.BaselinePosComboBox.FormattingEnabled = true;
            this.BaselinePosComboBox.Location = new System.Drawing.Point(84, 167);
            this.BaselinePosComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.BaselinePosComboBox.Name = "BaselinePosComboBox";
            this.BaselinePosComboBox.Size = new System.Drawing.Size(209, 26);
            this.BaselinePosComboBox.TabIndex = 33;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ForeColor = System.Drawing.Color.Black;
            this.label11.Location = new System.Drawing.Point(-1, 173);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(79, 19);
            this.label11.TabIndex = 34;
            this.label11.Text = "PlaceOn";
            // 
            // LoadCoordinateButton
            // 
            this.LoadCoordinateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadCoordinateButton.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.LoadCoordinateButton.Location = new System.Drawing.Point(84, 202);
            this.LoadCoordinateButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.LoadCoordinateButton.Name = "LoadCoordinateButton";
            this.LoadCoordinateButton.Size = new System.Drawing.Size(111, 29);
            this.LoadCoordinateButton.TabIndex = 31;
            this.LoadCoordinateButton.Text = "Load";
            this.LoadCoordinateButton.UseVisualStyleBackColor = true;
            this.LoadCoordinateButton.Click += new System.EventHandler(this.LoadCoordinatesClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label4.Location = new System.Drawing.Point(6, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 19);
            this.label4.TabIndex = 22;
            this.label4.Text = "by";
            // 
            // MatchByComboBox
            // 
            this.MatchByComboBox.ForeColor = System.Drawing.Color.Black;
            this.MatchByComboBox.FormattingEnabled = true;
            this.MatchByComboBox.Location = new System.Drawing.Point(54, 28);
            this.MatchByComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MatchByComboBox.Name = "MatchByComboBox";
            this.MatchByComboBox.Size = new System.Drawing.Size(240, 26);
            this.MatchByComboBox.TabIndex = 21;
            // 
            // ZComboBox
            // 
            this.ZComboBox.ForeColor = System.Drawing.Color.Black;
            this.ZComboBox.FormattingEnabled = true;
            this.ZComboBox.Location = new System.Drawing.Point(54, 132);
            this.ZComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ZComboBox.Name = "ZComboBox";
            this.ZComboBox.Size = new System.Drawing.Size(239, 26);
            this.ZComboBox.TabIndex = 5;
            // 
            // XComboBox
            // 
            this.XComboBox.ForeColor = System.Drawing.Color.Black;
            this.XComboBox.FormattingEnabled = true;
            this.XComboBox.Location = new System.Drawing.Point(54, 61);
            this.XComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.XComboBox.Name = "XComboBox";
            this.XComboBox.Size = new System.Drawing.Size(239, 26);
            this.XComboBox.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(16, 64);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 19);
            this.label5.TabIndex = 2;
            this.label5.Text = "x";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(16, 137);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(19, 19);
            this.label6.TabIndex = 6;
            this.label6.Text = "z";
            // 
            // YComboBox
            // 
            this.YComboBox.ForeColor = System.Drawing.Color.Black;
            this.YComboBox.FormattingEnabled = true;
            this.YComboBox.Location = new System.Drawing.Point(54, 96);
            this.YComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.YComboBox.Name = "YComboBox";
            this.YComboBox.Size = new System.Drawing.Size(239, 26);
            this.YComboBox.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Location = new System.Drawing.Point(16, 101);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(19, 19);
            this.label8.TabIndex = 4;
            this.label8.Text = "y";
            // 
            // LoadProfileFromGRDForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 767);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.groupBox3);
            this.Font = new System.Drawing.Font("SimSun", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "LoadProfileFromGRDForm";
            this.Text = "LoadProfileFromGRDForm";
            this.groupBox3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button LoadGridButton;
        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox GrdInforTextBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button LoadCoordinateButton;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox MatchByComboBox;
        private System.Windows.Forms.ComboBox ZComboBox;
        private System.Windows.Forms.ComboBox XComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox YComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox BaselinePosComboBox;
        private System.Windows.Forms.Label label11;
    }
}