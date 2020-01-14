//
// Copyright (c) 2019, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The ExportFormsData sample demonstrates how to Export forms data from XFA and AcroForms documents:
//
//  - Export data from a XFA (Dynamic or Static) document, the types supported include XDP, XML, or XFD
//  - Export data from an AcroForms document, the types supported include XFDF, FDF, or XML
//
// NOTE: The APDFL Forms Extension is available separately from APDFL.  Please contact Datalogics directly for more information about this extension.
//
// For more detail see the description of the ExportFormsData sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#exportformsdata

#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

//Header that includes Forms Extension methods
#include "DLExtrasCalls.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"

#define DEF_INPUT_XFA "DynamicXFA.pdf"
#define DEF_INPUT_ACROFORMS "AcroForm.pdf"

#define DEF_OUTPUT_XDP_EXPORTED "ExportFormsDataXFA.xdp"
#define DEF_OUTPUT_XFDF_EXPORTED "ExportFormsDataAcroForms.xfdf"

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
    std::string csOutputFileNameXDPExported(DEF_OUTPUT_XDP_EXPORTED);

    std::string csInputFileNameAcroForm(INPUT_DIR DEF_INPUT_ACROFORMS);
    std::string csOutputFileNameAcroFormsExported(DEF_OUTPUT_XFDF_EXPORTED);

    DURING
        //Must be set to true to prevent default legacy behavior of PDFL
        PDPrefSetAllowOpeningXFA(true);

        //XFA document
        APDFLDoc documentXFA(csInputFileNameXFA.c_str(), true);

        ASPathName xdpDataPath = APDFLDoc::makePath(csOutputFileNameXDPExported.c_str());

        //Export the data while specifying the type, in this case XDP
        bool result = PDDocExportXFAFormsData(documentXFA.getPDDoc(), ASGetDefaultFileSys(), xdpDataPath, XFAFormExportTypeXDP);

        if (result)
        {
            std::cout << "Forms data was exported!" << std::endl;
        }
        else
        {
            std::cout << "Exporting of Forms data failed!" << std::endl;
        }

        ASFileSysReleasePath(NULL, xdpDataPath);


        //AcroForms document
        APDFLDoc documentAcroForms(csInputFileNameAcroForm.c_str(), true);

        ASPathName xfdfDataPath = APDFLDoc::makePath(csOutputFileNameAcroFormsExported.c_str());

        //Export the data while specifying the type, in this case XFDF
        result = PDDocExportAcroFormsData(documentAcroForms.getPDDoc(), ASGetDefaultFileSys(), xfdfDataPath, AcroFormExportTypeXFDF);

        if (result)
        {
            std::cout << "Forms data was exported!" << std::endl;
        }
        else
        {
            std::cout << "Exporting of Forms data failed!" << std::endl;
        }

        ASFileSysReleasePath(NULL, xfdfDataPath);
    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;                               //APDFLib's destructor terminates the library.
}
