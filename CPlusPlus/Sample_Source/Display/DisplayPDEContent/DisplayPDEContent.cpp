//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This program generates an output text file that lists details regarding the PDE content on every page
// in an input PDF document. The PDFEdit Layer (PDE) of the Adobe Acrobat API contains classes that provide
// for editing objects in PDF documents, including color spaces, clip and page objects, fonts, form XObjects,
// and other objects. This program can list the number of pages in the document and the file size, and identify
// and describe a variety of features within the PDF, such as the page layout (landscape or portrait), annotations,
// text content, and graphics. For graphics, the program describes how the document manages graphic images, such
// as setting boundaries.
//
// The purpose of this program is to demonstrate gathering information from a PDE content tree in a PDF document.
// This content tree stores objects in the document. Note that a wide variety of different kinds of information
// can be drawn from the PDE content tree.  This sample provides a report that lists some of the kinds of data
// available, but you can edit the program to find and display other values that interest you.
//
// This program is also a good example of how to walk through the content of a PDF document.
//

#include <iostream>
#include <sstream>

#include "CosCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "CalcImageDPI-in.pdf"
#define OUTPUT_FILE "DisplayPDEContent-out.txt"

#include "DisplayPDEContent.h"
#include "dpcOutput.h"

static void DisplayPageInfo(PDPage page);

int main(int argc, char *argv[]) {
    APDFLib libInit; // Initialize the library
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFileName(argc > 2 ? argv[2] : OUTPUT_FILE);
    std::cout << "Analyzing file " << csInputFileName.c_str() << ", writing results to "
              << csOutputFileName.c_str() << std::endl;

    Outputter::Inst()->Init(csOutputFileName.c_str());

    DURING
        APDFLDoc apdflDoc(csInputFileName.c_str(), true);
        PDDoc pdDoc = apdflDoc.getPDDoc();

        // Save number of pages
        ASUns32 pagesInDoc = PDDocGetNumPages(pdDoc);

        // Complete whatever reporting you like about the document
        ASTFilePos fileSize = ASFileGetEOF(PDDocGetFile(pdDoc));
        Outputter::Inst()->GetOfs() << "Start of document " << csInputFileName.c_str() << "\nDocument has "
                                    << pagesInDoc << " pages contained in " << fileSize << " bytes.\n{\n";
        Outputter::Inst()->Indent();

        // Set up a unity matrix
        ASFixedMatrix UnityMatrix;
        UnityMatrix.a = UnityMatrix.d = fixedOne;
        UnityMatrix.b = UnityMatrix.c = 0;
        UnityMatrix.h = UnityMatrix.v = 0;

        // Acquire each page in turn from the document
        for (ASUns32 pageNumber = 0; pageNumber < pagesInDoc; pageNumber++) {
            // Obtain the page and its PDE content
            PDPage page = PDDocAcquirePage(pdDoc, pageNumber);
            PDEContent content = PDPageAcquirePDEContent(page, 0);

            // Complete whatever reporting you like at the start of a page
            Outputter::Inst()->GetOfs() << "Start of page " << pageNumber << std::endl;
            DisplayPageInfo(page);

            // Parse and analyze the PDE content of the page
            AnalyzePDEContent(content, &UnityMatrix);

            // Complete whatever reporting you like at the end of a page
            Outputter::Inst()->Outdent();
            Outputter::Inst()->GetOfs() << "} End of page " << pageNumber << " content\n";

            // Free the page content, and the page
            PDPageReleasePDEContent(page, 0);
            PDPageRelease(page);
        }

        // Do whatever reporting you wish to do at the end of the document
        Outputter::Inst()->Outdent();
        Outputter::Inst()->GetOfs() << "End of Document " << csInputFileName.c_str() << std::endl;

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    return 0;
}

static const char *GetRotationText(PDRotate r) {
    switch (r) {
    case pdRotate0:
        return "Upright";
    case pdRotate90:
        return "Landscape";
    case pdRotate180:
        return "Inverted";
    case pdRotate270:
        return "Inverted Landscape";
    default:
        return "Unknown Rotation";
    }
}

static std::string DisplayRectangle(ASFixedRect &r) {
    char Left[20], Right[20], Bottom[20], Top[20];
    ASFixedToCString(r.left, Left, 20, 5);
    ASFixedToCString(r.right, Right, 20, 5);
    ASFixedToCString(r.bottom, Bottom, 20, 5);
    ASFixedToCString(r.top, Top, 20, 5);
    std::ostringstream oss;
    oss << "[" << Left << ", " << Bottom << ", " << Right << ", " << Top << "]";
    return oss.str();
}

static std::string GetBoxDimsText(PDPage p, const char *box = 0) {
    ASFixedRect r;
    if (!box) {
        PDPageGetBBox(p, &r);
    } else {
        PDPageGetBox(p, ASAtomFromString(box), &r);
    }
    return DisplayRectangle(r);
}

//
// When working with objects embedded on a page, particularly graphics objects, PDF documents define a rectangular
// bounding type to describe the region where an image is found. Elements on the page that do not fall within the
// boundaries of this region are ignored and do not appear in output.
//
// There are a variety of ways to define these boundaries, and thus define how content would be selected. For example,
// the input content could use a media box, which defines the full boundaries of the actual page, or a crop box, which
// is limited to the exact boundaries for the print or graphic content shown on that page. So an object on an input page
// that is defined with a crop box will usually be smaller than an object defined using a media box.
static void DisplayPageInfo(PDPage page) {
    Outputter::Inst()->GetOfs() << "Page is displayed " << GetRotationText(PDPageGetRotate(page)) << std::endl;
    if (PDPageHasTransparency(page, true)) {
        Outputter::Inst()->GetOfs() << "This page uses some transparent ink features\n";
    }
    if (ASUns32 NumberOfAnnots = PDPageGetNumAnnots(page)) {
        Outputter::Inst()->GetOfs() << "This page contains " << NumberOfAnnots << " Annotations.\n";
    }
    Outputter::Inst()->GetOfs() << "Bounding Box: " << GetBoxDimsText(page) << std::endl;
    Outputter::Inst()->GetOfs() << "ArtBox: " << GetBoxDimsText(page, "ArtBox") << std::endl;
    Outputter::Inst()->GetOfs() << "BleedBox: " << GetBoxDimsText(page, "BleedBox") << std::endl;
    Outputter::Inst()->GetOfs() << "TrimBox: " << GetBoxDimsText(page, "TrimBox") << std::endl;
    Outputter::Inst()->GetOfs() << "CropBox: " << GetBoxDimsText(page, "CropBox") << std::endl;
    Outputter::Inst()->GetOfs() << "MediaBox: " << GetBoxDimsText(page, "MediaBox") << std::endl;
    Outputter::Inst()->GetOfs() << "Page Content:" << std::endl;
    Outputter::Inst()->GetOfs() << "{" << std::endl;
    Outputter::Inst()->Indent();
}

static void StartDisplayContent(PDEContent Container, ASFixedMatrix *Matrix,
                                PDEGraphicState *GState, ASBool HasGState) {
    PDEContentAttrs Attributes;
    std::ostringstream outText;
    PDEContentGetAttrs(Container, &Attributes, sizeof(PDEContentAttrs));
    if (Attributes.flags & kPDESetCacheDevice)

        if (Attributes.flags & kPDESetCacheDevice) {
            char Amount1[10], Amount2[10], Amount3[10], Amount4[10], Amount5[10], Amount6[10];
            ASFixedToCString(Attributes.cacheDevice[0], Amount1, 20, 5);
            ASFixedToCString(Attributes.cacheDevice[1], Amount2, 20, 5);
            ASFixedToCString(Attributes.cacheDevice[2], Amount3, 20, 5);
            ASFixedToCString(Attributes.cacheDevice[3], Amount4, 20, 5);
            ASFixedToCString(Attributes.cacheDevice[4], Amount5, 20, 5);
            ASFixedToCString(Attributes.cacheDevice[5], Amount6, 20, 5);
            outText << "Cache Values: [" << Amount1 << " " << Amount2 << " " << Amount3 << Amount4
                    << " " << Amount5 << " " << Amount6 << "] ";
        }

    if (Attributes.flags & kPDESetCharWidth) {
        char Amount1[10], Amount2[10];
        if (Attributes.flags & kPDESetCacheDevice) {
            ASFixedToCString(Attributes.cacheDevice[6], Amount1, 20, 5);
            ASFixedToCString(Attributes.cacheDevice[7], Amount2, 20, 5);
        } else {
            ASFixedToCString(Attributes.cacheDevice[0], Amount1, 20, 5);
            ASFixedToCString(Attributes.cacheDevice[1], Amount2, 20, 5);
        }
        outText << "Char Widths: [" << Amount1 << " " << Amount2 << "] ";
    }

    if (Attributes.formType) {
        outText << "Contains Form type " << Attributes.formType
                << ". Rounding Box: " << DisplayRectangle(Attributes.bbox).c_str();
        if (Attributes.flags & kPDEFormMatrix) {
            outText << ", Matrix " << DisplayMatrix(&Attributes.matrix);
        }
    }

    Outputter::Inst()->GetOfs() << "Content Object: At " << DisplayMatrix(Matrix)
                                << outText.str().c_str() << std::endl;
    Outputter::Inst()->GetOfs() << "{\n";
    return;
}

static void EndDisplayContent() { Outputter::Inst()->GetOfs() << "}" << std::endl; }

static void StartDisplayContainer(PDEContainer Container, ASFixedMatrix *Matrix,
                                  PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->GetOfs() << "Container Object: " << ASAtomGetString(PDEContainerGetMCTag(Container))
                                << std::endl;

    CosObj MCDict;
    ASBool InLine;
    if (PDEContainerGetDict(Container, &MCDict, &InLine)) {
        Outputter::Inst()->Indent();
        Outputter::Inst()->GetOfs()
            << "Marked Content Dictionary: " << DisplayCosDict(MCDict).c_str() << std::endl;
        Outputter::Inst()->Outdent();
    }
    Outputter::Inst()->GetOfs() << "{" << std::endl;
    Outputter::Inst()->Indent();
}

static void EndDisplayContainer(PDEContainer Container, ASFixedMatrix *Matrix,
                                PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->Outdent();
    Outputter::Inst()->GetOfs() << "} End Marked Content Container "
                                << ASAtomGetString(PDEContainerGetMCTag(Container)) << std::endl;
}

static void StartDisplayGroup(PDEGroup Group, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->GetOfs() << "Group Object: at " << DisplayMatrix(Matrix) << std::endl;
    Outputter::Inst()->GetOfs() << "{" << std::endl;
    Outputter::Inst()->Indent();
}

static void EndDisplayGroup(PDEGroup Group, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->GetOfs() << "}" << std::endl;
    Outputter::Inst()->Outdent();
}

static void StartDisplayForm(PDEForm Form, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->GetOfs() << "Forms XObject: At " << DisplayMatrix(Matrix) << std::endl;

    CosObj FormsObj, WorkObj;
    PDEFormGetCosObj(Form, &FormsObj);
    WorkObj = CosDictGet(FormsObj, ASAtomFromString("BBox"));
    ASFixedRect FormsBB;
    FormsBB.bottom = CosFixedValue(CosArrayGet(WorkObj, 0));
    FormsBB.left = CosFixedValue(CosArrayGet(WorkObj, 1));
    FormsBB.top = CosFixedValue(CosArrayGet(WorkObj, 2));
    FormsBB.right = CosFixedValue(CosArrayGet(WorkObj, 3));
    std::string BBText(DisplayRectangle(FormsBB));

    WorkObj = CosDictGet(FormsObj, ASAtomFromString("Matrix"));
    ASFixedMatrix FormsMatrix;
    if (CosObjGetType(WorkObj) == CosArray) {
        FormsMatrix.a = CosFixedValue(CosArrayGet(WorkObj, 0));
        FormsMatrix.b = CosFixedValue(CosArrayGet(WorkObj, 1));
        FormsMatrix.c = CosFixedValue(CosArrayGet(WorkObj, 2));
        FormsMatrix.d = CosFixedValue(CosArrayGet(WorkObj, 3));
        FormsMatrix.h = CosFixedValue(CosArrayGet(WorkObj, 4));
        FormsMatrix.v = CosFixedValue(CosArrayGet(WorkObj, 5));
    } else {
        FormsMatrix.a = FormsMatrix.d = fixedOne;
        FormsMatrix.b = FormsMatrix.c = 0;
        FormsMatrix.h = FormsMatrix.v = 0;
    }

    Outputter::Inst()->GetOfs() << "    Forms Matrix: " << DisplayMatrix(&FormsMatrix)
                                << ", Forms Bounding Box: " << BBText.c_str() << std::endl;
    Outputter::Inst()->GetOfs() << "{" << std::endl;
    Outputter::Inst()->Indent();
}

static void EndDisplayForm(PDEForm Form, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->Outdent();
    Outputter::Inst()->GetOfs() << "} End of Forms XObject content\n";
}

static void StartDisplayPostscript(PDEPS EPSImage, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    char First100[101];

    // Review the stream to determine the length, but only show first 100 characters
    ASStm PSStm = PDEPSGetDataStm(EPSImage);
    char Buffer[100];
    ASUns32 PSSize = 0;
    while (1) {
        ASUns32 Bytes = ASStmRead(Buffer, 100, 1, PSStm);
        if (0 == Bytes)
            break;
        if (0 == PSSize) {
            memmove(First100, Buffer, Bytes);
            First100[Bytes] = '\0';
        }
        PSSize += Bytes;
    }
    ASStmClose(PSStm);

    Outputter::Inst()->GetOfs() << "Postscript Object: " << PSSize << " Bytes long\n";
    Outputter::Inst()->Indent();
    Outputter::Inst()->GetOfs() << "PS Begins: " << First100 << std::endl;
    Outputter::Inst()->Outdent();
}

static void DisplayPlace(PDEPlace Place, ASFixedMatrix *Matrix, PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->GetOfs() << "Place Object: " << ASAtomGetString(PDEPlaceGetMCTag(Place)) << std::endl;

    CosObj MCDict;
    ASBool InLine;
    if (PDEPlaceGetDict(Place, &MCDict, &InLine)) {
        Outputter::Inst()->Outdent();
        Outputter::Inst()->GetOfs()
            << "Marked Content Dictionary: " << DisplayCosDict(MCDict).c_str() << std::endl;
        Outputter::Inst()->Outdent();
    }
}

static void DisplayBeginContainer(PDEBeginContainer Container, ASFixedMatrix *Matrix,
                                  PDEGraphicState *GState, ASBool HasGState) {
    Outputter::Inst()->GetOfs() << "Begin Container: "
                                << ASAtomGetString(PDEBeginContainerGetMCTag(Container)) << std::endl;

    CosObj MCDict;
    ASBool InLine;
    if (PDEBeginContainerGetDict(Container, &MCDict, &InLine)) {
        Outputter::Inst()->Indent();
        Outputter::Inst()->GetOfs()
            << "Marked Content Dictionary: " << DisplayCosDict(MCDict).c_str() << std::endl;
        Outputter::Inst()->Outdent();
    }
    Outputter::Inst()->GetOfs() << "{" << std::endl;
    Outputter::Inst()->Indent();
}

static void DisplayEndContainer() {
    Outputter::Inst()->Outdent();
    Outputter::Inst()->GetOfs() << "} End of container" << std::endl;
}

static void DisplayBeginGroup() {
    Outputter::Inst()->GetOfs() << "Begin Group:" << std::endl;
    Outputter::Inst()->GetOfs() << "{" << std::endl;
    Outputter::Inst()->Indent();
}

static void DisplayEndGroup() {
    Outputter::Inst()->Outdent();
    Outputter::Inst()->GetOfs() << "} End of a group" << std::endl;
}

void AnalyzePDEContent(PDEContent Content, ASFixedMatrix *Matrix) {
    // Get the number of PDE elements contained
    ASUns32 NumberOfElems = PDEContentGetNumElems(Content);

    // For each PDE element
    for (ASUns32 Index = 0; Index < NumberOfElems; Index++) {
        PDEGraphicState GState;
        ASBool HasGState;
        PDEContent SubContent;

        // Obtain the element
        PDEElement Element = PDEContentGetElem(Content, Index);

        // Obtain its Graphic State (if any)
        HasGState = PDEElementHasGState(Element, &GState, sizeof(PDEGraphicState));

        // Obtain its position in the container
        ASFixedMatrix ContentMatrix;
        PDEElementGetMatrix(Element, &ContentMatrix);

        // And Convert to a position in the page
        ASFixedMatrix PageMatrix;
        ASFixedMatrixConcat(&PageMatrix, &ContentMatrix, Matrix);

        // Process the PDE element by type
        PDEType Type = (PDEType)PDEObjectGetType((PDEObject)Element);
        switch (Type) {
        case kPDEContent:
            // Display a content object
            StartDisplayContent((PDEContent)Element, &PageMatrix, &GState, HasGState);

            // Analyze and display its content
            AnalyzePDEContent((PDEContent)Element, &PageMatrix);

            // Display end of a content object
            EndDisplayContent();
            break;

        case kPDEContainer:
            // Display a container object
            StartDisplayContainer((PDEContainer)Element, &PageMatrix, &GState, HasGState);

            // Obtain, Analyze, and Display its content
            SubContent = PDEContainerGetContent((PDEContainer)Element);
            AnalyzePDEContent(SubContent, &PageMatrix);

            // Display the end of a container object
            EndDisplayContainer((PDEContainer)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEGroup:
            // Display a group object
            StartDisplayGroup((PDEGroup)Element, &PageMatrix, &GState, HasGState);

            // Obtain, Analyze, and display its content
            SubContent = PDEGroupGetContent((PDEGroup)Element);
            AnalyzePDEContent(SubContent, &PageMatrix);

            // Display the end of a group object
            EndDisplayGroup((PDEGroup)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEText:
            // Display a complete text object
            DisplayText((PDEText)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEPath:
            // Display a complete path
            DisplayPath((PDEPath)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEImage:
            // Display an image object
            DisplayImage((PDEImage)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEShading:
            // Display the info for a Shading Operator
            DisplayShading((PDEShading)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEForm:
            // Display a forms object
            StartDisplayForm((PDEForm)Element, &PageMatrix, &GState, HasGState);

            // Obtain, Analyze, and Display its content
            SubContent = PDEFormGetContent((PDEForm)Element);
            AnalyzePDEContent(SubContent, &PageMatrix);

            // Display the end of a form object
            EndDisplayForm((PDEForm)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEPS:
            // Display a Postscript Object
            StartDisplayPostscript((PDEPS)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEPlace:
            // Display a Place Object
            DisplayPlace((PDEPlace)Element, &PageMatrix, &GState, HasGState);
            break;

        // These don't actually have content, but serve to group content in a stream
        case kPDEBeginContainer:
            DisplayBeginContainer((PDEBeginContainer)Element, &PageMatrix, &GState, HasGState);
            break;

        case kPDEEndContainer:
            DisplayEndContainer();
            break;

        case kPDEBeginGroup:
            DisplayBeginGroup();
            break;

        case kPDEEndGroup:
            DisplayEndGroup();
            break;

        //Reserved for Internal Use
        case kPDEGraphicFont:
            break;

        // These should never occur in content
        default:
        case kPDEXObject:
        case kPDEClip: {
            // None of these should ever end up in the content
            Outputter::Inst()->GetOfs() << "******* Invalid content. Type " << Type << std::endl;
            break;
        }
        }
    }
    return;
}
