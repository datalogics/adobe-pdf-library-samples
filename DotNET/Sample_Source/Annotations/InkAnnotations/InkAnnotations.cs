using System;
using System.Collections.Generic;
using Datalogics.PDFL;


/*
 * 
 * This sample creates and adds a new Ink annotation to a PDF document. An Ink annotation is a freeform line,
 * similar to what you would create with a pen, or with a stylus on a mobile device.
 *
 * For more detail see the description of the InkAnnotations sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-annotations#inkannotations
 *  
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace InkAnnotations
{
    class InkAnnotations
    {
        static void Main()
        {
            Console.WriteLine("InkAnnotations Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                // Create a new document and blank first page
                Document doc = new Document();
                Rect rect = new Rect(0, 0, 612, 792);
                Page page = doc.CreatePage(Document.BeforeFirstPage, rect);
                Console.WriteLine("Created new document and first page.");

                // Create and add a new InkAnnotation to the 0th element of first page's annotation array
                InkAnnotation inkAnnot = new InkAnnotation(page, rect, -1);
                Console.WriteLine("Created new InkAnnotation as 0th element of annotation array.");

                // Ask how many scribbles are in the ink annotation
                Console.WriteLine("Number of scribbles in ink annotation: " + inkAnnot.NumScribbles);

                // Create a vector of scribble vertices
                List<Point> scribble = new List<Point>();
                Point p = new Point(100, 100);
                scribble.Add(p);
                p = new Point(200, 300);
                scribble.Add(p);
                p = new Point(400, 200);
                scribble.Add(p);
                Console.WriteLine("Created an array of scribble points.");

                // Add the scribble to the ink annotation
                inkAnnot.AddScribble(scribble);
                Console.WriteLine("Added the scribble to the ink annotation.");

                // Ask how many scribbles are in the ink annotation
                Console.WriteLine("Number of scribbles in ink annotation: " + inkAnnot.NumScribbles);

                // Create another vector of scribble vertices
                scribble = new List<Point>();
                p = new Point(200, 200);
                scribble.Add(p);
                p = new Point(200, 300);
                scribble.Add(p);
                p = new Point(300, 300);
                scribble.Add(p);
                p = new Point(300, 200);
                scribble.Add(p);
                p = new Point(200, 100);
                scribble.Add(p);
                Console.WriteLine("Created another array of scribble points.");

                // Add the scribble to the ink annotation
                inkAnnot.AddScribble(scribble);
                Console.WriteLine("Added the scribble to the ink annotation.");

                // Create another vector of scribble vertices
                scribble = new List<Point>();
                p = new Point(300, 400);
                scribble.Add(p);
                p = new Point(200, 300);
                scribble.Add(p);
                p = new Point(300, 300);
                scribble.Add(p);
                Console.WriteLine("Created another array of scribble points.");

                // Add the scribble to the ink annotation
                inkAnnot.AddScribble(scribble);
                Console.WriteLine("Added the scribble to the ink annotation.");

                // Ask how many scribbles are in the ink annotation
                Console.WriteLine("Number of scribbles in ink annotation: " + inkAnnot.NumScribbles);

                // Get and display the points in ink annotation 0
                IList<Point> scribbleToGet = inkAnnot.GetScribble(0);
                for (int i = 0; i < scribbleToGet.Count; i++)
                    Console.WriteLine("Scribble 0, point" + i + " :" + scribbleToGet[i]);

                // Get and display the points in ink annotation 1
                scribbleToGet = new List<Point>();
                scribbleToGet = inkAnnot.GetScribble(1);
                for (int i = 0; i < scribbleToGet.Count; i++)
                    Console.WriteLine("Scribble 1, point" + i + " :" + scribbleToGet[i]);

                // Let's set the color and then ask the ink annotation to generate an appearance stream
                Color color = new Color(0.5, 0.3, 0.8);
                inkAnnot.Color = color;
                Console.WriteLine("Set the stroke color.");
                Form form = inkAnnot.GenerateAppearance();
                inkAnnot.NormalAppearance = form;
                Console.WriteLine("Generated the appearance stream.");

                // Update the page's content and save the file with clipping
                page.UpdateContent();
                doc.Save(SaveFlags.Full, "InkAnnotations-out1.pdf");
                Console.WriteLine("Saved InkAnnotations-out1.pdf");

                // Remove 0th scribble
                inkAnnot.RemoveScribble(0);
                // Ask how many scribbles are in the ink annotation
                Console.WriteLine("Number of scribbles in ink annotation: " + inkAnnot.NumScribbles);

                // Generate a new appearance stream, since we have removed the first scribble.
                inkAnnot.NormalAppearance = inkAnnot.GenerateAppearance();

                // Update the page's content and save the file with clipping
                page.UpdateContent();
                doc.Save(SaveFlags.Full, "InkAnnotations-out2.pdf");

                // Dispose the doc object
                doc.Dispose();
                Console.WriteLine("Disposed document object.");
            }
        }
    }
}
