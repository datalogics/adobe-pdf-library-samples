/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Presentation.Enums.PageViewMode;

/**
 * ViewModeCommand - allows to set document's paging type.
 */
public class ViewModeCommand extends ViewCommand {
    public ViewModeCommand(PageViewMode viewMode) {
        this.viewMode = (viewMode != null) ? viewMode : PageViewMode.PAGE_MODE_SINGLE;
    }

    @Override
    protected void prepare() throws FinalState {
    }

    @Override
    protected void doInner() throws FinalState {
        getPDFPresenter().setViewMode(viewMode);
    }

    private final static CommandType type = CommandType.VIEW; // field used through reflection

    private PageViewMode viewMode;
}
