/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Point;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF.ToolTip;

/**
 * HoverAnnotationInput - decides what to do when mouse is over the annotation.
 * 
 * This class shows the tooltip when mouse hovering an annotation.
 */
public class HoverAnnotationInput extends BaseAnnotationInput {
    @Override
    public void activate(boolean active) {
        if (active)
            onActivate();
        else
            onDeactivate();
    }

    @Override
    protected void onActivate() {
        super.onActivate();
        final Point location = getAnnotationEditor().getPageModel().transform(new com.datalogics.PDFL.Point(getAnnotationEditor().getProperties().getBoundingRect().getURx() + 10, getAnnotationEditor().getProperties().getBoundingRect().getLLy() + 10));
        final String title = getAnnotationEditor().getProperties().getTitle();
        final String contents = getAnnotationEditor().getProperties().getContents();

        final String message = title + "\n\n\n" + contents;

        if (title.length() != 0 || contents.length() != 0)
            toolTip = getAnnotationEditor().getPdfView().showToolTip(message, location);
    }

    @Override
    protected void onDeactivate() {
        if (toolTip != null)
            toolTip.cancel();
        super.onDeactivate();
    }

    private ToolTip toolTip;
}
