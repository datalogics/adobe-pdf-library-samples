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
import com.datalogics.PDFL.JavaViewer.Document.Command.CommandType;
import com.datalogics.PDFL.JavaViewer.Document.Command.CreateAnnotationCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;

public class CreateAnnotationAction extends SimpleAction {

    public CreateAnnotationAction(String annotationSubType) {
        this.annotationSubType = annotationSubType;
    }

    @Override
    public boolean isPermitted() {
        return JavaDocument.isCommandPermitted(getApplication().getActiveDocument(), CommandType.VIEW);
    }

    public void actionPerformed(ActionEvent e) {
        executeMainCommand(new InvokeParams[] { new InvokeParams(String.class, this.annotationSubType) });
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return CreateAnnotationCommand.class;
    }

    public String getSubType() {
        return annotationSubType;
    }

    private String annotationSubType;
}
