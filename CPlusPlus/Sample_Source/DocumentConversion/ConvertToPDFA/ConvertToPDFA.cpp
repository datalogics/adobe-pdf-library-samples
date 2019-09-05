//
// Copyright (c) 2017-2019, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// ConvertToPDFA converts the input PDF to a PDF/A compliant PDF.
//
// Command-line:  <input-pdf> <convert-option> <color-space>      (all parameters are optional moving from left to right)
//        where convert-option is '1a', '1b', '2b', '2u', '3b', or '3u' and where color-space is 'rgb' or 'cmyk'
//        if no parameters are specified, a pre-selected PDF is input and converted using 1b RGB.
//
//        For example, you might enter a command line statement that looks like this:
//
//        ConvertToPDFA input-file.pdf 2b
//
//        This statement provides the name of an input file and specifies the PDF/A-2b format, rather than
//        the default PDF/A-1b format.
//
//        NOTE: Level A conformance requires logical structure information (tagging) exist for the document, if this is not
//        present.  Conversion will be re-attempted using Level B conformance.
// For more detail see the description of the ConvertToPDFA sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#converttopdfa

#include <sstream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "PDFProcessorCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "Ulysses.pdf"
#define DEF_OUTPUT "ConvertToPDFA-out.pdf"

//forward declarations
ASBool PDFProcessorProgressMonitorCB(ASInt32 pageNum, ASInt32 totalPages, float current, void *clientData);
void SetupPDFAProcessorParams(PDFProcessorPDFAConvertParams userParams);

int main(int argc, char **argv)
{
    PDFProcessorPDFAConversionOption convertOption;
    char *outputPathName;

    /* Step 1) Select conversion option */
    if (argc < 2)
    {
        std::cout << "PDF Conversion Standard not specified or unknown, defaulting to PDFA1bRGB." << std::endl;

        convertOption = kPDFProcessorConvertToPDFA1bRGB;
        outputPathName = DEF_OUTPUT;
    }
    else if (argc > 2 && (!strcmp(argv[2], "1b") || !strcmp(argv[2], "1B")))
    {
        if (argc > 3)
        {
            if (!strcmp(argv[3], "rgb") || !strcmp(argv[3], "RGB"))
            {
                convertOption = kPDFProcessorConvertToPDFA1bRGB;
            }
            else if (!strcmp(argv[3], "cmyk") || !strcmp(argv[3], "CMYK"))
            {
                convertOption = kPDFProcessorConvertToPDFA1bCMYK;
            }
        }
        else
        {
            convertOption = kPDFProcessorConvertToPDFA1bRGB;
        }
        outputPathName = DEF_OUTPUT;
    }
    else if (argc > 2 && (!strcmp(argv[2], "1a") || !strcmp(argv[2], "1A")))
    {
        if (argc > 3)
        {
            if (!strcmp(argv[3], "rgb") || !strcmp(argv[3], "RGB"))
            {
                convertOption = kPDFProcessorConvertToPDFA1aRGB;
            }
            else if (!strcmp(argv[3], "cmyk") || !strcmp(argv[3], "CMYK"))
            {
                convertOption = kPDFProcessorConvertToPDFA1aCMYK;
            }
        }
        else
        {
            convertOption = kPDFProcessorConvertToPDFA1aRGB;
        }
        outputPathName = DEF_OUTPUT;
    }
    else if (argc > 2 && (!strcmp(argv[2], "2b") || !strcmp(argv[2], "2B")))
    {
        if (argc > 3)
        {
            if (!strcmp(argv[3], "rgb") || !strcmp(argv[3], "RGB"))
            {
                convertOption = kPDFProcessorConvertToPDFA2bRGB;
            }
            else if (!strcmp(argv[3], "cmyk") || !strcmp(argv[3], "CMYK"))
            {
                convertOption = kPDFProcessorConvertToPDFA2bCMYK;
            }
        }
        else
        {
            convertOption = kPDFProcessorConvertToPDFA2bRGB;
        }
        outputPathName = DEF_OUTPUT;
    }
    else if (argc > 2 && (!strcmp(argv[2], "2u") || !strcmp(argv[2], "2U")))
    {
        if (argc > 3)
        {
            if (!strcmp(argv[3], "rgb") || !strcmp(argv[3], "RGB"))
            {
                convertOption = kPDFProcessorConvertToPDFA2uRGB;
            }
            else if (!strcmp(argv[3], "cmyk") || !strcmp(argv[3], "CMYK"))
            {
                convertOption = kPDFProcessorConvertToPDFA2uCMYK;
            }
        }
        else
        {
            convertOption = kPDFProcessorConvertToPDFA2uRGB;
        }
        outputPathName = DEF_OUTPUT;
    }
    else if (argc > 2 && (!strcmp(argv[2], "3b") || !strcmp(argv[2], "3B")))
    {
        if (argc > 3)
        {
            if (!strcmp(argv[3], "rgb") || !strcmp(argv[3], "RGB"))
            {
                convertOption = kPDFProcessorConvertToPDFA3bRGB;
            }
            else if (!strcmp(argv[3], "cmyk") || !strcmp(argv[3], "CMYK"))
            {
                convertOption = kPDFProcessorConvertToPDFA3bCMYK;
            }
        }
        else
        {
            convertOption = kPDFProcessorConvertToPDFA3bRGB;
        }
        outputPathName = DEF_OUTPUT;
    }
    else if (argc > 2 && (!strcmp(argv[2], "3u") || !strcmp(argv[2], "3U")))
    {
        if (argc > 3)
        {
            if (!strcmp(argv[3], "rgb") || !strcmp(argv[3], "RGB"))
            {
                convertOption = kPDFProcessorConvertToPDFA3uRGB;
            }
            else if (!strcmp(argv[3], "cmyk") || !strcmp(argv[3], "CMYK"))
            {
                convertOption = kPDFProcessorConvertToPDFA3uCMYK;
            }
        }
        else
        {
            convertOption = kPDFProcessorConvertToPDFA3uRGB;
        }
        outputPathName = DEF_OUTPUT;
    }
    else
    {
        std::cout << "PDF Conversion Standard not specified or unknown, defaulting to PDFA1bRGB." << std::endl;
        convertOption = kPDFProcessorConvertToPDFA1bRGB;
        outputPathName = DEF_OUTPUT;
    }

    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false)
    {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }
    
    std::string csInputFileName ( argc > 1 ? argv[1] : DIR_LOC DEF_INPUT );
    std::string csOutputFileName ( DEF_OUTPUT );
    std::cout << "Will convert " << csInputFileName.c_str() << " to a PDF/A compliant PDF named "
              << csOutputFileName.c_str() << std::endl;

DURING
    gPDFProcessorHFT = InitPDFProcessorHFT;

    //Step 2) initialize PDFProcessor plugin
    if(PDFProcessorInitialize())
    {
        ASInt32 res = false;

        ASPathName destFilePath = NULL;

        // Step 3) Open the input PDF
        APDFLDoc inAPDoc ( csInputFileName.c_str(), true);  // Open the input document, repairing it if necessary.

#if !MAC_ENV
        destFilePath = ASFileSysCreatePathName( NULL, ASAtomFromString("Cstring"), csOutputFileName.c_str(), NULL );
#else
        destFilePath = APDFLDoc::makePath ( csOutputFileName.c_str() );
#endif

        // Step 4) Convert the input PDF
        PDFProcessorPDFAConvertParamsRec userParamsA;
        std::cout << "Converting using PDFProcessorConvertAndSaveToPDFA (with Callback)" << std::endl;
        SetupPDFAProcessorParams(&userParamsA);

        bool levelAConversionNotPossible = false;

        DURING
            res = PDFProcessorConvertAndSaveToPDFA(inAPDoc.getPDDoc(), destFilePath, ASGetDefaultFileSys(), convertOption, &userParamsA);
        HANDLER
            errCode = ERRORCODE;

            char errStr[250];
            ASGetErrorString(errCode, errStr, sizeof(errStr));

            bool levelAConformanceNotPossible = false;

            if (strcmp(errStr, "Structure Tree Root Is Missing") == 0)
            {
                std::cout << std::endl << "PDF/A Level A requires logical structure information (tagging), the input PDF was not " << std::endl
                          << "created with this information and this information can not be created automatically." << std::endl
                          << "Conversion will be re-attempted using Level B conformance instead." << std::endl << std::endl;

                if (!strcmp(argv[3], "rgb") || !strcmp(argv[3], "RGB"))
                {
                    convertOption = kPDFProcessorConvertToPDFA1bRGB;
                }
                else if (!strcmp(argv[3], "cmyk") || !strcmp(argv[3], "CMYK"))
                {
                    convertOption = kPDFProcessorConvertToPDFA1bCMYK;
                }

                errCode = 0;
                levelAConversionNotPossible = true;
            }
            else
            {
                RERAISE();
            }
        END_HANDLER

        if (levelAConversionNotPossible)
        {
            res = PDFProcessorConvertAndSaveToPDFA(inAPDoc.getPDDoc(), destFilePath, ASGetDefaultFileSys(), convertOption, &userParamsA);
        }

        if(res)
        {
            std::cout << "File " << csInputFileName.c_str() << " has been successfully Converted." << std::endl;
        }
        else
        {
            std::cout << "Conversion of file " << csInputFileName.c_str() << " has failed..." << std::endl;
        }

        // Cleanup and free memory
        if (destFilePath)
        {
            ASFileSysReleasePath(ASGetDefaultFileSys(), destFilePath);
        }

        //Terminate PDFProcessor plugin
        PDFProcessorTerminate();
    }
HANDLER
    errCode = ERRORCODE;
    lib.displayError(errCode);

    //Terminate PDFProcessor plugin
    PDFProcessorTerminate();
END_HANDLER

    return errCode;// APDFLib's destructor terminates APDFL
}

ASBool PDFProcessorProgressMonitorCB(ASInt32 pageNum, ASInt32 totalPages, float current, void *clientData)
{
    if (clientData)
    {
        ASBool * IsMonitorCalled = (ASBool*)clientData;
        if (*IsMonitorCalled == false)
        {
            std::cout << "PDFProcessor Progress Monitor CallBack" << std::endl;
            //Set to true to Display this Message Only Once
            *IsMonitorCalled = true;
        }
    }

    std::cout << "PDFProcessor Page "
        << pageNum + 1 << " of "                /* Adding 1, since Page numbers are 0-indexed*/
        << totalPages << ". Overall Progress = "
        << current << "%."                      /* Current Overall Progress */
        << std::endl;

    //Return 1 to Cancel conversion
    return 0;
}

void SetupPDFAProcessorParams(PDFProcessorPDFAConvertParams userParams)
{
    memset (userParams, 0, sizeof (PDFProcessorPDFAConvertParamsRec));
    userParams->size = sizeof(PDFProcessorPDFAConvertParamsRec);
    userParams->progMon = PDFProcessorProgressMonitorCB;
    userParams->noRasterizationOnFontErrors = false;
    userParams->removeAllAnnotations = false;
    userParams->colorCompression = kPDFProcessorColorJpegCompression;
    userParams->noValidationErrors = false;
    userParams->validateImplementationLimitsOfDocument = true;
}
