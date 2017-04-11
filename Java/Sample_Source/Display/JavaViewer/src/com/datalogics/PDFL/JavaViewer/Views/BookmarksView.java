/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

/*
 * BookmarksView - represents document bookmarks as a JTree and sends
 * notification to all listeners.
 */

import java.awt.Component;
import java.util.ArrayList;
import java.util.List;

import javax.swing.JScrollPane;
import javax.swing.JTabbedPane;
import javax.swing.JTree;
import javax.swing.event.TreeModelEvent;
import javax.swing.event.TreeModelListener;
import javax.swing.event.TreeSelectionEvent;
import javax.swing.event.TreeSelectionListener;
import javax.swing.tree.DefaultTreeCellRenderer;
import javax.swing.tree.TreeModel;
import javax.swing.tree.TreePath;
import javax.swing.tree.TreeSelectionModel;

import com.datalogics.PDFL.JavaViewer.Presentation.BookmarksPresenter;
import com.datalogics.PDFL.JavaViewer.Presentation.BookmarksPresenter.BookmarkElement;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Bookmarks;

public class BookmarksView extends JTree implements Bookmarks, TreeSelectionListener {
    public BookmarksView(BookmarksPresenter pres, JTabbedPane tabbedPane) {
        this.presenter = pres;
        this.tabbedPane = tabbedPane;
        this.scrollable = new JScrollPane(this);
        this.treeModelListeners = new ArrayList<TreeModelListener>();

        TreeSelectionModel tsm = this.getSelectionModel();
        tsm.setSelectionMode(TreeSelectionModel.SINGLE_TREE_SELECTION);

        try {
            DefaultTreeCellRenderer renderer = (DefaultTreeCellRenderer) this.getCellRenderer();
            renderer.setLeafIcon(null);
            renderer.setOpenIcon(null);
            renderer.setClosedIcon(null);
        } catch (ClassCastException e) {
        }

        this.setRootVisible(false);
        this.setShowsRootHandles(true);

        this.addTreeSelectionListener(this);

        setModel(new TreeModel() {
            public void addTreeModelListener(TreeModelListener l) {
                treeModelListeners.add(l);
            }

            public Object getChild(Object parent, int index) {
                return presenter.getChild(asBookmarkElement(parent), index);
            }

            public int getChildCount(Object parent) {
                return presenter.getChildCount(asBookmarkElement(parent));
            }

            public int getIndexOfChild(Object parent, Object child) {
                return presenter.getIndexOfChild(asBookmarkElement(parent), asBookmarkElement(child));
            }

            public Object getRoot() {
                return presenter.getRoot();
            }

            public boolean isLeaf(Object node) {
                return presenter.isLeaf(asBookmarkElement(node));
            }

            public void removeTreeModelListener(TreeModelListener l) {
                treeModelListeners.remove(l);
            }

            public void valueForPathChanged(TreePath path, Object newValue) {
            }
        });
    }

    public void nodeChanged(Object oldNode, Object newNode) {
        TreeModelEvent e = new TreeModelEvent(this, new Object[] { newNode });
        for (TreeModelListener tml : getListeners()) {
            tml.treeStructureChanged(e);
        }
    }

    public void setBookmarksVisible(boolean visible) {
        final boolean isVisible = (tabbedPane.indexOfComponent(this.scrollable) != -1);
        if (!visible && isVisible)
            tabbedPane.removeTabAt(tabbedPane.indexOfComponent(this.scrollable));
        else if (visible && !isVisible)
            tabbedPane.addTab("Bookmarks", this.scrollable);
    }

    public void valueChanged(TreeSelectionEvent e) {
        final Object node = this.getLastSelectedPathComponent();

        if (node != null)
            this.presenter.goToBookmark(asBookmarkElement(node));
    }

    private BookmarkElement asBookmarkElement(Object el) {
        return (BookmarkElement) el;
    }

    private TreeModelListener[] getListeners() {
        return treeModelListeners.toArray(new TreeModelListener[0]);
    }

    private final BookmarksPresenter presenter;
    private final JTabbedPane tabbedPane;
    private final Component scrollable;
    private final List<TreeModelListener> treeModelListeners;
}
