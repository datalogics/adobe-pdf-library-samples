//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample program uses the PDFlattener plugin to flatten a PDF input document.
//
// Command-line:  <input-file>  <output-file>       (Both optional)
//
// For more detail see the description of the FlattenTransparency sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#flattentransparency

#include <iomanip>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PagePDECntCalls.h"
#include "PDFlattenerCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "FlattenTransparency.pdf"
#define DEF_OUTPUT "FlattenTransparency-out.pdf"

// ASBool callback function: A function for PDFlattener which monitors the flattener's progress.
static ASBool flattenerProgMon(ASInt32 pageNum, ASInt32 totalPages, float current, ASInt32 reserved,
                               void *clientData);

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;
    APDFLib libInit;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Flattening transparencies of " << csInputFileName.c_str() << " and saving to "
              << csOutputFileName.c_str() << std::endl;

    PDFlattenerUserParamsRec flattenParams;
    memset(&flattenParams, 0, sizeof(PDFlattenerUserParamsRec));
    flattenParams.size = sizeof(PDFlattenerUserParamsRec);

    DURING

        // Initialize the PDFLattener plugin.

        // Sets the correct location for the PDFlattener function table.
        gPDFlattenerHFT = InitPDFlattenerHFT;

        if (!PDFlattenerInitialize()) {
            std::cout << "The PDFlattener plugin failed to initialize." << std::endl;
            errCode = -1;
        }

        if (0 == errCode) {

            // Step 1) Configure the PDFlattener parameters.

            // Appearance options

            // A profiled color space to use for transparent objects. For CMYK, use "U.S. Web Coated (SWOP)v2".
            flattenParams.profileDesc = ASTextFromUnicode((ASUTF16Val *)"sRGB IEC61966-2.1", kUTF8);
            // The ZIP compression scheme (Flate encoding) for images.
            flattenParams.colorCompression = kPDFlattenerZipCompression;
            // Raster/Vector balance. Use 0.00f for no vectors.
            flattenParams.transQuality = 100.0f;

            // Callback options.

            ASInt32 currentPage = -1;
            // Progress monitor callback data. I'm using this data to store the previous page
            //   the Flattener was working on, using -1 as "hasn't begun yet".
            flattenParams.progressClientData = (void *)&currentPage;
            // The progress monitor callback function.
            flattenParams.flattenProgress = flattenerProgMon;

            // Tile flattening options

            PDFlattenRec flattener;
            memset(&flattener, 0, sizeof(PDFlattenRec));
            flattener.size = sizeof(PDFlattenRec);

            flattener.tilingMode = kPDNoTiling;
            flattener.tileSizePts = 0;

            // Resolution for flattening the interior of an atomic region.
            flattener.internalDPI = 800.0f;
            // Resolution for flattening edges of atomic regions.
            flattener.externalDPI = 200.0f;

            flattener.clipComplexRegions = false;
            // If we convert stroked elements to filled elements.
            flattener.strokeToFill = true;
            // If we use rastered text instead of native text.
            flattener.useTextOutlines = false;
            // If we attempt to preserve overprint
            flattener.preserveOverprint = true;

            flattener.allowShadingOutput = true;
            flattener.allowLevel3ShadingOutput = true;

            // Maximum image size while flattening. 0 is default.
            flattener.maxFltnrImageSize = 0;
            // Adaptive flattening threshold. Doesn't matter, since we're not doing adaptive tiling. See tilingMode.
            flattener.adaptiveThreshold = 0;

            flattenParams.flattenParams = &flattener;

            // Step 2) Open the input pdf
            APDFLDoc doc(csInputFileName.c_str(), true);

            // Step 3) Call the PDFlattener.

            ASUns32 numFlattened = 0;

            PDDoc pddoc = doc.getPDDoc();

            // PDFlattenerConvertEx2 will set this to 0 if flattening failed.
            ASInt32 result = 0;

            result = PDFlattenerConvertEx2(pddoc, // The document whose pages we wish to flatten.
                                           0,     // The first page to flatten.
                                           PDDocGetNumPages(pddoc) - 1, // The last page to flatten.
                                           &numFlattened, // PDFlattener sets this to the number of pages
                                                          //   it flattened. It will not flatten pages that do
                                                          //   not contain transparent elements.
                                           &flattenParams); // Flattener options.

            if (result) {
                std::cout << "Flattened " << numFlattened << " pages." << std::endl;
            } else {
                std::cout << "Flattening failed." << std::endl;
                ASRaise(GenError(genErrGeneral));
            }

            // Step 4) Save the document, close it, release resources and terminate the plugin.

            doc.saveDoc(csOutputFileName.c_str());
            ASTextDestroy(flattenParams.profileDesc);
            PDFlattenerTerminate();

        } // if 0 == errCode
    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
        if (flattenParams.profileDesc != NULL)
            ASTextDestroy(flattenParams.profileDesc);
        PDFlattenerTerminate();
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library.
}

// ASBool callback function: A function for PDFlattener which monitors the flattener's progress.
//
// Note:
// clientData is an ASInt32* representing the previous page we were working on flattening.
// It must start with a page number that doesn't exist, like -1. With it, we have this function
// print the progress only every time a new page is begun, or when the Flattener is finished.
ASBool flattenerProgMon(ASInt32 pageNum, ASInt32 totalPages, float current, ASInt32 reserved, void *clientData) {
    // The previous page we were working on.
    ASInt32 *prevPage = (ASInt32 *)clientData;

    // If we've begun a new page, or if we've finished.
    if (pageNum != (*prevPage) || current == 100.0f) {
        // Print the completion percentage.
        std::cout << "[" << std::fixed << std::setw(6) << std::setfill('0') << std::setprecision(2)
                  << current << "%] ";

        // Print the current page.
        std::cout << "Flattening page " << pageNum + 1 << " of " << totalPages << ". " << std::endl;

        // Update previous page.
        *prevPage = pageNum;
    }

    // Return 1 to cancel Flattening.
    return 0;
}
