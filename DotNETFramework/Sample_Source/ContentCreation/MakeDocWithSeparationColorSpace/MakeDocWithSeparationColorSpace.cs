using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * This sample demonstrates creating a PDF document that uses a Separation color space.
 *
 * For more detail see the description of the ColorSpace sample programs on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace MakeDocWithSeparationColorSpace
{
    class MakeDocWithSeparationColorSpace
    {
        static void Main(string[] args)
        {
            using (Library lib = new Library())
            {

                String sOutput = "../Separation-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Writing to output " + sOutput);

                Document doc = new Document();
                Page page = doc.CreatePage(Document.BeforeFirstPage, new Rect(0, 0, 5 * 72, 4 * 72));
                Content content = page.Content;
                Font font = new Font("Times-Roman", FontCreateFlags.Embedded | FontCreateFlags.Subset);

                ColorSpace alternate = ColorSpace.DeviceRGB;

                Double[] domain = { 0.0, 1.0 };
                int nOutputs = 3;
                Double[] range = { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 };
                Double[] C0 = { 0.0, 0.0, 0.0 };
                Double[] C1 = { 1.0, 0.0, 0.0 };
                Function tintTransform = new ExponentialFunction(domain, nOutputs, C0, C1, 1.0);
                tintTransform.Range = range;

                ColorSpace cs = new SeparationColorSpace("DLColor", alternate, tintTransform);
                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(cs, new Double[] { 1.0 });

                Matrix textMatrix = new Matrix(24, 0, 0, 24, // Set font width and height to 24 point size
                                         1 * 72, 2 * 72);  // x, y coordinate on page, 1" x 2"

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
