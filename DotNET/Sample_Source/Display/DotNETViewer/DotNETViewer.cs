using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Datalogics.PDFL;
using DotNETViewerComponent;
using System.Drawing;


/* 
 * This sample is a utility that demonstrates a PDF viewing tool. You can use it to open, display, and edit PDF files.
 * 
 * For more detail see the description of the DotNETViewer on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/viewing-pdf-files
 * 
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
*/
namespace DotNETViewer
{
    static class DotNETViewer
    {
        [STAThread]
        static void Main(String[] args)
        {
            commandLineArgs = args;
            using (Library lib = new Library())
            {
                // Set visual styles
                // Set default thread exceptions
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

                if (!Library.EnableLicensedBehavior("Gqmeu6ollrqna8Pub5NmKYUwXmyH0IunziAwxui2fXc="))
                    throw new ApplicationException("Licensed behavior failure");

                Application.Run(new Form1());
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
#if DEBUG
            MessageBox.Show(e.Exception.ToString(), "Exception Occurred");
#else
            MessageBox.Show(e.Exception.Message, "Error");
#endif
        }
        public static String[] commandLineArgs = null;
    }
}