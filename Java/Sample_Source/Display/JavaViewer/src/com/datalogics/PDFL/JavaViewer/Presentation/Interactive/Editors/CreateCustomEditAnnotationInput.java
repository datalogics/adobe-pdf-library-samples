/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.EditorStates;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

/**
 * CreateCustomEditAnnotationInput - extends CreateRectangularAnnotationInput
 * and overrides doComplete() method from it.
 * 
 * This class is used for complex annotation type (LinkAnnotation,
 * FreeTextAnnotation).
 */
public class CreateCustomEditAnnotationInput extends CreateRectangularAnnotationInput {
    @Override
    protected void doComplete(InputData input) {
        getAnnotationEditor().setState(EditorStates.CUSTOM_EDIT);
    }
}
