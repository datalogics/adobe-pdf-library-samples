using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * Demonstrates working with the Calibrated Gray Space (CaLGray), based on the CIE color space.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 * 
 * For more detail see the description of the ColorSpace sample programs on our Developer�s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace MakeDocWithCalGrayColorSpace
{
    class MakeDocWithCalGrayColorSpace
    {
        static void Main(string[] args)
        {
            using (Library lib = new Library())
            {

                String sOutput = "../CalGray-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Writing to output " + sOutput);

                Document doc = new Document();
                Page page = doc.CreatePage(Document.BeforeFirstPage, new Rect(0, 0, 5 * 72, 4 * 72));
                Content content = page.Content;
                Font font = new Font("Times-Roman", FontCreateFlags.Embedded | FontCreateFlags.Subset);

                // a space consisting of the Y dimension of the CIE 1931 XYZ
                // space with the CCIR XA/11-recommended D65 white point and
                // opto-electronic transfer function.

                Double[] whitePoint = { 0.9505, 1.0000, 1.0890 };
                Double[] blackPoint = { 0.0, 0.0, 0.0 };
                double gamma = 2.2222;

                ColorSpace cs = new CalGrayColorSpace(whitePoint, blackPoint, gamma);
                GraphicState gs = new GraphicState();
                gs.FillColor = new Color(cs, new Double[] { 0.5 });


                Matrix textMatrix = new Matrix(24, 0, 0, 24, // Set font width and height to 24 point size
                                         1 * 72, 2 * 72);   // x, y coordinate on page, 1" x 2"

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
