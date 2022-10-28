package com.datalogics.pdfl.samples.Annotations.PolygonAnnotations;

import java.util.EnumSet;
import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.*;

/*
 * 
 * This program generates a PDF output file with a polygon shape (a triangle) as an annotation to the file.
 * The program defines the vertices for the outlines of the annotation, and the line and fill colors.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class PolygonAnnotations
{
	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable
	{
		System.out.println("PolygonAnnotation sample:");

		Library lib = new Library();
		System.out.println("Initialized the library.");

		try
		{
                        String sOutput = "PolygonAnnotations-out.pdf";
                        if ( args.length > 0 )
                            sOutput = args[0];
			// Create a new document and blank first page
			Document doc = new Document();
			Rect rect = new Rect(0, 0, 612, 792);
			Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, rect);
			System.out.println("Created new document and first page.");
				
			// Create a vector of polygon vertices
			List<com.datalogics.PDFL.Point> vertices = new ArrayList<com.datalogics.PDFL.Point>();
			com.datalogics.PDFL.Point p = new com.datalogics.PDFL.Point(100, 100);
			vertices.add(p);
			p = new com.datalogics.PDFL.Point(200, 300);
			vertices.add(p);
			p = new com.datalogics.PDFL.Point(400, 200);
			vertices.add(p);
			System.out.println("Created an array of vertex points.");

			// Create and add a new PolygonAnnotation to the 0th element of first page's annotation array
			PolygonAnnotation polygonAnnot = new PolygonAnnotation(page, rect, vertices, -1);
			System.out.println("Created new PolygonAnnotation as 0th element of annotation array.");

			// Now let's retrieve and display the vertices
			List<com.datalogics.PDFL.Point> vertices2;// = new ArrayList<com.datalogics.PDFL.Point>();
			vertices2 = polygonAnnot.getVertices();
			System.out.println("Retrieved the vertices of the polygon annotation.");
			System.out.println("They are:");
			for (int i = 0; i < vertices2.size(); i++)
			{
			    System.out.println("Vertex " + i + ": " + vertices2.get(i));
			}

			// Let's set some colors and then ask the polyline to generate an appearance stream
			com.datalogics.PDFL.Color color = new com.datalogics.PDFL.Color(0.5, 0.3, 0.8);
			polygonAnnot.setInteriorColor(color);
			color = new com.datalogics.PDFL.Color(0.9, 0.7, 0.1);
			polygonAnnot.setColor(color);
			System.out.println("Set the stroke and fill colors.");
			Form form = polygonAnnot.generateAppearance();
			polygonAnnot.setNormalAppearance(form);
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
