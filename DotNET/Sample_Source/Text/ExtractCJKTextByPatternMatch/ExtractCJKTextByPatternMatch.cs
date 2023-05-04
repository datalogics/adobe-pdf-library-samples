using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * This sample extracts text that matches a given CJK pattern in a PDF
 * document and saves the text to a file.
 *
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ExtractCJKTextByPatternMatch
{
    class ExtractCJKTextByPatternMatch
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ExtractCJKTextByPatternMatch Sample:");


            using (new Library())
            {
                Console.WriteLine("Initialized the library.");

                // Set Defaults
                String sInput = Library.ResourceDirectory + "Sample_Input/ExtractUnicodeText.pdf";
                String sOutput = "../ExtractCJKTextByPatternMatch-out.txt";
                String sPattern = "『世界人権宣言』";

                using (Document doc = new Document(sInput))
                {
                    int nPages = doc.NumPages;

                    Console.WriteLine("Input file:  " + sInput);

                    using (WordFinderConfig wordConfig = new WordFinderConfig())
                    {
                        // DocTextFinder creates a string of characters to search.
                        // This string may contain hyphens and the regular expression
                        // may contain hyphens.  So we do not want the wordfinder to
                        // alter that or else the search may fail.
                        wordConfig.NoHyphenDetection = true;

                        using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(sOutput))
                        {
                            Console.WriteLine("Writing to: " + sOutput);

                            // Create a DocTextFinder with the wordfinder.
                            using (DocTextFinder docTextFinder =
                                new DocTextFinder(doc, wordConfig))
                            {
                                // Retrieve the phrases matching the pattern.
                                IList<DocTextFinderMatch> docMatches =
                                    docTextFinder.GetMatchList(0, nPages - 1, sPattern);

                                // Iterate through the matches and write them out to the file.
                                foreach (DocTextFinderMatch match in docMatches)
                                {
                                    outfile.WriteLine(match.MatchString);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

