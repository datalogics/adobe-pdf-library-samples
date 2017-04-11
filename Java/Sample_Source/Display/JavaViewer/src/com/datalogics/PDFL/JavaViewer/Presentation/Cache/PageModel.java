/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Cache;

import java.awt.Rectangle;
import java.awt.geom.Point2D;

import com.datalogics.PDFL.Rect;

/**
 * PageModel - exposes page model representation with appropriate methods for
 * transformations.
 */
public interface PageModel {
    /**
     * Page parameters
     */
    Rectangle getBounds();

    double getScale();

    double getRotation();

    /**
     * pdf-to-view transformations
     */
    Rectangle transform(Rect rectPdf);

    java.awt.Point transform(com.datalogics.PDFL.Point pointPdf);

    /**
     * View-to-pdf transformations
     */
    Rect transform(Rectangle rectView);

    com.datalogics.PDFL.Point transform(java.awt.Point pointView);

    /**
     * Relative point on page
     */
    Point2D.Float relative(java.awt.Point pointView);

    Point2D.Float relative(com.datalogics.PDFL.Point pointPdf);

    java.awt.Point relative(Point2D.Float pointView);

    com.datalogics.PDFL.Point relativePdf(Point2D.Float pointView);
}
