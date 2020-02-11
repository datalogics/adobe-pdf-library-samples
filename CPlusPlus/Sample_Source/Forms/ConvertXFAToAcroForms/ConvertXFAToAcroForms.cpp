//
// Copyright (c) 2019, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// Command line argument:  (Optional) <input-file-name> <output-file-name>
//
// The ConvertXFAToAcroForms sample demonstrates how to convert XFA into AcroForms:
//
//  - Converts XFA (Dynamic or Static) fields to AcroForms fields and removes XFA fields
//
// NOTE: The APDFL Forms Extension is available separately from APDFL.  Please contact Datalogics directly for more information about this extension.
//
// For more detail see the description of the ConvertXFAToAcroForms sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#convertxfatoacroforms

#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

//Header that includes Forms Extension methods
#include "DLExtrasCalls.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"

#define DEF_INPUT_XFA "DynamicXFA.pdf"

#define DEF_OUTPUT_ACROFORMS "AcroForms-out.pdf"

int main(int argc, char** argv)
{
    //Initialize the Forms Extension for APDFL dependencies
    APDFLib libInit(NULL, kPDFLInitFormsExtension);
    ASErrorCode errCode = 0;

    if (libInit.isValid() == false)
    {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    //Verify the Forms Extension dependencies loaded correctly
    if (!IsFormsExtensionSupported())
    {
        std::cout << "Forms Extension dependencies were not properly loaded!" << std::endl;
        return -1;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_DIR DEF_INPUT_XFA);
    std::string csOutputFileNameAcroForms(argc > 2 ? argv[2] : DEF_OUTPUT_ACROFORMS);

    DURING
        //Must be set to true to prevent default legacy behavior of PDFL
        PDPrefSetAllowOpeningXFA(true);

        //XFA document
        APDFLDoc documentXFA(csInputFileName.c_str(), true);

        ASUns32 pagesOutput = 0;
        PDDocConvertXFAFieldsToAcroFormFields(documentXFA.getPDDoc(), &pagesOutput);

        std::cout << "XFA document was converted into an AcroForms document with " << pagesOutput << " pages." << std::endl;

        documentXFA.saveDoc(csOutputFileNameAcroForms.c_str(), PDSaveFull | PDSaveLinearized);
    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;                               //APDFLib's destructor terminates the library.
}
