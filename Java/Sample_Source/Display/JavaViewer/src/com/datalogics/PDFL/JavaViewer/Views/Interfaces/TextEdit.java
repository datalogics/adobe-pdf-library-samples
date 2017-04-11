/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

import java.awt.Color;
import java.awt.Rectangle;

import com.datalogics.PDFL.HorizontalAlignment;

public interface TextEdit {
    void removeTextEdit();

    void processInput(InputData input);

    Rectangle getViewBounds();

    void setViewBounds(Rectangle bound);

    String getText();

    void setText(String text);

    void setBgColor(Color bgColor);

    void setFontColor(Color color);

    void setFontSize(int fontSize);

    void setFontFace(String fontFace);

    void setFontAlign(HorizontalAlignment align);
}
