/*
 *
 * A sample which demonstrates the use of the DLE API to create a new
 * PDF file and add two pages containing a series of Path and Text elements
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.CancelProc;

/**
 * Cancel proc for demonstration purposes.
 */
public class AECancelProc extends CancelProc {

    @Override
    public boolean Call() {
        return false;
    }
}
