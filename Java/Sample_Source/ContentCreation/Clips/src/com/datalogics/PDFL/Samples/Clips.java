package com.datalogics.PDFL.Samples;


import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Clip;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Path;
import com.datalogics.PDFL.*;


/*
 * This sample demonstrates working with Clip objects. A clipping path is used to edit the borders of a graphics object.
 * 
 * For more detail see the description of the Clips sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/manipulating-graphics-and-separating-colors-for-images
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class Clips
{

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable
	{
		System.out.println("Clips sample:");

		Library lib = new Library();
		System.out.println("Initialized the library.");

		try
		{
                        String sOutput = "Clips-out.pdf";
                        if (args.length > 0 )
                            sOutput = args[0];

			// Create a new document and blank first page
			Document doc = new Document();
			Rect rect = new Rect(0, 0, 612, 792);
			Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, rect);
			System.out.println("Created new document and first page.");
			
			// Create a new path, set up its graphic state and PaintOp
			Path path = new Path();
			GraphicState gs = path.getGraphicState();
			Color color = new Color(0.0, 0.0, 0.0);
			gs.setFillColor(color);
			path.setGraphicState(gs);
			path.setPaintOp(EnumSet.of(PathPaintOpFlags.FILL));

			// Add a rectangle to the path
			Point point = new Point(100, 500);
			path.addRect(point, 300, 200);
			System.out.println("Created new path and added rectangle to it.");

			// Add a curve to the path too so we have something
			// interesting to look at after clipping
			Point linePoint1 = new Point(400, 450);
			Point linePoint2 = new Point(350, 300);
			path.addCurveV(linePoint1, linePoint2);
			System.out.println("Added curve to the path.");

			// Add the path to the page in the document
			Content content = page.getContent();
			content.addElement(path);
			System.out.println("Added path to page in document.");

			// Create a new path and add a rectangle to it
			Path clipPath = new Path();
			point = new Point(50, 300);
			clipPath.addRect(point, 300, 250);
			System.out.println("Created clipping path and added rectangle to it.");

			// Create a new clip and add the new path to it and then add
			// this new clip to the original path as its Clip property
			Clip clip = new Clip();
			clip.addElement(clipPath);
			path.setClip(clip);
			System.out.println("Created new clip, assigned clipping path to it, and added new clip to original path.");

			// Update the page's content and save the file with clipping
			page.updateContent();
			doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
			System.out.println("Saved to " + sOutput);

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
