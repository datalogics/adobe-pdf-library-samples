using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * Process a document using the optical recognition engine.
 * Then place the image and the processed text in an output pdf
 * 
 * For more detail see the description of AddTextToDocument on our Developers site, 
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/optical-character-recognition/
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
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
                    Form form = engine.PlaceTextUnder((Datalogics.PDFL.Image) e, doc);
                    content.RemoveElement(index);
                    content.AddElement(form, index - 1);
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


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/scanned_images.pdf";
                String sOutput = "AddTextToDocument-out.pdf";

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

                // If the resolution for the images in your document are not
                // 300 dpi, specify a default resolution here. Specifying a
                // correct resolution gives better results for OCR, especially
                // with automatic image preprocessing.
                // ocrParams.Resolution = 600;

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
