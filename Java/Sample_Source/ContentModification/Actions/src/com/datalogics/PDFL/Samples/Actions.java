package com.datalogics.PDFL.Samples;


import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.GoToAction;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.URIAction;
import com.datalogics.PDFL.ViewDestination;

/*
 * This sample creates a PDF document with a single page, featuring a rectangle.
 * An action is added to the rectangle in the form of a hyperlink; if the reader
 * clicks on the rectangle, the system opens the Datalogics web page.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class Actions {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("Actions sample:");
        String outFile = "Actions-out.pdf";

    	Library lib = new Library();

		try {
	        Document doc = new Document();
	        
	        Rect pageRect = new Rect(0, 0, 100, 100);
	        Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);
	        System.out.println("Created page.");
	        
	        // Create our first link with a URI action
	        LinkAnnotation newLink = new LinkAnnotation(docpage, new Rect(1.0, 2.0, 3.0, 4.0));
	        System.out.println(newLink.toString());
	        
	        doc.setBaseURI("http://www.datalogics.com");
	        URIAction uri = new URIAction("/products/pdfl/pdflibrary.asp", false);
	        System.out.println("Action data: " + uri.toString());
	        
	        newLink.setAction(uri);
	        
	        // Create a second link with a GoTo action
	        LinkAnnotation secondLink = new LinkAnnotation(docpage, new Rect(5.0, 6.0, 7.0, 8.0));
	        
	        Rect r = new Rect(5, 5, 100, 100);
	        GoToAction gta = new GoToAction(new ViewDestination(doc, 0, "FitR", r, 1.0));
	        System.out.println("Action data: " + gta.toString());
	        
	        secondLink.setAction(gta);
	        
	        // Read some URI properties
	        System.out.println("Extracted URI: " + uri.getURI());
	        
	        if (uri.getIsMap())
	        	System.out.println("Send mouse coordinates");
	        else
	        	System.out.println("Don't send mouse coordinates");
	        
	        // Change the URI properties
	        doc.setBaseURI("http://www.datalogics.com");
	        uri.setURI("/products/pdfl/pdflibrary.asp");
	        
	        uri.setIsMap(true);
	        
	        System.out.println("Complete changed URI:" + doc.getBaseURI() + uri.getURI());

	        if (uri.getIsMap())
	        	System.out.println("Send mouse coordinates");
	        else
	        	System.out.println("Don't send mouse coordinates");
	        
	        // Read some GoTo properties
	        System.out.println("Fit type of destination: " + gta.getDestination().getFitType().toString());
	        System.out.println("Rectangle of destination: " + gta.getDestination().getDestRect().toString());
	        System.out.println("Zoom of destination: " + gta.getDestination().getZoom());
	        System.out.println("Page number of destination: " + gta.getDestination().getPageNumber());

                System.out.println("Saving to file: " + outFile);
                doc.save(EnumSet.of(SaveFlags.FULL), outFile);
            
		}
		finally {
			lib.delete();
		}
	}

}
