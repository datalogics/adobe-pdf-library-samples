/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Graphics;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * GroupAnnotationOutput - allows to draw GroupAnnotation.
 * 
 * It also responsible for cursor view.
 */
public class GroupAnnotationOutput extends BaseAnnotationOutput {

    public GroupAnnotationOutput(PDF.Cursor cursor, boolean onlyCursor) {
        this.cursor = cursor;
        this.onlyCursor = onlyCursor;
    }

    @Override
    public void drawShape(Graphics g) {
        super.drawShape(g);
        for (BaseAnnotationEditor editor : ((GroupAnnotationEditor) getAnnotationEditor()).getEditors()) {
            editor.drawShape(g);
        }
    }

    @Override
    public void drawSelection(Graphics g) {
        super.drawSelection(g);
        getAnnotationEditor().drawBoundingRect(g);
    }

    @Override
    public void changeCursor(Hit hit) {
        super.changeCursor(hit);
        if (hit.noHit() && !onlyCursor) {
            revertCursor();
        } else {
            applyCursor(onlyCursor || hit.getGuide() == null ? cursor : hit.getGuide().getCursor());
        }
    }

    private PDF.Cursor cursor;
    private boolean onlyCursor;
}
