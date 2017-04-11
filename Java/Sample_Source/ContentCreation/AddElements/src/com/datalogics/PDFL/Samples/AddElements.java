package com.datalogics.PDFL.Samples;

/*
 * The AddElements sample program creates a new PDF file with two pages and several graphics
 * and text elements. The first page features a pentagram drawing; the second shows how to
 * create colored text, text that is vertical or at an angle, and a shape with color fill.
 * The third page features a rectangle and a curved design. Use this sample for ideas on how
 * to draw an image on a PDF page.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import com.datalogics.PDFL.*;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.EnumSet;
public class AddElements
{

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("Add Elements sample:");

    	Library lib = new Library();

		try {
			System.out.println("Initialized the library.");
             String sOutput = "AddElements-out.pdf";
             if (args.length != 0)
                 sOutput = args[0];
             System.out.println("Output file: " + sOutput);
             Document doc = new Document();
             Rect pageRect = new Rect(0, 0, 612, 792);
             Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);

             // Draw a five pointed star.
             Path starpath = new Path();
             GraphicState gs = starpath.getGraphicState();

             starpath.setPaintOp(EnumSet.of(PathPaintOpFlags.STROKE));
             gs.setWidth(2.0); // make the line thickness 2 PDF coordinates.
             List<Double> DashPattern = Arrays.asList(3.0, 5.0, 6.0); // Set 3 for the Dash Phase and the Dash Pattern list to [5 6]
             gs.setDashPattern(DashPattern);
             gs.setStrokeColor(new Color(0, 1.0, 0));// Green Star
             starpath.setGraphicState(gs);
             double CenterX = 306.0; // Center of Page
             double CenterY = 396.0;
             double Radius = 72 * 4.0; // 4 inches with 72 dpi
             double radians72 = 72/(45/Math.atan(1.0)); // angles must be in radians.
             double radians36 = 36/(45/Math.atan(1.0));
             Point CenterPoint = new Point(CenterX, CenterY);
             Point Point0 = new Point(CenterX, CenterY+Radius);
             Point Point1 = new Point(CenterX+Radius*Math.sin(radians72), CenterY+Radius*Math.cos(radians72));
             Point Point2 = new Point(CenterX + Radius*Math.sin(radians36), CenterY - Radius*Math.cos(radians36));
             Point Point3 = new Point(CenterX - Radius*Math.sin(radians36),CenterY - Radius*Math.cos(radians36));
             Point Point4 = new Point (CenterX - Radius * Math.sin(radians72), CenterY + Radius * Math.cos(radians72));
             starpath.moveTo(Point0);
             starpath.addLine(Point2);
             starpath.addLine(Point4);
             starpath.addLine(Point1);
             starpath.addLine(Point3);
             starpath.addLine(Point0);
             starpath.closePath();
             docpage.getContent().addElement(starpath);  // Add the new element to the Content of the page.
            
            // Draw a pentagon around the star
            Path pentpath = new Path();
            pentpath.setPaintOp(EnumSet.of(PathPaintOpFlags.STROKE));
            gs.setWidth(2.0);
            
            List<Double> PentDashPattern = Arrays.asList(0.0, 3.0); // Set 0 for the Dash Phase and the Dash Pattern list to [3]
            gs.setDashPattern(PentDashPattern);
            gs.setStrokeColor(new Color(1.0, 0, 1.0)); // purple pentagon
            pentpath.setGraphicState(gs);
            pentpath.moveTo(Point0);
            pentpath.addLine(Point1);
            pentpath.addLine(Point2);
            pentpath.addLine(Point3);
            pentpath.addLine(Point4);
            pentpath.addLine(Point0);
            pentpath.closePath();
            docpage.getContent().addElement(pentpath); // Add the new element to the Content of the page.
            
            // Add a single line star in the middle of the big star
            Path newstar = new Path();
            newstar.setPaintOp(EnumSet.of(PathPaintOpFlags.EO_FILL));
            gs.setWidth(1.0);
            ArrayList<Double> starDashPattern = new ArrayList<Double>();
            gs.setDashPattern(starDashPattern);
            gs.setStrokeColor(new Color(0, 0, 1.0)); // blue innerstar
            newstar.setGraphicState(gs);
            newstar.moveTo(CenterPoint);
            newstar.addLine(Point0);
            newstar.moveTo(CenterPoint);
            newstar.addLine(Point1);
            newstar.moveTo(CenterPoint);
            newstar.addLine(Point2);
            newstar.moveTo(CenterPoint);
            newstar.addLine(Point3);
            newstar.moveTo(CenterPoint);
            newstar.addLine(Point4);
            newstar.closePath();
            docpage.getContent().addElement(newstar);
            docpage.updateContent();
            
            /* Second page: a diamond with text inside */
            docpage = doc.createPage(0, pageRect);
            
            Path diamond = new Path();
            diamond.setPaintOp(EnumSet.of(PathPaintOpFlags.EO_FILL, PathPaintOpFlags.STROKE));
            gs.setStrokeColor(new Color(153.0/255.0, 0, 0)); // deep red
            gs.setFillColor(new Color(1.0, 1.0, 0)); // Yellow
            
            ArrayList<Double> rectDashPattern = new ArrayList<Double>();
            gs.setDashPattern(rectDashPattern);
            
            gs.setWidth(1.0);
            gs.setLineJoin(LineJoin.BEVEL_JOIN);
            diamond.setGraphicState(gs);

            diamond.moveTo(new Point(306, 198));
            diamond.addLine( new Point(459, 396));
            diamond.addLine(new Point(306, 594));
            diamond.addLine( new Point(153, 396));
            diamond.addLine(new Point(306, 198));
            diamond.closePath();
            docpage.getContent().addElement(diamond); // Add the new element to the Content of the page.

// Now add text to the PDF page. By default, text is filled with
// the fill color from the graphic state
            Text t = new Text();
            Font f = new Font("Arial", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));
            gs.setFillColor( new Color(0, 0, 1.0));
            TextState ts = new TextState();

            Matrix m = new Matrix().translate(180, 414).scale(24.0, 24.0);
            TextRun tr = new TextRun("Horizontal Blue Text", f, gs, ts, m);
            t.addRun(tr);

            // This text will be filled and stroked
            m = new Matrix().translate(315, 216).scale(24.0, 24.0).rotate(90);
            gs.setFillColor(new Color(1.0, 0, 0));
            gs.setStrokeColor(new Color(0.0, 0.5, 0.5));
            ts.setRenderMode(TextRenderMode.FILL_THEN_STROKE);
            tr = new TextRun("Vertical Red Text", f, gs, ts, m);
            t.addRun(tr);

            // This text will only be stroked
            m = new Matrix().translate(297, 576).scale(24.0, 24.0).rotate(-52);
            gs.setFillColor(new Color(0, 1.0, 0));
            ts.setRenderMode(TextRenderMode.STROKE);
            tr = new TextRun("Angled Green Text", f, gs, ts, m);
            t.addRun(tr);
            docpage.getContent().addElement(t);
            docpage.updateContent();

            /* Third page: a stroked path that uses all the segment types */
            docpage = doc.createPage(1, pageRect);
            Path path = new Path();

            path.setPaintOp(EnumSet.of(PathPaintOpFlags.STROKE));
            gs = path.getGraphicState();
            gs.setWidth(2);
            path.setGraphicState(gs);

            ArrayList<Segment> segments = new ArrayList<Segment>();

            segments.add(new MoveTo(72.0, 73.0));
            segments.add(new LineTo(76.0, 144.0));
            segments.add(new CurveTo(120.0, 144.0, 121.0, 96.0, 97.0, 98.0));
            segments.add(new CurveToV(80.0, 81.0, 128.0, 21.0));
            segments.add(new CurveToY(200.0, 201.0, 22.0, 160.0));
            segments.add(new ClosePath());
            segments.add(new RectSegment(256.0, 257.0, 123.0, 67.0));

            path.setSegments(segments);

            docpage.getContent().addElement(path);
            docpage.updateContent();


            doc.embedFonts(EnumSet.of(EmbedFlags.NONE), new AEProgressMonitor(), 
                    new AECancelProc(), new AEReportProc());
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput,
                    new AEProgressMonitor(), new AECancelProc());

		}
		finally {
			lib.delete();
		}
	}

}
