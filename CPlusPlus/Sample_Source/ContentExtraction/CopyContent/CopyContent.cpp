//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// CopyContent copies content from an input PDF document into a new PDF document
// and then saves the output file. You can specify the type of content you want to copy.
//
// Command-line:   <input-file>  <output-file>     (Both optional)
//
// For more detail see the description of the CopyContent sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#copycontent

#include <iostream>
#include <map>
#include <set>
#include <utility>
#include <string>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "ASExtraCalls.h"
#include "PDCalls.h"
#include "ASCalls.h"
#include "PEExpT.h"
#include "PagePDECntCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "CopyContent.pdf"
#define DEF_OUTPUT "CopyContent-out.pdf"

// Copies all elements in "from" into "to".
static void copyElements(PDEContent to, PDEContent from, const std::set<ASInt32> &willCopyList);

int main(int argc, char **argv) {
    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Copying (selected elements) from " << csInputFileName.c_str()
              << " and writing to " << csOutputFileName.c_str() << std::endl;

    // Step 1) Determine what kinds of content will be copied.

    // All element types in this set will be copied, others will be ignored
    std::set<ASInt32> sContentTypes;
    sContentTypes.insert(kPDEContainer);
    sContentTypes.insert(kPDEForm);
    sContentTypes.insert(kPDEGroup);
    sContentTypes.insert(kPDEImage);
    sContentTypes.insert(kPDEPath);
    sContentTypes.insert(kPDEPlace);
    sContentTypes.insert(kPDEPS);
    sContentTypes.insert(kPDEShading);
    sContentTypes.insert(kPDEText);
    sContentTypes.insert(kPDEUnknown);
    sContentTypes.insert(kPDEXObject);

    // Which pages we'll copy:   To copy all pages, omit populating this container with any numbers
    //    And don't forget, pages are 0-based in APDDFL!
    std::set<ASInt32> sPagesToCopy;
    sPagesToCopy.insert(0);
    sPagesToCopy.insert(1);
    sPagesToCopy.insert(3);
    sPagesToCopy.insert(5);

    DURING

        // Step 2) Open the input PDF, create the output PDF.

        APDFLDoc inAPDoc(csInputFileName.c_str(), true);
        PDDoc inDoc = inAPDoc.getPDDoc();

        int iInDocPages = PDDocGetNumPages(inDoc);

        // Create a blank document.
        APDFLDoc outAPDoc;
        PDDoc outDoc = outAPDoc.getPDDoc();

        // Step 3) Copy the specified content from the input PDF into the output PDF.

        for (int i = 0; i < iInDocPages; i++) {
            if (!sPagesToCopy.empty() && (sPagesToCopy.find(i) == sPagesToCopy.end())) {
                // Don't copy this page if the page selection set is not empty, and this page number isn't in it!
                continue;
            }

            // Give the output document a new page with the same size as page i of the input.
            PDPage inPage = inAPDoc.getPage(i);
            ASFixedRect inPageSize;
            PDPageGetSize(inPage, &(inPageSize.right), &(inPageSize.top));

            // Insert the new blank page and get a reference to it
            outAPDoc.insertPage(inPageSize.right, inPageSize.top, PDDocGetNumPages(outDoc) - 1);
            PDPage outPage = outAPDoc.getPage(PDDocGetNumPages(outDoc) - 1);

            // Obtain handles to the content of the input and output pages
            PDEContent inContent = PDPageAcquirePDEContent(inPage, 0);
            PDEContent outContent = PDPageAcquirePDEContent(outPage, 0);
            // And now copy the content.
            copyElements(outContent, inContent, sContentTypes);
            // Set the content into the page.
            PDPageSetPDEContentCanRaise(outPage, 0);

            // Release the resources of this iteration.
            PDPageReleasePDEContent(outPage, 0);
            PDPageRelease(outPage);
            PDPageReleasePDEContent(inPage, 0);
            PDPageRelease(inPage);
        }

        // Step 4) Save the output PDF and close both PDFs.

        outAPDoc.saveDoc(csOutputFileName.c_str());

        // APDFLDoc's destructor will properly close both documents,
        //    and APDFLib's will shut down the library

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode;
};

//  copyElements: Recursively copies selected PDEElement types from source to destination
//
// For aggregate element types (PDEContainer, PDEGroup, PDEForm) we will do so recursively;
//    "scalar" element types are copied directly
void copyElements(PDEContent to, PDEContent from, const std::set<ASInt32> &ElementTypesSet) {
    DURING

        ASInt32 i, iNumElems = PDEContentGetNumElems(from);

        for (i = 0; i < iNumElems; ++i) {
            // Fetch each element, and get its type
            PDEElement nextElem = PDEContentGetElem(from, i);
            ASInt32 type = PDEObjectGetType(reinterpret_cast<PDEObject>(nextElem));

            if (ElementTypesSet.find(type) == ElementTypesSet.end()) {
                continue;
            }

            switch (type) {
            case kPDEContainer: {
                // This is a compound container, need to recurse
                PDEContainer fromContainer = reinterpret_cast<PDEContainer>(nextElem);
                PDEContent fromContent = PDEContainerGetContent(fromContainer);

                // The new container, to which which we give blank contents, and give to the output document.
                PDEContainer toContainer = PDEContainerCreate(PDEContainerGetMCTag(fromContainer), NULL, true);
                PDEContent _content = PDEContentCreate();
                PDEContainerSetContent(toContainer, _content);
                PDEContent toContent = PDEContainerGetContent(toContainer);

                copyElements(toContent, fromContent, ElementTypesSet);

                // Now copy the new container into "to".
                PDEContentAddElem(to, kPDEAfterLast, reinterpret_cast<PDEElement>(toContainer));

                PDERelease(reinterpret_cast<PDEObject>(toContainer));
                PDERelease(reinterpret_cast<PDEObject>(_content));
                break;
            }

            case kPDEGroup: {
                // This is a compound container, need to recurse
                PDEGroup fromGroup = reinterpret_cast<PDEGroup>(nextElem);
                PDEContent fromContent = PDEGroupGetContent(fromGroup);

                PDEGroup toGroup = PDEGroupCreate();
                PDEGroupSetContent(toGroup, PDEContentCreate());
                PDEContent toContent = PDEGroupGetContent(toGroup);

                copyElements(toContent, fromContent, ElementTypesSet);

                // Now copy the new group  into "to".
                PDEContentAddElem(to, kPDEAfterLast, reinterpret_cast<PDEElement>(toGroup));

                PDERelease(reinterpret_cast<PDEObject>(toGroup));
                break;
            }

            case kPDEForm: {
                // This is a compound container, need to recurse
                PDEForm fromForm = reinterpret_cast<PDEForm>(nextElem);
                PDEContent fromFormContent = PDEFormGetContent(fromForm);

                // Note: this also clones the underlying xObject Cos Object(s), for each occurence.
                PDEForm toForm = PDEFormCreateClone(fromForm);

                // Replace the contents of toForm with a new one, with only the elements we want copied
                PDEContent toContent = PDEContentCreate();
                copyElements(toContent, fromFormContent, ElementTypesSet);
                PDEFormSetContent(toForm, toContent);

                // Now copy the new form  into "to".
                PDEContentAddElem(to, kPDEAfterLast, reinterpret_cast<PDEElement>(toForm));

                // Release the containers, since we're done with them.
                PDERelease(reinterpret_cast<PDEObject>(fromFormContent));
                PDERelease(reinterpret_cast<PDEObject>(toForm));
                PDERelease(reinterpret_cast<PDEObject>(toContent));
                break;
            }
            default: {
                // Must be a "scalar"  - just copy it
                PDEElement copyNextElem = PDEElementCopy(nextElem, kPDEElementCopyClipping);
                PDEContentAddElem(to, kPDEAfterLast, copyNextElem);
                PDERelease(reinterpret_cast<PDEObject>(copyNextElem));
            } break;
            }
        }

    HANDLER
        ASRaise(ERRORCODE); // If there was an exception, let the caller handle it.
    END_HANDLER

    return;
};
