/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder.AppearanceUpdate;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.GroupAnnotationHolder;

/**
 * GroupAnnotationEditor - manages GroupAnnotation's editors.
 * 
 * This class adds, removes an editor to the group. It also calculates bounding
 * box for group annotation.
 */
public class GroupAnnotationEditor extends BaseAnnotationEditor {
    public GroupAnnotationEditor() {
        editors = new ArrayList<BaseAnnotationEditor>();
    }

    @Override
    protected void doTransform(GuideRectangle guide, boolean moveAll, Matrix matrix) {
        super.doTransform(guide, moveAll, matrix);
        if (moveAll) {
            for (BaseAnnotationEditor editor : getEditors()) {
                editor.doTransform(guide, moveAll, matrix);
            }
        }
    }

    @Override
    protected void doDrawShape(Graphics2D gr) {
        final AnnotationProperties annotationProperties = getProperties();
        final int penWidth = (int) Math.round(annotationProperties.getLineWidth() * getPageModel().getScale());
        Rectangle boundingRect = getPageModel().transform(annotationProperties.getBoundingRect());
        boundingRect.grow(-penWidth / 2, -penWidth / 2); // consider pen width for circle bounds

        gr.setColor(Color.RED);
        gr.drawRect(boundingRect.x, boundingRect.y, boundingRect.width, boundingRect.height);
    }

    public void addEditor(BaseAnnotationEditor editor) {
        editors.add(editor);
        editor.capture();
        getHolder().getProperties().setBoundingRect(calcBoundRect());
        ((GroupAnnotationHolder) getHolder()).notifyShow(false);
    }

    public void removeEditor(BaseAnnotationEditor editor) {
        removeEditorInternal(editor);
        getHolder().getProperties().setBoundingRect(calcBoundRect());
        ((GroupAnnotationHolder) getHolder()).notifyShow(true);
    }

    public void clearEditors() {
        for (BaseAnnotationEditor editor : getEditors()) {
            removeEditorInternal(editor);
        }
        getHolder().getProperties().setBoundingRect(new Rect());
    }

    public BaseAnnotationEditor getEditor(int index) {
        return editors.get(index);
    }

    public List<BaseAnnotationEditor> getEditors() {
        return new ArrayList<BaseAnnotationEditor>(editors);
    }

    public boolean hasEditor(BaseAnnotationEditor editor) {
        return editors.indexOf(editor) != -1;
    }

    public int getEditorsCount() {
        return editors.size();
    }

    private void removeEditorInternal(BaseAnnotationEditor editor) {
        editors.remove(editor);
        editor.release();
    }

    private Rect calcBoundRect() {
        Rect bounds = editors.isEmpty() ? new Rect() : editors.get(0).getHolder().getProperties().getBoundingRect();
        for (BaseAnnotationEditor editor : getEditors()) {
            bounds.setLeft(Math.min(bounds.getLLx(), editor.getHolder().getProperties().getBoundingRect().getLLx()));
            bounds.setLLy(Math.min(bounds.getLLy(), editor.getHolder().getProperties().getBoundingRect().getLLy()));

            bounds.setURx(Math.max(bounds.getURx(), editor.getHolder().getProperties().getBoundingRect().getURx()));
            bounds.setURy(Math.max(bounds.getURy(), editor.getHolder().getProperties().getBoundingRect().getURy()));
        }

        return bounds;
    }

    @Override
    public void update(Object updateData) {
        if (AppearanceUpdate.SHAPE_CHANGED.equals(updateData) || AppearanceUpdate.PROPERTIES_CHANGED.equals(updateData)) {
            for (BaseAnnotationEditor editor : getEditors()) {
                final Rect rect = editor.getProperties().getBoundingRect();
                editor.getProperties().updateFrom(getProperties());

                editor.getProperties().setBoundingRect(rect);
                editor.getProperties().raiseUpdates(true);
            }
            getProperties().setDirty(false);
        }
    }

    private List<BaseAnnotationEditor> editors;
}
