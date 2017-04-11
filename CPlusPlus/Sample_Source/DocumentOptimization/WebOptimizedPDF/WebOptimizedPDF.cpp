//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample converts a PDF document into a web optimized, or linearized, file. 
//
// Command-line:  <input-file> <output-file>     (Both optional)
//
// For more detail see the description of the WebOptimizedPDF sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#weboptimizedpdf

#include <iostream>
#include "ASExtraCalls.h"
#include "PDCalls.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "NonLinearized.pdf"
#define DEF_OUTPUT "WebOptimizedPDF-out.pdf"

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
    std::cout << "Will open " << csInputFileName.c_str() << " and resave it, \"Linearized\", "
              << "as " << csOutputFileName.c_str() << std::endl;

DURING

//Step 1) Open the input document.

    APDFLDoc docIn ( csInputFileName.c_str(), true );

// 2) Save it as a web-optimized document and close it.
        
    docIn.saveDoc ( csOutputFileName.c_str(), PDSaveFull | PDSaveLinearized );

HANDLER
    errCode = ERRORCODE;
    libInit.displayError(errCode);
END_HANDLER

    return (errCode);
}
