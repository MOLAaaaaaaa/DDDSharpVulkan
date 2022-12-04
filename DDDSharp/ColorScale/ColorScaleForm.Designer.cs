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
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).BeginInit();
            this.SuspendLayout();
            // 
            // OKbutton1
            // 
            this.OKbutton1.Location = new System.Drawing.Point(18, 362);
            this.OKbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.OKbutton1.Name = "OKbutton1";
            this.OKbutton1.Size = new System.Drawing.Size(188, 36);
            this.OKbutton1.TabIndex = 16;
            this.OKbutton1.Text = "OK";
            this.OKbutton1.UseVisualStyleBackColor = true;
            this.OKbutton1.Click += new System.EventHandler(this.OKbutton1_Click);
            // 
            // Cancelbutton1
            // 
            this.Cancelbutton1.Location = new System.Drawing.Point(321, 362);
            this.Cancelbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Cancelbutton1.Name = "Cancelbutton1";
            this.Cancelbutton1.Size = new System.Drawing.Size(188, 36);
            this.Cancelbutton1.TabIndex = 17;
            this.Cancelbutton1.Text = "Cancel";
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
            this.propertyGrid1.Size = new System.Drawing.Size(497, 246);
            this.propertyGrid1.TabIndex = 18;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // LoadFrombutton1
            // 
            this.LoadFrombutton1.Location = new System.Drawing.Point(13, 292);
            this.LoadFrombutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadFrombutton1.Name = "LoadFrombutton1";
            this.LoadFrombutton1.Size = new System.Drawing.Size(61, 27);
            this.LoadFrombutton1.TabIndex = 19;
            this.LoadFrombutton1.Text = "Load";
            this.LoadFrombutton1.UseVisualStyleBackColor = true;
            this.LoadFrombutton1.Click += new System.EventHandler(this.LoadFrombutton1_Click);
            // 
            // ValueDistributionButton
            // 
            this.ValueDistributionButton.Location = new System.Drawing.Point(398, 292);
            this.ValueDistributionButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ValueDistributionButton.Name = "ValueDistributionButton";
            this.ValueDistributionButton.Size = new System.Drawing.Size(111, 27);
            this.ValueDistributionButton.TabIndex = 20;
            this.ValueDistributionButton.Text = "Distribution";
            this.ValueDistributionButton.UseVisualStyleBackColor = true;
            this.ValueDistributionButton.Click += new System.EventHandler(this.ValueDistributionButton_Click);
            // 
            // SaveAsButton
            // 
            this.SaveAsButton.Location = new System.Drawing.Point(91, 292);
            this.SaveAsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SaveAsButton.Name = "SaveAsButton";
            this.SaveAsButton.Size = new System.Drawing.Size(61, 27);
            this.SaveAsButton.TabIndex = 22;
            this.SaveAsButton.Text = "Save";
            this.SaveAsButton.UseVisualStyleBackColor = true;
            this.SaveAsButton.Click += new System.EventHandler(this.SaveAsButton_Click);
            // 
            // Inverse
            // 
            this.Inverse.Location = new System.Drawing.Point(310, 292);
            this.Inverse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Inverse.Name = "Inverse";
            this.Inverse.Size = new System.Drawing.Size(74, 27);
            this.Inverse.TabIndex = 23;
            this.Inverse.Text = "Reverse";
            this.Inverse.UseVisualStyleBackColor = true;
            this.Inverse.Click += new System.EventHandler(this.Inverse_Click);
            // 
            // colorBar
            // 
            this.colorBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colorBar.Location = new System.Drawing.Point(16, 243);
            this.colorBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.colorBar.Name = "colorBar";
            this.colorBar.Size = new System.Drawing.Size(497, 27);
            this.colorBar.TabIndex = 21;
            this.colorBar.TabStop = false;
            // 
            // DefaultButton
            // 
            this.DefaultButton.Location = new System.Drawing.Point(169, 292);
            this.DefaultButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DefaultButton.Name = "DefaultButton";
            this.DefaultButton.Size = new System.Drawing.Size(73, 27);
            this.DefaultButton.TabIndex = 24;
            this.DefaultButton.Text = "Default";
            this.DefaultButton.UseVisualStyleBackColor = true;
            this.DefaultButton.Click += new System.EventHandler(this.DefaultButton_Click);
            // 
            // ColorScaleForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(529, 443);
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
            ((System.ComponentModel.ISupportInitialize)(this.colorBar)).EndInit();
            this.ResumeLayout(false);

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
    }
}