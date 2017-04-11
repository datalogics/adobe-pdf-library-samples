/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation;

import java.util.ArrayList;

import com.datalogics.PDFL.Bookmark;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.JavaViewer.Document.Command.GoToCommand;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Bookmarks;

/**
 * BookmarksPresenter - lets to navigate through document by bookmarks.
 * 
 * This class connects document's bookmark logic and view representation. It
 * also contains a helper class BookmarkElement which is used for bookmarks tree
 * construction.
 */
public class BookmarksPresenter {
    public class BookmarkElement {
        BookmarkElement(Bookmark bm) {
            this.bm = bm;
        }

        boolean isLeaf() {
            return (this.bm == null || !this.bm.hasChildren());
        }

        int getChildCount() {
            cacheChildren();
            return (list != null) ? list.size() : 0;
        }

        int getIndexOfChild(Bookmark bm) {
            cacheChildren();
            return (list != null) ? list.indexOf(bm) : -1;
        }

        BookmarkElement getChild(int index) {
            cacheChildren();
            return (list != null) ? list.get(index) : null;
        }

        Bookmark getBookmark() {
            return this.bm;
        }

        @Override
        public String toString() {
            return (bm != null) ? bm.getTitle() : "";
        }

        private void cacheChildren() {
            if (list == null && !isLeaf()) {
                list = new ArrayList<BookmarkElement>();
                Bookmark bmChild = bm.getFirstChild();

                while (bmChild != null) {
                    list.add(new BookmarkElement(bmChild));
                    bmChild = bmChild.getNext();
                }
            }
        }

        private ArrayList<BookmarkElement> list;
        private Bookmark bm;
    }

    public BookmarksPresenter(Application application) {
        this.application = application;
    }

    public BookmarkElement getChild(BookmarkElement parent, int index) {
        return parent.getChild(index);
    }

    public int getChildCount(BookmarkElement parent) {
        return parent.getChildCount();
    }

    public int getIndexOfChild(BookmarkElement parent, BookmarkElement child) {
        return parent.getIndexOfChild(child.getBookmark());
    }

    public BookmarkElement getRoot() {
        return root;
    }

    public boolean isLeaf(BookmarkElement node) {
        return node.isLeaf();
    }

    public void goToBookmark(BookmarkElement bookmarkElement) {
        application.executeCommand(new GoToCommand(bookmarkElement.getBookmark().getAction()));
    }

    void setView(Bookmarks view) {
        this.bookmarksView = view;
    }

    void setPDF(Document pdfDoc) {
        // determine new bookmarks' root
        BookmarkElement oldRoot = root;
        root = new BookmarkElement((pdfDoc != null) ? pdfDoc.getBookmarkRoot() : null);

        bookmarksView.nodeChanged(oldRoot, root);
        bookmarksView.setBookmarksVisible(root.getBookmark() != null && root.getBookmark().hasChildren());
    }

    private BookmarkElement root;
    private Bookmarks bookmarksView;
    private Application application;
}
