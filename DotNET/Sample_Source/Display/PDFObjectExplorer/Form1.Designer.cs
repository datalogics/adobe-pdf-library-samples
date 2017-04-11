namespace PDF_Object_Explorer
{
    partial class mainWindow
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
            this.components = new System.ComponentModel.Container();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.OpenPDFButton = new System.Windows.Forms.ToolStripButton();
            this.RefreshButton = new System.Windows.Forms.ToolStripButton();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.OuterSplitContainer = new System.Windows.Forms.SplitContainer();
            this.mainTree = new System.Windows.Forms.TreeView();
            this.innerSplitContainer = new System.Windows.Forms.SplitContainer();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.streamViewer = new System.Windows.Forms.TabControl();
            this.streamViewRawTab = new System.Windows.Forms.TabPage();
            this.streamViewRaw = new System.Windows.Forms.TextBox();
            this.contextMenuForStreamView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewInHexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.streamViewCookedTab = new System.Windows.Forms.TabPage();
            this.streamViewCooked = new System.Windows.Forms.TextBox();
            this.mainToolStrip.SuspendLayout();
            this.OuterSplitContainer.Panel1.SuspendLayout();
            this.OuterSplitContainer.Panel2.SuspendLayout();
            this.OuterSplitContainer.SuspendLayout();
            this.innerSplitContainer.Panel1.SuspendLayout();
            this.innerSplitContainer.Panel2.SuspendLayout();
            this.innerSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.streamViewer.SuspendLayout();
            this.streamViewRawTab.SuspendLayout();
            this.contextMenuForStreamView.SuspendLayout();
            this.streamViewCookedTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenPDFButton,
            this.RefreshButton,
            this.saveButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(822, 25);
            this.mainToolStrip.TabIndex = 1;
            this.mainToolStrip.Text = "ToolBar";
            // 
            // OpenPDFButton
            // 
            this.OpenPDFButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenPDFButton.Image = global::PDF_Object_Explorer.Properties.Resources.FileOpen1616;
            this.OpenPDFButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenPDFButton.Name = "OpenPDFButton";
            this.OpenPDFButton.Size = new System.Drawing.Size(23, 22);
            this.OpenPDFButton.Text = "Open";
            this.OpenPDFButton.Click += new System.EventHandler(this.OpenPDFButton_Click);
            // 
            // RefreshButton
            // 
            this.RefreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RefreshButton.Image = global::PDF_Object_Explorer.Properties.Resources.Refresh3232;
            this.RefreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(23, 22);
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveButton.Image = global::PDF_Object_Explorer.Properties.Resources.save1616;
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(23, 22);
            this.saveButton.Text = "Save";
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // OuterSplitContainer
            // 
            this.OuterSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OuterSplitContainer.Location = new System.Drawing.Point(12, 28);
            this.OuterSplitContainer.Name = "OuterSplitContainer";
            // 
            // OuterSplitContainer.Panel1
            // 
            this.OuterSplitContainer.Panel1.Controls.Add(this.mainTree);
            this.OuterSplitContainer.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // OuterSplitContainer.Panel2
            // 
            this.OuterSplitContainer.Panel2.Controls.Add(this.innerSplitContainer);
            this.OuterSplitContainer.Size = new System.Drawing.Size(798, 420);
            this.OuterSplitContainer.SplitterDistance = 487;
            this.OuterSplitContainer.TabIndex = 2;
            // 
            // mainTree
            // 
            this.mainTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTree.LabelEdit = true;
            this.mainTree.Location = new System.Drawing.Point(0, 0);
            this.mainTree.Name = "mainTree";
            this.mainTree.Size = new System.Drawing.Size(487, 420);
            this.mainTree.TabIndex = 1;
            this.mainTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.mainTree_BeforeExpand);
            this.mainTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.mainTree_AfterSelect);
            // 
            // innerSplitContainer
            // 
            this.innerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.innerSplitContainer.Name = "innerSplitContainer";
            this.innerSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // innerSplitContainer.Panel1
            // 
            this.innerSplitContainer.Panel1.Controls.Add(this.dataGridView);
            // 
            // innerSplitContainer.Panel2
            // 
            this.innerSplitContainer.Panel2.Controls.Add(this.streamViewer);
            this.innerSplitContainer.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.innerSplitContainer.Size = new System.Drawing.Size(307, 420);
            this.innerSplitContainer.SplitterDistance = 301;
            this.innerSplitContainer.TabIndex = 1;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView.Size = new System.Drawing.Size(307, 301);
            this.dataGridView.TabIndex = 3;
            this.dataGridView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView_CellBeginEdit);
            this.dataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellEndEdit);
            // 
            // streamViewer
            // 
            this.streamViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.streamViewer.Controls.Add(this.streamViewRawTab);
            this.streamViewer.Controls.Add(this.streamViewCookedTab);
            this.streamViewer.Location = new System.Drawing.Point(0, 0);
            this.streamViewer.Name = "streamViewer";
            this.streamViewer.SelectedIndex = 0;
            this.streamViewer.Size = new System.Drawing.Size(307, 115);
            this.streamViewer.TabIndex = 0;
            // 
            // streamViewRawTab
            // 
            this.streamViewRawTab.Controls.Add(this.streamViewRaw);
            this.streamViewRawTab.Location = new System.Drawing.Point(4, 22);
            this.streamViewRawTab.Name = "streamViewRawTab";
            this.streamViewRawTab.Padding = new System.Windows.Forms.Padding(3);
            this.streamViewRawTab.Size = new System.Drawing.Size(299, 89);
            this.streamViewRawTab.TabIndex = 0;
            this.streamViewRawTab.Text = "Unfiltered";
            this.streamViewRawTab.UseVisualStyleBackColor = true;
            // 
            // streamViewRaw
            // 
            this.streamViewRaw.ContextMenuStrip = this.contextMenuForStreamView;
            this.streamViewRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.streamViewRaw.Location = new System.Drawing.Point(3, 3);
            this.streamViewRaw.Margin = new System.Windows.Forms.Padding(0);
            this.streamViewRaw.Multiline = true;
            this.streamViewRaw.Name = "streamViewRaw";
            this.streamViewRaw.ReadOnly = true;
            this.streamViewRaw.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.streamViewRaw.Size = new System.Drawing.Size(293, 83);
            this.streamViewRaw.TabIndex = 0;
            this.streamViewRaw.WordWrap = false;
            // 
            // contextMenuForStreamView
            // 
            this.contextMenuForStreamView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.contextMenuForStreamView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewInHexToolStripMenuItem});
            this.contextMenuForStreamView.Name = "contextMenuForStreamView";
            this.contextMenuForStreamView.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuForStreamView.Size = new System.Drawing.Size(185, 26);
            // 
            // viewInHexToolStripMenuItem
            // 
            this.viewInHexToolStripMenuItem.Name = "viewInHexToolStripMenuItem";
            this.viewInHexToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.viewInHexToolStripMenuItem.Text = "Hex View On/Off";
            this.viewInHexToolStripMenuItem.Click += new System.EventHandler(this.viewInHexToolStripMenuItem_Click);
            // 
            // streamViewCookedTab
            // 
            this.streamViewCookedTab.Controls.Add(this.streamViewCooked);
            this.streamViewCookedTab.Location = new System.Drawing.Point(4, 22);
            this.streamViewCookedTab.Name = "streamViewCookedTab";
            this.streamViewCookedTab.Padding = new System.Windows.Forms.Padding(3);
            this.streamViewCookedTab.Size = new System.Drawing.Size(299, 89);
            this.streamViewCookedTab.TabIndex = 1;
            this.streamViewCookedTab.Text = "Filtered";
            this.streamViewCookedTab.UseVisualStyleBackColor = true;
            // 
            // streamViewCooked
            // 
            this.streamViewCooked.ContextMenuStrip = this.contextMenuForStreamView;
            this.streamViewCooked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.streamViewCooked.Location = new System.Drawing.Point(3, 3);
            this.streamViewCooked.Multiline = true;
            this.streamViewCooked.Name = "streamViewCooked";
            this.streamViewCooked.ReadOnly = true;
            this.streamViewCooked.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.streamViewCooked.Size = new System.Drawing.Size(293, 83);
            this.streamViewCooked.TabIndex = 0;
            this.streamViewCooked.WordWrap = false;
            // 
            // mainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 460);
            this.Controls.Add(this.OuterSplitContainer);
            this.Controls.Add(this.mainToolStrip);
            this.Name = "mainWindow";
            this.Text = "PDF Object Explorer";
            this.Load += new System.EventHandler(this.mainWindow_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainWindow_FormClosing);
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.OuterSplitContainer.Panel1.ResumeLayout(false);
            this.OuterSplitContainer.Panel2.ResumeLayout(false);
            this.OuterSplitContainer.ResumeLayout(false);
            this.innerSplitContainer.Panel1.ResumeLayout(false);
            this.innerSplitContainer.Panel2.ResumeLayout(false);
            this.innerSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.streamViewer.ResumeLayout(false);
            this.streamViewRawTab.ResumeLayout(false);
            this.streamViewRawTab.PerformLayout();
            this.contextMenuForStreamView.ResumeLayout(false);
            this.streamViewCookedTab.ResumeLayout(false);
            this.streamViewCookedTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton OpenPDFButton;
        private System.Windows.Forms.SplitContainer OuterSplitContainer;
        private System.Windows.Forms.TreeView mainTree;
        private System.Windows.Forms.TabControl streamViewer;
        private System.Windows.Forms.TabPage streamViewRawTab;
        private System.Windows.Forms.TabPage streamViewCookedTab;
        private System.Windows.Forms.TextBox streamViewRaw;
        private System.Windows.Forms.TextBox streamViewCooked;
        private System.Windows.Forms.ToolStripButton RefreshButton;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuForStreamView;
        private System.Windows.Forms.ToolStripMenuItem viewInHexToolStripMenuItem;
        private System.Windows.Forms.SplitContainer innerSplitContainer;
    }
}

