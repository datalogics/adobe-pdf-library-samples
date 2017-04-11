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
 * ChangeFontSizeCommand - allows to set font size for annotations which support
 * this property.
 */
public class ChangeFontSizeCommand extends UpdatePropertyCommand {
    public ChangeFontSizeCommand(int fontSize) {
        this.fontSize = fontSize;
    }

    @Override
    protected void doInner() throws FinalState {
        ((ApplicationController) getApplication()).getActiveProps().setFontSize(fontSize);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;

    private int fontSize;
}
