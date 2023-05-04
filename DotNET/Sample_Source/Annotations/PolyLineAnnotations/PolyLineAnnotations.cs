using System;
using System.Collections.Generic;
using Datalogics.PDFL;


/*
 *
 * This program generates a PDF output file with a line shape (an angle) as an annotation to the file.
 * The program defines the vertices for the outlines of the annotation, and the line and fill colors.
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PolyLineAnnotations
{
    class PolyLineAnnotations
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PolyLineAnnotation Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "PolyLineAnnotations-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Writing to output " + sOutput);

                // Create a new document and blank first page
                Document doc = new Document();
                Rect rect = new Rect(0, 0, 612, 792);
                Page page = doc.CreatePage(Document.BeforeFirstPage, rect);
                Console.WriteLine("Created new document and first page.");

                // Create a vector of polyline vertices
                List<Point> vertices = new List<Point>();
                Point p = new Point(100, 100);
                vertices.Add(p);
                p = new Point(200, 300);
                vertices.Add(p);
                p = new Point(400, 200);
                vertices.Add(p);
                Console.WriteLine("Created an array of vertex points.");

                // Create and add a new PolyLineAnnotation to the 0th element of first page's annotation array
                PolyLineAnnotation polyLineAnnot = new PolyLineAnnotation(page, rect, vertices, -1);
                Console.WriteLine("Created new PolyLineAnnotation as 0th element of annotation array.");

                // Now let's retrieve and display the vertices
                IList<Point> vertices2;
                vertices2 = polyLineAnnot.Vertices;
                Console.WriteLine("Retrieved the vertices of the polyline annotation.");
                Console.WriteLine("They are:");
                for (int i = 0; i < vertices2.Count; i++)
                {
                    Console.WriteLine("Vertex " + i + ": " + vertices2[i]);
                }

                // Add some line ending styles to the ends of our PolyLine
                polyLineAnnot.StartPointEndingStyle = LineEndingStyle.ClosedArrow;
                polyLineAnnot.EndPointEndingStyle = LineEndingStyle.OpenArrow;

                // Let's set some colors and then ask the polyline to generate an appearance stream
                Color color = new Color(0.5, 0.3, 0.8);
                polyLineAnnot.InteriorColor = color;
                color = new Color(0.0, 0.8, 0.1);
                polyLineAnnot.Color = color;
                Console.WriteLine("Set the stroke and fill colors.");
                Form form = polyLineAnnot.GenerateAppearance();
                polyLineAnnot.NormalAppearance = form;
                Console.WriteLine("Generated the appearance stream.");

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
