/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Graphics2D;
import java.awt.Point;
import java.util.Map;

import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.Cache.PageModel;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * LineAnnotationEditor - allows to edit PDFL.LineAnnotation.
 * 
 * It draws PDFL.LineAnnotation on the page.
 */
public class LineAnnotationEditor extends BaseAnnotationEditor {
    @Override
    protected void doTransform(GuideRectangle guide, boolean moveAll, Matrix matrix) {
        final int layout = moveAll ? GuideRectangle.Layout.NOTHING : guide.getLayout();
        if (moveAll || layout == getStartGuide()) {
            getProperties().setStartPoint(ensurePointInPage(getProperties().getStartPoint().transform(matrix)));
        }
        if (moveAll || layout == getEndGuide()) {
            getProperties().setEndPoint(ensurePointInPage(getProperties().getEndPoint().transform(matrix)));
        }
    }

    @Override
    protected void doConfigureGuides(Map<Integer, GuideRectangle> guides, Rect r) {
        {
            final int layout = getStartGuide();
            guides.put(layout, new GuideRectangle(getProperties().getStartPoint(), layout, PDF.Cursor.MOVE));
        }
        {
            final int layout = getEndGuide();
            guides.put(layout, new GuideRectangle(getProperties().getEndPoint(), layout, PDF.Cursor.MOVE));
        }
    }

    @Override
    protected void doDrawShape(Graphics2D gr) {
        final PageModel pageModel = getPageModel();
        final AnnotationProperties annotationProperties = getProperties();

        final Point startPoint = pageModel.transform(annotationProperties.getStartPoint());
        final Point endPoint = pageModel.transform(annotationProperties.getEndPoint());

        gr.setColor(annotationProperties.getForeColor());
        gr.drawLine(startPoint.x, startPoint.y, endPoint.x, endPoint.y);
    }

    private int getStartGuide() {
        return GuideRectangle.Layout.custom(START_LINE_GUIDE);
    }

    private int getEndGuide() {
        return GuideRectangle.Layout.custom(END_LINE_GUIDE);
    }
}
