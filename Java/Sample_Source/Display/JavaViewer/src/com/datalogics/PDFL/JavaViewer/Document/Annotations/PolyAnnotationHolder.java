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
 * PolyAnnotationHolder - the child of the BaseAnnotationHolder.
 * 
 * This class is an ancestor of PolyLineHolder and PolygonHolder. It sets common
 * properties for its children. Children of PolyAnnotationHolder can set
 * additional properties by themselves.
 */
public class PolyAnnotationHolder extends BaseAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        super.init(annotation, index, pageIndex);
    }

    @Override
    protected void doReadProperties() {
        super.doReadProperties();

        readPropertyByName(AnnotationConsts.DASH_PATTERN, List.class /*List<Double>*/, false);
        readPropertyByName(AnnotationConsts.INTERIOR_COLOR, Color.class, false);
        readPropertyByName(AnnotationConsts.STYLE, String.class, false);
        readPropertyByName(AnnotationConsts.VERTICES, List.class /*List<Point>*/, true);
        readPropertyByName(AnnotationConsts.WIDTH, double.class, true);
    }

}
