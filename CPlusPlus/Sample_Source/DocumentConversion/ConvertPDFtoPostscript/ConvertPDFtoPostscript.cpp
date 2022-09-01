//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample converts the contents of an input PDF document into a PostScript file.
//
// Command-line:    <input-pdf>  <output-name>       (Both optional)
//
// For more detail see the description of the ConvertPDFtoPostscript sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#convertpdftopostscript

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "PDFLPrint.h"
#include "SetupPrintParams.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "Ulysses.pdf"
#define DEF_OUTPUT "ConvertPDFtoPostscript-out.ps"

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
    std::cout << "Will convert " << csInputFileName.c_str() << " to Postscript, writing output to "
              << csOutputFileName.c_str() << std::endl;

    DURING

        // Step 1) Open the input PDF

        // Open the input document, repairing it if necessary.
        APDFLDoc inAPDoc(csInputFileName.c_str(), true);
        PDDoc inDoc = inAPDoc.getPDDoc();

        // Step 2) Initialize print parameters

        // Set defaults (see SetupPrintParams files in Common folder)
        PDPrintParamsRec psParams;
        SetupPDPrintParams(&psParams);

        PDFLPrintUserParamsRec userParams;
        SetupPDFLPrintUserParams(&userParams);

        // Override defaults with specifics for this sample
        userParams.emitToFile = true; // Print to file
        psParams.emitPS = true;       // Create PS file

        // Step 3) Open the output stream and set print param for stream

        // Create the file for output
        ASFile outFile = APDFLDoc::OpenFlatFile(csOutputFileName.c_str(), ASFILE_WRITE | ASFILE_CREATE);

        // Create  writeable ProcStm to handle the print stream
        ASStm printStm = ASFileStmWrOpen(outFile, 0);

        userParams.printStm = printStm;     // Send output to the writeable stream
        userParams.printParams = &psParams; // Connect the two structures

        // Step 4) Write to PS file, close it, and clean up

        DURING
            PDFLPrintDoc(inDoc, &userParams);
        HANDLER
            errCode = ERRORCODE;
            lib.displayError(errCode);
        END_HANDLER

        // Cleanup and free memory
        DisposePDPrintParams(&psParams);
        DisposePDFLPrintUserParams(&userParams);

        ASStmClose(printStm); // Close the file stream
        PDDocClose(inDoc);    // Close the input document

        ASFileFlush(outFile); // Safely close outFile
        ASFileClose(outFile);

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the APDFL
};
