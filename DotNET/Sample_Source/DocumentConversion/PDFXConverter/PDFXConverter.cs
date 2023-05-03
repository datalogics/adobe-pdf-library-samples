using System;
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
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/converting-and-merging-pdf-content#pdfxconverter
 *
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
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

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sOutput = "../PDFXConverter-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ". Writing to output " + sOutput);

                using (Document doc = new Document(sInput))
                {
                    // Make a conversion parameters object
                    PDFXConvertParams pdfxParams = new PDFXConvertParams();

                    // Create a PDF/X compliant version of the document
                    PDFXConvertResult pdfxResult = doc.CloneAsPDFXDocument(PDFXConvertType.X4, pdfxParams);

                    // The conversion may have failed, we must check if the result has a valid Document.
                    if (pdfxResult.PDFXDocument == null)
                        Console.WriteLine("ERROR: Could not convert " + sInput + " to PDF/X.");
                    else
                    {
                        Console.WriteLine("Successfully converted " + sInput + " to PDF/X.");

                        Document pdfxDoc = pdfxResult.PDFXDocument;

                        // Set the SaveFlags returned in the PDFConvertResult.
                        pdfxDoc.Save(pdfxResult.PDFXSaveFlags, sOutput);
                    }
                }
            }
        }
    }
}
