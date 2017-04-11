/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;

import javax.swing.AbstractButton;

import com.datalogics.PDFL.JavaViewer.Views.Actions.SimpleAction;

public class SelectionListener implements PropertyChangeListener {

    public SelectionListener(AbstractButton button) {
        this.button = button;
    }

    public void propertyChange(PropertyChangeEvent e) {
        if (e.getPropertyName().equals(SimpleAction.SELECTED)) {
            button.setSelected(Boolean.TRUE.equals(e.getNewValue()));
        }
    }

    private AbstractButton button;
}
