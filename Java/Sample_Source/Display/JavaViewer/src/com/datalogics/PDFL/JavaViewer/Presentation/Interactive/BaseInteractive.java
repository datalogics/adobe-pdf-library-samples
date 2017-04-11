/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

import java.awt.Graphics;
import java.awt.Rectangle;

import com.datalogics.PDFL.JavaViewer.Presentation.Application;
import com.datalogics.PDFL.JavaViewer.Presentation.Cache.PageModel;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * BaseInteractive - implements Interactive interface.
 * 
 * It provides basic functionality for all interactive modes in the application.
 */
public abstract class BaseInteractive implements Interactive {
    public void mousePressed(InputData input) {
    }

    public void mouseReleased(InputData input) {
    }

    public void mouseMoved(InputData input) {
    }

    public void mouseDoubleClicked(InputData input) {
    }

    public void keyPressed(InputData input) {
    }

    public void keyReleased(InputData input) {
    }

    public void keyTyped(InputData input) {
    }

    public void draw(Graphics g, Rectangle updateRect) {
    }

    public void setInteractiveContext(InteractiveContext context) {
        this.context = context;
    }

    public void activate(boolean active) {
        this.active = active;
    }

    public void setActivePage(int page) {
        if (activePage != page) {
            activePage = page;
            onPageChanged();
        }
    }

    final protected int getActivePage() {
        return this.activePage;
    }

    final protected boolean isActive() {
        return this.active;
    }

    final protected Application getApplication() {
        return this.context.getApplication();
    }

    final protected PDF getPdfView() {
        return this.context.getPdfView();
    }

    final protected boolean hasPageModel() {
        return getPageModel() != null;
    }

    final protected PageModel getPageModel() {
        return this.context.getPageModel();
    }

    final protected InteractiveContext getInteractiveContext() {
        return this.context;
    }

    protected void onPageChanged() {
    }

    private boolean active;
    private InteractiveContext context;
    private int activePage;
}
