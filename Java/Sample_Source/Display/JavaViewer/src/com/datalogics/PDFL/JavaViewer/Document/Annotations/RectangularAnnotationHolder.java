/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import java.util.List;

import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.Color;

/**
 * RectangularAnnotationHolder - the child of the BaseAnnotationHolder.
 * 
 * This class is responsible for manipulation of Square and Circle annotations.
 */
public class RectangularAnnotationHolder extends BaseAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        super.init(annotation, index, pageIndex);
    }

    @Override
    protected void doReadProperties() {
        super.doReadProperties();

        readPropertyByName(AnnotationConsts.WIDTH, double.class, true);
        readPropertyByName(AnnotationConsts.INTERIOR_COLOR, Color.class, false);
        readPropertyByName(AnnotationConsts.DASH_PATTERN, List.class /*List<Double>*/, false);
        readPropertyByName(AnnotationConsts.STYLE, String.class /*List<Double>*/, false);

    }
}
