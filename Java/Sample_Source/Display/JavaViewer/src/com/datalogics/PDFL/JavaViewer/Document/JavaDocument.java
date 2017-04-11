/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document;

import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.EnumSet;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.LibraryException;
import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.PageInsertFlags;
import com.datalogics.PDFL.PermissionRequestOperation;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Word;
import com.datalogics.PDFL.WordFinder;
import com.datalogics.PDFL.WordFinderConfig;
import com.datalogics.PDFL.WordFinderVersion;
import com.datalogics.PDFL.JavaViewer.Utils;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationFactory;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationListener;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Document.Command.CommandType;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;

/**
 * JavaDocument - it is a logical layer that encapsulates the Datalogics document
 * and all the actions performed with it.
 * 
 * This class has methods for document manipulations: open (simple and
 * protected), save, close, append.
 * 
 * JavaDocument has methods for creation and deletion of annotations, it keeps
 * collected page annotations in the annotations' map. The class also stores
 * selected object (such as annotation), and notifies all listeners when
 * selection is changed.
 * 
 * JavaDocument notifies about different types of changes through the
 * DocumentListener interface.
 */
public class JavaDocument {
    public JavaDocument() {
        this.listeners = new ArrayList<DocumentListener>();
        invalidateParameters();
    }

    public static boolean isCommandPermitted(JavaDocument document, Class<? extends DocumentCommand> commandClass) {
        CommandType type = CommandType.UNDEF;

        try {
            Method method = commandClass.getMethod("getType", new Class[] { Class.class });
            type = (CommandType) method.invoke(null, new Object[] { commandClass });
        } catch (Exception e) {
        }
        return isCommandPermitted(document, type);
    }

    public static boolean isCommandPermitted(JavaDocument document, CommandType type) {
        return (document != null) ? document.isPermissionEnabled(type) : type == CommandType.OPEN;
    }

    public Document getPDF() {
        return this.pdfDoc;
    }

    public boolean isOpened() {
        return pdfDoc != null;
    }

    public void addDocumentListener(DocumentListener listener) {
        listeners.add(listener);
    }

    public void removeDocumentListener(DocumentListener listener) {
        listeners.remove(listener);
    }

    public Boolean open(String path) {
        return open(path, null);
    }

    public Boolean open(String path, String pass) {
        Boolean opened = false;
        PermissionRequestOperation permOp = PermissionRequestOperation.ALL_OPERATIONS;
        final boolean noPass = (pass == null);
        try {
            if (noPass)
                pdfDoc = new Document(path);
            else
                pdfDoc = new Document(path, pass, permOp, false);
            opened = true;
            docPath = path;
        } catch (LibraryException e) {
            if (e.getErrorCode() == Constants.PDF.wrongPassword && noPass) { // document is encrypted and no password provided
                opened = null;
            } else if (e.getErrorCode() == Constants.PDF.wrongFileName || e.getErrorCode() == Constants.PDF.unsupportedFileType) { // document wasn't found
                opened = false;
            } else {
                // try opening with reduced permissions
                try {
                    permOp = PermissionRequestOperation.OPEN;
                    pdfDoc = new Document(path, pass, permOp, false);
                    opened = true;
                } catch (LibraryException OpenExcept) {
                    opened = null;
                }
            }
        }
        if (Boolean.TRUE.equals(opened)) {
            initLayers();
            resetAnnotations();

            for (DocumentListener l : getListeners())
                l.pdfChanged(pdfDoc);

            updatePermissions();
        }
        return opened;
    }

    public void save() {
        save(getDocumentPath());
    }

    public void save(String path) {
        final Object savedSelection = this.selection;
        select(null); // temporary remove any selections

        EnumSet<SaveFlags> sf = EnumSet.of(SaveFlags.FULL);
        if (path != null) {
            pdfDoc.save(sf, path);
            docPath = path;
            for (DocumentListener l : getListeners())
                l.pdfChanged(pdfDoc);
        }

        select(savedSelection); // restore selection
        pdfDoc.setNeedsSave(false);
    }

    public void close() {
        if (pdfDoc != null) {
            invalidateParameters();

            if (pdfDoc != null) {
                pdfDoc.delete();
            }
            pdfDoc = null;
            for (DocumentListener l : getListeners())
                l.pdfChanged(pdfDoc);
        }
    }

    public boolean isModified() {
        return isOpened() && pdfDoc.getNeedsSave();
    }

    public boolean append(Document newDoc) {

        // prohibit appending the document to itself
        final boolean canAppend = !this.pdfDoc.getFileName().equals(newDoc.getFileName());
        if (canAppend) {
            pdfDoc.insertPages(Document.LAST_PAGE, newDoc, 0, newDoc.getNumPages(), EnumSet.of(PageInsertFlags.ALL));
            initLayers();
            resetAnnotations();

            for (DocumentListener l : getListeners())
                l.pdfChanged(pdfDoc);
        }
        return canAppend;
    }

    public void removeDocumentPages(int afterPage) {
        pdfDoc.deletePages(afterPage + 1, pdfDoc.getNumPages());
        initLayers();
        resetAnnotations();

        for (DocumentListener l : getListeners())
            l.pdfChanged(pdfDoc);
    }

    public boolean isLocked() {
        return isOpened() ? !pdfDoc.permRequest(PermissionRequestOperation.ALL_OPERATIONS) : false;
    }

    public boolean unlock(String pass) {
        final boolean permChanged = (pass != null) ? pdfDoc.permRequest(pass, PermissionRequestOperation.ALL_OPERATIONS) : false;
        if (permChanged)
            updatePermissions();
        return permChanged;
    }

    public void changeLayersState(boolean state, boolean isDisplayableLayer, List<OptionalContentGroup> ocgList) {
        for (OptionalContentGroup ocg : ocgList) {
            if (isDisplayableLayer)
                pdfDoc.getOptionalContentContext().setStateOfOCG(ocg, state);
            layersState.get(ocg).setLayerState(isDisplayableLayer, state);
        }

        for (DocumentListener l : getListeners())
            l.pdfLayerUpdated(ocgList);
    }

    public boolean getShowLayerState(OptionalContentGroup ocg) {
        return layersState.get(ocg).getShowLayer();
    }

    public boolean getPrintLayerState(OptionalContentGroup ocg) {
        return layersState.get(ocg).getPrintLayer();
    }

    public boolean hasLayers() {
        return !layersState.isEmpty();
    }

    public boolean hasBookmarks() {
        return isOpened() && pdfDoc.getBookmarkRoot().hasChildren();
    }

    public List<OptionalContentGroup> getOCGList() {
        return Collections.unmodifiableList(new ArrayList<OptionalContentGroup>(layersState.keySet()));
    }

    public Rect getSymbolOnPage(int pageNum, Point point) {
        Rect symbolRect = null;
        if (isOpened() && (pageNum >= 0 && pageNum < pdfDoc.getNumPages())) {
            final WordFinder wordFinder = new WordFinder(pdfDoc, WordFinderVersion.LATEST, new WordFinderConfig());
            final List<Word> wordList = wordFinder.getWordList(pageNum);
            for (Word w : wordList) {
                for (Quad quad : w.getCharQuads()) {
                    if (point.getH() >= quad.getTopLeft().getH() && point.getV() >= quad.getTopLeft().getV() && point.getH() >= quad.getBottomRight().getH() && point.getV() >= quad.getBottomRight().getV()) {
                        symbolRect = quad.toRect();
                    }
                }
            }
            wordFinder.delete();
        }
        return symbolRect;
    }

    public List<Quad> getPageQuads(int pageNum) {
        List<Quad> quads = new ArrayList<Quad>();
        if (isOpened() && (pageNum >= 0 && pageNum < pdfDoc.getNumPages())) {
            WordFinderConfig config = new WordFinderConfig();
            config.setIgnoreCharGaps(true);
            config.setNoAnnots(true);
            config.setNoEncodingGuess(true);
            config.setNoSkewedQuads(true);
            config.setNoStyleInfo(true);
            config.setPreserveSpaces(false);
            config.setPreserveRedundantChars(false);

            final WordFinder wordFinder = new WordFinder(pdfDoc, WordFinderVersion.LATEST, config);
            final List<Word> wordList = wordFinder.getWordList(pageNum);
            for (Word w : wordList) {
                quads.addAll(w.getCharQuads());
            }

            Collections.sort(quads, new Comparator<Quad>() {
                public int compare(Quad quad1, Quad quad2) {
                    int result = 0;
                    final Rect quadRect1 = quad1.toRect();
                    final Rect quadRect2 = quad2.toRect();
                    final Point center1 = Utils.rectCenter(quadRect1);
                    final Point center2 = Utils.rectCenter(quadRect2);

                    // check two quads belong to the same line
                    if (Utils.pointInRect(new Point(center2.getH(), center1.getV()), quadRect2) && Utils.pointInRect(new Point(center1.getH(), center2.getV()), quadRect1)) {
                        result = (center1.getH() < center2.getH()) ? -1 : 1;
                    } else {
                        result = (center1.getV() < center2.getV()) ? 1 : -1;
                    }
                    return result;
                }
            });
            wordFinder.delete();
        }
        return quads;
    }

    public List<Rect> searchText(int pageNum, String searchPhrase) {
        List<Rect> wordRects = new ArrayList<Rect>();
        if (isOpened() && (pageNum >= 0 && pageNum < pdfDoc.getNumPages())) {
            final WordFinder wordFinder = new WordFinder(pdfDoc, WordFinderVersion.LATEST, new WordFinderConfig());

            if (searchPhrase != null && searchPhrase.length() > 0) {
                final List<Word> wordList = wordFinder.getWordList(pageNum);
                for (Word w : wordList) {
                    final String word = w.getText().toLowerCase();
                    final int searchTextLen = searchPhrase.length() - 1;
                    int searchIndex = 0;
                    if (word.contains(searchPhrase)) {
                        while (searchIndex < word.length()) {
                            final int startOfText = word.indexOf(searchPhrase, searchIndex);
                            if (startOfText == -1 || startOfText >= word.length())
                                break;

                            Point leftBottom = w.getCharQuads().get(startOfText).getBottomLeft();
                            Point rightBottom = w.getCharQuads().get(startOfText + searchTextLen).getBottomRight();
                            Point leftTop = w.getCharQuads().get(startOfText).getTopLeft();
                            Point rightTop = w.getCharQuads().get(startOfText + searchTextLen).getTopRight();

                            Quad quad = new Quad(leftTop, rightTop, leftBottom, rightBottom);
                            wordRects.add(quad.toRect());
                            searchIndex = startOfText + searchPhrase.length();
                        }
                    }
                }
            }
            wordFinder.delete();
        }
        return Collections.unmodifiableList(wordRects);
    }

    public Object getSelection() {
        return this.selection;
    }

    public List<AnnotationHolder> getPageAnnotations(int index) {
        return Collections.unmodifiableList(getPageAnnotationsWritable(index));
    }

    public void select(Object select) {
        if (select != selection) {
            final AnnotationListener oldListener = annotationListener;

            // check if new selection is an annotation
            if (select instanceof AnnotationHolder) {
                final AnnotationHolder holder = (AnnotationHolder) select;

                // set up new annotation listener
                annotationListener = new AnnotationListener() {
                    public void update(Object updateData) {
                        if (AnnotationHolder.AppearanceUpdate.OBJECT_SHOWN.equals(updateData) || AnnotationHolder.AppearanceUpdate.OBJECT_HIDDEN.equals(updateData)) {
                            for (DocumentListener listener : getListeners())
                                listener.pageChanged(holder.getPageIndex(), holder.getProperties().getBoundingRect());
                        }
                    }
                };
                holder.addAnnotationListener(annotationListener);
            }

            // send updates
            final Object oldSelection = selection;
            selection = select;
            for (DocumentListener l : getListeners())
                l.selectionChanged(oldSelection);

            // remove previous listener from annotation
            if (oldSelection instanceof AnnotationHolder)
                ((AnnotationHolder) oldSelection).removeAnnotationListener(oldListener);
        }
    }

    public void deleteAnnotation(AnnotationHolder holder) {
        int annotationIndex = holder.getIndex();
        int pageNum = holder.getPageIndex();

        Page page = pdfDoc.getPage(pageNum);
        final List<AnnotationHolder> annotationList = annotations.get(pageNum);

        AnnotationFactory.deleteAnnotation(page, annotationIndex);

        if (annotationList != null) {
            annotationList.remove(holder);

            changeAnnotationIndexes(pageNum, annotationIndex, false);
            annotations.put(pageNum, annotationList);

            for (DocumentListener l : getListeners()) {
                l.annotationUpdated(null, false);
            }
        }
    }

    public AnnotationHolder createAnnotation(String subType, int pageNum, AnnotationProperties properties, int addAfter) {
        final boolean isLastAnnotation = addAfter == -2;
        AnnotationHolder holder = AnnotationFactory.createAnnotation(subType, properties, pdfDoc.getPage(pageNum), addAfter);
        ++addAfter;

        if (isLastAnnotation) {
            getPageAnnotationsWritable(pageNum).add(holder);
        } else {
            getPageAnnotationsWritable(pageNum).add(addAfter, holder);
            changeAnnotationIndexes(pageNum, addAfter + 1, true); // modifying all annotation indexes in holders after current holder
        }

        for (DocumentListener l : getListeners())
            l.annotationUpdated(holder, true);

        return holder;
    }

    public String getDocumentPath() {
        return docPath;
    }

    public void setDocumentPath(String docPath) {
        this.docPath = docPath;
        pdfDoc.setNeedsSave(true);
        for (DocumentListener l : getListeners())
            l.pdfChanged(pdfDoc);
    }

    private void changeAnnotationIndexes(int pageNum, int startIndex, boolean incDec) {
        if (annotations.containsKey(pageNum)) {
            List<AnnotationHolder> holders = annotations.get(pageNum);
            while (startIndex < holders.size()) {
                final AnnotationHolder holder = holders.get(startIndex);
                if (holder != null) {
                    holder.setIndex(incDec ? holder.getIndex() + 1 : holder.getIndex() - 1);
                    holders.set(startIndex, holder);
                    ++startIndex;
                }
            }
            annotations.put(pageNum, holders);
        }
    }

    private boolean isPermissionEnabled(CommandType type) {
        return permissions.contains(type);
    }

    private void initLayers() {
        resetLayers();

        for (OptionalContentGroup ocg : pdfDoc.getOptionalContentGroups()) {
            final boolean state = this.pdfDoc.getOptionalContentContext().getStateOfOCG(ocg);
            layersState.put(ocg, new LayerItem(state, state));
        }
    }

    private void resetLayers() {
        this.layersState = new LinkedHashMap<OptionalContentGroup, LayerItem>();
    }

    private void invalidateParameters() {
        resetLayers();
        resetAnnotations();
        changePermissions(EnumSet.of(CommandType.OPEN));
    }

    private void updatePermissions() {
        final Set<CommandType> perms = EnumSet.noneOf(CommandType.class);
        // read permissions from the current document
        for (CommandType permType : CommandType.values()) {
            if (pdfDoc.permRequest(permType.getPermission()))
                perms.add(permType);
        }
        changePermissions(perms);
    }

    private void changePermissions(Set<CommandType> permissions) {
        this.permissions = permissions;
        // send update about permission change
        for (DocumentListener l : getListeners())
            l.permissionChanged();
    }

    private class LayerItem {
        LayerItem(boolean showLayer, boolean printLayer) {
            this.displayableLayer = showLayer;
            this.printLayer = printLayer;
        }

        public void setLayerState(boolean isDisplayableLayer, boolean state) {
            if (isDisplayableLayer)
                this.displayableLayer = state;
            else
                this.printLayer = state;
        }

        public boolean getShowLayer() {
            return displayableLayer;
        }

        public boolean getPrintLayer() {
            return printLayer;
        }

        private boolean displayableLayer;
        private boolean printLayer;
    }

    private void resetAnnotations() {
        this.annotations = new HashMap<Integer, List<AnnotationHolder>>();
    }

    private List<AnnotationHolder> getPageAnnotationsWritable(int index) {
        if (!annotations.containsKey(index)) {
            annotations.put(index, annotationsOnPage(index));
        }
        return annotations.get(index);
    }

    private List<AnnotationHolder> annotationsOnPage(int index) {
        List<AnnotationHolder> pageAnnotations = new ArrayList<AnnotationHolder>();

        // collect annotations on page
        if (isOpened() && index >= 0 && index < pdfDoc.getNumPages()) {
            final Page page = pdfDoc.getPage(index);
            for (int i = 0, len = page.getNumAnnotations(); i < len; ++i) {
                // pageAnnotations.add(new AnnotationHolder(page.getAnnotation(i), i, page.getPageNumber()));
                pageAnnotations.add(AnnotationFactory.createAnnotationHolder(page.getAnnotation(i), i, page.getPageNumber()));
            }
        }
        return pageAnnotations;
    }

    private DocumentListener[] getListeners() {
        return listeners.toArray(new DocumentListener[0]);
    }

    private Document pdfDoc;
    private final List<DocumentListener> listeners;

    private Set<CommandType> permissions;
    private Map<OptionalContentGroup, LayerItem> layersState;

    private Map<Integer, List<AnnotationHolder>> annotations;
    private AnnotationListener annotationListener;
    private Object selection;

    private String docPath;
}
