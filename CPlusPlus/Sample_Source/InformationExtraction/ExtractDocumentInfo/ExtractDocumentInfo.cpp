//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample extracts standard information from a PDF document and saves it to an output text file.
//
// Command-line:  <input-file> <output-file>     (Both optional)
//
// For more detail see the description of the ExtractDocumentInfo sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#extractdocumentinfo
//

#include <string>
#include <iostream>
#include <fstream>
#include "ASCalls.h"
#include "InitializeLibrary.h"
#include "ASExtraCalls.h"
#include "PagePDECntCalls.h"
#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "APDFLDoc.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "extractInfoFrom.pdf"
#define DEF_OUTPUT "ExtractDocumentInfo-out.txt"

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
    std::cout << "Extracting document info from " << csInputFileName.c_str() << " and writing to "
              << csOutputFileName.c_str() << std::endl;

    DURING

        APDFLDoc inDoc(csInputFileName.c_str(), true);

        // Step 1) Extract document information from input document

        std::ofstream ofs(csOutputFileName.c_str());
        ofs << "<<<Extracted Document Information>>> from file " << csInputFileName.c_str() << std::endl;

        std::string keyNames[7] = {"Title",   "Author",   "Subject", "Keywords",
                                   "Creator", "Producer", "Trapped"};

        for (int i = 0; i < 7; ++i) {
            ASText keyText;
            keyText = ASTextFromPDText(keyNames[i].c_str());

            ASText infoText = ASTextNew();
            PDDocGetInfoASText(inDoc.getPDDoc(), keyText, infoText); // Obtain the description using the key
            char *infoString = ASTextGetPDTextCopy(infoText, 0);
            ofs << keyNames[i].c_str() << ": " << infoString << std::endl;
            // Delete text objects, no longer in use
            ASTextDestroy(infoText);
            ASTextDestroy(keyText);
            ASfree(infoString);
        }

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
