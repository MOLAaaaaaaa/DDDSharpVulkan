
namespace DDDSharp.Boreholes
{
    partial class BoreholesWellDataForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.OK = new System.Windows.Forms.Button();
            this.CANCEL = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.dataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traceAnglesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromExcelFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromMeshesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.Location = new System.Drawing.Point(12, 31);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(817, 417);
            this.dataGridView1.TabIndex = 9;
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OK.Location = new System.Drawing.Point(12, 465);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(117, 32);
            this.OK.TabIndex = 10;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // CANCEL
            // 
            this.CANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CANCEL.Location = new System.Drawing.Point(712, 465);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(117, 32);
            this.CANCEL.TabIndex = 11;
            this.CANCEL.Text = "CANCEL";
            this.CANCEL.UseVisualStyleBackColor = true;
            this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dataToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(841, 28);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // dataToolStripMenuItem
            // 
            this.dataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newLineToolStripMenuItem,
            this.traceAnglesToolStripMenuItem,
            this.addColumnsToolStripMenuItem,
            this.importFromToolStripMenuItem,
            this.expportToolStripMenuItem});
            this.dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            this.dataToolStripMenuItem.Size = new System.Drawing.Size(56, 24);
            this.dataToolStripMenuItem.Text = "&Data";
            // 
            // newLineToolStripMenuItem
            // 
            this.newLineToolStripMenuItem.Name = "newLineToolStripMenuItem";
            this.newLineToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.newLineToolStripMenuItem.Text = "&New Line";
            this.newLineToolStripMenuItem.Click += new System.EventHandler(this.newLineToolStripMenuItem_Click);
            // 
            // traceAnglesToolStripMenuItem
            // 
            this.traceAnglesToolStripMenuItem.Name = "traceAnglesToolStripMenuItem";
            this.traceAnglesToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.traceAnglesToolStripMenuItem.Text = "Trace Angles";
            this.traceAnglesToolStripMenuItem.Click += new System.EventHandler(this.traceAnglesToolStripMenuItem_Click);
            // 
            // addColumnsToolStripMenuItem
            // 
            this.addColumnsToolStripMenuItem.Name = "addColumnsToolStripMenuItem";
            this.addColumnsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.addColumnsToolStripMenuItem.Text = "Add Columns";
            this.addColumnsToolStripMenuItem.Click += new System.EventHandler(this.addColumnsToolStripMenuItem_Click);
            // 
            // importFromToolStripMenuItem
            // 
            this.importFromToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromExcelFileToolStripMenuItem,
            this.fromMeshesToolStripMenuItem});
            this.importFromToolStripMenuItem.Name = "importFromToolStripMenuItem";
            this.importFromToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.importFromToolStripMenuItem.Text = "Import";
            // 
            // fromExcelFileToolStripMenuItem
            // 
            this.fromExcelFileToolStripMenuItem.Name = "fromExcelFileToolStripMenuItem";
            this.fromExcelFileToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.fromExcelFileToolStripMenuItem.Text = "From Excel File";
            this.fromExcelFileToolStripMenuItem.Click += new System.EventHandler(this.importFromExelFileMenuItem_Click);
            // 
            // fromMeshesToolStripMenuItem
            // 
            this.fromMeshesToolStripMenuItem.Name = "fromMeshesToolStripMenuItem";
            this.fromMeshesToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.fromMeshesToolStripMenuItem.Text = "From Meshes";
            this.fromMeshesToolStripMenuItem.Click += new System.EventHandler(this.fromMeshesToolStripMenuItem_Click);
            // 
            // expportToolStripMenuItem
            // 
            this.expportToolStripMenuItem.Name = "expportToolStripMenuItem";
            this.expportToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.expportToolStripMenuItem.Text = "Expport";
            // 
            // BoreholesWellDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 497);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BoreholesWellDataForm";
            this.Text = "WellDataForm";
            this.Load += new System.EventHandler(this.WellDataForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem dataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem traceAnglesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addColumnsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFromToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromExcelFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromMeshesToolStripMenuItem;
    }
}