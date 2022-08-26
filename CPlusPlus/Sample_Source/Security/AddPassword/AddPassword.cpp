//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample adds a password to open an otherwise unsecured PDF document.
//
// Command-line:  <input-file> <password> <output-file>    (All optional)
//
// For more detail see the description of the AddPassword sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addpassword

#include <iostream>
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddPassword.pdf"
#define DEF_OUTPUT "AddPassword-out.pdf"
#define DEF_PASSWORD "Datalogics"

int main(int argc, char **argv) {
    APDFLib lib;

    ASErrorCode errCode = 0;
    if (lib.isValid() == false) {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT);
    std::string csPassword(argc > 2 ? argv[2] : DEF_PASSWORD);
    std::string csOutputFileName(argc > 3 ? argv[3] : DEF_OUTPUT);
    std::cout << "Will apply password \"" << csPassword.c_str() << "\" to "
              << csInputFileName.c_str() << " and save as " << csOutputFileName.c_str() << std::endl;

    DURING

        // Step 1) Open the input document and create a password for it.

        APDFLDoc APDoc(csInputFileName.c_str(), true);
        PDDoc document = APDoc.getPDDoc();

        // This structure will hold the new security data.
        PDDocSetNewCryptHandler(document, ASAtomFromString("Standard"));
        StdSecurityData securityData = (StdSecurityData)PDDocNewSecurityData(document);
        securityData->size = sizeof(StdSecurityDataRec);

        // Set the password needed to open the output file.
        securityData->hasUserPW = true;
        securityData->newUserPW = true;
        strcpy(securityData->userPW, csPassword.c_str());

        // Don't add a password to change restrictions once the file is open.
        //    (See SetUniquePermissions for this behavior.)
        securityData->hasOwnerPW = false;

        // Set the encryption method:
        // 2 = CF_METHOD_RC4_V2 - RC4 algorithm.
        // 5 = CF_METHOD_AES_V1 - AES algorithm with a zero initialization vector.
        // 6 = CF_METHOD_AES_V2 - AES algorithm with a 16 byte random initialization vector.
        // 7 = CF_METHOD_AES_V3 - AES algorithm with a 4 byte random initialization vector.
        securityData->encryptMethod = 2;
        securityData->keyLength = 16;
        // Either of the following two booleans can be set to true, but not both. It's up to you.
        securityData->encryptMetadata = true;
        securityData->encryptAttachmentsOnly = false;

        // The user will be able to perform all operations when the file is opened with the password.
        securityData->perms = pdPermOwner;

        // Step 2) Set the new security data into the document.

        PDDocSetNewSecurityData(document, (void *)securityData);
        ASfree((void *)securityData);

        // Step 3) Save and close the document.

        // Changing security permissions requires a full save. APDFLDoc saves with
        //    PDSaveFull by default, but we are making this explicit!
        APDoc.saveDoc(csOutputFileName.c_str(), PDSaveFull);

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode);
    END_HANDLER

    return errCode;
};
