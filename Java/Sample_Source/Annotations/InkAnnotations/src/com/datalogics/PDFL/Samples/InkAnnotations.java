package com.datalogics.PDFL.Samples;


import java.util.EnumSet;
import java.util.ArrayList;

import com.datalogics.PDFL.*;
import java.util.List;


/*
 * 
 * This sample creates and adds a new Ink annotation to a PDF document. An Ink annotation is a freeform line,
 * similar to what you would create with a pen, or with a stylus on a mobile device.
 *
 * For more detail see the description of the InkAnnotations sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-annotations-in-pdf-files#inkannotations
 *  
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class InkAnnotations
{

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable
	{
		System.out.println("InkAnnotations sample:");

		Library lib = new Library();
		System.out.println("Initialized the library.");

		try
		{
			// Create a new document and blank first page
			Document doc = new Document();
			Rect rect = new Rect(0, 0, 612, 792);
			Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, rect);
			System.out.println("Created new document and first page.");

			// Create and add a new InkAnnotation to the 0th element of first page's annotation array
			InkAnnotation inkAnnot = new InkAnnotation(page, rect, -1);
			System.out.println("Created new InkAnnotation as 0th element of annotation array.");

			// Ask how many scribbles are in the ink annotation
			System.out.println("Number of scribbles in ink annotation: " + inkAnnot.getNumScribbles());

			// Create a vector of scribble vertices
			ArrayList<com.datalogics.PDFL.Point> scribble = new ArrayList<com.datalogics.PDFL.Point>();
			com.datalogics.PDFL.Point p = new com.datalogics.PDFL.Point(100, 100);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(200, 300);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(400, 200);
			scribble.add(p);
			System.out.println("Created an array of scribble points.");

			// Add the scribble to the ink annotation
			inkAnnot.addScribble(scribble);
			System.out.println("Added the scribble to the ink annotation.");

			// Ask how many scribbles are in the ink annotation
			System.out.println("Number of scribbles in ink annotation: " + inkAnnot.getNumScribbles());

			// Create another vector of scribble vertices
			scribble = new ArrayList<com.datalogics.PDFL.Point>();
			p = new com.datalogics.PDFL.Point(200, 200);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(200, 300);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(300, 300);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(300, 200);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(200, 100);
			scribble.add(p);
			System.out.println("Created another array of scribble points.");

			// Add the scribble to the ink annotation
			inkAnnot.addScribble(scribble);
			System.out.println("Added the scribble to the ink annotation.");

			// Create another vector of scribble vertices
			scribble = new ArrayList<com.datalogics.PDFL.Point>();
			p = new com.datalogics.PDFL.Point(300, 400);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(200, 300);
			scribble.add(p);
			p = new com.datalogics.PDFL.Point(300, 300);
			scribble.add(p);
			System.out.println("Created another array of scribble points.");

			// Add the scribble to the ink annotation
			inkAnnot.addScribble(scribble);
			System.out.println("Added the scribble to the ink annotation.");

			// Ask how many scribbles are in the ink annotation
			System.out.println("Number of scribbles in ink annotation: " + inkAnnot.getNumScribbles());

			// Get and display the points in ink annotation 0
			List<com.datalogics.PDFL.Point> scribbleToGet = inkAnnot.getScribble(0);
			for (int i = 0; i < scribbleToGet.size(); i++)
				System.out.println("Scribble 0, point" + i + " :" + scribbleToGet.get(i));

			// Get and display the points in ink annotation 1
			scribbleToGet = inkAnnot.getScribble(1);
			for (int i = 0; i < scribbleToGet.size(); i++)
				System.out.println("Scribble 1, point" + i + " :" + scribbleToGet.get(i));

			// Let's set the color and then ask the ink annotation to generate an appearance stream
			com.datalogics.PDFL.Color color = new com.datalogics.PDFL.Color(0.5, 0.3, 0.8);
			inkAnnot.setColor(color);
			System.out.println("Set the stroke and fill colors.");
			Form form = inkAnnot.generateAppearance();
			inkAnnot.setNormalAppearance(form);
			System.out.println("Generated the appearance stream.");

			// Update the page's content and save the file with clipping
			page.updateContent();
			doc.save(EnumSet.of(SaveFlags.FULL), "InkAnnotations-out1.pdf");
			System.out.println("Saved InkAnnotations-out1.pdf");

			// Remove 0th scribble
			inkAnnot.removeScribble(0);
			// Ask how many scribbles are in the ink annotation
			System.out.println("Number of scribbles in ink annotation: " + inkAnnot.getNumScribbles());

			// Generate a new appearance stream, since we have removed the first scribble.
			inkAnnot.setNormalAppearance(inkAnnot.generateAppearance());

			// Update the page's content and save the file with clipping
			page.updateContent();
			doc.save(EnumSet.of(SaveFlags.FULL), "InkAnnotations-out2.pdf");
			System.out.println("Saved InkAnnotations-out2.pdf");

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
