using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * The AddElements sample program creates a new PDF file with two pages and several graphics
 * and text elements. The first page features a pentagram drawing; the second shows how to
 * create colored text, text that is vertical or at an angle, and a shape with color fill.
 * The third page features a rectangle and a curved design. Use this sample for ideas on how
 * to draw an image on a PDF page.
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace AddElements
{
    class AddElements
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AddElements Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "AddElements-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Output file: " + sOutput);

                Document doc = new Document();
                Rect pageRect = new Rect(0, 0, 612, 792);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);

                // Draw a five pointed star.
                Datalogics.PDFL.Path starpath = new Datalogics.PDFL.Path();
                GraphicState gs = starpath.GraphicState;

                starpath.PaintOp = PathPaintOpFlags.Stroke;
                gs.Width = 2.0; // make the line thickness 2 PDF coordinates.
                List<double> DashPattern = new List<double>();
                DashPattern.Add(3); // Set 3 for the Dash Phase
                DashPattern.Add(5);
                DashPattern.Add(6); // Set the Dash Pattern list to [5 6]
                gs.DashPattern = DashPattern;
                gs.StrokeColor = new Color(0, 1.0, 0); // Green Star
                starpath.GraphicState = gs;
                starpath.PaintOp = PathPaintOpFlags.Stroke;
                double CenterX = 306.0; // Center of Page
                double CenterY = 396.0;
                double Radius = 72 * 4.0; // 4 inches with 72 dpi
                double radians72 = 72 / (45 / Math.Atan(1.0)); // angles must be in radians.
                double radians36 = 36 / (45 / Math.Atan(1.0));
                Point CenterPoint = new Point(CenterX, CenterY);
                Point Point0 = new Point(CenterX, CenterY + Radius);
                Point Point1 = new Point(CenterX + Radius * Math.Sin(radians72),
                    CenterY + Radius * Math.Cos(radians72));
                Point Point2 = new Point(CenterX + Radius * Math.Sin(radians36),
                    CenterY - Radius * Math.Cos(radians36));
                Point Point3 = new Point(CenterX - Radius * Math.Sin(radians36),
                    CenterY - Radius * Math.Cos(radians36));
                Point Point4 = new Point(CenterX - Radius * Math.Sin(radians72),
                    CenterY + Radius * Math.Cos(radians72));
                starpath.MoveTo(Point0);
                starpath.AddLine(Point2);
                starpath.AddLine(Point4);
                starpath.AddLine(Point1);
                starpath.AddLine(Point3);
                starpath.AddLine(Point0);
                starpath.ClosePath();
                docpage.Content.AddElement(starpath); // Add the new element to the Content of the page.

                // Draw a pentagon around the star
                Datalogics.PDFL.Path pentpath = new Datalogics.PDFL.Path();
                pentpath.PaintOp = PathPaintOpFlags.Stroke;
                gs.Width = 2.0;
                List<double> PentDashPattern = new List<double>();
                PentDashPattern.Add(0); // Set 0 for the Dash Phase
                PentDashPattern.Add(3); // Set the Dash Pattern list to [3]
                gs.DashPattern = PentDashPattern;
                gs.StrokeColor = new Color(1.0, 0.0, 1.0); // purple pentagon
                pentpath.GraphicState = gs;
                pentpath.PaintOp = PathPaintOpFlags.Stroke;
                pentpath.MoveTo(Point0);
                pentpath.AddLine(Point1);
                pentpath.AddLine(Point2);
                pentpath.AddLine(Point3);
                pentpath.AddLine(Point4);
                pentpath.AddLine(Point0);
                pentpath.ClosePath();
                docpage.Content.AddElement(pentpath); // Add the new element to the Content of the page.

                // Add a single line star in the middle of the big star
                Datalogics.PDFL.Path newstar = new Datalogics.PDFL.Path();
                newstar.PaintOp = PathPaintOpFlags.EoFill;
                gs.Width = 1.0;
                List<double> starDashPattern = new List<double>();
                gs.DashPattern = starDashPattern;
                gs.StrokeColor = new Color(0, 0, 1.0); // blue innerstar
                newstar.GraphicState = gs;
                newstar.MoveTo(CenterPoint);
                newstar.AddLine(Point0);
                newstar.MoveTo(CenterPoint);
                newstar.AddLine(Point1);
                newstar.MoveTo(CenterPoint);
                newstar.AddLine(Point2);
                newstar.MoveTo(CenterPoint);
                newstar.AddLine(Point3);
                newstar.MoveTo(CenterPoint);
                newstar.AddLine(Point4);
                newstar.ClosePath();
                docpage.Content.AddElement(newstar);

                docpage.UpdateContent(); // Update the PDF page with the changed content

/* Second page: a diamond with text inside */
                docpage = doc.CreatePage(0, pageRect);

                Datalogics.PDFL.Path diamond = new Datalogics.PDFL.Path();
                List<double> diamondDashPattern = new List<double>();
                gs.DashPattern = diamondDashPattern;
                gs.FillColor = new Color(1.0, 1.0, 0); // Yellow
                gs.StrokeColor = new Color(153.0 / 255.0, 0, 0); // kind of a deep red
                diamond.PaintOp = PathPaintOpFlags.EoFill | PathPaintOpFlags.Stroke;

                gs.Width = 1.0;
                gs.LineJoin = LineJoin.BevelJoin;
                diamond.GraphicState = gs;

                diamond.MoveTo(new Point(306, 198));
                diamond.AddLine(new Point(459, 396));
                diamond.AddLine(new Point(306, 594));
                diamond.AddLine(new Point(153, 396));
                diamond.AddLine(new Point(306, 198));
                diamond.ClosePath();
                docpage.Content.AddElement(diamond); // Add the new element to the Content of the page.

// Now add text to the PDF page. By default, text is filled with
// the fill color from the graphic state
                Text t = new Text();
                Font f;
                try
                {
                    f = new Font("Arial", FontCreateFlags.Embedded | FontCreateFlags.Subset);
                }
                catch (ApplicationException ex)
                {
                    if (ex.Message.Equals("The specified font could not be found.") &&
                        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                            .OSPlatform.Linux) &&
                        !System.IO.Directory.Exists("/usr/share/fonts/msttcore/"))
                    {
                        Console.WriteLine("Please install Microsoft Core Fonts on Linux first.");
                        return;
                    }

                    throw;
                }

                gs = new GraphicState();
                gs.FillColor = new Color(0, 0, 1.0);
                TextState ts = new TextState();

                Matrix m = new Matrix().Translate(180, 414).Scale(24.0, 24.0);
                TextRun tr = new TextRun("Horizontal Blue Text", f, gs, ts, m);
                t.AddRun(tr);

                // This text will be filled and stroked
                m = new Matrix().Translate(315, 216).Scale(24.0, 24.0).Rotate(90);
                gs.FillColor = new Color(1.0, 0, 0);
                gs.StrokeColor = new Color(0.0, 0.5, 0.5);
                ts.RenderMode = TextRenderMode.FillThenStroke;
                tr = new TextRun("Vertical Red Text", f, gs, ts, m);
                t.AddRun(tr);

                // This text will only be stroked
                m = new Matrix().Translate(297, 576).Scale(24.0, 24.0).Rotate(-52);
                gs.FillColor = new Color(0, 1.0, 0);
                ts.RenderMode = TextRenderMode.Stroke;
                tr = new TextRun("Angled Green Text", f, gs, ts, m);
                t.AddRun(tr);

                docpage.Content.AddElement(t);
                docpage.UpdateContent();

                /* Third page: a stroked path that uses all the segment types */
                docpage = doc.CreatePage(1, pageRect);
                Datalogics.PDFL.Path path = new Datalogics.PDFL.Path();

                path.PaintOp = PathPaintOpFlags.Stroke;
                gs = path.GraphicState;
                gs.Width = 2;
                path.GraphicState = gs;

                List<Segment> segments = new List<Segment>();

                segments.Add(new MoveTo(72.0, 73.0));
                segments.Add(new LineTo(76.0, 144.0));
                segments.Add(new CurveTo(120.0, 144.0, 121.0, 96.0, 97.0, 98.0));
                segments.Add(new CurveToV(80.0, 81.0, 128.0, 21.0));
                segments.Add(new CurveToY(200.0, 201.0, 22.0, 160.0));
                segments.Add(new ClosePath());
                segments.Add(new RectSegment(256.0, 257.0, 123.0, 67.0));

                path.Segments = segments;

                docpage.Content.AddElement(path);
                docpage.UpdateContent();

                doc.EmbedFonts(EmbedFlags.None, new AEProgressMonitor(), new AECancelProc(), new AEReportProc());
                doc.Save(SaveFlags.Full, sOutput, new AEProgressMonitor(), new AECancelProc());
            }
        }
    }
}
