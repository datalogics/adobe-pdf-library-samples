/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * CreateLineAnnotationInput - allows to create PDFL.LineAnnotation.
 * 
 * It sets start/end points and returns handling to the base class.
 */
public class CreateLineAnnotationInput extends CreateAnnotationInput {
    @Override
    protected void createNewGuide(InputData input) {
        final Point startPoint = getAnnotationEditor().getPageModel().transform(input.getLocation());
        getAnnotationEditor().getProperties().setStartPoint(startPoint);
        getAnnotationEditor().getProperties().setEndPoint(startPoint);
        setCapturedGuide(GuideRectangle.Layout.custom(BaseAnnotationEditor.END_LINE_GUIDE));
    }
}
