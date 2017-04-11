package com.datalogics.PDFL.Samples;

import java.io.File;
import java.io.IOException;
import java.io.BufferedReader;
import java.io.InputStreamReader;


import com.datalogics.PDFL.*;
/*
 * 
 * Use the PDFOptimizerSample to experiment with the PDF Optimization feature in the Adobe PDF Library.
 * 
 * PDF Optimization allows you to compress a PDF document to make it smaller. Besides being easier to manage,
 * an optimized PDF document tends to load faster when opened in a web browser. The PDF Optimizer in the Library
 * is designed to work in a similar way as a feature in Adobe Acrobat that can save a PDF document to optimize
 * that file, when selecting Save As.
 * 
 * For more detail see the description of the PDFOptimizerSample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/using-the-pdf-optimizer-to-manage-the-size-of-pdf-documents
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class PDFOptimizerSample {

    /**
     * @param args
     */
    public static void main(String[] args) throws Exception {
        System.out.println("PDFOptimizer sample:");

        Library lib = new Library();
        try {

            String sInput = "../../Resources/Sample_Input/sample.pdf";
            String sOutput = "PDFOptimizer-out.pdf";
            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1 )
                sOutput = args[1];

            System.out.println ( "Will optimize " + sInput + " and save as " + sOutput );
            Document doc = new Document(sInput);
            PDFOptimizer opt = new PDFOptimizer();
            try {

                File oldfile = new File(sInput);
                long beforeLength = oldfile.length();

                boolean linearizeBefore = opt.getOption(OptimizerOption.LINEARIZE);
                opt.setOption(OptimizerOption.LINEARIZE, true);
                boolean linearizeAfter = opt.getOption(OptimizerOption.LINEARIZE);
                if (linearizeBefore != linearizeAfter)
                    System.out.println("Successfully set PDF Option Linearize to ON.");
                else
                    System.out.println("Failed to set PDF Option Linearize to ON!");



                opt.optimize(doc, sOutput);

                File newfile = new File(sOutput);
                long afterLength = newfile.length();

                System.out.print("Optimized file ");
                System.out.print(afterLength * 100.0 / beforeLength);
                System.out.print("% the size of the original.");
                System.out.flush();

            }
            finally
            {
                opt.delete();
            }

        } 
        finally
        {
            lib.delete();
        }

    }

}
