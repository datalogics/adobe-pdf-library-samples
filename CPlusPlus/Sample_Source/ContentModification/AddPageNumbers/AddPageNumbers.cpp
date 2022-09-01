//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// AddPageNumbers adds labels to the page numbers in a PDF document. These labels appear under the
// page thumbnails.
//
//  Command-line:   <input-file>  <output-file>     (Both optional)
//
// For more detail see the description of the AddPageNumbers sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addpagenumbers

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "ASExtraCalls.h"
#include <iostream>

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "toNumber.pdf"
#define DEF_OUTPUT "AddPageNumbers-out.pdf"

int main(int argc, char **argv) {

    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Will add some page labels and numbers to " << csInputFileName.c_str()
              << " and save as " << csOutputFileName.c_str() << std::endl;

    DURING

        // Step 1) Open the Document that will have labels added to.

        APDFLDoc document(csInputFileName.c_str(), true);
        PDDoc pdDoc = document.getPDDoc();

        // Step 2) Create label's for different sets of pages

        // NOTE:  PDPageLabel() takes in a style key:
        //    "R" for upper-case Roman numbers,
        //    "r" for lower-case Roman numbers,
        //    "A" for upper-case alphabetic numbers,
        //    "a" for lower-case alphabetic numbers, or
        //    "D" for decimal Arabic numerals.

        // Set the first page's label to "Cover" with the number counter to 1
        PDPageLabel coverLabel = PDPageLabelNew(pdDoc, ASAtomFromString("D"), "Cover ", 6, 1);
        PDDocSetPageLabel(pdDoc, 0, coverLabel);

        // Set the label of second page and up to "preface" with the number counter starting at 2
        PDPageLabel prefaceLabel = PDPageLabelNew(pdDoc, ASAtomFromString("r"), "preface ", 8, 2);
        PDDocSetPageLabel(pdDoc, 1, prefaceLabel);

        // Starting from the 5th page onwards, display pages numbers starting with 1 and upwards
        PDPageLabel pageLabel = PDPageLabelNew(pdDoc, ASAtomFromString("D"), "", 0, 1);
        PDDocSetPageLabel(pdDoc, 5, pageLabel);

        // Step 3) Save and exit

        document.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveLinearized);

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode;
}
