/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

import java.awt.Dimension;
import java.awt.Point;

import com.datalogics.PDFL.JavaViewer.Document.Command.ScrollCommand;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * ScrollModeInteractive - allows to scroll the document.
 * 
 * It processes mouse events to decide where to scroll. If mouse has been moved
 * this class generates the ScrollCommand, if mouse has been pressed/released it
 * changes the cursor view.
 */
public class ScrollModeInteractive extends BaseInteractive {
    public ScrollModeInteractive() {
        this.startPoint = new Point();
    }

    @Override
    public void mousePressed(InputData input) {
        if (input.getLeftButton() && hasPageModel() && getPageModel().getBounds().contains(input.getLocation())) {
            startPoint = input.getLocation();

            getInteractiveContext().changeCursor(this, PDF.Cursor.HAND);
            pageCaptured = true;
            input.markProcessed();
        }
    }

    @Override
    public void mouseMoved(InputData input) {
        if (pageCaptured) {
            final Point currentPoint = input.getLocation();
            final Dimension pageShift = new Dimension(startPoint.x - currentPoint.x, startPoint.y - currentPoint.y);

            if (Math.max(Math.abs(pageShift.width), Math.abs(pageShift.height)) >= MIN_MOUSE_MOVE) {
                getApplication().executeCommand(new ScrollCommand(new Dimension(pageShift)));

                // correct current point with regard to view origin move
                currentPoint.translate(pageShift.width, pageShift.height);
                startPoint = currentPoint;
            }
            input.markProcessed();
        }
    }

    @Override
    public void mouseReleased(InputData input) {
        if (pageCaptured) {
            getInteractiveContext().revertCursor(this);
            pageCaptured = false;

            input.markProcessed();
        }
    }

    private final static int MIN_MOUSE_MOVE = 5;

    private boolean pageCaptured;
    private Point startPoint;
}
