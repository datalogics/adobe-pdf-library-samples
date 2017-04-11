/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Annotations;

/**
 * This interface notifies its implementers about annotation update of different
 * types.
 * 
 * Main types of updates are: - annotation hidden/shown
 * (AnnotationHolder.AppearanceUpdate.OBJECT_HIDDEN/AnnotationHolder
 * .AppearanceUpdate.OBJECT_SHOWN); - annotation's shape changed - it's moved or
 * resized (AnnotationHolder.AppearanceUpdate.SHAPE_CHANGED); - annotation's
 * properties changed (AnnotationHolder.AppearanceUpdate.PROPERTIES_CHANGED)
 * other then move or resize.
 */
public interface AnnotationListener {
    void update(Object updateData);
}
