/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import com.datalogics.PDFL.JavaViewer.Document.Command.DeleteGroupCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * SelectGroupInput - this input is activated when the user selects group
 * annotation.
 * 
 * It also processes mouse and keyboard activity.
 */
public class SelectGroupInput extends BaseAnnotationInput {
    @Override
    public void mousePressed(InputData input) {
        super.mousePressed(input);
        if (getAnnotationEditor().testHit(input.getLocation()).hasHit()) {
            if (input.getRightButton()) {
                showContextMenu(getMenuType(), input.getLocation());
            } else if (input.getLeftButton() && !input.getShiftDown()) {
                getAnnotationEditor().setState(EditorStates.EDIT);
                input.markProcessed();
            }
        }
    }

    @Override
    public void keyPressed(InputData input) {
        super.keyPressed(input);
        if (input.getControlKey(InputData.ControlKey.DELETE)) {
            getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new DeleteGroupCommand());
            input.markProcessed();
        }
    }
}
