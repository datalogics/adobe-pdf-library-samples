//
// Copyright (c) 2019, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The FlattenForms sample demonstrates how to Flatten two types of forms fields:
//
//  - Flatten XFA (Dynamic or Static) to regular page content which converts and expands XFA fields to regular PDF content and removes the XFA fields
//  - Flatten AcroForms to regular page content which converts AcroForm fields to regular page content and removes the AcroForm fields
//
// NOTE: The APDFL Forms Extension is available separately from APDFL.  Please contact Datalogics directly for more information about this extension.
//
// For more detail see the description of the FlattenForms sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#flattenforms

#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

//Header that includes Forms Extension methods
#include "DLExtrasCalls.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"

#define DEF_INPUT_XFA "DynamicXFA.pdf"
#define DEF_INPUT_ACROFORMS "AcroForm.pdf"

#define DEF_OUTPUT_XFA_FLATTENED "FlattenXFA-out.pdf"
#define DEF_OUTPUT_ACROFORMS_FLATTENED "FlattenAcroForms-out.pdf"

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

    std::string csInputFileNameXFA(INPUT_DIR DEF_INPUT_XFA);
    std::string csOutputFileNameXFAFlattened(DEF_OUTPUT_XFA_FLATTENED);

    std::string csInputFileNameAcroForm(INPUT_DIR DEF_INPUT_ACROFORMS);
    std::string csOutputFileNameAcroFormsFlattened(DEF_OUTPUT_ACROFORMS_FLATTENED);

    DURING
        //Must be set to true to prevent default legacy behavior of PDFL
        PDPrefSetAllowOpeningXFA(true);

        //XFA document
        APDFLDoc documentXFA(csInputFileNameXFA.c_str(), true);

        ASUns32 pagesOutput = 0;
        PDDocFlattenXFAFields(documentXFA.getPDDoc(), &pagesOutput);

        std::cout << "XFA document was expanded into " << pagesOutput << " Flattened pages." << std::endl;

        documentXFA.saveDoc(csOutputFileNameXFAFlattened.c_str(), PDSaveFull | PDSaveLinearized);


        //AcroForms document
        APDFLDoc documentAcroForms(csInputFileNameAcroForm.c_str(), true);

        PDDocFlattenAcroFormFields(documentAcroForms.getPDDoc());

        std::cout << "AcroForms document was Flattened" << std::endl;

        documentAcroForms.saveDoc(csOutputFileNameAcroFormsFlattened.c_str(), PDSaveFull | PDSaveLinearized);
    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;                               //APDFLib's destructor terminates the library.
}
