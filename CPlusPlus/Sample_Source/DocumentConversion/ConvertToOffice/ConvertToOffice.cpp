//
// Copyright (c) 2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// ConvertToOffice converts sample PDF documents to Office Documents.
//
// For more detail see the description of the ConvertToOffice sample program on our Developer's site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#converttooffice

#include <sstream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "DLExtrasCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT_WORD "Word.pdf"
#define DEF_INPUT_EXCEL "Excel.pdf"
#define DEF_INPUT_POWERPOINT "PowerPoint.pdf"
#define DEF_OUTPUT_WORD "ConvertToWord-out.docx"
#define DEF_OUTPUT_EXCEL "ConvertToExcel-out.xlsx"
#define DEF_OUTPUT_POWERPOINT "ConvertToPowerPoint-out.pptx"

enum OfficeType
{
    Word = 0,
    Excel = 1,
    PowerPoint = 2
};

void ConvertToOffice(const char* inputFileName, const char* outputFileName, OfficeType officeType)
{
    ASPathName inputPathName = APDFLDoc::makePath(inputFileName);
    ASPathName outputPathName = APDFLDoc::makePath(outputFileName);

    ASFileSys fileSys = ASGetDefaultFileSys();

    ASBool result = false;

    if (officeType == Word)
    {
        result = ConvertPDFToWord(inputPathName, outputPathName, fileSys);
    }
    else if (officeType == Excel)
    {
        result = ConvertPDFToExcel(inputPathName, outputPathName, fileSys);
    }
    else if (officeType == PowerPoint)
    {
        result = ConvertPDFToPowerPoint(inputPathName, outputPathName, fileSys);
    }

    std::cout << "Conversion of file " << inputFileName << " has ";

    if (result) {
        std::cout << "been successfully Converted to " << outputFileName << std::endl;
    }
    else {
        std::cout << "failed..." << std::endl;
    }

    ASFileSysReleasePath(fileSys, inputPathName);
    ASFileSysReleasePath(fileSys, outputPathName);
}

int main(int argc, char **argv) {
    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }

    std::string csInputWordFileName(DIR_LOC DEF_INPUT_WORD);
    std::string csOutputWordFileName(DEF_OUTPUT_WORD);
    std::string csInputExcelFileName(DIR_LOC DEF_INPUT_EXCEL);
    std::string csOutputExcelFileName(DEF_OUTPUT_EXCEL);
    std::string csInputPowerPointFileName(DIR_LOC DEF_INPUT_POWERPOINT);
    std::string csOutputPowerPointFileName(DEF_OUTPUT_POWERPOINT);

    DURING
        ConvertToOffice(csInputWordFileName.c_str(), csOutputWordFileName.c_str(), Word);
        ConvertToOffice(csInputExcelFileName.c_str(), csOutputExcelFileName.c_str(), Excel);
        ConvertToOffice(csInputPowerPointFileName.c_str(), csOutputPowerPointFileName.c_str(), PowerPoint);
    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates APDFL
}

