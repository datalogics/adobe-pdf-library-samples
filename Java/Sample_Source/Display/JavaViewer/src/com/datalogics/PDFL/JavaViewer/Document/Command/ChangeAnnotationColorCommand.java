/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationConsts;
import com.datalogics.PDFL.JavaViewer.Document.Annotations.AnnotationProperties;
import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.ColorPicker;

/**
 * ChangeAnnotationColorCommand - allows to change annotation's
 * foreground/background color.
 */
public class ChangeAnnotationColorCommand extends UpdatePropertyCommand {

    @Override
    protected void prepare() throws FinalState {
        final AnnotationProperties props = ((ApplicationController) getApplication()).getActiveProps();
        colors = getApplication().getView().getDialogs().colorPickerDialog(new ColorPicker(props.getSubtype().equals(AnnotationConsts.Subtypes.FREETEXT) ? props.getTextColor() : props.getForeColor(), props.getInteriorColor()));
        super.prepare();
    }

    @Override
    protected void doInner() throws FinalState {
        final AnnotationProperties props = ((ApplicationController) getApplication()).getActiveProps();

        if (props.getSubtype().equals(AnnotationConsts.Subtypes.FREETEXT)) {
            props.setTextColor(colors.foreColor);
        } else {
            props.setForeColor(colors.foreColor);
        }
        props.setInteriorColor(colors.backColor);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;

    private ColorPicker colors;
}
