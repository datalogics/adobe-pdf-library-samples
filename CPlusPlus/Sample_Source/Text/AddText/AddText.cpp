//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates creating a new PDF document and adding text to the page, using a PDEFont element.
//
// Command-line:  <output-file>  <text-to-put-in-it>     (Both optional)
//
// For more detail see the description of the AddText sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addtext

#include <iostream>

#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "ASExtraCalls.h"

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#define DEF_OUTPUT "AddText-out.pdf"
#define DEF_TEXT "This text was placed!"

int main(int argc, char **argv) {

    ASErrorCode errCode = 0;
    APDFLib libInit;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csOutputFileName(argc > 1 ? argv[1] : DEF_OUTPUT);
    std::string csText(argc > 2 ? argv[2] : DEF_TEXT);
    std::cout << "Creating single-page document " << csOutputFileName.c_str() << " with text \""
              << csText.c_str() << "\" in it" << std::endl;
    DURING

        // Step 1: Create a document, insert/acquire a page, and acquire its PDEContent

        APDFLDoc doc;

        // Insert a 4 inch x 4 inch page into the document
        doc.insertPage(Int16ToFixed((4 * 72)), Int16ToFixed((4 * 72)), PDBeforeFirstPage);

        // Get the first page from the document.  (Page numbers are 0-based.)
        //     NOTE: Caller is responsible for releasing the PDPage acquired.
        PDPage page = doc.getPage(0);

        // Acquire PDEContent, PDE objects can be added to the acquired content.
        PDEContent content = PDPageAcquirePDEContent(page, NULL);

        // Step 2: Create a PDEFont object. The PDEFont object is created from a PDSysFont object,
        //     which contains attributes such as font name and type.

        PDEFontAttrs fontAttrs;
        memset(&fontAttrs, 0, sizeof(fontAttrs));
        fontAttrs.name = ASAtomFromString("CourierStd"); // Set the font name and type.
        fontAttrs.type = ASAtomFromString("Type1");

        // Locate the system font that corresponds to the PDEFontAttrs struct we just set.
        PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0);

        // Create the CourierStd Type1 font with embed flag set.
        PDEFont pdeFont = PDEFontCreateFromSysFont(sysFont, kPDEFontCreateEmbedded);

        // Step 3: Set the graphic state. The graphics state must be set for all PDE objects that will be
        //   displayed on a PDPage. This object is used to set information about colors, colorspace,
        //   linewidth, etc. In this sample we set the values to default because it is simple text placement.

        PDEGraphicState gState;
        PDEDefaultGState(&gState, 0);

        ASDoubleMatrix textMatrix; // Controls the size and location of text on page.
        memset(&textMatrix, 0, sizeof(textMatrix));
        textMatrix.a = 12.0;     // Character width.
        textMatrix.d = 12.0;     // Character height.
        textMatrix.h = 1 * 72.0; // Place at a x-val of 1 inch from the left side of the page.
        textMatrix.v = 2 * 72.0; // Place at a y-val of 2 inches from the bottom of the page.

        PDETextState tState; // Text state may be adjusted for character spacing, etc.
                             // Using default values in this sample.

        // Step 4: Create the PDEText object and add it to the PDEContent object.

        PDEText textObj = PDETextCreate();

        PDETextAddEx(textObj, // The PDEText object we just created.
                     kPDETextRun, // kPDETextRun/kPDETextChar specify whether a string/character will be inserted.
                     0, // The index after which to add the character or text run.
                     (Uns8 *)csText.c_str(), // The string that will be added should be type-cast as a pointer to Uns8.
                     static_cast<ASInt32>(csText.length()), // Length of the string.
                     pdeFont,         // The PDEFont we created above
                     &gState, 0, // PDEGraphicState and its size. ontains graphical attributes of the text object.
                     &tState, 0, // Text state and its size. Contains textual attributes of the text object.
                     &textMatrix, // Matrix containing size and location for the text.
                     NULL);       // The matrix for the line width when stroking text.

        // Add the text element to the page's content.
        PDEContentAddElem(content, kPDEAfterLast, reinterpret_cast<PDEElement>(textObj));

        PDPageSetPDEContentCanRaise(page, NULL); // Set the content back into the page.

        // Step 5: Save the output document and clean up resources

        doc.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveLinearized);

        // Release objects that are no longer in use.
        PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));
        PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));
        PDERelease(reinterpret_cast<PDEObject>(pdeFont));
        PDERelease(reinterpret_cast<PDEObject>(textObj));
        PDPageReleasePDEContent(page, NULL);
        PDPageRelease(page);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library.
}
