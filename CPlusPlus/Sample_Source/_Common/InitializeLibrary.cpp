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
#include <stdio.h>
#include "InitializeLibrary.h"

#ifdef MAC_PLATFORM
#include <limits.h> /* PATH_MAX */
#include <stdio.h>
#include <stdlib.h>
#endif
//========================================================================================================
// Constructor:
// initializes APDFL and does not default the DL180PDFL.dll directory. dlDir should be a relative path.
//========================================================================================================
APDFLib::APDFLib(wchar_t *dlDir)
#if AIX_GCC_COMPAT
    : gccHelp()
#endif
{
    initValid = false; // Whether the initialization succeeded.

#ifdef WIN_PLATFORM
    if (dlDir == NULL)
        dlDir = L"..\\..\\..\\Binaries"; // The default DL180PDFL.lib directory.

    HINSTANCE dllInst = loadDFL180PDFL(dlDir);
    if (dllInst == 0) {
        initValid = false;
        return;
    }
#endif

    memset(&pdflData, 0, sizeof(PDFLDataRec)); // Clear the data struct so we can set its data.

    // Set PDFLDataRec's data.
    pdflData.size = sizeof(PDFLDataRec); // Give it its size.
    pdflData.allocator = NULL;           // Use default memory allocation procedures.
    fillDirectories();                   // Set the directory inclusion data.

#ifdef LOAD_PLUGIN
    strcpy(pdflData.pluginDirList[0], "../../../Binaries"); /* specify plugin path */
#endif
#ifdef WIN_PLATFORM
    pdflData.inst = dllInst;
#endif
    initError = PDFLInitHFT(&pdflData); // Initialize the library.
    if (initError == 0)                 // If initError is 0, initialization succeeded.
        initValid = true;
}

//========================================================================================================
// ASInt32 function:
// Reports whether an error happened during initialization and returns that error.
//========================================================================================================
ASInt32 APDFLib::getInitError() {
    if (initValid == false) {
        std::wcerr << L"Initialization error. See \"AcroErr.h\" for more info.\n" << std::endl;
        std::wcerr << L"Error system: " << ErrGetSystem(initError) << std::endl;
        std::wcerr << L"Error Severity: " << ErrGetSeverity(initError) << std::endl;
        std::wcerr << L"Error Code: " << ErrGetCode(initError) << std::endl;
    }

    return initError;
}

//========================================================================================================
// ASInt32 function:
// Loads the DL180PDFL library dynamically.
//========================================================================================================
#ifdef WIN_PLATFORM
HINSTANCE APDFLib::loadDFL180PDFL(wchar_t *relativeDir) {
    // Prepare to find the full path name.
    const int bufsize = 4096;             // The size of the buffer we'll write the path to.
    TCHAR pathBuffer[bufsize] = TEXT(""); // The buffer we'll write the path to.
    TCHAR **lppPart = {NULL};             // Recieves the address of the final name component.

    GetFullPathNameW(relativeDir, // Turn the relative path into an absolute path.
                     bufsize, pathBuffer, lppPart);

    SetDllDirectoryW(pathBuffer); // Add the path to the DLL directory.

    // Ensure we have read and write access to it.
    if (_waccess(pathBuffer, 06) == -1) {
      int errNumber = errno;

      if (EACCES == errNumber) {
        std::wcout << L"DL180PDFL.dll : ACCESS DENIED" << std::endl;
        return 0;
      }
      if (ENOENT == errNumber) {
        std::wcout << L"DL180PDFL.dll : COULD NOT LOCATE FILE" << std::endl;
        return 0;
      }

      if (EINVAL == errNumber) {
        std::wcout << L"DL180PDFL.dll : INVALID PARAMETER" << std::endl;
        return 0;
      }
    }

    return (LoadLibrary(L"DL180PDFL.dll"));
}
#endif
#ifndef WIN_PLATFORM
size_t strnlen_safe(const char *str, size_t maxSize) {
    if (!str)
        return 0;
    size_t n;
    for (n = 0; n < maxSize && *str; n++, str++)
        ;
    return n;
}
size_t strlen_safe(const char *str) {
    if (!str)
        return 0;
    size_t n;
    for (n = 0; *str != '\0'; n++, str++)
        ;
    return n;
}
static void copyChars(char *dest, const char *src, size_t numChars) {
    while (numChars-- > 0)
        *(dest++) = *(src++);
}
int strncpy_safe(char *dest, size_t dest_size, const char *src, size_t n) {
    if (!dest || !src || dest_size == 0)
        return -1;

    size_t len = strnlen_safe(src, n);
    if (len < dest_size) {
        copyChars(dest, src, len);
        dest[len] = '\0';
        return 0;
    } else {
        copyChars(dest, src, dest_size - 1);
        dest[dest_size - 1] = '\0';
        return -1;
    }
}
int strcpy_safe(char *dest, size_t dest_size, const char *src) {
    if (!dest || !src || dest_size == 0)
        return -1;

    return strncpy_safe(dest, dest_size, src, strlen_safe(src));
}
int strncat_safe(char *dest, size_t dest_size, const char *src, size_t n) {
    if (!dest || !src || dest_size == 0)
        return -1;

    size_t dest_len = strnlen_safe(dest, dest_size);
    if (dest_size <= dest_len)
        return -1;

    char *new_dest = dest + dest_len;
    return strncpy_safe(new_dest, dest_size - dest_len, src, n);
}
int strcat_safe(char *dest, size_t dest_size, const char *src) {
    if (!dest || !src || dest_size == 0)
        return -1;

    return strncat_safe(dest, dest_size, src, strlen_safe(src));
}
#endif

//========================================================================================================

// Void function:
// Sets directory information for our PDFLDataRec.
//========================================================================================================

void APDFLib::fillDirectories() {
#ifdef WIN_PLATFORM
    // Set the font directory list and its length.
    fontDirList[0] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Font";
    fontDirList[1] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\CMap";
    pdflData.dirList = fontDirList;
    pdflData.listLen = NUM_FONTS;

    // Set the color profile directory list and its length.
    colorProfDirList[0] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Color\\Profiles";
    pdflData.colorProfileDirList = colorProfDirList;
    pdflData.colorProfileDirListLen = NUM_COLOR_PROFS;

    // Set the Unicode directory.
    pdflData.cMapDirectory = fontDirList[1];
    pdflData.unicodeDirectory = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Unicode";

    // Set the OfficeMetrics directory.
    pdflData.officeMetricsDirectory = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\OfficeMetrics";

    // Set the plugin directory and its length.
    static TCHAR pluginPathBuffer[1024];
    GetFullPathName(L"..\\..\\..\\Binaries", 1024, pluginPathBuffer, 0);
    pluginDirList[0] = (ASUTF16Val *)pluginPathBuffer;
    pdflData.pluginDirList = pluginDirList;
    pdflData.pluginDirListLen = NUM_PLUGIN_DIRS;
#endif
#ifdef MAC_PLATFORM
#define MAX_PATH 1000
    const unsigned int NO_OF_RESOURCE_DIR = 2;
    const char *SUB_RESOURCE_DIR[NO_OF_RESOURCE_DIR] = {"Font", "CMap"};

    // Set the font directory list and its length.
    fontDirList[0] = (ASUTF16Val *)"../../../../Resources/Font";
    fontDirList[1] = (ASUTF16Val *)"../../../../Resources/CMap";
    pdflData.dirList = (char **)fontDirList;
    pdflData.listLen = NUM_FONTS;

    // Set the color profile directory list and its length.
    colorProfDirList[0] = (ASUTF16Val *)"../../../../Resources/Color/Profiles";
    pdflData.colorProfileDirList = (char **)colorProfDirList;
    pdflData.colorProfileDirListLen = NUM_COLOR_PROFS;

    // Set the Unicode directory.
    pdflData.cMapDirectory = (char *)fontDirList[1];
    pdflData.unicodeDirectory = (char *)"../../../../Resources/Unicode";
    char resourceDirectory[MAX_PATH];

    CFBundleRef bundleRef = CFBundleGetMainBundle();
    CFURLRef baseURL = CFBundleCopyBundleURL(bundleRef);
    CFURLRef resourceURL = CFURLCreateWithFileSystemPathRelativeToBase(
        kCFAllocatorDefault, CFSTR("../../../../Resources/"), kCFURLPOSIXPathStyle, true, baseURL);
    CFURLGetFileSystemRepresentation(resourceURL, true, (unsigned char *)resourceDirectory, MAX_PATH);

    CFRelease(baseURL);
    CFRelease(resourceURL);

    char **tmpP = NULL;
    tmpP = (char **)malloc(sizeof(char *) * NO_OF_RESOURCE_DIR);

    char fontPath[NO_OF_RESOURCE_DIR][MAX_PATH];
    for (int i = 0; i < NO_OF_RESOURCE_DIR; i++) {
        strncpy_safe(fontPath[i], sizeof(fontPath[i]), resourceDirectory, sizeof(resourceDirectory));
        strcat_safe(fontPath[i], sizeof(fontPath[i]), "/");
        strcat_safe(fontPath[i], sizeof(fontPath[i]), SUB_RESOURCE_DIR[i]);
    }

    pdflData.pluginDirList = (char **)malloc(NUM_PLUGIN_DIRS * sizeof(char *));
    for (int i = 0; i < NUM_PLUGIN_DIRS; ++i) {
        pdflData.pluginDirList[i] = (char *)malloc(sizeof(char) * MAX_PATH);
    }
    strncpy_safe(pdflData.pluginDirList[0], MAX_PATH, "../../../Binaries\0\0", 19);
    pdflData.pluginDirListLen = NUM_PLUGIN_DIRS;
#endif
#ifdef UNIX_PLATFORM
#define MAX_PATH 1000

    // Set the font directory list and its length.
    fontDirList[0] = (ASUTF16Val *)"../../../../Resources/Font";
    fontDirList[1] = (ASUTF16Val *)"../../../../Resources/CMap";
    pdflData.dirList = (char **)fontDirList;
    pdflData.listLen = NUM_FONTS;

    // Set the color profile directory list and its length.
    colorProfDirList[0] = (ASUTF16Val *)"../../../../Resources/Color/Profiles";
    pdflData.colorProfileDirList = (char **)colorProfDirList;
    pdflData.colorProfileDirListLen = NUM_COLOR_PROFS;

    // Set the Unicode directory.
    pdflData.cMapDirectory = (char *)fontDirList[1];
    pdflData.unicodeDirectory = (char *)"../../../../Resources/Unicode";

    // Set the plugin
    pluginDirList[0] = (ASUTF16Val *)"../../../Binaries";
    pdflData.pluginDirList = (char **)pluginDirList;
    pdflData.pluginDirListLen = NUM_PLUGIN_DIRS;
#endif
}

//========================================================================================================
// Void function:
// Utility method, may be used to print APDFL errors to the terminal.
//========================================================================================================
/* static */ void APDFLib::displayError(ASErrorCode errCode) {
    if (errCode == 0 || GetHFTLocations() == NULL)
        return;

    char errStr[250];
    fprintf(stderr, "[Error 0x%08x] %s\n", errCode, ASGetErrorString(errCode, errStr, sizeof(errStr)));
}

//========================================================================================================
// Destructor:
// Terminates the library when program ends.
//========================================================================================================
APDFLib::~APDFLib() {
    if (initValid)
        PDFLTermHFT();
}
