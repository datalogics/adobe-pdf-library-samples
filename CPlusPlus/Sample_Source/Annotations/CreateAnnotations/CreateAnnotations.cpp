//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// CreateAnnotations demonstrates adding annotations to a PDF document, and how to extract text
// from these annotations and save the content to a separate PDF document.
//
// Command-line:  <input-file> <output-pdf> <output-text>    (All are optional)
//
// For more detail see the description of the CreateAnnotations sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#createannotations

#include <vector>
#include <iostream>
#include <fstream>
#include <sstream>
#include <cmath>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PSFCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "DLExtrasCalls.h"
#include "CosCalls.h"
#include "ASExtraVers.h"

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "CreateAnnotations.pdf"
#define DEF_OUTPUT_ANNOT "CreateAnnotations-out.pdf"
#define DEF_OUTPUT_TEXT "CreateAnnotations-out-text.txt"

static void extractPDEElements(PDEContent c, std::vector<PDEElement> &list);
static void SetAnnotationQuads(PDAnnot annot, ASFixedQuad *quads, ASArraySize numQuads);
static PDColorValue SelectColor(bool textAnnotation);

int main(int argc, char **argv) {
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false) {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }

    std::string csInputFileName(argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT_ANNOT);
    std::string csOutputTextFileName(argc > 3 ? argv[3] : DEF_OUTPUT_TEXT);
    std::cout << "Opening " << csInputFileName.c_str() << " and adding annotations to "
              << "all elements on the first page;" << std::endl
              << "Will save to " << csOutputFileName.c_str()
              << ", then we will reopen that file and extract all annotations to "
              << csOutputTextFileName.c_str() << std::endl;

    std::ofstream ofText(csOutputTextFileName.c_str());
    ofText << "Results from file " << csOutputFileName.c_str() << ":" << std::endl;

    DURING

        APDFLDoc doc(csInputFileName.c_str(), true); // Open the input document, repairing it if necessary.

        PDPage inPage = doc.getPage(0);
        PDEContent inPageContent = PDPageAcquirePDEContent(inPage, 0);

        // Step 0) Retrieve all the PDEElements on the page.

        std::vector<PDEElement> pageElements;
        extractPDEElements(inPageContent, pageElements);

        // Step 1) Annotate them all

        // An annotation with descriptive content will be placed in the same location as each
        // PDEElement. For text elements, we will create a red, blue, or green highlight annotation.
        // For other elements, we will create a yellow text annotation.

        ASAtom atHighlight(ASAtomFromString("Highlight"));
        ASAtom atText(ASAtomFromString("Text"));

        std::vector<PDEElement>::iterator itElem, itElemEnd = pageElements.end();
        for (itElem = pageElements.begin(); itElem != itElemEnd; ++itElem) {
            // Retrieve the element's location. We will place the annotation where the
            //    original page element was found.
            ASFixedRect elementLoc;
            PDEElementGetBBox(*itElem, &elementLoc);

            // Fetch the annotation's type
            ASInt32 elementType = PDEObjectGetType((PDEObject)*itElem);
            ASAtom annotationType = (elementType == kPDEText) ? atText : atHighlight;

            // Create the new annotation. Set the appearance and content.
            PDAnnot annot = PDPageAddNewAnnot(inPage, kPDEAfterLast, annotationType, &elementLoc);

            std::wstringstream annotContent;
            annotContent << L"This is a ";
            switch (elementType) {
            case kPDEContainer:
                annotContent << L"container containing "
                             << PDEContentGetNumElems(PDEContainerGetContent((PDEContainer)*itElem))
                             << L" elements";
                break;
            case kPDEForm:
                annotContent << L"form containing "
                             << PDEContentGetNumElems(PDEFormGetContent((PDEForm)*itElem)) << L" elements";
                break;
            case kPDEGroup:
                annotContent << L"group containing "
                             << PDEContentGetNumElems(PDEGroupGetContent((PDEGroup)*itElem)) << L" elements";
                break;
            case kPDEImage:
                annotContent << L"image";
                break;
            case kPDEPath:
                annotContent << L"path";
                break;
            case kPDEPlace:
                annotContent << L"place";
                break;
            case kPDEText:
                annotContent << L"text object";
                break;
            case kPDEXObject:
                annotContent << L"XObject";
                break;
            }

            annotContent << L". It is situated at: " << std::endl
                         << L"\ttop: " << ASFixedToFloat(elementLoc.top) << std::endl
                         << L"\tbottom: " << ASFixedToFloat(elementLoc.bottom) << std::endl
                         << L"\tleft: " << ASFixedToFloat(elementLoc.left) << std::endl
                         << L"\tright: " << ASFixedToFloat(elementLoc.right);

            // The content string is ready. Now make its ASText to add it to the annotation.
            ASText annotContentAST = ASTextFromUnicode((ASUTF16Val *)annotContent.str().c_str(),
                                                       APDFLDoc::GetHostUnicodeFormat());

            // The annotation must be cast to a TextAnnot to set its text content.
            PDTextAnnot textAnnot = CastToPDTextAnnot(annot);
            PDTextAnnotSetContentsASText(textAnnot, annotContentAST);
            ASTextDestroy(annotContentAST);

            // The annotation's title.
            const char *annotTitleStr = "Page Element";
            PDAnnotSetTitle(annot, annotTitleStr, strlen(annotTitleStr));

            // Set the annotation's quadrilateral values. This will properly
            //   position a highlight annotation, and have no effect on the text annotation.
            ASFixedQuad annotLocQuad = {{elementLoc.left, elementLoc.bottom},
                                        {elementLoc.right, elementLoc.bottom},
                                        {elementLoc.left, elementLoc.top},
                                        {elementLoc.right, elementLoc.top}};
            SetAnnotationQuads(annot, &annotLocQuad, 1);

            // The annotation will be locked so that it cannot be edited again later.
            PDAnnotSetFlags(annot, PDAnnotGetFlags(annot) | pdAnnotLock | pdAnnotLockContents);

            // Set its color
            PDAnnotSetColor(annot, SelectColor(annotationType == atText));
        }

        // Step 2) Save the document and close it.

        doc.saveDoc(csOutputFileName.c_str());
        PDPageRelease(inPage);
        // Close the document and release resources
        doc.~APDFLDoc();

        // Step 3) Reopen the document we created and Extract the annotations' text

        APDFLDoc annotDoc(csOutputFileName.c_str(), true);
        PDPage annotPage = annotDoc.getPage(0);

        int numAnnots = PDPageGetNumAnnots(annotPage);
        int numTextAnnots = 0;
        int numBlankAnnots = 0;

        ofText << "The input page has " << numAnnots << " annotations." << std::endl;

        // Extract each annotation's text content (if any)
        const size_t buffersize = 1000;
        static char contentBuffer[buffersize];
        for (int i = 0; i < numAnnots; ++i) {
            PDAnnot annotation = PDPageGetAnnot(annotPage, i);

            PDTextAnnot nextAsText = CastToPDTextAnnot(annotation);
            PDTextAnnotGetContents(nextAsText, contentBuffer, buffersize);
            if (contentBuffer[0] != '\0') {
                numTextAnnots++;
                ofText << "Annotation no. " << numTextAnnots << ": " << contentBuffer << std::endl;
            } else {
                ++numBlankAnnots;
            }
        }

        PDPageRelease(annotPage);

        ofText << numBlankAnnots << " annotations on the page did not have text content." << std::endl;

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode;
}

// Walk the PDEContent tree (recursively) and save a reference to every element found
/* static */ void extractPDEElements(PDEContent c, std::vector<PDEElement> &elements) {
    ASInt32 numElems = PDEContentGetNumElems(c);
    for (ASInt32 i = 0; i < numElems; ++i) {
        // Track each element found
        PDEElement elem = PDEContentGetElem(c, i);
        elements.push_back(elem);

        // ...and recurse into the aggregate element types
        switch (PDEObjectGetType((PDEObject)elem)) {
        case kPDEContainer:
            extractPDEElements(PDEContainerGetContent((PDEContainer)elem), elements);
            break;
        case kPDEForm:
            extractPDEElements(PDEFormGetContent((PDEForm)elem), elements);
            break;
        case kPDEGroup:
            extractPDEElements(PDEGroupGetContent((PDEGroup)elem), elements);
            break;
        }
    }
}

// Helper function to add quads in BL, BR, TL, TR order to get correct output.
/* static */ void SetAnnotationQuads(PDAnnot annot, ASFixedQuad *quads, ASArraySize numQuads) {
    CosObj coAnnot = PDAnnotGetCosObj(annot); // Acquire the annotation's cos object.
    CosDoc coDoc = CosObjGetDoc(coAnnot);     // Get the CosDoc containing the annotation.
    CosObj coQuads = CosNewArray(coDoc, false, numQuads * 8); // Create a cos array to hold the quadpoints.

    for (ASUns32 i = 0, n = 0; i < numQuads; ++i) {
        // Bottom left
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].bl.h));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].bl.v));
        // Bottom right
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].br.h));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].br.v));
        // Top LEFT
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tl.h));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tl.v));
        // Top RIGHT
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tr.h));
        CosArrayPut(coQuads, n++, CosNewFixed(coDoc, false, quads[i].tr.v));
    }
    CosDictPut(coAnnot, ASAtomFromString("QuadPoints"), coQuads);
}

/* static */ PDColorValue SelectColor(bool textAnnotation) {
    static PDColorValueRec colorRec;

    colorRec.space = PDDeviceRGB;

    static int counter(0);

    if (!textAnnotation) {
        // Cycle between R/G/B for highlight annotations.
        colorRec.value[0] = 0 == counter ? fixedOne : fixedHalf;
        colorRec.value[1] = 1 == counter ? fixedOne : fixedHalf;
        colorRec.value[2] = 2 == counter ? fixedOne : fixedHalf;
        ++counter %= 3;
    } else {
        // Text annotations will be yellow.
        colorRec.value[0] = fixedOne;
        colorRec.value[1] = fixedOne;
        colorRec.value[2] = fixedZero;
    }
    return &colorRec;
}

#if 0
    //Prepare the text object which will hold all the extracted text.
    PDEText annotationsText = PDETextCreate();                                                     //This will hold all the extracted text.
    ASFixedMatrix textLoc;                                                                         //Use this value to position the texts.
    memset(&textLoc, 0, sizeof(textLoc));                                                          //Ensure there's no garbage in the matrix.
    ASFixed fontSize = FloatToASFixed(12.0f);                                                      //Set the font to 12 points.
    textLoc.a = textLoc.d = fontSize;                                                              //Character width and height, respectively.
    textLoc.v = textLoc.h = 0;                                                                     //The vertical and horizontal position of the text, respectively. We will set v as we place texts.

    //Load the font we'll use to write the text content.
    PDEFontAttrs fontAttrs;                                                                        //Contains data to retrieve the font.
    memset(&fontAttrs, 0, sizeof(fontAttrs));                                                      //Ensure there's no garbage in the struct.
    fontAttrs.name = ASAtomFromString("CourierStd");                                               //The font name.
    fontAttrs.type = ASAtomFromString("Type1");                                                    //The font type.
    PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0);                           //Locate the system font that corresponds to the font attributes.
    PDEFont font = PDEFontCreateFromSysFont(sysFont, kPDEFontDoNotEmbed);                          //Create the pdeFont with the sysFont. We won't embed the font.

    //A default graphics state with which to draw the text.
    PDEGraphicState graphics;
    PDEDefaultGState(&graphics, sizeof(PDEGraphicState));

    int maxDigits = (int)(log10((double)PDPageGetNumAnnots(annotPage)) + 1);                       //The maximum number of digits of n that the nth text annotation can have. Used to pad the text string.
#endif
#if 0
            //Prepare the string to output.
            std::wstringstream extractedString;

            extractedString << L"Annotation no. " << numTextAnnots;

            //Pad out the spaces so that the extracted strings all line up.
            int numDigits = (int)(log10((double)numTextAnnots) + 1);                               //The number of digits of i that this annotation, the ith one, has.
            for (int i = 0; i < (maxDigits - numDigits); ++i)
                extractedString << L" ";

            extractedString << L" '" << contentBuffer << L"'";                                     //The content is placed in the output string here.

            //Add the text.
            ASUnicodeFormat format = (sizeof(wchar_t) == 4) ? kUTF32HostEndian : kUTF16HostEndian;
            ASText extractedAST = ASTextFromUnicode((ASUTF16Val*)extractedString.str().c_str(), format);
            PDETextAddASText(annotationsText, kPDETextRun, numTextAnnots - 1, extractedAST, font, &graphics, sizeof(PDEGraphicState), NULL, 0, &textLoc);
            ASTextDestroy(extractedAST);

            //Adjust our page dimension requirements.
            ASFixedRect thisBox;
            PDETextGetBBox(annotationsText, kPDETextRun, numTextAnnots-1, &thisBox);
            ASFixed thisWidth  = (thisBox.right - thisBox.left);                                       //The width of this text object.
            ASFixed thisHeight = (thisBox.top - thisBox.bottom);                                       //The height of this text object.

            if (thisWidth > neededWidth)
                neededWidth = thisWidth;

            neededHeight += thisHeight + fontSize;

            //Position the next text.
            textLoc.v -= fontSize*2;
#endif
#if 0
    PDERelease((PDEObject)font);
    PDERelease((PDEObject)graphics.fillColorSpec.space);
#endif
#if 0 
    ASFixed neededWidth  = fixedZero;                                                              //The required width of a page that can hold all the text objects.
    ASFixed neededHeight = fixedZero;                                                              //The required height.
    //The program needs to move each text object up so that it fits on the page. Otherwise it will be added to the bottom-left corner, and be invisible.
    for (int i = 0; i < numTextAnnots; ++i)
    {
        PDETextItem textItem = PDETextGetItem(annotationsText, i);

        //The new location will be the old location, adjusted up, and accounting for the font size.
        ASFixedMatrix newLoc;
        PDETextItemGetTextMatrix(textItem, 0, &newLoc);
        newLoc.v += neededHeight - fontSize*2;

        PDETextItemSetTextMatrix(textItem, &newLoc);
    }

    //Create our output document.
    APDFLDoc extractDoc;
    extractDoc.insertPage(neededWidth,neededHeight, PDBeforeFirstPage);
    PDPage extractPage = extractDoc.getPage(0);

    //Put the texts onto the output document.
    PDEContentAddElem(PDPageAcquirePDEContent(extractPage, 0), 0, (PDEElement)annotationsText);
    PDERelease((PDEObject)annotationsText);

    std::wcout << L"I extracted " << numTextAnnots << L" text-containing annotations." << std::endl;
    std::wcout << L"Saving the extracted text document." << std::endl;

    PDPageSetPDEContentCanRaise(extractPage, 0);
    PDPageReleasePDEContent(extractPage, 0);
    PDPageRelease(extractPage);
    extractDoc.saveDoc(L"AnnotationTexts.pdf",PDSaveFull);
#endif
