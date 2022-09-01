//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample demonstrates flattening annotations within a PDF document.
//
// Command-line:  <input-file>  <output-file>       (Both optional)
//
// For more detail see the description of the FlattenAnnotations sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#flattenannotations

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PEWCalls.h"
#include "PERCalls.h"
#include "PagePDECntCalls.h"
#include "CosCalls.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "FlattenAnnotations.pdf"
#define DEF_OUTPUT "FlattenAnnotations-out.pdf"

static CosObj FindAppearanceResourceEntry(CosObj strm, PDPage page);
static CosObj FindAnnotAppearanceStream(CosObj annotCos);

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);

    DURING

        APDFLDoc doc(csInputFileName.c_str(), true);

        PDPage page = doc.getPage(0);
        PDEContent pageContent = PDPageAcquirePDEContent(page, 0);

        // Step 1) Convert each Annotation into a Form Xobject, and remove the annotation.

        ASInt32 nAnnotations = PDPageGetNumAnnots(page);

        std::cout << "Flattening " << nAnnotations << " from " << csInputFileName.c_str()
                  << " and writing to " << csOutputFileName.c_str() << std::endl;

        // We need to iterate through the annotations in "reverse" order, since PDPageRemoveAnnot
        //    updates the array of annotations with each removal.
        for (ASInt32 i = nAnnotations - 1; i >= 0; --i) {
            // Get the next annotation.
            PDAnnot next = PDPageGetAnnot(page, i);
            CosObj annotCos = PDAnnotGetCosObj(next);

            CosObj appearanceStrm = FindAnnotAppearanceStream(annotCos);
            CosObj resource = FindAppearanceResourceEntry(appearanceStrm, page);

            if (CosObjGetType(resource) != CosNull) {
                // Place the annotation's resources in the page's content
                ASFixedRect nextLoc;
                PDAnnotGetRect(next, &nextLoc);
                ASDoubleMatrix unity;
                unity.a = unity.d = 1.0;
                unity.b = unity.c = 0.0;
                unity.h = (ASDouble)ASFixedToFloat(nextLoc.left);
                unity.v = (ASDouble)ASFixedToFloat(nextLoc.bottom);

                // Create and add the form xobject.
                PDEForm formXObject = PDEFormCreateFromCosObjEx(&appearanceStrm, &resource, &unity);
                PDEContentAddElem(pageContent, kPDEAfterLast, (PDEElement)formXObject);

                PDERelease((PDEObject)formXObject);
            } else {
                // This annotation has no appearance.
                std::cout << "Warning: The " << i << "th annotation, a "
                          << ASAtomGetString(PDAnnotGetSubtype(next))
                          << ", has no contained or inherited resources entry, so has no "
                             "appearance. "
                          << "It will still be removed." << std::endl;
            }

            PDPageRemoveAnnot(page, i);
        }
        PDPageSetPDEContentCanRaise(page, 0);

        // Step 2) Save and close.

        // Release resources.
        PDPageReleasePDEContent(page, 0);
        PDPageRelease(page);

        doc.saveDoc(csOutputFileName.c_str());

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

// Try to find an annotation's appearance stream, which may contain the resource's CosObj
//    that we will need to create the PDEForm of its appearance.
//
CosObj FindAnnotAppearanceStream(CosObj annotCos) {
    CosObj retObj = CosNewNull();
    // The appearance dictionary of our annotation.
    if (CosDictKnownKeyString(annotCos, "AP")) {
        // The appearance dictionary of this annotation
        CosObj APDict = CosDictGetKeyString(annotCos, "AP");
        if (CosDictKnownKeyString(APDict, "N")) {
            // The normal appearance of our annotation.
            CosObj normal = CosDictGetKeyString(APDict, "N");

            // The normal appearance is either a stream or a dictionary. If the appearance is a dictionary,
            // we will need to get the appearance stream from the appearance state ("AS").
            if (CosObjGetType(normal) == CosStream) {
                retObj = normal;
            } else {
                if (CosDictKnownKeyString(annotCos, "AS")) {
                    ASAtom appearanceName = CosNameValue(CosDictGetKeyString(annotCos, "AS"));
                    if (CosDictKnown(normal, appearanceName)) {
                        retObj = CosDictGet(normal, appearanceName);
                    }
                }
            }
        }
    }
    return retObj;
}

// If we found an appearance stream, we must find its resources entry. Otherwise the annotation has no appearance.
//
CosObj FindAppearanceResourceEntry(CosObj strm, PDPage page) {
    CosObj retObj = CosNewNull();
    if (CosObjGetType(strm) != CosNull) {
        retObj = CosDictGetKeyString(strm, "Resources");

        // If the appearance stream doesn't have a Resources entry, we must look for an appearance
        // that might have been inherited from a parent page in the page tree.
        if (CosObjGetType(retObj) == CosNull) {
            CosObj pageObj = PDPageGetCosObj(page);
            while (CosObjGetType(retObj) == CosNull) {
                retObj = CosDictGetKeyString(pageObj, "Resources");
                if (CosObjGetType(retObj) == CosNull) {
                    pageObj = CosDictGetKeyString(pageObj, "Parent");
                    if (CosObjGetType(pageObj) == CosNull) {
                        break;
                    }
                } else {
                    break;
                }
            }
        }
    }
    return retObj;
}
