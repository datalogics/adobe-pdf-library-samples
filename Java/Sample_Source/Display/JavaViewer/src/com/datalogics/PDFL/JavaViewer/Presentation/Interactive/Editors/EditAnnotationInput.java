/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.EditAnnotationCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.EditGroupCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * EditAnnotationIput - this class is indented for annotation editing.
 * 
 * It generates an EditCommand when annotation is modified and changes editor state.
 */
public class EditAnnotationInput extends BaseAnnotationInput {
    @Override
    public void mousePressed(InputData input) {
        super.mousePressed(input);
        lastHit = getAnnotationEditor().testHit(input.getLocation());
    }

    @Override
    public void mouseMoved(InputData input) {
        super.mouseMoved(input);

        if (lastHit.hasHit()) {
            getAnnotationEditor().move(lastHit, input.getLocation());
            input.markProcessed();
        }
    }

    @Override
    public void mouseReleased(InputData input) {
        super.mouseReleased(input);
        if (lastHit.hasHit()) {
            getAnnotationEditor().setState(EditorStates.SELECT);
            finishEdit();
            lastHit = new Hit();
            input.markProcessed();
        }
    }

    @Override
    protected void onActivate() {
        super.onActivate();
        editCommand = (getAnnotationEditor() instanceof GroupAnnotationEditor) ? new EditGroupCommand() : new EditAnnotationCommand();
        editCommand = getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(editCommand);
    }

    @Override
    protected void onDeactivate() {
        finishEdit();
        super.onDeactivate();
    }

    private void finishEdit() {
        if (editCommand != null) {
            getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(editCommand);
            editCommand = null;
        }
    }

    private Hit lastHit = new Hit();
    private DocumentCommand editCommand;
}
