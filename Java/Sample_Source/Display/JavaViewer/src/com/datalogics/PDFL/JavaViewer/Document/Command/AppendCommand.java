/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.io.File;

import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;

/**
 * AppendCommand - the command which allows to append some document to the
 * current document.
 * 
 * This command allows undo/redo functionality by saving temporary document and
 * restoring from it.
 */
public class AppendCommand extends DocumentCommand {
    @Override
    protected void cleanup() {
        new File(tmpName).delete();
        super.cleanup();
    }

    @Override
    protected void prepare() throws FinalState {
        appendFileName = getApplication().getView().getDialogs().openDialog();
        if (appendFileName != null) {
            originName = getApplication().getActiveDocument().getDocumentPath();
        } else {
            throw new FinalState(State.Canceled);
        }
    }

    @Override
    protected void doInner() throws FinalState {
        saveTemporary();

        JavaDocument newDocument = new JavaDocument();
        doOpen(newDocument, null, 1);
        if (newDocument.isOpened())
            getApplication().getActiveDocument().append(newDocument.getPDF());
    }

    @Override
    protected void undoInner() throws FinalState {
        super.undoInner();
        restoreTemporary();
    }

    private void saveTemporary() {
        getApplication().getActiveDocument().save(tmpName);
        getApplication().getActiveDocument().setDocumentPath(originName);
    }

    private void restoreTemporary() {
        getApplication().getActiveDocument().close();
        getApplication().getActiveDocument().open(tmpName);
        getApplication().getActiveDocument().setDocumentPath(originName);
    }

    private void doOpen(JavaDocument document, String pass, int counter) throws FinalState {
        final Boolean opened = document.open(appendFileName, pass);
        if (!Boolean.TRUE.equals(opened) || document.isLocked()) {
            if (counter == 0 || Boolean.FALSE.equals(opened)) {
                getApplication().getView().getDialogs().messageDialog(Dialogs.MessageType.OPEN_FAIL);
                throw new FinalState(State.Failed);
            }

            pass = getApplication().getView().getDialogs().promptPassword();
            doOpen(document, pass, --counter);
        }
    }

    private final static CommandType type = CommandType.EDIT; // field used through reflection

    private String appendFileName;
    private String originName;
    private final String tmpName = System.getProperty("java.io.tmpdir") + this.hashCode() + ".pdf";
}
