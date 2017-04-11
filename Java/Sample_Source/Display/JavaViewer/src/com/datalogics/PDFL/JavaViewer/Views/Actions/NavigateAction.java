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

import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.NavigateCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.NavigateMode;

public class NavigateAction extends SimpleAction {
    public NavigateAction() {
        this.fromControl = true;
        this.navigateMode = NavigateMode.NAVIGATE_DIRECT;
    }

    public NavigateAction(NavigateMode navigateMode) {
        this.fromControl = false;
        this.navigateMode = navigateMode;
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return NavigateCommand.class;
    }

    public void actionPerformed(ActionEvent event) {
        if (fromControl) {
            try {
                if (event.getSource() instanceof JTextField)
                    pageNum = Integer.parseInt(((JTextField) event.getSource()).getText());
                pageNum -= 1; // shift one page as it starts from 0 in the
                              // document
            } catch (NumberFormatException e) {
                pageNum = -1;
            }
        }

        final InvokeParams[] parameters = new InvokeParams[] { new InvokeParams(NavigateMode.class, navigateMode), new InvokeParams(int.class, pageNum) };
        executeMainCommand(parameters);
    }

    private boolean fromControl;
    private NavigateMode navigateMode;
    private int pageNum = -1;
}
