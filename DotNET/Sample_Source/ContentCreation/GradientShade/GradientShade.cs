using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates changing the shading of an image on a PDF document page. The image gradually
 * changes from black on the left side of the image, to red on the right side.
 * 
 * For more detail see the description of the GradientShade sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/entering-or-generating-graphics-from-pdf-files
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace GradientShade
{
    class GradientShade
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GradientShade Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "GradientShade-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Will write new file: " + sOutput);

                Document doc = new Document();
                Rect pageRect = new Rect(0, 0, 792, 612);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);

                Double[] domain = {0.0, 1.0};
                Double[] C0 = {0.0, 0.0, 0.0};
                Double[] C1 = {1.0, 0.0, 0.0};
                ExponentialFunction f = new ExponentialFunction(domain, 3, C0, C1, 1);

                Point[] coords =
                {
                    new Point(72, 72),
                    new Point(4 * 72, 72)
                };

                Function[] functionList = {f};
                AxialShadingPattern asp = new AxialShadingPattern(ColorSpace.DeviceRGB, coords, functionList);

                Datalogics.PDFL.Path path = new Datalogics.PDFL.Path();
                GraphicState gs = path.GraphicState;

                // Please note path does not create a default graphic state,
                // so this is necessary, but for the first element only.
                if (gs == null)
                    gs = new GraphicState();

                gs.FillColor = new Color(asp);
                path.GraphicState = gs;

                path.MoveTo(new Point(36, 36));
                path.AddLine(new Point(36, 8 * 72 - 36));
                path.AddLine(new Point(11 * 72 - 36, 8 * 72 - 36));
                path.AddLine(new Point(11 * 72 - 36, 36));
                path.ClosePath();
                path.PaintOp = PathPaintOpFlags.Stroke | PathPaintOpFlags.Fill;

                docpage.Content.AddElement(path); // Add the new element to the Content of the page.
                docpage.UpdateContent(); // Update the PDF page with the changed content

                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
