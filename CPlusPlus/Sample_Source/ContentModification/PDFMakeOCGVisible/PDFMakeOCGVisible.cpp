//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This program makes the Optional Content Groups within a PDF document visible within a viewing application,
// like Adobe Reader or Adobe Acrobat. The program finds any of these Optional Content Groups within the input
// PDF document and writes them to an output file.
//
// Optional Content Groups, also known as layers, can be used to separate and manage content or graphics on a
// single page. Layers are a very useful way to present information when opening a PDF file. For example,
// you could create a brochure with multiple layers offering the same content but in different languages.
// The first layer would be the blank background page. The resulting PDF file could be set up, with some script
// code, to select the appropriate layer with French or Spanish or English, depending on the language of the reader,
// and then display that language in the PDF file.
//

#include "CosCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "FlattenTransparency.pdf"
#define OUTPUT_FILE "PDFMakeOCGVisible-out.pdf"

ACCB1 ASBool ACCB2 showFirstThree(PDOCG ocgGroup, void *ctrVoidPtr);

int main(int argc, char *argv[]) {
    APDFLib libInit;

    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : OUTPUT_FILE);
    std::cout << "Reading file " << csInputFile.c_str() << ", writing to " << csOutputFile.c_str() << std::endl;

    // It is possible to separate a PDF document into constituent parts by opening
    // the document and pulling out the Optional Content Groups. A program function
    // could also copy the pages to a temporary document, and process the page copies
    // on a second pass.

    DURING
        APDFLDoc apdflDoc(csInputFile.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();

        PDPage pdPage = PDDocAcquirePage(pdDoc, 0);

        if (PDDocHasOC(pdDoc)) {
            ASUns32 ocgCounter = 1;

            // Enumerate the optional content, making the first three elements
            // visible and the rest invisible
            PDPageEnumOCGs(pdPage, showFirstThree, (void *)&ocgCounter);

            // "Flatten" the PDPage's optional content, so that the PDPage has its visible
            // content layers visible, and the invisible content is not on the page
            PDPageFlattenOC(pdPage, PDDocGetOCContext(pdDoc));

            // The above call does not remove the actual Optional Content structure. That is completed below.
            CosDoc cosDoc = PDDocGetCosDoc(pdDoc);
            CosObj cosCatalog = CosDocGetRoot(cosDoc);
            if (CosDictKnown(cosCatalog, ASAtomFromString("OCProperties"))) {
                CosDictRemove(cosCatalog, ASAtomFromString("OCProperties"));
            }
        } else {
            std::cout << "Document does not have any OC\n";
        }

        PDPageRelease(pdPage);
        apdflDoc.saveDoc(csOutputFile.c_str());
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}

// Switch the first three OCG groups passed in to the visible state,
// and switch all subsequent OCG groups to the invisible state.
ACCB1 ASBool ACCB2 showFirstThree(PDOCG ocgGroup, void *ctrVoidPtr) {
    ASUns32 *ctrPtr = (ASUns32 *)ctrVoidPtr;

    if (!ocgGroup || !ctrVoidPtr) {
        return TRUE; // This result would rarely appear
    }

    DURING
        PDDoc curPDDoc = PDOCGGetPDDoc(ocgGroup);
        PDOCConfig curOCconfig = PDDocGetOCConfig(curPDDoc);
        PDOCContext curOCcontext = PDDocGetOCContext(curPDDoc);

        // Get the name of this layer, and convert to a Roman character set
        ASText groupASName = PDOCGGetName(ocgGroup);
        if (groupASName) {
            std::cout << "Found group \"" << ASTextGetScriptText(groupASName, kASRomanScript) << "\", ";
            ASTextDestroy(groupASName);
        }

        if (*ctrPtr) {
            std::cout << "Setting group on...\n";
            PDOCGSetInitialState(ocgGroup, curOCconfig, TRUE);
            PDOCGSetCurrentState(ocgGroup, curOCcontext, TRUE);
        } else {
            std::cout << "Setting group off...\n";
            PDOCGSetInitialState(ocgGroup, curOCconfig, FALSE);
            PDOCGSetCurrentState(ocgGroup, curOCcontext, FALSE);
        }

    HANDLER
        std::cout << "Exception " << ERRORCODE << " in function \"showFirstThree\"\n";
    END_HANDLER

    // Verify the number of layers processed. This sample completes only the first three.
    if (*ctrPtr > 0) {
        *ctrPtr = *ctrPtr + 1;
        if (*ctrPtr >= 4)
            *ctrPtr = 0;
    }

    return TRUE;
}
