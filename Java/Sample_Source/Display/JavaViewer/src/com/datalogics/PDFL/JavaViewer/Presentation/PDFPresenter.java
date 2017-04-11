/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation;

import java.awt.Dimension;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.geom.Point2D;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.NoSuchElementException;
import java.util.Set;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Document.DocumentListener;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Presentation.Cache.DocumentCache;
import com.datalogics.PDFL.JavaViewer.Presentation.Cache.PageModel;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.PageViewMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * PDFPresenter - works with presentation of the PDF document.
 * 
 * It reacts on change of active page or scroll position or some other
 * document's visible characteristics. This class manages caching of the
 * document.
 * 
 * It allows setting paging mode, zoom, scroll and other parameters that affect
 * representation of the document.
 */
public class PDFPresenter implements DocumentListener {
    public static interface ViewListener {
        void viewUpdated(boolean changed);
    }

    public static interface ViewShot {
        double getZoom();

        int getPageNum();

        ZoomMode getZoomMode();

        Point2D.Float getLocation();
    }

    PDFPresenter(Application application) {
        this.application = application;
        this.listeners = new ArrayList<ViewListener>();

        this.document = null;
        this.docCache = new DocumentCache();

        this.activePage = -1;
        this.zoomMode = ZoomMode.ZOOM_NONE;
        this.viewMode = PageViewMode.PAGE_MODE_NONE;

        this.minZoom = 0.01;
        this.maxZoom = 1;

        this.relativeViewLocation = new Point2D.Float();
        this.topViewPage = -1;
    }

    void setView(PDF view) {
        this.pdfView = view;
        updateViewLocation(new Point(), true);
    }

    void activeDocumentChanged() {
        document = application.getActiveDocument();
    }

    void setDpi(int dpi) {
        docCache.setDpi(dpi);

        if (getZoomMode().isFitMode())
            recalculateViewInFitMode();
        else
            pdfView.repaintView();
    }

    int getDpi() {
        return docCache.getDpi();
    }

    void drawPDF(Graphics g, Rectangle updRect) {
        if (hasValidDocument()) {
            Set<Integer> pages = null;
            if (viewMode.isSinglePage()) {
                pages = new HashSet<Integer>();
                pages.add(activePage);
            } else {
                pages = docCache.pagesInRect(getVisibleViewRect());
            }
            docCache.draw((Graphics2D) g, updRect, pages);
        }
    }

    public void addListener(ViewListener listener) {
        listeners.add(listener);
    }

    public void removeListener(ViewListener listener) {
        listeners.remove(listener);
    }

    public int getPageByPoint(Point point) {
        final Set<Integer> pages = docCache.pagesInRect(new Rectangle(point.x, point.y, 1, 1));
        return !pages.isEmpty() ? pages.iterator().next() : -1;
    }

    public void visibleViewSized() {
        if (hasValidDocument()) {
            if (this.zoomMode.isFitMode())
                recalculateViewInFitMode();
            else
                updateViewLocation(getViewLocation(), true);

            onViewUpdate(true);
        }
    }

    /**
     * Method is called when scroll value has been changed. This method
     * calculate new scroll position considering view mode, page size and
     * document view port size. It changed active page if necessary.
     * 
     * @param vertical - this parameter is defined what scroll need to change:
     * vertical or horizontal.
     */
    public void scrollChanged(boolean vertical) {
        final PDF.Scrolling scroll = pdfView.getScroll(vertical);
        Point newLocation = new Point(getViewLocation());

        if (vertical)
            newLocation.y = scroll.getCurrent();
        else
            newLocation.x = scroll.getCurrent();

        if (!hasValidDocument() || newLocation.equals(getViewLocation()))
            return;

        final Rectangle visibleRect = getVisibleViewRect(newLocation);
        Set<Integer> pages = docCache.pagesInRect(visibleRect);
        int visiblePage = -1, newActivePage = -1;

        if (viewMode.isSinglePage()) {
            // determine new active page
            newActivePage = activePage;
            final int prevPage = activePage - 1;
            final int nextPage = activePage + 1;
            if (pages.contains(activePage)) {
                if (pages.contains(prevPage))
                    newActivePage = prevPage;
                else if (pages.contains(nextPage) && (!pageHeightInView(activePage) || docCache.getPageBounds(activePage, true).y < visibleRect.y))
                    newActivePage = nextPage;
            } else if (!pages.isEmpty())
                newActivePage = Collections.min(pages);

            // calculate new location of visible view
            if (newActivePage != activePage) {
                Rectangle newPageBounds = docCache.getPageBounds(newActivePage, true);
                if (newActivePage == prevPage && !pageHeightInView(newActivePage)) {
                    newLocation = new Point(newPageBounds.x, (int) Math.round(newPageBounds.getMaxY() - visibleRect.getHeight()));
                } else {
                    newLocation = newPageBounds.getLocation();
                }
            }
            visiblePage = newActivePage;

            if (newActivePage != activePage && scroll.inProgress()) {
                // prohibit page change when scroll is captured, as it causes
                // view stuttering
                return;
            }
        } else {
            if (pages.isEmpty()) {
                visiblePage = topViewPage;
            } else {
                final int visibleCenterY = (int) Math.round(visibleRect.getCenterY());
                visiblePage = Collections.min(pages);

                final Rectangle rectHalfTop = new Rectangle(visibleRect);
                rectHalfTop.height /= 2;
                int lastTop = visiblePage;
                for (Integer index : pages) {
                    if (docCache.getPageBounds(index, false).intersects(rectHalfTop) && index > lastTop)
                        lastTop = index;
                }

                final Rectangle rectHalfBottom = new Rectangle(visibleRect);
                rectHalfBottom.y = visibleCenterY;
                rectHalfBottom.height /= 2;
                int firstBottom = Collections.max(pages);
                for (Integer index : pages) {
                    if (docCache.getPageBounds(index, false).intersects(rectHalfBottom) && index < firstBottom)
                        firstBottom = index;
                }

                if (lastTop == firstBottom)
                    newActivePage = lastTop;
                else {
                    newActivePage = ((visibleCenterY - Math.round(docCache.getPageBounds(lastTop, false).getMaxY())) < (docCache.getPageBounds(firstBottom, false).y - visibleCenterY)) ? lastTop : firstBottom;
                }
            }
        }
        goToPositionAbsolute(visiblePage, newLocation);
        setActivePage(newActivePage);
    }

    public void pdfChanged(Document pdfDoc) {
        // save all the view parameters
        ZoomMode lastZoomMode = getZoomMode();
        double lastZoom = getZoom();
        PageViewMode lastViewMode = getViewMode();
        int lastActivePage = getActivePage();
        int lastTopPage = topViewPage;
        Point2D.Float lastViewLocation = (Point2D.Float) relativeViewLocation.clone();

        docCache.setPDF(pdfDoc);
        docCache.setPageIndent(hasValidDocument() ? new Dimension(10, 10) : new Dimension());

        // update zoom
        setZoom(0.0);
        this.zoomMode = ZoomMode.ZOOM_NONE;
        if (hasValidDocument()) {
            if (lastZoomMode == ZoomMode.ZOOM_NONE)
                lastZoomMode = ZoomMode.ZOOM_FIT_PAGE;
            if (lastZoom == 0)
                lastZoom = 1.0;

            setZoomMode(lastZoomMode);
            if (!lastZoomMode.isFitMode())
                setZoom(lastZoom);
        }

        // update view mode
        this.viewMode = PageViewMode.PAGE_MODE_NONE;
        if (hasValidDocument()) {
            if (lastViewMode == PageViewMode.PAGE_MODE_NONE)
                lastViewMode = PageViewMode.PAGE_MODE_SINGLE;

            setViewMode(lastViewMode);
        }

        // update active page
        this.activePage = this.topViewPage = -1;
        if (hasValidDocument()) {
            if (!docCache.isPageValid(lastActivePage))
                lastActivePage = 0;
            if (!docCache.isPageValid(lastTopPage))
                lastTopPage = 0;
        } else {
            lastActivePage = lastTopPage = -1;
        }
        updateViewSize();
        if (lastTopPage != -1)
            goToPositionAbsolute(lastTopPage, toAbsoluteLocation(lastTopPage, lastViewLocation));

        // force update when document is closed
        if (!hasValidDocument())
            onViewUpdate(true);
    }

    public void permissionChanged() {
    }

    public void pdfLayerUpdated(List<OptionalContentGroup> ocgList) {
        clearCache();
    }

    public void pageChanged(int index, Rect changedArea) {
        if (docCache.isPageValid(index)) {
            docCache.invalidate(index, docCache.getPageModel(index).transform(changedArea));
            pdfView.repaintView();
        }
    }

    public void selectionChanged(Object oldSelection) {
    }

    public void annotationUpdated(AnnotationHolder holder, boolean created) {
        clearCache();
    }

    public void annotationDeleted() {
        clearCache();
    }

    public void goToPosition(int newPage, Point2D.Float relLocation) {
        final Rectangle pageBounds = docCache.getPageBounds(newPage, true);
        Point absLocation = null; // location in document coordinates

        if (relLocation == LEFT_TOP_ALIGN) {
            absLocation = pageBounds.getLocation();
        } else if (relLocation == LEFT_BOTTOM_ALIGN) {
            absLocation = new Point(pageBounds.x, (int) Math.round(pageBounds.getMaxY() - getVisibleViewRect().getHeight()));
        } else if (relLocation != null) {
            absLocation = toAbsoluteLocation(newPage, relLocation);
            // since the location is calculated with regard to page indents, we
            // need to correct that location by indents' values
            final Dimension pageIndent = docCache.getPageIndent();
            absLocation.translate(pageIndent.width, pageIndent.height);
        }

        if (absLocation != null)
            goToPositionAbsolute(newPage, absLocation);
    }

    public void goToPositionAbsolute(int visiblePage, Point absLocation) {
        boolean changed = false;
        if (docCache.isPageValid(visiblePage)) {
            changed = true;
            boolean updateLocation = true;

            if (topViewPage != visiblePage || activePage != visiblePage) {
                final int oldPage = topViewPage;

                topViewPage = visiblePage;
                activePage = visiblePage;

                if (viewMode.isSinglePage()) {
                    updateLocation = false;
                    updateViewLocation(absLocation, true);

                    if (pageHeightInView(oldPage) || pageHeightInView(visiblePage))
                        pdfView.repaintView();
                }
            }

            if (updateLocation)
                updateViewLocation(absLocation, false);
        }
        onViewUpdate(changed);
    }

    public int getActivePage() {
        return this.activePage;
    }

    public void setActivePage(int page) {
        boolean changed = false;

        if (docCache.isPageValid(page) && page != activePage) {
            changed = true;
            if (viewMode.isSinglePage()) { // for single page mode perform
                                           // handling via goToPosition method
                goToPosition(page, LEFT_TOP_ALIGN);
                return;
            } else
                activePage = page;
        }
        onViewUpdate(changed);
    }

    public int getPageCount() {
        return docCache.getPageCount();
    }

    public void setViewMode(PageViewMode mode) {
        boolean changed = false;
        if (mode != null && mode != viewMode) {
            changed = true;

            viewMode = mode;
            updateViewLocation(getViewLocation(), true);
            pdfView.repaintView();
        }
        onViewUpdate(changed);
    }

    public PageViewMode getViewMode() {
        return viewMode;
    }

    public void setZoomBounds(double minZoom, double maxZoom) {
        if (minZoom <= maxZoom && minZoom > 0) {
            this.minZoom = minZoom;
            this.maxZoom = maxZoom;
        }
        onViewUpdate(false);
    }

    public void setZoom(double newZoom) {
        boolean changed = false;
        if (!this.zoomMode.isDirectMode()) {
            this.zoomMode = ZoomMode.ZOOM_DIRECT;
            docCache.setZoom(getZoom());
            changed = true;
        }

        newZoom = Math.max(Math.min(newZoom, maxZoom), minZoom);
        if (docCache.getZoom(activePage) != newZoom) {
            docCache.setZoom(newZoom);
            updateViewSize();
            changed = true;
        }
        onViewUpdate(changed);
    }

    public double getZoom() {
        return docCache.getZoom(activePage);
    }

    public double getZoom(int index) {
        return docCache.getZoom(index);
    }

    public void setZoomMode(ZoomMode newMode) {
        boolean changed = false;
        if (newMode != null && newMode.isFitMode() && this.zoomMode != newMode) {
            this.zoomMode = newMode;
            if (this.zoomMode.isFitMode())
                recalculateViewInFitMode();
            changed = true;
        }
        onViewUpdate(changed);
    }

    public ZoomMode getZoomMode() {
        return this.zoomMode;
    }

    public PageModel getPageModel(int index) {
        return docCache.getPageModel(index);
    }

    public ViewShot getViewShot() {
        final PDFPresenter pdfPresenter = this;

        return new ViewShot() {
            {
                pageNum = pdfPresenter.getActivePage();
                zoom = pdfPresenter.getZoom();
                zoomMode = pdfPresenter.getZoomMode();
                location = pdfPresenter.relativeViewLocation;
            }

            public int getPageNum() {
                return pageNum;
            }

            public double getZoom() {
                return zoom;
            }

            public ZoomMode getZoomMode() {
                return zoomMode;
            }

            public Point2D.Float getLocation() {
                return location;
            }

            private int pageNum;
            private double zoom;
            private ZoomMode zoomMode;
            private Point2D.Float location;
        };
    }

    public void setViewShot(ViewShot viewShot) {
        setZoom(viewShot.getZoom());
        setZoomMode(viewShot.getZoomMode());
        goToPositionAbsolute(viewShot.getPageNum(), toAbsoluteLocation(viewShot.getPageNum(), viewShot.getLocation()));
    }

    private void clearCache() {
        docCache.invalidate();
        pdfView.repaintView();
    }

    private void updateViewSize() {
        pdfView.setViewSize(docCache.getDocumentSize());
        updateViewLocation(getViewLocation(), true);
    }

    private void updateViewLocation(Point newLocation, boolean recalcScroll) {
        if (topViewPage != -1) {
            ensureCorrectViewLocation(newLocation);
        }

        final Point viewLocation = getViewLocation();
        pdfView.setViewOrigin(viewLocation);

        if (recalcScroll) {
            recalculateScrolling();
        } else {
            pdfView.getScroll(false).setCurrent(viewLocation.x);
            pdfView.getScroll(true).setCurrent(viewLocation.y);
        }
    }

    private void recalculateScrolling() {
        if (hasValidDocument()) {
            final Rectangle pageBounds = docCache.getPageBounds(activePage, true);
            final boolean singlePageMode = viewMode.isSinglePage();
            if (!singlePageMode || !pageBounds.isEmpty()) { // page must be
                                                            // valid for single
                                                            // page mode
                final Dimension visibleSize = pdfView.getVisibleViewSize();
                final Point viewLocation = getViewLocation();

                // vertical scrolling
                final PDF.Scrolling vertScroll = pdfView.getScroll(true);
                final Rectangle docBounds = new Rectangle(docCache.getDocumentSize());

                final int vertExtent = (singlePageMode && pageHeightInView(activePage)) ? pageBounds.height : visibleSize.height;
                vertScroll.setScrollValues(viewLocation.y, vertExtent, 0, docBounds.height);
                vertScroll.setScrollVisible((singlePageMode && docCache.getPageCount() != 1) || !rectHeightInView(docBounds));

                // horizontal scrolling
                final PDF.Scrolling horzScroll = pdfView.getScroll(false);
                final Rectangle measureBounds = (singlePageMode) ? pageBounds : docBounds;
                horzScroll.setScrollValues(viewLocation.x, visibleSize.width, 0, measureBounds.width);
                horzScroll.setScrollVisible(!rectWidthInView(measureBounds));
            }
        } else {
            pdfView.getScroll(false).setScrollVisible(false);
            pdfView.getScroll(true).setScrollVisible(false);
        }
    }

    private void recalculateViewInFitMode() {
        final Dimension visibleSize = pdfView.getVisibleViewSize();
        final Dimension pageIndent = docCache.getPageIndent();
        visibleSize.width -= pageIndent.width * 2;
        visibleSize.height -= pageIndent.height * 2;
        docCache.setZoom(1.0); // revert to normal zoom value

        double[] pageZooms = new double[docCache.getPageCount()];
        for (int i = 0; i < docCache.getPageCount(); ++i) {
            final Rectangle pageBounds = docCache.getPageBounds(i, false);
            final double xRatio = visibleSize.getWidth() / pageBounds.getWidth();
            final double yRatio = visibleSize.getHeight() / pageBounds.getHeight();
            double pageRatio = Math.min(xRatio, yRatio); // fit page by default

            if (zoomMode == ZoomMode.ZOOM_FIT_WIDTH)
                pageRatio = xRatio;
            else if (zoomMode == ZoomMode.ZOOM_FIT_HEIGHT)
                pageRatio = yRatio;
            pageZooms[i] = pageRatio;
        }
        docCache.setZoom(pageZooms);
        updateViewSize();
    }

    private void ensureCorrectViewLocation(Point oldLocation) {
        Point newLocation = new Point(oldLocation);
        final Rectangle boundsToCorrect = (viewMode.isSinglePage()) ? docCache.getPageBounds(activePage, true) : new Rectangle(docCache.getDocumentSize());
        final Rectangle visibleRect = getVisibleViewRect(oldLocation);
        Point deltaCorrection = new Point();

        if (rectWidthInView(boundsToCorrect))
            newLocation.x = boundsToCorrect.x;
        else
            deltaCorrection.x = Math.max((int) Math.round(visibleRect.getMaxX() - boundsToCorrect.getMaxX()), deltaCorrection.x);

        if (rectHeightInView(boundsToCorrect))
            newLocation.y = boundsToCorrect.y;
        else
            deltaCorrection.y = Math.max((int) Math.round(visibleRect.getMaxY() - boundsToCorrect.getMaxY()), deltaCorrection.y);

        newLocation.x = Math.max(newLocation.x - deltaCorrection.x, boundsToCorrect.x);
        newLocation.y = Math.max(newLocation.y - deltaCorrection.y, boundsToCorrect.y);

        if (!viewMode.isSinglePage() && !newLocation.equals(oldLocation)) { // we may select another relative page for view in the continuous mode
            try {
                topViewPage = Collections.min(docCache.pagesInRect(getVisibleViewRect(newLocation)));
            } catch (NoSuchElementException e) {
                // there might be no pages in the document
            }
        }
        relativeViewLocation = toRelativeLocation(topViewPage, newLocation);
    }

    private Rectangle getVisibleViewRect(Point location) {
        Rectangle visibleRect = new Rectangle(pdfView.getVisibleViewSize());
        visibleRect.setLocation(location);
        return visibleRect;
    }

    public Rectangle getVisibleViewRect() {
        return getVisibleViewRect(getViewLocation());
    }

    private boolean rectWidthInView(Rectangle rect) {
        return rect.width <= pdfView.getVisibleViewSize().width;
    }

    private boolean rectHeightInView(Rectangle rect) {
        return rect.height <= pdfView.getVisibleViewSize().height;
    }

    private boolean pageHeightInView(int index) {
        return rectHeightInView(docCache.getPageBounds(index, true));
    }

    private Point getViewLocation() {
        return toAbsoluteLocation(topViewPage, relativeViewLocation);
    }

    private Point toAbsoluteLocation(int index, Point2D.Float relLocation) {
        Point location = new Point();
        final Rectangle pageBounds = docCache.getPageBounds(index, false);

        if (!pageBounds.isEmpty()) {
            final Dimension pageIndent = docCache.getPageIndent();
            location.x = Math.round(relLocation.x * pageBounds.width) + pageBounds.x - pageIndent.width;
            location.y = Math.round(relLocation.y * pageBounds.height) + pageBounds.y - pageIndent.height;
        }
        return location;
    }

    private Point2D.Float toRelativeLocation(int index, Point location) {
        Point2D.Float relLocation = new Point2D.Float();
        final Rectangle pageBounds = docCache.getPageBounds(index, false);

        if (!pageBounds.isEmpty()) {
            final Dimension pageIndent = docCache.getPageIndent();
            relLocation.x = (float) (location.x - pageBounds.x + pageIndent.width) / (float) pageBounds.getWidth();
            relLocation.y = (float) (location.y - pageBounds.y + pageIndent.height) / (float) pageBounds.getHeight();
        }
        return relLocation;
    }

    private boolean hasValidDocument() {
        return document != null && document.isOpened();
    }

    private void onViewUpdate(boolean changed) {
        for (ViewListener l : getListeners())
            l.viewUpdated(changed);
    }

    private ViewListener[] getListeners() {
        return listeners.toArray(new ViewListener[0]);
    }

    // marker variables to indicate special positions
    public final static Point2D.Float LEFT_TOP_ALIGN = new Point2D.Float();
    public final static Point2D.Float LEFT_BOTTOM_ALIGN = new Point2D.Float();

    private Application application;
    private List<ViewListener> listeners;

    private JavaDocument document;
    private PDF pdfView;
    private DocumentCache docCache;

    // view parameters
    private int activePage;
    private ZoomMode zoomMode;
    private PageViewMode viewMode;

    // zoom boundaries
    private double minZoom;
    private double maxZoom;

    private Point2D.Float relativeViewLocation; // location of the view in relative coordinates to the special relative page
    private int topViewPage; // should be the top-most page of the view
}
