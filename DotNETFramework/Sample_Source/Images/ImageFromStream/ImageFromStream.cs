using System;
using System.IO;
using System.Drawing;
using Datalogics.PDFL;

/*
 * 
 * This sample program searches through the PDF file that you select and identifies drawings,
 * diagrams and photographs from the System.IO.Streams. 
 * 
 * A stream is a string of bytes of any length, embedded in a PDF document with a dictionary
 * that is used to interpret the values in the stream.
 * 
 * This program is similar to StreamIO.
 *
 * For more detail see the description of the ImagefromStream sample program on our Developer�s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/exporting-images-from-pdf-files/#imagefromstream
 * 
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ImageFromStream
{
    class ImageFromStream
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ImageFromStream Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String bitmapInput = Library.ResourceDirectory + "Sample_Input/Datalogics.bmp";
                String jpegInput = Library.ResourceDirectory + "Sample_Input/ducky.jpg";
                String docOutput = "../ImageFromStream-out2.pdf";

                if (args.Length > 0)
                    bitmapInput = args[0];

                if (args.Length > 1)
                    jpegInput = args[1];

                if (args.Length > 2)
                    docOutput = args[2];

                Console.WriteLine("using bitmap input " + bitmapInput + " and jpeg input " + jpegInput + ". Writing to output " + docOutput);

                // Create a .NET MemoryStream object.
                // A MemoryStream is used here for demonstration only, but the technique
                // works just as well for other streams which support seeking.
                System.IO.MemoryStream BitmapStream = new System.IO.MemoryStream();

                // Load a bitmap image into the MemoryStream.
                System.Drawing.Bitmap BitmapImage = new System.Drawing.Bitmap(bitmapInput);
                BitmapImage.Save(BitmapStream, System.Drawing.Imaging.ImageFormat.Bmp);

                // Reset the MemoryStream's seek position before handing it to the DLE API, 
                // which expects the seek position to be at the beginning of the stream.
                BitmapStream.Seek(0, System.IO.SeekOrigin.Begin);

                // Create the DLE Image object.
                Datalogics.PDFL.Image DLEBitmapImage = new Datalogics.PDFL.Image(BitmapStream);

                // Save the image to a PNG file.
                DLEBitmapImage.Save("ImageFromStream-out.png", ImageType.PNG);

                // The following demonstrates reading an image from a Stream and placing it into a document.
                // First, create a new Document and add a Page to it.
                Document doc = new Document();
                doc.CreatePage(Document.BeforeFirstPage, new Rect(0, 0, 612, 792));

                // Create a new MemoryStream for a new image file.
                System.IO.MemoryStream JpegStream = new System.IO.MemoryStream();

                // Load a JPEG image into the MemoryStream.
                System.Drawing.Image JpegImage = System.Drawing.Image.FromFile(jpegInput);
                JpegImage.Save(JpegStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                // An alternative method for resetting the MemoryStream's seek position.
                JpegStream.Position = 0;

                // Create the DLE Image object and put it in the Document.
                // Since the image will be placed in a Document, use the constructor with the optional 
                // Document parameter to optimize data usage for this image within the Document.
                Datalogics.PDFL.Image DLEJpegImage = new Datalogics.PDFL.Image(JpegStream, doc);
                Page pg = doc.GetPage(0);
                pg.Content.AddElement(DLEJpegImage);
                pg.UpdateContent();
                doc.Save(SaveFlags.Full, docOutput);
            }
        }
    }
}
