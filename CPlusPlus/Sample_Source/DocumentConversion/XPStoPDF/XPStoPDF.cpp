// Copyright (c) 2015, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates the XPS2PDF plugin, which converts a PDF document into an XPS document.
//
// For more detail see the description of the XPStoPDF sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#xpstopdf

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PDCalls.h"
#include "ASExtraCalls.h"

#include "XPS2PDFCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "XPStoPDF.xps"
#define DEF_OUTPUT "XPStoPDF-out.pdf"

int main(int argc, char **argv) {
    APDFLib lib;             // Initialize the Adobe PDF Library.
    ASErrorCode errCode = 0; // Variable used to report any exceptions/errors if they occured.

    if (lib.isValid() == false) // If there was a problem in initialization, return the error code.
        return lib.getInitError();

    DURING

        //=========================================================================================================================
        // 1) Load and configure settings for the plugin.
        //=========================================================================================================================

        gXPS2PDFHFT = InitXPS2PDFHFT; // Sets the function called by XPS2PDFInitialize().

        // Load the XPS2PDF plugin.
        if (!XPS2PDFInitialize()) {
            std::wcout << L"XPS2PDF Could not initialize:" << std::endl;
            ASRaise(ERRORCODE); // The handler will display the error code.
        }

        std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
        std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);

        std::cout << "Converting input file " << csInputFileName.c_str() << " and saving as "
                  << csOutputFileName.c_str() << std::endl;

        ASCab settings = ASCabNew();

        // The .joboptions file specifies a great number of settings which determine exactly how the
        // PDF document is created by the converter.
        ASText jobNameText =
            ASTextFromUnicode((ASUTF16Val *)"../../../../Resources/joboptions/Standard.joboptions", kUTF8);
        ASCabPutText(settings, "PDFSettings", jobNameText);

        // Specify which description in the .joboptions file we will use.
        // There are many others, for different langauges. See the file.
        ASText language = ASTextFromUnicode((ASUTF16Val *)"ENU", kUTF8);
        ASCabPutText(settings, "PDFSettingsLang", language);

        //=========================================================================================================================
        // 2) Convert the input XPS document.
        //=========================================================================================================================

        // The path of the input XPS.
        ASPathName asInPathName =
            ASFileSysCreatePathName(NULL, ASAtomFromString("Cstring"), csInputFileName.c_str(), 0);

        // We supply an empty PDDoc to convert the XPS into.
        PDDoc outputDoc = NULL;
        int ret_val = XPS2PDFConvert(settings, 0, asInPathName, NULL, &outputDoc, NULL);

        // If we succeeded, XPS2PDFConvert returns 1.
        if (ret_val != 1) {
            std::wcout << L"Conversion failed." << std::endl;
        } else {
            //=========================================================================================================================
            // 3) Save the new PDF document and release resources.
            //=========================================================================================================================

            // We construct an APDFLDoc object for this PDF to ease saving it.
            APDFLDoc outAPDoc;
            outAPDoc.pdDoc = outputDoc;
            outAPDoc.saveDoc(csOutputFileName.c_str());

            // Release the other resources we created.
            //(APDFLDoc's destructor takes care of closing the document and releasing the rest of its resources.)
            ASCabDestroy(settings);
            ASFileSysReleasePath(NULL, asInPathName);

            std::wcout << L"Successfully converted the document." << std::endl;
        }

        // Close the plugin.
        XPS2PDFTerminate();

    HANDLER

        errCode = ERRORCODE;
        lib.displayError(errCode); // If there was an error, display it.

    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library.
}