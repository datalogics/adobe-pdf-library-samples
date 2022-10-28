package com.datalogics.pdfl.samples.Annotations.AnnotationCopyPaste;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.EnumSet;

import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;

/*
 * 
 * This sample demonstrates how to copy and paste annotations from one PDF file to another.
 * 
 * The program provides two optional input PDF documents. The first is the source of the annotations, and the second
 * is the PDF document that receives these copied annotations. The program also provides a default name and path for 
 * the output PDF document. The second input document, Layers.pdf, is saved as the output document Paste-out.pdf.
 * 
 * You can provide your own file names for these values in the code, or you can enter your own file names as
 * command line parameters.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class AnnotationCopyPaste {

    /**
     * @param args
     * @throws IOException
     */
    public static void main(String[] args) throws Exception {
        System.out.println("AnnotationCopyPaste sample:");
        
        Library lib = new Library();
        
        try {
            String sInput1 = Library.getResourceDirectory() + "Sample_Input/sample_annotations.pdf";
            String sInput2 = Library.getResourceDirectory() + "Sample_Input/Layers.pdf";
            String sOutput = "AnnotationCopyPaste-out.pdf";
            
            if (args.length > 0) {
                sInput1 = args[0];
            }

            Document sourceDoc = new Document(sInput1);

            if (args.length > 1) {
                sInput2 = args[1];
            }
            
            Document destinationDoc = new Document(sInput2);
            
            if (args.length > 2){
                sOutput = args[2];
            }

            System.out.println("Copying annotations from " + sInput1 + " into " +
                sInput2 + " and writing to " + sOutput);
            
            Page sourcePage = sourceDoc.getPage(0);
            Page destinationPage = destinationDoc.getPage(0);
            
            int nAnnots = sourcePage.getNumAnnotations();

            // find each link annotation on the first page of the source document and
            // copy them to the first page of the destination document
            for (int i = 0; i < nAnnots; i++) {
                Annotation ann = sourcePage.getAnnotation(i);

                if (ann.getSubtype().equals("Link")) {
                    Rect linkRect = ann.getRect();
                    Point linkCenter = new Point();
                    
                    // find the center of the link
                    linkCenter.setH(linkRect.getLeft() + linkRect.getWidth() / 2);
                    linkCenter.setV(linkRect.getBottom() + linkRect.getHeight() / 2);
                    
                    try {
                        // copy the annotation to the destination and center it on 
                        // the center point of the original annotation.
                        //
                        // This may throw a RuntimeException if it cannot 
                        // copy/paste the annotation, in either case the message in
                        // the exception will specify which operation (copy or paste)
                        // that it could not complete.
                       ((LinkAnnotation)ann).copyTo(destinationPage, linkCenter);
                    }
                    catch (RuntimeException ae)
                    {
                        System.out.println(ae.getMessage());
                    }
                }
            }
            
            destinationDoc.save(EnumSet.of(SaveFlags.FULL), sOutput);
        }
        finally
        {
            lib.delete();
        }
        
    }
    
}
