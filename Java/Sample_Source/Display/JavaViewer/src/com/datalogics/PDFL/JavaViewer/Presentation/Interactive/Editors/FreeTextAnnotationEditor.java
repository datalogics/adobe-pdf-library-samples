/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Font;
import java.awt.FontMetrics;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.Cache.PageModel;

/**
 * FreeTextAnnotationEditor - allows to create PDFL.FreeTextAnnotation.
 * 
 * It sets the text parameters such as font type, font size, alignment etc.
 */
public class FreeTextAnnotationEditor extends BaseAnnotationEditor {
    @Override
    protected void doDrawShape(Graphics2D gr) {
        final PageModel pageModel = getPageModel();
        final AnnotationProperties annotationProperties = getProperties();
        Rectangle bound = pageModel.transform(annotationProperties.getBoundingRect());
        bound.grow(-GUIDE_SIZE, 0); // make indent by guide half-size from the
                                    // border

        final String contents = annotationProperties.getContents();
        final int textSize = (int) (annotationProperties.getFontSize() * getPageModel().getScale());
        final Font font = new Font(annotationProperties.getFontFace(), Font.PLAIN, textSize);
        final FontMetrics fontMetrics = gr.getFontMetrics(font);
        final HorizontalAlignment horzAlignment = annotationProperties.getQuadding();

        if (annotationProperties.hasFill()) {
            gr.setColor(annotationProperties.getInteriorColor());
            gr.fillRect(bound.x, bound.y, bound.width, bound.height);
        }

        gr.setFont(font);
        gr.setColor(annotationProperties.getTextColor());

        String[] stringList = contents.split("[\r\n]|\r\n");
        stringList = splitString(stringList, fontMetrics, bound.width, true);

        int yCoord = bound.y;
        for (String s : stringList) {
            int xCoord = 0;
            int lineWidth = fontMetrics.stringWidth(s);
            if (horzAlignment == HorizontalAlignment.LEFT)
                xCoord = bound.x;
            else if (horzAlignment == HorizontalAlignment.CENTER)
                xCoord = (bound.x + (bound.width / 2)) - lineWidth / 2;
            else if (horzAlignment == HorizontalAlignment.RIGHT)
                xCoord = (bound.x + bound.width) - lineWidth;

            yCoord += fontMetrics.getHeight();
            if (yCoord > bound.y + bound.height)
                break;

            gr.drawString(s, xCoord, yCoord);
        }
    }

    private String[] splitString(String[] splitString, FontMetrics fontMetrics, int boundWidth, boolean wrap) {
        List<String> stringList = new ArrayList<String>();
        for (String s : splitString) {
            boolean newSplitPart = true;
            int currSymbol = 0, accumulatedLen = 0;
            while (currSymbol < s.length()) { // break string into parts of needed length
                accumulatedLen += fontMetrics.charWidth(s.charAt(currSymbol));
                if (accumulatedLen > boundWidth) {
                    if (currSymbol != 0)
                        stringList.add(s.substring(0, currSymbol));
                    else
                        // if even one symbol can't be drawn within current bounds, just skip it
                        currSymbol = 1;

                    // prepare conditions for the next splitting iteration
                    s = wrap ? s.substring(currSymbol) : ""; // don't use the part of string that left if no wrap
                    currSymbol = accumulatedLen = 0;
                    newSplitPart = false;
                } else {
                    ++currSymbol;
                }
            }
            if (newSplitPart || s.length() != 0) // add the last piece which left or the whole new split part (even if it's empty)
                stringList.add(s);
        }
        return stringList.toArray(new String[0]);
    }
}
