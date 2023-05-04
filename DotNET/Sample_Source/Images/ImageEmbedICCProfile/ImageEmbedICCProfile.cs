using System;
using System.IO;
using Datalogics.PDFL;

/*
 * 
 * This sample program demonstrates how to embed an ICC color profile in a graphics file.
 * The program sets up how the output will be rendered and generates a TIF image file or
 * series of TIF files as output.
 * 
 * For more detail see the description of the ImageEmbedICCProfile sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/exporting-images-from-pdf-files/#imageembediccprofile
 * 
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ImageEmbedICCProfile
{
    public class ExportDocumentImages
    {
        ImageType exporttype;

        public void export_doc_images_type(Document doc, PDFStream profileStream, ImageType imtype)
        {
            exporttype = imtype;
            ColorSpace cs = new ICCBasedColorSpace(profileStream);
            for (int pgno = 0; pgno < doc.NumPages; pgno++)
            {
                Page pg = doc.GetPage(pgno);

                Export_Image(pg.Content, cs, pg, pgno);
            }
        }

        public void export_doc_images(Document doc, PDFStream profileStream)
        {
            export_doc_images_type(doc, profileStream, ImageType.TIFF);
        }

        public void Export_Image(Content content, ColorSpace csp, Page pg, int pNum)
        {
            ImageSaveParams isp;

            try
            {
                isp = new ImageSaveParams();
                isp.Compression = CompressionCode.LZW;

                PageImageParams pip = new PageImageParams();
                pip.ImageColorSpace = csp;

                Datalogics.PDFL.Image outImage = pg.GetImage(pg.CropBox, pip);

                String filenamevar = "";

                pip.RenderIntent = RenderIntent.Saturation;
                outImage = pg.GetImage(pg.CropBox, pip);
                filenamevar = "ImageEmbedICCProfile-out_sat" + pNum + ".tif";
                outImage.Save(filenamevar, exporttype, isp);

                pip.RenderIntent = RenderIntent.AbsColorimetric;
                outImage = pg.GetImage(pg.CropBox, pip);
                filenamevar = "ImageEmbedICCProfile-out_abs" + pNum + ".tif";
                outImage.Save(filenamevar, exporttype, isp);

                pip.RenderIntent = RenderIntent.Perceptual;
                outImage = pg.GetImage(pg.CropBox, pip);
                filenamevar = "ImageEmbedICCProfile-out_per" + pNum + ".tif";
                outImage.Save(filenamevar, exporttype, isp);

                pip.RenderIntent = RenderIntent.RelColorimetric;
                outImage = pg.GetImage(pg.CropBox, pip);
                filenamevar = "ImageEmbedICCProfile-out_rel" + pNum + ".tif";
                outImage.Save(filenamevar, exporttype, isp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot write file: " + ex.Message);
            }
        }
    }

    public class ImageEmbedICCProfile
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Image Embed ICC Profile sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String profileName = Library.ResourceDirectory + "Sample_Input/Probev1_ICCv2.icc";

                Console.WriteLine("Input file: " + sInput + " will have profile " + profileName + " applied.");

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    profileName = args[1];

                Document doc = new Document(sInput);

                ExportDocumentImages expdoc = new ExportDocumentImages();

                FileStream profileFileStream = new FileStream(profileName, FileMode.Open);
                PDFStream profilePDFStream = new PDFStream(profileFileStream, doc, null, null);

                expdoc.export_doc_images(doc, profilePDFStream);
            }
        }
    }
}
