//
// Copyright (c) 2017-2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// MergeDocuments opens two PDF input documents, merges them and saves the
// resulting document in the working directory.
//
// Command-line:  <input-file-1> <input-file-2> <output-file>     (All optional)
//
// For more detail see the description of the MergeDocuments sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#mergedocuments

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT_1 "merge1.pdf"
#define DEF_INPUT_2 "merge2.pdf"
#define DEF_OUTPUT "MergeDocuments-out.pdf"

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName1(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT_1);
    std::string csInputFileName2(argc > 2 ? argv[2] : DIR_LOC DEF_INPUT_2);
    std::string csOutputFileName(argc > 3 ? argv[3] : DEF_OUTPUT);
    std::cout << "Merging " << csInputFileName1.c_str() << " and " << csInputFileName2.c_str()
              << " and saving to " << csOutputFileName.c_str() << std::endl;

    DURING

        APDFLDoc doc1(csInputFileName1.c_str(), true);
        APDFLDoc doc2(csInputFileName2.c_str(), true);

        // Insert doc2's pages into doc1.
        // PDLastPage adds the pages after the last page of the Target Document.
        PDDocInsertPages(doc1.getPDDoc(), PDLastPage, doc2.getPDDoc(), 0, PDAllPages, PDInsertBookmarks | PDInsertThreads |
                         // For best performance processing large documents, set the following flags.
                         PDInsertDoNotMergeFonts | PDInsertDoNotResolveInvalidStructureParentReferences | PDInsertDoNotRemovePageInheritance,
                         NULL, NULL, NULL, NULL);

        // For best performance processing large documents, set the following flags.
        doc1.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveLinearizedNoOptimizeFonts, PDSaveCompressed);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
