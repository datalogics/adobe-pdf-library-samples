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

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationConsts;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;

/**
 * AnnotationEditorFactory - has static methods for creating annotation editors
 * and inputs.
 */
public class AnnotationEditorFactory {
    public static BaseAnnotationEditor createEditor(AnnotationHolder annotationHolder) {
        BaseAnnotationEditor editor = null;
        final String subType = annotationHolder != null ? annotationHolder.getProperties().getSubtype() : "";

        if (subType.equals(AnnotationConsts.Subtypes.GROUP)) {
            editor = new GroupAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.LINE)) {
            editor = new LineAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.CIRCLE)) {
            editor = new CircleAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.SQUARE)) {
            editor = new SquareAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.POLYGON)) {
            editor = new PolygonAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.POLYLINE)) {
            editor = new PolyLineAnnoationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.FREETEXT)) {
            editor = new FreeTextAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.LINK)) {
            editor = new LinkAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.INK)) {
            editor = new InkAnnotationEditor();
        } else if (subType.equals(AnnotationConsts.Subtypes.HIGHLIGHT) || subType.equals(AnnotationConsts.Subtypes.UNDERLINE)) {
            TextMarkupAnnotationEditor markupEditor = new TextMarkupAnnotationEditor();
            markupEditor.setSelectionColor(subType.equals(AnnotationConsts.Subtypes.HIGHLIGHT) ? Color.YELLOW : Color.GREEN);
            editor = markupEditor;
        } else {
            editor = new BaseAnnotationEditor() {
                @Override
                protected void doDrawShape(Graphics2D g) {
                }
            };
        }
        editor.init(annotationHolder, EditorStates.NONE);
        return editor;
    }

    public static BaseAnnotationInput createInput(AnnotationHolder annotationHolder) {
        BaseAnnotationInput input = null;
        final String subType = annotationHolder != null ? annotationHolder.getProperties().getSubtype() : "";

        if (subType.equals(AnnotationConsts.Subtypes.LINE)) {
            input = new CreateLineAnnotationInput();
        } else if (subType.equals(AnnotationConsts.Subtypes.CIRCLE) || subType.equals(AnnotationConsts.Subtypes.SQUARE)) {
            input = new CreateRectangularAnnotationInput();
        } else if (subType.equals(AnnotationConsts.Subtypes.POLYGON) || subType.equals(AnnotationConsts.Subtypes.POLYLINE)) {
            input = new CreatePolyAnnotationInput();
        } else if (subType.equals(AnnotationConsts.Subtypes.FREETEXT)) {
            input = new CreateCustomEditAnnotationInput();
        } else if (subType.equals(AnnotationConsts.Subtypes.LINK)) {
            input = new CreateCustomEditAnnotationInput();
        } else if (subType.equals(AnnotationConsts.Subtypes.INK)) {
            input = new CreateInkAnnotationInput();
        } else if (subType.equals(AnnotationConsts.Subtypes.HIGHLIGHT) || subType.equals(AnnotationConsts.Subtypes.UNDERLINE)) {
            input = new CreateTextMarkupAnnotationInput();
        } else {
            input = new BaseAnnotationInput() {
            };
        }
        return input;
    }
}
