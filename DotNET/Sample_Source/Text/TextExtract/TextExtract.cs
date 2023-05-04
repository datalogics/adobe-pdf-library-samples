using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * This program pulls text from a PDF file and exports it to a text file (TXT).
 * It will open a PDF file called Constitution.PDF and create an output file called
 * TextExtract-untagged-out.txt. The export file includes page number references, and
 * the text is produced using standard Times Roman encoding. The program is also written
 * to include a provision for working with tagged documents, and determines if the original
 * PDF file is tagged or untagged. Tagging is used to make PDF files accessible
 * to the blind or to people with vision problems. 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace TextExtract
{
    class TextExtract
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TextExtract Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                // This is a tagged PDF.
                String sInput = Library.ResourceDirectory + "Sample_Input/pdf_intro.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                // This is an untagged PDF.
                //Resources/Sample_Input/constitution.pdf"

                Document doc = new Document(sInput);

                Console.WriteLine("Input file:  " + sInput);

                // Determine if the PDF is tagged.  We'll use a slightly different set of rules 
                // for parsing tagged and untagged PDFs.
                //
                // We'll determine if the PDF is tagged by examining the MarkInfo 
                // dictionary of the document.  First, check for the existence of the MarkInfo dict.
                bool docIsTagged = false;
                PDFDict markInfoDict;
                PDFBoolean markedEntry;
                if ((markInfoDict = (PDFDict) doc.Root.Get("MarkInfo")) != null)
                {
                    if ((markedEntry = (PDFBoolean) markInfoDict.Get("Marked")) != null)
                    {
                        if (markedEntry.Value)
                            docIsTagged = true;
                    }
                }

                WordFinderConfig wordConfig = new WordFinderConfig();
                wordConfig.IgnoreCharGaps = false;
                wordConfig.IgnoreLineGaps = false;
                wordConfig.NoAnnots = false;
                wordConfig.NoEncodingGuess = false;

                // Std Roman treatment for custom encoding; overrides the noEncodingGuess option
                wordConfig.UnknownToStdEnc = false;

                wordConfig.DisableTaggedPDF = false; // legacy mode WordFinder creation
                wordConfig.NoXYSort = true;
                wordConfig.PreserveSpaces = false;
                wordConfig.NoLigatureExp = false;
                wordConfig.NoHyphenDetection = false;
                wordConfig.TrustNBSpace = false;
                wordConfig.NoExtCharOffset = false; // text extraction efficiency
                wordConfig.NoStyleInfo = false; // text extraction efficiency

                WordFinder wordFinder = new WordFinder(doc, WordFinderVersion.Latest, wordConfig);

                if (docIsTagged)
                    ExtractTextTagged(doc, wordFinder);
                else
                    ExtractTextUntagged(doc, wordFinder);
            }
        }

        static void ExtractTextUntagged(Document doc, WordFinder wordFinder)
        {
            int nPages = doc.NumPages;
            IList<Word> pageWords = null;

            System.IO.StreamWriter logfile = new System.IO.StreamWriter("TextExtract-untagged-out.txt");
            Console.WriteLine("Writing TextExtract-untagged-out.txt");

            for (int i = 0; i < nPages; i++)
            {
                pageWords = wordFinder.GetWordList(i);

                String textToExtract = "";

                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                {
                    Word wInfo;
                    wInfo = pageWords[wordnum];
                    string s = wInfo.Text;

                    // Check for hyphenated words that break across a line.  
                    if (((wInfo.Attributes & WordAttributeFlags.HasSoftHyphen) == WordAttributeFlags.HasSoftHyphen) &&
                        ((wInfo.Attributes & WordAttributeFlags.LastWordOnLine) == WordAttributeFlags.LastWordOnLine))
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
                        // Note we remove ascii hyphen, Unicode soft hyphen(\u00ad) and Unicode hyphen(0x2010).
                        string[] splitstrs = s.Split(new[] {'-', '\u00ad', '\x2010'});
                        textToExtract += splitstrs[0] + splitstrs[1];
                    }
                    else
                        textToExtract += s;

                    // Check for space adjacency and add a space if necessary.
                    if ((wInfo.Attributes & WordAttributeFlags.AdjacentToSpace) == WordAttributeFlags.AdjacentToSpace)
                    {
                        textToExtract += " ";
                    }

                    // Check for a line break and add one if necessary
                    if ((wInfo.Attributes & WordAttributeFlags.LastWordOnLine) == WordAttributeFlags.LastWordOnLine)
                        textToExtract += "\n";
                }

                logfile.WriteLine("<page " + (i + 1) + ">");
                logfile.WriteLine(textToExtract);

                // Release requested WordList
                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                    pageWords[wordnum].Dispose();
            }

            Console.WriteLine("Extracted " + nPages + " pages.");
            logfile.Close();
        }

        static void ExtractTextTagged(Document doc, WordFinder wordFinder)
        {
            int nPages = doc.NumPages;
            IList<Word> pageWords = null;

            System.IO.StreamWriter logfile = new System.IO.StreamWriter("TextExtract-tagged-out.txt");
            Console.WriteLine("Writing TextExtract-tagged-out.txt");

            for (int i = 0; i < nPages; i++)
            {
                pageWords = wordFinder.GetWordList(i);

                String textToExtract = "";

                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                {
                    Word wInfo;
                    wInfo = pageWords[wordnum];
                    string s = wInfo.Text;

                    // In most tagged PDFs, soft hyphens are used only to break words across lines, so we'll 
                    // check for any soft hyphens and remove them from our text output.
                    // 
                    // Note that we're not checking for the LastWordOnLine flag, unlike untagged PDF.  For Tagged PDF, 
                    // words are not flagged as being the last on the line if they are not at the end of a sentence.
                    if (((wInfo.Attributes & WordAttributeFlags.HasSoftHyphen) == WordAttributeFlags.HasSoftHyphen))
                    {
                        // Remove the hyphen and combine the two parts of the word before adding to the extracted text.
                        // Note that we pass in the Unicode character for soft hyphen(\u00ad) and hyphen(0x2010).
                        string[] splitstrs = s.Split(new[] {'\u00ad','\x2010'});
                        textToExtract += splitstrs[0] + splitstrs[1];
                    }
                    else
                        textToExtract += s;

                    // Check for space adjacency and add a space if necessary.
                    if ((wInfo.Attributes & WordAttributeFlags.AdjacentToSpace) == WordAttributeFlags.AdjacentToSpace)
                    {
                        textToExtract += " ";
                    }

                    // Check for a line break and add one if necessary.
                    // Normally this is accomplished using WordAttributeFlags.LastWordOnLine,
                    // but for tagged PDFs, the LastWordOnLine flag is set according to the
                    // tags in the PDF, not according to visual line breaks in the document.
                    //
                    // To preserve the visual line breaks in the document, we'll check whether
                    // the word is the last word in the region.  If you instead prefer to
                    // break lines according to the tags in the PDF, use
                    // (wInfo.Attributes & WordAttributeFlags.LastWordOnLine) == WordAttributeFlags.LastWordOnLine, 
                    // similar to the untagged case.
                    if (wInfo.IsLastWordInRegion)
                        textToExtract += "\n";
                }

                logfile.WriteLine("<page " + (i + 1) + ">");
                logfile.WriteLine(textToExtract);

                // Release requested WordList
                for (int wordnum = 0; wordnum < pageWords.Count; wordnum++)
                    pageWords[wordnum].Dispose();
            }

            Console.WriteLine("Extracted " + nPages + " pages.");
            logfile.Close();
        }
    }
}
