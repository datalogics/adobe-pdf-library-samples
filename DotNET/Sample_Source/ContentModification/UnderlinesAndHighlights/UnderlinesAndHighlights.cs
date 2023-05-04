using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 *
 * This program shows how to add annotations to an existing PDF file that will highlight and underline words.
 * When you run it, the program generates a PDF output file. The output sample annotates a PDF file showing
 * a National Weather Service web page, highlighting the word “Cloudy” wherever it appears and underlining
 * the word “Rain.”
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace UnderlinesAndHighlights
{
    public class UnderlinesAndHighlights
    {
        static void Main(string[] args)
        {
            Console.WriteLine("UnderlinesAndHighlights Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sOutput = "UnderlinesAndHighlights-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Document doc = new Document(sInput);

                Console.WriteLine("Opened a document " + sInput);

                Page docpage = doc.GetPage(0);

                //
                // Highlight occurrences of the word "cloudy" on the page.
                // Underline occurrences of the word "rain" on the page.
                //
                // For a more in-depth example of using the WordFinder, see the TextExtract sample.
                //
                List<Quad> cloudyQuads = new List<Quad>();
                List<Quad> rainQuads = new List<Quad>();
                WordFinderConfig wfc = new WordFinderConfig();
                WordFinder wf = new WordFinder(doc, WordFinderVersion.Latest, wfc);
                IList<Word> words = wf.GetWordList(docpage.PageNumber);
                foreach (Word w in words)
                {
                    // Store the Quads of all "Cloudy" words in a list for later use in
                    // creating the annotation.
                    if (w.Text.ToLower().Equals("cloudy") ||
                        ((w.Attributes & WordAttributeFlags.HasTrailingPunctuation) ==
                         WordAttributeFlags.HasTrailingPunctuation &&
                         w.Text.ToLower().StartsWith("cloudy")))
                        cloudyQuads.AddRange(w.Quads);

                    // Store the Quads of all "Rain" words
                    if (w.Text.ToLower().Equals("rain") ||
                        ((w.Attributes & WordAttributeFlags.HasTrailingPunctuation) ==
                         WordAttributeFlags.HasTrailingPunctuation &&
                         w.Text.ToLower().StartsWith("rain")))
                        rainQuads.AddRange(w.Quads);
                }

                HighlightAnnotation highlights = new HighlightAnnotation(docpage, cloudyQuads);
                highlights.Color = new Color(1.0, 0.75, 1.0);
                highlights.NormalAppearance = highlights.GenerateAppearance();

                UnderlineAnnotation underlines = new UnderlineAnnotation(docpage, rainQuads);
                underlines.Color = new Color(0.0, 0.0, 0.0);
                underlines.NormalAppearance = underlines.GenerateAppearance();

                // Read back the text that was annotated.
                Console.WriteLine("Cloudy text: {0}", highlights.GetAnnotatedText(true));
                Console.WriteLine("Rainy text: {0}", underlines.GetAnnotatedText(false));

                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
