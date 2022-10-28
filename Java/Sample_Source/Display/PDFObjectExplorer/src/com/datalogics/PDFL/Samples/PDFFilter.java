/*
 * Copyright (c) 2009-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 * ============================ PDFFilter ===================================
 * This filter is used with the file chooser to limit displayed files to those
 * with a .pdf extension (and directories).  It is adapted from public code on
 * the Java website at:
 * http://java.sun.com/docs/books/tutorial/uiswing/examples/components/FileChooserDemo2Project/src/components/ImageFilter.java
 */

package com.datalogics.pdfl.samples.Display.PDFObjectExplorer;

import java.io.File;
import javax.swing.filechooser.FileFilter;

/**
 *
 * @author Datalogics
 */
public class PDFFilter extends FileFilter{

    // Description of this filter
    public String getDescription() {
        return "PDF Files";
    }

    // Accept all directories and all PDF files
    public boolean accept(File f) {
        if (f.isDirectory()) {
            return true;
        }

        String extension = getExtension(f);

        // If the file has an extension, check it
        if (extension != null) {
            if (extension.equalsIgnoreCase("pdf")) {
                return true;
            } else {
                return false;
            }
        }

        // Also display files that have no extension
        return true;
    }

    // Get the extension of a file
    private String getExtension(File f) {
        String ext = null;
        String s = f.getName();
        int i = s.lastIndexOf('.');

        if (i > 0 && i < s.length() - 1) {
            ext = s.substring(i+1).toLowerCase();
        }
        return ext;
    }

}
