/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;

/**
 * ChangeFontFaceCommand - allows to update font face for annotations which
 * support this property.
 */
public class ChangeFontFaceCommand extends UpdatePropertyCommand {
    public ChangeFontFaceCommand(String fontFace) {
        this.fontFace = fontFace;
    }

    @Override
    protected void doInner() throws FinalState {
        ((ApplicationController) getApplication()).getActiveProps().setFontFace(fontFace);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;

    private String fontFace;
}
