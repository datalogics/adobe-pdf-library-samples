/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.LibraryException;
import com.datalogics.PDFL.JavaViewer.Document.Constants;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;

/**
 * SaveCommand - allows to save the document.
 * 
 * Document can be saved as a new document with the specified path.
 */
public class SaveCommand extends DocumentCommand {
    public static enum SaveMode {
        SAVE_AS, SILENT_SAVE, CLOSE_SAVE;
    }

    public SaveCommand(SaveMode saveMode) {
        this.saveMode = saveMode;
    }

    @Override
    protected void prepare() throws FinalState {
        JavaDocument document = getApplication().getActiveDocument();
        if ((document == null || !document.isModified()) && saveMode != SaveMode.SAVE_AS) // nothing to save
            throw new FinalState(State.Succeeded);
        if (saveMode == SaveMode.SAVE_AS)
            savePath = getApplication().getView().getDialogs().saveDialog();
        else if (saveMode == SaveMode.CLOSE_SAVE) {
            final Dialogs.Result save = getApplication().getView().getDialogs().messageDialog(Dialogs.MessageType.PROMPT_SAVE);
            if (save.isCanceled())
                throw new FinalState(State.Canceled);
            else if (save.isRefused()) // user refused saving
                throw new FinalState(State.Succeeded);

        }
    }

    @Override
    protected void doInner() throws FinalState {
        try {
            if (saveMode == SaveMode.SAVE_AS)
                getApplication().getActiveDocument().save(savePath);
            else
                getApplication().getActiveDocument().save();
        } catch (LibraryException e) {
            if (e.getErrorCode() == Constants.PDF.saveDocumentFailed) {
                final Dialogs.Result save = getApplication().getView().getDialogs().messageDialog(Dialogs.MessageType.SAVE_FAIL);
                if (save.isSucceed())
                    getApplication().executeCommand(new SaveCommand(SaveMode.SAVE_AS));
            }
        }
    }

    private final static CommandType type = CommandType.SAVE; // field used through reflection

    private SaveMode saveMode;
    private String savePath;
}
