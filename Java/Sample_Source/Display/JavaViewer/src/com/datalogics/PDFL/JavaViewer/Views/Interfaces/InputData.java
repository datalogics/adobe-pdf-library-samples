/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views.Interfaces;

import java.awt.Point;

public interface InputData {
    public static enum ControlKey {
        ESCAPE,
        DELETE,
        LEFT,
        RIGHT,
        UP,
        DOWN,
        PAGEUP,
        PAGEDOWN,
        HOME,
        END;
    }

    public boolean getLeftButton();

    public boolean getRightButton();

    public boolean getMousePressed();

    public boolean getShiftDown();

    public boolean getCtrlDown();

    public boolean getControlKey(ControlKey controlKey);

    public char getKeyChar();

    public Point getLocation();

    public boolean isProcessed();

    public void markProcessed();
}
