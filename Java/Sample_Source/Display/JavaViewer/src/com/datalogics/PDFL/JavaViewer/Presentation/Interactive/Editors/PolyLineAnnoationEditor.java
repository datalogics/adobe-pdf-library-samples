/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Graphics2D;

/**
 * PolyLineAnnotationEditor - draws polyline annotation on the page.
 */
public class PolyLineAnnoationEditor extends PolyAnnotationEditor {
    @Override
    protected void doDrawShape(Graphics2D gr) {
        gr.setColor(getProperties().getForeColor());
        gr.drawPolyline(getXPoints(), getYPoints(), getPointCount());
    }
}
