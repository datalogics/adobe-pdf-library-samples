using System;
using System.IO;
using Datalogics.PDFL;

namespace PrintPDFGUI
{
    public static class PrintPDF
    {
        public static void PrintOnePDF(FileInfo finfo, SamplePrintCancel cancel, SamplePrintProgress progress)
        {
            try
            {
                progress.PrintingStarted = false;   // Used to keep the Print Progress form hidden until printing begins

                //
                // This implements the core prep/actions related to printing (from DLE)
                // In order to support GUI refresh they MUST occur on a background thread.
                // Notifications related to Cancel and Print Progress all occur via callback, elsewhere.
                //

                using (Library lib = new Library())
                using (Document doc = new Document(finfo.FullName))
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

                            // If you un-comment the file path (binding) below, DLE will print via the platform print driver to disk.
                            // The type of file produced when doing so (e.g., PS, PCL, PCLXL) depends upon the type of printer/driver. 
                            //userParams.OutFileName = String.Format("{0}\\{1}.prn", finfo.Directory.FullName, System.IO.Path.GetFileNameWithoutExtension(finfo.FullName));

                            progress.NumPages = userParams.EndPage + 1;  // important! (establishes the range of the page progress bar)
                            progress.PrintingStarted = true;             // Just before print start, set this (so that the Print Progress form will show)

                            doc.Print(userParams, cancel, progress);     // Long running activity (cancel and progress will update via callback, elsewhere)
                        }
                    }
                }
            }
            catch (Exception ex)
            {   // Some PDFs won't print (for various reasons). All code should be written to expect failure (never assume success by default).
                Console.WriteLine("Exception when printing...{0}{1}", Environment.NewLine, ex.Message);
            }
            finally
            {   // Tell the Print Progress form we're done...
                progress.ReportProgressDone();
            }
        }
    }
}
