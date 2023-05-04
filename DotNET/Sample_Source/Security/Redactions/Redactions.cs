using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * This sample shows how to redact a PDF document. The program opens an input PDF, searches for
 * specific words using the Adobe PDF Library WordFinder, and then removes these words from the text.
 * 
 * For more detail see the description of the Redactions sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/redacting-text-from-a-pdf-document
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace Redactions
{
    class Redactions
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Redactions Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");
                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sOutput1 = "Redactions-out.pdf";
                String sOutput2 = "Redactions-out-applied.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);

                Page docpage = doc.GetPage(0);
                //
                // Redact occurrences of the word "rain" on the page.
                // Redact occurrences of the word "cloudy" on the page, changing the display details.
                //
                // For a more in-depth example of using the WordFinder, see the TextExtract sample.
                //
                // The TextExtract sample is described here.
                // http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/extracting-text-from-pdf-files
                //

                List<Quad> cloudyQuads = new List<Quad>();

                List<Quad> rainQuads = new List<Quad>();

                WordFinderConfig wordConfig = new WordFinderConfig();
                WordFinder wf = new WordFinder(doc, WordFinderVersion.Latest, wordConfig);

                IList<Word> words = wf.GetWordList(docpage.PageNumber);

                foreach (Word w in words)
                {
                    Console.WriteLine(" " + w.Text.ToLower());
                    // Store the Quads of all "Cloudy" words in a list for later use in
                    // creating the redaction object.
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

                Console.WriteLine("Found Cloudy instances: " + cloudyQuads.Count);
                Color red = new Color(1.0, 0.0, 0.0);
                Color white = new Color(1.0);

                Redaction not_cloudy = new Redaction(docpage, cloudyQuads, red);

                /* fill the "normal" appearance with 20% red */
                not_cloudy.FillNormal = true;
                not_cloudy.SetFillColor(red, 0.25);

                Console.WriteLine("Found rain instances: " + rainQuads.Count);
                Redaction no_rain = new Redaction(docpage, rainQuads);
                no_rain.InternalColor = new Color(0.0, 1.0, 0.0);

                /* Fill the redaction with the word "rain", drawn in white */
                no_rain.OverlayText = "rain";
                no_rain.Repeat = true;
                no_rain.ScaleToFit = true;
                no_rain.TextColor = white;
                no_rain.FontFace = "CourierStd";
                no_rain.FontSize = 8.0;

                doc.Save(SaveFlags.Full, sOutput1);

                Console.WriteLine("Wrote a pdf doc with unapplied redactions.");

                // actually all the redactions in the document
                doc.ApplyRedactions();

                doc.Save(SaveFlags.Full, sOutput2);

                Console.WriteLine("Wrote a redacted pdf doc.");
            }
        }
    }
}
