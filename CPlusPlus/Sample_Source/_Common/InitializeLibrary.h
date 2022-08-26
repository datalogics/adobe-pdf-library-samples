// Copyright (c) 2015-2016, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
//========================================================================
// Sample - Initialize: This class defines an object used for the
// initialization and termination of the Adobe PDF Library. It will also
// report initialization errors.
//
// InitializeLibrary.cpp: Contains the method implementations.
// InitializeLibrary.h: Contains the class definition.
//========================================================================

#ifndef INITLIB_H
#define INITLIB_H

#define NUM_FONTS 2 // The number of font directories we'll include during initialization.
#define NUM_COLOR_PROFS                                                                            \
    1 // The number of color profile directories we'll include during initialization.
#define NUM_PLUGIN_DIRS 1 // The number of plugin directories we'll include during initialization.

#include <iostream>
#include <cstring>
#include "PDFLCalls.h"
#include "ASCalls.h"

#ifdef AIX_GCC_COMPAT
#include <dlfcn.h>
// DLADD RobB 31Jan2017 - RAII helper to explicitly initialize xlC libraries on AIX from a g++ main()
class GCCAIXHelper {
  public:
    GCCAIXHelper()
        : hAXE(NULL), hXMP(NULL), hJP2K(NULL), hBIBUt(NULL), hBIB(NULL), hACE(NULL), hARE(NULL),
          hAGM(NULL), hCT(NULL), hPDFL(NULL) {
        if (!(hAXE = dlopen("libDL180AXE8SharedExpat.so", RTLD_NOW | RTLD_GLOBAL)))
            if (!(hXMP = dlopen("libDL180AdobeXMP.so", RTLD_NOW | RTLD_GLOBAL)))
                if (!(hJP2K = dlopen("libDL180JP2K.so", RTLD_NOW | RTLD_GLOBAL)))
                    if (!(hBIBUt = dlopen("libDL180BIBUtils.so", RTLD_NOW | RTLD_GLOBAL)))
                        if (!(hBIB = dlopen("libDL180BIB.so", RTLD_NOW | RTLD_GLOBAL)))
                            if (!(hACE = dlopen("libDL180ACE.so", RTLD_NOW | RTLD_GLOBAL)))
                                if (!(hARE = dlopen("libDL180ARE.so", RTLD_NOW | RTLD_GLOBAL)))
                                    if (!(hAGM = dlopen("libDL180AGM.so", RTLD_NOW | RTLD_GLOBAL)))
                                        if (!(hCT = dlopen("libDL180CoolType.so", RTLD_NOW | RTLD_GLOBAL)))
                                            hPDFL = dlopen("libDL180pdfl.so", RTLD_NOW | RTLD_GLOBAL);
    }
    ~GCCAIXHelper() {
        if (hPDFL)
            dlclose(hPDFL);
        if (hCT)
            dlclose(hCT);
        if (hAGM)
            dlclose(hAGM);
        if (hARE)
            dlclose(hARE);
        if (hACE)
            dlclose(hACE);
        if (hBIB)
            dlclose(hBIB);
        if (hBIBUt)
            dlclose(hBIBUt);
        if (hJP2K)
            dlclose(hJP2K);
        if (hXMP)
            dlclose(hXMP);
        if (hAXE)
            dlclose(hAXE);
    }

  private:
    void *hAXE, *hXMP, *hJP2K, *hBIBUt, *hBIB, *hACE, *hARE, *hAGM, *hCT, *hPDFL;
};
#endif // AIX_GCC_COMPAT
class APDFLib {
  public:
    APDFLib(wchar_t *dlDir = NULL); // Constructor initializes APDFL and sets the path to DL180PDFL.dll to dlDir. If NULL is passed, defaults to ../../../Binaries. dlDir should be a relative path.
    ~APDFLib();                     // Destructor terminates APDFL.

    ASInt32 getInitError(); // Reports whether an error happened during initialization and returns that error.
    ASBool isValid() { return initValid; }; // Returns true if the library initialized successfully.
    static void displayError(ASErrorCode); // Utility method, may be used to print APDFL errors to the terminal.

  private:
    APDFLib(const APDFLib &); // not implemented
    PDFLDataRec pdflData;     // A struct containing information that APDFL initializes with.
    ASInt32 initError;        // Used to record initialization errors.
    ASBool initValid;         // Set to true if the library initializes successfully.

    void fillDirectories(); // Sets directory information for our PDFLDataRec.
#if WIN_PLATFORM
    HINSTANCE loadDFL180PDFL(wchar_t *relativeDir); // Loads the DL180PDFL library dynamically.
#endif

    ASUTF16Val *fontDirList[NUM_FONTS]; // List of font directories we'll include during initialization. //TODO: platform divergences
    ASUTF16Val *colorProfDirList[NUM_COLOR_PROFS]; // List of color profile directories we'll include during initialization.     //TODO: platform divergences
    ASUTF16Val *pluginDirList[NUM_PLUGIN_DIRS]; // List of plugin directories we'll include during initialization.            //TODO: platform divergences
#if AIX_GCC_COMPAT
    GCCAIXHelper gccHelp;
#endif
};

#endif // INITLIB_H
