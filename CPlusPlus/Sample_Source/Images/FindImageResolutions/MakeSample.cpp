//
// Copyright (c) 2015-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This file contains the functions for producing a sample input file with images from
// a number of input graphics.
//

#include "ASExpT.h"
#include "ASCalls.h"
#include "CosCalls.h"
#include "PagePDECntCalls.h"
#include "PEWCalls.h"
#include "PERCalls.h"
#include "DLExtrasCalls.h"

#include <cstring>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

// Simple class to wrap scalars; we can use it to produce properly aligned
//   copies of variables that were serialied from a file in tightly packed structs
template <class _Type> struct Aligner {
    Aligner(_Type *data) { memcpy(&m_data, data, sizeof(_Type)); }
    operator _Type &() { return m_data; }
    Aligner<_Type> &operator=(const _Type rhs) {
        m_data = rhs;
        return *this;
    }
    _Type m_data;
};

typedef struct imagedesc {
    const char *m_name;
    PDEImageAttrs m_attrs;
    char m_padding[4];
    CosObj m_cosImage;
    PDESoftMask m_softMask;

} ImageDesc;

typedef enum duckytypes {
    DPI300 = 0,
    DPI438,
    DPI600,
    DPI1200,
    DPI2000,
    DPI2400,
    DPI300bg,
    DPI300Mask,
    DPI300bgStencil,
    DPI300sMask,
    DPI300bgSmask,
    DPI300SMForm
} DuckyTypes;

ImageDesc ImageTable[] = {
    {"DuckyFiles/PrivateDucky.jpg", {0, 2083, 2275, 8}},   // DPI300
    {"DuckyFiles/CorporalDucky.jpg", {0, 3041, 3321, 8}},  // DPI438
    {"DuckyFiles/SargentDucky.jpg", {0, 4166, 4550, 8}},   // DPI600
    {"DuckyFiles/CaptainDucky.jpg", {0, 8333, 9100, 8}},   // DPI1200
    {"DuckyFiles/ColonelDucky.jpg", {0, 13888, 15166, 8}}, // DPI2000
    {"DuckyFiles/GeneralDucky.jpg", {0, 16666, 18199, 8}}, // DPI2400
    {"DuckyFiles/CamoDucky.jpg", {0, 2083, 2275, 8}},      // DPI300bg
    {"DuckyFiles/ShadowDucky.bmp", {0, 2083, 2275, 8}},    // DPI300Mask
    {NULL, {0, 0, 0, 0}},        // DPI300bgStencil
    {NULL, {0, 0, 0, 0}},        // DPI300sMask
    {NULL, {0, 0, 0, 0}},        // DPI300bgSmask
    {NULL, {0, 0, 0, 0}}         // DPI300SMForm
};
#define ImageTableSize (sizeof(ImageTable) / sizeof(ImageDesc))

extern void DoubleMatrixRotate(ASDoubleMatrix *M, ASDouble Angle);

#ifndef WIN_ENV
#pragma pack(2)
// These are defined in wingdi.h for windows. They are
// included here as an aid to Unix systems
typedef ASInt8 CHAR;
typedef ASInt16 SHORT;
typedef ASInt32 LONG;
typedef ASUns32 DWORD;
typedef ASUns8 BYTE;
typedef ASUns16 WORD;
typedef float FLOAT;
typedef FLOAT *PFLOAT;
typedef ASInt32 INT;
typedef ASUns32 UINT;
typedef ASUns32 *PUINT;

// constants for the biCompression field
#define BI_RGB 0L
#define BI_RLE8 1L
#define BI_RLE4 2L
#define BI_BITFIELDS 3L
#define BI_JPEG 4L
#define BI_PNG 5L

typedef struct tagBITMAPFILEHEADER // Describes a bit map file
{
    WORD bfType;  // only "BM" will be handled
    DWORD bfSize; // Size in bytes of the File
    WORD bfReserved1;
    WORD bfReserved2;
    DWORD bfOffBits; // Displacement to start of map
} BITMAPFILEHEADER;

typedef struct tagBITMAPINFOHEADER // Describes the bit map contents
{
    DWORD biSize;         // Size of this structure
                          // The color table immediately follows this structure!
    LONG biWidth;         // In Pixels
    LONG biHeight;        // In Pixels
    WORD biPlanes;        // Always 1
    WORD biBitCount;      // 1 = Monochrome image  2 = color entries
                          // 4 = 16 Color entries
                          // 8 = 256 Color Entries
                          // 24 = 0 Color Entries
    DWORD biCompression;  // BI_RGB  (0) = uncompressed
                          // BI_RLE8 (1) = Run length encoded 8 BitsPerPixel
                          // BI_RLE4 (2) = Run Length Encoded 4 BitsPerPixel
    DWORD biSizeImage;    // Bytes in map, May be zero for BI_RGB
    LONG biXPelsPerMeter; // Desired original resolution
    LONG biYPelsPerMeter; // Desired original resolution
    DWORD biClrUsed;      // If zero, Number of entries is via
                          // biBitCount, else, this is number of entries
    DWORD biClrImportant; // Used only if lowering color count
} BITMAPINFOHEADER;

#endif // WIN_ENV

// Class to help with determining the host's endian-ness
class WhichEndian {
  public:
    static bool IsBigEndian() {
        if (!m_bDetermined) {
            // Lazy eval
            DetermineEndianness();
            m_bDetermined = true;
        }
        return m_bWeAreBigEndian;
    }
    static void Swap2(void *s) {
        unsigned char *c = (unsigned char *)s;
        unsigned char t = c[0];
        c[0] = c[1];
        c[1] = t;
    }
    static void Swap4(void *s) {
        unsigned char *c = (unsigned char *)s;
        unsigned char t = c[0];
        c[0] = c[3];
        c[3] = t;
        t = c[1];
        c[1] = c[2];
        c[2] = t;
    }

  private:
    static bool m_bDetermined;
    static bool m_bWeAreBigEndian;
    static void DetermineEndianness() {
        // Whatever the "sizeof" int is (as long as it is bigger than 1 byte, which
        //    is a valid assumption) the 1 bit (which is least significant) will
        //    always occupy the right-most byte if the host is big-endian (and 0
        //    will thus occupy the left-most)
        int x = 1;
        char *y = (char *)&x;
        m_bWeAreBigEndian = (0x00 == *y);
    }
};

bool WhichEndian::m_bDetermined = false;
bool WhichEndian::m_bWeAreBigEndian;

void CompleteImageTable(CosDoc doc) {
    for (size_t count = 0; count < ImageTableSize; count++) {
        ImageDesc *current = &ImageTable[count];
        memset(current->m_attrs.decode, 0, sizeof(current->m_attrs.decode));
        current->m_attrs.intent = 0;
        current->m_attrs.flags = kPDEImageExternal;

        if (current->m_name == NULL)
            break;

        ASPathName imagePath = APDFLDoc::makePath(current->m_name);
        ASFile imageFile;
        ASFileSysOpenFile64(NULL, imagePath, ASFILE_READ, &imageFile);

        ASSize_t fileSize = ASFileGetEOF(imageFile);

        if (strstr(current->m_name, ".jpg")) {
            ASStm fileStm = ASFileStmRdOpen(imageFile, 4096);

            CosObj jpegAttributes = CosNewDict(doc, false, 6);
            CosDictPutKeyString(jpegAttributes, "Type", CosNewNameFromString(doc, false, "XObject"));
            CosDictPutKeyString(jpegAttributes, "Subtype", CosNewNameFromString(doc, false, "Image"));
            CosDictPutKeyString(jpegAttributes, "Width", CosNewInteger(doc, false, current->m_attrs.width));
            CosDictPutKeyString(jpegAttributes, "Height", CosNewInteger(doc, false, current->m_attrs.height));
            CosDictPutKeyString(jpegAttributes, "ColorSpace", CosNewNameFromString(doc, false, "DeviceRGB"));
            CosDictPutKeyString(jpegAttributes, "BitsPerComponent",
                                CosNewInteger(doc, false, current->m_attrs.bitsPerComponent));
            CosDictPutKeyString(jpegAttributes, "Filter", CosNewNameFromString(doc, false, "DCTDecode"));

            current->m_cosImage =
                CosNewStream(doc, true, fileStm, 0, false, jpegAttributes, CosNewNull(), fileSize);

            ASStmClose(fileStm);
        } else {
            ASUns8 *buffer = (ASUns8 *)ASmalloc(fileSize);
            ASFileRead(imageFile, (char *)buffer, fileSize);
            BITMAPFILEHEADER *header = (BITMAPFILEHEADER *)buffer;
            BITMAPINFOHEADER *infoHeader = (BITMAPINFOHEADER *)&buffer[sizeof(BITMAPFILEHEADER)];
            if (WhichEndian::IsBigEndian()) {
                // The fields in the bitmap file's header are written Little-Endian.  If this is a
                // Big-Endian host, we much swap.
                WhichEndian::Swap4(&(header->bfOffBits));
                WhichEndian::Swap4(&(infoHeader->biHeight));
                WhichEndian::Swap4(&(infoHeader->biWidth));
                WhichEndian::Swap2(&(infoHeader->biBitCount));
                WhichEndian::Swap4(&(infoHeader->biSizeImage));
            }
            // For the benefit of host systems who cannot access integers packed on 2-byte boundaries,
            //    as the structures above are packed and read in from the file, we will safely copy to
            //    properly aligned locations
            Aligner<ASUns32> P_bfOffBits((ASUns32 *)&(header->bfOffBits));
            Aligner<ASInt32> P_biHeight((ASInt32 *)&(infoHeader->biHeight));
            Aligner<ASInt32> P_biWidth((ASInt32 *)&(infoHeader->biWidth));
            Aligner<ASUns16> P_biBitCount(&(infoHeader->biBitCount));
            Aligner<ASUns32> P_biSizeImage((ASUns32 *)&(infoHeader->biSizeImage));

            ASUns8 *data = (ASUns8 *)&buffer[P_bfOffBits];

            current->m_attrs.height = P_biHeight;
            current->m_attrs.width = P_biWidth;
            current->m_attrs.bitsPerComponent = P_biBitCount;

            CosObj bmpAttributes = CosNewDict(doc, false, 6);
            CosDictPutKeyString(bmpAttributes, "Type", CosNewNameFromString(doc, false, "XObject"));
            CosDictPutKeyString(bmpAttributes, "Subtype", CosNewNameFromString(doc, false, "Image"));
            CosDictPutKeyString(bmpAttributes, "Width", CosNewInteger(doc, false, current->m_attrs.width));
            CosDictPutKeyString(bmpAttributes, "Height", CosNewInteger(doc, false, current->m_attrs.height));
            CosDictPutKeyString(bmpAttributes, "BitsPerComponent",
                                CosNewInteger(doc, false, current->m_attrs.bitsPerComponent));
            CosDictPutKeyString(bmpAttributes, "ImageMask", CosNewBoolean(doc, false, true));
            CosObj decodeArray = CosNewArray(doc, false, 2);
            CosArrayPut(decodeArray, 0, CosNewDouble(doc, false, 1.0));
            CosArrayPut(decodeArray, 1, CosNewDouble(doc, false, 0.0));
            CosDictPutKeyString(bmpAttributes, "Decode", decodeArray);

            // We may need to compress the rows, if the row size is not an even multiple of 32.
            // And we will need to invert the image, so it is bottom up, rather than top down
            ASUns32 rowSize = ((current->m_attrs.width + 7) / 8);
            ASUns32 unpackedRowSize = P_biSizeImage / current->m_attrs.height;
            ASUns8 *newImage = (ASUns8 *)ASmalloc(rowSize * current->m_attrs.height);

            P_biSizeImage = rowSize * current->m_attrs.height;
            for (int count = 0; count < current->m_attrs.height; count++)
                memmove(&newImage[count * rowSize],
                        &data[(current->m_attrs.height - count - 1) * unpackedRowSize], rowSize);

            ASStm bmpStm = ASMemStmRdOpen((char *)newImage, P_biSizeImage);

            current->m_cosImage =
                CosNewStream(doc, true, bmpStm, 0, false, bmpAttributes, CosNewNull(), P_biSizeImage);

            ASStmClose(bmpStm);
            ASfree(buffer);
            ASfree(newImage);
        }
        ASFileClose(imageFile);
        ASFileSysReleasePath(NULL, imagePath);
    }

    // This will manufacture 4 more images.
    // The first will be the 300 dpi ducky with a radial background with the 300 DPI mask applied as a stencil mask.
    // The second will be the 300 DPI mask as a soft mask.
    // The third will the 300 DPI ducky with a radial background, with the 300 DPI mask applied as a soft mask.
    // The fourth will be the 300DPI mask, in a form, for use as a gState sMask

    ImageDesc *current = &ImageTable[DPI300bgStencil];
    ImageDesc *base = &ImageTable[DPI300bg];
    memmove(&current->m_attrs, &base->m_attrs, sizeof(PDEImageAttrs));
    current->m_cosImage = CosObjCopy(base->m_cosImage, doc, true);
    CosDictPutKeyString(current->m_cosImage, "Mask", ImageTable[DPI300Mask].m_cosImage);

    current = &ImageTable[DPI300sMask];
    base = &ImageTable[DPI300Mask];
    memmove(&current->m_attrs, &base->m_attrs, sizeof(PDEImageAttrs));
    current->m_cosImage = CosObjCopy(base->m_cosImage, doc, true);
    CosDictPutKeyString(current->m_cosImage, "ColorSpace", CosNewNameFromString(doc, false, "DeviceGray"));
    CosDictRemoveKeyString(current->m_cosImage, "ImageMask");
    CosDictRemoveKeyString(current->m_cosImage, "Decode");

    current = &ImageTable[DPI300bgSmask];
    base = &ImageTable[DPI300bg];
    memmove(&current->m_attrs, &base->m_attrs, sizeof(PDEImageAttrs));
    current->m_cosImage = CosObjCopy(base->m_cosImage, doc, true);
    CosDictPutKeyString(current->m_cosImage, "SMask", ImageTable[DPI300sMask].m_cosImage);

    current = &ImageTable[DPI300SMForm];
    base = &ImageTable[DPI300sMask];
    memmove(&current->m_attrs, &base->m_attrs, sizeof(PDEImageAttrs));
    PDEColorSpace rgbSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));
    ASDoubleMatrix unity = {static_cast<ASDouble>(base->m_attrs.width),  0, 0,
                            static_cast<ASDouble>(base->m_attrs.height), 0, 0};
    PDEImage smImage = PDEImageCreateFromCosObjEx(&base->m_cosImage, &unity, rgbSpace, NULL);
    PDEContent smContent = PDEContentCreate();
    PDEContentAddElem(smContent, kPDEAfterLast, (PDEElement)smImage);
    PDERelease((PDEObject)smImage);
    PDERelease((PDEObject)rgbSpace);

    CosObj smCos, smRes;
    PDEContentToCosObjEx(smContent, kPDEContentToForm, NULL, sizeof(PDEContentAttrs), doc, NULL, &smCos, &smRes);
    PDERelease((PDEObject)smContent);

    CosObj groupDict = CosNewDict(doc, false, 2);
    CosDictPutKeyString(groupDict, "CS", CosNewNameFromString(doc, false, "DeviceGray"));
    CosDictPutKeyString(groupDict, "S", CosNewNameFromString(doc, false, "Transparency"));
    CosDictPutKeyString(smCos, "Group", groupDict);
    CosDictPutKeyString(smCos, "Type", CosNewNameFromString(doc, false, "XObject"));
    CosDictPutKeyString(smCos, "Subtype", CosNewNameFromString(doc, false, "Form"));
    CosDictPutKeyString(smCos, "Resources", smRes);

    PDEForm smForm = PDEFormCreateFromCosObjEx(&smCos, &smRes, &unity);
    PDEFormCalcBBox(smForm);
    current->m_softMask = PDESoftMaskCreate(doc, kPDESoftMaskTypeAlpha, smForm);
    PDERelease((PDEObject)smForm);
    return;
}

void InsertImage(PDEContent content, ASInt32 imageIndex, double X, double Y, double wide,
                 double deep, double angle, ASBool mirrorH, ASBool mirrorV) {
    PDEColorSpace rgbSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));
    ASDoubleMatrix imageMatrix = {wide, 0, 0, deep, X + (wide / 2.0), Y + (deep / 2.0)};
    ASDoubleMatrix trans = {1, 0, 0, 1, 0, 0};
    ASDoublePoint place = {-wide / 2.0, -deep / 2.0};
    if (mirrorH) {
        imageMatrix.a = -wide;
        imageMatrix.h = X + (wide / 2.0);
        trans.a = 1;
        place.h = -wide / 2.0;
    }
    if (mirrorV) {
        imageMatrix.d = -deep;
        imageMatrix.v = Y + (deep / 2.0);
        trans.v = -1;
        place.v = -deep / 2.0;
    }
    DoubleMatrixRotate(&imageMatrix, angle);
    DoubleMatrixRotate(&trans, angle);
    ASDoubleMatrixTransform(&place, &trans, &place);
    if (mirrorH)
        imageMatrix.h -= place.h;
    else
        imageMatrix.h += place.h;
    if (mirrorV)
        imageMatrix.v -= place.v;
    else
        imageMatrix.v += place.v;

    PDEImage newImage =
        PDEImageCreateFromCosObjEx(&ImageTable[imageIndex].m_cosImage, &imageMatrix, rgbSpace, NULL);
    PDEContentAddElem(content, kPDEAfterLast, (PDEElement)newImage);

    PDERelease((PDEObject)newImage);
    PDERelease((PDEObject)rgbSpace);
}

// Create the image inside a form. The form will be set 1/2 size, the image twice size.
// The form with rotate twice angle, and the form will rotate -angle.
void InsertImageInForm(CosDoc doc, PDEContent content, ASInt32 imageIndex, double X, double Y,
                       double wide, double deep, double angle, ASBool sMask) {
    PDEContent formContent = PDEContentCreate();

    PDEColorSpace rgbSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));

    // Position the image in the form with it's center in the middle of the form.
    // Scale up by 2 (The form will be 1/2 size).
    ASDoubleMatrix imageMatrix = {wide * 2, 0, 0, deep * 2, wide, deep};

    // The form will be rotated -angle, so rotate the image double the angle
    DoubleMatrixRotate(&imageMatrix, angle * 2);
    ASDoubleMatrix trans = {1, 0, 0, 1, 0, 0};
    DoubleMatrixRotate(&trans, angle * 2);
    ASDoublePoint place = {-wide, -deep};
    ASDoubleMatrixTransform(&place, &trans, &place);
    imageMatrix.h += place.h;
    imageMatrix.v += place.v;

    PDEImage newImage =
        PDEImageCreateFromCosObjEx(&ImageTable[imageIndex].m_cosImage, &imageMatrix, rgbSpace, NULL);
    PDEContentAddElem(formContent, kPDEAfterLast, (PDEElement)newImage);

    PDERelease((PDEObject)newImage);
    PDERelease((PDEObject)rgbSpace);

    CosObj formCos, formResources;
    PDEContentToCosObjEx(formContent, kPDEContentToForm, NULL, sizeof(PDEContentAttrs), doc, NULL,
                         &formCos, &formResources);

    ASDoubleMatrix formMatrix = {0.5, 0, 0, 0.5, X + wide / 2, Y + deep / 2};
    DoubleMatrixRotate(&formMatrix, -angle);
    ASDoubleMatrix trans2 = {1.0, 0, 0, 1.0, 0, 0};
    DoubleMatrixRotate(&trans2, -angle);
    ASDoublePoint place2 = {-wide / 2, -deep / 2};
    ASDoubleMatrixTransform(&place2, &trans2, &place2);
    formMatrix.h += place2.h;
    formMatrix.v += place2.v;

    PDEForm form = PDEFormCreateFromCosObjEx(&formCos, &formResources, &formMatrix);
    PDEFormCalcBBox(form);

    if (sMask) {
        // Set a sMask of the ducky over the image
        PDEGraphicState gState;
        PDEDefaultGState(&gState, sizeof(PDEGraphicState));
        gState.extGState = PDEExtGStateCreateNew(doc);
        PDEExtGStateSetSoftMask(gState.extGState, ImageTable[DPI300SMForm].m_softMask);
        PDEExtGStateSetAIS(gState.extGState, true);
        PDEElementSetGState((PDEElement)form, &gState, sizeof(PDEGraphicState));
        PDERelease((PDEObject)gState.strokeColorSpec.space);
        PDERelease((PDEObject)gState.fillColorSpec.space);
        PDERelease((PDEObject)gState.extGState);
    }

    PDEContentAddElem(content, kPDEAfterLast, (PDEElement)form);
    PDERelease((PDEObject)form);
    PDERelease((PDEObject)formContent);
}

// Create the image inside a form.
// The form will be set double size, the second form 1/4 size, the image twice size.
// The form with rotate twice angle, and the form will rotate 3 * -angle, the image at double angle.
void InsertImageInFormInForm(CosDoc doc, PDEContent content, ASInt32 imageIndex, double X, double Y,
                             double wide, double deep, double angle) {
    PDEContent formContent = PDEContentCreate();

    PDEColorSpace rgbSpace = PDEColorSpaceCreateFromName(ASAtomFromString("DeviceRGB"));

    // Position the image in the form with it's center in the middle of the form.
    // Scale up by 2 (The form will be 1/2 size).
    ASDoubleMatrix imageMatrix = {wide * 2, 0, 0, deep * 2, wide, deep};

    // The form will be rotated -angle, so rotate the image double the angle
    DoubleMatrixRotate(&imageMatrix, angle * 2);
    ASDoubleMatrix trans = {1, 0, 0, 1, 0, 0};
    DoubleMatrixRotate(&trans, angle * 2);
    ASDoublePoint place = {-wide, -deep};
    ASDoubleMatrixTransform(&place, &trans, &place);
    imageMatrix.h += place.h;
    imageMatrix.v += place.v;

    PDEImage newImage =
        PDEImageCreateFromCosObjEx(&ImageTable[imageIndex].m_cosImage, &imageMatrix, rgbSpace, NULL);
    PDEContentAddElem(formContent, kPDEAfterLast, (PDEElement)newImage);

    PDERelease((PDEObject)newImage);
    PDERelease((PDEObject)rgbSpace);

    CosObj formCos, formResources;
    PDEContentToCosObjEx(formContent, kPDEContentToForm, NULL, sizeof(PDEContentAttrs), doc, NULL,
                         &formCos, &formResources);

    ASDoubleMatrix formMatrix = {0.25, 0, 0, 0.25, X + wide / 2, Y + deep / 2};
    DoubleMatrixRotate(&formMatrix, -angle * 3);
    ASDoubleMatrix trans2 = {1.0, 0, 0, 1.0, 0, 0};
    DoubleMatrixRotate(&trans2, -angle * 3);
    ASDoublePoint place2 = {-wide / 2, -deep / 2};
    ASDoubleMatrixTransform(&place2, &trans2, &place2);
    formMatrix.h += place2.h;
    formMatrix.v += place2.v;

    PDEForm form = PDEFormCreateFromCosObjEx(&formCos, &formResources, &formMatrix);
    PDEFormCalcBBox(form);
    PDERelease((PDEObject)content);

    CosObj form2Cos, form2Resources;
    PDEFormGetCosObj(form, &form2Cos);
    CosDictGetKeyString(form2Cos, "Resources");

    ASDoubleMatrix form2Matrix = {2, 0, 0, 2, X + wide / 2, Y + deep / 2};
    DoubleMatrixRotate(&formMatrix, angle * 2);
    ASDoubleMatrix trans3 = {1.0, 0, 0, 1.0, 0, 0};
    DoubleMatrixRotate(&trans3, angle * 3);
    ASDoublePoint place3 = {-wide / 2, -deep / 2};
    ASDoubleMatrixTransform(&place3, &trans3, &place3);
    formMatrix.h += place3.h;
    formMatrix.v += place3.v;

    PDEForm form2 = PDEFormCreateFromCosObjEx(&form2Cos, &form2Resources, &form2Matrix);
    PDEFormCalcBBox(form2);

    PDEContentAddElem(content, kPDEAfterLast, (PDEElement)form2);
    PDERelease((PDEObject)form);
    PDERelease((PDEObject)form2);
}

void ReleaseSample() {
    for (size_t count = 0; count < ImageTableSize; count++) {
        ImageDesc *current = &ImageTable[count];
        if (current->m_softMask != NULL) {
            PDERelease((PDEObject)current->m_softMask);
        }
    }
}

int MakeSample() {
    DURING
        // Make a sample file for find image resolutions
        APDFLDoc sample;

        // Make actual PDEImages
        CosDoc cosDoc = PDDocGetCosDoc(sample.getPDDoc());
        CompleteImageTable(cosDoc);

        // Create a page with 6 images
        // All images are Ducky, at various resolutions
        // 0.75 left and right bounds, 2 inch images, with 0.5 between
        // top row is 1 inch below page top, and 157.273 points deep (To retain aspect ratio
        // Second row is 4 inches below top and 157.273 points deep.
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), PDBeforeFirstPage);
        PDPage page = sample.getPage(0);
        PDEContent content = PDPageAcquirePDEContent(page, 0);

        ASDouble X = 0.75 * 72.0;
        ASDouble Y = 7 * 72.0;
        for (int count = 0; count < 6; count++) {
            InsertImage(content, count, X, Y, 144.0, 157.273, 0, false, false);
            X += 2.5 * 72;
            if (count == 2) {
                X = 0.75 * 72;
                Y -= 3.0 * 72.0;
            }
        }

        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 2 contains 25 duckies, all using the 300 DPI ducky
        // image. Each is 72 points by 78.6365. The first image is erect,
        // and each successive image is rotated 15 degrees counter clockwise
        // from the previous, through 360 degrees.
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImage(content, DPI300, X, Y, 72.0, 78.6365, angle, false, false);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 3 contains 25 duckies, all using the 300 DPI ducky with a background radial
        // stencil masked with the 300 DPI image mask, applied as a stencil mask
        // image. Each is 72 points by 78.6365. The first image is erect,
        // each successive image is rotated 15 degrees counter clockwise
        // from the previous, through 360 degrees.
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImage(content, DPI300bgStencil, X, Y, 72.0, 78.6365, angle, false, false);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 4 contains 25 duckies, all using the 300 DPI ducky with a background radial
        // soft masked with the 300 DPI image mask, applied as a stencil mask
        // image. Each is 72 points by 78.6365. The first image is erect,
        // each successive image is rotated 15 degrees counter clockwise
        // from the previous, through 360 degrees.
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImage(content, DPI300bgSmask, X, Y, 72.0, 78.6365, angle, false, false);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 5 contains 25 duckies, all using the 300 DPI ducky
        // image. Each is 72 points by 78.6365. The first image is erect,
        // and each successive image is rotated 15 degrees counter clockwise
        // from the previous, through 360 degrees.
        //
        // But here, each image is twice as large, and double rotated, centered
        // in a form, which is itself scaled to 1/2 size, and counter rotated.
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImageInForm(cosDoc, content, DPI300, X, Y, 72.0, 78.6365, angle, false);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 6 contains 25 duckies, all using the 300 DPI ducky
        // image. Each is 72 points by 78.6365. The first image is erect,
        // each successive image is rotated 15 degrees counter clockwise
        // from the previous, through 360 degrees.
        //
        // But here, each image is twice as large, and double rotated, centered
        // in a form, which is itself scaled to 1/4 size, and counter rotated angle * 3
        // which is located within a form scaled by 2, rotated angle * 2
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImageInForm(cosDoc, content, DPI300, X, Y, 72.0, 78.6365, angle, false);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 7 contains 25 duckies, identical to page 2, but mirrored vertically.
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImage(content, DPI300, X, Y, 72.0, 78.6365, angle, true, false);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 8 contains 25 duckies, identical to page 2, but mirrored horizontally.
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImage(content, DPI300, X, Y, 72.0, 78.6365, angle, false, true);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        // Page 9 contains 25 duckies, identical to page 2,
        // but mirrored horizontally and vertically
        sample.insertPage(ASFloatToFixed(8.5 * 72), ASFloatToFixed(11.0 * 72), sample.numPages() - 1);
        page = sample.getPage(sample.numPages() - 1);
        content = PDPageAcquirePDEContent(page, 0);
        X = 0.75 * 72.0;
        Y = 9.5 * 72.0;
        for (double angle = 0; angle < 360;) {
            for (int count = 0; count < 25; count++) {
                InsertImage(content, DPI300, X, Y, 72.0, 78.6365, angle, true, true);
                angle += 15;
                X += 1.5 * 72;
                if (X >= 8.25 * 72) {
                    X = 0.75 * 72;
                    Y -= 2.0 * 72.0;
                }
            }
        }
        PDPageSetPDEContent(page, 0);
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        sample.saveDoc("FindImageResolutions.pdf", PDSaveFull | PDSaveCollectGarbage);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;

    END_HANDLER
    return 0;
}
