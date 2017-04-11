/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

/**
 * EditorStates - enum which describe annotation state depending on user
 * activity.
 */
public enum EditorStates {
    NONE, IDLE, HOVER, SELECT, EDIT, CUSTOM_EDIT, CREATE;

    public boolean isSelected() {
        return this.equals(SELECT) || this.equals(CUSTOM_EDIT) || this.equals(EDIT) || this.equals(CREATE);

    }
}
