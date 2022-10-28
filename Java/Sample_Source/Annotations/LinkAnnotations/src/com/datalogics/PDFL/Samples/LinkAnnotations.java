package com.datalogics.pdfl.samples.Annotations.LinkAnnotations;

import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.HighlightStyle;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.ViewDestination;

/*
 * 
 * This program creates a PDF file with an embedded hyperlink, which takes the reader to the second page of the document.
 * 
 * For more detail see the description of the LinkAnnotation sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-annotations-in-pdf-files#linkannotation
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 */
public class LinkAnnotations {

    /**
     * @param args
     */
    public static void main(String[] args) throws Throwable {
        System.out.println("LinkAnnotations sample:");

        Library lib = new Library();

        try {
            String sInput = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
            String sOutput = "LinkAnnotations-out.pdf";
            
            if (args.length > 0) 
                sInput = args[0];
            if (args.length > 1)
                sOutput = args[1];

            Document doc = new Document(sInput);

            System.out.println("Opening " + sInput + "; will write to " + sOutput);

            Page docpage = doc.getPage(0);

            LinkAnnotation newLink = new LinkAnnotation(docpage, new Rect(1.0, 2.0, 3.0, 4.0));

            // Test some link features
            newLink.setNormalAppearance(newLink.generateAppearance());

            System.out.println("Current Link Annotation version = " + newLink.getAnnotationFeatureLevel());
            newLink.setAnnotationFeatureLevel(1.0);
            System.out.println("New Link Annotation version =" + newLink.getAnnotationFeatureLevel());

            // Give the link a destination
            ViewDestination dest = new ViewDestination(doc, 0, "XYZ", doc.getPage(0).getMediaBox(), 1.5);
            dest.setDestRect(new Rect(0.0, 0.0, 200.0, 200.0));
            System.out.println("The new destination rectangle: " + dest.getDestRect().toString());

            dest.setFitType("FitV");
            System.out.println("The new fit type: " + dest.getFitType());

            dest.setZoom(2.5);
            System.out.println("The new zoom level: " + dest.getZoom());

            dest.setPageNumber(1);
            System.out.println("The new page number: " + dest.getPageNumber());

            newLink.setDestination(dest);

            // Set the link highlight
            newLink.setHighlight(HighlightStyle.INVERT);

            if (newLink.getHighlight() == HighlightStyle.INVERT) {
                System.out.println("Invert highlighting.");
            }

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);

        } finally {
            lib.delete();
        }
    }
}
