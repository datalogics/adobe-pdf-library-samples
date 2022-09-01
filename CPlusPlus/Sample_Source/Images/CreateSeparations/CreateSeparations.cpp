//
// Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// The sample demonstrates how to create color separations for spot color images.
//
// Color separation is part of high volume offset printing processing. The original digital content
// is color separated to create a set of plates for printing, generally one plate per page for each
// of the primary colors—Cyan, Magenta, Yellow, and Black (CMYK). During printing each color layer is
// printed separately, one on top of the other, blended together to create the depth and variety of
// color in the final images. A spot color is a separate color added on top of the image after the
// four color plates are used to create the initial print run.
//
// For more detail see the description of the CreateSeparations sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#createseparations

// This sample will read each page from a document, and create a set of bitmap images for each page.
// the first image will show the entire page, in CMYK. It will then create separations for the page
// into Cyan, Magenta, Yellow, Black, and spot colors. NOTE: separation will be created for each
// spot color DEFINED in a page, Whether or not that color is USED in the page. All of these images
// will be inserted, one page per image, into the output document created. The process and spot
// color plated will be rendered visually in DeviceGray.
#include <string>

#include "PERCalls.h"
#include "PEWCalls.h"
#include "PSFCalls.h"
#include "PagePDECntCalls.h"
#include "DLExtrasCalls.h"
#include "PDPageDrawM.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "CosCalls.h"
#include <math.h>

// Resolution of image desired for plates.
#define RESOLUTION (300.0)

// Font used to label plates
#define FONT_NAME "MyriadPro-Regular"
#define FONT_TYPE "TrueType"

// default sample input and output
// (Can be overridden on command line as "CreateSeparations inputfilename outputfilename").
#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "RenderPage.pdf"
#define DEF_OUTPUT "Out.pdf"

// This is a structure used to carry information between functions, so as to lower the requirements
// for obtaining this info in each function. It carries information about the page currently being
// rendered, as well as information used in rendering
typedef struct pageInfo {
    PDPage page;
    ASUns8 numberOfColorants;

    // Width and Depth in pixels
    ASInt32 rows;
    ASInt32 cols;
    ASUns32 rowWidth;

    // Referenced from drawParams;
    ASRealRect drawWindow;
    ASRealMatrix drawMatrix;

    // Structure used to control the drawing of the page
    PDPageDrawMParamsRec drawParams;

    // Information needed to add text to page
    PDEFont textFont;
    PDEGraphicState textGState;
    PDETextState textTState;
    ASDoubleMatrix textMatrix;
} PageInfo;

void FillPageInfo(PageInfo *pageInfo, PDPage page);
void WriteCMYKImage(PageInfo *pageInfo, PDDoc outputDoc);
void WriteDeviceNImage(PageInfo *pageInfo, PDDoc outputDoc);
void WriteSeparationImage(PageInfo *pageInfo, PDDoc outputDoc, int separationNumber);
void AddImageToDoc(PageInfo *pageInfo, PDDoc outputDoc, PDEColorSpace color, char *buffer,
                   size_t bufferSize, ASInt16 channels, const char *Name, ASBool invertColor = false);
//
// The main process will intialize the library, open the input document, create the output document,
// and initialize the communication structure. Then, for each page, it will render the page to CMYK, and insert
// this image into the output document, then render to DeviceN, and separate out the colorants, writing them
// one-by-one to the output. It will then release global resources, save the output, and terminate the library.
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

        std::cout << "Creating separations for " << csInputFileName.c_str()
                  << " in files starting with " << csOutputFileName.c_str() << std::endl;

        // Open the input as a PDF file
        APDFLDoc papdflDoc(csInputFileName.c_str(), true);
        PDDoc doc = papdflDoc.getPDDoc();

        // Create an output Document
        PDDoc outputDoc = PDDocCreate();

        // Create a page Info Structure, and populate it with page
        // independent information
        PageInfo pageInfo;
        memset((char *)&pageInfo, 0, sizeof(PageInfo));

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

        pageInfo.textMatrix.a = pageInfo.textMatrix.d = 62.0;
        pageInfo.textMatrix.b = pageInfo.textMatrix.c = 0.0;
        pageInfo.textMatrix.h = 72.0;

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

            // Create the DeviceN rendering of this page
            WriteDeviceNImage(&pageInfo, outputDoc);

            // Divide the DeviceN image into separations, and write each to the output document
            for (int color = 0; color < pageInfo.numberOfColorants; color++)
                WriteSeparationImage(&pageInfo, outputDoc, color);

            // Release the page.
            PDPageRelease(pdPage);
        }

        // Release the information used for all pages
        PDERelease((PDEObject)pageInfo.textFont);
        PDERelease((PDEObject)pageInfo.textGState.fillColorSpec.space);
        PDERelease((PDEObject)pageInfo.textGState.strokeColorSpec.space);

        // Release the buffer for the last bitmap drawn
        if (pageInfo.drawParams.buffer != NULL)
            ASfree(pageInfo.drawParams.buffer);

        // Release the ink list for the last DeviceN rendering
        if (pageInfo.drawParams.deviceNColorInks)
            ASfree(pageInfo.drawParams.deviceNColorInks);

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

    // If we want to control other effects during rendering, it can be done here.
    pageInfo->drawParams.flags = kPDPageDoLazyErase | kPDPageUseAnnotFaces;

    // When doing separations, we NEVER want Anti-Aliasing!
    pageInfo->drawParams.smoothFlags = 0;

    // Reposition the text to near the top of the page
    pageInfo->textMatrix.v = pageInfo->drawWindow.top - 72.0;

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
                  pageInfo->drawParams.bufferSize, 4, const_cast<char*>("CMYK"));

    // Release the color space
    PDERelease((PDEObject)cmyk);

    return;
}

// Create the separations as a DeviceN bitmap
void WriteDeviceNImage(PageInfo *pageInfo, PDDoc outputDoc) {
    // First, draw the image to a buffer using PDDrawContentsToMemoryWithParams
    // All of the params relevant to positioning and scaling the page are already set.
    // Here, we need to set color space to CMYK
    pageInfo->drawParams.csAtom = ASAtomFromString("DeviceN");

    // If a buffer already exists, free it, and null the pointer
    if (pageInfo->drawParams.buffer != NULL)
        ASfree(pageInfo->drawParams.buffer);
    pageInfo->drawParams.buffer = NULL;

    // reset numberofColorants to force generation of color list
    pageInfo->numberOfColorants = 0;

    // Set up for color list
    // The easiest way to do this is to NOT provide a colorant list,
    // but instead, allow PDPageDrawContentsToMemory to construct the
    // colorant list. Provide a block of memory large enough to hold
    // 32 colorants (No more than 31 will ever be used).
    //
    // CAUTION: APDFL will not support more than 27 spot colors in any DeviceN rendering!
    // If your page contains more spot colorants than this, you will need to create and populate
    // the DeviceNColorInks array within your application, and render multiple times, to obtain
    // separations for all colorants.
    // The first 4 inks should always the Cyan, Magenta, Yelow, and Black, in that order.
    pageInfo->drawParams.deviceNColorSize = 32;
    pageInfo->drawParams.deviceNColorCount = &pageInfo->numberOfColorants;
    if (!pageInfo->drawParams.deviceNColorInks)
        pageInfo->drawParams.deviceNColorInks =
            (PDPageInk)ASmalloc(pageInfo->drawParams.deviceNColorSize * sizeof(PDPageInkRec));

    // Call PDPageDrawContentsToMemoryWithParams with zeros for the ink information.
    // This will cause ink information for all inks to be added
    PDPageDrawContentsToMemoryWithParams(pageInfo->page, &pageInfo->drawParams);

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

    // NOTE: We do not here render the DeviceN bitmap to an image. The construction of the DeviceN color space needed to do so
    // is quite complex, and for prepartion of printing plates, this is not needed!. If you do wish to directly render this
    // bitmap, in "true colors", see the example "OutputPreview" which contains a sample for creating such color spaces.

    return;
}

// Separate out one colorant from a DeviceN bitmap into a single plate.
void WriteSeparationImage(PageInfo *pageInfo, PDDoc outputDoc, int separationNumber) {
    /* Allocate a buffer to hold the separation image only */
    char *buffer = (char *)ASmalloc(pageInfo->cols * pageInfo->rows);

    // Separate out just one colorant
    for (int row = 0; row < pageInfo->rows; row++)
        for (int col = 0; col < pageInfo->cols; col++)
            buffer[(row * pageInfo->cols) + col] =
                pageInfo->drawParams.buffer[(row * pageInfo->rowWidth) + (col * pageInfo->numberOfColorants) + separationNumber];

    // Display the colorant name in the page
    char *colorantName =
        (char *)ASAtomGetString(pageInfo->drawParams.deviceNColorInks[separationNumber].colorantName);

    // Normally, we would create plates for printing by using DeviceGray to render the plate.
    PDEColorSpace gray = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceGray"));
    AddImageToDoc(pageInfo, outputDoc, gray, buffer, (pageInfo->cols * pageInfo->rows), 1, colorantName, true);
    PDERelease((PDEObject)gray);
    ASfree(buffer);
}

// Add a bitmap to the output document, as a single page, the size of the image, in that document.
void AddImageToDoc(PageInfo *pageInfo, PDDoc outputDoc, PDEColorSpace space, char *buffer,
                   size_t bufferSize, ASInt16 channels, const char *Name, ASBool invertColor) {
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

    // Normally, separation colors are additive, so "0" is white, and "1" is
    //   100% saturation of the colorant. However, when we display a separation
    //   using DeviceGray, we need to invert this, since DeviceGray is subtractive
    //   We do this here, by manipulating the color decode array
    if (invertColor) {
        attrs.decode[0] = fixedOne;
        attrs.decode[1] = 0;
        attrs.flags |= kPDEImageHaveDecode;
    }

    // If the image is for use internal to this process, we may want to avoid compressing it
    // If it is for use in a document, then we probably should compress.
    // If we do not compress, we can omit this block, and use "NULL" for the
    // pointer to a filter, below
    PDEFilterArray filterArray;
    memset(&filterArray, 0, sizeof(filterArray));
    filterArray.numFilters = 1;
    filterArray.spec[0].name = ASAtomFromString("FlateDecode");

    // This matrix is internal to the image. Not neccesarily
    // to its placement. As written here, it places the images
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
