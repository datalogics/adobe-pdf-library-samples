namespace DotNETViewerComponent
{
    partial class frmMonitorResolution
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMonitorResolution = new System.Windows.Forms.TextBox();
            this.lblValidationMsg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(89, 96);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.CausesValidation = false;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(170, 96);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Resolution (DPI):";
            // 
            // txtMonitorResolution
            // 
            this.txtMonitorResolution.Location = new System.Drawing.Point(133, 28);
            this.txtMonitorResolution.Name = "txtMonitorResolution";
            this.txtMonitorResolution.Size = new System.Drawing.Size(112, 20);
            this.txtMonitorResolution.TabIndex = 0;
            this.txtMonitorResolution.Validating += new System.ComponentModel.CancelEventHandler(this.txtMonitorResolution_Validating);
            // 
            // lblValidationMsg
            // 
            this.lblValidationMsg.AutoSize = true;
            this.lblValidationMsg.Location = new System.Drawing.Point(39, 66);
            this.lblValidationMsg.Name = "lblValidationMsg";
            this.lblValidationMsg.Size = new System.Drawing.Size(180, 13);
            this.lblValidationMsg.TabIndex = 4;
            this.lblValidationMsg.Text = "Enter a number between 36 and 288";
            this.lblValidationMsg.Visible = false;
            // 
            // frmMonitorResolution
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(257, 131);
            this.ControlBox = false;
            this.Controls.Add(this.lblValidationMsg);
            this.Controls.Add(this.txtMonitorResolution);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMonitorResolution";
            this.Text = "Monitor Resolution";
            this.Load += new System.EventHandler(this.frmMonitorResolution_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMonitorResolution;
        private System.Windows.Forms.Label lblValidationMsg;
    }
}