/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Graphics;
import java.util.List;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * CreateTextMarkupOutput - allows to draw PDFL.TextMarkupAnnotation when it was
 * selected or new is creating.
 * 
 * It also changes cursor view.
 */
public class CreateTextMarkupOutput extends BaseAnnotationOutput {
    public CreateTextMarkupOutput(PDF.Cursor cursor) {
        this.cursor = cursor;
    }

    @Override
    public void activate(boolean active) {
        super.activate(active);
        cachedQuads = active ? ((TextMarkupAnnotationEditor) getAnnotationEditor()).getCachedQuads() : null;
    }

    @Override
    public void drawShape(Graphics g) {
        super.drawShape(g);
        getAnnotationEditor().drawShape(g);
    }

    @Override
    public void drawSelection(Graphics g) {
        super.drawSelection(g);
    }

    @Override
    public void changeCursor(Hit hit) {
        super.changeCursor(hit);
        if (hasQuad(hit.getLocation())) {
            applyCursor(cursor);
        } else {
            revertCursor();
        }
    }

    private boolean hasQuad(Point point) {
        boolean hasQuad = false;
        for (int i = 0; i < cachedQuads.size() && !hasQuad; ++i) {
            hasQuad = getAnnotationEditor().getPageModel().transform(cachedQuads.get(i).toRect()).contains(getAnnotationEditor().getPageModel().transform(point));

        }
        return hasQuad;
    }

    private PDF.Cursor cursor;
    private List<Quad> cachedQuads;
}
