/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document;

import java.util.List;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;

/**
 * DocumentListener - an interface which allows its implementers to receive
 * notifications about document change.
 */
public interface DocumentListener {
    /**
     * This method is called when document is opened, closed or appended.
     */
    public void pdfChanged(Document pdfDoc);

    /**
     * Called when permissions get updated.
     */
    public void permissionChanged();

    /**
     * This method is called when layers get updated.
     */
    public void pdfLayerUpdated(List<OptionalContentGroup> ocgList);

    /**
     * The method is called when page contents changed at the specified area.
     */
    public void pageChanged(int index, Rect changedArea);

    /**
     * Called when document selection is changed.
     */
    public void selectionChanged(Object oldSelection);

    /**
     * Called when new annotation is created, updated or deleted.
     */
    public void annotationUpdated(AnnotationHolder holder, boolean created);
}
