//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// AddContent adds several elements to a blank PDF input document, including text and a rectangle.
//
// Command-line:  <input-file> <output-file>   (both optional)
//
// For more detail see the description of the AddContent sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addcontent

#include <iostream>

#include "InitializeLibrary.h"

#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "PSFCalls.h"
#include "ASCalls.h"
#include "ASExtraCalls.h"
#include "APDFLDoc.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddContent.pdf"
#define DEF_OUTPUT "AddContent-out.pdf"

// Helper function used to create a rectangle, parameter description in function definition.
static PDEPath PathRect(ASFixed, ASFixed, ASFixed, ASFixed, int, ASFixed, ASFixed, ASFixed);

int main(int argc, char **argv) {
    APDFLib libInit; // Initialize the Adobe PDF Library. (Going out of scope will terminate.)
    ASErrorCode errCode = 0;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    DURING

        std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
        std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
        std::cout << "Adding some PDE Elements to file " << csInputFileName.c_str()
                  << " and saving as " << csOutputFileName.c_str() << std::endl;

        APDFLDoc document(csInputFileName.c_str(), true);

        // Step 1) Set up the font for the text to be displayed.

        PDEFontAttrs attrs;
        memset(&attrs, 0, sizeof(attrs));
        attrs.name = ASAtomFromString("CourierStd");
        attrs.type = ASAtomFromString("Type1");
        // Get the corresponding system font.
        PDSysFont sysFont = PDFindSysFont(&attrs, sizeof(attrs), kPDSysFontMatchFontType);

        PDSysFontGetAttrs(sysFont, &attrs, sizeof(PDEFontAttrs));

        // Check if font is embeddable.
        PDEFont courierStdFont = NULL;
        if (attrs.cantEmbed != 0) {
            std::cerr << "Font " << ASAtomGetString(attrs.name) << " can not be embedded";
        } else {
            // Create font from the system font and embed.
            courierStdFont = PDEFontCreateFromSysFont(sysFont, kPDEFontCreateEmbedded);
        }

        // Step 2) Set up the content to be added to the document. Elements are added to the page at
        //         X and Y values starting from the bottom left corner.

        // Text that will be displayed on page
        std::string textToDisplay = "Here is some text in the CourierStd font, using both PDEText "
                                    "and PDEFont.";

        PDEGraphicState gState;
        memset(&gState, 0, sizeof(gState));

        PDEDefaultGState(&gState, sizeof(gState)); // Set the graphics state to default values

        PDETextState tState;
        memset(&tState, 0, sizeof(tState));

        // Transformation matrix for text which determines location of the text on page.
        ASDoubleMatrix textMatrix;

        memset(&textMatrix, 0, sizeof(textMatrix));
        textMatrix.a = 10;       // Set font width and height.
        textMatrix.d = 10;       // Set font point size.
        textMatrix.h = 72 * 0.6; // x coordinate on page (72 pixels = 1 inch).
        textMatrix.v = 72 * 8;   // y coordinate on page.

        PDEText pdeText = PDETextCreate(); // Create a new text run.

        // Adding the text run to the PDE text object
        PDETextAddEx(pdeText,                       // Text container to add to
                     kPDETextRun,                   // kPDETextRun or kPDETextChar as appropriate
                     0,                             // The index after which to add the text run
                     (Uns8 *)textToDisplay.c_str(), // Text to add
                     textToDisplay.length(),        // Length of text
                     courierStdFont,                // Font to apply to text
                     &gState, sizeof(gState),       // Graphic state and its size
                     &tState, sizeof(tState),       // Text state and its size
                     &textMatrix,                   // Transformation matrix for text
                     NULL); // Stroke matrix for the line width when stroking text

        textMatrix.v = 72 * 7; // y coordinate on page.
        textToDisplay = "Below is a PDEPath rectangle. ";

        // Adding the text run to the PDE text object
        PDETextAddEx(pdeText,                       // Text container to add to.
                     kPDETextRun,                   // kPDETextRun or kPDETextChar as appropriate
                     0,                             // The index after which to add the text run.
                     (Uns8 *)textToDisplay.c_str(), // Text to add.
                     textToDisplay.length(),        // Length of text
                     courierStdFont,                // Font to apply to text.
                     &gState, sizeof(gState),       // Graphic state and its size.
                     &tState, sizeof(tState),       // Text state and its size.
                     &textMatrix,                   // Transformation matrix for text.
                     NULL); // Stroke matrix for the line width when stroking text.

        // Determine the needed flags for embedding and call the appropriate routines for doing so.
        PDEFontEmbedNow(courierStdFont, PDDocGetCosDoc(document.pdDoc));

        // Cook up a blue rectangle...
        PDEPath rect = PathRect(ASFloatToFixed(72 * 3.25), ASInt32ToFixed(72 * 4), ASInt32ToFixed(72 * 2),
                                ASInt32ToFixed(72 * 2), 46, fixedZero, fixedZero, fixedOne);

        // Step 3) Acquire PDEContent and add elements to the page.

        PDPage pdPage = PDDocAcquirePage(document.pdDoc, 0);
        PDEContent pdeContent = PDPageAcquirePDEContent(pdPage, 0);
        // Add the rectangle to the page content.
        PDEContentAddElem(pdeContent, kPDEAfterLast, (PDEElement)rect);
        // Add the text into page content.
        PDEContentAddElem(pdeContent, kPDEAfterLast, (PDEElement)pdeText);
        // Set the content back into the page
        PDPageSetPDEContentCanRaise(pdPage, NULL);
        document.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveLinearized);

        // Release all objects
        PDPageReleasePDEContent(pdPage, NULL);
        PDPageRelease(pdPage);
        PDERelease((PDEObject)pdeText);
        PDERelease((PDEObject)rect);
        PDERelease((PDEObject)courierStdFont);
        PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));
        PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

// Helper function: Creates a PDEPath object from supplied parameters
PDEPath PathRect(ASFixed x, ASFixed y, ASFixed width, ASFixed height, int lineWidth, ASFixed r,
                 ASFixed g, ASFixed b) {
    // Create the PDEPath object that will be used to draw a rectangle.
    PDEPath rectangle = PDEPathCreate();

    // Set the paint operation to Stroke for the path.
    PDEPathSetPaintOp(rectangle, kPDEStroke);

    // PDEColorValue color components for the RGB color space.
    PDEColorValue strokeClrValue;
    memset(&strokeClrValue, 0, sizeof(PDEColorValue));
    strokeClrValue.color[0] = ASInt32ToFixed(r);
    strokeClrValue.color[1] = ASInt32ToFixed(g);
    strokeClrValue.color[2] = ASInt32ToFixed(b);

    // Use RGB color space
    PDEColorSpace clrSpace;
    clrSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));

    // Assign fill/stroke values to the appropriate PDEColorSpec.
    PDEColorSpec strokeClrSpec;
    strokeClrSpec.space = clrSpace;
    strokeClrSpec.value = strokeClrValue;

    // Create the graphics state object as per specs
    PDEGraphicState gState;
    memset(&gState, 0, sizeof(PDEGraphicState));
    gState.strokeColorSpec = strokeClrSpec;
    gState.lineWidth = ASInt32ToFixed(lineWidth);
    gState.miterLimit = fixedTen;
    gState.flatness = fixedOne;

    // Set graphics state to the path to give it the color and size of the rectange.
    PDEElementSetGState((PDEElement)rectangle, &gState, sizeof(PDEGraphicState));

    // Prepare Array structure for pathData, and assign to object
    ASFixed pathData[5];
    pathData[0] = kPDERect;
    pathData[1] = x;
    pathData[2] = y;
    pathData[3] = width;
    pathData[4] = height;
    PDEPathSetData(rectangle, pathData, sizeof(pathData));

    // Released objects
    PDERelease((PDEObject)clrSpace);

    return rectangle;
}
