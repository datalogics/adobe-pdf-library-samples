/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive;

import com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors.BaseAnnotationEditor;

/**
 * EditorListener - an interface which notifies its implementers when annotation
 * state (Edit, Create, Idle etc.) has been changed.
 */
public interface EditorListener {
    /**
     * send when annotation state was changed
     * 
     * @param originator - sender of event
     * @param oldState - old annotation state
     * @param newState - new annotation state
     */
    public void stateChanged(BaseAnnotationEditor originator, Enum<?> oldState, Enum<?> newState);
}
