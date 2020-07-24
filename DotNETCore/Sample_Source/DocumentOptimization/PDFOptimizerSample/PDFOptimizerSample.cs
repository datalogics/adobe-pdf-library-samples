using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * 
 * Use the PDFOptimizerSample to experiment with the PDF Optimization feature in the Adobe PDF Library.
 * 
 * PDF Optimization allows you to compress a PDF document to make it smaller. Besides being easier to manage,
 * an optimized PDF document tends to load faster when opened in a web browser. The PDF Optimizer in the Library
 * is designed to work in a similar way as a feature in Adobe Acrobat that can save a PDF document to optimize
 * that file, when selecting Save As.
 * 
 * For more detail see the description of the PDFOptimizerSample sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/using-the-pdf-optimizer-to-manage-the-size-of-pdf-documents
 * 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace PDFOptimizerSample
{
    class Optimize
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PDF Optimization Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf"; ;
                String sOutput = "../PDFOptimizer-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("input file: " + sInput + ". Writing to output " + sOutput);

                Document doc = new Document(sInput);

                using (PDFOptimizer opt = new PDFOptimizer())
                {
                    long beforeLength = new System.IO.FileInfo(sInput).Length;

                    bool linearizeBefore = opt.GetOption(OptimizerOption.Linearize);
                    opt.SetOption(OptimizerOption.Linearize, true);
                    bool linearizeAfter = opt.GetOption(OptimizerOption.Linearize);
                    if (linearizeBefore != linearizeAfter)
                        Console.WriteLine("Successfully set PDF Option Linearize to ON.");
                    else
                        Console.WriteLine("Failed to set PDF Option Linearize to ON!");

                    opt.Optimize(doc, sOutput);

                    long afterLength = new System.IO.FileInfo(sOutput).Length;

                    Console.Write("Optimized file ");
                    Console.Write(afterLength * 100.0 / beforeLength);
                    Console.Write("% the size of the original.");
                    Console.Out.Flush();
                }
            }
        }
    }
}
