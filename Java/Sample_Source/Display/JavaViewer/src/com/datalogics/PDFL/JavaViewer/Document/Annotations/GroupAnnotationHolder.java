/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

/**
 * GroupAnnotationHolder - the child of the AnnotationHolder.
 * 
 * This holder is used when group annotation has been created. This class has
 * the same behavior as BaseAnnotationHolder except that group annotation can't
 * be sized.
 */
public class GroupAnnotationHolder implements AnnotationHolder {
    public void addAnnotationListener(AnnotationListener listener) {
        listenerBroadcast.addAnnotationListener(listener);
    }

    public void removeAnnotationListener(AnnotationListener listener) {
        listenerBroadcast.removeAnnotationListener(listener);
    }

    void init(int pageIndex) {
        this.listenerBroadcast = new AnnotationListenerBroadcaster();
        this.pageIndex = pageIndex;

        this.properties = new AnnotationProperties();
        properties.setSubtype(AnnotationConsts.Subtypes.GROUP);
        properties.setUpdateAlways(true);
        properties.setDirty(false);
    }

    public int getIndex() {
        return index;
    }

    public void setIndex(int index) {
        this.index = index;
    }

    public int getPageIndex() {
        return pageIndex;
    }

    public AnnotationProperties getProperties() {
        return this.properties;
    }

    public void notifyShow(boolean show) {
        listenerBroadcast.onUpdate(show ? AppearanceUpdate.OBJECT_SHOWN : AppearanceUpdate.OBJECT_HIDDEN);
    }

    public void capture() {
        properties.addAnnotationListener(listenerBroadcast);
    }

    public void release() {
        properties.removeAnnotationListener(listenerBroadcast);
    }

    public void updateAppearance() {
    }

    private AnnotationProperties properties;
    private int pageIndex;
    private int index = Integer.MAX_VALUE - 1;
    private AnnotationListenerBroadcaster listenerBroadcast;
}
