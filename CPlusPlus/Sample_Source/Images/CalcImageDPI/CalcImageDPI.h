//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// CalcImageDPI.h
//

std::ostream &operator<<(std::ostream &o, const ASFixedMatrix &m);

typedef struct imageentry {
    ASInt16 pageNumb;
    ASInt16 imageNumb;
    ASFixedMatrix currentMatrix; // The matrix in effect when we did the call
    ASFixedMatrix elementMatrix; // The matrix used when we called the image
    ASFixedMatrix finalMatrix;   // The matrix that will actually apply when we draw the image
    double hRotation;            // The horizontal rotation
    double vRotation;            // The vertical rotation
    ASBool sheared;              // The horizontal and vertical angles do not agree
    ASFixedMatrix erectHMatrix;  // The matrix if we de-skew horizontally
    ASFixedMatrix erectVMatrix;  // The matrix if we de-skew vertically
    double xShear;               // The angle of shear horizontally
    double yShear;               // The angle of shear vertically
    double widthInPoints;        // The image width in Points
    double depthInPoints;        // The depth of the image in Points
    PDEImageAttrs attrs;         // The attributes take from the image
    double hDPI;                 // Horizontal DPI
    double vDPI;                 // Vertical DPI
    friend std::ostream &operator<<(std::ostream &os, const imageentry &ie);
} ImageEntry;

void WalkContent(PDEContent, ASFixedMatrix *, double, ASInt16, ASInt16 *, std::vector<ImageEntry> &);

void FindImageDPI(PDEImage image, ASFixedMatrix *matrix, double userScale, ASInt16 pageNumb,
                  ASInt16 sequence, std::vector<ImageEntry> &imageTable);

std::ostream &operator<<(std::ostream &o, const ASFixedMatrix &m) {
    o << "[" << std::fixed << std::setprecision(2) << ASFixedToFloat(m.a) << ", "
      << ASFixedToFloat(m.b) << ", " << ASFixedToFloat(m.c) << ", " << ASFixedToFloat(m.d) << ", "
      << ASFixedToFloat(m.h) << ", " << ASFixedToFloat(m.v) << "]";
    return o;
}

std::ostream &operator<<(std::ostream &ofs, const imageentry &image) {
    ofs << "Image (Page " << image.pageNumb << ", seq " << image.imageNumb << ") In Form at ";
    ofs << image.currentMatrix << " Image At " << image.elementMatrix << std::endl;
    ofs << "        Image rotated " << image.hRotation << " Horiz and " << image.vRotation
        << " vertical." << std::endl;
    ofs << "        Erect H Matrix is " << image.erectHMatrix << " Erect V Matrix is "
        << image.erectVMatrix << std::endl;
    ofs << "        Image was " << image.widthInPoints << " Points wide and " << image.depthInPoints
        << " Points deep." << std::endl;
    ofs << "        Image was " << image.attrs.width << " Pixels wide and " << image.attrs.height
        << " pixels deep." << std::endl;
    ofs << "     for a DPI of " << image.hDPI << " horiz and " << image.vDPI << " vertical." << std::endl
        << std::endl;
    return ofs;
}
