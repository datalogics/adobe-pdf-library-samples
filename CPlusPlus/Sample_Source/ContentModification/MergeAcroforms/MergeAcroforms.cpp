//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample program demonstrates how to use the Adobe PDF Library to move all of the AcroForm
// objects, namely forms fields and digital signatures, from one PDF document to another. AcroForm,
// or Acrobat Form, is the technology provided by Adobe Systems to build PDF forms documents.
//
// The MergeAcroforms sample program starts by creating, effectively, a facsimile of each page in
// the input PDF document. These pages are used to build the pages in the output PDF file, with each
// new page in the output file a rasterized image of the corresponding page found in the input file.
// Then, the sample fills in these copied pages in the output file by copying in to them the
// Acroform fields found in the input file. The end of the sample program copies the standard
// individual objects of the Acroforms dictionary array (such as “NeedAppearances” and “SigFlags”)
// from the input PDF document to the Acroforms dictionary in the output document.
//
#include <cstdlib>
#include "PEWCalls.h"
#include "PERCalls.h"
#include "CosCalls.h"
#include "PagePDECntCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "MergeAcroforms.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define DEF_INPUT "FSA_Fillable.pdf"
#define DEF_OUTPUT "MergeAcroforms-out.pdf"

int main(int argc, char *argv[]) {
    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_DIR DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Reading input file " << csInputFileName.c_str() << "; will write new file "
              << csOutputFileName.c_str() << std::endl;

    APDFLib libInit; // Initialize the Adobe PDF Library. The Library will terminate automatically when the scope is lost.

    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    ASPathName Path;
    PDDoc InDoc;
    DURING
        Path = APDFLDoc::makePath(csInputFileName.c_str());
        InDoc = PDDocOpen(Path, NULL, NULL, true);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER
    ASFileSysReleasePath(NULL, Path);

    PDDoc OutDoc;
    DURING
        OutDoc = PDDocCreate();
    HANDLER
        std::cout << "Unable to Create Document " << csOutputFileName.c_str() << std::endl;
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    ASInt32 PageNumber;
    for (PageNumber = 0; PageNumber < PDDocGetNumPages(InDoc); PageNumber++) {
        PDPage InPage, OutPage;
        ASFixedRect PageSize;
        ASInt32 InsertBefore;

        DURING
            // Acquire the input document page, and the page size
            InPage = PDDocAcquirePage(InDoc, PageNumber);
            PDPageGetMediaBox(InPage, &PageSize);

            // Create a page of the same size in the output document
            if (PageNumber > 0)
                InsertBefore = PageNumber - 1;
            else
                InsertBefore = PDBeforeFirstPage;
            OutPage = PDDocCreatePage(OutDoc, InsertBefore, PageSize);

            CopyPageToBitmap(InPage, OutPage);

            // Release input and output page
            PDPageRelease(InPage);
            PDPageRelease(OutPage);
        HANDLER
            char ErrorBuffer[512];
            ASGetErrorString(ERRORCODE, ErrorBuffer, sizeof(ErrorBuffer));
            std::cout << "Error copying page " << PageNumber + 1 << " error " << ERRORCODE << ":  "
                      << ErrorBuffer << ": Page will be blank on output document\n";
        END_HANDLER
    }

    DURING
        CopyAcroforms(InDoc, OutDoc);
    HANDLER
        char ErrorBuffer[512];
        ASGetErrorString(ERRORCODE, ErrorBuffer, sizeof(ErrorBuffer));
        ;
        std::cout << "Error copying Acroforms. Error " << ERRORCODE << ": " << ErrorBuffer << std::endl
                  << "Acrobat Forms will be missing from the output.\n";
    END_HANDLER

    DURING
        Path = APDFLDoc::makePath(csOutputFileName.c_str());
        PDDocSave(OutDoc, PDSaveFull | PDSaveCollectGarbage | PDSaveBinaryOK, Path, NULL, NULL, NULL);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    ASFileSysReleasePath(NULL, Path);
    PDDocClose(OutDoc);
    PDDocClose(InDoc);

    return 0;
}

// This routine creates a bitmap representation of the input page at 300 DPI in RGB. It then
// makes an Image XObject of that bitmap, Flate compressed, and places this image in the output page.
int CopyPageToBitmap(PDPage InPage, PDPage OutPage) {
    ASFixedRect MediaBox;
    ASInt32 Width, Height;
    ASInt32 BPS, SPP, Resolution;
    ASInt32 BitMapSize;
    ASInt32 rowBytes;
    unsigned char *BitMap;
    ASFixedMatrix Matrix;
    ASFixedRect Dest;
    ASInt32 bitsPerPixel;
    PDEImageAttrs ImageAttrs;
    PDEImage PageImage;
    PDEContent OutContent;
    PDEColorSpace RGBColor;
    PDEFilterArray Filters;
    ASAtom ColorSpaceName;

    // Always RGB, 300 DPI
    ColorSpaceName = ASAtomFromString("DeviceRGB");
    BPS = 8;
    SPP = 3;
    Resolution = 300;

    PDPageGetMediaBox(InPage, &MediaBox);
    Width = ((MediaBox.right - MediaBox.left) & 0xFFFF0000) >> 16;
    Height = ((MediaBox.top - MediaBox.bottom) & 0xFFFF0000) >> 16;

    Width = (FloatToASFixed(Width * Resolution / 72.0) & 0xFFFF0000) >> 16;
    Height = (FloatToASFixed(Height * Resolution / 72.0) & 0xFFFF0000) >> 16;

    // Must be 32 bit aligned
    bitsPerPixel = BPS * SPP;
    rowBytes = (((Width * bitsPerPixel) + 31) >> 5) << 2;

    DURING
        BitMapSize = rowBytes * Height;
        BitMap = (unsigned char *)malloc(BitMapSize);

        if (Resolution != 72) {
            Matrix.a = FloatToASFixed(Resolution / 72.0);
            Matrix.d = FloatToASFixed(Resolution / -72.0);
        } else {
            Matrix.a = fixedOne;
            Matrix.d = -fixedOne;
        }

        Matrix.b = Matrix.c = 0;
        Matrix.v = Int32ToFixed(Height);
        Matrix.h = 0;

        Dest.bottom = Dest.left = 0;
        Dest.top = Height << 16;
        Dest.right = Width << 16;

        PDPageDrawContentsToMemory(InPage, kPDPageDoLazyErase, &Matrix, NULL, 0, ColorSpaceName, 8,
                                   &Dest, (char *)BitMap, BitMapSize, NULL, NULL);

    HANDLER
        free(BitMap);
        ASRaise(ERRORCODE);
    END_HANDLER

    // APDFL draws rows in even long (32 bit) words.
    //
    // Reading these rows back into a raw bit map generally does not work properly.
    // This section of code compresses the content back into a "solid" bit map.
    if ((Width * 3) != rowBytes) {
        ASInt32 RealWidth = Width * 3;
        ASInt32 Count;

        for (Count = 1; Count < Height; Count++) {
            unsigned char *Real, *Desired;

            Real = BitMap + (Count * rowBytes);
            Desired = BitMap + (Count * RealWidth);
            memmove(Desired, Real, RealWidth);
        }
        BitMapSize = Width * RealWidth;
    }

    memset((char *)&ImageAttrs, 0, sizeof(PDEImageAttrs));
    ImageAttrs.flags = kPDEImageExternal; // (Not really external, but an XObject)
    ImageAttrs.width = Width;
    ImageAttrs.height = Height;
    ImageAttrs.bitsPerComponent = BPS;
    ImageAttrs.decode[0] = ImageAttrs.decode[2] = ImageAttrs.decode[4] = 0;
    ImageAttrs.decode[1] = ImageAttrs.decode[3] = ImageAttrs.decode[5] = fixedOne;
    Matrix.a = FloatToASFixed(Width * 72.0 / 300);
    Matrix.d = FloatToASFixed(Height * 72.0 / 300);
    Matrix.b = Matrix.c = Matrix.h = Matrix.v = 0;
    Filters.numFilters = 1;
    Filters.spec[0].name = ASAtomFromString("FlateDecode");
    Filters.spec[0].decodeParms = CosNewNull();
    Filters.spec[0].encodeParms = CosNewNull();
    Filters.spec[0].padding = 0;

    DURING
        RGBColor = PDEColorSpaceCreateFromName(ColorSpaceName);
        PageImage = PDEImageCreate(&ImageAttrs, sizeof(PDEImageAttrs), &Matrix, 0, RGBColor, NULL,
                                   &Filters, NULL, BitMap, BitMapSize);
        OutContent = PDPageAcquirePDEContent(OutPage, 0);

        PDEContentAddElem(OutContent, kPDEBeforeFirst, (PDEElement)PageImage);

        PDPageSetPDEContent(OutPage, 0);
        PDPageReleasePDEContent(OutPage, 0);
        PDERelease((PDEObject)RGBColor);
        PDERelease((PDEObject)PageImage);
    HANDLER
        free(BitMap);
        ASRaise(ERRORCODE);
    END_HANDLER
    free(BitMap);

    return 0;
}

ASBool FieldDictCopy(CosObj KeyObj, CosObj Value, void *ClientData) {
    CopyControl *Control = (CopyControl *)ClientData;
    ASAtom Key = CosNameValue(KeyObj);

    // Parentage will be established new
    if (Key == ASAtomFromString("Parent"))
        return (TRUE);

    // Kids will be copied in field routine
    if (Key == ASAtomFromString("Kids"))
        return (TRUE);

    // Transliterate the page reference
    if (Key == ASAtomFromString("P")) {
        ASInt32 Page;
        for (Page = 0; ((!CosObjEqual(Control->InPageTable[Page], Value)) && (Page < Control->Pages)); Page++) {
        }; // Deliberate no action

        if (Page < Control->Pages) {
            CosObj PageObj = Control->OutPageTable[Page];
            CosObj Annot = CosDictGet(PageObj, ASAtomFromString("Annots"));

            // Put the page reference into the field
            CosDictPut(Control->Field, ASAtomFromString("P"), PageObj);

            // Put the field into the annotation array
            if (CosObjGetType(Annot) == CosNull) {
                Annot = CosNewArray(Control->Doc, TRUE, 3);
                CosDictPut(PageObj, ASAtomFromString("Annots"), Annot);
            }
            CosArrayPut(Annot, CosArrayLength(Annot), Control->Field);
        }
        return (TRUE);
    }

    // Any direct object can be copied directly
    if (!CosObjIsIndirect(Value)) {
        CosDictPut(Control->Field, Key, CosObjCopy(Value, Control->Doc, FALSE));
        return (TRUE);
    }

    // Copy all other indirect objects
    CosDictPut(Control->Field, Key, CosObjCopy(Value, Control->Doc, TRUE));
    return (TRUE);
}

// This routine copies one field from the source to the destination document.
CosObj CopyField(CosDoc InDoc, CosDoc OutDoc, CosObj Field, CosObj Parent, ASInt32 PageCount,
                 CosObj *InPageTable, CosObj *OutPageTable) {
    CosObj NewField;
    CosObj Kids;

    NewField = CosNewDict(OutDoc, TRUE, 10);

    CopyControl Control(NewField, OutDoc, InPageTable, OutPageTable, PageCount);
    CosObjEnum(Field, FieldDictCopy, &Control);

    if (CosObjGetType(Parent) != CosNull)
        CosDictPut(NewField, ASAtomFromString("Parent"), Parent);

    // If there are kids, copy then over
    Kids = CosDictGet(Field, ASAtomFromString("Kids"));
    if (CosObjGetType(Kids) != CosNull) {
        ASInt32 Count;
        CosObj NewKids = CosNewArray(OutDoc, FALSE, CosArrayLength(Kids));
        CosDictPut(NewField, ASAtomFromString("Kids"), NewKids);

        for (Count = 0; Count < CosArrayLength(Kids); Count++) {
            CosObj Kid = CosArrayGet(Kids, Count);
            CosObj NewKid = CopyField(InDoc, OutDoc, Kid, NewField, PageCount, InPageTable, OutPageTable);
            CosArrayPut(NewKids, Count, NewKid);
        }
    }

    return (NewField);
}

// This routine copies all of the fields required for Acroforms from
// the source document to the destination document.
int CopyAcroforms(PDDoc InDoc, PDDoc OutDoc) {
    CosDoc InCosDoc = PDDocGetCosDoc(InDoc);
    CosDoc OutCosDoc = PDDocGetCosDoc(OutDoc);
    CosObj InRoot = CosDocGetRoot(InCosDoc);
    CosObj OutRoot = CosDocGetRoot(OutCosDoc);
    CosObj InAcroForms = CosDictGet(InRoot, ASAtomFromString("AcroForm"));
    CosObj OutAcroForms;
    CosObj Work;
    CosObj InFields;
    CosObj OutFields;
    ASInt32 FieldNumber;
    ASInt32 PageCount, Count;
    CosObj *InPageTable;
    CosObj *OutPageTable;

    // Process done if no Acroforms are found in the input document.
    if (CosObjGetType(InAcroForms) == CosNull) {
        std::cout << "No AcroForms found...\n";
        return 1;
    }

    // Process done if the Acroforms contain no fields.
    InFields = CosDictGet(InAcroForms, ASAtomFromString("Fields"));
    if ((CosObjGetType(InFields) != CosArray) || (CosArrayLength(InFields) == 0)) {
        std::cout << "AcroForm contains no fields...\n";
        return 2;
    }

    // Transliterate references to a page cos object into page number,
    // and hence into a page cos object in the output doc. To facilitate this,
    // the program creates an array of cos objects for both documents, each entry being a page.
    PageCount = PDDocGetNumPages(InDoc);
    InPageTable = (CosObj *)malloc(sizeof(CosObj) * PageCount);
    OutPageTable = (CosObj *)malloc(sizeof(CosObj) * PageCount);
    for (Count = 0; Count < PageCount; Count++) {
        PDPage InPage = PDDocAcquirePage(InDoc, Count);
        PDPage OutPage = PDDocAcquirePage(OutDoc, Count);
        InPageTable[Count] = PDPageGetCosObj(InPage);
        OutPageTable[Count] = PDPageGetCosObj(OutPage);
        PDPageRelease(InPage);
        PDPageRelease(OutPage);
    }

    // Create an Acroforms dictionary in the output. It should match the
    // input dictionary, except for the fields array (completed below).
    // Create the new dictionary, and insert it into the root.
    OutAcroForms = CosNewDict(OutCosDoc, FALSE, 5);
    CosDictPut(OutRoot, ASAtomFromString("AcroForm"), OutAcroForms);

    // Copy over NeedAppearances
    Work = CosDictGet(InAcroForms, ASAtomFromString("NeedAppearances"));
    if (CosObjGetType(Work) != CosNull) {
        Work = CosNewBoolean(OutCosDoc, FALSE, CosBooleanValue(Work));
        CosDictPut(OutAcroForms, ASAtomFromString("NeedAppearances"), Work);
    }

    // Copy over SigFlags
    Work = CosDictGet(InAcroForms, ASAtomFromString("SigFlags"));
    if (CosObjGetType(Work) != CosNull) {
        Work = CosNewInteger(OutCosDoc, FALSE, CosIntegerValue(Work));
        CosDictPut(OutAcroForms, ASAtomFromString("SigFlags"), Work);
    }

    // Copy over Q
    Work = CosDictGet(InAcroForms, ASAtomFromString("Q"));
    if (CosObjGetType(Work) != CosNull) {
        Work = CosNewInteger(OutCosDoc, FALSE, CosIntegerValue(Work));
        CosDictPut(OutAcroForms, ASAtomFromString("Q"), Work);
    }

    // Copy over DA
    Work = CosDictGet(InAcroForms, ASAtomFromString("DA"));
    if (CosObjGetType(Work) != CosNull) {
        unsigned char String[4096];
        ASInt32 Size;

        CosStringValueSafe(Work, (char *)String, sizeof(String), &Size);
        Work = CosNewString(OutCosDoc, FALSE, (char *)String, Size);
        CosDictPut(OutAcroForms, ASAtomFromString("DA"), Work);
    }

    // Copy over CO
    Work = CosDictGet(InAcroForms, ASAtomFromString("CO"));
    if (CosObjGetType(Work) != CosNull) {
        Work = CosObjCopy(Work, OutCosDoc, TRUE);
        CosDictPut(OutAcroForms, ASAtomFromString("CO"), Work);
    }

    // Copy over DR
    Work = CosDictGet(InAcroForms, ASAtomFromString("DR"));
    if (CosObjGetType(Work) != CosNull) {
        Work = CosObjCopy(Work, OutCosDoc, TRUE);
        CosDictPut(OutAcroForms, ASAtomFromString("DR"), Work);
    }

    // Copy over XFA
    Work = CosDictGet(InAcroForms, ASAtomFromString("XFA"));
    if (CosObjGetType(Work) != CosNull) {
        Work = CosObjCopy(Work, OutCosDoc, TRUE);
        CosDictPut(OutAcroForms, ASAtomFromString("XFA"), Work);
    }

    // For each field, move both the field and the widget(s) to the output document
    OutFields = CosNewArray(OutCosDoc, FALSE, CosArrayLength(InFields));
    CosDictPut(OutAcroForms, ASAtomFromString("Fields"), OutFields);
    for (FieldNumber = 0; FieldNumber < CosArrayLength(InFields); FieldNumber++) {
        CosObj InField = CosArrayGet(InFields, FieldNumber);
        CosObj OutField =
            CopyField(InCosDoc, OutCosDoc, InField, CosNewNull(), PageCount, InPageTable, OutPageTable);
        CosArrayPut(OutFields, FieldNumber, OutField);
    }

    free(InPageTable);
    free(OutPageTable);
    return 0;
}
