//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The RenderPage sample program shows how to render a PDF document page to memory.
//
// For more detail see the description of the RenderPage sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#renderpage

#include "RenderPage.h"

//Statics
ASAtom RenderPage::sDeviceRGB_K;
ASAtom RenderPage::sDeviceCMYK_K;
ASAtom RenderPage::sDeviceGray_K;

// Constructor
RenderPage::RenderPage(PDPage &pdPage, const char *colorSpace, const char *filterName, ASInt32 inBPC, float inResolution)
{
    // Set up the static colorspace atoms
    sDeviceRGB_K = ASAtomFromString("DeviceRGB");
    sDeviceCMYK_K = ASAtomFromString("DeviceCMYK");
    sDeviceGray_K = ASAtomFromString("DeviceGray");

    //If you are using a decode filter such as FlateDecode, the filterArray values will be set here
    filterArray = SetFilter(filterName);

    //Set resolution.  The default resolution is 72 units per inch.  Thus a value of 72.0 is the same
    //  To double the resolution, for example, you would set the value to 144.0.
    resolution = SetResolution(inResolution);

    //Get the colorspace atom, set the number of components per colorspace
    //and store the appropriate colorspace for an output PDEImage
    csAtom = SetColorSpace(colorSpace);

    //The stream of data being rasterized to memory is divided into units of n bits each, 
    //  where n is the number of bits per component.  
    bpc = SetBPC(inBPC);

    //Gets the matrix that transforms user space coordinates to rotated and cropped coordinates.
    //  The origin of this space is the top-left of the rotated, cropped page. Y is decreasing
    PDPageGetFlippedMatrix(pdPage, &matrix);

    //Gets the crop box for a page. The crop box is the region of the page to display and print
    PDPageGetCropBox(pdPage, &pageRect);

    //Set page coordinates/rectangle to crop box for PDDocCreatePage
    destRect = SetPageRect(pageRect);

    //Set the scale matrix that will be concatenated to the user space matrix
    scaleMatrix = SetScaleMatrix(resolution);

    //Apply the scale to the default matrix
    ASFixedMatrixConcat(&matrix, &scaleMatrix, &matrix);
    ASFixedMatrixTransformRect(&scaledDestRect, &scaleMatrix, &destRect);

    //Allocate the buffer fo storing the rendered page content,
    //  and initialize to background of white as appropriate for the colorspace selected
    bufferSize = SetBufferSize(pdPage, matrix, csAtom, bpc, scaledDestRect);
    buffer =   new char[bufferSize];
    char bkgColor = csAtom == sDeviceCMYK_K ? 0x0 : 0xff;
    memset(buffer, bkgColor, bufferSize);

    // Leave the memory buffer uninitialized for non-Device color spaces.

    // Render page content to the bitmap buffer
    PDPageDrawContentsToMemory ( pdPage,
                                 kPDPageDoLazyErase | kPDPageUseAnnotFaces,
                                 &matrix, 
                                 NULL,
                                 kPDPageDrawSmoothText | kPDPageDrawSmoothLineArt | kPDPageDrawSmoothImage,
                                 csAtom, 
                                 bpc, 
                                 &scaledDestRect, 
                                 buffer, 
                                 bufferSize,
                                 NULL, NULL);

    // Set up attributes for the PDEImage made by MakePDEImage - these attributes are also
    // used to repad the output buffer (from 32-bit row alignment to 8-bit alignment) in
    // the PadCompute function below
    SetImageAttrs(scaledDestRect, bpc);

    //If the number of data bits per row is not a multiple of 8, the end of the row is padded with 
    //  extra bits to fill out the last byte. A PDF consumer application ignores these padding bits.
    attrs.width = PadCompute(attrs, bpc, nComps, buffer, bufferSize);
}

RenderPage::~RenderPage() 
{ 
    if(buffer)
    {
        delete[] buffer;
    }
    buffer = 0;

    PDERelease(reinterpret_cast<PDEObject>(image));
    PDERelease(reinterpret_cast<PDEObject>(cs));
}

char * RenderPage::GetImageBuffer()
{
    return buffer;
}

ASInt32 RenderPage::GetImageBufferSize()
{
    return bufferSize;
}

ASFixedRect RenderPage::GetImageRect()
{
    return pageRect;
}

PDEImage RenderPage::MakePDEImage()
{
    //Prepare the image attributes
    attrs = SetImageAttrs(scaledDestRect, bpc);

    //Create the image matrix using the height/width attributes and apply the resolution.
    imageMatrix = SetImageMatrix(attrs, resolution);

    // Create an image XObject from the bitmap buffer to embed in the output document
    image = PDEImageCreate ( &attrs, 
                             sizeof(attrs),
                             &imageMatrix, 
                             0,
                             cs, 
                             NULL,
                             &filterArray, 
                             0,
                             (unsigned char*) buffer, 
                             bufferSize);

    return image;
}

PDEImageAttrs RenderPage::SetImageAttrs(ASFixedRect scaledDestRect, ASInt32 bpc)
{
    memset(&attrs, 0, sizeof(PDEImageAttrs));
    attrs.flags = kPDEImageExternal;
    attrs.height = abs(ASFixedRoundToInt16(scaledDestRect.top) - ASFixedRoundToInt16(scaledDestRect.bottom));
    attrs.width = abs(ASFixedRoundToInt16(scaledDestRect.right) - ASFixedRoundToInt16(scaledDestRect.left));
    attrs.bitsPerComponent = bpc;
    return attrs;
}

PDEFilterArray RenderPage::SetFilter(const char *filterName)
{    
    memset(&filterArray, 0, sizeof(PDEFilterArray));

    if(filterName)
    {
        filterArray.numFilters = 1;
        filterArray.spec[0].name = ASAtomFromString(filterName);
    }
    return filterArray;
}

//Apply the proportional resolution, width, and  height to the image matrix. 
ASFixedMatrix RenderPage::SetImageMatrix(PDEImageAttrs attrs, float resolution)
{    
    imageMatrix.a = FloatToASFixed(attrs.width / (resolution / 72.0));
    imageMatrix.d = FloatToASFixed(attrs.height / (resolution / 72.0));
    imageMatrix.b = imageMatrix.c = 0;
    imageMatrix.h = imageMatrix.v = 0;
    return imageMatrix;
}

//Create a matrix to use to increase or decrease the default matrix via concatenation
ASFixedMatrix RenderPage::SetScaleMatrix(float resolution)
{   
    scaleMatrix.a = scaleMatrix.d = FloatToASFixed(resolution / 72.0);
    scaleMatrix.b = scaleMatrix.c = scaleMatrix.h = scaleMatrix.v = 0;
    return scaleMatrix;
}

ASFixedRect RenderPage::SetPageRect(ASFixedRect destRect)
{
    pageRect.left = pageRect.bottom = 0;
    pageRect.right = destRect.right - destRect.left;
    pageRect.top = destRect.top - destRect.bottom;
    return pageRect;
}

// Validate colorspace selection and set the channels per color
ASAtom RenderPage::SetColorSpace(const char *colorSpace)
{    
    csAtom = ASAtomFromString ( colorSpace );
    if ( csAtom == sDeviceGray_K )
    {
        nComps = 1;
    }
    else if ( csAtom == sDeviceRGB_K )
    {
        nComps = 3;
    }
    else if ( csAtom == sDeviceCMYK_K )
    {
        nComps = 4;
    }
    else
    {
        // Not a valid colorspace
        ASRaise(genErrBadParm);
    }

    // initialize the output colorspace for the PDEImage we'll generate in MakePDEImage
    cs = PDEColorSpaceCreateFromName(csAtom);

    return csAtom;
}

// Set the BPC depending on the colorspace being used
ASInt32 RenderPage::SetBPC(ASInt32 bitsPerComp)
{   
    bpc = bitsPerComp;
    if(csAtom==sDeviceRGB_K || csAtom==sDeviceCMYK_K)
    {
        if(bitsPerComp!=8)
        {
            // Reset to 8
            bpc = 8;
        }
    }
    if(csAtom==sDeviceGray_K)
    {
        if(bitsPerComp == 1 || bitsPerComp == 8 || bitsPerComp == 24)
        {}else
        {
            // Reset to 8
            bpc = 8;
        }
    }
    return bpc;
}

ASInt32 RenderPage::PadCompute(PDEImageAttrs attrs, ASInt32 bpc, ASInt32 nComps, char *buffer, ASInt32 bufferSize)
{   
    // The bitmap data generated by PDPageDrawContentsToWindow is 32-bit aligned. 
    // The PDF image operator expects, however, 8-bit aligned image data. 
    // To remedy this difference, we check to see if the 32-bit aligned width
    // is different from the 8-bit aligned width. If so, we fix the image data by 
    // stripping off the padding at the end
    if (((((attrs.width * bpc * nComps) + 31) / 32) * 4) != ((attrs.width * bpc * nComps) /    8))
    {
        char *src, *dest;
        int sw, dw;                    
        sw = ((((attrs.width * bpc * nComps) + 31) / 32) * 4);
        if (bpc == 1)
            dw = attrs.width / 8 + ((attrs.width % 8) ? 1 : 0);
        else
            dw = (attrs.width * bpc * nComps) / 8;
        src = dest = buffer;
        // Copy the source bytes to the destination
        for (int i = 0; i < attrs.height; i++) {
            for (int j = 0; j < dw; j++) 
                dest[j] = src[j];
            src += sw; dest += dw;
        }
        // Recalculate buffer size
        bufferSize = dw * attrs.height;
    } else {
        attrs.width = (((( attrs.width* bpc * nComps) + 31) / 32) * 4) * 8 / (bpc * nComps);
    }
    return attrs.width;
}

//Calculate buffer size needed to for page contents
ASInt32 RenderPage::SetBufferSize(PDPage pdPage, ASFixedMatrix &matrix, ASAtom csAtom, ASInt32 bpc, ASFixedRect scaledDestRect)
{   
    bufferSize = PDPageDrawContentsToMemory ( pdPage,
                                              kPDPageDoLazyErase,
                                              &matrix, 
                                              NULL,
                                              0,
                                              csAtom,
                                              bpc,
                                              &scaledDestRect,
                                              NULL,
                                              0, 
                                              NULL, 
                                              NULL);
    return bufferSize;
}

//Correct for a resolution lower than or equal to zero
float RenderPage::SetResolution(float inResolution)
{   
    if(inResolution<=0.0)
        resolution=72.0;
    else
        resolution = inResolution;
    return resolution; 
}
