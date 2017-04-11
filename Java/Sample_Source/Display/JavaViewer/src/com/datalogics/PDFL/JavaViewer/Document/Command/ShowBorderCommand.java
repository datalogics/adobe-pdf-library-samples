/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Document.Command;

import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;

/**
 * ShowBorderCommand - allows to show/hide the border rectangle for the
 * PDFL.LinkAnnotation.
 */
public class ShowBorderCommand extends UpdatePropertyCommand {
    @Override
    protected void doInner() throws FinalState {
        final boolean borderState = ((ApplicationController) getApplication()).getActiveProps().getShowBorder();
        ((ApplicationController) getApplication()).getActiveProps().setShowBorder(!borderState);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;
}
