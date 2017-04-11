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
import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.Point;

/**
 * LineAnnotationHolder - the child of the BaseAnnotationHolder.
 * 
 * This class is a wrapper for PDFL.LineAnnotation.
 */
public class LineAnnotationHolder extends BaseAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        super.init(annotation, index, pageIndex);
    }

    @Override
    protected void doReadProperties() {
        super.doReadProperties();

        readPropertyByName(AnnotationConsts.START_POINT, Point.class, true);
        readPropertyByName(AnnotationConsts.END_POINT, Point.class, true);
        readPropertyByName(AnnotationConsts.DASH_PATTERN, List.class /*List<Double>*/, false);
        readPropertyByName(AnnotationConsts.INTERIOR_COLOR, Color.class, false);
        readPropertyByName(AnnotationConsts.STYLE, String.class, false);
        readPropertyByName(AnnotationConsts.WIDTH, double.class, true);
        readPropertyByName(AnnotationConsts.START_ENDING_STYLE, LineEndingStyle.class, false);
        readPropertyByName(AnnotationConsts.END_ENDING_STYLE, LineEndingStyle.class, false);
    }
}
