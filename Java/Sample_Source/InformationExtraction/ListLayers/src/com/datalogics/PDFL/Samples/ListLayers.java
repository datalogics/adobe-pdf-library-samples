package com.datalogics.pdfl.samples.InformationExtraction.ListLayers;
/*
 * 
 * A sample which demonstrates the use of the DLE API to list the
 * separate color layers of a PDF file.
 *
 * This sample searches for and lists the names of the color layers found in a PDF document. 
 *  
 * For more detail see the description of the List sample programs, and ListLayers, on our Developer’s site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files
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
import java.util.List;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.OptionalContentContext;
import com.datalogics.PDFL.OptionalContentGroup;

public class ListLayers {

	/**
	 * @param args
	 * @throws Throwable 
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("ListLayers sample:");

    	Library lib = new Library();

		try {
	        String filename = Library.getResourceDirectory() + "Sample_Input/Layers.pdf";
	        if (args.length > 0) {
	            filename = args[0];
                }
	        System.out.println("Layers in file: " + filename ); 
	        Document doc = new Document(filename);

	        List<OptionalContentGroup> ocgs = doc.getOptionalContentGroups();
	        for (OptionalContentGroup ocg : ocgs)
	        {
	        	System.out.println(ocg.getName());
	        	System.out.print("  Intent: ");
	        	System.out.println(ocg.getIntent());
	        }
	        
	        OptionalContentContext ctx = doc.getOptionalContentContext();
	        System.out.print("Optional content states: ");
	        System.out.println(ctx.getOCGStates(ocgs));
		}
		finally {
			lib.delete();
		}
	}
}
