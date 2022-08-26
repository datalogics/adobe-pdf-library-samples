// Copyright (c) 2016, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample allows a user to send a PDF document to a printer, using a Windows print interface.
//
// For more detail see the description of the PDFPrintGUI sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#pdfprintgui

/* Printing Support */
#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "PDFLPrint.h"
#include "SetupPrintParams.h"

#ifdef MAC_PLATFORM
#include "Cocoa/Cocoa.h"
#include <sys/types.h>
#include <sys/stat.h>
#endif

#define defaultDocument "../../../../Resources/Sample_Input/printpdf.pdf"

int main(int argc, char **argv) {
    APDFLib lib;             // Initialize the Adobe PDF Library
    ASErrorCode errCode = 0; // This will catch error codes thrown during library usage

    if (lib.isValid() == false) // If it failed to initialize, return the error code
        return lib.getInitError();

    DURING

        //=====================================================================================================================
        // Step 1) Open the input PDF
        //=====================================================================================================================
        char inPath[2048];

        // If a file name is not specified on the command line, use the default name
        if (argc < 2)
            strcpy(inPath, defaultDocument);
        else
            strcpy(inPath, argv[1]);

        std::wcout << L"Opening the input document - " << inPath << std::endl;

        APDFLDoc inAPDoc(inPath, true); // Open the input document, repairing it if necessary.
        PDDoc inDoc = inAPDoc.getPDDoc();

        //=====================================================================================================================
        // Step 2) Initialize print parameters
        //=====================================================================================================================

        std::wcout << L"Initializing print parameters." << std::endl;

        // Set defaults (see SetupPrintParams files in Common folder)
        PDPrintParamsRec psParams;
        SetupPDPrintParams(&psParams);

        PDFLPrintUserParamsRec userParams;
        SetupPDFLPrintUserParams(&userParams);

        // Link print params to userParams
        userParams.printParams = &psParams;

        // Override defaults with specifics for this sample
        userParams.emitToFile = false; // Print to printer
        userParams.emitToPrinter = true;

        //=====================================================================================================================
        // Step 3) Open the Print Dialog UI and gather user input
        //=====================================================================================================================
#ifdef MAC_PLATFORM
        /* set up dialog, and handle print settings */
        Boolean accepted = true;

        NSPrintInfo *thePrintInfo = [NSPrintInfo sharedPrintInfo];

        [NSApplication sharedApplication];
        NSPrintPanel *printPanel = [NSPrintPanel printPanel];
        accepted = [printPanel runModalWithPrintInfo:thePrintInfo];

        userParams.printSession = (PMPrintSession)[thePrintInfo PMPrintSession];
        userParams.printSettings = (PMPrintSettings)[thePrintInfo PMPrintSettings];
        userParams.pageFormat = (PMPageFormat)[thePrintInfo PMPageFormat];
        UInt32 first, last, numCopies;
        PMGetFirstPage(userParams.printSettings, &first);
        userParams.startPage = first;
        PMGetLastPage(userParams.printSettings, &last);
        userParams.endPage = last;
        PMGetCopies(userParams.printSettings, &numCopies);
        userParams.nCopies = numCopies;

        if (!accepted) {
            std::wcout << L"Print Canceled by user" << std::endl;
            DisposePDPrintParams(&psParams);
            DisposePDFLPrintUserParams(&userParams);
            PDDocClose(inDoc);
            return (-1);
        }

        const char *name = thePrintInfo.printer.name.UTF8String;
        std::cout << "Sending to the printer " << name << std::endl;
#else
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
        printDialog.nMaxPage = inAPDoc.numPages();

        // Invoke the printer dialog box.
        if (PrintDlgW(&printDialog)) {
            // Make a copy of the DEVMODE structure to send to APDFL which contains information
            // on the number of copies requested and whether to collate them or not.
            LPDEVMODEW devMode = (LPDEVMODEW)(GlobalLock(printDialog.hDevMode));
            userParams.pDevModeW = (DEVMODEW *)malloc(devMode->dmSize + devMode->dmDriverExtra);
            memcpy(userParams.pDevModeW, devMode, devMode->dmSize + devMode->dmDriverExtra);
            GlobalUnlock(printDialog.hDevMode);

            // Need to retrieve the printer name from the hDevNames structure.  It is also available
            // in the hDevMode structure but can be truncated due to the dmDeviceName field only
            // being capable of holding CCHDEVICENAME (32) characters.
            LPDEVNAMES devNames = (LPDEVNAMES)(GlobalLock(printDialog.hDevNames));
            std::wstring dmDeviceName((WCHAR *)(devNames) + devNames->wDeviceOffset);
            std::wstring dmFileName((WCHAR *)(devNames) + devNames->wOutputOffset);
            GlobalUnlock(printDialog.hDevNames);

            size_t lenDeviceName = dmDeviceName.length();
            userParams.deviceNameW = new ASUns16[lenDeviceName + 1];
            memcpy(userParams.deviceNameW, dmDeviceName.data(), lenDeviceName * sizeof(ASUns16));
            userParams.deviceNameW[lenDeviceName] = 0;

            // Print specific pages if requested in the print dialog box.
            // Otherwise, the All was left selected and all pages will be printed.
            if (printDialog.Flags & PD_PAGENUMS) {
                psParams.ranges[0].startPage = printDialog.nFromPage - 1; // Specify starting page
                psParams.ranges[0].endPage = printDialog.nToPage - 1;     // Specify ending page
            }

            // Check to see if "Print to a file" has been selected.
            // If "Print to file" is selected, all we need to do is set the outFileNameW to the
            // returned dmFileName which will be returned as "FILE:".  No need to change any other
            // APDFL parameters such as emitToFile or emitToPrinter.  Windows takes care of
            // prompting for the output file and saves to the file instead of sending it
            // to the printer.
            if (printDialog.Flags & PD_PRINTTOFILE) {
                size_t lenFileName = dmFileName.length();
                userParams.outFileNameW = new ASUns16[lenFileName + 1];
                memcpy(userParams.outFileNameW, dmFileName.data(), lenFileName * sizeof(ASUns16));
                userParams.outFileNameW[lenFileName] = 0;
            }
            userParams.printParams = &psParams; // Connect the two structures

            if (printDialog.Flags & PD_PRINTTOFILE) {
                std::wcout << L"Sending to the file." << std::endl;
            } else {
                std::wcout << L"Sending to the printer." << std::endl;
            }
        } else {
            std::wcout << L"Print Canceled by user" << std::endl;
            DisposePDPrintParams(&psParams);
            DisposePDFLPrintUserParams(&userParams);
            PDDocClose(inDoc);
            return (-1);
        }
#endif
        //=====================================================================================================================
        // Step 4) Write to printer or file and clean up
        //=====================================================================================================================

        DURING
            PDFLPrintDoc(inDoc, &userParams);
        HANDLER
            errCode = ERRORCODE;
            lib.displayError(errCode); // If there was an error, display it
        END_HANDLER

        std::wcout << L"Closing documents and cleaning up." << std::endl;

        // Cleanup and free memory
        DisposePDPrintParams(&psParams);
        DisposePDFLPrintUserParams(&userParams);

        PDDocClose(inDoc); // Close the input document

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode); // If there was an error, display it
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the APDFL
};
