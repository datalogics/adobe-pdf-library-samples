//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample program demonstrates how to calculate the resolution for the images found in a PDF
// document. The sample scans the PDF input file and processes the images on each page one by one,
// rotating them as needed before calculating the Dots per Inch (DPI) for each image. Then, it
// lists the results in an output text file.
//
// For more detail see the description of the CalcImageDPI sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#calcimagedpi

#include <iostream>
#include <iomanip>
#include <fstream>
#include <cmath>
#include <vector>

#include "PDFInit.h"
#include "CosCalls.h"
#include "CorCalls.h"
#include "ASCalls.h"
#include "PDCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "PSFCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "CalcImageDPI-in.pdf"
#define OUTPUT_FILE "CalcImageDPI-out.txt"

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

#define degrees_to_radians (M_PI / 180.0)

#include "CalcImageDPI.h"

int main(int argc, char **argv) {
    // Initialize the library
    APDFLib libInit;
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        std::cout << "APDFL Library initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    // Set file names
    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : OUTPUT_FILE);

    std::ofstream ofs(csOutputFile.c_str());
    if (!ofs.is_open()) {
        std::cout << "Unable to open output file " << csOutputFile.c_str() << std::endl;
        return -6;
    }
    ofs << "CalcImageDPI: Results for file " << csInputFile.c_str() << std::endl;
    std::cout << "Analyzing file " << csInputFile.c_str() << ", writing results to "
              << csOutputFile.c_str() << std::endl;

    std::vector<ImageEntry> ImageTable;

    DURING
        // Open input file
        APDFLDoc apdflDoc(csInputFile.c_str(), true);
        PDDoc doc = apdflDoc.getPDDoc();

        // Process each page
        ASInt32 pages = PDDocGetNumPages(doc);
        for (ASInt32 pageNumb = 0; pageNumb < pages; pageNumb++) {
            PDPage page = PDDocAcquirePage(doc, pageNumb);
            PDEContent content = PDPageAcquirePDEContent(page, 0);

            // Get the page's matrix. If we use unity, then we will have to "un rotate" all the
            // upright images in a rotated page (which we will do just fine). If we use the upright
            // matrix, they they wil be unrotated automatically. Either works.
            ASFixedMatrix UprightImage = {fixedOne, 0, 0, fixedOne, 0, 0};
            PDPageGetDefaultMatrix(page, &UprightImage);

            // Fetch the user-assigned scale factor. It may be anything, but is usually used to
            // make the "1 unit in user space" larger then 1/72nd of an inch. Doing this allows the
            // ASFixedMatrix to describe a larger page.
            // But regardless of how or why this is used, it enters into the DPI calaculation by moving the
            // default unit size away from 1/72nd of an inch, so we must track it to find DPI.
            double userUnit = PDPageGetUserUnitSize(page);

            // Now, walk the content of the page, starting with this matrix and user unit
            ASInt16 sequence = 1;
            WalkContent(content, &UprightImage, userUnit, pageNumb + 1, &sequence, ImageTable);

            PDPageReleasePDEContent(page, 0);
            PDPageRelease(page);
        }

        // Report out the results
        size_t numEntries(ImageTable.size());
        ofs << "We found " << numEntries << " images" << std::endl;
        for (size_t index = 0; index < numEntries; index++) {
            ofs << ImageTable[index];
        }

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return (0);
}

// Rotate a matrix by a given amount
void ASFixedMatrixRotate(ASFixedMatrix *M, double Angle) {
    double Sina, Cosa;
    double Ma, Mb, Mc, Md;

    while (Angle < 0)
        Angle += 360;

    while (Angle > 360.0)
        Angle = Angle - 360.0;

    if (Angle < 0.0001)
        return;

    Angle *= degrees_to_radians;
    Ma = ASFixedToFloat(M->a);
    Mb = ASFixedToFloat(M->b);
    Mc = ASFixedToFloat(M->c);
    Md = ASFixedToFloat(M->d);
    Sina = sin(Angle);
    Cosa = cos(Angle);

    M->a = FloatToASFixed((Cosa * Ma) + (Sina * Mc));
    M->b = FloatToASFixed((Cosa * Mb) + (Sina * Md));
    M->c = FloatToASFixed((Cosa * Mc) - (Sina * Ma));
    M->d = FloatToASFixed((Cosa * Md) - (Sina * Mb));

    return;
}

// Scale a natrix by a given amount

void ASFixedMatrixScale(ASFixedMatrix *M, double ScaleX, double ScaleY) {
    M->a = FloatToASFixed(ASFixedToFloat(M->a) * ScaleX);
    M->b = FloatToASFixed(ASFixedToFloat(M->b) * ScaleX);
    M->c = FloatToASFixed(ASFixedToFloat(M->c) * ScaleY);
    M->d = FloatToASFixed(ASFixedToFloat(M->d) * ScaleY);
}

// A simple utility routine to assure that angle is within
// 0 to 359 degrees, and has no more then 2 decimal places of precision
// This makes comparing angles simpler
double NormalizeRotation(double Angle) {
    Angle = floor((Angle * 100) + 0.5) / 100;
    while (Angle >= 360)
        Angle -= 360;
    while (Angle < 0)
        Angle += 360;
    return Angle;
}

// Calculate the DPI for the image
void FindImageDPI(PDEImage image, ASFixedMatrix *matrix, double userScale, ASInt16 pageNumb,
                  ASInt16 sequence, std::vector<ImageEntry> &imageTable) {
    // Select an output area
    ImageEntry idata;

    // Identify the image
    idata.pageNumb = pageNumb;
    idata.imageNumb = sequence;

    // Save the current Matrix
    memmove((char *)&idata.currentMatrix, (char *)matrix, sizeof(ASFixedMatrix));

    // Save the elements matrix
    PDEElementGetMatrix((PDEElement)image, &idata.elementMatrix);

    // Find the final matrix used to set the image, as the matrix currently in effect
    // was concatenated to the matrix used to set the image
    ASFixedMatrixConcat(&idata.finalMatrix, &idata.elementMatrix, &idata.currentMatrix);

    // Find the angle between the current matrix, and erect, horizontally and vertically
    // ArcTangent of the vertical over horizontal components of the movement yields angle in radians, and can convert
    // easily to degrees. The horizontal movement components are b and d, where d is the vertical and b the horizontal.
    // See section 8.8.4 of the ISO 32000-1:2008, Document Management-Portable Document Format-Part 1:PDF 1.7,
    // "Transformation Matrices," page 119.
    //
    // So Arctangent b/a is the basis of the angle of a row in the image data
    idata.hRotation = atan2(ASFixedToFloat(idata.finalMatrix.b), ASFixedToFloat(idata.finalMatrix.a)) / degrees_to_radians;

    // Then we do the same thing, for the vertical movement (columns)
    // Normally, we would expect to get an angle of 90 degrees to the horizontal angle. However, because some images are scanned
    // top to bottom, and PDF expects data to be bottom to top, we see an angle of -90 degrees between the two. If we make the same
    // calculations, then we should rotate such that we will have a positive movement in both planes, regardless of skew or mirroring.
    idata.vRotation =
        atan2(ASFixedToFloat(idata.finalMatrix.d), ASFixedToFloat(idata.finalMatrix.c)) / degrees_to_radians - 90;

    // If the two angles are not the same, then we have a shear applied to one or the other of the
    // components (or both).  To find that shear, we need to de-rotate
    memmove((char *)&idata.erectHMatrix, (char *)&idata.finalMatrix, sizeof(ASFixedMatrix));
    memmove((char *)&idata.erectVMatrix, (char *)&idata.finalMatrix, sizeof(ASFixedMatrix));
    idata.sheared = false;
    idata.xShear = idata.yShear = 0;

    // If the horizontal and vertical rotations are not normal
    // (the same, or inverted), a skew is present. Detect the skew,
    // record it,and remove it.
    //
    // Complete the test with a set of angles normalized to be between 0 and 359, and
    // round the result to no more than two decimal points. This is enough precision, and
    // it avoids trying to make skew calculations with numbers showing very long floating
    // point decimal ranges. But preserve the real values for conversion.
    //
    // If the user has sheared in both planes, and rotated the image, the process to calculate
    // the skew value will fail.
    double testX = NormalizeRotation(idata.hRotation);
    double testY = NormalizeRotation(idata.vRotation);
    if ((testX != testY) && (fabs(testX - testY) != 180)) {
        idata.sheared = true;
        ASFixedMatrixRotate(&idata.erectHMatrix, idata.hRotation);
        if (idata.erectHMatrix.b != 0) {
            // We are sheared horizontally
            idata.xShear =
                -(atan2(ASFixedToFloat(idata.finalMatrix.d), ASFixedToFloat(idata.finalMatrix.b)) / degrees_to_radians) +
                90;
            memmove((char *)&idata.erectHMatrix, (char *)&idata.finalMatrix, sizeof(ASFixedMatrix));
            ASFixedMatrixRotate(&idata.erectHMatrix, -idata.xShear);
            idata.finalMatrix.b = idata.erectHMatrix.b;
            idata.xShear = floor((idata.xShear * 100) + 0.5) / 100;
            idata.hRotation =
                atan2(ASFixedToFloat(idata.finalMatrix.b), ASFixedToFloat(idata.finalMatrix.a)) / degrees_to_radians;
        }
        ASFixedMatrixRotate(&idata.erectVMatrix, -idata.vRotation);
        if (idata.erectVMatrix.c != 0) {
            idata.yShear =
                atan2(ASFixedToFloat(idata.finalMatrix.c), ASFixedToFloat(idata.finalMatrix.a)) / degrees_to_radians;

            memmove((char *)&idata.erectVMatrix, (char *)&idata.finalMatrix, sizeof(ASFixedMatrix));
            ASFixedMatrixRotate(&idata.erectVMatrix, idata.yShear);
            idata.finalMatrix.c = idata.erectVMatrix.c;
            idata.yShear = floor((idata.yShear * 100) + 0.5) / 100;

            idata.vRotation =
                atan2(ASFixedToFloat(idata.finalMatrix.d), ASFixedToFloat(idata.finalMatrix.c)) / degrees_to_radians - 90;
        }
    }

    // Normalize rotations between 0 and 360 with a lower (0.01) resolution
    idata.hRotation = NormalizeRotation(idata.hRotation);
    idata.vRotation = NormalizeRotation(idata.vRotation);

    if (idata.finalMatrix.a != 0)
        idata.widthInPoints = ASFixedToFloat(idata.finalMatrix.a) / cos(idata.hRotation * degrees_to_radians);
    else
        idata.widthInPoints = ASFixedToFloat(idata.finalMatrix.b) / sin(idata.hRotation * degrees_to_radians);

    if (idata.finalMatrix.d != 0)
        idata.depthInPoints = ASFixedToFloat(idata.finalMatrix.d) / cos(idata.vRotation * degrees_to_radians);
    else
        idata.depthInPoints = ASFixedToFloat(idata.finalMatrix.c) / -sin(idata.vRotation * degrees_to_radians);

    // Get the image attributes
    PDEImageGetAttrs(image, &idata.attrs, sizeof(PDEImageAttrs));

    // Horizontal DPI is width in pixels divided by width in points divided by 72.
    // Round these to an even integer, just to make comparison easier.
    //
    // User scale is a scale factor the page creator can apply to the page. It is normally
    // used for very large pages, to allow a larger range for an ASFixed value. When we
    // calculate this DPI, it is in user units. So we need to multiply by user scale to
    // find the actual DPI in absolute space.
    // UserScale is not used often, but it must be allowed for.
    idata.hDPI = floor(idata.attrs.width / (idata.widthInPoints / 72.0) + 0.5) * userScale;

    // Vertical DPI is depth in pixels divided by depth in points divided by 72.
    // Round these to an even integer, just to make comparision easier.
    idata.vDPI = floor(idata.attrs.height / (idata.depthInPoints / 72.0) + 0.5) * userScale;

    imageTable.push_back(idata);
}

// Walk through the elements in a single content, recursing into forms and containers,
// and calling FindImageDPI for any images seen.
//
// Keep track of matrix changes caused by calling through a form and
// carry the user scale amount from the root.
void WalkContent(PDEContent content, ASFixedMatrix *matrix, double userScale, ASInt16 pageNumb,
                 ASInt16 *sequence, std::vector<ImageEntry> &imageTable) {
    if (!content)
        return;

    // Process each element of content
    ASInt32 numbElems = PDEContentGetNumElems(content);
    for (ASInt32 elemNumb = 0; elemNumb < numbElems; elemNumb++) {
        PDEElement elem = PDEContentGetElem(content, elemNumb);
        PDEType type = (PDEType)PDEObjectGetType((PDEObject)elem);
        switch (type) {
        case kPDEImage:
            FindImageDPI((PDEImage)elem, matrix, userScale, pageNumb, *sequence, imageTable);
            (*sequence)++;
            break;

        case kPDEForm: {
            // For a form, obtain it's content, and scan it.
            // Displace it's position and scale by the form's parents.
            ASFixedMatrix innerMatrix;
            PDEElementGetMatrix(elem, &innerMatrix);
            ASFixedMatrixConcat(&innerMatrix, matrix, &innerMatrix);
            PDEContent inner = PDEFormGetContent((PDEForm)elem);
            WalkContent(inner, &innerMatrix, userScale, pageNumb, sequence, imageTable);
            // Rememeber to release the content
            PDERelease((PDEObject)inner);
            break;
        }

        case kPDEContainer: {
            // For a container, simply descend into the container.
            // Containers do not displace or scale. Nor does the
            // content of a container need to be released.
            PDEContent inner = PDEContainerGetContent((PDEContainer)elem);
            WalkContent(inner, matrix, userScale, pageNumb, sequence, imageTable);
            break;
        }

        default:
            // For current purposes, ignore everything else
            break;
        }
    }
    return;
}
