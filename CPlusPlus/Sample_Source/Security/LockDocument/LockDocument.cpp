//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The LockDocument sample program makes a PDF input document read-only.
//
// Command-line:    <input-file>  <output-file>     (Both optional)
//
// For more detail see the description of the LockDocument sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#lockdocument

#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "LockDocument.pdf"
#define DEF_OUTPUT "LockDocument-out.pdf"

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Will apply new security data to " << csInputFileName.c_str() << " and save as "
              << csOutputFileName.c_str() << std::endl;

    // Password to change permissions we will add. Note: cannot be a wide character string.
    const char *password = "Datalogics";

    DURING

        // Step 1) Open the document, and create new security data for it which will disallow all editing permissions.

        APDFLDoc APDoc(csInputFileName.c_str(), true);
        PDDoc document = APDoc.getPDDoc();

        // This structure will hold the new security data.
        PDDocSetNewCryptHandler(document, ASAtomFromString("Standard"));

        // Prepare new security data...
        StdSecurityData securityData = (StdSecurityData)PDDocNewSecurityData(document);
        securityData->size = sizeof(StdSecurityDataRec);

        // Do not require a password to open the document.
        securityData->hasUserPW = false;

        // Specify "owner password", which will be required to change the new permissions of the document
        securityData->hasOwnerPW = true;
        securityData->newOwnerPW = true;
        strcpy(securityData->ownerPW, password);

        // Set the encryption method:
        // 2 = CF_METHOD_RC4_V2 - RC4 algorithm.
        // 5 = CF_METHOD_AES_V1 - AES algorithm with a zero initialization vector.
        // 6 = CF_METHOD_AES_V2 - AES algorithm with a 16 byte random initialization vector.
        // 7 = CF_METHOD_AES_V3 - AES algorithm with a 4 byte random initialization vector.
        securityData->encryptMethod = 2;
        // Encryption key length, in bytes.
        securityData->keyLength = 16;
        securityData->encryptMetadata = true;
        securityData->encryptAttachmentsOnly = false;

        // No permissions at all will be enabled.
        securityData->perms = 0x00000000;

        // Step 2) Set the new security data into the document.

        PDDocSetNewSecurityData(document, (void *)securityData);
        ASfree((void *)securityData);

        // Step 3) Save and close the document.

        // Changing the security of a document requires a full save. APDFLDoc
        APDoc.saveDoc(csOutputFileName.c_str(), PDSaveFull);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
};
