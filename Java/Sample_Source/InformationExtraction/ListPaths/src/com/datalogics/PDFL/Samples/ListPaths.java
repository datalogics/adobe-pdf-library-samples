package com.datalogics.PDFL.Samples;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.List;

import com.datalogics.PDFL.ClosePath;
import com.datalogics.PDFL.Container;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.CurveTo;
import com.datalogics.PDFL.CurveToV;
import com.datalogics.PDFL.CurveToY;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Element;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.Group;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LineTo;
import com.datalogics.PDFL.MoveTo;
import com.datalogics.PDFL.Path;
import com.datalogics.PDFL.RectSegment;
import com.datalogics.PDFL.Segment;

/*
 * 
 * This sample searches for and lists the contents of paths found in an existing PDF document.
 * Paths in PDF documents, or clipping paths, define the boundaries for art or graphics.
 * 
 * For more detail see the description of the List sample programs, and ListPaths, on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 * 
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class ListPaths {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		System.out.println("List Paths sample:");

		Library lib = new Library();
		try {
			System.out.println("Initialized the library.");
			String filename = "../../Resources/Sample_Input/sample.pdf";
			if (args.length > 0) {
				filename = args[0];
                        }

			Document doc = new Document(filename);
                        System.out.println("Displaying paths in " + filename);

			try {
				listPathsInDocument(doc);
			} finally {
				doc.delete();
			}
		} finally {
			lib.delete();
		}
	}

	private static void listPathsInDocument(Document doc) {
		for (int pgno = 0; pgno < doc.getNumPages(); pgno++) {
			listPathsInContent(doc.getPage(pgno).getContent(), pgno);
		}
	}

	private static void listPathsInContent(Content content, int pgno) {
		for (int i = 0; i < content.getNumElements(); i++) {
			Element e = content.getElement(i);

			if (e instanceof Path) {
				listPath((Path) e, pgno);
			} else if (e instanceof Container) {
				System.out.println("Recurring through a Container");
				listPathsInContent(((Container) e).getContent(), pgno);
			} else if (e instanceof Group) {
				System.out.println("Recurring through a Group");
				listPathsInContent(((Group) e).getContent(), pgno);
			} else if (e instanceof Form) {
				System.out.println("Recurring through a Form");
				listPathsInContent(((Form) e).getContent(), pgno);
			}

		}
	}

	private static void listPath(Path path, int pgno) {
		List<Segment> segments = path.getSegments();
		System.out.format("Path on page %d:\n", pgno);
		System.out.format("Transformation matrix: %s\n", path.getMatrix());
		for (Segment segment : segments) {
			if (segment instanceof MoveTo) {
				MoveTo moveto = (MoveTo) segment;
				System.out.format("  MoveTo x=%f, y=%f\n", moveto.getPoint()
						.getH(), moveto.getPoint().getV());
			} else if (segment instanceof LineTo) {
				LineTo lineto = (LineTo) segment;
				System.out.format("  LineTo x=%f, y=%f\n", lineto.getPoint()
						.getH(), lineto.getPoint().getV());
			} else if (segment instanceof CurveTo) {
				CurveTo curveto = (CurveTo) segment;
				System.out.format(
						"  CurveTo x1=%f, y1=%f, x2=%f, y2=%f, x3=%f, y3=%f\n",
						curveto.getPoint1().getH(), curveto.getPoint1().getV(),
						curveto.getPoint2().getH(), curveto.getPoint2().getV(),
						curveto.getPoint3().getH(), curveto.getPoint3().getV());
			} else if (segment instanceof CurveToV) {
				CurveToV curveto = (CurveToV) segment;
				System.out.format("  CurveToV x2=%f, y2=%f, x3=%f, y3=%f\n",
						curveto.getPoint2().getH(), curveto.getPoint2().getV(),
						curveto.getPoint3().getH(), curveto.getPoint3().getV());
			} else if (segment instanceof CurveToY) {
				CurveToY curveto = (CurveToY) segment;
				System.out.format("  CurveToV x1=%f, y1=%f, x3=%f, y3=%f\n",
						curveto.getPoint1().getH(), curveto.getPoint1().getV(),
						curveto.getPoint3().getH(), curveto.getPoint3().getV());
			} else if (segment instanceof RectSegment) {
				RectSegment rect = (RectSegment) segment;
				System.out.format(
						"  Rectangle x=%f, y=%f, width=%f, height=%f\n", rect
								.getPoint().getH(), rect.getPoint().getV(),
						rect.getWidth(), rect.getHeight());
			} else if (segment instanceof ClosePath) {
				System.out.format("  ClosePath\n");
			}
		}
	}

}
