using System;
using Datalogics.PDFL;

/*
 * Demonstrates working with the Lab color space. The Lab color space is based on the
 * CIE XYZ color space, but it includes a dimension L, for lightness,
 * along with a and b coordinates, to define the color.
 *
 * For more detail see the description of the ColorSpace sample programs on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace MakeDocWithLabColorSpace
{
    class MakeDocWithLabColorSpace
    {
        static void Main(string[] args)
        {

            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sOutput = "Lab-out.pdf";

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
                // CIE 1976 L*a*b* space with the CCIR XA/11-recommended D65
                // white point. The a* and b* components, although
                // theoretically unbounded, are defined to lie in the useful
                // range -128 to +127

                Double[] whitePoint = {0.9505, 1.0000, 1.0890};
                Double[] blackPoint = {0.0, 0.0, 0.0};
                Double[] range = {-128.0, 127.0, -128.0, 127.0};
                ColorSpace cs = new LabColorSpace(whitePoint, blackPoint, range);

                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(cs, new Double[] {55, -54, 55});


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
