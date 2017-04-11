package com.datalogics.PDFL.Samples;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.EnumSet;

import com.datalogics.PDFL.*;

/*
 * 
 * This sample shows how to list metadata about a PDF document. The program also provides an option 
 * to run interactively, and present command prompts to chagne these metadata values, if you uncomment
 * the "readLine" statements. For example, you could then use the program to enter new metadata values
 * for the document title or author.
 * 
 * The results are exported to a PDF output document.
 * 
 * For more detail see the description of the List sample programs, and ListInfo, on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 * 
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class ListInfo {
    public static void main (String[] args) throws Throwable
    {
        System.out.println("ListInfo sample:");

    	Library lib = new Library();

		try {
                String sInput = "../../Resources/Sample_Input/sample.pdf";
                String sOutput = "ListInfo-out.pdf";
                if (args.length > 0)
                    sInput = args[0];
                if (args.length > 1)
                    sOutput = args[1];
                System.out.println("Input file: " + sInput + ", will write with changed metadata to " + sOutput);
	        
	       	Document doc = new Document(sInput);
	       	
	       	System.out.println("Opened a document.");
	       	
	       	BufferedReader stdin = new BufferedReader(new InputStreamReader(System.in));

                // NOTE: To run interactively and enter data of your choosing, un-comment
                //    the following "readLine" lines
	       	System.out.println("Document Title " + doc.getTitle());
	       	System.out.println("Change document title to: ");
	       	String title = "The new title";
	       	//String title = stdin.readLine();
	       	doc.setTitle(title);
	       	
	       	System.out.println("Document Subject " + doc.getSubject());
	       	System.out.println("Change document subject to: ");
	       	String subject = "The new subject";
	       	//String subject = stdin.readLine();
	       	doc.setSubject(subject);
	       	
	       	System.out.println("Document Author " + doc.getAuthor());
	       	System.out.println("Change document author to: ");
	       	String author = "The new author";
	       	//String author = stdin.readLine();
	       	doc.setAuthor(author);
	       	
	       	System.out.println("Document Keywords " + doc.getKeywords());
	       	System.out.println("Change document keywords to: ");
	       	String keywords = "New keywords";
	       	//String keywords = stdin.readLine();
	       	doc.setKeywords(keywords);
	       	
	       	System.out.println("Document Creator " + doc.getCreator());
	       	System.out.println("Change document Creator to: ");
	       	String Creator = "New creator";
	       	//String Creator = stdin.readLine();
	       	doc.setCreator(Creator);
	       	
	       	System.out.println("Document Producer " + doc.getProducer());
	       	System.out.println("Change document producer to: ");
	       	String producer = "New producer";
	       	//String producer = stdin.readLine();
	       	doc.setProducer(producer);

            doc.save(EnumSet.of(SaveFlags.FULL, SaveFlags.LINEARIZED), sOutput);
		}
		finally {
			lib.delete();
		}
	}

};
