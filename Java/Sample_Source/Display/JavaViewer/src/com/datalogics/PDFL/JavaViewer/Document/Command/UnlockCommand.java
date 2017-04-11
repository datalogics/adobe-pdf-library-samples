/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;

/**
 * UnlockCommand - show the password dialog specified for the document.
 * 
 * This command passes the password to the JavaDocument object for unlocking the
 * document.
 */
public class UnlockCommand extends DocumentCommand {
    @Override
    protected void prepare() throws FinalState {
        pass = getApplication().getView().getDialogs().promptPassword();
    }

    @Override
    protected void doInner() throws FinalState {
        if (getApplication().getActiveDocument() != null && pass != null) {
            if (!getApplication().getActiveDocument().unlock(pass)) {
                getApplication().getView().getDialogs().messageDialog(Dialogs.MessageType.OPEN_FAIL);
            }
        }
    }

    private final static CommandType type = CommandType.VIEW; // field used through reflection

    private String pass;
}
