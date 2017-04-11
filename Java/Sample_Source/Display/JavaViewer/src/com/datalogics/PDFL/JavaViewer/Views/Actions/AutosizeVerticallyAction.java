/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import com.datalogics.PDFL.JavaViewer.Document.Command.AutosizeVerticallyCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;

public class AutosizeVerticallyAction extends SimpleAction {
    public void actionPerformed(ActionEvent arg0) {
        executeMainCommand(null);
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return AutosizeVerticallyCommand.class;
    }
}
