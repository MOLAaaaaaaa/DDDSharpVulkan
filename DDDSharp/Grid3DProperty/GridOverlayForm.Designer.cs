namespace DDDSharp
{
    partial class GridOverlayForm
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
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.textGrdFile = new System.Windows.Forms.TextBox();
            this.combineMethodComboBox = new System.Windows.Forms.ComboBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.OKbutton = new System.Windows.Forms.Button();
            this.Cancelbutton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.gridsListcomboBox = new System.Windows.Forms.ComboBox();
            this.channelComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.LoadP32button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(11, 30);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(100, 19);
            this.radioButton1.TabIndex = 3;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "from list";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(11, 60);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(100, 19);
            this.radioButton2.TabIndex = 4;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "from file";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // textGrdFile
            // 
            this.textGrdFile.Location = new System.Drawing.Point(112, 58);
            this.textGrdFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textGrdFile.Name = "textGrdFile";
            this.textGrdFile.Size = new System.Drawing.Size(376, 25);
            this.textGrdFile.TabIndex = 5;
            // 
            // combineMethodComboBox
            // 
            this.combineMethodComboBox.FormattingEnabled = true;
            this.combineMethodComboBox.Location = new System.Drawing.Point(85, 53);
            this.combineMethodComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.combineMethodComboBox.Name = "combineMethodComboBox";
            this.combineMethodComboBox.Size = new System.Drawing.Size(233, 23);
            this.combineMethodComboBox.TabIndex = 6;
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(490, 57);
            this.BrowseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(40, 27);
            this.BrowseButton.TabIndex = 7;
            this.BrowseButton.Text = "...";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // OKbutton
            // 
            this.OKbutton.Location = new System.Drawing.Point(12, 237);
            this.OKbutton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.OKbutton.Name = "OKbutton";
            this.OKbutton.Size = new System.Drawing.Size(89, 27);
            this.OKbutton.TabIndex = 8;
            this.OKbutton.Text = "OK";
            this.OKbutton.UseVisualStyleBackColor = true;
            this.OKbutton.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancelbutton
            // 
            this.Cancelbutton.Location = new System.Drawing.Point(257, 237);
            this.Cancelbutton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Cancelbutton.Name = "Cancelbutton";
            this.Cancelbutton.Size = new System.Drawing.Size(89, 27);
            this.Cancelbutton.TabIndex = 9;
            this.Cancelbutton.Text = "Cancel";
            this.Cancelbutton.UseVisualStyleBackColor = true;
            this.Cancelbutton.Click += new System.EventHandler(this.Cancelbutton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 56);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "combine";
            // 
            // gridsListcomboBox
            // 
            this.gridsListcomboBox.FormattingEnabled = true;
            this.gridsListcomboBox.Location = new System.Drawing.Point(112, 29);
            this.gridsListcomboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gridsListcomboBox.Name = "gridsListcomboBox";
            this.gridsListcomboBox.Size = new System.Drawing.Size(376, 23);
            this.gridsListcomboBox.TabIndex = 11;
            // 
            // channelComboBox
            // 
            this.channelComboBox.FormattingEnabled = true;
            this.channelComboBox.Location = new System.Drawing.Point(85, 24);
            this.channelComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.channelComboBox.Name = "channelComboBox";
            this.channelComboBox.Size = new System.Drawing.Size(233, 23);
            this.channelComboBox.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 28);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 15);
            this.label4.TabIndex = 13;
            this.label4.Text = "Channel";
            // 
            // LoadP32button1
            // 
            this.LoadP32button1.Location = new System.Drawing.Point(578, 21);
            this.LoadP32button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadP32button1.Name = "LoadP32button1";
            this.LoadP32button1.Size = new System.Drawing.Size(89, 27);
            this.LoadP32button1.TabIndex = 14;
            this.LoadP32button1.Text = "P32";
            this.LoadP32button1.UseVisualStyleBackColor = true;
            this.LoadP32button1.Click += new System.EventHandler(this.LoadP32button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.gridsListcomboBox);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.textGrdFile);
            this.groupBox1.Controls.Add(this.BrowseButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(530, 99);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "data from";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.channelComboBox);
            this.groupBox2.Controls.Add(this.combineMethodComboBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 117);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(530, 100);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Overlap";
            // 
            // GridOverlayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 304);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.LoadP32button1);
            this.Controls.Add(this.Cancelbutton);
            this.Controls.Add(this.OKbutton);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "GridOverlayForm";
            this.Text = "GridOverlayForm";
            this.Load += new System.EventHandler(this.GridOverlayForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.TextBox textGrdFile;
        private System.Windows.Forms.ComboBox combineMethodComboBox;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Button OKbutton;
        private System.Windows.Forms.Button Cancelbutton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox gridsListcomboBox;
        private System.Windows.Forms.ComboBox channelComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button LoadP32button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}