/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

/**
 * EditAnnotationCommand - allows to save the annotation's properties during
 * interaction with the user.
 */
public class EditAnnotationCommand extends BaseAnnotationCommand implements PendingCommand {
    public boolean consume(Class<? extends DocumentCommand> otherClass) {
        return false;
    }

    @Override
    protected void undoInner() throws FinalState {
        super.undoInner();
        changeProperties();
    }

    @Override
    protected void redoInner() throws FinalState {
        super.redoInner();
        changeProperties();
    }

    private final static CommandType type = CommandType.EDIT;
}
