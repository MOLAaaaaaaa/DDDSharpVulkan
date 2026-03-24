namespace DataCollection
{
    partial class ColorScaleForm
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
            this.OKbutton1 = new System.Windows.Forms.Button();
            this.Cancelbutton1 = new System.Windows.Forms.Button();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.LoadFrombutton1 = new System.Windows.Forms.Button();
            this.ValueDistributionButton = new System.Windows.Forms.Button();
            this.SaveAsButton = new System.Windows.Forms.Button();
            this.Inverse = new System.Windows.Forms.Button();
            this.colorBar = new System.Windows.Forms.PictureBox();
            this.DefaultButton = new System.Windows.Forms.Button();
            this.Edit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.AlphaTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // OKbutton1
            // 
            this.OKbutton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKbutton1.Location = new System.Drawing.Point(18, 457);
            this.OKbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.OKbutton1.Name = "OKbutton1";
            this.OKbutton1.Size = new System.Drawing.Size(188, 36);
            this.OKbutton1.TabIndex = 16;
            this.OKbutton1.Text = "&OK";
            this.OKbutton1.UseVisualStyleBackColor = true;
            this.OKbutton1.Click += new System.EventHandler(this.OKbutton1_Click);
            // 
            // Cancelbutton1
            // 
            this.Cancelbutton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancelbutton1.Location = new System.Drawing.Point(527, 457);
            this.Cancelbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Cancelbutton1.Name = "Cancelbutton1";
            this.Cancelbutton1.Size = new System.Drawing.Size(188, 36);
            this.Cancelbutton1.TabIndex = 17;
            this.Cancelbutton1.Text = "&Cancel";
            this.Cancelbutton1.UseVisualStyleBackColor = true;
            this.Cancelbutton1.Click += new System.EventHandler(this.Cancelbutton1_Click);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.LineColor = System.Drawing.SystemColors.ControlDark;
            this.propertyGrid1.Location = new System.Drawing.Point(16, 13);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(699, 243);
            this.propertyGrid1.TabIndex = 18;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // LoadFrombutton1
            // 
            this.LoadFrombutton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadFrombutton1.Location = new System.Drawing.Point(17, 406);
            this.LoadFrombutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadFrombutton1.Name = "LoadFrombutton1";
            this.LoadFrombutton1.Size = new System.Drawing.Size(61, 27);
            this.LoadFrombutton1.TabIndex = 19;
            this.LoadFrombutton1.Text = "&Load";
            this.LoadFrombutton1.UseVisualStyleBackColor = true;
            this.LoadFrombutton1.Click += new System.EventHandler(this.LoadFrombutton1_Click);
            // 
            // ValueDistributionButton
            // 
            this.ValueDistributionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ValueDistributionButton.Location = new System.Drawing.Point(503, 406);
            this.ValueDistributionButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ValueDistributionButton.Name = "ValueDistributionButton";
            this.ValueDistributionButton.Size = new System.Drawing.Size(111, 27);
            this.ValueDistributionButton.TabIndex = 20;
            this.ValueDistributionButton.Text = "&Distribution";
            this.ValueDistributionButton.UseVisualStyleBackColor = true;
            this.ValueDistributionButton.Click += new System.EventHandler(this.ValueDistributionButton_Click);
            // 
            // SaveAsButton
            // 
            this.SaveAsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveAsButton.Location = new System.Drawing.Point(95, 406);
            this.SaveAsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SaveAsButton.Name = "SaveAsButton";
            this.SaveAsButton.Size = new System.Drawing.Size(61, 27);
            this.SaveAsButton.TabIndex = 22;
            this.SaveAsButton.Text = "&Save";
            this.SaveAsButton.UseVisualStyleBackColor = true;
            this.SaveAsButton.Click += new System.EventHandler(this.SaveAsButton_Click);
            // 
            // Inverse
            // 
            this.Inverse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Inverse.Location = new System.Drawing.Point(415, 406);
            this.Inverse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Inverse.Name = "Inverse";
            this.Inverse.Size = new System.Drawing.Size(74, 27);
            this.Inverse.TabIndex = 23;
            this.Inverse.Text = "&Reverse";
            this.Inverse.UseVisualStyleBackColor = true;
            this.Inverse.Click += new System.EventHandler(this.Inverse_Click);
            // 
            // colorBar
            // 
            this.colorBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorBar.Location = new System.Drawing.Point(16, 261);
            this.colorBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.colorBar.Name = "colorBar";
            this.colorBar.Size = new System.Drawing.Size(699, 64);
            this.colorBar.TabIndex = 21;
            this.colorBar.TabStop = false;
            this.colorBar.DoubleClick += new System.EventHandler(this.colorBar_DoubleClick);
            this.colorBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.colorBar_MouseDown);
            // 
            // DefaultButton
            // 
            this.DefaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DefaultButton.Location = new System.Drawing.Point(173, 406);
            this.DefaultButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DefaultButton.Name = "DefaultButton";
            this.DefaultButton.Size = new System.Drawing.Size(73, 27);
            this.DefaultButton.TabIndex = 24;
            this.DefaultButton.Text = "&Default";
            this.DefaultButton.UseVisualStyleBackColor = true;
            this.DefaultButton.Click += new System.EventHandler(this.DefaultButton_Click);
            // 
            // Edit
            // 
            this.Edit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Edit.Location = new System.Drawing.Point(668, 406);
            this.Edit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Edit.Name = "Edit";
            this.Edit.Size = new System.Drawing.Size(51, 27);
            this.Edit.TabIndex = 25;
            this.Edit.Text = "&Edit";
            this.Edit.UseVisualStyleBackColor = true;
            this.Edit.Click += new System.EventHandler(this.Edit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 345);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 15);
            this.label1.TabIndex = 26;
            this.label1.Text = "Alpha";
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(124, 339);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(272, 56);
            this.trackBar1.TabIndex = 27;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // AlphaTextBox
            // 
            this.AlphaTextBox.Location = new System.Drawing.Point(65, 339);
            this.AlphaTextBox.Name = "AlphaTextBox";
            this.AlphaTextBox.ReadOnly = true;
            this.AlphaTextBox.Size = new System.Drawing.Size(53, 25);
            this.AlphaTextBox.TabIndex = 28;
            // 
            // ColorScaleForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(731, 505);
            this.Controls.Add(this.AlphaTextBox);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Edit);
            this.Controls.Add(this.DefaultButton);
            this.Controls.Add(this.Inverse);
            this.Controls.Add(this.SaveAsButton);
            this.Controls.Add(this.colorBar);
            this.Controls.Add(this.ValueDistributionButton);
            this.Controls.Add(this.LoadFrombutton1);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.Cancelbutton1);
            this.Controls.Add(this.OKbutton1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "ColorScaleForm";
            this.Text = "ColorScaleForm";
            this.Load += new System.EventHandler(this.ColorScaleForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ColorScaleForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ColorScaleForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button OKbutton1;
        private System.Windows.Forms.Button Cancelbutton1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button LoadFrombutton1;
        private System.Windows.Forms.Button ValueDistributionButton;
        private System.Windows.Forms.PictureBox colorBar;
        private System.Windows.Forms.Button SaveAsButton;
        private System.Windows.Forms.Button Inverse;
        private System.Windows.Forms.Button DefaultButton;
        private System.Windows.Forms.Button Edit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.TextBox AlphaTextBox;
    }
}