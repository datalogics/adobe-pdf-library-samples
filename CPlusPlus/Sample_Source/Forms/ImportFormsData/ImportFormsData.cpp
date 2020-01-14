//
// Copyright (c) 2019, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The ImportFormsData sample demonstrates how to Import forms data into XFA and AcroForms documents:
//
//  - Import data into a XFA (Dynamic or Static) document, the types supported include XDP, XML, or XFD
//  - Import data into an AcroForms document, the types supported include XFDF, FDF, or XML
//
// NOTE: The APDFL Forms Extension is available separately from APDFL.  Please contact Datalogics directly for more information about this extension.
//
// For more detail see the description of the ImportFormsData sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#importformsdata

#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

//Header that includes Forms Extension methods
#include "DLExtrasCalls.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"

#define DEF_INPUT_XFA "DynamicXFA.pdf"
#define DEF_INPUT_ACROFORMS "AcroForm.pdf"

#define DEF_INPUT_XFA_DATA "DynamicXFA_data.xdp"
#define DEF_INPUT_ACROFORMS_DATA "AcroForm_data.xfdf"

#define DEF_OUTPUT_XFA_IMPORTED "ImportFormsDataXFA-out.pdf"
#define DEF_OUTPUT_ACROFORMS_IMPORTED "ImportFormsDataAcroForms-out.pdf"

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
    std::string csInputFileNameXFAData(INPUT_DIR DEF_INPUT_XFA_DATA);
    std::string csOutputFileNameXFAImported(DEF_OUTPUT_XFA_IMPORTED);

    std::string csInputFileNameAcroForm(INPUT_DIR DEF_INPUT_ACROFORMS);
    std::string csInputFileNameAcroFormData(INPUT_DIR DEF_INPUT_ACROFORMS_DATA);
    std::string csOutputFileNameAcroFormsImported(DEF_OUTPUT_ACROFORMS_IMPORTED);

    DURING
        //Must be set to true to prevent default legacy behavior of PDFL
        PDPrefSetAllowOpeningXFA(true);

        //XFA document
        APDFLDoc documentXFA(csInputFileNameXFA.c_str(), true);

        ASPathName xfaDataPath = APDFLDoc::makePath(csInputFileNameXFAData.c_str());

        //Import the data, acceptable types include XDP, XML, and XFD
        bool result = PDDocImportXFAFormsData(documentXFA.getPDDoc(), ASGetDefaultFileSys(), xfaDataPath);

        if (result)
        {
            std::cout << "Forms data was imported!" << std::endl;

            documentXFA.saveDoc(csOutputFileNameXFAImported.c_str(), PDSaveFull | PDSaveLinearized);
        }
        else
        {
            std::cout << "Importing of Forms data failed!" << std::endl;
        }

        ASFileSysReleasePath(NULL, xfaDataPath);


        //AcroForms document
        APDFLDoc documentAcroForms(csInputFileNameAcroForm.c_str(), true);

        ASPathName acroformsDataPath = APDFLDoc::makePath(csInputFileNameAcroFormData.c_str());

        //Import the data, specify the type in this case XFDF
        result = PDDocImportAcroFormsData(documentAcroForms.getPDDoc(), ASGetDefaultFileSys(), acroformsDataPath, AcroFormImportTypeXFDF);

        if (result)
        {
            std::cout << "Forms data was imported!" << std::endl;

            documentAcroForms.saveDoc(csOutputFileNameAcroFormsImported.c_str(), PDSaveFull | PDSaveLinearized);
        }
        else
        {
            std::cout << "Importing of Forms data failed!" << std::endl;
        }

        ASFileSysReleasePath(NULL, acroformsDataPath);
    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;                               //APDFLib's destructor terminates the library.
}
