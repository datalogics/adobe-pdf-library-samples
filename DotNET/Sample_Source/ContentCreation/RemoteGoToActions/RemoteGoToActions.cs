using System;
using Datalogics.PDFL;

/*
 * RemoteGoToActions is similar to LaunchActions. The program generates a PDF file
 * with an annotation, in the form of a rectangle. Click on the rectangle shown in this
 * PDF file and a separate PDF file opens.
 * 
 * RemoteGoToActions differs from LaunchActions in that it includes a RemoteDestination object.
 * This object describes the rectangle used in the PDF file in a series of statements at the command prompt. 
 *
 * For more detail see the description of the AnnotationCopyPaste sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-actions-in-pdf-files#remotegotoactions
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace RemoteGoToActions
{
    class RemoteGoToActions
    {
        static void Main(string[] args)
        {

            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sFileSpec = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String sOutput = "RemoteGoToActions-out.pdf";

                if (args.Length > 0)
                    sFileSpec = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Writing to output " + sOutput + ". Using " + sFileSpec + " as file specification");

                Document doc = new Document();

                // Standard letter size page (8.5" x 11")
                Rect pageRect = new Rect(0, 0, 612, 792);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);
                Console.WriteLine("Created page.");

                LinkAnnotation newLink = new LinkAnnotation(docpage, new Rect(153, 198, 306, 396));
                Console.WriteLine("Created new Link Annotation");

                // RemoteGoToActions need a FileSpecification and a RemoteDestination.
                //
                // The FileSpecification specifies which file should be opened when the LinkAnnotation is "clicked"
                //
                // The RemoteDestination specifies a particular view (page number, Fit type, rectangle, and zoom level)
                // to display when the file in the FileSpecification is opened. Remember page numbers start at 0.
                //
                // FileSpecification objects, RemoteDestination objects, and RemoteGoToActions 
                // must be associated with a Document. The association happens at object creation time and cannot be changed.
                //
                // FileSpecifications and RemoteDestinations are associated with the Document that is passed in the constructor.
                // RemoteGoToActions are associated with the same Document as the RemoteDestination used to create it.
                //
                // When creating a RemoteGoToAction, make sure that the FileSpecification and RemoteDestination are both
                // associated with the same Document, or an exception will be thrown.

                // FileSpecifications can take either a relative or an absolute path. It is best to specify
                // a relative path if the document is intended to work across multiple platforms (Windows, Linux, Mac)
                FileSpecification fileSpec = new FileSpecification(doc, sFileSpec);
                Console.WriteLine("Path to remote document : " + fileSpec.Path);

                RemoteDestination remoteDest =
                    new RemoteDestination(doc, 0, "XYZ", new Rect(0, 0, 4 * 72, 4 * 72), 1.5);
                Console.WriteLine("When the Link is clicked the remote document will open to : ");
                Console.WriteLine("Page Number : " + remoteDest.PageNumber);
                Console.WriteLine("zoom level : " + remoteDest.Zoom);
                Console.WriteLine("fit type : " + remoteDest.FitType);
                Console.WriteLine("rectangle : " + remoteDest.DestRect);

                // Now create the RemoteGoToAction from the fileSpec and the RemoteDestination
                RemoteGoToAction remoteAction = new RemoteGoToAction(fileSpec, remoteDest);

                // assign the RemoteGoToAction to the LinkAnnotation
                newLink.Action = remoteAction;

                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
