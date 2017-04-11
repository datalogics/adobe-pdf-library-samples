/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import com.datalogics.PDFL.JavaViewer.Document.Command.DeleteAnnotationCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;

public class DeleteAnnotationAction extends SimpleAction {
    public void actionPerformed(ActionEvent e) {
        executeMainCommand(null);
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return DeleteAnnotationCommand.class;
    }
}
