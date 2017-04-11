using System;
namespace PrintPDFGUI
{
    partial class PrintProgressForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelPageInfo = new System.Windows.Forms.Label();
            this.progressPage = new System.Windows.Forms.ProgressBar();
            this.progressStage = new System.Windows.Forms.ProgressBar();
            this.labelStageInfo = new System.Windows.Forms.Label();
            this.labelStagePercent = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            //System.Windows.Forms.Application.DoEvents();
            //this.progressWorker = new System.ComponentModel.BackgroundWorker();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cancelButton);
            this.groupBox1.Controls.Add(this.labelStagePercent);
            this.groupBox1.Controls.Add(this.progressStage);
            this.groupBox1.Controls.Add(this.labelStageInfo);
            this.groupBox1.Controls.Add(this.progressPage);
            this.groupBox1.Controls.Add(this.labelPageInfo);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 146);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(179, 117);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.TabStop = true;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // labelPageInfo
            // 
            this.labelPageInfo.AutoSize = true;
            this.labelPageInfo.Location = new System.Drawing.Point(6, 16);
            this.labelPageInfo.Name = "labelPageInfo";
            this.labelPageInfo.Size = new System.Drawing.Size(0, 13);
            this.labelPageInfo.TabIndex = 0;
            this.labelPageInfo.Text = "Page(s)";
            // 
            // progressPage
            // 
            this.progressPage.Location = new System.Drawing.Point(6, 32);
            this.progressPage.Name = "progressPage";
            this.progressPage.Size = new System.Drawing.Size(248, 23);
            this.progressPage.TabIndex = 0;
            // 
            // labelStageInfo
            // 
            this.labelStageInfo.AutoSize = true;
            this.labelStageInfo.Location = new System.Drawing.Point(6, 58);
            this.labelStageInfo.Name = "labelStageInfo";
            this.labelStageInfo.Size = new System.Drawing.Size(0, 13);
            this.labelStageInfo.TabIndex = 0;
            this.labelStageInfo.Text = "Stages";
            // 
            // progressStage
            // 
            this.progressStage.Location = new System.Drawing.Point(6, 74);
            this.progressStage.Name = "progressStage";
            this.progressStage.Size = new System.Drawing.Size(207, 23);
            this.progressStage.TabIndex = 0;
            // 
            // labelStagePercent
            // 
            this.labelStagePercent.AutoSize = true;
            this.labelStagePercent.Location = new System.Drawing.Point(219, 84);
            this.labelStagePercent.Name = "labelStagePercent";
            this.labelStagePercent.Size = new System.Drawing.Size(0, 13);
            this.labelStagePercent.TabIndex = 0;
            this.labelStagePercent.Text = "%";
            //
            // progressWorker
            //
            //this.progressWorker.WorkerReportsProgress = true;
            //this.progressWorker.WorkerSupportsCancellation = true;
            //this.progressWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.progressWorker_DoWork);
            //this.progressWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.progressWorker_ProgressChanged);
            //this.progressWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.progressWorker_RunWorkerCompleted);
            // 
            // PrintProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 170);
            this.Controls.Add(this.groupBox1);
            this.Name = "progressForm";
            this.Text = "Print Progress";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintProgressForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ProgressBar progressPage;
        private System.Windows.Forms.Label labelPageInfo;
        private System.Windows.Forms.ProgressBar progressStage;
        private System.Windows.Forms.Label labelStagePercent;
        private System.Windows.Forms.Label labelStageInfo;
        //private System.ComponentModel.BackgroundWorker progressWorker;
    }
}