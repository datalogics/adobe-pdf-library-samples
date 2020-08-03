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

#include "DisplayPDEContent.h"
#include "dpcOutput.h"

void DisplayPath(PDEPath Path, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    PDEPathOpFlags PaintOp = (PDEPathOpFlags)PDEPathGetPaintOp(Path);
    ASInt32 PathSize = PDEPathGetData(Path, NULL, 0);
    ASInt32 *PathData = (ASInt32 *)ASmalloc(PathSize + 24); // Allow for me to read ahead, without crashing
    ASInt32 DataSize = PathSize / 4;
    PDEPathGetData(Path, PathData, PathSize);

    std::ostringstream oss;

    switch (PaintOp) {
    case kPDEInvisible:
        oss << "Invisible";
        break;
    case kPDEStroke:
        oss << "Stroke";
        break;
    case kPDEFill:
        oss << "Fill";
        break;
    case kPDEEoFill:
        oss << "EOFill";
        break;
    case (kPDEStroke | kPDEFill):
        oss << "Stroke and Fill";
        break;
    case (kPDEStroke | kPDEEoFill):
        oss << "Stroke and EOFill";
        break;
    }
    Outputter::Inst()->GetOfs() << "Path Object: " << oss.str().c_str() << " At "
                                << DisplayMatrix(Matrix).c_str() << std::endl;
    DisplayGraphicState(GState);
    Outputter::Inst()->GetOfs() << "{ Operators\n";
    Outputter::Inst()->Indent();

    ASInt32 Index = 0;
    while (1) {
        oss.str("");
        PDEPathElementType Operator;
        ASFixedPoint One, Two, Three;
        ASInt32 Increment;

        if (Index >= DataSize)
            break;

        Operator = (PDEPathElementType)PathData[Index];
        One.h = (ASFixed)PathData[Index + 1];
        One.v = (ASFixed)PathData[Index + 2];
        Two.h = (ASFixed)PathData[Index + 3];
        Two.v = (ASFixed)PathData[Index + 4];
        Three.h = (ASFixed)PathData[Index + 5];
        Three.v = (ASFixed)PathData[Index + 6];

        switch (Operator) {
        case kPDEMoveTo:
            oss << "MoveTo " << AppendPoint(&One, Matrix, false).c_str();
            Increment = 3;
            break;
        case kPDELineTo:
            oss << "LineTo " << AppendPoint(&One, Matrix, false).c_str();
            Increment = 3;
            break;
        case kPDECurveTo:
            oss << "CurveTo " << AppendPoint(&One, Matrix, true).c_str()
                << AppendPoint(&Two, Matrix, true).c_str() << AppendPoint(&Three, Matrix, false).c_str();
            Increment = 7;
            break;
        case kPDECurveToV:
            oss << "CurveToV " << AppendPoint(&One, Matrix, true).c_str()
                << AppendPoint(&Two, Matrix, false).c_str();
            Increment = 5;
            break;
        case kPDECurveToY:
            oss << "CurveToY " << AppendPoint(&One, Matrix, true).c_str()
                << AppendPoint(&Two, Matrix, false).c_str();
            Increment = 5;
            break;
        case kPDERect:
            oss << "Rectangle at  " << AppendPoint(&One, Matrix, true)
                << "Width: " << DisplayFixed(&Two.h) << " Height: " << DisplayFixed(&Two.v);
            Increment = 5;
            break;
        case kPDEClosePath:
            oss << "Close Path";
            Increment = 1;
            break;
        default:
            oss << "Invalid Path Operator";
            Increment = 1;
            break;
        }
        Outputter::Inst()->GetOfs() << oss.str().c_str() << std::endl;
        Index += Increment;
    }
    ASfree(PathData);
    Outputter::Inst()->Outdent();
    Outputter::Inst()->GetOfs() << "} End Path Content\n";
}
