//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample shows how to add three different kinds of hyperlinks to a PDF document.
//
// Command-line:  <input-file>  <output-file>    (Both optional)
//
// For more detail see the description of the AddLinks sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addlinks

#include <iostream>
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "PSFCalls.h"
#include "ASCalls.h"
#include "ASExtraCalls.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "CosCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddLinks.pdf"
#define DEF_OUTPUT "AddLinks-out.pdf"

// Helper function to create and display the text "Click Me" onto a page given the x and y position.
static PDEText clickMeTextMaker(double xPos, double yPos);

int main(int argc, char **argv) {
    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Adding 3 hyperlinks to file " << csInputFileName.c_str() << " and saving to "
              << csOutputFileName.c_str() << std::endl;

    DURING

        // Open document and fetch the first page and its content object
        APDFLDoc inDoc(csInputFileName.c_str(), true);
        CosDoc inputDocCosDoc = PDDocGetCosDoc(inDoc.pdDoc);
        PDPage page1 = PDDocAcquirePage(inDoc.pdDoc, 0);
        PDEContent pageContent = PDPageAcquirePDEContent(page1, NULL);

        // Step 1) Create, set up, and place a link that will open a file.

        // Create a text object which displays "Click Me" at the given location
        PDEText clickMeText1 = clickMeTextMaker(72 * 6.48, 72 * 7.1);

        // Add this text object to the page's content
        PDEContentAddElem(pageContent, kPDEAfterLast, (PDEElement)clickMeText1);

        // Set up the annotation's bounds. 72 pixels represent one inch.
        ASFixedRect annotLocation;
        PDETextGetBBox(clickMeText1, kPDETextRun, 0, &annotLocation);

        PDERelease(reinterpret_cast<PDEObject>(clickMeText1));

        // Create a file attachment annotation with the input page and bounds
        PDAnnot fileAnnot = PDPageCreateAnnot(page1, ASAtomFromString("FileAttachment"), &annotLocation);

        // Add the annotation as the first annotation on the page.
        PDPageAddAnnot(page1, 0, fileAnnot);

        // Cast the file attachment annotation as a link.
        PDLinkAnnot fileLink = CastToPDLinkAnnot(fileAnnot);

        // Make the link's border invisible
        PDLinkAnnotBorder linkBorder;
        linkBorder.hCornerRadius = 0;
        linkBorder.vCornerRadius = 0;
        linkBorder.width = 0;
        linkBorder.dashArrayLen = 0;
        PDLinkAnnotSetBorder(fileLink, &linkBorder);

        // Create a cos dictionary object to hold attributes of the link.
        CosObj fileLinkDict = CosNewDict(inputDocCosDoc, false, 2);

        // Using the file specification key "F" set up the file input path
        CosDictPut(fileLinkDict, ASAtomFromString("F"),
                   CosNewString(inputDocCosDoc, false, "../../../../Resources/Sample_Input/DOCXLink.docx",
                                strlen("../../../../Resources/Sample_Input/DOCXLink.docx")));

        // Using the name key "S" set the type to a Launch for opening the file
        CosDictPut(fileLinkDict, ASAtomFromString("S"),
                   CosNewName(inputDocCosDoc, false, ASAtomFromString("Launch")));

        PDAction fileLinkAction = PDActionFromCosObj(fileLinkDict);
        // Set up an action object with the given attributes from the dictionary.

        // Set the action to the link.
        PDLinkAnnotSetAction(fileLink, fileLinkAction);

        // Get the cos object from the link.
        CosObj fileLinkObj = PDAnnotGetCosObj(fileLink);

        // Set the cos object type to the subtype, link.
        CosDictPutKeyString(fileLinkObj, "Subtype", CosNewNameFromString(inputDocCosDoc, false, "Link"));

        // Step 2) Create, set up, and place a link that will move to a new location.

        // Create a text object which displays "Click Me" at the given location
        PDEText clickMeText2 = clickMeTextMaker(72 * 6.48, 72 * 6.3);

        PDEContentAddElem(pageContent, kPDEAfterLast, (PDEElement)clickMeText2);

        PDERelease(reinterpret_cast<PDEObject>(clickMeText2));

        // Set up the annotation's bounds
        PDETextGetBBox(clickMeText2, kPDETextRun, 0, &annotLocation);

        // Create a new destination annotation with the input page and bounds
        PDAnnot newDestAnnot = PDPageCreateAnnot(page1, ASAtomFromString("Text"), &annotLocation);

        // Add the annotation as the second annotation on the page.
        PDPageAddAnnot(page1, 1, newDestAnnot);

        // Cast the file attachment annotation as a link.
        PDLinkAnnot newDestLink = CastToPDLinkAnnot(newDestAnnot);

        // Set the link's border to the previously made invisible border.
        PDLinkAnnotSetBorder(newDestLink, &linkBorder);

        // Create an instance of the page to jump to.
        PDPage destPage = PDDocAcquirePage(inDoc.pdDoc, 2);

        // Get the bounds of that page
        ASFixedRect bounds;
        PDPageGetBBox(destPage, &bounds);

        // Create a View Destination pointing to the location desired along with different settings
        PDViewDestination nextDestination =
            PDViewDestCreate(inDoc.pdDoc, destPage,   // The source document and destination page.
                             ASAtomFromString("XYZ"), // Fit Type, set to the upper-left corner.
                             &bounds,                 // Pointer to the location rectangle we want.
                             PDViewDestNULL, // Zoom factor. 0 means to inherit the current zoom factor.
                             0);             // Unused argument.

        PDPageRelease(destPage);

        // Create an action representing the destination.
        PDAction nextDestAct = PDActionNewFromDest(inDoc.pdDoc, nextDestination, inDoc.pdDoc);

        // Set the action to the link.
        PDLinkAnnotSetAction(newDestLink, nextDestAct);

        // Get the cos object from the link.
        CosObj newDestLinkObj = PDAnnotGetCosObj(newDestLink);

        // Set the cos object type to the subtype, link.
        CosDictPutKeyString(newDestLinkObj, "Subtype", CosNewNameFromString(inputDocCosDoc, false, "Link"));

        // Step 3) Create, set up, and place a link the will open a webpage.

        // Create a text object which displays "Click Me" at the given location
        PDEText clickMeText3 = clickMeTextMaker(72 * 6.48, 72 * 5.5);

        PDEContentAddElem(pageContent, kPDEAfterLast, (PDEElement)clickMeText3);

        PDERelease(reinterpret_cast<PDEObject>(clickMeText3));

        // Set up the annotation's bounds
        PDETextGetBBox(clickMeText3, kPDETextRun, 0, &annotLocation);

        PDAnnot URIAnnot = PDPageCreateAnnot(page1, ASAtomFromString("Text"), &annotLocation);

        // Add the annotation as the third annotation on the page.
        PDPageAddAnnot(page1, 1, URIAnnot);

        // Cast the file attachment annotation as a link.
        PDLinkAnnot URILink = CastToPDLinkAnnot(URIAnnot);

        // Let's try out a border this time...
        linkBorder.width = 2;
        PDLinkAnnotSetBorder(URILink, &linkBorder);

        // Create a cos dictionary object to hold attributes of the link.
        CosObj URIDict = CosNewDict(inputDocCosDoc, false, 2);

        // Using the name key "S" set up the URI type
        CosDictPut(URIDict, ASAtomFromString("S"), CosNewName(inputDocCosDoc, false, ASAtomFromString("URI")));

        // Using the URI key "URI" set up the desired path
        CosDictPut(URIDict, ASAtomFromString("URI"),
                   CosNewString(inputDocCosDoc, false, "http://www.datalogics.com",
                                strlen("http://www.datalogics.com")));

        // Set up an action object with the given attributes from the dictionary.
        PDAction newPdAction = PDActionFromCosObj(URIDict);

        // Set the action to the link.
        PDLinkAnnotSetAction(URILink, newPdAction);

        // Get the cos object from the link.
        CosObj URILinkObj = PDAnnotGetCosObj(URILink);

        // Set the cos object type to the subtype, link.
        CosDictPutKeyString(URILinkObj, "Subtype", CosNewNameFromString(inputDocCosDoc, false, "Link"));

        // Step 4) Save and close.

        PDPageSetPDEContentCanRaise(page1, NULL); // Set the content back into the page.

        // Page and content released before closing the document.
        PDPageReleasePDEContent(page1, NULL);
        PDPageRelease(page1);

        inDoc.saveDoc(csOutputFileName.c_str());

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode); // If there was an error, display it.
    END_HANDLER

    return errCode; // Returns program status.
}

// PDEText Function: Creates and displays the text "Click Me" onto a pdf page given the x and y position
PDEText clickMeTextMaker(double xPos, double yPos) {
    PDEFontAttrs fontAttrs;
    memset(&fontAttrs, 0, sizeof(fontAttrs));
    // Set the font name and type.
    fontAttrs.name = ASAtomFromString("CourierStd");
    fontAttrs.type = ASAtomFromString("Type1");

    // Locate the system font that corresponds to the PDEFontAttrs struct we just set.
    PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0);

    // Create the CourierStd Type1 font with embed flag set.
    PDEFont courierFont = PDEFontCreateFromSysFont(sysFont, kPDEFontCreateEmbedded);

    std::string textToDisplay = " Click Me ";

    ASDoubleMatrix textMatrix;                  // Transformation matrix for location of the text
    memset(&textMatrix, 0, sizeof(textMatrix)); // Clear structure.
    textMatrix.a = 10;                          // Set font width and height.
    textMatrix.d = 10;                          // Set font point size.
    textMatrix.h = xPos;                        // x coordinate on page (72 pixels = 1 inch).
    textMatrix.v = yPos;                        // y coordinate on page.

    PDEGraphicState gState;
    PDEDefaultGState(&gState, sizeof(PDEGraphicState));

    // Release the stroke color space before modifying it.
    PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));

    // Set color to blue.
    ASFixed red = ASFloatToFixed(0);
    ASFixed green = ASFloatToFixed(0);
    ASFixed blue = ASFloatToFixed(1);

    // Set the RGB color space.
    gState.fillColorSpec.space = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));

    // Set up the color space values to form the wanted color.
    gState.fillColorSpec.value.color[0] = red;
    gState.fillColorSpec.value.color[1] = green;
    gState.fillColorSpec.value.color[2] = blue;

    PDEText textObj = PDETextCreate();

    // Adding the text run to the PDE text object.
    PDETextState tState;
    memset(&tState, 0, sizeof(tState));
    PDETextAddEx(textObj,     // Text container to add to.
                 kPDETextRun, // kPDETextRun or kPDETextChar for text runs or text characters.
                 0,           // The index after which to add the text run.
                 (Uns8 *)textToDisplay.c_str(), // Text to add.
                 static_cast<ASInt32>(textToDisplay.length()),        // Length of text.
                 courierFont,                   // Font to apply to text.
                 &gState, sizeof(gState), // PDEGraphicState and its size. Contains attributes of the text object.
                 &tState, 0,  // Text state and its size .Contains attributes of the text object.
                 &textMatrix, // Matrix containing size and location for the text.
                 NULL);       // Stroke matrix for the line width when stroking text.

    // Release used objects
    PDERelease(reinterpret_cast<PDEObject>(courierFont));
    PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));
    PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));

    return textObj;
}
