using Datalogics.PDFL;
using System;

/*
 *
 * This sample program demonstrates the use of PDFOptimizer. This compresses a PDF document
 * to make it smaller so it's easier to process and download.
 *
 * NOTE: Some documents can't be compressed because they're already well-compressed or contain
 * content that can't be assumed is safe to be removed.  However you can fine tune the optimization
 * to suit your applications needs and drop such content to achieve better compression if you already
 * know it's unnecessary.
 * 
 * For more detail see the description of the PDFOptimizer sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/using-the-pdf-optimizer-to-manage-the-size-of-pdf-documents
 * 
 * 
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace PDFOptimize
{
    class Optimize
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PDF Optimizer:");


            using (new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf"; ;
                String sOutput = "../PDFOptimizer-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput);
                Console.WriteLine("Writing to output " + sOutput);

                using (Document doc = new Document(sInput))
                {
                    using (Datalogics.PDFL.PDFOptimizer optimizer = new Datalogics.PDFL.PDFOptimizer())
                    {
                        long beforeLength = new System.IO.FileInfo(sInput).Length;

                        optimizer.Optimize(doc, sOutput);

                        long afterLength = new System.IO.FileInfo(sOutput).Length;

                        Console.WriteLine();
                        Console.WriteLine("Optimized file: {0:P2} the size of the original.", (double)afterLength / beforeLength);
                    }
                }
            }
        }
    }
}
