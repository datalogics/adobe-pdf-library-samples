/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.util.List;
import java.util.Map;

import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * PolyAnnotationEditor - allows to edit PDFL.PolyLineAnnotation and
 * PDFL.PolygonAnnotation.
 */
public abstract class PolyAnnotationEditor extends BaseAnnotationEditor {
    @Override
    protected void doTransform(GuideRectangle guide, boolean moveAll, Matrix matrix) {
        final int layout = moveAll ? GuideRectangle.Layout.NOTHING : guide.getLayout();
        final List<Point> vertices = getProperties().getVertices();
        for (int i = 0; i < vertices.size(); ++i) {
            if (moveAll || layout == getVertexGuide(i)) {
                vertices.set(i, (ensurePointInPage(vertices.get(i).transform(matrix))));
            }
        }
        getProperties().setVertices(vertices);
    }

    @Override
    protected void doConfigureGuides(Map<Integer, GuideRectangle> guides, Rect r) {
        super.doConfigureGuides(guides, r);

        final List<Point> vertices = getProperties().getVertices();
        for (int i = 0; i < vertices.size(); ++i) {
            final int layout = getVertexGuide(i);
            guides.put(layout, new GuideRectangle(vertices.get(i), layout, PDF.Cursor.MOVE));
        }
    }

    protected int[] getXPoints() {
        int[] xPoints = new int[getPointCount()];
        int i = 0;
        for (Point p : getProperties().getVertices()) {
            java.awt.Point point = getPageModel().transform(p);
            xPoints[i] = point.x;
            i++;
        }
        return xPoints;
    }

    protected int[] getYPoints() {
        int[] yPoints = new int[getPointCount()];
        int i = 0;
        for (Point p : getProperties().getVertices()) {
            java.awt.Point point = getPageModel().transform(p);
            yPoints[i] = point.y;
            i++;
        }
        return yPoints;
    }

    protected int getPointCount() {
        return getProperties().getVertices().size();
    }

    private int getVertexGuide(int index) {
        return GuideRectangle.Layout.custom(index);
    }
}
