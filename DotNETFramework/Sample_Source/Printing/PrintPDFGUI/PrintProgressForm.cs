using System;
using System.Threading;

/*
 * A sample which demonstrates the use of the DLE API to print a
 * PDF file to a windows printer output device.
 * 
 * Copyright 2015, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */

namespace PrintPDFGUI
{
    public partial class PrintProgressForm : System.Windows.Forms.Form
    {
        #region CancelHandler
        private SamplePrintCancel mCancelHandler;
        public SamplePrintCancel CancelHandler
        {   // Storage for a reference to the SamplePrintCancel class
            set
            {
                this.mCancelHandler = value;
            }
            get
            {
                return this.mCancelHandler;
            }
        }
        #endregion

        #region ProgressHandler
        private SamplePrintProgress mProgressHandler;
        public SamplePrintProgress ProgressHandler
        {   // Storage for a reference to the SamplePrintProgress class
            set
            {
                this.mProgressHandler = value;
            }
            get
            {
                return this.mProgressHandler;
            }
        }
        #endregion

        public PrintProgressForm()
        {
            InitializeComponent();

            this.CancelHandler = new SamplePrintCancel();
            this.ProgressHandler = new SamplePrintProgress();
            this.ProgressHandler.ProgressUI = this; // They need to know about one another
        }

        public void ProgressDone()
        {   // Continue propagation of request to close the Print Progress form...
            this.BeginInvoke(new ProgressDoneDelegate(ProgressDoneClose));
        }

        public void ProgressChanged(ProgressUpdateStateBlock state)
        {
            switch (state.what)
            {
                case ProgressUpdateState.Page:
                {
                    if (this.InvokeRequired)
                    {   // Consult: https://msdn.microsoft.com/en-us/library/2e08f6yc%28v=vs.110%29.aspx
                        object[] myArray = new object[1];
                        myArray[0] = new ProgressUpdateStateBlock(state);
                        progressPage.BeginInvoke(new ProgressDelegate(ProgressPageAssign), myArray);
                    }
                    else
                    {   // This needs to be present (albeit unlikely to ever execute)
                        if (state.pageInfo != null)
                        {
                            labelPageInfo.Text = state.pageInfo;
                        }

                        if (state.pageCurrent > 0)
                        {
                            if (state.pageCurrent > progressPage.Maximum)
                            {
                                state.pageCurrent = progressPage.Maximum;
                            }

                            progressPage.Value = state.pageCurrent;
                        }
                    }
                    break;
                }
                case ProgressUpdateState.Stage:
                {
                    if (this.InvokeRequired)
                    {   // Consult: https://msdn.microsoft.com/en-us/library/2e08f6yc%28v=vs.110%29.aspx
                        object[] myArray = new object[1];
                        myArray[0] = new ProgressUpdateStateBlock(state);
                        progressStage.BeginInvoke(new ProgressDelegate(ProgressStageAssign), myArray);
                    }
                    else
                    {   // This needs to be present (albeit unlikely to ever execute)
                        if (state.stageInfo != null)
                        {
                            labelStageInfo.Text = state.stageInfo;
                        }

                        if (state.stagePercent > 0.0 && state.stagePercent <= 1.0)
                        {
                            progressStage.Value = (int)Math.Floor(progressStage.Maximum * state.stagePercent);
                            labelStagePercent.Text = String.Format("{0}%", state.stagePercent);
                        }
                        else
                        {
                            progressStage.Value = 0;
                            labelStagePercent.Text = @"";
                        }
                    }
                    break;
                }
            }
        }

        #region Progress Delegate Stuff
        //
        // Consult: https://msdn.microsoft.com/en-us/library/2e08f6yc%28v=vs.110%29.aspx
        //
        public delegate void ProgressDelegate(ProgressUpdateStateBlock state);
        public void ProgressPageAssign(ProgressUpdateStateBlock state)
        {   // Transfer the state (block) information to the control(s)...
            // Updates (of the UI) will happen automatically (after assignment, per each)
            if (state.pageSum > 0 && progressPage.Maximum != state.pageSum)
            {
                progressPage.Maximum = state.pageSum;
            }

            if (state.pageInfo != null)
            {
                labelPageInfo.Text = state.pageInfo;
            }

            if (state.pageCurrent > 0)
            {
                if (state.pageCurrent > progressPage.Maximum)
                {
                    state.pageCurrent = progressPage.Maximum;
                }

                progressPage.Value = state.pageCurrent;
            }
        }
        public void ProgressStageAssign(ProgressUpdateStateBlock state)
        {   // Transfer the state (block) information to the control(s)...
            // Updates (of the UI) will happen automatically (after assignment, per each)
            if (state.stageInfo != null)
            {
                labelStageInfo.Text = state.stageInfo;
            }

            if (state.stagePercent > 0.0 && state.stagePercent <= 1.0)
            {
                progressStage.Value = (int)Math.Floor(progressStage.Maximum * state.stagePercent);
                labelStagePercent.Text = String.Format("{0}%", progressStage.Value);
            }
            else
            {
                progressStage.Value = 0;
                labelStagePercent.Text = @"";
            }
        }

        public delegate void ProgressDoneDelegate();
        public void ProgressDoneClose()
        {   // Here we do the (actual) form Close operation...
            this.Close();
        }
        #endregion

        #region Cancel Button Stuff
        private void CancelButton_Click(object sender, EventArgs e)
        {
            //
            // Setting Cancel to true will inform the cancel handler that *next time*
            // it is called by DLE that it should tell APDFL to cancel (printing).
            //

            CancelHandler.Cancel = true;

            //
            // The following roll back animation is completely optional.
            // It's just here for some fun...
            //

            RollbackAnimation();
        }

        private void RollbackAnimation()
        {
            int progressPageValue = progressPage.Value;
            int progressStageValue = progressStage.Value;

            ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
            state.what = ProgressUpdateState.Page;
            state.pageInfo = @"Canceling...";
            state.pageCurrent = progressPageValue;
            state.pageSum = ProgressHandler.NumPages;
            state.stageInfo = @"";
            ProgressChanged(state);

            for (int i = progressStageValue; i >= 0; --i)
            {
                if (progressStageValue > 0)
                {
                    if (i > 5)
                    {   // Sleeping helps with human comprehension (but let's not over do it)
                        Thread.Sleep(1);
                    }
                    else
                    {   // Sleeping helps with human comprehension
                        Thread.Sleep(2);
                    }

                    state.what = ProgressUpdateState.Stage;
                    state.stagePercent = i;
                    ProgressChanged(state);
                }
            }

            for (int i = progressPageValue; i >= 0; --i)
            {
                if (progressPageValue > 0)
                {
                    if (i > 50)
                    {   // Sleeping helps with human comprehension (but let's not over do it)
                        Thread.Sleep(1);
                    }
                    else
                    {   // Sleeping helps with human comprehension
                        Thread.Sleep(2);
                    }

                    state.what = ProgressUpdateState.Page;
                    state.pageCurrent = i;
                    state.pageSum = ProgressHandler.NumPages;
                    ProgressChanged(state);
                }
            }

            this.Close();
        }

        void PrintProgressForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (ProgressHandler.PrintingStarted == true)
            {   // Tell APDFL to cancel printing (when printing is still active)
                CancelHandler.Cancel = true;
            }
        }
        #endregion
    }
}
