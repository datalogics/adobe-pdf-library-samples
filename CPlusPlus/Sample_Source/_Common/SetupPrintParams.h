/*
// Copyright 2016, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// Setup default print parameters for samples.
*/

#ifndef _SETUPPRINTPARAMS_H_
#define _SETUPPRINTPARAMS_H_

#include <string.h>
#include "APDFLDoc.h"
#include "PDFLPrint.h"

// Setup the PD print parameters
void SetupPDPrintParams(PDPrintParams psParams);

// Cleanup the PD print parameters
void DisposePDPrintParams(PDPrintParams psParams);

// Setup the PDFL user print parameters
void SetupPDFLPrintUserParams(PDFLPrintUserParams userParams);

// Cleanup the PDFL user print parameters
void DisposePDFLPrintUserParams(PDFLPrintUserParams userParams);

#endif /* _SETUPPRINTPARAMS_H_ */
