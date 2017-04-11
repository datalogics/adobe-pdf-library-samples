// Copyright (c) 2015-2016, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
//===============================================================================
//Sample: APDFLDoc -This class is intended to assist with operations common to 
//most samples. The class is capable of opening/creating and saving a document.
//It can also insert and retrieve pages.
//
//APDFLDoc.cpp: Contains implementations of methods.
//APDFLDoc.h: Contains class definition.
//===============================================================================

#ifndef APDFLDOC_H
#define APDFLDOC_H

#include "PDCalls.h"
#include "ASCalls.h"
#include "ASExtraCalls.h"

#include <iostream>
#include <vector>

class APDFLDoc {

private:

    static const unsigned MAX_PATH_LENGTH = 1024;                          //Private data members assosciated with the document.
    wchar_t nameOfDocument[MAX_PATH_LENGTH];

    volatile ASPathName asPathName;
    ASErrorCode errorCode;

    void initialize();                                                     //Called in constructor to initialize some data members.
    ASErrorCode printErrorHandlerMessage();                                //Prints an error message and returns the appropriate error code.
    ASErrorCode setASPathName(wchar_t* );                                  //Helper method used to create ASPathName objects for operations.
    void CommonConstruct(wchar_t*,bool);                                   //Helper to allow char* overload of constructor with minimal code copying

    APDFLDoc(const APDFLDoc&);                                             //Do not allow copy constructor or assignment operator to be used.
    APDFLDoc& operator=(const APDFLDoc&);                                  //in order to prevent shallow copies of objects.

public:

    volatile PDDoc pdDoc;                                                  //Made public so it can be accessed directly.

    APDFLDoc(wchar_t*, bool doRepairDamagedFile);                          //Constructor used to open a document.
    APDFLDoc(const char*, bool doRepairDamagedFile);                       //Constructor used to open a document.
    APDFLDoc();                                                            //Constructor used to create a document.

    ASSize_t numPages () { return (PDDocGetNumPages (pdDoc)); }

    ASErrorCode insertPage(const ASFixed & width, const ASFixed & height, ASInt32);  //Inserts a page into the document.
    ASErrorCode insertPage(const ASInt16 & width, const ASInt16 & height, ASInt32);  //Inserts a page into the document.
    PDPage getPage(ASInt32);                                                         //Returns the specified PDPage, the first page is 0.

    volatile PDDoc& getPDDoc(){ return pdDoc; };                           //Returns a reference to the PDDoc that was created or opened.

    ASErrorCode saveDoc(wchar_t* = NULL, PDSaveFlags = PDSaveFull);        //Used to save the document, may be provided a path and PDSaveFlags.
    ASErrorCode saveDoc(const char*, PDSaveFlags = PDSaveFull);            //Used to save the document to a specified non-wide string, may be provided PDSaveFlags.
    
    ~APDFLDoc();                                                           //Destructor frees up resources.

    static ASPathName makePath(const char* path );                         //Provide functionality for device independent path construction
    static ASPathName makePath(const wchar_t* path );
    static ASFile OpenFlatFile ( const char* path, int mode = ASFILE_READ );   // Generic file open

    static ASUnicodeFormat GetHostUnicodeFormat() { return ( sizeof(wchar_t) == 2 ? kUTF16HostEndian : kUTF32HostEndian ); }
};

#endif
