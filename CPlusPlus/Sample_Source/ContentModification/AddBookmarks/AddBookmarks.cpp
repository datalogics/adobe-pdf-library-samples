//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample shows how to add bookmarks to a PDF document.
//
// Command line:  <target-search-word> <input-file> <output-file>   (all optional)
//
// For more detail see the description of the AddBookmarks sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addbookmarks

#include <sstream>
#include <string>
#include <vector>

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#include "PERCalls.h"
#include "PagePDECntCalls.h"
#include "ASCalls.h"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "Ulysses.pdf"
#define DEF_OUTPUT "AddBookmarks-out.pdf"
#define DEF_TARGET "before"

typedef struct _WordLocation {
    ASFixedRect m_loc; // Bounding rectangle (relative to page)
    ASInt32 m_page;    // Page number
    ASText m_text;     // Actual text
    _WordLocation(ASInt32 p) { m_page = p; }
} WordLocation;

static bool searchWord(PDWord nextWord, const char *target);

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;

    // Initialize the Adobe PDF Library.
    APDFLib lib;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csSearchWord(argc > 1 ? argv[1] : DEF_TARGET);
    std::string csInputFileName(argc > 2 ? argv[2] : INPUT_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 3 ? argv[3] : DEF_OUTPUT);

    std::cout << "Will search for \"" << csSearchWord.c_str() << "\" in " << csInputFileName.c_str()
              << " and write output to " << csOutputFileName.c_str() << std::endl;

    DURING

        APDFLDoc APDoc(csInputFileName.c_str(), true);
        PDDoc mydoc = APDoc.getPDDoc();

        // Step 1) Find each occurrence of target word in the document and record the location and text

        std::vector<WordLocation> vWords;

        // We'll use the PDWordFinder class to iterate through our document's words, looking for target word.
        PDWordFinder wordFinder = PDDocCreateWordFinderUCS(mydoc, WF_LATEST_VERSION, 0, NULL);

        // Our PDWordFinder will iterate through the words with this. We will not directly access it.
        PDWord *wordList = new PDWord();
        ASInt32 numWordsFound(0);

        // Search all words in each page of the document.  It is assumed that no word will span pages.
        for (int page = 0; page < PDDocGetNumPages(mydoc); ++page) {
            // Must be called before we call PDWordFinderGetNthWord.
            PDWordFinderAcquireWordList(wordFinder, page, wordList, NULL, NULL, &numWordsFound);

            for (int nextWordIndex = 0; nextWordIndex < numWordsFound; ++nextWordIndex) {
                PDWord nextWord = PDWordFinderGetNthWord(wordFinder, nextWordIndex);
                if (searchWord(nextWord, csSearchWord.c_str())) {
                    WordLocation wl(page);
                    // The text of the word.
                    ASText nextWordText = ASTextNew();
                    PDWordGetASText(nextWord, 0, nextWordText);
                    wl.m_text = nextWordText;
                    // Its quads: The word might be split up into several quads (e.g., if it's
                    //   hyphenated), and we want the last set.
                    ASInt16 nQuads = PDWordGetNumQuads(nextWord);
                    ASFixedQuad quad;
                    PDWordGetNthQuad(nextWord, nQuads - 1, &quad);
                    ASFixedRect partialWord;
                    // The left location of the box lines up with the horizontal coordinate of the top-left point.
                    partialWord.left = quad.tl.h;
                    // The top location of the box lines up with the vertical coordinate of the top-left pont.
                    partialWord.top = quad.tl.v;
                    wl.m_loc = partialWord;
                    vWords.push_back(wl);
                }
            }
            // Prepare to iterate over the word list for the next page.
            PDWordFinderReleaseWordList(wordFinder, page);
        }

        // Un-comment these lines if you wish to print out the words found
        // std::vector<WordLocation>::iterator it2, it2End = vWords.end();
        // for ( it2 = vWords.begin(); it2 != it2End; ++it2 )
        //{
        //  ASText partialText = it2->m_text;
        //  ASInt32 wordLen = 0;
        //  //Unicode text will probably not display correctly. Rest assured it will look fine in the document.
        //  std::cout << "page " << it2->m_page << " " << ASTextGetPDTextCopy(partialText, &wordLen) << std::endl;
        // }

        // Step 2) Create a bookmark for each occurrence of target word with this information.

        // We'll create each bookmark by iterating over the subheadings.
        // The bookmark's text will be the subheading's text,
        // And the bookmark's action will be to bring the reader to
        // the location of that subheading.

        // Bookmarks are added to a document's bookmark root.
        PDBookmark bookMarkRoot = PDDocGetBookmarkRoot(mydoc);

        std::vector<WordLocation>::iterator it, itEnd = vWords.end();
        for (it = vWords.begin(); it != itEnd; ++it) {
            // Get the necessary information.
            PDPage nextPage = APDoc.getPage(it->m_page);

            ASFixedRect *nextLocation = &(it->m_loc);

            // Make the bookmark and set its action; they must be created before their action is set.
            PDBookmark nextbm = PDBookmarkAddNewChildASText(bookMarkRoot, it->m_text);

            // We create a View Destination pointing to the location of the subheading, and then
            // create an action which will take the reader to that destination.
            PDViewDestination nextDestination =
                PDViewDestCreate(mydoc, nextPage,
                                 ASAtomFromString("XYZ"), // View Destination Fit Type
                                 nextLocation, // Pointer to the location rectangle we want.
                                 Int16ToFixed(0), // Zoom factor. 0 means to inherit the current zoom factor
                                 0);              // Unused argument

            // The first and third arguments are the source PDDoc and the destination PDDoc,
            // respectively. They must the the same.
            PDAction nextDestAct = PDActionNewFromDest(mydoc, nextDestination, mydoc);

            PDBookmarkSetAction(nextbm, nextDestAct); // Give the bookmark its action!
            PDPageRelease(nextPage);
        }

        // Step 3) Demonstrate different zoom levels.

        ASInt32 numZoomBookmarks(0);
        if (vWords.size() > 0) {
            // We will add a few children bookmarks to the first bookmark (if there was one)
            // that we created, which all copy that bookmark at different zoom levels.

            PDBookmark parentBm = PDBookmarkGetFirstChild(PDDocGetBookmarkRoot(mydoc));
            PDBookmark zoom100 = PDBookmarkAddNewChild(parentBm, "100% Zoom");
            PDBookmark zoom200 = PDBookmarkAddNewChild(parentBm, "200% Zoom");
            PDBookmark zoom800 = PDBookmarkAddNewChild(parentBm, "800% Zoom");
            PDBookmark zoom40 = PDBookmarkAddNewChild(parentBm, "40% Zoom");

            numZoomBookmarks = 4;
            PDBookmark zoomBookmarks[] = {zoom100, zoom200, zoom800, zoom40};
            ASFloat zoomFactors[] = {1.0f, 2.0f, 8.0f, 0.40f};

            // Copy the attributes of the parent bookmark.
            ASInt32 pageNumber;
            ASAtom fitType;
            ASFixedRect locationRect;
            ASFixed zoomFactor;
            PDViewDestination parentViewDestination = PDActionGetDest(PDBookmarkGetAction(parentBm));

            PDViewDestGetAttr(parentViewDestination, &pageNumber, &fitType, &locationRect, &zoomFactor);

            // Set each bookmark's view destination per the above array.
            PDPage parentPage = APDoc.getPage(pageNumber);
            for (int i = 0; i < numZoomBookmarks; ++i) {
                PDViewDestination nextView = PDViewDestCreate(mydoc, parentPage, fitType, &locationRect,
                                                              ASFloatToFixed(zoomFactors[i]), 0);
                PDAction nextAction = PDActionNewFromDest(mydoc, nextView, mydoc);
                PDBookmarkSetAction(zoomBookmarks[i], nextAction);
            }

            PDPageRelease(parentPage);
        }

        // Step 5) Save and close the document.

        std::cout << " --> Bookmarked " << vWords.size() << " occurrences, and added "
                  << numZoomBookmarks << " zoom bookmarks\n";
        // Release objects
        for (it = vWords.begin(); it != itEnd; ++it) {
            ASTextDestroy(it->m_text);
        }

        APDoc.saveDoc(csOutputFileName.c_str());

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode;
};

// The word-matching function.  Note that it is case sensitive!
bool searchWord(PDWord nextWord, const char *target) {
    ASText nextWordASText = ASTextNew();
    PDWordGetASText(nextWord, 0, nextWordASText);
    ASInt32 wordLen = 0;
    char *nextWordChar = ASTextGetPDTextCopy(nextWordASText, &wordLen);
    if (strlen(nextWordChar) < strlen(target)) {
        return false;
    }
    for (unsigned int index = 0; index < strlen(target); index++) {
        if (nextWordChar[index] != target[index]) {
            return false;
        }
    }
    ASfree(nextWordChar);
    return true;
}
