//
// Copyright 2015, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This is a Windows specific sample class...
//

#include <cstdio>

#include "ScopeGuard.h" // for ON_BLOCK_EXIT

#include "PDFLCalls.h"
#include "PDCalls.h"
#include "ASCalls.h"
#include "ASExtraCalls.h" // for ASTextFromUnicode

#include "SampleCustomExceptions.h"
#include "SampleFileUtils.h"

//
// File Utility Functions
//
// A collection of simple, utility functions that present platform user interface
// functionality in support of opening and saving file(s) and similar activities.
//

PDDoc SelectPDFDocument() {
    ASErrorCode err = 0;

    //
    // Populate pdfFilePath with a path collected from a user...
    //

    std::wstring pdfFilePath;
    SelectPDFPath(pdfFilePath);
    if (pdfFilePath.empty()) { // User canceled or similar
        return NULL;
    }

    PDDoc pDoc = NULL;
    DURING
        {
            //
            // Attempt to convert pdfFilePath to the correct composition for use with PDFL...
            //

            ASFileSys fileSys = ASGetDefaultFileSys();
            ASAtom pathSpecType = ASAtomFromString("DIPathWithASText");
            ASText pathSpec = ASTextFromUnicode(((ASUTF16Val *)pdfFilePath.c_str()), kUTF16HostEndian);
            ASPathName path = ASFileSysCreatePathName(NULL, pathSpecType, pathSpec, NULL);
            ON_BLOCK_EXIT(ASFileSysReleasePath, fileSys, path);

            //
            // Attempt to open the (pdf) document...
            //

            pDoc = PDDocOpen(path, fileSys, NULL, TRUE);
        }
    HANDLER
        { err = ERRORCODE; }
    END_HANDLER

    if (err) { // Something bad happened while trying to open the document (maybe the selected file *is not actually a valid* PDF?)
        if (pDoc) { // Open pdfs are *locked*. NEVER forget to close them!
            DURING
                PDDocClose(pDoc);
            HANDLER
                // Ignore any error
            END_HANDLER
        }

        //
        // Convert the error to an easy to review and nest, custom exception exception and throw *that*
        //

        char msg[64] = {0};
        sprintf_s(msg, sizeof(msg), "Error: %d\r\n", err);
        throw new InvalidFileException(msg);
    }

    return pDoc;
}

//
// This utility function presents Windows User Interface (i.e., Open File)
// and prompts the user to select a PDF. Filtration restricts what the user
// can select.
//
// See also: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646927%28v=vs.85%29.aspx
//
void SelectPDFPath(std::wstring &pdfFilePath /* OUT */) {
    OPENFILENAMEW ofn;
    memset(&ofn, 0, sizeof(OPENFILENAMEW));
    ofn.lStructSize = sizeof(OPENFILENAMEW);

    ofn.lpstrTitle = L"Select a PDF";
    ofn.lpstrDefExt = L"pdf";
    ofn.lpstrFilter = L"Portable Document File\0*.pdf;\0";
    ofn.nFilterIndex = 1;
    ofn.Flags = OFN_EXPLORER | OFN_LONGNAMES | OFN_NONETWORKBUTTON | OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;

    WCHAR pathBuf[0x1000] = {0};

    ofn.lpstrFile = pathBuf;
    ofn.nMaxFile = sizeof(pathBuf) / sizeof(WCHAR);

    SetLastError(0);
    BOOL result = GetOpenFileNameW(&ofn);
    if (!result) {
        DWORD err = CommDlgExtendedError();
        if (err == 0) { // Dialog canceled
            return;
        }

        //
        // Convert the error to an easy to review and nest, custom exception exception and throw *that*
        //

        char msg[64] = {0};
        sprintf_s(msg, sizeof(msg), "Error: %d\r\n", err);
        throw new GeneralException(msg);
    }

    if (ofn.lpstrFile && ofn.lpstrFile[0] != 0 /* not empty */) { // Now (and ONLY now) that we have a known good result, use it...
        pdfFilePath = ofn.lpstrFile;
    }
}

//
// This utility function presents Windows User Interface (i.e., Save As)
// and prompts for a PDF path. Filtration restricts what the user
// can specify.
//
// See also: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646928%28v=vs.85%29.aspx
//
void GetSaveAsFilePath(std::wstring &pdfFilePath /* OUT */) {
    OPENFILENAMEW ofn;
    memset(&ofn, 0, sizeof(OPENFILENAMEW));
    ofn.lStructSize = sizeof(OPENFILENAMEW);

    ofn.lpstrTitle = L"Save as PDF...";

    ofn.lpstrDefExt = L"pdf";
    ofn.lpstrFilter = L"Portable Document File\0*.pdf;\0";
    ofn.nFilterIndex = 1;
    ofn.Flags = OFN_EXPLORER | OFN_LONGNAMES | OFN_NONETWORKBUTTON | OFN_OVERWRITEPROMPT;

    WCHAR pathBuf[0x1000] = {0};

    ofn.lpstrFile = pathBuf;
    ofn.nMaxFile = sizeof(pathBuf) / sizeof(WCHAR);

    SetLastError(0);
    BOOL result = GetSaveFileNameW(&ofn);
    if (!result) {
        DWORD err = CommDlgExtendedError();
        { // dialog canceled
            return;
        }

        //
        // Convert the error to an easy to review and nest, custom exception exception and throw *that*
        //

        char msg[64] = {0};
        sprintf_s(msg, sizeof(msg), "Error: %d\r\n", err);
        throw new GeneralException(msg);
    }

    if (ofn.lpstrFile && ofn.lpstrFile[0] != 0 /* not empty */) {
        size_t len = wcslen(ofn.lpstrFile);
        LPWSTR newFilePath = new WCHAR[len + 1];

        memcpy(newFilePath, ofn.lpstrFile, len * sizeof(WCHAR));
        newFilePath[len] = 0;

        if (ofn.nFileExtension > 0 && ofn.nFileExtension < len) { // Remove the extension (we'll add one later when we know what type of file to make)
            newFilePath[ofn.nFileExtension - 1 /* account for the '.' */] = 0;
        }

        pdfFilePath = newFilePath;
    }
}

//
// This utility function presents Windows User Interface (i.e., Open File)
// and prompts the user to select an ICC profile. Filtration restricts what the user
// can select.
//
// See also: https://msdn.microsoft.com/en-us/library/windows/desktop/ms646927%28v=vs.85%29.aspx
//
void SelectICCProfile(std::wstring &iccFilePath /* OUT */) {
    OPENFILENAMEW ofn;
    memset(&ofn, 0, sizeof(OPENFILENAMEW));
    ofn.lStructSize = sizeof(OPENFILENAMEW);

    ofn.lpstrTitle = L"Select a ICC Profile";
    ofn.lpstrDefExt = L"icc";
    ofn.lpstrFilter = L"ICC Color Profile\0*.icc;\0";
    ofn.nFilterIndex = 1;
    ofn.Flags = OFN_EXPLORER | OFN_LONGNAMES | OFN_NONETWORKBUTTON | OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;

    WCHAR pathBuf[0x1000] = {0};

    ofn.lpstrFile = pathBuf;
    ofn.nMaxFile = sizeof(pathBuf) / sizeof(WCHAR);

    SetLastError(0);
    BOOL result = GetOpenFileNameW(&ofn);
    if (!result) {
        DWORD err = CommDlgExtendedError();
        if (err == 0) { // Dialog canceled
            return;
        }

        //
        // Convert the error to an easy to review and nest, custom exception exception and throw *that*
        //

        char msg[64] = {0};
        sprintf_s(msg, sizeof(msg), "Error: %d\r\n", err);
        throw new GeneralException(msg);
    }

    if (ofn.lpstrFile && ofn.lpstrFile[0] != 0 /* not empty */) { // Now (and ONLY now) that we have a known good result, use it...
        iccFilePath = ofn.lpstrFile;
    }
}
