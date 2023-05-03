using System;
using Datalogics.PDFL;

/*
 * Demonstrates working with the Calibrated RGB Color Space, based on the CIE color space.
 * A, B, and C represent red, blue, and green color values.
 *
 * For more detail see the description of the ColorSpace sample programs on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace MakeDocWithCalRGBColorSpace
{
    class MakeDocWithCalRGBColorSpace
    {
        static void Main(string[] args)
        {

            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sOutput = "CalRGB-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Writing to output " + sOutput);

                Document doc = new Document();
                Page page = doc.CreatePage(Document.BeforeFirstPage, new Rect(0, 0, 5 * 72, 4 * 72));
                Content content = page.Content;
                Font font;
                try
                {
                    font = new Font("Times-Roman", FontCreateFlags.Embedded | FontCreateFlags.Subset);
                }
                catch (ApplicationException ex)
                {
                    if (ex.Message.Equals("The specified font could not be found.") &&
                        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                            .OSPlatform.Linux) &&
                        !System.IO.Directory.Exists("/usr/share/fonts/msttcore/"))
                    {
                        Console.WriteLine("Please install Microsoft Core Fonts on Linux first.");
                        return;
                    }

                    throw;
                }
                // a CalRGB color space for the CCIR XA/11-recommended D65
                // white point with 1.8 gammas and Sony Trinitron phosphor
                // chromaticities
                // 
                // Plus a dummy value for testing the black point

                Double[] whitePoint = {0.9505, 1.0000, 1.0890};
                Double[] blackPoint = {0.0, 0.0, 0.0};
                Double[] gamma = {1.8, 1.8, 1.8};
                Double[] matrix = {0.4497, 0.2446, 0.0252, 0.3163, 0.6720, 0.1412, 0.1845, 0.0833, 0.9227};
                ColorSpace cs = new CalRGBColorSpace(whitePoint, blackPoint, gamma, matrix);


                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(cs, new[] {0.3, 0.7, 0.3});


                Matrix textMatrix = new Matrix(24, 0, 0, 24, // Set font width and height to 24 point size
                    1 * 72, 2 * 72); // x, y coordinate on page, 1" x 2"

                TextRun textRun = new TextRun("Hello World!", font, gs, new TextState(), textMatrix);
                Text text = new Text();
                text.AddRun(textRun);
                content.AddElement(text);
                page.UpdateContent();

                doc.EmbedFonts();
                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
