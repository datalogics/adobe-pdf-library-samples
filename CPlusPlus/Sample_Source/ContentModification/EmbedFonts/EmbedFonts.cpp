//
// Copyright (c) 2010-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample program demonstrates how to scan a PDF document to determine whether the fonts
// used in that document are embedded. For each font the program finds that is not embedded in
// the document, the sample looks for characters in that font that are used in the document.
// It gathers up the characters and then requests that the Adobe PDF Library to generate a subset
// font stream for these characters and then embed this font in a subset data stream. That is,
// the program subsets the characters from that font into the PDF document.
//
// The default input PDF document has some fonts that are embedded and some that are not.
// The sample program finds the fonts in the document that are not embedded, and then looks for
// an alternative font on the local computer system to use as a substitute. Then, the program
// subsets the characters of that font that are used in the document into that document, and
// renames this newly subset font to show that it was embedded and subset.
//
// For more detail see the description of the EmbedFonts sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#embedfonts

#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "EmbedFonts.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_NAME "EmbedFonts-in.pdf"
#define OUTPUT_NAME "EmbedFonts-out.pdf"

int main(int argc, char **argv) {
    APDFLib libInit; // Initialize the library
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_DIR INPUT_NAME);
    std::string csOutputFileName(argc > 2 ? argv[2] : OUTPUT_NAME);
    std::cout << "Will embed fonts into " << csInputFileName.c_str() << " and rewrite file as "
              << csOutputFileName.c_str() << std::endl;

    ContainerOfFonts vFontsInUse;

    DURING

        // Open the input document
        APDFLDoc apdflDoc(csInputFileName.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();

        // Enumerate the fonts used in the document, and save those currently unembedded
        PDDocEnumFonts(pdDoc, 0, PDDocGetNumPages(pdDoc) - 1, GetFontInfoProc, &vFontsInUse, 0, 0);

        // Embed a suitable system font for each font in the to-embed list
        ContainerOfFonts::iterator it, itE = vFontsInUse.end();
        for (it = vFontsInUse.begin(); it != itE; ++it) {
            EmbedSysFontForFontEntry(*it, pdDoc);

            // Perform clean up while we're here
            delete *it;
        }

        // Save the PDF to a new file
        apdflDoc.saveDoc(csOutputFileName.c_str(), PDSaveFull | PDSaveCollectGarbage);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}

void EmbedSysFontForFontEntry(struct _t_pdfUsedFont *fontEntry, PDDoc pdDoc) {
    DURING
        fontEntry->pdeFont = PDEFontCreateFromCosObj(&(fontEntry->fontCosObj));

        // If there is a Cmap, acquire it
        PDEFontAttrs attrs;
        memset(&attrs, 0, sizeof(attrs));
        PDEFontGetAttrs(fontEntry->pdeFont, &attrs, sizeof(attrs));

        PDSysEncoding sysEnc = NULL;
        if (attrs.type == ASAtomFromString("Type0")) {
            sysEnc = PDSysEncodingCreateFromCMapName(attrs.encoding);
        } else {
            sysEnc = PDSysEncodingCreateFromBaseName(attrs.encoding, NULL);
        }

        fontEntry->pdSysFont = PDFindSysFontForPDEFont(fontEntry->pdeFont, kPDSysFontMatchNameAndCharSet);

        // If the font is not found on the system, sysFont will be 0.
        if (0 == fontEntry->pdSysFont) {
            std::cout << "Could not find a pdSysFont " << std::endl;
            return;
        }

        PDEFontSetSysFont(fontEntry->pdeFont, fontEntry->pdSysFont);

        // If there was a Cmap, set it as the sysencoding
        if (sysEnc)
            PDEFontSetSysEncoding(fontEntry->pdeFont, sysEnc);

        if (attrs.cantEmbed != 0) {
            std::cout << "Font " << ASAtomGetString(attrs.name) << " cannot be embedded" << std::endl;
        } else {
            if (PDEFontIsMultiByte(fontEntry->pdeFont)) {
                // Subset embed font
                PDEFont pdeFont;
                if (sysEnc)
                    pdeFont = PDEFontCreateFromSysFontAndEncoding(
                        fontEntry->pdSysFont, sysEnc, attrs.name, kPDEFontCreateEmbedded | kPDEFontCreateSubset);
                else
                    pdeFont = PDEFontCreateFromSysFont(fontEntry->pdSysFont,
                                                       kPDEFontCreateEmbedded | kPDEFontCreateSubset);
                PDEFontSubsetNow(fontEntry->pdeFont, PDDocGetCosDoc(pdDoc));

                PDERelease((PDEObject)pdeFont);
            } else {
                // Fully embed font
                PDEFont pdeFont = PDEFontCreateFromSysFont(fontEntry->pdSysFont, kPDEFontCreateEmbedded);
                PDEFontEmbedNow(fontEntry->pdeFont, PDDocGetCosDoc(pdDoc));

                PDERelease((PDEObject)pdeFont);
            }
        }

        if (sysEnc)
            PDERelease((PDEObject)sysEnc);

    HANDLER
        APDFLib::displayError(ERRORCODE);
    END_HANDLER
}

ACCB1 ASBool ACCB2 GetFontInfoProc(PDFont pdFont, PDFontFlags *pdFontFlagsPtr, void *clientData) {
    CosObj cosFont;
    PDEFontAttrs attrs;
    PDSysFont sysFont;
    char fontNameBuf[PSNAMESIZE];
    const char *fontSubtypeP;
    char *fontNameStart = 0;
    ASBool fontEmbedded = false, fontSubset = false, fontIsSysFont = false;
    DURING
        memset(&attrs, 0, sizeof(attrs));

        PDFontGetName(pdFont, fontNameBuf, PSNAMESIZE);
        attrs.name = ASAtomFromString(fontNameBuf);
        attrs.type = PDFontGetSubtype(pdFont);
        fontSubtypeP = ASAtomGetString(attrs.type);

        fontEmbedded = PDFontIsEmbedded(pdFont);
        // Subset test: a font was subset if the 7th character is '+' (a plus-sign), according
        // to Acrobat/Reader and industry norms.
        if (fontEmbedded) {
            if ((strlen(fontNameBuf)) > 7 && (fontNameBuf[6] == '+'))
                fontSubset = true;
        }
        if (fontSubset)
            fontNameStart = fontNameBuf + 7; // skip the "ABCDEF+"
        else
            fontNameStart = fontNameBuf;

        // If the font is not found on the system, sysFont will be 0.
        sysFont = PDFindSysFont(&attrs, sizeof(PDEFontAttrs), 0);
        if (sysFont)
            fontIsSysFont = true;

        // Print font information
        std::cout << "Font " << fontNameStart << ", Subtype " << fontSubtypeP << " ("
                  << (fontIsSysFont ? "" : "Not a ") << "System Font, ";
        if (fontEmbedded) {
            std::cout << "embedded" << (fontSubset ? " subset" : "");
        } else {
            std::cout << "unembedded";
        }
        std::cout << ")" << std::endl;

        // Add font to the list of fonts to be subset. This example only subsets System
        // fonts that are not embedded in this PDF document. The sample will not subset fonts
        // that are fully embedded in the PDF file.
        if (fontIsSysFont && !fontEmbedded) {
            pContainerOfFonts pcf = (pContainerOfFonts)clientData;
            cosFont = PDFontGetCosObj(pdFont);
            pcf->push_back(new _t_pdfUsedFont(cosFont));
        }
    HANDLER
        std::cout << "Exception raised in GetFontInfoProc(): ";
        APDFLib::displayError(ERRORCODE);
    END_HANDLER

    return true;
}
