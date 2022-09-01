//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The UnicodeText sample demonstrates how the Library works with Unicode text.
// The sample converts hexadecimal characters into Unicode.
//
// Command-line:  <output-file>     (Optional)
//
// For more detail see the description of the UnicodeText sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#unicodetext

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "DLExtrasCalls.h"

// The number of fonts we're using. Must be accurate, of course.
#define NUM_FONTS_SAMPLE 3

// The number of Unicode text strings we'll draw. This must be accurate, of course.
#define NUM_TEXTS 5

#define DEF_OUTPUT "UnicodeText-out.pdf"

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csOutputFileName(argc > 1 ? argv[1] : DEF_OUTPUT);
    std::cout << "Will write 2 pages of assorted Unicode strings to " << csOutputFileName.c_str() << std::endl;

    // Step 1) Declare the Unicode strings.

    // The following are all the Unicode strings we intend to place. Note that every string
    //   lacks a BOM, lacks other identifiers, and is big endian.  These are all required
    //   characteristics for creating an ASText object from a Unicode string (minus the
    //   endianness - one could also use the host machine's endianness, but that is not
    //   guaranteed to be little-endian, so little endian is not demonstrated.)
    //
    // Our English text is in UTF-8.
    // This could have been a string literal, but for the sake of consistency:
    ASUTF8Val englishStr_U8[] = {0x55, 0x6E, 0x69, 0x76, 0x65, 0x72, 0x73, 0x61, 0x6C, 0x20,
                                 0x44, 0x65, 0x63, 0x6C, 0x61, 0x72, 0x61, 0x74, 0x69, 0x6F,
                                 0x6E, 0x20, 0x6F, 0x66, 0x20, 0x48, 0x75, 0x6D, 0x61, 0x6E,
                                 0x20, 0x52, 0x69, 0x67, 0x68, 0x74, 0x73, 0x00};

    // Our French text is in UTF-16B.
    ASUTF8Val frenchStr_U16B[] = {
        0x00, 0x44, 0x00, 0xE9, 0x00, 0x63, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x72, 0x00, 0x61,
        0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x20, 0x00, 0x75, 0x00, 0x6E,
        0x00, 0x69, 0x00, 0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x65, 0x00, 0x6C,
        0x00, 0x6C, 0x00, 0x65, 0x00, 0x20, 0x00, 0x64, 0x00, 0x65, 0x00, 0x73, 0x00, 0x20,
        0x00, 0x64, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x69, 0x00, 0x74, 0x00, 0x73, 0x00, 0x20,
        0x00, 0x64, 0x00, 0x65, 0x00, 0x20, 0x00, 0x6C, 0x00, 0x27, 0x00, 0x68, 0x00, 0x6F,
        0x00, 0x6D, 0x00, 0x6D, 0x00, 0x65, 0x00, 0x00};

    // Our Russian (cyrillic script) text is in UTF-16B.
    ASUTF8Val cyrillicStr_U16B[] = {
        0x04, 0x12, 0x04, 0x41, 0x04, 0x35, 0x04, 0x3E, 0x04, 0x31, 0x04, 0x49, 0x04, 0x30,
        0x04, 0x4F, 0x00, 0x20, 0x04, 0x34, 0x04, 0x35, 0x04, 0x3A, 0x04, 0x3B, 0x04, 0x30,
        0x04, 0x40, 0x04, 0x30, 0x04, 0x46, 0x04, 0x38, 0x04, 0x4F, 0x00, 0x20, 0x04, 0x3F,
        0x04, 0x40, 0x04, 0x30, 0x04, 0x32, 0x00, 0x20, 0x04, 0x47, 0x04, 0x35, 0x04, 0x3B,
        0x04, 0x3E, 0x04, 0x32, 0x04, 0x35, 0x04, 0x3A, 0x04, 0x30, 0x00, 0x00};

    // Our Japanese text is in UTF-16B.
    ASUTF8Val japaneseStr_U16B[] = {0x30, 0x0E, 0x4E, 0x16, 0x75, 0x4C, 0x4E, 0xBA, 0x6A,
                                    0x29, 0x5B, 0xA3, 0x8A, 0x00, 0x30, 0x0F, 0x00, 0x00};
    // Our Korean text is in UTF-16B.
    ASUTF8Val koreanStr_U16B[] = {0xC1, 0x38, 0x00, 0x20, 0xAC, 0xC4, 0x00, 0x20,
                                  0xC7, 0x78, 0x00, 0x20, 0xAD, 0x8C, 0x00, 0x20,
                                  0xC1, 0x20, 0x00, 0x20, 0xC5, 0xB8, 0x00, 0x00};

    DURING

        // Step 2) Load the necessary fonts.

        // Font objects for both vertical AND horizontal use:
        PDEFont courVFont, courHFont; // Courier - for English and French
        PDEFont kozgVFont, kozgHFont; // Kozgo - for Japanese Cyrillic
        PDEFont myunVFont, myunHFont; // Myungjo - for Korean

        // These arrays will be used to initialize our fonts. Each column here is for one font.
        //   They should all be in the PDFL distribution, and we use bosh horizontal and vertical.
        const char *fontNames[] = {"CourierStd", "KozGoPr6N-Medium", "AdobeMyungjoStd-Medium"};
        PDEFont *fontsV[] = {&courVFont, &kozgVFont, &myunVFont};
        PDEFont *fontsH[] = {&courHFont, &kozgHFont, &myunHFont};

        // These properties are common to all fonts, so are specified out here before the initialization loop.

        // The system encoding used to translate code points to font GIDs for horizontally-texts.
        PDSysEncoding iHEnc = PDSysEncodingCreateFromCMapName(ASAtomFromString("Identity-H"));
        // The same, for vertically-placed texts.
        PDSysEncoding iVEnc = PDSysEncodingCreateFromCMapName(ASAtomFromString("Identity-V"));
        // All our fonts are this type, otherwise this would have been another array.
        const char *fontType = "Type0";

        // The flags CreateEmbedded, WillSubset, and CreateToUnicode are promises
        //    on our part to do these things. See the "PDEFontSubsetNow" calls.
        PDEFontCreateFlags fontFlags = (PDEFontCreateFlags)(
            kPDEFontCreateEmbedded | kPDEFontWillSubset | kPDEFontCreateToUnicode | kPDEFontEncodeByGID);

        // This loop will load each font according to the properties just specified.
        //  A PDFL exception will be thrown here and caught in the handler if a font can't be found.
        PDEFontAttrs nextFontAttrs;
        PDSysFont nextSysFont;
        for (int i = 0; i < NUM_FONTS_SAMPLE; ++i) {
            memset(&nextFontAttrs, 0, sizeof(nextFontAttrs));
            nextFontAttrs.name = ASAtomFromString(fontNames[i]);
            nextFontAttrs.type = ASAtomFromString(fontType);

            nextSysFont = PDFindSysFont(&nextFontAttrs, sizeof(PDEFontAttrs), 0);

            *fontsV[i] = PDEFontCreateFromSysFontAndEncoding(nextSysFont, iVEnc, nextFontAttrs.name, fontFlags);
            *fontsH[i] = PDEFontCreateFromSysFontAndEncoding(nextSysFont, iHEnc, nextFontAttrs.name, fontFlags);
        }

        // Step 3) Prepare the text objects and check for font compatibility.

        ASText enAST = ASTextFromUnicode((ASUTF16Val *)englishStr_U8, kUTF8);

        // This takes advantage of the fact that all ASCII code points are identical to their
        //    UTF-8 counterparts. It's simpler if it's all ASText objects.
        ASText enTTL = ASTextFromUnicode((ASUTF16Val *)"English", kUTF8);
        ASText frAST = ASTextFromUnicode((ASUTF16Val *)frenchStr_U16B, kUTF16BigEndian);
        ASText frTTL = ASTextFromUnicode((ASUTF16Val *)"French", kUTF8);

        ASText jnAST = ASTextFromUnicode((ASUTF16Val *)japaneseStr_U16B, kUTF16BigEndian);
        ASText jnTTL = ASTextFromUnicode((ASUTF16Val *)"Japanese", kUTF8);

        ASText krAST = ASTextFromUnicode((ASUTF16Val *)koreanStr_U16B, kUTF16BigEndian);
        ASText krTTL = ASTextFromUnicode((ASUTF16Val *)"Korean", kUTF8);

        ASText ruAST = ASTextFromUnicode((ASUTF16Val *)cyrillicStr_U16B, kUTF16BigEndian);
        ASText ruTTL = ASTextFromUnicode((ASUTF16Val *)"Russian", kUTF8);

        struct textAndFont {
            ASText *text;   // The ASText object containing the text we'll draw to the PDF.
            ASText *title;  // The title for this font will be drawn with the horizontal text.
            PDEFont *fontH; // the horizontally-oriented font to use.
            PDEFont *fontV; // The vertically-oriented font to use.
        };

        // This array associates each ASText object with the vertical (V) and horizontal (H)
        //   fonts we want to use for it. This is the order they will be drawn in.
        textAndFont textsAndFonts[] = {
            {&enAST, &enTTL, &courHFont, &courVFont}, // The English ASText and associated fonts.
            {&jnAST, &jnTTL, &kozgHFont, &kozgVFont}, // Japanese...
            {&frAST, &frTTL, &courHFont, &courVFont}, // French...
            {&krAST, &krTTL, &myunHFont, &myunVFont}, // Korean...
            {&ruAST, &ruTTL, &kozgHFont, &kozgVFont}, // Russian...
        };

        // We need to check that each text is actually representable in the fonts we associated with it.
        //  It is entirely possible, and indeed usual, for fonts to not support every possible
        //  Unicode character, even for common languages.  We will however assume that Courier,
        //  the font we're using for the titles, can support the titles, since they're just ASCII.

        // This will be set to the index in the string of the first unrepresentable character
        ASUns32 firstBadGlyph = 0;
        for (int i = 0; i < NUM_TEXTS; ++i) {
            for (int f = 0; f < 2; f++) {
                const char *errString = "";
                PDEFont *nextFont = NULL;

                switch (f) {
                case (0):
                    errString = "horizontal";
                    nextFont = textsAndFonts[i].fontH;
                    break;
                case (1):
                    errString = "vertical";
                    nextFont = textsAndFonts[i].fontV;
                    break;
                }

                if (!PDEFontCheckASTextIsRepresentable(*nextFont, *textsAndFonts[i].text, &firstBadGlyph)) {
                    PDEFontAttrs badFontAttrs;
                    PDEFontGetAttrs(*nextFont, &badFontAttrs, sizeof(PDEFontAttrs));

                    std::cout << "Error: The " << errString << "font associated with the" << i << "th text "
                              << ASAtomGetString(badFontAttrs.name) << " has no glyph for the text's "
                              << firstBadGlyph << "th character." << std::endl;

                    return -1;
                }
            }
        }

        // Step 4) Prepare to draw the texts to a new document.

        // Create the output document
        APDFLDoc outDoc;

        // Insert 8.5" by 7.5" pages for the horizontal and vertical text.
        outDoc.insertPage(FloatToASFixed(8.5 * 72), FloatToASFixed(11 * 72), kPDEBeforeFirst);
        outDoc.insertPage(FloatToASFixed(8.5 * 72), FloatToASFixed(11 * 72), kPDEBeforeFirst);

        // Prepare first page for horizontal texts
        PDPage horzPage = outDoc.getPage(0);
        PDEContent horzPageCont = PDPageAcquirePDEContent(horzPage, 0);
        PDEText horzTexts = PDETextCreate();

        // Prepare first page for vertical texts
        PDPage vertPage = outDoc.getPage(1);
        PDEContent vertPageCont = PDPageAcquirePDEContent(vertPage, 0);
        PDEText vertTexts = PDETextCreate();

        // Retrieve the page bounds (it is the same for both pages, of course)
        ASFixedRect pageRect;
        PDPageGetCropBox(horzPage, &pageRect);

        // Matrices for placing text for each page
        ASFixedMatrix horzPlaceMatrix;
        memset(&horzPlaceMatrix, 0, sizeof(horzPlaceMatrix));
        ASFixedMatrix vertPlaceMatrix;
        memset(&vertPlaceMatrix, 0, sizeof(vertPlaceMatrix));

        // Both matrices may be initialized the same way to start out
        vertPlaceMatrix.a = horzPlaceMatrix.a = Int16ToFixed(14); // Font width, in points.
        vertPlaceMatrix.d = horzPlaceMatrix.d = Int16ToFixed(14); // Font height, in points.
        vertPlaceMatrix.h = horzPlaceMatrix.h = Int16ToFixed(1 * 72); // X coordinate on page: 1" from left side.
        vertPlaceMatrix.v = horzPlaceMatrix.v =
            pageRect.top - Int16ToFixed(1 * 72); // Y coordinate on page: 1" from the top.

        // We'll draw the text with whatever the default graphics state is.
        PDEGraphicState graphics;
        PDEDefaultGState(&graphics, sizeof(PDEGraphicState));

        // Step 5) Draw the texts, and subset the fonts we used.

        // Iterate through each ASText, placing both the horizontal and vertical forms,
        //   as well as the title for the horizontal text.
        for (int i = 0; i < NUM_TEXTS; ++i) {
            // The title for the horizontal text. We'll use the Courier horizontal font for consistency.
            PDETextAddASText(horzTexts, kPDETextRun, i, *textsAndFonts[i].title, courHFont,
                             &graphics, sizeof(graphics), NULL, 0, &horzPlaceMatrix);

            // Advance the y value by the height of the font, plus a little padding.
            horzPlaceMatrix.v -= horzPlaceMatrix.d + Int16ToFixed(10);

            // The horizontal text. This procedure converts the Unicode into Glyph IDs for specified font.
            PDETextAddASText(horzTexts, kPDETextRun, i + NUM_TEXTS, *textsAndFonts[i].text,
                             *textsAndFonts[i].fontH, &graphics, sizeof(graphics), NULL, 0, &horzPlaceMatrix);
            // Advance the y value by the height of the font, plus a lot of padding.
            horzPlaceMatrix.v -= horzPlaceMatrix.d + Int16ToFixed(30);

            // The vertical text. Same as above.
            PDETextAddASText(vertTexts, kPDETextRun, i, *textsAndFonts[i].text, *textsAndFonts[i].fontV,
                             &graphics, sizeof(graphics), NULL, 0, &vertPlaceMatrix);
            // Advance the x value by the width of the font, plus some padding.
            vertPlaceMatrix.h += vertPlaceMatrix.a + Int16ToFixed(20);
        }

        // Add the text elements to the content and place into the page
        PDEContentAddElem(horzPageCont, kPDEBeforeFirst, (PDEElement)horzTexts);
        PDPageSetPDEContentCanRaise(horzPage, 0);

        PDEContentAddElem(vertPageCont, kPDEBeforeFirst, (PDEElement)vertTexts);
        PDPageSetPDEContentCanRaise(vertPage, 0);

        // Simply calling "SubsetNow" subsets the font, creates a ToUnicode table (which maps Glyph IDs
        //  to Unicode), and creates widths. You don't need to call all three methods separately anymore.
        //  Remember, if you do this, you must use the kPDEFontWillSubset font flag during font retrieval,
        //  and vice versa.
        CosDoc cosDoc = PDDocGetCosDoc(outDoc.getPDDoc());
        for (int i = 0; i < NUM_TEXTS; ++i) {
            PDEFontSubsetNow(*textsAndFonts[i].fontV, cosDoc);
            PDEFontSubsetNow(*textsAndFonts[i].fontH, cosDoc);
        }

        // Step 6) Release resources, save, and close.

        // Release the text objects.
        PDERelease((PDEObject)horzTexts);
        PDERelease((PDEObject)vertTexts);

        // Release the PDEContents so we can release the pages.
        PDPageReleasePDEContent(horzPage, 0);
        PDPageReleasePDEContent(vertPage, 0);

        // Release the pages so we can close the document.
        PDPageRelease(horzPage);
        PDPageRelease(vertPage);

        // Destroy all the ASText objects we made.
        // We can't also release the fonts with textsAndFonts, since some texts use the same fonts.
        for (int i = 0; i < NUM_TEXTS; ++i) {
            ASTextDestroy(*textsAndFonts[i].text);
            ASTextDestroy(*textsAndFonts[i].title);
        }

        // Release the PDSysEncodings
        PDERelease((PDEObject)iHEnc);
        PDERelease((PDEObject)iVEnc);

        // Release all the fonts.
        for (int i = 0; i < NUM_FONTS_SAMPLE; ++i) {
            PDERelease((PDEObject)*fontsH[i]);
            PDERelease((PDEObject)*fontsV[i]);
        }

        // And graphics we used.
        PDERelease((PDEObject)graphics.fillColorSpec.space);
        PDERelease((PDEObject)graphics.strokeColorSpec.space);

        outDoc.saveDoc(csOutputFileName.c_str());

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}
