//
// Copyright(c) 2010 - 2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information see:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates how to add an encryption key and a password to a
// PDF document.
//

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include <cstdio>

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define DEF_INPUT_FILE "AESEncryption-in.pdf"
#define DEF_OUTPUT_FILE "AESEncryption-out.pdf"

int main(int argc, char *argv[]) {
    APDFLib libInit; // Initialize the library
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR DEF_INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : DEF_OUTPUT_FILE);
    std::cout << "Processing input file " << csInputFile.c_str() << std::endl;
    std::cout << "Will write to output file " << csOutputFile.c_str() << std::endl;

    DURING

        // Open the input document
        APDFLDoc apdflDoc(csInputFile.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();

        // Create the security data structure
        StdSecurityData securityData = NULL;

        PDDocSetNewCryptHandler(pdDoc, ASAtomFromString("Standard"));

        securityData = (StdSecurityData)PDDocNewSecurityData(pdDoc);
        securityData->size = sizeof(StdSecurityDataRec);
        securityData->newUserPW = true;
        securityData->hasUserPW = true;
        strcpy(securityData->userPW, "secure");
        securityData->perms = pdPrivPermFillandSign | pdPrivPermAccessible;
        securityData->keyLength = STDSEC_KEYLENGTH_AES256;
        securityData->encryptMethod = STDSEC_METHOD_AES_V3;
        securityData->revision = STDSEC_CryptRevision6;
        securityData->version = STDSEC_CryptVersionV5;

        // Apply security settings
        PDDocSetNewSecurityData(pdDoc, (void *)securityData);
        PDDocSetFlags(pdDoc, PDDocRequiresFullSave);

        // Save file
        apdflDoc.saveDoc(csOutputFile.c_str(), PDSaveFull | PDSaveLinearized);

        ASfree(securityData);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}
