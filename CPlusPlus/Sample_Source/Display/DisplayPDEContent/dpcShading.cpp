//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

#include <string>

#include "ASCalls.h"
#include "CosCalls.h"
#include "PDCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"

#include "DisplayPDEContent.h"
#include "dpcOutput.h"

static ASBool PdeClipenumProc(PDEElement Element, void *clientData) {
    PDEGraphicState GState;
    ASBool HasGState = PDEElementHasGState(Element, &GState, sizeof(PDEGraphicState));

    ASFixedMatrix fm1;
    ASFixedMatrix matrix;
    PDEElementGetMatrix(Element, &fm1);
    if ((fixedZero == fm1.a) && (fixedZero == fm1.b) && (fixedZero == fm1.c) &&
        (fixedZero == fm1.d) && (fixedZero == fm1.h) && (fixedZero == fm1.v)) {
        // Invert zero matrix --> identity matrix
        ASFixedMatrixInvert(&matrix, &fm1);
    } else {
        PDEElementGetMatrix(Element, &matrix);
    }

    PDEType Type = (PDEType)PDEObjectGetType((PDEObject)Element);
    switch (Type) {
    case kPDEText:
        // Display a complete text object
        DisplayText((PDEText)Element, &matrix, &GState, HasGState);
        break;

    case kPDEPath:
        // Display a complete path
        DisplayPath((PDEPath)Element, &matrix, &GState, HasGState);
        break;
    default:
        Outputter::Inst()->GetOfs() << "******* Unknown Clip Element. Type " << Type << std::endl;
        break;
    }

    return true;
}

static void DisplayClip(PDEClip Clip) {
    ASInt32 nElems = PDEClipGetNumElems(Clip);
    if (nElems) {
        Outputter::Inst()->GetOfs() << "Begin Clip:\n";
        Outputter::Inst()->GetOfs() << "{\n";
        Outputter::Inst()->Indent();
        PDEClipFlattenedEnumElems(Clip, (PDEClipEnumProc)PdeClipenumProc, NULL);
        Outputter::Inst()->Outdent();
        Outputter::Inst()->GetOfs() << "} End of Clip\n";
    }
}

void DisplayShading(PDEShading Shading, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->GetOfs() << "Shading op: At " << DisplayMatrix(Matrix).c_str() << std::endl;
    Outputter::Inst()->Indent();

    CosObj cobj;
    PDEShadingGetCosObj(Shading, &cobj);

    if (CosObjGetType(cobj) == CosDict) {
        Outputter::Inst()->GetOfs() << "Shading Dictionary: " << DisplayCosDict(cobj) << std::endl;
    }

    if (HasGState)
        DisplayGraphicState(GState);

    PDEClip clip = PDEElementGetClip((PDEElement)Shading);
    if (clip)
        DisplayClip(clip);

    Outputter::Inst()->Indent();
}
