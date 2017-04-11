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
 * ChangeDashPatternCommand - allows to modify dash pattern for annotations
 * which support this property.
 */
public class ChangeDashPatternCommand extends UpdatePropertyCommand {
    public ChangeDashPatternCommand(float[] pattern) {
        this.pattern = pattern;
    }

    @Override
    protected void doInner() throws FinalState {
        ((ApplicationController) getApplication()).getActiveProps().setDashPattern(pattern);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;

    private float[] pattern;
}
