/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

/*
 * InputDataView - it is an implementation of InputData interface. It maps
 * InputData key constants to the KeyEvent constants; This class keeps all
 * information about mouse and key events.
 */

import java.awt.Point;
import java.awt.event.KeyEvent;
import java.awt.event.MouseEvent;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.InputData;

public class InputDataView implements InputData {
    public InputDataView(MouseEvent mouseEvent, Point originLocation, boolean mousePressed) {
        this.mouseEvent = mouseEvent;
        this.mousePressed = mousePressed;

        // e.getModifiers() and e.getModifiersEx() represent exclusive modifier
        // flags, so we need both to have a full set
        // to figure out how it can be refer to the InputEvent source codes
        this.modifiers = mouseEvent.getModifiers() | mouseEvent.getModifiersEx();

        // make correct point with regard to view origin offset
        Point location = mouseEvent.getPoint();
        location.translate(originLocation.x, originLocation.y);

        this.location = location;
    }

    public InputDataView(KeyEvent keyEvent) {
        this.keyEvent = keyEvent;
        this.modifiers = keyEvent.getModifiersEx();
        this.location = new Point();
    }

    public boolean getLeftButton() {
        return (modifiers & (mousePressed ? MouseEvent.BUTTON1_DOWN_MASK : MouseEvent.BUTTON1_MASK)) != 0;
    }

    public boolean getRightButton() {
        return (modifiers & (mousePressed ? MouseEvent.BUTTON3_DOWN_MASK : MouseEvent.BUTTON3_MASK)) != 0;
    }

    public boolean getMousePressed() {
        return mousePressed;
    }

    public boolean getShiftDown() {
        return (modifiers & KeyEvent.SHIFT_DOWN_MASK) != 0;
    }

    public boolean getCtrlDown() {
        return (modifiers & KeyEvent.CTRL_DOWN_MASK) != 0;
    }

    public boolean getControlKey(ControlKey controlKey) {
        return keys.containsKey(controlKey) && keyEvent != null ? keys.get(controlKey).equals(keyEvent.getKeyCode()) : false;
    }

    public char getKeyChar() {
        return keyEvent != null ? keyEvent.getKeyChar() : null;
    }

    public Point getLocation() {
        return new Point(location);
    }

    public boolean isProcessed() {
        return this.processed;
    }

    public void markProcessed() {
        this.processed = true;
    }

    private boolean mousePressed;

    private int modifiers;
    private Point location;

    private boolean processed;

    private static final Map<ControlKey, Integer> keys = Collections.unmodifiableMap(new HashMap<ControlKey, Integer>() {
        {
            put(ControlKey.ESCAPE, KeyEvent.VK_ESCAPE);
            put(ControlKey.DELETE, KeyEvent.VK_DELETE);
            put(ControlKey.UP, KeyEvent.VK_UP);
            put(ControlKey.DOWN, KeyEvent.VK_DOWN);
            put(ControlKey.LEFT, KeyEvent.VK_LEFT);
            put(ControlKey.RIGHT, KeyEvent.VK_RIGHT);
            put(ControlKey.PAGEUP, KeyEvent.VK_PAGE_UP);
            put(ControlKey.PAGEDOWN, KeyEvent.VK_PAGE_DOWN);
            put(ControlKey.HOME, KeyEvent.VK_HOME);
            put(ControlKey.END, KeyEvent.VK_END);
        }
    });

    private MouseEvent mouseEvent;
    private KeyEvent keyEvent;
}
