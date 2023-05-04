using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * The sample uses an image as input which will be processed by the optical recognition engine.
 * We will then place the image and the processed text in an output pdf
 * 
 * For more detail see the description of AddTextToImage, on our Developers site, 
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/optical-character-recognition/
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
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


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/text_as_image.jpg";
                String sOutput = "AddTextToImage-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];
                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput);
                Console.WriteLine("Writing output to: " + sOutput);


                OCRParams ocrParams = new OCRParams();
                //The OCRParams.Languages parameter controls which languages the OCR engine attempts
                //to detect. By default the OCR engine searches for English.
                List<LanguageSetting> langList = new List<LanguageSetting>();
                LanguageSetting languageOne = new LanguageSetting(Language.English, false);
                langList.Add(languageOne);

                //You could add additional languages for the OCR engine to detect by adding 
                //more entries to the LanguageSetting list. 

                //LanguageSetting languageTwo = new LanguageSetting(Language.Japanese, false);
                //langList.Add(languageTwo);
                ocrParams.Languages = langList;

                // If your image resolution is not 300 dpi, specify it here. Specifying a
                // correct resolution gives better results for OCR, especially with
                // automatic image preprocessing.
                // ocrParams.Resolution = 600;

                using (OCREngine ocrEngine = new OCREngine(ocrParams))
                {
                    //Create a document object
                    using (Document doc = new Document())
                    {
                        using (Datalogics.PDFL.Image newimage = new Datalogics.PDFL.Image(sInput, doc))
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
                            Datalogics.PDFL.Image image = (Datalogics.PDFL.Image) elem;
                            //PlaceTextUnder creates a form with the image and the generated text
                            //under the image. The original image in the page is then replaced by
                            //by the form.
                            Form form = ocrEngine.PlaceTextUnder(image, doc);
                            content.RemoveElement(0);
                            content.AddElement(form, Content.BeforeFirst);
                            page.UpdateContent();
                        }

                        doc.Save(SaveFlags.Full, sOutput);
                    }
                }
            }
        }
    }
}
