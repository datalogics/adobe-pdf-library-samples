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

#include "ASCalls.h"

#include <cstdio>
#include <cstring>

using namespace std;

#ifdef WIN32
#pragma warning(disable:4267)
#endif

struct _altFSFile
{
    char             *Name;          // When present, the i/o file system file name reflected here
    unsigned char    *Buffer;        // Contents of this file
    ASSize_t         BufferSize;     // Maximum size of this file without expand
    ASSize_t         CurrentSize;    // Current size of this file
    struct _altFSFile  *Prev, *Next; 
};
typedef struct _altFSFile altFSFile;

// The altFSFileHandle is what is returned as an MDFile
struct _altFSFileHandle
{
    struct _altFSFile    *File;        // Pointer to common file info
    ASSize_t             Position;     // Last byte read or written
    ASUns16              Flags;        // Open flags
};
typedef struct _altFSFileHandle altFSFileHandle;

static ASFileSysRec      altFSRec;
static ASBool            altFSRecDefined = FALSE;
static altFSFile        *altFileRoot = NULL;

const char *MFSPathToCString(ASPathName inASPath)
{
    char *inSpec = (char *) inASPath;

    if ((!inSpec) || (!inSpec[0]) )
        return NULL;

    return (const char *) inASPath;
}

// Increase size of buffer by a factor of two.
static void altFSExpandBuffer (altFSFile *File, ASSize_t Size)
{
    ASSize_t TargetSize;

    if ((!File) || (!Size) || (Size < File->BufferSize))
        return;

    TargetSize = (Size > File->BufferSize * 2) ? Size : File->BufferSize * 2;
    if (TargetSize < 1024)        // Absolute minimum of 1k per file expansion
        TargetSize = 1024;

    if (!File->Buffer)
        File->Buffer = (unsigned char*) ASmalloc (TargetSize);
    else
        File->Buffer = (unsigned char*) ASrealloc (File->Buffer, TargetSize);
    File->BufferSize = TargetSize;
    if (!File->Buffer)
        ASRaise (genErrNoMemory);

    return;
}

// Find an element in the list
altFSFile *findAltFSFile (ASPathName Path)
{
    altFSFile *altFile;

    if (!MFSPathToCString(Path))
        return 0;

    for (altFile = altFileRoot; altFile; altFile = altFile->Next)
    {
        if (!strcmp (altFile->Name, MFSPathToCString(Path)))
            break;
    }
    return altFile;
}

// Remove an element from the list
ASInt32 altFSRemove (ASPathName Path)
{
    altFSFile *altFile;

    altFile = findAltFSFile (Path);
    if (altFile->Name)
        ASfree (altFile->Name);
    ASfree (altFile->Buffer);

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
ASInt32 altFSOpen (ASPathName Path, ASUns16 Flags, MDFile *File)
{
    altFSFileHandle    *altFileHandle;
    altFSFile          *altFile;
    const char *inCPath;

    // If the user asks for no file open mode, do not provide a file.
    if (Flags == 0)
        return 1;

    altFile = findAltFSFile (Path);
    if (altFile)
    {
        altFileHandle = (altFSFileHandle *) ASmalloc(sizeof(altFSFileHandle));
        if (!altFileHandle)
            ASRaise(genErrNoMemory);
        altFileHandle->Position = 0;
        altFileHandle->Flags = Flags;
        altFileHandle->File = altFile;

        *File = (MDFile *) altFileHandle;
        return 0;
    }

    altFile = (altFSFile *) ASmalloc (sizeof (altFSFile));
    altFileHandle = (altFSFileHandle *) ASmalloc(sizeof(altFSFileHandle));
    if (!altFile || !altFileHandle)
        ASRaise (genErrNoMemory);

    memset(altFile, 0, sizeof(altFSFile));
    altFile->Next = altFileRoot;
    altFileRoot = altFile;
    if (altFile->Next)
        altFile->Next->Prev = altFile;

    altFileHandle->Flags = Flags;
    altFileHandle->File = altFile;
    altFileHandle->Position = 0;

    if ( (inCPath = MFSPathToCString(Path)) )
    {
        altFile->Name = (char *) ASmalloc (sizeof(char) * (strlen (inCPath)+1));
        if (!altFile->Name)
            ASRaise (genErrNoMemory);
        strcpy (altFile->Name, inCPath);
    }

    // If a file in read mode, and not to be created, with no data
    // currently associated with the file, the program reads the
    // corresponding file on the disk as the initial data. If the file
    // is only opening in write mode, don't bother reading it.
    if ((altFile->Name) && (altFile->Name[0]) &&
        !(altFile->CurrentSize) && !(Flags & ASFILE_CREATE) )
    {
        FILE    *Initial;
        size_t  Position;
        char    *openPath = altFile->Name;

        Initial = fopen (openPath, "rb");

        if (Initial) 
        {
            if (Flags & ASFILE_READ)
            {
                fseek (Initial, 0, SEEK_END);
                Position = ftell (Initial);

                if (Position)
                {
                    fseek (Initial, 0, SEEK_SET);
                    altFile->Buffer = (unsigned char*) ASmalloc (Position);
                    if (!altFile->Buffer)
                    {
                        fclose (Initial);
                        ASfree(altFileHandle);
                        ASRaise (genErrNoMemory);
                    }
                    altFile->BufferSize = (ASSize_t)Position;
                    altFile->CurrentSize =
                        fread (altFile->Buffer, 1, altFile->BufferSize, Initial);
                }
                fclose (Initial);
            }
        }
        // If the system is in read, no-create mode, and no physical file is provided, the file does not exist.
        else
        {
            ASfree(altFileHandle);
            altFSRemove(Path);
            return 1;
        }
    }

    *File = (MDFile *) altFileHandle;

    return 0;
}

// Mark the file on the file system as closed
ASInt32 altFSClose (MDFile File)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile            *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;

    if ((altFile->Name) && (altFile->Name[0]) &&
        (altFileHandle->Flags & ASFILE_WRITE))
    {
        // Write this to a real file system, if a path name is given
        FILE    *Final;

        Final = fopen (altFile->Name, "wb");
        if (Final)
        {
            if (altFile->Buffer)
                fwrite (altFile->Buffer, altFile->CurrentSize, 1, Final);
            else
                fwrite (" ", 0, 0, Final);

            fclose (Final);
        }
    }
    return 0;
}

ASInt32 altFSFlush (MDFile File)
{
    // There is nothing to do for a flush operation
    return 0;
}

ASInt32 altFSSetPos64(MDFile File, ASFilePos64 Pos)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile            *altFile;

    if (!File)
        return (1);

    altFile = altFileHandle->File;

    altFSExpandBuffer (altFile, Pos);
    if (Pos > altFile->CurrentSize)
    {
        memset(altFile->Buffer + altFile->CurrentSize, 0, Pos - altFile->CurrentSize);
        altFile->CurrentSize = Pos;
    }
    altFileHandle->Position = Pos;

    return 0;
}

// Set the cursor to this file at a particular position
ASInt32    altFSSetPos (MDFile File, ASUns32 Pos)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile            *altFile;

    if (!File)
        return (1);

    altFile = altFileHandle->File;

    altFSExpandBuffer (altFile, Pos);
    if (Pos > altFile->CurrentSize)
    {
        memset(altFile->Buffer + altFile->CurrentSize, 0, Pos - altFile->CurrentSize);
        altFile->CurrentSize = Pos;
    }
    altFileHandle->Position = Pos;

    return 0;
}

// Get the current position of the cursor to this file
ASInt32    altFSGetPos (MDFile File, ASUns32 *Pos)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;

    if (!File)
        return 1;

    if (altFileHandle->Position >= 0)
        *Pos = (ASInt32) altFileHandle->Position;

    return 0;
}

// Set the EOF marker at a particular position in the file
ASInt32 altFSSetEOF (MDFile File, ASUns32 Pos)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile          *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;

    if (Pos > altFile->CurrentSize)
        altFSSetPos (File, Pos);
    else
        altFile->CurrentSize = Pos;

    return 0;
}

// Get the position (offset) from the beginning of the file of the EOF marker.
ASInt32 altFSGetEOF (MDFile File, ASUns32 *Pos)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile            *altFile;

    if (!File)
        return 1;

    altFile = altFileHandle->File;
    *Pos = altFile->CurrentSize;
    return 0;
}

// Similar to fRead, for the Alternate FileSystem
ASSize_t altFSRead (void *Buffer, ASSize_t Size, ASSize_t Count, MDFile File, ASInt32 *Error)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile          *altFile;
    ASSize_t            Recs;

    if (!File || !(altFileHandle->Flags & ASFILE_READ))
    {
        *Error = 11;
        return 0;
    }
    altFile = altFileHandle->File;

    if (altFileHandle->Position > altFile->CurrentSize || Count < 0 || Size < 0)
    {
        // End of file
        *Error = 12;
        return 0;
    }

    Recs = Size == 0 ? 0 : (altFile->CurrentSize - altFileHandle->Position) / Size;
    if (Recs > Count)
        Recs = Count;

    if (Recs)
    {
        ASUns32 amount = Recs * Size;

        memcpy(Buffer, altFile->Buffer + altFileHandle->Position, amount);
        altFileHandle->Position += amount;
    }

    *Error = 0;
    return Recs;
}

// Similar to fWrite, for the Alternate FileSystem
ASSize_t altFSWrite (void *Buffer, ASSize_t Size, ASSize_t Count, MDFile File, ASInt32 *Error)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile          *altFile;
    ASSize_t            Write = 0;

    if (!File || !(altFileHandle->Flags & ASFILE_WRITE) || !(altFileHandle->File) )
    {
        *Error = 13;
        return 0;
    }

    altFile = altFileHandle->File;
    Write = Count * Size;

    altFSExpandBuffer (altFile, altFileHandle->Position + Write);

    memcpy (&altFile->Buffer[altFileHandle->Position], Buffer, Write);
    altFileHandle->Position += Write;
    if (altFileHandle->Position > altFile->CurrentSize)
        altFile->CurrentSize = altFileHandle->Position;

    *Error = 0;
    return Write;
}

// Rename a file in the Alternate File System
ASInt32 altFSRename (MDFile *File, ASPathName Old, ASPathName New)
{
    altFSFileHandle    *altFileHandle = (altFSFileHandle *) File;
    altFSFile            *altFile;

    const char       *inCPath;

    if (File)
        altFile = altFileHandle->File;
    else
        altFile = findAltFSFile (Old);

    if (!altFile || !(inCPath = MFSPathToCString(New)) )
        return 1;

    altFile->Name = (char*) ASrealloc (altFile->Name, sizeof(char) *
        (strlen (inCPath) + 1));
    if (!altFile->Name)
        ASRaise (genErrNoMemory);

    strcpy (altFile->Name, inCPath);

    return 0;
}

ASBool altFSIsSameFile  (MDFile File, ASPathName P1, ASPathName P2)
{
    if (!strcmp((char *) P1, (char *) P2))
        return TRUE;

    return FALSE;
}

ASInt32 altFSGetName (ASPathName Path, char *Name, ASInt32 Max)
{
    if (Name && Max > (ASInt32) strlen((char *) Path) )
        strcpy(Name, (char *) Path);
    else
        strncpy(Name, (char *) Path, Max);
    return 0;
}

ASPathName altFSGetTempPathName (ASPathName Path)
{
    static int tmpFCounter = 0;

    char *outPath = (char *) ASmalloc(24); // Enough for the prefix + counter
    if (!outPath)
        ASRaise(genErrNoMemory);
    sprintf(outPath, "TmpFile%d", tmpFCounter++);

    return (ASPathName) outPath;

}

ASPathName altFSCopyPathName (ASPathName Path)
{
    char    *TempPath;

    if (!Path)
        return NULL;

    TempPath = (char*) ASmalloc (sizeof(char) * (strlen ((const char*) Path)+1));
    if (!TempPath)
        ASRaise (genErrNoMemory);

    strcpy (TempPath, (const char*) Path);
    return (ASPathName)(TempPath);
}

char *altFSDIPath (ASPathName Path, ASPathName Relative)
{
    char    *TempPath;

    TempPath = (char*) ASmalloc (sizeof(char) * (strlen ((const char*) Path)+1));
    if (!TempPath)
        ASRaise (genErrNoMemory);

    strcpy (TempPath, (const char*) Path);
    return (TempPath);
}

ASPathName altFSToDIPath (const char *Path, ASPathName Relative)
{
    char    *TempPath;

    TempPath = (char*) ASmalloc (sizeof(char) * (strlen (Path)+1));
    if (!TempPath)
        ASRaise (genErrNoMemory);

    strcpy (TempPath, Path);

    return (ASPathName)(TempPath);
}

void altFSDisposePath (ASPathName Path)
{
    ASfree (Path);
}

ASAtom altFSGetFileSysName (void)
{
    return ASAtomFromString("altFS");
}

ASUns32 altFSFreeSpace (ASPathName Path)
{
    return ASUns32(-1);
}

ASInt32 altFSFlushVolume (ASPathName Path)
{
    return TRUE;
}

ASUns32 altFSGetFlags (MDFile File)
{
    return 0;
}

ASInt32 altFSReadSynch (ASIORequest ior)
{
    altFSSetPos (ior->mdFile, ior->offset);
    ior->totalBytesCompleted = altFSRead (ior->ptr, ior->count, 1, ior->mdFile, &ior->pError);
    (ior->IODoneProc)(ior);

    return 0;
}

ASInt32 altFSWriteAsynch (ASIORequest ior)
{
    altFSSetPos (ior->mdFile, ior->offset);
    ior->totalBytesCompleted = altFSWrite (ior->ptr, ior->count, 1, ior->mdFile, &ior->pError);
    (ior->IODoneProc)(ior);

    return 0;
}

void altFSAbortAsynch (MDFile File)
{
    return;
}

ASUns32 altFSStatus (MDFile File)
{
    if (File)
        return kASFileOkay;
    return kASFileIsTerminating;
}

ASPathName altFSCreatePathName (ASAtom PathType, const void *Path, const void *MustBeZero)
{
    char    *TempPath;

    TempPath = (char*) ASmalloc (sizeof(char) * (strlen ((const char*) Path)+1));
    if (!TempPath)
        ASRaise (genErrNoMemory);

    strcpy (TempPath, (const char*) Path);
    return ASPathName(TempPath);
}

ASPathName altFSAcquirePath (ASPathName Path, ASFileSys Sys)
{
    char *TempPath = (char*)ASmalloc (sizeof(char) * (strlen ((const char*) Path)+1));
    if (!TempPath)
        ASRaise (genErrNoMemory);

    strcpy (TempPath, (const char*) Path);
    return ASPathName(TempPath);
}

// Set up the structure of function calls to the Alternate File System
int defineAltFileSys ()
{
    if (!altFSRecDefined)
    {
        memset (&altFSRec, 0, sizeof (ASFileSysRec));

        altFSRec.size = sizeof (ASFileSysRec);
        altFSRec.open = altFSOpen;
        altFSRec.close = altFSClose;
        altFSRec.flush = altFSFlush;
        altFSRec.setpos = altFSSetPos;
        altFSRec.fsRec_setpos64 = altFSSetPos64;
        altFSRec.getpos = altFSGetPos;
        altFSRec.seteof = altFSSetEOF;
        altFSRec.geteof = altFSGetEOF;
        altFSRec.read = altFSRead;
        altFSRec.write = altFSWrite;
        altFSRec.remove = altFSRemove;
        altFSRec.rename = altFSRename;
        altFSRec.isSameFile = altFSIsSameFile;
        altFSRec.getName = altFSGetName;
        altFSRec.getTempPathName = altFSGetTempPathName;
        altFSRec.copyPathName = altFSCopyPathName;
        altFSRec.diPathFromPath = altFSDIPath;
        altFSRec.pathFromDIPath = altFSToDIPath;
        altFSRec.disposePathName = altFSDisposePath;
        altFSRec.getFileSysName = altFSGetFileSysName;
        altFSRec.getStorageFreeSpace = altFSFreeSpace;
        altFSRec.flushVolume = altFSFlushVolume;
        altFSRec.getFileFlags = altFSGetFlags;
        altFSRec.readAsync = altFSReadSynch;
        altFSRec.writeAsync = altFSWriteAsynch;
        altFSRec.abortAsync = altFSAbortAsynch;
        altFSRec.getStatus = altFSStatus;
        altFSRec.createPathName = altFSCreatePathName;
        altFSRec.acquireFileSysPath = altFSAcquirePath;
    }
    altFSRecDefined = TRUE;

    return (TRUE);
}

ASFileSys    altFileSys()
{
    defineAltFileSys();
    return (&altFSRec); 
}
