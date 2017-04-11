package com.datalogics.PDFL.Samples;

import java.util.List;

import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.PageLabel;
import com.datalogics.PDFL.NumberStyle;

/*
 * This sample demonstrates working with page labels in a PDF document. Each PDF file has a 
 * data structure that governs how page numbers appear, such as the font and type of numeral.
 *
 * For more detail see the description of the PageLabels sample on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files#pagelabels
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class PageLabels {
    public static void main(String[] args) throws Throwable
    {
        Library lib = new Library();
    	
        System.out.println("Page Labels Sample:");

        String filename = "../../Resources/Sample_Input/pagelabels.pdf";
        if ( args.length > 0 )
            filename = args[0];

        Document doc = new Document(filename);

        System.out.println("Opened document" + filename);

        // Extract a page label from the document
        String labelString = doc.findLabelForPageNum(doc.getNumPages() - 1);
        System.out.println("Last page in the document is labeled " + labelString);

        // Index will equal (doc.getNumPages() - 1)
        int index = doc.findPageNumForLabel(labelString);
        System.out.println(labelString + " has an index of " + index + " in the document.");

        // Add a new page to the document
        // It will start on page 5 and have numbers of the style A-i, A-ii, etc.
        PageLabel pl = new PageLabel(5, NumberStyle.ROMAN_LOWERCASE, "A-", 1);

        List<PageLabel> labels = doc.getPageLabels();
        labels.add(pl);
        doc.setPageLabels(labels);

        System.out.println("Added page range starting on page 5.");

        // Change the properties of the third page range
        labels = doc.getPageLabels();       // Get a freshly sorted list
        labels.get(2).setPrefix("Section 3-");
        labels.get(2).setFirstNumberInRange(2);
        doc.setPageLabels(labels);

        System.out.println("Changed the prefix for the third range.");

        // Now walk the list of page labels
        for( PageLabel label : doc.getPageLabels() )
        {
            System.out.println("Label range starts on page " + label.getStartPageIndex()
                + ", ends on page " + label.getEndPageIndex());
            System.out.println("The prefix is '" + label.getPrefix()
                + "' and begins with number " + label.getFirstNumberInRange());
            System.out.println();
        }
        
        lib.delete();
    }
}
