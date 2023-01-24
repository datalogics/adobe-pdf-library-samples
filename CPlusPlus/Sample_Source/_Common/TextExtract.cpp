// Copyright (c) 2023, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
//===============================================================================
// Sample: TextExtract -This class is intended to assist with operations common to
// text extraction samples. The class contains methods to control types of words
// found and what information is returned to the user.
//
// TextExtract.cpp: Contains implementations of methods.
// TextExtract.h: Contains class definition.
//===============================================================================

#include "TextExtract.h"
#include <vector>
#include <sstream>

static void EnumerateAcroFormField(CosObj fieldObj, std::string prefix, std::vector<PDAcroFormExtractRec> &returnText);

//==============================================================================================================================
// Default Constructor - This creates a new TextExtract object.
//==============================================================================================================================

TextExtract::TextExtract(PDDoc inPDoc) {
    pDoc = inPDoc;
    void SetupWordFinderParams();
    wordFinder = PDDocCreateWordFinderEx(pDoc, WF_LATEST_VERSION, true, &wfConfig);
}

//==============================================================================================================================
// ~TextExtract() - Releases resources if they haven't already been freed.
//==============================================================================================================================

TextExtract::~TextExtract() {
    if (wordFinder != nullptr) {
        PDWordFinderDestroy(wordFinder);
    }
}

//==============================================================================================================================
// SetupWordFinderParams() - Setup params for WordFinder.  User can modify based on needs.
//==============================================================================================================================

void TextExtract::SetupWordFinderParams() {
    memset(&wfConfig, 0, sizeof(PDWordFinderConfigRec));
    wfConfig.recSize = sizeof(PDWordFinderConfigRec);

    wfConfig.disableTaggedPDF = false;
    wfConfig.noXYSort = false;
    wfConfig.preserveSpaces = false;
    wfConfig.noLigatureExp = false;
    wfConfig.noEncodingGuess = false;
    wfConfig.unknownToStdEnc = false;
    wfConfig.ignoreCharGaps = false;
    wfConfig.ignoreLineGaps = false;
    wfConfig.noAnnots = false;
    wfConfig.noHyphenDetection = false;
    wfConfig.trustNBSpace = false;
    wfConfig.noExtCharOffset = false;
    wfConfig.noStyleInfo = false;
    wfConfig.decomposeTbl = NULL;
    wfConfig.decomposeTblSize = 0;
    wfConfig.charTypeTbl = NULL;
    wfConfig.charTypeTblSize = 0;
    wfConfig.preserveRedundantChars = false;
    wfConfig.disableCharReordering = false;
    wfConfig.noSkewedQuads = false;
    wfConfig.noTextRenderMode3 = false;
    wfConfig.preciseQuad = false;
}

//==============================================================================================================================
// GetText() - Gets the text for the entire document.
//==============================================================================================================================

std::vector<PDTextExtractRec> TextExtract::GetText() {
    std::vector<PDTextExtractRec> returnText;

    ASInt32 numPages = PDDocGetNumPages(pDoc);

    for (ASInt32 pageNum = 0; pageNum < numPages; ++pageNum) {
        std::vector<PDTextExtractRec> pageText = GetText(pageNum);
        returnText.insert(returnText.end(), std::make_move_iterator(pageText.begin()),
                          std::make_move_iterator(pageText.end()));
    }
    return returnText;
}

//==============================================================================================================================
// GetText() - Gets the text on a specified page.
//==============================================================================================================================

std::vector<PDTextExtractRec> TextExtract::GetText(ASInt32 pageNum) {
    std::vector<PDTextExtractRec> returnText;
    PDWordFinderAcquireWordList(wordFinder, pageNum, &wordArray, NULL, NULL, &numWords);
    for (ASInt32 wordNum = 0; wordNum < numWords; ++wordNum) {

        PDWord pdWord = PDWordFinderGetNthWord(wordFinder, wordNum);
        ASText asTextWord = ASTextNew();
        PDWordGetASText(pdWord, 0, asTextWord);

        // Get the endian neutral UTF-8 string.
        ASUTF8Val *utf8String = reinterpret_cast<ASUTF8Val *>(ASTextGetUnicodeCopy(asTextWord, kUTF8));

        PDTextExtractRec record;
        record.text = reinterpret_cast<char *>(utf8String);
        ASTextDestroy(asTextWord);
        ASfree(utf8String);
        returnText.emplace_back(record);
    }
    PDWordFinderReleaseWordList(wordFinder, pageNum);
    return returnText;
}

//==============================================================================================================================
// GetTextAndDetails() - Gets the text and detail info for the entire document.
//==============================================================================================================================

std::vector<PDTextAndDetailsExtractRec> TextExtract::GetTextAndDetails() {
    std::vector<PDTextAndDetailsExtractRec> returnText;

    ASInt32 numPages = PDDocGetNumPages(pDoc);

    for (ASInt32 pageNum = 0; pageNum < numPages; ++pageNum) {
        std::vector<PDTextAndDetailsExtractRec> pageText = GetTextAndDetails(pageNum);
        returnText.insert(returnText.end(), std::make_move_iterator(pageText.begin()),
            std::make_move_iterator(pageText.end()));
    }
    return returnText;
}

//==============================================================================================================================
// GetTextAndDetails() - Gets the text and detail info for a specific page.
//==============================================================================================================================

std::vector<PDTextAndDetailsExtractRec> TextExtract::GetTextAndDetails(ASInt32 pageNum) {
    std::vector<PDTextAndDetailsExtractRec> returnText;
    PDWordFinderAcquireWordList(wordFinder, pageNum, &wordArray, NULL, NULL, &numWords);
    for (ASInt32 wordNum = 0; wordNum < numWords; ++wordNum) {

        PDWord pdWord = PDWordFinderGetNthWord(wordFinder, wordNum);
        ASText asTextWord = ASTextNew();
        PDWordGetASText(pdWord, 0, asTextWord);

        // Get the endian neutral UTF-8 string.
        ASUTF8Val *utf8String = reinterpret_cast<ASUTF8Val *>(ASTextGetUnicodeCopy(asTextWord, kUTF8));

        PDTextAndDetailsExtractRec record;
        record.text = reinterpret_cast<char *>(utf8String);
        ASTextDestroy(asTextWord);
        ASfree(utf8String);

        ASInt32 numQuads = PDWordGetNumQuads(pdWord);

        // A Word typically has only 1 quad, but can have more than one for hyphenated words, words on a curve, etc.
        for (ASInt32 quadNum = 0; quadNum < numQuads; ++quadNum) {

            ASFixedQuad wordQuad;
            PDWordGetNthQuad(pdWord, quadNum, &wordQuad);
            DLQuadFloat floatQuad;
            floatQuad.bl.h = ASFixedToFloat(wordQuad.bl.h);
            floatQuad.br.h = ASFixedToFloat(wordQuad.br.h);
            floatQuad.tl.h = ASFixedToFloat(wordQuad.tl.h);
            floatQuad.tr.h = ASFixedToFloat(wordQuad.tr.h);

            floatQuad.bl.v = ASFixedToFloat(wordQuad.bl.v);
            floatQuad.br.v = ASFixedToFloat(wordQuad.br.v);
            floatQuad.tl.v = ASFixedToFloat(wordQuad.tl.v);
            floatQuad.tr.v = ASFixedToFloat(wordQuad.tr.v);
            record.boundingQuads.emplace_back(floatQuad);
        }

        PDStyle pdStyle;
        ASInt16 transTbl[100];
        PDColorValueRec pdStyleColor;

        ASInt16 iRet = PDWordGetStyleTransition(pdWord, transTbl, 100);
        if (iRet) {
            for (int styleIndex = 0; styleIndex < iRet; ++styleIndex) {
                DLStyle dlstyle;
                dlstyle.charIndex = transTbl[styleIndex];
                pdStyle = PDWordGetNthCharStyle(wordFinder, pdWord, styleIndex);
                PDStyleGetColor(pdStyle, &pdStyleColor);
                switch (pdStyleColor.space) {
                case PDDeviceGray:
                    dlstyle.colorValues.DLSpace = "DeviceGray";
                    break;
                case PDDeviceRGB:
                    dlstyle.colorValues.DLSpace = "DeviceRGB";
                    break;
                case PDDeviceCMYK:
                    dlstyle.colorValues.DLSpace = "DeviceCMYK";
                    break;
                default:
                    dlstyle.colorValues.DLSpace = "Invalid";
                }

                dlstyle.colorValues.DLColor[0] = ASFixedToFloat(pdStyleColor.value[0]);
                dlstyle.colorValues.DLColor[1] = ASFixedToFloat(pdStyleColor.value[1]);
                dlstyle.colorValues.DLColor[2] = ASFixedToFloat(pdStyleColor.value[2]);
                dlstyle.colorValues.DLColor[3] = ASFixedToFloat(pdStyleColor.value[3]);

                dlstyle.fontsize = ASFixedToFloat(PDStyleGetFontSize(pdStyle));

                PDFont pdFont = PDStyleGetFont(pdStyle);
                char fontNameBuf[PSNAMESIZE];
                PDFontGetName(pdFont, fontNameBuf, PSNAMESIZE);
                ASBool fontEmbedded = PDFontIsEmbedded(pdFont);
                ASBool fontSubset = false;
                char *fontNameStart = 0;
                // Subset test: a font was subset if the 7th character is '+' (a plus-sign),
                // according to Acrobat/Reader and industry norms.
                if (fontEmbedded) {
                    if ((strlen(fontNameBuf)) > 7 && (fontNameBuf[6] == '+'))
                        fontSubset = true;
                }
                if (fontSubset)
                    fontNameStart = fontNameBuf + 7; // skip the "ABCDEF+"
                else
                    fontNameStart = fontNameBuf;
                dlstyle.fontname = fontNameStart;
                record.styles.emplace_back(dlstyle);
            }
        }
        returnText.emplace_back(record);
    }
    PDWordFinderReleaseWordList(wordFinder, pageNum);
    return returnText;
}

//==============================================================================================================================
// GetAcroFormFieldData() - Gets the AcroForm field data.
//==============================================================================================================================

std::vector<PDAcroFormExtractRec> TextExtract::GetAcroFormFieldData() {
    std::vector<PDAcroFormExtractRec> returnText;
    CosObj rootObj = CosDocGetRoot(PDDocGetCosDoc(pDoc));
    CosObj acroFormObj = CosDictGet(rootObj, ASAtomFromString("AcroForm"));
    if (CosObjGetType(acroFormObj) == CosNull) {
        return returnText;
    } else {
        CosObj fieldsObj = CosDictGet(acroFormObj, ASAtomFromString("Fields"));
        if ((CosObjGetType(fieldsObj) != CosArray) || (CosArrayLength(fieldsObj) == 0)) {
            return returnText;
        } else {
            for (ASInt32 fieldIndex = 0; fieldIndex < CosArrayLength(fieldsObj); ++fieldIndex) {
                CosObj fieldObj = CosArrayGet(fieldsObj, fieldIndex);
                EnumerateAcroFormField(fieldObj, "", returnText);
            }
        }
    }
    return returnText;
}

static void EnumerateAcroFormField(CosObj fieldObj, std::string prefix, std::vector<PDAcroFormExtractRec> &returnText) {

    std::string field_name;
    ASTCount textLength;

    if (CosObjGetType(fieldObj) == CosDict) {
        if (CosDictKnown(fieldObj, ASAtomFromString("T"))) {
            CosObj entryObj = CosDictGet(fieldObj, ASAtomFromString("T"));
            if (CosObjGetType(entryObj) == CosString) {
                std::string name_part(CosStringValue(entryObj, &textLength));
                if (prefix == "") {
                    field_name = name_part;
                } else {
                    std::ostringstream stringStream;
                    stringStream << prefix << "." << name_part;
                    field_name = stringStream.str();
                }
                // Process the Kids
                CosObj kidsObj = CosDictGet(fieldObj, ASAtomFromString("Kids"));
                if (CosObjGetType(kidsObj) == CosArray) {
                    for (ASInt32 kidIndex = 0; kidIndex < CosArrayLength(kidsObj); ++kidIndex) {
                        CosObj fieldObj = CosArrayGet(kidsObj, kidIndex);
                        EnumerateAcroFormField(fieldObj, field_name, returnText);
                    }
                }

                // Process this node
                CosObj nameObj = CosDictGet(fieldObj, ASAtomFromString("FT"));
                if (CosObjGetType(nameObj) == CosName) {
                    if (CosNameValue(nameObj) == ASAtomFromString("Tx")) {
                        PDAcroFormExtractRec record;
                        record.fieldName = field_name;
                        if (CosDictKnown(fieldObj, ASAtomFromString("V"))) {
                            CosObj entryValueObj = CosDictGet(fieldObj, ASAtomFromString("V"));
                            std::string textString(CosStringValue(entryValueObj, &textLength));
                            ASText asText = ASTextFromSizedPDText(textString.c_str(), textLength);
                            char *textStr = reinterpret_cast<char *>(ASTextGetUnicodeCopy(asText, kUTF8));
                            record.text = textStr;
                        }
                        returnText.emplace_back(record);
                    }
                }
            }
        }
    }
}

//==============================================================================================================================
// GetAnnotationText() - Gets the Annotation text.
//==============================================================================================================================

std::vector<PDAnnotationExtractRec> TextExtract::GetAnnotationText() {
    std::vector<PDAnnotationExtractRec> returnText;

    ASInt32 numPages = PDDocGetNumPages(pDoc);
    const size_t buffersize = 1000;
    static char contentBuffer[buffersize];
    for (ASInt32 pageNum = 0; pageNum < numPages; ++pageNum) {
        PDPage annotPage = PDDocAcquirePage(pDoc, pageNum);

        int numAnnots = PDPageGetNumAnnots(annotPage);

        // Extract each annotation's text content (if any)
        for (int annotIndex = 0; annotIndex < numAnnots; ++annotIndex) {

            PDAnnot annotation = PDPageGetAnnot(annotPage, annotIndex);
            ASAtom subtype = PDAnnotGetSubtype(annotation);

            if (subtype == ASAtomFromString("Text") || subtype == ASAtomFromString("FreeText")) {
                PDTextAnnot nextAsText = CastToPDTextAnnot(annotation);
                PDTextAnnotGetContents(nextAsText, contentBuffer, buffersize);
                PDAnnotationExtractRec record;
                record.type = ASAtomGetString(subtype);
                record.text = contentBuffer;
                returnText.emplace_back(record);
            }
        }
        PDPageRelease(annotPage);
    }
    return returnText;
}
