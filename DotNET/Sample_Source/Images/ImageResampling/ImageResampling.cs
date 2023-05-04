using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates how to find and resample images within a PDF document. The images are
 * then put back into the PDF document with a new resolution.
 * 
 * Resampling involves resizing an image or images within a PDF document. Commonly this process is 
 * used to reduce the resolution of an image or series of images, to make them smaller. As a result
 * the process makes the PDF document smaller.
 *
 * For more detail see the description of the ImageResampling sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/entering-or-generating-graphics-from-pdf-files
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ImageResampling
{
    class ImageResampling
    {
        static int numreplaced;

        static void ResampleImages(Content content)
        {
            int i = 0;
            while (i < content.NumElements)
            {
                Element e = content.GetElement(i);
                Console.WriteLine(i + " / " + content.NumElements + " = " + e.GetType());
                if (e is Datalogics.PDFL.Image)
                {
                    Datalogics.PDFL.Image img = (Datalogics.PDFL.Image) e;
                    try
                    {
                        Datalogics.PDFL.Image newimg = img.ChangeResolution(400);
                        Console.WriteLine("Replacing an image...");
                        content.AddElement(newimg, i);
                        content.RemoveElement(i);
                        Console.WriteLine("Replaced.");
                        numreplaced++;
                    }
                    catch (ApplicationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else if (e is Container)
                {
                    Console.WriteLine("Recursing through a Container");
                    ResampleImages((e as Container).Content);
                }
                else if (e is Group)
                {
                    Console.WriteLine("Recursing through a Group");
                    ResampleImages((e as Group).Content);
                }
                else if (e is Form)
                {
                    Console.WriteLine("Recursing through a Form");
                    Content formcontent = (e as Form).Content;
                    ResampleImages(formcontent);
                    (e as Form).Content = formcontent;
                }

                i++;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("ImageResampling Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String sOutput = "ImageResampling-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file " + sInput + ". Writing to output file " + sOutput);

                Document doc = new Document(sInput);

                Console.WriteLine("Opened a document.");
                try
                {
                    for (int pgno = 0; pgno < doc.NumPages; pgno++)
                    {
                        numreplaced = 0;
                        Page pg = doc.GetPage(pgno);
                        Content content = pg.Content;
                        ResampleImages(content);
                        if (numreplaced != 0)
                        {
                            pg.UpdateContent();
                        }
                    }

                    doc.Save(SaveFlags.Full | SaveFlags.CollectGarbage, sOutput);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
