//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
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


class RenderPage 
{
private:
    PDPage              pdPage;
    PDEImage            image; 
    PDEImageAttrs       attrs;
    PDEColorSpace       cs;
    PDEFilterArray      filterArray;
    ASFixedMatrix       matrix;
    ASFixedMatrix       scaleMatrix;
    ASFixedMatrix       imageMatrix; 
    ASFixedRect         destRect;
    ASFixedRect         scaledDestRect;
    ASFixedRect         pageRect;
    ASAtom              csAtom;
    ASInt32             nComps;
    ASInt32             bufferSize;
    ASInt32             bpc;
    char*               buffer; 
    char*               colorSpace;
    char*               filterName;
    float               resolution;

    PDEImageAttrs       SetImageAttrs(ASFixedRect scaledDestRect, ASInt32 bpc);
    PDEFilterArray      SetFilter(const char *filterName);
    ASFixedMatrix       SetImageMatrix(PDEImageAttrs attrs, float resolution);
    ASFixedMatrix       SetScaleMatrix(float resolution);
    ASFixedRect         SetPageRect(ASFixedRect	destRect);
    ASAtom              SetColorSpace(const char *colorSpace);
    ASInt32             SetBPC(ASInt32 bitsPerComp); 
    ASInt32             PadCompute(PDEImageAttrs attrs, ASInt32 bpc, ASInt32 nComps, char *buffer, ASInt32 bufferSize);	 
    ASInt32             SetBufferSize(PDPage pdPage, ASFixedMatrix &matrix, ASAtom csAtom, ASInt32 bpc, ASFixedRect scaledDestRect);
    float               SetResolution(float resolution);

    static ASAtom       sDeviceRGB_K;
    static ASAtom       sDeviceCMYK_K;
    static ASAtom       sDeviceGray_K;

public:
    RenderPage(PDPage &pdPage, const char *colorSpace, const char *filterName, ASInt32 bpc, float resolution);
    ~RenderPage();

    char*               GetImageBuffer();
    ASInt32             GetImageBufferSize();
    PDEImage            MakePDEImage();
    ASFixedRect         GetImageRect();
};
