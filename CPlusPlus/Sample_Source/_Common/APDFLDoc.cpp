// Copyright (c) 2015-2016, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
//===============================================================================
// Sample: APDFLDoc -This class is intended to assist with operations common to
// most samples. The class is capable of opening/creating and saving a document.
// It can also insert and retrieve pages.
//
// APDFLDoc.cpp: Contains the method implementations.
// APDFLDoc.h: Contains the class definition.
//===============================================================================

#include "APDFLDoc.h"
#include <cstring>
#include <cstdlib>
#include <iostream>

//==============================================================================================================================
// Default Constructor - This creates a new PDDoc object. This object will be automatically freed in the APDFLDoc's destructor.
//==============================================================================================================================

APDFLDoc::APDFLDoc() {

    initialize(); // Helper method sets some of the data members to NULL values.

    DURING

        pdDoc = PDDocCreate(); // Initialize the PDDoc data member.

    HANDLER

        printErrorHandlerMessage();

        RERAISE(); // Pass exception to the next HANDLER on the stack.

    END_HANDLER
}

//==============================================================================================================================
// Constructor - This constructor opens an existing PDF document. nameOfDocument is the relative path for the PDF document
// and the bool repairDamagedFile determines whether to repair (true) or not (false) a damaged file.
//==============================================================================================================================

APDFLDoc::APDFLDoc(wchar_t *docName, bool repairDamagedFile) {
    CommonConstruct(docName, repairDamagedFile);
}

//==============================================================================================================================
// Common constructor stuff.  Introduced to allow char* overload for constructor with minimal code rewriting
//==============================================================================================================================
void APDFLDoc::CommonConstruct(wchar_t *docName, bool repairDamagedFile) {
    initialize();
    if (NULL == docName) {
        return;
    }

    DURING

        setASPathName(docName); // Set the ASPathName data member.

        pdDoc = PDDocOpen(asPathName, NULL, NULL, repairDamagedFile); // Open the PDF document.

        ASFileSysReleasePath(NULL, asPathName); // Release the ASPathName that was just created.
        asPathName = NULL;

    HANDLER

        printErrorHandlerMessage(); // Report any exceptions that occured.

        RERAISE(); // Pass the exception to the next handler on the stack.

    END_HANDLER
}

//==============================================================================================================================
// Constructor - This overload accepts an ordinary c-string for the file name
//==============================================================================================================================

APDFLDoc::APDFLDoc(const char *docName, bool repairDamagedFile) {
    if (docName) {
        const size_t cSize = strlen(docName) + 1;
        wchar_t *wc = new wchar_t[cSize];
        if (wc) {
            mbstowcs(wc, docName, cSize);
        }
        CommonConstruct(wc, repairDamagedFile);
        if (wc) {
            delete[] wc;
        }
    }
}

//==============================================================================================================================
// initialize() - Helper method used to initialize data members to NULL values.
//==============================================================================================================================

void APDFLDoc::initialize() {
    pdDoc = NULL;
    asPathName = NULL;
    errorCode = 0;
    nameOfDocument[0] = L'\0';
}

//==============================================================================================================================
// saveDoc() - This overload accepts a non-wide path name, then passes through to the real function
//==============================================================================================================================

ASErrorCode APDFLDoc::saveDoc(const char *pathToSaveDoc, PDSaveFlags saveFlags, PDSaveFlags2 saveFlags2) {
    wchar_t *wc = NULL;
    if (pathToSaveDoc) {
        const size_t cSize = strlen(pathToSaveDoc) + 1;
        wc = new wchar_t[cSize];
        if (wc) {
            mbstowcs(wc, pathToSaveDoc, cSize);
        }
    }
    ASErrorCode rc = saveDoc(wc, saveFlags, saveFlags2);
    if (wc) {
        delete[] wc;
    }
    return rc;
}

//==============================================================================================================================
// saveDoc() - This method saves the PDDoc. If pathToSaveDoc is supplied it will save to the
// location specified. If it is not supplied it will overwrite the documents original location. The
// document will do a complete save by default, but other flags may be specified.
//==============================================================================================================================

ASErrorCode APDFLDoc::saveDoc(wchar_t *pathToSaveDoc, PDSaveFlags saveFlags, PDSaveFlags2 saveFlags2) {

    DURING

        // Error checking: Ensure a name has been set before saving the document.
        if (pathToSaveDoc == NULL && nameOfDocument[0] == L'\0') {
            std::wcerr << L"Failed to save document ensure PDDoc has a valid name before saving. " << std::endl;
            errorCode = -1;
            return errorCode;
        }

        // Error checking: Ensure that the ASPathName has been set before saving the document.
        if (pathToSaveDoc != NULL)
            setASPathName(pathToSaveDoc); // Use the path specified in saveDoc if it's been set.
        else
            setASPathName(nameOfDocument); // Overwrite the original document if it hasn't been set.

        PDDocSaveParamsRec saveParamsRec;
        memset(&saveParamsRec, 0, sizeof(PDDocSaveParamsRec));

        saveParamsRec.size = sizeof(PDDocSaveParamsRec);
        saveParamsRec.newPath = asPathName;
        saveParamsRec.saveFlags = saveFlags;
        saveParamsRec.saveFlags2 = saveFlags2;

        // Error Checking: Ensure document has a page before saving.
        if (PDDocGetNumPages(pdDoc) > 0)
            PDDocSaveWithParams(pdDoc, &saveParamsRec);
        else {
            std::wcerr << L"Failed to save document ensure PDDoc has pages. " << std::endl;
            errorCode = -2;
        }

        ASFileSysReleasePath(NULL, asPathName); // Release ASPathName object and set to NULL.
        asPathName = NULL;

    HANDLER

        return printErrorHandlerMessage(); // Return the error code that was generated by the exception.

    END_HANDLER

    return errorCode;
}

//==============================================================================================================================
// setASPAthName() - Helper method used to create an ASPathName. This is called by the saveDoc and open document constructor.
//==============================================================================================================================

ASErrorCode APDFLDoc::setASPathName(wchar_t *pathToCreate) {
    // Check to make sure there is room for path before copy
    if (wcslen(pathToCreate) <= MAX_PATH_LENGTH)
        wcscpy(nameOfDocument, pathToCreate);
    else {
        std::wcerr << L"Failed to create path, please check length of path name." << std::endl;
        return -1;
    }

    ASText textToCreatePath = NULL; // Text object to create ASPathName

    DURING

        // Determine size of wchar_t on system and get the ASText
        if (sizeof(wchar_t) == 2)
            textToCreatePath =
                ASTextFromUnicode(reinterpret_cast<ASUTF16Val *>(nameOfDocument), kUTF16HostEndian);
        else
            textToCreatePath =
                ASTextFromUnicode(reinterpret_cast<ASUTF16Val *>(nameOfDocument), kUTF32HostEndian);

        // Create the path for output file
        asPathName = ASFileSysCreatePathFromDIPathText(NULL, textToCreatePath, NULL);

    HANDLER

        return printErrorHandlerMessage(); // Return error code that was generated by exception

    END_HANDLER

    ASTextDestroy(textToCreatePath); // Release text object

    return errorCode;
}

//==============================================================================================================================
// makePath() - Wrap the various steps required to construct a proper ASPathName
//==============================================================================================================================

/* static */ ASPathName APDFLDoc::makePath(const char *path) {
    if (!path) {
        return NULL;
    }
    wchar_t *wc = NULL;
    {
        const size_t cSize = strlen(path) + 1;
        wc = new wchar_t[cSize];
        if (wc) {
            mbstowcs(wc, path, cSize);
        } else {
            return NULL;
        }
    }
    ASPathName pn = makePath(wc);
    delete[] wc;
    return pn;
}

/* static */ ASPathName APDFLDoc::makePath(const wchar_t *path) {
    ASText textToCreatePath = NULL; // Text object to create ASPathName
    ASPathName pn;
    DURING
        // Determine size of wchar_t on system and get the ASText
        if (sizeof(wchar_t) == 2)
            textToCreatePath = ASTextFromUnicode(reinterpret_cast<const ASUTF16Val *>(path), kUTF16HostEndian);
        else
            textToCreatePath = ASTextFromUnicode(reinterpret_cast<const ASUTF16Val *>(path), kUTF32HostEndian);

        pn = ASFileSysCreatePathFromDIPathText(NULL, textToCreatePath, NULL);
    HANDLER
        return NULL;
    END_HANDLER
    ASTextDestroy(textToCreatePath); // Release text object
    return pn;
}

//==============================================================================================================================
// insertPage() - Inserts a page into the PDDoc when provided ASFixed values for width, height and the location where the
// page will be inserted. The PDPage created is deallocated at the end of this method.
//==============================================================================================================================

ASErrorCode APDFLDoc::insertPage(const ASFixed &width, const ASFixed &height, ASInt32 afterPageNum) {

    DURING

        PDPage pdPage = NULL;
        // Set the page dimensions before creating the PDPage.
        ASFixedRect mediaBox;
        mediaBox.left = fixedZero;
        mediaBox.right = width;
        mediaBox.bottom = fixedZero;
        mediaBox.top = height;

        pdPage = PDDocCreatePage(pdDoc, afterPageNum, mediaBox); // Create and insert a page into the PDDoc.

        PDPageRelease(pdPage); // Release the PDPage object.

    HANDLER

        return printErrorHandlerMessage(); // Return the error code that was generated by the exception.

    END_HANDLER

    return errorCode;
}

//==============================================================================================================================
// insertPage() - This is the overloaded method that takes integer arguments instead of ASFixed values.
//==============================================================================================================================

ASErrorCode APDFLDoc::insertPage(const ASInt16 &width, const ASInt16 &height, ASInt32 afterPageNum) {
    return insertPage(Int16ToFixed(width), Int16ToFixed(height), afterPageNum);
}

//==============================================================================================================================
// getPage() - Accessor method for pages in the PDDoc. The argument is the page index with 0 being the first page.
//==============================================================================================================================

PDPage APDFLDoc::getPage(ASInt32 pageNumber) {

    PDPage pdPage = NULL;

    DURING

        pdPage = PDDocAcquirePage(pdDoc, pageNumber); // Get the page from the PDDoc object.

    HANDLER

        printErrorHandlerMessage(); // If an exception occured print the error.

    END_HANDLER

    return pdPage;
}

//==============================================================================================================================
// printErrorHandlerMessage() - Helper method that reports errors and returns an error code.
//==============================================================================================================================
ASErrorCode APDFLDoc::printErrorHandlerMessage() {

    errorCode = ERRORCODE; // Get the error code that caused the exception.

    char buf[256];

    ASGetErrorString(ERRORCODE, buf, sizeof(buf)); // Get the error message that coreesponds to the error code.

    std::cerr << "Error Code: " << errorCode << "Error Message: " << buf << std::endl;

    return errorCode;
}

//==============================================================================================================================
// ~APDFLDoc() - Releases resources if they haven't already been freed.
//==============================================================================================================================

APDFLDoc::~APDFLDoc() noexcept(false) {
    DURING

        if (pdDoc != NULL) // Close the PDDoc
            PDDocClose(pdDoc);

        if (asPathName != NULL) // Close the pathname
            ASFileSysReleasePath(NULL, asPathName);

    HANDLER

        printErrorHandlerMessage();

        RERAISE(); // Pass exception to the next handler on the stack.

    END_HANDLER
}

/* static */ ASFile APDFLDoc::OpenFlatFile(const char *path, int mode /* = ASFILE_READ */) {
    ASPathName pn(makePath(path));
    ASFile f(NULL);
    DURING
        ASErrorCode openErr = ASFileSysOpenFile(ASGetDefaultFileSys(), pn, mode, &f);

        // Release resources.
        ASFileSysReleasePath(ASGetDefaultFileSys(), pn);
        if (openErr) {
            // If there was an error opening the file, throw it.
            ASRaise(openErr);
        }
    HANDLER
        // If there was some other error, throw it.
        ASRaise(ERRORCODE);
    END_HANDLER

    return f;
}
