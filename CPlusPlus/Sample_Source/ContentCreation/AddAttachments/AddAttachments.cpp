//
// Copyright (c) 2015-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample program adds two attachments to an input PDF document.
//
// Command-line arguments:  <input-file> <output-file> <attachment-1> <attachment-2>   (All optional)
//
// For more detail see the description of the AddAttachments sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addattachments

#include <iostream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "CosCalls.h"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "noattachment.pdf"
#define DEF_ATT_1 "attachment1.xlsx"
#define DEF_ATT_2 "attachment2.docx"
#define DEF_OUTPUT "AddAttachments-out.pdf"

// Returns a newly opened ASFile, given the file's path.
ASFile openASFile(wchar_t *filepath);

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;

    // Initialize the Adobe PDF Library.  Termination will be automatic when "lib" goes out of scope
    APDFLib lib;

    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::string csAttachment1Name(argc > 3 ? argv[3] : INPUT_LOC DEF_ATT_1);
    std::string csAttachment2Name(argc > 4 ? argv[4] : INPUT_LOC DEF_ATT_2);

    DURING

        std::cout << "Opening the input file " << csInputFileName.c_str() << std::endl;

        APDFLDoc inputAPDoc(csInputFileName.c_str(), true); // Opens the input PDF document.
        PDDoc inputDoc = inputAPDoc.getPDDoc();             // The input PDF's PDDoc reference.
        CosDoc inputDocCD = PDDocGetCosDoc(inputDoc);       // The PDDoc's COS representation.

        // 1) Create the first PDFileAttachment.

        std::cout << "Opening attachment 1, " << csAttachment1Name.c_str() << std::endl;

        ASFile attach1ASFile = APDFLDoc::OpenFlatFile(csAttachment1Name.c_str());

        PDFileAttachment attach1PDFA =
            PDFileAttachmentNewFromFile(inputDocCD, // The PDF the file attachment will be referenced from.
                                        attach1ASFile, // The file we want to attach.
                                        NULL, 0,       // No filters for the file attachment stream.
                                        CosNewNull(),  // No filter parameters.
                                        NULL, NULL, NULL); // No progress monitoring.

        // Get the CosObj representation of the file attachment.
        CosObj attach1CO = PDFileAttachmentGetCosObj(attach1PDFA);

        ASFileClose(attach1ASFile); // We can release the ASFile at this point.

        // 2) Embed it to the document's name tree.

        // Retrieve the document's "EmbeddedFiles" name tree.
        //  (This name tree has not yet been used, so we must "create" it)
        PDNameTree filesTree = PDDocCreateNameTree(inputDoc, ASAtomFromString("EmbeddedFiles"));

        // The KEY to add to the inputDocCD CosDoc. Totally arbitrary.
        CosObj EmbedKey = CosNewString(inputDocCD, true, "TheSpreadsheet", 14);

        // Adding a key/value pair to the EmbeddedFiles name tree, where the value is
        // a CosObj of a PDFileAttachment, embeds the file in the PDDoc.
        PDNameTreePut(filesTree,  // The tree to add the Key/Value pair to.
                      EmbedKey,   // the KEY.
                      attach1CO); // The VALUE: the attachment's file specification

        // 3) Create the second PDFileAttachment.

        std::cout << "Opening attachment 2, " << csAttachment2Name.c_str() << std::endl;

        ASFile attach2ASFile = APDFLDoc::OpenFlatFile(csAttachment2Name.c_str());

        PDFileAttachment attach2PDFA = PDFileAttachmentNewFromFile(
            inputDocCD,    // The PDDoc CosDict that the file attachment will be referenced from.
            attach2ASFile, // The file we want to attach.
            NULL, 0,       // No filters for the file attachment stream.
            CosNewNull(),  // No filter parameters.
            NULL, NULL, NULL); // No progress monitoring.

        // Get the CosObject representation of the file attachment.
        CosObj attach2CO = PDFileAttachmentGetCosObj(attach2PDFA);

        ASFileClose(attach2ASFile); // We can release the ASFile at this point

        // 4) Create an annotation.

        // This rectangle determines the annotation's placement on the page.
        ASFixedRect annotLocation;
        annotLocation.left = ASFloatToFixed(2.50 * 72); // There are 72 pixels per inch.
        annotLocation.right = ASFloatToFixed(3.00 * 72);
        annotLocation.top = ASFloatToFixed(8.40 * 72);
        annotLocation.bottom = ASFloatToFixed(8.90 * 72);

        PDPage page1 = PDDocAcquirePage(inputDoc, 0); // The annotation will go on page 1.

        // This method just creates an annotation.
        PDAnnot newAnnot =
            PDPageCreateAnnot(page1, // The page the annotation will go on.
                              ASAtomFromString("FileAttachment"), // The type of annotation we're creating.
                              &annotLocation); // The annotation's location on the page.

        PDPageAddAnnot(page1, -2, newAnnot); // Add the annotation as the first annotation on the page.

        CosObj newAnnotCO = PDAnnotGetCosObj(newAnnot); // The annotation's CosObject representation.

        // 5) Embed the second PDFileAttachment to the annotation.

        // Like embedding to a PDDoc's name tree, adding a key/Value pair to
        // the annotation's CosDict, where the key is "FS" (the File Specification key)
        // and the value is the a PDFileAttachment cos object, will embed the file to
        // the annotation.
        CosDictPutKeyString(newAnnotCO, // The dictionary we want to add to.
                            "FS",       // The KEY: we're changing the File Specification.
                            attach2CO); // The VALUE: The file spec we want the annot to open

        // 6) Save and close.

        // PDPages must be released before closing the document.
        PDPageRelease(page1);
        inputAPDoc.saveDoc(csOutputFileName.c_str()); // inputAPDoc's destructor will close the document.

        std::cout << "Attached both attachements to input file, saved output as "
                  << csOutputFileName.c_str() << std::endl;

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode;
};
