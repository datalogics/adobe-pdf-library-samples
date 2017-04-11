/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Rectangle;

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationListener;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Document.Command.CustomEditCommand;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;
import com.datalogics.PDFL.JavaViewer.Presentation.PDFPresenter.ViewListener;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.TextEdit;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData.ControlKey;

/**
 * TextEditInput - the input which is
 * 
 * This class uses TextEdit interface for creating and updating RichTextBox.
 */
public class TextEditInput extends BaseAnnotationInput implements ViewListener, AnnotationListener {
    @Override
    public void mousePressed(InputData input) {
        super.mousePressed(input);
        if (textEdit.getViewBounds().contains(input.getLocation())) {
            textEdit.processInput(input);
        } else {
            getAnnotationEditor().setState(EditorStates.SELECT);
        }
    }

    @Override
    public void mouseReleased(InputData input) {
        super.mouseReleased(input);
        textEdit.processInput(input);
    }

    @Override
    public void mouseMoved(InputData input) {
        super.mouseMoved(input);
        textEdit.processInput(input);
    }

    @Override
    public void mouseDoubleClicked(InputData input) {
        super.mouseDoubleClicked(input);
        textEdit.processInput(input);
    }

    @Override
    public void keyPressed(InputData input) {
        super.keyPressed(input);
        if (input.getControlKey(ControlKey.ESCAPE)) {
            getAnnotationEditor().setState(EditorStates.SELECT);
        } else {
            textEdit.processInput(input);
        }
    }

    @Override
    public void keyReleased(InputData input) {
        super.keyReleased(input);
        textEdit.processInput(input);
    }

    @Override
    public void keyTyped(InputData input) {
        super.keyTyped(input);
        textEdit.processInput(input);
    }

    public void viewUpdated(boolean changed) {
        if (changed && textEdit != null) {
            textEdit.setFontSize(getFontSize());
            textEdit.setViewBounds(getAnnotationEditor().getPageModel().transform(getAnnotationEditor().getProperties().getBoundingRect()));
        }
    }

    public void update(Object updateData) {
        if (textEdit != null) {
            textEdit.setFontFace(getAnnotationEditor().getProperties().getFontFace());
            textEdit.setFontSize(getFontSize());
            textEdit.setFontAlign(getAnnotationEditor().getProperties().getQuadding());
            textEdit.setFontColor(getAnnotationEditor().getProperties().getTextColor());
            textEdit.setBgColor(getAnnotationEditor().getProperties().getInteriorColor());
        }
    }

    @Override
    protected void onActivate() {
        super.onActivate();
        startEdit();
    }

    @Override
    protected void onDeactivate() {
        finishEdit();
        super.onDeactivate();
    }

    private void finishEdit() {
        if (textEdit != null) {
            ((ApplicationController) getAnnotationEditor().getInteractiveContext().getApplication()).getPDFPresenter().removeListener(this);
            getAnnotationEditor().getHolder().removeAnnotationListener(this);

            getAnnotationEditor().getProperties().setContents(textEdit.getText());
            textEdit.removeTextEdit();
            textEdit = null;

            getAnnotationEditor().setState(EditorStates.SELECT);
            getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(null);
        }
    }

    private void startEdit() {
        final AnnotationProperties properties = getAnnotationEditor().getProperties();
        final Rectangle boundRect = getAnnotationEditor().getPageModel().transform(properties.getBoundingRect());
        textEdit = getAnnotationEditor().getInteractiveContext().getPdfView().createTextEdit(boundRect);

        textEdit.setBgColor(properties.getInteriorColor());
        textEdit.setFontColor(properties.getTextColor());

        textEdit.setText(properties.getContents());
        textEdit.setFontSize((int) (properties.getFontSize() * getAnnotationEditor().getPageModel().getScale()));
        textEdit.setFontFace(properties.getFontFace());

        getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new CustomEditCommand());
        ((ApplicationController) getAnnotationEditor().getInteractiveContext().getApplication()).getPDFPresenter().addListener(this);
        getAnnotationEditor().getHolder().addAnnotationListener(this);
    }

    private int getFontSize() {
        return (int) (getAnnotationEditor().getProperties().getFontSize() * getAnnotationEditor().getPageModel().getScale());
    }

    private TextEdit textEdit;
}
