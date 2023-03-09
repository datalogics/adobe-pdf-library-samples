// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample allows a user to send a PDF document to a printer, using a Windows print interface.
//
// For more detail see the description of the PDFPrintDefault sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#pdfprintdefault

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

        // All platforms require start and end pages, and number of copies
        userParams.startPage = 0;                         /* Start with the first page */
        userParams.endPage = PDDocGetNumPages(inDoc) - 1; /* end with the last. */
        userParams.nCopies = 1;                           /* Always one copy */

        // All platforms allow specification of paper width and height.
        userParams.paperWidth = 0;  /* When left zero, defaults to 8.5 inches */
        userParams.paperHeight = 0; /* When left zero, defaults to 11 inches */

        // All platforms allow  specification of a list of fonts not to download
        // This list is generally empty
        userParams.dontEmitListLen = 0;
        userParams.dontEmitList = NULL;

        //=====================================================================================================================
        // Step 3) Establish printer destination
        //=====================================================================================================================
#ifdef MAC_PLATFORM
        /* Leaving all fields at thier default settings will print
        ** to the last print device used, or the default printer,
        ** according to the mac printer settings */

        /* These values an be set for a Mac printer */
        userParams.shrinkToFit = true;
        userParams.printAnnots = false;
        userParams.psLevel = 3;
        userParams.binaryOK = true;
        userParams.emitHalftones = false;
        userParams.reverse = false; /* print pages in reverse order */
        userParams.doOPP = false;

        NSPrintInfo *thePrintInfo = [NSPrintInfo sharedPrintInfo];
        userParams.printSession = (PMPrintSession)[thePrintInfo PMPrintSession]; /* Pointer to a PMPrintSession */
        userParams.printSettings =
            (PMPrintSettings)[thePrintInfo PMPrintSettings]; /* Pointer to a PMPrintSettings */
        userParams.pageFormat = (PMPageFormat)[thePrintInfo PMPageFormat]; /* Pointer to a PMPageFormat */

        const char *name = thePrintInfo.printer.name.UTF8String;
        std::cout << "Sending to the printer " << name << std::endl;

#endif
#ifdef WIN_ENV
        // Use a PrintDLg call to obtain the name of the default printer
        PRINTDLGW printDialog;

        // Initialize the PRINTDLG structure.
        memset(&printDialog, 0, sizeof(printDialog));
        printDialog.lStructSize = sizeof(printDialog);

        // Set the print dialog flags
        // In this case, return information for he default printer.
        printDialog.Flags = PD_RETURNDEFAULT;

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
        }

        //  These fields may be set in the user params for a windows printer
        userParams.inFileName = inPath; /* Specifies the name of the input file , as a char array */
        userParams.inFileNameW = NULL; /* Specifies the name of theinput file, as a wide char array */

        userParams.outFileName = NULL;  /* Forces output to a file, rather than to the printer, */
        userParams.outFileNameW = NULL; /*   using either a char array, or a wide char array */

        userParams.deviceName = NULL; /* Specifies the printer to use. The wide array form is used above */

        userParams.driverName = NULL; /* Specifies driver to use. Usually "winspool", May be left */
        userParams.driverNameW = NULL; /*   empty, and APDFL will fill in correctly */

        userParams.portName = NULL;  /* Specifies the port to reach the printer,May be left */
        userParams.portNameW = NULL; /*   empty, and APDFL will fill in correctly. */

        userParams.pDevMode = NULL; /* Allows the user to provide a DevMode block, to specify */
        userParams.pDevModeW =
            NULL; /*  divergent print options. If not supplied, the default printer options will be used. */

        userParams.hDC = NULL; /* You should supply a Device Context only if you wish your application
                               ** to control StartDoc and EndDoc. */

        userParams.shrinkToFit = true;
        userParams.printAnnots = false;
        userParams.psLevel = 3;
        userParams.binaryOK = true;
        userParams.emitHalftones = false;
        userParams.reverse = false; /* print pages in reverse order */
        userParams.doOPP = false;

        userParams.farEastFontOpt = 0;
        userParams.forceGDIPrint =
            false; /* When true, we will print via the GDI interface, even if the selected printer supports postscript. */

        std::cout << "Sending to the printer " << userParams.deviceNameW << std::endl;
#endif

        userParams.transQuality = 100;

#ifdef UNIX_ENV
        // print to the printer lp0, suppress reporting job number to stdout.
        userParams.command = "lp -s";

        std::cout << "Sending to the printer LP0." << std::endl;
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

#ifdef WIN_ENV
        // NOTE: if we set the value of userParams.inFileName to &path,
        //       set it back to null here, or we will attempt tofree it in
        //       the dispose logic.
        if (userParams.inFileName)
            userParams.inFileName = NULL;
#endif

        DisposePDPrintParams(&psParams);
        DisposePDFLPrintUserParams(&userParams);

        PDDocClose(inDoc); // Close the input document

    HANDLER
        errCode = ERRORCODE;
        lib.displayError(errCode); // If there was an error, display it
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the APDFL
};
