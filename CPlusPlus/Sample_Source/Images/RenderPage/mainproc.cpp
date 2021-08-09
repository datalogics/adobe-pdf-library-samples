//
// Copyright (c) 2007-2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// Sample: RenderPage - Demonstrates the process of rasterizing the cropped area of a PDF page
//   and placing the resulting raster as an image into a different PDF document.
//
//  Command-line:    <input-file>  <output-file-prefix>      (Both are optional)
//

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"

#include "RenderPage.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "RenderPage.pdf"
#define DEF_OUTPUT "RenderPage-out.pdf"

#define RESOLUTION 300.0 // Other common choices might be 72.0, 150.0, 200.0, 300.0, or 600.0
#define FILTER "FlateDecode"
#define BPC 8 // This must be 8 for DeviceRGB and DeviceCMYK, 1, 8, or 24 for DeviceGray
#define COLORSPACE "DeviceRGB" // Typically this, DeviceGray, or DeviceCMYK
// DeviceRGBA is a special colorspace known to PDFL Rendering that can be used to produce a RGBA Bitmap (RGB + Alpha or 32-bit),
// which is output in this sample as a RGB Image with a Soft Mask set on the page of a PDF
// DeviceCMYKA is a special colorspace known to PDFL Rendering that can be used to produce a CMYKA Bitmap (CMYK + Alpha or 40-bit)
// which is output in this sample as a CMYK Image with a Soft Mask set on the page of a PDF

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;

    // Initialize the library
    APDFLib libInit;

    // If library in initialization failed.
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    // Report on action to be taken
    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Rendering " << csInputFileName.c_str() << " to " << csOutputFileName.c_str()
              << " with " << std::endl
              << " Resolution of " << RESOLUTION << ", Colorspace " << COLORSPACE << ", Filter "
              << FILTER << ", and BPC " << BPC << std::endl;

    DURING

        // Open the input document and acquire the first page
        APDFLDoc inDoc(csInputFileName.c_str(), true);
        PDPage pdPage = inDoc.getPage(0);

        // Construction of the drawPage object does all the work to rasterize the page
        RenderPage drawPage(pdPage, COLORSPACE, FILTER, BPC, RESOLUTION);

        // Release the first page.
        PDPageRelease(pdPage);

        // Create a document to hold the rendered page
        APDFLDoc outDoc;

        // Get the size of the image, in PDF Units
        ASFixedRect imageSize = drawPage.GetImageSize();

        // Construct a page to contain the image, exactly the size of the image.
        PDPage outputPDPage = PDDocCreatePage(outDoc.getPDDoc(), PDBeforeFirstPage, imageSize);

        // Acquire the PDE Content of the newly created page
        PDEContent content = PDPageAcquirePDEContent(outputPDPage, 0);

        // The call to MakePDEImage synthesizes a PDEImage object from the rasterized PDF page
        // created in the constructor, suitable for placing onto a PDF page.
        PDEImage pageImage = drawPage.GetPDEImage(outDoc.getPDDoc());

        // Insert the PDEImage into the page, and release it after it is used.
        PDEContentAddElem(content, 0, reinterpret_cast<PDEElement>(pageImage));

        // Release the PDEImage after it is used in the page
        PDERelease(reinterpret_cast<PDEObject>(pageImage));

        // Set the update PDE Content into the page
        PDPageSetPDEContentCanRaise(outputPDPage, 0);

        // Release the pages PDEContent
        PDPageReleasePDEContent(outputPDPage, 0);

        // Release the newly create page
        PDPageRelease(outputPDPage);

        // Save the outout Document
        outDoc.saveDoc(csOutputFileName.c_str(), PDSaveFull);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
