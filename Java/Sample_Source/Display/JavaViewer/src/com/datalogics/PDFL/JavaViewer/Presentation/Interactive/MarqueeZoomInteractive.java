/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

import java.awt.Color;
import java.awt.Graphics;
import java.awt.Point;
import java.awt.Rectangle;

import com.datalogics.PDFL.JavaViewer.Document.Command.MarqueeZoomCommand;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * MarqueeZoomInteractive - allows user to specify page area for zooming.
 * 
 * It also draws zoom rectangle and performs zooming by executing corresponding
 * command.
 */
public class MarqueeZoomInteractive extends BaseInteractive {
    @Override
    public void activate(boolean active) {
        super.activate(active);
        if (isActive()) {
            this.startPoint = new Point();
            this.endPoint = new Point();
        }
    }

    @Override
    public void mousePressed(InputData input) {
        if (input.getLeftButton()) {
            startPoint = input.getLocation();
            endPoint = new Point(startPoint);

            getPdfView().repaintView();
            input.markProcessed();
        }
    }

    @Override
    public void mouseMoved(InputData input) {
        if (input.getLeftButton()) {
            endPoint = input.getLocation();

            getPdfView().repaintView();
            input.markProcessed();
        }
    }

    @Override
    public void mouseReleased(InputData input) {
        getApplication().executeCommand(new MarqueeZoomCommand(calcMarqueeRect(), getPageModel()));
        input.markProcessed();
    }

    @Override
    public void keyPressed(InputData input) {
        if (input.getControlKey(InputData.ControlKey.ESCAPE)) {
            startPoint = endPoint = new Point();
            getApplication().executeCommand(new MarqueeZoomCommand(calcMarqueeRect(), getPageModel()));
            input.markProcessed();
        }
    }

    @Override
    public void draw(Graphics g, Rectangle updateRect) {
        final Rectangle marqueeRect = calcMarqueeRect();

        if (!marqueeRect.isEmpty()) {
            g.setColor(new Color(96, 96, 96, 64));
            g.fillRect(marqueeRect.x, marqueeRect.y, marqueeRect.width, marqueeRect.height);

            g.setColor(Color.black);
            g.drawRect(marqueeRect.x, marqueeRect.y, marqueeRect.width, marqueeRect.height);
        }
    }

    private Rectangle calcMarqueeRect() {
        return new Rectangle(Math.min(startPoint.x, endPoint.x), Math.min(startPoint.y, endPoint.y), Math.abs(startPoint.x - endPoint.x), Math.abs(startPoint.y - endPoint.y));
    }

    private Point startPoint;
    private Point endPoint;
}
