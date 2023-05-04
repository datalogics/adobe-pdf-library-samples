using System;
using Datalogics.PDFL;

/*
 * 
 * This sample shows how to list metadata about a PDF document, and presents command prompts
 * if you want to change these values, such as the title or author. The results are exported
 * to a PDF output document.
 * 
 * For more detail see the description of the List sample programs, and ListInfo, on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ListInfo
{
    class ListInfo
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ListInfo Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sOutput = "ListInfo-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ". Writing to output " + sOutput);

                Document doc = new Document(sInput);

                Console.WriteLine("Document Title " + doc.Title);
                String newTitle = "The Sciences";
                Console.WriteLine("Changed document title to: " + newTitle);

                Console.WriteLine("Document Subject " + doc.Subject);
                String newSubject = "Biology";
                Console.WriteLine("Changed document subject to: " + newSubject);

                Console.WriteLine("Document Author " + doc.Author);
                String newAuthor = "John Smith";
                Console.WriteLine("Changed document author to: " + newAuthor);


                Console.WriteLine("Document Keywords '" + doc.Keywords + "'");
                String newKeywords = "Sciences";
                Console.WriteLine("Changed document keywords to: " + newKeywords);

                Console.WriteLine("Document Creator " + doc.Creator);
                String newCreator = "Charles Darwin";
                Console.WriteLine("Changed document creator to: " + newCreator);


                Console.WriteLine("Document Producer " + doc.Producer);
                String newProducer = "Datalogics, Inc";
                Console.WriteLine("Changed document producer to: " + newProducer);

                doc.Title = newTitle;
                doc.Subject = newSubject;
                doc.Author = newAuthor;
                doc.Keywords = newKeywords;
                doc.Creator = newCreator;
                doc.Producer = newProducer;

                doc.Save(SaveFlags.Full | SaveFlags.Linearized, sOutput);
            }
        }
    }
}
