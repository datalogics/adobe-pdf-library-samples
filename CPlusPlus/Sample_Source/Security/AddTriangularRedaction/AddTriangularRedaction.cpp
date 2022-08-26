//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The AddTriangularRedaction sample redacts each page of a document with
// two full-page triangles, leaving only a narrow diagonal strip across each page.
// Text found within the triangular shapes is removed from the document, and gray
// redaction triangles are added.
//
// Command-line:  <input-file> <output-file>    (Both optional)
//
// For more detail see the description of the AddTriangularRedaction sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples/#addtriangularredaction

#include <iostream>
#include <string>
#include <algorithm>
#include "CosCalls.h"
#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddTriangularRedaction.pdf"
#define DEF_OUTPUT "AddTriangularRedaction-out.pdf"

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
    std::cout << "Redacting trianglular areas from " << csInputFileName.c_str() << ", saving to "
              << csOutputFileName.c_str() << "\n"
              << std::endl;

    DURING

        APDFLDoc document(csInputFileName.c_str(), true);

        // Step 1) Step through each page of the document, and get page width and height

        // Iterate through each page in the PDDoc
        for (ASInt32 pageNum = 0; pageNum < (PDDocGetNumPages(document.getPDDoc())); ++pageNum) {
            PDPage pdPage = document.getPage(pageNum); // Get current page
            ASFixedRect bounds;
            PDPageGetCropBox(pdPage, &bounds); // Get crop box of page

            ASFixed pageWidth = bounds.right - bounds.left;    // Get page width
            ASFixed pageHeight = bounds.top - bounds.bottom;   // Get page height
            ASFixed stripWidth = fixedSeventyTwo + fixedThree; // Set distance between triangles

            std::wcout << L"Page " << pageNum << std::endl;

            // Step 2) Calculate four quad coordinates to represent each of the two full-page
            //   triangles, leaving a diagonal strip between them across the middle of the current page

            std::vector<ASFixedQuad> quadVector;
            ASFixedQuad quad;

            // First quad represents triangle covering top left of page

            ASFixedPoint topLeft;
            topLeft.h = bounds.left;
            topLeft.v = bounds.bottom + pageHeight;
            quad.tl = topLeft; // Set first quad's top left coordinate

            ASFixedPoint topRight;
            topRight.h = bounds.left + pageWidth - stripWidth; // Include offset for space between triangles
            topRight.v = bounds.bottom + pageHeight;
            quad.tr = topRight; // Set first quad's top right coordinate

            ASFixedPoint bottomRight;
            bottomRight.h = bounds.left;
            bottomRight.v = bounds.bottom + stripWidth;
            quad.br = bottomRight; // Set first quad's bottom right coordinate

            ASFixedPoint bottomLeft;
            bottomLeft.h = bounds.left; // Use same coordinate for bottomRight, bottomLeft
            bottomLeft.v = bounds.bottom + stripWidth;
            quad.bl = bottomLeft; // Set first quad's bottom left coordinate

            quadVector.push_back(quad); // Store first quad in vector

            // Second quad represents triangle covering bottom right of page

            topLeft.h = bounds.left + pageWidth;
            topLeft.v = bounds.bottom + pageHeight - stripWidth;
            quad.tl = topLeft; // Set second quad's top left coordinate

            topRight.h = bounds.left + pageWidth; // Use same coordinate for topLeft, topRight
            topRight.v = bounds.bottom + pageHeight - stripWidth;
            quad.tr = topRight; // Set second quad's top right coordinate

            bottomRight.h = bounds.left + pageWidth;
            bottomRight.v = bounds.bottom;
            quad.br = bottomRight; // Set second quad's bottom right coordinate

            bottomLeft.h = bounds.left + stripWidth;
            bottomLeft.v = bounds.bottom;
            quad.bl = bottomLeft; // Set second quad's bottom left coordinate

            quadVector.push_back(quad); // Store second quad in vector

            // Step 3) Create and apply the redactions. The redaction configurations are set, the
            //   redaction is created and finally applied.

            PDRedactParams redactParams;
            PDRedactParamsRec rpRec;
            memset((char *)&rpRec, 0, sizeof(PDRedactParamsRec));
            redactParams = &rpRec;

            PDColorValueRec cvRec;

            redactParams->size = sizeof(PDRedactParamsRec);
            redactParams->pageNum = pageNum; // Page that the redaction will be applied to
            redactParams->redactQuads = &quadVector.front(); // Vector or array holding the quads
            redactParams->numQuads = quadVector.size(); // Number of entries in the vector or array
            redactParams->colorVal = &cvRec;
            redactParams->colorVal->space = PDDeviceRGB; // Set device color space to RGB
            redactParams->colorVal->value[0] = fixedHalf; // The redaction box will be set to 50% gray
            redactParams->colorVal->value[1] = fixedHalf;
            redactParams->colorVal->value[2] = fixedHalf;
            redactParams->horizAlign =
                kPDHorizLeft; // Horizontal alignment of the text when generating the redaction mark
            redactParams->overlayText = NULL; // Overlay text may be used to replace the underlying content

            // Create the redaction annotation. At this point the text HAS NOT been redacted.
            PDAnnot redactAnnot = PDDocCreateRedaction(document.getPDDoc(), redactParams);

            // Apply the redactions. IMPORTANT: until PDDocApplyRedactions is called, the
            //   words are merely _marked for redaction_, but not removed!
            PDDocApplyRedactions(document.getPDDoc(), NULL);
            std::wcout << L"Words have been permanently removed.\n" << std::endl;

            PDPageRelease(pdPage);
        }

        document.saveDoc(csOutputFileName.c_str(), true); // Save the redacted document

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode); // If there was an error, display the error that occured
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library
}
