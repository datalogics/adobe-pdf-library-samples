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

#ifndef TEXTEXTRACT_H
#define TEXTEXTRACT_H

#include "PDCalls.h"
#include "ASCalls.h"
#include "CosCalls.h"
#include "ASExtraCalls.h"

#include <string>
#include <vector>
#include <map>

// Regular expression patterns that follow the ECMAScript syntax.
// https://cplusplus.com/reference/regex/ECMAScript/

// These are samples of what can be done.
enum regexEnum {
    PHONE_PATTERN,
    EMAIL_PATTERN,
    URL_PATTERN,
    JAPANESE_PATTERN,
    FRENCH_PATTERN,
    KOREAN_PATTERN,
    RUSSIAN_PATTERN
};

static std::map<regexEnum, const char *> regexPattern = {
    {PHONE_PATTERN, R"((1-)?(\()?\d{3}(\))?(\s)?(-)?\d{3}-\d{4})"},
    {EMAIL_PATTERN, R"(\b[\w.!#$%&'*+\/=?^`{|}~-]+@[\w-]+(?:\.[\w-]+)*\b)"},
    {URL_PATTERN, R"((https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}))"},
    {JAPANESE_PATTERN, R"(『世界人権宣言』)"},
    {FRENCH_PATTERN, R"(Déclaration universelle des droits de l'homme)"},
    {KOREAN_PATTERN, R"(세 계 인 권 선 언)"},
    {RUSSIAN_PATTERN, R"(Всеобщая декларация прав человека)"}};

typedef struct {
    float h;
    float v;
} DLPointFloat, *DLPointFloatP;

typedef struct {
    DLPointFloat tl, tr, bl, br;
} DLQuadFloat, *DLQuadFloatP;

typedef struct {
    std::string DLSpace;
    float DLColor[4];
} DLColorValue, *DLColorValueP;

typedef struct {
    int charIndex;
    float fontsize;
    std::string fontname;
    DLColorValue colorValues;
} DLStyle, *DLStyleP;

/** Structure representing only the text  */
typedef struct {
    std::string text;
} PDTextExtractRec;

/** Structure representing the text and details */
typedef struct {
    std::string text;
    std::vector<DLQuadFloat> boundingQuads;
    std::vector<DLStyle> styles;
} PDTextAndDetailsExtractRec;

/** Structure representing the AcroForm text info */
typedef struct {
    std::string text;
    std::string fieldName;
} PDAcroFormExtractRec;

/** Structure representing the Annotation text info */
typedef struct {
    std::string type;
    std::string text;
} PDAnnotationExtractRec;

class TextExtract {

  private:
    PDDoc pDoc = nullptr;
    PDWordFinderConfigRec wfConfig;
    PDWordFinder wordFinder = nullptr;
    ASInt32 numWords = 0;
    PDWord wordArray = nullptr;

  public:
    TextExtract(PDDoc inPDoc);
    ~TextExtract();
    std::vector<PDTextExtractRec> GetText();
    std::vector<PDTextExtractRec> GetText(ASInt32 pageNum);
    std::vector<PDTextAndDetailsExtractRec> GetTextAndDetails();
    std::vector<PDTextAndDetailsExtractRec> GetTextAndDetails(ASInt32 pageNum);
    std::vector<PDAcroFormExtractRec> GetAcroFormFieldData();
    std::vector<PDAnnotationExtractRec> GetAnnotationText();
    void SetupWordFinderParams();
};

#endif
