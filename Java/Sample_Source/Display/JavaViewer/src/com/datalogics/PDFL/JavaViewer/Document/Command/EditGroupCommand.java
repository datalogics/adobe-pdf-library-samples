/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

/**
 * EditGroupCommand - allows to save the annotation's properties (except of
 * resizing) for group annotation during interaction with the user.
 */
public class EditGroupCommand extends BaseGroupCommand {
    @Override
    protected void redoInner() throws FinalState {
        super.redoInner();
        changeProperties();
    }

    @Override
    protected void undoInner() throws FinalState {
        super.undoInner();
        changeProperties();
    }

    private final static CommandType type = CommandType.EDIT;
}
