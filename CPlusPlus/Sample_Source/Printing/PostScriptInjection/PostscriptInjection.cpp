//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample illustrates adding PostScript comments and commands into a printable
// output document, generated using the Adobe PDF Library print API, PDFLPrintDoc.
// PDFLPrintDoc allows for either manual or automatic merging of printer capabilities
// when the print stream is generated, and it creates the stream most appropriate for
// the printer. The output is generated as a PostScript (.ps) file; if a local printer
// is defined and available, the program will send the output to the printer, to create
// a paper copy.
//
// For more detail see the description of the PostScriptInjection sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#postscriptinjection

#include <cstdio>

#include "PDFLCalls.h"
#include "NSelExpT.h"
#include "PDFLPrint.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#ifdef MAC_PLATFORM
#include <sys/types.h>
#include <sys/stat.h>
#endif

// Set this variable to 1 to send the output to a printer rather than to a file.
#define PRINT_TO_PRINTER 0

#define INPUT_FILE "../../../../Resources/Sample_Input/CopyContent.pdf"
#define OUTPUT_FILE "PostscriptInjection-out.ps"

int printOnePDF(const char *pdfName, const char *pName);

int main(int argc, char *argv[]) {
    APDFLib libInit; // Initialize the library
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_FILE);
    std::string csOutputFileName(argc > 2 ? argv[2] : OUTPUT_FILE);

#if !PRINT_TO_PRINTER
    std::cout << "Reading " << csInputFileName.c_str() << ", injecting PostScript and saving as "
              << csOutputFileName.c_str() << std::endl;
#else
    std::cout << "Reading " << csInputFileName.c_str() << ", injecting PostScript and Printing To ";
#endif

    int rc = printOnePDF(csInputFileName.c_str(), csOutputFileName.c_str());
    if (rc != 0) {
        std::cout << "Processing of PDF was not successful - rc " << rc << std::endl;
    }
    return rc;
}

// Called once for each page, before the page itself is emitted.
ACCB1 void ACCB2 cbPrintAfterPageBeginSetup(PDDoc inDoc, ASInt32 pageNum, ASStm pageStm, void *clientData) {
    char pBuf[2048];
    PDPage pdPage;
    ASFixedRect pRect;
    DURING
        pdPage = PDDocAcquirePage(inDoc, pageNum);
        PDPageGetMediaBox(pdPage, &pRect);
        sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterPageBeginSetup\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
        sprintf(pBuf, "%%    This is page %d\n%%    Media size: %f x %f in.\n", pageNum + 1,
                ASFixedToFloat(pRect.right - pRect.left), ASFixedToFloat(pRect.top - pRect.bottom));
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER

    DURING
        PDPageRelease(pdPage);
    HANDLER
    END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterBeginProlog(PDDoc inDoc, ASStm pageStm, void *clientData) {
    char pBuf[2048];
    DURING
        sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterBeginProlog\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterBeginSetup(PDDoc inDoc, ASStm pageStm, void *clientData) {
    char pBuf[2048];
    DURING
        sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterBeginSetup\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterEmitExtGState(ASStm pageStm, void *clientData) {
    char pBuf[2048];
    DURING
        sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterEmitExtGState\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterPageTrailer(PDDoc inDoc, ASInt32 pageNum, ASStm pageStm, void *clientData) {
    char pBuf[2048];
    DURING
        sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterPageTrailer\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER
}

// Do not write anything to the output PostScript here. This can force the system to create a new
// document, leading to an extra blank page being output.
ACCB1 void ACCB2 cbPrintAfterTrailer(PDDoc inDoc, ASStm pageStm, void *clientData) {}

ACCB1 void ACCB2 cbPrintBeforeAcrobatProcsets(PDDoc inDoc, ASStm pageStm, void *clientData) {
    char pBuf[2048];
    DURING
        sprintf(pBuf, "%% PostScriptInjection: cbPrintBeforeAcrobatProcsets\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER
}

ACCB1 void ACCB2 cbPrintBeforeEndComments(PDDoc inDoc, ASStm pageStm, void *clientData) {
    char pBuf[2048];
    DURING
        sprintf(pBuf, "%% PostScriptInjection: cbPrintBeforeEndComments\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER
}

ACCB1 void ACCB2 cbPrintBeforeEndSetup(PDDoc inDoc, ASStm pageStm, void *clientData) {
    char pBuf[2048];
    DURING
        sprintf(pBuf, "%% PostScriptInjection: cbPrintBeforeEndSetup\n");
        ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    HANDLER
    END_HANDLER
}

// A set of PostScript print defaults.
//
void setReasonablePDPrintParams(PDPrintParamsRec &psParams) {
    psParams.size = sizeof(PDPrintParamsRec);
    psParams.shrinkToFit = true;
    psParams.emitPS = true;
    psParams.psLevel = 2;
    psParams.outputType = PDOutput_PS;
    psParams.incBaseFonts = kIncludeNever;
    psParams.incType3Fonts = kIncludeOnEveryPage;

    psParams.emitShowpage = true;
    psParams.emitDSC = true;
    psParams.setupProcsets = true;
    psParams.scale = 100.0;

    psParams.incProcsets = kIncludeOncePerDoc;
    psParams.incOtherResources = kIncludeOncePerDoc;

    psParams.emitDeviceExtGState = true;
    psParams.emitPageClip = true;
    psParams.emitTransfer = true;
    psParams.emitBG = true;
    psParams.emitUCR = true;
    psParams.centerCropBox = true;
}

int printOnePDF(const char *pdfName, const char *psName) {
    PDDoc inputPDDoc = 0;
    PDPrintParamsRec psParams;
    PDFLPrintUserParamsRec printParams;

    DURING
        ASPathName pdfPath = APDFLDoc::makePath(pdfName);
        inputPDDoc = PDDocOpen(pdfPath, NULL, NULL, true);
        ASFileSysReleasePath(NULL, pdfPath);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    memset(&printParams, 0, sizeof(PDFLPrintUserParamsRec));
    memset(&psParams, 0, sizeof(PDPrintParamsRec));

    // These PostScript parameters affect both emitting to printer and emitting to a file.
    setReasonablePDPrintParams(psParams);

    printParams.printParams = &psParams;
    printParams.size = sizeof(PDFLPrintUserParamsRec);
    printParams.nCopies = 1;
    printParams.emitToFile = true; // emitToFile produces generic PS, not tied to a printer PPD.

    // A few parameters are treated differently under Unix.
#if UNIX_ENV
    PDPageRange pdrange;
    pdrange.startPage = 0;
    pdrange.endPage = PDDocGetNumPages(inputPDDoc) - 1;
    pdrange.pageSpec = PDAllPages;
    psParams.ranges = &pdrange;
    psParams.numRanges = 1;
#else
    printParams.psLevel = 2;
    printParams.shrinkToFit = true;
    printParams.startPage = 0;
    printParams.endPage = PDDocGetNumPages(inputPDDoc) - 1;
    PDPageRange pdrange;
    pdrange.startPage = 0;
    pdrange.endPage = PDDocGetNumPages(inputPDDoc) - 1;
    pdrange.pageSpec = PDAllPages;
    psParams.ranges = &pdrange;
    psParams.numRanges = 1;
#endif

    ASFile pFile = 0;
    // For a Windows system, if the print-to-printer variable is non-zero (see the top
    // of the source file), the program will send the content to the printer.  Otherwise,
    // the program generates a PostScript file.
#if PRINT_TO_PRINTER
    printParams.emitToPrinter = true; // emitToPrinter, for Mac and Windows, tailors PS to a specific PPD.
    printParams.emitToFile = false;
#ifdef WIN_ENV
    PRINTDLGW printDialog;

    // Initialize the PRINTDLG structure.
    memset(&printDialog, 0, sizeof(printDialog));
    printDialog.lStructSize = sizeof(printDialog);

    // Set the print dialog flags
    // Necessary to set PD_USEDEVMODECOPIESANDCOLLATE since the DEVMODE structure is being used by
    // APDFL to gather the number of copies and whether to collate or not.
    // This will also disable the Collate check box and the Number of Copies edit control unless
    // the printer driver supports multiple copies and collation.
    printDialog.Flags = PD_USEDEVMODECOPIESANDCOLLATE | PD_NOSELECTION;

    // Set the minimum and maximum page to allow in print dialog Pages selection
    printDialog.nMinPage = 1;
    printDialog.nMaxPage = PDDocGetNumPages(inputPDDoc);

    // Invoke the printer dialog box.
    if (PrintDlgW(&printDialog)) {
        // Make a copy of the DEVMODE structure to send to APDFL which contains information
        // on the number of copies requested and whether to collate them or not.
        LPDEVMODEW devMode = (LPDEVMODEW)(GlobalLock(printDialog.hDevMode));
        printParams.pDevModeW = (DEVMODEW *)malloc(devMode->dmSize + devMode->dmDriverExtra);
        memcpy(printParams.pDevModeW, devMode, devMode->dmSize + devMode->dmDriverExtra);
        GlobalUnlock(printDialog.hDevMode);

        // Need to retrieve the printer name from the hDevNames structure.  It is also available
        // in the hDevMode structure but can be truncated due to the dmDeviceName field only
        // being capable of holding CCHDEVICENAME (32) characters.
        LPDEVNAMES devNames = (LPDEVNAMES)(GlobalLock(printDialog.hDevNames));
        std::wstring dmDeviceName((WCHAR *)(devNames) + devNames->wDeviceOffset);
        std::wstring dmFileName((WCHAR *)(devNames) + devNames->wOutputOffset);
        GlobalUnlock(printDialog.hDevNames);

        size_t lenDeviceName = dmDeviceName.length();
        printParams.deviceNameW = new ASUns16[lenDeviceName + 1];
        memcpy(printParams.deviceNameW, dmDeviceName.data(), lenDeviceName * sizeof(ASUns16));
        printParams.deviceNameW[lenDeviceName] = 0;

        // Print specific pages if requested in the print dialog box.
        // Otherwise, the All was left selected and all pages will be printed.
        if (printDialog.Flags & PD_PAGENUMS) {
            printParams.startPage = printDialog.nFromPage - 1; // Specify starting page
            printParams.endPage = printDialog.nToPage - 1;     // Specify ending page
            pdrange.startPage = printParams.startPage;
            pdrange.endPage = printParams.endPage;
        }

        // Check to see if "Print to a file" has been selected.
        // If "Print to file" is selected, all we need to do is set the outFileNameW to the
        // returned dmFileName which will be returned as "FILE:".  No need to change any other
        // APDFL parameters such as emitToFile or emitToPrinter.  Windows takes care of
        // prompting for the output file and saves to the file instead of sending it
        // to the printer. This will save PS tailored to the selected printers PPD.
        if (printDialog.Flags & PD_PRINTTOFILE) {
            size_t lenFileName = dmFileName.length();
            printParams.outFileNameW = new ASUns16[lenFileName + 1];
            memcpy(printParams.outFileNameW, dmFileName.data(), lenFileName * sizeof(ASUns16));
            printParams.outFileNameW[lenFileName] = 0;
            if (!wcscmp((wchar_t *)printParams.outFileNameW, L"FILE:")) {
                delete (printParams.outFileNameW);
                printParams.outFileNameW = NULL;
                printParams.outFileName = new char[strlen(psName) + 1];
                strcpy(printParams.outFileName, psName);
            }
        }
        printParams.printParams = &psParams; // Connect the two structures

        if (printDialog.Flags & PD_PRINTTOFILE) {
            std::wcout << (wchar_t *)printParams.deviceNameW << L". Sending to the file ";
            if (printParams.outFileNameW != NULL)
                std::wcout << (wchar_t *)printParams.outFileNameW << std::endl;
            else
                std::cout << printParams.outFileName << std::endl;
        } else {
            std::wcout << (wchar_t *)printParams.deviceNameW << std::endl;
        }
    } else {
        std::wcout << std::endl << L"Print Canceled by user" << std::endl;
        return (-1);
    }
#endif // End of ifdef WIN_ENV

#ifdef MAC_PLATFORM

    /* set up dialog, and handle print settings */
    Boolean accepted = true;

    NSPrintInfo *thePrintInfo = [NSPrintInfo sharedPrintInfo];

    [NSApplication sharedApplication];
    NSPrintPanel *printPanel = [NSPrintPanel printPanel];
    accepted = [printPanel runModalWithPrintInfo:thePrintInfo];

    printParams.printSession = (PMPrintSession)[thePrintInfo PMPrintSession];
    printParams.printSettings = (PMPrintSettings)[thePrintInfo PMPrintSettings];
    printParams.pageFormat = (PMPageFormat)[thePrintInfo PMPageFormat];
    UInt32 first, last, numCopies;
    PMGetFirstPage(printParams.printSettings, &first);
    printParams.startPage = first - 1;
    PMGetLastPage(printParams.printSettings, &last);
    printParams.endPage = last - 1;
    PMGetCopies(printParams.printSettings, &numCopies);
    printParams.nCopies = numCopies;

    pdrange.startPage = printParams.startPage;
    pdrange.endPage = printParams.endPage;

    if (!accepted) {
        std::wcout << std::endl << L"Print Canceled by user" << std::endl;
        return (-1);
    }

    const char *name = thePrintInfo.printer.name.cString;
    std::cout << name << std::endl;

#endif // end of ifdef MAC_ENV

#ifdef UNIX_ENV
    // print to the printer lp0, suppress reporting job number to stdout.
    printParams.command = "lp -s";
    std::cout << "LP0." << std::endl;
#endif

#else  // Print to a file
    printParams.emitToFile = true;

    ASPathName path = APDFLDoc::makePath(psName);
    // Create writeable file stream to handle the print stream
    ASFileSysOpenFile(NULL, path, ASFILE_WRITE | ASFILE_CREATE, &pFile);
    printParams.printStm = ASFileStmWrOpen(pFile, 0);
#endif // end of ifdef PRINT_TO_PRINTER

    DURING

        AVExtensionMgrRegisterNotification(PSPrintAfterBeginPageSetupNSEL, 0,
                                           (void *)cbPrintAfterPageBeginSetup, 0);
        AVExtensionMgrRegisterNotification(PSPrintBeforeAcrobatProcsetsNSEL, 0,
                                           (void *)cbPrintBeforeAcrobatProcsets, 0);
        AVExtensionMgrRegisterNotification(PSPrintAfterPageTrailerNSEL, 0, (void *)cbPrintAfterPageTrailer, 0);
        AVExtensionMgrRegisterNotification(PSPrintAfterTrailerNSEL, 0, (void *)cbPrintAfterTrailer, 0);
        AVExtensionMgrRegisterNotification(PSPrintAfterBeginPrologNSEL, 0, (void *)cbPrintAfterBeginProlog, 0);
        AVExtensionMgrRegisterNotification(PSPrintAfterBeginSetupNSEL, 0, (void *)cbPrintAfterBeginSetup, 0);
        AVExtensionMgrRegisterNotification(PSPrintBeforeEndCommentsNSEL, 0, (void *)cbPrintBeforeEndComments, 0);

        // Executed, but content written to the stream is not emitted when sending
        // content to a printer.
        AVExtensionMgrRegisterNotification(PSPrintBeforeEndSetupNSEL, 0, (void *)cbPrintBeforeEndSetup, 0);

        AVExtensionMgrRegisterNotification(PSPrintAfterEmitExtGStateNSEL, 0,
                                           (void *)cbPrintAfterEmitExtGState, 0);

        PDFLPrintDoc(inputPDDoc, &printParams);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    // Cleanup. If an exception is raised, mute it.
    DURING
        if (printParams.printStm) {
            ASStmClose(printParams.printStm);
            ASFileClose(pFile);
        }
        if (inputPDDoc) {
            PDDocClose(inputPDDoc);
        }
    HANDLER
    END_HANDLER
    return 0;
}
