/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.util.Arrays;
import java.util.List;

import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.JavaViewer.Document.Command.DeleteAnnotationCommand;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * CreateTextMarkupAnnotationInput - allows to create PDFL.TextMarkupAnnotation.
 * 
 * It creates fake quads list because PDFL.TextMarkupAnnotation must be
 * initialized by at least one quad.
 */
public class CreateTextMarkupAnnotationInput extends CreateAnnotationInput {
    @Override
    protected void createNewGuide(InputData input) {
        getAnnotationEditor().getProperties().setQuads(FAKE_LIST);
        setCapturedGuide(GuideRectangle.Layout.custom(BaseAnnotationEditor.END_LINE_GUIDE));
    }

    @Override
    protected void doComplete(InputData input) {
        if (getAnnotationEditor().getProperties().getQuads().equals(FAKE_LIST)) {
            // if no selection changed delete this annotation
            getAnnotationEditor().getInteractiveContext().getApplication().executeCommand(new DeleteAnnotationCommand());
        } else {
            super.doComplete(input);
        }
    }

    private final static List<Quad> FAKE_LIST = Arrays.asList(new Quad[] { new Quad(new Point(0, 0), new Point(1, 1), new Point(0, 0), new Point(1, 1)) });
}
