/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Graphics;

import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Interactive.InteractiveContext;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * BaseAnnotationOutput - helper class which is
 * and for setting proper mouse cursor.
 * 
 * This class exposes custom output processing for editor depending on current
 * editor's state.
 */
public abstract class BaseAnnotationOutput {
    public void activate(boolean active) {
        if (!active) {
            revertCursor();
        }
    }

    public void drawShape(Graphics g) {
    }

    public void drawSelection(Graphics g) {
    }

    public void changeCursor(Hit hit) {
    }

    public void setAnnotationEditor(BaseAnnotationEditor annotationEditor) {
        this.annotationEditor = annotationEditor;
    }

    final protected BaseAnnotationEditor getAnnotationEditor() {
        return this.annotationEditor;
    }

    final protected PDF getPdfView() {
        return getAnnotationEditor().getPdfView();
    }

    final protected InteractiveContext getContext() {
        return getAnnotationEditor().getInteractiveContext();
    }

    final protected void applyCursor(PDF.Cursor cursor) {
        if (!cursor.equals(savedCursor)) {
            savedCursor = getPdfView().getViewCursor();
            getContext().changeCursor(getAnnotationEditor(), cursor);
        }
    }

    protected void revertCursor() {
        if (savedCursor != null) {
            getContext().revertCursor(getAnnotationEditor());
            savedCursor = null;
        }
    }

    private BaseAnnotationEditor annotationEditor;
    private PDF.Cursor savedCursor;
}
