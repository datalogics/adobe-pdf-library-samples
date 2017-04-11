/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

/**
 * PolygonAnnotationHolder - the child of the PolyAnnotationHolder.
 * 
 * This class wraps a PDFL.PolygonAnnotation object.
 */
import com.datalogics.PDFL.Annotation;

public class PolygonAnnotationHolder extends PolyAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        super.init(annotation, index, pageIndex);
    }
}
