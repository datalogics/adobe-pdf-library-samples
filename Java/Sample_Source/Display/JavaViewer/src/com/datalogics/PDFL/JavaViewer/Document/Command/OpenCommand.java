/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;

/**
 * OpenCommand - allows to open the simple or protected PDF documents.
 * 
 * This command checks previously opened document before opening the new one,
 * and if the document was opened it executes the CloseCommand.
 */
public class OpenCommand extends DocumentCommand {
    @Override
    protected void prepare() throws FinalState {
        fileName = getApplication().getView().getDialogs().openDialog();
        if (fileName == null)
            throw new FinalState(State.Canceled);
    }

    @Override
    protected void doInner() throws FinalState {
        // try to close the current document
        if (getApplication().executeCommand(new CloseCommand()).getState() == State.Canceled)
            throw new FinalState(State.Canceled);

        JavaDocument document = getApplication().getActiveDocument();
        getApplication().removeDocument(document); // close current document

        try {
            document = new JavaDocument(); // new document to open
            getApplication().addDocument(document, true); // add document to the application
            Boolean opened = document.open(fileName);
            // should try with password
            if (opened == null) {
                final String password = getApplication().getView().getDialogs().promptPassword();
                if (password == null)
                    throw new FinalState(State.Canceled);

                opened = document.open(fileName, password);
            }

            if (!Boolean.TRUE.equals(opened)) { // failed again
                getApplication().getView().getDialogs().messageDialog(Dialogs.MessageType.OPEN_FAIL);
                throw new FinalState(State.Failed);
            }
        } catch (FinalState e) {
            getApplication().removeDocument(document); // something went wrong - close recently added document
            throw e;
        }
    }

    private final static CommandType type = CommandType.OPEN; // field used through reflection

    private String fileName;
}
