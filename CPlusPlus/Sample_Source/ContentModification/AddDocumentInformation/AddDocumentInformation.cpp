//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The AddDocumentInformation sample inserts standard document metadata into a PDF document.
//
// The information is found in the "Document Information Dictionary" for a PDF. See
// "Document Information Dictionary" (section 14.3.3)in "ISO 32000-1:2008, Document
// Management-Portable Document Format-Part 1: PDF 1.7, page 549" at:
// http://www.adobe.com/content/dam/Adobe/en/devnet/acrobat/pdfs/PDF32000_2008.pdf#page=557.
//
// Command-line:  <input-file> <output-file>   (Both optional)
//
// For more detail see the description of the AddDocumentInformation sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#adddocumentinformation

#include <iostream>
#include "InitializeLibrary.h"
#include "ASExtraCalls.h"
#include "PDCalls.h"

#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddDocumentInformation.pdf"
#define DEF_OUTPUT "AddDocumentInformation-out.pdf"

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;
    APDFLib lib;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_DIR DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Adding some property information to file " << csInputFileName.c_str()
              << " and saving as " << csOutputFileName.c_str() << std::endl;

    DURING

        // Step 1) Open the Document that the document information will be inserted into.

        APDFLDoc apdflDoc(csInputFileName.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();

        // Step 2) Insert the document information. Inserting values for Title and Author are demonstrated here.

        // Create unicode strings for inserting the document's Title into the document.
        ASText key = ASTextFromUnicode(reinterpret_cast<const ASUTF16Val *>(L"Title"),
                                       APDFLDoc::GetHostUnicodeFormat());
        ASText value = ASTextFromUnicode(reinterpret_cast<const ASUTF16Val *>(L"Sample Title"),
                                         APDFLDoc::GetHostUnicodeFormat());

        PDDocSetInfoAsASText(pdDoc, key, value); // Insert the document information.
        // Free up resources.
        ASTextDestroy(key);
        ASTextDestroy(value);

        // Create unicode strings for inserting the document's Author into the document.
        key = ASTextFromUnicode(reinterpret_cast<const ASUTF16Val *>(L"Author"),
                                APDFLDoc::GetHostUnicodeFormat());
        value = ASTextFromUnicode(reinterpret_cast<const ASUTF16Val *>(L"Sample Author"),
                                  APDFLDoc::GetHostUnicodeFormat());

        PDDocSetInfoAsASText(pdDoc, key, value); // Insert the document information.

        // Free up resources.
        ASTextDestroy(key);
        ASTextDestroy(value);

        // Step 3) Save the Document and release Resources.

        apdflDoc.saveDoc(csOutputFileName.c_str());

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode;
}
