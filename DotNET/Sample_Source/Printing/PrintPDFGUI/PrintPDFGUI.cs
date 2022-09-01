using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Datalogics.PDFL;

/*
 * This sample printing a PDF file. It is similar to PrintPDF, but this
 * program provides a user interface.
 * 
 * For more detail see the description of the PrintPDFGUI sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/printing-pdf-files-and-generating-postscript-ps-files-from-pdf
 * 
 * Copyright (c) 2007-2018, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PrintPDFGUI
{
    static class PrintPDFGUI
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PrintPDFForm());
        }
    }
}
