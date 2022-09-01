//
// Copyright (c) 2010-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample shows how to create a transparency within a PDF document, in the form of a graphic
// image with an art graphic layered on top. PDF files can have objects that are partially or fully
// transparent, and thus can blend in various ways with objects behind them. Transparent graphics
// or images can be stacked in a PDF file, with each one blending to contribute to the final result
// that appears on the page. In this sample the text behind the image is partially visible.
//
// The PDFEdit Layer (PDE) of the Adobe Acrobat API contains classes that provide a means for editing
// objects in PDF documents, including images. The program creates a PDEImage object, using a JPG image
// with an adjustable compression level. The program also provides for creating a soft mask with the
// transparent object. A SoftMask object in the PDF format allows you to place an image on a PDF
// page and control the level of transparency of that image. You can provide settings to determine
// how much of the background color or text on the page shows through the SoftMask image appearing
// in the foreground.
//
// CreateImageWithTransparency does not define an input file or an input directory.
//
// For more detail see the description of the CreateImageWithTransparency sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#createimagewithtransparency

#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "CosCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define OUTPUT_FILE "CreateImageWithTransparency-out.pdf"

int main(int argc, char *argv[]) {
    // Initialize the library
    APDFLib libInit;
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    const char *HelloWorldStr = "Hello World!";

    std::string csOutputFileName(argc > 1 ? argv[1] : OUTPUT_FILE);
    std::cout << "CreateImageWithTransparency: writing output file " << csOutputFileName.c_str() << std::endl;

    // Create color spaces that will be needed
    PDEColorSpace ImageColorSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));
    PDEColorSpace MaskColorSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceGray"));

    DURING

        // Create an output document
        PDDoc outputPDDoc = PDDocCreate();

        // Create the image data
        PDEImageAttrs attrs;
        memset(&attrs, 0, sizeof(attrs));
        attrs.flags = kPDEImageExternal;
        attrs.height = 288; // in pixels, = 4 in. x 72 pixels/in.
        attrs.width = 648;  // in pixels, = 9 in. x 72 pixels/in.
        attrs.bitsPerComponent = 8;

        // Set the image encoding filter to JPEG
        PDEFilterArray filterArrayImage;
        memset(&filterArrayImage, 0, sizeof(filterArrayImage));
        filterArrayImage.numFilters = 1;
        filterArrayImage.spec[0].name = ASAtomFromString("DCTDecode");

        CosDoc outCosDoc = PDDocGetCosDoc(outputPDDoc);
        CosObj dctParams = CosNewDict(outCosDoc, false, 5);
        CosDictPut(dctParams, ASAtomFromString("Columns"), CosNewInteger(outCosDoc, false, attrs.width));
        CosDictPut(dctParams, ASAtomFromString("Rows"), CosNewInteger(outCosDoc, false, attrs.height));
        CosDictPut(dctParams, ASAtomFromString("Colors"),
                   CosNewInteger(outCosDoc, false, PDEColorSpaceGetNumComps(ImageColorSpace)));

        // The QFactor controls the quality/compression tradeoff.
        // These values describe the approximate image quality:
        // 0.15: Superior
        // 0.3: Excellent
        // 0.5 - 0.7: Very Good
        // 1.3: Fair
        CosDictPut(dctParams, ASAtomFromString("QFactor"), CosNewFixed(outCosDoc, false, FloatToASFixed(1.3)));
        filterArrayImage.spec[0].encodeParms = dctParams;

        // Set the mask encoding filter to Flate
        PDEFilterArray filterArrayMask;
        memset(&filterArrayMask, 0, sizeof(filterArrayMask));
        filterArrayMask.numFilters = 1;
        filterArrayMask.spec[0].name = ASAtomFromString("FlateDecode");

        ASFixedMatrix imageMatrix;
        imageMatrix.a = attrs.width * fixedOne;
        imageMatrix.d = attrs.height * fixedOne;
        imageMatrix.b = imageMatrix.c = 0;
        imageMatrix.h = imageMatrix.v = 0;

        ASInt32 ImageSize = attrs.height * attrs.width * 3;
        ASInt32 MaskSize = attrs.height * attrs.width;
        ASInt32 Line, Row;
        ASUns8 pixelValR = 0, pixelValG = 0, pixelValB = 0;

        ASUns8 *ImageBuffer = new ASUns8[ImageSize];
        ASUns8 *MaskBuffer = new ASUns8[MaskSize];

        // Image data - Create a 9 x 4 inch yellow rectangle
        for (Line = 0; Line < attrs.height; Line++) {
            for (Row = 0; Row < attrs.width; Row++) {
                ASUns8 *Pixel = &ImageBuffer[(Line * attrs.width * 3) + (Row * 3)];
                pixelValR = 0xFF;
                pixelValG = 0xFF;
                Pixel[0] = pixelValR;
                Pixel[1] = pixelValG;
                Pixel[2] = pixelValB;
            }
        }

        // Mask data - Set a 7 x 2 inch hole in the middle of the 9 x 4 inch rectangle.
        // Initially set the mask to all black (on)
        memset(MaskBuffer, 0xFF, MaskSize);

        // Also set a gradation in the middle of the hole.
        ASUns8 pixelVal = 0;
        // Blend color from one inch (72 points) from the bottom to one inch from the top.
        for (Line = 72; Line < attrs.height - 72; Line++) {
            // Blend color from one inch from the left to one inch from the right.
            for (Row = 72; Row < attrs.width - 72; Row++) {
                // From one inch from the left, gradiate from full color (0xFF) to (near) zero color.
                pixelVal = 0xFF - (ASUns8)((Row * 0xFF) / (attrs.width - 72));
                ASUns8 *Pixel = &MaskBuffer[(Line * attrs.width) + (Row)];
                Pixel[0] = pixelVal;
            }
        }

        // Create the PDEImage objects
        PDEImage pdeImageMain =
            PDEImageCreate(&attrs, sizeof(attrs), &imageMatrix, 0, ImageColorSpace, NULL,
                           &filterArrayImage, 0, (unsigned char *)ImageBuffer, ImageSize);

        PDEImage pdeImageMask = PDEImageCreate(&attrs, sizeof(attrs), &imageMatrix, 0, MaskColorSpace, NULL,
                                               &filterArrayMask, 0, (unsigned char *)MaskBuffer, MaskSize);

        // Associate the mask to the image
        PDEImageSetSMask(pdeImageMain, pdeImageMask);

        // Create a PDEText object to set text on the page
        PDEFontAttrs pdeFontAttrs;
        memset(&pdeFontAttrs, 0, sizeof(pdeFontAttrs));
        pdeFontAttrs.name = ASAtomFromString("CourierStd");
        pdeFontAttrs.type = ASAtomFromString("Type1");

        PDSysFont sysFont = PDFindSysFont(&pdeFontAttrs, sizeof(PDEFontAttrs), 0);
        PDEFont pdeFont = PDEFontCreateFromSysFont(sysFont, kPDEFontDoNotEmbed);

        PDEColorSpace pdeColorSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceGray"));
        PDEGraphicState gState; // graphic state to apply to operation
        memset(&gState, 0, sizeof(PDEGraphicState));
        gState.strokeColorSpec.space = gState.fillColorSpec.space = pdeColorSpace;
        gState.miterLimit = fixedTen;
        gState.flatness = fixedOne;
        gState.lineWidth = fixedOne;

        ASFixedMatrix textMatrix;                   // transformation matrix for text
        memset(&textMatrix, 0, sizeof(textMatrix)); // clear structure
        textMatrix.a = Int16ToFixed(24);            // set font width and height
        textMatrix.d = Int16ToFixed(24);            // to 24 point size
        textMatrix.h = Int16ToFixed(0.75 * 72);     // x,y coordinate on page
        textMatrix.v = Int16ToFixed(1.5 * 72);
        PDEText pdeText = PDETextCreate();  // create new text run
        PDETextAdd(pdeText,                 // text container to add to
                   kPDETextRun,             // kPDETextRun, kPDETextChar
                   0,                       // index
                   (Uns8 *)HelloWorldStr,   // text to add
                   strlen(HelloWorldStr),   // length of text
                   pdeFont,                 // font to apply to text
                   &gState, sizeof(gState), // graphic state to apply to text
                   NULL, 0,                 // text state and size of structure
                   &textMatrix,             // transformation matrix for text
                   NULL);                   // stroke matrix

        // Create the output page, 9 x 4 inch
        ASFixedRect pageRect;
        pageRect.left = pageRect.bottom = 0;
        pageRect.right = 72 * fixedNine;
        pageRect.top = 72 * fixedFour;

        PDPage outputPDPage = PDDocCreatePage(outputPDDoc, PDDocGetNumPages(outputPDDoc) - 1, pageRect);
        PDEContent content = PDPageAcquirePDEContent(outputPDPage, 0);

        // Add PDEText and PDEImage objects to page content
        PDEContentAddElem(content, kPDEAfterLast, (PDEElement)pdeText);
        PDEContentAddElem(content, 0, (PDEElement)pdeImageMain);

        PDPageSetPDEContent(outputPDPage, 0);

        // Cleanup
        PDPageReleasePDEContent(outputPDPage, 0);
        PDPageRelease(outputPDPage);

        PDERelease((PDEObject)pdeFont);
        PDERelease((PDEObject)pdeText);
        PDERelease((PDEObject)pdeImageMain);
        PDERelease((PDEObject)pdeImageMask);
        PDERelease((PDEObject)ImageColorSpace);
        PDERelease((PDEObject)MaskColorSpace);
        PDERelease((PDEObject)pdeColorSpace);

        delete[] ImageBuffer;
        delete[] MaskBuffer;

        ASPathName outputPathName = APDFLDoc::makePath(csOutputFileName.c_str());
        PDDocSave(outputPDDoc, PDSaveFull | PDSaveCollectGarbage, outputPathName, 0, 0, 0);
        if (outputPathName)
            ASFileSysReleasePath(0, outputPathName);

        PDDocClose(outputPDDoc);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}
