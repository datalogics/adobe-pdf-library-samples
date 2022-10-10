//
// Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample program is effectively a version of Hello World, except that when run it generates a PDF document
// with the text “Hello Japan,” using Japanese Kanji characters. Two default fonts are provided in the sample program,
// though the sample does not define a default input file, and it does not define an input directory. The program generates
// an output PDF with two pages, using each of the two fonts provided, one on each page.  If you run the program from the
// command line you can define the name of the output file and the fonts to use.
//
// The files for the fonts used for this sample program are shipped with the Adobe PDF Library, stored in the Resource
// directory under APDFL. When the Adobe PDF Library initializes, it loads the files found in the Resource directory.
//
// For more detail see the description of the HelloJapan sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#hellojapan

#include "PEWCalls.h"
#include "PERCalls.h"
#include "PSFCalls.h"
#include "PagePDECntCalls.h"

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#define DEFAULT_FONT_1 "KozGoPr6N-Medium"
#define DEFAULT_FONT_2 "KozMinPr6N-Regular"
#define USE_UTF 1

#define OUTPUT_FILE "HelloJapan-out.pdf"

int main(int argc, char *argv[]) {
    APDFLib libInit; // Initialize the Adobe PDF Library. Termination will be automatic when scope is lost.

    if (libInit.isValid() == false) // Check for errors upon initialization.
    {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csOutputFile(argc > 1 ? argv[1] : OUTPUT_FILE);
    std::string csFont1(argc > 2 ? argv[2] : DEFAULT_FONT_1);
    std::string csFont2(argc > 3 ? argv[3] : DEFAULT_FONT_2);

    std::cout << "Using fonts " << csFont1.c_str() << " and " << csFont2.c_str()
              << ", writing to file " << csOutputFile.c_str() << std::endl;

    // When supplying Unicode text to the PDEText API, such as for all Character Map (CMAP) files with
    // "UCS" in their names, the text must be provided as UTF16 in big endian format (UTF16-BE).
    // In this sample, arrays of ASUns8 (bytes) are explicitly populated for the sake of exposition.
#if USE_UTF
    size_t stringLen = 18;

    // "Hello [Nihon]." in UTF16-BE
    ASUns8 *HelloWorldStr = (ASUns8 *)"\x00\x48\x00\x65\x00\x6C\x00\x6C\x00\x6F\x00\x20\x65\xE5\x67"
                                      "\x2c\x00\x2E";
    //                                 H       e       l       l       o       [sp]    [Ni]    [Hon] .
#else
    size_t stringLen = 11;
    // "Hello [Nihon]." in Shift-JIS
    ASUns8 *HelloWorldStr = (ASUns8 *)"Hello \x93\xfa\x96\x7b.";
#endif

    // The first page of the PDF document uses a CIDType0 font (Character ID-based font)
    // for setting. In this case, the KozGoPr6N-Medium font is used by default.
    DURING
        PDDoc pdDoc = PDDocCreate();
        ASFixedRect mediaBox; // dimensions of page
        mediaBox.left = fixedZero;
        mediaBox.top = Int16ToFixed(4 * 72);
        mediaBox.right = Int16ToFixed(5 * 72);
        mediaBox.bottom = fixedZero;

        PDPage pdPage = PDDocCreatePage(pdDoc, PDBeforeFirstPage, mediaBox);
        PDEContent pdeContent = PDPageAcquirePDEContent(pdPage, NULL);

        PDEFontAttrs pdeFontAttrs;
        memset(&pdeFontAttrs, 0, sizeof(pdeFontAttrs));
        pdeFontAttrs.name = ASAtomFromString(csFont1.c_str());

        PDSysFont sysFont = PDFindSysFont(&pdeFontAttrs, sizeof(PDEFontAttrs), 0);
        PDSysEncoding sysEncoding;
#if USE_UTF
        sysEncoding = PDSysEncodingCreateFromCMapName(ASAtomFromString("UniJIS-UTF16-H"));
#else
        sysEncoding = PDSysEncodingCreateFromCMapName(ASAtomFromString("90msp-RKSJ-H"));
#endif
        PDEFont pdeFont = PDEFontCreateFromSysFontAndEncoding(
            sysFont, sysEncoding, pdeFontAttrs.name, kPDEFontCreateEmbedded | kPDEFontCreateSubset);

        PDEGraphicState gState; // graphic state to apply to operation
        PDEDefaultGState(&gState, sizeof(PDEGraphicState));

        // transformation matrix for text:
        ASFixedMatrix textMatrix = {24 * fixedOne, fixedZero,       fixedZero,
                                    24 * fixedOne, fixedSeventyTwo, 2 * fixedSeventyTwo};
        PDEText pdeText = PDETextCreate();
        PDETextAdd(pdeText, kPDETextRun, 0, HelloWorldStr, stringLen, pdeFont, &gState,
                   sizeof(gState), NULL, 0, &textMatrix, NULL);
        PDEContentAddElem(pdeContent, kPDEAfterLast, (PDEElement)pdeText);
        PDPageSetPDEContent(pdPage, NULL);

        // For subsetted Type0 fonts, signal to the PDF Library that all the characters
        // needed have been placed into PDEText areas. This allows APDFL to create the
        // subset font stream.
        PDEFontSubsetNow(pdeFont, PDDocGetCosDoc(pdDoc));
        PDERelease((PDEObject)pdeFont);
        PDERelease((PDEObject)pdeText);
        PDPageReleasePDEContent(pdPage, NULL);
        PDPageRelease(pdPage);
        PDERelease((PDEObject)sysEncoding);

        // The second page of the document uses a different font,
        // KozMinPr6N-Regular. This is a CIDType2 font, or a glyph ID-based font.
        pdPage = PDDocCreatePage(pdDoc, PDDocGetNumPages(pdDoc) - 1, mediaBox);
        pdeContent = PDPageAcquirePDEContent(pdPage, NULL);
        memset(&pdeFontAttrs, 0, sizeof(pdeFontAttrs));
        pdeFontAttrs.name = ASAtomFromString(csFont2.c_str());
        sysFont = PDFindSysFont(&pdeFontAttrs, sizeof(PDEFontAttrs), 0);
#if USE_UTF
        sysEncoding = PDSysEncodingCreateFromCMapName(ASAtomFromString("UniJIS-UTF16-H"));
#else
        sysEncoding = PDSysEncodingCreateFromCMapName(ASAtomFromString("90msp-RKSJ-H"));
#endif
        pdeFont = PDEFontCreateFromSysFontAndEncoding(sysFont, sysEncoding, pdeFontAttrs.name,
                                                      kPDEFontCreateEmbedded | kPDEFontCreateSubset);

        textMatrix.b = textMatrix.c = 0;
        textMatrix.a = Int32ToFixed(24);
        textMatrix.d = Int32ToFixed(24);
        textMatrix.h = Int32ToFixed(1 * 72);
        textMatrix.v = Int32ToFixed(2 * 72);
        pdeText = PDETextCreate();
        PDETextAdd(pdeText, kPDETextRun, 0, HelloWorldStr, stringLen, pdeFont, &gState,
                   sizeof(gState), NULL, 0, &textMatrix, NULL);
        PDEContentAddElem(pdeContent, kPDEAfterLast, (PDEElement)pdeText);
        PDPageSetPDEContent(pdPage, NULL);

        // For subsetted Type0 fonts, signal to the PDF Library that all the characters
        // needed have been placed into PDEText areas. This allows APDFL to create the
        // subset font stream.
        PDEFontSubsetNow(pdeFont, PDDocGetCosDoc(pdDoc));
        PDERelease((PDEObject)pdeFont);
        PDERelease((PDEObject)pdeText);
        PDPageReleasePDEContent(pdPage, NULL);
        PDPageRelease(pdPage);
        PDERelease((PDEObject)gState.strokeColorSpec.space);
        PDERelease((PDEObject)gState.fillColorSpec.space);
        PDERelease((PDEObject)sysEncoding);

        ASPathName outPath = APDFLDoc::makePath(csOutputFile.c_str());
        PDDocSave(pdDoc, PDSaveFull | PDSaveLinearized, outPath, ASGetDefaultFileSys(), NULL, NULL);
        ASFileSysReleasePath(NULL, outPath);
        PDDocClose(pdDoc);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}
