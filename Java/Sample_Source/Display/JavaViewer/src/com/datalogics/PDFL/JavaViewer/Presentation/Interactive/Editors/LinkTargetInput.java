/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import com.datalogics.PDFL.JavaViewer.Document.Command.CustomEditCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.DocumentCommand;
import com.datalogics.PDFL.JavaViewer.Document.Command.GoToCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.Application;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Presentation.Enums.ApplicationMode;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.LinkTargetInteractive;
import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter.ViewShot;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * LinkTargetInput - defines PDFL.LinkAnnotation creation mode.
 * 
 * It saves current view state before creating link annotation and restores it
 * after link annotation creation has been done.
 */
public class LinkTargetInput extends BaseAnnotationInput {
    public enum TargettingState {
        NONE, IN_TARGETTING, DIALOG_FINISHED, DEACTIVATE_MODE;

        public boolean isTargetting() {
            return this.equals(IN_TARGETTING);
        }

        public boolean dialogFinished() {
            return this.equals(DIALOG_FINISHED);
        }

        public boolean modeDeactivate() {
            return this.equals(DEACTIVATE_MODE);
        }
    }

    public LinkTargetInput() {
        resetMembers();
    }

    public void setTargettingState(TargettingState targettingState) {
        if (this.targettingState.equals(targettingState))
            return;
        this.targettingState = targettingState;
    }

    public TargettingState getTargettingState() {
        return targettingState;
    }

    public boolean isTargetting() {
        return getTargettingState().equals(TargettingState.IN_TARGETTING);
    }

    @Override
    public void keyPressed(InputData input) {
    }

    @Override
    public void keyReleased(InputData input) {
    }

    @Override
    public void keyTyped(InputData input) {
    }

    @Override
    public void mouseDoubleClicked(InputData input) {
    }

    @Override
    public void mouseMoved(InputData input) {
    }

    @Override
    public void mousePressed(InputData input) {
    }

    @Override
    public void mouseReleased(InputData input) {
    }

    @Override
    protected void onActivate() {
        super.onActivate();
        startEdit();
    }

    @Override
    protected void onDeactivate() {
        super.onDeactivate();
        finishEdit();
    }

    private void startEdit() {
        if (!getTargettingState().isTargetting()) {
            editCommand = getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new CustomEditCommand());
            // save view parameters
            final Application application = getAnnotationEditor().getInteractiveContext().getApplication();
            viewShot = ((ApplicationController) application).getPDFPresenter().getViewShot();

            mode = ((ApplicationController) application).getMode();
            setTargettingState(TargettingState.IN_TARGETTING);
            ((ApplicationController) application).setCustomMode(new LinkTargetInteractive(getAnnotationEditor(), this));
        }
    }

    private void finishEdit() {
        if (viewShot != null && !getTargettingState().isTargetting()) {

            // firstly restore view parameters
            getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new GoToCommand(viewShot));

            // then complete executing command
            if (getTargettingState().dialogFinished())
                ((ApplicationController) getAnnotationEditor().getInteractiveContext().getApplication()).setMode(mode);

            getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(editCommand);
            resetMembers();
        }
    }

    private void resetMembers() {
        editCommand = null;
        viewShot = null;
        mode = ApplicationMode.NONE_MODE;
        targettingState = TargettingState.NONE;
    }

    private ViewShot viewShot;
    private TargettingState targettingState;

    private DocumentCommand editCommand;
    private ApplicationMode mode;
}
