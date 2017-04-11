package com.datalogics.PDFL.Samples;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.*;

import com.datalogics.PDFL.*;

/*
 * 
 * This sample lists the text for the words in a PDF document.
 * 
 * For more detail see the description of the List sample programs, and ListWords, on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class ListWords {
    public static void main (String[] args) throws Throwable
    {
        System.out.println("ListWords sample:");

    	Library lib = new Library();

		try {
	        String filename = "../../Resources/Sample_Input/sample.pdf";
	        if (args.length > 0) {
	            filename = args[0];
                }
	        
                System.out.println("Words in file " + filename + ":");
	       	Document doc = new Document(filename);
			
			int nPages = doc.getNumPages();
			
			System.out.println("Pages=" + Integer.toString(nPages));
			
			WordFinderConfig wordConfig = new WordFinderConfig();
			wordConfig.setDisableTaggedPDF(true);
			wordConfig.setIgnoreCharGaps(true);
			
			WordFinder wordFinder = new WordFinder(doc, WordFinderVersion.LATEST, wordConfig);

            List<Word> pageWords = null;
            
            for (int i = 0; i < nPages; i++)
            {
                pageWords = wordFinder.getWordList(i);
                for ( Word w : pageWords )
                {
                	System.out.println(w.getText());
                	System.out.println(w.getQuads().toString());
                    System.out.println(w.getStyleTransitions());
                    System.out.println(w.getAttributes().toString());
                }
            }
			doc.close();
		}
		finally {
			lib.delete();
		}
	}
};
