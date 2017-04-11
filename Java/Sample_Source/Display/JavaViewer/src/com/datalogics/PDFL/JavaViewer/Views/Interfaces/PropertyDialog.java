/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

public class PropertyDialog {
    public void setProperties(String title, String content) {
        this.title = title;
        this.content = content;
    }

    public String title;
    public String content;
}
