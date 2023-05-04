using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates working with masking in PDF documents. Masking an image allows you to remove or 
 * change a feature, while a soft mask allows you to place an image on a page and define the level of 
 * transparency for that image.
 *
 * For more detail see the description of the ImageSoftMask sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/entering-or-generating-graphics-from-pdf-files
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ImageSoftMask
{
    class ImageSoftMask
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Image Soft Mask sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.jpg";
                String sMask = Library.ResourceDirectory + "Sample_Input/Mask.tif";
                String sOutput = "ImageSoftMask-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sMask = args[1];

                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("Input file: " + sInput + ", mask: " + sMask + "; will write to " + sOutput);

                Document doc = new Document();
                Rect pageRect = new Rect(0, 0, 612, 792);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);

                Datalogics.PDFL.Image baseImage;
                baseImage = new Datalogics.PDFL.Image(sInput);
                Console.WriteLine("Created the image to mask.");

                Datalogics.PDFL.Image maskImage;
                maskImage = new Datalogics.PDFL.Image(sMask);
                Console.WriteLine("Created the image to use as mask.");

                baseImage.SoftMask = maskImage;
                Console.WriteLine("Set the SoftMask property.");

                docpage.Content.AddElement(baseImage);
                docpage.UpdateContent();
                Console.WriteLine("Got the content, added the element and updated the content.");

                doc.Save(SaveFlags.Full, sOutput);

                // Dispose the doc object
                doc.Dispose();
                Console.WriteLine("Disposed document object.");
            }
        }
    }
}
