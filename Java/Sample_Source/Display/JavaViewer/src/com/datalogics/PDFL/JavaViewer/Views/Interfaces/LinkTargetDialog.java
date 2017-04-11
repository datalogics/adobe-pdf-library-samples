/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

public interface LinkTargetDialog {
    public boolean isCustom();

    public boolean isZoom();

    public boolean isPosition();

    public boolean isPageNumber();

    public void cancel();
}
