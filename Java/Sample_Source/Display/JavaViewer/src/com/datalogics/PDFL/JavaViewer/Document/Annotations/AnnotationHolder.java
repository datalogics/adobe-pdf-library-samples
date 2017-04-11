/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

/**
 * AnnotationHolder - an interface which has two implementations (at this
 * moment): first - BaseAnnotationHolder; second - GroupAnnotationHolder.
 * 
 * Interface contains methods to capture/release annotation and to update
 * annotation's appearance.
 * 
 * AnnotationHolder.AppearanceUpdate - contains possible update types for
 * annotations. Used in AnnotationListener and AnnotationListenerBroadcaster
 * update() methods.
 */
public interface AnnotationHolder {
    public static enum AppearanceUpdate {
        OBJECT_HIDDEN, // hide annotation when it was captured
        OBJECT_SHOWN, // show annotation when it was released
        SHAPE_CHANGED, // annotation was moved or sized
        PROPERTIES_CHANGED // some of annotation properties(color, line style,
                           // line width an so on) was changed
    }

    public int getIndex();

    public int getPageIndex();

    public void setIndex(int index);

    public AnnotationProperties getProperties();

    public void addAnnotationListener(AnnotationListener listener);

    public void removeAnnotationListener(AnnotationListener listener);

    public void updateAppearance();

    public void capture();

    public void release();
}
