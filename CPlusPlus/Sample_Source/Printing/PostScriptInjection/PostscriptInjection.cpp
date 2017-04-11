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

// For a Windows system, set this variable to TRUE to send the output to a printer rather than to a file.
// The printer driver will still generate a file output.  It is possible to eliminate this file. Set the
// psParams.emitPS parameter equal to "false"; you can find this parameter in the code below.

#define WIN_PRINT_TO_PRINTER 0

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE  "CopyContent.pdf"
#define OUTPUT_FILE "PostscriptInjection-out.ps"

int printOnePDF(const char* pdfName, const char* pName);

int main (int argc, char *argv[])
{
    APDFLib libInit;       // Initialize the library
    if (libInit.isValid() == false)
    {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError ( errCode );
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFileName(argc > 2 ? argv[2] : OUTPUT_FILE);
    std::cout << "Reading " << csInputFileName.c_str() << ", injecting PostScript and saving as " 
              << csOutputFileName.c_str() << std::endl;

    int rc = printOnePDF(csInputFileName.c_str(), csOutputFileName.c_str());
    if ( rc != 0)
    {
        std::cout << "Processing of PDF was not successful - rc " << rc << std::endl;
    }
    return rc;
}

// Called once for each page, before the page itself is emitted.
ACCB1 void ACCB2 cbPrintAfterPageBeginSetup(PDDoc inDoc, ASInt32 pageNum,
                                     ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
    PDPage      pdPage;
    ASFixedRect pRect;
DURING
    pdPage = PDDocAcquirePage(inDoc, pageNum);
    PDPageGetMediaBox(pdPage, &pRect);
    sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterPageBeginSetup\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
    sprintf(pBuf,"%%    This is page %d\n%%    Media size: %f x %f in.\n",
        pageNum + 1, ASFixedToFloat(pRect.right - pRect.left),
        ASFixedToFloat(pRect.top - pRect.bottom) );
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER

DURING
    PDPageRelease(pdPage);
HANDLER
END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterBeginProlog(PDDoc inDoc, ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
DURING
    sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterBeginProlog\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterBeginSetup(PDDoc inDoc, ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
DURING
    sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterBeginSetup\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterEmitExtGState(ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
DURING
    sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterEmitExtGState\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER
}

ACCB1 void ACCB2 cbPrintAfterPageTrailer(PDDoc inDoc, ASInt32 pageNum,
                                     ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
DURING
    sprintf(pBuf, "%% PostScriptInjection: cbPrintAfterPageTrailer\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER
}

// Do not write anything to the output PostScript here. This can force the system to create a new
// document, leading to an extra blank page being output.
ACCB1 void ACCB2 cbPrintAfterTrailer(PDDoc inDoc, ASStm pageStm, void *clientData)
{
}

ACCB1 void ACCB2 cbPrintBeforeAcrobatProcsets(PDDoc inDoc, ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
DURING
    sprintf(pBuf, "%% PostScriptInjection: cbPrintBeforeAcrobatProcsets\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER
}

ACCB1 void ACCB2 cbPrintBeforeEndComments(PDDoc inDoc, ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
DURING
    sprintf(pBuf, "%% PostScriptInjection: cbPrintBeforeEndComments\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER
}

ACCB1 void ACCB2 cbPrintBeforeEndSetup(PDDoc inDoc, ASStm pageStm, void *clientData)
{
    char        pBuf[2048];
DURING
    sprintf(pBuf, "%% PostScriptInjection: cbPrintBeforeEndSetup\n");
    ASStmWrite(pBuf, 1, (ASTCount)strlen(pBuf), pageStm);
HANDLER
END_HANDLER
}

// A set of PostScript print defaults.
//
void setReasonablePDPrintParams(PDPrintParamsRec &psParams)
{
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

int printOnePDF(const char *pdfName, const char* psName)
{
    PDDoc                   inputPDDoc = 0;
    PDPrintParamsRec        psParams;
    PDFLPrintUserParamsRec  printParams;

DURING
    ASPathName pdfPath = APDFLDoc::makePath ( pdfName );
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
#endif

    ASFile pFile = 0;
    // For a Windows system, if the print-to-printer variable is non-zero (see the top
    // of the source file), the program will send the content to the printer.  Otherwise,
    // the program generates a PostScript file.
#if (WIN_ENV && WIN_PRINT_TO_PRINTER)
    printParams.emitToPrinter = true;
    printParams.inFileName = "Postscript Injection Test";
    printParams.driverName = "winspool";

    // Set the outFileName value to cause the printer driver to create a file, rather than APDFL.
    // The result reflects on what would have been sent to the printer, which is useful for debugging.
    printParams.outFileName = psName;

    // Provide a printer name.  For a list of the current Windows printer drivers available, look at
    // "Print server properties" in the Devices and Printers dialog in the Windows Control Panel.
    // Make sure you provide a PostScript printer driver here.
    printParams.deviceName = "\\\\gutenberg\\eaglesnest";
    printParams.portName = "Ne05:";
#else
    printParams.emitToFile = true;

    ASPathName path = APDFLDoc::makePath ( psName );
    // Create writeable file stream to handle the print stream
    ASFileSysOpenFile (NULL, path, ASFILE_WRITE | ASFILE_CREATE, &pFile);
    printParams.printStm = ASFileStmWrOpen (pFile, 0);
#endif

DURING

    AVExtensionMgrRegisterNotification(PSPrintAfterBeginPageSetupNSEL, 0,
        (void*)cbPrintAfterPageBeginSetup, 0);
    AVExtensionMgrRegisterNotification(PSPrintBeforeAcrobatProcsetsNSEL, 0,
        (void*)cbPrintBeforeAcrobatProcsets, 0);
    AVExtensionMgrRegisterNotification(PSPrintAfterPageTrailerNSEL, 0,
        (void*)cbPrintAfterPageTrailer, 0);
    AVExtensionMgrRegisterNotification(PSPrintAfterTrailerNSEL, 0,
        (void*)cbPrintAfterTrailer, 0);
    AVExtensionMgrRegisterNotification(PSPrintAfterBeginPrologNSEL, 0,
        (void*)cbPrintAfterBeginProlog, 0);
    AVExtensionMgrRegisterNotification(PSPrintAfterBeginSetupNSEL, 0,
        (void*)cbPrintAfterBeginSetup, 0);
    AVExtensionMgrRegisterNotification(PSPrintBeforeEndCommentsNSEL, 0,
        (void*)cbPrintBeforeEndComments, 0);

    // Executed, but content written to the stream is not emitted when sending
    // content to a printer.
    AVExtensionMgrRegisterNotification(PSPrintBeforeEndSetupNSEL, 0,
        (void*)cbPrintBeforeEndSetup, 0);

    AVExtensionMgrRegisterNotification(PSPrintAfterEmitExtGStateNSEL, 0,
        (void*)cbPrintAfterEmitExtGState, 0);

    PDFLPrintDoc(inputPDDoc, &printParams);
HANDLER
    APDFLib::displayError(ERRORCODE);
    return ERRORCODE;
END_HANDLER

    // Cleanup. If an exception is raised, mute it.
DURING
    if (printParams.printStm)
    {
        ASStmClose(printParams.printStm);
        ASFileClose ( pFile );
    }
    if (inputPDDoc)
    {
        PDDocClose(inputPDDoc);
    }
HANDLER
END_HANDLER
    return 0;
}
