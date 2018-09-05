//
// Copyright (c) 2018, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// The TextSelectEnum sample demonstrates how the Library is able to select all of the 
// text present in an area of the page. 
//
// Command-line:  <output-file>     (Optional)
//
// For more detail see the description of the UnicodeText sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#TextSelectEnum

#include "APDFLDoc.h"
#include "InitializeLibrary.h"
#include "DLExtrasCalls.h"
#include "ASExtraCalls.h"
#include "PagePDECntCalls.h"
#include "PEWCalls.h"
#include "PERCalls.h"
#include <vector>

#define DIR_LOC "../../../../Resources/Sample_Input/"
#define DEF_INPUT "TextSearch.pdf"
#define DEF_OUTPUT "TextSelectEnum-out.pdf"

// A structure to hold the information returned by text select enum
typedef struct enumeratedRun
{
    PDFont          font;
    ASFixed         size;
    PDColorValueRec color;
    ASText          text;
} EnumeratedRun;

// A typedef to hold a list of words.
typedef std::vector<EnumeratedRun> wordList;

// This is the callback routine used to add one text run to the list of text runs
ASBool enumSelectedTextUCS (void *procObj, PDFont font, ASFixed size, PDColorValue color, char *text, ASInt32 textLen)
{
    wordList *words = (wordList *)procObj;

    EnumeratedRun thisWord;
    thisWord.text = ASTextFromSizedUnicode ((ASUTF16Val *)text, kUTF16BigEndian, textLen);
    thisWord.font = font;
    thisWord.size = size;
    memcpy ((char *)&thisWord.color, (char *)color, sizeof (PDColorValueRec));

    words->push_back (thisWord);
    return (true);
}


int main(int argc, char** argv)
{
    APDFLib libInit;
    ASErrorCode errCode = 0;
    if (libInit.isValid() == false)
    {
        errCode = libInit.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return libInit.getInitError();
    }
    
    std::string csInputFileName (argc > 1 ? argv[1] : DIR_LOC DEF_INPUT);
    std::string csOutputFileName (argc > 2 ? argv[2] : DEF_OUTPUT);
    std::cout << "Will search " << csInputFileName.c_str () << " for all text runs, "
        << "and create a new document with one run, in the font, size, and color of the original, on"
        << " each line of the document, and save to " << csOutputFileName.c_str () << std::endl;

DURING

    // Open the document 
    APDFLDoc document (csInputFileName.c_str (), true);

    // Step 1) Declare the selected text. 
    //         In this sample, we will ask for all of the text on page 1.
    PDPage page = PDDocAcquirePage (document.getPDDoc (), 0);
    ASFixedRect pageSize;
    PDPageGetCropBox (page, &pageSize);
    PDTextSelect selection = PDDocCreateTextSelect (document.getPDDoc (), 0, &pageSize);

    // Step 2) Acquire a list of all selected word in UCS format
    wordList words;
    PDTextSelectEnumTextUCS (selection, enumSelectedTextUCS, &words);

    //Step 3) Release the source resources
    //  (NOTE: Do NOT close the source document. We need to continue to 
    //   exist to give meaning to the PDFonts, referenced in the runs.
    PDPageRelease (page);


    // Step 4) Create an output document, and write each run to one line of that document.
    PDDoc outDoc = PDDocCreate ();
    ASFixedRect newPageSize = { 0, FloatToASFixed (11.0 * 72), FloatToASFixed (8.5 * 72.0), 0 };
    ASFixed indents = FloatToASFixed (72.0);
    page = PDDocCreatePage (outDoc, PDDocGetNumPages (outDoc)-1, newPageSize);
    PDEContent content = PDPageAcquirePDEContent (page, 0);
    ASFixed Baseline = newPageSize.top - indents;

    for (int i = 0; i < words.size (); i++)
    {
        EnumeratedRun run = words[i];

        Baseline -= run.size;

        if (Baseline < indents)
        {
            // If we go off the bottom of the page, 
            //   finish the current page, and start a new page
            PDPageSetPDEContent (page, 0);
            PDPageReleasePDEContent (page, 0);
            PDPageRelease (page);

            Baseline = newPageSize.top - indents - run.size;
            page = PDDocCreatePage (outDoc, PDDocGetNumPages (outDoc)-1, newPageSize);
            content = PDPageAcquirePDEContent (page, 0);
        }

        ASFixedMatrix matrix = { run.size, 0, 0, run.size, indents, Baseline };

        PDEText text = PDETextCreate ();
        CosObj cosFont = PDFontGetCosObj (run.font);
        PDEFont pdeFont = PDEFontCreateFromCosObj (&cosFont);

        // Create the graphic state to display the text
        PDEGraphicState gState;
        PDEDefaultGState (&gState, sizeof (PDEGraphicState));
        PDERelease ((PDEObject)gState.fillColorSpec.space);
        switch (run.color.space)
        {
            case PDDeviceGray:
                gState.fillColorSpec.space = PDEColorSpaceCreateFromName (ASAtomFromString ("DeviceGray"));
                gState.fillColorSpec.value.color[0] = run.color.value[0];
                break;
            case PDDeviceRGB:
                gState.fillColorSpec.space = PDEColorSpaceCreateFromName (ASAtomFromString ("DeviceRGB"));
                gState.fillColorSpec.value.color[0] = run.color.value[0];
                gState.fillColorSpec.value.color[1] = run.color.value[1];
                gState.fillColorSpec.value.color[2] = run.color.value[2];
                break;
            case PDDeviceCMYK:
                gState.fillColorSpec.space = PDEColorSpaceCreateFromName (ASAtomFromString ("DeviceCMYK"));
                gState.fillColorSpec.value.color[0] = run.color.value[0];
                gState.fillColorSpec.value.color[1] = run.color.value[1];
                gState.fillColorSpec.value.color[2] = run.color.value[2];
                gState.fillColorSpec.value.color[3] = run.color.value[3];
                break;
        }

        // Set up the text state
        // (We do not set the type size in the text state object, because we set it in the 
        //  matrix object).
        PDETextState tState;
        memset ((char *)&tState, 0, sizeof (PDETextState));

        PDETextAddASText (text, kPDETextRun, kPDEAfterLast, run.text,
                            pdeFont, &gState, sizeof (PDEGraphicState),
                            &tState, sizeof (PDETextState), &matrix);

        PDEContentAddElem (content, PDEContentGetNumElems(content), (PDEElement)text);

        PDERelease ((PDEObject)text);
        PDERelease ((PDEObject)pdeFont);
        PDERelease ((PDEObject)gState.fillColorSpec.space);
        PDERelease ((PDEObject)gState.strokeColorSpec.space);
    }

    // Finish the last page
    PDPageSetPDEContent (page, 0);
    PDPageReleasePDEContent (page, 0);
    PDPageRelease (page);

    // Save the document 
    ASPathName outPath = APDFLDoc::makePath (csOutputFileName.c_str ());
    PDDocSave (outDoc, PDSaveFull, outPath, NULL, NULL, NULL);
    ASFileSysReleasePath (NULL, outPath);
    PDDocRelease (outDoc);

HANDLER
    errCode = ERRORCODE;
    libInit.displayError(errCode);
END_HANDLER

    return errCode;              
}
