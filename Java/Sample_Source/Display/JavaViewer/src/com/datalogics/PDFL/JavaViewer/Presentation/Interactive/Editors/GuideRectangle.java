/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.JavaViewer.Utils;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * GuideRectangle - an abstraction of sizing handle to perform manipulations
 * with annotations.
 * 
 * It stores current position, cursor type and layout type of guide rectangle.
 */
public class GuideRectangle {
    static class Layout {
        static final int NOTHING = 0;
        static final int N = 1;
        static final int E = 2;
        static final int S = 4;
        static final int W = 8;
        static final int WE = E | W;
        static final int NE = N | E;
        static final int NW = N | W;
        static final int NS = N | S;
        static final int SE = S | E;
        static final int SW = S | W;
        static final int MOVE = N | E | S | W;

        static int reflect(int layout, boolean vertical) {
            if (!isCustom(layout)) {
                int revert = (vertical) ? WE : NS;
                if ((layout & revert) == 0) // nothing to revert
                    revert = 0;

                layout = rotate(layout, 180);
                layout ^= revert;
            }
            return layout;
        }

        static int rotate(int layout, int angle) {
            if (!isCustom(layout)) {
                final int bits = ((360 - angle) % 360) / 90;
                layout <<= bits;
                final int cyclicPart = (layout & 0xF0) >> 4;

                layout &= 0x0F;
                layout |= cyclicPart;
            }
            return layout;
        }

        static boolean isCustom(int layout) {
            return layout >= MOVE;
        }

        static int custom(int index) {
            return MOVE + (index << 4);
        }
    }

    GuideRectangle(Point location, int layout, PDF.Cursor cursor) {
        this.location = Utils.clone(location);
        this.layout = layout;
        this.cursor = cursor;
    }

    Point getLocation() {
        return Utils.clone(location);
    }

    int getLayout() {
        return layout;
    }

    PDF.Cursor getCursor() {
        return cursor;
    }

    private Point location;
    private int layout;
    private PDF.Cursor cursor;
}
