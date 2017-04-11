package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.*;

/*
 * 
 * This sample demonstrates working with the NumberTree class. A number tree is a type of dictionary
 * that is often used as a data structure in PDF files. It is similar to a name tree, except that the
 * keys used in a number tree are integers, rather than character strings, and these keys are sorted
 * in ascending numerical order. 
 *
 * For more detail see the description of the NumberTrees sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-large-amounts-of-data-in-a-pdf
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class NumberTrees
{
	public static void main(String[] args) throws Throwable
	{
		System.out.println("NumberTrees sample:");

		Library lib = new Library();
		System.out.println("Initialized the library.");

		try
		{
			// Create a new document and blank first page
			Document doc = new Document();
			Rect rect = new Rect(0, 0, 612, 792);
			doc.createPage(Document.BEFORE_FIRST_PAGE, rect);
			System.out.println("Created new document and first page.");

			// Create a NumberTree and put a key-value pair in it
			NumberTree numbertree = new NumberTree(doc);
			
			// Use put() to put a key-value pair in it
			int key = 1;
			PDFString value = new PDFString("Smorgasbord", doc, false, false);
			numbertree.put(key, value);
			System.out.println("\nCreated NumberTree and added first key-value pair.");

			// Put another key-value pair in it
			numbertree.put(2, new PDFString("Copasetic", doc, false, false));

			// Retrieve second entry
			int keysearchinteger = 2;
			PDFObject lookup = numbertree.get(keysearchinteger);
			System.out.println("\nRetrieving two entries:");
			System.out.println(lookup);

			// Retrieve first entry
			lookup = numbertree.get(1);
			System.out.println(lookup);
			
			// Use remove() method to remove first entry
			numbertree.remove(1);

			// Get 1 and 2, and demonstrate that 1 is now gone
			System.out.println("\nAfter removing entry 1, we now have:");
			lookup = numbertree.get(1);
			System.out.println(lookup);
			lookup = numbertree.get(2);
			System.out.println(lookup);

			// Create two new NumberTrees and set each to our original numbertree
			NumberTree abc = numbertree;
			NumberTree xyz = numbertree;

			// Show they are equal
			if (abc.equals(xyz))
				System.out.println("\nThe two NumberTrees abc and xyz are the same");
			else
				System.out.println("\nThe two NumberTrees abc and xyz are not the same");

			// Create two new NumberTrees
			abc = new NumberTree(doc);
			xyz = new NumberTree(doc);
			
			// Show they are not equal
			if (abc.equals(xyz))
				System.out.println("\nThe two NumberTrees abc and xyz are the same");
			else
				System.out.println("\nThe two NumberTrees abc and xyz are not the same");

			// Get the PDFDict from the NumberTree
			PDFDict dict = numbertree.getPDFDict();
			System.out.println("\nThe PDFDict from the NumberTree:"); 
			System.out.println(dict);
			
			// Kill the NumberTree object
			numbertree.delete();
			System.out.println("\nKilled the NumberTree object.");
			
			// Kill the doc object
			doc.delete();
			System.out.println("Killed document object.");
		}
		finally
		{
			lib.delete();
		}
	}

}
