/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Enums;

/**
 * DialogMenuItem - represents menu items from presentation.
 */
public enum DialogMenuItem {
    COMPLETE("Complete"),
    CANCEL("Cancel");

    private DialogMenuItem(String name) {
        this.name = name;
    }

    public String getName() {
        return name;
    }

    private String name;
}
