/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Graphics2D;
import java.util.List;

import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Point;

/**
 * InkAnnotationEditor - allows to edit PDFL.InkAnnotation.
 * 
 * It draws PDFL.InkAnnotation on the page.
 */
public class InkAnnotationEditor extends BaseAnnotationEditor {
    @Override
    protected void doTransform(GuideRectangle guide, boolean moveAll, Matrix matrix) {
        List<List<Point>> newScribbles = getProperties().getScribbles();
        for (int i = 0; i < newScribbles.size(); ++i) {
            for (int j = 0; j < newScribbles.get(i).size(); j++)
                newScribbles.get(i).set(j, ensurePointInPage(newScribbles.get(i).get(j).transform(matrix)));
        }

        getProperties().setScribbles(newScribbles);
        getProperties().setBoundingRect(getProperties().getBoundingRect().transform(matrix));
    }

    @Override
    protected void doDrawShape(Graphics2D g) {
        g.setColor(getProperties().getForeColor());
        List<List<Point>> scribbles = getProperties().getScribbles();
        for (int i = 0; i < scribbles.size(); ++i) {
            List<Point> vertices = scribbles.get(i);
            int[] xPoint = new int[vertices.size()];
            int[] yPoint = new int[vertices.size()];
            for (int j = 0; j < vertices.size(); ++j) {
                xPoint[j] = getPageModel().transform(vertices.get(j)).x;
                yPoint[j] = getPageModel().transform(vertices.get(j)).y;
            }
            g.drawPolyline(xPoint, yPoint, vertices.size());
        }
    }
}
