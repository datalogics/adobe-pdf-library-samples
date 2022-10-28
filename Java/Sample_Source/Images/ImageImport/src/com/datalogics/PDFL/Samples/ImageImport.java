/*
 * 
 * This program demonstrates how to import images into a PDF file. The program runs without
 * prompting you, and creates two PDF files, demonstrating how to import graphics from image files
 * into a PDF file. One of the PDF output files is the result of graphics imported from a multi-page TIF file.
 * 
 * The sample has two optional arguments, one a JPG image file, and the other the name of the default PDF output file.
 * The program imports the JPG file into a PDF and saves it to an output PDF file.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.pdfl.samples.Images.ImageImport;

import java.util.EnumSet;

import com.datalogics.PDFL.*;

public class ImageImport {

    public static void main(String [] args) throws Exception {
        System.out.println("ImageImport sample:");
        System.out.println("Reading image files from " + Library.getResourceDirectory() +
                           " and writing ImageExport-out*.pdf");

        Library lib = new Library(); 
        try {
            Document doc = new Document();
            Image newimage;
            
            String sInput = Library.getResourceDirectory() + "Sample_Input/ducky.jpg";
            String sOutput = "ImageImport-out1.pdf";

            if (args.length > 0)
                sInput = args[0];

            if (args.length > 1)
                sOutput = args[1];
            
            System.out.println("Reading image file" + sInput + " and writing " + sOutput );

            // Create a PDF page which is one inch larger all around than this image
            // The design width and height for the image are carried in the Image's
            // Matrix's A and D fields, respectively.
            // There are 72 PDF user space units in one inch.
            newimage = new Image(sInput, doc);
            Matrix imageMatrix = newimage.getMatrix();
            Rect pageRect = new Rect(0, 0, imageMatrix.getA() + 144, imageMatrix.getD() + 144);
            Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);
            newimage.translate(72, 72);
            docpage.getContent().addElement(newimage);
            docpage.updateContent();

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);


        }
        finally {
            lib.delete();
        }
    }
}

