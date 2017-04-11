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
 * AutosizeVerticallyCommand - extends the UpdatePropertyCommand class.
 * 
 * This class allows to set auto size for FreeTextAnnotation.
 */
public class AutosizeVerticallyCommand extends UpdatePropertyCommand {
    @Override
    protected void doInner() throws FinalState {
        ((ApplicationController) getApplication()).getActiveProps().setAutoSize(true);
        super.doInner();
    }

    private final static CommandType type = CommandType.EDIT;
}
