using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates converting a standard PDF document into a
 * PDF Archive, or PDF/A, compliant version of a PDF file.
 *
 * For more detail see the description of the PDFAConverter sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/converting-and-merging-pdf-content
 * 
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace PDFAConverter
{
    class PDFAConverter
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PDFAConverter Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String sOutput = "PDFAConverter-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Converting " + sInput + ", output file is " + sOutput);

                using (Document doc = new Document(sInput))
                {
                    // Make a conversion parameters object
                    PDFAConvertParams pdfaParams = new PDFAConvertParams();
                    pdfaParams.AbortIfXFAIsPresent = true;
                    pdfaParams.IgnoreFontErrors = false;
                    pdfaParams.NoValidationErrors = false;
                    pdfaParams.ValidateImplementationLimitsOfDocument = true;

                    // Create a PDF/A compliant version of the document
                    PDFAConvertResult pdfaResult = doc.CloneAsPDFADocument(PDFAConvertType.RGB3b, pdfaParams);

                    // The conversion may have failed: we must check if the result has a valid Document
                    if (pdfaResult.PDFADocument == null)
                    {
                        Console.WriteLine("ERROR: Could not convert " + sInput + " to PDF/A.");
                    }
                    else
                    {
                        Console.WriteLine("Successfully converted " + sInput + " to PDF/A.");

                        Document pdfaDoc = pdfaResult.PDFADocument;

                        //Save the result.
                        pdfaDoc.Save(pdfaResult.PDFASaveFlags, sOutput);
                    }
                }
            }
        }
    }
}
