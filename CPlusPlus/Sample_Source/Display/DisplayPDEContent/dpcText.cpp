//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

#include <sstream>
#include <string>

#include "ASCalls.h"
#include "ASExtraCalls.h"
#include "CosCalls.h"
#include "PDCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"

#include "DisplayPDEContent.h"
#include "dpcOutput.h"

static std::string DisplayTextState(PDETextState *State) {
    char Amount[20];
    std::ostringstream oss;

    ASFixedToCString(State->charSpacing, Amount, 20, 5);
    oss << "Char Space: " << Amount << ", ";

    ASFixedToCString(State->wordSpacing, Amount, 20, 5);
    oss << "Word Space: " << Amount << ", ";

    ASFixedToCString(State->fontSize, Amount, 20, 5);
    oss << "Font Size: " << Amount << ", ";

    ASFixedToCString(State->hScale, Amount, 20, 5);
    oss << "Horizontal Scale: " << Amount << ", ";

    ASFixedToCString(State->textRise, Amount, 20, 5);
    oss << "Text Rise: " << Amount << ", ";

    oss << "Render Mode: ";
    switch (State->renderMode) {
    case 0:
        oss << "Fill";
        break;
    case 1:
        oss << "Stroke";
        break;
    case 2:
        oss << "Fill then stroke";
        break;
    case 3:
        oss << "Invisible";
        break;
    case 4:
        oss << "Fill and clip";
        break;
    case 5:
        oss << "Stroke and clip";
        break;
    case 6:
        oss << "Clip";
        break;
    case 7:
        oss << "Fill";
        break;
    }
    return oss.str();
}

void DisplayTextRun(PDEText Text, ASUns32 Run) {
    ASFixedMatrix RunMatrix, TextMatrix;
    ASFixedQuad TextQuad;
    PDETextState TextState;
    PDEGraphicState GState;
    ASFixedPoint Advance;
    CosObj COSFont;
    PDFont TranslationFont;
    PDEFont Font = PDETextGetFont(Text, kPDETextRun, Run);
    ASUns32 NumBytes = PDETextGetNumBytes(Text, kPDETextRun, Run);
    ASUns32 UCSBytes;
    ASUns8 *TextContent;
    ASUns8 *UCSText;
    char *FontName;

    PDETextGetGState(Text, kPDETextRun, Run, &GState, sizeof(PDEGraphicState));
    PDETextGetMatrix(Text, kPDETextRun, Run, &RunMatrix);
    PDETextGetQuad(Text, kPDETextRun, Run, &TextQuad);
    PDETextGetState(Text, kPDETextRun, Run, &TextState, sizeof(TextState));
    PDETextGetTextMatrix(Text, kPDETextRun, Run, &TextMatrix);
    PDETextGetAdvance(Text, kPDETextRun | kPDETextPageSpace, Run, &Advance);

    // Obtain the text in whatever encoding the font used
    TextContent = (ASUns8 *)ASmalloc(NumBytes + 2);
    PDETextGetText(Text, kPDETextRun, Run, TextContent);
    TextContent[NumBytes] = TextContent[NumBytes + 1] = '\0';

    // Create a PD font, and translate the font encoded text to UCS
    PDEFontGetCosObj(Font, &COSFont);
    TranslationFont = PDFontFromCosObj(COSFont);
    UCSBytes = PDFontXlateToUCS(TranslationFont, TextContent, NumBytes, NULL, 0);
    UCSText = (ASUns8 *)ASmalloc(UCSBytes + 2);
    UCSBytes = PDFontXlateToUCS(TranslationFont, TextContent, NumBytes, UCSText, UCSBytes + 2);

    ASText displayText = ASTextFromSizedUnicode((ASUTF16Val *)UCSText, kUTF16BigEndian, UCSBytes);
    char *displayTextUTF8 = reinterpret_cast<char *>(ASTextGetUnicodeCopy(displayText, kUTF8));
    ASTextDestroy(displayText);

    ASText asFontName = ASTextNew();
    PDFontGetASTextName(TranslationFont, false, asFontName);
    FontName = (char *)ASTextGetUnicodeCopy(asFontName, kUTF8);
    ASTextDestroy(asFontName);

    Outputter::Inst()->GetOfs() << "Text Run: At " << DisplayQuad(&TextQuad) << std::endl;
    Outputter::Inst()->Indent();
    Outputter::Inst()->GetOfs() << "Matrix: " << DisplayMatrix(&TextMatrix) << " Font: " << FontName
                                << std::endl;
    Outputter::Inst()->GetOfs() << "Text State: " << DisplayTextState(&TextState) << std::endl;

    DisplayGraphicState(&GState);

    Outputter::Inst()->GetOfs() << "Content: \"" << displayTextUTF8 << "\"" << std::endl;
    Outputter::Inst()->Outdent();

    ASfree(UCSText);
    ASfree(TextContent);
    ASfree(displayTextUTF8);
    ASfree(FontName);
}

void DisplayText(PDEText Text, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    ASUns32 Runs = PDETextGetNumRuns(Text);

    Outputter::Inst()->GetOfs() << "Text Object: At " << DisplayMatrix(Matrix) << ", contains "
                                << Runs << " runs.\n";
    Outputter::Inst()->GetOfs() << "{\n";
    Outputter::Inst()->Indent();

    for (ASUns32 Run = 0; Run < Runs; Run++) {
        DisplayTextRun(Text, Run);
    }

    Outputter::Inst()->Outdent();
    Outputter::Inst()->GetOfs() << "}\n";
}
