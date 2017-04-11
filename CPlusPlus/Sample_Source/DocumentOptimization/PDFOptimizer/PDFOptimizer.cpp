//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample program demonstrates the use of the PDFOptimizer feature offered with the
// Adobe PDF Library. This effectively compresses a PDF document to make it smaller and
// thus faster and easier to download and open.
//
// Command-line:  <input-file>   <output-file>    (Both optional)
//
// For more detail see the description of the PDFOptimizer sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#pdfoptimizer

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "DLExtrasCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "Ulysses.pdf"
#define DEF_OUTPUT "PDFOptimizer-out.pdf"

int main(int argc, char **argv)
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
    std::cout << "Will optimize " << csInputFileName.c_str() << " and save as "
              << csOutputFileName.c_str() << std::endl;

    DURING

// Step 1) Open the input PDF

    APDFLDoc inAPDoc ( csInputFileName.c_str(), true);
    PDDoc inDoc = inAPDoc.getPDDoc();

    // Set defaults
    PDFOptimizationParams optParams = PDDocOptimizeDefaultParams();

//Step 2: Optimize and save the document and release resources. 

    ASPathName outPathName = APDFLDoc::makePath ( csOutputFileName.c_str() );

    PDDocumentOptimize(inDoc, outPathName, NULL, optParams, NULL, NULL, NULL, NULL);

// Step 3) Release all objects that are still in use.

    ASFileSysReleasePath(NULL, outPathName);    

    PDDocClose(inDoc);

HANDLER
    errCode = ERRORCODE;
    libInit.displayError(errCode);
END_HANDLER

    return errCode;
};
