//
// Copyright (c) 2017-2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// Sample: RenderPage
//
// This file contains declarations for the RenderPage class.
//

#include <stdio.h>
#include <string.h>

#include "CosCalls.h"
#include "ASCalls.h"
#include "PDCalls.h"
#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "PDFLExpT.h"
#include "PDFLCalls.h"
#include "DLExtrasCalls.h"

class RenderPage {
  private:
    PDPage pdPage;
    PDEImageAttrs attrs;
    PDEColorSpace cs;
    PDEFilterArray filterArray;
    ASAtom csAtom;
    ASInt32 nComps;
    ASSize_t bufferSize;
    ASInt32 bpc;
    char *buffer;
    char *colorSpace;
    char *filterName;

    ASFixedRect imageSize; // This will carry the image size in PDF units.

    PDEFilterArray SetDCTFilterParams(CosDoc cosDoc);
    PDEFilterArray SetCCITTFaxFilterParams(CosDoc cosDoc);
    ASAtom SetColorSpace(const char *colorSpace);
    ASInt32 SetBPC(ASInt32 bitsPerComp);

    static ASAtom sDeviceRGB_K;
    static ASAtom sDeviceRGBA_K;
    static ASAtom sDeviceCMYKA_K;
    static ASAtom sDeviceCMYK_K;
    static ASAtom sDeviceGray_K;

  public:
    RenderPage(PDPage &pdPage, const char *colorSpace, const char *filterName, ASInt32 bpc, double resolution);
    ~RenderPage();

    char *GetImageBuffer();
    ASSize_t GetImageBufferSize();
    PDEImage GetPDEImage(PDDoc outDoc);
    ASFixedRect GetImageSize();
};
