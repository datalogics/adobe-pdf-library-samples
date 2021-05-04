package com.datalogics.pdfl.samples.Security.AddRegexRedaction;

import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.DocTextFinder;
import com.datalogics.PDFL.DocTextFinderMatch;
import com.datalogics.PDFL.DocTextFinderQuadInfo;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Redaction;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.WordFinderConfig;

import java.util.EnumSet;
import java.util.List;

/*
 * This sample demonstrates using the DocTextFinder to find examples of a specific phrase in a PDF
 * document that matches a user-supplied regular expression. When the sample finds the text it
 * will redact the phrase from the output document.
 * 
 * Copyright (c) 2007-2021, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class AddRegexRedaction {

    public static void main(String[] args) throws Throwable {
        System.out.println("Redactions regex sample:");

        Library lib = new Library();
        try {
            
            System.out.println("Initialized the library.");
            
            final String sInput = (args.length > 0) ? args[0] : Library.getResourceDirectory() + "Sample_Input/AddRegexRedaction.pdf";
            final String sOutput1 = "AddRegexRedaction-out.pdf";
            final String sOutput2 = "AddRegexRedaction-out-applied.pdf";
            
            // Highlight and redact occurrences of the words that match this regular expression.
            // Phone numbers
            final String sRegex = "((1-)?(\\()?\\d{3}(\\))?(\\s)?(-)?\\d{3}-\\d{4})";
            // Email addresses
            //final String sRegex = "(\\b[\\w.!#$%&'*+\\/=?^`{|}~-]+@[\\w-]+(?:\\.[\\w-]+)*\\b)";
            // URLs
            //final String sRegex = "((https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,}))";

            Document doc = new Document(sInput);
            
            final int nPages = doc.getNumPages();

            System.out.println("Input file: " + sInput);

            // Create a WordFinder configuration with default parameters
            WordFinderConfig wordConfig = new WordFinderConfig();
            
            // Need to set this to true so phrases will be concatenated properly
            wordConfig.setNoHyphenDetection(true);
                    
            // Create a DocTextFinder with the default wordfinder parameters
            DocTextFinder docTextFinder = new DocTextFinder(doc, wordConfig);
            
            // Retrieve the phrases matching a regular expression
            List<DocTextFinderMatch> docMatches = docTextFinder.getMatchList(0, nPages - 1, sRegex);
            
            // Redaction color will be red
            Color red = new Color(1.0, 0.0, 0.0);

            for (DocTextFinderMatch wInfo : docMatches) {
                // Show the matching phrase
                String s = wInfo.getMatchString();
                System.out.println(s);

                // Get the quads
                List<DocTextFinderQuadInfo> quadInfo = wInfo.getQuadInfo();

                // Iterate through the quad info and create highlights
                for (DocTextFinderQuadInfo qInfo : quadInfo) {
                    Page docpage = doc.getPage(qInfo.getPageNum());

                    Redaction red_fill = new Redaction(docpage, qInfo.getQuads(), red);

                    /* fill the "normal" appearance with 25% red */
                    red_fill.setFillNormal(true);
                    red_fill.setFillColor(red, 0.25);
                }
            }
            // Save the document with the highlighted matched strings
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput1);

            System.out.println("Wrote a PDF document with unapplied redactions.");

            // Apply all the redactions in the document
            doc.applyRedactions();

            // Save the document with the redacted matched strings
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput2);
            doc.close();

            System.out.println("Wrote a redacted PDF document.");

        }
        finally {
            lib.delete();
        }
    }
}
