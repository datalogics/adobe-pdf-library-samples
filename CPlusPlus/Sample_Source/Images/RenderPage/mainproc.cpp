//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// Sample: RenderPage - Demonstrates the process of rasterizing a PDF page
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

#define RESOLUTION  150.0         // Other common choices might be 72.0, 150.0, 200.0, 300.0, or 600.0  
#define COLORSPACE  "DeviceRGB"   // Typically this, DeviceGray or DeviceCMYK
#define FILTER      "FlateDecode" // Could also be ASCIIHexDecode, LZWDecode, DCTDecode
#define BPC         8             // This must be 8 for DeviceRGB and DeviceCYMK, 
                                  //  1, 8, or 24 for DeviceGray

int main(int argc, char** argv)
{
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false)
    {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }
    
    std::string csInputFileName ( argc > 1 ? argv[1] : DIR_LOC DEF_INPUT );
    std::string csOutputFileName ( argc > 2 ? argv[2] : DEF_OUTPUT );
    std::cout << "Rendering " << csInputFileName.c_str() << " to " << csOutputFileName.c_str()
              << " with " << std::endl << " Resolution of " << RESOLUTION << ", Colorspace " 
              << COLORSPACE << ", Filter " << FILTER << ", and BPC " << BPC << std::endl;

DURING

    // Open the input document and get the first page
    APDFLDoc inDoc ( csInputFileName.c_str(), true);
    PDPage pdPage = inDoc.getPage(0);

    // Construction of the drawPage object does all the work to rasterize the page
    RenderPage drawPage(pdPage, COLORSPACE, FILTER, BPC, RESOLUTION); 

    APDFLDoc outDoc;
    PDPage outputPDPage = PDDocCreatePage(outDoc.getPDDoc(), PDBeforeFirstPage, drawPage.GetImageRect());
    PDEContent content = PDPageAcquirePDEContent(outputPDPage, 0);

    // The call to MakePDEImage synthesizes a PDEImage object from the rasterized PDF page
    // created in the constructor, suitable for placing onto a PDF page.
    PDEContentAddElem(content, 0, (PDEElement) drawPage.MakePDEImage() );
    PDPageSetPDEContentCanRaise(outputPDPage, 0);
    PDPageReleasePDEContent(outputPDPage, 0);

    outDoc.saveDoc ( csOutputFileName.c_str(), PDSaveFull | PDSaveCollectGarbage);

    PDPageRelease(outputPDPage);
    PDPageRelease(pdPage);
    
HANDLER
    errCode = ERRORCODE;
    libInit.displayError(errCode);
END_HANDLER
 
    return errCode;
}
