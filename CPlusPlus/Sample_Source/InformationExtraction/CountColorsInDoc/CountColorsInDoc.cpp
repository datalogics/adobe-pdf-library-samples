//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample reviews a PDF document to determine the distinct colors found,
// and then generates an output text file listing those colors. The program
// identifies colors from either RGB or CYMK color spaces, as well as gray
// scale shading. The program identifies the colors in the PDF document by
// referring to the list of colors defined in the color profile stored within
// the PDF document itself.
//
// This sample demonstrates how to find information in a PDF document,
// and how to access an object within a PDF.
//
// For more detail see the description of the CountColorsInDoc sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#countcolorsindoc

#include <iostream>
#include <fstream>

#include "CosCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "CountColorsInDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "ColorNoColor.pdf"
#define OUTPUT_FILE "CountColorsInDoc-out.txt"

int main(int argc, char *argv[]) {
    // Initialize library
    APDFLib libInit;
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : OUTPUT_FILE);

    std::ofstream ofLog(csOutputFile.c_str());
    ofLog << "Analyzing file " << csInputFile.c_str() << std::endl;
    std::cout << "Analyzing file " << csInputFile.c_str() << std::endl;
    std::cout << "Writing results to " << csOutputFile.c_str() << std::endl;

    DURING

        // Open the input document
        APDFLDoc apdflDoc(csInputFile.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();

        ColorsUsed colorsDoc;

        // For each page in the document, walk content, and summarize colors
        for (int pageNumb = 0; pageNumb < PDDocGetNumPages(pdDoc); pageNumb++) {
            ColorsUsed colorsPage;

            PDPage pdPage = PDDocAcquirePage(pdDoc, pageNumb);
            PDEContent pdeContent = PDPageAcquirePDEContent(pdPage, 0);

            // Walk the content
            WalkPDETree(pdeContent, &colorsPage);
            // Logically combine the page data into the document data
            colorsDoc |= colorsPage;

            // Release resources
            PDPageReleasePDEContent(pdPage, 0);
            PDPageRelease(pdPage);

            ofLog << "Page " << pageNumb << ": " << colorsPage;
        }
        ofLog << "Document summary: " << colorsDoc;

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}

// Summarize one PDE Color spec into flags
void MarkColors(PDEColorSpec *color, ColorsUsed *colors) {
    if (color->value.colorObj) {
        // This is a patterned color space
        CosObj patternCOS;
        PDEPatternGetCosObj((PDEPattern)color->value.colorObj, &patternCOS);
        ASUns32 type = CosIntegerValue(CosDictGetKeyString(patternCOS, "PatternType"));
        if (type == 1) {
            // Tiling pattern
            colors->hadPattern = true;
            ASUns32 paintType = CosIntegerValue(CosDictGetKeyString(patternCOS, "PaintType"));
            if (paintType == 1) {
                // A colored pattern needs to be parsed
                CosObj resource = CosDictGetKeyString(patternCOS, "Resources");
                PDEContent patternContent = PDEContentCreateFromCosObj(&patternCOS, &resource);
                WalkPDETree(patternContent, colors);
                PDERelease((PDEObject)patternContent);
                return;
            }
            return; // An uncolored pattern uses the colors already seen
        } else {
            // Shading pattern
            PDEColorSpec shadeSpec;
            CosObj colorObj;
            colorObj = CosDictGet(patternCOS, ASAtomFromString("ColorSpace"));
            memset((char *)&shadeSpec, 0, sizeof(PDEColorSpec));
            shadeSpec.space = PDEColorSpaceCreateFromCosObj(&colorObj);
            MarkColors(&shadeSpec, colors);
            PDERelease((PDEObject)shadeSpec.space);
            colors->hadShading = true;
            colors->hadNotGray = true;
        }
    } else {
        // For any color, find the number of channels and name
        ASUns16 channels = PDEColorSpaceGetNumComps(color->space);
        ASAtom name = PDEColorSpaceGetName(color->space);

        // If the color is indexed, then note it, and get the base color (and number of channels)
        if (name == ASAtomFromString("Indexed")) {
            colors->hadIndex = true;
            name = PDEColorSpaceGetBase(color->space);
            channels = PDEColorSpaceGetBaseNumComps(color->space);
        }

        static ColorModels sModels;

        // Find the model of this name, and use it to set flags for what has
        // been seen.  Hold on to it for the flags denoting gray and additive.
        const ColorModel &thisColor = sModels.FindModel(name);
        // Mark this color into the summary
        colors->hadGray |= thisColor.mark.hadGray;
        colors->hadRGB |= thisColor.mark.hadRGB;
        colors->hadCMYK |= thisColor.mark.hadCMYK;
        colors->hadLab |= thisColor.mark.hadLab;
        colors->hadICC |= thisColor.mark.hadICC;
        colors->hadDeviceN |= thisColor.mark.hadDeviceN;
        colors->hadSep |= thisColor.mark.hadSep;
        colors->hadCalibrated |= thisColor.mark.hadCalibrated;
        colors->hadUncalibrated |= thisColor.mark.hadUncalibrated;

        // If this is a gray ink, it will never be colored
        if (thisColor.neverColored)
            return;

        // Figuring out if a color specified is a color or a shade of gray can be difficult
        // when working with ICC, DeviceN, and Separation colors. The program creates a
        // single array to hold all color values.
        // The program is not designed to write each color channel to an array one color
        // at a time, looping through the process until the array is filled.

        ASFixed colorValue[20];
        if (color->value.colorObj2 && (name == ASAtomFromString("DeviceN"))) {
            // This color is a DeviceN color space.
            // Not all deviceN color spaces will use this object, but any of them can.
            // When it is used, the color per channel values are stored here, instead of
            // in the array in the color value field.
            colors->hadDeviceN = true;
            for (ASUns16 index = 0; index < channels; index++)
                colorValue[index] =
                    PDEDeviceNColorsGetColorValue((PDEDeviceNColors)color->value.colorObj2, index);
        } else {
            for (ASUns16 index = 0; index < channels; index++)
                colorValue[index] = color->value.color[index];
        }

        // If this is CMYK, ignore the last channel (black)
        if (thisColor.mark.hadCMYK)
            channels -= 1;

        // Program considers any ink drawn as color ink.
        // The sample does not look for and ignore DeviceN channels or separation colors
        // that contain or draw black or white.
        //
        for (ASUns16 index = 0; index < channels; index++) {
            if ((thisColor.additive) && (colorValue[index] != 0))
                colors->hadNotGray = true;
            if ((!thisColor.additive) && (colorValue[index] != fixedOne))
                colors->hadNotGray = true;
        }
    }
}

// Walk through the elements in one content item
void WalkPDETree(PDEContent content, ColorsUsed *colors) {
    // Get the number of elements in this content element
    ASUns16 count = PDEContentGetNumElems(content);

    for (ASUns16 index = 0; index < count; index++) {
        PDEElement elem = PDEContentGetElem(content, index);
        PDEType type = (PDEType)PDEObjectGetType((PDEObject)elem);
        switch (type) {
        case kPDEText: {
            // For a text element, we need to iterate on runs
            PDEGraphicState gState;
            PDETextState tState;
            for (ASUns16 run = 0; run < PDETextGetNumRuns((PDEText)elem); run++) {
                // Each run may draw text stroked, filled, or both
                PDETextGetGState((PDEText)elem, kPDETextRun, run, &gState, sizeof(PDEGraphicState));
                PDETextGetTextState((PDEText)elem, kPDETextRun, run, &tState, sizeof(PDETextState));
                switch (tState.renderMode) {
                case 0:
                case 4:
                    MarkColors(&gState.fillColorSpec, colors);
                    break;
                case 1:
                case 5:
                    MarkColors(&gState.strokeColorSpec, colors);
                    break;
                case 2:
                case 6:
                    MarkColors(&gState.fillColorSpec, colors);
                    MarkColors(&gState.strokeColorSpec, colors);
                    break;
                case 3:
                case 7:
                    break;
                }
            }
        } break;
        case kPDEPath: {
            // The path carries its color in the object itself.
            // The path may be filled stroked, or both.
            PDEGraphicState gState;
            PDEElementGetGState(elem, &gState, sizeof(PDEGraphicState));
            ASUns32 op = PDEPathGetPaintOp((PDEPath)elem);
            if (op & kPDEStroke)
                MarkColors(&gState.strokeColorSpec, colors);
            if (op & (kPDEFill | kPDEEoFill))
                MarkColors(&gState.fillColorSpec, colors);
        } break;
        case kPDEImage: {
            PDEColorSpec imageSpec;
            PDEImageAttrs attrs;
            PDEImageGetAttrs((PDEImage)elem, &attrs, sizeof(PDEImageAttrs));
            imageSpec.space = PDEImageGetColorSpace((PDEImage)elem);
            if (attrs.flags & kPDEImageIsMask)
                PDEImageGetColorValue((PDEImage)elem, &imageSpec.value);
            else {
                // If there is a colored image, presume it is never gray.
                // Setting one channel to maximum, and one to minimum, will cover both the
                // additive and subtractive cases.
                memset(&imageSpec.value, 0, sizeof(PDEColorValue));
                imageSpec.value.color[0] = fixedOne;
                imageSpec.value.color[1] = 0;
            }
            MarkColors(&imageSpec, colors);
        } break;
        case kPDEShading: {
            PDEColorSpec shadeSpec;
            CosObj shadeObj, colorObj;
            PDEShadingGetCosObj((PDEShading)elem, &shadeObj);
            colorObj = CosDictGet(shadeObj, ASAtomFromString("ColorSpace"));
            memset(&shadeSpec.value, 0, sizeof(PDEColorValue));
            shadeSpec.value.color[0] = fixedOne;
            shadeSpec.value.color[1] = 0;
            shadeSpec.space = PDEColorSpaceCreateFromCosObj(&colorObj);
            MarkColors(&shadeSpec, colors);
            PDERelease((PDEObject)shadeSpec.space);
        } break;
        case kPDEForm: {
            // Recurse through the form content
            PDEContent content = PDEFormGetContent((PDEForm)elem);
            WalkPDETree(content, colors);
            PDERelease((PDEObject)content);
        } break;
        case kPDEContainer: {
            // Recurse through the container content
            PDEContent content = PDEContainerGetContent((PDEContainer)elem);
            WalkPDETree(content, colors);
        } break;
        default:
            break;
        }
    }
    return;
}
