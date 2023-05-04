using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates working with page labels in a PDF document. Each PDF file has a 
 * data structure that governs how page numbers appear, such as the font and type of numeral.
 *
 * For more detail see the description of the PageLabels sample on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/listing-information-about-values-and-objects-in-pdf-files#pagelabels
 * 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PageLabels
{
    class PageLabels
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Page Labels Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/pagelabels.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file " + sInput);

                Document doc = new Document(sInput);

                // Extract a page label from the document
                String labelString = doc.FindLabelForPageNum(doc.NumPages - 1);
                Console.WriteLine("Last page in the document is labeled " + labelString);

                // Index will equal (doc.NumPages - 1)
                int index = doc.FindPageNumForLabel(labelString);
                Console.WriteLine(labelString + " has an index of " + index + " in the document.");

                // Add a new page to the document
                // It will start on page 5 and have numbers of the style A-i, A-ii, etc.
                PageLabel pl = new PageLabel(5, NumberStyle.RomanLowercase, "A-", 1);

                IList<PageLabel> labels = doc.PageLabels;
                labels.Add(pl);
                doc.PageLabels = labels;

                Console.WriteLine("Added page range starting on page 5.");

                // Change the properties of the third page range
                labels = doc.PageLabels; // Get a freshly sorted list
                labels[2].Prefix = "Section 3-";
                labels[2].FirstNumberInRange = 2;
                doc.PageLabels = labels;

                Console.WriteLine("Changed the prefix for the third range.");

                // Now walk the list of page labels
                foreach (PageLabel label in doc.PageLabels)
                {
                    Console.WriteLine("Label range starts on page " + label.StartPageIndex
                                                                    + ", ends on page " + label.EndPageIndex);
                    Console.WriteLine("The prefix is '" + label.Prefix
                                                        + "' and begins with number " + label.FirstNumberInRange);
                    Console.WriteLine();
                }
            }
        }
    }
}
