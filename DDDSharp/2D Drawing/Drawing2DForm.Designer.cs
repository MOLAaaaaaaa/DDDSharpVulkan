namespace DDDSharp
{
    partial class Drawing2DForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.定位ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTracesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOutlinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLayersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.faultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polygonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPropertyGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.snapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.geoObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graphicsObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.griddingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridSampleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataRangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonArrow = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2ZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3ZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4Pan = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5Reset = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFill = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonLocate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonMove = new System.Windows.Forms.ToolStripButton();
            this.UndoToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.RedoToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.curPositionLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileToolStripMenuItem,
            this.定位ToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.drawToolStripMenuItem,
            this.griddingToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(904, 30);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileToolStripMenuItem
            // 
            this.FileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.FileToolStripMenuItem.Name = "FileToolStripMenuItem";
            this.FileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.FileToolStripMenuItem.Text = "&File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.loadToolStripMenuItem.Text = "&Load";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.saveToolStripMenuItem.Text = "&Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            // 
            // 定位ToolStripMenuItem
            // 
            this.定位ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTracesToolStripMenuItem});
            this.定位ToolStripMenuItem.Name = "定位ToolStripMenuItem";
            this.定位ToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.定位ToolStripMenuItem.Text = "&Trace";
            // 
            // addTracesToolStripMenuItem
            // 
            this.addTracesToolStripMenuItem.Name = "addTracesToolStripMenuItem";
            this.addTracesToolStripMenuItem.Size = new System.Drawing.Size(165, 26);
            this.addTracesToolStripMenuItem.Text = "&Add Traces";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showPointToolStripMenuItem,
            this.smoothLineToolStripMenuItem,
            this.showLocationToolStripMenuItem,
            this.showOutlinesToolStripMenuItem,
            this.showLayersToolStripMenuItem,
            this.showPropertyGridToolStripMenuItem,
            this.resetViewToolStripMenuItem,
            this.snapToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // showPointToolStripMenuItem
            // 
            this.showPointToolStripMenuItem.Name = "showPointToolStripMenuItem";
            this.showPointToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.showPointToolStripMenuItem.Text = "Show Grid Point";
            // 
            // smoothLineToolStripMenuItem
            // 
            this.smoothLineToolStripMenuItem.Name = "smoothLineToolStripMenuItem";
            this.smoothLineToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.smoothLineToolStripMenuItem.Text = "Smooth Line";
            // 
            // showLocationToolStripMenuItem
            // 
            this.showLocationToolStripMenuItem.Name = "showLocationToolStripMenuItem";
            this.showLocationToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.showLocationToolStripMenuItem.Text = "Show Location";
            // 
            // showOutlinesToolStripMenuItem
            // 
            this.showOutlinesToolStripMenuItem.Name = "showOutlinesToolStripMenuItem";
            this.showOutlinesToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.showOutlinesToolStripMenuItem.Text = "Show Background";
            // 
            // showLayersToolStripMenuItem
            // 
            this.showLayersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.faultToolStripMenuItem,
            this.polygonToolStripMenuItem});
            this.showLayersToolStripMenuItem.Name = "showLayersToolStripMenuItem";
            this.showLayersToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.showLayersToolStripMenuItem.Text = "Show Layers";
            // 
            // faultToolStripMenuItem
            // 
            this.faultToolStripMenuItem.Name = "faultToolStripMenuItem";
            this.faultToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.faultToolStripMenuItem.Text = "Line";
            // 
            // polygonToolStripMenuItem
            // 
            this.polygonToolStripMenuItem.Name = "polygonToolStripMenuItem";
            this.polygonToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.polygonToolStripMenuItem.Text = "Polygon";
            // 
            // showPropertyGridToolStripMenuItem
            // 
            this.showPropertyGridToolStripMenuItem.Name = "showPropertyGridToolStripMenuItem";
            this.showPropertyGridToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.showPropertyGridToolStripMenuItem.Text = "Show Property Grid";
            // 
            // resetViewToolStripMenuItem
            // 
            this.resetViewToolStripMenuItem.Name = "resetViewToolStripMenuItem";
            this.resetViewToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.resetViewToolStripMenuItem.Text = "Reset View";
            // 
            // snapToolStripMenuItem
            // 
            this.snapToolStripMenuItem.Name = "snapToolStripMenuItem";
            this.snapToolStripMenuItem.Size = new System.Drawing.Size(220, 26);
            this.snapToolStripMenuItem.Text = "&Snap";
            // 
            // drawToolStripMenuItem
            // 
            this.drawToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.geoObjectsToolStripMenuItem,
            this.graphicsObjectsToolStripMenuItem});
            this.drawToolStripMenuItem.Name = "drawToolStripMenuItem";
            this.drawToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.drawToolStripMenuItem.Text = "&Draw";
            // 
            // geoObjectsToolStripMenuItem
            // 
            this.geoObjectsToolStripMenuItem.Name = "geoObjectsToolStripMenuItem";
            this.geoObjectsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.geoObjectsToolStripMenuItem.Text = "&GeoObjects";
            // 
            // graphicsObjectsToolStripMenuItem
            // 
            this.graphicsObjectsToolStripMenuItem.Name = "graphicsObjectsToolStripMenuItem";
            this.graphicsObjectsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.graphicsObjectsToolStripMenuItem.Text = "Graphics Objects";
            // 
            // griddingToolStripMenuItem
            // 
            this.griddingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gridSampleToolStripMenuItem});
            this.griddingToolStripMenuItem.Name = "griddingToolStripMenuItem";
            this.griddingToolStripMenuItem.Size = new System.Drawing.Size(81, 24);
            this.griddingToolStripMenuItem.Text = "Gridding";
            // 
            // gridSampleToolStripMenuItem
            // 
            this.gridSampleToolStripMenuItem.Name = "gridSampleToolStripMenuItem";
            this.gridSampleToolStripMenuItem.Size = new System.Drawing.Size(174, 26);
            this.gridSampleToolStripMenuItem.Text = "Grid Sample";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dataRangeToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(75, 24);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // dataRangeToolStripMenuItem
            // 
            this.dataRangeToolStripMenuItem.Name = "dataRangeToolStripMenuItem";
            this.dataRangeToolStripMenuItem.Size = new System.Drawing.Size(166, 26);
            this.dataRangeToolStripMenuItem.Text = "DataRange";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonArrow,
            this.toolStripButton2ZoomIn,
            this.toolStripButton3ZoomOut,
            this.toolStripButton4Pan,
            this.toolStripButton5Reset,
            this.toolStripButtonFill,
            this.toolStripButtonLocate,
            this.toolStripButtonMove,
            this.UndoToolStripButton,
            this.RedoToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 30);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(904, 31);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonArrow
            // 
            this.toolStripButtonArrow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonArrow.Image = global::DDDSharp.Properties.Resources.Arrow24;
            this.toolStripButtonArrow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonArrow.Name = "toolStripButtonArrow";
            this.toolStripButtonArrow.Size = new System.Drawing.Size(29, 24);
            this.toolStripButtonArrow.Text = "Select tool";
            this.toolStripButtonArrow.ToolTipText = "Select Tool";
            // 
            // toolStripButton2ZoomIn
            // 
            this.toolStripButton2ZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2ZoomIn.Image = global::DDDSharp.Properties.Resources.zoom_in;
            this.toolStripButton2ZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2ZoomIn.Name = "toolStripButton2ZoomIn";
            this.toolStripButton2ZoomIn.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton2ZoomIn.Text = "Zoom In";
            this.toolStripButton2ZoomIn.ToolTipText = "Zoom In";
            // 
            // toolStripButton3ZoomOut
            // 
            this.toolStripButton3ZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3ZoomOut.Image = global::DDDSharp.Properties.Resources.zoom_out;
            this.toolStripButton3ZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3ZoomOut.Name = "toolStripButton3ZoomOut";
            this.toolStripButton3ZoomOut.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton3ZoomOut.Text = "Zoom Out";
            this.toolStripButton3ZoomOut.ToolTipText = "Zoom Out";
            // 
            // toolStripButton4Pan
            // 
            this.toolStripButton4Pan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4Pan.Image = global::DDDSharp.Properties.Resources.Hand;
            this.toolStripButton4Pan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4Pan.Name = "toolStripButton4Pan";
            this.toolStripButton4Pan.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton4Pan.Text = "Pan Screen";
            this.toolStripButton4Pan.ToolTipText = "Pan Screen";
            // 
            // toolStripButton5Reset
            // 
            this.toolStripButton5Reset.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5Reset.Image = global::DDDSharp.Properties.Resources.Reset24;
            this.toolStripButton5Reset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5Reset.Name = "toolStripButton5Reset";
            this.toolStripButton5Reset.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton5Reset.Text = "Reset View";
            this.toolStripButton5Reset.ToolTipText = " Reset view to fit screen";
            // 
            // toolStripButtonFill
            // 
            this.toolStripButtonFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFill.Image = global::DDDSharp.Properties.Resources.Fill24;
            this.toolStripButtonFill.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFill.Name = "toolStripButtonFill";
            this.toolStripButtonFill.Size = new System.Drawing.Size(29, 24);
            this.toolStripButtonFill.Text = "toolStripButton1";
            this.toolStripButtonFill.ToolTipText = "Fill Tool";
            // 
            // toolStripButtonLocate
            // 
            this.toolStripButtonLocate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLocate.Image = global::DDDSharp.Properties.Resources.Locate24;
            this.toolStripButtonLocate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLocate.Name = "toolStripButtonLocate";
            this.toolStripButtonLocate.Size = new System.Drawing.Size(29, 24);
            this.toolStripButtonLocate.Text = "toolStripButton1";
            this.toolStripButtonLocate.ToolTipText = "Location";
            // 
            // toolStripButtonMove
            // 
            this.toolStripButtonMove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonMove.Image = global::DDDSharp.Properties.Resources.Move24;
            this.toolStripButtonMove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonMove.Name = "toolStripButtonMove";
            this.toolStripButtonMove.Size = new System.Drawing.Size(29, 24);
            this.toolStripButtonMove.Text = "toolStripButton1";
            this.toolStripButtonMove.ToolTipText = "Move Object";
            // 
            // UndoToolStripButton
            // 
            this.UndoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.UndoToolStripButton.Image = global::DDDSharp.Properties.Resources.undo24;
            this.UndoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.UndoToolStripButton.Name = "UndoToolStripButton";
            this.UndoToolStripButton.Size = new System.Drawing.Size(29, 24);
            this.UndoToolStripButton.Text = "toolStripButton1";
            // 
            // RedoToolStripButton
            // 
            this.RedoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RedoToolStripButton.Image = global::DDDSharp.Properties.Resources.redo24;
            this.RedoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RedoToolStripButton.Name = "RedoToolStripButton";
            this.RedoToolStripButton.Size = new System.Drawing.Size(29, 24);
            this.RedoToolStripButton.Text = "toolStripButton2";
            // 
            // curPositionLabel
            // 
            this.curPositionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.curPositionLabel.AutoSize = true;
            this.curPositionLabel.Location = new System.Drawing.Point(12, 442);
            this.curPositionLabel.Name = "curPositionLabel";
            this.curPositionLabel.Size = new System.Drawing.Size(135, 15);
            this.curPositionLabel.TabIndex = 4;
            this.curPositionLabel.Text = "current position";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(12, 58);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(880, 381);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // Drawing2DForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 466);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.curPositionLabel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Drawing2DForm";
            this.Text = "Drawing2D";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

       
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 定位ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTracesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPointToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOutlinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showLayersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem faultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polygonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPropertyGridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem snapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem geoObjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem graphicsObjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem griddingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridSampleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataRangeToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonArrow;
        private System.Windows.Forms.ToolStripButton toolStripButton2ZoomIn;
        private System.Windows.Forms.ToolStripButton toolStripButton3ZoomOut;
        private System.Windows.Forms.ToolStripButton toolStripButton4Pan;
        private System.Windows.Forms.ToolStripButton toolStripButton5Reset;
        private System.Windows.Forms.ToolStripButton toolStripButtonFill;
        private System.Windows.Forms.ToolStripButton toolStripButtonLocate;
        private System.Windows.Forms.ToolStripButton toolStripButtonMove;
        private System.Windows.Forms.ToolStripButton UndoToolStripButton;
        private System.Windows.Forms.ToolStripButton RedoToolStripButton;
        private System.Windows.Forms.Label curPositionLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}