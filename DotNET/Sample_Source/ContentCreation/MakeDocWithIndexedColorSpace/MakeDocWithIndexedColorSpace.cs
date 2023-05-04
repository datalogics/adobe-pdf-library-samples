using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * Demonstrates working with the Indexed Color Space. Indexed Color Spaces are used
 * to reduce the amount of system memory used for processing images when a limited
 * number of colors are needed.
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

namespace MakeDocWithIndexedColorSpace
{
    class MakeDocWithIndexedColorSpace
    {
        static void Main(string[] args)
        {

            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sOutput = "Indexed-out.pdf";

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

                ColorSpace baseCS = ColorSpace.DeviceRGB;

                List<Int32> lookup = new List<Int32>();

                int[] lowhi = {0, 255};
                foreach (int r in lowhi)
                {
                    foreach (int g in lowhi)
                    {
                        foreach (int b in lowhi)
                        {
                            lookup.Add(r);
                            lookup.Add(g);
                            lookup.Add(b);
                        }
                    }
                }

                IndexedColorSpace cs = new IndexedColorSpace(baseCS, 7, lookup);

                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(cs, new[] {4.0});


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
