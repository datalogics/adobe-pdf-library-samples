/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import com.datalogics.PDFL.JavaViewer.Document.Annotations.LinkAnnotationHolder;
import com.datalogics.PDFL.JavaViewer.Document.Command.DeleteAnnotationCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.GoToCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * SelectAnnotationInput - this input is activated when the user selects an
 * annotation.
 * 
 * This class changes an editor state in Edit or Custom_edit depending on editor
 * type. It also processes key activity and generates delete command when delete
 * key is pressed.
 */
public class SelectAnnotationInput extends BaseAnnotationInput {
    @Override
    public void mousePressed(InputData input) {
        super.mousePressed(input);
        if (getAnnotationEditor().testHit(input.getLocation()).hasHit()) {
            if (input.getRightButton()) {
                showContextMenu(getMenuType(), input.getLocation());
            } else if (input.getCtrlDown() && getAnnotationEditor() instanceof LinkAnnotationEditor) {
                getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new GoToCommand(getAnnotationEditor().getProperties().getAction().createAction((LinkAnnotationHolder) getAnnotationEditor().getHolder())));
            } else if (input.getShiftDown()) {
                // group annotation will be create via EditAnnotationInteractive
            } else if (input.getLeftButton()) {
                getAnnotationEditor().setState(EditorStates.EDIT);
            }
            input.markProcessed();
        }
    }

    @Override
    public void mouseDoubleClicked(InputData input) {
        super.mouseDoubleClicked(input);
        if (getAnnotationEditor() instanceof FreeTextAnnotationEditor || getAnnotationEditor() instanceof LinkAnnotationEditor) {
            getAnnotationEditor().setState(EditorStates.CUSTOM_EDIT);
            input.markProcessed();
        }
    }

    @Override
    public void keyPressed(InputData input) {
        super.keyPressed(input);
        if (input.getControlKey(InputData.ControlKey.DELETE)) {
            getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new DeleteAnnotationCommand());
            input.markProcessed();
        }
    }
}
