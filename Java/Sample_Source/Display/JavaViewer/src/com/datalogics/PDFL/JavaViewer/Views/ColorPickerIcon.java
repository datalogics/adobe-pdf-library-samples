/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer.Views;

import java.awt.Color;
import java.awt.Component;
import java.awt.Graphics;
import java.awt.Rectangle;

import javax.swing.Icon;

import com.datalogics.PDFL.JavaViewer.Views.Interfaces.ColorPicker;

public class ColorPickerIcon implements Icon {
    public ColorPickerIcon(int height, int width, ColorPicker colors) {
        this.foreColor = colors.foreColor;
        this.backColor = colors.backColor;
        this.borderColor = Color.BLACK;
        this.buttonWidth = width;
        this.buttonHeight = height;
    }

    public ColorPickerIcon(int height, int width) {
        this(height, width, new ColorPicker(new Color(161, 161, 161), new Color(161, 161, 161)));
        this.borderColor = new Color(128, 128, 128);
    }

    public int getIconHeight() {
        return buttonHeight;
    }

    public int getIconWidth() {
        return buttonWidth;
    }

    public Rectangle getForeRect() {
        return new Rectangle(getIconWidth() / 3, getIconHeight() / 3, getIconWidth() / 2, getIconHeight() / 2);
    }

    public Rectangle getBackRect() {
        return new Rectangle(getIconWidth() / 6, getIconHeight() / 6, getIconWidth() / 2, getIconHeight() / 2);
    }

    public void paintIcon(Component c, Graphics g, int x, int y) {
        final Rectangle backRect = getBackRect();
        final Rectangle foreRect = getForeRect();
        final Color fColor = foreColor;
        final Color bColor = backColor;

        g.setColor(bColor == null ? Color.WHITE : bColor);
        g.fillRect(backRect.x, backRect.y, backRect.width, backRect.height);
        if (bColor == null) {
            g.setColor(Color.RED);
            g.drawLine(getBackRect().width + getBackRect().x, getBackRect().y, getBackRect().x, getBackRect().height + getBackRect().y);
        }
        g.setColor(borderColor);
        g.drawRect(backRect.x, backRect.y, backRect.width, backRect.height);

        g.setColor(fColor);
        g.fillRect(foreRect.x, foreRect.y, foreRect.width, foreRect.height);
        g.setColor(borderColor);
        g.drawRect(foreRect.x, foreRect.y, foreRect.width, foreRect.height);

    }

    private Color foreColor;
    private Color backColor;
    private Color borderColor;

    private int buttonWidth;
    private int buttonHeight;
}
