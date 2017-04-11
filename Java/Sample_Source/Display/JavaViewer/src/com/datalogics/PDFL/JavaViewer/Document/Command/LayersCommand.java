/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import java.util.List;

import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.JavaViewer.Document.JavaDocument;

/**
 * LayersCommand - responsible for show/hide page layers.
 * 
 * This command does not support undo/redo functionality.
 */
public class LayersCommand extends DocumentCommand {
    public LayersCommand(boolean state, boolean isDisplayableLayers, List<OptionalContentGroup> ocgList) {
        this.state = state;
        this.isDisplayableLayers = isDisplayableLayers;
        this.ocgList = ocgList;
    }

    @Override
    protected void prepare() throws FinalState {
    }

    @Override
    protected void doInner() throws FinalState {
        final JavaDocument document = getApplication().getActiveDocument();
        if (document == null || ocgList == null || ocgList.isEmpty())
            throw new FinalState(State.Failed);

        document.changeLayersState(state, isDisplayableLayers, ocgList);
    }

    private final static CommandType type = CommandType.VIEW; // field used through reflection

    private boolean state;
    private boolean isDisplayableLayers;
    private List<OptionalContentGroup> ocgList;
}
