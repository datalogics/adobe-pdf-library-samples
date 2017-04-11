/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Cache;

import java.awt.Dimension;
import java.awt.Graphics2D;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.geom.Area;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import com.datalogics.PDFL.Document;

/**
 * DocumentCahce - responsible for the cache of all pages.
 * 
 * This class adds new pages to the cache and clears it if necessary. It also
 * has a couple of service methods for drawing pages, setting dpi etc.
 */
public class DocumentCache {
    public DocumentCache() {
        this.dpiCorrection = 1 / pdfDpi;

        this.pageIndent = new Dimension();
        this.pageCache = new ArrayList<PageCache>();
        createPages();
    }

    public void setDpi(int outputDpi) {
        if (outputDpi != getDpi()) {
            double[] pageZooms = new double[getPageCount()];
            for (int i = 0; i < getPageCount(); ++i) {
                pageZooms[i] = getZoom(i);
            }

            this.dpiCorrection = (double) outputDpi / pdfDpi;
            setZoom(pageZooms);
        }
    }

    public int getDpi() {
        return (int) Math.round(dpiCorrection * pdfDpi);
    }

    public void draw(Graphics2D g, Rectangle updRect, Set<Integer> pages) {
        if (pdfDoc != null) {
            Area nonPaintArea = new Area(updRect);
            for (Integer pageNum : pages) {
                try {
                    cachePage(pageNum);
                    final Rectangle occupiedRect = pageCache.get(pageNum).draw(g, updRect);
                    nonPaintArea.subtract(new Area(occupiedRect));
                } catch (IndexOutOfBoundsException e) {
                } catch (OutOfMemoryError e) {
                    if (flushCache(pageNum)) {
                        draw(g, updRect, pages);
                        return;
                    }
                }
            }
            g.setColor(java.awt.SystemColor.control);
            g.fill(nonPaintArea);
        }
    }

    public Dimension getPageIndent() {
        return this.pageIndent;
    }

    public void setPageIndent(Dimension indent) {
        this.pageIndent = new Dimension(indent);
        updatePageOffsets();
    }

    public PageModel getPageModel(int index) {
        return isPageValid(index) ? pageCache.get(index) : null;
    }

    public boolean isPageValid(int index) {
        return index >= 0 && index < getPageCount();
    }

    public Rectangle getPageBounds(int index, boolean withIndent) {
        Rectangle pageBounds = null;
        try {
            pageBounds = pageCache.get(index).getBounds();

            if (withIndent)
                pageBounds.grow(pageIndent.width, pageIndent.height);
        } catch (IndexOutOfBoundsException e) {
            pageBounds = new Rectangle();
        }
        return pageBounds;
    }

    public Dimension getDocumentSize() {
        return documentSize;
    }

    public Set<Integer> pagesInRect(Rectangle rect) {
        HashSet<Integer> pages = new HashSet<Integer>();
        for (int i = 0, len = pageCache.size(); i < len; ++i) {
            final Rectangle pageBounds = getPageBounds(i, false);
            if (Math.round(pageBounds.getMaxY()) < rect.y) // page is before the rectangle, skip it
                continue;
            else if (pageBounds.y > Math.round(rect.getMaxY())) // page is ahead of rectangle, no need to continue
                break;

            if (pageBounds.intersects(rect))
                pages.add(i);
        }
        return pages;
    }

    public void setPDF(Document pdfDoc) {
        this.pdfDoc = pdfDoc;
        createPages();
    }

    public void setZoom(double zoom) {
        createPages(zoom);
    }

    public void setZoom(double[] pageZooms) {
        for (int i = 0, len = Math.min(pageZooms.length, pageCache.size()); i < len; ++i) {
            pageCache.remove(i);
            pageCache.add(i, new PageCache(pdfDoc.getPage(i), getFullZoom(pageZooms[i]), pdfDoc.getOptionalContentContext()));
        }
        updatePageOffsets();
    }

    public double getZoom(int index) {
        double pageZoom = 0.0;
        try {
            pageZoom = pageCache.get(index).getScale() / dpiCorrection;
        } catch (IndexOutOfBoundsException e) {
        }
        return pageZoom;
    }

    public int getPageCount() {
        return this.pageCache.size();
    }

    public void invalidate() {
        for (PageCache page : pageCache) {
            page.invalidate(false);
        }
        System.gc();
    }

    public void invalidate(int index, Rectangle changedArea) {
        try {
            pageCache.get(index).invalidate(changedArea);
        } catch (IndexOutOfBoundsException e) {
        }
    }

    private void createPages() {
        createPages(0.0);
    }

    private void createPages(double zoom) {
        final int pageCount = (pdfDoc != null) ? pdfDoc.getNumPages() : 0;
        pageCache.clear();
        for (int i = 0; i < pageCount; ++i) {
            pageCache.add(new PageCache(pdfDoc.getPage(i), getFullZoom(zoom), pdfDoc.getOptionalContentContext()));
        }
        updatePageOffsets();
    }

    private int calcDocumentWidth() {
        return pageCache.isEmpty() ? 0 : Collections.max(pageCache, new Comparator<PageCache>() {
            public int compare(PageCache page1, PageCache page2) {
                final int widthDelta = page1.getPageSize().width - page2.getPageSize().width;
                return (widthDelta == 0) ? 0 : widthDelta / Math.abs(widthDelta);
            }
        }).getPageSize().width + pageIndent.width * 2;
    }

    private int calcDocumentHeight() {
        return pageCache.isEmpty() ? 0 : (int) pageCache.get(pageCache.size() - 1).getBounds().getMaxY() + pageIndent.height;
    }

    private void updatePageOffsets() {
        Point currentOffset = new Point(pageIndent.width, pageIndent.height);
        for (PageCache page : pageCache) {
            page.setOffset(currentOffset);

            currentOffset.y += page.getPageSize().height;
            currentOffset.y += pageIndent.height;
        }
        documentSize = new Dimension(calcDocumentWidth(), calcDocumentHeight());
    }

    private void cachePage(int index) {
        try {
            if ((Math.min(MAX_HEAP_SIZE, Runtime.getRuntime().maxMemory()) - Runtime.getRuntime().totalMemory()) <= MIN_FREE_SPACE) {
                flushCache(index);
            }
            PageCache page = pageCache.get(index);
            if (!page.isValid())
                page.cache();

        } catch (IndexOutOfBoundsException e) {
        }
    }

    private boolean flushCache(int indexNeedsMemory) {
        boolean memoryFreed = false;
        boolean canFlushOthers = false;
        for (int i = 0, len = pageCache.size(); i < len; ++i) {
            if (i == indexNeedsMemory)
                continue;

            if (canFlushOthers = canFlushOthers || pageCache.get(i).isValid())
                break;
        }

        if (canFlushOthers) {
            for (int i = 0, len = pageCache.size(); i < len; ++i) {
                if (i == indexNeedsMemory)
                    continue;

                pageCache.get(i).invalidate(false);
            }
            System.gc();
            memoryFreed = true;
        } else {

            pageCache.get(indexNeedsMemory).invalidate(true);
            memoryFreed = true;
        }
        return memoryFreed;
    }

    private double getFullZoom(double locZoom) {
        return locZoom * dpiCorrection;
    }

    private final static int pdfDpi = 72;
    private final int MAX_HEAP_SIZE = 128 << 20; // max available memory is 128Mb
    private final int MIN_FREE_SPACE = 10 << 20;

    private Document pdfDoc;
    private double dpiCorrection;
    private final List<PageCache> pageCache;
    private Dimension pageIndent;
    private Dimension documentSize;
}
