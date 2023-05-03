using System;
using Datalogics.PDFL;

/*
 * 
 * This sample program reads the pages of the PDF file that you provide and extracts images
 * that it finds on each page and saves those images to external graphics files, one each for
 * TIF, JPG, PNG, GIF, and BMP. 
 * 
 * To be more specific, the program examines the content stream for image elements and exports
 * those image objects. If a page in a PDF file has three images, the program will create three
 * sets of graphics files for those three images. The sample program ignores text, parsing the
 * PDF syntax to identify any raster or vector images found on every page.
 *
 * For more detail see the description of the ImageExport sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/exporting-images-from-pdf-files
 * 
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ImageExport
{
    public class ExportDocumentImages
    {
        ImageType exporttype;
        int next;
        ImageCollection ic = new ImageCollection();

        public void export_doc_images_type(Document doc, ImageType imtype)
        {
            exporttype = imtype;
            int pgno;
            for (pgno = 0; pgno < doc.NumPages; pgno++)
            {
                Page pg = doc.GetPage(pgno);
                Content content = pg.Content;
                Export_Element_Images(content);
            }

            if (ic.Count != 0)
            {
                try
                {
                    ic.Save("ImageExport-page" + pgno + "-out.tif", ImageType.TIFF);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot write file: " + ex.Message);
                }
            }
        }

        public void export_doc_images(Document doc)
        {
            export_doc_images_type(doc, ImageType.TIFF);
            export_doc_images_type(doc, ImageType.JPEG);
            export_doc_images_type(doc, ImageType.PNG);
            export_doc_images_type(doc, ImageType.GIF);
            export_doc_images_type(doc, ImageType.BMP);
        }

        public void Export_Element_Images(Content content)
        {
            int i = 0;
            ImageSaveParams isp;

            while (i < content.NumElements)
            {
                Element e = content.GetElement(i);
                if (e is Datalogics.PDFL.Image)
                {
                    Datalogics.PDFL.Image img = (Datalogics.PDFL.Image) e;
                    // Weed out impossible on nonsensical combinations.
                    if (img.ColorSpace == ColorSpace.DeviceCMYK && exporttype != ImageType.JPEG)
                    {
                        i++;
                        continue;
                    }

                    if (exporttype == ImageType.TIFF)
                    {
                        ic.Append(img);
                        isp = new ImageSaveParams();
                        isp.Compression = CompressionCode.LZW;
                        img.Save("ImageExport-out" + next + ".tif", exporttype, isp);
                    }

                    try
                    {
                        if (exporttype == ImageType.JPEG)
                        {
                            isp = new ImageSaveParams();
                            isp.JPEGQuality = 80;
                            img.Save("ImageExport-out" + next + ".jpg", exporttype, isp);
                        }

                        if (exporttype == ImageType.PNG)
                        {
                            img.Save("ImageExport-out" + next + ".png", exporttype);
                        }

                        if (exporttype == ImageType.GIF)
                        {
                            img.Save("ImageExport-out" + next + ".gif", exporttype);
                        }

                        if (exporttype == ImageType.BMP)
                        {
                            img.Save("ImageExport-out" + next + ".bmp", exporttype);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot write file: " + ex.Message);
                    }

                    next++;
                }
                else if (e is Container)
                {
                    Console.WriteLine("Recursing through a Container");
                    Export_Element_Images((e as Container).Content);
                }
                else if (e is Group)
                {
                    Console.WriteLine("Recursing through a Group");
                    Export_Element_Images((e as Group).Content);
                }
                else if (e is Form)
                {
                    Console.WriteLine("Recursing through a Form");
                    Export_Element_Images((e as Form).Content);
                }

                i++;
            }
        }
    }

    class ImageExport
    {
        static void Main(String[] args)
        {
            Console.WriteLine("Image Export sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);
                ExportDocumentImages expdoc = new ExportDocumentImages();
                expdoc.export_doc_images(doc);
            }
        }
    }
}
