package com.datalogics.pdfl.samples.Security.Redactions;

import java.util.*;

import com.datalogics.PDFL.*;

/*
  * 
 * This sample shows how to redact a PDF document. The program opens an input PDF, searches for
 * specific words using the Adobe PDF Library WordFinder, and then removes these words from the text.
 * 
 * For more detail see the description of the Redactions sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/redacting-text-from-a-pdf-document
 *
 * Copyright (c) 2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class Redactions {

    /**
     * @param args
     */
    public static void main(String[] args) throws Throwable {
        System.out.println("Redactions sample:");

        Library lib = new Library();
        try {
            
            System.out.println("Initialized the library.");
            
            String sInput = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
            String sOutput =  "Redactions-out-applied.pdf";

            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1)
                sOutput = args[1];

            Document doc = new Document(sInput);

            System.out.println("Input file: " + sInput + ", will write to " + sOutput);

            Page currentPage = doc.getPage(0);
            //
            // Redact occurrences of the word "rain" on the page.
            // Redact occurrences of the word "cloudy" on the page, changing the display details.
            //
            // For a more in-depth example of using the WordFinder, see the TextExtract sample.
            //
            // The TextExtract sample is described here.
            // http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/extracting-text-from-pdf-files
            //

            List<Quad> rainQuads = new ArrayList<Quad>();

            List<Quad> cloudyQuads = new ArrayList<Quad>();

            WordFinderConfig wordConfig = new WordFinderConfig();
            wordConfig.setDisableTaggedPDF(true);
            wordConfig.setIgnoreCharGaps(true);
            WordFinder wordFinder = new WordFinder(doc, WordFinderVersion.LATEST, wordConfig);

            List<Word> pageWords = wordFinder.getWordList(0);

            for ( Word word : pageWords )
                {
                    String currentWord = word.getText().toLowerCase();
                    System.out.print(" " + currentWord);
                    // Store the Quads of all "Cloudy" words in a list for later use in
                    // creating the redaction object.
                    if (currentWord.contains("rain"))
                        {
                            System.out.println("\nFound \"" + word.getText() + "\" on page 0 ");
                            rainQuads.addAll(word.getQuads());
                        }
                    else if (currentWord.contains("cloudy"))
                        {
                            System.out.println("\nFound \"" + word.getText() + "\" on page 0 ");
                            cloudyQuads.addAll(word.getQuads());
                        }
                }
            Color red = new Color(1.0, 0.0, 0.0);
            Color green = new Color(0.0, 1.0, 0.0);
            Color white = new Color (1.0);

            System.out.println("\nFound "+ cloudyQuads.size() +" \"cloudy\" instances.");
            Redaction not_cloudy = new Redaction(currentPage, cloudyQuads, red);

            // Fill the "normal" appearance of text with 25% opaque red;
            not_cloudy.setFillNormal(true);
            not_cloudy.setFillColor(red, 0.25);

            System.out.println("\nFound "+ rainQuads.size() +" \"rain\" instances.");
            Redaction annot = new Redaction(currentPage, rainQuads);
            annot.setInternalColor(green);
            annot.setTextColor(white);
            annot.setFontFace("CourierStd");
            annot.setFontSize(8.0);

            // Fill the redaction with the word "rain", drawn in white 
            annot.setOverlayText("rain");
            annot.setRepeat(true);
            annot.setScaleToFit(true);

            // Update the page's content and save the file with clipping
            currentPage.updateContent();

            doc.save(EnumSet.of(SaveFlags.FULL), "Redactions-out.pdf");
            // actually redact the instances of the found word
            doc.applyRedactions();

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);

        }
        finally {
            lib.delete();
        }
    }
}
