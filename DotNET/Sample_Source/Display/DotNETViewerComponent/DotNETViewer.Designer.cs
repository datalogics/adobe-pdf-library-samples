namespace DotNETViewerComponent
{
    partial class DotNETViewer
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
                disableControls();
                userInterfaceControls.Dipose();

                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.appendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.unlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewPrintModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.continuousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singlePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monitorResolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBookmarkLayersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createDocToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.test1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainToolStrip = new System.Windows.Forms.ToolStrip();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.printToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.firstPageToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.previousPageToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.selectedPageNumberToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.pageNumberToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.nextPageToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.lastPageToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.decreaseZoomToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.zoomLevelToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.increaseZoomToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.freeTextToolStripDropDownButton = new System.Windows.Forms.ToolStripSplitButton();
            this.fontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.justificationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.centerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownAnnotationButton = new System.Windows.Forms.ToolStripSplitButton();
            this.ellipseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startArrowheadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowStartNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowStartOpenArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowStartClosedArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowStartCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.endArrowheadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowEndNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowEndOpenArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowEndClosedArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowEndCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rectangleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polygonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polylineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineStartArrowheadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineStartNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineStartOpenArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineStartClosedArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineStartCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineEndArrowheadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineEndNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineEndOpenArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineEndClosedArrowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polyLineEndCircleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.underlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownLineTypeButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.solidToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashed1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashed2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashed3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashed4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownLineWidthButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.lineWidth1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineWidth2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineWidth3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineWidth5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.otherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorPickerToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.selectModeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.zoomModeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.marqueeZoomToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.searchPhraseToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.searchToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ViewingSurfaceSplitContainer = new System.Windows.Forms.SplitContainer();
            this.BookmarkAndLayersTabControl = new System.Windows.Forms.TabControl();
            this.BookmarkTabPage = new System.Windows.Forms.TabPage();
            this.LayerTabPage = new System.Windows.Forms.TabPage();
            this.MainMenuStrip.SuspendLayout();
            this.MainToolStrip.SuspendLayout();
            this.ViewingSurfaceSplitContainer.Panel1.SuspendLayout();
            this.ViewingSurfaceSplitContainer.SuspendLayout();
            this.BookmarkAndLayersTabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenuStrip
            // 
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.testsToolStripMenuItem});
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "MainMenuStrip";
            this.MainMenuStrip.Size = new System.Drawing.Size(1033, 24);
            this.MainMenuStrip.TabIndex = 0;
            this.MainMenuStrip.Text = "DotNETViewer Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator6,
            this.appendToolStripMenuItem,
            this.printToolStripMenuItem,
            this.toolStripSeparator7,
            this.unlockToolStripMenuItem,
            this.toolStripSeparator8,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + O";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + S";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(155, 6);
            // 
            // appendToolStripMenuItem
            // 
            this.appendToolStripMenuItem.Name = "appendToolStripMenuItem";
            this.appendToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.appendToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.appendToolStripMenuItem.Text = "Append";
            this.appendToolStripMenuItem.Click += new System.EventHandler(this.appendToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + P";
            this.printToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.printToolStripMenuItem.Text = "&Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(155, 6);
            // 
            // unlockToolStripMenuItem
            // 
            this.unlockToolStripMenuItem.Name = "unlockToolStripMenuItem";
            this.unlockToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.unlockToolStripMenuItem.Text = "Unlock";
            this.unlockToolStripMenuItem.Click += new System.EventHandler(this.unlockToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(155, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + C";
            this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.closeToolStripMenuItem.Text = "C&lose";
            this.closeToolStripMenuItem.ToolTipText = "Close the Current Document";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.E)));
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.deleteToolStripMenuItem.Text = "&Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewPrintModeToolStripMenuItem,
            this.monitorResolutionToolStripMenuItem,
            this.showBookmarkLayersToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O)));
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // viewPrintModeToolStripMenuItem
            // 
            this.viewPrintModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.continuousToolStripMenuItem,
            this.singlePageToolStripMenuItem});
            this.viewPrintModeToolStripMenuItem.Name = "viewPrintModeToolStripMenuItem";
            this.viewPrintModeToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.viewPrintModeToolStripMenuItem.Text = "View/Print &Mode";
            // 
            // continuousToolStripMenuItem
            // 
            this.continuousToolStripMenuItem.Name = "continuousToolStripMenuItem";
            this.continuousToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + Alt + C";
            this.continuousToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt)
                        | System.Windows.Forms.Keys.C)));
            this.continuousToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.continuousToolStripMenuItem.Text = "Continuous";
            this.continuousToolStripMenuItem.Click += new System.EventHandler(this.continuousToolStripMenuItem_Click);
            // 
            // singlePageToolStripMenuItem
            // 
            this.singlePageToolStripMenuItem.Checked = true;
            this.singlePageToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.singlePageToolStripMenuItem.Name = "singlePageToolStripMenuItem";
            this.singlePageToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + Alt + S";
            this.singlePageToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt)
                        | System.Windows.Forms.Keys.S)));
            this.singlePageToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this.singlePageToolStripMenuItem.Text = "Single Page";
            this.singlePageToolStripMenuItem.Click += new System.EventHandler(this.singlePageToolStripMenuItem_Click);
            // 
            // monitorResolutionToolStripMenuItem
            // 
            this.monitorResolutionToolStripMenuItem.Name = "monitorResolutionToolStripMenuItem";
            this.monitorResolutionToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.monitorResolutionToolStripMenuItem.Text = "Monitor &Resolution";
            this.monitorResolutionToolStripMenuItem.Click += new System.EventHandler(this.monitorResolutionToolStripMenuItem_Click);
            // 
            // showBookmarkLayersToolStripMenuItem
            // 
            this.showBookmarkLayersToolStripMenuItem.Checked = true;
            this.showBookmarkLayersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBookmarkLayersToolStripMenuItem.Name = "showBookmarkLayersToolStripMenuItem";
            this.showBookmarkLayersToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + Alt + P";
            this.showBookmarkLayersToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt)
                        | System.Windows.Forms.Keys.P)));
            this.showBookmarkLayersToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.showBookmarkLayersToolStripMenuItem.Text = "Show Bookmark/Layer &Panel";
            this.showBookmarkLayersToolStripMenuItem.Click += new System.EventHandler(this.showBookmarkLayersToolStripMenuItem_Click);
            // 
            // testsToolStripMenuItem
            // 
            this.testsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createDocToolStripMenuItem,
            this.test1ToolStripMenuItem});
            this.testsToolStripMenuItem.Name = "testsToolStripMenuItem";
            this.testsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.testsToolStripMenuItem.Text = "Tests";
            // 
            // createDocToolStripMenuItem
            // 
            this.createDocToolStripMenuItem.Name = "createDocToolStripMenuItem";
            this.createDocToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.createDocToolStripMenuItem.Text = "Create doc";
            this.createDocToolStripMenuItem.Click += new System.EventHandler(this.createDocToolStripMenuItem_Click);
            // 
            // test1ToolStripMenuItem
            // 
            this.test1ToolStripMenuItem.Name = "test1ToolStripMenuItem";
            this.test1ToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.test1ToolStripMenuItem.Text = "Test annotations";
            this.test1ToolStripMenuItem.Click += new System.EventHandler(this.test1ToolStripMenuItem_Click);
            // 
            // MainToolStrip
            // 
            this.MainToolStrip.AutoSize = false;
            this.MainToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripButton,
            this.saveToolStripButton,
            this.printToolStripButton,
            this.toolStripSeparator1,
            this.firstPageToolStripButton,
            this.previousPageToolStripButton,
            this.selectedPageNumberToolStripTextBox,
            this.pageNumberToolStripLabel,
            this.nextPageToolStripButton,
            this.lastPageToolStripButton,
            this.toolStripSeparator2,
            this.decreaseZoomToolStripButton,
            this.zoomLevelToolStripComboBox,
            this.increaseZoomToolStripButton,
            this.toolStripSeparator3,
            this.freeTextToolStripDropDownButton,
            this.toolStripDropDownAnnotationButton,
            this.toolStripDropDownLineTypeButton,
            this.toolStripDropDownLineWidthButton,
            this.colorPickerToolStripButton,
            this.toolStripSeparator4,
            this.selectModeToolStripButton,
            this.zoomModeToolStripButton,
            this.marqueeZoomToolStripButton,
            this.toolStripSeparator5,
            this.searchPhraseToolStripTextBox,
            this.searchToolStripButton});
            this.MainToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.MainToolStrip.Location = new System.Drawing.Point(0, 24);
            this.MainToolStrip.Name = "MainToolStrip";
            this.MainToolStrip.Size = new System.Drawing.Size(1033, 36);
            this.MainToolStrip.Stretch = true;
            this.MainToolStrip.TabIndex = 1;
            this.MainToolStrip.Text = "Document Tools";
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.AutoSize = false;
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.FileOpenMenu1;
            this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(45, 45);
            this.openToolStripButton.ToolTipText = "Open a document";
            this.openToolStripButton.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.FileSaveMenu1;
            this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.saveToolStripButton.ToolTipText = "Save a document";
            this.saveToolStripButton.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // printToolStripButton
            // 
            this.printToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.PrintMenu1;
            this.printToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.printToolStripButton.Name = "printToolStripButton";
            this.printToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.printToolStripButton.ToolTipText = "Print a document";
            this.printToolStripButton.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 36);
            // 
            // firstPageToolStripButton
            // 
            this.firstPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.firstPageToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.FirstPageMenu1;
            this.firstPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.firstPageToolStripButton.Name = "firstPageToolStripButton";
            this.firstPageToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.firstPageToolStripButton.ToolTipText = "First Page";
            this.firstPageToolStripButton.Click += new System.EventHandler(this.firstPageToolStripButton_Click);
            // 
            // previousPageToolStripButton
            // 
            this.previousPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.previousPageToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.PreviousPageMenu1;
            this.previousPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.previousPageToolStripButton.Name = "previousPageToolStripButton";
            this.previousPageToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.previousPageToolStripButton.ToolTipText = "Previous Page";
            this.previousPageToolStripButton.Click += new System.EventHandler(this.previousPageToolStripButton_Click);
            // 
            // selectedPageNumberToolStripTextBox
            // 
            this.selectedPageNumberToolStripTextBox.Name = "selectedPageNumberToolStripTextBox";
            this.selectedPageNumberToolStripTextBox.Size = new System.Drawing.Size(50, 36);
            this.selectedPageNumberToolStripTextBox.ToolTipText = "Current Page Number";
            this.selectedPageNumberToolStripTextBox.Leave += new System.EventHandler(this.selectedPageNumberToolStripTextBox_Leave);
            this.selectedPageNumberToolStripTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.selectedPageNumberToolStripTextBox_KeyDown);
            // 
            // pageNumberToolStripLabel
            // 
            this.pageNumberToolStripLabel.Name = "pageNumberToolStripLabel";
            this.pageNumberToolStripLabel.Size = new System.Drawing.Size(27, 33);
            this.pageNumberToolStripLabel.Text = "of 0";
            this.pageNumberToolStripLabel.ToolTipText = "Total Number of Pages";
            // 
            // nextPageToolStripButton
            // 
            this.nextPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nextPageToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.NextPageMenu1;
            this.nextPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.nextPageToolStripButton.Name = "nextPageToolStripButton";
            this.nextPageToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.nextPageToolStripButton.ToolTipText = "Next Page";
            this.nextPageToolStripButton.Click += new System.EventHandler(this.nextPageToolStripButton_Click);
            // 
            // lastPageToolStripButton
            // 
            this.lastPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.lastPageToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.LastPageMenu1;
            this.lastPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.lastPageToolStripButton.Name = "lastPageToolStripButton";
            this.lastPageToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.lastPageToolStripButton.ToolTipText = "Last Page";
            this.lastPageToolStripButton.Click += new System.EventHandler(this.lastPageToolStripButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 36);
            // 
            // decreaseZoomToolStripButton
            // 
            this.decreaseZoomToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.decreaseZoomToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.ZoomDecreaseMenu1;
            this.decreaseZoomToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.decreaseZoomToolStripButton.Name = "decreaseZoomToolStripButton";
            this.decreaseZoomToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.decreaseZoomToolStripButton.ToolTipText = "Decrease Zoom";
            this.decreaseZoomToolStripButton.Click += new System.EventHandler(this.decreaseZoomToolStripButton_Click);
            // 
            // zoomLevelToolStripComboBox
            // 
            this.zoomLevelToolStripComboBox.DropDownWidth = 90;
            this.zoomLevelToolStripComboBox.MaxDropDownItems = 20;
            this.zoomLevelToolStripComboBox.Name = "zoomLevelToolStripComboBox";
            this.zoomLevelToolStripComboBox.Size = new System.Drawing.Size(90, 36);
            this.zoomLevelToolStripComboBox.ToolTipText = "Select a Zoom Level";
            this.zoomLevelToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.zoomLevelToolStripComboBox_SelectedIndexChanged);
            this.zoomLevelToolStripComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.zoomLevelToolStripComboBox_KeyDown);
            // 
            // increaseZoomToolStripButton
            // 
            this.increaseZoomToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.increaseZoomToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.ZoomIncreaseMenu1;
            this.increaseZoomToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.increaseZoomToolStripButton.Name = "increaseZoomToolStripButton";
            this.increaseZoomToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.increaseZoomToolStripButton.ToolTipText = "Increase Zoom";
            this.increaseZoomToolStripButton.Click += new System.EventHandler(this.increaseZoomToolStripButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 36);
            // 
            // freeTextToolStripDropDownButton
            // 
            this.freeTextToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.freeTextToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fontToolStripMenuItem,
            this.sizeToolStripMenuItem,
            this.justificationToolStripMenuItem});
            this.freeTextToolStripDropDownButton.Image = global::DotNETViewerComponent.Properties.Resources.FreeTextMenu2;
            this.freeTextToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.freeTextToolStripDropDownButton.Name = "freeTextToolStripDropDownButton";
            this.freeTextToolStripDropDownButton.Size = new System.Drawing.Size(48, 33);
            this.freeTextToolStripDropDownButton.ToolTipText = "Create Free Text Annotation";
            this.freeTextToolStripDropDownButton.ButtonClick += new System.EventHandler(this.freeTextToolStripDropDownButton_ButtonClick);
            // 
            // fontToolStripMenuItem
            // 
            this.fontToolStripMenuItem.Name = "fontToolStripMenuItem";
            this.fontToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.fontToolStripMenuItem.Text = "Font";
            this.fontToolStripMenuItem.ToolTipText = "Select a Font";
            // 
            // sizeToolStripMenuItem
            // 
            this.sizeToolStripMenuItem.Name = "sizeToolStripMenuItem";
            this.sizeToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.sizeToolStripMenuItem.Text = "Size";
            this.sizeToolStripMenuItem.ToolTipText = "Select a Font Size";
            // 
            // justificationToolStripMenuItem
            // 
            this.justificationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.leftToolStripMenuItem,
            this.centerToolStripMenuItem,
            this.rightToolStripMenuItem});
            this.justificationToolStripMenuItem.Name = "justificationToolStripMenuItem";
            this.justificationToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.justificationToolStripMenuItem.Text = "Justification";
            this.justificationToolStripMenuItem.ToolTipText = "Select a Font Alignment";
            // 
            // leftToolStripMenuItem
            // 
            this.leftToolStripMenuItem.Checked = true;
            this.leftToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.leftToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.leftToolStripMenuItem.Name = "leftToolStripMenuItem";
            this.leftToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.leftToolStripMenuItem.Text = "Left";
            this.leftToolStripMenuItem.Click += new System.EventHandler(this.leftToolStripMenuItem_Click);
            // 
            // centerToolStripMenuItem
            // 
            this.centerToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.centerToolStripMenuItem.Name = "centerToolStripMenuItem";
            this.centerToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.centerToolStripMenuItem.Text = "Center";
            this.centerToolStripMenuItem.Click += new System.EventHandler(this.centerToolStripMenuItem_Click);
            // 
            // rightToolStripMenuItem
            // 
            this.rightToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.rightToolStripMenuItem.Name = "rightToolStripMenuItem";
            this.rightToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.rightToolStripMenuItem.Text = "Right";
            this.rightToolStripMenuItem.Click += new System.EventHandler(this.rightToolStripMenuItem_Click);
            // 
            // toolStripDropDownAnnotationButton
            // 
            this.toolStripDropDownAnnotationButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownAnnotationButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ellipseToolStripMenuItem,
            this.lineToolStripMenuItem,
            this.arrowToolStripMenuItem,
            this.rectangleToolStripMenuItem,
            this.polygonToolStripMenuItem,
            this.polylineToolStripMenuItem,
            this.linkToolStripMenuItem,
            this.inkToolStripMenuItem,
            this.underlineToolStripMenuItem,
            this.highlightToolStripMenuItem});
            this.toolStripDropDownAnnotationButton.Image = global::DotNETViewerComponent.Properties.Resources.EllipseMenuChoice1;
            this.toolStripDropDownAnnotationButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripDropDownAnnotationButton.Name = "toolStripDropDownAnnotationButton";
            this.toolStripDropDownAnnotationButton.Size = new System.Drawing.Size(48, 33);
            this.toolStripDropDownAnnotationButton.Text = "toolStripDropDownButton1";
            this.toolStripDropDownAnnotationButton.ToolTipText = "Select an Annotation Shape";
            this.toolStripDropDownAnnotationButton.ButtonClick += new System.EventHandler(this.toolStripDropDownAnnotationButton_ButtonClick);
            // 
            // ellipseToolStripMenuItem
            // 
            this.ellipseToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.EllipseMenuChoice1;
            this.ellipseToolStripMenuItem.Name = "ellipseToolStripMenuItem";
            this.ellipseToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.ellipseToolStripMenuItem.Text = "Ellipse";
            this.ellipseToolStripMenuItem.Click += new System.EventHandler(this.ellipseToolStripMenuItem_Click);
            // 
            // lineToolStripMenuItem
            // 
            this.lineToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.LineMenuChoice1;
            this.lineToolStripMenuItem.Name = "lineToolStripMenuItem";
            this.lineToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.lineToolStripMenuItem.Text = "Line";
            this.lineToolStripMenuItem.Click += new System.EventHandler(this.lineToolStripMenuItem_Click);
            // 
            // arrowToolStripMenuItem
            // 
            this.arrowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startArrowheadToolStripMenuItem,
            this.endArrowheadToolStripMenuItem});
            this.arrowToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.ArrowMenuChoice1;
            this.arrowToolStripMenuItem.Name = "arrowToolStripMenuItem";
            this.arrowToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.arrowToolStripMenuItem.Text = "Arrow";
            this.arrowToolStripMenuItem.Click += new System.EventHandler(this.arrowToolStripMenuItem_Click);
            // 
            // startArrowheadToolStripMenuItem
            // 
            this.startArrowheadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.arrowStartNoneToolStripMenuItem,
            this.arrowStartOpenArrowToolStripMenuItem,
            this.arrowStartClosedArrowToolStripMenuItem,
            this.arrowStartCircleToolStripMenuItem});
            this.startArrowheadToolStripMenuItem.Name = "startArrowheadToolStripMenuItem";
            this.startArrowheadToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.startArrowheadToolStripMenuItem.Text = "Start Arrowhead";
            this.startArrowheadToolStripMenuItem.ToolTipText = "Select an Arrowhead to display at the start of the line";
            // 
            // arrowStartNoneToolStripMenuItem
            // 
            this.arrowStartNoneToolStripMenuItem.Checked = true;
            this.arrowStartNoneToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.arrowStartNoneToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.arrowStartNoneToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowStartNoneToolStripMenuItem.Name = "arrowStartNoneToolStripMenuItem";
            this.arrowStartNoneToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowStartNoneToolStripMenuItem.Text = "None";
            this.arrowStartNoneToolStripMenuItem.Click += new System.EventHandler(this.StartNoneToolStripMenuItem_Click);
            // 
            // arrowStartOpenArrowToolStripMenuItem
            // 
            this.arrowStartOpenArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowStartOpenArrowToolStripMenuItem.Name = "arrowStartOpenArrowToolStripMenuItem";
            this.arrowStartOpenArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowStartOpenArrowToolStripMenuItem.Text = "Open Arrow";
            this.arrowStartOpenArrowToolStripMenuItem.Click += new System.EventHandler(this.StartOpenArrowToolStripMenuItem_Click);
            // 
            // arrowStartClosedArrowToolStripMenuItem
            // 
            this.arrowStartClosedArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowStartClosedArrowToolStripMenuItem.Name = "arrowStartClosedArrowToolStripMenuItem";
            this.arrowStartClosedArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowStartClosedArrowToolStripMenuItem.Text = "Closed Arrow";
            this.arrowStartClosedArrowToolStripMenuItem.Click += new System.EventHandler(this.StartClosedArrowToolStripMenuItem_Click);
            // 
            // arrowStartCircleToolStripMenuItem
            // 
            this.arrowStartCircleToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowStartCircleToolStripMenuItem.Name = "arrowStartCircleToolStripMenuItem";
            this.arrowStartCircleToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowStartCircleToolStripMenuItem.Text = "Circle";
            this.arrowStartCircleToolStripMenuItem.Click += new System.EventHandler(this.StartCircleToolStripMenuItem_Click);
            // 
            // endArrowheadToolStripMenuItem
            // 
            this.endArrowheadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.arrowEndNoneToolStripMenuItem,
            this.arrowEndOpenArrowToolStripMenuItem,
            this.arrowEndClosedArrowToolStripMenuItem,
            this.arrowEndCircleToolStripMenuItem});
            this.endArrowheadToolStripMenuItem.Name = "endArrowheadToolStripMenuItem";
            this.endArrowheadToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.endArrowheadToolStripMenuItem.Text = "End Arrowhead";
            this.endArrowheadToolStripMenuItem.ToolTipText = "Select an Arrowhead to display at the end of the line";
            // 
            // arrowEndNoneToolStripMenuItem
            // 
            this.arrowEndNoneToolStripMenuItem.Checked = true;
            this.arrowEndNoneToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.arrowEndNoneToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowEndNoneToolStripMenuItem.Name = "arrowEndNoneToolStripMenuItem";
            this.arrowEndNoneToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowEndNoneToolStripMenuItem.Text = "None";
            this.arrowEndNoneToolStripMenuItem.Click += new System.EventHandler(this.EndNoneToolStripMenuItem_Click);
            // 
            // arrowEndOpenArrowToolStripMenuItem
            // 
            this.arrowEndOpenArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowEndOpenArrowToolStripMenuItem.Name = "arrowEndOpenArrowToolStripMenuItem";
            this.arrowEndOpenArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowEndOpenArrowToolStripMenuItem.Text = "Open Arrow";
            this.arrowEndOpenArrowToolStripMenuItem.Click += new System.EventHandler(this.EndOpenArrowToolStripMenuItem_Click);
            // 
            // arrowEndClosedArrowToolStripMenuItem
            // 
            this.arrowEndClosedArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowEndClosedArrowToolStripMenuItem.Name = "arrowEndClosedArrowToolStripMenuItem";
            this.arrowEndClosedArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowEndClosedArrowToolStripMenuItem.Text = "Closed Arrow";
            this.arrowEndClosedArrowToolStripMenuItem.Click += new System.EventHandler(this.EndClosedArrowToolStripMenuItem_Click);
            // 
            // arrowEndCircleToolStripMenuItem
            // 
            this.arrowEndCircleToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.arrowEndCircleToolStripMenuItem.Name = "arrowEndCircleToolStripMenuItem";
            this.arrowEndCircleToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.arrowEndCircleToolStripMenuItem.Text = "Circle";
            this.arrowEndCircleToolStripMenuItem.Click += new System.EventHandler(this.EndCircleToolStripMenuItem_Click);
            // 
            // rectangleToolStripMenuItem
            // 
            this.rectangleToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.RectangleMenuChoice1;
            this.rectangleToolStripMenuItem.Name = "rectangleToolStripMenuItem";
            this.rectangleToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.rectangleToolStripMenuItem.Text = "Rectangle";
            this.rectangleToolStripMenuItem.Click += new System.EventHandler(this.rectangleToolStripMenuItem_Click);
            // 
            // polygonToolStripMenuItem
            // 
            this.polygonToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.PolygonMenu;
            this.polygonToolStripMenuItem.Name = "polygonToolStripMenuItem";
            this.polygonToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.polygonToolStripMenuItem.Text = "Polygon";
            this.polygonToolStripMenuItem.Click += new System.EventHandler(this.polygonToolStripMenuItem_Click);
            // 
            // polylineToolStripMenuItem
            // 
            this.polylineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.polyLineStartArrowheadToolStripMenuItem,
            this.polyLineEndArrowheadToolStripMenuItem});
            this.polylineToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.PolyLineMenu1;
            this.polylineToolStripMenuItem.Name = "polylineToolStripMenuItem";
            this.polylineToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.polylineToolStripMenuItem.Text = "Polyline";
            this.polylineToolStripMenuItem.Click += new System.EventHandler(this.polylineToolStripMenuItem_Click);
            // 
            // polyLineStartArrowheadToolStripMenuItem
            // 
            this.polyLineStartArrowheadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.polyLineStartNoneToolStripMenuItem,
            this.polyLineStartOpenArrowToolStripMenuItem,
            this.polyLineStartClosedArrowToolStripMenuItem,
            this.polyLineStartCircleToolStripMenuItem});
            this.polyLineStartArrowheadToolStripMenuItem.Name = "polyLineStartArrowheadToolStripMenuItem";
            this.polyLineStartArrowheadToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.polyLineStartArrowheadToolStripMenuItem.Text = "Start Arrowhead";
            this.polyLineStartArrowheadToolStripMenuItem.ToolTipText = "Select an Arrowhead to display at the start of the PolyLine";
            // 
            // polyLineStartNoneToolStripMenuItem
            // 
            this.polyLineStartNoneToolStripMenuItem.Checked = true;
            this.polyLineStartNoneToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.polyLineStartNoneToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineStartNoneToolStripMenuItem.Name = "polyLineStartNoneToolStripMenuItem";
            this.polyLineStartNoneToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineStartNoneToolStripMenuItem.Text = "None";
            this.polyLineStartNoneToolStripMenuItem.Click += new System.EventHandler(this.StartNoneToolStripMenuItem_Click);
            // 
            // polyLineStartOpenArrowToolStripMenuItem
            // 
            this.polyLineStartOpenArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineStartOpenArrowToolStripMenuItem.Name = "polyLineStartOpenArrowToolStripMenuItem";
            this.polyLineStartOpenArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineStartOpenArrowToolStripMenuItem.Text = "Open Arrow";
            this.polyLineStartOpenArrowToolStripMenuItem.Click += new System.EventHandler(this.StartOpenArrowToolStripMenuItem_Click);
            // 
            // polyLineStartClosedArrowToolStripMenuItem
            // 
            this.polyLineStartClosedArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineStartClosedArrowToolStripMenuItem.Name = "polyLineStartClosedArrowToolStripMenuItem";
            this.polyLineStartClosedArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineStartClosedArrowToolStripMenuItem.Text = "Closed Arrow";
            this.polyLineStartClosedArrowToolStripMenuItem.Click += new System.EventHandler(this.StartClosedArrowToolStripMenuItem_Click);
            // 
            // polyLineStartCircleToolStripMenuItem
            // 
            this.polyLineStartCircleToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineStartCircleToolStripMenuItem.Name = "polyLineStartCircleToolStripMenuItem";
            this.polyLineStartCircleToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineStartCircleToolStripMenuItem.Text = "Circle";
            this.polyLineStartCircleToolStripMenuItem.Click += new System.EventHandler(this.StartCircleToolStripMenuItem_Click);
            // 
            // polyLineEndArrowheadToolStripMenuItem
            // 
            this.polyLineEndArrowheadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.polyLineEndNoneToolStripMenuItem,
            this.polyLineEndOpenArrowToolStripMenuItem,
            this.polyLineEndClosedArrowToolStripMenuItem,
            this.polyLineEndCircleToolStripMenuItem});
            this.polyLineEndArrowheadToolStripMenuItem.Name = "polyLineEndArrowheadToolStripMenuItem";
            this.polyLineEndArrowheadToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.polyLineEndArrowheadToolStripMenuItem.Text = "End Arrowhead";
            this.polyLineEndArrowheadToolStripMenuItem.ToolTipText = "Select an Arrowhead to display at the end of the PolyLine";
            // 
            // polyLineEndNoneToolStripMenuItem
            // 
            this.polyLineEndNoneToolStripMenuItem.Checked = true;
            this.polyLineEndNoneToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.polyLineEndNoneToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineEndNoneToolStripMenuItem.Name = "polyLineEndNoneToolStripMenuItem";
            this.polyLineEndNoneToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineEndNoneToolStripMenuItem.Text = "None";
            this.polyLineEndNoneToolStripMenuItem.Click += new System.EventHandler(this.EndNoneToolStripMenuItem_Click);
            // 
            // polyLineEndOpenArrowToolStripMenuItem
            // 
            this.polyLineEndOpenArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineEndOpenArrowToolStripMenuItem.Name = "polyLineEndOpenArrowToolStripMenuItem";
            this.polyLineEndOpenArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineEndOpenArrowToolStripMenuItem.Text = "Open Arrow";
            this.polyLineEndOpenArrowToolStripMenuItem.Click += new System.EventHandler(this.EndOpenArrowToolStripMenuItem_Click);
            // 
            // polyLineEndClosedArrowToolStripMenuItem
            // 
            this.polyLineEndClosedArrowToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineEndClosedArrowToolStripMenuItem.Name = "polyLineEndClosedArrowToolStripMenuItem";
            this.polyLineEndClosedArrowToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineEndClosedArrowToolStripMenuItem.Text = "Closed Arrow";
            this.polyLineEndClosedArrowToolStripMenuItem.Click += new System.EventHandler(this.EndClosedArrowToolStripMenuItem_Click);
            // 
            // polyLineEndCircleToolStripMenuItem
            // 
            this.polyLineEndCircleToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.polyLineEndCircleToolStripMenuItem.Name = "polyLineEndCircleToolStripMenuItem";
            this.polyLineEndCircleToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.polyLineEndCircleToolStripMenuItem.Text = "Circle";
            this.polyLineEndCircleToolStripMenuItem.Click += new System.EventHandler(this.EndCircleToolStripMenuItem_Click);
            // 
            // linkToolStripMenuItem
            // 
            this.linkToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Link;
            this.linkToolStripMenuItem.Name = "linkToolStripMenuItem";
            this.linkToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.linkToolStripMenuItem.Text = "Link";
            this.linkToolStripMenuItem.Click += new System.EventHandler(this.linkToolStripMenuItem_Click);
            // 
            // inkToolStripMenuItem
            // 
            this.inkToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Ink;
            this.inkToolStripMenuItem.Name = "inkToolStripMenuItem";
            this.inkToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.inkToolStripMenuItem.Text = "Ink";
            this.inkToolStripMenuItem.Click += new System.EventHandler(this.inkToolStripMenuItem_Click);
            // 
            // underlineToolStripMenuItem
            // 
            this.underlineToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Underline;
            this.underlineToolStripMenuItem.Name = "underlineToolStripMenuItem";
            this.underlineToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.underlineToolStripMenuItem.Text = "Underline";
            this.underlineToolStripMenuItem.Click += new System.EventHandler(this.underlineToolStripMenuItem_Click);
            // 
            // highlightToolStripMenuItem
            // 
            this.highlightToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Highlight;
            this.highlightToolStripMenuItem.Name = "highlightToolStripMenuItem";
            this.highlightToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.highlightToolStripMenuItem.Text = "Highlight";
            this.highlightToolStripMenuItem.Click += new System.EventHandler(this.highlightToolStripMenuItem_Click);
            // 
            // toolStripDropDownLineTypeButton
            // 
            this.toolStripDropDownLineTypeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownLineTypeButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.solidToolStripMenuItem,
            this.dashed1ToolStripMenuItem,
            this.dashed2ToolStripMenuItem,
            this.dashed3ToolStripMenuItem,
            this.dashed4ToolStripMenuItem});
            this.toolStripDropDownLineTypeButton.Image = global::DotNETViewerComponent.Properties.Resources.SolidLineMenuChoice1;
            this.toolStripDropDownLineTypeButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripDropDownLineTypeButton.Name = "toolStripDropDownLineTypeButton";
            this.toolStripDropDownLineTypeButton.Size = new System.Drawing.Size(45, 33);
            this.toolStripDropDownLineTypeButton.Text = "toolStripDropDownButton2";
            this.toolStripDropDownLineTypeButton.ToolTipText = "Select a line pattern";
            // 
            // solidToolStripMenuItem
            // 
            this.solidToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.SolidLineMenuChoice1;
            this.solidToolStripMenuItem.Name = "solidToolStripMenuItem";
            this.solidToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.solidToolStripMenuItem.Text = "Solid";
            this.solidToolStripMenuItem.Click += new System.EventHandler(this.solidToolStripMenuItem_Click);
            // 
            // dashed1ToolStripMenuItem
            // 
            this.dashed1ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Dashed1LineMenuChoice1;
            this.dashed1ToolStripMenuItem.Name = "dashed1ToolStripMenuItem";
            this.dashed1ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.dashed1ToolStripMenuItem.Text = "Dashed 1";
            this.dashed1ToolStripMenuItem.Click += new System.EventHandler(this.dashed1ToolStripMenuItem_Click);
            // 
            // dashed2ToolStripMenuItem
            // 
            this.dashed2ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Dashed2LineMenuChoice1;
            this.dashed2ToolStripMenuItem.Name = "dashed2ToolStripMenuItem";
            this.dashed2ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.dashed2ToolStripMenuItem.Text = "Dashed 2";
            this.dashed2ToolStripMenuItem.Click += new System.EventHandler(this.dashed2ToolStripMenuItem_Click);
            // 
            // dashed3ToolStripMenuItem
            // 
            this.dashed3ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Dashed3LineMenuChoice1;
            this.dashed3ToolStripMenuItem.Name = "dashed3ToolStripMenuItem";
            this.dashed3ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.dashed3ToolStripMenuItem.Text = "Dashed 3";
            this.dashed3ToolStripMenuItem.Click += new System.EventHandler(this.dashed3ToolStripMenuItem_Click);
            // 
            // dashed4ToolStripMenuItem
            // 
            this.dashed4ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.Dashed4LineMenuChoice1;
            this.dashed4ToolStripMenuItem.Name = "dashed4ToolStripMenuItem";
            this.dashed4ToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.dashed4ToolStripMenuItem.Text = "Dashed 4";
            this.dashed4ToolStripMenuItem.Click += new System.EventHandler(this.dashed4ToolStripMenuItem_Click);
            // 
            // toolStripDropDownLineWidthButton
            // 
            this.toolStripDropDownLineWidthButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownLineWidthButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lineWidth1ToolStripMenuItem,
            this.lineWidth2ToolStripMenuItem,
            this.lineWidth3ToolStripMenuItem,
            this.lineWidth5ToolStripMenuItem,
            this.otherToolStripMenuItem});
            this.toolStripDropDownLineWidthButton.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight1MenuChoice1;
            this.toolStripDropDownLineWidthButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripDropDownLineWidthButton.Name = "toolStripDropDownLineWidthButton";
            this.toolStripDropDownLineWidthButton.Size = new System.Drawing.Size(45, 33);
            this.toolStripDropDownLineWidthButton.Text = "toolStripDropDownButton3";
            this.toolStripDropDownLineWidthButton.ToolTipText = "Select a line width or specify your own with \"Other\"";
            // 
            // lineWidth1ToolStripMenuItem
            // 
            this.lineWidth1ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight1MenuChoice1;
            this.lineWidth1ToolStripMenuItem.Name = "lineWidth1ToolStripMenuItem";
            this.lineWidth1ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.lineWidth1ToolStripMenuItem.Text = "1 pt.";
            this.lineWidth1ToolStripMenuItem.Click += new System.EventHandler(this.lineWidth1ToolStripMenuItem_Click);
            // 
            // lineWidth2ToolStripMenuItem
            // 
            this.lineWidth2ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight2MenuChoice1;
            this.lineWidth2ToolStripMenuItem.Name = "lineWidth2ToolStripMenuItem";
            this.lineWidth2ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.lineWidth2ToolStripMenuItem.Text = "2 pt.";
            this.lineWidth2ToolStripMenuItem.Click += new System.EventHandler(this.lineWidth2ToolStripMenuItem_Click);
            // 
            // lineWidth3ToolStripMenuItem
            // 
            this.lineWidth3ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight3MenuChoice1;
            this.lineWidth3ToolStripMenuItem.Name = "lineWidth3ToolStripMenuItem";
            this.lineWidth3ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.lineWidth3ToolStripMenuItem.Text = "3 pt.";
            this.lineWidth3ToolStripMenuItem.Click += new System.EventHandler(this.lineWidth3ToolStripMenuItem_Click);
            // 
            // lineWidth5ToolStripMenuItem
            // 
            this.lineWidth5ToolStripMenuItem.Image = global::DotNETViewerComponent.Properties.Resources.LineWeight4MenuChoice1;
            this.lineWidth5ToolStripMenuItem.Name = "lineWidth5ToolStripMenuItem";
            this.lineWidth5ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.lineWidth5ToolStripMenuItem.Text = "5 pt.";
            this.lineWidth5ToolStripMenuItem.Click += new System.EventHandler(this.lineWidth5ToolStripMenuItem_Click);
            // 
            // otherToolStripMenuItem
            // 
            this.otherToolStripMenuItem.Name = "otherToolStripMenuItem";
            this.otherToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.otherToolStripMenuItem.Text = "Other";
            this.otherToolStripMenuItem.Click += new System.EventHandler(this.otherToolStripMenuItem_Click);
            // 
            // colorPickerToolStripButton
            // 
            this.colorPickerToolStripButton.AutoSize = false;
            this.colorPickerToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.colorPickerToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.colorPickerToolStripButton.Name = "colorPickerToolStripButton";
            this.colorPickerToolStripButton.Size = new System.Drawing.Size(33, 33);
            this.colorPickerToolStripButton.ToolTipText = "Choose Annotation Fill and Stroke Color";
            this.colorPickerToolStripButton.Paint += new System.Windows.Forms.PaintEventHandler(this.colorPickerToolStripButton_Paint);
            this.colorPickerToolStripButton.Click += new System.EventHandler(this.colorPickerToolStripButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 36);
            // 
            // selectModeToolStripButton
            // 
            this.selectModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.selectModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.SelectModeMenu1;
            this.selectModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.selectModeToolStripButton.Name = "selectModeToolStripButton";
            this.selectModeToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.selectModeToolStripButton.ToolTipText = "Select Mode";
            this.selectModeToolStripButton.Click += new System.EventHandler(this.selectModeToolStripButton_Click);
            // 
            // zoomModeToolStripButton
            // 
            this.zoomModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomModeToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.ZoomModeMenu1;
            this.zoomModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.zoomModeToolStripButton.Name = "zoomModeToolStripButton";
            this.zoomModeToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.zoomModeToolStripButton.ToolTipText = "Zoom Mode (shift key + mouse click zooms out)";
            this.zoomModeToolStripButton.Click += new System.EventHandler(this.zoomModeToolStripButton_Click);
            // 
            // marqueeZoomToolStripButton
            // 
            this.marqueeZoomToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.marqueeZoomToolStripButton.Image = global::DotNETViewerComponent.Properties.Resources.MarqueeZoomModeMenu1;
            this.marqueeZoomToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.marqueeZoomToolStripButton.Name = "marqueeZoomToolStripButton";
            this.marqueeZoomToolStripButton.Size = new System.Drawing.Size(36, 33);
            this.marqueeZoomToolStripButton.ToolTipText = "Marquee Zoom Mode ";
            this.marqueeZoomToolStripButton.Click += new System.EventHandler(this.marqueeZoomToolStripButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 36);
            // 
            // searchPhraseToolStripTextBox
            // 
            this.searchPhraseToolStripTextBox.Name = "searchPhraseToolStripTextBox";
            this.searchPhraseToolStripTextBox.Size = new System.Drawing.Size(100, 36);
            this.searchPhraseToolStripTextBox.ToolTipText = "Text to search document for";
            this.searchPhraseToolStripTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchPhraseToolStripTextBox_KeyDown);
            this.searchPhraseToolStripTextBox.TextChanged += new System.EventHandler(this.searchPhraseToolStripTextBox_TextChanged);
            // 
            // searchToolStripButton
            // 
            this.searchToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.searchToolStripButton.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.searchToolStripButton.Name = "searchToolStripButton";
            this.searchToolStripButton.Size = new System.Drawing.Size(46, 33);
            this.searchToolStripButton.Text = "Search";
            this.searchToolStripButton.ToolTipText = "Search the document for the text enterted to the left of this button";
            this.searchToolStripButton.Click += new System.EventHandler(this.searchToolStripButton_Click);
            // 
            // ViewingSurfaceSplitContainer
            // 
            this.ViewingSurfaceSplitContainer.BackColor = System.Drawing.SystemColors.Control;
            this.ViewingSurfaceSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ViewingSurfaceSplitContainer.Location = new System.Drawing.Point(0, 60);
            this.ViewingSurfaceSplitContainer.Name = "ViewingSurfaceSplitContainer";
            // 
            // ViewingSurfaceSplitContainer.Panel1
            // 
            this.ViewingSurfaceSplitContainer.Panel1.Controls.Add(this.BookmarkAndLayersTabControl);
            this.ViewingSurfaceSplitContainer.Panel1.Resize += new System.EventHandler(this.ViewingSurfaceSplitContainer_Panel1_Resize);
            // 
            // ViewingSurfaceSplitContainer.Panel2
            // 
            this.ViewingSurfaceSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.ViewingSurfaceSplitContainer.Size = new System.Drawing.Size(1033, 394);
            this.ViewingSurfaceSplitContainer.SplitterDistance = 322;
            this.ViewingSurfaceSplitContainer.TabIndex = 2;
            this.ViewingSurfaceSplitContainer.TabStop = false;
            // 
            // BookmarkAndLayersTabControl
            // 
            this.BookmarkAndLayersTabControl.Controls.Add(this.BookmarkTabPage);
            this.BookmarkAndLayersTabControl.Controls.Add(this.LayerTabPage);
            this.BookmarkAndLayersTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BookmarkAndLayersTabControl.Location = new System.Drawing.Point(0, 0);
            this.BookmarkAndLayersTabControl.Name = "BookmarkAndLayersTabControl";
            this.BookmarkAndLayersTabControl.SelectedIndex = 0;
            this.BookmarkAndLayersTabControl.Size = new System.Drawing.Size(322, 394);
            this.BookmarkAndLayersTabControl.TabIndex = 0;
            this.BookmarkAndLayersTabControl.TabStop = false;
            this.BookmarkAndLayersTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.BookmarkAndLayersTabControl_Selected);
            this.BookmarkAndLayersTabControl.SelectedIndexChanged += new System.EventHandler(this.BookmarkAndLayersTabControl_SelectedIndexChanged);
            // 
            // BookmarkTabPage
            // 
            this.BookmarkTabPage.Location = new System.Drawing.Point(4, 22);
            this.BookmarkTabPage.Name = "BookmarkTabPage";
            this.BookmarkTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.BookmarkTabPage.Size = new System.Drawing.Size(314, 368);
            this.BookmarkTabPage.TabIndex = 0;
            this.BookmarkTabPage.Text = "Bookmarks";
            this.BookmarkTabPage.UseVisualStyleBackColor = true;
            // 
            // LayerTabPage
            // 
            this.LayerTabPage.Location = new System.Drawing.Point(4, 22);
            this.LayerTabPage.Name = "LayerTabPage";
            this.LayerTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.LayerTabPage.Size = new System.Drawing.Size(314, 368);
            this.LayerTabPage.TabIndex = 1;
            this.LayerTabPage.Text = "Layers";
            this.LayerTabPage.UseVisualStyleBackColor = true;
            // 
            // DotNETViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ViewingSurfaceSplitContainer);
            this.Controls.Add(this.MainToolStrip);
            this.Controls.Add(this.MainMenuStrip);
            this.Name = "DotNETViewer";
            this.Size = new System.Drawing.Size(1033, 454);
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();
            this.MainToolStrip.ResumeLayout(false);
            this.MainToolStrip.PerformLayout();
            this.ViewingSurfaceSplitContainer.Panel1.ResumeLayout(false);
            this.ViewingSurfaceSplitContainer.ResumeLayout(false);
            this.BookmarkAndLayersTabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.MenuStrip MainMenuStrip;
        private System.Windows.Forms.ToolStrip MainToolStrip;
        private System.Windows.Forms.SplitContainer ViewingSurfaceSplitContainer;
        private System.Windows.Forms.TabControl BookmarkAndLayersTabControl;
        private System.Windows.Forms.TabPage BookmarkTabPage;
        private System.Windows.Forms.TabPage LayerTabPage;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem appendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unlockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewPrintModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem continuousToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem singlePageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monitorResolutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBookmarkLayersToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton openToolStripButton;
        private System.Windows.Forms.ToolStripButton saveToolStripButton;
        private System.Windows.Forms.ToolStripButton printToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton firstPageToolStripButton;
        private System.Windows.Forms.ToolStripButton previousPageToolStripButton;
        private System.Windows.Forms.ToolStripTextBox selectedPageNumberToolStripTextBox;
        private System.Windows.Forms.ToolStripLabel pageNumberToolStripLabel;
        private System.Windows.Forms.ToolStripButton nextPageToolStripButton;
        private System.Windows.Forms.ToolStripButton lastPageToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton decreaseZoomToolStripButton;
        private System.Windows.Forms.ToolStripComboBox zoomLevelToolStripComboBox;
        private System.Windows.Forms.ToolStripButton increaseZoomToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSplitButton freeTextToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem fontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem justificationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem leftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem centerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rightToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripDropDownAnnotationButton;
        private System.Windows.Forms.ToolStripMenuItem ellipseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rectangleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polygonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polylineToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownLineTypeButton;
        private System.Windows.Forms.ToolStripMenuItem solidToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dashed1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dashed2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dashed3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dashed4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownLineWidthButton;
        private System.Windows.Forms.ToolStripMenuItem lineWidth1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineWidth2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineWidth3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lineWidth5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem otherToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton colorPickerToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton selectModeToolStripButton;
        private System.Windows.Forms.ToolStripButton zoomModeToolStripButton;
        private System.Windows.Forms.ToolStripButton marqueeZoomToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripTextBox searchPhraseToolStripTextBox;
        private System.Windows.Forms.ToolStripButton searchToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem startArrowheadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem endArrowheadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowStartNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowStartOpenArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowStartClosedArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowStartCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowEndNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowEndOpenArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowEndClosedArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrowEndCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineStartArrowheadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineStartNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineStartOpenArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineStartClosedArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineStartCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineEndArrowheadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineEndNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineEndOpenArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineEndClosedArrowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem polyLineEndCircleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem linkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem underlineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem highlightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem test1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createDocToolStripMenuItem;
    }
}
