package com.datalogics.pdfl.samples.ContentModification.PDFObject;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.EnumSet;

import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.PDFString;
import com.datalogics.PDFL.PDFDict;
import com.datalogics.PDFL.URIAction;
import com.datalogics.PDFL.LinkAnnotation;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
/*
 * This sample demonstrates working with data objects in a PDF document. It examines the Objects and displays
 * information about them.  The sample extracts the dictionary for an object called URIAction and updates it using PDFObjects.
 * 
 * For more detail see the description of the PDFObject sample on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files#pdfobject
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class PDFObjectSample {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Exception {
        System.out.println("PDFObject sample:");

        Library lib = new Library();
        try {

		String sInput = Library.getResourceDirectory() + "Sample_Input/sample_links.pdf";
                String sOutput = "PDFObject-out.pdf";
        	if (args.length > 0) 
        		sInput = args[0];
                if (args.length > 1)
                        sOutput = args[1];
                System.out.println("Input: " + sInput + "; output: " + sOutput);

        	Document doc = new Document(sInput);
        	Page pg = doc.getPage(0);
        	
        	LinkAnnotation annot = (LinkAnnotation)pg.getAnnotation(1);
        	URIAction uri = (URIAction)annot.getAction();
        	
        	// Print some info about the URI action, before we modify it
        	System.out.println("Initial URL: " + uri.getURI());
        	System.out.println("Is Map property: " + uri.getIsMap());
        	
            // Modify the URIAction
            //
            // A URI action is a dictionary containing:
            //    Key: S     Contents: a name object with the value "URI" (required)
            //    Key: URI   Contents: a string object for the uniform resource locator (required)
            //    Key: IsMap Contents: a boolean for whether the link is part of a map (optional)
            //    (see section 8.5.3, "Action Types", of the PDF Reference)
            //
            // We will change the URI entry and delete the IsMap entry for this dictionary
        	
        	PDFDict uri_dict = uri.getPDFDict();	// Extract the dictionary
        	
        	// Create a new string object
        	PDFString uri_string = new PDFString("http://www.google.com", doc, false, false);
        	
        	uri_dict.put("URI", uri_string);	// Change the URI (replaces the old one)
        	uri_dict.remove("IsMap");			// Remove the IsMap entry
        	
        	// Check that we deleted the IsMap entry
        	System.out.println("Does this dictionary have an IsMap entry? " + uri_dict.contains("IsMap"));
        	
        	// Save the changed document out
        	doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
        	doc.close();
        	
        	// Check the modified contents of the link
        	doc = new Document(sOutput);
        	pg = doc.getPage(0);
        	annot = (LinkAnnotation)pg.getAnnotation(1);
        	uri = (URIAction)annot.getAction();
        	
        	System.out.println("Modified URL: " + uri.getURI());
        	System.out.println("Is Map property (if not present, defaults to false): " + uri.getIsMap());
        } 
        finally
        {
        	lib.delete();
        }
	}
}
