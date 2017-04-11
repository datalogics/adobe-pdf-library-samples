/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;

/**
 * CreateAnnotationCommand - allows to create annotation.
 * 
 * It also keeps annotationSubtype string which is used for recreating
 * annotation during redo phase.
 */
public class CreateAnnotationCommand extends BaseAnnotationCommand implements PendingCommand {
    public CreateAnnotationCommand(String annotationSubtype) {
        this.annotationSubtype = annotationSubtype;
    }

    public boolean consume(Class<? extends DocumentCommand> otherClass) {
        boolean consumed = true;

        if (otherClass.equals(DeleteAnnotationCommand.class)) {
            cancel(); // request for creation cancellation
        } else if (otherClass.equals(CustomEditCommand.class)) {
            // count initial free text custom edit mode a part of creation cycle
        } else if (otherClass.equals(UpdatePropertyCommand.class)) {

        } else {
            consumed = false;
        }
        return consumed;
    }

    @Override
    protected void prepare() throws FinalState {
        final AnnotationHolder holder = getApplication().getActiveDocument().createAnnotation(annotationSubtype, getPDFPresenter().getActivePage(), getProperties(), -2); // annotation index = -2 it's mean that annotation add to the end of the annotation array
        getApplication().getActiveDocument().select(holder);

        getAnnotationInteractive().getEditorByHolder(holder).setState(EditorStates.CREATE);
        super.prepare();
    }

    @Override
    protected void undoInner() throws FinalState {
        super.undoInner();
        getApplication().getActiveDocument().select(null);
        getApplication().getActiveDocument().deleteAnnotation(getHolder());
    }

    @Override
    protected void redoInner() throws FinalState {
        super.redoInner();
        getApplication().getActiveDocument().createAnnotation(annotationSubtype, getPageNum(), getProperties(), getAnnotationIndex() - 1);
        getAnnotationInteractive().getEditorByHolder(getHolder()).setState(EditorStates.SELECT);
        changeProperties();
    }

    private final static CommandType type = CommandType.EDIT; // field used through reflection

    private String annotationSubtype;
}
