using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates converting a standard PDF document into a
 * PDF Archive, or PDF/A, compliant version of a PDF file.
 *
 * For more detail see the description of the PDFAConverter sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/converting-and-merging-pdf-content
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
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

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = "../../Resources/Sample_Input/ducky.pdf";
                String sOutput = "../PDFAConverter-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Converting " + sInput + ", output file is " +  sOutput);

                Document doc = new Document(sInput);

                // Make a conversion parameters object
                PDFAConvertParams pdfaParams = new PDFAConvertParams();

                // We'll set a property on it an as example
                pdfaParams.AbortIfXFAIsPresent = true;

                // Create a PDF/A compliant version of the document
                //   (Or use convert type PDFAConvertType.CMYK1b)
                PDFAConvertResult pdfaResult = doc.CloneAsPDFADocument(PDFAConvertType.RGB1b, pdfaParams);

                // The conversion may have failed: we must check if the result has a valid Document
                if (pdfaResult.PDFADocument == null)
                    Console.WriteLine("ERROR: Could not convert " + sInput + " to PDF/A.");
                else
                {
                    Console.WriteLine("Successfully converted " + sInput + " to PDF/A.");

                    Document pdfaDoc = pdfaResult.PDFADocument;

                    // We MUST use the SaveFlags returned in the PDFConvertResult.
                    // If we do not use them, the document will no longer be PDF/A compliant.
                    pdfaDoc.Save(pdfaResult.PDFASaveFlags, sOutput);

                    // Note that the returned document will have its major and minor
                    // version set--this is required for PDF/A compliance.  This is only
                    // visible AFTER you save the document.
                    Console.WriteLine(sOutput + " has version number: " + pdfaDoc.MajorVersion + "." + pdfaDoc.MinorVersion);
                }

            }
        }
    }
}
