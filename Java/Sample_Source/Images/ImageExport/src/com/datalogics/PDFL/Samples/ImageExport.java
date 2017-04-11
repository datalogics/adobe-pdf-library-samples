package com.datalogics.PDFL.Samples;

/*
 *  
 * This sample program reads the pages of the PDF file that you provide and extracts images
 * that it finds on each page and saves those images to external graphics files, one each for
 * TIF, JPG, PNG, GIF, and BMP. 
 * 
 * To be more specific, the program examines the content stream for image elements and exports
 * those image objects. If a page in a PDF file has three images, the program will create three
 * sets of graphics files for those three images. The sample program ignores text, parsing the
 * PDF syntax to identify any raster or vector images found on every page.
 *
 * For more detail see the description of the ImageExport sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/exporting-images-from-pdf-files
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

import java.io.BufferedReader;
import java.io.InputStreamReader;
import com.datalogics.PDFL.*;

public class ImageExport
{
 
    public static void main(String[] args)
    {
        System.out.println("Image Export sample:");

        Library lib = new Library();
        try {
            System.out.println("Initialized the library.");
            String filename = "../../Resources/Sample_Input/ducky.pdf";
            if (args.length > 0) {
                filename = args[0];
            }
            System.out.println("Input file " + filename );

            Document doc = new Document(filename);

            ExportDocumentImages expdoc= new ExportDocumentImages();
            expdoc.export_doc_images(doc);
        }
        finally {
            lib.delete();
        }
    }
}
