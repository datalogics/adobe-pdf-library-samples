using System;
using System.Collections.Generic;
using Datalogics.PDFL;
using ExtractTextNameSpace;

/*
 * This sample extracts text from a specific target area of a page in a PDF
 * document and saves the text to a file.
 *
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ExtractTextByRegion
{
    class ExtractTextByRegion
    {
        // Set Defaults
        static String sInput = Library.ResourceDirectory + "Sample_Input/ExtractTextByRegion.pdf";
        static String sOutput = "../ExtractTextByRegion-out.txt";

        // Rectangular region to extract text in points (origin of the page is bottom left)
        // (545,576,694,710) is a rectangle encompassing the invoice entry for this sample.
        static double userTargetRegionLeft = 545;    // left
        static double userTargetRegionRight = 576;   // right
        static double userTargetRegionBottom = 694;  // bottom
        static double userTargetRegionTop = 710;     // top

        static void Main(string[] args)
        {
            Console.WriteLine("ExtractTextByRegion Sample:");


            using (new Library())
            {
                Console.WriteLine("Initialized the library.");

                using (Document doc = new Document(sInput))
                {
                    Console.WriteLine("Input file: " + sInput);

                    using (ExtractText docText = new ExtractText(doc))
                    {
                        List<TextAndDetailsObject> result = docText.GetTextAndDetails();

                        using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(sOutput))
                        {
                            Console.WriteLine("Writing to: " + sOutput);

                            foreach (TextAndDetailsObject textInfo in result)
                            {
                                bool allQuadsWithinRegion = true;
                                // A Word typically has only 1 quad, but can have more than one
                                // for hyphenated words, words on a curve, etc.
                                foreach (Quad quad in textInfo.Quads)
                                {
                                    if (!CheckWithinRegion(quad))
                                    {
                                        allQuadsWithinRegion = false;
                                    }
                                }
                                if (allQuadsWithinRegion)
                                {
                                    // Put this Word that meets our criteria in the output document
                                    outfile.WriteLine(textInfo.Text);
                                }
                            }
                        }
                    }
                }
            }
        }
        // For this sample, we will consider a Word to be in the region of interest if the
        // complete Word fits within the specified rectangular box
        static bool CheckWithinRegion(Quad wordQuad)
        {
            if (wordQuad.BottomLeft.H >= userTargetRegionLeft && wordQuad.BottomRight.H <= userTargetRegionRight &&
                wordQuad.TopLeft.H >= userTargetRegionLeft && wordQuad.TopRight.H <= userTargetRegionRight &&
                wordQuad.BottomLeft.V >= userTargetRegionBottom && wordQuad.TopLeft.V <= userTargetRegionTop &&
                wordQuad.BottomRight.V >= userTargetRegionBottom && wordQuad.TopRight.V <= userTargetRegionTop)
            {
                return true;
            }
            return false;
        }
    }
}

