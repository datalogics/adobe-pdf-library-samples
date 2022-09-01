//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// This sample reads an input PDF document and inserts text for a header and for a footer on each page. The program adjusts
// the contents of each page if necessary to make sure that the header and footer will fit. It also provides default text
// for the header and footer, and then saves the PDF as an output file. InsertHeadFoot does not work with rotated pages.
//
// The font used for the text in the header and footer is defined in DEF_FONT and DEF_CHARSET, and the point size for the
// text for the header and footer is also set. You can name any font you like, but keep in mind that the program must be able
// to find this font stored on the system where it is run. The program queries the local environment to see if the default font
// named in the program is available there.
//
// The program is also designed to encrypt the output file so that the header and footer cannot be removed or edited without a password.
//
// For more detail see the description of the InsertHeadFoot sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#insertheadfoot

#ifdef WIN32
// Avoid compiler warning. This is a sample program.
#pragma warning(disable : 4267)
#endif

#include <cstdlib>
#include <string>
#include <sstream>

#include "CosCalls.h"
#include "PEWCalls.h"
#include "PERCalls.h"
#include "PSFCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "InsertHeadFoot.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "CopyContent.pdf"
#define OUTPUT_FILE "InsertHeadFoot-out.pdf"

#define DEF_FONT "CourierStd"
#define DEF_CHARSET "Roman"
#define HeadPointSize 32
#define FootPointSize 12

const char *Heading = "A Security Heading";
const char *Footing = "Some security footing text, Smaller than the head.";

int main(int argc, char *argv[]) {
    APDFLib libInit; // Initialize the library
    if (libInit.isValid() == false) {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError(errCode);
        return errCode;
    }

    // Verify that the default font named in the program is found on the local system and load it.
    // The font name and character set name are drawn from HEADFOOT_FONT and HEADFOOT_CHARSET.
    // If these are valid environment variables on the local system, the program will use that local
    // font and character set. Otherwise, the program will use the font "CourierStd" and character
    // set "Roman" defined above, under DEF_FONT and DEF_CHARSET.
    char *ep = std::getenv("HEADFOOT_FONT");
    std::string csFontName(ep ? ep : DEF_FONT);
    ep = std::getenv("HEADFOOT_CHARSET");
    std::string csCharSet(ep ? ep : DEF_CHARSET);

    PDEFontAttrs pdFontAttr;
    memset(&pdFontAttr, 0, sizeof(pdFontAttr));
    pdFontAttr.name = ASAtomFromString(csFontName.c_str());
    pdFontAttr.charSet = ASAtomFromString(csCharSet.c_str());
    PDSysFont gSysFont = PDFindSysFont(&pdFontAttr, sizeof(pdFontAttr), kPDSysFontMatchNameAndCharSet);
    if (NULL == gSysFont) {
        std::cout << "Couldn't find font " << csFontName.c_str() << " (charset " << csCharSet.c_str()
                  << ") on this system.  Try requesting a different font" << std::endl;
        return -4;
    } else {
        std::cout << "Using font " << csFontName.c_str() << " char set " << csCharSet.c_str() << std::endl;
    }

    std::string csInputFile(argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE);
    std::string csOutputFile(argc > 2 ? argv[2] : OUTPUT_FILE);

    std::cout << "Operating on file " << csInputFile.c_str() << ", will write file "
              << csOutputFile.c_str() << std::endl;

    PDDoc InDoc;
    PDEGraphicState gState;
    PDETextState tState;

    DURING
        // Open the source document
        ASPathName InPath = APDFLDoc::makePath(csInputFile.c_str());
        InDoc = PDDocOpen(InPath, NULL, GetPassWord, true);
        ASFileSysReleasePath(NULL, InPath);
    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    DURING
        // Set up a graphic state for setting the header and footer
        PDEDefaultGState(&gState, sizeof(PDEGraphicState));
        memset(&tState, 0, sizeof(PDETextState));

        CosObj CosWorkFont;
        // Prepare fonts
        {
            PDEFont pdeFont = PDEFontCreateFromSysFont(gSysFont, 0);
            PDEFontGetCosObj(pdeFont, &CosWorkFont);
            CosDoc cosDoc = PDDocGetCosDoc(InDoc);
            CosWorkFont = CosObjCopy(CosWorkFont, cosDoc, true);
            PDERelease((PDEObject)pdeFont);
        }

        ASFixedRect HeadSpace, FootSpace;
        memset(&HeadSpace, 0, sizeof(HeadSpace));
        memset(&FootSpace, 0, sizeof(FootSpace));

        ASInt32 PageNumb;
        CosObj CosHead = CosNewNull(), CosFoot = CosNewNull();
        for (PageNumb = 0; PageNumb < PDDocGetNumPages(InDoc); PageNumb++) {
            PDPage InPage;
            ASFixedRect MediaBox, BBox;
            ASFixed Width, Depth, Scale, HOffset, VOffset, Free, FreeAbove, FreeBelow, HeadDepth, FootDepth;
            ASFixedMatrix Matrix, PageMatrix, IPageMatrix;
            CosObj Content, CosWork, CosPage, CosResource, CosFontDict;

            // Get the page, and find the page size
            InPage = PDDocAcquirePage(InDoc, PageNumb);
            PDPageGetMediaBox(InPage, &MediaBox);

            // Determine if the page has free space on the top and bottom
            PDPageGetBBox(InPage, &BBox);

            // Get the content object
            CosPage = PDPageGetCosObj(InPage);
            Content = CosDictGet(CosPage, ASAtomFromString("Contents"));
            CosResource = CosDictGet(CosPage, ASAtomFromString("Resources"));
            CosWork = CosPage;
            while (CosObjGetType(CosResource) == CosNull) {
                CosWork = CosDictGet(CosWork, ASAtomFromString("Parent"));
                CosResource = CosDictGet(CosPage, ASAtomFromString("Resources"));
            }
            CosFontDict = CosDictGet(CosResource, ASAtomFromString("Font"));
            if (CosObjGetType(CosFontDict) == CosNull) {
                // There are no fonts on this page yet
                CosFontDict = CosNewDict(PDDocGetCosDoc(InDoc), false, 1);
                CosDictPut(CosResource, ASAtomFromString("Font"), CosFontDict);
            }

            PDPageGetDefaultMatrix(InPage, &PageMatrix);
            ASFixedMatrixTransformRect(&MediaBox, &PageMatrix, &MediaBox);
            ASFixedMatrixTransformRect(&BBox, &PageMatrix, &BBox);

            ASFixedMatrixInvert(&IPageMatrix, &PageMatrix);
            PDPageRelease(InPage);

            // Check to see if the page has room for a two inch header and footer.
            Width = MediaBox.right - MediaBox.left;
            Depth = MediaBox.top - MediaBox.bottom;
            FreeAbove = MediaBox.top - BBox.top;
            FreeBelow = BBox.bottom - MediaBox.bottom;
            Free = FreeAbove + FreeBelow;
            HeadDepth = FootDepth = fixedOne * 72;
            if (Free < (HeadDepth + FootDepth)) {
                // Scale the header and footer to fit as needed
                Scale = ASFixedDiv(Depth, // The available space on the page
                                   Depth + ((HeadDepth + FootDepth) - Free)); // The amount of scaling needed for the header & footer
                FreeAbove = ASFixedDiv(FreeAbove, Scale) + (Depth - ASFixedMul(Depth, Scale));
                FreeBelow = ASFixedDiv(FreeBelow, Scale);
            } else
                Scale = fixedOne;

            // Verify if the page needs to be moved down to clear the header
            if (FreeAbove < HeadDepth) {
                // Move image down (- offset) to accommodate heading
                VOffset = -(HeadDepth - FreeAbove);
                FreeAbove -= VOffset;
                FreeBelow += VOffset;
                ASFixedMul(VOffset, Scale);
            } else {
                if (FreeBelow < FootDepth) {
                    // Move image up (+ offset) to accommodate heading
                    VOffset = (FootDepth - FreeBelow);
                    FreeAbove -= VOffset;
                    FreeBelow += VOffset;
                    ASFixedMul(VOffset, Scale);
                } else {
                    VOffset = 0;
                }
            }

            // Scale symmetrically.  Center the page horizontally.
            if (Scale != fixedOne)
                HOffset = (Width - ASFixedMul(Width, Scale)) / 2;
            else
                HOffset = 0;

            // Calculate the size of and content of header and footer
            // Reuse previous header if page is the same size
            if ((Width != (HeadSpace.right - HeadSpace.left)) || (MediaBox.top != HeadSpace.top)) {
                HeadSpace.left = FootSpace.left = 0;
                HeadSpace.right = FootSpace.right = Width;
                HeadSpace.top = Depth;
                HeadSpace.bottom = HeadSpace.top - HeadDepth;
                FootSpace.bottom = 0;
                FootSpace.top = FootDepth;

                CosHead = BuildSecurityHead(InDoc, &HeadSpace, &IPageMatrix, CosWorkFont);
                CosFoot = BuildSecurityFoot(InDoc, &FootSpace, &IPageMatrix, CosWorkFont);
            }
            ASAtom CosWorkName = ASAtomFromString("WorkFont");
            CosDictPut(CosFontDict, CosWorkName, CosWorkFont);

            // Form the complete transformation matrix
            Matrix.h = HOffset;
            Matrix.v = VOffset;
            Matrix.a = Matrix.d = Scale;
            Matrix.b = Matrix.c = 0;

            // Prepend the transformation to the content. Put a save before
            // it, and a restore after it.
            // This will transform the text from the document to the new location on the
            // page. The text was transformed on the page after scaling was done to accommodate
            // the header and footer being inserted.
            if (CosObjGetType(Content) == CosStream) {
                CosObj NewContent;

                Content = CopyStreamAddDisplacement(Content, &Matrix);
                NewContent = CosNewArray(PDDocGetCosDoc(InDoc), true, 3);
                CosArrayPut(NewContent, 0, CosHead);
                CosArrayPut(NewContent, 1, CosFoot);
                CosArrayPut(NewContent, 2, Content);
                Content = NewContent;
            } else {
                CosWork = CosArrayGet(Content, 0);
                CosWork = CopyStreamAddDisplacementStart(CosWork, &Matrix);
                CosArrayPut(Content, 0, CosWork);
                CosWork = CosArrayGet(Content, CosArrayLength(Content) - 1);
                CosWork = CopyStreamAddRestore(CosWork);
                CosArrayPut(Content, CosArrayLength(Content) - 1, CosWork);
                CosArrayInsert(Content, 0, CosHead);
                CosArrayInsert(Content, 1, CosFoot);
            }
            CosDictPut(CosPage, ASAtomFromString("Contents"), Content);
        }

        ASPathName OutPath = APDFLDoc::makePath(csOutputFile.c_str());
        PDDocSave(InDoc, (PDSaveFull | PDSaveCollectGarbage), OutPath, NULL, NULL, NULL);
        ASFileSysReleasePath(NULL, OutPath);

    HANDLER
        APDFLib::displayError(ERRORCODE);
        return ERRORCODE;
    END_HANDLER

    // Don't forget to release the color spaces in the default graphic state
    PDERelease((PDEObject)gState.fillColorSpec.space);
    PDERelease((PDEObject)gState.strokeColorSpec.space);

    return 0;
}

// Call-back which PDDocOpen will invoke when it tries to open an encrypted document
ASBool GetPassWord(PDDoc Doc) {
    ASAtom Handler = PDDocGetNewCryptHandler(Doc);
    if (Handler == ASAtomNull)
        return (TRUE);

    // Input a password from the user
    char PassWord[100];
    std::cout << "Enter password for document: ";
    std::cout.flush();
    std::cin >> PassWord;

    int Permissions = pdPermOwner | pdPermOpen;
    int NewPerms = PDDocAuthorize(Doc, Permissions, PassWord);
    if (NewPerms & (pdPermOpen | pdPermOwner))
        return (TRUE);

    // The program makes no provision for multiple attempts to enter a password
    std::cout << "Incorrect Password.\n";
    return false;
}

CosObj CopyStreamAddDisplacement(CosObj Stream, ASFixedMatrix *Matrix) {
    std::ostringstream ossMatrixMoveCmd;
    std::string sRestoreCmd("\nQ\n");
    ossMatrixMoveCmd << "q " << FixedMatrixToStrings(Matrix).c_str() << " cm \n";

    CStreamContents csc(Stream);
    csc.AddCommands(ossMatrixMoveCmd.str().c_str(), sRestoreCmd.c_str());

    CosObj Attribute = CosObjCopy(CosStreamDict(Stream), CosObjGetDoc(Stream), false);
    CosDictRemove(Attribute, ASAtomFromString("Length"));
    CosObj Parms = CosDictGet(Attribute, ASAtomFromString("DecodeParms"));

    ASStm NewStm = ASMemStmRdOpen(csc.GetBuffer(), csc.GetSize());
    CosObj NewContent = CosNewStream(CosObjGetDoc(Stream), true, NewStm, 0, true, Attribute, Parms, -1);
    ASStmClose(NewStm);

    return NewContent;
}

CosObj CopyStreamAddDisplacementStart(CosObj Stream, ASFixedMatrix *Matrix) {
    std::ostringstream ossMatrixMoveCmd;
    ossMatrixMoveCmd << "q " << FixedMatrixToStrings(Matrix).c_str() << " cm \n";

    CStreamContents csc(Stream);
    csc.AddCommands(ossMatrixMoveCmd.str().c_str(), NULL);

    CosObj Attribute = CosObjCopy(CosStreamDict(Stream), CosObjGetDoc(Stream), false);
    CosDictRemove(Attribute, ASAtomFromString("Length"));
    CosObj Parms = CosDictGet(Attribute, ASAtomFromString("DecodeParms"));

    ASStm NewStm = ASMemStmRdOpen(csc.GetBuffer(), csc.GetSize());
    CosObj NewContent = CosNewStream(CosObjGetDoc(Stream), true, NewStm, 0, true, Attribute, Parms, -1);
    ASStmClose(NewStm);

    return NewContent;
}

CosObj CopyStreamAddRestore(CosObj Stream) {
    std::string sRestoreCmd("\nQ\n");

    CStreamContents csc(Stream);
    csc.AddCommands(NULL, sRestoreCmd.c_str());

    CosObj Attribute = CosObjCopy(CosStreamDict(Stream), CosObjGetDoc(Stream), false);
    CosDictRemove(Attribute, ASAtomFromString("Length"));
    CosObj Parms = CosDictGet(Attribute, ASAtomFromString("DecodeParms"));

    ASStm NewStm = ASMemStmRdOpen(csc.GetBuffer(), csc.GetSize());
    CosObj NewContent = CosNewStream(CosObjGetDoc(Stream), true, NewStm, 0, true, Attribute, Parms, -1);
    ASStmClose(NewStm);

    return NewContent;
}

// Utility function to render the elements in an ASFixedMatrix as a string in a, b, c, d, h, v order
// To learn more about these matrix values, visit the Datalogics KnowledgeBase, feedback.datalogics.com/knowledgebase, and search on "ASFixedMatrix."
std::string FixedMatrixToStrings(ASFixedMatrix *pM) {
    char A[10], B[10], C[10], D[10], H[10], V[10];
    ASFixedToCString(pM->a, A, sizeof(A) - 1, 4);
    ASFixedToCString(pM->b, B, sizeof(B) - 1, 4);
    ASFixedToCString(pM->c, C, sizeof(C) - 1, 4);
    ASFixedToCString(pM->d, D, sizeof(D) - 1, 4);
    ASFixedToCString(pM->h, H, sizeof(H) - 1, 4);
    ASFixedToCString(pM->v, V, sizeof(V) - 1, 4);
    std::ostringstream oss;
    oss << A << ' ' << B << ' ' << C << ' ' << D << ' ' << H << ' ' << V;
    return oss.str();
}

CosObj BuildSecurityHead(PDDoc InDoc, ASFixedRect *HeadSpace, ASFixedMatrix *InMatrix, CosObj &cosWorkFont) {
    CosDoc cosDoc = PDDocGetCosDoc(InDoc);
    PDEFont pdeFont;

    pdeFont = PDEFontCreateFromCosObj(&cosWorkFont);
    ASFixed Horiz = ASInt32ToFixed(PDEFontSumWidths(pdeFont, (ASUns8 *)Heading, strlen(Heading))) /
                    1000 * HeadPointSize;
    Horiz = (HeadSpace->right + HeadSpace->left - Horiz) / 2;
    ASFixed Vert = HeadSpace->top - ASInt32ToFixed(((HeadPointSize * 2) / 3)) - (fixedOne * 10);
    PDERelease((PDEObject)pdeFont);

    ASFixedMatrix Matrix;
    Matrix.a = Matrix.d = fixedOne;
    Matrix.b = Matrix.c = 0;
    Matrix.h = Horiz;
    Matrix.v = Vert;
    ASFixedMatrixConcat(&Matrix, InMatrix, &Matrix);

    std::ostringstream oss;
    oss << "q BT 0 g /WorkFont " << HeadPointSize << " Tf " << FixedMatrixToStrings(&Matrix)
        << " Tm (" << Heading << ") Tj ET Q";
    std::string header(oss.str());
    ASStm WorkStream = ASMemStmRdOpen(header.c_str(), header.length());
    CosObj StmDict = CosNewDict(cosDoc, false, 1);
    CosDictPut(StmDict, ASAtomFromString("Filter"), CosNewName(cosDoc, false, ASAtomFromString("FlateDecode")));
    CosObj NewStream = CosNewStream(cosDoc, true, WorkStream, 0, true, StmDict, CosNewNull(), -1);

    return NewStream;
}

CosObj BuildSecurityFoot(PDDoc InDoc, ASFixedRect *FootSpace, ASFixedMatrix *InMatrix, CosObj &cosWorkFont) {
    CosDoc cosDoc = PDDocGetCosDoc(InDoc);

    PDEFont pdeFont = PDEFontCreateFromCosObj(&cosWorkFont);
    ASFixed Horiz = ASInt32ToFixed(PDEFontSumWidths(pdeFont, (ASUns8 *)Footing, strlen(Footing))) /
                    1000 * FootPointSize;
    Horiz = (FootSpace->right + FootSpace->left - Horiz) / 2;
    ASFixed Vert = FootSpace->bottom + ASInt32ToFixed(((FootPointSize * 2) / 3)) + (10 * fixedOne);
    PDERelease((PDEObject)pdeFont);

    ASFixedMatrix Matrix;
    memmove(&Matrix, InMatrix, sizeof(ASFixedMatrix));
    Matrix.a = Matrix.d = fixedOne;
    Matrix.b = Matrix.c = 0;
    Matrix.h = Horiz;
    Matrix.v = Vert;
    ASFixedMatrixConcat(&Matrix, InMatrix, &Matrix);

    std::ostringstream oss;
    oss << "q BT 0 g /WorkFont " << FootPointSize << " Tf " << FixedMatrixToStrings(&Matrix)
        << " Tm (" << Footing << ") Tj ET Q";
    std::string footer(oss.str());
    ASStm WorkStream = ASMemStmRdOpen(footer.c_str(), footer.length());
    CosObj StmDict = CosNewDict(cosDoc, false, 1);
    CosObj NewStream = CosNewStream(cosDoc, true, WorkStream, 0, true, StmDict, CosNewNull(), -1);

    return NewStream;
}
