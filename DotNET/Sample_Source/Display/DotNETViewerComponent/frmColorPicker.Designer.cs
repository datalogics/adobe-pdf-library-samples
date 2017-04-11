namespace DotNETViewerComponent
{
    partial class frmColorPicker
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
            this.label1 = new System.Windows.Forms.Label();
            this.OKBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.swapBtn = new System.Windows.Forms.Button();
            this.fillTransparentBtn = new System.Windows.Forms.Button();
            this.fillColorPictureBox = new System.Windows.Forms.PictureBox();
            this.strokeColorPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.fillColorPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.strokeColorPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Click on a color square to change its color.";
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(211, 256);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(70, 23);
            this.OKBtn.TabIndex = 3;
            this.OKBtn.Text = "&OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(287, 256);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(68, 23);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.Text = "&Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // swapBtn
            // 
            this.swapBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.swapBtn.Location = new System.Drawing.Point(134, 220);
            this.swapBtn.Name = "swapBtn";
            this.swapBtn.Size = new System.Drawing.Size(146, 26);
            this.swapBtn.TabIndex = 2;
            this.swapBtn.Text = "Swap Colors";
            this.swapBtn.UseVisualStyleBackColor = true;
            this.swapBtn.Click += new System.EventHandler(this.swapBtn_Click);
            // 
            // fillTransparentBtn
            // 
            this.fillTransparentBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fillTransparentBtn.Location = new System.Drawing.Point(214, 47);
            this.fillTransparentBtn.Name = "fillTransparentBtn";
            this.fillTransparentBtn.Size = new System.Drawing.Size(146, 26);
            this.fillTransparentBtn.TabIndex = 1;
            this.fillTransparentBtn.Text = "Make Fill Transparent";
            this.fillTransparentBtn.UseVisualStyleBackColor = true;
            this.fillTransparentBtn.Click += new System.EventHandler(this.fillTransparentBtn_Click);
            // 
            // fillColorPictureBox
            // 
            this.fillColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fillColorPictureBox.Location = new System.Drawing.Point(82, 47);
            this.fillColorPictureBox.Name = "fillColorPictureBox";
            this.fillColorPictureBox.Size = new System.Drawing.Size(100, 100);
            this.fillColorPictureBox.TabIndex = 5;
            this.fillColorPictureBox.TabStop = false;
            this.fillColorPictureBox.Click += new System.EventHandler(this.fillColorPictureBox_Click);
            // 
            // strokeColorPictureBox
            // 
            this.strokeColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.strokeColorPictureBox.Location = new System.Drawing.Point(134, 99);
            this.strokeColorPictureBox.Name = "strokeColorPictureBox";
            this.strokeColorPictureBox.Size = new System.Drawing.Size(100, 100);
            this.strokeColorPictureBox.TabIndex = 6;
            this.strokeColorPictureBox.TabStop = false;
            this.strokeColorPictureBox.Click += new System.EventHandler(this.strokeColorPictureBox_Click);
            // 
            // frmColorPicker
            // 
            this.AcceptButton = this.OKBtn;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(372, 292);
            this.ControlBox = false;
            this.Controls.Add(this.strokeColorPictureBox);
            this.Controls.Add(this.fillColorPictureBox);
            this.Controls.Add(this.fillTransparentBtn);
            this.Controls.Add(this.swapBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmColorPicker";
            this.Text = "Color Picker";
            this.Load += new System.EventHandler(this.frmColorPicker_Load);
            ((System.ComponentModel.ISupportInitialize)(this.fillColorPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.strokeColorPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button swapBtn;
        private System.Windows.Forms.Button fillTransparentBtn;
        private System.Windows.Forms.PictureBox fillColorPictureBox;
        private System.Windows.Forms.PictureBox strokeColorPictureBox;
    }
}