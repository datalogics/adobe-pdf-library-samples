using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * This sample creates a PDF document with a single page, featuring a rectangle.
 * An action is added to the rectangle in the form of a hyperlink; if the reader
 * clicks on the rectangle, the system opens a Datalogics web page.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace Actions
{
    class Actions
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Actions Sample:");

            using (Library lib = new Library())
            {
                String sOutput = "../Actions-out.pdf";

                Console.WriteLine("Initialized the library.");
                
                Document doc = new Document();

                using (Path newpath = new Path())
                {
                    // Create a PDF page which is the same size of the image.
                    Rect pageRect = new Rect(0, 0, 100, 100);
                    Page docpage = doc.CreatePage(Document.BeforeFirstPage,
                        pageRect);
                    Console.WriteLine("Created page.");

                    // Create our first link with a URI action
                    LinkAnnotation newLink = (LinkAnnotation)docpage.CreateAnnotation("Link", new Rect(1.0, 2.0, 3.0, 4.0));
                    Console.WriteLine(newLink.ToString());

                    doc.BaseURI = "http://www.datalogics.com";
                    URIAction uri = new URIAction("/products/pdfl/pdflibrary.asp", false);
                    Console.WriteLine("Action data: " + uri.ToString());

                    newLink.Action = uri;
                    docpage.AddAnnotation(-2, newLink);

                    // Create a second link with a GoTo action
                    LinkAnnotation secondLink = (LinkAnnotation)docpage.CreateAnnotation("Link", new Rect(5.0, 6.0, 7.0, 8.0));

                    Rect r = new Rect(5, 5, 100, 100);
                    GoToAction gta = new GoToAction(new ViewDestination(doc, 0, "FitR", r, 1.0));
                    Console.WriteLine("Action data: " + gta.ToString());

                    secondLink.Action = gta;
                    docpage.AddAnnotation(-2, secondLink);

                    // Read some URI properties
                    Console.WriteLine("Extracted URI: " + uri.URI);

                    if (uri.IsMap)
                        Console.WriteLine("Send mouse coordinates");
                    else
                        Console.WriteLine("Don't send mouse coordinates");

                    // Change the URI properties
                    doc.BaseURI = "http://www.datalogics.com";
                    uri.URI = "/products/pdfl/pdflibrary.asp";

                    uri.IsMap = true;

                    Console.WriteLine("Complete changed URI:" + doc.BaseURI + uri.URI);
 
                    if (uri.IsMap)
                        Console.WriteLine("Send mouse coordinates");
                    else
                        Console.WriteLine("Don't send mouse coordinates");

                    // Testing gta
                    Console.WriteLine("Fit type of destination: " + gta.Destination.FitType.ToString());
                    Console.WriteLine("Rectangle of destination: " + gta.Destination.DestRect.ToString());
                    Console.WriteLine("Zoom of destination: " + gta.Destination.Zoom.ToString());
                    Console.WriteLine("Page number of destination: " + gta.Destination.PageNumber.ToString());

                    doc.Save(SaveFlags.Full, sOutput);
                }
            }
        }
    }
}
