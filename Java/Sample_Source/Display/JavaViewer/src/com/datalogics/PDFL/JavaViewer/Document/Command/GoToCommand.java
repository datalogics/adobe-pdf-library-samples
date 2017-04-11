/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.awt.Rectangle;
import java.awt.geom.Point2D;
import java.awt.geom.Point2D.Float;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;

import com.datalogics.PDFL.Action;
import com.datalogics.PDFL.FileSpecification;
import com.datalogics.PDFL.GoToAction;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.RemoteGoToAction;
import com.datalogics.PDFL.URIAction;
import com.datalogics.PDFL.ViewDestination;
import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter.ViewShot;
import com.datalogics.PDFL.JavaViewer.Presentation.Cache.PageModel;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;

/**
 * GoToCommand - allows to set new document position in the PDFPresenter.
 * 
 * It constructs ViewShot object directly or uses ViewDestination from
 * PDFL.GoToAction for it. ViewShot is an object which used by PDFPresenter for
 * setting new position in the document with specified zoom and/or fit.
 */
public class GoToCommand extends ViewCommand {
    public GoToCommand(Action action) {
        this.action = action;
    }

    public GoToCommand(ViewShot viewShot) {
        this.viewShot = viewShot;
    }

    @Override
    protected void prepare() throws FinalState {
        if (action instanceof GoToAction) {
            ViewDestination destination = ((GoToAction) action).getDestination();
            int pageNum = 0;
            ZoomMode zoomMode = ZoomMode.ZOOM_NONE;
            double zoom = 0;

            Point2D.Float location = new Point2D.Float();
            boolean useHorzLocation = false;
            boolean useVertLocation = false;

            if (destination == null)
                throw new FinalState(State.Failed);

            Rect destRect = destination.getDestRect();
            useHorzLocation = !Double.isInfinite(destRect.getLeft()) && !Double.isNaN(destRect.getLeft());
            useVertLocation = !Double.isInfinite(destRect.getTop()) && !Double.isNaN(destRect.getTop());
            pageNum = destination.getPageNumber();
            zoomMode = ZoomMode.ZOOM_DIRECT;

            final String fitType = destination.getFitType();
            final PageModel pageModel = getPDFPresenter().getPageModel(pageNum);
            if (!fitType.startsWith("Fit")) { // "XYZ" mode
                zoom = destination.getZoom();
                if (Double.isInfinite(zoom) || Double.isNaN(zoom) || zoom == 0)
                    zoomMode = ZoomMode.ZOOM_NONE;
                location = pageModel.relative(new com.datalogics.PDFL.Point(destRect.getLeft(), destRect.getTop()));
            } else {
                boolean hasHorz = (fitType.indexOf('H') != -1);
                boolean hasVert = (fitType.indexOf('V') != -1);
                if (hasHorz)
                    zoomMode = ZoomMode.ZOOM_FIT_WIDTH;
                else if (hasVert)
                    zoomMode = ZoomMode.ZOOM_FIT_HEIGHT;
                else
                    zoomMode = ZoomMode.ZOOM_FIT_PAGE;

                boolean withRect = false;
                if (fitType.indexOf('B') != -1) {
                    withRect = true;
                    destRect = getApplication().getActiveDocument().getPDF().getPage(pageNum).getBBox();
                } else if (fitType.indexOf('R') != -1) {
                    withRect = true;
                    hasHorz = true;
                    hasVert = true;
                }

                if (withRect) {
                    final Rectangle fitRect = pageModel.transform(destRect);
                    final double zoomWidth = fitRect.width > 0 ? getApplication().getView().getPDF().getVisibleViewSize().getWidth() / fitRect.getWidth() : 0;
                    final double zoomHeight = fitRect.height > 0 ? getApplication().getView().getPDF().getVisibleViewSize().getHeight() / fitRect.getHeight() : 0;

                    zoomMode = ZoomMode.ZOOM_DIRECT; // change zoom mode to direct for rectangle fit mode
                    zoom = Math.min(zoomWidth, zoomHeight) * getPDFPresenter().getZoom();
                }
                location = pageModel.relative(new com.datalogics.PDFL.Point(hasVert ? destRect.getLeft() : 0, hasHorz ? destRect.getTop() : 0));
            }

            // parameter validation
            final ViewShot currentShot = getPDFPresenter().getViewShot();

            if (zoomMode == ZoomMode.ZOOM_NONE) {
                zoom = currentShot.getZoom();
                zoomMode = currentShot.getZoomMode();
            }

            // overwrite location coordinate with current values if it's not used
            if (!useHorzLocation) {
                location.x = currentShot.getLocation().x;
            }

            // overwrite location coordinate with current values if it's not used
            if (!useVertLocation) {
                location.y = currentShot.getLocation().y;
            }

            final double _zoom = zoom;
            final ZoomMode _zoomMode = zoomMode;
            final Point2D.Float _location = location;
            final int _pageNum = pageNum;
            viewShot = new ViewShot() {
                public ZoomMode getZoomMode() {
                    return _zoomMode;
                }

                public double getZoom() {
                    return _zoom;
                }

                public int getPageNum() {
                    return _pageNum;
                }

                public Float getLocation() {
                    return _location;
                }
            };
        }

        if (viewShot == null && action instanceof GoToAction)
            throw new FinalState(State.Failed);
    }

    @Override
    protected void doInner() throws FinalState {
        if (viewShot != null) {
            getPDFPresenter().setViewShot(viewShot);
        } else if (action instanceof URIAction || action instanceof RemoteGoToAction) {
            String url = action instanceof URIAction ? ((URIAction) action).getURI() : ((RemoteGoToAction) action).getFileSpecification().getPath();
            openURI(url);
        }
    }

    private void openURI(String url) {
        String os = System.getProperty("os.name").toLowerCase();
        ArrayList<String> commands = new ArrayList<String>();
        if (os.indexOf("win") >= 0) {
            commands.addAll(Arrays.asList("cmd", "/c", "start", url));
        } else {
            final String command = os.indexOf("mac") >= 0 ? "open" : "xdg-open";
            commands.addAll(Arrays.asList(command, url));
        }

        ProcessBuilder builder = new ProcessBuilder(commands);
        try {
            builder.start();
        } catch (IOException e) {
        }
    }

    private final static CommandType type = CommandType.VIEW; // field used through reflection

    private Action action;
    private ViewShot viewShot;
}
