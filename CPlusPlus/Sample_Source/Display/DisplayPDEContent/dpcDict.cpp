//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

#include <sstream>

#include "ASCalls.h"
#include "CosCalls.h"
#include "PDCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "DisplayPDEContent.h"
#include "dpcOutput.h"

typedef struct dictent {
    char *Key;
    char *Value;
} DICTENT;

typedef struct dict {
    CosObj DictObj;
    ASUns32 NumberOfEntries;
    ASUns32 CurrentEntry;
    DICTENT Entries[1];
} DICT;

static ASBool CountCosDictEnumProc(CosObj obj, CosObj value, void *clientData) {
    ASInt32 *Count = (ASInt32 *)clientData;

    (*Count)++;
    return (true);
}

static ASBool LoadCosDictEnumProc(CosObj obj, CosObj value, void *clientData) {
    DICT *Dictionary = (DICT *)clientData;
    DICTENT *Entry = &Dictionary->Entries[Dictionary->CurrentEntry];
    ASTCount NameSize;
    CosType ValueType = CosObjGetType(value);
    ASText astKey, astValue;

    Dictionary->CurrentEntry++;
    astKey = ASTextFromPDText(CosCopyNameStringValue(obj, &NameSize));
    Entry->Key = reinterpret_cast<char *>(ASTextGetUnicodeCopy(astKey, kUTF8));
    ASTextDestroy(astKey);

    Entry->Key = CosCopyNameStringValue(obj, &NameSize);

    switch (ValueType) {
    case CosArray:
        Entry->Value = (char *)ASmalloc(19);
        strcpy(Entry->Value, "An Array Of Values");
        break;

    case CosBoolean:
        Entry->Value = (char *)ASmalloc(6);
        if (CosBooleanValue(value))
            strcpy(Entry->Value, "true");
        else
            strcpy(Entry->Value, "false");
        break;

    case CosDict:
        Entry->Value = (char *)ASmalloc(23);
        strcpy(Entry->Value, "A dictionary of values");
        break;

    case CosInteger:
    case CosReal: {
        double Numeric = CosFloatValue(value);
        std::ostringstream ossF;
        ossF << Numeric;
        Entry->Value = (char *)ASmalloc(ossF.str().length() + 1);
        strcpy(Entry->Value, ossF.str().c_str());
    } break;

    case CosName:
        astValue = ASTextFromPDText(CosCopyNameStringValue(value, &NameSize));
        Entry->Value = reinterpret_cast<char *>(ASTextGetUnicodeCopy(astValue, kUTF8));
        ASTextDestroy(astValue);
        break;

    case CosNull:
        Entry->Value = (char *)ASmalloc(11);
        strcpy(Entry->Value, "null value");
        break;

    case CosStream:
        Entry->Value = (char *)ASmalloc(9);
        strcpy(Entry->Value, "A Stream");
        break;

    case CosString:
        astValue = ASTextFromPDText(CosCopyStringValue(value, &NameSize));
        Entry->Value = reinterpret_cast<char *>(ASTextGetUnicodeCopy(astValue, kUTF8));
        ASTextDestroy(astValue);
        break;
    }
    return (true);
}

static DICT *LoadCosDict(CosObj Dictionary) {
    DICT *Result;
    CosType DictType = CosObjGetType(Dictionary);
    ASUns32 Size = 0;

    if ((DictType != CosDict) && (DictType != CosStream))
        return (NULL);

    CosObjEnum(Dictionary, CountCosDictEnumProc, &Size);
    ASUns32 dict_size = sizeof(DICT) + Size * sizeof(DICTENT);
    Result = (DICT *)ASmalloc(dict_size);
    memset(Result, 0, dict_size);

    Result->DictObj = Dictionary;
    Result->NumberOfEntries = Size;
    Result->CurrentEntry = 0;
    CosObjEnum(Dictionary, LoadCosDictEnumProc, Result);

    return (Result);
}

static void FreeDict(DICT *Dictionary) {
    ASUns32 Index;

    for (Index = 0; Index < Dictionary->NumberOfEntries; Index++) {
        DICTENT *Entry = &Dictionary->Entries[Index];

        ASfree(Entry->Key);
        ASfree(Entry->Value);
    }

    ASfree(Dictionary);
    return;
}

std::string DisplayCosDict(CosObj cobj) {
    std::ostringstream oss;
    oss << '<';
    if (CosObjGetType(cobj) == CosDict) {
        DICT *Dictionary = LoadCosDict(cobj);
        ASUns32 NumEntries = Dictionary->NumberOfEntries;

        for (ASUns32 Index = 0; Index < NumEntries; Index++) {
            DICTENT *Entry = &Dictionary->Entries[Index];
            oss << '\"' << Entry->Key << "\" = \"" << Entry->Value << '\"';
            if (Index + 1 < NumEntries) {
                oss << ", ";
            }
        }
        FreeDict(Dictionary);
    }
    oss << '>';
    return oss.str();
}
