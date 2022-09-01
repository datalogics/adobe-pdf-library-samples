//
// Copyright (c) 2010-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// The PDFUncompress sample is a utility that demonstrates how to completely un-compress the elements within a PDF
// document into a readable form.
//
// Nearly all PDF documents feature compressed elements to make the documents more efficient to use, and most of the time
// these documents are left in their compressed state even when being opened in a browser or viewing tool.  But sometimes
// it is necessary to completely uncompress a PDF document so that it can be opened and all of its contents viewed in detail
// in a text editor. This would be useful if you want to find the reason for a problem with a PDF document or set of PDF documents,
// or with a workflow that generates PDF documents.
//
// For example, this sample also uncompresses font streams that are embedded in the document. The fonts are normally compressed
// using the Flate compression algorithm, but PDFUncompress can render this font content as ASCII or Hexadecimal characters.
//
// For more detail see the description of the PDFUncompress sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#pdfuncompress

#include "CosCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PDFUncompress.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "CopyContent.pdf"
#define OUTPUT_FILE "PDFUncompress-out.pdf"

int main(int argc, char *argv[]) {
    // Initialize library
    APDFLib libInit;
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : OUTPUT_FILE);
    std::cout << "Will uncompress file " << csInputFile.c_str() << " and save as "
              << csOutputFile.c_str() << std::endl;

    // Open input document
    PDDoc pdDoc;
    DURING
        ASPathName InPath = APDFLDoc::makePath(csInputFile.c_str());
        pdDoc = PDDocOpen(InPath, NULL, NULL, true);
        ASFileSysReleasePath(NULL, InPath);
    HANDLER
        std::cout << "Could not open file " << csInputFile.c_str() << std::endl;
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    // Determine if copying is permitted from this document
    bool DocumentEncrypted(false);
    DURING
        PDDocAuthorize(pdDoc, pdPermAll, NULL);
        PDDocSetNewCryptHandler(pdDoc, ASAtomNull);
    HANDLER
        DocumentEncrypted = true;
    END_HANDLER

    if (DocumentEncrypted) {
        PDDoc DocCopy;
        DURING
            DocCopy = PDDocCreate();
            PDDocInsertPages(DocCopy, PDBeforeFirstPage, pdDoc, 0, PDAllPages, PDInsertAll, NULL,
                             NULL, NULL, NULL);
        HANDLER
            std::cout << "Document is encoded, and copy is not permitted!\n";
            return -2;
        END_HANDLER
    }

    int CurrentPage(0);
    int Pages = PDDocGetNumPages(pdDoc);
    DURING
        for (CurrentPage = 0; CurrentPage < Pages; CurrentPage++) {
            PDPage Page = PDDocAcquirePage(pdDoc, CurrentPage);
            CosObj cosPage = PDPageGetCosObj(Page);
            CosObj Content = CosDictGet(cosPage, ASAtomFromString("Contents"));

            switch (CosObjGetType(Content)) {
            case CosStream:
                Content = Uncompress(Content);
                CosDictPut(cosPage, ASAtomFromString("Contents"), Content);
                break;

            case CosArray:
                for (int ContentElement = 0; ContentElement < CosArrayLength(Content); ContentElement++) {
                    CosObj Stream;
                    Stream = CosArrayGet(Content, ContentElement);
                    CosArrayPut(Content, ContentElement, Uncompress(Stream));
                }
                break;

            case CosNull:
                break;

            default:
                break;
            }

            Content = CosDictGet(cosPage, ASAtomFromString("Annots"));
            if (CosObjGetType(Content) != CosNull) {
                for (int count = 0; count < CosArrayLength(Content); count++) {
                    CosObj NewAnnot;
                    NewAnnot = UncompressAnnot(CosArrayGet(Content, count));
                    if (CosObjGetType(NewAnnot) != CosNull)
                        CosArrayPut(Content, count, NewAnnot);
                }
            }

            Content = CosDictGet(cosPage, ASAtomFromString("Resources"));
            if (CosObjGetType(Content) != CosNull) {
                CosObj Xobjs = CosDictGet(Content, ASAtomFromString("XObject"));
                if (CosObjGetType(Xobjs) == CosDict)
                    CosObjEnum(Xobjs, CompressXobj, &Xobjs);
            }

            PDPageRelease(Page);
        }
    HANDLER
        std::cout << "Problem uncompressing page " << CurrentPage + 1 << std::endl;
        return -3;
    END_HANDLER

    PDDocEnumResources(pdDoc, 0, Pages - 1, ASAtomNull, UncompressResource, pdDoc);

    DURING
        // Save document
        ASPathName path = APDFLDoc::makePath(csOutputFile.c_str());
        PDDocSaveParamsRec docSaveParamsRec;
        memset(&docSaveParamsRec, 0, sizeof(docSaveParamsRec));
        docSaveParamsRec.size = sizeof(PDDocSaveParamsRec);
        docSaveParamsRec.saveFlags = PDSaveFull | PDSaveCollectGarbage;
        docSaveParamsRec.newPath = path;
        docSaveParamsRec.major = 1;
        docSaveParamsRec.minor = 1;
        docSaveParamsRec.saveFlags2 = PDSaveUncompressed;
        PDDocSaveWithParams(pdDoc, &docSaveParamsRec);
    HANDLER
        {
            std::cout << "Problem writing file \"" << csOutputFile.c_str() << "\"" << std::endl;
            return -8;
        }
    END_HANDLER

    PDDocRelease(pdDoc);
    return 0;
}

CosObj Uncompress(CosObj Stream) {
    CosObj Filter;
    CosObj NewStream;
    CosObj CosDict;
    ASStm Stm;

    if (CosObjGetType(Stream) != CosStream)
        return (Stream);

    Filter = CosDictGet(Stream, ASAtomFromString("Filter"));

    if (CosObjGetType(Filter) == CosNull)
        return (Stream);

    CosDict = CosStreamDict(Stream);
    CosDict = CosObjCopy(CosDict, CosObjGetDoc(Stream), TRUE);
    CosDictRemove(CosDict, ASAtomFromString("Filter"));
    CosDictRemove(CosDict, ASAtomFromString("DecodeParms"));
    CosDictRemove(CosDict, ASAtomFromString("Length"));

    Stm = CosStreamOpenStm(Stream, cosOpenFiltered);
    NewStream = CosNewStream(CosObjGetDoc(Stream), TRUE, Stm, 0, TRUE, CosDict, CosNewNull(), -1);
    ASStmClose(Stm);

    return (NewStream);
}

ASBool UncompressResource(CosObj Key, CosObj Value, void *clientData) {
    const char *KeyName, *DictType;
    CosType KeyType = CosObjGetType(Key);
    CosType ValueType = CosObjGetType(Value);

    if (CosObjGetType(Key) == CosName)
        KeyName = ASAtomGetString(CosNameValue(Key));

    if (KeyType == CosDict) {
        ASAtom Type;
        CosObj TypeObj;

        TypeObj = CosDictGet(Key, ASAtomFromString("Type"));
        if (CosObjGetType(TypeObj) == CosName) {
            Type = CosNameValue(TypeObj);
            DictType = ASAtomGetString(Type);
        } else
            Type = ASAtomNull;

        if (Type == ASAtomFromString("Font")) {
            CosObj ToUnicode, CIDToGIDMap, CIDSet, FontDesc, DescFont;
            ToUnicode = CosDictGet(Key, ASAtomFromString("ToUnicode"));
            if (!CosObjEqual(ToUnicode, CosNewNull())) {
                ToUnicode = Uncompress(ToUnicode);
                CosDictPut(Key, ASAtomFromString("ToUnicode"), ToUnicode);
            }

            ToUnicode = CosDictGet(Key, ASAtomFromString("Encoding"));
            if (CosObjGetType(ToUnicode) == CosStream) {
                ToUnicode = Uncompress(ToUnicode);
                CosDictPut(Key, ASAtomFromString("Encoding"), ToUnicode);
            }

            DescFont = CosDictGet(Key, ASAtomFromString("DescendantFonts"));
            if (!CosObjEqual(DescFont, CosNewNull())) {
                DescFont = CosArrayGet(DescFont, 0);
                CIDToGIDMap = CosDictGet(DescFont, ASAtomFromString("CIDToGIDMap"));
                if (!CosObjEqual(CIDToGIDMap, CosNewNull())) {
                    CIDToGIDMap = Uncompress(CIDToGIDMap);
                    CosDictPut(DescFont, ASAtomFromString("CIDToGIDMap"), CIDToGIDMap);
                }

                FontDesc = CosDictGet(DescFont, ASAtomFromString("FontDescriptor"));
                if (!CosObjEqual(FontDesc, CosNewNull())) {
                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("CISSet"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("CISSet"), CIDSet);
                    }

                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("FontFile"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("FontFile"), CIDSet);
                    }

                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("FontFile2"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("FontFile2"), CIDSet);
                    }

                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("FontFile3"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("FontFile3"), CIDSet);
                    }
                }
            } else {
                FontDesc = CosDictGet(Key, ASAtomFromString("FontDescriptor"));
                if (!CosObjEqual(FontDesc, CosNewNull())) {
                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("CISSet"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("CISSet"), CIDSet);
                    }

                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("FontFile"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("FontFile"), CIDSet);
                    }

                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("FontFile2"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("FontFile2"), CIDSet);
                    }

                    CIDSet = CosDictGet(FontDesc, ASAtomFromString("FontFile3"));
                    if (CosObjGetType(CIDSet) == CosStream) {
                        CIDSet = Uncompress(CIDSet);
                        CosDictPut(FontDesc, ASAtomFromString("FontFile3"), CIDSet);
                    }
                }
            }
        }

        if (KeyType == CosStream)
            Uncompress(Key);
        if (ValueType == CosStream)
            Uncompress(Value);
    }

    return (TRUE);
}

ASBool CompressXobj(CosObj Key, CosObj Value, void *Data) {
    CosObj NewXobj = Uncompress(Value);
    CosDictPut(*(CosObj *)Data, CosNameValue(Key), NewXobj);
    return TRUE;
}

ASBool SubAnnot(CosObj Key, CosObj Value, void *Data) {
    CosType Type = CosObjGetType(Value);

    if (Type == CosStream)
        CosDictPut(*(CosObj *)Data, CosNameValue(Key), Uncompress(Value));

    if (Type == CosDict)
        CosObjEnum(Value, SubAnnot, &Value);

    return TRUE;
}

CosObj UncompressAnnot(CosObj Annot) {
    CosObj APDict;

    if (CosObjGetType(Annot) != CosDict)
        return (CosNewNull());

    APDict = CosDictGet(Annot, ASAtomFromString("AP"));
    if (CosObjGetType(APDict) == CosNull)
        return (CosNewNull());

    if (CosObjGetType(APDict) == CosStream)
        return (Uncompress(APDict));

    CosObjEnum(APDict, SubAnnot, &APDict);
    return (CosNewNull());
}
