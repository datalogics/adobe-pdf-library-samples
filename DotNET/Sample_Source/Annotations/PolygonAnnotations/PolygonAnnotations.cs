using System;
using System.Collections.Generic;
using Datalogics.PDFL;


/*
 * 
 * This program generates a PDF output file with a polygon shape (a triangle) as an annotation to the file.
 * The program defines the vertices for the outlines of the annotation, and the line and fill colors.
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PolygonAnnotations
{
    class PolygonAnnotations
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PolygonAnnotation Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "PolygonAnnotations-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Writing to output " + sOutput);

                // Create a new document and blank first page
                Document doc = new Document();
                Rect rect = new Rect(0, 0, 612, 792);
                Page page = doc.CreatePage(Document.BeforeFirstPage, rect);
                Console.WriteLine("Created new document and first page.");

                // Create a vector of polygon vertices
                List<Point> vertices = new List<Point>();
                Point p = new Point(100, 100);
                vertices.Add(p);
                p = new Point(200, 300);
                vertices.Add(p);
                p = new Point(400, 200);
                vertices.Add(p);
                Console.WriteLine("Created an array of vertex points.");

                // Create and add a new PolygonAnnotation to the 0th element of first page's annotation array
                PolygonAnnotation polygonAnnot = new PolygonAnnotation(page, rect, vertices, -1);
                Console.WriteLine("Created new PolygonAnnotation as 0th element of annotation array.");

                // Now let's retrieve and display the vertices
                IList<Point> vertices2;
                vertices2 = polygonAnnot.Vertices;
                Console.WriteLine("Retrieved the vertices of the polygon annotation.");
                Console.WriteLine("They are:");
                for (int i = 0; i < vertices2.Count; i++)
                {
                    Console.WriteLine("Vertex " + i + ": " + vertices2[i]);
                }

                // Let's set some colors and then ask the polyline to generate an appearance stream
                Color color = new Color(0.5, 0.3, 0.8);
                polygonAnnot.InteriorColor = color;
                color = new Color(0.9, 0.7, 0.1);
                polygonAnnot.Color = color;
                Console.WriteLine("Set the stroke and fill colors.");
                Form form = polygonAnnot.GenerateAppearance();
                polygonAnnot.NormalAppearance = form;
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
