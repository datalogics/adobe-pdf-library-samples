//
// Copyright (c) 2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates creating a new PDF document with a Header and Footer.
//
//
// For more detail see the description of the AddHeaderFooter sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addheaderfooter

#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "ASExtraCalls.h"

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#define DEF_OUTPUT "AddHeaderFooter-out.pdf"

// Calculate the Width of the glyphs used by the Font at its point size
static double MeasureTextWidth(PDEFont pdeFont, double fontSize, ASUns8* textString)
{
    double textWidth = 0;

    double widthEMs = PDEFontSumWidths(pdeFont, textString, strlen((const char*)textString));

    // Convert from Glyph space to Text Space
    textWidth = (widthEMs / 1000) * fontSize;

    return textWidth;
}

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;
    APDFLib libInit;

    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    ASDouble fontSize = 12.0;
    ASDouble topBottomMargin = 0.5;
    ASInt16 pageWidth = 8.5 * 72;
    ASInt16 pageHeight = 11 * 72;

    std::string csOutputFileName(DEF_OUTPUT);
    std::string csHeaderText("Title of Document");
    std::string csFooterText("Page 1");

    std::cout << "Creating document " << csOutputFileName.c_str() << " with Header and Footer."<< std::endl;

    DURING
        APDFLDoc doc;

        // Create the new page
        doc.insertPage(pageWidth, pageHeight, PDBeforeFirstPage);

        PDPage page = doc.getPage(0);

        PDEContent content = PDPageAcquirePDEContent(page, NULL);

        PDEFontAttrs fontAttrs;
        memset(&fontAttrs, 0, sizeof(PDEFontAttrs));

        fontAttrs.name = ASAtomFromString("Times-Roman");

        PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0);

        PDEFont pdeFont = PDEFontCreateFromSysFont(sysFont, kPDEFontCreateSubset);

        PDEFontGetAttrs(pdeFont, &fontAttrs, sizeof(PDEFontAttrs));

        PDEGraphicState gState;
        PDEDefaultGState(&gState, sizeof(PDEGraphicState));

        // Calculate the position to place the Header and Footer Text
        ASDoubleMatrix headerTextMatrix;
        memset(&headerTextMatrix, 0, sizeof(ASDoubleMatrix));

        double textHeight = (double)(fontAttrs.ascent + std::abs(fontAttrs.descent)) / 1000;

        headerTextMatrix.a = fontSize;
        headerTextMatrix.d = fontSize;
        headerTextMatrix.h = (pageWidth - MeasureTextWidth(pdeFont, fontSize, (ASUns8*)csHeaderText.c_str())) / 2;
        headerTextMatrix.v = pageHeight - topBottomMargin * 72.0 + textHeight;

        ASDoubleMatrix footerTextMatrix;
        memset(&footerTextMatrix, 0, sizeof(ASDoubleMatrix));

        footerTextMatrix.a = fontSize;
        footerTextMatrix.d = fontSize;
        footerTextMatrix.h = (pageWidth - MeasureTextWidth(pdeFont, fontSize, (ASUns8*)csFooterText.c_str())) / 2;
        footerTextMatrix.v = topBottomMargin * 72.0 - textHeight;

        PDETextState tState;
        memset(&tState, 0, sizeof(PDETextState));

        PDEText textObj = PDETextCreate();

        // Add the Text elements
        PDETextAddEx(textObj, kPDETextRun, 0, (ASUns8 *)csHeaderText.c_str(), csHeaderText.length(), pdeFont, &gState, sizeof(PDEGraphicState), &tState, sizeof(PDETextState), &headerTextMatrix, NULL);

        PDETextAddEx(textObj, kPDETextRun, 1, (ASUns8 *)csFooterText.c_str(), csFooterText.length(), pdeFont, &gState, sizeof(PDEGraphicState), &tState, sizeof(PDETextState), &footerTextMatrix, NULL);

        // Add the Text element to the Page Content
        PDEContentAddElem(content, kPDEAfterLast, reinterpret_cast<PDEElement>(textObj));

        PDPageSetPDEContentCanRaise(page, NULL);

        doc.saveDoc(csOutputFileName.c_str(), PDSaveFull);

        // Release the resources acquired
        PDERelease(reinterpret_cast<PDEObject>(gState.strokeColorSpec.space));
        PDERelease(reinterpret_cast<PDEObject>(gState.fillColorSpec.space));
        PDERelease(reinterpret_cast<PDEObject>(pdeFont));
        PDERelease(reinterpret_cast<PDEObject>(textObj));

        PDPageReleasePDEContent(page, NULL);
        PDPageRelease(page);
    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

