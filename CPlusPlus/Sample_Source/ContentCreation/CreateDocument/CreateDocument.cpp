//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample creates a new PDF document and inserts pages.
//
// Command-line:  <output-file>   <number-of-pages>    (Both optional)
//
// For more detail see the description of the CreateDocument sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#createdocument

#include <string>
#include <cstdlib>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "PDCalls.h"

#define DEF_OUTPUT "CreateDocument-out.pdf"
#define PAGES_TO_INSERT 5

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csOutputFileName(argc > 1 ? argv[1] : DEF_OUTPUT);
    int nPages(argc > 2 ? atoi(argv[2]) : PAGES_TO_INSERT);
    std::cout << "Creating new document " << csOutputFileName.c_str() << " and inserting " << nPages
              << " pages into it." << std::endl;

    DURING

        // Step 1: Create the document and add pages.

        PDDoc pdDoc = PDDocCreate(); // Create a new PDF document.

        // Set the dimensions for the page. In this case 8.5 x 11 inches.
        //    Note: 72 pixels == 1 inch
        ASFixedRect mediaBox;
        mediaBox.left = fixedZero;
        mediaBox.right = FloatToASFixed(72.0 * 8.5);
        mediaBox.bottom = fixedZero;
        mediaBox.top = FloatToASFixed(72.0 * 11.0);

        // Create and insert pages into the PDF document.
        for (int i = 0; i < nPages; ++i) {
            // Since we are not doing anything with the pages created, we
            //     can release them at the very same time
            PDPageRelease(PDDocCreatePage(pdDoc, PDBeforeFirstPage, mediaBox));
        }

        // Step 2: Save the document and release resources.

        // Create an ASPathName object for the output file name
        ASPathName asPathName = APDFLDoc::makePath(csOutputFileName.c_str());
        PDDocSave(pdDoc, PDSaveFull, asPathName, ASGetDefaultFileSys(), NULL, NULL);

        ASFileSysReleasePath(NULL, asPathName);
        PDDocClose(pdDoc);

    HANDLER
        errCode = libInit.getInitError();
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
