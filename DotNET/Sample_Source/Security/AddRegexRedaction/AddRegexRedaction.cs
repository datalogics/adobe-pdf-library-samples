using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * This sample demonstrates using the DocTextFinder to find examples of a specific phrase in a PDF
 * document that matches a user-supplied regular expression. When the sample finds the text it
 * will redact the phrase from the output document.
 * 
 * Copyright (c) 2007-2021, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace AddRegexRedaction
{
    class AddRegexRedaction
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AddRegexRedaction Sample:");


            using (new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/AddRegexRedaction.pdf";
                String sOutput1 = "AddRegexRedaction-out.pdf";
                String sOutput2 = "AddRegexRedaction-out-applied.pdf";

                // Highlight and redact occurrences of the phrases that match this regular expression.
                // Uncomment only the one you are interested in seeing displayed redacted.
                // Phone numbers
                String sRegex = "((1-)?(\\()?\\d{3}(\\))?(\\s)?(-)?\\d{3}-\\d{4})";
                // Email addresses
                //String sRegex = "(\\b[\\w.!#$%&'*+\\/=?^`{|}~-]+@[\\w-]+(?:\\.[\\w-]+)*\\b)";
                // URLs
                //String sRegex = "((https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,}))";

                if (args.Length > 0)
                    sInput = args[0];

                using (Document doc = new Document(sInput))
                {
                    int nPages = doc.NumPages;

                    Console.WriteLine("Input file:  " + sInput);

                    // Create a WordFinder configuration
                    WordFinderConfig wordConfig = new WordFinderConfig();

                    // Need to set this to true so phrases will be concatenated properly
                    wordConfig.NoHyphenDetection = true;

                    // Create a DocTextFinder with the default wordfinder parameters
                    using (DocTextFinder docTextFinder =
                        new DocTextFinder(doc, wordConfig))
                    {
                        // Retrieve the phrases and words matching a regular expression
                        IList<DocTextFinderMatch> docMatches =
                            docTextFinder.GetMatchList(0, nPages - 1, sRegex);

                        // Redaction color will be red
                        Color red = new Color(1.0, 0.0, 0.0);

                        foreach (DocTextFinderMatch wInfo in docMatches)
                        {
                            // Show the matching phrase
                            Console.WriteLine(wInfo.MatchString);

                            // Get the quads
                            IList<DocTextFinderQuadInfo> QuadInfo = wInfo.QuadInfo;

                            // Iterate through the quad info and create highlights
                            foreach (DocTextFinderQuadInfo qInfo in QuadInfo)
                            {
                                Page docpage = doc.GetPage(qInfo.PageNum);

                                Redaction red_fill = new Redaction(docpage, qInfo.Quads, red);

                                /* fill the "normal" appearance with 25% red */
                                red_fill.FillNormal = true;
                                red_fill.SetFillColor(red, 0.25);
                            }
                        }
                    }
                    // Save the document with the highlighted matched strings
                    doc.Save(SaveFlags.Full, sOutput1);

                    Console.WriteLine("Wrote a PDF document with unapplied redactions.");

                    // Apply all the redactions in the document
                    doc.ApplyRedactions();

                    // Save the document with the redacted matched strings
                    doc.Save(SaveFlags.Full, sOutput2);

                    Console.WriteLine("Wrote a redacted PDF document.");
                }
            }
        }
    }
}
