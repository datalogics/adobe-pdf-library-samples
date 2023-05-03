using System;
using Datalogics.PDFL;

/*
 * The sample creates a PDF document with a single blank page, featuring a rectangle.
 * An action is added to the rectangle in the form of a hyperlink; if the reader clicks
 * on the rectangle, a different PDF file opens, showing an image.
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace LaunchActions
{
    class LaunchActions
    {
        static void Main(string[] args)
        {

            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String sOutput = "LaunchActions-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ". Writing to output " + sOutput);

                Document doc = new Document();

                // Standard letter size page (8.5" x 11")
                Rect pageRect = new Rect(0, 0, 612, 792);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);
                Console.WriteLine("Created page.");

                LinkAnnotation newLink = new LinkAnnotation(docpage, new Rect(153, 198, 306, 396));
                Console.WriteLine("Created new Link Annotation");

                // LaunchActions need a FileSpecification object.
                //
                // The FileSpecification specifies which file should be opened when the Link Annotation is "clicked".
                // 
                // Both FileSpecification objects and LaunchAction objects must be associated with a Document.
                // The association happens at object creation time and cannot be changed.
                //
                // FileSpecifications are associated with the Document that is passed in the constructor.
                // LaunchActions are associated with the same Document as the FileSpecification used to create it.

                // FileSpecifications can take either a relative or an absolute path. It is best to specify
                // a relative path if the document is intended to work across multiple platforms (Windows, Linux, Mac)
                FileSpecification fileSpec = new FileSpecification(doc, sInput);
                Console.WriteLine("Created a new FileSpecification with a path : " + fileSpec.Path);

                LaunchAction launch = new LaunchAction(fileSpec);
                Console.WriteLine("Created a new Launch Action");

                // setting NewWindow to true causes the document to open in a new window,
                // by default this is set to false.
                launch.NewWindow = true;

                newLink.Action = launch;

                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
