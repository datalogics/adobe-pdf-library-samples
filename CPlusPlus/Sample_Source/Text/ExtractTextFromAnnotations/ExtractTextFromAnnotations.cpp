//
// Copyright (c) 2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
//  This sample extracts text from the Annotations in a PDF
//  document and saves the text to a file.
//
// For more detail see the description of the ExtractTextFromAnnotations sample program on our Developer’s
// site, http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples

#include <fstream>
#include <string>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "TextExtract.h"

#ifdef MAC_PLATFORM
#include "../../_Common/ThirdParty/json.hpp"
#else
#include "ThirdParty/json.hpp"
#endif

// Set the namespace for the JSON library
using json = nlohmann::json;

const char *DEF_INPUT = "../../../../Resources/Sample_Input/sample_annotations.pdf";
const char *DEF_OUTPUT = "ExtractTextFromAnnotations-out.json";

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

        // This array will hold the JSON stream that we will print to the output JSON file.
        json result = json::array();

        // Create the TextExtract object
        TextExtract textExtract(inAPDoc.getPDDoc());

        std::vector<PDAnnotationExtractRec> extractedText = textExtract.GetAnnotationText();

        for (size_t textIndex = 0; textIndex < extractedText.size(); ++textIndex)
        {
            json textObject = json::object();

            textObject["annotation-type"] = extractedText[textIndex].type;
            textObject["annotation-text"] = extractedText[textIndex].text;
            result.push_back(textObject);
        }
        std::ofstream jsonOutputFileStream(DEF_OUTPUT, std::ios_base::trunc | std::ios_base::out);
        jsonOutputFileStream << std::setw(4) << result;
        jsonOutputFileStream.close();

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
