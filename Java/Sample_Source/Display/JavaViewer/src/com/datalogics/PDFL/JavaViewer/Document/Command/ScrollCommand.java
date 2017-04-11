/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.awt.Dimension;
import java.awt.Point;

/**
 * ScrollCommand - calculates new scroll position depending on input parameter.
 * 
 * This command uses PDFPresenter object to set new scroll position for the
 * document.
 */
public class ScrollCommand extends ViewCommand {
    public ScrollCommand(Dimension pageShift) {
        this.pageShift = pageShift;
    }

    @Override
    protected void prepare() throws FinalState {
    }

    @Override
    protected void doInner() throws FinalState {

        Point viewOrigin = getApplication().getView().getPDF().getViewOrigin();// getPdfView().getViewOrigin();
        viewOrigin.translate(pageShift.width, pageShift.height);
        getPDFPresenter().goToPositionAbsolute(getPDFPresenter().getActivePage(), viewOrigin);
    }

    private final static CommandType type = CommandType.VIEW; // field used through reflection
    private Dimension pageShift;
}
