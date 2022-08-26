//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

#include <sstream>

#include "ASCalls.h"
#include "PEWCalls.h"

#include "DisplayPDEContent.h"

std::string DisplayQuad(ASFixedQuad *Quad) {
    char LLX[20], LLY[20], ULX[20], ULY[20];
    char LRX[20], LRY[20], URX[20], URY[20];

    ASFixedToCString(Quad->bl.h, LLX, 20, 5);
    ASFixedToCString(Quad->bl.v, LLY, 20, 5);
    ASFixedToCString(Quad->tl.h, ULX, 20, 5);
    ASFixedToCString(Quad->tl.v, ULY, 20, 5);
    ASFixedToCString(Quad->br.h, LRX, 20, 5);
    ASFixedToCString(Quad->br.v, LRY, 20, 5);
    ASFixedToCString(Quad->tr.h, URX, 20, 5);
    ASFixedToCString(Quad->tr.v, URY, 20, 5);
    std::ostringstream oss;
    oss << "[LL(" << LLX << ',' << LLY << "), UL(" << ULX << ',' << ULY << "), LR(" << LRX << ','
        << LRY << "), UR(" << URX << ',' << URY << ")]";
    return oss.str();
}

std::string DisplayMatrix(ASFixedMatrix *Matrix) {
    char a[20], b[20], c[20], d[20], h[20], v[20];

    ASFixedToCString(Matrix->a, a, 20, 5);
    ASFixedToCString(Matrix->b, b, 20, 5);
    ASFixedToCString(Matrix->c, c, 20, 5);
    ASFixedToCString(Matrix->d, d, 20, 5);
    ASFixedToCString(Matrix->h, h, 20, 5);
    ASFixedToCString(Matrix->v, v, 20, 5);
    std::ostringstream oss;
    oss << "[" << a << ", " << b << ", " << c << ", " << d << ", " << h << ", " << v << "]";
    return oss.str();
}

std::string DisplayFixed(ASFixed *Fixed) {
    char LocalText[120];
    ASFixedToCString(*Fixed, LocalText, 20, 5);
    return std::string(LocalText);
}

std::string AppendPoint(ASFixedPoint *Point, ASFixedMatrix *Matrix, ASBool MoreToCome) {
    ASFixedPoint Local;
    if (Matrix)
        ASFixedMatrixTransform(&Local, Matrix, Point);
    else {
        Local.h = Point->h;
        Local.v = Point->v;
    }

    char h[20], v[20];
    ASFixedToCString(Local.h, h, 20, 5);
    ASFixedToCString(Local.v, v, 20, 5);
    std::ostringstream oss;
    oss << "(" << h << ", " << v << ")" << (MoreToCome ? ", " : "");
    return oss.str();
}
