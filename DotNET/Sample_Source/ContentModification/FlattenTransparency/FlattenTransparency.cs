using System;
using Datalogics.PDFL;

/*
 * 
 * This sample shows how to flatten transparencies in a PDF document.
 *
 * PDF files can have objects that are partially or fully transparent, and thus
 * can blend in various ways with objects behind them. Transparent graphics or images
 * can be stacked in a PDF file, with each one contributing to the final result that
 * appears on the page. The process to flatten a set of transparencies merges them
 * into a single image on the page.
 *
 * For more detail see the description of the FlattenTransparency sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/layers_and_transparencies#flattentransparency
 * 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */


namespace FlattenTransparency
{
    class FlattenTransparency
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FlattenTransparency sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sInput1 = Library.ResourceDirectory + "Sample_Input/trans_1page.pdf";
                String sOutput1 = "FlattenTransparency-out1.pdf";
                String sInput2 = Library.ResourceDirectory + "Sample_Input/trans_multipage.pdf";
                String sOutput2 = "FlattenTransparency-out2.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];
                if (args.Length > 1)
                    sInput2 = args[1];
                if (args.Length > 2)
                    sOutput1 = args[2];
                if (args.Length > 3)
                    sOutput2 = args[3];

                // Open a document with a single page.
                Document doc1 = new Document(sInput1);

                // Verify that the page has transparency.  The parameter indicates
                // whether to include the appearances of annotations or not when
                // checking for transparency.
                Page pg1 = doc1.GetPage(0);
                bool isTransparent = pg1.HasTransparency(true);

                // If there is transparency, flatten the document.
                if (isTransparent)
                {
                    // Flattening the document will check each page for transparency.
                    // If a page has transparency, PDFL will create a new, flattened
                    // version of the page and replace the original page with the
                    // new one.  Because of this, make sure to dispose of outstanding Page objects
                    // that refer to pages in the Document before calling flattenTransparency.
                    pg1.Dispose();

                    doc1.FlattenTransparency();
                    Console.WriteLine("Flattened single page document " + sInput1 + " as " + sOutput1 + ".");
                    doc1.Save(SaveFlags.Full, sOutput1);
                }

                // Open a document with multiple pages.
                Document doc2 = new Document(sInput2);

                // Iterate over the pages of the document and find the first page that has
                // transparency.
                isTransparent = false;
                int totalPages = doc2.NumPages;
                int pageCounter = 0;
                while (!isTransparent && pageCounter <= totalPages)
                {
                    Page pg = doc2.GetPage(pageCounter);
                    if (pg.HasTransparency(true))
                    {
                        isTransparent = true;
                        // Explicitly delete the page here, to ensure the reference is gone before we 
                        // attempt to flatten the document.  
                        pg.Dispose();
                        break;
                    }

                    pageCounter++;
                }

                if (isTransparent)
                {
                    // Set up some parameters for the flattening.
                    FlattenTransparencyParams ftParams = new FlattenTransparencyParams();

                    // The Quality setting indicates the percentage (0%-100%) of vector information
                    // that is preserved.  Lower values result in higher rasterization of vectors.
                    ftParams.Quality = 50;

                    // Flatten transparency in the document, starting from the first page
                    // that has transparency.
                    doc2.FlattenTransparency(ftParams, pageCounter, Document.LastPage);
                    Console.WriteLine("Flattened a multi-page document " + sInput2 + " as " + sOutput2 + ".");
                    doc2.Save(SaveFlags.Full, sOutput2);
                }
            }
        }
    }
}
