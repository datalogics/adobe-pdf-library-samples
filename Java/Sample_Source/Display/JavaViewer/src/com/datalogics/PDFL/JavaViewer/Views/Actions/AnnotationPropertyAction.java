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
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationConsts;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Document.Command.CommandType;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.PropertyDialog;

public class AnnotationPropertyAction extends SimpleAction {
    public void actionPerformed(ActionEvent e) {
        AnnotationProperties props = ((ApplicationController) getApplication()).getActiveProps();
        final boolean editable = !props.getSubtype().equals(AnnotationConsts.Subtypes.FREETEXT);
        PropertyDialog properties = getApplication().getView().getDialogs().propertyDialog(props.getTitle(), props.getContents(), editable);
        props.setTitle(properties.title);
        props.setContents(properties.content);
    }

    @Override
    public boolean isPermitted() {
        return JavaDocument.isCommandPermitted(getApplication().getActiveDocument(), CommandType.EDIT);
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return null;
    }
}
