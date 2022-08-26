//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The CreateLayers sample program demonstrates how to programmatically add two layers to a PDF
// document, one that displays text, and the other, annotations.
//
// Command-line:  <output-file>   <number-of-pages>    (Both optional)
//
// For more detail see the description of the CreateLayers sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#createlayers

#include <iostream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "CosCalls.h"

#define DEF_OUTPUT "CreateLayers-out.pdf"

// A function that places text onto a given position in a PDF.
/* static */ PDEText textMaker(std::string displayText, double xPos, double yPos);

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csOutputFileName(argc > 1 ? argv[1] : DEF_OUTPUT);
    std::cout << "Creating new document " << csOutputFileName.c_str()
              << " and inserting 2 layers..." << std::endl;

    DURING

        // Step 1) Create a pdf document and obtain a reference to it's first page, and its' content

        APDFLDoc doc;

        // Insert a standard 8.5 inch x 11 inch page into the document.
        doc.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11 * 72), PDBeforeFirstPage);
        PDPage page = doc.getPage(0);
        PDEContent pageContent = PDPageAcquirePDEContent(page, NULL);

        // Step 2) Set up the optional content groups, commonly referred to as layers.

        // Create optional content groups (Layers) for texts and annotations.
        PDOCG optionalGroupText = PDOCGCreate(doc.pdDoc, ASTextFromPDText("TextLayer"));
        PDOCG optionalGroupAnnot = PDOCGCreate(doc.pdDoc, ASTextFromPDText("AnnotationLayer"));

        // Set the their initial state to visible.
        PDOCConfig ocConfig = PDDocGetOCConfig(doc.pdDoc);
        PDOCGSetInitialState(optionalGroupText, ocConfig, true);
        PDOCGSetInitialState(optionalGroupAnnot, ocConfig, true);

        CosObj order;                                   // The order of the optional content.
        PDOCConfigGetOCGOrder(ocConfig, &order);        // Find the order.
        ASInt32 cosObjectTotal = CosArrayLength(order); // Find the total cosObjects.

        // Insert the layers as a cosObject in the pdf.
        CosArrayInsert(order, cosObjectTotal, PDOCGGetCosObj(optionalGroupText));
        CosArrayInsert(order, cosObjectTotal + 1, PDOCGGetCosObj(optionalGroupAnnot));

        // Put the new order back as a part of the pdf's configuration.
        PDOCConfigSetOCGOrder(ocConfig, order);

        // Create a layer array in order to properly pass the layer into the membership dictionary creation function.
        PDOCG pdDocArrayText[2];
        pdDocArrayText[0] = optionalGroupText;
        pdDocArrayText[1] = NULL;

        // Obtain the membership dictionary of the text layer.
        PDOCMD optionalGroupMDText = PDOCMDCreate(doc.pdDoc, pdDocArrayText, kOCMDVisibility_AllOn);

        // Create a layer array in order to properly pass the layer into the membership dictionary creation function.
        PDOCG pdDocArrayAnnot[2];
        pdDocArrayAnnot[0] = optionalGroupAnnot;
        pdDocArrayAnnot[1] = NULL;

        // Obtain the membership dictionary of the annotation layer.
        PDOCMD optionalGroupMDAnnot = PDOCMDCreate(doc.pdDoc, pdDocArrayAnnot, kOCMDVisibility_AllOn);

        // Step 3) Add text to the page and set what layer they belong to.

        // By calling the textMaker function, place the following text at the given location.
        PDEText displayText1 =
            textMaker("All the text on this page will be placed in it's own layer", 72 * 1, 72 * 10);
        PDEText displayText2 =
            textMaker("Whereas the attachments will appear in a separate layer", 72 * 1, 72 * 9.75);
        PDEText displayText3 = textMaker("There will be an annotation to the right.", 72 * 1, 72 * 8);
        PDEText displayText4 = textMaker("There will be an annotation to the right.", 72 * 1, 72 * 7);

        PDEContent texts = PDEContentCreate();

        // Add the created text objects to the page's content.
        PDEContentAddElem(texts, kPDEAfterLast, (PDEElement)displayText1);
        PDEContentAddElem(texts, kPDEAfterLast, (PDEElement)displayText2);
        PDEContentAddElem(texts, kPDEAfterLast, (PDEElement)displayText3);
        PDEContentAddElem(texts, kPDEAfterLast, (PDEElement)displayText4);

        // Create an empty container for the text.
        PDEContainer textContainer = PDEContainerCreate(ASAtomFromString("Texts"), NULL, false);

        // Place them into this container
        PDEContainerSetContent(textContainer, texts);

        // Add the text to the page's content
        PDEContentAddElem(pageContent, kPDEAfterLast, (PDEElement)textContainer);

        // Set the container's membership dictionary to the text layer.
        PDEElementSetOCMD((PDEElement)textContainer, optionalGroupMDText);

        // Set the content back into the page.
        PDPageSetPDEContentCanRaise(page, NULL);

        // Release used objects
        PDERelease(reinterpret_cast<PDEObject>(displayText1));
        PDERelease(reinterpret_cast<PDEObject>(displayText2));
        PDERelease(reinterpret_cast<PDEObject>(displayText3));
        PDERelease(reinterpret_cast<PDEObject>(displayText4));
        PDERelease(reinterpret_cast<PDEObject>(textContainer));

        // Step 4) Add annotations to the page and set the layer they belong to.

        // Set up the bounds for the first annotation. 72 pixels represents one inch.
        ASFixedRect annotLocation;
        annotLocation.left = ASFloatToFixed(5.50 * 72);
        annotLocation.right = ASFloatToFixed(5.00 * 72);
        annotLocation.top = ASFloatToFixed(8.20 * 72);
        annotLocation.bottom = ASFloatToFixed(7.70 * 72);

        // Create the annotation at the location.
        PDAnnot newAnnot = PDPageCreateAnnot(page, ASAtomFromString("FileAttachment"), &annotLocation);

        // Add the annotation to the page. -2 means to add to the end of the array.
        PDPageAddAnnot(page, -2, newAnnot);

        // Set the annotation to the annotation layer.
        PDAnnotSetOCMD(newAnnot, optionalGroupMDAnnot);

        // Move the bounds for the second annotation.
        annotLocation.left = ASFloatToFixed(5.50 * 72);
        annotLocation.right = ASFloatToFixed(5.00 * 72);
        annotLocation.top = ASFloatToFixed(7.20 * 72);
        annotLocation.bottom = ASFloatToFixed(6.70 * 72);

        // Create the second annotation.
        PDAnnot newAnnot2 = PDPageCreateAnnot(page, ASAtomFromString("FileAttachment"), &annotLocation);

        // Set the annotation to the annotation layer.
        PDAnnotSetOCMD(newAnnot2, optionalGroupMDAnnot);

        // Add the annotation to the page.
        PDPageAddAnnot(page, -2, newAnnot2);

        // Step 5) Save the output document and exit.

        // Release objects no longer in use
        PDPageReleasePDEContent(page, NULL);
        PDPageRelease(page);

        doc.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveLinearized);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

// Helper function to create a PDEText object
/* static */ PDEText textMaker(std::string displayText, double xPos, double yPos) {
    PDEFontAttrs fontAttrs;
    memset(&fontAttrs, 0, sizeof(fontAttrs));
    fontAttrs.name = ASAtomFromString("CourierStd");
    fontAttrs.type = ASAtomFromString("Type1");

    // Locate the system font that corresponds to the PDEFontAttrs struct we just set.
    PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0);

    // Create the CourierStd Type1 font with embed flag set.
    PDEFont courierFont = PDEFontCreateFromSysFont(sysFont, kPDEFontCreateEmbedded);

    // Transformation matrix for text which determines location of the text on page.
    ASDoubleMatrix textMatrix;
    memset(&textMatrix, 0, sizeof(textMatrix));
    textMatrix.a = 10;   // Font width and height.
    textMatrix.d = 10;   // Font point size.
    textMatrix.h = xPos; // x coordinate on page (72 pixels = 1 inch).
    textMatrix.v = yPos; // y coordinate on page.

    // Create a Graphics State object with defaults
    PDEGraphicState gState;
    PDEDefaultGState(&gState, sizeof(PDEGraphicState));

    // Create a new text run
    PDEText textObj = PDETextCreate();

    PDETextState tState;

    // Adding the text run to the PDE text object.
    PDETextAddEx(textObj,                     // Text container to add to.
                 kPDETextRun,                 // kPDETextRun or kPDETextChar as appropriate
                 0,                           // The index after which to add the text run.
                 (Uns8 *)displayText.c_str(), // Text to add.
                 displayText.length(),        // Length of text.
                 courierFont,                 // Font to apply to text.
                 &gState, sizeof(gState),     // PDEGraphicState and its size.
                 &tState, 0,                  // Text state and its size.
                 &textMatrix,                 // Matrix containing size and location for the text.
                 NULL); // Stroke matrix for the line width when stroking text.

    // Release used objects.
    PDERelease(reinterpret_cast<PDEObject>(courierFont));
    PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));
    PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));

    return textObj;
}
