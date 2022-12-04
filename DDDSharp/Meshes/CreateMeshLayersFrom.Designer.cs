
namespace DDDSharp
{
    partial class CreateMeshLayersFrom
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.MoveDownBotton = new System.Windows.Forms.Button();
            this.MoveUpButton = new System.Windows.Forms.Button();
            this.RemoveButton2 = new System.Windows.Forms.Button();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.AddButton2 = new System.Windows.Forms.Button();
            this.CANCELBUTTON = new System.Windows.Forms.Button();
            this.OKBUTTON = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.propertyGrid1);
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.MoveDownBotton);
            this.groupBox1.Controls.Add(this.MoveUpButton);
            this.groupBox1.Controls.Add(this.RemoveButton2);
            this.groupBox1.Controls.Add(this.listBox2);
            this.groupBox1.Controls.Add(this.AddButton2);
            this.groupBox1.ForeColor = System.Drawing.Color.Sienna;
            this.groupBox1.Location = new System.Drawing.Point(13, 12);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(986, 476);
            this.groupBox1.TabIndex = 58;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Meshed Layers--from top to bottom";
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.LineColor = System.Drawing.SystemColors.ControlDark;
            this.propertyGrid1.Location = new System.Drawing.Point(683, 12);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(302, 421);
            this.propertyGrid1.TabIndex = 61;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(158, 436);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(211, 23);
            this.comboBox1.TabIndex = 61;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(11, 438);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(141, 19);
            this.checkBox1.TabIndex = 60;
            this.checkBox1.Text = "Layers Triming";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // MoveDownBotton
            // 
            this.MoveDownBotton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveDownBotton.ForeColor = System.Drawing.Color.SlateBlue;
            this.MoveDownBotton.Location = new System.Drawing.Point(651, 168);
            this.MoveDownBotton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MoveDownBotton.Name = "MoveDownBotton";
            this.MoveDownBotton.Size = new System.Drawing.Size(24, 27);
            this.MoveDownBotton.TabIndex = 59;
            this.MoveDownBotton.Text = "↓";
            this.MoveDownBotton.UseVisualStyleBackColor = true;
            this.MoveDownBotton.Click += new System.EventHandler(this.MoveDownBotton_Click);
            // 
            // MoveUpButton
            // 
            this.MoveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveUpButton.ForeColor = System.Drawing.Color.SlateBlue;
            this.MoveUpButton.Location = new System.Drawing.Point(651, 135);
            this.MoveUpButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MoveUpButton.Name = "MoveUpButton";
            this.MoveUpButton.Size = new System.Drawing.Size(24, 27);
            this.MoveUpButton.TabIndex = 57;
            this.MoveUpButton.Text = "↑";
            this.MoveUpButton.UseVisualStyleBackColor = true;
            this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
            // 
            // RemoveButton2
            // 
            this.RemoveButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveButton2.ForeColor = System.Drawing.Color.SlateBlue;
            this.RemoveButton2.Location = new System.Drawing.Point(651, 57);
            this.RemoveButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.RemoveButton2.Name = "RemoveButton2";
            this.RemoveButton2.Size = new System.Drawing.Size(24, 27);
            this.RemoveButton2.TabIndex = 56;
            this.RemoveButton2.Text = "-";
            this.RemoveButton2.UseVisualStyleBackColor = true;
            this.RemoveButton2.Click += new System.EventHandler(this.RemoveButton2_Click);
            // 
            // listBox2
            // 
            this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.ItemHeight = 15;
            this.listBox2.Location = new System.Drawing.Point(11, 24);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(636, 409);
            this.listBox2.TabIndex = 0;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            // 
            // AddButton2
            // 
            this.AddButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddButton2.ForeColor = System.Drawing.Color.SlateBlue;
            this.AddButton2.Location = new System.Drawing.Point(651, 24);
            this.AddButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.AddButton2.Name = "AddButton2";
            this.AddButton2.Size = new System.Drawing.Size(24, 27);
            this.AddButton2.TabIndex = 55;
            this.AddButton2.Text = "+";
            this.AddButton2.UseVisualStyleBackColor = true;
            this.AddButton2.Click += new System.EventHandler(this.AddButton2_Click);
            // 
            // CANCELBUTTON
            // 
            this.CANCELBUTTON.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CANCELBUTTON.Location = new System.Drawing.Point(875, 516);
            this.CANCELBUTTON.Name = "CANCELBUTTON";
            this.CANCELBUTTON.Size = new System.Drawing.Size(123, 34);
            this.CANCELBUTTON.TabIndex = 60;
            this.CANCELBUTTON.Text = "CANCEL";
            this.CANCELBUTTON.UseVisualStyleBackColor = true;
            this.CANCELBUTTON.Click += new System.EventHandler(this.CANCELBUTTON_Click);
            // 
            // OKBUTTON
            // 
            this.OKBUTTON.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKBUTTON.Location = new System.Drawing.Point(14, 516);
            this.OKBUTTON.Name = "OKBUTTON";
            this.OKBUTTON.Size = new System.Drawing.Size(123, 34);
            this.OKBUTTON.TabIndex = 59;
            this.OKBUTTON.Text = "OK";
            this.OKBUTTON.UseVisualStyleBackColor = true;
            this.OKBUTTON.Click += new System.EventHandler(this.OKBUTTON_Click);
            // 
            // CreateMeshLayersFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 576);
            this.Controls.Add(this.CANCELBUTTON);
            this.Controls.Add(this.OKBUTTON);
            this.Controls.Add(this.groupBox1);
            this.Name = "CreateMeshLayersFrom";
            this.Text = "CreateMeshLayersFrom";
            this.Load += new System.EventHandler(this.CreateMeshLayersFrom_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button MoveDownBotton;
        private System.Windows.Forms.Button MoveUpButton;
        private System.Windows.Forms.Button RemoveButton2;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button AddButton2;
        private System.Windows.Forms.Button CANCELBUTTON;
        private System.Windows.Forms.Button OKBUTTON;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
    }
}