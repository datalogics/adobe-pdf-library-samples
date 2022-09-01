//
// Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This is an executable driver to demonstrate the use of the
// ASFileSys (defined in AlternateFileSystem.cpp) with APDFL.
//
// An input PDF file (annotated.pdf, unless another file name is given
// as an argument to this program) will be opened and read.
// Its contents will be added to a PDDoc which has very simple
// content, and written to an output PDF file.
//
// Process return code:  0 on success, otherwise, non-zero
//

#ifdef WIN32
#pragma warning(disable : 4267)
#endif

#include <cstdio>

#include "PDFInit.h"
#include "CosCalls.h"
#include "CorCalls.h"
#include "ASCalls.h"
#include "PDCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PIExcept.h"
#include "PagePDECntCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "AlternateFileSystem-in.pdf"
#define OUTPUT_FILE "AlternateFileSystem-out.pdf"

// In AlternateFileSystem.cpp
extern ASFileSys altFileSys();

int main(int argc, char *argv[]) {
    APDFLib libInit; // Initialize the Adobe PDF Library.  Termination will be automatic when scope is lost

    if (libInit.isValid() == false) // Check for errors upon initialization.
    {
        int errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : OUTPUT_FILE);
    std::cout << "Processing input file " << csInputFile.c_str() << std::endl;
    std::cout << "Will write to output file " << csOutputFile.c_str() << std::endl;

    PDDoc pdDocIn;
    ASFileSys asFileSys = altFileSys();
    ASPathName asPathName = NULL;

    // Open a PDF file via the alternate file system and get a PDDoc from it
    DURING
        // Create asPathName with an Alternate FileSystem.
        asPathName =
            ASFileSysCreatePathName(asFileSys, ASAtomFromString(
#if MAC_ENV
                                                                "POSIXPath"
#else
                                                                "Cstring"
#endif
                                                                ), csInputFile.c_str(), 0);

        // Open pdDocIn from asPathName.
        pdDocIn = PDDocOpen(asPathName, asFileSys, NULL, true);
        ASFileSysReleasePath(asFileSys, asPathName);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        std::cout << "Couldn't open alternate input file " << csInputFile.c_str() << std::endl;
        return ERRORCODE;
    END_HANDLER

    // Create Doc, Page, Content Container
    PDDoc pdDoc;                 // reference to a PDF document
    PDPage pdPage;               // reference to a page in doc
    PDEContent pdeContent;       // container for page content
    ASFixedRect mediaBox;        // dimensions of page
    PDEFont pdeFont;             // reference to a font used on a page
    PDEFontAttrs pdeFontAttrs;   // font attributes
    PDEText pdeText;             // container for text
    ASFixedMatrix textMatrix;    // transformation matrix for text
    PDEColorSpace pdeColorSpace; // ColorSpace
    PDEGraphicState gState;      // graphic state to apply to operation
    const char *HelloWorldStr = "Hello AltFSTest!";

    DURING

        pdDoc = PDDocCreate(); // create new document

        mediaBox.left = fixedZero;           // dimensions of page
        mediaBox.top = Int16ToFixed(4 * 72); // in this case 5" x 4"
        mediaBox.right = Int16ToFixed(5 * 72);
        mediaBox.bottom = fixedZero;

        // Create page with those dimensions and insert as first page
        pdPage = PDDocCreatePage(pdDoc, PDBeforeFirstPage, mediaBox);

        // Acquire PDEContent container for page
        pdeContent = PDPageAcquirePDEContent(pdPage, NULL);

        // Acquire font, add text, insert into page content container
        memset(&pdeFontAttrs, 0, sizeof(pdeFontAttrs));
        pdeFontAttrs.name = ASAtomFromString("Times-Roman"); // font name
        pdeFontAttrs.type = ASAtomFromString("Type1");       // font type

        pdeFont = PDEFontCreate(&pdeFontAttrs, sizeof(pdeFontAttrs), 0, 255, 0, // Widths
                                0, ASAtomNull,                                  // Encoding
                                0, 0, 0, 0);                                    // Font embedding

        // The following code sets up the default Graphics state.  We do this so that
        // we can free the PDEColorSpace objects
        pdeColorSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceGray"));
        memset(&gState, 0, sizeof(PDEGraphicState));
        gState.strokeColorSpec.space = gState.fillColorSpec.space = pdeColorSpace;
        gState.miterLimit = fixedTen;
        gState.flatness = fixedOne;
        gState.lineWidth = fixedOne;

        memset(&textMatrix, 0, sizeof(textMatrix)); // clear structure
        textMatrix.a = Int16ToFixed(24);            // set font width and height
        textMatrix.d = Int16ToFixed(24);            // to 24 point size
        textMatrix.h = Int16ToFixed(1 * 72);        // x,y coordinate on page
        textMatrix.v = Int16ToFixed(2 * 72);        // in this case, 1" x 2"
        pdeText = PDETextCreate();                  // create new text run
        PDETextAdd(pdeText,                         // text container to add to
                   kPDETextRun,                     // kPDETextRun, kPDETextChar
                   0,                               // index
                   (ASUns8 *)HelloWorldStr,         // text to add
                   strlen(HelloWorldStr),           // length of text
                   pdeFont,                         // font to apply to text
                   &gState, sizeof(gState),         // graphic state to apply to text
                   NULL, 0,                         // text state and size of structure
                   &textMatrix,                     // transformation matrix for text
                   NULL);                           // stroke matrix

        // insert text into page content
        PDEContentAddElem(pdeContent, kPDEAfterLast, (PDEElement)pdeText);

        // Convert To Objects, Add To Page, Release Resources
        // Set the PDEContent for the page
        PDPageSetPDEContent(pdPage, NULL);

        // Insert pages from the opened doc to the created pdDoc
        PDDocInsertPages(pdDoc, PDLastPage, pdDocIn, 0, PDAllPages, PDInsertAll, NULL, NULL, NULL, NULL);

        ASPathName Path = APDFLDoc::makePath(csOutputFile.c_str());
        PDDocSave(pdDoc, PDSaveFull, Path, NULL, NULL, NULL);
        ASFileSysReleasePath(NULL, Path);

        // remember to release all objects that were created
        PDERelease((PDEObject)pdeFont);
        PDERelease((PDEObject)pdeText);
        PDPageReleasePDEContent(pdPage, NULL);
        PDERelease((PDEObject)pdeColorSpace);
        PDPageRelease(pdPage);
        PDDocRelease(pdDoc);

        PDDocClose(pdDocIn);
        PDDocRelease(pdDocIn);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}
