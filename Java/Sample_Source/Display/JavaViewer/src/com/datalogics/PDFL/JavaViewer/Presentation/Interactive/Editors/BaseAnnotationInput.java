/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Point;

import com.datalogics.PDFL.JavaViewer.Presentation.Enums.MenuType;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * BaseAnnotationInput - handles mouse activity.
 * 
 * This class exposes input handler for editor an redirects input to it. It
 * shows context menu depending on annotation type.
 */
public abstract class BaseAnnotationInput {
    public void activate(boolean active) {
        // make binding of annotation handler with editor
        if (active) {
            getAnnotationEditor().capture();
            onActivate();
        } else {
            onDeactivate();
            getAnnotationEditor().release();
        }
    }

    public void mousePressed(InputData input) {
        getAnnotationEditor().testHit(input.getLocation()); // initiate shape cursor change if needed
    }

    public void mouseReleased(InputData input) {
        getAnnotationEditor().testHit(input.getLocation()); // initiate shape cursor change if needed
    }

    public void mouseMoved(InputData input) {
        if (!input.getMousePressed())
            getAnnotationEditor().testHit(input.getLocation()); // initiate shape cursor change if needed
    }

    public void mouseDoubleClicked(InputData input) {
        getAnnotationEditor().testHit(input.getLocation()); // initiate shape cursor change if needed
    }

    public void keyPressed(InputData input) {
    }

    public void keyReleased(InputData input) {
    }

    public void keyTyped(InputData input) {
    }

    public void setAnnotationEditor(BaseAnnotationEditor annotationEditor) {
        this.annotationEditor = annotationEditor;
    }

    public void showContextMenu(MenuType menuType, Point location) {
        getAnnotationEditor().getInteractiveContext().getApplication().getView().showContextMenu(menuType, location);
    }

    public MenuType getMenuType() {
        MenuType menuType = MenuType.NONE;
        final BaseAnnotationEditor editor = getAnnotationEditor();
        if (editor instanceof SquareAnnotationEditor || editor instanceof CircleAnnotationEditor || editor instanceof InkAnnotationEditor || editor instanceof PolygonAnnotationEditor || editor instanceof GroupAnnotationEditor) {
            menuType = MenuType.RECTANGULAR_ANNOTATION;
        } else if (editor instanceof LineAnnotationEditor) {
            menuType = MenuType.LINE_ANNOTATION;
        } else if (editor instanceof LinkAnnotationEditor) {
            menuType = MenuType.LINK_ANNOTATION;
        } else if (editor instanceof PolyLineAnnoationEditor) {
            menuType = MenuType.POLYLINE_ANNOTATION;
        } else if (editor instanceof TextMarkupAnnotationEditor) {
            menuType = MenuType.TEXTMARKUP_ANNOTATION;
        } else if (editor instanceof FreeTextAnnotationEditor) {
            menuType = MenuType.FREETEXT_ANNOTATION;
        }

        return menuType;
    }

    final protected BaseAnnotationEditor getAnnotationEditor() {
        return this.annotationEditor;
    }

    protected void onActivate() {
    }

    protected void onDeactivate() {
    }

    private BaseAnnotationEditor annotationEditor;
}
