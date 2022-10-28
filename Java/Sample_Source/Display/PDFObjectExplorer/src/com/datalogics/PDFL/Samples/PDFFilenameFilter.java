/*
 * Copyright (c) 2009-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 * ============================ PDFFilenameFilter ===================================
 * This filter is used with the file dialog to limit displayed files to those
 * with a .pdf extension (and directories).
 */
package com.datalogics.pdfl.samples.Display.PDFObjectExplorer;

import java.io.File;
import java.io.FilenameFilter;

/**
 *
 */
public class PDFFilenameFilter implements FilenameFilter {

    public boolean accept(File dir, String name) {
        return name.toLowerCase().endsWith(".pdf");
    }
}
