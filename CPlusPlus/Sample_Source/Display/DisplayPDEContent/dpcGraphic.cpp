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

void DisplayGraphicState(PDEGraphicState *GState) {
    char Amount[20];
    std::ostringstream oss;
    oss << "Graphic State: ";

    ASFixedToCString(GState->dash.dashPhase, Amount, 20, 5);
    oss << "Dash Phase: " << Amount << ", ";

    oss << "Dash Length: " << GState->dash.dashLen << ", ";
    oss << "Dashes [";
    for (ASInt32 Dash = 0; Dash < GState->dash.dashLen; Dash++) {
        ASFixedToCString(GState->dash.dashes[Dash], Amount, 20, 5);
        oss << Amount;
        if (Dash + 1 < GState->dash.dashLen)
            oss << ", ";
    }
    oss << "], ";

    ASFixedToCString(GState->lineWidth, Amount, 20, 5);
    oss << "Line Width: " << Amount << ", ";

    ASFixedToCString(GState->miterLimit, Amount, 20, 5);
    oss << "Miter Limit: " << Amount << ", ";

    ASFixedToCString(GState->flatness, Amount, 20, 5);
    oss << "Flatness: " << Amount << ", ";

    ASFixedToCString(GState->lineCap, Amount, 20, 5);
    oss << "LineCap: " << Amount << ", ";

    ASFixedToCString(GState->lineJoin, Amount, 20, 5);
    oss << "Line Join: " << Amount << ", ";
    oss << "Render Intent: " << ASAtomGetString(GState->renderIntent);

    Outputter::Inst()->GetOfs() << oss.str() << std::endl;
    Outputter::Inst()->Indent();
    oss.str("");

    if (GState->extGState) {
        // Format extended gstate
        PDEExtGState ExtState = GState->extGState;
        oss << "Alpha Is Shape: " << (PDEExtGStateGetAIS(ExtState) ? "true " : "false ");

        oss << "Blend Mode: " << ASAtomGetString(PDEExtGStateGetBlendMode(ExtState)) << " ";

        ASFixedToCString(PDEExtGStateGetOpacityFill(ExtState), Amount, 20, 5);
        oss << " Fill Opacity: " << Amount << ", ";

        ASFixedToCString(PDEExtGStateGetOpacityStroke(ExtState), Amount, 20, 5);
        oss << "Stroke Opacity: " << Amount << ", ";

        oss << "Fill Overprinting: " << (PDEExtGStateGetOPFill(ExtState) ? "on " : "off ")
            << "Stroke Overprinting: " << (PDEExtGStateGetOPStroke(ExtState) ? "on " : "off ") << std::endl;
        Outputter::Inst()->GetOfs() << oss.str();
        oss.str("");

        ASFixedToCString(PDEExtGStateGetOPM(ExtState), Amount, 20, 5);
        oss << "Overprint Mode: " << Amount << ", ";
        oss << "Stroke Adjustment: " << (PDEExtGStateGetSA(ExtState) ? "enabled " : "disabled ");
        oss << "Text Knockout: " << (PDEExtGStateGetTK(ExtState) ? "enabled " : "disabled ");
        oss << (PDEExtGStateHasSoftMask(ExtState) ? "Has Soft Mask " : "Has No Soft Mask") << std::endl;
        Outputter::Inst()->GetOfs() << oss.str();
    }

    Outputter::Inst()->GetOfs() << "Fill Color: " << DisplayColor(&GState->fillColorSpec) << std::endl;
    Outputter::Inst()->GetOfs() << "Stroke Color: " << DisplayColor(&GState->strokeColorSpec) << std::endl;
    Outputter::Inst()->Outdent();
}
