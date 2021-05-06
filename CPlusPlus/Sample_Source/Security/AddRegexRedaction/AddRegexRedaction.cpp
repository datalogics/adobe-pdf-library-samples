//
// Copyright (c) 2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The AddRegexRedaction sample program uses DocTextFinder to locate matches to be
// redacted in a PDF document when given a user-supplied regular expression. The text is
// permanently removed from the document.
//
// Command-line:  <input-file> <output-file> <unredacted-output-file> <search-regular-expression> (Optional)
//
// For more detail see the description of the AddRegexRedaction sample program on our Developer's site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addregexredaction

#include <map>
#include "APDFLDoc.h"
#include "InitializeLibrary.h"
#include "DLExtrasCalls.h"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddRegexRedaction.pdf"
#define DEF_OUTPUT "AddRegexRedaction-out.pdf"
#define DEF_UNREDACTED_OUTPUT "AddRegexRedaction-NotApplied-out.pdf"


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

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;
    APDFLib libInit;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::string csOutputUnredactedFileName(argc > 3 ? argv[3] : DEF_UNREDACTED_OUTPUT);
    std::string csSearchRegex(argc > 4 ? argv[4] : DEF_SEARCH_REGEX);

    std::cout << "Redacting regular expression matches for \"";
    std::cout << csSearchRegex.c_str();
    std::cout << "\" from " << csInputFileName.c_str() << ", saving to " << csOutputFileName.c_str()
              << std::endl;

    DURING

        APDFLDoc document(csInputFileName.c_str(), true);

        // Step 1) Use DocTextFinder to locate matches that will be redacted and save their quad locations.

        std::map<ASInt32, std::vector<ASFixedQuad> > pageQuadMap;

        // Setup the word finder configuration.
        PDWordFinderConfigRec wfConfig;
        memset(&wfConfig, 0, sizeof(wfConfig));           // Always do this!
        wfConfig.recSize = sizeof(PDWordFinderConfigRec); //...and this!

        // Need to set this to true so phrases will be concatenated properly
        wfConfig.noHyphenDetection = true;

        // Create the DocTextFinder object and use it to find matches.
        PDDocTextFinder matchFinder = PDDocTextFinderCreate(&wfConfig);
        PDDocTextFinderMatchList matchList = PDDocTextFinderAcquireMatchList(
            matchFinder, document.getPDDoc(), PDAllPages, 0, csSearchRegex.c_str());

        // Iterate over the matches that were found by DocTextFinder.
        for (ASUns32 matchInstance = 0; matchInstance < matchList.numMatches; ++matchInstance) {

            // Get the match.
            PDDocTextFinderMatchRec match = matchList.matches[matchInstance];

            // Store the quad location and the page it was located on.
            for (ASUns32 quadInstance = 0; quadInstance < match.numQuads; ++quadInstance) {
                pageQuadMap[match.quads[quadInstance].pageNum].push_back(match.quads[quadInstance].boundingQuad);
            }

            // Uncomment this line if you wish to print matches to the screen
            //std::cout << match.phrase << std::endl;
        }

        // Step 2) Create and apply the redactions.

        if (pageQuadMap.size() > 0) {
            PDRedactParams redactParams;
            PDRedactParamsRec rpRec;
            memset((char *)&rpRec, 0, sizeof(PDRedactParamsRec));
            redactParams = &rpRec;

            PDColorValueRec cvRec;
            PDColorValueRec borderCVRec;
            PDColorValueRec fillCVRec;

            redactParams->size = sizeof(PDRedactParamsRec);
            redactParams->colorVal = &cvRec;
            redactParams->colorVal->space = PDDeviceRGB;             // Set device color space to RGB.
            redactParams->colorVal->value[0] = FloatToASFixed(0.0);  // The redaction box will be set to black.
            redactParams->colorVal->value[1] = FloatToASFixed(0.0);
            redactParams->colorVal->value[2] = FloatToASFixed(0.0);
            redactParams->horizAlign = kPDHorizCenter;               // Horizontal alignment of the text when
                                                                     // generating the redaction mark.

            // Describe the appearance of the quads when they are drawn prior to redaction.
            redactParams->fillColor = &fillCVRec;                    // In the "normal" or "unredacted" appearance, fill each
            redactParams->fillColor->space = PDDeviceRGB;            // quad with this color, at the specified opacity.
            redactParams->fillColor->value[0] = FloatToASFixed(1.0);
            redactParams->fillColor->value[1] =
                FloatToASFixed(0.0);                                 // In this case, 0% red. Note that if opacity is set
            redactParams->fillColor->value[2] =
                FloatToASFixed(0.0);                                 // to 0, there will be no visible color, and if opacity
            redactParams->fillOpacity = FloatToASFixed(0.25);        // is set to 1, any underlaying text that is applied cannot be seen.
            redactParams->borderColor = &borderCVRec;                // Set the color to draw the border around each quad,
            redactParams->borderColor->space = PDDeviceGray;         // in the unredacted appearance.

            std::map<ASInt32, std::vector<ASFixedQuad> >::iterator iter;

            // Pass the page numbers and quads of all the matches to redactParams and create the redactions.
            for (iter = pageQuadMap.begin(); iter != pageQuadMap.end(); ++iter) {
                redactParams->pageNum = iter->first;                 // The page that the redaction will be applied to.
                redactParams->redactQuads = &(iter->second).front(); // The vector or array holding the quads.
                redactParams->numQuads = (iter->second).size();      // The number of entries in the vector or array.

                // Create the redaction annotation. At this point, the redaction has been created,
                // but the redaction has not been 'applied' yet permanently redacting the content.
                PDAnnot redactAnnot = PDDocCreateRedaction(document.getPDDoc(), redactParams);
            }

            // Save the document with the redactions created, but not applied.
            document.saveDoc(csOutputUnredactedFileName.c_str(), true);

            // Apply the redactions. IMPORTANT: until PDDocApplyRedactions is called, the
            // words are merely _marked for redaction_, but not removed!
            PDDocApplyRedactions(document.getPDDoc(), NULL);

        } else {
            std::cout << "No words were matched, no redactions needed." << std::endl;
        }

        document.saveDoc(csOutputFileName.c_str(), true);

        // Release resources here.
        PDDocTextFinderReleaseMatchList(matchFinder);
        PDDocTextFinderDestroy(matchFinder);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library.
}
