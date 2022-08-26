//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// In a PDF document, a thumbnail is a small graphic image that represents a page.
// Thumbnails appear in a panel on the left side of the Adobe Acrobat window and
// aid in navigating through a document, as a user can scroll through a series of
// thumbnails quickly to find a page. This sample program demonstrates how to create
// thumbnails for a PDF document, one for each page. The program saves the thumbnail
// images in a PDF output file, using an indexed color table with 256 colors RGB.
//
// For more detail see the description of the AddThumbnailsToPDF sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addthumbnailstopdf

#include <cstdio>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "AddThumbnailsToPDF-in.pdf"
#define OUTPUT_FILE "AddThumbnailsToPDF-out.pdf"

static void MakeColorTable(ASUns8 *lookupTable);
static ASBool MycancelProc(void *clientData);

int main(int argc, char *argv[]) {
    // Initialize the Adobe PDF Library. Termination will be automatic when scope is lost.
    APDFLib libInit;

    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : OUTPUT_FILE);
    std::cout << "Placing thumbnails into " << csInputFile.c_str() << ", writing to "
              << csOutputFile.c_str() << std::endl;

    DURING

        // Open the input document
        APDFLDoc apdflDoc(csInputFile.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();

        // Prepare the color lookup table
        ASUns8 lookupTable[768]; // Enough space for 256 colors (with three color components)
        MakeColorTable(lookupTable);

        // Create thumbnails
        PDDocCreateThumbs(pdDoc, 0, PDLastPage, NULL, NULL, ASAtomFromString("DeviceRGB"), 8, 255,
                          (char *)lookupTable, NULL, NULL, MycancelProc, NULL);

        // Save the output document
        apdflDoc.saveDoc(csOutputFile.c_str());

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}

// The Make Color Table function creates a generic indexed color table.
// It takes an array that must have memory allocated for 768 entries,
// 256 colors times three (RGB). Anything in the array will
// be overwritten.
//
void MakeColorTable(ASUns8 *p) {
    ASUns8 red, green, blue;
    int redCount, greenCount, blueCount;
    int i;

    // Populate a 6x6x6 entry RGB cube
    red = 0;
    for (redCount = 7; --redCount;) {
        green = 0;
        for (greenCount = 7; --greenCount;) {
            blue = 0;
            for (blueCount = 7; --blueCount;) {
                *p++ = red;
                *p++ = green;
                *p++ = blue;
                blue += 0x33;
            }
            green += 0x33;
        }
        red += 0x33;
    }

    /* Add 10 additional grays */
    red = 0;
    for (redCount = 0; redCount < 10; redCount++) {
        // Skip this one because it's already in the 6x6x6 cube
        if ((redCount & 1) == 0) {
            red += 0x11;
        }
        *p++ = red;
        *p++ = red;
        *p++ = red;
        red += 0x11;
    }

    // Fill the rest of the table with white
    for (i = 226; i < 256; i++) {
        *p++ = 255;
        *p++ = 255;
        *p++ = 255;
    }
}

//
// Call-back to provide some feedback to the user as we process
//
ASBool MycancelProc(void *clientData) {
    std::cout << '.';
    std::cout.flush();
    return false;
}
