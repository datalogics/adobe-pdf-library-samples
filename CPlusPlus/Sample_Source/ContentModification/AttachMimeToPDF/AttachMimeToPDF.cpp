//
// Copyright (c) 2015-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample program demonstrates how to add a file attachment to a PDF document, and
// then save the updated file. If you run this program and then open the PDF output document
// in Adobe Acrobat, the newly attached file appears in the Attachments pane on the left side
// of the window.
//
// MIME refers to the Multipurpose Internet Mail Extensions (MIME) standard.  It was developed
// to define the types of files that can be attached to an electronic mail message using the SMTP
// format, but it also applies to other protocols like HTTP, and the MIME standard is used to
// define the types of files that can be attached to a PDF document.
//
// For more detail see the description of the AttachMimeToPDF sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#attachmimetopdf

#include <cstdio>

#include "CosCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define DEFAULT_INPUT "AttachMimeToPDF-in.pdf"
#define DEFAULT_ATTACH "attachment.txt"
#define DEFAULT_OUTPUT "AttachMimeToPDF-out.pdf"

int main(int argc, char *argv[]) {
    // Initialize the PDF library
    APDFLib libInit;
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputPdf(argc > 1 ? argv[1] : INPUT_DIR DEFAULT_INPUT);
    std::string csAttachment(argc > 2 ? argv[2] : INPUT_DIR DEFAULT_ATTACH);
    std::string csOutputPdf(argc > 3 ? argv[3] : DEFAULT_OUTPUT);
    std::cout << "Will attach file " << csAttachment.c_str() << " to " << csInputPdf.c_str()
              << " and save to " << csOutputPdf.c_str() << std::endl;

    DURING

        // Open the input document
        APDFLDoc apdflDoc(csInputPdf.c_str(), true);
        PDDoc PDFDoc = apdflDoc.getPDDoc();

        // Open attachment file
        ASFile AttachFile;
        ASPathName AttachPath = APDFLDoc::makePath(csAttachment.c_str());
        if (ASFileSysOpenFile(NULL, AttachPath, ASFILE_READ, &AttachFile) != 0) {
            std::cout << "Unable to open Attachment file \"" << csAttachment.c_str() << "\"" << std::endl;
            return -4;
        }
        ASFileSysReleasePath(NULL, AttachPath);

        // Make a convenient pointer to the cos doc
        CosDoc cosPDFDoc = PDDocGetCosDoc(PDFDoc);

        // Setup and testing complete. PDF document in place, with a file to embed in that PDF
        // and a MIME type to apply to it.
        PDNameTree nameTree = PDDocCreateNameTree(PDFDoc, ASAtomFromString("EmbeddedFiles"));
        CosObj EmbeddedNames;
        CosObj cosNameTree = PDNameTreeGetCosObj(nameTree);

        ASAtom filters[1] = {ASAtomFromString("FlateDecode")};
        PDFileAttachment attachment =
            PDFileAttachmentNewFromFile(cosPDFDoc, AttachFile, filters, (ASArraySize)1,
                                        CosNewDict(cosPDFDoc, true, 0), NULL, NULL, NULL);

        EmbeddedNames = CosDictGet(cosNameTree, ASAtomFromString("Names"));
        char WorkString[256];
        if (CosObjGetType(EmbeddedNames) == CosNull) {
            sprintf(WorkString, "Untitled Object");
        } else {
            sprintf(WorkString, "Untitled Object %01d", CosArrayLength(EmbeddedNames));
        }

        int nameLen;
        ASText wrkTxt = ASTextFromEncoded(WorkString, 0);
        char *pdWorkName = ASTextGetPDTextCopy(wrkTxt, &nameLen);
        CosObj cosattachName = CosNewString(cosPDFDoc, false, pdWorkName, nameLen);
        ASTextDestroy(wrkTxt);
        ASfree(pdWorkName);

        PDNameTreePut(nameTree, cosattachName, PDFileAttachmentGetCosObj(attachment));

        ASFileClose(AttachFile);

        // Write out file
        apdflDoc.saveDoc(csOutputPdf.c_str(), PDSaveFull | PDSaveCollectGarbage | PDSaveBinaryOK | PDSaveCopy);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}
