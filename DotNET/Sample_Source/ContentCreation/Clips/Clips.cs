using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates working with Clip objects. A clipping path is used to edit the borders of a graphics object.
 *
 * 
 * For more detail see the description of the Clips sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/manipulating-graphics-and-separating-colors-for-images
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace Clips
{
    class Clips
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Clips Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "Clips-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Output file: " + sOutput);

                // Create a new document and blank first page
                Document doc = new Document();
                Rect rect = new Rect(0, 0, 612, 792);
                Page page = doc.CreatePage(Document.BeforeFirstPage, rect);
                Console.WriteLine("Created new document and first page.");

                // Create a new path, set up its graphic state and PaintOp
                Datalogics.PDFL.Path path = new Datalogics.PDFL.Path();
                GraphicState gs = new GraphicState();
                Color color = new Color(0.0, 0.0, 0.0);
                gs.FillColor = color;
                path.GraphicState = gs;
                path.PaintOp = PathPaintOpFlags.Fill;

                // Add a rectangle to the path
                Point point = new Point(100, 500);
                path.AddRect(point, 300, 200);
                Console.WriteLine("Created new path and added rectangle to it.");

                // Add a curve to the path too so we have something
                // interesting to look at after clipping
                Point linePoint1 = new Point(400, 450);
                Point linePoint2 = new Point(350, 300);
                path.AddCurveV(linePoint1, linePoint2);
                Console.WriteLine("Added curve to the path.");

                // Add the path to the page in the document
                Content content = page.Content;
                content.AddElement(path);
                Console.WriteLine("Added path to page in document.");

                // Create a new path and add a rectangle to it
                Datalogics.PDFL.Path clipPath = new Datalogics.PDFL.Path();
                point = new Point(50, 300);
                clipPath.AddRect(point, 300, 250);
                Console.WriteLine("Created clipping path and added rectangle to it.");

                // Create a new clip and add the new path to it and then add
                // this new clip to the original path as its Clip property
                Clip clip = new Clip();
                clip.AddElement(clipPath);
                path.Clip = clip;
                Console.WriteLine(
                    "Created new clip, assigned clipping path to it, and added new clip to original path.");

                // Update the page's content and save the file with clipping
                page.UpdateContent();
                doc.Save(SaveFlags.Full, sOutput);

                // Dispose the doc object
                doc.Dispose();
                Console.WriteLine("Disposed document object.");
            }
        }
    }
}
