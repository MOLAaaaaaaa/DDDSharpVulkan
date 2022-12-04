namespace DDDSharp
{
    partial class SlicerDrawForm
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
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.PositionRangeLabel = new System.Windows.Forms.Label();
            this.OKbutton1 = new System.Windows.Forms.Button();
            this.ExportSlicerButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.slicerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawSlicerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.slicerPlanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xOYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xOZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yOZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.CreateLineMeshButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.PlayButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ZRangeLabel = new System.Windows.Forms.Label();
            this.AddZ = new System.Windows.Forms.Button();
            this.ZPosTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.YRangeLabel = new System.Windows.Forms.Label();
            this.AddY = new System.Windows.Forms.Button();
            this.YPosTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.XRangeLabel = new System.Windows.Forms.Label();
            this.AddX = new System.Windows.Forms.Button();
            this.XPosTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1DrawLine = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2ZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3ZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4Pan = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5Reset = new System.Windows.Forms.ToolStripButton();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(75, 27);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(53, 25);
            this.numericUpDown1.TabIndex = 7;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(9, 29);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(59, 23);
            this.comboBox1.TabIndex = 5;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // PositionRangeLabel
            // 
            this.PositionRangeLabel.AutoSize = true;
            this.PositionRangeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.PositionRangeLabel.Location = new System.Drawing.Point(168, 32);
            this.PositionRangeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.PositionRangeLabel.Name = "PositionRangeLabel";
            this.PositionRangeLabel.Size = new System.Drawing.Size(63, 15);
            this.PositionRangeLabel.TabIndex = 3;
            this.PositionRangeLabel.Text = "0 - 324";
            // 
            // OKbutton1
            // 
            this.OKbutton1.Location = new System.Drawing.Point(14, 634);
            this.OKbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.OKbutton1.Name = "OKbutton1";
            this.OKbutton1.Size = new System.Drawing.Size(214, 27);
            this.OKbutton1.TabIndex = 0;
            this.OKbutton1.Text = "Create Slicers";
            this.OKbutton1.UseVisualStyleBackColor = true;
            this.OKbutton1.Click += new System.EventHandler(this.OKbutton1_Click);
            // 
            // ExportSlicerButton
            // 
            this.ExportSlicerButton.Location = new System.Drawing.Point(162, 601);
            this.ExportSlicerButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ExportSlicerButton.Name = "ExportSlicerButton";
            this.ExportSlicerButton.Size = new System.Drawing.Size(68, 27);
            this.ExportSlicerButton.TabIndex = 3;
            this.ExportSlicerButton.Text = "Export";
            this.ExportSlicerButton.UseVisualStyleBackColor = true;
            this.ExportSlicerButton.Click += new System.EventHandler(this.ExportSlicerButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(88, 601);
            this.DeleteButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(68, 27);
            this.DeleteButton.TabIndex = 2;
            this.DeleteButton.Text = "Remove";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.Location = new System.Drawing.Point(14, 601);
            this.LoadButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(68, 27);
            this.LoadButton.TabIndex = 1;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(11, 25);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(217, 124);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slicerToolStripMenuItem,
            this.playToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1121, 28);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // slicerToolStripMenuItem
            // 
            this.slicerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawSlicerToolStripMenuItem,
            this.slicerPlanToolStripMenuItem});
            this.slicerToolStripMenuItem.Name = "slicerToolStripMenuItem";
            this.slicerToolStripMenuItem.Size = new System.Drawing.Size(104, 24);
            this.slicerToolStripMenuItem.Text = "Draw Slicer";
            // 
            // drawSlicerToolStripMenuItem
            // 
            this.drawSlicerToolStripMenuItem.Name = "drawSlicerToolStripMenuItem";
            this.drawSlicerToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.drawSlicerToolStripMenuItem.Text = "Draw Slicer";
            this.drawSlicerToolStripMenuItem.Click += new System.EventHandler(this.drawSlicerToolStripMenuItem_Click);
            // 
            // slicerPlanToolStripMenuItem
            // 
            this.slicerPlanToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xOYToolStripMenuItem,
            this.xOZToolStripMenuItem,
            this.yOZToolStripMenuItem});
            this.slicerPlanToolStripMenuItem.Name = "slicerPlanToolStripMenuItem";
            this.slicerPlanToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.slicerPlanToolStripMenuItem.Text = "Slicer Plan";
            this.slicerPlanToolStripMenuItem.DropDownOpened += new System.EventHandler(this.slicerPlanToolStripMenuItem_DropDownOpened);
            // 
            // xOYToolStripMenuItem
            // 
            this.xOYToolStripMenuItem.Name = "xOYToolStripMenuItem";
            this.xOYToolStripMenuItem.Size = new System.Drawing.Size(123, 26);
            this.xOYToolStripMenuItem.Text = "XOY";
            // 
            // xOZToolStripMenuItem
            // 
            this.xOZToolStripMenuItem.Name = "xOZToolStripMenuItem";
            this.xOZToolStripMenuItem.Size = new System.Drawing.Size(123, 26);
            this.xOZToolStripMenuItem.Text = "XOZ";
            // 
            // yOZToolStripMenuItem
            // 
            this.yOZToolStripMenuItem.Name = "yOZToolStripMenuItem";
            this.yOZToolStripMenuItem.Size = new System.Drawing.Size(123, 26);
            this.yOZToolStripMenuItem.Text = "YOZ";
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingToolStripMenuItem,
            this.playToolStripMenuItem1});
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(52, 24);
            this.playToolStripMenuItem.Text = "Play";
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            this.settingToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.settingToolStripMenuItem.Text = "Setting";
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // playToolStripMenuItem1
            // 
            this.playToolStripMenuItem1.Name = "playToolStripMenuItem1";
            this.playToolStripMenuItem1.Size = new System.Drawing.Size(145, 26);
            this.playToolStripMenuItem1.Text = "Play";
            this.playToolStripMenuItem1.Click += new System.EventHandler(this.playToolStripMenuItem1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.propertyGrid1);
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Location = new System.Drawing.Point(13, 262);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(238, 324);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Slicers";
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.LineColor = System.Drawing.SystemColors.ControlDark;
            this.propertyGrid1.Location = new System.Drawing.Point(12, 156);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(217, 161);
            this.propertyGrid1.TabIndex = 11;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // CreateLineMeshButton
            // 
            this.CreateLineMeshButton.Location = new System.Drawing.Point(14, 667);
            this.CreateLineMeshButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CreateLineMeshButton.Name = "CreateLineMeshButton";
            this.CreateLineMeshButton.Size = new System.Drawing.Size(214, 27);
            this.CreateLineMeshButton.TabIndex = 10;
            this.CreateLineMeshButton.Text = "Create Line Meshe";
            this.CreateLineMeshButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.PlayButton);
            this.groupBox2.Controls.Add(this.numericUpDown1);
            this.groupBox2.Controls.Add(this.PositionRangeLabel);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.ForeColor = System.Drawing.Color.Blue;
            this.groupBox2.Location = new System.Drawing.Point(13, 61);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(238, 64);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Slicer Plan";
            // 
            // PlayButton
            // 
            this.PlayButton.Location = new System.Drawing.Point(133, 28);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(33, 23);
            this.PlayButton.TabIndex = 8;
            this.PlayButton.Text = ">>";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ZRangeLabel);
            this.groupBox3.Controls.Add(this.AddZ);
            this.groupBox3.Controls.Add(this.ZPosTextBox);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.YRangeLabel);
            this.groupBox3.Controls.Add(this.AddY);
            this.groupBox3.Controls.Add(this.YPosTextBox);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.XRangeLabel);
            this.groupBox3.Controls.Add(this.AddX);
            this.groupBox3.Controls.Add(this.XPosTextBox);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.ForeColor = System.Drawing.Color.Blue;
            this.groupBox3.Location = new System.Drawing.Point(13, 131);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(238, 120);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Axis Slicer";
            // 
            // ZRangeLabel
            // 
            this.ZRangeLabel.AutoSize = true;
            this.ZRangeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.ZRangeLabel.Location = new System.Drawing.Point(104, 91);
            this.ZRangeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ZRangeLabel.Name = "ZRangeLabel";
            this.ZRangeLabel.Size = new System.Drawing.Size(63, 15);
            this.ZRangeLabel.TabIndex = 12;
            this.ZRangeLabel.Text = "0 - 324";
            // 
            // AddZ
            // 
            this.AddZ.ForeColor = System.Drawing.Color.Black;
            this.AddZ.Location = new System.Drawing.Point(177, 86);
            this.AddZ.Name = "AddZ";
            this.AddZ.Size = new System.Drawing.Size(52, 23);
            this.AddZ.TabIndex = 11;
            this.AddZ.Text = "Add";
            this.AddZ.UseVisualStyleBackColor = true;
            this.AddZ.Click += new System.EventHandler(this.AddZ_Click);
            // 
            // ZPosTextBox
            // 
            this.ZPosTextBox.Location = new System.Drawing.Point(30, 86);
            this.ZPosTextBox.Name = "ZPosTextBox";
            this.ZPosTextBox.Size = new System.Drawing.Size(69, 25);
            this.ZPosTextBox.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label6.Location = new System.Drawing.Point(9, 91);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(15, 15);
            this.label6.TabIndex = 9;
            this.label6.Text = "Z";
            // 
            // YRangeLabel
            // 
            this.YRangeLabel.AutoSize = true;
            this.YRangeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.YRangeLabel.Location = new System.Drawing.Point(103, 60);
            this.YRangeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.YRangeLabel.Name = "YRangeLabel";
            this.YRangeLabel.Size = new System.Drawing.Size(63, 15);
            this.YRangeLabel.TabIndex = 8;
            this.YRangeLabel.Text = "0 - 324";
            // 
            // AddY
            // 
            this.AddY.ForeColor = System.Drawing.Color.Black;
            this.AddY.Location = new System.Drawing.Point(176, 55);
            this.AddY.Name = "AddY";
            this.AddY.Size = new System.Drawing.Size(52, 23);
            this.AddY.TabIndex = 7;
            this.AddY.Text = "Add";
            this.AddY.UseVisualStyleBackColor = true;
            this.AddY.Click += new System.EventHandler(this.AddY_Click);
            // 
            // YPosTextBox
            // 
            this.YPosTextBox.Location = new System.Drawing.Point(29, 55);
            this.YPosTextBox.Name = "YPosTextBox";
            this.YPosTextBox.Size = new System.Drawing.Size(69, 25);
            this.YPosTextBox.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label4.Location = new System.Drawing.Point(8, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Y";
            // 
            // XRangeLabel
            // 
            this.XRangeLabel.AutoSize = true;
            this.XRangeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.XRangeLabel.Location = new System.Drawing.Point(103, 29);
            this.XRangeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.XRangeLabel.Name = "XRangeLabel";
            this.XRangeLabel.Size = new System.Drawing.Size(63, 15);
            this.XRangeLabel.TabIndex = 4;
            this.XRangeLabel.Text = "0 - 324";
            // 
            // AddX
            // 
            this.AddX.ForeColor = System.Drawing.Color.Black;
            this.AddX.Location = new System.Drawing.Point(176, 24);
            this.AddX.Name = "AddX";
            this.AddX.Size = new System.Drawing.Size(52, 23);
            this.AddX.TabIndex = 2;
            this.AddX.Text = "Add";
            this.AddX.UseVisualStyleBackColor = true;
            this.AddX.Click += new System.EventHandler(this.AddX_Click);
            // 
            // XPosTextBox
            // 
            this.XPosTextBox.Location = new System.Drawing.Point(29, 24);
            this.XPosTextBox.Name = "XPosTextBox";
            this.XPosTextBox.Size = new System.Drawing.Size(69, 25);
            this.XPosTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(8, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "X";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1DrawLine,
            this.toolStripButton2ZoomIn,
            this.toolStripButton3ZoomOut,
            this.toolStripButton4Pan,
            this.toolStripButton5Reset});
            this.toolStrip1.Location = new System.Drawing.Point(0, 28);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1121, 27);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1DrawLine
            // 
            this.toolStripButton1DrawLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1DrawLine.Image = global::DDDSharp.Properties.Resources.line;
            this.toolStripButton1DrawLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1DrawLine.Name = "toolStripButton1DrawLine";
            this.toolStripButton1DrawLine.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton1DrawLine.Text = "toolStripButton1";
            this.toolStripButton1DrawLine.Click += new System.EventHandler(this.drawSlicerToolStripMenuItem_Click);
            // 
            // toolStripButton2ZoomIn
            // 
            this.toolStripButton2ZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2ZoomIn.Image = global::DDDSharp.Properties.Resources.zoom_in;
            this.toolStripButton2ZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2ZoomIn.Name = "toolStripButton2ZoomIn";
            this.toolStripButton2ZoomIn.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton2ZoomIn.Text = "toolStripButton2";
            // 
            // toolStripButton3ZoomOut
            // 
            this.toolStripButton3ZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3ZoomOut.Image = global::DDDSharp.Properties.Resources.zoom_out;
            this.toolStripButton3ZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3ZoomOut.Name = "toolStripButton3ZoomOut";
            this.toolStripButton3ZoomOut.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton3ZoomOut.Text = "toolStripButton3";
            // 
            // toolStripButton4Pan
            // 
            this.toolStripButton4Pan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4Pan.Image = global::DDDSharp.Properties.Resources.Hand;
            this.toolStripButton4Pan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4Pan.Name = "toolStripButton4Pan";
            this.toolStripButton4Pan.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton4Pan.Text = "toolStripButton4";
            // 
            // toolStripButton5Reset
            // 
            this.toolStripButton5Reset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5Reset.Image = global::DDDSharp.Properties.Resources.Reset24;
            this.toolStripButton5Reset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5Reset.Name = "toolStripButton5Reset";
            this.toolStripButton5Reset.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton5Reset.Text = "toolStripButton5";
            // 
            // InfoLabel
            // 
            this.InfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.Location = new System.Drawing.Point(257, 709);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(55, 15);
            this.InfoLabel.TabIndex = 10;
            this.InfoLabel.Text = "X,Y = ";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(257, 61);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(852, 641);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.SizeChanged += new System.EventHandler(this.pictureBox1_SizeChanged);
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // SlicerDrawForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1121, 729);
            this.Controls.Add(this.CreateLineMeshButton);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.ExportSlicerButton);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.LoadButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.OKbutton1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SlicerDrawForm";
            this.Text = "SlicerDrawForm";
            this.Load += new System.EventHandler(this.SlicerDrawForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SlicerDrawForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SlicerDrawForm_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button OKbutton1;
        private System.Windows.Forms.Button ExportSlicerButton;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem slicerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawSlicerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem slicerPlanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xOYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xOZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yOZToolStripMenuItem;
        private System.Windows.Forms.Label PositionRangeLabel;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label ZRangeLabel;
        private System.Windows.Forms.Button AddZ;
        private System.Windows.Forms.TextBox ZPosTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label YRangeLabel;
        private System.Windows.Forms.Button AddY;
        private System.Windows.Forms.TextBox YPosTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label XRangeLabel;
        private System.Windows.Forms.Button AddX;
        private System.Windows.Forms.TextBox XPosTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1DrawLine;
        private System.Windows.Forms.ToolStripButton toolStripButton2ZoomIn;
        private System.Windows.Forms.ToolStripButton toolStripButton3ZoomOut;
        private System.Windows.Forms.ToolStripButton toolStripButton4Pan;
        private System.Windows.Forms.ToolStripButton toolStripButton5Reset;
        private System.Windows.Forms.Button CreateLineMeshButton;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.Button PlayButton;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
    }
}