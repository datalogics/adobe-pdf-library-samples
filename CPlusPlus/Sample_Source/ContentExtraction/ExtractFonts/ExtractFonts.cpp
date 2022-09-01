//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample is similar to EmbedFonts. The sample looks for fonts embedded in a PDF document and then
// extracts those fonts and saves them as font files, storing them in the same directory on the local
// system where the input PDF document is found.
//
// The ExtractFonts sample does not define a default output file.
//
// For more information about working with fonts in the Adobe PDF Library, see the description of the EmbedFonts sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#embedfonts

#include "CosCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "CopyContent.pdf"

ASBool ExtractEmbeddedFonts(CosObj obj, CosObj value, void *clientData);

int main(int argc, char *argv[]) {
    // Initialize the PDF library
    APDFLib libInit;
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::cout << "Will extract fonts found in document " << csInputFile.c_str() << std::endl;

    DURING
        APDFLDoc apdflDoc(csInputFile.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();
        // Enumerate font resources found in document
        PDDocEnumResources(pdDoc, 0, PDDocGetNumPages(pdDoc) - 1, ASAtomFromString("Font"),
                           ExtractEmbeddedFonts, NULL);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}

ASBool ExtractEmbeddedFonts(CosObj obj, CosObj value, void *clientData) {
    if (CosObjGetType(obj) != CosDict) {
        return (true);
    }

    if (CosDictKnown(obj, ASAtomFromString("DescendantFonts"))) {
        // Only type 0 fonts have a DescendantFonts entry, which is an
        // array of other fonts, and it will be recursive.
        // In practice, it will always be a single entry array, pointing to a CID font.
        //
        // The Type 0/Original Composite Font (OCF) format is a composite font designed
        // to support a character set with a large number of glyphs, particularly Asian
        // languages like Korean, Japanese, and Mandarin.
        //
        // Adobe Systems developed the Character Identifier Font (CID) to improve the
        // performance the OCF format.

        CosObj descendants = CosDictGet(obj, ASAtomFromString("DescendantFonts"));
        for (int index = 0; index < CosArrayLength(descendants); index++) {
            ExtractEmbeddedFonts(CosArrayGet(descendants, index), CosNewNull(), clientData);
        }
        return (true);
    }

    if (CosDictKnown(obj, ASAtomFromString("FontDescriptor"))) {
        CosObj descriptor = CosDictGet(obj, ASAtomFromString("FontDescriptor"));
        ASAtom nameAtom = CosNameValue(CosDictGet(descriptor, ASAtomFromString("FontName")));
        std::string sName(ASAtomGetString(nameAtom));

        ASAtom subType = CosNameValue(CosDictGet(obj, ASAtomFromString("Subtype")));

        CosObj fontData;
        if (CosDictKnown(descriptor, ASAtomFromString("FontFile"))) {
            fontData = CosDictGet(descriptor, ASAtomFromString("FontFile"));
            sName += ".pfb";
        } else {
            if (CosDictKnown(descriptor, ASAtomFromString("FontFile2"))) {
                fontData = CosDictGet(descriptor, ASAtomFromString("FontFile2"));
                sName += ".ttf";
            } else {
                if (CosDictKnown(descriptor, ASAtomFromString("FontFile3"))) {
                    fontData = CosDictGet(descriptor, ASAtomFromString("FontFile3"));
                    if ((subType == ASAtomFromString("Type1C")) || (subType == ASAtomFromString("Type1")) ||
                        (subType == ASAtomFromString("CIDFontType0")) ||
                        (subType == ASAtomFromString("CIDFontType0C"))) {
                        sName += ".pfb";
                    }
                    if ((subType == ASAtomFromString("OpenType")) ||
                        (subType == ASAtomFromString("CIDFontType2")) ||
                        (subType == ASAtomFromString("TrueType"))) {
                        sName += ".otf";
                    }
                } else {
                    return true;
                }
            }
        }

        ASPathName fontName = APDFLDoc::makePath(sName.c_str());
        ASFile fontFile;
        ASFileSysOpenFile(NULL, fontName, ASFILE_WRITE | ASFILE_CREATE, &fontFile);
        ASFileSysReleasePath(NULL, fontName);

        ASStm fontStm = CosStreamOpenStm(fontData, cosOpenFiltered);
        while (1) {
            char buffer[4096];
            int bytesRead;

            bytesRead = ASStmRead(buffer, 1, sizeof(buffer), fontStm);
            if (bytesRead == 0)
                break;
            ASFileWrite(fontFile, buffer, bytesRead);
        }
        ASFileClose(fontFile);
        ASStmClose(fontStm);
        std::cout << "Extracted the font " << sName.c_str() << std::endl;
        ;
    }
    return true;
}
