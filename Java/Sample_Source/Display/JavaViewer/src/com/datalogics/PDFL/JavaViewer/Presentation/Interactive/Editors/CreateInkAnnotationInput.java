/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * CreateInkAnnotationInput - allows to create PDFL.InkAnnotation.
 * 
 * This class adds new scribbles to the InkAnnotation.
 */
public class CreateInkAnnotationInput extends CreateAnnotationInput {
    public CreateInkAnnotationInput() {
        scribbles = new ArrayList<List<Point>>();
        scribbleIndex = -1;
    }

    @Override
    protected boolean canMoveGuide(InputData input) {
        return input.getMousePressed() && input.getLeftButton() && canCreateGuide(input);
    }

    @Override
    protected void moveGuide(InputData input) {
        super.moveGuide(input);
        Point point = getAnnotationEditor().getPageModel().transform(input.getLocation());
        updateScribbles(point, scribbleIndex);
    }

    @Override
    protected void createNewGuide(InputData input) {
        updateScribbles(getAnnotationEditor().getPageModel().transform(input.getLocation()), ++scribbleIndex);
    }

    @Override
    protected boolean canComplete(InputData input) {
        return input.getRightButton();
    }

    @Override
    protected void doComplete(InputData input) {
        showContextMenu(input.getLocation());
    }

    private void updateScribbles(Point point, int index) {
        if (scribbles.size() - 1 < index)
            scribbles.add(new ArrayList<Point>());

        scribbles.get(index).add(point);
        getAnnotationEditor().getHolder().getProperties().setScribbles(scribbles);
    }

    private List<List<Point>> scribbles;
    private int scribbleIndex;
}
