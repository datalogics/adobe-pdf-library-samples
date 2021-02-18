//
// Copyright (c) 2010-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
// File:  EmbedFonts.h
//

#include <vector>

// Tracking information for the fonts used in a document
typedef struct _t_pdfUsedFont {
    CosObj fontCosObj;
    PDSysFont pdSysFont;
    PDEFont pdeFont;
    _t_pdfUsedFont(CosObj co) : fontCosObj(co) {
        memset(&pdSysFont, 0, sizeof(PDSysFont));
        memset(&pdeFont, 0, sizeof(PDEFont));
    }
    ~_t_pdfUsedFont() { PDERelease(reinterpret_cast<PDEObject>(pdeFont)); }
} PdfUsedFont;

typedef std::vector<_t_pdfUsedFont *> ContainerOfFonts, *pContainerOfFonts;

void EmbedSysFontForFontEntry(struct _t_pdfUsedFont *fontEntry, PDDoc pdDoc);
ACCB1 ASBool ACCB2 GetFontInfoProc(PDFont pdFont, PDFontFlags *pdFontFlagsPtr, void *clientData);
