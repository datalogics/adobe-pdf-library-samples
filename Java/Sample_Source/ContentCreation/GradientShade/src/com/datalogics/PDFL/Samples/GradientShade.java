package com.datalogics.pdfl.samples.ContentCreation.GradientShade;

import java.util.EnumSet;
import java.util.List;
import java.util.Arrays;
import com.datalogics.PDFL.*;

/*
 *
 * This sample demonstrates changing the shading of an image on a PDF document page. The image gradually
 * changes from black on the left side of the image, to red on the right side.
 * 
 * For more detail see the description of the GradientShade sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/entering-or-generating-graphics-from-pdf-files
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class GradientShade {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		System.out.println("GradientShade Sample:");
		Library lib = new Library();
                String sOutput = "GradientShade-out.pdf";
                if (args.length > 0)
                    sOutput = args[0];
                System.out.println("Will write new file " + sOutput);

		try {
			System.out.println("Initialized the library.");
			Document doc = new Document();
			Rect pageRect = new Rect(0, 0, 792, 612);
			Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);

			List<Double> domain = Arrays.asList(0.0, 1.0);
			List<Double> C0 = Arrays.asList(0.0, 0.0, 0.0);
			List<Double> C1 = Arrays.asList(1.0, 0.0, 0.0);
			Function f = new ExponentialFunction(domain, 3, C0, C1, 1);

			List<Point> coords = Arrays.asList(new Point(72, 72),
                new Point(4 * 72, 72));

			List<Function> functionList = Arrays.asList(f);
			AxialShadingPattern asp = new AxialShadingPattern(
					ColorSpace.DEVICE_RGB, coords, functionList);

			Path path = new Path();
			GraphicState gs = path.getGraphicState();

			gs.setFillColor(new Color(asp));
			path.setGraphicState(gs);

			path.moveTo(new Point(36, 36));
			path.addLine(new Point(36, 8 * 72 - 36));
			path.addLine(new Point(11 * 72 - 36, 8 * 72 - 36));
			path.addLine(new Point(11 * 72 - 36, 36));
			path.closePath();
			path.setPaintOp(EnumSet.of(PathPaintOpFlags.STROKE,
					PathPaintOpFlags.FILL));

			docpage.getContent().addElement(path); // Add the new element to the Content of the page.
			docpage.updateContent(); // Update the PDF page with the changed content

			doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
		} finally {
			lib.delete();
		}
	}

}
