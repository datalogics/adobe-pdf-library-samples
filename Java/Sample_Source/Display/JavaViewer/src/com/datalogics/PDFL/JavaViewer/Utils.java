/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer;

import java.util.List;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.Rect;

/**
 * Utils - class which keeps a few static methods for internal necessities of
 * the application. These are: 1. Methods for transformation from Datalogics
 * data structures (colors, points, rectangles) to java and vice versa. 2.
 * Methods for cloning Datalogics data structures (rectangles, quads, points).
 */
public class Utils {

    public static Point clone(Point point) {
        return new Point(point.getH(), point.getV());
    }

    public static Rect clone(Rect rect) {
        return new Rect(rect.getLeft(), rect.getBottom(), rect.getRight(), rect.getTop());
    }

    public static Rect cloneNonSorted(Rect rect) {
        Rect clone = new Rect();
        clone.setLLx(rect.getLLx());
        clone.setLLy(rect.getLLy());
        clone.setURx(rect.getURx());
        clone.setURy(rect.getURy());

        return clone;
    }

    public static Quad clone(Quad quad) {
        return new Quad(clone(quad.getTopLeft()), clone(quad.getTopRight()), clone(quad.getBottomLeft()), clone(quad.getBottomRight()));
    }

    public static boolean pointInRect(Point point, Rect rect) {
        return (point.getH() >= rect.getLLx()) && (point.getH() <= rect.getURx()) && (point.getV() >= rect.getLLy()) && (point.getV() <= rect.getURy());
    }

    public static Point rectCenter(Rect rect) {
        return new Point((rect.getLeft() + rect.getRight()) / 2.0, (rect.getTop() + rect.getBottom()) / 2.0);
    }

    public static java.awt.Color transform(com.datalogics.PDFL.Color color) {
        java.awt.Color newColor = null;
        if (color != null) {
            if (color.getSpace().equals(com.datalogics.PDFL.ColorSpace.DEVICE_GRAY)) {
                final float grey = color.getValue().get(0).floatValue();
                newColor = new java.awt.Color(grey, grey, grey);
            } else {
                final List<Double> rgb = color.getValue();
                newColor = new java.awt.Color(rgb.get(0).floatValue(), rgb.get(1).floatValue(), rgb.get(2).floatValue());
            }
        }
        return newColor;
    }

    public static com.datalogics.PDFL.Color transform(java.awt.Color color) {
        com.datalogics.PDFL.Color newColor = null;
        if (color != null) {
            newColor = new com.datalogics.PDFL.Color(color.getRed() / 255.0, color.getGreen() / 255.0, color.getBlue() / 255.0);
        }
        return newColor;
    }
}
