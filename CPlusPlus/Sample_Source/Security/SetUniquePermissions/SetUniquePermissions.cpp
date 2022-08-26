//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The SetUniquePermissions sample shows how to assign security permissions to a PDF document.
//
// Command-line:  <input-file>   <output-file>    (Both optional)
//
// For more detail see the description of the SetUniquePermissions sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#setuniquepermissions

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "SetUniquePermissions.pdf"
#define DEF_OUTPUT "SetUniquePermissions-out.pdf"

// To reduce verbosity.
#define PERM_PAIR std::make_pair<bool, PDPerms>

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
    std::cout << "Will apply custom security settings to " << csInputFileName.c_str()
              << " and save as " << csOutputFileName.c_str() << std::endl;

    const char *password = "Datalogics";

    // Step 1) Select which permissions you want to allow/deny.

    std::vector<std::pair<bool, PDPerms> > permList(15); // There are 15 permissions

    // Most Permissions

    // All permissions, except page extraction.
    permList.push_back(PERM_PAIR(false, pdPermAll));
    // All permissions, except page extraction.
    permList.push_back(PERM_PAIR(false, pdPermUser));

    // User Editing Permissions

    // Sets these four permissions true: pdPermEdit, pdPermEditNotes, pdPermPrint, pdPermCopy.
    permList.push_back(PERM_PAIR(false, pdPermSettable));
    // Enables changing the document, document assembly, form field filling, signing, and template page spawning.
    permList.push_back(PERM_PAIR(false, pdPermEdit));
    // Enables commenting, form field filling, and signing.
    permList.push_back(PERM_PAIR(false, pdPermEditNotes));
    // Enables page insertion/deletion/rotation, as well as bookmark creation.
    permList.push_back(PERM_PAIR(false, pdPrivPermDocAssembly));
    // Enables form field filling, signing, and spawning template pages.
    permList.push_back(PERM_PAIR(false, pdPrivPermFillandSign));
    // Whether the form can be submitted outside of a browser. Be sure you enable form field filling and signing.
    permList.push_back(PERM_PAIR(false, pdPrivPermFormSubmit));

    // Copying

    // Enables content copying (i.e., to the clipboard) and content copying for accessibility.
    permList.push_back(PERM_PAIR(false, pdPermCopy));
    // Enables copying of content related to Acrobat's accessibility features for people with disabilities.
    permList.push_back(PERM_PAIR(false, pdPrivPermAccessible));

    // Printing

    // Enables printing.
    permList.push_back(PERM_PAIR(false, pdPermPrint));
    // If pdPermPrint is true and this is false, only low quality printing (Print As Image) is allowed.
    //   On UNIX platforms where Print As Image doesn't exist, printing will be disabled.
    permList.push_back(PERM_PAIR(false, pdPrivPermHighPrint));

    // Open/Save

    // The user can open and decrypt the document. This will have no effect if a user password is not set.
    permList.push_back(PERM_PAIR(false, pdPermOpen));
    // Enables Save As..., with the followng caveats:
    //      If both pdPermEdit and pdPermEditNotes are disallowed, "Save" will be disabled but
    //      "Save As" will be enabled. Note that the "Save As" menu item is not necessarily
    //      disabled even if this is set to false!
    permList.push_back(PERM_PAIR(false, pdPermSaveAs));

    // Security

    // The user can change the document's security settings.
    //   This will have no effect unless an owner password is set.
    permList.push_back(PERM_PAIR(false, pdPermSecure));

    DURING

        APDFLDoc APDoc(csInputFileName.c_str(), true);
        PDDoc document = APDoc.getPDDoc();

        // Step 2) Create new security data with the specified permissions.

        // Prepare and create the structure that will hold the new security data.
        PDDocSetNewCryptHandler(document, ASAtomFromString("Standard"));
        StdSecurityData securityData = (StdSecurityData)PDDocNewSecurityData(document);
        securityData->size = sizeof(StdSecurityDataRec);

        // See the sample Encryption for a demonstration of user passwords.
        securityData->hasUserPW = false;

        // The permissions of the document will not be able to be changed
        //    back unless the user supplies this password.
        securityData->hasOwnerPW = true;
        securityData->newOwnerPW = true;
        strcpy(securityData->ownerPW, password);

        // Set the encryption method:
        // 2 = CF_METHOD_RC4_V2 - RC4 algorithm.
        // 5 = CF_METHOD_AES_V1 - AES algorithm with a zero initialization vector.
        // 6 = CF_METHOD_AES_V2 - AES algorithm with a 16 byte random initialization vector.
        // 7 = CF_METHOD_AES_V3 - AES algorithm with a 4 byte random initialization vector.
        securityData->encryptMethod = 2;
        securityData->keyLength = 16; // encryption key length in bytes.
        securityData->encryptMetadata = true;
        securityData->encryptAttachmentsOnly = false;

        // Set the supplied user permissions flags.
        securityData->perms = 0x00000000; // First, default to no permissions...
        for (int i = 0; i < 15; i++) // Then bitwise OR in all the permissions we paired with true.
        {
            std::pair<bool, PDPerms> x = permList[i];
            if (x.first)
                securityData->perms |= x.second;
        }

        // Step 3) Set the new security data into the document.

        PDDocSetNewSecurityData(document, (void *)securityData);
        ASfree((void *)securityData);

        // Step 4) Save and close the document.

        // Specifying PDSaveFull, even though APDFLDoc would do that by default
        APDoc.saveDoc(csOutputFileName.c_str(), PDSaveFull);

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
};
