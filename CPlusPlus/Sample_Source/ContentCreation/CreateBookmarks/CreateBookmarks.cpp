//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// A bookmark in a PDF document labels a place within the PDF document to serve as a destination,
// often a heading or a graphic. If you manually add a bookmark to a PDF, you can create a link
// elsewhere in the same PDF document and connect it to that bookmark. When the reader clicks on
// that link the viewer will take the reader to the place in the PDF where that bookmark is found.
//
// This sample demonstrates how to add a bookmark to a PDF documnent. It creates a parent bookmark
// in Korean via Unicode (bookmark 1), and then creates a child bookmark under that (bookmark 1.1).
// The program saves the PDF document as an output file in the current directory.
//

#include <iostream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "SixPages.pdf"
#define OUTPUT_FILE "CreateBookmarks-out.pdf"

#define PARENTBOOKMARK "bookmark 1"
#define CHILDBOOKMARK "bookmark 1.1"

#define dstPageNumParent 0
#define dstPageNumChild 1

int main(int argc, char *argv[]) {
    // Initialize the library
    APDFLib libInit;
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFileName(argc > 2 ? argv[2] : OUTPUT_FILE);
    std::cout << "Adding bookmarks to " << csInputFileName.c_str() << ", writing to "
              << csOutputFileName.c_str() << std::endl;

    DURING

        // Open the input document
        APDFLDoc apdflDoc(csInputFileName.c_str(), true);
        PDDoc pdDoc1 = apdflDoc.getPDDoc();

        // Get bookmark root. Return value is valid even if bookmark root is empty.
        // Then add new child or siblings.
        PDBookmark rootBm = PDDocGetBookmarkRoot(pdDoc1);

        // Set the parent title in Unicode
        unsigned char korTitle[] = {0xFE, 0xFF, 0xB3, 0x00, 0xD5, 0x5C,
                                    0xBB, 0xFC, 0xAD, 0x6D, 0x00, 0x00};

        // Use reinterpret_cast to allow korTitle to be 'unsigned', necessary to avoid the C4309 warning
        PDBookmark parentBm = PDBookmarkAddNewChild(rootBm, reinterpret_cast<char *>(korTitle));

        // Acquire PDPage of the bookmark destination from PDDoc
        PDPage dstPageParent = PDDocAcquirePage(pdDoc1, dstPageNumParent);

        // Initialize parameters for the destination PDPage
        ASAtom fitType = ASAtomFromString("Fit");
        ASFixed zoom = 2;
        ASFixedRect destRect = {0, Int16ToFixed(5 * 72), Int16ToFixed(8 * 72), 0};

        // Create view of the destination PDPage
        PDViewDestination destParent =
            PDViewDestCreate(pdDoc1, dstPageParent, fitType, &destRect, zoom, dstPageNumParent);

        // Create new PDAction and set bookmark action
        if (PDViewDestIsValid(destParent)) {
            PDAction pdActionParent = PDActionNewFromDest(pdDoc1, destParent, pdDoc1);
            if (PDActionIsValid(pdActionParent)) {
                PDBookmarkSetAction(parentBm, pdActionParent);
            } else {
                std::cout << "PDAction not valid" << std::endl;
                return -1;
            }
        }

        // Now create a child bookmark for this parent
        PDBookmark childBm = PDBookmarkAddNewChild(parentBm, CHILDBOOKMARK);
        PDPage dstPageChild = PDDocAcquirePage(pdDoc1, dstPageNumChild);

        fitType = ASAtomFromString("FitH");
        zoom = 5;

        PDViewDestination destChild =
            PDViewDestCreate(pdDoc1, dstPageChild, fitType, &destRect, zoom, dstPageNumChild);

        if (PDViewDestIsValid(destChild)) {
            PDAction pdActionChild = PDActionNewFromDest(pdDoc1, destChild, pdDoc1);
            if (PDActionIsValid(pdActionChild)) {
                PDBookmarkSetAction(childBm, pdActionChild);
            } else {
                std::cout << "PDAction not valid (child)" << std::endl;
                return -2;
            }
        }

        // Save document to output file
        apdflDoc.saveDoc(csOutputFileName.c_str());
        PDPageRelease(dstPageParent);
        PDPageRelease(dstPageChild);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}
