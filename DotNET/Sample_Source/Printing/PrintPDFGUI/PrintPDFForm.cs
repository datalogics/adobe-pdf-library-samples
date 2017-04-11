using System;
using System.IO;
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
    public class SimpleDelegate
    {
        #region ProgressForm
        private PrintProgressForm mProgressForm;
        public PrintProgressForm ProgressForm
        {   // Reference to the Progress window (form)...
            set
            {
                this.mProgressForm = value;
            }
            private get
            {
                return this.mProgressForm;
            }
        }
        #endregion

        #region PDFInfo
        private FileInfo mPDFInfo;
        public FileInfo PDFInfo
        {   // Storage for the PDF's FileInfo block...
            set
            {
                this.mPDFInfo = value;
            }
            get
            {
                return this.mPDFInfo;
            }
        }
        #endregion

        public void Work()
        {   // The threaded work...
            PrintPDF.PrintOnePDF(PDFInfo, ProgressForm.CancelHandler, ProgressForm.ProgressHandler);
        }
    };

    public partial class PrintPDFForm : System.Windows.Forms.Form
    {
        #region PrintThread
        private Thread mPrintThread;
        private Thread PrintThread
        {   // Reference to the print thread...
            set
            {
                this.mPrintThread = value;
            }
            get
            {
                return this.mPrintThread;
            }
        }
        #endregion

        #region ProgressForm
        private PrintProgressForm mProgressForm;
        private PrintProgressForm ProgressForm
        {   // Reference to the Progress window (form)...
            set
            {
                this.mProgressForm = value;
            }
            get
            {
                return this.mProgressForm;
            }
        }
        #endregion

        #region PDFInfo
        private FileInfo mPDFInfo;
        private FileInfo PDFInfo
        {   // Storage for the PDF's FileInfo block...
            set
            {
                this.mPDFInfo = value;
            }
            get
            {
                return this.mPDFInfo;
            }
        }
        #endregion

        public PrintPDFForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filename = textBox1.Text.ToString();
            if (filename.Length < 1)
            {   // Invalid entry...
                this.Close();
                return;
            }

            PDFInfo = new FileInfo(filename.Trim('"'));
            if (!PDFInfo.Exists)
            {   // Invalid entry...
                this.Close();
                return;
            }

            if (PrintThread != null && PrintThread.IsAlive)
            {
                //
                // Simultaneous print cycles are not supported by this sample (or DLE/APDFL).
                //
                // If one wants something like that, they'll need to capture the humans
                // desires in a List<FileInfo> and serialize the act of printing
                // N-many times into a larger, linear (one after the other) design.
                //

                return;
            }

            //
            // One cannot print and update UI on a common thread. To present
            // a GUI one MUST construct a multi-threaded application (like that
            // shown here). Along with that come requirements for cross-thread
            // information sharing. Consequently, one must know how to use
            // (older) BeginInvoke delegation (as shown here) and/or .Net Task
            // functionality as well as become well versed in how to deal with
            // exceptions and memory management under such conditions. Memory
            // management does not "just happen for for free" (even in a GCed
            // environment).
            //

            //
            // WARNING: An application like this (that invokes DLE/APDFL on
            // child threads MUST have a base "using(Library lib = new Library)"
            // on the primary (application) thread. This requirement is not optional
            // and must be provided or the application may (likely will) crash when
            // one least expects! For more on why, please go find/read the commentary
            // near the base Library invocation in PrintPDFGUI.cs
            //

            ProgressForm = new PrintProgressForm();

            SimpleDelegate d = new SimpleDelegate();
            d.ProgressForm = ProgressForm;
            d.PDFInfo = PDFInfo;

            PrintThread = new Thread(new ThreadStart(d.Work));
            PrintThread.Start();

            while (ProgressForm.ProgressHandler.PrintingStarted == false)
            {   // Wait for printing to start
                Thread.Sleep(0);
            }

            if (ProgressForm.ProgressHandler.PrintingStarted)
            {   // The human didn't cancel (when they could have) at the print dialog...
                // So, we're just about ready to (actually) begin printing...
                ProgressForm.Show();
            }
        }
    }
}
