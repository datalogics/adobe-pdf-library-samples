using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates how to copy and paste the annotations from one PDF file to another PDF file.
 * 
 * The program provides two optional input PDF documents. The first is the source of the annotations, and the second
 * is the PDF document that receives these copied annotations. The program also provides a default name and path for 
 * the output PDF document. The second input document, Layers.pdf, is saved as the output document Paste-out.pdf.
 * 
 * You can provide your own file names for these values in the code, or you can enter your own file names as
 * command line parameters.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace AnnotationCopyPaste
{
    class AnnotationCopyPaste
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AnnotationCopyPaste Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput1 = "../../Resources/Sample_Input/sample_annotations.pdf";
                String sInput2 = "../../Resources/Sample_Input/Layers.pdf";
                String sOutput = "../AnnotationCopyPaste-out.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                Document sourceDoc = new Document(sInput1);

                if (args.Length > 1)
                    sInput2 = args[1];

                Document destinationDoc = new Document(sInput2);

                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("Copying annotations from " + sInput1 + " into " + sInput2 + " and writing to " + sOutput);

                Page sourcePage = sourceDoc.GetPage(0);
                Page destinationPage = destinationDoc.GetPage(0);

                int nAnnots = sourcePage.NumAnnotations;

                // find each link annotation on the first page of the source document and
                // copy them to the first page of the destination document
                for (int i = 0; i < nAnnots; i++)
                {
                    Annotation ann = sourcePage.GetAnnotation(i);

                    if (ann.Subtype == "Link")
                    {
                        Rect linkRect = ann.Rect;
                        Point linkCenter = new Point();

                        // find the center of the link
                        linkCenter.H = linkRect.Left + linkRect.Width / 2;
                        linkCenter.V = linkRect.Bottom + linkRect.Height / 2;

                        try
                        {
                            // copy the annotation to the destination and center it on 
                            // the center point of the original annotation.
                            //
                            // This may throw an ApplicationException if it cannot 
                            // copy/paste the annotation, in either case the message in
                            // the exception will specify which operation (copy or paste)
                            // that it could not complete.
                            LinkAnnotation copiedLink = ((LinkAnnotation)ann).CopyTo(destinationPage, linkCenter);
                        }
                        catch (ApplicationException ae)
                        {
                            Console.WriteLine(ae.Message);
                        }
                    }
                }

                destinationDoc.Save(SaveFlags.Full, sOutput);

            }
        }
    }
}
