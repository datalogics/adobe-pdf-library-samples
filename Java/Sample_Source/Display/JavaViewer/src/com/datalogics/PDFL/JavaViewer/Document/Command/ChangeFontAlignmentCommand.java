/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;

/**
 * ChangeFontAlignmentCommand - allows to change alignment for annotations which
 * support this property.
 */
public class ChangeFontAlignmentCommand extends UpdatePropertyCommand {
    public ChangeFontAlignmentCommand(HorizontalAlignment align) {
        this.align = align;
    }

    @Override
    protected void doInner() throws FinalState {
        ((ApplicationController) getApplication()).getActiveProps().setQuadding(align);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;

    private HorizontalAlignment align;
}
