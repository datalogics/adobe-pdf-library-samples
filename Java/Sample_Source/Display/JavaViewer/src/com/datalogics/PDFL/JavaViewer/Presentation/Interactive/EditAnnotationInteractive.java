/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

import java.awt.Rectangle;
import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.JavaViewer.Utils;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Document.DocumentListener;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationConsts;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.AnnotationEditorFactory;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.BaseAnnotationEditor;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.BaseAnnotationInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.CreateTextMarkupOutput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.EditAnnotationInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.EditAnnotationOuptut;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.FreeTextAnnotationEditor;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.GroupAnnotationEditor;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.GroupAnnotationOutput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.Hit;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.HoverAnnotationInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.HoverAnnotationOutput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.LinkAnnotationEditor;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.LinkTargetInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.LinkTargetOutput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.SelectAnnotationInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.SelectGroupInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.TextEditInput;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.TextMarkupAnnotationEditor;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PDF;

/**
 * EditAnnotationInteractive - provides interactivity for editing annotations.
 * 
 * It collects the annotations from the page and presents the ability for
 * annotation editing, selection etc.
 */
public class EditAnnotationInteractive extends BaseInteractive implements DocumentListener, EditorListener {
    public EditAnnotationInteractive() {
        this.scrollInteractive = new ScrollModeInteractive();
        this.editors = new ArrayList<BaseAnnotationEditor>();
    }

    @Override
    public void mousePressed(InputData input) {
        super.mousePressed(input);
        if (input.getLeftButton() && input.getShiftDown()) {
            updateGroup();
        } else {
            // perform selection and handle all the selection functionality in
            // selectionChanged()
            boolean supAnnot = lastSearch.hasValidAnnotation() && getEditors()[lastSearch.annotationIndex].getProperties().getIsSuported() && AnnotationConsts.Subtypes.isSupportedType(getEditors()[lastSearch.annotationIndex].getProperties().getSubtype());

            getDocument().select(supAnnot ? editors.get(lastSearch.getAnnotationIndex()).getHolder() : null);
        }

        // give activated editor a chance to process input by itself
        if (selectedEditor != null) {
            selectedEditor.mousePressed(input);
        }
    }

    @Override
    public void mouseMoved(InputData input) {
        super.mouseMoved(input);

        final SearchResult search = findAnnotation(input.getLocation(), lastSearch);
        if (search.getAnnotationIndex() != lastSearch.getAnnotationIndex()) {
            if (lastSearch.hasValidAnnotation() && !getEditorState(editors.get(lastSearch.getAnnotationIndex())).isSelected())
                editors.get(lastSearch.getAnnotationIndex()).setState(EditorStates.IDLE);

            if (search.hasValidAnnotation() && !getEditorState(editors.get(search.getAnnotationIndex())).isSelected())
                editors.get(search.getAnnotationIndex()).setState(EditorStates.HOVER);

            lastSearch = search;
        }

        if (lastSearch.hasValidAnnotation())
            input.markProcessed();
    }

    @Override
    public void activate(boolean active) {
        super.activate(active);
        scrollInteractive.activate(active);

        // form chain of handlers
        getApplication().registerInteractive(scrollInteractive, active);
        getApplication().registerInteractive(this, active); // make this handler be ahead of the scroll handler

        if (isActive()) {
            getApplication().getActiveDocument().addDocumentListener(this);
            updatePageEditors(getDocument().getPageAnnotations(getActivePage()));
        } else {
            updatePageEditors(new ArrayList<AnnotationHolder>());
            getApplication().getActiveDocument().removeDocumentListener(this);
        }
    }

    public void pdfChanged(Document pdfDoc) {
    }

    public void permissionChanged() {
    }

    public void pdfLayerUpdated(List<OptionalContentGroup> ocgList) {
    }

    public void pageChanged(int index, Rect changedArea) {
    }

    public void annotationUpdated(AnnotationHolder holder, boolean created) {
        updatePageEditors(getDocument().getPageAnnotations(getActivePage()));
    }

    public void stateChanged(BaseAnnotationEditor sender, Enum<?> oldState, Enum<?> newState) {
        if (((EditorStates) oldState).isSelected() && newState.equals(EditorStates.IDLE) && getApplication().getActiveDocument().getSelection() == sender.getHolder()) {
            getApplication().getActiveDocument().select(null);
            // sender.release();
        } else if (oldState.equals(EditorStates.IDLE) && ((EditorStates) newState).isSelected()) {
            getApplication().getActiveDocument().select(sender.getHolder());
            // sender.capture();
        }
    }

    public void selectionChanged(Object oldSelection) {
        if (oldSelection instanceof AnnotationHolder) {
            BaseAnnotationEditor editor = getEditorByHolder((AnnotationHolder) oldSelection);
            if (editor != null) {
                editor.setState(EditorStates.IDLE);
                selectedEditor = null;

                if (editor instanceof GroupAnnotationEditor) {
                    deleteGroupAnnotation();
                }
                for (int i = editors.indexOf(editor), len = editors.size(); i < len; ++i) { // put all other editors that were further than active one higher in the input processing again
                    getApplication().registerInteractive(editor, true);
                }
            }
        }

        final Object newSelection = getDocument().getSelection();
        if (newSelection instanceof AnnotationHolder) {
            BaseAnnotationEditor editor = getEditorByHolder((AnnotationHolder) newSelection);
            getApplication().registerInteractive(editor, true); // put current editor on top of all others

            if (!getEditorState(editor).isSelected())
                editor.setState(EditorStates.SELECT);
            selectedEditor = editor;
        }
    }

    public BaseAnnotationEditor getEditorByHolder(AnnotationHolder holder) {
        BaseAnnotationEditor editor = null;
        for (BaseAnnotationEditor e : editors) {
            if (e.getHolder() == holder) {
                editor = e;
                break;
            }
        }
        return editor;
    }

    public void enableEditors(boolean enable) {
        getApplication().registerInteractive(this, enable);

        for (BaseAnnotationEditor editor : getEditors())
            getApplication().registerInteractive(editor, enable);

        getApplication().registerInteractive(selectedEditor, enable);
    }

    @Override
    protected void onPageChanged() {
        super.onPageChanged();
        if (isActive()) {
            updatePageEditors(getDocument().getPageAnnotations(getActivePage()));
        }
    }

    private void updateGroup() {
        if (lastSearch.hasValidAnnotation()) {
            if (selectedEditor == null) {
                getDocument().select(editors.get(lastSearch.getAnnotationIndex()).getHolder());// if first annotation was created just select it
            } else {
                final AnnotationHolder lastHolder = editors.get(lastSearch.getAnnotationIndex()).getHolder();
                if (groupEditor == null) {
                    final AnnotationHolder selectedHolder = selectedEditor.getHolder();
                    final AnnotationHolder groupHolder = getDocument().createAnnotation(AnnotationConsts.Subtypes.GROUP, getActivePage(), selectedEditor.getHolder().getProperties(), getDocument().getPageAnnotations(getActivePage()).size() - 1);

                    getDocument().select(groupHolder);

                    final BaseAnnotationEditor editor = getEditorByHolder(selectedHolder);
                    groupEditor.addEditor(editor);
                    configureGroupCustomStates(editor, true);
                }
                updateGroupEditor(getEditorByHolder(lastHolder));
            }
        }
    }

    private void updateGroupEditor(BaseAnnotationEditor editor) {
        final boolean hasEditor = groupEditor.hasEditor(editor);
        if (!(editor instanceof GroupAnnotationEditor)) {
            if (hasEditor)
                groupEditor.removeEditor(editor);
            else
                groupEditor.addEditor(editor);

            configureGroupCustomStates(editor, !hasEditor);

            if (groupEditor.getEditorsCount() == 1) {
                editor = groupEditor.getEditor(0);
                groupEditor.removeEditor(editor);
                getDocument().select(editor.getHolder());
            }
        }
    }

    private void deleteGroupAnnotation() {
        if (groupEditor != null) {
            if (!groupEditor.getEditors().isEmpty()) {
                final List<BaseAnnotationEditor> editorsCopy = groupEditor.getEditors();
                groupEditor.clearEditors();
                for (BaseAnnotationEditor editor : editorsCopy) {
                    configureIdleHover(editor);
                }
            }
            getDocument().deleteAnnotation(groupEditor.getHolder());
            groupEditor = null;
        }
    }

    private void configureGroupCustomStates(BaseAnnotationEditor editor, boolean group) {
        final EditorStates state = (EditorStates) editor.getState();
        editor.setState(EditorStates.NONE);

        if (group)
            configureGroupIdleHover(editor);
        else
            configureIdleHover(editor);

        editor.setState(state);
    }

    private void configureGroupIdleHover(BaseAnnotationEditor editor) {
        editor.configureState(EditorStates.HOVER, null, new HoverAnnotationOutput());
        editor.configureState(EditorStates.IDLE, null, null);
    }

    private void configureIdleHover(BaseAnnotationEditor editor) {
        editor.configureState(EditorStates.HOVER, new HoverAnnotationInput(), new HoverAnnotationOutput());
        editor.configureState(EditorStates.IDLE, null, null);

    }

    private JavaDocument getDocument() {
        return getApplication().getActiveDocument();
    }

    /**
     * Finds the annotation under mouse pointer.
     * 
     * @param location - the place where mouse pointer is located.
     * @param lastSearch - previous search result used for search optimization.
     * @return - new instance of SearchResult with annotation index and
     * location; if annotation hasn't been found, its index is -1 and location
     * is null
     */
    private SearchResult findAnnotation(java.awt.Point location, SearchResult lastSearch) {
        final Rectangle bounds = getPageModel().getBounds();
        if (!bounds.contains(location) || editors.isEmpty())
            return new SearchResult();
        if (lastSearch.getHit() != null && location.equals(lastSearch.getHit().getLocation()))
            return lastSearch;

        // try hitting to the previously selected annotation first
        final int cachedIndex = lastSearch.getAnnotationIndex();
        if ((cachedIndex >= 0) && (cachedIndex < editors.size()) && getEditorState(editors.get(cachedIndex)).isSelected()) {
            Hit hit = editors.get(cachedIndex).testHit(location);
            if (hit.hasHit()) {
                return new SearchResult(cachedIndex, hit);
            }
        }

        // find annotation with a closest center to the given location
        final Point pdfPoint = getPageModel().transform(location);
        double distance = Double.MAX_VALUE;
        int nearestAnnoIndex = -1;
        Hit nearestHit = null;

        for (int index = 0; index < editors.size(); ++index) {
            BaseAnnotationEditor editor = editors.get(index);
            final Hit hit = editor.testHit(location);
            if (hit.noHit())
                continue;

            final Rect rect = editor.getProperties().getBoundingRect();
            final Point center = Utils.rectCenter(rect);
            final double d = (center.getH() - pdfPoint.getH()) * (center.getH() - pdfPoint.getH()) + (center.getV() - pdfPoint.getV()) * (center.getV() - pdfPoint.getV());

            if (d < distance) {
                distance = d;
                nearestAnnoIndex = index;
                nearestHit = hit;
            }
        }
        return new SearchResult(nearestAnnoIndex, nearestHit);
    }

    /**
     * Method is used for recreating annotation editors and configures their
     * states; it calls when page or document has been changed or annotations on
     * page has been modified.
     * 
     * @param holders - list of page annotations
     */
    private void updatePageEditors(List<AnnotationHolder> holders) {
        if (getDocument().getSelection() instanceof AnnotationHolder && !holders.contains(getDocument().getSelection())) {
            getDocument().select(null);
        }

        // editor cleanup
        for (BaseAnnotationEditor editor : editors) {
            editor.setState(EditorStates.IDLE);
            editor.removeEditorListener(this);
            getApplication().registerInteractive(editor, false);
        }
        editors.clear();
        for (AnnotationHolder holder : holders) {
            final BaseAnnotationEditor editor = AnnotationEditorFactory.createEditor(holder);

            if (editor instanceof GroupAnnotationEditor) {
                groupEditor = (GroupAnnotationEditor) editor;
            }

            final BaseAnnotationInput input = AnnotationEditorFactory.createInput(holder);
            editor.addEditorListener(this);

            getApplication().registerInteractive(editor, true);
            configureIdleHover(editor);

            if (editor instanceof GroupAnnotationEditor) {
                editor.configureState(EditorStates.SELECT, new SelectGroupInput(), new GroupAnnotationOutput(PDF.Cursor.HAND, false));
                editor.configureState(EditorStates.EDIT, new EditAnnotationInput(), new GroupAnnotationOutput(PDF.Cursor.HAND, false));
            } else {
                editor.configureState(EditorStates.SELECT, new SelectAnnotationInput(), new EditAnnotationOuptut(PDF.Cursor.HAND, false));
                editor.configureState(EditorStates.EDIT, new EditAnnotationInput(), new EditAnnotationOuptut(PDF.Cursor.HAND, false));
            }

            if (editor instanceof TextMarkupAnnotationEditor)
                editor.configureState(EditorStates.CREATE, input, new CreateTextMarkupOutput(PDF.Cursor.TEXT));
            else
                editor.configureState(EditorStates.CREATE, input, new EditAnnotationOuptut(PDF.Cursor.CROSS, true));

            if (editor instanceof FreeTextAnnotationEditor)
                editor.configureState(EditorStates.CUSTOM_EDIT, new TextEditInput(), new EditAnnotationOuptut(PDF.Cursor.HAND, false));
            if (editor instanceof LinkAnnotationEditor)
                editor.configureState(EditorStates.CUSTOM_EDIT, new LinkTargetInput(), new LinkTargetOutput());

            editor.setState(EditorStates.IDLE);
            editors.add(editor);
        }
        lastSearch = new SearchResult();
    }

    private EditorStates getEditorState(BaseAnnotationEditor editor) {
        return (EditorStates) editor.getState();
    }

    private BaseAnnotationEditor[] getEditors() {
        return editors.toArray(new BaseAnnotationEditor[0]);
    }

    private static class SearchResult {
        SearchResult(int index, Hit hit) {
            this.annotationIndex = index;
            this.hit = hit;
        }

        SearchResult() {
            this(-1, null);
        }

        boolean hasValidAnnotation() {
            return annotationIndex != -1;
        }

        int getAnnotationIndex() {
            return annotationIndex;
        }

        Hit getHit() {
            return hit;
        }

        private int annotationIndex;
        private Hit hit;
    }

    private final ScrollModeInteractive scrollInteractive;
    private final List<BaseAnnotationEditor> editors;

    private BaseAnnotationEditor selectedEditor;
    private GroupAnnotationEditor groupEditor;
    private SearchResult lastSearch;
}
