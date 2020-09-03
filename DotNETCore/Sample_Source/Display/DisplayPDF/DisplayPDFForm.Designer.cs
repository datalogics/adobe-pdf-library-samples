/*
Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved. 

The information and code in this sample is for the exclusive use of Datalogics
customers and evaluation users only.  Datalogics permits you to use, modify and
distribute this file in accordance with the terms of your license agreement.
Sample code is for demonstrative purposes only and is not intended for production use.
 */

namespace DisplayPDF
{
    partial class DisplayPDFForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayPDFForm));
            this.menu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.PDFPagepanel = new System.Windows.Forms.Panel();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.printToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.textsearchbutton = new System.Windows.Forms.ToolStripButton();
            this.TextSearch = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.PageNumberTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.PageLabel = new System.Windows.Forms.ToolStripLabel();
            this.scalepercent = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.PDFPagepicturebox = new DisplayPDF.MyPictureBox();
            this.menu.SuspendLayout();
            this.PDFPagepanel.SuspendLayout();
            this.mainToolStrip.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PDFPagepicturebox)).BeginInit();
            this.SuspendLayout();
            // 
            // menu
            // 
            this.menu.Dock = System.Windows.Forms.DockStyle.None;
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(726, 24);
            this.menu.TabIndex = 0;
            this.menu.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.printToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(137, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // PDFPagepanel
            // 
            this.PDFPagepanel.AutoScroll = true;
            this.PDFPagepanel.AutoSize = true;
            this.PDFPagepanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PDFPagepanel.Controls.Add(this.PDFPagepicturebox);
            this.PDFPagepanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PDFPagepanel.Location = new System.Drawing.Point(0, 0);
            this.PDFPagepanel.MaximumSize = new System.Drawing.Size(4096, 3072);
            this.PDFPagepanel.Name = "PDFPagepanel";
            this.PDFPagepanel.Size = new System.Drawing.Size(726, 315);
            this.PDFPagepanel.TabIndex = 2;
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.AutoSize = false;
            this.mainToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripButton,
            this.printToolStripButton,
            this.toolStripSeparator,
            this.textsearchbutton,
            this.TextSearch,
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripButton3,
            this.toolStripButton4,
            this.PageNumberTextBox,
            this.PageLabel,
            this.scalepercent,
            this.toolStripButton5,
            this.toolStripButton6});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 24);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(726, 25);
            this.mainToolStrip.Stretch = true;
            this.mainToolStrip.TabIndex = 19;
            this.mainToolStrip.Text = "PDFL Viewer Tool Strip";
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripButton.Image")));
            this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openToolStripButton.Text = "&Open";
            this.openToolStripButton.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // printToolStripButton
            // 
            this.printToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("printToolStripButton.Image")));
            this.printToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printToolStripButton.Name = "printToolStripButton";
            this.printToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.printToolStripButton.Text = "&Print";
            this.printToolStripButton.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // textsearchbutton
            // 
            this.textsearchbutton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.textsearchbutton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.textsearchbutton.Image = ((System.Drawing.Image)(resources.GetObject("textsearchbutton.Image")));
            this.textsearchbutton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.textsearchbutton.Name = "textsearchbutton";
            this.textsearchbutton.Size = new System.Drawing.Size(44, 22);
            this.textsearchbutton.Text = "Search";
            this.textsearchbutton.Click += new System.EventHandler(this.textsearchbutton_Click);
            // 
            // TextSearch
            // 
            this.TextSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.TextSearch.Name = "TextSearch";
            this.TextSearch.Size = new System.Drawing.Size(100, 25);
            this.TextSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textsearchbutton_KeyDown);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(27, 22);
            this.toolStripButton1.Text = "<<";
            this.toolStripButton1.ToolTipText = "First Page";
            this.toolStripButton1.Click += new System.EventHandler(this.rewindtodocstart_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "<";
            this.toolStripButton2.ToolTipText = "Previous Page";
            this.toolStripButton2.Click += new System.EventHandler(this.gobackonepage_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = ">";
            this.toolStripButton3.ToolTipText = "Next Page";
            this.toolStripButton3.Click += new System.EventHandler(this.forwardonedocpage_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(27, 22);
            this.toolStripButton4.Text = ">>";
            this.toolStripButton4.ToolTipText = "Last Page";
            this.toolStripButton4.Click += new System.EventHandler(this.forwardtodocend_Click);
            // 
            // PageNumberTextBox
            // 
            this.PageNumberTextBox.Name = "PageNumberTextBox";
            this.PageNumberTextBox.Size = new System.Drawing.Size(50, 25);
            this.PageNumberTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PageNumberTextBox_KeyDown);
            // 
            // PageLabel
            // 
            this.PageLabel.Name = "PageLabel";
            this.PageLabel.Size = new System.Drawing.Size(10, 22);
            this.PageLabel.Text = " ";
            // 
            // scalepercent
            // 
            this.scalepercent.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.scalepercent.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.scalepercent.AutoSize = false;
            this.scalepercent.Items.AddRange(new object[] {
            "10 %",
            "25 %",
            "50 %",
            "75 %",
            "100 %",
            "125 %",
            "150 %",
            "200 %",
            "400 %",
            "800 %",
            "1600 %",
            "2400 %",
            "3200 %"});
            this.scalepercent.Margin = new System.Windows.Forms.Padding(20, 0, 1, 0);
            this.scalepercent.Name = "scalepercent";
            this.scalepercent.Size = new System.Drawing.Size(75, 21);
            this.scalepercent.Text = "100 %";
            this.scalepercent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.scalepercent_KeyDown);
            this.scalepercent.SelectedIndexChanged += new System.EventHandler(this.scalepercent_LostFocus);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.Text = "-";
            this.toolStripButton5.ToolTipText = "Zoom Out";
            this.toolStripButton5.Click += new System.EventHandler(this.scaledown_Click);
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton6.Text = "+";
            this.toolStripButton6.ToolTipText = "Zoom In";
            this.toolStripButton6.Click += new System.EventHandler(this.scaleup_Click);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.AutoScroll = true;
            this.toolStripContainer1.ContentPanel.Controls.Add(this.PDFPagepanel);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(726, 315);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(726, 364);
            this.toolStripContainer1.TabIndex = 20;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menu);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mainToolStrip);
            // 
            // PDFPagepicturebox
            // 
            this.PDFPagepicturebox.Location = new System.Drawing.Point(0, 3);
            this.PDFPagepicturebox.Name = "PDFPagepicturebox";
            this.PDFPagepicturebox.Size = new System.Drawing.Size(726, 312);
            this.PDFPagepicturebox.TabIndex = 0;
            this.PDFPagepicturebox.TabStop = false;
            // 
            // DisplayPDFForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 364);
            this.Controls.Add(this.toolStripContainer1);
            this.MainMenuStrip = this.menu;
            this.Name = "DisplayPDFForm";
            this.Text = "Display PDF";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.PDFPagepanel.ResumeLayout(false);
            this.PDFPagepanel.PerformLayout();
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PDFPagepicturebox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Panel PDFPagepanel;
        private MyPictureBox PDFPagepicturebox;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripButton openToolStripButton;
        private System.Windows.Forms.ToolStripButton printToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripTextBox TextSearch;
        private System.Windows.Forms.ToolStripButton textsearchbutton;
        private System.Windows.Forms.ToolStripTextBox PageNumberTextBox;
        private System.Windows.Forms.ToolStripLabel PageLabel;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripComboBox scalepercent;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}

