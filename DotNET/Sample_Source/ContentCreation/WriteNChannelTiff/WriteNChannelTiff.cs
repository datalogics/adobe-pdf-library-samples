using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * This sample generates a multi-page TIFF file, selecting graphics drawn from
 * the first page of the PDF document provided.
 * 
 * For more detail see the description of the WriteNChannelTiff sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/manipulating-graphics-and-separating-colors-for-images#writenchanneltiff
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace WriteNChannelTiff
{
    class WriteNChannelTiff
    {
        static void Main(string[] args)
        {
            Console.WriteLine("WriteNChannelTiff Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sOutput = "WriteNChannelTiff-out.tif";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + " writing to " + sOutput);

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

                Datalogics.PDFL.Image images = pg.GetImage(pg.CropBox, pip, colorants);
                // Save images as multi-channeled tiff.
                images.Save(sOutput, Datalogics.PDFL.ImageType.TIFF);
            }
        }
    }
}
