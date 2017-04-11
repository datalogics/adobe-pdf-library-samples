/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Point;
import java.util.EnumSet;

import com.datalogics.PDFL.JavaViewer.Document.Command.DeleteAnnotationCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.DialogMenuItem;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.DialogMenuResultNotifier;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData.ControlKey;

/**
 * CreateAnnotationInput - the class is used to create annotation.
 * 
 * It has methods for checking whether annotation can be created and when
 * annotation creation can be finished. Conditions of creation and finalization
 * of annotation depend on annotation type. This class also generates a command
 * when annotation is created or deleted.
 */
public abstract class CreateAnnotationInput extends BaseAnnotationInput implements DialogMenuResultNotifier {
    @Override
    public final void mousePressed(InputData input) {
        super.mousePressed(input);
        if (canCreateGuide(input)) {
            createNewGuide(input);
        }
        input.markProcessed(); // consume all mouse input
    }

    @Override
    public final void mouseMoved(InputData input) {
        super.mouseMoved(input);
        if (canMoveGuide(input)) {
            moveGuide(input);
        }
        input.markProcessed(); // consume all mouse input
    }

    @Override
    public final void mouseReleased(InputData input) {
        super.mouseReleased(input);
        if (canComplete(input)) {
            doComplete(input);
        }
        input.markProcessed(); // consume all mouse input
    }

    @Override
    public final void mouseDoubleClicked(InputData input) {
        input.markProcessed(); // consume all mouse input
    }

    @Override
    public void keyPressed(InputData input) {
        super.keyPressed(input);
        if (input.getControlKey(ControlKey.ESCAPE) || input.getControlKey(ControlKey.DELETE)) {
            cancel();
            input.markProcessed();
        }
    }

    public void showContextMenu(Point location) {
        getAnnotationEditor().getInteractiveContext().getApplication().getView().showDialogMenu(EnumSet.of(DialogMenuItem.COMPLETE, DialogMenuItem.CANCEL), this, location);
    }

    public void dialogMenuResult(DialogMenuItem item) {
        if (item.equals(DialogMenuItem.COMPLETE)) {
            complete();
        } else if (item.equals(DialogMenuItem.CANCEL)) {
            cancel();
        }
    }

    protected abstract void createNewGuide(InputData input);

    protected boolean canMoveGuide(InputData input) {
        return input.getLeftButton();
    }

    protected boolean canCreateGuide(InputData input) {
        return input.getLeftButton() && getAnnotationEditor().getPageModel().getBounds().contains(input.getLocation());
    }

    protected void moveGuide(InputData input) {
        if (capturedGuide != null)
            getAnnotationEditor().move(capturedGuide, input.getLocation());
    }

    protected void setCapturedGuide(int layout) {
        capturedGuide = getAnnotationEditor().getGuideHit(layout);
    }

    protected boolean canComplete(InputData input) {
        return true;
    }

    protected void doComplete(InputData input) {
        complete();
    }

    protected void complete() {
        getAnnotationEditor().setState(EditorStates.SELECT);
        getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(null);
    }

    protected void cancel() {
        getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new DeleteAnnotationCommand());
    }

    private Hit capturedGuide;
}
