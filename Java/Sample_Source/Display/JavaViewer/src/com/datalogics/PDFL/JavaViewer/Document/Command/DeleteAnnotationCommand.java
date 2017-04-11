/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;

/**
 * DeleteAnnotationCommand - allows to delete annotation from the page with
 * ability to undo the operation.
 */
public class DeleteAnnotationCommand extends BaseAnnotationCommand {
    @Override
    protected void prepare() throws FinalState {
        saveAnotationParams();
        annotationSubtype = getHolder().getProperties().getSubtype();
    }

    @Override
    protected void doInner() throws FinalState {
        getApplication().getActiveDocument().deleteAnnotation(getHolder());
    }

    @Override
    protected void undoInner() throws FinalState {
        super.undoInner();
        getApplication().getActiveDocument().createAnnotation(annotationSubtype, getPageNum(), getProperties(), getAnnotationIndex() - 1);
        getAnnotationInteractive().getEditorByHolder(getHolder()).setState(EditorStates.SELECT);
        changeProperties();
    }

    @Override
    protected void redoInner() throws FinalState {
        super.redoInner();
        doInner();
    }

    private final static CommandType type = CommandType.EDIT; // field used through reflection

    private String annotationSubtype;
}
