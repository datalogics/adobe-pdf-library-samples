using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * This sample shows how to convert a PDF document into a PDF/X compliant version of that file.
 * 
 * PDF/X is used for graphics exchange when printing content. It is a version of the PDF format that guarantees accuracy in colors used.
 * 
 * The sample takes a default input and output a document (both optional). 
 * 
 * For more detail see the description of the PDFXConverter sample program on our Developer’s site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/converting-and-merging-pdf-content#pdfxconverter
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace PDFXConverter
{
    class PDFXConverter
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PDFXConverter Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf"; ;
                String sOutput = "PDFXConverter-out-X1a-csh.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ". Writing to output " + sOutput);

                Document doc = new Document(sInput);

                // Make a conversion parameters object
                PDFXConvertParams pdfxParams = new PDFXConvertParams();

                // We'll set a property on it an as example
                pdfxParams.AbortIfXFAIsPresent = true;

                // Create a PDF/X compliant version of the document
                //   (Or use convert type PDFXConvertType.X3)
                PDFXConvertResult pdfxResult = doc.CloneAsPDFXDocument(PDFXConvertType.X1a, pdfxParams);

                // The conversion may have failed: we must check if the result has a valid Document
                if (pdfxResult.PDFXDocument == null)
                    Console.WriteLine("ERROR: Could not convert " + sInput + " to PDF/X.");
                else
                {
                    Console.WriteLine("Successfully converted " + sInput + " to PDF/X.");

                    Document pdfxDoc = pdfxResult.PDFXDocument;

                    // We MUST use the SaveFlags returned in the PDFConvertResult.
                    // If we do not use them, the document will no longer be PDF/X compliant.
                    pdfxDoc.Save(pdfxResult.PDFXSaveFlags, sOutput);

                    // Note that the returned document will have its major and minor
                    // version set--this is required for PDF/X compliance.  This is only
                    // visible AFTER you save the document.
                    Console.WriteLine(sOutput + " has version number: " + pdfxDoc.MajorVersion + "." + pdfxDoc.MinorVersion);
                }

            }
        }
    }
}
