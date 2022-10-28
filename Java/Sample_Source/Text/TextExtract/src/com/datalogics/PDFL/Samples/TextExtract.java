package com.datalogics.pdfl.samples.Text.TextExtract;

import java.io.FileOutputStream;
import java.io.OutputStreamWriter;
import java.util.*;

import com.datalogics.PDFL.*;

/*
 * 
* This program pulls text from a PDF file and exports it to a text file (TXT).
 * It will open a PDF file called Constitution.PDF and create an output file called
 * TextExtract-untagged-out.txt. The export file includes page number references, and
 * the text is produced using standard Times Roman encoding. The program is also written
 * to include a provision for working with tagged documents, and determines if the original
 * PDF file is tagged or untagged. Tagging is used to make PDF files accessible
 * to the blind or to people with vision problems. 
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class TextExtract {
    public static void main (String[] args) throws Throwable
    {
        System.out.println("TextExtract sample:");

        Library lib = new Library();

        try {

            // This is an untagged PDF.
            // Library.getResourceDirectory() + "Sample_Input/constitution.pdf"

            // This is a tagged PDF.
            String sInput = Library.getResourceDirectory() + "Sample_Input/pdf_intro.pdf";
            if ( args.length > 0 )
                sInput = args[0];
            System.out.println("Reading " + sInput);

            Document doc = new Document(sInput);

            System.out.println("Opened document " + sInput);

            // Determine if the PDF is tagged.  We'll use a slightly different set of rules 
            // for parsing tagged and untagged PDFs.
            //
            // We'll determine if the PDF is tagged by examining the MarkInfo 
            // dictionary of the document.  First, check for the existence of the MarkInfo dict.
            Boolean docIsTagged = false;
            PDFDict markInfoDict;
            PDFBoolean markedEntry;
            if ((markInfoDict = (PDFDict)doc.getRoot().get("MarkInfo")) != null)
            {
                if ((markedEntry = (PDFBoolean)markInfoDict.get("Marked")) != null)
                {
                    if (markedEntry.getValue())
                        docIsTagged = true;
                }
            }

            WordFinderConfig wordConfig = new WordFinderConfig();
            wordConfig.setIgnoreCharGaps(false);
            wordConfig.setIgnoreLineGaps(false);
            wordConfig.setNoAnnots(false);
            wordConfig.setNoEncodingGuess(false);

            // Std Roman treatment for custom encoding; overrides the noEncodingGuess option
            wordConfig.setUnknownToStdEnc(false);

            wordConfig.setDisableTaggedPDF(false);  // legacy mode WordFinder creation
            wordConfig.setNoXYSort(true);
            wordConfig.setPreserveSpaces(false);
            wordConfig.setNoLigatureExp(false);
            wordConfig.setNoHyphenDetection(false);
            wordConfig.setTrustNBSpace(false);
            wordConfig.setNoExtCharOffset(false);       // text extraction efficiency
            wordConfig.setNoStyleInfo(false);           // text extraction efficiency

            WordFinder wordFinder = new WordFinder(doc, WordFinderVersion.LATEST, wordConfig);

            if (docIsTagged)
                ExtractTextTagged(doc, wordFinder);
            else
                ExtractTextUntagged(doc, wordFinder);
            
            doc.close();
        }
        finally {
            lib.delete();
        }
    }
    
    static void ExtractTextUntagged(Document doc, WordFinder wordFinder) throws Throwable
    {
        int nPages = doc.getNumPages();
        List<Word> pageWords = null;

        FileOutputStream logfile = new FileOutputStream("TextExtract-untagged-out.txt");
        System.out.println("Writing TextExtract-untagged-out.txt");
        OutputStreamWriter logwriter = new OutputStreamWriter(logfile, "UTF-8");

        for (int i = 0; i < nPages; i++)
        {
            pageWords = wordFinder.getWordList(i);
            
            String textToExtract = "";
            
            for (int wordnum = 0; wordnum < pageWords.size(); wordnum++)
            {
                Word wInfo;
                wInfo = pageWords.get(wordnum);
                String s = wInfo.getText();
                                      
                // Check for hyphenated words that break across a line.  
                if ((wInfo.getAttributes().contains(WordAttributeFlags.HAS_SOFT_HYPHEN)) &&
                    (wInfo.getAttributes().contains(WordAttributeFlags.LAST_WORD_ON_LINE)))
                {
                    // Remove the hyphen and combine the two parts of the word before adding to the extracted text.
                    // Note that we pass in the Unicode character for soft hyphen as well as the regular hyphen.
                    //
                    // In untagged PDF, it's not uncommon to find a mixture of hard and soft hyphens that may
                    // not be used for their intended purposes.
                    // (Soft hyphens are intended only for words that break across lines.)
                    //
                    // For the purposes of this sample, we'll remove all hyphens.  In practice, you may need to check 
                    // words against a dictionary to determine if the hyphenated word is actually one word or two.
                    // Note we remove ascii hyphen, Unicode soft hyphen(\u00ad) and Unicode hyphen(0x2010)
                    String[] splitstrs = s.split("-|\u00ad|0x2010");
                    for(int j = 0; j < splitstrs.length; j++)
                        textToExtract = textToExtract + splitstrs[j];
                }
                else
                    textToExtract = textToExtract + s;

                // Check for space adjacency or last word in region and add a space if necessary.
                // LastWordInRegion is true if the WordFinder determined that this is the last word in a region.
                // This may be set for words that are visually separated when viewing the PDF,
                // but are not separated by a space.  Here, it's used in conjunction with
                // WordAttributes.AdjacentToSpace to determine where to insert spaces when
                // post-processing WordFinder results.
                if (wInfo.getAttributes().contains(WordAttributeFlags.ADJACENT_TO_SPACE) || wInfo.getIsLastWordInRegion())
                {
                    textToExtract = textToExtract + " ";
                }
                // Check for a line break and add one if necessary
                if (wInfo.getAttributes().contains(WordAttributeFlags.LAST_WORD_ON_LINE))
                    textToExtract = textToExtract + "\n";
            }
            String pageNum = "<page " + (i+1) + ">\n";
            logwriter.write(pageNum, 0, pageNum.length());
            logwriter.write(textToExtract, 0, textToExtract.length());
            logwriter.write("\n");

            // Release requested WordList
            for (int wordnum = 0; wordnum < pageWords.size(); wordnum++)
                pageWords.get(wordnum).delete();
        }
        System.out.println("Extracted " + nPages + " pages.");
        logwriter.close();
    }

    static void ExtractTextTagged(Document doc, WordFinder wordFinder) throws Throwable
    {
        int nPages = doc.getNumPages();
        List<Word> pageWords = null;

        FileOutputStream logfile = new FileOutputStream("TextExtract-tagged-out.txt");
        System.out.println("Writing TextExtract-tagged-out.txt");
        OutputStreamWriter logwriter = new OutputStreamWriter(logfile, "UTF-8");

        for (int i = 0; i < nPages; i++)
        {
            pageWords = wordFinder.getWordList(i);

            String textToExtract = "";

            for (int wordnum = 0; wordnum < pageWords.size(); wordnum++)
            {
                Word wInfo;
                wInfo = pageWords.get(wordnum);
                String s = wInfo.getText();

                // In most tagged PDFs, soft hyphens are used only to break words across lines, so we'll 
                // check for any soft hyphens and remove them from our text output.
                // 
                // Note that we're not checking for the LAST_WORD_ON_LINE flag, unlike untagged PDF.  For Tagged PDF, 
                // words are not flagged as being the last on the line if they are not at the end of a sentence.
                if (wInfo.getAttributes().contains(WordAttributeFlags.HAS_SOFT_HYPHEN))
                {
                    // Remove the hyphen and combine the two parts of the word before adding to the extracted text.
                    // Note that we pass in the Unicode character for soft hyphen(\u00ad) and Unicode hyphen(0x2010).
                    String[] splitstrs = s.split("\u00ad|0x2010");
                    for(int j = 0; j < splitstrs.length; j++)
                        textToExtract = textToExtract + splitstrs[j];
                }
                else
                    textToExtract = textToExtract + s;

                    // Check for space adjacency or last word in region and add a space if necessary.
                    // LastWordInRegion is true if the WordFinder determined that this is the last word in a region.
                    // This may be set for words that are visually separated when viewing the PDF,
                    // but are not separated by a space.  Here, it's used in conjunction with
                    // WordAttributes.AdjacentToSpace to determine where to insert spaces when
                    // post-processing WordFinder results.
                if (wInfo.getAttributes().contains(WordAttributeFlags.ADJACENT_TO_SPACE) || wInfo.getIsLastWordInRegion())
                {
                    textToExtract =  textToExtract + " ";
                }
                // Check for a line break and add one if necessary
                if (wInfo.getAttributes().contains(WordAttributeFlags.LAST_WORD_ON_LINE))
                    textToExtract = textToExtract + "\n";
            }
            String pageNum = "<page " + (i+1) + ">\n";
            logwriter.write(pageNum, 0, pageNum.length());
            logwriter.write(textToExtract, 0, textToExtract.length());
            logwriter.write("\n");

            // Release requested WordList
            for (int wordnum = 0; wordnum < pageWords.size(); wordnum++)
                pageWords.get(wordnum).delete();

        }
        System.out.println("Extracted " + nPages + " pages.");
        logwriter.close();
    }   
};
