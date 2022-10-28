package com.datalogics.pdfl.samples.InformationExtraction.ListBookmarks;
/*
* 
 * This sample finds and describes the bookmarks included in a PDF document.
 * 
 * For more detail see the description of the List sample programs, and ListBookmarks, on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import java.io.BufferedReader;
import java.io.InputStreamReader;

import com.datalogics.PDFL.Bookmark;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.ViewDestination;

public class ListBookmarks {

	static void EnumerateBookmarks(Bookmark b)
	{
		if (b != null)
		{
			System.out.print(b);
			System.out.print(": ");
			System.out.print(b.getTitle());
			ViewDestination v = b.getViewDestination();
			if (v != null){
				System.out.print(", page ");
				System.out.print(v.getPageNumber());
				System.out.print(", fit ");
				System.out.print(v.getFitType());
				System.out.print(", dest rect ");
				System.out.print(v.getDestRect());
				System.out.print(", zoom ");
				System.out.print(v.getZoom());
			}
			System.out.println();
			EnumerateBookmarks(b.getFirstChild());
			EnumerateBookmarks(b.getNext());
		}
	}
	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("ListBookmarks sample:");

    	Library lib = new Library();

		try {
	        String filename = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
	        if (args.length > 0) {
	            filename = args[0];
                }
	        
	        Document doc = new Document(filename);

	        Bookmark rootBookmark = doc.getBookmarkRoot();
	        EnumerateBookmarks(rootBookmark);
		}
		finally {
			lib.delete();
		}
	}

}
