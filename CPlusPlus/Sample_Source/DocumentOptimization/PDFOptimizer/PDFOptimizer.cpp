//
// Copyright (c) 2017-2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample program demonstrates the use of PDFOptimizer. This compresses a PDF document
// to make it smaller so it's easier to process and download.
//
// NOTE: Some documents can't be compressed because they're already well-compressed or contain
// content that can't be assumed is safe to be removed.  However you can fine tune the optimization
// to suit your applications needs and drop such content to achieve better compression if you already
// know it's unnecessary.
//
// Command-line:  <input-file>   <output-file>    (Both optional)
//
// For more detail see the description of the PDFOptimizer sample program on our Developer's site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#pdfoptimizer

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "DLExtrasCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "toOptimize.pdf"
#define DEF_OUTPUT "PDFOptimizer-out.pdf"

int main(int argc, char **argv) {

    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Will optimize " << csInputFileName.c_str() << " and save as "
              << csOutputFileName.c_str() << std::endl;

    DURING
        // Step 1) Open the input PDF
        APDFLDoc inAPDoc(csInputFileName.c_str(), true);
        PDDoc inDoc = inAPDoc.getPDDoc();

        // Set optimization defaults.
        PDFOptimizationParams optParams = PDDocOptimizeDefaultParams();

        ASPathName outPathName = APDFLDoc::makePath(csOutputFileName.c_str());

        // Step 2: Optimize the document.
        PDDocumentOptimize(inDoc, outPathName, NULL, optParams, NULL, NULL, NULL, NULL);

        // Step 3) Release resources.
        ASFileSysReleasePath(NULL, outPathName);

        PDDocOptimizeReleaseParams(optParams);

        PDDocClose(inDoc);
    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
};
