//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// Command line argument:  (Optional) <output-file-name>
//
// The AddArt sample demonstrates drawing a graphic image on a PDF page by manipulating PDEPath objects.
//
// For more detail see the description of the AddArt sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addart

#include <iostream>
#include "PagePDECntCalls.h"
#include "PEWCalls.h"
#include "PERCalls.h"
#include "ASCalls.h"
#include "ASExtraCalls.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define DEF_OUTPUT "AddArt-out.pdf"

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csOutputFileName(argc > 1 ? argv[1] : DEF_OUTPUT);

    DURING
        // Step 1: Create a PDF document with one 10"x10" page, acquire a page and it's content
        APDFLDoc document;

        document.insertPage(Int32ToFixed((72 * 10)), Int32ToFixed((72 * 10)), PDBeforeFirstPage);

        PDPage pdPage = document.getPage(0);

        PDEContent pdeContent = PDPageAcquirePDEContent(pdPage, 0);

        // Step 2: Set the graphics state for the first shape. This PDEGraphicsState object
        //     contains attributes about how a PDEElement will be displayed.

        PDEPath pdePath = PDEPathCreate();

        // PDEPath will be stroked, but not filled.
        PDEPathSetPaintOp(pdePath, kPDEStroke);

        // Struct that will hold display attributes.
        PDEGraphicState gState;

        // Set graphics state to default values.
        PDEDefaultGState(&gState, sizeof(PDEGraphicState));

        // Release the stroke color space before modifiying it.
        PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));

        ASFixed red = ASFloatToFixed(.6);   // Red Intensity range is [0.0 - 1.0].
        ASFixed green = ASFloatToFixed(.2); // Green Intensity range is [0.0 - 1.0].
        ASFixed blue = ASFloatToFixed(1.0); // Blue Intensity range is [0.0 - 1.0].

        // We are using the RGB color space in this case. Default value is "DeviceGray".
        gState.strokeColorSpec.space = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));

        gState.strokeColorSpec.value.color[0] = red; // Initially value = 0 (Black.)
        gState.strokeColorSpec.value.color[1] = green; // In this case the object will be painted purple.
        gState.strokeColorSpec.value.color[2] = blue;
        gState.lineWidth = Int32ToFixed(3); // Line width is set to a thickness of 3.
        gState.lineCap = 1;                 // Using rounded line caps.
        gState.lineJoin = 1;                // Using rounded line joins.

        // Set the PDEPath's graphic state.
        PDEElementSetGState(reinterpret_cast<PDEElement>(pdePath), &gState, sizeof(PDEGraphicState));

        // Release the PDEColorSpace objects.
        PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));
        PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));

        // Step 3: Draw the arrow using a list of coordinates and calling PDEPathAddSegment to
        //    join the (x, y) coordinates with straight lines.

        const unsigned numberOfPoints = 7;

        // Set up (x, y) coordinates for points of the arrow. Note:  72 pixels in one inch.
        ASInt32 arrowXCoordinates[numberOfPoints] = {(72 * 7), (72 * 6), (72 * 6), (72 * 4),
                                                     (72 * 4), (72 * 3), (72 * 5)};
        ASInt32 arrowYCoordinates[numberOfPoints] = {(72 * 7), (72 * 7), (72 * 3), (72 * 3),
                                                     (72 * 7), (72 * 7), (72 * 9)};
        ASFixedPoint pointToInsert;

        // Move to start point at the tip of the arrow before drawing lines.
        pointToInsert.h = ASInt32ToFixed(72 * 5);
        pointToInsert.v = ASInt32ToFixed(72 * 9);
        PDEPathAddSegment(pdePath, kPDEMoveTo, pointToInsert.h, pointToInsert.v, fixedZero,
                          fixedZero, fixedZero, fixedZero);

        // Loop through the coordinates and draw lines connecting each (x, y) pair.
        for (unsigned i = 0; i < numberOfPoints; ++i) {
            pointToInsert.h = ASInt32ToFixed(arrowXCoordinates[i]);
            pointToInsert.v = ASInt32ToFixed(arrowYCoordinates[i]);
            PDEPathAddSegment(pdePath, kPDELineTo, pointToInsert.h, pointToInsert.v, fixedZero,
                              fixedZero, fixedZero, fixedZero);
        }

        // Insert the PDEPath into the PDEContent that was acquired.
        PDEContentAddElem(pdeContent, kPDEAfterLast, reinterpret_cast<PDEElement>(pdePath));

        // Set the PDEContent back into the PDPage's Cos object.
        PDPageSetPDEContentCanRaise(pdPage, 0);

        // Release the path object.
        PDERelease(reinterpret_cast<PDEObject>(pdePath));

        // Step 4: Draw a second arrow and apply transformations: create a new PDEPath object and apply
        //   some transformations to it using a transformation matrix. We will add a fill color of yellow
        //   to this object to differentiate between the objects.  For more information on translations,
        //   rotations, scaling, and skews, see  "Coordinate Systems" (section 8.3) in
        //   "ISO 32000-1:2008, Document Management-Portable Document Format-Part 1: PDF 1.7, page 114" at
        //   http://www.adobe.com/content/dam/Adobe/en/devnet/acrobat/pdfs/PDF32000_2008.pdf#page=122

        // Second path object that will be drawn to the page.
        PDEPath pdePath2 = PDEPathCreate();

        // This arrow will be stroked and filled with color.
        PDEPathSetPaintOp(pdePath2, kPDEStroke | kPDEFill);

        // Fill color is yellow
        red = ASFloatToFixed(1.0);
        blue = ASFloatToFixed(0.0);
        green = ASFloatToFixed(1.0);

        // Set the Graphics state color space to RGB.
        gState.fillColorSpec.space = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));
        gState.strokeColorSpec.space = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));

        gState.fillColorSpec.value.color[0] = red; // Assign RGB color values.
        gState.fillColorSpec.value.color[1] = green;
        gState.fillColorSpec.value.color[2] = blue;

        PDEElementSetGState(reinterpret_cast<PDEElement>(pdePath2), &gState, sizeof(gState));

        PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));
        PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));

        // Redraw the exact same arrow we created in step 3.
        pointToInsert.h = ASInt32ToFixed(72 * 5);
        pointToInsert.v = ASInt32ToFixed(72 * 9);
        PDEPathAddSegment(pdePath2, kPDEMoveTo, pointToInsert.h, pointToInsert.v, fixedZero,
                          fixedZero, fixedZero, fixedZero);

        for (unsigned i = 0; i < numberOfPoints; ++i) {
            pointToInsert.h = ASInt32ToFixed(arrowXCoordinates[i]);
            pointToInsert.v = ASInt32ToFixed(arrowYCoordinates[i]);

            PDEPathAddSegment(pdePath2, kPDELineTo, pointToInsert.h, pointToInsert.v, fixedZero,
                              fixedZero, fixedZero, fixedZero);
        }

        // Move the arrow 4 inches right and 2 inches down.
        ASDoubleMatrix translationMatrix = {1, 0.0, 0.0, 1, (72.0 * 4.0), (-72.0 * 2.0)};
        // Scale it to 75% of its original size.
        ASDoubleMatrix scalingMatrix = {0.75, 0.0, 0.0, 0.75, 0.0, 0.0};
        // Holds result of multiplying the last two matrices.
        ASDoubleMatrix translateAndScale;

        // IMPORTANT NOTE! If applying multiple transformations to a matrix you must concatenate
        //    the matrices before applying transformations.
        // DO NOT attempt to call PDEElementSetMatrixEx() more than one time on the same PDEElement.
        ASDoubleMatrixConcat(&translateAndScale, &translationMatrix, &scalingMatrix);

        // Apply the transformation matrix.
        PDEElementSetMatrixEx(reinterpret_cast<PDEElement>(pdePath2), &translateAndScale);

        // Insert the PDEPath into the PDEContent that was acquired.
        PDEContentAddElem(pdeContent, kPDEAfterLast, reinterpret_cast<PDEElement>(pdePath2));

        // Set the PDEContent back into the PDPage's Cos object.
        PDPageSetPDEContentCanRaise(pdPage, 0);

        // Step 5) Release resources and save the PDF document.

        PDERelease(reinterpret_cast<PDEObject>(pdePath2));
        PDPageReleasePDEContent(pdPage, 0);
        PDPageRelease(pdPage);

        document.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveLinearized);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library.
}
