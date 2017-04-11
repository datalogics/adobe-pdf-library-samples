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
import com.datalogics.PDFL.JavaViewer.Document.Command.ChangeDashPatternCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.CommandType;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;

public class ChangeDashPatternAction extends SimpleAction {
    public ChangeDashPatternAction() {
    }

    public ChangeDashPatternAction(float[] pattern) {
        this.pattern = pattern;
    }

    public void actionPerformed(ActionEvent event) {
        final InvokeParams[] parameters = new InvokeParams[] { new InvokeParams(float[].class, pattern) };
        executeMainCommand(parameters);
    }

    @Override
    public boolean isPermitted() {
        return JavaDocument.isCommandPermitted(getApplication().getActiveDocument(), CommandType.EDIT);
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return ChangeDashPatternCommand.class;
    }

    private float[] pattern;
}
