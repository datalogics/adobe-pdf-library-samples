using System;
using System.Windows.Forms;
using Datalogics.PDFL;

/*
 * This sample printing a PDF file. It is similar to PrintPDF, but this
 * program provides a user interface.
 *
 * For more detail see the description of the PrintPDFGUI sample program on our Developer’s site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/printing-pdf-files-and-generating-postscript-ps-files-from-pdf
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PrintPDFGUI
{
    public partial class PrintPDFForm : System.Windows.Forms.Form
    {
        public PrintPDFForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                using (Library lib = new Library())
                {
                    using (Document doc = new Document(textBox1.Text))
                    {
                        using (PrintUserParams userParams = new PrintUserParams())
                        {
                            userParams.NCopies = 1;

                            if (userParams.PosePrintDialog(doc))
                            {
                                userParams.PrintParams.ShrinkToFit = true;
                                userParams.ShrinkToFit = userParams.PrintParams.ShrinkToFit;
                                userParams.PaperHeight = PrintUserParams.UseMediaBox;
                                userParams.PaperWidth = PrintUserParams.UseMediaBox;

                                // If you un-comment the file path (binding) below, PDFL will print via the platform print driver to disk.
                                // The type of file produced when doing so (e.g., PS, PCL, PCLXL) depends upon the type of printer/driver. 
                                //userParams.OutFileName = String.Format("{0}\\{1}.prn", finfo.Directory.FullName, System.IO.Path.GetFileNameWithoutExtension(finfo.FullName));

                                doc.Print(userParams, null, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when printing...{0}{1}", Environment.NewLine, ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.FileName;
                    button1.Enabled = true;
                }
            }
        }
    }
}
