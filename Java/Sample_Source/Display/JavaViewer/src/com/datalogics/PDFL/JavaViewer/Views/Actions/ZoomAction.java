/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Actions;

import java.awt.event.ActionEvent;

import javax.swing.JComboBox;

import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.ZoomCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ZoomMode;

public class ZoomAction extends SimpleAction {
    public ZoomAction() {
        this.fromControl = true;
        this.zoomMode = ZoomMode.ZOOM_DIRECT;
    }

    public ZoomAction(double zoomValue) {
        this.fromControl = false;
        this.zoomMode = ZoomMode.ZOOM_DIRECT;
        this.zoomValue = zoomValue;
    }

    public ZoomAction(double[] ZoomArray, boolean next) {
        this.fromControl = false;
        this.zoomMode = next ? ZoomMode.ZOOM_NEXT : ZoomMode.ZOOM_PREV;
        this.zoomArray = ZoomArray;
    }

    public ZoomAction(boolean fitWidth, boolean fitHeight) {
        this.fromControl = false;
        if (fitWidth && !fitHeight)
            this.zoomMode = ZoomMode.ZOOM_FIT_WIDTH;
        else if (!fitWidth && fitHeight)
            this.zoomMode = ZoomMode.ZOOM_FIT_HEIGHT;
        else
            this.zoomMode = ZoomMode.ZOOM_FIT_PAGE;
    }

    @Override
    protected Class<? extends DocumentCommand> getMainCommand() {
        return ZoomCommand.class;
    }

    public void actionPerformed(ActionEvent event) {
        if (fromControl) {
            if (event.getSource() instanceof JComboBox) {
                JComboBox combo = (JComboBox) event.getSource();
                Object item = combo.getSelectedItem();

                if (item instanceof ZoomAction) { // fast-forward to specified action
                    ((ZoomAction) item).actionPerformed(event);
                    return;
                } else if (item instanceof String) {
                    try {
                        String value = (String) item;
                        if (value.lastIndexOf('%') == (value.length() - 1)) // remove last % symbol if available
                            value = value.substring(0, value.length() - 1);

                        zoomValue = Double.parseDouble((String) value) / 100.0;
                    } catch (NumberFormatException e) {
                        zoomValue = 0.0;
                    }
                }
            }
        }

        final InvokeParams[] parameters = new InvokeParams[] { new InvokeParams(ZoomMode.class, zoomMode), new InvokeParams(double[].class, zoomArray), new InvokeParams(double.class, zoomValue) };
        executeMainCommand(parameters);
    }

    @Override
    public String toString() {
        return (zoomMode.isDirectMode()) ? String.valueOf((int) (zoomValue * 100)) + "%" : (String) getValue(NAME);
    }

    private boolean fromControl;
    private ZoomMode zoomMode;
    private double zoomValue;
    private double[] zoomArray = null;
}
