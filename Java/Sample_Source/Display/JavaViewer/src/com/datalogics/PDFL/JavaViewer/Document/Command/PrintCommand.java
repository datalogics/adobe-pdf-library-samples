/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.PrintUserParams;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs;

/**
 * PrintCommand - allows to print the document.
 * 
 * The printing ability should be permitted for the document.
 */
public class PrintCommand extends DocumentCommand {

    @Override
    protected void prepare() throws FinalState {
        final JavaDocument dleDocument = getApplication().getActiveDocument();
        document = getApplication().getActiveDocument().getPDF();
        if (document != null) {
            for (OptionalContentGroup ocg : dleDocument.getOCGList())
                document.getOptionalContentContext().setStateOfOCG(ocg, dleDocument.getPrintLayerState(ocg));
            userParams = new PrintUserParams();
        } else {
            getApplication().getView().getDialogs().messageDialog(Dialogs.MessageType.PRINT_FAIL);
            throw new FinalState(State.Failed);
        }
    }

    @Override
    protected void doInner() throws FinalState {
        final JavaDocument dleDocument = getApplication().getActiveDocument();
        if (userParams != null && userParams.posePrintDialog(document))
            document.print(userParams);

        for (OptionalContentGroup ocg : dleDocument.getOCGList())
            document.getOptionalContentContext().setStateOfOCG(ocg, dleDocument.getShowLayerState(ocg));
    }

    private final static CommandType type = CommandType.PRINT; // field used through reflection

    private PrintUserParams userParams;
    private Document document;
}
