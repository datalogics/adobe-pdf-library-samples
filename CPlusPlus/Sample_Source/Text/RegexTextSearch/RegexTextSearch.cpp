//
// Copyright (c) 2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates using the DocTextFinder to find examples of a specific phrase in a PDF
// document that matches a user-supplied regular expression. When the sample finds the text it
// highlights each example and saves the file as an output document.
//
// Command-line:   <input-file> <output-file> <search-regular-expression>    (All optional)
//
// For more detail see the description of the RegexTextSearch sample program on our Developer's site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#regextextsearch

#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "CosCalls.h"
#include "DLExtrasCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "RegexTextSearch.pdf"
#define DEF_OUTPUT "RegexTextSearch-out.pdf"

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

static void ApplyQuadsToAnnot(PDAnnot, ASFixedQuad *, ASArraySize);
static void AnnotateMatch(ASFixedQuad, PDPage, PDColorValue);

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
              << "the regular expression \"" << csSearchRegex.c_str() << "\"," << std::endl
              << "highlight them all, "
              << " and save to " << csOutputFileName.c_str() << std::endl;

    DURING

        APDFLDoc document(csInputFileName.c_str(), true);

        // Step 1) Setup the word finder configuration.
        PDWordFinderConfigRec wfConfig;
        memset(&wfConfig, 0, sizeof(wfConfig));           // Always do this!
        wfConfig.recSize = sizeof(PDWordFinderConfigRec); //...and this!

        // Need to set this to true so phrases will be concatenated properly
        wfConfig.noHyphenDetection = true;

        // Step 2) Fill in color information for highlighting text. In this case, our color will be set to orange.

        PDColorValueRec colorValRec;
        PDColorValue pdColorValue(&colorValRec);
        pdColorValue->space = PDDeviceRGB;             // Colors are set using the RGB color space.
        pdColorValue->value[0] = ASFloatToFixed(1.0);  // red level
        pdColorValue->value[1] = ASFloatToFixed(0.65); // green level
        pdColorValue->value[2] = ASFloatToFixed(0.0);  // blue level

        // Step 3) Search for the text that matches the regular expression and highlight all occurrences.

        PDDocTextFinder matchFinder = PDDocTextFinderCreate(&wfConfig);
        PDDocTextFinderMatchList matchList = PDDocTextFinderAcquireMatchList(
            matchFinder, document.getPDDoc(), PDAllPages, 0, csSearchRegex.c_str());

        // Iterate over the matches that were found by DocTextFinder
        for (ASUns32 matchInstance = 0; matchInstance < matchList.numMatches; ++matchInstance) {

            PDDocTextFinderMatchRec match = matchList.matches[matchInstance];

            for (ASUns32 quadInstance = 0; quadInstance < match.numQuads; ++quadInstance) {
                // The PDPage object will be needed for adding the highlight annotation
                PDPage pdPage = document.getPage(match.quads[quadInstance].pageNum);
                AnnotateMatch(match.quads[quadInstance].boundingQuad, pdPage, pdColorValue);
                PDPageRelease(pdPage);
            }

            // Uncomment this line if you wish to print matches to the screen
            //std::cout << match.phrase << std::endl;
        }

        document.saveDoc(csOutputFileName.c_str());

        // Release resources here.
        PDDocTextFinderReleaseMatchList(matchFinder);
        PDDocTextFinderDestroy(matchFinder);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

// Add quads to an annotation's CosObj.
// NOTE: There is Adobe documentation that specifies quads be added in the order - BL, BR, TR, TL.
// However, currently the order needs to be BL, BR, TL, TR to get correct output.
void ApplyQuadsToAnnot(PDAnnot annot, ASFixedQuad *quads, ASArraySize numQuads) {
    CosObj coAnnot = PDAnnotGetCosObj(annot);
    CosDoc coDoc = CosObjGetDoc(coAnnot);
    CosObj coQuads = CosNewArray(coDoc, false, numQuads * 8);
    ASAtom atQP = ASAtomFromString("QuadPoints");

    for (ASUns32 i = 0, n = 0; i < numQuads; ++i) {
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].bl.h));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].bl.v));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].br.h));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].br.v));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tl.h)); // See note above
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tl.v));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tr.h)); // See note above
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tr.v));
    }

    CosDictPut(coAnnot, atQP, coQuads);
}

void AnnotateMatch(ASFixedQuad quad, PDPage p, PDColorValue c) {
    ASAtom atH = ASAtomFromString("Highlight");

    // A value of -2 adds the annotation to the end of the page's annotation array
    ASInt32 addAfterCode = -2;

    // Coordinates of the annotation
    ASFixedRect annotationRect;
    annotationRect.left = quad.bl.h;
    annotationRect.top = quad.tr.v;
    annotationRect.right = quad.tr.h;
    annotationRect.bottom = quad.bl.v;

    // Create the annotation
    PDAnnot highlight = PDPageCreateAnnot(p, atH, &annotationRect);

    // Set its coordinates and color
    ApplyQuadsToAnnot(highlight, &quad, 1);
    PDAnnotSetColor(highlight, c);

    PDPageAddAnnot(p, addAfterCode, highlight);
}
