/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation;

import java.awt.Graphics;
import java.awt.Rectangle;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Interactive.InteractiveContext;

/**
 * TextSearchPresenter - responsible for searching text.
 * 
 * This class also highlights found text.
 */
public class TextSearchPresenter {
    TextSearchPresenter(InteractiveContext context) {
        this.context = context;
        highlightRectsCache = new HashMap<Integer, List<Rect>>();
    }

    void activeDocumentChanged() {
        document = context.getApplication().getActiveDocument();
        highlightRectsCache.clear();
    }

    void setActivePage(int pageNum) {
        if (this.activePage != pageNum) {
            this.activePage = pageNum;
            searchText();
        }
    }

    void setSearchPhrase(String searchPhrase) {
        searchPhrase = searchPhrase.toLowerCase();
        if (searchPhrase.length() == 0 || !searchPhrase.equals(this.searchPhrase))
            highlightRectsCache.clear();

        this.searchPhrase = searchPhrase;
        searchText();
    }

    void drawSearchedRects(Graphics g) {
        if (!highlightRectsCache.isEmpty()) {
            final java.awt.Color color = g.getColor();
            g.setColor(java.awt.Color.BLUE);

            for (Rect r : highlightRectsCache.get(activePage)) {
                Rectangle rect = context.getPageModel().transform(r);
                g.drawRect(rect.x, rect.y, rect.width, rect.height);
            }
            g.setColor(color);
        }
    }

    private void searchText() {
        if (!highlightRectsCache.containsKey(activePage) && document != null)
            highlightRectsCache.put(activePage, document.searchText(activePage, searchPhrase));
    }

    private final Map<Integer, List<Rect>> highlightRectsCache;

    private JavaDocument document;
    private InteractiveContext context;

    private int activePage;
    private String searchPhrase;
}
