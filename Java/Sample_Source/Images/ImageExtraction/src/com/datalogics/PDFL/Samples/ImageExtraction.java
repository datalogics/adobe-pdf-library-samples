package com.datalogics.PDFL.Samples;

/*
 * 
 * This sample program searches through the PDF file that you select and identifies
 * raster drawings, diagrams and photographs among the text. Then, it extracts these
 * images from the PDF file and copies them to a separate set of graphics files in the
 * same directory. Vector images, such as clip art, will not be exported.
 * 
 * For more detail see the description of the ImageExtraction sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/exporting-images-from-pdf-files/#imageextraction
 * 
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

import java.io.BufferedReader;
import java.io.InputStreamReader;
import javax.imageio.ImageIO;
import java.io.File;

import com.datalogics.PDFL.*;

public class ImageExtraction {

    static int next = 0;

    public static void extractImages(Content content) throws Exception {
        for (int i = 0; i < content.getNumElements(); i++) {
            Element e = content.getElement(i);
            if (e instanceof Image) {
                Image img = (Image)e;
                ImageIO.write(img.getBufferedImage(), "bmp", new File("ImageExtraction-extract-out" + (next++) + ".bmp"));
                // the bitmap may be saved in any format supported by ImageIO, e.g.:
                //ImageIO.write(img, "jpg", new File("extract" + i + ".jpg"));
                //ImageIO.write(img, "png", new File("extract" + i + ".png"));
            } else if (e instanceof Container) {
                extractImages(((Container)e).getContent());
            } else if (e instanceof Group) {
                extractImages(((Group)e).getContent());
            } else if (e instanceof Form) {
                extractImages(((Form)e).getContent());
            }
        }
    }

    public static void main(String [] args) throws Exception {
        System.out.println("ImageExtraction sample:");
    	
        Library lib = new Library();

        try {
            String filename = "../../Resources/Sample_Input/ducky.pdf";
            if (args.length > 0) {
                filename = args[0];
            }
            System.out.println("Input file: " + filename);
            Document doc = new Document(filename);
            System.setProperty("java.awt.headless", "true");
            Page pg = doc.getPage(0);
            Content content = pg.getContent();
            extractImages(content);
        } 
        finally {
            lib.delete();
        }
    }
}

