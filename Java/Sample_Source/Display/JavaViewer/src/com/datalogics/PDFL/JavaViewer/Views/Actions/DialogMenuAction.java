/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import javax.swing.AbstractAction;
import javax.swing.Action;

import com.datalogics.PDFL.JavaViewer.Presentation.Enums.DialogMenuItem;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.DialogMenuResultNotifier;

public class DialogMenuAction extends AbstractAction {
    public DialogMenuAction(DialogMenuItem item, final DialogMenuResultNotifier subscriber, String name) {
        this.putValue(Action.NAME, name);

        this.subscriber = subscriber;
        this.item = item;
    }

    public void actionPerformed(ActionEvent e) {
        subscriber.dialogMenuResult(item);
    }

    private DialogMenuResultNotifier subscriber;
    private DialogMenuItem item;
}
