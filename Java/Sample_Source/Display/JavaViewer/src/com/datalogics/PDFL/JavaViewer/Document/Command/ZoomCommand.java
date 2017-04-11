/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;

/**
 * ZoomCommand - allows to set new zoom value depending on input parameters.
 * 
 * This class uses PDFPresenter object to set calculated zoom value.
 */
public class ZoomCommand extends ViewCommand {
    public ZoomCommand(ZoomMode zoomMode, double[] zoomArray, double zoom) {
        this.zoomMode = zoomMode;
        this.zoomArray = zoomArray;
        this.zoom = zoom;
    }

    public ZoomCommand(ZoomMode zoomMode, double[] zoomArray) {
        this(zoomMode, zoomArray, 0);
    }

    public ZoomCommand(ZoomMode zoomMode, double zoom) {
        this(zoomMode, null, zoom);
    }

    public ZoomCommand(ZoomMode zoomMode) {
        this(zoomMode, null, 0);
    }

    @Override
    protected void prepare() throws FinalState {
    }

    @Override
    protected void doInner() throws FinalState {
        if (getApplication().getActiveDocument() == null || zoomMode == null || (zoomMode.isDirectMode() && (Double.isInfinite(zoom) || Double.isNaN(zoom))))
            throw new FinalState(State.Failed);

        double newZoom = zoom;
        ZoomMode newMode = zoomMode;

        switch (zoomMode) {
        case ZOOM_NEXT:
            newZoom = ZoomMode.getNextZoom(getPDFPresenter().getZoom(), zoomArray);
            newMode = ZoomMode.ZOOM_DIRECT;
            break;
        case ZOOM_PREV:
            newZoom = ZoomMode.getPrevZoom(getPDFPresenter().getZoom(), zoomArray);
            newMode = ZoomMode.ZOOM_DIRECT;
            break;
        }
        getPDFPresenter().setZoomMode(newMode);
        if (newMode.isDirectMode())
            getPDFPresenter().setZoom(newZoom);
    }

    private final static CommandType type = CommandType.VIEW;

    private ZoomMode zoomMode;
    private double[] zoomArray;
    private double zoom;
}
