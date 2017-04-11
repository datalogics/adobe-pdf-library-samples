package com.datalogics.PDFL.Samples;


import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.FileSpecification;
import com.datalogics.PDFL.LaunchAction;

/*
 * This sample creates a PDF document with a single blank page, featuring a rectangle.
 * An action is added to the rectangle in the form of a hyperlink; if the reader clicks
 * on the rectangle, a different PDF file opens, showing an image.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class LaunchActions {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("LaunchActions sample:");

        Library lib = new Library();

        try {
            Document doc = new Document();

            String sInput = "../../Resources/Sample_Input/ducky.pdf";
            String sOutput = "LaunchActions-out.pdf";
            
            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1)
                sOutput = args[1];

            System.out.println("Input file: " + sInput + ", will write to " + sOutput);

            // Standard letter size page (8.5" x 11")
            Rect pageRect = new Rect(0, 0, 612, 792);
            Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);
            System.out.println("Created page.");
            
            LinkAnnotation newLink = new LinkAnnotation(docpage, new Rect(153, 198, 306, 396));
            System.out.println("Created new Link Annotation");
            
            // LaunchActions need a FileSpecification object.
            //
            // The FileSpecification specifies which file should be opened when the LinkAnnotation is "clicked".
            //
            // Both FileSpecification objects and LaunchAction objects must be associated with a Document.
            // The association happens at object creation time and cannot be changed.
            //
            // FileSpecifications are associated with the Document that is passed in the constructor.
            // LaunchActions are associated with the same Document as the FileSpecification used to create it.

            // FileSpecifications can take either a relative or an absolute path. It is best to specify
            // a relative path if the document is intended to work across multiple platforms (Windows, Linux, Mac)    
            FileSpecification fileSpec = new FileSpecification(doc, sInput);
            System.out.println("Created a new FileSpecification with a path : " + fileSpec.getPath());

            LaunchAction launch = new LaunchAction(fileSpec);
            System.out.println("Created a new Launch Action");

            // setting NewWinow to true causes the document to open in a new window,
            // by default this is set to false.	        
            launch.setNewWindow(true);               
  
            newLink.setAction(launch);

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
           
            System.out.println("Output saved as " + sOutput );
        }
        finally {
            lib.delete();
        }
    }

}
