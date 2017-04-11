namespace DotNETViewerComponent
{
    partial class frmLinkAnnotationProperties
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
            this.SetLink = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.PageNum = new System.Windows.Forms.CheckBox();
            this.ViewParams = new System.Windows.Forms.GroupBox();
            this.positionCheckBox = new System.Windows.Forms.CheckBox();
            this.zoomCheckBox = new System.Windows.Forms.CheckBox();
            this.customParams = new System.Windows.Forms.RadioButton();
            this.viewRect = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.ViewParams.SuspendLayout();
            this.SuspendLayout();
            // 
            // SetLink
            // 
            this.SetLink.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.SetLink.Location = new System.Drawing.Point(37, 255);
            this.SetLink.Name = "SetLink";
            this.SetLink.Size = new System.Drawing.Size(75, 23);
            this.SetLink.TabIndex = 1;
            this.SetLink.Text = "Set Link";
            this.SetLink.UseVisualStyleBackColor = true;
            this.SetLink.Click += new System.EventHandler(this.SetLink_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(157, 255);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // PageNum
            // 
            this.PageNum.AutoSize = true;
            this.PageNum.Checked = true;
            this.PageNum.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PageNum.Enabled = false;
            this.PageNum.Location = new System.Drawing.Point(8, 23);
            this.PageNum.Name = "PageNum";
            this.PageNum.Size = new System.Drawing.Size(91, 17);
            this.PageNum.TabIndex = 0;
            this.PageNum.Text = "Page Number";
            this.PageNum.UseVisualStyleBackColor = true;
            // 
            // ViewParams
            // 
            this.ViewParams.Controls.Add(this.positionCheckBox);
            this.ViewParams.Controls.Add(this.PageNum);
            this.ViewParams.Controls.Add(this.zoomCheckBox);
            this.ViewParams.Controls.Add(this.customParams);
            this.ViewParams.Controls.Add(this.viewRect);
            this.ViewParams.Location = new System.Drawing.Point(36, 79);
            this.ViewParams.Name = "ViewParams";
            this.ViewParams.Size = new System.Drawing.Size(197, 154);
            this.ViewParams.TabIndex = 0;
            this.ViewParams.TabStop = false;
            this.ViewParams.Text = "View Parameters";
            // 
            // positionCheckBox
            // 
            this.positionCheckBox.AutoSize = true;
            this.positionCheckBox.Location = new System.Drawing.Point(29, 125);
            this.positionCheckBox.Name = "positionCheckBox";
            this.positionCheckBox.Size = new System.Drawing.Size(63, 17);
            this.positionCheckBox.TabIndex = 4;
            this.positionCheckBox.Text = "Position";
            this.positionCheckBox.UseVisualStyleBackColor = true;
            // 
            // zoomCheckBox
            // 
            this.zoomCheckBox.AutoSize = true;
            this.zoomCheckBox.Location = new System.Drawing.Point(29, 99);
            this.zoomCheckBox.Name = "zoomCheckBox";
            this.zoomCheckBox.Size = new System.Drawing.Size(53, 17);
            this.zoomCheckBox.TabIndex = 3;
            this.zoomCheckBox.Text = "Zoom";
            this.zoomCheckBox.UseVisualStyleBackColor = true;
            // 
            // customParams
            // 
            this.customParams.AutoSize = true;
            this.customParams.Location = new System.Drawing.Point(8, 75);
            this.customParams.Name = "customParams";
            this.customParams.Size = new System.Drawing.Size(115, 17);
            this.customParams.TabIndex = 2;
            this.customParams.TabStop = true;
            this.customParams.Text = "Custom parameters";
            this.customParams.UseVisualStyleBackColor = true;
            this.customParams.CheckedChanged += new System.EventHandler(this.customParams_CheckedChanged);
            // 
            // viewRect
            // 
            this.viewRect.AutoSize = true;
            this.viewRect.Location = new System.Drawing.Point(8, 49);
            this.viewRect.Name = "viewRect";
            this.viewRect.Size = new System.Drawing.Size(95, 17);
            this.viewRect.TabIndex = 1;
            this.viewRect.TabStop = true;
            this.viewRect.Text = "View rectangle";
            this.viewRect.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(33, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(199, 67);
            this.label1.TabIndex = 3;
            this.label1.Text = "Use the scrollbars, mouse and zoom tools to select the target view, then press \'S" +
                "et Link\' to create the link destination";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmLinkAnnotationProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 303);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ViewParams);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.SetLink);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLinkAnnotationProperties";
            this.Text = "Link Annotation Properties";
            this.ViewParams.ResumeLayout(false);
            this.ViewParams.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.GroupBox ViewParams;
        private System.Windows.Forms.CheckBox PageNum;
        private System.Windows.Forms.CheckBox positionCheckBox;
        private System.Windows.Forms.CheckBox zoomCheckBox;
        private System.Windows.Forms.RadioButton customParams;
        private System.Windows.Forms.RadioButton viewRect;
        public System.Windows.Forms.Button SetLink;
        private System.Windows.Forms.Label label1;
    }
}