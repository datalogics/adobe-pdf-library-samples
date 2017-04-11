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
 * ChangeLineWidthCommand - allows to set line width for annotations which
 * support this property.
 */
public class ChangeLineWidthCommand extends UpdatePropertyCommand {
    public ChangeLineWidthCommand(float lineWidth) {
        this.lineWidth = lineWidth;
    }

    @Override
    protected void doInner() throws FinalState {
        ((ApplicationController) getApplication()).getActiveProps().setLineWidth(lineWidth);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;

    private float lineWidth;
}
