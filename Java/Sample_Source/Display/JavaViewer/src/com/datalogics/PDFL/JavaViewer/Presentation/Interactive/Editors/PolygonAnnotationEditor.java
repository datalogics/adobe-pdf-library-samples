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
 * PolygonAnnotationEditor - draws polygon annotation on the page.
 */
public class PolygonAnnotationEditor extends PolyAnnotationEditor {
    @Override
    protected void doDrawShape(Graphics2D gr) {
        if (getProperties().hasFill()) {
            gr.setColor(getProperties().getInteriorColor());
            gr.fillPolygon(getXPoints(), getYPoints(), getPointCount());
        }
        gr.setColor(getProperties().getForeColor());
        gr.drawPolygon(getXPoints(), getYPoints(), getPointCount());
    }
}
