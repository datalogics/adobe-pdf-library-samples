//
// Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// The sample demonstrates how to create color separations for spot color images.
// 
// Color separation is part of high volume offset printing processing. The original digital content
// is color separated to create a set of plates for printing, generally one plate per page for each
// of the primary colors—Cyan, Magenta, Yellow, and Black (CMYK). During printing each color layer is
// printed separately, one on top of the other, blended together to create the depth and variety of
// color in the final images. A spot color is a separate color added on top of the image after the
// four color plates are used to create the initial print run.
//
// For more detail see the description of the CreateSeparations sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#createseparations

#include <iostream>
#include <sstream>
#include <string>

#include "PERCalls.h"
#include "PEWCalls.h"
#include "PSFCalls.h"
#include "PagePDECntCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "CreateSeparations.h"

// Resolution of image desired for plates.  Process plates will be 4 times this resolution.
#define RESOLUTION (300.0)

#define FONT_NAME "MyriadPro-Regular"
#define FONT_TYPE "TrueType"

#define INPUT_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "RenderPage.pdf"
#define DEF_ROOT "Out_.pdf"

int main ( int argc, char *argv[] )
{
    std::string  csInputFileName, csOutputRootName;
    VerifyValidate ( csInputFileName, csOutputRootName, argc > 1 ? argv[1] : INPUT_LOC DEF_INPUT,
                     argc > 2 ? argv[2] : DEF_ROOT );
    std::cout << "Creating separations for " << csInputFileName.c_str() << 
                 " in files starting with " << csOutputRootName.c_str() << std::endl;

    APDFLib libInit;       // Initialize the library
    if ( libInit.isValid() == false )
    {
        ASErrorCode errCode = libInit.getInitError();
        APDFLib::displayError ( errCode );
        return errCode;
    }

    PDDoc       Doc;
    APDFLDoc* papdflDoc ( NULL );

    // Open the input as a PDF file
DURING
    papdflDoc = new APDFLDoc ( csInputFileName.c_str(), true );
    Doc = papdflDoc->getPDDoc();
HANDLER
    APDFLib::displayError ( ERRORCODE );
    return ERRORCODE;   
END_HANDLER

DURING
    
    // We create here the structures needed to add text to output pages
    PDEFontAttrs FontAttrs;
    memset(&FontAttrs, 0, sizeof(FontAttrs));
    FontAttrs.name = ASAtomFromString( FONT_NAME );
    FontAttrs.type = ASAtomFromString( FONT_TYPE );
    PDSysFont SysFont = PDFindSysFont(&FontAttrs, sizeof(PDEFontAttrs), 0);
    if (!SysFont ) 
    {
        std::cout << "Cannot obtain System Font " << FONT_NAME << " " << FONT_TYPE << std::endl;
        return 1;
    }
    PDSysFontGetAttrs (SysFont, &FontAttrs, sizeof(PDEFontAttrs));
    PDEFont Font = PDEFontCreateFromSysFont(SysFont, kPDEFontDoNotEmbed);

    PDEGraphicState   GState;
    PDEDefaultGState (&GState, sizeof (GState));

    ASDoubleMatrix TextMatrix;
    TextMatrix.a = TextMatrix.d = 62.0;
    TextMatrix.b = TextMatrix.c = 0.0;
    TextMatrix.h = 1 * 72.0;

    SeparationsContainer vSeparations;

    // For each page of the document
    ASInt32 numPages = PDDocGetNumPages ( Doc );
    for ( ASInt32 PageNumber = 0; PageNumber < numPages; ++PageNumber )
    {
        // Obtain the page
        PDPage Page = PDDocAcquirePage (Doc, PageNumber);
        // Create a list of spot colors
        CreateColorList ( Page, &vSeparations );

        /* Create a CMYK Bitmap of the page */
        ASInt32 Width, Depth;
        ASUns8* Image = CreateCMYKBitmap (Page, RESOLUTION, &Width, &Depth);
        ASInt32 Size ( Width * Depth * 4 );
        ASUns8* ImageCopy = new ASUns8[Size];  // Make a copy to put into page 1
        memmove ( ImageCopy, Image, Size );

        // Separate the image in to it's components
        CreateSeparation ( Image, Width, Depth, &vSeparations );

        // From this point on the program simply displays the separations built previously.
        // In other words, the code that follows is intended to verify that the three calls above,
        // CreateColorList, CreateCMYKBitmap and Create separations, completed successfully.

        // Create a document to hold the separation display.
        PDDoc OutDoc = PDDocCreate ();

        // Make the page one inch larger than the image (1/2 inch border on all sides)
        // to provide room to identify the separations.
        ASFixedRect PageSize;
        PageSize.left = PageSize.bottom = 0;
        PageSize.right = ASInt32ToFixed (Width) + ASInt32ToFixed ( 72 );
        PageSize.top = ASInt32ToFixed (Depth) + ASInt32ToFixed ( 72 );
        PDPage OutPage = PDDocCreatePage ( OutDoc, PDBeforeFirstPage, PageSize );

        // Get the container for the page
        PDEContent Content = PDPageAcquirePDEContent (OutPage, 0);

        // Make the whole image into a PDE image, and place it into the
        // first page. Use a copy of the original, because the process of
        // separating the original destroyed the content of the original
        PDEImageAttrs ImageAttrs;
        memset(&ImageAttrs, 0, sizeof(ImageAttrs));
        ImageAttrs.flags = kPDEImageExternal;
        ImageAttrs.height = Depth;
        ImageAttrs.width = Width;
        ImageAttrs.bitsPerComponent = 8;

        PDEFilterArray FilterArray;
        memset(&FilterArray, 0, sizeof(FilterArray));
        FilterArray.numFilters = 1;
        FilterArray.spec[0].name = ASAtomFromString("FlateDecode");

        ASFixedMatrix ImageMatrix;
        ImageMatrix.a = ImageAttrs.width * fixedOne;
        ImageMatrix.d = ImageAttrs.height * fixedOne;
        ImageMatrix.b = ImageMatrix.c = 0;
        ImageMatrix.h = fixedOne * 36;
        ImageMatrix.v = fixedOne * 36;

        PDEColorSpace ColorSpace;
        ColorSpace = PDEColorSpaceCreateFromName(ASAtomFromString ("DeviceCMYK"));
        PDEImage WholeImage;
        WholeImage = PDEImageCreate (&ImageAttrs, sizeof(ImageAttrs), &ImageMatrix, 0,
                     ColorSpace, NULL, &FilterArray, 0, (ASUns8 *)ImageCopy, Width * Depth * 4);
        delete[] ImageCopy;

        // Put this into the output document as page 1
        PDEContentAddElem (Content, kPDEBeforeFirst, (PDEElement) WholeImage);

        // Label it
        PDEText Text = PDETextCreate ();
        TextMatrix.v = Depth;
        PDETextAddEx (Text, kPDETextRun, 0, (ASUns8*)"Original Image", 14, Font, 
            &GState, sizeof (GState), NULL, 0, &TextMatrix, NULL);
        PDEContentAddElem (Content, kPDEAfterLast, (PDEElement) Text);
        PDERelease ((PDEObject) Text);
    
        // Set the content into the page
        PDPageSetPDEContent (OutPage, 0);

        // Release resources
        PDPageReleasePDEContent (OutPage, 0);
        PDPageRelease (OutPage);
        PDERelease ((PDEObject) WholeImage);
        PDERelease ((PDEObject) ColorSpace);

        // Create a page which has every component on it. This should
        // appear identical to the original page.
        OutPage = PDDocCreatePage (OutDoc, 0, PageSize);
        Content = PDPageAcquirePDEContent (OutPage, 0);
        ASUns32 CurrentColor;
        for (CurrentColor = 0; CurrentColor < vSeparations.size(); ++CurrentColor)
        {
            SEPARATION *Sep = vSeparations[CurrentColor];

            // Label it
            if (CurrentColor == 0)
            {
                Text = PDETextCreate ();
                PDETextAddEx (Text, kPDETextRun, 0, (ASUns8*)"Combined Image", 14, Font, 
                    &GState, sizeof (GState), NULL, 0, &TextMatrix, NULL);

                PDEContentAddElem (Content, kPDEBeforeFirst, (PDEElement) Text);
                PDERelease ((PDEObject) Text);
            }

        /* If we did not use this color, skip it */
        if (!Sep->m_present)
            continue;

        // Create a PDE image of this color, masked such that all
        // content which is not this color is transparent
        CreateMaskedImage (Sep);

        // Put all of these, overlaying each other, into the document as page 2
        PDEContentAddElem (Content, kPDEAfterLast, (PDEElement) Sep->m_masked_image);
        }

        PDPageSetPDEContent (OutPage, 0);
        PDPageReleasePDEContent (OutPage, 0);
        PDPageRelease (OutPage);

        // Create a page for each separate color
        for (CurrentColor = 0; CurrentColor < vSeparations.size(); CurrentColor++)
        {
            SEPARATION *Sep = vSeparations[CurrentColor];
    
            if (!Sep->m_present)
                continue;
            
            std::string ColorName ( ASAtomGetString ( Sep->m_name ) );
            if ( Sep->m_process_color )
            {
                ColorName.insert ( 0, "Process " );
            }
            OutPage = PDDocCreatePage (OutDoc, PDDocGetNumPages (OutDoc)-1, PageSize);
            Content = PDPageAcquirePDEContent (OutPage, 0);
    
            Text = PDETextCreate ();
            PDETextAddEx (Text, kPDETextRun, 0, (ASUns8 *)ColorName.c_str(), ColorName.length(), Font, 
                        &GState, sizeof (GState), NULL, 0, &TextMatrix, NULL);
            PDEContentAddElem (Content, kPDEBeforeFirst, (PDEElement) Text);
            PDERelease ((PDEObject) Text);
    
            PDEContentAddElem (Content, kPDEAfterLast, (PDEElement) Sep->m_masked_image);
            PDPageSetPDEContent (OutPage, 0);
            PDPageReleasePDEContent (OutPage, 0);
            PDPageRelease (OutPage);
        }

        // Save the output document
        std::ostringstream ossFileName;
        ossFileName << csOutputRootName.c_str() << PageNumber + 1 << ".pdf";
        ASPathName Path = APDFLDoc::makePath ( ossFileName.str().c_str() );
        PDDocSave (OutDoc, PDSaveFull | PDSaveCollectGarbage, Path, NULL, NULL, NULL);
        ASFileSysReleasePath (NULL, Path);
        PDDocRelease (OutDoc);
        PDDocClose (OutDoc);

        // Free resources for this page
        DestroyColorList ( vSeparations );
        delete[] Image;
        PDPageRelease (Page);
    }

    // Free resources
    PDERelease ((PDEObject) GState.fillColorSpec.space);
    PDERelease ((PDEObject) GState.strokeColorSpec.space);
    PDERelease ((PDEObject) Font);

HANDLER
    APDFLib::displayError ( ERRORCODE );
    delete papdflDoc;
    return ERRORCODE;   
END_HANDLER

    delete papdflDoc;
    return (0);
}

// This routine separates a CMYK image into 4 plates, one for each of those four colors.
// Each image will be an 8 bit single color image, representing the proportion of that color
// used in each pixel. The image will be at 4 times the resolution of the original image, and will
// use only one fourth of the pixels. The effect should be identical to a 25% screen image at 45 degrees.
//
// A separate mask image will be supplied. It will also be at 4 times the resolution of the 
// original image, but will be 1 bit per pixel. It will have turned on only those pixels which are 
// actually present in the color separation, and hence may be used to create a soft masked image, 
// suitable for display in PDF. This mask has no use in plate making.
void ProcessSeparation ( ASUns8 *OriginalImage, ASInt32 InWidth, ASInt32 InDepth, SeparationsContainer* sc )
{
    ASInt32     Width = InWidth * 2;
    ASInt32     Depth = InDepth * 2;
    ASInt32     RowWidth = ((Width + 7) / 8);

    // This routine will produce 4 masks. Each mask will be 4 
    // times the size of the spot color masks, twice as wide and 
    // twice as deep. This is so that there can be 4 pixels in each
    // of the process plates for each pixel in the original plate. 
    // The effect below is to create a plate for each of the 4 
    // process colors which is equivalent to a 25% 45 degree screened
    // print of the original

    // Set up the descriptions of Cyan, Magenta, Yellow, and Black
    SEPARATION *C = new SEPARATION ( Width, Depth, RowWidth, "Cyan" );
    C->m_cyan = true;
    sc->push_back ( C );

    SEPARATION *M = new SEPARATION ( Width, Depth, RowWidth, "Magenta" );
    M->m_magenta = true;
    sc->push_back ( M );

    SEPARATION *Y = new SEPARATION ( Width, Depth, RowWidth, "Yellow" );
    Y->m_yellow = true;
    sc->push_back ( Y );

    SEPARATION *K = new SEPARATION ( Width, Depth, RowWidth, "Black" );
    K->m_black = true;
    sc->push_back ( K );

    /* The following diagram is intended to "rotate" the
     * four pixels in the pattern

    -------------------------------------------------
    |CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|
    |YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|
    |CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|
    |YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|
    |CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|
    |YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|
    |CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|CM|KY|
    |YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|YK|MC|
    -------------------------------------------------

    */

    for ( ASInt32 Line = 0; Line < InDepth; Line++ )
    {
        for ( ASInt32 Row = 0; Row < InWidth; Row++ )
        {
            ASUns8    *Color = &OriginalImage[((Line * InWidth) + Row) * 4];
            ASUns32   ColorOut = ((Line * Width * 2) + (Row * 2));
            ASUns32   MaskOut = (Line * RowWidth * 2) + ((Row * 2)  / 8);

            // Mark the separation present if it has a non-zero value
            if (Color[0])
                C->m_present = true;
            if (Color[1])
                M->m_present = true;
            if (Color[2])
                Y->m_present = true;
            if (Color[3])
                K->m_present = true;

            switch (((Row % 2) + (Line % 2)) % 2)
            {
            case 0:
            {
                C->m_buffer[ColorOut] = Color[0];
                M->m_buffer[ColorOut+1] = Color[1];
                Y->m_buffer[ColorOut+Width] = Color[2];
                K->m_buffer[ColorOut+Width+1] = Color[3];
                if (Color[0] != 0)
                    C->m_mask[MaskOut] |= 1 << (7 - (((Row) % 4) * 2));
                if (Color[1] != 0)
                    M->m_mask[MaskOut] |= 1 << (7 - ((((Row) % 4) * 2) + 1));
                if (Color[2] != 0)
                    Y->m_mask[MaskOut+RowWidth] |= 1 << (7 - (((Row)% 4) * 2));
                if (Color[3] != 0)
                    K->m_mask[MaskOut+RowWidth] |= 1 << (7 - ((((Row) % 4) * 2) + 1));
                break;
            }
            case 1:
            {
                K->m_buffer[ColorOut] = Color[3];
                Y->m_buffer[ColorOut+1] = Color[2];
                M->m_buffer[ColorOut+Width] = Color[1];
                C->m_buffer[ColorOut+Width+1] = Color[0];
                if (Color[3] != 0)
                    K->m_mask[MaskOut] |= 1 << (7 - (((Row) % 4) * 2));
                if (Color[2] != 0)
                    Y->m_mask[MaskOut] |= 1 << (7 - ((((Row) % 4) * 2) + 1));
                if (Color[1] != 0)
                    M->m_mask[MaskOut+RowWidth] |= 1 << (7 - (((Row)% 4) * 2));
                if (Color[0] != 0)
                    C->m_mask[MaskOut+RowWidth] |= 1 << (7 - ((((Row) % 4) * 2) + 1));
                break;
            }
            }
        }
    }

    C->ReleaseBuffersIfAbsent();
    M->ReleaseBuffersIfAbsent();
    Y->ReleaseBuffersIfAbsent();
    K->ReleaseBuffersIfAbsent();

    return;
}

// This routine will create separation plates for spot colors. Each plate will be a 1 bit per pixel
// bitmap in the original image resolution. A true bit indicates that the spot color is present,
// a false bit indicates it is not present.
//
// As each color is imaged on a separate plate, it is removed from the original CMYK image. If there
// are no colors left in the CMYK image when the spot color is removed, the program will not create
// process color plates. If there are colors remaining, the program will create process color plates.
void CreateSeparation ( ASUns8 *OriginalImage, ASInt32 Width, ASInt32 Depth, SeparationsContainer* sc )
{
    ASUns32 *InputImage = (ASUns32*) OriginalImage;

    for ( ASUns32 CurrentColor = 0; CurrentColor < sc->size(); CurrentColor++ )
    {
        SEPARATION *Sep = (*sc)[CurrentColor];

        // Allocate a two tone bitmap to contain this color 
        // Even number of bytes per row
        ASUns32* Color = &Sep->m_cmyk;
        ASInt32 RowWidth = (Width + 7) / 8;
        Sep->ReallocBuffer ( Width, Depth, RowWidth );

        ASInt32 Row, Line;
        // Scan the image looking for Color. When we find one, turn on the corresponding bit in the 
        // separation bitmap.
        for (Line = 0; Line < Depth; Line++)
        {
            for (Row = 0; Row < Width; Row++)
            {
            ASUns32 *Pixel = &InputImage[(Line * Width) + Row];
            if (Color[0] == Pixel[0])
                {
                    Sep->m_buffer[(Line * RowWidth) + (Row / 8)] |= (1 << (7 - (Row % 8)));
                    Sep->m_present = true;
                    Pixel[0] = 0;
                }
            }
        }
        if (!Sep->m_present)
        {
            Sep->ReleaseBuffer();
        }
    }

    // Test if any color remains that was not separated
    ASBool UnSeparatedColor = false;
    for ( ASInt32 Line = 0; ( Line < Depth ) && !UnSeparatedColor ; Line++ )
    {
        for ( ASInt32 Row = 0; Row < Width; Row++ )
        {
            ASUns32 *Pixel = &InputImage[(Line * Width) + Row];
            if (Pixel[0] != 0)
            {
                UnSeparatedColor = true;
                break;
            }
        }
    }

    if (UnSeparatedColor)
    {
        ProcessSeparation (OriginalImage, Width, Depth, sc );
    }
    return;
}

// This routine will create a separation object for one given ink. 
// It will ignore process colors.
ASBool EnumerateInks (PDPageInk ink, void *clientData)
{
    /* Do not create separations for process colors */
    if (ink->isProcessColor)
    {
        return (true);
    }

    SeparationsContainer* sc = (SeparationsContainer*)clientData; 

    SEPARATION *s = new SEPARATION;

    s->m_name = ink->colorantName;
    s->AddCMYK ( ink->cyan, ink->magenta, ink->yellow, ink->black );

    sc->push_back ( s );

    return (true);
}

// This routine will create a list of separations for the spot colors defined on
// a given page. Note that it lists the colors defined. They may or may not be used
// on any given page.
void CreateColorList (PDPage Page, SeparationsContainer* sc )
{
    PDPageEnumInks (Page, EnumerateInks, (void *)sc, true);
}

// This routine will create a CMYK bitmap for the specified page.
// Note that resolution given is defined at the start of this program.
ASUns8 *CreateCMYKBitmap (PDPage Page, double Resolution, ASInt32 *Width, ASInt32 *Depth)
{
    ASInt32        ImageSize;
    ASUns8        *Image;
    ASFixedMatrix  Matrix, ScaleMatrix;
    ASFixedRect    PageSize;
    ASAtom         ColorSpace = ASAtomFromString ("DeviceCMYK");

    PDPageGetFlippedMatrix(Page, &Matrix);
    PDPageGetMediaBox (Page, &PageSize);
    Matrix.v += PageSize.bottom;
    Matrix.h += PageSize.left;

    ScaleMatrix.a = ScaleMatrix.d = FloatToASFixed (Resolution / 72.0);
    ScaleMatrix.b = ScaleMatrix.c = ScaleMatrix.h = ScaleMatrix.v = 0;
    ASFixedMatrixConcat (&Matrix, &ScaleMatrix, &Matrix);
    ASFixedMatrixTransformRect (&PageSize, &Matrix, &PageSize);

    ImageSize = PDPageDrawContentsToMemory (Page, kPDPageDoLazyErase, &Matrix, NULL, 
                        0, ColorSpace, 8, &PageSize, NULL, 0, NULL, NULL);

    Image = new ASUns8[ImageSize];
    ImageSize = PDPageDrawContentsToMemory (Page, kPDPageDoLazyErase, &Matrix, NULL, 
                        0, ColorSpace, 8, &PageSize, (char *) Image, ImageSize, NULL, NULL);
    *Width = ASFixedRoundToInt32 (PageSize.right - PageSize.left);
    *Depth = ASFixedRoundToInt32 (PageSize.top - PageSize.bottom);

    return (Image);

}

// This routine is present simply to verify results. A consumer of bitmaps would not
// need it. It creates a soft masked PDF Image for each plate.
void CreateMaskedImage (SEPARATION *Sep)
{
    PDEImageAttrs         ImageAttrs;
    PDEFilterArray        FilterArray;
    PDEImage              SepImage, MaskImage;
    PDEIndexedColorData   IColorDef;
    PDEColorSpaceStruct   ColorDef;
    PDEColorSpace         ColorSpace, GreySpace;
    ASFixedMatrix         ImageMatrix;
    ASUns8                ProcessColor[256 * 4];

    memset(&FilterArray, 0, sizeof(FilterArray));
    FilterArray.numFilters = 1;
    FilterArray.spec[0].name = ASAtomFromString("FlateDecode");

    if (!Sep->m_process_color)
    {
        ImageMatrix.a = Sep->m_width * fixedOne;
        ImageMatrix.d = Sep->m_depth * fixedOne;
        ImageMatrix.b = ImageMatrix.c = 0;
        ImageMatrix.h = fixedOne * 36;
        ImageMatrix.v = fixedOne * 36;

        memset(&ImageAttrs, 0, sizeof(ImageAttrs));
        ImageAttrs.flags = kPDEImageExternal;
        ImageAttrs.height = Sep->m_depth;
        ImageAttrs.width = Sep->m_width;
        ImageAttrs.bitsPerComponent = 1;

        memset ((char *)&IColorDef, 0, sizeof (PDEIndexedColorData));
        IColorDef.size =sizeof (PDEIndexedColorData);
        IColorDef.baseCs = PDEColorSpaceCreateFromName(ASAtomFromString ("DeviceCMYK"));
        IColorDef.hival = 1;

        memset (&ProcessColor[0], 0, 245 * 4);
        ProcessColor[0] = ProcessColor[1] = ProcessColor[2] = ProcessColor[3] = 0;
        ProcessColor[4] = Sep->m_cmyk >> 0;
        ProcessColor[5] = Sep->m_cmyk >> 8;
        ProcessColor[6] = Sep->m_cmyk >> 16;
        ProcessColor[7] = Sep->m_cmyk >> 24;
        IColorDef.lookup = (char *) ProcessColor;
        IColorDef.lookupLen = 8;

        ColorDef.indexed = &IColorDef;
        ColorSpace = PDEColorSpaceCreate(ASAtomFromString ("Indexed"), &ColorDef);
    }
    else
    {
        ASUns32 Count;

        ImageMatrix.a = Sep->m_width * fixedOne / 2;
        ImageMatrix.d = Sep->m_depth * fixedOne / 2;
        ImageMatrix.b = ImageMatrix.c = 0;
        ImageMatrix.h = 36 * fixedOne;
        ImageMatrix.v = 36 * fixedOne;

        memset(&ImageAttrs, 0, sizeof(ImageAttrs));
        ImageAttrs.flags = kPDEImageExternal;
        ImageAttrs.height = Sep->m_depth;
        ImageAttrs.width = Sep->m_width;
        ImageAttrs.bitsPerComponent = 8;

        memset ((char *)&IColorDef, 0, sizeof (PDEIndexedColorData));
        IColorDef.size =sizeof (PDEIndexedColorData);
        IColorDef.baseCs = PDEColorSpaceCreateFromName(ASAtomFromString ("DeviceCMYK"));
        IColorDef.hival = 255;
        IColorDef.lookup = (char *) ProcessColor;
        IColorDef.lookupLen = 256 * 4;

        memset (&ProcessColor[0], 0, 245 * 4);
        for (Count = 0; Count < 256; Count++)
        {
            ASUns8    *C = &ProcessColor[Count * 4];
            
            if (Sep->m_cyan)
                C[0] = Count;
            else
                C[0] = 0;

            if (Sep->m_magenta)
                C[1] = Count;
            else
                C[1] = 0;

            if (Sep->m_yellow)
                C[2] = Count;
            else
                C[2] = 0;

            if (Sep->m_black)
                C[3] = Count;
            else
                C[3] = 0;
        }
        ColorDef.indexed = &IColorDef;
        ColorSpace = PDEColorSpaceCreate(ASAtomFromString ("Indexed"), &ColorDef);
    }
    SepImage = PDEImageCreate (&ImageAttrs, sizeof(ImageAttrs), &ImageMatrix, 0,
                ColorSpace, NULL, &FilterArray, 0, Sep->m_buffer, 
                Sep->m_size);

    GreySpace = PDEColorSpaceCreateFromName(ASAtomFromString ("DeviceGray"));
    ImageAttrs.bitsPerComponent = 1;
    if (Sep->m_process_color)
        MaskImage = PDEImageCreate (&ImageAttrs, sizeof(ImageAttrs), &ImageMatrix, 0,
                    GreySpace, NULL, &FilterArray, 0, Sep->m_mask, Sep->m_mask_size);
    else
        MaskImage = PDEImageCreate (&ImageAttrs, sizeof(ImageAttrs), &ImageMatrix, 0,
                    GreySpace, NULL, &FilterArray, 0, Sep->m_buffer, Sep->m_size);
    PDEImageSetSMask(SepImage, MaskImage);
    Sep->m_masked_image = SepImage;

    PDERelease ((PDEObject) MaskImage);
    PDERelease ((PDEObject) IColorDef.baseCs);
    PDERelease ((PDEObject) ColorSpace);
    PDERelease ((PDEObject) GreySpace);

    return;
}

void VerifyValidate ( std::string& ifname, std::string& ofroot, const char* input, const char* output )
{

    ifname = input;
    ofroot = output;
    // If output file name already ends in ".pdf" remove the extension.
    std::size_t slen = ofroot.length();
    if ( ( slen > 4 ) && ( 0 == ofroot.compare ( slen - 4, 4, ".pdf" ) ) )
    {
        ofroot.erase ( slen - 4 );
    }
}

void DestroyColorList ( SeparationsContainer& sc )
{
    SeparationsContainer::iterator it, itE = sc.end();
    for ( it = sc.begin(); it != itE; ++it )
    {
        delete *it;
    }
    sc.clear();
}
