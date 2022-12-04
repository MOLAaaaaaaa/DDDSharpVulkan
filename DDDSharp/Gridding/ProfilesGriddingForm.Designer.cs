
namespace DDDSharp
{
    partial class ProfilesGriddingForm
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
            this.textZNum = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.RemoveButton1 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.AddButton1 = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.PauseButton = new System.Windows.Forms.Button();
            this.StartButton = new System.Windows.Forms.Button();
            this.textOutputFile = new System.Windows.Forms.TextBox();
            this.outputButton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.labelMemoryInfo = new System.Windows.Forms.Label();
            this.methodComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.Methodbutton1 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.textStepZ = new System.Windows.Forms.TextBox();
            this.textZ2 = new System.Windows.Forms.TextBox();
            this.textZ1 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelTimeLeft = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SaveAsButton = new System.Windows.Forms.Button();
            this.textYNum = new System.Windows.Forms.TextBox();
            this.textX1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textStepY = new System.Windows.Forms.TextBox();
            this.textY2 = new System.Windows.Forms.TextBox();
            this.textY1 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textXNum = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textStepX = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textX2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.MoveDownBotton = new System.Windows.Forms.Button();
            this.MoveUpButton = new System.Windows.Forms.Button();
            this.RemoveButton2 = new System.Windows.Forms.Button();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.AddButton2 = new System.Windows.Forms.Button();
            this.textBoxGridInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textZNum
            // 
            this.textZNum.Location = new System.Drawing.Point(376, 97);
            this.textZNum.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textZNum.Name = "textZNum";
            this.textZNum.Size = new System.Drawing.Size(68, 25);
            this.textZNum.TabIndex = 23;
            this.textZNum.TextChanged += new System.EventHandler(this.textZNum_TextChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.RemoveButton1);
            this.groupBox6.Controls.Add(this.listBox1);
            this.groupBox6.Controls.Add(this.AddButton1);
            this.groupBox6.ForeColor = System.Drawing.Color.Sienna;
            this.groupBox6.Location = new System.Drawing.Point(15, 24);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox6.Size = new System.Drawing.Size(458, 220);
            this.groupBox6.TabIndex = 46;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Geological Profiles";
            // 
            // RemoveButton1
            // 
            this.RemoveButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveButton1.ForeColor = System.Drawing.Color.SlateBlue;
            this.RemoveButton1.Location = new System.Drawing.Point(432, 57);
            this.RemoveButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.RemoveButton1.Name = "RemoveButton1";
            this.RemoveButton1.Size = new System.Drawing.Size(24, 27);
            this.RemoveButton1.TabIndex = 56;
            this.RemoveButton1.Text = "-";
            this.RemoveButton1.UseVisualStyleBackColor = true;
            this.RemoveButton1.Click += new System.EventHandler(this.RemoveButton1_Click);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(11, 24);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(419, 184);
            this.listBox1.TabIndex = 0;
            // 
            // AddButton1
            // 
            this.AddButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddButton1.ForeColor = System.Drawing.Color.SlateBlue;
            this.AddButton1.Location = new System.Drawing.Point(432, 24);
            this.AddButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.AddButton1.Name = "AddButton1";
            this.AddButton1.Size = new System.Drawing.Size(24, 27);
            this.AddButton1.TabIndex = 55;
            this.AddButton1.Text = "+";
            this.AddButton1.UseVisualStyleBackColor = true;
            this.AddButton1.Click += new System.EventHandler(this.AddButton1_Click);
            // 
            // StopButton
            // 
            this.StopButton.ForeColor = System.Drawing.Color.SlateBlue;
            this.StopButton.Location = new System.Drawing.Point(268, 539);
            this.StopButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(85, 27);
            this.StopButton.TabIndex = 45;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            // 
            // PauseButton
            // 
            this.PauseButton.ForeColor = System.Drawing.Color.SlateBlue;
            this.PauseButton.Location = new System.Drawing.Point(139, 539);
            this.PauseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(85, 27);
            this.PauseButton.TabIndex = 44;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            // 
            // StartButton
            // 
            this.StartButton.ForeColor = System.Drawing.Color.SlateBlue;
            this.StartButton.Location = new System.Drawing.Point(13, 539);
            this.StartButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(85, 27);
            this.StartButton.TabIndex = 43;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // textOutputFile
            // 
            this.textOutputFile.Location = new System.Drawing.Point(2, 18);
            this.textOutputFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textOutputFile.Name = "textOutputFile";
            this.textOutputFile.Size = new System.Drawing.Size(380, 25);
            this.textOutputFile.TabIndex = 11;
            // 
            // outputButton
            // 
            this.outputButton.ForeColor = System.Drawing.Color.RoyalBlue;
            this.outputButton.Location = new System.Drawing.Point(378, 17);
            this.outputButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outputButton.Name = "outputButton";
            this.outputButton.Size = new System.Drawing.Size(79, 27);
            this.outputButton.TabIndex = 5;
            this.outputButton.Text = "Browse";
            this.outputButton.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.labelMemoryInfo);
            this.groupBox5.Controls.Add(this.methodComboBox);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.Methodbutton1);
            this.groupBox5.ForeColor = System.Drawing.Color.Sienna;
            this.groupBox5.Location = new System.Drawing.Point(13, 384);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox5.Size = new System.Drawing.Size(460, 84);
            this.groupBox5.TabIndex = 56;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Gridding Method";
            // 
            // labelMemoryInfo
            // 
            this.labelMemoryInfo.AutoSize = true;
            this.labelMemoryInfo.ForeColor = System.Drawing.Color.Black;
            this.labelMemoryInfo.Location = new System.Drawing.Point(10, 55);
            this.labelMemoryInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMemoryInfo.Name = "labelMemoryInfo";
            this.labelMemoryInfo.Size = new System.Drawing.Size(127, 15);
            this.labelMemoryInfo.TabIndex = 36;
            this.labelMemoryInfo.Text = "Memory Required";
            // 
            // methodComboBox
            // 
            this.methodComboBox.FormattingEnabled = true;
            this.methodComboBox.Location = new System.Drawing.Point(11, 22);
            this.methodComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.methodComboBox.Name = "methodComboBox";
            this.methodComboBox.Size = new System.Drawing.Size(357, 23);
            this.methodComboBox.TabIndex = 33;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(-11, 12);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 15);
            this.label8.TabIndex = 34;
            // 
            // Methodbutton1
            // 
            this.Methodbutton1.ForeColor = System.Drawing.Color.RoyalBlue;
            this.Methodbutton1.Location = new System.Drawing.Point(378, 20);
            this.Methodbutton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Methodbutton1.Name = "Methodbutton1";
            this.Methodbutton1.Size = new System.Drawing.Size(79, 27);
            this.Methodbutton1.TabIndex = 35;
            this.Methodbutton1.Text = "Options";
            this.Methodbutton1.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.textOutputFile);
            this.groupBox4.Controls.Add(this.outputButton);
            this.groupBox4.ForeColor = System.Drawing.Color.Sienna;
            this.groupBox4.Location = new System.Drawing.Point(13, 474);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(460, 59);
            this.groupBox4.TabIndex = 42;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output Grid file";
            // 
            // textStepZ
            // 
            this.textStepZ.Location = new System.Drawing.Point(249, 97);
            this.textStepZ.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textStepZ.Name = "textStepZ";
            this.textStepZ.Size = new System.Drawing.Size(112, 25);
            this.textStepZ.TabIndex = 22;
            this.textStepZ.TextChanged += new System.EventHandler(this.textStepZ_TextChanged);
            // 
            // textZ2
            // 
            this.textZ2.Location = new System.Drawing.Point(138, 97);
            this.textZ2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textZ2.Name = "textZ2";
            this.textZ2.Size = new System.Drawing.Size(97, 25);
            this.textZ2.TabIndex = 21;
            this.textZ2.TextChanged += new System.EventHandler(this.textZ2_TextChanged);
            // 
            // textZ1
            // 
            this.textZ1.Location = new System.Drawing.Point(26, 97);
            this.textZ1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textZ1.Name = "textZ1";
            this.textZ1.Size = new System.Drawing.Size(100, 25);
            this.textZ1.TabIndex = 20;
            this.textZ1.TextChanged += new System.EventHandler(this.textZ1_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ForeColor = System.Drawing.Color.Black;
            this.label12.Location = new System.Drawing.Point(2, 100);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(23, 15);
            this.label12.TabIndex = 19;
            this.label12.Text = "Z:";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox1.HorizontalExtent = 500;
            this.checkedListBox1.HorizontalScrollbar = true;
            this.checkedListBox1.Location = new System.Drawing.Point(2, 27);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.ScrollAlwaysVisible = true;
            this.checkedListBox1.Size = new System.Drawing.Size(414, 144);
            this.checkedListBox1.TabIndex = 18;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkedListBox1);
            this.groupBox2.ForeColor = System.Drawing.Color.Sienna;
            this.groupBox2.Location = new System.Drawing.Point(486, 384);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(430, 182);
            this.groupBox2.TabIndex = 54;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Compute Devices";
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.labelTitle.Location = new System.Drawing.Point(189, 602);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(95, 15);
            this.labelTitle.TabIndex = 53;
            this.labelTitle.Text = "percentages";
            // 
            // labelTimeLeft
            // 
            this.labelTimeLeft.AutoSize = true;
            this.labelTimeLeft.ForeColor = System.Drawing.Color.BlueViolet;
            this.labelTimeLeft.Location = new System.Drawing.Point(485, 578);
            this.labelTimeLeft.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTimeLeft.Name = "labelTimeLeft";
            this.labelTimeLeft.Size = new System.Drawing.Size(103, 15);
            this.labelTimeLeft.TabIndex = 49;
            this.labelTimeLeft.Text = "time left(s)";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(13, 572);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(460, 27);
            this.progressBar1.TabIndex = 48;
            // 
            // SaveAsButton
            // 
            this.SaveAsButton.ForeColor = System.Drawing.Color.SlateBlue;
            this.SaveAsButton.Location = new System.Drawing.Point(388, 539);
            this.SaveAsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SaveAsButton.Name = "SaveAsButton";
            this.SaveAsButton.Size = new System.Drawing.Size(85, 27);
            this.SaveAsButton.TabIndex = 47;
            this.SaveAsButton.Text = "Save As";
            this.SaveAsButton.UseVisualStyleBackColor = true;
            // 
            // textYNum
            // 
            this.textYNum.Location = new System.Drawing.Point(376, 67);
            this.textYNum.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textYNum.Name = "textYNum";
            this.textYNum.Size = new System.Drawing.Size(68, 25);
            this.textYNum.TabIndex = 18;
            this.textYNum.TextChanged += new System.EventHandler(this.textYNum_TextChanged);
            // 
            // textX1
            // 
            this.textX1.Location = new System.Drawing.Point(26, 37);
            this.textX1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textX1.Name = "textX1";
            this.textX1.Size = new System.Drawing.Size(100, 25);
            this.textX1.TabIndex = 6;
            this.textX1.TextChanged += new System.EventHandler(this.textX1_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(2, 40);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "X:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textZNum);
            this.groupBox3.Controls.Add(this.textStepZ);
            this.groupBox3.Controls.Add(this.textZ2);
            this.groupBox3.Controls.Add(this.textZ1);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.textYNum);
            this.groupBox3.Controls.Add(this.textStepY);
            this.groupBox3.Controls.Add(this.textY2);
            this.groupBox3.Controls.Add(this.textY1);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.textXNum);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.textStepX);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.textX2);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.textX1);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.ForeColor = System.Drawing.Color.Maroon;
            this.groupBox3.Location = new System.Drawing.Point(15, 245);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(458, 133);
            this.groupBox3.TabIndex = 41;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Gridding Geometry";
            // 
            // textStepY
            // 
            this.textStepY.Location = new System.Drawing.Point(249, 67);
            this.textStepY.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textStepY.Name = "textStepY";
            this.textStepY.Size = new System.Drawing.Size(112, 25);
            this.textStepY.TabIndex = 17;
            this.textStepY.TextChanged += new System.EventHandler(this.textStepY_TextChanged);
            // 
            // textY2
            // 
            this.textY2.Location = new System.Drawing.Point(138, 67);
            this.textY2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textY2.Name = "textY2";
            this.textY2.Size = new System.Drawing.Size(97, 25);
            this.textY2.TabIndex = 16;
            this.textY2.TextChanged += new System.EventHandler(this.textY2_TextChanged);
            // 
            // textY1
            // 
            this.textY1.Location = new System.Drawing.Point(26, 67);
            this.textY1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textY1.Name = "textY1";
            this.textY1.Size = new System.Drawing.Size(100, 25);
            this.textY1.TabIndex = 15;
            this.textY1.TextChanged += new System.EventHandler(this.textY1_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ForeColor = System.Drawing.Color.Black;
            this.label11.Location = new System.Drawing.Point(2, 70);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(23, 15);
            this.label11.TabIndex = 14;
            this.label11.Text = "Y:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.Color.Black;
            this.label10.Location = new System.Drawing.Point(384, 19);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(31, 15);
            this.label10.TabIndex = 13;
            this.label10.Text = "Num";
            // 
            // textXNum
            // 
            this.textXNum.Location = new System.Drawing.Point(376, 37);
            this.textXNum.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textXNum.Name = "textXNum";
            this.textXNum.Size = new System.Drawing.Size(68, 25);
            this.textXNum.TabIndex = 12;
            this.textXNum.TextChanged += new System.EventHandler(this.textXNum_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.Color.Black;
            this.label9.Location = new System.Drawing.Point(258, 19);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 15);
            this.label9.TabIndex = 11;
            this.label9.Text = "Spacing";
            // 
            // textStepX
            // 
            this.textStepX.Location = new System.Drawing.Point(249, 37);
            this.textStepX.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textStepX.Name = "textStepX";
            this.textStepX.Size = new System.Drawing.Size(112, 25);
            this.textStepX.TabIndex = 10;
            this.textStepX.TextChanged += new System.EventHandler(this.textStepX_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(151, 19);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 15);
            this.label7.TabIndex = 9;
            this.label7.Text = "Maximum";
            // 
            // textX2
            // 
            this.textX2.Location = new System.Drawing.Point(138, 37);
            this.textX2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textX2.Name = "textX2";
            this.textX2.Size = new System.Drawing.Size(97, 25);
            this.textX2.TabIndex = 8;
            this.textX2.TextChanged += new System.EventHandler(this.textX2_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(42, 19);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 15);
            this.label6.TabIndex = 7;
            this.label6.Text = "Minimum";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.MoveDownBotton);
            this.groupBox1.Controls.Add(this.MoveUpButton);
            this.groupBox1.Controls.Add(this.RemoveButton2);
            this.groupBox1.Controls.Add(this.listBox2);
            this.groupBox1.Controls.Add(this.AddButton2);
            this.groupBox1.ForeColor = System.Drawing.Color.Sienna;
            this.groupBox1.Location = new System.Drawing.Point(486, 24);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(458, 220);
            this.groupBox1.TabIndex = 57;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Restricted interfaces";
            // 
            // MoveDownBotton
            // 
            this.MoveDownBotton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveDownBotton.ForeColor = System.Drawing.Color.SlateBlue;
            this.MoveDownBotton.Location = new System.Drawing.Point(433, 166);
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
            this.MoveUpButton.Location = new System.Drawing.Point(433, 133);
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
            this.RemoveButton2.Location = new System.Drawing.Point(433, 57);
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
            this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.ItemHeight = 15;
            this.listBox2.Location = new System.Drawing.Point(11, 24);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(419, 169);
            this.listBox2.TabIndex = 0;
            // 
            // AddButton2
            // 
            this.AddButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddButton2.ForeColor = System.Drawing.Color.SlateBlue;
            this.AddButton2.Location = new System.Drawing.Point(433, 24);
            this.AddButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.AddButton2.Name = "AddButton2";
            this.AddButton2.Size = new System.Drawing.Size(24, 27);
            this.AddButton2.TabIndex = 55;
            this.AddButton2.Text = "+";
            this.AddButton2.UseVisualStyleBackColor = true;
            this.AddButton2.Click += new System.EventHandler(this.AddButton2_Click);
            // 
            // textBoxGridInfo
            // 
            this.textBoxGridInfo.Location = new System.Drawing.Point(497, 254);
            this.textBoxGridInfo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxGridInfo.Multiline = true;
            this.textBoxGridInfo.Name = "textBoxGridInfo";
            this.textBoxGridInfo.ReadOnly = true;
            this.textBoxGridInfo.Size = new System.Drawing.Size(419, 122);
            this.textBoxGridInfo.TabIndex = 58;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.BlueViolet;
            this.label1.Location = new System.Drawing.Point(13, 198);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 15);
            this.label1.TabIndex = 59;
            this.label1.Text = "Meshes Sorted by Z values Asc";
            // 
            // ProfilesGriddingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 665);
            this.Controls.Add(this.textBoxGridInfo);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelTimeLeft);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.SaveAsButton);
            this.Controls.Add(this.groupBox3);
            this.Name = "ProfilesGriddingForm";
            this.Text = "ProfilesGriddingForm";
            this.Load += new System.EventHandler(this.ProfilesGriddingForm_Load);
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textZNum;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button RemoveButton1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button AddButton1;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button PauseButton;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.TextBox textOutputFile;
        private System.Windows.Forms.Button outputButton;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label labelMemoryInfo;
        private System.Windows.Forms.ComboBox methodComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button Methodbutton1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox textStepZ;
        private System.Windows.Forms.TextBox textZ2;
        private System.Windows.Forms.TextBox textZ1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelTimeLeft;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button SaveAsButton;
        private System.Windows.Forms.TextBox textYNum;
        private System.Windows.Forms.TextBox textX1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textStepY;
        private System.Windows.Forms.TextBox textY2;
        private System.Windows.Forms.TextBox textY1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textXNum;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textStepX;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textX2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button RemoveButton2;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button AddButton2;
        private System.Windows.Forms.TextBox textBoxGridInfo;
        private System.Windows.Forms.Button MoveDownBotton;
        private System.Windows.Forms.Button MoveUpButton;
        private System.Windows.Forms.Label label1;
    }
}