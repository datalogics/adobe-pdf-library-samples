/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

import com.datalogics.PDFL.JavaViewer.Document.Command.ZoomCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * ZoomModeInteractive - special mode for zooming page through the mouse input.
 * 
 * It increases or decreases page zoom depending on state of shift key.
 */
public class ZoomModeInteractive extends BaseInteractive {
    public ZoomModeInteractive() {
        setZoomArray(new double[] { 1.0 });
    }

    public void setZoomArray(double[] zoomArray) {
        this.zoomArray = zoomArray;
    }

    @Override
    public void mousePressed(InputData input) {
        super.mousePressed(input);

        if (isActive() && input.getLeftButton() && hasPageModel() && getPageModel().getBounds().contains(input.getLocation())) {
            getApplication().executeCommand(new ZoomCommand(input.getShiftDown() ? ZoomMode.ZOOM_PREV : ZoomMode.ZOOM_NEXT, zoomArray));
            input.markProcessed();
        }
    }

    private double[] zoomArray;
}
