/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;

import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Utils;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * TextMarkupAnnotationEditor - allows to edit PDFL.TextMarkupAnnotation.
 * 
 * It sets guide rectangles' positions when annotation was selected. This class
 * also draws markup annotation on the page.
 */
public class TextMarkupAnnotationEditor extends BaseAnnotationEditor {
    public TextMarkupAnnotationEditor() {
        verticalLines = new ArrayList<Quad>();
        horizontalIndexes = new ArrayList<Integer>();
        startIndex = -1;
        endIndex = -1;

        setSelectionColor(Color.YELLOW);
    }

    @Override
    protected void onCapture() {
        super.onCapture();
        cachedQuads = Collections.unmodifiableList(getInteractiveContext().getApplication().getActiveDocument().getPageQuads(getHolder().getPageIndex()));

        Quad lastQuad = cachedQuads.isEmpty() ? new Quad() : cachedQuads.get(0);
        for (int i = 1, len = cachedQuads.size(); i <= len; ++i) { // iterate one extra time to add the last line
            Quad quad = (i != len) ? cachedQuads.get(i) : new Quad();
            if (!pointInQuad(lastQuad, Utils.rectCenter(quad.toRect()), false, true)) {
                verticalLines.add(lastQuad);
                horizontalIndexes.add(i - 1);
                lastQuad = quad;
            }
        }

        startIndex = findQuadStrict(getProperties().getQuads().get(0));
        endIndex = findQuadStrict(getProperties().getQuads().get(getProperties().getQuads().size() - 1));
    }

    @Override
    protected void onRelease() {
        verticalLines.clear();
        horizontalIndexes.clear();
        cachedQuads = null;

        startIndex = -1;
        endIndex = -1;
        super.onRelease();
    }

    @Override
    protected void doTransform(GuideRectangle guide, boolean moveAll, Matrix matrix) {
        if (!moveAll) {
            Point location = Utils.clone(guide.getLocation());
            location = location.transform(matrix);
            final boolean startGuide = guide.getLayout() == getStartGuide();

            final int num = startGuide ? findStartQuadClose(location) : findEndQuadClose(location);
            if (hasValidSelection()) {
                if (startGuide)
                    startIndex = num;
                else
                    endIndex = num;
            } else {
                startIndex = endIndex = num;
            }

            if (hasValidSelection()) {
                getProperties().setQuads(cachedQuads.subList(Math.min(startIndex, endIndex), Math.max(startIndex, endIndex) + 1));
            }
        }
    }

    @Override
    protected void doConfigureGuides(Map<Integer, GuideRectangle> guides, Rect r) {
        {
            final int layout = getStartGuide();
            final Quad quad = getProperties().getQuads().get(0);
            final Point guideLocation = new Point(quad.getTopLeft().getH(), (quad.getTopLeft().getV() + quad.getBottomLeft().getV()) / 2.0);
            guides.put(layout, new GuideRectangle(guideLocation, layout, PDF.Cursor.TEXT));
        }
        {
            final int layout = getEndGuide();
            final Quad quad = getProperties().getQuads().get(getProperties().getQuads().size() - 1);
            final Point guideLocation = new Point(quad.getBottomRight().getH(), (quad.getTopRight().getV() + quad.getBottomRight().getV()) / 2.0);
            guides.put(layout, new GuideRectangle(guideLocation, layout, PDF.Cursor.TEXT));
        }
    }

    @Override
    protected void doDrawShape(Graphics2D gr) {
        gr.setColor(selectionColor);
        for (Quad quad : getProperties().getQuads()) {
            final Rectangle quadRect = getPageModel().transform(quad.toRect());
            gr.fillRect(quadRect.x, quadRect.y, quadRect.width, quadRect.height);
        }
    }

    @Override
    protected GuideRectangle reflectGuide(Map<Integer, GuideRectangle> guides, GuideRectangle guide, double x0, double y0, double x1, double y1) {
        // indexes were calculated in previous iteration of doTransform() and need to be changed here
        if (startIndex > endIndex) {
            final boolean startGuide = guide.getLayout() == getStartGuide();
            final int layout = startGuide ? getEndGuide() : getStartGuide();
            guide = guides.get(layout);

            int tmp = startIndex;
            startIndex = endIndex;
            endIndex = tmp;
        }
        return guide;
    }

    List<Quad> getCachedQuads() {
        return cachedQuads;
    }

    void setSelectionColor(Color color) {
        this.selectionColor = new Color(color.getRed(), color.getGreen(), color.getBlue(), 128);
    }

    private boolean hasValidSelection() {
        return startIndex != -1 && endIndex != -1;
    }

    private int findQuadStrict(Quad quad) {
        return findQuadStrict(Utils.rectCenter(quad.toRect()));
    }

    private int findQuadStrict(Point location) {
        int quadNum = -1;
        final int line = getQuadNum(verticalLines, location, new SearchCondition() {
            public boolean findQuad(List<Quad> quads, int index, Point location) {
                return pointInQuad(quads.get(index), location, false, true);
            }
        });

        if (line != -1) {
            quadNum = getQuadByLine(line);
            final int horz = getQuadNum(getQuads(line, quadNum), location, new SearchCondition() {
                public boolean findQuad(List<Quad> quads, int index, Point location) {
                    return pointInQuad(quads.get(index), location, true, false);
                }
            });
            quadNum = (horz == -1) ? -1 : quadNum + horz;
        }
        return quadNum;
    }

    private int findStartQuadClose(Point location) {
        int quadNum = -1;
        final int line = getQuadNum(verticalLines, location, new SearchCondition() {
            public boolean findQuad(List<Quad> quads, int index, Point location) {
                return location.getV() > Utils.rectCenter(quads.get(index).toRect()).getV();
            }
        });

        if (line != -1) {
            quadNum = getQuadByLine(line);
            int horz = getQuadNum(getQuads(line, quadNum), location, new SearchCondition() {
                public boolean findQuad(List<Quad> quads, int index, Point location) {
                    return (pointInQuad(quads.get(index), location, true, false) || pointInQuad(getBetweenQuadSpace(quads, index), location, true, false) || (location.getH() < quads.get(index).getBottomLeft().getH()));
                }
            });
            quadNum = (horz == -1) ? getQuadByLine(line + 1) : quadNum + horz;
        }
        return quadNum;
    }

    private int findEndQuadClose(Point location) {
        int quadNum = -1;
        final int line = getQuadNum(verticalLines, location, new SearchCondition() {
            public boolean findQuad(List<Quad> quads, int index, Point location) {
                return index == (quads.size() - 1) || location.getV() > Utils.rectCenter(quads.get(index).toRect()).getV();
            }
        });

        if (line != -1) {
            quadNum = getQuadByLine(line);
            quadNum += getQuadNum(getQuads(line, quadNum), location, new SearchCondition() {
                public boolean findQuad(List<Quad> quads, int index, Point location) {
                    return (pointInQuad(quads.get(index), location, true, false) || pointInQuad(getBetweenQuadSpace(quads, index), location, true, false) || (index == (quads.size() - 1) && location.getH() > quads.get(index).getBottomRight().getH()));
                }
            });
        }
        return quadNum;
    }

    private int getQuadNum(List<Quad> quads, Point location, SearchCondition search) {
        int quadNum = -1;
        for (int i = 0, len = quads.size(); i < len && quadNum == -1; ++i) {
            if (search.findQuad(quads, i, location)) {
                quadNum = i;
            }
        }
        return quadNum;
    }

    private int getQuadByLine(int line) {
        return line > 0 ? horizontalIndexes.get(line - 1) + 1 : 0;
    }

    private List<Quad> getQuads(int line, int posFrom) {
        return cachedQuads.subList(posFrom, horizontalIndexes.get(line) + 1);
    }

    private boolean pointInQuad(Quad quad, Point location, boolean useHorz, boolean useVert) {
        final Rect rect = quad.toRect();
        Point loc = Utils.clone(location);
        if (!useHorz)
            loc.setH((rect.getLeft() + rect.getRight()) / 2.0);
        if (!useVert)
            loc.setV((rect.getTop() + rect.getBottom()) / 2.0);
        return Utils.pointInRect(loc, rect);
    }

    private Quad getBetweenQuadSpace(List<Quad> quads, int index) {
        Quad quad = new Quad();
        if (index < quads.size() - 1) {
            Quad prev = quads.get(index);
            Quad next = quads.get(index + 1);
            quad = new Quad(prev.getTopRight(), next.getTopLeft(), prev.getBottomRight(), next.getBottomLeft());
        }
        return quad;
    }

    private int getStartGuide() {
        return GuideRectangle.Layout.custom(START_LINE_GUIDE);
    }

    private int getEndGuide() {
        return GuideRectangle.Layout.custom(END_LINE_GUIDE);
    }

    private interface SearchCondition {
        public boolean findQuad(List<Quad> quads, int index, Point location);
    }

    private List<Quad> cachedQuads;

    // arrays are synchronized by indexes
    final private List<Quad> verticalLines;
    final private List<Integer> horizontalIndexes;

    private int startIndex;
    private int endIndex;
    private Color selectionColor;
}
