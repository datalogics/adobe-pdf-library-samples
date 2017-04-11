/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;

/**
 * ChangeArrowHeadCommand - allows to modify arrow style for annotations which
 * support this property.
 */
public class ChangeArrowHeadCommand extends UpdatePropertyCommand {
    public ChangeArrowHeadCommand(LineEndingStyle style, boolean startEnd) {
        this.style = style;
        this.startEnd = startEnd;
    }

    @Override
    protected void doInner() throws FinalState {
        final AnnotationProperties props = ((ApplicationController) getApplication()).getActiveProps();
        if (startEnd)
            props.setStartEndingStyle(style);
        else
            props.setEndEndingStyle(style);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;

    private LineEndingStyle style;
    private boolean startEnd;
}
