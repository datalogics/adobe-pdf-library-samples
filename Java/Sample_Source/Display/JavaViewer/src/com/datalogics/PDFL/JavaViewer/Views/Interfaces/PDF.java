/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

import java.awt.Dimension;
import java.awt.Point;
import java.awt.Rectangle;

public interface PDF {
    public static interface Scrolling {
        void setScrollVisible(boolean visible);

        void setCurrent(int value);

        int getCurrent();

        void setScrollValues(int value, int extent, int min, int max);

        boolean inProgress();
    }

    public static interface ToolTip {
        public void cancel();
    }

    public static enum Cursor {
        DEFAULT,
        CROSS,
        HAND,
        TEXT,
        MOVE,
        SIZE_N,
        SIZE_NE,
        SIZE_E,
        SIZE_SE,
        SIZE_S,
        SIZE_SW,
        SIZE_W,
        SIZE_NW;
    }

    Scrolling getScroll(boolean vertical);

    Dimension getVisibleViewSize();

    void setViewSize(Dimension preferredSize);

    Point getViewOrigin();

    void setViewOrigin(Point origin);

    void repaintView();

    Cursor getViewCursor();

    void setViewCursor(Cursor cursor);

    TextEdit createTextEdit(Rectangle textRect);

    ToolTip showToolTip(String message, Point location);
}
