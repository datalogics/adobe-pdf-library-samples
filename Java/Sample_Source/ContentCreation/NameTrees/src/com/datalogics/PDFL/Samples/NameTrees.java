package com.datalogics.PDFL.Samples;


//import com.datalogics.PDFL.Document;
//import com.datalogics.PDFL.Library;
//import com.datalogics.PDFL.Page;
//import com.datalogics.PDFL.Rect;
//import com.datalogics.PDFL.SaveFlags;
//import com.datalogics.PDFL.Clip;
//import com.datalogics.PDFL.Content;
//import com.datalogics.PDFL.Point;
//import com.datalogics.PDFL.Path;
import com.datalogics.PDFL.*;


/*
 *  
 * This sample demonstrates working with the NameTree class. A name tree is a type
 * of dictionary that is often used as a data structure in PDF files. Unlike a standard
 * dictionary, instead of using a code value as a key to identify an object, a name tree
 * uses names as keys to map to data objects. 
 *
 * For more detail see the description of the NameTrees sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-large-amounts-of-data-in-a-pdf
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class NameTrees
{
	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable
	{
		System.out.println("NameTrees sample:");

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
			NameTree nametree = new NameTree(doc);

			// Use put() to put a key-value pair in it
			PDFString key = new PDFString("Bailout", doc, false, false);;
			PDFString value = new PDFString("Smorgasbord", doc, false, false);
			nametree.put(key, value);
			System.out.println("\nCreated NameTree and added first key-value pair.");

			// Put another key-value pair in it
			nametree.put(new PDFString("Brandish", doc, false, false), new PDFString("Copasetic", doc, false, false));

			// Retrieve second entry
			PDFString keysearchstring = new PDFString("Brandish", doc, false, false);
			PDFObject lookup = nametree.get(keysearchstring);
			System.out.println("\nRetrieving two entries:");
			System.out.println(lookup);

			// Retrieve first entry
			lookup = nametree.get(new PDFString("Bailout", doc, false, false));
			System.out.println(lookup);

			// Use remove() method to remove first entry
			nametree.remove(new PDFString("Bailout", doc, false, false));

			// Get both entries, and demonstrate that first is now gone
			System.out.println("\nAfter removing entry 1, we now have:");
			lookup = nametree.get(new PDFString("Bailout", doc, false, false));
			System.out.println(lookup);
			lookup = nametree.get(new PDFString("Brandish", doc, false, false));
			System.out.println(lookup);

			// Create two new NameTrees and set each to our original numbertree
			NameTree abc = nametree;
			NameTree xyz = nametree;

			// Show they are equal
			if (abc.equals(xyz))
				System.out.println("\nThe two NameTrees abc and xyz are the same");
			else
				System.out.println("\nThe two NameTrees abc and xyz are not the same");

			// Create two new NameTrees
			abc = new NameTree(doc);
			xyz = new NameTree(doc);

			// Show they are not equal
			if (abc.equals(xyz))
				System.out.println("\nThe two NameTrees abc and xyz are the same");
			else
				System.out.println("\nThe two NameTrees abc and xyz are not the same");

			// Get the PDFDict from the NameTree
			PDFDict dict = nametree.getPDFDict();
			System.out.println("\nThe PDFDict from the NameTree:");
			System.out.println(dict);

			// Kill the NameTree object
			nametree.delete();
			System.out.println("\nKilled the NameTree object.");

			//////////////////////////////////////////////////
			// Now use Document methods to operate on NameTree
			//////////////////////////////////////////////////
			
			// Create a NameTree in the document using createNameTree() method
			NameTree docCreatedNameTree = doc.createNameTree("MyNameTree");
			docCreatedNameTree.put(new PDFString("Argyle", doc, false, false), new PDFString("Seamstress", doc, false, false));
			System.out.println("Created a NameTree object in the document.");

			// Look for the NameTree in the document by using the getNameTree() method
			System.out.println("\nTwo searches for NameTree using getNameTree() method; first fails, second succeeeds:");
			docCreatedNameTree = doc.getNameTree("Garbage");
			System.out.println(docCreatedNameTree);
			docCreatedNameTree = doc.getNameTree("MyNameTree");
			System.out.println(docCreatedNameTree);

			// Remove the NameTree from the document by using the remove NameTree() method
			System.out.println("\nRemove the NameTree from the document.");
			doc.removeNameTree("Garbage");
			docCreatedNameTree = doc.getNameTree("MyNameTree");
			System.out.println(docCreatedNameTree);
			doc.removeNameTree("MyNameTree");
			docCreatedNameTree = doc.getNameTree("MyNameTree");
			System.out.println(docCreatedNameTree);

			// Kill the doc object
			doc.delete();
			System.out.println("\nKilled document object.");
		}
		finally
		{
			lib.delete();
		}
	}

}
