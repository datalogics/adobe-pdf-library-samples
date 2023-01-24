//
// Copyright (c) 2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample extracts text that matches a given pattern using regular expressions (regex)
// in a PDF document and saves the text to a file.
//
// For more detail see the description of the ExtractTextByPatternMatch sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples

#include <fstream>
#include <string>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "DLExtrasCalls.h"
#include "TextExtract.h"

const char *DEF_INPUT = "../../../../Resources/Sample_Input/ExtractTextByPatternMatch.pdf";
const char *DEF_OUTPUT = "ExtractTextByPatternMatch-out.txt";

// This sample will look for text that matches a phone number pattern
const char *DEF_PATTERN = regexPattern[PHONE_PATTERN];

int main(int argc, char **argv) {

    // Initialize the library
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    DURING

        APDFLDoc inAPDoc(DEF_INPUT, true);
        std::ofstream outputFile(DEF_OUTPUT);

        // Use PDWordfinder to find all the Words in our input document
        PDWordFinderConfigRec wfConfig;
        memset(&wfConfig, 0, sizeof(PDWordFinderConfigRec));
        wfConfig.recSize = sizeof(PDWordFinderConfigRec);

        // Need to set this to true so phrases will be concatenated properly
        wfConfig.noHyphenDetection = true;

        // Get the matches for this document
        PDDocTextFinder matchFinder = PDDocTextFinderCreate(&wfConfig);
        PDDocTextFinderMatchList matchList =
            PDDocTextFinderAcquireMatchList(matchFinder, inAPDoc.getPDDoc(), PDAllPages, 0, DEF_PATTERN);

        // Iterate over the matches that were found by PDDocTextFinder and print them out in the
        // output file.
        for (size_t matchInstance = 0; matchInstance < matchList.numMatches; ++matchInstance) {

            PDDocTextFinderMatchRec match = matchList.matches[matchInstance];

            // Put this matched phrase in the output document
            outputFile << match.phrase << std::endl;
        }

        // Release resources here.
        PDDocTextFinderReleaseMatchList(matchFinder);
        PDDocTextFinderDestroy(matchFinder);

        // Close the output file.
        outputFile.close();

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
