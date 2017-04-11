//
// Copyright (c) 2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample adds watermarks to an input document, a text watermark and a graphic.
//
// Command-line:   <input-file-name> <watermark-document> <output-file-name>    (All optional)
//
// For more detail see the description of the AddWatermark sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#addwatermark

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "PagePDECntCalls.h"
#include "PERCalls.h"
#include "PSFCalls.h"
#include "PEWCalls.h"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddWatermark.pdf"
#define DEF_WATER "Watermark.pdf"
#define DEF_OUTPUT "AddWatermark-out.pdf"
#define DEF_TEXT "Copyright (c) 2017, Datalogics, Inc."

int main(int argc, char** argv)
{
    ASErrorCode errCode = 0;
    APDFLib libInit;

    if (libInit.isValid() == false)
    {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csInputFileName ( argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT );
    std::string csWatermarkFileName ( argc > 2 ? argv[2] : INPUT_LOC DEF_WATER );
    std::string csOutputFileName ( argc > 3 ? argv[3] : DEF_OUTPUT );
    std::string csWatermarkText ( DEF_TEXT );
    std::cout << "Adding first page of " << csWatermarkFileName.c_str() << " to first 2 pages of "
              << csInputFileName.c_str() << ",\n  and adding text \"" << csWatermarkText.c_str()
              << "\" to second 2 pages, writing output to " << csOutputFileName.c_str() << std::endl;

DURING

    APDFLDoc inDoc ( csInputFileName.c_str(), true);
    // We'll take the watermark image from the first page of this PDF:
    APDFLDoc watermarkDoc ( csWatermarkFileName.c_str(), true);    

//Step 1) Set the watermark parameters struct.

    // Note: This struct is used by both our text and page watermarks.
    PDDocAddWatermarkParamsRec watermarkOptions;
    memset(&watermarkOptions, 0, sizeof(PDDocAddWatermarkParamsRec));
    watermarkOptions.size = sizeof(PDDocAddWatermarkParamsRec);

    // Set display options:
    
    // This is a list-initialized PDPageRange. The first page to watermark is 0, 
    //      the last page is 1, and we'll add a watermark to every page in the range.
    PDPageRange	placeHolder = {0, 1, PDAllPages};
    watermarkOptions.targetRange = placeHolder;							 

    watermarkOptions.zOrderTop = false;       // Watermarks to be added to page background, not on top.
    watermarkOptions.showOnScreen = true;     // Watermarks will be visible in a PDF viewer...
    watermarkOptions.showOnPrint = false;     // ..but will not show up if the PDF is printed.
    watermarkOptions.fixedPrint  = false;     // Watermarks will not be a fixed print watermark, 
                                              //   meaning it changes its size and position based on 
                                              //   the target media's dimensions, if necessary.

    //Placement options.
    watermarkOptions.horizAlign = kPDHorizCenter;   // Horizontally aligned at the center of the page.
    watermarkOptions.horizValue = 0.0f;             // No horizontal offset.
    watermarkOptions.vertAlign  = kPDVertCenter;    // Vertically aligned at the top.
    watermarkOptions.vertValue  = 0.0f;             // No vertical offset either.
    watermarkOptions.percentageVals = false;        // Indicates whether horizValue and vertValues 
                                                    //   (above) are percentages of the page size or not.
                                                    //   If not, they are in user units.

    //Transformation options.
    watermarkOptions.scale = 0.5f;           // Scale to draw watermark - 1.0f representing 100%.
    watermarkOptions.rotation = -25.0f;      // Counterclockwise rotation for watermark, in degrees.
    watermarkOptions.opacity  =  0.5f;       // Opacity for watermark, with 1.0f representing 100%.

    //Optional callbacks. 
    watermarkOptions.progMon = NULL;         // Optional progress monitor, to monitoring watermarking
    watermarkOptions.progMonData = NULL;     // Optional data to send to progress monitor.
    watermarkOptions.cancelProc = NULL;      // Optional cancel procedure function
    watermarkOptions.cancelProcData = NULL;  // Optional data to send to cancel procedure.

// Step 2) Set the text watermark parameters struct.

    PDDocWatermarkTextParamsRec textWatermarkOptions;
    memset(&textWatermarkOptions, 0, sizeof(PDDocWatermarkTextParamsRec));
    textWatermarkOptions.size = sizeof(PDDocWatermarkTextParamsRec);

// Step 2a) Create the ASText object for the text watermark
    
    //This is possible because ASCII is equivalent to UTF-8.
    ASText text = ASTextFromUnicode((ASUTF16Val*)csWatermarkText.c_str(), kUTF8);    
    textWatermarkOptions.srcText = text;
    textWatermarkOptions.textAlign = kPDHorizCenter;

// Step 2b) Prepare a font for the text watermark

    PDEFontAttrs fontAttrs;
    memset(&fontAttrs, 0, sizeof(fontAttrs));
    fontAttrs.name = ASAtomFromString("CourierStd");
    fontAttrs.type = ASAtomFromString("Type1");
    
    PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(fontAttrs), 0);
    PDEFont pdeFont = PDEFontCreateFromSysFont(sysFont, kPDEFontDoNotEmbed);
    
    textWatermarkOptions.pdeFont = pdeFont;
    textWatermarkOptions.sysFontName = fontAttrs.name;
    textWatermarkOptions.fontSize = 14.0f;

// Step 2c) Create a PDColorValueRec which specifies the color of the text.
    
    PDColorValueRec color;
    color.space = PDDeviceRGB;
    color.value[0] = fixedZero;  //Red value.
    color.value[1] = fixedZero;  //Green value.
    color.value[2] = fixedZero;  //Blue value. I've gone with the color black.
    textWatermarkOptions.color = color;

//Step 3) Add the page and text watermarks.

    //This page is our watermark image.
    PDPage pageWatermarkSource = watermarkDoc.getPage(0);
    PDDocAddWatermarkFromPDPage ( inDoc.getPDDoc(), pageWatermarkSource, &watermarkOptions );
    PDPageRelease(pageWatermarkSource);

    //We're going to modify our watermark parameters a bit for the text watermark.
    watermarkOptions.vertAlign = kPDVertBottom;
    watermarkOptions.scale = 1.0f;
    watermarkOptions.rotation = 30.0f;
    //This vertical offset ensures the text will be visible on the page.
    watermarkOptions.vertValue = textWatermarkOptions.fontSize;                             
    //We'll put this one on the last 2 pages...
    PDPageRange	placeHolder2 = {1, 2, PDAllPages};
    watermarkOptions.targetRange = placeHolder2;							 

    PDDocAddWatermarkFromText(inDoc.getPDDoc(), &textWatermarkOptions, &watermarkOptions);
    ASTextDestroy(text);
    PDERelease((PDEObject)pdeFont);

    //Save the document. APDFLDoc defaults to using the "PDSaveFull" flag while saving. 
    inDoc.saveDoc ( csOutputFileName.c_str() );
                                                                                              
    //(APDFLDoc's destructor takes care of closing the documents and releasing the rest of their resources.)
 
HANDLER
    errCode = ERRORCODE;
    libInit.displayError(errCode);
END_HANDLER

    return errCode;      //APDFLib's destructor terminates the library.
}
