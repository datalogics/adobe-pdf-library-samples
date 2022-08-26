//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample extracts Unicode and ASCII text from two PDF input documents and saves the
// content to a text file and to a PDF output document.
//
// Command-line:    <input-pdf>  <output-name>       (Both optional)
//
// For more detail see the description of the ExtractText sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#extracttext

#include <iostream>
#include <fstream>
#include <cstring>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "DLExtrasCalls.h"
#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT_1 "ExtractText.pdf"
#define DEF_INPUT_2 "ExtractUnicodeText.pdf"
#define DEF_OUTPUT_1 "ExtractText-out.pdf"
#define DEF_OUTPUT_2 "ExtractText-out.txt"

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName1(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT_1);
    std::string csInputFileName2(argc > 2 ? argv[2] : DIR_LOC DEF_INPUT_2);
    std::string csOutputFileName1(argc > 3 ? argv[3] : DEF_OUTPUT_1);
    std::string csOutputFileName2(argc > 4 ? argv[4] : DEF_OUTPUT_2);

    DURING

        APDFLDoc inAPDoc(csInputFileName1.c_str(), true);

        // Step 1) Initialize the PDWordFinder class and things we'll need to draw the text to the output.

        // Note: This is not the only way to search through text (See the sample AddBookmarks
        // for another way), but it is a well-supported way.

        // Prepare the font we'll draw the text with.
        PDEFontAttrs fontAttrs;
        memset(&fontAttrs, 0, sizeof(fontAttrs));
        fontAttrs.name = ASAtomFromString("CourierStd");
        fontAttrs.type = ASAtomFromString("Type0");
        PDEFont font = PDEFontCreateFromSysFont(PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0), kPDEFontDoNotEmbed);

        // A default graphics state with which to draw the text.
        PDEGraphicState graphics;
        PDEDefaultGState(&graphics, sizeof(PDEGraphicState));

        // This FixedMatrix will point to where each next word will be drawn.
        ASFixedMatrix nextWordLocation;
        memset(&nextWordLocation, 0, sizeof(nextWordLocation));
        ASInt16 fontSize = 9;
        nextWordLocation.a = Int16ToFixed(fontSize); // Character width.
        nextWordLocation.d = Int16ToFixed(fontSize); // Character height.
        ASFixed leftMargin = ASFloatToFixed(0.5 * 72);
        nextWordLocation.h = leftMargin; // Left margin of 1/2 inch (72 pixels per inch)
        nextWordLocation.v = ASInt32ToFixed(10 * 72); // Start drawing text 10 inches from the bottom.

        // Use default settings for the PDWordFinder. See the sample TextSearch for an example of PDWordFinder settings.
        PDWordFinderConfigRec wfConfig;
        memset(&wfConfig, 0, sizeof(PDWordFinderConfigRec));
        wfConfig.recSize = sizeof(PDWordFinderConfigRec);

        // We'll use the PDWordfinder class to iterate through all the words in our input document.
        // If boolean value is set to true, the word finder extracts text in unicode.
        PDWordFinder wordFinder =
            PDDocCreateWordFinderEx(inAPDoc.getPDDoc(), WF_LATEST_VERSION, false, &wfConfig);

        PDWord wordList;
        ASInt32 numWordsFound;
        PDWordFinderAcquireWordList(wordFinder, 0, &wordList, NULL, NULL, &numWordsFound);

        // Step 2) Iterate through each word of the input document and draw each new line of text to the output document.

        std::cout << "Extracting " << numWordsFound << " words on the first page of "
                  << csInputFileName1.c_str() << " and placing into " << csOutputFileName1.c_str()
                  << std::endl;

        APDFLDoc outAPDoc;
        outAPDoc.insertPage(FloatToASFixed(8.5 * 72), Int16ToFixed(11 * 72), PDBeforeFirstPage);
        PDPage outPage = outAPDoc.getPage(0);
        PDEContent outPageContent = PDPageAcquirePDEContent(outPage, 0);

        for (int i = 0; i < numWordsFound; ++i) {
            PDEText nextLine = PDETextCreate();
            int nextLineIndex = 0;
            ASUns16 nextWordAttrs = 0;

            do {
                PDWord nextWord = PDWordFinderGetNthWord(wordFinder, i);

                // We'll copy over the next word of the line using its associated ASText.
                ASText nextWordASText = ASTextNew();
                PDWordGetASText(nextWord, 0, nextWordASText);

                // NOTE:  Uncomment this paragraph to print the text to the screen as well
                // This will print the text in UTF-16BE or in PDFDocEncoding; it may not output correctly on the console.
                // It will be correct in the output document, however.
                //
                // ASInt32 wordLen = 0;
                // char* consoleMessage = ASTextGetPDTextCopy(nextWordASText, &wordLen);
                // std::cout << consoleMessage;
                // ASfree(consoleMessage);
                // std::cout << "(Unicode text will not have displayed correctly.)";
                // std::cout << "(But unicode text is printed in the output correctly.)" << std::endl;
                // std::cout << "The text has been added to the output document." << std::endl;

                // Append the word to the line.
                PDETextAddASText(nextLine,
                                 kPDETextRun, // It's a text run because it's (usually) multiple characters.
                                 nextLineIndex, nextWordASText, font, &graphics,
                                 sizeof(PDEGraphicState), NULL, 0, &nextWordLocation);

                // Now that the word's been added, get the new location of the end of the line to prepare to add text there.
                ASFixedRect newLocation;
                PDETextGetBBox(nextLine, kPDETextRun, PDETextGetNumRuns(nextLine) - 1, &newLocation);
                // The h coordinate of our next word will be (at least) the very end of the last word.
                nextWordLocation.h = newLocation.right;

                // If the last word we printed is followed by a space, add a little to
                // the starting h coordinate of the next word to account for that.
                nextWordAttrs = PDWordGetAttr(nextWord);
                if (WXE_ADJACENT_TO_SPACE & nextWordAttrs) {
                    nextWordLocation.h += ASFloatToFixed(0.5 * fontSize);
                }

                ASTextDestroy(nextWordASText);

                ++nextLineIndex;
                ++i;

            } while ((!(WXE_LAST_WORD_ON_LINE & nextWordAttrs))); // Note that this procedure works only
                                                                  // if the last word in the page is the
                                                                  // last word in its line (which is true).

            --i; // i got overstepped at the end of the do-while loop.

            // We've captured the line, so add it to our output page's content.
            PDEContentAddElem(outPageContent, kPDEAfterLast, reinterpret_cast<PDEElement>(nextLine));

            // Prepare to draw the next line.
            nextWordLocation.h = leftMargin;
            // Drop down the v coordinate just enough to draw new text that won't overlap
            nextWordLocation.v -= Int16ToFixed(fontSize);
            PDERelease(reinterpret_cast<PDEObject>(nextLine));
        }

        // We've now captured every line of text. This sets all the content we've just added into the page.
        PDPageSetPDEContentCanRaise(outPage, NULL);

        PDERelease(reinterpret_cast<PDEObject>(font));
        PDWordFinderReleaseWordList(wordFinder, 0);
        PDWordFinderDestroy(wordFinder);

        PDPageReleasePDEContent(outPage, NULL);
        PDPageRelease(outPage);
        outAPDoc.saveDoc(csOutputFileName1.c_str());

        // Step 3) Extract unicode from a second PDF document into a text file

        APDFLDoc document(csInputFileName2.c_str(), true);

        std::ofstream outputFile(csOutputFileName2.c_str());

        if (outputFile.is_open()) {
            // Here, we set the boolean value is set to true, in order to extract text in unicode.
            PDWordFinder pdWordFinder =
                PDDocCreateWordFinderEx(document.getPDDoc(), WF_LATEST_VERSION, true, &wfConfig);

            ASInt32 numWords;
            PDWord wordArray;
            PDWordFinderAcquireWordList(pdWordFinder, 0, &wordArray, NULL, NULL, &numWords);

            std::cout << "Extracting " << numWords << " (Unicode) words on the first page of "
                      << csInputFileName2.c_str() << "; saving to " << csOutputFileName2.c_str()
                      << std::endl;

            PDPage pdPage = document.getPage(0);

            for (ASInt32 index = 0; index < numWords; ++index) {
                ASUTF8Val *utf8String;
                PDWord pdWord = PDWordFinderGetNthWord(pdWordFinder, index);

                ASText asText = ASTextNew();
                PDWordGetASText(pdWord, 0, asText);

                // Get the endian neutral utf8 string.
                utf8String = reinterpret_cast<ASUTF8Val *>(ASTextGetUnicodeCopy(asText, kUTF8));

                ASUns16 wordAttrs = PDWordGetAttr(pdWord);

                outputFile << utf8String;

                if (WXE_ADJACENT_TO_SPACE & wordAttrs) {
                    outputFile << " ";
                }

                if ((WXE_LAST_WORD_ON_LINE & wordAttrs) == WXE_LAST_WORD_ON_LINE ||
                    PDWordIsLastWordInRegion(pdWord)) {
                    outputFile << std::endl;
                }

                ASTextDestroy(asText);
            }

            // Close any remaining resources. APDFLDoc's destructor will take care of closing the documents.
            outputFile.close();
            PDPageReleasePDEContent(pdPage, 0);
            PDPageRelease(pdPage);
            PDWordFinderReleaseWordList(pdWordFinder, 0);
            PDWordFinderDestroy(pdWordFinder);

            PDERelease((PDEObject)graphics.fillColorSpec.space);
            PDERelease((PDEObject)graphics.strokeColorSpec.space);
        } else {
            std::cout << "Error opening output file." << std::endl;
        }

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
};
