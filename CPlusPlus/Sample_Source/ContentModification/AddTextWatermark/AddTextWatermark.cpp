//
// Copyright (c) 2020, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This sample adds a text watermark to a documents contents, rather than using the Watermark Annotation.
// The resulting document is saved encrypted, with only view and Print permissions. The font used to render the watermark
// will be fully embedded in the output document. 
//
//  When a watermark is created using the watermark annotation, it is quite easy to remove it (simply remove the annotation). When it is 
//  embedded in the page content, it becomes a great deal harder to remove. If the document is also encrypted to prevent changes, it becomes
//  very difficult indeed to locate and remove the watermark.
//
//  This sample will set the specified text, scaled to fill the page, drawn on a baseline from the lower left to the upper right corner of the page. 
//  The text may be simply filled with a given color, or it may be filled with one color, and stroked with another. When opacituy is set to 1.0, this
//  becomes an overlay of the page, at lower values, it becomes an underlay.
//
// Command-line:   <input-file-name> <output-file-name> <Watermark text string> <font name> <Text Color> <Text Opacity> <Stroke Color> <Stroke Opacity> (All optional)
//
// For more detail see the description of the AddTextWatermark sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#AddTextWatermark



/* Presumptions
** The watermark to be used will be single test string, in a single font. The string an font name may be supplied from the command line,
** The text will be set on a basline running from lower left to upper right corners of the page, offset so as to keep the top of the type on 
** the page at the bottom, and scaled to fill the page. 
**
** Command line is:   Input File Name       Optional, defaults to Resources/SampleInput/AddWatermark.pdf
                      Output File Name      Optional, defaults to AddWatermark-out.pdf
                      Watermark Text        Optional, Defaults to "Datalogics Sample"
                      Font Name             Optional, defaults to "CourierStd" (Which is available on all platforms, via APDFL resources)
                      Text Color            Optional, defaults to black. Specified as 6 HEX digits, in RGB. If all three channels are the same, will be set in DeviceGray, otherwise, DeviceRGB.
                      Opacity               Optional, defaults to 0.2. Specificed a a real number between 0 and 1.0.
                      Stroke Color          Optional, defauls to not present, specified as text color is
                      Stroke Opacity        Optional, defaults to Opacity, Specified as a real number between 0 and 1.0
**
*/

#include <cstdio>
#include <cmath>

#include "APDFLDoc.h"
#include "InitializeLibrary.h"

#include "PERCalls.h"
#include "PEWCalls.h"
#include "PSFCalls.h"
#include "CosCalls.h"
#include "ASCalls.h"
#include "PagePDECntCalls.h"

#ifdef WIN_PLATFORM
#include "io.h"
#define access _access
#endif

#ifdef UNIX_PLATFORM
#include <unistd.h>
#endif

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "AddWatermark.pdf"
#define DEF_OUTPUT "AddWatermark-out.pdf"

typedef struct colorspec
{
    double R, G, B;
} ColorSpec;

typedef struct copyStreamInfo
{
    CosObj pageArray;
    ASStm currentStream;
    int currentEntry;
    char *watermark;
    bool  watermarkStarted;
    int watermarkPosition;
    int watermarkEndPosition;
} CopyStreamInfo;




/* This routine will copy the COS content of the page into a single new stream, and 
** append the watermark to the end of that stream. It will add the COS font created from the 
** PDEFont to the page resources as "Fnt1", and, if a ExtGState is needed, it will add that to the 
** page resources as "EXg1".
**
** The first half is the actual stream to stream copy procedure. The second sets up for it, including the 
** creation of the watermark content
*/
ASTCount CopyStreamAddingWatermark(char *data, ASTArraySize nData, void *clientData)
{
    /* This procedure is called to get the next block of content to write into the 
    ** new stream. We will provide content from the original content streams, until they 
    ** are exhausted, then copy the watermark content. Then return EOF
    */
    CopyStreamInfo *info = (CopyStreamInfo *)clientData;

    /* If we have finished copying the stream, but did not complete the 
    ** watermark, complete the watermark here
    */
    if (info->watermarkStarted)
    {
        ASTArraySize length = nData;
        if ((info->watermarkEndPosition - info->watermarkPosition) < nData)
            length = info->watermarkEndPosition - info->watermarkPosition;

        /* A return code of zero is EOF*/
        if (length == 0)
            return (0);

        strncpy(data, &info->watermark[info->watermarkPosition], length);
        info->watermarkPosition += length;
        return (length);
    }

    ASTArraySize bytesRead = 0;
    if (!info->currentStream)
    {
        /* The first time we are called is the only time this will be true. 
        ** so we open a stream for the first entry in the array.
        */
        CosObj work = CosArrayGet(info->pageArray, 0);
        info->currentStream = CosStreamOpenStm(work, cosOpenFiltered);

        /* Insert a "save" before all stream contentm then
        ** Copy from the current stream to the new stream */
        data[0] = 'q';
        data[1] = ' ';
        bytesRead = ASStmRead(data+2, 1, nData-2, info->currentStream) + 2;
    }
    else
    {
        /* Copy from the current stream to the new stream */
        bytesRead = ASStmRead(data, 1, nData, info->currentStream);
    }

    if (bytesRead == nData)
        return (bytesRead);


    /* If the number of bytes copied is less than the size of the buffer
    ** open the next stream
    */
    while (bytesRead < nData)
    {
        ASStmClose(info->currentStream);
        info->currentEntry += 1;
        if (info->currentEntry >= CosArrayLength(info->pageArray))
        {
            /* We have finished the last original cos stream*/
            info->watermarkStarted = true;
            return (bytesRead);
        }
        CosObj work = CosArrayGet(info->pageArray, info->currentEntry);
        info->currentStream = CosStreamOpenStm(work, cosOpenFiltered);
        ASTArraySize bytesRead2 = ASStmRead(&data[bytesRead], 1, nData-bytesRead, info->currentStream);
        bytesRead += bytesRead2;
    }

    if (bytesRead == nData)
        return (bytesRead);

    /* Close the last stream */
    ASStmClose(info->currentStream);

    /* Just for safety sake. If the page content is empty, we will not include the 
    ** water mark. So mae sure we do NOT return zero here, and include the water mark text if we were going to return 0
    */
    if ((bytesRead == 0) && (info->watermarkStarted))
    {
        ASTArraySize length = nData;
        if ((info->watermarkEndPosition - info->watermarkPosition) < nData)
            length = info->watermarkEndPosition - info->watermarkPosition;

        /* A return code of zero is EOF*/
        if (length == 0)
            return (0);

        strncpy(data, &info->watermark[info->watermarkPosition], length);
        info->watermarkPosition += length;
        return (length);
    }

    return (bytesRead);
}

void  AddWatermarkToPage(CosObj cosPage, ASDoubleMatrix matrix, char *text, CosObj font, ColorSpec *textColor,double textOpacity, ColorSpec *strokeColor, double strokeOpacity)
{
    /* Get the COsDoc for the page object, and hang on to it for later */
    CosDoc cosDoc = CosObjGetDoc(cosPage);

    /* Get the page contents */
    CosObj content = CosDictGetKeyString(cosPage, "Contents");

    /* If the page contents is not already an array, put it into an array
    ** (This simplifies the copy, as it will then always see and array)
    */
    if (CosObjGetType(content) != CosArray)
    {
        CosObj work = CosNewArray(cosDoc, false, 1);
        CosArrayPut(work, 0, content);
        content = work;
    }

    /* Create the command string to draw the watermark
    ** I am allocating a buffer MUCH, MUCH larger than I should ever need!
    */
    char watermark[4096];
    char work[1024];
    watermark[0] = 0;

    /* insert a POP to match the push we added before the stream, 
    ** then insert a push, and a cm to the matrix supplied
    */
    strcat(watermark, "Q q ");
    sprintf(work, "%1.5f %1.5f %1.5f %1.5f %1.5f %1.5f cm\n", matrix.a, matrix.b, matrix.c, matrix.d, matrix.h, matrix.v);
    strcat(watermark, work);

    /* Start a text block */
    strcat(watermark, "BT\n");

    /* Set the font Size it to 12 points */
    strcat(watermark, "/Fnt1 12 Tf\n");

    /* Set the default values for text */
    strcat(watermark, "0 Tc 0 Tw 0 Ts 100 Tz 1 0 0 1 0 0 Tm ");

    /* If we are going to stroke the text, we need to set Tr to 2, and we need to set the line
    ** width to 0.5. We also need to set the stroke color
    */
    if (strokeColor)
    {
        strcat(watermark, "2 Tr 0.5 w\n");
        if ((strokeColor->R == strokeColor->G) && (strokeColor->R == strokeColor->B))
            sprintf(work, "%1.3f G\n", strokeColor->R);
        else
            sprintf(work, "%1.3f %1.3f %1.3f RG\n", strokeColor->R, strokeColor->G, strokeColor->B);
        strcat(watermark, work);
    }
    else
        strcat(watermark, "0 Tr\n");

    /* Set the text color */
    /*if the color channels are all the same, use gray, otheriwse use RGB */
    if ((textColor->R == textColor->G) && (textColor->R == textColor->B))
        sprintf(work, "%1.3f g\n", textColor->R);
    else
        sprintf(work, "%1.3f %1.3f %1.3f rg\n", textColor->R, textColor->G, textColor->B);
    strcat(watermark, work);

    CosObj opacityGS = CosNewNull();

    /* If fill or stroke opacity is not 1.0, create an extGState, and reference it here
    */
    if (textOpacity != 1.0)
    {
        strcat(watermark, "/EXg1 gs\n");
        opacityGS = CosNewDict(cosDoc, true, 3);
        CosDictPutKeyString(opacityGS, "Type", CosNewName(cosDoc, false, ASAtomFromString("ExtGState")));
        CosDictPutKeyString(opacityGS, "ca", CosNewDouble(cosDoc, false, textOpacity));
        CosDictPutKeyString(opacityGS, "CA", CosNewDouble(cosDoc, false, strokeOpacity));
    }

    /* Write the text string 
    */
    sprintf(work, "(%s) Tj\n", text);
    strcat(watermark, work);

    /* end the text, and pop the stack*/
    strcat(watermark, "ET\nQ");

    /* Create a new stream object, by copying all of the original stream contents into that new object, and then 
    ** appending the watermark to the end of it
    */
    CopyStreamInfo info;
    info.pageArray = content;
    info.currentEntry = 0;
    info.currentStream = NULL;
    info.watermark = watermark;
    info.watermarkStarted = false;
    info.watermarkPosition = 0;
    info.watermarkEndPosition = strlen(watermark);

    ASStm oldCosStream = ASProcStmRdOpen(CopyStreamAddingWatermark, &info);
    CosObj attributesDict = CosNewDict(cosDoc, false, 2);
    CosDictPutKeyString(attributesDict, "Filter", CosNewName(cosDoc, false, ASAtomFromString("FlateDecode")));
    CosObj newContent = CosNewStream(cosDoc, true, oldCosStream, 0, true, attributesDict, CosNewNull(), -1);

    /* Free the stream */
    ASStmClose(oldCosStream);

    /* Replace the page contents */
    CosDictPutKeyString(cosPage, "Contents", newContent);

    /* Add the resources */
    CosObj resources = CosDictGetKeyString(cosPage, "Resources");
    if (CosObjGetType(resources) == CosNull)
    {
        resources = CosNewDict(cosDoc, false, 2);
        CosDictPutKeyString(cosPage, "Resources", resources);
    }

    CosObj fonts = CosDictGetKeyString(resources, "Font");
    if (CosObjGetType(fonts) == CosNull)
    {
        fonts = CosNewDict(cosDoc, false, 1);
        CosDictPutKeyString(resources, "Font", fonts);
    }
    CosDictPutKeyString(fonts, "Fnt1", font);

    if (CosObjGetType(opacityGS) == CosDict)
    {
        CosObj extGS = CosDictGetKeyString(resources, "ExtGState");
        if (CosObjGetType(extGS) == CosNull)
        {
            extGS = CosNewDict(cosDoc, false, 1);
            CosDictPutKeyString(resources, "ExtGState", extGS);
        }
        CosDictPutKeyString(extGS, "EXg1", opacityGS);
    }

}

int main(int argc, char **argv) {
    ASErrorCode errCode = 0;
    APDFLib libInit;

    if (libInit.isValid() == false) {
      errCode = libInit.getInitError();
      std::cout << "Initialization failed with code " << errCode << std::endl;
      return errCode;
    }

    std::string csInputFileName(argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT);
    std::string csOutputFileName(argc > 2 ? argv[2] : DEF_OUTPUT);
    std::string csWaterMarkText(argc > 3 ? argv[3] : "Datalogics Sample");
    std::string csWaterMarkFont(argc > 4 ? argv[4] : "CourierStd");
    
    if (access(csInputFileName.c_str(), 4))
    {
        std::cout << "Input File Name " << csInputFileName.c_str() << " can not be opened for read!" << std::endl;
        return (-1);
    }

    std::cout << "Adding text watermark to all pages of "
              << csInputFileName.c_str() << "and saving to "
              << csOutputFileName.c_str() << std::endl;

    double textOpacity = 0.2;
    ColorSpec textColor;
    textColor.R = 0;
    textColor.G = 0;
    textColor.B = 0;

    bool strokeText = false;
    double strokeOpacity = 0.2;
    ColorSpec strokeColor;
    strokeColor.R = 0;
    strokeColor.G = 0;
    strokeColor.B = 0;

    if (argc > 5)
    {
        unsigned int r, g, b;
        bool badFormat = false;
        if (strlen(argv[5]) != 6)
            badFormat = true;
        else
        {
            int found = sscanf(argv[5], "%2x%2x%2x", &r, &g, &b);
            if (found != 3)
                badFormat = true;
        }
        if (badFormat)
        {
           std::cout << "The fifth argument, if present, must be the color of the type. It must be three hex couplets. It is present, but not parsable. \"" << argv[5] << "\"" << std::endl;
           return (-1);
        }

        if (r != 0)
            textColor.R = ((r + 1) * 1.0) / 256;
        if (g != 0)
            textColor.G = ((g + 1) * 1.0) / 256;
        if (b != 0)
            textColor.B = ((b + 1) * 1.0) / 256;
    }

    if (argc > 6)
    {
        float opacity;
        int found = sscanf(argv[6], "%f", &opacity);
        if ((found != 1) || (opacity > 1.0) || (opacity < 0))
        {
            std::cout << "The sixth argument, if present, must be the opacity of the type. It must be a real number in value between 0 and 1.0. It is present, but not parsable. \"" << argv[6] << std::endl;
            return (-1);
        }
        textOpacity = opacity;
        strokeOpacity = opacity;
    }

    if (argc > 7)
    {
        unsigned int r, g, b;
        bool badFormat = false;
        if (strlen(argv[5]) != 6)
            badFormat = true;
        else
        {
            int found = sscanf(argv[7], "%2x%2x%2x", &r, &g, &b);
            if (found != 3)
                badFormat = true;
        }
        if (badFormat)
        {
            std::cout << "The seventh argument, if present, must be the color to stroke type in. It must be three hex couplets. It is present, but not parsable. \"" << argv[5] << "\"" << std::endl;
            return (-1);
        }

        strokeText = true;

        if (r != 0)
            strokeColor.R = ((r + 1) * 1.0) / 256;
        if (g != 0)
            strokeColor.G = ((g + 1) * 1.0) / 256;
        if (b != 0)
            strokeColor.B = ((b + 1) * 1.0) / 256;
    }

    if (argc > 8)
    {
        float opacity;
        int found = sscanf(argv[8], "%f", &opacity);
        if ((found != 1) || (opacity > 1.0) || (opacity < 0))
        {
            std::cout << "The eight argument, if present, must be the opacity of the type outline. It must be a real number in value between 0 and 1.0. It is present, but not parsable. \"" << argv[6] << std::endl;
            return (-1);
        }
        strokeOpacity = opacity;
    }

    DURING
        /* Open the document we are going to watermark
        ** We need to do this first, so that we can create a copy of the COS image
        ** in this document.
        */
        APDFLDoc inDoc(csInputFileName.c_str(), true);

        /* Get the PDDoc, as we will need it a lot */
        PDDoc pdDoc = inDoc.getPDDoc();

        /* Create a PDETExtObject to render the text we want to see in the document.
        ** We are going to use this to obtain the width and height of the watermark
        */
        PDEText textItem = PDETextCreate();

        PDEFontAttrs fontAttrs;
        memset((char *)&fontAttrs, 0, sizeof(PDEFontAttrs));
        fontAttrs.name = ASAtomFromString(csWaterMarkFont.c_str());
        fontAttrs.charSet = ASAtomFromString("Roman");
        PDSysFont sysFont = PDFindSysFont(&fontAttrs, sizeof(PDEFontAttrs),
                                          kPDSysFontMatchNameAndCharSet);

        /* If the font cannot be found on the system, end here */
        if (!sysFont)
        {
            std::cout << "We were not able to create a PDF Font using the name (" << csWaterMarkFont.c_str() << " supplied for the font." << std::endl;
            return (-1);
        }

        PDEFont pdeFont = PDEFontCreateFromSysFont(sysFont, kPDEFontCreateEmbedded);

        PDEGraphicState gState;
        PDEDefaultGState(&gState, sizeof(PDEGraphicState));
        PDETextState tState;
        memset((char *)&tState, 0, sizeof(PDETextState));
        ASDoubleMatrix matrix = {12.0, 0, 0, 12.0, 0, 0.0};

        PDETextAddEx(textItem, kPDETextRun, kPDEBeforeFirst, (ASUns8 *)csWaterMarkText.c_str(),
                     csWaterMarkText.size(), pdeFont, &gState,
                     sizeof(PDEGraphicState), &tState, sizeof(PDETextState), &matrix,
                     NULL);

        /* Get the text size
        */

        ASFixedRect textBBox;
        PDETextGetBBox(textItem, kPDETextRun | kPDETextPreciseQuad, 0, &textBBox);

        double width = ASFixedToFloat (textBBox.right - textBBox.left);
        double height = ASFixedToFloat(textBBox.top - textBBox.bottom);

        /* Release the text, we don't need it any more
        */
        PDERelease((PDEObject)textItem);

        /* Make a copy of the PDEFont's COS Obj in the destination document, as a 
        ** fully embedded font
        */
        PDEFontEmbedNow(pdeFont, PDDocGetCosDoc(pdDoc));
        CosObj cosFont;
        PDEFontGetCosObj(pdeFont, &cosFont);

        /* And we don't need the PDEFont any longer
        */
        PDERelease((PDEObject)pdeFont);

        cosFont = CosObjCopy(cosFont, PDDocGetCosDoc(pdDoc), true);

        /* Add the watermark to each page
         */
        for (int page = 0; page < PDDocGetNumPages(pdDoc); page++) {
            PDPage pdPage = PDDocAcquirePage(pdDoc, page);

            /* Get the page size */
            ASFixedRect mediaBox;
            PDPageGetMediaBox(pdPage, &mediaBox);
            double pageWidth = ASFixedToFloat(mediaBox.right - mediaBox.left);
            double pageHeight = ASFixedToFloat(mediaBox.top - mediaBox.bottom);

            /* Scale and Rotate the text box to draw text from the lower left corner of the page
            ** to the upper right. 
            */

            /* Find the angle of the baseline
            */
            double diagonal = atan2(pageWidth, pageHeight);
            double Sin = sin(diagonal);
            double Cos = cos(diagonal);
            double Tan = tan(diagonal);

            double baseLength = pageHeight / Cos;

            /* As a first approximation of the text size, scale to match the 
            ** width of the text to diagonal length
            */
            double scale = baseLength / width;

            /* Now, If I just rotate the text, and set it on this baseline, then the 
            ** Upper left corner of the first character will be off the page, So I need to 
            ** move the text up the diagonal enough to clear the first character. This is 
            ** actually a function of "how high does the text raise above the baseline", 
            ** but we dont' need a perfect fit, and in fact would prefer to be a little away
            ** from the edge, so I am going to use text height instead
            */
            double textBackoffX = height * Sin * scale;
            double textBackoffY = height * Cos * scale ;
            double effectivelength = baseLength - ((textBackoffX / Cos) * 2);

            /* Now, lower the scale to effective Length
            */
            scale = effectivelength / width;
            
            /* Now, put this into a matrix!*/
            ASDoubleMatrix matrix;
            matrix.h = textBackoffX;
            matrix.v = textBackoffY;
            matrix.a = Sin * scale;
            matrix.b = Cos * scale;
            matrix.c = -Cos * scale;
            matrix.d = Sin * scale;

            CosObj cosPage = PDPageGetCosObj(pdPage);
            PDPageRelease(pdPage);

            if (strokeText)
                AddWatermarkToPage(cosPage, matrix, (char *)csWaterMarkText.c_str(), cosFont, &textColor, textOpacity, &strokeColor, strokeOpacity);
            else
                AddWatermarkToPage(cosPage, matrix, (char *)csWaterMarkText.c_str(), cosFont, &textColor, textOpacity, NULL, textOpacity);

        }


        /* Add encryption to the document.*/
        StdSecurityData securityData = NULL;
        PDDocSetNewCryptHandler(pdDoc, ASAtomFromString("Standard"));
        securityData = (StdSecurityData)PDDocNewSecurityData(pdDoc);
        securityData->size = sizeof(StdSecurityDataRec);
        /* Specifically allow the document to be printed, But pretty much nothing else */
        securityData->perms |= pdPrivPermAccessible | pdPermPrint;

        PDDocSetNewSecurityData(pdDoc, (void *)securityData);


        // Save the document. APDFLDoc defaults to using the "PDSaveFull" flag while
        // saving.
        inDoc.saveDoc(csOutputFileName.c_str());

    HANDLER
        errCode = ERRORCODE;
        libInit.displayError(errCode);
    END_HANDLER

    return errCode; // APDFLib's destructor terminates the library.
}
