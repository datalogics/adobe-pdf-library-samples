//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates transparency and blend modes in Adobe PDF Library for CMYK and RGB color spaces.
// It creates 13 sets of RGB and CMYK color blending circles, one for each of the 12 blend modes, and an
// additional set demonstrating the absence of blending. So a total of 24 color blending circles are drawn.
//
// Command-line:    <output-file>      (Optional)
//
// See ISO 32000, table 136, for more information, available here:
// http://www.adobe.com/content/dam/Adobe/en/devnet/acrobat/pdfs/PDF32000_2008.pdf#page=332
//
// For more detail see the description of the CreateTransparency sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#createtransparency

#include <iostream>
#include <sstream>
#include <string>
#include <vector>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PagePDECntCalls.h"
#include "PERCalls.h"
#include "PSFCalls.h"
#include "PEWCalls.h"

#define DEF_OUTPUT "CreateTransparency-out.pdf"

// The number of shapes in each color blending group. We've gone with a triad.
#define NUM_BLENDING_SHAPES 3

/* static */ PDEForm contentToForm(PDEContent content, ASInt32 formType, PDDoc document);

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csOutputFileName(argc > 1 ? argv[1] : DEF_OUTPUT);
    std::cout << "Creating " << csOutputFileName.c_str() << " with 13 pages demonstrating "
              << "blending modes" << std::endl;

    DURING

        // Step 1) Create and title a page for each color blending mode.

        APDFLDoc doc;

        ASFixed pageLength = ASFloatToFixed(10.0 * 72.0); // 1 inch == 72 pixels
        ASFixed pageHeight = ASFloatToFixed(4.25 * 72.0);

        // Compute page center,  we'll need these to place the triads later on.
        ASFixed pageCenter_X = (pageLength / 2);
        ASFixed pageCenter_Y = (pageHeight / 2);

        // The center point of the left half of the page.
        ASFixed leftHalfCenter_X = (pageCenter_X / 2);
        ASFixed leftHalfCenter_Y = pageCenter_Y;

        // The center point of the right half of the page.
        ASFixed rightHalfCenter_X = pageCenter_X + leftHalfCenter_X;
        ASFixed rightHalfCenter_Y = pageCenter_Y;

        // All twelve blend modes available in APDFL.
        std::vector<std::string> vBlendModes;
        vBlendModes.push_back("Opaque (None)");
        vBlendModes.push_back("Normal");
        vBlendModes.push_back("Multiply");
        vBlendModes.push_back("Screen");
        vBlendModes.push_back("Overlay");
        vBlendModes.push_back("Darken");
        vBlendModes.push_back("Lighten");
        vBlendModes.push_back("ColorDodge");
        vBlendModes.push_back("ColorBurn");
        vBlendModes.push_back("HardLight");
        vBlendModes.push_back("SoftLight");
        vBlendModes.push_back("Difference");
        vBlendModes.push_back("Exclusion");

        int numPages = vBlendModes.size(); // A page to demonstrate each blend mode

        // Now we will title each page with the name of its associated blending mode.
        PDEFontAttrs fontAttrs;
        memset(&fontAttrs, 0, sizeof(fontAttrs));
        fontAttrs.name = ASAtomFromString("CourierStd");
        fontAttrs.type = ASAtomFromString("Type1");

        PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0);
        PDEFont pdeFont = PDEFontCreateFromSysFont(sysFont, kPDEFontDoNotEmbed);

        // This matrix will determine how the text is displayed.
        ASDoubleMatrix textMatrix;
        memset(&textMatrix, 0, sizeof(textMatrix));
        ASDouble fontSize = 24.0;               // Font size, in points.
        textMatrix.a = textMatrix.d = fontSize; // Character width, height
        textMatrix.h = (0.25 * 72.0);           // 1/4 of an inch from the left margin.
        textMatrix.v =
            (ASFixedToFloat(pageHeight) - (0.20 * 72.00) - fontSize); // 1/5 of an inch from the top margin,
                                                                      //    adjusting for font size.

        // We create a default graphics state to draw the text with.
        PDEGraphicState gState;
        PDEDefaultGState(&gState, sizeof(gState));

        // Now create each page, and place each page title.
        for (int i = 0; i < numPages; i++) {
            doc.insertPage(pageLength, pageHeight, PDDocGetNumPages(doc.getPDDoc()) - 1);

            PDEText textObj = PDETextCreate();

            std::stringstream title;
            title << "Blend Mode: " << vBlendModes[i] << ".";

            // Add the text run
            PDETextAddEx(textObj,                     // To this object
                         kPDETextRun,                 // kPDETextRun vs. kPDETextChar
                         0,                           // Index
                         (Uns8 *)title.str().c_str(), // The text,
                         strlen(title.str().c_str()), // and its length.
                         pdeFont,                     // The font we want used.
                         &gState, sizeof(gState),     // Default the graphics state.
                         NULL, 0,                     // Default the text state.
                         &textMatrix,                 // The size and location of the text.
                         NULL);                       // Default the stroke matrix.

            PDPage outPage = doc.getPage(PDDocGetNumPages(doc.getPDDoc()) - 1);
            PDEContent pagecontent = PDPageAcquirePDEContent(outPage, 0);

            // Add the text element to the page's content.
            PDEContentAddElem(pagecontent, kPDEBeforeFirst, (PDEElement)(textObj));
            // We're done with the text element now that we've added it.
            PDERelease((PDEObject)(textObj));
            // Set the content back into the page.
            PDPageSetPDEContentCanRaise(outPage, NULL);

            // We're now done with this page and its contents.
            PDPageReleasePDEContent(outPage, 0);
            PDPageRelease(outPage);
        }
        PDERelease((PDEObject)gState.strokeColorSpec.space);
        PDERelease((PDEObject)gState.fillColorSpec.space);
        PDERelease((PDEObject)pdeFont);

        // Step 2) Define and create a prototype for the basic shape of each triad.

        // Note: PDEForm objects will be used as prototypes because they can group
        //    different PDEElements together, and for their cloning ability.

        // First we define the shape we'll use as a base. We'll make circles.

        ASFixed circleDiameter = FloatToASFixed(2.0 * 72.0);
        ASFixed radius = circleDiameter / 2;
        ASFixed threeFourthsRadius = (ASFloatToFixed(3.0f) * radius) / ASFloatToFixed(4.0f);
        ASFixed zero = FloatToASFixed(0.0);

        // Now we want to define how the shapes will be positioned relative to each
        //    other, by making position deltas (or vectors, if you prefer) for each.
        //    They'll come out in a nice triangle.

        //                                       shape 1                      shape 2  shape 3
        ASFixed delta_x[NUM_BLENDING_SHAPES] = {radius - threeFourthsRadius, zero, (radius / 2) - threeFourthsRadius};
        ASFixed delta_y[NUM_BLENDING_SHAPES] = {zero, zero, -radius};

        // Now we can create the basic shape's prototype.
        // We'll put our circle into this content object, which will be converted into a PDEForm object.
        PDEContent singleShapeContent = PDEContentCreate();

        // Make a circle shape.
        PDEPath shapePath = PDEPathCreate();
        ASFixed handleLength = FloatToASFixed(ASFixedToFloat(circleDiameter) * 0.66666);
        PDEPathSetPaintOp(shapePath, kPDEFill);
        PDEPathAddSegment(shapePath, kPDEMoveTo, -radius, 0, 0, 0, 0, 0);
        PDEPathAddSegment(shapePath, kPDECurveTo, -radius, handleLength, radius, handleLength, radius, 0);
        PDEPathAddSegment(shapePath, kPDECurveTo, radius, -handleLength, -radius, -handleLength, -radius, 0);

        // Give the circle its graphics state. This graphics state will be re-used for the triads.
        PDEGraphicState shapeGState;
        PDEDefaultGState(&shapeGState, sizeof(PDEGraphicState));
        // We haven't added anything special to this graphics state yet.
        shapeGState.wasSetFlags = 0;
        PDEElementSetGState((PDEElement)shapePath, &shapeGState, sizeof(shapeGState));

        // The circle is complete. Add it to our content object.
        PDEContentAddElem(singleShapeContent, kPDEBeforeFirst, (PDEElement)shapePath);
        PDERelease((PDEObject)shapePath);

        // Convert the content containing our circle into a PDEForm.
        PDDoc pdoc = doc.getPDDoc();
        PDEForm singleShape = contentToForm(singleShapeContent, 1, pdoc); // This is the basic shape prototype.
        PDERelease((PDEObject)singleShapeContent);

        // Step 3) Create a prototype for the CMYK and RGB triads by triplicating the basic shape.

        // These containers will hold the shapes the CMYK triad and the RGB triad.
        //   They'll also be converted to PDEForms to create our prototypes.
        PDEContent cmykContent = PDEContentCreate();
        PDEContent rgbContent = PDEContentCreate();

        PDEContent content[2] = {cmykContent, rgbContent};
        PDEContent nextContent;
        for (int i = 0; i < 2; i++) {
            nextContent = content[i];
            // Set the color space of the next triad.
            const char *colorSpace = (nextContent == cmykContent ? "DeviceCMYK" : "DeviceRGB");
            shapeGState.fillColorSpec.space = PDEColorSpaceCreateFromName(ASAtomFromString((colorSpace)));
            shapeGState.wasSetFlags = kPDEFillCSpaceWasSet;

            // Give each triad three shapes of different colors, positioned as specified in delta_x and delta_y.
            for (int i = 0; i < NUM_BLENDING_SHAPES; ++i) {
                // Clone the basic shape prototype.
                PDEForm nextShape = PDEFormCreateClone(singleShape);

                // Set the position for this shape.
                ASFixedMatrix shapePosition = {fixedOne, 0, 0, fixedOne, 0, 0};
                shapePosition.h = delta_x[i];
                shapePosition.v = delta_y[i];

                // Set the graphics state of this shape.
                //    The conditional values are used to easily create three shapes of different colors.
                shapeGState.fillColorSpec.value.color[0] = (i == 0 ? fixedOne : fixedZero); // Red/Cyan value.
                shapeGState.fillColorSpec.value.color[1] = (i == 1 ? fixedOne : fixedZero); // Green/Magenta value.
                shapeGState.fillColorSpec.value.color[2] = (i == 2 ? fixedOne : fixedZero); // Blue/Yellow value.
                shapeGState.fillColorSpec.value.color[3] = fixedZero; // Not using CMYK's Black
                shapeGState.wasSetFlags |= kPDEFillCValueWasSet;

                // Set the position and graphics state of this triad.
                PDEElementSetMatrix((PDEElement)nextShape, &shapePosition);
                PDEElementSetGState((PDEElement)nextShape, &shapeGState, sizeof(shapeGState));

                // Add the triad to its content.
                PDEContentAddElem(nextContent, kPDEBeforeFirst, (PDEElement)nextShape);

                PDERelease((PDEObject)nextShape);
            }
            PDERelease((PDEObject)shapeGState.fillColorSpec.space);
            PDERelease((PDEObject)shapeGState.extGState);
        }

        PDERelease((PDEObject)singleShape);

        // The RGB triad prototype.
        PDEForm rgbTriad = contentToForm(rgbContent, 1, pdoc);
        PDERelease((PDEObject)rgbContent);

        // The CMYK triad prototype.
        PDEForm cmykTriad = contentToForm(cmykContent, 1, pdoc);
        PDERelease((PDEObject)cmykContent);

        // Step 4) Copy the triads to each page, with a different blending modes each time.

        for (int i = 0; i < numPages; ++i) {
            PDPage outPage = doc.getPage(i);
            PDEContent pagecontent = PDPageAcquirePDEContent(outPage, 0);

            // The first blend mode, "Opaque", is not actually a blend mode,
            //    but is used to demonstrate the absence of blend modes.
            if (i != 0) {
                // The PDEExtGState determines the transparency and blending mode
                //   of whatever PDEGraphicsState object it is set to.
                PDEExtGState shapeExtGState = PDEExtGStateCreateNew(PDDocGetCosDoc(pdoc));
                // Each shape will have 3/4 transparency.
                PDEExtGStateSetOpacityFill(shapeExtGState, fixedThreeQuarters);
                // This will correspond to the title on the page.
                PDEExtGStateSetBlendMode(shapeExtGState, ASAtomFromString(vBlendModes[i].c_str()));

                shapeGState.extGState = shapeExtGState;
                shapeGState.wasSetFlags |= kPDEExtGStateWasSet;
            }

            PDEElementSetGState((PDEElement)rgbTriad, &shapeGState, sizeof(PDEGraphicState));
            PDEElementSetGState((PDEElement)cmykTriad, &shapeGState, sizeof(PDEGraphicState));

            // Position and set the RGB triad.
            ASFixedMatrix rgbPosition = {fixedOne, 0, 0, fixedOne, 0, 0};
            rgbPosition.h = rightHalfCenter_X;
            rgbPosition.v = rightHalfCenter_Y;

            PDEElementSetMatrix((PDEElement)rgbTriad, &rgbPosition);
            PDEContentAddElem(pagecontent, kPDEBeforeFirst, (PDEElement)rgbTriad);

            // Position and set the CMYK triad.
            ASFixedMatrix cmykPosition = {fixedOne, 0, 0, fixedOne, 0, 0};
            cmykPosition.h = leftHalfCenter_X;
            cmykPosition.v = leftHalfCenter_Y;

            PDEElementSetMatrix((PDEElement)cmykTriad, &cmykPosition);
            PDEContentAddElem(pagecontent, kPDEBeforeFirst, (PDEElement)cmykTriad);

            // Set all the new content into the page and release resources.
            PDPageSetPDEContentCanRaise(outPage, 0);
            PDPageReleasePDEContent(outPage, 0);
            PDPageRelease(outPage);
            PDERelease((PDEObject)shapeGState.extGState);
        }

        // Now we're done with everything except for the document itself.
        PDERelease((PDEObject)rgbTriad);
        PDERelease((PDEObject)cmykTriad);
        PDERelease((PDEObject)shapeGState.strokeColorSpec.space);
        PDERelease((PDEObject)shapeGState.fillColorSpec.space);

        doc.saveDoc(csOutputFileName.c_str());

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
};

// Helper function to transform the supplied PDEContent into a PDEForm.
//     NOTE:  The caller is responsible for releasing the input content.
PDEForm contentToForm(PDEContent content, ASInt32 formType, PDDoc document) {
    // Determine the attributes of our input content.
    PDEContentAttrs contentAttrs;
    memset((char *)&contentAttrs, 0, sizeof(PDEContentAttrs));
    contentAttrs.formType = formType;

    // The bounding box of our output PDEForm will match the bounding box of our input PDEContent.
    PDEElementGetBBox((PDEElement)content, &contentAttrs.bbox);
    contentAttrs.matrix.a = contentAttrs.matrix.d = fixedOne;

    // Convert the PDEContent into CosObjects.
    CosObj cosContent, cosResources;
    PDEContentToCosObj(content, kPDEContentToForm, &contentAttrs, sizeof(PDEContentAttrs),
                       PDDocGetCosDoc(document), NULL, &cosContent, &cosResources);

    // Convert the CosObjects into a PDEForm.
    ASDoubleMatrix unity = {1.0, 0, 0, 1.0, 0, 0};
    return PDEFormCreateFromCosObjEx(&cosContent, &cosResources, &unity);
}
