//
// Copyright (c) 2017-2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// ConvertToPDFX converts the input PDF to a PDF/X compliant PDF.
//
// Command-line:  <input-pdf> <convert-option>       (all parameters are optional moving from left to right)
//        where convert-option is 'PDFX4', 'PDFX1a2001', 'PDFX32003'
//        if no parameters are specified, a pre-selected PDF is input and converted using PDF/X-4
//
//        For example:
//
//        ConvertToPDFX input-file.pdf PDFX4
//
//        This statement provides the name of an input file and specifies the PDF/X-4 format (default).
//
// For more detail see the description of the ConvertToPDFX sample program on our Developer's site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#converttopdfx

#include <string>
#include <iostream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "PDFProcessorCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "Ulysses.pdf"
#define DEF_OUTPUT "ConvertToPDFX-out.pdf"

// forward declarations
void SetupPDFXProcessorParams(PDFProcessorPDFXConvertParams userParams);

int main(int argc, char **argv) {
    PDFProcessorPDFXConversionOption convertOption;

    // Step 1) Select Conversion option
    if (argc > 2 && (!strcmp(argv[2], "PDFX4") || !strcmp(argv[2], "PDF/X-4"))) {
        convertOption = kPDFProcessorConvertToPDFX1a2001;
    }
    else if (argc > 2 && (!strcmp(argv[2], "PDFX1a2001") || !strcmp(argv[2], "PDF/X-1A:2001"))) {
        convertOption = kPDFProcessorConvertToPDFX1a2001;
    } else if (argc > 2 && (!strcmp(argv[2], "PDF/X-3:2003"))) {
        convertOption = kPDFProcessorConvertToPDFX32003;
    } else {
        std::cout << "PDF Conversion Standard not specified or unknown, defaulting to PDF/X-4." << std::endl;
        convertOption = kPDFProcessorConvertToPDFX42008;
    }

    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(DEF_OUTPUT);
    std::cout << "Will convert " << csInputFileName << " to a PDF/X compliant PDF named "
              << csOutputFileName << std::endl;

    DURING
        gPDFProcessorHFT = InitPDFProcessorHFT;

        // Step 2) Initialize PDFProcessor plugin
        if (PDFProcessorInitialize()) {
            ASPathName destFilePath = NULL;

            // Step 3) Open the input PDF, repairing it if necessary
            APDFLDoc inAPDoc(csInputFileName.c_str(), true);

#if !MAC_ENV
            destFilePath =
                ASFileSysCreatePathName(NULL, ASAtomFromString("Cstring"), csOutputFileName.c_str(), NULL);
#else
            destFilePath = APDFLDoc::makePath(csOutputFileName.c_str());
#endif

            // Step 4) Convert the document
            PDFProcessorPDFXConvertParamsRec userParamsX;
            std::cout << "Converting using PDFProcessorConvertAndSaveToPDFX (with Callback)" << std::endl;
            SetupPDFXProcessorParams(&userParamsX);

            ASInt32 res = PDFProcessorConvertAndSaveToPDFX(inAPDoc.getPDDoc(), destFilePath, ASGetDefaultFileSys(), convertOption, &userParamsX);

            if (res) {
                std::cout << "File " << csInputFileName << " has been successfully Converted." << std::endl;
            } else {
                std::cout << "Conversion of file " << csInputFileName << " has failed..." << std::endl;
            }

            // Release resources
            if (destFilePath) {
                ASFileSysReleasePath(ASGetDefaultFileSys(), destFilePath);
            }

            PDFProcessorTerminate();
        }
    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);

        PDFProcessorTerminate();
    END_HANDLER

    return errCode; // APDFLib's destructor terminates APDFL
}

ASBool PDFProcessorProgressMonitorCB(ASInt32 pageNum, ASInt32 totalPages, float current, void *clientData) {
    if (clientData) {
        ASBool *IsMonitorCalled = (ASBool *)clientData;
        if (*IsMonitorCalled == false) {
            std::cout << "PDFProcessor Progress Monitor CallBack" << std::endl;
            // Set to true to Display this Message Only Once
            *IsMonitorCalled = true;
        }
    }

    // Page numbers are 0-indexeed.
    std::cout << "PDFProcessor Page " << pageNum + 1 << " of " << totalPages << ". Overall Progress = " << current << "%." << std::endl;

    // Return true to Cancel conversion
    return false;
}

void SetupPDFXProcessorParams(PDFProcessorPDFXConvertParams userParams) {
    memset(userParams, 0, sizeof(PDFProcessorPDFXConvertParamsRec));

    userParams->size = sizeof(PDFProcessorPDFXConvertParamsRec);
    userParams->progMon = PDFProcessorProgressMonitorCB;
}
