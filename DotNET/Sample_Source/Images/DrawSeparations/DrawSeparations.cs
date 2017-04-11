using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using Datalogics.PDFL;

/*
 * This sample demonstrates for drawing a list of grayscale separations from a PDF file.
 *
 * For more detail see the description of the DrawSeparations sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/manipulating-graphics-and-separating-colors-for-images
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace DrawSeparations
{
    class DrawSeparations
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DrawSeparations Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = "../../Resources/Sample_Input/ducky.pdf";
                String sOutput = "../DrawSeparations-out";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ", writing output using prefix: " + sOutput);

                Document doc = new Document(sInput);
                Page pg = doc.GetPage(0);

                // Get all inks that are present on the page
                List<Ink> inks = (List<Ink>)pg.ListInks();
                List<SeparationColorSpace> colorants = new List<SeparationColorSpace>();

                // Here we decide, which inks should be drawn
                foreach (Ink theInk in inks)
                {
                    // note: if the Ink can't be found in page's resources,
                    // default tintTransform and alternate will be used
                    colorants.Add(new SeparationColorSpace(pg, theInk));
                }

                double width = pg.MediaBox.Right - pg.MediaBox.Left;
                double height = pg.MediaBox.Top - pg.MediaBox.Bottom;

                // Must invert the page to get from PDF with origin at lower left,
                // to a bitmap with the origin at upper right.
                Matrix matrix = new Matrix().Scale(1, -1).Translate(0, -height);

                DrawParams parms = new DrawParams();
                parms.Matrix = matrix;
                parms.DestRect = new Rect(0, 0, width, height);
                parms.Flags = DrawFlags.DoLazyErase | DrawFlags.UseAnnotFaces;

                // Acquiring list of separations
                List<Bitmap> separatedColorChannels = pg.DrawContents(parms, colorants);

                for (int i = 0; i < separatedColorChannels.Count; ++i)
                {
                    separatedColorChannels[i].Save(sOutput + i + "-" + colorants[i].SeparationName + ".png", ImageFormat.Png);
                }
            }

        }
    }
}
