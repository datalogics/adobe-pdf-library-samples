//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample removes security from a password-protected document. It is effectively
// the opposite of the EncryptDocument sample program.
//
// Command-line:  <input-file>   <output-file>    (Both optional)
//
// For more detail see the description of the OpenEncrypted sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#openencrypted

#include <iostream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "ASExtraCalls.h"
#include "PDFInit.h"
#include "PDFLCalls.h"
#include "PDExpT.h"
#include "PDCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "OpenEncrypted.pdf"
#define DEF_OUTPUT "OpenEncrypted-out.pdf"

#define PASSWORD "mypassword"

// Callback function called by PDDocOpenEx to obtain permission to open the document by supplying the password.
static ACCB1 ASBool ACCB2 openAuthorizationProcedure(PDDoc encrypted, void *password);

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
    std::cout << "Will remove all security from " << csInputFileName.c_str() << " and save as "
              << csOutputFileName.c_str() << std::endl;

    DURING

        // Step 1) Open the document with the password.

        ASPathName inputPathName = APDFLDoc::makePath(csInputFileName.c_str());

        // PDDocOpenEx openAuthorizationProcedure to supply the password.
        PDDoc document =
            PDDocOpenEx(inputPathName, ASGetDefaultFileSys(), &openAuthorizationProcedure, 0, true);

        // This can be released at this point
        ASFileSysReleasePath(ASGetDefaultFileSys(), inputPathName);

        // Step 2) Remove the encryption.

        // Setting it to ASAtomNull completely removes security from the document.
        PDDocSetNewCryptHandler(document, ASAtomNull);

        // Step 3) Save and close the document.

        ASPathName pathOutput = APDFLDoc::makePath(csOutputFileName.c_str());
        PDDocSave(document, PDDocNeedsSave | PDDocIsOpen, pathOutput, ASGetDefaultFileSys(), NULL, NULL);

        // Release resources.
        PDDocClose(document);
        ASFileSysReleasePath(ASGetDefaultFileSys(), pathOutput);

        // Step 4) Open it without a password to ensure the encryption is gone.

        ASErrorCode openError = 0;

        DURING

            // Since this class' default is to open without a password (or authorization proc) this will
            //     throw instantly if there is a password on the file!
            APDFLDoc doc(csOutputFileName.c_str(), true);

        HANDLER
            openError = ERRORCODE;
        END_HANDLER

        if (openError) {
            if (ErrGetSystem(openError) == ErrSysPDDoc && ErrGetCode(openError) == pdErrNeedPassword) {
                std::cout << "Error:  The document still requires a password ]." << std::endl;
            } else {
                std::cout << "An unexpected error occured." << std::endl;
            }
            return openError;
        }

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
};

// Callback for PDDocOpenEx to obtain permission to open the document by supplying the password.
static ACCB1 ASBool ACCB2 openAuthorizationProcedure(PDDoc encrypted, void *clientData) {

    PDPermReqStatus permReqStatus;

    DURING

        // Request open permission by supplying the password.
        permReqStatus = PDDocPermRequest(encrypted,       // The document we want to open.
                                         PDPermReqObjDoc, // Object of the request: a document.
                                         PDPermReqOprOpen, // Target operation of the request: to open it.
                                         (void *)PASSWORD); // And, of course, the password.

    HANDLER
        ASRaise(ERRORCODE);
    END_HANDLER

    std::cout << "openAuthorizationProcedure: ";
    switch (permReqStatus) {
    case PDPermReqGranted:
        std::cout << "Password verified." << std::endl;
        break;
    case PDPermReqDenied:
        std::cout << "Invalid password." << std::endl;
        break;
    case PDPermReqUnknownObject:
        std::cout << "Target object unknown for the permisson request." << std::endl;
        break;
    case PDPermReqUnknownOperation:
        std::cout << "Target operation unknown for the permission request." << std::endl;
        break;
    default:
        std::cout << "An unexpected error occured while trying to open the document." << std::endl;
        break;
    }

    return (permReqStatus == PDPermReqGranted);
};
