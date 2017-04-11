/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Dimension;
import java.awt.Rectangle;

import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * CreateRectangularAnnotationInput - allows to create PDFL.CircleAnnotation and PDFL.SquareAnnotation.
 * 
 * It sets starting bounding box and returns handling to the base class.
 */
public class CreateRectangularAnnotationInput extends CreateAnnotationInput {
    @Override
    protected void createNewGuide(InputData input) {
        final Rect boundingRect = getAnnotationEditor().getPageModel().transform(new Rectangle(input.getLocation(), new Dimension(1, 1)));
        getAnnotationEditor().getHolder().getProperties().setBoundingRect(boundingRect);
        setCapturedGuide(GuideRectangle.Layout.SE);
    }
}
