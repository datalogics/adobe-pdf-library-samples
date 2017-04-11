package com.datalogics.PDFL.Samples;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.EnumSet;

import com.datalogics.PDFL.*;

/*
 * 
 * This sample demonstrates merging one PDF document into another. The program
 * offers two optional input documents and defines a default output document. The sample
 * takes the content from the second PDF input document and inserts it in the first
 * input document, and saves the result to the output PDF document.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class MergePDF {
    public static void main (String[] args) throws Throwable
    {
        System.out.println("MergePDF sample:");

        Library lib = new Library();

        String sInput1 = "../../Resources/Sample_Input/merge_pdf1.pdf";
        String sInput2 = "../../Resources/Sample_Input/merge_pdf2.pdf";
        String sOutput = "MergePDF-out.pdf";

        if (args.length > 0)
            sInput1 = args[0];
        if (args.length > 1)
            sInput2 = args[1];
        if (args.length > 2)
            sOutput = args[2];

        System.out.println("MergePDF: adding " + sInput1 + " and " + sInput2
            + " and writing to " + sOutput );

        Document doc1 = new Document(sInput1);
        Document doc2 = new Document(sInput2);

        doc1.insertPages(Document.LAST_PAGE, doc2, 0, Document.ALL_PAGES, EnumSet.of(PageInsertFlags.ALL));
        doc1.save(EnumSet.of(SaveFlags.FULL, SaveFlags.LINEARIZED), sOutput);

        doc1.close();
        doc2.close();

        // this properly terminates the library, and is required
        lib.delete();
    }
};
