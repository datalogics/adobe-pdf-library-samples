/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Document.Command.ChangeLineWidthCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.CommandType;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;

public class ChangeLineWidthAction extends SimpleAction {
    public ChangeLineWidthAction(float lineWidth) {
        this.lineWidth = lineWidth;
    }

    public void actionPerformed(ActionEvent e) {
        final float width = lineWidth == -1 ? getApplication().getView().getDialogs().customLineWidth(0, 100, (int) ((ApplicationController) getApplication()).getActiveProps().getLineWidth()) : lineWidth;
        executeMainCommand(new InvokeParams[] { new InvokeParams(float.class, width) });
    }

    @Override
    public boolean isPermitted() {
        return JavaDocument.isCommandPermitted(getApplication().getActiveDocument(), CommandType.EDIT);
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return ChangeLineWidthCommand.class;
    }

    private float lineWidth;
}
