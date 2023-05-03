using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * Run this program to automatically send a PDF file to a printer.  The program prompts for
 * the PDF file name, and then it sends the PDF file to the default printer assigned to the
 * computer in use.  This program is useful in that it identifies the API you need to use to
 * print PDF files.
 * 
 * The program also generates a Postscript (PS) output file. If you open it, the Adobe PDF Distiller 
 * runs and automatically generates a PDF output file from the PS file.  This file will match the PDF
 * file you entered to print.
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PrintPDF
{
    class SamplePrintProgressProc : PrintProgressProc
    {
        /// <summary>
        /// The method implements a callback (it MUST be named "Call" and exhibit the method signature described)
        /// It will be driven by PDFL and provide data that can be used to update a progress bar, etc.
        /// </summary>
        /// <param name="page">The current page number. It is *always* 0-based. One *will* encounter a value of -1. That means "not applicable".</param>
        /// <param name="totalPages">The total number of pages printing. One *may* encounter a value of -1. That means "not applicable".</param>
        /// <param name="stagePercent">A percentage complete (of the stage). Values will always be in the range of 0.0 (0%) to 1.0 (100%)</param>
        /// <param name="info">A string that will present optional information that may be written to user interface</param>
        /// <param name="stage">An enumeration that indicates the current printing stage</param>
        public override void Call(int page, int totalPages, float stagePercent, string info, PrintProgressStage stage)
        {
            mSomeBoolean = ((mSomeBoolean) ? false : true);
            Console.WriteLine("Print Progress Proc was called.");

            //
            // Stage Information (info) is a *English language only* stage information string (e.g., font name)
            // suitable for use as (pre-localized) source material for a text label related to the current stage.
            // Most stages do not present any stage information (it's never null but *often* empty). For those
            // conditions one must create their own string value(s) to represent the stage name.
            // Stage (stage) is an enumeration of the stage (type). Switch on this value (always) to
            // vary the user presentation of any stage progress bar/info.
            //
            // For more detail (and inspiration) consult the PrintPDFGUI sample. The SamplePrintProgressProc
            // class (there) has a much richer implementation.
            //

            switch (stage)
            {
                case PrintProgressStage.PrintPage:
                    if (stagePercent < 1F)
                    {
                        Console.WriteLine("Printing Page {0} of {1}", page + 1, totalPages);
                    }

                    break;
            }
        }

        static private bool mSomeBoolean;
    }

    class PrintPDF
    {
        static void Main(string[] args)
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Console.WriteLine("For .Net 6, System.Drawing.Printing.PrinterSettings is only supported on Windows.");
                return;
            }

            Console.WriteLine("PrintPDF Sample:");

            System.Drawing.Printing.PrinterSettings settings = new System.Drawing.Printing.PrinterSettings();
            if (String.IsNullOrEmpty(settings.PrinterName))
            {
                Console.WriteLine("A printer must be made available to use this sample.");
                return;
            }

            try
            {

                // Printing may fail for reasons that have nothing to do with APDFL.
                // PDF documents may contain material that cannot be printed, etc. Given
                // that, it's always best to expect the printing step to fail and take
                // appropriate measures. Such care becomes critical for server-based 24/7
                // systems. This try block is simply here as an example (the steps one
                // would normally take are more involved).

                // To use APDFL one must always begin by initializing the library. This action
                // is expensive (both time and resource wise) and should only be done when necessary.
                // In a threaded product, a separate library must be instantiated on each thread.
                // ReSharper disable once UnusedVariable
                using (Library lib = new Library())
                {
                    Console.WriteLine(@"Library initialized.");

                    String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";

                    // HINT: you'll find the file (in the working directory) next to PrintPDF.exe
                    String outFileNamePrn = "PrintPDF_out.prn";

                    // HINT: you'll find the file (in the working directory) next to PrintPDF.exe
                    string outFileNamePs = "PrintPDF_out.ps";

                    if (args.Length > 0)
                        sInput = args[0];

                    Console.WriteLine("Input file: " + sInput + ". Writing to output " + outFileNamePrn + " and " +
                                      outFileNamePs);

                    // Open a PDF document ("using" will automatically .Close and .Dispose it)...
                    using (Document doc = new Document(sInput))
                    {
                        #region Print To File (via Printer Driver)

                        // Platform print to a file...
                        //
                        // Printed output from the following method is composed by the selected
                        // printer's driver; along with assistance from APDFL. The actual
                        // output format will vary (e.g., PCL, PostScript, XPS, etc.). PostScript
                        // files produced via a PostScript driver and this method are NOT suitable
                        // for Distillation, Normalization, etc. All output from the method below
                        // will be optimized specifically for the target printer and is only suitable
                        // for direct consumption by that printer (or a print spooler that will
                        // ultimately transfer it to the target printer).

                        using (PrintUserParams userParams = new PrintUserParams())
                        {
                            // NOTE: userParams are only valid for ONE print job...
                            userParams.NCopies = 1;
                            userParams.PrintParams.ExpandToFit = true;

#if WINDOWS
                            userParams.ShrinkToFit = true;

                            // This sets the file name that the printer driver knows about
                            // It's completely optional and only needs to be set if one wants
                            // a value that differs from the PDF file name (which is
                            // automatically provided by default). It is used for the
                            // %%Title comment in a PostScript file, etc.

                            //Isolate the filename from the path
                            int index = sInput.LastIndexOf("/");
                            String nameOnly;

                            if (index != -1)
                                nameOnly = sInput.Substring(index + 1);
                            else
                                nameOnly = sInput;

                            userParams.InFileName = nameOnly;
#endif

                            // When print to file you MUST use 1+ page ranges...
                            // Page ranges allow complex collections of pages to be emitted.
                            // If you don't provide a page range, only the first page of the document will print.
                            //
                            // The code below creates 3 page ranges...
                            // As specified (below), PDFL will emit up to 8 pages (1-4, 2, 4, 1, 3).
                            int upToFourPages = ((doc.NumPages > 4) ? 3 : doc.NumPages - 1); // 0-based
                            if (upToFourPages == 0)
                            {
                                // the end page must always be >= 1
                                upToFourPages = 1;
                            }

                            IList<PageRange> pageRanges = new List<PageRange>();
                            pageRanges.Add(new PageRange(0, upToFourPages, PageSpec.AllPages)); // p1-4
                            if (doc.NumPages > 1)
                            {
                                // you can't ask for even or odd pages from a 1 page document
                                pageRanges.Add(new PageRange(0, upToFourPages, PageSpec.OddPagesOnly)); // p1,3
                                pageRanges.Add(new PageRange(0, upToFourPages, PageSpec.EvenPagesOnly)); // p2,4
                            }

                            PrintParams printParams = userParams.PrintParams;
                            printParams.PageRanges = pageRanges;

                            // Why .prn? Because we're printing via the printer driver (here)
                            // and cannot tell what type of output the driver will produce.
                            // PostScript drivers will produce PostScript output. PCL drivers
                            // will produce PCL or PCL_XL. Similarly, XPS drivers may produce
                            // XPS and PDF drivers may produce PDF (directly).
                            //
                            // PostScript produced via the PrintToFile method is NOT equivalent
                            // to PostScript produced via the ExportAsPostScript method.

                            Console.WriteLine("Printing to File: {0}", outFileNamePrn);
                            doc.PrintToFile(userParams, null /* for cancel see the PrintPDFGUI sample */,
                                new SamplePrintProgressProc(), outFileNamePrn);
                        }

                        #endregion

                        #region Print To Printer (via Printer Driver)

                        // Now let's, print directly to a printer (without ui)...
                        //
                        // Printed output from the following method is composed by the selected
                        // printer's driver; along with assistance from APDFL. The actual
                        // output format will vary (e.g., PCL, PostScript, XPS, etc.). PostScript
                        // files produced via a PostScript driver and this method are NOT suitable
                        // for Distillation, Normalization, etc. All output from the method below
                        // will be optimized specifically for the target printer and is only suitable
                        // for direct consumption by that printer (or a print spooler that will
                        // ultimately transfer it to the target printer).

                        using (PrintUserParams userParams = new PrintUserParams())
                        {
                            // NOTE: userParams are only valid for ONE print job...
                            userParams.NCopies = 1;
                            userParams.PrintParams.ExpandToFit = true;

#if WINDOWS
                            userParams.ShrinkToFit = true;

                            // This sets the file name that the printer driver knows about
                            // It's completely optional and only needs to be set if one wants
                            // a value that differs from the PDF file name (which is
                            // automatically provided by default). It is used for the
                            // %%Title comment in a PostScript file, etc.

                            //Isolate the filename from the path
                            int index = sInput.LastIndexOf("/");
                            String nameOnly;

                            if (index != -1)
                                nameOnly = sInput.Substring(index + 1);
                            else
                                nameOnly = sInput;

                            userParams.InFileName = nameOnly;

                            // When printing (directly) to a printer you cannot use page ranges...
                            // You need to use userParams.StartPage and EndPage (to request a single,
                            // linear sequence) instead. If you do not specify anything the entire
                            // document will print (i.e., all pages).
                            //
                            // As specified (below), PDFL will print up to 4 pages (1-4).
                            int upToFourPages = ((doc.NumPages > 4) ? 3 : doc.NumPages - 1);
                            userParams.StartPage = 0;           // 0-based
                            userParams.EndPage = upToFourPages; // 0-based
#endif

                            // Use the default printer...
#if Windows || MacOS
                            userParams.UseDefaultPrinter(doc);
#endif

                            // ...or uncomment the code below (and comment out the code above) and
                            // assign the name of a (accessible) printer to userParams.DeviceName so
                            // as to explicitly target a printer.
                            //userParams.DeviceName = @"Change Me to a valid Printer Name";

#if WINDOWS
                            Console.WriteLine(String.Format("Printing (direct) to Printer: {0}", userParams.DeviceName));
#endif
                            doc.Print(userParams, null /* for cancel see the PrintPDFGUI sample */,
                                new SamplePrintProgressProc());
                        }

                        #endregion

                        #region Export As PostScript (Device Independent, DSC Compliant)

                        // Export as (PDFL composed) PostScript...
                        //
                        // PostScript files produced via this *export* method are suitable
                        // for Distillation, Normalization, etc. If a PostScript Printer
                        // Description (PPD) is registered (not shown here) the output will
                        // be device specific. Otherwise, it will be device independent.
                        // Consult the PostScript Language Document Structuring Conventions
                        // for more information about the conformance / structure of the
                        // exported PostScript.
                        //
                        // https://partners.adobe.com/public/developer/en/ps/5001.DSC_Spec.pdf

                        using (PrintUserParams userParams = new PrintUserParams())
                        {
                            // NOTE: userParams are only valid for ONE print job...
                            userParams.NCopies = 1;

                            // When export as PostScript you MUST use 1+ page ranges...
                            // Page ranges allow complex collections of pages to be emitted.
                            //
                            // The code below exports the entire PDF document (to a PostScript file)...
                            IList<PageRange> pageRanges = new List<PageRange>();
                            pageRanges.Add(new PageRange(0, (((doc.NumPages > 1)) ? doc.NumPages - 1 : 1),
                                PageSpec.AllPages)); // all pages
                            PrintParams printParams = userParams.PrintParams;
                            printParams.PageRanges = pageRanges;

                            Console.WriteLine("Exporting as PostScript to File: {0}", outFileNamePs);
                            doc.ExportAsPostScript(userParams, null /* for cancel see the PrintPDFGUI sample */,
                                new SamplePrintProgressProc(), outFileNamePs);
                        }

                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"An exception occurred. Here is the related information:");
                Console.Write(ex.ToString());
            }
        }
    }
}
