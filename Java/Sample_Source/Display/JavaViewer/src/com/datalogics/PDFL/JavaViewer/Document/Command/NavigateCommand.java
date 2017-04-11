/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.awt.geom.Point2D;

import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.NavigateMode;

/**
 * NavigateCommand - allows to navigate through the document using PDFPresenter object.
 * 
 * This class sets the new page number depend on NavigatMode or new location in
 * the document.
 */
public class NavigateCommand extends ViewCommand {
    public NavigateCommand(NavigateMode navigateMode, int pageNum) {
        this(pageNum, PDFPresenter.LEFT_TOP_ALIGN);
        this.navigateMode = navigateMode;
    }

    public NavigateCommand(int pageNum, Point2D.Float relPageLocation) {
        this.navigateMode = NavigateMode.NAVIGATE_DIRECT;
        this.pageNum = pageNum;
        this.relPageLocation = relPageLocation;
    }

    @Override
    protected void prepare() throws FinalState {
    }

    @Override
    protected void doInner() throws FinalState {
        if (getApplication().getActiveDocument() == null)
            throw new FinalState(State.Failed);

        final int pageCount = getPDFPresenter().getPageCount();
        final int activePage = getPDFPresenter().getActivePage();
        int pageToGo = pageNum;

        switch (navigateMode) {
        case NAVIGATE_FIRST:
            pageToGo = 0;
            break;
        case NAVIGATE_PREV:
            pageToGo = activePage - 1;
            break;
        case NAVIGATE_NEXT:
            pageToGo = activePage + 1;
            break;
        case NAVIGATE_LAST:
            pageToGo = pageCount - 1;
            break;
        }
        getPDFPresenter().goToPosition(pageToGo, relPageLocation);
    }

    private final static CommandType type = CommandType.VIEW; // field used through reflection

    private NavigateMode navigateMode;
    private int pageNum;

    private Point2D.Float relPageLocation;
}
