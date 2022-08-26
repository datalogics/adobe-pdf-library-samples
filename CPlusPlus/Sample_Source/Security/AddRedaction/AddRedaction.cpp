//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The AddRedaction sample program uses PDWordFinder to locate text to be redacted in
// a PDF document. The text is permanently removed from the document.
//
// Command-line:  <input-file> <output-file>    (Both optional)
//
// For more detail see the description of the AddRedaction sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addredaction

#include <iostream>
#include <string>
#include <algorithm>
#include "CosCalls.h"
#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddRedaction.pdf"
#define DEF_OUTPUT "AddRedaction-out.pdf"
#define DEF_UNREDACTED_OUTPUT "AddRedaction-NotApplied-out.pdf"

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;
    APDFLib libInit;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    const wchar_t *rWords[2] = {L"navigation", L"screen"};
    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::string csOutputUnredactedFileName(argc > 3 ? argv[3] : DEF_UNREDACTED_OUTPUT);
    std::cout << "Redacting words \"";
    std::wcout << rWords[0] << L"\" and \"" << rWords[1];
    std::cout << "\" from " << csInputFileName.c_str() << ", saving to " << csOutputFileName.c_str()
              << std::endl;

    DURING

        APDFLDoc document(csInputFileName.c_str(), true);

        // Step 1) Use PDWordFinder to locate words that will be redacted and save their locations

        std::vector<ASFixedQuad> quadVector;

        // Set the default word finder settings.
        PDWordFinderConfigRec wfConfig;
        memset(&wfConfig, 0, sizeof(PDWordFinderConfigRec));
        wfConfig.recSize = sizeof(PDWordFinderConfigRec);

        // Create the word finder object.
        PDWordFinder wordFinder =
            PDDocCreateWordFinderEx(document.getPDDoc(), WF_LATEST_VERSION, true, &wfConfig);
        // Create a PDWord object used to hold individual words.
        PDWord pdWordArray;

        ASInt32 numberOfWords = 0;

        PDWordFinderAcquireWordList(wordFinder, 0, &pdWordArray, NULL, NULL, &numberOfWords);

        for (ASInt32 index = 0; index < numberOfWords; ++index) {
            // Get the PDWord at the given index.
            PDWord pdWord = PDWordFinderGetNthWord(wordFinder, index);

            // Convert word to an ASText object.
            ASText asTextWord = ASTextNew();

            PDWordGetASText(pdWord, 0, asTextWord);

            // Convert ASText object to a wstring.
            std::wstring testString = reinterpret_cast<wchar_t *>(
                ASTextGetUnicodeCopy(asTextWord, APDFLDoc::GetHostUnicodeFormat()));

            std::transform(testString.begin(), testString.end(), testString.begin(), ::tolower);

            // If either string is matched, push the words quad points into a vector.
            if ((wcsstr(testString.c_str(), rWords[0]) != NULL) ||
                (wcsstr(testString.c_str(), rWords[1]) != NULL)) {
                ASFixedQuad quad;
                PDWordGetNthQuad(pdWord, 0, &quad);
                quadVector.push_back(quad);
            }

            ASTextDestroy(asTextWord);
        }

        PDWordFinderReleaseWordList(wordFinder, 0);

        // Step 2) Create and apply the redactions.

        if (quadVector.size() > 0) {
            PDRedactParams redactParams;
            PDRedactParamsRec rpRec;
            memset((char *)&rpRec, 0, sizeof(PDRedactParamsRec));
            redactParams = &rpRec;

            PDColorValueRec cvRec;
            PDColorValueRec borderCVRec;
            PDColorValueRec fillCVRec;
            PDColorValueRec textCVRec;

            redactParams->size = sizeof(PDRedactParamsRec);
            redactParams->pageNum = 0; // The page that the redaction will be applied to.
            redactParams->redactQuads = &quadVector.front(); // The vector or array holding the quads.
            redactParams->numQuads = static_cast<ASInt32>(quadVector.size()); // The number of entries in the vector or array.
            redactParams->colorVal = &cvRec;
            redactParams->colorVal->space = PDDeviceRGB;            // Set device color space to RGB
            redactParams->colorVal->value[0] = FloatToASFixed(0.0); // The redaction box will be set to black.
            redactParams->colorVal->value[1] = FloatToASFixed(0.0);
            redactParams->colorVal->value[2] = FloatToASFixed(0.0);
            redactParams->horizAlign = kPDHorizCenter; // Horizontal alignment of the text when
                                                       //  generating the redaction mark.

            // Describe the appearance of the text that is drawn to overlay the quads, when they are
            // redacted, or when they are viewed as if redacted.
            redactParams->overlayText = ASTextFromPDText(" Redacted "); // Overlay text may be used to overprint
                                                                        // the redacted areas
            redactParams->textColor = &textCVRec;
            redactParams->textColor->space = PDDeviceGray; // Draw the overlay text in white
            redactParams->textColor->value[0] = fixedOne;
            redactParams->textFontName = const_cast<char*>("CourierStd");     // Draw the overlay text using CourierStd font
            redactParams->textSize = FloatToASFixed(10.0); // Draw the text size as 10 point (nominally, may
                                                           // be reduced is ScaleToFit is true).
            redactParams->repeat = true; // Draw the text in each quad, repeating to fill the quad
            redactParams->scaleToFit = true; // Reduce text size if needed, to fit the quad.

            // Describe the appearance of the quads when they are drawn prior to redaction
            redactParams->fillColor = &fillCVRec; // In the "normal" or "unredacted" appearance, fill each
            redactParams->fillColor->space = PDDeviceRGB; // quad with this color, at the specified opacity
            redactParams->fillColor->value[0] = FloatToASFixed(1.0); //
            redactParams->fillColor->value[1] =
                FloatToASFixed(0.0); // In this case, 25% red. Note that if opacity is set
            redactParams->fillColor->value[2] =
                FloatToASFixed(0.0); // to 0, there will be no visible color, and if opacity
            redactParams->fillOpacity = FloatToASFixed(0.25); // is set to 1, the underlaying text cannot be seen.
            redactParams->borderColor = &borderCVRec; // Set the color ro draw the border around each quad,
            redactParams->borderColor->space = PDDeviceGray; // in the unredacted appearance.

            // Create the redaction annotation. At this point the text HAS NOT been redacted.
            PDAnnot redactAnnot = PDDocCreateRedaction(document.getPDDoc(), redactParams);

            // Save the document with the redactions created, but not applied
            document.saveDoc(csOutputUnredactedFileName.c_str(), true);

            // Apply the redactions. IMPORTANT: until PDDocApplyRedactions is called, the
            //    words are merely _marked for redaction_, but not removed!
            PDDocApplyRedactions(document.getPDDoc(), NULL);

            std::cout << quadVector.size() << " words have been permanently removed...  ";
        } else
            std::cout << "No words were matched, no redactions needed." << std::endl;

        // Step 3) Verify that the words were permanently removed.

        if (quadVector.size() > 0) {
            wordFinder = PDDocCreateWordFinderEx(document.getPDDoc(), WF_LATEST_VERSION, true, &wfConfig);

            numberOfWords = 0;
            memset(&pdWordArray, 0, sizeof(pdWordArray));

            PDWordFinderAcquireWordList(wordFinder, 0, &pdWordArray, NULL, NULL, &numberOfWords);

            ASInt32 cnt = 0;

            for (ASInt32 index = 0; index < numberOfWords; ++index) {
                PDWord pdWord = PDWordFinderGetNthWord(wordFinder, index); // Get the PDWord at the given index.

                ASText asTextWord = ASTextNew(); // Convert word to an ASText object.

                PDWordGetASText(pdWord, 0, asTextWord);

                // Convert ASText object to a wstring.
                std::wstring testString = reinterpret_cast<wchar_t *>(
                    ASTextGetUnicodeCopy(asTextWord, APDFLDoc::GetHostUnicodeFormat()));

                std::transform(testString.begin(), testString.end(), testString.begin(), ::tolower);

                // If either string is matched, increment the count so we can report them.
                if ((wcsstr(testString.c_str(), rWords[0]) != NULL) ||
                    (wcsstr(testString.c_str(), rWords[1]) != NULL)) {
                    ++cnt;
                }
                ASTextDestroy(asTextWord);
            }

            PDWordFinderReleaseWordList(wordFinder, 0);

            if (0 == cnt) {
                std::cout << "Verified." << std::endl;
            } else {
                std::cout << "Some words remain in the document that weren't redacted yet." << std::endl;
            }
        }
        document.saveDoc(csOutputFileName.c_str(), true);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library.
}
