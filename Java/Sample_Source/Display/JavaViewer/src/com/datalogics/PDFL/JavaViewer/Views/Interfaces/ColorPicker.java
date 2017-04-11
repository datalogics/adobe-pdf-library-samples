/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

import java.awt.Color;

public class ColorPicker {
    public ColorPicker(Color foreColor, Color backColor) {
        this.foreColor = foreColor;
        this.backColor = backColor;
    }

    public void swap() {
        Color tmp = foreColor;
        foreColor = backColor;
        backColor = tmp;
    }

    public Color foreColor;
    public Color backColor;
}
