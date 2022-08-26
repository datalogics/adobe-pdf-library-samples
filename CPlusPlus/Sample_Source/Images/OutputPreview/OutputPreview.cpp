//
// Copyright (c) 2004-2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// The sample demonstrates how to create true color separations and composite images of spot and process colors.
//
// Output Preview is an operation performed in many document creation/maintenance tools. It allows the user to "see" a
// display of a page with selected colorants "turned off".
//
// For more detail see the description of the OutputPreview sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#outputpreview

// This sample reads each page from a document and creates a set of bitmap images for each page.
// The first image shows the entire page in CMYK. The sample then creates separations for the page
// into Cyan, Magenta, Yellow, Black, and spot colors.
//
// NOTE: The sample creates a color separation for each spot color DEFINED in a page, whether or not that
// color is USED in the page. A composite of all of these colorants will be inserted into the output document.
// This will be followed by several images of the composite page with specific colorants turned off.
// Finally, the sample inserts a separation for each colorant, one page per separation, into the output
// document, and includes an indication of how much of the page each colorant covers. The process and spot color
// plates will be rendered in the specified colorant, or its conversion to CMYK, in the case of spot colors.

#include <math.h>
#include <stdio.h>
#include <string>
#include <vector>

#include "PERCalls.h"
#include "PEWCalls.h"
#include "PSFCalls.h"
#include "PagePDECntCalls.h"
#include "DLExtrasCalls.h"
#include "PDPageDrawM.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "CosCalls.h"

// Resolution of image desired for plates.
#define RESOLUTION (300.0)

// Font used to label plates
#define FONT_NAME "MyriadPro-Regular"
#define FONT_TYPE "TrueType"

// Default sample input and output
// (Can be overridden on command line as "CreateSeparations inputfilename outputfilename").
#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "No_Transparency.pdf"
//#define DEF_INPUT "With_Transparency.pdf"
#define DEF_OUTPUT "Out.pdf"

// This is a structure used to carry information between functions, so as to lower the requirements
// for obtaining this info in each function. It carries information about the page currently being
// rendered, as well as information used in rendering
typedef struct PageInfo {
    PDPage page;
    ASUns8 numberOfColorants;
    CosDoc cosDoc;

    // Width and Depth in pixels
    ASInt32 rows;
    ASInt32 cols;
    ASInt32 rowWidth;

    // Referenced from drawParams;
    ASRealRect drawWindow;
    ASRealMatrix drawMatrix;

    // Structure used to control the drawing of the page
    PDPageDrawMParamsRec drawParams;

    // Information about the colors on this page
    std::vector<PDPageInkRec> pageInks;
    std::vector<ASAtom> pageInkNames;
    std::vector<PDEColorSpace> spotColors;
    PDEColorSpace deviceNspace;

    // Information needed to add text to page
    PDEFont textFont;
    PDEGraphicState textGState;
    PDETextState textTState;
    ASDoubleMatrix textMatrix;
    ASDoubleMatrix secondaryTextMatrix;

    PageInfo()
    {
        page = NULL;
        numberOfColorants = 0;
        cosDoc = NULL;
        rows = 0;
        cols = 0;
        rowWidth = 0;
        memset(&drawWindow, 0, sizeof(ASRealRect));
        memset(&drawMatrix, 0, sizeof(ASRealMatrix));
        memset(&drawParams, 0, sizeof(PDPageDrawMParamsRec));
        deviceNspace = NULL;
        textFont = NULL;
        memset(&textGState, 0, sizeof(PDEGraphicState));
        memset(&textTState, 0, sizeof(PDETextState));
        memset(&textMatrix, 0, sizeof(ASDoubleMatrix));
        memset(&secondaryTextMatrix, 0, sizeof(ASDoubleMatrix));
    };
} PageInfo;

void FillPageInfo(PageInfo *pageInfo, PDPage page);
void WriteCMYKImage(PageInfo *pageInfo, PDDoc outputDoc);
void WriteDeviceNImage(PageInfo *pageInfo, PDDoc outputDoc);
void WriteSeparationImage(PageInfo *pageInfo, PDDoc outputDoc, int separationNumber);
void AddImageToDoc(PageInfo *pageInfo, PDDoc outputDoc, PDEColorSpace color, char *buffer,
                   size_t bufferSize, ASInt16 channels, const char *Name, const char *Info = NULL);
ASBool CollectInkInfo(PDPageInk ink, void *clientData);
PDEColorSpace CreateProcessColorSpace(PageInfo *pageInfo, PDPageInk ink);
PDEColorSpace CreateSpotColorSpace(PageInfo *pageInfo, PDPageInk ink);
void ConstructDeviceNColorSpace(PageInfo *pageInfo);
PDEColorSpace ModifyColorSpace(PageInfo *pageInfo, int index);

//
// The main process will initialize the library, open the input document, create the output document,
// and initialize the communication structure. Then, for each page, it will render the page to CMYK, and insert
// this image into the output document, then render to DeviceN, and insert this image into the output several times.
// Finally,  it will separate out the colorants, writing them  one-by-one to the output. It will then release global
// resources, save the output, and terminate the library.
int main(int argc, char *argv[]) {
    APDFLib libInit; // Initialize the Adobe PDF Library. (Going out of scope will terminate.)
    ASErrorCode errCode = 0;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    DURING
        std::string csInputFileName(argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT);
        std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);

        std::cout << "Creating Output Preview for " << csInputFileName.c_str()
                  << " in files starting with " << csOutputFileName.c_str() << std::endl;

        // Open the input as a PDF file
        APDFLDoc papdflDoc(csInputFileName.c_str(), true);
        PDDoc doc = papdflDoc.getPDDoc();

        // Create an output Document
        PDDoc outputDoc = PDDocCreate();

        // Create a page Info Structure, and populate it with page
        // independent information
        PageInfo pageInfo;

        // We create here the structures needed to add text to output pages
        PDEFontAttrs FontAttrs;
        memset(&FontAttrs, 0, sizeof(FontAttrs));
        FontAttrs.name = ASAtomFromString(FONT_NAME);
        FontAttrs.type = ASAtomFromString(FONT_TYPE);
        PDSysFont SysFont = PDFindSysFont(&FontAttrs, sizeof(PDEFontAttrs), 0);
        if (!SysFont) {
            std::cout << "Cannot obtain System Font " << FONT_NAME << " " << FONT_TYPE << std::endl;
            return 1;
        }
        PDSysFontGetAttrs(SysFont, &FontAttrs, sizeof(PDEFontAttrs));
        pageInfo.textFont = PDEFontCreateFromSysFont(SysFont, kPDEFontDoNotEmbed);

        PDEDefaultGState(&pageInfo.textGState, sizeof(PDEGraphicState));
        memset((char *)&pageInfo.textTState, 0, sizeof(PDETextState));

        // Size and horizontal position of plate identifier
        pageInfo.textMatrix.a = pageInfo.textMatrix.d = 62.0;
        pageInfo.textMatrix.b = pageInfo.textMatrix.c = 0.0;
        pageInfo.textMatrix.h = 72.0;

        // Size and horizontal position of Additional Information
        pageInfo.secondaryTextMatrix.a = pageInfo.secondaryTextMatrix.d = 40.0;
        pageInfo.secondaryTextMatrix.b = pageInfo.secondaryTextMatrix.c = 0.0;
        pageInfo.secondaryTextMatrix.h = 72.0;

        // Create separations for each page
        // To the output file, draw a CMYK rendering of the entire file, and
        // a DeviceN Composite rendering, then a separation plate for each colorant.
        for (int page = 0; page < PDDocGetNumPages(doc); page++) {
            // Acquire the page to render
            PDPage pdPage = PDDocAcquirePage(doc, page);

            // Fill in the information needed to render the page
            FillPageInfo(&pageInfo, pdPage);

            // Create, and write to the output document, the CMYK rendering of this page
            WriteCMYKImage(&pageInfo, outputDoc);

            // When a page uses translucent ink, that page may not be accurately rendered as
            // a deviceN bitmap. This is because a page with translucent ink may require an alpha
            // value for each colorant at each pixel, but a deviceN bitmap may have only a single
            // alpha value for all colorants at a given pixel.
            // You can render a page with translucent ink to a CMYK bitmap instead.
            // The example file "With_Transparency.pdf" is the same as "No_Transparency.pdf"
            // except it has translucent ink.
            if (!PDPageHasTransparency(pageInfo.page, false)) {
                // doesn't have translucent ink

                // Create the DeviceN rendering of this page, output to document as composite page,
                // only process colors, and only spot colors.
                WriteDeviceNImage(&pageInfo, outputDoc);

                // Divide the DeviceN image into separations, and write each to the output document
                for (int color = 0; color < pageInfo.numberOfColorants; color++)
                    WriteSeparationImage(&pageInfo, outputDoc, color);

                // Release the buffer for the last bitmap drawn
                if (pageInfo.drawParams.buffer != NULL)
                    ASfree(pageInfo.drawParams.buffer);
            }

            // Release the page.
            PDPageRelease(pdPage);
        }

        // Release the information used for all pages
        PDERelease((PDEObject)pageInfo.textFont);
        PDERelease((PDEObject)pageInfo.textGState.fillColorSpec.space);
        PDERelease((PDEObject)pageInfo.textGState.strokeColorSpec.space);

        // Release the final DeviceN color space
        if (pageInfo.deviceNspace)
            PDERelease((PDEObject)pageInfo.deviceNspace);
        pageInfo.deviceNspace = NULL;

        // Release the final color lists
        pageInfo.pageInks.clear();
        pageInfo.pageInkNames.clear();
        for (ASUns8 count = 0; count < pageInfo.spotColors.size(); count++)
            PDERelease((PDEObject)pageInfo.spotColors[count]);
        pageInfo.spotColors.clear();

        // Save the output document
        ASPathName outputPathName = APDFLDoc::makePath(csOutputFileName.c_str());
        PDDocSave(outputDoc, PDSaveFull | PDSaveCollectGarbage, outputPathName, 0, 0, 0);
        ASFileSysReleasePath(NULL, outputPathName);
        PDDocClose(outputDoc);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return (0);
}

// This will fill in the information needed to render the given page, in the
//  given resolution. The page will always be displayed upright.
void FillPageInfo(PageInfo *pageInfo, PDPage page) {
    // Save the page in the collected information
    pageInfo->page = page;

    // Save the CosDoc, as we will need it in a number of places
    // to build color spaces
    pageInfo->cosDoc = PDDocGetCosDoc(PDPageGetDoc(page));

    // Obtain the page size, and the matrix needed to draw
    // the page from top to bottom
    ASFixedMatrix pageMatrix;
    ASFixedRect pageRect;
    PDPageGetFlippedMatrix(page, &pageMatrix);
    PDPageGetCropBox(page, &pageRect);

    // Convert the size and matrix from
    // ASFixed values to double values
    double left, right, top, bottom;
    left = ASFixedToFloat(pageRect.left);
    right = ASFixedToFloat(pageRect.right);
    top = ASFixedToFloat(pageRect.top);
    bottom = ASFixedToFloat(pageRect.bottom);

    /* Draw Window is the SIZE of the resultant image. we want it
    ** always to be based at (0,0), and extend right by width, and up by height
    */
    pageInfo->drawWindow.left = 0;
    pageInfo->drawWindow.bottom = 0;
    pageInfo->drawWindow.top = (ASReal)floor((ASReal)((top - bottom) * RESOLUTION / 72.0) + 0.5);
    pageInfo->drawWindow.right = (ASReal)floor((ASReal)((right - left) * RESOLUTION / 72.0) + 0.5);

    // If the page has a 90 or 270 degree rotation,
    // we need to swap the width and depth
    ASUns16 rotation = PDPageGetRotate(page);
    if ((rotation == 90) || (rotation == 270)) {
        ASReal save = pageInfo->drawWindow.top;
        pageInfo->drawWindow.top = pageInfo->drawWindow.right;
        pageInfo->drawWindow.right = save;
    }

    // Note above, where we forced top and right to an integer pixel!
    pageInfo->rows = (ASInt32)pageInfo->drawWindow.top;
    pageInfo->cols = (ASInt32)pageInfo->drawWindow.right;

    /* drawMatrix is the piece of the page that we want to draw.
    ** h is set to the left edge of what we want to see (Move right by h, hence
    ** subtract left edge from h). v is set to the bottom of the area we want
    ** to see. Hence move up (-) by amount of bottom
    **
    ** A, B, C, And D are all increased by the scale factor. They can be any values,
    ** as set by the page to reflect the pages rotation. The result will always be an "upright"
    ** image. We draw here from bottom to top, and note in creating the image to be put into
    ** the page that we are drawing from bottom to top
    */
    pageInfo->drawMatrix.a = (ASReal)(RESOLUTION / 72.0 * ASFixedToFloat(pageMatrix.a));
    pageInfo->drawMatrix.b = (ASReal)(RESOLUTION / 72.0 * ASFixedToFloat(pageMatrix.b));
    pageInfo->drawMatrix.c = (ASReal)(RESOLUTION / 72.0 * ASFixedToFloat(pageMatrix.c));
    pageInfo->drawMatrix.d = (ASReal)(RESOLUTION / 72.0 * ASFixedToFloat(pageMatrix.d));
    pageInfo->drawMatrix.tx = (ASReal)(RESOLUTION / 72.0 * ASFixedToFloat(pageMatrix.h));
    pageInfo->drawMatrix.ty = (ASReal)(RESOLUTION / 72.0 * ASFixedToFloat(pageMatrix.v));

    pageInfo->drawParams.size = sizeof(PDPageDrawMParamsRec);
    pageInfo->drawParams.asRealMatrix = &pageInfo->drawMatrix;
    pageInfo->drawParams.asRealDestRect = &pageInfo->drawWindow;
    pageInfo->drawParams.bpc = 8;

    // If we want to control other effects during rendering, it can be done here. In general though
    //   only lazyErase is used.
    pageInfo->drawParams.flags = kPDPageDoLazyErase;

    // When doing separations, we NEVER want Anti-Aliasing!
    pageInfo->drawParams.smoothFlags = 0;

    // Reposition the text to near the top of the page
    pageInfo->textMatrix.v = pageInfo->drawWindow.top - 72.0;

    // Reposition the secondary line to follow the first line
    pageInfo->secondaryTextMatrix.v = pageInfo->textMatrix.v - 62.0;

    // Clear out old color info
    pageInfo->pageInks.clear();
    pageInfo->pageInkNames.clear();
    for (ASUns8 count = 0; count < pageInfo->spotColors.size(); count++)
        PDERelease((PDEObject)pageInfo->spotColors[count]);
    pageInfo->spotColors.clear();

    // Collect the information needed about the inks
    PDPageEnumInks(pageInfo->page, CollectInkInfo, pageInfo, false);
    pageInfo->numberOfColorants = pageInfo->pageInks.size();

    // Discard the old DeviceN Color space, if there is one
    if (pageInfo->deviceNspace)
        PDERelease((PDEObject)pageInfo->deviceNspace);
    pageInfo->deviceNspace = NULL;

    // Construct a color space to show all channels of the
    // DeviceN bitmap.
    ConstructDeviceNColorSpace(pageInfo);

    return;
}

// Draw the page to CMYK, and insert into the destination document.
void WriteCMYKImage(PageInfo *pageInfo, PDDoc outputDoc) {
    // First, draw the image to a buffer using PDDrawContentsToMemoryWithParams
    // All of the params relevant to positioning and scaling the page are already set.
    // Here, we need to set color space to CMYK
    pageInfo->drawParams.csAtom = ASAtomFromString("DeviceCMYK");

    // If a buffer already exists, free it, and null the pointer
    if (pageInfo->drawParams.buffer != NULL)
        ASfree(pageInfo->drawParams.buffer);
    pageInfo->drawParams.buffer = NULL;

    // Call PDPageDrawContentsToMemoryWithParams with a null pointer to the buffer,
    // in order to find the required buffer size
    pageInfo->drawParams.bufferSize =
        PDPageDrawContentsToMemoryWithParams(pageInfo->page, &pageInfo->drawParams);

    // Allocate a buffer of that size
    pageInfo->drawParams.buffer = (char *)ASmalloc(pageInfo->drawParams.bufferSize);

    // Actually draw the page into the buffer.
    pageInfo->drawParams.bufferSize =
        PDPageDrawContentsToMemoryWithParams(pageInfo->page, &pageInfo->drawParams);

    // APDFL will pad each row to an even 32 bit boundary. For this reason, row width may not be equal to
    // pixels wide * channels. However, for CMYK, each pixel is 32 bits, so they are always the same
    pageInfo->rowWidth = pageInfo->cols * 4;

    // This color space is easy, it is just CMYK
    PDEColorSpace cmyk = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceCMYK"));

    // Add the image to the output document
    AddImageToDoc(pageInfo, outputDoc, cmyk, pageInfo->drawParams.buffer,
                  pageInfo->drawParams.bufferSize, 4, "CMYK");

    // Release the color space
    PDERelease((PDEObject)cmyk);

    return;
}

// Create the separations as a DeviceN bitmap
void WriteDeviceNImage(PageInfo *pageInfo, PDDoc outputDoc) {
    // First, draw the image to a buffer using PDDrawContentsToMemoryWithParams
    // All of the params relevant to positioning and scaling the page are already set.
    // Here, we need to set color space to DeviceN
    pageInfo->drawParams.csAtom = ASAtomFromString("DeviceN");

    // If a buffer already exists, free it, and null the pointer
    if (pageInfo->drawParams.buffer != NULL)
        ASfree(pageInfo->drawParams.buffer);
    pageInfo->drawParams.buffer = NULL;

    // reset numberOfColorants to force generation of color list
    pageInfo->numberOfColorants = pageInfo->pageInkNames.size();

    // Set up for color list
    // The easiest way to do this is to NOT provide a colorant list,
    // but instead, allow PDPageDrawContentsToMemory to construct the
    // colorant list. Provide a block of memory large enough to hold
    // 32 colorants (No more than 31 will ever be used).
    //
    // However, if you do this, then you will not be able to control the sequence of colorants
    // in the image, or have access to the list of inks used. So in this example, we will create the
    // ink information in the sample, and provide it to APDFL.
    //
    // CAUTION: APDFL will not support more than 27 spot colors in any DeviceN rendering!
    // If your page contains more spot colorants than this, you will need to create and populate
    // the deviceNColorInks array within your application, and render multiple time, to obtain
    // separations for all colorants.
    // The first 4 inks should always the Cyan, Magenta, Yelow, and Black, in that order.
    pageInfo->drawParams.deviceNColorSize = pageInfo->numberOfColorants;
    pageInfo->drawParams.deviceNColorCount = &pageInfo->numberOfColorants;
    pageInfo->drawParams.deviceNColorInks = &pageInfo->pageInks[0];

    // Call PDPageDrawContentsToMemoryWithParams with a null pointer to the buffer,
    // in order to find the required buffer size
    pageInfo->drawParams.bufferSize =
        PDPageDrawContentsToMemoryWithParams(pageInfo->page, &pageInfo->drawParams);

    // Allocate a buffer of that size
    pageInfo->drawParams.buffer = (char *)ASmalloc(pageInfo->drawParams.bufferSize);

    // Actually draw the page into the buffer.
    pageInfo->drawParams.bufferSize =
        PDPageDrawContentsToMemoryWithParams(pageInfo->page, &pageInfo->drawParams);

    // APDFL will pad each row to an even 32 bit boundary. For this reason, row width may not be
    // equal to pixels wide * channels. Calculate the actual width of a row here
    pageInfo->rowWidth = (((pageInfo->cols * pageInfo->numberOfColorants * 8) + 31) / 32) * 4;

    // Add the image to the output document using a full DeviceN ColorSpace
    AddImageToDoc(pageInfo, outputDoc, pageInfo->deviceNspace, pageInfo->drawParams.buffer,
                  pageInfo->drawParams.bufferSize, pageInfo->numberOfColorants, "Composite DeviceN",
                  "All colorants");

    // Now add the image with a DeviceN color space for just process plates
    PDEColorSpace ModifiedSpace = ModifyColorSpace(pageInfo, 0x0f);
    AddImageToDoc(pageInfo, outputDoc, ModifiedSpace, pageInfo->drawParams.buffer,
                  pageInfo->drawParams.bufferSize, pageInfo->numberOfColorants, "Composite DeviceN",
                  "Only Process Colorants");
    PDERelease((PDEObject)ModifiedSpace);

    // And add another, with a DeviceN color space for just spot color plates
    ModifiedSpace = ModifyColorSpace(pageInfo, 0xfff0);
    AddImageToDoc(pageInfo, outputDoc, ModifiedSpace, pageInfo->drawParams.buffer,
                  pageInfo->drawParams.bufferSize, pageInfo->numberOfColorants, "Composite DeviceN",
                  "Only Spot Colorants");
    PDERelease((PDEObject)ModifiedSpace);

    return;
}

// Separate out one colorant from a DeviceN bitmap into a single plate.
void WriteSeparationImage(PageInfo *pageInfo, PDDoc outputDoc, int separationNumber) {
    /* Allocate a buffer to hold the separation image only */
    char *buffer = (char *)ASmalloc(pageInfo->cols * pageInfo->rows);

    // Keep track of the number of non-zero pixels
    ASSize_t pixelsUsed = 0;

    // Separate out just one colorant
    for (int row = 0; row < pageInfo->rows; row++)
        for (int col = 0; col < pageInfo->cols; col++) {
            buffer[(row * pageInfo->cols) + col] =
                pageInfo->drawParams.buffer[(row * pageInfo->rowWidth) + (col * pageInfo->numberOfColorants) + separationNumber];
            if (buffer[(row * pageInfo->cols) + col] != 0)
                pixelsUsed++;
        }

    // Display the colorant name in the page
    char *colorantName = (char *)ASAtomGetString(pageInfo->pageInkNames[separationNumber]);

    // Display the percentage of coverage of this colorant in the page
    char coverage[100];
    sprintf(coverage, "%0.3g Percent coverage.", (pixelsUsed * 100.0) / (pageInfo->cols * pageInfo->rows * 1.0));

    // Normally, we would create plates for printing by using DeviceGray to render the plate.
    // But for display, we will use the appropriate Separation color space to display the separation.
    PDEColorSpace cs = pageInfo->spotColors[separationNumber];
    AddImageToDoc(pageInfo, outputDoc, cs, buffer, (pageInfo->cols * pageInfo->rows), 1, colorantName, coverage);

    // Free the separation bitmap
    ASfree(buffer);

    return;
}

// Add a bitmap to the output document, as a single page, the size of the image, in that document.
void AddImageToDoc(PageInfo *pageInfo, PDDoc outputDoc, PDEColorSpace space, char *buffer,
                   size_t bufferSize, ASInt16 channels, const char *Name, const char *Info) {
    // If the image row size is greater than the packed row size,
    // pack the image here.
    // Since it will always be smaller than the original image, we can pack it in place
    size_t packedRow = pageInfo->cols * channels;

    if ((channels > 1) && (packedRow != pageInfo->rowWidth)) {
        for (int row = 1; row < pageInfo->rows; row++)
            memmove((char *)&buffer[row * packedRow], (char *)&buffer[row * pageInfo->rowWidth], packedRow);

        // Update image and row width to reflect packing
        bufferSize = packedRow * pageInfo->cols;
        if (buffer == pageInfo->drawParams.buffer)
            pageInfo->drawParams.bufferSize = bufferSize;
        pageInfo->rowWidth = packedRow;
    }

    // Convert the bitmap to a PDEImage
    PDEImageAttrs attrs;
    memset(&attrs, 0, sizeof(attrs));
    attrs.flags = kPDEImageExternal; // Make an XObject image
    attrs.height = pageInfo->rows;   // Image depth in pixels
    attrs.width = pageInfo->cols;    // Image width in pixels
    attrs.bitsPerComponent = 8;      // Bits per channel

    // If the image is for use internal to this process, we may want to avoid compressing it.
    // If it is for use in a document, then we probably should compress.
    // If we do not compress, we can omit this block, and use "NULL" for the
    // pointer to a filter, below
    PDEFilterArray filterArray;
    memset(&filterArray, 0, sizeof(filterArray));
    filterArray.numFilters = 1;
    filterArray.spec[0].name = ASAtomFromString("FlateDecode");

    // This matrix is internal to the image. Not necessarily
    // to it's placement. As written here, it places the images
    // lower left corner at (0, 0), draws the image erect, and
    // scales it at 1 pixel per point.
    ASFixedMatrix imageMatrix;
    imageMatrix.a = ASInt16ToFixed(attrs.width);
    imageMatrix.d = ASInt16ToFixed(attrs.height);
    imageMatrix.b = imageMatrix.c = 0;
    imageMatrix.h = 0;
    imageMatrix.v = 0;

    // Create the image
    PDEImage image = PDEImageCreate(&attrs, sizeof(attrs), &imageMatrix, 0, space, NULL,
                                    &filterArray, 0, (ASUns8 *)buffer, bufferSize);

    // Create a page to hold the image
    // The page will be precisely the size of the image.
    ASFixedRect pageWindow;
    pageWindow.left = FloatToASFixed(pageInfo->drawWindow.left);
    pageWindow.right = FloatToASFixed(pageInfo->drawWindow.right);
    pageWindow.top = FloatToASFixed(pageInfo->drawWindow.top);
    pageWindow.bottom = FloatToASFixed(pageInfo->drawWindow.bottom);
    PDPage outputPage = PDDocCreatePage(outputDoc, PDDocGetNumPages(outputDoc) - 1, pageWindow);

    // Get the pages content
    PDEContent content = PDPageAcquirePDEContent(outputPage, 0);

    // Add the image to it
    PDEContentAddElem(content, 0, (PDEElement)image);

    // Release the image
    PDERelease((PDEObject)image);

    /* Create the line of text to ID the page */
    PDEText text = PDETextCreate();
    PDETextAddEx(text, kPDETextRun, 0, (ASUns8 *)Name, strlen(Name), pageInfo->textFont,
                 &pageInfo->textGState, sizeof(PDEGraphicState), &pageInfo->textTState,
                 sizeof(PDETextState), &pageInfo->textMatrix, NULL);

    // If there is a second information line, add it as well
    if (Info != NULL)
        PDETextAddEx(text, kPDETextRun, 1, (ASUns8 *)Info, strlen(Info), pageInfo->textFont,
                     &pageInfo->textGState, sizeof(PDEGraphicState), &pageInfo->textTState,
                     sizeof(PDETextState), &pageInfo->secondaryTextMatrix, NULL);

    // Add it to the page, over the image
    PDEContentAddElem(content, 1, (PDEElement)text);

    // Release the text
    PDERelease((PDEObject)text);

    // Set the content in the page
    PDPageSetPDEContent(outputPage, 0);

    // Release the page content
    PDPageReleasePDEContent(outputPage, 0);

    // release the page
    PDPageRelease(outputPage);

    return;
}

ASBool CollectInkInfo(PDPageInk ink, void *clientData) {
    PageInfo *pageInfo = (PageInfo *)clientData;

    // It is possible to have multiple definitions for the same
    // named colorant. When that happens here, we will discard
    // all but the first such ink. All inks of a given name will
    // be rendered to the same plate, regardless of which definition
    // for the name is used
    for (ASUns8 count = 0; count < pageInfo->pageInkNames.size(); count++)
        if (pageInfo->pageInkNames[count] == ink->colorantName)
            return (true);

    pageInfo->pageInkNames.push_back(ink->colorantName);
    pageInfo->pageInks.push_back(*ink);

    if (ink->isProcessColor)
        pageInfo->spotColors.push_back(CreateProcessColorSpace(pageInfo, ink));
    else
        pageInfo->spotColors.push_back(CreateSpotColorSpace(pageInfo, ink));

    return (true);
}

// Create the separation color spaces to use for Cyan, Magenta, Yellow, and Black
PDEColorSpace CreateProcessColorSpace(PageInfo *pageInfo, PDPageInk ink) {
    PDEColorSpace result;

    // Set up to create a new separation color space
    PDEColorSpaceStruct colorSpace;
    PDESeparationColorData color;
    colorSpace.sep = &color;

    // Size is a constant, name is the name of the colorant,
    // and the alternate is always DeviceCMYK
    // (Make sure to release this after the color is constructed).
    color.size = sizeof(PDESeparationColorData);
    color.name = ink->colorantName;
    color.alt = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceCMYK"));

    // The tint transform is where things get ugly. There are no high level interfaces
    // to help with this, it must be done completely using the COS level commands

    // Obtain the COS doc to allow for COS creation.
    CosDoc cosDoc = pageInfo->cosDoc;

    // The domain is always a single range, from 0 to 1, and reflects the
    // range of saturation of this colorant
    CosObj cosDomain = CosNewArray(cosDoc, false, 2);
    CosArrayPut(cosDomain, 0, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosDomain, 1, CosNewFloat(cosDoc, false, 1.0));

    // The range is always for 4 process colorants (CMYK), and also from
    // 0 to 1, reflecting the saturation of the alternate space to reflect
    // this colorant.
    CosObj cosRange = CosNewArray(cosDoc, false, 8);
    CosArrayPut(cosRange, 0, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 1, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 2, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 3, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 4, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 5, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 6, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 7, CosNewFloat(cosDoc, false, 1.0));

    // The tint transform is how we convert this colorant into CMYK
    // I am using Postscript Functions here, because they are a simple to
    // understand means of converting the colors
    CosObj transformDict = CosNewDict(cosDoc, false, 4);
    CosDictPutKeyString(transformDict, "FunctionType", CosNewInteger(cosDoc, false, 4));
    CosDictPutKeyString(transformDict, "Domain", cosDomain);
    CosDictPutKeyString(transformDict, "Range", cosRange);

    // The difference for each of the process colorants is simply where we put it in the
    // resultant CMYK color. If the colorant is Cyan, we just need to add three zeros
    // after it. If it is Magenta, we need to add a zero before it, and two after, and
    // so on.
    char transformText[200];
    if (ink->colorantName == ASAtomFromString("Cyan"))
        strcpy(transformText, "{0 0 0}");
    if (ink->colorantName == ASAtomFromString("Magenta"))
        strcpy(transformText, "{0 exch 0 0}");
    if (ink->colorantName == ASAtomFromString("Yellow"))
        strcpy(transformText, "{0 exch 0 exch 0}");
    if (ink->colorantName == ASAtomFromString("Black"))
        strcpy(transformText, "{0 exch 0 exch 0 exch}");

    // Build the transform function.
    ASInt32 length = (ASInt32)strlen(transformText);
    ASStm transformStm = ASMemStmRdOpen(transformText, length);
    CosObj transform = CosNewStream(cosDoc, true, transformStm, 0, 0, transformDict, CosNewNull(), length);
    ASStmClose(transformStm);

    // Reference the newly built transform function
    color.tintTransform = transform;

    // Create a separation color space for this colorant
    result = PDEColorSpaceCreate(ASAtomFromString("Separation"), &colorSpace);

    PDERelease((PDEObject)color.alt);
    return (result);
}

// Create spot colors differs from create process colors only in the action of the tint transform
PDEColorSpace CreateSpotColorSpace(PageInfo *pageInfo, PDPageInk ink) {
    PDEColorSpace result;

    // Set up to create a new separation color space
    PDEColorSpaceStruct colorSpace;
    PDESeparationColorData color;
    colorSpace.sep = &color;

    // Size is a constant, name is the name of the colorant,
    // and the alternate is always DeviceCMYK
    // (Make sure to release this after the color is constructed).
    color.size = sizeof(PDESeparationColorData);
    color.name = ink->colorantName;
    color.alt = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceCMYK"));

    // The tint transform is where things get ugly. There are no high level interfaces
    // to help with this, it must be done completely using the COS level commands

    // Obtain the COS doc to allow for COS creation.
    CosDoc cosDoc = pageInfo->cosDoc;

    // The domain is always a single range, from 0 to 1, and reflects the
    // range of saturation of this colorant
    CosObj cosDomain = CosNewArray(cosDoc, false, 2);
    CosArrayPut(cosDomain, 0, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosDomain, 1, CosNewFloat(cosDoc, false, 1.0));

    // The range is always for 4 process colorants (CMYK), and also from
    // 0 to 1, reflecting the saturation of the alternate space to reflect
    // this colorant.
    CosObj cosRange = CosNewArray(cosDoc, false, 8);
    CosArrayPut(cosRange, 0, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 1, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 2, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 3, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 4, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 5, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 6, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 7, CosNewFloat(cosDoc, false, 1.0));

    // The tint transform is how we convert this colorant into CMYK
    // I am using Postscript Functions here, because they are a simple to
    // understand means of converting the colors
    CosObj transformDict = CosNewDict(cosDoc, false, 4);
    CosDictPutKeyString(transformDict, "FunctionType", CosNewInteger(cosDoc, false, 4));
    CosDictPutKeyString(transformDict, "Domain", cosDomain);
    CosDictPutKeyString(transformDict, "Range", cosRange);

    // The PDPageInk record contains the percentage of each process color, at saturation,
    //  needed to create this color. So we need to multiple the value of the color, by each of these
    //  values, to obtain the desired tint in CMYK.
    char baseTransformText[] = "{dup %1.5g mul exch dup %1.5g mul exch dup %1.5g mul exch %1.5g "
                               "mul}";
    char transformText[1024];
    sprintf(transformText, baseTransformText, ink->cyan, ink->magenta, ink->yellow, ink->black);

    // Build the transform function.
    ASInt32 length = (ASInt32)strlen(transformText);
    ASStm transformStm = ASMemStmRdOpen(transformText, length);
    CosObj transform = CosNewStream(cosDoc, true, transformStm, 0, 0, transformDict, CosNewNull(), length);
    ASStmClose(transformStm);

    // Reference the newly built transform function
    color.tintTransform = transform;

    // create a separation color space for this colorant
    result = PDEColorSpaceCreate(ASAtomFromString("Separation"), &colorSpace);

    // Release the alternate space
    PDERelease((PDEObject)color.alt);

    return (result);
}

// Construct a set of DeviceN Color space to show all of the colors in the DeviceN bitmap
void ConstructDeviceNColorSpace(PageInfo *pageInfo) {
    // Set up to create a new DeviceN NChannel color space
    // There is no high level help for this either, so it must be done using COS level commands

    // Obtain the COS doc to allow for COS creation.
    CosDoc cosDoc = pageInfo->cosDoc;

    // create the color space object
    CosObj colorSpace = CosNewArray(cosDoc, true, 5);

    // Identify it as a DeviceN color space
    CosArrayPut(colorSpace, 0, CosNewName(cosDoc, false, ASAtomFromString("DeviceN")));

    // Create the array of names, and add them to the color space
    CosObj names = CosNewArray(cosDoc, false, pageInfo->pageInkNames.size());
    for (ASUns8 count = 0; count < pageInfo->pageInkNames.size(); count++)
        CosArrayPut(names, count, CosNewName(cosDoc, false, pageInfo->pageInkNames[count]));
    CosArrayPut(colorSpace, 1, names);

    // The alternate color space shall be CMYK
    PDEColorSpace cmykSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceCMYK"));
    CosObj cosAltColor;
    PDEColorSpaceGetCosObj(cmykSpace, &cosAltColor);
    CosArrayPut(colorSpace, 2, cosAltColor);

    // The creation of the transform is more than a bit tricky.
    // Because we don't know how many spot colors there are, we cannot use a constant
    // stream, so we need to generate it per page.
    // Because we will be adding each of the spot color conversions into CMYK, it is simpler
    // to isolate these values to a dictionary.

    // The domain is a set of ranges, each 0 to 1, one for each colorant in the page
    CosObj cosDomain = CosNewArray(cosDoc, false, 2 * pageInfo->numberOfColorants);
    for (int count = 0; count < pageInfo->numberOfColorants; count++) {
        CosArrayPut(cosDomain, count * 2, CosNewFloat(cosDoc, false, 0));
        CosArrayPut(cosDomain, (count * 2) + 1, CosNewFloat(cosDoc, false, 1.0));
    }

    // The range is always for 4 process colorants (CMYK), and also from
    // 0 to 1, reflecting the saturation of the alternate space to reflect
    // this colorant.
    CosObj cosRange = CosNewArray(cosDoc, false, 8);
    CosArrayPut(cosRange, 0, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 1, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 2, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 3, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 4, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 5, CosNewFloat(cosDoc, false, 1.0));
    CosArrayPut(cosRange, 6, CosNewFloat(cosDoc, false, 0));
    CosArrayPut(cosRange, 7, CosNewFloat(cosDoc, false, 1.0));

    // The tint transform is how we convert this colorant into CMYK
    // I am using Postscript Functions here, because they are a simple to
    // understand means of converting the colors
    CosObj transformDict = CosNewDict(cosDoc, false, 4);
    CosDictPutKeyString(transformDict, "FunctionType", CosNewInteger(cosDoc, false, 4));
    CosDictPutKeyString(transformDict, "Domain", cosDomain);
    CosDictPutKeyString(transformDict, "Range", cosRange);

    // The PDPageInk record contains the percentage of each process color, at saturation,
    //  needed to create this color. So we need to multiple the value of the color, by each of these
    //  values, to obtain the desired tint in CMYK.
    char transformText[8192] = "";
    char workText[1024];

    // The postscript transform allows only a small set of postscript operators
    // It excludes dictionaries, and arrays. So this will be very obscure, as every operation
    // must be done on the stack.
    //
    // On entry, the stack will contain N numbers, in the order of the inks in the ink list
    // the first 4 will always be CMYK, followed by the spot colors.
    // I will expand each spot color into a CMYK value, on the stack, then add it's 4 components
    // to the base CMYK values. (We cannot do all of the spot colors, then add them, as this would
    // exceed the Postscript Function Stack limit (100) on the 24th spot color).
    ASInt16 numbOfSpots = pageInfo->numberOfColorants - 4;

    if (numbOfSpots == 0) {
        // In the special case where there are no spot colors,
        //  We don't need to do anything here!
        strcpy(transformText, "{}");
    } else {
        // Open the function
        strcat(transformText, "{");

        //   For each colorant, starting with the last spot colorant
        for (int color = pageInfo->numberOfColorants - 1; color > 3; color--) {
            // Multiply this colors density by it's CMYK conversion
            sprintf(workText, "dup %1.5g mul exch dup %1.5g mul exch dup %1.5g mul exch %1.5g mul ",
                    pageInfo->pageInks[color].cyan, pageInfo->pageInks[color].magenta,
                    pageInfo->pageInks[color].yellow, pageInfo->pageInks[color].black);
            strcat(transformText, workText);

            //  The stack contains "C M Y K sp1, sp2, ... spn-1 Cspn Mspn Yspn Kspn"
            //  "4 3 roll" brings Cspn to the top of the stack, "color+4 -1 roll" brings
            // C to the top of the stack. add sums them, and "color+3 1 roll" puts C
            // back at the bottom of the stack. Each process colorant removes one value from
            // the stack, and leaves one unchanged at the bottom of the stack, so each transform
            // has 2 less stack items to deal with.
            //
            // NOTE: This transform is considering a pixel that contains multiple spot or process
            //       colors to be ADDITIVE. To make the last seen spot color opaque, replace the
            //       word "add" with "pop" in each of these 4 transforms. To use a more complex
            //       mixing algorithm here is QUITE difficult. Remember that, since we are in
            //       essence displaying separated data here, we have no alpha value, and no
            //       conception of the sequence colorants were added to the pixels.
            sprintf(workText, "4 -1 roll %01d -1 roll add %01d 1 roll ", color + 4, color + 3);
            strcat(transformText, workText);

            sprintf(workText, "3 -1 roll %01d -1 roll add %01d 1 roll ", color + 2, color + 1);
            strcat(transformText, workText);

            sprintf(workText, "2 -1 roll %01d -1 roll add %01d 1 roll ", color, color - 1);
            strcat(transformText, workText);

            sprintf(workText, "%01d -1 roll add %01d 1 roll ", color - 2, color - 3);
            strcat(transformText, workText);
        }

        // Validate that all 4 colors are less than 1.0
        // Since the above transform was additive, it is possible to get values
        // larger then 1.0 for any given process colorant. This set of transforms
        // limits the value to 1.0
        sprintf(workText, "dup 1.0 gt{ 1.0 exch pop } if 4 1 roll ");
        strcat(transformText, workText);
        sprintf(workText, "dup 1.0 gt{ 1.0 exch pop } if 4 1 roll ");
        strcat(transformText, workText);
        sprintf(workText, "dup 1.0 gt{ 1.0 exch pop } if 4 1 roll ");
        strcat(transformText, workText);
        sprintf(workText, "dup 1.0 gt{ 1.0 exch pop } if 4 1 roll ");
        strcat(transformText, workText);

        // close the function
        strcat(transformText, "}");
    }

    // Build the transform function.
    ASInt32 length = (ASInt32)strlen(transformText);
    ASStm transformStm = ASMemStmRdOpen(transformText, length);
    CosObj transform = CosNewStream(cosDoc, true, transformStm, 0, 0, transformDict, CosNewNull(), length);
    ASStmClose(transformStm);

    // Put the transform into the color space
    CosArrayPut(colorSpace, 3, transform);

    // Create the PDEColor space
    pageInfo->deviceNspace = PDEColorSpaceCreateFromCosObj(&colorSpace);

    // Release the alternate color space
    PDERelease((PDEObject)cmykSpace);

    return;
}

// This routine is included to allow the user to create a new color space,
// which shows only "some of" the colorants. It can be used with the full
// DeviceN bitmap to show only some colorants.
//
//  The "index" value here is a bitwise collection of on/off values for each colorant.
//  the first colorant is bit zero, the second bit one, and so on. The colorants which
//  correspond to bits which are On will be displayed, those corresponding to bits which
//  off will not be displayed.
PDEColorSpace ModifyColorSpace(PageInfo *pageInfo, int index) {
    CosDoc cosDoc = pageInfo->cosDoc;
    CosObj oldSpace;
    PDEColorSpaceGetCosObj(pageInfo->deviceNspace, &oldSpace);
    CosObj newSpace = CosObjCopy(oldSpace, cosDoc, false);

    CosObj inkList = CosArrayGet(newSpace, 1);

    size_t mask = 1;
    for (int count = 0; count < pageInfo->numberOfColorants; count++) {
        if (!(index & mask)) {
            // Remove this colorant
            CosArrayPut(inkList, count, CosNewName(cosDoc, false, ASAtomFromString("None")));
        }
        mask = mask << 1;
    }

    return (PDEColorSpaceCreateFromCosObj(&newSpace));
}
