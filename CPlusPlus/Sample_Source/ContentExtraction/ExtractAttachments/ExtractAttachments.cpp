//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample extracts from a PDF input document all of the embedded attachments
// found on the first page, and all attachments in the nametree.
//
// To run the program from the command line, enter the input file name and prefix. Both are optional.
// To save the attachments with no prefix, type "" (two quotation marks) for the second command-line argument.
//
// For more detail see the description of the ExtractAttachments sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#extractattachments

#include "PERCalls.h"
#include "CosCalls.h"
#include "ASExtraCalls.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include <iostream>

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "extractFrom.pdf"
#define DEF_PREFIX "_x_"

ACCB1 ASBool ACCB2 extractor(CosObj obj, CosObj value, void *clientData);

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csPrefix(argc > 2 ? argv[2] : DEF_PREFIX);
    std::cout << "Extracting attachments (annotations on the first page) and all nametree-embedded "
              << "files from " << csInputFileName.c_str() << " and "
              << "saving with prefix of \"" << csPrefix.c_str() << "\"" << std::endl;

    DURING

        APDFLDoc document(csInputFileName.c_str(), true);

        // Step 1) Iterate through the annotations on a page 0 and extract and save their embedded files

        ASAtom atFileAttachment(ASAtomFromString("FileAttachment"));
        PDPage pdPage = document.getPage(0);

        int annotTotal = PDPageGetNumAnnots(pdPage);

        for (int i = 0; i < annotTotal; i++) {
            PDAnnot annot = PDPageGetAnnot(pdPage, i);

            // Operate only on "FileAttachment" annotations
            if (atFileAttachment == PDAnnotGetSubtype(annot)) {
                CosObj obj = PDAnnotGetCosObj(annot);

                // Get the cos dictionary object from the CosObj, using the File Specification key
                CosObj dictObj = CosDictGet(obj, ASAtomFromString("FS"));

                // Obtain the accessed attachemnt using the dictionary
                PDFileAttachment fileAttachment = PDFileAttachmentFromCosObj(dictObj);

                // Grab the file's name using the cos object dictionary
                ASTCount len = 0;
                std::string sFileName(CosStringValue(CosDictGet(dictObj, ASAtomFromString("F")), &len));

                std::cout << "\tAnnotation attachment " << sFileName.c_str() << " --> ";
                sFileName.insert(0, csPrefix);
                std::cout << sFileName.c_str() << std::endl;

                ASFile outFile = APDFLDoc::OpenFlatFile(sFileName.c_str(), ASFILE_CREATE);

                PDFileAttachmentSaveToFile(fileAttachment, outFile);

                // Safely close outFile
                ASFileFlush(outFile);
                ASFileClose(outFile);
            }
        }

        PDPageRelease(pdPage);

        // Step 2) Iterate through the file's nametree and extract and save its embedded files

        // Create the nametree
        PDNameTree nameTree = PDDocCreateNameTree(document.getPDDoc(), ASAtomFromString("EmbeddedFi"
                                                                                        "les"));

        // Apply the enum function to the nametree so it can iterate through, extracting the attachments.
        PDNameTreeEnum(nameTree, &extractor, &csPrefix);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

// Procedure called by PDNameTreeEnum
ACCB1 ASBool ACCB2 extractor(CosObj obj, CosObj value, void *clientData) {
    std::string *psPrefix = (std::string *)clientData;

    PDFileAttachment fileAttachment = PDFileAttachmentFromCosObj(value);

    // Grab the file's name using the cos object dictionary and the File Specifcation String key.
    ASTCount len = 0;
    std::string sFileName(CosStringValue(CosDictGet(value, ASAtomFromString("F")), &len));

    std::cout << "\tName-tree attachment " << sFileName.c_str() << " --> ";
    sFileName.insert(0, *psPrefix);
    std::cout << sFileName.c_str() << std::endl;

    ASFile outFile = APDFLDoc::OpenFlatFile(sFileName.c_str(), ASFILE_CREATE);

    PDFileAttachmentSaveToFile(fileAttachment, outFile);
    ASFileFlush(outFile);
    ASFileClose(outFile);

    return true;
}
