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
            List<string> paths = new List<string>();
            paths.Add(Library.ResourceDirectory + "Fonts");
            paths.Add(Library.ResourceDirectory + "CMaps");

            using (Library lib = new Library(paths, Library.ResourceDirectory + "CMap", Library.ResourceDirectory + "Unicode", Library.ResourceDirectory + "Color", LibraryFlags.NoFlags))
            {
                // Set visual styles
                // Set default thread exceptions
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

                /* You may find that you receive exceptions when you attempt to open
                 * PDF files that contain permissions restrictions on content or image
                 * extraction.  This is due to the APIs used for viewing: these can
                 * also be used in other contexts for content extraction or enabling
                 * save-as-image capabilities. If you are making a PDF file viewer and
                 * you encounter this situation, please contact your support
                 * representative or support@datalogics.com to request a key to enable
                 * bypassing this restriction check.
                 */
                //if (!Library.EnableLicensedBehavior("xxxxxxxxxxxxxxxxxxxxx="))
                //    throw new ApplicationException("Licensed behavior failure");

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