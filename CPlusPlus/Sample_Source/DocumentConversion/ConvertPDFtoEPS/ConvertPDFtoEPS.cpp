//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// ConvertPDFtoEPS converts the contents of each page of an input PDF into a series of new
// Encapsulated Postscript files.
//
// Command-line:  <input-pdf> <output-name-root>      (Both are optional)
//
// For more detail see the description of the ConvertPDFtoEPS sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#convertpdftoeps

#include <sstream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "PDFLPrint.h"
#include "SetupPrintParams.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "Ulysses.pdf"
#define DEF_OUTPUT "ConvertPDFtoEPS-out-"

int main(int argc, char **argv) {
    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Will convert each page of " << csInputFileName.c_str() << " to an EPS file named "
              << csOutputFileName.c_str() << "*.eps" << std::endl;

    DURING

        // Step 1) Open the input PDF

        APDFLDoc inAPDoc(csInputFileName.c_str(), true); // Open the input document, repairing it if necessary.
        PDDoc inDoc = inAPDoc.getPDDoc();

        // Step 2) Initialize print parameters

        // Set defaults
        PDPrintParamsRec psParams;
        SetupPDPrintParams(&psParams);

        PDFLPrintUserParamsRec userParams;
        SetupPDFLPrintUserParams(&userParams);

        // Override defaults with specifics for this sample
        userParams.emitToFile = true; // Print to file

        psParams.emitPS = true;                   // Create file
        psParams.outputType = PDOutput_EPSNoPrev; // Create an EPS file with no preview

        // Step 3) Open the output stream for each page, set print param for stream, and write to file

        // Iterate for each page
        for (int pageIndex = 0; pageIndex < PDDocGetNumPages(inDoc); pageIndex++) {
            std::ostringstream ossFileName;
            ossFileName << csOutputFileName.c_str() << pageIndex + 1 << ".eps";

            ASFile outFile = APDFLDoc::OpenFlatFile(ossFileName.str().c_str(), ASFILE_WRITE | ASFILE_CREATE);

            ASStm printStm = ASFileStmWrOpen(outFile, 0);

            userParams.printStm = printStm; // Send output to a writeable stream

            psParams.ranges[0].startPage = pageIndex; // Specify starting page via the current page index
            psParams.ranges[0].endPage = pageIndex; // Specify ending page (also current page index)

            userParams.printParams = &psParams; // Connect the two structures

            // Create EPS document from the current page
            PDFLPrintDoc(inDoc, &userParams);

            // Safely close and release all files and objects
            ASStmClose(printStm);
            ASFileFlush(outFile);
            ASFileClose(outFile);
        }

        // Cleanup and free memory
        DisposePDPrintParams(&psParams);
        DisposePDFLPrintUserParams(&userParams);

        PDDocClose(inDoc);

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the APDFL
};
