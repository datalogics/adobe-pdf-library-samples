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

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder.AppearanceUpdate;

/**
 * AnnotationListenerBroadcaster - the class which implements AnnotationListener
 * and keeps AnnotationListeners' list. It is used by AnnotationHolder's
 * implementers for adding listener to the AnnotationListeners' list or removing
 * listener from it. This class also notifies all its listeners when the
 * annotation has been updated.
 */
class AnnotationListenerBroadcaster implements AnnotationListener {
    AnnotationListenerBroadcaster() {
        listeners = new ArrayList<AnnotationListener>();
    }

    public void update(Object shapeChanged) {
        onUpdate(Boolean.TRUE.equals(shapeChanged) ? AppearanceUpdate.SHAPE_CHANGED : AppearanceUpdate.PROPERTIES_CHANGED);
    }

    void addAnnotationListener(AnnotationListener listener) {
        listeners.add(listener);
    }

    void removeAnnotationListener(AnnotationListener listener) {
        listeners.remove(listener);
    }

    void onUpdate(AppearanceUpdate appearanceUpdate) {
        for (AnnotationListener listener : getListeners()) {
            listener.update(appearanceUpdate);
        }
    }

    private AnnotationListener[] getListeners() {
        return listeners.toArray(new AnnotationListener[0]);
    }

    private final List<AnnotationListener> listeners;
}
