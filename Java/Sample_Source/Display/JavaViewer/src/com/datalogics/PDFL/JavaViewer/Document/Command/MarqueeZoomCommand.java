/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.awt.Dimension;
import java.awt.Rectangle;
import java.awt.geom.Point2D;

import com.datalogics.PDFL.JavaViewer.Presentation.Cache.PageModel;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;

/**
 * MarqueeZoomCommand - allows to zoom specified part of the page.
 * 
 * This command does not support undo/redo functionality.
 */
public class MarqueeZoomCommand extends ViewCommand {
    public MarqueeZoomCommand(Rectangle marqueeRect, PageModel pageModel) {
        this.marqueeRect = marqueeRect;
        this.pageModel = pageModel;
    }

    @Override
    protected void prepare() throws FinalState {
        if (marqueeRect.isEmpty())
            throw new FinalState(State.Failed);
    }

    @Override
    protected void doInner() throws FinalState {
        final Dimension viewSize = getApplication().getView().getPDF().getVisibleViewSize();
        final double marqueeRatio = Math.min(viewSize.getWidth() / marqueeRect.getWidth(), viewSize.getHeight() / marqueeRect.getHeight());

        // calculate page location before zoom change, as page model will be changed too
        Point2D.Float relLocation = pageModel.relative(marqueeRect.getLocation());

        if (getApplication().executeCommand(new ZoomCommand(ZoomMode.ZOOM_DIRECT, getPDFPresenter().getZoom() * marqueeRatio)).getState() != State.Succeeded)
            throw new FinalState(State.Failed);

        if (getApplication().executeCommand(new NavigateCommand(getPDFPresenter().getActivePage(), relLocation)).getState() != State.Succeeded)
            throw new FinalState(State.Failed);

        getApplication().getView().getPDF().repaintView(); // clean up view from artifacts left by this command's draw method
    }

    private final static CommandType type = CommandType.VIEW; // field used through reflection

    private Rectangle marqueeRect;
    private PageModel pageModel;
}
