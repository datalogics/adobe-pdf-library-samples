/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.InkAnnotation;
import com.datalogics.PDFL.Point;

/**
 * InkAnnotationHolder - the child of the BaseAnnotationHolder.
 * 
 * It is the wrapper for PDFL.InkAnnotation. This class allows to read and write
 * scribbles and set other special parameters.
 */
public class InkAnnotationHolder extends BaseAnnotationHolder {
    @Override
    protected void init(Annotation annotation, int index, int pageIndex) {
        this.annotation = (InkAnnotation) annotation;
        super.init(annotation, index, pageIndex);
    }

    @Override
    protected void doReadProperties() {
        super.doReadProperties();
        readPropertyByName(AnnotationConsts.DASH_PATTERN, List.class /*List<Double>*/, false);
        readPropertyByName(AnnotationConsts.STYLE, String.class, false);
        readPropertyByName(AnnotationConsts.WIDTH, double.class, true);
        readScribbles();

    }

    private void readScribbles() {
        List<List<Point>> scribbles = new ArrayList<List<Point>>();
        for (int i = 0; i < annotation.getNumScribbles(); ++i) {
            scribbles.add(annotation.getScribble(i));
        }
        getProperties().setNotNativeProperty(AnnotationConsts.SCRIBBLES, scribbles, List.class, true);
    }

    @Override
    protected void doWriteProperties() {
        super.doWriteProperties();
        writeScribbles(getProperties().getScribbles());
    }

    private void writeScribbles(List<List<Point>> scribbles) {
        while (annotation.getNumScribbles() > 0)
            annotation.removeScribble(0);
        for (List<Point> scribble : scribbles)
            annotation.addScribble(scribble);
    }

    private InkAnnotation annotation;
}
