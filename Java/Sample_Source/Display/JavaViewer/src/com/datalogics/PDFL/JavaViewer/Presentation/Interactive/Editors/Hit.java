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

/**
 * Hit - determines set of flags to check mouse hit.
 */
public class Hit {
    public static class Flags {
        public static final int NO_HIT = 0;
        public static final int SHAPE_HIT = 1;
        public static final int RECT_HIT = 2;
        public static final int BORDER_HIT = 4;
        public static final int GUIDE_HIT = 8;
    }

    Hit() {
        flags = Flags.NO_HIT;
        guide = null;
        location = new Point(Double.MIN_VALUE, Double.MIN_VALUE);
    }

    void includeHit(int flag) {
        this.flags |= flag;
    }

    void setGuide(GuideRectangle guide) {
        this.guide = guide;
    }

    void setLocation(Point location) {
        this.location = Utils.clone(location);
    }

    public boolean noHit() {
        return this.flags == Flags.NO_HIT;
    }

    public boolean hasHit() {
        return !noHit();
    }

    public boolean containsHit(int flag) {
        return (this.flags & flag) != 0;
    }

    public GuideRectangle getGuide() {
        return this.guide;
    }

    public Point getLocation() {
        return Utils.clone(this.location);
    }

    private int flags;
    private GuideRectangle guide;
    private Point location;
}
