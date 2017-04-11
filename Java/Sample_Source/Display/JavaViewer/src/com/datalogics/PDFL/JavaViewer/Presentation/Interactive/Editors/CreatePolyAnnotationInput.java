/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.util.List;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * CreatePolyAnnotationInput - allows to create PDFL.PolyLine and PDFL.Polygon annotations.
 */
public class CreatePolyAnnotationInput extends CreateAnnotationInput {
    @Override
    protected void createNewGuide(InputData input) {
        final Point vertex = getAnnotationEditor().getPageModel().transform(input.getLocation());

        if (!hasVertices()) {
            vertices = getAnnotationEditor().getProperties().getVertices();
            vertices.clear();
            vertices.add(vertex);
        }

        vertices.add(vertex);
        getAnnotationEditor().getHolder().getProperties().setVertices(vertices);
        setCapturedGuide(GuideRectangle.Layout.custom(getLastElement()));
    }

    @Override
    protected boolean canMoveGuide(InputData input) {
        return hasVertices();
    }

    @Override
    protected void moveGuide(InputData input) {
        super.moveGuide(input);
        vertices.set(getLastElement(), getAnnotationEditor().getPageModel().transform(input.getLocation()));
    }

    @Override
    protected boolean canComplete(InputData input) {
        return input.getRightButton();
    }

    @Override
    protected void doComplete(InputData input) {
        showContextMenu(input.getLocation());
    }

    private int getLastElement() {
        return vertices.size() - 1;
    }

    private boolean hasVertices() {
        return vertices != null;
    }

    private List<Point> vertices; // cached version of vertices
}
