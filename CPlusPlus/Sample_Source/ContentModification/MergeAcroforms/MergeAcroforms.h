//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This is a sample program written by Datalogics Inc. to demonstrate
// how to use the Adobe PDF Library to move all of the Acroforms from a given
// document to a new document. In this example, we will create the new document
// as raster image pages of the existing document.
//

typedef struct COPYCONTROL {
    CosObj Field;
    CosDoc Doc;
    CosObj *InPageTable;
    CosObj *OutPageTable;
    ASInt32 Pages;
    COPYCONTROL(CosObj f, CosDoc d, CosObj *it, CosObj *ot, ASInt32 p)
        : Field(f), Doc(d), InPageTable(it), OutPageTable(ot), Pages(p) {}
} CopyControl;

int CopyPageToBitmap(PDPage InPage, PDPage OutPage);
int CopyAcroforms(PDDoc InDoc, PDDoc OutDoc);
CosObj CopyField(CosDoc InDoc, CosDoc OutDoc, CosObj Field, CosObj Parent, ASInt32 PageCount,
                 CosObj *InPageTable, CosObj *OutPageTable);
ASBool FieldDictCopy(CosObj KeyObj, CosObj Value, void *ClientData);
