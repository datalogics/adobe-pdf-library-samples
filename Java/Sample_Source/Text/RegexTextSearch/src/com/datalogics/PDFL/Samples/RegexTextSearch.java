package com.datalogics.pdfl.samples.Text.RegexTextSearch;

import com.datalogics.PDFL.DocTextFinder;
import com.datalogics.PDFL.DocTextFinderMatch;
import com.datalogics.PDFL.DocTextFinderQuadInfo;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.HighlightAnnotation;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.WordFinderConfig;

import java.util.EnumSet;
import java.util.List;

/*
  * 
 * This sample shows how to search a PDF document using regex pattern matching. The program opens an input PDF, searches for
 * words using the Adobe PDF Library DocTextFinder, and then prints these words to the console.
 *
 * Copyright (c) 2021, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class RegexTextSearch {
	public static void main (String[] args) throws Throwable
    {
        System.out.println("RegexTextSearch sample:");

        Library lib = new Library();

        try {

            final String sInput = (args.length > 0) ? args[0] : Library.getResourceDirectory() + "Sample_Input/RegexTextSearch.pdf";
            final String sOutput = "RegexTextSearch-out.pdf";
            
            // Highlight occurrences of the words that match this regular expression.
            // Phone numbers
            final String sRegex = "((1-)?(\\()?\\d{3}(\\))?(\\s)?(-)?\\d{3}-\\d{4})";
            // Email addresses
            //String sRegex = "(\\b[\\w.!#$%&'*+\\/=?^`{|}~-]+@[\\w-]+(?:\\.[\\w-]+)*\\b)";
            // URLs
            //String sRegex = "((https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,}))";
            
            System.out.println("Reading " + sInput);

            Document doc = new Document(sInput);
            
            final int nPages = doc.getNumPages();

            System.out.println("Opened document " + sInput);
            
            WordFinderConfig wordConfig = new WordFinderConfig();
            
            // Need to set this to true so phrases will be concatenated properly
            wordConfig.setNoHyphenDetection(true);
            
            DocTextFinder docTextFinder = new DocTextFinder(doc, wordConfig);
            
            List<DocTextFinderMatch> docMatches = docTextFinder.getMatchList(0, nPages - 1, sRegex);

            for (DocTextFinderMatch wInfo : docMatches) {
                // Show the matching phrase
                String s = wInfo.getMatchString();
                System.out.println(s);

                // Get the word quads
                List<DocTextFinderQuadInfo> quadList = wInfo.getQuadInfo();

                // Iterate through the quad info and create highlights
                for (DocTextFinderQuadInfo qInfo : quadList) {
                    Page docPage = doc.getPage(qInfo.getPageNum());
                    HighlightAnnotation highlight = new HighlightAnnotation(docPage, qInfo.getQuads());
                    highlight.setNormalAppearance(highlight.generateAppearance());
                }
            }

            // Save the document with the highlighted matched strings
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
            doc.close();
        }
        finally {
            lib.delete();
        }
    }
}
