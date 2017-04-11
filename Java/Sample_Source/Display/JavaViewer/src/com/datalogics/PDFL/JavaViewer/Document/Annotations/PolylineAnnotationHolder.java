/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.LineEndingStyle;

/**
 * PolylineAnnotationHolder - the child of the PolyAnnotationHolder.
 * 
 * This class wraps PDFL.PolyLineAnnotation and sets additional properties for
 * it.
 */
public class PolylineAnnotationHolder extends PolyAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        super.init(annotation, index, pageIndex);
    }

    @Override
    protected void doReadProperties() {
        super.doReadProperties();

        readPropertyByName(AnnotationConsts.START_ENDING_STYLE, LineEndingStyle.class, false);
        readPropertyByName(AnnotationConsts.END_ENDING_STYLE, LineEndingStyle.class, false);
    }
}
