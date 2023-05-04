using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * This sample lists the text for the words in a PDF document.
 * 
 * For more detail see the description of the List sample programs, and ListWords, on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 * 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ListWords
{
    class ListWords
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ListWords Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);

                int nPages = doc.NumPages;

                WordFinderConfig wordConfig = new WordFinderConfig();
                wordConfig.IgnoreCharGaps = true;
                wordConfig.IgnoreLineGaps = false;
                wordConfig.NoAnnots = true;
                wordConfig.NoEncodingGuess = true; // leave non-Roman single-byte font alone

                // Std Roman treatment for custom encoding; overrides the noEncodingGuess option
                wordConfig.UnknownToStdEnc = false;

                wordConfig.DisableTaggedPDF = true; // legacy mode WordFinder creation
                wordConfig.NoXYSort = false;
                wordConfig.PreserveSpaces = false;
                wordConfig.NoLigatureExp = false;
                wordConfig.NoHyphenDetection = false;
                wordConfig.TrustNBSpace = false;
                wordConfig.NoExtCharOffset = false; // text extraction efficiency
                wordConfig.NoStyleInfo = false; // text extraction efficiency

                WordFinder wordFinder = new WordFinder(doc, WordFinderVersion.Latest, wordConfig);
                IList<Word> pageWords = null;
                for (int i = 0; i < nPages; i++)
                {
                    pageWords = wordFinder.GetWordList(i);
                    foreach (Word wInfo in pageWords)
                    {
                        string s = wInfo.Text;
                        IList<Quad> QuadList = wInfo.Quads;

                        foreach (Quad Q in QuadList)
                        {
                            Console.WriteLine(Q);
                        }

                        foreach (StyleTransition st in wInfo.StyleTransitions)
                        {
                            Console.WriteLine(st);
                        }

                        IList<StyleTransition> styleList = wInfo.StyleTransitions;
                        foreach (StyleTransition st in styleList)
                        {
                            Console.WriteLine(st);
                        }

                        Console.WriteLine(wInfo.Attributes);
                        Console.WriteLine(s);
                    }
                }

                Console.WriteLine("Pages=" + nPages);
            }
        }
    }
}
