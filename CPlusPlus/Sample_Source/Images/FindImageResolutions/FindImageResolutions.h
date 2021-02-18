//
// Copyright (c) 2015-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// Sample: FindImageResolutions - Create a list of all images in a document, and their parameters,
//         with a sublist of instances where these images are displayed, and
//         their effective resolutions.
//

// These are used in calculating the rotation specified in a Matrix
#ifndef M_PI
#define M_PI 3.1415926535897932385E0 /*Hex  2^ 1 * 1.921FB54442D18 */
#endif
#define degrees_to_radians (M_PI / 180.0)

// Define a structure to describe a reference to an image
typedef struct imageReference {
    ASSize_t m_page;         // Page n umber this reference occurs on.
    PDEImageAttrs m_attrs;   // The image attributes used at reference time.
    PDEImage m_reference;    // The PDEImage Object which references this image
    ASDoubleMatrix m_matrix; // The matrix in effect at the time of reference
    ASDouble m_hRes, m_vRes; // Effective Horizontal and Vertical resolutions
    ASDouble m_rotation;     // Rotation angle of the image (In Degrees)
    ASDouble m_shear;        // Shear of horizontal/vertical (In Degrees)
} ImageRef;

typedef std::vector<ImageRef> ImageRefList;

// Define a structure to describe one image definition, noting the
// places the structure is referenced in
typedef struct imagedef {
    ASBool m_inline;          // This will be true is this is an InLine image.
                              //  If this is the case, then there will be only one reference,
                              //  and there will be no CosObj.
    CosObj m_cos_object;      //  The COS Object which defines this image
    ASUns32 m_width, m_depth; //  The width and depth of the image in pixels
    ImageRefList m_refs;      // A list of places where this image is referenced.
    ASBool m_mask;            // Image is a mask image, applied to another image
    ASBool m_smask;           // Image is a Soft Mask, applied to another image
    imagedef(PDEImage img, ASBool mask, ASBool smask)
        : m_inline(false), m_width(0), m_depth(0), m_mask(mask), m_smask(smask) {
        PDEImageGetCosObj(img, &m_cos_object);
    }
    friend std::ostream &operator<<(std::ostream &os, const imagedef &id);
} ImageDef;

typedef std::vector<ImageDef> ImageList;

std::ostream &operator<<(std::ostream &os, const imagedef &id) {
    if (id.m_smask)
        os << " (Soft Mask)";
    if (id.m_mask)
        os << " (Mask)";
    os << " is an " << (id.m_inline ? "InLine" : "XObject") << " image " << id.m_width << " pixels wide, and "
       << id.m_depth << " pixels deep. It is referenced " << id.m_refs.size() << " times.\n";

    std::vector<ImageRef>::const_iterator it, itE = id.m_refs.end();
    int rCount;
    for (rCount = 1, it = id.m_refs.begin(); it != itE; ++it, ++rCount) {
        os << "\n      Reference " << rCount << " is on page " << it->m_page + 1 << " and has a resolution of "
           << it->m_hRes << "horizontal, and " << it->m_vRes << " vertical.\n";
        if (it->m_rotation)
            os << "       Image is rotated " << it->m_rotation << " degrees\n";
        if (it->m_shear)
            os << "       Image is sheared " << it->m_shear << " degrees\n";
    }
    os << std::endl;

    return os;
}

// Function object for tallying total number of references
struct RefAccumulator {
    int operator()(int x, const ImageDef &id) { return x + id.m_refs.size(); }
};

void FindImagesInContent(ASSize_t pageNumber, PDEContent content, ASDoubleMatrix matrix,
                         ImageList *imageList, ASBool inSoftMask);
void DoubleMatrixRotate(ASDoubleMatrix *M, ASDouble Angle);
void CalculateResolution(ImageDef *image, ImageRef *reference);
void CreateImageEntry(ASSize_t pageNo, PDEImage image, ASDoubleMatrix matrix, ImageList *imageList,
                      ASBool mask, ASBool sMask);
int MakeSample();
void ReleaseSample();
