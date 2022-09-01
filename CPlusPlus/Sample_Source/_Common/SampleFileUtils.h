/*
// Copyright 2015, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// This is a Windows specific sample class...
*/

#ifndef _SAMPLE_FILE_UTILS_H_
#define _SAMPLE_FILE_UTILS_H_

/* Helper functions that work with Windows to acquire information */
/* that everyone needs to work with APDFL... */

/* Pops UI for selection of a PDF and then opens, returns a PDDoc. */
PDDoc SelectPDFDocument();

/* Pops UI for selection of a PDF and fills in a path to it. */
void SelectPDFPath(std::wstring &pdfFilePath /* OUT */);

/* Pops UI for creation of a Save As path (for a PDF). */
void GetSaveAsFilePath(std::wstring &pdfFilePath /* OUT */);

/* Pops UI for selection of an ICC profile and fills in a path to it. */
void SelectICCProfile(std::wstring &iccFilePath /* OUT */);

#endif /* _SAMPLE_FILE_UTILS_H_ */
