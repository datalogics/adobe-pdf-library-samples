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
 * HoverAnnotationOutput - changes cursor view and draws bounding box of the
 * hovered annotation.
 */
public class HoverAnnotationOutput extends BaseAnnotationOutput {
    @Override
    public void drawSelection(Graphics g) {
        super.drawSelection(g);
        getAnnotationEditor().drawBoundingRect(g);
    }

    @Override
    public void changeCursor(Hit hit) {
        super.changeCursor(hit);
        if (hit.noHit()) {
            revertCursor();
        } else {
            applyCursor(PDF.Cursor.HAND);
        }
    }
}
