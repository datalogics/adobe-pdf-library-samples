//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The SplitPDF opens a PDF input document and exports the pages to a set of separate PDF documents.
// This sample is effectively the opposite of MergeDocuments.
//
//  Command-line:    <input-file>  <output-file-prefix>      (Both are optional)
//
// For more detail see the description of the SplitPDF sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#splitpdf

#include <string>
#include <iostream>
#include <sstream>
#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "PDFToBeSplit.pdf"
#define DEF_OUTPUT_PREFIX "_b_"

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFilePrefix(argc > 2 ? argv[2] : DEF_OUTPUT_PREFIX);
    std::cout << "Will split " << csInputFileName.c_str() << " into 1 PDF file per page, "
              << " with names beginning with " << csOutputFilePrefix.c_str() << std::endl;

    DURING

        // Step 1) Open input file

        APDFLDoc document(csInputFileName.c_str(), true);

        // Step 2) Copy each page into a newly created empty document, and save it

        ASUns32 numInputPages = PDDocGetNumPages(document.getPDDoc());
        for (ASUns32 page = 0; page < numInputPages; ++page) {
            // Create new empty document
            APDFLDoc outDoc;

            // Insert the current page into the new document
            PDDocInsertPages(outDoc.getPDDoc(), PDBeforeFirstPage, document.getPDDoc(), page, 1,
                             PDInsertDoNotResolveInvalidStructureParentReferences, NULL, NULL, NULL, NULL);

            // Cook up the file name, and save
            std::ostringstream ossFile;
            ossFile << csOutputFilePrefix.c_str() << (page + 1) << ".pdf";
            outDoc.saveDoc(ossFile.str().c_str());
        }

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
