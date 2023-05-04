using System;
using System.IO;
using Datalogics.PDFL;
using SkiaSharp;

/*
 * 
 * This sample program searches through the PDF file that you select and identifies
 * raster drawings, diagrams and photographs among the text. Then, it extracts these
 * images from the PDF file and copies them to a separate set of graphics files in the
 * same directory. Vector images, such as clip art, will not be exported.
 * 
 * For more detail see the description of the ImageExtraction sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/exporting-images-from-pdf-files/#imageextraction
 * 
 * 
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ImageExtraction
{
    class ImageExtraction
    {
        static int next;

        static void ExtractImages(Content content)
        {
            for (int i = 0; i < content.NumElements; i++)
            {
                Element e = content.GetElement(i);
                if (e is Datalogics.PDFL.Image)
                {
                    Console.WriteLine("Saving an image");
                    Datalogics.PDFL.Image img = (Datalogics.PDFL.Image)e;
                    using (SKBitmap sKBitmap = img.SKBitmap)
                    {
                        using (FileStream f = File.OpenWrite("ImageExtraction-extract-out" + (next) + ".Png"))
                            sKBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(f);
                    }

                    Datalogics.PDFL.Image newimg = img.ChangeResolution(500);
                    using (SKBitmap sKBitmap = newimg.SKBitmap)
                    {
                        using (FileStream f = File.OpenWrite("ImageExtraction-extract-Resolution-500-out" + (next) + ".Png"))
                            sKBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(f);
                    }
                    next++;

                }
                else if (e is Datalogics.PDFL.Container)
                {
                    ExtractImages((e as Datalogics.PDFL.Container).Content);
                }
                else if (e is Datalogics.PDFL.Group)
                {
                    ExtractImages((e as Datalogics.PDFL.Group).Content);
                }
                else if (e is Form)
                {
                    ExtractImages((e as Form).Content);
                }
            }
        }

        static void Main(string[] args)
        {

            Console.WriteLine("ImageExtraction Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);

                Page pg = doc.GetPage(0);
                Content content = pg.Content;
                ExtractImages(content);
            }
        }
    }
}
