package com.datalogics.pdfl.samples.Annotations.PolyLineAnnotations;

import java.util.EnumSet;
import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.*;

/*
 *
 * This program generates a PDF output file with a line shape (an angle) as an annotation to the file.
 * The program defines the vertices for the outlines of the annotation, and the line and fill colors.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class PolyLineAnnotations
{
	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable
	{
		System.out.println("PolyLineAnnotation sample:");

		Library lib = new Library();
		System.out.println("Initialized the library.");

		try
		{
                        String sOutput = "PolyLineAnnotations-out.pdf";
			// Create a new document and blank first page
			Document doc = new Document();
			Rect rect = new Rect(0, 0, 612, 792);
			Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, rect);
			System.out.println("Created new document and first page.");

			// Create a vector of polyline vertices
			ArrayList<com.datalogics.PDFL.Point> vertices = new ArrayList<com.datalogics.PDFL.Point>();
			com.datalogics.PDFL.Point p = new com.datalogics.PDFL.Point(100, 100);
			vertices.add(p);
			p = new com.datalogics.PDFL.Point(200, 300);
			vertices.add(p);
			p = new com.datalogics.PDFL.Point(400, 200);
			vertices.add(p);
			System.out.println("Created an array of vertex points.");

			// Create and add a new PolyLineAnnotation to the 0th element of first page's annotation array
			PolyLineAnnotation polyLineAnnot = new PolyLineAnnotation(page, rect, vertices, -1);
			System.out.println("Created new PolyLineAnnotation as 0th element of annotation array.");

			// Now let's retrieve and display the vertices
			List<com.datalogics.PDFL.Point> vertices2;// = new ArrayList<com.datalogics.PDFL.Point>();
			vertices2 = polyLineAnnot.getVertices();
			System.out.println("Retrieved the vertices of the polyline annotation.");
			System.out.println("They are:");
			for (int i = 0; i < vertices2.size(); i++)
			{
				System.out.println("Vertex " + i + ": " + vertices2.get(i));
			}

			// Add some line ending styles to the ends of our PolyLine
			polyLineAnnot.setStartPointEndingStyle(LineEndingStyle.CLOSED_ARROW);
			polyLineAnnot.setEndPointEndingStyle(LineEndingStyle.OPEN_ARROW);

			// Let's set some colors and then ask the polyline to generate an appearance stream
			com.datalogics.PDFL.Color color = new com.datalogics.PDFL.Color(0.5, 0.3, 0.8);
			polyLineAnnot.setInteriorColor(color);
			color = new com.datalogics.PDFL.Color(0.0, 0.8, 0.1);
			polyLineAnnot.setColor(color);
			System.out.println("Set the stroke and fill colors.");
			Form form = polyLineAnnot.generateAppearance();
			polyLineAnnot.setNormalAppearance(form);
			System.out.println("Generated the appearance stream.");

			// Update the page's content and save the file with clipping
			page.updateContent();
			doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
			System.out.println("Saved " + sOutput);

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
