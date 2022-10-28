package com.datalogics.pdfl.samples.ContentModification.FlattenTransparency;

import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.FlattenTransparencyParams;

/*
 * 
 * This sample shows how to flatten transparencies in a PDF document.
 *
 * PDF files can have objects that are partially or fully transparent, and thus
 * can blend in various ways with objects behind them. Transparent graphics or images
 * can be stacked in a PDF file, with each one contributing to the final result that
 * appears on the page. The process to flatten a set of transparencies merges them
 * into a single image on the page.
 *
 * For more detail see the description of the FlattenTransparency sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/layers-and-transparencies/#flattentransparency 
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class FlattenTransparency {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("FlattenTransparency sample:");

        String sInput1 = Library.getResourceDirectory() + "Sample_Input/trans_1page.pdf";
        String sInput2 = Library.getResourceDirectory() + "Sample_Input/trans_multipage.pdf";
        String sOutput1 = "FlattenTransparency-out1.pdf";
        String sOutput2 = "FlattenTransparency-out2.pdf";
        if (args.length > 0)
            sInput1 = args[0];
        if (args.length > 1)
            sInput2 = args[1];
        if (args.length > 2)
            sOutput1 = args[2];
        if (args.length > 3)
            sOutput2 = args[3];
    	Library lib = new Library();

		try {
            // Open a document with a single page.
	        Document doc1 = new Document(sInput1);
	        
            // Verify that the page has transparency.  The parameter indicates
            // whether to include the appearances of annotations or not when
            // checking for transparency.
            Page pg1 = doc1.getPage(0);
            Boolean isTransparent = pg1.hasTransparency(true);

            // If there is transparency, flatten the document.
            if(isTransparent)
            {
                // Flattening the document will check each page for transparency.
                // If a page has transparency, DLE will create a new, flattened
                // version of the page and replace the original page with the
                // new one.  Because of this, make sure to dispose of outstanding Page objects
                // that refer to pages in the Document before calling flattenTransparency.
                pg1.delete();
                
                doc1.flattenTransparency();
                System.out.println("Flattened single page document " + sInput1 + " as " + sOutput1 + ".");
                doc1.save(EnumSet.of(SaveFlags.FULL), sOutput1);                
            }
            
            // Open a document with multiple pages.
            Document doc2 = new Document(sInput2);
            
            // Iterate over the pages of the document and find the first page that has
            // transparency.
            isTransparent = false;
            int totalPages = doc2.getNumPages();
            int pageCounter = 0;
            while(!isTransparent && pageCounter <= totalPages)
            {
                Page pg = doc2.getPage(pageCounter);
                if(pg.hasTransparency(true))
                {
                    isTransparent = true;
                    // Explicitly delete the page here, to ensure the reference is gone before we 
                    // attempt to flatten the document.  
                    pg.delete();
                    break;
                }
                pageCounter++;
            }
            
            if(isTransparent)
            {
                // Set up some parameters for the flattening.
                FlattenTransparencyParams ftParams = new FlattenTransparencyParams();

                // The Quality setting indicates the percentage (0%-100%) of vector information
                // that is preserved.  Lower values result in higher rasterization of vectors.
                ftParams.setQuality(50);
                
                // Flatten transparency in the document, starting from the first page
                // that has transparency.
                doc2.flattenTransparency(ftParams, pageCounter, Document.LAST_PAGE);
                    System.out.println("Flattened multi-page document " + sInput2 + " as " + sOutput2 + ".");
                doc2.save(EnumSet.of(SaveFlags.FULL), sOutput2);                
            }
		}
		finally {
			lib.delete();
		}
	}

}
