//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The EncryptDocument encrypts a PDF document to secure it, and saves it with a password.
//
// Command-line:    <input-file>  <output-file>  <password>   (Optional)
//
// For more detail see the description of the EncryptDocument sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#encryptdocument

#include <iostream>
#include "ASExtraCalls.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "toBeEncrypted.pdf"
#define DEF_OUTPUT "EncryptDocument-out.pdf"
#define DEF_PW "myPass"

static bool IsDocEncrypted(PDDoc d);

int main(int argc, char *argv[]) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::string csPassword(argc > 3 ? argv[3] : DEF_PW);
    std::cout << "Will apply password \"" << csPassword.c_str() << "\" to " << csInputFileName.c_str()
              << " and resave as " << csOutputFileName.c_str() << std::endl;

    DURING

        // Step 0) Open input document, repair if damaged, and verify un-encrypted

        APDFLDoc document(csInputFileName.c_str(), true);

        // Verify it is not encrypted at this time
        // Actually, this is not needed, as based on the technique just used to open the document (not
        // calling PDDocOpen with a PDAuthProc provided) the above constructor would have raised were
        // the document already password encrypted.
        // It is included here for contrast against its result at the end of the program.
        if (IsDocEncrypted(document.pdDoc)) {
            std::cout << "Error! Input document seems to already be encrypted!" << std::endl;
            return 2;
        }

        // Step 1) Create new security data, set with a user password for encryption using the RC4 algorithm.

        // Note: Encrytpion can be set from among 4 different types.
        //       2 = CF_METHOD_RC4_V2 - RC4 algorithm.
        //       5 = CF_METHOD_AES_V1 - AES algorithm with a zero initialization vector.
        //       6 = CF_METHOD_AES_V2 - AES algorithm with a 16 byte random initialization vector.
        //       7 = CF_METHOD_AES_V3 - AES algorithm with a 4 byte random initialization vector.

        // Set new security handler.
        PDDocSetNewCryptHandler(document.pdDoc, ASAtomFromString("Standard"));

        // Set up the security data record
        StdSecurityData securityData = (StdSecurityData)PDDocNewSecurityData(document.pdDoc);
        securityData->size = sizeof(StdSecurityDataRec);
        securityData->hasUserPW = true;
        securityData->newUserPW = true;
        strcpy(securityData->userPW, csPassword.c_str());
        securityData->hasOwnerPW = false;
        securityData->newOwnerPW = false;
        strcpy(securityData->ownerPW, "");
        securityData->perms = pdPermUser;
        securityData->keyLength = 16;
        securityData->encryptMethod = 2;

        // Step 2) Set the encrptyion method to the document, save and exit

        // Set the security data to the document.
        PDDocSetNewSecurityData(document.pdDoc, securityData);

        // Changing the document security requires a full save.
        PDDocSetFlags(document.pdDoc, PDDocRequiresFullSave);

        ASfree(securityData);

        // Verify that it is encrypted
        document.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveLinearized);
        if (!IsDocEncrypted(document.pdDoc)) {
            std::cout << "Error! Input document would seem to NOT be encrypted!" << std::endl;
            return 2;
        }

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

/* static */ bool IsDocEncrypted(PDDoc d) { return !(ASAtomNull == PDDocGetNewCryptHandler(d)); }
