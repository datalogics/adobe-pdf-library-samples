package com.datalogics.pdfl.samples.Images.ImageFromBufferedImage;

import java.io.BufferedInputStream;
import java.io.FileInputStream;
import java.io.InputStream;
import java.util.EnumSet;

import javax.imageio.stream.ImageInputStream;
import javax.imageio.stream.MemoryCacheImageInputStream;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Image;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;

/*
 *
 * This sample shows how to create an image object in a PDF document by drawing a graphic from 
 * memory, rather than from another PDF document.
 *
 * For more detail see the description of this sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/importing-images-into-pdf-files/
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class ImageFromBufferedImage {

    /**
     * @param args
     */
    public static void main(String[] args) throws Throwable {
        System.out.println("ImageFromBufferedImage sample:");
        
        System.setProperty("java.awt.headless", "true");

        Library lib = new Library();

        try {
            Document document = null;
            Page page = null;
            Image img = null;
            Document doc = null;
            Page docpage = null;
            
            String sInput = Library.getResourceDirectory() + "Sample_Input/ducky.jpg";
            String sOutput = "ImageFromBufferedImage.pdf";
            
            if (args.length > 0)
                sInput = args[0];
            
            if (args.length > 1)
                sOutput = args[1];
            
            System.out.println("Input file: " + sInput + ", will write to " + sOutput);

            try {
                InputStream fileStream = new BufferedInputStream(new FileInputStream(sInput));
                ImageInputStream inputStream = new MemoryCacheImageInputStream(fileStream);

                img = new Image(inputStream);

                doc = new Document();
                Rect pageRect = new Rect(0, 0, img.getWidth(), img.getHeight());

                docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);

                docpage.getContent().addElement(img);
                docpage.updateContent();

                doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
            }
            finally {
                if (document != null) {
                    document.delete();
                }
                if (page != null) {
                    page.delete();
                }
                if (img != null) {
                    img.delete();
                }
                if (doc != null) {
                    doc.delete();
                }
                if (docpage != null) {
                    docpage.delete();
                }
            }
        }
        finally {
            lib.delete();
        }
    }
}
