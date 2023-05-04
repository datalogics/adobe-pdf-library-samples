using System;
using Datalogics.PDFL;


/*
 * 
 * The Watermark sample program shows how to create a watermark and copy it to a new PDF file.
 * You could use this code to create a message to apply to PDF files you select, like
 * “Confidential” or “Draft Copy.” Or you might want to place a copyright statement over
 * a set of photographs shown in a PDF file so that they cannot be easily duplicated without
 * the permission of the owner.
 * 
 * For more detail see the description of the Watermark sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/adding-text-and-graphic-elements#watermark
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace Watermark
{
    class Watermark
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Watermark Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sWatermark = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String sOutput = "Watermark-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sWatermark = args[1];

                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("Adding watermark from " + sWatermark + " to " + sInput + " and saving to " +
                                  sOutput);

                Document doc = new Document(sInput);

                Document watermarkDoc = new Document(sWatermark);

                WatermarkParams watermarkParams = new WatermarkParams();
                watermarkParams.Opacity = 0.8f;
                watermarkParams.Rotation = 45.3f;
                watermarkParams.Scale = 0.5f;
                watermarkParams.TargetRange.PageSpec = PageSpec.EvenPagesOnly;

                doc.Watermark(watermarkDoc.GetPage(0), watermarkParams);

                watermarkParams.TargetRange.PageSpec = PageSpec.OddPagesOnly;

                WatermarkTextParams watermarkTextParams = new WatermarkTextParams();
                Color color = new Color(109.0f / 255.0f, 15.0f / 255.0f, 161.0f / 255.0f);
                watermarkTextParams.Color = color;

                watermarkTextParams.Text = "Multiline\nWatermark";

                Font f = new Font("Courier", FontCreateFlags.Embedded | FontCreateFlags.Subset);
                watermarkTextParams.Font = f;
                watermarkTextParams.TextAlign = HorizontalAlignment.Center;

                doc.Watermark(watermarkTextParams, watermarkParams);

                doc.EmbedFonts();
                doc.Save(SaveFlags.Full | SaveFlags.Linearized, sOutput);
            }
        }
    }
}
