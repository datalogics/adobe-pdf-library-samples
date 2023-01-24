//
// Copyright (c) 2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample extracts text from a specific region of a page in a PDF
// document and saves the text to a file.
//
// For more detail see the description of the ExtractTextByRegion sample program on our Developer’s
// site, http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples

#include <fstream>
#include <string>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "TextExtract.h"

const char *DEF_INPUT = "../../../../Resources/Sample_Input/ExtractTextByRegion.pdf";
const char *DEF_OUTPUT = "ExtractTextByRegion-out.txt";

// Rectangular region to extract text in points (origin of the page is bottom left)
// (545,576,694,710) is a rectangle encompassing the invoice entry for this sample.
const float DEF_LEFT = 545;   // left
const float DEF_RIGHT = 576;  // right
const float DEF_BOTTOM = 694; // bottom
const float DEF_TOP = 710;    // top

bool CheckWithinRegion(DLQuadFloat wordQuad);

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

        TextExtract textExtract(inAPDoc.getPDDoc());

        std::vector<PDTextAndDetailsExtractRec> extractedText = textExtract.GetTextAndDetails();

        for (size_t textIndex = 0; textIndex < extractedText.size(); ++textIndex) {

            bool allQuadsWithinRegion = true;
            // A Word typically has only 1 quad, but can have more than one for hyphenated words, words on a curve, etc.
            for (size_t quadIndex = 0; quadIndex < extractedText[textIndex].boundingQuads.size(); ++quadIndex) {

                DLQuadFloat wordQuad = extractedText[textIndex].boundingQuads[quadIndex];
                if (!CheckWithinRegion(wordQuad)) {
                    allQuadsWithinRegion = false;
                }
            }
            if (allQuadsWithinRegion)
                // Put this Word that meets our criteria in the output document
                outputFile << extractedText[textIndex].text << std::endl;
        }
        outputFile.close();

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

// For this sample, we will consider a Word to be in the region of interest if the
// complete Word fits within the specified rectangular box
bool CheckWithinRegion(DLQuadFloat wordQuad) {

    if (wordQuad.bl.h >= DEF_LEFT && wordQuad.br.h <= DEF_RIGHT &&
        wordQuad.tl.h >= DEF_LEFT && wordQuad.tr.h <= DEF_RIGHT &&
        wordQuad.bl.v >= DEF_BOTTOM && wordQuad.tl.v <= DEF_TOP &&
        wordQuad.br.v >= DEF_BOTTOM && wordQuad.tr.v <= DEF_TOP) {

        return true;
    }
    return false;
}
