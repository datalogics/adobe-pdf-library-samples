// Copyright (c) 2015-2016, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
//===============================================================================
// Sample: SetupPrintParams -These functions are intended to assist with
// operations common to most print samples.
//===============================================================================

#include "SetupPrintParams.h"

/*
Set up PDPrintParams struct.
Set the same independent of platform or destination for printing.
Contains options for indicating how a document should be printed or generated.
*/
void SetupPDPrintParams(PDPrintParams psParams) {
    /* Set up the PDPrintParamsRec */
    memset(psParams, 0, sizeof(PDPrintParamsRec));
    psParams->size = sizeof(PDPrintParamsRec);

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Attributes present in both PDPrintParams, and PDFLPrintUserParams.
    // When these values conflict, the values specified in PDFLPrintUserParams shall be used.
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    psParams->shrinkToFit = true; // If the page is larger than the media, it will be evenly scaled
                                  // down to fit the media (overrides scale parameter)
    psParams->psLevel = 3L; // PS Level - 1, 2, or 3
                            // Level of PostScript created. In practice, this should match the level
                            // of the printer. If the printer does not support the level specified,
                            // this will be replaced with the highest level known to the printer.
    psParams->binaryOK = true; // Permits binary operators in PostScript output. If printing to a printer,
                               // the printers ability to accept binary may override this setting
                               // ("Output Protocol" device settings.  Turning true to false, only).

    psParams->reverse = false; // Reverse the order of page output
                               // (NOTE: This does NOT add a blank backing page, in the event of
                               // duplex is true, and number of pages is odd.)
    psParams->doOPP = false; // When true, CMYK colors will “mix” in the manner of the OP blending model.
    psParams->transparencyQuality =
        100; // Echoed in PDFLPrintUserParams as the transQuality attribute.
               // Used to establish how much time/resources should be applied to flattening
               // transparency while printing the page. At the lowest values, this will
               // basically render all transparent areas as bitmaps.
               // At the highest setting, it will attempt to render using vector operators
               // as much as possible.
    psParams->farEastFontOpt =
        false; // Used to describe how large fonts should he handled, either downloaded, or not.
    psParams->rotate = false; // When true, enables page auto rotation to align the long edges of the paper.
    psParams->ocContext = NULL; // Optional-content context to use for visibility state information, or NULL
                                // to use the document's current states in the default context.

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Attributes effective in all print modes except Print Via DC.
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    psParams->expandToFit = false; // Small pages are scaled to fit the printer page size (overrides scale parameter)
    psParams->rotateAndCenter =
        false; // When true, indicate that the long edges of the page, and the media,
               // should be aligned, and that the page should be centered in the media.
               // (this works in conjunction with shrinkToFit and expandToFit)
    psParams->centerCropBox =
        true; // When true, position the page such that the center of the page is at the
              // center of the media. When false, the upper left corner of the page will
              // align with the upper left corner of the media.
              // (NOTE: This is only effective when expandToFit and/or shrinkToFit is false.
    psParams->duplex =
        kPDDuplexOff; // Simplex or duplex printed pages (single or double sided)
                      // This is also specified in the DevMode, for Print To PostScript or Print To
                      // GDI. In case of conflict, the DevMode value will be used.

    psParams->whichMarks = 0; // Page mark indication. It is a bit-wise OR of the PDPageMarkFlags values.
    psParams->markStyle = 0; // Specify the style to use for page marks. The PDPrintMarkStyles enum
                             // contains possible styles.
    psParams->customMarksFileName = NULL; // If markStyle == -1, this should be a valid file name
                                          // pointing to a valid .mrk file for custom printer marks.
    psParams->scale = 100.0; // Document-wide scale factor

    psParams->hostBasedCM = false; // Do hostBased color management using the destination profile
                                   // (see destProfile parameter).
                                   // The default is false, which means doing CSA generation for
                                   // profiles instead of converting all colors on the host.

    psParams->doProofing = false; // When true, print using proofing settings.
                                  // (see proofProfile, inkBlack, paperWhite parameters)

    psParams->mirrorprint = kPDPrintFlipNone; // Used to mirror page renderings, horizontally, vertically, or both.
                                              // Mirroring is done in the PostScript output stream.
                                              // One of the following constants:
                                              //    kPDPrintFlipNone = 0x01
                                              //    kPDPrintFlipX = 0x02
                                              //    kPDPrintFlipY = 0x04
                                              //    kPDPrintFlipXY = 0x08
    psParams->lineWidth = 0; // Width of the line to be used in printer marks.
    psParams->disableFlattening =
        false; // When true, will cause transparency flattening to be bypassed when printing.
    psParams->doNotDownloadFauxFonts = false; // When true, disables the downloading of fonts which
                                              // are created to replace fonts not directly usable.
    psParams->grayToK = false; // When true, will cause gray colors to be printed as CMYK colors,
                               // using only the black component.

    psParams->maxFlatSeconds = 0; // When nonzero, sets a limit to the amount of time APDFL will
                                  // spend attempting to flatten a given page to vector art. If the
                                  // time is exceeded, the page will be flattened to a bitmap image.

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Attributes effective in both PostScript to a File, and PostScript to a Printer.
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    psParams->setPageSize = false; // When true, causes a PostScript setcachedevice command to be
                                   // issued, setting the page size.
    psParams->outputType = PDOutput_PS; // Print PostScript or EPS with or without a preview

    psParams->incBaseFonts = kIncludeOncePerDoc; // Embed the base fonts
    psParams->incEmbeddedFonts =
        kIncludeOncePerDoc; // Embed fonts that are embedded in the PDF file.
                            // (NOTE: To include an embedded font, need both incEmbeddedFonts
                            // and the include attribute for that type of font, to specify inclusion.)
    psParams->incType1Fonts = kIncludeOncePerDoc; // Embed Type 1 fonts
    psParams->incType3Fonts = kIncludeOnEveryPage; // Embed Type 3 fonts (this must be set to kIncludeOnEveryPage)
    psParams->incTrueTypeFonts = kIncludeOncePerDoc; // Embed TrueType fonts
    psParams->incCIDFonts = kIncludeOncePerDoc;      // Embed CID fonts
    psParams->incProcsets = kIncludeOncePerDoc;      // Embed Procsets in the file
    psParams->incOtherResources = kIncludeOncePerDoc; // Embed all other types of resources in the file

    // emitColorSeps, emitInRIPSeps and negative, can be used (Only when using PostScript to a
    // Printer) to print separation colors, rather than composite color pages.
    psParams->emitColorSeps = false; // Emit images for Level-1 separations.
    psParams->emitInRipSeps = false; // When true, will cause pages to be separated in the RIP engine,
                                     // rather than on the host. This is effective only if emitColorSeps is true.
    psParams->negative = false; // When true, invert the plate.

    psParams->emitRawData = false; // When true, will suppress the use of all filters for images.
                                   // (Effectively, turn off compression of images).

    psParams->TTasT42 = true; // If including TrueType fonts, convert to Adobe Type 42 fonts instead of Type 1 fonts.

    psParams->emitPSXObjects =
        false; // When true, will cause PDF Xobjects to be emitted to PostScript as PostScript
               // XObjects. When false, they will be copied to each point of reference.
    psParams->useFontAliasNames =
        false; // When true, uses the name of the font as it was supplied by the system, or subset,
               // while downloading the font. When false, the base font name is used.

    psParams->emitPageClip = true; // When true, will emit a clip region around the pages clip path.
                                   // This is useful if the page actually contains content outside of it’s clip path.
    psParams->emitTransfer = true; // When true, will emit transfer functions to the PostScript
                                   // document, rather than resolving their usage on the host.
    psParams->emitBG = true; // When true,  will emit Black Generation values to the PostScript
                             // file, rather than resolving their usage on the host.
    psParams->emitUCR = true; // When true, will emit Under Color Removal values to the PostScript
                              // file, rather than resolving their usage locally.

    psParams->suppressCJKSubstitution =
        false; // If allowed, will control the use of printer based substitutes for CJK Fonts.
    psParams->brokenCRDs = false; // When true, disables the emission of profiled colors to
                                  // PostScript. In essence, this will force Host color conversion.

    psParams->bitmapResolution = 0; // Set the resolution, in DPI, of emitted bitmaps
                                    // when these are done as actual bitmaps.
    psParams->gradientResolution =
        0; // Set the resolution, in DPI, of gradients interior to the object (not edges).
           // It can generally be lower than the bitmapResolution.

    psParams->flattenInfo = NULL; // When present, is used to control the flattening of translucent pages.
                                  // If not present, a set of default values is used in its place.

    psParams->emitFlatness = false; // When true, causes the flatness setting in Graphics State to be honored.
    psParams->trapType = 0; // Controls whether trapping is done on the host or on the printer.
    psParams->suppressSnapToDevice =
        false; // When true, will disable the “Snap To Device” rendering option.
               // Snap To Device causes all elements to be pixel aligned at the printer resolution.
    psParams->suppressElement = 0; // Controls suppression of XMP data, written to the PostScript
                                   // stream. Data may be controlled by type.

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Attributes effective only in PostScript to a File.  (These purely affect PostScript construction.)
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Set psParams.ranges = NULL to print entire document or specify ranges as shown below
    psParams->numRanges = 1; // Number of print ranges to specify  (ignored if ranges == NULL)
    psParams->ranges = (PDPageRange *)malloc(psParams->numRanges * sizeof(PDPageRange));

    // Repeat the following for each range up to numRanges (ie psParams.ranges[1], psParams.ranges[2], etc)
    psParams->ranges[0].startPage = 0;         // Specify starting page
    psParams->ranges[0].endPage = -1;          // Specify ending page or -1 for end of document
    psParams->ranges[0].pageSpec = PDAllPages; // Specify PDAllPages, PDEvenPagesOnly, or PDOddPagesOnly

    psParams->printWhat =
        PDPrintWhat_DOCUMENT; // Print the document, the document and comments, or document form
                              // fields only. (NOTE: Effective only when printing with PDFLPrintPDF)
    psParams->printWhatAnnot = PDPrintWhatAnnot_NoExtras; // Annotation flags which modify PDPrintWhat.
                                                          // (NOTE: Effective only when printing with PDFLPrintPDF)

    psParams->emitPS = true;       // Emit a PostScript file
    psParams->emitShowpage = true; // Controls emission of the showpage command within the page.
                                   // If there are plans for combining pages at a later date,
                                   // may wish to suppress the show page command.
    psParams->emitPageRotation = false; // When true, rotates the page according to PDRotate, before printing.
                                        // (NOTE: This is ONLY used when creating EPSF output)
    psParams->destCSAtom = 0; // Used only when printing to a file, and describes the desired
                              // color space of the printer (One of DeviceGray, DeviceRGB, or DeviceCMYK, only).

    psParams->suppressOPPWhenNoSpots =
        false; // When true, will suppress OP processing of pages which contain no spot colors.
               // This attribute may not be effective in all cases
               // (That is, OP Processing may be done on some pages which do not contain spot
               // colors, regardless of the setting of this flag).

    psParams->optimizeForSpeed =
        true; // Emit PostScript such that all fonts are emitted once at the beginning
              // of the document. This results in faster transmission times and smaller
              // PostScript documents but requires more PostScript printer virtual memory.
              // If it is set to true, font downloads are forced from
              // kIncludeOnEveryPage to kIncludeOncePerDoc.
              // Therefore, false means PostScript code must be page-independent.

    psParams->saveVM = true; // When true, attempts to minimize the use of the PostScript virtual memory.

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Attributes which are no longer effective.
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    psParams->incProcsets = kIncludeOncePerDoc;
    psParams->fontPerDocVM = 0;
    psParams->emitTTFontsFirst = false;
    psParams->emitDSC = false;
    psParams->setupProcsets = false;
    psParams->useSubFileDecode = false;
    psParams->emitExternalStreamRef = false;
    psParams->emitHalftones = false;
    psParams->emitSeparableImagesOnly = false;
    psParams->emitDeviceExtGState = false;
    psParams->hostBased = false;
    psParams->hostBasedOutputCS = false;
    psParams->useMaxVM = false;
    psParams->applyOCGPrintOverrides = false;
    psParams->useFullResolutionJP2KData = false;
    psParams->westernMarksStyle = false;
    psParams->useExecForm = false;
    psParams->numCollatedCopies = false;
    psParams->TTasCIDT2 = false;
    psParams->macQDPrinter = false;
}

void DisposePDPrintParams(PDPrintParams psParams) {
    if (psParams->ranges) {
        psParams->numRanges = 0;
        free(psParams->ranges);
        psParams->ranges = NULL;
    }
}

/*
 * The PDFLPrintUserParamsRec is used to specify the types of parameters that
 * a user would usually specify in the print dialogue box. It is different for
 * each platform due to the different printing functionality available. The following
 * code uses conditional compilation so that the samples work on all platforms
 */
void SetupPDFLPrintUserParams(PDFLPrintUserParams userParams) {
    /* Initialize the struct */
    memset(userParams, 0, sizeof(PDFLPrintUserParamsRec));

    userParams->size = sizeof(PDFLPrintUserParamsRec);

    // Attributes emitToFile and emitToPrinter must not both be true.
    // If they are both true, behavior is as if emitToPrinter is false
    userParams->emitToPrinter =
        true; // If emitToPrinter is true, then the printer identification information should be present.
              // If it is not, the system default printer will be used.
              // If there is no system default printer, an exception will be raised.

#ifdef WIN_ENV

    userParams->deviceName = NULL; // Printer Identification information consists of device, port, and driver names.
    userParams->deviceNameW =
        NULL; // These may be supplied as either ASCII, or wide text strings.
              // If both the ASCII and wide text names are supplied, the wide text name will be used.
    userParams->portName = NULL;
    userParams->portNameW = NULL;

    userParams->driverName = NULL;
    userParams->driverNameW = NULL;

    userParams->inFileName = NULL; // Used only in working with an actual printer. They are supplied to the
    userParams->inFileNameW = NULL; // print driver, for use in a separator page (At the drivers
                                    // discretion). When both are supplied, wide form is used.

    userParams->outFileName = NULL; // Used only when printing to a printer. They indicate that the final driver
    userParams->outFileNameW =
        NULL; // file should be saved in the named file, rather than passed to the printer
              // Effectively, they are equivalent to the “print to a file” toggle in most
              // printer controls.
              // When both are supplied, the wide form will be used.

    userParams->pDevMode = NULL; // Optional attribute. Reference to a Windows DevMode structure.
    userParams->pDevModeW =
        NULL; // The use of the wide form is preferable. If both are present, the wide form will be used.
              // When present, it will be used to establish values for printer control information.
              // When it is NOT present, and  printing to a printer, APDFL will obtain the default
              // DevMode for the selected printer to use in its place.
              // When the user supplies the DevMode, it’s values for nCopies, Duplex, and Collate
              // will be used, rather than the values specified in the control structures.
              // When it is obtained from the default for the printer, the values specified in the
              // control structures will be preferred.

    userParams->farEastFontOpt =
        false; // Controls if very large fonts are to be downloaded or not. This is effective only for PostScript.

    userParams->startResult =
        0; // Should be set to the jobId returned by StartDoc, if the application does the StartDoc.
    userParams->pDC = NULL; // Will be filled in by APDFL to the DeviceContext for the currently selected printer.
                            // It will contain the value supplied in hDC, if such a value is supplied.

    userParams->hDC = NULL; // Supplied by the user. This should be a Device Context for the same
                            // print device as is named in deviceName. If it is not, then deviceName
                            // will be replaced with the name of the device this context refers to.

    userParams->forceGDIPrint = false; // When true, will cause Print To GDI to be used, even if the
                                       // printer selected is capable of supporting PostScript.

#endif

    userParams->transQuality = 5; // Controls how the flattener will remove transparency from a page.

    userParams->paperWidth =
        kPDPrintUseMediaBox; // Used to select paper.  The value kPDPrintUseMediaBox will cause the
                             // page size to be used, rather than a fixed size.
    userParams->paperHeight =
        kPDPrintUseMediaBox; // Used to select paper.  The value kPDPrintUseMediaBox will cause the
                             // page size to be used, rather than a fixed size.

    userParams->dontEmitList = NULL; // Used only when creating PostScript and indicates fonts that are not to
    userParams->dontEmitListLen = 0; // be downloaded to the printer, or included in the PostScript file.

    userParams->cancelProc =
        NULL; // Establish a procedure, which is called periodically, which is able to cancel a print operation
    userParams->clientData = NULL; // in progress.

    userParams->PPDFeatures =
        NULL; // Used only in printing to PostScript. Normally, the PPD File will be located via the system
    userParams->ppdFileName =
        NULL; // for PostScript to a Printer, and the features available will be only those features in the system
              // installed PPD File. This mechanism may be used to attach feature information for PostScript to a File.

    userParams->progressProc =
        NULL; // Used to identify a procedure that should be called periodically during printing, to
              // permit a progress reporting function.
    userParams->userCallbacks = NULL; // Establishes callbacks to be used in this print run.

    userParams->emitToFile = false; // If emitToFile is true, then the attribute printStm
                                    // must contain a reference to a valid ASStm object,
                                    // which will contain the PostScript file.
    userParams->printStm = NULL;

#ifdef UNIX_ENV
    userParams->command = const_cast<char *>("lp"); // Optional command line arguments (used only if emitToPrinter is true)
#endif

#ifndef UNIX_ENV
    userParams->nCopies = 1;   // Number of copies to print
                               // (applies to both a PostScript printer and a file)
    userParams->startPage = 0; // Page to start printing with, using zero-based numbering
                               // When we are printing PostScript to a File, this is ignored
                               // and the range defined in the PDPrintParamsRec is used instead.
    userParams->endPage = 1;   // Page on which to finish printing (applies to a printer)
                               // When we are printing PostScript to a File, this is ignored
                               // and the range defined in the PDPrintParamsRec is used instead.

    userParams->psLevel = 3; // Controls the contents of a PostScript file to be limited to
                             // the specified level. If the printer itself specifies a lower level,
                             // then that level will be used.
    userParams->shrinkToFit = true; // When true,  will cause the page size to be decreased to match the media size.

    userParams->printAnnots = false; // When true, will cause annotations, only those allowed to be printed, to print.
    userParams->binaryOK = false; // When true, will permit binary content in generated PostScript.
    userParams->emitHalftones = false; // When true, halftone data will be included in a PostScript rendering.
    userParams->reverse = false; // When true,  will cause the pages to be printed from last to first.

    userParams->doOPP = false; // Echoes the PDPrintParamsRec attribute of the same name. When true,
                               // we will “blend” CMYK colors using the OP transparency model.
#endif
}

void DisposePDFLPrintUserParams(PDFLPrintUserParams userParams) {
#ifdef WIN_ENV
    if (userParams->pDevMode) {
        free(userParams->pDevMode);
    }
    if (userParams->pDevModeW) {
        free(userParams->pDevModeW);
    }

    if (userParams->deviceName) {
        delete[] userParams->deviceName;
    }
    if (userParams->deviceNameW) {
        delete[] userParams->deviceNameW;
    }

    if (userParams->portName) {
        delete[] userParams->portName;
    }
    if (userParams->portNameW) {
        delete[] userParams->portNameW;
    }

    if (userParams->driverName) {
        delete[] userParams->driverName;
    }
    if (userParams->driverNameW) {
        delete[] userParams->driverNameW;
    }

    if (userParams->inFileName) {
        delete[] userParams->inFileName;
    }
    if (userParams->inFileNameW) {
        delete[] userParams->inFileNameW;
    }

    if (userParams->outFileName) {
        delete[] userParams->outFileName;
    }
    if (userParams->outFileNameW) {
        delete[] userParams->outFileNameW;
    }
#endif
}
