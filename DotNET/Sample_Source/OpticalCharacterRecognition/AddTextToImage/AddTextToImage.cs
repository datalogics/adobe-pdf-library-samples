using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * The sample uses an image as input which will be processed by the optical recognition engine.
 * We will then place the image and the processed text in an output pdf
 * 
 * For more detail see the description of AddTextToImage, on our Developers site, 
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/optical-character-recognition/
 * 
 * Copyright (c) 2007-2019, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace AddTextToImage
{
    class AddTextToImage
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AddTextToImage Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/text_as_image.jpg";
                String sOutput = "../AddTextToImage-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];
                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput);
                Console.WriteLine("Writing output to: " + sOutput);


                OCRParams ocrParams = new OCRParams();
                // Setting the segmentation mode to AUTOMATIC lets the OCR engine
                // choose how to segment the page for text detection.
                ocrParams.PageSegmentationMode = PageSegmentationMode.Automatic;
                // This tells the selected engine to improve accuracy at the expense
                // of increased run time. For Tesseract 3, it runs two different
                // algorithms, and chooses the one that has the most confidence.
                ocrParams.Performance = Performance.BestAccuracy;
                using (OCREngine ocrEngine = new OCREngine(ocrParams))
                {
                    //Create a document object
                    using (Document doc = new Document())
                    {
                        using (Image newimage = new Image(sInput, doc))
                        {
                            // Create a PDF page which is the size of the image.
                            // Matrix.A and Matrix.D fields, respectively.
                            // There are 72 PDF user space units in one inch.
                            Rect pageRect = new Rect(0, 0, newimage.Matrix.A, newimage.Matrix.D);
                            using (Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect))
                            {
                                docpage.Content.AddElement(newimage);
                                docpage.UpdateContent();
                            }
                        }
                        using (Page page = doc.GetPage(0))
                        {
                                Content content = page.Content;
                                Element elem = content.GetElement(0);
                                Image image = (Image)elem;
                                //PlaceTextUnder creates a form with the image and the generated text
                                //under the image. The original image in the page is then replaced by
                                //by the form.
                                Form form = ocrEngine.PlaceTextUnder(image, doc);
                                content.RemoveElement(0);
                                content.AddElement(form, -1);
                                page.UpdateContent();
                        }
                        doc.Save(SaveFlags.Full, sOutput);
                    }
                }
            }
        }

    }
}
