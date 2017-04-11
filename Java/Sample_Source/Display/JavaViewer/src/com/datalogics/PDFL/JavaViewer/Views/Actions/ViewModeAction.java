/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.ViewModeCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.PageViewMode;

public class ViewModeAction extends SimpleAction {
    public ViewModeAction(PageViewMode viewMode) {
        this.viewMode = viewMode;
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return ViewModeCommand.class;
    }

    public void actionPerformed(ActionEvent e) {
        final InvokeParams[] parameters = new InvokeParams[] { new InvokeParams(PageViewMode.class, viewMode) };
        executeMainCommand(parameters);
    }

    private PageViewMode viewMode;
}
