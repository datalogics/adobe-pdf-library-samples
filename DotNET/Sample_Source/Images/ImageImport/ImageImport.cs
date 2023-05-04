using System;
using Datalogics.PDFL;

/*
 * 
 * This program demonstrates how to import images into a PDF file. The program runs without
 * prompting you, and creates two PDF files, demonstrating how to import graphics from image files
 * into a PDF file. One of the PDF output files is the result of graphics imported from a multi-page TIF file.
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ImageImport
{
// In this scenario the Image object is used alone to create a
// new PDF page with the image as the content.
    class ImageImport
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Import Images Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");
                Document doc = new Document();

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.jpg";
                String sOutput = "ImageImport-out1.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Reading image file" + sInput + " and writing " + sOutput);

                using (Image newimage = new Image(sInput, doc))
                {
                    // Create a PDF page which is one inch larger all around than this image
                    // The design width and height for the image are carried in the
                    // Matrix.A and Matrix.D fields, respectively.
                    // There are 72 PDF user space units in one inch.
                    Rect pageRect = new Rect(0, 0, newimage.Matrix.A + 144, newimage.Matrix.D + 144);
                    Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);
                    // Center the image on the page
                    newimage.Translate(72, 72);
                    docpage.Content.AddElement(newimage);
                    docpage.UpdateContent();
                }

                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
