/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Presentation.Interactive.Editors;

import java.awt.Graphics;

/**
 * LinkTargetOutput - draws Link annotation shape when it has been selected.
 */
public class LinkTargetOutput extends BaseAnnotationOutput {
    @Override
    public void drawShape(Graphics g) {
        super.drawShape(g);
        getAnnotationEditor().drawShape(g);
    }

    @Override
    public void drawSelection(Graphics g) {
        // no border for link annotation targeting mode
    }
}
