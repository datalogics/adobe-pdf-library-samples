using System;
using System.Collections.Generic;
using System.IO;
using Datalogics.PDFL;
using ExtractTextNameSpace;

/*
 * This sample processes PDF files in a folder and extracts text from specific regions
 * of its pages and saves the text to a CSV file.
 * 
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ExtractTextFromMultiRegions
{
    class ExtractTextFromMultiRegions
    {
        // Set Defaults
        static String sInput = Library.ResourceDirectory + "Sample_Input/ExtractTextFromMultiRegions";
        static String sOutput = "../ExtractTextFromMultiRegions-out.csv";

        // Rectangular regions to extract text in points (origin of the page is bottom left)
        // (Left, Right, Bottom, Top)
        static double[] invoice_number = { 500, 590, 692, 710 };
        static double[] date = { 500, 590, 680, 700 };
        static double[] order_number = { 500, 590, 672, 688 };
        static double[] cust_id = { 500, 590, 636, 654 };
        static double[] total = { 500, 590, 52, 73 };

        static List<double[]> regions = new List<double[]>
        {
            invoice_number,
            date,
            order_number,
            cust_id,
            total
        };
        enum quadSide { left, right, bottom, top };

        static void Main(string[] args)
        {
            Console.WriteLine("ExtractTextFromMultiRegions Sample:");


            using (new Library())
            {
                Console.WriteLine("Initialized the library.");
                using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(sOutput))
                {
                    Console.WriteLine("Writing to: " + sOutput);
                    // Print out a heading for CSV file
                    outfile.WriteLine("Filename,Invoice Number,Date,Order Number,Customer ID,Total");

                    string[] pdfFiles = Directory.GetFiles(sInput);

                    foreach (string fileName in pdfFiles)
                    {
                        using (Document doc = new Document(fileName))
                        {
                            outfile.Write(System.IO.Path.GetFileName(fileName));
                            Console.WriteLine("Input file: " + System.IO.Path.GetFileName(fileName));

                            using (ExtractText docText = new ExtractText(doc))
                            {
                                for (int pageNum = 0; pageNum < doc.NumPages; ++pageNum)
                                {
                                    List<TextAndDetailsObject> result = docText.GetTextAndDetails(pageNum);
                                    foreach (double[] region in regions)
                                    {
                                        outfile.Write(",");
                                        foreach (TextAndDetailsObject textInfo in result)
                                        {
                                            bool allQuadsWithinRegion = true;
                                            // A Word typically has only 1 quad, but can have more than one
                                            // for hyphenated words, words on a curve, etc.
                                            foreach (Quad quad in textInfo.Quads)
                                            {
                                                if (!CheckWithinRegion(quad, region))
                                                {
                                                    allQuadsWithinRegion = false;
                                                    break;
                                                }
                                            }
                                            if (allQuadsWithinRegion)
                                            {
                                                // Put this Word that meets our criteria in the output document
                                                outfile.Write(textInfo.Text);
                                            }
                                        }
                                    }
                                    outfile.WriteLine();
                                }
                            }
                        }
                    }
                }
            }
        }
        // For this sample, we will consider a Word to be in the region of interest if the
        // complete Word fits within the specified rectangular box
        static bool CheckWithinRegion(Quad wordQuad, double[] region)
        {
            if (wordQuad.BottomLeft.H >= region[(int)quadSide.left] && wordQuad.BottomRight.H <= region[(int)quadSide.right] &&
                wordQuad.TopLeft.H >= region[(int)quadSide.left] && wordQuad.TopRight.H <= region[(int)quadSide.right] &&
                wordQuad.BottomLeft.V >= region[(int)quadSide.bottom] && wordQuad.TopLeft.V <= region[(int)quadSide.top] &&
                wordQuad.BottomRight.V >= region[(int)quadSide.bottom] && wordQuad.TopRight.V <= region[(int)quadSide.top])
            {
                return true;
            }
            return false;
        }
    }
}

