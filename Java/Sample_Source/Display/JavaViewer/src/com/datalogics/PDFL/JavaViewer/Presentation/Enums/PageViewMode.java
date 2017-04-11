/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Enums;

/**
 * PageViewMode - enum which determines PDF document's paging mode.
 * 
 * @author 1
 * 
 */
public enum PageViewMode {
    PAGE_MODE_NONE,
    PAGE_MODE_SINGLE,
    PAGE_MODE_CONTINUOUS;

    public boolean isSinglePage() {
        return this.equals(PAGE_MODE_SINGLE);
    }

    public boolean isContinuousPage() {
        return this.equals(PAGE_MODE_CONTINUOUS);
    }
}
