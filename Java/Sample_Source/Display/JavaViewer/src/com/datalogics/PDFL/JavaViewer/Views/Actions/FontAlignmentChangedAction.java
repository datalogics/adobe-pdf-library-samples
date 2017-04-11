/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import com.datalogics.PDFL.HorizontalAlignment;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;
import com.datalogics.PDFL.JavaViewer.Document.Command.ChangeFontAlignmentCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.CommandType;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;

public class FontAlignmentChangedAction extends SimpleAction {
    public void actionPerformed(ActionEvent e) {
        final String align = e.getActionCommand();
        HorizontalAlignment alignment = HorizontalAlignment.LEFT;
        for (HorizontalAlignment horzAlign : HorizontalAlignment.values()) {
            if (horzAlign.name().toLowerCase().equals(align.toLowerCase())) {
                alignment = horzAlign;
                break;
            }
        }
        executeMainCommand(new InvokeParams[] { new InvokeParams(HorizontalAlignment.class, alignment) });
    }

    @Override
    public boolean isPermitted() {
        return JavaDocument.isCommandPermitted(getApplication().getActiveDocument(), CommandType.VIEW);
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return ChangeFontAlignmentCommand.class;
    }
}
