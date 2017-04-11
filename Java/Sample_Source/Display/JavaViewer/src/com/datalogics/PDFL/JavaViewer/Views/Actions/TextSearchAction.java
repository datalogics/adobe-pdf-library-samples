/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import javax.swing.JTextField;

import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Document.Command.CommandType;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;

public class TextSearchAction extends SimpleAction {

    public void actionPerformed(ActionEvent e) {
        if (textField != null) {
            final String seacrhPhrase = textField.getText();
            ((ApplicationController) getApplication()).searchText(seacrhPhrase);
        }
    }

    public void setTextField(JTextField textField) {
        this.textField = textField;
    }

    @Override
    public boolean isPermitted() {
        return (textField != null) && JavaDocument.isCommandPermitted(getApplication().getActiveDocument(), CommandType.VIEW);
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return null;
    }

    private JTextField textField;
}
