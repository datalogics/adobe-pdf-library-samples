using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates working with data objects in a PDF document. It examines the Objects and displays
 * information about them.  The sample extracts the dictionary for an object called URIAction and updates it using PDFObjects.
 * 
 * For more detail see the description of the PDFObject sample on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/listing-information-about-values-and-objects-in-pdf-files#pdfobject
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 * 
 * Suggested input file: Library.ResourceDirectory + "Sample_Input/sample_links.pdf"
 * Input file properties: First page must have an annotation with a URI link
 */

namespace PDFObject
{
    class PDFObjectDemo
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PDFObject Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sInput = Library.ResourceDirectory + "Sample_Input/sample_links.pdf";
                String sOutput = "PDFObject-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ". Writing to output " + sOutput);

                Document doc = new Document(sInput);
                Page page = doc.GetPage(0);

                LinkAnnotation annot = (LinkAnnotation) page.GetAnnotation(1);
                URIAction uri = (URIAction) annot.Action;

                // Print some info about the URI action, before we modify it
                Console.WriteLine("Initial URL: " + uri.URI);
                Console.WriteLine("Is Map property: " + uri.IsMap);

                // Modify the URIAction
                //
                // A URI action is a dictionary containing:
                //    Key: S     Contents: a name object with the value "URI" (required)
                //    Key: URI   Contents: a string object for the uniform resource locator (required)
                //    Key: IsMap Contents: a boolean for whether the link is part of a map (optional)
                //    (see section 8.5.3, "Action Types", of the PDF Reference)
                //
                // We will change the URI entry and delete the IsMap entry for this dictionary

                PDFDict uri_dict = uri.PDFDict; // Extract the dictionary

                // Create a new string object
                PDFString uri_string = new PDFString("http://www.google.com", doc, false, false);

                uri_dict.Put("URI", uri_string); // Change the URI (replaces the old one)
                uri_dict.Remove("IsMap"); // Remove the IsMap entry

                // Check that we deleted the IsMap entry
                Console.WriteLine("Does this dictionary have an IsMap entry? " + uri_dict.Contains("IsMap"));

                doc.Save(SaveFlags.Full, sOutput);
                doc.Close();

                // Check the modified contents of the link
                doc = new Document(sOutput);
                page = doc.GetPage(0);
                annot = (LinkAnnotation) page.GetAnnotation(1);
                uri = (URIAction) annot.Action;

                Console.WriteLine("Modified URL: " + uri.URI);
                Console.WriteLine("Is Map property (if not present, defaults to false): " + uri.IsMap);
            }
        }
    }
}
