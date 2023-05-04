using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * This sample demonstrates drawing a list of grayscale separations from a PDF file to multi-paged TIFF file.
 *
 * For more detail see the description of the GetSeparatedImages sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/manipulating-graphics-and-separating-colors-for-images#getseparatedimages
 * 
 * 
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace GetSeparatedImages
{
    class GetSeparatedImages
    {
        static void Main(string[] args)
        {

            Console.WriteLine("GetSeparatedImages Sample:");

            String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
            String sOutput = "GetSeparatedImages-out.tiff";

            if (args.Length > 0)
                sInput = args[0];
            if (args.Length > 1)
                sOutput = args[1];

            Console.WriteLine("Input file: " + sInput + ", will write to " + sOutput);


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Document doc = new Document(sInput);
                Page pg = doc.GetPage(0);

                // Get all inks that are present on the page
                IList<Ink> inks = pg.ListInks();
                List<SeparationColorSpace> colorants = new List<SeparationColorSpace>();

                // Here we decide, which inks should be drawn
                foreach (Ink theInk in inks)
                {
                    // note: if the Ink can't be found in page's resources,
                    // default tintTransform and alternate will be used
                    colorants.Add(new SeparationColorSpace(pg, theInk));
                }

                PageImageParams pip = new PageImageParams();
                pip.HorizontalResolution = 300;
                pip.VerticalResolution = 300;

                ImageCollection images = pg.GetImageSeparations(pg.CropBox, pip, colorants);
                // Save images as multi-paged tiff - each page is a separated color from the page bitmap.
                images.Save(sOutput, ImageType.TIFF);
            }
        }
    }
}
