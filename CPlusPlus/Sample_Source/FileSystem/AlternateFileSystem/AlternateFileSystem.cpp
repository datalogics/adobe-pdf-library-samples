//
//  Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
//  For complete copyright information, see:
//  http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample program shows how to implement an ASFileSys structure in an
// Adobe PDF Library application. It also demonstrates adding a simplified
// “in memory” file system for use in an app.
//
// The Alternate File System structure (ASFileSys) is a series of routines within
// the Adobe PDF Library that allows a developer to implement file system services
// in an APDFL application. ASFileSys allows an application to open and delete files,
// read data from a file, and write data to a file. Adobe Acrobat and the Adobe PDF
// Library both offer a built-in Alternate File System that serves as the platform’s
// native file system, but developers working with the Adobe PDF Library can create
// additional ASFileSys objects to serve other file systems.  The sample does not
// demonstrate all of the calls available for use with ASFileSys, but it implements
// enough to illustrate the most common uses.
//
// This sample does not define input or output files, or an input directory.
//
// For more detail see the description of the AlternateFileSystem sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#alternatefilesystem

// Some notes on this file systems behaviour
//
//  All file reads and writes will occur as simple memory access.
//  A file may have a "name" of the empty string (""). Such a file will exist in memory only, and never be saved or
//   initialized from a disk file
//  If a files name is NOT the empty string, then, at open time, if the file mode is read and not create, we will use the
//    native file system to read the entire file into memory, in a single read. At close time, if the file is opened for write, we will copy the entire
//    contents of the file to a disk file of the given name, using the native file system. we will free the in-memory copy of
//    that file regardless of open mode.
//  Any file may be reopened after closing using the ASFileReopen() interface. If the file name is the empty string, the in-memory copy of the
//     file remains and is reused. If the file string is not empty, and the file mode is reopened for read, and not create, we will reload the file.
//  The ASFileSysRemove will free the memory buffer and destroy the internal "directory" for any file. For files with non-empty names, it will also
//     remove the file from the disk, using the native file system, if the open mode included ASFILE_TEMPORARY.
//
//  The ASPathName object used here will be an ASText with the DI Representation of the file path carried in it. This should provide complete support
//     for unicode file names.

#include "ASCalls.h"
#include "ASExtraCalls.h"
#include "PDFLCalls.h"

#include <cstdio>
#include <cstring>

using namespace std;

#ifdef WIN32
#pragma warning(disable : 4267)
#else
#include <stdio.h>
#include <wchar.h>
#endif

struct _altFSFile {
    ASText Name;             // When present, the i/o file system file name reflected here
    char *Buffer;            // Contents of this file
    ASFilePos64 BufferSize;  // Maximum size of this file without expand
    ASFilePos64 CurrentSize; // Current size of this file
    struct _altFSFile *Prev, *Next;
    ASBool temp;
};
typedef struct _altFSFile altFSFile;

// The altFSFileHandle is what is returned as an MDFile
struct _altFSFileHandle {
    struct _altFSFile *File; // Pointer to common file info
    ASFilePos64 Position;    // Last byte read or written
    ASUns16 Flags;           // Open flags
};
typedef struct _altFSFileHandle altFSFileHandle;

static ASFileSysRec altFSRec;
static ASBool altFSRecDefined = FALSE;
static altFSFile *altFileRoot = NULL;
static ASFileSys nativeFileSys = NULL;

// Increase size of buffer by a factor of two.
static void altFSExpandBuffer(altFSFile *File, ASSize_t Size) {
    ASSize_t TargetSize;

    if ((!File) || (!Size) || (Size < File->BufferSize))
        return;

    TargetSize = (Size > File->BufferSize * 2) ? Size : (size_t)(File->BufferSize * 2);
    if (TargetSize < 1024) // Absolute minimum of 1k per file expansion
        TargetSize = 1024;

    if (!File->Buffer)
        File->Buffer = (char *)ASmalloc(TargetSize);
    else
        File->Buffer = (char *)ASrealloc(File->Buffer, TargetSize);
    File->BufferSize = TargetSize;
    if (!File->Buffer)
        ASRaise(genErrNoMemory);

    return;
}

// Find an element in the list
altFSFile *findAltFSFile(ASPathName Path) {
    altFSFile *altFile;

    /* If the path name is the empty path,
    ** then we cannot find it
    */
    ASText emptyPath = ASTextNew();
    if (!ASTextCmp(emptyPath, (ASText)Path))
        return 0;

    /* Locate the file in the file system that has the same name as the
    ** requested file
    */
    for (altFile = altFileRoot; altFile; altFile = altFile->Next) {
        if (!ASTextCmp(altFile->Name, (ASText)Path))
            break;
    }
    return altFile;
}

// Remove an element from the list
ASInt32 altFSRemove(ASPathName Path) {
    altFSFile *altFile;

    altFile = findAltFSFile(Path);
    if (altFile->Name) {
        if (altFile->temp) {
            /* Remove the file from the native file system as well */
            ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, Path, nativeFileSys);
            ASFileSysRemoveFile(nativeFileSys, nativePath);
            ASFileSysReleasePath(nativeFileSys, nativePath);
        }
        ASfree(altFile->Name);
    }
    ASfree(altFile->Buffer);

    if (altFile->Next)
        altFile->Next->Prev = altFile->Prev;
    if (altFile->Prev)
        altFile->Prev->Next = altFile->Next;
    else
        altFileRoot = altFile->Next;
    ASfree(altFile);

    return 0;
}

// Open a file in this file system with the given mode.
// If the file exists on the list, update the status flags and return.
// Otherwise, insert the file at the head of the list.
ASInt32 altFSOpen(ASPathName Path, ASUns16 Flags, MDFile *File) {
    altFSFileHandle *altFileHandle;
    altFSFile *altFile;

    // If the user asks for no file open mode, do not provide a file.
    if (Flags == 0)
        return 1;

    altFile = findAltFSFile(Path);
    if (altFile) {
        altFileHandle = (altFSFileHandle *)ASmalloc(sizeof(altFSFileHandle));
        if (!altFileHandle)
            ASRaise(genErrNoMemory);
        altFileHandle->Position = 0;
        altFileHandle->Flags = Flags;
        altFileHandle->File = altFile;

        *File = (MDFile *)altFileHandle;
        return 0;
    }

    altFile = (altFSFile *)ASmalloc(sizeof(altFSFile));
    altFileHandle = (altFSFileHandle *)ASmalloc(sizeof(altFSFileHandle));
    if (!altFile || !altFileHandle)
        ASRaise(genErrNoMemory);

    memset(altFile, 0, sizeof(altFSFile));
    altFile->Next = altFileRoot;
    altFileRoot = altFile;
    if (altFile->Next)
        altFile->Next->Prev = altFile;

    if (Flags & ASFILE_TEMPORARY)
        altFile->temp = true;
    else
        altFile->temp = false;

    altFileHandle->Flags = Flags;
    altFileHandle->File = altFile;
    altFileHandle->Position = 0;
    altFile->Name = ASTextDup((ASText)Path);

    ASText emptyText = ASTextNew();

    // If a file is in read mode, and not to be created, with no data
    // currently associated with the file, the program reads the
    // corresponding file on the disk as the initial data, using the native file system. If the file
    // is only opening in write mode, don't bother reading it.
    if ((altFile->Name) && (ASTextCmp(altFile->Name, emptyText)) && !(altFile->CurrentSize) &&
        !(Flags & ASFILE_CREATE)) {

        ASAtom pathType;
        ASPlatformPath platformPath;
        const char *platformPathStr = NULL;
        FILE *nativeFile = NULL;
#if MAC_PLATFORM
        pathType = ASAtomFromString("POSIXPath"); /*  For MAC, we need the POSIX path */
#else
        pathType = ASAtomFromString("Cstring"); /* For windows and Unix, a simple CString path (Which may be UTF-8) */
#endif // MAC_PLATFORM

        ASErrorCode errNum = ASFileSysAcquirePlatformPath(&altFSRec, Path, pathType, &platformPath);
        if (!errNum) {
            platformPathStr = ASPlatformPathGetCstringPtr(platformPath);

#if WIN_ENV
            errno_t errOpen = fopen_s(&nativeFile, platformPathStr, "rb");
            if (errOpen)
                nativeFile = NULL;
#else
            nativeFile = fopen(platformPathStr, "rb");
#endif // WIN_PLATFORM

            ASFileSysReleasePlatformPath(&altFSRec, platformPath);

            if (nativeFile) {
                fseek(nativeFile, 0, SEEK_END);
                ASFilePos64 size = ftell(nativeFile);
                fseek(nativeFile, 0, SEEK_SET);
                altFile->Buffer = (char *)ASmalloc((size_t)size);
                if (!altFile->Buffer) {
                    fclose(nativeFile);
                    ASfree(altFileHandle);
                    ASRaise(genErrNoMemory);
                }
                altFile->BufferSize = (ASFilePos64)size;
                altFile->CurrentSize = fread(altFile->Buffer, 1, (ASTArraySize)size, nativeFile);
                fclose(nativeFile);
            }
        } /* If file did not exist, initialize it as an empty file */
    }
    ASTextDestroy(emptyText);

    *File = (MDFile *)altFileHandle;

    return 0;
}

// Mark the file on the file system as closed
ASInt32 altFSClose(MDFile File) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;

    ASText emptyText = ASTextNew();

    if ((altFile->Name) && (ASTextCmp(altFile->Name, emptyText)) &&
        (altFileHandle->Flags & ASFILE_WRITE) && (altFile->CurrentSize > 0)) {
        // Write this to a real file system, if a path name is given
        /* Use the platform native file system to write this file to disk */

        ASAtom pathType;
        ASPlatformPath platformPath;
        const char *platformPathStr = NULL;
        FILE *nativeFile = NULL;
#if MAC_PLATFORM
        pathType = ASAtomFromString("POSIXPath"); /*  For MAC, we need the POSIX path */
#else
        pathType = ASAtomFromString("Cstring"); /* For windows and Unix, a simple CString path (Which may be UTF-8) */
#endif // MAC_PLATFORM
        ASErrorCode errNum =
            ASFileSysAcquirePlatformPath(&altFSRec, (ASPathName)altFile->Name, pathType, &platformPath);
        if (!errNum) {
            platformPathStr = ASPlatformPathGetCstringPtr(platformPath);

#if WIN_ENV
            errno_t errOpen = fopen_s(&nativeFile, platformPathStr, "wb");
            if (errOpen)
                nativeFile = NULL;
#else
            nativeFile = fopen(platformPathStr, "wb");
#endif // WIN_PLATFORM
            ASFileSysReleasePlatformPath(&altFSRec, platformPath);
            if (nativeFile)
                fwrite(altFile->Buffer, 1, (size_t)altFile->CurrentSize, nativeFile);
            fclose(nativeFile);
        } /* If file did not exist and cannot be created, do not save it */
    }

    ASTextDestroy(emptyText);

    return 0;
}

ASMDFile altFSReopen(ASMDFile f, ASFileMode newMode, ASErrorCode *error) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)f;

    if (!f)
    {
        return f;
    }

    altFileHandle->Position = 0;
    altFileHandle->Flags |= newMode;

    *error = 0;

    return (f);
}

ASErrorCode altFSFlush(ASMDFile f) { return 0; }

ASErrorCode altFSHardFlush(ASMDFile f) { return 0; }

ASErrorCode altFSSetPos64(MDFile File, ASFilePos64 Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!File)
        return (1);

    altFile = altFileHandle->File;

    altFSExpandBuffer(altFile, (size_t)Pos);
    if (Pos > altFile->CurrentSize) {
        memset(altFile->Buffer + altFile->CurrentSize, 0, (size_t)(Pos - altFile->CurrentSize));
        altFile->CurrentSize = Pos;
    }
    altFileHandle->Position = Pos;

    return 0;
}

// Set the cursor to this file at a particular position
ASErrorCode altFSSetPos(MDFile File, ASUns32 Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!File)
        return (1);

    altFile = altFileHandle->File;

    altFSExpandBuffer(altFile, Pos);
    if (Pos > altFile->CurrentSize) {
        memset(altFile->Buffer + altFile->CurrentSize, 0, (size_t)(Pos - altFile->CurrentSize));
        altFile->CurrentSize = Pos;
    }
    altFileHandle->Position = Pos;

    return 0;
}

// Get the current position of the cursor to this file
ASErrorCode altFSGetPos64(MDFile File, ASFilePos64 *Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;

    if (!File)
        return 1;

    *Pos = altFileHandle->Position;

    return 0;
}

// Get the current position of the cursor to this file
ASErrorCode altFSGetPos(MDFile File, ASUns32 *Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;

    if (!File)
        return 1;

    *Pos = altFileHandle->Position;

    return 0;
}

// Set the EOF marker at a particular position in the file
ASErrorCode altFSSetEOF64(MDFile File, ASFilePos64 Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;

    if (Pos > altFile->CurrentSize)
        altFSSetPos64(File, Pos);
    else {
        /* Truncate file to specified size */
        altFile->CurrentSize = Pos;
        altFile->BufferSize = Pos;
        altFile->Buffer = (char *)ASrealloc(altFile->Buffer, (size_t)Pos);
        if (altFileHandle->Position > altFile->CurrentSize)
            altFileHandle->Position = altFile->CurrentSize;
    }

    return 0;
}

// Set the EOF marker at a particular position in the file
ASErrorCode altFSSetEOF(MDFile File, ASUns32 Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;

    if (Pos > altFile->CurrentSize)
        altFSSetPos(File, Pos);
    else {
        /* Truncate file to specified size */
        altFile->CurrentSize = Pos;
        altFile->BufferSize = Pos;
        altFile->Buffer = (char *)ASrealloc(altFile->Buffer, Pos);
        if (altFileHandle->Position > altFile->CurrentSize)
            altFileHandle->Position = altFile->CurrentSize;
    }

    return 0;
}

// Get the position (offset) from the beginning of the file to the EOF marker.
ASErrorCode altFSGetEOF64(MDFile File, ASFilePos64 *Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;
    *Pos = altFile->CurrentSize;
    return 0;
}

// Get the position (offset) from the beginning of the file to the EOF marker.
ASErrorCode altFSGetEOF(MDFile File, ASUns32 *Pos) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;
    *Pos = altFile->CurrentSize;
    return 0;
}

ASBool altFSCanSetEof(ASMDFile File, ASFilePos Pos) {

    if (!File)
        return false;

    return true;
}

// Similar to fRead, for the Alternate FileSystem
ASSize_t altFSRead(void *Buffer, ASSize_t Size, ASSize_t Count, MDFile File, ASInt32 *Error) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;
    ASSize_t Recs;

    if (!File || !(altFileHandle->Flags & ASFILE_READ)) {
        *Error = 11;
        return 0;
    }
    altFile = altFileHandle->File;

    if (altFileHandle->Position > altFile->CurrentSize) {
        // End of file
        *Error = 12;
        return 0;
    }

    Recs = Size == 0 ? 0 : (size_t)(altFile->CurrentSize - altFileHandle->Position) / Size;
    if (Recs > Count)
        Recs = Count;

    if (Recs) {
        ASUns32 amount = Recs * Size;

        memcpy(Buffer, altFile->Buffer + altFileHandle->Position, amount);
        altFileHandle->Position += amount;
    }

    *Error = 0;
    return Recs;
}

// Similar to fWrite, for the Alternate FileSystem
ASSize_t altFSWrite(void *Buffer, ASSize_t Size, ASSize_t Count, MDFile File, ASInt32 *Error) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;
    ASSize_t Write = 0;

    if (!File || !(altFileHandle->Flags & ASFILE_WRITE) || !(altFileHandle->File)) {
        *Error = 13;
        return 0;
    }

    altFile = altFileHandle->File;
    Write = Count * Size;

    altFSExpandBuffer(altFile, (size_t)(altFileHandle->Position + Write));

    memcpy(&altFile->Buffer[altFileHandle->Position], Buffer, Write);
    altFileHandle->Position += Write;
    if (altFileHandle->Position > altFile->CurrentSize)
        altFile->CurrentSize = altFileHandle->Position;

    *Error = 0;
    return Write;
}

// Rename a file in the Alternate File System
ASInt32 altFSRename(MDFile *File, ASPathName Old, ASPathName New) {
    altFSFileHandle *altFileHandle = (altFSFileHandle *)File;
    altFSFile *altFile;

    if (!ASTextCmp((ASText)Old, (ASText)New))
        return 1;

    if (File)
        altFile = altFileHandle->File;
    else
        altFile = findAltFSFile(Old);

    if (!altFile)
        return (1);

    ASTextDestroy(altFile->Name);
    altFile->Name = ASTextDup((ASText)New);

    return 0;
}

/* See if two paths name the same file */
ASBool altFSIsSameFile(MDFile File, ASPathName P1, ASPathName P2) {
    if (!ASTextCmp((ASText)P1, (ASText)P2))
        return TRUE;

    return FALSE;
}

/* Get the PDEncoded name from an ASPath Name*/
ASInt32 altFSGetName(ASPathName Path, char *Name, ASInt32 Max) {
    ASTArraySize length;
    char *copy = ASTextGetPDTextCopy((ASText)Path, &length);

    if (length < Max)
        memmove(Name, copy, length);
    else
        memmove(Name, copy, Max);

    return length < Max ? length : Max;
}

/* Generate a temporary file name, using the user provided 
** temporary file path name, or the user supplied sibling path
*/
ASPathName altFSGetTempPathName(ASPathName Path) {
    static int tmpFCounter = 0;

    wchar_t workPath[2048];
    workPath[0] = 0;
    workPath[1] = 0;
    workPath[2] = 0;
    workPath[3] = 0;
    wchar_t pathSep = L'/'; /* Because the path we are working with is a DI path, the separator is ALWAYS a forward slash! */

    /* If a sibling path is defined, append the generated name to it*/
    ASPathName tempPath = NULL;
    if (Path)
        tempPath = Path;

    /* Otherwise, If the user has set a default path for temp files for this
    ** file system, retrieve it and append the file name to it
    */
    else
        tempPath = ASFileSysGetDefaultTempPath(&altFSRec);

    ASUnicodeFormat format = kUTF16HostEndian;
    if (sizeof(wchar_t) == 4)
        format = kUTF32HostEndian;

    if (tempPath) {
        wchar_t *pathString;
        pathString = (wchar_t *)ASTextGetUnicodeCopy((ASText)tempPath, format);

        wcscat(workPath, pathString);
        if (workPath[wcslen(workPath) - 1] != pathSep) {
            workPath[wcslen(workPath) + 1] = 0;
            workPath[wcslen(workPath)] = pathSep;
        }

        ASfree(pathString);
    }
    wchar_t actualPath[2048];

#ifdef WIN_ENV
    _swprintf(actualPath, L"%sTmpFile%d", workPath, tmpFCounter++);
#else
    swprintf(actualPath, sizeof(workPath), L"%lsTmpFile%d", workPath, tmpFCounter++);
#endif

    ASText outPath = ASTextFromUnicode((ASUTF16Val *)actualPath, format);

    return (ASPathName)outPath;
}

/* Create a copy of the ASPAthName */
ASPathName altFSCopyPathName(ASPathName Path) {

    if (!Path)
        return NULL;

    ASText TempPath = ASTextDup((ASText)Path);
    return (ASPathName)(TempPath);
}

/* Create a PDDocEncoded DI Path for the ASPathName */
char *altFSDIPathFromPath(ASPathName Path, ASPathName Relative) {

    /* Use the native file system to convert the name to a DI Path,combining with the sibling path */
    ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, Path, nativeFileSys);
    ASPathName nativeRelative = NULL;
    if (Relative != NULL)
        nativeRelative = ASFileSysAcquireFileSysPath(&altFSRec, Relative, nativeFileSys);
    char *nativeDIPath = ASFileSysDIPathFromPath(nativeFileSys, nativePath, nativeRelative);

    ASFileSysReleasePath(nativeFileSys, nativePath);
    ASFileSysReleasePath(nativeFileSys, nativeRelative);

    return (nativeDIPath);
}

/* Create an ASText of the DIPath of the specified ASPathName*/
ASErrorCode altFSDIPathFromPathEx(ASPathName path, ASPathName relativeToThisPath, ASText diPathText) {

    /* Use the native file system to convert the name to a DI Path, combining with the sibling path */
    ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, path, nativeFileSys);
    ASPathName nativeRelative = ASFileSysAcquireFileSysPath(&altFSRec, relativeToThisPath, nativeFileSys);
    ASErrorCode error = ASFileSysDIPathFromPathEx(nativeFileSys, nativePath, nativeRelative, diPathText);

    ASFileSysReleasePath(nativeFileSys, nativePath);
    ASFileSysReleasePath(nativeFileSys, nativeRelative);

    return (error);
}

/* Create an ASPathName from the PDDocEncoded DI Path */
ASPathName altFSPathFromDIPath(const char *Path, ASPathName Relative) {

    ASPathName newPath = ASFileSysCreatePathName(&altFSRec, ASAtomFromString("DIPath"), Path, Relative);

    return newPath;
}

/* Create an ASPathName from the ASText DI Path */
ASPathName altFSPathFromDIPathEx(ASConstText Path, ASPathName Relative) {

    ASPathName newPath =
        ASFileSysCreatePathName(&altFSRec, ASAtomFromString("DIPathWithASText"), Path, Relative);

    return newPath;
}

/* Release a path name that is no longer needed */
void altFSDisposePathName(ASPathName Path) { ASTextDestroy((ASText)Path); }

/* Return the name of this file system */
ASAtom altFSGetFileSysName(void) { return ASAtomFromString("DLSampleAltFS"); }

/* Return the amount of free space in this file system */
ASUns32 altFSGetStorageFreeSpace(ASPathName Path) { return ASUns32(-1); }
ASUns64 altFSGetStorageFreeSpace64(ASPathName Path) { return ASUns64(-1); }

/* Create a new ASPathName, using a specified type of data */
ASPathName altFSCreatePathName(ASAtom PathType, const void *Path, const void *MustBeZero) {

    /* The path to be created is fairly complex, depending on the pathType
    ** Rather than recreating all of the complexity of the handling of various types,
    ** use the native file system to build the path, then extract it's DI path and use that
    ** as this file system's ASPathName
    */
    ASPathName nativePath = ASFileSysCreatePathName(nativeFileSys, PathType, Path, MustBeZero);
    ASText diPath = ASTextNew();
    ASFileSysDIPathFromPathEx(nativeFileSys, nativePath, NULL, diPath);
    ASFileSysReleasePath(nativeFileSys, nativePath);

    return ASPathName(diPath);
}

/* Convert an ASPathName from this file system, into some other file system's ASPathName */
ASPathName altFSAcquireFileSysPath(ASPathName Path, ASFileSys Sys) {

    if (Path == NULL)
        return (NULL);

    /* The ASPathName input does NOT belong to this file system, but rather to a second file system
    ** Convert the name to a DIPath, and set that ASText as the new path in this file system
    */
    ASPathName newPath = ASFileSysCreatePathFromDIPathText(Sys, (ASText)Path, NULL);

    return (newPath);
}

/* Acquire the Platform Path Structure for this ASPathName */
ASInt32 altFSAcquirePlatformPath(ASPathName path, ASAtom platformPathType, ASPlatformPath *platformPath) {
    /* We do not want to duplicate the complexity of the path type logic here
    ** So again, we will use the native file system to do this conversion
    */
    ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, path, nativeFileSys);
    ASInt32 length = ASFileSysAcquirePlatformPath(nativeFileSys, nativePath, platformPathType, platformPath);
    ASFileSysReleasePath(nativeFileSys, nativePath);
    return (length);
}

/* Release the Platform Path Structure for this file name */
void altFSReleasePlatformPath(ASPlatformPath platformPath) {
    ASFileSysReleasePlatformPath(nativeFileSys, platformPath);
}

/* Get the name of the path as an ASText object */
ASErrorCode altFSGetNameASASText(ASPathName pathName, ASText name) {
    name = (ASText)pathName;
    return (0);
}

/* Get the display name of the path as an ASText object */
ASErrorCode altFSGetNameForDisplay(ASPathName path, ASText displayText) {
    ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, path, nativeFileSys);
    ASErrorCode result = ASFileSysGetNameFromPathForDisplay(nativeFileSys, nativePath, displayText);
    ASFileSysReleasePath(nativeFileSys, nativePath);
    return (result);
}

/* Get the URL Of the text as a simple string */
char *altFSurlFromPath(ASPathName path) {
    ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, path, nativeFileSys);
    char *url = ASFileSysURLFromPath(nativeFileSys, nativePath);
    ASFileSysReleasePath(nativeFileSys, nativePath);
    return (url);
}

/* Get the display name of the ASPathName as a simple string */
char *altFSDisplayStringFromPath(ASPathName path) {
    ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, path, nativeFileSys);
    char *display = ASFileSysDisplayStringFromPath(nativeFileSys, nativePath);
    ASFileSysReleasePath(nativeFileSys, nativePath);

    return (display);
}

/* Get the display name of the ASPathName as an ASText object */
ASErrorCode altFSDisplayASTextFromPath(ASPathName path, ASText displayText) {
    ASPathName nativePath = ASFileSysAcquireFileSysPath(&altFSRec, path, nativeFileSys);
    ASErrorCode error = ASFileSysDisplayASTextFromPath(nativeFileSys, nativePath, displayText);
    ASFileSysReleasePath(nativeFileSys, nativePath);

    return (error);
}


// Set up the structure of function calls to the Alternate File System
int defineAltFileSys() {
    if (!altFSRecDefined) {
        memset(&altFSRec, 0, sizeof(ASFileSysRec));

        /* This is the complete ASFileSysRec, as of 11/3/2020
        ** Entries set to NULL are entries that are not needed for
        ** this specific file system.
        **
        ** These are re-ordered from the actual definition to show logical
        ** groupings of interfaces
        **
        ** Note that new entries may be added at any time,
        ** and should be evaluated for addition to this sample
        ** as they occur
        */

        altFSRec.size = sizeof(ASFileSysRec);

        altFSRec.getFileSysName = altFSGetFileSysName;

        altFSRec.open = altFSOpen;
        altFSRec.fsRec_open64 = altFSOpen;
        altFSRec.close = altFSClose;
        altFSRec.reopen = altFSReopen;
        altFSRec.flush = altFSFlush;
        altFSRec.hardFlush = altFSHardFlush;
        altFSRec.setpos = altFSSetPos;
        altFSRec.fsRec_setpos64 = altFSSetPos64;
        altFSRec.getpos = altFSGetPos;
        altFSRec.fsRec_getpos64 = altFSGetPos64;
        altFSRec.seteof = altFSSetEOF;
        altFSRec.fsRec_seteof64 = altFSSetEOF64;
        altFSRec.geteof = altFSGetEOF;
        altFSRec.fsRec_geteof64 = altFSGetEOF64;
        altFSRec.read = altFSRead;
        altFSRec.write = altFSWrite;
        altFSRec.setFileMode = NULL;
        altFSRec.remove = altFSRemove;
        altFSRec.rename = altFSRename;

        altFSRec.isSameFile = altFSIsSameFile;
        altFSRec.getName = altFSGetName;
        altFSRec.getfileposlimit = NULL;

        altFSRec.createPathName = altFSCreatePathName;
        altFSRec.getTempPathName = altFSGetTempPathName;
        altFSRec.copyPathName = altFSCopyPathName;
        altFSRec.diPathFromPath = altFSDIPathFromPath;
        altFSRec.pathFromDIPath = altFSPathFromDIPath;
        altFSRec.diPathFromPathEx = altFSDIPathFromPathEx;
        altFSRec.pathFromDIPathEx = altFSPathFromDIPathEx;
        altFSRec.disposePathName = altFSDisposePathName;
        altFSRec.acquireFileSysPath = altFSAcquireFileSysPath;
        altFSRec.urlFromPath = altFSurlFromPath;
        altFSRec.displayStringFromPath = altFSDisplayStringFromPath;
        altFSRec.acquirePlatformPath = altFSAcquirePlatformPath;
        altFSRec.releasePlatformPath = altFSReleasePlatformPath;
        altFSRec.getNameAsASText = altFSGetNameASASText;
        altFSRec.displayASTextFromPath = altFSDisplayASTextFromPath;
        altFSRec.getNameForDisplay = altFSGetNameForDisplay;

        altFSRec.getStorageFreeSpace = altFSGetStorageFreeSpace;
        altFSRec.fsRec_getStorageFreeSpace64 = altFSGetStorageFreeSpace64;

        altFSRec.flushVolume = NULL;
        altFSRec.getFileFlags = NULL;

        altFSRec.readAsync = NULL;
        altFSRec.writeAsync = NULL;
        altFSRec.abortAsync = NULL;
        altFSRec.yield = NULL;
        altFSRec.mreadRequest = NULL;
        altFSRec.getStatus = NULL;
        altFSRec.clearOutstandingMReads = NULL;
        altFSRec.rangeArrived = NULL;

        altFSRec.getItemProps = NULL;
        altFSRec.getItemPropsAsCab = NULL;
        altFSRec.canPerformOpOnItem = NULL;
        altFSRec.performOpOnItem = NULL;
        altFSRec.firstFolderItem = NULL;
        altFSRec.nextFolderItem = NULL;
        altFSRec.destroyFolderIterator = NULL;
        altFSRec.getParent = NULL;
        altFSRec.createFolder = NULL;
        altFSRec.removeFolder = NULL;

        altFSRec.setTypeAndCreator = NULL;
        altFSRec.getTypeAndCreator = NULL;
        altFSRec.getPlatformThing = NULL;

        altFSRec.canSetEof = altFSCanSetEof;
        altFSRec.isInUse = NULL;

        /* Save the orignal "native" file system */
        nativeFileSys = ASGetDefaultFileSys();
    }
    altFSRecDefined = TRUE;

    return (TRUE);
}

ASFileSys altFileSys() {
    defineAltFileSys();
    return (&altFSRec);
}
