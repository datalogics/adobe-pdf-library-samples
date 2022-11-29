package com.datalogics.pdfl.samples.DocumentConversion.PDFXConverter;

import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.PDFXConvertParams;
import com.datalogics.PDFL.PDFXConvertType;
import com.datalogics.PDFL.PDFXConvertResult;

/*
 * 
 * This sample shows how to convert a PDF document into a PDF/X compliant version of that file.
 * 
 * PDF/X is used for graphics exchange when printing content. It is a version of the PDF format that guarantees accuracy in colors used.
 * 
 * The sample takes a default input and output a document (both optional). 
 * 
 * For more detail see the description of the PDFXConverter sample program on our Developer's site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/converting-and-merging-pdf-content#pdfxconverter
 *
 * Copyright (c) 2017-2022, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class PDFXConverter
{
    public static void main(String[] args) throws Exception {
        System.out.println("PDFXConverter Sample:");

        Library lib = new Library();
        try {
            System.out.println("Initialzed the library.");

            String filename = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
            String outnamePDFX = "PDFXConverter-out.pdf";
            if (args.length > 0) 
                filename = args[0];
            if (args.length > 1 )
                outnamePDFX = args[1];
                          
            Document doc = new Document(filename);

            // Make a conversion parameters object
            PDFXConvertParams pdfxParams = new PDFXConvertParams();

            // Create a PDF/X compliant version of the document
            PDFXConvertResult pdfxResult = doc.cloneAsPDFXDocument(PDFXConvertType.X4, pdfxParams);

            // The conversion may have failed, we must check if the result has a valid Document.
            if (pdfxResult.getPDFXDocument() == null)
                System.out.println("ERROR: Could not convert " + filename + " to PDF/X.");
            else
            {
                System.out.println("Successfully converted " + filename + " to PDF/X.");

                Document pdfxDoc = pdfxResult.getPDFXDocument();

                // Set the SaveFlags returned in the PDFConvertResult.
                pdfxDoc.save(pdfxResult.getPDFXSaveFlags(), outnamePDFX);
            }
        }
        finally
        {
            lib.delete();
        }
    }
}
