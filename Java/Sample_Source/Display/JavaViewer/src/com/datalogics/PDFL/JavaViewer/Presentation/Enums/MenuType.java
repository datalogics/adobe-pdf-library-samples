/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Enums;

/**
 * MenuType - used from context menu to determine annotation type.
 */
public enum MenuType {
    NONE,
    RECTANGULAR_ANNOTATION, // circle, square, ink, group, polygon
    LINE_ANNOTATION, // line
    POLYLINE_ANNOTATION,
    LINK_ANNOTATION,
    TEXTMARKUP_ANNOTATION,
    FREETEXT_ANNOTATION;

    public boolean isRectangular() {
        return this.equals(RECTANGULAR_ANNOTATION);
    }

    public boolean isLine() {
        return this.equals(LINE_ANNOTATION);
    }

    public boolean isPolyLine() {
        return this.equals(POLYLINE_ANNOTATION);
    }

    public boolean isLink() {
        return this.equals(LINK_ANNOTATION);
    }

    public boolean isFreeText() {
        return this.equals(FREETEXT_ANNOTATION);
    }

    public boolean isMarkup() {
        return this.equals(TEXTMARKUP_ANNOTATION);
    }
}
