/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Enums;

/**
 * ZoomMode - enum which determines zooming type for PDF document.
 * 
 * It also contains helper functions to calculate real zoom values from logical
 * values like 'next', 'previous' etc.
 */
public enum ZoomMode {
    ZOOM_NONE,
    ZOOM_DIRECT,
    ZOOM_NEXT,
    ZOOM_PREV,
    ZOOM_FIT_PAGE,
    ZOOM_FIT_WIDTH,
    ZOOM_FIT_HEIGHT;

    // helper functions for zoom manipulation
    public static double getNextZoom(double curZoom, double[] zoomArray) {
        final double zoomToFind = curZoom + EPS * 10;
        if (zoomArray == null || zoomToFind > zoomArray[zoomArray.length - 1])
            return curZoom;

        final int index = getArrayPos(zoomToFind, zoomArray, false);
        return (index != -1) ? zoomArray[index] : curZoom;
    }

    public static double getPrevZoom(double curZoom, double[] zoomArray) {
        final double zoomToFind = curZoom - EPS * 10;
        if (zoomArray == null || zoomToFind < zoomArray[0])
            return curZoom;

        final int index = getArrayPos(zoomToFind, zoomArray, false);
        return (index != -1 && index > 0) ? zoomArray[index - 1] : zoomArray[zoomArray.length - 1];
    }

    public static int getArrayPos(double curZoom, double[] zoomArray, boolean exact) {
        int pos = -1;
        for (int i = 0, len = (zoomArray != null) ? zoomArray.length : 0; i < len && pos == -1; ++i) {
            if (Math.abs(curZoom - zoomArray[i]) < EPS || (!exact && curZoom < zoomArray[i]))
                pos = i;
        }
        return pos;
    }

    public boolean isFitMode() {
        return this.equals(ZOOM_FIT_PAGE) || this.equals(ZOOM_FIT_WIDTH) || this.equals(ZOOM_FIT_HEIGHT);
    }

    public boolean isDirectMode() {
        return this.equals(ZOOM_DIRECT);
    }

    private final static double EPS = 0.0001;
}
