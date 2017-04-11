/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.util.List;

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditAnnotationInteractive;
import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter.ViewShot;

/**
 * BaseAnnotationCommand class - base class for commands that allow
 * manipulations with annotations.
 * 
 * It has generic methods for do/undo/redo execution. It saves annotation
 * properties before command is executed, keeps them and restores in redo phase.
 */
public abstract class BaseAnnotationCommand extends ViewCommand {
    public static enum CommandState {
        DO, UNDO, REDO;

        boolean isUndo() {
            return this.equals(UNDO);
        }

        boolean isRedo() {
            return this.equals(REDO);
        }

        boolean isDo() {
            return this.equals(DO);
        }
    }

    public void setAnnotationInteractive(EditAnnotationInteractive annotationInteractive) {
        this.annotationInteractive = annotationInteractive;
    }

    public void setProperties(AnnotationProperties properties) {
        this.properties = new AnnotationProperties();
        this.properties.setFrom(properties);
    }

    protected EditAnnotationInteractive getAnnotationInteractive() {
        return annotationInteractive;
    }

    @Override
    protected void prepare() throws FinalState {
        saveAnotationParams();
        throw new FinalState(State.Pending);
    }

    @Override
    protected void doInner() throws FinalState {
        setCommandState(CommandState.DO);
        saveNewProperties();

        getApplication().getActiveDocument().select(getHolder());
        if (properties.equals(newProperties)) {
            // undo last changes if annotation hasn't been modified without
            // reverting back the view state
            viewParams = null;
            undoInner();
            throw new FinalState(State.Rejected);
        }
    }

    @Override
    protected void undoInner() throws FinalState {
        super.undoInner();
        setCommandState(CommandState.UNDO);
        backViewToPreviousState();
    }

    @Override
    protected void redoInner() throws FinalState {
        setCommandState(CommandState.REDO);
        backViewToPreviousState();
    }

    protected void saveNewProperties() {
        this.newProperties = new AnnotationProperties();
        this.newProperties.setFrom(getHolder().getProperties());
    }

    protected void changeProperties() {
        final AnnotationHolder holder = getHolder();
        getApplication().getActiveDocument().select(holder);
        AnnotationProperties props = getProperties();
        if (!holder.getProperties().equals(props)) {
            holder.getProperties().setFrom(props);
            holder.getProperties().setDirty(true);
            holder.getProperties().raiseUpdates(true);
        }
    }

    protected void saveAnotationParams() {
        if (getApplication().getActiveDocument().getSelection() instanceof AnnotationHolder) {
            AnnotationHolder holder = (AnnotationHolder) getApplication().getActiveDocument().getSelection();

            this.pageNum = holder.getPageIndex();
            this.annotationIndex = holder.getIndex();
        }

        setProperties(getHolder().getProperties());
        this.viewParams = getPDFPresenter().getViewShot();
    }

    protected int getPageNum() {
        return pageNum;
    }

    protected void setPageNum(int pageNum) {
        this.pageNum = pageNum;
    }

    protected int getAnnotationIndex() {
        return annotationIndex;
    }

    protected void setAnnotationIndex(int annotationIndex) {
        this.annotationIndex = annotationIndex;
    }

    protected AnnotationProperties getProperties() {
        AnnotationProperties props = getCommadState() == null ? new AnnotationProperties() : null;
        if (getCommadState() == null || getCommadState().isUndo()) {
            props = properties == null ? new AnnotationProperties() : properties;
        } else {
            props = newProperties == null ? new AnnotationProperties() : newProperties;
        }
        return props;
    }

    protected AnnotationHolder getHolder() {
        final List<AnnotationHolder> holderList = getApplication().getActiveDocument().getPageAnnotations(pageNum);
        AnnotationHolder holder = null;
        for (AnnotationHolder hold : holderList) {
            if (hold.getIndex() == annotationIndex) {
                holder = hold;
                break;
            }
        }
        return holder;
    }

    private void backViewToPreviousState() {
        if (viewParams != null)
            getApplication().executeCommand(new GoToCommand(viewParams));
    }

    protected void setCommandState(CommandState commandState) {
        this.commandState = commandState;
    }

    protected CommandState getCommadState() {
        return commandState;
    }

    private int pageNum;
    private int annotationIndex;

    private AnnotationProperties properties;
    private AnnotationProperties newProperties;

    private ViewShot viewParams;
    private EditAnnotationInteractive annotationInteractive;
    private CommandState commandState;
}
