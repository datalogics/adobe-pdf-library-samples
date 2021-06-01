//
// Copyright (c) 2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates using PDDocTextFinder to find instances of a phrase
// that matches a user-supplied regular expression. The output is a JSON file that
// has the match information.
//
// Command-line:   <input-file> <output-file> <search-regular-expression>    (All optional)
//
// For more detail see the description of the RegexExtractText sample program on our Developer's site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#regexextracttext

#include <fstream>
#include <iomanip>
#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "CosCalls.h"
#include "DLExtrasCalls.h"

#ifdef MAC_PLATFORM
#include "../../_Common/ThirdParty/json.hpp"
#else
#include "ThirdParty/json.hpp"
#endif

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "RegexExtractText.pdf"
#define DEF_OUTPUT "RegexExtractText-out.json"

// Set the namespace for the JSON library
using json = nlohmann::json;

// If compiler supports C++11 or greater, then a raw string can be used.
// Uncomment only one of the given regular expressions to see the results
// properly displayed in the output document.
#if __cplusplus >= 201103L
// Phone numbers
#define DEF_SEARCH_REGEX R"((1-)?(\()?\d{3}(\))?(\s)?(-)?\d{3}-\d{4})"
// Email addresses
//#define DEF_SEARCH_REGEX R"(\b[\w.!#$%&'*+\/=?^`{|}~-]+@[\w-]+(?:\.[\w-]+)*\b)"
// URLs
//#define DEF_SEARCH_REGEX R"((https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}))"
#else
// Phone numbers
#define DEF_SEARCH_REGEX "(1-)?(\\()?\\d{3}(\\))?(\\s)?(-)?\\d{3}-\\d{4}"
// Email addresses
//#define DEF_SEARCH_REGEX "\\b[\\w.!#$%&'*+\\/=?^`{|}~-]+@[\\w-]+(?:\\.[\\w-]+)*\\b"
// URLs
//#define DEF_SEARCH_REGEX "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,})"
#endif

int main(int argc, char *argv[]) {
    APDFLib libInit;
    ASErrorCode errCode = 0;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::string csSearchRegex(argc > 3 ? argv[3] : DEF_SEARCH_REGEX);
    std::cout << "Will search " << csInputFileName.c_str() << " for all occurrences of "
              << "the regular expression \"" << csSearchRegex.c_str() << "\","
              << " and save to " << csOutputFileName.c_str() << std::endl;

    DURING

        APDFLDoc document(csInputFileName.c_str(), true);

        // This array will hold the JSON stream that we will print to the output JSON file.
        json result = json::array();

        // Step 1) Setup the word finder configuration.
        PDWordFinderConfigRec wfConfig;
        memset(&wfConfig, 0, sizeof(wfConfig));           // Always do this!
        wfConfig.recSize = sizeof(PDWordFinderConfigRec); //...and this!

        // Need to set this to true so phrases will be concatenated properly.
        wfConfig.noHyphenDetection = true;

        // Step 2) Search for the text that matches the regular expression and add to JSON array that we will print to a file later.
        PDDocTextFinder matchFinder = PDDocTextFinderCreate(&wfConfig);
        PDDocTextFinderMatchList matchList = PDDocTextFinderAcquireMatchList(
            matchFinder, document.getPDDoc(), PDAllPages, 0, csSearchRegex.c_str());

        // Iterate over the matches that were found by PDDocTextFinder.
        for (ASUns32 matchInstance = 0; matchInstance < matchList.numMatches; ++matchInstance) {

            // This JSON object will store the match phrase and an array of quads for the match.
            json matchObject = json::object();

            // This JSON array will store the page number and quad location for each match quad.
            json matchQuadInformation = json::array();

            PDDocTextFinderMatchRec match = matchList.matches[matchInstance];

            // Set the match phrase in the JSON object.
            matchObject["match-phrase"] = match.phrase;

            for (ASUns32 quadInstance = 0; quadInstance < match.numQuads; ++quadInstance) {

                // Get the coordinates of the quad.
                double top_left_x = ASFixedToFloat(match.quads[quadInstance].boundingQuad.tl.h);
                double top_left_y = ASFixedToFloat(match.quads[quadInstance].boundingQuad.tl.v);

                double bottom_left_x = ASFixedToFloat(match.quads[quadInstance].boundingQuad.bl.h);
                double bottom_left_y = ASFixedToFloat(match.quads[quadInstance].boundingQuad.bl.v);

                double top_right_x = ASFixedToFloat(match.quads[quadInstance].boundingQuad.tr.h);
                double top_right_y = ASFixedToFloat(match.quads[quadInstance].boundingQuad.tr.v);

                double bottom_right_x = ASFixedToFloat(match.quads[quadInstance].boundingQuad.br.h);
                double bottom_right_y = ASFixedToFloat(match.quads[quadInstance].boundingQuad.br.v);

                // Set the quad's page number in a JSON object.
                json tempQuadPageInformation = {"page-number", match.quads[quadInstance].pageNum};

                // Set the quad's location in a JSON object.
                json tempQuadInformation = {
                    "quad-location",
                    {{"top-left", {{"x", top_left_x}, {"y", top_left_y}}},
                     {"bottom-left", {{"x", bottom_left_x}, {"y", bottom_left_y}}},
                     {"top-right", {{"x", top_right_x}, {"y", top_right_y}}},
                     {"bottom-right", {{"x", bottom_right_x}, {"y", bottom_right_y}}}}};

                // Insert the match's page number and quad location(s) in the matchQuadInformation JSON array.
                matchQuadInformation.push_back({tempQuadPageInformation, tempQuadInformation});
            }

            // Set the match's quad information in the matchObject.
            matchObject["match-quads"] = matchQuadInformation;

            result.push_back(matchObject);
        }

        // Step 3) Write the match information to the output JSON file.
        std::ofstream jsonOutputFileStream(csOutputFileName, std::ios_base::trunc | std::ios_base::out);
        jsonOutputFileStream << std::setw(4) << result;

        // Release resources here.
        PDDocTextFinderReleaseMatchList(matchFinder);
        PDDocTextFinderDestroy(matchFinder);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
