using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * Process a document using the optical recognition engine.
 * Then place the image and the processed text in an output pdf
 * 
 * For more detail see the description of AddTextToDocument on our Developers site, 
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/optical-character-recognition/
 * 
 * Copyright (c) 2007-2019, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace AddTextToDocument
{
    class AddTextToDocument
    {
        //This function will find every image in the document and add text if 
        //possible
        static void AddTextToImages(Document doc, Content content, OCREngine engine)
        {
            for (int index = 0; index < content.NumElements; index++)
            {
                Element e = content.GetElement(index);
                if (e is Datalogics.PDFL.Image)
                {
                    //PlaceTextUnder creates a form with the image and the generated text
                    //under the image. The original image in the page is then replaced by
                    //by the form.
                    Form form = engine.PlaceTextUnder((Image)e, doc);
                    content.RemoveElement(index);
                    content.AddElement(form, index -1);
                }
                else if (e is Container)
                {
                    AddTextToImages(doc, (e as Container).Content, engine);
                }
                else if (e is Group)
                {
                    AddTextToImages(doc, (e as Group).Content, engine);
                }
                else if (e is Form)
                {
                    AddTextToImages(doc, (e as Form).Content, engine);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("AddTextToDocument Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/scanned_images.pdf";
                String sOutput = "../AddTextToDocument-out.pdf";

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
                    //Create a document object using the input file
                    using (Document doc = new Document(sInput))
                    {
                        for (int numPage = 0; numPage < doc.NumPages; numPage++)
                        {
                            using (Page page = doc.GetPage(numPage))
                            {
                                Content content = page.Content;
                                Console.WriteLine("Adding text to page: " + numPage);
                                AddTextToImages(doc, content, ocrEngine);
                                page.UpdateContent();
                            }
                        }
                        doc.Save(SaveFlags.Full, sOutput);
                    }
                }
            }
        }

    }
}
